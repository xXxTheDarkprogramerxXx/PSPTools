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
namespace pspsharp.mediaengine
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.ExceptionManager.EXCEP_INT;

	using Level = org.apache.log4j.Level;
	using Logger = org.apache.log4j.Logger;

	using Instruction = pspsharp.Allegrex.Common.Instruction;
	using Decoder = pspsharp.Allegrex.Decoder;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using MMIOHandlerInterruptMan = pspsharp.memory.mmio.MMIOHandlerInterruptMan;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// The PSP Media Engine is very close to the PSP main CPU. It has the same instructions
	/// with the addition of 3 new ones. It has a FPU but no VFPU.
	/// 
	/// The ME specific instructions are:
	/// - mtvme rt, imm16
	///      opcode: 0xB0E00000 | (rt << 16) | imm16
	///      stores the content of the CPU register rt to an unknown VME register imm16
	/// - mfvme rt, imm16
	///      opcode: 0x68E00000 | (rt << 16) | imm16
	///      loads the content of an unknown VME register imm16 to the CPU register rt
	/// - dbreak
	///      opcode: 0x7000003F
	///      debugging break causing a trap to the address 0xBFC01000 (?)
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class MEProcessor : Processor
	{
		public static new Logger log = Logger.getLogger("me");
		private const int STATE_VERSION = 0;
		public const int CPUID_ME = 1;
		private static MEProcessor instance;
		private MEMemory meMemory;
		private readonly int[] vmeRegisters = new int[0x590]; // Highest VME register number seen is 0x058F
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool halt_Renamed;
		private int pendingInterruptIPbits;
		private Instruction[] instructions;
		private const int optimizedRunStart = MemoryMap.START_RAM;
		private static readonly int optimizedRunEnd = MemoryMap.START_RAM + 0x3000;

		public static MEProcessor Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MEProcessor();
				}
				return instance;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			stream.readInts(vmeRegisters);
			halt_Renamed = stream.readBoolean();
			pendingInterruptIPbits = stream.readInt();
			base.read(stream);

			instructions = null;
			sync();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInts(vmeRegisters);
			stream.writeBoolean(halt_Renamed);
			stream.writeInt(pendingInterruptIPbits);
			base.write(stream);
		}

		private MEProcessor()
		{
			Logger = log;
			meMemory = new MEMemory(RuntimeContextLLE.MMIO, log);
			cpu.Memory = meMemory;

			// CPUID is 1 for the ME
			cp0.Cpuid = CPUID_ME;

			halt_Renamed = true;
		}

		public virtual MEMemory MEMemory
		{
			get
			{
				return meMemory;
			}
		}

		public virtual int getVmeRegister(int reg)
		{
			return vmeRegisters[reg];
		}

		public virtual void setVmeRegister(int reg, int value)
		{
			vmeRegisters[reg] = value;
		}

		public virtual void triggerException(int IP)
		{
			pendingInterruptIPbits |= IP;

			if (pendingInterruptIPbits != 0)
			{
				halt_Renamed = false;
				METhread.Instance.sync();
			}
		}

		private void sync()
		{
			METhread meThread = METhread.Instance;
			meThread.Processor = this;
			meThread.sync();
		}

		public virtual void triggerReset()
		{
			int status = 0;
			// BEV = 1 during bootstrapping
			status |= 0x00400000;
			// Set the initial status
			cp0.Status = status;
			// All interrupts disabled
			disableInterrupts();

			cpu.pc = unchecked((int)0xBFC00000);

			halt_Renamed = false;

			if (log.TraceEnabled)
			{
				// The TRACE level is generating too much output during the initial reset (or after a sceKernelLoadExec())
				log.Level = Level.DEBUG;
			}

			sync();
		}

		public virtual void halt()
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("MEProcessor.halt: pendingInterruptIPbits=0x%X, isInterruptExecutionAllowed=%b, doTriggerException=%b, status=0x%X, pc=0x%08X", pendingInterruptIPbits, isInterruptExecutionAllowed(), pspsharp.memory.mmio.MMIOHandlerInterruptMan.getInstance(this).doTriggerException(), cp0.getStatus(), cpu.pc));
				log.debug(string.Format("MEProcessor.halt: pendingInterruptIPbits=0x%X, isInterruptExecutionAllowed=%b, doTriggerException=%b, status=0x%X, pc=0x%08X", pendingInterruptIPbits, InterruptExecutionAllowed, MMIOHandlerInterruptMan.getInstance(this).doTriggerException(), cp0.Status, cpu.pc));
			}

			if (pendingInterruptIPbits == 0 && !MMIOHandlerInterruptMan.getInstance(this).doTriggerException())
			{
				halt_Renamed = true;
			}
		}

		public virtual bool Halted
		{
			get
			{
				return halt_Renamed;
			}
		}

		private bool InterruptExecutionAllowed
		{
			get
			{
				if (pendingInterruptIPbits == 0)
				{
					return false;
				}
    
				if (InterruptsDisabled)
				{
					return false;
				}
    
				int status = cp0.Status;
    
				// Is the processor already in an exception state?
				if ((status & 0x2) != 0)
				{
					return false;
				}
    
				// Is the interrupt masked?
				if (((pendingInterruptIPbits << 8) & status) == 0)
				{
					return false;
				}
    
				return true;
			}
		}

		private int ExceptionCause
		{
			set
			{
				int cause = cp0.Cause;
				cause = (cause & unchecked((int)0xFFFFFF00)) | (value << 2);
				cp0.Cause = cause;
			}
		}

		private int prepareExceptionHandlerCall(bool forceNoDelaySlot)
		{
			int epc = cpu.pc;

			int cause = cp0.Cause;
			if (!forceNoDelaySlot && epc != 0 && isInstructionInDelaySlot(cpu.memory, epc))
			{
				cause |= unchecked((int)0x80000000); // Set BD flag (Branch Delay Slot)
				epc -= 4; // The EPC is set to the instruction having the delay slot
			}
			else
			{
				cause &= ~0x80000000; // Clear BD flag (Branch Delay Slot)
			}
			cp0.Cause = cause;

			// Set the EPC
			cp0.Epc = epc;

			// Set the EXL bit
			int status = cp0.Status;
			status |= 0x2; // Set EXL bit
			cp0.Status = status;

			int ebase;
			// BEV bit set?
			if ((status & 0x00400000) == 0)
			{
				ebase = cp0.Ebase;
			}
			else
			{
				ebase = unchecked((int)0xBFC00000);
			}

			halt_Renamed = false;

			return ebase;
		}

		private void checkPendingInterruptException()
		{
			if (InterruptExecutionAllowed)
			{
				int cause = cp0.Cause;
				cause |= (pendingInterruptIPbits << 8);
				pendingInterruptIPbits = 0;
				cp0.Cause = cause;

				ExceptionCause = EXCEP_INT;
				cpu.pc = prepareExceptionHandlerCall(false);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("MEProcessor calling exception handler at 0x{0:X8}, IP bits=0x{1:X2}", cpu.pc, (cause >> 8) & 0xFF));
				}
			}
		}

		private void initRun()
		{
			instructions = new Instruction[(optimizedRunEnd - optimizedRunStart) >> 2];
			for (int pc = optimizedRunStart; pc < optimizedRunEnd; pc += 4)
			{
				int opcode = memory.read32(pc);
				instructions[(pc - optimizedRunStart) >> 2] = Decoder.instruction(opcode);
			}
		}

		private void optimizedRun()
		{
			int[] memoryInt = RuntimeContext.MemoryInt;

			int count = 0;
			long start = Emulator.Clock.currentTimeMillis();

			while (!halt_Renamed && !Emulator.pause)
			{
				if (pendingInterruptIPbits != 0)
				{
					checkPendingInterruptException();
				}

				int pc = cpu.pc & Memory.addressMask;
				if (pc >= optimizedRunEnd)
				{
					break;
				}
				int insnIndex = (pc - optimizedRunStart) >> 2;
				int opcode = memoryInt[pc >> 2];
				cpu.pc += 4;

				Instruction insn = instructions[insnIndex];
				if (log.TraceEnabled)
				{
					log.trace(string.Format("Interpreting 0x{0:X8}: [0x{1:X8}] - {2}", cpu.pc - 4, opcode, insn.disasm(cpu.pc - 4, opcode)));
				}
				insn.interpret(this, opcode);
				count++;
			}

			long end = Emulator.Clock.currentTimeMillis();
			if (count > 0 && log.InfoEnabled)
			{
				int duration = System.Math.Max((int)(end - start), 1);
				log.info(string.Format("MEProcessor {0:D} instructions executed in {1:D} ms: {2:D} instructions per ms", count, duration, (count + duration / 2) / duration));
			}
		}

		private void normalRun()
		{
			int count = 0;
			long start = Emulator.Clock.currentTimeMillis();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean hasMemoryInt = pspsharp.Allegrex.compiler.RuntimeContext.hasMemoryInt();
			bool hasMemoryInt = RuntimeContext.hasMemoryInt();

			while (!halt_Renamed && !Emulator.pause)
			{
				if (pendingInterruptIPbits != 0)
				{
					checkPendingInterruptException();
				}

				step();
				count++;

				int pc = cpu.pc & Memory.addressMask;
				if (hasMemoryInt && pc >= optimizedRunStart && pc < optimizedRunEnd)
				{
					break;
				}

				if (cpu.pc == unchecked((int)0x883000E0) && log.DebugEnabled)
				{
					log.debug(string.Format("Initial ME memory content from meimg.img:"));
					log.debug(Utilities.getMemoryDump(meMemory, 0x00101000, cpu._v0));
	//				log.setLevel(Level.TRACE);
				}
			}

			long end = Emulator.Clock.currentTimeMillis();
			if (count > 0 && log.InfoEnabled)
			{
				int duration = System.Math.Max((int)(end - start), 1);
				log.info(string.Format("MEProcessor {0:D} instructions executed in {1:D} ms: {2:D} instructions per ms", count, duration, (count + duration / 2) / duration));
			}
		}

		public virtual void run()
		{
			if (!Emulator.run_Renamed)
			{
				return;
			}

			if (instructions == null)
			{
				initRun();
			}

			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("MEProcessor starting run: halt=%b, pendingInterruptIPbits=0x%X, pc=0x%08X", halt, pendingInterruptIPbits, cpu.pc));
				log.debug(string.Format("MEProcessor starting run: halt=%b, pendingInterruptIPbits=0x%X, pc=0x%08X", halt_Renamed, pendingInterruptIPbits, cpu.pc));
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean hasMemoryInt = pspsharp.Allegrex.compiler.RuntimeContext.hasMemoryInt();
			bool hasMemoryInt = RuntimeContext.hasMemoryInt();

			while (!halt_Renamed && !Emulator.pause)
			{
				int pc = cpu.pc & Memory.addressMask;
				if (hasMemoryInt && pc >= optimizedRunStart && pc < optimizedRunEnd)
				{
					optimizedRun();
				}
				else
				{
					normalRun();
				}
			}

			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("MEProcessor exiting run: halt=%b, pendingInterruptIPbits=0x%X, isInterruptExecutionAllowed=%b, status=0x%X, pc=0x%08X", halt, pendingInterruptIPbits, isInterruptExecutionAllowed(), cp0.getStatus(), cpu.pc));
				log.debug(string.Format("MEProcessor exiting run: halt=%b, pendingInterruptIPbits=0x%X, isInterruptExecutionAllowed=%b, status=0x%X, pc=0x%08X", halt_Renamed, pendingInterruptIPbits, InterruptExecutionAllowed, cp0.Status, cpu.pc));
			}
		}
	}

}