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
namespace pspsharp.memory.mmio.memorystick
{
	using pspAbstractMemoryMappedStructure = pspsharp.HLE.kernel.types.pspAbstractMemoryMappedStructure;

	/// <summary>
	/// The Memory Stick Pro MBR attribute entry structure.
	/// Based on information from
	/// https://github.com/torvalds/linux/blob/master/drivers/memstick/core/mspro_block.c
	/// see "struct mspro_mbr".
	/// </summary>
	public class MemoryStickMbr : pspAbstractMemoryMappedStructure
	{
		public int bootPartition;
		public int startHead;
		public int startSector;
		public int startCylinder;
		public int partitionType;
		public int endHead;
		public int endSector;
		public int endCylinder;
		public int startSectors;
		public int sectorsPerPartition;

		protected internal override void read()
		{
			bootPartition = read8(); // Offset 0
			startHead = read8(); // Offset 1
			startSector = read8(); // Offset 2
			startCylinder = read8(); // Offset 3
			partitionType = read8(); // Offset 4
			endHead = read8(); // Offset 5
			endSector = read8(); // Offset 6
			endCylinder = read8(); // Offset 7
			startSectors = read32(); // Offset 8
			sectorsPerPartition = read32(); // Offset 12
		}

		protected internal override void write()
		{
			write8((sbyte) bootPartition);
			write8((sbyte) startHead);
			write8((sbyte) startSector);
			write8((sbyte) startCylinder);
			write8((sbyte) partitionType);
			write8((sbyte) endHead);
			write8((sbyte) endSector);
			write8((sbyte) endCylinder);
			write32(startSectors);
			write32(sectorsPerPartition);
		}

		public override int @sizeof()
		{
			return 16;
		}
	}

}