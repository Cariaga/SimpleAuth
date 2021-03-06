﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;
#if __ANDROID__
using Android.Runtime;
#elif __IOS__
using Foundation;
#endif
namespace SimpleAuth.Providers
{
	public class GoogleApi : OAuthApi
	{

		/// <summary>
		/// Optional value that is used to get a server token when used with Native iOS. 
		/// The server token will be added to account.UserData["ServerToken"]
		/// </summary>
		/// <value>The server client identifier.</value>
		public string ServerClientId { get; set; }
		public static bool ForceNativeLogin { get; set; } =
#if __UNIFIED__ || __ANDROID__
			true;
#else
			false;
#endif
		public static string NativeClientSecret = "native";
		/// <summary>
		/// Only use this Constructor for platforms using NativeAuth
		/// </summary>
		/// <param name="identifier">Identifier.</param>
		/// <param name="clientId">Client identifier.</param>
		/// <param name="handler">Handler.</param>
		public GoogleApi (string identifier, string clientId, HttpMessageHandler handler = null) : this (identifier, clientId, NativeClientSecret, handler)
		{

		}

		public GoogleApi (string identifier, string clientId, string clientSecret, HttpMessageHandler handler = null) : base (identifier, CleanseClientId (clientId), clientSecret, handler)
		{
			this.TokenUrl = "https://accounts.google.com/o/oauth2/token";
#if __UNIFIED__
			if (ForceNativeLogin) {
				IsUsingSFSafari = true;
				this.CurrentShowAuthenticator = NativeSafariAuthenticator.ShowAuthenticator;
				NativeSafariAuthenticator.RegisterCallbacks ();
			}

#endif
			if (GoogleShowAuthenticator != null)
				CurrentShowAuthenticator = GoogleShowAuthenticator;
			CheckNative ();
		}

#if __ANDROID__
		const string NativeRequiredException = "Google no longer supports Web View authetnication. Add the Clancey.SimpleAuth.Google.Droid nuget, and follow the instructions found here: https://github.com/Clancey/SimpleAuth/blob/master/README.md";
#elif __UNIFIED__
		static bool IsUsingSFSafari;
		const string NativeRequiredException = "Google no longer supports Web View authetnication. Use the SFSafariViewController and follow the instructions found here: https://github.com/Clancey/SimpleAuth/blob/master/README.md";
#else
		const string NativeRequiredException = "Google no longer supports Web View authetnication. Follow the instructions found here: https://github.com/Clancey/SimpleAuth/blob/master/README.md";
#endif
		void CheckNative ()
		{
			bool isUsingNative = IsUsingNative;
#if __UNIFIED__
			isUsingNative = IsUsingSFSafari || NativeSafariAuthenticator.IsActivated;
#endif
			if (ForceNativeLogin && !isUsingNative) {
				throw new Exception (NativeRequiredException);
			}
		}

		public static Action<WebAuthenticator> GoogleShowAuthenticator { get; set; }
		public static string CleanseClientId (string clientId) => clientId?.Replace (".apps.googleusercontent.com", "");

		public static bool IsUsingNative { get; set; }
		public Uri RedirectUrl { get; set; } = new Uri ("http://localhost");

		protected override WebAuthenticator CreateAuthenticator ()
		{
			return new GoogleAuthenticator {
				Scope = Scopes.ToList (),
				ClientId = ClientId,
				ClientSecret = ClientSecret,
				ClearCookiesBeforeLogin = CalledReset,
				RedirectUrl = RedirectUrl,
				IsUsingNative = IsUsingNative,
				ServerClientId = ServerClientId,
			};
		}
		public static Action<string, string> OnLogOut { get; set; }
		protected override Task<OAuthAccount> GetAccountFromAuthCode (WebAuthenticator authenticator, string identifier)
		{
			var auth = authenticator as GoogleAuthenticator;
			//Native lib returns the auth token already
			if (IsUsingNative && auth.ClientSecret == NativeClientSecret) {
				return Task.FromResult (new OAuthAccount () {
					ExpiresIn = 3600,
					Created = DateTime.UtcNow,
					Scope = authenticator.Scope?.ToArray (),
					TokenType = "Bearer",
					Token = auth.AuthCode,
					//Android wont send a refresh
					RefreshToken = auth.RefreshToken ?? auth.AuthCode,
					ClientId = ClientId,
					Identifier = identifier,
					IdToken = auth.IdToken,
					UserData = {
						{"ServerToken", auth.ServerToken},
					}
				});
			}

			return base.GetAccountFromAuthCode (authenticator, identifier);
		}

