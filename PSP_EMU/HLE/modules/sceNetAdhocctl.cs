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
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;


	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceNetAdhocctlPeerInfo = pspsharp.HLE.kernel.types.SceNetAdhocctlPeerInfo;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using Wlan = pspsharp.hardware.Wlan;
	using INetworkAdapter = pspsharp.network.INetworkAdapter;

	using Logger = org.apache.log4j.Logger;

	public class sceNetAdhocctl : HLEModule
	{
		public static Logger log = Modules.getLogger("sceNetAdhocctl");

		public const int PSP_ADHOCCTL_EVENT_ERROR = 0;
		public const int PSP_ADHOCCTL_EVENT_CONNECTED = 1;
		public const int PSP_ADHOCCTL_EVENT_DISCONNECTED = 2;
		public const int PSP_ADHOCCTL_EVENT_SCAN = 3;
		public const int PSP_ADHOCCTL_EVENT_GAME = 4;
		public const int PSP_ADHOCCTL_EVENT_DISCOVER = 5;
		public const int PSP_ADHOCCTL_EVENT_WOL = 6;
		public const int PSP_ADHOCCTL_EVENT_WOL_INTERRUPTED = 7;

		public const int PSP_ADHOCCTL_STATE_DISCONNECTED = 0;
		public const int PSP_ADHOCCTL_STATE_CONNECTED = 1;
		public const int PSP_ADHOCCTL_STATE_SCAN = 2;
		public const int PSP_ADHOCCTL_STATE_GAME = 3;
		public const int PSP_ADHOCCTL_STATE_DISCOVER = 4;
		public const int PSP_ADHOCCTL_STATE_WOL = 5;

		public const int PSP_ADHOCCTL_MODE_NORMAL = 0;
		public const int PSP_ADHOCCTL_MODE_GAMEMODE = 1;
		public const int PSP_ADHOCCTL_MODE_NONE = -1;

		public const int NICK_NAME_LENGTH = 128;
		public const int GROUP_NAME_LENGTH = 8;
		public const int IBSS_NAME_LENGTH = 6;
		public const int ADHOC_ID_LENGTH = 9;
		public const int MAX_GAME_MODE_MACS = 16;

		private bool isInitialized;
		protected internal int adhocctlCurrentState;
		protected internal string adhocctlCurrentGroup;
		protected internal string adhocctlCurrentIBSS;
		protected internal int adhocctlCurrentMode;
		protected internal int adhocctlCurrentChannel;
		protected internal int adhocctlCurrentType;
		protected internal string adhocctlCurrentAdhocID;
		protected internal bool doTerminate;
		protected internal SceKernelThreadInfo adhocctlThread;
		private bool doScan;
		private volatile long scanStartMillis;
		private const int SCAN_DURATION_MILLIS = 700;
		private bool doDisconnect;
		private bool doJoin;
		private bool gameModeJoinComplete;
		protected internal LinkedList<AdhocctlPeer> peers;
		protected internal LinkedList<AdhocctlNetwork> networks;
		protected internal LinkedList<pspNetMacAddress> gameModeMacs;
		protected internal LinkedList<pspNetMacAddress> requiredGameModeMacs;
		protected internal INetworkAdapter networkAdapter;
		private long connectCompleteTimestamp;
		// Some games have problems when the PSP_ADHOCCTL_EVENT_CONNECTED
		// is sent too quickly after connecting to a network.
		// The connection will be set CONNECTED with a delay of 200ms.
		private const int CONNECT_COMPLETE_DELAY_MILLIS = 200;

		private Dictionary<int, AdhocctlHandler> adhocctlIdMap = new Dictionary<int, AdhocctlHandler>();
		private const string adhocctlHandlerIdPurpose = "sceNetAdhocctl-Handler";

		protected internal class AdhocctlHandler
		{
			private readonly sceNetAdhocctl outerInstance;

			internal int entryAddr;
			internal int currentEvent;
			internal int currentError;
			internal int currentArg;
			internal readonly int id;

			internal AdhocctlHandler(sceNetAdhocctl outerInstance, int addr, int arg)
			{
				this.outerInstance = outerInstance;
				entryAddr = addr;
				currentArg = arg;
				// PSP returns a handler ID between 0 and 3
				id = SceUidManager.getNewId(adhocctlHandlerIdPurpose, 0, 3);
			}

			protected internal virtual void triggerAdhocctlHandler()
			{
				SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
				if (thread != null)
				{
					Modules.ThreadManForUserModule.executeCallback(thread, entryAddr, null, true, currentEvent, currentError, currentArg);
				}
			}

			protected internal virtual int Id
			{
				get
				{
					return id;
				}
			}

			protected internal virtual int Event
			{
				set
				{
					currentEvent = value;
				}
			}

			protected internal virtual int Error
			{
				set
				{
					currentError = value;
				}
			}

			protected internal virtual void delete()
			{
				SceUidManager.releaseId(id, adhocctlHandlerIdPurpose);
			}

			public override string ToString()
			{
				return string.Format("AdhocctlHandler[id={0:D}, entry=0x{1:X8}, arg=0x{2:X8}]", Id, entryAddr, currentArg);
			}
		}

		protected internal class AdhocctlPeer
		{
			public string nickName;
			public sbyte[] macAddress;
			public long timestamp;

			public AdhocctlPeer(string nickName, sbyte[] macAddress)
			{
				this.nickName = nickName;
				this.macAddress = macAddress.Clone();
				updateTimestamp();
			}

			public virtual void updateTimestamp()
			{
				timestamp = CurrentTimestamp;
			}

			public virtual bool Equals(string nickName, sbyte[] macAddress)
			{
				return nickName.Equals(this.nickName) && sceNetAdhoc.isSameMacAddress(macAddress, this.macAddress);
			}

			public virtual bool Equals(sbyte[] macAddress)
			{
				return sceNetAdhoc.isSameMacAddress(macAddress, this.macAddress);
			}

			public override string ToString()
			{
				return string.Format("nickName='{0}', macAddress={1}, timestamp={2:D}", nickName, sceNet.convertMacAddressToString(macAddress), timestamp);
			}
		}

		protected internal class AdhocctlNetwork
		{
			/// <summary>
			/// Channel number </summary>
			public int channel;
			/// <summary>
			/// Name of the connection (alphanumeric characters only) </summary>
			public string name;
			/// <summary>
			/// The BSSID </summary>
			public string bssid;
			/// <summary>
			/// mode </summary>
			public int mode;

			public virtual bool Equals(int channel, string name, string bssid, int mode)
			{
				return channel == this.channel && name.Equals(this.name) && bssid.Equals(this.bssid) && mode == this.mode;
			}

			public override string ToString()
			{
				return string.Format("AdhocctlNetwork[channel={0:D}, name='{1}', bssid='{2}', mode={3:D}]", channel, name, bssid, mode);
			}
		}

		public override void start()
		{
			peers = new LinkedList<sceNetAdhocctl.AdhocctlPeer>();
			networks = new LinkedList<sceNetAdhocctl.AdhocctlNetwork>();
			gameModeMacs = new LinkedList<pspNetMacAddress>();
			requiredGameModeMacs = new LinkedList<pspNetMacAddress>();
			adhocctlCurrentIBSS = "pspsharp";
			adhocctlCurrentMode = PSP_ADHOCCTL_MODE_NONE;
			adhocctlCurrentChannel = Wlan.AdhocChannel;
			isInitialized = false;
			networkAdapter = Modules.sceNetModule.NetworkAdapter;

			base.start();
		}

		protected internal static long CurrentTimestamp
		{
			get
			{
				return Emulator.Clock.microTime();
			}
		}

		protected internal virtual void checkInitialized()
		{
			if (!isInitialized)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_NET_ADHOCCTL_NOT_INITIALIZED);
			}
		}

		public virtual void hleNetAdhocctlAddGameModeMac(sbyte[] macAddr)
		{
			foreach (pspNetMacAddress macAddress in gameModeMacs)
			{
				if (sceNetAdhoc.isSameMacAddress(macAddress.macAddress, macAddr))
				{
					// Already in the list
					return;
				}
			}

			pspNetMacAddress macAddress = new pspNetMacAddress();
			macAddress.MacAddress = macAddr;
			gameModeMacs.AddLast(macAddress);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Adding new Game Mode MAC: {0}", macAddress));
			}
		}

		private void doConnect()
		{
			if (!string.ReferenceEquals(adhocctlCurrentGroup, null) && networkAdapter.ConnectComplete)
			{
				long now = Emulator.Clock.currentTimeMillis();
				if (now >= connectCompleteTimestamp)
				{
					if (adhocctlCurrentMode == PSP_ADHOCCTL_MODE_GAMEMODE)
					{
						State = PSP_ADHOCCTL_STATE_GAME;
						notifyAdhocctlHandler(PSP_ADHOCCTL_EVENT_GAME, 0);
					}
					else
					{
						State = PSP_ADHOCCTL_STATE_CONNECTED;
						notifyAdhocctlHandler(PSP_ADHOCCTL_EVENT_CONNECTED, 0);
					}
					doJoin = false;
				}
			}
		}

		private int AdhocctlThreadPollDelay
		{
			get
			{
				// Poll every 100ms
				const int quickPollDelay = 100000;
    
				if (adhocctlCurrentState == PSP_ADHOCCTL_STATE_SCAN)
				{
					// Scanning...
					return quickPollDelay;
				}
				if (doJoin)
				{
					// Joining...
					return quickPollDelay;
				}
				if (adhocctlCurrentState == PSP_ADHOCCTL_STATE_DISCONNECTED && !string.ReferenceEquals(adhocctlCurrentGroup, null))
				{
					// Connecting or Creating...
					return quickPollDelay;
				}
    
				// Poll every 500ms
				return 500000;
			}
		}

		public virtual void hleNetAdhocctlThread(Processor processor)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;

			if (log.DebugEnabled)
			{
				log.debug("hleNetAdhocctlThread");
			}

			if (doTerminate)
			{
				State = PSP_ADHOCCTL_STATE_DISCONNECTED;
				notifyAdhocctlHandler(PSP_ADHOCCTL_EVENT_DISCONNECTED, 0);
				setGroupName(null, PSP_ADHOCCTL_MODE_NONE);
			}
			else if (doDisconnect)
			{
				State = PSP_ADHOCCTL_STATE_DISCONNECTED;
				notifyAdhocctlHandler(PSP_ADHOCCTL_EVENT_DISCONNECTED, 0);
				setGroupName(null, PSP_ADHOCCTL_MODE_NONE);
				doDisconnect = false;
			}
			else if (doScan)
			{
				State = PSP_ADHOCCTL_STATE_SCAN;
				scanStartMillis = Emulator.Clock.milliTime();
				doScan = false;
			}
			else if (doJoin)
			{
				if (adhocctlCurrentMode == PSP_ADHOCCTL_MODE_GAMEMODE)
				{
					// Join complete when all the required MACs have joined
					if (requiredGameModeMacs.Count > 0 && gameModeMacs.Count >= requiredGameModeMacs.Count)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("All GameMode MACs have joined, GameMode Join is now complete"));
						}
						hleNetAdhocctlSetGameModeJoinComplete(true);

						// Make sure the list of game mode MACs is in the same order as the one
						// given at sceNetAdhocctlCreateEnterGameMode
						gameModeMacs.Clear();
						gameModeMacs.addAll(requiredGameModeMacs);
					}

					if (gameModeJoinComplete)
					{
						doConnect();
					}
					else
					{
						// Add own MAC to list of game mode MACs
						hleNetAdhocctlAddGameModeMac(Wlan.MacAddress);
					}
				}
				else
				{
					doConnect();
				}
			}
			else if (adhocctlCurrentState == PSP_ADHOCCTL_STATE_DISCONNECTED)
			{
				doConnect();
			}

			if (adhocctlCurrentState == PSP_ADHOCCTL_STATE_CONNECTED || adhocctlCurrentState == PSP_ADHOCCTL_STATE_GAME || doJoin)
			{
				networkAdapter.updatePeers();
			}
			else if (adhocctlCurrentState == PSP_ADHOCCTL_STATE_SCAN)
			{
				networkAdapter.updatePeers();

				// End of SCAN?
				long now = Emulator.Clock.milliTime();
				if (now - scanStartMillis > SCAN_DURATION_MILLIS)
				{
					// Return to DISCONNECTED state and trigger SCAN event
					State = PSP_ADHOCCTL_STATE_DISCONNECTED;
					notifyAdhocctlHandler(PSP_ADHOCCTL_EVENT_SCAN, 0);
				}
			}

			if (doTerminate)
			{
				// Exit thread with status 0
				processor.cpu._v0 = 0;
				threadMan.hleKernelExitDeleteThread();
				adhocctlThread = null;
				doTerminate = false;
			}
			else
			{
				threadMan.hleKernelDelayThread(AdhocctlThreadPollDelay, false);
			}
		}

		protected internal virtual int State
		{
			set
			{
				adhocctlCurrentState = value;
			}
		}

		protected internal virtual void setGroupName(string groupName, int mode)
		{
			adhocctlCurrentGroup = groupName;
			adhocctlCurrentMode = mode;
			gameModeJoinComplete = false;
			gameModeMacs.Clear();

			if (!string.ReferenceEquals(groupName, null))
			{
				// Some games have problems when the PSP_ADHOCCTL_EVENT_CONNECTED
				// is sent too quickly after connecting to a network.
				// The connection will be set CONNECTED with a small delay.
				connectCompleteTimestamp = Emulator.Clock.currentTimeMillis() + CONNECT_COMPLETE_DELAY_MILLIS;
			}
		}

		public virtual void hleNetAdhocctlConnect(string groupName)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleNetAdhocctlConnect groupName='{0}'", groupName));
			}

			if (string.ReferenceEquals(hleNetAdhocctlGetGroupName(), null) || !hleNetAdhocctlGetGroupName().Equals(groupName))
			{
				setGroupName(groupName, PSP_ADHOCCTL_MODE_NORMAL);

				networkAdapter.sceNetAdhocctlConnect();
			}
		}

		public virtual void hleNetAdhocctlConnectGame(string groupName)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleNetAdhocctlConnectGame groupName='{0}'", groupName));
			}
			setGroupName(groupName, PSP_ADHOCCTL_MODE_GAMEMODE);
		}

		public virtual int hleNetAdhocctlGetState()
		{
			return adhocctlCurrentState;
		}

		protected internal virtual void notifyAdhocctlHandler(int @event, int error)
		{
			foreach (AdhocctlHandler handler in adhocctlIdMap.Values)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Notifying handler {0} with event={1:D}, error={2:D}", handler, @event, error));
				}
				handler.Event = @event;
				handler.Error = error;
				handler.triggerAdhocctlHandler();
			}
		}

		public virtual string hleNetAdhocctlGetAdhocID()
		{
			return adhocctlCurrentAdhocID;
		}

		public virtual string hleNetAdhocctlGetGroupName()
		{
			return adhocctlCurrentGroup;
		}

		public virtual string hleNetAdhocctlGetIBSS()
		{
			return adhocctlCurrentIBSS;
		}

		public virtual int hleNetAdhocctlGetMode()
		{
			return adhocctlCurrentMode;
		}

		public virtual int hleNetAdhocctlGetChannel()
		{
			return adhocctlCurrentChannel;
		}

		public virtual void hleNetAdhocctlAddNetwork(string groupName, pspNetMacAddress mac, int mode)
		{
			hleNetAdhocctlAddNetwork(groupName, mac, adhocctlCurrentChannel, adhocctlCurrentIBSS, mode);
		}

		public virtual void hleNetAdhocctlAddNetwork(string groupName, pspNetMacAddress mac, int channel, string ibss, int mode)
		{
			bool found = false;
			foreach (AdhocctlNetwork network in networks)
			{
				if (network.Equals(channel, groupName, ibss, mode))
				{
					found = true;
					break;
				}
			}

			if (!found)
			{
				AdhocctlNetwork network = new AdhocctlNetwork();
				network.channel = channel;
				network.name = groupName;
				network.bssid = ibss;
				network.mode = mode;
				networks.AddLast(network);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("New network discovered {0}", network));
				}
			}
		}

		public virtual void hleNetAdhocctlScanComplete()
		{
			// Force a completion of the scan at the next run of hleNetAdhocctlThread.
			// Note: the scan completion has to be executed from a PSP thread because it
			// is triggering a callback.
			scanStartMillis = 0;
		}

		public virtual void hleNetAdhocctlAddPeer(string nickName, pspNetMacAddress mac)
		{
			   bool peerFound = false;
			   foreach (AdhocctlPeer peer in peers)
			   {
				   if (peer.Equals(nickName, mac.macAddress))
				   {
					   // Update the timestamp
					   peer.updateTimestamp();
					   peerFound = true;
					   break;
				   }
			   }

			   if (!peerFound)
			   {
				AdhocctlPeer peer = new AdhocctlPeer(nickName, mac.macAddress);
				peers.AddLast(peer);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("New peer discovered {0}", peer));
				}
			   }
		}

		public virtual IList<string> PeersNickName
		{
			get
			{
				IList<string> nickNames = new LinkedList<string>();
				foreach (AdhocctlPeer peer in peers)
				{
					nickNames.Add(peer.nickName);
				}
    
				return nickNames;
			}
		}

		public virtual int NumberPeers
		{
			get
			{
				return peers.Count;
			}
		}

		public virtual string getPeerNickName(sbyte[] macAddress)
		{
			foreach (AdhocctlPeer peer in peers)
			{
				if (peer.Equals(macAddress))
				{
					return peer.nickName;
				}
			}

			return null;
		}

		public virtual void hleNetAdhocctlDeletePeer(sbyte[] macAddress)
		{
			foreach (AdhocctlPeer peer in peers)
			{
				if (peer.Equals(macAddress))
				{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET LinkedList equivalent to the Java 'remove' method:
					peers.remove(peer);

					if (log.DebugEnabled)
					{
						log.debug(string.Format("Peer deleted {0}", peer));
					}
					break;
				}
			}
		}

		public virtual void hleNetAdhocctlPeerUpdateTimestamp(sbyte[] macAddress)
		{
			foreach (AdhocctlPeer peer in peers)
			{
				if (peer.Equals(macAddress))
				{
					peer.updateTimestamp();
					break;
				}
			}
		}

		public virtual bool GameModeComplete
		{
			get
			{
				// The Join for GameMode is complete when all the required MACs have joined
				return gameModeMacs.Count >= requiredGameModeMacs.Count;
			}
		}

		public virtual IList<pspNetMacAddress> hleNetAdhocctlGetRequiredGameModeMacs()
		{
			return requiredGameModeMacs;
		}

		public virtual void hleNetAdhocctlSetGameModeJoinComplete(bool gameModeJoinComplete)
		{
			this.gameModeJoinComplete = gameModeJoinComplete;
		}

		public virtual void hleNetAdhocctlSetGameModeMacs(sbyte[][] gameModeMacs)
		{
			// Make sure the list of game mode MACs is in the same order as the one
			// given at sceNetAdhocctlCreateEnterGameMode
			this.gameModeMacs.Clear();
			for (int i = 0; i < gameModeMacs.Length; i++)
			{
				hleNetAdhocctlAddGameModeMac(gameModeMacs[i]);
			}
		}

		public static void fillNextPointersInLinkedList(TPointer buffer, int size, int elementSize)
		{
			for (int offset = 0; offset < size; offset += elementSize)
			{
				if (offset + elementSize >= size)
				{
					// Last one
					buffer.setValue32(offset, 0);
				}
				else
				{
					// Pointer to next one
					buffer.setValue32(offset, buffer.Address + offset + elementSize);
				}
			}
		}

		/// <summary>
		/// Initialise the Adhoc control library
		/// </summary>
		/// <param name="stacksize"> - Stack size of the adhocctl thread. Set to 0x2000 </param>
		/// <param name="priority"> - Priority of the adhocctl thread. Set to 0x30 </param>
		/// <param name="product"> - Pass a filled in ::productStruct
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE26F226E, version = 150, checkInsideInterrupt = true) public int sceNetAdhocctlInit(int stackSize, int priority, @CanBeNull pspsharp.HLE.TPointer product)
		[HLEFunction(nid : 0xE26F226E, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetAdhocctlInit(int stackSize, int priority, TPointer product)
		{
			if (isInitialized)
			{
				return SceKernelErrors.ERROR_NET_ADHOCCTL_ALREADY_INITIALIZED;
			}

			if (product.NotNull)
			{
				adhocctlCurrentType = product.getValue32(0); // 0 - Commercial type / 1 - Debug type.
				adhocctlCurrentAdhocID = product.getStringNZ(4, ADHOC_ID_LENGTH);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Found product data: type={0:D}, AdhocID='{1}'", adhocctlCurrentType, adhocctlCurrentAdhocID));
				}
			}

			State = PSP_ADHOCCTL_STATE_DISCONNECTED;
			doTerminate = false;
			doScan = false;
			doDisconnect = false;
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			adhocctlThread = threadMan.hleKernelCreateThread("SceNetAdhocctl", ThreadManForUser.NET_ADHOC_CTL_LOOP_ADDRESS, priority, stackSize, 0, 0, SysMemUserForUser.USER_PARTITION_ID);
			threadMan.hleKernelStartThread(adhocctlThread, 0, 0, adhocctlThread.gpReg_addr);

			networkAdapter.sceNetAdhocctlInit();

			isInitialized = true;

			return 0;
		}

		/// <summary>
		/// Terminate the Adhoc control library
		/// </summary>
		/// <returns> 0 on success, < on error. </returns>
		[HLEFunction(nid : 0x9D689E13, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetAdhocctlTerm()
		{
			doTerminate = true;
			isInitialized = false;

			networkAdapter.sceNetAdhocctlTerm();

			return 0;
		}

		/// <summary>
		/// Connect to the Adhoc control
		/// </summary>
		/// <param name="name"> - The name of the connection (maximum 8 alphanumeric characters).
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0AD043ED, version = 150, checkInsideInterrupt = true) public int sceNetAdhocctlConnect(@CanBeNull @StringInfo(maxLength=GROUP_NAME_LENGTH) pspsharp.HLE.PspString groupName)
		[HLEFunction(nid : 0x0AD043ED, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetAdhocctlConnect(PspString groupName)
		{
			checkInitialized();

			hleNetAdhocctlConnect(groupName.String);

			return 0;
		}

		/// <summary>
		/// Connect to the Adhoc control (as a host)
		/// </summary>
		/// <param name="name"> - The name of the connection (maximum 8 alphanumeric characters).
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xEC0635C1, version = 150, checkInsideInterrupt = true) public int sceNetAdhocctlCreate(@CanBeNull @StringInfo(maxLength=GROUP_NAME_LENGTH) pspsharp.HLE.PspString groupName)
		[HLEFunction(nid : 0xEC0635C1, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetAdhocctlCreate(PspString groupName)
		{
			checkInitialized();

			setGroupName(groupName.String, PSP_ADHOCCTL_MODE_NORMAL);

			networkAdapter.sceNetAdhocctlCreate();

			return 0;
		}

		/// <summary>
		/// Connect to the Adhoc control (as a client)
		/// </summary>
		/// <param name="scaninfo"> - A valid ::SceNetAdhocctlScanInfo struct that has been filled by sceNetAchocctlGetScanInfo
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x5E7F79C9, version = 150, checkInsideInterrupt = true) public int sceNetAdhocctlJoin(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=28, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer scanInfoAddr)
		[HLEFunction(nid : 0x5E7F79C9, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetAdhocctlJoin(TPointer scanInfoAddr)
		{
			checkInitialized();

			if (scanInfoAddr.AddressGood)
			{
				// IBSS Data field.
				int nextAddr = scanInfoAddr.getValue32(0); // Next group data.
				int ch = scanInfoAddr.getValue32(4);
				string groupName = scanInfoAddr.getStringNZ(8, GROUP_NAME_LENGTH);
				string bssID = scanInfoAddr.getStringNZ(16, IBSS_NAME_LENGTH);
				int mode = scanInfoAddr.getValue32(24);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceNetAdhocctlJoin nextAddr 0x{0:X8}, ch {1:D}, groupName '{2}', bssID '{3}', mode {4:D}", nextAddr, ch, groupName, bssID, mode));
				}
				doJoin = true;
				setGroupName(groupName, PSP_ADHOCCTL_MODE_NORMAL);

				networkAdapter.sceNetAdhocctlJoin();
			}

			return 0;
		}

		/// <summary>
		/// Scan the adhoc channels
		/// </summary>
		/// <returns> 0 on success, < 0 on error. </returns>
		[HLEFunction(nid : 0x08FFF7A0, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetAdhocctlScan()
		{
			checkInitialized();

			doScan = true;

			networkAdapter.sceNetAdhocctlScan();

			return 0;
		}

		/// <summary>
		/// Disconnect from the Adhoc control
		/// </summary>
		/// <returns> 0 on success, < 0 on error </returns>
		[HLEFunction(nid : 0x34401D65, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetAdhocctlDisconnect()
		{
			checkInitialized();

			doDisconnect = true;

			networkAdapter.sceNetAdhocctlDisconnect();

			// Delete all the peers
			while (peers.Count > 0)
			{
				AdhocctlPeer peer = peers.get(0);
				hleNetAdhocctlDeletePeer(peer.macAddress);
			}

			return 0;
		}

		/// <summary>
		/// Register an adhoc event handler
		/// </summary>
		/// <param name="handler"> - The event handler. </param>
		/// <param name="unknown60"> - Pass NULL.
		/// </param>
		/// <returns> Handler id on success, < 0 on error. </returns>
		[HLEFunction(nid : 0x20B317A0, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetAdhocctlAddHandler(TPointer adhocctlHandlerAddr, int adhocctlHandlerArg)
		{
			checkInitialized();

			AdhocctlHandler adhocctlHandler = new AdhocctlHandler(this, adhocctlHandlerAddr.Address, adhocctlHandlerArg);
			int id = adhocctlHandler.Id;
			if (id == SceUidManager.INVALID_ID)
			{
				return SceKernelErrors.ERROR_NET_ADHOCCTL_TOO_MANY_HANDLERS;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetAdhocctlAddHandler returning id=0x{0:X}", id));
			}
			adhocctlIdMap[id] = adhocctlHandler;

			return id;
		}

		/// <summary>
		/// Delete an adhoc event handler
		/// </summary>
		/// <param name="id"> - The handler id as returned by sceNetAdhocctlAddHandler.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
		[HLEFunction(nid : 0x6402490B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetAdhocctlDelHandler(int id)
		{
			checkInitialized();

			AdhocctlHandler handler = adhocctlIdMap.Remove(id);
			if (handler != null)
			{
				handler.delete();
			}

			return 0;
		}

		/// <summary>
		/// Get the state of the Adhoc control
		/// </summary>
		/// <param name="event"> - Pointer to an integer to receive the status. Can continue when it becomes 1.
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x75ECD386, version = 150, checkInsideInterrupt = true) public int sceNetAdhocctlGetState(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 stateAddr)
		[HLEFunction(nid : 0x75ECD386, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetAdhocctlGetState(TPointer32 stateAddr)
		{
			checkInitialized();

			stateAddr.setValue(adhocctlCurrentState);

			return 0;
		}

		/// <summary>
		/// Get the adhoc ID
		/// </summary>
		/// <param name="product"> - A pointer to a  ::productStruct
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
		[HLEFunction(nid : 0x362CBE8F, version : 150)]
		public virtual int sceNetAdhocctlGetAdhocId(TPointer addr)
		{
			checkInitialized();

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetAdhocctlGetAdhocId returning type={0:D}, adhocID='{1}'", adhocctlCurrentType, adhocctlCurrentAdhocID));
			}
			addr.setValue32(0, adhocctlCurrentType);
			addr.setStringNZ(4, ADHOC_ID_LENGTH, adhocctlCurrentAdhocID);

			return 0;
		}

		/// <summary>
		/// Get a list of peers
		/// </summary>
		/// <param name="length"> - The length of the list. </param>
		/// <param name="buf"> - An allocated area of size length.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE162CB14, version = 150) public int sceNetAdhocctlGetPeerList(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer32 sizeAddr, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=152, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buf)
		[HLEFunction(nid : 0xE162CB14, version : 150)]
		public virtual int sceNetAdhocctlGetPeerList(TPointer32 sizeAddr, TPointer buf)
		{
			checkInitialized();

			int size = sizeAddr.getValue();
			SceNetAdhocctlPeerInfo peerInfo = new SceNetAdhocctlPeerInfo();
			sizeAddr.setValue(peerInfo.@sizeof() * peers.Count);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetAdhocctlGetPeerList returning size={0:D}", sizeAddr.getValue()));
			}

			if (buf.NotNull)
			{
				int offset = 0;
				foreach (AdhocctlPeer peer in peers)
				{
					// Check if enough space available to write the next structure
					if (offset + peerInfo.@sizeof() > size || peer == null)
					{
						break;
					}

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetAdhocctlGetPeerList returning {0} at 0x{1:X8}", peer, buf.Address + offset));
					}

					peerInfo.nickName = peer.nickName;
					peerInfo.macAddress = new pspNetMacAddress();
					peerInfo.macAddress.MacAddress = peer.macAddress;
					peerInfo.timestamp = peer.timestamp;
					peerInfo.write(buf, offset);

					offset += peerInfo.@sizeof();
				}

				fillNextPointersInLinkedList(buf, offset, peerInfo.@sizeof());
			}

			return 0;
		}

		/// <summary>
		/// Get peer information
		/// </summary>
		/// <param name="mac"> - The mac address of the peer. </param>
		/// <param name="size"> - Size of peerinfo. </param>
		/// <param name="peerinfo"> - Pointer to store the information.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8DB83FDC, version = 150) public int sceNetAdhocctlGetPeerInfo(pspsharp.HLE.kernel.types.pspNetMacAddress macAddress, int size, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.previousParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer peerInfoAddr)
		[HLEFunction(nid : 0x8DB83FDC, version : 150)]
		public virtual int sceNetAdhocctlGetPeerInfo(pspNetMacAddress macAddress, int size, TPointer peerInfoAddr)
		{
			checkInitialized();

			int result = SceKernelErrors.ERROR_NET_ADHOC_NO_ENTRY;
			if (sceNetAdhoc.isMyMacAddress(macAddress.macAddress))
			{
				SceNetAdhocctlPeerInfo peerInfo = new SceNetAdhocctlPeerInfo();
				peerInfo.nickName = sceUtility.SystemParamNickname;
				peerInfo.macAddress = new pspNetMacAddress(Wlan.MacAddress);
				peerInfo.timestamp = CurrentTimestamp;
				peerInfo.write(peerInfoAddr);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceNetAdhocctlGetPeerInfo for own MAC address, returning {0}", peerInfo));
				}
				result = 0;
			}
			else
			{
				foreach (AdhocctlPeer peer in peers)
				{
					if (macAddress.Equals(peer.macAddress))
					{
						SceNetAdhocctlPeerInfo peerInfo = new SceNetAdhocctlPeerInfo();
						peerInfo.nickName = peer.nickName;
						peerInfo.macAddress = new pspNetMacAddress(peer.macAddress);
						peerInfo.timestamp = peer.timestamp;
						peerInfo.write(peerInfoAddr);
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceNetAdhocctlGetPeerInfo returning {0}", peerInfo));
						}
						result = 0;
						break;
					}
				}
			}
			if (result != 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceNetAdhocctlGetPeerInfo returning 0x{0:X8}", result));
				}
			}

			return result;
		}

		/// <summary>
		/// Get mac address from nickname
		/// </summary>
		/// <param name="nickname"> - The nickname. </param>
		/// <param name="length"> - The length of the list. </param>
		/// <param name="buf"> - An allocated area of size length.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x99560ABE, version = 150) public int sceNetAdhocctlGetAddrByName(@StringInfo(maxLength=NICK_NAME_LENGTH) pspsharp.HLE.PspString nickName, pspsharp.HLE.TPointer32 sizeAddr, @CanBeNull pspsharp.HLE.TPointer buf)
		[HLEFunction(nid : 0x99560ABE, version : 150)]
		public virtual int sceNetAdhocctlGetAddrByName(PspString nickName, TPointer32 sizeAddr, TPointer buf)
		{
			checkInitialized();

			// Search for peers matching the given nick name
			LinkedList<AdhocctlPeer> matchingPeers = new LinkedList<sceNetAdhocctl.AdhocctlPeer>();
			foreach (AdhocctlPeer peer in peers)
			{
				if (nickName.Equals(peer.nickName))
				{
					matchingPeers.AddLast(peer);
				}
			}

			int size = sizeAddr.getValue();
			SceNetAdhocctlPeerInfo peerInfo = new SceNetAdhocctlPeerInfo();
			sizeAddr.setValue(peerInfo.@sizeof() * matchingPeers.Count);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetAdhocctlGetAddrByName returning size={0:D}", sizeAddr.getValue()));
			}

			if (buf.NotNull)
			{
				int offset = 0;
				foreach (AdhocctlPeer peer in matchingPeers)
				{
					// Check if enough space available to write the next structure
					if (offset + peerInfo.@sizeof() > size)
					{
						break;
					}

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetAdhocctlGetAddrByName returning {0} at 0x{1:X8}", peer, buf.Address + offset));
					}

					peerInfo.nickName = peer.nickName;
					peerInfo.macAddress = new pspNetMacAddress();
					peerInfo.macAddress.MacAddress = peer.macAddress;
					peerInfo.timestamp = peer.timestamp;
					peerInfo.write(buf, offset);

					offset += peerInfo.@sizeof();
				}

				fillNextPointersInLinkedList(buf, offset, peerInfo.@sizeof());
			}

			return 0;
		}

		/// <summary>
		/// Get nickname from a mac address
		/// </summary>
		/// <param name="mac"> - The mac address. </param>
		/// <param name="nickname"> - Pointer to a char buffer where the nickname will be stored.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
		[HLEFunction(nid : 0x8916C003, version : 150)]
		public virtual int sceNetAdhocctlGetNameByAddr(pspNetMacAddress macAddress, TPointer nickNameAddr)
		{
			checkInitialized();

			string nickName = "";
			foreach (AdhocctlPeer peer in peers)
			{
				if (sceNetAdhoc.isSameMacAddress(macAddress.macAddress, peer.macAddress))
				{
					nickName = peer.nickName;
				}
			}

			nickNameAddr.setStringNZ(NICK_NAME_LENGTH, nickName);

			return 0;
		}

		/// <summary>
		/// Get Adhocctl parameter
		/// </summary>
		/// <param name="params"> - Pointer to a ::SceNetAdhocctlParams
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
		[HLEFunction(nid : 0xDED9D28E, version : 150)]
		public virtual int sceNetAdhocctlGetParameter(TPointer paramsAddr)
		{
			checkInitialized();

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetAdhocctlGetParameter returning channel={0:D}, group='{1}', IBSS='{2}', nickName='{3}'", adhocctlCurrentChannel, adhocctlCurrentGroup, adhocctlCurrentIBSS, sceUtility.SystemParamNickname));
			}
			paramsAddr.setValue32(0, adhocctlCurrentChannel);
			paramsAddr.setStringNZ(4, GROUP_NAME_LENGTH, adhocctlCurrentGroup);
			paramsAddr.setStringNZ(12, IBSS_NAME_LENGTH, adhocctlCurrentIBSS);
			paramsAddr.setStringNZ(18, NICK_NAME_LENGTH, sceUtility.SystemParamNickname);

			return 0;
		}

		/// <summary>
		/// Get the results of a scan
		/// </summary>
		/// <param name="length"> - The length of the list. </param>
		/// <param name="buf"> - An allocated area of size length.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x81AEE1BE, version = 150) public int sceNetAdhocctlGetScanInfo(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer32 sizeAddr, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=112, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buf)
		[HLEFunction(nid : 0x81AEE1BE, version : 150)]
		public virtual int sceNetAdhocctlGetScanInfo(TPointer32 sizeAddr, TPointer buf)
		{
			checkInitialized();

			const int scanInfoSize = 28;

			int size = sizeAddr.getValue();
			sizeAddr.setValue(scanInfoSize * networks.Count);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetAdhocctlGetScanInfo returning size={0:D}", sizeAddr.getValue()));
			}

			if (buf.NotNull)
			{
				int offset = 0;
				foreach (AdhocctlNetwork network in networks)
				{
					// Check if enough space available to write the next structure
					if (offset + scanInfoSize > size || network == null)
					{
						break;
					}

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceNetAdhocctlGetScanInfo returning {0} at 0x{1:X8}", network, buf.Address + offset));
					}

					/// <summary>
					/// Pointer to next Network structure in list: will be written later </summary>
					offset += 4;

					/// <summary>
					/// Channel number </summary>
					buf.setValue32(offset, network.channel);
					offset += 4;

					/// <summary>
					/// Name of the connection (alphanumeric characters only) </summary>
					buf.setStringNZ(offset, GROUP_NAME_LENGTH, network.name);
					offset += GROUP_NAME_LENGTH;

					/// <summary>
					/// The BSSID </summary>
					buf.setStringNZ(offset, IBSS_NAME_LENGTH, network.bssid);
					offset += IBSS_NAME_LENGTH;

					/// <summary>
					/// Padding </summary>
					buf.setValue16(offset, (short) 0);
					offset += 2;

					/// <summary>
					/// Mode </summary>
					buf.setValue32(offset, network.mode);
					offset += 4;
				}

				fillNextPointersInLinkedList(buf, offset, scanInfoSize);
			}

			return 0;
		}

		/// <summary>
		/// Connect to the Adhoc control game mode (as a host)
		/// </summary>
		/// <param name="name"> - The name of the connection (maximum 8 alphanumeric characters). </param>
		/// <param name="unknown"> - Pass 1. </param>
		/// <param name="num"> - The total number of players (including the host). </param>
		/// <param name="macs"> - A pointer to a list of the participating mac addresses, host first, then clients. </param>
		/// <param name="timeout"> - Timeout in microseconds. </param>
		/// <param name="unknown2"> - pass 0.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA5C055CE, version = 150) public int sceNetAdhocctlCreateEnterGameMode(@CanBeNull @StringInfo(maxLength=GROUP_NAME_LENGTH) pspsharp.HLE.PspString groupName, int unknown, int num, pspsharp.HLE.TPointer macsAddr, int timeout, int unknown2)
		[HLEFunction(nid : 0xA5C055CE, version : 150)]
		public virtual int sceNetAdhocctlCreateEnterGameMode(PspString groupName, int unknown, int num, TPointer macsAddr, int timeout, int unknown2)
		{
			checkInitialized();

			if (unknown <= 0 || unknown > 3 || num < 2 || num > 16)
			{
				return SceKernelErrors.ERROR_NET_ADHOCCTL_INVALID_PARAMETER;
			}

			if (unknown == 1 && num > 4)
			{
				return SceKernelErrors.ERROR_NET_ADHOCCTL_INVALID_PARAMETER;
			}

			gameModeMacs.Clear();
			requiredGameModeMacs.Clear();
			for (int i = 0; i < num; i++)
			{
				pspNetMacAddress macAddress = new pspNetMacAddress();
				macAddress.read(macsAddr, i * macAddress.@sizeof());
				requiredGameModeMacs.AddLast(macAddress);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceNetAdhocctlCreateEnterGameMode macAddress#{0:D}={1}", i, macAddress));
				}
			}

			// We have to wait for all the MACs to have joined to go into CONNECTED state
			doJoin = true;
			setGroupName(groupName.String, PSP_ADHOCCTL_MODE_GAMEMODE);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB0B80E80, version = 150) public int sceNetAdhocctlCreateEnterGameModeMin()
		[HLEFunction(nid : 0xB0B80E80, version : 150)]
		public virtual int sceNetAdhocctlCreateEnterGameModeMin()
		{
			checkInitialized();

			return 0;
		}

		/// <summary>
		/// Connect to the Adhoc control game mode (as a client)
		/// </summary>
		/// <param name="name"> - The name of the connection (maximum 8 alphanumeric characters). </param>
		/// <param name="hostmac"> - The mac address of the host. </param>
		/// <param name="timeout"> - Timeout in microseconds. </param>
		/// <param name="unknown"> - pass 0.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1FF89745, version = 150) public int sceNetAdhocctlJoinEnterGameMode(@StringInfo(maxLength=GROUP_NAME_LENGTH) pspsharp.HLE.PspString groupName, pspsharp.HLE.kernel.types.pspNetMacAddress macAddress, int timeout, int unknown)
		[HLEFunction(nid : 0x1FF89745, version : 150)]
		public virtual int sceNetAdhocctlJoinEnterGameMode(PspString groupName, pspNetMacAddress macAddress, int timeout, int unknown)
		{
			checkInitialized();

			doJoin = true;
			setGroupName(groupName.String, PSP_ADHOCCTL_MODE_GAMEMODE);

			return 0;
		}

		/// <summary>
		/// Exit game mode.
		/// </summary>
		/// <returns> 0 on success, < 0 on error. </returns>
		[HLEFunction(nid : 0xCF8E084D, version : 150)]
		public virtual int sceNetAdhocctlExitGameMode()
		{
			checkInitialized();

			doDisconnect = true;
			Modules.sceNetAdhocModule.hleExitGameMode();

			return 0;
		}

		/// <summary>
		/// Get game mode information
		/// </summary>
		/// <param name="gamemodeinfo"> - Pointer to store the info.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
		[HLEFunction(nid : 0x5A014CE0, version : 150)]
		public virtual int sceNetAdhocctlGetGameModeInfo(TPointer gameModeInfoAddr)
		{
			checkInitialized();

			int offset = 0;
			gameModeInfoAddr.setValue32(offset, gameModeMacs.Count);
			offset += 4;
			foreach (pspNetMacAddress macAddress in gameModeMacs)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceNetAdhocctlGetGameModeInfo returning {0}", macAddress));
				}
				macAddress.write(gameModeInfoAddr, offset);
				offset += macAddress.@sizeof();
			}

			return 0;
		}
	}
}