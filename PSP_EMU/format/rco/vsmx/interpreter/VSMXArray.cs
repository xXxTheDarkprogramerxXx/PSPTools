using System;
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
namespace pspsharp.format.rco.vsmx.interpreter
{
	public class VSMXArray : VSMXObject
	{
		private const string className = "Array";
		private int Length;

		public VSMXArray(VSMXInterpreter interpreter) : base(interpreter, className)
		{
		}

		public VSMXArray(VSMXInterpreter interpreter, int size) : base(interpreter, className)
		{

			if (size > 0)
			{
				Length = size;
				for (int i = 0; i < size; i++)
				{
					create(i);
				}
			}
		}

		public virtual int Length
		{
			get
			{
				return Length;
			}
		}

		private void create(int index)
		{
			base.setPropertyValue(Convert.ToString(index), VSMXUndefined.singleton);
		}

		private void delete(int index)
		{
			base.deletePropertyValue(Convert.ToString(index));
		}

		private void updateLength(int index)
		{
			if (index >= Length)
			{
				for (int i = Length; i <= index; i++)
				{
					create(i);
				}
				Length = index + 1;
			}
		}

		public override VSMXBaseObject getPropertyValue(string name)
		{
			if (lengthName.Equals(name))
			{
				return new VSMXNumber(interpreter, Length);
			}

			int index = getIndex(name);
			if (index >= 0)
			{
				return getPropertyValue(index);
			}

			return base.getPropertyValue(name);
		}

		public override void setPropertyValue(string name, VSMXBaseObject value)
		{
			if (lengthName.Equals(name))
			{
				int newLength = value.IntValue;
				if (newLength > Length)
				{
					for (int i = Length; i < newLength; i++)
					{
						create(i);
					}
				}
				else if (newLength < Length)
				{
					for (int i = newLength; i < Length; i++)
					{
						delete(i);
					}
				}
				return;
			}

			int index = getIndex(name);
			if (index >= 0)
			{
				setPropertyValue(index, value);
			}
			else
			{
				base.setPropertyValue(name, value);
			}
		}

		public override void deletePropertyValue(string name)
		{
			if (lengthName.Equals(name))
			{
				// Cannot delete "Length" property
				return;
			}

			int index = getIndex(name);
			if (index >= 0)
			{
				deletePropertyValue(index);
			}
			else
			{
				base.deletePropertyValue(name);
			}
		}

		public override VSMXBaseObject getPropertyValue(int index)
		{
			if (index < 0)
			{
				return VSMXUndefined.singleton;
			}

			updateLength(index);

			return base.getPropertyValue(Convert.ToString(index));
		}

		public override void setPropertyValue(int index, VSMXBaseObject value)
		{
			if (index >= 0)
			{
				updateLength(index);
				base.setPropertyValue(Convert.ToString(index), value);
			}
		}

		public override void deletePropertyValue(int index)
		{
			if (index >= 0)
			{
				if (index == Length - 1)
				{ // Deleting the last element of the array?
					delete(index);
					Length = index;
				}
				else if (index < Length)
				{ // Deleting in the middle of the array?
					create(index);
				}
			}
		}

		public override bool hasPropertyValue(string name)
		{
			if (lengthName.Equals(name))
			{
				return true;
			}

			return base.hasPropertyValue(name);
		}

		public override bool BooleanValue
		{
			get
			{
				// "if" on an empty array seems to return false. E.g.
				//     x = {};
				//     if (x) { notexecuted; }
				return Length > 0;
			}
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			s.Append(string.Format("[Length={0:D}", Length));
			ToString(s);
			s.Append("]");

			return s.ToString();
		}

		public override bool Equals(VSMXBaseObject value)
		{
			if (value is VSMXArray)
			{
				// Empty arrays are always equal
				if (Length == 0 && ((VSMXArray) value).Length == 0)
				{
					return true;
				}
			}

			return base.Equals(value);
		}
	}

}