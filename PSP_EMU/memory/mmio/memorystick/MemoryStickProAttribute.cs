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
	/// The Memory Stick Pro attribute structure.
	/// Based on information from
	/// https://github.com/torvalds/linux/blob/master/drivers/memstick/core/mspro_block.c
	/// see "struct mspro_attribute".
	/// </summary>
	public class MemoryStickProAttribute : pspAbstractMemoryMappedStructure
	{
		public int signature;
		public int version;
		public int count;
		public readonly sbyte[] reserved = new sbyte[11];
		public readonly MemoryStickProAttributeEntry[] entries = new MemoryStickProAttributeEntry[12];

		public MemoryStickProAttribute()
		{
			for (int i = 0; i < entries.Length; i++)
			{
				entries[i] = new MemoryStickProAttributeEntry();
			}
		}

		protected internal override bool BigEndian
		{
			get
			{
				return true;
			}
		}

		protected internal override void read()
		{
			signature = read16();
			version = read16();
			count = read8();
			read8Array(reserved);
			for (int i = 0; i < entries.Length; i++)
			{
				read(entries[i]);
			}
		}

		protected internal override void write()
		{
			write16((short) signature);
			write16((short) version);
			write8((sbyte) count);
			write8Array(reserved);
			for (int i = 0; i < entries.Length; i++)
			{
				write(entries[i]);
			}
		}

		public override int @sizeof()
		{
			return 16 + entries.Length * MemoryStickProAttributeEntry.SIZE_OF;
		}
	}

}