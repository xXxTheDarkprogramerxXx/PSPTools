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
namespace pspsharp.HLE.modules
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocctl.fillNextPointersInLinkedList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeBytes;


	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using pspAbstractMemoryMappedStructure = pspsharp.HLE.kernel.types.pspAbstractMemoryMappedStructure;
	using pspAbstractMemoryMappedStructureVariableLength = pspsharp.HLE.kernel.types.pspAbstractMemoryMappedStructureVariableLength;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using Wlan = pspsharp.hardware.Wlan;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using INetworkAdapter = pspsharp.network.INetworkAdapter;
	using AdhocMessage = pspsharp.network.adhoc.AdhocMessage;
	using PdpObject = pspsharp.network.adhoc.PdpObject;
	using PtpObject = pspsharp.network.adhoc.PtpObject;
	using AutoDetectJpcsp = pspsharp.network.upnp.AutoDetectJpcsp;
	using Scheduler = pspsharp.scheduler.Scheduler;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class sceNetAdhoc : HLEModule
	{
		public static Logger log = Modules.getLogger("sceNetAdhoc");

		// For test purpose when running 2 different pspsharp instances on the same computer:
		// one computer has to have netClientPortShift=0 and netServerPortShift=100,
		// the other computer, netClientPortShift=100 and netServerPortShift=0.
		private int netClientPortShift = 0;
		private int netServerPortShift = 0;

		// Period to update the Game Mode
		protected internal const int GAME_MODE_UPDATE_MICROS = 12000;

		protected internal const int PSP_ADHOC_POLL_READY_TO_SEND = 1;
		protected internal const int PSP_ADHOC_POLL_DATA_AVAILABLE = 2;
		protected internal const int PSP_ADHOC_POLL_CAN_CONNECT = 4;
		protected internal const int PSP_ADHOC_POLL_CAN_ACCEPT = 8;

		protected internal Dictionary<int, PdpObject> pdpObjects;
		protected internal Dictionary<int, PtpObject> ptpObjects;
		private int currentFreePort;
		public static readonly sbyte[] ANY_MAC_ADDRESS = new sbyte[] {unchecked((sbyte) 0xFF), unchecked((sbyte) 0xFF), unchecked((sbyte) 0xFF), unchecked((sbyte) 0xFF), unchecked((sbyte) 0xFF), unchecked((sbyte) 0xFF)};
		private GameModeScheduledAction gameModeScheduledAction;
		protected internal GameModeArea masterGameModeArea;
		protected internal LinkedList<GameModeArea> replicaGameModeAreas;
		private const string replicaIdPurpose = "sceNetAdhoc-Replica";
		private const int adhocGameModePort = 31000;
		private DatagramSocket gameModeSocket;
		private bool isInitialized;

		private class ClientPortShiftSettingsListener : AbstractBoolSettingsListener
		{
			private readonly sceNetAdhoc outerInstance;

			public ClientPortShiftSettingsListener(sceNetAdhoc outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override void settingsValueChanged(bool value)
			{
				if (value)
				{
					outerInstance.NetClientPortShift = 100;
				}
				else
				{
					outerInstance.NetClientPortShift = 0;
				}
			}
		}

		private class ServerPortShiftSettingsListener : AbstractBoolSettingsListener
		{
			private readonly sceNetAdhoc outerInstance;

			public ServerPortShiftSettingsListener(sceNetAdhoc outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override void settingsValueChanged(bool value)
			{
				if (value)
				{
					outerInstance.NetServerPortShift = 100;
				}
				else
				{
					outerInstance.NetServerPortShift = 0;
				}
			}
		}

		protected internal class GameModeScheduledAction : IAction
		{
			internal readonly int scheduleRepeatMicros;
			internal long nextSchedule;

			public GameModeScheduledAction(int scheduleRepeatMicros)
			{
				this.scheduleRepeatMicros = scheduleRepeatMicros;
			}

			public virtual void stop()
			{
				Scheduler.Instance.removeAction(nextSchedule, this);
			}

			public virtual void start()
			{
				Scheduler.Instance.addAction(this);
			}

			public virtual void execute()
			{
				Modules.sceNetAdhocModule.hleGameModeUpdate();

				nextSchedule = Scheduler.Now + scheduleRepeatMicros;
				Scheduler.Instance.addAction(nextSchedule, this);
			}
		}

		public class GameModeArea
		{
			public pspNetMacAddress macAddress;
			public int addr;
			public int size;
			public int id;
			internal sbyte[] newData;
			internal long updateTimestamp;

			public GameModeArea(int addr, int size)
			{
				this.addr = addr;
				this.size = size;
				id = -1;
			}

			public GameModeArea(pspNetMacAddress macAddress, int addr, int size)
			{
				this.macAddress = macAddress;
				this.addr = addr;
				this.size = size;
				id = SceUidManager.getNewUid(replicaIdPurpose);
			}

			public virtual void delete()
			{
				if (id >= 0)
				{
					SceUidManager.releaseUid(id, replicaIdPurpose);
					id = -1;
				}
			}

			public virtual sbyte[] NewData
			{
				set
				{
					updateTimestamp = Emulator.Clock.microTime();
					this.newData = value;
				}
				get
				{
					return newData;
				}
			}

			public virtual void setNewData()
			{
				sbyte[] data = new sbyte[size];
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(addr, size, 1);
				for (int i = 0; i < data.Length; i++)
				{
					data[i] = (sbyte) memoryReader.readNext();
				}

				NewData = data;
			}

			public virtual void resetNewData()
			{
				newData = null;
			}


			public virtual bool hasNewData()
			{
				return newData != null;
			}

			public virtual void writeNewData()
			{
				if (newData != null)
				{
					writeBytes(addr, System.Math.Min(size, newData.Length), newData, 0);
				}
			}

			public virtual long UpdateTimestamp
			{
				get
				{
					return updateTimestamp;
				}
			}

			public override string ToString()
			{
				if (macAddress == null)
				{
					return string.Format("Master GameModeArea addr=0x{0:X8}, size={1:D}", addr, size);
				}
				return string.Format("Replica GameModeArea id={0:D}, macAddress={1}, addr=0x{2:X8}, size={3:D}", id, macAddress, addr, size);
			}
		}

		protected internal static string getPollEventName(int @event)
		{
			return string.Format("Unknown 0x{0:X}", @event);
		}

		protected internal class pspAdhocPollId : pspAbstractMemoryMappedStructure
		{
			public int id;
			public int events;
			public int revents;

			protected internal override void read()
			{
				id = read32();
				events = read32();
				revents = read32();
			}

			protected internal override void write()
			{
				write32(id);
				write32(events);
				write32(revents);
			}

			public override int @sizeof()
			{
				return 12;
			}

			public override string ToString()
			{
				return string.Format("PollId[id={0:D}, events=0x{1:X}({2}), revents=0x{3:X}({4})]", id, events, getPollEventName(events), revents, getPollEventName(revents));
			}
		}

		protected internal class GameModeUpdateInfo : pspAbstractMemoryMappedStructureVariableLength
		{
			public int updated;
			public long timeStamp;

			protected internal override void read()
			{
				base.read();
				updated = read32();
				timeStamp = read64();
			}

			protected internal override void write()
			{
				base.write();
				write32(updated);
				write64(timeStamp);
			}
		}

		public override void start()
		{
			setSettingsListener("emu.netClientPortShift", new ClientPortShiftSettingsListener(this));
			setSettingsListener("emu.netServerPortShift", new ServerPortShiftSettingsListener(this));

			AutoDetectJpcsp autoDetectJpcsp = AutoDetectJpcsp.Instance;
			if (autoDetectJpcsp != null)
			{
				autoDetectJpcsp.discoverOtherJpcspInBackground();
			}

			pdpObjects = new Dictionary<int, PdpObject>();
			ptpObjects = new Dictionary<int, PtpObject>();
			currentFreePort = 0x4000;
			replicaGameModeAreas = new LinkedList<sceNetAdhoc.GameModeArea>();
			isInitialized = false;

			base.start();
		}

		public virtual int NetClientPortShift
		{
			set
			{
				this.netClientPortShift = value;
				log.info(string.Format("Using netClientPortShift={0:D}", value));
			}
		}

		public virtual int NetServerPortShift
		{
			set
			{
				this.netServerPortShift = value;
				log.info(string.Format("Using netServerPortShift={0:D}", value));
			}
		}

		public virtual int getClientPortFromRealPort(sbyte[] clientMacAddress, int realPort)
		{
			if (isMyMacAddress(clientMacAddress))
			{
				// if the client is my-self, then this is actually a server port...
				return getServerPortFromRealPort(realPort);
			}

			return realPort - netClientPortShift;
		}

		public virtual int getRealPortFromClientPort(sbyte[] clientMacAddress, int clientPort)
		{
			if (isMyMacAddress(clientMacAddress))
			{
				// if the client is my-self, then this is actually a server port...
				return getRealPortFromServerPort(clientPort);
			}

			return clientPort + netClientPortShift;
		}

		public virtual int getServerPortFromRealPort(int realPort)
		{
			return realPort - netServerPortShift;
		}

		public virtual int getRealPortFromServerPort(int serverPort)
		{
			return serverPort + netServerPortShift;
		}

		public virtual bool hasNetPortShiftActive()
		{
			return netServerPortShift > 0 || netClientPortShift > 0;
		}

		protected internal virtual void checkInitialized()
		{
			if (!isInitialized)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_NET_ADHOC_NOT_INITIALIZED);
			}
		}

		public virtual void hleExitGameMode()
		{
			masterGameModeArea = null;
			replicaGameModeAreas.Clear();
			stopGameMode();
		}

		public virtual void hleGameModeUpdate()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleGameModeUpdate"));
			}

			try
			{
				if (gameModeSocket == null)
				{
					gameModeSocket = new DatagramSocket(Modules.sceNetAdhocModule.getRealPortFromServerPort(adhocGameModePort));
					// For broadcast
					gameModeSocket.Broadcast = true;
					// Non-blocking (timeout = 0 would mean blocking)
					gameModeSocket.SoTimeout = 1;
				}

				// Send master area
				if (masterGameModeArea != null && masterGameModeArea.hasNewData())
				{
					try
					{
						AdhocMessage adhocGameModeMessage = NetworkAdapter.createAdhocGameModeMessage(masterGameModeArea);
						SocketAddress[] socketAddress = Modules.sceNetAdhocModule.getMultiSocketAddress(sceNetAdhoc.ANY_MAC_ADDRESS, Modules.sceNetAdhocModule.getRealPortFromClientPort(sceNetAdhoc.ANY_MAC_ADDRESS, adhocGameModePort));
						for (int i = 0; i < socketAddress.Length; i++)
						{
							DatagramPacket packet = new DatagramPacket(adhocGameModeMessage.Message, adhocGameModeMessage.MessageLength, socketAddress[i]);
							gameModeSocket.send(packet);

							if (log.DebugEnabled)
							{
								log.debug(string.Format("GameMode message sent to {0}: {1}", socketAddress[i], adhocGameModeMessage));
							}
						}
					}
					catch (SocketTimeoutException)
					{
						// Ignore exception
					}
				}

				// Receive all waiting messages
				do
				{
					try
					{
						sbyte[] bytes = new sbyte[10000];
						DatagramPacket packet = new DatagramPacket(bytes, bytes.Length);
						gameModeSocket.receive(packet);
						AdhocMessage adhocGameModeMessage = NetworkAdapter.createAdhocGameModeMessage(packet.Data, packet.Length);

						if (log.DebugEnabled)
						{
							log.debug(string.Format("GameMode received: {0}", adhocGameModeMessage));
						}

						foreach (GameModeArea gameModeArea in replicaGameModeAreas)
						{
							if (isSameMacAddress(gameModeArea.macAddress.macAddress, adhocGameModeMessage.FromMacAddress))
							{
								if (log.DebugEnabled)
								{
									log.debug(string.Format("Received new Data for GameMode Area {0}", gameModeArea));
								}
								gameModeArea.NewData = adhocGameModeMessage.Data;
								break;
							}
						}
					}
					catch (SocketTimeoutException)
					{
						// No more messages available
						break;
					}
				} while (true);
			}
			catch (IOException e)
			{
				log.error("hleGameModeUpdate", e);
			}
		}

		protected internal virtual void startGameMode()
		{
			if (gameModeScheduledAction == null)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Starting GameMode"));
				}
				gameModeScheduledAction = new GameModeScheduledAction(GAME_MODE_UPDATE_MICROS);
				gameModeScheduledAction.start();
			}
		}

		protected internal virtual void stopGameMode()
		{
			if (gameModeScheduledAction != null)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Stopping GameMode"));
				}
				gameModeScheduledAction.stop();
				gameModeScheduledAction = null;
			}

			if (gameModeSocket != null)
			{
				gameModeSocket.close();
				gameModeSocket = null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.net.SocketAddress getSocketAddress(byte[] macAddress, int realPort) throws java.net.UnknownHostException
		public virtual SocketAddress getSocketAddress(sbyte[] macAddress, int realPort)
		{
			return NetworkAdapter.getSocketAddress(macAddress, realPort);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.net.SocketAddress[] getMultiSocketAddress(byte[] macAddress, int realPort) throws java.net.UnknownHostException
		public virtual SocketAddress[] getMultiSocketAddress(sbyte[] macAddress, int realPort)
		{
			return NetworkAdapter.getMultiSocketAddress(macAddress, realPort);
		}

		public static bool isSameMacAddress(sbyte[] macAddress1, sbyte[] macAddress2)
		{
			if (macAddress1.Length != macAddress2.Length)
			{
				return false;
			}

			for (int i = 0; i < macAddress1.Length; i++)
			{
				if (macAddress1[i] != macAddress2[i])
				{
					return false;
				}
			}

			return true;
		}

		public static bool isAnyMacAddress(sbyte[] macAddress)
		{
			return isSameMacAddress(macAddress, ANY_MAC_ADDRESS);
		}

		public static bool isMyMacAddress(sbyte[] macAddress)
		{
			return isSameMacAddress(Wlan.MacAddress, macAddress);
		}

		private int FreePort
		{
			get
			{
				int freePort = currentFreePort;
				if (netClientPortShift > 0 || netServerPortShift > 0)
				{
					currentFreePort += 2;
				}
				else
				{
					currentFreePort++;
				}
    
				if (currentFreePort > 0x7FFF)
				{
					currentFreePort -= 0x4000;
				}
    
				return freePort;
			}
		}

		public virtual int checkPdpId(int pdpId)
		{
			checkInitialized();

			if (!pdpObjects.ContainsKey(pdpId))
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Invalid Pdp Id={0:D}", pdpId));
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_NET_ADHOC_INVALID_SOCKET_ID);
			}

			return pdpId;
		}

		public virtual int checkPtpId(int ptpId)
		{
			checkInitialized();

			if (!ptpObjects.ContainsKey(ptpId))
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Invalid Ptp Id={0:D}", ptpId));
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_NET_ADHOC_INVALID_SOCKET_ID);
			}

			return ptpId;
		}

		public virtual void hleAddPtpObject(PtpObject ptpObject)
		{
			ptpObjects[ptpObject.Id] = ptpObject;
		}

		protected internal virtual INetworkAdapter NetworkAdapter
		{
			get
			{
				return Modules.sceNetModule.NetworkAdapter;
			}
		}

		/// <summary>
		/// Initialize the adhoc library.
		/// </summary>
		/// <returns> 0 on success, < 0 on error </returns>
		[HLEFunction(nid : 0xE1D621D7, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetAdhocInit()
		{
			log.info(string.Format("sceNetAdhocInit: using MAC address={0}, nick name='{1}'", sceNet.convertMacAddressToString(Wlan.MacAddress), sceUtility.SystemParamNickname));

			if (isInitialized)
			{
				return SceKernelErrors.ERROR_NET_ADHOC_ALREADY_INITIALIZED;
			}

			isInitialized = true;

			return 0;
		}

		/// <summary>
		/// Terminate the adhoc library
		/// </summary>
		/// <returns> 0 on success, < 0 on error </returns>
		[HLEFunction(nid : 0xA62C6F57, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetAdhocTerm()
		{
			isInitialized = false;

			return 0;
		}

		[HLEFunction(nid : 0x7A662D6B, version : 150)]
		public virtual int sceNetAdhocPollSocket(TPointer socketsAddr, int count, int timeout, int nonblock)
		{
			checkInitialized();

			Memory mem = Memory.Instance;

			int countEvents = 0;
			for (int i = 0; i < count; i++)
			{
				pspAdhocPollId pollId = new pspAdhocPollId();
				pollId.read(mem, socketsAddr.Address + i * pollId.@sizeof());

				PdpObject pdpObject = pdpObjects[pollId.id];
				PtpObject ptpObject = null;
				if (pdpObject == null)
				{
					ptpObject = ptpObjects[pollId.id];
					pdpObject = ptpObject;
				}
				if (pdpObject != null)
				{
					try
					{
						pdpObject.update();
					}
					catch (IOException)
					{
						// Ignore exception
					}
				}

				pollId.revents = 0;
				if ((pollId.events & PSP_ADHOC_POLL_DATA_AVAILABLE) != 0 && pdpObject.RcvdData > 0)
				{
					pollId.revents |= PSP_ADHOC_POLL_DATA_AVAILABLE;
				}
				if ((pollId.events & PSP_ADHOC_POLL_READY_TO_SEND) != 0)
				{
					// Data can always be sent
					pollId.revents |= PSP_ADHOC_POLL_READY_TO_SEND;
				}
				if ((pollId.events & PSP_ADHOC_POLL_CAN_CONNECT) != 0)
				{
					if (ptpObject != null && ptpObject.canConnect())
					{
						pollId.revents |= PSP_ADHOC_POLL_CAN_CONNECT;
					}
				}
				if ((pollId.events & PSP_ADHOC_POLL_CAN_ACCEPT) != 0)
				{
					if (ptpObject != null && ptpObject.canAccept())
					{
						pollId.revents |= PSP_ADHOC_POLL_CAN_ACCEPT;
					}
				}

				if (pollId.revents != 0)
				{
					countEvents++;
				}

				pollId.write(mem);

				log.info(string.Format("sceNetAdhocPollSocket pollId[{0:D}]={1}", i, pollId));
			}

			return countEvents;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x73BFD52D, version = 150) public int sceNetAdhocSetSocketAlert(int id, int flags)
		[HLEFunction(nid : 0x73BFD52D, version : 150)]
		public virtual int sceNetAdhocSetSocketAlert(int id, int flags)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4D2CE199, version = 150) public int sceNetAdhocGetSocketAlert()
		[HLEFunction(nid : 0x4D2CE199, version : 150)]
		public virtual int sceNetAdhocGetSocketAlert()
		{
			return 0;
		}

		/// <summary>
		/// Create a PDP object.
		/// </summary>
		/// <param name="mac"> - Your MAC address (from sceWlanGetEtherAddr) </param>
		/// <param name="port"> - Port to use, lumines uses 0x309 </param>
		/// <param name="bufsize"> - Socket buffer size, lumines sets to 0x400 </param>
		/// <param name="unk1"> - Unknown, lumines sets to 0
		/// </param>
		/// <returns> The ID of the PDP object (< 0 on error) </returns>
		[HLEFunction(nid : 0x6F92741B, version : 150)]
		public virtual int sceNetAdhocPdpCreate(pspNetMacAddress macAddress, int port, int bufSize, int unk1)
		{
			checkInitialized();

			if (port == 0)
			{
				// Allocate a free port
				port = FreePort;
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceNetAdhocPdpCreate: using free port {0:D}", port));
				}
			}

			PdpObject pdpObject = NetworkAdapter.createPdpObject();
			int result = pdpObject.create(macAddress, port, bufSize);
			if (result == pdpObject.Id)
			{
				pdpObjects[pdpObject.Id] = pdpObject;

				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceNetAdhocPdpCreate: returning id=0x{0:X}", result));
				}
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceNetAdhocPdpCreate: returning error=0x{0:X8}", result));
				}
			}

			return result;
		}

		/// <summary>
		/// Send a PDP packet to a destination
		/// </summary>
		/// <param name="id"> - The ID as returned by ::sceNetAdhocPdpCreate </param>
		/// <param name="destMacAddr"> - The destination MAC address, can be set to all 0xFF for broadcast </param>
		/// <param name="port"> - The port to send to </param>
		/// <param name="data"> - The data to send </param>
		/// <param name="len"> - The length of the data. </param>
		/// <param name="timeout"> - Timeout in microseconds. </param>
		/// <param name="nonblock"> - Set to 0 to block, 1 for non-blocking.
		/// </param>
		/// <returns> Bytes sent, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xABED3790, version = 150) public int sceNetAdhocPdpSend(@CheckArgument("checkPdpId") int id, pspsharp.HLE.kernel.types.pspNetMacAddress destMacAddress, int port, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer data, int len, int timeout, int nonblock)
		[HLEFunction(nid : 0xABED3790, version : 150)]
		public virtual int sceNetAdhocPdpSend(int id, pspNetMacAddress destMacAddress, int port, TPointer data, int len, int timeout, int nonblock)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("Send data: {0}", Utilities.getMemoryDump(data.Address, len)));
			}

			return pdpObjects[id].send(destMacAddress, port, data, len, timeout, nonblock);
		}

		/// <summary>
		/// Receive a PDP packet
		/// </summary>
		/// <param name="id"> - The ID of the PDP object, as returned by ::sceNetAdhocPdpCreate </param>
		/// <param name="srcMacAddr"> - Buffer to hold the source mac address of the sender </param>
		/// <param name="port"> - Buffer to hold the port number of the received data </param>
		/// <param name="data"> - Data buffer </param>
		/// <param name="dataLength"> - The length of the data buffer </param>
		/// <param name="timeout"> - Timeout in microseconds. </param>
		/// <param name="nonblock"> - Set to 0 to block, 1 for non-blocking.
		/// </param>
		/// <returns> Number of bytes received, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xDFE53E03, version = 150) public int sceNetAdhocPdpRecv(@CheckArgument("checkPdpId") int id, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=6, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer srcMacAddr, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer16 portAddr, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=32, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer data, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer32 dataLengthAddr, int timeout, int nonblock)
		[HLEFunction(nid : 0xDFE53E03, version : 150)]
		public virtual int sceNetAdhocPdpRecv(int id, TPointer srcMacAddr, TPointer16 portAddr, TPointer data, TPointer32 dataLengthAddr, int timeout, int nonblock)
		{
			int result = pdpObjects[id].recv(srcMacAddr, portAddr, data, dataLengthAddr, timeout, nonblock);

			return result;
		}

		/// <summary>
		/// Delete a PDP object.
		/// </summary>
		/// <param name="id"> - The ID returned from ::sceNetAdhocPdpCreate </param>
		/// <param name="unk1"> - Unknown, set to 0
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7F27BB5E, version = 150) public int sceNetAdhocPdpDelete(@CheckArgument("checkPdpId") int id, int unk1)
		[HLEFunction(nid : 0x7F27BB5E, version : 150)]
		public virtual int sceNetAdhocPdpDelete(int id, int unk1)
		{
			pdpObjects.Remove(id).delete();

			return 0;
		}

		/// <summary>
		/// Get the status of all PDP objects
		/// </summary>
		/// <param name="size"> - Pointer to the size of the stat array (e.g 20 for one structure) </param>
		/// <param name="stat"> - Pointer to a list of ::pspStatStruct structures.
		/// 
		/// typedef struct pdpStatStruct
		/// {
		///    struct pdpStatStruct *next; // Pointer to next PDP structure in list
		///    int pdpId;                  // pdp ID
		///    unsigned char mac[6];       // MAC address
		///    unsigned short port;        // Port
		///    unsigned int rcvdData;      // Bytes received
		/// } pdpStatStruct
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC7C1FC57, version = 150) public int sceNetAdhocGetPdpStat(pspsharp.HLE.TPointer32 sizeAddr, @CanBeNull pspsharp.HLE.TPointer buf)
		[HLEFunction(nid : 0xC7C1FC57, version : 150)]
		public virtual int sceNetAdhocGetPdpStat(TPointer32 sizeAddr, TPointer buf)
		{
			checkInitialized();

			const int objectInfoSize = 20;

			int size = sizeAddr.getValue();
			sizeAddr.setValue(objectInfoSize * pdpObjects.Count);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetAdhocGetPdpStat returning size={0:D}", sizeAddr.getValue()));
			}

			if (buf.NotNull)
			{
				int offset = 0;
				foreach (int pdpId in pdpObjects.Keys)
				{
					PdpObject pdpObject = pdpObjects[pdpId];

					// Check if enough space available to write the next structure
					if (offset + objectInfoSize > size || pdpObject == null)
					{
						break;
					}

					try
					{
						pdpObject.update();
					}
					catch (IOException)
					{
						// Ignore error
					}

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetAdhocGetPdpStat returning {0} at 0x{1:X8}", pdpObject, buf.Address + offset));
					}

					/// <summary>
					/// Pointer to next PDP structure in list: will be written later </summary>
					offset += 4;

					/// <summary>
					/// pdp ID </summary>
					buf.setValue32(offset, pdpObject.Id);
					offset += 4;

					/// <summary>
					/// MAC address </summary>
					pdpObject.MacAddress.write(buf.Memory, buf.Address + offset);
					offset += pdpObject.MacAddress.@sizeof();

					/// <summary>
					/// Port </summary>
					buf.setValue16(offset, (short) pdpObject.Port);
					offset += 2;

					/// <summary>
					/// Bytes received </summary>
					buf.setValue32(offset, pdpObject.RcvdData);
					offset += 4;
				}

				fillNextPointersInLinkedList(buf, offset, objectInfoSize);
			}

			return 0;
		}

		/// <summary>
		/// Open a PTP connection
		/// </summary>
		/// <param name="srcmac"> - Local mac address. </param>
		/// <param name="srcport"> - Local port. </param>
		/// <param name="destmac"> - Destination mac. </param>
		/// <param name="destport"> - Destination port </param>
		/// <param name="bufsize"> - Socket buffer size </param>
		/// <param name="delay"> - Interval between retrying (microseconds). </param>
		/// <param name="count"> - Number of retries. </param>
		/// <param name="unk1"> - Pass 0.
		/// </param>
		/// <returns> A socket ID on success, < 0 on error. </returns>
		[HLEFunction(nid : 0x877F6D66, version : 150)]
		public virtual int sceNetAdhocPtpOpen(pspNetMacAddress srcMacAddress, int srcPort, pspNetMacAddress destMacAddress, int destPort, int bufSize, int retryDelay, int retryCount, int unk1)
		{
			checkInitialized();

			PtpObject ptpObject = NetworkAdapter.createPtpObject();
			ptpObject.MacAddress = srcMacAddress;
			ptpObject.Port = srcPort;
			ptpObject.DestMacAddress = destMacAddress;
			ptpObject.DestPort = destPort;
			ptpObject.BufSize = bufSize;
			ptpObject.RetryDelay = retryDelay;
			ptpObject.RetryCount = retryCount;
			int result = ptpObject.open();
			if (result != 0)
			{
				// Open failed...
				ptpObject.delete();
				return result;
			}

			ptpObjects[ptpObject.Id] = ptpObject;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetAdhocPtpOpen: returning id=0x{0:X}", ptpObject.Id));
			}

			return ptpObject.Id;
		}

		/// <summary>
		/// Wait for connection created by sceNetAdhocPtpOpen()
		/// </summary>
		/// <param name="id"> - A socket ID. </param>
		/// <param name="timeout"> - Timeout in microseconds. </param>
		/// <param name="nonblock"> - Set to 0 to block, 1 for non-blocking.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xFC6FC07B, version = 150) public int sceNetAdhocPtpConnect(@CheckArgument("checkPtpId") int id, int timeout, int nonblock)
		[HLEFunction(nid : 0xFC6FC07B, version : 150)]
		public virtual int sceNetAdhocPtpConnect(int id, int timeout, int nonblock)
		{
			return ptpObjects[id].connect(timeout, nonblock);
		}

		/// <summary>
		/// Wait for an incoming PTP connection
		/// </summary>
		/// <param name="srcmac"> - Local mac address. </param>
		/// <param name="srcport"> - Local port. </param>
		/// <param name="bufsize"> - Socket buffer size </param>
		/// <param name="delay"> - Interval between retrying (microseconds). </param>
		/// <param name="count"> - Number of retries. </param>
		/// <param name="queue"> - Connection queue length. </param>
		/// <param name="unk1"> - Pass 0.
		/// </param>
		/// <returns> A socket ID on success, < 0 on error. </returns>
		[HLEFunction(nid : 0xE08BDAC1, version : 150)]
		public virtual int sceNetAdhocPtpListen(pspNetMacAddress srcMacAddress, int srcPort, int bufSize, int retryDelay, int retryCount, int queue, int unk1)
		{
			checkInitialized();

			PtpObject ptpObject = NetworkAdapter.createPtpObject();
			ptpObject.MacAddress = srcMacAddress;
			ptpObject.Port = srcPort;
			ptpObject.BufSize = bufSize;
			ptpObject.RetryDelay = retryDelay;
			ptpObject.RetryCount = retryCount;
			ptpObject.Queue = queue;
			int result = ptpObject.listen();
			if (result != 0)
			{
				// Listen failed...
				ptpObject.delete();
				return result;
			}

			ptpObjects[ptpObject.Id] = ptpObject;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetAdhocPtpListen: returning id=0x{0:X}", ptpObject.Id));
			}

			return ptpObject.Id;
		}

		/// <summary>
		/// Accept an incoming PTP connection
		/// </summary>
		/// <param name="id"> - A socket ID. </param>
		/// <param name="mac"> - Connecting peers mac. </param>
		/// <param name="port"> - Connecting peers port. </param>
		/// <param name="timeout"> - Timeout in microseconds. </param>
		/// <param name="nonblock"> - Set to 0 to block, 1 for non-blocking.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x9DF81198, version = 150) public int sceNetAdhocPtpAccept(@CheckArgument("checkPtpId") int id, @CanBeNull pspsharp.HLE.TPointer peerMacAddr, @CanBeNull pspsharp.HLE.TPointer16 peerPortAddr, int timeout, int nonblock)
		[HLEFunction(nid : 0x9DF81198, version : 150)]
		public virtual int sceNetAdhocPtpAccept(int id, TPointer peerMacAddr, TPointer16 peerPortAddr, int timeout, int nonblock)
		{
			return ptpObjects[id].accept(peerMacAddr.Address, peerPortAddr.Address, timeout, nonblock);
		}

		/// <summary>
		/// Send data
		/// </summary>
		/// <param name="id"> - A socket ID. </param>
		/// <param name="data"> - Data to send. </param>
		/// <param name="datasize"> - Size of the data. </param>
		/// <param name="timeout"> - Timeout in microseconds. </param>
		/// <param name="nonblock"> - Set to 0 to block, 1 for non-blocking.
		/// </param>
		/// <returns> 0 success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x4DA4C788, version = 150) public int sceNetAdhocPtpSend(@CheckArgument("checkPtpId") int id, pspsharp.HLE.TPointer data, pspsharp.HLE.TPointer32 dataSizeAddr, int timeout, int nonblock)
		[HLEFunction(nid : 0x4DA4C788, version : 150)]
		public virtual int sceNetAdhocPtpSend(int id, TPointer data, TPointer32 dataSizeAddr, int timeout, int nonblock)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("Send data: {0}", Utilities.getMemoryDump(data.Address, dataSizeAddr.getValue())));
			}

			return ptpObjects[id].send(data.Address, dataSizeAddr, timeout, nonblock);
		}

		/// <summary>
		/// Receive data
		/// </summary>
		/// <param name="id"> - A socket ID. </param>
		/// <param name="data"> - Buffer for the received data. </param>
		/// <param name="datasize"> - Size of the data received. </param>
		/// <param name="timeout"> - Timeout in microseconds. </param>
		/// <param name="nonblock"> - Set to 0 to block, 1 for non-blocking.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8BEA2B3E, version = 150) public int sceNetAdhocPtpRecv(@CheckArgument("checkPtpId") int id, pspsharp.HLE.TPointer data, pspsharp.HLE.TPointer32 dataSizeAddr, int timeout, int nonblock)
		[HLEFunction(nid : 0x8BEA2B3E, version : 150)]
		public virtual int sceNetAdhocPtpRecv(int id, TPointer data, TPointer32 dataSizeAddr, int timeout, int nonblock)
		{
			return ptpObjects[id].recv(data, dataSizeAddr, timeout, nonblock);
		}

		/// <summary>
		/// Wait for data in the buffer to be sent
		/// </summary>
		/// <param name="id"> - A socket ID. </param>
		/// <param name="timeout"> - Timeout in microseconds. </param>
		/// <param name="nonblock"> - Set to 0 to block, 1 for non-blocking.
		/// </param>
		/// <returns> A socket ID on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x9AC2EEAC, version = 150) public int sceNetAdhocPtpFlush(@CheckArgument("checkPtpId") int id, int timeout, int nonblock)
		[HLEFunction(nid : 0x9AC2EEAC, version : 150)]
		public virtual int sceNetAdhocPtpFlush(int id, int timeout, int nonblock)
		{
			// Faked: return successful completion
			return 0;
		}

		/// <summary>
		/// Close a socket
		/// </summary>
		/// <param name="id"> - A socket ID. </param>
		/// <param name="unk1"> - Pass 0.
		/// </param>
		/// <returns> A socket ID on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x157E6225, version = 150) public int sceNetAdhocPtpClose(@CheckArgument("checkPtpId") int id, int unknown)
		[HLEFunction(nid : 0x157E6225, version : 150)]
		public virtual int sceNetAdhocPtpClose(int id, int unknown)
		{
			ptpObjects.Remove(id).delete();

			return 0;
		}

		/// <summary>
		/// Get the status of all PTP objects
		/// </summary>
		/// <param name="size"> - Pointer to the size of the stat array (e.g 20 for one structure) </param>
		/// <param name="stat"> - Pointer to a list of ::ptpStatStruct structures.
		/// 
		/// typedef struct ptpStatStruct
		/// {
		///    struct ptpStatStruct *next; // Pointer to next PTP structure in list
		///    int ptpId;                  // ptp ID
		///    unsigned char mac[6];       // MAC address
		///    unsigned char peermac[6];   // Peer MAC address
		///    unsigned short port;        // Port
		///    unsigned short peerport;    // Peer Port
		///    unsigned int sentData;      // Bytes sent
		///    unsigned int rcvdData;      // Bytes received
		///    int unk1;                   // Unknown
		/// } ptpStatStruct;
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB9685118, version = 150) public int sceNetAdhocGetPtpStat(pspsharp.HLE.TPointer32 sizeAddr, @CanBeNull pspsharp.HLE.TPointer buf)
		[HLEFunction(nid : 0xB9685118, version : 150)]
		public virtual int sceNetAdhocGetPtpStat(TPointer32 sizeAddr, TPointer buf)
		{
			checkInitialized();

			const int objectInfoSize = 36;

			int size = sizeAddr.getValue();
			// Return size required
			sizeAddr.setValue(objectInfoSize * ptpObjects.Count);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetAdhocGetPtpStat returning size={0:D}", sizeAddr.getValue()));
			}

			if (buf.NotNull)
			{
				int offset = 0;
				pspNetMacAddress nonExistingDestMacAddress = new pspNetMacAddress();
				foreach (int pdpId in ptpObjects.Keys)
				{
					PtpObject ptpObject = ptpObjects[pdpId];

					// Check if enough space available to write the next structure
					if (offset + objectInfoSize > size || ptpObject == null)
					{
						break;
					}

					try
					{
						ptpObject.update();
					}
					catch (IOException)
					{
						// Ignore error
					}

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetAdhocGetPtpStat returning {0} at 0x{1:X8}", ptpObject, buf.Address + offset));
					}

					/// <summary>
					/// Pointer to next PDP structure in list: will be written later </summary>
					offset += 4;

					/// <summary>
					/// ptp ID </summary>
					buf.setValue32(offset, ptpObject.Id);
					offset += 4;

					/// <summary>
					/// MAC address </summary>
					ptpObject.MacAddress.write(buf.Memory, buf.Address + offset);
					offset += ptpObject.MacAddress.@sizeof();

					/// <summary>
					/// Dest MAC address </summary>
					if (ptpObject.DestMacAddress != null)
					{
						ptpObject.DestMacAddress.write(buf.Memory, buf.Address + offset);
						offset += ptpObject.DestMacAddress.@sizeof();
					}
					else
					{
						nonExistingDestMacAddress.write(buf.Memory, buf.Address + offset);
						offset += nonExistingDestMacAddress.@sizeof();
					}

					/// <summary>
					/// Port </summary>
					buf.setValue16(offset, (short) ptpObject.Port);
					offset += 2;

					/// <summary>
					/// Dest Port </summary>
					buf.setValue16(offset, (short) ptpObject.DestPort);
					offset += 2;

					/// <summary>
					/// Bytes sent </summary>
					buf.setValue32(offset, ptpObject.SentData);
					offset += 4;

					/// <summary>
					/// Bytes received </summary>
					buf.setValue32(offset, ptpObject.RcvdData);
					offset += 4;

					/// <summary>
					/// Unknown </summary>
					buf.setValue32(offset, 4); // PSP seems to return value 4 here
					offset += 4;
				}

				fillNextPointersInLinkedList(buf, offset, objectInfoSize);
			}

			return 0;
		}

		/// <summary>
		/// Create own game object type data.
		/// </summary>
		/// <param name="data"> - A pointer to the game object data. </param>
		/// <param name="size"> - Size of the game data.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
		[HLEFunction(nid : 0x7F75C338, version : 150)]
		public virtual int sceNetAdhocGameModeCreateMaster(TPointer data, int size)
		{
			checkInitialized();

			masterGameModeArea = new GameModeArea(data.Address, size);
			startGameMode();

			return 0;
		}

		/// <summary>
		/// Create peer game object type data.
		/// </summary>
		/// <param name="mac"> - The mac address of the peer. </param>
		/// <param name="data"> - A pointer to the game object data. </param>
		/// <param name="size"> - Size of the game data.
		/// </param>
		/// <returns> The id of the replica on success, < 0 on error. </returns>
		[HLEFunction(nid : 0x3278AB0C, version : 150)]
		public virtual int sceNetAdhocGameModeCreateReplica(pspNetMacAddress macAddress, TPointer data, int size)
		{
			checkInitialized();

			bool found = false;
			int result = 0;
			foreach (GameModeArea gameModeArea in replicaGameModeAreas)
			{
				if (isSameMacAddress(gameModeArea.macAddress.macAddress, macAddress.macAddress))
				{
					// Updating the exiting replica
					gameModeArea.addr = data.Address;
					gameModeArea.size = size;
					result = gameModeArea.id;
					found = true;
					break;
				}
			}

			if (!found)
			{
				GameModeArea gameModeArea = new GameModeArea(macAddress, data.Address, size);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Adding GameMode Replica {0}", gameModeArea));
				}
				result = gameModeArea.id;
				replicaGameModeAreas.AddLast(gameModeArea);
			}

			startGameMode();

			return result;
		}

		/// <summary>
		/// Update own game object type data.
		/// </summary>
		/// <returns> 0 on success, < 0 on error. </returns>
		[HLEFunction(nid : 0x98C204C8, version : 150)]
		public virtual int sceNetAdhocGameModeUpdateMaster()
		{
			checkInitialized();

			if (masterGameModeArea != null)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("Master Game Mode Area: {0}", Utilities.getMemoryDump(masterGameModeArea.addr, masterGameModeArea.size)));
				}
				masterGameModeArea.setNewData();
			}

			return 0;
		}

		/// <summary>
		/// Update peer game object type data.
		/// </summary>
		/// <param name="id"> - The id of the replica returned by sceNetAdhocGameModeCreateReplica. </param>
		/// <param name="info"> - address of GameModeUpdateInfo structure.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xFA324B4E, version = 150) public int sceNetAdhocGameModeUpdateReplica(int id, @CanBeNull pspsharp.HLE.TPointer infoAddr)
		[HLEFunction(nid : 0xFA324B4E, version : 150)]
		public virtual int sceNetAdhocGameModeUpdateReplica(int id, TPointer infoAddr)
		{
			checkInitialized();

			foreach (GameModeArea gameModeArea in replicaGameModeAreas)
			{
				if (gameModeArea.id == id)
				{
					GameModeUpdateInfo gameModeUpdateInfo = new GameModeUpdateInfo();
					if (infoAddr.NotNull)
					{
						gameModeUpdateInfo.read(infoAddr);
					}

					if (gameModeArea.hasNewData())
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Updating GameMode Area with new data: {0}", gameModeArea));
						}
						gameModeArea.writeNewData();
						gameModeArea.resetNewData();
						if (log.TraceEnabled)
						{
							log.trace(string.Format("Replica GameMode Area updated: {0}", Utilities.getMemoryDump(gameModeArea.addr, gameModeArea.size)));
						}
						gameModeUpdateInfo.updated = 1;
					}
					else
					{
						gameModeUpdateInfo.updated = 0;
					}

					if (infoAddr.Address != 0)
					{
						gameModeUpdateInfo.timeStamp = gameModeArea.UpdateTimestamp;
						gameModeUpdateInfo.write(Memory.Instance);
					}
					break;
				}
			}

			return 0;
		}

		/// <summary>
		/// Delete own game object type data.
		/// </summary>
		/// <returns> 0 on success, < 0 on error. </returns>
		[HLEFunction(nid : 0xA0229362, version : 150)]
		public virtual int sceNetAdhocGameModeDeleteMaster()
		{
			checkInitialized();

			masterGameModeArea = null;
			if (replicaGameModeAreas.Count <= 0)
			{
				stopGameMode();
			}

			return 0;
		}

		/// <summary>
		/// Delete peer game object type data.
		/// </summary>
		/// <param name="id"> - The id of the replica.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
		[HLEFunction(nid : 0x0B2228E9, version : 150)]
		public virtual int sceNetAdhocGameModeDeleteReplica(int id)
		{
			checkInitialized();

			foreach (GameModeArea gameModeArea in replicaGameModeAreas)
			{
				if (gameModeArea.id == id)
				{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET LinkedList equivalent to the Java 'remove' method:
					replicaGameModeAreas.remove(gameModeArea);
					break;
				}
			}

			if (replicaGameModeAreas.Count <= 0 && masterGameModeArea == null)
			{
				stopGameMode();
			}

			return 0;
		}
	}
}