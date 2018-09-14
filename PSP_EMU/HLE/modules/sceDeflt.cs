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


	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using MemoryInputStream = pspsharp.util.MemoryInputStream;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class sceDeflt : HLEModule
	{
		public static Logger log = Modules.getLogger("sceDeflt");
		protected internal const int GZIP_MAGIC = 0x8B1F;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2EE39A64, version = 150) public int sceZlibAdler32()
		[HLEFunction(nid : 0x2EE39A64, version : 150)]
		public virtual int sceZlibAdler32()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x44054E03, version = 150) public int sceDeflateDecompress()
		[HLEFunction(nid : 0x44054E03, version : 150)]
		public virtual int sceDeflateDecompress()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x6DBCF897, version = 150) public int sceGzipDecompress(pspsharp.HLE.TPointer outBufferAddr, int outBufferLength, pspsharp.HLE.TPointer inBufferAddr, @CanBeNull pspsharp.HLE.TPointer32 crc32Addr)
		[HLEFunction(nid : 0x6DBCF897, version : 150)]
		public virtual int sceGzipDecompress(TPointer outBufferAddr, int outBufferLength, TPointer inBufferAddr, TPointer32 crc32Addr)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceGzipDecompress: {0}", Utilities.getMemoryDump(inBufferAddr.Address, 16)));
			}

			int result;
			CRC32 crc32 = new CRC32();
			sbyte[] buffer = new sbyte[4096];
			try
			{
				// Using a GZIPInputStream instead of an Inflater because the GZIPInputStream
				// is skipping the GZIP header and this should be done manually with an Inflater.
				GZIPInputStream @is = new GZIPInputStream(new MemoryInputStream(inBufferAddr.Address));
				IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(outBufferAddr.Address, outBufferLength, 1);
				int decompressedLength = 0;
				while (decompressedLength < outBufferLength)
				{
					int length = @is.read(buffer);
					if (length < 0)
					{
						// End of GZIP stream
						break;
					}
					if (decompressedLength + length > outBufferLength)
					{
						log.warn(string.Format("sceGzipDecompress : decompress buffer too small inBuffer={0}, outLength={1:D}", inBufferAddr, outBufferLength));
						@is.close();
						return SceKernelErrors.ERROR_INVALID_SIZE;
					}

					crc32.update(buffer, 0, length);

					for (int i = 0; i < length; i++)
					{
						memoryWriter.writeNext(buffer[i] & 0xFF);
					}
					decompressedLength += length;
				}
				@is.close();
				memoryWriter.flush();
				result = decompressedLength;
			}
			catch (IOException e)
			{
				log.error("sceGzipDecompress", e);
				return SceKernelErrors.ERROR_INVALID_FORMAT;
			}
			crc32Addr.setValue((int) crc32.Value);

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB767F9A0, version = 150) public int sceGzipGetComment()
		[HLEFunction(nid : 0xB767F9A0, version : 150)]
		public virtual int sceGzipGetComment()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0BA3B9CC, version = 150) public int sceGzipGetCompressedData()
		[HLEFunction(nid : 0x0BA3B9CC, version : 150)]
		public virtual int sceGzipGetCompressedData()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8AA82C92, version = 150) public int sceGzipGetInfo()
		[HLEFunction(nid : 0x8AA82C92, version : 150)]
		public virtual int sceGzipGetInfo()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x106A3552, version = 150) public int sceGzipGetName()
		[HLEFunction(nid : 0x106A3552, version : 150)]
		public virtual int sceGzipGetName()
		{
			return 0;
		}

		[HLEFunction(nid : 0x1B5B82BC, version : 150)]
		public virtual bool sceGzipIsValid(TPointer gzipData)
		{
			int magic = gzipData.Value16 & 0xFFFF;
			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceGzipIsValid gzipData:{0}", Utilities.getMemoryDump(gzipData.Address, 16)));
			}

			return magic == GZIP_MAGIC;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA9E4FB28, version = 150) public int sceZlibDecompress(pspsharp.HLE.TPointer outBufferAddr, int outBufferLength, pspsharp.HLE.TPointer inBufferAddr, @CanBeNull pspsharp.HLE.TPointer32 crc32Addr)
		[HLEFunction(nid : 0xA9E4FB28, version : 150)]
		public virtual int sceZlibDecompress(TPointer outBufferAddr, int outBufferLength, TPointer inBufferAddr, TPointer32 crc32Addr)
		{
			sbyte[] inBuffer = new sbyte[4096];
			sbyte[] outBuffer = new sbyte[4096];
			int inBufferPtr = 0;
			IMemoryReader reader = MemoryReader.getMemoryReader(inBufferAddr.Address, 1);
			IMemoryWriter writer = MemoryWriter.getMemoryWriter(outBufferAddr.Address, outBufferLength, 1);
			CRC32 crc32 = new CRC32();
			Inflater inflater = new Inflater();

			while (!inflater.finished())
			{
				if (inflater.needsInput())
				{
					for (inBufferPtr = 0; inBufferPtr < inBuffer.Length; ++inBufferPtr)
					{
						inBuffer[inBufferPtr] = (sbyte) reader.readNext();
					}
					inflater.Input = inBuffer;
				}

				try
				{
					int count = inflater.inflate(outBuffer);

					if (inflater.TotalOut > outBufferLength)
					{
						log.warn(string.Format("sceZlibDecompress : zlib decompress buffer too small inBuffer={0}, outLength={1:D}", inBufferAddr, outBufferLength));
						return SceKernelErrors.ERROR_INVALID_SIZE;
					}
					crc32.update(outBuffer, 0, count);
					for (int i = 0; i < count; ++i)
					{
						writer.writeNext(outBuffer[i] & 0xFF);
					}
				}
				catch (DataFormatException)
				{
					log.warn(string.Format("sceZlibDecompress : malformed zlib stream inBuffer={0}", inBufferAddr));
					return SceKernelErrors.ERROR_INVALID_FORMAT;
				}
			}
			writer.flush();

			crc32Addr.setValue((int) crc32.Value);

			return inflater.TotalOut;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6A548477, version = 150) public int sceZlibGetCompressedData()
		[HLEFunction(nid : 0x6A548477, version : 150)]
		public virtual int sceZlibGetCompressedData()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAFE01FD3, version = 150) public int sceZlibGetInfo()
		[HLEFunction(nid : 0xAFE01FD3, version : 150)]
		public virtual int sceZlibGetInfo()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE46EB986, version = 150) public int sceZlibIsValid()
		[HLEFunction(nid : 0xE46EB986, version : 150)]
		public virtual int sceZlibIsValid()
		{
			return 0;
		}
	}
}