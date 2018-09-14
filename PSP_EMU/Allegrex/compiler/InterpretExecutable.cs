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
	using Instruction = pspsharp.Allegrex.Common.Instruction;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class InterpretExecutable : IExecutable
	{
		private CodeBlock codeBlock;
		private bool isAnalyzed;
		private bool isSimple;

		public InterpretExecutable(CodeBlock codeBlock)
		{
			this.codeBlock = codeBlock;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int exec() throws Exception
		public virtual int exec()
		{
			// Analyze at first call only
			if (!isAnalyzed)
			{
				isSimple = Compiler.Instance.checkSimpleInterpretedCodeBlock(codeBlock);
				isAnalyzed = true;
			}

			int returnAddress;
			if (isSimple)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final pspsharp.Allegrex.Common.Instruction[] insns = codeBlock.getInterpretedInstructions();
				Instruction[] insns = codeBlock.InterpretedInstructions;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[] opcodes = codeBlock.getInterpretedOpcodes();
				int[] opcodes = codeBlock.InterpretedOpcodes;
				for (int i = 0; i < insns.Length; i++)
				{
					insns[i].interpret(RuntimeContext.processor, opcodes[i]);
				}
				returnAddress = RuntimeContext.cpu._ra;
			}
			else
			{
				returnAddress = RuntimeContext.executeInterpreter(codeBlock.StartAddress);
			}

			return returnAddress;
		}

		public virtual IExecutable Executable
		{
			set
			{
				// Nothing to do
			}
			get
			{
				return null;
			}
		}

	}

}