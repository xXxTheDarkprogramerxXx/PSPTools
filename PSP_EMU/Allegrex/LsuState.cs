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
namespace pspsharp.Allegrex
{

	/// <summary>
	/// Load Store Unit, handles memory operations.
	/// 
	/// @author hli
	/// </summary>
	public class LsuState : MduState
	{

		public Memory memory = Memory.Instance;
		protected internal const bool CHECK_ALIGNMENT = true;

		public override void reset()
		{
		}

		public override void resetAll()
		{
			base.resetAll();
		}

		public LsuState()
		{
		}

		public virtual void copy(LsuState that)
		{
			base.copy(that);
		}

		public LsuState(LsuState that) : base(that)
		{
		}

		public virtual Memory Memory
		{
			set
			{
				this.memory = value;
			}
		}

		public virtual void doLB(int rt, int rs, int simm16)
		{
			int word = (sbyte)memory.read8(getRegister(rs) + simm16);
			if (rt != 0)
			{
				setRegister(rt, word);
			}
		}

		public virtual void doLBU(int rt, int rs, int simm16)
		{
			int word = memory.read8(getRegister(rs) + simm16) & 0xff;
			if (rt != 0)
			{
				setRegister(rt, word);
			}
		}

		public virtual void doLH(int rt, int rs, int simm16)
		{
			if (CHECK_ALIGNMENT)
			{
				CpuState cpu = Emulator.Processor.cpu;
				int address = getRegister(rs) + simm16;
				if ((address & 1) != 0)
				{
					Memory.log.error(string.Format("LH unaligned addr:0x{0:x8} pc:0x{1:x8}", address, cpu.pc));
				}
			}

			int word = (short)memory.read16(getRegister(rs) + simm16);
			if (rt != 0)
			{
				setRegister(rt, word);
			}
		}

		public virtual void doLHU(int rt, int rs, int simm16)
		{
			if (CHECK_ALIGNMENT)
			{
				CpuState cpu = Emulator.Processor.cpu;
				int address = getRegister(rs) + simm16;
				if ((address & 1) != 0)
				{
					Memory.log.error(string.Format("LHU unaligned addr:0x{0:x8} pc:0x{1:x8}", address, cpu.pc));
				}
			}

			int word = memory.read16(getRegister(rs) + simm16) & 0xffff;
			if (rt != 0)
			{
				setRegister(rt, word);
			}
		}

		private static readonly int[] lwlMask = new int[] {0xffffff, 0xffff, 0xff, 0};
		private static readonly int[] lwlShift = new int[] {24, 16, 8, 0};

		public virtual void doLWL(int rt, int rs, int simm16)
		{
			int address = getRegister(rs) + simm16;
			int offset = address & 0x3;
			int value = getRegister(rt);

			int data = memory.read32(address & unchecked((int)0xfffffffc));
			if (rt != 0)
			{
				setRegister(rt, (data << lwlShift[offset]) | (value & lwlMask[offset]));
			}
		}

		public virtual void doLW(int rt, int rs, int simm16)
		{
			if (CHECK_ALIGNMENT)
			{
				CpuState cpu = Emulator.Processor.cpu;
				int address = getRegister(rs) + simm16;
				if ((address & 3) != 0)
				{
					Memory.log.error(string.Format("LW unaligned addr:0x{0:x8} pc:0x{1:x8}", address, cpu.pc));
				}
			}

			int word = memory.read32(getRegister(rs) + simm16);
			if (rt != 0)
			{
				setRegister(rt, word);
			}
		}

		private static readonly int[] lwrMask = new int[] {0, unchecked((int)0xff000000), unchecked((int)0xffff0000), unchecked((int)0xffffff00)};
		private static readonly int[] lwrShift = new int[] {0, 8, 16, 24};

		public virtual void doLWR(int rt, int rs, int simm16)
		{
			int address = getRegister(rs) + simm16;
			int offset = address & 0x3;
			int value = getRegister(rt);

			int data = memory.read32(address & unchecked((int)0xfffffffc));
			if (rt != 0)
			{
				setRegister(rt, ((int)((uint)data >> lwrShift[offset])) | (value & lwrMask[offset]));
			}
		}

		public virtual void doSB(int rt, int rs, int simm16)
		{
			memory.write8(getRegister(rs) + simm16, unchecked((sbyte)(getRegister(rt) & 0xFF)));
		}

		public virtual void doSH(int rt, int rs, int simm16)
		{
			if (CHECK_ALIGNMENT)
			{
				CpuState cpu = Emulator.Processor.cpu;
				int address = getRegister(rs) + simm16;
				if ((address & 1) != 0)
				{
					Memory.log.error(string.Format("SH unaligned addr:0x{0:x8} pc:0x{1:x8}", address, cpu.pc));
				}
			}

			memory.write16(getRegister(rs) + simm16, unchecked((short)(getRegister(rt) & 0xFFFF)));
		}

		private static readonly int[] swlMask = new int[] {unchecked((int)0xffffff00), unchecked((int)0xffff0000), unchecked((int)0xff000000), 0};
		private static readonly int[] swlShift = new int[] {24, 16, 8, 0};

		public virtual void doSWL(int rt, int rs, int simm16)
		{
			int address = getRegister(rs) + simm16;
			int offset = address & 0x3;
			int value = getRegister(rt);
			int data = memory.read32(address & unchecked((int)0xfffffffc));

			data = ((int)((uint)value >> swlShift[offset])) | (data & swlMask[offset]);

			memory.write32(address & unchecked((int)0xfffffffc), data);
		}

		public virtual void doSW(int rt, int rs, int simm16)
		{
			if (CHECK_ALIGNMENT)
			{
				CpuState cpu = Emulator.Processor.cpu;
				int address = getRegister(rs) + simm16;
				if ((address & 3) != 0)
				{
					Memory.log.error(string.Format("SW unaligned addr:0x{0:x8} pc:0x{1:x8}", address, cpu.pc));
				}
			}

			memory.write32(getRegister(rs) + simm16, getRegister(rt));
		}

		private static readonly int[] swrMask = new int[] {0, 0xff, 0xffff, 0xffffff};
		private static readonly int[] swrShift = new int[] {0, 8, 16, 24};

		public virtual void doSWR(int rt, int rs, int simm16)
		{
			int address = getRegister(rs) + simm16;
			int offset = address & 0x3;
			int value = getRegister(rt);
			int data = memory.read32(address & unchecked((int)0xfffffffc));

			data = (value << swrShift[offset]) | (data & swrMask[offset]);

			memory.write32(address & unchecked((int)0xfffffffc), data);
		}

		public virtual void doLL(int rt, int rs, int simm16)
		{
			int word = memory.read32(getRegister(rs) + simm16);
			if (rt != 0)
			{
				setRegister(rt, word);
			}
			//ll_bit = 1;
		}

		public virtual void doSC(int rt, int rs, int simm16)
		{
			memory.write32(getRegister(rs) + simm16, getRegister(rt));
			if (rt != 0)
			{
				setRegister(rt, 1); // = ll_bit;
			}
		}
	}
}