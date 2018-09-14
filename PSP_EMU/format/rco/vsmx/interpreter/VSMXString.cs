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
	public class VSMXString : VSMXBaseObject
	{
		private string value;

		public VSMXString(VSMXInterpreter interpreter, string value) : base(interpreter)
		{
			this.value = value;
		}

		public override float FloatValue
		{
			get
			{
				return Convert.ToSingle(value);
			}
			set
			{
				this.value = Convert.ToString(value);
			}
		}


		public override string StringValue
		{
			get
			{
				return value;
			}
		}

		public override VSMXBaseObject getPropertyValue(string name)
		{
			if (lengthName.Equals(name))
			{
				return new VSMXNumber(interpreter, value.Length);
			}

			return base.getPropertyValue(name);
		}

		public override bool Equals(VSMXBaseObject value)
		{
			return StringValue.Equals(value.StringValue);
		}

		public override bool identity(VSMXBaseObject value)
		{
			if (value is VSMXString)
			{
				return StringValue.Equals(value.StringValue);
			}
			return base.identity(value);
		}

		public override string typeOf()
		{
			return "string";
		}

		public override string ClassName
		{
			get
			{
				return "String";
			}
		}

		public virtual VSMXBaseObject toUpperCase(VSMXBaseObject @object)
		{
			return new VSMXString(Interpreter, StringValue.ToUpper());
		}

		public virtual VSMXBaseObject toLowerCase(VSMXBaseObject @object)
		{
			return new VSMXString(Interpreter, StringValue.ToLower());
		}

		public virtual VSMXBaseObject Substring(VSMXBaseObject @object, VSMXBaseObject start)
		{
			string s = StringValue;

			int beginIndex = start.IntValue;
			beginIndex = System.Math.Max(beginIndex, 0);
			beginIndex = System.Math.Min(beginIndex, s.Length);

			return new VSMXString(Interpreter, s.Substring(beginIndex));
		}

		public virtual VSMXBaseObject Substring(VSMXBaseObject @object, VSMXBaseObject start, VSMXBaseObject end)
		{
			string s = StringValue;

			int beginIndex = start.IntValue;
			beginIndex = System.Math.Max(beginIndex, 0);
			beginIndex = System.Math.Min(beginIndex, s.Length);

			int endIndex = end.IntValue;
			endIndex = System.Math.Max(endIndex, 0);
			endIndex = System.Math.Min(endIndex, s.Length);

			// The Substring method uses the lower value of start and end as the beginning point of the Substring.
			if (beginIndex > endIndex)
			{
				int tmp = beginIndex;
				beginIndex = endIndex;
				endIndex = tmp;
			}

			return new VSMXString(Interpreter, s.Substring(beginIndex, endIndex - beginIndex));
		}

		public virtual VSMXBaseObject lastIndexOf(VSMXBaseObject @object, VSMXBaseObject Substring)
		{
			return lastIndexOf(@object, Substring, new VSMXNumber(interpreter, @object.StringValue.Length));
		}

		public virtual VSMXBaseObject lastIndexOf(VSMXBaseObject @object, VSMXBaseObject Substring, VSMXBaseObject startIndex)
		{
			int startIndexInt = startIndex.IntValue;
			string substringString = Substring.StringValue;
			string s = StringValue;

			if (startIndexInt < 0)
			{
				startIndexInt = 0;
			}
			else if (startIndexInt > s.Length)
			{
				startIndexInt = s.Length;
			}

			int lastIndexOfInt = s.LastIndexOf(substringString, startIndexInt, StringComparison.Ordinal);

			return new VSMXNumber(interpreter, lastIndexOfInt);
		}

		public virtual VSMXBaseObject charAt(VSMXBaseObject @object, VSMXBaseObject index)
		{
			string s = StringValue;
			int i = index.IntValue;
			if (i < 0 || i >= s.Length)
			{
				return new VSMXString(interpreter, "");
			}

			char c = s[i];
			return new VSMXString(interpreter, "" + c);
		}

		public override string ToString()
		{
			return string.Format("\"{0}\"", value);
		}
	}

}