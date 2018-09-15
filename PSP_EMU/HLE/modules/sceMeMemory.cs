using System.Collections.Generic;

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
namespace pspsharp.HLE.modules
{

	//using Logger = org.apache.log4j.Logger;

	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;

	public class sceMeMemory : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceMeMemory");
		private IDictionary<int, SysMemInfo> allocated;

		public override void start()
		{
			allocated = new Dictionary<int, SysMemUserForUser.SysMemInfo>();

			base.start();
		}

		[HLEFunction(nid : 0xC4EDA9F4, version : 150)]
		public virtual int sceMeCalloc(int num, int size)
		{
			SysMemInfo info = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "sceMeCalloc", SysMemUserForUser.PSP_SMEM_Low, num * size, 0);
			if (info.addr == 0)
			{
				return 0;
			}
			Memory mem = Memory.Instance;
			mem.memset(info.addr, (sbyte) 0, info.size);

			allocated[info.addr] = info;

			return info.addr;
		}

		[HLEFunction(nid : 0x92D3BAA1, version : 150)]
		public virtual int sceMeMalloc(int size)
		{
			SysMemInfo info = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "sceMeCalloc", SysMemUserForUser.PSP_SMEM_Low, size, 0);
			if (info.addr == 0)
			{
				return 0;
			}

			allocated[info.addr] = info;

			return info.addr;
		}

		[HLEFunction(nid : 0x6ED69327, version : 150)]
		public virtual void sceMeFree(int addr)
		{
			SysMemInfo info = allocated.Remove(addr);
			if (info != null)
			{
				Modules.SysMemUserForUserModule.free(info);
			}
		}
	}

}