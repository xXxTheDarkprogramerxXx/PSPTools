using System.Collections.Generic;
using System.Text;
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
namespace pspsharp.HLE.modules
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ERRNO_BASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocctl.fillNextPointersInLinkedList;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;


	using Logger = org.apache.log4j.Logger;

	using RawSocket = com.savarese.rocksaw.net.RawSocket;

	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceNetInetTcpcbstat = pspsharp.HLE.kernel.types.SceNetInetTcpcbstat;
	using pspAbstractMemoryMappedStructure = pspsharp.HLE.kernel.types.pspAbstractMemoryMappedStructure;
	using pspNetSockAddrInternet = pspsharp.HLE.kernel.types.pspNetSockAddrInternet;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using RawChannel = pspsharp.network.RawChannel;
	using RawSelector = pspsharp.network.RawSelector;
	using HTTPConfiguration = pspsharp.remote.HTTPConfiguration;
	using HttpServerConfiguration = pspsharp.remote.HTTPConfiguration.HttpServerConfiguration;
	using HTTPServer = pspsharp.remote.HTTPServer;
	using AbstractStringSettingsListener = pspsharp.settings.AbstractStringSettingsListener;
	using Settings = pspsharp.settings.Settings;
	using Utilities = pspsharp.util.Utilities;

	public class sceNetInet : HLEModule
	{
		public static Logger log = Modules.getLogger("sceNetInet");

		public const int AF_INET = 2; // Address familiy internet

		public const int SOCK_STREAM = 1; // Stream socket
		public const int SOCK_DGRAM = 2; // Datagram socket
		public const int SOCK_RAW = 3; // Raw socket
		public const int SOCK_STREAM_UNKNOWN_10 = 10; // Looks like a SOCK_STREAM, but specifics unknown
		public const int SOCK_DGRAM_UNKNOWN_6 = 6; // Looks like a SOCK_DGRAM, but specifics unknown
		private static readonly string[] socketTypeNames = new string[] {"Unknown0", "SOCK_STREAM", "SOCK_DGRAM", "SOCK_RAW", "Unknown4", "Unknown5", "SOCK_DGRAM_UNKNOWN_6", "Unknown7", "Unknown8", "Unknown9", "SOCK_STREAM_UNKNOWN_10"};

		public const int SOL_SOCKET = 0xFFFF; // Socket level
		public const int INADDR_ANY = 0x00000000; // wildcard/any IP address
		public const int INADDR_BROADCAST = unchecked((int)0xFFFFFFFF); // Broadcast address

		public static readonly int EAGAIN = SceKernelErrors.ERROR_ERRNO_RESOURCE_UNAVAILABLE & 0x0000FFFF;
		public static readonly int EWOULDBLOCK = EAGAIN; // EWOULDBLOCK == EAGAIN
		public static readonly int EINPROGRESS = SceKernelErrors.ERROR_ERRNO_IN_PROGRESS & 0x0000FFFF;
		public static readonly int ENOTCONN = SceKernelErrors.ERROR_ERRNO_NOT_CONNECTED & 0x0000FFFF;
		public static readonly int ECLOSED = SceKernelErrors.ERROR_ERRNO_CLOSED & 0x0000FFFF;
		public static readonly int EIO = SceKernelErrors.ERROR_ERRNO_IO_ERROR & 0x0000FFFF;
		public static readonly int EISCONN = SceKernelErrors.ERROR_ERRNO_IS_ALREADY_CONNECTED & 0x0000FFFF;
		public static readonly int EALREADY = SceKernelErrors.ERROR_ERRNO_ALREADY & 0x0000FFFF;
		public static readonly int EADDRNOTAVAIL = SceKernelErrors.ERROR_ERRNO_ADDRESS_NOT_AVAILABLE & 0x0000FFFF;

		// Types of socket shutdown ("how" parameter)
		public const int SHUT_RD = 0; // Disallow further receives
		public const int SHUT_WR = 1; // Disallow further sends
		public const int SHUT_RDWR = 2; // Disallow further sends/receives

		// Socket options
		public const int SO_DEBUG = 0x0001; // turn on debugging info recording
		public const int SO_ACCEPTCONN = 0x0002; // socket has had listen()
		public const int SO_REUSEADDR = 0x0004; // allow local address reuse
		public const int SO_KEEPALIVE = 0x0008; // keep connections alive
		public const int SO_DONTROUTE = 0x0010; // just use interface addresses
		public const int SO_BROADCAST = 0x0020; // permit sending of broadcast msgs
		public const int SO_USELOOPBACK = 0x0040; // bypass hardware when possible
		public const int SO_LINGER = 0x0080; // linger on close if data present
		public const int SO_OOBINLINE = 0x0100; // leave received OOB data in line
		public const int SO_REUSEPORT = 0x0200; // allow local address & port reuse
		public const int SO_TIMESTAMP = 0x0400; // timestamp received dgram traffic
		public const int SO_ONESBCAST = 0x0800; // allow broadcast to 255.255.255.255
		public const int SO_SNDBUF = 0x1001; // send buffer size
		public const int SO_RCVBUF = 0x1002; // receive buffer size
		public const int SO_SNDLOWAT = 0x1003; // send low-water mark
		public const int SO_RCVLOWAT = 0x1004; // receive low-water mark
		public const int SO_SNDTIMEO = 0x1005; // send timeout
		public const int SO_RCVTIMEO = 0x1006; // receive timeout
		public const int SO_ERROR = 0x1007; // get error status and clear
		public const int SO_TYPE = 0x1008; // get socket type
		public const int SO_OVERFLOWED = 0x1009; // datagrams: return packets dropped
		public const int SO_NONBLOCK = 0x1009; // non-blocking I/O

		// Bitmasks for sceNetInetPoll()
		public const int POLLIN = 0x0001;
		public const int POLLPRI = 0x0002;
		public const int POLLOUT = 0x0004;
		public const int POLLERR = 0x0008;
		public const int POLLHUP = 0x0010;
		public const int POLLNVAL = 0x0020;
		public const int POLLRDNORM = 0x0040;
		public const int POLLRDBAND = 0x0080;
		public const int POLLWRBAND = 0x0100;

		// Infinite timeout for scenetInetPoll()
		public const int POLL_INFTIM = -1;

		// Polling period (micro seconds) for blocking operations
		protected internal const int BLOCKED_OPERATION_POLLING_MICROS = 10000;

		protected internal static readonly int readSelectionKeyOperations = SelectionKey.OP_READ | SelectionKey.OP_ACCEPT;
		protected internal static readonly int writeSelectionKeyOperations = SelectionKey.OP_WRITE;

		// MSG flag options for sceNetInetRecvfrom and sceNetInetSendto (from <sys/socket.h>)
		public const int MSG_OOB = 0x1; // Requests out-of-band data.
		public const int MSG_PEEK = 0x2; // Peeks at an incoming message.
		public const int MSG_DONTROUTE = 0x4; // Sends data without routing tables.
		public const int MSG_EOR = 0x8; // Terminates a record.
		public const int MSG_TRUNC = 0x10; // Truncates data before receiving it.
		public const int MSG_CTRUNC = 0x20; // Truncates control data before receiving it.
		public const int MSG_WAITALL = 0x40; // Waits until all data can be returned (blocking).
		public const int MSG_DONTWAIT = 0x80; // Doesn't wait until all data can be returned (non-blocking).
		public const int MSG_BCAST = 0x100; // Message received by link-level broadcast.
		public const int MSG_MCAST = 0x200; // Message received by link-level multicast.

		// TCP FSM state definitions. Per RFC793, September, 1981.
		public const int TCPS_CLOSED = 0; // closed
		public const int TCPS_LISTEN = 1; // listening for connection
		public const int TCPS_SYN_SENT = 2; // active, have sent syn
		public const int TCPS_SYN_RECEIVED = 3; // have send and received syn
		public const int TCPS_ESTABLISHED = 4; // established
		public const int TCPS_CLOSE_WAIT = 5; // rcvd fin, waiting for close
		public const int TCPS_FIN_WAIT_1 = 6; // have closed, sent fin
		public const int TCPS_CLOSING = 7; // closed xchd FIN; await FIN ACK
		public const int TCPS_LAST_ACK = 8; // had fin and close; await FIN ACK
		public const int TCPS_FIN_WAIT_2 = 9; // have closed, fin is acked
		public const int TCPS_TIME_WAIT = 10; // in 2*msl quiet wait after close

		private static InetAddress[] broadcastAddresses;

		private class BroadcastAddressSettingsListener : AbstractStringSettingsListener
		{
			protected internal override void settingsValueChanged(string value)
			{
				// Force a new evaluation of broadcasrAddresses in getBroadcastInetSocketAddress()
				broadcastAddresses = null;
			}
		}

		private class AsyncStartThread : Thread
		{
			private readonly sceNetInet outerInstance;

			public AsyncStartThread(sceNetInet outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void run()
			{
				// These DNS requests take some time to complete (1-2 seconds),
				// this is why we execute them in a separate thread.
				foreach (HTTPConfiguration.HttpServerConfiguration doProxyServer in HTTPConfiguration.doProxyServers)
				{
					try
					{
						InetAddress[] inetAddresses = (InetAddress[]) InetAddress.getAllByName(doProxyServer.serverName);
						outerInstance.doProxyInetAddresses = Utilities.merge(outerInstance.doProxyInetAddresses, inetAddresses);

						if (log.DebugEnabled)
						{
							if (inetAddresses == null)
							{
								log.debug(string.Format("doProxyInetAddress {0}: IP address cannot be resolved", doProxyServer.serverName));
							}
							else
							{
								foreach (InetAddress inetAddress in inetAddresses)
								{
									log.debug(string.Format("doProxyInetAddress {0}: {1}", doProxyServer.serverName, inetAddress));
								}
							}
						}
					}
					catch (UnknownHostException e)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceNetInet cannot resolve '{0}': {1}", doProxyServer, e.ToString()));
						}
					}
				}
			}
		}

		public virtual void addProxyInetAddress(string serverName, InetAddress inetAddress)
		{
			if (inetAddress == null || string.ReferenceEquals(serverName, null))
			{
				return;
			}

			bool doProxy = false;
			foreach (HTTPConfiguration.HttpServerConfiguration doProxyServer in HTTPConfiguration.doProxyServers)
			{
				if (doProxyServer.serverName.Equals(serverName))
				{
					doProxy = true;
					break;
				}
			}

			if (!doProxy)
			{
				// We do not proxy this server name
				return;
			}

			if (doProxyInetAddresses != null)
			{
				foreach (InetAddress proxyInetAddress in doProxyInetAddresses)
				{
					if (proxyInetAddress.Equals(inetAddress))
					{
						// The InetAddress is already present
						return;
					}
				}
			}

			doProxyInetAddresses = Utilities.add(doProxyInetAddresses, inetAddress);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static java.net.InetSocketAddress[] getBroadcastInetSocketAddress(int port) throws java.net.UnknownHostException
		public static InetSocketAddress[] getBroadcastInetSocketAddress(int port)
		{
			if (broadcastAddresses == null)
			{
				string broadcastAddressNames = Settings.Instance.readString("network.broadcastAddress");
				if (!string.ReferenceEquals(broadcastAddressNames, null) && broadcastAddressNames.Length > 0)
				{
					string[] addressNames = broadcastAddressNames.Split(" *[,;] *", true);
					List<InetAddress> addresses = new List<InetAddress>();
					for (int i = 0; i < addressNames.Length; i++)
					{
						try
						{
							InetAddress address = InetAddress.getByName(addressNames[i]);
							addresses.Add(address);
						}
						catch (Exception e)
						{
							log.error(string.Format("Error resolving the broadcast address '{0}' from the Settings file", addressNames[i]), e);
						}
					}

					if (addresses.Count > 0)
					{
						broadcastAddresses = addresses.ToArray();
					}
				}

				if (broadcastAddresses == null)
				{
					// When SO_ONESBCAST is not enabled, map the broadcast address
					// to the broadcast address from the network of the local IP address.
					// E.g.
					//  - localHostIP: A.B.C.D
					//  - subnetMask: 255.255.255.0
					// -> localBroadcastIP: A.B.C.255
					InetAddress localInetAddress = InetAddress.getByName(sceNetApctl.LocalHostIP);
					int localAddress = bytesToInternetAddress(localInetAddress.Address);
					int subnetMask = Integer.reverseBytes(sceNetApctl.SubnetMaskInt);
					int localBroadcastAddressInt = localAddress & subnetMask;
					localBroadcastAddressInt |= INADDR_BROADCAST & ~subnetMask;

					broadcastAddresses = new InetAddress[1];
					broadcastAddresses[0] = InetAddress.getByAddress(internetAddressToBytes(localBroadcastAddressInt));
				}

				if (log.DebugEnabled)
				{
					for (int i = 0; i < broadcastAddresses.Length; i++)
					{
						log.debug(string.Format("Using the following broadcast address#{0:D}: {1}", i + 1, broadcastAddresses[i].HostAddress));
					}
				}
			}

			InetSocketAddress[] socketAddresses = new InetSocketAddress[broadcastAddresses.Length];
			for (int i = 0; i < socketAddresses.Length; i++)
			{
				socketAddresses[i] = new InetSocketAddress(broadcastAddresses[i], port);
			}

			return socketAddresses;
		}

		protected internal abstract class BlockingState : IAction
		{
			public pspInetSocket inetSocket;
			public int threadId;
			public bool threadBlocked;
			public long timeout; // microseconds
			public long start; // Clock.microTime
			internal bool insideExecute;

			public BlockingState(pspInetSocket inetSocket, long timeout)
			{
				this.inetSocket = inetSocket;
				threadId = Modules.ThreadManForUserModule.CurrentThreadID;
				threadBlocked = false;
				start = Emulator.Clock.microTime();
				this.timeout = timeout;
			}

			public virtual bool Timeout
			{
				get
				{
					long now = Emulator.Clock.microTime();
					return now >= start + timeout;
				}
			}

			public virtual void execute()
			{
				// Avoid executing the blocking state while already processing it.
				// E.g. when the thread is unblocked by executeBlockingState(),
				// this action is called again
				if (!insideExecute)
				{
					insideExecute = true;
					executeBlockingState();
					insideExecute = false;
				}
			}

			protected internal abstract void executeBlockingState();
		}

		protected internal class BlockingAcceptState : BlockingState
		{
			public pspNetSockAddrInternet acceptAddr;

			public BlockingAcceptState(pspInetSocket inetSocket, pspNetSockAddrInternet acceptAddr) : base(inetSocket, pspInetSocket.NO_TIMEOUT)
			{
				this.acceptAddr = acceptAddr;
			}

			protected internal override void executeBlockingState()
			{
				inetSocket.blockedAccept(this);
			}
		}

		protected internal class BlockingPollState : BlockingState
		{
			public Selector selector;
			public pspInetPollFd[] pollFds;

			public BlockingPollState(Selector selector, pspInetPollFd[] pollFds, long timeout) : base(null, timeout)
			{
				this.selector = selector;
				this.pollFds = pollFds;
			}

			protected internal override void executeBlockingState()
			{
				Modules.sceNetInetModule.blockedPoll(this);
			}
		}

		protected internal class BlockingSelectState : BlockingState
		{
			public Selector selector;
			public RawSelector rawSelector;
			public int numberSockets;
			public TPointer readSocketsAddr;
			public TPointer writeSocketsAddr;
			public TPointer outOfBandSocketsAddr;
			public int count;

			public BlockingSelectState(Selector selector, RawSelector rawSelector, int numberSockets, TPointer readSocketsAddr, TPointer writeSocketsAddr, TPointer outOfBandSocketsAddr, long timeout, int count) : base(null, timeout)
			{
				this.selector = selector;
				this.rawSelector = rawSelector;
				this.numberSockets = numberSockets;
				this.readSocketsAddr = readSocketsAddr;
				this.writeSocketsAddr = writeSocketsAddr;
				this.outOfBandSocketsAddr = outOfBandSocketsAddr;
				this.count = count;
			}

			protected internal override void executeBlockingState()
			{
				Modules.sceNetInetModule.blockedSelect(this);
			}
		}

		protected internal class BlockingReceiveState : BlockingState
		{
			public int buffer;
			public int bufferLength;
			public int flags;
			public int receivedLength;

			public BlockingReceiveState(pspInetSocket inetSocket, int buffer, int bufferLength, int flags, int receivedLength) : base(inetSocket, inetSocket.ReceiveTimeout)
			{
				this.buffer = buffer;
				this.bufferLength = bufferLength;
				this.flags = flags;
				this.receivedLength = receivedLength;
			}

			protected internal override void executeBlockingState()
			{
				inetSocket.blockedRecv(this);
			}
		}

		protected internal class BlockingReceiveFromState : BlockingState
		{
			public int buffer;
			public int bufferLength;
			public int flags;
			public pspNetSockAddrInternet fromAddr;
			public int receivedLength;

			public BlockingReceiveFromState(pspInetSocket inetSocket, int buffer, int bufferLength, int flags, pspNetSockAddrInternet fromAddr, int receivedLength) : base(inetSocket, inetSocket.ReceiveTimeout)
			{
				this.buffer = buffer;
				this.bufferLength = bufferLength;
				this.flags = flags;
				this.fromAddr = fromAddr;
				this.receivedLength = receivedLength;
			}

			protected internal override void executeBlockingState()
			{
				inetSocket.blockedRecvfrom(this);
			}
		}

		protected internal class BlockingSendState : BlockingState
		{
			public int buffer;
			public int bufferLength;
			public int flags;
			public int sentLength;

			public BlockingSendState(pspInetSocket inetSocket, int buffer, int bufferLength, int flags, int sentLength) : base(inetSocket, inetSocket.SendTimeout)
			{
				this.buffer = buffer;
				this.bufferLength = bufferLength;
				this.flags = flags;
				this.sentLength = sentLength;
			}

			protected internal override void executeBlockingState()
			{
				inetSocket.blockedSend(this);
			}
		}

		protected internal class BlockingSendToState : BlockingState
		{
			public int buffer;
			public int bufferLength;
			public int flags;
			public pspNetSockAddrInternet toAddr;
			public int sentLength;

			public BlockingSendToState(pspInetSocket inetSocket, int buffer, int bufferLength, int flags, pspNetSockAddrInternet toAddr, int sentLength) : base(inetSocket, inetSocket.SendTimeout)
			{
				this.buffer = buffer;
				this.bufferLength = bufferLength;
				this.flags = flags;
				this.toAddr = toAddr;
				this.sentLength = sentLength;
			}

			protected internal override void executeBlockingState()
			{
				inetSocket.blockedSendto(this);
			}
		}

		protected internal abstract class pspInetSocket
		{
			private readonly sceNetInet outerInstance;

			public static readonly long NO_TIMEOUT = int.MaxValue * 1000000L;
			public static readonly int NO_TIMEOUT_INT = int.MaxValue;
			internal int uid;
			protected internal bool blocking = true;
			protected internal bool broadcast;
			protected internal bool onesBroadcast;
			protected internal int receiveLowWaterMark = 1;
			protected internal int sendLowWaterMark = 2048;
			protected internal int receiveTimeout = NO_TIMEOUT_INT;
			protected internal int sendTimeout = NO_TIMEOUT_INT;
			protected internal int receiveBufferSize = 0x4000;
			protected internal int sendBufferSize = 0x4000;
			protected internal int error;
			protected internal bool reuseAddress;
			protected internal bool keepAlive;
			protected internal bool lingerEnabled;
			protected internal int linger;
			protected internal bool tcpNoDelay;
			internal pspNetSockAddrInternet localAddr;
			internal pspNetSockAddrInternet remoteAddr;

			public pspInetSocket(sceNetInet outerInstance, int uid)
			{
				this.outerInstance = outerInstance;
				this.uid = uid;
			}

			public virtual int Uid
			{
				get
				{
					return uid;
				}
			}

			public abstract int connect(pspNetSockAddrInternet addr);
			public abstract int bind(pspNetSockAddrInternet addr);
			public abstract int recv(int buffer, int bufferLength, int flags, BlockingReceiveState blockingState);
			public abstract int send(int buffer, int bufferLength, int flags, BlockingSendState blockingState);
			public abstract int recvfrom(int buffer, int bufferLength, int flags, pspNetSockAddrInternet fromAddr, BlockingReceiveFromState blockingState);
			public abstract int sendto(int buffer, int bufferLength, int flags, pspNetSockAddrInternet toAddr, BlockingSendToState blockingState);
			public abstract int close();
			public abstract SelectableChannel SelectableChannel {get;}
			public abstract bool Valid {get;}
			public abstract int getSockname(pspNetSockAddrInternet sockAddrInternet);
			public abstract int getPeername(pspNetSockAddrInternet sockAddrInternet);
			public abstract int shutdown(int how);
			public abstract int listen(int backlog);
			public abstract int accept(pspNetSockAddrInternet sockAddrInternet, BlockingAcceptState blockingState);
			public abstract bool finishConnect();

			public virtual int setBlocking(bool blocking)
			{
				this.blocking = blocking;

				return 0;
			}

			public virtual bool Blocking
			{
				get
				{
					return blocking;
				}
			}

			public virtual bool isBlocking(int flags)
			{
				bool blocking;

				// Flag MSG_DONTWAIT set: the IO operation is non-blocking
				if ((flags & MSG_DONTWAIT) != 0)
				{
					blocking = false;
				}
				else
				{
					blocking = Blocking;
				}

				if (log.DebugEnabled)
				{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("isBlocking(0x%X)=%b", flags, blocking));
					log.debug(string.Format("isBlocking(0x%X)=%b", flags, blocking));
				}

				return blocking;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected java.net.SocketAddress getSocketAddress(int address, int port) throws java.net.UnknownHostException
			protected internal virtual SocketAddress getSocketAddress(int address, int port)
			{
				SocketAddress socketAddress;
				if (address == INADDR_ANY)
				{
					socketAddress = new InetSocketAddress(port);
				}
				else if (address == INADDR_BROADCAST && !OnesBroadcast)
				{
					socketAddress = getBroadcastInetSocketAddress(port)[0];
				}
				else
				{
					socketAddress = new InetSocketAddress(InetAddress.getByAddress(internetAddressToBytes(address)), port);
				}

				return socketAddress;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected java.net.SocketAddress[] getMultiSocketAddress(int address, int port) throws java.net.UnknownHostException
			protected internal virtual SocketAddress[] getMultiSocketAddress(int address, int port)
			{
				SocketAddress[] socketAddress;
				if (address == INADDR_ANY)
				{
					socketAddress = new SocketAddress[1];
					socketAddress[0] = new InetSocketAddress(port);
				}
				else if (address == INADDR_BROADCAST && !OnesBroadcast)
				{
					socketAddress = getBroadcastInetSocketAddress(port);
				}
				else
				{
					socketAddress = new SocketAddress[1];
					socketAddress[0] = new InetSocketAddress(InetAddress.getByAddress(internetAddressToBytes(address)), port);
				}

				return socketAddress;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected java.net.SocketAddress getSocketAddress(pspsharp.HLE.kernel.types.pspNetSockAddrInternet addr) throws java.net.UnknownHostException
			protected internal virtual SocketAddress getSocketAddress(pspNetSockAddrInternet addr)
			{
				return getSocketAddress(addr.sin_addr, addr.sin_port);
			}

			protected internal virtual pspNetSockAddrInternet getNetSockAddrInternet(SocketAddress socketAddress)
			{
				pspNetSockAddrInternet addr = null;

				if (socketAddress != null)
				{
					if (socketAddress is InetSocketAddress)
					{
						addr = new pspNetSockAddrInternet();
						addr.readFromInetSocketAddress((InetSocketAddress) socketAddress);
					}
				}

				return addr;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected java.net.SocketAddress[] getMultiSocketAddress(pspsharp.HLE.kernel.types.pspNetSockAddrInternet addr) throws java.net.UnknownHostException
			protected internal virtual SocketAddress[] getMultiSocketAddress(pspNetSockAddrInternet addr)
			{
				return getMultiSocketAddress(addr.sin_addr, addr.sin_port);
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected java.net.InetAddress getInetAddress(int address) throws java.net.UnknownHostException
			protected internal virtual InetAddress getInetAddress(int address)
			{
				InetAddress inetAddress;
				inetAddress = InetAddress.getByAddress(internetAddressToBytes(address));

				return inetAddress;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected java.net.InetAddress getInetAddress(pspsharp.HLE.kernel.types.pspNetSockAddrInternet addr) throws java.net.UnknownHostException
			protected internal virtual InetAddress getInetAddress(pspNetSockAddrInternet addr)
			{
				return getInetAddress(addr.sin_addr);
			}

			protected internal virtual void copySocketAttributes(pspInetSocket from)
			{
				Blocking = from.Blocking;
				Broadcast = from.Broadcast;
				OnesBroadcast = from.OnesBroadcast;
				ReceiveLowWaterMark = from.ReceiveLowWaterMark;
				SendLowWaterMark = from.SendLowWaterMark;
				ReceiveTimeout = from.ReceiveTimeout;
				SendTimeout = from.SendTimeout;
				ReceiveBufferSize = from.ReceiveBufferSize;
				SendBufferSize = from.SendBufferSize;
				ReuseAddress = from.ReuseAddress;
				KeepAlive = from.KeepAlive;
				setLinger(from.LingerEnabled, from.Linger);
				TcpNoDelay = from.TcpNoDelay;
			}

			public virtual bool Broadcast
			{
				get
				{
					return broadcast;
				}
			}

			public virtual int setBroadcast(bool broadcast)
			{
				this.broadcast = broadcast;

				return 0;
			}

			public virtual int ReceiveLowWaterMark
			{
				get
				{
					return receiveLowWaterMark;
				}
				set
				{
					this.receiveLowWaterMark = value;
				}
			}


			public virtual int SendLowWaterMark
			{
				get
				{
					return sendLowWaterMark;
				}
				set
				{
					this.sendLowWaterMark = value;
				}
			}


			protected internal virtual sbyte[] getByteArray(int address, int length)
			{
				sbyte[] bytes = new sbyte[length];
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(address, length, 1);
				for (int i = 0; i < length; i++)
				{
					bytes[i] = (sbyte) memoryReader.readNext();
				}

				return bytes;
			}

			protected internal virtual ByteBuffer getByteBuffer(int address, int length)
			{
				return ByteBuffer.wrap(getByteArray(address, length));
			}

			public override string ToString()
			{
				return string.Format("pspInetSocket[uid={0:D}]", uid);
			}

			public virtual bool OnesBroadcast
			{
				get
				{
					return onesBroadcast;
				}
				set
				{
					this.onesBroadcast = value;
				}
			}


			public virtual int ReceiveTimeout
			{
				get
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("receiveTimeout={0:D}", receiveTimeout));
					}
					return receiveTimeout;
				}
				set
				{
					this.receiveTimeout = value;
				}
			}


			public virtual int SendTimeout
			{
				get
				{
					return sendTimeout;
				}
				set
				{
					this.sendTimeout = value;
				}
			}


			public virtual int ErrorAndClearToSelf
			{
				get
				{
					int value = error;
					// clear error and errno
					clearErrorToSelf();
    
					return value;
				}
			}

			protected internal virtual int SocketError
			{
				set
				{
					this.error = value;
				}
			}

			protected internal virtual Exception SocketError
			{
				set
				{
					if (value is NotYetConnectedException)
					{
						SocketError = ENOTCONN;
					}
					else if (value is ClosedChannelException)
					{
						SocketError = ECLOSED;
					}
					else if (value is AsynchronousCloseException)
					{
						SocketError = ECLOSED;
					}
					else if (value is ClosedByInterruptException)
					{
						SocketError = ECLOSED;
					}
					else if (value is BindException)
					{
						SocketError = EADDRNOTAVAIL;
					}
					else if (value is IOException)
					{
						SocketError = EIO;
					}
					else
					{
						SocketError = -1; // Unknown error
					}
				}
			}

			protected internal virtual void setError(IOException e, BlockingState blockingState)
			{
				SocketError = e;
				setErrno(error, blockingState);
			}

			protected internal virtual IOException ErrorToSelf
			{
				set
				{
					setError(value, null);
				}
			}

			protected internal virtual void setError(int error, BlockingState blockingState)
			{
				SocketError = error;
				setErrno(this.error, blockingState);
			}

			protected internal virtual int ErrorToSelf
			{
				set
				{
					setError(value, null);
				}
			}

			protected internal virtual void clearErrorToSelf()
			{
				clearError(null);
			}

			protected internal virtual void clearError(BlockingState blockingState)
			{
				error = 0;
				setErrno(0, blockingState);
			}

			public virtual int ReceiveBufferSize
			{
				get
				{
					return receiveBufferSize;
				}
			}

			public virtual int setReceiveBufferSize(int receiveBufferSize)
			{
				this.receiveBufferSize = receiveBufferSize;

				return 0;
			}

			public virtual int SendBufferSize
			{
				get
				{
					return sendBufferSize;
				}
			}

			public virtual int setSendBufferSize(int sendBufferSize)
			{
				this.sendBufferSize = sendBufferSize;

				return 0;
			}

			public virtual bool ReuseAddress
			{
				get
				{
					return reuseAddress;
				}
			}

			public virtual int setReuseAddress(bool reuseAddress)
			{
				this.reuseAddress = reuseAddress;
				return 0;
			}

			public virtual bool KeepAlive
			{
				get
				{
					return keepAlive;
				}
			}

			public virtual int setKeepAlive(bool keepAlive)
			{
				this.keepAlive = keepAlive;
				return 0;
			}

			public virtual bool LingerEnabled
			{
				get
				{
					return lingerEnabled;
				}
			}

			public virtual int Linger
			{
				get
				{
					return linger;
				}
			}

			public virtual int setLinger(bool enabled, int linger)
			{
				this.lingerEnabled = enabled;
				this.linger = linger;
				return 0;
			}

			public virtual bool TcpNoDelay
			{
				get
				{
					return tcpNoDelay;
				}
			}

			public virtual int setTcpNoDelay(bool tcpNoDelay)
			{
				this.tcpNoDelay = tcpNoDelay;
				return 0;
			}

			protected internal virtual void storeBytes(int address, int length, sbyte[] bytes)
			{
				if (length > 0)
				{
					IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(address, length, 1);
					for (int i = 0; i < length; i++)
					{
						memoryWriter.writeNext(bytes[i]);
					}
					memoryWriter.flush();
				}
			}

			public virtual Selector getSelector(Selector selector, RawSelector rawSelector)
			{
				return selector;
			}

			public virtual int recv(int buffer, int bufferLength, int flags)
			{
				if ((flags & ~MSG_DONTWAIT) != 0)
				{
					log.warn(string.Format("sceNetInetRecv unsupported flag 0x{0:X} on socket", flags));
				}
				return recv(buffer, bufferLength, flags, null);
			}

			public virtual void blockedRecv(BlockingReceiveState blockingState)
			{
				if (blockingState.Timeout)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetRecv socket=0x{0:X} returning {1:D} (timeout)", Uid, blockingState.receivedLength));
					}
					setErrno(EAGAIN, blockingState);
					outerInstance.unblockThread(blockingState, blockingState.receivedLength);
				}
				else
				{
					int length = recv(blockingState.buffer + blockingState.receivedLength, blockingState.bufferLength - blockingState.receivedLength, blockingState.flags, blockingState);
					if (length >= 0)
					{
						outerInstance.unblockThread(blockingState, blockingState.receivedLength);
					}
				}
			}

			public virtual int send(int buffer, int bufferLength, int flags)
			{
				if ((flags & ~MSG_DONTWAIT) != 0)
				{
					log.warn(string.Format("sceNetInetSend unsupported flag 0x{0:X} on socket", flags));
				}
				return send(buffer, bufferLength, flags, null);
			}

			public virtual void blockedSend(BlockingSendState blockingState)
			{
				if (blockingState.Timeout)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetSend socket=0x{0:X} returning {1:D} (timeout)", Uid, blockingState.sentLength));
					}
					setErrno(EAGAIN, blockingState);
					outerInstance.unblockThread(blockingState, blockingState.sentLength);
				}
				else
				{
					int length = send(blockingState.buffer + blockingState.sentLength, blockingState.bufferLength - blockingState.sentLength, blockingState.flags, blockingState);
					if (length > 0)
					{
						outerInstance.unblockThread(blockingState, blockingState.sentLength);
					}
				}
			}

			public virtual int sendto(int buffer, int bufferLength, int flags, pspNetSockAddrInternet toAddr)
			{
				if ((flags & ~MSG_DONTWAIT) != 0)
				{
					log.warn(string.Format("sceNetInetSendto unsupported flag 0x{0:X} on socket", flags));
				}
				return sendto(buffer, bufferLength, flags, toAddr, null);
			}

			public virtual void blockedSendto(BlockingSendToState blockingState)
			{
				if (blockingState.Timeout)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetSendto socket=0x{0:X} returning {1:D} (timeout)", Uid, blockingState.sentLength));
					}
					setErrno(EAGAIN, blockingState);
					outerInstance.unblockThread(blockingState, blockingState.sentLength);
				}
				else
				{
					int length = sendto(blockingState.buffer + blockingState.sentLength, blockingState.bufferLength - blockingState.sentLength, blockingState.flags, blockingState.toAddr, blockingState);
					if (length > 0)
					{
						outerInstance.unblockThread(blockingState, blockingState.sentLength);
					}
				}
			}

			public virtual int recvfrom(int buffer, int bufferLength, int flags, pspNetSockAddrInternet fromAddr)
			{
				if ((flags & ~MSG_DONTWAIT) != 0)
				{
					log.warn(string.Format("sceNetInetRecvfrom unsupported flag 0x{0:X} on socket", flags));
				}
				return recvfrom(buffer, bufferLength, flags, fromAddr, null);
			}

			public virtual void blockedRecvfrom(BlockingReceiveFromState blockingState)
			{
				if (blockingState.Timeout)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetRecvfrom socket=0x{0:X} returning {1:D} (timeout)", Uid, blockingState.receivedLength));
					}
					setErrno(EAGAIN, blockingState);
					outerInstance.unblockThread(blockingState, blockingState.receivedLength);
				}
				else
				{
					int length = recvfrom(blockingState.buffer + blockingState.receivedLength, blockingState.bufferLength - blockingState.receivedLength, blockingState.flags, blockingState.fromAddr, blockingState);
					if (length >= 0)
					{
						outerInstance.unblockThread(blockingState, blockingState.receivedLength);
					}
				}
			}

			public virtual int accept(pspNetSockAddrInternet sockAddrInternet)
			{
				return accept(sockAddrInternet, null);
			}

			public virtual void blockedAccept(BlockingAcceptState blockingState)
			{
				int socketUid = accept(blockingState.acceptAddr, blockingState);
				if (socketUid >= 0)
				{
					outerInstance.unblockThread(blockingState, socketUid);
				}
			}

			internal virtual pspNetSockAddrInternet LocalAddr
			{
				get
				{
					return localAddr;
				}
				set
				{
					this.localAddr = value;
				}
			}


			internal virtual pspNetSockAddrInternet RemoteAddr
			{
				get
				{
					return remoteAddr;
				}
				set
				{
					this.remoteAddr = value;
				}
			}

		}

		protected internal class pspInetStreamSocket : pspInetSocket
		{
			private readonly sceNetInet outerInstance;

			internal SocketChannel socketChannel;
			internal ServerSocketChannel serverSocketChannel;
			internal bool isServerSocket;
			internal SocketAddress pendingBindAddress;
			internal int backlog;

			public pspInetStreamSocket(sceNetInet outerInstance, int uid) : base(outerInstance, uid)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void configureSocketChannel() throws java.io.IOException
			internal virtual void configureSocketChannel()
			{
				// We have to use non-blocking sockets at Java level
				// to allow further PSP thread scheduling while the PSP is
				// waiting for a blocking operation.
				socketChannel.configureBlocking(false);

				socketChannel.socket().ReceiveBufferSize = receiveBufferSize;
				socketChannel.socket().SendBufferSize = sendBufferSize;
				socketChannel.socket().KeepAlive = keepAlive;
				socketChannel.socket().ReuseAddress = reuseAddress;
				socketChannel.socket().setSoLinger(lingerEnabled, linger);
				socketChannel.socket().TcpNoDelay = tcpNoDelay;

				// Connect has no timeout
				socketChannel.socket().SoTimeout = 0;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void openChannel() throws java.io.IOException
			internal virtual void openChannel()
			{
				if (isServerSocket)
				{
					if (serverSocketChannel == null)
					{
						serverSocketChannel = ServerSocketChannel.open();

						// We have to use non-blocking sockets at Java level
						// to allow further PSP thread scheduling while the PSP is
						// waiting for a blocking operation.
						serverSocketChannel.configureBlocking(false);

						if (socketChannel != null)
						{
							// If the socket was already bound, remember the bind address.
							// It will be rebound in bindChannel().
							if (socketChannel.socket().LocalSocketAddress != null)
							{
								pendingBindAddress = socketChannel.socket().LocalSocketAddress;
							}
							socketChannel.close();
							socketChannel = null;
						}
					}
				}
				else
				{
					if (socketChannel == null)
					{
						socketChannel = SocketChannel.open();
						configureSocketChannel();
					}
				}
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void bindChannel() throws java.io.IOException
			internal virtual void bindChannel()
			{
				if (pendingBindAddress != null)
				{
					if (isServerSocket)
					{
						serverSocketChannel.socket().bind(pendingBindAddress, backlog);
					}
					else
					{
						socketChannel.socket().bind(pendingBindAddress);
					}
					pendingBindAddress = null;
				}
			}

			public override bool finishConnect()
			{
				if (!Blocking && socketChannel.ConnectionPending)
				{
					// Try to finish the connection
					try
					{
						bool connected = socketChannel.finishConnect();
						if (connected)
						{
							LocalAddr = getNetSockAddrInternet(socketChannel.LocalAddress);
						}
						return connected;
					}
					catch (IOException e)
					{
						log.error(e);
						SocketError = e;
						return false;
					}
				}

				return true;
			}

			public override int connect(pspNetSockAddrInternet addr)
			{
				if (isServerSocket)
				{
					log.error(string.Format("connect not supported on server socket stream addr={0}, {1}", addr.ToString(), ToString()));
					return -1;
				}

				try
				{
					openChannel();
					bindChannel();
					// On non-blocking, the connect might still be in progress
					if (!finishConnect())
					{
						// Connect already in progress
						ErrnoToSelf = EALREADY;
						return -1;
					}
					if (socketChannel.Connected)
					{
						// Already connected
						ErrnoToSelf = EISCONN;
						return -1;
					}

					bool connected = socketChannel.connect(getSocketAddress(addr));
					RemoteAddr = addr;

					if (Blocking)
					{
						// blocking mode: wait for the connection to complete
						while (!socketChannel.finishConnect())
						{
							try
							{
								Thread.Sleep(1);
							}
							catch (InterruptedException)
							{
								// Ignore exception
							}
						}
					}
					else if (!connected)
					{
						// non-blocking mode: return EINPROGRESS
						ErrnoToSelf = EINPROGRESS;
						return -1;
					}
				}
				catch (IOException e)
				{
					log.error(e);
					ErrorToSelf = e;
					return -1;
				}

				clearErrorToSelf();
				return 0;
			}

			public override int bind(pspNetSockAddrInternet addr)
			{
				BlockingState blockingState = null;
				try
				{
					openChannel();
					if (isServerSocket)
					{
						pendingBindAddress = getSocketAddress(addr);
					}
					else
					{
						pendingBindAddress = null;
						socketChannel.socket().bind(getSocketAddress(addr));
					}
				}
				catch (IOException e)
				{
					log.error(e);
					setError(e, blockingState);
					return -1;
				}

				clearError(blockingState);
				return 0;
			}

			public override int recv(int buffer, int bufferLength, int flags, BlockingReceiveState blockingState)
			{
				try
				{
					// On non-blocking, the connect might still be in progress
					if (!finishConnect())
					{
						setErrno(EAGAIN, blockingState);
						return -1;
					}

					sbyte[] bytes = new sbyte[bufferLength];
					int length = socketChannel.read(ByteBuffer.wrap(bytes));
					storeBytes(buffer, length, bytes);

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetRecv socket=0x{0:X} received 0x{1:X} bytes", Uid, length));
						if (log.TraceEnabled && length > 0)
						{
							log.trace(string.Format("Received data: {0}", Utilities.getMemoryDump(buffer, length)));
						}
					}

					// end of stream
					if (length < 0)
					{
						clearError(blockingState);
						return 0;
					}

					// Nothing received on a non-blocking stream, return EAGAIN in errno
					if (length == 0 && !isBlocking(flags))
					{
						if (bufferLength == 0)
						{
							clearError(blockingState);
							return 0;
						}
						setErrno(EAGAIN, blockingState);
						return -1;
					}

					if (blockingState != null)
					{
						blockingState.receivedLength += length;
					}

					// With a blocking stream, at least the low water mark has to be read
					if (isBlocking(flags))
					{
						if (blockingState == null)
						{
							blockingState = new BlockingReceiveState(this, buffer, bufferLength, flags, length);
						}

						// If we have not yet read as much as the low water mark,
						// block the thread and retry later.
						if (blockingState.receivedLength < ReceiveLowWaterMark && length < bufferLength)
						{
							outerInstance.blockThread(blockingState);
							return -1;
						}
					}

					clearError(blockingState);
					return length;
				}
				catch (IOException e)
				{
					log.error(e);
					setError(e, blockingState);
					return -1;
				}
			}

			public override int send(int buffer, int bufferLength, int flags, BlockingSendState blockingState)
			{
				try
				{
					// On non-blocking, the connect might still be in progress
					if (!finishConnect())
					{
						setError(ENOTCONN, blockingState);
						return -1;
					}

					ByteBuffer byteBuffer = getByteBuffer(buffer, bufferLength);
					int length = socketChannel.write(byteBuffer);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetSend socket=0x{0:X} successfully sent 0x{1:X} bytes", Uid, length));
					}

					// Nothing sent on a non-blocking stream, return EAGAIN in errno
					if (length == 0 && !isBlocking(flags))
					{
						setErrno(EAGAIN, blockingState);
						return -1;
					}

					if (blockingState != null)
					{
						blockingState.sentLength += length;
					}

					// With a blocking stream, we have to send all the bytes
					if (isBlocking(flags))
					{
						if (blockingState == null)
						{
							blockingState = new BlockingSendState(this, buffer, bufferLength, flags, length);
						}

						// If we have not yet sent all the bytes, block the thread
						// and retry later
						if (length < bufferLength)
						{
							outerInstance.blockThread(blockingState);
							return -1;
						}
					}

					clearError(blockingState);
					return length;
				}
				catch (IOException e)
				{
					log.error(e);
					setError(e, blockingState);
					return -1;
				}
			}

			public override int close()
			{
				BlockingState blockingState = null;

				if (socketChannel != null)
				{
					try
					{
						socketChannel.close();
						socketChannel = null;
					}
					catch (IOException e)
					{
						log.error(e);
						setError(e, blockingState);
						return -1;
					}
				}

				if (serverSocketChannel != null)
				{
					try
					{
						serverSocketChannel.close();
						serverSocketChannel = null;
					}
					catch (IOException e)
					{
						log.error(e);
						setError(e, blockingState);
						return -1;
					}
				}

				clearError(blockingState);
				return 0;
			}

			public override int recvfrom(int buffer, int bufferLength, int flags, pspNetSockAddrInternet fromAddr, BlockingReceiveFromState blockingState)
			{
				log.warn("sceNetInetRecvfrom not supported on stream socket");
				setError(-1, blockingState);
				return -1;
			}

			public override int sendto(int buffer, int bufferLength, int flags, pspNetSockAddrInternet toAddr, BlockingSendToState blockingState)
			{
				log.warn("sceNetInetSendto not supported on stream socket");
				setError(-1, blockingState);
				return -1;
			}

			public override SelectableChannel SelectableChannel
			{
				get
				{
					if (isServerSocket)
					{
						return serverSocketChannel;
					}
					return socketChannel;
				}
			}

			public override bool Valid
			{
				get
				{
					if (isServerSocket)
					{
						return serverSocketChannel != null;
					}
    
					if (socketChannel == null)
					{
						return false;
					}
    
					if (socketChannel.ConnectionPending)
					{
						// Finish the connection otherwise, the channel will never
						// be readable/writable
						try
						{
							socketChannel.finishConnect();
						}
						catch (IOException e)
						{
							if (log.DebugEnabled)
							{
								log.debug(string.Format("{0}: {1}", ToString(), e.ToString()));
							}
							return false;
						}
					}
					else if (!socketChannel.Connected)
					{
						return false;
					}
    
					return !socketChannel.socket().Closed;
				}
			}

			public override int setReceiveBufferSize(int receiveBufferSize)
			{
				base.ReceiveBufferSize = receiveBufferSize;
				if (socketChannel != null)
				{
					try
					{
						socketChannel.socket().ReceiveBufferSize = receiveBufferSize;
					}
					catch (SocketException e)
					{
						ErrorToSelf = e;
						return -1;
					}
				}

				return 0;
			}

			public override int setSendBufferSize(int sendBufferSize)
			{
				base.SendBufferSize = sendBufferSize;
				if (socketChannel != null)
				{
					try
					{
						socketChannel.socket().SendBufferSize = sendBufferSize;
					}
					catch (SocketException e)
					{
						ErrorToSelf = e;
						return -1;
					}
				}

				return 0;
			}

			public override int getPeername(pspNetSockAddrInternet sockAddrInternet)
			{
				if (socketChannel == null)
				{
					return -1;
				}

				InetAddress inetAddress = socketChannel.socket().InetAddress;
				sockAddrInternet.readFromInetAddress(inetAddress, RemoteAddr);

				return 0;
			}

			public override int getSockname(pspNetSockAddrInternet sockAddrInternet)
			{
				if (socketChannel == null)
				{
					return -1;
				}

				InetAddress inetAddress = socketChannel.socket().LocalAddress;
				sockAddrInternet.readFromInetAddress(inetAddress, LocalAddr);

				return 0;
			}

			public override int setKeepAlive(bool keepAlive)
			{
				base.KeepAlive = keepAlive;
				if (socketChannel != null)
				{
					try
					{
						socketChannel.socket().KeepAlive = keepAlive;
					}
					catch (SocketException e)
					{
						log.error(e);
						ErrorToSelf = e;
						return -1;
					}
				}

				return 0;
			}

			public override int setLinger(bool enabled, int linger)
			{
				base.setLinger(enabled, linger);
				if (socketChannel != null)
				{
					try
					{
						socketChannel.socket().setSoLinger(enabled, linger);
					}
					catch (SocketException e)
					{
						log.error(e);
						ErrorToSelf = e;
						return -1;
					}
				}

				return 0;
			}

			public override int setReuseAddress(bool reuseAddress)
			{
				base.ReuseAddress = reuseAddress;
				if (socketChannel != null)
				{
					try
					{
						socketChannel.socket().ReuseAddress = reuseAddress;
					}
					catch (SocketException e)
					{
						log.error(e);
						ErrorToSelf = e;
						return -1;
					}
				}

				return 0;
			}

			public override int setTcpNoDelay(bool tcpNoDelay)
			{
				base.TcpNoDelay = tcpNoDelay;
				if (socketChannel != null)
				{
					try
					{
						socketChannel.socket().TcpNoDelay = tcpNoDelay;
					}
					catch (SocketException e)
					{
						log.error(e);
						ErrorToSelf = e;
						return -1;
					}
				}

				return 0;
			}

			public override int shutdown(int how)
			{
				if (socketChannel != null)
				{
					try
					{
						switch (how)
						{
							case SHUT_RD:
								socketChannel.socket().shutdownInput();
								break;
							case SHUT_WR:
								socketChannel.socket().shutdownOutput();
								break;
							case SHUT_RDWR:
								socketChannel.socket().shutdownInput();
								socketChannel.socket().shutdownOutput();
								break;
						}
					}
					catch (IOException e)
					{
						log.error(e);
						ErrorToSelf = e;
						return -1;
					}
				}

				return 0;
			}

			public override int listen(int backlog)
			{
				isServerSocket = true;
				this.backlog = backlog;
				try
				{
					openChannel();
					bindChannel();
				}
				catch (IOException e)
				{
					log.error(e);
					ErrorToSelf = e;
					return -1;
				}

				return 0;
			}

			public override int accept(pspNetSockAddrInternet sockAddrInternet)
			{
				if (!isServerSocket)
				{
					log.error(string.Format("sceNetInetAccept on non-server socket stream not allowed addr={0}, {1}", sockAddrInternet.ToString(), ToString()));
					return -1;
				}
				return base.accept(sockAddrInternet);
			}

			public override int accept(pspNetSockAddrInternet sockAddrInternet, BlockingAcceptState blockingState)
			{
				SocketChannel socketChannel;
				try
				{
					openChannel();
					bindChannel();
					socketChannel = serverSocketChannel.accept();
				}
				catch (IOException e)
				{
					log.error(e);
					setError(e, blockingState);
					return -1;
				}

				if (socketChannel == null)
				{
					if (Blocking)
					{
						if (blockingState == null)
						{
							blockingState = new BlockingAcceptState(this, sockAddrInternet);
						}
						outerInstance.blockThread(blockingState);
						return -1;
					}

					setErrno(EWOULDBLOCK, blockingState);
					return -1;
				}

				pspInetStreamSocket inetSocket = (pspInetStreamSocket) outerInstance.createSocket(SOCK_STREAM, 0);
				inetSocket.socketChannel = socketChannel;
				inetSocket.copySocketAttributes(this);
				try
				{
					inetSocket.configureSocketChannel();
				}
				catch (IOException e)
				{
					log.error(e);
				}
				sockAddrInternet.readFromInetSocketAddress((InetSocketAddress) socketChannel.socket().RemoteSocketAddress);
				sockAddrInternet.write(Memory.Instance);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceNetInetAccept accepted connection from {0} on socket {1}", sockAddrInternet.ToString(), inetSocket.ToString()));
				}
				else if (log.InfoEnabled)
				{
					log.info(string.Format("sceNetInetAccept accepted connection from {0}", sockAddrInternet.ToString()));
				}

				return inetSocket.Uid;
			}
		}

		protected internal class pspInetDatagramSocket : pspInetSocket
		{
			private readonly sceNetInet outerInstance;

			internal DatagramChannel datagramChannel;

			public pspInetDatagramSocket(sceNetInet outerInstance, int uid) : base(outerInstance, uid)
			{
				this.outerInstance = outerInstance;

				// Datagrams have different default buffer sizes
				receiveBufferSize = 41600;
				sendBufferSize = 9216;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void openChannel() throws java.io.IOException
			internal virtual void openChannel()
			{
				if (datagramChannel == null)
				{
					datagramChannel = DatagramChannel.open();
					// We have to use non-blocking sockets at Java level
					// to allow further PSP thread scheduling while the PSP is
					// waiting for a blocking operation.
					datagramChannel.configureBlocking(false);

					datagramChannel.socket().ReceiveBufferSize = receiveBufferSize;
					datagramChannel.socket().SendBufferSize = sendBufferSize;
				}
			}

			public override int connect(pspNetSockAddrInternet addr)
			{
				try
				{
					openChannel();
					datagramChannel.connect(getSocketAddress(addr));
					RemoteAddr = addr;
				}
				catch (IOException e)
				{
					log.error(e);
					ErrorToSelf = e;
					return -1;
				}

				clearErrorToSelf();
				return 0;
			}

			public override int bind(pspNetSockAddrInternet addr)
			{
				try
				{
					openChannel();
					datagramChannel.socket().bind(getSocketAddress(addr));
				}
				catch (IOException e)
				{
					log.error(e);
					ErrorToSelf = e;
					return -1;
				}

				clearErrorToSelf();
				return 0;
			}

			public override int close()
			{
				if (datagramChannel != null)
				{
					try
					{
						datagramChannel.close();
						datagramChannel = null;
					}
					catch (IOException e)
					{
						log.error(e);
						ErrorToSelf = e;
						return -1;
					}
				}

				clearErrorToSelf();
				return 0;
			}

			public override int recv(int buffer, int bufferLength, int flags, BlockingReceiveState blockingState)
			{
				try
				{
					sbyte[] bytes = new sbyte[bufferLength];
					ByteBuffer byteBuffer = ByteBuffer.wrap(bytes);
					SocketAddress socketAddress = datagramChannel.receive(byteBuffer);
					int length = byteBuffer.position();
					storeBytes(buffer, length, bytes);

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetRecv socket=0x{0:X} received 0x{1:X} bytes from {2}", Uid, length, socketAddress));
						if (log.TraceEnabled && length > 0)
						{
							log.trace(string.Format("Received data: {0}", Utilities.getMemoryDump(buffer, length)));
						}
					}

					if (length < 0)
					{
						// end of stream
						clearError(blockingState);
						return 0;
					}

					// Nothing received on a non-blocking stream, return EAGAIN in errno
					if (length == 0 && !isBlocking(flags))
					{
						if (bufferLength == 0)
						{
							clearError(blockingState);
							return 0;
						}
						setErrno(EAGAIN, blockingState);
						return -1;
					}

					if (blockingState != null)
					{
						blockingState.receivedLength += length;
					}

					// With a blocking stream, at least the low water mark has to be read
					if (isBlocking(flags))
					{
						if (blockingState == null)
						{
							blockingState = new BlockingReceiveState(this, buffer, bufferLength, flags, length);
						}

						// If we have not yet read as much as the low water mark,
						// block the thread and retry later.
						if (blockingState.receivedLength < ReceiveLowWaterMark && length < bufferLength)
						{
							outerInstance.blockThread(blockingState);
							return -1;
						}
					}

					clearError(blockingState);
					return length;
				}
				catch (IOException e)
				{
					log.error(e);
					setError(e, blockingState);
					return -1;
				}
			}

			public override int send(int buffer, int bufferLength, int flags, BlockingSendState blockingState)
			{
				log.warn("sceNetInetSend not supported on datagram socket");
				setError(-1, blockingState);
				return -1;
			}

			public override int recvfrom(int buffer, int bufferLength, int flags, pspNetSockAddrInternet fromAddr, BlockingReceiveFromState blockingState)
			{
				try
				{
					sbyte[] bytes = new sbyte[bufferLength];
					ByteBuffer byteBuffer = ByteBuffer.wrap(bytes);
					SocketAddress socketAddress = datagramChannel.receive(byteBuffer);
					int length = byteBuffer.position();
					storeBytes(buffer, length, bytes);

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetRecvfrom socket=0x{0:X} received 0x{1:X} bytes from {2}", Uid, length, socketAddress));
						if (log.TraceEnabled && length > 0)
						{
							log.trace(string.Format("Received data: {0}", Utilities.getMemoryDump(buffer, length)));
						}
					}

					if (socketAddress == null)
					{
						// Nothing received on a non-blocking datagram, return EAGAIN in errno
						if (!isBlocking(flags))
						{
							setErrno(EAGAIN, blockingState);
							return -1;
						}

						// Nothing received on a blocking datagram, block the thread
						if (blockingState == null)
						{
							blockingState = new BlockingReceiveFromState(this, buffer, bufferLength, flags, fromAddr, length);
						}
						outerInstance.blockThread(blockingState);
						return -1;
					}

					if (socketAddress is InetSocketAddress)
					{
						InetSocketAddress inetSocketAddress = (InetSocketAddress) socketAddress;
						fromAddr.readFromInetSocketAddress(inetSocketAddress);
						fromAddr.write(Memory.Instance);
					}

					clearError(blockingState);
					return length;
				}
				catch (IOException e)
				{
					log.error(e);
					setError(e, blockingState);
					return -1;
				}
			}

			public override int sendto(int buffer, int bufferLength, int flags, pspNetSockAddrInternet toAddr, BlockingSendToState blockingState)
			{
				try
				{
					openChannel();
					ByteBuffer byteBuffer = getByteBuffer(buffer, bufferLength);
					SocketAddress socketAddress = getSocketAddress(toAddr);
					int length = datagramChannel.send(byteBuffer, socketAddress);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetSendto socket=0x{0:X} successfully sent 0x{1:X} bytes", Uid, length));
					}

					// Nothing sent on a non-blocking stream, return EAGAIN in errno
					if (length == 0 && !isBlocking(flags))
					{
						setErrno(EAGAIN, blockingState);
						return -1;
					}

					if (blockingState != null)
					{
						blockingState.sentLength += length;
					}

					// With a blocking stream, we have to send all the bytes
					if (isBlocking(flags))
					{
						if (blockingState == null)
						{
							blockingState = new BlockingSendToState(this, buffer, bufferLength, flags, toAddr, length);
						}

						// If we have not yet sent all the bytes, block the thread
						// and retry later
						if (length < bufferLength)
						{
							outerInstance.blockThread(blockingState);
							return -1;
						}
					}

					clearError(blockingState);
					return length;
				}
				catch (IOException e)
				{
					log.error(e);
					setError(e, blockingState);
					return -1;
				}
			}

			public override int setBroadcast(bool broadcast)
			{
				base.Broadcast = broadcast;
				try
				{
					openChannel();
					datagramChannel.socket().Broadcast = broadcast;
				}
				catch (IOException e)
				{
					log.error(e);
					ErrorToSelf = e;
					return -1;
				}

				clearErrorToSelf();
				return 0;
			}

			public override SelectableChannel SelectableChannel
			{
				get
				{
					return datagramChannel;
				}
			}

			public override bool Valid
			{
				get
				{
					return datagramChannel != null && !datagramChannel.socket().Closed;
				}
			}

			public override int setReceiveBufferSize(int receiveBufferSize)
			{
				base.ReceiveBufferSize = receiveBufferSize;
				if (datagramChannel != null)
				{
					try
					{
						datagramChannel.socket().ReceiveBufferSize = receiveBufferSize;
					}
					catch (SocketException e)
					{
						ErrorToSelf = e;
						return -1;
					}
				}

				return 0;
			}

			public override int setSendBufferSize(int sendBufferSize)
			{
				base.SendBufferSize = sendBufferSize;
				if (datagramChannel != null)
				{
					try
					{
						datagramChannel.socket().SendBufferSize = sendBufferSize;
					}
					catch (SocketException e)
					{
						ErrorToSelf = e;
						return -1;
					}
				}

				return 0;
			}

			public override int getPeername(pspNetSockAddrInternet sockAddrInternet)
			{
				if (datagramChannel == null)
				{
					return -1;
				}

				InetAddress inetAddress = datagramChannel.socket().InetAddress;
				sockAddrInternet.readFromInetAddress(inetAddress, RemoteAddr);

				return 0;
			}

			public override int getSockname(pspNetSockAddrInternet sockAddrInternet)
			{
				if (datagramChannel == null)
				{
					return -1;
				}

				InetAddress inetAddress = datagramChannel.socket().LocalAddress;
				sockAddrInternet.readFromInetAddress(inetAddress, LocalAddr);

				return 0;
			}

			public override int setReuseAddress(bool reuseAddress)
			{
				base.ReuseAddress = reuseAddress;
				if (datagramChannel != null)
				{
					try
					{
						datagramChannel.socket().ReuseAddress = reuseAddress;
					}
					catch (SocketException e)
					{
						log.error(e);
						ErrorToSelf = e;
						return -1;
					}
				}

				return 0;
			}

			public override int shutdown(int how)
			{
				log.error(string.Format("Shutdown not supported on datagram socket: how={0:D}, {1}", how, ToString()));
				return -1;
			}

			public override bool finishConnect()
			{
				// Nothing to do for datagrams
				return true;
			}

			public override int listen(int backlog)
			{
				log.error(string.Format("Listen not supported on datagram socket: backlog={0:D}, {1}", backlog, ToString()));
				return -1;
			}

			public override int accept(pspNetSockAddrInternet sockAddrInternet, BlockingAcceptState blockingState)
			{
				log.error(string.Format("Accept not supported on datagram socket: sockAddrInternet={0}, {1}", sockAddrInternet.ToString(), ToString()));
				return -1;
			}
		}

		protected internal class pspInetRawSocket : pspInetSocket
		{
			private readonly sceNetInet outerInstance;

			internal RawChannel rawChannel;
			internal int protocol;
			internal bool isAvailable;

			public pspInetRawSocket(sceNetInet outerInstance, int uid, int protocol) : base(outerInstance, uid)
			{
				this.outerInstance = outerInstance;
				this.protocol = protocol;
				isAvailable = true;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected boolean openChannel() throws IllegalStateException
			protected internal virtual bool openChannel()
			{
				if (!isAvailable)
				{
					return false;
				}

				if (rawChannel == null)
				{
					try
					{
						rawChannel = new RawChannel();
					}
					catch (UnsatisfiedLinkError e)
					{
						log.error(string.Format("The rocksaw library is not available on your system ({0}). This library is required to implement RAW sockets. Disabling this feature.", e.ToString()));
						isAvailable = false;
						return false;
					}

					try
					{
						rawChannel.socket().open(RawSocket.PF_INET, protocol);

						// Use non-blocking IO's
						rawChannel.configureBlocking(false);
					}
					catch (IOException e)
					{
						log.error(string.Format("You need to start pspsharp with administator right to be able to open RAW sockets ({0}). Disabling this feature.", e.ToString()));
						isAvailable = false;
						return false;
					}
				}

				return rawChannel.socket().Open;
			}

			public override int bind(pspNetSockAddrInternet addr)
			{
				if (!openChannel())
				{
					return -1;
				}

				try
				{
					rawChannel.socket().bind(getInetAddress(addr));
				}
				catch (System.InvalidOperationException e)
				{
					log.error(e);
					return -1;
				}
				catch (UnknownHostException e)
				{
					log.error(e);
					ErrorToSelf = e;
					return -1;
				}
				catch (IOException e)
				{
					log.error(e);
					ErrorToSelf = e;
					return -1;
				}

				return 0;
			}

			public override int close()
			{
				if (rawChannel != null)
				{
					try
					{
						rawChannel.close();
					}
					catch (IOException e)
					{
						log.error(e);
						ErrorToSelf = e;
						return -1;
					}
					finally
					{
						rawChannel = null;
					}
				}

				return 0;
			}

			public override int connect(pspNetSockAddrInternet addr)
			{
				log.error(string.Format("sceNetInetConnect is not supported on Raw sockets: {0}", ToString()));
				return -1;
			}

			public override bool finishConnect()
			{
				return openChannel();
			}

			public override int getPeername(pspNetSockAddrInternet sockAddrInternet)
			{
				log.error(string.Format("sceNetInetGetpeername is not supported on Raw sockets: {0}", ToString()));
				return -1;
			}

			public override SelectableChannel SelectableChannel
			{
				get
				{
					if (!openChannel())
					{
						return null;
					}
					return rawChannel;
				}
			}

			public override int getSockname(pspNetSockAddrInternet sockAddrInternet)
			{
				log.error(string.Format("sceNetInetGetsockname is not supported on Raw sockets: {0}", ToString()));
				return -1;
			}

			public override bool Valid
			{
				get
				{
					return openChannel();
				}
			}

			public override int listen(int backlog)
			{
				log.error(string.Format("sceNetInetListen is not supported on Raw sockets: {0}", ToString()));
				return -1;
			}

			public override int recv(int buffer, int bufferLength, int flags, BlockingReceiveState blockingState)
			{
				log.error(string.Format("sceNetInetRecv is not supported on Raw sockets: {0}", ToString()));
				return -1;
			}

			public override int recvfrom(int buffer, int bufferLength, int flags, pspNetSockAddrInternet fromAddr, BlockingReceiveFromState blockingState)
			{
				try
				{
					if (!openChannel())
					{
						return -1;
					}

					// Nothing available for read?
					if (!rawChannel.socket().SelectedForRead)
					{
						if (!isBlocking(flags))
						{
							// Nothing received on a non-blocking stream, return EAGAIN in errno
							setErrno(EAGAIN, blockingState);
							return -1;
						}

						if (blockingState == null)
						{
							blockingState = new BlockingReceiveFromState(this, buffer, bufferLength, flags, fromAddr, 0);
						}

						// Block the thread and retry later.
						outerInstance.blockThread(blockingState);
						return -1;
					}

					sbyte[] bytes = new sbyte[bufferLength];
					sbyte[] address = new sbyte[4];
					int length = rawChannel.socket().read(bytes, address);
					storeBytes(buffer, length, bytes);

					if (blockingState != null)
					{
						blockingState.receivedLength += length;
					}

					fromAddr.sin_family = AF_INET;
					fromAddr.sin_addr = bytesToInternetAddress(address);
					fromAddr.write(Memory.Instance);

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetRecvfrom socket=0x{0:X} received 0x{1:X} bytes from {2}", Uid, length, fromAddr));
						if (log.TraceEnabled && length > 0)
						{
							log.trace(string.Format("Received data: {0}", Utilities.getMemoryDump(buffer, length)));
						}
					}

					return length;
				}
				catch (InterruptedIOException e)
				{
					log.error(e);
					setError(e, blockingState);
					return -1;
				}
				catch (IOException e)
				{
					log.error(e);
					setError(e, blockingState);
					return -1;
				}
			}

			public override int send(int buffer, int bufferLength, int flags, BlockingSendState blockignState)
			{
				log.error(string.Format("sceNetInetSend is not supported on Raw sockets: {0}", ToString()));
				return -1;
			}

			public override int sendto(int buffer, int bufferLength, int flags, pspNetSockAddrInternet toAddr, BlockingSendToState blockingState)
			{
				try
				{
					if (!openChannel())
					{
						return -1;
					}

					// Ready for write?
					if (!rawChannel.socket().SelectedForWrite)
					{
						if (!isBlocking(flags))
						{
							// Nothing sent on a non-blocking stream, return EAGAIN in errno
							setErrno(EAGAIN, blockingState);
							return -1;
						}

						// With a blocking stream, we have to send all the bytes
						if (blockingState == null)
						{
							blockingState = new BlockingSendToState(this, buffer, bufferLength, flags, toAddr, 0);
						}

						// Block the thread and retry later
						outerInstance.blockThread(blockingState);
						return -1;
					}

					InetAddress inetAddress = getInetAddress(toAddr);
					sbyte[] data = getByteArray(buffer, bufferLength);
					int length = rawChannel.socket().write(inetAddress, data);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetSendto socket=0x{0:X} successfully sent 0x{1:X} bytes", Uid, length));
					}

					if (blockingState != null)
					{
						blockingState.sentLength += length;
					}

					return length;
				}
				catch (System.InvalidOperationException e)
				{
					log.error(e);
					return -1;
				}
				catch (IOException e)
				{
					log.error(e);
					setError(e, blockingState);
					return -1;
				}
			}

			public override int shutdown(int how)
			{
				log.warn(string.Format("sceNetInetShutdown is not supported on Raw sockets: {0}", ToString()));
				return 0;
			}

			public override Selector getSelector(Selector selector, RawSelector rawSelector)
			{
				return rawSelector;
			}

			public override int accept(pspNetSockAddrInternet sockAddrInternet, BlockingAcceptState blockingState)
			{
				log.error(string.Format("sceNetInetAccept is not supported on Raw sockets: {0}", ToString()));
				return -1;
			}
		}

		protected internal class pspInetPollFd : pspAbstractMemoryMappedStructure
		{
			public int fd;
			public int events;
			public int revents;

			protected internal override void read()
			{
				fd = read32();
				events = read16();
				revents = read16();
			}

			protected internal override void write()
			{
				write32(fd);
				write16((short) events);
				write16((short) revents);
			}

			public override int @sizeof()
			{
				return 8;
			}

			public override string ToString()
			{
				return string.Format("PollFd[fd={0:D}, events=0x{1:X4}({2}), revents=0x{3:X4}({4})]", fd, events, getPollEventName(events), revents, getPollEventName(revents));
			}
		}

		protected internal Dictionary<int, pspInetSocket> sockets;
		protected internal const string idPurpose = "sceNetInet-socket";
		private InetAddress[] doProxyInetAddresses;
		private readonly sbyte[] unknown1 = new sbyte[4];
		private readonly sbyte[] unknown2 = new sbyte[4];

		public override void start()
		{
			setSettingsListener("network.broadcastAddress", new BroadcastAddressSettingsListener());

			sockets = new Dictionary<int, pspInetSocket>();
			doProxyInetAddresses = null;

			// Perform long running actions in a separate thread to not brake
			// the start of the emulator
			AsyncStartThread asyncStartThread = new AsyncStartThread(this);
			asyncStartThread.Name = "sceNetInet Async Start Thread";
			asyncStartThread.Start();

			base.start();
		}

		public override void stop()
		{
			// Close all the open sockets
			foreach (pspInetSocket inetSocket in sockets.Values)
			{
				inetSocket.close();
			}
			sockets.Clear();

			base.stop();
		}

		/// <summary>
		/// Set the errno to an error value.
		/// Each thread has its own errno.
		/// </summary>
		/// <param name="errno"> </param>
		public static int ErrnoToSelf
		{
			set
			{
				setErrno(value, Modules.ThreadManForUserModule.CurrentThread);
			}
		}

		private static void setErrno(int errno, SceKernelThreadInfo thread)
		{
			thread.errno = errno;
		}

		private static void setErrno(int errno, BlockingState blockingState)
		{
			if (blockingState == null)
			{
				ErrnoToSelf = errno;
			}
			else
			{
				setErrno(errno, Modules.ThreadManForUserModule.getThreadById(blockingState.threadId));
			}
		}

		/// <summary>
		/// Return the current value of the errno.
		/// Each thread has its own errno.
		/// </summary>
		/// <returns> the errno of the current thread </returns>
		public static int Errno
		{
			get
			{
				return Modules.ThreadManForUserModule.CurrentThread.errno;
			}
		}

		protected internal virtual int createSocketId()
		{
			// A socket ID has to be a number [1..255],
			// because sceNetInetSelect can handle only 256 bits.
			// 0 is considered by LuaPLayer as an invalid value as well.
			return SceUidManager.getNewId(idPurpose, 1, 255);
		}

		protected internal virtual pspInetSocket createSocket(int type, int protocol)
		{
			int uid = createSocketId();
			pspInetSocket inetSocket = null;
			if (type == SOCK_STREAM)
			{
				inetSocket = new pspInetStreamSocket(this, uid);
			}
			else if (type == SOCK_DGRAM)
			{
				inetSocket = new pspInetDatagramSocket(this, uid);
			}
			else if (type == SOCK_RAW)
			{
				inetSocket = new pspInetRawSocket(this, uid, protocol);
			}
			else if (type == SOCK_DGRAM_UNKNOWN_6)
			{
				inetSocket = new pspInetDatagramSocket(this, uid);
			}
			else if (type == SOCK_STREAM_UNKNOWN_10)
			{
				inetSocket = new pspInetStreamSocket(this, uid);
			}
			sockets[uid] = inetSocket;

			return inetSocket;
		}

		protected internal virtual void releaseSocketId(int id)
		{
			SceUidManager.releaseId(id, idPurpose);
		}

		protected internal virtual int readSocketList(Selector selector, RawSelector rawSelector, TPointer address, int n, int selectorOperation, string comment)
		{
			int closedSocketsCount = 0;

			if (address.NotNull)
			{
				LinkedList<int> closedChannels = new LinkedList<int>();
				int length = (n + 7) / 8;
				if (selectorOperation != 0)
				{
					IMemoryReader memoryReader = MemoryReader.getMemoryReader(address.Address, length, 4);
					int value = 0;
					for (int socket = 0; socket < n; socket++)
					{
						if ((socket % 32) == 0)
						{
							value = memoryReader.readNext();
						}
						int bit = (value & 1);
						value = (int)((uint)value >> 1);
						if (bit != 0)
						{
							pspInetSocket inetSocket = sockets[socket];
							if (inetSocket != null)
							{
								SelectableChannel selectableChannel = inetSocket.SelectableChannel;
								if (selectableChannel != null)
								{
									int registeredOperation = selectorOperation & selectableChannel.validOps();
									if (registeredOperation != 0)
									{
										Selector socketSelector = inetSocket.getSelector(selector, rawSelector);

										// A channel may be registered at most once with any particular selector
										if (selectableChannel.Registered)
										{
											// If the channel is already registered,
											// add the new operation to the active registration
											SelectionKey selectionKey = selectableChannel.keyFor(socketSelector);
											selectionKey.interestOps(selectionKey.interestOps() | registeredOperation);
										}
										else
										{
											try
											{
												selectableChannel.register(socketSelector, registeredOperation, new int?(socket));
											}
											catch (ClosedChannelException e)
											{
												closedChannels.AddLast(socket);
												if (log.DebugEnabled)
												{
													log.debug(string.Format("{0}: {1}", inetSocket.ToString(), e.ToString()));
												}
											}
										}
									}
								}
							}
						}
					}
				}

				// Clear the socket list so that we just have to set the bits for
				// the sockets that are ready.
				address.clear(length);

				// and set the bit for all the closed channels
				foreach (int? socket in closedChannels)
				{
					setSelectBit(address, socket.Value);
					closedSocketsCount++;
				}
			}

			return closedSocketsCount;
		}

		protected internal virtual string dumpSelectBits(TPointer addr, int n)
		{
			if (addr.Null || n <= 0)
			{
				return "";
			}

			StringBuilder dump = new StringBuilder();
			for (int socket = 0; socket < n; socket++)
			{
				int bit = 1 << (socket % 8);
				int value = addr.getValue8(socket / 8);
				if ((value & bit) != 0)
				{
					if (dump.Length > 0)
					{
						dump.Append(", ");
					}
					dump.Append(string.Format("{0:D}", socket));
				}
			}

			return dump.ToString();
		}

		protected internal virtual void setSelectBit(TPointer addr, int socket)
		{
			if (addr.NotNull)
			{
				int offset = socket / 8;
				int value = 1 << (socket % 8);
				addr.setValue8(offset, (sbyte)(addr.getValue8(offset) | value));
			}
		}

		protected internal virtual void blockThread(BlockingState blockingState)
		{
			if (!blockingState.threadBlocked)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Blocking the current thread {0}", Modules.ThreadManForUserModule.CurrentThread.ToString()));
				}
				Modules.ThreadManForUserModule.hleBlockCurrentThread(SceKernelThreadInfo.JPCSP_WAIT_NET, blockingState);
				blockingState.threadBlocked = true;
			}
			long schedule = Emulator.Clock.microTime() + BLOCKED_OPERATION_POLLING_MICROS;
			Emulator.Scheduler.addAction(schedule, blockingState);
		}

		protected internal virtual void unblockThread(BlockingState blockingState, int returnValue)
		{
			SceKernelThreadInfo thread = Modules.ThreadManForUserModule.getThreadById(blockingState.threadId);
			if (thread != null)
			{
				thread.cpuContext._v0 = returnValue;
			}
			if (blockingState.threadBlocked)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Unblocking the thread {0}", thread.ToString()));
				}
				Modules.ThreadManForUserModule.hleUnblockThread(blockingState.threadId);
				blockingState.threadBlocked = false;
			}
		}

		protected internal virtual int checkInvalidSelectedSockets(BlockingSelectState blockingState)
		{
			int countInvalidSocket = 0;

			// Check for valid sockets.
			// When a socket is no longer valid (e.g. connect failed),
			// return the select bit for this socket so that the application
			// has a chance to see the failed connection.
			foreach (SelectionKey selectionKey in blockingState.selector.keys())
			{
				if (selectionKey.Valid)
				{
					int socket = (int?) selectionKey.attachment().Value;
					pspInetSocket inetSocket = sockets[socket];
					if (inetSocket == null || !inetSocket.Valid)
					{
						countInvalidSocket++;

						int interestOps;
						try
						{
							interestOps = selectionKey.interestOps();
						}
						catch (CancelledKeyException)
						{
							// The key has been cancelled, set the selection bit for all operations
							interestOps = SelectionKey.OP_READ | SelectionKey.OP_WRITE | SelectionKey.OP_CONNECT | SelectionKey.OP_ACCEPT;
						}

						if ((interestOps & readSelectionKeyOperations) != 0)
						{
							setSelectBit(blockingState.readSocketsAddr, socket);
						}
						if ((interestOps & writeSelectionKeyOperations) != 0)
						{
							setSelectBit(blockingState.writeSocketsAddr, socket);
						}
						// Out-of-band data is not implemented (not supported by Java?)
					}
				}
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("checkInvalidSelectedSockets returns {0:D}", countInvalidSocket));
			}

			return countInvalidSocket;
		}

		protected internal virtual void setPollResult(pspInetPollFd[] pollFds, int socket, int revents)
		{
			for (int i = 0; i < pollFds.Length; i++)
			{
				if (pollFds[i].fd == socket)
				{
					pollFds[i].revents |= revents;
					break;
				}
			}
		}

		protected internal virtual void blockedPoll(BlockingPollState blockingState)
		{
			try
			{
				// Try to finish all the pending connections
				for (int i = 0; i < blockingState.pollFds.Length; i++)
				{
					pspInetSocket inetSocket = sockets[blockingState.pollFds[i].fd];
					if (inetSocket != null)
					{
						inetSocket.finishConnect();
					}
				}

				// We do not want to block here on the selector, call selectNow
				int count = blockingState.selector.selectNow();

				bool threadBlocked;
				if (count <= 0)
				{
					// Check for timeout
					if (blockingState.Timeout)
					{
						// Timeout, unblock the thread and return 0
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceNetInetPoll returns {0:D} sockets (timeout)", count));
						}
						threadBlocked = false;
					}
					else
					{
						// No timeout, keep blocking the thread
						threadBlocked = true;
					}
				}
				else
				{
					// Some sockets are ready, unblock the thread and return the count
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetPoll returns {0:D} sockets", count));
					}

					for (IEnumerator<SelectionKey> it = blockingState.selector.selectedKeys().GetEnumerator(); it.MoveNext();)
					{
						// Retrieve the next key and remove it from the set
						SelectionKey selectionKey = it.Current;
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						it.remove();

						if (selectionKey.Readable)
						{
							int socket = (int?) selectionKey.attachment().Value;
							setPollResult(blockingState.pollFds, socket, POLLIN | POLLRDNORM);
						}
						if (selectionKey.Writable)
						{
							int socket = (int?) selectionKey.attachment().Value;
							setPollResult(blockingState.pollFds, socket, POLLOUT);
						}
					}

					threadBlocked = false;
				}

				if (threadBlocked)
				{
					blockThread(blockingState);
				}
				else
				{
					// We do no longer need the selector, close it
					blockingState.selector.close();

					// Write back the updated revents fields
					Memory mem = Memory.Instance;
					for (int i = 0; i < blockingState.pollFds.Length; i++)
					{
						blockingState.pollFds[i].write(mem);
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceNetInetPoll returning pollFd[{0:D}]={1}", i, blockingState.pollFds[i].ToString()));
						}
					}

					// sceNetInetPoll can now return the count, unblock the thread
					unblockThread(blockingState, count);
				}
			}
			catch (IOException e)
			{
				log.error(e);
			}
		}

		protected internal virtual void processSelectedKey(Selector selector, BlockingSelectState blockingState)
		{
			for (IEnumerator<SelectionKey> it = selector.selectedKeys().GetEnumerator(); it.MoveNext();)
			{
				// Retrieve the next key and remove it from the set
				SelectionKey selectionKey = it.Current;
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
				it.remove();

				if ((selectionKey.readyOps() & readSelectionKeyOperations) != 0)
				{
					int socket = (int?) selectionKey.attachment().Value;
					setSelectBit(blockingState.readSocketsAddr, socket);
				}
				if ((selectionKey.readyOps() & writeSelectionKeyOperations) != 0)
				{
					int socket = (int?) selectionKey.attachment().Value;
					setSelectBit(blockingState.writeSocketsAddr, socket);
				}
			}
		}

		protected internal virtual void blockedSelect(BlockingSelectState blockingState)
		{
			try
			{
				// Try to finish all the pending connections
				for (IEnumerator<SelectionKey> it = blockingState.selector.keys().GetEnumerator(); it.MoveNext();)
				{
					SelectionKey selectionKey = it.Current;
					int? socket = (int?) selectionKey.attachment();
					pspInetSocket inetSocket = sockets[socket];
					if (inetSocket != null)
					{
						inetSocket.finishConnect();
					}
				}

				// Start with the count of closed channels
				// (detected when registering the selector)
				int count = blockingState.count;
				// We do not want to block here on the selector, call selectNow
				count += blockingState.selector.selectNow();
				count += blockingState.rawSelector.selectNow();
				// add any socket becoming invalid (e.g. connect failed)
				count += checkInvalidSelectedSockets(blockingState);

				bool threadBlocked;
				if (count <= 0)
				{
					// Check for timeout
					if (blockingState.Timeout)
					{
						// Timeout, unblock the thread and return 0
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceNetInetSelect returns {0:D} sockets (timeout)", count));
						}
						threadBlocked = false;
					}
					else
					{
						// No timeout, keep blocking the thread
						threadBlocked = true;
					}
				}
				else
				{
					// Some sockets are ready, unblock the thread and return the count
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetSelect returns {0:D} sockets", count));
					}

					processSelectedKey(blockingState.selector, blockingState);
					processSelectedKey(blockingState.rawSelector, blockingState);

					threadBlocked = false;
				}

				if (threadBlocked)
				{
					blockThread(blockingState);
				}
				else
				{
					// We do no longer need the selectors, close them
					blockingState.selector.close();
					blockingState.rawSelector.close();

					if (log.DebugEnabled && count > 0)
					{
						if (blockingState.readSocketsAddr.NotNull)
						{
							log.debug(string.Format("sceNetInetSelect returning Read Sockets       : {0}", dumpSelectBits(blockingState.readSocketsAddr, blockingState.numberSockets)));
						}
						if (blockingState.writeSocketsAddr.NotNull)
						{
							log.debug(string.Format("sceNetInetSelect returning Write Sockets      : {0}", dumpSelectBits(blockingState.writeSocketsAddr, blockingState.numberSockets)));
						}
						if (blockingState.outOfBandSocketsAddr.NotNull)
						{
							log.debug(string.Format("sceNetInetSelect returning Out-of-band Sockets: {0}", dumpSelectBits(blockingState.outOfBandSocketsAddr, blockingState.numberSockets)));
						}
					}

					// sceNetInetSelect can now return the count, unblock the thread
					unblockThread(blockingState, count);
				}
			}
			catch (IOException e)
			{
				log.error(e);
			}
		}

		public static string internetAddressToString(int address)
		{
			int n4 = (address >> 24) & 0xFF;
			int n3 = (address >> 16) & 0xFF;
			int n2 = (address >> 8) & 0xFF;
			int n1 = (address) & 0xFF;

			return string.Format("{0:D}.{1:D}.{2:D}.{3:D}", n1, n2, n3, n4);
		}

		public static int bytesToInternetAddress(sbyte[] bytes)
		{
			if (bytes == null)
			{
				return 0;
			}

			int inetAddress = 0;
			for (int i = bytes.Length - 1; i >= 0; i--)
			{
				inetAddress = (inetAddress << 8) | (bytes[i] & 0xFF);
			}

			return inetAddress;
		}

		public static sbyte[] internetAddressToBytes(int address)
		{
			sbyte[] bytes = new sbyte[4];

			int n4 = (address >> 24) & 0xFF;
			int n3 = (address >> 16) & 0xFF;
			int n2 = (address >> 8) & 0xFF;
			int n1 = (address) & 0xFF;
			bytes[3] = (sbyte) n4;
			bytes[2] = (sbyte) n3;
			bytes[1] = (sbyte) n2;
			bytes[0] = (sbyte) n1;

			return bytes;
		}

		protected internal static string getSocketTypeNameString(int type)
		{
			if (type < 0 || type >= socketTypeNames.Length)
			{
				return string.Format("Unknown type {0:D}", type);
			}

			return socketTypeNames[type];
		}

		protected internal static string getPollEventName(int @event)
		{
			StringBuilder name = new StringBuilder();

			if ((@event & POLLIN) != 0)
			{
				name.Append("|POLLIN");
			}
			if ((@event & POLLPRI) != 0)
			{
				name.Append("|POLLPRI");
			}
			if ((@event & POLLOUT) != 0)
			{
				name.Append("|POLLOUT");
			}
			if ((@event & POLLERR) != 0)
			{
				name.Append("|POLLERR");
			}
			if ((@event & POLLHUP) != 0)
			{
				name.Append("|POLLHUP");
			}
			if ((@event & POLLNVAL) != 0)
			{
				name.Append("|POLLNVAL");
			}
			if ((@event & POLLRDNORM) != 0)
			{
				name.Append("|POLLRDNORM");
			}
			if ((@event & POLLRDBAND) != 0)
			{
				name.Append("|POLLRDBAND");
			}
			if ((@event & POLLWRBAND) != 0)
			{
				name.Append("|POLLWRBAND");
			}

			if (name.Length > 0 && name[0] == '|')
			{
				name.Remove(0, 1);
			}

			return name.ToString();
		}

		protected internal static string getOptionNameString(int optionName)
		{
			switch (optionName)
			{
				case SO_DEBUG:
					return "SO_DEBUG";
				case SO_ACCEPTCONN:
					return "SO_ACCEPTCONN";
				case SO_REUSEADDR:
					return "SO_REUSEADDR";
				case SO_KEEPALIVE:
					return "SO_KEEPALIVE";
				case SO_DONTROUTE:
					return "SO_DONTROUTE";
				case SO_BROADCAST:
					return "SO_BROADCAST";
				case SO_USELOOPBACK:
					return "SO_USELOOPBACK";
				case SO_LINGER:
					return "SO_LINGER";
				case SO_OOBINLINE:
					return "SO_OOBINLINE";
				case SO_REUSEPORT:
					return "SO_REUSEPORT";
				case SO_TIMESTAMP:
					return "SO_TIMESTAMP";
				case SO_ONESBCAST:
					return "SO_ONESBCAST";
				case SO_SNDBUF:
					return "SO_SNDBUF";
				case SO_RCVBUF:
					return "SO_RCVBUF";
				case SO_SNDLOWAT:
					return "SO_SNDLOWAT";
				case SO_RCVLOWAT:
					return "SO_RCVLOWAT";
				case SO_SNDTIMEO:
					return "SO_SNDTIMEO";
				case SO_RCVTIMEO:
					return "SO_RCVTIMEO";
				case SO_ERROR:
					return "SO_ERROR";
				case SO_TYPE:
					return "SO_TYPE";
				case SO_NONBLOCK:
					return "SO_NONBLOCK";
				default:
					return string.Format("Unknown option 0x{0:X}", optionName);
			}
		}

		public virtual int checkSocket(int socket)
		{
			if (!sockets.ContainsKey(socket))
			{
				log.warn(string.Format("checkSocket invalid socket=0x{0:X}", socket));
				throw new SceKernelErrorException(-1); // Unknown error code
			}

			return socket;
		}

		public virtual int checkAddressLength(int addressLength)
		{
			if (addressLength < 16)
			{
				log.warn(string.Format("checkAddressLength invalid addressLength={0:D}", addressLength));
				throw new SceKernelErrorException(-1); // Unknown error code
			}

			return addressLength;
		}

		private void getProxyForSockAddrInternet(pspNetSockAddrInternet sockAddrInternet)
		{
			foreach (InetAddress inetAddress in doProxyInetAddresses)
			{
				if (sockAddrInternet.Equals(inetAddress))
				{
					sockAddrInternet.sin_addr = HTTPServer.Instance.ProxyAddress;
					sockAddrInternet.sin_port = HTTPServer.Instance.ProxyPort;
					break;
				}
			}
		}

		// int sceNetInetInit(void);
		[HLEFunction(nid : 0x17943399, version : 150)]
		public virtual int sceNetInetInit()
		{
			return 0;
		}

		// int sceNetInetTerm(void);
		[HLEFunction(nid : 0xA9ED66B9, version : 150)]
		public virtual int sceNetInetTerm()
		{
			return 0;
		}

		// int sceNetInetAccept(int s, struct sockaddr *addr, socklen_t *addrlen);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xDB094E1B, version = 150) public int sceNetInetAccept(@CheckArgument("checkSocket") int socket, pspsharp.HLE.TPointer address, pspsharp.HLE.TPointer32 addressLengthAddr)
		[HLEFunction(nid : 0xDB094E1B, version : 150)]
		public virtual int sceNetInetAccept(int socket, TPointer address, TPointer32 addressLengthAddr)
		{
			pspInetSocket inetSocket = sockets[socket];
			pspNetSockAddrInternet sockAddrInternet = new pspNetSockAddrInternet();

			int addressLength = addressLengthAddr.getValue();
			// addressLength is unsigned int
			if (addressLength < 0)
			{
				addressLength = int.MaxValue;
			}
			sockAddrInternet.MaxSize = addressLength;
			sockAddrInternet.write(address);
			if (sockAddrInternet.@sizeof() < addressLength)
			{
				addressLengthAddr.setValue(sockAddrInternet.@sizeof());
			}

			return inetSocket.accept(sockAddrInternet);
		}

		// int sceNetInetBind(int socket, const struct sockaddr *address, socklen_t address_len);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1A33F9AE, version = 150) public int sceNetInetBind(@CheckArgument("checkSocket") int socket, pspsharp.HLE.kernel.types.pspNetSockAddrInternet sockAddrInternet, @CheckArgument("checkAddressLength") int addressLength)
		[HLEFunction(nid : 0x1A33F9AE, version : 150)]
		public virtual int sceNetInetBind(int socket, pspNetSockAddrInternet sockAddrInternet, int addressLength)
		{
			if (sockAddrInternet.sin_family != AF_INET)
			{
				log.warn(string.Format("sceNetInetBind invalid socket address family={0:D}", sockAddrInternet.sin_family));
				return -1;
			}

			pspInetSocket inetSocket = sockets[socket];
			int result = inetSocket.bind(sockAddrInternet);

			if (result == 0)
			{
				if (log.InfoEnabled)
				{
					log.info(string.Format("sceNetInetBind binding to {0}", sockAddrInternet.ToString()));
				}
			}
			else
			{
				log.warn(string.Format("sceNetInetBind failed binding to {0}", sockAddrInternet.ToString()));
			}

			return result;
		}

		// int sceNetInetClose(int s);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8D7284EA, version = 150) public int sceNetInetClose(@CheckArgument("checkSocket") int socket)
		[HLEFunction(nid : 0x8D7284EA, version : 150)]
		public virtual int sceNetInetClose(int socket)
		{
			pspInetSocket inetSocket = sockets[socket];
			int result = inetSocket.close();
			releaseSocketId(socket);
			sockets.Remove(socket);

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x805502DD, version = 150) public int sceNetInetCloseWithRST()
		[HLEFunction(nid : 0x805502DD, version : 150)]
		public virtual int sceNetInetCloseWithRST()
		{
			return 0;
		}

		// int sceNetInetConnect(int socket, const struct sockaddr *serv_addr, socklen_t addrlen);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x410B34AA, version = 150) public int sceNetInetConnect(@CheckArgument("checkSocket") int socket, pspsharp.HLE.kernel.types.pspNetSockAddrInternet sockAddrInternet, @CheckArgument("checkAddressLength") int addressLength)
		[HLEFunction(nid : 0x410B34AA, version : 150)]
		public virtual int sceNetInetConnect(int socket, pspNetSockAddrInternet sockAddrInternet, int addressLength)
		{
			if (sockAddrInternet.sin_family != AF_INET)
			{
				log.warn(string.Format("sceNetInetConnect invalid socket address family={0:D}", sockAddrInternet.sin_family));
				return -1;
			}

			getProxyForSockAddrInternet(sockAddrInternet);

			pspInetSocket inetSocket = sockets[socket];
			int result = inetSocket.connect(sockAddrInternet);

			if (result == 0)
			{
				if (log.InfoEnabled)
				{
					log.info(string.Format("sceNetInetConnect connected to {0}", sockAddrInternet.ToString()));
				}
			}
			else
			{
				if (Errno == EINPROGRESS)
				{
					if (log.InfoEnabled)
					{
						log.info(string.Format("sceNetInetConnect connecting to {0}", sockAddrInternet.ToString()));
					}
				}
				else
				{
					log.warn(string.Format("sceNetInetConnect failed connecting to {0} (errno={1:D})", sockAddrInternet.ToString(), Errno));
				}
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE247B6D6, version = 150) public int sceNetInetGetpeername(@CheckArgument("checkSocket") int socket, pspsharp.HLE.TPointer address, pspsharp.HLE.TPointer32 addressLengthAddr)
		[HLEFunction(nid : 0xE247B6D6, version : 150)]
		public virtual int sceNetInetGetpeername(int socket, TPointer address, TPointer32 addressLengthAddr)
		{
			pspNetSockAddrInternet sockAddrInternet = new pspNetSockAddrInternet();
			pspInetSocket inetSocket = sockets[socket];
			int result = inetSocket.getSockname(sockAddrInternet);

			sockAddrInternet.MaxSize = addressLengthAddr.getValue();
			sockAddrInternet.write(address);
			addressLengthAddr.setValue(sockAddrInternet.@sizeof());

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x162E6FD5, version = 150) public int sceNetInetGetsockname(@CheckArgument("checkSocket") int socket, pspsharp.HLE.TPointer address, pspsharp.HLE.TPointer32 addressLengthAddr)
		[HLEFunction(nid : 0x162E6FD5, version : 150)]
		public virtual int sceNetInetGetsockname(int socket, TPointer address, TPointer32 addressLengthAddr)
		{
			pspNetSockAddrInternet sockAddrInternet = new pspNetSockAddrInternet();
			pspInetSocket inetSocket = sockets[socket];
			int result = inetSocket.getSockname(sockAddrInternet);

			sockAddrInternet.MaxSize = addressLengthAddr.getValue();
			sockAddrInternet.write(address);
			addressLengthAddr.setValue(sockAddrInternet.@sizeof());

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x4A114C7C, version = 150) public int sceNetInetGetsockopt(@CheckArgument("checkSocket") int socket, int level, int optionName, pspsharp.HLE.TPointer optionValue, @CanBeNull pspsharp.HLE.TPointer32 optionLengthAddr)
		[HLEFunction(nid : 0x4A114C7C, version : 150)]
		public virtual int sceNetInetGetsockopt(int socket, int level, int optionName, TPointer optionValue, TPointer32 optionLengthAddr)
		{
			int optionLength = optionLengthAddr.Null ? 0 : optionLengthAddr.getValue();

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetInetGetsockopt optionName={0}, optionLength={1:D}", getOptionNameString(optionName), optionLength));
			}

			pspInetSocket inetSocket = sockets[socket];

			if (optionName == SO_ERROR && optionLength >= 4)
			{
				optionValue.setValue32(inetSocket.ErrorAndClearToSelf);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceNetInetGetsockopt SO_ERROR returning {0:D}", optionValue.getValue32()));
				}
				optionLengthAddr.setValue(4);
			}
			else if (optionName == SO_NONBLOCK && optionLength >= 4)
			{
				optionValue.setValue32(inetSocket.Blocking);
				optionLengthAddr.setValue(4);
			}
			else if (optionName == SO_BROADCAST && optionLength >= 4)
			{
				optionValue.setValue32(inetSocket.Broadcast);
				optionLengthAddr.setValue(4);
			}
			else if (optionName == SO_RCVLOWAT && optionLength >= 4)
			{
				optionValue.setValue32(inetSocket.ReceiveLowWaterMark);
				optionLengthAddr.setValue(4);
			}
			else if (optionName == SO_SNDLOWAT && optionLength >= 4)
			{
				optionValue.setValue32(inetSocket.SendLowWaterMark);
				optionLengthAddr.setValue(4);
			}
			else if (optionName == SO_RCVTIMEO && optionLength >= 4)
			{
				int timeout = inetSocket.ReceiveTimeout;
				// Returning 0 for "no timeout" value
				optionValue.setValue32(timeout == pspInetSocket.NO_TIMEOUT_INT ? 0 : timeout);
				optionLengthAddr.setValue(4);
			}
			else if (optionName == SO_SNDTIMEO && optionLength >= 4)
			{
				int timeout = inetSocket.SendTimeout;
				// Returning 0 for "no timeout" value
				optionValue.setValue32(timeout == pspInetSocket.NO_TIMEOUT_INT ? 0 : timeout);
				optionLengthAddr.setValue(4);
			}
			else if (optionName == SO_RCVBUF && optionLength >= 4)
			{
				optionValue.setValue32(inetSocket.ReceiveBufferSize);
				optionLengthAddr.setValue(4);
			}
			else if (optionName == SO_SNDBUF && optionLength >= 4)
			{
				optionValue.setValue32(inetSocket.SendBufferSize);
				optionLengthAddr.setValue(4);
			}
			else if (optionName == SO_KEEPALIVE && optionLength >= 4)
			{
				optionValue.setValue32(inetSocket.KeepAlive);
				optionLengthAddr.setValue(4);
			}
			else if (optionName == SO_LINGER && optionLength >= 8)
			{
				optionValue.setValue32(0, inetSocket.LingerEnabled);
				optionValue.setValue32(4, inetSocket.Linger);
				optionLengthAddr.setValue(8);
			}
			else if (optionName == SO_REUSEADDR && optionLength >= 4)
			{
				optionValue.setValue32(inetSocket.ReuseAddress);
				optionLengthAddr.setValue(4);
			}
			else
			{
				log.warn(string.Format("Unimplemented sceNetInetGetsockopt socket=0x{0:X}, level=0x{1:X}, optionName=0x{2:X}, optionValue={3}, optionLength={4}(0x{5:X})", socket, level, optionName, optionValue, optionLengthAddr, optionLength));
				return -1;
			}

			return 0;
		}

		// int sceNetInetListen(int s, int backlog);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD10A1A7A, version = 150) public int sceNetInetListen(@CheckArgument("checkSocket") int socket, int backlog)
		[HLEFunction(nid : 0xD10A1A7A, version : 150)]
		public virtual int sceNetInetListen(int socket, int backlog)
		{
			pspInetSocket inetSocket = sockets[socket];
			return inetSocket.listen(backlog);
		}

		/*
		 * sceNetInetPoll seems to work in a similar way to the BSD socket poll() function:
		 *
		 * int poll(struct pollfd *fds, nfds_t nfds, int timeout);
		 * fds      Points to an array of pollfd structures, which are defined as:
		 *            struct pollfd {
		 *                int fd;
		 *                short events;
		 *                short revents;
		 *            }
		 *          The fd member is an open file descriptor. If fd is -1, the
		 *          pollfd structure is considered unused, and revents will be
		 *          cleared.
		 *
		 *          The events and revents members are bitmasks of conditions to
		 *          monitor and conditions found, respectively.
		 *
		 * nfds     An unsigned integer specifying the number of pollfd structures
		 *          in the array.
		 *
		 * timeout  Maximum interval to wait for the poll to complete, in milliseconds.
		 *          If this value is 0, poll() will return immediately.
		 *          If this value is INFTIM (-1), poll() will block indefinitely until
		 *          a condition is found.
		 *
		 * The calling process sets the events bitmask and poll() sets the revents
		 * bitmask. Each call to poll() resets the revents bitmask for accuracy. The
		 * condition flags in the bitmasks are defined as:
		 *
		 * POLLIN      Data other than high-priority data may be read without blocking.
		 * POLLRDNORM  Normal data may be read without blocking.
		 * POLLRDBAND  Priority data may be read without blocking.
		 * POLLPRI     High-priority data may be read without blocking.
		 * POLLOUT     Normal data may be written without blocking.
		 * POLLWRBAND  Priority data may be written.
		 * POLLERR     An error has occurred on the device or socket. This flag is
		 *             only valid in the revents bitmask; it is ignored in the
		 *             events member.
		 * POLLHUP     The device or socket has been disconnected. This event and
		 *             POLLOUT are mutually-exclusive; a descriptor can never be
		 *             writable if a hangup has occurred. However, this event and
		 *             POLLIN, POLLRDNORM, POLLRDBAND, or POLLPRI are not mutually-
		 *             exclusive. This flag is only valid in the revents bitmask; it
		 *             is ignored in the events member.
		 * POLLNVAL    The corresponding file descriptor is invalid. This flag is
		 *             only valid in the revents bitmask; it is ignored in the
		 *             events member.
		 *
		 * Bitmask Values:
		 *   POLLIN     0x0001
		 *   POLLRDNORM 0x0040
		 *   POLLRDBAND 0x0080
		 *   POLLPRI    0x0002
		 *   POLLOUT    0x0004
		 *   POLLWRBAND 0x0100
		 *   POLLERR    0x0008
		 *   POLLHUP    0x0010
		 *   POLLNVAL   0x0020
		 *
		 * Return values:
		 *             Upon error, poll() returns -1 and sets the global variable errno
		 *             to indicate the error. If the timeout interval was reached before
		 *             any events occurred, poll() returns 0. Otherwise, poll() returns
		 *             the number of file descriptors for which revents is non-zero.
		 */
		[HLEFunction(nid : 0xFAABB1DD, version : 150)]
		public virtual int sceNetInetPoll(TPointer fds, int nfds, int timeout)
		{
			int result = 0;
			long timeoutUsec;
			if (timeout == POLL_INFTIM)
			{
				timeoutUsec = pspInetSocket.NO_TIMEOUT;
			}
			else
			{
				timeoutUsec = timeout * 1000L;
			}

			try
			{
				Selector selector = Selector.open();

				pspInetPollFd[] pollFds = new pspInetPollFd[nfds];
				for (int i = 0; i < nfds; i++)
				{
					pspInetPollFd pollFd = new pspInetPollFd();
					pollFd.read(fds, i * pollFd.@sizeof());
					pollFds[i] = pollFd;

					if (pollFd.fd == -1)
					{
						pollFd.revents = 0;
					}
					else
					{
						pspInetSocket inetSocket = sockets[pollFd.fd];
						if (inetSocket == null)
						{
							pollFd.revents = POLLNVAL;
						}
						else
						{
							SelectableChannel selectableChannel = inetSocket.SelectableChannel;
							if (selectableChannel == null)
							{
								pollFd.revents = POLLHUP;
							}
							else
							{
								int registeredOperations = 0;
								if ((pollFd.events & (POLLIN | POLLRDNORM | POLLRDBAND | POLLPRI)) != 0)
								{
									registeredOperations |= SelectionKey.OP_READ;
								}
								if ((pollFd.events & (POLLOUT | POLLWRBAND)) != 0)
								{
									registeredOperations |= SelectionKey.OP_WRITE;
								}
								registeredOperations &= selectableChannel.validOps();
								if (selectableChannel.Registered)
								{
									log.warn(string.Format("sceNetInetPoll channel already registered pollFd[{0:D}]={1}", i, pollFd));
								}
								else
								{
									try
									{
										selectableChannel.register(selector, registeredOperations, new int?(pollFd.fd));
										pollFd.revents = 0;
									}
									catch (ClosedChannelException)
									{
										pollFd.revents = POLLHUP;
									}
								}
							}
						}
					}

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetInetPoll pollFd[{0:D}]={1}", i, pollFd));
					}
				}

				BlockingPollState blockingState = new BlockingPollState(selector, pollFds, timeoutUsec);

				SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
				thread.cpuContext._v0 = 0; // This will be overwritten by the execution of the blockingState
				setErrno(0, blockingState);

				// Check if there are ready operations, otherwise, block the thread
				blockingState.execute();
				result = thread.cpuContext._v0;
			}
			catch (IOException e)
			{
				log.error("sceNetInetPoll", e);
				ErrnoToSelf = -1;
				return -1;
			}

			return result;
		}

		// size_t  sceNetInetRecv(int s, void *buf, size_t len, int flags);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xCDA85C99, version = 150) public int sceNetInetRecv(@CheckArgument("checkSocket") int socket, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.returnValue, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buffer, int bufferLength, int flags)
		[HLEFunction(nid : 0xCDA85C99, version : 150)]
		public virtual int sceNetInetRecv(int socket, TPointer buffer, int bufferLength, int flags)
		{
			pspInetSocket inetSocket = sockets[socket];
			return inetSocket.recv(buffer.Address, bufferLength, flags);
		}

		// size_t  sceNetInetRecvfrom(int socket, void *buffer, size_t bufferLength, int flags, struct sockaddr *from, socklen_t *fromlen);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC91142E4, version = 150) public int sceNetInetRecvfrom(@CheckArgument("checkSocket") int socket, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.returnValue, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buffer, int bufferLength, int flags, pspsharp.HLE.TPointer from, pspsharp.HLE.TPointer32 fromLengthAddr)
		[HLEFunction(nid : 0xC91142E4, version : 150)]
		public virtual int sceNetInetRecvfrom(int socket, TPointer buffer, int bufferLength, int flags, TPointer from, TPointer32 fromLengthAddr)
		{
			pspInetSocket inetSocket = sockets[socket];
			pspNetSockAddrInternet fromAddrInternet = new pspNetSockAddrInternet();

			int fromLength = fromLengthAddr.getValue();
			fromAddrInternet.MaxSize = fromLength;
			fromAddrInternet.write(from);
			if (fromAddrInternet.@sizeof() < fromLength)
			{
				fromLengthAddr.setValue(fromAddrInternet.@sizeof());
			}

			return inetSocket.recvfrom(buffer.Address, bufferLength, flags, fromAddrInternet);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEECE61D2, version = 150) public int sceNetInetRecvmsg()
		[HLEFunction(nid : 0xEECE61D2, version : 150)]
		public virtual int sceNetInetRecvmsg()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x5BE8D595, version = 150) public int sceNetInetSelect(int numberSockets, @CanBeNull pspsharp.HLE.TPointer readSocketsAddr, @CanBeNull pspsharp.HLE.TPointer writeSocketsAddr, @CanBeNull pspsharp.HLE.TPointer outOfBandSocketsAddr, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x5BE8D595, version : 150)]
		public virtual int sceNetInetSelect(int numberSockets, TPointer readSocketsAddr, TPointer writeSocketsAddr, TPointer outOfBandSocketsAddr, TPointer32 timeoutAddr)
		{
			int result = 0;
			numberSockets = System.Math.Min(numberSockets, 256);

			long timeoutUsec;
			if (timeoutAddr.NotNull)
			{
				// timeoutAddr points to the following structure:
				// - offset 0: int seconds
				// - offset 4: int microseconds
				timeoutUsec = timeoutAddr.getValue(0) * 1000000L;
				timeoutUsec += timeoutAddr.getValue(4);
			}
			else
			{
				// Take a very large value
				timeoutUsec = pspInetSocket.NO_TIMEOUT;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetInetSelect timeout={0:D} us", timeoutUsec));
				if (readSocketsAddr.NotNull)
				{
					log.debug(string.Format("sceNetInetSelect Read Sockets       : {0}", dumpSelectBits(readSocketsAddr, numberSockets)));
				}
				if (writeSocketsAddr.NotNull)
				{
					log.debug(string.Format("sceNetInetSelect Write Sockets      : {0}", dumpSelectBits(writeSocketsAddr, numberSockets)));
				}
				if (outOfBandSocketsAddr.NotNull)
				{
					log.debug(string.Format("sceNetInetSelect Out-of-band Sockets: {0}", dumpSelectBits(outOfBandSocketsAddr, numberSockets)));
				}
			}

			try
			{
				Selector selector = Selector.open();
				RawSelector rawSelector = RawSelector.open();

				int count = 0;

				// Read the socket list for the read operation and register them with the selector
				count += readSocketList(selector, rawSelector, readSocketsAddr, numberSockets, readSelectionKeyOperations, "readSockets");

				// Read the socket list for the write operation and register them with the selector
				count += readSocketList(selector, rawSelector, writeSocketsAddr, numberSockets, writeSelectionKeyOperations, "writeSockets");

				// Read the socket list for the out-of-band data and register them with the selector.
				// Out-of-band data is currently not implemented as I don't see any
				// support in Java for this rarely used feature.
				count += readSocketList(selector, rawSelector, outOfBandSocketsAddr, numberSockets, 0, "outOfBandSockets");

				BlockingSelectState blockingState = new BlockingSelectState(selector, rawSelector, numberSockets, readSocketsAddr, writeSocketsAddr, outOfBandSocketsAddr, timeoutUsec, count);

				setErrno(0, blockingState);
				SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
				thread.cpuContext._v0 = 0; // This will be overwritten by the execution of the blockingState

				// Check if there are ready operations, otherwise, block the thread
				blockingState.execute();
				result = thread.cpuContext._v0;
			}
			catch (IOException e)
			{
				log.error(e);
				ErrnoToSelf = -1;
				return -1;
			}

			return result;
		}

		// size_t sceNetInetSend(int socket, const void *buffer, size_t bufferLength, int flags);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7AA671BC, version = 150) public int sceNetInetSend(@CheckArgument("checkSocket") int socket, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer buffer, int bufferLength, int flags)
		[HLEFunction(nid : 0x7AA671BC, version : 150)]
		public virtual int sceNetInetSend(int socket, TPointer buffer, int bufferLength, int flags)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("Send data: {0}", Utilities.getMemoryDump(buffer.Address, bufferLength)));
			}

			pspInetSocket inetSocket = sockets[socket];
			return inetSocket.send(buffer.Address, bufferLength, flags);
		}

		// size_t sceNetInetSendto(int socket, const void *buffer, size_t bufferLength, int flags, const struct sockaddr *to, socklen_t tolen);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x05038FC7, version = 150) public int sceNetInetSendto(@CheckArgument("checkSocket") int socket, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer buffer, int bufferLength, int flags, pspsharp.HLE.kernel.types.pspNetSockAddrInternet toSockAddress, @CheckArgument("checkAddressLength") int toLength)
		[HLEFunction(nid : 0x05038FC7, version : 150)]
		public virtual int sceNetInetSendto(int socket, TPointer buffer, int bufferLength, int flags, pspNetSockAddrInternet toSockAddress, int toLength)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("Sendto data: {0}", Utilities.getMemoryDump(buffer.Address, bufferLength)));
			}

			if (toSockAddress.sin_family != AF_INET)
			{
				log.warn(string.Format("sceNetInetSendto invalid socket address familiy sin_family={0:D}", toSockAddress.sin_family));
				return -1;
			}
			pspInetSocket inetSocket = sockets[socket];
			return inetSocket.sendto(buffer.Address, bufferLength, flags, toSockAddress);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x774E36F4, version = 150) public int sceNetInetSendmsg()
		[HLEFunction(nid : 0x774E36F4, version : 150)]
		public virtual int sceNetInetSendmsg()
		{
			return 0;
		}

		// int sceNetInetSetsockopt(int socket, int level, int optname, const void *optval, socklen_t optlen);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x2FE71FE7, version = 150) public int sceNetInetSetsockopt(@CheckArgument("checkSocket") int socket, int level, int optionName, @CanBeNull pspsharp.HLE.TPointer optionValueAddr, int optionLength)
		[HLEFunction(nid : 0x2FE71FE7, version : 150)]
		public virtual int sceNetInetSetsockopt(int socket, int level, int optionName, TPointer optionValueAddr, int optionLength)
		{
			int result = 0;
			int optionValue = 0;
			if (optionValueAddr.NotNull && optionLength >= 4)
			{
				optionValue = optionValueAddr.getValue32();
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetInetSetsockopt optionName={0}", getOptionNameString(optionName)));
				if (log.TraceEnabled)
				{
					log.trace(string.Format("Option value: {0}", Utilities.getMemoryDump(optionValueAddr.Address, optionLength)));
				}
			}

			pspInetSocket inetSocket = sockets[socket];

			if (level == SOL_SOCKET)
			{
				if (optionName == SO_NONBLOCK && optionLength == 4)
				{
					result = inetSocket.setBlocking(optionValue == 0);
				}
				else if (optionName == SO_BROADCAST && optionLength == 4)
				{
					result = inetSocket.setBroadcast(optionValue != 0);
				}
				else if (optionName == SO_RCVLOWAT && optionLength == 4)
				{
					inetSocket.ReceiveLowWaterMark = optionValue;
					result = 0;
				}
				else if (optionName == SO_SNDLOWAT && optionLength == 4)
				{
					inetSocket.SendLowWaterMark = optionValue;
					result = 0;
				}
				else if (optionName == SO_RCVTIMEO && optionLength == 4)
				{
					// 0 means "no timeout"
					inetSocket.ReceiveTimeout = optionValue == 0 ? pspInetSocket.NO_TIMEOUT_INT : optionValue;
					result = 0;
				}
				else if (optionName == SO_SNDTIMEO && optionLength == 4)
				{
					// 0 means "no timeout"
					inetSocket.SendTimeout = optionValue == 0 ? pspInetSocket.NO_TIMEOUT_INT : optionValue;
					result = 0;
				}
				else if (optionName == SO_RCVBUF && optionLength == 4)
				{
					result = inetSocket.setReceiveBufferSize(optionValue);
				}
				else if (optionName == SO_SNDBUF && optionLength == 4)
				{
					result = inetSocket.setSendBufferSize(optionValue);
				}
				else if (optionName == SO_KEEPALIVE && optionLength == 4)
				{
					result = inetSocket.setKeepAlive(optionValue != 0);
				}
				else if (optionName == SO_LINGER && optionLength == 8)
				{
					result = inetSocket.setLinger(optionValue != 0, optionValueAddr.getValue32(4));
				}
				else if (optionName == SO_REUSEADDR && optionLength == 4)
				{
					result = inetSocket.setReuseAddress(optionValue != 0);
				}
				else
				{
					log.warn(string.Format("Unimplemented sceNetInetSetsockopt optionName={0}", getOptionNameString(optionName)));
					result = 0;
				}
			}
			else
			{
				log.warn(string.Format("Unimplemented sceNetInetSetsockopt unknown level=0x{0:X}, optionName={1}", level, getOptionNameString(optionName)));
				result = 0;
			}

			return result;
		}

		// int sceNetInetShutdown(int s, int how);
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x4CFE4E56, version = 150) public int sceNetInetShutdown(@CheckArgument("checkSocket") int socket, int how)
		[HLEFunction(nid : 0x4CFE4E56, version : 150)]
		public virtual int sceNetInetShutdown(int socket, int how)
		{
			if (how < SHUT_RD || how > SHUT_RDWR)
			{
				log.warn(string.Format("sceNetInetShutdown invalid how={0:D}", how));
				return -1;
			}

			pspInetSocket inetSocket = sockets[socket];
			return inetSocket.shutdown(how);
		}

		// int sceNetInetSocket(int domain, int type, int protocol);
		[HLEFunction(nid : 0x8B7B220F, version : 150)]
		public virtual int sceNetInetSocket(int domain, int type, int protocol)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetInetSocket domain=0x{0:X}, type=0x{1:X}({2}), protocol=0x{3:X}", domain, type, getSocketTypeNameString(type), protocol));
			}

			if (domain != AF_INET)
			{
				log.warn(string.Format("sceNetInetSocket unsupported domain=0x{0:X}, type=0x{1:X}({2}), protocol=0x{3:X}", domain, type, getSocketTypeNameString(type), protocol));
				return -1;
			}
			if (type != SOCK_DGRAM && type != SOCK_STREAM && type != SOCK_RAW && type != SOCK_DGRAM_UNKNOWN_6 && type != SOCK_STREAM_UNKNOWN_10)
			{
				log.warn(string.Format("sceNetInetSocket unsupported type=0x{0:X}({1}), domain=0x{2:X}, protocol=0x{3:X}", type, getSocketTypeNameString(type), domain, protocol));
				return -1;
			}

			pspInetSocket inetSocket = createSocket(type, protocol);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetInetSocket created socket=0x{0:X}", inetSocket.Uid));
			}

			return inetSocket.Uid;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x80A21ABD, version = 150) public int sceNetInetSocketAbort(@CheckArgument("checkSocket") int socket)
		[HLEFunction(nid : 0x80A21ABD, version : 150)]
		public virtual int sceNetInetSocketAbort(int socket)
		{
			return 0;
		}

		[HLEFunction(nid : 0xFBABE411, version : 150)]
		public virtual int sceNetInetGetErrno()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetInetGetErrno returning 0x{0:X8}({0:D})", Errno));
			}

			return Errno;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB3888AD4, version = 150) public int sceNetInetGetTcpcbstat(pspsharp.HLE.TPointer32 sizeAddr, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=26, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buf)
		[HLEFunction(nid : 0xB3888AD4, version : 150)]
		public virtual int sceNetInetGetTcpcbstat(TPointer32 sizeAddr, TPointer buf)
		{
			int tcpCount = 0;
			foreach (pspInetSocket socket in sockets.Values)
			{
				if (socket is pspInetStreamSocket)
				{
					tcpCount++;
				}
			}

			int size = sizeAddr.getValue();
			SceNetInetTcpcbstat stat = new SceNetInetTcpcbstat();
			sizeAddr.setValue(stat.@sizeof() * tcpCount);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetInetGetTcpcbstat returning size={0:D}", sizeAddr.getValue()));
			}

			if (buf.NotNull)
			{
				int offset = 0;
				foreach (pspInetSocket socket in sockets.Values)
				{
					if (socket is pspInetStreamSocket)
					{
						// Check if enough space available to write the next structure
						if (offset + stat.@sizeof() > size || socket == null)
						{
							break;
						}

						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceNetInetGetTcpcbstat returning {0} at 0x{1:X8}", socket, buf.Address + offset));
						}

						stat.ts_so_snd_sb_cc = 0;
						stat.ts_so_rcv_sb_cc = 0;

						pspNetSockAddrInternet localAddr = socket.LocalAddr;
						if (localAddr == null)
						{
							stat.ts_inp_laddr = 0;
							stat.ts_inp_lport = 0;
						}
						else
						{
							stat.ts_inp_laddr = localAddr.sin_addr;
							stat.ts_inp_lport = localAddr.sin_port;
						}

						pspNetSockAddrInternet remoteAddr = socket.RemoteAddr;
						if (remoteAddr == null)
						{
							stat.ts_inp_faddr = 0;
							stat.ts_inp_fport = 0;
						}
						else
						{
							stat.ts_inp_faddr = remoteAddr.sin_addr;
							stat.ts_inp_fport = remoteAddr.sin_port;
						}

						// TODO Find the correct socket state
						stat.ts_t_state = TCPS_ESTABLISHED;

						stat.write(buf, offset);

						offset += stat.@sizeof();
					}
				}

				fillNextPointersInLinkedList(buf, offset, stat.@sizeof());

				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceNetInetGetTcpcbstat returning {0}", Utilities.getMemoryDump(buf.Address, offset)));
				}
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x39B0C7D3, version = 150) public int sceNetInetGetUdpcbstat()
		[HLEFunction(nid : 0x39B0C7D3, version : 150)]
		public virtual int sceNetInetGetUdpcbstat()
		{
			return 0;
		}

		[HLEFunction(nid : 0xB75D5B0A, version : 150)]
		public virtual int sceNetInetInetAddr(PspString name)
		{
			sbyte[] inetAddressBytes;
			try
			{
				inetAddressBytes = InetAddress.getByName(name.String).Address;
			}
			catch (UnknownHostException e)
			{
				log.error("sceNetInetInetAddr", e);
				return -1;
			}

			int inetAddress = bytesToInternetAddress(inetAddressBytes);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetInetInetAddr {0} returning 0x{1:X8}", name, inetAddress));
			}

			return inetAddress;
		}

		[HLEFunction(nid : 0x1BDF5D13, version : 150)]
		public virtual int sceNetInetInetAton(PspString hostname, TPointer32 addr)
		{
			int result;
			try
			{
				InetAddress inetAddress = InetAddress.getByName(hostname.String);
				int resolvedAddress = bytesToInternetAddress(inetAddress.Address);
				addr.setValue(resolvedAddress);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceNetInetInetAton returning address 0x{0:X8}('{1}')", resolvedAddress, sceNetInet.internetAddressToString(resolvedAddress)));
				}
				else if (log.InfoEnabled)
				{
					log.info(string.Format("sceNetInetInetAton resolved '{0}' into '{1}'", hostname.String, sceNetInet.internetAddressToString(resolvedAddress)));
				}
				result = 1;
			}
			catch (UnknownHostException e)
			{
				log.error("sceNetInetInetAton", e);
				result = 0;
			}

			return result;
		}

		[HLEFunction(nid : 0xD0792666, version : 150)]
		public virtual int sceNetInetInetNtop(int family, TPointer32 srcAddr, TPointer buffer, int bufferLength)
		{
			if (family != AF_INET)
			{
				log.warn(string.Format("sceNetInetInetNtop unsupported family 0x{0:X}", family));
				return 0;
			}

			int addr = srcAddr.getValue();
			string ip = internetAddressToString(addr);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetInetInetNtop returning {0} for 0x{1:X8}", ip, addr));
			}
			buffer.setStringNZ(bufferLength, ip);

			return buffer.Address;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE30B8C19, version = 150) public int sceNetInetInetPton(int family, String src, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 buffer)
		[HLEFunction(nid : 0xE30B8C19, version : 150)]
		public virtual int sceNetInetInetPton(int family, string src, TPointer32 buffer)
		{
			int result;

			if (family != AF_INET)
			{
				log.warn(string.Format("sceNetInetInetPton unsupported family 0x{0:X}", family));
				return -1;
			}

			try
			{
				InetAddress inetAddress = InetAddress.getByName(src);
				sbyte[] inetAddressBytes = inetAddress.Address;
				int address = bytesToInternetAddress(inetAddressBytes);
				buffer.setValue(address);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceNetInetInetPton returning 0x{0:X8}({1}) for '{2}'", address, internetAddressToString(address), src));
				}
				result = 1;

				// Add this address to the proxy list in case we received
				// a new address from the DNS.
				addProxyInetAddress(src, inetAddress);
			}
			catch (UnknownHostException e)
			{
				log.warn(string.Format("sceNetInetInetPton returned error '{0}' for '{1}'", e.ToString(), src));
				result = 0;
			}

			return result;
		}

		[HLEFunction(nid : 0x8CA3A97E, version : 150)]
		public virtual int sceNetInetGetPspError()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetInetGetPspError returning 0x{0:X8}({0:D})", Errno));
			}

			return ERROR_ERRNO_BASE | (Errno & 0xFFFF);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAEE60F84, version = 150) public int sceNetInet_lib_AEE60F84(@CheckArgument("checkSocket") int socket, int unknown1, int unknown2)
		[HLEFunction(nid : 0xAEE60F84, version : 150)]
		public virtual int sceNetInet_lib_AEE60F84(int socket, int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAAF4895A, version = 150) public int sceNetInet_lib_AAF4895A(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.in) @CanBeNull pspsharp.HLE.TPointer unknown1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.in) @CanBeNull pspsharp.HLE.TPointer unknown2)
		[HLEFunction(nid : 0xAAF4895A, version : 150)]
		public virtual int sceNetInet_lib_AAF4895A(TPointer unknown1, TPointer unknown2)
		{
			unknown1.getArray8(this.unknown1);
			unknown2.getArray8(this.unknown2);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAC9D90A5, version = 150) public int sceNetInet_lib_AC9D90A5(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.out) @CanBeNull pspsharp.HLE.TPointer unknown1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.out) @CanBeNull pspsharp.HLE.TPointer unknown2)
		[HLEFunction(nid : 0xAC9D90A5, version : 150)]
		public virtual int sceNetInet_lib_AC9D90A5(TPointer unknown1, TPointer unknown2)
		{
			unknown1.Array = this.unknown1;
			unknown2.Array = this.unknown2;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE6F67D14, version = 150) public int sceNetInet_lib_E6F67D14(String interfaceName)
		[HLEFunction(nid : 0xE6F67D14, version : 150)]
		public virtual int sceNetInet_lib_E6F67D14(string interfaceName)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6A046357, version = 150) public int sceNetInet_lib_6A046357(String interfaceName)
		[HLEFunction(nid : 0x6A046357, version : 150)]
		public virtual int sceNetInet_lib_6A046357(string interfaceName)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5155EC8A, version = 150) public int sceNetInet_lib_5155EC8A(String interfaceName, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.out) @CanBeNull pspsharp.HLE.TPointer unknown1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.out) @CanBeNull pspsharp.HLE.TPointer unknown2, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.out) @CanBeNull pspsharp.HLE.TPointer unknown3)
		[HLEFunction(nid : 0x5155EC8A, version : 150)]
		public virtual int sceNetInet_lib_5155EC8A(string interfaceName, TPointer unknown1, TPointer unknown2, TPointer unknown3)
		{
			unknown1.clear(4);
			unknown2.clear(4);
			unknown3.clear(4);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA94A75E7, version = 150) public int sceNetInet_lib_A94A75E7(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer ipAddress)
		[HLEFunction(nid : 0xA94A75E7, version : 150)]
		public virtual int sceNetInet_lib_A94A75E7(TPointer ipAddress)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7776A492, version = 150) public int sceNetInet_lib_7776A492(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknown)
		[HLEFunction(nid : 0x7776A492, version : 150)]
		public virtual int sceNetInet_lib_7776A492(TPointer unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4F68DB0E, version = 150) public int sceNetInet_lib_4F68DB0E(String interfaceName, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.in) @CanBeNull pspsharp.HLE.TPointer ipAddress, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.in) @CanBeNull pspsharp.HLE.TPointer unknown, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.in) @CanBeNull pspsharp.HLE.TPointer subnetMask)
		[HLEFunction(nid : 0x4F68DB0E, version : 150)]
		public virtual int sceNetInet_lib_4F68DB0E(string interfaceName, TPointer ipAddress, TPointer unknown, TPointer subnetMask)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x59561561, version = 150) public int sceNetInet_lib_59561561(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=6, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknown1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknown2)
		[HLEFunction(nid : 0x59561561, version : 150)]
		public virtual int sceNetInet_lib_59561561(TPointer unknown1, TPointer unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8FE19FC4, version = 150) public int sceNetInet_lib_8FE19FC4(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=4, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown)
		[HLEFunction(nid : 0x8FE19FC4, version : 150)]
		public virtual int sceNetInet_lib_8FE19FC4(TPointer unknown)
		{
			unknown.clear(4);
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0238B6DF, version = 150) public int sceNetInet_lib_0238B6DF(int unknown1, int unknown2, int unknown3, int unknown4)
		[HLEFunction(nid : 0x0238B6DF, version : 150)]
		public virtual int sceNetInet_lib_0238B6DF(int unknown1, int unknown2, int unknown3, int unknown4)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4C13BE10, version = 150) public int sceNetInet_lib_4C13BE10()
		[HLEFunction(nid : 0x4C13BE10, version : 150)]
		public virtual int sceNetInet_lib_4C13BE10()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD609AD36, version = 150) public int sceNetInet_lib_D609AD36(int unknown1, int unknown2, int unknown3, int unknown4)
		[HLEFunction(nid : 0xD609AD36, version : 150)]
		public virtual int sceNetInet_lib_D609AD36(int unknown1, int unknown2, int unknown3, int unknown4)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDC38FEE9, version = 150) public int sceNetInet_lib_DC38FEE9()
		[HLEFunction(nid : 0xDC38FEE9, version : 150)]
		public virtual int sceNetInet_lib_DC38FEE9()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7CB1D9E3, version = 150) public int sceNetInet_lib_7CB1D9E3(int unknown)
		[HLEFunction(nid : 0x7CB1D9E3, version : 150)]
		public virtual int sceNetInet_lib_7CB1D9E3(int unknown)
		{
			return 0;
		}
	}
}