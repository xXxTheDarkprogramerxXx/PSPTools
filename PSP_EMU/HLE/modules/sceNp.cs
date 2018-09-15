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

	public class sceNp : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceNp");
		public const int PARENTAL_CONTROL_DISABLED = 0;
		public const int PARENTAL_CONTROL_ENABLED = 1;
		public int parentalControl = PARENTAL_CONTROL_ENABLED;
		protected internal bool initialized;

		public override void start()
		{
			initialized = false;
			base.start();
		}

		public virtual string OnlineId
		{
			get
			{
				string onlineId = "DummyOnlineId";
    
				return onlineId;
			}
		}

		public virtual string AvatarUrl
		{
			get
			{
				return "http://DummyAvatarUrl";
			}
		}

		public virtual int UserAge
		{
			get
			{
				return 13;
			}
		}

		/// <summary>
		/// Initialization.
		/// 
		/// @return
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x857B47D3, version = 150, checkInsideInterrupt = true) public int sceNpInit()
		[HLEFunction(nid : 0x857B47D3, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpInit()
		{
			// No parameters
			initialized = true;

			return 0;
		}

		/// <summary>
		/// Termination.
		/// 
		/// @return
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x37E1E274, version = 150, checkInsideInterrupt = true) public int sceNpTerm()
		[HLEFunction(nid : 0x37E1E274, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpTerm()
		{
			// No parameters
			initialized = false;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x633B5F71, version = 150) public int sceNpGetNpId(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=36, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0x633B5F71, version : 150)]
		public virtual int sceNpGetNpId(TPointer buffer)
		{
			// The first 20 bytes are the onlineId
			buffer.setStringNZ(0, 20, OnlineId);
			// The next 16 bytes are unknown
			buffer.clear(20, 16);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA0BE3C4B, version = 150) public int sceNpGetAccountRegion(pspsharp.HLE.TPointer unknown1, pspsharp.HLE.TPointer32 unknown2)
		[HLEFunction(nid : 0xA0BE3C4B, version : 150)]
		public virtual int sceNpGetAccountRegion(TPointer unknown1, TPointer32 unknown2)
		{
			// Unknown structure of 3 bytes
			unknown1.setValue8(0, (sbyte) 0);
			unknown1.setValue8(1, (sbyte) 0);
			unknown1.setValue8(2, (sbyte) 0);

			unknown2.setValue(0);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBB069A87, version = 150) public int sceNpGetContentRatingFlag(@CanBeNull pspsharp.HLE.TPointer32 parentalControlAddr, @CanBeNull pspsharp.HLE.TPointer32 userAgeAddr)
		[HLEFunction(nid : 0xBB069A87, version : 150)]
		public virtual int sceNpGetContentRatingFlag(TPointer32 parentalControlAddr, TPointer32 userAgeAddr)
		{
			parentalControlAddr.setValue(parentalControl);
			userAgeAddr.setValue(UserAge);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7E0864DF, version = 150) public int sceNpGetUserProfile(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=216, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0x7E0864DF, version : 150)]
		public virtual int sceNpGetUserProfile(TPointer buffer)
		{
			// The first 20 bytes are the onlineId
			buffer.setStringNZ(0, 20, OnlineId);
			// The next 16 bytes are unknown
			buffer.clear(20, 16);
			// The next 127 bytes are the avatar URL
			buffer.setStringNZ(36, 128, AvatarUrl);
			// The next 52 bytes are unknown
			buffer.clear(164, 52);
			// Total size 216 bytes

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4B5C71C8, version = 150) public int sceNpGetOnlineId(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=20, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0x4B5C71C8, version : 150)]
		public virtual int sceNpGetOnlineId(TPointer buffer)
		{
			buffer.setStringNZ(0, 20, OnlineId);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1D60AE4B, version = 150) public int sceNpGetChatRestrictionFlag()
		[HLEFunction(nid : 0x1D60AE4B, version : 150)]
		public virtual int sceNpGetChatRestrictionFlag()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCDCC21D3, version = 150) public int sceNpGetMyLanguages()
		[HLEFunction(nid : 0xCDCC21D3, version : 150)]
		public virtual int sceNpGetMyLanguages()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x02CA8CAA, version = 150) public int sceNp_02CA8CAA()
		[HLEFunction(nid : 0x02CA8CAA, version : 150)]
		public virtual int sceNp_02CA8CAA()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0B8BEE48, version = 150) public int sceNp_0B8BEE48()
		[HLEFunction(nid : 0x0B8BEE48, version : 150)]
		public virtual int sceNp_0B8BEE48()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1916432C, version = 150) public int sceNp_1916432C()
		[HLEFunction(nid : 0x1916432C, version : 150)]
		public virtual int sceNp_1916432C()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2D8E93D2, version = 150) public int sceNp_2D8E93D2()
		[HLEFunction(nid : 0x2D8E93D2, version : 150)]
		public virtual int sceNp_2D8E93D2()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2E4769C3, version = 150) public int sceNp_2E4769C3()
		[HLEFunction(nid : 0x2E4769C3, version : 150)]
		public virtual int sceNp_2E4769C3()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4556A257, version = 150) public int sceNp_4556A257()
		[HLEFunction(nid : 0x4556A257, version : 150)]
		public virtual int sceNp_4556A257()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4B09907A, version = 150) public int sceNp_4B09907A()
		[HLEFunction(nid : 0x4B09907A, version : 150)]
		public virtual int sceNp_4B09907A()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5FA879D8, version = 150) public int sceNp_5FA879D8(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown1, pspsharp.HLE.PspString unknown2, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=32, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown3, int unknown4, @CanBeNull pspsharp.HLE.TPointer32 unknown5)
		[HLEFunction(nid : 0x5FA879D8, version : 150)]
		public virtual int sceNp_5FA879D8(TPointer unknown1, PspString unknown2, TPointer unknown3, int unknown4, TPointer32 unknown5)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x60A4791A, version = 150) public int sceNp_60A4791A()
		[HLEFunction(nid : 0x60A4791A, version : 150)]
		public virtual int sceNp_60A4791A()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x64F72B22, version = 150) public int sceNp_64F72B22()
		[HLEFunction(nid : 0x64F72B22, version : 150)]
		public virtual int sceNp_64F72B22()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x66B86876, version = 150) public int sceNp_66B86876(int unknown1, int unknown2, int unknown3, int unknown4)
		[HLEFunction(nid : 0x66B86876, version : 150)]
		public virtual int sceNp_66B86876(int unknown1, int unknown2, int unknown3, int unknown4)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x67BCF9E3, version = 150) public int sceNp_67BCF9E3()
		[HLEFunction(nid : 0x67BCF9E3, version : 150)]
		public virtual int sceNp_67BCF9E3()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6999EDC4, version = 150) public int sceNp_6999EDC4()
		[HLEFunction(nid : 0x6999EDC4, version : 150)]
		public virtual int sceNp_6999EDC4()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6CBB0614, version = 150) public int sceNp_6CBB0614()
		[HLEFunction(nid : 0x6CBB0614, version : 150)]
		public virtual int sceNp_6CBB0614()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9A264EF2, version = 150) public int sceNp_9A264EF2()
		[HLEFunction(nid : 0x9A264EF2, version : 150)]
		public virtual int sceNp_9A264EF2()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9B87B19B, version = 150) public int sceNp_9B87B19B()
		[HLEFunction(nid : 0x9B87B19B, version : 150)]
		public virtual int sceNp_9B87B19B()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB2BFADB2, version = 150) public int sceNp_B2BFADB2()
		[HLEFunction(nid : 0xB2BFADB2, version : 150)]
		public virtual int sceNp_B2BFADB2()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB4063F7A, version = 150) public int sceNp_B4063F7A()
		[HLEFunction(nid : 0xB4063F7A, version : 150)]
		public virtual int sceNp_B4063F7A()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB819A0C8, version = 150) public int sceNp_B819A0C8(pspsharp.HLE.TPointer unknown1, pspsharp.HLE.TPointer unknown2, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=8, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown3)
		[HLEFunction(nid : 0xB819A0C8, version : 150)]
		public virtual int sceNp_B819A0C8(TPointer unknown1, TPointer unknown2, TPointer unknown3)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC0B3616C, version = 150) public int sceNp_C0B3616C(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=32, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer unknown1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknown2)
		[HLEFunction(nid : 0xC0B3616C, version : 150)]
		public virtual int sceNp_C0B3616C(TPointer unknown1, TPointer unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC250A650, version = 150) public int sceNp_C250A650()
		[HLEFunction(nid : 0xC250A650, version : 150)]
		public virtual int sceNp_C250A650()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC48F2847, version = 150) public int sceNp_C48F2847()
		[HLEFunction(nid : 0xC48F2847, version : 150)]
		public virtual int sceNp_C48F2847()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCF83CC3B, version = 150) public int sceNp_CF83CC3B()
		[HLEFunction(nid : 0xCF83CC3B, version : 150)]
		public virtual int sceNp_CF83CC3B()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE24DA399, version = 150) public int sceNp_E24DA399()
		[HLEFunction(nid : 0xE24DA399, version : 150)]
		public virtual int sceNp_E24DA399()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE41288E7, version = 150) public int sceNp_E41288E7()
		[HLEFunction(nid : 0xE41288E7, version : 150)]
		public virtual int sceNp_E41288E7()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE90DFBC4, version = 150) public int sceNp_E90DFBC4()
		[HLEFunction(nid : 0xE90DFBC4, version : 150)]
		public virtual int sceNp_E90DFBC4()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF2B95034, version = 150) public int sceNp_F2B95034()
		[HLEFunction(nid : 0xF2B95034, version : 150)]
		public virtual int sceNp_F2B95034()
		{
			return 0;
		}
	}
}