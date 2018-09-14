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

	public class sceMeVideo : HLEModule
	{
		public static Logger log = Modules.getLogger("sceMeVideo");

		// Called by sceVideocodecOpen
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC441994C, version = 150) public int sceMeVideo_driver_C441994C(int type, pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0xC441994C, version : 150)]
		public virtual int sceMeVideo_driver_C441994C(int type, TPointer buffer)
		{
			return 0;
		}

		// Called by sceVideocodecInit
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE8CD3C75, version = 150) public int sceMeVideo_driver_E8CD3C75(int type, pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0xE8CD3C75, version : 150)]
		public virtual int sceMeVideo_driver_E8CD3C75(int type, TPointer buffer)
		{
			return 0;
		}

		// Called by sceVideocodecGetVersion (=> unknown == 3)
		// Called by sceVideocodecSetMemory (=> unknown == 1)
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6D68B223, version = 150) public int sceMeVideo_driver_6D68B223(int type, int unknown, pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0x6D68B223, version : 150)]
		public virtual int sceMeVideo_driver_6D68B223(int type, int unknown, TPointer buffer)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x21521BE5, version = 150) public int sceMeVideo_driver_21521BE5()
		[HLEFunction(nid : 0x21521BE5, version : 150)]
		public virtual int sceMeVideo_driver_21521BE5()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4D78330C, version = 150) public int sceMeVideo_driver_4D78330C()
		[HLEFunction(nid : 0x4D78330C, version : 150)]
		public virtual int sceMeVideo_driver_4D78330C()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8768915D, version = 150) public int sceMeVideo_driver_8768915D(int type, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=96, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0x8768915D, version : 150)]
		public virtual int sceMeVideo_driver_8768915D(int type, TPointer buffer)
		{
			return 0;
		}

		// Called by sceVideocodecDelete()
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8DD56014, version = 150) public int sceMeVideo_driver_8DD56014(int type, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=96, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0x8DD56014, version : 150)]
		public virtual int sceMeVideo_driver_8DD56014(int type, TPointer buffer)
		{
			return 0;
		}
	}

}