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
namespace pspsharp.HLE.VFS.crypto
{

	using CryptoEngine = pspsharp.crypto.CryptoEngine;
	using PGD = pspsharp.crypto.PGD;
	using Utilities = pspsharp.util.Utilities;

	public class PGDBlockVirtualFile : AbstractProxyVirtualFile
	{
		private const int pgdHeaderSize = 0x90;
		private sbyte[] key;
		private int dataOffset;
		private int dataSize;
		private int blockSize;
		private bool headerValid;
		private bool headerPresent;
		private sbyte[] buffer;
		private sbyte[] header;
		private PGD pgd;
		private bool sequentialRead;

		public PGDBlockVirtualFile(IVirtualFile pgdFile, sbyte[] key, int dataOffset) : base(pgdFile)
		{

			if (key != null)
			{
				this.key = key.Clone();
			}
			this.dataOffset = dataOffset;

			base.ioLseek(dataOffset);

			pgd = (new CryptoEngine()).PGDEngine;

			readHeader();

			this.dataOffset += dataOffset;
		}

		private void readHeader()
		{
			headerValid = false;
			headerPresent = false;
			sbyte[] inBuf = new sbyte[pgdHeaderSize];
			base.ioRead(inBuf, 0, pgdHeaderSize);

			// Check if the "PGD" header is present
			if (inBuf[0] != 0 || inBuf[1] != (sbyte)'P' || inBuf[2] != (sbyte)'G' || inBuf[3] != (sbyte)'D')
			{
				// No "PGD" found in the header,
				Console.WriteLine(string.Format("No PGD header detected {0:X2} {1:X2} {2:X2} {3:X2} ('{4}{5}{6}{7}') detected in file '{8}'", inBuf[0] & 0xFF, inBuf[1] & 0xFF, inBuf[2] & 0xFF, inBuf[3] & 0xFF, (char) inBuf[0], (char) inBuf[1], (char) inBuf[2], (char) inBuf[3], vFile));
				return;
			}
			headerPresent = true;

			// Decrypt 0x30 bytes at offset 0x30 to expose the first header.
			sbyte[] headerBuf = new sbyte[0x30 + 0x10];
			Array.Copy(inBuf, 0x10, headerBuf, 0, 0x10);
			Array.Copy(inBuf, 0x30, headerBuf, 0x10, 0x30);
			if (key == null)
			{
				key = pgd.GetEDATPGDKey(inBuf, pgdHeaderSize);
			}
			header = pgd.DecryptPGD(headerBuf, headerBuf.Length, key, 0);

			// Extract the decryption parameters.
			IntBuffer decryptedHeader = ByteBuffer.wrap(header).order(ByteOrder.LITTLE_ENDIAN).asIntBuffer();
			dataSize = decryptedHeader.get(5);
			blockSize = decryptedHeader.get(6);
			dataOffset = decryptedHeader.get(7);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("PGD dataSize={0:D}, blockSize={1:D}, dataOffset={2:D}", dataSize, blockSize, dataOffset));
				if (log.TraceEnabled)
				{
					log.trace(string.Format("PGD Header: {0}", Utilities.getMemoryDump(inBuf, 0, pgdHeaderSize)));
					log.trace(string.Format("Decrypted PGD Header: {0}", Utilities.getMemoryDump(header, 0, header.Length)));
				}
			}

			if (dataOffset < 0 || dataOffset > base.Length() || dataSize < 0)
			{
				// The decrypted PGD header is incorrect...
				Console.WriteLine(string.Format("Incorrect PGD header: dataSize={0:D}, chunkSize={1:D}, hashOffset={2:D}", dataSize, blockSize, dataOffset));
				return;
			}

			buffer = new sbyte[blockSize + 0x10];

			headerValid = true;
			sequentialRead = false;
		}

		public virtual int BlockSize
		{
			get
			{
				return blockSize;
			}
		}

		public virtual bool HeaderValid
		{
			get
			{
				return headerValid;
			}
		}

		public virtual bool HeaderPresent
		{
			get
			{
				return headerPresent;
			}
		}

		public override int ioRead(sbyte[] outputBuffer, int outputOffset, int outputLength)
		{
			int seed = 0;
			if (!sequentialRead)
			{
				seed = (int)(Position >> 4);
			}

			int readLength = blockSize;
			base.ioRead(buffer, 0x10, readLength);
			Array.Copy(header, 0, buffer, 0, 0x10);

			sbyte[] decryptedBytes;
			if (sequentialRead)
			{
				// This operation is more efficient than a complete DecryptPGD() call
				// but can only be used when decrypting sequential packets.
				decryptedBytes = pgd.UpdatePGDCipher(buffer, readLength + 0x10);
			}
			else
			{
				pgd.FinishPGDCipher();
				decryptedBytes = pgd.DecryptPGD(buffer, readLength + 0x10, key, seed);
				sequentialRead = true;
			}
			int Length = System.Math.Min(outputLength, decryptedBytes.Length);
			Array.Copy(decryptedBytes, 0, outputBuffer, outputOffset, Length);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("PGDBlockVirtualFile.ioRead Length=0x{0:X}: {1}", Length, Utilities.getMemoryDump(decryptedBytes, 0, Length)));
			}

			return Length;
		}

		public override int ioRead(TPointer outputPointer, int outputLength)
		{
			return ioReadBuf(outputPointer, outputLength);
		}

		public override long ioLseek(long offset)
		{
			long result = base.ioLseek(dataOffset + offset);
			if (result >= 0 && result >= dataOffset)
			{
				result -= dataOffset;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("PGDBlockVirtualFile.ioLseek offset=0x{0:X}, result=0x{1:X}", offset, result));
			}

			sequentialRead = false;

			return result;
		}

		public override long Position
		{
			get
			{
				long position = base.Position;
				if (position >= 0 && position >= dataOffset)
				{
					position -= dataOffset;
				}
				return position;
			}
		}

		public override long Length()
		{
			return dataSize;
		}
	}

}