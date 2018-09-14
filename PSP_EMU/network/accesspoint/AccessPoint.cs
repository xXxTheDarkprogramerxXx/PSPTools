using System;
using System.Collections.Generic;
using System.Threading;

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
namespace pspsharp.network.accesspoint
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhoc.ANY_MAC_ADDRESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceWlan.WLAN_CMD_DATA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.hardware.Wlan.MAC_ADDRESS_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.ARP.ARP_OPERATION_REPLY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.ARP.ARP_OPERATION_REQUEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.DHCP.DHCP_BOOT_REPLY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.DNS.DNS_RESPONSE_CODE_NAME_ERROR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.DNS.DNS_RESPONSE_CODE_NO_ERROR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.EtherFrame.ETHER_TYPE_ARP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.EtherFrame.ETHER_TYPE_IPv4;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.ICMP.ICMP_CONTROL_ECHO_REQUEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.IPv4.IPv4_PROTOCOL_ICMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.IPv4.IPv4_PROTOCOL_TCP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.IPv4.IPv4_PROTOCOL_UDP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.NetPacket.getIpAddressString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.protocols.UDP.UDP_PORT_DNS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeStringNZ;


	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using sceNetApctl = pspsharp.HLE.modules.sceNetApctl;
	using sceNetInet = pspsharp.HLE.modules.sceNetInet;
	using sceWlan = pspsharp.HLE.modules.sceWlan;
	using ARP = pspsharp.network.protocols.ARP;
	using DHCP = pspsharp.network.protocols.DHCP;
	using DNS = pspsharp.network.protocols.DNS;
	using DNSAnswerRecord = pspsharp.network.protocols.DNS.DNSAnswerRecord;
	using EtherFrame = pspsharp.network.protocols.EtherFrame;
	using ICMP = pspsharp.network.protocols.ICMP;
	using IPv4 = pspsharp.network.protocols.IPv4;
	using NetPacket = pspsharp.network.protocols.NetPacket;
	using TCP = pspsharp.network.protocols.TCP;
	using UDP = pspsharp.network.protocols.UDP;
	using DNSRecord = pspsharp.network.protocols.DNS.DNSRecord;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class AccessPoint
	{
		public static Logger log = Logger.getLogger("accesspoint");
		public const int HARDWARE_TYPE_ETHERNET = 0x0001;
		public const int IP_ADDRESS_LENGTH = 4;
		private const int BUFFER_SIZE = 2000;
		private static AccessPoint instance;
		private int apSocketPort = 30020;
		private pspNetMacAddress apMacAddress;
		private sbyte[] apIpAddress;
		private sbyte[] localIpAddress;
		private DatagramSocket apSocket;
		private AccessPointThread apThread;
		private string apSsid;
		private IList<TcpConnectionState> tcpConnectionStates;
		private System.Random random;

		private class TcpConnectionState
		{
			public pspNetMacAddress sourceMacAddress;
			public sbyte[] sourceIPAddress;
			public int sourcePort;
			public int sourceSequenceNumber;
			public pspNetMacAddress destinationMacAddress;
			public sbyte[] destinationIPAddress;
			public int destinationPort;
			public int destinationSequenceNumber;
			public SocketChannel socketChannel;
			public sbyte[] pendingWriteData;
			public bool pendingConnection;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void openChannel() throws java.io.IOException
			internal virtual void openChannel()
			{
				if (socketChannel == null)
				{
					socketChannel = SocketChannel.open();
					// Use a non-blocking channel as we are polling for data
					socketChannel.configureBlocking(false);
					// Connect has no timeout
					socketChannel.socket().SoTimeout = 0;
				}
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void connect() throws java.io.IOException
			public virtual void connect()
			{
				openChannel();
				if (!socketChannel.Connected && !socketChannel.ConnectionPending)
				{
					SocketAddress socketAddress = new InetSocketAddress(InetAddress.getByAddress(destinationIPAddress), destinationPort);
					socketChannel.connect(socketAddress);
				}
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
			public virtual void close()
			{
				if (socketChannel != null)
				{
					socketChannel.close();
					socketChannel = null;
				}
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(byte[] buffer) throws java.io.IOException
			public virtual void write(sbyte[] buffer)
			{
				if (buffer != null)
				{
					write(buffer, 0, buffer.Length);
				}
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(byte[] buffer, int offset, int length) throws java.io.IOException
			public virtual void write(sbyte[] buffer, int offset, int length)
			{
				socketChannel.write(ByteBuffer.wrap(buffer, 0, length));
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte[] read() throws java.io.IOException
			public virtual sbyte[] read()
			{
				sbyte[] buffer = new sbyte[BUFFER_SIZE];
				int length = socketChannel.read(ByteBuffer.wrap(buffer));
				if (length <= 0)
				{
					return null;
				}

				sbyte[] readBuffer = new sbyte[length];
				Array.Copy(buffer, 0, readBuffer, 0, length);

				return readBuffer;
			}

			public virtual void addPendingWriteData(sbyte[] data)
			{
				if (data != null && data.Length > 0)
				{
					pendingWriteData = Utilities.extendArray(pendingWriteData, data);
				}
			}

			public override string ToString()
			{
				return string.Format("source={0}/{1}:0x{2:X}(sequenceNumber=0x{3:X}), destination={4}/{5}:0x{6:X}(sequenceNumber=0x{7:X})", sourceMacAddress, getIpAddressString(sourceIPAddress), sourcePort, sourceSequenceNumber, destinationMacAddress, getIpAddressString(destinationIPAddress), destinationPort, destinationSequenceNumber);
			}
		}

		private class AccessPointThread : Thread
		{
			private readonly AccessPoint outerInstance;

			public AccessPointThread(AccessPoint outerInstance)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal bool exit_Renamed = false;

			public override void run()
			{
				while (!exit_Renamed)
				{
					if (!outerInstance.receiveAccessPointMessage())
					{
						if (!exit_Renamed && !outerInstance.receiveTcpMessages())
						{
							Utilities.sleep(10, 0);
						}
					}
				}
			}

			public virtual void exit()
			{
				exit_Renamed = true;
			}
		}

		public static AccessPoint Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new AccessPoint();
				}
    
				return instance;
			}
		}

		private AccessPoint()
		{
			// Generate a random MAC address for the Address Point
			apMacAddress = new pspNetMacAddress(pspNetMacAddress.RandomMacAddress);

			apIpAddress = getIpAddress(sceNetApctl.Gateway);
			localIpAddress = getIpAddress(sceNetApctl.LocalHostIP);

			tcpConnectionStates = new LinkedList<>();

			random = new System.Random();

			apThread = new AccessPointThread(this);
			apThread.Daemon = true;
			apThread.Name = "Access Point Thread";
			apThread.Start();

			if (log.DebugEnabled)
			{
				log.debug(string.Format("AccessPoint using MAC={0}, IP={1}", apMacAddress, getIpAddressString(apIpAddress)));
			}
		}

		public static void exit()
		{
			if (instance != null)
			{
				if (instance.apThread != null)
				{
					instance.apThread.exit();
					instance.apThread = null;
				}
			}
		}

		public virtual int Port
		{
			get
			{
				return apSocketPort;
			}
		}

		public virtual pspNetMacAddress MacAddress
		{
			get
			{
				return apMacAddress;
			}
		}

		public virtual sbyte[] IpAddress
		{
			get
			{
				return apIpAddress;
			}
		}

		private sbyte[] LocalIpAddress
		{
			get
			{
				return localIpAddress;
			}
		}

		private static sbyte[] getIpAddress(string hostName)
		{
			try
			{
				InetAddress inetAddress = InetAddress.getByName(hostName);
				return inetAddress.Address;
			}
			catch (UnknownHostException e)
			{
				log.error("getIpAddress", e);
			}

			return null;
		}

		private static sbyte[] getIpAddress(int ipAddressInt)
		{
			sbyte[] ipAddress = new sbyte[IP_ADDRESS_LENGTH];
			ipAddress[0] = (sbyte)(ipAddressInt >> 24);
			ipAddress[1] = (sbyte)(ipAddressInt >> 16);
			ipAddress[2] = (sbyte)(ipAddressInt >> 8);
			ipAddress[3] = (sbyte) ipAddressInt;

			return ipAddress;
		}

		private bool createAccessPointSocket()
		{
			if (apSocket == null)
			{
				bool retry;
				do
				{
					retry = false;
					try
					{
						apSocket = new DatagramSocket(apSocketPort);
						// For broadcast
						apSocket.Broadcast = true;
						// Non-blocking (timeout = 0 would mean blocking)
						apSocket.SoTimeout = 1;
					}
					catch (BindException e)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("createAccessPointSocket port {0:D} already in use ({1}) - retrying with port {2:D}", apSocketPort, e, apSocketPort + 1));
						}
						// The port is already busy, retrying with another port
						apSocketPort++;
						retry = true;
					}
					catch (SocketException e)
					{
						log.error("createWlanSocket", e);
					}
				} while (retry);
			}

			return apSocket != null;
		}

		private bool receiveAccessPointMessage()
		{
			bool packetReceived = false;

			if (!createAccessPointSocket())
			{
				return packetReceived;
			}

			sbyte[] bytes = new sbyte[10000];
			DatagramPacket packet = new DatagramPacket(bytes, bytes.Length);
			try
			{
				apSocket.receive(packet);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("receiveMessage message: {0}", Utilities.getMemoryDump(packet.Data, packet.Offset, packet.Length)));
				}

				packetReceived = true;

				sbyte[] dataBytes = packet.Data;
				int dataOffset = packet.Offset;
				int dataLength = packet.Length;
				NetPacket netPacket = new NetPacket(dataBytes, dataOffset, dataLength);

				processMessage(netPacket);
			}
			catch (SocketTimeoutException)
			{
				// Timeout can be ignored as we are polling
			}
			catch (IOException e)
			{
				log.error("receiveMessage", e);
			}

			return packetReceived;
		}

		private int BroadcastPort
		{
			get
			{
				return sceWlan.SocketPort;
			}
		}

		private void sendPacket(sbyte[] buffer, int bufferLength)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sendPacket {0}", Utilities.getMemoryDump(buffer, 0, bufferLength)));
			}

			try
			{
				InetSocketAddress[] broadcastAddress = sceNetInet.getBroadcastInetSocketAddress(BroadcastPort);
				if (broadcastAddress != null)
				{
					for (int i = 0; i < broadcastAddress.Length; i++)
					{
						DatagramPacket packet = new DatagramPacket(buffer, bufferLength, broadcastAddress[i]);
						apSocket.send(packet);
					}
				}
			}
			catch (UnknownHostException e)
			{
				log.error("sendPacket", e);
			}
			catch (IOException e)
			{
				log.error("sendPacket", e);
			}
		}

		private void sendPacket(NetPacket packet)
		{
			int packetLength = packet.Offset;

			sbyte[] buffer = new sbyte[33 + packetLength];
			int offset = 0;

			buffer[offset++] = WLAN_CMD_DATA;

			writeStringNZ(buffer, offset, 32, apSsid);
			offset += 32;

			Array.Copy(packet.Buffer, 0, buffer, offset, packetLength);
			offset += packetLength;

			sendPacket(buffer, offset);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void processMessage(pspsharp.network.protocols.NetPacket packet) throws java.io.EOFException
		private void processMessage(NetPacket packet)
		{
			sbyte cmd = packet.readByte();

			if (cmd != WLAN_CMD_DATA)
			{
				log.error(string.Format("processMessage unknown command 0x{0:X}", cmd));
				return;
			}

			string ssid = packet.readStringNZ(32);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("processMessage ssid='{0}'", ssid));
			}

			if (string.ReferenceEquals(apSsid, null))
			{
				apSsid = ssid;
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Using ssid='{0}' for the Access Point", apSsid));
				}
			}

			EtherFrame frame = new EtherFrame();
			frame.read(packet);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("processMessage {0}", frame));
			}

			switch (frame.type)
			{
				case ETHER_TYPE_ARP:
					processMessageARP(packet);
					break;
				case ETHER_TYPE_IPv4: // See https://www.ietf.org/rfc/rfc894.txt
					processMessageDatagram(packet, frame);
					break;
				default:
					log.warn(string.Format("Unknow message of type 0x{0:X4}", frame.type));
					break;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void processMessageARP(pspsharp.network.protocols.NetPacket packet) throws java.io.EOFException
		private void processMessageARP(NetPacket packet)
		{
			ARP arp = new ARP();
			arp.read(packet);

			if (arp.hardwareType != HARDWARE_TYPE_ETHERNET)
			{
				log.warn(string.Format("processMessageARP unknown hardwareType=0x{0:X}", arp.hardwareType));
				return;
			}

			if (arp.protocolType != ETHER_TYPE_IPv4)
			{
				log.warn(string.Format("processMessageARP unknown protocolType=0x{0:X}", arp.protocolType));
				return;
			}

			if (arp.hardwareAddressLength != MAC_ADDRESS_LENGTH)
			{
				log.warn(string.Format("processMessageARP unknown hardwareAddressLength=0x{0:X}", arp.protocolType));
				return;
			}

			if (arp.protocolAddressLength != IP_ADDRESS_LENGTH)
			{
				log.warn(string.Format("processMessageARP unknown protocolAddressLength=0x{0:X}", arp.protocolType));
				return;
			}

			if (arp.operation != ARP_OPERATION_REQUEST && arp.operation != ARP_OPERATION_REPLY)
			{
				log.warn(string.Format("processMessageARP unknown operation=0x{0:X}", arp.operation));
				return;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("processMessageARP {0}", arp));
			}

			if (arp.targetHardwareAddress.EmptyMacAddress)
			{
				// A gratuitous ARP message has been received.
				// It is used to announce a new IP address.
				// Send back a gratuitous ARP message to announce ourself.
				sendGratuitousARP();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendGratuitousARP() throws java.io.EOFException
		private void sendGratuitousARP()
		{
			EtherFrame frame = new EtherFrame();
			frame.dstMac = new pspNetMacAddress(ANY_MAC_ADDRESS);
			frame.srcMac = MacAddress;
			frame.type = ETHER_TYPE_ARP;

			ARP arp = new ARP();
			arp.hardwareType = HARDWARE_TYPE_ETHERNET;
			arp.protocolType = ETHER_TYPE_IPv4;
			arp.hardwareAddressLength = MAC_ADDRESS_LENGTH;
			arp.protocolAddressLength = IP_ADDRESS_LENGTH;
			arp.operation = ARP_OPERATION_REQUEST;
			arp.senderHardwareAddress = MacAddress;
			arp.senderProtocolAddress = IpAddress;
			// Set the target hardware address to 00:00:00:00:00:00
			arp.targetHardwareAddress = new pspNetMacAddress();
			arp.targetProtocolAddress = IpAddress;

			NetPacket packet = new NetPacket(EtherFrame.sizeOf() + arp.sizeOf());
			frame.write(packet);
			arp.write(packet);

			sendPacket(packet);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void processMessageDatagram(pspsharp.network.protocols.NetPacket packet, pspsharp.network.protocols.EtherFrame frame) throws java.io.EOFException
		private void processMessageDatagram(NetPacket packet, EtherFrame frame)
		{
			IPv4 ipv4 = new IPv4();
			ipv4.read(packet);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("processMessageDatagram IPv4 {0}", ipv4));
			}

			switch (ipv4.protocol)
			{
				case IPv4_PROTOCOL_ICMP:
					processMessageDatagramICMP(packet, frame, ipv4);
					break;
				case IPv4_PROTOCOL_TCP:
					processMessageTCP(packet, frame, ipv4);
					break;
				case IPv4_PROTOCOL_UDP:
					processMessageUDP(packet, frame, ipv4);
					break;
				default:
					log.warn(string.Format("processMessageDatagram unknown protocol {0:D}", ipv4.protocol));
					break;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void processMessageUDP(pspsharp.network.protocols.NetPacket packet, pspsharp.network.protocols.EtherFrame frame, pspsharp.network.protocols.IPv4 ipv4) throws java.io.EOFException
		private void processMessageUDP(NetPacket packet, EtherFrame frame, IPv4 ipv4)
		{
			UDP udp = new UDP();
			udp.read(packet);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("processMessageUDP {0}", udp));
			}

			switch (udp.destinationPort)
			{
				case UDP_PORT_DNS:
					processMessageDNS(packet, frame, ipv4, udp);
					break;
				case UDP.UDP_PORT_DHCP_SERVER:
					processMessageDHCP(packet, frame, ipv4, udp);
					break;
				default:
					log.warn(string.Format("processMessageUDP unknown destination port 0x{0:X}", udp.destinationPort));
					break;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void processMessageDNS(pspsharp.network.protocols.NetPacket packet, pspsharp.network.protocols.EtherFrame frame, pspsharp.network.protocols.IPv4 ipv4, pspsharp.network.protocols.UDP udp) throws java.io.EOFException
		private void processMessageDNS(NetPacket packet, EtherFrame frame, IPv4 ipv4, UDP udp)
		{
			DNS dns = new DNS();
			dns.read(packet);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("processMessageDNS {0}", dns));
			}

			if (!dns.isResponseFlag && dns.questionCount == 1)
			{
				DNS.DNSRecord question = dns.questions[0];
				string hostName = question.recordName;

				DNS answerDns = new DNS(dns);
				try
				{
					InetAddress inetAddress = InetAddress.getByName(hostName);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("DNS response '{0}'={1}", hostName, inetAddress));
					}

					DNS.DNSAnswerRecord answer = new DNS.DNSAnswerRecord();
					answer.recordName = hostName;
					answer.recordClass = question.recordClass;
					answer.recordType = question.recordType;
					answer.data = inetAddress.Address;
					answer.dataLength = answer.data.Length;
					answerDns.responseCode = DNS_RESPONSE_CODE_NO_ERROR;
					answerDns.answerRecordCount = 1;
					answerDns.answerRecords = new DNS.DNSAnswerRecord[] {answer};
				}
				catch (UnknownHostException e)
				{
					answerDns.responseCode = DNS_RESPONSE_CODE_NAME_ERROR;
					if (log.DebugEnabled)
					{
						log.debug(string.Format("processMessageDNS unknown host '{0}'({1})", hostName, e.ToString()));
					}
				}

				answerDns.isResponseFlag = true;

				EtherFrame answerFrame = new EtherFrame(frame);
				answerFrame.swapSourceAndDestination();

				IPv4 answerIPv4 = new IPv4(ipv4);
				answerIPv4.swapSourceAndDestination();
				answerIPv4.timeToLive--; // When a packet arrives at a router, the router decreases the TTL field.

				UDP answerUdp = new UDP(udp);
				answerUdp.swapSourceAndDestination();

				// Update lengths and checksums
				answerUdp.length = answerUdp.sizeOf() + answerDns.sizeOf();
				answerUdp.computeChecksum();
				answerIPv4.totalLength = answerIPv4.sizeOf() + answerUdp.length;
				answerIPv4.computeChecksum();

				// Write the different headers in sequence
				NetPacket answerPacket = new NetPacket(BUFFER_SIZE);
				answerFrame.write(answerPacket);
				answerIPv4.write(answerPacket);
				answerUdp.write(answerPacket);
				answerDns.write(answerPacket);

				sendPacket(answerPacket);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void processMessageDatagramICMP(pspsharp.network.protocols.NetPacket packet, pspsharp.network.protocols.EtherFrame frame, pspsharp.network.protocols.IPv4 ipv4) throws java.io.EOFException
		private void processMessageDatagramICMP(NetPacket packet, EtherFrame frame, IPv4 ipv4)
		{
			ICMP icmp = new ICMP();
			icmp.read(packet);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("processMessageDatagramICMP {0}", icmp));
			}

			switch (icmp.type)
			{
				case ICMP_CONTROL_ECHO_REQUEST:
					sendICMPEchoResponse(packet, frame, ipv4, icmp);
					break;
				default:
					log.warn(string.Format("processMessageDatagramICMP unknown type=0x{0:X}, code=0x{1:X}", icmp.type, icmp.code));
					break;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendICMPEchoResponse(pspsharp.network.protocols.NetPacket packet, pspsharp.network.protocols.EtherFrame frame, pspsharp.network.protocols.IPv4 ipv4, pspsharp.network.protocols.ICMP icmp) throws java.io.EOFException
		private void sendICMPEchoResponse(NetPacket packet, EtherFrame frame, IPv4 ipv4, ICMP icmp)
		{
			bool reachable = false;
			try
			{
				InetAddress inetAddress = InetAddress.getByAddress(ipv4.destinationIPAddress);
				// Timeout after 1 second
				reachable = inetAddress.isReachable(null, ipv4.timeToLive, 1000);
			}
			catch (UnknownHostException)
			{
			}
			catch (IOException)
			{
			}

			if (reachable)
			{
				// See https://en.wikipedia.org/wiki/Ping_(networking_utility)
				EtherFrame answerFrame = new EtherFrame(frame);
				answerFrame.swapSourceAndDestination();

				IPv4 answerIPv4 = new IPv4(ipv4);
				answerIPv4.swapSourceAndDestination();
				answerIPv4.timeToLive--; // When a packet arrives at a router, the router decreases the TTL field.

				ICMP answerIcmp = new ICMP(icmp);
				answerIcmp.type = ICMP.ICMP_CONTROL_ECHO_REPLY;
				answerIcmp.computeChecksum();

				answerIPv4.totalLength = answerIPv4.sizeOf() + answerIcmp.sizeOf();
				answerIPv4.computeChecksum();

				// Write the different headers in sequence
				NetPacket answerPacket = new NetPacket(BUFFER_SIZE);
				answerFrame.write(answerPacket);
				answerIPv4.write(answerPacket);
				answerIcmp.write(answerPacket);

				sendPacket(answerPacket);
			}
		}

		private TcpConnectionState getTcpConnectionState(IPv4 ipv4, TCP tcp)
		{
			foreach (TcpConnectionState tcpConnectionState in tcpConnectionStates)
			{
				if (tcp.sourcePort == tcpConnectionState.sourcePort && tcp.destinationPort == tcpConnectionState.destinationPort && Arrays.Equals(ipv4.sourceIPAddress, tcpConnectionState.sourceIPAddress) && Arrays.Equals(ipv4.destinationIPAddress, tcpConnectionState.destinationIPAddress))
				{
					return tcpConnectionState;
				}
			}

			// Not found
			return null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void processMessageTCP(pspsharp.network.protocols.NetPacket packet, pspsharp.network.protocols.EtherFrame frame, pspsharp.network.protocols.IPv4 ipv4) throws java.io.EOFException
		private void processMessageTCP(NetPacket packet, EtherFrame frame, IPv4 ipv4)
		{
			TCP tcp = new TCP();
			tcp.read(packet);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("processMessageTCP {0}", tcp));
			}

			TcpConnectionState tcpConnectionState = getTcpConnectionState(ipv4, tcp);
			if (tcp.flagSYN)
			{
				if (tcpConnectionState != null)
				{
					if (!tcpConnectionState.pendingConnection)
					{
						log.error(string.Format("processMessageTCP SYN received but connection already exists: {0}", tcpConnectionState));
						return;
					}

					if (log.DebugEnabled)
					{
						log.debug(string.Format("processMessageTCP SYN received for a connection still pending ({0}), retrying the connection", tcpConnectionState));
					}

					try
					{
						tcpConnectionState.close();
					}
					catch (IOException e)
					{
						if (log.DebugEnabled)
						{
							log.debug("error while closing connection", e);
						}
					}
					tcpConnectionStates.Remove(tcpConnectionState);
				}

				tcpConnectionState = new TcpConnectionState();
				tcpConnectionState.sourceMacAddress = frame.srcMac;
				tcpConnectionState.destinationMacAddress = frame.dstMac;
				tcpConnectionState.sourceIPAddress = ipv4.sourceIPAddress;
				tcpConnectionState.destinationIPAddress = ipv4.destinationIPAddress;
				tcpConnectionState.sourcePort = tcp.sourcePort;
				tcpConnectionState.destinationPort = tcp.destinationPort;
				tcpConnectionState.sourceSequenceNumber = tcp.sequenceNumber + tcp.data.Length;
				tcpConnectionState.destinationSequenceNumber = random.Next();
				tcpConnectionState.pendingConnection = true;
				tcpConnectionStates.Add(tcpConnectionState);
			}
			else if (tcp.flagACK)
			{
				if (tcpConnectionState == null)
				{
					// Acknowledge to an unknown connection, ignore
					if (log.DebugEnabled)
					{
						log.debug(string.Format("processMessageTCP ACK received for unknown connection: {0}", tcp));
					}
					return;
				}

				try
				{
					if (tcp.flagFIN)
					{
						tcpConnectionState.sourceSequenceNumber += tcp.data.Length;
						tcpConnectionState.sourceSequenceNumber++;
						sendAcknowledgeTCP(tcpConnectionState, false);
					}
					else if (tcp.flagPSH)
					{
						// Acknowledge the reception of the data
						tcpConnectionState.sourceSequenceNumber += tcp.data.Length;
						sendAcknowledgeTCP(tcpConnectionState, false);

						// Queue the received data for the destination
						tcpConnectionState.addPendingWriteData(tcp.data);
					}
				}
				catch (IOException e)
				{
					log.error("processMessageTCP", e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendAcknowledgeTCP(TcpConnectionState tcpConnectionState, boolean flagSYN) throws java.io.EOFException
		private void sendAcknowledgeTCP(TcpConnectionState tcpConnectionState, bool flagSYN)
		{
			EtherFrame answerFrame = new EtherFrame();
			answerFrame.srcMac = tcpConnectionState.destinationMacAddress;
			answerFrame.dstMac = tcpConnectionState.sourceMacAddress;
			answerFrame.type = ETHER_TYPE_IPv4;

			IPv4 answerIPv4 = new IPv4();
			answerIPv4.protocol = IPv4_PROTOCOL_TCP;
			answerIPv4.sourceIPAddress = tcpConnectionState.destinationIPAddress;
			answerIPv4.destinationIPAddress = tcpConnectionState.sourceIPAddress;

			TCP answerTcp = new TCP();
			answerTcp.sourcePort = tcpConnectionState.destinationPort;
			answerTcp.destinationPort = tcpConnectionState.sourcePort;
			answerTcp.sequenceNumber = tcpConnectionState.destinationSequenceNumber;
			answerTcp.acknowledgmentNumber = tcpConnectionState.sourceSequenceNumber;
			answerTcp.flagACK = true;
			answerTcp.flagSYN = flagSYN;

			// Update lengths and checksums
			answerTcp.computeChecksum(answerIPv4);
			answerIPv4.totalLength = answerIPv4.sizeOf() + answerTcp.sizeOf();
			answerIPv4.computeChecksum();

			// Write the different headers in sequence
			NetPacket answerPacket = new NetPacket(BUFFER_SIZE);
			answerFrame.write(answerPacket);
			answerIPv4.write(answerPacket);
			answerTcp.write(answerPacket);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sendAcknowledgeTCP frame={0}", answerFrame));
				log.debug(string.Format("sendAcknowledgeTCP IPv4={0}", answerIPv4));
				log.debug(string.Format("sendAcknowledgeTCP TCP={0}", answerTcp));
			}

			sendPacket(answerPacket);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendTcpData(TcpConnectionState tcpConnectionState, byte[] data) throws java.io.EOFException
		private void sendTcpData(TcpConnectionState tcpConnectionState, sbyte[] data)
		{
			EtherFrame answerFrame = new EtherFrame();
			answerFrame.srcMac = tcpConnectionState.destinationMacAddress;
			answerFrame.dstMac = tcpConnectionState.sourceMacAddress;
			answerFrame.type = ETHER_TYPE_IPv4;

			IPv4 answerIPv4 = new IPv4();
			answerIPv4.protocol = IPv4_PROTOCOL_TCP;
			answerIPv4.sourceIPAddress = tcpConnectionState.destinationIPAddress;
			answerIPv4.destinationIPAddress = tcpConnectionState.sourceIPAddress;

			TCP answerTcp = new TCP();
			answerTcp.sourcePort = tcpConnectionState.destinationPort;
			answerTcp.destinationPort = tcpConnectionState.sourcePort;
			answerTcp.sequenceNumber = tcpConnectionState.destinationSequenceNumber;
			answerTcp.acknowledgmentNumber = tcpConnectionState.sourceSequenceNumber;
			answerTcp.flagACK = true;
			answerTcp.flagPSH = true;
			tcpConnectionState.destinationSequenceNumber += data.Length;
			answerTcp.data = data;

			// Update lengths and checksums
			answerTcp.computeChecksum(answerIPv4);
			answerIPv4.totalLength = answerIPv4.sizeOf() + answerTcp.sizeOf();
			answerIPv4.computeChecksum();

			// Write the different headers in sequence
			NetPacket answerPacket = new NetPacket(BUFFER_SIZE);
			answerFrame.write(answerPacket);
			answerIPv4.write(answerPacket);
			answerTcp.write(answerPacket);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sendTcpData frame={0}", answerFrame));
				log.debug(string.Format("sendTcpData IPv4={0}", answerIPv4));
				log.debug(string.Format("sendTcpData TCP={0}", answerTcp));
			}

			sendPacket(answerPacket);
		}

		private bool receiveTcpMessages()
		{
			bool received = false;
			IList<TcpConnectionState> tcpConnectionStatesToBeDeleted = new LinkedList<TcpConnectionState>();

			foreach (TcpConnectionState tcpConnectionState in tcpConnectionStates)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("receiveTcpMessages polling {0}", tcpConnectionState));
				}

				if (tcpConnectionState.pendingConnection)
				{
					try
					{
						tcpConnectionState.connect();
						SocketChannel socketChannel = tcpConnectionState.socketChannel;
						if (socketChannel != null && socketChannel.finishConnect())
						{
							tcpConnectionState.sourceSequenceNumber++;
							// Send SYN-ACK acknowledge
							sendAcknowledgeTCP(tcpConnectionState, true);
							tcpConnectionState.destinationSequenceNumber++;
							tcpConnectionState.pendingConnection = false;
						}
					}
					catch (IOException e)
					{
						// connect failed, do not send any TCP SYN-ACK, forget the connection state
						tcpConnectionStatesToBeDeleted.Add(tcpConnectionState);
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Pending TCP connection {0} failed: {1}", tcpConnectionState, e.ToString()));
						}
					}
				}

				try
				{
					if (!tcpConnectionState.pendingConnection)
					{
						// Write any pending data
						sbyte[] pendingWriteData = tcpConnectionState.pendingWriteData;
						if (pendingWriteData != null)
						{
							tcpConnectionState.pendingWriteData = null;

							if (log.DebugEnabled)
							{
								log.debug(string.Format("receiveTcpMessages sending pending write data: {0}", Utilities.getMemoryDump(pendingWriteData)));
							}
							tcpConnectionState.write(pendingWriteData);
						}

						// Receive any available data
						sbyte[] receivedData = tcpConnectionState.read();
						if (receivedData != null)
						{
							received = true;
							sendTcpData(tcpConnectionState, receivedData);
						}
					}
				}
				catch (IOException e)
				{
					// Ignore exceptions
					log.error("receiveTcpMessages", e);
				}
			}

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
			tcpConnectionStates.removeAll(tcpConnectionStatesToBeDeleted);

			return received;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendDHCPReply(pspsharp.network.protocols.EtherFrame frame, pspsharp.network.protocols.IPv4 ipv4, pspsharp.network.protocols.UDP udp, pspsharp.network.protocols.DHCP dhcp, int messageType) throws java.io.EOFException
		private void sendDHCPReply(EtherFrame frame, IPv4 ipv4, UDP udp, DHCP dhcp, int messageType)
		{
			// Send back a DHCP offer message
			EtherFrame answerFrame = new EtherFrame(frame);
			answerFrame.swapSourceAndDestination();
			answerFrame.srcMac = MacAddress;

			IPv4 answerIPv4 = new IPv4(ipv4);
			answerIPv4.sourceIPAddress = IpAddress;
			answerIPv4.timeToLive--; // When a packet arrives at a router, the router decreases the TTL field.

			UDP answerUdp = new UDP(udp);
			answerUdp.swapSourceAndDestination();

			DHCP answerDhcp = new DHCP(dhcp);
			answerDhcp.opcode = DHCP_BOOT_REPLY;
			answerDhcp.yourIPAddress = LocalIpAddress;
			answerDhcp.nextServerIPAddress = IpAddress;

			answerDhcp.clearOptions();
			// The DHCP message type
			answerDhcp.addOption(new DHCP.DHCPOption(DHCP.DHCP_OPTION_MESSAGE_TYPE, (sbyte) messageType));
			// The subnet mask
			answerDhcp.addOption(new DHCP.DHCPOption(DHCP.DHCP_OPTION_SUBNET_MASK, getIpAddress(sceNetApctl.SubnetMaskInt)));
			// The only router is myself
			answerDhcp.addOption(new DHCP.DHCPOption(DHCP.DHCP_OPTION_ROUTER, IpAddress));
			// The IP address lease time is forever
			answerDhcp.addOption(new DHCP.DHCPOption(DHCP.DHCP_OPTION_IP_ADDRESS_LEASE_TIME, int.MaxValue));
			// The DHCP server identification is myself
			answerDhcp.addOption(new DHCP.DHCPOption(DHCP.DHCP_OPTION_SERVER_IDENTIFIER, IpAddress));
			// The only DNS server is myself
			answerDhcp.addOption(new DHCP.DHCPOption(DHCP.DHCP_OPTION_DNS, IpAddress));
			// The broadcast address
			answerDhcp.addOption(new DHCP.DHCPOption(DHCP.DHCP_OPTION_BROADCAST_ADDRESS, DHCP.broadcastIPAddress));

			// Update lengths and checksums
			answerUdp.length = answerUdp.sizeOf() + answerDhcp.sizeOf();
			answerUdp.computeChecksum();
			answerIPv4.totalLength = answerIPv4.sizeOf() + answerUdp.length;
			answerIPv4.computeChecksum();

			// Write the different headers in sequence
			NetPacket answerPacket = new NetPacket(BUFFER_SIZE);
			answerFrame.write(answerPacket);
			answerIPv4.write(answerPacket);
			answerUdp.write(answerPacket);
			answerDhcp.write(answerPacket);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sendDHCPReply frame={0}", answerFrame));
				log.debug(string.Format("sendDHCPReply IPv4={0}", answerIPv4));
				log.debug(string.Format("sendDHCPReply UDP={0}", answerUdp));
				log.debug(string.Format("sendDHCPReply messageType={0:D}, DHCP={1}", messageType, answerDhcp));
			}

			sendPacket(answerPacket);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void processMessageDHCP(pspsharp.network.protocols.NetPacket packet, pspsharp.network.protocols.EtherFrame frame, pspsharp.network.protocols.IPv4 ipv4, pspsharp.network.protocols.UDP udp) throws java.io.EOFException
		private void processMessageDHCP(NetPacket packet, EtherFrame frame, IPv4 ipv4, UDP udp)
		{
			DHCP dhcp = new DHCP();
			dhcp.read(packet);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("processMessageDHCP {0}", dhcp));
			}

			if (dhcp.isDiscovery(udp, ipv4))
			{
				// Send back a DHCP offset message
				sendDHCPReply(frame, ipv4, udp, dhcp, DHCP.DHCP_OPTION_MESSAGE_TYPE_DHCPOFFER);
			}
			else if (dhcp.isRequest(udp, ipv4, LocalIpAddress))
			{
				// Send back a DHCP acknowledgment message
				sendDHCPReply(frame, ipv4, udp, dhcp, DHCP.DHCP_OPTION_MESSAGE_TYPE_DHCPACK);
			}
			else
			{
				log.warn(string.Format("Unknown DHCP request {0}", dhcp));
			}
		}
	}

}