/*

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

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class MemoryReaderWriter
	{
		private static IMemoryReaderWriter getFastMemoryReaderWriter(int address, int step)
		{
			int[] memoryInt = RuntimeContext.MemoryInt;

			// Implement the most common cases with dedicated classes.
			switch (step)
			{
			case 2:
				return new MemoryReaderWriterIntArray16(memoryInt, address);
			case 4:
				return new MemoryReaderWriterIntArray32(memoryInt, address);
			}

			// No dedicated class available, use the generic one.
			return new MemoryReaderWriterGeneric(address, step);
		}

		public static IMemoryReaderWriter getMemoryReaderWriter(int address, int Length, int step)
		{
			address &= Memory.addressMask;
			if (RuntimeContext.hasMemoryInt())
			{
				return getFastMemoryReaderWriter(address, step);
			}

			// No dedicated class available, use the generic one.
			return new MemoryReaderWriterGeneric(address, Length, step);
		}

		public static IMemoryReaderWriter getMemoryReaderWriter(int address, int step)
		{
			address &= Memory.addressMask;
			if (RuntimeContext.hasMemoryInt())
			{
				return getFastMemoryReaderWriter(address, step);
			}

			// No dedicated class available, use the generic one.
			return new MemoryReaderWriterGeneric(address, step);
		}

		private sealed class MemoryReaderWriterGeneric : IMemoryReaderWriter
		{
			internal IMemoryReader memoryReader;
			internal IMemoryWriter memoryWriter;
			internal int currentValue;

			public MemoryReaderWriterGeneric(int address, int Length, int step)
			{
				memoryReader = MemoryReader.getMemoryReader(address, Length, step);
				memoryWriter = MemoryWriter.getMemoryWriter(address, Length, step);
				currentValue = memoryReader.readNext();
			}

			public MemoryReaderWriterGeneric(int address, int step)
			{
				memoryReader = MemoryReader.getMemoryReader(address, step);
				memoryWriter = MemoryWriter.getMemoryWriter(address, step);
				currentValue = memoryReader.readNext();
			}

			public void writeNext(int value)
			{
				memoryWriter.writeNext(value);
				currentValue = memoryReader.readNext();
			}

			public void skip(int n)
			{
				if (n > 0)
				{
					memoryWriter.skip(n);
					memoryReader.skip(n - 1);
					currentValue = memoryReader.readNext();
				}
			}

			public void flush()
			{
				memoryWriter.flush();
			}

			public int CurrentAddress
			{
				get
				{
					return memoryWriter.CurrentAddress;
				}
			}

			public int readCurrent()
			{
				return currentValue;
			}
		}

		private sealed class MemoryReaderWriterIntArray32 : IMemoryReaderWriter
		{
			internal int offset;
			internal readonly int[] buffer;

			public MemoryReaderWriterIntArray32(int[] buffer, int addr)
			{
				offset = addr >> 2;
				this.buffer = buffer;
			}

			public void writeNext(int value)
			{
				buffer[offset++] = value;
			}

			public void skip(int n)
			{
				offset += n;
			}

			public void flush()
			{
			}

			public int CurrentAddress
			{
				get
				{
					return offset << 2;
				}
			}

			public int readCurrent()
			{
				return buffer[offset];
			}
		}

		private sealed class MemoryReaderWriterIntArray16 : IMemoryReaderWriter
		{
			internal int index;
			internal int offset;
			internal int value;
			internal readonly int[] buffer;

			public MemoryReaderWriterIntArray16(int[] buffer, int addr)
			{
				this.buffer = buffer;
				offset = addr >> 2;
				index = (addr >> 1) & 1;
				if (index != 0)
				{
					value = buffer[offset] & 0x0000FFFF;
				}
			}

			public void writeNext(int n)
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

			public void skip(int n)
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

			public void flush()
			{
				if (index != 0)
				{
					buffer[offset] = (buffer[offset] & unchecked((int)0xFFFF0000)) | value;
				}
			}

			public int CurrentAddress
			{
				get
				{
					return (offset << 2) + (index << 1);
				}
			}

			public int readCurrent()
			{
				if (index == 0)
				{
					return buffer[offset] & 0xFFFF;
				}
				return (int)((uint)buffer[offset] >> 16);
			}
		}
	}

}