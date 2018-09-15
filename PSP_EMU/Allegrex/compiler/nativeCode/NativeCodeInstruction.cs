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
	using MethodVisitor = org.objectweb.asm.MethodVisitor;

	using Instruction = pspsharp.Allegrex.Common.Instruction;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class NativeCodeInstruction : CodeInstruction
	{
		private NativeCodeSequence nativeCodeSequence;
		private int flags = 0;

		public NativeCodeInstruction(int address, NativeCodeSequence nativeCodeSequence)
		{
			this.nativeCodeSequence = nativeCodeSequence;
			this.address = address;
			init();
		}

		private void init()
		{
			if (nativeCodeSequence.hasBranchInstruction())
			{
				// Handle like a branch/jump instruction
				flags |= Instruction.FLAG_CANNOT_BE_SPLIT;
				Branching = true;

				int branchInstructionAddress = Address + (nativeCodeSequence.BranchInstruction << 2);

				int branchOpcode = Memory.Instance.read32(branchInstructionAddress);
				Instruction branchInsn = Decoder.instruction(branchOpcode);
				int npc = branchInstructionAddress + 4;
				if (branchInsn.hasFlags(Instruction.FLAG_IS_BRANCHING))
				{
					BranchingTo = Compiler.branchTarget(npc, branchOpcode);
				}
				else if (branchInsn.hasFlags(Instruction.FLAG_IS_JUMPING))
				{
					BranchingTo = Compiler.jumpTarget(npc, branchOpcode);
				}
				else
				{
					Compiler.Console.WriteLine(string.Format("Incorrect Branch Instruction at 0x{0:X8} - {1}", branchInstructionAddress, branchInsn.disasm(branchInstructionAddress, branchOpcode)));
				}
			}

			if (nativeCodeSequence.Returning)
			{
				// Handle like a "JR $ra" instruction
				flags |= Instruction.FLAG_CANNOT_BE_SPLIT;
			}
		}

		public virtual NativeCodeSequence NativeCodeSequence
		{
			get
			{
				return nativeCodeSequence;
			}
		}

		public override bool hasFlags(int testFlags)
		{
			return (flags & testFlags) == testFlags;
		}

		public override void compile(CompilerContext context, MethodVisitor mv)
		{
			startCompile(context, mv);
			context.compileNativeCodeSequence(nativeCodeSequence, this);
		}

		public override int EndAddress
		{
			get
			{
				return Address + ((nativeCodeSequence.NumOpcodes - 1) << 2);
			}
		}

		public override int Length
		{
			get
			{
				return nativeCodeSequence.NumOpcodes;
			}
		}

		public override string ToString()
		{
			return string.Format("0x{0:X} - {1}", Address, nativeCodeSequence.ToString());
		}
	}

}