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
namespace pspsharp.memory
{

	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using TPointer = pspsharp.HLE.TPointer;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class MemoryReader
	{
		private static int getMaxLength(int rawAddress)
		{
			int Length;

			int address = rawAddress & Memory.addressMask;

			if (address >= MemoryMap.START_RAM && address <= MemoryMap.END_RAM)
			{
				Length = MemoryMap.END_RAM - address + 1;
			}
			else if (address >= MemoryMap.START_VRAM && address <= MemoryMap.END_VRAM)
			{
				Length = MemoryMap.END_VRAM - address + 1;
			}
			else if (address >= MemoryMap.START_SCRATCHPAD && address <= MemoryMap.END_SCRATCHPAD)
			{
				Length = MemoryMap.END_SCRATCHPAD - address + 1;
			}
			else if (rawAddress >= MemoryMap.START_IO_0 && rawAddress <= MemoryMap.END_IO_1)
			{
				Length = MemoryMap.END_IO_1 - rawAddress + 1;
			}
			else
			{
				Length = 0;
			}

			return Length;
		}

		private static IMemoryReader getFastMemoryReader(int address, int step)
		{
			int[] memoryInt = RuntimeContext.MemoryInt;

			switch (step)
			{
			case 1:
				return new MemoryReaderIntArray8(memoryInt, address);
			case 2:
				return new MemoryReaderIntArray16(memoryInt, address);
			case 4:
				return new MemoryReaderIntArray32(memoryInt, address);
			}

			// Default (generic) MemoryReader
			return new MemoryReaderGeneric(address, getMaxLength(address), step);
		}

		/// <summary>
		/// Creates a MemoryReader to read values from memory.
		/// </summary>
		/// <param name="address"> the address where to start reading.
		///                When step == 2, the address has to be 16-bit aligned ((address & 1) == 0).
		///                When step == 4, the address has to be 32-bit aligned ((address & 3) == 0). </param>
		/// <param name="Length">  the maximum number of bytes that can be read. </param>
		/// <param name="step">    when step == 1, read 8-bit values
		///                when step == 2, read 16-bit values
		///                when step == 4, read 32-bit values
		///                other value for step are not allowed. </param>
		/// <returns>        the MemoryReader </returns>
		public static IMemoryReader getMemoryReader(int address, int Length, int step)
		{
			if (Memory.isAddressGood(address))
			{
				address &= Memory.addressMask;
				if (RuntimeContext.hasMemoryInt())
				{
					return getFastMemoryReader(address, step);
				}

				if (!DebuggerMemory.Installed)
				{
					Buffer buffer = Memory.Instance.getBuffer(address, Length);

					if (buffer is IntBuffer)
					{
						IntBuffer intBuffer = (IntBuffer) buffer;
						switch (step)
						{
						case 1:
							return new MemoryReaderInt8(intBuffer, address);
						case 2:
							return new MemoryReaderInt16(intBuffer, address);
						case 4:
							return new MemoryReaderInt32(intBuffer, address);
						}
					}
					else if (buffer is ByteBuffer)
					{
						ByteBuffer byteBuffer = (ByteBuffer) buffer;
						switch (step)
						{
						case 1:
							return new MemoryReaderByte8(byteBuffer, address);
						case 2:
							return new MemoryReaderByte16(byteBuffer, address);
						case 4:
							return new MemoryReaderByte32(byteBuffer, address);
						}
					}
				}
			}

			// Default (generic) MemoryReader
			return new MemoryReaderGeneric(address, Length, step);
		}

		/// <summary>
		/// Creates a MemoryReader to read values from memory.
		/// </summary>
		/// <param name="mem">     the memory to be used. </param>
		/// <param name="address"> the address where to start reading.
		///                When step == 2, the address has to be 16-bit aligned ((address & 1) == 0).
		///                When step == 4, the address has to be 32-bit aligned ((address & 3) == 0). </param>
		/// <param name="Length">  the maximum number of bytes that can be read. </param>
		/// <param name="step">    when step == 1, read 8-bit values
		///                when step == 2, read 16-bit values
		///                when step == 4, read 32-bit values
		///                other value for step are not allowed. </param>
		/// <returns>        the MemoryReader </returns>
		public static IMemoryReader getMemoryReader(Memory mem, int address, int Length, int step)
		{
			// Use the optimized version if we are just using the standard memory
			if (mem == RuntimeContext.memory)
			{
				return getMemoryReader(address, Length, step);
			}

			// Default (generic) MemoryReader
			return new MemoryReaderGeneric(mem, address, Length, step);
		}

		/// <summary>
		/// Creates a MemoryReader to read values from memory.
		/// </summary>
		/// <param name="address"> the address and memory where to start reading. </param>
		/// <param name="Length">  the maximum number of bytes that can be read. </param>
		/// <param name="step">    when step == 1, read 8-bit values
		///                when step == 2, read 16-bit values
		///                when step == 4, read 32-bit values
		///                other value for step are not allowed. </param>
		/// <returns>        the MemoryReader </returns>
		public static IMemoryReader getMemoryReader(TPointer address, int Length, int step)
		{
			return getMemoryReader(address.Memory, address.Address, Length, step);
		}

		/// <summary>
		/// Creates a MemoryReader to read values from memory.
		/// </summary>
		/// <param name="address"> the address where to start reading.
		///                When step == 2, the address has to be 16-bit aligned ((address & 1) == 0).
		///                When step == 4, the address has to be 32-bit aligned ((address & 3) == 0). </param>
		/// <param name="step">    when step == 1, read 8-bit values
		///                when step == 2, read 16-bit values
		///                when step == 4, read 32-bit values
		///                other value for step are not allowed. </param>
		/// <returns>        the MemoryReader </returns>
		public static IMemoryReader getMemoryReader(int address, int step)
		{
			if (RuntimeContext.hasMemoryInt(address))
			{
				address &= Memory.addressMask;
				return getFastMemoryReader(address, step);
			}
			return getMemoryReader(address, getMaxLength(address), step);
		}

		public static IMemoryReader getMemoryReader(int address, sbyte[] bytes, int offset, int Length, int step)
		{
			switch (step)
			{
			case 1:
				return new MemoryReaderBytes8(address, bytes, offset, Length);
			case 2:
				return new MemoryReaderBytes16(address, bytes, offset, Length);
			case 4:
				return new MemoryReaderBytes32(address, bytes, offset, Length);
			}
			return null;
		}

		public static IMemoryReader getMemoryReader(int address, int[] ints, int offset, int Length)
		{
			return new MemoryReaderInts32(address, ints, offset, Length);

		}

		// The Java JIT compiler is producing slightly faster code for "final" methods.
		// Added "final" here only for performance reasons. Can be removed if inheritance
		// of these classes is required.
		private sealed class MemoryReaderGeneric : IMemoryReader
		{
			internal readonly Memory mem;
			internal int address;
			internal int Length;
			internal readonly int step;

			public MemoryReaderGeneric(Memory mem, int address, int Length, int step)
			{
				this.mem = mem;
				this.address = address;
				this.Length = Length;
				this.step = step;
			}

			public MemoryReaderGeneric(int address, int Length, int step)
			{
				this.address = address;
				this.Length = Length;
				this.step = step;
				if (Memory.isAddressGood(address) || RuntimeContextLLE.MMIO == null)
				{
					mem = Memory.Instance;
				}
				else
				{
					mem = RuntimeContextLLE.MMIO;
				}
			}

			public int readNext()
			{
				int n;

				if (Length <= 0)
				{
					return 0;
				}

				switch (step)
				{
				case 1:
					n = mem.read8(address);
					break;
				case 2:
					n = mem.read16(address);
					break;
				case 4:
					n = mem.read32(address);
					break;
				default:
					n = 0;
					break;
				}

				address += step;
				Length -= step;

				return n;
			}

			public void skip(int n)
			{
				address += n * step;
				Length -= n * step;
			}

			public int CurrentAddress
			{
				get
				{
					return address;
				}
			}
		}

		private sealed class MemoryReaderIntArray8 : IMemoryReader
		{
			internal int index;
			internal int offset;
			internal int value;
			internal int[] buffer;

			public MemoryReaderIntArray8(int[] buffer, int addr)
			{
				this.buffer = buffer;
				offset = addr / 4;
				index = addr & 3;
				value = buffer[offset] >> (index << 3);
			}

			public int readNext()
			{
				int n;

				if (index == 4)
				{
					index = 0;
					offset++;
					value = buffer[offset];
				}
				n = value & 0xFF;
				value >>= 8;
				index++;

				return n;
			}

			public void skip(int n)
			{
				if (n > 0)
				{
					index += n;
					offset += index >> 2;
					index &= 3;
					value = buffer[offset] >> (index << 3);
				}
			}

			public int CurrentAddress
			{
				get
				{
					return (offset << 2) + index;
				}
			}
		}

		private sealed class MemoryReaderIntArray16 : IMemoryReader
		{
			internal int index;
			internal int offset;
			internal int value;
			internal int[] buffer;

			public MemoryReaderIntArray16(int[] buffer, int addr)
			{
				this.buffer = buffer;
				offset = addr >> 2;
				index = (addr >> 1) & 1;
				if (index != 0)
				{
					value = buffer[offset];
				}
			}

			public int readNext()
			{
				int n;

				if (index == 0)
				{
					value = buffer[offset];
					n = value & 0xFFFF;
					index = 1;
				}
				else
				{
					index = 0;
					offset++;
					n = (int)((uint)value >> 16);
				}

				return n;
			}

			public void skip(int n)
			{
				if (n > 0)
				{
					index += n;
					offset += index >> 1;
					index &= 1;
					if (index != 0)
					{
						value = buffer[offset];
					}
				}
			}

			public int CurrentAddress
			{
				get
				{
					return (offset << 2) + (index << 1);
				}
			}
		}

		private sealed class MemoryReaderIntArray32 : IMemoryReader
		{
			internal int offset;
			internal int[] buffer;

			public MemoryReaderIntArray32(int[] buffer, int addr)
			{
				offset = addr / 4;
				this.buffer = buffer;
			}

			public int readNext()
			{
				return buffer[offset++];
			}

			public void skip(int n)
			{
				offset += n;
			}

			public int CurrentAddress
			{
				get
				{
					return offset << 2;
				}
			}
		}

		private sealed class MemoryReaderInt8 : IMemoryReader
		{
			internal int index;
			internal int value;
			internal IntBuffer buffer;
			internal int address;

			public MemoryReaderInt8(IntBuffer buffer, int index)
			{
				this.buffer = buffer;
				this.address = address & ~3;
				index = address & 0x03;
				if (buffer.capacity() > 0)
				{
					value = buffer.get() >> (index << 3);
				}
			}

			public int readNext()
			{
				int n;

				if (index == 4)
				{
					index = 0;
					value = buffer.get();
				}
				n = value & 0xFF;
				value >>= 8;
				index++;

				return n;
			}

			public void skip(int n)
			{
				if (n > 0)
				{
					index += n;
					buffer.position(buffer.position() + (index >> 2));
					index &= 3;
					value = buffer.get() >> (8 * index);
				}
			}

			public int CurrentAddress
			{
				get
				{
					return address + (buffer.position() << 2) + index;
				}
			}
		}

		private sealed class MemoryReaderInt16 : IMemoryReader
		{
			internal int index;
			internal int value;
			internal IntBuffer buffer;
			internal int address;

			public MemoryReaderInt16(IntBuffer buffer, int index)
			{
				this.buffer = buffer;
				this.address = address & ~3;
				this.index = (address & 0x02) >> 1;
				if (index != 0 && buffer.capacity() > 0)
				{
					value = buffer.get();
				}
			}

			public int readNext()
			{
				int n;

				if (index == 0)
				{
					value = buffer.get();
					n = value & 0xFFFF;
					index = 1;
				}
				else
				{
					index = 0;
					n = (int)((uint)value >> 16);
				}

				return n;
			}

			public void skip(int n)
			{
				if (n > 0)
				{
					index += n;
					buffer.position(buffer.position() + (index >> 1));
					index &= 1;
					if (index != 0)
					{
						value = buffer.get();
					}
				}
			}

			public int CurrentAddress
			{
				get
				{
					return address + (buffer.position() << 2) + index;
				}
			}
		}

		private sealed class MemoryReaderInt32 : IMemoryReader
		{
			internal IntBuffer buffer;
			internal int address;

			public MemoryReaderInt32(IntBuffer buffer, int address)
			{
				this.buffer = buffer;
				this.address = address;
			}

			public int readNext()
			{
				return buffer.get();
			}

			public void skip(int n)
			{
				if (n > 0)
				{
					buffer.position(buffer.position() + n);
				}
			}

			public int CurrentAddress
			{
				get
				{
					return address + (buffer.position() << 2);
				}
			}
		}

		private sealed class MemoryReaderByte8 : IMemoryReader
		{
			internal ByteBuffer buffer;
			internal int address;

			public MemoryReaderByte8(ByteBuffer buffer, int address)
			{
				this.buffer = buffer;
				this.address = address;
			}

			public int readNext()
			{
				return (buffer.get()) & 0xFF;
			}

			public void skip(int n)
			{
				if (n > 0)
				{
					buffer.position(buffer.position() + n);
				}
			}

			public int CurrentAddress
			{
				get
				{
					return address + buffer.position();
				}
			}
		}

		private sealed class MemoryReaderByte16 : IMemoryReader
		{
			internal ByteBuffer buffer;
			internal int address;

			public MemoryReaderByte16(ByteBuffer buffer, int address)
			{
				this.buffer = buffer;
				this.address = address;
			}

			public int readNext()
			{
				return (buffer.Short) & 0xFFFF;
			}

			public void skip(int n)
			{
				if (n > 0)
				{
					buffer.position(buffer.position() + (n << 1));
				}
			}

			public int CurrentAddress
			{
				get
				{
					return address + buffer.position();
				}
			}
		}

		private sealed class MemoryReaderByte32 : IMemoryReader
		{
			internal ByteBuffer buffer;
			internal int address;

			public MemoryReaderByte32(ByteBuffer buffer, int address)
			{
				this.buffer = buffer;
				this.address = address;
			}

			public int readNext()
			{
				return buffer.Int;
			}

			public void skip(int n)
			{
				if (n > 0)
				{
					buffer.position(buffer.position() + (n << 2));
				}
			}

			public int CurrentAddress
			{
				get
				{
					return address + buffer.position();
				}
			}
		}

		private sealed class MemoryReaderBytes8 : IMemoryReader
		{
			internal int address;
			internal readonly sbyte[] bytes;
			internal int offset;
			internal int maxOffset;

			public MemoryReaderBytes8(int address, sbyte[] bytes, int offset, int Length)
			{
				this.address = address;
				this.bytes = bytes;
				this.offset = offset;
				maxOffset = offset + Length;
			}

			public int readNext()
			{
				if (offset >= maxOffset)
				{
					return 0;
				}
				address++;
				return bytes[offset++] & 0xFF;
			}

			public void skip(int n)
			{
				offset += n;
				address += n;
			}

			public int CurrentAddress
			{
				get
				{
					return address;
				}
			}
		}

		private sealed class MemoryReaderBytes16 : IMemoryReader
		{
			internal int address;
			internal readonly sbyte[] bytes;
			internal int offset;
			internal int maxOffset;

			public MemoryReaderBytes16(int address, sbyte[] bytes, int offset, int Length)
			{
				this.address = address;
				this.bytes = bytes;
				this.offset = offset;
				maxOffset = offset + Length;
			}

			public int readNext()
			{
				if (offset >= maxOffset)
				{
					return 0;
				}
				address += 2;
				return (bytes[offset++] & 0xFF) | ((bytes[offset++] & 0xFF) << 8);
			}

			public void skip(int n)
			{
				offset += n * 2;
				address += n * 2;
			}

			public int CurrentAddress
			{
				get
				{
					return address;
				}
			}
		}

		private sealed class MemoryReaderBytes32 : IMemoryReader
		{
			internal int address;
			internal readonly sbyte[] bytes;
			internal int offset;
			internal int maxOffset;

			public MemoryReaderBytes32(int address, sbyte[] bytes, int offset, int Length)
			{
				this.address = address;
				this.bytes = bytes;
				this.offset = offset;
				maxOffset = offset + Length;
			}

			public int readNext()
			{
				if (offset >= maxOffset)
				{
					return 0;
				}
				address += 4;
				return (bytes[offset++] & 0xFF) | ((bytes[offset++] & 0xFF) << 8) | ((bytes[offset++] & 0xFF) << 16) | ((bytes[offset++] & 0xFF) << 24);
			}

			public void skip(int n)
			{
				offset += n * 4;
				address += n * 4;
			}

			public int CurrentAddress
			{
				get
				{
					return address;
				}
			}
		}

		private sealed class MemoryReaderInts32 : IMemoryReader
		{
			internal int address;
			internal readonly int[] ints;
			internal int offset;
			internal int maxOffset;

			public MemoryReaderInts32(int address, int[] ints, int offset, int Length)
			{
				this.address = address;
				this.ints = ints;
				this.offset = offset;
				maxOffset = offset + (Length >> 2);
			}

			public int readNext()
			{
				if (offset >= maxOffset)
				{
					return 0;
				}
				address += 4;
				return ints[offset++];
			}

			public void skip(int n)
			{
				offset += n;
				address += n * 4;
			}

			public int CurrentAddress
			{
				get
				{
					return address;
				}
			}
		}
	}

}