using System;
using System.Text;
using System.Threading;
using System.Collections;
using System.IO;
using System.Numerics;
using System.Net;


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
namespace pspsharp.util
{
	using Common = pspsharp.Allegrex.Common;
	using CpuState = pspsharp.Allegrex.CpuState;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using HLEModuleFunction = pspsharp.HLE.HLEModuleFunction;
	using HLEModuleManager = pspsharp.HLE.HLEModuleManager;
	using Modules = pspsharp.HLE.Modules;
	using TPointer = pspsharp.HLE.TPointer;
	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using SeekableDataInput = pspsharp.filesystems.SeekableDataInput;
	using SeekableRandomFile = pspsharp.filesystems.SeekableRandomFile;
	using UmdIsoFile = pspsharp.filesystems.umdiso.UmdIsoFile;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using MMIO = pspsharp.memory.mmio.MMIO;

	public class Utilities
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private static readonly int[] round4_Renamed = new int[] {0, 3, 2, 1};
        //public static readonly string lineSeparator = System.getProperty("line.separator");
        public static readonly string lineSeparator = Environment.NewLine;
		private static readonly char[] lineTemplate = (lineSeparator + "0x00000000 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  >................<").ToCharArray();
		private static readonly char[] hexDigits = "0123456789ABCDEF".ToCharArray();
		private static readonly char[] ascii = new char[256];
		static Utilities()
		{
			for (int i = 0; i < ascii.Length; i++)
			{
				char c = (char) i;
				if (c < ' ' || c > '~')
				{
					c = '.';
				}
				ascii[i] = c;
			}
		}

		public static string formatString(string type, string oldstring)
		{
			int counter = 0;
			if (type.Equals("byte"))
			{
				counter = 2;
			}
			if (type.Equals("short"))
			{
				counter = 4;
			}
			if (type.Equals("long"))
			{
				counter = 8;
			}
			int len = oldstring.Length;
			StringBuilder sb = new StringBuilder();
			while (len++ < counter)
			{
				sb.Append('0');
			}
			oldstring = sb.Append(oldstring).ToString();
			return oldstring;

		}

        public static string integerToBin(int value)
        {
            return Convert.ToString(0x0000000100000000L | ((value) & 0x00000000FFFFFFFFL)).Substring(1);
        }

		public static string integerToHex(int value)
		{
			return (0x100 | value).ToString("x").Substring(1).ToUpper();
		}

