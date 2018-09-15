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
	//using Logger = org.apache.log4j.Logger;

	using SHA256 = pspsharp.crypto.SHA256;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Utilities = pspsharp.util.Utilities;

	public class sceSha256 : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceSha256");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5368F1BC, version = 370) public int sceSha256BlockInit(pspsharp.HLE.TPointer sha)
		[HLEFunction(nid : 0x5368F1BC, version : 370)]
		public virtual int sceSha256BlockInit(TPointer sha)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7310DDCF, version = 370) public int sceSha256BlockUpdate(pspsharp.HLE.TPointer sha, pspsharp.HLE.TPointer data, int Length)
		[HLEFunction(nid : 0x7310DDCF, version : 370)]
		public virtual int sceSha256BlockUpdate(TPointer sha, TPointer data, int Length)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x82C67FB3, version = 370) public int sceSha256BlockResult(pspsharp.HLE.TPointer sha, pspsharp.HLE.TPointer digest)
		[HLEFunction(nid : 0x82C67FB3, version : 370)]
		public virtual int sceSha256BlockResult(TPointer sha, TPointer digest)
		{
			return 0;
		}

		[HLEFunction(nid : 0x318A350C, version : 370)]
		public virtual int sceSha256Digest(TPointer data, int Length, TPointer digest)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceSha256Digest data:{0}", Utilities.getMemoryDump(data.Address, Length)));
			}

			// Read in the source data.
			sbyte[] b = new sbyte[Length];
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(data.Address, Length, 1);
			for (int i = 0; i < Length; i++)
			{
				b[i] = (sbyte) memoryReader.readNext();
			}

			// Calculate SHA-256.
			SHA256 sha256 = new SHA256();
			sbyte[] d = sha256.doSHA256(b, Length);

			// Write back the resulting digest.
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(digest.Address, 0x20, 1);
			for (int i = 0; i < 0x20; i++)
			{
				memoryWriter.writeNext((sbyte) d[i]);
			}

			return 0;
		}
	}

}