using System;
using System.Threading;

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


	using Modules = pspsharp.HLE.Modules;

	//using Logger = org.apache.log4j.Logger;
	using BouncyCastleProvider = org.bouncycastle.jce.provider.BouncyCastleProvider;

	public class AES128
	{
		private static Logger log = Modules.log;
		private static readonly sbyte[] const_Zero = new sbyte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
		private static readonly sbyte[] const_Rb = new sbyte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, unchecked((sbyte) 0x87)};
		private sbyte[] contentKey;
		private System.IO.MemoryStream barros;
		private static Cipher cipher;
		private static readonly sbyte[] iv0 = new sbyte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
		// Do not use Bouncy Castle as the default implementation is much faster
		public const bool useBouncyCastle = false;

		public static void init()
		{
			// Run in a background thread as the initialization is taking around 300 milliseconds
			Thread staticInit = new Thread(() =>
			{
			init("AES/CBC/NoPadding");
			});
			staticInit.Start();
		}

		private static void init(string mode)
		{
			if (cipher == null)
			{
				if (useBouncyCastle)
				{
					Security.addProvider(new BouncyCastleProvider());
				}
				try
				{
					if (useBouncyCastle)
					{
						cipher = Cipher.getInstance(mode, "BC");
					}
					else
					{
						cipher = Cipher.getInstance(mode);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("AES128 Cipher", e);
				}
			}
		}

		public AES128(string mode)
		{
			init(mode);
		}

		private Key getKeySpec(sbyte[] encKey)
		{
			return new SecretKeySpec(encKey, "AES");
		}

		// Private encrypting method for CMAC (IV == 0).
		private sbyte[] encryptCMAC(sbyte[] @in, sbyte[] encKey)
		{
			return encryptCMAC(@in, getKeySpec(encKey));
		}

		// Private encrypting method for CMAC (IV == 0).
		private sbyte[] encryptCMAC(sbyte[] @in, Key keySpec)
		{
			return encrypt(@in, keySpec, iv0);
		}

		// Public encrypting/decrypting methods (for CryptoEngine calls).
		public virtual sbyte[] encrypt(sbyte[] @in, sbyte[] encKey, sbyte[] iv)
		{
			return encrypt(@in, getKeySpec(encKey), iv);
		}

		// Public encrypting/decrypting methods (for CryptoEngine calls).
		public virtual sbyte[] encrypt(sbyte[] @in, Key keySpec, sbyte[] iv)
		{
			IvParameterSpec ivec = new IvParameterSpec(iv);
			sbyte[] result = null;
			try
			{
				Cipher c = cipher;
				c.init(Cipher.ENCRYPT_MODE, keySpec, ivec);
				result = c.doFinal(@in);
			}
			catch (InvalidKeyException e)
			{
				Console.WriteLine("encrypt", e);
			}
			catch (InvalidAlgorithmParameterException e)
			{
				Console.WriteLine("encrypt", e);
			}
			catch (IllegalBlockSizeException e)
			{
				Console.WriteLine("encrypt", e);
			}
			catch (BadPaddingException e)
			{
				Console.WriteLine("encrypt", e);
			}

			return result;
		}

		public virtual sbyte[] decrypt(sbyte[] @in, sbyte[] decKey, sbyte[] iv)
		{
			Key keySpec = new SecretKeySpec(decKey, "AES");
			IvParameterSpec ivec = new IvParameterSpec(iv);
			sbyte[] result = null;
			try
			{
				Cipher c = cipher;
				c.init(Cipher.DECRYPT_MODE, keySpec, ivec);
				result = c.doFinal(@in);
			}
			catch (Exception e)
			{
				Console.WriteLine("decrypt", e);
			}

			return result;
		}

		public virtual void doInitCMAC(sbyte[] contentKey)
		{
			this.contentKey = contentKey;
			barros = new System.IO.MemoryStream();
		}

		public virtual void doUpdateCMAC(sbyte[] input, int offset, int len)
		{
			barros.Write(input, offset, len);
		}

		public virtual void doUpdateCMAC(sbyte[] input)
		{
			barros.Write(input, 0, input.Length);
		}

		public virtual sbyte[] doFinalCMAC()
		{
			object[] keys = generateSubKey(contentKey);
			sbyte[] K1 = (sbyte[]) keys[0];
			sbyte[] K2 = (sbyte[]) keys[1];

			sbyte[] input = barros.toByteArray();

			int numberOfRounds = (input.Length + 15) / 16;
			bool lastBlockComplete;

			if (numberOfRounds == 0)
			{
				numberOfRounds = 1;
				lastBlockComplete = false;
			}
			else
			{
				if (input.Length % 16 == 0)
				{
					lastBlockComplete = true;
				}
				else
				{
					lastBlockComplete = false;
				}
			}

			sbyte[] M_last;
			int srcPos = 16 * (numberOfRounds - 1);

			if (lastBlockComplete)
			{
				sbyte[] partInput = new sbyte[16];

				Array.Copy(input, srcPos, partInput, 0, 16);
				M_last = xor128(partInput, K1);
			}
			else
			{
				sbyte[] partInput = new sbyte[input.Length % 16];

				Array.Copy(input, srcPos, partInput, 0, input.Length % 16);
				sbyte[] padded = doPaddingCMAC(partInput);
				M_last = xor128(padded, K2);
			}

			sbyte[] X = const_Zero.Clone();
			sbyte[] partInput = new sbyte[16];
			sbyte[] Y;

			Key keySpec = getKeySpec(contentKey);
			for (int i = 0; i < numberOfRounds - 1; i++)
			{
				srcPos = 16 * i;
				Array.Copy(input, srcPos, partInput, 0, 16);

				Y = xor128(partInput, X); // Y := Mi (+) X
				X = encryptCMAC(Y, keySpec);
			}

			Y = xor128(X, M_last);
			X = encryptCMAC(Y, contentKey);

			return X;
		}

		public virtual bool doVerifyCMAC(sbyte[] verificationCMAC)
		{
			sbyte[] cmac = doFinalCMAC();

			if (verificationCMAC == null || verificationCMAC.Length != cmac.Length)
			{
				return false;
			}

			for (int i = 0; i < cmac.Length; i++)
			{
				if (cmac[i] != verificationCMAC[i])
				{
					return false;
				}
			}

			return true;
		}

		private sbyte[] doPaddingCMAC(sbyte[] input)
		{
			sbyte[] padded = new sbyte[16];

			for (int j = 0; j < 16; j++)
			{
				if (j < input.Length)
				{
					padded[j] = input[j];
				}
				else if (j == input.Length)
				{
					padded[j] = unchecked((sbyte) 0x80);
				}
				else
				{
					padded[j] = (sbyte) 0x00;
				}
			}

			return padded;
		}

		private object[] generateSubKey(sbyte[] key)
		{
			sbyte[] L = encryptCMAC(const_Zero, key);

			sbyte[] K1 = null;
			if ((L[0] & 0x80) == 0)
			{ // If MSB(L) = 0, then K1 = L << 1
				K1 = doLeftShiftOneBit(L);
			}
			else
			{ // Else K1 = ( L << 1 ) (+) Rb
				sbyte[] tmp = doLeftShiftOneBit(L);
				K1 = xor128(tmp, const_Rb);
			}

			sbyte[] K2 = null;
			if ((K1[0] & 0x80) == 0)
			{
				K2 = doLeftShiftOneBit(K1);
			}
			else
			{
				sbyte[] tmp = doLeftShiftOneBit(K1);
				K2 = xor128(tmp, const_Rb);
			}

			object[] result = new object[2];
			result[0] = K1;
			result[1] = K2;
			return result;
		}

		private static sbyte[] xor128(sbyte[] input1, sbyte[] input2)
		{
			sbyte[] output = new sbyte[input1.Length];
			for (int i = 0; i < input1.Length; i++)
			{
				output[i] = unchecked((sbyte)(((int) input1[i] ^ (int) input2[i]) & 0xFF));
			}
			return output;
		}

		private static sbyte[] doLeftShiftOneBit(sbyte[] input)
		{
			sbyte[] output = new sbyte[input.Length];
			sbyte overflow = 0;

			for (int i = (input.Length - 1); i >= 0; i--)
			{
				output[i] = unchecked((sbyte)((int) input[i] << 1 & 0xFF));
				output[i] |= overflow;
				overflow = ((input[i] & 0x80) != 0) ? (sbyte) 1 : (sbyte) 0;
			}

			return output;
		}
	}
}