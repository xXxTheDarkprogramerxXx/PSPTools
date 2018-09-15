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
namespace pspsharp.HLE.kernel.managers
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_ILLEGAL_ATTR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_ILLEGAL_MEMBLOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_ILLEGAL_MEMSIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NOT_FOUND_VPOOL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NO_MEMORY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_TIMEOUT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_CANCELLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_CAN_NOT_WAIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_DELETE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_STATUS_RELEASED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_READY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_VPL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_High;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_Low;


	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelVplInfo = pspsharp.HLE.kernel.types.SceKernelVplInfo;
	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;

	//using Logger = org.apache.log4j.Logger;

	public class VplManager
	{
		//public static Logger log = Modules.getLogger("ThreadManForUser");

		private Dictionary<int, SceKernelVplInfo> vplMap;
		private VplWaitStateChecker vplWaitStateChecker;

		public const int PSP_VPL_ATTR_FIFO = 0;
		public const int PSP_VPL_ATTR_PRIORITY = 0x100;
		private const int PSP_VPL_ATTR_PASS = 0x200; // Allow threads that want to allocate small memory blocks to bypass the waiting queue (less memory goes first).
		public const int PSP_VPL_ATTR_ADDR_HIGH = 0x4000; // Create the VPL in high memory.
		//public final static int PSP_VPL_ATTR_EXT =        0x8000;   // Automatically extend the VPL's memory area (when allocating a block from the VPL and the remaining size is too small, this flag tells the VPL to automatically attempt to extend it's memory area).
		public static readonly int PSP_VPL_ATTR_MASK = PSP_VPL_ATTR_ADDR_HIGH | PSP_VPL_ATTR_PASS | PSP_VPL_ATTR_PRIORITY | 0xFF; // Anything outside this mask is an illegal attr.

		public virtual void reset()
		{
			vplMap = new Dictionary<int, SceKernelVplInfo>();
			vplWaitStateChecker = new VplWaitStateChecker(this);
		}

		private bool removeWaitingThread(SceKernelThreadInfo thread)
		{
			SceKernelVplInfo fpl = vplMap[thread.wait.Vpl_id];
			if (fpl == null)
			{
				return false;
			}

			fpl.threadWaitingList.removeWaitingThread(thread);

			return true;
		}

		public virtual void onThreadWaitTimeout(SceKernelThreadInfo thread)
		{
			// Untrack
			if (removeWaitingThread(thread))
			{
				// Return WAIT_TIMEOUT
				thread.cpuContext._v0 = ERROR_KERNEL_WAIT_TIMEOUT;
			}
			else
			{
				Console.WriteLine("VPL deleted while we were waiting for it! (timeout expired)");
				// Return WAIT_DELETE
				thread.cpuContext._v0 = ERROR_KERNEL_WAIT_DELETE;
			}
		}

		public virtual void onThreadWaitReleased(SceKernelThreadInfo thread)
		{
			// Untrack
			if (removeWaitingThread(thread))
			{
				// Return ERROR_WAIT_STATUS_RELEASED
				thread.cpuContext._v0 = ERROR_KERNEL_WAIT_STATUS_RELEASED;
			}
			else
			{
				Console.WriteLine("EventFlag deleted while we were waiting for it!");
				// Return WAIT_DELETE
				thread.cpuContext._v0 = ERROR_KERNEL_WAIT_DELETE;
			}
		}

		public virtual void onThreadDeleted(SceKernelThreadInfo thread)
		{
			if (thread.isWaitingForType(PSP_WAIT_VPL))
			{
				removeWaitingThread(thread);
			}
		}

		private void onVplDeletedCancelled(int vid, int result)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			for (IEnumerator<SceKernelThreadInfo> it = threadMan.GetEnumerator(); it.MoveNext();)
			{
				SceKernelThreadInfo thread = it.Current;
				if (thread.isWaitingFor(PSP_WAIT_VPL, vid))
				{
					thread.cpuContext._v0 = result;
					threadMan.hleChangeThreadState(thread, PSP_THREAD_READY);
					reschedule = true;
				}
			}
			// Reschedule only if threads waked up.
			if (reschedule)
			{
				threadMan.hleRescheduleCurrentThread();
			}
		}

		private void onVplDeleted(int vid)
		{
			onVplDeletedCancelled(vid, ERROR_KERNEL_WAIT_DELETE);
		}

		private void onVplCancelled(int vid)
		{
			onVplDeletedCancelled(vid, ERROR_KERNEL_WAIT_CANCELLED);
		}

		private void onVplFree(SceKernelVplInfo info)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			SceKernelThreadInfo checkedThread = null;
			while (info.freeSize > 0)
			{
				SceKernelThreadInfo thread = info.threadWaitingList.getNextWaitingThread(checkedThread);
				if (thread == null)
				{
					break;
				}
				int addr = tryAllocateVpl(info, thread.wait.Vpl_size);
				if (addr != 0)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("onVplFree waking thread {0}", thread));
					}
					// Return allocated address
					thread.wait.Vpl_dataAddr.setValue(addr);
					info.threadWaitingList.removeWaitingThread(thread);
					thread.cpuContext._v0 = 0;
					threadMan.hleChangeThreadState(thread, PSP_THREAD_READY);
					reschedule = true;
				}
				else
				{
					checkedThread = thread;
				}
			}

			// Reschedule only if threads waked up.
			if (reschedule)
			{
				threadMan.hleRescheduleCurrentThread();
			}
		}

		/// <returns> the address of the allocated block or 0 if failed. </returns>
		public virtual int tryAllocateVpl(SceKernelVplInfo info, int size)
		{
			return info.alloc(size);
		}

		public virtual SceKernelVplInfo getVplInfoByName(string name)
		{
			foreach (SceKernelVplInfo info in vplMap.Values)
			{
				if (name.Equals(info.name))
				{
					return info;
				}
			}

			return null;
		}

		public virtual int checkVplID(int uid)
		{
			SceUidManager.checkUidPurpose(uid, "ThreadMan-Vpl", true);
			if (!vplMap.ContainsKey(uid))
			{
				Console.WriteLine(string.Format("checkVplID unknown uid=0x{0:X}", uid));
				throw new SceKernelErrorException(ERROR_KERNEL_NOT_FOUND_VPOOL);
			}

			return uid;
		}

		public virtual int sceKernelCreateVpl(PspString name, int partitionid, int attr, int size, TPointer option)
		{
			if (name.Null)
			{
				// PSP is returning this error is case of a NULL name
				return SceKernelErrors.ERROR_KERNEL_ERROR;
			}

			if (option.NotNull)
			{
				int optionSize = option.getValue32();
				Console.WriteLine(string.Format("sceKernelCreateVpl option at {0}, size={1:D}", option, optionSize));
			}

			int memType = PSP_SMEM_Low;
			if ((attr & PSP_VPL_ATTR_ADDR_HIGH) == PSP_VPL_ATTR_ADDR_HIGH)
			{
				memType = PSP_SMEM_High;
			}

			if ((attr & ~PSP_VPL_ATTR_MASK) != 0)
			{
				Console.WriteLine("sceKernelCreateVpl bad attr value 0x" + attr.ToString("x"));
				return ERROR_KERNEL_ILLEGAL_ATTR;
			}
			if (size == 0)
			{
				return ERROR_KERNEL_ILLEGAL_MEMSIZE;
			}
			if (size < 0)
			{
				return ERROR_KERNEL_NO_MEMORY;
			}

			SceKernelVplInfo info = SceKernelVplInfo.tryCreateVpl(name.String, partitionid, attr, size, memType);
			if (info == null)
			{
				return ERROR_KERNEL_NO_MEMORY;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelCreateVpl returning {0}", info));
			}
			vplMap[info.uid] = info;

			return info.uid;
		}

		public virtual int sceKernelDeleteVpl(int uid)
		{
			SceKernelVplInfo info = vplMap.Remove(uid);
			if (info.freeSize < info.poolSize)
			{
				Console.WriteLine(string.Format("sceKernelDeleteVpl approx 0x{0:X} unfreed bytes allocated", info.poolSize - info.freeSize));
			}
			info.delete();
			onVplDeleted(uid);

			return 0;
		}

		private int hleKernelAllocateVpl(int uid, int size, TPointer32 dataAddr, TPointer32 timeoutAddr, bool wait, bool doCallbacks)
		{
			SceKernelVplInfo vpl = vplMap[uid];
			if (size <= 0 || size > vpl.poolSize)
			{
				return ERROR_KERNEL_ILLEGAL_MEMSIZE;
			}

			int addr = tryAllocateVpl(vpl, size);
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			if (addr == 0)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleKernelAllocateVpl {0} fast check failed", vpl));
				}
				if (!wait)
				{
					return ERROR_KERNEL_WAIT_CAN_NOT_WAIT;
				}
				// Go to wait state
				SceKernelThreadInfo currentThread = threadMan.CurrentThread;
				vpl.threadWaitingList.addWaitingThread(currentThread);
				// Wait on a specific fpl
				currentThread.wait.Vpl_id = uid;
				currentThread.wait.Vpl_size = size;
				currentThread.wait.Vpl_dataAddr = dataAddr;
				threadMan.hleKernelThreadEnterWaitState(PSP_WAIT_VPL, uid, vplWaitStateChecker, timeoutAddr.Address, doCallbacks);
			}
			else
			{
				// Success, do not reschedule the current thread.
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleKernelAllocateVpl {0} fast check succeeded, allocated addr=0x{1:X8}", vpl, addr));
				}
				dataAddr.setValue(addr);
			}

			return 0;
		}

		public virtual int sceKernelAllocateVpl(int uid, int size, TPointer32 dataAddr, TPointer32 timeoutAddr)
		{
			return hleKernelAllocateVpl(uid, size, dataAddr, timeoutAddr, true, false);
		}

		public virtual int sceKernelAllocateVplCB(int uid, int size, TPointer32 dataAddr, TPointer32 timeoutAddr)
		{
			return hleKernelAllocateVpl(uid, size, dataAddr, timeoutAddr, true, true);
		}

		public virtual int sceKernelTryAllocateVpl(int uid, int size, TPointer32 dataAddr)
		{
			return hleKernelAllocateVpl(uid, size, dataAddr, TPointer32.NULL, false, false);
		}

		public virtual int sceKernelFreeVpl(int uid, TPointer dataAddr)
		{
			SceKernelVplInfo info = vplMap[uid];
			if (!info.free(dataAddr.Address))
			{
				return ERROR_KERNEL_ILLEGAL_MEMBLOCK;
			}

			onVplFree(info);

			return 0;
		}

		public virtual int sceKernelCancelVpl(int uid, TPointer32 numWaitThreadAddr)
		{
			SceKernelVplInfo info = vplMap[uid];
			numWaitThreadAddr.setValue(info.NumWaitingThreads);
			info.threadWaitingList.removeAllWaitingThreads();
			onVplCancelled(uid);

			return 0;
		}

		public virtual int sceKernelReferVplStatus(int uid, TPointer infoAddr)
		{
			SceKernelVplInfo info = vplMap[uid];
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelReferVplStatus returning {0}", info));
			}
			info.write(infoAddr);

			return 0;
		}

		private class VplWaitStateChecker : IWaitStateChecker
		{
			private readonly VplManager outerInstance;

			public VplWaitStateChecker(VplManager outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				// Check if the thread has to continue its wait state or if the vpl
				// has been allocated during the callback execution.
				SceKernelVplInfo vpl = outerInstance.vplMap[wait.Vpl_id];
				if (vpl == null)
				{
					thread.cpuContext._v0 = ERROR_KERNEL_NOT_FOUND_VPOOL;
					return false;
				}

				// Check vpl.
				int addr = outerInstance.tryAllocateVpl(vpl, wait.Vpl_size);
				if (addr != 0)
				{
					// Return the allocated address
					wait.Vpl_dataAddr.setValue(addr);
					vpl.threadWaitingList.removeWaitingThread(thread);
					thread.cpuContext._v0 = 0;
					return false;
				}

				return true;
			}
		}
		public static readonly VplManager singleton = new VplManager();

		private VplManager()
		{
		}
	}
}