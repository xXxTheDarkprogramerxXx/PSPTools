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
namespace pspsharp.HLE
{
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Utilities = pspsharp.util.Utilities;

	public sealed class TPointer : ITPointerBase
	{
		private Memory memory;
		private int address;
		private bool isNull;
		public static readonly TPointer NULL = new TPointer();

		protected internal TPointer()
		{
			memory = null;
			address = 0;
			isNull = true;
		}

		public TPointer(Memory memory, int address)
		{
			this.memory = memory;
			this.address = memory.normalize(address);
			isNull = (address == 0);
		}

		public TPointer(TPointer @base)
		{
			memory = @base.Memory;
			address = @base.Address;
			isNull = @base.Null;
		}

		public TPointer(TPointer @base, int addressOffset)
		{
			memory = @base.Memory;
			if (@base.Null)
			{
				address = 0;
			}
			else
			{
				address = @base.Address + addressOffset;
			}
			isNull = @base.Null;
		}

		public void add(int addressOffset)
		{
			if (NotNull)
			{
				address += addressOffset;
			}
		}

		public bool AddressGood
		{
			get
			{
				return Memory.isAddressGood(address);
			}
		}

		public bool isAlignedTo(int offset)
		{
			return (address % offset) == 0;
		}

		public int Address
		{
			get
			{
				return address;
			}
			set
			{
				this.address = memory.normalize(value);
				isNull = (value == 0);
			}
		}


		public Memory Memory
		{
			get
			{
				return memory;
			}
		}

		public bool Null
		{
			get
			{
				return isNull;
			}
		}

		public bool NotNull
		{
			get
			{
				return !isNull;
			}
		}

		public TPointer forceNonNull()
		{
			isNull = false;

			return this;
		}

		public sbyte Value8
		{
			get
			{
				return getValue8(0);
			}
			set
			{
				setValue8(0, value);
			}
		}
		public short Value16
		{
			get
			{
				return getValue16(0);
			}
			set
			{
				setValue16(0, value);
			}
		}
		public int getValue32()
		{
			return getValue32(0);
		}
		public long Value64
		{
			get
			{
				return getValue64(0);
			}
			set
			{
				setValue64(0, value);
			}
		}

		public void setValue32(int value)
		{
			setValue32(0, value);
		}
		public void setValue32(bool value)
		{
			setValue32(0, value);
		}

		public sbyte getValue8(int offset)
		{
			return (sbyte) memory.read8(address + offset);
		}
		public short getValue16(int offset)
		{
			return (short) memory.read16(address + offset);
		}
		public int getValue32(int offset)
		{
			return memory.read32(address + offset);
		}
		public long getValue64(int offset)
		{
			return memory.read64(address + offset);
		}

		public void setValue8(int offset, sbyte value)
		{
			if (NotNull)
			{
				memory.write8(address + offset, value);
			}
		}
		public void setValue16(int offset, short value)
		{
			if (NotNull)
			{
				memory.write16(address + offset, value);
			}
		}
		public void setValue32(int offset, int value)
		{
			if (NotNull)
			{
				memory.write32(address + offset, value);
			}
		}
		public void setValue32(int offset, bool value)
		{
			if (NotNull)
			{
				memory.write32(address + offset, value ? 1 : 0);
			}
		}
		public void setValue64(int offset, long value)
		{
			if (NotNull)
			{
				memory.write64(address + offset, value);
			}
		}

		public float Float
		{
			get
			{
				return getFloat(0);
			}
			set
			{
				setFloat(0, value);
			}
		}

		public float getFloat(int offset)
		{
			return Float.intBitsToFloat(getValue32(offset));
		}


		public void setFloat(int offset, float value)
		{
			setValue32(offset, Float.floatToRawIntBits(value));
		}

		public string StringZ
		{
			get
			{
				return Utilities.readStringZ(memory, address);
			}
			set
			{
				Utilities.writeStringZ(memory, address, value);
			}
		}

		public string getStringNZ(int n)
		{
			return getStringNZ(0, n);
		}

		public string getStringNZ(int offset, int n)
		{
			return Utilities.readStringNZ(memory, address + offset, n);
		}

		public void setStringNZ(int n, string s)
		{
			setStringNZ(0, n, s);
		}

		public void setStringNZ(int offset, int n, string s)
		{
			Utilities.writeStringNZ(memory, address + offset, n, s);
		}


