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
	public class pspIoDrvArg : pspAbstractMemoryMappedStructure
	{
		public int drvAddr;
		public pspIoDrv drv;
		public int arg;

		protected internal override void read()
		{
			drvAddr = read32();
			arg = read32();

			if (drvAddr == 0)
			{
				drv = null;
			}
			else
			{
				drv = new pspIoDrv();
				drv.read(mem, drvAddr);
			}
		}

		protected internal override void write()
		{
			write32(drvAddr);
			write32(arg);
		}

		public override int @sizeof()
		{
			return 8;
		}

		public override string ToString()
		{
			return string.Format("drv=0x{0:X8}({1}), arg=0x{2:X8}", drvAddr, drv, arg);
		}
	}

}