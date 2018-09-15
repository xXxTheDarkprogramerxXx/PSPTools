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

	//using Logger = org.apache.log4j.Logger;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceKernelLoadExecVSHParam = pspsharp.HLE.kernel.types.SceKernelLoadExecVSHParam;
	using Settings = pspsharp.settings.Settings;
	using Utilities = pspsharp.util.Utilities;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;

	public class LoadExecForKernel : HLEModule
	{
		//public static Logger log = Modules.getLogger("LoadExecForKernel");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA3D5E142, version = 150) public int sceKernelExitVSHVSH(@CanBeNull pspsharp.HLE.TPointer param)
		[HLEFunction(nid : 0xA3D5E142, version : 150)]
		public virtual int sceKernelExitVSHVSH(TPointer param)
		{
			// when called in game mode it will have the same effect that sceKernelExitGame 
			if (param.NotNull)
			{
				log.info(string.Format("sceKernelExitVSHVSH param={0}", Utilities.getMemoryDump(param.Address, 36)));
			}
			Emulator.PauseEmu();
			RuntimeContext.reset();
			Modules.ThreadManForUserModule.stop();
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x6D302D3D, version = 150) public int sceKernelExitVSHKernel(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.variableLength, usage=pspsharp.HLE.BufferInfo.Usage.in) @CanBeNull pspsharp.HLE.TPointer param)
		[HLEFunction(nid : 0x6D302D3D, version : 150)]
		public virtual int sceKernelExitVSHKernel(TPointer param)
		{
			SceKernelLoadExecVSHParam loadExecVSHParam = new SceKernelLoadExecVSHParam();
			loadExecVSHParam.read(param);

			//  Test in real PSP in  "Hatsune Miku Project Diva Extend" chinese patched version, same effect as sceKernelExitGame
			if (param.NotNull)
			{
				log.info(string.Format("sceKernelExitVSHKernel param={0}", loadExecVSHParam));
			}
			Emulator.PauseEmu();
			RuntimeContext.reset();
			Modules.ThreadManForUserModule.stop();
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC3474C2A, version = 660) public int sceKernelExitVSHKernel_660(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.variableLength, usage=pspsharp.HLE.BufferInfo.Usage.in) @CanBeNull pspsharp.HLE.TPointer param)
		[HLEFunction(nid : 0xC3474C2A, version : 660)]
		public virtual int sceKernelExitVSHKernel_660(TPointer param)
		{
			return sceKernelExitVSHKernel(param);
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x28D0D249, version : 150)]
		public virtual int sceKernelLoadExecVSHMs2(PspString filename, TPointer param)
		{
			SceKernelLoadExecVSHParam loadExecVSHParam = new SceKernelLoadExecVSHParam();
			loadExecVSHParam.read(param);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelLoadExecVSHMs2 param: {0}", loadExecVSHParam));
				if (loadExecVSHParam.args > 0)
				{
					Console.WriteLine(string.Format("sceKernelLoadExecVSHMs2 argp: {0}", Utilities.getMemoryDump(loadExecVSHParam.argp, loadExecVSHParam.args)));
				}
				if (loadExecVSHParam.vshmainArgsSize > 0)
				{
					Console.WriteLine(string.Format("sceKernelLoadExecVSHMs2 vshmainArgs: {0}", Utilities.getMemoryDump(loadExecVSHParam.vshmainArgs, loadExecVSHParam.vshmainArgsSize)));
				}
			}

			if (loadExecVSHParam.args > 0 && loadExecVSHParam.argp != 0)
			{
				string arg = Utilities.readStringNZ(loadExecVSHParam.argp, loadExecVSHParam.args);
				if (arg.StartsWith("disc0:", StringComparison.Ordinal))
				{
					Modules.IoFileMgrForUserModule.setfilepath("disc0/");
				}
				else if (arg.StartsWith("ms0:", StringComparison.Ordinal))
				{
					int dirIndex = arg.LastIndexOf('/');
					if (dirIndex >= 0)
					{
						Modules.IoFileMgrForUserModule.setfilepath(Settings.Instance.getDirectoryMapping("ms0") + arg.Substring(4, dirIndex - 4));
					}
				}
			}

			return Modules.LoadExecForUserModule.hleKernelLoadExec(filename, loadExecVSHParam.args, loadExecVSHParam.argp);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x08F7166C, version = 660, checkInsideInterrupt = true) public int sceKernelExitVSHVSH_660(pspsharp.HLE.TPointer param)
		[HLEFunction(nid : 0x08F7166C, version : 660, checkInsideInterrupt : true)]
		public virtual int sceKernelExitVSHVSH_660(TPointer param)
		{
			SceKernelLoadExecVSHParam loadExecVSHParam = new SceKernelLoadExecVSHParam();
			loadExecVSHParam.read(param);

			if (param.NotNull)
			{
				log.info(string.Format("sceKernelExitVSHVSH_660 param={0}", loadExecVSHParam));
			}

			return 0;
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0xD940C83C, version : 660)]
		public virtual int sceKernelLoadExecVSHMs2_660(PspString filename, TPointer param)
		{
			return sceKernelLoadExecVSHMs2(filename, param);
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0xF9CFCF2F, version : 660)]
		public virtual int sceKernelLoadExec_F9CFCF2F(PspString filename, TPointer param)
		{
			return sceKernelLoadExecVSHMs2(filename, param);
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0xD8320A28, version : 660)]
		public virtual int sceKernelLoadExecVSHDisc_660(PspString filename, TPointer param)
		{
			return sceKernelLoadExecVSHMs2(filename, param);
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0xBEF585EC, version : 150)]
		public virtual int sceKernelLoadExecBufferVSHUsbWlan(int bufferSize, TPointer bufferAddr, TPointer param)
		{
			SceKernelLoadExecVSHParam loadExecParam = new SceKernelLoadExecVSHParam();
			loadExecParam.read(param);

			int argSize = 0;
			int argAddr = 0;
			if (param.NotNull)
			{
				argSize = loadExecParam.args;
				argAddr = loadExecParam.argp;
				log.info(string.Format("sceKernelLoadExecBufferVSHUsbWlan param={0}", loadExecParam));
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelLoadExecBufferVSHUsbWlan buffAddr: {0}", Utilities.getMemoryDump(bufferAddr.Address, System.Math.Min(bufferSize, 1024))));
			}

			sbyte[] moduleBytes = bufferAddr.getArray8(bufferSize);
			ByteBuffer moduleBuffer = ByteBuffer.wrap(moduleBytes);

			return Modules.LoadExecForUserModule.hleKernelLoadExec(moduleBuffer, argSize, argAddr, null, null);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x11412288, version = 150) public int sceKernelLoadExec_11412288(pspsharp.HLE.TPointer callback)
		[HLEFunction(nid : 0x11412288, version : 150)]
		public virtual int sceKernelLoadExec_11412288(TPointer callback)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA5ECA6E3, version = 660) public int sceKernelLoadExec_11412288_660(pspsharp.HLE.TPointer callback)
		[HLEFunction(nid : 0xA5ECA6E3, version : 660)]
		public virtual int sceKernelLoadExec_11412288_660(TPointer callback)
		{
			return sceKernelLoadExec_11412288(callback);
		}
	}

}