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
	public class SceNetWlanScanInfo : pspAbstractMemoryMappedStructure
	{
		public string bssid;
		public int channel;
		public string ssid;
		public int mode;
		public int unknown44;

		protected internal override void read()
		{
			bssid = readStringNZ(6); // Offset 0
			channel = read8(); // Offset 6
			int ssidLength = read8(); // Offset 7
			ssid = readStringNZ(ssidLength); // Offset 8
			readUnknown(32 - ssidLength);
			mode = read32(); // Offset 40
			unknown44 = read32(); // Offset 44
		}

		protected internal override void write()
		{
			writeStringN(6, bssid); // Offset 0
			write8((sbyte) channel); // Offset 6
			if (string.ReferenceEquals(ssid, null))
			{
				write8((sbyte) 0); // Offset 7
			}
			else
			{
				write8((sbyte) ssid.Length); // Offset 7
			}
			writeStringN(32, ssid); // Offset 8
			write32(mode); // Offset 40
			write32(unknown44); // Offset 44
			writeUnknown(44); // Offset 48
		}

		public override int @sizeof()
		{
			return 92;
		}

		public override string ToString()
		{
			return string.Format("bssid='{0}', channel={1:D}, ssid='{2}', mode=0x{3:X}, unknown44=0x{4:X}", bssid, channel, ssid, mode, unknown44);
		}
	}

}