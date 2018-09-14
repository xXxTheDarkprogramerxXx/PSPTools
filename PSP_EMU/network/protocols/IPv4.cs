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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.NetPacket.getIpAddressString;

	using Utilities = pspsharp.util.Utilities;

	public class IPv4
	{
		// IPv4 packet format, see https://en.wikipedia.org/wiki/IPv4
		public const int IPv4_PROTOCOL_ICMP = 1;
		public const int IPv4_PROTOCOL_TCP = 6;
		public const int IPv4_PROTOCOL_UDP = 17;
		public int version;
		public int internetHeaderLength;
		public int differentiatedServicesCodePoint;
		public int explicitCongestionNotification;
		public int totalLength;
		public int identification;
		public int flags;
		public int fragmentOffset;
		public int timeToLive;
		public int protocol;
		public int headerChecksum;
		public sbyte[] sourceIPAddress;
		public sbyte[] destinationIPAddress;
		public sbyte[] options;

		public IPv4()
		{
			version = 4;
			internetHeaderLength = 5;
			timeToLive = 0x40;
		}

		public IPv4(IPv4 ipv4)
		{
			version = ipv4.version;
			internetHeaderLength = ipv4.internetHeaderLength;
			differentiatedServicesCodePoint = ipv4.differentiatedServicesCodePoint;
			explicitCongestionNotification = ipv4.explicitCongestionNotification;
			totalLength = ipv4.totalLength;
			identification = ipv4.identification;
			flags = ipv4.flags;
			fragmentOffset = ipv4.fragmentOffset;
			timeToLive = ipv4.timeToLive;
			protocol = ipv4.protocol;
			headerChecksum = ipv4.headerChecksum;
			sourceIPAddress = ipv4.sourceIPAddress;
			destinationIPAddress = ipv4.destinationIPAddress;
			options = ipv4.options;
		}

		private int OptionsLength
		{
			get
			{
				return System.Math.Max((internetHeaderLength - 5) * 4, 0);
			}
		}

		public virtual void swapSourceAndDestination()
		{
			sbyte[] ip = sourceIPAddress;
			sourceIPAddress = destinationIPAddress;
			destinationIPAddress = ip;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void computeChecksum() throws java.io.EOFException
		public virtual void computeChecksum()
		{
			// Computes the checksum with 0 at the headerChecksum field
			headerChecksum = 0;
			NetPacket checksumPacket = write();
			headerChecksum = computeInternetChecksum(checksumPacket.Buffer, 0, checksumPacket.Offset);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(NetPacket packet) throws java.io.EOFException
		public virtual void read(NetPacket packet)
		{
			version = packet.readBits(4);
			internetHeaderLength = packet.readBits(4);
			differentiatedServicesCodePoint = packet.readBits(6);
			explicitCongestionNotification = packet.readBits(2);
			totalLength = packet.read16();
			identification = packet.read16();
			flags = packet.readBits(3);
			fragmentOffset = packet.readBits(13);
			timeToLive = packet.read8();
			protocol = packet.read8();
			headerChecksum = packet.read16();
			sourceIPAddress = packet.readIpAddress();
			destinationIPAddress = packet.readIpAddress();
			options = packet.readBytes(OptionsLength);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write(NetPacket packet) throws java.io.EOFException
		public virtual NetPacket write(NetPacket packet)
		{
			packet.writeBits(version, 4);
			packet.writeBits(internetHeaderLength, 4);
			packet.writeBits(differentiatedServicesCodePoint, 6);
			packet.writeBits(explicitCongestionNotification, 2);
			packet.write16(totalLength);
			packet.write16(identification);
			packet.writeBits(flags, 3);
			packet.writeBits(fragmentOffset, 13);
			packet.write8(timeToLive);
			packet.write8(protocol);
			packet.write16(headerChecksum);
			packet.writeIpAddress(sourceIPAddress);
			packet.writeIpAddress(destinationIPAddress);
			packet.writeBytes(options, 0, OptionsLength);

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
			return internetHeaderLength * 4;
		}

		public override string ToString()
		{
			return string.Format("version=0x{0:X}, internetHeaderLength=0x{1:X}, differentiatedServicesCodePoint=0x{2:X}, explicitCongestionNotification=0x{3:X}, totalLength=0x{4:X}, identification=0x{5:X}, flags=0x{6:X}, fragmentOffset=0x{7:X}, timeToLive=0x{8:X}, protocol=0x{9:X}, headerChecksum=0x{10:X4}, sourceIP={11}, destinationIP={12}, options={13}", version, internetHeaderLength, differentiatedServicesCodePoint, explicitCongestionNotification, totalLength, identification, flags, fragmentOffset, timeToLive, protocol, headerChecksum, getIpAddressString(sourceIPAddress), getIpAddressString(destinationIPAddress), Utilities.getMemoryDump(options, 0, OptionsLength));
		}
	}

}