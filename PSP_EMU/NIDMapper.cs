using System;
using System.Collections.Generic;
using System.Text;

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
namespace pspsharp
{

	using Logger = org.apache.log4j.Logger;

	using Modules = pspsharp.HLE.Modules;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;

	public class NIDMapper
	{
		private static Logger log = Modules.log;
		private static NIDMapper instance;
		private readonly IDictionary<int, NIDInfo> syscallMap;
		private readonly IDictionary<string, IDictionary<int, NIDInfo>> moduleNidMap;
		private readonly IDictionary<int, NIDInfo> nidMap;
		private readonly IDictionary<int, NIDInfo> addressMap;
		private readonly IDictionary<string, NIDInfo> nameMap;
		private int freeSyscallNumber;

		protected internal class NIDInfo
		{
			internal readonly int nid;
			internal readonly int syscall;
			internal int address;
			internal readonly string name;
			internal readonly string moduleName;
			internal readonly bool variableExport; // Is coming from a function or variable export?
			internal int firmwareVersion;
			internal bool overwritten;
			internal bool loaded;
			internal bool validModuleName;

			/// <summary>
			/// New NIDInfo for a NID from a loaded module.
			/// </summary>
			/// <param name="nid"> </param>
			/// <param name="address"> </param>
			/// <param name="moduleName"> </param>
			public NIDInfo(int nid, int address, string moduleName, bool variableExport)
			{
				this.nid = nid;
				this.address = address;
				this.moduleName = moduleName;
				this.variableExport = variableExport;
				name = null;
				syscall = -1;
				firmwareVersion = 999;
				overwritten = false;
				loaded = true;
				validModuleName = true;
			}

			/// <summary>
			/// New NIDInfo for a NID from an HLE syscall.
			/// </summary>
			/// <param name="nid"> </param>
			/// <param name="syscall"> </param>
			/// <param name="name"> </param>
			/// <param name="moduleName"> </param>
			/// <param name="firmwareVersion"> </param>
			public NIDInfo(int nid, int syscall, string name, string moduleName, int firmwareVersion)
			{
				this.nid = nid;
				this.syscall = syscall;
				this.name = name;
				this.moduleName = moduleName;
				variableExport = false;
				this.firmwareVersion = firmwareVersion;
				address = 0;
				overwritten = false;
				loaded = true;
				validModuleName = false; // the given moduleName is probably not the correct one...
			}

			public virtual int Nid
			{
				get
				{
					return nid;
				}
			}

			public virtual int Syscall
			{
				get
				{
					return syscall;
				}
			}

			public virtual bool hasSyscall()
			{
				return syscall >= 0;
			}

			public virtual int Address
			{
				get
				{
					return address;
				}
				set
				{
					this.address = value;
				}
			}


			public virtual bool hasAddress()
			{
				return address != 0;
			}

			public virtual string Name
			{
				get
				{
					return name;
				}
			}

			public virtual bool hasName()
			{
				return !string.ReferenceEquals(name, null) && name.Length > 0;
			}

			public virtual string ModuleName
			{
				get
				{
					return moduleName;
				}
			}

			public virtual bool Overwritten
			{
				get
				{
					return overwritten;
				}
				set
				{
					this.overwritten = value;
				}
			}


			public virtual void overwrite(int address)
			{
				Overwritten = true;
				Address = address;
			}

			public virtual void undoOverwrite()
			{
				Overwritten = false;
				Address = 0;
			}

			public virtual int FirmwareVersion
			{
				get
				{
					return firmwareVersion;
				}
				set
				{
					this.firmwareVersion = value;
				}
			}


			public virtual bool isFromModule(string moduleName)
			{
				return moduleName.Equals(this.moduleName);
			}

			public virtual bool Loaded
			{
				get
				{
					return loaded;
				}
				set
				{
					this.loaded = value;
				}
			}


			public virtual bool ValidModuleName
			{
				get
				{
					return validModuleName;
				}
			}

			public virtual bool VariableExport
			{
				get
				{
					return variableExport;
				}
			}

			public override string ToString()
			{
				StringBuilder s = new StringBuilder();

				if (!string.ReferenceEquals(name, null))
				{
					s.Append(string.Format("{0}(nid=0x{1:X8})", name, nid));
				}
				else
				{
					s.Append(string.Format("nid=0x{0:X8}", nid));
				}
				s.Append(string.Format(", moduleName='{0}'", moduleName));
				if (!ValidModuleName)
				{
					s.Append("(probably invalid)");
				}
				if (VariableExport)
				{
					s.Append(", variable export");
				}
				s.Append(string.Format(", firmwareVersion={0:D}", firmwareVersion));
				if (Overwritten)
				{
					s.Append(", overwritten");
				}
				if (hasAddress())
				{
					s.Append(string.Format(", address=0x{0:X8}", address));
				}
				if (hasSyscall())
				{
					s.Append(string.Format(", syscall=0x{0:X}", syscall));
				}

				return s.ToString();
			}
		}

