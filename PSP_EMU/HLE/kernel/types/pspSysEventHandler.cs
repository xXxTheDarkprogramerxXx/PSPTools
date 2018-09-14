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
	public class pspSysEventHandler : pspAbstractMemoryMappedStructure
	{
		public int size;
		public int nameAddr;
		public string name;
		public int typeMask;
		public int handlerAddr;
		public int gp;
		public bool busy;
		public int next;
		public int[] reserved = new int[9];

		public override int @sizeof()
		{
			return 64;
		}

		protected internal override void read()
		{
			size = read32();
			nameAddr = read32();
			typeMask = read32();
			handlerAddr = read32();
			gp = read32();
			busy = readBoolean();
			next = read32();
			for (int i = 0; i < reserved.Length; i++)
			{
				reserved[i] = read32();
			}

			if (nameAddr != 0)
			{
				name = readStringZ(nameAddr);
			}
		}

		protected internal override void write()
		{
			write32(size);
			write32(nameAddr);
			write32(typeMask);
			write32(handlerAddr);
			write32(gp);
			writeBoolean(busy);
			write32(next);
			for (int i = 0; i < reserved.Length; i++)
			{
				write32(reserved[i]);
			}
		}

		public override string ToString()
		{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("size=0x%X, name=0x%08X('%s'), typeMask=0x%X, handler=0x%08X, gp=0x%08X, busy=%b, next=0x%08X", size, nameAddr, name, typeMask, handlerAddr, gp, busy, next);
			return string.Format("size=0x%X, name=0x%08X('%s'), typeMask=0x%X, handler=0x%08X, gp=0x%08X, busy=%b, next=0x%08X", size, nameAddr, name, typeMask, handlerAddr, gp, busy, next);
		}
	}

}