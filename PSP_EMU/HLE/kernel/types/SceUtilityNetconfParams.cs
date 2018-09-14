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
namespace pspsharp.HLE.kernel.types
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocctl.GROUP_NAME_LENGTH;

	public class SceUtilityNetconfParams : pspUtilityBaseDialog
	{
		public int netAction; // The netconf action (PSPSDK): sets how to connect.
			public const int PSP_UTILITY_NETCONF_CONNECT_APNET = 0;
			public const int PSP_UTILITY_NETCONF_GET_STATUS_APNET = 1;
			public const int PSP_UTILITY_NETCONF_CONNECT_ADHOC = 2;
			public const int PSP_UTILITY_NETCONF_CONNECT_APNET_LASTUSED = 3;
			public const int PSP_UTILITY_NETCONF_CREATE_ADHOC = 4;
			public const int PSP_UTILITY_NETCONF_JOIN_ADHOC = 5;
		public int netconfDataAddr;
		public SceUtilityNetconfData netconfData;
		public int netHotspot; // Flag to allow hotspot connections (PSPSDK).
		public int netHotspotConnected; // Flag to check if a hotspot connection is active (PSPSDK).
		public int netWifiSp; // Flag to allow WIFI connections (PSPSDK).

		public class SceUtilityNetconfData : pspAbstractMemoryMappedStructure
		{
			public string groupName;
			public int timeout;

			protected internal override void read()
			{
				groupName = readStringNZ(GROUP_NAME_LENGTH);
				timeout = read32();
			}

			protected internal override void write()
			{
				writeStringNZ(GROUP_NAME_LENGTH, groupName);
				write32(timeout);
			}

			public override int @sizeof()
			{
				return GROUP_NAME_LENGTH + 4;
			}

			public override string ToString()
			{
				return string.Format("groupName={0}, timeout={1:D}", groupName, timeout);
			}
		}

		protected internal override void read()
		{
			@base = new pspUtilityDialogCommon();
			read(@base);
			MaxSize = @base.totalSizeof();

			netAction = read32();
			netconfDataAddr = read32();
			if (netconfDataAddr != 0)
			{
				netconfData = new SceUtilityNetconfData();
				netconfData.read(mem, netconfDataAddr);
			}
			else
			{
				netconfData = null;
			}
			netHotspot = read32();
			netHotspotConnected = read32();
			netWifiSp = read32();
		}

		protected internal override void write()
		{
			write(@base);
			MaxSize = @base.totalSizeof();

			write32(netAction);
			write32(netconfDataAddr);
			if (netconfData != null && netconfDataAddr != 0)
			{
				netconfData.write(mem, netconfDataAddr);
			}
			write32(netHotspot);
			write32(netHotspotConnected);
			write32(netWifiSp);
		}

		public override int @sizeof()
		{
			return @base.totalSizeof();
		}

		public override string ToString()
		{
			return string.Format("SceUtilityNetconf[address=0x{0:X8}, netAction={1:D}, {2}]", BaseAddress, netAction, netconfData);
		}
	}
}