		public static NIDMapper Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new NIDMapper();
				}
				return instance;
			}
		}

		private NIDMapper()
		{
			moduleNidMap = new Dictionary<>();
			nidMap = new Dictionary<>();
			syscallMap = new Dictionary<>();
			addressMap = new Dictionary<>();
			nameMap = new Dictionary<>();
			// Official syscalls start at 0x2000,
			// so we'll put the HLE syscalls far away at 0x4000.
			freeSyscallNumber = 0x4000;
		}

		private void addNIDInfo(NIDInfo info)
		{
			IDictionary<int, NIDInfo> moduleMap = moduleNidMap[info.ModuleName];
			if (moduleMap == null)
			{
				moduleMap = new Dictionary<int, NIDInfo>();
				moduleNidMap[info.ModuleName] = moduleMap;
			}
			moduleMap[info.Nid] = info;

			// For HLE NID's, do not trust the module names defined in pspsharp, use only the NID.
			if (!info.ValidModuleName)
			{
				nidMap[info.Nid] = info;
			}

			if (info.hasAddress())
			{
				addressMap[info.Address] = info;
			}

			if (info.hasSyscall())
			{
				syscallMap[info.Syscall] = info;
			}

			if (info.hasName())
			{
				nameMap[info.Name] = info;
			}
		}

		private void removeNIDInfo(NIDInfo info)
		{
			IDictionary<int, NIDInfo> moduleMap = moduleNidMap[info.ModuleName];
			if (moduleMap != null)
			{
				moduleMap.Remove(info.Nid);
				if (moduleMap.Count == 0)
				{
					moduleNidMap.Remove(info.ModuleName);
				}
			}

			// For HLE NID's, do not trust the module names defined in pspsharp, use only the NID.
			if (!info.ValidModuleName)
			{
				nidMap.Remove(info.Nid);
			}

			if (info.hasAddress())
			{
				addressMap.Remove(info.Address);
			}

			if (info.hasSyscall())
			{
				syscallMap.Remove(info.Syscall);
			}

			if (info.hasName())
			{
				nameMap.Remove(info.Name);
			}
		}

		private NIDInfo getNIDInfoByNid(string moduleName, int nid)
		{
			NIDInfo info = null;

			IDictionary<int, NIDInfo> moduleMap = moduleNidMap[moduleName];
			if (moduleMap != null)
			{
				info = moduleMap[nid];
			}

			// For HLE NID's, do not trust the module names defined in pspsharp, use only the NID.
			if (info == null)
			{
				info = nidMap[nid];
			}

			return info;
		}

		private NIDInfo getNIDInfoBySyscall(int syscall)
		{
			return syscallMap[syscall];
		}

		private NIDInfo getNIDInfoByAddress(int address)
		{
			return addressMap[address];
		}

		private NIDInfo getNIDInfoByName(string name)
		{
			return nameMap[name];
		}

		public virtual int NewSyscallNumber
		{
			get
			{
				return freeSyscallNumber++;
			}
		}

		/// <summary>
		/// Add a NID from an HLE syscall.
		/// </summary>
		/// <param name="nid">             the nid </param>
		/// <param name="name">            the function name </param>
		/// <param name="moduleName">      the module name </param>
		/// <param name="firmwareVersion"> the firmware version defining this nid </param>
		/// <returns>                true if the NID has been added
		///                        false if the NID was already added </returns>
		public virtual bool addHLENid(int nid, string name, string moduleName, int firmwareVersion)
		{
			NIDInfo info = getNIDInfoByNid(moduleName, nid);
			if (info != null)
			{
				// This NID is already added, verify that we are trying to use the same data
				if (!name.Equals(info.Name) || !moduleName.Equals(info.ModuleName) || firmwareVersion != info.FirmwareVersion)
				{
					return false;
				}
				return true;
			}

			int syscall = NewSyscallNumber;
			info = new NIDInfo(nid, syscall, name, moduleName, firmwareVersion);

			addNIDInfo(info);

			return true;
		}

		/// <summary>
		/// Add a NID loaded from a module.
		/// </summary>
		/// <param name="module">     the loaded module </param>
		/// <param name="moduleName"> the module name </param>
		/// <param name="nid">        the nid </param>
		/// <param name="address">    the address of the nid </param>
		/// <param name="variableExport"> coming from a function or variable export </param>
		public virtual void addModuleNid(SceModule module, string moduleName, int nid, int address, bool variableExport)
		{
			address &= Memory.addressMask;

			NIDInfo info = getNIDInfoByNid(moduleName, nid);
			if (info != null)
			{
				// Only modules from flash0 are allowed to overwrite NIDs from syscalls
				if (string.ReferenceEquals(module.pspfilename, null) || !module.pspfilename.StartsWith("flash0:", StringComparison.Ordinal))
				{
					return;
				}
				if (log.InfoEnabled)
				{
					log.info(string.Format("NID {0}[0x{1:X8}] at address 0x{2:X8} from module '{3}' overwriting an HLE syscall", info.Name, nid, address, moduleName));
				}
				info.overwrite(address);
				addressMap[address] = info;
			}
			else
			{
				info = new NIDInfo(nid, address, moduleName, variableExport);

				addNIDInfo(info);
			}
		}

		/// <summary>
		/// Remove all the NIDs that have been loaded from a module.
		/// </summary>
		/// <param name="moduleName"> the module name </param>
		public virtual void removeModuleNids(string moduleName)
		{
			IList<NIDInfo> nidsToBeRemoved = new LinkedList<NIDInfo>();
			IList<int> addressesToBeRemoved = new LinkedList<int>();
			foreach (NIDInfo info in addressMap.Values)
			{
				if (info.isFromModule(moduleName))
				{
					if (info.Overwritten)
					{
						addressesToBeRemoved.Add(info.Address);
						info.undoOverwrite();
					}
					else
					{
						nidsToBeRemoved.Add(info);
					}
				}
			}

			foreach (NIDInfo info in nidsToBeRemoved)
			{
				removeNIDInfo(info);
			}

			foreach (int? address in addressesToBeRemoved)
			{
				addressMap.Remove(address);
			}
		}

		public virtual int getAddressByNid(int nid, string moduleName)
		{
			NIDInfo info = getNIDInfoByNid(moduleName, nid);
			if (info == null || !info.hasAddress())
			{
				return 0;
			}

			if (!string.ReferenceEquals(moduleName, null) && !info.isFromModule(moduleName))
			{
				log.debug(string.Format("Trying to resolve {0} from module '{1}'", info, moduleName));
			}

			return info.Address;
		}

		public virtual int getAddressByNid(int nid)
		{
			return getAddressByNid(nid, null);
		}

		public virtual int getAddressBySyscall(int syscall)
		{
			NIDInfo info = getNIDInfoBySyscall(syscall);
			if (info == null || !info.hasAddress())
			{
				return 0;
			}

			return info.Address;
		}

		public virtual int getAddressByName(string name)
		{
			NIDInfo info = getNIDInfoByName(name);
			if (info == null || !info.hasAddress())
			{
				return 0;
			}

			return info.Address;
		}

		public virtual int getSyscallByNid(int nid, string moduleName)
		{
			NIDInfo info = getNIDInfoByNid(moduleName, nid);
			if (info == null || !info.hasSyscall())
			{
				return -1;
			}

			if (!string.ReferenceEquals(moduleName, null) && !info.isFromModule(moduleName))
			{
				log.debug(string.Format("Trying to resolve {0} from module '{1}'", info, moduleName));
			}

			return info.Syscall;
		}

		public virtual int getSyscallByNid(int nid)
		{
			return getSyscallByNid(nid, null);
		}

		public virtual string getNameBySyscall(int syscall)
		{
			NIDInfo info = getNIDInfoBySyscall(syscall);
			if (info == null)
			{
				return null;
			}

			return info.Name;
		}

		public virtual int getNidBySyscall(int syscall)
		{
			NIDInfo info = getNIDInfoBySyscall(syscall);
			if (info == null)
			{
				return 0;
			}

			return info.Nid;
		}

		public virtual int getNidByAddress(int address)
		{
			NIDInfo info = getNIDInfoByAddress(address);
			if (info == null)
			{
				return 0;
			}

			return info.Nid;
		}

		public virtual void unloadNid(int nid)
		{
			// Search for the NID in all the modules
			foreach (string moduleName in moduleNidMap.Keys)
			{
				NIDInfo info = getNIDInfoByNid(moduleName, nid);
				if (info != null)
				{
					info.Loaded = false;
				}
			}
		}

		public virtual void unloadAll()
		{
			foreach (IDictionary<int, NIDInfo> moduleMap in moduleNidMap.Values)
			{
				foreach (NIDInfo info in moduleMap.Values)
				{
					if (info.Overwritten)
					{
						info.undoOverwrite();
					}
					info.Loaded = false;
				}
			}
		}

		public virtual int[] getModuleNids(string moduleName)
		{
			IDictionary<int, NIDInfo> moduleMap = moduleNidMap[moduleName];
			if (moduleMap == null)
			{
				return null;
			}

			int?[] nids = moduleMap.Keys.toArray(new int?[moduleMap.Count]);
			if (nids == null)
			{
				return null;
			}

			int[] result = new int[nids.Length];
			for (int i = 0; i < nids.Length; i++)
			{
				result[i] = nids[i].Value;
			}

			return result;
		}

		public virtual string[] ModuleNames
		{
			get
			{
				string[] moduleNames = moduleNidMap.Keys.toArray(new string[moduleNidMap.Count]);
				return moduleNames;
			}
		}

		public virtual bool isVariableExportByAddress(int address)
		{
			NIDInfo info = getNIDInfoByAddress(address);
			if (info == null)
			{
				return false;
			}

			return info.VariableExport;
		}
	}
}