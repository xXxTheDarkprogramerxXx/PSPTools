using System;

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
//	import static pspsharp.util.Utilities.round4;


	using Modules = pspsharp.HLE.Modules;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class FastMemory : Memory
	{
		//
		// In a typical application, the following read/write operations are performed:
		//   - read8  :  1,45% of total read/write,  1,54% of total read operations
		//   - read16 : 13,90% of total read/write, 14,76% of total read operations
		//   - read32 : 78,80% of total read/write, 83,70% of total read operations
		//   - write8 :  1,81% of total read/write, 30,96% of total write operations
		//   - write16:  0,02% of total read/write,  0,38% of total write operations
		//   - write32:  4,02% of total read/write, 68,67% of total write operations
		//
		// This is why this Memory implementation is optimized for fast read32 operations.
		// Drawback is the higher memory requirements.
		//
		// This implementation is performing very few checks for the validity of
		// memory address references to achieve the highest performance.
		// Use SafeFastMemory for complete address checks.
		//
		private int[] all;

		// Enable/disable read & write tracing.
		// Use final variables to not reduce the performance
		// (the code is removed/inserted at Java compile time)
		private const bool traceRead = false;
		private const bool traceWrite = false;

		// Array containing only 0, for fast memset(addr, 0, length);
		public static readonly int[] zero = new int[32768];

		public static readonly int[] memory8Shift = new int[] {0, 8, 16, 24};
		public static readonly int[] memory8Mask = new int[] {unchecked((int)0xFFFFFF00), unchecked((int)0xFFFF00FF), unchecked((int)0xFF00FFFF), 0x00FFFFFF};
		public static readonly int[] memory16Shift = new int[] {0, 0, 16, 16};
		public static readonly int[] memory16Mask = new int[] {unchecked((int)0xFFFF0000), unchecked((int)0xFFFF0000), 0x0000FFFF, 0x0000FFFF};
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private static readonly bool[] isIntAligned_Renamed = new bool[] {true, false, false, false};

		public override bool allocate()
		{
			// Free previously allocated memory
			all = null;

			int allSize = (MemoryMap.END_RAM + 1) >> 2;
			try
			{
				all = new int[allSize];
			}
			catch (System.OutOfMemoryException)
			{
				// Not enough memory provided for this VM, cannot use FastMemory model
				Memory.log.warn("Cannot allocate FastMemory: add the option '-Xmx256m' to the Java Virtual Machine startup command to improve Performance");
				Memory.log.info("The current Java Virtual Machine has been started using '-Xmx" + (Runtime.Runtime.maxMemory() / (1024 * 1024)) + "m'");
				return false;
			}

			return base.allocate();
		}

		public override void Initialise()
		{
			Arrays.fill(zero, 0);
			Arrays.fill(all, 0);
		}

		public override int read8(int address)
		{
			address &= addressMask;
			int data = (all[address >> 2] >> memory8Shift[address & 0x03]) & 0xFF;

			if (traceRead)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("read8(0x{0:X8})=0x{1:X2}", address, data));
				}
			}

			return data;
		}

		public override int read16(int address)
		{
			address &= addressMask;
			int data = (all[address >> 2] >> memory16Shift[address & 0x02]) & 0xFFFF;

			if (traceRead)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("read16(0x{0:X8})=0x{1:X4}", address, data));
				}
			}

			return data;
		}

		public override int read32(int address)
		{
			try
			{
				address &= addressMask;
				int data = all[address >> 2];

				if (traceRead)
				{
					if (log.TraceEnabled)
					{
						log.trace(string.Format("read32(0x{0:X8})=0x{1:X8} ({2:F})", address, data, Float.intBitsToFloat(data)));
					}
				}

				return data;
			}
			catch (System.IndexOutOfRangeException)
			{
				if (read32AllowedInvalidAddress(address))
				{
					return 0;
				}

				invalidMemoryAddress(address, "read32", Emulator.EMU_STATUS_MEM_READ);
				return 0;
			}
		}

		public override long read64(int address)
		{
			address &= addressMask;
			long data = (((long) all[(address >> 2) + 1]) << 32) | (((long) all[address >> 2]) & 0xFFFFFFFFL);

			if (traceRead)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("read64(0x{0:X8})=0x{1:X}", address, data));
				}
			}

			return data;
		}

		public override void write8(int address, sbyte data)
		{
			address &= addressMask;
			int index = address & 0x03;
			int memData = (all[address >> 2] & memory8Mask[index]) | ((data & 0xFF) << memory8Shift[index]);

			if (traceWrite)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("write8(0x{0:X8}, 0x{1:X2})", address, data & 0xFF));
				}
			}

			all[address >> 2] = memData;
			Modules.sceDisplayModule.write8(address);
		}

		public override void write16(int address, short data)
		{
			address &= addressMask;
			int index = address & 0x02;
			int memData = (all[address >> 2] & memory16Mask[index]) | ((data & 0xFFFF) << memory16Shift[index]);

			if (traceWrite)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("write16(0x{0:X8}, 0x{1:X4})", address, data & 0xFFFF));
				}
			}

			all[address >> 2] = memData;
			Modules.sceDisplayModule.write16(address);
		}

		public override void write32(int address, int data)
		{
			address &= addressMask;

			if (traceWrite)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("write32(0x{0:X8}, {1:X8} ({2:F}))", address, data, Float.intBitsToFloat(data)));
				}
			}

			all[address >> 2] = data;
			Modules.sceDisplayModule.write32(address);
		}

		public override void write64(int address, long data)
		{
			address &= addressMask;

			if (traceWrite)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("write64(0x{0:X8}, 0x{1:X}", address, data));
				}
			}

			all[address >> 2] = (int) data;
			all[(address >> 2) + 1] = (int)(data >> 32);
		}

		public override IntBuffer MainMemoryByteBuffer
		{
			get
			{
				return IntBuffer.wrap(all, MemoryMap.START_RAM >> 2, MemoryMap.SIZE_RAM >> 2);
			}
		}

		public override IntBuffer getBuffer(int address, int length)
		{
			address = normalizeAddress(address);

			IntBuffer buffer = MainMemoryByteBuffer;
			buffer.position(address >> 2);
			buffer.limit(round4(round4(address) + length) >> 2);

			return buffer.slice();
		}

		private static bool isIntAligned(int n)
		{
			return isIntAligned_Renamed[n & 0x03];
		}

		public override void memset(int address, sbyte data, int length)
		{
			address = normalizeAddress(address);

			Modules.sceDisplayModule.write(address);

			for (; !isIntAligned(address) && length > 0; address++, length--)
			{
				write8(address, data);
			}

			int count4 = length >> 2;
			if (count4 > 0)
			{
				if (data == 0)
				{
					// Fast memset(addr, 0, length) using copy from "zero" array
					for (int i = 0; i < count4; i += zero.Length)
					{
						Array.Copy(zero, 0, all, (address >> 2) + i, System.Math.Min(zero.Length, count4 - i));
					}
				}
				else
				{
					int data1 = data & 0xFF;
					int data4 = (data1 << 24) | (data1 << 16) | (data1 << 8) | data1;
					Arrays.fill(all, address >> 2, (address >> 2) + count4, data4);
				}
				address += count4 << 2;
				length -= count4 << 2;
			}

			for (; length > 0; address++, length--)
			{
				write8(address, data);
			}
		}

		public override void copyToMemory(int address, ByteBuffer source, int length)
		{
			// copy in 1 byte steps until address is "int"-aligned
			while (!isIntAligned(address) && length > 0 && source.hasRemaining())
			{
				sbyte b = source.get();
				write8(address, b);
				address++;
				length--;
			}

			// copy 1 int at each loop
			int countInt = System.Math.Min(length, source.remaining()) >> 2;
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(address, countInt << 2, 4);
			for (int i = 0; i < countInt; i++)
			{
				int data1 = source.get() & 0xFF;
				int data2 = source.get() & 0xFF;
				int data3 = source.get() & 0xFF;
				int data4 = source.get() & 0xFF;
				int data = (data4 << 24) | (data3 << 16) | (data2 << 8) | data1;
				memoryWriter.writeNext(data);
			}
			memoryWriter.flush();
			int copyLength = countInt << 2;
			length -= copyLength;
			address += copyLength;

			// copy rest length in 1 byte steps (rest length <= 3)
			while (length > 0 && source.hasRemaining())
			{
				sbyte b = source.get();
				write8(address, b);
				address++;
				length--;
			}
		}

		public virtual int[] All
		{
			get
			{
				return all;
			}
		}

		// Source, destination and length are "int"-aligned
		private void memcpyAligned4(int destination, int source, int length, bool checkOverlap)
		{
			if (checkOverlap || !areOverlapping(destination, source, length))
			{
				// Direct copy, System.arraycopy is handling correctly overlapping arrays
				Array.Copy(all, source >> 2, all, destination >> 2, length >> 2);
			}
			else
			{
				// Buffers are overlapping, but we have to copy as they would not overlap.
				// Unfortunately, IntBuffer operation are always checking for overlapping buffers,
				// so we have to copy manually...
				int src = source >> 2;
				int dst = destination >> 2;
				for (int i = 0; i < length; i += 4)
				{
					all[dst++] = all[src++];
				}
			}
		}

		protected internal override void memcpy(int destination, int source, int length, bool checkOverlap)
		{
			if (length <= 0)
			{
				return;
			}

			destination = normalizeAddress(destination);
			source = normalizeAddress(source);

			Modules.sceDisplayModule.write(destination);

			if (isIntAligned(source) && isIntAligned(destination) && isIntAligned(length))
			{
				// Source, destination and length are "int"-aligned
				memcpyAligned4(destination, source, length, checkOverlap);
			}
			else if ((source & 0x03) == (destination & 0x03) && (!checkOverlap || !areOverlapping(destination, source, length)))
			{
				// Source and destination have the same alignment and are not overlapping
				while (!isIntAligned(source) && length > 0)
				{
					write8(destination, (sbyte) read8(source));
					source++;
					destination++;
					length--;
				}

				int length4 = length & ~0x03;
				if (length4 > 0)
				{
					memcpyAligned4(destination, source, length4, checkOverlap);
					source += length4;
					destination += length4;
					length -= length4;
				}

				while (length > 0)
				{
					write8(destination, (sbyte) read8(source));
					destination++;
					source++;
					length--;
				}
			}
			else
			{
				//
				// Buffers are not "int"-aligned, copy in 1 byte steps.
				// Overlapping address ranges must be correctly handled:
				//   If source >= destination:
				//                 [---source---]
				//       [---destination---]
				//      => Copy from the head
				//   If source < destination:
				//       [---source---]
				//                 [---destination---]
				//      => Copy from the tail
				//
				if (!checkOverlap || source >= destination || !areOverlapping(destination, source, length))
				{
					if (areOverlapping(destination, source, 4))
					{
						// Cannot use MemoryReader if source and destination are overlapping in less than 4 bytes
						for (int i = 0; i < length; i++)
						{
							write8(destination + i, (sbyte) read8(source + i));
						}
					}
					else
					{
						IMemoryReader sourceReader = MemoryReader.getMemoryReader(source, length, 1);
						IMemoryWriter destinationWriter = MemoryWriter.getMemoryWriter(destination, length, 1);
						for (int i = 0; i < length; i++)
						{
							destinationWriter.writeNext(sourceReader.readNext());
						}
						destinationWriter.flush();
					}
				}
				else
				{
					for (int i = length - 1; i >= 0; i--)
					{
						write8(destination + i, (sbyte) read8(source + i));
					}
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected void read(pspsharp.state.StateInputStream stream, int address, int length) throws java.io.IOException
		protected internal override void read(StateInputStream stream, int address, int length)
		{
			stream.readInts(all, address >> 2, length >> 2);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected void write(pspsharp.state.StateOutputStream stream, int address, int length) throws java.io.IOException
		protected internal override void write(StateOutputStream stream, int address, int length)
		{
			stream.writeInts(all, address >> 2, length >> 2);
		}
	}

}