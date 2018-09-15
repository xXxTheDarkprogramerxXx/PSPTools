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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_ILLEGAL_COUNT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NOT_FOUND_SEMAPHORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_SEMA_OVERFLOW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_SEMA_ZERO;
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
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_SEMA;


	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelSemaInfo = pspsharp.HLE.kernel.types.SceKernelSemaInfo;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;

	//using Logger = org.apache.log4j.Logger;

	public class SemaManager
	{
		protected internal static Logger log = Modules.getLogger("ThreadManForUser");

		private Dictionary<int, SceKernelSemaInfo> semaMap;
		private SemaWaitStateChecker semaWaitStateChecker;

		public const int PSP_SEMA_ATTR_FIFO = 0; // Signal waiting threads with a FIFO iterator.
		public const int PSP_SEMA_ATTR_PRIORITY = 0x100; // Signal waiting threads with a priority based iterator.

		public virtual void reset()
		{
			semaMap = new Dictionary<int, SceKernelSemaInfo>();
			semaWaitStateChecker = new SemaWaitStateChecker(this);
		}

		/// <summary>
		/// Don't call this unless thread.wait.waitingOnSemaphore == true </summary>
		/// <returns> true if the thread was waiting on a valid sema  </returns>
		private bool removeWaitingThread(SceKernelThreadInfo thread)
		{
			SceKernelSemaInfo sema = semaMap[thread.wait.Semaphore_id];
			if (sema == null)
			{
				return false;
			}

			sema.threadWaitingList.removeWaitingThread(thread);

			return true;
		}

		/// <summary>
		/// Don't call this unless thread.wait.waitingOnSemaphore == true </summary>
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
				Console.WriteLine("Sema deleted while we were waiting for it! (timeout expired)");
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
			if (thread.isWaitingForType(PSP_WAIT_SEMA))
			{
				// decrement numWaitThreads
				removeWaitingThread(thread);
			}
		}

		private void onSemaphoreDeletedCancelled(int semaid, int result)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			for (IEnumerator<SceKernelThreadInfo> it = threadMan.GetEnumerator(); it.MoveNext();)
			{
				SceKernelThreadInfo thread = it.Current;
				if (thread.isWaitingFor(PSP_WAIT_SEMA, semaid))
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

		private void onSemaphoreDeleted(int semaid)
		{
			onSemaphoreDeletedCancelled(semaid, ERROR_KERNEL_WAIT_DELETE);
		}

		private void onSemaphoreCancelled(int semaid)
		{
			onSemaphoreDeletedCancelled(semaid, ERROR_KERNEL_WAIT_CANCELLED);
		}

		private void onSemaphoreModified(SceKernelSemaInfo sema)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			SceKernelThreadInfo checkedThread = null;
			while (sema.currentCount > 0)
			{
				SceKernelThreadInfo thread = sema.threadWaitingList.getNextWaitingThread(checkedThread);
				if (thread == null)
				{
					break;
				}
				if (tryWaitSemaphore(sema, thread.wait.Semaphore_signal))
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("onSemaphoreModified waking thread {0}", thread));
					}
					sema.threadWaitingList.removeWaitingThread(thread);
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

		private bool tryWaitSemaphore(SceKernelSemaInfo sema, int signal)
		{
			bool success = false;
			if (sema.currentCount >= signal)
			{
				sema.currentCount -= signal;
				success = true;
			}
			return success;
		}

		public virtual int checkSemaID(int semaid)
		{
			SceUidManager.checkUidPurpose(semaid, "ThreadMan-sema", true);
			if (!semaMap.ContainsKey(semaid))
			{
				if (semaid == 0)
				{
					// Some applications systematically try to signal a semaid=0.
					// Do not spam WARNings for this case.
					Console.WriteLine(string.Format("checkSemaID - unknown uid 0x{0:X}", semaid));
				}
				else
				{
					Console.WriteLine(string.Format("checkSemaID - unknown uid 0x{0:X}", semaid));
				}
				throw new SceKernelErrorException(ERROR_KERNEL_NOT_FOUND_SEMAPHORE);
			}

			return semaid;
		}

		public virtual SceKernelSemaInfo hleKernelCreateSema(string name, int attr, int initVal, int maxVal, TPointer option)
		{
			if (option.NotNull)
			{
				// The first int does not seem to be the size of the struct, found values:
				// SSX On Tour: 0, 0x08B0F9E4, 0x0892E664, 0x08AF7257 (some values are used in more than one semaphore)
				int optionSize = option.getValue32();
				Console.WriteLine(string.Format("sceKernelCreateSema option at {0}, size={1:D}", option, optionSize));
			}

			SceKernelSemaInfo sema = new SceKernelSemaInfo(name, attr, initVal, maxVal);
			semaMap[sema.uid] = sema;

			return sema;
		}

		public virtual int hleKernelWaitSema(SceKernelSemaInfo sema, int signal, TPointer32 timeoutAddr, bool doCallbacks)
		{
			if (!tryWaitSemaphore(sema, signal))
			{
				// Failed, but it's ok, just wait a little
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleKernelWaitSema {0} fast check failed", sema));
				}
				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				SceKernelThreadInfo currentThread = threadMan.CurrentThread;
				sema.threadWaitingList.addWaitingThread(currentThread);
				// Wait on a specific semaphore
				currentThread.wait.Semaphore_id = sema.uid;
				currentThread.wait.Semaphore_signal = signal;
				threadMan.hleKernelThreadEnterWaitState(PSP_WAIT_SEMA, sema.uid, semaWaitStateChecker, timeoutAddr.Address, doCallbacks);
			}
			else
			{
				// Success, do not reschedule the current thread.
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleKernelWaitSema {0} fast check succeeded", sema));
				}
			}

			return 0;
		}

		private int hleKernelWaitSema(int semaid, int signal, TPointer32 timeoutAddr, bool doCallbacks)
		{
			if (signal <= 0)
			{
				Console.WriteLine(string.Format("hleKernelWaitSema - bad signal {0:D}", signal));
				return ERROR_KERNEL_ILLEGAL_COUNT;
			}

			SceKernelSemaInfo sema = semaMap[semaid];
			if (signal > sema.maxCount)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleKernelWaitSema returning 0x{0:X8}(ERROR_KERNEL_ILLEGAL_COUNT)", ERROR_KERNEL_ILLEGAL_COUNT));
				}
				return ERROR_KERNEL_ILLEGAL_COUNT;
			}

			return hleKernelWaitSema(sema, signal, timeoutAddr, doCallbacks);
		}

		public virtual int hleKernelPollSema(SceKernelSemaInfo sema, int signal)
		{
			if (signal > sema.currentCount)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleKernelPollSema returning 0x{0:X8}(ERROR_KERNEL_SEMA_ZERO)", ERROR_KERNEL_SEMA_ZERO));
				}
				return ERROR_KERNEL_SEMA_ZERO;
			}

			sema.currentCount -= signal;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("hleKernelPollSema returning 0, {0}", sema));
			}

			return 0;
		}

		public virtual int hleKernelSignalSema(SceKernelSemaInfo sema, int signal)
		{
			// Check that currentCount will not exceed the maxCount
			// after releasing all the threads waiting on this sema.
			int newCount = sema.currentCount + signal;
			if (newCount > sema.maxCount)
			{
				for (IEnumerator<SceKernelThreadInfo> it = Modules.ThreadManForUserModule.GetEnumerator(); it.MoveNext();)
				{
					SceKernelThreadInfo thread = it.Current;
					if (thread.isWaitingForType(PSP_WAIT_SEMA) && thread.wait.Semaphore_id == sema.uid)
					{
						newCount -= thread.wait.Semaphore_signal;
					}
				}
				if (newCount > sema.maxCount)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("hleKernelSignalSema returning 0x{0:X8}(ERROR_KERNEL_SEMA_OVERFLOW)", ERROR_KERNEL_SEMA_OVERFLOW));
					}
					return ERROR_KERNEL_SEMA_OVERFLOW;
				}
			}

			sema.currentCount += signal;

			onSemaphoreModified(sema);

			// Sanity check...
			if (sema.currentCount > sema.maxCount)
			{
				// This situation should never happen, otherwise something went wrong
				// in the overflow check above.
				Console.WriteLine(string.Format("hleKernelSignalSema currentCount {0:D} exceeding maxCount {1:D}", sema.currentCount, sema.maxCount));
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("hleKernelSignalSema returning 0, {0}", sema));
			}

			return 0;
		}

		public virtual int sceKernelCreateSema(string name, int attr, int initVal, int maxVal, TPointer option)
		{
			if (string.ReferenceEquals(name, null))
			{
				return SceKernelErrors.ERROR_KERNEL_ERROR;
			}
			SceKernelSemaInfo sema = hleKernelCreateSema(name, attr, initVal, maxVal, option);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelCreateSema {0}", sema));
			}

			return sema.uid;
		}

		public virtual int sceKernelDeleteSema(int semaid)
		{
			semaMap.Remove(semaid);
			onSemaphoreDeleted(semaid);

			return 0;
		}

		public virtual int sceKernelWaitSema(int semaid, int signal, TPointer32 timeoutAddr)
		{
			return hleKernelWaitSema(semaid, signal, timeoutAddr, false);
		}

		public virtual int sceKernelWaitSemaCB(int semaid, int signal, TPointer32 timeoutAddr)
		{
			return hleKernelWaitSema(semaid, signal, timeoutAddr, true);
		}

		public virtual int sceKernelSignalSema(int semaid, int signal)
		{
			SceKernelSemaInfo sema = semaMap[semaid];
			return hleKernelSignalSema(sema, signal);
		}

		/// <summary>
		/// This is attempt to signal the sema and always return immediately </summary>
		public virtual int sceKernelPollSema(int semaid, int signal)
		{
			if (signal <= 0)
			{
				Console.WriteLine(string.Format("sceKernelPollSema id=0x{0:X}, signal={1:D}: bad signal", semaid, signal));
				return ERROR_KERNEL_ILLEGAL_COUNT;
			}

			SceKernelSemaInfo sema = semaMap[semaid];
			return hleKernelPollSema(sema, signal);
		}

		public virtual int sceKernelCancelSema(int semaid, int newcount, TPointer32 numWaitThreadAddr)
		{
			SceKernelSemaInfo sema = semaMap[semaid];

			if (newcount > sema.maxCount)
			{
				return ERROR_KERNEL_ILLEGAL_COUNT;
			}

			// Write previous numWaitThreads count.
			numWaitThreadAddr.setValue(sema.NumWaitThreads);
			sema.threadWaitingList.removeAllWaitingThreads();
			// Reset this semaphore's count based on newcount.
			// Note: If newcount is negative, the count becomes this semaphore's initCount.
			if (newcount < 0)
			{
				sema.currentCount = sema.initCount;
			}
			else
			{
				sema.currentCount = newcount;
			}
			onSemaphoreCancelled(semaid);

			return 0;
		}

		public virtual int sceKernelReferSemaStatus(int semaid, TPointer addr)
		{
			SceKernelSemaInfo sema = semaMap[semaid];
			sema.write(addr);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelReferSemaStatus returning {0}", sema));
			}

			return 0;
		}

		private class SemaWaitStateChecker : IWaitStateChecker
		{
			private readonly SemaManager outerInstance;

			public SemaWaitStateChecker(SemaManager outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				// Check if the thread has to continue its wait state or if the sema
				// has been signaled during the callback execution.
				SceKernelSemaInfo sema = outerInstance.semaMap[wait.Semaphore_id];
				if (sema == null)
				{
					thread.cpuContext._v0 = ERROR_KERNEL_NOT_FOUND_SEMAPHORE;
					return false;
				}

				// Check the sema.
				if (outerInstance.tryWaitSemaphore(sema, wait.Semaphore_signal))
				{
					sema.threadWaitingList.removeWaitingThread(thread);
					thread.cpuContext._v0 = 0;
					return false;
				}

				return true;
			}
		}
		public static readonly SemaManager singleton = new SemaManager();

		private SemaManager()
		{
		}
	}
}