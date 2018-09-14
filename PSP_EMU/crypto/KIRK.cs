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

	using Utilities = pspsharp.util.Utilities;

	public class KIRK
	{

		// PSP specific values.
		private int fuseID0;
		private int fuseID1;
		private sbyte[] priv_iv = new sbyte[0x10];
		private sbyte[] prng_data = new sbyte[0x14];

		// KIRK error values.
		public const int PSP_KIRK_NOT_ENABLED = 0x1;
		public const int PSP_KIRK_INVALID_MODE = 0x2;
		public const int PSP_KIRK_INVALID_HEADER_HASH = 0x3;
		public const int PSP_KIRK_INVALID_DATA_HASH = 0x4;
		public const int PSP_KIRK_INVALID_SIG_CHECK = 0x5;
		public const int PSP_KIRK_UNK1 = 0x6;
		public const int PSP_KIRK_UNK2 = 0x7;
		public const int PSP_KIRK_UNK3 = 0x8;
		public const int PSP_KIRK_UNK4 = 0x9;
		public const int PSP_KIRK_UNK5 = 0xA;
		public const int PSP_KIRK_UNK6 = 0xB;
		public const int PSP_KIRK_NOT_INIT = 0xC;
		public const int PSP_KIRK_INVALID_OPERATION = 0xD;
		public const int PSP_KIRK_INVALID_SEED = 0xE;
		public const int PSP_KIRK_INVALID_SIZE = 0xF;
		public const int PSP_KIRK_DATA_SIZE_IS_ZERO = 0x10;
		public const int PSP_SUBCWR_NOT_16_ALGINED = 0x90A;
		public const int PSP_SUBCWR_HEADER_HASH_INVALID = 0x920;
		public const int PSP_SUBCWR_BUFFER_TOO_SMALL = 0x1000;

		// KIRK commands.
		public const int PSP_KIRK_CMD_DECRYPT_PRIVATE = 0x1; // Master decryption command, used by firmware modules. Applies CMAC checking.
		public const int PSP_KIRK_CMD_ENCRYPT_SIGN = 0x2; // Used for key type 3 (blacklisting), encrypts and signs data with a ECDSA signature.
		public const int PSP_KIRK_CMD_DECRYPT_SIGN = 0x3; // Used for key type 3 (blacklisting), decrypts and signs data with a ECDSA signature.
		public const int PSP_KIRK_CMD_ENCRYPT = 0x4; // Key table based encryption used for general purposes by several modules.
		public const int PSP_KIRK_CMD_ENCRYPT_FUSE = 0x5; // Fuse ID based encryption used for general purposes by several modules.
		public const int PSP_KIRK_CMD_ENCRYPT_USER = 0x6; // User specified ID based encryption used for general purposes by several modules.
		public const int PSP_KIRK_CMD_DECRYPT = 0x7; // Key table based decryption used for general purposes by several modules.
		public const int PSP_KIRK_CMD_DECRYPT_FUSE = 0x8; // Fuse ID based decryption used for general purposes by several modules.
		public const int PSP_KIRK_CMD_DECRYPT_USER = 0x9; // User specified ID based decryption used for general purposes by several modules.
		public const int PSP_KIRK_CMD_PRIV_SIG_CHECK = 0xA; // Private signature (SCE) checking command.
		public const int PSP_KIRK_CMD_SHA1_HASH = 0xB; // SHA1 hash generating command.
		public const int PSP_KIRK_CMD_ECDSA_GEN_KEYS = 0xC; // ECDSA key generating mul1 command.
		public const int PSP_KIRK_CMD_ECDSA_MULTIPLY_POINT = 0xD; // ECDSA key generating mul2 command.
		public const int PSP_KIRK_CMD_PRNG = 0xE; // Random number generating command.
		public const int PSP_KIRK_CMD_INIT = 0xF; // KIRK initialization command.
		public const int PSP_KIRK_CMD_ECDSA_SIGN = 0x10; // ECDSA signing command.
		public const int PSP_KIRK_CMD_ECDSA_VERIFY = 0x11; // ECDSA checking command.
		public const int PSP_KIRK_CMD_CERT_VERIFY = 0x12; // Certificate checking command.

		// KIRK command modes.
		public const int PSP_KIRK_CMD_MODE_CMD1 = 0x1;
		public const int PSP_KIRK_CMD_MODE_CMD2 = 0x2;
		public const int PSP_KIRK_CMD_MODE_CMD3 = 0x3;
		public const int PSP_KIRK_CMD_MODE_ENCRYPT_CBC = 0x4;
		public const int PSP_KIRK_CMD_MODE_DECRYPT_CBC = 0x5;

		// KIRK header structs.
		private class SHA1_Header
		{
			private readonly KIRK outerInstance;


			internal int dataSize;
			internal sbyte[] data;

			public SHA1_Header(KIRK outerInstance, ByteBuffer buf)
			{
				this.outerInstance = outerInstance;
				dataSize = buf.Int;
			}

			internal virtual void readData(ByteBuffer buf, int size)
			{
				data = new sbyte[size];
				buf.get(data, 0, size);
			}
		}

		private class AES128_CBC_Header
		{

			internal int mode;
			internal int unk1;
			internal int unk2;
			internal int keySeed;
			internal int dataSize;

			public AES128_CBC_Header(ByteBuffer buf)
			{
				mode = buf.Int;
				unk1 = buf.Int;
				unk2 = buf.Int;
				keySeed = buf.Int;
				dataSize = buf.Int;
			}
		}

		private class AES128_CMAC_Header
		{

			internal sbyte[] AES128Key = new sbyte[16];
			internal sbyte[] CMACKey = new sbyte[16];
			internal sbyte[] CMACHeaderHash = new sbyte[16];
			internal sbyte[] CMACDataHash = new sbyte[16];
			internal sbyte[] unk1 = new sbyte[32];
			internal int mode;
			internal sbyte useECDSAhash;
			internal sbyte[] unk2 = new sbyte[11];
			internal int dataSize;
			internal int dataOffset;
			internal sbyte[] unk3 = new sbyte[8];
			internal sbyte[] unk4 = new sbyte[16];

			public AES128_CMAC_Header(ByteBuffer buf)
			{
				buf.get(AES128Key, 0, 16);
				buf.get(CMACKey, 0, 16);
				buf.get(CMACHeaderHash, 0, 16);
				buf.get(CMACDataHash, 0, 16);
				buf.get(unk1, 0, 32);
				mode = buf.Int;
				useECDSAhash = buf.get();
				buf.get(unk2, 0, 11);
				dataSize = buf.Int;
				dataOffset = buf.Int;
				buf.get(unk3, 0, 8);
				buf.get(unk4, 0, 16);

				// For PRX, the mode is big-endian, for direct sceKernelUtilsCopyWithRange,
				// the mode is little-endian. I don't know how to better differentiate these cases.
				if ((mode & 0x00FFFFFF) == 0x000000)
				{
					mode = Integer.reverseBytes(mode);
				}
			}

			public static int SIZEOF()
			{
				return 144;
			}
		}

		private class AES128_CMAC_ECDSA_Header
		{

			internal sbyte[] AES128Key = new sbyte[16];
			internal sbyte[] ECDSAHeaderSig_r = new sbyte[20];
			internal sbyte[] ECDSAHeaderSig_s = new sbyte[20];
			internal sbyte[] ECDSADataSig_r = new sbyte[20];
			internal sbyte[] ECDSADataSig_s = new sbyte[20];
			internal int mode;
			internal sbyte useECDSAhash;
			internal sbyte[] unk1 = new sbyte[11];
			internal int dataSize;
			internal int dataOffset;
			internal sbyte[] unk2 = new sbyte[8];
			internal sbyte[] unk3 = new sbyte[16];

			public AES128_CMAC_ECDSA_Header(ByteBuffer buf)
			{
				buf.get(AES128Key, 0, 16);
				buf.get(ECDSAHeaderSig_r, 0, 20);
				buf.get(ECDSAHeaderSig_s, 0, 20);
				buf.get(ECDSADataSig_r, 0, 20);
				buf.get(ECDSADataSig_s, 0, 20);
				mode = buf.Int;
				useECDSAhash = buf.get();
				buf.get(unk1, 0, 11);
				dataSize = buf.Int;
				dataOffset = buf.Int;
				buf.get(unk2, 0, 8);
				buf.get(unk3, 0, 16);
			}
		}

		private class ECDSASig
		{

			internal sbyte[] r = new sbyte[0x14];
			internal sbyte[] s = new sbyte[0x14];

			internal ECDSASig()
			{
			}
		}

		private class ECDSAPoint
		{

			internal sbyte[] x = new sbyte[0x14];
			internal sbyte[] y = new sbyte[0x14];

			internal ECDSAPoint()
			{
			}

			internal ECDSAPoint(sbyte[] data)
			{
				Array.Copy(data, 0, x, 0, 0x14);
				Array.Copy(data, 0x14, y, 0, 0x14);
			}

			public virtual sbyte[] toByteArray()
			{
				sbyte[] point = new sbyte[0x28];
				Array.Copy(point, 0, x, 0, 0x14);
				Array.Copy(point, 0x14, y, 0, 0x14);
				return point;
			}
		}

		private class ECDSAKeygenCtx
		{

			internal sbyte[] private_key = new sbyte[0x14];
			internal ECDSAPoint public_key;
			internal ByteBuffer @out;

			internal ECDSAKeygenCtx(ByteBuffer output)
			{
				public_key = new ECDSAPoint();
				@out = output;
			}

			public virtual void write()
			{
				@out.put(private_key);
				@out.put(public_key.toByteArray());
			}
		}

		private class ECDSAMultiplyCtx
		{

			internal sbyte[] multiplier = new sbyte[0x14];
			internal ECDSAPoint public_key = new ECDSAPoint();
			internal ByteBuffer @out;

			internal ECDSAMultiplyCtx(ByteBuffer input, ByteBuffer output)
			{
				@out = output;
				input.get(multiplier, 0, 0x14);
				input.get(public_key.x, 0, 0x14);
				input.get(public_key.y, 0, 0x14);
			}

			public virtual void write()
			{
				@out.put(multiplier);
				@out.put(public_key.toByteArray());
			}
		}

		private class ECDSASignCtx
		{

			internal sbyte[] enc = new sbyte[0x20];
			internal sbyte[] hash = new sbyte[0x14];

			internal ECDSASignCtx(ByteBuffer buf)
			{
				buf.get(enc, 0, 0x20);
				buf.get(hash, 0, 0x14);
			}
		}

		private class ECDSAVerifyCtx
		{

			internal ECDSAPoint public_key = new ECDSAPoint();
			internal sbyte[] hash = new sbyte[0x14];
			internal ECDSASig sig = new ECDSASig();

			internal ECDSAVerifyCtx(ByteBuffer buf)
			{
				buf.get(public_key.x, 0, 0x14);
				buf.get(public_key.y, 0, 0x14);
				buf.get(hash, 0, 0x14);
				buf.get(sig.r, 0, 0x14);
				buf.get(sig.s, 0, 0x14);
			}
		}

		// Helper functions.
		private static int[] getAESKeyFromSeed(int seed)
		{
			switch (seed)
			{
				case (0x02):
					return KeyVault.kirkAESKey20;
				case (0x03):
					return KeyVault.kirkAESKey1;
				case (0x04):
					return KeyVault.kirkAESKey2;
				case (0x05):
					return KeyVault.kirkAESKey3;
				case (0x07):
					return KeyVault.kirkAESKey21;
				case (0x0C):
					return KeyVault.kirkAESKey4;
				case (0x0D):
					return KeyVault.kirkAESKey5;
				case (0x0E):
					return KeyVault.kirkAESKey6;
				case (0x0F):
					return KeyVault.kirkAESKey7;
				case (0x10):
					return KeyVault.kirkAESKey8;
				case (0x11):
					return KeyVault.kirkAESKey9;
				case (0x12):
					return KeyVault.kirkAESKey10;
				case (0x38):
					return KeyVault.kirkAESKey11;
				case (0x39):
					return KeyVault.kirkAESKey12;
				case (0x3A):
					return KeyVault.kirkAESKey13;
				case (0x44):
					return KeyVault.kirkAESKey22;
				case (0x4B):
					return KeyVault.kirkAESKey14;
				case (0x53):
					return KeyVault.kirkAESKey15;
				case (0x57):
					return KeyVault.kirkAESKey16;
				case (0x5D):
					return KeyVault.kirkAESKey17;
				case (0x63):
					return KeyVault.kirkAESKey18;
				case (0x64):
					return KeyVault.kirkAESKey19;
				default:
					return null;
			}
		}

		public KIRK()
		{
		}

		public KIRK(sbyte[] seed, int seedLength, int fuseid0, int fuseid1)
		{
			// Set up the data for the pseudo random number generator using a
			// seed set by the user.
			sbyte[] temp = new sbyte[0x104];
			temp[0] = 0;
			temp[1] = 0;
			temp[2] = 1;
			temp[3] = 0;

			ByteBuffer bTemp = ByteBuffer.wrap(temp);
			ByteBuffer bPRNG = ByteBuffer.wrap(prng_data);

			// Random data to act as a key.
			sbyte[] key = new sbyte[] {(sbyte) 0x07, unchecked((sbyte) 0xAB), unchecked((sbyte) 0xEF), unchecked((sbyte) 0xF8), unchecked((sbyte) 0x96), unchecked((sbyte) 0x8C), unchecked((sbyte) 0xF3), unchecked((sbyte) 0xD6), (sbyte) 0x14, unchecked((sbyte) 0xE0), unchecked((sbyte) 0xEB), unchecked((sbyte) 0xB2), unchecked((sbyte) 0x9D), unchecked((sbyte) 0x8B), (sbyte) 0x4E, (sbyte) 0x74};

			// Direct call to get the system time.
			int systime = (int) DateTimeHelper.CurrentUnixTimeMillis();

			// Generate a SHA-1 hash for the PRNG.
			if (seedLength > 0)
			{
				sbyte[] seedBuf = new sbyte[seedLength + 4];
				ByteBuffer bSeedBuf = ByteBuffer.wrap(seedBuf);

				SHA1_Header seedHeader = new SHA1_Header(this, bSeedBuf);
				bSeedBuf.rewind();

				seedHeader.dataSize = seedLength;
				executeKIRKCmd11(bPRNG, bSeedBuf, seedLength + 4);
			}

			// Use the system time for randomness.
			Array.Copy(prng_data, 0, temp, 4, 0x14);
			temp[0x18] = unchecked((sbyte)(systime & 0xFF));
			temp[0x19] = unchecked((sbyte)((systime >> 8) & 0xFF));
			temp[0x1A] = unchecked((sbyte)((systime >> 16) & 0xFF));
			temp[0x1B] = unchecked((sbyte)((systime >> 24) & 0xFF));

			// Set the final PRNG number.
			Array.Copy(key, 0, temp, 0x1C, 0x10);
			bPRNG.clear();
			executeKIRKCmd11(bPRNG, bTemp, 0x104);

			fuseID0 = fuseid0;
			fuseID1 = fuseid1;
		}

		/*
		 * KIRK commands: main emulated crypto functions.
		 */
		// Decrypt with AESCBC128-CMAC header and sig check.
		private int executeKIRKCmd1(ByteBuffer @out, ByteBuffer @in, int size)
		{
			// Return an error if the crypto engine hasn't been initialized.
			if (!CryptoEngine.CryptoEngineStatus)
			{
				return PSP_KIRK_NOT_INIT;
			}

			int outPosition = @out.position();

			// Copy the input for sig check.
			ByteBuffer sigIn = @in.duplicate();
			sigIn.order(@in.order()); // duplicate() does not copy the order()

			int headerSize = AES128_CMAC_Header.SIZEOF();
			int headerOffset = @in.position();

			// Read in the CMD1 format header.
			AES128_CMAC_Header header = new AES128_CMAC_Header(@in);

			if (header.mode != PSP_KIRK_CMD_MODE_CMD1)
			{
				return PSP_KIRK_INVALID_MODE; // Only valid for mode CMD1.
			}

			// Start AES128 processing.
			AES128 aes = new AES128("AES/CBC/NoPadding");

			// Convert the AES CMD1 key into a real byte array for SecretKeySpec.
			sbyte[] k = new sbyte[16];
			for (int i = 0; i < KeyVault.kirkAESKey0.Length; i++)
			{
				k[i] = (sbyte) KeyVault.kirkAESKey0[i];
			}

			// Decrypt and extract the new AES and CMAC keys from the top of the data.
			sbyte[] encryptedKeys = new sbyte[32];
			Array.Copy(header.AES128Key, 0, encryptedKeys, 0, 16);
			Array.Copy(header.CMACKey, 0, encryptedKeys, 16, 16);
			sbyte[] decryptedKeys = aes.decrypt(encryptedKeys, k, priv_iv);

			// Check for a valid signature.
			int sigCheck = executeKIRKCmd10(sigIn, size);

			if (decryptedKeys == null)
			{
				// Only return the sig check result if the keys are invalid
				// to allow skipping the CMAC comparision.
				// TODO: Trace why the CMAC hashes aren't matching.
				return sigCheck;
			}

			// Get the newly decrypted AES key and proceed with the
			// full data decryption.
			sbyte[] aesBuf = new sbyte[16];
			Array.Copy(decryptedKeys, 0, aesBuf, 0, aesBuf.Length);

			// Extract the final ELF params.
			int elfDataSize = header.dataSize;
			int elfDataOffset = header.dataOffset;

			// Input buffer for decryption must have a length aligned on 16 bytes
			int paddedElfDataSize = Utilities.alignUp(elfDataSize, 15);

			// Decrypt all the ELF data.
			sbyte[] inBuf = new sbyte[paddedElfDataSize];
			Array.Copy(@in.array(), elfDataOffset + headerOffset + headerSize, inBuf, 0, paddedElfDataSize);
			sbyte[] outBuf = aes.decrypt(inBuf, aesBuf, priv_iv);

			@out.position(outPosition);
			@out.put(outBuf);
			@out.limit(elfDataSize);
			@in.clear();

			return 0;
		}

		// Encrypt with AESCBC128 using keys from table.
		private int executeKIRKCmd4(ByteBuffer @out, ByteBuffer @in, int size)
		{
			// Return an error if the crypto engine hasn't been initialized.
			if (!CryptoEngine.CryptoEngineStatus)
			{
				return PSP_KIRK_NOT_INIT;
			}

			int outPosition = @out.position();

			// Read in the CMD4 format header.
			AES128_CBC_Header header = new AES128_CBC_Header(@in);

			if (header.mode != PSP_KIRK_CMD_MODE_ENCRYPT_CBC)
			{
				return PSP_KIRK_INVALID_MODE; // Only valid for mode ENCRYPT_CBC.
			}

			if (header.dataSize == 0)
			{
				return PSP_KIRK_DATA_SIZE_IS_ZERO;
			}

			int[] key = getAESKeyFromSeed(header.keySeed);
			if (key == null)
			{
				return PSP_KIRK_INVALID_SEED;
			}

			sbyte[] encKey = new sbyte[16];
			for (int i = 0; i < encKey.Length; i++)
			{
				encKey[i] = (sbyte) key[i];
			}

			AES128 aes = new AES128("AES/CBC/NoPadding");

			sbyte[] inBuf = new sbyte[header.dataSize];
			@in.get(inBuf, 0, header.dataSize);
			sbyte[] outBuf = aes.encrypt(inBuf, encKey, priv_iv);

			@out.position(outPosition);
			// The header is kept in the output and the header.mode is even updated from
			// PSP_KIRK_CMD_MODE_ENCRYPT_CBC to PSP_KIRK_CMD_MODE_DECRYPT_CBC.
			@out.putInt(PSP_KIRK_CMD_MODE_DECRYPT_CBC);
			@out.putInt(header.unk1);
			@out.putInt(header.unk2);
			@out.putInt(header.keySeed);
			@out.putInt(header.dataSize);
			@out.put(outBuf);
			@in.clear();

			return 0;
		}

		// Encrypt with AESCBC128 using keys from table.
		private int executeKIRKCmd5(ByteBuffer @out, ByteBuffer @in, int size)
		{
			// Return an error if the crypto engine hasn't been initialized.
			if (!CryptoEngine.CryptoEngineStatus)
			{
				return PSP_KIRK_NOT_INIT;
			}

			int outPosition = @out.position();

			// Read in the CMD4 format header.
			AES128_CBC_Header header = new AES128_CBC_Header(@in);

			if (header.mode != PSP_KIRK_CMD_MODE_ENCRYPT_CBC)
			{
				return PSP_KIRK_INVALID_MODE; // Only valid for mode ENCRYPT_CBC.
			}

			if (header.dataSize == 0)
			{
				return PSP_KIRK_DATA_SIZE_IS_ZERO;
			}

			sbyte[] key = null;
			if (header.keySeed == 0x100)
			{
				key = priv_iv;
			}
			else
			{
				return PSP_KIRK_INVALID_SIZE; // Dummy.
			}

			sbyte[] encKey = new sbyte[16];
			for (int i = 0; i < encKey.Length; i++)
			{
				encKey[i] = (sbyte) key[i];
			}

			AES128 aes = new AES128("AES/CBC/NoPadding");

			sbyte[] inBuf = new sbyte[header.dataSize];
			@in.get(inBuf, 0, header.dataSize);
			sbyte[] outBuf = aes.encrypt(inBuf, encKey, priv_iv);

			@out.position(outPosition);
			// The header is kept in the output and the header.mode is even updated from
			// PSP_KIRK_CMD_MODE_ENCRYPT_CBC to PSP_KIRK_CMD_MODE_DECRYPT_CBC.
			@out.putInt(PSP_KIRK_CMD_MODE_DECRYPT_CBC);
			@out.putInt(header.unk1);
			@out.putInt(header.unk2);
			@out.putInt(header.keySeed);
			@out.putInt(header.dataSize);
			@out.put(outBuf);
			@in.clear();

			return 0;
		}

		// Decrypt with AESCBC128 using keys from table.
		private int executeKIRKCmd7(ByteBuffer @out, ByteBuffer @in, int size)
		{
			// Return an error if the crypto engine hasn't been initialized.
			if (!CryptoEngine.CryptoEngineStatus)
			{
				return PSP_KIRK_NOT_INIT;
			}

			int outPosition = @out.position();

			// Read in the CMD7 format header.
			AES128_CBC_Header header = new AES128_CBC_Header(@in);

			if (header.mode != PSP_KIRK_CMD_MODE_DECRYPT_CBC)
			{
				return PSP_KIRK_INVALID_MODE; // Only valid for mode DECRYPT_CBC.
			}

			if (header.dataSize == 0)
			{
				return PSP_KIRK_DATA_SIZE_IS_ZERO;
			}

			int[] key = getAESKeyFromSeed(header.keySeed);
			if (key == null)
			{
				return PSP_KIRK_INVALID_SEED;
			}

			sbyte[] decKey = new sbyte[16];
			for (int i = 0; i < decKey.Length; i++)
			{
				decKey[i] = (sbyte) key[i];
			}

			AES128 aes = new AES128("AES/CBC/NoPadding");

			sbyte[] inBuf = new sbyte[header.dataSize];
			@in.get(inBuf, 0, header.dataSize);
			sbyte[] outBuf = aes.decrypt(inBuf, decKey, priv_iv);

			@out.position(outPosition);
			@out.put(outBuf);
			@in.clear();

			return 0;
		}

		// Decrypt with AESCBC128 using keys from table.
		private int executeKIRKCmd8(ByteBuffer @out, ByteBuffer @in, int size)
		{
			// Return an error if the crypto engine hasn't been initialized.
			if (!CryptoEngine.CryptoEngineStatus)
			{
				return PSP_KIRK_NOT_INIT;
			}

			int outPosition = @out.position();

			// Read in the CMD7 format header.
			AES128_CBC_Header header = new AES128_CBC_Header(@in);

			if (header.mode != PSP_KIRK_CMD_MODE_DECRYPT_CBC)
			{
				return PSP_KIRK_INVALID_MODE; // Only valid for mode DECRYPT_CBC.
			}

			if (header.dataSize == 0)
			{
				return PSP_KIRK_DATA_SIZE_IS_ZERO;
			}

			sbyte[] key = null;
			if (header.keySeed == 0x100)
			{
				key = priv_iv;
			}
			else
			{
				return PSP_KIRK_INVALID_SIZE; // Dummy.
			}

			sbyte[] decKey = new sbyte[16];
			for (int i = 0; i < decKey.Length; i++)
			{
				decKey[i] = (sbyte) key[i];
			}

			AES128 aes = new AES128("AES/CBC/NoPadding");

			sbyte[] inBuf = new sbyte[header.dataSize];
			@in.get(inBuf, 0, header.dataSize);
			sbyte[] outBuf = aes.decrypt(inBuf, decKey, priv_iv);

			@out.position(outPosition);
			@out.put(outBuf);
			@in.clear();

			return 0;
		}

		// CMAC Sig check.
		private int executeKIRKCmd10(ByteBuffer @in, int size)
		{
			// Return an error if the crypto engine hasn't been initialized.
			if (!CryptoEngine.CryptoEngineStatus)
			{
				return PSP_KIRK_NOT_INIT;
			}

			int headerOffset = @in.position();

			// Read in the CMD10 format header.
			AES128_CMAC_Header header = new AES128_CMAC_Header(@in);
			if ((header.mode != PSP_KIRK_CMD_MODE_CMD1) && (header.mode != PSP_KIRK_CMD_MODE_CMD2) && (header.mode != PSP_KIRK_CMD_MODE_CMD3))
			{
				return PSP_KIRK_INVALID_MODE; // Only valid for modes CMD1, CMD2 and CMD3.
			}

			if (header.dataSize == 0)
			{
				return PSP_KIRK_DATA_SIZE_IS_ZERO;
			}

			AES128 aes = new AES128("AES/CBC/NoPadding");

			// Convert the AES CMD1 key into a real byte array.
			sbyte[] k = new sbyte[16];
			for (int i = 0; i < KeyVault.kirkAESKey0.Length; i++)
			{
				k[i] = (sbyte) KeyVault.kirkAESKey0[i];
			}

			// Decrypt and extract the new AES and CMAC keys from the top of the data.
			sbyte[] encryptedKeys = new sbyte[32];
			Array.Copy(header.AES128Key, 0, encryptedKeys, 0, 16);
			Array.Copy(header.CMACKey, 0, encryptedKeys, 16, 16);
			sbyte[] decryptedKeys = aes.decrypt(encryptedKeys, k, priv_iv);

			sbyte[] cmacHeaderHash = new sbyte[16];
			sbyte[] cmacDataHash = new sbyte[16];

			sbyte[] cmacBuf = new sbyte[16];
			Array.Copy(decryptedKeys, 16, cmacBuf, 0, cmacBuf.Length);

			// Position the buffer at the CMAC keys offset.
			sbyte[] inBuf = new sbyte[@in.capacity() - 0x60 - headerOffset];
			Array.Copy(@in.array(), headerOffset + 0x60, inBuf, 0, inBuf.Length);

			// Calculate CMAC header hash.
			aes.doInitCMAC(cmacBuf);
			aes.doUpdateCMAC(inBuf, 0, 0x30);
			cmacHeaderHash = aes.doFinalCMAC();

			int blockSize = header.dataSize;
			if ((blockSize % 16) != 0)
			{
				blockSize += (16 - (blockSize % 16));
			}

			// Calculate CMAC data hash.
			aes.doInitCMAC(cmacBuf);
			aes.doUpdateCMAC(inBuf, 0, 0x30 + blockSize + header.dataOffset);
			cmacDataHash = aes.doFinalCMAC();

			for (int i = 0; i < cmacHeaderHash.Length; i++)
			{
				if (cmacHeaderHash[i] != header.CMACHeaderHash[i])
				{
					return PSP_KIRK_INVALID_HEADER_HASH;
				}
			}

			for (int i = 0; i < cmacDataHash.Length; i++)
			{
				if (cmacDataHash[i] != header.CMACDataHash[i])
				{
					return PSP_KIRK_INVALID_DATA_HASH;
				}
			}

			return 0;
		}

		// Generate SHA1 hash.
		private int executeKIRKCmd11(ByteBuffer @out, ByteBuffer @in, int size)
		{
			// Return an error if the crypto engine hasn't been initialized.
			if (!CryptoEngine.CryptoEngineStatus)
			{
				return PSP_KIRK_NOT_INIT;
			}

			int outPosition = @out.position();

			SHA1_Header header = new SHA1_Header(this, @in);
			SHA1 sha1 = new SHA1();

			size = (size < header.dataSize) ? size : header.dataSize;
			header.readData(@in, size);

			@out.position(outPosition);
			@out.put(sha1.doSHA1(header.data, size));
			@in.clear();

			return 0;
		}

		// Generate ECDSA key pair.
		private int executeKIRKCmd12(ByteBuffer @out, int size)
		{
			// Return an error if the crypto engine hasn't been initialized.
			if (!CryptoEngine.CryptoEngineStatus)
			{
				return PSP_KIRK_NOT_INIT;
			}

			if (size != 0x3C)
			{
				return PSP_KIRK_INVALID_SIZE;
			}

			// Start the ECDSA context.
			ECDSA ecdsa = new ECDSA();
			ECDSAKeygenCtx ctx = new ECDSAKeygenCtx(@out);
			ecdsa.setCurve();

			// Generate the private/public key pair and write it back.
			ctx.private_key = ecdsa.PrivateKey;
			ctx.public_key = new ECDSAPoint(ecdsa.PublicKey);

			ctx.write();

			return 0;
		}

		// Multiply ECDSA point.
		private int executeKIRKCmd13(ByteBuffer @out, int outSize, ByteBuffer @in, int inSize)
		{
			// Return an error if the crypto engine hasn't been initialized.
			if (!CryptoEngine.CryptoEngineStatus)
			{
				return PSP_KIRK_NOT_INIT;
			}

			if ((inSize != 0x3C) || (outSize != 0x28))
			{
				// Accept inSize==0x3C and outSize==0x3C as this is sent by sceMemab_9BF0C95D from a real PSP
				if (outSize != inSize)
				{
					return PSP_KIRK_INVALID_SIZE;
				}
			}

			// Start the ECDSA context.
			ECDSA ecdsa = new ECDSA();
			ECDSAMultiplyCtx ctx = new ECDSAMultiplyCtx(@in, @out);
			ecdsa.setCurve();

			// Multiply the public key.
			ecdsa.multiplyPublicKey(ctx.public_key.toByteArray(), ctx.multiplier);

			ctx.write();

			return 0;
		}

		// Generate pseudo random number.
		private int executeKIRKCmd14(ByteBuffer @out, int size)
		{
			// Return an error if the crypto engine hasn't been initialized.
			if (!CryptoEngine.CryptoEngineStatus)
			{
				return PSP_KIRK_NOT_INIT;
			}

			// Set up a temporary buffer.
			sbyte[] temp = new sbyte[0x104];
			temp[0] = 0;
			temp[1] = 0;
			temp[2] = 1;
			temp[3] = 0;

			ByteBuffer bTemp = ByteBuffer.wrap(temp);

			// Random data to act as a key.
			sbyte[] key = new sbyte[] {unchecked((sbyte) 0xA7), (sbyte) 0x2E, (sbyte) 0x4C, unchecked((sbyte) 0xB6), unchecked((sbyte) 0xC3), (sbyte) 0x34, unchecked((sbyte) 0xDF), unchecked((sbyte) 0x85), (sbyte) 0x70, (sbyte) 0x01, (sbyte) 0x49, unchecked((sbyte) 0xFC), unchecked((sbyte) 0xC0), unchecked((sbyte) 0x87), unchecked((sbyte) 0xC4), (sbyte) 0x77};

			// Direct call to get the system time.
			int systime = (int) DateTimeHelper.CurrentUnixTimeMillis();

			Array.Copy(prng_data, 0, temp, 4, 0x14);
			temp[0x18] = unchecked((sbyte)(systime & 0xFF));
			temp[0x19] = unchecked((sbyte)((systime >> 8) & 0xFF));
			temp[0x1A] = unchecked((sbyte)((systime >> 16) & 0xFF));
			temp[0x1B] = unchecked((sbyte)((systime >> 24) & 0xFF));

			Array.Copy(key, 0, temp, 0x1C, 0x10);

			// Generate a SHA-1 for this PRNG context.
			ByteBuffer bPRNG = ByteBuffer.wrap(prng_data);
			executeKIRKCmd11(bPRNG, bTemp, 0x104);

			@out.put(bPRNG.array());

			// Process the data recursively.
			for (int i = 0; i < size; i += 0x14)
			{
				int remaining = size % 0x14;
				int block = size / 0x14;

				if (block > 0)
				{
					@out.put(bPRNG.array());
					executeKIRKCmd14(@out, i);
				}
				else
				{
					if (remaining > 0)
					{
						@out.put(prng_data, @out.position(), remaining);
						i += remaining;
					}
				}
			}
			@out.rewind();

			return 0;
		}

		// Sign data with ECDSA key pair.
		private int executeKIRKCmd16(ByteBuffer @out, int outSize, ByteBuffer @in, int inSize)
		{
			// Return an error if the crypto engine hasn't been initialized.
			if (!CryptoEngine.CryptoEngineStatus)
			{
				return PSP_KIRK_NOT_INIT;
			}

			if ((inSize != 0x34) || (outSize != 0x28))
			{
				return PSP_KIRK_INVALID_SIZE;
			}

			// TODO
			ECDSA ecdsa = new ECDSA();
			ECDSASignCtx ctx = new ECDSASignCtx(@in);
			ECDSASig sig = new ECDSASig();
			ecdsa.setCurve();

			return 0;
		}

		// Verify ECDSA signature.
		private int executeKIRKCmd17(ByteBuffer @in, int size)
		{
			// Return an error if the crypto engine hasn't been initialized.
			if (!CryptoEngine.CryptoEngineStatus)
			{
				return PSP_KIRK_NOT_INIT;
			}

			if (size != 0x64)
			{
				return PSP_KIRK_INVALID_SIZE;
			}

			// TODO
			ECDSA ecdsa = new ECDSA();
			ECDSAVerifyCtx ctx = new ECDSAVerifyCtx(@in);
			ecdsa.setCurve();

			return 0;
		}

		/*
		 * HLE functions: high level implementation of crypto functions from
		 * several modules which employ various algorithms and communicate with the
		 * crypto engine in different ways.
		 */

		/*
		 * sceUtils - memlmd_01g.prx and memlmd_02g.prx
		 */
		public virtual void hleUtilsSetFuseID(int id0, int id1)
		{
			fuseID0 = id0;
			fuseID1 = id1;
		}

		public virtual int hleUtilsBufferCopyWithRange(ByteBuffer @out, int outsize, ByteBuffer @in, int insize, int cmd)
		{
			return hleUtilsBufferCopyWithRange(@out, outsize, @in, insize, insize, cmd);
		}

		public virtual int hleUtilsBufferCopyWithRange(ByteBuffer @out, int outsize, ByteBuffer @in, int insizeAligned, int insize, int cmd)
		{
			switch (cmd)
			{
				case PSP_KIRK_CMD_DECRYPT_PRIVATE:
					return executeKIRKCmd1(@out, @in, insizeAligned);
				case PSP_KIRK_CMD_ENCRYPT:
					return executeKIRKCmd4(@out, @in, insizeAligned);
				case PSP_KIRK_CMD_ENCRYPT_FUSE:
					return executeKIRKCmd5(@out, @in, insizeAligned);
				case PSP_KIRK_CMD_DECRYPT:
					return executeKIRKCmd7(@out, @in, insizeAligned);
				case PSP_KIRK_CMD_DECRYPT_FUSE:
					return executeKIRKCmd8(@out, @in, insizeAligned);
				case PSP_KIRK_CMD_PRIV_SIG_CHECK:
					return executeKIRKCmd10(@in, insizeAligned);
				case PSP_KIRK_CMD_SHA1_HASH:
					return executeKIRKCmd11(@out, @in, insizeAligned);
				case PSP_KIRK_CMD_ECDSA_GEN_KEYS:
					return executeKIRKCmd12(@out, outsize);
				case PSP_KIRK_CMD_ECDSA_MULTIPLY_POINT:
					return executeKIRKCmd13(@out, outsize, @in, insize);
				case PSP_KIRK_CMD_PRNG:
					return executeKIRKCmd14(@out, insizeAligned);
				case PSP_KIRK_CMD_ECDSA_SIGN:
					return executeKIRKCmd16(@out, outsize, @in, insize);
				case PSP_KIRK_CMD_ECDSA_VERIFY:
					return executeKIRKCmd17(@in, insize);
				case PSP_KIRK_CMD_INIT:
					return 0;
				case PSP_KIRK_CMD_CERT_VERIFY:
					return 0;
				default:
					return PSP_KIRK_INVALID_OPERATION; // Dummy.
			}
		}
	}

}