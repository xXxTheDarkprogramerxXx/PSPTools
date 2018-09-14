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
	public class SceSysmemMemoryBlockInfo : pspAbstractMemoryMappedStructureVariableLength
	{
		public string name;
		public int attr;
		public int addr;
		public int memSize;
		public int sizeLocked;
		public int unused;

		protected internal override void read()
		{
			base.read();
			name = readStringNZ(32);
			attr = read32();
			addr = read32();
			memSize = read32();
			sizeLocked = read32();
			unused = read32();
		}

		protected internal override void write()
		{
			base.write();
			writeStringN(32, name);
			write32(attr);
			write32(addr);
			write32(memSize);
			write32(sizeLocked);
			write32(unused);
		}
	}

}