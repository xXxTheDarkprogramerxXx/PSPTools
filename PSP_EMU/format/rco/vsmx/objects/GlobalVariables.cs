using System.Text;

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

	using VSMXArray = pspsharp.format.rco.vsmx.interpreter.VSMXArray;
	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXBoolean = pspsharp.format.rco.vsmx.interpreter.VSMXBoolean;
	using VSMXInterpreter = pspsharp.format.rco.vsmx.interpreter.VSMXInterpreter;
	using VSMXNativeObject = pspsharp.format.rco.vsmx.interpreter.VSMXNativeObject;
	using VSMXNumber = pspsharp.format.rco.vsmx.interpreter.VSMXNumber;
	using VSMXObject = pspsharp.format.rco.vsmx.interpreter.VSMXObject;
	using VSMXUndefined = pspsharp.format.rco.vsmx.interpreter.VSMXUndefined;
	using Screen = pspsharp.hardware.Screen;

	public class GlobalVariables : BaseNativeObject
	{
		private new static readonly Logger log = VSMX.log;
		private static readonly Logger logWriteln = Logger.getLogger("writeln");
		private StringBuilder writeBuffer = new StringBuilder();

		public static VSMXNativeObject create(VSMXInterpreter interpreter)
		{
			GlobalVariables globalVariables = new GlobalVariables(interpreter);
			VSMXNativeObject @object = new VSMXNativeObject(interpreter, globalVariables);
			globalVariables.Object = @object;

			@object.setPropertyValue("undefined", VSMXUndefined.singleton);
			@object.setPropertyValue("Array", new VSMXArray(interpreter));
			@object.setPropertyValue("Object", new VSMXObject(interpreter, null));

			@object.setPropertyValue("timer", Timer.create(interpreter));

			@object.setPropertyValue("x", new VSMXNumber(interpreter, 0));
			@object.setPropertyValue("y", new VSMXNumber(interpreter, 0));
			@object.setPropertyValue("width", new VSMXNumber(interpreter, Screen.width));
			@object.setPropertyValue("height", new VSMXNumber(interpreter, Screen.height));

			return @object;
		}

		private GlobalVariables(VSMXInterpreter interpreter)
		{
		}

		private void writeln()
		{
			logWriteln.debug(writeBuffer.ToString());
			writeBuffer.Length = 0;
		}

		private void writeln(VSMXBaseObject @object, params VSMXBaseObject[] strings)
		{
			write(@object, strings);
			writeln();
		}

		private void write(VSMXBaseObject @object, params VSMXBaseObject[] strings)
		{
			for (int i = 0; i < strings.Length; i++)
			{
				writeBuffer.Append(strings[i].StringValue);
			}
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("write: '{0}'", writeBuffer.ToString()));
			}
		}

		public virtual void write(VSMXBaseObject @object, VSMXBaseObject s1)
		{
			write(@object, new VSMXBaseObject[] {s1});
		}

		public virtual void writeln(VSMXBaseObject @object)
		{
			writeln();
		}

		public virtual void writeln(VSMXBaseObject @object, VSMXBaseObject s1)
		{
			writeln(@object, new VSMXBaseObject[] {s1});
		}

		public virtual void writeln(VSMXBaseObject @object, VSMXBaseObject s1, VSMXBaseObject s2)
		{
			writeln(@object, new VSMXBaseObject[] {s1, s2});
		}

		public virtual void writeln(VSMXBaseObject @object, VSMXBaseObject s1, VSMXBaseObject s2, VSMXBaseObject s3)
		{
			writeln(@object, new VSMXBaseObject[] {s1, s3});
		}

		public virtual void writeln(VSMXBaseObject @object, VSMXBaseObject s1, VSMXBaseObject s2, VSMXBaseObject s3, VSMXBaseObject s4)
		{
			writeln(@object, new VSMXBaseObject[] {s1, s3, s4});
		}

		public virtual void writeln(VSMXBaseObject @object, VSMXBaseObject s1, VSMXBaseObject s2, VSMXBaseObject s3, VSMXBaseObject s4, VSMXBaseObject s5)
		{
			writeln(@object, new VSMXBaseObject[] {s1, s3, s4, s5});
		}

		public virtual void writeln(VSMXBaseObject @object, VSMXBaseObject s1, VSMXBaseObject s2, VSMXBaseObject s3, VSMXBaseObject s4, VSMXBaseObject s5, VSMXBaseObject s6)
		{
			writeln(@object, new VSMXBaseObject[] {s1, s3, s4, s5, s6});
		}

		public virtual VSMXBaseObject parseFloat(VSMXBaseObject @object, VSMXBaseObject value)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("parseFloat: {0}", value));
			}

			return new VSMXNumber(@object.Interpreter, value.FloatValue);
		}

		public virtual VSMXBaseObject parseInt(VSMXBaseObject @object, VSMXBaseObject value)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("parseInt: {0}", value));
			}

			return new VSMXNumber(@object.Interpreter, value.IntValue);
		}

		public virtual VSMXBaseObject isNaN(VSMXBaseObject @object, VSMXBaseObject value)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("isNaN: {0}", value));
			}

			bool isNaN = float.IsNaN(value.FloatValue);

			return VSMXBoolean.getValue(isNaN);
		}

		public virtual VSMXBaseObject Float(VSMXBaseObject @object, VSMXBaseObject value)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Float: {0}", value));
			}

			return new VSMXNumber(@object.Interpreter, value.FloatValue);
		}

		public virtual VSMXBaseObject Int(VSMXBaseObject @object, VSMXBaseObject value)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Int: {0}", value));
			}

			return new VSMXNumber(@object.Interpreter, value.IntValue);
		}
	}

}