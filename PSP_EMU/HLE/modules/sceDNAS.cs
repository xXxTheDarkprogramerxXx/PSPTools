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
	using Managers = pspsharp.HLE.kernel.Managers;

	//using Logger = org.apache.log4j.Logger;

	public class sceDNAS : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceDNAS");
		private int eventFlagUid;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0D560144, version = 150) public int sceDNASInit(int unknown1, int unknown2)
		[HLEFunction(nid : 0x0D560144, version : 150)]
		public virtual int sceDNASInit(int unknown1, int unknown2)
		{
			eventFlagUid = Managers.eventFlags.sceKernelCreateEventFlag("SceDNASExternal", 0, 0, TPointer.NULL);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBE0998D7, version = 150) public int sceDNASTerm()
		[HLEFunction(nid : 0xBE0998D7, version : 150)]
		public virtual int sceDNASTerm()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x45C1AAF5, version = 150) public int sceDNASGetEventFlag(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown)
		[HLEFunction(nid : 0x45C1AAF5, version : 150)]
		public virtual int sceDNASGetEventFlag(TPointer32 unknown)
		{
			unknown.setValue(eventFlagUid);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF3787AD8, version = 150) public int sceDNASInternalStart(int unknown)
		[HLEFunction(nid : 0xF3787AD8, version : 150)]
		public virtual int sceDNASInternalStart(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCA8B8909, version = 150) public int sceDNASNetStart()
		[HLEFunction(nid : 0xCA8B8909, version : 150)]
		public virtual int sceDNASNetStart()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9FF48DD3, version = 150) public int sceDNASStop()
		[HLEFunction(nid : 0x9FF48DD3, version : 150)]
		public virtual int sceDNASStop()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6929100C, version = 150) public int sceDNASGetProductCode()
		[HLEFunction(nid : 0x6929100C, version : 150)]
		public virtual int sceDNASGetProductCode()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA646E771, version = 150) public int sceDNASGetState(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 stateAddr, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 errorCodeAddr)
		[HLEFunction(nid : 0xA646E771, version : 150)]
		public virtual int sceDNASGetState(TPointer32 stateAddr, TPointer32 errorCodeAddr)
		{
			stateAddr.setValue(0);
			errorCodeAddr.setValue(0);
			Managers.eventFlags.sceKernelSetEventFlag(eventFlagUid, 1);

			return 0;
		}
	}

}