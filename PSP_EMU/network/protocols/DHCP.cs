using System.Collections.Generic;
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
//	import static pspsharp.network.accesspoint.AccessPoint.IP_ADDRESS_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.NetPacket.getIpAddressString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.UDP.UDP_PORT_DHCP_CLIENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.UDP.UDP_PORT_DHCP_SERVER;


	using Utilities = pspsharp.util.Utilities;

	public class DHCP
	{
		// See https://en.wikipedia.org/wiki/Dynamic_Host_Configuration_Protocol
		public static readonly sbyte[] nullIPAddress = new sbyte[] {(sbyte) 0, (sbyte) 0, (sbyte) 0, (sbyte) 0};
		public static readonly sbyte[] broadcastIPAddress = new sbyte[] {unchecked((sbyte) 0xFF), unchecked((sbyte) 0xFF), unchecked((sbyte) 0xFF), unchecked((sbyte) 0xFF)};
		public const int DHCP_BOOT_REQUEST = 1;
		public const int DHCP_BOOT_REPLY = 2;
		// See DHCP Options in https://tools.ietf.org/html/rfc1533
		public const int DHCP_OPTION_MAGIC_COOKIE = 0x63825363;
		public const int DHCP_OPTION_PAD = 0;
		public const int DHCP_OPTION_SUBNET_MASK = 1;
		public const int DHCP_OPTION_ROUTER = 3;
		public const int DHCP_OPTION_DNS = 6;
		public const int DHCP_OPTION_DOMAIN_NAME = 15;
		public const int DHCP_OPTION_BROADCAST_ADDRESS = 28;
		public const int DHCP_OPTION_REQUESTED_IP_ADDRESS = 50;
		public const int DHCP_OPTION_IP_ADDRESS_LEASE_TIME = 51;
		public const int DHCP_OPTION_MESSAGE_TYPE = 53;
		public const int DHCP_OPTION_SERVER_IDENTIFIER = 54;
		public const int DHCP_OPTION_PARAMETER_REQUEST = 55;
		public const int DHCP_OPTION_MAXIMUM_DHCP_MESSAGE = 57;
		public const int DHCP_OPTION_CLIENT_IDENTIFIER = 61;
		public const int DHCP_OPTION_END = 255;
		public static readonly string[] DHCP_OPTION_NAMES = new string[256];
		// See DHCP Message type in https://tools.ietf.org/html/rfc1533, chapter "9.4. DHCP Message Type"
		public const int DHCP_OPTION_MESSAGE_TYPE_DHCPDISCOVER = 1;
		public const int DHCP_OPTION_MESSAGE_TYPE_DHCPOFFER = 2;
		public const int DHCP_OPTION_MESSAGE_TYPE_DHCPREQUEST = 3;
		public const int DHCP_OPTION_MESSAGE_TYPE_DHCPACK = 5;
		public int opcode;
		public int hardwareAddressType;
		public int hardwareAddressLength;
		public int hops;
		public int transactionID;
		public int seconds;
		public bool flagBroadcast;
		public int flagsZero;
		public sbyte[] clientIPAddress;
		public sbyte[] yourIPAddress;
		public sbyte[] nextServerIPAddress;
		public sbyte[] relayAgentIPAddress;
		public sbyte[] clientHardwareAddress;
		public string serverHostName;
		public string bootFileName;
		public IList<DHCPOption> options;

		static DHCP()
		{
			DHCP_OPTION_NAMES[DHCP_OPTION_PAD] = "PAD";
			DHCP_OPTION_NAMES[DHCP_OPTION_SUBNET_MASK] = "SUBNET_MASK";
			DHCP_OPTION_NAMES[DHCP_OPTION_ROUTER] = "ROUTER";
			DHCP_OPTION_NAMES[DHCP_OPTION_DNS] = "DNS";
			DHCP_OPTION_NAMES[DHCP_OPTION_DOMAIN_NAME] = "DOMAIN_NAME";
			DHCP_OPTION_NAMES[DHCP_OPTION_BROADCAST_ADDRESS] = "BROADCAST_ADDRESS";
			DHCP_OPTION_NAMES[DHCP_OPTION_REQUESTED_IP_ADDRESS] = "REQUESTED_IP_ADDRESS";
			DHCP_OPTION_NAMES[DHCP_OPTION_IP_ADDRESS_LEASE_TIME] = "IP_ADDRESS_LEASE_TIME";
			DHCP_OPTION_NAMES[DHCP_OPTION_MESSAGE_TYPE] = "MESSAGE_TYPE";
			DHCP_OPTION_NAMES[DHCP_OPTION_SERVER_IDENTIFIER] = "SERVER_IDENTIFIER";
			DHCP_OPTION_NAMES[DHCP_OPTION_PARAMETER_REQUEST] = "PARAMETER_REQUEST";
			DHCP_OPTION_NAMES[DHCP_OPTION_MAXIMUM_DHCP_MESSAGE] = "MAXIMUM_DHCP_MESSAGE";
			DHCP_OPTION_NAMES[DHCP_OPTION_CLIENT_IDENTIFIER] = "CLIENT_IDENTIFIER";
			DHCP_OPTION_NAMES[DHCP_OPTION_END] = "END";
		}

		public class DHCPOption
		{
			internal int tag;
			internal int length;
			internal sbyte[] data;

			public DHCPOption()
			{
				tag = DHCP_OPTION_END;
			}

			public DHCPOption(int tag)
			{
				this.tag = tag;
			}

			public DHCPOption(int tag, sbyte data)
			{
				this.tag = tag;
				this.length = 1;
				this.data = new sbyte[] {data};
			}

			public DHCPOption(int tag, int data)
			{
				this.tag = tag;
				this.length = 4;
				this.data = new sbyte[4];
				this.data[0] = (sbyte)(data >> 24);
				this.data[1] = (sbyte)(data >> 16);
				this.data[2] = (sbyte)(data >> 8);
				this.data[3] = (sbyte) data;
			}

			public DHCPOption(int tag, sbyte[] data)
			{
				this.tag = tag;
				this.length = data == null ? 0 : data.Length;
				this.data = data;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(NetPacket packet) throws java.io.EOFException
			public virtual void read(NetPacket packet)
			{
				tag = packet.read8();
				if (!ZeroLengthTag)
				{
					length = packet.read8();
					data = packet.readBytes(length);
				}
			}

			internal virtual bool ZeroLengthTag
			{
				get
				{
					// PAD and END tags have no length.
					return tag == DHCP_OPTION_PAD || tag == DHCP_OPTION_END;
				}
			}

			public virtual int sizeOf()
			{
				if (ZeroLengthTag)
				{
					return 1;
				}

				return 2 + length;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write(NetPacket packet) throws java.io.EOFException
			public virtual NetPacket write(NetPacket packet)
			{
				packet.write8(tag);
				if (!ZeroLengthTag)
				{
					packet.write8(length);
					packet.writeBytes(data, 0, length);
				}

				return packet;
			}

			public virtual int DataAsInt
			{
				get
				{
					int value = 0;
    
					switch (length)
					{
						case 1:
							value = data[0] & 0xFF;
							break;
						case 2:
							value = (data[0] & 0xFF) << 8;
							value |= data[1] & 0xFF;
							break;
						case 4:
							value = (data[0] & 0xFF) << 24;
							value |= (data[1] & 0xFF) << 16;
							value |= (data[2] & 0xFF) << 8;
							value |= data[3] & 0xFF;
							break;
					}
    
					return value;
				}
			}

			internal virtual string TagName
			{
				get
				{
					if (tag >= 0 && tag < DHCP_OPTION_NAMES.Length)
					{
						if (!string.ReferenceEquals(DHCP_OPTION_NAMES[tag], null))
						{
							return DHCP_OPTION_NAMES[tag];
						}
					}
    
					return string.Format("tag={0:D}", tag);
				}
			}

			public override string ToString()
			{
				if (ZeroLengthTag)
				{
					return TagName;
				}

				return string.Format("{0}, length=0x{1:X}, data={2}", TagName, length, Utilities.getMemoryDump(data, 0, length));
			}
		}

		public DHCP()
		{
			options = new LinkedList<DHCP.DHCPOption>();
		}

		public DHCP(DHCP dhcp)
		{
			opcode = dhcp.opcode;
			hardwareAddressType = dhcp.hardwareAddressType;
			hardwareAddressLength = dhcp.hardwareAddressLength;
			hops = dhcp.hops;
			transactionID = dhcp.transactionID;
			seconds = dhcp.seconds;
			flagBroadcast = dhcp.flagBroadcast;
			flagsZero = dhcp.flagsZero;
			clientIPAddress = dhcp.clientIPAddress;
			yourIPAddress = dhcp.yourIPAddress;
			nextServerIPAddress = dhcp.nextServerIPAddress;
			relayAgentIPAddress = dhcp.relayAgentIPAddress;
			clientHardwareAddress = dhcp.clientHardwareAddress;
			serverHostName = dhcp.serverHostName;
			bootFileName = dhcp.bootFileName;
			options = dhcp.options;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(NetPacket packet) throws java.io.EOFException
		public virtual void read(NetPacket packet)
		{
			opcode = packet.read8();
			hardwareAddressType = packet.read8();
			hardwareAddressLength = packet.read8();
			hops = packet.read8();
			transactionID = packet.read32();
			seconds = packet.read16();
			flagBroadcast = packet.readBoolean();
			flagsZero = packet.readBits(15);
			clientIPAddress = packet.readBytes(IP_ADDRESS_LENGTH);
			yourIPAddress = packet.readBytes(IP_ADDRESS_LENGTH);
			nextServerIPAddress = packet.readBytes(IP_ADDRESS_LENGTH);
			relayAgentIPAddress = packet.readBytes(IP_ADDRESS_LENGTH);
			clientHardwareAddress = packet.readBytes(16);
			serverHostName = packet.readStringNZ(64);
			bootFileName = packet.readStringNZ(128);

			int optionsLength = 312;
			int magicCookie = packet.read32();
			optionsLength -= 4;
			if (magicCookie == DHCP_OPTION_MAGIC_COOKIE)
			{
				while (optionsLength > 0)
				{
					DHCPOption option = new DHCPOption();
					option.read(packet);
					options.Add(option);
					optionsLength -= option.sizeOf();

					// END tag marks the end of the options
					if (option.tag == DHCP_OPTION_END)
					{
						break;
					}
				}
			}
			else
			{
				packet.skip8(optionsLength);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write(NetPacket packet) throws java.io.EOFException
		public virtual NetPacket write(NetPacket packet)
		{
			packet.write8(opcode);
			packet.write8(hardwareAddressType);
			packet.write8(hardwareAddressLength);
			packet.write8(hops);
			packet.write32(transactionID);
			packet.write16(seconds);
			packet.writeBoolean(flagBroadcast);
			packet.writeBits(flagsZero, 15);
			packet.writeBytes(clientIPAddress, 0, IP_ADDRESS_LENGTH);
			packet.writeBytes(yourIPAddress, 0, IP_ADDRESS_LENGTH);
			packet.writeBytes(nextServerIPAddress, 0, IP_ADDRESS_LENGTH);
			packet.writeBytes(relayAgentIPAddress, 0, IP_ADDRESS_LENGTH);
			packet.writeBytes(clientHardwareAddress, 0, 16);
			packet.writeStringNZ(serverHostName, 64);
			packet.writeStringNZ(bootFileName, 128);

			int optionsLength = 312;
			packet.write32(DHCP_OPTION_MAGIC_COOKIE);
			optionsLength -= 4;
			foreach (DHCPOption option in options)
			{
				if (optionsLength < option.sizeOf())
				{
					break;
				}
				option.write(packet);
			}

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
			return 548;
		}

		public virtual bool isMessageOfType(UDP udp, IPv4 ipv4, int messageType)
		{
			if (opcode != DHCP_BOOT_REQUEST)
			{
				return false;
			}
			if (udp.sourcePort != UDP_PORT_DHCP_CLIENT || udp.destinationPort != UDP_PORT_DHCP_SERVER)
			{
				return false;
			}
			if (!Arrays.Equals(ipv4.sourceIPAddress, nullIPAddress))
			{
				return false;
			}
			if (!Arrays.Equals(ipv4.destinationIPAddress, broadcastIPAddress))
			{
				return false;
			}
			DHCPOption option = getOptionByTag(DHCP_OPTION_MESSAGE_TYPE);
			if (option == null || option.length != 1)
			{
				return false;
			}
			int optionMessageType = option.DataAsInt;
			if (optionMessageType != messageType)
			{
				return false;
			}

			return true;
		}

		public virtual bool isDiscovery(UDP udp, IPv4 ipv4)
		{
			return isMessageOfType(udp, ipv4, DHCP_OPTION_MESSAGE_TYPE_DHCPDISCOVER);
		}

		public virtual bool isRequest(UDP udp, IPv4 ipv4, sbyte[] requestedIpAddress)
		{
			if (!isMessageOfType(udp, ipv4, DHCP_OPTION_MESSAGE_TYPE_DHCPREQUEST))
			{
				return false;
			}

			// Verify that the requested IP address is matching
			// the one specified in the options.
			DHCPOption requestedIpAddressOption = getOptionByTag(DHCP_OPTION_REQUESTED_IP_ADDRESS);
			if (requestedIpAddressOption == null || requestedIpAddressOption.length != 4)
			{
				return false;
			}
			if (!Arrays.Equals(requestedIpAddress, requestedIpAddressOption.data))
			{
				return false;
			}

			return true;
		}

		public virtual void addOption(DHCPOption option)
		{
			options.Add(option);
		}

		public virtual void clearOptions()
		{
			options.Clear();
		}

		public virtual DHCPOption getOptionByTag(int tag)
		{
			foreach (DHCPOption option in options)
			{
				if (option.tag == tag)
				{
					return option;
				}
			}

			return null;
		}

		private string optionsToString()
		{
			StringBuilder s = new StringBuilder();

			foreach (DHCPOption option in options)
			{
				if (s.Length > 0)
				{
					s.Append(", ");
				}
				s.Append(option.ToString());
			}

			return s.ToString();
		}

		public override string ToString()
		{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("opcode=0x%X, hardwareAddressType=0x%X, hardwareAddressLength=0x%X, hops=0x%X, transactionID=0x%08X, seconds=0x%X, flagBroadcast=%b, flags=0x%04X, clientIPAddress=%s, yourIPAddress=%s, nextServerIPAddress=%s, relayAgentIPAddress=%s, clientHardwareAddress=%s, serverHostName='%s', bootFileName='%s', options=%s", opcode, hardwareAddressType, hardwareAddressLength, hops, transactionID, seconds, flagBroadcast, flagsZero, getIpAddressString(clientIPAddress), getIpAddressString(yourIPAddress), getIpAddressString(nextServerIPAddress), getIpAddressString(relayAgentIPAddress), pspsharp.util.Utilities.getMemoryDump(clientHardwareAddress, 0, hardwareAddressLength), serverHostName, bootFileName, optionsToString());
			return string.Format("opcode=0x%X, hardwareAddressType=0x%X, hardwareAddressLength=0x%X, hops=0x%X, transactionID=0x%08X, seconds=0x%X, flagBroadcast=%b, flags=0x%04X, clientIPAddress=%s, yourIPAddress=%s, nextServerIPAddress=%s, relayAgentIPAddress=%s, clientHardwareAddress=%s, serverHostName='%s', bootFileName='%s', options=%s", opcode, hardwareAddressType, hardwareAddressLength, hops, transactionID, seconds, flagBroadcast, flagsZero, getIpAddressString(clientIPAddress), getIpAddressString(yourIPAddress), getIpAddressString(nextServerIPAddress), getIpAddressString(relayAgentIPAddress), Utilities.getMemoryDump(clientHardwareAddress, 0, hardwareAddressLength), serverHostName, bootFileName, optionsToString());
		}
	}

}