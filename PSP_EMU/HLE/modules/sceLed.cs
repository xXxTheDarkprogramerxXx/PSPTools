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
namespace pspsharp.HLE.modules
{
	//using Logger = org.apache.log4j.Logger;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;

	public class sceLed : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceLed");
		public const int PSP_LED_TYPE_MS = 0; //* Memory-Stick LED.
		public const int PSP_LED_TYPE_WLAN = 1; //* W-LAN LED.
		public const int PSP_LED_TYPE_BT = 2; //* Bluetooth LED.
		public const int SCE_LED_MODE_OFF = 0; //* Turn a LED OFF.
		public const int SCE_LED_MODE_ON = 1; //* Turn a LED ON.
		public const int SCE_LED_MODE_BLINK = 2; //* Set a blink event for a LED.
		public const int SCE_LED_MODE_SELECTIVE_EXEC = 3; //* Register LED configuration commands and execute them.

		/// <summary>
		/// Set a LED mode.
		/// </summary>
		/// <param name="led"> The LED to set a mode for. One of ::ScePspLedTypes. </param>
		/// <param name="mode"> The mode to set for a LED. One of ::SceLedModes. </param>
		/// <param name="config"> Configuration settings for a LED. Is only used for the ::SceLedModes
		///               SCE_LED_MODE_BLINK and SCE_LED_MODE_SELECTIVE_EXEC.
		/// </param>
		/// <returns> SCE_ERROR_OK on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEA24BE03, version = 150) public int sceLedSetMode(int led, int mode, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=20, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer config)
		[HLEFunction(nid : 0xEA24BE03, version : 150)]
		public virtual int sceLedSetMode(int led, int mode, TPointer config)
		{
			return 0;
		}
	}

}