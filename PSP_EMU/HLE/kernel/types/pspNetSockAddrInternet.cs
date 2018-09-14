using System.Text;

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

	using sceNetInet = pspsharp.HLE.modules.sceNetInet;

	public class pspNetSockAddrInternet : pspAbstractMemoryMappedStructure
	{
		public int sin_len;
		public int sin_family;
		public int sin_port;
		public int sin_addr;
		public int sin_zero1;
		public int sin_zero2;

		protected internal override void read()
		{
			// start address is not 32-bit aligned
			sin_len = read8();
			sin_family = read8();
			sin_port = endianSwap16((short) readUnaligned16());
			sin_addr = readUnaligned32();
			sin_zero1 = readUnaligned32();
			sin_zero2 = readUnaligned32();
		}

		protected internal override void write()
		{
			// start address is not 32-bit aligned
			write8((sbyte) sin_len);
			write8((sbyte) sin_family);
			writeUnaligned16((short) endianSwap16((short) sin_port));
			writeUnaligned32(sin_addr);
			writeUnaligned32(sin_zero1);
			writeUnaligned32(sin_zero2);
		}

		public virtual void readFromInetAddress(InetAddress inetAddress)
		{
			sin_len = @sizeof();
			sin_family = sceNetInet.AF_INET;
			sin_port = 0;
			sin_addr = sceNetInet.bytesToInternetAddress(inetAddress.Address);
		}

		public virtual void readFromInetAddress(InetAddress inetAddress, pspNetSockAddrInternet netSockAddrInternet)
		{
			sin_len = @sizeof();
			sin_family = netSockAddrInternet != null ? netSockAddrInternet.sin_family : sceNetInet.AF_INET;
			sin_port = netSockAddrInternet != null ? netSockAddrInternet.sin_port : 0;
			sin_addr = sceNetInet.bytesToInternetAddress(inetAddress.Address);
		}

		public virtual void readFromInetSocketAddress(InetSocketAddress inetSocketAddress)
		{
			sin_len = @sizeof();
			sin_family = sceNetInet.AF_INET;
			sin_port = inetSocketAddress.Port;
			sin_addr = sceNetInet.bytesToInternetAddress(inetSocketAddress.Address.Address);
		}

		public override int @sizeof()
		{
			return 16;
		}

		public virtual bool Equals(InetAddress inetAddress)
		{
			int addr = sceNetInet.bytesToInternetAddress(inetAddress.Address);
			return addr == sin_addr;
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();

			if (BaseAddress != 0)
			{
				s.Append(string.Format("0x{0:X8}(", BaseAddress));
			}
			s.Append(string.Format("pspNetSockAddrInternet[family={0:D}, port={1:D}, addr=0x{2:X8}({3})]", sin_family, sin_port, sin_addr, sceNetInet.internetAddressToString(sin_addr)));
			if (BaseAddress != 0)
			{
				s.Append(")");
			}

			return s.ToString();
		}
	}

}