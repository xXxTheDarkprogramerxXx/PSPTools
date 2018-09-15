using System;
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
//	import static pspsharp.HLE.modules.sceNetAdhocctl.IBSS_NAME_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocctl.fillNextPointersInLinkedList;


	//using Logger = org.apache.log4j.Logger;

	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using Wlan = pspsharp.hardware.Wlan;
	using Settings = pspsharp.settings.Settings;

	public class sceNetApctl : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceNetApctl");

		public const int PSP_NET_APCTL_STATE_DISCONNECTED = 0;
		public const int PSP_NET_APCTL_STATE_SCANNING = 1;
		public const int PSP_NET_APCTL_STATE_JOINING = 2;
		public const int PSP_NET_APCTL_STATE_GETTING_IP = 3;
		public const int PSP_NET_APCTL_STATE_GOT_IP = 4;
		public const int PSP_NET_APCTL_STATE_EAP_AUTH = 5;
		public const int PSP_NET_APCTL_STATE_KEY_EXCHANGE = 6;

		public const int PSP_NET_APCTL_EVENT_CONNECT_REQUEST = 0;
		public const int PSP_NET_APCTL_EVENT_SCAN_REQUEST = 1;
		public const int PSP_NET_APCTL_EVENT_SCAN_COMPLETE = 2;
		public const int PSP_NET_APCTL_EVENT_ESTABLISHED = 3;
		public const int PSP_NET_APCTL_EVENT_GET_IP = 4;
		public const int PSP_NET_APCTL_EVENT_DISCONNECT_REQUEST = 5;
		public const int PSP_NET_APCTL_EVENT_ERROR = 6;
		public const int PSP_NET_APCTL_EVENT_INFO = 7;
		public const int PSP_NET_APCTL_EVENT_EAP_AUTH = 8;
		public const int PSP_NET_APCTL_EVENT_KEY_EXCHANGE = 9;
		public const int PSP_NET_APCTL_EVENT_RECONNECT = 10;

		public const int PSP_NET_APCTL_INFO_PROFILE_NAME = 0;
		public const int PSP_NET_APCTL_INFO_BSSID = 1;
		public const int PSP_NET_APCTL_INFO_SSID = 2;
		public const int PSP_NET_APCTL_INFO_SSID_LENGTH = 3;
		public const int PSP_NET_APCTL_INFO_SECURITY_TYPE = 4;
		public const int PSP_NET_APCTL_INFO_STRENGTH = 5;
		public const int PSP_NET_APCTL_INFO_CHANNEL = 6;
		public const int PSP_NET_APCTL_INFO_POWER_SAVE = 7;
		public const int PSP_NET_APCTL_INFO_IP = 8;
		public const int PSP_NET_APCTL_INFO_SUBNETMASK = 9;
		public const int PSP_NET_APCTL_INFO_GATEWAY = 10;
		public const int PSP_NET_APCTL_INFO_PRIMDNS = 11;
		public const int PSP_NET_APCTL_INFO_SECDNS = 12;
		public const int PSP_NET_APCTL_INFO_USE_PROXY = 13;
		public const int PSP_NET_APCTL_INFO_PROXY_URL = 14;
		public const int PSP_NET_APCTL_INFO_PROXY_PORT = 15;
		public const int PSP_NET_APCTL_INFO_8021_EAP_TYPE = 16;
		public const int PSP_NET_APCTL_INFO_START_BROWSER = 17;
		public const int PSP_NET_APCTL_INFO_WIFISP = 18;
		public const int PSP_NET_APCTL_INFO_UNKNOWN19 = 19;
		private static readonly string[] apctlInfoNames = new string[] {"PROFILE_NAME", "BSSID", "SSID", "SSID_LENGTH", "SECURITY_TYPE", "STRENGTH", "CHANNEL", "POWER_SAVE", "IP", "SUBNETMASK", "GATEWAY", "PRIMDNS", "SECDNS", "USE_PROXY", "PROXY_URL", "PROXY_PORT", "8021_EAP_TYPE", "START_BROWSER", "WIFISP"};

		public const int PSP_NET_APCTL_INFO_SECURITY_TYPE_NONE = 0;
		public const int PSP_NET_APCTL_INFO_SECURITY_TYPE_WEP = 1;
		public const int PSP_NET_APCTL_INFO_SECURITY_TYPE_WPA_TKIP = 2;
		public const int PSP_NET_APCTL_INFO_SECURITY_TYPE_UNSUPPORTED = 3;
		public const int PSP_NET_APCTL_INFO_SECURITY_TYPE_WPA_AES = 4;

		public const int PSP_NET_APCTL_DESC_IBSS = 0;
		public const int PSP_NET_APCTL_DESC_SSID_NAME = 1;
		public const int PSP_NET_APCTL_DESC_SSID_NAME_LENGTH = 2;
		public const int PSP_NET_APCTL_DESC_SIGNAL_STRENGTH = 4;
		public const int PSP_NET_APCTL_DESC_SECURITY = 5;

		public const int SSID_NAME_LENGTH = 32;

		private const string dummyPrimaryDNS = "1.2.3.4";
		private const string dummySecondaryDNS = "1.2.3.5";
		private const string dummySubnetMask = "255.255.255.0";
		private const int dummySubnetMaskInt = unchecked((int)0xFFFFFF00);

		protected internal const string uidPurpose = "sceNetApctl";
		protected internal const string uidHandlerPurpose = "sceNetApctlHandler";
		protected internal int state = PSP_NET_APCTL_STATE_DISCONNECTED;
		protected internal int connectionIndex = 0;
		private static string localHostIP;
		private Dictionary<int, ApctlHandler> apctlHandlers = new Dictionary<int, ApctlHandler>();
		protected internal const int stateTransitionDelay = 100000; // 100ms
		protected internal SceKernelThreadInfo sceNetApctlThread;
		protected internal bool sceNetApctlThreadTerminate;
		private bool doScan;
		private volatile long scanStartMillis;
		private const int SCAN_DURATION_MILLIS = 700;

		public override void stop()
		{
			sceNetApctlThread = null;
			sceNetApctlThreadTerminate = false;
			doScan = false;
			apctlHandlers.Clear();
			base.stop();
		}

		protected internal class ApctlHandler
		{
			private readonly sceNetApctl outerInstance;

			internal int id;
			internal int addr;
			internal int pArg;

			internal ApctlHandler(sceNetApctl outerInstance, int id, int addr, int pArg)
			{
				this.outerInstance = outerInstance;
				this.id = id;
				this.addr = addr;
				this.pArg = pArg;
			}

			protected internal virtual void triggerHandler(int oldState, int newState, int @event, int error)
			{
				SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
				if (thread != null)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Triggering hanlder 0x{0:X8}, oldState={1:D}, newState={2:D}, event={3:D}, error=0x{4:X8}", addr, oldState, newState, @event, error));
					}
					Modules.ThreadManForUserModule.executeCallback(thread, addr, null, true, oldState, newState, @event, error, pArg);
				}
			}

			public override string ToString()
			{
				return string.Format("ApctlHandler[id={0:D}, addr=0x{1:X8}, pArg=0x{2:X8}]", id, addr, pArg);
			}
		}

		protected internal virtual void notifyHandler(int oldState, int newState, int @event, int error)
		{
			foreach (ApctlHandler handler in apctlHandlers.Values)
			{
				handler.triggerHandler(oldState, newState, @event, error);
			}
		}

		protected internal virtual void changeState(int newState)
		{
			int oldState = state;
			int error = 0;
			int @event;

			if (newState == oldState)
			{
				// State not changed, no notification
				return;
			}

			// The event is set to match tests done on a real PSP
			switch (newState)
			{
				case PSP_NET_APCTL_STATE_JOINING:
					@event = PSP_NET_APCTL_EVENT_CONNECT_REQUEST;
					break;
				case PSP_NET_APCTL_STATE_GETTING_IP:
					@event = PSP_NET_APCTL_EVENT_ESTABLISHED;
					break;
				case PSP_NET_APCTL_STATE_GOT_IP:
					@event = PSP_NET_APCTL_EVENT_GET_IP;
					break;
				case PSP_NET_APCTL_STATE_DISCONNECTED:
					if (oldState == PSP_NET_APCTL_STATE_SCANNING)
					{
						@event = PSP_NET_APCTL_EVENT_SCAN_COMPLETE;
					}
					else
					{
						@event = PSP_NET_APCTL_EVENT_DISCONNECT_REQUEST;
					}
					break;
				case PSP_NET_APCTL_STATE_SCANNING:
					@event = PSP_NET_APCTL_EVENT_SCAN_REQUEST;
					break;
				default:
					@event = PSP_NET_APCTL_EVENT_CONNECT_REQUEST;
					break;
			}

			// Set the new state before calling the handler, in case
			// the handler is calling back sceNetApctl (e.g. sceNetApctlDisconnect()).
			state = newState;

			notifyHandler(oldState, newState, @event, error);

			if (newState == PSP_NET_APCTL_STATE_JOINING)
			{
				triggerNetApctlThread();
			}
		}

		protected internal static string getApctlInfoName(int code)
		{
			if (code < 0 || code >= apctlInfoNames.Length)
			{
				return string.Format("Unknown Info {0:D}", code);
			}

			return apctlInfoNames[code];
		}

		public static string SSID
		{
			get
			{
				string ssid = null;
				try
				{
					ssid = NetworkInterface.getByInetAddress(InetAddress.LocalHost).DisplayName;
				}
				catch (SocketException e)
				{
					Console.WriteLine(e);
				}
				catch (UnknownHostException e)
				{
					Console.WriteLine(e);
				}
    
				return ssid;
			}
		}

		public static string PrimaryDNS
		{
			get
			{
				string ip = LocalHostIP;
				if (!string.ReferenceEquals(ip, null))
				{
					// Try to guess the primary DNS by replacing the last part of our
					// IP address with 1.
					// e.g.: ip=A.B.C.D -> primaryDNS=A.B.C.1
					int lastDot = ip.LastIndexOf(".", StringComparison.Ordinal);
					if (lastDot >= 0)
					{
						string primaryDNS = ip.Substring(0, lastDot) + ".1";
						return primaryDNS;
					}
				}
    
				return dummyPrimaryDNS;
			}
		}

		public static string SecondaryDNS
		{
			get
			{
				return dummySecondaryDNS;
			}
		}

		public static string Gateway
		{
			get
			{
				string gateway = LocalHostIP;
    
				// Replace last component of the local IP with "1".
				// E.g. "192.168.1.10" -> "192.168.1.1"
				int lastDot = gateway.LastIndexOf('.');
				if (lastDot >= 0)
				{
					gateway = gateway.Substring(0, lastDot + 1) + "1";
				}
    
				return gateway;
			}
		}

		public static string SubnetMask
		{
			get
			{
				return dummySubnetMask;
			}
		}

		public static int SubnetMaskInt
		{
			get
			{
				return dummySubnetMaskInt;
			}
		}

		/// <summary>
		/// Returns the best IP address for the local host.
		/// The best IP address is defined as follows:
		/// - in all the IPv4 address of the local host, one IP address not being
		///   a gateway address (not X.X.X.1).
		///   E.g. such gateway addresses are defined for VMware bridges.
		/// - if no such address is found, take the IP address returned by
		///     InetAddress.getLocalHost().getHostAddress()
		/// - if everything fails, take a dummy address "192.168.1.1"
		/// </summary>
		/// <returns> the best IP address for the local host </returns>
		public static string LocalHostIP
		{
			get
			{
				if (string.ReferenceEquals(localHostIP, null))
				{
					localHostIP = "192.168.1.1";
					try
					{
						localHostIP = InetAddress.LocalHost.HostAddress;
						InetAddress localHostAddress = InetAddress.LocalHost;
						InetAddress[] allLocalIPs = InetAddress.getAllByName(localHostAddress.HostName);
						for (int i = 0; allLocalIPs != null && i < allLocalIPs.Length; i++)
						{
							//if (log.DebugEnabled)
							{
								Console.WriteLine(string.Format("IP address of local host: {0}", allLocalIPs[i].HostAddress));
							}
							sbyte[] bytes = allLocalIPs[i].Address;
							if (bytes != null && bytes.Length == 4 && bytes[3] != 1)
							{
								localHostIP = allLocalIPs[i].HostAddress;
							}
						}
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Using IP address of local host: {0}, Subnet Mask {1}", localHostIP, SubnetMask));
						}
					}
					catch (UnknownHostException e)
					{
						Console.WriteLine(e);
					}
				}
    
				return localHostIP;
			}
		}

		public virtual void hleNetApctlConnect(int index)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("hleNetApctlConnect index={0:D}", index));
			}
			connectionIndex = index;

			changeState(PSP_NET_APCTL_STATE_JOINING);
		}

		public virtual int hleNetApctlGetState()
		{
			return state;
		}

		protected internal virtual void triggerNetApctlThread()
		{
			if (sceNetApctlThread != null)
			{
				Modules.ThreadManForUserModule.hleKernelWakeupThread(sceNetApctlThread);
			}
		}

		public virtual void hleNetApctlThread(Processor processor)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("hleNetApctlThread state={0:D}", state));
			}

			if (sceNetApctlThreadTerminate)
			{
				processor.cpu._v0 = 0; // Exit status
				Modules.ThreadManForUserModule.hleKernelExitDeleteThread();
				sceNetApctlThread = null;
			}
			else
			{
				bool stateTransitionCompleted = true;

				// Make a transition to the next state
				switch (state)
				{
					case PSP_NET_APCTL_STATE_JOINING:
						changeState(PSP_NET_APCTL_STATE_GETTING_IP);
						stateTransitionCompleted = false;
						break;
					case PSP_NET_APCTL_STATE_GETTING_IP:
						changeState(PSP_NET_APCTL_STATE_GOT_IP);
						break;
					case PSP_NET_APCTL_STATE_DISCONNECTED:
						if (doScan)
						{
							scanStartMillis = Emulator.Clock.milliTime();
							changeState(PSP_NET_APCTL_STATE_SCANNING);
							doScan = false;
							stateTransitionCompleted = false;
						}
						break;
					case PSP_NET_APCTL_STATE_SCANNING:
						// End of SCAN?
						long now = Emulator.Clock.milliTime();
						if (now - scanStartMillis > SCAN_DURATION_MILLIS)
						{
							// Return to DISCONNECTED state and trigger SCAN event
							changeState(PSP_NET_APCTL_STATE_DISCONNECTED);
						}
						else
						{
							stateTransitionCompleted = false;
						}
					break;
				}

				if (stateTransitionCompleted)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("hleNetApctlThread sleeping with state={0:D}", state));
					}

					// Wait for a new state reset... wakeup is done by triggerNetApctlThread()
					Modules.ThreadManForUserModule.hleKernelSleepThread(false);
				}
				else
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("hleNetApctlThread waiting for {0:D} us with state={1:D}", stateTransitionDelay, state));
					}

					// Wait a little bit before moving to the next state...
					Modules.ThreadManForUserModule.hleKernelDelayThread(stateTransitionDelay, false);
				}
			}
		}

		/// <summary>
		/// Init the apctl.
		/// </summary>
		/// <param name="stackSize"> - The stack size of the internal thread.
		/// </param>
		/// <param name="initPriority"> - The priority of the internal thread.
		/// </param>
		/// <returns> < 0 on error. </returns>
		[HLEFunction(nid : 0xE2F91F9B, version : 150)]
		public virtual int sceNetApctlInit(int stackSize, int initPriority)
		{
			if (sceNetApctlThread == null)
			{
				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				sceNetApctlThread = threadMan.hleKernelCreateThread("SceNetApctl", ThreadManForUser.NET_APCTL_LOOP_ADDRESS, initPriority, stackSize, threadMan.CurrentThread.attr, 0, SysMemUserForUser.USER_PARTITION_ID);
				sceNetApctlThreadTerminate = false;
				threadMan.hleKernelStartThread(sceNetApctlThread, 0, 0, sceNetApctlThread.gpReg_addr);
			}

			return 0;
		}

		/// <summary>
		/// Terminate the apctl.
		/// </summary>
		/// <returns> < 0 on error. </returns>
		[HLEFunction(nid : 0xB3EDD0EC, version : 150)]
		public virtual int sceNetApctlTerm()
		{
			changeState(PSP_NET_APCTL_STATE_DISCONNECTED);

			sceNetApctlThreadTerminate = true;
			triggerNetApctlThread();

			return 0;
		}

		/// <summary>
		/// Get the apctl information.
		/// </summary>
		/// <param name="code"> - One of the PSP_NET_APCTL_INFO_* defines.
		/// </param>
		/// <param name="pInfo"> - Pointer to a ::SceNetApctlInfo.
		/// </param>
		/// <returns> < 0 on error. </returns>
		//		union SceNetApctlInfo
		//		{
		//		        char name[64];                  /* Name of the config used */
		//		        unsigned char bssid[6];         /* MAC address of the access point */
		//		        unsigned char ssid[32];         /* ssid */                     
		//		        unsigned int ssidLength;        /* ssid string Length*/
		//		        unsigned int securityType;      /* 0 for none, 1 for WEP, 2 for WPA) */
		//		        unsigned char strength;         /* Signal strength in % */
		//		        unsigned char channel;          /* Channel */
		//		        unsigned char powerSave;        /* 1 on, 0 off */
		//		        char ip[16];                    /* PSP's ip */
		//		        char subNetMask[16];            /* Subnet mask */
		//		        char gateway[16];               /* Gateway */
		//		        char primaryDns[16];            /* Primary DNS */
		//		        char secondaryDns[16];          /* Secondary DNS */
		//		        unsigned int useProxy;          /* 1 for proxy, 0 for no proxy */
		//		        char proxyUrl[128];             /* Proxy url */
		//		        unsigned short proxyPort;       /* Proxy port */
		//		        unsigned int eapType;           /* 0 is none, 1 is EAP-MD5 */
		//		        unsigned int startBrowser;      /* Should browser be started */
		//		        unsigned int wifisp;            /* 1 if connection is for Wifi service providers (WISP) */
		//		
		//		};
		[HLEFunction(nid : 0x2BEFDF23, version : 150)]
		public virtual int sceNetApctlGetInfo(int code, TPointer pInfo)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceNetApctlGetInfo code=0x{0:X}({1})", code, getApctlInfoName(code)));
			}

			switch (code)
			{
				case PSP_NET_APCTL_INFO_PROFILE_NAME:
				{
					string name = sceUtility.getNetParamName(connectionIndex);
					pInfo.setStringNZ(128, name);
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("sceNetApctlGetInfo returning Profile name '{0}'", name));
					}
					break;
				}
				case PSP_NET_APCTL_INFO_IP:
				{
					string ip = LocalHostIP;
					pInfo.setStringNZ(16, ip);
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("sceNetApctlGetInfo returning IP address '{0}'", ip));
					}
					break;
				}
				case PSP_NET_APCTL_INFO_SSID:
				{
					string ssid = SSID;
					if (string.ReferenceEquals(ssid, null))
					{
						return -1;
					}
					pInfo.setStringNZ(SSID_NAME_LENGTH, ssid);
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("sceNetApctlGetInfo returning SSID '{0}'", ssid));
					}
					break;
				}
				case PSP_NET_APCTL_INFO_SSID_LENGTH:
				{
					string ssid = SSID;
					if (string.ReferenceEquals(ssid, null))
					{
						return -1;
					}
					pInfo.setValue32(System.Math.Min(ssid.Length, SSID_NAME_LENGTH));
					break;
				}
				case PSP_NET_APCTL_INFO_PRIMDNS:
				{
					pInfo.setStringNZ(16, PrimaryDNS);
					break;
				}
				case PSP_NET_APCTL_INFO_SECDNS:
				{
					pInfo.setStringNZ(16, SecondaryDNS);
					break;
				}
				case PSP_NET_APCTL_INFO_GATEWAY:
				{
					pInfo.setStringNZ(16, Gateway);
					break;
				}
				case PSP_NET_APCTL_INFO_SUBNETMASK:
				{
					pInfo.setStringNZ(16, SubnetMask);
					break;
				}
				case PSP_NET_APCTL_INFO_CHANNEL:
				{
					int channel = Settings.Instance.readInt("emu.sysparam.adhocchannel", 0);
					pInfo.Value8 = (sbyte) channel;
					break;
				}
				case PSP_NET_APCTL_INFO_STRENGTH:
				{
					pInfo.Value8 = (sbyte) Wlan.SignalStrenth;
					break;
				}
				case PSP_NET_APCTL_INFO_USE_PROXY:
				{
					pInfo.setValue32(false); // Don't use proxy
					break;
				}
				case PSP_NET_APCTL_INFO_START_BROWSER:
				{
					// Is it needed to start the browser to login/authenticate
					// this connection?
					pInfo.setValue32(false); // Do not start the browser
					break;
				}
				case PSP_NET_APCTL_INFO_UNKNOWN19:
				{
					// The PSP is returning value 1 (tested with JpcspTrace)
					pInfo.setValue32(1);
					break;
				}
				default:
				{
					Console.WriteLine(string.Format("sceNetApctlGetInfo unimplemented code=0x{0:X}({1})", code, getApctlInfoName(code)));
					return -1;
				}
			}

			return 0;
		}

		/// <summary>
		/// Add an apctl event handler.
		/// </summary>
		/// <param name="handler"> - Pointer to the event handler function.
		///                  typedef void (*sceNetApctlHandler)(int oldState, int newState, int event, int error, void *pArg)
		/// </param>
		/// <param name="pArg"> - Value to be passed to the pArg parameter of the handler function.
		/// </param>
		/// <returns> A handler id or < 0 on error. </returns>
		[HLEFunction(nid : 0x8ABADD51, version : 150)]
		public virtual int sceNetApctlAddHandler(TPointer handler, int handlerArg)
		{
			int uid = SceUidManager.getNewUid(uidPurpose);
			ApctlHandler apctlHandler = new ApctlHandler(this, uid, handler.Address, handlerArg);
			apctlHandlers[uid] = apctlHandler;

			return uid;
		}

		/// <summary>
		/// Delete an apctl event handler.
		/// </summary>
		/// <param name="handlerid"> - A handler as created returned from sceNetApctlAddHandler.
		/// </param>
		/// <returns> < 0 on error. </returns>
		[HLEFunction(nid : 0x5963991B, version : 150)]
		public virtual int sceNetApctlDelHandler(int handlerId)
		{
			if (!apctlHandlers.ContainsKey(handlerId))
			{
				Console.WriteLine(string.Format("sceNetApctlDelHandler unknown handlerId=0x{0:X}", handlerId));
				return -1;
			}
			SceUidManager.releaseUid(handlerId, uidPurpose);
			apctlHandlers.Remove(handlerId);

			return 0;
		}

		/// <summary>
		/// Connect to an access point.
		/// </summary>
		/// <param name="connIndex"> - The index of the connection.
		/// </param>
		/// <returns> < 0 on error. </returns>
		[HLEFunction(nid : 0xCFB957C6, version : 150)]
		public virtual int sceNetApctlConnect(int connIndex)
		{
			hleNetApctlConnect(connIndex);

			return 0;
		}

		/// <summary>
		/// Disconnect from an access point.
		/// </summary>
		/// <returns> < 0 on error. </returns>
		[HLEFunction(nid : 0x24FE91A1, version : 150)]
		public virtual int sceNetApctlDisconnect()
		{
			changeState(PSP_NET_APCTL_STATE_DISCONNECTED);

			return 0;
		}

		/// <summary>
		/// Get the state of the access point connection.
		/// </summary>
		/// <param name="pState"> - Pointer to receive the current state (one of the PSP_NET_APCTL_STATE_* defines).
		/// </param>
		/// <returns> < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x5DEAC81B, version = 150) public int sceNetApctlGetState(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 stateAddr)
		[HLEFunction(nid : 0x5DEAC81B, version : 150)]
		public virtual int sceNetApctlGetState(TPointer32 stateAddr)
		{
			stateAddr.setValue(state);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2935C45B, version = 150) public int sceNetApctlGetBSSDescEntry2()
		[HLEFunction(nid : 0x2935C45B, version : 150)]
		public virtual int sceNetApctlGetBSSDescEntry2()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA3E77E13, version = 150) public int sceNetApctlScanSSID2()
		[HLEFunction(nid : 0xA3E77E13, version : 150)]
		public virtual int sceNetApctlScanSSID2()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF25A5006, version = 150) public int sceNetApctlGetBSSDescIDList2()
		[HLEFunction(nid : 0xF25A5006, version : 150)]
		public virtual int sceNetApctlGetBSSDescIDList2()
		{
			return 0;
		}

		[HLEFunction(nid : 0xE9B2E5E6, version : 150)]
		public virtual int sceNetApctlScanUser()
		{
			doScan = true;
			triggerNetApctlThread();

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x6BDDCB8C, version = 150) public int sceNetApctlGetBSSDescIDListUser(pspsharp.HLE.TPointer32 sizeAddr, @CanBeNull pspsharp.HLE.TPointer buf)
		[HLEFunction(nid : 0x6BDDCB8C, version : 150)]
		public virtual int sceNetApctlGetBSSDescIDListUser(TPointer32 sizeAddr, TPointer buf)
		{
			const int userInfoSize = 8;
			int entries = 1;
			int size = sizeAddr.getValue();
			// Return size required
			sizeAddr.setValue(entries * userInfoSize);

			if (buf.NotNull)
			{
				int offset = 0;
				for (int i = 0; i < entries; i++)
				{
					// Check if enough space available to write the next structure
					if (offset + userInfoSize > size)
					{
						break;
					}

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("sceNetApctlGetBSSDescIDListUser returning {0:D} at 0x{1:X8}", i, buf.Address + offset));
					}

					/// <summary>
					/// Pointer to next Network structure in list: will be written later </summary>
					offset += 4;

					/// <summary>
					/// Entry ID </summary>
					buf.setValue32(offset, i);
					offset += 4;
				}

				fillNextPointersInLinkedList(buf, offset, userInfoSize);
			}

			return 0;
		}

		[HLEFunction(nid : 0x04776994, version : 150)]
		public virtual int sceNetApctlGetBSSDescEntryUser(int entryId, int infoId, TPointer result)
		{
			switch (infoId)
			{
				case PSP_NET_APCTL_DESC_IBSS: // IBSS, 6 bytes
					string ibss = Modules.sceNetAdhocctlModule.hleNetAdhocctlGetIBSS();
					result.setStringNZ(IBSS_NAME_LENGTH, ibss);
					break;
				case PSP_NET_APCTL_DESC_SSID_NAME:
					// Return 32 bytes
					string ssid = SSID;
					result.setStringNZ(SSID_NAME_LENGTH, ssid);
					break;
				case PSP_NET_APCTL_DESC_SSID_NAME_LENGTH:
					// Return one 32-bit value
					int Length = System.Math.Min(SSID.Length, SSID_NAME_LENGTH);
					result.setValue32(Length);
					break;
				case PSP_NET_APCTL_DESC_SIGNAL_STRENGTH:
					// Return 1 byte
					result.Value8 = (sbyte) Wlan.SignalStrenth;
					break;
				case PSP_NET_APCTL_DESC_SECURITY:
					// Return one 32-bit value
					result.setValue32(PSP_NET_APCTL_INFO_SECURITY_TYPE_WPA_AES);
					break;
				default:
					Console.WriteLine(string.Format("sceNetApctlGetBSSDescEntryUser unknown id {0:D}", infoId));
					return -1;
			}

			return 0;
		}

		[HLEFunction(nid : 0x7CFAB990, version : 150)]
		public virtual int sceNetApctlAddInternalHandler(TPointer handler, int handlerArg)
		{
			// This seems to be a 2nd kind of handler
			return sceNetApctlAddHandler(handler, handlerArg);
		}

		[HLEFunction(nid : 0xE11BAFAB, version : 150)]
		public virtual int sceNetApctlDelInternalHandler(int handlerId)
		{
			// This seems to be a 2nd kind of handler
			return sceNetApctlDelHandler(handlerId);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA7BB73DF, version = 150) public int sceNetApctl_A7BB73DF(pspsharp.HLE.TPointer handler, int handlerArg)
		[HLEFunction(nid : 0xA7BB73DF, version : 150)]
		public virtual int sceNetApctl_A7BB73DF(TPointer handler, int handlerArg)
		{
			// This seems to be a 3rd kind of handler
			return sceNetApctlAddHandler(handler, handlerArg);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6F5D2981, version = 150) public int sceNetApctl_6F5D2981(int handlerId)
		[HLEFunction(nid : 0x6F5D2981, version : 150)]
		public virtual int sceNetApctl_6F5D2981(int handlerId)
		{
			// This seems to be a 3rd kind of handler
			return sceNetApctlDelHandler(handlerId);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x69745F0A, version = 150) public int sceNetApctl_lib2_69745F0A(int handlerId)
		[HLEFunction(nid : 0x69745F0A, version : 150)]
		public virtual int sceNetApctl_lib2_69745F0A(int handlerId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4C19731F, version = 150) public int sceNetApctl_lib2_4C19731F(int code, pspsharp.HLE.TPointer pInfo)
		[HLEFunction(nid : 0x4C19731F, version : 150)]
		public virtual int sceNetApctl_lib2_4C19731F(int code, TPointer pInfo)
		{
			return sceNetApctlGetInfo(code, pInfo);
		}

		[HLEFunction(nid : 0xB3CF6849, version : 150)]
		public virtual int sceNetApctlScan()
		{
			return sceNetApctlScanUser();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0C7FFA5C, version = 150) public int sceNetApctlGetBSSDescIDList(pspsharp.HLE.TPointer32 sizeAddr, @CanBeNull pspsharp.HLE.TPointer buf)
		[HLEFunction(nid : 0x0C7FFA5C, version : 150)]
		public virtual int sceNetApctlGetBSSDescIDList(TPointer32 sizeAddr, TPointer buf)
		{
			return sceNetApctlGetBSSDescIDListUser(sizeAddr, buf);
		}

		[HLEFunction(nid : 0x96BEB231, version : 150)]
		public virtual int sceNetApctlGetBSSDescEntry(int entryId, int infoId, TPointer result)
		{
			return sceNetApctlGetBSSDescEntryUser(entryId, infoId, result);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC20A144C, version = 150) public int sceNetApctl_lib2_C20A144C(int connIndex, pspsharp.HLE.kernel.types.pspNetMacAddress ps3MacAddress)
		[HLEFunction(nid : 0xC20A144C, version : 150)]
		public virtual int sceNetApctl_lib2_C20A144C(int connIndex, pspNetMacAddress ps3MacAddress)
		{
			return sceNetApctlConnect(connIndex);
		}
	}
}