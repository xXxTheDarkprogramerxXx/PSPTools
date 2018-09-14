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
//	import static pspsharp.util.Utilities.formatString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.integerToHex;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUWord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readWord;


	public class Elf32SectionHeader
	{
		// Flags
		public const int SHF_NONE = 0x00000000;
		public const int SHF_WRITE = 0x00000001;
		public const int SHF_ALLOCATE = 0x00000002;
		public const int SHF_EXECUTE = 0x00000004;

		// Types
		public const int SHT_NULL = 0x00000000;
		public const int SHT_PROGBITS = 0x00000001;
		public const int SHT_SYMTAB = 0x00000002;
		public const int SHT_STRTAB = 0x00000003;
		public const int SHT_RELA = 0x00000004;
		public const int SHT_HASH = 0x00000005;
		public const int SHT_DYNAMIC = 0x00000006;
		public const int SHT_NOTE = 0x00000007;
		public const int SHT_NOBITS = 0x00000008;
		public const int SHT_REL = 0x00000009;
		public const int SHT_SHLIB = 0x0000000A;
		public const int SHT_DYNSYM = 0x0000000B;
		public const int SHT_PRXREL = 0x700000A0;

		private string sh_namez = "";
		private int sh_name;
		private int sh_type;
		private int sh_flags;
		private int sh_addr;
		private int sh_offset;
		private int sh_size;
		private int sh_link;
		private int sh_info;
		private int sh_addralign;
		private int sh_entsize;

		public static int @sizeof()
		{
			return 40;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Elf32SectionHeader(ByteBuffer f) throws java.io.IOException
		public Elf32SectionHeader(ByteBuffer f)
		{
			sh_name = readUWord(f);
			sh_type = readWord(f);
			sh_flags = readWord(f);
			sh_addr = readUWord(f);
			sh_offset = readUWord(f);
			sh_size = readUWord(f);
			sh_link = readWord(f);
			sh_info = readWord(f);
			sh_addralign = readWord(f);
			sh_entsize = readWord(f);
		}

		public Elf32SectionHeader(Memory mem, int address)
		{
			sh_name = mem.read32(address);
			sh_type = mem.read32(address + 4);
			sh_flags = mem.read32(address + 8);
			sh_addr = mem.read32(address + 12);
			sh_offset = mem.read32(address + 16);
			sh_size = mem.read32(address + 20);
			sh_link = mem.read32(address + 24);
			sh_info = mem.read32(address + 28);
			sh_addralign = mem.read32(address + 32);
			sh_entsize = mem.read32(address + 36);
		}

		public override string ToString()
		{
			StringBuilder str = new StringBuilder();
			str.Append("sh_name " + "\t " + formatString("long", (Sh_name & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			if (!string.ReferenceEquals(sh_namez, null) && sh_namez.Length > 0)
			{
				str.Append("sh_namez \t '" + sh_namez + "'\n");
			}
			str.Append("sh_type " + "\t " + formatString("long", (Sh_type & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("sh_flags " + "\t " + integerToHex(Sh_flags & 0xFF) + "\n");
			str.Append("sh_addr " + "\t " + formatString("long", (Sh_addr & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("sh_offset " + "\t " + formatString("long", (Sh_offset & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("sh_size " + "\t " + formatString("long", (Sh_size & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("sh_link " + "\t " + integerToHex(Sh_link & 0xFF) + "\n");
			str.Append("sh_info " + "\t " + integerToHex(Sh_info & 0xFF) + "\n");
			str.Append("sh_addralign " + "\t " + integerToHex(Sh_addralign & 0xFF) + "\n");
			str.Append("sh_entsize " + "\t " + formatString("long", (Sh_entsize & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			return str.ToString();
		}

		public virtual string Sh_namez
		{
			get
			{
				return sh_namez;
			}
			set
			{
				this.sh_namez = value;
			}
		}


		public virtual int Sh_name
		{
			get
			{
				return sh_name;
			}
		}

		public virtual int Sh_type
		{
			get
			{
				return sh_type;
			}
		}

		public virtual int Sh_flags
		{
			get
			{
				return sh_flags;
			}
		}

		public virtual int Sh_addr
		{
			get
			{
				return sh_addr;
			}
		}

		public virtual int getSh_addr(int baseAddress)
		{
			if (Memory.isAddressGood(Sh_addr) && Sh_addr >= baseAddress)
			{
				return Sh_addr;
			}
			return baseAddress + Sh_addr;
		}

		public virtual int Sh_offset
		{
			get
			{
				return sh_offset;
			}
		}

		public virtual int Sh_size
		{
			get
			{
				return sh_size;
			}
		}

		public virtual int Sh_link
		{
			get
			{
				return sh_link;
			}
		}

		public virtual int Sh_info
		{
			get
			{
				return sh_info;
			}
		}

		public virtual int Sh_addralign
		{
			get
			{
				return sh_addralign;
			}
		}

		public virtual int Sh_entsize
		{
			get
			{
				return sh_entsize;
			}
		}
	}

}