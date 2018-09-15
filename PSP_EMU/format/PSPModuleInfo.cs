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
//	import static pspsharp.util.Utilities.readUHalf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUWord;

	using pspAbstractMemoryMappedStructure = pspsharp.HLE.kernel.types.pspAbstractMemoryMappedStructure;

	public class PSPModuleInfo : pspAbstractMemoryMappedStructure
	{
		private const int NAME_LENGTH = 28;
		private int m_attr;
		private int m_version;
		private int m_gp;
		private int m_exports;
		private int m_exp_end;
		private int m_imports;
		private int m_imp_end;
		private string m_namez = ""; // String version of m_name

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(ByteBuffer f) throws java.io.IOException
		public virtual void read(ByteBuffer f)
		{
			m_attr = readUHalf(f);
			m_version = readUHalf(f);
			sbyte[] m_name = new sbyte[NAME_LENGTH];
			f.get(m_name);
			m_gp = readUWord(f);
			m_exports = readUWord(f); // .lib.ent
			m_exp_end = readUWord(f);
			m_imports = readUWord(f); // .lib.stub
			m_imp_end = readUWord(f);

			// Convert the array of bytes used for the module name to a Java String
			// Calculate the Length of the printable portion of the string, otherwise
			// any extra trailing characters may be printed as garbage.
			int len = 0;
			while (len < 28 && m_name[len] != 0)
			{
				len++;
			}
			m_namez = StringHelper.NewString(m_name, 0, len);
		}

		protected internal override void read()
		{
			m_attr = read16();
			m_version = read16();
			m_namez = readStringNZ(NAME_LENGTH);
			m_gp = read32();
			m_exports = read32();
			m_exp_end = read32();
			m_imports = read32();
			m_imp_end = read32();
		}

		protected internal override void write()
		{
			write16((short) m_attr);
			write16((short) m_version);
			writeStringNZ(NAME_LENGTH, m_namez);
			write32(m_gp);
			write32(m_exports);
			write32(m_exp_end);
			write32(m_imports);
			write32(m_imp_end);
		}

		public virtual int M_attr
		{
			get
			{
				return m_attr;
			}
		}

		public virtual int M_version
		{
			get
			{
				return m_version;
			}
		}

		public virtual int M_gp
		{
			get
			{
				return m_gp;
			}
		}

		public virtual int M_exports
		{
			get
			{
				return m_exports;
			}
		}

		public virtual int M_exp_end
		{
			get
			{
				return m_exp_end;
			}
		}

		public virtual int M_imports
		{
			get
			{
				return m_imports;
			}
		}

		public virtual int M_imp_end
		{
			get
			{
				return m_imp_end;
			}
		}

		public virtual string M_namez
		{
			get
			{
				return m_namez;
			}
		}

		public override int @sizeof()
		{
			return 52;
		}
	}

}