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
namespace pspsharp.filesystems.umdiso
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Tlzrc.lzrc_decompress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.endianSwap32;


	using AMCTRL = pspsharp.crypto.AMCTRL;
	using BBCipher_Ctx = pspsharp.crypto.AMCTRL.BBCipher_Ctx;
	using BBMac_Ctx = pspsharp.crypto.AMCTRL.BBMac_Ctx;
	using CryptoEngine = pspsharp.crypto.CryptoEngine;
	using Utilities = pspsharp.util.Utilities;

	public class PBPFileSectorDevice : AbstractFileSectorDevice, IBrowser
	{
		private int lbaSize;
		private int blockSize;
		private int blockLBAs;
		private int numBlocks;
		private int numSectors;
		private sbyte[] vkey;
		private sbyte[] hkey = new sbyte[16];
		private TableInfo[] table;
		private int currentBlock;
		private AMCTRL amctrl;
		private sbyte[] blockBuffer;
		private sbyte[] tempBuffer;
		private int offsetParamSFO;
		private int offsetIcon0;
		private int offsetIcon1;
		private int offsetPic0;
		private int offsetPic1;
		private int offsetSnd0;
		private int offsetPspData;
		private int offsetPsarData;

		private class TableInfo
		{
			public const int FLAG_IS_UNCRYPTED = 4;
			public sbyte[] mac = new sbyte[16];
			public int offset;
			public int size;
			public int flags;
			public int unknown;
		}

		public PBPFileSectorDevice(RandomAccessFile fileAccess) : base(fileAccess)
		{

			try
			{
				int magic = endianSwap32(fileAccess.readInt());
				int version = endianSwap32(fileAccess.readInt());
				offsetParamSFO = endianSwap32(fileAccess.readInt());
				offsetIcon0 = endianSwap32(fileAccess.readInt());
				offsetIcon1 = endianSwap32(fileAccess.readInt());
				offsetPic0 = endianSwap32(fileAccess.readInt());
				offsetPic1 = endianSwap32(fileAccess.readInt());
				offsetSnd0 = endianSwap32(fileAccess.readInt());
				offsetPspData = endianSwap32(fileAccess.readInt());
				offsetPsarData = endianSwap32(fileAccess.readInt());
				if (magic != 0x50425000)
				{
					throw new IOException(string.Format("Invalid PBP header 0x{0:X8}", magic));
				}
				if (version != 0x00010000 && version != 0x00000100 && version != 0x00010001)
				{
					throw new IOException(string.Format("Invalid PBP version 0x{0:X8}", version));
				}
				fileAccess.seek(offsetPsarData);
				sbyte[] header = new sbyte[256];
				int readSize = fileAccess.read(header);
				if (readSize != header.Length)
				{
					int psarDataLength = (int)(fileAccess.length() - offsetPsarData);
					if (psarDataLength != 0 && psarDataLength != 16)
					{
						throw new IOException(string.Format("Invalid PBP header"));
					}
				}
				else if (header[0] == (sbyte)'N' && header[1] == (sbyte)'P' && header[2] == (sbyte)'U' && header[3] == (sbyte)'M' && header[4] == (sbyte)'D' && header[5] == (sbyte)'I' && header[6] == (sbyte)'M' && header[7] == (sbyte)'G')
				{
					CryptoEngine cryptoEngine = new CryptoEngine();
					amctrl = cryptoEngine.AMCTRLEngine;

					AMCTRL.BBMac_Ctx macContext = new AMCTRL.BBMac_Ctx();
					AMCTRL.BBCipher_Ctx cipherContext = new AMCTRL.BBCipher_Ctx();

					// getKey
					amctrl.hleDrmBBMacInit(macContext, 3);
					amctrl.hleDrmBBMacUpdate(macContext, header, 0xC0);
					sbyte[] macKeyC0 = new sbyte[16];
					Array.Copy(header, 0xC0, macKeyC0, 0, macKeyC0.Length);
					vkey = amctrl.GetKeyFromBBMac(macContext, macKeyC0);

					// decrypt NP header
					sbyte[] cipherData = new sbyte[0x60];
					Array.Copy(header, 0x40, cipherData, 0, cipherData.Length);
					Array.Copy(header, 0xA0, hkey, 0, hkey.Length);
					amctrl.hleDrmBBCipherInit(cipherContext, 1, 2, hkey, vkey);
					amctrl.hleDrmBBCipherUpdate(cipherContext, cipherData, cipherData.Length);
					amctrl.hleDrmBBCipherFinal(cipherContext);

					int lbaStart = Utilities.readUnaligned32(cipherData, 0x14);
					int lbaEnd = Utilities.readUnaligned32(cipherData, 0x24);
					numSectors = lbaEnd + 1;
					lbaSize = numSectors - lbaStart;
					blockLBAs = Utilities.readUnaligned32(header, 0x0C);
					blockSize = blockLBAs * ISectorDevice_Fields.sectorLength;
					numBlocks = (lbaSize + blockLBAs - 1) / blockLBAs;

					blockBuffer = new sbyte[blockSize];
					tempBuffer = new sbyte[blockSize];

					table = new TableInfo[numBlocks];

					int tableOffset = Utilities.readUnaligned32(cipherData, 0x2C);
					fileAccess.seek(offsetPsarData + tableOffset);
					sbyte[] tableBytes = new sbyte[numBlocks * 32];
					readSize = fileAccess.read(tableBytes);
					if (readSize != tableBytes.Length)
					{
						log.error(string.Format("Could not read table with size {0:D} (readSize={1:D})", tableBytes.Length, readSize));
					}

					IntBuffer tableInts = ByteBuffer.wrap(tableBytes).order(ByteOrder.LITTLE_ENDIAN).asIntBuffer();
					for (int i = 0; i < numBlocks; i++)
					{
						int p0 = tableInts.get();
						int p1 = tableInts.get();
						int p2 = tableInts.get();
						int p3 = tableInts.get();
						int p4 = tableInts.get();
						int p5 = tableInts.get();
						int p6 = tableInts.get();
						int p7 = tableInts.get();
						int k0 = p0 ^ p1;
						int k1 = p1 ^ p2;
						int k2 = p0 ^ p3;
						int k3 = p2 ^ p3;

						TableInfo tableInfo = new TableInfo();
						Array.Copy(tableBytes, i * 32, tableInfo.mac, 0, tableInfo.mac.Length);
						tableInfo.offset = p4 ^ k3;
						tableInfo.size = p5 ^ k1;
						tableInfo.flags = p6 ^ k2;
						tableInfo.unknown = p7 ^ k0;
						table[i] = tableInfo;
					}

					currentBlock = -1;
				}
			}
			catch (IOException e)
			{
				log.error("Reading PBP", e);
			}
		}

		public override int NumSectors
		{
			get
			{
				return numSectors;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void readSector(int sectorNumber, byte[] buffer, int offset) throws java.io.IOException
		public override void readSector(int sectorNumber, sbyte[] buffer, int offset)
		{
			int lba = sectorNumber - currentBlock;
			if (table == null)
			{
				Arrays.fill(buffer, offset, offset + ISectorDevice_Fields.sectorLength, (sbyte) 0);
			}
			else if (currentBlock >= 0 && lba >= 0 && lba < blockLBAs)
			{
				Array.Copy(blockBuffer, lba * ISectorDevice_Fields.sectorLength, buffer, offset, ISectorDevice_Fields.sectorLength);
			}
			else
			{
				int block = sectorNumber / blockLBAs;
				lba = sectorNumber % blockLBAs;
				currentBlock = block * blockLBAs;

				if (table[block].unknown == 0)
				{
					fileAccess.seek(offsetPsarData + table[block].offset);

					sbyte[] readBuffer;
					if (table[block].size < blockSize)
					{
						// For compressed blocks, decode into a temporary buffer
						readBuffer = tempBuffer;
					}
					else
					{
						readBuffer = blockBuffer;
					}

					int readSize = fileAccess.read(readBuffer, 0, table[block].size);
					if (readSize == table[block].size)
					{
						if ((table[block].flags & TableInfo.FLAG_IS_UNCRYPTED) == 0)
						{
							AMCTRL.BBCipher_Ctx cipherContext = new AMCTRL.BBCipher_Ctx();
							amctrl.hleDrmBBCipherInit(cipherContext, 1, 2, hkey, vkey, table[block].offset >> 4);
							amctrl.hleDrmBBCipherUpdate(cipherContext, readBuffer, table[block].size);
							amctrl.hleDrmBBCipherFinal(cipherContext);
						}

						// Compressed block?
						if (table[block].size < blockSize)
						{
							int lzsize = lzrc_decompress(blockBuffer, blockBuffer.Length, readBuffer, table[block].size);
							if (lzsize != blockSize)
							{
								log.error(string.Format("LZRC decompress error: decompressedSized={0:D}, should be {1:D}", lzsize, blockSize));
							}
						}

						Array.Copy(blockBuffer, lba * ISectorDevice_Fields.sectorLength, buffer, offset, ISectorDevice_Fields.sectorLength);
					}
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private byte[] read(int offset, int length) throws java.io.IOException
		private sbyte[] read(int offset, int length)
		{
			if (length <= 0)
			{
				return null;
			}

			sbyte[] buffer = new sbyte[length];
			fileAccess.seek(offset & 0xFFFFFFFFL);
			int read = fileAccess.read(buffer);
			if (read < 0)
			{
				return null;
			}

			// Read less than expected?
			if (read < length)
			{
				// Shrink the buffer to the read size
				sbyte[] newBuffer = new sbyte[read];
				Array.Copy(buffer, 0, newBuffer, 0, read);
				buffer = newBuffer;
			}

			return buffer;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readParamSFO() throws java.io.IOException
		public virtual sbyte[] readParamSFO()
		{
			return read(offsetParamSFO, offsetIcon0 - offsetParamSFO);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readIcon0() throws java.io.IOException
		public virtual sbyte[] readIcon0()
		{
			return read(offsetIcon0, offsetIcon1 - offsetIcon0);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readIcon1() throws java.io.IOException
		public virtual sbyte[] readIcon1()
		{
			return read(offsetIcon1, offsetPic0 - offsetIcon1);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readPic0() throws java.io.IOException
		public virtual sbyte[] readPic0()
		{
			return read(offsetPic0, offsetPic1 - offsetPic0);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readPic1() throws java.io.IOException
		public virtual sbyte[] readPic1()
		{
			return read(offsetPic1, offsetSnd0 - offsetPic1);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readSnd0() throws java.io.IOException
		public virtual sbyte[] readSnd0()
		{
			return read(offsetSnd0, offsetPspData - offsetSnd0);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readPspData() throws java.io.IOException
		public virtual sbyte[] readPspData()
		{
			return read(offsetPspData, offsetPsarData - offsetPspData);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readPsarData() throws java.io.IOException
		public virtual sbyte[] readPsarData()
		{
			return read(offsetPsarData, (int)(fileAccess.length() - offsetPsarData));
		}
	}

}