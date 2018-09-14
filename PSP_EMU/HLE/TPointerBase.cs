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

	public abstract class TPointerBase : ITPointerBase
	{
		protected internal TPointer pointer;
		private bool canBeNull;

		protected internal TPointerBase()
		{
			pointer = TPointer.NULL;
			canBeNull = true;
		}

		protected internal TPointerBase(Memory memory, int address, bool canBeNull)
		{
			pointer = new TPointer(memory, address);
			this.canBeNull = canBeNull;
		}

		public virtual bool AddressGood
		{
			get
			{
				return Memory.isAddressGood(pointer.Address);
			}
		}

		public virtual bool isAlignedTo(int offset)
		{
			return pointer.isAlignedTo(offset);
		}

		public virtual int Address
		{
			get
			{
				return pointer.Address;
			}
		}

		public virtual bool Null
		{
			get
			{
				return pointer.Null;
			}
		}

		public virtual bool NotNull
		{
			get
			{
				return pointer.NotNull;
			}
		}

		public virtual Memory Memory
		{
			get
			{
				return pointer.Memory;
			}
		}

		/// <summary>
		/// Tests if the value can be set.
		/// A value can be set if the pointer cannot be NULL or is not NULL.
		/// A value can be ignored if the pointer can be NULL and is NULL.
		/// </summary>
		/// <returns> true  if the value can be set
		///         false if the value can be ignored </returns>
		protected internal virtual bool canSetValue()
		{
			return !canBeNull || NotNull;
		}

		public override string ToString()
		{
			return string.Format("0x{0:X8}", Address);
		}
	}

}