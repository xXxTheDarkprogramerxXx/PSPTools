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
	using Utilities = pspsharp.util.Utilities;

	public class PspString
	{
		protected internal string @string;
		protected internal int address;
		protected internal int maxLength;
		protected internal bool canBeNull;

		public PspString(int address)
		{
			this.@string = null;
			this.address = address;
			this.maxLength = MemoryMap.SIZE_RAM; // Never will be greater than the whole PSP memory :P
		}

		public PspString(int address, int maxLength)
		{
			this.@string = null;
			this.address = address;
			this.maxLength = maxLength;
		}

		public PspString(int address, int maxLength, bool canBeNull)
		{
			this.@string = null;
			this.address = address;
			this.maxLength = maxLength;
			this.canBeNull = canBeNull;
		}

		public virtual string String
		{
			get
			{
				if (string.ReferenceEquals(@string, null))
				{
					if (canBeNull && Null)
					{
						@string = "";
					}
					else
					{
						@string = Utilities.readStringNZ(address, maxLength);
					}
				}
				return @string;
			}
		}

		public virtual int Address
		{
			get
			{
				return address;
			}
		}

		public virtual bool Null
		{
			get
			{
				return address == 0;
			}
		}

		public virtual bool NotNull
		{
			get
			{
				return address != 0;
			}
		}

		public override string ToString()
		{
			return string.Format("0x{0:X8}('{1}')", Address, string);
		}

		public virtual bool Equals(string s)
		{
			if (string.ReferenceEquals(s, null))
			{
				return Null;
			}

			if (Null)
			{
				return false;
			}

			return s.Equals(string);
		}
	}

}