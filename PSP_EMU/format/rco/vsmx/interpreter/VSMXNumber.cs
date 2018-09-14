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
	public class VSMXNumber : VSMXBaseObject
	{
		private float value;

		public VSMXNumber(VSMXInterpreter interpreter, float value) : base(interpreter)
		{
			this.value = value;
		}

		public VSMXNumber(VSMXInterpreter interpreter, int value) : base(interpreter)
		{
			this.value = (float) value;
		}

		public override float FloatValue
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}

		public override string typeOf()
		{
			return "number";
		}

		public override string ClassName
		{
			get
			{
				return "Number";
			}
		}


		public override bool identity(VSMXBaseObject value)
		{
			if (value is VSMXNumber)
			{
				return FloatValue == value.FloatValue;
			}

			return base.identity(value);
		}
	}

}