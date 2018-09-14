using System;
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
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class sceParseHttp : HLEModule
	{
		public static Logger log = Modules.getLogger("sceParseHttp");

		private string getHeaderString(IMemoryReader memoryReader)
		{
			StringBuilder line = new StringBuilder();
			while (true)
			{
				int c = memoryReader.readNext();
				if (c == '\n' || c == '\r')
				{
					break;
				}
				line.Append((char) c);
			}

			return line.ToString();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAD7BFDEF, version = 150) public int sceParseHttpResponseHeader(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer header, int headerLength, pspsharp.HLE.PspString fieldName, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 valueAddr, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 valueLength)
		[HLEFunction(nid : 0xAD7BFDEF, version : 150)]
		public virtual int sceParseHttpResponseHeader(TPointer header, int headerLength, PspString fieldName, TPointer32 valueAddr, TPointer32 valueLength)
		{
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(header.Address, headerLength, 1);
			int endAddress = header.Address + headerLength;

			bool found = false;
			while (memoryReader.CurrentAddress < endAddress)
			{
				int addr = memoryReader.CurrentAddress;
				string headerString = getHeaderString(memoryReader);
				string[] fields = headerString.Split(" *: *", 2);
				if (fields != null && fields.Length == 2)
				{
					if (fields[0].Equals(fieldName.String, StringComparison.OrdinalIgnoreCase))
					{
						addr += fields[0].Length;
						Memory mem = header.Memory;
						int c;
						while (true)
						{
							c = mem.read8(addr);
							if (c != ' ')
							{
								break;
							}
							addr++;
						}
						c = mem.read8(addr++);
						if (c == ':')
						{
							while (true)
							{
								c = mem.read8(addr);
								if (c != ' ')
								{
									break;
								}
								addr++;
							}

							valueLength.setValue(memoryReader.CurrentAddress - addr - 1);
							valueAddr.setValue(addr);
							found = true;
							if (log.DebugEnabled)
							{
								log.debug(string.Format("sceParseHttpResponseHeader returning valueLength=0x{0:X}: {1}", valueLength.getValue(), Utilities.getMemoryDump(valueAddr.getValue(), valueLength.getValue())));
							}
							break;
						}
					}
				}
			}

			if (!found)
			{
				valueAddr.setValue(0);
				valueLength.setValue(0);
				return SceKernelErrors.ERROR_PARSE_HTTP_NOT_FOUND;
			}

			return memoryReader.CurrentAddress - 1 - header.Address;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8077A433, version = 150) public int sceParseHttpStatusLine(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer header, int headerLength, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 httpVersionMajorAddr, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 httpVersionMinorAddr, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 httpStatusCodeAddr, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 httpStatusCommentAddr, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 httpStatusCommentLengthAddr)
		[HLEFunction(nid : 0x8077A433, version : 150)]
		public virtual int sceParseHttpStatusLine(TPointer header, int headerLength, TPointer32 httpVersionMajorAddr, TPointer32 httpVersionMinorAddr, TPointer32 httpStatusCodeAddr, TPointer32 httpStatusCommentAddr, TPointer32 httpStatusCommentLengthAddr)
		{
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(header.Address, headerLength, 1);
			string headerString = getHeaderString(memoryReader);
			Pattern pattern = Pattern.compile("HTTP/(\\d)\\.(\\d)\\s+(\\d+)(.*)");
			Matcher matcher = pattern.matcher(headerString);
			if (!matcher.matches())
			{
				return -1;
			}

			int httpVersionMajor = int.Parse(matcher.group(1));
			int httpVersionMinor = int.Parse(matcher.group(2));
			int httpStatusCode = int.Parse(matcher.group(3));
			string httpStatusComment = matcher.group(4);

			httpVersionMajorAddr.setValue(httpVersionMajor);
			httpVersionMinorAddr.setValue(httpVersionMinor);
			httpStatusCodeAddr.setValue(httpStatusCode);
			httpStatusCommentAddr.setValue(header.Address + headerString.IndexOf(httpStatusComment, StringComparison.Ordinal));
			httpStatusCommentLengthAddr.setValue(httpStatusComment.Length);

			return 0;
		}
	}

}