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
	/// The Memory Stick boot header structure.
	/// Based on information from
	/// https://github.com/torvalds/linux/blob/master/drivers/memstick/core/ms_block.h
	/// </summary>
	public class MemoryStickBootHeader : pspAbstractMemoryMappedStructure
	{
		public const int MS_BOOT_BLOCK_ID = 0x0001;
		public const int MS_BOOT_BLOCK_FORMAT_VERSION = 0x0100;
		public const int MS_BOOT_BLOCK_DATA_ENTRIES = 2;
		public int blockId;
		public int formatVersion;
		public readonly sbyte[] reserved0 = new sbyte[184];
		public int numberOfDataEntry;
		public readonly sbyte[] reserved1 = new sbyte[179];

		protected internal override bool BigEndian
		{
			get
			{
				return true;
			}
		}

		protected internal override void read()
		{
			blockId = read16();
			formatVersion = read16();
			read8Array(reserved0);
			numberOfDataEntry = read8();
			read8Array(reserved1);
		}

		protected internal override void write()
		{
			write16((short) blockId);
			write16((short) formatVersion);
			write8Array(reserved0);
			write8((sbyte) numberOfDataEntry);
			write8Array(reserved1);
		}

		public override int @sizeof()
		{
			return 368;
		}
	}

}