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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.crypto.PreDecrypt.preDecrypt;

	//using Logger = org.apache.log4j.Logger;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using CryptoEngine = pspsharp.crypto.CryptoEngine;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Utilities = pspsharp.util.Utilities;

	public class semaphore : HLEModule
	{
		//public static Logger log = Modules.getLogger("semaphore");

		public virtual int hleUtilsBufferCopyWithRange(sbyte[] @out, int outOffset, int outSize, sbyte[] @in, int inOffset, int inSize, int cmd)
		{
			int result = 0;
			if (preDecrypt(@out, outOffset, outSize, @in, inOffset, inSize, cmd))
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleUtilsBufferCopyWithRange using pre-decrypted data"));
				}
			}
			else
			{
				// Call the KIRK engine to perform the given command
				ByteBuffer outBuffer = null;
				if (@out != null)
				{
					outBuffer = ByteBuffer.wrap(@out).order(ByteOrder.LITTLE_ENDIAN);
					outBuffer.position(outOffset);
				}

				ByteBuffer inBuffer = null;
				if (@in != null)
				{
					inBuffer = ByteBuffer.wrap(@in).order(ByteOrder.LITTLE_ENDIAN);
					inBuffer.position(inOffset);
				}
				int inSizeAligned = Utilities.alignUp(inSize, 15);

				CryptoEngine crypto = new CryptoEngine();
				result = crypto.KIRKEngine.hleUtilsBufferCopyWithRange(outBuffer, outSize, inBuffer, inSizeAligned, inSize, cmd);
				if (result != 0)
				{
					Console.WriteLine(string.Format("hleUtilsBufferCopyWithRange cmd=0x{0:X} returned 0x{1:X}", cmd, result));
				}
			}

			return result;
		}

		public virtual int hleUtilsBufferCopyWithRange(TPointer outAddr, int outSize, TPointer inAddr, int inSize, int cmd)
		{
			int originalInSize = inSize;

			// The input size needs for some KIRK commands to be 16-bytes aligned
			inSize = Utilities.alignUp(inSize, 15);

			// Read the whole input buffer, including a possible header
			// (up to 144 bytes, depending on the KIRK command)
			sbyte[] inBytes = new sbyte[inSize + 144]; // Up to 144 bytes header
			IMemoryReader memoryReaderIn = MemoryReader.getMemoryReader(inAddr, inSize, 1);
			for (int i = 0; i < inSize; i++)
			{
				inBytes[i] = (sbyte) memoryReaderIn.readNext();
			}

			// Some KIRK commands (e.g. PSP_KIRK_CMD_SHA1_HASH) only update a part of the output buffer.
			// Read the whole output buffer so that it can be updated completely after the KIRK call.
			sbyte[] outBytes = new sbyte[Utilities.alignUp(outSize, 15)];
			IMemoryReader memoryReaderOut = MemoryReader.getMemoryReader(outAddr, outBytes.Length, 1);
			for (int i = 0; i < outBytes.Length; i++)
			{
				outBytes[i] = (sbyte) memoryReaderOut.readNext();
			}

			int result = hleUtilsBufferCopyWithRange(outBytes, 0, outSize, inBytes, 0, originalInSize, cmd);

			// Write back the whole output buffer to the memory.
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(outAddr, outSize, 1);
			for (int i = 0; i < outSize; i++)
			{
				memoryWriter.writeNext(outBytes[i] & 0xFF);
			}
			memoryWriter.flush();

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x4C537C72, version = 150) public int sceUtilsBufferCopyWithRange(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out, maxDumpLength=256) pspsharp.HLE.TPointer outAddr, int outSize, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in, maxDumpLength=256) pspsharp.HLE.TPointer inAddr, int inSize, int cmd)
		[HLEFunction(nid : 0x4C537C72, version : 150)]
		public virtual int sceUtilsBufferCopyWithRange(TPointer outAddr, int outSize, TPointer inAddr, int inSize, int cmd)
		{
			hleUtilsBufferCopyWithRange(outAddr, outSize, inAddr, inSize, cmd);
			// Fake a successful operation
			return 0;
		}

		[HLEFunction(nid : 0x77E97079, version : 150)]
		public virtual int sceUtilsBufferCopyByPollingWithRange(TPointer outAddr, int outSize, TPointer inAddr, int inSize, int cmd)
		{
			return sceUtilsBufferCopyWithRange(outAddr, outSize, inAddr, inSize, cmd);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x00EEC06A, version = 150) public int sceUtilsBufferCopy(pspsharp.HLE.TPointer outAddr, pspsharp.HLE.TPointer inAddr, int cmd)
		[HLEFunction(nid : 0x00EEC06A, version : 150)]
		public virtual int sceUtilsBufferCopy(TPointer outAddr, TPointer inAddr, int cmd)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8EEB7BF2, version = 150) public int sceUtilsBufferCopyByPolling(pspsharp.HLE.TPointer outAddr, pspsharp.HLE.TPointer inAddr, int cmd)
		[HLEFunction(nid : 0x8EEB7BF2, version : 150)]
		public virtual int sceUtilsBufferCopyByPolling(TPointer outAddr, TPointer inAddr, int cmd)
		{
			return 0;
		}
	}

}