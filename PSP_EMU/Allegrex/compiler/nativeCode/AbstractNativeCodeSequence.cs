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
namespace pspsharp.Allegrex.compiler.nativeCode
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.alignDown;

	using Logger = org.apache.log4j.Logger;

	using Instruction = pspsharp.Allegrex.Common.Instruction;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public abstract class AbstractNativeCodeSequence : INativeCodeSequence
	{
		protected internal static Logger log = Emulator.log;
		protected internal static int[] toUpperCase = buildToUpperCase();
		protected internal static int[] toLowerCase = buildToLowerCase();

		internal static int[] buildToUpperCase()
		{
			int[] toUpperCase = new int[256];
			for (int c = 0; c < toUpperCase.Length; c++)
			{
				toUpperCase[c] = (c >= 0x61 && c <= 0x7A) ? c - 32 : c;
			}

			return toUpperCase;
		}

		internal static int[] buildToLowerCase()
		{
			int[] toLowerCase = new int[256];
			for (int c = 0; c < toLowerCase.Length; c++)
			{
				toLowerCase[c] = (c >= 0x41 && c <= 0x5A) ? c + 32 : c;
			}

			return toLowerCase;
		}

		protected internal static Processor Processor
		{
			get
			{
				return RuntimeContext.processor;
			}
		}

		protected internal static CpuState Cpu
		{
			get
			{
				return RuntimeContext.cpu;
			}
		}

		protected internal static Memory MemoryForLLE
		{
			get
			{
				Memory mem;
				if (RuntimeContextLLE.LLEActive)
				{
					mem = RuntimeContextLLE.MMIO;
				}
				else
				{
					mem = Memory;
				}
    
				return mem;
			}
		}

		protected internal static Memory Memory
		{
			get
			{
				return RuntimeContext.memory;
			}
		}

		protected internal static int Pc
		{
			get
			{
				return Cpu.pc;
			}
		}

		protected internal static int getRegisterValue(int register)
		{
			return Cpu.getRegister(register);
		}

		protected internal static long getLong(int low, int high)
		{
			return (((long) high) << 32) | (low & 0xFFFFFFFFL);
		}

		protected internal static int GprA0
		{
			get
			{
				return Cpu._a0;
			}
		}

		protected internal static int GprA1
		{
			get
			{
				return Cpu._a1;
			}
		}

		protected internal static int GprA2
		{
			get
			{
				return Cpu._a2;
			}
		}

		protected internal static int GprA3
		{
			get
			{
				return Cpu._a3;
			}
		}

		protected internal static int GprT0
		{
			get
			{
				return Cpu._t0;
			}
		}

		protected internal static int GprT1
		{
			get
			{
				return Cpu._t1;
			}
		}

		protected internal static int GprT2
		{
			get
			{
				return Cpu._t2;
			}
		}

		protected internal static int GprT3
		{
			get
			{
				return Cpu._t3;
			}
		}

		protected internal static int StackParam0
		{
			get
			{
				return Memory.read32(GprSp);
			}
		}

		protected internal static int StackParam1
		{
			get
			{
				return Memory.read32(GprSp + 4);
			}
		}

		protected internal static int StackParam2
		{
			get
			{
				return Memory.read32(GprSp + 8);
			}
		}

		protected internal static int GprSp
		{
			get
			{
				return Cpu._sp;
			}
		}

		protected internal static int GprV0
		{
			set
			{
				Cpu._v0 = value;
			}
		}

		protected internal static long GprV0V1
		{
			set
			{
				Cpu._v0 = (int) value;
				Cpu._v1 = (int)(value >> 32);
			}
		}

		protected internal static void setRegisterValue(int register, int value)
		{
			Cpu.setRegister(register, value);
		}

		internal static float[] Fpr
		{
			get
			{
				return Cpu.fpr;
			}
		}

		protected internal static float FprF12
		{
			get
			{
				return Fpr[12];
			}
		}

		protected internal static float FprF0
		{
			set
			{
				Fpr[0] = value;
			}
		}

		protected internal static float getFRegisterValue(int register)
		{
			return Fpr[register];
		}

		public static void strcpy(int dstAddr, int srcAddr)
		{
			int srcLength = getStrlen(srcAddr);
			Memory.memcpy(dstAddr, srcAddr, srcLength + 1);
		}

		public static int strcmp(int src1Addr, int src2Addr)
		{
			if (src1Addr == 0)
			{
				if (src2Addr == 0)
				{
					return 0;
				}
				return -1;
			}

			if (src2Addr == 0)
			{
				return 1;
			}

			IMemoryReader memoryReader1 = MemoryReader.getMemoryReader(src1Addr, 1);
			IMemoryReader memoryReader2 = MemoryReader.getMemoryReader(src2Addr, 1);

			if (memoryReader1 != null && memoryReader2 != null)
			{
				while (true)
				{
					int c1 = memoryReader1.readNext();
					int c2 = memoryReader2.readNext();
					if (c1 != c2)
					{
						return c1 > c2 ? 1 : -1;
					}
					else if (c1 == 0)
					{
						// c1 == 0 and c2 == 0
						break;
					}
				}
			}

			return 0;
		}

		public static int getStrlen(int srcAddr)
		{
			if (srcAddr == 0)
			{
				return 0;
			}

			int srcAddr3 = srcAddr & 3;
			// Reading 32-bit values is much faster
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(srcAddr - srcAddr3, 4);
			if (memoryReader == null)
			{
				Compiler.log.warn("getStrlen: null MemoryReader");
				return 0;
			}

			int value;
			int offset = 0;
			switch (srcAddr3)
			{
				case 1:
					value = memoryReader.readNext();
					if ((value & 0x0000FF00) == 0)
					{
						return 0;
					}
					if ((value & 0x00FF0000) == 0)
					{
						return 1;
					}
					if ((value & 0xFF000000) == 0)
					{
						return 2;
					}
					offset = 3;
					break;
				case 2:
					value = memoryReader.readNext();
					if ((value & 0x00FF0000) == 0)
					{
						return 0;
					}
					if ((value & 0xFF000000) == 0)
					{
						return 1;
					}
					offset = 2;
					break;
				case 3:
					value = memoryReader.readNext();
					if ((value & 0xFF000000) == 0)
					{
						return 0;
					}
					offset = 1;
					break;
			}

			// Read 32-bit values and check for a null-byte
			while (true)
			{
				value = memoryReader.readNext();
				if ((value & 0x000000FF) == 0)
				{
					return offset;
				}
				if ((value & 0x0000FF00) == 0)
				{
					return offset + 1;
				}
				if ((value & 0x00FF0000) == 0)
				{
					return offset + 2;
				}
				if ((value & 0xFF000000) == 0)
				{
					return offset + 3;
				}
				offset += 4;
			}
		}

		protected internal static int getStrlen(int srcAddr, int maxLength)
		{
			if (srcAddr == 0 || maxLength <= 0)
			{
				return 0;
			}

			int srcAddr3 = srcAddr & 3;
			// Reading 32-bit values is much faster
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(srcAddr - srcAddr3, 4);
			if (memoryReader == null)
			{
				Compiler.log.warn("getStrlen: null MemoryReader");
				return 0;
			}

			int value;
			int offset = 0;
			switch (srcAddr3)
			{
				case 1:
					value = memoryReader.readNext();
					if ((value & 0x0000FF00) == 0)
					{
						return 0;
					}
					if ((value & 0x00FF0000) == 0)
					{
						return 1;
					}
					if ((value & 0xFF000000) == 0)
					{
						return System.Math.Min(2, maxLength);
					}
					offset = 3;
					break;
				case 2:
					value = memoryReader.readNext();
					if ((value & 0x00FF0000) == 0)
					{
						return 0;
					}
					if ((value & 0xFF000000) == 0)
					{
						return 1;
					}
					offset = 2;
					break;
				case 3:
					value = memoryReader.readNext();
					if ((value & 0xFF000000) == 0)
					{
						return 0;
					}
					offset = 1;
					break;
			}

			// Read 32-bit values and check for a null-byte
			while (offset < maxLength)
			{
				value = memoryReader.readNext();
				if ((value & 0x000000FF) == 0)
				{
					return offset;
				}
				if ((value & 0x0000FF00) == 0)
				{
					return offset + 1;
				}
				if ((value & 0x00FF0000) == 0)
				{
					return System.Math.Min(offset + 2, maxLength);
				}
				if ((value & 0xFF000000) == 0)
				{
					return System.Math.Min(offset + 3, maxLength);
				}
				offset += 4;
			}

			return maxLength;
		}

		protected internal static int getRelocatedAddress(int address1, int address2)
		{
			int address = (address1 << 16) + (short) address2;
			return address & Memory.addressMask;
		}

		protected internal static void interpret(int opcode)
		{
			Instruction insn = Decoder.instruction(opcode);
			insn.interpret(RuntimeContext.processor, opcode);
		}

		protected internal static void invalidateCache(int address, int length)
		{
			address = Memory.normalize(address);
			int endAddress = address + length;
			const int invalidateSize = 64;
			address = alignDown(address, invalidateSize - 1);

			while (address < endAddress)
			{
				RuntimeContext.invalidateRange(address, invalidateSize);
				address += invalidateSize;
			}
		}
	}

}