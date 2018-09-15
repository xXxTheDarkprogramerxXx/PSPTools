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
namespace pspsharp.Allegrex.compiler
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._ra;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._zr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_WRITES_RD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.Instruction.FLAG_WRITES_RT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.NOP;

	using Instruction = pspsharp.Allegrex.Common.Instruction;
	using NativeCodeInstruction = pspsharp.Allegrex.compiler.nativeCode.NativeCodeInstruction;
	using NativeCodeSequence = pspsharp.Allegrex.compiler.nativeCode.NativeCodeSequence;

	//using Logger = org.apache.log4j.Logger;
	using Label = org.objectweb.asm.Label;
	using MethodVisitor = org.objectweb.asm.MethodVisitor;
	using Opcodes = org.objectweb.asm.Opcodes;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class CodeInstruction
	{
		private static Logger log = Compiler.log;
		private const bool interpretAllVfpuInstructions = false;
		protected internal int address;
		private int opcode;
		private Instruction insn;
		private bool isBranchTarget;
		private int branchingTo;
		private bool isBranching;
		private Label label;
		private bool isDelaySlot;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool useMMIO_Renamed;

		protected internal CodeInstruction()
		{
		}

		public CodeInstruction(int address, int opcode, Instruction insn, bool isBranchTarget, bool isBranching, int branchingTo)
		{
			this.address = address;
			this.opcode = opcode;
			this.insn = insn;
			this.isBranchTarget = isBranchTarget;
			this.isBranching = isBranching;
			this.branchingTo = branchingTo;
		}

		public CodeInstruction(CodeInstruction codeInstruction)
		{
			this.address = codeInstruction.address;
			this.opcode = codeInstruction.opcode;
			this.insn = codeInstruction.insn;
			this.isBranchTarget = codeInstruction.isBranchTarget;
			this.branchingTo = codeInstruction.branchingTo;
			this.isBranching = codeInstruction.isBranching;
			this.label = codeInstruction.label;
		}

		public virtual int Address
		{
			get
			{
				return address;
			}
			set
			{
				this.address = value;
			}
		}

		public virtual int EndAddress
		{
			get
			{
				return address;
			}
		}

		public virtual int Length
		{
			get
			{
				return 1;
			}
		}


		public virtual int Opcode
		{
			get
			{
				return opcode;
			}
			set
			{
				this.opcode = value;
			}
		}


		public virtual Instruction Insn
		{
			get
			{
				return insn;
			}
			set
			{
				this.insn = value;
			}
		}


		public virtual bool BranchTarget
		{
			get
			{
				return isBranchTarget;
			}
			set
			{
				this.isBranchTarget = value;
			}
		}


		public virtual bool Branching
		{
			get
			{
				return isBranching;
			}
			set
			{
				this.isBranching = value;
			}
		}


		public virtual int BranchingTo
		{
			get
			{
				return branchingTo;
			}
			set
			{
				this.branchingTo = value;
			}
		}


		public virtual bool DelaySlot
		{
			get
			{
				return isDelaySlot;
			}
		}

		public virtual bool IsDelaySlot
		{
			set
			{
				this.isDelaySlot = value;
			}
		}

		public virtual Label getLabel(bool isBranchTarget)
		{
			if (label == null)
			{
				label = new Label();
				if (isBranchTarget)
				{
					BranchTarget = true;
				}
			}

			return label;
		}

		public virtual Label Label
		{
			get
			{
				return getLabel(true);
			}
			set
			{
				this.label = value;
			}
		}

		public virtual bool hasLabel()
		{
			return label != null;
		}

		public virtual void forceNewLabel()
		{
			label = new Label();
		}


		protected internal virtual void startCompile(CompilerContext context, MethodVisitor mv)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(ToString());
			}

			context.CodeInstruction = this;

			context.beforeInstruction(this);

			if (hasLabel())
			{
				mv.visitLabel(Label);
			}

			context.startInstruction(this);
		}

		public virtual void compile(CompilerContext context, MethodVisitor mv)
		{
			startCompile(context, mv);

			if (Branching)
			{
				compileBranch(context, mv);
			}
			else if (insn == Instructions.JR)
			{
				compileJr(context, mv);
			}
			else if (insn == Instructions.JALR)
			{
				compileJalr(context, mv);
			}
			else if (insn == Instructions.ERET)
			{
				context.compileEret();
			}
			else if (interpretAllVfpuInstructions && insn.category().StartsWith("VFPU", StringComparison.Ordinal))
			{
				context.visitIntepreterCall(opcode, insn);
			}
			else
			{
				insn.compile(context, Opcode);
			}

			context.endInstruction();
		}

		private void compileJr(CompilerContext context, MethodVisitor mv)
		{
			// Retrieve the call address from the Rs register before executing
			// the delay slot instruction, as it might theoretically modify the
			// content of the Rs register.
			context.loadRs();
			compileDelaySlot(context, mv);
			context.visitJump();
		}

		private void compileJalr(CompilerContext context, MethodVisitor mv)
		{
			// Retrieve the call address from the Rs register before executing
			// the delay slot instruction, as it might theoretically modify the
			// content of the Rs register.
			context.loadRs();

			// It seems the PSP ignores the lowest 2 bits of the address.
			// These bits are used and set by interruptman.prx
			// but never cleared explicitly before executing a jalr instruction.
			context.loadImm(0xFFFFFFFC);
			mv.visitInsn(Opcodes.IAND);

			compileDelaySlot(context, mv);
			context.visitCall(Address + 8, context.RdRegisterIndex);
		}

		private void compileBranch(CompilerContext context, MethodVisitor mv)
		{
			// Perform any required checkSync() before executing the delay slot instruction,
			// so that the emulation can be paused in a consistent state
			// before the branch and its delay slot.
			context.startJump(BranchingTo);

			int branchingOpcode = getBranchingOpcode(context, mv);

			if (branchingOpcode != Opcodes.NOP)
			{
				CodeInstruction branchingToCodeInstruction = context.CodeBlock.getCodeInstruction(BranchingTo);

				// Fallback when branching to the 2nd instruction of a native code sequence whose
				// 1st instruction is the delay slot instruction.
				// In such a case, assume a branch to the native code sequence.
				if (branchingToCodeInstruction == null)
				{
					CodeInstruction nativeCodeInstruction = context.getCodeInstruction(BranchingTo - 4);
					if (nativeCodeInstruction != null && nativeCodeInstruction is NativeCodeInstruction)
					{
						NativeCodeSequence nativeCodeSequence = ((NativeCodeInstruction) nativeCodeInstruction).NativeCodeSequence;
						if (getDelaySlotCodeInstruction(context).Opcode == nativeCodeSequence.FirstOpcode)
						{
							//if (log.DebugEnabled)
							{
								Console.WriteLine(string.Format("0x{0:X8}: branching to the 2nd instruction of a native code sequence, assuming the 1st instruction", Address));
							}
							branchingToCodeInstruction = nativeCodeInstruction;
						}
					}
				}

				if (branchingToCodeInstruction != null)
				{
					// Some applications do have branches to delay slot instructions
					// (probably from programmers that didn't know/care about delay slots).
					//
					// Handle a branch to a NOP in a delay slot: just skip the NOP and assume the branch
					// is to the instruction following the NOP.
					// E.g.:
					//    0x00000000    b 0x00000014 -> branching to a NOP in a delay slot, assume a branch to 0x00000018
					//    0x00000004    nop
					//    ...
					//    0x00000010    b 0x00000020
					//    0x00000014    nop
					//    0x00000018    something
					//
					if (branchingToCodeInstruction.Insn == Instructions.NOP)
					{
						CodeInstruction beforeBranchingToCodeInstruction = context.CodeBlock.getCodeInstruction(BranchingTo - 4);
						if (beforeBranchingToCodeInstruction != null && beforeBranchingToCodeInstruction.hasFlags(Instruction.FLAG_HAS_DELAY_SLOT))
						{
							//if (log.DebugEnabled)
							{
								Console.WriteLine(string.Format("0x{0:X8}: branching to a NOP in a delay slot, correcting to the next instruction", Address));
							}
							branchingToCodeInstruction = context.CodeBlock.getCodeInstruction(BranchingTo + 4);
						}
					}
					context.visitJump(branchingOpcode, branchingToCodeInstruction);
				}
				else
				{
					context.visitJump(branchingOpcode, BranchingTo);
				}
			}
		}

		private CodeInstruction getAfterDelaySlotCodeInstruction(CompilerContext context)
		{
			return context.CodeBlock.getCodeInstruction(Address + 8);
		}

		private CodeInstruction getDelaySlotCodeInstruction(CompilerContext context)
		{
			return context.CodeBlock.getCodeInstruction(Address + 4);
		}

		private void compileDelaySlot(CompilerContext context, MethodVisitor mv)
		{
			CodeInstruction delaySlotCodeInstruction = getDelaySlotCodeInstruction(context);
			compileDelaySlot(context, mv, delaySlotCodeInstruction);
		}

		private void compileDelaySlot(CompilerContext context, MethodVisitor mv, CodeInstruction delaySlotCodeInstruction)
		{
			if (delaySlotCodeInstruction == null)
			{
				Console.WriteLine(string.Format("Cannot find delay slot instruction at 0x{0:X8}", Address + 4));
				return;
			}

			if (delaySlotCodeInstruction.hasFlags(Instruction.FLAG_HAS_DELAY_SLOT))
			{
				// Issue a warning when compiling an instruction having a delay slot inside a delay slot.
				// See http://code.google.com/p/pcsx2/source/detail?r=5541
				string lineSeparator = System.getProperty("line.separator");
				Console.WriteLine(string.Format("Instruction in a delay slot having a delay slot:{0}{1}{2}{3}", lineSeparator, this, lineSeparator, delaySlotCodeInstruction));
			}

			delaySlotCodeInstruction.IsDelaySlot = true;
			Label delaySlotLabel = null;
			if (delaySlotCodeInstruction.hasLabel())
			{
				delaySlotLabel = delaySlotCodeInstruction.Label;
				delaySlotCodeInstruction.forceNewLabel();
			}
			delaySlotCodeInstruction.compile(context, mv);
			if (delaySlotLabel != null)
			{
				delaySlotCodeInstruction.Label = delaySlotLabel;
			}
			else if (delaySlotCodeInstruction.hasLabel())
			{
				delaySlotCodeInstruction.forceNewLabel();
			}
			context.CodeInstruction = this;
			context.skipInstructions(1, false);
		}

		private int getBranchingOpcodeBranch0(CompilerContext context, MethodVisitor mv)
		{
			compileDelaySlot(context, mv);

			if (BranchingTo == Address)
			{
				context.visitLogInfo(mv, string.Format("Pausing emulator - jump to self (death loop) at 0x{0:X8}", Address));
				context.visitPauseEmuWithStatus(mv, Emulator.EMU_STATUS_JUMPSELF);
			}

			return Opcodes.GOTO;
		}

		/// <summary>
		/// Check if the delay slot instruction is modifying the given register.
		/// </summary>
		/// <param name="context">        the current compiler context </param>
		/// <param name="registerIndex">  the register to be checked </param>
		/// <returns>               true if the delay slot instruction is modifying the register
		///                       false otherwise. </returns>
		private bool isDelaySlotWritingRegister(CompilerContext context, int registerIndex)
		{
			CodeInstruction delaySlotCodeInstruction = getDelaySlotCodeInstruction(context);
			if (delaySlotCodeInstruction == null)
			{
				return false;
			}
			return delaySlotCodeInstruction.isWritingRegister(registerIndex);
		}

		private int getBranchingOpcodeCall0(CompilerContext context, MethodVisitor mv)
		{
			context.prepareCall(BranchingTo, Address + 8, _ra);
			compileDelaySlot(context, mv);
			context.visitCall(BranchingTo, Address + 8, _ra, isDelaySlotWritingRegister(context, _ra), false);

			return Opcodes.NOP;
		}

		private int getBranchingOpcodeBranch1(CompilerContext context, MethodVisitor mv, int branchingOpcode, int notBranchingOpcode)
		{
			// Retrieve the call address from the Rs register before executing
			// the delay slot instruction, as it might theoretically modify the
			// content of the Rs register.
			context.loadRs();
			compileDelaySlot(context, mv);

			return branchingOpcode;
		}

		private int getBranchingOpcodeBranch1L(CompilerContext context, MethodVisitor mv, int branchingOpcode, int notBranchingOpcode)
		{
			context.loadRs();
			CodeInstruction afterDelaySlotCodeInstruction = getAfterDelaySlotCodeInstruction(context);
			context.visitJump(notBranchingOpcode, afterDelaySlotCodeInstruction);
			compileDelaySlot(context, mv);

			return Opcodes.GOTO;
		}

		private int getBranchingOpcodeCall1(CompilerContext context, MethodVisitor mv, int branchingOpcode, int notBranchingOpcode)
		{
			context.prepareCall(BranchingTo, Address + 8, _ra);
			context.loadRs();
			compileDelaySlot(context, mv);
			CodeInstruction afterDelaySlotCodeInstruction = getAfterDelaySlotCodeInstruction(context);
			context.visitJump(notBranchingOpcode, afterDelaySlotCodeInstruction);
			context.visitCall(BranchingTo, Address + 8, _ra, isDelaySlotWritingRegister(context, _ra), true);

			return Opcodes.NOP;
		}

		private int getBranchingOpcodeCall1L(CompilerContext context, MethodVisitor mv, int branchingOpcode, int notBranchingOpcode)
		{
			context.prepareCall(BranchingTo, Address + 8, _ra);
			context.loadRs();
			CodeInstruction afterDelaySlotCodeInstruction = getAfterDelaySlotCodeInstruction(context);
			context.visitJump(notBranchingOpcode, afterDelaySlotCodeInstruction);
			compileDelaySlot(context, mv);
			context.visitCall(BranchingTo, Address + 8, _ra, isDelaySlotWritingRegister(context, _ra), true);

			return Opcodes.NOP;
		}

		private int loadRegistersForBranchingOpcodeBranch2(CompilerContext context, MethodVisitor mv, int branchingOpcode)
		{
			bool loadRs = true;
			bool loadRt = true;

			if (context.RsRegisterIndex == context.RtRegisterIndex)
			{
				if (branchingOpcode == Opcodes.IF_ICMPEQ)
				{
					loadRs = false;
					loadRt = false;
					branchingOpcode = Opcodes.GOTO;

					// The ASM library has problems with small frames having no
					// stack (NullPointerException). Generate a dummy stack requirement:
					//   ILOAD 0
					//   POP
					context.loadImm(0);
					context.MethodVisitor.visitInsn(Opcodes.POP);
				}
				else if (branchingOpcode == Opcodes.IF_ICMPNE)
				{
					loadRs = false;
					loadRt = false;
					branchingOpcode = Opcodes.NOP;
				}
			}
			else if (context.RsRegister0)
			{
				if (branchingOpcode == Opcodes.IF_ICMPEQ)
				{
					loadRs = false;
					branchingOpcode = Opcodes.IFEQ;
				}
				else if (branchingOpcode == Opcodes.IF_ICMPNE)
				{
					loadRs = false;
					branchingOpcode = Opcodes.IFNE;
				}
			}
			else if (context.RtRegister0)
			{
				if (branchingOpcode == Opcodes.IF_ICMPEQ)
				{
					loadRt = false;
					branchingOpcode = Opcodes.IFEQ;
				}
				else if (branchingOpcode == Opcodes.IF_ICMPNE)
				{
					loadRt = false;
					branchingOpcode = Opcodes.IFNE;
				}
			}

			if (loadRs)
			{
				context.loadRs();
			}
			if (loadRt)
			{
				context.loadRt();
			}

			return branchingOpcode;
		}

		private int getBranchingOpcodeBranch2(CompilerContext context, MethodVisitor mv, int branchingOpcode, int notBranchingOpcode)
		{
			// Retrieve the registers for the branching opcode before executing
			// the delay slot instruction, as it might theoretically modify the
			// content of these registers.
			branchingOpcode = loadRegistersForBranchingOpcodeBranch2(context, mv, branchingOpcode);

			CodeInstruction delaySlotCodeInstruction = getDelaySlotCodeInstruction(context);
			if (delaySlotCodeInstruction != null && delaySlotCodeInstruction.hasFlags(Instruction.FLAG_HAS_DELAY_SLOT))
			{
				// We are compiling a sequence where the delay instruction has itself a delay slot:
				//    beq $reg1, $reg2, label
				//    jr  $ra
				//    nop
				// Handle the sequence by inserting one nop between the instructions:
				//    bne $reg1, $reg2, label
				//    nop
				//    jr  $ra
				//    nop
				string lineSeparator = System.getProperty("line.separator");
				Console.WriteLine(string.Format("Instruction in a delay slot having a delay slot:{0}{1}{2}{3}", lineSeparator, this, lineSeparator, delaySlotCodeInstruction));
			}
			else
			{
				compileDelaySlot(context, mv, delaySlotCodeInstruction);
			}

			if (branchingOpcode == Opcodes.GOTO && BranchingTo == Address && delaySlotCodeInstruction.Opcode == NOP())
			{
				context.visitLogInfo(mv, string.Format("Pausing emulator - branch to self (death loop) at 0x{0:X8}", Address));
				context.visitPauseEmuWithStatus(mv, Emulator.EMU_STATUS_JUMPSELF);
			}

			return branchingOpcode;
		}

		private int getBranchingOpcodeBranch2L(CompilerContext context, MethodVisitor mv, int branchingOpcode, int notBranchingOpcode)
		{
			// Retrieve the registers for the branching opcode before executing
			// the delay slot instruction, as it might theoretically modify the
			// content of these registers.
			notBranchingOpcode = loadRegistersForBranchingOpcodeBranch2(context, mv, notBranchingOpcode);
			if (notBranchingOpcode != Opcodes.NOP)
			{
				CodeInstruction afterDelaySlotCodeInstruction = getAfterDelaySlotCodeInstruction(context);
				context.visitJump(notBranchingOpcode, afterDelaySlotCodeInstruction);
			}
			compileDelaySlot(context, mv);

			return Opcodes.GOTO;
		}

		private int getBranchingOpcodeBC1(CompilerContext context, MethodVisitor mv, int branchingOpcode, int notBranchingOpcode)
		{
			context.loadFcr31c();
			compileDelaySlot(context, mv);

			return branchingOpcode;
		}

		private int getBranchingOpcodeBC1L(CompilerContext context, MethodVisitor mv, int branchingOpcode, int notBranchingOpcode)
		{
			context.loadFcr31c();
			CodeInstruction afterDelaySlotCodeInstruction = getAfterDelaySlotCodeInstruction(context);
			context.visitJump(notBranchingOpcode, afterDelaySlotCodeInstruction);
			compileDelaySlot(context, mv);

			return Opcodes.GOTO;
		}

		private int getBranchingOpcodeBV(CompilerContext context, MethodVisitor mv, int branchingOpcode, int notBranchingOpcode)
		{
			context.loadVcrCc();

			CodeInstruction delaySlotCodeInstruction = getDelaySlotCodeInstruction(context);
			if (delaySlotCodeInstruction != null && delaySlotCodeInstruction.insn == insn)
			{
				// We are compiling a sequence where the delay instruction is again a BV
				// instruction:
				//    bvt 0, label
				//    bvt 1, label
				//    bvt 2, label
				//    bvt 3, label
				//    nop
				// Handle the sequence by inserting nop's between the BV instructions:
				//    bvt 0, label
				//    nop
				//    bvt 1, label
				//    nop
				//    bvt 2, label
				//    nop
				//    bvt 3, label
				//    nop
			}
			else
			{
				compileDelaySlot(context, mv, delaySlotCodeInstruction);
			}

			return branchingOpcode;
		}

		private int getBranchingOpcodeBVL(CompilerContext context, MethodVisitor mv, int branchingOpcode, int notBranchingOpcode)
		{
			context.loadVcrCc();
			CodeInstruction afterDelaySlotCodeInstruction = getAfterDelaySlotCodeInstruction(context);
			context.visitJump(notBranchingOpcode, afterDelaySlotCodeInstruction);
			compileDelaySlot(context, mv);

			return Opcodes.GOTO;
		}

		private int getBranchingOpcode(CompilerContext context, MethodVisitor mv)
		{
			int branchingOpcode = Opcodes.NOP;

			if (insn == Instructions.BEQ)
			{
				branchingOpcode = getBranchingOpcodeBranch2(context, mv, Opcodes.IF_ICMPEQ, Opcodes.IF_ICMPNE);
			}
			else if (insn == Instructions.BEQL)
			{
				branchingOpcode = getBranchingOpcodeBranch2L(context, mv, Opcodes.IF_ICMPEQ, Opcodes.IF_ICMPNE);
			}
			else if (insn == Instructions.BNE)
			{
				branchingOpcode = getBranchingOpcodeBranch2(context, mv, Opcodes.IF_ICMPNE, Opcodes.IF_ICMPEQ);
			}
			else if (insn == Instructions.BNEL)
			{
				branchingOpcode = getBranchingOpcodeBranch2L(context, mv, Opcodes.IF_ICMPNE, Opcodes.IF_ICMPEQ);
			}
			else if (insn == Instructions.BGEZ)
			{
				branchingOpcode = getBranchingOpcodeBranch1(context, mv, Opcodes.IFGE, Opcodes.IFLT);
			}
			else if (insn == Instructions.BGEZL)
			{
				branchingOpcode = getBranchingOpcodeBranch1L(context, mv, Opcodes.IFGE, Opcodes.IFLT);
			}
			else if (insn == Instructions.BGTZ)
			{
				branchingOpcode = getBranchingOpcodeBranch1(context, mv, Opcodes.IFGT, Opcodes.IFLE);
			}
			else if (insn == Instructions.BGTZL)
			{
				branchingOpcode = getBranchingOpcodeBranch1L(context, mv, Opcodes.IFGT, Opcodes.IFLE);
			}
			else if (insn == Instructions.BLEZ)
			{
				branchingOpcode = getBranchingOpcodeBranch1(context, mv, Opcodes.IFLE, Opcodes.IFGT);
			}
			else if (insn == Instructions.BLEZL)
			{
				branchingOpcode = getBranchingOpcodeBranch1L(context, mv, Opcodes.IFLE, Opcodes.IFGT);
			}
			else if (insn == Instructions.BLTZ)
			{
				branchingOpcode = getBranchingOpcodeBranch1(context, mv, Opcodes.IFLT, Opcodes.IFGE);
			}
			else if (insn == Instructions.BLTZL)
			{
				branchingOpcode = getBranchingOpcodeBranch1L(context, mv, Opcodes.IFLT, Opcodes.IFGE);
			}
			else if (insn == Instructions.J)
			{
				branchingOpcode = getBranchingOpcodeBranch0(context, mv);
			}
			else if (insn == Instructions.JAL)
			{
				branchingOpcode = getBranchingOpcodeCall0(context, mv);
			}
			else if (insn == Instructions.BLTZAL)
			{
				branchingOpcode = getBranchingOpcodeCall1(context, mv, Opcodes.IFLT, Opcodes.IFGE);
			}
			else if (insn == Instructions.BLTZALL)
			{
				branchingOpcode = getBranchingOpcodeCall1L(context, mv, Opcodes.IFLT, Opcodes.IFGE);
			}
			else if (insn == Instructions.BGEZAL)
			{
				branchingOpcode = getBranchingOpcodeCall1(context, mv, Opcodes.IFGE, Opcodes.IFLT);
			}
			else if (insn == Instructions.BGEZALL)
			{
				branchingOpcode = getBranchingOpcodeCall1L(context, mv, Opcodes.IFGE, Opcodes.IFLT);
			}
			else if (insn == Instructions.BC1F)
			{
				branchingOpcode = getBranchingOpcodeBC1(context, mv, Opcodes.IFEQ, Opcodes.IFNE);
			}
			else if (insn == Instructions.BC1FL)
			{
				branchingOpcode = getBranchingOpcodeBC1L(context, mv, Opcodes.IFEQ, Opcodes.IFNE);
			}
			else if (insn == Instructions.BC1T)
			{
				branchingOpcode = getBranchingOpcodeBC1(context, mv, Opcodes.IFNE, Opcodes.IFEQ);
			}
			else if (insn == Instructions.BC1TL)
			{
				branchingOpcode = getBranchingOpcodeBC1L(context, mv, Opcodes.IFNE, Opcodes.IFEQ);
			}
			else if (insn == Instructions.BVF)
			{
				branchingOpcode = getBranchingOpcodeBV(context, mv, Opcodes.IFEQ, Opcodes.IFNE);
			}
			else if (insn == Instructions.BVT)
			{
				branchingOpcode = getBranchingOpcodeBV(context, mv, Opcodes.IFNE, Opcodes.IFEQ);
			}
			else if (insn == Instructions.BVFL)
			{
				branchingOpcode = getBranchingOpcodeBVL(context, mv, Opcodes.IFEQ, Opcodes.IFNE);
			}
			else if (insn == Instructions.BVTL)
			{
				branchingOpcode = getBranchingOpcodeBVL(context, mv, Opcodes.IFNE, Opcodes.IFEQ);
			}
			else
			{
				Console.WriteLine("CodeInstruction.getBranchingOpcode: unknown instruction " + insn.disasm(Address, Opcode));
			}

			return branchingOpcode;
		}

		public virtual bool hasFlags(int flags)
		{
			return Insn.hasFlags(flags);
		}

		public virtual int SaValue
		{
			get
			{
				return (opcode >> 6) & 0x1F;
			}
		}

		public virtual int RsRegisterIndex
		{
			get
			{
				return (opcode >> 21) & 0x1F;
			}
		}

		public virtual int RtRegisterIndex
		{
			get
			{
				return (opcode >> 16) & 0x1F;
			}
		}

		public virtual int RdRegisterIndex
		{
			get
			{
				return (opcode >> 11) & 0x1F;
			}
		}

		public virtual int FdRegisterIndex
		{
			get
			{
				return (opcode >> 6) & 0x1F;
			}
		}

		public virtual int FsRegisterIndex
		{
			get
			{
				return (opcode >> 11) & 0x1F;
			}
		}

		public virtual int FtRegisterIndex
		{
			get
			{
				return (opcode >> 16) & 0x1F;
			}
		}

		public virtual int CrValue
		{
			get
			{
				return (opcode >> 11) & 0x1F;
			}
		}

		public virtual int VdRegisterIndex
		{
			get
			{
				return (opcode >> 0) & 0x7F;
			}
		}

		public virtual int VsRegisterIndex
		{
			get
			{
				return (opcode >> 8) & 0x7F;
			}
		}

		public virtual int VtRegisterIndex
		{
			get
			{
				return (opcode >> 16) & 0x7F;
			}
		}

		public virtual int Vsize
		{
			get
			{
				int one = (opcode >> 7) & 1;
				int two = (opcode >> 15) & 1;
    
				return 1 + one + (two << 1);
			}
		}

		public virtual int getImm14(bool signedImm)
		{
			int imm14 = opcode & 0xFFFC;
			if (signedImm)
			{
				imm14 = (int)(short) imm14;
			}

			return imm14;
		}

		public virtual int getImm16(bool signedImm)
		{
			int imm16 = opcode & 0xFFFF;
			if (signedImm)
			{
				imm16 = (int)(short) imm16;
			}

			return imm16;
		}

		public virtual int Imm7
		{
			get
			{
				return opcode & 0x7F;
			}
		}

		public virtual int Imm5
		{
			get
			{
				return (opcode >> 16) & 0x1F;
			}
		}

		public virtual int Imm4
		{
			get
			{
				return opcode & 0xF;
			}
		}

		public virtual int Imm3
		{
			get
			{
				return (opcode >> 16) & 0x7;
			}
		}

		public virtual bool isWritingRegister(int registerIndex)
		{
			// Register $zr is never written
			if (registerIndex == _zr)
			{
				return false;
			}

			if (hasFlags(FLAG_WRITES_RT))
			{
				if (RtRegisterIndex == registerIndex)
				{
					return true;
				}
			}

			if (hasFlags(FLAG_WRITES_RD))
			{
				if (RdRegisterIndex == registerIndex)
				{
					return true;
				}
			}

			return false;
		}

		public virtual string disasm(int address, int opcode)
		{
			if (Insn == null)
			{
				return ToString();
			}

			return Insn.disasm(address, opcode);
		}

		public virtual bool useMMIO()
		{
			return useMMIO_Renamed;
		}

		public virtual bool UseMMIO
		{
			set
			{
				this.useMMIO_Renamed = value;
			}
		}

		public override string ToString()
		{
			char branchingFlag;
			if (Branching)
			{
				if (hasFlags(Instruction.FLAG_STARTS_NEW_BLOCK))
				{
					branchingFlag = '<'; // branching "out" of the current block
				}
				else if (BranchingTo <= Address)
				{
					branchingFlag = '^'; // branching "up"
				}
				else
				{
					branchingFlag = 'v'; // branching "down"
				}
			}
			else
			{
				branchingFlag = ' '; // no branching
			}

			char branchTargetFlag = BranchTarget ? '>' : ' ';

			return string.Format("{0}{1} 0x{2:X8}: [0x{3:X8}] - {4}", branchingFlag, branchTargetFlag, Address, Opcode, disasm(Address, Opcode));
		}
	}
}