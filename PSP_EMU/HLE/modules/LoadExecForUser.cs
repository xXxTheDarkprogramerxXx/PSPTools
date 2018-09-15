using System;

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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_PROHIBIT_LOADEXEC_DEVICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_ILLEGAL_LOADEXEC_FILENAME;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using XmbIsoVirtualFile = pspsharp.HLE.VFS.xmb.XmbIsoVirtualFile;
	using SceKernelCallbackInfo = pspsharp.HLE.kernel.types.SceKernelCallbackInfo;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using SeekableDataInput = pspsharp.filesystems.SeekableDataInput;
	using UmdIsoReader = pspsharp.filesystems.umdiso.UmdIsoReader;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using Utilities = pspsharp.util.Utilities;

	//using Logger = org.apache.log4j.Logger;

	public class LoadExecForUser : HLEModule
	{
		//public static Logger log = Modules.getLogger("LoadExecForUser");
		protected internal int registeredExitCallbackUid;
		protected internal const string encryptedBootPath = "disc0:/PSP_GAME/SYSDIR/EBOOT.BIN";
		protected internal const string unencryptedBootPath = "disc0:/PSP_GAME/SYSDIR/BOOT.BIN";

		public virtual void triggerExitCallback()
		{
			Modules.ThreadManForUserModule.hleKernelNotifyCallback(SceKernelThreadInfo.THREAD_CALLBACK_EXIT, 0);
		}

		public virtual int hleKernelLoadExec(ByteBuffer moduleBuffer, int argSize, int argAddr, string moduleFileName, UmdIsoReader iso)
		{
			sbyte[] arguments = null;
			if (argSize > 0)
			{
				// Save the memory content for the arguments because
				// the memory would be overwritten by the loading of the new module.
				arguments = new sbyte[argSize];
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(argAddr, argSize, 1);
				for (int i = 0; i < argSize; i++)
				{
					arguments[i] = (sbyte) memoryReader.readNext();
				}
			}

			// Flush system memory to mimic a real PSP reset.
			Modules.SysMemUserForUserModule.reset();

			try
			{
				if (moduleBuffer != null)
				{
					SceModule module = Emulator.Instance.load(moduleFileName, moduleBuffer, true);
					Emulator.Clock.resume();

					// After a sceKernelLoadExec, host0: is relative to the directory where
					// the loaded file (prx) was located.
					// E.g.:
					//  after
					//    sceKernelLoadExec("disc0:/PSP_GAME/USRDIR/A.PRX")
					//  the following file access
					//    sceIoOpen("host0:B")
					//  is actually referencing the file
					//    disc0:/PSP_GAME/USRDIR/B
					if (!string.ReferenceEquals(moduleFileName, null))
					{
						int pathIndex = moduleFileName.LastIndexOf("/", StringComparison.Ordinal);
						if (pathIndex >= 0)
						{
							Modules.IoFileMgrForUserModule.Host0Path = moduleFileName.Substring(0, pathIndex + 1);
						}
					}

					if ((module.fileFormat & Loader.FORMAT_ELF) != Loader.FORMAT_ELF)
					{
						Console.WriteLine("sceKernelLoadExec - failed, target is not an ELF");
						throw new SceKernelErrorException(ERROR_KERNEL_ILLEGAL_LOADEXEC_FILENAME);
					}

					// Set the given arguments to the root thread.
					// Do not pass the file name as first parameter (tested on PSP).
					SceKernelThreadInfo rootThread = Modules.ThreadManForUserModule.getRootThread(module);
					Modules.ThreadManForUserModule.hleKernelSetThreadArguments(rootThread, arguments, argSize);

					// The memory model (32MB / 64MB) could have been changed, update the RuntimeContext
					RuntimeContext.updateMemory();

					if (iso != null)
					{
						Modules.IoFileMgrForUserModule.IsoReader = iso;
						Modules.sceUmdUserModule.IsoReader = iso;
					}
				}
			}
			catch (GeneralJpcspException e)
			{
				Console.WriteLine("General Error", e);
				Emulator.PauseEmu();
			}
			catch (IOException e)
			{
				Console.WriteLine(string.Format("sceKernelLoadExec - Error while loading module '{0}'", moduleFileName), e);
				return ERROR_KERNEL_PROHIBIT_LOADEXEC_DEVICE;
			}

			return 0;
		}

		public virtual int hleKernelLoadExec(PspString filename, int argSize, int argAddr)
		{
			string name = filename.String;

			// The PSP is replacing a loadexec of disc0:/PSP_GAME/SYSDIR/BOOT.BIN with EBOOT.BIN
			if (name.Equals(unencryptedBootPath))
			{
				log.info(string.Format("sceKernelLoadExec '{0}' replaced by '{1}'", name, encryptedBootPath));
				name = encryptedBootPath;
			}

			ByteBuffer moduleBuffer = null;

			IVirtualFile vFile = Modules.IoFileMgrForUserModule.getVirtualFile(name, IoFileMgrForUser.PSP_O_RDONLY, 0);
			UmdIsoReader iso = null;
			if (vFile is XmbIsoVirtualFile)
			{
				try
				{
					IVirtualFile vFileLoadExec = ((XmbIsoVirtualFile) vFile).ioReadForLoadExec();
					if (vFileLoadExec != null)
					{
						iso = ((XmbIsoVirtualFile) vFile).IsoReader;

						vFile.ioClose();
						vFile = vFileLoadExec;
					}
				}
				catch (IOException e)
				{
					Console.WriteLine("hleKernelLoadExec", e);
				}
			}

			if (vFile != null)
			{
				sbyte[] moduleBytes = Utilities.readCompleteFile(vFile);
				vFile.ioClose();
				if (moduleBytes != null)
				{
					moduleBuffer = ByteBuffer.wrap(moduleBytes);
				}
			}
			else
			{
				SeekableDataInput moduleInput = Modules.IoFileMgrForUserModule.getFile(name, IoFileMgrForUser.PSP_O_RDONLY);
				if (moduleInput != null)
				{
					try
					{
						sbyte[] moduleBytes = new sbyte[(int) moduleInput.Length()];
						moduleInput.readFully(moduleBytes);
						moduleInput.Dispose();
						moduleBuffer = ByteBuffer.wrap(moduleBytes);
					}
					catch (IOException e)
					{
						Console.WriteLine(string.Format("sceKernelLoadExec - Error while loading module '{0}'", name), e);
						return ERROR_KERNEL_PROHIBIT_LOADEXEC_DEVICE;
					}
				}
			}

			return hleKernelLoadExec(moduleBuffer, argSize, argAddr, name, iso);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLELogging(level="info") @HLEFunction(nid = 0xBD2F1094, version = 150, checkInsideInterrupt = true) public int sceKernelLoadExec(pspsharp.HLE.PspString filename, @CanBeNull pspsharp.HLE.TPointer32 optionAddr)
		[HLELogging(level:"info"), HLEFunction(nid : 0xBD2F1094, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelLoadExec(PspString filename, TPointer32 optionAddr)
		{
			int argSize = 0;
			int argAddr = 0;
			if (optionAddr.NotNull)
			{
				int optSize = optionAddr.getValue(0); // Size of the option struct.
				if (optSize >= 16)
				{
					argSize = optionAddr.getValue(4); // Size of memory required for arguments.
					argAddr = optionAddr.getValue(8); // Arguments (memory area of size argSize).
					int keyAddr = optionAddr.getValue(12); // Pointer to an encryption key (may not be used).

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("sceKernelLoadExec params: optSize={0:D}, argSize={1:D}, argAddr=0x{2:X8}, keyAddr=0x{3:X8}: {4}", optSize, argSize, argAddr, keyAddr, Utilities.getMemoryDump(argAddr, argSize)));
					}

				}
			}

			return hleKernelLoadExec(filename, argSize, argAddr);
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x2AC9954B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelExitGameWithStatus(int status)
		{
			Emulator.PauseEmuWithStatus(status);
			RuntimeContext.reset();
			Modules.ThreadManForUserModule.stop();

			return 0;
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x05572A5F, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelExitGame()
		{
			Emulator.PauseEmu();
			RuntimeContext.reset();
			Modules.ThreadManForUserModule.stop();

			return 0;
		}

		[HLEFunction(nid : 0x4AC57943, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelRegisterExitCallback(int uid)
		{
			if (Modules.ThreadManForUserModule.hleKernelRegisterCallback(SceKernelThreadInfo.THREAD_CALLBACK_EXIT, uid))
			{
				registeredExitCallbackUid = uid;
			}

			return 0;
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x362A956B, version : 500)]
		public virtual int LoadExecForUser_362A956B()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("LoadExecForUser_362A956B registeredExitCallbackUid=0x{0:X}", registeredExitCallbackUid));
			}

			SceKernelCallbackInfo callbackInfo = Modules.ThreadManForUserModule.getCallbackInfo(registeredExitCallbackUid);
			if (callbackInfo == null)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("LoadExecForUser_362A956B registeredExitCallbackUid=0x{0:x} callback not found", registeredExitCallbackUid));
				}
				return SceKernelErrors.ERROR_KERNEL_NOT_FOUND_CALLBACK;
			}
			int callbackArgument = callbackInfo.CallbackArgument;
			if (!Memory.isAddressGood(callbackArgument))
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("LoadExecForUser_362A956B invalid address for callbackArgument=0x{0:X8}", callbackArgument));
				}
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_ADDR;
			}

			Memory mem = Processor.memory;

			int unknown1 = mem.read32(callbackArgument - 8);
			if (unknown1 < 0 || unknown1 >= 4)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("LoadExecForUser_362A956B invalid value unknown1=0x{0:X8}", unknown1));
				}
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_ARGUMENT;
			}

			int parameterArea = mem.read32(callbackArgument - 4);
			if (!Memory.isAddressGood(parameterArea))
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("LoadExecForUser_362A956B invalid address for parameterArea=0x{0:X8}", parameterArea));
				}
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_ADDR;
			}

			int size = mem.read32(parameterArea);
			if (size < 12)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("LoadExecForUser_362A956B invalid parameter area size {0:D}", size));
				}
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_SIZE;
			}

			mem.write32(parameterArea + 4, 0);
			mem.write32(parameterArea + 8, -1);

			return 0;
		}
	}
}