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
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;

	//using Logger = org.apache.log4j.Logger;

	public class sceLibUpdateDL : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceLibUpdateDL");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFC1AB540, version = 150) public int sceUpdateDownloadInit()
		[HLEFunction(nid : 0xFC1AB540, version : 150)]
		public virtual int sceUpdateDownloadInit()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF6690A9A, version = 150) public int sceUpdateDownloadInitEx(pspsharp.HLE.TPointer unknown1, pspsharp.HLE.TPointer unknown2)
		[HLEFunction(nid : 0xF6690A9A, version : 150)]
		public virtual int sceUpdateDownloadInitEx(TPointer unknown1, TPointer unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD6A09757, version = 150) public int sceUpdateDownloadEnd()
		[HLEFunction(nid : 0xD6A09757, version : 150)]
		public virtual int sceUpdateDownloadEnd()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4F49C9C1, version = 150) public int sceUpdateDownloadAbort()
		[HLEFunction(nid : 0x4F49C9C1, version : 150)]
		public virtual int sceUpdateDownloadAbort()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFD675E8D, version = 150) public int sceUpdateDownloadConnectServer(@CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown1, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown2)
		[HLEFunction(nid : 0xFD675E8D, version : 150)]
		public virtual int sceUpdateDownloadConnectServer(TPointer32 unknown1, TPointer32 unknown2)
		{
			unknown1.setValue(0);
			unknown2.setValue(0);

			return SceKernelErrors.ERROR_LIB_UPDATE_LATEST_VERSION_INSTALLED;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA3000F72, version = 150) public int sceUpdateDownloadCreateCtx()
		[HLEFunction(nid : 0xA3000F72, version : 150)]
		public virtual int sceUpdateDownloadCreateCtx()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x782EF929, version = 150) public int sceUpdateDownloadDeleteCtx()
		[HLEFunction(nid : 0x782EF929, version : 150)]
		public virtual int sceUpdateDownloadDeleteCtx()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFA9AA797, version = 150) public int sceUpdateDownloadReadData()
		[HLEFunction(nid : 0xFA9AA797, version : 150)]
		public virtual int sceUpdateDownloadReadData()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC3E1C200, version = 150) public int sceUpdateDownloadSetBuildNum()
		[HLEFunction(nid : 0xC3E1C200, version : 150)]
		public virtual int sceUpdateDownloadSetBuildNum()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB2EC0E06, version = 150) public int sceUpdateDownloadSetProductCode()
		[HLEFunction(nid : 0xB2EC0E06, version : 150)]
		public virtual int sceUpdateDownloadSetProductCode()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC6BFE5B8, version = 150) public int sceUpdateDownloadSetRange()
		[HLEFunction(nid : 0xC6BFE5B8, version : 150)]
		public virtual int sceUpdateDownloadSetRange()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x59106229, version = 150) public int sceUpdateDownloadSetUrl()
		[HLEFunction(nid : 0x59106229, version : 150)]
		public virtual int sceUpdateDownloadSetUrl()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC1AF1076, version = 150) public int sceUpdateDownloadSetVersion(int buildNumber)
		[HLEFunction(nid : 0xC1AF1076, version : 150)]
		public virtual int sceUpdateDownloadSetVersion(int buildNumber)
		{
			// E.g. buildNumber is 5455 (can also be found in version.txt)
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x34243B86, version = 150) public int sceLibUpdateDL_34243B86(pspsharp.HLE.PspString release)
		[HLEFunction(nid : 0x34243B86, version : 150)]
		public virtual int sceLibUpdateDL_34243B86(PspString release)
		{
			// E.g. release is "6.60"
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x88FF3935, version = 150) public int sceUpdateDownloadSetDestCode(int destCode)
		[HLEFunction(nid : 0x88FF3935, version : 150)]
		public virtual int sceUpdateDownloadSetDestCode(int destCode)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB5DB018D, version = 150) public int sceUpdateDownloadSetServerRegion(int serverRegion1, int serverRegion2)
		[HLEFunction(nid : 0xB5DB018D, version : 150)]
		public virtual int sceUpdateDownloadSetServerRegion(int serverRegion1, int serverRegion2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x36D8F34B, version = 150) public int sceUpdate_36D8F34B(int unknown)
		[HLEFunction(nid : 0x36D8F34B, version : 150)]
		public virtual int sceUpdate_36D8F34B(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF7E66CB4, version = 150) public int sceUpdate_F7E66CB4()
		[HLEFunction(nid : 0xF7E66CB4, version : 150)]
		public virtual int sceUpdate_F7E66CB4()
		{
			return 0;
		}
	}

}