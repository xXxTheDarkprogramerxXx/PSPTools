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


	public class sceDdr : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceDdr");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x87D86769, version = 150) public int sceDdrFlush(int unknown)
		[HLEFunction(nid : 0x87D86769, version : 150)]
		public virtual int sceDdrFlush(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4CE55E76, version = 150) public int sceDdrChangePllClock(int unknown)
		[HLEFunction(nid : 0x4CE55E76, version : 150)]
		public virtual int sceDdrChangePllClock(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCF07B9E2, version = 150) public int sceDdrChangePllClock_660(int unknown)
		[HLEFunction(nid : 0xCF07B9E2, version : 150)]
		public virtual int sceDdrChangePllClock_660(int unknown)
		{
			return sceDdrChangePllClock(unknown);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x95B9A692, version = 150) public int sceDdr_driver_95B9A692()
		[HLEFunction(nid : 0x95B9A692, version : 150)]
		public virtual int sceDdr_driver_95B9A692()
		{
			// Has no parameters
			return 0;
		}
	}

}