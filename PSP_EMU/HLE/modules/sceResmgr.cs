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

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;

	public class sceResmgr : HLEModule
	{
		public static Logger log = Modules.getLogger("sceResmgr");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9DC14891, version = 150) public int sceResmgr_9DC14891(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer buffer, int bufferSize, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 resultLengthAddr)
		[HLEFunction(nid : 0x9DC14891, version : 150)]
		public virtual int sceResmgr_9DC14891(TPointer buffer, int bufferSize, TPointer32 resultLengthAddr)
		{
			string result = "release:6.60:\n";
			result += "build:5455,0,3,1,0:builder@vsh-build6\n";
			result += "system:57716@release_660,0x06060010:\n";
			result += "vsh:p6616@release_660,v58533@release_660,20110727:\n";
			result += "target:1:WorldWide\n";

			buffer.StringZ = result;
			resultLengthAddr.setValue(result.Length);

			return 0;
		}
	}

}