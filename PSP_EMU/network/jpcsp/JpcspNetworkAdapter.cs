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
namespace pspsharp.network.pspsharp
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.Modules.sceNetAdhocModule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.Modules.sceNetAdhocctlModule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocctl.PSP_ADHOCCTL_MODE_GAMEMODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.pspsharp.JpcspAdhocPtpMessage.PTP_MESSAGE_TYPE_DATA;


	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using sceNetAdhoc = pspsharp.HLE.modules.sceNetAdhoc;
	using sceNetAdhocctl = pspsharp.HLE.modules.sceNetAdhocctl;
	using sceNetInet = pspsharp.HLE.modules.sceNetInet;
	using sceUtility = pspsharp.HLE.modules.sceUtility;
	using GameModeArea = pspsharp.HLE.modules.sceNetAdhoc.GameModeArea;
	using Wlan = pspsharp.hardware.Wlan;
	using AdhocMatchingEventMessage = pspsharp.network.adhoc.AdhocMatchingEventMessage;
	using AdhocMessage = pspsharp.network.adhoc.AdhocMessage;
	using MatchingObject = pspsharp.network.adhoc.MatchingObject;
	using PdpObject = pspsharp.network.adhoc.PdpObject;
	using PtpObject = pspsharp.network.adhoc.PtpObject;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class JpcspNetworkAdapter : BaseNetworkAdapter
	{
		private DatagramSocket adhocctlSocket;
		// Try to use a port unused by other applications...
		private const int adhocctlBroadcastPort = 30004;

		public override void stop()
		{
			if (adhocctlSocket != null)
			{
				adhocctlSocket.close();
				adhocctlSocket = null;
			}

			base.stop();
		}

		public override void sceNetAdhocctlInit()
		{
		}

		public override void sceNetAdhocctlTerm()
		{
		}

		public override void sceNetAdhocctlConnect()
		{
		}

		public override void sceNetAdhocctlCreate()
		{
		}

		public override void sceNetAdhocctlJoin()
		{
		}

		public override void sceNetAdhocctlDisconnect()
		{
		}

		public override void sceNetAdhocctlScan()
		{
		}

		public override AdhocMessage createAdhocPdpMessage(int address, int length, sbyte[] destMacAddress)
		{
			return new JpcspAdhocPdpMessage(address, length, destMacAddress);
		}

		public override AdhocMessage createAdhocPdpMessage(sbyte[] message, int length)
		{
			return new JpcspAdhocPdpMessage(message, length);
		}

		public override PdpObject createPdpObject()
		{
			return new JpcspPdpObject(this);
		}

		public override PtpObject createPtpObject()
		{
			return new JpcspPtpObject(this);
		}

		public override AdhocMessage createAdhocPtpMessage(int address, int length)
		{
			return new JpcspAdhocPtpMessage(address, length, PTP_MESSAGE_TYPE_DATA);
		}

		public override AdhocMessage createAdhocPtpMessage(sbyte[] message, int length)
		{
			return new JpcspAdhocPtpMessage(message, length);
		}

		public override AdhocMessage createAdhocGameModeMessage(sceNetAdhoc.GameModeArea gameModeArea)
		{
			return new JpcspAdhocGameModeMessage(gameModeArea);
		}

		public override AdhocMessage createAdhocGameModeMessage(sbyte[] message, int length)
		{
			return new JpcspAdhocGameModeMessage(message, length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public java.net.SocketAddress getSocketAddress(byte[] macAddress, int realPort) throws java.net.UnknownHostException
		public override SocketAddress getSocketAddress(sbyte[] macAddress, int realPort)
		{
			if (sceNetAdhocModule.hasNetPortShiftActive())
			{
				return new InetSocketAddress(InetAddress.LocalHost, realPort);
			}
			return sceNetInet.getBroadcastInetSocketAddress(realPort)[0];
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public java.net.SocketAddress[] getMultiSocketAddress(byte[] macAddress, int realPort) throws java.net.UnknownHostException
		public override SocketAddress[] getMultiSocketAddress(sbyte[] macAddress, int realPort)
		{
			if (sceNetAdhocModule.hasNetPortShiftActive())
			{
				return base.getMultiSocketAddress(macAddress, realPort);
			}
			return sceNetInet.getBroadcastInetSocketAddress(realPort);
		}

		public override MatchingObject createMatchingObject()
		{
			return new JpcspMatchingObject(this);
		}

		public override AdhocMatchingEventMessage createAdhocMatchingEventMessage(MatchingObject matchingObject, int @event)
		{
			return new JpcspAdhocMatchingEventMessage(matchingObject, @event);
		}

		public override AdhocMatchingEventMessage createAdhocMatchingEventMessage(MatchingObject matchingObject, int @event, int data, int dataLength, sbyte[] macAddress)
		{
			return new JpcspAdhocMatchingEventMessage(matchingObject, @event, data, dataLength, macAddress);
		}

		public override AdhocMatchingEventMessage createAdhocMatchingEventMessage(MatchingObject matchingObject, sbyte[] message, int length)
		{
			return new JpcspAdhocMatchingEventMessage(matchingObject, message, length);
		}

		public override void sendChatMessage(string message)
		{
			// TODO Implement Chat
			log.warn(string.Format("Chat functionality not supported: {0}", message));
		}

		public override bool ConnectComplete
		{
			get
			{
				return sceNetAdhocctlModule.NumberPeers > 0 || sceNetAdhocctlModule.hleNetAdhocctlGetState() == sceNetAdhocctl.PSP_ADHOCCTL_STATE_DISCONNECTED;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void openSocket() throws java.net.SocketException
		private void openSocket()
		{
			if (adhocctlSocket == null)
			{
				adhocctlSocket = new DatagramSocket(sceNetAdhocModule.getRealPortFromServerPort(adhocctlBroadcastPort));
				// For broadcast
				adhocctlSocket.Broadcast = true;
				// Non-blocking (timeout = 0 would mean blocking)
				adhocctlSocket.SoTimeout = 1;
			}
		}

		private void broadcastPeers()
		{
			if (sceNetAdhocctlModule.hleNetAdhocctlGetGroupName() == null)
			{
				return;
			}

			try
			{
				openSocket();

				JpcspAdhocctlMessage adhocctlMessage = new JpcspAdhocctlMessage(sceUtility.SystemParamNickname, Wlan.MacAddress, sceNetAdhocctlModule.hleNetAdhocctlGetGroupName());
				if (sceNetAdhocctlModule.hleNetAdhocctlGetMode() == PSP_ADHOCCTL_MODE_GAMEMODE && sceNetAdhocctlModule.hleNetAdhocctlGetRequiredGameModeMacs().size() > 0)
				{
					bool gameModeComplete = sceNetAdhocctlModule.GameModeComplete;
					adhocctlMessage.setGameModeComplete(gameModeComplete, sceNetAdhocctlModule.hleNetAdhocctlGetRequiredGameModeMacs());
				}
				SocketAddress[] socketAddress = sceNetAdhocModule.getMultiSocketAddress(sceNetAdhoc.ANY_MAC_ADDRESS, sceNetAdhocModule.getRealPortFromClientPort(sceNetAdhoc.ANY_MAC_ADDRESS, adhocctlBroadcastPort));
				for (int i = 0; i < socketAddress.Length; i++)
				{
					DatagramPacket packet = new DatagramPacket(adhocctlMessage.Message, JpcspAdhocctlMessage.MessageLength, socketAddress[i]);
					adhocctlSocket.send(packet);

					if (log.DebugEnabled)
					{
						log.debug(string.Format("broadcast sent to peer[{0}]: {1}", socketAddress[i], adhocctlMessage));
					}
				}
			}
			catch (SocketException e)
			{
				log.error("broadcastPeers", e);
			}
			catch (IOException e)
			{
				log.error("broadcastPeers", e);
			}
		}

		private void pollPeers()
		{
			try
			{
				openSocket();

				// Poll all the available messages.
				// Exiting the loop only when no more messages are available (SocketTimeoutException)
				while (true)
				{
					sbyte[] bytes = new sbyte[JpcspAdhocctlMessage.MessageLength];
					DatagramPacket packet = new DatagramPacket(bytes, bytes.Length);
					adhocctlSocket.receive(packet);
					JpcspAdhocctlMessage adhocctlMessage = new JpcspAdhocctlMessage(packet.Data, packet.Length);

					if (log.DebugEnabled)
					{
						log.debug(string.Format("broadcast received from peer: {0}", adhocctlMessage));
					}

					// Ignore messages coming from myself
					if (!sceNetAdhoc.isSameMacAddress(Wlan.MacAddress, adhocctlMessage.macAddress))
					{
						if (adhocctlMessage.groupName.Equals(sceNetAdhocctlModule.hleNetAdhocctlGetGroupName()))
						{
							sceNetAdhocctlModule.hleNetAdhocctlAddPeer(adhocctlMessage.nickName, new pspNetMacAddress(adhocctlMessage.macAddress));
						}

						if (adhocctlMessage.ibss.Equals(sceNetAdhocctlModule.hleNetAdhocctlGetIBSS()))
						{
							sceNetAdhocctlModule.hleNetAdhocctlAddNetwork(adhocctlMessage.groupName, new pspNetMacAddress(adhocctlMessage.macAddress), adhocctlMessage.channel, adhocctlMessage.ibss, adhocctlMessage.mode);

							if (adhocctlMessage.mode == PSP_ADHOCCTL_MODE_GAMEMODE)
							{
								sceNetAdhocctlModule.hleNetAdhocctlAddGameModeMac(adhocctlMessage.macAddress);
								if (sceNetAdhocctlModule.hleNetAdhocctlGetRequiredGameModeMacs().size() <= 0)
								{
									sceNetAdhocctlModule.hleNetAdhocctlSetGameModeJoinComplete(adhocctlMessage.gameModeComplete);
									if (adhocctlMessage.gameModeComplete)
									{
										sbyte[][] macs = adhocctlMessage.gameModeMacs;
										if (macs != null)
										{
											sceNetAdhocctlModule.hleNetAdhocctlSetGameModeMacs(macs);
										}
									}
								}
							}
						}
					}
				}
			}
			catch (SocketException e)
			{
				log.error("broadcastPeers", e);
			}
			catch (SocketTimeoutException)
			{
				// Nothing available
			}
			catch (IOException e)
			{
				log.error("broadcastPeers", e);
			}
		}

		public override void updatePeers()
		{
			broadcastPeers();
			pollPeers();
		}
	}

}