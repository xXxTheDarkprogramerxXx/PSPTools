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
namespace pspsharp.HLE.kernel.types
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.alignUp;

	public class MemoryChunk
	{
		// Start address of this MemoryChunk
		public int addr;
		// Size of this MemoryChunk: it extends from addr to (addr + size -1)
		public int size;
		// The MemoryChunk are kept sorted by addr and linked with next/previous
		// The MemoryChunk with the lowest addr has previous == null
		// The MemoryChunk with the highest addr has next == null
		public MemoryChunk next;
		public MemoryChunk previous;

		public MemoryChunk(int addr, int size)
		{
			this.addr = addr;
			this.size = size;
		}

		/// <summary>
		/// Check if the memoryChunk has enough space to allocate a block.
		/// </summary>
		/// <param name="requestedSize"> size of the requested block </param>
		/// <param name="addrAlignment"> base address alignment of the requested block </param>
		/// <returns>              true if the chunk is large enough to allocate the block
		///                      false if the chunk is too small for the requested block </returns>
		public virtual bool isAvailable(int requestedSize, int addrAlignment)
		{
			// Quick check on requested size
			if (requestedSize > size)
			{
				return false;
			}

			if (alignUp(addr, addrAlignment) + requestedSize <= addr + size)
			{
				return true;
			}

			return false;
		}

		public override string ToString()
		{
			return string.Format("[addr=0x{0:X8}-0x{1:X8}, size=0x{2:X}]", addr, addr + size, size);
		}
	}

}