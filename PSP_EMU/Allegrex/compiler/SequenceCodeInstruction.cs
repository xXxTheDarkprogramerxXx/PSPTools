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
	using MethodVisitor = org.objectweb.asm.MethodVisitor;

	public class SequenceCodeInstruction : CodeInstruction
	{
		private CodeSequence codeSequence;

		public SequenceCodeInstruction(CodeSequence codeSequence)
		{
			this.codeSequence = codeSequence;
			Address = codeSequence.StartAddress;

			CodeInstruction firstInstruction = codeSequence.Instructions[0];
			BranchTarget = firstInstruction.BranchTarget;
			Branching = firstInstruction.Branching;
			BranchingTo = firstInstruction.BranchingTo;
		}

		public virtual CodeSequence CodeSequence
		{
			get
			{
				return codeSequence;
			}
		}

		public override int EndAddress
		{
			get
			{
				return codeSequence.EndAddress;
			}
		}

		public override int Length
		{
			get
			{
				return codeSequence.Length;
			}
		}

		public override void compile(CompilerContext context, MethodVisitor mv)
		{
			startCompile(context, mv);

			context.visitCall(context.CodeBlock.StartAddress, getMethodName(context));
		}

		public override bool hasFlags(int flags)
		{
			return false;
		}

		public virtual string getMethodName(CompilerContext context)
		{
			return context.StaticExecMethodName + codeSequence.StartAddress.ToString("x");
		}

		public override string ToString()
		{
			return string.Format("0x{0:X} - {1}", codeSequence.StartAddress, codeSequence.ToString());
		}
	}

}