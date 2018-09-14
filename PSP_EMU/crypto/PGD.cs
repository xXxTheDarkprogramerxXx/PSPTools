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
	public class PGD
	{
		private static AMCTRL amctrl;
		private AMCTRL.BBCipher_Ctx pgdCipherContext;
		private AMCTRL.BBMac_Ctx pgdMacContext;

		public PGD()
		{
			amctrl = new AMCTRL();
		}

		// Plain PGD handling functions.
		public virtual sbyte[] DecryptPGD(sbyte[] inbuf, int size, sbyte[] key, int seed)
		{
			// Setup the crypto and keygen modes and initialize both context structs.
			int sdEncMode = 1;
			int sdGenMode = 2;
			pgdMacContext = new AMCTRL.BBMac_Ctx();
			pgdCipherContext = new AMCTRL.BBCipher_Ctx();

			// Align the buffers to 16-bytes.
			int alignedSize = ((size + 0xF) >> 4) << 4;
			sbyte[] outbuf = new sbyte[alignedSize - 0x10];
			sbyte[] dataBuf = new sbyte[alignedSize];

			// Fully copy the contents of the encrypted file.
			Array.Copy(inbuf, 0, dataBuf, 0, size);

			amctrl.hleDrmBBMacInit(pgdMacContext, sdEncMode);
			amctrl.hleDrmBBCipherInit(pgdCipherContext, sdEncMode, sdGenMode, dataBuf, key, seed);
			amctrl.hleDrmBBMacUpdate(pgdMacContext, dataBuf, 0x10);
			Array.Copy(dataBuf, 0x10, outbuf, 0, alignedSize - 0x10);
			amctrl.hleDrmBBMacUpdate(pgdMacContext, outbuf, alignedSize - 0x10);
			amctrl.hleDrmBBCipherUpdate(pgdCipherContext, outbuf, alignedSize - 0x10);

			return outbuf;
		}

		public virtual sbyte[] UpdatePGDCipher(sbyte[] inbuf, int size)
		{
			// Align the buffers to 16-bytes.
			int alignedSize = ((size + 0xF) >> 4) << 4;
			sbyte[] outbuf = new sbyte[alignedSize - 0x10];
			sbyte[] dataBuf = new sbyte[alignedSize];

			Array.Copy(inbuf, 0, dataBuf, 0, size);
			Array.Copy(dataBuf, 0x10, outbuf, 0, alignedSize - 0x10);
			amctrl.hleDrmBBCipherUpdate(pgdCipherContext, outbuf, alignedSize - 0x10);

			return outbuf;
		}

		public virtual void FinishPGDCipher()
		{
			amctrl.hleDrmBBCipherFinal(pgdCipherContext);
		}

		public virtual sbyte[] GetEDATPGDKey(sbyte[] inbuf, int size)
		{
			// Setup the crypto and keygen modes and initialize both context structs.
			int macEncMode;
			int pgdFlag = 2;
			pgdMacContext = new AMCTRL.BBMac_Ctx();

			// Align the buffer to 16-bytes.
			int alignedSize = ((size + 0xF) >> 4) << 4;
			sbyte[] dataBuf = new sbyte[alignedSize];

			// Fully copy the contents of the encrypted file.
			Array.Copy(inbuf, 0, dataBuf, 0, size);

			int keyIndex = dataBuf[0x4];
			int drmType = dataBuf[0x8];

			if ((drmType & 0x1) == 0x1)
			{
				macEncMode = 1;
				pgdFlag |= 4;
				if (keyIndex > 0x1)
				{
					macEncMode = 3;
					pgdFlag |= 8;
				}
			}
			else
			{
				macEncMode = 2;
			}

			// Get fixed DNAS keys.
			sbyte[] dnasKey = new sbyte[0x10];
			if ((pgdFlag & 0x2) == 0x2)
			{
				for (int i = 0; i < KeyVault.drmDNASKey1.Length; i++)
				{
					dnasKey[i] = unchecked((sbyte)(KeyVault.drmDNASKey1[i] & 0xFF));
				}
			}
			else if ((pgdFlag & 0x1) == 0x1)
			{
				for (int i = 0; i < KeyVault.drmDNASKey2.Length; i++)
				{
					dnasKey[i] = unchecked((sbyte)(KeyVault.drmDNASKey2[i] & 0xFF));
				}
			}
			else
			{
				return null;
			}

			// Get mac80 from input.
			sbyte[] macKey80 = new sbyte[0x10];
			Array.Copy(dataBuf, 0x80, macKey80, 0, 0x10);

			// Get mac70 from input.
			sbyte[] macKey70 = new sbyte[0x10];
			Array.Copy(dataBuf, 0x70, macKey70, 0, 0x10);

			// MAC_0x80
			amctrl.hleDrmBBMacInit(pgdMacContext, macEncMode);
			amctrl.hleDrmBBMacUpdate(pgdMacContext, dataBuf, 0x80);
			amctrl.hleDrmBBMacFinal2(pgdMacContext, macKey80, dnasKey);

			// MAC_0x70
			amctrl.hleDrmBBMacInit(pgdMacContext, macEncMode);
			amctrl.hleDrmBBMacUpdate(pgdMacContext, dataBuf, 0x70);

			// Get the decryption key from BBMAC.
			return amctrl.GetKeyFromBBMac(pgdMacContext, macKey70);
		}

		public virtual bool CheckEDATRenameKey(sbyte[] fileName, sbyte[] hash, sbyte[] data)
		{
			// Set up MAC context.
			pgdMacContext = new AMCTRL.BBMac_Ctx();

			// Perform hash check.
			amctrl.hleDrmBBMacInit(pgdMacContext, 3);
			amctrl.hleDrmBBMacUpdate(pgdMacContext, data, 0x30);
			amctrl.hleDrmBBMacUpdate(pgdMacContext, fileName, fileName.Length);

			// Get the fixed rename key.
			sbyte[] renameKey = new sbyte[0x10];
			for (int i = 0; i < 0x10; i++)
			{
				renameKey[i] = unchecked((sbyte)(KeyVault.drmRenameKey[i] & 0xFF));
			}

			// Compare and return.
			return (amctrl.hleDrmBBMacFinal2(pgdMacContext, hash, renameKey) == 0);
		}
	}
}