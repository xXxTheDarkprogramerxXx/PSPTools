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

	public sealed class TPointer64 : TPointerBase
	{
		public TPointer64(Memory memory, int address) : base(memory, address, false)
		{
		}

		public TPointer64(Memory memory, int address, bool canBeNull) : base(memory, address, canBeNull)
		{
		}

		public long Value
		{
			get
			{
				return pointer.Value64;
			}
			set
			{
				if (canSetValue())
				{
					pointer.Value64 = value;
				}
			}
		}


		public long getValue(int offset)
		{
			return pointer.getValue64(offset);
		}

		public void setValue(int offset, long value)
		{
			if (canSetValue())
			{
				pointer.setValue64(offset, value);
			}
		}
	}

}