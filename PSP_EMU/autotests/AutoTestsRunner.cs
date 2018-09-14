using System;
using System.Text;
using System.Threading;

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
namespace pspsharp.autotests
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.VideoEngine.readLittleEndianInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.VideoEngine.readLittleEndianShort;


	using FileUtil = pspsharp.util.FileUtil;
	using LWJGLFixer = pspsharp.util.LWJGLFixer;
	using ConsoleAppender = org.apache.log4j.ConsoleAppender;
	using Level = org.apache.log4j.Level;
	using Logger = org.apache.log4j.Logger;
	using DOMConfigurator = org.apache.log4j.xml.DOMConfigurator;

	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using IMainGUI = pspsharp.GUI.IMainGUI;
	using HLEModuleManager = pspsharp.HLE.HLEModuleManager;
	using Modules = pspsharp.HLE.Modules;
	using EmulatorVirtualFileSystem = pspsharp.HLE.VFS.emulator.EmulatorVirtualFileSystem;
	using UmdIsoReader = pspsharp.filesystems.umdiso.UmdIsoReader;
	using Screen = pspsharp.hardware.Screen;
	using LoggingOutputStream = pspsharp.log.LoggingOutputStream;

	public class AutoTestsRunner
	{
		private static readonly Logger log = Logger.getLogger("pspautotests");
		private const int FAIL_TIMEOUT = 10; // in seconds

		static AutoTestsRunner()
		{
			LWJGLFixer.fixOnce();
			log.addAppender(new ConsoleAppender());
		}

		public static void Main(string[] args)
		{
			(new AutoTestsRunner()).run();
		}

		internal Emulator emulator;

		private static void debug(string str)
		{
			//log.info(str);
			Console.Error.WriteLine(str);
		}

		private static void info(string str)
		{
			//log.info(str);
			Console.WriteLine(str);
		}

		private static void error(string str)
		{
			//log.error(str);
			Console.Error.WriteLine(str);
		}

		internal class DummyGUI : IMainGUI
		{
			private readonly AutoTestsRunner outerInstance;

			public DummyGUI(AutoTestsRunner outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual string MainTitle
			{
				set
				{
				}
			}
			public virtual void RefreshButtons()
			{
			}
			public virtual void setLocation()
			{
			}
			public virtual DisplayMode DisplayMode
			{
				get
				{
					return new DisplayMode(480, 272, 32, 60);
				}
			}
			public virtual void endWindowDialog()
			{
			}
			public virtual bool FullScreen
			{
				get
				{
					return false;
				}
			}
			public virtual bool Visible
			{
				get
				{
					return false;
				}
			}
			public virtual void pack()
			{
			}
			public virtual void setFullScreenDisplaySize()
			{
			}
			public virtual void startWindowDialog(Window window)
			{
			}
			public virtual void startBackgroundWindowDialog(Window window)
			{
			}
			public virtual Rectangle CaptureRectangle
			{
				get
				{
					return null;
				}
			}
			public virtual void onUmdChange()
			{
			}
			public virtual void onMemoryStickChange()
			{
			}
			public virtual void setDisplayMinimumSize(int width, int height)
			{
			}
			public virtual void setDisplaySize(int width, int height)
			{
			}
			public virtual void run()
			{
			}
			public virtual void pause()
			{
			}
			public virtual void reset()
			{
			}
			public virtual bool RunningFromVsh
			{
				get
				{
					return false;
				}
			}
			public virtual bool RunningReboot
			{
				get
				{
					return false;
				}
			}
		}

		public AutoTestsRunner()
		{
			emulator = new Emulator(new DummyGUI(this));
			emulator.setFirmwareVersion(630);
		}

		public virtual void run()
		{
			DOMConfigurator.configure("LogSettings.xml");
			System.Out = new PrintStream(new LoggingOutputStream(Logger.getLogger("emu"), Level.INFO));
			Screen.HasScreen = false;
			//IoFileMgrForUser.defaultTimings.get(IoFileMgrForUser.IoOperation.iodevctl).setDelayMillis(0);
			Modules.sceDisplayModule.setCalledFromCommandLine();

			try
			{
				runImpl();
			}
			catch (Exception o)
			{
				Console.WriteLine(o.ToString());
				Console.Write(o.StackTrace);
			}

			Environment.Exit(0);
		}

		private static File rootDirectory = FileUtil.findFolderNameInAncestors(new File("."), "pspautotests");

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void runImpl() throws Throwable
		protected internal virtual void runImpl()
		{
			File folder = rootDirectory;
			if (folder != null)
			{
				runTestFolder(new File(folder, "/tests"));
			}
			else
			{
				error("Can't find pspautotests folder");
			}
	//		runTest(rootDirectory + "/tests/cpu/vfpu/vector");
	//		runTestFolder(rootDirectory + "/tests/cpu");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void runTestFolder(java.io.File folder) throws Throwable
		protected internal virtual void runTestFolder(File folder)
		{
			File[] files = folder.listFiles();
			if (files != null)
			{
				foreach (File file in files)
				{
					if (file.Name.charAt(0) == '.')
					{
						continue;
					}
					if (file.Directory)
					{
						runTestFolder(file);
					}
					else if (file.File)
					{
						string name = file.Path;
						if (name.Substring(name.Length - 9).Equals(".expected"))
						{
							runTest(name.Substring(0, name.Length - 9));
						}
					}
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void runTest(String baseFileName) throws Throwable
		protected internal virtual void runTest(string baseFileName)
		{
			if (System.IO.Directory.Exists(EmulatorVirtualFileSystem.ScreenshotFileName)) System.IO.Directory.Delete(EmulatorVirtualFileSystem.ScreenshotFileName, true); else System.IO.File.Delete(EmulatorVirtualFileSystem.ScreenshotFileName);

			bool timeout = false;
			try
			{
				runFile(baseFileName + ".prx");
			}
			catch (TimeoutException)
			{
				timeout = true;
			}
			checkOutput(baseFileName, baseFileName + ".expected", timeout);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected java.awt.image.BufferedImage readBmp(java.io.File imageFile) throws java.io.IOException
		protected internal virtual BufferedImage readBmp(File imageFile)
		{
			System.IO.Stream @is = new BufferedInputStream(new System.IO.FileStream(imageFile, System.IO.FileMode.Open, System.IO.FileAccess.Read));

			// Reading file header
			int magic = readLittleEndianShort(@is);
			int fileSize = readLittleEndianInt(@is);
			@is.skip(4);
			int dataOffset = readLittleEndianInt(@is);

			// Reading DIB header
			int dibHeaderLength = readLittleEndianInt(@is);
			int imageWidth = readLittleEndianInt(@is);
			int imageHeight = readLittleEndianInt(@is);
			int numberPlanes = readLittleEndianShort(@is);
			int bitsPerPixel = readLittleEndianShort(@is);

			// Skip rest of DIB header until data start
			@is.skip(dataOffset - 14 - 16);

			BufferedImage img = null;
			if (magic == (('M' << 8) | 'B') && dibHeaderLength >= 16 && fileSize >= dataOffset && numberPlanes == 1 && bitsPerPixel == 32)
			{
				img = new BufferedImage(imageWidth, imageHeight, BufferedImage.TYPE_INT_ARGB);
				for (int y = imageHeight - 1; y >= 0; y--)
				{
					for (int x = 0; x < imageWidth; x++)
					{
						int argb = readLittleEndianInt(@is);
						img.setRGB(x, y, argb);
					}
				}
			}

			@is.Close();

			return img;
		}

		protected internal virtual bool areColorsEqual(int color1, int color2)
		{
			return (color1 & 0x00FFFFFF) == (color2 & 0x00FFFFFF);
		}

		protected internal virtual bool compareScreenshots(File expected, File result, File compare)
		{
			bool equals = false;

			try
			{
				BufferedImage expectedImg = ImageIO.read(expected);

				BufferedImage resultImg;
				try
				{
					resultImg = ImageIO.read(result);
				}
				catch (Exception)
				{
					// java.lang.RuntimeException: New BMP version not implemented yet.
					resultImg = readBmp(result);
				}

				int width = System.Math.Min(expectedImg.Width, resultImg.Width);
				int height = System.Math.Min(expectedImg.Height, resultImg.Height);
				BufferedImage compareImg = new BufferedImage(width, height, BufferedImage.TYPE_INT_RGB);
				equals = true;
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						int expectedColor = expectedImg.getRGB(x, y);
						int resultColor = resultImg.getRGB(x, y);
						if (areColorsEqual(expectedColor, resultColor))
						{
							compareImg.setRGB(x, y, 0x000000);
						}
						else
						{
							compareImg.setRGB(x, y, 0xFF0000);
							equals = false;
						}
					}
				}
				ImageIO.write(compareImg, "bmp", compare);
			}
			catch (IOException e)
			{
				error(string.Format("comparing screenshots {0}", e));
			}

			return equals;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void checkOutput(String baseFileName, String fileName, boolean timeout) throws java.io.IOException
		protected internal virtual void checkOutput(string baseFileName, string fileName, bool timeout)
		{
			string actualOutput = AutoTestsOutput.Output.Trim();
			string expectedOutput = readFileAsString(fileName).Trim();
			if (actualOutput.Equals(expectedOutput))
			{
				info(string.Format("{0}: OK", baseFileName));
			}
			else
			{
				if (timeout)
				{
					error(string.Format("{0}: FAIL, TIMEOUT", baseFileName));
				}
				else
				{
					error(string.Format("{0}: FAIL", baseFileName));
				}
				diff(expectedOutput, actualOutput);
			}

			File screenshotExpected = new File(fileName + ".bmp");
			if (screenshotExpected.canRead())
			{
				File screenshotResult = new File(EmulatorVirtualFileSystem.ScreenshotFileName);
				if (screenshotResult.canRead())
				{
					File savedScreenshotResult = new File(baseFileName + ".result.bmp");
					savedScreenshotResult.delete();
					if (screenshotResult.renameTo(savedScreenshotResult))
					{
						info(string.Format("{0}: saved screenshot under '{1}'", baseFileName, savedScreenshotResult));

						File compareScreenshot = new File(baseFileName + ".compare.bmp");
						if (compareScreenshots(screenshotExpected, savedScreenshotResult, compareScreenshot))
						{
							info(string.Format("{0}: screenshots are identical", baseFileName));
						}
						else
						{
							error(string.Format("{0}: screenshots differ, see '{1}'", baseFileName, compareScreenshot));
						}
					}
					else
					{
						error(string.Format("{0}: cannot save screenshot from '{1}' to '{2}'", baseFileName, screenshotResult, savedScreenshotResult));
					}
				}
				else
				{
					error(string.Format("{0}: FAIL, no result screenshot found", baseFileName));
				}
			}
		}

		public static void diff(string x, string y)
		{
			diff(x.Split("\\n", true), y.Split("\\n", true));
		}

		public static void diff(string[] x, string[] y)
		{
			// number of lines of each file
			int M = x.Length;
			int N = y.Length;

			// opt[i][j] = length of LCS of x[i..M] and y[j..N]
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] opt = new int[M+1][N+1];
			int[][] opt = RectangularArrays.ReturnRectangularIntArray(M + 1, N + 1);

			// compute length of LCS and all subproblems via dynamic programming
			for (int i = M - 1; i >= 0; i--)
			{
				for (int j = N - 1; j >= 0; j--)
				{
					if (x[i].Equals(y[j]))
					{
						opt[i][j] = opt[i + 1][j + 1] + 1;
					}
					else
					{
						opt[i][j] = System.Math.Max(opt[i + 1][j], opt[i][j + 1]);
					}
				}
			}

			// recover LCS itself and print out non-matching lines to standard output
			int i = 0, j = 0;
			while (i < M && j < N)
			{
				if (x[i].Equals(y[j]))
				{
					debug("  " + x[i]);
					i++;
					j++;
				}
				else if (opt[i + 1][j] >= opt[i][j + 1])
				{
					info("- " + x[i++]);
				}
				else
				{
					info("+ " + y[j++]);
				}
			}

			// dump out one remainder of one string if the other is exhausted
			while (i < M || j < N)
			{
				if (i == M)
				{
					info("+ " + y[j++]);
				}
				else if (j == N)
				{
					info("- " + x[i++]);
				}
			}
		}

		protected internal virtual void reset()
		{
			AutoTestsOutput.clearOutput();

			Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_PAUSE);
			Emulator.Instance.initNewPsp(false);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void runFile(String fileName) throws Throwable
		protected internal virtual void runFile(string fileName)
		{
			File file = new File(fileName);

			reset();

			try
			{
				RandomAccessFile raf = new RandomAccessFile(file, "r");
				try
				{
					FileChannel roChannel = raf.Channel;
					try
					{
						ByteBuffer readbuffer = roChannel.map(FileChannel.MapMode.READ_ONLY, 0, (int) roChannel.size());
						{
							//module =
							emulator.load(file.Path, readbuffer);
						}
					}
					finally
					{
						roChannel.close();
					}
				}
				finally
				{
					raf.close();
				}
			}
			catch (FileNotFoundException)
			{
			}

			RuntimeContext.IsHomebrew = true;

			UmdIsoReader umdIsoReader = new UmdIsoReader(rootDirectory + "/input/cube.cso");
			Modules.IoFileMgrForUserModule.IsoReader = umdIsoReader;
			Modules.sceUmdUserModule.IsoReader = umdIsoReader;
			Modules.IoFileMgrForUserModule.setfilepath(file.Parent);

			debug(string.Format("Running: {0}...", fileName));
			{
				RuntimeContext.IsHomebrew = false;

				HLEModuleManager.Instance.startModules(false);
				Modules.sceDisplayModule.UseSoftwareRenderer = true;
				{
					emulator.RunEmu();

					long startTime = DateTimeHelper.CurrentUnixTimeMillis();
					while (!Emulator.pause)
					{
						Modules.sceDisplayModule.step();
						if (DateTimeHelper.CurrentUnixTimeMillis() - startTime > FAIL_TIMEOUT * 1000)
						{
							throw (new TimeoutException());
						}
						Thread.Sleep(1);
					}
				}
				HLEModuleManager.Instance.stopModules();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static String readFileAsString(String filePath) throws java.io.IOException
		private static string readFileAsString(string filePath)
		{
			StringBuilder s = new StringBuilder();
			System.IO.StreamReader f = null;
			try
			{
				f = new System.IO.StreamReader(filePath);
				// Read line by line to exclude all carriage returns ('\r')
				while (true)
				{
					string line = f.ReadLine();
					if (string.ReferenceEquals(line, null))
					{
						break;
					}
					s.Append(line);
					s.Append('\n');
				}
			}
			finally
			{
				if (f != null)
				{
					try
					{
						f.Close();
					}
					catch (IOException)
					{
					}
				}
			}

			return s.ToString();
		}
	}

}