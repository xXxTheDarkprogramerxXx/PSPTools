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
namespace pspsharp.crypto
{
	using pspAbstractMemoryMappedStructure = pspsharp.HLE.kernel.types.pspAbstractMemoryMappedStructure;
	using PSF = pspsharp.format.PSF;
	using Utilities = pspsharp.util.Utilities;

	public class SAVEDATA
	{

		private static KIRK kirk;
		private static readonly sbyte[] sdseed = new sbyte[] {(sbyte) 'S', (sbyte) 'A', (sbyte) 'V', (sbyte) 'E', (sbyte) 'D', (sbyte) 'A', (sbyte) 'T', (sbyte) 'A', (sbyte) 'S', (sbyte) 'E', (sbyte) 'E', (sbyte) 'D', (sbyte) 'J', (sbyte) 'P', (sbyte) 'C', (sbyte) 'S', (sbyte) 'P', (sbyte) '0', (sbyte) '0', (sbyte) '0'};

		public SAVEDATA()
		{
			// Start the KIRK engine with a dummy seed and fuseID.
			kirk = new KIRK(sdseed, 0x14, unchecked((int)0xDEADC0DE), 0x12345678);
		}

		// CHNNLSV SD context structs.
		public class SD_Ctx1 : pspAbstractMemoryMappedStructure
		{
			public int mode;
			public sbyte[] pad = new sbyte[16];
			public sbyte[] key = new sbyte[16];
			public int padSize;

			protected internal override void read()
			{
				mode = read32();
				read8Array(pad);
				read8Array(key);
				padSize = read32();
			}

			protected internal override void write()
			{
				write32(mode);
				write8Array(pad);
				write8Array(key);
				write32(padSize);
			}

			public override int @sizeof()
			{
				return 40;
			}

			public override string ToString()
			{
				return string.Format("mode=0x{0:X}, pad={1}, key={2}, padSize=0x{3:X}", mode, Utilities.getMemoryDump(pad, 0, pad.Length), Utilities.getMemoryDump(key, 0, key.Length), padSize);
			}
		}

		public class SD_Ctx2 : pspAbstractMemoryMappedStructure
		{
			internal int mode;
			internal int unk;
			internal sbyte[] buf;

			public SD_Ctx2()
			{
				mode = 0;
				unk = 0;
				buf = new sbyte[16];
			}

			protected internal override void read()
			{
				mode = read32();
				unk = read32();
				read8Array(buf);
			}

			protected internal override void write()
			{
				write32(mode);
				write32(unk);
				write8Array(buf);
			}

			public override int @sizeof()
			{
				return 24;
			}

			public override string ToString()
			{
				return string.Format("mode=0x{0:X}, unk=0x{1:X}, buf={2}", mode, unk, Utilities.getMemoryDump(buf, 0, buf.Length));
			}
		}

		private static bool isNullKey(sbyte[] key)
		{
			if (key != null)
			{
				for (int i = 0; i < key.Length; i++)
				{
					if (key[i] != (sbyte) 0)
					{
						return false;
					}
				}
			}
			return true;
		}

		private sbyte[] xorHash(sbyte[] dest, int dest_offset, int[] src, int src_offset, int size)
		{
			for (int i = 0; i < size; i++)
			{
				dest[dest_offset + i] = (sbyte)(dest[dest_offset + i] ^ src[src_offset + i]);
			}
			return dest;
		}

		private sbyte[] xorKey(sbyte[] dest, int dest_offset, sbyte[] src, int src_offset, int size)
		{
			for (int i = 0; i < size; i++)
			{
				dest[dest_offset + i] = (sbyte)(dest[dest_offset + i] ^ src[src_offset + i]);
			}
			return dest;
		}

