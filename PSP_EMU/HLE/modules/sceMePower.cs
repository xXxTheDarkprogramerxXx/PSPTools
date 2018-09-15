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


	public class sceMePower : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceMePower");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1862B784, version = 150) public int sceMePower_driver_1862B784(int unknown1, int unknown2)
		[HLEFunction(nid : 0x1862B784, version : 150)]
		public virtual int sceMePower_driver_1862B784(int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE9F69ACF, version = 150) public int sceMePower_driver_E9F69ACF(int unknown1, int unknown2)
		[HLEFunction(nid : 0xE9F69ACF, version : 150)]
		public virtual int sceMePower_driver_E9F69ACF(int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB37562AA, version = 150) public int sceMePowerControlAvcPower(int unknown)
		[HLEFunction(nid : 0xB37562AA, version : 150)]
		public virtual int sceMePowerControlAvcPower(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x984E2608, version = 150) public int sceMePower_driver_984E2608(int unknown)
		[HLEFunction(nid : 0x984E2608, version : 150)]
		public virtual int sceMePower_driver_984E2608(int unknown)
		{
			return 0;
		}
	}

}