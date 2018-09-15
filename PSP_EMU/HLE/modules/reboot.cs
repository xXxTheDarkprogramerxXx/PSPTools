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
namespace pspsharp.HLE.modules
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.Modules.LoadCoreForKernelModule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.InitForKernel.SCE_INIT_APITYPE_KERNEL_REBOOT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_Addr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.VSHELL_PARTITION_ID;

	//using Logger = org.apache.log4j.Logger;

	using Compiler = pspsharp.Allegrex.compiler.Compiler;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using IVirtualFileSystem = pspsharp.HLE.VFS.IVirtualFileSystem;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelLoadExecVSHParam = pspsharp.HLE.kernel.types.SceKernelLoadExecVSHParam;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceLoadCoreBootInfo = pspsharp.HLE.kernel.types.SceLoadCoreBootInfo;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using SceSysmemUidCB = pspsharp.HLE.kernel.types.SceSysmemUidCB;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using Model = pspsharp.hardware.Model;
	using Utilities = pspsharp.util.Utilities;

	public class reboot : HLEModule
	{
		//public static Logger log = Modules.getLogger("reboot");
		public static bool enableReboot = false;
		private const string rebootFileName = "flash0:/reboot.bin";
		private static readonly int rebootBaseAddress = MemoryMap.START_KERNEL + 0x600000;
		private static readonly int rebootParamAddress = MemoryMap.START_KERNEL + 0x400000;

		private class SetLog4jMDC : IAction
		{
			public virtual void execute()
			{
				setLog4jMDC();
			}
		}

		public virtual bool loadAndRun()
		{
			if (!enableReboot)
			{
				return false;
			}

			Memory mem = Memory.Instance;

			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = Modules.IoFileMgrForUserModule.getVirtualFileSystem(rebootFileName, localFileName);
			if (vfs == null)
			{
				return false;
			}

			IVirtualFile vFile = vfs.ioOpen(localFileName.ToString(), IoFileMgrForUser.PSP_O_RDONLY, 0);
			if (vFile == null)
			{
				return false;
			}

			int rebootFileLength = (int) vFile.Length();
			if (rebootFileLength <= 0)
			{
				return false;
			}

			SceModule rebootModule = new SceModule(true);
			rebootModule.modname = Name;
			rebootModule.pspfilename = rebootFileName;
			rebootModule.baseAddress = rebootBaseAddress;
			rebootModule.text_addr = rebootBaseAddress;
			rebootModule.text_size = rebootFileLength;
			rebootModule.data_size = 0;
			rebootModule.bss_size = 0x26B80;

			const bool fromSyscall = false;
			Emulator.Instance.initNewPsp(fromSyscall);
			Emulator.Instance.ModuleLoaded = true;
			HLEModuleManager.Instance.startModules(fromSyscall);
			Modules.ThreadManForUserModule.Initialise(rebootModule, rebootModule.baseAddress, 0, rebootModule.pspfilename, -1, 0, fromSyscall);

			int rebootMemSize = rebootModule.text_size + rebootModule.data_size + rebootModule.bss_size;
			SysMemInfo rebootMemInfo = Modules.SysMemUserForUserModule.malloc(VSHELL_PARTITION_ID, "reboot", PSP_SMEM_Addr, rebootMemSize, rebootModule.text_addr);
			if (rebootMemInfo == null)
			{
				return false;
			}

			TPointer rebootBinAddr = new TPointer(mem, rebootBaseAddress);
			int readLength = vFile.ioRead(rebootBinAddr, rebootFileLength);
			vFile.ioClose();
			if (readLength != rebootFileLength)
			{
				return false;
			}

			markMMIO();

			addFunctionNames(rebootModule);

			SysMemInfo rebootParamInfo = Modules.SysMemUserForUserModule.malloc(VSHELL_PARTITION_ID, "reboot-parameters", PSP_SMEM_Addr, 0x10000, rebootParamAddress);
			TPointer sceLoadCoreBootInfoAddr = new TPointer(mem, rebootParamInfo.addr);
			SceLoadCoreBootInfo sceLoadCoreBootInfo = new SceLoadCoreBootInfo();

			sceLoadCoreBootInfoAddr.clear(0x1000 + 0x1C000 + 0x380);

			TPointer startAddr = new TPointer(sceLoadCoreBootInfoAddr, 0x1000);

			TPointer sceKernelLoadExecVSHParamAddr = new TPointer(startAddr, 0x1C000);
			TPointer loadModeStringAddr = new TPointer(sceKernelLoadExecVSHParamAddr, 48);
			loadModeStringAddr.StringZ = "vsh";
			SceKernelLoadExecVSHParam sceKernelLoadExecVSHParam = new SceKernelLoadExecVSHParam();
			sceKernelLoadExecVSHParamAddr.setValue32(48);
			sceKernelLoadExecVSHParam.flags = 0x10000;
			sceKernelLoadExecVSHParam.keyAddr = loadModeStringAddr;
			sceKernelLoadExecVSHParam.write(sceKernelLoadExecVSHParamAddr);

			sceLoadCoreBootInfo.memBase = MemoryMap.START_KERNEL;
			sceLoadCoreBootInfo.memSize = MemoryMap.SIZE_RAM;
			sceLoadCoreBootInfo.startAddr = startAddr;
			sceLoadCoreBootInfo.endAddr = new TPointer(sceKernelLoadExecVSHParamAddr, 0x380);
			sceLoadCoreBootInfo.modProtId = -1;
			sceLoadCoreBootInfo.modArgProtId = -1;
			sceLoadCoreBootInfo.model = Model.Model;
			sceLoadCoreBootInfo.dipswLo = Modules.KDebugForKernelModule.sceKernelDipswLow32();
			sceLoadCoreBootInfo.dipswHi = Modules.KDebugForKernelModule.sceKernelDipswHigh32();
			sceLoadCoreBootInfo.unknown72 = MemoryMap.END_USERSPACE | unchecked((int)0x80000000); // Must be larger than 0x89000000 + size of pspbtcnf.bin file
			sceLoadCoreBootInfo.cpTime = Modules.KDebugForKernelModule.sceKernelDipswCpTime();
			sceLoadCoreBootInfo.write(sceLoadCoreBootInfoAddr);

			SceKernelThreadInfo rootThread = Modules.ThreadManForUserModule.getRootThread(null);
			if (rootThread != null)
			{
				rootThread.cpuContext._a0 = sceLoadCoreBootInfoAddr.Address;
				rootThread.cpuContext._a1 = sceKernelLoadExecVSHParamAddr.Address;
				rootThread.cpuContext._a2 = SCE_INIT_APITYPE_KERNEL_REBOOT;
				rootThread.cpuContext._a3 = Modules.SysMemForKernelModule.sceKernelGetInitialRandomValue();
			}

			// This will set the Log4j MDC values for the root thread
			Emulator.Scheduler.addAction(new SetLog4jMDC());

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceReboot arg0={0}, arg1={1}", sceLoadCoreBootInfoAddr, sceKernelLoadExecVSHParamAddr));
			}

			return true;
		}

		private void markMMIO()
		{
			Compiler compiler = Compiler.Instance;
			compiler.addMMIORange(MemoryMap.START_KERNEL, 0x800000);
			compiler.addMMIORange(unchecked((int)0xBFC00C00), 0x240);
		}

		private void addFunctionNid(int moduleAddress, SceModule module, string name)
		{
			int nid = HLEModuleManager.Instance.getNIDFromFunctionName(name);
			if (nid != 0)
			{
				int address = module.text_addr + moduleAddress;
				LoadCoreForKernelModule.addFunctionNid(address, nid);
			}
		}

		private void addFunctionNames(SceModule rebootModule)
		{
			// These function names are taken from uOFW (https://github.com/uofw/uofw)
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x0080, "sceInit.patchGames");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x0218, "sceInit.InitCBInit");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x02E0, "sceInit.ExitInit");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x03F4, "sceInit.ExitCheck");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x0438, "sceInit.PowerUnlock");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x048C, "sceInit.invoke_init_callback");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x05F0, "sceInit.sub_05F0");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x06A8, "sceInit.CleanupPhase1");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x0790, "sceInit.CleanupPhase2");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x08F8, "sceInit.ProtectHandling");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x0CFC, "sceInit.sub_0CFC_IsModuleInUserPartition");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x0D4C, "sceInit.ClearFreeBlock");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x0DD0, "sceInit.sub_0DD0_IsApplicationTypeGame");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x1038, "sceInit.LoadModuleBufferAnchorInBtcnf");
			LoadCoreForKernelModule.addFunctionName("sceInit", 0x1240, "sceInit.InitThreadEntry");
			LoadCoreForKernelModule.addFunctionName("sceLoaderCore", 0x56B8, "sceLoaderCore.PspUncompress");
			LoadCoreForKernelModule.addFunctionName("sceGE_Manager", 0x0258, "sceGE_Manager.sceGeInit");
			LoadCoreForKernelModule.addFunctionName("sceMeCodecWrapper", 0x1C04, "sceMeCodecWrapper.decrypt");
			LoadCoreForKernelModule.addFunctionName("sceAudio_Driver", 0x0000, "sceAudio_Driver.updateAudioBuf");
			LoadCoreForKernelModule.addFunctionName("sceAudio_Driver", 0x137C, "sceAudio_Driver.audioOutput");
			LoadCoreForKernelModule.addFunctionName("sceAudio_Driver", 0x0530, "sceAudio_Driver.audioOutputDmaCb");
			LoadCoreForKernelModule.addFunctionName("sceAudio_Driver", 0x01EC, "sceAudio_Driver.dmaUpdate");
			LoadCoreForKernelModule.addFunctionName("sceAudio_Driver", 0x1970, "sceAudio_Driver.audioIntrHandler");
			LoadCoreForKernelModule.addFunctionName("sceAudio_Driver", 0x02B8, "sceAudio_Driver.audioMixerThread");
			LoadCoreForKernelModule.addFunctionName("sceSYSCON_Driver", 0x0A10, "sceSYSCON_Driver._sceSysconGpioIntr");
			LoadCoreForKernelModule.addFunctionName("sceSYSCON_Driver", 0x2434, "sceSYSCON_Driver._sceSysconPacketEnd");
			LoadCoreForKernelModule.addFunctionName("sceDisplay_Service", 0x04EC, "sceDisplay_Service.sceDisplayInit");
			LoadCoreForKernelModule.addFunctionName("scePower_Service", 0x0000, "scePower_Service.scePowerInit");
			LoadCoreForKernelModule.addFunctionName("sceHP_Remote_Driver", 0x0704, "sceHP_Remote_Driver.sceHpRemoteThreadEntry");
			LoadCoreForKernelModule.addFunctionName("sceLowIO_Driver", 0x9C7C, "sceNandTransferDataToNandBuf");

			// Mapping of subroutines defined in
			//     https://github.com/uofw/uofw/blob/master/src/reboot/unk.c
			// and https://github.com/uofw/uofw/blob/master/src/reboot/nand.c
			addFunctionNid(0x0000EFCC, rebootModule, "sceNandInit2");
			addFunctionNid(0x0000F0C4, rebootModule, "sceNandIsReady");
			addFunctionNid(0x0000F0D4, rebootModule, "sceNandSetWriteProtect");
			addFunctionNid(0x0000F144, rebootModule, "sceNandLock");
			addFunctionNid(0x0000F198, rebootModule, "sceNandReset");
			addFunctionNid(0x0000F234, rebootModule, "sceNandReadId");
			addFunctionNid(0x0000F28C, rebootModule, "sceNandReadAccess");
			addFunctionNid(0x0000F458, rebootModule, "sceNandWriteAccess");
			addFunctionNid(0x0000F640, rebootModule, "sceNandEraseBlock");
			addFunctionNid(0x0000F72C, rebootModule, "sceNandReadExtraOnly");
			addFunctionNid(0x0000F8A8, rebootModule, "sceNandReadStatus");
			addFunctionNid(0x0000F8DC, rebootModule, "sceNandSetScramble");
			addFunctionNid(0x0000F8EC, rebootModule, "sceNandReadPages");
			addFunctionNid(0x0000F930, rebootModule, "sceNandWritePages");
			addFunctionNid(0x0000F958, rebootModule, "sceNandReadPagesRawExtra");
			addFunctionNid(0x0000F974, rebootModule, "sceNandWritePagesRawExtra");
			addFunctionNid(0x0000F998, rebootModule, "sceNandReadPagesRawAll");
			addFunctionNid(0x0000F9D0, rebootModule, "sceNandTransferDataToNandBuf");
			addFunctionNid(0x0000FC40, rebootModule, "sceNandIntrHandler");
			addFunctionNid(0x0000FF60, rebootModule, "sceNandTransferDataFromNandBuf");
			addFunctionNid(0x000103C8, rebootModule, "sceNandWriteBlockWithVerify");
			addFunctionNid(0x0001047C, rebootModule, "sceNandReadBlockWithRetry");
			addFunctionNid(0x00010500, rebootModule, "sceNandVerifyBlockWithRetry");
			addFunctionNid(0x00010650, rebootModule, "sceNandEraseBlockWithRetry");
			addFunctionNid(0x000106C4, rebootModule, "sceNandIsBadBlock");
			addFunctionNid(0x00010750, rebootModule, "sceNandDoMarkAsBadBlock");
			addFunctionNid(0x000109DC, rebootModule, "sceNandDetectChipMakersBBM");
			addFunctionNid(0x00010D1C, rebootModule, "sceNandGetPageSize");
			addFunctionNid(0x00010D28, rebootModule, "sceNandGetPagesPerBlock");
			addFunctionNid(0x00010D34, rebootModule, "sceNandGetTotalBlocks");
		}

		public static void dumpAllModulesAndLibraries()
		{
			if (!enableReboot)
			{
				return;
			}

			Memory mem = Memory.Instance;
			int g_loadCore = unchecked((int)0x8802111C);
			int registeredMods = mem.read32(g_loadCore + 524);
			int registeredLibs = g_loadCore + 0;
			dumpAllModules(mem, registeredMods);
			for (int i = 0; i < 512; i += 4)
			{
				dumpAllLibraries(mem, mem.read32(registeredLibs + i));
			}
		}

		private static void dumpAllModules(Memory mem, int address)
		{
			while (address != 0)
			{
				string moduleName = Utilities.readStringNZ(address + 8, 27);
				int textAddr = mem.read32(address + 108);
				int textSize = mem.read32(address + 112);

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Module '{0}': text 0x{1:X8}-0x{2:X8}", moduleName, textAddr, textAddr + textSize));
				}
				// Next
				address = mem.read32(address);
			}
		}

		private static void dumpAllLibraries(Memory mem, int address)
		{
			while (address != 0)
			{
				string libName = Utilities.readStringZ(mem.read32(address + 68));
				int numExports = mem.read32(address + 16);
				int entryTable = mem.read32(address + 32);

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Library '{0}':", libName));
				}
				for (int i = 0; i < numExports; i++)
				{
					int nid = mem.read32(entryTable + i * 4);
					int entryAddress = mem.read32(entryTable + (i + numExports) * 4);
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("   0x{0:X8}: 0x{1:X8}", nid, entryAddress));
					}
				}

				// Next
				address = mem.read32(address);
			}
		}

		/// <summary>
		/// Set information about the current thread that can be used for logging in log4j:
		/// - LLE-thread-name: the current thread name
		/// - LLE-thread-uid: the current thread UID in the format 0x%X
		/// - LLE-thread: the current thread name and uid
		/// 
		/// These values can be used in LogSettings.xml inside a PatternLayout:
		///   <layout class="org.apache.log4j.PatternLayout">
		///     <param name="ConversionPattern" value="%d{HH:mm:ss,SSS} %5p %8c - %X{LLE-thread} - %m%n" />
		///   </layout>
		/// </summary>
		public static void setLog4jMDC()
		{
			if (!enableReboot)
			{
				return;
			}

			Processor processor = Emulator.Processor;
			bool isInterruptContext = processor.cp0.getControlRegister(13) != 0;
			if (isInterruptContext)
			{
				RuntimeContext.Log4jMDC = "Interrupt";
			}
			else
			{
				Memory mem = Memory.Instance;
				int threadManInfo = unchecked((int)0x88048740);

				int currentThread = mem.read32(threadManInfo + 0);
				if (Memory.isAddressGood(currentThread))
				{
					int uid = mem.read32(currentThread + 8);
					int cb = SysMemForKernel.getCBFromUid(uid);
					int nameAddr = mem.read32(cb + 16);
					string name = Utilities.readStringZ(nameAddr);

					RuntimeContext.setLog4jMDC(name, uid);
				}
				else
				{
					RuntimeContext.Log4jMDC = "root";
				}
			}
		}

		public static void dumpAllThreads()
		{
			if (!enableReboot)
			{
				return;
			}

			Memory mem = Memory.Instance;
			int threadManInfo = unchecked((int)0x88048740);

			int currentThread = mem.read32(threadManInfo + 0);
			int nextThread = mem.read32(threadManInfo + 4);

			dumpThreadTypeList(mem, mem.read32(threadManInfo + 1228));
			dumpThread(mem, currentThread, "Current thread");
			if (nextThread != 0 && nextThread != currentThread)
			{
				dumpThread(mem, nextThread, "Next thread");
			}
			dumpThreadList(mem, threadManInfo + 1176, "Sleeping thread");
			dumpThreadList(mem, threadManInfo + 1184, "Delayed thread");
			dumpThreadList(mem, threadManInfo + 1192, "Stopped thread");
			dumpThreadList(mem, threadManInfo + 1200, "Suspended thread");
			dumpThreadList(mem, threadManInfo + 1208, "Dead thread");
			dumpThreadList(mem, threadManInfo + 1216, "??? thread");
			for (int priority = 0; priority < 128; priority++)
			{
				dumpThreadList(mem, threadManInfo + 152 + priority * 8, string.Format("Ready thread[prio=0x{0:X}]", priority));
			}
		}

		private static void dumpThreadTypeList(Memory mem, int list)
		{
			for (int cb = mem.read32(list); cb != list; cb = mem.read32(cb))
			{
				SceSysmemUidCB sceSysmemUidCB = new SceSysmemUidCB();
				sceSysmemUidCB.read(mem, cb);
				dumpThread(mem, cb + sceSysmemUidCB.size * 4, "Thread");
			}
		}

		private static void dumpThread(Memory mem, int address, string comment)
		{
			int uid = mem.read32(address + 8);
			int status = mem.read32(address + 12);
			int currentPriority = mem.read32(address + 16);

			StringBuilder waitInfo = new StringBuilder();
			if (SceKernelThreadInfo.isWaitingStatus(status))
			{
				int waitType = mem.read32(address + 88);
				if (waitType != 0)
				{
					waitInfo.Append(string.Format(", waitType=0x{0:X}({1})", waitType, SceKernelThreadInfo.getWaitName(waitType)));
				}

				int waitTypeCBaddr = mem.read32(address + 92);
				if (waitTypeCBaddr != 0)
				{
					SceSysmemUidCB waitTypeCB = new SceSysmemUidCB();
					waitTypeCB.read(mem, waitTypeCBaddr);
					waitInfo.Append(string.Format(", waitUid=0x{0:X}({1})", waitTypeCB.uid, waitTypeCB.name));
				}

				if (waitType == SceKernelThreadInfo.PSP_WAIT_DELAY)
				{
					int waitDelay = mem.read32(address + 96);
					waitInfo.Append(string.Format(", waitDelay=0x{0:X}", waitDelay));
				}
				else if (waitType == SceKernelThreadInfo.PSP_WAIT_EVENTFLAG)
				{
					int bits = mem.read32(address + 96);
					waitInfo.Append(string.Format(", waitEventFlagBits=0x{0:X}", bits));
				}
			}

			int cb = SysMemForKernel.getCBFromUid(uid);
			SceSysmemUidCB sceSysmemUidCB = new SceSysmemUidCB();
			sceSysmemUidCB.read(mem, cb);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("{0}: uid=0x{1:X}, name='{2}', status=0x{3:X}({4}), currentPriority=0x{5:X}{6}", comment, uid, sceSysmemUidCB.name, status, SceKernelThreadInfo.getStatusName(status), currentPriority, waitInfo));
				if (log.TraceEnabled)
				{
					log.trace(Utilities.getMemoryDump(address, 0x140));
				}
			}
		}

		private static void dumpThreadList(Memory mem, int list, string comment)
		{
			for (int address = mem.read32(list); address != list; address = mem.read32(address))
			{
				dumpThread(mem, address, comment);
			}
		}
	}

}