		public sbyte[] getArray8(int n)
		{
			return getArray8(0, n);
		}

		public sbyte[] getArray8(int offset, int n)
		{
			return getArray8(offset, new sbyte[n], 0, n);
		}

		public sbyte[] getArray8(sbyte[] bytes)
		{
			if (bytes == null)
			{
				return bytes;
			}
			return getArray8(0, bytes, 0, bytes.Length);
		}

		public sbyte[] getArray8(int offset, sbyte[] bytes, int bytesOffset, int n)
		{
			if (NotNull)
			{
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(Memory, Address + offset, n, 1);
				for (int i = 0; i < n; i++)
				{
					bytes[bytesOffset + i] = (sbyte) memoryReader.readNext();
				}
			}

			return bytes;
		}

		public sbyte[] Array
		{
			set
			{
				if (value != null)
				{
					setArray(value, value.Length);
				}
			}
		}

		public void setArray(sbyte[] bytes, int n)
		{
			setArray(0, bytes, n);
		}

		public void setArray(int offset, sbyte[] bytes, int n)
		{
			setArray(offset, bytes, 0, n);
		}

		public void setArray(int offset, sbyte[] bytes, int bytesOffset, int n)
		{
			if (NotNull)
			{
				IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(Memory, Address + offset, n, 1);
				for (int i = 0; i < n; i++)
				{
					memoryWriter.writeNext(bytes[bytesOffset + i] & 0xFF);
				}
				memoryWriter.flush();
			}
		}

		public TPointer Pointer
		{
			get
			{
				return getPointer(0);
			}
			set
			{
				setPointer(0, value);
			}
		}

		public TPointer getPointer(int offset)
		{
			if (Null)
			{
				return TPointer.NULL;
			}

			return new TPointer(Memory, getValue32(offset));
		}


		public void setPointer(int offset, TPointer value)
		{
			if (value == null)
			{
				setValue32(offset, 0);
			}
			else
			{
				setValue32(offset, value.Address);
			}
		}

		public void memcpy(int src, int length)
		{
			memcpy(0, src, length);
		}

		public void memcpy(int offset, int src, int length)
		{
			if (NotNull)
			{
				memory.memcpy(Address + offset, src, length);
			}
		}

		public void memmove(int src, int length)
		{
			memmove(0, src, length);
		}

		public void memmove(int offset, int src, int length)
		{
			if (NotNull)
			{
				memory.memmove(Address + offset, src, length);
			}
		}

		/// <summary>
		/// Set "length" bytes to the value "data" starting at the pointer address.
		/// Equivalent to
		///     Memory.memset(getAddress(), data, length);
		/// </summary>
		/// <param name="data">    the byte to be set in memory </param>
		/// <param name="length">  the number of bytes to be set </param>
		public void memset(sbyte data, int length)
		{
			memset(0, data, length);
		}

		/// <summary>
		/// Set "length" bytes to the value "data" starting at the pointer address
		/// with the given "offset".
		/// Equivalent to
		///     Memory.memset(getAddress() + offset, data, length);
		/// </summary>
		/// <param name="offset">  the address offset from the pointer address </param>
		/// <param name="data">    the byte to be set in memory </param>
		/// <param name="length">  the number of bytes to be set </param>
		public void memset(int offset, sbyte data, int length)
		{
			if (NotNull)
			{
				memory.memset(Address + offset, data, length);
			}
		}

		/// <summary>
		/// Set "length" bytes to the value 0 starting at the pointer address.
		/// Equivalent to
		///     Memory.memset(getAddress(), 0, length);
		/// </summary>
		/// <param name="length">  the number of bytes to be set </param>
		public void clear(int length)
		{
			clear(0, length);
		}

		/// <summary>
		/// Set "length" bytes to the value 0 starting at the pointer address
		/// with the given "offset".
		/// Equivalent to
		///     Memory.memset(getAddress() + offset, 0, length);
		/// </summary>
		/// <param name="offset">  the address offset from the pointer address </param>
		/// <param name="length">  the number of bytes to be set </param>
		public void clear(int offset, int length)
		{
			memset(offset, (sbyte) 0, length);
		}

		public void setUnalignedValue32(int offset, int value)
		{
			if (NotNull)
			{
				Utilities.writeUnaligned32(Memory, Address + offset, value);
			}
		}

		public override string ToString()
		{
			return string.Format("0x{0:X8}", Address);
		}
	}

}