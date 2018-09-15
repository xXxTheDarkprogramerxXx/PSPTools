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

	public class sceNetUpnp : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceNetUpnp");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE24220B5, version = 150) public int sceNetUpnpInit(int unknown1, int unknown2)
		[HLEFunction(nid : 0xE24220B5, version : 150)]
		public virtual int sceNetUpnpInit(int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x540491EF, version = 150) public int sceNetUpnpTerm()
		[HLEFunction(nid : 0x540491EF, version : 150)]
		public virtual int sceNetUpnpTerm()
		{
			// No parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3432B2E5, version = 150) public int sceNetUpnpStart()
		[HLEFunction(nid : 0x3432B2E5, version : 150)]
		public virtual int sceNetUpnpStart()
		{
			// No parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3E32ED9E, version = 150) public int sceNetUpnpStop()
		[HLEFunction(nid : 0x3E32ED9E, version : 150)]
		public virtual int sceNetUpnpStop()
		{
			// No parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x27045362, version = 150) public int sceNetUpnpGetNatInfo(pspsharp.HLE.TPointer unknown)
		[HLEFunction(nid : 0x27045362, version : 150)]
		public virtual int sceNetUpnpGetNatInfo(TPointer unknown)
		{
			// Unknown structure of 16 bytes
			unknown.clear(16);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8513C6D1, version = 150) public int sceNetUpnp_8513C6D1(pspsharp.HLE.TPointer unknown1, pspsharp.HLE.TPointer unknown2, pspsharp.HLE.TPointer unknown3)
		[HLEFunction(nid : 0x8513C6D1, version : 150)]
		public virtual int sceNetUpnp_8513C6D1(TPointer unknown1, TPointer unknown2, TPointer unknown3)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFDA78483, version = 150) public int sceNetUpnp_FDA78483()
		[HLEFunction(nid : 0xFDA78483, version : 150)]
		public virtual int sceNetUpnp_FDA78483()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1038E77A, version = 150) public int sceNetUpnp_1038E77A(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=48, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown)
		[HLEFunction(nid : 0x1038E77A, version : 150)]
		public virtual int sceNetUpnp_1038E77A(TPointer unknown)
		{
			unknown.clear(48);
			unknown.setValue32(4, 1);

			return 0;
		}
	}

}