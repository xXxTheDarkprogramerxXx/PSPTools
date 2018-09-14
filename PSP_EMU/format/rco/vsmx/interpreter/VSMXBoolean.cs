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
	public class VSMXBoolean : VSMXBaseObject
	{
		public static readonly VSMXBoolean singletonTrue = new VSMXBoolean(true);
		public static readonly VSMXBoolean singletonFalse = new VSMXBoolean(false);
		private bool value;

		private VSMXBoolean(bool value) : base(null)
		{
			this.value = value;
		}

		public static void init(VSMXInterpreter interpreter)
		{
			singletonTrue.Interpreter = interpreter;
			singletonFalse.Interpreter = interpreter;
		}

		public static VSMXBoolean getValue(bool value)
		{
			return value ? singletonTrue : singletonFalse;
		}

		public static VSMXBoolean getValue(int value)
		{
			return getValue(value != 0);
		}

		public override float FloatValue
		{
			get
			{
				return value ? 1f : 0f;
			}
		}

		public override int IntValue
		{
			get
			{
				return value ? 1 : 0;
			}
		}

		public override bool BooleanValue
		{
			get
			{
				return value;
			}
		}

		public override string typeOf()
		{
			return "boolean";
		}

		public override string ClassName
		{
			get
			{
				return "Boolean";
			}
		}

		public override string ToString()
		{
			return value ? "true" : "false";
		}
	}

}