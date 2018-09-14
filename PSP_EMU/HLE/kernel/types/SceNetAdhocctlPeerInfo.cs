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
	using sceNetAdhocctl = pspsharp.HLE.modules.sceNetAdhocctl;

	/// <summary>
	/// Peer info structure </summary>
	public class SceNetAdhocctlPeerInfo : pspAbstractMemoryMappedStructure
	{
		public int nextAddr;
		/// <summary>
		/// Nickname </summary>
		public string nickName;
		/// <summary>
		/// Mac address </summary>
		public pspNetMacAddress macAddress;
		/// <summary>
		/// Time stamp </summary>
		public long timestamp;

		protected internal override void read()
		{
			nextAddr = read32();
			nickName = readStringNZ(sceNetAdhocctl.NICK_NAME_LENGTH);
			macAddress = new pspNetMacAddress();
			read(macAddress);
			readUnknown(6);
			timestamp = read64();
		}

		protected internal override void write()
		{
			write32(nextAddr);
			writeStringNZ(sceNetAdhocctl.NICK_NAME_LENGTH, nickName);
			write(macAddress);
			writeUnknown(6);
			write64(timestamp);
		}

		public override int @sizeof()
		{
			return 152;
		}

		public override string ToString()
		{
			return string.Format("nickName='{0}', macAddress={1}, timestamp={2:D}", nickName, macAddress, timestamp);
		}
	}

}