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
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Managers = pspsharp.HLE.kernel.Managers;
	using SceKernelTls = pspsharp.HLE.kernel.types.SceKernelTls;

	using Logger = org.apache.log4j.Logger;

	public class Kernel_Library : HLEModule
	{
		public static Logger log = Modules.getLogger("Kernel_Library");

		private readonly int flagInterruptsEnabled = 1;
		private readonly int flagInterruptsDisabled = 0;

		/// <summary>
		/// Suspend all interrupts.
		/// 
		/// @returns The current state of the interrupt controller, to be used with ::sceKernelCpuResumeIntr().
		/// </summary>
		[HLEFunction(nid : 0x092968F4, version : 150)]
		public virtual int sceKernelCpuSuspendIntr(Processor processor)
		{
			int returnValue;
			if (processor.InterruptsEnabled)
			{
				returnValue = flagInterruptsEnabled;
				processor.disableInterrupts();
			}
			else
			{
				returnValue = flagInterruptsDisabled;
			}

			return returnValue;
		}

		protected internal virtual void hleKernelCpuResumeIntr(Processor processor, int flagInterrupts)
		{
			if (flagInterrupts == flagInterruptsEnabled)
			{
				processor.enableInterrupts();
			}
			else if (flagInterrupts == flagInterruptsDisabled)
			{
				processor.disableInterrupts();
			}
			else
			{
				log.warn(string.Format("hleKernelCpuResumeIntr unknown flag value 0x{0:X}", flagInterrupts));
			}
		}

		/// <summary>
		/// Resume all interrupts.
		/// </summary>
		/// <param name="flags"> - The value returned from ::sceKernelCpuSuspendIntr(). </param>
		[HLEFunction(nid : 0x5F10D406, version : 150)]
		public virtual void sceKernelCpuResumeIntr(Processor processor, int flagInterrupts)
		{
			hleKernelCpuResumeIntr(processor, flagInterrupts);
		}

		/// <summary>
		/// Resume all interrupts (using sync instructions).
		/// </summary>
		/// <param name="flags"> - The value returned from ::sceKernelCpuSuspendIntr() </param>
		[HLEFunction(nid : 0x3B84732D, version : 150)]
		public virtual void sceKernelCpuResumeIntrWithSync(Processor processor, int flagInterrupts)
		{
			hleKernelCpuResumeIntr(processor, flagInterrupts);
		}

		/// <summary>
		/// Determine if interrupts are suspended or active, based on the given flags.
		/// </summary>
		/// <param name="flags"> - The value returned from ::sceKernelCpuSuspendIntr().
		/// 
		/// @returns 1 if flags indicate that interrupts were not suspended, 0 otherwise. </param>
		[HLEFunction(nid : 0x47A0B729, version : 150)]
		public virtual bool sceKernelIsCpuIntrSuspended(int flagInterrupts)
		{
			return flagInterrupts == flagInterruptsDisabled;
		}

		/// <summary>
		/// Determine if interrupts are enabled or disabled.
		/// 
		/// @returns 1 if interrupts are currently enabled.
		/// </summary>
		[HLEFunction(nid : 0xB55249D2, version : 150)]
		public virtual bool sceKernelIsCpuIntrEnable(Processor processor)
		{
			return processor.InterruptsEnabled;
		}

		[HLEFunction(nid : 0x15B6446B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelUnlockLwMutex(TPointer workAreaAddr, int count)
		{
			return Managers.lwmutex.sceKernelUnlockLwMutex(workAreaAddr, count);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1FC64E09, version = 380, checkInsideInterrupt = true) public int sceKernelLockLwMutexCB(pspsharp.HLE.TPointer workAreaAddr, int count, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x1FC64E09, version : 380, checkInsideInterrupt : true)]
		public virtual int sceKernelLockLwMutexCB(TPointer workAreaAddr, int count, TPointer32 timeoutAddr)
		{
			return Managers.lwmutex.sceKernelLockLwMutexCB(workAreaAddr, count, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xBEA46419, version = 150, checkInsideInterrupt = true) public int sceKernelLockLwMutex(pspsharp.HLE.TPointer workAreaAddr, int count, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0xBEA46419, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelLockLwMutex(TPointer workAreaAddr, int count, TPointer32 timeoutAddr)
		{
			return Managers.lwmutex.sceKernelLockLwMutex(workAreaAddr, count, timeoutAddr);
		}

		[HLEFunction(nid : 0xC1734599, version : 380)]
		public virtual int sceKernelReferLwMutexStatus(TPointer workAreaAddr, TPointer addr)
		{
			return Managers.lwmutex.sceKernelReferLwMutexStatus(workAreaAddr, addr);
		}

		[HLEFunction(nid : 0xDC692EE3, version : 380, checkInsideInterrupt : true)]
		public virtual int sceKernelTryLockLwMutex(TPointer workAreaAddr, int count)
		{
			return Managers.lwmutex.sceKernelTryLockLwMutex(workAreaAddr, count);
		}

		[HLEFunction(nid : 0x37431849, version : 380, checkInsideInterrupt : true)]
		public virtual int sceKernelTryLockLwMutex_600(TPointer workAreaAddr, int count)
		{
			return Managers.lwmutex.sceKernelTryLockLwMutex(workAreaAddr, count);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLELogging(level="trace") @HLEFunction(nid = 0x1839852A, version = 150) public int sceKernelMemcpy(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextNextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer dst, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer src, int length)
		[HLELogging(level:"trace"), HLEFunction(nid : 0x1839852A, version : 150)]
		public virtual int sceKernelMemcpy(TPointer dst, TPointer src, int length)
		{
			if (dst.Address != src.Address)
			{
				dst.Memory.memcpyWithVideoCheck(dst.Address, src.Address, length);
			}

			return dst.Address;
		}

		[HLEFunction(nid : 0xA089ECA4, version : 150)]
		public virtual int sceKernelMemset(TPointer destAddr, int data, int size)
		{
			destAddr.memset((sbyte) data, size);

			return 0;
		}

		[HLEFunction(nid : 0xFA835CDE, version : 620)]
		public virtual int sceKernel_FA835CDE(int uid)
		{
			SceKernelTls tls = Modules.ThreadManForUserModule.getKernelTls(uid);
			if (tls == null)
			{
				return 0;
			}

			int addr = tls.TlsAddress;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernel_FA835CDE returning 0x{0:X8}", addr));
			}

			return addr;
		}
	}
}