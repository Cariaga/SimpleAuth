﻿//
//  Copyright 2016  Clancey
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Diagnostics;

namespace SimpleAuth
{
	internal static class Console
	{
		public static void WriteLine()
		{
			Debug.WriteLine("");
		}

		public static void WriteLine(bool value)
		{
			Debug.WriteLine(value);
		}

		public static void WriteLine(char value)
		{
			Debug.WriteLine(value);
		}

		public static void WriteLine(char[] buffer)
		{
			Debug.WriteLine(buffer);
		}

		public static void WriteLine(decimal value)
		{
			Debug.WriteLine(value);
		}

		public static void WriteLine(double value)
		{
			Debug.WriteLine(value);
		}

		public static void WriteLine(int value)
		{
			Debug.WriteLine(value);
		}

		public static void WriteLine(long value)
		{
			Debug.WriteLine(value);
		}

		public static void WriteLine(object value)
		{
			Debug.WriteLine(value);
		}

		public static void WriteLine(float value)
		{
			Debug.WriteLine(value);
		}

		public static void WriteLine(string value)
		{
			Debug.WriteLine(value);
		}

		[CLSCompliant(false)]
		public static void WriteLine(uint value)
		{
			Debug.WriteLine(value);
		}

		[CLSCompliant(false)]
		public static void WriteLine(ulong value)
		{
			Debug.WriteLine(value);
		}

		public static void WriteLine(string format, object arg0)
		{
			Debug.WriteLine(format, arg0);
		}

		public static void WriteLine(string format, params object[] arg)
		{
			if (arg == null)
				Debug.WriteLine(format);
			else
				Debug.WriteLine(format, arg);
		}


		public static void WriteLine(string format, object arg0, object arg1)
		{
			Debug.WriteLine(format, arg0, arg1);
		}

		public static void WriteLine(string format, object arg0, object arg1, object arg2)
		{
			Debug.WriteLine(format, arg0, arg1, arg2);
		}


	}
}

