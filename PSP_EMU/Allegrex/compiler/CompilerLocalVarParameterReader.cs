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
	/// An implementation of CompilerParameterReader reading the first 8 parameters
	/// ($a0..$a3, $t0..$t3) from Java local variables.
	/// 
	/// @author gid15
	/// </summary>
	public class CompilerLocalVarParameterReader : CompilerParameterReader
	{
		private int localVarIndex;

		public CompilerLocalVarParameterReader(ICompilerContext compilerContext, int localVarIndex) : base(compilerContext)
		{

			this.localVarIndex = localVarIndex;
		}

		protected internal override void loadParameterIntFromRegister(int index)
		{
			compilerContext.loadLocalVar(localVarIndex + index);
		}
	}

}