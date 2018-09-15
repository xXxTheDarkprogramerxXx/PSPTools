using System;
using System.Collections.Generic;
using System.Reflection;

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
namespace pspsharp.util
{

	//using Logger = org.apache.log4j.Logger;
	using ClassReader = org.objectweb.asm.ClassReader;
	using ClassNode = org.objectweb.asm.tree.ClassNode;
	using LocalVariableNode = org.objectweb.asm.tree.LocalVariableNode;
	using MethodNode = org.objectweb.asm.tree.MethodNode;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class ClassAnalyzer
	{
		private static Logger log = Logger.getLogger("classAnalyzer");

		public class ParameterInfo
		{
			public readonly string name;
			public readonly Type type;

			public ParameterInfo(string name, Type type)
			{
				this.name = name;
				this.type = type;
			}
		}

		public virtual ParameterInfo[] getParameters(string methodName, Type c)
		{
			ParameterInfo[] parameters = null;

			try
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				ClassReader cr = new ClassReader(c.FullName.Replace('.', '/'));
				Method[] methods = c.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
				if (methods != null)
				{
					AnalyzerClassVisitor cn = null;
					for (int i = 0; i < methods.Length; i++)
					{
						if (methodName.Equals(methods[i].Name))
						{
							cn = new AnalyzerClassVisitor(methods[i]);
							break;
						}
					}

					if (cn != null)
					{
						cr.accept(cn, 0);
						parameters = cn.Parameters;
					}
				}
			}
			catch (IOException e)
			{
				Console.WriteLine("Cannot read class", e);
			}

			return parameters;
		}

		private class AnalyzerClassVisitor : ClassNode
		{
			internal ParameterInfo[] parameters = null;
			internal string methodName;
			internal Method method;

			public AnalyzerClassVisitor(Method method)
			{
				this.method = method;
				this.methodName = method.Name;
			}

			public virtual ParameterInfo[] Parameters
			{
				get
				{
					return parameters;
				}
			}

			public override void visitEnd()
			{
				// Visit all the methods
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.Iterator<?> it = methods.iterator(); it.hasNext();)
				for (IEnumerator<object> it = methods.GetEnumerator(); it.MoveNext();)
				{
					MethodNode method = (MethodNode) it.Current;
					if (methodName.Equals(method.name))
					{
						visitMethod(method);
					}
				}
			}

			internal virtual void visitMethod(MethodNode methodNode)
			{
				// First parameter is "this" for non-static methods
				int firstIndex = Modifier.isStatic(method.Modifiers) ? 0 : 1;
				Type[] parameterTypes = method.ParameterTypes;
				int numberParameters = System.Math.Min(parameterTypes.Length, methodNode.localVariables.size() - firstIndex);

				Dictionary<int, int> parameterIndices = new Dictionary<int, int>();
				for (int i = 0, currentIndex = firstIndex; i < numberParameters; i++, currentIndex++)
				{
					parameterIndices[currentIndex] = i;
					if (parameterTypes[i] == typeof(long) || parameterTypes[i] == typeof(double))
					{
						currentIndex++;
					}
				}

				parameters = new ParameterInfo[numberParameters];
				for (int i = 0; i < methodNode.localVariables.size(); i++)
				{
					LocalVariableNode localVariableNode = (LocalVariableNode) methodNode.localVariables.get(i);
					if (parameterIndices.ContainsKey(localVariableNode.index))
					{
						int parameterIndex = parameterIndices[localVariableNode.index];
						parameters[parameterIndex] = new ParameterInfo(localVariableNode.name, parameterTypes[parameterIndex]);
					}
				}
			}
		}
	}

}