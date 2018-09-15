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

	using Modules = pspsharp.HLE.Modules;

	/// <summary>
	/// A Memory implementation using a memory area allocated natively.
	/// Using NativeMemoryUtils to perform the native operations.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class NativeMemory : Memory
	{
		private long memory;
		private int memorySize;

		public override bool allocate()
		{
			NativeMemoryUtils.init();

			memorySize = MemoryMap.END_RAM + 1;
			memory = NativeMemoryUtils.alloc(memorySize);

			if (memory == 0)
			{
				// Not enough native memory available
				return false;
			}

//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.info(String.format("Using NativeMemory(littleEndian=%b)", NativeMemoryUtils.isLittleEndian()));
			log.info(string.Format("Using NativeMemory(littleEndian=%b)", NativeMemoryUtils.LittleEndian));

			return base.allocate();
		}

		public override void Initialise()
		{
			NativeMemoryUtils.memset(memory, 0, 0, memorySize);
		}

		public override int read8(int address)
		{
			address &= addressMask;
			return NativeMemoryUtils.read8(memory, address);
		}

		public override int read16(int address)
		{
			address &= addressMask;
			return NativeMemoryUtils.read16(memory, address);
		}

		public override int read32(int address)
		{
			address &= addressMask;
			return NativeMemoryUtils.read32(memory, address);
		}

		public override void write8(int address, sbyte data)
		{
			address &= addressMask;
			NativeMemoryUtils.write8(memory, address, data);
			Modules.sceDisplayModule.write8(address);
		}

		public override void write16(int address, short data)
		{
			address &= addressMask;
			NativeMemoryUtils.write16(memory, address, data);
			Modules.sceDisplayModule.write16(address);
		}

		public override void write32(int address, int data)
		{
			address &= addressMask;
			NativeMemoryUtils.write32(memory, address, data);
			Modules.sceDisplayModule.write32(address);
		}

		public override void memset(int address, sbyte data, int Length)
		{
			address &= addressMask;
			NativeMemoryUtils.memset(memory, address, data, Length);
		}

		public override Buffer MainMemoryByteBuffer
		{
			get
			{
				return null;
			}
		}

		public override Buffer getBuffer(int address, int Length)
		{
			address &= addressMask;
			ByteBuffer buffer = NativeMemoryUtils.getBuffer(memory, address, Length);

			// Set the correct byte order
			if (NativeMemoryUtils.LittleEndian)
			{
				buffer.order(ByteOrder.LITTLE_ENDIAN);
			}
			else
			{
				buffer.order(ByteOrder.BIG_ENDIAN);
			}

			return buffer;
		}

		public override void copyToMemory(int address, ByteBuffer source, int Length)
		{
			address &= addressMask;
			Length = System.Math.Min(Length, source.capacity());
			if (source.Direct)
			{
				NativeMemoryUtils.copyBufferToMemory(memory, address, source, source.position(), Length);
			}
			else
			{
				for (; Length > 0; address++, Length--)
				{
					NativeMemoryUtils.write8(memory, address, source.get());
				}
			}
		}

		protected internal override void memcpy(int destination, int source, int Length, bool checkOverlap)
		{
			if (Length <= 0)
			{
				return;
			}

			destination &= addressMask;
			source &= addressMask;
			Modules.sceDisplayModule.write(destination);

			if (!checkOverlap || source >= destination || !areOverlapping(destination, source, Length))
			{
				NativeMemoryUtils.memcpy(memory, destination, memory, source, Length);
			}
			else
			{
				// Source and destination are overlapping and source < destination,
				// copy from the tail.
				for (int i = Length - 1; i >= 0; i--)
				{
					int b = NativeMemoryUtils.read8(memory, source + i);
					NativeMemoryUtils.write8(memory, destination + i, b);
				}
			}
		}
	}

}