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
	using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public abstract class InvalidatedExecutable : IExecutable
	{
		public abstract int exec();
		protected internal static Logger log = Compiler.log;
		private IExecutable executable;

		protected internal InvalidatedExecutable(CodeBlock codeBlock)
		{
			executable = codeBlock.Executable.Executable;
			while (executable != null && executable is InvalidatedExecutable)
			{
				executable = executable.Executable;
			}
		}

		public virtual IExecutable Executable
		{
			set
			{
				// Nothing to do
			}
			get
			{
				return executable;
			}
		}

	}

}