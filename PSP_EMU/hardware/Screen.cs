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
namespace pspsharp.hardware
{

	public class Screen
	{
		private static DisableScreenSaverThread disableScreenSaverThread;
		public const int width = 480;
		public const int height = 272;
		private static int brightnessLevel = 100;
		private static long lastPowerTick;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private static bool hasScreen_Renamed = true;

		public static void hleKernelPowerTick()
		{
			lastPowerTick = ClockMillis;
		}

		public static void start()
		{
			lastPowerTick = ClockMillis;

			if (disableScreenSaverThread == null)
			{
				disableScreenSaverThread = new DisableScreenSaverThread();
				disableScreenSaverThread.Name = "Disable Screen Saver";
				disableScreenSaverThread.Daemon = true;
				disableScreenSaverThread.Start();
			}
		}

		private static long ClockMillis
		{
			get
			{
				return Emulator.Clock.milliTime();
			}
		}

		public static void exit()
		{
			if (disableScreenSaverThread != null)
			{
				disableScreenSaverThread.exit();
			}
		}

		private class DisableScreenSaverThread : Thread
		{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal volatile bool exit_Renamed;
			internal const int tickMillis = 60 * 1000; // One minute

			public virtual void exit()
			{
				exit_Renamed = true;
			}

			public override void run()
			{
				try
				{
					Robot robot = new Robot();
					while (!exit_Renamed)
					{
						try
						{
							Thread.Sleep(tickMillis);
						}
						catch (InterruptedException)
						{
							// Ignore exception
						}

						long now = ClockMillis;
						if (now - lastPowerTick < tickMillis)
						{
							if (Emulator.log.TraceEnabled)
							{
								Emulator.log.trace(string.Format("Moving the mouse to disable the screen saver (PowerTick since {0:D} ms)", now - lastPowerTick));
							}
							robot.waitForIdle();

							// Move the mouse to its current location to disable the screensaver
							PointerInfo mouseInfo = MouseInfo.PointerInfo;
							robot.mouseMove(mouseInfo.Location.x, mouseInfo.Location.y);
						}
						else
						{
							if (Emulator.log.TraceEnabled)
							{
								Emulator.log.trace(string.Format("PowerTick not called since {0:D} ms", now - lastPowerTick));
							}
						}
					}
				}
				catch (AWTException e)
				{
					Emulator.log.error(e);
				}
			}
		}

		public static bool hasScreen()
		{
			return hasScreen_Renamed;
		}

		public static bool HasScreen
		{
			set
			{
				Screen.hasScreen_Renamed = value;
			}
		}

		public static int BrightnessLevel
		{
			get
			{
				return brightnessLevel;
			}
			set
			{
				Screen.brightnessLevel = value;
			}
		}

	}

}