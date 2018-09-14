using System.Collections.Generic;
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
namespace pspsharp.Allegrex.compiler
{

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class MemoryRanges
	{
		private IList<MemoryRange> ranges = new LinkedList<MemoryRange>();

		public virtual void addAddress(int rawAddress)
		{
			const int length = 4;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int address = rawAddress & pspsharp.Memory.addressMask;
			int address = rawAddress & Memory.addressMask;
			foreach (MemoryRange memoryRange in ranges)
			{
				// The most common case: the address is extending the top of the range
				if (address == memoryRange.Address + memoryRange.Length)
				{
					memoryRange.extendTop(length);
					return;
				}
				if (address == memoryRange.Address - length)
				{
					memoryRange.extendBottom(length);
					return;
				}
				if (memoryRange.isOverlappingWithAddress(address))
				{
					// Address already in the range, nothing more to do
					return;
				}
			}

			MemoryRange memoryRange = new MemoryRange(rawAddress, length);
			ranges.Add(memoryRange);
		}

		public virtual void updateValues()
		{
			foreach (MemoryRange memoryRange in ranges)
			{
				memoryRange.updateValues();
			}
		}

		public virtual bool areValuesChanged()
		{
			foreach (MemoryRange memoryRange in ranges)
			{
				if (memoryRange.areValuesChanged())
				{
					return true;
				}
			}

			return false;
		}

		public virtual bool isOverlappingWithAddressRange(int address, int size)
		{
			foreach (MemoryRange memoryRange in ranges)
			{
				if (memoryRange.isOverlappingWithAddressRange(address, size))
				{
					return true;
				}
			}

			return false;
		}

		public virtual int getValue(int address)
		{
			foreach (MemoryRange memoryRange in ranges)
			{
				if (memoryRange.isOverlappingWithAddress(address))
				{
					return memoryRange.getValue(address);
				}
			}

			return 0;
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();

			foreach (MemoryRange memoryRange in ranges)
			{
				if (s.Length > 0)
				{
					s.Append(", ");
				}
				s.Append(memoryRange.ToString());
			}

			return s.ToString();
		}
	}

}