		private void ScrambleSD(sbyte[] buf, int size, int seed, int cbc, int kirk_code)
		{
			// Set CBC mode.
			buf[0] = 0;
			buf[1] = 0;
			buf[2] = 0;
			buf[3] = (sbyte) cbc;

			// Set unkown parameters to 0.
			buf[4] = 0;
			buf[5] = 0;
			buf[6] = 0;
			buf[7] = 0;

			buf[8] = 0;
			buf[9] = 0;
			buf[10] = 0;
			buf[11] = 0;

			// Set the the key seed to seed.
			buf[12] = 0;
			buf[13] = 0;
			buf[14] = 0;
			buf[15] = (sbyte) seed;

			// Set the the data size to size.
			buf[16] = unchecked((sbyte)((size >> 24) & 0xFF));
			buf[17] = unchecked((sbyte)((size >> 16) & 0xFF));
			buf[18] = unchecked((sbyte)((size >> 8) & 0xFF));
			buf[19] = unchecked((sbyte)(size & 0xFF));

			// Ignore PSP_KIRK_CMD_ENCRYPT_FUSE and PSP_KIRK_CMD_DECRYPT_FUSE. 
			if (kirk_code == KIRK.PSP_KIRK_CMD_ENCRYPT_FUSE || kirk_code == KIRK.PSP_KIRK_CMD_DECRYPT_FUSE)
			{
				return;
			}

			ByteBuffer bBuf = ByteBuffer.wrap(buf);
			kirk.hleUtilsBufferCopyWithRange(bBuf, size, bBuf, size, kirk_code);
		}

		private int getModeSeed(int mode)
		{
			int seed;
			switch (mode)
			{
				case 0x6:
					seed = 0x11;
					break;
				case 0x4:
					seed = 0xD;
					break;
				case 0x2:
					seed = 0x5;
					break;
				case 0x1:
					seed = 0x3;
					break;
				case 0x3:
					seed = 0xC;
					break;
				default:
					seed = 0x10;
					break;
			}
			return seed;
		}

		private void cryptMember(SD_Ctx2 ctx, sbyte[] data, int data_offset, int length)
		{
			int finalSeed;
			sbyte[] dataBuf = new sbyte[length + 0x14];
			sbyte[] keyBuf1 = new sbyte[0x10];
			sbyte[] keyBuf2 = new sbyte[0x10];
			sbyte[] hashBuf = new sbyte[0x10];

			// Copy the hash stored by hleSdCreateList.
			Array.Copy(ctx.buf, 0, dataBuf, 0x14, 0x10);

			if (ctx.mode == 0x1)
			{
				// Decryption mode 0x01: decrypt the hash directly with KIRK CMD7.
				ScrambleSD(dataBuf, 0x10, 0x4, 5, KIRK.PSP_KIRK_CMD_DECRYPT);
				finalSeed = 0x53;
			}
			else if (ctx.mode == 0x2)
			{
				// Decryption mode 0x02: decrypt the hash directly with KIRK CMD8.
				ScrambleSD(dataBuf, 0x10, 0x100, 5, KIRK.PSP_KIRK_CMD_DECRYPT_FUSE);
				finalSeed = 0x53;
			}
			else if (ctx.mode == 0x3)
			{
				// Decryption mode 0x03: XOR the hash with SD keys and decrypt with KIRK CMD7.
				dataBuf = xorHash(dataBuf, 0x14, KeyVault.sdHashKey4, 0, 0x10);
				ScrambleSD(dataBuf, 0x10, 0xE, 5, KIRK.PSP_KIRK_CMD_DECRYPT);
				dataBuf = xorHash(dataBuf, 0, KeyVault.sdHashKey3, 0, 0x10);
				finalSeed = 0x57;
			}
			else if (ctx.mode == 0x4)
			{
				// Decryption mode 0x04: XOR the hash with SD keys and decrypt with KIRK CMD8.
				dataBuf = xorHash(dataBuf, 0x14, KeyVault.sdHashKey4, 0, 0x10);
				ScrambleSD(dataBuf, 0x10, 0x100, 5, KIRK.PSP_KIRK_CMD_DECRYPT_FUSE);
				dataBuf = xorHash(dataBuf, 0, KeyVault.sdHashKey3, 0, 0x10);
				finalSeed = 0x57;
			}
			else if (ctx.mode == 0x6)
			{
				// Decryption mode 0x06: XOR the hash with new SD keys and decrypt with KIRK CMD8.
				dataBuf = xorHash(dataBuf, 0x14, KeyVault.sdHashKey7, 0, 0x10);
				ScrambleSD(dataBuf, 0x10, 0x100, 5, KIRK.PSP_KIRK_CMD_DECRYPT_FUSE);
				dataBuf = xorHash(dataBuf, 0, KeyVault.sdHashKey6, 0, 0x10);
				finalSeed = 0x64;
			}
			else
			{
				// Decryption mode 0x05: XOR the hash with new SD keys and decrypt with KIRK CMD7.
				dataBuf = xorHash(dataBuf, 0x14, KeyVault.sdHashKey7, 0, 0x10);
				ScrambleSD(dataBuf, 0x10, 0x12, 5, KIRK.PSP_KIRK_CMD_DECRYPT);
				dataBuf = xorHash(dataBuf, 0, KeyVault.sdHashKey6, 0, 0x10);
				finalSeed = 0x64;
			}

			// Store the calculated key.
			Array.Copy(dataBuf, 0, keyBuf2, 0, 0x10);

			// Apply extra padding if ctx.unk is not 1.
			if (ctx.unk != 0x1)
			{
				Array.Copy(keyBuf2, 0, keyBuf1, 0, 0xC);
				keyBuf1[0xC] = unchecked((sbyte)((ctx.unk - 1) & 0xFF));
				keyBuf1[0xD] = unchecked((sbyte)(((ctx.unk - 1) >> 8) & 0xFF));
				keyBuf1[0xE] = unchecked((sbyte)(((ctx.unk - 1) >> 16) & 0xFF));
				keyBuf1[0xF] = unchecked((sbyte)(((ctx.unk - 1) >> 24) & 0xFF));
			}

			// Copy the first 0xC bytes of the obtained key and replicate them
			// across a new list buffer. As a terminator, add the ctx1.seed parameter's
			// 4 bytes (endian swapped) to achieve a full numbered list.
			for (int i = 0x14; i < (length + 0x14); i += 0x10)
			{
				Array.Copy(keyBuf2, 0, dataBuf, i, 0xC);
				dataBuf[i + 0xC] = unchecked((sbyte)(ctx.unk & 0xFF));
				dataBuf[i + 0xD] = unchecked((sbyte)((ctx.unk >> 8) & 0xFF));
				dataBuf[i + 0xE] = unchecked((sbyte)((ctx.unk >> 16) & 0xFF));
				dataBuf[i + 0xF] = unchecked((sbyte)((ctx.unk >> 24) & 0xFF));
				ctx.unk++;
			}

			// Copy the generated hash to hashBuf.
			Array.Copy(dataBuf, length + 0x04, hashBuf, 0, 0x10);

			// Decrypt the hash with KIRK CMD7.
			ScrambleSD(dataBuf, length, finalSeed, 5, KIRK.PSP_KIRK_CMD_DECRYPT);

			// XOR the first 16-bytes of data with the saved key to generate a new hash.
			dataBuf = xorKey(dataBuf, 0, keyBuf1, 0, 0x10);

			// Copy back the last hash from the list to the first keyBuf.
			Array.Copy(hashBuf, 0, keyBuf1, 0, 0x10);

			// Finally, XOR the full list with the given data.
			xorKey(data, data_offset, dataBuf, 0, length);
		}

