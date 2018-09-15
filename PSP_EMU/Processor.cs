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
namespace pspsharp
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.BcuState.jumpTarget;

	using Instruction = pspsharp.Allegrex.Common.Instruction;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using IState = pspsharp.state.IState;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;
	using Common = pspsharp.Allegrex.Common;
	using Cp0State = pspsharp.Allegrex.Cp0State;
	using CpuState = pspsharp.Allegrex.CpuState;
	using Decoder = pspsharp.Allegrex.Decoder;
	using Instructions = pspsharp.Allegrex.Instructions;

	//using Logger = org.apache.log4j.Logger;

	public class Processor : IState
	{
		private const int STATE_VERSION = 0;
		public CpuState cpu = new CpuState();
		public Cp0State cp0 = new Cp0State();
		public static readonly Memory memory = Memory.Instance;
		protected internal Logger log = Logger.getLogger("cpu");
		private bool interruptsEnabled;

		public Processor()
		{
			Logger = log;
			reset();
		}

		protected internal virtual Logger Logger
		{
			set
			{
				this.log = value;
				cpu.Logger = value;
			}
			get
			{
				return log;
			}
		}


		public virtual CpuState Cpu
		{
			set
			{
				this.cpu = value;
			}
		}

		public virtual void reset()
		{
			interruptsEnabled = true;
			cpu.reset();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			cpu.read(stream);
			cp0.read(stream);
			interruptsEnabled = stream.readBoolean();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			cpu.write(stream);
			cp0.write(stream);
			stream.writeBoolean(interruptsEnabled);
		}

		public virtual Instruction interpret()
		{
			int opcode = cpu.fetchOpcode();
			Instruction insn = Decoder.instruction(opcode);
			if (log.TraceEnabled)
			{
				log.trace(string.Format("Interpreting 0x{0:X8}: [0x{1:X8}] - {2}", cpu.pc - 4, opcode, insn.disasm(cpu.pc - 4, opcode)));
			}
			insn.interpret(this, opcode);

			if (RuntimeContext.debugCodeBlockCalls)
			{
				if (insn == Instructions.JAL)
				{
					RuntimeContext.debugCodeBlockStart(cpu, cpu.pc);
				}
				else if (insn == Instructions.JR && ((opcode >> 21) & 31) == Common._ra)
				{
					int opcodeCaller = cpu.memory.read32(cpu._ra - 8);
					Instruction insnCaller = Decoder.instruction(opcodeCaller);
					int codeBlockStart = cpu.pc;
					if (insnCaller == Instructions.JAL)
					{
						codeBlockStart = jumpTarget(cpu.pc, (opcodeCaller) & 0x3FFFFFF);
					}
					RuntimeContext.debugCodeBlockEnd(cpu, codeBlockStart, cpu._ra);
				}
			}

			return insn;
		}

		public virtual void interpretDelayslot()
		{
			int opcode = cpu.nextOpcode();
			Instruction insn = Decoder.instruction(opcode);
			if (log.TraceEnabled)
			{
				log.trace(string.Format("Interpreting 0x{0:X8}: [0x{1:X8}] - {2}", cpu.pc - 4, opcode, insn.disasm(cpu.pc - 4, opcode)));
			}
			insn.interpret(this, opcode);
			cpu.nextPc();
		}

		public virtual bool InterruptsEnabled
		{
			get
			{
				return interruptsEnabled;
			}
			set
			{
				if (this.interruptsEnabled != value)
				{
					this.interruptsEnabled = value;
    
					if (value)
					{
						// Interrupts have been enabled
						IntrManager.Instance.onInterruptsEnabled();
					}
				}
			}
		}

		public virtual bool InterruptsDisabled
		{
			get
			{
				return !InterruptsEnabled;
			}
		}


		public virtual void enableInterrupts()
		{
			InterruptsEnabled = true;
		}

		public virtual void disableInterrupts()
		{
			InterruptsEnabled = false;
		}

		public virtual void step()
		{
			interpret();
		}

		public static bool isInstructionInDelaySlot(Memory memory, int address)
		{
			int previousInstruction = memory.read32(address - 4);
			switch ((previousInstruction >> 26) & 0x3F)
			{
				case AllegrexOpcodes.J:
				case AllegrexOpcodes.JAL:
				case AllegrexOpcodes.BEQ:
				case AllegrexOpcodes.BNE:
				case AllegrexOpcodes.BLEZ:
				case AllegrexOpcodes.BGTZ:
				case AllegrexOpcodes.BEQL:
				case AllegrexOpcodes.BNEL:
				case AllegrexOpcodes.BLEZL:
				case AllegrexOpcodes.BGTZL:
					return true;
				case AllegrexOpcodes.SPECIAL:
					switch (previousInstruction & 0x3F)
					{
						case AllegrexOpcodes.JR:
						case AllegrexOpcodes.JALR:
							return true;
					}
					break;
				case AllegrexOpcodes.REGIMM:
					switch ((previousInstruction >> 16) & 0x1F)
					{
						case AllegrexOpcodes.BLTZ:
						case AllegrexOpcodes.BGEZ:
						case AllegrexOpcodes.BLTZL:
						case AllegrexOpcodes.BGEZL:
						case AllegrexOpcodes.BLTZAL:
						case AllegrexOpcodes.BGEZAL:
						case AllegrexOpcodes.BLTZALL:
						case AllegrexOpcodes.BGEZALL:
							return true;
					}
					break;
				case AllegrexOpcodes.COP1:
					switch ((previousInstruction >> 21) & 0x1F)
					{
						case AllegrexOpcodes.COP1BC:
							switch ((previousInstruction >> 16) & 0x1F)
							{
								case AllegrexOpcodes.BC1F:
								case AllegrexOpcodes.BC1T:
								case AllegrexOpcodes.BC1FL:
								case AllegrexOpcodes.BC1TL:
									return true;
							}
							break;
					}
					break;
			}

			return false;
		}
	}
}