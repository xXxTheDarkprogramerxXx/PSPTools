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
	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class RecompileExecutable : InvalidatedExecutable
	{
		private CodeBlock codeBlock;

		public RecompileExecutable(CodeBlock codeBlock) : base(codeBlock)
		{
			this.codeBlock = codeBlock;
		}

		/* (non-Javadoc)
		 * @see pspsharp.Allegrex.compiler.IExecutable#exec(int, int, boolean)
		 * 
		 * Recompile the codeBlock and set its runtime executable to the recompiled
		 * executable.
		 */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int exec() throws Exception
		public override int exec()
		{
			// Recompile the codeBlock
			int newInstanceIndex = codeBlock.NewInstanceIndex;
			IExecutable executable = Compiler.Instance.compile(codeBlock.StartAddress, newInstanceIndex);

			// Set the executable used at runtime to the recompiled executable.
			codeBlock.Executable.Executable = executable;

			// Execute the recompiled executable
			return executable.exec();
		}
	}

}