		/*
		 * sceSd - chnnlsv.prx
		 */
		public virtual int hleSdSetIndex(SD_Ctx1 ctx, int encMode)
		{
			// Set all parameters to 0 and assign the encMode.
			ctx.mode = encMode;
			ctx.padSize = 0;
			for (int i = 0; i < 0x10; i++)
			{
				ctx.pad[i] = 0;
			}
			for (int i = 0; i < 0x10; i++)
			{
				ctx.key[i] = 0;
			}
			return 0;
		}

		public virtual int hleSdRemoveValue(SD_Ctx1 ctx, sbyte[] data, int length)
		{
			if (ctx.padSize > 0x10 || (length < 0))
			{
				// Invalid key or length.
				return -1;
			}
			else if (((ctx.padSize + length) <= 0x10))
			{
				// The key hasn't been set yet.
				// Extract the hash from the data and set it as the key.
				Array.Copy(data, 0, ctx.pad, ctx.padSize, length);
				ctx.padSize += length;
				return 0;
			}
			else
			{
				// Calculate the seed.
				int seed = getModeSeed(ctx.mode);

				// Setup the buffer. 
				sbyte[] scrambleBuf = new sbyte[0x800 + 0x14];

				// Copy the previous pad key to the buffer.
				Array.Copy(ctx.pad, 0, scrambleBuf, 0x14, ctx.padSize);

				// Calculate new key length.
				int kLen = ((ctx.padSize + length) & 0x0F);
				if (kLen == 0)
				{
					kLen = 0x10;
				}

				// Calculate new data length.
				int nLen = ctx.padSize;
				ctx.padSize = kLen;

				// Copy data's footer to make a new key.
				int remaining = length - kLen;
				Array.Copy(data, remaining, ctx.pad, 0, kLen);

				// Process the encryption in 0x800 blocks.
				int blockSize = 0x800;

				for (int i = 0; i < remaining; i++)
				{
					if (nLen == blockSize)
					{
						// XOR with result and encrypt with KIRK CMD 4.
						scrambleBuf = xorKey(scrambleBuf, 0x14, ctx.key, 0, 0x10);
						ScrambleSD(scrambleBuf, blockSize, seed, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);
						Array.Copy(scrambleBuf, blockSize + 0x4, ctx.key, 0, 0x10);
						// Reset length.
						nLen = 0;
					}
					// Keep copying data.
					scrambleBuf[0x14 + nLen] = data[i];
					nLen++;
				}

				// Process any leftover data.
				if (nLen > 0)
				{
					scrambleBuf = xorKey(scrambleBuf, 0x14, ctx.key, 0, 0x10);
					ScrambleSD(scrambleBuf, nLen, seed, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);
					Array.Copy(scrambleBuf, nLen + 0x4, ctx.key, 0, 0x10);
				}

				return 0;
			}
		}

