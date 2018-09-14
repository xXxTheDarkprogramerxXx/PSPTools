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
namespace pspsharp.network.adhoc
{

	using Modules = pspsharp.HLE.Modules;
	using TPointer = pspsharp.HLE.TPointer;
	using TPointer32 = pspsharp.HLE.TPointer32;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public abstract class PtpObject : PdpObject
	{
		/// <summary>
		/// Destination MAC address </summary>
		private pspNetMacAddress destMacAddress;
		/// <summary>
		/// Destination port </summary>
		private int destPort;
		/// <summary>
		/// Retry delay </summary>
		private int retryDelay;
		/// <summary>
		/// Retry count </summary>
		private int retryCount;
		/// <summary>
		/// Queue size </summary>
		private int queue;
		/// <summary>
		/// Bytes sent </summary>
		private int sentData;
		// Polling period (micro seconds) for blocking operations
		protected internal new const int BLOCKED_OPERATION_POLLING_MICROS = 10000;
		private AdhocMessage receivedMessage;
		private int receivedMessageOffset;

		protected internal abstract class BlockedPtpAction : BlockedPdpAction
		{
			protected internal readonly PtpObject ptpObject;

			protected internal BlockedPtpAction(PtpObject ptpObject, int timeout) : base(ptpObject, timeout)
			{
				this.ptpObject = ptpObject;
			}
		}

		protected internal class BlockedPtpAccept : BlockedPtpAction
		{
			internal readonly int peerMacAddr;
			internal readonly int peerPortAddr;

			public BlockedPtpAccept(PtpObject ptpObject, int peerMacAddr, int peerPortAddr, int timeout) : base(ptpObject, timeout)
			{
				this.peerMacAddr = peerMacAddr;
				this.peerPortAddr = peerPortAddr;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected boolean poll() throws java.io.IOException
			protected internal override bool poll()
			{
				return ptpObject.pollAccept(peerMacAddr, peerPortAddr, thread);
			}

			protected internal override int getExceptionResult(IOException e)
			{
				return SceKernelErrors.ERROR_NET_ADHOC_TIMEOUT;
			}
		}

		protected internal class BlockedPtpConnect : BlockedPtpAction
		{
			public BlockedPtpConnect(PtpObject ptpObject, int timeout) : base(ptpObject, timeout)
			{
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected boolean poll() throws java.io.IOException
			protected internal override bool poll()
			{
				return ptpObject.pollConnect(thread);
			}

			protected internal override int getExceptionResult(IOException e)
			{
				return SceKernelErrors.ERROR_NET_ADHOC_CONNECTION_REFUSED;
			}
		}

		protected internal class BlockedPtpRecv : BlockedPtpAction
		{
			protected internal readonly TPointer data;
			protected internal readonly TPointer32 dataLengthAddr;

			protected internal BlockedPtpRecv(PtpObject ptpObject, TPointer data, TPointer32 dataLengthAddr, int timeout) : base(ptpObject, timeout)
			{
				this.data = data;
				this.dataLengthAddr = dataLengthAddr;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected boolean poll() throws java.io.IOException
			protected internal override bool poll()
			{
				return ptpObject.pollRecv(data, dataLengthAddr, thread);
			}

			protected internal override int getExceptionResult(IOException e)
			{
				return SceKernelErrors.ERROR_NET_ADHOC_TIMEOUT;
			}

		}

		public PtpObject(PtpObject ptpObject) : base(ptpObject)
		{
			destMacAddress = ptpObject.destMacAddress;
			destPort = ptpObject.destPort;
			retryDelay = ptpObject.retryDelay;
			retryCount = ptpObject.retryCount;
			queue = ptpObject.queue;
		}

		public PtpObject(INetworkAdapter networkAdapter) : base(networkAdapter)
		{
		}

		public virtual pspNetMacAddress DestMacAddress
		{
			get
			{
				return destMacAddress;
			}
			set
			{
				this.destMacAddress = value;
			}
		}


		public virtual int DestPort
		{
			get
			{
				return destPort;
			}
			set
			{
				this.destPort = value;
			}
		}


		public virtual int RetryDelay
		{
			get
			{
				return retryDelay;
			}
			set
			{
				this.retryDelay = value;
			}
		}


		public virtual int RetryCount
		{
			get
			{
				return retryCount;
			}
			set
			{
				this.retryCount = value;
			}
		}


		public virtual int Queue
		{
			get
			{
				return queue;
			}
			set
			{
				this.queue = value;
			}
		}


		public virtual int SentData
		{
			get
			{
				return sentData;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void openSocket() throws java.net.UnknownHostException, java.io.IOException
		public override void openSocket()
		{
			if (socket == null)
			{
				base.openSocket();
				if (DestMacAddress != null)
				{
					int realDestPort = Modules.sceNetAdhocModule.getRealPortFromClientPort(DestMacAddress.macAddress, DestPort);
					SocketAddress socketAddress = Modules.sceNetAdhocModule.getSocketAddress(DestMacAddress.macAddress, realDestPort);
					if (log.TraceEnabled)
					{
						log.trace(string.Format("Ptp openSocket address={0}, port={1:D}", socketAddress, realDestPort));
					}
					socket.connect(socketAddress, realDestPort);
				}
			}
		}

		public virtual int open()
		{
			int result = 0;

			try
			{
				openSocket();
			}
			catch (BindException e)
			{
				if (log.DebugEnabled)
				{
					log.debug("open", e);
				}
				result = SceKernelErrors.ERROR_NET_ADHOC_PORT_IN_USE;
			}
			catch (ConnectException e)
			{
				if (log.DebugEnabled)
				{
					log.debug("open", e);
				}
				result = SceKernelErrors.ERROR_NET_ADHOC_INVALID_ARG;
			}
			catch (SocketException e)
			{
				log.error("open", e);
				result = SceKernelErrors.ERROR_NET_ADHOC_INVALID_ARG;
			}
			catch (UnknownHostException e)
			{
				log.error("open", e);
				result = SceKernelErrors.ERROR_NET_ADHOC_INVALID_ARG;
			}
			catch (IOException e)
			{
				log.error("open", e);
				result = SceKernelErrors.ERROR_NET_ADHOC_INVALID_ARG;
			}

			return result;
		}

		public virtual int listen()
		{
			int result = 0;

			try
			{
				openSocket();
			}
			catch (BindException e)
			{
				if (log.DebugEnabled)
				{
					log.debug("listen", e);
				}
				result = SceKernelErrors.ERROR_NET_ADHOC_PORT_IN_USE;
			}
			catch (SocketException e)
			{
				log.error("listen", e);
			}
			catch (UnknownHostException e)
			{
				log.error("listen", e);
			}
			catch (IOException e)
			{
				log.error("listen", e);
			}

			return result;
		}

		public virtual int accept(int peerMacAddr, int peerPortAddr, int timeout, int nonblock)
		{
			int result = 0;

			SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
			if (pollAccept(peerMacAddr, peerPortAddr, thread))
			{
				// Accept completed immediately
				result = thread.cpuContext._v0;
			}
			else if (nonblock != 0)
			{
				// Accept cannot be completed in non-blocking mode
				result = SceKernelErrors.ERROR_NET_ADHOC_NO_DATA_AVAILABLE;
			}
			else
			{
				// Block current thread
				BlockedPtpAction blockedPtpAction = new BlockedPtpAccept(this, peerMacAddr, peerPortAddr, timeout);
				blockedPtpAction.blockCurrentThread();
			}

			return result;
		}

		public virtual int connect(int timeout, int nonblock)
		{
			int result = 0;

			if (!pollConnect(Modules.ThreadManForUserModule.CurrentThread))
			{
				if (nonblock != 0)
				{
					result = SceKernelErrors.ERROR_NET_ADHOC_NO_DATA_AVAILABLE;
				}
				else
				{
					BlockedPtpAction blockedPtpAction = new BlockedPtpConnect(this, timeout);
					blockedPtpAction.blockCurrentThread();
				}
			}

			return result;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void send(AdhocMessage adhocMessage) throws java.io.IOException
		public override void send(AdhocMessage adhocMessage)
		{
			adhocMessage.FromMacAddress = MacAddress.macAddress;
			adhocMessage.ToMacAddress = DestMacAddress.macAddress;
			send(adhocMessage, DestPort);
		}

		public virtual int send(int data, TPointer32 dataSizeAddr, int timeout, int nonblock)
		{
			int result = 0;

			try
			{
				AdhocMessage adhocMessage = networkAdapter.createAdhocPtpMessage(data, dataSizeAddr.getValue());
				send(adhocMessage);
			}
			catch (IOException e)
			{
				result = SceKernelErrors.ERROR_NET_ADHOC_DISCONNECTED;
				log.error("send returning ERROR_NET_ADHOC_DISCONNECTED", e);
			}

			return result;
		}

		// For Ptp sockets, data in read as a byte stream. Data is not organized in packets.
		// Read as much data as the provided buffer can contain.
		public virtual int recv(TPointer data, TPointer32 dataLengthAddr, int timeout, int nonblock)
		{
			int result = 0;

			try
			{
				SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
				if (pollRecv(data, dataLengthAddr, thread))
				{
					// Recv completed immediately
					result = thread.cpuContext._v0;
				}
				else if (nonblock != 0)
				{
					// Recv cannot be completed in non-blocking mode
					result = SceKernelErrors.ERROR_NET_ADHOC_NO_DATA_AVAILABLE;
				}
				else
				{
					// Block current thread
					BlockedPdpAction blockedPdpAction = new BlockedPtpRecv(this, data, dataLengthAddr, timeout);
					blockedPdpAction.blockCurrentThread();
				}
			}
			catch (IOException e)
			{
				result = SceKernelErrors.ERROR_NET_ADHOC_DISCONNECTED;
				log.error("recv", e);
			}

			return result;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected boolean pollRecv(pspsharp.HLE.TPointer data, pspsharp.HLE.TPointer32 dataLengthAddr, pspsharp.HLE.kernel.types.SceKernelThreadInfo thread) throws java.io.IOException
		protected internal virtual bool pollRecv(TPointer data, TPointer32 dataLengthAddr, SceKernelThreadInfo thread)
		{
			int length = dataLengthAddr.getValue();
			bool completed = false;

			if (length > 0)
			{
				if (RcvdData <= 0 || receivedMessage != null)
				{
					update();
				}

				if (RcvdData > 0)
				{
					if (length > RcvdData)
					{
						length = RcvdData;
					}
					// Copy the data already received
					dataLengthAddr.setValue(length);
					Memory mem = Memory.Instance;
					mem.memcpy(data.Address, buffer.addr, length);
					if (RcvdData > length)
					{
						// Shift the remaining buffer data to the beginning of the buffer
						mem.memmove(buffer.addr, buffer.addr + length, RcvdData - length);
					}
					rcvdData -= length;

					if (log.DebugEnabled)
					{
						log.debug(string.Format("Returned received data: {0:D} bytes", length));
						if (log.TraceEnabled)
						{
							log.trace(string.Format("Returned data: {0}", Utilities.getMemoryDump(data.Address, length)));
						}
					}
					setReturnValue(thread, 0);
					completed = true;
				}
			}

			return completed;
		}

		// For Ptp sockets, data is stored in the internal buffer as a continuous byte stream.
		// The organization in packets doesn't matter.
		private int addReceivedMessage(AdhocMessage adhocMessage, int offset)
		{
			int length = System.Math.Min(adhocMessage.DataLength - offset, BufSize - RcvdData);
			int addr = buffer.addr + RcvdData;
			adhocMessage.writeDataToMemory(addr, offset, length);
			rcvdData += length;
			if (log.DebugEnabled)
			{
				if (offset == 0)
				{
					log.debug(string.Format("Successfully received message (length={0:D}, rcvdData={1:D}) {2}", length, RcvdData, adhocMessage));
				}
				else
				{
					log.debug(string.Format("Appending received message (offset={0:D}, length={1:D}, rcvdData={2:D}) {3}", offset, length, RcvdData, adhocMessage));
				}
				if (log.TraceEnabled)
				{
					log.trace(string.Format("Message data: {0}", Utilities.getMemoryDump(addr, length)));
				}
			}

			return length;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void update() throws java.io.IOException
		public override void update()
		{
			// Receive all messages available
			while (RcvdData < BufSize)
			{
				if (receivedMessage != null)
				{
					receivedMessageOffset += addReceivedMessage(receivedMessage, receivedMessageOffset);
					if (receivedMessageOffset >= receivedMessage.DataLength)
					{
						receivedMessage = null;
						receivedMessageOffset = 0;
					}
				}
				else
				{
					try
					{
						openSocket();
						socket.Timeout = 1;
						sbyte[] bytes = new sbyte[0x10000]; // 64K buffer
						int length = socket.receive(bytes, bytes.Length);
						if (length <= 0)
						{
							break;
						}
						int receivedPort = socket.ReceivedPort;
						InetAddress receivedAddress = socket.ReceivedAddress;
						AdhocMessage adhocMessage = createAdhocMessage(bytes, length);
						if (isForMe(adhocMessage, receivedPort, receivedAddress))
						{
							receivedMessage = adhocMessage;
							receivedMessageOffset = 0;
						}
						else
						{
							if (log.DebugEnabled)
							{
								log.debug(string.Format("Received message not for me: {0}", adhocMessage));
							}
						}
	//				} catch (SocketException e) {
	//					log.error("update", e);
	//					break;
					}
					catch (SocketTimeoutException)
					{
						// Timeout
						break;
					}
				}
			}
		}

		protected internal override AdhocMessage createAdhocMessage(sbyte[] message, int length)
		{
			return networkAdapter.createAdhocPtpMessage(message, length);
		}

		protected internal abstract bool pollAccept(int peerMacAddr, int peerPortAddr, SceKernelThreadInfo thread);
		protected internal abstract bool pollConnect(SceKernelThreadInfo thread);
		public abstract bool canAccept();
		public abstract bool canConnect();

		public override string ToString()
		{
			return string.Format("PtpObject[id=0x{0:X}, srcMacAddress={1}, srcPort={2:D}, destMacAddress={3}, destPort={4:D}, bufSize={5:D}, retryDelay={6:D}, retryCount={7:D}, queue={8:D}, rcvdData={9:D}]", Id, MacAddress, Port, DestMacAddress, DestPort, BufSize, RetryDelay, RetryCount, Queue, RcvdData);
		}
	}

}