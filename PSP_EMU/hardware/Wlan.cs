using System;

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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNet.convertStringToMacAddress;

	//using Logger = org.apache.log4j.Logger;

	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using sceUtility = pspsharp.HLE.modules.sceUtility;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using Settings = pspsharp.settings.Settings;

	public class Wlan
	{
		private static Logger log = Emulator.log;
		public static int PSP_WLAN_SWITCH_OFF = 0;
		public static int PSP_WLAN_SWITCH_ON = 1;
		private static int switchState = PSP_WLAN_SWITCH_ON;
		public const int MAC_ADDRESS_LENGTH = 6;
		private static sbyte[] macAddress = new sbyte[] {0x01, 0x02, 0x03, 0x04, 0x05, 0x06};
		private const string settingsMacAddress = "macAddress";
		public static int PSP_ADHOC_CHANNEL_AUTO = 0;
		public static int PSP_ADHOC_CHANNEL_DEFAULT = 11;
		private static int signalStrength = 100;
		private static WlanSwitchSettingsListener wlanSwitchSettingsListener;

		private class WlanSwitchSettingsListener : AbstractBoolSettingsListener
		{
			protected internal override void settingsValueChanged(bool value)
			{
				SwitchState = value ? PSP_WLAN_SWITCH_ON : PSP_WLAN_SWITCH_OFF;
			}
		}

		public static void initialize()
		{
			string macAddressString = Settings.Instance.readString(settingsMacAddress);
			if (string.ReferenceEquals(macAddressString, null) || macAddressString.Length <= 0)
			{
				// MAC Address not set from the settings file, generate a random one
				macAddress = pspNetMacAddress.RandomMacAddress;
				// Do not save the new MAC address to the settings so that different instances
				// of pspsharp are using different MAC addresses (required for Adhoc networking).
				//Settings.getInstance().writeString(settingsMacAddress, convertMacAddressToString(macAddress));
			}
			else
			{
				macAddress = convertStringToMacAddress(macAddressString);
				// Both least significant bits of the first byte have a special meaning
				// (see http://en.wikipedia.org/wiki/Mac_address):
				// bit 0: 0=Unicast / 1=Multicast
				// bit 1: 0=Globally unique / 1=Locally administered
				macAddress[0] &= unchecked((sbyte)0xFC);
			}

			if (wlanSwitchSettingsListener == null)
			{
				wlanSwitchSettingsListener = new WlanSwitchSettingsListener();
				Settings.Instance.registerSettingsListener("WlanSwitch", "network.wlanSwitchOn", wlanSwitchSettingsListener);
			}
		}

		public static int SwitchState
		{
			get
			{
				return switchState;
			}
			set
			{
				Wlan.switchState = value;
				log.info(string.Format("WLAN Switch {0}", value == PSP_WLAN_SWITCH_OFF ? "off" : "on"));
			}
		}


		public static sbyte[] MacAddress
		{
			get
			{
				return macAddress;
			}
			set
			{
				Array.Copy(value, 0, macAddress, 0, MAC_ADDRESS_LENGTH);
			}
		}


		public static int AdhocChannel
		{
			get
			{
				int channel = sceUtility.SystemParamAdhocChannel;
				if (channel == PSP_ADHOC_CHANNEL_AUTO)
				{
					channel = PSP_ADHOC_CHANNEL_DEFAULT;
				}
    
				return channel;
			}
		}

		public static int SignalStrenth
		{
			get
			{
				return signalStrength;
			}
		}
	}

}