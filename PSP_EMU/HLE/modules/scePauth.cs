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
	//using Logger = org.apache.log4j.Logger;


	using CryptoEngine = pspsharp.crypto.CryptoEngine;
	using KeyVault = pspsharp.crypto.KeyVault;
	using SeekableRandomFile = pspsharp.filesystems.SeekableRandomFile;
	using Settings = pspsharp.settings.Settings;

	public class scePauth : HLEModule
	{
		//public static Logger log = Modules.getLogger("scePauth");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF7AA47F6, version = 500) public int scePauth_F7AA47F6(pspsharp.HLE.TPointer inputAddr, int inputLength, @CanBeNull pspsharp.HLE.TPointer32 resultLengthAddr, pspsharp.HLE.TPointer keyAddr)
		[HLEFunction(nid : 0xF7AA47F6, version : 500)]
		public virtual int scePauth_F7AA47F6(TPointer inputAddr, int inputLength, TPointer32 resultLengthAddr, TPointer keyAddr)
		{
			CryptoEngine crypto = new CryptoEngine();
			sbyte[] @in = inputAddr.getArray8(inputLength);
			sbyte[] key = keyAddr.getArray8(0x10);
			sbyte[] xor = new sbyte[0x10];
			for (int i = 0; i < 0x10; i++)
			{
				xor[i] = unchecked((sbyte)(KeyVault.pauthXorKey[i] & 0xFF));
			}

			// Try to read/write PAUTH data for external decryption.
			try
			{
				// Calculate CRC32 for PAUTH data.
				CRC32 crc = new CRC32();
				crc.update(@in, 0, inputLength);
				int tag = (int) crc.Value;

				// Set PAUTH file name and PAUTH dir name.
				string pauthDirName = string.Format("{0}PAUTH{1}", Settings.Instance.DiscTmpDirectory, System.IO.Path.DirectorySeparatorChar);
				string pauthFileName = pauthDirName + string.Format("pauth-{0}.bin", tag.ToString("x"));
				string pauthDecFileName = pauthDirName + string.Format("pauth-{0}.bin.decrypt", tag.ToString("x"));
				string pauthKeyFileName = pauthDirName + string.Format("pauth-{0}.key", tag.ToString("x"));

				// Check for an already decrypted file.
				File dec = new File(pauthDecFileName);
				if (dec.exists())
				{
					log.info("Reading PAUTH data file from " + pauthDecFileName);

					// Read the externally decrypted file.
					SeekableRandomFile pauthPRXDec = new SeekableRandomFile(pauthDecFileName, "rw");
					int pauthSize = (int) pauthPRXDec.Length();
					sbyte[] pauthDec = new sbyte[pauthSize];
					pauthPRXDec.read(pauthDec);
					pauthPRXDec.Dispose();

					inputAddr.setArray(pauthDec, pauthSize);
					resultLengthAddr.setValue(pauthSize);
				}
				else
				{
					// Create PAUTH dir under tmp.
					File f = new File(pauthDirName);
					f.mkdirs();

					log.info("Writting PAUTH data file to " + pauthFileName);
					log.info("Writting PAUTH key file to " + pauthKeyFileName);

					// Write the PAUTH file and key for external decryption.
					SeekableRandomFile pauthPRX = new SeekableRandomFile(pauthFileName, "rw");
					SeekableRandomFile pauthKey = new SeekableRandomFile(pauthKeyFileName, "rw");
					pauthPRX.write(@in);
					pauthKey.write(key);
					pauthPRX.Dispose();
					pauthKey.Dispose();

					// Decryption is not working properly due to a missing KIRK key.
					int reslength = crypto.PRXEngine.DecryptPRX(@in, inputLength, 5, key, xor);

					// Fake the result.
					inputAddr.clear(reslength);
					resultLengthAddr.setValue(reslength);
				}
			}
			catch (IOException ioe)
			{
				Console.WriteLine(ioe);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x98B83B5D, version = 500) public int scePauth_98B83B5D(pspsharp.HLE.TPointer inputAddr, int inputLength, @CanBeNull pspsharp.HLE.TPointer32 resultLengthAddr, pspsharp.HLE.TPointer keyAddr)
		[HLEFunction(nid : 0x98B83B5D, version : 500)]
		public virtual int scePauth_98B83B5D(TPointer inputAddr, int inputLength, TPointer32 resultLengthAddr, TPointer keyAddr)
		{
			CryptoEngine crypto = new CryptoEngine();
			sbyte[] @in = inputAddr.getArray8(inputLength);
			sbyte[] key = keyAddr.getArray8(0x10);
			sbyte[] xor = new sbyte[0x10];
			for (int i = 0; i < 0x10; i++)
			{
				xor[i] = unchecked((sbyte)(KeyVault.pauthXorKey[i] & 0xFF));
			}

			// Try to read/write PAUTH data for external decryption.
			try
			{
				// Calculate CRC32 for PAUTH data.
				CRC32 crc = new CRC32();
				crc.update(@in, 0, inputLength);
				int tag = (int) crc.Value;

				// Set PAUTH file name and PAUTH dir name.
				string pauthDirName = string.Format("{0}PAUTH{1}", Settings.Instance.DiscTmpDirectory, System.IO.Path.DirectorySeparatorChar);
				string pauthFileName = pauthDirName + string.Format("pauth_{0}.bin", tag.ToString("x"));
				string pauthDecFileName = pauthDirName + string.Format("pauth_{0}.bin.decrypt", tag.ToString("x"));
				string pauthKeyFileName = pauthDirName + string.Format("pauth_{0}.key", tag.ToString("x"));

				// Check for an already decrypted file.
				File dec = new File(pauthDecFileName);
				if (dec.exists())
				{
					log.info("Reading PAUTH data file from " + pauthDecFileName);

					// Read the externally decrypted file.
					SeekableRandomFile pauthPRXDec = new SeekableRandomFile(pauthDecFileName, "rw");
					int pauthSize = (int) pauthPRXDec.Length();
					sbyte[] pauthDec = new sbyte[pauthSize];
					pauthPRXDec.read(pauthDec);
					pauthPRXDec.Dispose();

					inputAddr.setArray(pauthDec, pauthSize);
					resultLengthAddr.setValue(pauthSize);
				}
				else
				{
					// Create PAUTH dir under tmp.
					File f = new File(pauthDirName);
					f.mkdirs();

					log.info("Writting PAUTH data file to " + pauthFileName);
					log.info("Writting PAUTH key file to " + pauthKeyFileName);

					// Write the PAUTH file and key for external decryption.
					SeekableRandomFile pauthPRX = new SeekableRandomFile(pauthFileName, "rw");
					SeekableRandomFile pauthKey = new SeekableRandomFile(pauthKeyFileName, "rw");
					pauthPRX.write(@in);
					pauthKey.write(key);
					pauthPRX.Dispose();
					pauthKey.Dispose();

					// Decryption is not working properly due to a missing KIRK key.
					int reslength = crypto.PRXEngine.DecryptPRX(@in, inputLength, 5, key, xor);

					// Fake the result.
					inputAddr.clear(reslength);
					resultLengthAddr.setValue(reslength);
				}
			}
			catch (IOException ioe)
			{
				Console.WriteLine(ioe);
			}

			return 0;
		}
	}

}