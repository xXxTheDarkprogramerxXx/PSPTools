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
namespace pspsharp.remote
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.RuntimeContext.setLog4jMDC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNpAuth.STATUS_ACCOUNT_PARENTAL_CONTROL_ENABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNpAuth.addTicketDateParam;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNpAuth.addTicketLongParam;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNpAuth.addTicketParam;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.filesystems.umdiso.UmdIsoFile.sectorLength;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.getDefaultPortForProtocol;



	using keyCode = pspsharp.Controller.keyCode;
	using Modules = pspsharp.HLE.Modules;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceNpTicket = pspsharp.HLE.kernel.types.SceNpTicket;
	using TicketParam = pspsharp.HLE.kernel.types.SceNpTicket.TicketParam;
	using sceNp = pspsharp.HLE.modules.sceNp;
	using UmdIsoFile = pspsharp.filesystems.umdiso.UmdIsoFile;
	using UmdIsoReader = pspsharp.filesystems.umdiso.UmdIsoReader;
	using Elf32Header = pspsharp.format.Elf32Header;
	using HttpServerConfiguration = pspsharp.remote.HTTPConfiguration.HttpServerConfiguration;
	using Settings = pspsharp.settings.Settings;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class HTTPServer
	{
		private static Logger log = Logger.getLogger("http");
		private static HTTPServer instance;
		private static readonly HTTPServerDescriptor[] serverDescriptors = new HTTPServerDescriptor[]
		{
			new HTTPServerDescriptor(0, 80, false),
			new HTTPServerDescriptor(1, 443, true)
		};
		public static bool processProxyRequestLocally = false;
		private const string method = "method";
		private const string path = "path";
		private const string host = "host";
		private const string parameters = "parameters";
		private const string version = "version";
		private const string data = "data";
		private const string contentLength = "content-length";
		private const string eol = "\r\n";
		private const string boundary = "--boundarybetweensingleimages";
		private const string isoDirectory = "/iso/";
		private const string iconDirectory = "/icon/";
		private const string rootDirectory = "root";
		private const string widgetDirectory = "Widget";
		private static readonly string widgetPath = rootDirectory + "/" + widgetDirectory;
		private const string indexFile = "index.html";
		private const string naclDirectory = "nacl";
		private const string widgetlistFile = "/widgetlist.xml";
		private HTTPServerThread[] serverThreads;
		private Robot captureRobot;
		private UmdIsoReader previousUmdIsoReader;
		private string previousIsoFilename;
		private Dictionary<int, keyCode> keyMapping;
		private int runMapping = -1;
		private int pauseMapping = -1;
		private int resetMapping = -1;
		private const int MAX_COMPRESSED_COUNT = 0x7F;
		private DisplayAction displayAction;
		private int displayActionUsageCount = 0;
		private BufferedImage currentDisplayImage;
		private bool currentDisplayImageHasAlpha = false;
		private Proxy proxy;
		private int proxyPort;
		private int proxyAddress;
		private SceNpTicket ticket;

		public static HTTPServer Instance
		{
			get
			{
				if (instance == null)
				{
					Utilities.disableSslCertificateChecks();
					instance = new HTTPServer();
				}
				return instance;
			}
		}

		private class HTTPServerDescriptor
		{
			internal int index;
			internal int port;
			internal bool ssl;

			public HTTPServerDescriptor(int index, int port, bool ssl)
			{
				this.index = index;
				this.port = port;
				this.ssl = ssl;
			}

			public virtual int Index
			{
				get
				{
					return index;
				}
			}

			public virtual int Port
			{
				get
				{
					return port;
				}
			}

			public virtual bool Ssl
			{
				get
				{
					return ssl;
				}
			}
		}

		private class HTTPServerThread : Thread
		{
			private readonly HTTPServer outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal bool exit_Renamed;
			internal ServerSocket serverSocket;
			internal HTTPServerDescriptor descriptor;

			public HTTPServerThread(HTTPServer outerInstance, HTTPServerDescriptor descriptor)
			{
				this.outerInstance = outerInstance;
				this.descriptor = descriptor;
			}

			public override void run()
			{
				setLog4jMDC();
				try
				{
					if (descriptor.Ssl)
					{
						SSLServerSocketFactory factory = SSLServerSocketFactory;
						if (factory != null)
						{
							serverSocket = factory.createServerSocket(descriptor.Port);
						}
					}
					else
					{
						serverSocket = new ServerSocket(descriptor.Port);
					}
					if (serverSocket != null)
					{
						serverSocket.SoTimeout = 1;
					}
				}
				catch (IOException e)
				{
					log.error(string.Format("Server socket at port {0:D} not available: {1}", descriptor.Port, e));
				}
				catch (KeyStoreException e)
				{
					log.error(string.Format("SSL Server socket at port {0:D} not available: {1}", descriptor.Port, e));
				}
				catch (NoSuchAlgorithmException e)
				{
					log.error(string.Format("SSL Server socket at port {0:D} not available: {1}", descriptor.Port, e));
				}
				catch (CertificateException e)
				{
					log.error(string.Format("SSL Server socket at port {0:D} not available: {1}", descriptor.Port, e));
				}
				catch (UnrecoverableKeyException e)
				{
					log.error(string.Format("SSL Server socket at port {0:D} not available: {1}", descriptor.Port, e));
				}
				catch (KeyManagementException e)
				{
					log.error(string.Format("SSL Server socket at port {0:D} not available: {1}", descriptor.Port, e));
				}

				if (serverSocket == null)
				{
					exit();
				}

				while (!exit_Renamed)
				{
					try
					{
						Socket socket = serverSocket.accept();
						socket.SoTimeout = 1;
						HTTPSocketHandlerThread handlerThread = new HTTPSocketHandlerThread(outerInstance, descriptor, socket);
						handlerThread.Name = string.Format("HTTP Handler {0:D}/{1:D}", descriptor.Port, socket.Port);
						handlerThread.Daemon = true;
						handlerThread.Start();
					}
					catch (SocketTimeoutException)
					{
						// Ignore timeout
					}
					catch (IOException e)
					{
						log.debug("Accept server socket", e);
					}

					Utilities.sleep(10);
				}

				if (serverSocket != null)
				{
					try
					{
						serverSocket.close();
					}
					catch (IOException)
					{
						// Ignore exception
					}
				}

				outerInstance.serverThreads[descriptor.Index] = null;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private javax.net.ssl.SSLServerSocketFactory getSSLServerSocketFactory() throws java.security.KeyStoreException, java.security.NoSuchAlgorithmException, java.security.cert.CertificateException, java.io.IOException, java.security.UnrecoverableKeyException, java.security.KeyManagementException
			internal virtual SSLServerSocketFactory SSLServerSocketFactory
			{
				get
				{
					string jksFileName = "pspsharp.jks";
					if (!(new File(jksFileName)).canRead())
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("getSSLServerSocketFactory cannot read the file '{0}'", jksFileName));
						}
						return null;
					}
    
					char[] password = "changeit".ToCharArray();
					System.IO.FileStream keyStoreInputStream = new System.IO.FileStream(jksFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
    
					KeyStore keyStore = KeyStore.getInstance("JKS");
					keyStore.load(keyStoreInputStream, password);
    
					string defaultAlgorithm = KeyManagerFactory.DefaultAlgorithm;
					KeyManagerFactory keyManagerFactory = KeyManagerFactory.getInstance(defaultAlgorithm);
					keyManagerFactory.init(keyStore, password);
    
					SSLContext sslContext = SSLContext.getInstance("TLS");
					sslContext.init(keyManagerFactory.KeyManagers, null, null);
    
					return sslContext.ServerSocketFactory;
				}
			}

			public virtual void exit()
			{
				exit_Renamed = true;
			}
		}

		private class HTTPSocketHandlerThread : Thread
		{
			private readonly HTTPServer outerInstance;

			internal HTTPServerDescriptor descriptor;
			internal Socket socket;

			public HTTPSocketHandlerThread(HTTPServer outerInstance, HTTPServerDescriptor descriptor, Socket socket)
			{
				this.outerInstance = outerInstance;
				this.descriptor = descriptor;
				this.socket = socket;
			}

			public override void run()
			{
				setLog4jMDC();
				outerInstance.process(descriptor, socket);
			}
		}

		private class DisplayAction : IAction
		{
			private readonly HTTPServer outerInstance;

			public DisplayAction(HTTPServer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.currentDisplayImage = Modules.sceDisplayModule.getCurrentDisplayAsBufferedImage(false);
				outerInstance.currentDisplayImageHasAlpha = false;
			}
		}

		private HTTPServer()
		{
			keyMapping = new Dictionary<int, keyCode>();

			serverThreads = new HTTPServerThread[serverDescriptors.Length];
			foreach (HTTPServerDescriptor descriptor in serverDescriptors)
			{
				if (descriptor.Index == 0)
				{
					string addressName = "localhost";
					proxyPort = descriptor.Port;

					InetSocketAddress socketAddress = new InetSocketAddress(addressName, proxyPort);
					proxy = new Proxy(Proxy.Type.HTTP, socketAddress);

					sbyte[] addrBytes = socketAddress.Address.Address;
					proxyAddress = (addrBytes[0] & 0xFF) | ((addrBytes[1] & 0xFF) << 8) | ((addrBytes[2] & 0xFF) << 16) | ((addrBytes[3] & 0xFF) << 24);
				}
				HTTPServerThread serverThread = new HTTPServerThread(this, descriptor);
				serverThreads[descriptor.Index] = serverThread;
				serverThread.Daemon = true;
				serverThread.Name = "HTTP Server";
				serverThread.Start();
			}

			Authenticator.Default = new AuthenticatorAnonymousInnerClass(this);

			try
			{
				captureRobot = new Robot();
				captureRobot.AutoDelay = 0;
			}
			catch (AWTException e)
			{
				log.error("Create captureRobot", e);
			}
		}

		private class AuthenticatorAnonymousInnerClass : Authenticator
		{
			private readonly HTTPServer outerInstance;

			public AuthenticatorAnonymousInnerClass(HTTPServer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override PasswordAuthentication PasswordAuthentication
			{
				get
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("getPasswordAuthentication called for scheme='{0}', prompt='{1}'", RequestingScheme, RequestingPrompt));
					}
    
					if ("digest".Equals(RequestingScheme))
					{
						return new PasswordAuthentication("c7y-basic01", "A9QTbosh0W0D^{7467l-n_>2Y%JG^v>o".ToCharArray());
					}
					else if ("c7y-basic".Equals(RequestingPrompt))
					{
						// This is the PSP authentication, but it seems to no longer be accepted...
						char[] pwd = new char[] {(char) 0x35, (char) 0x03, (char) 0x0f, (char) 0x19, (char) 0x40, (char) 0x16, (char) 0x49, (char) 0x04, (char) 0x1c, (char) 0x35, (char) 0x03, (char) 0x1e, (char) 0x21, (char) 0x48, (char) 0x2d, (char) 0x4e, (char) 0x07, (char) 0x1c, (char) 0x5a, (char) 0x36, (char) 0x0e, (char) 0x3f, (char) 0x0c, (char) 0x18, (char) 0x49, (char) 0x15, (char) 0x4e, (char) 0x21, (char) 0x14, (char) 0x36, (char) 0x1d, (char) 0x16};
						return new PasswordAuthentication("c7y-basic02", pwd);
					}
					else if ("c7y-ranking".Equals(RequestingPrompt))
					{
						// This is the PSP authentication, but it seems to no longer be accepted...
						char[] pwd = new char[] {(char) 0x21, (char) 0x2D, (char) 0x18, (char) 0x1B, (char) 0x1D, (char) 0x0E, (char) 0x2A, (char) 0x23, (char) 0x04, (char) 0x4C, (char) 0x4B, (char) 0x19, (char) 0x4F, (char) 0x25, (char) 0x26, (char) 0x3F, (char) 0x4B, (char) 0x4D, (char) 0x4C, (char) 0x44, (char) 0x58, (char) 0x3C, (char) 0x31, (char) 0x4C, (char) 0x15, (char) 0x4C, (char) 0x5C, (char) 0x41, (char) 0x32, (char) 0x38, (char) 0x1E, (char) 0x08};
						return new PasswordAuthentication("c7y-ranking01", pwd);
					}
    
					return base.PasswordAuthentication;
				}
			}
		}

		private static string decodePath(string path)
		{
			StringBuilder decoded = new StringBuilder();

			for (int i = 0; i < path.Length; i++)
			{
				char c = path[i];
				if (c == '+')
				{
					decoded.Append(' ');
				}
				else if (c == '%')
				{
					int hex = Convert.ToInt32(path.Substring(i + 1, (i + 3) - (i + 1)), 16);
					i += 2;
					decoded.Append((char) hex);
				}
				else
				{
					decoded.Append(c);
				}
			}

			return decoded.ToString();
		}

		private void process(HTTPServerDescriptor descriptor, Socket socket)
		{
			System.IO.Stream @is = null;
			try
			{
				@is = socket.InputStream;
			}
			catch (IOException e)
			{
				log.error("process InputStream", e);
			}
			System.IO.Stream os = null;
			try
			{
				os = socket.OutputStream;
			}
			catch (IOException e)
			{
				log.error("process OutputStream", e);
			}

			sbyte[] buffer = new sbyte[10000];
			int bufferLength = 0;
			while (@is != null && os != null)
			{
				try
				{
					int length = @is.Read(buffer, bufferLength, buffer.Length - bufferLength);
					if (length < 0)
					{
						break;
					}
					if (length > 0)
					{
						bufferLength += length;
						string request = StringHelper.NewString(buffer, 0, bufferLength);
						Dictionary<string, string> requestHeaders = parseRequest(request);
						if (requestHeaders != null)
						{
							if (log.DebugEnabled)
							{
								log.debug(string.Format("Received request: '{0}', headers: {1}", request, requestHeaders));
							}
							bool keepAlive = process(descriptor, requestHeaders, os);
							os.Flush();

							if (keepAlive)
							{
								bufferLength = 0;
							}
							else
							{
								break;
							}
						}
					}
				}
				catch (SocketTimeoutException)
				{
					// Ignore timeout
				}
				catch (IOException e)
				{
					// Do not log the exception when the remote client has closed the connection
					if (!(e.InnerException is EOFException))
					{
						if (log.DebugEnabled)
						{
							log.debug("Receive socket", e);
						}
					}
					break;
				}
			}

			try
			{
				socket.close();
			}
			catch (IOException)
			{
				// Ignore exception
			}
		}

		private Dictionary<string, string> parseRequest(string request)
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			string[] lines = request.Split(eol, false); // Do not loose trailing empty strings
			bool header = true;

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				if (i == 0)
				{
					// Parse e.g. "GET / HTTP/1.1" into 3 words: "GET", "/" and "HTTP/1.1"
					string[] words = line.Split(" ", true);
					if (words.Length >= 1)
					{
						headers[method] = words[0];
					}
					if (words.Length >= 2)
					{
						string completePath = words[1];
						int parametersIndex = completePath.IndexOf("?", StringComparison.Ordinal);
						if (parametersIndex >= 0)
						{
							headers[path] = decodePath(completePath.Substring(0, parametersIndex));
							headers[parameters] = completePath.Substring(parametersIndex + 1);
						}
						else
						{
							headers[path] = decodePath(completePath);
						}
					}
					if (words.Length >= 3)
					{
						headers[version] = words[2];
					}
				}
				else if (header)
				{
					if (line.Length == 0)
					{
						// End of header
						header = false;
					}
					else
					{
						// Parse e.g. "Host: localhost:30005" into 2 words: "Host" and "localhost:30005"
						string[] words = line.Split(": *", 2);
						if (words.Length >= 2)
						{
							headers[words[0].ToLower()] = words[1];
						}
					}
				}
				else if (line.Length > 0)
				{
					string previousData = headers[data];
					if (!string.ReferenceEquals(previousData, null))
					{
						headers[data] = previousData + "\n" + line;
					}
					else
					{
						headers[data] = line;
					}
				}
			}

			if (header)
			{
				return null;
			}

			if (!string.ReferenceEquals(headers[contentLength], null))
			{
				int headerContentLength = int.Parse(headers[contentLength]);
				if (headerContentLength > 0)
				{
					string additionalData = headers[data];
					if (string.ReferenceEquals(additionalData, null) || additionalData.Length < headerContentLength)
					{
						return null;
					}
				}
			}

			return headers;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean doProxy(pspsharp.remote.HTTPConfiguration.HttpServerConfiguration httpServerConfiguration, HTTPServerDescriptor descriptor, java.util.HashMap<String, String> request, java.io.OutputStream os, String pathValue) throws java.io.IOException
		private bool doProxy(HttpServerConfiguration httpServerConfiguration, HTTPServerDescriptor descriptor, Dictionary<string, string> request, System.IO.Stream os, string pathValue)
		{
			int forcedPort = 0;
			if (httpServerConfiguration.serverPort != descriptor.port)
			{
				forcedPort = httpServerConfiguration.serverPort;
			}

			bool keepAlive = doProxy(descriptor, request, os, pathValue, forcedPort);
			if (!httpServerConfiguration.doKeepAlive)
			{
				keepAlive = false;
			}

			return keepAlive;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean doProxy(HTTPServerDescriptor descriptor, java.util.HashMap<String, String> request, java.io.OutputStream os, String pathValue, int forcedPort) throws java.io.IOException
		private bool doProxy(HTTPServerDescriptor descriptor, Dictionary<string, string> request, System.IO.Stream os, string pathValue, int forcedPort)
		{
			bool keepAlive = false;

			string remoteUrl = getUrl(descriptor, request, pathValue, forcedPort);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("doProxy connecting to '{0}'", remoteUrl));
			}

			HttpURLConnection connection = (HttpURLConnection) (new URL(remoteUrl)).openConnection();
			foreach (string key in request.Keys)
			{
				if (!data.Equals(key) && !method.Equals(key) && !version.Equals(key) && !path.Equals(key) && !parameters.Equals(key))
				{
					connection.setRequestProperty(key, request[key]);
				}
			}

			// Do not follow HTTP redirects
			connection.InstanceFollowRedirects = false;

			connection.RequestMethod = request[method];
			string additionalData = request[data];
			if (!string.ReferenceEquals(additionalData, null))
			{
				if ("/nav/auth".Equals(pathValue) && additionalData.Contains("&consoleid="))
				{
					// Remove the "consoleid" parameter as it is recognized as invalid.
					// The dummy value returned by sceOpenPSIDGetPSID is not valid.
					additionalData = additionalData.replaceAll("\\&consoleid=[0-9a-fA-F]*", "");
				}

				connection.DoOutput = true;
				System.IO.Stream dataStream = connection.OutputStream;
				dataStream.WriteByte(additionalData.GetBytes());
				dataStream.Close();
			}
			connection.connect();

			int dataLength = connection.ContentLength;

			sbyte[] buffer = new sbyte[100000];
			int length = 0;
			bool endOfInputReached = false;
			System.IO.Stream @in = null;
			try
			{
				@in = connection.InputStream;
				while (length < buffer.Length)
				{
					int l = @in.Read(buffer, length, buffer.Length - length);
					if (l < 0)
					{
						endOfInputReached = true;
						break;
					}
					length += l;
				}
			}
			catch (IOException e)
			{
				log.debug("doProxy", e);
			}

			string bufferString = StringHelper.NewString(buffer, 0, length);
			bool bufferPatched = false;
			if (bufferString.Contains("https://legaldoc.dl.playstation.net"))
			{
				bufferString = bufferString.Replace("https://legaldoc.dl.playstation.net", "http://legaldoc.dl.playstation.net");
				bufferPatched = true;
			}

			if (bufferPatched)
			{
				buffer = bufferString.GetBytes();
				length = buffer.Length;

				// Also update the "Content-Length" header if it was specified
				if (dataLength >= 0)
				{
					dataLength = length;
				}
			}

			sendHTTPResponseCode(os, connection.ResponseCode, connection.ResponseMessage);

			// Only send a "Content-Length" header if the remote server did send it
			if (dataLength >= 0)
			{
				sendResponseHeader(os, contentLength, dataLength);
			}

			foreach (KeyValuePair<string, IList<string>> entry in connection.HeaderFields.entrySet())
			{
				string key = entry.Key;
				if (!string.ReferenceEquals(key, null) && !"transfer-encoding".Equals(key.ToLower()))
				{
					foreach (string value in entry.Value)
					{
						// Ignore "Set-Cookie" with an empty value
						if ("Set-Cookie".Equals(key, StringComparison.OrdinalIgnoreCase) && value.Length == 0)
						{
							continue;
						}

						// If we changed "https" into "http", remove the information that the cookie can
						// only be sent over https, otherwise, it will be lost.
						if (forcedPort == 443 && "Set-Cookie".Equals(key, StringComparison.OrdinalIgnoreCase))
						{
							value = value.Replace("; Secure", "");
						}

						// If we changed "https" into "http", keep redirecting to the
						// http address instead of https.
						if (forcedPort == 443 && "Location".Equals(key, StringComparison.OrdinalIgnoreCase))
						{
							if (value.StartsWith("https:", StringComparison.Ordinal))
							{
								value = value.replaceFirst("https:", "http:");
							}
						}

						sendResponseHeader(os, key, value);

						if ("connection".Equals(key, StringComparison.OrdinalIgnoreCase) && "keep-alive".Equals(value, StringComparison.OrdinalIgnoreCase))
						{
							keepAlive = true;
						}
						if ("content-type".Equals(key, StringComparison.OrdinalIgnoreCase) && "application/x-i-5-ticket".Equals(value, StringComparison.OrdinalIgnoreCase) && length > 0)
						{
							ticket = new SceNpTicket();
							ticket.read(buffer, 0, length);
						}
					}
				}
			}
			sendEndOfHeaders(os);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("doProxy{0}:\n{1}", (bufferPatched ? " (response patched)" : ""), Utilities.getMemoryDump(buffer, 0, length)));
			}

			os.Write(buffer, 0, length);

			if (@in != null)
			{
				while (!endOfInputReached)
				{
					length = 0;
					try
					{
						while (length < buffer.Length)
						{
							int l = @in.Read(buffer, length, buffer.Length - length);
							if (l < 0)
							{
								endOfInputReached = true;
								break;
							}
							length += l;
						}
					}
					catch (IOException e)
					{
						log.debug("doProxy", e);
					}
					os.Write(buffer, 0, length);
				}
				@in.Close();
			}

			return keepAlive;
		}

		private HttpServerConfiguration getHttpServerConfiguration(string serverName, string pathValue)
		{
			if (!string.ReferenceEquals(serverName, null))
			{
				foreach (HttpServerConfiguration httpServerConfiguration in HTTPConfiguration.doProxyServers)
				{
					if (httpServerConfiguration.serverName.Equals(serverName))
					{
						bool found = true;
						if (httpServerConfiguration.fakedPaths != null)
						{
							foreach (string fakedPath in httpServerConfiguration.fakedPaths)
							{
								if (fakedPath.Equals(pathValue))
								{
									found = false;
									break;
								}
							}
						}

						if (found)
						{
							return httpServerConfiguration;
						}
					}
				}
			}

			return null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean process(HTTPServerDescriptor descriptor, java.util.HashMap<String, String> request, java.io.OutputStream os) throws java.io.IOException
		private bool process(HTTPServerDescriptor descriptor, Dictionary<string, string> request, System.IO.Stream os)
		{
			bool keepAlive = false;
			try
			{
				string pathValue = request[path];
				string baseUrl = getBaseUrl(descriptor, request, 0);
				if (pathValue.StartsWith(baseUrl, StringComparison.Ordinal))
				{
					pathValue = pathValue.Substring(baseUrl.Length - 1);
				}

				HttpServerConfiguration httpServerConfiguration = getHttpServerConfiguration(request[host], pathValue);

				if (httpServerConfiguration != null)
				{
					keepAlive = doProxy(httpServerConfiguration, descriptor, request, os, pathValue);
	//			} else if ("auth.np.ac.playstation.net".equals(request.get(host)) && "/nav/auth".equals(pathValue)) {
	//				sendNpNavAuth(request.get(data), os);
	//			} else if ("getprof.gb.np.community.playstation.net".equals(request.get(host)) && "/basic_view/sec/get_self_profile".equals(pathValue)) {
	//				sendNpGetSelfProfile(request.get(data), os);
				}
				else if ("commerce.np.ac.playstation.net".Equals(request[host]) && "/cap.m".Equals(pathValue))
				{
					sendCapM(request[data], os);
				}
				else if ("commerce.np.ac.playstation.net".Equals(request[host]) && "/kdp.m".Equals(pathValue))
				{
					sendKdpM(request[data], os);
				}
				else if ("video.dl.playstation.net".Equals(request[host]) && pathValue.matches("/cdn/video/[A-Z][A-Z]/g"))
				{
					sendVideoStore(os);
				}
				else if ("GET".Equals(request[method]))
				{
					if ("/".Equals(pathValue))
					{
						sendResponseFile(os, rootDirectory + "/" + indexFile);
					}
					else if ("/screen.png".Equals(pathValue))
					{
						sendScreenImage(os, "png");
					}
					else if ("/screen.jpg".Equals(pathValue))
					{
						sendScreenImage(os, "jpg");
					}
					else if ("/screen.mjpg".Equals(pathValue))
					{
						sendVideoMJPG(os);
					}
					else if ("/screen.raw".Equals(pathValue))
					{
						sendVideoRAW(os);
					}
					else if ("/screen.craw".Equals(pathValue))
					{
						sendVideoCompressedRAW(os);
					}
					else if ("/audio.wav".Equals(pathValue))
					{
						sendAudioWAV(os);
					}
					else if ("/audio.raw".Equals(pathValue))
					{
						sendAudioRAW(os);
					}
					else if ("/controls".Equals(pathValue))
					{
						processControls(os, request[parameters]);
					}
					else if (pathValue.StartsWith(iconDirectory, StringComparison.Ordinal))
					{
						sendIcon(os, pathValue);
					}
					else if (pathValue.StartsWith(isoDirectory, StringComparison.Ordinal))
					{
						sendIso(request, os, pathValue, true);
					}
					else if (widgetlistFile.Equals(pathValue))
					{
						sendWidgetlist(descriptor, request, os, pathValue);
					}
					else if (pathValue.StartsWith("/" + widgetDirectory + "/", StringComparison.Ordinal))
					{
						sendWidget(os, request[parameters], rootDirectory + pathValue);
					}
					else if (pathValue.StartsWith("/" + naclDirectory + "/", StringComparison.Ordinal))
					{
						sendNaClResponse(os, pathValue.Substring(6));
					}
					else if (pathValue.EndsWith(".html", StringComparison.Ordinal))
					{
						sendResponseFile(os, rootDirectory + pathValue);
					}
					else if (pathValue.EndsWith(".txt", StringComparison.Ordinal))
					{
						sendResponseFile(os, rootDirectory + pathValue);
					}
					else if (pathValue.EndsWith(".xml", StringComparison.Ordinal))
					{
						sendResponseFile(os, rootDirectory + pathValue);
					}
					else
					{
						sendErrorNotFound(os);
					}
				}
				else if ("HEAD".Equals(request[method]))
				{
					if (pathValue.StartsWith(isoDirectory, StringComparison.Ordinal))
					{
						sendIso(request, os, pathValue, false);
					}
					else
					{
						sendErrorNotFound(os);
					}
				}
				else
				{
					sendError(os, 405);
				}
			}
			catch (SocketException)
			{
				// Ignore exception (e.g. Connection reset by peer)
				keepAlive = false;
			}

			return keepAlive;
		}

		private static string guessMimeType(string fileName)
		{
			if (!string.ReferenceEquals(fileName, null))
			{
				if (fileName.EndsWith(".js", StringComparison.Ordinal))
				{
					return "application/javascript";
				}
				else if (fileName.EndsWith(".html", StringComparison.Ordinal))
				{
					return "text/html";
				}
				else if (fileName.EndsWith(".css", StringComparison.Ordinal))
				{
					return "text/css";
				}
				else if (fileName.EndsWith(".png", StringComparison.Ordinal))
				{
					return "image/png";
				}
				else if (fileName.EndsWith(".jpg", StringComparison.Ordinal) || fileName.EndsWith(".jpeg", StringComparison.Ordinal))
				{
					return "image/jpeg";
				}
				else if (fileName.EndsWith(".xml", StringComparison.Ordinal))
				{
					return "text/xml";
				}
				else if (fileName.EndsWith(".zip", StringComparison.Ordinal))
				{
					return "application/zip";
				}
			}

			return "application/octet-stream";
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendRedirect(java.io.OutputStream os, String redirect) throws java.io.IOException
		private void sendRedirect(System.IO.Stream os, string redirect)
		{
			sendHTTPResponseCode(os, 302);
			sendResponseHeader(os, "Location", redirect);
			sendEndOfHeaders(os);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendResponseFile(java.io.OutputStream os, String fileName) throws java.io.IOException
		private void sendResponseFile(System.IO.Stream os, string fileName)
		{
			sendResponseFile(os, this.GetType().getResourceAsStream(fileName), guessMimeType(fileName));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendResponseFile(java.io.OutputStream os, java.io.InputStream is, String contentType) throws java.io.IOException
		private void sendResponseFile(System.IO.Stream os, System.IO.Stream @is, string contentType)
		{
			sbyte[] buffer = new sbyte[1000];
			int contentLength = 0;
			if (@is != null)
			{
				while (true)
				{
					if (buffer.Length - contentLength < 1000)
					{
						buffer = Utilities.extendArray(buffer, 1000);
					}
					int length = @is.Read(buffer, contentLength, buffer.Length - contentLength);
					if (length < 0)
					{
						break;
					}
					contentLength += length;
				}
				@is.Close();
			}

			sendOK(os);
			if (!string.ReferenceEquals(contentType, null))
			{
				sendResponseHeader(os, "Content-Type", contentType);
			}
			if (contentLength > 0)
			{
				sendResponseHeader(os, "Content-Length", contentLength);
			}
			sendEndOfHeaders(os);

			if (contentLength > 0)
			{
				os.Write(buffer, 0, contentLength);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendResponseLine(java.io.OutputStream os, String line) throws java.io.IOException
		private void sendResponseLine(System.IO.Stream os, string line)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Response: {0}", line));
			}
			os.WriteByte(line.GetBytes());
			os.WriteByte(eol.GetBytes());
		}

		private static string guessHTTPResponseCodeMsg(int code)
		{
			switch (code)
			{
				case 200:
					return "OK";
				case 206:
					return "Partial Content";
				case 302:
					return "Found";
				case 404:
					return "Not Found";
				case 405:
					return "Method Not Allowed";
			}

			return "";
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendHTTPResponseCode(java.io.OutputStream os, int code, String msg) throws java.io.IOException
		private void sendHTTPResponseCode(System.IO.Stream os, int code, string msg)
		{
			sendResponseLine(os, string.Format("HTTP/1.1 {0:D} {1}", code, msg));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendHTTPResponseCode(java.io.OutputStream os, int code) throws java.io.IOException
		private void sendHTTPResponseCode(System.IO.Stream os, int code)
		{
			sendHTTPResponseCode(os, code, guessHTTPResponseCodeMsg(code));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendOK(java.io.OutputStream os) throws java.io.IOException
		private void sendOK(System.IO.Stream os)
		{
			sendHTTPResponseCode(os, 200);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendResponseHeader(java.io.OutputStream os, String name, String value) throws java.io.IOException
		private void sendResponseHeader(System.IO.Stream os, string name, string value)
		{
			sendResponseLine(os, string.Format("{0}: {1}", name, value));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendResponseHeader(java.io.OutputStream os, String name, int value) throws java.io.IOException
		private void sendResponseHeader(System.IO.Stream os, string name, int value)
		{
			sendResponseHeader(os, name, value.ToString());
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendResponseHeader(java.io.OutputStream os, String name, long value) throws java.io.IOException
		private void sendResponseHeader(System.IO.Stream os, string name, long value)
		{
			sendResponseHeader(os, name, value.ToString());
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendNoCache(java.io.OutputStream os) throws java.io.IOException
		private void sendNoCache(System.IO.Stream os)
		{
			sendResponseHeader(os, "Cache-Control", "no-cache");
			sendResponseHeader(os, "Cache-Control", "private");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendEndOfHeaders(java.io.OutputStream os) throws java.io.IOException
		private void sendEndOfHeaders(System.IO.Stream os)
		{
			sendResponseLine(os, "");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendError(java.io.OutputStream os, int code) throws java.io.IOException
		private void sendError(System.IO.Stream os, int code)
		{
			sendHTTPResponseCode(os, code);
			sendEndOfHeaders(os);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendErrorNotFound(java.io.OutputStream os) throws java.io.IOException
		private void sendErrorNotFound(System.IO.Stream os)
		{
			sendError(os, 404);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendScreenImage(java.io.OutputStream os, String fileFormat) throws java.io.IOException
		private void sendScreenImage(System.IO.Stream os, string fileFormat)
		{
			string fileName = string.Format("{0}{1}screen.{2}", Settings.Instance.readString("emu.tmppath"), System.IO.Path.DirectorySeparatorChar, fileFormat);
			File file = new File(fileName);
			file.deleteOnExit();
			Rectangle rect = Emulator.MainGUI.CaptureRectangle;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Capturing screen from {0} to {1}", rect, fileName));
			}

			BufferedImage img = captureRobot.createScreenCapture(rect);
			try
			{
				file.delete();
				ImageIO.write(img, fileFormat, file);
				img.flush();
			}
			catch (IOException e)
			{
				log.error("Error saving screenshot", e);
			}

			if (file.canRead())
			{
				int length = (int) file.length();
				sendOK(os);
				sendNoCache(os);
				sendResponseHeader(os, "Content-Type", string.Format("image/{0}", fileFormat));
				sendResponseHeader(os, "Content-Length", length);
				sendEndOfHeaders(os);
				sbyte[] buffer = new sbyte[length];
				System.IO.Stream @is = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				length = @is.Read(buffer, 0, buffer.Length);
				@is.Close();
				file.delete();
				os.Write(buffer, 0, length);
			}
			else
			{
				sendErrorNotFound(os);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendVideoMJPG(java.io.OutputStream os) throws java.io.IOException
		private void sendVideoMJPG(System.IO.Stream os)
		{
			string fileFormat = "jpg";
			string fileName = string.Format("{0}{1}screen.{2}", Settings.Instance.readString("emu.tmppath"), System.IO.Path.DirectorySeparatorChar, fileFormat);
			File file = new File(fileName);
			file.deleteOnExit();
			Rectangle rect = Emulator.MainGUI.CaptureRectangle;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Capturing screen from {0} to {1}", rect, fileName));
			}

			startDisplayAction();

			try
			{
				sendOK(os);
				sendNoCache(os);
				sendResponseHeader(os, "Content-Type", string.Format("multipart/x-mixed-replace; boundary={0}", boundary));
				sendEndOfHeaders(os);

				while (true)
				{
					BufferedImage img = getScreenImage(rect);
					try
					{
						file.delete();
						ImageIO.write(img, fileFormat, file);
						img.flush();
					}
					catch (IOException e)
					{
						log.error("Error saving screenshot", e);
					}

					if (file.canRead())
					{
						int length = (int) file.length();
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Sending video image length={0:D}", length));
						}
						sbyte[] buffer = new sbyte[length];
						System.IO.Stream @is = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read);
						length = @is.Read(buffer, 0, buffer.Length);
						@is.Close();

						sendResponseLine(os, boundary);
						sendResponseHeader(os, "Content-Type", "image/jpeg");
						sendResponseHeader(os, "Content-Length", length);
						sendEndOfHeaders(os);
						os.Write(buffer, 0, length);
						sendEndOfHeaders(os);
						os.Flush();
					}
					else
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Cannot read capture file {0}", file));
						}
						break;
					}
				}
			}
			finally
			{
				stopDisplayAction();
				file.delete();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendAudioWAV(java.io.OutputStream os) throws java.io.IOException
		private void sendAudioWAV(System.IO.Stream os)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sendAudioWAV"));
			}
			sendOK(os);
			sendResponseHeader(os, "Content-Type", "audio/wav");
			sendEndOfHeaders(os);

			int channels = 2;
			int sampleRate = 44100;
			sbyte[] silenceBuffer = new sbyte[1024 * channels * 2];

			sbyte[] header = new sbyte[100];
			int n = 0;
			// "RIFF"
			header[n++] = (sbyte)'R';
			header[n++] = (sbyte)'I';
			header[n++] = (sbyte)'F';
			header[n++] = (sbyte)'F';
			// Total file size
			header[n++] = 0;
			header[n++] = 0;
			header[n++] = 0;
			header[n++] = 0x7F;
			// "WAVE"
			header[n++] = (sbyte)'W';
			header[n++] = (sbyte)'A';
			header[n++] = (sbyte)'V';
			header[n++] = (sbyte)'E';
			// "fmt " tag
			header[n++] = (sbyte)'f';
			header[n++] = (sbyte)'m';
			header[n++] = (sbyte)'t';
			header[n++] = (sbyte)' ';
			// length of "fmt " tag
			header[n++] = 16;
			header[n++] = 0;
			header[n++] = 0;
			header[n++] = 0;
			// format tag (1 == PCM)
			header[n++] = 1;
			header[n++] = 0;
			// channels
			header[n++] = (sbyte) channels;
			header[n++] = 0;
			// sample rate
			header[n++] = unchecked((sbyte)((sampleRate) & 0xFF));
			header[n++] = unchecked((sbyte)((sampleRate >> 8) & 0xFF));
			header[n++] = 0;
			header[n++] = 0;
			// bytes per second
			int bytesPerSecond = 2 * channels * sampleRate;
			header[n++] = unchecked((sbyte)((bytesPerSecond) & 0xFF));
			header[n++] = unchecked((sbyte)((bytesPerSecond >> 8) & 0xFF));
			header[n++] = unchecked((sbyte)((bytesPerSecond >> 16) & 0xFF));
			header[n++] = unchecked((sbyte)((bytesPerSecond >> 24) & 0xFF));
			// block align
			header[n++] = (sbyte)(2 * channels);
			header[n++] = 0;
			// bits per sample
			header[n++] = 16;
			header[n++] = 0;
			os.Write(header, 0, n);

			sbyte[] dataHeader = new sbyte[8];
			dataHeader[0] = (sbyte)'d';
			dataHeader[1] = (sbyte)'a';
			dataHeader[2] = (sbyte)'t';
			dataHeader[3] = (sbyte)'a';

			long start = DateTimeHelper.CurrentUnixTimeMillis();
			while (true)
			{
				long now = DateTimeHelper.CurrentUnixTimeMillis();
				while (now < start)
				{
					Utilities.sleep(1, 0);
					now = DateTimeHelper.CurrentUnixTimeMillis();
				}
				sbyte[] buffer = Modules.sceAudioModule.audioData;
				if (buffer == null)
				{
					buffer = silenceBuffer;
				}
				int length = buffer.Length;
				dataHeader[4] = unchecked((sbyte)((length) & 0xFF));
				dataHeader[5] = unchecked((sbyte)((length >> 8) & 0xFF));
				dataHeader[6] = unchecked((sbyte)((length >> 16) & 0xFF));
				dataHeader[7] = unchecked((sbyte)((length >> 24) & 0xFF));
				os.Write(dataHeader, 0, dataHeader.Length);
				os.Write(buffer, 0, length);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sendAudioWAV sent {0:D} bytes", length));
				}
				start += 1000 * length / (2 * channels * sampleRate);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendAudioRAW(java.io.OutputStream os) throws java.io.IOException
		private void sendAudioRAW(System.IO.Stream os)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sendAudioRAW"));
			}
			sendOK(os);
			sendResponseHeader(os, "Content-Type", "audio/raw");
			sendEndOfHeaders(os);

			int channels = 2;
			int sampleRate = 44100;
			sbyte[] silenceBuffer = new sbyte[1024 * channels * 2];

			long start = DateTimeHelper.CurrentUnixTimeMillis();
			while (true)
			{
				long now = DateTimeHelper.CurrentUnixTimeMillis();
				while (now < start)
				{
					Utilities.sleep(1, 0);
					now = DateTimeHelper.CurrentUnixTimeMillis();
				}
				sbyte[] buffer = Modules.sceAudioModule.audioData;
				if (buffer == null)
				{
					buffer = silenceBuffer;
				}
				int length = buffer.Length;
				os.Write(buffer, 0, length);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sendAudioRAW sent {0:D} bytes", length));
				}
				start += 1000 * length / (2 * channels * sampleRate);
			}
		}

		private BufferedImage getScreenImage(Rectangle rect)
		{
			if (currentDisplayImage != null)
			{
				return currentDisplayImage;
			}

			currentDisplayImageHasAlpha = true;
			return captureRobot.createScreenCapture(rect);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendVideoRAW(java.io.OutputStream os) throws java.io.IOException
		private void sendVideoRAW(System.IO.Stream os)
		{
			Rectangle rect = Emulator.MainGUI.CaptureRectangle;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Capturing RAW screen from {0}", rect));
			}

			startDisplayAction();

			try
			{
				sendOK(os);
				sendNoCache(os);
				sendResponseHeader(os, "Content-Type", "video/raw");
				sendEndOfHeaders(os);

				sbyte[] pixels = new sbyte[rect.width * rect.height * 3];
				while (true)
				{
					BufferedImage img = getScreenImage(rect);

					int i = 0;
					for (int y = 0; y < img.Height; y++)
					{
						for (int x = 0; x < img.Width; x++, i += 3)
						{
							int color = img.getRGB(x, y);
							pixels[i + 0] = unchecked((sbyte)((color >> 16) & 0xFF));
							pixels[i + 1] = unchecked((sbyte)((color >> 8) & 0xFF));
							pixels[i + 2] = unchecked((sbyte)((color >> 0) & 0xFF));
						}
					}
					os.Write(pixels, 0, pixels.Length);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sendVideoRAW sent {0:D}x{1:D} image ({2:D} bytes)", rect.width, rect.height, pixels.Length));
					}
					os.Flush();
				}
			}
			finally
			{
				stopDisplayAction();
			}
		}

		private int storeCompressedPixel(int color, sbyte[] buffer, int compressedLength, bool rle, int count)
		{
			if (!rle)
			{
				count |= 0x80;
			}

			buffer[compressedLength++] = (sbyte) count;
			buffer[compressedLength++] = unchecked((sbyte)((color >> 16) & 0xFF));
			buffer[compressedLength++] = unchecked((sbyte)((color >> 8) & 0xFF));
			buffer[compressedLength++] = unchecked((sbyte)((color >> 0) & 0xFF));

			return compressedLength;
		}

		private int compressImage(int width, int height, int[] image, int[] previousImage, sbyte[] buffer, int compressedLength)
		{
			int i = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width;)
				{
					int color = image[i];
					int previousColor = previousImage[i];
					i++;
					x++;

					// RLE?
					if (x < width && color == image[i])
					{
						if (color == previousColor)
						{
							// Both methods apply: RLE and matching previous video.
							// Choose the one matching the most pixels.
							bool rleFailed = false;
							bool previousFailed = false;
							int count;
							for (count = 0; x < width && count < MAX_COMPRESSED_COUNT; count++)
							{
								bool rleMatch = !rleFailed && image[i] == color;
								bool previousMatch = !previousFailed && image[i] == previousImage[i];

								if (rleMatch)
								{
									if (previousMatch)
									{
										// OK, both still matching
									}
									else
									{
										// Continue RLE, previous image no longer matching
										previousFailed = true;
									}
								}
								else
								{
									if (previousMatch)
									{
										// Continue testing previous image, RLE no longer matching
										rleFailed = true;
									}
									else
									{
										// Both tests failed, abort
										break;
									}
								}
								i++;
								x++;
							}

							// If none failed, prefer RLE encoding (because faster decoding)
							if (!rleFailed)
							{
								compressedLength = storeCompressedPixel(color, buffer, compressedLength, true, count);
							}
							else
							{
								// Encode to match the previous image
								if (x < width)
								{
									color = image[i++];
									x++;
								}
								else if (count > 0)
								{
									// Past screen width, take previous pixel
									color = image[i - 1];
									count--;
								}
								compressedLength = storeCompressedPixel(color, buffer, compressedLength, false, count);
							}
						}
						else
						{
							// Only RLE, not matching previous image
							i++;
							x++;
							int count;
							for (count = 1; x < width; count++)
							{
								if (color != image[i] || count >= MAX_COMPRESSED_COUNT)
								{
									break;
								}
								i++;
								x++;
							}
							compressedLength = storeCompressedPixel(color, buffer, compressedLength, true, count);
						}
					}
					else if (x < width && color == previousColor)
					{
						// No RLE, only matching previous image
						int count;
						for (count = 0; x < width; count++)
						{
							color = image[i];
							previousColor = previousImage[i];
							i++;
							x++;
							if (color != previousColor || count >= MAX_COMPRESSED_COUNT || x >= width)
							{
								break;
							}
						}
						compressedLength = storeCompressedPixel(color, buffer, compressedLength, false, count);
					}
					else
					{
						// No RLE, not matching previous image
						compressedLength = storeCompressedPixel(color, buffer, compressedLength, true, 0);
					}
				}
			}

			return compressedLength;
		}

		private static void write32(sbyte[] buffer, int offset, int value)
		{
			buffer[offset + 0] = unchecked((sbyte)((value >> 0) & 0xFF));
			buffer[offset + 1] = unchecked((sbyte)((value >> 8) & 0xFF));
			buffer[offset + 2] = unchecked((sbyte)((value >> 16) & 0xFF));
			buffer[offset + 3] = unchecked((sbyte)((value >> 24) & 0xFF));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendVideoCompressedRAW(java.io.OutputStream os) throws java.io.IOException
		private void sendVideoCompressedRAW(System.IO.Stream os)
		{
			Rectangle rect = Emulator.MainGUI.CaptureRectangle;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Capturing compressed RAW screen from {0}", rect));
			}

			startDisplayAction();

			try
			{
				sendOK(os);
				sendNoCache(os);
				sendResponseHeader(os, "Content-Type", "video/compressed-raw");
				sendEndOfHeaders(os);

				int[] image = new int[0];
				int[] previousImage = null;
				sbyte[] buffer = null;

				while (true)
				{
					BufferedImage img = getScreenImage(rect);

					if (img != null)
					{
						int width = img.Width;
						int height = img.Height;
						int imageSize = width * height;

						// Is the image now larger?
						if (image.Length < imageSize)
						{
							// Resize the buffers
							image = new int[imageSize];
							previousImage = new int[imageSize];
							buffer = new sbyte[imageSize * 4 + 12];
						}

						img.getRGB(0, 0, width, height, image, 0, width);

						if (currentDisplayImageHasAlpha)
						{
							for (int i = 0; i < imageSize; i++)
							{
								image[i] &= 0x00FFFFFF;
							}
						}

						// The first 12 bytes of the buffer will contain
						// - the length of the compressed image (including the 12 bytes header)
						// - the image width in pixels
						// - the image height in pixels
						int compressedLength = compressImage(width, height, image, previousImage, buffer, 12);
						// Store the length of the compressed image and its size
						write32(buffer, 0, compressedLength);
						write32(buffer, 4, width);
						write32(buffer, 8, height);

						os.Write(buffer, 0, compressedLength);
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sendVideoCompressedRAW sent {0:D}x{1:D} image ({2:D} bytes, compression rate {3:F1}%)", width, height, compressedLength, 100f * compressedLength / (image.Length * 3)));
						}
						os.Flush();

						// Swap previous and current image buffers
						int[] swapImage = image;
						image = previousImage;
						previousImage = swapImage;
					}
					else
					{
						Utilities.sleep(10, 0);
					}
				}
			}
			finally
			{
				stopDisplayAction();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendIso(java.util.HashMap<String, String> request, java.io.OutputStream os, String pathValue, boolean sendContent) throws java.io.IOException
		private void sendIso(Dictionary<string, string> request, System.IO.Stream os, string pathValue, bool sendContent)
		{
			string isoFileName = pathValue.Substring(isoDirectory.Length);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sendIso '{0}'", isoFileName));
			}

			bool contentSent = false;
			try
			{
				UmdIsoReader iso = getIso(isoFileName);
				if (iso != null)
				{
					if (sendContent)
					{
						string range = request["range"];
						if (!string.ReferenceEquals(range, null))
						{
							if (range.StartsWith("bytes=", StringComparison.Ordinal))
							{
								string rangeValues = range.Substring(6);
								string[] ranges = rangeValues.Split("-", true);
								if (ranges != null && ranges.Length == 2)
								{
									long from = long.Parse(ranges[0]);
									long to = long.Parse(ranges[1]);
									if (log.DebugEnabled)
									{
										log.debug(string.Format("sendIso bytes from=0x{0:X}, to=0x{1:X}, length=0x{2:X}", from, to, to - from + 1));
									}

									sendHTTPResponseCode(os, 206);
									sendResponseHeader(os, "Content-Range", string.Format("bytes {0:D}-{1:D}", from, to));
									sendEndOfHeaders(os);
									sendIsoContent(os, iso, from, to);
									contentSent = true;
								}
								else
								{
									log.warn(string.Format("sendIso: unsupported range format '{0}'", range));
								}
							}
							else
							{
								log.warn(string.Format("sendIso: unsupported range format '{0}'", range));
							}
						}
					}
					else
					{
						sendOK(os);

						long isoLength = iso.NumSectors * (long) sectorLength;
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sendIso returning content-length=0x{0:X}", isoLength));
						}
						sendResponseHeader(os, "Content-Length", isoLength);
						sendResponseHeader(os, "Accept-Ranges", "bytes");
						sendEndOfHeaders(os);
						contentSent = true;
					}
				}
			}
			catch (IOException)
			{
				contentSent = false;
			}

			if (!contentSent)
			{
				sendErrorNotFound(os);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private pspsharp.filesystems.umdiso.UmdIsoReader getIso(String isoFileName) throws java.io.FileNotFoundException, java.io.IOException
		private UmdIsoReader getIso(string isoFileName)
		{
			UmdIsoReader iso = null;
			if (isoFileName.Equals(previousIsoFilename))
			{
				iso = previousUmdIsoReader;
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Reusing previous UmdIsoReader for '{0}'", isoFileName));
				}
			}
			else
			{
				if ("umdbuffer.iso".Equals(isoFileName))
				{
					iso = new UmdIsoReader((string) null, true);
				}
				else
				{
					File[] umdPaths = MainGUI.getUmdPaths(false);
					for (int i = 0; i < umdPaths.Length; i++)
					{
						File isoPath = new File(string.Format("{0}{1}{2}", umdPaths[i], File.separator, isoFileName));
						if (isoPath.exists())
						{
							iso = new UmdIsoReader(isoPath.Path, false);
							break;
						}
					}
				}

				if (previousUmdIsoReader != null)
				{
					previousUmdIsoReader.close();
				}
				previousIsoFilename = isoFileName;
				previousUmdIsoReader = iso;
			}

			return iso;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendIsoContent(java.io.OutputStream os, pspsharp.filesystems.umdiso.UmdIsoReader iso, long from, long to) throws java.io.IOException
		private void sendIsoContent(System.IO.Stream os, UmdIsoReader iso, long from, long to)
		{
			int startSector = (int)(from / sectorLength);
			int endSector = (int)((to + sectorLength) / sectorLength);
			int numberSectors = endSector - startSector;
			sbyte[] buffer = new sbyte[numberSectors * UmdIsoFile.sectorLength];
			iso.readSectors(startSector, numberSectors, buffer, 0);

			int startSectorOffset = (int)(from - startSector * (long) sectorLength);
			int length = (int)(to - from + 1);
			os.Write(buffer, startSectorOffset, length);
		}

		private static IDictionary<string, string> parseParameters(string parameters)
		{
			IDictionary<string, string> result = new Dictionary<string, string>();
			string[] nvpairs = parameters.Split("&", true);
			foreach (string nvpair in nvpairs)
			{
				string[] nv = nvpair.Split("=", 2);
				if (nv != null && nv.Length >= 2)
				{
					string name = nv[0];
					string value = decodePath(nv[1]);
					result[name] = value;
				}
			}

			return result;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void processControls(java.io.OutputStream os, String parameters) throws java.io.IOException
		private void processControls(System.IO.Stream os, string parameters)
		{
			if (!string.ReferenceEquals(parameters, null))
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("processControls {0}", parameters));
				}

				IDictionary<string, string> @event = parseParameters(parameters);

				string type = @event["type"];
				if ("keyup".Equals(type))
				{
					int code = int.Parse(@event["keyCode"]);
					if (code == runMapping)
					{
						Emulator.MainGUI.run();
					}
					else if (code == pauseMapping)
					{
						Emulator.MainGUI.pause();
					}
					else if (code == resetMapping)
					{
						Emulator.MainGUI.reset();
					}
					else if (keyMapping.ContainsKey(code))
					{
						State.controller.keyReleased(keyMapping[code]);
					}
					else
					{
						State.controller.keyReleased(code);
					}
				}
				else if ("keydown".Equals(type))
				{
					int code = int.Parse(@event["keyCode"]);
					if (keyMapping.ContainsKey(code))
					{
						State.controller.keyPressed(keyMapping[code]);
					}
					else
					{
						State.controller.keyPressed(code);
					}
				}
				else if ("run".Equals(type))
				{
					Emulator.MainGUI.run();
				}
				else if ("pause".Equals(type))
				{
					Emulator.MainGUI.pause();
				}
				else if ("reset".Equals(type))
				{
					Emulator.MainGUI.reset();
				}
				else if ("mapping".Equals(type))
				{
					processKeyMapping(@event);
				}
				else
				{
					log.warn(string.Format("processControls unknown type '{0}'", type));
				}
			}

			sendOK(os);
			sendEndOfHeaders(os);
		}

		private void processKeyMapping(IDictionary<string, string> @event)
		{
			foreach (string key in @event.Keys)
			{
				string value = @event[key];
				if (value.Length == 0)
				{
					// Silently ignore empty values
				}
				else if ("run".Equals(key))
				{
					runMapping = int.Parse(value);
				}
				else if ("pause".Equals(key))
				{
					pauseMapping = int.Parse(value);
				}
				else if ("reset".Equals(key))
				{
					resetMapping = int.Parse(value);
				}
				else if (!"type".Equals(key))
				{
					try
					{
						keyCode code = Enum.Parse(typeof(keyCode), key);
						keyMapping[int.Parse(value)] = code;
					}
					catch (System.ArgumentException)
					{
						// Ignore exception
					}
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendIcon(java.io.OutputStream os, String pathValue) throws java.io.IOException
		private void sendIcon(System.IO.Stream os, string pathValue)
		{
			sendResponseFile(os, "/pspsharp/icons/" + pathValue.Substring(iconDirectory.Length));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private String readInputStream(java.io.InputStream is) throws java.io.IOException
		private string readInputStream(System.IO.Stream @is)
		{
			sbyte[] buffer = new sbyte[100000];
			int length = @is.Read(buffer, 0, buffer.Length);
			return StringHelper.NewString(buffer, 0, length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private String readResource(String name) throws java.io.IOException
		private string readResource(string name)
		{
			System.IO.Stream @is = this.GetType().getResourceAsStream(name);
			return readInputStream(@is);
		}

		private string extractTemplateRepeat(string template)
		{
			int repeat = template.IndexOf("$REPEAT", StringComparison.Ordinal);
			int end = template.IndexOf("$END", StringComparison.Ordinal);
			if (repeat < 0 || end < 0 || end < repeat)
			{
				return "";
			}

			return template.Substring(repeat + 7, end - (repeat + 7));
		}

		private string replaceTemplate(string template, string name, string value)
		{
			return template.Replace(name, value);
		}

		private string replaceTemplateRepeat(string template, string value)
		{
			int repeat = template.IndexOf("$REPEAT", StringComparison.Ordinal);
			int end = template.IndexOf("$END", StringComparison.Ordinal);
			if (repeat < 0 || end < 0 || end < repeat)
			{
				return template;
			}

			return template.Substring(0, repeat) + value + template.Substring(end + 4);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private String[] getWidgetList() throws java.io.IOException
		private string[] WidgetList
		{
			get
			{
				IList<string> list = new LinkedList<string>();
				System.IO.StreamReader dir = null;
				try
				{
					dir = new System.IO.StreamReader(this.GetType().getResourceAsStream(widgetPath));
					while (true)
					{
						string entry = dir.ReadLine();
						if (string.ReferenceEquals(entry, null))
						{
							break;
						}
						list.Add(entry);
					}
				}
				finally
				{
					if (dir != null)
					{
						dir.Close();
					}
				}
    
				return list.ToArray();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.io.InputStream getFileFromZip(String zipFileName, String fileName) throws java.io.IOException
		private System.IO.Stream getFileFromZip(string zipFileName, string fileName)
		{
			if (System.IO.Path.DirectorySeparatorChar != '/')
			{
				fileName = fileName.Replace('/', System.IO.Path.DirectorySeparatorChar);
			}

			System.IO.Stream zipInput = this.GetType().getResourceAsStream(zipFileName);
			if (zipInput != null)
			{
				ZipInputStream zipContent = new ZipInputStream(zipInput);
				while (true)
				{
					ZipEntry entry = zipContent.NextEntry;
					if (entry == null)
					{
						break;
					}
					if (fileName.Equals(entry.Name, StringComparison.OrdinalIgnoreCase))
					{
						return zipContent;
					}
				}
			}

			return null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendNaClResponse(java.io.OutputStream os, String pathValue) throws java.io.IOException
		private void sendNaClResponse(System.IO.Stream os, string pathValue)
		{
			int sepIndex = pathValue.IndexOf("/", StringComparison.Ordinal);
			if (sepIndex < 0)
			{
				if (pathValue.Length == 0 || indexFile.Equals(pathValue))
				{
					string template = readResource(rootDirectory + "/" + naclDirectory + "/" + indexFile);
					string repeat = extractTemplateRepeat(template);
					StringBuilder lines = new StringBuilder();
					foreach (string widget in WidgetList)
					{
						lines.Append(replaceTemplate(repeat, "$NAME", widget.Replace(".zip", "")));
					}
					string html = replaceTemplateRepeat(template, lines.ToString());
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sendNaClResponse returning:\n{0}", html));
					}
					sendResponseFile(os, new System.IO.MemoryStream(html.GetBytes()), guessMimeType(indexFile));
				}
				else
				{
					sendRedirect(os, pathValue + "/" + indexFile);
				}
				return;
			}
			string zipFileName = pathValue.Substring(0, sepIndex);
			string resourceFileName = pathValue.Substring(sepIndex + 1);

			if (resourceFileName.StartsWith("$MANAGER_WIDGET/", StringComparison.Ordinal))
			{
				// Sending dummy Widget.js and TVKeyValue.js
				sendResponseFile(os, rootDirectory + "/" + resourceFileName.Substring(1));
			}
			else
			{
				zipFileName = string.Format("{0}/{1}.zip", widgetPath, zipFileName);
				System.IO.Stream zipContent = getFileFromZip(zipFileName, resourceFileName);

				if (zipContent != null)
				{
					sendResponseFile(os, zipContent, guessMimeType(resourceFileName));
				}
				else
				{
					sendError(os, 404);
				}
			}
		}

		private void startDisplayAction()
		{
			displayActionUsageCount++;

			if (displayAction == null)
			{
				displayAction = new DisplayAction(this);
				Modules.sceDisplayModule.addDisplayAction(displayAction);
			}
		}

		private void stopDisplayAction()
		{
			displayActionUsageCount--;

			if (displayAction != null && displayActionUsageCount <= 0)
			{
				Modules.sceDisplayModule.removeDisplayAction(displayAction);
				displayAction = null;
			}
		}

		private static string getBaseUrl(HTTPServerDescriptor descriptor, Dictionary<string, string> request, int forcedPort)
		{
			string hostName = request[host];
			int port = forcedPort > 0 ? forcedPort : descriptor.Port;
			string protocol = request["x-forwarded-proto"];
			if (string.ReferenceEquals(protocol, null))
			{
				if (forcedPort > 0)
				{
					protocol = forcedPort == 443 ? "https" : "http";
				}
				else
				{
					protocol = descriptor.Ssl ? "https" : "http";
				}
			}

			StringBuilder baseUrl = new StringBuilder();
			baseUrl.Append(protocol);
			baseUrl.Append("://");
			baseUrl.Append(hostName);

			// Add the port if this is not the default one
			if (port != getDefaultPortForProtocol(protocol))
			{
				baseUrl.Append(":");
				baseUrl.Append(port);
			}
			baseUrl.Append("/");

			return baseUrl.ToString();
		}

		private static string getUrl(HTTPServerDescriptor descriptor, Dictionary<string, string> request, string pathValue, int forcedPort)
		{
			if (pathValue.StartsWith("https://", StringComparison.Ordinal) || pathValue.StartsWith("http://", StringComparison.Ordinal))
			{
				int endOfPath = pathValue.IndexOf("/", 8, StringComparison.Ordinal);
				if (endOfPath >= 0)
				{
					pathValue = pathValue.Substring(endOfPath);
				}
				else
				{
					pathValue = "";
				}
			}

			string baseUrl = getBaseUrl(descriptor, request, forcedPort);

			string query = "";
			if (request.ContainsKey(parameters))
			{
				query = "?" + request[parameters];
			}

			if (string.ReferenceEquals(pathValue, null))
			{
				return baseUrl + query;
			}

			if (pathValue.StartsWith("/", StringComparison.Ordinal))
			{
				pathValue = pathValue.Substring(1);
			}

			return baseUrl + pathValue + query;
		}

		private static string getArchitecture(Dictionary<string, string> request)
		{
			string architecture = null;

			string userAgent = request["user-agent"];
			if (!string.ReferenceEquals(userAgent, null) && userAgent.IndexOf("SmartTV", StringComparison.Ordinal) > 0)
			{
				// Samsung Smart TV is using the ARM architecture
				architecture = Convert.ToString(Elf32Header.E_MACHINE_ARM);
			}

			return architecture;
		}

		/*
		 * Send the widgetlist.xml as expected by a Samsung Smart TV.
		 *
		 * The XML response is build dynamically, based on the packages available
		 * under the Widget directory.
		 */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendWidgetlist(HTTPServerDescriptor descriptor, java.util.HashMap<String, String> request, java.io.OutputStream os, String pathValue) throws java.io.IOException
		private void sendWidgetlist(HTTPServerDescriptor descriptor, Dictionary<string, string> request, System.IO.Stream os, string pathValue)
		{
			string template = readResource(rootDirectory + widgetlistFile + ".template");
			string repeat = extractTemplateRepeat(template);

			string architecture = getArchitecture(request);
			string architectureParam = "";
			if (!string.ReferenceEquals(architecture, null))
			{
				architectureParam = "?architecture=" + architecture;
			}

			StringBuilder list = new StringBuilder();
			Pattern pattern = Pattern.compile("<widgetname(\\s+itemtype=\"string\")?>(.*)</widgetname>", Pattern.MULTILINE | Pattern.CASE_INSENSITIVE);
			foreach (string widget in WidgetList)
			{
				string zipFileName = string.Format("{0}/{1}", widgetPath, widget);
				System.IO.Stream configXml = getFileFromZip(zipFileName, "config.xml");
				if (configXml != null)
				{
					string xml = readInputStream(configXml);
					Matcher matcher = pattern.matcher(xml);
					if (matcher.find())
					{
						string widgetName = matcher.group(2);
						string downloadUrl = string.Format("{0}{1}/{2}{3}", getBaseUrl(descriptor, request, 0), widgetDirectory, widget, architectureParam);
						string entry = replaceTemplate(repeat, "$WIDGETNAME", widgetName);
						entry = replaceTemplate(entry, "$DOWNLOADURL", downloadUrl);
						list.Append(entry);
					}
				}
			}
			string xml = replaceTemplateRepeat(template, list.ToString());

			sendResponseFile(os, new System.IO.MemoryStream(xml.GetBytes()), guessMimeType(pathValue));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean isMatchingELFArchitecture(byte[] header, int length, int machineArchitecture) throws java.io.IOException
		private bool isMatchingELFArchitecture(sbyte[] header, int length, int machineArchitecture)
		{
			ByteBuffer byteBuffer = ByteBuffer.wrap(header, 0, length);
			Elf32Header elfHeader = new Elf32Header(byteBuffer);

			return elfHeader.Valid && elfHeader.E_machine == machineArchitecture;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sendWidget(java.io.OutputStream os, String parameters, String pathValue) throws java.io.IOException
		private void sendWidget(System.IO.Stream os, string parameters, string pathValue)
		{
			string architecture = null;
			if (!string.ReferenceEquals(parameters, null))
			{
				IDictionary<string, string> map = parseParameters(parameters);
				architecture = map["architecture"];
			}

			if (string.ReferenceEquals(architecture, null))
			{
				sendResponseFile(os, pathValue);
			}
			else
			{
				// Filter the Widget zip file to only include the ELF files
				// matching the given architecture.
				// The Samsung Smart TV is rejecting the installation of a Widget
				// containing code for another architecture.
				int machineArchitecture = int.Parse(architecture);
				ZipInputStream zin = new ZipInputStream(this.GetType().getResourceAsStream(pathValue));
				System.IO.MemoryStream @out = new System.IO.MemoryStream(1000000);
				ZipOutputStream zout = new ZipOutputStream(@out);
				sbyte[] buffer = new sbyte[100000];
				sbyte[] header = new sbyte[0x40];

				while (true)
				{
					ZipEntry entry = zin.NextEntry;
					if (entry == null)
					{
						break;
					}

					int length = 0;
					bool doCopy = true;
					if (entry.Name.EndsWith(".nexe"))
					{
						length = zin.read(header);
						if (!isMatchingELFArchitecture(header, length, machineArchitecture))
						{
							if (log.DebugEnabled)
							{
								log.debug(string.Format("Skipping the Widget entry '{0}' because it is not matching the architecture 0x{1:X}", entry.Name, machineArchitecture));
							}
							doCopy = false;
						}
					}

					if (doCopy)
					{
						zout.putNextEntry(entry);
						zout.write(header, 0, length);
						while (true)
						{
							length = zin.read(buffer);
							if (length <= 0)
							{
								break;
							}
							zout.write(buffer, 0, length);
						}
					}
				}
				zin.close();
				zout.close();

				sendResponseFile(os, new System.IO.MemoryStream(@out.toByteArray()), guessMimeType(pathValue));
			}
		}

		public virtual Proxy Proxy
		{
			get
			{
				return proxy;
			}
		}

		public virtual int ProxyPort
		{
			get
			{
				return proxyPort;
			}
		}

		public virtual int ProxyAddress
		{
			get
			{
				return proxyAddress;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void sendNpNavAuth(String data, java.io.OutputStream os) throws java.io.IOException
		public virtual void sendNpNavAuth(string data, System.IO.Stream os)
		{
			IDictionary<string, string> parameters = parseParameters(data);

			SceNpTicket ticket = new SceNpTicket();
			ticket.version = 0x00000121;
			ticket.size = 0xF0;
			ticket.unknown = 0x3000;
			ticket.sizeParams = 0xA4;
			addTicketParam(ticket, "XXXXXXXXXXXXXXXXXXXX", 20);
			addTicketParam(ticket, 0);
			long now = DateTimeHelper.CurrentUnixTimeMillis();
			addTicketDateParam(ticket, now);
			addTicketDateParam(ticket, now + 10 * 60 * 1000); // now + 10 minutes
			addTicketLongParam(ticket, 0L); // Used by DRM
			addTicketParam(ticket, SceNpTicket.TicketParam.PARAM_TYPE_STRING, "DummyOnlineID", 32);
			addTicketParam(ticket, "gb", 4);
			addTicketParam(ticket, SceNpTicket.TicketParam.PARAM_TYPE_STRING, "XX", 4);
			addTicketParam(ticket, parameters["serviceid"], 24);
			int status = 0;
			if (Modules.sceNpModule.parentalControl == sceNp.PARENTAL_CONTROL_ENABLED)
			{
				status |= STATUS_ACCOUNT_PARENTAL_CONTROL_ENABLED;
			}
			status |= (Modules.sceNpModule.UserAge & 0x7F) << 24;
			addTicketParam(ticket, status);
			addTicketParam(ticket);
			addTicketParam(ticket);
			ticket.unknownBytes = new sbyte[72];
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sendNpNavAuth returning dummy ticket: {0}", ticket));
			}
			sbyte[] response = ticket.toByteArray();

			sendOK(os);
			sendResponseHeader(os, "X-I-5-Status", "OK");
			sendResponseHeader(os, "X-I-5-Version", "2.1");
			sendResponseHeader(os, "Content-Length", response.Length);
			sendResponseHeader(os, "Content-Type", "application/x-i-5-ticket");
			sendEndOfHeaders(os);
			os.Write(response, 0, response.Length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void sendNpGetSelfProfile(String data, java.io.OutputStream os) throws java.io.IOException
		public virtual void sendNpGetSelfProfile(string data, System.IO.Stream os)
		{
			string xml = "<profile result=\"00\">";
			xml += "<jid>DummyOnlineID@a8.gb.np.playstation.net</jid>";
			xml += "<onlinename upd=\"0\">DummyOnlineID</onlinename>";
			xml += "<country>gb</country>";
			xml += "<language1>1</language1>";
			xml += "<language2 />";
			xml += "<language3 />";
			xml += "<aboutme />";
			xml += "<avatarurl id=\"0\">http://static-resource.np.community.playstation.net/avatar_s/default/DefaultAvatar_s.png</avatarurl>";
			xml += "<ptlp>0</ptlp>";
			xml += "</profile>";
			sbyte[] response = xml.GetBytes();

			sendOK(os);
			sendResponseHeader(os, "Content-Length", response.Length);
			sendResponseHeader(os, "Content-Type", "text/xml;charset=UTF-8");
			sendEndOfHeaders(os);
			os.Write(response, 0, response.Length);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Response: {0}", xml));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void sendCapM(String data, java.io.OutputStream os) throws java.io.IOException
		public virtual void sendCapM(string data, System.IO.Stream os)
		{
			int responseLength = 4240;
			sbyte[] response = new sbyte[responseLength];

			if (ticket != null)
			{
				SceNpTicket.TicketParam ticketParam = ticket.parameters[4];
				for (int i = 0; i < 8; i++)
				{
					response[i + 80] = ticketParam.BytesValue[7 - i];
				}
			}

			sendOK(os);
			sendResponseHeader(os, "X-I-5-DRM-Version", "1.0");
			sendResponseHeader(os, "X-I-5-DRM-Status", "OK; max_console=1; current_console=0");
			sendResponseHeader(os, "Content-Length", responseLength);
			sendResponseHeader(os, "Content-Type", "application/x-i-5-drm");
			sendEndOfHeaders(os);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Response:{0}", Utilities.getMemoryDump(response, 0, responseLength)));
			}

			os.Write(response, 0, responseLength);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void sendKdpM(String data, java.io.OutputStream os) throws java.io.IOException
		public virtual void sendKdpM(string data, System.IO.Stream os)
		{
			IDictionary<string, string> parameters = parseParameters(data);
			string productId = parameters["productid"];

			int responseLength = 4240;
			sbyte[] response = new sbyte[responseLength];

			if (!string.ReferenceEquals(productId, null))
			{
				ByteBuffer buffer = ByteBuffer.wrap(response);
				buffer.position(16);
				Utilities.writeStringZ(buffer, productId);
			}

			if (ticket != null)
			{
				SceNpTicket.TicketParam ticketParam = ticket.parameters[4];
				for (int i = 0; i < 8; i++)
				{
					response[i + 80] = ticketParam.BytesValue[7 - i];
				}
			}

			sendOK(os);
			sendResponseHeader(os, "X-I-5-DRM-Version", "1.0");
			sendResponseHeader(os, "X-I-5-DRM-Status", "OK");
			sendResponseHeader(os, "Content-Length", responseLength);
			sendResponseHeader(os, "Content-Type", "application/x-i-5-drm");
			sendEndOfHeaders(os);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Response:{0}", Utilities.getMemoryDump(response, 0, responseLength)));
			}

			os.Write(response, 0, responseLength);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void sendVideoStore(java.io.OutputStream os) throws java.io.IOException
		public virtual void sendVideoStore(System.IO.Stream os)
		{
			sbyte[] response = new sbyte[1];
			response[0] = (sbyte) '3';
			int responseLength = response.Length;

			sendOK(os);
			sendResponseHeader(os, "Content-Length", responseLength);
			sendEndOfHeaders(os);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Response:{0}", Utilities.getMemoryDump(response, 0, responseLength)));
			}

			os.Write(response, 0, responseLength);
		}
	}

}