		public static string integerToHexShort(int value)
		{
			return (0x10000 | value).ToString("x").Substring(1).ToUpper();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static long readUWord(pspsharp.filesystems.SeekableDataInput f) throws java.io.IOException
		public static long readUWord(SeekableDataInput f)
		{
			long l = (f.readUnsignedByte() | (f.readUnsignedByte() << 8) | (f.readUnsignedByte() << 16) | (f.readUnsignedByte() << 24));
			return (l & 0xFFFFFFFFL);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int readUByte(pspsharp.filesystems.SeekableDataInput f) throws java.io.IOException
		public static int readUByte(SeekableDataInput f)
		{
			return f.readUnsignedByte();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int readUHalf(pspsharp.filesystems.SeekableDataInput f) throws java.io.IOException
		public static int readUHalf(SeekableDataInput f)
		{
			return f.readUnsignedByte() | (f.readUnsignedByte() << 8);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int readWord(pspsharp.filesystems.SeekableDataInput f) throws java.io.IOException
		public static int readWord(SeekableDataInput f)
		{
			//readByte() isn't more correct? (already exists one readUWord() method to unsign values)
			return (f.readUnsignedByte() | (f.readUnsignedByte() << 8) | (f.readUnsignedByte() << 16) | (f.readUnsignedByte() << 24));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void skipUnknown(ByteBuffer buf, int length) throws java.io.IOException
		public static void skipUnknown(ByteBuffer buf, int length)
		{
			buf.position(buf.position() + length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String readStringZ(ByteBuffer buf) throws java.io.IOException
		public static string readStringZ(ByteBuffer buf)
		{
			StringBuilder sb = new StringBuilder();
			sbyte b;
			for (; buf.position() < buf.limit();)
			{
				b = (sbyte) readUByte(buf);
				if (b == 0)
				{
					break;
				}
				sb.Append((char) b);
			}
			return sb.ToString();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String readStringNZ(ByteBuffer buf, int n) throws java.io.IOException
		public static string readStringNZ(ByteBuffer buf, int n)
		{
			StringBuilder sb = new StringBuilder();
			sbyte b;
			for (; n > 0; n--)
			{
				b = (sbyte) readUByte(buf);
				if (b != 0)
				{
					sb.Append((char) b);
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Read a string from memory. The string ends when the maximal length is
		/// reached or a '\0' byte is found. The memory bytes are interpreted as
		/// UTF-8 bytes to form the string.
		/// </summary>
		/// <param name="mem"> the memory </param>
		/// <param name="address"> the address of the first byte of the string </param>
		/// <param name="n"> the maximal string length </param>
		/// <returns> the string converted to UTF-8 </returns>
		public static string readStringNZ(Memory mem, int address, int n)
		{
			address &= Memory.addressMask;
			if (address + n > MemoryMap.END_RAM)
			{
				n = MemoryMap.END_RAM - address + 1;
				if (n < 0)
				{
					n = 0;
				}
			}

			// Allocate a byte array to store the bytes of the string.
			// At first, allocate maximum 10000 bytes in case we don't know
			// the maximal string length. The array will be extended if required.
			sbyte[] bytes = new sbyte[System.Math.Min(n, 10000)];

			int length = 0;
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(address, n, 1);
			for (; n > 0; n--)
			{
				int b = memoryReader.readNext();
				if (b == 0)
				{
					break;
				}

				if (length >= bytes.Length)
				{
					// Extend the bytes array
					bytes = extendArray(bytes, 10000);
				}

				bytes[length] = (sbyte) b;
				length++;
			}

			// Convert the bytes to UTF-8
			return StringHelper.NewString(bytes, 0, length, Constants.charset);
		}

		public static string readStringZ(Memory mem, int address)
		{
			address &= Memory.addressMask;
			return readStringNZ(mem, address, MemoryMap.END_RAM - address + 1);
		}

		public static string readStringZ(int address)
		{
			return readStringZ(Memory.Instance, address);
		}

		public static string readStringNZ(sbyte[] buffer, int offset, int n)
		{
			StringBuilder s = new StringBuilder();
			for (int i = 0; i < n; i++)
			{
				sbyte b = buffer[offset + i];
				if (b == (sbyte) 0)
				{
					break;
				}
				s.Append((char) b);
			}

			return s.ToString();
		}

		public static string readStringZ(sbyte[] buffer, int offset)
		{
			StringBuilder s = new StringBuilder();
			while (offset < buffer.Length)
			{
				sbyte b = buffer[offset++];
				if (b == (sbyte) 0)
				{
					break;
				}
				s.Append((char) b);
			}

			return s.ToString();
		}

		public static string readStringNZ(int address, int n)
		{
			return readStringNZ(Memory.Instance, address, n);
		}

		public static void writeStringNZ(Memory mem, int address, int n, string s)
		{
			int offset = 0;
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(address, n, 1);
			if (!string.ReferenceEquals(s, null))
			{
				sbyte[] bytes = s.GetBytes(Constants.charset);
				while (offset < bytes.Length && offset < n)
				{
					memoryWriter.writeNext(bytes[offset]);
					offset++;
				}
			}
			while (offset < n)
			{
				memoryWriter.writeNext(0);
				offset++;
			}
			memoryWriter.flush();
		}

		public static void writeStringNZ(sbyte[] buffer, int offset, int n, string s)
		{
			if (!string.ReferenceEquals(s, null))
			{
				sbyte[] bytes = s.GetBytes(Constants.charset);
				int length = System.Math.Min(n, bytes.Length);
				Array.Copy(bytes, 0, buffer, offset, length);
				if (length < n)
				{
					Arrays.fill(buffer, offset + length, offset + n, (sbyte) 0);
				}
			}
			else
			{
				Arrays.fill(buffer, offset, offset + n, (sbyte) 0);
			}
		}

		public static void writeStringZ(Memory mem, int address, string s)
		{
			// add 1 to the length to write the final '\0'
			writeStringNZ(mem, address, s.Length + 1, s);
		}

		public static void writeStringZ(ByteBuffer buf, string s)
		{
			buf.put(s.GetBytes());
			buf.put((sbyte) 0);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int getUnsignedByte(ByteBuffer bb) throws java.io.IOException
		public static int getUnsignedByte(ByteBuffer bb)
		{
			return bb.get() & 0xFF;
		}

		public static void putUnsignedByte(ByteBuffer bb, int value)
		{
			bb.put(unchecked((sbyte)(value & 0xFF)));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int readUByte(ByteBuffer buf) throws java.io.IOException
		public static int readUByte(ByteBuffer buf)
		{
			return getUnsignedByte(buf);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int readUHalf(ByteBuffer buf) throws java.io.IOException
		public static int readUHalf(ByteBuffer buf)
		{
			return getUnsignedByte(buf) | (getUnsignedByte(buf) << 8);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int readUWord(ByteBuffer buf) throws java.io.IOException
		public static int readUWord(ByteBuffer buf)
		{
			// No difference between signed and unsigned word (32-bit value)
			return readWord(buf);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int readWord(ByteBuffer buf) throws java.io.IOException
		public static int readWord(ByteBuffer buf)
		{
			return getUnsignedByte(buf) | (getUnsignedByte(buf) << 8) | (getUnsignedByte(buf) << 16) | (getUnsignedByte(buf) << 24);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int read8(pspsharp.HLE.VFS.IVirtualFile vFile) throws java.io.IOException
		public static int read8(IVirtualFile vFile)
		{
			sbyte[] buffer = new sbyte[1];
			int result = vFile.ioRead(buffer, 0, buffer.Length);
			if (result < buffer.Length)
			{
				return 0;
			}

			return buffer[0] & 0xFF;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int read32(pspsharp.HLE.VFS.IVirtualFile vFile) throws java.io.IOException
		public static int read32(IVirtualFile vFile)
		{
			return read8(vFile) | (read8(vFile) << 8) | (read8(vFile) << 16) | (read8(vFile) << 24);
		}

		public static void writeWord(ByteBuffer buf, int value)
		{
			putUnsignedByte(buf, value >> 0);
			putUnsignedByte(buf, value >> 8);
			putUnsignedByte(buf, value >> 16);
			putUnsignedByte(buf, value >> 24);
		}

		public static void writeHalf(ByteBuffer buf, int value)
		{
			putUnsignedByte(buf, value >> 0);
			putUnsignedByte(buf, value >> 8);
		}

		public static void writeByte(ByteBuffer buf, int value)
		{
			putUnsignedByte(buf, value);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int parseAddress(String s) throws NumberFormatException
		public static int parseAddress(string s)
		{
			int address = 0;
			if (string.ReferenceEquals(s, null))
			{
				return address;
			}

			s = s.Trim();

			if (s.StartsWith("0x", StringComparison.Ordinal))
			{
				s = s.Substring(2);
			}

			if (s.Length == 8 && s[0] >= '8')
			{
				address = (int) Convert.ToInt64(s, 16);
			}
			else
			{
				address = Convert.ToInt32(s, 16);
			}

			return address;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int parseInteger(String s) throws NumberFormatException
		public static int parseInteger(string s)
		{
			int value = 0;
			if (string.ReferenceEquals(s, null))
			{
				return value;
			}

			s = s.Trim();

			bool neg = false;
			if (s.StartsWith("-", StringComparison.Ordinal))
			{
				s = s.Substring(1);
				neg = true;
			}

			int @base = 10;
			if (s.StartsWith("0x", StringComparison.Ordinal))
			{
				s = s.Substring(2);
				@base = 16;
			}

			if (s.Length == 8 && s[0] >= '8')
			{
				value = (int) Long.parseLong(s, @base);
			}
			else
			{
				value = Integer.parseInt(s, @base);
			}

			if (neg)
			{
				value = -value;
			}

			return value;
		}

		public static int getRegister(string s)
		{
			for (int i = 0; i < Common.gprNames.Length; i++)
			{
				if (Common.gprNames[i].Equals(s, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}

			return -1;
		}

		public static int parseAddressExpression(string s)
		{
			if (string.ReferenceEquals(s, null))
			{
				return 0;
			}

			s = s.Trim();

			// Build a pattern matching all Gpr register names
			string regPattern = "";
			foreach (string gprName in Common.gprNames)
			{
				regPattern += "\\" + gprName + "|";
			}

			Memory mem = Emulator.Memory;
			CpuState cpu = Emulator.Processor.cpu;
			Pattern p;
			Matcher m;

			// Parse e.g.: "$a0"
			p = Pattern.compile(regPattern);
			m = p.matcher(s);
			if (m.matches())
			{
				int reg = getRegister(s);
				if (reg >= 0)
				{
					return cpu.getRegister(reg);
				}
			}

			// Parse e.g.: "16($a0)", "0xc($a1)"
			p = Pattern.compile("((0x)?\\p{XDigit}+)\\((" + regPattern + ")\\)");
			m = p.matcher(s);
			if (m.matches())
			{
				int offset = parseInteger(m.group(1));
				int reg = getRegister(m.group(3));

				if (reg >= 0)
				{
					return mem.read32(cpu.getRegister(reg) + offset);
				}
			}

			// Parse e.g.: "$a0 + 16", "$a1 - 0xc"
			p = Pattern.compile("(" + regPattern + ")\\s*([+\\-])\\s*((0x)?\\p{XDigit}+)");
			m = p.matcher(s);
			if (m.matches())
			{
				int reg = getRegister(m.group(1));
				int offset = parseInteger(m.group(3));
				if (m.group(2).Equals("-"))
				{
					offset = -offset;
				}

				if (reg >= 0)
				{
					return cpu.getRegister(reg) + offset;
				}
			}

			return Utilities.parseAddress(s);
		}

		/// <summary>
		/// Parse the string as a number and returns its value. If the string starts
		/// with "0x", the number is parsed in base 16, otherwise base 10.
		/// </summary>
		/// <param name="s"> the string to be parsed </param>
		/// <returns> the numeric value represented by the string. </returns>
		public static long parseLong(string s)
		{
			long value = 0;

			if (string.ReferenceEquals(s, null))
			{
				return value;
			}

			if (s.StartsWith("0x", StringComparison.Ordinal))
			{
				value = Convert.ToInt64(s.Substring(2), 16);
			}
			else
			{
				value = long.Parse(s);
			}
			return value;
		}

		/// <summary>
		/// Parse the string as a number and returns its value. The number is always
		/// parsed in base 16. The string can start as an option with "0x".
		/// </summary>
		/// <param name="s"> the string to be parsed in base 16 </param>
		/// <param name="ignoreTrailingChars"> true if trailing (i.e. non-hex characters) have to be ignored
		///                            false if non-hex characters have to raise an exception NumberFormatException </param>
		/// <returns> the numeric value represented by the string. </returns>
		public static long parseHexLong(string s, bool ignoreTrailingChars)
		{
			long value = 0;

			if (string.ReferenceEquals(s, null))
			{
				return value;
			}

			if (s.StartsWith("0x", StringComparison.Ordinal))
			{
				s = s.Substring(2);
			}

			if (ignoreTrailingChars && s.Length > 0)
			{
				for (int i = 0; i < s.Length; i++)
				{
					char c = s[i];
					// Is it an hexadecimal character?
					if ("0123456789abcdefABCDEF".IndexOf(c) < 0)
					{
						// Delete the trailing non-hex characters
						s = s.Substring(0, i);
						break;
					}
				}
			}

			value = Convert.ToInt64(s, 16);
			return value;
		}

		public static int makePow2(int n)
		{
			--n;
			n = (n >> 1) | n;
			n = (n >> 2) | n;
			n = (n >> 4) | n;
			n = (n >> 8) | n;
			n = (n >> 16) | n;
			return ++n;
		}

		/// <summary>
		/// Check if a value is a power of 2, i.e. a value that be can computed as (1 << x).
		/// </summary>
		/// <param name="n">      value to be checked </param>
		/// <returns>       true if the value is a power of 2,
		///               false otherwise. </returns>
		public static bool isPower2(int n)
		{
			return (n & (n - 1)) == 0;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void readFully(pspsharp.filesystems.SeekableDataInput input, int address, int length) throws java.io.IOException
		public static void readFully(SeekableDataInput input, int address, int length)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int blockSize = 16 * pspsharp.filesystems.umdiso.UmdIsoFile.sectorLength;
			int blockSize = 16 * UmdIsoFile.sectorLength; // 32Kb
			sbyte[] buffer = null;
			while (length > 0)
			{
				int size = System.Math.Min(length, blockSize);
				if (buffer == null || size != buffer.Length)
				{
					buffer = new sbyte[size];
				}
				input.readFully(buffer);
				Memory.Instance.copyToMemory(address, ByteBuffer.wrap(buffer), size);
				address += size;
				length -= size;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void write(pspsharp.filesystems.SeekableRandomFile output, int address, int length) throws java.io.IOException
		public static void write(SeekableRandomFile output, int address, int length)
		{
			Buffer buffer = Memory.Instance.getBuffer(address, length);
			if (buffer is ByteBuffer)
			{
				output.Channel.write((ByteBuffer) buffer);
			}
			else if (length > 0)
			{
				sbyte[] bytes = new sbyte[length];
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(address, length, 1);
				for (int i = 0; i < length; i++)
				{
					bytes[i] = (sbyte) memoryReader.readNext();
				}
				output.write(bytes);
			}
		}

		public static void bytePositionBuffer(Buffer buffer, int bytePosition)
		{
			buffer.position(bytePosition / bufferElementSize(buffer));
		}

		public static int bufferElementSize(Buffer buffer)
		{
			if (buffer is IntBuffer)
			{
				return 4;
			}

			return 1;
		}

		public static string stripNL(string s)
		{
			if (!string.ReferenceEquals(s, null) && s.EndsWith("\n", StringComparison.Ordinal))
			{
				s = s.Substring(0, s.Length - 1);
			}

			return s;
		}

		public static void putBuffer(ByteBuffer destination, Buffer source, ByteOrder sourceByteOrder)
		{
			// Set the destination to the desired ByteOrder
			ByteOrder order = destination.order();
			destination.order(sourceByteOrder);

			if (source is IntBuffer)
			{
				destination.asIntBuffer().put((IntBuffer) source);
			}
			else if (source is ShortBuffer)
			{
				destination.asShortBuffer().put((ShortBuffer) source);
			}
			else if (source is ByteBuffer)
			{
				destination.put((ByteBuffer) source);
			}
			else if (source is FloatBuffer)
			{
				destination.asFloatBuffer().put((FloatBuffer) source);
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				Modules.log.error("Utilities.putBuffer: Unsupported Buffer type " + source.GetType().FullName);
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_UNIMPLEMENTED);
			}

			// Reset the original ByteOrder of the destination
			destination.order(order);
		}

		public static void putBuffer(ByteBuffer destination, Buffer source, ByteOrder sourceByteOrder, int lengthInBytes)
		{
			// Set the destination to the desired ByteOrder
			ByteOrder order = destination.order();
			destination.order(sourceByteOrder);

			int srcLimit = source.limit();
			if (source is IntBuffer)
			{
				int copyLength = lengthInBytes & ~3;
				destination.asIntBuffer().put((IntBuffer) source.limit(source.position() + (copyLength >> 2)));
				int restLength = lengthInBytes - copyLength;
				if (restLength > 0)
				{
					// 1 to 3 bytes left to copy
					source.limit(srcLimit);
					int value = ((IntBuffer) source).get();
					int position = destination.position() + copyLength;
					do
					{
						destination.put(position, (sbyte) value);
						value >>= 8;
						restLength--;
						position++;
					} while (restLength > 0);
				}
			}
			else if (source is ByteBuffer)
			{
				destination.put((ByteBuffer) source.limit(source.position() + lengthInBytes));
			}
			else if (source is ShortBuffer)
			{
				int copyLength = lengthInBytes & ~1;
				destination.asShortBuffer().put((ShortBuffer) source.limit(source.position() + (copyLength >> 1)));
				int restLength = lengthInBytes - copyLength;
				if (restLength > 0)
				{
					// 1 byte left to copy
					source.limit(srcLimit);
					short value = ((ShortBuffer) source).get();
					destination.put(destination.position() + copyLength, (sbyte) value);
				}
			}
			else if (source is FloatBuffer)
			{
				int copyLength = lengthInBytes & ~3;
				destination.asFloatBuffer().put((FloatBuffer) source.limit(source.position() + (copyLength >> 2)));
				int restLength = lengthInBytes - copyLength;
				if (restLength > 0)
				{
					// 1 to 3 bytes left to copy
					source.limit(srcLimit);
					int value = Float.floatToRawIntBits(((FloatBuffer) source).get());
					int position = destination.position() + copyLength;
					do
					{
						destination.put(position, (sbyte) value);
						value >>= 8;
						restLength--;
						position++;
					} while (restLength > 0);
				}
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				Emulator.log.error("Utilities.putBuffer: Unsupported Buffer type " + source.GetType().FullName);
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_UNIMPLEMENTED);
			}

			// Reset the original ByteOrder of the destination
			destination.order(order);
			// Reset the original limit of the source
			source.limit(srcLimit);
		}

		/// <summary>
		/// Reads inputstream i into a String with the UTF-8 charset until the
		/// inputstream is finished (don't use with infinite streams).
		/// </summary>
		/// <param name="inputStream"> to read into a string </param>
		/// <param name="close"> if true, close the inputstream </param>
		/// <returns> a string </returns>
		/// <exception cref="java.io.IOException"> if thrown on reading the stream </exception>
		/// <exception cref="java.lang.NullPointerException"> if the given inputstream is null </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String toString(java.io.InputStream inputStream, boolean close) throws java.io.IOException
		public static string ToString(System.IO.Stream inputStream, bool close)
		{
			if (inputStream == null)
			{
				throw new System.NullReferenceException("null inputstream");
			}
			string @string;
			StringBuilder outputBuilder = new StringBuilder();

			try
			{
				System.IO.StreamReader reader = new System.IO.StreamReader(inputStream, Encoding.UTF8);
				while (null != (@string = reader.ReadLine()))
				{
					outputBuilder.Append(@string).Append('\n');
				}
			}
			finally
			{
				if (close)
				{
					Utilities.close(inputStream);
				}
			}
			return outputBuilder.ToString();
		}

		/// <summary>
		/// Close closeables. Use this in a finally clause.
		/// </summary>
		public static void close(params System.IDisposable[] closeables)
		{
			foreach (System.IDisposable c in closeables)
			{
				if (c != null)
				{
					try
					{
						c.Dispose();
					}
					catch (Exception ex)
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						Logger.getLogger(typeof(Utilities).FullName).log(Level.WARNING, "Couldn't close Closeable", ex);
					}
				}
			}
		}

		public static int getSizeKb(long sizeByte)
		{
			return (int)((sizeByte + 1023) / 1024);
		}

		private static void addAsciiDump(StringBuilder dump, IMemoryReader charReader, int bytesPerLine)
		{
			dump.Append("  >");
			for (int i = 0; i < bytesPerLine; i++)
			{
				dump.Append(ascii[charReader.readNext()]);
			}
			dump.Append("<");
		}

		private static string getMemoryDump(int address, int length, int step, int bytesPerLine, IMemoryReader memoryReader, IMemoryReader charReader)
		{
			if (length <= 0 || bytesPerLine <= 0 || step <= 0)
			{
				return "";
			}

			StringBuilder dump = new StringBuilder();

			if (length < bytesPerLine)
			{
				bytesPerLine = length;
			}

			string format = string.Format(" %0{0:D}X", step * 2);
			for (int i = 0; i < length; i += step)
			{
				if ((i % bytesPerLine) < step)
				{
					if (i > 0)
					{
						// Add an ASCII representation at the end of the line
						addAsciiDump(dump, charReader, bytesPerLine);
					}
					dump.Append(lineSeparator);
					dump.Append(string.Format("0x{0:X8}", address + i));
				}

				int value = memoryReader.readNext();
				if (length - i >= step)
				{
					dump.Append(string.format(format, value));
				}
				else
				{
					switch (length - i)
					{
						case 3:
							dump.Append(string.Format(" {0:X6}", value & 0x00FFFFFF));
							break;
						case 2:
							dump.Append(string.Format(" {0:X4}", value & 0x0000FFFF));
							break;
						case 1:
							dump.Append(string.Format(" {0:X2}", value & 0x000000FF));
							break;
					}
				}
			}

			int lengthLastLine = length % bytesPerLine;
			if (lengthLastLine > 0)
			{
				for (int i = lengthLastLine; i < bytesPerLine; i++)
				{
					dump.Append("  ");
					if ((i % step) == 0)
					{
						dump.Append(" ");
					}
				}
				addAsciiDump(dump, charReader, lengthLastLine);
			}
			else
			{
				addAsciiDump(dump, charReader, bytesPerLine);
			}

			return dump.ToString();
		}

		// Optimize the most common case
		private static string getMemoryDump(int[] memoryInt, int address, int length)
		{
			if (length <= 0)
			{
				return "";
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int numberLines = length >> 4;
			int numberLines = length >> 4;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final char[] chars = new char[numberLines * lineTemplate.length];
			char[] chars = new char[numberLines * lineTemplate.Length];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int lineOffset = lineSeparator.length() + 2;
			int lineOffset = lineSeparator.Length + 2;

			for (int i = 0, j = 0, a = (address & Memory.addressMask) >> 2; i < numberLines; i++, j += lineTemplate.Length, address += 16)
			{
				Array.Copy(lineTemplate, 0, chars, j, lineTemplate.Length);

				// Address field
				int k = j + lineOffset;
				chars[k++] = hexDigits[((int)((uint)address >> 28))];
				chars[k++] = hexDigits[(address >> 24) & 0xF];
				chars[k++] = hexDigits[(address >> 20) & 0xF];
				chars[k++] = hexDigits[(address >> 16) & 0xF];
				chars[k++] = hexDigits[(address >> 12) & 0xF];
				chars[k++] = hexDigits[(address >> 8) & 0xF];
				chars[k++] = hexDigits[(address >> 4) & 0xF];
				chars[k++] = hexDigits[(address) & 0xF];
				k++;

				// First 32-bit value
				int value = memoryInt[a++];
				if (value != 0)
				{
					chars[k++] = hexDigits[(value >> 4) & 0xF];
					chars[k++] = hexDigits[(value) & 0xF];
					k++;
					chars[k++] = hexDigits[(value >> 12) & 0xF];
					chars[k++] = hexDigits[(value >> 8) & 0xF];
					k++;
					chars[k++] = hexDigits[(value >> 20) & 0xF];
					chars[k++] = hexDigits[(value >> 16) & 0xF];
					k++;
					chars[k++] = hexDigits[((int)((uint)value >> 28))];
					chars[k++] = hexDigits[(value >> 24) & 0xF];
					k++;

					chars[k + 38] = ascii[(value) & 0xFF];
					chars[k + 39] = ascii[(value >> 8) & 0xFF];
					chars[k + 40] = ascii[(value >> 16) & 0xFF];
					chars[k + 41] = ascii[((int)((uint)value >> 24))];
				}
				else
				{
					k += 12;
				}

				// Second 32-bit value
				value = memoryInt[a++];
				if (value != 0)
				{
					chars[k++] = hexDigits[(value >> 4) & 0xF];
					chars[k++] = hexDigits[(value) & 0xF];
					k++;
					chars[k++] = hexDigits[(value >> 12) & 0xF];
					chars[k++] = hexDigits[(value >> 8) & 0xF];
					k++;
					chars[k++] = hexDigits[(value >> 20) & 0xF];
					chars[k++] = hexDigits[(value >> 16) & 0xF];
					k++;
					chars[k++] = hexDigits[((int)((uint)value >> 28))];
					chars[k++] = hexDigits[(value >> 24) & 0xF];
					k++;

					chars[k + 30] = ascii[(value) & 0xFF];
					chars[k + 31] = ascii[(value >> 8) & 0xFF];
					chars[k + 32] = ascii[(value >> 16) & 0xFF];
					chars[k + 33] = ascii[((int)((uint)value >> 24))];
				}
				else
				{
					k += 12;
				}

				// Third 32-bit value
				value = memoryInt[a++];
				if (value != 0)
				{
					chars[k++] = hexDigits[(value >> 4) & 0xF];
					chars[k++] = hexDigits[(value) & 0xF];
					k++;
					chars[k++] = hexDigits[(value >> 12) & 0xF];
					chars[k++] = hexDigits[(value >> 8) & 0xF];
					k++;
					chars[k++] = hexDigits[(value >> 20) & 0xF];
					chars[k++] = hexDigits[(value >> 16) & 0xF];
					k++;
					chars[k++] = hexDigits[((int)((uint)value >> 28))];
					chars[k++] = hexDigits[(value >> 24) & 0xF];
					k++;

					chars[k + 22] = ascii[(value) & 0xFF];
					chars[k + 23] = ascii[(value >> 8) & 0xFF];
					chars[k + 24] = ascii[(value >> 16) & 0xFF];
					chars[k + 25] = ascii[((int)((uint)value >> 24))];
				}
				else
				{
					k += 12;
				}

				// Fourth 32-bit value
				value = memoryInt[a++];
				if (value != 0)
				{
					chars[k++] = hexDigits[(value >> 4) & 0xF];
					chars[k++] = hexDigits[(value) & 0xF];
					k++;
					chars[k++] = hexDigits[(value >> 12) & 0xF];
					chars[k++] = hexDigits[(value >> 8) & 0xF];
					k++;
					chars[k++] = hexDigits[(value >> 20) & 0xF];
					chars[k++] = hexDigits[(value >> 16) & 0xF];
					k++;
					chars[k++] = hexDigits[((int)((uint)value >> 28))];
					chars[k++] = hexDigits[(value >> 24) & 0xF];
					k += 15;

					chars[k++] = ascii[(value) & 0xFF];
					chars[k++] = ascii[(value >> 8) & 0xFF];
					chars[k++] = ascii[(value >> 16) & 0xFF];
					chars[k] = ascii[((int)((uint)value >> 24))];
				}
			}

			return new string(chars);
		}

		public static string getMemoryDump(int address, int length)
		{
			if (RuntimeContext.hasMemoryInt() && (length & 0xF) == 0 && (address & 0x3) == 0 && Memory.isAddressGood(address))
			{
				// The most common case has been optimized
				return getMemoryDump(RuntimeContext.MemoryInt, address, length);
			}

			// Convenience function using default step and bytesPerLine
			return getMemoryDump(address, length, 1, 16);
		}

		public static string getMemoryDump(Memory mem, int address, int length)
		{
			// Convenience function using default step and bytesPerLine
			return getMemoryDump(mem, address, length, 1, 16);
		}

		public static string getMemoryDump(Memory mem, int address, int length, int step, int bytesPerLine)
		{
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(mem, address, length, step);
			IMemoryReader charReader = MemoryReader.getMemoryReader(mem, address, length, 1);

			return getMemoryDump(address, length, step, bytesPerLine, memoryReader, charReader);
		}

		public static string getMemoryDump(TPointer address, int length)
		{
			return getMemoryDump(address.Memory, address.Address, length);
		}

		public static string getMemoryDump(int address, int length, int step, int bytesPerLine)
		{
			Memory mem = Memory.Instance;
			if (!Memory.isAddressGood(address))
			{
				if (!RuntimeContextLLE.LLEActive || !MMIO.isAddressGood(address))
				{
					return string.Format("Invalid memory address 0x{0:X8}", address);
				}
				mem = RuntimeContextLLE.MMIO;
			}

			IMemoryReader memoryReader = MemoryReader.getMemoryReader(mem, address, length, step);
			IMemoryReader charReader = MemoryReader.getMemoryReader(mem, address, length, 1);

			return getMemoryDump(address, length, step, bytesPerLine, memoryReader, charReader);
		}

		public static string getMemoryDump(sbyte[] bytes)
		{
			return getMemoryDump(bytes, 0, bytes == null ? 0 : bytes.Length);
		}

		public static string getMemoryDump(sbyte[] bytes, int offset, int length)
		{
			// Convenience function using default step and bytesPerLine
			return getMemoryDump(bytes, offset, length, 1, 16);
		}

		public static string getMemoryDump(sbyte[] bytes, int offset, int length, int step, int bytesPerLine)
		{
			if (bytes == null || length <= 0 || bytesPerLine <= 0 || step <= 0)
			{
				return "";
			}

			IMemoryReader memoryReader = MemoryReader.getMemoryReader(0, bytes, offset, length, step);
			IMemoryReader charReader = MemoryReader.getMemoryReader(0, bytes, offset, length, step);

			return getMemoryDump(0, length, step, bytesPerLine, memoryReader, charReader);
		}

		public static int alignUp(int value, int alignment)
		{
			return alignDown(value + alignment, alignment);
		}

		public static int alignDown(int value, int alignment)
		{
			return value & ~alignment;
		}

		public static long alignDown(long value, long alignment)
		{
			return value & ~alignment;
		}

		public static int endianSwap32(int x)
		{
			return Integer.reverseBytes(x);
		}

		public static int endianSwap16(int x)
		{
			return ((x >> 8) & 0x00FF) | ((x << 8) & 0xFF00);
		}

		public static int readUnaligned32(Memory mem, int address)
		{
			switch (address & 3)
			{
				case 0:
					return mem.read32(address);
				case 2:
					return mem.read16(address) | (mem.read16(address + 2) << 16);
				default:
					return (mem.read8(address + 3) << 24) | (mem.read8(address + 2) << 16) | (mem.read8(address + 1) << 8) | (mem.read8(address));
			}
		}

		public static int readUnaligned16(Memory mem, int address)
		{
			if ((address & 1) == 0)
			{
				return mem.read16(address);
			}
			return (mem.read8(address + 1) << 8) | mem.read8(address);
		}

		public static int read8(sbyte[] buffer, int offset)
		{
			return buffer[offset] & 0xFF;
		}

		public static int readUnaligned32(sbyte[] buffer, int offset)
		{
			return (read8(buffer, offset + 3) << 24) | (read8(buffer, offset + 2) << 16) | (read8(buffer, offset + 1) << 8) | (read8(buffer, offset));
		}

		public static long readUnaligned64(sbyte[] buffer, int offset)
		{
			return (((long) read8(buffer, offset + 7)) << 56) | (((long) read8(buffer, offset + 6)) << 48) | (((long) read8(buffer, offset + 5)) << 40) | (((long) read8(buffer, offset + 4)) << 32) | (((long) read8(buffer, offset + 3)) << 24) | (((long) read8(buffer, offset + 2)) << 16) | (((long) read8(buffer, offset + 1)) << 8) | (((long) read8(buffer, offset)));
		}

		public static int readUnaligned16(sbyte[] buffer, int offset)
		{
			return (read8(buffer, offset + 1) << 8) | read8(buffer, offset);
		}

		public static void writeUnaligned32(Memory mem, int address, int data)
		{
			switch (address & 3)
			{
				case 0:
					mem.write32(address, data);
					break;
				case 2:
					mem.write16(address, (short) data);
					mem.write16(address + 2, (short)(data >> 16));
					break;
				default:
					mem.write8(address, (sbyte) data);
					mem.write8(address + 1, (sbyte)(data >> 8));
					mem.write8(address + 2, (sbyte)(data >> 16));
					mem.write8(address + 3, (sbyte)(data >> 24));
				break;
			}
		}

		public static void writeUnaligned32(sbyte[] buffer, int offset, int data)
		{
			buffer[offset + 0] = (sbyte) data;
			buffer[offset + 1] = (sbyte)(data >> 8);
			buffer[offset + 2] = (sbyte)(data >> 16);
			buffer[offset + 3] = (sbyte)(data >> 24);
		}

		public static void writeUnaligned16(sbyte[] buffer, int offset, int data)
		{
			buffer[offset + 0] = (sbyte) data;
			buffer[offset + 1] = (sbyte)(data >> 8);
		}

		public static void writeUnaligned64(sbyte[] buffer, int offset, long data)
		{
			buffer[offset + 0] = (sbyte) data;
			buffer[offset + 1] = (sbyte)(data >> 8);
			buffer[offset + 2] = (sbyte)(data >> 16);
			buffer[offset + 3] = (sbyte)(data >> 24);
			buffer[offset + 4] = (sbyte)(data >> 32);
			buffer[offset + 5] = (sbyte)(data >> 40);
			buffer[offset + 6] = (sbyte)(data >> 48);
			buffer[offset + 7] = (sbyte)(data >> 56);
		}

		public static int min(int a, int b)
		{
			return System.Math.Min(a, b);
		}

		public static float min(float a, float b)
		{
			return System.Math.Min(a, b);
		}

		public static int max(int a, int b)
		{
			return System.Math.Max(a, b);
		}

		public static float max(float a, float b)
		{
			return System.Math.Max(a, b);
		}

		/// <summary>
		/// Minimum value rounded down.
		/// </summary>
		/// <param name="a"> first float value </param>
		/// <param name="b"> second float value </param>
		/// <returns> the largest int value that is less than or equal to both
		/// parameters </returns>
		public static int minInt(float a, float b)
		{
			return floor(min(a, b));
		}

		/// <summary>
		/// Minimum value rounded down.
		/// </summary>
		/// <param name="a"> first int value </param>
		/// <param name="b"> second float value </param>
		/// <returns> the largest int value that is less than or equal to both
		/// parameters </returns>
		public static int minInt(int a, float b)
		{
			return min(a, floor(b));
		}

		/// <summary>
		/// Maximum value rounded up.
		/// </summary>
		/// <param name="a"> first float value </param>
		/// <param name="b"> second float value </param>
		/// <returns> the smallest int value that is greater than or equal to both
		/// parameters </returns>
		public static int maxInt(float a, float b)
		{
			return ceil(max(a, b));
		}

		/// <summary>
		/// Maximum value rounded up.
		/// </summary>
		/// <param name="a"> first float value </param>
		/// <param name="b"> second float value </param>
		/// <returns> the smallest int value that is greater than or equal to both
		/// parameters </returns>
		public static int maxInt(int a, float b)
		{
			return max(a, ceil(b));
		}

		public static int min(int a, int b, int c)
		{
			return System.Math.Min(a, System.Math.Min(b, c));
		}

		public static int max(int a, int b, int c)
		{
			return System.Math.Max(a, System.Math.Max(b, c));
		}

		public static void sleep(int micros)
		{
			sleep(micros / 1000, micros % 1000);
		}

		public static void sleep(int millis, int micros)
		{
			if (millis < 0)
			{
				return;
			}

			try
			{
				if (micros <= 0)
				{
					Thread.Sleep(millis);
				}
				else
				{
					Thread.Sleep(millis, micros * 1000);
				}
			}
			catch (InterruptedException)
			{
				// Ignore exception
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static void matrixMult(final float[] result, float[] m1, float[] m2)
		public static void matrixMult(float[] result, float[] m1, float[] m2)
		{
			// If the result has to be stored into one of the input matrix,
			// duplicate the input matrix.
			if (result == m1)
			{
				m1 = m1.Clone();
			}
			if (result == m2)
			{
				m2 = m2.Clone();
			}

			int i = 0;
			for (int j = 0; j < 16; j += 4)
			{
				for (int x = 0; x < 4; x++)
				{
					result[i] = m1[x] * m2[j] + m1[x + 4] * m2[j + 1] + m1[x + 8] * m2[j + 2] + m1[x + 12] * m2[j + 3];
					i++;
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static void vectorMult(final float[] result, final float[] m, final float[] v)
		public static void vectorMult(float[] result, float[] m, float[] v)
		{
			for (int i = 0; i < result.Length; i++)
			{
				float s = v[0] * m[i];
				int k = i + 4;
				for (int j = 1; j < v.Length; j++)
				{
					s += v[j] * m[k];
					k += 4;
				}
				result[i] = s;
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static void vectorMult33(final float[] result, final float[] m, final float[] v)
		public static void vectorMult33(float[] result, float[] m, float[] v)
		{
			result[0] = v[0] * m[0] + v[1] * m[4] + v[2] * m[8];
			result[1] = v[0] * m[1] + v[1] * m[5] + v[2] * m[9];
			result[2] = v[0] * m[2] + v[1] * m[6] + v[2] * m[10];
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static void vectorMult34(final float[] result, final float[] m, final float[] v)
		public static void vectorMult34(float[] result, float[] m, float[] v)
		{
			result[0] = v[0] * m[0] + v[1] * m[4] + v[2] * m[8] + v[3] * m[12];
			result[1] = v[0] * m[1] + v[1] * m[5] + v[2] * m[9] + v[3] * m[13];
			result[2] = v[0] * m[2] + v[1] * m[6] + v[2] * m[10] + v[3] * m[14];
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static void vectorMult44(final float[] result, final float[] m, final float[] v)
		public static void vectorMult44(float[] result, float[] m, float[] v)
		{
			result[0] = v[0] * m[0] + v[1] * m[4] + v[2] * m[8] + v[3] * m[12];
			result[1] = v[0] * m[1] + v[1] * m[5] + v[2] * m[9] + v[3] * m[13];
			result[2] = v[0] * m[2] + v[1] * m[6] + v[2] * m[10] + v[3] * m[14];
			result[3] = v[0] * m[3] + v[1] * m[7] + v[2] * m[11] + v[3] * m[15];
		}

		// This is equivalent to Math.round but faster: Math.round is using StrictMath.
		public static int round(float n)
		{
			return (int)(n + .5f);
		}

		public static int floor(float n)
		{
			return (int) System.Math.Floor(n);
		}

		public static int ceil(float n)
		{
			return (int) System.Math.Ceiling(n);
		}

		public static int getPower2(int n)
		{
			return Integer.numberOfTrailingZeros(makePow2(n));
		}

		public static void copy(bool[] to, bool[] from)
		{
			arraycopy(from, 0, to, 0, to.Length);
		}

		public static void copy(bool[][] to, bool[][] from)
		{
			for (int i = 0; i < to.Length; i++)
			{
				copy(to[i], from[i]);
			}
		}

		public static void copy(int[] to, int[] from)
		{
			arraycopy(from, 0, to, 0, to.Length);
		}

		public static void copy(int[][] to, int[][] from)
		{
			for (int i = 0; i < to.Length; i++)
			{
				copy(to[i], from[i]);
			}
		}

		public static void copy(int[][][] to, int[][][] from)
		{
			for (int i = 0; i < to.Length; i++)
			{
				copy(to[i], from[i]);
			}
		}

		public static void copy(int[][][][] to, int[][][][] from)
		{
			for (int i = 0; i < to.Length; i++)
			{
				copy(to[i], from[i]);
			}
		}

		public static void copy(float[] to, float[] from)
		{
			arraycopy(from, 0, to, 0, to.Length);
		}

		public static void copy(float[][] to, float[][] from)
		{
			for (int i = 0; i < to.Length; i++)
			{
				copy(to[i], from[i]);
			}
		}

		public static void copy(float[][][] to, float[][][] from)
		{
			for (int i = 0; i < to.Length; i++)
			{
				copy(to[i], from[i]);
			}
		}

		public static void copy(float[][][][] to, float[][][][] from)
		{
			for (int i = 0; i < to.Length; i++)
			{
				copy(to[i], from[i]);
			}
		}

		public static float dot3(float[] a, float[] b)
		{
			return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
		}

		public static float dot3(float[] a, float x, float y, float z)
		{
			return a[0] * x + a[1] * y + a[2] * z;
		}

		public static float length3(float[] a)
		{
			return (float) System.Math.Sqrt(a[0] * a[0] + a[1] * a[1] + a[2] * a[2]);
		}

		public static float invertedLength3(float[] a)
		{
			float length = length3(a);
			if (length == 0.0f)
			{
				return 0.0f;
			}
			return 1.0f / length;
		}

		public static void normalize3(float[] result, float[] a)
		{
			float invertedLength = invertedLength3(a);
			result[0] = a[0] * invertedLength;
			result[1] = a[1] * invertedLength;
			result[2] = a[2] * invertedLength;
		}

		public static float pow(float a, float b)
		{
			return (float) System.Math.Pow(a, b);
		}

		public static float clamp(float n, float minValue, float maxValue)
		{
			return max(minValue, min(n, maxValue));
		}

		/// <summary>
		/// Invert a 3x3 matrix.
		/// 
		/// Based on
		/// http://en.wikipedia.org/wiki/Invert_matrix#Inversion_of_3.C3.973_matrices
		/// </summary>
		/// <param name="result"> the inverted matrix (stored as a 4x4 matrix, but only 3x3
		/// is returned) </param>
		/// <param name="m"> the matrix to be inverted (stored as a 4x4 matrix, but only 3x3
		/// is used) </param>
		/// <returns> true if the matrix could be inverted false if the matrix could
		/// not be inverted </returns>
		public static bool invertMatrix3x3(float[] result, float[] m)
		{
			float A = m[5] * m[10] - m[6] * m[9];
			float B = m[6] * m[8] - m[4] * m[10];
			float C = m[4] * m[9] - m[5] * m[8];
			float det = m[0] * A + m[1] * B + m[2] * C;

			if (det == 0.0f)
			{
				// Matrix could not be inverted
				return false;
			}

			float invertedDet = 1.0f / det;
			result[0] = A * invertedDet;
			result[1] = (m[2] * m[9] - m[1] * m[10]) * invertedDet;
			result[2] = (m[1] * m[6] - m[2] * m[5]) * invertedDet;
			result[4] = B * invertedDet;
			result[5] = (m[0] * m[10] - m[2] * m[8]) * invertedDet;
			result[6] = (m[2] * m[4] - m[0] * m[6]) * invertedDet;
			result[8] = C * invertedDet;
			result[9] = (m[8] * m[1] - m[0] * m[9]) * invertedDet;
			result[10] = (m[0] * m[5] - m[1] * m[4]) * invertedDet;

			return true;
		}

		public static void transposeMatrix3x3(float[] result, float[] m)
		{
			for (int i = 0, j = 0; i < 3; i++, j += 4)
			{
				result[i] = m[j];
				result[i + 4] = m[j + 1];
				result[i + 8] = m[j + 2];
			}
		}

		public static bool sameColor(float[] c1, float[] c2, float[] c3)
		{
			for (int i = 0; i < 4; i++)
			{
				if (c1[i] != c2[i] || c1[i] != c3[i])
				{
					return false;
				}
			}

			return true;
		}

		public static bool sameColor(float[] c1, float[] c2, float[] c3, float[] c4)
		{
			for (int i = 0; i < 4; i++)
			{
				if (c1[i] != c2[i] || c1[i] != c3[i] || c1[i] != c4[i])
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Transform a pixel coordinate (floating-point value "u" or "v") into a
		/// texel coordinate (integer value to access the texture).
		/// 
		/// The texel coordinate is calculated by truncating the floating point
		/// value, not by rounding it. Otherwise transition problems occur at the
		/// borders. E.g. if a texture has a width of 64, valid texel coordinates
		/// range from 0 to 63. 64 is already outside of the texture and should not
		/// be generated when approaching the border to the texture.
		/// </summary>
		/// <param name="coordinate"> the pixel coordinate </param>
		/// <returns> the texel coordinate </returns>
		public static int pixelToTexel(float coordinate)
		{
			return (int) coordinate;
		}

		/// <summary>
		/// Wrap the value to the range [0..1[ (1 is excluded).
		/// 
		/// E.g. value == 4.0 -> return 0.0 value == 4.1 -> return 0.1 value == 4.9
		/// -> return 0.9 value == -4.0 -> return 0.0 value == -4.1 -> return 0.9
		/// (and not 0.1) value == -4.9 -> return 0.1 (and not 0.9)
		/// </summary>
		/// <param name="value"> the value to be wrapped </param>
		/// <returns> the wrapped value in the range [0..1[ (1 is excluded) </returns>
		public static float wrap(float value)
		{
			if (value >= 0.0f)
			{
				// value == 4.0 -> return 0.0
				// value == 4.1 -> return 0.1
				// value == 4.9 -> return 0.9
				return value - (int) value;
			}

			// value == -4.0 -> return 0.0
			// value == -4.1 -> return 0.9
			// value == -4.9 -> return 0.1
			// value == -1e-8 -> return 0.0
			float wrappedValue = value - (float) System.Math.Floor(value);
			if (wrappedValue >= 1.0f)
			{
				wrappedValue -= 1.0f;
			}
			return wrappedValue;
		}

		public static int wrap(float value, int valueMask)
		{
			return pixelToTexel(value) & valueMask;
		}

		public static void readBytes(int address, int length, sbyte[] bytes, int offset)
		{
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(address, length, 1);
			for (int i = 0; i < length; i++)
			{
				bytes[offset + i] = (sbyte) memoryReader.readNext();
			}
		}

		public static void writeBytes(int address, int length, sbyte[] bytes, int offset)
		{
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(address, length, 1);
			for (int i = 0; i < length; i++)
			{
				memoryWriter.writeNext(bytes[i + offset] & 0xFF);
			}
			memoryWriter.flush();
		}

		public static void readInt32(int address, int length, int[] a, int offset)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int length4 = length >> 2;
			int length4 = length >> 2;
			// Optimize the most common case
			if (RuntimeContext.hasMemoryInt())
			{
				Array.Copy(RuntimeContext.MemoryInt, (address & addressMask) >> 2, a, offset, length4);
			}
			else
			{
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(address, length, 4);
				for (int i = 0; i < length4; i++)
				{
					a[offset + i] = memoryReader.readNext();
				}
			}
		}

		public static void writeInt32(int address, int length, int[] a, int offset)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int length4 = length >> 2;
			int length4 = length >> 2;
			// Optimize the most common case
			if (RuntimeContext.hasMemoryInt())
			{
				Array.Copy(a, offset, RuntimeContext.MemoryInt, (address & addressMask) >> 2, length4);
			}
			else
			{
				IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(address, length, 4);
				for (int i = 0; i < length4; i++)
				{
					memoryWriter.writeNext(a[offset + i]);
				}
				memoryWriter.flush();
			}
		}

		public static int[] readInt32(int address, int length)
		{
			int[] a = new int[length >> 2];
			readInt32(address, length, a, 0);

			return a;
		}

		public static int round4(int n)
		{
			return n + round4_Renamed[n & 3];
		}

		public static int round2(int n)
		{
			return n + (n & 1);
		}

		public static int[] extendArray(int[] array, int extend)
		{
			if (array == null)
			{
				return new int[extend];
			}

			int[] newArray = new int[array.Length + extend];
			Array.Copy(array, 0, newArray, 0, array.Length);

			return newArray;
		}

		public static sbyte[] extendArray(sbyte[] array, int extend)
		{
			if (array == null)
			{
				return new sbyte[extend];
			}

			sbyte[] newArray = new sbyte[array.Length + extend];
			Array.Copy(array, 0, newArray, 0, array.Length);

			return newArray;
		}

		public static sbyte[] extendArray(sbyte[] array, sbyte[] extend)
		{
			if (extend == null)
			{
				return array;
			}
			return extendArray(array, extend, 0, extend.Length);
		}

		public static sbyte[] extendArray(sbyte[] array, sbyte[] extend, int offset, int length)
		{
			if (length <= 0)
			{
				return array;
			}

			if (array == null)
			{
				array = new sbyte[length];
				Array.Copy(extend, offset, array, 0, length);
				return array;
			}

			sbyte[] newArray = new sbyte[array.Length + length];
			Array.Copy(array, 0, newArray, 0, array.Length);
			Array.Copy(extend, offset, newArray, array.Length, length);

			return newArray;
		}

		public static sbyte[] copyToArrayAndExtend(sbyte[] destination, int destinationOffset, sbyte[] source, int sourceOffset, int length)
		{
			if (source == null || length <= 0)
			{
				return destination;
			}

			if (destination == null)
			{
				destination = new sbyte[destinationOffset + length];
				Array.Copy(source, sourceOffset, destination, destinationOffset, length);
				return destination;
			}

			if (destinationOffset + length > destination.Length)
			{
				destination = extendArray(destination, destinationOffset + length - destination.Length);
			}

			Array.Copy(source, sourceOffset, destination, destinationOffset, length);

			return destination;
		}

		public static TPointer[] extendArray(TPointer[] array, int extend)
		{
			if (array == null)
			{
				return new TPointer[extend];
			}

			TPointer[] newArray = new TPointer[array.Length + extend];
			Array.Copy(array, 0, newArray, 0, array.Length);

			return newArray;
		}

		public static string[] add(string[] array, string s)
		{
			if (string.ReferenceEquals(s, null))
			{
				return array;
			}
			if (array == null)
			{
				return new string[] {s};
			}

			string[] newArray = new string[array.Length + 1];
			Array.Copy(array, 0, newArray, 0, array.Length);
			newArray[array.Length] = s;

			return newArray;
		}

		public static int[] add(int[] array, int n)
		{
			if (array == null)
			{
				return new int[] {n};
			}

			int[] newArray = new int[array.Length + 1];
			Array.Copy(array, 0, newArray, 0, array.Length);
			newArray[array.Length] = n;

			return newArray;
		}

		public static sbyte[] add(sbyte[] array, sbyte n)
		{
			if (array == null)
			{
				return new sbyte[] {n};
			}

			sbyte[] newArray = new sbyte[array.Length + 1];
			Array.Copy(array, 0, newArray, 0, array.Length);
			newArray[array.Length] = n;

			return newArray;
		}

		public static File[] add(File[] array, File f)
		{
			if (f == null)
			{
				return array;
			}
			if (array == null)
			{
				return new File[] {f};
			}

			File[] newArray = new File[array.Length + 1];
			Array.Copy(array, 0, newArray, 0, array.Length);
			newArray[array.Length] = f;

			return newArray;
		}

		public static sbyte[] readCompleteFile(IVirtualFile vFile)
		{
			if (vFile == null)
			{
				return null;
			}

			sbyte[] buffer;
			try
			{
				buffer = new sbyte[(int)(vFile.length() - vFile.Position)];
			}
			catch (System.OutOfMemoryException e)
			{
				Emulator.log.error("Error while reading a complete vFile", e);
				return null;
			}

			int length = 0;
			while (length < buffer.Length)
			{
				int readLength = vFile.ioRead(buffer, length, buffer.Length - length);
				if (readLength < 0)
				{
					break;
				}
				length += readLength;
			}

			if (length < buffer.Length)
			{
				sbyte[] resizedBuffer;
				try
				{
					resizedBuffer = new sbyte[length];
				}
				catch (System.OutOfMemoryException e)
				{
					Emulator.log.error("Error while reading a complete vFile", e);
					return null;
				}
				Array.Copy(buffer, 0, resizedBuffer, 0, length);
				buffer = resizedBuffer;
			}

			return buffer;
		}

		public static bool isSystemLibraryExisting(string libraryName)
		{
			string[] extensions = new string[] {".dll", ".so"};

			string path = System.getProperty("java.library.path");
			if (string.ReferenceEquals(path, null))
			{
				path = "";
			}
			else if (!path.EndsWith("/", StringComparison.Ordinal))
			{
				path += "/";
			}

			foreach (string extension in extensions)
			{
				File libraryFile = new File(string.Format("{0}{1}{2}", path, libraryName, extension));
				if (libraryFile.canExecute())
				{
					return true;
				}
			}

			return false;
		}

		public static int signExtend(int value, int bits)
		{
			int shift = (sizeof(int) * 8) - bits;
			return (value << shift) >> shift;
		}

		public static int clip(int value, int min, int max)
		{
			if (value < min)
			{
				return min;
			}
			if (value > max)
			{
				return max;
			}

			return value;
		}

		public static float clipf(float value, float min, float max)
		{
			if (value < min)
			{
				return min;
			}
			if (value > max)
			{
				return max;
			}

			return value;
		}

		public static void fill(int[][] a, int value)
		{
			for (int i = 0; i < a.Length; i++)
			{
				Arrays.fill(a[i], value);
			}
		}

		public static void fill(float[] a, float value)
		{
			Arrays.fill(a, value);
		}

		public static void fill(float[][] a, float value)
		{
			for (int i = 0; i < a.Length; i++)
			{
				Arrays.fill(a[i], value);
			}
		}

		public static void fill(float[][][] a, float value)
		{
			for (int i = 0; i < a.Length; i++)
			{
				fill(a[i], value);
			}
		}

		public static void fill(float[][][][] a, float value)
		{
			for (int i = 0; i < a.Length; i++)
			{
				fill(a[i], value);
			}
		}

		public static long getReturnValue64(CpuState cpu)
		{
			long low = cpu._v0;
			long high = cpu._v1;
			return (low & 0xFFFFFFFFL) | (high << 32);
		}

		public static int convertABGRtoARGB(int abgr)
		{
			return (abgr & unchecked((int)0xFF00FF00)) | ((abgr & 0x00FF0000) >> 16) | ((abgr & 0x000000FF) << 16);
		}

		public static void disableSslCertificateChecks()
		{
			try
			{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: javax.net.ssl.TrustManager[] trustAllCerts = new javax.net.ssl.TrustManager[] { new javax.net.ssl.X509TrustManager() { @Override public java.security.cert.X509Certificate[] getAcceptedIssuers() { return null; } @Override public void checkClientTrusted(java.security.cert.X509Certificate[] certs, String authType) { } @Override public void checkServerTrusted(java.security.cert.X509Certificate[] certs, String authType) { } } };
				TrustManager[] trustAllCerts = new TrustManager[] {new X509TrustManager() {public X509Certificate[] AcceptedIssuers {return null;} public void checkClientTrusted(X509Certificate[] certs, string authType) { } public void checkServerTrusted(X509Certificate[] certs, string authType) { }}};
				SSLContext sc = SSLContext.getInstance("SSL");
				sc.init(null, trustAllCerts, new SecureRandom());
				HttpsURLConnection.DefaultSSLSocketFactory = sc.SocketFactory;
				HostnameVerifier allHostsValid = new HostnameVerifierAnonymousInnerClass();
				HttpsURLConnection.DefaultHostnameVerifier = allHostsValid;
			}
			catch (NoSuchAlgorithmException e)
			{
				Emulator.log.error(e);
			}
			catch (KeyManagementException e)
			{
				Emulator.log.error(e);
			}
		}

		private class HostnameVerifierAnonymousInnerClass : HostnameVerifier
		{
			public HostnameVerifierAnonymousInnerClass()
			{
			}

			public override bool verify(string hostname, SSLSession session)
			{
				return true;
			}
		}

		public static int getDefaultPortForProtocol(string protocol)
		{
			if ("http".Equals(protocol))
			{
				return 80;
			}
			if ("https".Equals(protocol))
			{
				return 443;
			}

			return -1;
		}

		public static string[] merge(string[] a1, string[] a2)
		{
			if (a1 == null)
			{
				return a2;
			}
			if (a2 == null)
			{
				return a1;
			}

			string[] a = new string[a1.Length + a2.Length];
			Array.Copy(a1, 0, a, 0, a1.Length);
			Array.Copy(a2, 0, a, a1.Length, a2.Length);

			return a;
		}

		public static InetAddress[] merge(InetAddress[] a1, InetAddress[] a2)
		{
			if (a1 == null)
			{
				return a2;
			}
			if (a2 == null)
			{
				return a1;
			}

			InetAddress[] a = new InetAddress[a1.Length + a2.Length];
			Array.Copy(a1, 0, a, 0, a1.Length);
			Array.Copy(a2, 0, a, a1.Length, a2.Length);

			return a;
		}

		public static InetAddress[] add(InetAddress[] array, InetAddress inetAddress)
		{
			if (inetAddress == null)
			{
				return array;
			}
			if (array == null)
			{
				return new InetAddress[] {inetAddress};
			}

			InetAddress[] newArray = new InetAddress[array.Length + 1];
			Array.Copy(array, 0, newArray, 0, array.Length);
			newArray[array.Length] = inetAddress;

			return newArray;
		}

		public static bool Equals(sbyte[] array1, int offset1, sbyte[] array2, int offset2, int length)
		{
			for (int i = 0; i < length; i++)
			{
				if (array1[offset1 + i] != array2[offset2 + i])
				{
					return false;
				}
			}

			return true;
		}


		public static void patch(Memory mem, SceModule module, int offset, int oldValue, int newValue)
		{
			patch(mem, module, offset, oldValue, newValue, unchecked((int)0xFFFFFFFF));
		}

		public static void patch(Memory mem, SceModule module, int offset, int oldValue, int newValue, int mask)
		{
			int checkValue = mem.read32(module.baseAddress + offset);
			if ((checkValue & mask) != (oldValue & mask))
			{
				Emulator.log.error(string.Format("Patching of module '{0}' failed at offset 0x{1:X}, 0x{2:X8} found instead of 0x{3:X8}", module.modname, offset, checkValue, oldValue));
			}
			else
			{
				mem.write32(module.baseAddress + offset, newValue);
			}
		}

		public static void patchRemoveStringChar(Memory mem, SceModule module, int offset, int oldChar)
		{
			int address = module.baseAddress + offset;
			int checkChar = mem.read8(address);
			if (checkChar != oldChar)
			{
				Emulator.log.error(string.Format("Patching of module '{0}' failed at offset 0x{1:X}, 0x{2:X2} found instead of 0x{3:X2}: {4}", module.modname, offset, checkChar, oldChar, Utilities.getMemoryDump(address - 0x100, 0x200)));
			}
			else
			{
				string s = Utilities.readStringZ(address);
				s = s.Substring(1);
				Utilities.writeStringZ(mem, address, s);
			}
		}

		public static HLEModuleFunction getHLEFunctionByAddress(int address)
		{
			HLEModuleFunction func = HLEModuleManager.Instance.getFunctionFromAddress(address);
			if (func == null)
			{
				func = Modules.LoadCoreForKernelModule.getHLEFunctionByAddress(address);
			}

			return func;
		}

		public static string getFunctionNameByAddress(int address)
		{
			string functionName = null;

			HLEModuleFunction func = HLEModuleManager.Instance.getFunctionFromAddress(address);
			if (func != null)
			{
				functionName = func.FunctionName;
			}

			if (string.ReferenceEquals(functionName, null))
			{
				functionName = Modules.LoadCoreForKernelModule.getFunctionNameByAddress(address);
			}

			return functionName;
		}

		public static void addHex(StringBuilder s, int value)
		{
			if (value == 0)
			{
				s.Append('0');
				return;
			}

			int shift = 28 - (Integer.numberOfLeadingZeros(value) & 0x3C);
			for (; shift >= 0; shift -= 4)
			{
				int digit = (value >> shift) & 0xF;
				s.Append(hexDigits[digit]);
			}
		}

		public static void addAddressHex(StringBuilder s, int address)
		{
			s.Append(hexDigits[((int)((uint)address >> 28))]);
			s.Append(hexDigits[(address >> 24) & 0xF]);
			s.Append(hexDigits[(address >> 20) & 0xF]);
			s.Append(hexDigits[(address >> 16) & 0xF]);
			s.Append(hexDigits[(address >> 12) & 0xF]);
			s.Append(hexDigits[(address >> 8) & 0xF]);
			s.Append(hexDigits[(address >> 4) & 0xF]);
			s.Append(hexDigits[(address) & 0xF]);
		}

		public static bool hasBit(int value, int bit)
		{
			return (value & (1 << bit)) != 0;
		}

		public static int setBit(int value, int bit)
		{
			return value | (1 << bit);
		}

		public static int clearBit(int value, int bit)
		{
			return value & ~(1 << bit);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static ByteBuffer readAsByteBuffer(java.io.RandomAccessFile raf) throws java.io.IOException
		public static ByteBuffer readAsByteBuffer(RandomAccessFile raf)
		{
			sbyte[] bytes = new sbyte[(int) raf.length()];
			int offset = 0;
			// Read large files by chunks.
			while (offset < bytes.Length)
			{
				int len = raf.read(bytes, offset, System.Math.Min(10 * 1024, bytes.Length - offset));
				if (len < 0)
				{
					break;
				}
				if (len > 0)
				{
					offset += len;
				}
			}

			return ByteBuffer.wrap(bytes, 0, offset);
		}
	}

}