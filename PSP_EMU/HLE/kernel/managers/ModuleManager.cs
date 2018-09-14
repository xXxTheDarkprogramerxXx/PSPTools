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
namespace pspsharp.HLE.kernel.managers
{

	using SceModule = pspsharp.HLE.kernel.types.SceModule;

	public class ModuleManager
	{
		private int moduleCount;
		private Dictionary<int, SceModule> moduleNumToModule;
		private Dictionary<int, SceModule> moduleUidToModule;
		private Dictionary<string, SceModule> moduleNameToModule;

		public virtual void reset()
		{
			moduleCount = 0;
			moduleNumToModule = new Dictionary<int, SceModule>();
			moduleUidToModule = new Dictionary<int, SceModule>();
			moduleNameToModule = new Dictionary<string, SceModule>();
		}

		public virtual void addModule(SceModule module)
		{
			moduleCount++;
			moduleNumToModule[moduleCount] = module;
			moduleUidToModule[module.modid] = module;
			moduleNameToModule[module.modname] = module;
		}

		public virtual void removeModule(int uid)
		{
			if (moduleCount > 0)
			{
				moduleCount--;
			}
			SceModule sceModule = moduleUidToModule.Remove(uid);
			if (sceModule != null)
			{
				moduleNameToModule.Remove(sceModule.modname);
			}
		}

		public virtual ICollection<SceModule> values()
		{
			return moduleUidToModule.Values;
		}

		public virtual SceModule getModuleByUID(int uid)
		{
			return moduleUidToModule[uid];
		}

		// Used by sceKernelFindModuleByName.
		public virtual SceModule getModuleByName(string name)
		{
			return moduleNameToModule[name];
		}

		public virtual SceModule getModuleByAddress(int address)
		{
			foreach (SceModule module in moduleUidToModule.Values)
			{
				if (address >= module.loadAddressLow && address < module.loadAddressHigh)
				{
					return module;
				}
			}
			return null;
		}

		public static readonly ModuleManager singleton;

		static ModuleManager()
		{
			singleton = new ModuleManager();
			singleton.reset();
		}

		private ModuleManager()
		{
		}
	}
}