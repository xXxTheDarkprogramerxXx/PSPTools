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

	public class UDP
	{
		// UDP packet structure, see https://en.wikipedia.org/wiki/User_Datagram_Protocol
		public const int UDP_PORT_DNS = 53;
		public const int UDP_PORT_DHCP_SERVER = 67;
		public const int UDP_PORT_DHCP_CLIENT = 68;
		public int sourcePort;
		public int destinationPort;
		public int length;
		public int checksum;

		public UDP()
		{
		}

		public UDP(UDP udp)
		{
			sourcePort = udp.sourcePort;
			destinationPort = udp.destinationPort;
			length = udp.length;
			checksum = udp.checksum;
		}

		public virtual void swapSourceAndDestination()
		{
			int port = sourcePort;
			sourcePort = destinationPort;
			destinationPort = port;
		}

		public virtual void computeChecksum()
		{
			// The checksum field carries all-zeros if unused.
			checksum = 0;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(NetPacket packet) throws java.io.EOFException
		public virtual void read(NetPacket packet)
		{
			sourcePort = packet.read16();
			destinationPort = packet.read16();
			length = packet.read16();
			checksum = packet.read16();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write(NetPacket packet) throws java.io.EOFException
		public virtual NetPacket write(NetPacket packet)
		{
			packet.write16(sourcePort);
			packet.write16(destinationPort);
			packet.write16(length);
			packet.write16(checksum);

			return packet;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write() throws java.io.EOFException
		public virtual NetPacket write()
		{
			return write(new NetPacket(sizeOf()));
		}

		public virtual int sizeOf()
		{
			return 8;
		}

		public override string ToString()
		{
			return string.Format("sourcePort=0x{0:X}, destinationPort=0x{1:X}, length=0x{2:X}, checksum=0x{3:X4}", sourcePort, destinationPort, length, checksum);
		}
	}

}