		public async Task<GoogleUserProfile> GetUserInfo (bool forceRefresh = false)
		{
			string userInfoJson;
			if (forceRefresh || !CurrentAccount.UserData.TryGetValue ("userInfo", out userInfoJson)) {
				CurrentAccount.UserData ["userInfo"] =
					userInfoJson = await Get ("https://www.googleapis.com/oauth2/v1/userinfo?alt=json");
				SaveAccount (CurrentAccount);
			}

			return Deserialize<GoogleUserProfile> (userInfoJson);

		}
		public override void ResetData ()
		{
			OnLogOut?.Invoke (ClientId, ClientSecret);
			base.ResetData ();
		}
	}

	public class GoogleAuthenticator : OAuthAuthenticator
	{
		/// <summary>
		/// Optional, used to get the ServerTokens. The server token will be added to account.UserData["ServerToken"]
		/// </summary>
		/// <value>The server client identifier.</value>
		public string ServerClientId { get; set; }
		public override string BaseUrl {
			get;
			set;
		} = "https://accounts.google.com/o/oauth2/auth";

		public override Uri RedirectUrl {
			get;
			set;
		}

		public override async Task<Dictionary<string, string>> GetTokenPostData (string clientSecret)
		{
			var data = await base.GetTokenPostData (clientSecret);
			data ["scope"] = string.Join (" ", Scope);
			data ["client_id"] = GetGoogleClientId (ClientId);
			if (data ["client_secret"] == "native")
				data.Remove ("client_secret");
			data ["redirect_uri"] = GetRedirectUrl ();
			return data;
		}
		public override Dictionary<string, string> GetInitialUrlQueryParameters ()
		{
			var data = base.GetInitialUrlQueryParameters ();
			data ["redirect_uri"] = GetRedirectUrl ();
			data ["client_id"] = GetGoogleClientId (ClientId);
			return data;
		}

		public static string GetGoogleClientId (string clientId) => string.IsNullOrWhiteSpace (clientId) ? null : $"{GoogleApi.CleanseClientId (clientId)}.apps.googleusercontent.com";

		public bool IsUsingNative { get; set; }
		public virtual string GetRedirectUrl ()
		{
#if __UNIFIED__
			//for google, the redirect is a reverse of the client ID
			return $"com.googleusercontent.apps.{ClientId}:/oauthredirect";
#elif WINDOWS_UWP
            return "urn:ietf:wg:oauth:2.0:oob";
#else
			if (IsUsingNative)
				return "";
#endif
			return RedirectUrl.AbsoluteUri;
		}
		public string IdToken { get; set; }
		public string ServerToken { get; set; }
		public string RefreshToken { get; set; }
		public override bool CheckUrl (Uri url, Cookie [] cookies)
		{
			try {
				if (url == null || string.IsNullOrWhiteSpace (url.Query))
					return false;
				var parts = HttpUtility.ParseQueryString (url.Query);
				var code = parts ["code"];
				if (!string.IsNullOrWhiteSpace (code)) {
					Cookies = cookies?.Select (x => new CookieHolder { Domain = x.Domain, Path = x.Path, Name = x.Name, Value = x.Value }).ToArray ();
					FoundAuthCode (code);
					return true;
				}

			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
			return false;
		}


		public static string UrlFromClientId (string clientId)
		{
			var parts = clientId.Split ('.');
			return string.Join (".", parts.Reverse ());
		}


		public override async Task<Uri> GetInitialUrl ()
		{
			var uri = await base.GetInitialUrl ();
			return new Uri (uri.AbsoluteUri + "&access_type=offline");
		}

		public void OnRecievedAuthCode (string authCode)
		{
			FoundAuthCode (authCode);
		}
	}

#if __MOBILE__
	[Preserve (AllMembers = true)]
#endif
	public class GoogleUserProfile
	{
		[JsonProperty ("id")]
		public string Id {
			get;
			set;
		}

		[JsonProperty ("email")]
		public string Email {
			get;
			set;
		}

		[JsonProperty ("verified_email")]
		public bool VerifiedEmail {
			get;
			set;
		}

		[JsonProperty ("name")]
		public string Name {
			get;
			set;
		}

		[JsonProperty ("given_name")]
		public string GivenName {
			get;
			set;
		}

		[JsonProperty ("family_name")]
		public string FamilyName {
			get;
			set;
		}

		[JsonProperty ("link")]
		public string Link {
			get;
			set;
		}

		[JsonProperty ("picture")]
		public string Picture {
			get;
			set;
		}

		[JsonProperty ("gender")]
		public string Gender {
			get;
			set;
		}

		[JsonProperty ("locale")]
		public string Locale {
			get;
			set;
		}

		[JsonProperty ("hd")]
		public string Hd {
			get;
			set;
		}
	}

}
