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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_ILLEGAL_MEMSIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_ILLEGAL_MEMBLOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NOT_FOUND_FPOOL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NO_MEMORY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_TIMEOUT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_DELETE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_STATUS_RELEASED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_CANCELLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_CAN_NOT_WAIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_READY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_FPL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_High;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_Low;


	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelFplInfo = pspsharp.HLE.kernel.types.SceKernelFplInfo;
	using SceKernelFplOptParam = pspsharp.HLE.kernel.types.SceKernelFplOptParam;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class FplManager
	{

		protected internal static Logger log = Modules.getLogger("ThreadManForUser");

		private Dictionary<int, SceKernelFplInfo> fplMap;
		private FplWaitStateChecker fplWaitStateChecker;

		public const int PSP_FPL_ATTR_FIFO = 0;
		public const int PSP_FPL_ATTR_PRIORITY = 0x100;
		private const int PSP_FPL_ATTR_MASK = 0x41FF; // Anything outside this mask is an illegal attr.
		private const int PSP_FPL_ATTR_ADDR_HIGH = 0x4000; // Create the fpl in high memory.

		public virtual void reset()
		{
			fplMap = new Dictionary<int, SceKernelFplInfo>();
			fplWaitStateChecker = new FplWaitStateChecker(this);
		}

		private bool removeWaitingThread(SceKernelThreadInfo thread)
		{
			SceKernelFplInfo fpl = fplMap[thread.wait.Fpl_id];
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
				log.warn("FPL deleted while we were waiting for it! (timeout expired)");
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
				log.warn("EventFlag deleted while we were waiting for it!");
				// Return WAIT_DELETE
				thread.cpuContext._v0 = ERROR_KERNEL_WAIT_DELETE;
			}
		}

		public virtual void onThreadDeleted(SceKernelThreadInfo thread)
		{
			if (thread.isWaitingForType(PSP_WAIT_FPL))
			{
				removeWaitingThread(thread);
			}
		}

		private void onFplDeletedCancelled(int fid, int result)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			for (IEnumerator<SceKernelThreadInfo> it = threadMan.GetEnumerator(); it.MoveNext();)
			{
				SceKernelThreadInfo thread = it.Current;
				if (thread.isWaitingFor(PSP_WAIT_FPL, fid))
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

		private void onFplDeleted(int fid)
		{
			onFplDeletedCancelled(fid, ERROR_KERNEL_WAIT_DELETE);
		}

		private void onFplCancelled(int fid)
		{
			onFplDeletedCancelled(fid, ERROR_KERNEL_WAIT_CANCELLED);
		}

		private void onFplFree(SceKernelFplInfo info)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			SceKernelThreadInfo checkedThread = null;
			while (info.freeBlocks > 0)
			{
				SceKernelThreadInfo thread = info.threadWaitingList.getNextWaitingThread(checkedThread);
				if (thread == null)
				{
					break;
				}
				int addr = tryAllocateFpl(info);
				if (addr != 0)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("onFplFree waking thread {0}", thread));
					}
					// Return the allocated address
					thread.wait.Fpl_dataAddr.setValue(addr);

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
		private int tryAllocateFpl(SceKernelFplInfo info)
		{
			int block;
			int addr = 0;

			if (info.freeBlocks == 0 || (block = info.findFreeBlock()) == -1)
			{
				log.warn("tryAllocateFpl no free blocks (numBlocks=" + info.numBlocks + ")");
				return 0;
			}
			addr = info.allocateBlock(block);

			return addr;
		}

		public virtual int checkFplID(int uid)
		{
			SceUidManager.checkUidPurpose(uid, "ThreadMan-Fpl", true);
			if (!fplMap.ContainsKey(uid))
			{
				log.warn(string.Format("checkFplID unknown uid=0x{0:X}", uid));
				throw new SceKernelErrorException(ERROR_KERNEL_NOT_FOUND_FPOOL);
			}

			return uid;
		}

		public virtual int sceKernelCreateFpl(PspString name, int partitionid, int attr, int blocksize, int blocks, TPointer option)
		{
			if (name.Null)
			{
				// PSP is returning this error in case of NULL name
				return SceKernelErrors.ERROR_KERNEL_NO_MEMORY;
			}

			int memType = PSP_SMEM_Low;
			if ((attr & PSP_FPL_ATTR_ADDR_HIGH) == PSP_FPL_ATTR_ADDR_HIGH)
			{
				memType = PSP_SMEM_High;
			}
			int memAlign = 4; // 4-bytes is default.
			if (option.NotNull)
			{
				int optionSize = option.getValue32();
				// Up to firmware 6.20 only two FplOptParam fields exist, being the
				// first one the struct size, the second is the memory alignment (0 is default,
				// which is 4-byte/32-bit).
				if ((optionSize >= 4) && (optionSize <= 8))
				{
					SceKernelFplOptParam optParams = new SceKernelFplOptParam();
					optParams.read(option);
					memAlign = optParams.align;
					if (!Utilities.isPower2(memAlign))
					{
						// The alignment has to be a power of 2.
						return SceKernelErrors.ERROR_KERNEL_ILLEGAL_ARGUMENT;
					}
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceKernelCreateFpl options: struct size={0:D}, alignment=0x{1:X}", optParams.@sizeof(), optParams.align));
					}
				}
				else
				{
					log.warn(string.Format("sceKernelCreateFpl option at {0}, size={1:D}", option, optionSize));
				}
			}
			if ((attr & ~PSP_FPL_ATTR_MASK) != 0)
			{
				log.warn(string.Format("sceKernelCreateFpl bad attr value 0x{0:X}", attr));
				return ERROR_KERNEL_ILLEGAL_ATTR;
			}
			if (blocksize <= 0)
			{
				log.warn(string.Format("sceKernelCreateFpl bad blocksize {0:D}", blocksize));
				return ERROR_KERNEL_ILLEGAL_MEMSIZE;
			}
			if (blocks <= 0)
			{
				log.warn(string.Format("sceKernelCreateFpl bad number of blocks {0:D}", blocks));
				return ERROR_KERNEL_ILLEGAL_MEMSIZE;
			}
			if (blocks * blocksize < 0)
			{
				return ERROR_KERNEL_NO_MEMORY;
			}
			if (blocks * blocksize != blocks * (long) blocksize)
			{
				return ERROR_KERNEL_ILLEGAL_MEMSIZE;
			}

			SceKernelFplInfo info = SceKernelFplInfo.tryCreateFpl(name.String, partitionid, attr, blocksize, blocks, memType, memAlign);
			if (info == null)
			{
				return ERROR_KERNEL_NO_MEMORY;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelCreateFpl returning {0}", info));
			}
			fplMap[info.uid] = info;

			return info.uid;
		}

		public virtual int sceKernelDeleteFpl(int uid)
		{
			SceKernelFplInfo info = fplMap.Remove(uid);
			if (info.freeBlocks < info.numBlocks)
			{
				log.warn(string.Format("sceKernelDeleteFpl {0} unfreed blocks, deleting", info.numBlocks - info.freeBlocks));
			}
			info.deleteSysMemInfo();
			onFplDeleted(uid);

			return 0;
		}

		private int hleKernelAllocateFpl(int uid, TPointer32 dataAddr, TPointer32 timeoutAddr, bool wait, bool doCallbacks)
		{
			SceKernelFplInfo fpl = fplMap[uid];
			int addr = tryAllocateFpl(fpl);
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			if (addr == 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleKernelAllocateFpl {0} fast check failed", fpl));
				}
				if (!wait)
				{
					return ERROR_KERNEL_WAIT_CAN_NOT_WAIT;
				}
				// Go to wait state
				SceKernelThreadInfo currentThread = threadMan.CurrentThread;
				fpl.threadWaitingList.addWaitingThread(currentThread);
				currentThread.wait.Fpl_id = uid;
				currentThread.wait.Fpl_dataAddr = dataAddr;
				threadMan.hleKernelThreadEnterWaitState(PSP_WAIT_FPL, uid, fplWaitStateChecker, timeoutAddr.Address, doCallbacks);
			}
			else
			{
				// Success, do not reschedule the current thread.
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleKernelAllocateFpl {0} fast check succeeded", fpl));
				}
				dataAddr.setValue(addr);
			}

			return 0;
		}

		public virtual int sceKernelAllocateFpl(int uid, TPointer32 dataAddr, TPointer32 timeoutAddr)
		{
			return hleKernelAllocateFpl(uid, dataAddr, timeoutAddr, true, false);
		}

		public virtual int sceKernelAllocateFplCB(int uid, TPointer32 dataAddr, TPointer32 timeoutAddr)
		{
			return hleKernelAllocateFpl(uid, dataAddr, timeoutAddr, true, true);
		}

		public virtual int sceKernelTryAllocateFpl(int uid, TPointer32 dataAddr)
		{
			return hleKernelAllocateFpl(uid, dataAddr, TPointer32.NULL, false, false);
		}

		public virtual int sceKernelFreeFpl(int uid, TPointer dataAddr)
		{
			SceKernelFplInfo info = fplMap[uid];
			int block = info.findBlockByAddress(dataAddr.Address);
			if (block < 0)
			{
				log.warn(string.Format("sceKernelFreeFpl unknown block address={0}", dataAddr));
				return ERROR_KERNEL_ILLEGAL_MEMBLOCK;
			}

			info.freeBlock(block);
			onFplFree(info);

			return 0;
		}

		public virtual int sceKernelCancelFpl(int uid, TPointer32 numWaitThreadAddr)
		{
			SceKernelFplInfo info = fplMap[uid];
			numWaitThreadAddr.setValue(info.NumWaitThreads);
			info.threadWaitingList.removeAllWaitingThreads();
			onFplCancelled(uid);

			return 0;
		}

		public virtual int sceKernelReferFplStatus(int uid, TPointer infoAddr)
		{
			SceKernelFplInfo info = fplMap[uid];
			info.write(infoAddr);

			return 0;
		}

		private class FplWaitStateChecker : IWaitStateChecker
		{
			private readonly FplManager outerInstance;

			public FplWaitStateChecker(FplManager outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				// Check if the thread has to continue its wait state or if the fpl
				// has been allocated during the callback execution.
				SceKernelFplInfo fpl = outerInstance.fplMap[wait.Fpl_id];
				if (fpl == null)
				{
					thread.cpuContext._v0 = ERROR_KERNEL_NOT_FOUND_FPOOL;
					return false;
				}

				// Check fpl.
				int addr = outerInstance.tryAllocateFpl(fpl);
				if (addr != 0)
				{
					fpl.threadWaitingList.removeWaitingThread(thread);
					thread.wait.Fpl_dataAddr.setValue(addr);
					thread.cpuContext._v0 = 0;
					return false;
				}

				return true;
			}
		}
		public static readonly FplManager singleton = new FplManager();

		private FplManager()
		{
		}
	}
}