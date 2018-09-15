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


	public class sceMeAudio : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceMeAudio");

		// Called by sceAudiocodecCheckNeedMem
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x81956A0B, version = 150) public int sceMeAudio_driver_81956A0B(int codecType, pspsharp.HLE.TPointer workArea)
		[HLEFunction(nid : 0x81956A0B, version : 150)]
		public virtual int sceMeAudio_driver_81956A0B(int codecType, TPointer workArea)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6AD33F60, version = 150) public int sceMeAudio_driver_6AD33F60()
		[HLEFunction(nid : 0x6AD33F60, version : 150)]
		public virtual int sceMeAudio_driver_6AD33F60()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9A9E21EE, version = 150) public int sceMeAudio_driver_9A9E21EE()
		[HLEFunction(nid : 0x9A9E21EE, version : 150)]
		public virtual int sceMeAudio_driver_9A9E21EE()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB57F033A, version = 150) public int sceMeAudio_driver_B57F033A()
		[HLEFunction(nid : 0xB57F033A, version : 150)]
		public virtual int sceMeAudio_driver_B57F033A()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC300D466, version = 150) public int sceMeAudio_driver_C300D466()
		[HLEFunction(nid : 0xC300D466, version : 150)]
		public virtual int sceMeAudio_driver_C300D466()
		{
			return 0;
		}
	}

}