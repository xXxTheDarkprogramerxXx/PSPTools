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
	public class pspIoDrvFileArg : pspAbstractMemoryMappedStructure
	{
		public int unknown1;
		public int fsNum;
		public int drvArgAddr;
		public pspIoDrvArg drvArg;
		public int unknown2;
		public int arg;

		protected internal override void read()
		{
			unknown1 = read32();
			fsNum = read32();
			drvArgAddr = read32();
			unknown2 = read32();
			arg = read32();

			if (drvArgAddr == 0)
			{
				drvArg = null;
			}
			else
			{
				drvArg = new pspIoDrvArg();
				drvArg.read(mem, drvArgAddr);
			}
		}

		protected internal override void write()
		{
			write32(unknown1);
			write32(fsNum);
			write32(drvArgAddr);
			write32(unknown2);
			write32(arg);
		}

		public override int @sizeof()
		{
			return 20;
		}

		public override string ToString()
		{
			return string.Format("unknown1=0x{0:X}, fsNum=0x{1:X}, drvArg=0x{2:X8}({3}), unknown2=0x{4:X}, arg=0x{5:X8}", unknown1, fsNum, drvArgAddr, drvArg, unknown2, arg);
		}
	}

}