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
//	import static pspsharp.HLE.Modules.sceNetIfhandleModule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.SceUidManager.INVALID_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceNetWlanMessage.WLAN_PROTOCOL_SUBTYPE_CONTROL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceNetWlanMessage.WLAN_PROTOCOL_SUBTYPE_DATA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceNetWlanMessage.WLAN_PROTOCOL_TYPE_SONY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.KERNEL_PARTITION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.hardware.Wlan.MAC_ADDRESS_LENGTH;


	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SystemTimeManager = pspsharp.HLE.kernel.managers.SystemTimeManager;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceNetIfHandle = pspsharp.HLE.kernel.types.SceNetIfHandle;
	using SceNetIfMessage = pspsharp.HLE.kernel.types.SceNetIfMessage;
	using SceNetWlanMessage = pspsharp.HLE.kernel.types.SceNetWlanMessage;
	using SceNetWlanScanInfo = pspsharp.HLE.kernel.types.SceNetWlanScanInfo;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using Wlan = pspsharp.hardware.Wlan;
	using AccessPoint = pspsharp.network.accesspoint.AccessPoint;
	using Scheduler = pspsharp.scheduler.Scheduler;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class sceWlan : HLEModule
	{
		public static Logger log = Modules.getLogger("sceWlan");
		public const int IOCTL_CMD_UNKNOWN_0x2 = 0x2;
		public const int IOCTL_CMD_START_SCANNING = 0x34;
		public const int IOCTL_CMD_CREATE = 0x35;
		public const int IOCTL_CMD_CONNECT = 0x36;
		public const int IOCTL_CMD_GET_INFO = 0x37;
		public const int IOCTL_CMD_DISCONNECT = 0x38;
		public const int IOCTL_CMD_UNKNOWN_0x42 = 0x42;
		public const int IOCTL_CMD_ENTER_GAME_MODE = 0x44;
		public const int IOCTL_CMD_SET_WEP_KEY = 0x47;
		public const int WLAN_MODE_INFRASTRUCTURE = 1;
		public const int WLAN_MODE_ADHOC = 2;
		private static int wlanSocketPort = 30010;
		private const int wlanThreadPollingDelayUs = 12000; // 12ms
		private const int wlanScanActionDelayUs = 50000; // 50ms
		private const int wlanConnectActionDelayUs = 50000; // 50ms
		private const int wlanCreateActionDelayUs = 50000; // 50ms
		private const int wlanDisconnectActionDelayUs = 50000; // 50ms
		public static readonly sbyte WLAN_CMD_DATA = (sbyte) 0;
		public static readonly sbyte WLAN_CMD_SCAN_REQUEST = (sbyte) 1;
		public static readonly sbyte WLAN_CMD_SCAN_RESPONSE = (sbyte) 2;
		private static readonly sbyte[] dummyOtherMacAddress = new sbyte[] {0x10, 0x22, 0x33, 0x44, 0x55, 0x66};
		private static readonly int[] channels = new int[] {1, 6, 11};
		private int joinedChannel;
		private int dummyMessageStep;
		private TPointer dummyMessageHandleAddr;
		private DatagramSocket wlanSocket;
		private TPointer wlanHandleAddr;
		private int wlanThreadUid;
		private int unknownValue1;
		private int unknownValue2;
		private int unknownValue3;
		private bool isGameMode;
		private IList<pspNetMacAddress> activeMacAddresses;
		private IList<GameModeState> gameModeStates;
		private int gameModeDataLength;
		private string[] channelSSIDs;
		private int[] channelModes;
		private int wlanDropRate;
		private int wlanDropDuration;

		private class GameModeState
		{
			public long timeStamp;
			public bool updated;
			public pspNetMacAddress macAddress;
			public sbyte[] data;
			public int dataLength;

			public GameModeState(pspNetMacAddress macAddress)
			{
				this.macAddress = macAddress;
				dataLength = Modules.sceWlanModule.gameModeDataLength;
				data = new sbyte[dataLength];
			}

			public virtual void doUpdate()
			{
				updated = true;
				timeStamp = SystemTimeManager.SystemTime;
			}

			public override string ToString()
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("macAddress=%s, updated=%b, timeStamp=%d, dataLength=0x%X, data=%s", macAddress, updated, timeStamp, dataLength, pspsharp.util.Utilities.getMemoryDump(data, 0, dataLength));
				return string.Format("macAddress=%s, updated=%b, timeStamp=%d, dataLength=0x%X, data=%s", macAddress, updated, timeStamp, dataLength, Utilities.getMemoryDump(data, 0, dataLength));
			}
		}

		private class WlanScanAction : IAction
		{
			private readonly sceWlan outerInstance;

			internal TPointer handleAddr;
			internal TPointer inputAddr;
			internal TPointer outputAddr;
			internal int callCount;

			public WlanScanAction(sceWlan outerInstance, TPointer handleAddr, int inputAddr, int outputAddr)
			{
				this.outerInstance = outerInstance;
				this.handleAddr = handleAddr;
				this.inputAddr = new TPointer(handleAddr.Memory, inputAddr);
				this.outputAddr = new TPointer(handleAddr.Memory, outputAddr);
			}

			public virtual void execute()
			{
				outerInstance.hleWlanScanAction(handleAddr, inputAddr, outputAddr, this, callCount);
				callCount++;
			}
		}

		private class WlanConnectAction : IAction
		{
			private readonly sceWlan outerInstance;

			internal TPointer handleAddr;

			public WlanConnectAction(sceWlan outerInstance, TPointer handleAddr)
			{
				this.outerInstance = outerInstance;
				this.handleAddr = handleAddr;
			}

			public virtual void execute()
			{
				outerInstance.hleWlanConnectAction(handleAddr);
			}
		}

		private class WlanCreateAction : IAction
		{
			private readonly sceWlan outerInstance;

			internal TPointer handleAddr;

			public WlanCreateAction(sceWlan outerInstance, TPointer handleAddr)
			{
				this.outerInstance = outerInstance;
				this.handleAddr = handleAddr;
			}

			public virtual void execute()
			{
				outerInstance.hleWlanCreateAction(handleAddr);
			}
		}

		private class WlanDisconnectAction : IAction
		{
			private readonly sceWlan outerInstance;

			internal TPointer handleAddr;

			public WlanDisconnectAction(sceWlan outerInstance, TPointer handleAddr)
			{
				this.outerInstance = outerInstance;
				this.handleAddr = handleAddr;
			}

			public virtual void execute()
			{
				outerInstance.hleWlanDisconnectAction(handleAddr);
			}
		}

		public override void start()
		{
			wlanThreadUid = INVALID_ID;
			dummyMessageStep = -1;
			activeMacAddresses = new LinkedList<pspNetMacAddress>();
			gameModeStates = new LinkedList<GameModeState>();
			gameModeDataLength = 256;
			int maxChannel = -1;
			for (int i = 0; i < channels.Length; i++)
			{
				maxChannel = System.Math.Max(maxChannel, channels[i]);
			}
			channelSSIDs = new string[maxChannel + 1];
			channelModes = new int[maxChannel + 1];
			joinedChannel = -1;

			base.start();
		}

		public virtual void hleWlanThread()
		{
			if (log.TraceEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.trace(String.format("hleWlanThread isGameMode=%b", isGameMode));
				log.trace(string.Format("hleWlanThread isGameMode=%b", isGameMode));
			}

			if (wlanThreadMustExit())
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Exiting hleWlanThread {0}", Modules.ThreadManForUserModule.CurrentThread));
				}
				Modules.ThreadManForUserModule.hleKernelExitDeleteThread(0);
				return;
			}

			if (isGameMode)
			{
				hleWlanSendGameMode();
			}

			while (!wlanThreadMustExit() && hleWlanReceive())
			{
				// Receive all available messages
			}

			if (dummyMessageStep > 0)
			{
				sendDummyMessage(dummyMessageStep, dummyMessageHandleAddr);
				dummyMessageStep = 0;
			}

			Modules.ThreadManForUserModule.hleKernelDelayThread(wlanThreadPollingDelayUs, true);
		}

		private bool wlanThreadMustExit()
		{
			return wlanThreadUid != Modules.ThreadManForUserModule.CurrentThreadID;
		}

		private void hleWlanScanAction(TPointer handleAddr, TPointer inputAddr, TPointer outputAddr, WlanScanAction action, int callCount)
		{
			// Send a scan request packet
			sbyte[] scanRequestPacket = new sbyte[1 + MAC_ADDRESS_LENGTH];
			scanRequestPacket[0] = WLAN_CMD_SCAN_REQUEST;
			Array.Copy(Wlan.MacAddress, 0, scanRequestPacket, 1, MAC_ADDRESS_LENGTH);
			sendPacket(scanRequestPacket, scanRequestPacket.Length);

			while (hleWlanReceive())
			{
				// Process all pending messages
			}

			if (callCount < 20)
			{
				// Schedule this action for 20 times (1 second)
				// before terminating the scan action
				Emulator.Scheduler.addAction(Scheduler.Now + wlanScanActionDelayUs, action);
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("End of scan action:"));
					foreach (int ch in channels)
					{
						log.debug(string.Format("Scan result channel#{0:D}, ssid='{1}', mode={2:D}", ch, channelSSIDs[ch], channelModes[ch]));
					}
				}

				TPointer addr = new TPointer(outputAddr);
				for (int i = 0; i < 14; i++)
				{
					int channel = inputAddr.getValue8(10 + i);
					if (channel == 0)
					{
						break;
					}
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Scan on channel {0:D}", channel));
					}

					if (isValidChannel(channel))
					{
						string ssid = channelSSIDs[channel];
						if (!string.ReferenceEquals(ssid, null) && ssid.Length > 0)
						{
							SceNetWlanScanInfo scanInfo = new SceNetWlanScanInfo();
							scanInfo.bssid = "pspsharp";
							scanInfo.channel = channel;
							scanInfo.ssid = ssid;
							scanInfo.mode = channelModes[channel];
							scanInfo.unknown44 = 1000; // Unknown value, need to be != 0
							scanInfo.write(addr.Memory, addr.Address + 4);

							addr.setValue32(0, addr.Address + 4 + scanInfo.@sizeof()); // Link to next SSID
							addr.add(4 + scanInfo.@sizeof());
						}
					}
				}

				if (addr.Address > outputAddr.Address)
				{
					addr.setValue32(-96, 0); // Last SSID, no next one
				}

				// Signal the sema when the scan has completed
				SceNetIfHandle handle = new SceNetIfHandle();
				handle.read(handleAddr);
				Modules.ThreadManForUserModule.sceKernelSignalSema(handle.handleInternal.ioctlSemaId, 1);
			}
		}

		private void hleWlanConnectAction(TPointer handleAddr)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleWlanConnectAction handleAddr={0}", handleAddr));
			}

			// Signal the sema that the connect/join has completed
			SceNetIfHandle handle = new SceNetIfHandle();
			handle.read(handleAddr);
			Modules.ThreadManForUserModule.sceKernelSignalSema(handle.handleInternal.ioctlSemaId, 1);
		}

		private void hleWlanCreateAction(TPointer handleAddr)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleWlanCreateAction handleAddr={0}", handleAddr));
			}

			// Signal the sema that the create has completed
			SceNetIfHandle handle = new SceNetIfHandle();
			handle.read(handleAddr);
			Modules.ThreadManForUserModule.sceKernelSignalSema(handle.handleInternal.ioctlSemaId, 1);
		}

		private void hleWlanDisconnectAction(TPointer handleAddr)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleWlanDisconnectAction handleAddr={0}", handleAddr));
			}

			// Signal the sema that the disconnect has completed
			SceNetIfHandle handle = new SceNetIfHandle();
			handle.read(handleAddr);
			Modules.ThreadManForUserModule.sceKernelSignalSema(handle.handleInternal.ioctlSemaId, 1);
		}

		private bool hleWlanReceive()
		{
			if (isGameMode)
			{
				return hleWlanReceiveGameMode();
			}

			return hleWlanReceiveMessage();
		}

		private bool createWlanSocket()
		{
			if (wlanSocket == null)
			{
				bool retry;
				do
				{
					retry = false;
					try
					{
						wlanSocket = new DatagramSocket(wlanSocketPort);
						// For broadcast
						wlanSocket.Broadcast = true;
						// Non-blocking (timeout = 0 would mean blocking)
						wlanSocket.SoTimeout = 1;
					}
					catch (BindException e)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("createWlanSocket port {0:D} already in use ({1}) - retrying with port {2:D}", wlanSocketPort, e, wlanSocketPort + 1));
						}
						// The port is already busy, retrying with another port
						wlanSocketPort++;
						retry = true;
					}
					catch (SocketException e)
					{
						log.error("createWlanSocket", e);
					}
				} while (retry);
			}

			return wlanSocket != null;
		}

		public static int SocketPort
		{
			get
			{
				return wlanSocketPort;
			}
		}

		private int getBroadcastPort(int channel)
		{
			if (channel >= 0 && channelModes[channel] == WLAN_MODE_INFRASTRUCTURE)
			{
				return AccessPoint.Instance.Port;
			}

			return wlanSocketPort ^ 1;
		}

		protected internal virtual void sendPacket(sbyte[] buffer, int bufferLength)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sendPacket {0}", Utilities.getMemoryDump(buffer, 0, bufferLength)));
			}

			try
			{
				InetSocketAddress[] broadcastAddress = sceNetInet.getBroadcastInetSocketAddress(getBroadcastPort(joinedChannel));
				if (broadcastAddress != null)
				{
					for (int i = 0; i < broadcastAddress.Length; i++)
					{
						DatagramPacket packet = new DatagramPacket(buffer, bufferLength, broadcastAddress[i]);
						wlanSocket.send(packet);
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

		protected internal virtual void sendDataPacket(sbyte[] buffer, int bufferLength)
		{
			sbyte[] packetBuffer = new sbyte[bufferLength + 1 + 32];
			int offset = 0;
			// Add the cmd in front of the data
			packetBuffer[offset] = WLAN_CMD_DATA;
			offset++;
			// Add the joined SSID in front of the data
			if (joinedChannel >= 0)
			{
				Utilities.writeStringNZ(packetBuffer, offset, 32, channelSSIDs[joinedChannel]);
			}
			offset += 32;
			// Add the data
			Array.Copy(buffer, 0, packetBuffer, offset, bufferLength);
			offset += bufferLength;

			sendPacket(packetBuffer, offset);
		}

		private GameModeState getGameModeStat(sbyte[] macAddress)
		{
			GameModeState myGameModeState = null;
			foreach (GameModeState gameModeState in gameModeStates)
			{
				if (gameModeState.macAddress.Equals(macAddress))
				{
					myGameModeState = gameModeState;
					break;
				}
			}

			return myGameModeState;
		}

		private GameModeState MyGameModeState
		{
			get
			{
				return getGameModeStat(Wlan.MacAddress);
			}
		}

		private void addActiveMacAddress(pspNetMacAddress macAddress)
		{
			if (!sceNetAdhoc.isAnyMacAddress(macAddress.macAddress))
			{
				if (!activeMacAddresses.Contains(macAddress))
				{
					activeMacAddresses.Add(macAddress);
					gameModeStates.Add(new GameModeState(macAddress));
				}
			}
		}

		private bool isValidChannel(int channel)
		{
			for (int i = 0; i < channels.Length; i++)
			{
				if (channels[i] == channel)
				{
					return true;
				}
			}

			return false;
		}

		private void setChannelSSID(int channel, string ssid, int mode)
		{
			if (!string.ReferenceEquals(ssid, null) && ssid.Length > 0 && isValidChannel(channel))
			{
				channelSSIDs[channel] = ssid;
				channelModes[channel] = mode;
			}
		}

		private void joinChannelSSID(int channel, string ssid, int mode)
		{
			setChannelSSID(channel, ssid, mode);
			joinedChannel = channel;
		}

		private void processCmd(sbyte cmd, sbyte[] buffer, int offset, int length)
		{
			sbyte[] packetMacAddress = new sbyte[MAC_ADDRESS_LENGTH];
			Array.Copy(buffer, offset, packetMacAddress, 0, MAC_ADDRESS_LENGTH);
			offset += MAC_ADDRESS_LENGTH;
			length -= MAC_ADDRESS_LENGTH;
			sbyte[] myMacAddress = Wlan.MacAddress;
			bool macAddressEqual = true;
			for (int i = 0; i < MAC_ADDRESS_LENGTH; i++)
			{
				if (packetMacAddress[i] != myMacAddress[i])
				{
					macAddressEqual = false;
					break;
				}
			}
			if (macAddressEqual)
			{
				// This packet is coming from myself, ignore it
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Ignoring packet coming from myself"));
				}
				return;
			}

			if (cmd == WLAN_CMD_SCAN_REQUEST)
			{
				sbyte[] scanResponse = new sbyte[1 + MAC_ADDRESS_LENGTH + (32 + 2) * channels.Length];
				int responseOffset = 0;

				scanResponse[responseOffset] = WLAN_CMD_SCAN_RESPONSE;
				responseOffset++;

				Array.Copy(Wlan.MacAddress, 0, scanResponse, responseOffset, MAC_ADDRESS_LENGTH);
				responseOffset += MAC_ADDRESS_LENGTH;

				foreach (int channel in channels)
				{
					scanResponse[responseOffset] = (sbyte) channel;
					responseOffset++;

					scanResponse[responseOffset] = (sbyte) channelModes[channel];
					responseOffset++;

					Utilities.writeStringNZ(scanResponse, responseOffset, 32, channelSSIDs[channel]);
					responseOffset += 32;
				}
				sendPacket(scanResponse, responseOffset);
			}
			else if (cmd == WLAN_CMD_SCAN_RESPONSE)
			{
				while (length >= 34)
				{
					int channel = buffer[offset];
					offset++;
					length--;

					int mode = buffer[offset];
					offset++;
					length--;

					string ssid = Utilities.readStringNZ(buffer, offset, 32);
					if (!string.ReferenceEquals(ssid, null) && ssid.Length > 0)
					{
						// Do not overwrite the information for our joined channel
						if (channel != joinedChannel)
						{
							setChannelSSID(channel, ssid, mode);
						}
					}
					offset += 32;
					length -= 32;
				}
			}
			else
			{
				if (log.InfoEnabled)
				{
					log.info(string.Format("processCmd unknown cmd=0x{0:X}, buffer={1}", cmd, Utilities.getMemoryDump(buffer, offset, length)));
				}
			}
		}

		private bool hleWlanReceiveMessage()
		{
			bool packetReceived = false;

			if (!createWlanSocket())
			{
				return packetReceived;
			}

			sbyte[] bytes = new sbyte[10000];
			DatagramPacket packet = new DatagramPacket(bytes, bytes.Length);
			try
			{
				wlanSocket.receive(packet);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleWlanReceiveMessage message: {0}", Utilities.getMemoryDump(packet.Data, packet.Offset, packet.Length)));
				}

				packetReceived = true;

				sbyte[] dataBytes = packet.Data;
				int dataOffset = packet.Offset;
				int dataLength = packet.Length;

				sbyte cmd = dataBytes[dataOffset];
				dataOffset++;
				dataLength--;
				if (cmd != WLAN_CMD_DATA)
				{
					processCmd(cmd, dataBytes, dataOffset, dataLength);
					return packetReceived;
				}

				string ssid = Utilities.readStringNZ(dataBytes, dataOffset, 32);
				dataOffset += 32;
				dataLength -= 32;

				if (joinedChannel >= 0 && !ssid.Equals(channelSSIDs[joinedChannel]))
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("hleWlanReceiveMessage message SSID('{0}') not matching the joined SSID('{1}')", ssid, channelSSIDs[joinedChannel]));
					}
					return packetReceived;
				}

				SceNetIfMessage message = new SceNetIfMessage();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int size = message.sizeof() + dataLength;
				int size = message.@sizeof() + dataLength;
				int allocatedAddr = Modules.sceNetIfhandleModule.hleNetMallocInternal(size);
				if (allocatedAddr > 0)
				{
					Memory mem = Memory.Instance;
					mem.memset(allocatedAddr, (sbyte) 0, size);
					RuntimeContext.debugMemory(allocatedAddr, size);

					TPointer messageAddr = new TPointer(mem, allocatedAddr);
					TPointer data = new TPointer(mem, messageAddr.Address + message.@sizeof());

					// Write the received bytes to memory
					Utilities.writeBytes(data.Address, dataLength, dataBytes, dataOffset);

					// Write the message header
					message.dataAddr = data.Address;
					message.dataLength = dataLength;
					message.unknown16 = 1;
					message.unknown18 = 2;
					message.unknown24 = dataLength;
					message.write(messageAddr);

					SceNetWlanMessage wlanMessage = new SceNetWlanMessage();
					wlanMessage.read(data);
					addActiveMacAddress(wlanMessage.srcMacAddress);
					addActiveMacAddress(wlanMessage.dstMacAddress);

					if (dataLength > 0)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Notifying received message: {0}", message));
							log.debug(string.Format("Message WLAN: {0}", wlanMessage));
							log.debug(string.Format("Message data: {0}", Utilities.getMemoryDump(data.Address, dataLength)));
						}

						int sceNetIfEnqueue = NIDMapper.Instance.getAddressByName("sceNetIfEnqueue");
						if (sceNetIfEnqueue != 0)
						{
							SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
							Modules.ThreadManForUserModule.executeCallback(thread, sceNetIfEnqueue, null, true, wlanHandleAddr.Address, messageAddr.Address);
						}
					}
				}
			}
			catch (SocketTimeoutException)
			{
				// Timeout can be ignored as we are polling
			}
			catch (IOException e)
			{
				log.error("hleWlanReceiveMessage", e);
			}

			return packetReceived;
		}

		private void hleWlanSendGameMode()
		{
			GameModeState myGameModeState = MyGameModeState;
			if (myGameModeState == null)
			{
				return;
			}

			sbyte[] buffer = new sbyte[myGameModeState.dataLength + myGameModeState.macAddress.@sizeof()];
			int offset = 0;

			Array.Copy(myGameModeState.macAddress.macAddress, 0, buffer, offset, myGameModeState.macAddress.@sizeof());
			offset += myGameModeState.macAddress.@sizeof();

			Array.Copy(myGameModeState.data, 0, buffer, offset, myGameModeState.dataLength);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleWlanSendGameMode sending packet: {0}", Utilities.getMemoryDump(buffer, 0, buffer.Length)));
			}

			sendDataPacket(buffer, buffer.Length);

			myGameModeState.updated = false;
		}

		private bool hleWlanReceiveGameMode()
		{
			bool packetReceived = false;

			if (!createWlanSocket())
			{
				return packetReceived;
			}

			pspNetMacAddress macAddress = new pspNetMacAddress();

			sbyte[] bytes = new sbyte[gameModeDataLength + macAddress.@sizeof() + 1 + 8];
			DatagramPacket packet = new DatagramPacket(bytes, bytes.Length);
			try
			{
				wlanSocket.receive(packet);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleWlanReceiveGameMode message: {0}", Utilities.getMemoryDump(packet.Data, packet.Offset, packet.Length)));
				}

				packetReceived = true;

				sbyte[] dataBytes = packet.Data;
				int dataOffset = packet.Offset;
				int dataLength = packet.Length;

				sbyte cmd = dataBytes[dataOffset];
				dataOffset++;
				dataLength--;
				if (cmd != WLAN_CMD_DATA)
				{
					processCmd(cmd, dataBytes, dataOffset, dataLength);
					return packetReceived;
				}

				string ssid = Utilities.readStringNZ(dataBytes, dataOffset, 32);
				dataOffset += 32;
				dataLength -= 32;

				if (joinedChannel >= 0 && !ssid.Equals(channelSSIDs[joinedChannel]))
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("hleWlanReceiveGameMode message SSID('{0}') not matching the joined SSID('{1}')", ssid, channelSSIDs[joinedChannel]));
					}
					return packetReceived;
				}

				macAddress.setMacAddress(dataBytes, dataOffset);
				dataOffset += macAddress.@sizeof();
				dataLength -= macAddress.@sizeof();

				GameModeState gameModeState = getGameModeStat(macAddress.macAddress);
				if (gameModeState != null)
				{
					int length = System.Math.Min(dataLength, gameModeState.dataLength);
					Array.Copy(dataBytes, dataOffset, gameModeState.data, 0, length);

					gameModeState.doUpdate();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("hleWlanReceiveGameMode updated GameModeState {0}", gameModeState));
					}
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("hleWlanReceiveGameMode could not find GameModeState for MAC address {0}", macAddress));
					}
				}
			}
			catch (SocketTimeoutException)
			{
				// Timeout can be ignored as we are polling
			}
			catch (IOException e)
			{
				log.error("hleWlanReceiveMessage", e);
			}

			return packetReceived;
		}

		protected internal virtual void hleWlanSendMessage(TPointer handleAddr, SceNetIfMessage message)
		{
			Memory mem = handleAddr.Memory;
			SceNetWlanMessage wlanMessage = new SceNetWlanMessage();
			wlanMessage.read(mem, message.dataAddr);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleWlanSendMessage message: {0}: {1}", message, Utilities.getMemoryDump(message.BaseAddress, message.@sizeof())));
				log.debug(string.Format("hleWlanSendMessage WLAN message : {0}", wlanMessage));
				log.debug(string.Format("hleWlanSendMessage message data: {0}", Utilities.getMemoryDump(message.dataAddr + wlanMessage.@sizeof(), message.dataLength - wlanMessage.@sizeof())));
			}

			if (!createWlanSocket())
			{
				return;
			}

			sbyte[] messageBytes = null;
			while (true)
			{
				if (message.dataLength > 0)
				{
					int messageBytesOffset = messageBytes == null ? 0 : messageBytes.Length;
					messageBytes = Utilities.extendArray(messageBytes, message.dataLength);
					Utilities.readBytes(message.dataAddr, message.dataLength, messageBytes, messageBytesOffset);
				}

				if (message.nextDataAddr == 0)
				{
					break;
				}
				message.read(mem, message.nextDataAddr);
			}

			if (messageBytes != null)
			{
				sendDataPacket(messageBytes, messageBytes.Length);
			}

			if (false)
			{
				sendDummyMessage(handleAddr, message, wlanMessage);
			}
		}

		public virtual int hleWlanSendCallback(TPointer handleAddr)
		{
			SceNetIfHandle handle = new SceNetIfHandle();
			handle.read(handleAddr);

			Memory mem = handleAddr.Memory;
			TPointer firstMessageAddr = new TPointer(mem, handle.addrFirstMessageToBeSent);
			SceNetIfMessage message = new SceNetIfMessage();
			message.read(firstMessageAddr);
			RuntimeContext.debugMemory(firstMessageAddr.Address, message.@sizeof());
			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleWlanSendCallback handleAddr={0}: {1}", handleAddr, handle));
			}

			hleWlanSendMessage(handleAddr, message);

			// Unlink the message from the handle
			handle.addrFirstMessageToBeSent = message.nextMessageAddr;
			handle.numberOfMessagesToBeSent--;
			if (handle.addrFirstMessageToBeSent == 0)
			{
				handle.addrLastMessageToBeSent = 0;
			}
			handle.write(handleAddr);

			// Call sceNetMFreem to free the received message
			int sceNetMFreem = NIDMapper.Instance.getAddressByName("sceNetMFreem");
			if (sceNetMFreem != 0)
			{
				Modules.ThreadManForUserModule.executeCallback(null, sceNetMFreem, null, true, firstMessageAddr.Address);
			}
			else
			{
				Modules.sceNetIfhandleModule.sceNetMFreem(firstMessageAddr);
			}

			return 0;
		}

		// Called by sceNetIfhandleIfUp
		public virtual int hleWlanUpCallback(TPointer handleAddr)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleWlanUpCallback handleAddr: {0}", Utilities.getMemoryDump(handleAddr.Address, 44)));
				int handleInternalAddr = handleAddr.getValue32();
				if (handleInternalAddr != 0)
				{
					log.debug(string.Format("hleWlanUpCallback handleInternalAddr: {0}", Utilities.getMemoryDump(handleInternalAddr, 320)));
				}
			}

			SceNetIfHandle handle = new SceNetIfHandle();
			handle.read(handleAddr);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleWlanUpCallback handleAddr={0}: {1}", handleAddr, handle));
			}

			wlanHandleAddr = handleAddr;

			// Add my own MAC address to the active list
			addActiveMacAddress(new pspNetMacAddress(Wlan.MacAddress));

			// This thread will call hleWlanThread() in a loop
			SceKernelThreadInfo thread = Modules.ThreadManForUserModule.hleKernelCreateThread("SceWlanHal", ThreadManForUser.WLAN_LOOP_ADDRESS, 39, 2048, 0, 0, KERNEL_PARTITION_ID);
			if (thread != null)
			{
				wlanThreadUid = thread.uid;
				Modules.ThreadManForUserModule.hleKernelStartThread(thread, 0, 0, 0);
			}

			Modules.ThreadManForUserModule.sceKernelSignalSema(handle.handleInternal.ioctlSemaId, 1);

			return 0;
		}

		// Called by sceNetIfhandleIfDown
		public virtual int hleWlanDownCallback(TPointer handleAddr)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleWlanDownCallback handleAddr: {0}", Utilities.getMemoryDump(handleAddr.Address, 44)));
				int handleInternalAddr = handleAddr.getValue32();
				if (handleInternalAddr != 0)
				{
					log.debug(string.Format("hleWlanDownCallback handleInternalAddr: {0}", Utilities.getMemoryDump(handleInternalAddr, 320)));
				}
			}

			SceNetIfHandle handle = new SceNetIfHandle();
			handle.read(handleAddr);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleWlanDownCallback handleAddr={0}: {1}", handleAddr, handle));
			}

			// This will force the current wlan thread to exit
			wlanThreadUid = INVALID_ID;

			Modules.ThreadManForUserModule.sceKernelSignalSema(handle.handleInternal.ioctlSemaId, 1);

			return 0;
		}

		public virtual int hleWlanIoctlCallback(TPointer handleAddr, int cmd, TPointer unknown, TPointer32 buffersAddr)
		{
			SceNetIfHandle handle = new SceNetIfHandle();
			handle.read(handleAddr);

			Memory mem = Memory.Instance;
			int inputAddr = buffersAddr.getValue(0);
			int outputAddr = buffersAddr.getValue(4);

			if (log.DebugEnabled)
			{
				int inputLength = 0x80;
				int outputLength = 0x80;
				switch (cmd)
				{
					case IOCTL_CMD_START_SCANNING:
						inputLength = 0x4C;
						outputLength = 0x600;
						break;
					case IOCTL_CMD_CREATE:
						inputLength = 0x70;
						break;
					case IOCTL_CMD_CONNECT:
						inputLength = 0x70;
						break;
					case IOCTL_CMD_GET_INFO:
						inputLength = 0x60;
						break;
					case IOCTL_CMD_ENTER_GAME_MODE:
						inputLength = 0x50;
						outputLength = 0x6;
						break;
					case IOCTL_CMD_SET_WEP_KEY:
						inputLength = 0xA0;
						break;
					case IOCTL_CMD_UNKNOWN_0x42:
						// Has no input and no output parameters
						break;
					case IOCTL_CMD_UNKNOWN_0x2:
						// Has no input and no output parameters
						break;
				}
				log.debug(string.Format("hleWlanIoctlCallback cmd=0x{0:X}, handleAddr={1}: {2}", cmd, handleAddr, handle));
				if (inputAddr != 0 && Memory.isAddressGood(inputAddr) && inputLength > 0)
				{
					log.debug(string.Format("hleWlanIoctlCallback inputAddr: {0}", Utilities.getMemoryDump(inputAddr, inputLength)));
					RuntimeContext.debugMemory(inputAddr, inputLength);
				}
				if (outputAddr != 0 && Memory.isAddressGood(outputAddr) && outputLength > 0)
				{
					log.debug(string.Format("hleWlanIoctlCallback outputAddr: {0}", Utilities.getMemoryDump(outputAddr, outputLength)));
					RuntimeContext.debugMemory(outputAddr, outputLength);
				}
				RuntimeContext.debugMemory(unknown.Address, 32);
			}

			bool signalSema = true;
			int errorCode = 0;
			string ssid;
			string bssid;
			int ssidLength;
			int mode;
			int channel;
			switch (cmd)
			{
				case IOCTL_CMD_START_SCANNING: // Start scanning
					mode = mem.read32(inputAddr + 0);
					channel = mem.read8(inputAddr + 10);
					// If scanning only the joined channel, it seems no
					// scan is really started as all information are available
					if (channel == joinedChannel && mem.read8(inputAddr + 11) == 0)
					{
						SceNetWlanScanInfo scanInfo = new SceNetWlanScanInfo();
						scanInfo.bssid = "pspsharp";
						scanInfo.channel = channel;
						scanInfo.ssid = channelSSIDs[channel];
						scanInfo.mode = channelModes[channel];
						scanInfo.unknown44 = 1000; // Unknown value, need to be != 0
						scanInfo.write(handleAddr.Memory, outputAddr + 4);

						mem.write32(outputAddr, 0); // Link to next SSID
					}
					else
					{
						if (channel != joinedChannel)
						{
							// When called by sceNetAdhocctlCreate() or sceNetAdhocctlConnect(),
							// the SSID is available in the inputAddr structure.
							// When called by sceNetAdhocctlScan(), no SSID is available
							// in the inputAddr structure.
							ssidLength = mem.read8(inputAddr + 24);
							ssid = Utilities.readStringNZ(mem, inputAddr + 28, ssidLength);
							setChannelSSID(channel, ssid, mode);
						}

						if (createWlanSocket())
						{
							signalSema = false;
							Emulator.Scheduler.addAction(Scheduler.Now, new WlanScanAction(this, handleAddr, inputAddr, outputAddr));
						}
					}
					break;
				case IOCTL_CMD_CREATE: // Called by sceNetAdhocctlCreate()
					channel = mem.read8(inputAddr + 6);
					ssidLength = mem.read8(inputAddr + 7);
					ssid = Utilities.readStringNZ(mem, inputAddr + 8, ssidLength);
					mode = mem.read32(inputAddr + 40);
					int unknown44 = mem.read32(inputAddr + 44); // 0x64
					int unknown62 = mem.read16(inputAddr + 62); // 0x22
					if (log.DebugEnabled)
					{
						log.debug(string.Format("hleWlanIoctlCallback cmd=0x{0:X}, channel={1:D}, ssid='{2}', mode=0x{3:X}, unknown44=0x{4:X}, unknown62=0x{5:X}", cmd, channel, ssid, mode, unknown44, unknown62));
					}
					joinChannelSSID(channel, ssid, mode);

					signalSema = false;
					Emulator.Scheduler.addAction(Scheduler.Now + wlanCreateActionDelayUs, new WlanCreateAction(this, handleAddr));
					break;
				case IOCTL_CMD_CONNECT: // Called by sceNetAdhocctlConnect() and sceNetAdhocctlJoin()
					// Receiving as input the SSID structure returned by cmd=0x34
					SceNetWlanScanInfo scanInfo = new SceNetWlanScanInfo();
					scanInfo.read(mem, inputAddr);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("hleWlanIoctlCallback cmd=0x{0:X}, channel={1:D}, ssid='{2}', mode=0x{3:X}", cmd, scanInfo.channel, scanInfo.ssid, scanInfo.mode));
					}
					joinChannelSSID(scanInfo.channel, scanInfo.ssid, scanInfo.mode);

					signalSema = false;
					Emulator.Scheduler.addAction(Scheduler.Now + wlanConnectActionDelayUs, new WlanConnectAction(this, handleAddr));
					break;
				case IOCTL_CMD_GET_INFO: // Get joined SSID
					// Remark: returning the joined SSID in the inputAddr!
					mem.memset(inputAddr, (sbyte) 0, 40);
					if (joinedChannel >= 0)
					{
						bssid = "pspsharp";
						Utilities.writeStringNZ(mem, inputAddr + 0, 6, bssid);
						mem.write8(inputAddr + 6, (sbyte) joinedChannel);
						mem.write8(inputAddr + 7, (sbyte) channelSSIDs[joinedChannel].Length);
						Utilities.writeStringNZ(mem, inputAddr + 8, 32, channelSSIDs[joinedChannel]);
					}
					break;
				case IOCTL_CMD_DISCONNECT: // Disconnect
					isGameMode = false;
					joinedChannel = -1;

					signalSema = false;
					Emulator.Scheduler.addAction(Scheduler.Now + wlanDisconnectActionDelayUs, new WlanDisconnectAction(this, handleAddr));
					break;
				case IOCTL_CMD_ENTER_GAME_MODE: // Enter Game Mode
					pspNetMacAddress multicastMacAddress = new pspNetMacAddress();
					multicastMacAddress.read(mem, inputAddr + 6);

					ssidLength = mem.read8(inputAddr + 12);
					ssid = Utilities.readStringNZ(mem, inputAddr + 14, ssidLength);

					pspNetMacAddress macAddress = new pspNetMacAddress();
					macAddress.read(mem, outputAddr + 0);

					if (log.DebugEnabled)
					{
						log.debug(string.Format("hleWlanIoctlCallback cmd=0x{0:X}, ssid='{1}', multicastMacAddress={2}, macAddress={3}", cmd, ssid, multicastMacAddress, macAddress));
					}
					isGameMode = true;
					break;
				case IOCTL_CMD_SET_WEP_KEY:
					int unknown1 = mem.read32(inputAddr + 0); // Always 0
					int unknown2 = mem.read32(inputAddr + 4); // Always 1
					if (log.DebugEnabled)
					{
						log.debug(string.Format("hleWlanIoctlCallback unknown1=0x{0:X}, unknown2=0x{1:X}", unknown1, unknown2));
					}

					int wepKeyAddr = inputAddr + 12;
					// 4 times the same data...
					for (int i = 0; i < 4; i++)
					{
						mode = mem.read32(wepKeyAddr + 0);
						string wepKey = Utilities.readStringNZ(wepKeyAddr + 4, 13);
						if (log.DebugEnabled)
						{
							log.debug(string.Format("hleWlanIoctlCallback cmd=0x{0:X}, wekKey#{1:D}: mode=0x{2:X}, wepKey='{3}'", cmd, i, mode, wepKey));
						}
						wepKeyAddr += 20;
					}
					break;
				default:
					log.warn(string.Format("hleWlanIoctlCallback unknown cmd=0x{0:X}", cmd));
					break;
			}
			handle.handleInternal.errorCode = errorCode;
			handle.write(handleAddr);
			if (signalSema)
			{
				Modules.ThreadManForUserModule.sceKernelSignalSema(handle.handleInternal.ioctlSemaId, 1);
			}

			return 0;
		}

		private static void sendDummyMessage(int step, TPointer handleAddr)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sendDummyMessage step={0:D}", step));
			}
			Memory mem = Memory.Instance;
			SceNetIfMessage message = new SceNetIfMessage();
			SceNetWlanMessage wlanMessage = new SceNetWlanMessage();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int size = message.sizeof() + wlanMessage.sizeof() + pspsharp.HLE.kernel.types.SceNetWlanMessage.maxContentLength + 0x12;
			int size = message.@sizeof() + wlanMessage.@sizeof() + SceNetWlanMessage.maxContentLength + 0x12;
			int allocatedAddr = Modules.sceNetIfhandleModule.hleNetMallocInternal(size);
			if (allocatedAddr <= 0)
			{
				return;
			}
			RuntimeContext.debugMemory(allocatedAddr, size);
			mem.memset(allocatedAddr, (sbyte) 0, size);

			TPointer messageAddr = new TPointer(mem, allocatedAddr);
			TPointer data = new TPointer(mem, messageAddr.Address + message.@sizeof());
			TPointer header = new TPointer(mem, data.Address);
			TPointer content = new TPointer(mem, header.Address + wlanMessage.@sizeof());

			int dataLength;
			int controlType;
			int contentLength;
			switch (step)
			{
				case 1:
					controlType = 2; // possible values: [1..8]
					contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];
					dataLength = wlanMessage.@sizeof() + contentLength;

					wlanMessage.dstMacAddress = new pspNetMacAddress(Wlan.MacAddress);
					wlanMessage.srcMacAddress = new pspNetMacAddress(dummyOtherMacAddress);
					wlanMessage.protocolType = WLAN_PROTOCOL_TYPE_SONY;
					wlanMessage.protocolSubType = WLAN_PROTOCOL_SUBTYPE_CONTROL; // 1 or 2 -> 1 will trigger sceNetIfhandleModule.unknownCallback3, 2 will trigger sceNetIfhandleModule.unknownCallback1
					wlanMessage.unknown16 = 1; // possible value: only 1
					wlanMessage.controlType = controlType;
					wlanMessage.contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];

					content.clear(contentLength);
					break;
				case 2:
					controlType = 0;
					contentLength = 0x4C;
					dataLength = wlanMessage.@sizeof() + contentLength;

					wlanMessage.dstMacAddress = new pspNetMacAddress(new sbyte[] {(sbyte)-1, (sbyte)-1, (sbyte)-1, (sbyte)-1, (sbyte)-1, (sbyte)-1}); // Broadcast MAC address
					wlanMessage.srcMacAddress = new pspNetMacAddress(dummyOtherMacAddress);
					wlanMessage.protocolType = WLAN_PROTOCOL_TYPE_SONY;
					wlanMessage.protocolSubType = WLAN_PROTOCOL_SUBTYPE_DATA; // 1 or 2 -> 1 will trigger sceNetIfhandleModule.unknownCallback3, 2 will trigger sceNetIfhandleModule.unknownCallback1
					wlanMessage.unknown16 = 0;
					wlanMessage.controlType = controlType;
					wlanMessage.contentLength = contentLength;

					content.clear(contentLength);
					content.setStringNZ(0x34, 5, "pspsharp");
					break;
				case 3:
					controlType = 2; // possible values: [1..8]
					contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];
					dataLength = wlanMessage.@sizeof() + contentLength + 0x12;

					wlanMessage.dstMacAddress = new pspNetMacAddress(new sbyte[] {(sbyte)-1, (sbyte)-1, (sbyte)-1, (sbyte)-1, (sbyte)-1, (sbyte)-1});
					wlanMessage.srcMacAddress = new pspNetMacAddress(dummyOtherMacAddress);
					wlanMessage.protocolType = WLAN_PROTOCOL_TYPE_SONY;
					wlanMessage.protocolSubType = WLAN_PROTOCOL_SUBTYPE_CONTROL; // 1 or 2 -> 1 will trigger sceNetIfhandleModule.unknownCallback3, 2 will trigger sceNetIfhandleModule.unknownCallback1
					wlanMessage.unknown16 = 1; // possible value: only 1
					wlanMessage.controlType = controlType;
					wlanMessage.contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];

					content.setStringNZ(0, 0x80, "JpcspOther");
					content.setValue8(0x80, (sbyte) 1);
					content.setValue8(0x81, (sbyte) 4);
					content.setUnalignedValue32(0x82, Modules.SysMemUserForUserModule.sceKernelDevkitVersion());
					content.setValue8(0x86, (sbyte) 2);
					content.setValue8(0x87, (sbyte) 4);
					content.setUnalignedValue32(0x88, Modules.SysMemUserForUserModule.sceKernelGetCompiledSdkVersion());
					content.setValue8(0x8C, (sbyte) 3);
					content.setValue8(0x8D, (sbyte) 4);
					content.setUnalignedValue32(0x8E, Modules.SysMemForKernelModule.sceKernelGetModel());
					break;
				case 4:
					controlType = 3; // possible values: [1..8]
					contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];
					dataLength = wlanMessage.@sizeof() + contentLength + 0x12;

					wlanMessage.dstMacAddress = new pspNetMacAddress(Wlan.MacAddress);
					wlanMessage.srcMacAddress = new pspNetMacAddress(dummyOtherMacAddress);
					wlanMessage.protocolType = WLAN_PROTOCOL_TYPE_SONY;
					wlanMessage.protocolSubType = WLAN_PROTOCOL_SUBTYPE_CONTROL; // 1 or 2 -> 1 will trigger sceNetIfhandleModule.unknownCallback3, 2 will trigger sceNetIfhandleModule.unknownCallback1
					wlanMessage.unknown16 = 1; // possible value: only 1
					wlanMessage.controlType = controlType;
					wlanMessage.contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];

					content.clear(contentLength);
					content.setStringNZ(0xA0, 0x80, "JpcspOther");
					content.setValue8(0x120, (sbyte) 1);
					content.setValue8(0x121, (sbyte) 4);
					content.setUnalignedValue32(0x82, Modules.SysMemUserForUserModule.sceKernelDevkitVersion());
					content.setValue8(0x126, (sbyte) 2);
					content.setValue8(0x127, (sbyte) 4);
					content.setUnalignedValue32(0x88, Modules.SysMemUserForUserModule.sceKernelGetCompiledSdkVersion());
					content.setValue8(0x12C, (sbyte) 3);
					content.setValue8(0x12D, (sbyte) 4);
					content.setUnalignedValue32(0x12E, Modules.SysMemForKernelModule.sceKernelGetModel());
					break;
				case 5:
					controlType = 4; // possible values: [1..8]
					contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];
					dataLength = wlanMessage.@sizeof() + contentLength;

					wlanMessage.dstMacAddress = new pspNetMacAddress(Wlan.MacAddress);
					wlanMessage.srcMacAddress = new pspNetMacAddress(dummyOtherMacAddress);
					wlanMessage.protocolType = WLAN_PROTOCOL_TYPE_SONY;
					wlanMessage.protocolSubType = WLAN_PROTOCOL_SUBTYPE_CONTROL; // 1 or 2 -> 1 will trigger sceNetIfhandleModule.unknownCallback3, 2 will trigger sceNetIfhandleModule.unknownCallback1
					wlanMessage.unknown16 = 1; // possible value: only 1
					wlanMessage.controlType = controlType;
					wlanMessage.contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];

					content.clear(contentLength);
					break;
				case 6:
					controlType = 5; // possible values: [1..8]
					contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];
					dataLength = wlanMessage.@sizeof() + contentLength;

					wlanMessage.dstMacAddress = new pspNetMacAddress(Wlan.MacAddress);
					wlanMessage.srcMacAddress = new pspNetMacAddress(dummyOtherMacAddress);
					wlanMessage.protocolType = WLAN_PROTOCOL_TYPE_SONY;
					wlanMessage.protocolSubType = WLAN_PROTOCOL_SUBTYPE_CONTROL; // 1 or 2 -> 1 will trigger sceNetIfhandleModule.unknownCallback3, 2 will trigger sceNetIfhandleModule.unknownCallback1
					wlanMessage.unknown16 = 1; // possible value: only 1
					wlanMessage.controlType = controlType;
					wlanMessage.contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];

					content.clear(contentLength);
					break;
				case 7:
					controlType = 6; // possible values: [1..8]
					contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];
					dataLength = wlanMessage.@sizeof() + contentLength;

					wlanMessage.dstMacAddress = new pspNetMacAddress(Wlan.MacAddress);
					wlanMessage.srcMacAddress = new pspNetMacAddress(dummyOtherMacAddress);
					wlanMessage.protocolType = WLAN_PROTOCOL_TYPE_SONY;
					wlanMessage.protocolSubType = WLAN_PROTOCOL_SUBTYPE_CONTROL; // 1 or 2 -> 1 will trigger sceNetIfhandleModule.unknownCallback3, 2 will trigger sceNetIfhandleModule.unknownCallback1
					wlanMessage.unknown16 = 1; // possible value: only 1
					wlanMessage.controlType = controlType;
					wlanMessage.contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];

					content.clear(contentLength);
					break;
				case 8:
					controlType = 8; // possible values: [1..8]
					contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];
					dataLength = wlanMessage.@sizeof() + contentLength;

					wlanMessage.dstMacAddress = new pspNetMacAddress(Wlan.MacAddress);
					wlanMessage.srcMacAddress = new pspNetMacAddress(dummyOtherMacAddress);
					wlanMessage.protocolType = WLAN_PROTOCOL_TYPE_SONY;
					wlanMessage.protocolSubType = WLAN_PROTOCOL_SUBTYPE_CONTROL; // 1 or 2 -> 1 will trigger sceNetIfhandleModule.unknownCallback3, 2 will trigger sceNetIfhandleModule.unknownCallback1
					wlanMessage.unknown16 = 1; // possible value: only 1
					wlanMessage.controlType = controlType;
					wlanMessage.contentLength = SceNetWlanMessage.contentLengthFromMessageType[controlType];

					content.clear(contentLength);
					break;
				default:
					dataLength = 0;
					break;
			}

			wlanMessage.write(header);

			message.dataAddr = data.Address;
			message.dataLength = dataLength;
			message.unknown18 = 0;
			message.unknown24 = dataLength;
			message.write(messageAddr);

			if (dataLength > 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Sending dummy message: {0}", message));
					log.debug(string.Format("Dummy message data: {0}", Utilities.getMemoryDump(data.Address, dataLength)));
				}

				int sceNetIfEnqueue = NIDMapper.Instance.getAddressByName("sceNetIfEnqueue");
				if (sceNetIfEnqueue != 0)
				{
					SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
					Modules.ThreadManForUserModule.executeCallback(thread, sceNetIfEnqueue, null, true, handleAddr.Address, messageAddr.Address);
				}
			}
		}

		private void sendDummyMessage(TPointer handleAddr, SceNetIfMessage sentMessage, SceNetWlanMessage sentWlanMessage)
		{
			int step = 0;
			if (false)
			{
				step = 1;
			}
			else if (false)
			{
				step = 2;
			}
			else if (dummyMessageStep < 0 && !sentWlanMessage.dstMacAddress.Equals(dummyOtherMacAddress))
			{
				step = 3;
			}
			else if (sentWlanMessage.controlType == 3)
			{
				step = 5;
			}
			else if (sentWlanMessage.controlType == 4)
			{
				step = 5;
			}
			else if (sentWlanMessage.controlType == 5)
			{
				step = 7;
			}
			else if (sentWlanMessage.controlType == 7)
			{
				step = 8;
			}
			else
			{
				step = 0;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Adding action step={0:D} for sending dummy message", step));
			}
			dummyMessageStep = step;
			dummyMessageHandleAddr = handleAddr;
		}

		private class AfterNetCreateIfhandleEtherAction : IAction
		{
			private readonly sceWlan outerInstance;

			internal SceKernelThreadInfo thread;
			internal TPointer handleAddr;

			public AfterNetCreateIfhandleEtherAction(sceWlan outerInstance, SceKernelThreadInfo thread, TPointer handleAddr)
			{
				this.outerInstance = outerInstance;
				this.thread = thread;
				this.handleAddr = handleAddr;
			}

			public virtual void execute()
			{
				outerInstance.afterNetCreateIfhandleEtherAction(thread, handleAddr);
			}
		}

		private void afterNetCreateIfhandleEtherAction(SceKernelThreadInfo thread, TPointer handleAddr)
		{
			int tempMem = Modules.sceNetIfhandleModule.hleNetMallocInternal(32);
			if (tempMem <= 0)
			{
				return;
			}

			int macAddressAddr = tempMem;
			int interfaceNameAddr = tempMem + 8;

			pspNetMacAddress macAddress = new pspNetMacAddress(Wlan.MacAddress);
			macAddress.write(handleAddr.Memory, macAddressAddr);

			Utilities.writeStringZ(handleAddr.Memory, interfaceNameAddr, "wlan");

			int sceNetAttachIfhandleEther = NIDMapper.Instance.getAddressByName("sceNetAttachIfhandleEther");
			if (sceNetAttachIfhandleEther == 0)
			{
				return;
			}

			Modules.ThreadManForUserModule.executeCallback(thread, sceNetAttachIfhandleEther, null, true, handleAddr.Address, macAddressAddr, interfaceNameAddr);
		}

		private int createWlanInterface()
		{
			SceNetIfHandle handle = new SceNetIfHandle();
			handle.callbackArg4 = 0x11040404; // dummy callback value
			handle.upCallbackAddr = ThreadManForUser.WLAN_UP_CALLBACK_ADDRESS;
			handle.downCallbackAddr = ThreadManForUser.WLAN_DOWN_CALLBACK_ADDRESS;
			handle.sendCallbackAddr = ThreadManForUser.WLAN_SEND_CALLBACK_ADDRESS;
			handle.ioctlCallbackAddr = ThreadManForUser.WLAN_IOCTL_CALLBACK_ADDRESS;
			int handleMem = Modules.sceNetIfhandleModule.hleNetMallocInternal(handle.@sizeof());
			TPointer handleAddr = new TPointer(Memory.Instance, handleMem);
			handle.write(handleAddr);
			RuntimeContext.debugMemory(handleAddr.Address, handle.@sizeof());

			int sceNetCreateIfhandleEther = NIDMapper.Instance.getAddressByName("sceNetCreateIfhandleEther");
			if (sceNetCreateIfhandleEther == 0)
			{
				int result = sceNetIfhandleModule.hleNetCreateIfhandleEther(handleAddr);
				if (result < 0)
				{
					return result;
				}

				result = sceNetIfhandleModule.hleNetAttachIfhandleEther(handleAddr, new pspNetMacAddress(Wlan.MacAddress), "wlan");
				if (result < 0)
				{
					return result;
				}
			}
			else
			{
				SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
				Modules.ThreadManForUserModule.executeCallback(thread, sceNetCreateIfhandleEther, new AfterNetCreateIfhandleEtherAction(this, thread, handleAddr), false, handleAddr.Address);
			}

			return 0;
		}

		/// <summary>
		/// Get the Ethernet Address of the wlan controller
		/// </summary>
		/// <param name="etherAddr"> - pointer to a buffer of u8 (NOTE: it only writes to 6 bytes, but
		/// requests 8 so pass it 8 bytes just in case) </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0C622081, version = 150, checkInsideInterrupt = true) public int sceWlanGetEtherAddr(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=6, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer etherAddr)
		[HLEFunction(nid : 0x0C622081, version : 150, checkInsideInterrupt : true)]
		public virtual int sceWlanGetEtherAddr(TPointer etherAddr)
		{
			pspNetMacAddress macAddress = new pspNetMacAddress();
			macAddress.MacAddress = Wlan.MacAddress;
			macAddress.write(etherAddr);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceWlanGetEtherAddr returning {0}", macAddress));
			}

			return 0;
		}

		/// <summary>
		/// Determine the state of the Wlan power switch
		/// </summary>
		/// <returns> 0 if off, 1 if on </returns>
		[HLEFunction(nid : 0xD7763699, version : 150)]
		public virtual int sceWlanGetSwitchState()
		{
			return Wlan.SwitchState;
		}

		/// <summary>
		/// Determine if the wlan device is currently powered on
		/// </summary>
		/// <returns> 0 if off, 1 if on </returns>
		[HLEFunction(nid : 0x93440B11, version : 150)]
		public virtual int sceWlanDevIsPowerOn()
		{
			return Wlan.SwitchState;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x482CAE9A, version = 150) public int sceWlanDevAttach()
		[HLEFunction(nid : 0x482CAE9A, version : 150)]
		public virtual int sceWlanDevAttach()
		{
			// Has no parameters
			int result = createWlanInterface();
			if (result < 0)
			{
				log.error(string.Format("Cannot create the WLAN Interface: 0x{0:X8}", result));
				return result;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC9A8CAB7, version = 150) public int sceWlanDevDetach()
		[HLEFunction(nid : 0xC9A8CAB7, version : 150)]
		public virtual int sceWlanDevDetach()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8D5F551B, version = 150) public int sceWlanDrv_lib_8D5F551B(int unknown)
		[HLEFunction(nid : 0x8D5F551B, version : 150)]
		public virtual int sceWlanDrv_lib_8D5F551B(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x749B813A, version = 150) public int sceWlanSetHostDiscover(int unknown1, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=40, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknown2)
		[HLEFunction(nid : 0x749B813A, version : 150)]
		public virtual int sceWlanSetHostDiscover(int unknown1, TPointer unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFE8A0B46, version = 150) public int sceWlanSetWakeUp(int unknown1, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=40, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknown2)
		[HLEFunction(nid : 0xFE8A0B46, version : 150)]
		public virtual int sceWlanSetWakeUp(int unknown1, TPointer unknown2)
		{
			return 0;
		}

		[HLEFunction(nid : 0x5E7C8D94, version : 150)]
		public virtual bool sceWlanDevIsGameMode()
		{
			return isGameMode;
		}

		[HLEFunction(nid : 0x5ED4049A, version : 150)]
		public virtual int sceWlanGPPrevEstablishActive(pspNetMacAddress macAddress)
		{
			int index = 0;
			foreach (pspNetMacAddress activeMacAddress in activeMacAddresses)
			{
				if (activeMacAddress.Equals(macAddress.macAddress))
				{
					return index;
				}
				index++;
			}

			return -1;
		}

		/*
		 * Called by sceNetAdhocGameModeUpdateReplica()
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA447103A, version = 150) public int sceWlanGPRecv(int id, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buffer, int bufferLength, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.variableLength, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer updateInfoAddr)
		[HLEFunction(nid : 0xA447103A, version : 150)]
		public virtual int sceWlanGPRecv(int id, TPointer buffer, int bufferLength, TPointer updateInfoAddr)
		{
			if (!isGameMode)
			{
				return SceKernelErrors.ERROR_WLAN_NOT_IN_GAMEMODE;
			}
			if (id < 0 || id >= gameModeStates.Count)
			{
				return SceKernelErrors.ERROR_WLAN_BAD_PARAMS;
			}
			if (bufferLength < 0 || bufferLength > gameModeDataLength)
			{
				return SceKernelErrors.ERROR_WLAN_BAD_PARAMS;
			}

			GameModeState gameModeState = gameModeStates[id];
			int size = System.Math.Min(gameModeState.dataLength, bufferLength);
			Utilities.writeBytes(buffer.Address, size, gameModeState.data, 0);

			if (updateInfoAddr.NotNull)
			{
				sceNetAdhoc.GameModeUpdateInfo updateInfo = new sceNetAdhoc.GameModeUpdateInfo();
				updateInfo.read(updateInfoAddr);
				updateInfo.updated = gameModeState.updated ? 1 : 0;
				updateInfo.timeStamp = gameModeState.timeStamp;
				updateInfo.write(updateInfoAddr);
			}

			return 0;
		}

		/*
		 * Called by sceNetAdhocGameModeUpdateMaster()
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB4D7CB74, version = 150) public int sceWlanGPSend(int unknown, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer buffer, int bufferLength)
		[HLEFunction(nid : 0xB4D7CB74, version : 150)]
		public virtual int sceWlanGPSend(int unknown, TPointer buffer, int bufferLength)
		{
			if (!isGameMode)
			{
				return SceKernelErrors.ERROR_WLAN_NOT_IN_GAMEMODE;
			}
			if (bufferLength < 0 || bufferLength > gameModeDataLength)
			{
				return SceKernelErrors.ERROR_WLAN_BAD_PARAMS;
			}

			GameModeState myGameModeState = MyGameModeState;
			if (myGameModeState == null)
			{
				log.error(string.Format("sceWlanGPSend not found my GameModeState!"));
				return -1;
			}

			Utilities.readBytes(buffer.Address, bufferLength, myGameModeState.data, 0);
			myGameModeState.doUpdate();

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2D0FAE4E, version = 150) public int sceWlanDrv_lib_2D0FAE4E(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=6, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer16 unknown)
		[HLEFunction(nid : 0x2D0FAE4E, version : 150)]
		public virtual int sceWlanDrv_lib_2D0FAE4E(TPointer16 unknown)
		{
			unknownValue1 = unknown.getValue(0);
			unknownValue2 = unknown.getValue(2);
			unknownValue3 = unknown.getValue(4);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x56F467CA, version = 150) public int sceWlanDrv_lib_56F467CA(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=6, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer16 unknown)
		[HLEFunction(nid : 0x56F467CA, version : 150)]
		public virtual int sceWlanDrv_lib_56F467CA(TPointer16 unknown)
		{
			unknown.setValue(0, unknownValue1);
			unknown.setValue(2, unknownValue2);
			unknown.setValue(4, unknownValue3);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5BAA1FE5, version = 150) public int sceWlanDrv_lib_5BAA1FE5(int unknown1, int unknown2)
		[HLEFunction(nid : 0x5BAA1FE5, version : 150)]
		public virtual int sceWlanDrv_lib_5BAA1FE5(int unknown1, int unknown2)
		{
			return 0;
		}

		/// <summary>
		/// Checks if a packet has to be dropped according
		/// to the parameters defined by sceNetSetDropRate.
		/// </summary>
		/// <returns> true if the packet should be dropped
		///         false if the packet should be processed </returns>
		[HLEFunction(nid : 0x2519EAA7, version : 150)]
		public virtual bool sceWlanIsPacketToBeDropped()
		{
			// Has no parameters
			return false;
		}

		[HLEFunction(nid : 0x325F7172, version : 150)]
		public virtual int sceWlanSetDropRate(int dropRate, int dropDuration)
		{
			wlanDropRate = dropRate;
			wlanDropDuration = dropDuration;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB6A9700D, version = 150) public int sceWlanGetDropRate(@CanBeNull pspsharp.HLE.TPointer32 dropRateAddr, @CanBeNull pspsharp.HLE.TPointer32 dropDurationAddr)
		[HLEFunction(nid : 0xB6A9700D, version : 150)]
		public virtual int sceWlanGetDropRate(TPointer32 dropRateAddr, TPointer32 dropDurationAddr)
		{
			dropRateAddr.setValue(wlanDropRate);
			dropDurationAddr.setValue(wlanDropDuration);

			return 0;
		}
	}
}