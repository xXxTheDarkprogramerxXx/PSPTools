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
	public class pspIoDrv : pspAbstractMemoryMappedStructure
	{
		public int nameAddr;
		public string name;
		public int devType;
		public int unknown;
		public int descriptionAddr;
		public string description;
		public int funcsAddr;
		public pspIoDrvFuncs ioDrvFuncs;

		protected internal override void read()
		{
			nameAddr = read32();
			devType = read32();
			unknown = read32();
			descriptionAddr = read32();
			funcsAddr = read32();

			name = readStringZ(nameAddr);
			description = readStringZ(descriptionAddr);
			if (funcsAddr == 0)
			{
				ioDrvFuncs = null;
			}
			else
			{
				ioDrvFuncs = new pspIoDrvFuncs();
				ioDrvFuncs.read(mem, funcsAddr);
			}
		}

		protected internal override void write()
		{
			write32(nameAddr);
			write32(devType);
			write32(unknown);
			write32(descriptionAddr);
			write32(funcsAddr);

			if (ioDrvFuncs != null && funcsAddr != 0)
			{
				ioDrvFuncs.write(mem, funcsAddr);
			}
		}

		public override int @sizeof()
		{
			return 20;
		}

		public override string ToString()
		{
			return string.Format("name=0x{0:X8}('{1}'), devType=0x{2:X}, unknown=0x{3:X}, description=0x{4:X8}('{5}'), funcsAddr=0x{6:X8}({7})", nameAddr, name, devType, unknown, descriptionAddr, description, funcsAddr, ioDrvFuncs);
		}
	}

}