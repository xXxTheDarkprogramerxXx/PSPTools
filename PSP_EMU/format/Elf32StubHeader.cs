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
//	import static pspsharp.util.Utilities.readUByte;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUHalf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUWord;


	public class Elf32StubHeader
	{
		// Resolved version of s_modulename in a Java String
		private string s_modulenamez;

		private int s_modulename;
		private int s_version;
		private int s_flags;
		private int s_size;
		private int s_vstub_size;
		private int s_imports;
		private int s_nid;
		private int s_text;
		private int s_vstub;

		public static int @sizeof()
		{
			return 20;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Elf32StubHeader(ByteBuffer f) throws java.io.IOException
		public Elf32StubHeader(ByteBuffer f)
		{
			s_modulenamez = "";

			s_modulename = readUWord(f);
			s_version = readUHalf(f);
			s_flags = readUHalf(f);
			s_size = readUByte(f);
			s_vstub_size = readUByte(f);
			s_imports = readUHalf(f);
			s_nid = readUWord(f);
			s_text = readUWord(f);
			if (hasVStub())
			{
				s_vstub = readUWord(f);
			}
		}

		public Elf32StubHeader(Memory mem, int address)
		{
			s_modulenamez = "";

			s_modulename = mem.read32(address);
			s_version = mem.read16(address + 4);
			s_flags = mem.read16(address + 6);
			s_size = mem.read8(address + 8);
			s_vstub_size = mem.read8(address + 9);
			s_imports = mem.read16(address + 10);
			s_nid = mem.read32(address + 12);
			s_text = mem.read32(address + 16);
			if (hasVStub())
			{
				s_vstub = mem.read32(address + 20);
			}
		}

		public override string ToString()
		{
			StringBuilder str = new StringBuilder();
			if (!string.ReferenceEquals(s_modulenamez, null) && s_modulenamez.Length > 0)
			{
				str.Append(s_modulenamez + "\n");
			}
			str.Append("s_modulename" + "\t" + formatString("long", (s_modulename & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("s_version" + "\t\t" + formatString("short", (s_version & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("s_flags" + "\t\t\t" + formatString("short", (s_flags & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("s_size" + "\t\t\t" + formatString("short", (s_size & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("s_imports" + "\t\t" + formatString("short", (s_imports & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("s_nid" + "\t\t\t" + formatString("long", (s_nid & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("s_text" + "\t\t\t" + formatString("long", (s_text & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			if (hasVStub())
			{
				str.Append("s_vstub" + "\t\t\t" + formatString("long", (s_vstub & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			}
			return str.ToString();
		}

		public virtual string ModuleNamez
		{
			get
			{
				return s_modulenamez;
			}
			set
			{
				s_modulenamez = value;
			}
		}


		public virtual int OffsetModuleName
		{
			get
			{
				return s_modulename;
			}
		}

		public virtual int Version
		{
			get
			{
				return s_version;
			}
		}

		public virtual int Flags
		{
			get
			{
				return s_flags;
			}
		}

		public virtual int Size
		{
			get
			{
				return s_size;
			}
		}

		public virtual int VStubSize
		{
			get
			{
				return s_vstub_size;
			}
		}

		/// <summary>
		/// The number of imports from this module </summary>
		public virtual int Imports
		{
			get
			{
				return s_imports;
			}
		}

		public virtual int OffsetNid
		{
			get
			{
				return s_nid;
			}
		}

		public virtual int OffsetText
		{
			get
			{
				return s_text;
			}
		}

		public virtual int VStub
		{
			get
			{
				return s_vstub;
			}
		}

		public virtual bool hasVStub()
		{
			return s_size >= 6;
		}
	}

}