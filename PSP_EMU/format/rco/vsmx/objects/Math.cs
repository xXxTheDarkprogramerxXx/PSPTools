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

	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXInterpreter = pspsharp.format.rco.vsmx.interpreter.VSMXInterpreter;
	using VSMXNativeObject = pspsharp.format.rco.vsmx.interpreter.VSMXNativeObject;
	using VSMXNumber = pspsharp.format.rco.vsmx.interpreter.VSMXNumber;

	//using Logger = org.apache.log4j.Logger;

	public class Math : BaseNativeObject
	{
		private new static readonly Logger log = VSMX.log;
		public const string objectName = "Math";
		private VSMXInterpreter interpreter;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private System.Random random_Renamed;

		public static VSMXNativeObject create(VSMXInterpreter interpreter)
		{
			Math math = new Math(interpreter);
			VSMXNativeObject @object = new VSMXNativeObject(interpreter, math);
			math.Object = @object;

			@object.setPropertyValue("PI", new VSMXNumber(interpreter, (float) Math.PI));

			return @object;
		}

		private Math(VSMXInterpreter interpreter)
		{
			this.interpreter = interpreter;
			random_Renamed = new System.Random();
		}

		public virtual VSMXBaseObject random(VSMXBaseObject @object)
		{
			float value = random_Renamed.nextFloat();

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Math.random() returns {0:F}", value));
			}

			return new VSMXNumber(interpreter, value);
		}

		public virtual VSMXBaseObject floor(VSMXBaseObject @object, VSMXBaseObject value)
		{
			return new VSMXNumber(interpreter, (float) System.Math.Floor(value.FloatValue));
		}

		public virtual VSMXBaseObject abs(VSMXBaseObject @object, VSMXBaseObject value)
		{
			return new VSMXNumber(interpreter, (float) System.Math.Abs(value.FloatValue));
		}

		public virtual VSMXBaseObject sin(VSMXBaseObject @object, VSMXBaseObject value)
		{
			return new VSMXNumber(interpreter, (float) System.Math.Sin(value.FloatValue));
		}

		public virtual VSMXBaseObject cos(VSMXBaseObject @object, VSMXBaseObject value)
		{
			return new VSMXNumber(interpreter, (float) System.Math.Cos(value.FloatValue));
		}
	}

}