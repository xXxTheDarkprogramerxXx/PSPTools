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
namespace pspsharp.network.protocols
{

	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;

	public class EtherFrame
	{
		public const int ETHER_TYPE_IPv4 = 0x0800;
		public const int ETHER_TYPE_ARP = 0x0806;
		// Frame specification as defined by IEEE Std 802.3
		// Frame:
		//     destination address: 6 octets
		//     source address: 6 octets
		//     length/type: 2 octets
		//     client data: 46 to 1500 octets
		public pspNetMacAddress dstMac;
		public pspNetMacAddress srcMac;
		public int type;

		public EtherFrame()
		{
		}

		public EtherFrame(EtherFrame frame)
		{
			dstMac = frame.dstMac;
			srcMac = frame.srcMac;
			type = frame.type;
		}

		public virtual void swapSourceAndDestination()
		{
			pspNetMacAddress mac = srcMac;
			srcMac = dstMac;
			dstMac = mac;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(NetPacket packet) throws java.io.EOFException
		public virtual void read(NetPacket packet)
		{
			dstMac = packet.readMacAddress();
			srcMac = packet.readMacAddress();
			type = packet.read16();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write(NetPacket packet) throws java.io.EOFException
		public virtual NetPacket write(NetPacket packet)
		{
			packet.writeMacAddress(dstMac);
			packet.writeMacAddress(srcMac);
			packet.write16(type);

			return packet;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write() throws java.io.EOFException
		public virtual NetPacket write()
		{
			return write(new NetPacket(sizeOf()));
		}

		public static int sizeOf()
		{
			return 14;
		}

		public override string ToString()
		{
			return string.Format("dstMac={0}, srcMac={1}, type/length=0x{2:X4}", dstMac, srcMac, type);
		}
	}

}