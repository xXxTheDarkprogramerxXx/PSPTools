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
namespace pspsharp.HLE.kernel.types
{

	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Utilities = pspsharp.util.Utilities;

	public abstract class pspAbstractMemoryMappedStructure
	{
		private const int unknown = 0x11111111;
		public static readonly Charset charset16 = Charset.forName("UTF-16LE");

		private int baseAddress;
		private int maxSize = int.MaxValue;
		private int offset;
		protected internal Memory mem;

		public abstract int @sizeof();
		protected internal abstract void read();
		protected internal abstract void write();

		private void start(Memory mem)
		{
			this.mem = mem;
			offset = 0;
		}

		protected internal virtual void start(Memory mem, int address)
		{
			start(mem);
			baseAddress = address;
		}

		private void start(Memory mem, int address, int maxSize)
		{
			start(mem, address);
			this.maxSize = maxSize;
		}

		public virtual int MaxSize
		{
			set
			{
				// value is an unsigned int
				if (value < 0)
				{
					value = int.MaxValue;
				}
				this.maxSize = value;
			}
		}

		public virtual void read(Memory mem, int address)
		{
			start(mem, address);
			if (address != 0)
			{
				read();
			}
		}

		public virtual void read(ITPointerBase pointer)
		{
			read(pointer, 0);
		}

		public virtual void read(ITPointerBase pointer, int offset)
		{
			start(pointer.Memory, pointer.Address + offset);
			if (pointer.NotNull)
			{
				read();
			}
		}

		public virtual void write(Memory mem, int address)
		{
			start(mem, address);
			write();
		}

		public virtual void write(ITPointerBase pointer)
		{
			write(pointer, 0);
		}

		public virtual void write(ITPointerBase pointer, int offset)
		{
			write(pointer.Memory, pointer.Address + offset);
		}

		public virtual void write(Memory mem)
		{
			start(mem);
			write();
		}

		protected internal virtual int read8()
		{
			int value;
			if (offset >= maxSize)
			{
				value = 0;
			}
			else
			{
				value = mem.read8(baseAddress + offset);
			}
			offset += 1;

			return value;
		}

		protected internal virtual void read8Array(sbyte[] array)
		{
			for (int i = 0; array != null && i < array.Length; i++)
			{
				array[i] = (sbyte) read8();
			}
		}

		protected internal virtual void align16()
		{
			offset = (offset + 1) & ~1;
		}

		protected internal virtual void align32()
		{
			offset = (offset + 3) & ~3;
		}

		protected internal virtual int read16()
		{
			align16();

			int value;
			if (offset >= maxSize)
			{
				value = 0;
			}
			else
			{
				value = mem.read16(baseAddress + offset);
			}
			offset += 2;

			if (BigEndian)
			{
				value = endianSwap16((short) value);
			}

			return value;
		}

		protected internal virtual int readUnaligned16()
		{
			int n0 = read8();
			int n1 = read8();

			if (BigEndian)
			{
				return (n0 << 8) | n1;
			}
			return (n1 << 8) | n0;
		}

		protected internal virtual int read32()
		{
			align32();

			int value;
			if (offset >= maxSize)
			{
				value = 0;
			}
			else
			{
				value = mem.read32(baseAddress + offset);
			}
			offset += 4;

			if (BigEndian)
			{
				value = endianSwap32(value);
			}

			return value;
		}

		protected internal virtual int readUnaligned32()
		{
			int n01 = readUnaligned16();
			int n23 = readUnaligned16();

			if (BigEndian)
			{
				return (n01 << 16) | n23;
			}
			return (n23 << 16) | n01;
		}

		protected internal virtual long read64()
		{
			align32();

			long value;
			if (offset >= maxSize)
			{
				value = 0;
			}
			else
			{
				value = mem.read64(baseAddress + offset);
			}
			offset += 8;

			if (BigEndian)
			{
				value = endianSwap64(value);
			}

			return value;
		}

		protected internal virtual void read32Array(int[] array)
		{
			for (int i = 0; array != null && i < array.Length; i++)
			{
				array[i] = read32();
			}
		}

		protected internal virtual bool readBoolean()
		{
			int value = read8();

			return (value != 0);
		}

		protected internal virtual void readBooleanArray(bool[] array)
		{
			for (int i = 0; array != null && i < array.Length; i++)
			{
				array[i] = readBoolean();
			}
		}

		protected internal virtual float readFloat()
		{
			int int32 = read32();

			return Float.intBitsToFloat(int32);
		}

		protected internal virtual void readFloatArray(float[] array)
		{
			for (int i = 0; array != null && i < array.Length; i++)
			{
				array[i] = readFloat();
			}
		}

		protected internal virtual void readFloatArray(float[][] array)
		{
			for (int i = 0; array != null && i < array.Length; i++)
			{
				readFloatArray(array[i]);
			}
		}

