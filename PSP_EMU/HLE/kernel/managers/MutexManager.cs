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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_MUTEX_NOT_FOUND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_MUTEX_LOCKED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_MUTEX_RECURSIVE_NOT_ALLOWED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_MUTEX_UNLOCKED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_MUTEX_UNLOCK_UNDERFLOW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_CANCELLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_DELETE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_STATUS_RELEASED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_TIMEOUT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_READY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_MUTEX;


	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelMutexInfo = pspsharp.HLE.kernel.types.SceKernelMutexInfo;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;

	//using Logger = org.apache.log4j.Logger;

	public class MutexManager
	{
		//public static Logger log = ThreadManForUser.log;

		private Dictionary<int, SceKernelMutexInfo> mutexMap;
		private MutexWaitStateChecker mutexWaitStateChecker;

		public const int PSP_MUTEX_ATTR_FIFO = 0;
		public const int PSP_MUTEX_ATTR_PRIORITY = 0x100;
		private const int PSP_MUTEX_ATTR_ALLOW_RECURSIVE = 0x200;

		public virtual void reset()
		{
			mutexMap = new Dictionary<int, SceKernelMutexInfo>();
			mutexWaitStateChecker = new MutexWaitStateChecker(this);
		}

		/// <summary>
		/// Don't call this unless thread.waitType == PSP_WAIT_MUTEX </summary>
		/// <returns> true if the thread was waiting on a valid mutex  </returns>
		private bool removeWaitingThread(SceKernelThreadInfo thread)
		{
			SceKernelMutexInfo info = mutexMap[thread.wait.Mutex_id];
			if (info == null)
			{
				return false;
			}

			info.threadWaitingList.removeWaitingThread(thread);

			return true;
		}

		/// <summary>
		/// Don't call this unless thread.wait.waitingOnMutex == true </summary>
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
				Console.WriteLine("Mutex deleted while we were waiting for it! (timeout expired)");
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
			if (thread.isWaitingForType(PSP_WAIT_MUTEX))
			{
				// decrement numWaitThreads
				removeWaitingThread(thread);
			}
			foreach (SceKernelMutexInfo info in mutexMap.Values)
			{
				if (info.threadid == thread.uid)
				{
					log.info(string.Format("onThreadDeleted: thread {0} owning mutex {1}", thread, info));
				}
			}
		}

		private void onMutexDeletedCancelled(int mid, int result)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			for (IEnumerator<SceKernelThreadInfo> it = threadMan.GetEnumerator(); it.MoveNext();)
			{
				SceKernelThreadInfo thread = it.Current;
				if (thread.isWaitingFor(PSP_WAIT_MUTEX, mid))
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

		private void onMutexDeleted(int mid)
		{
			onMutexDeletedCancelled(mid, ERROR_KERNEL_WAIT_DELETE);
		}

		private void onMutexCancelled(int mid)
		{
			onMutexDeletedCancelled(mid, ERROR_KERNEL_WAIT_CANCELLED);
		}

		private void onMutexModified(SceKernelMutexInfo info)
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
				if (tryLockMutex(info, thread.wait.Mutex_count, thread))
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("onMutexModified waking thread {0}", thread));
					}
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

		private bool tryLockMutex(SceKernelMutexInfo info, int count, SceKernelThreadInfo thread)
		{
			if (info.lockedCount == 0)
			{
				// If the mutex is not locked, allow this thread to lock it.
				info.threadid = thread.uid;
				info.lockedCount += count;
				return true;
			}
			else if (info.threadid == thread.uid)
			{
				// If the mutex is already locked, but it's trying to be locked by the same thread
				// that acquired it initially, check if recursive locking is allowed.
				// If not, return an error.
				if (((info.attr & PSP_MUTEX_ATTR_ALLOW_RECURSIVE) == PSP_MUTEX_ATTR_ALLOW_RECURSIVE))
				{
					info.lockedCount += count;
					return true;
				}
			}
			return false;
		}

		public virtual int sceKernelCreateMutex(PspString name, int attr, int count, int option_addr)
		{
			if (count < 0 || (count > 1 && (attr & PSP_MUTEX_ATTR_ALLOW_RECURSIVE) == 0))
			{
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_COUNT;
			}

			SceKernelMutexInfo info = new SceKernelMutexInfo(name.String, count, attr);
			mutexMap[info.uid] = info;

			return info.uid;
		}

		public virtual int sceKernelDeleteMutex(int uid)
		{
			SceKernelMutexInfo info = mutexMap.Remove(uid);
			if (info == null)
			{
				Console.WriteLine("sceKernelDeleteMutex unknown UID " + uid.ToString("x"));
				return ERROR_KERNEL_MUTEX_NOT_FOUND;
			}

			onMutexDeleted(uid);

			return 0;
		}

		private int hleKernelLockMutex(int uid, int count, int timeout_addr, bool wait, bool doCallbacks)
		{
			SceKernelMutexInfo info = mutexMap[uid];
			if (info == null)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("hleKernelLockMutex uid=%d, count=%d, timeout_addr=0x%08X, wait=%b, doCallbacks=%b - unknown UID", uid, count, timeout_addr, wait, doCallbacks));
				Console.WriteLine(string.Format("hleKernelLockMutex uid=%d, count=%d, timeout_addr=0x%08X, wait=%b, doCallbacks=%b - unknown UID", uid, count, timeout_addr, wait, doCallbacks));
				return ERROR_KERNEL_MUTEX_NOT_FOUND;
			}
			if (count <= 0)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("hleKernelLockMutex uid=%d, count=%d, timeout_addr=0x%08X, wait=%b, doCallbacks=%b - illegal count", uid, count, timeout_addr, wait, doCallbacks));
				Console.WriteLine(string.Format("hleKernelLockMutex uid=%d, count=%d, timeout_addr=0x%08X, wait=%b, doCallbacks=%b - illegal count", uid, count, timeout_addr, wait, doCallbacks));
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_COUNT;
			}
			if (count > 1 && (info.attr & PSP_MUTEX_ATTR_ALLOW_RECURSIVE) == 0)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("hleKernelLockMutex uid=%d, count=%d, timeout_addr=0x%08X, wait=%b, doCallbacks=%b - illegal count", uid, count, timeout_addr, wait, doCallbacks));
				Console.WriteLine(string.Format("hleKernelLockMutex uid=%d, count=%d, timeout_addr=0x%08X, wait=%b, doCallbacks=%b - illegal count", uid, count, timeout_addr, wait, doCallbacks));
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_COUNT;
			}

			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			SceKernelThreadInfo currentThread = threadMan.CurrentThread;

			if (!tryLockMutex(info, count, currentThread))
			{
				//if (log.DebugEnabled)
				{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("hleKernelLockMutex %s, count=%d, timeout_addr=0x%08X, wait=%b, doCallbacks=%b - fast check failed", info.toString(), count, timeout_addr, wait, doCallbacks));
					Console.WriteLine(string.Format("hleKernelLockMutex %s, count=%d, timeout_addr=0x%08X, wait=%b, doCallbacks=%b - fast check failed", info.ToString(), count, timeout_addr, wait, doCallbacks));
				}
				if (wait && info.threadid != currentThread.uid)
				{
					// Failed, but it's ok, just wait a little
					info.threadWaitingList.addWaitingThread(currentThread);
					// Wait on a specific mutex
					currentThread.wait.Mutex_id = uid;
					currentThread.wait.Mutex_count = count;
					threadMan.hleKernelThreadEnterWaitState(PSP_WAIT_MUTEX, uid, mutexWaitStateChecker, timeout_addr, doCallbacks);
				}
				else
				{
					if ((info.attr & PSP_MUTEX_ATTR_ALLOW_RECURSIVE) != PSP_MUTEX_ATTR_ALLOW_RECURSIVE)
					{
						return ERROR_KERNEL_MUTEX_RECURSIVE_NOT_ALLOWED;
					}
					return ERROR_KERNEL_MUTEX_LOCKED;
				}
			}
			else
			{
				// Success, do not reschedule the current thread.
				//if (log.DebugEnabled)
				{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("hleKernelLockMutex %s, count=%d, timeout_addr=0x%08X, wait=%b, doCallbacks=%b - fast check succeeded", info.toString(), count, timeout_addr, wait, doCallbacks));
					Console.WriteLine(string.Format("hleKernelLockMutex %s, count=%d, timeout_addr=0x%08X, wait=%b, doCallbacks=%b - fast check succeeded", info.ToString(), count, timeout_addr, wait, doCallbacks));
				}
			}

			return 0;
		}

		public virtual int sceKernelLockMutex(int uid, int count, int timeout_addr)
		{
			return hleKernelLockMutex(uid, count, timeout_addr, true, false);
		}

		public virtual int sceKernelLockMutexCB(int uid, int count, int timeout_addr)
		{
			return hleKernelLockMutex(uid, count, timeout_addr, true, true);
		}

		public virtual int sceKernelTryLockMutex(int uid, int count)
		{
			return hleKernelLockMutex(uid, count, 0, false, false);
		}

		public virtual int sceKernelUnlockMutex(int uid, int count)
		{
			SceKernelMutexInfo info = mutexMap[uid];
			if (info == null)
			{
				Console.WriteLine("sceKernelUnlockMutex unknown uid");
				return ERROR_KERNEL_MUTEX_NOT_FOUND;
			}
			if (info.lockedCount == 0)
			{
				// log only as debug to avoid warning spams on some games
				Console.WriteLine("sceKernelUnlockMutex not locked");
				return ERROR_KERNEL_MUTEX_UNLOCKED;
			}
			if ((info.lockedCount - count) < 0)
			{
				Console.WriteLine("sceKernelUnlockMutex underflow");
				return ERROR_KERNEL_MUTEX_UNLOCK_UNDERFLOW;
			}

			info.lockedCount -= count;
			if (info.lockedCount == 0)
			{
				info.threadid = -1;
				onMutexModified(info);
			}

			return 0;
		}

		public virtual int sceKernelCancelMutex(int uid, int newcount, TPointer32 numWaitThreadAddr)
		{
			SceKernelMutexInfo info = mutexMap[uid];
			if (info == null)
			{
				Console.WriteLine("sceKernelCancelMutex unknown UID " + uid.ToString("x"));
				return ERROR_KERNEL_MUTEX_NOT_FOUND;
			}
			if (info.lockedCount == 0)
			{
				Console.WriteLine("sceKernelCancelMutex UID " + uid.ToString("x") + " not locked");
				return -1;
			}
			if (newcount < 0)
			{
				newcount = info.initCount;
			}
			if (newcount > 1 && (info.attr & PSP_MUTEX_ATTR_ALLOW_RECURSIVE) == 0)
			{
				Console.WriteLine(string.Format("sceKernelCancelMutex uid={0:D}, newcount={1:D} - illegal count", uid, newcount));
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_COUNT;
			}

			// Write previous numWaitThreads count.
			numWaitThreadAddr.setValue(info.NumWaitingThreads);
			info.threadWaitingList.removeAllWaitingThreads();

			// Set new count.
			info.lockedCount = newcount;

			onMutexCancelled(uid);

			return 0;
		}

		public virtual int sceKernelReferMutexStatus(int uid, TPointer addr)
		{
			SceKernelMutexInfo info = mutexMap[uid];
			if (info == null)
			{
				Console.WriteLine("sceKernelReferMutexStatus unknown UID " + uid.ToString("x"));
				return ERROR_KERNEL_MUTEX_NOT_FOUND;
			}

			info.write(addr);

			return 0;
		}

		private class MutexWaitStateChecker : IWaitStateChecker
		{
			private readonly MutexManager outerInstance;

			public MutexWaitStateChecker(MutexManager outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				// Check if the thread has to continue its wait state or if the mutex
				// has been unlocked during the callback execution.
				SceKernelMutexInfo info = outerInstance.mutexMap[wait.Mutex_id];
				if (info == null)
				{
					thread.cpuContext._v0 = ERROR_KERNEL_MUTEX_NOT_FOUND;
					return false;
				}

				// Check the mutex.
				if (outerInstance.tryLockMutex(info, wait.Mutex_count, thread))
				{
					info.threadWaitingList.removeWaitingThread(thread);
					thread.cpuContext._v0 = 0;
					return false;
				}

				return true;
			}
		}
		public static readonly MutexManager singleton = new MutexManager();

		private MutexManager()
		{
		}
	}
}