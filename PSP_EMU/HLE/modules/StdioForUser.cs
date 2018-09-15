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
	using Managers = pspsharp.HLE.kernel.Managers;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelMppInfo = pspsharp.HLE.kernel.types.SceKernelMppInfo;

	//using Logger = org.apache.log4j.Logger;

	public class StdioForUser : HLEModule
	{
		//public static Logger log = Modules.getLogger("StdioForUser");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3054D478, version = 150) public int sceKernelStdioRead()
		[HLEFunction(nid : 0x3054D478, version : 150)]
		public virtual int sceKernelStdioRead()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0CBB0571, version = 150) public int sceKernelStdioLseek()
		[HLEFunction(nid : 0x0CBB0571, version : 150)]
		public virtual int sceKernelStdioLseek()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA46785C9, version = 150) public int sceKernelStdioSendChar()
		[HLEFunction(nid : 0xA46785C9, version : 150)]
		public virtual int sceKernelStdioSendChar()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA3B931DB, version = 150) public int sceKernelStdioWrite()
		[HLEFunction(nid : 0xA3B931DB, version : 150)]
		public virtual int sceKernelStdioWrite()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9D061C19, version = 150) public int sceKernelStdioClose()
		[HLEFunction(nid : 0x9D061C19, version : 150)]
		public virtual int sceKernelStdioClose()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x924ABA61, version = 150) public int sceKernelStdioOpen()
		[HLEFunction(nid : 0x924ABA61, version : 150)]
		public virtual int sceKernelStdioOpen()
		{
			return 0;
		}

		[HLEFunction(nid : 0x172D316E, version : 150)]
		public virtual int sceKernelStdin()
		{
			return IoFileMgrForUser.STDIN_ID;
		}

		[HLEFunction(nid : 0xA6BAB2E9, version : 150)]
		public virtual int sceKernelStdout()
		{
			return IoFileMgrForUser.STDOUT_ID;
		}

		[HLEFunction(nid : 0xF78BA90A, version : 150)]
		public virtual int sceKernelStderr()
		{
			return IoFileMgrForUser.STDERR_ID;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x432D8F5C, version = 300) public int sceKernelRegisterStdoutPipe(int msgPipeUid)
		[HLEFunction(nid : 0x432D8F5C, version : 300)]
		public virtual int sceKernelRegisterStdoutPipe(int msgPipeUid)
		{
			SceKernelMppInfo msgPipeInfo = Managers.msgPipes.getMsgPipeInfo(msgPipeUid);
			if (msgPipeInfo == null)
			{
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_ARGUMENT;
			}

			log.info(string.Format("sceKernelRegisterStdoutPipe {0}", msgPipeInfo));

			Modules.IoFileMgrForUserModule.hleRegisterStdPipe(IoFileMgrForUser.STDOUT_ID, msgPipeInfo);

			return 0;
		}
	}
}