		protected internal virtual void readUnknown(int length)
		{
			offset += length;
		}

		protected internal virtual string readStringNZ(int n)
		{
			string s;
			if (offset >= maxSize)
			{
				s = null;
			}
			else
			{
				s = Utilities.readStringNZ(mem, baseAddress + offset, n);
			}
			offset += n;

			return s;
		}

		protected internal virtual string readStringZ(int addr)
		{
			if (addr == 0)
			{
				return null;
			}
			return Utilities.readStringZ(mem, addr);
		}

		protected internal virtual string readStringUTF16NZ(int n)
		{
			StringBuilder s = new StringBuilder();
			while (n > 0)
			{
				int char16 = read16();
				n -= 2;
				if (char16 == 0)
				{
					break;
				}
				sbyte[] bytes = new sbyte[2];
				bytes[0] = (sbyte) char16;
				bytes[1] = (sbyte)(char16 >> 8);
				s.Append(StringHelper.NewString(bytes, charset16));
			}

			if (n > 0)
			{
				readUnknown(n);
			}

			return s.ToString();
		}

		protected internal virtual int writeStringUTF16NZ(int n, string s)
		{
			sbyte[] bytes = s.GetBytes(charset16);
			int length = 0;
			if (bytes != null)
			{
				length = bytes.Length;
				for (int i = 0; i < bytes.Length && n > 0; i++, n--)
				{
					write8(bytes[i]);
				}
			}

			// Write trailing '\0\0'
			if (n > 0)
			{
				write8((sbyte) 0);
				n--;
			}
			if (n > 0)
			{
				write8((sbyte) 0);
				n--;
			}

			if (n > 0)
			{
				offset += n;
			}

			return length;
		}

		/// <summary>
		/// Read a string in UTF16, until '\0\0' </summary>
		/// <param name="addr"> address of the string </param>
		/// <returns> the string </returns>
		protected internal virtual string readStringUTF16Z(int addr)
		{
			if (addr == 0)
			{
				return null;
			}

			IMemoryReader memoryReader = MemoryReader.getMemoryReader(addr, 2);
			StringBuilder s = new StringBuilder();
			while (true)
			{
				int char16 = memoryReader.readNext();
				if (char16 == 0)
				{
					break;
				}
				sbyte[] bytes = new sbyte[2];
				bytes[0] = (sbyte) char16;
				bytes[1] = (sbyte)(char16 >> 8);
				s.Append(StringHelper.NewString(bytes, charset16));
			}

			return s.ToString();
		}

		protected internal virtual TPointer readPointer()
		{
			int value = read32();
			if (value == 0)
			{
				return TPointer.NULL;
			}

			return new TPointer(mem, value);
		}

		protected internal virtual void readPointerArray(TPointer[] array)
		{
			for (int i = 0; array != null && i < array.Length; i++)
			{
				array[i] = readPointer();
			}
		}

		/// <summary>
		/// Write a string in UTF16, including a trailing '\0\0' </summary>
		/// <param name="addr"> address where to write the string </param>
		/// <param name="s"> the string to write </param>
		/// <returns> the number of bytes written (not including the trailing '\0\0') </returns>
		protected internal virtual int writeStringUTF16Z(int addr, string s)
		{
			if (addr == 0 || string.ReferenceEquals(s, null))
			{
				return 0;
			}

			sbyte[] bytes = s.GetBytes(charset16);
			if (bytes == null)
			{
				return 0;
			}

			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(addr, bytes.Length + 2, 1);
			for (int i = 0; i < bytes.Length; i++)
			{
				memoryWriter.writeNext(bytes[i] & 0xFF);
			}

			// Write trailing '\0\0'
			memoryWriter.writeNext(0);
			memoryWriter.writeNext(0);
			memoryWriter.flush();

			return bytes.Length;
		}

		protected internal virtual void read(pspAbstractMemoryMappedStructure @object)
		{
			if (@object == null)
			{
				return;
			}

			if (offset < maxSize)
			{
				@object.start(mem, baseAddress + offset, maxSize - offset);
				@object.read();
			}
			offset += @object.@sizeof();
		}

		protected internal virtual void write8(sbyte data)
		{
			if (offset < maxSize)
			{
				mem.write8(baseAddress + offset, data);
			}
			offset += 1;
		}

		protected internal virtual void write8Array(sbyte[] array)
		{
			for (int i = 0; array != null && i < array.Length; i++)
			{
				write8(array[i]);
			}
		}

		protected internal virtual void write16(short data)
		{
			align16();
			if (offset < maxSize)
			{
				if (BigEndian)
				{
					data = (short) endianSwap16(data);
				}

				mem.write16(baseAddress + offset, data);
			}
			offset += 2;
		}

