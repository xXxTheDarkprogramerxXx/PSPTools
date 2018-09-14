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

	using Logger = org.apache.log4j.Logger;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using LocalVirtualFile = pspsharp.HLE.VFS.local.LocalVirtualFile;
	using SeekableRandomFile = pspsharp.filesystems.SeekableRandomFile;
	using Model = pspsharp.hardware.Model;

	public class scePopsMan : HLEModule
	{
		public static Logger log = Modules.getLogger("scePopsMan");
		private string ebootPbp;
		private int ebootPbpUid;
		private IVirtualFile vFileEbootPbp;

		[HLEFunction(nid : 0x29B3FB24, version : 150)]
		public virtual int scePopsManLoadModule(PspString ebootPbp, int unknown)
		{
			this.ebootPbp = ebootPbp.String;
			ebootPbpUid = -1;
			vFileEbootPbp = null;

			string popsFileName = string.Format("flash0:/kd/pops_{0:D2}g.prx", Model.Model + 1);

	//    	Modules.ModuleMgrForUserModule.hleKernelLoadAndStartModule("flash0:/vsh/module/paf.prx", 0x19);
			return Modules.ModuleMgrForUserModule.hleKernelLoadAndStartModule(popsFileName, 0x20);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0090B2C8, version = 150) public int scePopsManExitVSHKernel()
		[HLEFunction(nid : 0x0090B2C8, version : 150)]
		public virtual int scePopsManExitVSHKernel()
		{
			return Modules.LoadExecForKernelModule.sceKernelExitVSHKernel(TPointer.NULL);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8D5A07D2, version = 150) public int sceMeAudio_8D5A07D2()
		[HLEFunction(nid : 0x8D5A07D2, version : 150)]
		public virtual int sceMeAudio_8D5A07D2()
		{
			ebootPbpUid = 0x1234;
			try
			{
				vFileEbootPbp = new LocalVirtualFile(new SeekableRandomFile(new File(ebootPbp), "r"));
			}
			catch (FileNotFoundException e)
			{
				log.error("sceMeAudio_8D5A07D2", e);
			}

			return ebootPbpUid;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x30BE34E4, version = 150) public int sceMeAudio_30BE34E4(int uid, pspsharp.HLE.TPointer dataAddr, int offset, int size)
		[HLEFunction(nid : 0x30BE34E4, version : 150)]
		public virtual int sceMeAudio_30BE34E4(int uid, TPointer dataAddr, int offset, int size)
		{
			if (uid != ebootPbpUid)
			{
				return -1;
			}
			long seekOffset = offset & 0xFFFFFFFFL;
			long result = vFileEbootPbp.ioLseek(seekOffset);
			if (result != seekOffset)
			{
				return -1;
			}

			return vFileEbootPbp.ioRead(dataAddr, size);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0BABD960, version = 150) public int sceMeAudio_0BABD960()
		[HLEFunction(nid : 0x0BABD960, version : 150)]
		public virtual int sceMeAudio_0BABD960()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0FA28FE6, version = 150) public int sceMeAudio_0FA28FE6()
		[HLEFunction(nid : 0x0FA28FE6, version : 150)]
		public virtual int sceMeAudio_0FA28FE6()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x14447BA0, version = 150) public int sceMeAudio_14447BA0()
		[HLEFunction(nid : 0x14447BA0, version : 150)]
		public virtual int sceMeAudio_14447BA0()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1A23C094, version = 150) public int sceMeAudio_1A23C094()
		[HLEFunction(nid : 0x1A23C094, version : 150)]
		public virtual int sceMeAudio_1A23C094()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2AB4FE43, version = 150) public int sceMeAudio_2AB4FE43()
		[HLEFunction(nid : 0x2AB4FE43, version : 150)]
		public virtual int sceMeAudio_2AB4FE43()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2AC64C3F, version = 150) public int sceMeAudio_2AC64C3F()
		[HLEFunction(nid : 0x2AC64C3F, version : 150)]
		public virtual int sceMeAudio_2AC64C3F()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3771229C, version = 150) public int sceMeAudio_3771229C()
		[HLEFunction(nid : 0x3771229C, version : 150)]
		public virtual int sceMeAudio_3771229C()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x42F0EA37, version = 150) public int sceMeAudio_42F0EA37()
		[HLEFunction(nid : 0x42F0EA37, version : 150)]
		public virtual int sceMeAudio_42F0EA37()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4F5B6D82, version = 150) public int sceMeAudio_4F5B6D82()
		[HLEFunction(nid : 0x4F5B6D82, version : 150)]
		public virtual int sceMeAudio_4F5B6D82()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x528266FA, version = 150) public int sceMeAudio_528266FA(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=1024, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0x528266FA, version : 150)]
		public virtual int sceMeAudio_528266FA(TPointer buffer)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x54F2AE52, version = 150) public int sceMeAudio_54F2AE52()
		[HLEFunction(nid : 0x54F2AE52, version : 150)]
		public virtual int sceMeAudio_54F2AE52()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x68C55F4C, version = 150) public int sceMeAudio_68C55F4C()
		[HLEFunction(nid : 0x68C55F4C, version : 150)]
		public virtual int sceMeAudio_68C55F4C()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x69C4BCCB, version = 150) public int sceMeAudio_69C4BCCB()
		[HLEFunction(nid : 0x69C4BCCB, version : 150)]
		public virtual int sceMeAudio_69C4BCCB()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7014C540, version = 150) public int sceMeAudio_7014C540()
		[HLEFunction(nid : 0x7014C540, version : 150)]
		public virtual int sceMeAudio_7014C540()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x805D1205, version = 150) public int sceMeAudio_805D1205()
		[HLEFunction(nid : 0x805D1205, version : 150)]
		public virtual int sceMeAudio_805D1205()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x83378E12, version = 150) public int sceMeAudio_83378E12()
		[HLEFunction(nid : 0x83378E12, version : 150)]
		public virtual int sceMeAudio_83378E12()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8A8DFE17, version = 150) public int sceMeAudio_8A8DFE17()
		[HLEFunction(nid : 0x8A8DFE17, version : 150)]
		public virtual int sceMeAudio_8A8DFE17()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9B4AAF7D, version = 150) public int sceMeAudio_9B4AAF7D()
		[HLEFunction(nid : 0x9B4AAF7D, version : 150)]
		public virtual int sceMeAudio_9B4AAF7D()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA6EDDF16, version = 150) public int sceMeAudio_A6EDDF16()
		[HLEFunction(nid : 0xA6EDDF16, version : 150)]
		public virtual int sceMeAudio_A6EDDF16()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAE5AC375, version = 150) public int sceMeAudio_AE5AC375()
		[HLEFunction(nid : 0xAE5AC375, version : 150)]
		public virtual int sceMeAudio_AE5AC375()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBD5F7689, version = 150) public int sceMeAudio_BD5F7689()
		[HLEFunction(nid : 0xBD5F7689, version : 150)]
		public virtual int sceMeAudio_BD5F7689()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC93C56F8, version = 150) public int sceMeAudio_C93C56F8()
		[HLEFunction(nid : 0xC93C56F8, version : 150)]
		public virtual int sceMeAudio_C93C56F8()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD4F17F54, version = 150) public int sceMeAudio_D4F17F54()
		[HLEFunction(nid : 0xD4F17F54, version : 150)]
		public virtual int sceMeAudio_D4F17F54()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDE630CD2, version = 150) public int sceMeAudio_DE630CD2()
		[HLEFunction(nid : 0xDE630CD2, version : 150)]
		public virtual int sceMeAudio_DE630CD2()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE7F06E2B, version = 150) public int sceMeAudio_E7F06E2B()
		[HLEFunction(nid : 0xE7F06E2B, version : 150)]
		public virtual int sceMeAudio_E7F06E2B()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE907AE69, version = 150) public int sceMeAudio_E907AE69()
		[HLEFunction(nid : 0xE907AE69, version : 150)]
		public virtual int sceMeAudio_E907AE69()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF6637A72, version = 150) public int sceMeAudio_F6637A72()
		[HLEFunction(nid : 0xF6637A72, version : 150)]
		public virtual int sceMeAudio_F6637A72()
		{
			return 0;
		}
	}

}