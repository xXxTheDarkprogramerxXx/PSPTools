using System;
using System.Collections.Generic;
using System.Text;

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
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using HTTPConfiguration = pspsharp.remote.HTTPConfiguration;
	using HttpServerConfiguration = pspsharp.remote.HTTPConfiguration.HttpServerConfiguration;
	using HTTPServer = pspsharp.remote.HTTPServer;
	using ThreadLocalCookieManager = pspsharp.util.ThreadLocalCookieManager;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class sceHttp : HLEModule
	{
		public static Logger log = Modules.getLogger("sceHttp");
		public const int PSP_HTTP_SYSTEM_COOKIE_HEAP_SIZE = 130 * 1024;
		private bool isHttpInit;
		private bool isSystemCookieLoaded;
		private int maxMemSize;
		private SysMemInfo memInfo;
		protected internal Dictionary<int, HttpTemplate> httpTemplates = new Dictionary<int, HttpTemplate>();
		protected internal Dictionary<int, HttpConnection> httpConnections = new Dictionary<int, HttpConnection>();
		protected internal Dictionary<int, HttpRequest> httpRequests = new Dictionary<int, HttpRequest>();
		private CookieManager cookieManager;
		private static readonly string[] httpMethods = new string[] {"GET", "POST", "HEAD", "OPTIONS", "PUT", "DELETE", "TRACE", "CONNECT"};

		protected internal class HttpRequest
		{
			internal const string uidPurpose = "sceHttp-HttpRequest";
			internal int id;
			internal string url;
			internal string path;
			internal int method;
			internal long contentLength;
			internal HttpConnection httpConnection;
			internal URLConnection urlConnection;
			internal HttpURLConnection httpUrlConnection;
			internal Dictionary<string, string> headers = new Dictionary<string, string>();
			internal sbyte[] sendData;
			internal int sendDataLength;

			public HttpRequest()
			{
				id = SceUidManager.getNewUid(uidPurpose);
				Modules.sceHttpModule.httpRequests[id] = this;
			}

			public virtual void delete()
			{
				Modules.sceHttpModule.httpRequests.Remove(id);
				SceUidManager.releaseUid(id, uidPurpose);
				id = -1;
			}

			public virtual int Id
			{
				get
				{
					return id;
				}
			}

			public virtual string Url
			{
				get
				{
					if (!string.ReferenceEquals(url, null))
					{
						return url;
					}
					if (!string.ReferenceEquals(path, null))
					{
						if (path.StartsWith("http:", StringComparison.Ordinal) || path.StartsWith("https:", StringComparison.Ordinal))
						{
							return path;
						}
						return HttpConnection.Url + path;
					}
    
					return null;
				}
				set
				{
					this.url = value;
				}
			}


			public virtual string Path
			{
				set
				{
					this.path = value;
				}
			}

			public virtual int Method
			{
				get
				{
					return method;
				}
				set
				{
					this.method = value;
				}
			}


			public virtual long ContentLength
			{
				get
				{
					return contentLength;
				}
				set
				{
					this.contentLength = value;
				}
			}


			public virtual HttpConnection HttpConnection
			{
				get
				{
					return httpConnection;
				}
				set
				{
					this.httpConnection = value;
				}
			}


			public virtual void send(int data, int dataSize)
			{
				if (dataSize > 0)
				{
					sendData = Utilities.extendArray(sendData, dataSize);
					Utilities.readBytes(data, dataSize, sendData, sendDataLength);
					sendDataLength += dataSize;
				}
			}

			public virtual void connect()
			{
				if (urlConnection != null)
				{
					// Already connected
					return;
				}

				ThreadLocalCookieManager.CookieManager = Modules.sceHttpModule.cookieManager;

				if (log.TraceEnabled)
				{
					log.trace(string.Format("HttpRequest {0} send: {1}", this, Utilities.getMemoryDump(sendData, 0, sendDataLength)));
				}

				string sendUrl = Url;
				Proxy proxy = getProxyForUrl(sendUrl);

				// Replace https with http when using a proxy
				if (proxy != null)
				{
					if (sendUrl.StartsWith("https:", StringComparison.Ordinal))
					{
						sendUrl = "http:" + sendUrl.Substring(6);
					}
				}

				try
				{
					if (proxy != null)
					{
						urlConnection = (new URL(sendUrl)).openConnection(proxy);
					}
					else
					{
						urlConnection = (new URL(sendUrl)).openConnection();
					}

					string agent = HttpConnection.HttpTemplate.Agent;
					if (!string.ReferenceEquals(agent, null))
					{
						if (log.TraceEnabled)
						{
							log.trace((string.Format("Adding header '{0}': '{1}'", "User-Agent", agent)));
						}
						urlConnection.setRequestProperty("User-Agent", agent);
					}

					foreach (string header in headers.Keys)
					{
						if (log.TraceEnabled)
						{
							log.trace(string.Format("Adding header '{0}': '{1}'", header, headers[header]));
						}
						urlConnection.setRequestProperty(header, headers[header]);
					}

					if (urlConnection is HttpURLConnection)
					{
						httpUrlConnection = (HttpURLConnection) urlConnection;
						httpUrlConnection.RequestMethod = httpMethods[method];
						httpUrlConnection.InstanceFollowRedirects = HttpConnection.EnableRedirect;
						if (sendDataLength > 0)
						{
							httpUrlConnection.DoOutput = true;
							System.IO.Stream os = httpUrlConnection.OutputStream;
							os.Write(sendData, 0, sendDataLength);
							os.Close();
						}
					}
					else
					{
						httpUrlConnection = null;
					}
					urlConnection.connect();
					ContentLength = urlConnection.ContentLength;
				}
				catch (MalformedURLException e)
				{
					log.error("HttpRequest.send", e);
				}
				catch (IOException e)
				{
					log.error("HttpRequest.send", e);
				}
			}

			public virtual int readData(int data, int dataSize)
			{
				sbyte[] buffer = new sbyte[dataSize];
				int bufferLength = 0;

				try
				{
					while (bufferLength < dataSize)
					{
						int readSize = urlConnection.InputStream.read(buffer, bufferLength, dataSize - bufferLength);
						if (readSize < 0)
						{
							break;
						}
						bufferLength += readSize;
					}
				}
				catch (FileNotFoundException e)
				{
					log.debug("HttpRequest.readData", e);
				}
				catch (IOException e)
				{
					log.error("HttpRequest.readData", e);
				}

				if (bufferLength > 0)
				{
					IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(data, bufferLength, 1);
					for (int i = 0; i < bufferLength; i++)
					{
						memoryWriter.writeNext(buffer[i] & 0xFF);
					}
					memoryWriter.flush();
				}

				return bufferLength;
			}

			public virtual string AllHeaders
			{
				get
				{
					if (urlConnection == null)
					{
						return null;
					}
    
					StringBuilder allHeaders = new StringBuilder();
					IDictionary<string, IList<string>> properties = urlConnection.HeaderFields;
					foreach (string key in properties.Keys)
					{
						if (!string.ReferenceEquals(key, null))
						{
							IList<string> values = properties[key];
							foreach (string value in values)
							{
								allHeaders.Append(string.Format("{0}: {1}\r\n", key, value));
							}
						}
					}
    
					return allHeaders.ToString();
				}
			}

			public virtual int StatusCode
			{
				get
				{
					int statusCode = 0;
    
					if (httpUrlConnection != null)
					{
						try
						{
							statusCode = httpUrlConnection.ResponseCode;
						}
						catch (IOException e)
						{
							log.error("HttpRequest.getStatusCode", e);
						}
					}
    
					return statusCode;
				}
			}

			internal virtual void addHeader(string name, string value)
			{
				headers[name] = value;
			}

			public override string ToString()
			{
				return string.Format("HttpRequest id={0:D}, url='{1}', method={2:D}, contentLength={3:D}", Id, Url, Method, contentLength);
			}
		}

		protected internal class HttpConnection
		{
			internal const string uidPurpose = "sceHttp-HttpConnection";
			internal int id;
			internal Dictionary<int, HttpRequest> httpRequests = new Dictionary<int, sceHttp.HttpRequest>();
			internal string url;
			internal HttpTemplate httpTemplate;
			internal bool enableRedirect;

			public HttpConnection()
			{
				id = SceUidManager.getNewUid(uidPurpose);
				Modules.sceHttpModule.httpConnections[id] = this;
			}

			public virtual void delete()
			{
				// Delete all the HttpRequests
				foreach (HttpRequest httpRequest in httpRequests.Values)
				{
					httpRequest.delete();
				}
				httpRequests.Clear();

				Modules.sceHttpModule.httpConnections.Remove(id);
				SceUidManager.releaseUid(id, uidPurpose);
				id = -1;
			}

			public virtual void addHttpRequest(HttpRequest httpRequest)
			{
				httpRequest.HttpConnection = this;
				httpRequests[httpRequest.Id] = httpRequest;
			}

			public virtual int Id
			{
				get
				{
					return id;
				}
			}

			public virtual string Url
			{
				get
				{
					return url;
				}
				set
				{
					this.url = value;
				}
			}


			public virtual int getDefaultPort(string protocol)
			{
				if ("http".Equals(protocol))
				{
					return 80;
				}
				if ("https".Equals(protocol))
				{
					return 443;
				}

				return -1;
			}

			public virtual void setUrl(string host, string protocol, int port)
			{
				url = string.Format("{0}://{1}", protocol, host);
				if (port != getDefaultPort(protocol))
				{
					url += string.Format(":{0}", port);
				}
			}

			public virtual HttpTemplate HttpTemplate
			{
				get
				{
					return httpTemplate;
				}
				set
				{
					this.httpTemplate = value;
				}
			}


			public virtual bool EnableRedirect
			{
				get
				{
					return enableRedirect;
				}
				set
				{
					this.enableRedirect = value;
				}
			}


			public override string ToString()
			{
				return string.Format("HttpConnection id={0:D}, url='{1}'", Id, Url);
			}
		}

		protected internal class HttpTemplate
		{
			internal const string uidPurpose = "sceHttp-HttpTemplate";
			internal int id;
			internal Dictionary<int, HttpConnection> httpConnections = new Dictionary<int, sceHttp.HttpConnection>();
			internal string agent;
			internal bool enableRedirect;

			public HttpTemplate()
			{
				id = SceUidManager.getNewUid(uidPurpose);
				Modules.sceHttpModule.httpTemplates[id] = this;
			}

			public virtual void delete()
			{
				// Delete all the HttpConnections
				foreach (HttpConnection httpConnection in httpConnections.Values)
				{
					httpConnection.delete();
				}
				httpConnections.Clear();

				Modules.sceHttpModule.httpTemplates.Remove(id);
				SceUidManager.releaseUid(id, uidPurpose);
				id = -1;
			}

			public virtual void addHttpConnection(HttpConnection httpConnection)
			{
				httpConnection.HttpTemplate = this;
				httpConnection.EnableRedirect = EnableRedirect;
				httpConnections[httpConnection.Id] = httpConnection;
			}

			public virtual int Id
			{
				get
				{
					return id;
				}
			}

			public virtual string Agent
			{
				get
				{
					return agent;
				}
				set
				{
					this.agent = value;
				}
			}


			public virtual bool EnableRedirect
			{
				get
				{
					return enableRedirect;
				}
				set
				{
					this.enableRedirect = value;
				}
			}


			public override string ToString()
			{
				return string.Format("HttpTemplate id={0:D}, agent='{1}'", Id, Agent);
			}
		}

		public override void start()
		{
			CookieHandler.Default = new ThreadLocalCookieManager();
			cookieManager = new CookieManager();

			base.start();
		}

		public virtual void checkHttpInit()
		{
			if (!isHttpInit)
			{
				throw (new SceKernelErrorException(SceKernelErrors.ERROR_HTTP_NOT_INIT));
			}
		}

		private static Proxy getProxyForUrl(string url)
		{
			foreach (HTTPConfiguration.HttpServerConfiguration httpServerConfiguration in HTTPConfiguration.doProxyServers)
			{
				if (httpServerConfiguration.isMatchingUrl(url))
				{
					return HTTPServer.Instance.Proxy;
				}
			}

			return null;
		}

		public static string patchUrl(string url)
		{
			foreach (HTTPConfiguration.HttpServerConfiguration httpServerConfiguration in HTTPConfiguration.doProxyServers)
			{
				if (httpServerConfiguration.Https)
				{
					if (httpServerConfiguration.isMatchingUrl(url))
					{
						// Replace https with http
						return url.replaceFirst("https", "http");
					}
				}
			}

			return url;
		}

		protected internal virtual HttpRequest getHttpRequest(int requestId)
		{
			HttpRequest httpRequest = httpRequests[requestId];
			if (httpRequest == null)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_ARGUMENT);
			}

			return httpRequest;
		}

		protected internal virtual HttpConnection getHttpConnection(int connectionId)
		{
			HttpConnection httpConnection = httpConnections[connectionId];
			if (httpConnection == null)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_ARGUMENT);
			}

			return httpConnection;
		}

		protected internal virtual bool isHttpTemplateId(int templateId)
		{
			return httpTemplates.ContainsKey(templateId);
		}

		protected internal virtual HttpTemplate getHttpTemplate(int templateId)
		{
			HttpTemplate httpTemplate = httpTemplates[templateId];
			if (httpTemplate == null)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_ARGUMENT);
			}

			return httpTemplate;
		}

		private int TempMemory
		{
			get
			{
				if (memInfo == null)
				{
					memInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.VSHELL_PARTITION_ID, "sceHttp", SysMemUserForUser.PSP_SMEM_Low, maxMemSize, 0);
					if (memInfo == null)
					{
						return 0;
					}
				}
    
				return memInfo.addr;
			}
		}

		/// <summary>
		/// Init the http library.
		/// </summary>
		/// <param name="heapSize"> - Memory pool size? Pass 20000 </param>
		/// <returns> 0 on success, < 0 on error. </returns>
		[HLELogging(level:"info"), HLEFunction(nid : 0xAB1ABE07, version : 150, checkInsideInterrupt : true)]
		public virtual int sceHttpInit(int heapSize)
		{
			if (isHttpInit)
			{
				return SceKernelErrors.ERROR_HTTP_ALREADY_INIT;
			}

			maxMemSize = heapSize;
			isHttpInit = true;
			memInfo = null;

			// Allocate memory during sceHttpInit
			int addr = TempMemory;
			if (addr == 0)
			{
				log.warn(string.Format("sceHttpInit cannot allocate 0x{0:X} bytes", maxMemSize));
				return -1;
			}

			Utilities.disableSslCertificateChecks();

			return 0;
		}

		/// <summary>
		/// Terminate the http library.
		/// </summary>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD1C8945E, version = 150, checkInsideInterrupt = true) public int sceHttpEnd()
		[HLEFunction(nid : 0xD1C8945E, version : 150, checkInsideInterrupt : true)]
		public virtual int sceHttpEnd()
		{
			checkHttpInit();

			isSystemCookieLoaded = false;
			isHttpInit = false;

			if (memInfo != null)
			{
				Modules.SysMemUserForUserModule.free(memInfo);
				memInfo = null;
			}

			return 0;
		}

		/// <summary>
		/// Get http request response length.
		/// </summary>
		/// <param name="requestid"> - ID of the request created by sceHttpCreateRequest or sceHttpCreateRequestWithURL </param>
		/// <param name="contentlength"> - The size of the content </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0282A3BD, version = 150) public int sceHttpGetContentLength(int requestId, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer64 contentLengthAddr)
		[HLEFunction(nid : 0x0282A3BD, version : 150)]
		public virtual int sceHttpGetContentLength(int requestId, TPointer64 contentLengthAddr)
		{
			HttpRequest httpRequest = getHttpRequest(requestId);
			httpRequest.connect();
			long contentLength = httpRequest.ContentLength;

			int result;
			if (contentLength < 0)
			{
				// Value in contentLengthAddr is left unchanged when returning an error, checked on PSP.
				result = SceKernelErrors.ERROR_HTTP_NO_CONTENT_LENGTH;
			}
			else
			{
				contentLengthAddr.Value = contentLength;
				result = 0;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceHttpGetContentLength request {0} returning 0x{1:X}, contentLength=0x{2:X}", httpRequest, result, contentLengthAddr.Value));
			}

			return result;
		}

		/// <summary>
		/// Set resolver retry
		/// </summary>
		/// <param name="id"> - ID of the template or connection </param>
		/// <param name="count"> - Number of retries </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x03D9526F, version = 150) public int sceHttpSetResolveRetry(int templateId, int count)
		[HLEFunction(nid : 0x03D9526F, version : 150)]
		public virtual int sceHttpSetResolveRetry(int templateId, int count)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x06488A1C, version = 150) public int sceHttpSetCookieSendCallback()
		[HLEFunction(nid : 0x06488A1C, version : 150)]
		public virtual int sceHttpSetCookieSendCallback()
		{
			return 0;
		}

		/// <summary>
		/// Enable redirect
		/// </summary>
		/// <param name="id"> - ID of the template or connection </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0809C831, version = 150) public int sceHttpEnableRedirect(int id)
		[HLEFunction(nid : 0x0809C831, version : 150)]
		public virtual int sceHttpEnableRedirect(int id)
		{
			if (isHttpTemplateId(id))
			{
				HttpTemplate httpTemplate = getHttpTemplate(id);
				httpTemplate.EnableRedirect = true;
			}
			else
			{
				HttpConnection httpConnection = getHttpConnection(id);
				httpConnection.EnableRedirect = true;
			}

			return 0;
		}

		/// <summary>
		/// Disable cookie
		/// </summary>
		/// <param name="id"> - ID of the template or connection </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0B12ABFB, version = 150) public int sceHttpDisableCookie(int templateId)
		[HLEFunction(nid : 0x0B12ABFB, version : 150)]
		public virtual int sceHttpDisableCookie(int templateId)
		{
			return 0;
		}

		/// <summary>
		/// Enable cookie
		/// </summary>
		/// <param name="id"> - ID of the template or connection </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0DAFA58F, version = 150) public int sceHttpEnableCookie(int templateId)
		[HLEFunction(nid : 0x0DAFA58F, version : 150)]
		public virtual int sceHttpEnableCookie(int templateId)
		{
			return 0;
		}

		/// <summary>
		/// Delete content header
		/// </summary>
		/// <param name="id"> - ID of the template, connection or request </param>
		/// <param name="name"> - Name of the content </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x15540184, version = 150) public int sceHttpDeleteHeader(int templateId, int name)
		[HLEFunction(nid : 0x15540184, version : 150)]
		public virtual int sceHttpDeleteHeader(int templateId, int name)
		{
			return 0;
		}

		/// <summary>
		/// Disable redirect
		/// </summary>
		/// <param name="id"> - ID of the template or connection </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1A0EBB69, version = 150) public int sceHttpDisableRedirect(int id)
		[HLEFunction(nid : 0x1A0EBB69, version : 150)]
		public virtual int sceHttpDisableRedirect(int id)
		{
			if (isHttpTemplateId(id))
			{
				HttpTemplate httpTemplate = getHttpTemplate(id);
				httpTemplate.EnableRedirect = false;
			}
			else
			{
				HttpConnection httpConnection = getHttpConnection(id);
				httpConnection.EnableRedirect = false;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1CEDB9D4, version = 150) public int sceHttpFlushCache()
		[HLEFunction(nid : 0x1CEDB9D4, version : 150)]
		public virtual int sceHttpFlushCache()
		{
			return 0;
		}

		/// <summary>
		/// Set receive timeout
		/// </summary>
		/// <param name="id"> - ID of the template or connection </param>
		/// <param name="timeout"> - Timeout value in microseconds </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1F0FC3E3, version = 150) public int sceHttpSetRecvTimeOut(int templateId, int timeout)
		[HLEFunction(nid : 0x1F0FC3E3, version : 150)]
		public virtual int sceHttpSetRecvTimeOut(int templateId, int timeout)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2255551E, version = 150) public int sceHttpGetNetworkPspError(int connectionId, pspsharp.HLE.TPointer32 errorAddr)
		[HLEFunction(nid : 0x2255551E, version : 150)]
		public virtual int sceHttpGetNetworkPspError(int connectionId, TPointer32 errorAddr)
		{
			errorAddr.setValue(0);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x267618F4, version = 150) public int sceHttpSetAuthInfoCallback(int templateId, pspsharp.HLE.TPointer callback, int callbackArg)
		[HLEFunction(nid : 0x267618F4, version : 150)]
		public virtual int sceHttpSetAuthInfoCallback(int templateId, TPointer callback, int callbackArg)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2A6C3296, version = 150) public int sceHttpSetAuthInfoCB()
		[HLEFunction(nid : 0x2A6C3296, version : 150)]
		public virtual int sceHttpSetAuthInfoCB()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2C3C82CF, version = 150) public int sceHttpFlushAuthList()
		[HLEFunction(nid : 0x2C3C82CF, version : 150)]
		public virtual int sceHttpFlushAuthList()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3A67F306, version = 150) public int sceHttpSetCookieRecvCallback()
		[HLEFunction(nid : 0x3A67F306, version : 150)]
		public virtual int sceHttpSetCookieRecvCallback()
		{
			return 0;
		}

		/// <summary>
		/// Add content header
		/// </summary>
		/// <param name="id"> - ID of the template, connection or request </param>
		/// <param name="name"> - Name of the content </param>
		/// <param name="value"> - Value of the content </param>
		/// <param name="unknown1"> - Pass 0 </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3EABA285, version = 150) public int sceHttpAddExtraHeader(int requestId, pspsharp.HLE.PspString name, pspsharp.HLE.PspString value, int unknown1)
		[HLEFunction(nid : 0x3EABA285, version : 150)]
		public virtual int sceHttpAddExtraHeader(int requestId, PspString name, PspString value, int unknown1)
		{
			HttpRequest httpRequest = getHttpRequest(requestId);
			httpRequest.addHeader(name.String, value.String);

			return 0;
		}

		/// <summary>
		/// Create a http request.
		/// </summary>
		/// <param name="connectionid"> - ID of the connection created by sceHttpCreateConnection or sceHttpCreateConnectionWithURL </param>
		/// <param name="method"> - One of ::PspHttpMethod </param>
		/// <param name="path"> - Path to access </param>
		/// <param name="contentlength"> - Length of the content (POST method only) </param>
		/// <returns> A request ID on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x47347B50, version = 150) public int sceHttpCreateRequest(int connectionId, int method, pspsharp.HLE.PspString path, long contentLength)
		[HLEFunction(nid : 0x47347B50, version : 150)]
		public virtual int sceHttpCreateRequest(int connectionId, int method, PspString path, long contentLength)
		{
			HttpConnection httpConnection = getHttpConnection(connectionId);
			HttpRequest httpRequest = new HttpRequest();
			httpRequest.Method = method;
			httpRequest.Path = path.String;
			httpRequest.ContentLength = contentLength;
			httpConnection.addHttpRequest(httpRequest);

			return httpRequest.Id;
		}

		/// <summary>
		/// Set resolver timeout
		/// </summary>
		/// <param name="id"> - ID of the template or connection </param>
		/// <param name="timeout"> - Timeout value in microseconds </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x47940436, version = 150) public int sceHttpSetResolveTimeOut(int templateId, int timeout)
		[HLEFunction(nid : 0x47940436, version : 150)]
		public virtual int sceHttpSetResolveTimeOut(int templateId, int timeout)
		{
			return 0;
		}

		/// <summary>
		/// Get http request status code.
		/// </summary>
		/// <param name="requestid"> - ID of the request created by sceHttpCreateRequest or sceHttpCreateRequestWithURL </param>
		/// <param name="statuscode"> - The status code from the host (200 is ok, 404 is not found etc) </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4CC7D78F, version = 150) public int sceHttpGetStatusCode(int requestId, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 statusCode)
		[HLEFunction(nid : 0x4CC7D78F, version : 150)]
		public virtual int sceHttpGetStatusCode(int requestId, TPointer32 statusCode)
		{
			HttpRequest httpRequest = getHttpRequest(requestId);
			httpRequest.connect();
			statusCode.setValue(httpRequest.StatusCode);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceHttpGetStatusCode on request {0} returning statusCode={1:D}", httpRequest, statusCode.getValue()));
			}

			return 0;
		}

		/// <summary>
		/// Delete a http connection.
		/// </summary>
		/// <param name="connectionid"> - ID of the connection created by sceHttpCreateConnection or sceHttpCreateConnectionWithURL </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5152773B, version = 150) public int sceHttpDeleteConnection(int connectionId)
		[HLEFunction(nid : 0x5152773B, version : 150)]
		public virtual int sceHttpDeleteConnection(int connectionId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x54E7DF75, version = 150) public int sceHttpIsRequestInCache(int requestId, int unknown1, int unknown2)
		[HLEFunction(nid : 0x54E7DF75, version : 150)]
		public virtual int sceHttpIsRequestInCache(int requestId, int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x59E6D16F, version = 150) public int sceHttpEnableCache(int templateId)
		[HLEFunction(nid : 0x59E6D16F, version : 150)]
		public virtual int sceHttpEnableCache(int templateId)
		{
			return 0;
		}

		/// <summary>
		/// Save cookie
		/// </summary>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x76D1363B, version = 150, checkInsideInterrupt = true) public int sceHttpSaveSystemCookie()
		[HLEFunction(nid : 0x76D1363B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceHttpSaveSystemCookie()
		{
			checkHttpInit();

			if (!isSystemCookieLoaded)
			{
				return SceKernelErrors.ERROR_HTTP_SYSTEM_COOKIE_NOT_LOADED;
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7774BF4C, version = 150) public int sceHttpAddCookie(pspsharp.HLE.PspString url, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer cookieAddr, int length)
		[HLEFunction(nid : 0x7774BF4C, version : 150)]
		public virtual int sceHttpAddCookie(PspString url, TPointer cookieAddr, int length)
		{
			string cookie = cookieAddr.getStringNZ(length);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceHttpAddCookie for URL '{0}': '{1}'", url.String, cookie));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x77EE5319, version = 150) public int sceHttpLoadAuthList()
		[HLEFunction(nid : 0x77EE5319, version : 150)]
		public virtual int sceHttpLoadAuthList()
		{
			return 0;
		}

		/// <summary>
		/// Enable keep alive
		/// </summary>
		/// <param name="id"> - ID of the template or connection </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x78A0D3EC, version = 150) public int sceHttpEnableKeepAlive(int templateId)
		[HLEFunction(nid : 0x78A0D3EC, version : 150)]
		public virtual int sceHttpEnableKeepAlive(int templateId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x78B54C09, version = 150) public int sceHttpEndCache()
		[HLEFunction(nid : 0x78B54C09, version : 150)]
		public virtual int sceHttpEndCache()
		{
			return 0;
		}

		/// <summary>
		/// Set connect timeout
		/// </summary>
		/// <param name="id"> - ID of the template, connection or request </param>
		/// <param name="timeout"> - Timeout value in microseconds </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8ACD1F73, version = 150) public int sceHttpSetConnectTimeOut(int templateId, int timeout)
		[HLEFunction(nid : 0x8ACD1F73, version : 150)]
		public virtual int sceHttpSetConnectTimeOut(int templateId, int timeout)
		{
			return 0;
		}

		/// <summary>
		/// Create a http connection.
		/// </summary>
		/// <param name="templateid"> - ID of the template created by sceHttpCreateTemplate </param>
		/// <param name="host"> - Host to connect to </param>
		/// <param name="protocol"> - Pass "http" </param>
		/// <param name="port"> - Port to connect on </param>
		/// <param name="unknown1"> - Pass 0 </param>
		/// <returns> A connection ID on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8EEFD953, version = 150) public int sceHttpCreateConnection(int templateId, pspsharp.HLE.PspString host, pspsharp.HLE.PspString protocol, int port, int unknown1)
		[HLEFunction(nid : 0x8EEFD953, version : 150)]
		public virtual int sceHttpCreateConnection(int templateId, PspString host, PspString protocol, int port, int unknown1)
		{
			HttpTemplate httpTemplate = getHttpTemplate(templateId);
			HttpConnection httpConnection = new HttpConnection();
			httpConnection.setUrl(host.String, protocol.String, port);
			httpTemplate.addHttpConnection(httpConnection);

			return httpConnection.Id;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x951D310E, version = 150) public int sceHttpDisableProxyAuth()
		[HLEFunction(nid : 0x951D310E, version : 150)]
		public virtual int sceHttpDisableProxyAuth()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9668864C, version = 150) public int sceHttpSetRecvBlockSize()
		[HLEFunction(nid : 0x9668864C, version : 150)]
		public virtual int sceHttpSetRecvBlockSize()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x96F16D3E, version = 150) public int sceHttpGetCookie(pspsharp.HLE.PspString url, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextNextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer cookie, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 cookieLengthAddr, int prepare, int secure)
		[HLEFunction(nid : 0x96F16D3E, version : 150)]
		public virtual int sceHttpGetCookie(PspString url, TPointer cookie, TPointer32 cookieLengthAddr, int prepare, int secure)
		{
			return SceKernelErrors.ERROR_HTTP_NOT_FOUND;
		}

		/// <summary>
		/// Set send timeout
		/// </summary>
		/// <param name="id"> - ID of the template, connection or request </param>
		/// <param name="timeout"> - Timeout value in microseconds </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9988172D, version = 150) public int sceHttpSetSendTimeOut(int templateId, int timeout)
		[HLEFunction(nid : 0x9988172D, version : 150)]
		public virtual int sceHttpSetSendTimeOut(int templateId, int timeout)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9AFC98B2, version = 150) public int sceHttpSendRequestInCacheFirstMode()
		[HLEFunction(nid : 0x9AFC98B2, version : 150)]
		public virtual int sceHttpSendRequestInCacheFirstMode()
		{
			return 0;
		}

		/// <summary>
		/// Create a http template.
		/// </summary>
		/// <param name="agent"> - User agent </param>
		/// <param name="unknown1"> - Pass 1 </param>
		/// <param name="unknown2"> - Pass 0 </param>
		/// <returns> A template ID on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9B1F1F36, version = 150) public int sceHttpCreateTemplate(pspsharp.HLE.PspString agent, int unknown1, int unknown2)
		[HLEFunction(nid : 0x9B1F1F36, version : 150)]
		public virtual int sceHttpCreateTemplate(PspString agent, int unknown1, int unknown2)
		{
			HttpTemplate httpTemplate = new HttpTemplate();
			httpTemplate.Agent = agent.String;

			return httpTemplate.Id;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9FC5F10D, version = 150) public int sceHttpEnableAuth(int templateId)
		[HLEFunction(nid : 0x9FC5F10D, version : 150)]
		public virtual int sceHttpEnableAuth(int templateId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA4496DE5, version = 150) public int sceHttpSetRedirectCallback(int templateId, @CanBeNull pspsharp.HLE.TPointer callbackAddr, int callbackArg)
		[HLEFunction(nid : 0xA4496DE5, version : 150)]
		public virtual int sceHttpSetRedirectCallback(int templateId, TPointer callbackAddr, int callbackArg)
		{
			return 0;
		}

		/// <summary>
		/// Delete a http request.
		/// </summary>
		/// <param name="requestid"> - ID of the request created by sceHttpCreateRequest or sceHttpCreateRequestWithURL </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA5512E01, version = 150) public int sceHttpDeleteRequest(int requestId)
		[HLEFunction(nid : 0xA5512E01, version : 150)]
		public virtual int sceHttpDeleteRequest(int requestId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA6800C34, version = 150) public int sceHttpInitCache()
		[HLEFunction(nid : 0xA6800C34, version : 150)]
		public virtual int sceHttpInitCache()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAE948FEE, version = 150) public int sceHttpDisableAuth(int templateId)
		[HLEFunction(nid : 0xAE948FEE, version : 150)]
		public virtual int sceHttpDisableAuth(int templateId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB0C34B1D, version = 150) public int sceHttpSetCacheContentLengthMaxSize()
		[HLEFunction(nid : 0xB0C34B1D, version : 150)]
		public virtual int sceHttpSetCacheContentLengthMaxSize()
		{
			return 0;
		}

		/// <summary>
		/// Create a http request with url.
		/// </summary>
		/// <param name="connectionid"> - ID of the connection created by sceHttpCreateConnection or sceHttpCreateConnectionWithURL </param>
		/// <param name="method"> - One of ::PspHttpMethod </param>
		/// <param name="url"> - url to access </param>
		/// <param name="contentlength"> - Length of the content (POST method only) </param>
		/// <returns> A request ID on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB509B09E, version = 150) public int sceHttpCreateRequestWithURL(int connectionId, int method, pspsharp.HLE.PspString url, long contentLength)
		[HLEFunction(nid : 0xB509B09E, version : 150)]
		public virtual int sceHttpCreateRequestWithURL(int connectionId, int method, PspString url, long contentLength)
		{
			HttpConnection httpConnection = getHttpConnection(connectionId);
			HttpRequest httpRequest = new HttpRequest();
			httpRequest.Method = method;
			httpRequest.Url = url.String;
			httpRequest.ContentLength = contentLength;
			httpConnection.addHttpRequest(httpRequest);

			return httpRequest.Id;
		}

		/// <summary>
		/// Send a http request.
		/// </summary>
		/// <param name="requestid"> - ID of the request created by sceHttpCreateRequest or sceHttpCreateRequestWithURL </param>
		/// <param name="data"> - For POST methods specify a pointer to the post data, otherwise pass NULL </param>
		/// <param name="datasize"> - For POST methods specify the size of the post data, otherwise pass 0 </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBB70706F, version = 150) public int sceHttpSendRequest(int requestId, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer data, int dataSize)
		[HLEFunction(nid : 0xBB70706F, version : 150)]
		public virtual int sceHttpSendRequest(int requestId, TPointer data, int dataSize)
		{
			HttpRequest httpRequest = getHttpRequest(requestId);
			httpRequest.send(data.Address, dataSize);

			return 0;
		}

		/// <summary>
		/// Abort a http request.
		/// </summary>
		/// <param name="requestid"> - ID of the request created by sceHttpCreateRequest or sceHttpCreateRequestWithURL </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC10B6BD9, version = 150) public int sceHttpAbortRequest(int requestId)
		[HLEFunction(nid : 0xC10B6BD9, version : 150)]
		public virtual int sceHttpAbortRequest(int requestId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC6330B0D, version = 150) public int sceHttpChangeHttpVersion()
		[HLEFunction(nid : 0xC6330B0D, version : 150)]
		public virtual int sceHttpChangeHttpVersion()
		{
			return 0;
		}

		/// <summary>
		/// Disable keep alive
		/// </summary>
		/// <param name="id"> - ID of the template or connection </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC7EF2559, version = 150) public int sceHttpDisableKeepAlive(int templateId)
		[HLEFunction(nid : 0xC7EF2559, version : 150)]
		public virtual int sceHttpDisableKeepAlive(int templateId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC98CBBA7, version = 150) public int sceHttpSetResHeaderMaxSize()
		[HLEFunction(nid : 0xC98CBBA7, version : 150)]
		public virtual int sceHttpSetResHeaderMaxSize()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCCBD167A, version = 150) public int sceHttpDisableCache(int templateId)
		[HLEFunction(nid : 0xCCBD167A, version : 150)]
		public virtual int sceHttpDisableCache(int templateId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCDB0DC58, version = 150) public int sceHttpEnableProxyAuth()
		[HLEFunction(nid : 0xCDB0DC58, version : 150)]
		public virtual int sceHttpEnableProxyAuth()
		{
			return 0;
		}

		/// <summary>
		/// Create a http connection to a url.
		/// </summary>
		/// <param name="templateid"> - ID of the template created by sceHttpCreateTemplate </param>
		/// <param name="url"> - url to connect to </param>
		/// <param name="unknown1"> - Pass 0 </param>
		/// <returns> A connection ID on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCDF8ECB9, version = 150) public int sceHttpCreateConnectionWithURL(int templateId, pspsharp.HLE.PspString url, int unknown1)
		[HLEFunction(nid : 0xCDF8ECB9, version : 150)]
		public virtual int sceHttpCreateConnectionWithURL(int templateId, PspString url, int unknown1)
		{
			HttpTemplate httpTemplate = getHttpTemplate(templateId);
			HttpConnection httpConnection = new HttpConnection();
			httpConnection.Url = url.String;
			httpTemplate.addHttpConnection(httpConnection);

			return httpConnection.Id;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD081EC8F, version = 150) public int sceHttpGetNetworkErrno(int requestId, pspsharp.HLE.TPointer32 errno)
		[HLEFunction(nid : 0xD081EC8F, version : 150)]
		public virtual int sceHttpGetNetworkErrno(int requestId, TPointer32 errno)
		{
			errno.setValue(0);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD70D4847, version = 150) public int sceHttpGetProxy()
		[HLEFunction(nid : 0xD70D4847, version : 150)]
		public virtual int sceHttpGetProxy()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDB266CCF, version = 150) public int sceHttpGetAllHeader(int requestId, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 headerAddr, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 headerLengthAddr)
		[HLEFunction(nid : 0xDB266CCF, version : 150)]
		public virtual int sceHttpGetAllHeader(int requestId, TPointer32 headerAddr, TPointer32 headerLengthAddr)
		{
			HttpRequest httpRequest = getHttpRequest(requestId);
			httpRequest.connect();
			string allHeaders = httpRequest.AllHeaders;
			if (string.ReferenceEquals(allHeaders, null))
			{
				return -1;
			}

			int addr = TempMemory;
			Utilities.writeStringZ(Memory.Instance, addr, allHeaders);
			headerAddr.setValue(addr);
			headerLengthAddr.setValue(allHeaders.Length);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceHttpGetAllHeader returning at 0x{0:X8}: {1}", addr, Utilities.getMemoryDump(addr, headerLengthAddr.getValue())));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDD6E7857, version = 150) public int sceHttpSaveAuthList()
		[HLEFunction(nid : 0xDD6E7857, version : 150)]
		public virtual int sceHttpSaveAuthList()
		{
			return 0;
		}

		/// <summary>
		/// Read a http request response.
		/// </summary>
		/// <param name="requestid"> - ID of the request created by sceHttpCreateRequest or sceHttpCreateRequestWithURL </param>
		/// <param name="data"> - Buffer for the response data to be stored </param>
		/// <param name="datasize"> - Size of the buffer </param>
		/// <returns> The size read into the data buffer, 0 if there is no more data, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEDEEB999, version = 150) public int sceHttpReadData(int requestId, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.returnValue, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer data, int dataSize)
		[HLEFunction(nid : 0xEDEEB999, version : 150)]
		public virtual int sceHttpReadData(int requestId, TPointer data, int dataSize)
		{
			HttpRequest httpRequest = getHttpRequest(requestId);
			httpRequest.connect();
			int readSize = httpRequest.readData(data.Address, dataSize);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceHttpReadData returning 0x{0:X}: {1}", readSize, Utilities.getMemoryDump(data.Address, readSize)));
			}

			return readSize;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF0F46C62, version = 150) public int sceHttpSetProxy()
		[HLEFunction(nid : 0xF0F46C62, version : 150)]
		public virtual int sceHttpSetProxy()
		{
			return 0;
		}

		/// <summary>
		/// Load cookie
		/// </summary>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF1657B22, version = 150, checkInsideInterrupt = true) public int sceHttpLoadSystemCookie()
		[HLEFunction(nid : 0xF1657B22, version : 150, checkInsideInterrupt : true)]
		public virtual int sceHttpLoadSystemCookie()
		{
			checkHttpInit();

			if (isSystemCookieLoaded)
			{ // The system's cookie list can only be loaded once per session.
				return SceKernelErrors.ERROR_HTTP_ALREADY_INIT;
			}
			else if (maxMemSize < PSP_HTTP_SYSTEM_COOKIE_HEAP_SIZE)
			{
				return SceKernelErrors.ERROR_HTTP_NO_MEMORY;
			}
			else
			{
				isSystemCookieLoaded = true;
				return 0;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF49934F6, version = 150) public int sceHttpSetMallocFunction(pspsharp.HLE.TPointer function1, pspsharp.HLE.TPointer function2, pspsharp.HLE.TPointer function3)
		[HLEFunction(nid : 0xF49934F6, version : 150)]
		public virtual int sceHttpSetMallocFunction(TPointer function1, TPointer function2, TPointer function3)
		{
			return 0;
		}

		/// <summary>
		/// Delete a http template.
		/// </summary>
		/// <param name="templateid"> - ID of the template created by sceHttpCreateTemplate </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFCF8C055, version = 150) public int sceHttpDeleteTemplate(int templateId)
		[HLEFunction(nid : 0xFCF8C055, version : 150)]
		public virtual int sceHttpDeleteTemplate(int templateId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x87F1E666, version = 150) public int sceHttp_87F1E666(int templateId, int unknown)
		[HLEFunction(nid : 0x87F1E666, version : 150)]
		public virtual int sceHttp_87F1E666(int templateId, int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3C478044, version = 150) public int sceHttp_3C478044(int templateId, int unknown)
		[HLEFunction(nid : 0x3C478044, version : 150)]
		public virtual int sceHttp_3C478044(int templateId, int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x739C2D79, version = 150) public int sceHttpInitExternalCache()
		[HLEFunction(nid : 0x739C2D79, version : 150)]
		public virtual int sceHttpInitExternalCache()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA461A167, version = 150) public int sceHttpEndExternalCache()
		[HLEFunction(nid : 0xA461A167, version : 150)]
		public virtual int sceHttpEndExternalCache()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8046E250, version = 150) public int sceHttpEnableExternalCache()
		[HLEFunction(nid : 0x8046E250, version : 150)]
		public virtual int sceHttpEnableExternalCache()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB0257723, version = 150) public int sceHttpFlushExternalCache()
		[HLEFunction(nid : 0xB0257723, version : 150)]
		public virtual int sceHttpFlushExternalCache()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x457D221D, version = 150) public int sceHttpFlushCookie()
		[HLEFunction(nid : 0x457D221D, version : 150)]
		public virtual int sceHttpFlushCookie()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4E4A284A, version = 150) public int sceHttpCloneTemplate(int templateId)
		[HLEFunction(nid : 0x4E4A284A, version : 150)]
		public virtual int sceHttpCloneTemplate(int templateId)
		{
			HttpTemplate clonedHttpTemplate = new HttpTemplate();
			HttpTemplate httpTemplate = getHttpTemplate(templateId);
			clonedHttpTemplate.Agent = httpTemplate.Agent;

			return clonedHttpTemplate.Id;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD80BE761, version = 150) public int sceHttp_D80BE761(int templateId)
		[HLEFunction(nid : 0xD80BE761, version : 150)]
		public virtual int sceHttp_D80BE761(int templateId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA909F2AE, version = 150) public int sceHttp_A909F2AE1()
		[HLEFunction(nid : 0xA909F2AE, version : 150)]
		public virtual int sceHttp_A909F2AE1()
		{
			return 0;
		}
	}
}