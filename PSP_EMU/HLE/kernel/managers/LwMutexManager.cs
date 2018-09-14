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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_LWMUTEX_NOT_FOUND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_LWMUTEX_LOCKED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_LWMUTEX_RECURSIVE_NOT_ALLOWED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_LWMUTEX_UNLOCKED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_LWMUTEX_UNLOCK_UNDERFLOW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_DELETE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_STATUS_RELEASED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_TIMEOUT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_READY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_LWMUTEX;


	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using SceKernelLwMutexInfo = pspsharp.HLE.kernel.types.SceKernelLwMutexInfo;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;

	using Logger = org.apache.log4j.Logger;

	public class LwMutexManager
	{
		protected internal static Logger log = Modules.getLogger("ThreadManForUser");

		private Dictionary<int, SceKernelLwMutexInfo> lwMutexMap;
		private LwMutexWaitStateChecker lwMutexWaitStateChecker;

		public const int PSP_LWMUTEX_ATTR_FIFO = 0;
		public const int PSP_LWMUTEX_ATTR_PRIORITY = 0x100;
		private const int PSP_LWMUTEX_ATTR_ALLOW_RECURSIVE = 0x200;

		public virtual void reset()
		{
			lwMutexMap = new Dictionary<int, SceKernelLwMutexInfo>();
			lwMutexWaitStateChecker = new LwMutexWaitStateChecker(this);
		}

		private bool removeWaitingThread(SceKernelThreadInfo thread)
		{
			SceKernelLwMutexInfo info = lwMutexMap[thread.wait.LwMutex_id];
			if (info == null)
			{
				return false;
			}

			info.threadWaitingList.removeWaitingThread(thread);

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
				log.warn("LwMutex deleted while we were waiting for it! (timeout expired)");
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
			if (thread.isWaitingForType(PSP_WAIT_LWMUTEX))
			{
				// decrement numWaitThreads
				removeWaitingThread(thread);
			}
		}

		private void onLwMutexDeleted(int lwmid)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			for (IEnumerator<SceKernelThreadInfo> it = threadMan.GetEnumerator(); it.MoveNext();)
			{
				SceKernelThreadInfo thread = it.Current;
				if (thread.isWaitingFor(PSP_WAIT_LWMUTEX, lwmid))
				{
					thread.cpuContext._v0 = ERROR_KERNEL_WAIT_DELETE;
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

		private void onLwMutexModified(SceKernelLwMutexInfo info)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			SceKernelThreadInfo checkedThread = null;
			while (true)
			{
				SceKernelThreadInfo thread = info.threadWaitingList.getNextWaitingThread(checkedThread);
				if (thread == null)
				{
					break;
				}
				if (tryLockLwMutex(info, thread.wait.LwMutex_count, thread))
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("onLwMutexModified waking thread {0}", thread));
					}
					// New thread is taking control of LwMutex.
					info.threadid = thread.uid;
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
				Modules.ThreadManForUserModule.hleRescheduleCurrentThread();
			}
		}

		private bool tryLockLwMutex(SceKernelLwMutexInfo info, int count, SceKernelThreadInfo thread)
		{
			if (info.lockedCount == 0)
			{
				// If the lwmutex is not locked, allow this thread to lock it.
				info.threadid = thread.uid;
				info.lockedCount += count;
				return true;
			}
			else if (info.threadid == thread.uid)
			{
				// If the lwmutex is already locked, but it's trying to be locked by the same thread
				// that acquired it initially, check if recursive locking is allowed.
				// If not, return an error.
				if (((info.attr & PSP_LWMUTEX_ATTR_ALLOW_RECURSIVE) == PSP_LWMUTEX_ATTR_ALLOW_RECURSIVE))
				{
					info.lockedCount += count;
					return true;
				}
			}
			return false;
		}

		private int hleKernelLockLwMutex(int uid, int count, TPointer32 timeoutAddr, bool wait, bool doCallbacks)
		{
			SceKernelLwMutexInfo info = lwMutexMap[uid];
			if (info == null)
			{
				if (uid == 0)
				{
					// Avoid spamming messages for uid==0
					if (log.DebugEnabled)
					{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("hleKernelLockLwMutex uid=%d, count=%d, timeout_addr=%s, wait=%b, doCallbacks=%b -  - unknown UID", uid, count, timeoutAddr, wait, doCallbacks));
						log.debug(string.Format("hleKernelLockLwMutex uid=%d, count=%d, timeout_addr=%s, wait=%b, doCallbacks=%b -  - unknown UID", uid, count, timeoutAddr, wait, doCallbacks));
					}
				}
				else
				{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.warn(String.format("hleKernelLockLwMutex uid=%d, count=%d, timeout_addr=%s, wait=%b, doCallbacks=%b -  - unknown UID", uid, count, timeoutAddr, wait, doCallbacks));
					log.warn(string.Format("hleKernelLockLwMutex uid=%d, count=%d, timeout_addr=%s, wait=%b, doCallbacks=%b -  - unknown UID", uid, count, timeoutAddr, wait, doCallbacks));
				}
				return ERROR_KERNEL_LWMUTEX_NOT_FOUND;
			}

			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			SceKernelThreadInfo currentThread = threadMan.CurrentThread;
			if (!tryLockLwMutex(info, count, currentThread))
			{
				if (log.DebugEnabled)
				{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("hleKernelLockLwMutex %s, count=%d, timeout_addr=%s, wait=%b, doCallbacks=%b - fast check failed", info.toString(), count, timeoutAddr, wait, doCallbacks));
					log.debug(string.Format("hleKernelLockLwMutex %s, count=%d, timeout_addr=%s, wait=%b, doCallbacks=%b - fast check failed", info.ToString(), count, timeoutAddr, wait, doCallbacks));
				}
				if (wait && info.threadid != currentThread.uid)
				{
					// Failed, but it's ok, just wait a little
					info.threadWaitingList.addWaitingThread(currentThread);
					// Wait on a specific lwmutex
					currentThread.wait.LwMutex_id = uid;
					currentThread.wait.LwMutex_count = count;
					threadMan.hleKernelThreadEnterWaitState(PSP_WAIT_LWMUTEX, uid, lwMutexWaitStateChecker, timeoutAddr.Address, doCallbacks);
				}
				else
				{
					if ((info.attr & PSP_LWMUTEX_ATTR_ALLOW_RECURSIVE) != PSP_LWMUTEX_ATTR_ALLOW_RECURSIVE)
					{
						return ERROR_KERNEL_LWMUTEX_RECURSIVE_NOT_ALLOWED;
					}
					return ERROR_KERNEL_LWMUTEX_LOCKED;
				}
			}
			else
			{
				// Success, do not reschedule the current thread.
				if (log.DebugEnabled)
				{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("hleKernelLockLwMutex %s, count=%d, timeout_addr=%s, wait=%b, doCallbacks=%b - fast check succeeded", info.toString(), count, timeoutAddr, wait, doCallbacks));
					log.debug(string.Format("hleKernelLockLwMutex %s, count=%d, timeout_addr=%s, wait=%b, doCallbacks=%b - fast check succeeded", info.ToString(), count, timeoutAddr, wait, doCallbacks));
				}
			}

			return 0;
		}

		public virtual int sceKernelCreateLwMutex(TPointer workAreaAddr, string name, int attr, int count, TPointer option)
		{
			SceKernelLwMutexInfo info = new SceKernelLwMutexInfo(workAreaAddr, name, count, attr);
			lwMutexMap[info.uid] = info;

			// Return 0 in case of no error, do not return the UID of the created mutex
			return 0;
		}

		public virtual int sceKernelDeleteLwMutex(TPointer workAreaAddr)
		{
			int uid = workAreaAddr.getValue32();

			SceKernelLwMutexInfo info = lwMutexMap.Remove(uid);
			if (info == null)
			{
				log.warn("sceKernelDeleteLwMutex unknown UID " + uid.ToString("x"));
				return ERROR_KERNEL_LWMUTEX_NOT_FOUND;
			}

			workAreaAddr.setValue32(0); // Clear uid.
			onLwMutexDeleted(uid);

			return 0;
		}

		public virtual int sceKernelLockLwMutex(TPointer workAreaAddr, int count, TPointer32 timeoutAddr)
		{
			int uid = workAreaAddr.getValue32();
			return hleKernelLockLwMutex(uid, count, timeoutAddr, true, false);
		}

		public virtual int sceKernelLockLwMutexCB(TPointer workAreaAddr, int count, TPointer32 timeoutAddr)
		{
			int uid = workAreaAddr.getValue32();
			return hleKernelLockLwMutex(uid, count, timeoutAddr, true, true);
		}

		public virtual int sceKernelTryLockLwMutex(TPointer workAreaAddr, int count)
		{
			int uid = workAreaAddr.getValue32();
			return hleKernelLockLwMutex(uid, count, TPointer32.NULL, false, false);
		}

		public virtual int sceKernelUnlockLwMutex(TPointer workAreaAddr, int count)
		{
			int uid = workAreaAddr.getValue32();
			SceKernelLwMutexInfo info = lwMutexMap[uid];
			if (info == null)
			{
				if (uid == 0)
				{
					// Avoid spamming messages for uid==0
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceKernelUnlockLwMutex unknown uid=0x{0:X}", uid));
					}
				}
				else
				{
					log.warn(string.Format("sceKernelUnlockLwMutex unknown uid=0x{0:X}", uid));
				}
				return ERROR_KERNEL_LWMUTEX_NOT_FOUND;
			}
			if (info.lockedCount == 0)
			{
				log.debug("sceKernelUnlockLwMutex not locked");
				return ERROR_KERNEL_LWMUTEX_UNLOCKED;
			}
			if (info.lockedCount < 0)
			{
				log.warn("sceKernelUnlockLwMutex underflow");
				return ERROR_KERNEL_LWMUTEX_UNLOCK_UNDERFLOW;
			}

			info.lockedCount -= count;
			if (info.lockedCount == 0)
			{
				info.threadid = -1;
				onLwMutexModified(info);
			}

			return 0;
		}

		public virtual int sceKernelReferLwMutexStatus(TPointer workAreaAddr, TPointer addr)
		{
			int uid = workAreaAddr.getValue32();
			SceKernelLwMutexInfo info = lwMutexMap[uid];
			if (info == null)
			{
				log.warn("sceKernelReferLwMutexStatus unknown UID " + uid.ToString("x"));
				return ERROR_KERNEL_LWMUTEX_NOT_FOUND;
			}

			info.write(addr);

			return 0;
		}

		public virtual int sceKernelReferLwMutexStatusByID(int uid, TPointer addr)
		{
			SceKernelLwMutexInfo info = lwMutexMap[uid];
			if (info == null)
			{
				log.warn("sceKernelReferLwMutexStatus unknown UID " + uid.ToString("x"));
				return ERROR_KERNEL_LWMUTEX_NOT_FOUND;
			}

			info.write(addr);

			return 0;
		}

		private class LwMutexWaitStateChecker : IWaitStateChecker
		{
			private readonly LwMutexManager outerInstance;

			public LwMutexWaitStateChecker(LwMutexManager outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				// Check if the thread has to continue its wait state or if the lwmutex
				// has been unlocked during the callback execution.
				SceKernelLwMutexInfo info = outerInstance.lwMutexMap[wait.LwMutex_id];
				if (info == null)
				{
					thread.cpuContext._v0 = ERROR_KERNEL_LWMUTEX_NOT_FOUND;
					return false;
				}

				// Check the lwmutex.
				if (outerInstance.tryLockLwMutex(info, wait.LwMutex_count, thread))
				{
					info.threadWaitingList.removeWaitingThread(thread);
					thread.cpuContext._v0 = 0;
					return false;
				}

				return true;
			}
		}
		public static readonly LwMutexManager singleton = new LwMutexManager();

		private LwMutexManager()
		{
		}
	}
}