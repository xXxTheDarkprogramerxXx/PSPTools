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
namespace pspsharp.network.proonline
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhoc.isSameMacAddress;


	using ChatGUI = pspsharp.GUI.ChatGUI;
	using Modules = pspsharp.HLE.Modules;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using sceNet = pspsharp.HLE.modules.sceNet;
	using sceNetAdhoc = pspsharp.HLE.modules.sceNetAdhoc;
	using sceNetApctl = pspsharp.HLE.modules.sceNetApctl;
	using GameModeArea = pspsharp.HLE.modules.sceNetAdhoc.GameModeArea;
	using AdhocMatchingEventMessage = pspsharp.network.adhoc.AdhocMatchingEventMessage;
	using AdhocMessage = pspsharp.network.adhoc.AdhocMessage;
	using MatchingObject = pspsharp.network.adhoc.MatchingObject;
	using PdpObject = pspsharp.network.adhoc.PdpObject;
	using PtpObject = pspsharp.network.adhoc.PtpObject;
	using SceNetAdhocctlPacketBaseC2S = pspsharp.network.proonline.PacketFactory.SceNetAdhocctlPacketBaseC2S;
	using SceNetAdhocctlPacketBaseS2C = pspsharp.network.proonline.PacketFactory.SceNetAdhocctlPacketBaseS2C;
	using UPnP = pspsharp.network.upnp.UPnP;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using AbstractIntSettingsListener = pspsharp.settings.AbstractIntSettingsListener;
	using AbstractStringSettingsListener = pspsharp.settings.AbstractStringSettingsListener;
	using Settings = pspsharp.settings.Settings;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class ProOnlineNetworkAdapter : BaseNetworkAdapter
	{
		protected internal static new Logger log = Logger.getLogger("ProOnline");
		private static bool enabled = false;
		private UPnP upnp;
		private Socket metaSocket;
		private static int metaPort = 27312;
		private static string metaServer = "coldbird.net";
		private const int pingTimeoutMillis = 2000;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private volatile bool exit_Renamed;
		private volatile bool friendFinderActive;
		// All access to macIps have to be synchronized because they can be executed
		// from different threads (PSP thread + Friend Finder thread).
		private IList<MacIp> macIps = new LinkedList<MacIp>();
		private PacketFactory packetFactory = new PacketFactory();
		private PortManager portManager;
		private InetAddress broadcastInetAddress;
		private InetAddress loopbackInetAddress;
		private InetAddress localHostInetAddress;
		private ChatGUI chatGUI;
		private bool connectComplete;

		private class MetaServerSettingsListener : AbstractStringSettingsListener
		{
			protected internal override void settingsValueChanged(string value)
			{
				metaServer = value;
			}
		}

		private class MetaServerPortSettingsListener : AbstractIntSettingsListener
		{
			protected internal override void settingsValueChanged(int value)
			{
				metaPort = value;
			}
		}

		private class EnabledSettingsListener : AbstractBoolSettingsListener
		{
			protected internal override void settingsValueChanged(bool value)
			{
				Enabled = value;
			}
		}

		public static bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				ProOnlineNetworkAdapter.enabled = value;
				if (value)
				{
					log.info("Enabling ProOnline network");
				}
			}
		}


		public static string MetaServer
		{
			get
			{
				return metaServer;
			}
		}

		protected internal class FriendFinder : Thread
		{
			private readonly ProOnlineNetworkAdapter outerInstance;

			public FriendFinder(ProOnlineNetworkAdapter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void run()
			{
				outerInstance.friendFinder();
			}
		}

		public override void start()
		{
			base.start();

			log.info(string.Format("ProOnline start, server {0}:{1:D}", metaServer, metaPort));

			try
			{
				broadcastInetAddress = InetAddress.getByAddress(new sbyte[] {1, 1, 1, 1});
			}
			catch (UnknownHostException e)
			{
				log.error("Unable to set the broadcast address", e);
			}

			try
			{
				loopbackInetAddress = InetAddress.getByName("localhost");
			}
			catch (UnknownHostException e)
			{
				log.error("Unable to set the loopback address", e);
			}

			try
			{
				localHostInetAddress = InetAddress.getByName(sceNetApctl.LocalHostIP);
			}
			catch (UnknownHostException e)
			{
				log.error("Unable to set the local address", e);
			}

			upnp = new UPnP();
			upnp.discoverInBackground();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void sendToMetaServer(pspsharp.network.proonline.PacketFactory.SceNetAdhocctlPacketBaseC2S packet) throws java.io.IOException
		protected internal virtual void sendToMetaServer(SceNetAdhocctlPacketBaseC2S packet)
		{
			if (metaSocket != null)
			{
				metaSocket.OutputStream.write(packet.Bytes);
				metaSocket.OutputStream.flush();
				if (log.TraceEnabled)
				{
					log.trace(string.Format("Sent packet to meta server: {0}", packet));
				}
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Message not sent to meta server because not connected: {0}", packet));
				}
			}
		}

		protected internal virtual void safeSendToMetaServer(SceNetAdhocctlPacketBaseC2S packet)
		{
			try
			{
				sendToMetaServer(packet);
			}
			catch (IOException)
			{
				// Ignore exception
			}
		}

		private void openChat()
		{
			if (chatGUI == null || !chatGUI.Visible)
			{
				chatGUI = new ChatGUI();
				Emulator.MainGUI.startBackgroundWindowDialog(chatGUI);
				chatGUI.updateMembers(Modules.sceNetAdhocctlModule.PeersNickName);
			}
		}

		private void closeChat()
		{
			if (chatGUI != null)
			{
				chatGUI.dispose();
				chatGUI = null;
			}
		}

		private void waitForFriendFinderToExit()
		{
			while (friendFinderActive && exit_Renamed)
			{
				Utilities.sleep(1, 0);
			}
		}

		public override void sceNetAdhocctlInit()
		{
			if (log.DebugEnabled)
			{
				log.debug("sceNetAdhocctlInit");
			}

			// Wait for a previous instance of the Friend Finder thread to terminate
			waitForFriendFinderToExit();

			terminatePortManager();
			closeConnectionToMetaServer();
			connectToMetaServer();
			exit_Renamed = false;

			portManager = new PortManager(upnp);

			if (metaSocket != null)
			{
				Thread friendFinderThread = new FriendFinder(this);
				friendFinderThread.Name = "ProOnline Friend Finder";
				friendFinderThread.Daemon = true;
				friendFinderThread.Start();
			}
		}

		public override void sceNetAdhocctlConnect()
		{
			if (log.DebugEnabled)
			{
				log.debug("sceNetAdhocctlConnect redirecting to sceNetAdhocctlCreate");
			}

			sceNetAdhocctlCreate();
		}

		public override void sceNetAdhocctlCreate()
		{
			if (log.DebugEnabled)
			{
				log.debug("sceNetAdhocctlCreate");
			}

			try
			{
				sendToMetaServer(new PacketFactory.SceNetAdhocctlConnectPacketC2S(this));
				openChat();
			}
			catch (IOException e)
			{
				log.error("sceNetAdhocctlCreate", e);
			}
		}

		public override void sceNetAdhocctlJoin()
		{
			if (log.DebugEnabled)
			{
				log.debug("sceNetAdhocctlJoin redirecting to sceNetAdhocctlCreate");
			}

			sceNetAdhocctlCreate();
		}

		public override void sceNetAdhocctlDisconnect()
		{
			if (log.DebugEnabled)
			{
				log.debug("sceNetAdhocctlDisconnect");
			}

			try
			{
				sendToMetaServer(new PacketFactory.SceNetAdhocctlDisconnectPacketC2S(this));
				ConnectComplete = false;
				deleteAllFriends();
				closeChat();
			}
			catch (IOException e)
			{
				log.error("sceNetAdhocctlDisconnect", e);
			}
		}

		public override void sceNetAdhocctlTerm()
		{
			if (log.DebugEnabled)
			{
				log.debug("sceNetAdhocctlTerm");
			}

			exit_Renamed = true;
			terminatePortManager();
		}

		public override void sceNetAdhocctlScan()
		{
			if (log.DebugEnabled)
			{
				log.debug("sceNetAdhocctlScan");
			}

			try
			{
				sendToMetaServer(new PacketFactory.SceNetAdhocctlScanPacketC2S(this));
			}
			catch (IOException e)
			{
				log.error("sceNetAdhocctlScan", e);
			}
		}

		public virtual void sceNetPortOpen(string protocol, int port)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetPortOpen {0}, port={1:D}", protocol, port));
			}
			portManager.addPort(port, protocol);
		}

		public virtual void sceNetPortClose(string protocol, int port)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetPortClose {0}, port={1:D}", protocol, port));
			}
			portManager.removePort(port, protocol);
		}

		private void connectToMetaServer()
		{
			try
			{
				metaSocket = new Socket(metaServer, metaPort);
				metaSocket.ReuseAddress = true;
				metaSocket.SoTimeout = 500;

				PacketFactory.SceNetAdhocctlLoginPacketC2S loginPacket = new PacketFactory.SceNetAdhocctlLoginPacketC2S(this);

				sendToMetaServer(loginPacket);
			}
			catch (UnknownHostException e)
			{
				log.error(string.Format("Could not connect to meta server {0}:{1:D}", metaServer, metaPort), e);
			}
			catch (IOException e)
			{
				log.error(string.Format("Could not connect to meta server {0}:{1:D}", metaServer, metaPort), e);
			}
		}

		/// <summary>
		/// Delete all the port/host mappings
		/// </summary>
		private void terminatePortManager()
		{
			if (portManager != null)
			{
				portManager.clear();
				portManager = null;
			}
		}

		private void closeConnectionToMetaServer()
		{
			if (metaSocket != null)
			{
				try
				{
					metaSocket.close();
				}
				catch (IOException e)
				{
					log.error("friendFinder", e);
				}
				metaSocket = null;
			}
		}

		protected internal virtual void friendFinder()
		{
			long lastPing = Emulator.Clock.currentTimeMillis();
			sbyte[] buffer = new sbyte[1024];
			int offset = 0;

			if (log.DebugEnabled)
			{
				log.debug("Starting friendFinder");
			}

			friendFinderActive = true;

			while (!exit_Renamed)
			{
				long now = Emulator.Clock.currentTimeMillis();
				if (now - lastPing >= pingTimeoutMillis)
				{
					lastPing = now;
					safeSendToMetaServer(new PacketFactory.SceNetAdhocctlPingPacketC2S(this));
				}

				try
				{
					int length = metaSocket.InputStream.read(buffer, offset, buffer.Length - offset);
					if (length > 0)
					{
						offset += length;
					}
					else if (length < 0)
					{
						// The connection has been closed by the server, try to reconnect...
						closeConnectionToMetaServer();
						connectToMetaServer();
					}
				}
				catch (SocketTimeoutException)
				{
					// Ignore read timeout
				}
				catch (IOException e)
				{
					log.error("friendFinder", e);
				}

				if (offset > 0)
				{
					if (log.TraceEnabled)
					{
						log.trace(string.Format("Received from meta server: OPCODE {0:D}", buffer[0]));
					}

					int consumed = 0;
					SceNetAdhocctlPacketBaseS2C packet = packetFactory.createPacketS2C(this, buffer, offset);
					if (packet == null)
					{
						// Skip the unknown opcode
						consumed = 1;
					}
					else if (offset >= packet.Length)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Incoming server packet {0}", packet));
						}
						packet.process();
						consumed = packet.Length;
					}

					if (consumed > 0)
					{
						Array.Copy(buffer, consumed, buffer, 0, offset - consumed);
						offset -= consumed;
					}
				}
			}

			if (log.DebugEnabled)
			{
				log.debug("Exiting friendFinder");
			}

			// Be clean, send a disconnect message to the server
			try
			{
				sendToMetaServer(new PacketFactory.SceNetAdhocctlDisconnectPacketC2S(this));
			}
			catch (IOException)
			{
				// Ignore error
			}

			closeConnectionToMetaServer();
			exit_Renamed = false;

			friendFinderActive = false;
		}

		public static string convertIpToString(int ip)
		{
			return string.Format("{0:D}.{1:D}.{2:D}.{3:D}", ip & 0xFF, (ip >> 8) & 0xFF, (ip >> 16) & 0xFF, (ip >> 24) & 0xFF);
		}

		protected internal virtual void addFriend(string nickName, pspNetMacAddress mac, int ip)
		{
			lock (this)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Adding friend nickName='{0}', mac={1}, ip={2}", nickName, mac, convertIpToString(ip)));
				}
        
				Modules.sceNetAdhocctlModule.hleNetAdhocctlAddPeer(nickName, mac);
				if (chatGUI != null)
				{
					chatGUI.updateMembers(Modules.sceNetAdhocctlModule.PeersNickName);
				}
        
				bool found = false;
				foreach (MacIp macIp in macIps)
				{
					if (mac.Equals(macIp.mac))
					{
						macIp.Ip = ip;
						found = true;
						break;
					}
				}
        
				if (!found)
				{
					MacIp macIp = new MacIp(mac.macAddress, ip);
					macIps.Add(macIp);
        
					portManager.addHost(convertIpToString(ip));
				}
			}
		}

		protected internal virtual void deleteFriend(int ip)
		{
			lock (this)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Deleting friend ip={0}", convertIpToString(ip)));
				}
        
				foreach (MacIp macIp in macIps)
				{
					if (macIp.ip == ip)
					{
						// Delete the MacIp mapping
						macIps.Remove(macIp);
						// Delete the peer
						Modules.sceNetAdhocctlModule.hleNetAdhocctlDeletePeer(macIp.mac);
						// Delete the router ports
						portManager.removeHost(convertIpToString(ip));
						// Delete the nickName from the Chat members
						if (chatGUI != null)
						{
							chatGUI.updateMembers(Modules.sceNetAdhocctlModule.PeersNickName);
						}
						break;
					}
				}
			}
		}

		protected internal virtual void deleteAllFriends()
		{
			lock (this)
			{
				while (macIps.Count > 0)
				{
					MacIp macIp = macIps[0];
					deleteFriend(macIp.ip);
				}
			}
		}

		public virtual bool isBroadcast(SocketAddress socketAddress)
		{
			if (socketAddress is InetSocketAddress)
			{
				InetSocketAddress inetSocketAddress = (InetSocketAddress) socketAddress;
				return inetSocketAddress.Address.Equals(broadcastInetAddress);
			}

			return false;
		}

		public virtual int getBroadcastPort(SocketAddress socketAddress)
		{
			if (socketAddress is InetSocketAddress)
			{
				InetSocketAddress inetSocketAddress = (InetSocketAddress) socketAddress;
				if (inetSocketAddress.Address.Equals(broadcastInetAddress))
				{
					return inetSocketAddress.Port;
				}
			}

			return -1;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public java.net.SocketAddress getSocketAddress(byte[] macAddress, int realPort) throws java.net.UnknownHostException
		public override SocketAddress getSocketAddress(sbyte[] macAddress, int realPort)
		{
			InetAddress inetAddress = getInetAddress(macAddress);
			if (inetAddress == null && sceNetAdhoc.isMyMacAddress(macAddress))
			{
				inetAddress = localHostInetAddress;
			}
			if (inetAddress == null)
			{
				throw new UnknownHostException(string.Format("ProOnline: unknown MAC address {0}", sceNet.convertMacAddressToString(macAddress)));
			}

			return new InetSocketAddress(inetAddress, realPort);
		}

		public virtual int NumberMacIps
		{
			get
			{
				lock (this)
				{
					return macIps.Count;
				}
			}
		}

		public virtual MacIp getMacIp(int index)
		{
			lock (this)
			{
				if (index < 0 || index >= macIps.Count)
				{
					return null;
				}
				return macIps[index];
			}
		}

		public virtual InetAddress getInetAddress(sbyte[] macAddress)
		{
			if (sceNetAdhoc.isAnyMacAddress(macAddress))
			{
				return broadcastInetAddress;
			}

			MacIp macIp = getMacIp(macAddress);
			if (macIp == null)
			{
				return null;
			}

			return macIp.inetAddress;
		}

		public virtual int getIp(sbyte[] macAddress)
		{
			MacIp macIp = getMacIp(macAddress);
			if (macIp == null)
			{
				return 0;
			}

			return macIp.ip;
		}

		public virtual MacIp getMacIp(sbyte[] macAddress)
		{
			lock (this)
			{
				foreach (MacIp macIp in macIps)
				{
					if (isSameMacAddress(macAddress, macIp.mac))
					{
						return macIp;
					}
				}
        
				return null;
			}
		}

		private bool isLocalInetAddress(InetAddress inetAddress)
		{
			return inetAddress.Equals(loopbackInetAddress) || inetAddress.Equals(localHostInetAddress);
		}

		public virtual MacIp getMacIp(InetAddress inetAddress)
		{
			lock (this)
			{
				foreach (MacIp macIp in macIps)
				{
					if (inetAddress.Equals(macIp.inetAddress))
					{
						return macIp;
					}
        
					// When using 2 instances of pspsharp on the local machine
					if (isLocalInetAddress(inetAddress) && isLocalInetAddress(macIp.inetAddress))
					{
						return macIp;
					}
				}
        
				return null;
			}
		}

		public virtual sbyte[] getMacAddress(InetAddress inetAddress)
		{
			MacIp macIp = getMacIp(inetAddress);
			if (macIp == null)
			{
				return null;
			}

			return macIp.mac;
		}

		public override PdpObject createPdpObject()
		{
			return new ProOnlinePdpObject(this);
		}

		public override PtpObject createPtpObject()
		{
			return new ProOnlinePtpObject(this);
		}

		public override AdhocMessage createAdhocPdpMessage(int address, int length, sbyte[] destMacAddress)
		{
			return new ProOnlineAdhocMessage(this, address, length, destMacAddress);
		}

		public override AdhocMessage createAdhocPdpMessage(sbyte[] message, int length)
		{
			return new ProOnlineAdhocMessage(this, message, length);
		}

		public override AdhocMessage createAdhocPtpMessage(int address, int length)
		{
			return new ProOnlineAdhocMessage(this, address, length);
		}

		public override AdhocMessage createAdhocPtpMessage(sbyte[] message, int length)
		{
			return new ProOnlineAdhocMessage(this, message, length);
		}

		public override AdhocMessage createAdhocGameModeMessage(sceNetAdhoc.GameModeArea gameModeArea)
		{
			log.error("Adhoc GameMode not supported by ProOnline");
			return null;
		}

		public override AdhocMessage createAdhocGameModeMessage(sbyte[] message, int length)
		{
			log.error("Adhoc GameMode not supported by ProOnline");
			return null;
		}

		public override MatchingObject createMatchingObject()
		{
			return new ProOnlineMatchingObject(this);
		}

		public override AdhocMatchingEventMessage createAdhocMatchingEventMessage(MatchingObject matchingObject, int @event)
		{
			return MatchingPacketFactory.createPacket(this, matchingObject, @event);
		}

		public override AdhocMatchingEventMessage createAdhocMatchingEventMessage(MatchingObject matchingObject, int @event, int data, int dataLength, sbyte[] macAddress)
		{
			return MatchingPacketFactory.createPacket(this, matchingObject, @event, data, dataLength, macAddress);
		}

		public override AdhocMatchingEventMessage createAdhocMatchingEventMessage(MatchingObject matchingObject, sbyte[] message, int length)
		{
			return MatchingPacketFactory.createPacket(this, matchingObject, message, length);
		}

		public virtual bool isForMe(AdhocMessage adhocMessage, int port, InetAddress address)
		{
			sbyte[] fromMacAddress = getMacAddress(address);
			if (fromMacAddress == null)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("not for me: port={0:D}, address={1}, message={2}", port, address, adhocMessage));
				}
				// Unknown source IP address, ignore the message
				return false;
			}

			// Copy the source MAC address from the source InetAddress
			adhocMessage.FromMacAddress = fromMacAddress;

			// There is no broadcasting, all messages are for me
			return true;
		}

		public override void sendChatMessage(string message)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Sending chat message '{0}'", message));
			}

			try
			{
				sendToMetaServer(new PacketFactory.SceNetAdhocctlChatPacketC2S(this, message));
			}
			catch (IOException e)
			{
				log.warn("Error while sending chat message", e);
			}
		}

		public virtual void displayChatMessage(string nickName, string message)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Displaying chat message from '{0}': '{1}'", nickName, message));
			}

			chatGUI.addChatMessage(nickName, message);
		}

		public static void init()
		{
			Settings.Instance.registerSettingsListener("ProOnline", "emu.enableProOnline", new EnabledSettingsListener());
			Settings.Instance.registerSettingsListener("ProOnline", "network.ProOnline.metaServer", new MetaServerSettingsListener());
			Settings.Instance.registerSettingsListener("ProOnline", "network.ProOnline.metaPort", new MetaServerPortSettingsListener());
		}

		public static void exit()
		{
			Settings.Instance.removeSettingsListener("ProOnline");

			if (!Enabled)
			{
				return;
			}

			INetworkAdapter networkAdapter = Modules.sceNetModule.NetworkAdapter;
			if (networkAdapter == null || !(networkAdapter is ProOnlineNetworkAdapter))
			{
				return;
			}

			ProOnlineNetworkAdapter proOnline = (ProOnlineNetworkAdapter) networkAdapter;
			proOnline.exit_Renamed = true;
			proOnline.terminatePortManager();
			proOnline.waitForFriendFinderToExit();
		}

		public override bool ConnectComplete
		{
			get
			{
				return connectComplete;
			}
			set
			{
				this.connectComplete = value;
			}
		}


		public override void updatePeers()
		{
			// Nothing to do
		}
	}

}