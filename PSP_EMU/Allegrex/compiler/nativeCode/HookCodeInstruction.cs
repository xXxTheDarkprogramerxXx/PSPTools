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


	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class HookCodeInstruction : CodeInstruction
	{
		private NativeCodeSequence nativeCodeSequence;

		public HookCodeInstruction(NativeCodeSequence nativeCodeSequence, CodeInstruction codeInstruction) : base(codeInstruction)
		{
			this.nativeCodeSequence = nativeCodeSequence;
		}

		public override void compile(CompilerContext context, MethodVisitor mv)
		{
			// Generate the instruction label before the hook call so that
			// the hook is being executed when branching to the instruction.
			if (hasLabel())
			{
				mv.visitLabel(Label);
				Label = null;
			}

			context.visitHook(nativeCodeSequence);
			base.compile(context, mv);
		}
	}

}