		public virtual int hleSdGetLastIndex(SD_Ctx1 ctx, sbyte[] hash, sbyte[] key)
		{
			if (ctx.padSize > 0x10)
			{
				// Invalid key length.
				return -1;
			}

			// Calculate the seed.
			int seed = getModeSeed(ctx.mode);

			// Set up the buffer.
			sbyte[] scrambleBuf = new sbyte[0x800 + 0x14];

			// Set up necessary buffers.
			sbyte[] keyBuf = new sbyte[0x10];
			sbyte[] resultBuf = new sbyte[0x10];

			// Encrypt the buffer with KIRK CMD 4.
			ScrambleSD(scrambleBuf, 0x10, seed, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);

			// Store the generated key.
			Array.Copy(scrambleBuf, 0x14, keyBuf, 0, 0x10);

			// Apply custom padding management to the stored key.
			sbyte b = ((keyBuf[0] & unchecked((sbyte) 0x80)) != 0) ? unchecked((sbyte) 0x87) : 0;
			for (int i = 0; i < 0xF; i++)
			{
				int b1 = ((keyBuf[i] & 0xFF) << 1);
				int b2 = ((keyBuf[i + 1] & 0xFF) >> 7);
				keyBuf[i] = (sbyte)(b1 | b2);
			}
			sbyte t = (sbyte)((keyBuf[0xF] & 0xFF) << 1);
			keyBuf[0xF] = (sbyte)(t ^ b);

			if (ctx.padSize < 0x10)
			{
				sbyte bb = ((keyBuf[0] < 0)) ? unchecked((sbyte) 0x87) : 0;
				for (int i = 0; i < 0xF; i++)
				{
					int bb1 = ((keyBuf[i] & 0xFF) << 1);
					int bb2 = ((keyBuf[i + 1] & 0xFF) >> 7);
					keyBuf[i] = (sbyte)(bb1 | bb2);
				}
				sbyte tt = (sbyte)((keyBuf[0xF] & 0xFF) << 1);
				keyBuf[0xF] = (sbyte)(tt ^ bb);

				ctx.pad[ctx.padSize] = unchecked((sbyte) 0x80);
				if ((ctx.padSize + 1) < 0x10)
				{
					for (int i = 0; i < (0x10 - ctx.padSize - 1); i++)
					{
						ctx.pad[ctx.padSize + 1 + i] = 0;
					}
				}
			}

			// XOR previous pad key with new one and copy the result back to the buffer.
			ctx.pad = xorKey(ctx.pad, 0, keyBuf, 0, 0x10);
			Array.Copy(ctx.pad, 0, scrambleBuf, 0x14, 0x10);

			// Save the previous result key.
			Array.Copy(ctx.key, 0, resultBuf, 0, 0x10);

			// XOR the decrypted key with the result key.
			scrambleBuf = xorKey(scrambleBuf, 0x14, resultBuf, 0, 0x10);

			// Encrypt the key with KIRK CMD 4.
			ScrambleSD(scrambleBuf, 0x10, seed, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);

			// Copy back the key into the result buffer.
			Array.Copy(scrambleBuf, 0x14, resultBuf, 0, 0x10);

			// If ctx.mode is new mode 0x5 or 0x6, XOR with the new hash key 5, else, XOR with hash key 2.
			if ((ctx.mode == 0x5) || (ctx.mode == 0x6))
			{
				resultBuf = xorHash(resultBuf, 0, KeyVault.sdHashKey5, 0, 0x10);
			}
			else if ((ctx.mode == 0x3) || (ctx.mode == 0x4))
			{
				resultBuf = xorHash(resultBuf, 0, KeyVault.sdHashKey2, 0, 0x10);
			}

			// If mode is 2, 4 or 6, encrypt again with KIRK CMD 5 and then KIRK CMD 4.
			if ((ctx.mode == 0x2) || (ctx.mode == 0x4) || (ctx.mode == 0x6))
			{
				// Copy the result buffer into the data buffer.
				Array.Copy(resultBuf, 0, scrambleBuf, 0x14, 0x10);

				// Encrypt with KIRK CMD 5 (seed is always 0x100).
				ScrambleSD(scrambleBuf, 0x10, 0x100, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT_FUSE);

				// Encrypt again with KIRK CMD 4.
				ScrambleSD(scrambleBuf, 0x10, seed, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);

				// Copy back into result buffer.
				Array.Copy(scrambleBuf, 0x14, resultBuf, 0, 0x10);
			}

			// XOR with the supplied key and encrypt with KIRK CMD 4.
			if (key != null)
			{
				// XOR result buffer with user key.
				resultBuf = xorKey(resultBuf, 0, key, 0, 0x10);

				// Copy the result buffer into the data buffer.
				Array.Copy(resultBuf, 0, scrambleBuf, 0x14, 0x10);

				// Encrypt with KIRK CMD 4.
				ScrambleSD(scrambleBuf, 0x10, seed, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);

				// Copy back into the result buffer.
				Array.Copy(scrambleBuf, 0x14, resultBuf, 0, 0x10);
			}

			// Copy back the generated hash.
			Array.Copy(resultBuf, 0, hash, 0, 0x10);

			// Clear the context fields.
			ctx.mode = 0;
			ctx.padSize = 0;
			for (int i = 0; i < 0x10; i++)
			{
				ctx.pad[i] = 0;
			}
			for (int i = 0; i < 0x10; i++)
			{
				ctx.key[i] = 0;
			}

			return 0;
		}

