using System;
using System.Collections.Generic;
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


	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;
	using Document = org.w3c.dom.Document;
	using Element = org.w3c.dom.Element;
	using Node = org.w3c.dom.Node;
	using NodeList = org.w3c.dom.NodeList;
	using SAXException = org.xml.sax.SAXException;

	public class UPnP
	{
		public static Logger log = Logger.getLogger("upnp");
		protected internal IGD igd;
		public const int discoveryTimeoutMillis = 2000;
		public const int discoveryPort = 1900;
		public const int discoverySearchPort = 1901;
		public const string multicastIp = "239.255.255.250";
		private static readonly string[] deviceList = new string[] {"urn:schemas-upnp-org:device:InternetGatewayDevice:1", "urn:schemas-upnp-org:service:WANIPConnection:1", "urn:schemas-upnp-org:service:WANPPPConnection:1", "upnp:rootdevice"};

		private class DiscoverThread : Thread
		{
			private readonly UPnP outerInstance;

			public DiscoverThread(UPnP outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void run()
			{
				outerInstance.discover();
			}
		}

		public virtual void discoverInBackground()
		{
			DiscoverThread discoverThread = new DiscoverThread(this);
			discoverThread.Name = "UPnP Discover Thread";
			discoverThread.Daemon = true;
			discoverThread.Start();
		}

		private class ListenerThread : Thread
		{
			internal UPnP upnp;
			internal IGD igd;
			internal bool done;
			internal volatile bool ready;

			public ListenerThread(UPnP upnp, IGD igd)
			{
				this.upnp = upnp;
				this.igd = igd;
			}

			public override void run()
			{
				MulticastSocket[] sockets = new MulticastSocket[100];
				int numberSockets = 0;
				try
				{
					IEnumerator<NetworkInterface> networkInterfaces = NetworkInterface.NetworkInterfaces;
					while (networkInterfaces.MoveNext() && numberSockets < sockets.Length)
					{
						NetworkInterface networkInterface = networkInterfaces.Current;
						if (networkInterface.Up && networkInterface.supportsMulticast() && !networkInterface.Loopback)
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							for (IEnumerator<InetAddress> addresses = networkInterface.InetAddresses; addresses.hasMoreElements() && numberSockets < sockets.Length;)
							{
								InetAddress address = addresses.Current;
								if (address is Inet4Address && !address.LoopbackAddress)
								{
									sockets[numberSockets] = new MulticastSocket(new InetSocketAddress(address, discoverySearchPort));
									sockets[numberSockets].SoTimeout = 1;
									numberSockets++;
								}
							}
						}
					}
				}
				catch (SocketException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}

				ISet<string> processedUrls = new HashSet<string>();
				ready = true;
				sbyte[] buffer = new sbyte[1536];
				while (!Done)
				{
					for (int i = 0; i < numberSockets && !Done; i++)
					{
						try
						{
							DatagramPacket responsePacket = new DatagramPacket(buffer, buffer.Length);
							sockets[i].receive(responsePacket);
							if (responsePacket.Length > 0)
							{
								string reply = new string(responsePacket.Data, responsePacket.Offset, responsePacket.Length);
								if (log.DebugEnabled)
								{
									log.debug(string.Format("Discovery: {0}", reply));
								}

								string location = null;
								Pattern pLocation = Pattern.compile("^location: *(\\S+)$", Pattern.CASE_INSENSITIVE | Pattern.MULTILINE | Pattern.DOTALL);
								Matcher mLocation = pLocation.matcher(reply);
								if (mLocation.find())
								{
									location = mLocation.group(1);
								}

								string st = null;
								Pattern pSt = Pattern.compile("^st: *(\\S+)$", Pattern.CASE_INSENSITIVE | Pattern.MULTILINE | Pattern.DOTALL);
								Matcher mSt = pSt.matcher(reply);
								if (mSt.find())
								{
									st = mSt.group(1);
								}

								if (!string.ReferenceEquals(location, null) && !string.ReferenceEquals(st, null))
								{
									if (log.DebugEnabled)
									{
										log.debug(string.Format("Location: '{0}', st: '{1}'", location, st));
									}

									if (!processedUrls.Contains(location))
									{
										igd.discover(location);
										processedUrls.Add(location);
										if (igd.Valid && igd.isConnected(upnp))
										{
											if (log.DebugEnabled)
											{
												log.debug(string.Format("IGD connected with external IP: {0}", igd.getExternalIPAddress(upnp)));
											}
											Done = true;
										}
									}
								}
								else
								{
									log.error(string.Format("Could not parse discovery response: {0}", reply));
								}
							}
						}
						catch (SocketTimeoutException)
						{
						}
						catch (IOException)
						{
						}
					}
				}

				for (int i = 0; i < numberSockets; i++)
				{
					sockets[i].disconnect();
					sockets[i].close();
				}
				ready = true;
			}

			public virtual bool Done
			{
				get
				{
					return done;
				}
				set
				{
					this.done = value;
					ready = false;
				}
			}


			public virtual bool Ready
			{
				get
				{
					return ready;
				}
			}
		}

		public virtual void discover()
		{
			try
			{
				igd = new IGD();
				ListenerThread listener = new ListenerThread(this, igd);
				listener.Daemon = true;
				listener.Name = "UPnP Discovery Listener";
				listener.Start();
				while (!listener.Ready)
				{
					Utilities.sleep(100);
				}

				foreach (string device in deviceList)
				{
					string discoveryRequest = string.Format("M-SEARCH * HTTP/1.1\r\nHOST: {0}:{1:D}\r\nST: {2}\r\nMAN: \"ssdp:discover\"\r\nMX: {3:D}\r\n\r\n", multicastIp, discoveryPort, device, discoveryTimeoutMillis / 1000);
					IEnumerator<NetworkInterface> networkInterfaces = NetworkInterface.NetworkInterfaces;
					while (networkInterfaces.MoveNext())
					{
						NetworkInterface networkInterface = networkInterfaces.Current;
						if (networkInterface.Up && networkInterface.supportsMulticast())
						{
							for (IEnumerator<InetAddress> addresses = networkInterface.InetAddresses; addresses.MoveNext();)
							{
								InetAddress address = addresses.Current;
								if (address is Inet4Address && !address.LoopbackAddress)
								{
									MulticastSocket socket = new MulticastSocket(new InetSocketAddress(address, discoverySearchPort));
									InetSocketAddress socketAddress = new InetSocketAddress(multicastIp, discoveryPort);
									DatagramPacket packet = new DatagramPacket(discoveryRequest.GetBytes(), discoveryRequest.Length, socketAddress);
									socket.send(packet);
									socket.disconnect();
									socket.close();
								}
							}
						}
					}
				}

				for (int i = 0; i < discoveryTimeoutMillis / 10; i++)
				{
					if (listener.Done)
					{
						break;
					}
					Utilities.sleep(10, 0);
				}

				listener.Done = true;
				while (!listener.Ready)
				{
					Utilities.sleep(100);
				}

			}
			catch (IOException e)
			{
				log.error("discover", e);
			}
		}

		public virtual IGD IGD
		{
			get
			{
				return igd;
			}
		}

		protected internal virtual Dictionary<string, string> executeSimpleUPnPcommand(string controlUrl, string serviceType, string action, Dictionary<string, string> arguments)
		{
			Dictionary<string, string> result = null;

			StringBuilder body = new StringBuilder();

			body.Append(string.Format("<?xml version=\"1.0\"?>\r\n"));
			body.Append(string.Format("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n"));
			body.Append(string.Format("<s:Body>\r\n"));
			body.Append(string.Format("  <u:{0} xmlns:u=\"{1}\">\r\n", action, serviceType));
			if (arguments != null)
			{
				foreach (string name in arguments.Keys)
				{
					string value = arguments[name];
					if (string.ReferenceEquals(value, null) || value.Length == 0)
					{
						body.Append(string.Format("    <{0} />\r\n", name));
					}
					else
					{
						body.Append(string.Format("    <{0}>{1}</{2}>\r\n", name, value, name));
					}
				}
			}
			body.Append(string.Format("  </u:{0}>\r\n", action));
			body.Append(string.Format("</s:Body>\r\n"));
			body.Append(string.Format("</s:Envelope>\r\n"));

			if (log.TraceEnabled)
			{
				log.trace(string.Format("Sending UPnP command: {0}", body.ToString()));
			}

			sbyte[] bodyBytes = body.ToString().GetBytes();

			try
			{
				URL url = new URL(controlUrl);
				URLConnection connection = url.openConnection();
				if (connection is HttpURLConnection)
				{
					HttpURLConnection httpURLConnection = (HttpURLConnection) connection;
					httpURLConnection.RequestMethod = "POST";
				}
				connection.setRequestProperty("SOAPAction", string.Format("{0}#{1}", serviceType, action));
				connection.setRequestProperty("Content-Type", "text/xml");
				connection.setRequestProperty("Content-Length", Convert.ToString(bodyBytes.Length));
				connection.DoOutput = true;
				System.IO.Stream output = connection.OutputStream;
				output.Write(bodyBytes, 0, bodyBytes.Length);
				output.Flush();
				output.Close();

				connection.connect();

				System.IO.Stream response = connection.InputStream;
				StringBuilder content = new StringBuilder();
				sbyte[] buffer = new sbyte[1024];
				int n;
				do
				{
					n = response.Read(buffer, 0, buffer.Length);
					if (n > 0)
					{
						content.Append(StringHelper.NewString(buffer, 0, n));
					}
				} while (n >= 0);
				response.Close();

				if (log.DebugEnabled)
				{
					log.debug(string.Format("UPnP command serviceType {0}, action {1}, result: {2}", serviceType, action, content.ToString()));
				}

				result = parseSimpleCommandResponse(content.ToString());

				if (log.DebugEnabled)
				{
					string errorCode = result["errorCode"];
					if (!string.ReferenceEquals(errorCode, null))
					{
						log.debug(string.Format("UPnP command {0}: errorCode = {1}", action, errorCode));
					}
				}
			}
			catch (MalformedURLException e)
			{
				log.error("executeUPnPcommand", e);
			}
			catch (IOException e)
			{
				if (log.DebugEnabled)
				{
					log.debug("executeUPnPcommand", e);
				}
			}

			return result;
		}

		protected internal virtual Dictionary<string, string> parseSimpleCommandResponse(string content)
		{
			Dictionary<string, string> result = null;

			DocumentBuilderFactory documentBuilderFactory = DocumentBuilderFactory.newInstance();
			documentBuilderFactory.IgnoringElementContentWhitespace = true;
			documentBuilderFactory.IgnoringComments = true;
			documentBuilderFactory.Coalescing = true;

			try
			{
				DocumentBuilder documentBuilder = documentBuilderFactory.newDocumentBuilder();
				Document response = documentBuilder.parse(new System.IO.MemoryStream(content.GetBytes()));
				result = new Dictionary<string, string>();
				parseElement(response.DocumentElement, result, null);
			}
			catch (ParserConfigurationException e)
			{
				log.error("Discovery", e);
			}
			catch (SAXException e)
			{
				log.error("Discovery", e);
			}
			catch (MalformedURLException e)
			{
				log.error("Discovery", e);
			}
			catch (IOException e)
			{
				log.error("Discovery", e);
			}

			return result;
		}

		protected internal virtual void parseElement(Element element, Dictionary<string, string> result, string name)
		{
			NodeList children = element.ChildNodes;
			for (int i = 0; i < children.Length; i++)
			{
				Node node = children.item(i);
				if (node is Element)
				{
					parseElement((Element) node, result, node.NodeName);
				}
				else if (!string.ReferenceEquals(name, null) && node.TextContent != null)
				{
					string value = node.TextContent;
					if (result.ContainsKey(name))
					{
						value = result[name] + value;
					}
					result[name] = value;
				}
			}
		}

		public virtual string getStatusInfo(string controlUrl, string serviceType)
		{
			Dictionary<string, string> result = executeSimpleUPnPcommand(controlUrl, serviceType, "GetStatusInfo", null);

			return result["NewConnectionStatus"];
		}

		public virtual string getExternalIPAddress(string controlUrl, string serviceType)
		{
			Dictionary<string, string> result = executeSimpleUPnPcommand(controlUrl, serviceType, "GetExternalIPAddress", null);
			if (result == null)
			{
				return null;
			}

			return result["NewExternalIPAddress"];
		}

		public virtual void addPortMapping(string controlUrl, string serviceType, string remoteHost, int externalPort, string protocol, int internalPort, string internalClient, string description, int leaseDuration)
		{
			Dictionary<string, string> arguments = new Dictionary<string, string>();

			arguments["NewRemoteHost"] = remoteHost;
			arguments["NewExternalPort"] = Convert.ToString(externalPort);
			arguments["NewProtocol"] = getProtocol(protocol);
			arguments["NewInternalPort"] = Convert.ToString(internalPort);
			arguments["NewInternalClient"] = internalClient;
			arguments["NewEnabled"] = "1";
			arguments["NewPortMappingDescription"] = !string.ReferenceEquals(description, null) ? description : string.Format("pspsharp-{0}", State.discId);
			arguments["NewLeaseDuration"] = Convert.ToString(leaseDuration);

			Dictionary<string, string> result = executeSimpleUPnPcommand(controlUrl, serviceType, "AddPortMapping", arguments);

			if (log.DebugEnabled && result != null)
			{
				log.debug(string.Format("addPortMapping errorCode={0}", result["errorCode"]));
			}
		}

		public virtual void deletePortMapping(string controlUrl, string serviceType, string remoteHost, int externalPort, string protocol)
		{
			Dictionary<string, string> arguments = new Dictionary<string, string>();

			arguments["NewRemoteHost"] = remoteHost;
			arguments["NewExternalPort"] = Convert.ToString(externalPort);
			arguments["NewProtocol"] = getProtocol(protocol);

			Dictionary<string, string> result = executeSimpleUPnPcommand(controlUrl, serviceType, "DeletePortMapping", arguments);

			if (log.DebugEnabled && result != null)
			{
				log.debug(string.Format("deletePortMapping errorCode={0}", result["errorCode"]));
			}
		}

		protected internal virtual string getProtocol(string protocol)
		{
			if (!string.ReferenceEquals(protocol, null))
			{
				protocol = protocol.ToUpper();
				if (!protocol.Equals("TCP") && !protocol.Equals("UDP"))
				{
					// Unknown protocol
					protocol = null;
				}
			}

			return protocol;
		}
	}

}