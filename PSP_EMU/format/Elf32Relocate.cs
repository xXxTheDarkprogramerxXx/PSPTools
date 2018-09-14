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

	public class Elf32Relocate
	{
		private int r_offset;
		private int r_info;

		public static int @sizeof()
		{
			return 8;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(ByteBuffer f) throws java.io.IOException
		public virtual void read(ByteBuffer f)
		{
			R_offset = readUWord(f);
			R_info = readUWord(f);
		}

		public override string ToString()
		{
			StringBuilder str = new StringBuilder();
			str.Append(string.Format("r_offset \t 0x{0:X8}\n", R_offset));
			str.Append(string.Format("r_info \t\t 0x{0:X8}\n", R_info));
			return str.ToString();
		}

		public virtual int R_offset
		{
			get
			{
				return r_offset;
			}
			set
			{
				this.r_offset = value;
			}
		}


		public virtual int R_info
		{
			get
			{
				return r_info;
			}
			set
			{
				this.r_info = value;
			}
		}

	}

}