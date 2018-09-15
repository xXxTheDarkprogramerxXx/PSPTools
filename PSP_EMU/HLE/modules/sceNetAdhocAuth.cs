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

	public class sceNetAdhocAuth : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceNetAdhocAuth");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x86004235, version = 150) public int sceNetAdhocAuthInit()
		[HLEFunction(nid : 0x86004235, version : 150)]
		public virtual int sceNetAdhocAuthInit()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6074D8F1, version = 150) public int sceNetAdhocAuthTerm()
		[HLEFunction(nid : 0x6074D8F1, version : 150)]
		public virtual int sceNetAdhocAuthTerm()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x015A8A64, version = 150) public int sceNetAdhocAuth_015A8A64(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer buffer, int bufferLength)
		[HLEFunction(nid : 0x015A8A64, version : 150)]
		public virtual int sceNetAdhocAuth_015A8A64(TPointer buffer, int bufferLength)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1F9A90B8, version = 150) public int sceNetAdhocAuth_1F9A90B8()
		[HLEFunction(nid : 0x1F9A90B8, version : 150)]
		public virtual int sceNetAdhocAuth_1F9A90B8()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2AD8C677, version = 150) public int sceNetAdhocAuth_2AD8C677()
		[HLEFunction(nid : 0x2AD8C677, version : 150)]
		public virtual int sceNetAdhocAuth_2AD8C677()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2E6AA271, version = 150) public int sceNetAdhocAuth_2E6AA271()
		[HLEFunction(nid : 0x2E6AA271, version : 150)]
		public virtual int sceNetAdhocAuth_2E6AA271()
		{
			// Has no parameters
			// Termination of what has been initialized with sceNetAdhocAuth_89F2A732()
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x312BD812, version = 150) public int sceNetAdhocAuth_312BD812()
		[HLEFunction(nid : 0x312BD812, version : 150)]
		public virtual int sceNetAdhocAuth_312BD812()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6CE209A3, version = 150) public int sceNetAdhocAuth_6CE209A3()
		[HLEFunction(nid : 0x6CE209A3, version : 150)]
		public virtual int sceNetAdhocAuth_6CE209A3()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x72AAC6D3, version = 150) public int sceNetAdhocAuth_72AAC6D3()
		[HLEFunction(nid : 0x72AAC6D3, version : 150)]
		public virtual int sceNetAdhocAuth_72AAC6D3()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x76F26AB0, version = 150) public int sceNetAdhocAuth_76F26AB0()
		[HLEFunction(nid : 0x76F26AB0, version : 150)]
		public virtual int sceNetAdhocAuth_76F26AB0()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x89F2A732, version = 150) public int sceNetAdhocAuth_89F2A732(pspsharp.HLE.TPointer unknown1, int threadPriority, int threadStackSize, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=20, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 unknown2, int unknown3, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=128, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknown4)
		[HLEFunction(nid : 0x89F2A732, version : 150)]
		public virtual int sceNetAdhocAuth_89F2A732(TPointer unknown1, int threadPriority, int threadStackSize, TPointer32 unknown2, int unknown3, TPointer unknown4)
		{
			// Some initialization routine
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAAB06250, version = 150) public int sceNetAdhocAuth_AAB06250()
		[HLEFunction(nid : 0xAAB06250, version : 150)]
		public virtual int sceNetAdhocAuth_AAB06250()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBD144DA6, version = 150) public int sceNetAdhocAuth_BD144DA6()
		[HLEFunction(nid : 0xBD144DA6, version : 150)]
		public virtual int sceNetAdhocAuth_BD144DA6()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCF4D9BED, version = 150) public int sceNetAdhocAuth_CF4D9BED()
		[HLEFunction(nid : 0xCF4D9BED, version : 150)]
		public virtual int sceNetAdhocAuth_CF4D9BED()
		{
			return 0;
		}
	}

}