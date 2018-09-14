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

	public class VSMXReference : VSMXBaseObject
	{
		private VSMXObject refObject;
		private string refProperty;
		private int refIndex;

		public VSMXReference(VSMXInterpreter interpreter, VSMXObject refObject, string refProperty) : base(interpreter)
		{
			this.refObject = refObject;
			this.refProperty = refProperty;
		}

		public VSMXReference(VSMXInterpreter interpreter, VSMXObject refObject, int refIndex) : base(interpreter)
		{
			this.refObject = refObject;
			this.refIndex = refIndex;
		}

		public virtual void assign(VSMXBaseObject value)
		{
			if (string.ReferenceEquals(refProperty, null))
			{
				refObject.setPropertyValue(refIndex, value.Value);
			}
			else
			{
				refObject.setPropertyValue(refProperty, value.Value);
			}
		}

		public virtual string RefProperty
		{
			get
			{
				if (string.ReferenceEquals(refProperty, null))
				{
					return Convert.ToString(refIndex);
				}
    
				return refProperty;
			}
		}

		protected internal virtual VSMXBaseObject Ref
		{
			get
			{
				return getRef(0);
			}
		}

		protected internal virtual VSMXBaseObject getRef(int numberOfArguments)
		{
			if (string.ReferenceEquals(refProperty, null))
			{
				return refObject.getPropertyValue(refIndex);
			}

			if (!refObject.hasPropertyValue(refProperty) && refObject is VSMXNativeObject)
			{
				VSMXNativeObject nativeObject = (VSMXNativeObject) refObject;
				INativeFunction nativeFunction = NativeFunctionFactory.Instance.getNativeFunction(nativeObject, refProperty, numberOfArguments);
				if (nativeFunction != null)
				{
					return new VSMXNativeFunction(interpreter, nativeFunction);
				}
			}

			return refObject.getPropertyValue(refProperty);
		}

		public override VSMXBaseObject Value
		{
			get
			{
				return Ref;
			}
		}

		public override VSMXBaseObject getValueWithArguments(int numberOfArguments)
		{
			return getRef(numberOfArguments);
		}

		public override float FloatValue
		{
			get
			{
				return Ref.FloatValue;
			}
			set
			{
				Ref.FloatValue = value;
			}
		}

		public override int IntValue
		{
			get
			{
				return Ref.IntValue;
			}
		}

		public override bool BooleanValue
		{
			get
			{
				return Ref.BooleanValue;
			}
		}

		public override string StringValue
		{
			get
			{
				return Ref.StringValue;
			}
		}

		public override bool Equals(VSMXBaseObject value)
		{
			return Ref.Equals(value);
		}

		public override VSMXBaseObject getPropertyValue(string name)
		{
			return Ref.getPropertyValue(name);
		}

		public override void setPropertyValue(string name, VSMXBaseObject value)
		{
			Ref.setPropertyValue(name, value);
		}

		public override void deletePropertyValue(string name)
		{
			Ref.deletePropertyValue(name);
		}


		public override string typeOf()
		{
			return Ref.typeOf();
		}

		public override string ClassName
		{
			get
			{
				return Ref.ClassName;
			}
		}

		public override string ToString()
		{
			if (string.ReferenceEquals(refProperty, null))
			{
				return string.Format("@OBJ[{0:D}]={1}", refIndex, Ref);
			}
			return string.Format("@OBJ.{0}={1}", refProperty, Ref);
		}
	}

}