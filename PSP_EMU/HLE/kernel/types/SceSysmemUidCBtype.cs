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
	using Utilities = pspsharp.util.Utilities;

	public class SceSysmemUidCBtype : pspAbstractMemoryMappedStructure
	{
		public int parent0;
		public int nextChild;
		public int meta;
		public int uid;
		public int nameAddr;
		public string name;
		public int childSize;
		public int size;
		public int attr;
		public int next;
		public int parent1;
		public int funcTable;

		protected internal override void read()
		{
			parent0 = read32(); // Offset 0
			nextChild = read32(); // Offset 4
			meta = read32(); // Offset 8
			uid = read32(); // Offset 12
			nameAddr = read32(); // Offset 16
			if (nameAddr == 0)
			{
				name = null;
			}
			else
			{
				name = readStringZ(nameAddr);
			}
			childSize = read8(); // Offset 20
			size = read8(); // Offset 21
			attr = read16(); // Offset 22
			next = read32(); // Offset 24
			parent1 = read32(); // Offset 28
			funcTable = read32(); // Offset 32
		}

		protected internal override void write()
		{
			write32(parent0); // Offset 0
			write32(nextChild); // Offset 4
			write32(meta); // Offset 8
			write32(uid); // Offset 12
			write32(nameAddr); // Offset 16
			write8((sbyte) childSize); // Offset 20
			write8((sbyte) size); // Offset 21
			write16((short) attr); // Offset 22
			write32(next); // Offset 24
			write32(parent1); // Offset 28
			write32(funcTable); // Offset 32
		}

		public virtual void allocAndSetName(int uidHeap, string name)
		{
			this.name = name;
			if (string.ReferenceEquals(name, null))
			{
				nameAddr = 0;
			}
			else
			{
				nameAddr = Modules.SysMemForKernelModule.sceKernelAllocHeapMemory(uidHeap, name.Length + 1);
				if (nameAddr < 0)
				{
					nameAddr = 0;
				}
				else if (nameAddr != 0)
				{
					Utilities.writeStringZ(mem, nameAddr, name);
				}
			}
		}

		public override int @sizeof()
		{
			return 36;
		}
	}

}