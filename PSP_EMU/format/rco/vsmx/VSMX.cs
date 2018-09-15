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
namespace pspsharp.format.rco.vsmx
{

	//using Logger = org.apache.log4j.Logger;

	public class VSMX
	{
		//public static Logger log = Logger.getLogger("vsmx");
		private const int VSMX_SIGNATURE = 0x584D5356; // "VSMX"
		private const int VSMX_VERSION = 0x00010000;
		private sbyte[] buffer;
		private int offset;
		private VSMXHeader header;
		private VSMXMem mem;
		private string name;

		public virtual VSMXMem Mem
		{
			get
			{
				return mem;
			}
		}

		private int read8()
		{
			return buffer[offset++] & 0xFF;
		}

		private int read16()
		{
			return read8() | (read8() << 8);
		}

		private int read32()
		{
			return read16() | (read16() << 16);
		}

		private void read(sbyte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = (sbyte) read8();
			}
		}

		private void seek(int offset)
		{
			this.offset = offset;
		}

		public VSMX(sbyte[] buffer, string name)
		{
			this.buffer = buffer;
			this.name = name;

			read();
		}

		public virtual string Name
		{
			get
			{
				return name;
			}
		}

		private void readHeader()
		{
			header = new VSMXHeader();
			header.sig = read32();
			header.ver = read32();
			header.codeOffset = read32();
			header.codeLength = read32();
			header.textOffset = read32();
			header.textLength = read32();
			header.textEntries = read32();
			header.propOffset = read32();
			header.propLength = read32();
			header.propEntries = read32();
			header.namesOffset = read32();
			header.namesLength = read32();
			header.namesEntries = read32();
		}

		private static bool isZero(sbyte[] buffer, int offset, int Length)
		{
			for (int i = 0; i < Length; i++)
			{
				if (buffer[offset + i] != (sbyte) 0)
				{
					return false;
				}
			}

			return true;
		}

		private string[] readStrings(int stringsOffset, int Length, int entries, Charset charset, int bytesPerChar)
		{
			string[] strings = new string[entries];
			int stringIndex = 0;
			sbyte[] buffer = new sbyte[Length];
			seek(stringsOffset);
			read(buffer);
			int stringStart = 0;
			for (int i = 0; i < Length; i += bytesPerChar)
			{
				if (isZero(buffer, i, bytesPerChar))
				{
					string s = StringHelper.NewString(buffer, stringStart, i - stringStart, charset);
					stringStart = i + bytesPerChar;
					strings[stringIndex++] = s;
				}
			}

			if (stringIndex != entries)
			{
				Console.WriteLine(string.Format("readStrings: incorrect number of strings read: stringsOffset=0x{0:X}, Length=0x{1:X}, entries=0x{2:X}, bytesPerChar={3:D}, read entries=0x{4:X}", stringsOffset, Length, entries, bytesPerChar, stringIndex));
			}

			return strings;
		}

		private void read()
		{
			readHeader();
			if (header.sig != VSMX_SIGNATURE)
			{
				Console.WriteLine(string.Format("Invalid VSMX signature 0x{0:X8}", header.sig));
				return;
			}
			if (header.ver != VSMX_VERSION)
			{
				Console.WriteLine(string.Format("Invalid VSMX version 0x{0:X8}", header.ver));
				return;
			}

			if (header.codeOffset > header.size())
			{
				Console.WriteLine(string.Format("VSMX: skipping range after header: 0x{0:X}-0x{1:X}", header.size(), header.codeOffset));
				seek(header.codeOffset);
			}

			if ((header.codeLength % VSMXGroup.SIZE_OF) != 0)
			{
				Console.WriteLine(string.Format("VSMX: code Length is not aligned to 8 bytes: 0x{0:X}", header.codeLength));
			}

			mem = new VSMXMem();
			mem.codes = new VSMXGroup[header.codeLength / VSMXGroup.SIZE_OF];
			for (int i = 0; i < mem.codes.Length; i++)
			{
				mem.codes[i] = new VSMXGroup();
				mem.codes[i].id = read32();
				mem.codes[i].value = read32();
			}

			mem.texts = readStrings(header.textOffset, header.textLength, header.textEntries, Charset.forName("UTF-16LE"), 2);
			mem.properties = readStrings(header.propOffset, header.propLength, header.propEntries, Charset.forName("UTF-16LE"), 2);
			mem.names = readStrings(header.namesOffset, header.namesLength, header.namesEntries, Charset.forName("ISO-8859-1"), 1);

			//if (log.DebugEnabled)
			{
				debug();
			}
		}

