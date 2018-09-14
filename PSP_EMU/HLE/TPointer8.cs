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
namespace pspsharp.HLE
{

	public class TPointer8 : TPointerBase
	{
		public TPointer8(Memory memory, int address) : base(memory, address, false)
		{
		}

		public TPointer8(Memory memory, int address, bool canBeNull) : base(memory, address, canBeNull)
		{
		}

		public virtual int Value
		{
			get
			{
				return pointer.Value8;
			}
			set
			{
				if (canSetValue())
				{
					pointer.Value8 = (sbyte) value;
				}
			}
		}


		public virtual int getValue(int offset)
		{
			return pointer.getValue8(offset);
		}

		public virtual void setValue(int offset, int value)
		{
			if (canSetValue())
			{
				pointer.setValue8(offset, (sbyte) value);
			}
		}
	}

}