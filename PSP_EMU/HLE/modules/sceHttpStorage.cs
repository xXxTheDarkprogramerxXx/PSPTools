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
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;

	using Logger = org.apache.log4j.Logger;

	public class sceHttpStorage : HLEModule
	{
		public static Logger log = Modules.getLogger("sceHttpStorage");
		private const int TYPE_AUTH_DAT = 0;
		private const int TYPE_COOKIE_DAT = 1;
		private readonly int[] fileIds = new int[2];
		private static readonly string[] fileNames = new string[] {"flash1:/net/http/auth.dat", "flash1:/net/http/cookie.dat"};

		public virtual int checkType(int type)
		{
			if (type != TYPE_AUTH_DAT && type != TYPE_COOKIE_DAT)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_ID);
			}

			return type;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2D8DAE58, version = 150) public int sceHttpStorageGetstat(@CheckArgument("checkType") int type, pspsharp.HLE.TPointer statAddr)
		[HLEFunction(nid : 0x2D8DAE58, version : 150)]
		public virtual int sceHttpStorageGetstat(int type, TPointer statAddr)
		{
			return Modules.IoFileMgrForUserModule.hleIoGetstat(0, fileNames[type], statAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x700AAD44, version = 150) public int sceHttpStorageOpen(@CheckArgument("checkType") int type, int flags, int permissions)
		[HLEFunction(nid : 0x700AAD44, version : 150)]
		public virtual int sceHttpStorageOpen(int type, int flags, int permissions)
		{
			int fileId = Modules.IoFileMgrForUserModule.hleIoOpen(0, fileNames[type], flags, permissions, false);
			if (fileId < 0)
			{
				return fileId;
			}

			fileIds[type] = fileId;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCDA3D8F6, version = 150) public int sceHttpStorageClose(@CheckArgument("checkType") int type)
		[HLEFunction(nid : 0xCDA3D8F6, version : 150)]
		public virtual int sceHttpStorageClose(int type)
		{
			int result = Modules.IoFileMgrForUserModule.sceIoClose(fileIds[type]);
			fileIds[type] = -1;

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB33389CE, version = 150) public int sceHttpStorageLseek(@CheckArgument("checkType") int type, long offset, int whence)
		[HLEFunction(nid : 0xB33389CE, version : 150)]
		public virtual int sceHttpStorageLseek(int type, long offset, int whence)
		{
			return (int) Modules.IoFileMgrForUserModule.sceIoLseek(fileIds[type], offset, whence);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCDDF1103, version = 150) public int sceHttpStorageRead(@CheckArgument("checkType") int type, pspsharp.HLE.TPointer buffer, int bufferSize)
		[HLEFunction(nid : 0xCDDF1103, version : 150)]
		public virtual int sceHttpStorageRead(int type, TPointer buffer, int bufferSize)
		{
			return Modules.IoFileMgrForUserModule.sceIoRead(fileIds[type], buffer, bufferSize);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x24AA94F4, version = 150) public int sceHttpStorageWrite(@CheckArgument("checkType") int type, pspsharp.HLE.TPointer buffer, int bufferSize)
		[HLEFunction(nid : 0x24AA94F4, version : 150)]
		public virtual int sceHttpStorageWrite(int type, TPointer buffer, int bufferSize)
		{
			return Modules.IoFileMgrForUserModule.sceIoWrite(fileIds[type], buffer, bufferSize);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x04EF00F8, version = 150) public int sceHttpStorage_04EF00F8(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=8, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer8 psCode)
		[HLEFunction(nid : 0x04EF00F8, version : 150)]
		public virtual int sceHttpStorage_04EF00F8(TPointer8 psCode)
		{
			return Modules.sceChkregModule.sceChkregGetPsCode(psCode);
		}
	}

}