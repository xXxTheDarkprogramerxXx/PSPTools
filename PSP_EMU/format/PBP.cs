using System.Text;

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
	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using ByteUtil = pspsharp.util.ByteUtil;
	using FileUtil = pspsharp.util.FileUtil;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.formatString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.read32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUWord;

	public class PBP
	{
		public const int PBP_MAGIC = 0x50425000;
		private const string PBP_UNPACK_PATH_PREFIX = "unpacked-pbp/";

		private static readonly string[] FILE_NAMES = new string[]{"param.sfo", "icon0.png", "icon1.pmf", "pic0.png", "pic1.png", "snd0.at3", "psp.data", "psar.data"};

		private const int TOTAL_FILES = 8;

		private const int PARAM_SFO = 0;
		private const int ICON0_PNG = 1;
		private const int ICON1_PMF = 2;
		private const int PIC0_PNG = 3;
		private const int PIC1_PNG = 4;
		private const int SND0_AT3 = 5;
		private const int PSP_DATA = 6;
		private const int PSAR_DATA = 7;

		private string info;
		private int size_pbp;

		private int p_magic;
		private int p_version;
		private int[] p_offsets;
		private Elf32 elf32;
		private PSF psf;

		public virtual bool Valid
		{
			get
			{
				return size_pbp != 0 && p_magic == PBP_MAGIC;
			}
		}

		public virtual Elf32 Elf32
		{
			set
			{
				elf32 = value;
			}
			get
			{
				return elf32;
			}
		}


		public virtual PSF PSF
		{
			get
			{
				return psf;
			}
		}

		public virtual string Info
		{
			set
			{
				info = value;
			}
			get
			{
				return info;
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PBP(ByteBuffer f) throws java.io.IOException
		public PBP(ByteBuffer f)
		{
			size_pbp = f.limit();
			if (size_pbp == 0)
			{
				return;
			}
			p_magic = readUWord(f);
			if (Valid)
			{
				p_version = readUWord(f);

				p_offsets = new int[] {readUWord(f), readUWord(f), readUWord(f), readUWord(f), readUWord(f), readUWord(f), readUWord(f), readUWord(f), size_pbp};

				info = ToString();
			}
		}

		private PBP()
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PSF readPSF(ByteBuffer f) throws java.io.IOException
		public virtual PSF readPSF(ByteBuffer f)
		{
			if (OffsetParam > 0)
			{
				f.position(OffsetParam);
				psf = new PSF(OffsetParam);
				psf.read(f);
				return psf;
			}
			return null;
		}

		public override string ToString()
		{
			StringBuilder str = new StringBuilder();
			str.Append("-----PBP HEADER---------" + "\n");
			str.Append("p_magic " + "\t\t" + formatString("long", (p_magic & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_version " + "\t\t" + formatString("long", (p_version & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_offset_param_sfo " + "\t" + formatString("long", (OffsetParam & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_offset_icon0_png " + "\t" + formatString("long", (OffsetIcon0 & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_offset_icon1_pmf " + "\t" + formatString("long", (OffsetIcon1 & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_offset_pic0_png " + "\t" + formatString("long", (OffsetPic0 & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_offset_pic1_png " + "\t" + formatString("long", (OffsetPic1 & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_offset_snd0_at3 " + "\t" + formatString("long", (OffsetSnd0 & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_offset_psp_data " + "\t" + formatString("long", (OffsetPspData & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_offset_psar_data " + "\t" + formatString("long", (OffsetPsarData & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			return str.ToString();
		}

		private string getName(int index)
		{
			return FILE_NAMES[index];
		}

		private int getOffset(int index)
		{
			return this.p_offsets[index];
		}

		private int getSize(int index)
		{
			return this.p_offsets[index + 1] - this.p_offsets[index];
		}

		private sbyte[] getBytes(ByteBuffer f, int index)
		{
			return ByteUtil.readBytes(f, getOffset(index), getSize(index));
		}

		public virtual int Magic
		{
			get
			{
				return p_magic;
			}
		}

		public virtual int Version
		{
			get
			{
				return p_version;
			}
		}

		public virtual int OffsetParam
		{
			get
			{
				return getOffset(PARAM_SFO);
			}
		}

		public virtual int OffsetIcon0
		{
			get
			{
				return getOffset(ICON0_PNG);
			}
		}

		public virtual int OffsetIcon1
		{
			get
			{
				return getOffset(ICON1_PMF);
			}
		}

		public virtual int OffsetPic0
		{
			get
			{
				return getOffset(PIC0_PNG);
			}
		}

		public virtual int OffsetPic1
		{
			get
			{
				return getOffset(PIC1_PNG);
			}
		}

		public virtual int OffsetSnd0
		{
			get
			{
				return getOffset(SND0_AT3);
			}
		}

		public virtual int OffsetPspData
		{
			get
			{
				return getOffset(PSP_DATA);
			}
		}

		public virtual int OffsetPsarData
		{
			get
			{
				return getOffset(PSAR_DATA);
			}
		}

		public virtual int SizeIcon0
		{
			get
			{
				return getSize(ICON0_PNG);
			}
		}

		public static bool deleteDir(File dir)
		{
			if (dir.Directory)
			{
				string[] children = dir.list();
				for (int i = 0; i < children.Length; i++)
				{
					bool success = deleteDir(new File(dir, children[i]));
					if (!success)
					{
						return false;
					}
				}
			}
			// The directory is now empty so delete it
			return dir.delete();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void unpackPBP(ByteBuffer f) throws java.io.IOException
		public static void unpackPBP(ByteBuffer f)
		{
			f.position(0); //seek to 0
			PBP pbp = new PBP(f);
			if (!pbp.Valid)
			{
				return;
			}
			File dir = new File(PBP_UNPACK_PATH_PREFIX);
			deleteDir(dir); //delete all files and directory
			dir.mkdir();

			for (int index = 0; index < TOTAL_FILES; index++)
			{
				sbyte[] bytes = pbp.getBytes(f, index);
				if (bytes != null && bytes.Length > 0)
				{
					FileUtil.writeBytes(new File(PBP_UNPACK_PATH_PREFIX + pbp.getName(index)), bytes);
				}
			}
		}

		/// <summary>
		/// Unpack a PBP file, avoiding to consume too much memory
		/// (i.e. not reading each section completely in memory).
		/// </summary>
		/// <param name="vFile">        the PBP file </param>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void unpackPBP(pspsharp.HLE.VFS.IVirtualFile vFile) throws java.io.IOException
		public static void unpackPBP(IVirtualFile vFile)
		{
			vFile.ioLseek(0L);
			PBP pbp = new PBP();
			pbp.size_pbp = (int) vFile.length();
			pbp.p_magic = read32(vFile);
			if (!pbp.Valid)
			{
				return;
			}
			pbp.p_version = read32(vFile);
			pbp.p_offsets = new int[] {read32(vFile), read32(vFile), read32(vFile), read32(vFile), read32(vFile), read32(vFile), read32(vFile), read32(vFile), pbp.size_pbp};

			File dir = new File(PBP_UNPACK_PATH_PREFIX);
			deleteDir(dir); //delete all files and directory
			dir.mkdir();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] buffer = new byte[10 * 1024];
			sbyte[] buffer = new sbyte[10 * 1024];
			for (int index = 0; index < TOTAL_FILES; index++)
			{
				int size = pbp.getSize(index);
				if (size > 0)
				{
					long offset = pbp.getOffset(index) & 0xFFFFFFFFL;
					if (vFile.ioLseek(offset) == offset)
					{
						System.IO.Stream os = new System.IO.FileStream(PBP_UNPACK_PATH_PREFIX + pbp.getName(index), System.IO.FileMode.Create, System.IO.FileAccess.Write);
						while (size > 0)
						{
							int length = System.Math.Min(size, buffer.Length);
							int readLength = vFile.ioRead(buffer, 0, length);
							if (readLength > 0)
							{
								os.Write(buffer, 0, readLength);
								size -= readLength;
							}
							if (readLength != length)
							{
								break;
							}
						}
						os.Close();
					}
				}
			}
		}
	}

}