		protected internal virtual void writeUnaligned16(short data)
		{
			if (BigEndian)
			{
				write8((sbyte)((short)((ushort)data >> 8)));
				write8((sbyte) data);
			}
			else
			{
				write8((sbyte) data);
				write8((sbyte)((short)((ushort)data >> 8)));
			}
		}

		protected internal virtual void write32(int data)
		{
			align32();
			if (offset < maxSize)
			{
				if (BigEndian)
				{
					data = endianSwap32(data);
				}

				mem.write32(baseAddress + offset, data);
			}
			offset += 4;
		}

		protected internal virtual void writeUnaligned32(int data)
		{
			if (BigEndian)
			{
				writeUnaligned16((short)((int)((uint)data >> 16)));
				writeUnaligned16((short) data);
			}
			else
			{
				writeUnaligned16((short) data);
				writeUnaligned16((short)((int)((uint)data >> 16)));
			}
		}

		protected internal virtual void write64(long data)
		{
			align32();
			if (offset < maxSize)
			{
				if (BigEndian)
				{
					data = endianSwap64(data);
				}

				mem.write64(baseAddress + offset, data);
			}
			offset += 8;
		}

		protected internal virtual void write32Array(int[] array)
		{
			for (int i = 0; array != null && i < array.Length; i++)
			{
				write32(array[i]);
			}
		}

		protected internal virtual void writeBoolean(bool data)
		{
			write8(data ? (sbyte) 1 : (sbyte) 0);
		}

		protected internal virtual void writeBooleanArray(bool[] array)
		{
			for (int i = 0; array != null && i < array.Length; i++)
			{
				writeBoolean(array[i]);
			}
		}

		protected internal virtual void writeFloat(float data)
		{
			int int32 = Float.floatToIntBits(data);
			write32(int32);
		}

		protected internal virtual void writeFloatArray(float[] array)
		{
			for (int i = 0; array != null && i < array.Length; i++)
			{
				writeFloat(array[i]);
			}
		}

		protected internal virtual void writeFloatArray(float[][] array)
		{
			for (int i = 0; array != null && i < array.Length; i++)
			{
				writeFloatArray(array[i]);
			}
		}

		protected internal virtual void writeUnknown(int length)
		{
			for (int i = 0; i < length; i++)
			{
				write8((sbyte) unknown);
			}
		}

		protected internal virtual void writeSkip(int length)
		{
			offset += length;
		}

		protected internal virtual void writeStringN(int n, string s)
		{
			if (offset < maxSize)
			{
				Utilities.writeStringNZ(mem, baseAddress + offset, n, s);
				// A NULL-byte has only been written at the end of the string
				// when enough space was available.
			}
			offset += n;
		}

		protected internal virtual void writeStringNZ(int n, string s)
		{
			if (offset < maxSize)
			{
				Utilities.writeStringNZ(mem, baseAddress + offset, n - 1, s);
				// Write always a NULL-byte at the end of the string.
				mem.write8(baseAddress + offset + n - 1, (sbyte) 0);
			}
			offset += n;
		}

		protected internal virtual void writeStringZ(string s, int addr)
		{
			if (!string.ReferenceEquals(s, null) && addr != 0)
			{
				Utilities.writeStringZ(mem, addr, s);
			}
		}

		protected internal virtual void write(pspAbstractMemoryMappedStructure @object)
		{
			if (@object == null)
			{
				return;
			}

			if (offset < maxSize)
			{
				@object.start(mem, baseAddress + offset, maxSize - offset);
				@object.write();
			}
			offset += @object.@sizeof();
		}

		protected internal virtual void writePointer(TPointer pointer)
		{
			if (pointer == null)
			{
				write32(0);
			}
			else
			{
				write32(pointer.Address);
			}
		}

		protected internal virtual void writePointerArray(TPointer[] array)
		{
			for (int i = 0; array != null && i < array.Length; i++)
			{
				writePointer(array[i]);
			}
		}

		protected internal virtual int Offset
		{
			get
			{
				return offset;
			}
		}

		public virtual int BaseAddress
		{
			get
			{
				return baseAddress;
			}
		}

		public virtual bool Null
		{
			get
			{
				return baseAddress == 0;
			}
		}

		public virtual bool NotNull
		{
			get
			{
				return baseAddress != 0;
			}
		}

		protected internal virtual int endianSwap16(short data)
		{
			return Short.reverseBytes(data) & 0xFFFF;
		}

		protected internal virtual int endianSwap32(int data)
		{
			return Integer.reverseBytes(data);
		}

		protected internal virtual long endianSwap64(long data)
		{
			return Long.reverseBytes(data);
		}

		protected internal virtual bool BigEndian
		{
			get
			{
				return false;
			}
		}

		public override string ToString()
		{
			return string.Format("0x{0:X8}", BaseAddress);
		}
	}

}