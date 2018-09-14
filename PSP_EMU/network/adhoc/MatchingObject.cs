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
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_ACCEPT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_CANCEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_COMPLETE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_DATA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_DATA_CONFIRM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_DISCONNECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_HELLO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_INTERNAL_PING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_JOIN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_LEFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_MODE_CLIENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.adhoc.AdhocMessage.MAX_HEADER_SIZE;


	using Modules = pspsharp.HLE.Modules;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using SysMemUserForUser = pspsharp.HLE.modules.SysMemUserForUser;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;
	using sceNetAdhoc = pspsharp.HLE.modules.sceNetAdhoc;
	using sceNetAdhocMatching = pspsharp.HLE.modules.sceNetAdhocMatching;
	using Wlan = pspsharp.hardware.Wlan;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public abstract class MatchingObject : AdhocObject
	{
		private int mode;
		private int maxPeers;
		private int helloDelay;
		private int pingDelay;
		private int initCount;
		private int msgDelay;
		private int callback;
		private bool started;
		private long lastHelloMicros;
		private long lastPingMicros;
		private sbyte[] helloOptData;
		private SceKernelThreadInfo eventThread;
		private SceKernelThreadInfo inputThread;
		private sbyte[] pendingJoinRequest;
		private bool inConnection;
		private bool connected;
		private bool pendingComplete;
		private LinkedList<pspNetMacAddress> members = new LinkedList<pspNetMacAddress>();
		private LinkedList<CallbackEvent> pendingCallbackEvents = new LinkedList<MatchingObject.CallbackEvent>();

		private class CallbackEvent
		{
			internal int @event;
			internal pspNetMacAddress macAddress;
			internal int optLen;
			internal int optData;

			public CallbackEvent(int @event, int macAddr, int optLen, int optData)
			{
				this.@event = @event;
				macAddress = new pspNetMacAddress();
				macAddress.read(Memory.Instance, macAddr);
				this.optLen = optLen;
				this.optData = optData;
			}

			public virtual int Event
			{
				get
				{
					return @event;
				}
			}

			public virtual pspNetMacAddress MacAddress
			{
				get
				{
					return macAddress;
				}
			}

			public virtual int OptLen
			{
				get
				{
					return optLen;
				}
			}

			public virtual int OptData
			{
				get
				{
					return optData;
				}
			}
		}

		public MatchingObject(INetworkAdapter networkAdapter) : base(networkAdapter)
		{
		}

		public virtual int Mode
		{
			get
			{
				return mode;
			}
			set
			{
				this.mode = value;
			}
		}


		public virtual int MaxPeers
		{
			get
			{
				return maxPeers;
			}
			set
			{
				this.maxPeers = value;
			}
		}


		public virtual int HelloDelay
		{
			get
			{
				return helloDelay;
			}
			set
			{
				this.helloDelay = value;
			}
		}


		public virtual int PingDelay
		{
			get
			{
				return pingDelay;
			}
			set
			{
				this.pingDelay = value;
			}
		}


		public virtual int InitCount
		{
			get
			{
				return initCount;
			}
			set
			{
				this.initCount = value;
			}
		}


		public virtual int MsgDelay
		{
			get
			{
				return msgDelay;
			}
			set
			{
				this.msgDelay = value;
			}
		}


		public virtual int Callback
		{
			get
			{
				return callback;
			}
			set
			{
				this.callback = value;
			}
		}


		public virtual IList<pspNetMacAddress> Members
		{
			get
			{
				return members;
			}
		}

		public virtual void create()
		{
		}

		public virtual int start(int evthPri, int evthStack, int inthPri, int inthStack, int optLen, int optData)
		{
			try
			{
				setHelloOpt(optLen, optData);
				openSocket();

				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				if (eventThread == null)
				{
					eventThread = threadMan.hleKernelCreateThread("SceNetAdhocMatchingEvent", ThreadManForUser.NET_ADHOC_MATCHING_EVENT_LOOP_ADDRESS, evthPri, evthStack, threadMan.CurrentThread.attr, 0, SysMemUserForUser.USER_PARTITION_ID);
					threadMan.hleKernelStartThread(eventThread, 0, 0, eventThread.gpReg_addr);
					eventThread.cpuContext.setRegister(sceNetAdhocMatching.loopThreadRegisterArgument, Id);
				}
				if (inputThread == null)
				{
					inputThread = threadMan.hleKernelCreateThread("SceNetAdhocMatchingInput", ThreadManForUser.NET_ADHOC_MATCHING_INPUT_LOOP_ADDRESS, inthPri, inthStack, threadMan.CurrentThread.attr, 0, SysMemUserForUser.USER_PARTITION_ID);
					threadMan.hleKernelStartThread(inputThread, 0, 0, inputThread.gpReg_addr);
					inputThread.cpuContext.setRegister(sceNetAdhocMatching.loopThreadRegisterArgument, Id);
				}

				// Add myself as the first member
				addMember(Wlan.MacAddress);

				started = true;
			}
			catch (SocketException e)
			{
				log.error("start", e);
			}
			catch (UnknownHostException e)
			{
				log.error("start", e);
			}
			catch (IOException e)
			{
				log.error("start", e);
			}

			return 0;
		}

		public virtual int stop()
		{
			if (connected)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Sending disconnect to port {0:D}", Port));
				}

				try
				{
					AdhocMatchingEventMessage adhocMatchingEventMessage = createMessage(PSP_ADHOC_MATCHING_EVENT_DISCONNECT);
					send(adhocMatchingEventMessage);
				}
				catch (IOException e)
				{
					log.error("stop", e);
				}
			}

			closeSocket();
			removeMember(Wlan.MacAddress);

			return 0;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendHello() throws java.io.IOException
		private void sendHello()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Sending hello to port {0:D}", Port));
			}
			AdhocMatchingEventMessage adhocMatchingEventMessage = createMessage(PSP_ADHOC_MATCHING_EVENT_HELLO);
			if (HelloOptLen > 0)
			{
				adhocMatchingEventMessage.Data = helloOptData;
			}
			send(adhocMatchingEventMessage);

			lastHelloMicros = Emulator.Clock.microTime();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendPing() throws java.io.IOException
		private void sendPing()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Sending ping to port {0:D}", Port));
			}
			AdhocMatchingEventMessage adhocMatchingEventMessage = createMessage(PSP_ADHOC_MATCHING_EVENT_INTERNAL_PING);
			send(adhocMatchingEventMessage);

			lastPingMicros = Emulator.Clock.microTime();
		}

		protected internal override void closeSocket()
		{
			base.closeSocket();
			started = false;
			connected = false;
			inConnection = false;
			pendingComplete = false;
		}

		public virtual int send(pspNetMacAddress macAddress, int dataLen, int data)
		{
			int result = 0;

			try
			{
				AdhocMatchingEventMessage adhocMatchingEventMessage = createMessage(PSP_ADHOC_MATCHING_EVENT_DATA, data, dataLen, macAddress.macAddress);
				send(adhocMatchingEventMessage, macAddress, dataLen, data);
				result = dataLen;
			}
			catch (SocketException e)
			{
				log.error("send", e);
			}
			catch (UnknownHostException e)
			{
				log.error("send", e);
			}
			catch (IOException e)
			{
				log.error("send", e);
			}

			return result;
		}

		public virtual int selectTarget(pspNetMacAddress macAddress, int optLen, int optData)
		{
			int result = 0;

			try
			{
				int @event;
				if (pendingJoinRequest != null && sceNetAdhoc.isSameMacAddress(pendingJoinRequest, macAddress.macAddress))
				{
					@event = PSP_ADHOC_MATCHING_EVENT_ACCEPT;
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Sending accept to port {0:D}", Port));
					}

					if (Mode == sceNetAdhocMatching.PSP_ADHOC_MATCHING_MODE_HOST)
					{
						addMember(macAddress.macAddress);
						connected = true;
						inConnection = false;
					}
				}
				else
				{
					@event = PSP_ADHOC_MATCHING_EVENT_JOIN;
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Sending join to port {0:D}", Port));
					}
				}
				AdhocMatchingEventMessage adhocMatchingEventMessage = createMessage(@event, optData, optLen, macAddress.macAddress);
				send(adhocMatchingEventMessage, macAddress, optLen, optData);

				inConnection = true;
			}
			catch (SocketException e)
			{
				log.error("selectTarget", e);
			}
			catch (UnknownHostException e)
			{
				log.error("selectTarget", e);
			}
			catch (IOException e)
			{
				log.error("selectTarget", e);
			}

			return result;
		}

		public virtual int cancelTarget(pspNetMacAddress macAddress)
		{
			return cancelTarget(macAddress, 0, 0);
		}

		public virtual int cancelTarget(pspNetMacAddress macAddress, int optLen, int optData)
		{
			int result = 0;

			try
			{
				int @event;
				if (connected)
				{
					@event = PSP_ADHOC_MATCHING_EVENT_LEFT;
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Sending leave to port {0:D}", Port));
					}
				}
				else
				{
					@event = PSP_ADHOC_MATCHING_EVENT_CANCEL;
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Sending cancel to port {0:D}", Port));
					}
				}
				AdhocMatchingEventMessage adhocMatchingEventMessage = createMessage(@event, optData, optLen, macAddress.macAddress);
				send(adhocMatchingEventMessage, macAddress, optLen, optData);
			}
			catch (SocketException e)
			{
				log.error("cancelTarget", e);
			}
			catch (UnknownHostException e)
			{
				log.error("cancelTarget", e);
			}
			catch (IOException e)
			{
				log.error("cancelTarget", e);
			}
			removeMember(macAddress.macAddress);
			connected = false;
			inConnection = false;

			return result;
		}

		public virtual int HelloOptLen
		{
			get
			{
				return helloOptData == null ? 0 : helloOptData.Length;
			}
		}

		public virtual sbyte[] HelloOptData
		{
			get
			{
				return helloOptData;
			}
		}

		public virtual void setHelloOpt(int optLen, int optData)
		{
			if (optLen <= 0 || optData == 0)
			{
				this.helloOptData = null;
				return;
			}

			// Copy the HelloOpt into an internal buffer, the user memory can be overwritten
			// after this call.
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(optData, optLen, 1);
			helloOptData = new sbyte[optLen];
			for (int i = 0; i < optLen; i++)
			{
				helloOptData[i] = (sbyte) memoryReader.readNext();
			}
		}

		public virtual void notifyCallbackEvent(int @event, int macAddr, int optLen, int optData)
		{
			if (Callback == 0)
			{
				return;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Notify callback 0x{0:X8}, event={1:D}, macAddr=0x{2:X8}, optLen={3:D}, optData=0x{4:X8}", Callback, @event, macAddr, optLen, optData));
			}
			Modules.ThreadManForUserModule.executeCallback(eventThread, Callback, null, true, Id, @event, macAddr, optLen, optData);
		}

		public virtual bool eventLoop()
		{
			if (socket == null || !started)
			{
				return false;
			}

			if (Emulator.Clock.microTime() - lastPingMicros >= PingDelay)
			{
				try
				{
					sendPing();
				}
				catch (IOException e)
				{
					log.error("eventLoop ping", e);
				}
			}

			if (!inConnection)
			{
				if (!connected && Mode != PSP_ADHOC_MATCHING_MODE_CLIENT)
				{
					if (Emulator.Clock.microTime() - lastHelloMicros >= HelloDelay)
					{
						try
						{
							sendHello();
						}
						catch (IOException e)
						{
							log.error("eventLoop hello", e);
						}
					}
				}
			}

			return true;
		}

		private void addMember(sbyte[] macAddr)
		{
			foreach (pspNetMacAddress member in members)
			{
				if (sceNetAdhoc.isSameMacAddress(macAddr, member.macAddress))
				{
					// Already in the members list
					return;
				}
			}

			pspNetMacAddress member = new pspNetMacAddress();
			member.MacAddress = macAddr;
			members.AddLast(member);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Adding member {0}", member));
			}
		}

		private void removeMember(sbyte[] macAddr)
		{
			foreach (pspNetMacAddress member in members)
			{
				if (sceNetAdhoc.isSameMacAddress(macAddr, member.macAddress))
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Removing member {0}", member));
					}
//JAVA TO C# CONVERTER TODO TASK: There is no .NET LinkedList equivalent to the Java 'remove' method:
					members.remove(member);
					break;
				}
			}
		}

		public virtual void addCallbackEvent(int @event, int macAddr, int optLen, int optData)
		{
			CallbackEvent callbackEvent = new CallbackEvent(@event, macAddr, optLen, optData);
			pendingCallbackEvents.AddLast(callbackEvent);
		}

		public virtual bool inputLoop()
		{
			if (socket == null || !started)
			{
				return false;
			}

			// Execute all the pending callback events
			while (pendingCallbackEvents.Count > 0)
			{
				CallbackEvent callbackEvent = pendingCallbackEvents.RemoveFirst();
				pspNetMacAddress macAddress = callbackEvent.MacAddress;
				macAddress.write(Memory.Instance, buffer.addr);
				notifyCallbackEvent(callbackEvent.Event, macAddress.BaseAddress, callbackEvent.OptLen, callbackEvent.OptData);
			}

			try
			{
				sbyte[] bytes = new sbyte[BufSize + MAX_HEADER_SIZE];
				int length = socket.receive(bytes, bytes.Length);
				if (length > 0)
				{
					int receivedPort = socket.ReceivedPort;
					InetAddress receivedAddress = socket.ReceivedAddress;
					AdhocMatchingEventMessage adhocMatchingEventMessage = createMessage(bytes, length);
					if (isForMe(adhocMatchingEventMessage, receivedPort, receivedAddress))
					{
						int @event = adhocMatchingEventMessage.Event;
						int macAddr = buffer.addr;
						int optData = buffer.addr + 8;
						int optLen = adhocMatchingEventMessage.DataLength;
						adhocMatchingEventMessage.writeDataToMemory(optData);
						pspNetMacAddress macAddress = new pspNetMacAddress();
						macAddress.MacAddress = adhocMatchingEventMessage.FromMacAddress;
						macAddress.write(Memory.Instance, macAddr);

						if (log.DebugEnabled)
						{
							log.debug(string.Format("Received message length={0:D}, event={1:D}, fromMac={2}, port={3:D}: {4}", adhocMatchingEventMessage.DataLength, @event, macAddress, socket.ReceivedPort, adhocMatchingEventMessage));
							if (log.TraceEnabled && optLen > 0)
							{
								log.trace(string.Format("Message data: {0}", Utilities.getMemoryDump(optData, optLen)));
							}
						}

						// Keep track that we received a new message from this MAC address
						Modules.sceNetAdhocctlModule.hleNetAdhocctlPeerUpdateTimestamp(adhocMatchingEventMessage.FromMacAddress);

						if (@event == PSP_ADHOC_MATCHING_EVENT_JOIN)
						{
							pendingJoinRequest = adhocMatchingEventMessage.FromMacAddress;
							inConnection = true;
						}
						adhocMatchingEventMessage.processOnReceive(macAddr, optData, optLen);

						if (@event == PSP_ADHOC_MATCHING_EVENT_ACCEPT)
						{
							addMember(adhocMatchingEventMessage.FromMacAddress);
							if (log.DebugEnabled)
							{
								log.debug(string.Format("Sending complete to port {0:D}", Port));
							}
							adhocMatchingEventMessage = createMessage(PSP_ADHOC_MATCHING_EVENT_COMPLETE, optData, optLen, macAddress.macAddress);
							send(adhocMatchingEventMessage);

							pendingComplete = true;
							connected = true;
							inConnection = false;
						}
						else if (@event == PSP_ADHOC_MATCHING_EVENT_COMPLETE)
						{
							addMember(adhocMatchingEventMessage.FromMacAddress);
							if (!pendingComplete)
							{
								if (log.DebugEnabled)
								{
									log.debug(string.Format("Sending complete to port {0:D}", Port));
								}
								adhocMatchingEventMessage = createMessage(PSP_ADHOC_MATCHING_EVENT_COMPLETE, optData, optLen, macAddress.macAddress);
								send(adhocMatchingEventMessage);
							}
							connected = true;
							inConnection = false;
						}
						else if (@event == PSP_ADHOC_MATCHING_EVENT_DATA)
						{
							if (log.DebugEnabled)
							{
								log.debug(string.Format("Sending data confirm to port {0:D}", Port));
							}
							adhocMatchingEventMessage = createMessage(PSP_ADHOC_MATCHING_EVENT_DATA_CONFIRM, 0, 0, macAddress.macAddress);
							send(adhocMatchingEventMessage);
						}
						else if (@event == PSP_ADHOC_MATCHING_EVENT_DISCONNECT || @event == PSP_ADHOC_MATCHING_EVENT_LEFT)
						{
							if (log.DebugEnabled)
							{
								log.debug(string.Format("Received disconnect/leave from {0}", macAddress));
							}
							removeMember(adhocMatchingEventMessage.FromMacAddress);
							if (members.Count <= 1)
							{
								connected = false;
								inConnection = false;
							}
						}
					}
					else
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Received message not for me: {0}", adhocMatchingEventMessage));
						}
					}
				}
			}
			catch (SocketTimeoutException)
			{
				// Nothing available
			}
			catch (IOException e)
			{
				log.error("inputLoop", e);
			}

			return true;
		}

		protected internal virtual INetworkAdapter NetworkAdapter
		{
			get
			{
				return Modules.sceNetModule.NetworkAdapter;
			}
		}

		protected internal virtual AdhocMatchingEventMessage createMessage(int @event)
		{
			return NetworkAdapter.createAdhocMatchingEventMessage(this, @event);
		}

		protected internal virtual AdhocMatchingEventMessage createMessage(int @event, int data, int dataLength, sbyte[] macAddress)
		{
			return NetworkAdapter.createAdhocMatchingEventMessage(this, @event, data, dataLength, macAddress);
		}

		protected internal virtual AdhocMatchingEventMessage createMessage(sbyte[] message, int length)
		{
			return NetworkAdapter.createAdhocMatchingEventMessage(this, message, length);
		}

		protected internal virtual bool isForMe(AdhocMessage adhocMessage, int port, InetAddress address)
		{
			return adhocMessage.ForMe;
		}

		public override string ToString()
		{
			return string.Format("MatchingObject[id=0x{0:X}, mode={1:D}, maxPeers={2:D}, port={3:D}, callback=0x{4:X8}]", Id, mode, maxPeers, Port, callback);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void send(AdhocMatchingEventMessage adhocMatchingEventMessage, pspsharp.HLE.kernel.types.pspNetMacAddress macAddress, int dataLen, int data) throws java.io.IOException
		protected internal virtual void send(AdhocMatchingEventMessage adhocMatchingEventMessage, pspNetMacAddress macAddress, int dataLen, int data)
		{
			base.send(adhocMatchingEventMessage);

			if (adhocMatchingEventMessage != null)
			{
				adhocMatchingEventMessage.processOnSend(macAddress.BaseAddress, data, dataLen);
			}
		}
	}

}