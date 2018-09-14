using System;
using System.Collections.Generic;

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

	using Type = org.objectweb.asm.Type;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class CompilerTypeManager
	{
		private Dictionary<Type, CompilerTypeInformation> compilerTypeInformations;
		private CompilerTypeInformation defaultCompilerTypeInformation;

		public CompilerTypeManager()
		{
			compilerTypeInformations = new Dictionary<Type, CompilerTypeInformation>();
			defaultCompilerTypeInformation = new CompilerTypeInformation(null, null, "%s");

			addCompilerTypeInformation(typeof(int), new CompilerTypeInformation(Type.getInternalName(typeof(Integer)), "(I)V", "0x%X"));
			addCompilerTypeInformation(typeof(bool), new CompilerTypeInformation(Type.getInternalName(typeof(Boolean)), "(Z)V", "%b"));
			addCompilerTypeInformation(typeof(long), new CompilerTypeInformation(Type.getInternalName(typeof(Long)), "(J)V", "0x%X"));
			addCompilerTypeInformation(typeof(short), new CompilerTypeInformation(Type.getInternalName(typeof(Short)), "(S)V", "0x%X"));
			addCompilerTypeInformation(typeof(float), new CompilerTypeInformation(Type.getInternalName(typeof(Float)), "(F)V", "%f"));
			addCompilerTypeInformation(typeof(string), new CompilerTypeInformation(null, null, "'%s'"));
		}

		private void addCompilerTypeInformation(Type type, CompilerTypeInformation compilerTypeInformation)
		{
			compilerTypeInformations[type] = compilerTypeInformation;
		}

		public virtual CompilerTypeInformation getCompilerTypeInformation(Type type)
		{
			CompilerTypeInformation compilerTypeInformation = compilerTypeInformations[type];
			if (compilerTypeInformation == null)
			{
				compilerTypeInformation = defaultCompilerTypeInformation;
			}

			return compilerTypeInformation;
		}
	}

}