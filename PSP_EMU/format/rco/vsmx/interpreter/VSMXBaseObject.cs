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
namespace pspsharp.format.rco.vsmx.interpreter
{

	//using Logger = org.apache.log4j.Logger;

	public abstract class VSMXBaseObject
	{
		public static readonly Logger log = VSMX.log;
		protected internal const string lengthName = "Length";
		protected internal const string prototypeName = "prototype";
		protected internal const string callName = "call";
		protected internal VSMXInterpreter interpreter;

		public VSMXBaseObject(VSMXInterpreter interpreter)
		{
			Interpreter = interpreter;
		}

		protected internal virtual VSMXInterpreter Interpreter
		{
			set
			{
				this.interpreter = value;
			}
			get
			{
				return interpreter;
			}
		}


		public virtual VSMXBaseObject Value
		{
			get
			{
				return this;
			}
		}

		public virtual VSMXBaseObject getValueWithArguments(int numberOfArguments)
		{
			return Value;
		}

		public virtual float FloatValue
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public virtual int IntValue
		{
			get
			{
				return (int) FloatValue;
			}
		}

		public virtual bool BooleanValue
		{
			get
			{
				return FloatValue != 0f;
			}
		}

		public virtual string StringValue
		{
			get
			{
				return ToString();
			}
		}

		public virtual bool Equals(VSMXBaseObject value)
		{
			return FloatValue == value.FloatValue;
		}

		public virtual bool identity(VSMXBaseObject value)
		{
			return this == value;
		}

		public virtual VSMXBaseObject getPropertyValue(string name)
		{
			if (prototypeName.Equals(name))
			{
				VSMXObject prototype = Prototype;
				if (prototype != null)
				{
					return prototype;
				}
			}
			return VSMXUndefined.singleton;
		}

		public virtual VSMXBaseObject getPropertyValue(int index)
		{
			return getPropertyValue(Convert.ToString(index));
		}

		public virtual void setPropertyValue(string name, VSMXBaseObject value)
		{
		}

		public virtual void setPropertyValue(int index, VSMXBaseObject value)
		{
			setPropertyValue(Convert.ToString(index), value);
		}

		public virtual void deletePropertyValue(string name)
		{
			setPropertyValue(name, VSMXUndefined.singleton);
		}

		public virtual void deletePropertyValue(int index)
		{
			deletePropertyValue(Convert.ToString(index));
		}

		public virtual bool hasPropertyValue(string name)
		{
			return !VSMXUndefined.singleton.Equals(getPropertyValue(name));
		}

		public virtual IList<string> PropertyNames
		{
			get
			{
				return new LinkedList<string>();
			}
		}


		public abstract string typeOf();

		public abstract string ClassName {get;}

		protected internal virtual VSMXObject Prototype
		{
			get
			{
				string className = ClassName;
				if (string.ReferenceEquals(className, null))
				{
					return null;
				}
    
				if (!interpreter.GlobalVariables.hasPropertyValue(className))
				{
					return null;
				}
    
				VSMXBaseObject classObject = interpreter.GlobalVariables.getPropertyValue(className).Prototype;
				if (!(classObject is VSMXObject))
				{
					classObject = new VSMXObject(interpreter, className);
					interpreter.GlobalVariables.setPropertyValue(className, classObject);
				}
    
				return (VSMXObject) classObject;
			}
		}

		public virtual VSMXBaseObject ToString(VSMXBaseObject @object)
		{
			return new VSMXString(Interpreter, StringValue);
		}

		public virtual VSMXBaseObject ToString(VSMXBaseObject @object, VSMXBaseObject radix)
		{
			string s = Convert.ToString(IntValue, radix.IntValue);
			return new VSMXString(Interpreter, s);
		}

		public override string ToString()
		{
			if (FloatValue == (float) IntValue)
			{
				return Convert.ToString(IntValue);
			}
			return Convert.ToString(FloatValue);
		}
	}

}