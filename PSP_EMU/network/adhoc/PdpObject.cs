using System.Collections.Generic;

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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.adhoc.AdhocMessage.MAX_HEADER_SIZE;


	using Modules = pspsharp.HLE.Modules;
	using TPointer = pspsharp.HLE.TPointer;
	using TPointer16 = pspsharp.HLE.TPointer16;
	using TPointer32 = pspsharp.HLE.TPointer32;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public abstract class PdpObject : AdhocObject
	{
		/// <summary>
		/// MAC address </summary>
		private pspNetMacAddress macAddress;
		/// <summary>
		/// Bytes received </summary>
		protected internal int rcvdData;
		private LinkedList<AdhocBufferMessage> rcvdMessages = new LinkedList<AdhocBufferMessage>();
		// Polling period (micro seconds) for blocking operations
		protected internal const int BLOCKED_OPERATION_POLLING_MICROS = 10000;

		protected internal abstract class BlockedPdpAction : IAction
		{
			protected internal readonly PdpObject pdpObject;
			protected internal readonly long timeoutMicros;
			protected internal readonly int threadUid;
			protected internal readonly SceKernelThreadInfo thread;

			public BlockedPdpAction(PdpObject pdpObject, long timeout)
			{
				this.pdpObject = pdpObject;
				timeoutMicros = Emulator.Clock.microTime() + timeout;
				threadUid = Modules.ThreadManForUserModule.CurrentThreadID;
				thread = Modules.ThreadManForUserModule.getThreadById(threadUid);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("BlockedPdpAction for thread {0}", thread));
				}
			}

			public virtual void blockCurrentThread()
			{
				long schedule = Emulator.Clock.microTime() + BLOCKED_OPERATION_POLLING_MICROS;
				Emulator.Scheduler.addAction(schedule, this);
				Modules.ThreadManForUserModule.hleBlockCurrentThread(SceKernelThreadInfo.JPCSP_WAIT_NET);
			}

			public virtual void execute()
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("BlockedPdpAction: poll on {0}, thread {1}", pdpObject, thread));
				}

				try
				{
					if (poll())
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("BlockedPdpAction: unblocking thread {0}", thread));
						}
						Modules.ThreadManForUserModule.hleUnblockThread(threadUid);
					}
					else
					{
						long now = Emulator.Clock.microTime();
						if (now >= timeoutMicros)
						{
							if (log.DebugEnabled)
							{
								log.debug(string.Format("BlockedPdpAction: timeout for thread {0}", thread));
							}
							// Unblock thread and return timeout error
							setReturnValue(thread, SceKernelErrors.ERROR_NET_ADHOC_TIMEOUT);
							Modules.ThreadManForUserModule.hleUnblockThread(threadUid);
						}
						else
						{
							if (log.DebugEnabled)
							{
								log.debug(string.Format("BlockedPdpAction: continue polling"));
							}
							long schedule = now + BLOCKED_OPERATION_POLLING_MICROS;
							Emulator.Scheduler.addAction(schedule, this);
						}
					}
				}
				catch (IOException e)
				{
					setReturnValue(thread, getExceptionResult(e));
					log.error(this.GetType().Name, e);
				}
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected abstract boolean poll() throws java.io.IOException;
			protected internal abstract bool poll();
			protected internal abstract int getExceptionResult(IOException e);
		}

		protected internal class BlockedPdpRecv : BlockedPdpAction
		{
			protected internal readonly TPointer srcMacAddr;
			protected internal readonly TPointer16 portAddr;
			protected internal readonly TPointer data;
			protected internal readonly TPointer32 dataLengthAddr;

			public BlockedPdpRecv(PdpObject pdpObject, TPointer srcMacAddr, TPointer16 portAddr, TPointer data, TPointer32 dataLengthAddr, long timeout) : base(pdpObject, timeout)
			{
				this.srcMacAddr = srcMacAddr;
				this.portAddr = portAddr;
				this.data = data;
				this.dataLengthAddr = dataLengthAddr;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected boolean poll() throws java.io.IOException
			protected internal override bool poll()
			{
				return pdpObject.pollRecv(srcMacAddr, portAddr, data, dataLengthAddr, thread);
			}

			protected internal override int getExceptionResult(IOException e)
			{
				return SceKernelErrors.ERROR_NET_ADHOC_TIMEOUT;
			}
		}

		public PdpObject(INetworkAdapter networkAdapter) : base(networkAdapter)
		{
		}

		public PdpObject(PdpObject pdpObject) : base(pdpObject)
		{
			macAddress = pdpObject.macAddress;
		}

		public virtual pspNetMacAddress MacAddress
		{
			get
			{
				return macAddress;
			}
			set
			{
				this.macAddress = value;
			}
		}


		public virtual int RcvdData
		{
			get
			{
				return rcvdData;
			}
		}

		public virtual int create(pspNetMacAddress macAddress, int port, int bufSize)
		{
			int result = Id;

			MacAddress = macAddress;
			Port = port;
			BufSize = bufSize;
			try
			{
				openSocket();
			}
			catch (BindException e)
			{
				if (log.DebugEnabled)
				{
					log.debug("create", e);
				}
				result = SceKernelErrors.ERROR_NET_ADHOC_PORT_IN_USE;
			}
			catch (SocketException e)
			{
				log.error("create", e);
			}
			catch (UnknownHostException e)
			{
				log.error("create", e);
			}
			catch (IOException e)
			{
				log.error("create", e);
			}

			return result;
		}

		public virtual int send(pspNetMacAddress destMacAddress, int destPort, TPointer data, int length, int timeout, int nonblock)
		{
			int result = 0;

			try
			{
				openSocket();
				setTimeout(timeout, nonblock);
				AdhocMessage adhocMessage = networkAdapter.createAdhocPdpMessage(data.Address, length, destMacAddress.macAddress);
				send(adhocMessage, destPort);
			}
			catch (SocketException e)
			{
				log.error("send", e);
			}
			catch (UnknownHostException e)
			{
				result = SceKernelErrors.ERROR_NET_ADHOC_INVALID_ADDR;
				log.error("send", e);
			}
			catch (SocketTimeoutException e)
			{
				log.error("send", e);
			}
			catch (IOException e)
			{
				log.error("send", e);
			}

			return result;
		}

		// For Pdp sockets, data is stored in the internal buffer as a sequence of packets.
		// The organization in packets must be kept for reading.
		private void addReceivedMessage(AdhocMessage adhocMessage, int port)
		{
			AdhocBufferMessage bufferMessage = new AdhocBufferMessage();
			bufferMessage.length = adhocMessage.DataLength;
			bufferMessage.macAddress.MacAddress = adhocMessage.FromMacAddress;
			bufferMessage.port = Modules.sceNetAdhocModule.getClientPortFromRealPort(adhocMessage.FromMacAddress, port);
			bufferMessage.offset = rcvdData;
			adhocMessage.writeDataToMemory(buffer.addr + bufferMessage.offset);

			// Update the timestamp of the peer
			Modules.sceNetAdhocctlModule.hleNetAdhocctlPeerUpdateTimestamp(adhocMessage.FromMacAddress);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Successfully received {0:D} bytes from {1} on port {2:D}({3:D})", bufferMessage.length, bufferMessage.macAddress, bufferMessage.port, port));
				if (log.TraceEnabled)
				{
					log.trace(string.Format("Message data: {0}", Utilities.getMemoryDump(buffer.addr + bufferMessage.offset, bufferMessage.length)));
				}
			}

			rcvdData += bufferMessage.length;
			rcvdMessages.AddLast(bufferMessage);
		}

		private void removeFirstReceivedMessage()
		{
			AdhocBufferMessage bufferMessage = rcvdMessages.RemoveFirst();
			if (bufferMessage == null)
			{
				return;
			}

			if (rcvdData > bufferMessage.length)
			{
				// Move the remaining buffer data to the beginning of the buffer
				Memory.Instance.memcpy(buffer.addr, buffer.addr + bufferMessage.length, rcvdData - bufferMessage.length);
				foreach (AdhocBufferMessage rcvdMessage in rcvdMessages)
				{
					rcvdMessage.offset -= bufferMessage.length;
				}
			}
			rcvdData -= bufferMessage.length;
		}

		// For Pdp sockets, data is read one packet at a time.
		// The caller has to provide enough space to fully read the available packet.
		public virtual int recv(TPointer srcMacAddr, TPointer16 portAddr, TPointer data, TPointer32 dataLengthAddr, int timeout, int nonblock)
		{
			int result = 0;

			try
			{
				SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
				if (pollRecv(srcMacAddr, portAddr, data, dataLengthAddr, thread))
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
					BlockedPdpAction blockedPdpAction = new BlockedPdpRecv(this, srcMacAddr, portAddr, data, dataLengthAddr, timeout);
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
//ORIGINAL LINE: public boolean pollRecv(pspsharp.HLE.TPointer srcMacAddr, pspsharp.HLE.TPointer16 portAddr, pspsharp.HLE.TPointer data, pspsharp.HLE.TPointer32 dataLengthAddr, pspsharp.HLE.kernel.types.SceKernelThreadInfo thread) throws java.io.IOException
		public virtual bool pollRecv(TPointer srcMacAddr, TPointer16 portAddr, TPointer data, TPointer32 dataLengthAddr, SceKernelThreadInfo thread)
		{
			int length = dataLengthAddr.getValue();
			bool completed = false;

			if (rcvdMessages.Count == 0)
			{
				update();
			}

			if (rcvdMessages.Count > 0)
			{
				AdhocBufferMessage bufferMessage = rcvdMessages.First.Value;
				if (length < bufferMessage.length)
				{
					// Buffer is too small to contain all the available data.
					// Return the buffer size that would be required.
					dataLengthAddr.setValue(bufferMessage.length);
					setReturnValue(thread, SceKernelErrors.ERROR_NET_BUFFER_TOO_SMALL);
				}
				else
				{
					// Copy the data already received
					dataLengthAddr.setValue(bufferMessage.length);
					Memory.Instance.memcpy(data.Address, buffer.addr + bufferMessage.offset, bufferMessage.length);
					if (srcMacAddr != null && !srcMacAddr.Null)
					{
						bufferMessage.macAddress.write(srcMacAddr);
					}
					if (portAddr != null && portAddr.NotNull)
					{
						portAddr.Value = bufferMessage.port;
					}

					removeFirstReceivedMessage();

					if (log.DebugEnabled)
					{
						log.debug(string.Format("Returned received data: {0:D} bytes from {1} on port {2:D}", dataLengthAddr.getValue(), bufferMessage.macAddress, portAddr.Value));
						if (log.TraceEnabled)
						{
							log.trace(string.Format("Returned data: {0}", Utilities.getMemoryDump(data.Address, dataLengthAddr.getValue())));
						}
					}
					setReturnValue(thread, 0);
				}
				completed = true;
			}

			return completed;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void update() throws java.io.IOException
		public virtual void update()
		{
			// Receive all messages available
			while (rcvdData < BufSize)
			{
				try
				{
					openSocket();
					socket.Timeout = 1;
					sbyte[] bytes = new sbyte[BufSize - rcvdData + MAX_HEADER_SIZE];
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
						if (RcvdData + adhocMessage.DataLength <= BufSize)
						{
							addReceivedMessage(adhocMessage, receivedPort);
						}
						else
						{
							if (log.DebugEnabled)
							{
								log.debug(string.Format("Discarded message, receive buffer full ({0:D} of {1:D}): {2}", RcvdData, BufSize, adhocMessage));
							}
						}
					}
					else
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Received message not for me: {0}", adhocMessage));
						}
					}
				}
				catch (SocketException e)
				{
					log.error("update", e);
					break;
				}
				catch (SocketTimeoutException)
				{
					// Timeout
					break;
				}
			}
		}

		protected internal virtual AdhocMessage createAdhocMessage(sbyte[] message, int length)
		{
			return networkAdapter.createAdhocPdpMessage(message, length);
		}

		protected internal virtual bool isForMe(AdhocMessage adhocMessage, int port, InetAddress address)
		{
			return adhocMessage.ForMe;
		}

		protected internal static void setReturnValue(SceKernelThreadInfo thread, int value)
		{
			thread.cpuContext._v0 = value;
		}

		public override string ToString()
		{
			return string.Format("PdpObject[id=0x{0:X}, macAddress={1}, port={2:D}, bufSize={3:D}, rcvdData={4:D}]", Id, macAddress, Port, BufSize, rcvdData);
		}
	}

}