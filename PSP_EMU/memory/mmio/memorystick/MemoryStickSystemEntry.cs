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
	/// The Memory Stick boot system entry structure.
	/// Based on information from
	/// https://github.com/torvalds/linux/blob/master/drivers/memstick/core/ms_block.h
	/// </summary>
	public class MemoryStickSystemEntry : pspAbstractMemoryMappedStructure
	{
		public readonly MemoryStickSystemItem disabledBlock = new MemoryStickSystemItem();
		public readonly MemoryStickSystemItem cisIdi = new MemoryStickSystemItem();
		public readonly sbyte[] reserved = new sbyte[24];

		protected internal override bool BigEndian
		{
			get
			{
				return true;
			}
		}

		protected internal override void read()
		{
			read(disabledBlock);
			read(cisIdi);
			read8Array(reserved);
		}

		protected internal override void write()
		{
			write(disabledBlock);
			write(cisIdi);
			write8Array(reserved);
		}

		public override int @sizeof()
		{
			return disabledBlock.@sizeof() + cisIdi.@sizeof() + reserved.Length;
		}
	}

}