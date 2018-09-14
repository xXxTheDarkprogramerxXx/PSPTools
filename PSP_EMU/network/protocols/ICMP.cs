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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.InternetChecksum.computeInternetChecksum;

	using Utilities = pspsharp.util.Utilities;

	public class ICMP
	{
		// ICMP packet format, see https://en.wikipedia.org/wiki/Internet_Control_Message_Protocol
		public const int ICMP_CONTROL_ECHO_REPLY = 0; // Ping reply
		public const int ICMP_CONTROL_ECHO_REQUEST = 8; // Used to ping
		public int type;
		public int code;
		public int checksum;
		public int restOfHeader;
		public sbyte[] payload;

		public ICMP()
		{
		}

		public ICMP(ICMP icmp)
		{
			type = icmp.type;
			code = icmp.code;
			checksum = icmp.checksum;
			restOfHeader = icmp.restOfHeader;
			payload = icmp.payload;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(NetPacket packet) throws java.io.EOFException
		public virtual void read(NetPacket packet)
		{
			type = packet.read8();
			code = packet.read8();
			checksum = packet.read16();
			restOfHeader = packet.read32();
			// The rest of the packet is the payload data
			payload = packet.readBytes(packet.Length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write(NetPacket packet) throws java.io.EOFException
		public virtual NetPacket write(NetPacket packet)
		{
			packet.write8(type);
			packet.write8(code);
			packet.write16(checksum);
			packet.write32(restOfHeader);
			packet.writeBytes(payload);

			return packet;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write() throws java.io.EOFException
		public virtual NetPacket write()
		{
			return write(new NetPacket(sizeOf()));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void computeChecksum() throws java.io.EOFException
		public virtual void computeChecksum()
		{
			// Computes the checksum with 0 at the checksum field
			checksum = 0;
			NetPacket checksumPacket = write();
			checksum = computeInternetChecksum(checksumPacket.Buffer, 0, checksumPacket.Offset);
		}

		public virtual int sizeOf()
		{
			int size = 8;
			if (payload != null)
			{
				size += payload.Length;
			}

			return size;
		}

		public override string ToString()
		{
			return string.Format("type=0x{0:X}, code=0x{1:X}, checksum=0x{2:X4}, restOfHeader=0x{3:X8}, payload={4}", type, code, checksum, restOfHeader, Utilities.getMemoryDump(payload, 0, payload.Length));
		}
	}

}