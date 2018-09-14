using System;

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
	public class CompilerClassLoader : ClassLoader
	{
		public static Logger log = Logger.getLogger("loader");
		private ICompiler compiler;

		public CompilerClassLoader(ICompiler compiler)
		{
			this.compiler = compiler;
		}

		public virtual Type defineClass(string name, sbyte[] b)
		{
			return defineClass(name, b, 0, b.Length);
		}

		public virtual Type defineClass(sbyte[] b)
		{
			return defineClass(null, b, 0, b.Length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected Class findClass(String name) throws ClassNotFoundException
		protected internal override Type findClass(string name)
		{
			// Check if the class has already been loaded
			Type loadedClass = findLoadedClass(name);

			if (loadedClass == null && compiler != null)
			{
				if (log.TraceEnabled)
				{
					log.trace("ClassLoader creating class " + name);
				}
				IExecutable executable = compiler.compile(name);
				if (executable != null)
				{
					loadedClass = executable.GetType();
				}
			}

			if (loadedClass == null)
			{
				loadedClass = base.findClass(name);
			}

			return loadedClass;
		}
	}

}