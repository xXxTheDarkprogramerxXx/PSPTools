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
	/// A Memory implementation using multiple memory areas allocated natively.
	/// Using NativeMemoryUtils to perform the native operations.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class SparseNativeMemory : Memory
	{
		private bool InstanceFieldsInitialized = false;

		public SparseNativeMemory()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			pageSize = 1 << pageShift;
			pageMask = pageSize - 1;
		}

		private long[] memory;
		private int memorySize;
		// We need to have the PSP RAM residing into one single page (for getBuffer).
		private int pageShift = 26;
		private int pageSize;
		private int pageMask;

		public override bool allocate()
		{
			NativeMemoryUtils.init();

			memorySize = MemoryMap.END_RAM + 1;
			memory = new long[System.Math.Max((memorySize + pageSize - 1) >> pageShift, 1)];
			for (int i = 0; i < memory.Length; i++)
			{
				memory[i] = NativeMemoryUtils.alloc(pageSize);
				if (memory[i] == 0)
				{
					// Not enough native memory available
					for (int j = 0; j < i; j++)
					{
						NativeMemoryUtils.free(memory[j]);
					}
					return false;
				}
			}

//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.info(String.format("Using SparseNativeMemory(littleEndian=%b)", NativeMemoryUtils.isLittleEndian()));
			log.info(string.Format("Using SparseNativeMemory(littleEndian=%b)", NativeMemoryUtils.LittleEndian));

			return base.allocate();
		}

		public override void Initialise()
		{
			for (int i = 0; i < memory.Length; i++)
			{
				NativeMemoryUtils.memset(memory[i], 0, 0, pageSize);
			}
		}

		public override int read8(int address)
		{
			address &= addressMask;
			return NativeMemoryUtils.read8(memory[address >> pageShift], address & pageMask);
		}

		public override int read16(int address)
		{
			address &= addressMask;
			return NativeMemoryUtils.read16(memory[address >> pageShift], address & pageMask);
		}

		public override int read32(int address)
		{
			address &= addressMask;
			return NativeMemoryUtils.read32(memory[address >> pageShift], address & pageMask);
		}

		public override void write8(int address, sbyte data)
		{
			address &= addressMask;
			NativeMemoryUtils.write8(memory[address >> pageShift], address & pageMask, data);
			Modules.sceDisplayModule.write8(address);
		}

		public override void write16(int address, short data)
		{
			address &= addressMask;
			NativeMemoryUtils.write16(memory[address >> pageShift], address & pageMask, data);
			Modules.sceDisplayModule.write16(address);
		}

		public override void write32(int address, int data)
		{
			address &= addressMask;
			NativeMemoryUtils.write32(memory[address >> pageShift], address & pageMask, data);
			Modules.sceDisplayModule.write32(address);
		}

		public override void memset(int address, sbyte data, int length)
		{
			address &= addressMask;
			while (length > 0)
			{
				int pageLength = System.Math.Min(pageSize - (address & pageMask), length);
				NativeMemoryUtils.memset(memory[address >> pageShift], address & pageMask, data, pageLength);
				length -= pageLength;
				address += pageLength;
			}
		}

		public override Buffer MainMemoryByteBuffer
		{
			get
			{
				return null;
			}
		}

		public override Buffer getBuffer(int address, int length)
		{
			address &= addressMask;
			ByteBuffer buffer = NativeMemoryUtils.getBuffer(memory[address >> pageShift], address & pageMask, length);

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

		public override void copyToMemory(int address, ByteBuffer source, int length)
		{
			address &= addressMask;
			length = System.Math.Min(length, source.capacity());
			if (source.Direct)
			{
				NativeMemoryUtils.copyBufferToMemory(memory[address >> pageShift], address & pageMask, source, source.position(), length);
			}
			else
			{
				for (; length > 0; address++, length--)
				{
					NativeMemoryUtils.write8(memory[address >> pageShift], address & pageMask, source.get());
				}
			}
		}

		protected internal override void memcpy(int destination, int source, int length, bool checkOverlap)
		{
			if (length <= 0)
			{
				return;
			}

			destination &= addressMask;
			source &= addressMask;
			Modules.sceDisplayModule.write(destination);

			if (!checkOverlap || source >= destination || !areOverlapping(destination, source, length))
			{
				while (length > 0)
				{
					int pageLengthDestination = System.Math.Min(pageSize - (destination & pageMask), length);
					int pageLengthSource = System.Math.Min(pageSize - (source & pageMask), length);
					int pageLength = System.Math.Min(pageLengthDestination, pageLengthSource);
					NativeMemoryUtils.memcpy(memory[destination >> pageShift], destination & pageMask, memory[source >> pageShift], source & pageMask, pageLength);
					length -= pageLength;
					destination += pageLength;
					source += pageLength;
				}
			}
			else
			{
				// Source and destination are overlapping and source < destination,
				// copy from the tail.
				for (int i = length - 1; i >= 0; i--)
				{
					int b = read8(source + i);
					write8(destination + i, (sbyte) b);
				}
			}
		}

	}

}