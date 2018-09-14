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


	public class sceNpCore : HLEModule
	{
		public static Logger log = Modules.getLogger("sceNpCore");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x52440ABF, version = 150) public int sceNpCore_52440ABF(pspsharp.HLE.TPointer unknown)
		[HLEFunction(nid : 0x52440ABF, version : 150)]
		public virtual int sceNpCore_52440ABF(TPointer unknown)
		{
			return Modules.SysMemForKernelModule.SysMemForKernel_7FF2F35A(unknown);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0366DAB6, version = 150) public int sceNpCore_0366DAB6()
		[HLEFunction(nid : 0x0366DAB6, version : 150)]
		public virtual int sceNpCore_0366DAB6()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x04096629, version = 150) public int sceNpCore_04096629()
		[HLEFunction(nid : 0x04096629, version : 150)]
		public virtual int sceNpCore_04096629()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x243690EE, version = 150) public int sceNpCore_243690EE()
		[HLEFunction(nid : 0x243690EE, version : 150)]
		public virtual int sceNpCore_243690EE()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5145344F, version = 150) public int sceNpCore_5145344F()
		[HLEFunction(nid : 0x5145344F, version : 150)]
		public virtual int sceNpCore_5145344F()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x515B65E8, version = 150) public int sceNpCore_515B65E8()
		[HLEFunction(nid : 0x515B65E8, version : 150)]
		public virtual int sceNpCore_515B65E8()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x57E15796, version = 150) public int sceNpCore_57E15796()
		[HLEFunction(nid : 0x57E15796, version : 150)]
		public virtual int sceNpCore_57E15796()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8AFAB4A0, version = 150) public int sceNpCore_8AFAB4A0()
		[HLEFunction(nid : 0x8AFAB4A0, version : 150)]
		public virtual int sceNpCore_8AFAB4A0()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9218ACF6, version = 150) public int sceNpCore_9218ACF6()
		[HLEFunction(nid : 0x9218ACF6, version : 150)]
		public virtual int sceNpCore_9218ACF6()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB13D27CA, version = 150) public int sceNpCore_B13D27CA()
		[HLEFunction(nid : 0xB13D27CA, version : 150)]
		public virtual int sceNpCore_B13D27CA()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE7AED5A3, version = 150) public int sceNpCore_E7AED5A3()
		[HLEFunction(nid : 0xE7AED5A3, version : 150)]
		public virtual int sceNpCore_E7AED5A3()
		{
			return 0;
		}
	}

}