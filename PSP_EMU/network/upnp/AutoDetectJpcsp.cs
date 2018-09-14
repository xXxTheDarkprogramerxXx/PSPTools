using System;
using System.Text;
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
namespace pspsharp.network.upnp
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetApctl.getLocalHostIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.upnp.UPnP.discoveryPort;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.upnp.UPnP.discoveryTimeoutMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.upnp.UPnP.multicastIp;


	using Modules = pspsharp.HLE.Modules;
	using ProOnlineNetworkAdapter = pspsharp.network.proonline.ProOnlineNetworkAdapter;
	using ProOnlineServer = pspsharp.network.proonline.ProOnlineServer;

	using Logger = org.apache.log4j.Logger;

	public class AutoDetectJpcsp
	{
		protected internal static Logger log = Logger.getLogger("network");
		private static AutoDetectJpcsp instance = null;
		private ListenerThread listenerThread;
		private const string deviceName = "pspsharp";

		private class DiscoverThread : Thread
		{
			private readonly AutoDetectJpcsp outerInstance;

			public DiscoverThread(AutoDetectJpcsp outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void run()
			{
				outerInstance.discover();
			}
		}

		public static AutoDetectJpcsp Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new AutoDetectJpcsp();
				}
				return instance;
			}
		}

		private AutoDetectJpcsp()
		{
		}

		public virtual void discoverOtherJpcspInBackground()
		{
			DiscoverThread discoverThread = new DiscoverThread(this);
			discoverThread.Name = "Auto Detect pspsharp Discover Thread";
			discoverThread.Daemon = true;
			discoverThread.Start();
		}

		private void discover()
		{
			if (OtherJpcspAvailable)
			{
				log.debug(string.Format("Other pspsharp is running"));
			}
			else
			{
				startDaemon();

				if (ProOnlineNetworkAdapter.Enabled && ProOnlineNetworkAdapter.MetaServer.Equals("localhost", StringComparison.OrdinalIgnoreCase))
				{
					ProOnlineServer.Instance.start();
				}
			}
		}

		private bool OtherJpcspAvailable
		{
			get
			{
				bool found = false;
				try
				{
					DatagramSocket socket = new DatagramSocket();
					socket.SoTimeout = discoveryTimeoutMillis;
					socket.ReuseAddress = true;
					string discoveryRequest = string.Format("M-SEARCH * HTTP/1.1\r\nHOST: {0}:{1:D}\r\nST: {2}\r\n\r\n", multicastIp, discoveryPort, deviceName);
					DatagramPacket packet = new DatagramPacket(discoveryRequest.GetBytes(), discoveryRequest.Length, new InetSocketAddress(multicastIp, discoveryPort));
					socket.send(packet);
					sbyte[] response = new sbyte[1536];
					DatagramPacket responsePacket = new DatagramPacket(response, response.Length);
					socket.receive(responsePacket);
					if (responsePacket.Length > 0)
					{
						string reply = new string(responsePacket.Data, responsePacket.Offset, responsePacket.Length);
						log.debug(string.Format("Discovery {0}: {1}", deviceName, reply));
						Pattern p = Pattern.compile("^location: *(\\S+):(\\d+)$", Pattern.CASE_INSENSITIVE | Pattern.MULTILINE | Pattern.DOTALL);
						Matcher m = p.matcher(reply);
						if (m.find())
						{
							string address = m.group(1);
							int port = int.Parse(m.group(2));
							log.info(string.Format("Found {0} at location: address='{1}', port={2:D}", deviceName, address, port));
							if (address.Equals(LocalHostIP))
							{
								Modules.sceNetAdhocModule.NetClientPortShift = port;
								Modules.sceNetAdhocModule.NetServerPortShift = 0;
								found = true;
							}
						}
						else
						{
							log.error(string.Format("Could not parse discovery response for {0}: {1}", deviceName, reply));
						}
					}
					socket.close();
				}
				catch (SocketTimeoutException e)
				{
					log.debug(string.Format("Timeout while discovering pspsharp: {0}", e.Message));
				}
				catch (IOException e)
				{
					log.error("Discover pspsharp", e);
				}
    
				return found;
			}
		}

		public virtual void startDaemon()
		{
			listenerThread = new ListenerThread(this);
			listenerThread.Name = "AutoDetectJpcsp - ListenerThread";
			listenerThread.Daemon = true;
			listenerThread.Start();
		}

		public static void exit()
		{
			if (instance != null)
			{
				if (instance.listenerThread != null)
				{
					instance.listenerThread.exit();
					instance.listenerThread = null;
				}
				instance = null;
			}
		}

		private class ListenerThread : Thread
		{
			private readonly AutoDetectJpcsp outerInstance;

			public ListenerThread(AutoDetectJpcsp outerInstance)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal bool exit_Renamed = false;

			public override void run()
			{
				log.debug(string.Format("Starting AutoDetectJpcsp ListenerThread"));
				sbyte[] response = new sbyte[256];

				while (!exit_Renamed)
				{
					try
					{
						InetAddress listenAddress = InetAddress.getByName(multicastIp);
						MulticastSocket socket = new MulticastSocket(discoveryPort);
						socket.joinGroup(listenAddress);
						while (!exit_Renamed)
						{
							DatagramPacket packet = new DatagramPacket(response, response.Length);
							socket.receive(packet);
							processRequest(socket, new string(packet.Data, packet.Offset, packet.Length), packet.Address, packet.Port);
						}
						socket.close();
					}
					catch (IOException e)
					{
						log.error("ListenerThread", e);
						exit();
					}
				}
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void processRequest(java.net.MulticastSocket socket, String request, java.net.InetAddress address, int port) throws java.io.IOException
			internal virtual void processRequest(MulticastSocket socket, string request, InetAddress address, int port)
			{
				log.debug(string.Format("Received '{0}' from {1}:{2:D}", request, address, port));

				Pattern p = Pattern.compile("SEARCH +\\* +.*^ST: *" + deviceName + "$.*", Pattern.CASE_INSENSITIVE | Pattern.MULTILINE | Pattern.DOTALL);
				Matcher m = p.matcher(request);
				if (m.find())
				{
					StringBuilder response = new StringBuilder();
					int netServerPortShift = Modules.sceNetAdhocModule.getRealPortFromServerPort(0);
					if (netServerPortShift == 0)
					{
						// Set a default server port shift if none has been set.
						netServerPortShift = 100;
						Modules.sceNetAdhocModule.NetServerPortShift = netServerPortShift;
						Modules.sceNetAdhocModule.NetClientPortShift = 0;
					}
					response.Append(string.Format("Location: {0}:{1:D}", LocalHostIP, netServerPortShift));

					log.debug(string.Format("Sending response '{0}' to {1}:{2:D}", response, address, port));
					DatagramPacket packet = new DatagramPacket(response.ToString().GetBytes(), response.Length, address, port);
					socket.send(packet);
				}
			}

			public virtual void exit()
			{
				exit_Renamed = true;
			}
		}
	}

}