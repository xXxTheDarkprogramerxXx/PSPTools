using System.Text;

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

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class SequenceSWCodeInstruction : CodeInstruction
	{
		protected internal int baseRegister;
		protected internal int[] offsets;
		protected internal int[] registers;

		public SequenceSWCodeInstruction(int baseRegister, int[] offsets, int[] registers)
		{
			this.baseRegister = baseRegister;
			this.offsets = offsets;
			this.registers = registers;
		}

		public override void compile(CompilerContext context, MethodVisitor mv)
		{
			startCompile(context, mv);
			compileInstruction(context);
			context.endInstruction();
		}

		protected internal virtual string InstructionName
		{
			get
			{
				return "sw";
			}
		}

		protected internal virtual void compileInstruction(CompilerContext context)
		{
			context.compileSWsequence(baseRegister, offsets, registers);
		}

		public override bool hasFlags(int flags)
		{
			return false;
		}

		public override string disasm(int address, int opcode)
		{
			StringBuilder result = new StringBuilder(string.Format("{0,-10} ", InstructionName));

			for (int i = 0; i < registers.Length; i++)
			{
				if (i > 0)
				{
					result.Append("/");
				}
				result.Append(Common.gprNames[registers[i]]);
			}
			result.Append(", ");
			for (int i = 0; i < offsets.Length; i++)
			{
				if (i > 0)
				{
					result.Append("/");
				}
				result.Append(offsets[i]);
			}
			result.Append("(");
			result.Append(Common.gprNames[baseRegister]);
			result.Append(")");

			return result.ToString();
		}
	}

}