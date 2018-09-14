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
namespace pspsharp.format
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readStringNZ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUByte;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUHalf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readWord;


	using CryptoEngine = pspsharp.crypto.CryptoEngine;
	using Settings = pspsharp.settings.Settings;

	/// 
	/// <summary>
	/// @author shadow
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public class PSP
	public class PSP
	{
		public const int PSP_HEADER_SIZE = 336;
		public const int PSP_MAGIC = 0x5053507E;
		public const int SCE_KERNEL_MAX_MODULE_SEGMENT = 4;
		public const int AES_KEY_SIZE = 16;
		public const int CMAC_KEY_SIZE = 16;
		public const int CMAC_HEADER_HASH_SIZE = 16;
		public const int CMAC_DATA_HASH_SIZE = 16;
		public const int CHECK_SIZE = 88;
		public const int SHA1_HASH_SIZE = 20;
		public const int KEY_DATA_SIZE = 16;
		private int magic;
		private int mod_attr;
		private int comp_mod_attr;
		private int mod_ver_lo;
		private int mod_ver_hi;
		private string modname;
		private int mod_version;
		private int nsegments;
		private int elf_size;
		private int psp_size;
		private int boot_entry;
		private int modinfo_offset;
		private int bss_size;
		private int[] seg_align = new int[SCE_KERNEL_MAX_MODULE_SEGMENT];
		private int[] seg_address = new int[SCE_KERNEL_MAX_MODULE_SEGMENT];
		private int[] seg_size = new int[SCE_KERNEL_MAX_MODULE_SEGMENT];
		private int[] reserved = new int[5];
		private int devkit_version;
		private int dec_mode;
		private int pad;
		private int overlap_size;
		private int[] aes_key = new int[AES_KEY_SIZE];
		private int[] cmac_key = new int[CMAC_KEY_SIZE];
		private int[] cmac_header_hash = new int[CMAC_HEADER_HASH_SIZE];
		private int comp_size;
		private int comp_offset;
		private int unk1;
		private int unk2;
		private int[] cmac_data_hash = new int[CMAC_DATA_HASH_SIZE];
		private int tag;
		private int[] sig_check = new int[CHECK_SIZE];
		private int[] sha1_hash = new int[SHA1_HASH_SIZE];
		private int[] key_data = new int[KEY_DATA_SIZE];

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PSP(ByteBuffer f) throws java.io.IOException
		public PSP(ByteBuffer f)
		{
			read(f);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void read(ByteBuffer f) throws java.io.IOException
		private void read(ByteBuffer f)
		{
			if (f.capacity() == 0)
			{
				return;
			}

			magic = readWord(f);
			mod_attr = readUHalf(f);
			comp_mod_attr = readUHalf(f);
			mod_ver_lo = readUByte(f);
			mod_ver_hi = readUByte(f);
			modname = readStringNZ(f, 28);
			mod_version = readUByte(f);
			nsegments = readUByte(f);
			elf_size = readWord(f);
			psp_size = readWord(f);
			boot_entry = readWord(f);
			modinfo_offset = readWord(f);
			bss_size = readWord(f);
			seg_align[0] = readUHalf(f);
			seg_align[1] = readUHalf(f);
			seg_align[2] = readUHalf(f);
			seg_align[3] = readUHalf(f);
			seg_address[0] = readWord(f);
			seg_address[1] = readWord(f);
			seg_address[2] = readWord(f);
			seg_address[3] = readWord(f);
			seg_size[0] = readWord(f);
			seg_size[1] = readWord(f);
			seg_size[2] = readWord(f);
			seg_size[3] = readWord(f);
			reserved[0] = readWord(f);
			reserved[1] = readWord(f);
			reserved[2] = readWord(f);
			reserved[3] = readWord(f);
			reserved[4] = readWord(f);
			devkit_version = readWord(f);
			dec_mode = readUByte(f);
			pad = readUByte(f);
			overlap_size = readUHalf(f);
			for (int i = 0; i < AES_KEY_SIZE; i++)
			{
				aes_key[i] = readUByte(f);
			}
			for (int i = 0; i < CMAC_KEY_SIZE; i++)
			{
				cmac_key[i] = readUByte(f);
			}
			for (int i = 0; i < CMAC_HEADER_HASH_SIZE; i++)
			{
				cmac_header_hash[i] = readUByte(f);
			}
			comp_size = readWord(f);
			comp_offset = readWord(f);
			unk1 = readWord(f);
			unk2 = readWord(f);
			for (int i = 0; i < CMAC_DATA_HASH_SIZE; i++)
			{
				cmac_data_hash[i] = readUByte(f);
			}
			tag = readWord(f);
			for (int i = 0; i < CHECK_SIZE; i++)
			{
				sig_check[i] = readUByte(f);
			}
			for (int i = 0; i < SHA1_HASH_SIZE; i++)
			{
				sha1_hash[i] = readUByte(f);
			}
			for (int i = 0; i < KEY_DATA_SIZE; i++)
			{
				key_data[i] = readUByte(f);
			}
		}

		public virtual ByteBuffer decrypt(ByteBuffer f)
		{
			if (f.capacity() == 0)
			{
				return null;
			}

			CryptoEngine crypto = new CryptoEngine();
			sbyte[] inBuf;
			if (f.hasArray() && f.position() <= PSP_HEADER_SIZE)
			{
				inBuf = f.array();
			}
			else
			{
				int currentPosition = f.position();
				f.position(currentPosition - PSP_HEADER_SIZE);
				inBuf = new sbyte[f.remaining()];
				f.get(inBuf);
				f.position(currentPosition);
			}

			int inSize = inBuf.Length;
			sbyte[] elfBuffer = crypto.PRXEngine.DecryptAndUncompressPRX(inBuf, inSize);

			if (elfBuffer == null)
			{
				return null;
			}

			if (CryptoEngine.ExtractEbootStatus)
			{
				try
				{
					string ebootPath = Settings.Instance.DiscTmpDirectory;
					System.IO.Directory.CreateDirectory(ebootPath);
					RandomAccessFile raf = new RandomAccessFile(ebootPath + "EBOOT.BIN", "rw");
					raf.write(elfBuffer);
					raf.close();
				}
				catch (IOException)
				{
					// Ignore.
				}
			}

			return ByteBuffer.wrap(elfBuffer);
		}

		public virtual bool Valid
		{
			get
			{
				return magic == PSP_MAGIC; // ~PSP
			}
		}

		public virtual string Modname
		{
			get
			{
				return modname;
			}
		}
	}
}