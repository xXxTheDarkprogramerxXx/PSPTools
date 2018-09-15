using System;
using System.Collections.Generic;
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
	using Native = com.sun.jna.Native;
	using Structure = com.sun.jna.Structure;
	using StdCallLibrary = com.sun.jna.win32.StdCallLibrary;
	using OS = pspsharp.util.OS;


	/// <summary>
	/// @author gid15
	/// </summary>
	public class BatteryUpdateThread : Thread
	{
		private static BatteryUpdateThread instance = null;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private long sleepMillis_Renamed;

		public static void initialize()
		{
			if (instance == null)
			{
				long secondsForOnePercentDrain = Battery.LifeTime * 60 / 100;
				instance = new BatteryUpdateThread(secondsForOnePercentDrain * 1000);
				instance.Daemon = true;
				instance.Name = "Battery Drain";
				instance.Start();
			}
		}

		public BatteryUpdateThread(long sleepMillis)
		{
			this.sleepMillis_Renamed = sleepMillis;
		}

		public override void run()
		{
			if (OS.isWindows)
			{
				updateWindows();
			}
			else if (OS.isLinux)
			{
				updateLinux();
			}
			else if (OS.isMac)
			{
				updateMac();
			}
			else
			{
				updateGeneric();
			}
		}

		private void updateWindows()
		{
			while (true)
			{
				BatteryWindows.Kernel32_SYSTEM_POWER_STATUS status = BatteryWindows.status();

				// getLifeTime
				int batteryLifeTimeInSeconds = status.BatteryLifeTime;
				if (batteryLifeTimeInSeconds < 0)
				{
					batteryLifeTimeInSeconds = 5 * 3600; // Unknown lifetime
				}
				Battery.LifeTime = batteryLifeTimeInSeconds / 60;

				// isPluggedIn
				Battery.PluggedIn = status.ACLineStatus == 1;

				// isPResent
				Battery.Present = (status.BatteryFlag & 128) != 0;

				// currentPowerPercent
				int percent = status.BatteryLifePercent;
				if (percent >= 0 && percent <= 100)
				{
					Battery.CurrentPowerPercent = percent;
				}
				else
				{
					// Invalid value, not update it!
				}

				// isCharging
				Battery.Charging = (status.BatteryFlag & 8) != 0;

				sleepMillis(5 * 1000); // Wait five second between updates
			}
		}

		private static void sleepMillis(long millis)
		{
			try
			{
				Thread.Sleep(millis);
			}
			catch (InterruptedException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		private void updateLinux()
		{
			updateGeneric();
		}

		private void updateMac()
		{
			updateGeneric();
		}

		private void updateGeneric()
		{
			while (true)
			{
				sleepMillis(sleepMillis_Renamed);

				int powerPercent = Battery.CurrentPowerPercent;

				// Increase/decrease power by 1%
				if (Battery.Charging)
				{
					if (powerPercent < 100)
					{
						powerPercent++;
					}
				}
				else
				{
					if (powerPercent > 0)
					{
						powerPercent--;
					}
				}

				Battery.CurrentPowerPercent = powerPercent;
			}
		}

		internal class BatteryWindows
		{
			internal static Kernel32_SYSTEM_POWER_STATUS result = new Kernel32_SYSTEM_POWER_STATUS(this);

			public static Kernel32_SYSTEM_POWER_STATUS status()
			{
				Kernel32_Fields.INSTANCE.GetSystemPowerStatus(result);
				return result;
			}

			// http://stackoverflow.com/questions/3434719/how-to-get-the-remaining-battery-life-in-a-windows-system
			// http://msdn2.microsoft.com/en-us/library/aa373232.aspx
			public interface Kernel32 : StdCallLibrary
			{

				int GetSystemPowerStatus(Kernel32_SYSTEM_POWER_STATUS result);
			}

			public static class Kernel32_Fields
			{
				private readonly BatteryUpdateThread.BatteryWindows outerInstance;

				public Kernel32_Fields(BatteryUpdateThread.BatteryWindows outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public static readonly Kernel32 INSTANCE = (Kernel32) Native.loadLibrary("Kernel32", typeof(Kernel32));
			}

			public class Kernel32_SYSTEM_POWER_STATUS : Structure
			{
				private readonly BatteryUpdateThread.BatteryWindows outerInstance;

				public Kernel32_SYSTEM_POWER_STATUS(BatteryUpdateThread.BatteryWindows outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public sbyte ACLineStatus; // 0 = Offline, 1 = Online, other = Unknown
				public sbyte BatteryFlag; // 1 = High (more than 66%), 2 = Low (less than 33%), 4 = Critical (less than 5%), 8 = Charging, 128 = No system battery
				public sbyte BatteryLifePercent; // 0-100 (-1 on desktop)
				public sbyte Reserved1;
				public int BatteryLifeTime; // Estimated Lifetime in seconds
				public int BatteryFullLifeTime; // Estimated Lifetime in seconds on full charge

				protected internal override IList<string> FieldOrder
				{
					get
					{
						return Array.asList("ACLineStatus", "BatteryFlag", "BatteryLifePercent", "Reserved1", "BatteryLifeTime", "BatteryFullLifeTime");
					}
				}
			}
		}

		// http://stackoverflow.com/questions/22128336/how-to-add-a-command-to-check-battery-level-in-linux-shell
		internal class BatteryLinux
		{
			internal static BatteryLinux INSTANCE = new BatteryLinux();
			internal static string FOLDER = "/sys/class/power_supply/BAT0";

			public static BatteryLinux status()
			{
				return INSTANCE;
			}

			public static void refresh()
			{
			}
		}
	}

}