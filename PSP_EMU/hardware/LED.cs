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
	public class LED
	{
		private static bool ledMemoryStickOn;
		private static bool ledWlanOn;
		private static bool ledPowerOn;
		private static bool ledBluetoothOn;

		public static bool LedMemoryStickOn
		{
			get
			{
				return ledMemoryStickOn;
			}
			set
			{
				LED.ledMemoryStickOn = value;
			}
		}


		public static bool LedWlanOn
		{
			get
			{
				return ledWlanOn;
			}
			set
			{
				LED.ledWlanOn = value;
			}
		}


		public static bool LedPowerOn
		{
			get
			{
				return ledPowerOn;
			}
			set
			{
				LED.ledPowerOn = value;
			}
		}


		public static bool LedBluetoothOn
		{
			get
			{
				return ledBluetoothOn;
			}
			set
			{
				LED.ledBluetoothOn = value;
			}
		}

	}

}