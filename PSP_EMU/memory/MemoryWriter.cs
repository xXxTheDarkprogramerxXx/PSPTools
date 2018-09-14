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
	using TPointer = pspsharp.HLE.TPointer;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class MemoryWriter
	{
		private static int getMaxLength(int address)
		{
			int length;

			if (address >= MemoryMap.START_RAM && address <= MemoryMap.END_RAM)
			{
				length = MemoryMap.END_RAM - address + 1;
			}
			else if (address >= MemoryMap.START_VRAM && address <= MemoryMap.END_VRAM)
			{
				length = MemoryMap.END_VRAM - address + 1;
			}
			else if (address >= MemoryMap.START_SCRATCHPAD && address <= MemoryMap.END_SCRATCHPAD)
			{
				length = MemoryMap.END_SCRATCHPAD - address + 1;
			}
			else
			{
				length = 0;
			}

			return length;
		}

		private static IMemoryWriter getFastMemoryWriter(int address, int step)
		{
			int[] memoryInt = RuntimeContext.MemoryInt;

			switch (step)
			{
			case 1:
				return new MemoryWriterIntArray8(memoryInt, address);
			case 2:
				return new MemoryWriterIntArray16(memoryInt, address);
			case 4:
				return new MemoryWriterIntArray32(memoryInt, address);
			}

			// Default (generic) MemoryWriter
			return new MemoryWriterGeneric(address, getMaxLength(address), step);
		}

		/// <summary>
		/// Creates a MemoryWriter to write values to memory.
		/// </summary>
		/// <param name="address"> the address where to start writing.
		///                When step == 2, the address has to be 16-bit aligned ((address & 1) == 0).
		///                When step == 4, the address has to be 32-bit aligned ((address & 3) == 0). </param>
		/// <param name="length">  the maximum number of bytes that can be written. </param>
		/// <param name="step">    when step == 1, write 8-bit values
		///                when step == 2, write 16-bit values
		///                when step == 4, write 32-bit values
		///                other value for step are not allowed. </param>
		/// <returns>        the MemoryWriter </returns>
		public static IMemoryWriter getMemoryWriter(int address, int length, int step)
		{
			address &= Memory.addressMask;
			if (RuntimeContext.hasMemoryInt())
			{
				return getFastMemoryWriter(address, step);
			}

			if (!DebuggerMemory.Installed)
			{
				Buffer buffer = Memory.Instance.getBuffer(address, length);

				if (buffer is IntBuffer)
				{
					IntBuffer intBuffer = (IntBuffer) buffer;
					switch (step)
					{
					case 1:
						return new MemoryWriterInt8(intBuffer, address);
					case 2:
						return new MemoryWriterInt16(intBuffer, address);
					case 4:
						return new MemoryWriterInt32(intBuffer, address);
					}
				}
				else if (buffer is ByteBuffer)
				{
					ByteBuffer byteBuffer = (ByteBuffer) buffer;
					switch (step)
					{
					case 1:
						return new MemoryWriterByte8(byteBuffer, address);
					case 2:
						return new MemoryWriterByte16(byteBuffer, address);
					case 4:
						return new MemoryWriterByte32(byteBuffer, address);
					}
				}
			}

			// Default (generic) MemoryWriter
			return new MemoryWriterGeneric(address, length, step);
		}

		/// <summary>
		/// Creates a MemoryWriter to write values to memory.
		/// </summary>
		/// <param name="address"> the address where to start writing.
		///                When step == 2, the address has to be 16-bit aligned ((address & 1) == 0).
		///                When step == 4, the address has to be 32-bit aligned ((address & 3) == 0). </param>
		/// <param name="step">    when step == 1, write 8-bit values
		///                when step == 2, write 16-bit values
		///                when step == 4, write 32-bit values
		///                other value for step are not allowed. </param>
		/// <returns>        the MemoryWriter </returns>
		public static IMemoryWriter getMemoryWriter(int address, int step)
		{
			address &= Memory.addressMask;
			if (RuntimeContext.hasMemoryInt())
			{
				return getFastMemoryWriter(address, step);
			}
			return getMemoryWriter(address, getMaxLength(address), step);
		}

		/// <summary>
		/// Creates a MemoryWriter to write values to memory.
		/// </summary>
		/// <param name="mem">     the memory to be used. </param>
		/// <param name="address"> the address where to start writing.
		///                When step == 2, the address has to be 16-bit aligned ((address & 1) == 0).
		///                When step == 4, the address has to be 32-bit aligned ((address & 3) == 0). </param>
		/// <param name="length">  the maximum number of bytes that can be written. </param>
		/// <param name="step">    when step == 1, write 8-bit values
		///                when step == 2, write 16-bit values
		///                when step == 4, write 32-bit values
		///                other value for step are not allowed. </param>
		/// <returns>        the MemoryWriter </returns>
		public static IMemoryWriter getMemoryWriter(Memory mem, int address, int length, int step)
		{
			// Use the optimized version if we are just using the standard memory
			if (mem == RuntimeContext.memory)
			{
				return getMemoryWriter(address, length, step);
			}

			// Default (generic) MemoryWriter
			return new MemoryWriterGeneric(mem, address, length, step);
		}

		/// <summary>
		/// Creates a MemoryWriter to write values to memory.
		/// </summary>
		/// <param name="address"> the address and memory where to start writing. </param>
		/// <param name="length">  the maximum number of bytes that can be written. </param>
		/// <param name="step">    when step == 1, write 8-bit values
		///                when step == 2, write 16-bit values
		///                when step == 4, write 32-bit values
		///                other value for step are not allowed. </param>
		/// <returns>        the MemoryWriter </returns>
		public static IMemoryWriter getMemoryWriter(TPointer address, int length, int step)
		{
			return getMemoryWriter(address.Memory, address.Address, length, step);
		}

		private class MemoryWriterGeneric : IMemoryWriter
		{
			internal Memory mem;
			internal int address;
			internal int length;
			internal int step;

			public MemoryWriterGeneric(Memory mem, int address, int length, int step)
			{
				this.mem = mem;
				this.address = address;
				this.length = length;
				this.step = step;
			}

			public MemoryWriterGeneric(int address, int length, int step)
			{
				this.address = address;
				this.length = length;
				this.step = step;
				mem = Memory.Instance;
			}

			public virtual void writeNext(int value)
			{
				if (length <= 0)
				{
					return;
				}

				switch (step)
				{
				case 1:
					mem.write8(address, (sbyte) value);
					break;
				case 2:
					mem.write16(address, (short) value);
					break;
				case 4:
					mem.write32(address, value);
					break;
				}

				address += step;
				length -= step;
			}

			public virtual void flush()
			{
			}

			public virtual void skip(int n)
			{
				address += n * step;
				length -= n * step;
			}

			public virtual int CurrentAddress
			{
				get
				{
					return address;
				}
			}
		}

		private class MemoryWriterIntArray8 : IMemoryWriter
		{
			internal int index;
			internal int offset;
			internal int value;
			internal int[] buffer;
			internal static readonly int[] mask = new int[] {0, 0x000000FF, 0x0000FFFF, 0x00FFFFFF, unchecked((int)0xFFFFFFFF)};

			public MemoryWriterIntArray8(int[] buffer, int addr)
			{
				this.buffer = buffer;
				offset = addr >> 2;
				index = addr & 3;
				value = buffer[offset] & mask[index];
			}

			public virtual void writeNext(int n)
			{
				n &= 0xFF;
				if (index == 4)
				{
					buffer[offset++] = value;
					value = n;
					index = 1;
				}
				else
				{
					value |= (n << (index << 3));
					index++;
				}
			}

			public virtual void flush()
			{
				if (index > 0)
				{
					buffer[offset] = (buffer[offset] & ~mask[index]) | value;
				}
			}

			public void skip(int n)
			{
				if (n > 0)
				{
					flush();
					index += n;
					offset += index >> 2;
					index &= 3;
					value = buffer[offset] & mask[index];
				}
			}

			public virtual int CurrentAddress
			{
				get
				{
					return (offset << 2) + index;
				}
			}
		}

		private class MemoryWriterIntArray16 : IMemoryWriter
		{
			internal int index;
			internal int offset;
			internal int value;
			internal int[] buffer;

			public MemoryWriterIntArray16(int[] buffer, int addr)
			{
				this.buffer = buffer;
				offset = addr >> 2;
				index = (addr >> 1) & 1;
				if (index != 0)
				{
					value = buffer[offset] & 0x0000FFFF;
				}
			}

			public virtual void writeNext(int n)
			{
				if (index == 0)
				{
					value = n & 0xFFFF;
					index = 1;
				}
				else
				{
					buffer[offset++] = (n << 16) | value;
					index = 0;
				}
			}

			public virtual void flush()
			{
				if (index != 0)
				{
					buffer[offset] = (buffer[offset] & unchecked((int)0xFFFF0000)) | value;
				}
			}

			public virtual void skip(int n)
			{
				if (n > 0)
				{
					flush();
					index += n;
					offset += index >> 1;
					index &= 1;
					if (index != 0)
					{
						value = buffer[offset] & 0x0000FFFF;
					}
				}
			}

			public virtual int CurrentAddress
			{
				get
				{
					return (offset << 2) + (index << 1);
				}
			}
		}

		private class MemoryWriterIntArray32 : IMemoryWriter
		{
			internal int offset;
			internal int[] buffer;

			public MemoryWriterIntArray32(int[] buffer, int addr)
			{
				offset = addr >> 2;
				this.buffer = buffer;
			}

			public virtual void writeNext(int value)
			{
				buffer[offset++] = value;
			}

			public virtual void flush()
			{
			}

			public virtual void skip(int n)
			{
				offset += n;
			}

			public virtual int CurrentAddress
			{
				get
				{
					return offset << 2;
				}
			}
		}

		private class MemoryWriterInt8 : IMemoryWriter
		{
			internal int index;
			internal int value;
			internal IntBuffer buffer;
			internal int address;
			internal static readonly int[] mask = new int[] {0, 0x000000FF, 0x0000FFFF, 0x00FFFFFF, unchecked((int)0xFFFFFFFF)};

			public MemoryWriterInt8(IntBuffer buffer, int address)
			{
				this.buffer = buffer;
				this.address = address & ~3;
				index = address & 3;
				if (index > 0 && buffer.capacity() > 0)
				{
					value = buffer.get(buffer.position()) & mask[index];
				}
			}

			public virtual void writeNext(int n)
			{
				n &= 0xFF;
				if (index == 4)
				{
					buffer.put(value);
					value = n;
					index = 1;
				}
				else
				{
					value |= (n << (index << 3));
					index++;
				}
			}

			public virtual void flush()
			{
				if (index > 0)
				{
					buffer.put((buffer.get(buffer.position()) & ~mask[index]) | value);
				}
			}

			public virtual void skip(int n)
			{
				throw new System.NotSupportedException();
			}

			public virtual int CurrentAddress
			{
				get
				{
					return address + (buffer.position() << 2) + index;
				}
			}
		}

		private class MemoryWriterInt16 : IMemoryWriter
		{
			internal int index;
			internal int value;
			internal IntBuffer buffer;
			internal int address;

			public MemoryWriterInt16(IntBuffer buffer, int address)
			{
				this.buffer = buffer;
				this.address = address & ~3;
				this.index = (address & 0x02) >> 1;
				if (index != 0 && buffer.capacity() > 0)
				{
					value = buffer.get(buffer.position()) & 0x0000FFFF;
				}
			}

			public virtual void writeNext(int n)
			{
				if (index == 0)
				{
					value = n & 0xFFFF;
					index = 1;
				}
				else
				{
					buffer.put((n << 16) | value);
					index = 0;
				}
			}

			public virtual void flush()
			{
				if (index != 0)
				{
					buffer.put((buffer.get(buffer.position()) & 0xFFFF0000) | value);
				}
			}

			public virtual void skip(int n)
			{
				if (n > 0)
				{
					int bufferSkip = 0;
					if (index != 0)
					{
						flush();
						bufferSkip++;
						n--;
					}
					bufferSkip += n / 2;
					buffer.position(buffer.position() + bufferSkip);
					index = n & 1;
					if (index != 0)
					{
						value = buffer.get(buffer.position()) & 0x0000FFFF;
					}
				}
			}

			public virtual int CurrentAddress
			{
				get
				{
					return address + (buffer.position() << 2) + index;
				}
			}
		}

		private class MemoryWriterInt32 : IMemoryWriter
		{
			internal IntBuffer buffer;
			internal int address;

			public MemoryWriterInt32(IntBuffer buffer, int address)
			{
				this.buffer = buffer;
				this.address = address;
			}

			public virtual void writeNext(int value)
			{
				buffer.put(value);
			}

			public virtual void flush()
			{
			}

			public virtual void skip(int n)
			{
				if (n > 0)
				{
					buffer.position(buffer.position() + n);
				}
			}

			public virtual int CurrentAddress
			{
				get
				{
					return address + (buffer.position() << 2);
				}
			}
		}

		private class MemoryWriterByte8 : IMemoryWriter
		{
			internal ByteBuffer buffer;
			internal int address;

			public MemoryWriterByte8(ByteBuffer buffer, int address)
			{
				this.buffer = buffer;
				this.address = address;
			}

			public virtual void writeNext(int value)
			{
				buffer.put((sbyte) value);
			}

			public virtual void flush()
			{
			}

			public void skip(int n)
			{
				if (n > 0)
				{
					buffer.position(buffer.position() + n);
				}
			}

			public virtual int CurrentAddress
			{
				get
				{
					return address + buffer.position();
				}
			}
		}

		private class MemoryWriterByte16 : IMemoryWriter
		{
			internal ByteBuffer buffer;
			internal int address;

			public MemoryWriterByte16(ByteBuffer buffer, int address)
			{
				this.buffer = buffer;
				this.address = address;
			}

			public virtual void writeNext(int value)
			{
				buffer.putShort((short) value);
			}

			public virtual void flush()
			{
			}

			public void skip(int n)
			{
				if (n > 0)
				{
					buffer.position(buffer.position() + (n << 1));
				}
			}

			public virtual int CurrentAddress
			{
				get
				{
					return address + buffer.position();
				}
			}
		}

		private class MemoryWriterByte32 : IMemoryWriter
		{
			internal ByteBuffer buffer;
			internal int address;

			public MemoryWriterByte32(ByteBuffer buffer, int address)
			{
				this.buffer = buffer;
				this.address = address;
			}

			public virtual void writeNext(int value)
			{
				buffer.putInt(value);
			}

			public virtual void flush()
			{
			}

			public void skip(int n)
			{
				if (n > 0)
				{
					buffer.position(buffer.position() + (n << 2));
				}
			}

			public virtual int CurrentAddress
			{
				get
				{
					return address + buffer.position();
				}
			}
		}
	}

}