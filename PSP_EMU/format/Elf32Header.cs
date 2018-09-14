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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUByte;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUHalf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUWord;

	using Utilities = pspsharp.util.Utilities;

	public class Elf32Header
	{
		public const int ELF_MAGIC = 0x464C457F;
		public const int E_MACHINE_SPARC = 0x0002;
		public const int E_MACHINE_x86 = 0x0003;
		public const int E_MACHINE_MIPS = 0x0008;
		public const int E_MACHINE_PowerPC = 0x0014;
		public const int E_MACHINE_ARM = 0x0028;
		public const int E_MACHINE_SuperH = 0x002A;
		public const int E_MACHINE_IA_64 = 0x0032;
		public const int E_MACHINE_x86_64 = 0x003E;
		public const int E_MACHINE_AArch64 = 0x00B7;
		public const int ET_SCE_PRX = 0xFFA0;
		private int e_magic;
		private int e_class;
		private int e_data;
		private int e_idver;
		private sbyte[] e_pad = new sbyte[9];
		private int e_type;
		private int e_machine;
		private int e_version;
		private int e_entry;
		private int e_phoff;
		private int e_shoff;
		private int e_flags;
		private int e_ehsize;
		private int e_phentsize;
		private int e_phnum;
		private int e_shentsize;
		private int e_shnum;
		private int e_shstrndx;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void read(ByteBuffer f) throws java.io.IOException
		private void read(ByteBuffer f)
		{
			if (f.capacity() == 0)
			{
				return;
			}
			e_magic = readUWord(f);
			e_class = readUByte(f);
			e_data = readUByte(f);
			e_idver = readUByte(f);
			f.get(E_pad); // can raise EOF exception
			e_type = readUHalf(f);
			e_machine = readUHalf(f);
			e_version = readUWord(f);
			e_entry = readUWord(f);
			e_phoff = readUWord(f);
			e_shoff = readUWord(f);
			e_flags = readUWord(f);
			e_ehsize = readUHalf(f);
			e_phentsize = readUHalf(f);
			e_phnum = readUHalf(f);
			e_shentsize = readUHalf(f);
			e_shnum = readUHalf(f);
			e_shstrndx = readUHalf(f);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Elf32Header(ByteBuffer f) throws java.io.IOException
		public Elf32Header(ByteBuffer f)
		{
			read(f);
		}

		public static int @sizeof()
		{
			return 52;
		}

		public virtual bool Valid
		{
			get
			{
				return E_magic == ELF_MAGIC;
			}
		}

		public virtual bool MIPSExecutable
		{
			get
			{
				return E_machine == E_MACHINE_MIPS;
			}
		}

		public virtual bool PRXDetected
		{
			get
			{
				return E_type == ET_SCE_PRX;
			}
		}

		public virtual bool requiresRelocation()
		{
			return PRXDetected || E_entry < MemoryMap.START_RAM;
		}

		public override string ToString()
		{
			StringBuilder str = new StringBuilder();
			str.Append("-----ELF HEADER---------" + "\n");
			str.Append("e_magic " + "\t " + Utilities.formatString("long", (E_magic & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("e_class " + "\t " + Utilities.integerToHex(E_class & 0xFF) + "\n");
			// str.append("e_class " + "\t " +  Utilities.formatString("byte", Integer.toHexString(e_class & 0xFF ).toUpperCase())+ "\n");
			str.Append("e_data " + "\t\t " + Utilities.formatString("byte", (E_data & 0xFF).ToString("x").ToUpper()) + "\n");
			str.Append("e_idver " + "\t " + Utilities.formatString("byte", (E_idver & 0xFF).ToString("x").ToUpper()) + "\n");
			str.Append("e_type " + "\t\t " + Utilities.formatString("short", (E_type & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("e_machine " + "\t " + Utilities.formatString("short", (E_machine & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("e_version " + "\t " + Utilities.formatString("long", (E_version & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("e_entry " + "\t " + Utilities.formatString("long", (E_entry & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("e_phoff " + "\t " + Utilities.formatString("long", (E_phoff & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("e_shoff " + "\t " + Utilities.formatString("long", (E_shoff & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("e_flags " + "\t " + Utilities.formatString("long", (E_flags & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("e_ehsize " + "\t " + Utilities.formatString("short", (E_ehsize & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("e_phentsize " + "\t " + Utilities.formatString("short", (E_phentsize & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("e_phnum " + "\t " + Utilities.formatString("short", (E_phnum & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("e_shentsize " + "\t " + Utilities.formatString("short", (E_shentsize & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("e_shnum " + "\t " + Utilities.formatString("short", (E_shnum & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("e_shstrndx " + "\t " + Utilities.formatString("short", (E_shstrndx & 0xFFFF).ToString("x").ToUpper()) + "\n");
			return str.ToString();
		}

		public virtual int E_magic
		{
			get
			{
				return e_magic;
			}
		}

		public virtual int E_class
		{
			get
			{
				return e_class;
			}
		}

		public virtual int E_data
		{
			get
			{
				return e_data;
			}
		}

		public virtual int E_idver
		{
			get
			{
				return e_idver;
			}
		}

		public virtual sbyte[] E_pad
		{
			get
			{
				return e_pad;
			}
		}

		public virtual int E_type
		{
			get
			{
				return e_type;
			}
		}

		public virtual int E_machine
		{
			get
			{
				return e_machine;
			}
		}

		public virtual int E_version
		{
			get
			{
				return e_version;
			}
		}

		public virtual int E_entry
		{
			get
			{
				return e_entry;
			}
		}

		public virtual int E_phoff
		{
			get
			{
				return e_phoff;
			}
		}

		public virtual int E_shoff
		{
			get
			{
				return e_shoff;
			}
		}

		public virtual int E_flags
		{
			get
			{
				return e_flags;
			}
		}

		public virtual int E_ehsize
		{
			get
			{
				return e_ehsize;
			}
		}

		public virtual int E_phentsize
		{
			get
			{
				return e_phentsize;
			}
		}

		public virtual int E_phnum
		{
			get
			{
				return e_phnum;
			}
		}

		public virtual int E_shentsize
		{
			get
			{
				return e_shentsize;
			}
		}

		public virtual int E_shnum
		{
			get
			{
				return e_shnum;
			}
		}

		public virtual int E_shstrndx
		{
			get
			{
				return e_shstrndx;
			}
		}
	}

}