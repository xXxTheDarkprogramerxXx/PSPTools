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
	/// The Memory Stick boot system item structure.
	/// Based on information from
	/// https://github.com/torvalds/linux/blob/master/drivers/memstick/core/ms_block.h
	/// </summary>
	public class MemoryStickSystemItem : pspAbstractMemoryMappedStructure
	{
		public const int MS_SYSENT_TYPE_INVALID_BLOCK = 0x01;
		public const int MS_SYSENT_TYPE_CIS_IDI = 0x0A;
		public int startAddr;
		public int dataSize;
		public int dataTypeId;
		public readonly sbyte[] reserved = new sbyte[3];

		protected internal override bool BigEndian
		{
			get
			{
				return true;
			}
		}

		protected internal override void read()
		{
			startAddr = read32();
			dataSize = read32();
			dataTypeId = read8();
			read8Array(reserved);
		}

		protected internal override void write()
		{
			write32(startAddr);
			write32(dataSize);
			write8((sbyte) dataTypeId);
			write8Array(reserved);
		}

		public override int @sizeof()
		{
			return 12;
		}
	}

}