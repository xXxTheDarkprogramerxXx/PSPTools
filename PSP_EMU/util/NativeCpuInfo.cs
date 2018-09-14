using System.Runtime.InteropServices;

/*
 This file is part of pspsharp.

 pspsharp is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.

 pspsharp is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with pspsharp.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace pspsharp.util
{
	using Logger = org.apache.log4j.Logger;

	/// 
	/// <summary>
	/// @author shadow
	/// </summary>
	public class NativeCpuInfo
	{
		private static Logger log = Logger.getLogger("cpuinfo");
		private static bool isAvailable = false;

		static NativeCpuInfo()
		{
			try
			{
//JAVA TO C# CONVERTER TODO TASK: The library is specified in the 'DllImport' attribute for .NET:
//				System.loadLibrary("cpuinfo");
				isAvailable = true;
			}
			catch (UnsatisfiedLinkError ule)
			{
				log.error("Loading cpuinfo native library", ule);
			}
		}

		public static bool Available
		{
			get
			{
				return isAvailable;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void init();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern boolean hasSSE();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern boolean hasSSE2();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern boolean hasSSE3();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern boolean hasSSSE3();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern boolean hasSSE41();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern boolean hasSSE42();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern boolean hasAVX();

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern boolean hasAVX2();

		public static void printInfo()
		{
			log.info("Supports SSE    " + hasSSE());
			log.info("Supports SSE2   " + hasSSE2());
			log.info("Supports SSE3   " + hasSSE3());
			log.info("Supports SSSE3  " + hasSSSE3());
			log.info("Supports SSE4.1 " + hasSSE41());
			log.info("Supports SSE4.2 " + hasSSE42());
			log.info("Supports AVX    " + hasAVX());
			log.info("Supports AVX2   " + hasAVX2());
		}
	}

}