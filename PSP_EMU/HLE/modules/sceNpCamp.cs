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


	public class sceNpCamp : HLEModule
	{
		public static Logger log = Modules.getLogger("sceNpCamp");

		// Init function?
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x486E4110, version = 150) public int sceNpCamp_486E4110()
		[HLEFunction(nid : 0x486E4110, version : 150)]
		public virtual int sceNpCamp_486E4110()
		{
			// Has no parameters
			return 0;
		}

		// Term function?
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA2D126CC, version = 150) public int sceNpCamp_A2D126CC()
		[HLEFunction(nid : 0xA2D126CC, version : 150)]
		public virtual int sceNpCamp_A2D126CC()
		{
			// Has no parameters
			return 0;
		}

		// Abort function?
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x72EC7057, version = 150) public int sceNpCamp_72EC7057()
		[HLEFunction(nid : 0x72EC7057, version : 150)]
		public virtual int sceNpCamp_72EC7057()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x18B9D112, version = 150) public int sceNpCamp_18B9D112()
		[HLEFunction(nid : 0x18B9D112, version : 150)]
		public virtual int sceNpCamp_18B9D112()
		{
			return 0;
		}
	}

}