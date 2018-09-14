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
	/// The Memory Stick boot page structure.
	/// Based on information from
	/// https://github.com/torvalds/linux/blob/master/drivers/memstick/core/ms_block.h
	/// </summary>
	public class MemoryStickBootPage : pspAbstractMemoryMappedStructure
	{
		public readonly MemoryStickBootHeader header = new MemoryStickBootHeader();
		public readonly MemoryStickSystemEntry entry = new MemoryStickSystemEntry();
		public readonly MemoryStickBootAttributesInfo attr = new MemoryStickBootAttributesInfo();

		protected internal override bool BigEndian
		{
			get
			{
				return true;
			}
		}

		protected internal override void read()
		{
			read(header);
			read(entry);
			read(attr);
		}

		protected internal override void write()
		{
			write(header);
			write(entry);
			write(attr);
		}

		public override int @sizeof()
		{
			// Will return 512
			return header.@sizeof() + entry.@sizeof() + attr.@sizeof();
		}
	}

}