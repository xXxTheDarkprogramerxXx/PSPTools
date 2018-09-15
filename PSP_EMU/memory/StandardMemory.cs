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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.MemoryMap.SIZE_RAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.MemoryMap.SIZE_SCRATCHPAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.MemoryMap.SIZE_VRAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.MemoryMap.START_RAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.MemoryMap.START_SCRATCHPAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.MemoryMap.START_VRAM;


	using Modules = pspsharp.HLE.Modules;

	public class StandardMemory : Memory
	{
		private const int PAGE_COUNT = 0x00100000;
		private const int PAGE_MASK = 0x00000FFF;
		private const int PAGE_SHIFT = 12;

		private const int INDEX_SCRATCHPAD = 0;
		private static readonly int INDEX_VRAM = (int)((uint)SIZE_SCRATCHPAD >> PAGE_SHIFT);
		private static readonly int INDEX_RAM = INDEX_VRAM + ((int)((uint)SIZE_VRAM >> PAGE_SHIFT));

		private static int SIZE_ALLMEM;

		private sbyte[] all; // all psp memory is held in here
		private static int[] map; // hold map of memory
		private ByteBuffer buf; // for easier memory reads/writes

		private ByteBuffer scratchpad;
		private ByteBuffer videoram;
		private ByteBuffer mainmemory;

		public override bool allocate()
		{
			// This value can change depending on the PSP memory configuration (32MB/64MB)
			SIZE_ALLMEM = SIZE_SCRATCHPAD + SIZE_VRAM + SIZE_RAM;
			// Free previously allocated memory
			all = null;
			map = null;

			try
			{
				all = new sbyte[SIZE_ALLMEM];
				map = new int[PAGE_COUNT];
				buf = ByteBuffer.wrap(all);
				buf.order(ByteOrder.LITTLE_ENDIAN);

				scratchpad = ByteBuffer.wrap(all, 0, SIZE_SCRATCHPAD).slice();
				scratchpad.order(ByteOrder.LITTLE_ENDIAN);

				videoram = ByteBuffer.wrap(all, SIZE_SCRATCHPAD, SIZE_VRAM).slice();
				videoram.order(ByteOrder.LITTLE_ENDIAN);

				mainmemory = ByteBuffer.wrap(all, SIZE_SCRATCHPAD + SIZE_VRAM, SIZE_RAM).slice();
				mainmemory.order(ByteOrder.LITTLE_ENDIAN);

				buildMap();
			}
			catch (System.OutOfMemoryException)
			{
				// Not enough memory provided for this VM, cannot use StandardMemory model
				Memory.Console.WriteLine("Cannot allocate StandardMemory: add the option '-Xmx64m' to the Java Virtual Machine startup command to improve Performance");
				return false;
			}

			return base.allocate();
		}

		public override void Initialise()
		{
			Arrays.Fill(all, (sbyte)0);
		}

		public StandardMemory()
		{
		}

		private void buildMap()
		{
			int i;
			int page;

			Arrays.Fill(map, -1);

			page = (int)((uint)START_SCRATCHPAD >> PAGE_SHIFT);
			for (i = 0; i < ((int)((uint)SIZE_SCRATCHPAD >> PAGE_SHIFT)); ++i)
			{
				map[0x00000 + page + i] = (INDEX_SCRATCHPAD + i) << PAGE_SHIFT;
				map[0x40000 + page + i] = (INDEX_SCRATCHPAD + i) << PAGE_SHIFT;
				map[0x80000 + page + i] = (INDEX_SCRATCHPAD + i) << PAGE_SHIFT;
			}

			page = (int)((uint)START_VRAM >> PAGE_SHIFT);
			for (i = 0; i < ((int)((uint)SIZE_VRAM >> PAGE_SHIFT)); ++i)
			{
				map[0x00000 + page + i] = (INDEX_VRAM + i) << PAGE_SHIFT;
				map[0x40000 + page + i] = (INDEX_VRAM + i) << PAGE_SHIFT;
				map[0x80000 + page + i] = (INDEX_VRAM + i) << PAGE_SHIFT;
				// Test on a PSP: 0x4200000 is equivalent to 0x4000000
				map[0x00000 + 0x200 + page + i] = (INDEX_VRAM + i) << PAGE_SHIFT;
				map[0x40000 + 0x200 + page + i] = (INDEX_VRAM + i) << PAGE_SHIFT;
				map[0x80000 + 0x200 + page + i] = (INDEX_VRAM + i) << PAGE_SHIFT;
			}

			page = (int)((uint)START_RAM >> PAGE_SHIFT);
			for (i = 0; i < ((int)((uint)SIZE_RAM >> PAGE_SHIFT)); ++i)
			{
				map[0x00000 + page + i] = (INDEX_RAM + i) << PAGE_SHIFT;
				map[0x40000 + page + i] = (INDEX_RAM + i) << PAGE_SHIFT;
				map[0x80000 + page + i] = (INDEX_RAM + i) << PAGE_SHIFT;
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static int indexFromAddr(int address) throws Exception
		private static int indexFromAddr(int address)
		{
			int index = map[(int)((uint)address >> PAGE_SHIFT)];
			if (index == -1)
			{
				throw new Exception("Invalid memory address : " + address.ToString("x") + " PC=" + (Emulator.Processor.cpu.pc).ToString("x"));
			}
			return index;
		}

		public override int read8(int address)
		{
			try
			{
				int page = indexFromAddr(address);
				return buf.get(page + (address & PAGE_MASK)) & 0xFF;
			}
			catch (Exception e)
			{
				Memory.Console.WriteLine("read8 - " + e.Message);
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_MEM_READ);
				return 0;
			}
		}

		public override int read16(int address)
		{
			try
			{
				int page = indexFromAddr(address);
				return buf.getShort(page + (address & PAGE_MASK)) & 0xFFFF;
			}
			catch (Exception e)
			{
				Memory.Console.WriteLine("read16 - " + e.Message);
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_MEM_READ);
				return 0;
			}
		}

		public override int read32(int address)
		{
			try
			{
				int page = indexFromAddr(address);
				return buf.getInt(page + (address & PAGE_MASK));
			}
			catch (Exception e)
			{
				if (read32AllowedInvalidAddress(address))
				{
					return 0;
				}

				Memory.Console.WriteLine("read32 - " + e.Message);
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_MEM_READ);
				return 0;
			}
		}

		public override long read64(int address)
		{
			try
			{
				int page = indexFromAddr(address);
				return buf.getLong(page + (address & PAGE_MASK));
			}
			catch (Exception e)
			{
				Memory.Console.WriteLine("read64 - " + e.Message);
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_MEM_READ);
				return 0;
			}
		}

		public override void write8(int address, sbyte data)
		{
			try
			{
				int page = indexFromAddr(address);
				buf.put(page + (address & PAGE_MASK), data);
				Modules.sceDisplayModule.write8(address & addressMask);
			}
			catch (Exception e)
			{
				Memory.Console.WriteLine("write8 - " + e.Message);
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_MEM_WRITE);
			}
		}

		public override void write16(int address, short data)
		{
			try
			{
				int page = indexFromAddr(address);
				buf.putShort(page + (address & PAGE_MASK), data);
				Modules.sceDisplayModule.write16(address & addressMask);
			}
			catch (Exception e)
			{
				Memory.Console.WriteLine("write16 - " + e.Message);
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_MEM_WRITE);
			}
		}

		public override void write32(int address, int data)
		{
			try
			{
				int page = indexFromAddr(address);
				buf.putInt(page + (address & PAGE_MASK), data);
				Modules.sceDisplayModule.write32(address & addressMask);
			}
			catch (Exception e)
			{
				Memory.Console.WriteLine("write32 - " + e.Message);
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_MEM_WRITE);
			}
		}

		public override void write64(int address, long data)
		{
			try
			{
				int page = indexFromAddr(address);
				buf.putLong(page + (address & PAGE_MASK), data);
				//Modules.sceDisplayModule.write64(address, data);
			}
			catch (Exception e)
			{
				Memory.Console.WriteLine("write64 - " + e.Message);
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_MEM_WRITE);
			}
		}

		public override ByteBuffer MainMemoryByteBuffer
		{
			get
			{
				return mainmemory;
			}
		}

		public override ByteBuffer getBuffer(int address, int Length)
		{
			address = normalizeAddress(address);

			int endAddress = address + Length - 1;
			if (address >= MemoryMap.START_RAM && endAddress <= MemoryMap.END_RAM)
			{
				return ByteBuffer.wrap(mainmemory.array(), mainmemory.arrayOffset() + address - MemoryMap.START_RAM, Length).slice().order(ByteOrder.LITTLE_ENDIAN);
			}
			else if (address >= MemoryMap.START_VRAM && endAddress <= MemoryMap.END_VRAM)
			{
				return ByteBuffer.wrap(videoram.array(), videoram.arrayOffset() + address - MemoryMap.START_VRAM, Length).slice().order(ByteOrder.LITTLE_ENDIAN);
			}
			else if (address >= MemoryMap.START_SCRATCHPAD && endAddress <= MemoryMap.END_SCRATCHPAD)
			{
				return ByteBuffer.wrap(scratchpad.array(), scratchpad.arrayOffset() + address - MemoryMap.START_SCRATCHPAD, Length).slice().order(ByteOrder.LITTLE_ENDIAN);
			}

			return null;
		}

		public override void memset(int address, sbyte data, int Length)
		{
			ByteBuffer buffer = getBuffer(address, Length);
			Arrays.Fill(buffer.array(), buffer.arrayOffset(), buffer.arrayOffset() + Length, data);
		}

		public override void copyToMemory(int address, ByteBuffer source, int Length)
		{
			sbyte[] data = new sbyte[Length];
			source.get(data);
			ByteBuffer destination = getBuffer(address, Length);
			destination.put(data);
		}

		protected internal override void memcpy(int destination, int source, int Length, bool checkOverlap)
		{
			destination = normalizeAddress(destination);
			source = normalizeAddress(source);

			if (checkOverlap || !areOverlapping(destination, source, Length))
			{
				// Direct copy if buffers do not overlap.
				// ByteBuffer operations are handling correctly overlapping buffers.
				ByteBuffer destinationBuffer = getBuffer(destination, Length);
				ByteBuffer sourceBuffer = getBuffer(source, Length);
				destinationBuffer.put(sourceBuffer);
			}
			else
			{
				// Buffers are overlapping and we have to copy them as they would not overlap.
				IMemoryReader sourceReader = MemoryReader.getMemoryReader(source, Length, 1);
				for (int i = 0; i < Length; i++)
				{
					write8(destination + i, (sbyte) sourceReader.readNext());
				}
			}
		}
	}

}