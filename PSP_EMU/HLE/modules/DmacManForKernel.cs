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

	public class DmacManForKernel : HLEModule
	{
		public static Logger log = Modules.getLogger("DmacManForKernel");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x59615199, version = 150) public int sceKernelDmaOpAlloc()
		[HLEFunction(nid : 0x59615199, version : 150)]
		public virtual int sceKernelDmaOpAlloc()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x745E19EF, version = 150) public int sceKernelDmaOpFree()
		[HLEFunction(nid : 0x745E19EF, version : 150)]
		public virtual int sceKernelDmaOpFree()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF64BAB99, version = 150) public int sceKernelDmaOpAssign(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=30, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer dmaOpAddr, int unknown1, int unknown2, int unknown3)
		[HLEFunction(nid : 0xF64BAB99, version : 150)]
		public virtual int sceKernelDmaOpAssign(TPointer dmaOpAddr, int unknown1, int unknown2, int unknown3)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3BDEA96C, version = 150) public int sceKernelDmaOpEnQueue(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=30, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer dmaOpAddr)
		[HLEFunction(nid : 0x3BDEA96C, version : 150)]
		public virtual int sceKernelDmaOpEnQueue(TPointer dmaOpAddr)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5AF32783, version = 150) public int sceKernelDmaOpQuit(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=30, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer dmaOpAddr)
		[HLEFunction(nid : 0x5AF32783, version : 150)]
		public virtual int sceKernelDmaOpQuit(TPointer dmaOpAddr)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x92700CCD, version = 150) public int sceKernelDmaOpDeQueue()
		[HLEFunction(nid : 0x92700CCD, version : 150)]
		public virtual int sceKernelDmaOpDeQueue()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCE467D9B, version = 150) public int sceKernelDmaOpSetupNormal(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=30, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer dmaOpAddr, int status, pspsharp.HLE.TPointer dstAddress, pspsharp.HLE.TPointer srcAddress, int attributes)
		[HLEFunction(nid : 0xCE467D9B, version : 150)]
		public virtual int sceKernelDmaOpSetupNormal(TPointer dmaOpAddr, int status, TPointer dstAddress, TPointer srcAddress, int attributes)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD0358BE9, version = 150) public int sceKernelDmaOpSetCallback(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=30, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer dmaOpAddr, pspsharp.HLE.TPointer callback, int unknown)
		[HLEFunction(nid : 0xD0358BE9, version : 150)]
		public virtual int sceKernelDmaOpSetCallback(TPointer dmaOpAddr, TPointer callback, int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDB286D65, version = 150) public int sceKernelDmaOpSync(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=30, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer dmaOpAddr, int waitType, int timeout)
		[HLEFunction(nid : 0xDB286D65, version : 150)]
		public virtual int sceKernelDmaOpSync(TPointer dmaOpAddr, int waitType, int timeout)
		{
			// waitType = 0: do not wait for completion of DMA Operation, return error when still running
			// waitType = 1: wait indefinitely for completion of the DMA Operation
			// waitType = 2: wait for given timeout for completion of the DMA Operation
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7D21A2EF, version = 150) public int sceKernelDmaOpSetupLink(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=30, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer dmaOpAddr, int status, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=16, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 linkStructure)
		[HLEFunction(nid : 0x7D21A2EF, version : 150)]
		public virtual int sceKernelDmaOpSetupLink(TPointer dmaOpAddr, int status, TPointer32 linkStructure)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3FAD5844, version = 150) public int sceKernelDmaOpSetupMemcpy(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=30, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer dmaOpAddr, pspsharp.HLE.TPointer dstAddress, pspsharp.HLE.TPointer srcAddress, int length)
		[HLEFunction(nid : 0x3FAD5844, version : 150)]
		public virtual int sceKernelDmaOpSetupMemcpy(TPointer dmaOpAddr, TPointer dstAddress, TPointer srcAddress, int length)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x32757C57, version = 150) public int DmacManForKernel_32757C57(@CanBeNull pspsharp.HLE.TPointer setupLinkCallback)
		[HLEFunction(nid : 0x32757C57, version : 150)]
		public virtual int DmacManForKernel_32757C57(TPointer setupLinkCallback)
		{
			return 0;
		}
	}

}