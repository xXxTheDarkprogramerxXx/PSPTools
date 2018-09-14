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
	public class DRM
	{

		private static AMCTRL amctrl;
		private sbyte[] iv = new sbyte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

		public DRM()
		{
			amctrl = new AMCTRL();
		}

		/*
		 * sceNpDrm - npdrm.prx
		 */
		public virtual sbyte[] hleNpDrmGetFixedKey(sbyte[] hash, sbyte[] data, int mode)
		{
			// Setup the crypto and keygen modes and initialize both context structs.   
			AMCTRL.BBMac_Ctx bbctx = new AMCTRL.BBMac_Ctx();
			AES128 aes = new AES128("AES/CBC/NoPadding");

			// Get the encryption key.
			sbyte[] encKey = new sbyte[0x10];
			if ((mode & 0x1) == 0x1)
			{
				for (int i = 0; i < 0x10; i++)
				{
					encKey[i] = unchecked((sbyte)(KeyVault.drmEncKey1[i] & 0xFF));
				}
			}
			else if ((mode & 0x2) == 0x2)
			{
				for (int i = 0; i < 0x10; i++)
				{
					encKey[i] = unchecked((sbyte)(KeyVault.drmEncKey2[i] & 0xFF));
				}
			}
			else if ((mode & 0x3) == 0x3)
			{
				for (int i = 0; i < 0x10; i++)
				{
					encKey[i] = unchecked((sbyte)(KeyVault.drmEncKey3[i] & 0xFF));
				}
			}
			else
			{
				return null;
			}

			// Get the fixed key.
			sbyte[] fixedKey = new sbyte[0x10];
			for (int i = 0; i < 0x10; i++)
			{
				fixedKey[i] = unchecked((sbyte)(KeyVault.drmFixedKey[i] & 0xFF));
			}

			// Call the BBMac functions.
			amctrl.hleDrmBBMacInit(bbctx, 1);
			amctrl.hleDrmBBMacUpdate(bbctx, data, data.Length);
			amctrl.hleDrmBBMacFinal(bbctx, hash, fixedKey);

			// Encrypt and return the hash.
			return aes.encrypt(hash, encKey, iv);
		}

		public virtual sbyte[] GetKeyFromRif(sbyte[] rifBuf, sbyte[] actdatBuf, sbyte[] openPSID)
		{
			AES128 aes = new AES128("AES/ECB/NoPadding");

			sbyte[] rifIndex = new sbyte[0x10];
			sbyte[] rifDatKey = new sbyte[0x10];
			sbyte[] encRifIndex = new sbyte[0x10];
			sbyte[] encRifDatKey = new sbyte[0x10];

			sbyte[] rifKey = new sbyte[KeyVault.drmRifKey.Length];
			for (int i = 0; i < KeyVault.drmRifKey.Length; i++)
			{
				rifKey[i] = unchecked((sbyte)(KeyVault.drmRifKey[i] & 0xFF));
			}

			Array.Copy(rifBuf, 0x40, encRifIndex, 0x0, 0x10);
			Array.Copy(rifBuf, 0x50, encRifDatKey, 0x0, 0x10);

			rifIndex = aes.decrypt(encRifIndex, rifKey, iv);

			long index = rifIndex[0xF];
			if (index < 0x80)
			{
				sbyte[] actDat = DecryptActdat(actdatBuf, openPSID);
				sbyte[] datKey = new sbyte[0x10];
				Array.Copy(actDat, (int) index * 16, datKey, 0, 0x10);
				rifDatKey = aes.decrypt(encRifDatKey, datKey, iv);
			}

			return rifDatKey;
		}

		public virtual sbyte[] DecryptActdat(sbyte[] actdatBuf, sbyte[] openPSID)
		{
			AES128 aes = new AES128("AES/ECB/NoPadding");

			sbyte[] actdat = new sbyte[0x800];
			sbyte[] consoleKey = GetConsoleKey(openPSID);
			Array.Copy(actdatBuf, 0x10, actdat, 0x0, actdat.Length - 0x10);

			return aes.decrypt(actdat, consoleKey, iv);
		}

		public virtual sbyte[] GetConsoleKey(sbyte[] openPSID)
		{
			AES128 aes = new AES128("AES/ECB/NoPadding");

			sbyte[] actdatKey = new sbyte[KeyVault.drmActdatKey.Length];
			for (int i = 0; i < KeyVault.drmActdatKey.Length; i++)
			{
				actdatKey[i] = unchecked((sbyte)(KeyVault.drmActdatKey[i] & 0xFF));
			}

			return aes.encrypt(openPSID, actdatKey, iv);
		}
	}
}