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

	public class sceMcctrl : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceMcctrl");

		// Init function?
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3EF531DB, version = 150) public int sceMcctrl_3EF531DB()
		[HLEFunction(nid : 0x3EF531DB, version : 150)]
		public virtual int sceMcctrl_3EF531DB()
		{
			// Has no parameters
			return 0;
		}

		// Term function?
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x877CD3A5, version = 150) public int sceMcctrl_877CD3A5()
		[HLEFunction(nid : 0x877CD3A5, version : 150)]
		public virtual int sceMcctrl_877CD3A5()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1EDFD6BB, version = 150) public int sceMcctrl_1EDFD6BB(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=288, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=176, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown2)
		[HLEFunction(nid : 0x1EDFD6BB, version : 150)]
		public virtual int sceMcctrl_1EDFD6BB(TPointer unknown1, TPointer unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7CAC25B2, version = 150) public int sceMcctrl_7CAC25B2()
		[HLEFunction(nid : 0x7CAC25B2, version : 150)]
		public virtual int sceMcctrl_7CAC25B2()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9618EE57, version = 150) public int sceMcctrl_9618EE57()
		[HLEFunction(nid : 0x9618EE57, version : 150)]
		public virtual int sceMcctrl_9618EE57()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x61550814, version = 150) public int sceMcctrl_61550814()
		[HLEFunction(nid : 0x61550814, version : 150)]
		public virtual int sceMcctrl_61550814()
		{
			return 0;
		}
	}

}