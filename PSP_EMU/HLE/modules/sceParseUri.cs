using System;

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

	using Logger = org.apache.log4j.Logger;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using pspParsedUri = pspsharp.HLE.kernel.types.pspParsedUri;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Utilities = pspsharp.util.Utilities;

	public class sceParseUri : HLEModule
	{
		public static Logger log = Modules.getLogger("sceParseUri");
		private static readonly bool[] escapeCharTable = new bool[] {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, false, true, true, false, false, true, false, false, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, true, true, true, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true};
		private static readonly int[] hexTable = new int[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

		protected internal virtual int getHexValue(int hexChar)
		{
			if (hexChar >= '0' && hexChar <= '9')
			{
				return hexChar - '0';
			}
			if (hexChar >= 'A' && hexChar <= 'F')
			{
				return hexChar - 'A' + 10;
			}
			if (hexChar >= 'a' && hexChar <= 'f')
			{
				return hexChar - 'a' + 10;
			}

			return 0;
		}

		private int addString(TPointer workArea, int workAreaSize, int offset, string s)
		{
			if (string.ReferenceEquals(s, null))
			{
				s = "";
			}

			int length = s.Length + 1;
			if (offset + length > workAreaSize)
			{
				length = workAreaSize - offset;
				if (length <= 0)
				{
					return offset;
				}
			}

			workArea.setStringNZ(offset, length, s);

			return offset + length;
		}

		private string getUriComponent(int componentAddr, int flags, int flag)
		{
			if ((flags & flag) == 0 || componentAddr == 0)
			{
				return null;
			}

			return Utilities.readStringZ(componentAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x568518C9, version = 150) public int sceUriParse(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=44, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer parsedUriArea, pspsharp.HLE.PspString url, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextNextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer workArea, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 workAreaSizeAddr, int workAreaSize)
		[HLEFunction(nid : 0x568518C9, version : 150)]
		public virtual int sceUriParse(TPointer parsedUriArea, PspString url, TPointer workArea, TPointer32 workAreaSizeAddr, int workAreaSize)
		{
			if (parsedUriArea.Null || workArea.Null)
			{
				// The required workArea size if maximum the size if the URL + 7 times the null-byte
				// for string termination.
				workAreaSizeAddr.setValue(url.String.Length + 7);
				return 0;
			}

			string urlString = sceHttp.patchUrl(url.String);
			if (!urlString.Equals(url.String))
			{
				log.info(string.Format("sceUriParse patched URL '{0}' into '{1}'", url.String, urlString));
			}

			// Parse the URL into URI components
			URI uri;
			try
			{
				uri = new URI(urlString);
			}
			catch (URISyntaxException e)
			{
				log.error("parsedUriArea", e);
				return -1;
			}

			// Parsing of the userInfo in the format "<userName>:<password>"
			string userInfo = uri.UserInfo;
			string userInfoUserName = userInfo;
			string userInfoPassword = "";
			if (!string.ReferenceEquals(userInfo, null))
			{
				int userInfoColon = userInfo.IndexOf(":", StringComparison.Ordinal);
				if (userInfoColon >= 0)
				{
					userInfoUserName = userInfo.Substring(0, userInfoColon);
					userInfoPassword = userInfo.Substring(userInfoColon + 1);
				}
			}

			string query = uri.Query;
			if (!string.ReferenceEquals(query, null) && query.Length > 0)
			{
				query = "?" + query;
			}

			pspParsedUri parsedUri = new pspParsedUri();
			int offset = 0;

			if (uri.SchemeSpecificPart != null && uri.SchemeSpecificPart.StartsWith("//"))
			{
				parsedUri.noSlash = 0;
			}
			else
			{
				parsedUri.noSlash = 1;
			}
			// Store the URI components in sequence into workArea
			// and store the respective addresses into the parsedUri structure.
			parsedUri.schemeAddr = workArea.Address + offset;
			offset = addString(workArea, workAreaSize, offset, uri.Scheme);

			parsedUri.userInfoUserNameAddr = workArea.Address + offset;
			offset = addString(workArea, workAreaSize, offset, userInfoUserName);

			parsedUri.userInfoPasswordAddr = workArea.Address + offset;
			offset = addString(workArea, workAreaSize, offset, userInfoPassword);

			parsedUri.hostAddr = workArea.Address + offset;
			offset = addString(workArea, workAreaSize, offset, uri.Host);

			parsedUri.pathAddr = workArea.Address + offset;
			offset = addString(workArea, workAreaSize, offset, uri.Path);

			parsedUri.queryAddr = workArea.Address + offset;
			offset = addString(workArea, workAreaSize, offset, query);

			parsedUri.fragmentAddr = workArea.Address + offset;
			offset = addString(workArea, workAreaSize, offset, uri.Fragment);

			if (uri.Port < 0)
			{
				if ("http".Equals(uri.Scheme))
				{
					parsedUri.port = 80;
				}
				else if ("https".Equals(uri.Scheme))
				{
					parsedUri.port = 443;
				}
				else
				{
					parsedUri.port = 0;
				}
			}
			else
			{
				parsedUri.port = uri.Port;
			}

			workAreaSizeAddr.setValue(offset);
			parsedUri.write(parsedUriArea);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7EE318AF, version = 150) public int sceUriBuild(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextNextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer workArea, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 workAreaSizeAddr, int workAreaSize, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=44, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer parsedUriAddr, int flags)
		[HLEFunction(nid : 0x7EE318AF, version : 150)]
		public virtual int sceUriBuild(TPointer workArea, TPointer32 workAreaSizeAddr, int workAreaSize, TPointer parsedUriAddr, int flags)
		{
			pspParsedUri parsedUri = new pspParsedUri();
			parsedUri.read(parsedUriAddr);

			// Extract the URI components from the parseUri structure
			string scheme = getUriComponent(parsedUri.schemeAddr, flags, 0x1);
			string userInfoUserName = getUriComponent(parsedUri.userInfoUserNameAddr, flags, 0x10);
			string userInfoPassword = getUriComponent(parsedUri.userInfoPasswordAddr, flags, 0x20);
			string host = getUriComponent(parsedUri.hostAddr, flags, 0x2);
			string path = getUriComponent(parsedUri.pathAddr, flags, 0x8);
			string query = getUriComponent(parsedUri.queryAddr, flags, 0x40);
			string fragment = getUriComponent(parsedUri.fragmentAddr, flags, 0x80);
			int port = (flags & 0x4) != 0 ? parsedUri.port : -1;

			// Build the complete URI
			string uri = "";
			if (!string.ReferenceEquals(scheme, null) && scheme.Length > 0)
			{
				uri += scheme + ":";
			}
			if (parsedUri.noSlash == 0)
			{
				uri += "//";
			}
			if (!string.ReferenceEquals(userInfoUserName, null))
			{
				uri += userInfoUserName;
			}
			if (!string.ReferenceEquals(userInfoPassword, null) && userInfoPassword.Length > 0)
			{
				uri += ":" + userInfoPassword;
			}
			if (!string.ReferenceEquals(host, null))
			{
				uri += host;
			}
			if (port > 0)
			{
				int defaultPort = -1;
				if (parsedUri.schemeAddr != 0)
				{
					string protocol = Utilities.readStringZ(parsedUri.schemeAddr);
					defaultPort = Utilities.getDefaultPortForProtocol(protocol);
				}
				if (port > 0 && port != defaultPort)
				{
					uri += ":" + port;
				}
			}
			if (!string.ReferenceEquals(path, null))
			{
				uri += path;
			}
			if (!string.ReferenceEquals(query, null))
			{
				uri += query;
			}
			if (!string.ReferenceEquals(fragment, null))
			{
				uri += fragment;
			}

			// Return the URI and its size
			if (workArea.NotNull)
			{
				workArea.setStringNZ(workAreaSize, uri);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceUriBuild returning '{0}'", uri));
				}
			}
			workAreaSizeAddr.setValue(uri.Length + 1);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x49E950EC, version = 150) public int sceUriEscape(@CanBeNull pspsharp.HLE.TPointer escapedAddr, @CanBeNull pspsharp.HLE.TPointer32 escapedLengthAddr, int escapedBufferLength, pspsharp.HLE.TPointer source)
		[HLEFunction(nid : 0x49E950EC, version : 150)]
		public virtual int sceUriEscape(TPointer escapedAddr, TPointer32 escapedLengthAddr, int escapedBufferLength, TPointer source)
		{
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(source.Address, 1);
			IMemoryWriter memoryWriter = null;
			if (escapedAddr.NotNull)
			{
				memoryWriter = MemoryWriter.getMemoryWriter(escapedAddr.Address, escapedBufferLength, 1);
			}
			int escapedLength = 0;
			while (true)
			{
				int c = memoryReader.readNext();
				if (c == 0)
				{
					if (escapedAddr.NotNull)
					{
						if (escapedLength < escapedBufferLength)
						{
							memoryWriter.writeNext(c);
						}
					}
					escapedLength++;
					break;
				}
				if (escapeCharTable[c])
				{
					if (escapedAddr.NotNull)
					{
						if (escapedLength + 3 > escapedBufferLength)
						{
							break;
						}
						memoryWriter.writeNext('%');
						memoryWriter.writeNext(hexTable[c >> 4]);
						memoryWriter.writeNext(hexTable[c & 0x0F]);
					}
					escapedLength += 3;
				}
				else
				{
					if (escapedAddr.NotNull)
					{
						if (escapedLength + 1 > escapedBufferLength)
						{
							break;
						}
						memoryWriter.writeNext(c);
					}
					escapedLength++;
				}
			}
			if (memoryWriter != null)
			{
				memoryWriter.flush();
			}
			escapedLengthAddr.setValue(escapedLength);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x062BB07E, version = 150) public int sceUriUnescape(@CanBeNull pspsharp.HLE.TPointer unescapedAddr, @CanBeNull pspsharp.HLE.TPointer32 unescapedLengthAddr, int unescapedBufferLength, pspsharp.HLE.TPointer source)
		[HLEFunction(nid : 0x062BB07E, version : 150)]
		public virtual int sceUriUnescape(TPointer unescapedAddr, TPointer32 unescapedLengthAddr, int unescapedBufferLength, TPointer source)
		{
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(source.Address, 1);
			IMemoryWriter memoryWriter = null;
			if (unescapedAddr.NotNull)
			{
				memoryWriter = MemoryWriter.getMemoryWriter(unescapedAddr.Address, unescapedBufferLength, 1);
			}
			int unescapedLength = 0;
			while (true)
			{
				int c = memoryReader.readNext();
				if (c == 0)
				{
					if (unescapedAddr.NotNull)
					{
						if (unescapedLength < unescapedBufferLength)
						{
							memoryWriter.writeNext(c);
						}
					}
					unescapedLength++;
					break;
				}
				if (unescapedAddr.NotNull)
				{
					if (unescapedLength + 1 > unescapedBufferLength)
					{
						break;
					}
					if (c == '%')
					{
						int hex1 = memoryReader.readNext();
						int hex2 = memoryReader.readNext();
						c = (getHexValue(hex1) << 4) + getHexValue(hex2);
					}
					// Remark: '+' sign is not unescaped to ' ' by this function
					memoryWriter.writeNext(c);
				}
				unescapedLength++;
			}
			if (memoryWriter != null)
			{
				memoryWriter.flush();
			}
			unescapedLengthAddr.setValue(unescapedLength);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8885A782, version = 150) public int sceUriSweepPath(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextNextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer outputAddr, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer inputAddr, int length)
		[HLEFunction(nid : 0x8885A782, version : 150)]
		public virtual int sceUriSweepPath(TPointer outputAddr, TPointer inputAddr, int length)
		{
			// TODO Implemented URI path sweeping...
			outputAddr.memcpy(inputAddr.Address, length);

			return 0;
		}
	}

}