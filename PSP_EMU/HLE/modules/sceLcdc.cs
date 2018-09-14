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


	public class sceLcdc : HLEModule
	{
		public static Logger log = Modules.getLogger("sceLcdc");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE9DBD35F, version = 150) public int sceLcdc_driver_E9DBD35F(int unknown1, int displayWidth, int displayHeight, int unknown2)
		[HLEFunction(nid : 0xE9DBD35F, version : 150)]
		public virtual int sceLcdc_driver_E9DBD35F(int unknown1, int displayWidth, int displayHeight, int unknown2)
		{
			return 0;
		}
	}

}