		public virtual int hleSdCleanList(SD_Ctx2 ctx)
		{
			ctx.mode = 0;
			ctx.unk = 0;
			for (int i = 0; i < 0x10; i++)
			{
				ctx.buf[i] = 0;
			}
			return 0;
		}

		public virtual int hleSdCreateList(SD_Ctx2 ctx, int encMode, int genMode, sbyte[] data, sbyte[] key)
		{
			// If the key is not a 16-byte key, return an error.
			if (!isNullKey(key) && key.Length < 0x10)
			{
				return -1;
			}

			// Set the mode and the unknown parameters.
			ctx.mode = encMode;
			ctx.unk = 0x1;

			// Key generator mode 0x1 (encryption): use an encrypted pseudo random number before XORing the data with the given key.
			if (genMode == 0x1)
			{
				sbyte[] header = new sbyte[0x24];
				sbyte[] seed = new sbyte[0x14];

				// Generate SHA-1 to act as seed for encryption.
				ByteBuffer bSeed = ByteBuffer.wrap(seed);
				kirk.hleUtilsBufferCopyWithRange(bSeed, 0x14, null, 0, KIRK.PSP_KIRK_CMD_PRNG);

				// Propagate SHA-1 in kirk header.
				Array.Copy(bSeed.array(), 0, header, 0, 0x14);
				Array.Copy(bSeed.array(), 0, header, 0x14, 0x10);
				header[0x20] = 0;
				header[0x21] = 0;
				header[0x22] = 0;
				header[0x23] = 0;

				// Encryption mode 0x1: encrypt with KIRK CMD4 and XOR with the given key.
				if (ctx.mode == 0x1)
				{
					ScrambleSD(header, 0x10, 0x4, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);
					Array.Copy(header, 0x14, ctx.buf, 0, 0x10);
					Array.Copy(header, 0x14, data, 0, 0x10);
					// If the key is not null, XOR the hash with it.
					if (!isNullKey(key))
					{
						ctx.buf = xorKey(ctx.buf, 0, key, 0, 0x10);
					}
					return 0;
				}
				else if (ctx.mode == 0x2)
				{ // Encryption mode 0x2: encrypt with KIRK CMD5 and XOR with the given key.
					ScrambleSD(header, 0x10, 0x100, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT_FUSE);
					Array.Copy(header, 0x14, ctx.buf, 0, 0x10);
					Array.Copy(header, 0x14, data, 0, 0x10);
					// If the key is not null, XOR the hash with it.
					if (!isNullKey(key))
					{
						ctx.buf = xorKey(ctx.buf, 0, key, 0, 0x10);
					}
					return 0;
				}
				else if (ctx.mode == 0x3)
				{ // Encryption mode 0x3: XOR with SD keys, encrypt with KIRK CMD4 and XOR with the given key.
					header = xorHash(header, 0x14, KeyVault.sdHashKey3, 0, 0x10);
					ScrambleSD(header, 0x10, 0xE, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);
					header = xorHash(header, 0x14, KeyVault.sdHashKey4, 0, 0x10);
					Array.Copy(header, 0x14, ctx.buf, 0, 0x10);
					Array.Copy(header, 0x14, data, 0, 0x10);
					// If the key is not null, XOR the hash with it.
					if (!isNullKey(key))
					{
						ctx.buf = xorKey(ctx.buf, 0, key, 0, 0x10);
					}
					return 0;
				}
				else if (ctx.mode == 0x4)
				{ // Encryption mode 0x4: XOR with SD keys, encrypt with KIRK CMD5 and XOR with the given key.
					header = xorHash(header, 0x14, KeyVault.sdHashKey3, 0, 0x10);
					ScrambleSD(header, 0x10, 0x100, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT_FUSE);
					header = xorHash(header, 0x14, KeyVault.sdHashKey4, 0, 0x10);
					Array.Copy(header, 0x14, ctx.buf, 0, 0x10);
					Array.Copy(header, 0x14, data, 0, 0x10);
					// If the key is not null, XOR the hash with it.
					if (!isNullKey(key))
					{
						ctx.buf = xorKey(ctx.buf, 0, key, 0, 0x10);
					}
					return 0;
				}
				else if (ctx.mode == 0x6)
				{ // Encryption mode 0x6: XOR with new SD keys, encrypt with KIRK CMD5 and XOR with the given key.
					header = xorHash(header, 0x14, KeyVault.sdHashKey6, 0, 0x10);
					ScrambleSD(header, 0x10, 0x100, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT_FUSE);
					header = xorHash(header, 0x14, KeyVault.sdHashKey7, 0, 0x10);
					Array.Copy(header, 0x14, ctx.buf, 0, 0x10);
					Array.Copy(header, 0x14, data, 0, 0x10);
					// If the key is not null, XOR the hash with it.
					if (!isNullKey(key))
					{
						ctx.buf = xorKey(ctx.buf, 0, key, 0, 0x10);
					}
					return 0;
				}
				else
				{ // Encryption mode 0x5: XOR with new SD keys, encrypt with KIRK CMD4 and XOR with the given key.
					header = xorHash(header, 0x14, KeyVault.sdHashKey6, 0, 0x10);
					ScrambleSD(header, 0x10, 0x12, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);
					header = xorHash(header, 0x14, KeyVault.sdHashKey7, 0, 0x10);
					Array.Copy(header, 0x14, ctx.buf, 0, 0x10);
					Array.Copy(header, 0x14, data, 0, 0x10);
					// If the key is not null, XOR the hash with it.
					if (!isNullKey(key))
					{
						ctx.buf = xorKey(ctx.buf, 0, key, 0, 0x10);
					}
					return 0;
				}
			}
			else if (genMode == 0x2)
			{ // Key generator mode 0x02 (decryption): directly XOR the data with the given key.
				// Grab the data hash (first 16-bytes).
				Array.Copy(data, 0, ctx.buf, 0, 0x10);
				// If the key is not null, XOR the hash with it.
				if (!isNullKey(key))
				{
					ctx.buf = xorKey(ctx.buf, 0, key, 0, 0x10);
				}
				return 0;
			}
			else
			{
				// Invalid mode.
				return -1;
			}
		}

