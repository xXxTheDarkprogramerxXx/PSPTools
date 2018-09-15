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

	public class sceNpInstall : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceNpInstall");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0B039B36, version = 150) public int sceNpInstallActivation(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknown1, int size, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=64, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer unknown2)
		[HLEFunction(nid : 0x0B039B36, version : 150)]
		public virtual int sceNpInstallActivation(TPointer unknown1, int size, TPointer unknown2)
		{
			unknown2.clear(64);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5847D8C7, version = 150) public int sceNpInstallGetChallenge(int unknown1, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=32, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer inputKey, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=128, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer challenge, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=64, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer unknown4)
		[HLEFunction(nid : 0x5847D8C7, version : 150)]
		public virtual int sceNpInstallGetChallenge(int unknown1, TPointer inputKey, TPointer challenge, TPointer unknown4)
		{
			challenge.clear(128);
			unknown4.clear(64);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7AE4C8BC, version = 150) public int sceNpInstallDeactivation()
		[HLEFunction(nid : 0x7AE4C8BC, version : 150)]
		public virtual int sceNpInstallDeactivation()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x91F9D50D, version = 150) public int sceNpInstallCheckActivation(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown1, int unknown2)
		[HLEFunction(nid : 0x91F9D50D, version : 150)]
		public virtual int sceNpInstallCheckActivation(TPointer32 unknown1, int unknown2)
		{
			unknown1.setValue(0);

			return 0;
		}
	}

}