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
	using Utilities = pspsharp.util.Utilities;

	public class mp4msv : HLEModule
	{
		//public static Logger log = Modules.getLogger("mp4msv");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3C2183C7, version = 150) public int mp4msv_3C2183C7(int unknown, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=20, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer addr)
		[HLEFunction(nid : 0x3C2183C7, version : 150)]
		public virtual int mp4msv_3C2183C7(int unknown, TPointer addr)
		{
			if (addr.NotNull)
			{
				// addr is pointing to five 32-bit values (20 bytes)
				Console.WriteLine(string.Format("mp4msv_3C2183C7 unknown values: {0}", Utilities.getMemoryDump(addr.Address, 20, 4, 20)));
			}

			// mp4msv_3C2183C7 is called by sceMp4Init
			Modules.sceMp4Module.hleMp4Init();

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9CA13D1A, version = 150) public int mp4msv_9CA13D1A(int unknown, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=68, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 addr)
		[HLEFunction(nid : 0x9CA13D1A, version : 150)]
		public virtual int mp4msv_9CA13D1A(int unknown, TPointer32 addr)
		{
			if (addr.NotNull)
			{
				// addr is pointing to 17 32-bit values (68 bytes)
				Console.WriteLine(string.Format("mp4msv_9CA13D1A unknown values: {0}", Utilities.getMemoryDump(addr.Address, 68, 4, 16)));
			}

			// mp4msv_9CA13D1A is called by sceMp4Init
			Modules.sceMp4Module.hleMp4Init();

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF595F917, version = 150) public int mp4msv_F595F917(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown)
		[HLEFunction(nid : 0xF595F917, version : 150)]
		public virtual int mp4msv_F595F917(TPointer32 unknown)
		{
			unknown.setValue(0);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3D8D41A0, version = 150) public int mp4msv_3D8D41A0(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer unknown1, int unknown2)
		[HLEFunction(nid : 0x3D8D41A0, version : 150)]
		public virtual int mp4msv_3D8D41A0(TPointer unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x67AF9E0F, version = 150) public int mp4msv_67AF9E0F(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer unknown1, int unknown2, int unknown3)
		[HLEFunction(nid : 0x67AF9E0F, version : 150)]
		public virtual int mp4msv_67AF9E0F(TPointer unknown1, int unknown2, int unknown3)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x07C60A23, version = 150) public int mp4msv_07C60A23(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer unknown1, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown2, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown3)
		[HLEFunction(nid : 0x07C60A23, version : 150)]
		public virtual int mp4msv_07C60A23(TPointer unknown1, TPointer32 unknown2, TPointer32 unknown3)
		{
			unknown1.setValue32(0);
			unknown2.setValue(0);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0D32271B, version = 150) public int mp4msv_0D32271B(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer unknown1, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown2)
		[HLEFunction(nid : 0x0D32271B, version : 150)]
		public virtual int mp4msv_0D32271B(TPointer unknown1, TPointer32 unknown2)
		{
			unknown2.setValue(0);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAD3AF34E, version = 150) public int mp4msv_AD3AF34E(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer32 unknown1, int unknown2, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=40, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown3)
		[HLEFunction(nid : 0xAD3AF34E, version : 150)]
		public virtual int mp4msv_AD3AF34E(TPointer32 unknown1, int unknown2, TPointer unknown3)
		{
			if (unknown1.Null)
			{
				return 4;
			}
			if (unknown1.getValue(0) == 0)
			{
				return 4;
			}
			TPointer unknown5 = unknown1.getPointer(0);
			if (unknown5.getValue32(184) == 0)
			{
				return 0x2003;
			}
			if (unknown2 == 0)
			{
				return 6;
			}
			if (unknown5.getValue32(220) == 0 || unknown5.getValue32(220) < unknown2)
			{
				return 0x2002;
			}
			int unknown4 = unknown5.getValue32(232) + 52 * (unknown2 - 1);
			unknown3.memcpy(unknown4, 40);

			return 0;
		}
	}

}