using System;
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
//	import static pspsharp.Allegrex.Common._a0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._a1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._ra;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._sp;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ERRNO_FILE_NOT_FOUND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_UNKNOWN_MODULE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.KERNEL_PARTITION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.ADDIU;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.J;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.JAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.LUI;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.LW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.SW;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;

	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using IVirtualFileSystem = pspsharp.HLE.VFS.IVirtualFileSystem;
	using Managers = pspsharp.HLE.kernel.Managers;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceIoStat = pspsharp.HLE.kernel.types.SceIoStat;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelModuleInfo = pspsharp.HLE.kernel.types.SceKernelModuleInfo;
	using SceKernelLMOption = pspsharp.HLE.kernel.types.SceKernelLMOption;
	using SceKernelSMOption = pspsharp.HLE.kernel.types.SceKernelSMOption;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using SeekableDataInput = pspsharp.filesystems.SeekableDataInput;
	using UmdIsoFile = pspsharp.filesystems.umdiso.UmdIsoFile;
	using PSP = pspsharp.format.PSP;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class ModuleMgrForUser : HLEModule
	{
		public static Logger log = Modules.getLogger("ModuleMgrForUser");
		public const int SCE_HEADER_LENGTH = 0x40;

		public class LoadModuleContext
		{
			public string fileName;
			public string moduleName;
			public int flags;
			public int uid;
			public int buffer;
			public int bufferSize;
			public SceKernelLMOption lmOption;
			public bool byUid;
			public bool needModuleInfo;
			public bool allocMem;
			public int baseAddr;
			public int basePartition;
			public SceKernelThreadInfo thread;
			public ByteBuffer moduleBuffer;

			public LoadModuleContext()
			{
				basePartition = SysMemUserForUser.USER_PARTITION_ID;
			}

			public override string ToString()
			{
				return string.Format("fileName='{0}', moduleName='{1}'", fileName, moduleName);
			}
		}

		public const int loadHLEModuleDelay = 50000; // 50 ms delay
		protected internal int startModuleHandler;
		private SysMemInfo startOptionsMem;
		private TPointer startOptions;

		public override void start()
		{
			startModuleHandler = 0;
			startOptionsMem = null;
			startOptions = null;

			base.start();
		}

		private class LoadModuleAction : IAction
		{
			private readonly ModuleMgrForUser outerInstance;

			internal LoadModuleContext loadModuleContext;

			public LoadModuleAction(ModuleMgrForUser outerInstance, LoadModuleContext loadModuleContext)
			{
				this.outerInstance = outerInstance;
				this.loadModuleContext = loadModuleContext;
			}

			public virtual void execute()
			{
				outerInstance.hleKernelLoadModuleNow(loadModuleContext);
			}
		}

		private int hleKernelLoadHLEModule(LoadModuleContext loadModuleContext)
		{
			string fileName = loadModuleContext.fileName;
			HLEModuleManager moduleManager = HLEModuleManager.Instance;

			// Extract the module name from the file name
			int startPrx = fileName.LastIndexOf("/", StringComparison.Ordinal);
			int endPrx = fileName.ToLower().IndexOf(".prx", StringComparison.Ordinal);
			if (endPrx >= 0)
			{
				loadModuleContext.moduleName = fileName.Substring(startPrx + 1, endPrx - (startPrx + 1));
			}
			else
			{
				loadModuleContext.moduleName = fileName;
			}

			if (!moduleManager.hasFlash0Module(loadModuleContext.moduleName))
			{
				// Retrieve the module name from the file content
				// if it could not be guessed from the file name.
				getModuleNameFromFileContent(loadModuleContext);
			}

			// Check if the module is not overwritten
			// by a file located under flash0 (decrypted from a real PSP).
			string modulePrxFileName = moduleManager.getModulePrxFileName(loadModuleContext.moduleName);
			if (!string.ReferenceEquals(modulePrxFileName, null))
			{
				StringBuilder localFileName = new StringBuilder();
				IVirtualFileSystem vfs = Modules.IoFileMgrForUserModule.getVirtualFileSystem(modulePrxFileName, localFileName);
				if (vfs.ioGetstat(localFileName.ToString(), new SceIoStat()) == 0)
				{
					// The flash0 file is available, load it
					loadModuleContext.fileName = modulePrxFileName;
					return -1;
				}
			}

			if (!string.ReferenceEquals(loadModuleContext.fileName, null) && loadModuleContext.fileName.StartsWith("flash0:", StringComparison.Ordinal))
			{
				StringBuilder localFileName = new StringBuilder();
				IVirtualFileSystem vfs = Modules.IoFileMgrForUserModule.getVirtualFileSystem(loadModuleContext.fileName, localFileName);
				if (vfs.ioGetstat(localFileName.ToString(), new SceIoStat()) == 0)
				{
					// The flash0 file is available, load it
					return -1;
				}
			}

			// Check if the PRX name matches an HLE module
			if (moduleManager.hasFlash0Module(loadModuleContext.moduleName))
			{
				if (log.InfoEnabled)
				{
					log.info(string.Format("hleKernelLoadModule(path='{0}') HLE module {1} loaded", loadModuleContext.fileName, loadModuleContext.moduleName));
				}
				return moduleManager.LoadFlash0Module(loadModuleContext.moduleName);
			}

			return -1;
		}

		private sbyte[] readHeader(LoadModuleContext loadModuleContext)
		{
			sbyte[] header = new sbyte[SCE_HEADER_LENGTH + PSP.PSP_HEADER_SIZE];

			if (loadModuleContext.buffer != 0)
			{
				Utilities.readBytes(loadModuleContext.buffer, header.Length, header, 0);
			}
			else
			{
				SeekableDataInput file;
				if (loadModuleContext.byUid)
				{
					file = Modules.IoFileMgrForUserModule.getFile(loadModuleContext.uid);
				}
				else
				{
					file = Modules.IoFileMgrForUserModule.getFile(loadModuleContext.fileName, IoFileMgrForUser.PSP_O_RDONLY);
				}

				if (file == null)
				{
					return null;
				}

				try
				{
					long position = file.FilePointer;
					file.readFully(header);
					file.seek(position);
				}
				catch (IOException)
				{
					return null;
				}
				finally
				{
					if (!loadModuleContext.byUid)
					{
						try
						{
							file.Dispose();
						}
						catch (IOException)
						{
							// Ignore exception
						}
					}
				}
			}

			return header;
		}

		private void getModuleNameFromFileContent(LoadModuleContext loadModuleContext)
		{
			// Extract the library name from the file itself
			// for files in "~SCE"/"~PSP" format.
			sbyte[] header = readHeader(loadModuleContext);
			if (header == null)
			{
				return;
			}

			try
			{
				ByteBuffer f = ByteBuffer.wrap(header);

				// Skip an optional "~SCE" header
				int magic = Utilities.readWord(f);
				if (magic == Loader.SCE_MAGIC)
				{
					f.position(SCE_HEADER_LENGTH);
				}
				else
				{
					f.position(0);
				}

				// Retrieve the library name from the "~PSP" header
				PSP psp = new PSP(f);
				if (psp.Valid)
				{
					string libName = psp.Modname;
					if (!string.ReferenceEquals(libName, null) && libName.Length > 0)
					{
						// We could extract the library name from the file
						loadModuleContext.moduleName = libName;
						if (log.DebugEnabled)
						{
							log.debug(string.Format("getModuleNameFromFileContent {0}", loadModuleContext));
						}
					}
				}
			}
			catch (IOException)
			{
				// Ignore exception
			}
		}

		public virtual int hleKernelLoadModule(LoadModuleContext loadModuleContext)
		{
			loadModuleContext.thread = Modules.ThreadManForUserModule.CurrentThread;
			IAction delayedLoadModule = new LoadModuleAction(this, loadModuleContext);

			Modules.ThreadManForUserModule.CurrentThread.wait.Io_id = -1;
			Modules.ThreadManForUserModule.hleBlockCurrentThread(SceKernelThreadInfo.JPCSP_WAIT_IO);
			Emulator.Scheduler.addAction(Emulator.Clock.microTime() + 100000, delayedLoadModule);

			return 0;
		}

		public virtual int hleKernelLoadAndStartModule(string name, int startPriority)
		{
			return hleKernelLoadAndStartModule(name, startPriority, null);
		}

		public virtual int hleKernelLoadAndStartModule(string name, int startPriority, IAction onModuleStartAction)
		{
			return hleKernelLoadAndStartModule(name, startPriority, 0, TPointer.NULL, onModuleStartAction);
		}

		public virtual int hleKernelLoadAndStartModule(string name, int startPriority, int argSize, TPointer argp, IAction onModuleStartAction)
		{
			LoadModuleContext loadModuleContext = new LoadModuleContext();
			loadModuleContext.fileName = name;
			loadModuleContext.allocMem = true;
			loadModuleContext.thread = Modules.ThreadManForUserModule.CurrentThread;

			int moduleUid = hleKernelLoadModuleNow(loadModuleContext);

			if (moduleUid >= 0)
			{
				if (startOptionsMem == null)
				{
					const int startOptionsSize = 20;
					startOptionsMem = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "ModuleStartOptions", SysMemUserForUser.PSP_SMEM_Low, startOptionsSize, 0);
					startOptions = new TPointer(Memory.Instance, startOptionsMem.addr);
					startOptions.setValue32(startOptionsSize);
				}

				SceKernelSMOption sceKernelSMOption = new SceKernelSMOption();
				sceKernelSMOption.mpidStack = 0;
				sceKernelSMOption.stackSize = 0;
				sceKernelSMOption.attribute = 0;
				sceKernelSMOption.priority = startPriority;
				sceKernelSMOption.write(startOptions);

				hleKernelStartModule(moduleUid, argSize, argp, TPointer32.NULL, startOptions, false, onModuleStartAction);
			}

			return moduleUid;
		}

		private int hleKernelLoadModuleNow(LoadModuleContext loadModuleContext)
		{
			int result = delayedKernelLoadModule(loadModuleContext);
			if (loadModuleContext.thread != null)
			{
				loadModuleContext.thread.cpuContext._v0 = result;
				Modules.ThreadManForUserModule.hleUnblockThread(loadModuleContext.thread.uid);
			}

			return result;
		}

		public virtual SceModule getModuleInfo(string name, ByteBuffer moduleBuffer, int mpidText, int mpidData)
		{
			SceModule module = null;
			try
			{
				module = Loader.Instance.LoadModule(name, moduleBuffer, MemoryMap.START_USERSPACE, mpidText, mpidData, true, false, true);
				moduleBuffer.rewind();
			}
			catch (IOException e)
			{
				log.error("getModuleRequiredMemorySize", e);
			}

			return module;
		}

		public virtual int getModuleRequiredMemorySize(SceModule module)
		{
			if (module == null)
			{
				return 0;
			}

			return module.loadAddressHigh - module.loadAddressLow;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int hleKernelLoadModuleFromModuleBuffer(LoadModuleContext loadModuleContext) throws java.io.IOException
		private int hleKernelLoadModuleFromModuleBuffer(LoadModuleContext loadModuleContext)
		{
			int result;

			int moduleBase;
			int mpidText;
			int mpidData;
			SysMemInfo moduleInfo = null;

			if (loadModuleContext.allocMem)
			{
				mpidText = loadModuleContext.lmOption != null && loadModuleContext.lmOption.mpidText != 0 ? loadModuleContext.lmOption.mpidText : SysMemUserForUser.USER_PARTITION_ID;
				mpidData = loadModuleContext.lmOption != null && loadModuleContext.lmOption.mpidData != 0 ? loadModuleContext.lmOption.mpidData : SysMemUserForUser.USER_PARTITION_ID;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int allocType = loadModuleContext.lmOption != null ? loadModuleContext.lmOption.position : SysMemUserForUser.PSP_SMEM_Low;
				int allocType = loadModuleContext.lmOption != null ? loadModuleContext.lmOption.position : SysMemUserForUser.PSP_SMEM_Low;
				const int moduleHeaderSize = 256;

				// Load the module in analyze mode to find out its required memory size
				SceModule testModule = getModuleInfo(loadModuleContext.fileName, loadModuleContext.moduleBuffer, mpidText, mpidData);
				int totalAllocSize = moduleHeaderSize + getModuleRequiredMemorySize(testModule);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Module '{0}' requires {1:D} bytes memory", loadModuleContext.fileName, totalAllocSize));
				}

				// Take the partition IDs from the module information, if available
				if (loadModuleContext.lmOption == null || loadModuleContext.lmOption.mpidText == 0)
				{
					if (testModule.mpidtext != 0)
					{
						mpidText = testModule.mpidtext;
					}
				}
				if (loadModuleContext.lmOption == null || loadModuleContext.lmOption.mpidData == 0)
				{
					if (testModule.mpiddata != 0)
					{
						mpidData = testModule.mpiddata;
					}
				}

				SysMemInfo testInfo = Modules.SysMemUserForUserModule.malloc(mpidText, "ModuleMgr-TestInfo", allocType, totalAllocSize, 0);
				if (testInfo == null)
				{
					log.error(string.Format("Failed module allocation of size 0x{0:X8} for '{1}' (maxFreeMemSize=0x{2:X8})", totalAllocSize, loadModuleContext.fileName, Modules.SysMemUserForUserModule.maxFreeMemSize(mpidText)));
					return -1;
				}
				int testBase = testInfo.addr;
				Modules.SysMemUserForUserModule.free(testInfo);

				// Allocate the memory for the memory header itself,
				// the space required by the module will be allocated by the Loader.
				if (loadModuleContext.needModuleInfo)
				{
					moduleInfo = Modules.SysMemUserForUserModule.malloc(mpidText, "ModuleMgr", SysMemUserForUser.PSP_SMEM_Addr, moduleHeaderSize, testBase);
					if (moduleInfo == null)
					{
						log.error(string.Format("Failed module allocation 0x{0:X8} != null for '{1}'", testBase, loadModuleContext.fileName));
						return -1;
					}
					if (moduleInfo.addr != testBase)
					{
						log.error(string.Format("Failed module allocation 0x{0:X8} != 0x{1:X8} for '{2}'", testBase, moduleInfo.addr, loadModuleContext.fileName));
						return -1;
					}
					moduleBase = moduleInfo.addr + moduleHeaderSize;
				}
				else
				{
					moduleBase = testBase;
				}

				if ((testModule.attribute & SceModule.PSP_MODULE_KERNEL) != 0 && testModule.mpidtext == KERNEL_PARTITION_ID)
				{
					moduleBase |= MemoryMap.START_KERNEL; // Set the address kernel flag 0x80000000
				}
			}
			else
			{
				moduleBase = loadModuleContext.baseAddr;
				mpidText = loadModuleContext.basePartition;
				mpidData = loadModuleContext.basePartition;
			}

			// Load the module
			SceModule module = Loader.Instance.LoadModule(loadModuleContext.fileName, loadModuleContext.moduleBuffer, moduleBase, mpidText, mpidData, false, loadModuleContext.allocMem, true);
			module.load();

			if ((module.fileFormat & Loader.FORMAT_ELF) != 0)
			{
				module.addAllocatedMemory(moduleInfo);
				result = module.modid;
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleKernelLoadModule returning uid=0x{0:X}", result));
				}
			}
			else if ((module.fileFormat & (Loader.FORMAT_SCE | Loader.FORMAT_PSP)) != 0)
			{
				// Simulate a successful loading
				log.info(string.Format("hleKernelLoadModule(path='{0}') encrypted module not loaded", loadModuleContext.fileName));
				SceModule fakeModule = new SceModule(true);
				fakeModule.modname = loadModuleContext.moduleName.ToString();
				fakeModule.addAllocatedMemory(moduleInfo);
				if (moduleInfo != null)
				{
					fakeModule.write(Memory.Instance, moduleInfo.addr);
				}
				Managers.modules.addModule(fakeModule);
				result = fakeModule.modid;
			}
			else
			{
				// The Loader class now manages the module's memory footprint, it won't allocate if it failed to load
				result = -1;
			}

			return result;
		}

		private int delayedKernelLoadModule(LoadModuleContext loadModuleContext)
		{
			int result = hleKernelLoadHLEModule(loadModuleContext);
			if (result >= 0)
			{
				Modules.ThreadManForUserModule.hleKernelDelayThread(loadHLEModuleDelay, false);
				return result;
			}

			// Load module as ELF
			try
			{
				loadModuleContext.moduleBuffer = null;
				if (loadModuleContext.buffer != 0)
				{
					sbyte[] bytes = new sbyte[loadModuleContext.bufferSize];
					IMemoryReader memoryReader = MemoryReader.getMemoryReader(loadModuleContext.buffer, loadModuleContext.bufferSize, 1);
					for (int i = 0; i < loadModuleContext.bufferSize; i++)
					{
						bytes[i] = (sbyte) memoryReader.readNext();
					}
					loadModuleContext.moduleBuffer = ByteBuffer.wrap(bytes);
				}
				else
				{
					// TODO we need to properly handle the loading byUid (sceKernelLoadModuleByID)
					// where the module to be loaded is only a part of a big file.
					// We currently assume that the file contains only the module to be loaded.
					SeekableDataInput moduleInput = Modules.IoFileMgrForUserModule.getFile(loadModuleContext.fileName, loadModuleContext.flags);
					if (moduleInput != null)
					{
						if (moduleInput is UmdIsoFile)
						{
							UmdIsoFile umdIsoFile = (UmdIsoFile) moduleInput;
							string realFileName = umdIsoFile.Name;
							if (!string.ReferenceEquals(realFileName, null) && !loadModuleContext.fileName.EndsWith(realFileName, StringComparison.Ordinal))
							{
								loadModuleContext.fileName = realFileName;
								result = hleKernelLoadHLEModule(loadModuleContext);
								if (result >= 0)
								{
									moduleInput.Dispose();
									return result;
								}
							}
						}

						sbyte[] moduleBytes = new sbyte[(int) moduleInput.length()];
						moduleInput.readFully(moduleBytes);
						moduleInput.Dispose();
						loadModuleContext.moduleBuffer = ByteBuffer.wrap(moduleBytes);
					}
				}

				if (loadModuleContext.moduleBuffer != null)
				{
					result = hleKernelLoadModuleFromModuleBuffer(loadModuleContext);
				}
				else
				{
					log.warn(string.Format("hleKernelLoadModule(path='{0}') can't find file", loadModuleContext.fileName));
					return ERROR_ERRNO_FILE_NOT_FOUND;
				}
			}
			catch (IOException e)
			{
				log.error(string.Format("hleKernelLoadModule - Error while loading module {0}", loadModuleContext.fileName), e);
				return -1;
			}

			return result;
		}

		public virtual int hleKernelStartModule(int uid, int argSize, TPointer argp, TPointer32 statusAddr, TPointer optionAddr, bool waitForThreadEnd, IAction onModuleStartAction)
		{
			SceModule sceModule = Managers.modules.getModuleByUID(uid);
			SceKernelSMOption smOption = null;
			if (optionAddr.NotNull)
			{
				smOption = new SceKernelSMOption();
				smOption.read(optionAddr);
			}

			if (sceModule == null)
			{
				log.warn(string.Format("sceKernelStartModule - unknown module UID 0x{0:X}", uid));
				return ERROR_KERNEL_UNKNOWN_MODULE;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelStartModule starting module '{0}'", sceModule.modname));
			}

			statusAddr.setValue(0);

			if (sceModule.isFlashModule)
			{
				// Trying to start a module loaded from flash0:
				// Do nothing...
				if (HLEModuleManager.Instance.hasFlash0Module(sceModule.modname))
				{
					log.info(string.Format("IGNORING:sceKernelStartModule HLE module '{0}'", sceModule.modname));
				}
				else
				{
					log.warn(string.Format("IGNORING:sceKernelStartModule flash module '{0}'", sceModule.modname));
				}
				sceModule.start();
				return sceModule.modid; // return the module id
			}

			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			int attribute = sceModule.attribute;
			int entryAddr = sceModule.entry_addr;
			if (Memory.isAddressGood(sceModule.module_start_func))
			{
				// Always take the module start function if one is defined.
				entryAddr = sceModule.module_start_func;
				if (sceModule.module_start_thread_attr != 0)
				{
					attribute = sceModule.module_start_thread_attr;
				}
			}

			if (Memory.isAddressGood(entryAddr))
			{
				int priority = 0x20;
				if (smOption != null && smOption.priority > 0)
				{
					priority = smOption.priority;
				}
				else if (sceModule.module_start_thread_priority > 0)
				{
					priority = sceModule.module_start_thread_priority;
				}

				int stackSize = 0x40000;
				if (smOption != null && smOption.stackSize > 0)
				{
					stackSize = smOption.stackSize;
				}
				else if (sceModule.module_start_thread_stacksize > 0)
				{
					stackSize = sceModule.module_start_thread_stacksize;
				}

				int mpidStack = sceModule.mpiddata;
				if (smOption != null && smOption.mpidStack > 0)
				{
					mpidStack = smOption.mpidStack;
				}

				if (smOption != null && smOption.attribute != 0)
				{
					attribute = smOption.attribute;
				}

				// Remember the current thread as it can be changed by hleKernelStartThread.
				SceKernelThreadInfo currentThread = threadMan.CurrentThread;

				SceKernelThreadInfo thread = threadMan.hleKernelCreateThread("SceModmgrStart", entryAddr, priority, stackSize, attribute, 0, mpidStack);
				// override inherited module id with the new module we are starting
				thread.moduleid = sceModule.modid;
				// Store the thread exit status into statusAddr when the thread terminates
				thread.exitStatusAddr = statusAddr;

				// Store any action that need to be executed when starting the module
				if (onModuleStartAction != null)
				{
					thread.OnThreadStartAction = onModuleStartAction;
				}

				sceModule.start();

				if (startModuleHandler != 0)
				{
					// Install the start module handler so that it is called before the module entry point.
					const int numberInstructions = 12;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int newEntryAddr = thread.getStackAddr() + thread.stackSize - numberInstructions * 4;
					int newEntryAddr = thread.StackAddr + thread.stackSize - numberInstructions * 4;
					int moduleAddr1 = (int)((uint)sceModule.address >> 16);
					int moduleAddr2 = sceModule.address & 0xFFFF;
					if (moduleAddr2 >= 0x8000)
					{
						moduleAddr1 += 1;
						moduleAddr2 = (moduleAddr2 - 0x10000) & 0xFFFF;
					}
					IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(newEntryAddr, numberInstructions * 4, 4);
					memoryWriter.writeNext(ADDIU(_sp, _sp, -16));
					memoryWriter.writeNext(SW(_a0, _sp, 0));
					memoryWriter.writeNext(SW(_a1, _sp, 4));
					memoryWriter.writeNext(SW(_ra, _sp, 8));
					memoryWriter.writeNext(LUI(_a0, moduleAddr1));
					memoryWriter.writeNext(JAL(startModuleHandler));
					memoryWriter.writeNext(ADDIU(_a0, _a0, moduleAddr2));
					memoryWriter.writeNext(LW(_a0, _sp, 0));
					memoryWriter.writeNext(LW(_a1, _sp, 4));
					memoryWriter.writeNext(LW(_ra, _sp, 8));
					memoryWriter.writeNext(J(thread.entry_addr));
					memoryWriter.writeNext(ADDIU(_sp, _sp, 16));
					memoryWriter.flush();
					thread.entry_addr = newEntryAddr;
					thread.preserveStack = true; // Do not overwrite above code

					RuntimeContext.invalidateRange(newEntryAddr, numberInstructions * 4);

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceKernelStartModule installed hook to call startModuleHandler 0x{0:X8} from 0x{1:X8} for sceModule 0x{2:X8}", startModuleHandler, newEntryAddr, sceModule.address));
					}
				}

				// Start the module start thread
				threadMan.hleKernelStartThread(thread, argSize, argp.Address, sceModule.gp_value);

				if (waitForThreadEnd)
				{
					// Wait for the end of the module start thread.
					// Do no return the thread exit status as the result of this call,
					// return the module ID.
					threadMan.hleKernelWaitThreadEnd(currentThread, thread.uid, TPointer32.NULL, false, false);
				}
			}
			else if (entryAddr == 0 || entryAddr == -1)
			{
				Modules.log.info("sceKernelStartModule - no entry address");
				sceModule.start();
			}
			else
			{
				log.warn(string.Format("sceKernelStartModule - invalid entry address 0x{0:X8}", entryAddr));
				return -1;
			}

			return sceModule.modid;
		}

		protected internal virtual int SelfModuleId
		{
			get
			{
				return Modules.ThreadManForUserModule.CurrentThread.moduleid;
			}
		}

		public virtual int hleKernelUnloadModule(int uid)
		{
			SceModule sceModule = Managers.modules.getModuleByUID(uid);
			if (sceModule == null)
			{
				log.warn(string.Format("hleKernelUnloadModule unknown module UID 0x{0:X}", uid));
				return -1;
			}
			if (sceModule.ModuleStarted && !sceModule.ModuleStopped)
			{
				log.warn(string.Format("hleKernelUnloadModule module 0x{0:X} is still running!", uid));
				return SceKernelErrors.ERROR_KERNEL_MODULE_CANNOT_REMOVE;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleKernelUnloadModule '{0}'", sceModule.modname));
			}

			sceModule.unload();
			HLEModuleManager.Instance.UnloadFlash0Module(sceModule);

			return sceModule.modid;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB7F46618, version = 150, checkInsideInterrupt = true) public int sceKernelLoadModuleByID(int uid, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0xB7F46618, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelLoadModuleByID(int uid, TPointer optionAddr)
		{
			string name = Modules.IoFileMgrForUserModule.getFileFilename(uid);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelLoadModuleByID name='{0}'", name));
			}

			SceKernelLMOption lmOption = null;
			if (optionAddr.NotNull)
			{
				lmOption = new SceKernelLMOption();
				lmOption.read(optionAddr);
				if (log.InfoEnabled)
				{
					log.info(string.Format("sceKernelLoadModuleByID options: {0}", lmOption));
				}
			}

			LoadModuleContext loadModuleContext = new LoadModuleContext();
			loadModuleContext.fileName = name;
			loadModuleContext.uid = uid;
			loadModuleContext.lmOption = lmOption;
			loadModuleContext.byUid = true;
			loadModuleContext.needModuleInfo = true;
			loadModuleContext.allocMem = true;

			return hleKernelLoadModule(loadModuleContext);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x977DE386, version = 150, checkInsideInterrupt = true) public int sceKernelLoadModule(pspsharp.HLE.PspString path, int flags, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0x977DE386, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelLoadModule(PspString path, int flags, TPointer optionAddr)
		{
			SceKernelLMOption lmOption = null;
			if (optionAddr.NotNull)
			{
				lmOption = new SceKernelLMOption();
				lmOption.read(optionAddr);
				if (log.InfoEnabled)
				{
					log.info(string.Format("sceKernelLoadModule options: {0}", lmOption));
				}
			}

			LoadModuleContext loadModuleContext = new LoadModuleContext();
			loadModuleContext.fileName = path.String;
			loadModuleContext.flags = flags;
			loadModuleContext.lmOption = lmOption;
			loadModuleContext.needModuleInfo = true;
			loadModuleContext.allocMem = true;

			return hleKernelLoadModule(loadModuleContext);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x710F61B5, version = 150, checkInsideInterrupt = true) public int sceKernelLoadModuleMs()
		[HLEFunction(nid : 0x710F61B5, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelLoadModuleMs()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF9275D98, version = 150, checkInsideInterrupt = true) public int sceKernelLoadModuleBufferUsbWlan(int bufSize, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.previousParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer buffer, int flags, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0xF9275D98, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelLoadModuleBufferUsbWlan(int bufSize, TPointer buffer, int flags, TPointer optionAddr)
		{
			SceKernelLMOption lmOption = null;
			if (optionAddr.NotNull)
			{
				lmOption = new SceKernelLMOption();
				lmOption.read(optionAddr);
				if (log.InfoEnabled)
				{
					log.info(string.Format("sceKernelLoadModuleBufferUsbWlan options: {0}", lmOption));
				}
			}

			LoadModuleContext loadModuleContext = new LoadModuleContext();
			loadModuleContext.fileName = buffer.ToString();
			loadModuleContext.flags = flags;
			loadModuleContext.buffer = buffer.Address;
			loadModuleContext.bufferSize = bufSize;
			loadModuleContext.lmOption = lmOption;
			loadModuleContext.needModuleInfo = true;
			loadModuleContext.allocMem = true;

			return Modules.ModuleMgrForUserModule.hleKernelLoadModule(loadModuleContext);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x50F0C1EC, version = 150, checkInsideInterrupt = true) public int sceKernelStartModule(int uid, int argSize, @CanBeNull pspsharp.HLE.TPointer argp, @CanBeNull pspsharp.HLE.TPointer32 statusAddr, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0x50F0C1EC, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelStartModule(int uid, int argSize, TPointer argp, TPointer32 statusAddr, TPointer optionAddr)
		{
			return hleKernelStartModule(uid, argSize, argp, statusAddr, optionAddr, true, null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLELogging(level="info") @HLEFunction(nid = 0xD1FF982A, version = 150, checkInsideInterrupt = true) public int sceKernelStopModule(int uid, int argSize, @CanBeNull pspsharp.HLE.TPointer argp, @CanBeNull pspsharp.HLE.TPointer32 statusAddr, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLELogging(level:"info"), HLEFunction(nid : 0xD1FF982A, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelStopModule(int uid, int argSize, TPointer argp, TPointer32 statusAddr, TPointer optionAddr)
		{
			SceModule sceModule = Managers.modules.getModuleByUID(uid);
			SceKernelSMOption smOption = null;
			if (optionAddr.NotNull)
			{
				smOption = new SceKernelSMOption();
				smOption.read(optionAddr);
			}

			if (sceModule == null)
			{
				log.warn("sceKernelStopModule - unknown module UID 0x" + uid.ToString("x"));
				return ERROR_KERNEL_UNKNOWN_MODULE;
			}

			statusAddr.setValue(0);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelStopModule '{0}'", sceModule.modname));
			}

			if (sceModule.isFlashModule)
			{
				// Trying to stop a module loaded from flash0:
				// Shouldn't get here...
				if (HLEModuleManager.Instance.hasFlash0Module(sceModule.modname))
				{
					log.info("IGNORING:sceKernelStopModule HLE module '" + sceModule.modname + "'");
				}
				else
				{
					log.warn("IGNORING:sceKernelStopModule flash module '" + sceModule.modname + "'");
				}
				sceModule.stop();
				return 0; // Fake success.
			}

			if (Memory.isAddressGood(sceModule.module_stop_func))
			{
				int priority = 0x20;
				if (smOption != null && smOption.priority > 0)
				{
					priority = smOption.priority;
				}
				else if (sceModule.module_stop_thread_priority > 0)
				{
					priority = sceModule.module_stop_thread_priority;
				}

				int stackSize = 0x40000;
				if (smOption != null && smOption.stackSize > 0)
				{
					stackSize = smOption.stackSize;
				}
				else if (sceModule.module_stop_thread_stacksize > 0)
				{
					stackSize = sceModule.module_stop_thread_stacksize;
				}

				int mpidStack = sceModule.mpiddata;
				if (smOption != null && smOption.mpidStack > 0)
				{
					mpidStack = smOption.mpidStack;
				}

				int attribute = sceModule.module_stop_thread_attr;
				if (smOption != null)
				{
					attribute = smOption.attribute;
				}

				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				SceKernelThreadInfo currentThread = threadMan.CurrentThread;

				SceKernelThreadInfo thread = threadMan.hleKernelCreateThread("SceModmgrStop", sceModule.module_stop_func, priority, stackSize, attribute, 0, mpidStack);

				thread.moduleid = sceModule.modid;
				// Store the thread exit status into statusAddr when the thread terminates
				thread.exitStatusAddr = statusAddr;
				sceModule.stop();

				// Start the "stop" thread...
				threadMan.hleKernelStartThread(thread, argSize, argp.Address, sceModule.gp_value);

				// ...and wait for its end.
				threadMan.hleKernelWaitThreadEnd(currentThread, thread.uid, TPointer32.NULL, false, false);
			}
			else if (sceModule.module_stop_func == 0)
			{
				log.info("sceKernelStopModule - module has no stop function");
				sceModule.stop();
			}
			else if (sceModule.ModuleStopped)
			{
				log.warn("sceKernelStopModule - module already stopped");
				return SceKernelErrors.ERROR_KERNEL_MODULE_ALREADY_STOPPED;
			}
			else
			{
				log.warn(string.Format("sceKernelStopModule - invalid stop function 0x{0:X8}", sceModule.module_stop_func));
				return -1;
			}

			return 0;
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x2E0911AA, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelUnloadModule(int uid)
		{
			return hleKernelUnloadModule(uid);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD675EBB8, version = 150, checkInsideInterrupt = true) public int sceKernelSelfStopUnloadModule(int exitCode, int argSize, @CanBeNull pspsharp.HLE.TPointer argp)
		[HLEFunction(nid : 0xD675EBB8, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelSelfStopUnloadModule(int exitCode, int argSize, TPointer argp)
		{
			SceModule sceModule = Managers.modules.getModuleByUID(SelfModuleId);
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			SceKernelThreadInfo thread = null;
			if (Memory.isAddressGood(sceModule.module_stop_func))
			{
				// Start the module stop thread function.
				thread = threadMan.hleKernelCreateThread("SceModmgrStop", sceModule.module_stop_func, sceModule.module_stop_thread_priority, sceModule.module_stop_thread_stacksize, sceModule.module_stop_thread_attr, 0, sceModule.mpiddata);
				thread.moduleid = sceModule.modid;
				// Unload the module when the stop thread will be deleted
				thread.unloadModuleAtDeletion = true;
			}
			else
			{
				// Stop and unload the module immediately
				sceModule.stop();
				sceModule.unload();
			}

			threadMan.hleKernelExitDeleteThread(); // Delete the current thread.
			if (thread != null)
			{
				threadMan.hleKernelStartThread(thread, argSize, argp.Address, sceModule.gp_value);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8F2DF740, version = 150, checkInsideInterrupt = true) public int sceKernelStopUnloadSelfModuleWithStatus(int exitCode, int argSize, @CanBeNull pspsharp.HLE.TPointer argp, @CanBeNull pspsharp.HLE.TPointer32 statusAddr, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0x8F2DF740, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelStopUnloadSelfModuleWithStatus(int exitCode, int argSize, TPointer argp, TPointer32 statusAddr, TPointer optionAddr)
		{
			SceModule sceModule = Managers.modules.getModuleByUID(SelfModuleId);

			if (log.InfoEnabled)
			{
				log.info(string.Format("sceKernelStopUnloadSelfModuleWithStatus {0}, exitCode=0x{1:X}", sceModule, exitCode));
			}

			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			SceKernelThreadInfo thread = null;
			statusAddr.setValue(0);
			if (Memory.isAddressGood(sceModule.module_stop_func))
			{
				// Start the module stop thread function.
				statusAddr.setValue(0); // TODO set to return value of the thread (when it exits, of course)

				thread = threadMan.hleKernelCreateThread("SceModmgrStop", sceModule.module_stop_func, sceModule.module_stop_thread_priority, sceModule.module_stop_thread_stacksize, sceModule.module_stop_thread_attr, optionAddr.Address, sceModule.mpiddata);
				thread.moduleid = sceModule.modid;
				// Store the thread exit status into statusAddr when the thread terminates
				thread.exitStatusAddr = statusAddr;
				threadMan.CurrentThread.exitStatus = exitCode; // Set the current thread's exit status.
				// Unload the module when the stop thread will be deleted
				thread.unloadModuleAtDeletion = true;
			}
			else
			{
				// Stop and unload the module immediately
				sceModule.stop();
				sceModule.unload();
			}

			threadMan.hleKernelExitDeleteThread(); // Delete the current thread.
			if (thread != null)
			{
				threadMan.hleKernelStartThread(thread, argSize, argp.Address, sceModule.gp_value);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xCC1D3699, version = 150, checkInsideInterrupt = true) public int sceKernelStopUnloadSelfModule(int argSize, @CanBeNull pspsharp.HLE.TPointer argp, @CanBeNull pspsharp.HLE.TPointer32 statusAddr, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0xCC1D3699, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelStopUnloadSelfModule(int argSize, TPointer argp, TPointer32 statusAddr, TPointer optionAddr)
		{
			SceModule sceModule = Managers.modules.getModuleByUID(SelfModuleId);

			if (log.InfoEnabled)
			{
				log.info(string.Format("sceKernelStopUnloadSelfModule {0}", sceModule));
			}

			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			SceKernelThreadInfo thread = null;
			statusAddr.setValue(0);
			if (Memory.isAddressGood(sceModule.module_stop_func))
			{
				// Start the module stop thread function.
				thread = threadMan.hleKernelCreateThread("SceModmgrStop", sceModule.module_stop_func, sceModule.module_stop_thread_priority, sceModule.module_stop_thread_stacksize, sceModule.module_stop_thread_attr, optionAddr.Address, sceModule.mpiddata);
				thread.moduleid = sceModule.modid;
				// Store the thread exit status into statusAddr when the thread terminates
				thread.exitStatusAddr = statusAddr;
				// Unload the module when the stop thread will be deleted
				thread.unloadModuleAtDeletion = true;
			}
			else
			{
				// Stop and unload the module immediately
				sceModule.stop();
				sceModule.unload();
			}

			threadMan.hleKernelExitDeleteThread(); // Delete the current thread.
			if (thread != null)
			{
				threadMan.hleKernelStartThread(thread, argSize, argp.Address, sceModule.gp_value);
			}

			return 0;
		}

		/// <summary>
		/// Get a list of module IDs. </summary>
		/// <param name="resultBuffer">      Buffer to store the module list </param>
		/// <param name="resultBufferSize">  Number of bytes in the resultBuffer </param>
		/// <param name="idCountAddr">       Returns the number of module ids </param>
		/// <returns> >= 0 on success  </returns>
		[HLEFunction(nid : 0x644395E2, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelGetModuleIdList(TPointer32 resultBuffer, int resultBufferSize, TPointer32 idCountAddr)
		{
			int idCount = 0;
			int resultBufferOffset = 0;
			foreach (SceModule module in Managers.modules.values())
			{
				if (!module.isFlashModule && module.isLoaded)
				{
					if (resultBufferOffset < resultBufferSize)
					{
						resultBuffer.setValue(resultBufferOffset, module.modid);
						resultBufferOffset += 4;
					}
					idCount++;
				}
			}
			idCountAddr.setValue(idCount);

			return 0;
		}

		[HLEFunction(nid : 0x748CBED9, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelQueryModuleInfo(int uid, TPointer infoAddr)
		{
			SceModule sceModule = Managers.modules.getModuleByUID(uid);
			if (sceModule == null)
			{
				log.warn("sceKernelQueryModuleInfo unknown module UID 0x" + uid.ToString("x"));
				return -1;
			}

			SceKernelModuleInfo moduleInfo = new SceKernelModuleInfo();
			moduleInfo.copy(sceModule);
			moduleInfo.write(infoAddr);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelQueryModuleInfo returning {0}", Utilities.getMemoryDump(infoAddr.Address, infoAddr.getValue32())));
			}

			return 0;
		}

		[HLEFunction(nid : 0xF0A26395, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelGetModuleId()
		{
			int moduleId = SelfModuleId;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelGetModuleId returning 0x{0:X}", moduleId));
			}

			return moduleId;
		}

		[HLEFunction(nid : 0xD8B73127, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelGetModuleIdByAddress(TPointer addr)
		{
			SceModule module = Managers.modules.getModuleByAddress(addr.Address);
			if (module == null)
			{
				log.warn(string.Format("sceKernelGetModuleIdByAddress addr={0} module not found", addr));
				return -1;
			}

			return module.modid;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA1A78C58, version = 150) public int sceKernelLoadModuleDisc(pspsharp.HLE.PspString path, int flags, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0xA1A78C58, version : 150)]
		public virtual int sceKernelLoadModuleDisc(PspString path, int flags, TPointer optionAddr)
		{
			SceKernelLMOption lmOption = null;
			if (optionAddr.NotNull)
			{
				lmOption = new SceKernelLMOption();
				lmOption.read(optionAddr);
				if (log.InfoEnabled)
				{
					log.info(string.Format("sceKernelLoadModuleDisc options: {0}", lmOption));
				}
			}

			LoadModuleContext loadModuleContext = new LoadModuleContext();
			loadModuleContext.fileName = path.String;
			loadModuleContext.flags = flags;
			loadModuleContext.lmOption = lmOption;
			loadModuleContext.allocMem = true;

			return hleKernelLoadModule(loadModuleContext);
		}

		/// <summary>
		/// Sets a function to be called just before module_start of a module is gonna be called (useful for patching purposes)
		/// </summary>
		/// <param name="startModuleHandler"> - The function, that will receive the module structure before the module is started.
		/// 
		/// @returns - The previous set function (NULL if none);
		/// @Note: because only one handler function is handled by HEN, you should
		///        call the previous function in your code.
		/// 
		/// @Example: 
		/// 
		/// STMOD_HANDLER previous = NULL;
		/// 
		/// int OnModuleStart(SceModule2 *mod);
		/// 
		/// void somepointofmycode()
		/// {
		///     previous = sctrlHENSetStartModuleHandler(OnModuleStart);
		/// }
		/// 
		/// int OnModuleStart(SceModule2 *mod)
		/// {
		///     if (strcmp(mod->modname, "vsh_module") == 0)
		///     {
		///         // Do something with vsh module here
		///     }
		/// 
		///     if (!previous)
		///         return 0;
		/// 
		///     // Call previous handler
		/// 
		///     return previous(mod);
		/// }
		/// 
		/// @Note2: The above example should be compiled with the flag -fno-pic
		///         in order to avoid problems with gp register that may lead to a crash.
		///  </param>
		[HLEFunction(nid : 0x1C90BECB, version : 150)]
		public virtual int sctrlHENSetStartModuleHandler(TPointer startModuleHandler)
		{
			int previousStartModuleHandler = this.startModuleHandler;

			this.startModuleHandler = startModuleHandler.Address;

			return previousStartModuleHandler;
		}

		/// <summary>
		/// Finds a driver 
		/// </summary>
		/// <param name="drvname"> - The name of the driver (without ":" or numbers) 
		/// 
		/// @returns the driver if found, NULL otherwise 
		///  </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x78E46415, version = 150) public int sctrlHENFindDriver(String drvname)
		[HLEFunction(nid : 0x78E46415, version : 150)]
		public virtual int sctrlHENFindDriver(string drvname)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFEF27DC1, version = 271, checkInsideInterrupt = true) public int sceKernelLoadModuleDNAS(pspsharp.HLE.PspString path, pspsharp.HLE.TPointer key, int unknown, @CanBeNull pspsharp.HLE.TPointer32 optionAddr)
		[HLEFunction(nid : 0xFEF27DC1, version : 271, checkInsideInterrupt : true)]
		public virtual int sceKernelLoadModuleDNAS(PspString path, TPointer key, int unknown, TPointer32 optionAddr)
		{
			SceKernelLMOption lmOption = null;
			if (optionAddr.NotNull)
			{
				lmOption = new SceKernelLMOption();
				lmOption.read(optionAddr);
				if (log.InfoEnabled)
				{
					log.info(string.Format("sceKernelLoadModuleDNAS options: {0}", lmOption));
				}
			}

			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = Modules.IoFileMgrForUserModule.getVirtualFileSystem(path.String, localFileName);
			if (vfs != null)
			{
				IVirtualFile vFile = vfs.ioOpen(localFileName.ToString(), IoFileMgrForUser.PSP_O_RDONLY, 0);
				if (vFile == null)
				{
					return ERROR_ERRNO_FILE_NOT_FOUND;
				}
			}
			else
			{
				return SceKernelErrors.ERROR_ERRNO_DEVICE_NOT_FOUND;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF2D8D1B4, version = 271) public int sceKernelLoadModuleNpDrm(pspsharp.HLE.PspString path, int flags, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0xF2D8D1B4, version : 271)]
		public virtual int sceKernelLoadModuleNpDrm(PspString path, int flags, TPointer optionAddr)
		{
			SceKernelLMOption lmOption = null;
			if (optionAddr.NotNull)
			{
				lmOption = new SceKernelLMOption();
				lmOption.read(optionAddr);
				if (log.InfoEnabled)
				{
					log.info(string.Format("sceKernelLoadModuleNpDrm options: {0}", lmOption));
				}
			}

			// SPRX modules can't be decrypted yet.
			if (!Modules.scePspNpDrm_userModule.DisableDLCStatus)
			{
				log.warn(string.Format("sceKernelLoadModuleNpDrm detected encrypted DLC module: {0}", path.String));
				return SceKernelErrors.ERROR_NPDRM_INVALID_PERM;
			}

			LoadModuleContext loadModuleContext = new LoadModuleContext();
			loadModuleContext.fileName = path.String;
			loadModuleContext.flags = flags;
			loadModuleContext.lmOption = lmOption;
			loadModuleContext.needModuleInfo = true;
			loadModuleContext.allocMem = true;

			return hleKernelLoadModule(loadModuleContext);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE4C4211C, version = 150, checkInsideInterrupt = true) public int sceKernelLoadModuleWithBlockOffset(pspsharp.HLE.PspString path, int memoryBlockId, int memoryBlockOffset, int flags)
		[HLEFunction(nid : 0xE4C4211C, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelLoadModuleWithBlockOffset(PspString path, int memoryBlockId, int memoryBlockOffset, int flags)
		{
			return 0;
		}

		[HLEFunction(nid : 0xD2FBC957, version : 150)]
		public virtual int sceKernelGetModuleGPByAddress(int address, TPointer32 gpAddr)
		{
			SceModule module = Managers.modules.getModuleByAddress(address);
			if (module == null)
			{
				log.warn(string.Format("sceKernelGetModuleGPByAddress not found module address=0x{0:X8}", address));
				return -1;
			}

			gpAddr.setValue(module.gp_value);

			return 0;
		}

		[HLEFunction(nid : 0x22BDBEFF, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelQueryModuleInfo_660(int uid, TPointer infoAddr)
		{
			return sceKernelQueryModuleInfo(uid, infoAddr);
		}
	}
}