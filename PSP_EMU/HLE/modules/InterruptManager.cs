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
	using Managers = pspsharp.HLE.kernel.Managers;
	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;

	//using Logger = org.apache.log4j.Logger;

	public class InterruptManager : HLEModule
	{
		//public static Logger log = Modules.getLogger("InterruptManager");

		public override void stop()
		{
			Managers.intr.stop();
			base.stop();
		}

		[HLEFunction(nid : 0xCA04A2B9, version : 150)]
		public virtual int sceKernelRegisterSubIntrHandler(int intrNumber, int subIntrNumber, TPointer handlerAddress, int handlerArgument)
		{
			return Managers.intr.sceKernelRegisterSubIntrHandler(intrNumber, subIntrNumber, handlerAddress, handlerArgument);
		}

		[HLEFunction(nid : 0xD61E6961, version : 150)]
		public virtual int sceKernelReleaseSubIntrHandler(int intrNumber, int subIntrNumber)
		{
			return Managers.intr.sceKernelReleaseSubIntrHandler(intrNumber, subIntrNumber);
		}

		[HLEFunction(nid : 0xFB8E22EC, version : 150)]
		public virtual int sceKernelEnableSubIntr(int intrNumber, int subIntrNumber)
		{
			return Managers.intr.sceKernelEnableSubIntr(intrNumber, subIntrNumber);
		}

		[HLEFunction(nid : 0x8A389411, version : 150)]
		public virtual int sceKernelDisableSubIntr(int intrNumber, int subIntrNumber)
		{
			return Managers.intr.sceKernelDisableSubIntr(intrNumber, subIntrNumber);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5CB5A78B, version = 150) public int sceKernelSuspendSubIntr()
		[HLEFunction(nid : 0x5CB5A78B, version : 150)]
		public virtual int sceKernelSuspendSubIntr()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7860E0DC, version = 150) public int sceKernelResumeSubIntr()
		[HLEFunction(nid : 0x7860E0DC, version : 150)]
		public virtual int sceKernelResumeSubIntr()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFC4374B8, version = 150) public int sceKernelIsSubInterruptOccurred()
		[HLEFunction(nid : 0xFC4374B8, version : 150)]
		public virtual int sceKernelIsSubInterruptOccurred()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD2E8363F, version = 150) public int QueryIntrHandlerInfo()
		[HLEFunction(nid : 0xD2E8363F, version : 150)]
		public virtual int QueryIntrHandlerInfo()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEEE43F47, version = 150) public int sceKernelRegisterUserSpaceIntrStack()
		[HLEFunction(nid : 0xEEE43F47, version : 150)]
		public virtual int sceKernelRegisterUserSpaceIntrStack()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD774BA45, version = 150) public int sceKernelDisableIntr(int intrNumber)
		[HLEFunction(nid : 0xD774BA45, version : 150)]
		public virtual int sceKernelDisableIntr(int intrNumber)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4D6E7305, version = 150) public int sceKernelEnableIntr(int intrNumber)
		[HLEFunction(nid : 0x4D6E7305, version : 150)]
		public virtual int sceKernelEnableIntr(int intrNumber)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDB14CBE0, version = 150) public int sceKernelResumeIntr()
		[HLEFunction(nid : 0xDB14CBE0, version : 150)]
		public virtual int sceKernelResumeIntr()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0C5F7AE3, version = 150) public int sceKernelCallSubIntrHandler(int intrNum, int subIntrNum, int handlerArg0, int handlerArg2)
		[HLEFunction(nid : 0x0C5F7AE3, version : 150)]
		public virtual int sceKernelCallSubIntrHandler(int intrNum, int subIntrNum, int handlerArg0, int handlerArg2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4023E1A7, version = 150) public int sceKernelDisableSubIntr()
		[HLEFunction(nid : 0x4023E1A7, version : 150)]
		public virtual int sceKernelDisableSubIntr()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x58DD8978, version = 150) public int sceKernelRegisterIntrHandler(int intrNumber, int unknown1, pspsharp.HLE.TPointer func, int funcArg, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 handler)
		[HLEFunction(nid : 0x58DD8978, version : 150)]
		public virtual int sceKernelRegisterIntrHandler(int intrNumber, int unknown1, TPointer func, int funcArg, TPointer32 handler)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF987B1F0, version = 150) public int sceKernelReleaseIntrHandler(int intrNumber)
		[HLEFunction(nid : 0xF987B1F0, version : 150)]
		public virtual int sceKernelReleaseIntrHandler(int intrNumber)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFFA8B183, version = 660) public int sceKernelRegisterSubIntrHandler_660(int intrNumber, int subIntrNumber, pspsharp.HLE.TPointer handlerAddress, int handlerArgument)
		[HLEFunction(nid : 0xFFA8B183, version : 660)]
		public virtual int sceKernelRegisterSubIntrHandler_660(int intrNumber, int subIntrNumber, TPointer handlerAddress, int handlerArgument)
		{
			return sceKernelRegisterSubIntrHandler(intrNumber, subIntrNumber, handlerAddress, handlerArgument);
		}

		[HLEFunction(nid : 0xFE28C6D9, version : 150)]
		public virtual bool sceKernelIsIntrContext()
		{
			return IntrManager.Instance.InsideInterrupt;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA0F88036, version = 150) public int sceKernelGetSyscallRA()
		[HLEFunction(nid : 0xA0F88036, version : 150)]
		public virtual int sceKernelGetSyscallRA()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x14D4C61A, version = 660) public int sceKernelRegisterSystemCallTable_660(pspsharp.HLE.TPointer syscallTable)
		[HLEFunction(nid : 0x14D4C61A, version : 660)]
		public virtual int sceKernelRegisterSystemCallTable_660(TPointer syscallTable)
		{
			return 0;
		}
	}
}