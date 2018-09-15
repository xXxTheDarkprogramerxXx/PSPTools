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
namespace pspsharp.memory.mmio
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.FastMemory.memory16Mask;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.FastMemory.memory16Shift;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.FastMemory.memory8Mask;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.FastMemory.memory8Shift;

	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerReadWrite : MMIOHandlerBase
	{
		private const int STATE_VERSION = 0;
		private readonly int[] memory;

		public MMIOHandlerReadWrite(int baseAddress, int Length) : base(baseAddress)
		{

			memory = new int[Length >> 2];
		}

		public MMIOHandlerReadWrite(int baseAddress, int Length, int[] memory) : base(baseAddress)
		{

			this.memory = memory;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			stream.readInts(memory);
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInts(memory);
			base.write(stream);
		}

		public virtual int[] InternalMemory
		{
			get
			{
				return memory;
			}
		}

		public override int read32(int address)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read32(0x{1:X8})=0x{2:X8}", Pc, address, memory[(address - baseAddress) >> 2]));
			}

			return memory[(address - baseAddress) >> 2];
		}

		public override int read16(int address)
		{
			int data = (memory[(address - baseAddress) >> 2] >> memory16Shift[address & 0x02]) & 0xFFFF;
			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read16(0x{1:X8})=0x{2:X4}", Pc, address, data));
			}

			return data;
		}

		public override int read8(int address)
		{
			int data = (memory[(address - baseAddress) >> 2] >> memory8Shift[address & 0x03]) & 0xFF;
			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read8(0x{1:X8})=0x{2:X2}", Pc, address, data));
			}

			return data;
		}

		public override void write32(int address, int value)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write32(0x{1:X8}, 0x{2:X8})", Pc, address, value));
			}

			memory[(address - baseAddress) >> 2] = value;
		}

		public override void write16(int address, short value)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write16(0x{1:X8}, 0x{2:X4})", Pc, address, value & 0xFFFF));
			}

			int index = address & 0x02;
			int memData = (memory[(address - baseAddress) >> 2] & memory16Mask[index]) | ((value & 0xFFFF) << memory16Shift[index]);

			memory[(address - baseAddress) >> 2] = memData;
		}

		public override void write8(int address, sbyte value)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write8(0x{1:X8}, 0x{2:X2})", Pc, address, value & 0xFF));
			}

			int index = address & 0x03;
			int memData = (memory[(address - baseAddress) >> 2] & memory8Mask[index]) | ((value & 0xFF) << memory8Shift[index]);

			memory[(address - baseAddress) >> 2] = memData;
		}
	}

}