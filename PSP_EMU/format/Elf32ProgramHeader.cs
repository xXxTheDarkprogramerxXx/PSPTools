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
//	import static pspsharp.util.Utilities.readUWord;

	using Utilities = pspsharp.util.Utilities;

	public class Elf32ProgramHeader
	{
		public static readonly int PF_X = (1 << 0); // Segment is executable
		public static readonly int PF_W = (1 << 1); // Segment is writable
		public static readonly int PF_R = (1 << 2); // Segment is readable
		private int p_type;
		private int p_offset;
		private int p_vaddr;
		private int p_paddr;
		private int p_filesz;
		private int p_memsz;
		private int p_flags; // Bits: 0x1=executable, 0x2=writable, 0x4=readable, demo PRX's were found to be not writable
		private int p_align;

		public static int @sizeof()
		{
			return 32;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Elf32ProgramHeader(ByteBuffer f) throws java.io.IOException
		public Elf32ProgramHeader(ByteBuffer f)
		{
			read(f);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void read(ByteBuffer f) throws java.io.IOException
		private void read(ByteBuffer f)
		{
			p_type = readUWord(f);
			p_offset = readUWord(f);
			p_vaddr = readUWord(f);
			p_paddr = readUWord(f);
			p_filesz = readUWord(f);
			p_memsz = readUWord(f);
			p_flags = readUWord(f);
			p_align = readUWord(f);
		}

		public override string ToString()
		{
			StringBuilder str = new StringBuilder();
			str.Append("p_type " + "\t\t " + Utilities.formatString("long", (P_type & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_offset " + "\t " + Utilities.formatString("long", (P_offset & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_vaddr " + "\t " + Utilities.formatString("long", (P_vaddr & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_paddr " + "\t " + Utilities.formatString("long", (P_paddr & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_filesz " + "\t " + Utilities.formatString("long", (P_filesz & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_memsz " + "\t " + Utilities.formatString("long", (P_memsz & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_flags " + "\t " + Utilities.formatString("long", (P_flags & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("p_align " + "\t " + Utilities.formatString("long", (P_align & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			return str.ToString();
		}

		public virtual int P_type
		{
			get
			{
				return p_type;
			}
		}

		public virtual int P_offset
		{
			get
			{
				return p_offset;
			}
		}

		public virtual int P_vaddr
		{
			get
			{
				return p_vaddr;
			}
		}

		public virtual int P_paddr
		{
			get
			{
				return p_paddr;
			}
		}

		public virtual int P_filesz
		{
			get
			{
				return p_filesz;
			}
		}

		public virtual int P_memsz
		{
			get
			{
				return p_memsz;
			}
		}

		public virtual int P_flags
		{
			get
			{
				return p_flags;
			}
		}

		public virtual int P_align
		{
			get
			{
				return p_align;
			}
		}
	}

}