		public virtual int hleSdSetMember(SD_Ctx2 ctx, sbyte[] data, int length)
		{
			if (length == 0)
			{
				return 0;
			}
			if ((length & 0xF) != 0)
			{
				return -1;
			}

			// Parse the data in 0x800 blocks first.
			int index = 0;
			if (length >= 0x800)
			{
				for (index = 0; length >= 0x800; index += 0x800)
				{
					cryptMember(ctx, data, index, 0x800);
					length -= 0x800;
				}
			}

			// Finally parse the rest of the data.
			if (length >= 0x10)
			{
				cryptMember(ctx, data, index, length);
			}

			return 0;
		}

		public virtual sbyte[] DecryptSavedata(sbyte[] buf, int size, sbyte[] key)
		{
			// Initialize the context structs.
			int sdDecMode;
			SD_Ctx1 ctx1 = new SD_Ctx1();
			SD_Ctx2 ctx2 = new SD_Ctx2();

			// Setup the buffers.
			int alignedSize = (((size + 0xF) >> 4) << 4) - 0x10;
			sbyte[] decbuf = new sbyte[size - 0x10];
			sbyte[] tmpbuf = new sbyte[alignedSize];

			// Set the decryption mode.
			if (isNullKey(key))
			{
				sdDecMode = 1;
			}
			else
			{
				// After firmware version 2.7.1 the decryption mode used is 5.
				// Note: Due to a mislabel, 3 games from firmware 2.8.1 (Sonic Rivals, 
				// Star Trek: Tactical Assault and Brothers in Arms: D-Day) 
				// still use the decryption mode 3.
				if (Emulator.Instance.FirmwareVersion > 271 && !((State.discId.Equals("ULUS10195") || State.discId.Equals("ULES00622")) || (State.discId.Equals("ULUS10193") || State.discId.Equals("ULES00608")) || (State.discId.Equals("ULUS10150") || State.discId.Equals("ULES00623"))))
				{
					sdDecMode = 5;
				}
				else
				{
					sdDecMode = 3;
				}
			}

			// Perform the decryption.
			hleSdSetIndex(ctx1, sdDecMode);
			hleSdCreateList(ctx2, sdDecMode, 2, buf, key);
			hleSdRemoveValue(ctx1, buf, 0x10);

			Array.Copy(buf, 0x10, tmpbuf, 0, size - 0x10);
			hleSdRemoveValue(ctx1, tmpbuf, alignedSize);

			hleSdSetMember(ctx2, tmpbuf, alignedSize);

			// Clear context 2.
			hleSdCleanList(ctx2);

			// Copy back the data.
			Array.Copy(tmpbuf, 0, decbuf, 0, size - 0x10);

			return decbuf;
		}