		public virtual void debug()
		{
			for (int i = 0; i < mem.codes.Length; i++)
			{
				StringBuilder s = new StringBuilder();
				VSMXGroup code = mem.codes[i];
				int opcode = code.id & 0xFF;
				if (opcode >= 0 && opcode < VSMXCode.VsmxDecOps.Length)
				{
					s.Append(VSMXCode.VsmxDecOps[opcode]);
				}
				else
				{
					s.Append(string.Format("UNKNOWN_{0:X}", opcode));
				}

				switch (opcode)
				{
					case VSMXCode.VID_CONST_BOOL:
						if (code.value == 1)
						{
							s.Append(" true");
						}
						else if (code.value == 0)
						{
							s.Append(" false");
						}
						else
						{
							s.Append(string.Format(" 0x{0:X}", code.value));
						}
						break;
					case VSMXCode.VID_CONST_INT:
					case VSMXCode.VID_DEBUG_LINE:
						s.Append(string.Format(" {0:D}", code.value));
						break;
					case VSMXCode.VID_CONST_FLOAT:
						s.Append(string.Format(" {0:F}", code.FloatValue));
						break;
					case VSMXCode.VID_CONST_STRING:
					case VSMXCode.VID_DEBUG_FILE:
						s.Append(string.Format(" '{0}'", mem.texts[code.value]));
						break;
					case VSMXCode.VID_VARIABLE:
						s.Append(string.Format(" {0}", mem.names[code.value]));
						break;
					case VSMXCode.VID_PROPERTY:
					case VSMXCode.VID_METHOD:
					case VSMXCode.VID_SET_ATTR:
					case VSMXCode.VID_UNSET:
					case VSMXCode.VID_OBJ_ADD_ATTR:
						s.Append(string.Format(" {0}", mem.properties[code.value]));
						break;
					case VSMXCode.VID_FUNCTION:
						int n = (code.id >> 16) & 0xFF;
						if (n != 0)
						{
							Console.WriteLine(string.Format("Unexpected localvars value for function at line {0:D}, expected 0, got {1:D}", i, n));
						}
						int args = (code.id >> 8) & 0xFF;
						int localVars = (code.id >> 24) & 0xFF;
						s.Append(string.Format(" args={0:D}, localVars={1:D}, startLine={2:D}", args, localVars, code.value));
						break;
					case VSMXCode.VID_UNNAMED_VAR:
						s.Append(string.Format(" {0:D}", code.value));
						break;
					// jumps
					case VSMXCode.VID_JUMP:
					case VSMXCode.VID_JUMP_TRUE:
					case VSMXCode.VID_JUMP_FALSE:
						s.Append(string.Format(" line={0:D}", code.value));
						break;
					// function calls
					case VSMXCode.VID_CALL_FUNC:
					case VSMXCode.VID_CALL_METHOD:
					case VSMXCode.VID_CALL_NEW:
						s.Append(string.Format(" args={0:D}", code.value));
						break;
					case VSMXCode.VID_MAKE_FLOAT_ARRAY:
						s.Append(string.Format(" items={0:D}", code.value));
						break;
					// ops w/o arg - check for zero
					case VSMXCode.VID_OPERATOR_ASSIGN:
					case VSMXCode.VID_OPERATOR_ADD:
					case VSMXCode.VID_OPERATOR_SUBTRACT:
					case VSMXCode.VID_OPERATOR_MULTIPLY:
					case VSMXCode.VID_OPERATOR_DIVIDE:
					case VSMXCode.VID_OPERATOR_MOD:
					case VSMXCode.VID_OPERATOR_POSITIVE:
					case VSMXCode.VID_OPERATOR_NEGATE:
					case VSMXCode.VID_OPERATOR_NOT:
					case VSMXCode.VID_P_INCREMENT:
					case VSMXCode.VID_P_DECREMENT:
					case VSMXCode.VID_INCREMENT:
					case VSMXCode.VID_DECREMENT:
					case VSMXCode.VID_OPERATOR_TYPEOF:
					case VSMXCode.VID_OPERATOR_EQUAL:
					case VSMXCode.VID_OPERATOR_NOT_EQUAL:
					case VSMXCode.VID_OPERATOR_IDENTITY:
					case VSMXCode.VID_OPERATOR_NON_IDENTITY:
					case VSMXCode.VID_OPERATOR_LT:
					case VSMXCode.VID_OPERATOR_LTE:
					case VSMXCode.VID_OPERATOR_GT:
					case VSMXCode.VID_OPERATOR_GTE:
					case VSMXCode.VID_OPERATOR_B_AND:
					case VSMXCode.VID_OPERATOR_B_XOR:
					case VSMXCode.VID_OPERATOR_B_OR:
					case VSMXCode.VID_OPERATOR_B_NOT:
					case VSMXCode.VID_OPERATOR_LSHIFT:
					case VSMXCode.VID_OPERATOR_RSHIFT:
					case VSMXCode.VID_OPERATOR_URSHIFT:
					case VSMXCode.VID_STACK_COPY:
					case VSMXCode.VID_STACK_SWAP:
					case VSMXCode.VID_END_STMT:
					case VSMXCode.VID_CONST_NULL:
					case VSMXCode.VID_CONST_EMPTYARRAY:
					case VSMXCode.VID_CONST_OBJECT:
					case VSMXCode.VID_ARRAY:
					case VSMXCode.VID_THIS:
					case VSMXCode.VID_ARRAY_INDEX:
					case VSMXCode.VID_ARRAY_INDEX_ASSIGN:
					case VSMXCode.VID_ARRAY_PUSH:
					case VSMXCode.VID_RETURN:
					case VSMXCode.VID_END:
						if (code.value != 0)
						{
							Console.WriteLine(string.Format("Unexpected non-zero value at line #{0:D}: 0x{1:X}!", i, code.value));
						}
						break;
					default:
						s.Append(string.Format(" 0x{0:X}", code.value));
						break;
				}

				Console.WriteLine(string.Format("Line#{0:D}: {1}", i, s.ToString()));
			}

			Console.WriteLine(decompile());
		}

		private string decompile()
		{
			VSMXDecompiler decompiler = new VSMXDecompiler(this);

			return decompiler.ToString();
		}
	}

}