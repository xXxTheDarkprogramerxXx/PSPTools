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

	public class TPointer32 : TPointerBase
	{
		public static readonly TPointer32 NULL = new TPointer32();

		private TPointer32() : base()
		{
		}

		public TPointer32(Memory memory, int address) : base(memory, address, false)
		{
		}

		public TPointer32(Memory memory, int address, bool canBeNull) : base(memory, address, canBeNull)
		{
		}

		public virtual int getValue()
		{
			return getValue(0);
		}

		public virtual void setValue(int value)
		{
			setValue(0, value);
		}

		public virtual void setValue(bool value)
		{
			setValue(0, value);
		}

		public virtual int getValue(int offset)
		{
			return pointer.getValue32(offset);
		}

		public virtual void setValue(int offset, int value)
		{
			if (canSetValue())
			{
				pointer.setValue32(offset, value);
			}
		}

		public virtual void setValue(int offset, bool value)
		{
			if (canSetValue())
			{
				pointer.setValue32(offset, value);
			}
		}

		public virtual TPointer Pointer
		{
			get
			{
				return getPointer(0);
			}
		}

		public virtual TPointer getPointer(int offset)
		{
			if (Null)
			{
				return TPointer.NULL;
			}

			return new TPointer(Memory, getValue(offset));
		}
	}

}