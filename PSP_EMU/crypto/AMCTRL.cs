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

	public class AMCTRL
	{

		private static KIRK kirk;
		private static readonly sbyte[] amseed = new sbyte[] {(sbyte) 'A', (sbyte) 'M', (sbyte) 'C', (sbyte) 'T', (sbyte) 'R', (sbyte) 'L', (sbyte) 'S', (sbyte) 'E', (sbyte) 'E', (sbyte) 'D', (sbyte) 'J', (sbyte) 'P', (sbyte) 'C', (sbyte) 'S', (sbyte) 'P', (sbyte) '0', (sbyte) '0', (sbyte) '0', (sbyte) '0', (sbyte) '0'};

		public AMCTRL()
		{
			// Start the KIRK engine with a dummy seed and fuseID.
			kirk = new KIRK(amseed, 0x14, unchecked((int)0xDEADC0DE), 0x12345678);
		}

		// AMCTRL context structs.
		public class BBCipher_Ctx
		{

			internal int mode;
			internal int seed;
			internal sbyte[] buf = new sbyte[16];

			public BBCipher_Ctx()
			{
				mode = 0;
				seed = 0;
			}
		}

		public class BBMac_Ctx
		{

			internal int mode;
			internal sbyte[] key = new sbyte[16];
			internal sbyte[] pad = new sbyte[16];
			internal int padSize;

			public BBMac_Ctx()
			{
				mode = 0;
				padSize = 0;
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

		private int getModeSeed(int mode)
		{
			int seed;
			switch (mode)
			{
				case 0x2:
					seed = 0x3A;
					break;
				default:
					seed = 0x38;
					break;
			}
			return seed;
		}

		private void ScrambleBB(sbyte[] buf, int size, int seed, int cbc, int kirk_code)
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

			ByteBuffer bBuf = ByteBuffer.wrap(buf);
			kirk.hleUtilsBufferCopyWithRange(bBuf, size, bBuf, size, kirk_code);
		}

		private void cipherMember(BBCipher_Ctx ctx, sbyte[] data, int data_offset, int length)
		{
			sbyte[] dataBuf = new sbyte[length + 0x14];
			sbyte[] keyBuf1 = new sbyte[0x10];
			sbyte[] keyBuf2 = new sbyte[0x10];
			sbyte[] hashBuf = new sbyte[0x10];

			// Copy the hash stored by hleDrmBBCipherInit.
			Array.Copy(ctx.buf, 0, dataBuf, 0x14, 0x10);

			if (ctx.mode == 0x2)
			{
				// Decryption mode 0x02: XOR the hash with AMCTRL keys and decrypt with KIRK CMD8.
				dataBuf = xorHash(dataBuf, 0x14, KeyVault.amHashKey5, 0, 0x10);
				ScrambleBB(dataBuf, 0x10, 0x100, 5, KIRK.PSP_KIRK_CMD_DECRYPT_FUSE);
				dataBuf = xorHash(dataBuf, 0, KeyVault.amHashKey4, 0, 0x10);
			}
			else
			{
				// Decryption mode 0x01: XOR the hash with AMCTRL keys and decrypt with KIRK CMD7.
				dataBuf = xorHash(dataBuf, 0x14, KeyVault.amHashKey5, 0, 0x10);
				ScrambleBB(dataBuf, 0x10, 0x39, 5, KIRK.PSP_KIRK_CMD_DECRYPT);
				dataBuf = xorHash(dataBuf, 0, KeyVault.amHashKey4, 0, 0x10);
			}

			// Store the calculated key.
			Array.Copy(dataBuf, 0, keyBuf2, 0, 0x10);

			// Apply extra padding if ctx.seed is not 1.
			if (ctx.seed != 0x1)
			{
				Array.Copy(keyBuf2, 0, keyBuf1, 0, 0xC);
				keyBuf1[0xC] = unchecked((sbyte)((ctx.seed - 1) & 0xFF));
				keyBuf1[0xD] = unchecked((sbyte)(((ctx.seed - 1) >> 8) & 0xFF));
				keyBuf1[0xE] = unchecked((sbyte)(((ctx.seed - 1) >> 16) & 0xFF));
				keyBuf1[0xF] = unchecked((sbyte)(((ctx.seed - 1) >> 24) & 0xFF));
			}

			// Copy the first 0xC bytes of the obtained key and replicate them
			// across a new list buffer. As a terminator, add the ctx.seed parameter's
			// 4 bytes (endian swapped) to achieve a full numbered list.
			for (int i = 0x14; i < (length + 0x14); i += 0x10)
			{
				Array.Copy(keyBuf2, 0, dataBuf, i, 0xC);
				dataBuf[i + 0xC] = unchecked((sbyte)(ctx.seed & 0xFF));
				dataBuf[i + 0xD] = unchecked((sbyte)((ctx.seed >> 8) & 0xFF));
				dataBuf[i + 0xE] = unchecked((sbyte)((ctx.seed >> 16) & 0xFF));
				dataBuf[i + 0xF] = unchecked((sbyte)((ctx.seed >> 24) & 0xFF));
				ctx.seed++;
			}

			// Copy the generated hash to hashBuf.
			Array.Copy(dataBuf, length + 0x04, hashBuf, 0, 0x10);

			// Decrypt the hash with KIRK CMD7 and seed 0x63.
			ScrambleBB(dataBuf, length, 0x63, 5, KIRK.PSP_KIRK_CMD_DECRYPT);

			// XOR the first 16-bytes of data with the saved key to generate a new hash.
			dataBuf = xorKey(dataBuf, 0, keyBuf1, 0, 0x10);

			// Copy back the last hash from the list to the first keyBuf.
			Array.Copy(hashBuf, 0, keyBuf1, 0, 0x10);

			// Finally, XOR the full list with the given data.
			xorKey(data, data_offset, dataBuf, 0, length);
		}

		/*
		 * sceDrmBB - amctrl.prx
		 */
		public virtual int hleDrmBBMacInit(BBMac_Ctx ctx, int encMode)
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

		public virtual int hleDrmBBMacUpdate(BBMac_Ctx ctx, sbyte[] data, int length)
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
						ScrambleBB(scrambleBuf, blockSize, seed, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);
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
					ScrambleBB(scrambleBuf, nLen, seed, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);
					Array.Copy(scrambleBuf, nLen + 0x4, ctx.key, 0, 0x10);
				}

				return 0;
			}
		}

		public virtual int hleDrmBBMacFinal(BBMac_Ctx ctx, sbyte[] hash, sbyte[] key)
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
			ScrambleBB(scrambleBuf, 0x10, seed, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);

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
			ScrambleBB(scrambleBuf, 0x10, seed, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);

			// Copy back the key into the result buffer.
			Array.Copy(scrambleBuf, 0x14, resultBuf, 0, 0x10);

			// XOR with amHashKey3.
			resultBuf = xorHash(resultBuf, 0, KeyVault.amHashKey3, 0, 0x10);

			// If mode is 2, encrypt again with KIRK CMD 5 and then KIRK CMD 4.
			if (ctx.mode == 0x2)
			{
				// Copy the result buffer into the data buffer.
				Array.Copy(resultBuf, 0, scrambleBuf, 0x14, 0x10);

				// Encrypt with KIRK CMD 5 (seed is always 0x100).
				ScrambleBB(scrambleBuf, 0x10, 0x100, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT_FUSE);

				// Encrypt again with KIRK CMD 4.
				ScrambleBB(scrambleBuf, 0x10, seed, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);

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
				ScrambleBB(scrambleBuf, 0x10, seed, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);

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

		public virtual int hleDrmBBMacFinal2(BBMac_Ctx ctx, sbyte[] hash, sbyte[] key)
		{
			sbyte[] resBuf = new sbyte[0x10];
			sbyte[] hashBuf = new sbyte[0x10];

			int mode = ctx.mode;

			// Call hleDrmBBMacFinal on an empty buffer.
			hleDrmBBMacFinal(ctx, resBuf, key);

			// If mode is 3, decrypt the hash first.
			if ((mode & 0x3) == 0x3)
			{
				hashBuf = DecryptBBMacKey(hash, 0x63);
			}
			else
			{
				hashBuf = hash;
			}

			// Compare the hashes.
			for (int i = 0; i < 0x10; i++)
			{
				if (hashBuf[i] != resBuf[i])
				{
					return -1;
				}
			}

			return 0;
		}

		public virtual int hleDrmBBCipherInit(BBCipher_Ctx ctx, int encMode, int genMode, sbyte[] data, sbyte[] key)
		{
			return hleDrmBBCipherInit(ctx, encMode, genMode, data, key, 0);
		}

		public virtual int hleDrmBBCipherInit(BBCipher_Ctx ctx, int encMode, int genMode, sbyte[] data, sbyte[] key, int seed)
		{
			// If the key is not a 16-byte key, return an error.
			if (key.Length < 0x10)
			{
				return -1;
			}

			// Set the mode and the unknown parameters.
			ctx.mode = encMode;
			ctx.seed = seed + 0x1;

			// Key generator mode 0x1 (encryption): use an encrypted pseudo random number before XORing the data with the given key.
			if (genMode == 0x1)
			{
				sbyte[] header = new sbyte[0x24];
				sbyte[] rseed = new sbyte[0x14];

				// Generate SHA-1 to act as seed for encryption.
				ByteBuffer bSeed = ByteBuffer.wrap(rseed);
				kirk.hleUtilsBufferCopyWithRange(bSeed, 0x14, null, 0, KIRK.PSP_KIRK_CMD_PRNG);

				// Propagate SHA-1 in kirk header.
				Array.Copy(bSeed.array(), 0, header, 0, 0x14);
				Array.Copy(bSeed.array(), 0, header, 0x14, 0x10);
				header[0x20] = 0;
				header[0x21] = 0;
				header[0x22] = 0;
				header[0x23] = 0;

				if (ctx.mode == 0x2)
				{ // Encryption mode 0x2: XOR with AMCTRL keys, encrypt with KIRK CMD5 and XOR with the given key.
					header = xorHash(header, 0x14, KeyVault.amHashKey4, 0, 0x10);
					ScrambleBB(header, 0x10, 0x100, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT_FUSE);
					header = xorHash(header, 0x14, KeyVault.amHashKey5, 0, 0x10);
					Array.Copy(header, 0x14, ctx.buf, 0, 0x10);
					Array.Copy(header, 0x14, data, 0, 0x10);
					// If the key is not null, XOR the hash with it.
					if (!isNullKey(key))
					{
						ctx.buf = xorKey(ctx.buf, 0, key, 0, 0x10);
					}
				}
				else
				{ // Encryption mode 0x1: XOR with AMCTRL keys, encrypt with KIRK CMD4 and XOR with the given key.
					header = xorHash(header, 0x14, KeyVault.amHashKey4, 0, 0x10);
					ScrambleBB(header, 0x10, 0x39, 0x4, KIRK.PSP_KIRK_CMD_ENCRYPT);
					header = xorHash(header, 0x14, KeyVault.amHashKey5, 0, 0x10);
					Array.Copy(header, 0x14, ctx.buf, 0, 0x10);
					Array.Copy(header, 0x14, data, 0, 0x10);
					// If the key is not null, XOR the hash with it.
					if (!isNullKey(key))
					{
						ctx.buf = xorKey(ctx.buf, 0, key, 0, 0x10);
					}
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
			}
			else
			{
				// Invalid mode.
				return -1;
			}

			return 0;
		}

		public virtual int hleDrmBBCipherUpdate(BBCipher_Ctx ctx, sbyte[] data, int length)
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
					cipherMember(ctx, data, index, 0x800);
					length -= 0x800;
				}
			}

			// Finally parse the rest of the data.
			if (length >= 0x10)
			{
				cipherMember(ctx, data, index, length);
			}

			return 0;
		}

		public virtual int hleDrmBBCipherFinal(BBCipher_Ctx ctx)
		{
			ctx.mode = 0;
			ctx.seed = 0;
			for (int i = 0; i < 0x10; i++)
			{
				ctx.buf[i] = 0;
			}
			return 0;
		}

		public virtual sbyte[] DecryptBBMacKey(sbyte[] key, int seed)
		{
			sbyte[] scrambleBuf = new sbyte[0x10 + 0x14];
			sbyte[] decKey = new sbyte[0x10];

			Array.Copy(key, 0, scrambleBuf, 0x14, 0x10);
			ScrambleBB(scrambleBuf, 0x10, seed, 0x5, KIRK.PSP_KIRK_CMD_DECRYPT);
			Array.Copy(scrambleBuf, 0, decKey, 0, 0x10);

			return decKey;
		}

		public virtual sbyte[] GetKeyFromBBMac(BBMac_Ctx ctx, sbyte[] bbmac)
		{
			sbyte[] key = new sbyte[0x10];
			sbyte[] decKey = new sbyte[0x10];
			sbyte[] macKey = new sbyte[0x10];
			sbyte[] finalKey = new sbyte[0x10];

			int mode = ctx.mode; // This will be reset to 0 by hleDrmBBMacFinal
			hleDrmBBMacFinal(ctx, macKey, null);

			if ((mode & 0x3) == 0x3)
			{
				decKey = DecryptBBMacKey(bbmac, 0x63);
			}
			else
			{
				Array.Copy(bbmac, 0, decKey, 0, 0x10);
			}

			int seed = getModeSeed(mode);
			finalKey = DecryptBBMacKey(decKey, seed);

			key = xorKey(macKey, 0, finalKey, 0, 0x10);

			return key;
		}
	}

}