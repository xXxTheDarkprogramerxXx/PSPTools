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

	using CpuState = pspsharp.Allegrex.CpuState;

	public class KDebugForKernel : HLEModule
	{
		//public static Logger log = Modules.getLogger("KDebugForKernel");
		protected internal static Logger kprintf = Logger.getLogger("kprintf");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE7A3874D, version = 150) public int sceKernelRegisterAssertHandler()
		[HLEFunction(nid : 0xE7A3874D, version : 150)]
		public virtual int sceKernelRegisterAssertHandler()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2FF4E9F9, version = 150) public int sceKernelAssert()
		[HLEFunction(nid : 0x2FF4E9F9, version : 150)]
		public virtual int sceKernelAssert()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9B868276, version = 150) public int sceKernelGetDebugPutchar()
		[HLEFunction(nid : 0x9B868276, version : 150)]
		public virtual int sceKernelGetDebugPutchar()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE146606D, version = 150) public int sceKernelRegisterDebugPutchar()
		[HLEFunction(nid : 0xE146606D, version : 150)]
		public virtual int sceKernelRegisterDebugPutchar()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7CEB2C09, version = 150) public int sceKernelRegisterKprintfHandler()
		[HLEFunction(nid : 0x7CEB2C09, version : 150)]
		public virtual int sceKernelRegisterKprintfHandler()
		{
			return 0;
		}

		[HLEFunction(nid : 0x84F370BC, version : 150)]
		public virtual int Kprintf(CpuState cpu, PspString formatString)
		{
			return Modules.SysMemUserForUserModule.hleKernelPrintf(cpu, formatString, kprintf);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5CE9838B, version = 150) public int sceKernelDebugWrite()
		[HLEFunction(nid : 0x5CE9838B, version : 150)]
		public virtual int sceKernelDebugWrite()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x66253C4E, version = 150) public int sceKernelRegisterDebugWrite()
		[HLEFunction(nid : 0x66253C4E, version : 150)]
		public virtual int sceKernelRegisterDebugWrite()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDBB5597F, version = 150) public int sceKernelDebugRead()
		[HLEFunction(nid : 0xDBB5597F, version : 150)]
		public virtual int sceKernelDebugRead()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE6554FDA, version = 150) public int sceKernelRegisterDebugRead()
		[HLEFunction(nid : 0xE6554FDA, version : 150)]
		public virtual int sceKernelRegisterDebugRead()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB9C643C9, version = 150) public int sceKernelDebugEcho()
		[HLEFunction(nid : 0xB9C643C9, version : 150)]
		public virtual int sceKernelDebugEcho()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7D1C74F0, version = 150) public int sceKernelDebugEchoSet()
		[HLEFunction(nid : 0x7D1C74F0, version : 150)]
		public virtual int sceKernelDebugEchoSet()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x24C32559, version = 150) public int sceKernelDipsw(int unknown)
		[HLEFunction(nid : 0x24C32559, version : 150)]
		public virtual int sceKernelDipsw(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD636B827, version = 150) public int sceKernelDipswAll()
		[HLEFunction(nid : 0xD636B827, version : 150)]
		public virtual int sceKernelDipswAll()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5282DD5E, version = 150) public int sceKernelDipswSet()
		[HLEFunction(nid : 0x5282DD5E, version : 150)]
		public virtual int sceKernelDipswSet()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEE75658D, version = 150) public int sceKernelDipswClear()
		[HLEFunction(nid : 0xEE75658D, version : 150)]
		public virtual int sceKernelDipswClear()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9F8703E4, version = 150) public int KDebugForKernel_9F8703E4()
		[HLEFunction(nid : 0x9F8703E4, version : 150)]
		public virtual int KDebugForKernel_9F8703E4()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x333DCEC7, version = 150) public int KDebugForKernel_333DCEC7()
		[HLEFunction(nid : 0x333DCEC7, version : 150)]
		public virtual int KDebugForKernel_333DCEC7()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE892D9A1, version = 150) public int KDebugForKernel_E892D9A1()
		[HLEFunction(nid : 0xE892D9A1, version : 150)]
		public virtual int KDebugForKernel_E892D9A1()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA126F497, version = 150) public int KDebugForKernel_A126F497()
		[HLEFunction(nid : 0xA126F497, version : 150)]
		public virtual int KDebugForKernel_A126F497()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB7251823, version = 150) public int KDebugForKernel_B7251823()
		[HLEFunction(nid : 0xB7251823, version : 150)]
		public virtual int KDebugForKernel_B7251823()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x47570AC5, version = 150) public int sceKernelIsToolMode()
		[HLEFunction(nid : 0x47570AC5, version : 150)]
		public virtual int sceKernelIsToolMode()
		{
			return 0;
		}

		[HLEFunction(nid : 0x27B23800, version : 150)]
		public virtual bool sceKernelIsUMDMode()
		{
			return !sceKernelIsDVDMode();
		}

		[HLEFunction(nid : 0xB41E2430, version : 150)]
		public virtual bool sceKernelIsDVDMode()
		{
			return false;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x86010FCB, version = 150) public int sceKernelDipsw_660(int unknown)
		[HLEFunction(nid : 0x86010FCB, version : 150)]
		public virtual int sceKernelDipsw_660(int unknown)
		{
			return sceKernelDipsw(unknown);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xACF427DC, version = 150) public int sceKernelIsDevelopmentToolMode()
		[HLEFunction(nid : 0xACF427DC, version : 150)]
		public virtual int sceKernelIsDevelopmentToolMode()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF339073C, version = 150) public int sceKernelDeci2pReferOperations()
		[HLEFunction(nid : 0xF339073C, version : 150)]
		public virtual int sceKernelDeci2pReferOperations()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6CB0BDA4, version = 150) public int sceKernelDipswHigh32()
		[HLEFunction(nid : 0x6CB0BDA4, version : 150)]
		public virtual int sceKernelDipswHigh32()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x43F0F8AB, version = 150) public int sceKernelDipswLow32()
		[HLEFunction(nid : 0x43F0F8AB, version : 150)]
		public virtual int sceKernelDipswLow32()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x568DCD25, version = 150) public int sceKernelDipswCpTime()
		[HLEFunction(nid : 0x568DCD25, version : 150)]
		public virtual int sceKernelDipswCpTime()
		{
			// Has no parameters
			return 0;
		}
	}

}