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
namespace pspsharp.format.rco.vsmx.interpreter
{
	using NativeFunctionFactory = pspsharp.format.rco.vsmx.objects.NativeFunctionFactory;

	public class VSMXMethod : VSMXBaseObject
	{
		private VSMXBaseObject @object;
		private string name;
		private VSMXBaseObject thisObject;
		private int numberOfArguments;
		private VSMXBaseObject[] arguments;

		public VSMXMethod(VSMXInterpreter interpreter, VSMXBaseObject @object, string name) : base(interpreter)
		{
			this.@object = @object;
			this.name = name;
		}

		public virtual VSMXBaseObject Object
		{
			get
			{
				return @object;
			}
		}

		public virtual string Name
		{
			get
			{
				return name;
			}
		}

		public virtual VSMXBaseObject ThisObject
		{
			get
			{
				return thisObject;
			}
		}

		public virtual VSMXBaseObject[] Arguments
		{
			get
			{
				return arguments;
			}
		}

		public virtual int NumberOfArguments
		{
			get
			{
				return numberOfArguments;
			}
		}

		public virtual VSMXFunction getFunction(int numberOfArguments, VSMXBaseObject[] arguments)
		{
			this.numberOfArguments = numberOfArguments;
			this.arguments = arguments;
			thisObject = @object.Value;

			if (@object.hasPropertyValue(name))
			{
				VSMXBaseObject function = @object.getPropertyValue(name).Value;
				if (function != null && function is VSMXFunction)
				{
					return (VSMXFunction) function;
				}
			}

			if (@object is VSMXFunction && callName.Equals(name))
			{
				// The first argument of the "call()" function call is the "this" object.
				if (numberOfArguments > 0)
				{
					this.numberOfArguments--;
					if (arguments.Length > 0)
					{
						thisObject = arguments[0];
						this.arguments = new VSMXBaseObject[this.numberOfArguments];
						Array.Copy(arguments, 1, this.arguments, 0, this.numberOfArguments);
					}
				}

				return (VSMXFunction) @object;
			}

			INativeFunction nativeFunction = null;
			if (@object is VSMXNativeObject)
			{
				VSMXNativeObject nativeObject = (VSMXNativeObject) @object;
				nativeFunction = NativeFunctionFactory.Instance.getNativeFunction(nativeObject, name, numberOfArguments);
			}
			else if (@object is VSMXBaseObject)
			{
				nativeFunction = NativeFunctionFactory.Instance.getNativeFunction(@object, name, numberOfArguments);
			}

			if (nativeFunction != null)
			{
				return new VSMXNativeFunction(interpreter, nativeFunction);
			}

			return null;
		}

		public virtual void call(VSMXBaseObject[] arguments)
		{
		}

		public override string typeOf()
		{
			return "function";
		}

		public override string ClassName
		{
			get
			{
				return name;
			}
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}()", @object, name);
		}
	}

}