		public virtual sbyte[] EncryptSavedata(sbyte[] buf, int size, sbyte[] key)
		{
			// Initialize the context structs.
			int sdEncMode;
			SD_Ctx1 ctx1 = new SD_Ctx1();
			SD_Ctx2 ctx2 = new SD_Ctx2();

			// Setup the buffers.
			int alignedSize = ((size + 0xF) >> 4) << 4;
			sbyte[] tmpbuf1 = new sbyte[alignedSize + 0x10];
			sbyte[] tmpbuf2 = new sbyte[alignedSize];
			sbyte[] hash = new sbyte[0x10];

			// Copy the plain data to tmpbuf.
			Array.Copy(buf, 0, tmpbuf1, 0x10, size);

			// Set the encryption mode.
			if (isNullKey(key))
			{
				sdEncMode = 1;
			}
			else
			{
				// After firmware version 2.7.1 the encryption mode used is 5.
				// Note: Due to a mislabel, 3 games from firmware 2.8.1 (Sonic Rivals, 
				// Star Trek: Tactical Assault and Brothers in Arms: D-Day) 
				// still use the encryption mode 3.
				if (Emulator.Instance.FirmwareVersion > 271 && !((State.discId.Equals("ULUS10195") || State.discId.Equals("ULES00622")) || (State.discId.Equals("ULUS10193") || State.discId.Equals("ULES00608")) || (State.discId.Equals("ULUS10150") || State.discId.Equals("ULES00623"))))
				{
					sdEncMode = 5;
				}
				else
				{
					sdEncMode = 3;
				}
			}

			// Generate the encryption IV (first 0x10 bytes).
			hleSdCreateList(ctx2, sdEncMode, 1, tmpbuf1, key);
			hleSdSetIndex(ctx1, sdEncMode);
			hleSdRemoveValue(ctx1, tmpbuf1, 0x10);

			Array.Copy(tmpbuf1, 0x10, tmpbuf2, 0, alignedSize);
			hleSdSetMember(ctx2, tmpbuf2, alignedSize);

			// Clear extra bytes.
			for (int i = 0; i < (alignedSize - size); i++)
			{
				tmpbuf2[size + i] = 0;
			}

			// Encrypt the data.
			hleSdRemoveValue(ctx1, tmpbuf2, alignedSize);

			// Copy back the encrypted data + IV.
			for (int i = 0; i < (tmpbuf1.Length - 0x10); i++)
			{
				tmpbuf1[0x10 + i] = 0;
			}
			Array.Copy(tmpbuf2, 0, tmpbuf1, 0x10, alignedSize);
			Array.Copy(tmpbuf1, 0, buf, 0, buf.Length);

			// Clear context 2.
			hleSdCleanList(ctx2);

			// Generate a file hash for this data.
			hleSdGetLastIndex(ctx1, hash, key);

			return hash;
		}

