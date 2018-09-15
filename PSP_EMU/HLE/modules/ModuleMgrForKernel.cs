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
namespace pspsharp.HLE.modules
{

	//using Logger = org.apache.log4j.Logger;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using IVirtualFileSystem = pspsharp.HLE.VFS.IVirtualFileSystem;
	using Managers = pspsharp.HLE.kernel.Managers;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelLMOption = pspsharp.HLE.kernel.types.SceKernelLMOption;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using LoadModuleContext = pspsharp.HLE.modules.ModuleMgrForUser.LoadModuleContext;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using Utilities = pspsharp.util.Utilities;

	public class ModuleMgrForKernel : HLEModule
	{
		//public static Logger log = Modules.getLogger("ModuleMgrForKernel");
		private ISet<string> modulesWithMemoryAllocated;

		public override void start()
		{
			modulesWithMemoryAllocated = new HashSet<>();

			base.start();
		}

		public virtual bool isMemoryAllocatedForModule(string moduleName)
		{
			if (modulesWithMemoryAllocated == null)
			{
				return false;
			}
			return modulesWithMemoryAllocated.Contains(moduleName);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xBA889C07, version = 150) public int sceKernelLoadModuleBuffer(pspsharp.HLE.TPointer buffer, int bufSize, int flags, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0xBA889C07, version : 150)]
		public virtual int sceKernelLoadModuleBuffer(TPointer buffer, int bufSize, int flags, TPointer optionAddr)
		{
			SceKernelLMOption lmOption = null;
			if (optionAddr.NotNull)
			{
				lmOption = new SceKernelLMOption();
				lmOption.read(optionAddr);
				if (log.InfoEnabled)
				{
					log.info(string.Format("sceKernelLoadModuleBuffer options: {0}", lmOption));
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

		/// <summary>
		/// Load a module with the VSH apitype.
		/// </summary>
		/// <param name="path">        The path to the module to load. </param>
		/// <param name="flags">       Unused, always 0 . </param>
		/// <param name="optionAddr">  Pointer to a mod_param_t structure. Can be NULL.
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLELogging(level = "info") @HLEFunction(nid = 0xD5DDAB1F, version = 150) public int sceKernelLoadModuleVSH(pspsharp.HLE.PspString path, int flags, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLELogging(level : "info"), HLEFunction(nid : 0xD5DDAB1F, version : 150)]
		public virtual int sceKernelLoadModuleVSH(PspString path, int flags, TPointer optionAddr)
		{
			SceKernelLMOption lmOption = null;
			if (optionAddr.NotNull)
			{
				lmOption = new SceKernelLMOption();
				lmOption.read(optionAddr);
				if (log.InfoEnabled)
				{
					log.info(string.Format("sceKernelLoadModuleVSH options: {0}", lmOption));
				}
			}

			LoadModuleContext loadModuleContext = new LoadModuleContext();
			loadModuleContext.fileName = path.String;
			loadModuleContext.flags = flags;
			loadModuleContext.lmOption = lmOption;
			loadModuleContext.needModuleInfo = true;
			loadModuleContext.allocMem = true;

			return Modules.ModuleMgrForUserModule.hleKernelLoadModule(loadModuleContext);
		}

		[HLEFunction(nid : 0xD86DD11B, version : 150)]
		public virtual int sceKernelSearchModuleByName(PspString name)
		{
			SceModule module = Managers.modules.getModuleByName(name.String);
			if (module == null)
			{
				return SceKernelErrors.ERROR_KERNEL_UNKNOWN_MODULE;
			}

			return module.modid;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x939E4270, version = 150, checkInsideInterrupt = true) public int sceKernelLoadModule_660(pspsharp.HLE.PspString path, int flags, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0x939E4270, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelLoadModule_660(PspString path, int flags, TPointer optionAddr)
		{
			return Modules.ModuleMgrForUserModule.sceKernelLoadModule(path, flags, optionAddr);
		}

		[HLEFunction(nid : 0x387E3CA9, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelUnloadModule_660(int uid)
		{
			return Modules.ModuleMgrForUserModule.sceKernelUnloadModule(uid);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x3FF74DF1, version = 150, checkInsideInterrupt = true) public int sceKernelStartModule_660(int uid, int argSize, @CanBeNull pspsharp.HLE.TPointer argp, @CanBeNull pspsharp.HLE.TPointer32 statusAddr, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0x3FF74DF1, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelStartModule_660(int uid, int argSize, TPointer argp, TPointer32 statusAddr, TPointer optionAddr)
		{
			return Modules.ModuleMgrForUserModule.sceKernelStartModule(uid, argSize, argp, statusAddr, optionAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE5D6087B, version = 150, checkInsideInterrupt = true) public int sceKernelStopModule_660(int uid, int argSize, @CanBeNull pspsharp.HLE.TPointer argp, @CanBeNull pspsharp.HLE.TPointer32 statusAddr, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0xE5D6087B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelStopModule_660(int uid, int argSize, TPointer argp, TPointer32 statusAddr, TPointer optionAddr)
		{
			return Modules.ModuleMgrForUserModule.sceKernelStopModule(uid, argSize, argp, statusAddr, optionAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD4EE2D26, version = 150, checkInsideInterrupt = true) public int sceKernelLoadModuleToBlock(pspsharp.HLE.PspString path, int blockId, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 separatedBlockId, int unknown2, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0xD4EE2D26, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelLoadModuleToBlock(PspString path, int blockId, TPointer32 separatedBlockId, int unknown2, TPointer optionAddr)
		{
			SceKernelLMOption lmOption = null;
			if (optionAddr.NotNull)
			{
				lmOption = new SceKernelLMOption();
				lmOption.read(optionAddr);
				if (log.InfoEnabled)
				{
					log.info(string.Format("sceKernelLoadModuleToBlock options: {0}", lmOption));
				}
			}

			SysMemInfo sysMemInfo = Modules.SysMemUserForUserModule.getSysMemInfo(blockId);
			if (sysMemInfo == null)
			{
				return -1;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelLoadModuleToBlock sysMemInfo={0}", sysMemInfo));
			}

			modulesWithMemoryAllocated.Add(path.String);

			// If we cannot load the module file, return the same blockId
			separatedBlockId.setValue(blockId);

			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = Modules.IoFileMgrForUserModule.getVirtualFileSystem(path.String, localFileName);
			if (vfs != null)
			{
				IVirtualFile vFile = vfs.ioOpen(localFileName.ToString(), IoFileMgrForUser.PSP_O_RDONLY, 0);
				if (vFile != null)
				{
					sbyte[] bytes = new sbyte[(int) vFile.Length()];
					int Length = vFile.ioRead(bytes, 0, bytes.Length);
					ByteBuffer moduleBuffer = ByteBuffer.wrap(bytes, 0, Length);

					SceModule module = Modules.ModuleMgrForUserModule.getModuleInfo(path.String, moduleBuffer, sysMemInfo.partitionid, sysMemInfo.partitionid);
					if (module != null)
					{
						int size = Modules.ModuleMgrForUserModule.getModuleRequiredMemorySize(module);

						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("sceKernelLoadModuleToBlock module requiring 0x{0:X} bytes", size));
						}

						// Aligned on 256 bytes boundary
						size = Utilities.alignUp(size, 0xFF);
						SysMemInfo separatedSysMemInfo = Modules.SysMemUserForUserModule.separateMemoryBlock(sysMemInfo, size);
						// This is the new blockId after calling sceKernelSeparateMemoryBlock
						separatedBlockId.setValue(separatedSysMemInfo.uid);

						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("sceKernelLoadModuleToBlock separatedSysMemInfo={0}", separatedSysMemInfo));
						}
					}
				}
			}

			LoadModuleContext loadModuleContext = new LoadModuleContext();
			loadModuleContext.fileName = path.String;
			loadModuleContext.lmOption = lmOption;
			loadModuleContext.needModuleInfo = true;
			loadModuleContext.allocMem = false;
			loadModuleContext.baseAddr = sysMemInfo.addr;
			loadModuleContext.basePartition = sysMemInfo.partitionid;

			return Modules.ModuleMgrForUserModule.hleKernelLoadModule(loadModuleContext);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCC873DFA, version = 150) public int sceKernelRebootBeforeForUser(pspsharp.HLE.TPointer param)
		[HLEFunction(nid : 0xCC873DFA, version : 150)]
		public virtual int sceKernelRebootBeforeForUser(TPointer param)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9B7102E2, version = 150) public int sceKernelRebootPhaseForKernel(int unknown1, pspsharp.HLE.TPointer param, int unknown2, int unknown3)
		[HLEFunction(nid : 0x9B7102E2, version : 150)]
		public virtual int sceKernelRebootPhaseForKernel(int unknown1, TPointer param, int unknown2, int unknown3)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5FC3B3DA, version = 150) public int sceKernelRebootBeforeForKernel(pspsharp.HLE.TPointer param, int unknown1, int unknown2, int unknown3)
		[HLEFunction(nid : 0x5FC3B3DA, version : 150)]
		public virtual int sceKernelRebootBeforeForKernel(TPointer param, int unknown1, int unknown2, int unknown3)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC3DDABEF, version = 150) public int ModuleMgrForKernel_C3DDABEF()
		[HLEFunction(nid : 0xC3DDABEF, version : 150)]
		public virtual int ModuleMgrForKernel_C3DDABEF()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1CF0B794, version = 150) public int sceKernelLoadModuleBufferBootInitBtcnf(int modSize, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.previousParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer modBuf, int flags, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.variableLength, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer option, int unknown)
		[HLEFunction(nid : 0x1CF0B794, version : 150)]
		public virtual int sceKernelLoadModuleBufferBootInitBtcnf(int modSize, TPointer modBuf, int flags, TPointer option, int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x955D6CB2, version = 150) public int sceKernelLoadModuleBootInitBtcnf(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=256, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer modBuf, int flags, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.variableLength, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer option)
		[HLEFunction(nid : 0x955D6CB2, version : 150)]
		public virtual int sceKernelLoadModuleBootInitBtcnf(TPointer modBuf, int flags, TPointer option)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4E38EA1D, version = 150) public int sceKernelLoadModuleBufferForRebootKernel(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=256, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer modBuf, int flags, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.variableLength, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer option, int unknown)
		[HLEFunction(nid : 0x4E38EA1D, version : 150)]
		public virtual int sceKernelLoadModuleBufferForRebootKernel(TPointer modBuf, int flags, TPointer option, int unknown)
		{
			return 0;
		}
	}

}