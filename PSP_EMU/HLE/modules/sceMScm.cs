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
	using MemoryStick = pspsharp.hardware.MemoryStick;

	public class sceMScm : HLEModule
	{
		public static Logger log = Modules.getLogger("sceMScm");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0128147B, version = 150) public int sceMScmWriteMSReg()
		[HLEFunction(nid : 0x0128147B, version : 150)]
		public virtual int sceMScmWriteMSReg()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x08A1EC6B, version = 150) public int sceMScmUnRegisterCLD()
		[HLEFunction(nid : 0x08A1EC6B, version : 150)]
		public virtual int sceMScmUnRegisterCLD()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0A054CDA, version = 150) public int sceMScmHCLastStatus()
		[HLEFunction(nid : 0x0A054CDA, version : 150)]
		public virtual int sceMScmHCLastStatus()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0D8D6A54, version = 150) public int sceMScm_driver_0D8D6A54()
		[HLEFunction(nid : 0x0D8D6A54, version : 150)]
		public virtual int sceMScm_driver_0D8D6A54()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x12BD8FEC, version = 150) public int sceMScmRegisterCLD()
		[HLEFunction(nid : 0x12BD8FEC, version : 150)]
		public virtual int sceMScmRegisterCLD()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x18DD6E90, version = 150) public int sceMScm_driver_18DD6E90()
		[HLEFunction(nid : 0x18DD6E90, version : 150)]
		public virtual int sceMScm_driver_18DD6E90()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x21183216, version = 150) public int sceMScmWriteDataPIO(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 internalStructure, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer buffer, int bufferLength, int timeout)
		[HLEFunction(nid : 0x21183216, version : 150)]
		public virtual int sceMScmWriteDataPIO(TPointer32 internalStructure, TPointer buffer, int bufferLength, int timeout)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2AD0A649, version = 150) public int sceMScmGetSlotState(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 internalStructure, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 slotStateAddr)
		[HLEFunction(nid : 0x2AD0A649, version : 150)]
		public virtual int sceMScmGetSlotState(TPointer32 internalStructure, TPointer32 slotStateAddr)
		{
			slotStateAddr.setValue(MemoryStick.Inserted ? 1 : 2);
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2D7C40FA, version = 150) public int sceMScmWaitHCIntr(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 internalStructure, int unknown1, int unknown2)
		[HLEFunction(nid : 0x2D7C40FA, version : 150)]
		public virtual int sceMScmWaitHCIntr(TPointer32 internalStructure, int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x34124B97, version = 150) public int sceMScm_driver_34124B97()
		[HLEFunction(nid : 0x34124B97, version : 150)]
		public virtual int sceMScm_driver_34124B97()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x36921225, version = 150) public int sceMScm_driver_36921225()
		[HLEFunction(nid : 0x36921225, version : 150)]
		public virtual int sceMScm_driver_36921225()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3FFE76E5, version = 150) public int sceMScm_driver_3FFE76E5()
		[HLEFunction(nid : 0x3FFE76E5, version : 150)]
		public virtual int sceMScm_driver_3FFE76E5()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x42A40895, version = 150) public int sceMScmReadMSReg()
		[HLEFunction(nid : 0x42A40895, version : 150)]
		public virtual int sceMScmReadMSReg()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4451D813, version = 150) public int sceMScmDetachCLD()
		[HLEFunction(nid : 0x4451D813, version : 150)]
		public virtual int sceMScmDetachCLD()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x46AAC4E5, version = 150) public int sceMScmSendTPCDMA()
		[HLEFunction(nid : 0x46AAC4E5, version : 150)]
		public virtual int sceMScmSendTPCDMA()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x494FB570, version = 150) public int sceMScm_driver_494FB570()
		[HLEFunction(nid : 0x494FB570, version : 150)]
		public virtual int sceMScm_driver_494FB570()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4E4C3099, version = 150) public int sceMScm_driver_4E4C3099()
		[HLEFunction(nid : 0x4E4C3099, version : 150)]
		public virtual int sceMScm_driver_4E4C3099()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4E923738, version = 150) public int sceMScmStartModule()
		[HLEFunction(nid : 0x4E923738, version : 150)]
		public virtual int sceMScmStartModule()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4FA42259, version = 150) public int sceMScmTPCExSetCmd()
		[HLEFunction(nid : 0x4FA42259, version : 150)]
		public virtual int sceMScmTPCExSetCmd()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x54B3A3F1, version = 150) public int sceMScmGetCLDM()
		[HLEFunction(nid : 0x54B3A3F1, version : 150)]
		public virtual int sceMScmGetCLDM()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5AB92658, version = 150) public int sceMScm_driver_5AB92658()
		[HLEFunction(nid : 0x5AB92658, version : 150)]
		public virtual int sceMScm_driver_5AB92658()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5BAAB238, version = 150) public int sceMScmRegisterCLDM()
		[HLEFunction(nid : 0x5BAAB238, version : 150)]
		public virtual int sceMScmRegisterCLDM()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x68C14F25, version = 150) public int sceMScmStopModule()
		[HLEFunction(nid : 0x68C14F25, version : 150)]
		public virtual int sceMScmStopModule()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6C8AEF0B, version = 150) public int sceMScm_driver_6C8AEF0B()
		[HLEFunction(nid : 0x6C8AEF0B, version : 150)]
		public virtual int sceMScm_driver_6C8AEF0B()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6F4F7E4C, version = 150) public int sceMScm_driver_6F4F7E4C()
		[HLEFunction(nid : 0x6F4F7E4C, version : 150)]
		public virtual int sceMScm_driver_6F4F7E4C()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7B5396BA, version = 150) public int sceMScm_driver_7B5396BA()
		[HLEFunction(nid : 0x7B5396BA, version : 150)]
		public virtual int sceMScm_driver_7B5396BA()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x907D7766, version = 150) public int sceMScmSendTPC(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=12, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 internalStructure, int command, int length, int timeout)
		[HLEFunction(nid : 0x907D7766, version : 150)]
		public virtual int sceMScmSendTPC(TPointer32 internalStructure, int command, int length, int timeout)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x97290A44, version = 150) public int sceMScmStartWriteDataDMA()
		[HLEFunction(nid : 0x97290A44, version : 150)]
		public virtual int sceMScmStartWriteDataDMA()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9A5CB5CC, version = 150) public int sceMScmUnRegisterCLDM()
		[HLEFunction(nid : 0x9A5CB5CC, version : 150)]
		public virtual int sceMScmUnRegisterCLDM()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAA67D87A, version = 150) public int sceMScmResetHC()
		[HLEFunction(nid : 0xAA67D87A, version : 150)]
		public virtual int sceMScmResetHC()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB01C378C, version = 150) public int sceMScm_driver_B01C378C()
		[HLEFunction(nid : 0xB01C378C, version : 150)]
		public virtual int sceMScm_driver_B01C378C()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB1D1C718, version = 150) public int sceMScm_driver_B1D1C718()
		[HLEFunction(nid : 0xB1D1C718, version : 150)]
		public virtual int sceMScm_driver_B1D1C718()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBE455B5D, version = 150) public int sceMScmReadDataPIO(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 internalStructure, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buffer, int bufferLength, int timeout)
		[HLEFunction(nid : 0xBE455B5D, version : 150)]
		public virtual int sceMScmReadDataPIO(TPointer32 internalStructure, TPointer buffer, int bufferLength, int timeout)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC003705D, version = 150) public int sceMScm_driver_C003705D()
		[HLEFunction(nid : 0xC003705D, version : 150)]
		public virtual int sceMScm_driver_C003705D()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCBB2BF6F, version = 150) public int sceMScm_driver_CBB2BF6F()
		[HLEFunction(nid : 0xCBB2BF6F, version : 150)]
		public virtual int sceMScm_driver_CBB2BF6F()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCFD8F662, version = 150) public int sceMScm_driver_CFD8F662()
		[HLEFunction(nid : 0xCFD8F662, version : 150)]
		public virtual int sceMScm_driver_CFD8F662()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD6DB3199, version = 150) public int sceMScm_driver_D6DB3199()
		[HLEFunction(nid : 0xD6DB3199, version : 150)]
		public virtual int sceMScm_driver_D6DB3199()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDE188BB6, version = 150) public int sceMScmStartReadDataDMA()
		[HLEFunction(nid : 0xDE188BB6, version : 150)]
		public virtual int sceMScmStartReadDataDMA()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE24B5D0C, version = 150) public int sceMScm_driver_E24B5D0C()
		[HLEFunction(nid : 0xE24B5D0C, version : 150)]
		public virtual int sceMScm_driver_E24B5D0C()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEF42A4A3, version = 150) public int sceMScm_driver_EF42A4A3()
		[HLEFunction(nid : 0xEF42A4A3, version : 150)]
		public virtual int sceMScm_driver_EF42A4A3()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF0CC24D1, version = 150) public int sceMScm_driver_F0CC24D1()
		[HLEFunction(nid : 0xF0CC24D1, version : 150)]
		public virtual int sceMScm_driver_F0CC24D1()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF1133DAF, version = 150) public int sceMScm_driver_F1133DAF()
		[HLEFunction(nid : 0xF1133DAF, version : 150)]
		public virtual int sceMScm_driver_F1133DAF()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF1924A06, version = 150) public int sceMScmSetSlotPower()
		[HLEFunction(nid : 0xF1924A06, version : 150)]
		public virtual int sceMScmSetSlotPower()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF82AF926, version = 150) public int sceMScmTPCSetCmd(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 internalStructure, int command, int unknown1, int unknown2)
		[HLEFunction(nid : 0xF82AF926, version : 150)]
		public virtual int sceMScmTPCSetCmd(TPointer32 internalStructure, int command, int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFCD92C74, version = 150) public int sceMScmGetSlotPower()
		[HLEFunction(nid : 0xFCD92C74, version : 150)]
		public virtual int sceMScmGetSlotPower()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFF6C50D8, version = 150) public int sceMScmTPCGetInt(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 internalStructure, int unknown1, int unknown2, int unknown3)
		[HLEFunction(nid : 0xFF6C50D8, version : 150)]
		public virtual int sceMScmTPCGetInt(TPointer32 internalStructure, int unknown1, int unknown2, int unknown3)
		{
			return 0;
		}
	}

}