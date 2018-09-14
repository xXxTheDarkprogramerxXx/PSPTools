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

	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using SceNetAdhocctlConnectPacketS2C = pspsharp.network.proonline.PacketFactory.SceNetAdhocctlConnectPacketS2C;
	using SceNetAdhocctlDisconnectPacketS2C = pspsharp.network.proonline.PacketFactory.SceNetAdhocctlDisconnectPacketS2C;
	using SceNetAdhocctlPacketBaseC2S = pspsharp.network.proonline.PacketFactory.SceNetAdhocctlPacketBaseC2S;
	using SceNetAdhocctlPacketBaseS2C = pspsharp.network.proonline.PacketFactory.SceNetAdhocctlPacketBaseS2C;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	/*
	 * Ported from ProOnline aemu server
	 * https://code.google.com/p/aemu/source/browse/#hg%2Fpspnet_adhocctl_server
	 */
	public class ProOnlineServer
	{
		protected internal static Logger log = ProOnlineNetworkAdapter.log;
		private static ProOnlineServer instance;
		private ProOnlineServerThread serverThread;
		private const int port = 27312;
		private ServerSocket serverSocket;
		private IList<User> users;
		private PacketFactory packetFactory;
		private User currentUser;
		private IList<Game> games;

		public static ProOnlineServer Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new ProOnlineServer();
				}
    
				return instance;
			}
		}

		private ProOnlineServer()
		{
		}

		private class User
		{
			public Socket socket;
			public long lastReceiveTimestamp;
			public sbyte[] buffer = new sbyte[1000];
			public int bufferLength = 0;
			public pspNetMacAddress mac;
			public string nickName;
			public Game game;
			public Group group;
			public int ip;
			public string ipString;

			public virtual bool Timeout
			{
				get
				{
					bool isTimeout = DateTimeHelper.CurrentUnixTimeMillis() - lastReceiveTimestamp > 15000;
					if (isTimeout)
					{
						log.debug(string.Format("User timed out now={0:D}, lastReceiveTimestamp={1:D}", DateTimeHelper.CurrentUnixTimeMillis(), lastReceiveTimestamp));
					}
					return isTimeout;
				}
			}

			public override string ToString()
			{
				return string.Format("{0} (MAC: {1} - IP: {2})", nickName, mac, ipString);
			}
		}

		private class Game
		{
			public string name;
			public int playerCount;
			public IList<Group> groups;

			public Game(string name)
			{
				this.name = name;
				groups = new LinkedList<Group>();
			}
		}

		private class Group
		{
			public string name;
			public Game game;
			public IList<User> players;

			public Group(string name, Game game)
			{
				this.name = name;
				this.game = game;
				players = new LinkedList<User>();
				if (game != null)
				{
					game.groups.Add(this);
				}
			}
		}

		private class ProOnlineServerThread : Thread
		{
			private readonly ProOnlineServer outerInstance;

			public ProOnlineServerThread(ProOnlineServer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal bool exit_Renamed;

			public override void run()
			{
				log.debug(string.Format("Starting ProOnlineServerThread"));
				while (!exit_Renamed)
				{
					try
					{
						Socket socket = outerInstance.serverSocket.accept();
						socket.SoTimeout = 1;
						outerInstance.loginUserStream(socket);
					}
					catch (SocketTimeoutException)
					{
						// Ignore timeout
					}
					catch (IOException e)
					{
						log.debug("Accept server socket", e);
					}

					foreach (User user in outerInstance.users)
					{
						int length = 0;
						try
						{
							System.IO.Stream @is = user.socket.InputStream;
							length = @is.Read(user.buffer, user.bufferLength, user.buffer.Length - user.bufferLength);
						}
						catch (SocketTimeoutException)
						{
							// Ignore timeout
						}
						catch (IOException e)
						{
							log.debug("Receive user socket", e);
						}

						if (length > 0)
						{
							user.bufferLength += length;
							user.lastReceiveTimestamp = DateTimeHelper.CurrentUnixTimeMillis();
							outerInstance.processUserStream(user);
						}
						else if (length < 0 || user.Timeout)
						{
							outerInstance.logoutUser(user);
						}
					}

					Utilities.sleep(10);
				}
			}

			public virtual void exit()
			{
				exit_Renamed = true;
			}
		}

		public virtual void start()
		{
			packetFactory = new PacketFactory();

			try
			{
				serverSocket = new ServerSocket(port);
				serverSocket.SoTimeout = 1;
			}
			catch (IOException e)
			{
				log.error(string.Format("Server socket at port {0:D} not available: {1}", port, e));
				return;
			}

			users = new LinkedList<ProOnlineServer.User>();
			games = new LinkedList<ProOnlineServer.Game>();

			serverThread = new ProOnlineServerThread(this);
			serverThread.Name = "ProOnline Server Thread";
			serverThread.Daemon = true;
			serverThread.Start();
		}

		public virtual void exit()
		{
			if (serverThread != null)
			{
				serverThread.exit();
				serverThread = null;
			}

			if (serverSocket != null)
			{
				try
				{
					serverSocket.close();
				}
				catch (IOException e)
				{
					log.debug("Closing server socket", e);
				}
				serverSocket = null;
			}
		}

		private static int convertIp(sbyte[] bytes)
		{
			int ip = 0;

			for (int i = 0; i < bytes.Length; i++)
			{
				ip |= bytes[i] << (i * 8);
			}

			return ip;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendToUser(User user, pspsharp.network.proonline.PacketFactory.SceNetAdhocctlPacketBaseS2C packet) throws java.io.IOException
		private void sendToUser(User user, SceNetAdhocctlPacketBaseS2C packet)
		{
			System.IO.Stream os = user.socket.OutputStream;
			os.Write(packet.Bytes, 0, packet.Bytes.Length);
			os.Flush();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void loginUserStream(java.net.Socket socket) throws java.io.IOException
		private void loginUserStream(Socket socket)
		{
			string ip = socket.InetAddress.HostAddress;

			// Check for duplicated user
	//		for (User user : users) {
	//			if (user.ipString.equals(ip)) {
	//				// Duplicate user (same IP & same port)
	//				log.debug(String.format("Duplicate user IP: %s", ip));
	//				socket.close();
	//				return;
	//			}
	//		}

			User user = new User();
			user.ip = convertIp(socket.InetAddress.Address);
			user.ipString = ip;
			user.socket = socket;
			user.lastReceiveTimestamp = DateTimeHelper.CurrentUnixTimeMillis();
			users.Add(user);

			log.info(string.Format("New Connection from {0}", user.ipString));
		}

		private void logoutUser(User user)
		{
			if (user.group != null)
			{
				disconnectUser(user);
			}

			try
			{
				user.socket.close();
			}
			catch (IOException)
			{
				// Ignore exception
			}

			if (user.game != null)
			{
				log.info(string.Format("{0} stopped playing {1}.", user, user.game.name));

				user.game.playerCount--;

				// Empty game
				if (user.game.playerCount <= 0)
				{
					games.Remove(user.game);
				}

				user.game = null;
			}
			else
			{
				log.info(string.Format("Dropped Connection {0}.", user));
			}

			users.Remove(user);
		}

		private void processUserStream(User user)
		{
			if (user.bufferLength <= 0)
			{
				return;
			}

			int consumed = 0;
			SceNetAdhocctlPacketBaseC2S packet = packetFactory.createPacketC2S(null, this, user.buffer, user.bufferLength);
			if (packet == null)
			{
				// Skip the unknown code
				consumed = 1;
			}
			else if (user.bufferLength >= packet.Length)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Incoming client packet {0}", packet));
				}

				currentUser = user;
				packet.process();
				currentUser = null;

				consumed = packet.Length;
			}

			if (consumed >= user.bufferLength)
			{
				user.bufferLength = 0;
			}
			else
			{
				// Removed consumed bytes from the buffer
				user.bufferLength -= consumed;
				Array.Copy(user.buffer, consumed, user.buffer, 0, user.bufferLength);
			}
		}

		public virtual void processLogin(pspNetMacAddress mac, string nickName, string gameName)
		{
			if (gameName.matches("[A-Z0-9]{9}"))
			{
				currentUser.game = null;
				foreach (Game game in games)
				{
					if (game.name.Equals(gameName))
					{
						currentUser.game = game;
						break;
					}
				}

				if (currentUser.game == null)
				{
					currentUser.game = new Game(gameName);
					games.Add(currentUser.game);
				}

				currentUser.game.playerCount++;
				currentUser.mac = mac;
				currentUser.nickName = nickName;

				log.info(string.Format("{0} started playing {1}.", currentUser, currentUser.game.name));
			}
			else
			{
				log.info(string.Format("Invalid login for game '{0}'", gameName));
			}
		}

		private void disconnectUser(User user)
		{
			if (user.group != null)
			{
				Group group = user.group;
				group.players.Remove(user);

				foreach (User groupUser in group.players)
				{
					SceNetAdhocctlDisconnectPacketS2C packet = new SceNetAdhocctlDisconnectPacketS2C(user.ip);
					try
					{
						sendToUser(groupUser, packet);
					}
					catch (IOException e)
					{
						log.debug("disconnectUser", e);
					}
				}

				log.info(string.Format("{0} left {1} group {2}.", user, user.game.name, group.name));

				user.group = null;

				// Empty group
				if (group.players.Count == 0)
				{
					group.game.groups.Remove(group);
				}
			}
			else
			{
				log.info(string.Format("{0} attempted to leave {1} without joining one first.", user, user.game.name));
				logoutUser(user);
			}
		}

		public virtual void processDisconnect()
		{
			disconnectUser(currentUser);
		}

		public virtual void processScan()
		{
			// User is disconnected
			if (currentUser.group == null)
			{
				// Iterate game groups
				foreach (Group group in currentUser.game.groups)
				{
					pspNetMacAddress mac = new pspNetMacAddress();
					if (group.players.Count > 0)
					{
						// Founder of the group is the first player
						mac = group.players[0].mac;
					}
					try
					{
						sendToUser(currentUser, new PacketFactory.SceNetAdhocctlScanPacketS2C(group.name, mac));
					}
					catch (IOException e)
					{
						log.debug("processScan", e);
					}
				}
			}
			else
			{
				log.info(string.Format("{0} attempted to scan for {1} groups without disconnecting from {2} first.", currentUser, currentUser.game.name, currentUser.group.name));
				logoutUser(currentUser);
			}
		}

		private void spreadMessage(User fromUser, string message)
		{
			// Global notice
			if (fromUser == null)
			{
				// Iterate players
				foreach (User user in users)
				{
					// User has access to chat
					if (user.group != null)
					{
						try
						{
							sendToUser(user, new PacketFactory.SceNetAdhocctlChatPacketS2C(message, ""));
						}
						catch (IOException e)
						{
							log.debug("spreadMessage global notice", e);
						}
					}
				}
			}
			else if (fromUser.group != null)
			{
				// User is connected
				int messageCount = 0;
				foreach (User user in fromUser.group.players)
				{
					// Skip self
					if (user != fromUser)
					{
						try
						{
							sendToUser(user, new PacketFactory.SceNetAdhocctlChatPacketS2C(message, fromUser.nickName));
							messageCount++;
						}
						catch (IOException e)
						{
							log.debug("spreadMessage", e);
						}
					}
				}

				if (messageCount > 0)
				{
					log.info(string.Format("{0} sent '{1}' to {2:D} players in {3} group {4}", fromUser, message, messageCount, fromUser.game.name, fromUser.group.name));
				}
			}
			else
			{
				// User is disconnected
				log.info(string.Format("{0} attempted to send a text message without joining a {1} group first", fromUser, fromUser.game.name));
			}
		}

		public virtual void processChat(string message)
		{
			spreadMessage(currentUser, message);
		}

		public virtual void processConnect(string groupName)
		{
			if (groupName.matches("[A-Za-z0-9]*"))
			{
				// User is disconnected
				if (currentUser.group == null)
				{
					foreach (Group group in currentUser.game.groups)
					{
						if (group.name.Equals(groupName))
						{
							currentUser.group = group;
							break;
						}
					}

					// New group
					if (currentUser.group == null)
					{
						currentUser.group = new Group(groupName, currentUser.game);
					}

					foreach (User user in currentUser.group.players)
					{
						SceNetAdhocctlConnectPacketS2C packet = new SceNetAdhocctlConnectPacketS2C(currentUser.nickName, currentUser.mac, currentUser.ip);
						try
						{
							sendToUser(user, packet);
						}
						catch (IOException e)
						{
							log.debug("processConnect", e);
						}

						packet = new SceNetAdhocctlConnectPacketS2C(user.nickName, user.mac, user.ip);
						try
						{
							sendToUser(currentUser, packet);
						}
						catch (IOException e)
						{
							log.debug("processConnect", e);
						}
					}

					currentUser.group.players.Add(currentUser);

					try
					{
						sendToUser(currentUser, new PacketFactory.SceNetAdhocctlConnectBSSIDPacketS2C(currentUser.group.players[0].mac));
					}
					catch (IOException e)
					{
						log.debug("processConnect", e);
					}
					log.info(string.Format("{0} joined {1} group '{2}'.", currentUser, currentUser.game == null ? "" : currentUser.game.name, currentUser.group.name));
				}
				else
				{
					// Already connected to another group
					log.info(string.Format("{0} attempted to join {1} group '{2}' without disconnecting from {3} first.", currentUser, currentUser.game == null ? "" : currentUser.game.name, groupName, currentUser.group.name));
					logoutUser(currentUser);
				}
			}
			else
			{
				log.info(string.Format("{0} attempted to join invalid {1} group '{2}'.", currentUser, currentUser.game == null ? "" : currentUser.game.name, groupName));
				logoutUser(currentUser);
			}
		}
	}

}