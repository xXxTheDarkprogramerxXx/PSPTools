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
	using Instruction = pspsharp.Allegrex.Common.Instruction;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class CachedInterpreter : AbstractNativeCodeSequence
	{
		private static Instruction[] instructions;
		private static int[] opcodes;

		private class UpdateOpcodesAction : IAction
		{
			internal int address;
			internal int length;
			internal int[] opcodes;
			internal int offset;

			public UpdateOpcodesAction(int address, int length, int[] opcodes, int offset)
			{
				this.address = address;
				this.length = length;
				this.opcodes = opcodes;
				this.offset = offset;
			}

			public virtual void execute()
			{
				// Re-read the opcodes that have been updated by the application
				Utilities.readInt32(address, length, opcodes, offset);
			}
		}

		/*
		 * This method is interpreting a code sequence but caching the decoded instructions.
		 * No jumps or branches are allowed in the code sequence.
		 */
		public static void call(int numberInstructions, int codeBlockContextSize)
		{
			// First time being called?
			if (instructions == null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int startAddress = getPc();
				int startAddress = Pc;

				// Read the opcodes
				opcodes = Utilities.readInt32(startAddress, numberInstructions << 2);

				// Decode the opcodes into instructions
				instructions = new Instruction[numberInstructions];
				for (int i = 0; i < numberInstructions; i++)
				{
					instructions[i] = Decoder.instruction(opcodes[i]);
				}

				// Search for the code block including this sequence (search backwards)
				CodeBlock codeBlock = null;
				for (int i = 0; i < codeBlockContextSize && codeBlock == null; i++)
				{
					codeBlock = RuntimeContext.getCodeBlock(startAddress - (i << 2));
				}

				// Define the action that need to be executed when the compiler has detected
				// that the opcodes have been modified by the application.
				// In that case, the "opcodes" array need to be updated.
				if (codeBlock != null)
				{
					codeBlock.UpdateOpcodesAction = new UpdateOpcodesAction(startAddress, numberInstructions << 2, opcodes, 0);
				}
				else
				{
					log.error(string.Format("CachedInterpreter: could not find the CodeBlock 0x{0:X8}", startAddress));
				}
			}

			// Interpret the decoded instructions
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final pspsharp.Processor processor = getProcessor();
			Processor processor = Processor;
			for (int i = 0; i < numberInstructions; i++)
			{
				instructions[i].interpret(processor, opcodes[i]);
			}
		}
	}

}