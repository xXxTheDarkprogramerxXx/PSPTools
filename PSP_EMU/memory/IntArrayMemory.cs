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
//	import static pspsharp.memory.FastMemory.memory16Mask;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.FastMemory.memory16Shift;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.FastMemory.memory8Mask;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.FastMemory.memory8Shift;

	using TPointer = pspsharp.HLE.TPointer;

	public class IntArrayMemory : Memory
	{
		private int[] memory;
		private int offset;

		public IntArrayMemory(int[] memory)
		{
			this.memory = memory;
			offset = 0;
		}

		public IntArrayMemory(int[] memory, int offset)
		{
			this.memory = memory;
			this.offset = offset;
		}

		public virtual TPointer getPointer(int address)
		{
			return (new TPointer(this, address)).forceNonNull();
		}

		public virtual TPointer Pointer
		{
			get
			{
				return getPointer(0);
			}
		}

		public override void Initialise()
		{
		}

		private int getOffset(int address)
		{
			return (address >> 2) + offset;
		}

		public override int read32(int address)
		{
			return memory[getOffset(address)];
		}

		public override int read16(int address)
		{
			int data = (memory[getOffset(address)] >> memory16Shift[address & 0x02]) & 0xFFFF;
			return data;
		}

		public override int read8(int address)
		{
			int data = (memory[getOffset(address)] >> memory8Shift[address & 0x03]) & 0xFF;
			return data;
		}

		public override void write32(int address, int value)
		{
			memory[getOffset(address)] = value;
		}

		public override void write16(int address, short value)
		{
			int index = address & 0x02;
			int memData = (memory[getOffset(address)] & memory16Mask[index]) | ((value & 0xFFFF) << memory16Shift[index]);
			memory[getOffset(address)] = memData;
		}

		public override void write8(int address, sbyte value)
		{
			int index = address & 0x03;
			int memData = (memory[getOffset(address)] & memory8Mask[index]) | ((value & 0xFF) << memory8Shift[index]);
			memory[getOffset(address)] = memData;
		}

		public override void memset(int address, sbyte data, int length)
		{
			for (int i = 0; i < length; i++)
			{
				write8(address + i, data);
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
			return null;
		}

		public override void copyToMemory(int address, ByteBuffer source, int length)
		{
			log.error(string.Format("Unimplemented copyToMemory address=0x{0:X8}, source={1}, length=0x{2:X}", address, source, length));
		}

		protected internal override void memcpy(int destination, int source, int length, bool checkOverlap)
		{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.error(String.format("Unimplemented memcpy destination=0x%08X, source=0x%08X, length=0x%X, checkOverlap=%b", destination, source, length, checkOverlap));
			log.error(string.Format("Unimplemented memcpy destination=0x%08X, source=0x%08X, length=0x%X, checkOverlap=%b", destination, source, length, checkOverlap));
		}
	}

}