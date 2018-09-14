using System;

namespace pspsharp.util
{

	// References:
	// https://github.com/libgdx/libgdx/blob/master/gdx/src/com/badlogic/gdx/utils/SharedLibraryLoader.java
	// https://github.com/LWJGL/lwjgl3/blob/master/modules/core/src/main/java/org/lwjgl/system/SharedLibraryLoader.java
	// This is not required on lwjgl3 since SharedLibraryLoader takes care of it and allows fatjars and launch4j executables
	public class LWJGLFixer
	{
		private static bool @fixed = false;

		public static void fixOnce()
		{
			if (!@fixed)
			{
				@fixed = true;
				fix();
			}
		}

		private static void fix()
		{
			try
			{
				fixInternal();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		private static string[] LibrariesToLoad
		{
			get
			{
				if (OS.isWindows)
				{
					return new string[]{"lwjgl.dll", "lwjgl64.dll", "OpenAL32.dll", "OpenAL64.dll", "jinput-dx8.dll", "jinput-dx8_64.dll", "jinput-raw.dll", "jinput-raw_64.dll", "jinput-wintab.dll"};
				}
				else if (OS.isLinux)
				{
					return new string[]{"liblwjgl.so", "liblwjgl64.so", "libopenal.so", "libopenal64.so", "libjinput-linux.so", "libjinput-linux64.so"};
				}
				else if (OS.isMac)
				{
					return new string[]{"liblwjgl.dylib", "openal.dylib", "libjinput-osx.jnilib"};
				}
				else
				{
					return new string[0];
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void fixInternal() throws Throwable
		public static void fixInternal()
		{
			ClassLoader systemClassLoader = ClassLoader.SystemClassLoader;
			string[] libraries = LibrariesToLoad;

			try
			{
				File folder = new File(System.getProperty("java.io.tmpdir") + "/lwgl-2.9.3/" + System.getProperty("user.name"));
				folder.mkdirs();

				foreach (string library in libraries)
				{
					URL libUrl = systemClassLoader.getResource(library);
					string basename = FileUtil.getURLBaseName(libUrl);
					File outFile = new File(folder, basename);

					if (!outFile.exists())
					{
						FileUtil.writeBytes(outFile, FileUtil.readURL(libUrl));
					}
				}

				System.setProperty("org.lwjgl.librarypath", folder.AbsolutePath);
				System.setProperty("net.java.games.input.librarypath", folder.AbsolutePath);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}
	}

}