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

	public class sceChkreg : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceChkreg");
		public const int PS_CODE_JAPAN = 3;
		public const int PS_CODE_NORTH_AMERICA = 4;
		public const int PS_CODE_EUROPE = 5;
		public const int PS_CODE_KOREA = 6;
		public const int PS_CODE_AUSTRALIA = 9;
		public const int PS_CODE_HONGKONG = 10;
		public const int PS_CODE_TAIWAN = 11;
		public const int PS_CODE_RUSSIA = 12;
		public const int PS_CODE_CHINA = 13;

		public virtual int ValueReturnedBy6894A027
		{
			get
			{
				return 1; // Fake value
			}
		}

		[HLEFunction(nid : 0x54495B19, version : 150)]
		public virtual int sceChkregCheckRegion(int unknown1, int unknown2)
		{
			// 0: region is not correct
			// 1: region is correct
			return 1;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x59F8491D, version = 150) public int sceChkregGetPsCode(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=8, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer8 psCode)
		[HLEFunction(nid : 0x59F8491D, version : 150)]
		public virtual int sceChkregGetPsCode(TPointer8 psCode)
		{
			psCode.setValue(0, 1);
			psCode.setValue(1, 0);
			psCode.setValue(2, PS_CODE_EUROPE);
			psCode.setValue(3, 0);
			psCode.setValue(4, 1);
			psCode.setValue(5, 0);
			psCode.setValue(6, 1);
			psCode.setValue(7, 0);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6894A027, version = 150) public int sceChkreg_driver_6894A027(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer8 unknown1, int unknown2)
		[HLEFunction(nid : 0x6894A027, version : 150)]
		public virtual int sceChkreg_driver_6894A027(TPointer8 unknown1, int unknown2)
		{
			unknown1.Value = ValueReturnedBy6894A027;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7939C851, version = 150) public int sceChkreg_driver_7939C851()
		[HLEFunction(nid : 0x7939C851, version : 150)]
		public virtual int sceChkreg_driver_7939C851()
		{
			// Has no parameters
			return 1;
		}
	}

}