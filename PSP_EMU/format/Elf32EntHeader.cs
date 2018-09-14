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


	public class Elf32EntHeader
	{
		// Resolved version of modulename and in a Java String
		private string modulenamez;

		private int modulename;
		private int version;
		private int attr;
		private int size;
		private int vcount;
		private int fcount;
		private int resident;
		private int vcountNew;
		private int unknown1;
		private int unknown2;

		public static int @sizeof()
		{
			return 16;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Elf32EntHeader(ByteBuffer f) throws java.io.IOException
		public Elf32EntHeader(ByteBuffer f)
		{
			modulenamez = "";

			modulename = readUWord(f);
			version = readUHalf(f);
			attr = readUHalf(f);
			size = readUByte(f);
			vcount = readUByte(f);
			fcount = readUHalf(f);
			resident = readUWord(f);
			if (size >= 5)
			{
				vcountNew = readUHalf(f);
				unknown1 = readUByte(f);
				unknown2 = readUByte(f);
			}
		}

		public Elf32EntHeader(Memory mem, int address)
		{
			modulenamez = "";

			modulename = mem.read32(address);
			version = mem.read16(address + 4);
			attr = mem.read16(address + 6);
			size = mem.read8(address + 8);
			vcount = mem.read8(address + 9);
			fcount = mem.read16(address + 10);
			resident = mem.read32(address + 12);
			if (size >= 5)
			{
				vcountNew = mem.read16(address + 16);
				unknown1 = mem.read8(address + 18);
				unknown2 = mem.read8(address + 19);
			}
		}

		public override string ToString()
		{
			StringBuilder str = new StringBuilder();
			if (!string.ReferenceEquals(modulenamez, null) && modulenamez.Length > 0)
			{
				str.Append(modulenamez + "\n");
			}
			str.Append("modulename" + "\t" + formatString("long", (modulename & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			str.Append("version" + "\t\t" + formatString("short", (version & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("attr" + "\t\t" + formatString("short", (attr & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("size" + "\t\t" + formatString("byte", (size & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("vcount" + "\t\t" + formatString("byte", (vcount & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("fcount" + "\t\t" + formatString("short", (fcount & 0xFFFF).ToString("x").ToUpper()) + "\n");
			str.Append("resident" + "\t\t" + formatString("long", (resident & 0xFFFFFFFFL).ToString("x").ToUpper()) + "\n");
			if (size >= 5)
			{
				str.Append(string.Format("vcountNew\t\t0x{0:X4}\n", vcountNew));
				str.Append(string.Format("unknown1\t\t0x{0:X2}\n", unknown1));
				str.Append(string.Format("unknown2\t\t0x{0:X2}\n", unknown2));
			}
			return str.ToString();
		}

		public virtual string ModuleNamez
		{
			get
			{
				return modulenamez;
			}
			set
			{
				modulenamez = value;
			}
		}


		public virtual int OffsetModuleName
		{
			get
			{
				return modulename;
			}
		}

		public virtual int Version
		{
			get
			{
				return version;
			}
		}

		public virtual int Attr
		{
			get
			{
				return attr;
			}
		}

		public virtual int Size
		{
			get
			{
				return size;
			}
		}

		public virtual int VariableCount
		{
			get
			{
				if (size <= 4)
				{
					return vcount;
				}
    
				// A new vcount value has been introduced for size >= 5.
				return System.Math.Max(vcount, vcountNew);
			}
		}

		public virtual int FunctionCount
		{
			get
			{
				return fcount;
			}
		}

		public virtual int OffsetResident
		{
			get
			{
				return resident;
			}
		}
	}

}