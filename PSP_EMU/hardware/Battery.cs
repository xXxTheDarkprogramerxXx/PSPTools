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
	public class Battery
	{
		//battery life time in minutes
		private static int lifeTime = (5 * 60); // 5 hours
		//some standard battery temperature 28 deg C
		private static int temperature = 28;
		//battery voltage 4,135 in slim
		private static int voltage = 4135;

		private static bool pluggedIn = true;
		private static bool present = true;
		private static int currentPowerPercent = 100;
		// led starts flashing at 12%
		private const int lowPercent = 12;
		// PSP auto suspends at 4%
		private const int forceSuspendPercent = 4;
		// battery capacity in mAh when it is full
		private const int fullCapacity = 1800;
		private static bool charging = false;

		public static void initialize()
		{
			BatteryUpdateThread.initialize();
		}

		public static int LifeTime
		{
			get
			{
				return lifeTime;
			}
			set
			{
				Battery.lifeTime = value;
			}
		}


		public static int Temperature
		{
			get
			{
				return temperature;
			}
			set
			{
				Battery.temperature = value;
			}
		}


		public static int Voltage
		{
			get
			{
				return voltage;
			}
			set
			{
				Battery.voltage = value;
			}
		}


		public static bool PluggedIn
		{
			get
			{
				return pluggedIn;
			}
			set
			{
				Battery.pluggedIn = value;
			}
		}


		public static bool Present
		{
			get
			{
				return present;
			}
			set
			{
				Battery.present = value;
			}
		}


		public static int CurrentPowerPercent
		{
			get
			{
				return currentPowerPercent;
			}
			set
			{
				Battery.currentPowerPercent = value;
			}
		}


		public static bool Charging
		{
			get
			{
				return charging;
			}
			set
			{
				Battery.charging = value;
			}
		}


		public static int LowPercent
		{
			get
			{
				return lowPercent;
			}
		}

		public static int ForceSuspendPercent
		{
			get
			{
				return forceSuspendPercent;
			}
		}

		public static int FullCapacity
		{
			get
			{
				return fullCapacity;
			}
		}
	}

}