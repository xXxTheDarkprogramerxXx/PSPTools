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

	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	/// <summary>
	/// Branch Control Unit, handles branching and jumping operations
	/// 
	/// @author hli
	/// 
	/// </summary>
	public class BcuState : LsuState
	{
		private const int STATE_VERSION = 0;
		public int pc;
		public int npc;

		public override void reset()
		{
			pc = 0;
			npc = 0;
		}

		public override void resetAll()
		{
			base.resetAll();
			pc = 0;
			npc = 0;
		}

		public BcuState()
		{
			pc = 0;
			npc = 0;
		}

		public virtual void copy(BcuState that)
		{
			base.copy(that);
			pc = that.pc;
			npc = that.npc;
		}

		public BcuState(BcuState that) : base(that)
		{
			pc = that.pc;
			npc = that.npc;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			pc = stream.readInt();
			npc = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(pc);
			stream.writeInt(npc);
			base.write(stream);
		}

		public static int branchTarget(int npc, int simm16)
		{
			return npc + (simm16 << 2);
		}

		public static int jumpTarget(int npc, int uimm26)
		{
			return (npc & unchecked((int)0xf0000000)) | (uimm26 << 2);
		}

		public virtual int fetchOpcode()
		{
			npc = pc + 4;

			int opcode = memory.read32(pc);

			// by default, the next instruction to emulate is at the next address
			pc = npc;

			return opcode;
		}

		public virtual int nextOpcode()
		{
			int opcode = memory.read32(pc);

			// by default, the next instruction to emulate is at the next address
			pc += 4;

			return opcode;
		}

		public virtual void nextPc()
		{
			pc = npc;
			npc = pc + 4;
		}

		public virtual bool doJR(int rs)
		{
			npc = getRegister(rs);
			return true;
		}

		public virtual bool doJALR(int rd, int rs)
		{
			if (rd != 0)
			{
				setRegister(rd, pc + 4);
			}
			npc = getRegister(rs);
			// It seems the PSP ignores the lowest 2 bits of the address.
			// These bits are used and set by interruptman.prx
			// but never cleared explicitly before executing a jalr instruction.
			npc &= unchecked((int)0xFFFFFFFC);

			return true;
		}

		public virtual bool doBLTZ(int rs, int simm16)
		{
			npc = (getRegister(rs) < 0) ? branchTarget(pc, simm16) : (pc + 4);
			return true;
		}

		public virtual bool doBGEZ(int rs, int simm16)
		{
			npc = (getRegister(rs) >= 0) ? branchTarget(pc, simm16) : (pc + 4);
			return true;
		}

		public virtual bool doBLTZL(int rs, int simm16)
		{
			if (getRegister(rs) < 0)
			{
				npc = branchTarget(pc, simm16);
				return true;
			}
			pc += 4;
			return false;
		}

		public virtual bool doBGEZL(int rs, int simm16)
		{
			if (getRegister(rs) >= 0)
			{
				npc = branchTarget(pc, simm16);
				return true;
			}
			pc += 4;
			return false;
		}

		public virtual bool doBLTZAL(int rs, int simm16)
		{
			int target = pc + 4;
			bool t = (getRegister(rs) < 0);
			_ra = target;
			npc = t ? branchTarget(pc, simm16) : target;
			return true;
		}

		public virtual bool doBGEZAL(int rs, int simm16)
		{
			int target = pc + 4;
			bool t = (getRegister(rs) >= 0);
			_ra = target;
			npc = t ? branchTarget(pc, simm16) : target;
			return true;
		}

		public virtual bool doBLTZALL(int rs, int simm16)
		{
			bool t = (getRegister(rs) < 0);
			_ra = pc + 4;
			if (t)
			{
				npc = branchTarget(pc, simm16);
			}
			else
			{
				pc += 4;
			}
			return t;
		}

		public virtual bool doBGEZALL(int rs, int simm16)
		{
			bool t = (getRegister(rs) >= 0);
			_ra = pc + 4;
			if (t)
			{
				npc = branchTarget(pc, simm16);
			}
			else
			{
				pc += 4;
			}
			return t;
		}

		public virtual bool doJ(int uimm26)
		{
			npc = jumpTarget(pc, uimm26);
			if (npc == pc - 4)
			{
				log.info("Pausing emulator - jump to self (death loop)");
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_JUMPSELF);
			}
			return true;
		}

		public virtual bool doJAL(int uimm26)
		{
			_ra = pc + 4;
			npc = jumpTarget(pc, uimm26);
			return true;
		}

		public virtual bool doBEQ(int rs, int rt, int simm16)
		{
			npc = (getRegister(rs) == getRegister(rt)) ? branchTarget(pc, simm16) : (pc + 4);
			if (npc == pc - 4 && rs == rt)
			{
				log.info("Pausing emulator - branch to self (death loop)");
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_JUMPSELF);
			}
			return true;
		}

		public virtual bool doBNE(int rs, int rt, int simm16)
		{
			npc = (getRegister(rs) != getRegister(rt)) ? branchTarget(pc, simm16) : (pc + 4);
			return true;
		}

		public virtual bool doBLEZ(int rs, int simm16)
		{
			npc = (getRegister(rs) <= 0) ? branchTarget(pc, simm16) : (pc + 4);
			return true;
		}

		public virtual bool doBGTZ(int rs, int simm16)
		{
			npc = (getRegister(rs) > 0) ? branchTarget(pc, simm16) : (pc + 4);
			return true;
		}

		public virtual bool doBEQL(int rs, int rt, int simm16)
		{
			if (getRegister(rs) == getRegister(rt))
			{
				npc = branchTarget(pc, simm16);
				return true;
			}
			pc += 4;
			return false;
		}

		public virtual bool doBNEL(int rs, int rt, int simm16)
		{
			if (getRegister(rs) != getRegister(rt))
			{
				npc = branchTarget(pc, simm16);
				return true;
			}
			pc += 4;
			return false;
		}

		public virtual bool doBLEZL(int rs, int simm16)
		{
			if (getRegister(rs) <= 0)
			{
				npc = branchTarget(pc, simm16);
				return true;
			}
			pc += 4;
			return false;
		}

		public virtual bool doBGTZL(int rs, int simm16)
		{
			if (getRegister(rs) > 0)
			{
				npc = branchTarget(pc, simm16);
				return true;
			}
			pc += 4;
			return false;
		}

		public virtual int doERET(Processor processor)
		{
			int status = processor.cp0.Status;
			int epc;
			if ((status & 0x4) != 0)
			{
				status &= ~0x4; // Clear ERL
				epc = processor.cp0.ErrorEpc;
			}
			else
			{
				status &= ~0x2; // Clear EXL
				epc = processor.cp0.Epc;
			}
			processor.cp0.Status = status;

			processor.enableInterrupts();

			if (Emulator.log.DebugEnabled)
			{
				Emulator.Console.WriteLine(string.Format("0x{0:X8} - eret with status=0x{1:X}, epc=0x{2:X8}", processor.cpu.pc, status, epc));
			}

			return epc;
		}
	}
}