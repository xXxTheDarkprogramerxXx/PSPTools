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

	//using Logger = org.apache.log4j.Logger;

	public class sceGameUpdate : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceGameUpdate");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCBE69FB3, version = 150) public int sceGameUpdateInit()
		[HLEFunction(nid : 0xCBE69FB3, version : 150)]
		public virtual int sceGameUpdateInit()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBB4B68DE, version = 150) public int sceGameUpdateTerm()
		[HLEFunction(nid : 0xBB4B68DE, version : 150)]
		public virtual int sceGameUpdateTerm()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x596AD78C, version = 150) public int sceGameUpdateRun()
		[HLEFunction(nid : 0x596AD78C, version : 150)]
		public virtual int sceGameUpdateRun()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5F5D98A6, version = 150) public int sceGameUpdateAbort()
		[HLEFunction(nid : 0x5F5D98A6, version : 150)]
		public virtual int sceGameUpdateAbort()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x381CB9B3, version = 150) public int sceGameUpdate_381CB9B3(int unknown1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=36, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknown2)
		[HLEFunction(nid : 0x381CB9B3, version : 150)]
		public virtual int sceGameUpdate_381CB9B3(int unknown1, TPointer unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3EBB6055, version = 150) public int sceGameUpdate_3EBB6055(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=16, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=152, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown2)
		[HLEFunction(nid : 0x3EBB6055, version : 150)]
		public virtual int sceGameUpdate_3EBB6055(TPointer unknown1, TPointer unknown2)
		{
			return 0;
		}
	}
}