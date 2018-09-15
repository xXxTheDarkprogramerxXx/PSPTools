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

	using Usage = pspsharp.HLE.BufferInfo.Usage;

	/*
	 * scePwm - Power Management
	 */
	public class scePwm : HLEModule
	{
		//public static Logger log = Modules.getLogger("scePwm");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x36F98EBA, version = 150) public int scePwm_driver_36F98EBA(int unknown1, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer16 unknown2, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer16 unknown3, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown4)
		[HLEFunction(nid : 0x36F98EBA, version : 150)]
		public virtual int scePwm_driver_36F98EBA(int unknown1, TPointer16 unknown2, TPointer16 unknown3, TPointer32 unknown4)
		{
			unknown2.Value = 0;
			unknown3.Value = 0;
			unknown4.setValue(0);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x94552DD4, version = 150) public int scePwm_driver_94552DD4(int unknown1, int unknown2, int unknown3, int unknown4)
		[HLEFunction(nid : 0x94552DD4, version : 150)]
		public virtual int scePwm_driver_94552DD4(int unknown1, int unknown2, int unknown3, int unknown4)
		{
			return 0;
		}
	}

}