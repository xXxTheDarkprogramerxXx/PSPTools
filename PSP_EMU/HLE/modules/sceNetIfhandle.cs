using System.Collections.Generic;

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

	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using Managers = pspsharp.HLE.kernel.Managers;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelVplInfo = pspsharp.HLE.kernel.types.SceKernelVplInfo;
	using SceNetIfHandle = pspsharp.HLE.kernel.types.SceNetIfHandle;
	using SceNetIfHandleInternal = pspsharp.HLE.kernel.types.SceNetIfHandle.SceNetIfHandleInternal;
	using SceNetIfMessage = pspsharp.HLE.kernel.types.SceNetIfMessage;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;

	using Logger = org.apache.log4j.Logger;

	public class sceNetIfhandle : HLEModule
	{
		public static Logger log = Modules.getLogger("sceNetIfhandle");
		protected internal Dictionary<int, SysMemInfo> allocatedMemory;
		protected internal int unknownCallback1;
		protected internal int unknownCallback2;
		protected internal int unknownCallback3;
		protected internal bool callbacksDefined;
		protected internal TPointer[] handles;
		protected internal int unknownValue1;
		protected internal int unknownValue2;
		protected internal int unknownValue3;
		protected internal int unknownValue4;

		public override void start()
		{
			allocatedMemory = new Dictionary<int, SysMemUserForUser.SysMemInfo>();
			unknownCallback1 = 0;
			unknownCallback2 = 0;
			unknownCallback3 = 0;
			callbacksDefined = false;
			handles = new TPointer[8];
			for (int i = 0; i < handles.Length; i++)
			{
				handles[i] = TPointer.NULL;
			}

			base.start();
		}

		public virtual TPointer checkHandleAddr(TPointer handleAddr)
		{
			for (int i = 0; i < handles.Length; i++)
			{
				if (handles[i].Address == handleAddr.Address)
				{
					return handleAddr;
				}
			}

			throw new SceKernelErrorException(SceKernelErrors.ERROR_NOT_FOUND);
		}

		public virtual TPointer checkHandleInternalAddr(TPointer handleInternalAddr)
		{
			for (int i = 0; i < handles.Length; i++)
			{
				if (handles[i].getValue32() == handleInternalAddr.Address)
				{
					return handleInternalAddr;
				}
			}

			throw new SceKernelErrorException(SceKernelErrors.ERROR_NOT_FOUND);
		}

		protected internal virtual int hleNetCreateIfhandleEther(TPointer handleAddr)
		{
			int handleIndex = -1;
			for (int i = 0; i < handles.Length; i++)
			{
				if (handles[i].Null)
				{
					handleIndex = i;
					break;
				}
			}

			if (handleIndex < 0)
			{
				return SceKernelErrors.ERROR_OUT_OF_MEMORY;
			}

			SceNetIfHandle.SceNetIfHandleInternal handleInternal = new SceNetIfHandle.SceNetIfHandleInternal();
			int allocatedMem = hleNetMallocInternal(handleInternal.@sizeof());
			if (allocatedMem < 0)
			{
				return SceKernelErrors.ERROR_OUT_OF_MEMORY;
			}
			handleAddr.setValue32(0, allocatedMem);
			handles[handleIndex] = handleAddr;

			handleInternal.write(new TPointer(Memory.Instance, allocatedMem));

			RuntimeContext.debugMemory(allocatedMem, handleInternal.@sizeof());

			return 0;
		}

		protected internal virtual int hleNetAttachIfhandleEther(TPointer handleAddr, pspNetMacAddress macAddress, string interfaceName)
		{
			SceNetIfHandle handle = new SceNetIfHandle();
			handle.read(handleAddr);
			handle.handleInternal.macAddress = macAddress;
			handle.handleInternal.interfaceName = interfaceName;
			handle.addrFirstMessageToBeSent = 0;
			handle.addrLastMessageToBeSent = 0;
			handle.numberOfMessagesToBeSent = 0;
			handle.unknown36 = 0;
			handle.unknown40 = 0;
			handle.write(handleAddr);

			return 0;
		}

		public virtual int hleNetMallocInternal(int size)
		{
			int allocatedAddr;
			// When flash0:/kd/ifhandle.prx is in use, allocate the memory through this
			// implementation instead of using the HLE implementation.
			// ifhandle.prx is creating a VPL name "SceNet" and allocating from it.
			SceKernelVplInfo vplInfo = Managers.vpl.getVplInfoByName("SceNet");
			if (vplInfo != null)
			{
				allocatedAddr = Managers.vpl.tryAllocateVpl(vplInfo, size);
			}
			else
			{
				allocatedAddr = sceNetMallocInternal(size);
			}

			return allocatedAddr;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC80181A2, version = 150, checkInsideInterrupt = true) public int sceNetGetDropRate(@CanBeNull pspsharp.HLE.TPointer32 dropRateAddr, @CanBeNull pspsharp.HLE.TPointer32 dropDurationAddr)
		[HLEFunction(nid : 0xC80181A2, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetGetDropRate(TPointer32 dropRateAddr, TPointer32 dropDurationAddr)
		{
			return Modules.sceWlanModule.sceWlanGetDropRate(dropRateAddr, dropDurationAddr);
		}

		[HLEFunction(nid : 0xFD8585E1, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNetSetDropRate(int dropRate, int dropDuration)
		{
			return Modules.sceWlanModule.sceWlanSetDropRate(dropRate, dropDuration);
		}

		[HLEFunction(nid : 0x15CFE3C0, version : 150)]
		public virtual int sceNetMallocInternal(int size)
		{
			SysMemInfo info = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.USER_PARTITION_ID, "sceNetMallocInternal", SysMemUserForUser.PSP_SMEM_Low, size, 0);

			if (info == null)
			{
				return 0;
			}

			allocatedMemory[info.addr] = info;

			return info.addr;
		}

		[HLEFunction(nid : 0x76BAD213, version : 150)]
		public virtual int sceNetFreeInternal(int memory)
		{
			SysMemInfo info = allocatedMemory.Remove(memory);
			if (info != null)
			{
				Modules.SysMemUserForUserModule.free(info);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0542835F, version = 150) public int sceNetIfhandle_driver_0542835F(int unknownCallback1, int unknownCallback2, int unknownCallback3)
		[HLEFunction(nid : 0x0542835F, version : 150)]
		public virtual int sceNetIfhandle_driver_0542835F(int unknownCallback1, int unknownCallback2, int unknownCallback3)
		{
			this.unknownCallback1 = unknownCallback1;
			this.unknownCallback2 = unknownCallback2;
			this.unknownCallback3 = unknownCallback3;
			callbacksDefined = true;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x773FC77C, version = 150) public int sceNetIfhandle_driver_773FC77C()
		[HLEFunction(nid : 0x773FC77C, version : 150)]
		public virtual int sceNetIfhandle_driver_773FC77C()
		{
			// Has no parameters
			unknownCallback1 = 0;
			unknownCallback2 = 0;
			unknownCallback3 = 0;
			callbacksDefined = false;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9CBA24D4, version = 150) public int sceNetIfhandle_driver_9CBA24D4(pspsharp.HLE.PspString interfaceName)
		[HLEFunction(nid : 0x9CBA24D4, version : 150)]
		public virtual int sceNetIfhandle_driver_9CBA24D4(PspString interfaceName)
		{
			int handleAddr = 0;
			for (int i = 0; i < handles.Length; i++)
			{
				if (handles[i].NotNull)
				{
					SceNetIfHandle handle = new SceNetIfHandle();
					handle.read(handles[i]);
					if (interfaceName.Equals(handle.handleInternal.interfaceName))
					{
						handleAddr = handles[i].Address;
						break;
					}
				}
			}

			return handleAddr;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC5623112, version = 150) public int sceNetIfhandle_driver_C5623112(@CheckArgument(value="checkHandleAddr") @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=44, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer handleAddr, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=6, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer8 macAddress)
		[HLEFunction(nid : 0xC5623112, version : 150)]
		public virtual int sceNetIfhandle_driver_C5623112(TPointer handleAddr, TPointer8 macAddress)
		{
			SceNetIfHandle handle = new SceNetIfHandle();
			handle.read(handleAddr);
			handle.handleInternal.macAddress.write(macAddress);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x16042084, version = 150) public int sceNetCreateIfhandleEther(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=44, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer handleAddr)
		[HLEFunction(nid : 0x16042084, version : 150)]
		public virtual int sceNetCreateIfhandleEther(TPointer handleAddr)
		{
			return hleNetCreateIfhandleEther(handleAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAE81C0CB, version = 150) public int sceNetAttachIfhandleEther(@CheckArgument("checkHandleAddr") pspsharp.HLE.TPointer handleAddr, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=6, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer8 macAddress, pspsharp.HLE.PspString interfaceName)
		[HLEFunction(nid : 0xAE81C0CB, version : 150)]
		public virtual int sceNetAttachIfhandleEther(TPointer handleAddr, TPointer8 macAddress, PspString interfaceName)
		{
			pspNetMacAddress netMacAddress = new pspNetMacAddress();
			netMacAddress.read(macAddress);

			return hleNetAttachIfhandleEther(handleAddr, netMacAddress, interfaceName.String);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x07505747, version = 150) public int sceNetIfhandle_07505747(int unknown)
		[HLEFunction(nid : 0x07505747, version : 150)]
		public virtual int sceNetIfhandle_07505747(int unknown)
		{
			return 0; // Current thread delay in ms (used only for some gameIds)
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0B258B5E, version = 150) public int sceNetIfhandle_0B258B5E()
		[HLEFunction(nid : 0x0B258B5E, version : 150)]
		public virtual int sceNetIfhandle_0B258B5E()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0C391E9F, version = 150) public int sceNetIfhandle_0C391E9F(int partitionId, int memorySize)
		[HLEFunction(nid : 0x0C391E9F, version : 150)]
		public virtual int sceNetIfhandle_0C391E9F(int partitionId, int memorySize)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0FB8AE0D, version = 150) public void sceNetIfhandle_0FB8AE0D()
		[HLEFunction(nid : 0x0FB8AE0D, version : 150)]
		public virtual void sceNetIfhandle_0FB8AE0D()
		{
			// Has no parameters
			unknownValue1 = 0;
			unknownValue2 = 0;
			unknownValue3 = 0;
			unknownValue4 = 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x29ED84C5, version = 150) public int sceNetIfhandle_29ED84C5()
		[HLEFunction(nid : 0x29ED84C5, version : 150)]
		public virtual int sceNetIfhandle_29ED84C5()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x35FAB6A2, version = 150) public void sceNetIfhandle_35FAB6A2()
		[HLEFunction(nid : 0x35FAB6A2, version : 150)]
		public virtual void sceNetIfhandle_35FAB6A2()
		{
			// Has no parameters
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5FB31C72, version = 150) public int sceNetIfhandle_5FB31C72(@CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknownValue1Addr, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknownValue2Addr, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknownValue3Addr, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknownValue4Addr)
		[HLEFunction(nid : 0x5FB31C72, version : 150)]
		public virtual int sceNetIfhandle_5FB31C72(TPointer32 unknownValue1Addr, TPointer32 unknownValue2Addr, TPointer32 unknownValue3Addr, TPointer32 unknownValue4Addr)
		{
			unknownValue1Addr.setValue(unknownValue1);
			unknownValue2Addr.setValue(unknownValue2);
			unknownValue3Addr.setValue(unknownValue3);
			unknownValue4Addr.setValue(unknownValue4);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x62B20015, version = 150) public int sceNetIfhandle_62B20015(int unknownValue1, int unknownValue2, int unknownValue3, int unknownValue4)
		[HLEFunction(nid : 0x62B20015, version : 150)]
		public virtual int sceNetIfhandle_62B20015(int unknownValue1, int unknownValue2, int unknownValue3, int unknownValue4)
		{
			this.unknownValue1 = unknownValue1;
			this.unknownValue2 = unknownValue2;
			this.unknownValue3 = unknownValue3;
			this.unknownValue4 = unknownValue4;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x955F2924, version = 150) public int sceNetIfhandle_955F2924()
		[HLEFunction(nid : 0x955F2924, version : 150)]
		public virtual int sceNetIfhandle_955F2924()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE9BF5332, version = 150) public int sceNetIfhandle_E9BF5332()
		[HLEFunction(nid : 0xE9BF5332, version : 150)]
		public virtual int sceNetIfhandle_E9BF5332()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0296C7D6, version = 150) public void sceNetIfhandleIfIoctl(@CanBeNull @CheckArgument("checkHandleInternalAddr") @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=320, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer handleInternalAddr, int cmd, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=32, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknown)
		[HLEFunction(nid : 0x0296C7D6, version : 150)]
		public virtual void sceNetIfhandleIfIoctl(TPointer handleInternalAddr, int cmd, TPointer unknown)
		{
			if (log.DebugEnabled)
			{
				string interfaceName = unknown.getStringNZ(16);
				int flags = unknown.getValue16(16);
				log.debug(string.Format("sceNetIfhandleIfIoctl interfaceName='{0}' flags=0x{1:X}", interfaceName, flags));
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1560F143, version = 150) public int sceNetMCopyback()
		[HLEFunction(nid : 0x1560F143, version : 150)]
		public virtual int sceNetMCopyback()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x16246B99, version = 150) public int sceNetIfPrepend()
		[HLEFunction(nid : 0x16246B99, version : 150)]
		public virtual int sceNetIfPrepend()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2162EE67, version = 150) public int sceNetIfhandlePollSema()
		[HLEFunction(nid : 0x2162EE67, version : 150)]
		public virtual int sceNetIfhandlePollSema()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x263767F6, version = 150) public int sceNetFlagIfEvent(@CanBeNull pspsharp.HLE.TPointer handleAddr, int unknown1, int unknown2)
		[HLEFunction(nid : 0x263767F6, version : 150)]
		public virtual int sceNetFlagIfEvent(TPointer handleAddr, int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x30602CE9, version = 150) public int sceNetIfhandleSignalSema()
		[HLEFunction(nid : 0x30602CE9, version : 150)]
		public virtual int sceNetIfhandleSignalSema()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x30F69334, version = 150) public int sceNetIfhandleInit(int eventFlagId)
		[HLEFunction(nid : 0x30F69334, version : 150)]
		public virtual int sceNetIfhandleInit(int eventFlagId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3E8DD3F8, version = 150) public int sceNetMCat()
		[HLEFunction(nid : 0x3E8DD3F8, version : 150)]
		public virtual int sceNetMCat()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x456E3146, version = 150) public int sceNetMCopym()
		[HLEFunction(nid : 0x456E3146, version : 150)]
		public virtual int sceNetMCopym()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x49EDBB18, version = 150) public int sceNetMPullup()
		[HLEFunction(nid : 0x49EDBB18, version : 150)]
		public virtual int sceNetMPullup()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4C2886CB, version = 150) public int sceNetGetMallocStatInternal()
		[HLEFunction(nid : 0x4C2886CB, version : 150)]
		public virtual int sceNetGetMallocStatInternal()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4CF15C43, version = 150) public int sceNetMGethdr()
		[HLEFunction(nid : 0x4CF15C43, version : 150)]
		public virtual int sceNetMGethdr()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4FB43BCE, version = 150) public int sceNetIfhandleGetDetachEther()
		[HLEFunction(nid : 0x4FB43BCE, version : 150)]
		public virtual int sceNetIfhandleGetDetachEther()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x54D1AEA1, version = 150) public int sceNetDetachIfhandleEther()
		[HLEFunction(nid : 0x54D1AEA1, version : 150)]
		public virtual int sceNetDetachIfhandleEther()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x59F0D619, version = 150) public int sceNetMGetclr()
		[HLEFunction(nid : 0x59F0D619, version : 150)]
		public virtual int sceNetMGetclr()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6AB53C27, version = 150) public int sceNetMDup()
		[HLEFunction(nid : 0x6AB53C27, version : 150)]
		public virtual int sceNetMDup()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8FCB05A1, version = 150) public int sceNetIfhandleIfUp(@CheckArgument("checkHandleInternalAddr") @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=320, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer handleInternalAddr)
		[HLEFunction(nid : 0x8FCB05A1, version : 150)]
		public virtual int sceNetIfhandleIfUp(TPointer handleInternalAddr)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9A6261EC, version = 150) public int sceNetMCopydata(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=76, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer messageAddr, int dataOffset, int length, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.previousParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer destinationAddr)
		[HLEFunction(nid : 0x9A6261EC, version : 150)]
		public virtual int sceNetMCopydata(TPointer messageAddr, int dataOffset, int length, TPointer destinationAddr)
		{
			if (destinationAddr.NotNull)
			{
				SceNetIfMessage message = new SceNetIfMessage();
				while (messageAddr.NotNull)
				{
					message.read(messageAddr);

					if (dataOffset < message.dataLength)
					{
						break;
					}
					dataOffset -= message.dataLength;
					messageAddr.Address = message.nextDataAddr;
				}

				while (length > 0 && messageAddr.NotNull)
				{
					message.read(messageAddr);
					int copyLength = System.Math.Min(length, message.dataLength - dataOffset);
					destinationAddr.memcpy(message.dataAddr + dataOffset, copyLength);
					length -= copyLength;
					destinationAddr.add(copyLength);
					dataOffset = 0;
				}
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA493AA5F, version = 150) public int sceNetMGet(int unknown1, int unknown2)
		[HLEFunction(nid : 0xA493AA5F, version : 150)]
		public virtual int sceNetMGet(int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB1F5BB87, version = 150) public void sceNetIfhandleIfStart(@CanBeNull pspsharp.HLE.TPointer handleInternalAddr, @CanBeNull pspsharp.HLE.kernel.types.SceNetIfMessage messageToBeSent)
		[HLEFunction(nid : 0xB1F5BB87, version : 150)]
		public virtual void sceNetIfhandleIfStart(TPointer handleInternalAddr, SceNetIfMessage messageToBeSent)
		{
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB8188F96, version = 150) public int sceNetIfhandleGetAttachEther(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 handleInternalAddrAddr, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown)
		[HLEFunction(nid : 0xB8188F96, version : 150)]
		public virtual int sceNetIfhandleGetAttachEther(TPointer32 handleInternalAddrAddr, TPointer32 unknown)
		{
			// returns the address of handleInternal into handleInternalAddrAddr
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB9096E48, version = 150) public int sceNetIfhandleTerm()
		[HLEFunction(nid : 0xB9096E48, version : 150)]
		public virtual int sceNetIfhandleTerm()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBFF3CEA5, version = 150) public void sceNetMAdj(@CanBeNull pspsharp.HLE.TPointer messageAddr, int sizeAdj)
		[HLEFunction(nid : 0xBFF3CEA5, version : 150)]
		public virtual void sceNetMAdj(TPointer messageAddr, int sizeAdj)
		{
			if (messageAddr.Null)
			{
				return;
			}

			SceNetIfMessage message = new SceNetIfMessage();

			if (sizeAdj < 0)
			{
				sizeAdj = -sizeAdj;
				int totalSize = 0;
				int currentMessageAddr = messageAddr.Address;
				do
				{
					message.read(messageAddr.Memory, currentMessageAddr);
					totalSize += message.dataLength;
					currentMessageAddr = message.nextDataAddr;
				} while (currentMessageAddr != 0);

				if (message.dataLength < sizeAdj)
				{
					totalSize -= sizeAdj;
					message.read(messageAddr);
					totalSize = System.Math.Max(totalSize - sizeAdj, 0);
					if ((message.unknown18 & 2) != 0)
					{
						message.unknown24 = totalSize;
						message.write(messageAddr);
					}

					currentMessageAddr = messageAddr.Address;
					while (currentMessageAddr != 0)
					{
						message.read(messageAddr.Memory, currentMessageAddr);
						if (message.dataLength < totalSize)
						{
							currentMessageAddr = message.nextDataAddr;
							totalSize -= message.dataLength;
						}
						else
						{
							message.dataLength = totalSize;
							message.write(messageAddr.Memory, currentMessageAddr);
							break;
						}
					}

					currentMessageAddr = message.nextDataAddr;
					while (currentMessageAddr != 0)
					{
						message.read(messageAddr.Memory, currentMessageAddr);
						message.dataLength = 0;
						message.write(messageAddr.Memory, currentMessageAddr);
						currentMessageAddr = message.nextDataAddr;
					}
				}
				else
				{
					message.read(messageAddr);
					if ((message.unknown18 & 2) != 0)
					{
						message.unknown24 -= sizeAdj;
						message.write(messageAddr);
					}
				}
			}
			else
			{
				int totalSizeAdj = sizeAdj;
				int currentMessageAddr = messageAddr.Address;
				do
				{
					message.read(messageAddr.Memory, currentMessageAddr);
					if (sizeAdj < message.dataLength)
					{
						message.dataLength -= sizeAdj;
						message.dataAddr += sizeAdj;
						message.write(messageAddr);
						sizeAdj = 0;
					}
					else
					{
						sizeAdj -= message.dataLength;
						message.dataLength = 0;
						message.write(messageAddr);
						currentMessageAddr = message.nextDataAddr;
					}
				} while (messageAddr.NotNull && sizeAdj > 0);

				message.read(messageAddr);
				if ((message.unknown18 & 2) != 0)
				{
					message.unknown24 -= totalSizeAdj - sizeAdj;
					message.write(messageAddr);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC28F6FF2, version = 150) public int sceNetIfEnqueue()
		[HLEFunction(nid : 0xC28F6FF2, version : 150)]
		public virtual int sceNetIfEnqueue()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC3325FDC, version = 150) public int sceNetMPrepend()
		[HLEFunction(nid : 0xC3325FDC, version : 150)]
		public virtual int sceNetMPrepend()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC6D14282, version = 150) public int sceNetIfhandle_driver_C6D14282(pspsharp.HLE.TPointer handleAddr, int callbackArg4)
		[HLEFunction(nid : 0xC6D14282, version : 150)]
		public virtual int sceNetIfhandle_driver_C6D14282(TPointer handleAddr, int callbackArg4)
		{
			SceNetIfHandle handle = new SceNetIfHandle();
			handle.read(handleAddr);

			handle.callbackArg4 = callbackArg4;
			handle.write(handleAddr);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC9344A59, version = 150) public int sceNetDestroyIfhandleEther()
		[HLEFunction(nid : 0xC9344A59, version : 150)]
		public virtual int sceNetDestroyIfhandleEther()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD5AD6DEA, version = 150) public int sceNetIfhandle_driver_D5AD6DEA(@CanBeNull pspsharp.HLE.TPointer handleAddr)
		[HLEFunction(nid : 0xD5AD6DEA, version : 150)]
		public virtual int sceNetIfhandle_driver_D5AD6DEA(TPointer handleAddr)
		{
			if (handleAddr.Null)
			{
				return 0;
			}
			SceNetIfHandle handle = new SceNetIfHandle();
			handle.read(handleAddr);

			return handle.callbackArg4;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD5DA7B3C, version = 150) public int sceNetIfhandleWaitSema()
		[HLEFunction(nid : 0xD5DA7B3C, version : 150)]
		public virtual int sceNetIfhandleWaitSema()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE2F4F1C9, version = 150) public int sceNetIfDequeue()
		[HLEFunction(nid : 0xE2F4F1C9, version : 150)]
		public virtual int sceNetIfDequeue()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE440A7D8, version = 150) public int sceNetIfhandleIfDequeue()
		[HLEFunction(nid : 0xE440A7D8, version : 150)]
		public virtual int sceNetIfhandleIfDequeue()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE80F00A4, version = 150) public int sceNetMPulldown()
		[HLEFunction(nid : 0xE80F00A4, version : 150)]
		public virtual int sceNetMPulldown()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEAD3A759, version = 150) public int sceNetIfhandleIfDown()
		[HLEFunction(nid : 0xEAD3A759, version : 150)]
		public virtual int sceNetIfhandleIfDown()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF56FAC82, version = 150) public int sceNetMFreem(@CanBeNull pspsharp.HLE.TPointer messageAddr)
		[HLEFunction(nid : 0xF56FAC82, version : 150)]
		public virtual int sceNetMFreem(TPointer messageAddr)
		{
			SceNetIfMessage message = new SceNetIfMessage();
			while (messageAddr.NotNull)
			{
				message.read(messageAddr);
				int nextMessage = message.nextDataAddr;
				sceNetFreeInternal(messageAddr.Address);
				messageAddr = new TPointer(messageAddr.Memory, nextMessage);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF8825DC4, version = 150) public int sceNetMFree()
		[HLEFunction(nid : 0xF8825DC4, version : 150)]
		public virtual int sceNetMFree()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF94BAF52, version = 150) public int sceNetSendIfEvent(pspsharp.HLE.TPointer handleAddr)
		[HLEFunction(nid : 0xF94BAF52, version : 150)]
		public virtual int sceNetSendIfEvent(TPointer handleAddr)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9173FD47, version = 150) public int sceNetIfhandle_9173FD47()
		[HLEFunction(nid : 0x9173FD47, version : 150)]
		public virtual int sceNetIfhandle_9173FD47()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB40A882F, version = 150) public int sceNetIfhandle_B40A882F()
		[HLEFunction(nid : 0xB40A882F, version : 150)]
		public virtual int sceNetIfhandle_B40A882F()
		{
			return 0;
		}
	}
}