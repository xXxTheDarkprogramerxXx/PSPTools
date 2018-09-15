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
namespace pspsharp.format.rco.vsmx.objects
{

	//using Logger = org.apache.log4j.Logger;

	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXNativeObject = pspsharp.format.rco.vsmx.interpreter.VSMXNativeObject;

	public class NativeFunctionFactory
	{
		private static readonly Logger log = VSMX.log;
		private static NativeFunctionFactory singleton;

		private class NativeFunction : INativeFunction
		{
			internal Method method;
			internal object @object;
			internal int args;

			public NativeFunction(object @object, Method method, int args)
			{
				this.@object = @object;
				this.method = method;
				this.args = args;
			}

			public virtual int Args
			{
				get
				{
					return args;
				}
			}

			public virtual VSMXBaseObject call(VSMXBaseObject[] arguments)
			{
				VSMXBaseObject returnValue = null;
				try
				{
					object result = method.invoke(@object, (object[]) arguments);
					if (result is VSMXBaseObject)
					{
						returnValue = (VSMXBaseObject) result;
					}
				}
				catch (System.ArgumentException e)
				{
					Console.WriteLine("call", e);
				}
				catch (IllegalAccessException e)
				{
					Console.WriteLine("call", e);
				}
				catch (InvocationTargetException e)
				{
					Console.WriteLine("call", e);
				}
				return returnValue;
			}
		}

		public static NativeFunctionFactory Instance
		{
			get
			{
				if (singleton == null)
				{
					singleton = new NativeFunctionFactory();
				}
				return singleton;
			}
		}

		private NativeFunctionFactory()
		{
		}

		private INativeFunction getNativeFunctionInterface(object @object, string name, int numberOfArguments)
		{
			INativeFunction nativeFunction = null;

			// movieplayer.play has as much as 10 parameters
			for (int args = numberOfArguments + 1; args < 12; args++)
			{
				Type[] arguments = new Type[args];
				for (int i = 0; i < arguments.Length; i++)
				{
					arguments[i] = typeof(VSMXBaseObject);
				}
				try
				{
					Method method = @object.GetType().GetMethod(name, arguments);
					nativeFunction = new NativeFunction(@object, method, args - 1);
					break;
				}
				catch (SecurityException e)
				{
					Console.WriteLine("getNativeFunction", e);
				}
				catch (NoSuchMethodException)
				{
					// Ignore error
				}
			}

			if (nativeFunction == null && log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Not finding native function {0}.{1}(args={2:D})", @object, name, numberOfArguments + 1));
			}

			return nativeFunction;
		}

		public virtual INativeFunction getNativeFunction(VSMXNativeObject @object, string name, int numberOfArguments)
		{
			BaseNativeObject nativeObject = @object.Object;
			INativeFunction nativeFunction = getNativeFunctionInterface(nativeObject, name, numberOfArguments);

			return nativeFunction;
		}

		public virtual INativeFunction getNativeFunction(VSMXBaseObject @object, string name, int numberOfArguments)
		{
			INativeFunction nativeFunction = getNativeFunctionInterface(@object, name, numberOfArguments);

			return nativeFunction;
		}
	}

}