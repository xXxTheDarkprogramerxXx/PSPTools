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

	using pspIoDrv = pspsharp.HLE.kernel.types.pspIoDrv;

	public class IoFileMgrForKernel : HLEModule
	{
		//public static Logger log = Modules.getLogger("IoFileMgrForKernel");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8E982A74, version = 150) public int sceIoAddDrv(pspsharp.HLE.kernel.types.pspIoDrv pspsharp.HLE.kernel.types.pspIoDrv)
		[HLEFunction(nid : 0x8E982A74, version : 150)]
		public virtual int sceIoAddDrv(pspIoDrv pspsharp)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC7F35804, version = 150) public int sceIoDelDrv(pspsharp.HLE.PspString driverName)
		[HLEFunction(nid : 0xC7F35804, version : 150)]
		public virtual int sceIoDelDrv(PspString driverName)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2B6A9B21, version = 150) public int IoFileMgrForKernel_30E8ABB3()
		[HLEFunction(nid : 0x2B6A9B21, version : 150)]
		public virtual int IoFileMgrForKernel_30E8ABB3()
		{
			return 0;
		}
	}

}