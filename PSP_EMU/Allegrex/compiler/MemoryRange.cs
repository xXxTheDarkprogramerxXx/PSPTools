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
namespace pspsharp.Allegrex.compiler
{
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class MemoryRange
	{
		private int address;
		private int rawAddress;
		private int length;
		private int[] values;

		public MemoryRange(int address, int length)
		{
			Address = address;
			Length = length;
		}

		public virtual int Address
		{
			get
			{
				return address;
			}
			set
			{
				this.address = value & Memory.addressMask;
				rawAddress = value;
			}
		}


		public virtual int Length
		{
			get
			{
				return length;
			}
			set
			{
				this.length = value;
			}
		}


		public virtual void updateValues()
		{
			values = new int[length >> 2];

			if (RuntimeContext.hasMemoryInt(address))
			{
				Array.Copy(RuntimeContext.MemoryInt, address >> 2, values, 0, values.Length);
			}
			else
			{
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(rawAddress, length, 4);
				for (int i = 0; i < values.Length; i++)
				{
					values[i] = memoryReader.readNext();
				}
			}
		}

		public virtual bool isOverlappingWithAddress(int address)
		{
			return this.address <= address && address < this.address + length;
		}

		public virtual void extendBottom(int size)
		{
			Address = address - size;
			length += size;
		}

		public virtual void extendTop(int size)
		{
			length += size;
		}

		public virtual int getValue(int address)
		{
			return values[(int)((uint)(address - this.address) >> 2)];
		}

		public virtual bool areValuesChanged()
		{
			if (RuntimeContext.hasMemoryInt(address))
			{
				// Optimized for the most common case (i.e. using memoryInt)
				int[] memoryInt = RuntimeContext.MemoryInt;
				int memoryIndex = address >> 2;
				for (int i = 0; i < values.Length; i++, memoryIndex++)
				{
					if (memoryInt[memoryIndex] != values[i])
					{
						return true;
					}
				}
			}
			else
			{
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(rawAddress, length, 4);
				for (int i = 0; i < values.Length; i++)
				{
					if (values[i] != memoryReader.readNext())
					{
						return true;
					}
				}
			}

			return false;
		}

		public virtual bool isOverlappingWithAddressRange(int address, int size)
		{
			// Address range is completely above or below our range: no overlap
			// E.g.:
			//                           [...MemoryRange...]
			//      [...address&size...]           or        [...address&size...]
			if (address >= this.address + length || address + size < this.address)
			{
				return false;
			}

			// The range begin or end is within our range: overlap
			// E.g.:
			//                    [...MemoryRange...]
			//      [...address&size...]  or   [...address&size...]
			if (isOverlappingWithAddress(address) || isOverlappingWithAddress(address + size))
			{
				return true;
			}

			// Range overlaps completely our range: overlap
			// E.g.:
			//         [...MemoryRange...]
			//      [.....address&size.....]
			if (address < this.address && address + size >= this.address + length)
			{
				return true;
			}

			// No overlap found
			return false;
		}

		public override string ToString()
		{
			return string.Format("[0x{0:X8}-0x{1:X8}]", rawAddress, rawAddress + length);
		}
	}

}