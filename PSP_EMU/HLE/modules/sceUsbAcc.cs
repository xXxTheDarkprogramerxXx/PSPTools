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


	public class sceUsbAcc : HLEModule
	{
		public static Logger log = Modules.getLogger("sceUsbAcc");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0CD7D4AA, version = 260) public int sceUsbAccGetInfo(pspsharp.HLE.TPointer resultAddr)
		[HLEFunction(nid : 0x0CD7D4AA, version : 260)]
		public virtual int sceUsbAccGetInfo(TPointer resultAddr)
		{
			// resultAddr is pointing to an 8-byte area.
			// Not sure about the content...
			resultAddr.clear(8);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x79A1C743, version = 260) public int sceUsbAccGetAuthStat()
		[HLEFunction(nid : 0x79A1C743, version : 260)]
		public virtual int sceUsbAccGetAuthStat()
		{
			// Has no parameters
			return 0;
		}
	}

}