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
namespace pspsharp.network.protocols
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.InternetChecksum.computeInternetChecksum;

	using Utilities = pspsharp.util.Utilities;

	public class TCP
	{
		// TCP packet structure, see https://en.wikipedia.org/wiki/Transmission_Control_Protocol
		public int sourcePort;
		public int destinationPort;
		public int sequenceNumber;
		public int acknowledgmentNumber;
		public int dataOffset;
		public int reserved;
		public bool flagNS; // ECN-nonce concealment protection
		public bool flagCWR; // Congestion Window Reduced
		public bool flagECE; // ECN-Echo has a dual role, depending on the value of the SYN flag
		public bool flagURG; // indicates that the Urgent pointer field is significant
		public bool flagACK; // indicates that the Acknowledgment field is significant
		public bool flagPSH; // Push function
		public bool flagRST; // Reset the connection
		public bool flagSYN; // Synchronize sequence numbers
		public bool flagFIN; // Last package from sender
		public int windowSize;
		public int checksum;
		public int urgentPointer;
		public sbyte[] options;
		public sbyte[] data;

		public TCP()
		{
			dataOffset = 5;
			windowSize = 0x4000;
		}

		public TCP(TCP tcp)
		{
			sourcePort = tcp.sourcePort;
			destinationPort = tcp.destinationPort;
			sequenceNumber = tcp.sequenceNumber;
			acknowledgmentNumber = tcp.acknowledgmentNumber;
			dataOffset = tcp.dataOffset;
			reserved = tcp.reserved;
			flagNS = tcp.flagNS;
			flagCWR = tcp.flagCWR;
			flagECE = tcp.flagECE;
			flagURG = tcp.flagURG;
			flagACK = tcp.flagACK;
			flagPSH = tcp.flagPSH;
			flagRST = tcp.flagRST;
			flagSYN = tcp.flagSYN;
			flagFIN = tcp.flagFIN;
			windowSize = tcp.windowSize;
			checksum = tcp.checksum;
			urgentPointer = tcp.urgentPointer;
			options = tcp.options;
			data = tcp.data;
		}

		public virtual void swapSourceAndDestination()
		{
			int port = sourcePort;
			sourcePort = destinationPort;
			destinationPort = port;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void computeChecksum(IPv4 ipv4) throws java.io.EOFException
		public virtual void computeChecksum(IPv4 ipv4)
		{
			// Computes the checksum with 0 at the checksum field
			checksum = 0;

			// The checksum also covers a 12 bytes pseudo header
			NetPacket checksumPacket = new NetPacket(12 + sizeOf());
			// Pseudo header:
			// - source IP address (4 bytes)
			// - destination IP address (4 bytes)
			// - 0 (1 byte)
			// - protocol (1 byte)
			// - TCP Length (2 bytes)
			checksumPacket.writeBytes(ipv4.sourceIPAddress);
			checksumPacket.writeBytes(ipv4.destinationIPAddress);
			checksumPacket.write8(0);
			checksumPacket.write8(ipv4.protocol);
			checksumPacket.write16(sizeOf());
			write(checksumPacket);
			checksum = computeInternetChecksum(checksumPacket.Buffer, 0, checksumPacket.Offset);
		}

		private int OptionsLength
		{
			get
			{
				return System.Math.Max((dataOffset - 5) * 4, 0);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(NetPacket packet) throws java.io.EOFException
		public virtual void read(NetPacket packet)
		{
			sourcePort = packet.read16();
			destinationPort = packet.read16();
			sequenceNumber = packet.read32();
			acknowledgmentNumber = packet.read32();
			dataOffset = packet.readBits(4);
			reserved = packet.readBits(3);
			flagNS = packet.readBoolean();
			flagCWR = packet.readBoolean();
			flagECE = packet.readBoolean();
			flagURG = packet.readBoolean();
			flagACK = packet.readBoolean();
			flagPSH = packet.readBoolean();
			flagRST = packet.readBoolean();
			flagSYN = packet.readBoolean();
			flagFIN = packet.readBoolean();
			windowSize = packet.read16();
			checksum = packet.read16();
			urgentPointer = packet.read16();
			options = packet.readBytes(OptionsLength);
			data = packet.readBytes(packet.Length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write(NetPacket packet) throws java.io.EOFException
		public virtual NetPacket write(NetPacket packet)
		{
			packet.write16(sourcePort);
			packet.write16(destinationPort);
			packet.write32(sequenceNumber);
			packet.write32(acknowledgmentNumber);
			packet.writeBits(dataOffset, 4);
			packet.writeBits(reserved, 3);
			packet.writeBoolean(flagNS);
			packet.writeBoolean(flagCWR);
			packet.writeBoolean(flagECE);
			packet.writeBoolean(flagURG);
			packet.writeBoolean(flagACK);
			packet.writeBoolean(flagPSH);
			packet.writeBoolean(flagRST);
			packet.writeBoolean(flagSYN);
			packet.writeBoolean(flagFIN);
			packet.write16(windowSize);
			packet.write16(checksum);
			packet.write16(urgentPointer);
			packet.writeBytes(options, 0, OptionsLength);
			packet.writeBytes(data);

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
			int size = dataOffset * 4;
			if (data != null)
			{
				size += data.Length;
			}

			return size;
		}

		private void addFlagString(StringBuilder s, string flagName, bool flagValue)
		{
			if (flagValue)
			{
				if (s.Length > 0)
				{
					s.Append("|");
				}
				s.Append(flagName);
			}
		}

		private string FlagsString
		{
			get
			{
				StringBuilder s = new StringBuilder();
				addFlagString(s, "NS", flagNS);
				addFlagString(s, "CWR", flagCWR);
				addFlagString(s, "ECE", flagECE);
				addFlagString(s, "URG", flagURG);
				addFlagString(s, "ACK", flagACK);
				addFlagString(s, "PSH", flagPSH);
				addFlagString(s, "RST", flagRST);
				addFlagString(s, "SYN", flagSYN);
				addFlagString(s, "FIN", flagFIN);
    
				return s.ToString();
			}
		}

		public override string ToString()
		{
			return string.Format("sourcePort=0x{0:X}, destinationPort=0x{1:X}, sequenceNumber=0x{2:X}, acknowledgmentNumber=0x{3:X}, dataOffset=0x{4:X}, reserved=0x{5:X}, flags={6}, windowSize=0x{7:X}, checksum=0x{8:X4}, urgentPointer=0x{9:X}, options={10}, data={11}", sourcePort, destinationPort, sequenceNumber, acknowledgmentNumber, dataOffset, reserved, FlagsString, windowSize, checksum, urgentPointer, Utilities.getMemoryDump(options, 0, OptionsLength), Utilities.getMemoryDump(data));
		}
	}

}