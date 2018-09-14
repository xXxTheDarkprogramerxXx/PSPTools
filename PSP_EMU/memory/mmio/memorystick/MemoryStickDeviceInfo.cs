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
	/// The Memory Stick Pro devinfo attribute entry structure.
	/// Based on information from
	/// https://github.com/torvalds/linux/blob/master/drivers/memstick/core/mspro_block.c
	/// see "struct mspro_devinfo".
	/// </summary>
	public class MemoryStickDeviceInfo : pspAbstractMemoryMappedStructure
	{
		public int cylinders;
		public int heads;
		public int bytesPerTrack;
		public int bytesPerSector;
		public int sectorsPerTrack;
		public readonly sbyte[] reserved = new sbyte[6];

		protected internal override bool BigEndian
		{
			get
			{
				return true;
			}
		}

		protected internal override void read()
		{
			cylinders = read16();
			heads = read16();
			bytesPerTrack = read16();
			bytesPerSector = read16();
			sectorsPerTrack = read16();
			read8Array(reserved);
		}

		protected internal override void write()
		{
			write16((short) cylinders);
			write16((short) heads);
			write16((short) bytesPerTrack);
			write16((short) bytesPerSector);
			write16((short) sectorsPerTrack);
			write8Array(reserved);
		}

		public override int @sizeof()
		{
			return 16;
		}
	}

}