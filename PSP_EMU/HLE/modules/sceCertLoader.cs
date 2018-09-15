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

	public class sceCertLoader : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceCertLoader");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDD629A24, version = 150) public int sceLoadCertFromFlash(int unknown1, int unknown2, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown3, int unknown4, int unknown5, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown6)
		[HLEFunction(nid : 0xDD629A24, version : 150)]
		public virtual int sceLoadCertFromFlash(int unknown1, int unknown2, TPointer32 unknown3, int unknown4, int unknown5, TPointer32 unknown6)
		{
			unknown3.setValue(0);
			unknown6.setValue(7100);

			return 0;
		}
	}

}