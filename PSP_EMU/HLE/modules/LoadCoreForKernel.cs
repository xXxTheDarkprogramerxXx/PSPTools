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
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using Managers = pspsharp.HLE.kernel.Managers;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceLoadCoreBootInfo = pspsharp.HLE.kernel.types.SceLoadCoreBootInfo;
	using SceLoadCoreBootModuleInfo = pspsharp.HLE.kernel.types.SceLoadCoreBootModuleInfo;
	using SceLoadCoreExecFileInfo = pspsharp.HLE.kernel.types.SceLoadCoreExecFileInfo;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using SceResidentLibraryEntryTable = pspsharp.HLE.kernel.types.SceResidentLibraryEntryTable;
	using SysMemThreadConfig = pspsharp.HLE.kernel.types.SysMemThreadConfig;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using Elf32Header = pspsharp.format.Elf32Header;
	using Elf32ProgramHeader = pspsharp.format.Elf32ProgramHeader;
	using Elf32SectionHeader = pspsharp.format.Elf32SectionHeader;
	using PSPModuleInfo = pspsharp.format.PSPModuleInfo;
	using Utilities = pspsharp.util.Utilities;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._v0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._zr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.HLEModuleManager.HLESyscallNid;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ModuleMgrForUser.SCE_HEADER_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.KERNEL_PARTITION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_Low;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.ADDIU;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.JR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.SYSCALL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Loader.SCE_MAGIC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.Elf32SectionHeader.SHF_ALLOCATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.Elf32SectionHeader.SHF_EXECUTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.Elf32SectionHeader.SHT_PROGBITS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.PSP.PSP_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.PSP.PSP_MAGIC;


	using Logger = org.apache.log4j.Logger;

	public class LoadCoreForKernel : HLEModule
	{
		public static Logger log = Modules.getLogger("LoadCoreForKernel");
		private ISet<int> dummyModuleData;
		private TPointer syscallStubAddr;
		private int availableSyscallStubs;
		private readonly IDictionary<string, string> functionNames = new Dictionary<string, string>();
		private readonly IDictionary<int, int> functionNids = new Dictionary<int, int>();

		private class OnModuleStartAction : IAction
		{
			private readonly LoadCoreForKernel outerInstance;

			public OnModuleStartAction(LoadCoreForKernel outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.onModuleStart();
			}
		}

		private class AfterSceKernelFindModuleByName : IAction
		{
			private readonly LoadCoreForKernel outerInstance;

			internal SceKernelThreadInfo thread;
			internal TPointer moduleNameAddr;

			public AfterSceKernelFindModuleByName(LoadCoreForKernel outerInstance, SceKernelThreadInfo thread, TPointer moduleNameAddr)
			{
				this.outerInstance = outerInstance;
				this.thread = thread;
				this.moduleNameAddr = moduleNameAddr;
			}

			public virtual void execute()
			{
				outerInstance.fixExistingModule(moduleNameAddr, thread.cpuContext._v0);
			}
		}

		public virtual IAction ModuleStartAction
		{
			get
			{
				return new OnModuleStartAction(this);
			}
		}

		/// <summary>
		/// Hook to force the creation of a larger heap used by sceKernelRegisterLibrary_660().
		/// </summary>
		/// <param name="partitionId"> the partitionId of the heap </param>
		/// <param name="size">        the size of the heap </param>
		/// <param name="flags">       the flags for the heap creation </param>
		/// <param name="name">        the heap name </param>
		/// <returns>            the new size of the heap </returns>
		public virtual int hleKernelCreateHeapHook(int partitionId, int size, int flags, string name)
		{
			if ("SceKernelLoadCore".Equals(name) && partitionId == KERNEL_PARTITION_ID && size == 0x1000 && flags == 0x1)
			{
				size += 0x4000;
			}

			return size;
		}

		public virtual bool decodeInitModuleData(TPointer buffer, int size, TPointer32 resultSize)
		{
			if (dummyModuleData == null || !dummyModuleData.Contains(buffer.Address))
			{
				return false;
			}

			buffer.memmove(buffer.Address + PSP_HEADER_SIZE, size - PSP_HEADER_SIZE);
			resultSize.setValue(size - PSP_HEADER_SIZE);

			return true;
		}

		private TPointer allocMem(int size)
		{
			SysMemInfo memInfo = Modules.SysMemUserForUserModule.malloc(KERNEL_PARTITION_ID, "LoadCore-StartModuleParameters", PSP_SMEM_Low, size, 0);
			if (memInfo == null)
			{
				log.error(string.Format("Cannot allocate memory for loadcore.prx start parameters"));
				return TPointer.NULL;
			}

			TPointer pointer = new TPointer(Memory.Instance, memInfo.addr);
			pointer.clear(size);

			return pointer;
		}

		private void freeMem(TPointer pointer)
		{
			SysMemInfo memInfo = Modules.SysMemUserForUserModule.getSysMemInfoByAddress(pointer.Address);
			if (memInfo != null)
			{
				Modules.SysMemUserForUserModule.free(memInfo);
			}
		}

		private int addSyscallStub(int syscallCode)
		{
			// Each stub requires 8 bytes
			const int stubSize = 8;

			if (availableSyscallStubs <= 0)
			{
				availableSyscallStubs = 128;
				syscallStubAddr = allocMem(availableSyscallStubs * stubSize);
				if (syscallStubAddr.Null)
				{
					availableSyscallStubs = 0;
					log.error(string.Format("No more free memory to create a new Syscall stub!"));
					return 0;
				}
			}

			int stubAddr = syscallStubAddr.Address;
			syscallStubAddr.setValue32(0, JR());
			syscallStubAddr.setValue32(4, SYSCALL(syscallCode));

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Adding a syscall 0x{0:X} stub at 0x{1:X8}", syscallCode, stubAddr));
			}

			syscallStubAddr.add(stubSize);
			availableSyscallStubs--;

			return stubAddr;
		}

		private TPointer createDummyModule(SceLoadCoreBootModuleInfo sceLoadCoreBootModuleInfo, string moduleName, int initCodeOffset)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int moduleInfoSizeof = new pspsharp.format.PSPModuleInfo().sizeof();
			int moduleInfoSizeof = (new PSPModuleInfo()).@sizeof();
			// sceInit module code
			const int initCodeSize = 8;
			int totalSize = SCE_HEADER_LENGTH + PSP_HEADER_SIZE + Elf32Header.@sizeof() + Elf32ProgramHeader.@sizeof() + Elf32SectionHeader.@sizeof() + moduleInfoSizeof + initCodeOffset + initCodeSize;
			TPointer modBuf = allocMem(totalSize);
			if (log.TraceEnabled)
			{
				log.trace(string.Format("Allocated dummy module buffer {0}", modBuf));
			}
			sceLoadCoreBootModuleInfo.modBuf = modBuf;

			int offset = 0;
			// SCE header
			modBuf.setValue32(offset + 0, SCE_MAGIC);
			modBuf.setValue32(offset + 4, SCE_HEADER_LENGTH); // SceHeader.size
			offset += SCE_HEADER_LENGTH;

			// PSP header
			dummyModuleData.Add(modBuf.Address + offset);
			modBuf.setValue32(offset + 0, PSP_MAGIC);
			modBuf.setValue16(offset + 4, (short) 0x1000); // PspHeader.mod_attr = SCE_MODULE_KERNEL
			modBuf.setValue32(offset + 44, totalSize - SCE_HEADER_LENGTH); // PspHeader.psp_size, must be > 0
			modBuf.setValue32(offset + 48, 0); // PspHeader.boot_entry = 0
			modBuf.setValue32(offset + 52, 0x80000000 + Elf32Header.@sizeof() + Elf32ProgramHeader.@sizeof() + Elf32SectionHeader.@sizeof()); // PspHeader.modinfo_offset = IS_KERNEL_ADDR
			modBuf.setValue8(offset + 124, (sbyte) 2); // PspHeader.dec_mode = DECRYPT_MODE_KERNEL_MODULE
			modBuf.setValue32(offset + 208, 0); // PspHeader.tag = 0
			offset += PSP_HEADER_SIZE;

			// ELF header
			modBuf.setValue32(offset + 0, Elf32Header.ELF_MAGIC);
			modBuf.setValue8(offset + 4, (sbyte) 1); // Elf32Header.e_class = ELFCLASS32
			modBuf.setValue8(offset + 5, (sbyte) 1); // Elf32Header.e_data = ELFDATA2LSB
			modBuf.setValue16(offset + 16, (short) Elf32Header.ET_SCE_PRX); // Elf32Header.e_type = ET_SCE_PRX
			modBuf.setValue16(offset + 18, (short) Elf32Header.E_MACHINE_MIPS); // Elf32Header.e_machine = EM_MIPS_ALLEGREX
			modBuf.setValue32(offset + 24, moduleInfoSizeof + initCodeOffset); // Elf32Header.e_entry = dummy entry point, must be != 0
			modBuf.setValue32(offset + 28, Elf32Header.@sizeof()); // Elf32Header.e_phoff = sizeof(Elf32Header)
			modBuf.setValue32(offset + 32, Elf32Header.@sizeof() + Elf32ProgramHeader.@sizeof()); // Elf32Header.e_shoff = sizeof(Elf32Header) + sizeof(Elf32ProgramHeader)
			modBuf.setValue16(offset + 44, (short) 1); // Elf32Header.e_phnum, must be > 0
			modBuf.setValue16(offset + 48, (short) 1); // Elf32Header.e_shnum, must be > 0
			modBuf.setValue16(offset + 50, (short) 0); // Elf32Header.e_shstrndx = 0
			offset += Elf32Header.@sizeof();

			// ELF Program header
			modBuf.setValue32(offset + 0, 1); // Elf32ProgramHeader.p_type = PT_LOAD
			modBuf.setValue32(offset + 4, Elf32Header.@sizeof() + Elf32ProgramHeader.@sizeof() + Elf32SectionHeader.@sizeof()); // Elf32ProgramHeader.p_offset
			modBuf.setValue32(offset + 8, 0); // Elf32ProgramHeader.p_vaddr = 0
			modBuf.setValue32(offset + 12, Elf32Header.@sizeof() + Elf32ProgramHeader.@sizeof() + Elf32SectionHeader.@sizeof()); // Elf32ProgramHeader.p_paddr = sizeof(Efl32Header) + sizeof(Elf32ProgramHeader)
			modBuf.setValue32(offset + 16, moduleInfoSizeof + initCodeOffset + initCodeSize); // Elf32ProgramHeader.p_filesz
			modBuf.setValue32(offset + 20, moduleInfoSizeof + initCodeOffset + initCodeSize); // Elf32ProgramHeader.p_memsz
			offset += Elf32ProgramHeader.@sizeof();

			// ELF Section header
			modBuf.setValue32(offset + 4, SHT_PROGBITS); // Elf32SectionHeader.sh_type = SHT_PROGBITS
			modBuf.setValue32(offset + 8, SHF_ALLOCATE | SHF_EXECUTE); // Elf32SectionHeader.sh_flags = SHF_ALLOCATE | SHF_EXECUTE
			modBuf.setValue32(offset + 16, 1); // Elf32SectionHeader.sh_offset, must be > 0
			modBuf.setValue32(offset + 20, 0); // Elf32SectionHeader.sh_size = 0
			offset += Elf32SectionHeader.@sizeof();

			// PSP Module Info
			modBuf.setStringNZ(offset + 4, 28, moduleName); // PSPModuleInfo.m_name = moduleName
			offset += (new PSPModuleInfo()).@sizeof();

			// Allow the entry point of the module to be at different addresses
			offset += initCodeOffset;

			// Module entry point:
			// Code for "return SCE_KERNEL_NO_RESIDENT"
			TPointer entryPointCode = new TPointer(modBuf, offset);
			modBuf.setValue32(offset + 0, JR());
			modBuf.setValue32(offset + 4, ADDIU(_v0, _zr, 0));
			offset += initCodeSize;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("createDummyModule moduleName='{0}', entryPointCode={1}", moduleName, entryPointCode));
			}
			return entryPointCode;
		}

		private void createDummyInitModule(SceLoadCoreBootModuleInfo sceLoadCoreBootModuleInfo)
		{
			TPointer entryPointCode = createDummyModule(sceLoadCoreBootModuleInfo, "sceInit", 4);

			// After loading the sceInit module, fix the previously loaded module(s)
			entryPointCode.setValue32(4, SYSCALL(this, "hleLoadCoreInitStart"));
		}

		private void addKernelUserLibs(SysMemThreadConfig sysMemThreadConfig, string libName, int[] nids)
		{
			SceResidentLibraryEntryTable sceResidentLibraryEntryTable = new SceResidentLibraryEntryTable();
			sceResidentLibraryEntryTable.libNameAddr = allocMem(libName.Length + 1);
			sceResidentLibraryEntryTable.libNameAddr.StringZ = libName;
			sceResidentLibraryEntryTable.version[0] = (sbyte) 0x11;
			// Do not specify the SCE_LIB_SYSCALL_EXPORT attribute as we will link only through jumps.
			sceResidentLibraryEntryTable.attribute = 0x0001;
			sceResidentLibraryEntryTable.len = 5;
			sceResidentLibraryEntryTable.vStubCount = 0;
			sceResidentLibraryEntryTable.stubCount = 0;
			sceResidentLibraryEntryTable.vStubCountNew = 0;

			int[] addresses = new int[nids.Length];
			bool[] areVariableExports = new bool[nids.Length];

			TPointer sceResidentLibraryEntryTableAddr = allocMem(sceResidentLibraryEntryTable.@sizeof() + nids.Length * 8);
			TPointer entryTable = new TPointer(sceResidentLibraryEntryTableAddr, sceResidentLibraryEntryTable.@sizeof());
			sceResidentLibraryEntryTable.entryTable = entryTable;

			NIDMapper nidMapper = NIDMapper.Instance;
			// Analyze all the NIDs and separate into function or variable stubs
			for (int i = 0; i < nids.Length; i++)
			{
				int nid = nids[i];
				int address = nidMapper.getAddressByNid(nid, libName);
				// No address for the NID?
				// I.e. is the NID only called through a syscall?
				if (address == 0)
				{
					int syscallCode = nidMapper.getSyscallByNid(nid);
					if (syscallCode >= 0)
					{
						// Add a syscall stub as we always link through a jump.
						address = addSyscallStub(syscallCode);
					}
				}
				bool isVariableExport = nidMapper.isVariableExportByAddress(address);

				if (log.DebugEnabled)
				{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("Registering library '%s' NID 0x%08X at address 0x%08X (variableExport=%b)", libName, nid, address, isVariableExport));
					log.debug(string.Format("Registering library '%s' NID 0x%08X at address 0x%08X (variableExport=%b)", libName, nid, address, isVariableExport));
				}

				addresses[i] = address;
				areVariableExports[i] = isVariableExport;

				if (isVariableExport)
				{
					sceResidentLibraryEntryTable.vStubCountNew++;
				}
				else
				{
					sceResidentLibraryEntryTable.stubCount++;
				}
			}
			sceResidentLibraryEntryTable.write(sceResidentLibraryEntryTableAddr);

			// Write the function and variable table entries in the following order:
			// - stubCount NID values
			// - vStubCountNew NID values
			// - stubCount addresses
			// - vStubCountNew addresses
			int functionIndex = 0;
			int variableIndex = sceResidentLibraryEntryTable.stubCount;
			for (int i = 0; i < nids.Length; i++)
			{
				int index = areVariableExports[i] ? variableIndex++: functionIndex++;
				entryTable.setValue32(index * 4, nids[i]);
				entryTable.setValue32((index + nids.Length) * 4, addresses[i]);
			}

			int userLibIndex = sysMemThreadConfig.numExportLibs - sysMemThreadConfig.numKernelLibs;
			if (userLibIndex == sysMemThreadConfig.userLibs.Length)
			{
				sysMemThreadConfig.userLibs = Utilities.extendArray(sysMemThreadConfig.userLibs, 1);
			}
			sysMemThreadConfig.userLibs[userLibIndex] = sceResidentLibraryEntryTableAddr;
			sysMemThreadConfig.numExportLibs++;
		}

		private void addKernelUserLibs(SysMemThreadConfig sysMemThreadConfig, string libName)
		{
			// LoadCoreForKernel is already registered by the initialization code itself
			if ("LoadCoreForKernel".Equals(libName))
			{
				return;
			}

			int[] nids = NIDMapper.Instance.getModuleNids(libName);
			if (nids == null)
			{
				log.warn(string.Format("Unknown library '{0}', no NIDs found", libName));
				return;
			}

			// Keep the list of NIDs sorted for easier debugging
			Arrays.sort(nids);

			addKernelUserLibs(sysMemThreadConfig, libName, nids);
		}

		private void prepareKernelLibs(SysMemThreadConfig sysMemThreadConfig)
		{
			// Pass all the available libraries/modules NIDs to the
			// initialization code of loadcore.prx so that they will be
			// registered by calling sceKernelRegisterLibrary().
			string[] moduleNames = NIDMapper.Instance.ModuleNames;
			foreach (string moduleName in moduleNames)
			{
				addKernelUserLibs(sysMemThreadConfig, moduleName);
			}
		}

		private void addExistingModule(SceLoadCoreBootInfo sceLoadCoreBootInfo, string moduleName)
		{
			SceModule module = Managers.modules.getModuleByName(moduleName);
			if (module == null)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("addExistingModule could not find module '{0}'", moduleName));
				}
				return;
			}

			SceLoadCoreBootModuleInfo sceLoadCoreBootModuleInfo = new SceLoadCoreBootModuleInfo();
			createDummyModule(sceLoadCoreBootModuleInfo, moduleName, 0);

			TPointer sceLoadCoreBootModuleInfoAddr = new TPointer(sceLoadCoreBootInfo.startAddr, sceLoadCoreBootInfo.numModules * sceLoadCoreBootModuleInfo.@sizeof());
			sceLoadCoreBootModuleInfo.write(sceLoadCoreBootModuleInfoAddr);
			sceLoadCoreBootInfo.numModules++;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = HLESyscallNid, version = 150) public int hleLoadCoreInitStart(int argc, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=128, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer sceLoadCoreBootInfoAddr)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleLoadCoreInitStart(int argc, TPointer sceLoadCoreBootInfoAddr)
		{
			SceLoadCoreBootInfo sceLoadCoreBootInfo = new SceLoadCoreBootInfo();
			sceLoadCoreBootInfo.read(sceLoadCoreBootInfoAddr);

			int sceKernelFindModuleByName = NIDMapper.Instance.getAddressByName("sceKernelFindModuleByName_660");
			if (sceKernelFindModuleByName != 0)
			{
				SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;

				// Fix the previously loaded scePaf_Module
				string moduleName = "scePaf_Module";
				TPointer moduleNameAddr = allocMem(moduleName.Length + 1);
				moduleNameAddr.StringZ = moduleName;
				Modules.ThreadManForUserModule.executeCallback(thread, sceKernelFindModuleByName, new AfterSceKernelFindModuleByName(this, thread, moduleNameAddr), false, moduleNameAddr.Address);
			}

			return 0;
		}

		private void fixExistingModule(TPointer moduleNameAddr, int sceModuleAddr)
		{
			string moduleName = moduleNameAddr.StringZ;
			Memory mem = moduleNameAddr.Memory;
			freeMem(moduleNameAddr);

			if (sceModuleAddr != 0)
			{
				SceModule sceModule = Managers.modules.getModuleByName(moduleName);
				if (sceModule != null)
				{
					// These values are required by the PSP implementation of
					// sceKernelFindModuleByAddress().
					mem.write32(sceModuleAddr + 108, sceModule.text_addr);
					mem.write32(sceModuleAddr + 112, sceModule.text_size);
					mem.write32(sceModuleAddr + 116, sceModule.data_size);
					mem.write32(sceModuleAddr + 120, sceModule.bss_size);
				}
			}
		}

		private void onModuleStart()
		{
			dummyModuleData = new HashSet<int>();
			SceLoadCoreBootInfo sceLoadCoreBootInfo = new SceLoadCoreBootInfo();
			SysMemThreadConfig sysMemThreadConfig = new SysMemThreadConfig();
			SceLoadCoreExecFileInfo loadCoreExecInfo = new SceLoadCoreExecFileInfo();
			SceLoadCoreExecFileInfo sysMemExecInfo = new SceLoadCoreExecFileInfo();
			PSPModuleInfo loadCoreModuleInfo = new PSPModuleInfo();
			PSPModuleInfo sysMemModuleInfo = new PSPModuleInfo();

			prepareKernelLibs(sysMemThreadConfig);

			// loadcore.prx is computing a checksum on the first 64 bytes of the first segment
			int dummySegmentSize = 64;
			TPointer dummySegmentAddr = allocMem(dummySegmentSize);
			loadCoreExecInfo.segmentAddr[0] = dummySegmentAddr;
			loadCoreExecInfo.segmentSize[0] = dummySegmentSize;
			loadCoreExecInfo.numSegments = 1;
			sysMemExecInfo.segmentAddr[0] = dummySegmentAddr;
			sysMemExecInfo.segmentSize[0] = dummySegmentSize;
			sysMemExecInfo.numSegments = 1;

			const int totalNumberOfModules = 2;
			SceLoadCoreBootModuleInfo sceLoadCoreBootModuleInfo = new SceLoadCoreBootModuleInfo();
			sceLoadCoreBootInfo.startAddr = allocMem(totalNumberOfModules * sceLoadCoreBootModuleInfo.@sizeof());
			sceLoadCoreBootInfo.numModules = 0;

			addExistingModule(sceLoadCoreBootInfo, "scePaf_Module");

			// Add the "sceInit" module as the last one
			createDummyInitModule(sceLoadCoreBootModuleInfo);
			TPointer sceLoadCoreBootModuleInfoAddr = new TPointer(sceLoadCoreBootInfo.startAddr, sceLoadCoreBootInfo.numModules * sceLoadCoreBootModuleInfo.@sizeof());
			sceLoadCoreBootModuleInfo.write(sceLoadCoreBootModuleInfoAddr);
			sceLoadCoreBootInfo.numModules++;

			TPointer loadCoreModuleInfoAddr = allocMem(loadCoreModuleInfo.@sizeof());
			loadCoreModuleInfo.write(loadCoreModuleInfoAddr);
			loadCoreExecInfo.moduleInfo = loadCoreModuleInfoAddr;

			TPointer sysMemModuleInfoAddr = allocMem(sysMemModuleInfo.@sizeof());
			sysMemModuleInfo.write(sysMemModuleInfoAddr);
			sysMemExecInfo.moduleInfo = sysMemModuleInfoAddr;

			TPointer sysMemExecInfoAddr = allocMem(sysMemExecInfo.@sizeof());
			sysMemExecInfo.write(sysMemExecInfoAddr);
			sysMemThreadConfig.sysMemExecInfo = sysMemExecInfoAddr;

			TPointer loadCoreExecInfoAddr = allocMem(loadCoreExecInfo.@sizeof());
			loadCoreExecInfo.write(loadCoreExecInfoAddr);
			sysMemThreadConfig.loadCoreExecInfo = loadCoreExecInfoAddr;

			TPointer sceLoadCoreBootInfoAddr = allocMem(sceLoadCoreBootInfo.@sizeof());
			sceLoadCoreBootInfo.write(sceLoadCoreBootInfoAddr);

			TPointer sysMemThreadConfigAddr = allocMem(sysMemThreadConfig.@sizeof());
			sysMemThreadConfig.write(sysMemThreadConfigAddr);

			TPointer argp = allocMem(8);
			argp.setPointer(0, sceLoadCoreBootInfoAddr);
			argp.setPointer(4, sysMemThreadConfigAddr);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("onModuleStart sceLoadCoreBootInfoAddr={0}, sysMemThreadConfigAddr={1}, loadCoreExecInfoAddr={2}, sysMemExecInfoAddr={3}", sceLoadCoreBootInfoAddr, sysMemThreadConfigAddr, loadCoreExecInfoAddr, sysMemExecInfoAddr));
			}

			// Set the thread start parameters
			SceKernelThreadInfo currentThread = Modules.ThreadManForUserModule.CurrentThread;
			currentThread.cpuContext._a0 = 8;
			currentThread.cpuContext._a1 = argp.Address;
		}

		private int LoadCoreBaseAddress
		{
			get
			{
				return unchecked((int)0x8802111C);
			}
		}

		public virtual HLEModuleFunction getHLEFunctionByAddress(int address)
		{
			if (!reboot.enableReboot)
			{
				return null;
			}

			address &= Memory.addressMask;

			Memory mem = Memory.Instance;
			int g_loadCore = LoadCoreBaseAddress;
			int registeredLibs = g_loadCore + 0;

			int[] nids = getFunctionNIDsByAddress(mem, registeredLibs, address);
			if (nids == null)
			{
				if (Memory.isAddressGood(address))
				{
					// Verify if this not the address of a stub call:
					//   J   realAddress
					//   NOP
					if (((int)((uint)mem.read32(address) >> 26)) == AllegrexOpcodes.J)
					{
						if (mem.read32(address + 4) == ThreadManForUser.NOP())
						{
							int jumpAddress = (mem.read32(address) & 0x03FFFFFF) << 2;

							nids = getFunctionNIDsByAddress(mem, registeredLibs, jumpAddress);
						}
					}
				}
			}

			if (nids != null)
			{
				foreach (int nid in nids)
				{
					HLEModuleFunction hleFunction = HLEModuleManager.Instance.getFunctionFromNID(nid);
					if (hleFunction != null)
					{
						return hleFunction;
					}
				}
			}

			return null;
		}

		public virtual string getFunctionNameByAddress(int address)
		{
			if (!reboot.enableReboot)
			{
				return null;
			}

			address &= Memory.addressMask;

			HLEModuleFunction hleModuleFunction = getHLEFunctionByAddress(address);
			if (hleModuleFunction != null)
			{
				return hleModuleFunction.FunctionName;
			}

			Memory mem = Memory.Instance;
			int g_loadCore = LoadCoreBaseAddress;
			int registeredMods = mem.read32(g_loadCore + 524);
			int module = getModuleByAddress(mem, registeredMods, address);
			string functionName = null;
			if (module != 0)
			{
				string moduleName = Utilities.readStringNZ(module + 8, 27);
				int moduleStart = mem.read32(module + 80) & Memory.addressMask;
				int moduleStop = mem.read32(module + 84) & Memory.addressMask;
				int moduleBootStart = mem.read32(module + 88) & Memory.addressMask;
				int moduleRebootBefore = mem.read32(module + 92) & Memory.addressMask;
				int moduleRebootPhase = mem.read32(module + 96) & Memory.addressMask;
				int entryAddr = mem.read32(module + 100) & Memory.addressMask;
				int textAddr = mem.read32(module + 108) & Memory.addressMask;

				if (address == moduleStart)
				{
					functionName = string.Format("{0}.module_start", moduleName);
				}
				else if (address == moduleStop)
				{
					functionName = string.Format("{0}.module_stop", moduleName);
				}
				else if (address == moduleBootStart)
				{
					functionName = string.Format("{0}.module_bootstart", moduleName);
				}
				else if (address == moduleRebootBefore)
				{
					functionName = string.Format("{0}.module_reboot_before", moduleName);
				}
				else if (address == moduleRebootPhase)
				{
					functionName = string.Format("{0}.module_reboot_phase", moduleName);
				}
				else if (address == entryAddr)
				{
					functionName = string.Format("{0}.module_start", moduleName);
				}
				else
				{
					functionName = string.Format("{0}.sub_{1:X8}", moduleName, address - textAddr);
				}
			}

			if (!string.ReferenceEquals(functionName, null) && functionNames.ContainsKey(functionName))
			{
				functionName = functionNames[functionName];
			}

			return functionName;
		}

		public virtual void addFunctionName(string moduleName, int address, string functionName)
		{
			functionNames[string.Format("{0}.sub_{1:X8}", moduleName, address)] = functionName;
		}

		public virtual void addFunctionNid(int address, int nid)
		{
			address &= Memory.addressMask;

			functionNids[address] = nid;
		}

		private int[] getFunctionNIDsByAddress(Memory mem, int registeredLibs, int address)
		{
			int[] nids = null;

			for (int i = 0; i < 512; i += 4)
			{
				int linkedLibraries = mem.read32(registeredLibs + i);
				while (linkedLibraries != 0)
				{
					int numExports = mem.read32(linkedLibraries + 16);
					int entryTable = mem.read32(linkedLibraries + 32);

					for (int j = 0; j < numExports; j++)
					{
						int nid = mem.read32(entryTable + j * 4);
						int entryAddress = mem.read32(entryTable + (j + numExports) * 4) & Memory.addressMask;

						if (address == entryAddress)
						{
							nids = Utilities.add(nids, nid);
						}
					}

					// Next
					linkedLibraries = mem.read32(linkedLibraries);
				}
			}

			if (nids == null && functionNids.ContainsKey(address))
			{
				nids = new int[] {functionNids[address]};
			}

			return nids;
		}

		private int getModuleByAddress(Memory mem, int linkedModules, int address)
		{
			while (linkedModules != 0 && Memory.isAddressGood(linkedModules))
			{
				int textAddr = mem.read32(linkedModules + 108) & Memory.addressMask;
				int textSize = mem.read32(linkedModules + 112);

				if (textAddr <= address && address < textAddr + textSize)
				{
					return linkedModules;
				}

				// Next
				linkedModules = mem.read32(linkedModules);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xACE23476, version = 150) public int sceKernelCheckPspConfig()
		[HLEFunction(nid : 0xACE23476, version : 150)]
		public virtual int sceKernelCheckPspConfig()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7BE1421C, version = 150) public int sceKernelCheckExecFile()
		[HLEFunction(nid : 0x7BE1421C, version : 150)]
		public virtual int sceKernelCheckExecFile()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBF983EF2, version = 150) public int sceKernelProbeExecutableObject()
		[HLEFunction(nid : 0xBF983EF2, version : 150)]
		public virtual int sceKernelProbeExecutableObject()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7068E6BA, version = 150) public int sceKernelLoadExecutableObject()
		[HLEFunction(nid : 0x7068E6BA, version : 150)]
		public virtual int sceKernelLoadExecutableObject()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB4D6FECC, version = 150) public int sceKernelApplyElfRelSection()
		[HLEFunction(nid : 0xB4D6FECC, version : 150)]
		public virtual int sceKernelApplyElfRelSection()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x54AB2675, version = 150) public int sceKernelApplyPspRelSection()
		[HLEFunction(nid : 0x54AB2675, version : 150)]
		public virtual int sceKernelApplyPspRelSection()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2952F5AC, version = 150) public int sceKernelDcacheWBinvAll()
		[HLEFunction(nid : 0x2952F5AC, version : 150)]
		public virtual int sceKernelDcacheWBinvAll()
		{
			return 0;
		}

		[HLELogging(level:"trace"), HLEFunction(nid : 0xD8779AC6, version : 150)]
		public virtual int sceKernelIcacheClearAll()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x99A695F0, version = 150) public int sceKernelRegisterLibrary()
		[HLEFunction(nid : 0x99A695F0, version : 150)]
		public virtual int sceKernelRegisterLibrary()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5873A31F, version = 150) public int sceKernelRegisterLibraryForUser()
		[HLEFunction(nid : 0x5873A31F, version : 150)]
		public virtual int sceKernelRegisterLibraryForUser()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0B464512, version = 150) public int sceKernelReleaseLibrary()
		[HLEFunction(nid : 0x0B464512, version : 150)]
		public virtual int sceKernelReleaseLibrary()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9BAF90F6, version = 150) public int sceKernelCanReleaseLibrary()
		[HLEFunction(nid : 0x9BAF90F6, version : 150)]
		public virtual int sceKernelCanReleaseLibrary()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0E760DBA, version = 150) public int sceKernelLinkLibraryEntries()
		[HLEFunction(nid : 0x0E760DBA, version : 150)]
		public virtual int sceKernelLinkLibraryEntries()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0DE1F600, version = 150) public int sceKernelLinkLibraryEntriesForUser()
		[HLEFunction(nid : 0x0DE1F600, version : 150)]
		public virtual int sceKernelLinkLibraryEntriesForUser()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDA1B09AA, version = 150) public int sceKernelUnLinkLibraryEntries()
		[HLEFunction(nid : 0xDA1B09AA, version : 150)]
		public virtual int sceKernelUnLinkLibraryEntries()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC99DD47A, version = 150) public int sceKernelQueryLoadCoreCB()
		[HLEFunction(nid : 0xC99DD47A, version : 150)]
		public virtual int sceKernelQueryLoadCoreCB()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x616FCCCD, version = 150) public int sceKernelSetBootCallbackLevel()
		[HLEFunction(nid : 0x616FCCCD, version : 150)]
		public virtual int sceKernelSetBootCallbackLevel()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x52A86C21, version = 150) public int sceKernelGetModuleFromUID()
		[HLEFunction(nid : 0x52A86C21, version : 150)]
		public virtual int sceKernelGetModuleFromUID()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCD0F3BAC, version = 150) public int sceKernelCreateModule()
		[HLEFunction(nid : 0xCD0F3BAC, version : 150)]
		public virtual int sceKernelCreateModule()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6B2371C2, version = 150) public int sceKernelDeleteModule()
		[HLEFunction(nid : 0x6B2371C2, version : 150)]
		public virtual int sceKernelDeleteModule()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8D8A8ACE, version = 150) public int sceKernelAssignModule()
		[HLEFunction(nid : 0x8D8A8ACE, version : 150)]
		public virtual int sceKernelAssignModule()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAFF947D4, version = 150) public int sceKernelCreateAssignModule()
		[HLEFunction(nid : 0xAFF947D4, version : 150)]
		public virtual int sceKernelCreateAssignModule()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAE7C6E76, version = 150) public int sceKernelRegisterModule(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=228, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer module)
		[HLEFunction(nid : 0xAE7C6E76, version : 150)]
		public virtual int sceKernelRegisterModule(TPointer module)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x74CF001A, version = 150) public int sceKernelReleaseModule()
		[HLEFunction(nid : 0x74CF001A, version : 150)]
		public virtual int sceKernelReleaseModule()
		{
			return 0;
		}

		[HLEFunction(nid : 0xCF8A41B1, version : 150)]
		public virtual int sceKernelFindModuleByName(PspString moduleName)
		{
			SceModule module = Managers.modules.getModuleByName(moduleName.String);
			if (module == null)
			{
				log.warn(string.Format("sceKernelFindModuleByName not found moduleName={0}", moduleName));
				return 0; // return NULL
			}

			if (!Modules.ThreadManForUserModule.KernelMode)
			{
				log.warn("kernel mode required (sceKernelFindModuleByName)");
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelFindModuleByName returning 0x{0:X8}", module.address));
			}

			return module.address;
		}

		[HLEFunction(nid : 0xFB8AE27D, version : 150)]
		public virtual int sceKernelFindModuleByAddress(TPointer address)
		{
			SceModule module = Managers.modules.getModuleByAddress(address.Address);
			if (module == null)
			{
				log.warn(string.Format("sceKernelFindModuleByAddress not found module address={0}", address));
				return 0; // return NULL
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelFindModuleByAddress found module '{0}'", module.modname));
			}

			if (!Modules.ThreadManForUserModule.KernelMode)
			{
				log.warn("kernel mode required (sceKernelFindModuleByAddress)");
			}

			return module.address;
		}

		[HLEFunction(nid : 0xCCE4A157, version : 150)]
		public virtual int sceKernelFindModuleByUID(int uid)
		{
			SceModule module = Managers.modules.getModuleByUID(uid);
			if (module == null)
			{
				log.warn(string.Format("sceKernelFindModuleByUID not found module uid=0x{0:X}", uid));
				return 0; // return NULL
			}

			// The pspsdk is not properly handling module exports with a size > 4.
			// See
			//    pspSdkFindExport()
			// in
			//    https://github.com/pspdev/pspsdk/blob/master/src/sdk/fixup.c
			// which is assuming that all module exports have a size==4 (i.e. 16 bytes).
			// This code is leading to an invalid memory access when processing the exports
			// from real PSP modules, which do have exports with a size==5.
			// Ban these modules in a case of the homebrew.
			if (RuntimeContext.Homebrew)
			{
				string[] bannedModules = new string[] {"sceNet_Library", "sceNetInet_Library", "sceNetApctl_Library", "sceNetResolver_Library"};
				foreach (string bannedModule in bannedModules)
				{
					if (bannedModule.Equals(module.modname))
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceKernelFindModuleByUID banning module '{0}' for a homebrew", module.modname));
						}
						return 0; // NULL
					}
				}
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelFindModuleByUID found module '{0}'", module.modname));
			}

			if (!Modules.ThreadManForUserModule.KernelMode)
			{
				log.warn("kernel mode required (sceKernelFindModuleByUID)");
			}

			return module.address;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x929B5C69, version = 150) public int sceKernelGetModuleListWithAlloc()
		[HLEFunction(nid : 0x929B5C69, version : 150)]
		public virtual int sceKernelGetModuleListWithAlloc()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x05D915DB, version = 150) public int sceKernelGetModuleIdListForKernel()
		[HLEFunction(nid : 0x05D915DB, version : 150)]
		public virtual int sceKernelGetModuleIdListForKernel()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB27CC244, version = 150) public int sceKernelLoadRebootBin(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer fileData, int fileSize)
		[HLEFunction(nid : 0xB27CC244, version : 150)]
		public virtual int sceKernelLoadRebootBin(TPointer fileData, int fileSize)
		{
			return 0;
		}

		/// <summary>
		/// Load a module. This function is used to boot modules during the start of Loadcore. In order for 
		/// a module to be loaded, it has to be a kernel module.
		/// </summary>
		/// <param name="bootModInfo"> Pointer to module information (including the file content of the module, 
		///                    its size,...) used to boot the module. </param>
		/// <param name="execInfo"> Pointer an allocated execInfo structure used to handle load-checks against the 
		///                 program module.
		///                 Furthermore, it collects various information about the module, such as its elfType, 
		///                 its segments (.text, .data, .bss), the locations of its exported functions. </param>
		/// <param name="modMemId"> The memory id of the allocated kernelPRX memory block used for the program module 
		///                 sections. The memory block specified by the ID holds the .text segment of the module. 
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x493EE781, version = 660) public int sceKernelLoadModuleBootLoadCore_660(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=32, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer bootModInfo, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=192, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer execInfo, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 modMemId)
		[HLEFunction(nid : 0x493EE781, version : 660)]
		public virtual int sceKernelLoadModuleBootLoadCore_660(TPointer bootModInfo, TPointer execInfo, TPointer32 modMemId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD3353EC4, version = 660) public int sceKernelCheckExecFile_660(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=256, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer buf, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=192, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer execInfo)
		[HLEFunction(nid : 0xD3353EC4, version : 660)]
		public virtual int sceKernelCheckExecFile_660(TPointer buf, TPointer execInfo)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x41D10899, version = 660) public int sceKernelProbeExecutableObject_660(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=256, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer buf, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=192, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer execInfo)
		[HLEFunction(nid : 0x41D10899, version : 660)]
		public virtual int sceKernelProbeExecutableObject_660(TPointer buf, TPointer execInfo)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1C394885, version = 660) public int sceKernelLoadExecutableObject_660(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=256, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer buf, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=192, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer execInfo)
		[HLEFunction(nid : 0x1C394885, version : 660)]
		public virtual int sceKernelLoadExecutableObject_660(TPointer buf, TPointer execInfo)
		{
			return 0;
		}

		/// <summary>
		/// Register a resident library's entry table in the system. A resident module can register any 
		/// number of resident libraries. Note that this function is only meant to register kernel mode 
		/// resident libraries. In order to register user mode libraries, use sceKernelRegisterLibraryForUser().
		/// </summary>
		/// <param name="libEntryTable"> Pointer to the resident library's entry table.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x48AF96A9, version = 660) public int sceKernelRegisterLibrary_660(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=20, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer libEntryTable)
		[HLEFunction(nid : 0x48AF96A9, version : 660)]
		public virtual int sceKernelRegisterLibrary_660(TPointer libEntryTable)
		{
			return 0;
		}

		/// <summary>
		/// Check if a resident library can be released. This check returns "true" when all corresponding stub
		/// libraries at the time of the check have one the following status:
		///      a) unlinked
		///      b) have the the attribute SCE_LIB_WEAK_IMPORT (they can exist without the resident library 
		///         being registered).
		/// </summary>
		/// <param name="libEntryTable"> Pointer to the resident library's entry table.
		/// </param>
		/// <returns> 0 indicates the library can be released. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x538129F8, version = 660) public int sceKernelCanReleaseLibrary_660(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=20, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer libEntryTable)
		[HLEFunction(nid : 0x538129F8, version : 660)]
		public virtual int sceKernelCanReleaseLibrary_660(TPointer libEntryTable)
		{
			return 0;
		}

		/// <summary>
		/// Link kernel mode stub libraries with the corresponding registered resident libraries. Note that 
		/// this function assumes that the resident libraries linked with reside in kernel memory. Linking 
		/// with user mode resident libraries will result in failure.
		/// </summary>
		/// <param name="libStubTable"> Pointer to a stub library's entry table. If you want to link an array of 
		///                     entry tables, make libStubTable a pointer to the first element of that array. </param>
		/// <param name="size">         The number of entry tables to link.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8EAE9534, version = 660) public int sceKernelLinkLibraryEntries_660(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=26, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer libEntryTable, int size)
		[HLEFunction(nid : 0x8EAE9534, version : 660)]
		public virtual int sceKernelLinkLibraryEntries_660(TPointer libEntryTable, int size)
		{
			return 0;
		}

		/// <summary>
		/// Unlink stub libraries from their corresponding registered resident libraries. 
		/// </summary>
		/// <param name="libStubTable"> Pointer to a stub library's entry table. If you want to unlink an array of 
		///                     entry tables, make libStubTable a pointer to the first element of that array. </param>
		/// <param name="size"> The number of entry tables to unlink. </param>
		/// <returns>  </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0295CFCE, version = 660) public int sceKernelUnLinkLibraryEntries_660(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=26, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer libEntryTable, int size)
		[HLEFunction(nid : 0x0295CFCE, version : 660)]
		public virtual int sceKernelUnLinkLibraryEntries_660(TPointer libEntryTable, int size)
		{
			return 0;
		}

		/// <summary>
		/// Save interrupts state and disable all interrupts.
		/// </summary>
		/// <returns> The current state of the interrupt controller. Use sceKernelLoadCoreUnlock() to return 
		///         to that state. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1999032F, version = 660) public int sceKernelLoadCoreLock_660()
		[HLEFunction(nid : 0x1999032F, version : 660)]
		public virtual int sceKernelLoadCoreLock_660()
		{
			// Has no parameters
			return 0;
		}

		/// <summary>
		/// Return interrupt state.
		/// </summary>
		/// <param name="intrState"> The state acquired by sceKernelLoadCoreLock(). </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB6C037EA, version = 660) public int sceKernelLoadCoreUnlock_660(int intrState)
		[HLEFunction(nid : 0xB6C037EA, version : 660)]
		public virtual int sceKernelLoadCoreUnlock_660(int intrState)
		{
			return 0;
		}

		/// <summary>
		/// Register a user mode resident library's entry table in the system. A resident module can register 
		/// any number of resident libraries. In order to register kernel mode libraries, use 
		/// sceKernelRegisterLibrary().
		/// 
		/// Restrictions on user mode resident libraries:
		///    1) The resident library has to live in user memory.
		///    2) Functions cannot be exported via the SYSCALL technique.
		///    3) The resident library cannot be linked with stub libraries living in kernel memory.
		/// </summary>
		/// <param name="libEntryTable"> Pointer to the resident library's entry table.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2C60CCB8, version = 660) public int sceKernelRegisterLibraryForUser_660(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=26, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer libEntryTable)
		[HLEFunction(nid : 0x2C60CCB8, version : 660)]
		public virtual int sceKernelRegisterLibraryForUser_660(TPointer libEntryTable)
		{
			return 0;
		}

		/// <summary>
		/// Delete a registered resident library from the system. Deletion cannot be performed if there are 
		/// loaded modules using the resident library. These modules must be deleted first.
		/// </summary>
		/// <param name="libEntryTable"> Pointer to the resident library's entry table.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCB636A90, version = 660) public int sceKernelReleaseLibrary_660(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=26, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer libEntryTable)
		[HLEFunction(nid : 0xCB636A90, version : 660)]
		public virtual int sceKernelReleaseLibrary_660(TPointer libEntryTable)
		{
			return 0;
		}

		/// 
		/// <param name="libStubTable"> Pointer to a stub library's entry table. If you want to link an array of entry 
		///                     tables, make libStubTable a pointer to the first element of that array. </param>
		/// <param name="size"> The number of entry tables to link.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6ECFFFBA, version = 660) public int sceKernelLinkLibraryEntriesForUser_660(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer libStubTable, int size)
		[HLEFunction(nid : 0x6ECFFFBA, version : 660)]
		public virtual int sceKernelLinkLibraryEntriesForUser_660(TPointer libStubTable, int size)
		{
			return 0;
		}

		/// 
		/// <param name="libStubTable"> Pointer to a stub library's entry table. If you want to link an array of entry 
		///                     tables, make libStubTable a pointer to the first element of that array. </param>
		/// <param name="size"> The number of entry tables to link.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA481E30E, version = 660) public int sceKernelLinkLibraryEntriesWithModule_660(pspsharp.HLE.TPointer mod, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer libStubTable, int size)
		[HLEFunction(nid : 0xA481E30E, version : 660)]
		public virtual int sceKernelLinkLibraryEntriesWithModule_660(TPointer mod, TPointer libStubTable, int size)
		{
			return 0;
		}

		/// <summary>
		/// Does nothing but a simple return.
		/// </summary>
		/// <returns> 0. </returns>
		[HLEFunction(nid : 0x1915737F, version : 660)]
		public virtual int sceKernelMaskLibraryEntries_660()
		{
			// Has no parameters
			return 0;
		}

		/// <summary>
		/// Delete a module from the system. The module has to be stopped and released before.
		/// </summary>
		/// <param name="mod"> The module to delete.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x001B57BB, version = 660) public int sceKernelDeleteModule_660(pspsharp.HLE.TPointer mod)
		[HLEFunction(nid : 0x001B57BB, version : 660)]
		public virtual int sceKernelDeleteModule_660(TPointer mod)
		{
			return 0;
		}

		/// <summary>
		/// Allocate memory for a new SceModule structure and fill it with default values. This function is 
		/// called during the loading process of a module.
		/// </summary>
		/// <returns> A pointer to the allocated SceModule structure on success, otherwise NULL. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2C44F793, version = 660) public int sceKernelCreateModule_660()
		[HLEFunction(nid : 0x2C44F793, version : 660)]
		public virtual int sceKernelCreateModule_660()
		{
			// Has no parameters
			return 0;
		}

		/// <summary>
		/// Receive a list of UIDs of loaded modules.
		/// </summary>
		/// <param name="modIdList"> Pointer to a SceUID array which will receive the UIDs of the loaded modules. </param>
		/// <param name="size"> Size of modIdList. Specifies the number of entries that can be stored into modIdList. </param>
		/// <param name="modCount"> A pointer which will receive the total number of loaded modules. </param>
		/// <param name="userModsOnly"> Set to 1 to only receive UIDs from user mode modules. Set to 0 to receive UIDs 
		///                     from all loaded modules.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x37E6F41B, version = 660) public int sceKernelGetModuleIdListForKernel_660(pspsharp.HLE.TPointer32 modIdList, int size, pspsharp.HLE.TPointer32 modCount, boolean userModsOnly)
		[HLEFunction(nid : 0x37E6F41B, version : 660)]
		public virtual int sceKernelGetModuleIdListForKernel_660(TPointer32 modIdList, int size, TPointer32 modCount, bool userModsOnly)
		{
			return 0;
		}

		/// <summary>
		/// Receive a list of UIDs of all loaded modules.
		/// </summary>
		/// <param name="modCount"> A pointer which will receive the total number of loaded modules.
		/// </param>
		/// <returns> The UID of the allocated array containing UIDs of the loaded modules on success. It should 
		///        be greater than 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3FE631F0, version = 660) public int sceKernelGetModuleListWithAlloc_660(pspsharp.HLE.TPointer32 modCount)
		[HLEFunction(nid : 0x3FE631F0, version : 660)]
		public virtual int sceKernelGetModuleListWithAlloc_660(TPointer32 modCount)
		{
			return 0;
		}

		/// <summary>
		/// Find a loaded module by its UID.
		/// </summary>
		/// <param name="uid"> The UID of the module to find.
		/// </param>
		/// <returns> Pointer to the found SceModule structure on success, otherwise NULL. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x40972E6E, version = 660) public int sceKernelFindModuleByUID_660(int uid)
		[HLEFunction(nid : 0x40972E6E, version : 660)]
		public virtual int sceKernelFindModuleByUID_660(int uid)
		{
			return 0;
		}

		/// <summary>
		/// Get the global pointer value of a module.
		/// </summary>
		/// <param name="addr"> Memory address belonging to the module, i.e. the address of a function/global variable 
		///             within the module.
		/// </param>
		/// <returns> The global pointer value (greater than 0) of the found module on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x410084F9, version = 660) public int sceKernelGetModuleGPByAddressForKernel_660(int addr)
		[HLEFunction(nid : 0x410084F9, version : 660)]
		public virtual int sceKernelGetModuleGPByAddressForKernel_660(int addr)
		{
			return 0;
		}

		/// <summary>
		/// Compute a checksum of every segment of a module.
		/// </summary>
		/// <param name="mod"> The module to create the checksum for.
		/// </param>
		/// <returns> The checksum. Shouldn't be 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5FDDB07A, version = 660) public int sceKernelSegmentChecksum_660(pspsharp.HLE.TPointer mod)
		[HLEFunction(nid : 0x5FDDB07A, version : 660)]
		public virtual int sceKernelSegmentChecksum_660(TPointer mod)
		{
			return 0;
		}

		/// <summary>
		/// Unlink a module from the internal loaded-modules-linked-list. The module has to be stopped before.
		/// </summary>
		/// <param name="mod"> The module to release.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB17F5075, version = 660) public int sceKernelReleaseModule_660(pspsharp.HLE.TPointer mod)
		[HLEFunction(nid : 0xB17F5075, version : 660)]
		public virtual int sceKernelReleaseModule_660(TPointer mod)
		{
			return 0;
		}

		/// <summary>
		/// Find a loaded module containing the specified address.
		/// </summary>
		/// <param name="addr"> Memory address belonging to the module, i.e. the address of a function/global variable 
		///             within the module.
		/// </param>
		/// <returns> Pointer to the found SceModule structure on success, otherwise NULL. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBC99C625, version = 660) public int sceKernelFindModuleByAddress_660(int addr)
		[HLEFunction(nid : 0xBC99C625, version : 660)]
		public virtual int sceKernelFindModuleByAddress_660(int addr)
		{
			return 0;
		}

		/// <summary>
		/// Find a loaded module by its name. If more than one module with the same name is loaded, return 
		/// the module which was loaded last.
		/// </summary>
		/// <param name="name"> The name of the module to find. 
		/// </param>
		/// <returns> Pointer to the found SceModule structure on success, otherwise NULL. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF6B1BF0F, version = 660) public int sceKernelFindModuleByName_660(String name)
		[HLEFunction(nid : 0xF6B1BF0F, version : 660)]
		public virtual int sceKernelFindModuleByName_660(string name)
		{
			return 0;
		}

		/// <summary>
		/// Register a module in the system and link it into the internal loaded-modules-linked-list.
		/// </summary>
		/// <param name="mod"> The module to register.
		/// </param>
		/// <returns> 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBF2E388C, version = 660) public int sceKernelRegisterModule_660(pspsharp.HLE.TPointer mod)
		[HLEFunction(nid : 0xBF2E388C, version : 660)]
		public virtual int sceKernelRegisterModule_660(TPointer mod)
		{
			return 0;
		}

		/// <summary>
		/// Get a loaded module from its UID.
		/// </summary>
		/// <param name="uid"> The UID (of a module) to check for.
		/// </param>
		/// <returns> Pointer to the found SceModule structure on success, otherwise NULL. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCD26E0CA, version = 660) public int sceKernelGetModuleFromUID_660(int uid)
		[HLEFunction(nid : 0xCD26E0CA, version : 660)]
		public virtual int sceKernelGetModuleFromUID_660(int uid)
		{
			return 0;
		}

		/// <summary>
		/// Assign a module and check if it can be loaded, is a valid module and copy the moduleInfo section 
		/// of the execution file over to the SceModule structure.
		/// </summary>
		/// <param name="mod"> The module to receive the moduleInfo section data based on the provided execution file 
		///            information. </param>
		/// <param name="execFileInfo"> The execution file information used to copy over the moduleInfo section for 
		///        the specified module.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF3DD4808, version = 660) public int sceKernelAssignModule_660(pspsharp.HLE.TPointer mod, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=192, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer execFileInfo)
		[HLEFunction(nid : 0xF3DD4808, version : 660)]
		public virtual int sceKernelAssignModule_660(TPointer mod, TPointer execFileInfo)
		{
			return 0;
		}
	}
}