		private sbyte[] GenerateSavedataHash(sbyte[] data, int size, int mode)
		{
			SD_Ctx1 ctx1 = new SD_Ctx1();
			sbyte[] hash = new sbyte[0x10];

			// Generate a new hash using a key.
			hleSdSetIndex(ctx1, mode);
			hleSdRemoveValue(ctx1, data, size);
			if (hleSdGetLastIndex(ctx1, hash, null) < 0)
			{
				for (int i = 0; i < 0x10; i++)
				{
					// Generate a dummy hash in case of failure.
					hash[i] = 1;
				}
			}
			return hash;
		}

		public virtual void UpdateSavedataHashes(PSF psf, sbyte[] data, int size, sbyte[] @params, sbyte[] key)
		{
			// Setup the params, hash and mode.
			sbyte[] hash = new sbyte[0x10];

			// Determine the hashing mode.
			int mode = 0;
			int check_bit = 1;
			if (!isNullKey(key))
			{
				if (Emulator.Instance.FirmwareVersion > 271)
				{
					mode = 4;
				}
				else
				{
					mode = 2;
				}
			}

			// Check for previous SAVEDATA_PARAMS.
			if (@params != null)
			{
				for (int i = 0; i < @params.Length; i++)
				{
					if (@params[i] != 0)
					{
						// Extract the mode setup from the already existing data.
						mode = ((@params[0] >> 4) & 0xF);
						check_bit = ((@params[0]) & 0xF);
						break;
					}
				}
			}

			// New mode (after firmware 2.7.1).
			if ((mode & 0x4) == 0x4)
			{
				// Generate a type 6 hash.
				hash = GenerateSavedataHash(data, size, 6);
				Array.Copy(hash, 0, data, 0x11B0 + 0x20, 0x10);
				// Set the SAVEDATA_PARAMS byte to 0x41.
				data[0x11B0] |= 0x01;
				data[0x11B0] |= 0x40;
				// Generate a type 5 hash.
				hash = GenerateSavedataHash(data, size, 5);
				Array.Copy(hash, 0, data, 0x11B0 + 0x70, 0x10);
			}
			else if ((mode & 0x2) == 0x2)
			{ // Last old mode (firmware 2.0.0 to 2.7.1).
				// Generate a type 4 hash.
				hash = GenerateSavedataHash(data, size, 4);
				Array.Copy(hash, 0, data, 0x11B0 + 0x20, 0x10);
				// Set the SAVEDATA_PARAMS byte to 0x21.
				data[0x11B0] |= 0x01;
				data[0x11B0] |= 0x20;
				// Generate a type 3 hash.
				hash = GenerateSavedataHash(data, size, 3);
				Array.Copy(hash, 0, data, 0x11B0 + 0x70, 0x10);
			}
			else
			{ // First old mode (before firmware 2.0.0).
				// Generate a type 2 hash.
				hash = GenerateSavedataHash(data, size, 2);
				Array.Copy(hash, 0, data, 0x11B0 + 0x20, 0x10);
				// Set the SAVEDATA_PARAMS byte to 0x01.
				data[0x11B0] |= 0x01;
			}

			if ((check_bit & 0x1) == 0x1)
			{
				// Generate a type 1 hash.
				hash = GenerateSavedataHash(data, size, 1);
				Array.Copy(hash, 0, data, 0x11B0 + 0x10, 0x10);
			}

			// Output the final PSF file containing the SAVEDATA_PARAMS and file hashes.
			try
			{
				// Update the SAVEDATA_PARAMS.
				sbyte[] savedataParams = new sbyte[0x80];
				for (int i = 0; i < 0x80; i++)
				{
					savedataParams[i] = data[0x11B0 + i];
				}
				psf.put("SAVEDATA_PARAMS", savedataParams);
			}
			catch (Exception)
			{
				// Ignore...
			}
		}
	}

}