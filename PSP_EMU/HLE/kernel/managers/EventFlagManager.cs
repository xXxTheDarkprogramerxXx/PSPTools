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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_EVENT_FLAG_ILLEGAL_WAIT_PATTERN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_EVENT_FLAG_NO_MULTI_PERM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_EVENT_FLAG_POLL_FAILED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_ILLEGAL_MODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NOT_FOUND_EVENT_FLAG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_CANCELLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_CAN_NOT_WAIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_DELETE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_STATUS_RELEASED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_TIMEOUT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_READY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_EVENTFLAG;


	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using SceKernelEventFlagInfo = pspsharp.HLE.kernel.types.SceKernelEventFlagInfo;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;

	//using Logger = org.apache.log4j.Logger;

	public class EventFlagManager
	{

		protected internal static Logger log = Modules.getLogger("ThreadManForUser");

		private static Dictionary<int, SceKernelEventFlagInfo> eventMap;
		private EventFlagWaitStateChecker eventFlagWaitStateChecker;

		protected internal const int PSP_EVENT_WAITSINGLE = 0;
		protected internal const int PSP_EVENT_WAITMULTIPLE = 0x200;
		protected internal const int PSP_EVENT_WAITANDOR_MASK = 0x01;
		protected internal const int PSP_EVENT_WAITAND = 0x00;
		protected internal const int PSP_EVENT_WAITOR = 0x01;
		protected internal const int PSP_EVENT_WAITCLEARALL = 0x10;
		protected internal const int PSP_EVENT_WAITCLEAR = 0x20;

		public virtual void reset()
		{
			eventMap = new Dictionary<int, SceKernelEventFlagInfo>();
			eventFlagWaitStateChecker = new EventFlagWaitStateChecker(this);
		}

		/// <summary>
		/// Don't call this unless thread.waitType == PSP_WAIT_EVENTFLAG </summary>
		/// <returns> true if the thread was waiting on a valid event flag  </returns>
		private bool removeWaitingThread(SceKernelThreadInfo thread)
		{
			SceKernelEventFlagInfo @event = eventMap[thread.wait.EventFlag_id];
			if (@event == null)
			{
				return false;
			}

			@event.threadWaitingList.removeWaitingThread(thread);

			// Store the currentPattern at the outBits address, even in case of error
			thread.wait.EventFlag_outBits_addr.setValue(@event.currentPattern);

			return true;
		}

		/// <summary>
		/// Don't call this unless thread.wait.waitingOnEventFlag == true </summary>
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
				Console.WriteLine("EventFlag deleted while we were waiting for it! (timeout expired)");
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
			if (thread.isWaitingForType(PSP_WAIT_EVENTFLAG))
			{
				// decrement numWaitThreads
				removeWaitingThread(thread);
			}
		}

		private void onEventFlagDeletedCancelled(int evid, int result)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			for (IEnumerator<SceKernelThreadInfo> it = threadMan.GetEnumerator(); it.MoveNext();)
			{
				SceKernelThreadInfo thread = it.Current;
				if (thread.isWaitingFor(PSP_WAIT_EVENTFLAG, evid))
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

		private void onEventFlagDeleted(int evid)
		{
			onEventFlagDeletedCancelled(evid, ERROR_KERNEL_WAIT_DELETE);
		}

		private void onEventFlagCancelled(int evid)
		{
			onEventFlagDeletedCancelled(evid, ERROR_KERNEL_WAIT_CANCELLED);
		}

		private void onEventFlagModified(SceKernelEventFlagInfo @event)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			SceKernelThreadInfo checkedThread = null;
			while (@event.currentPattern != 0)
			{
				SceKernelThreadInfo thread = @event.threadWaitingList.getNextWaitingThread(checkedThread);
				if (thread == null)
				{
					break;
				}
				if (checkEventFlag(@event, thread.wait.EventFlag_bits, thread.wait.EventFlag_wait, thread.wait.EventFlag_outBits_addr))
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("onEventFlagModified waking thread {0}", thread));
					}
					@event.threadWaitingList.removeWaitingThread(thread);
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

		private bool checkEventFlag(SceKernelEventFlagInfo @event, int bits, int wait, TPointer32 outBitsAddr)
		{
			bool matched = false;

			if (((wait & PSP_EVENT_WAITANDOR_MASK) == PSP_EVENT_WAITAND) && ((@event.currentPattern & bits) == bits))
			{
				matched = true;
			}
			else if (((wait & PSP_EVENT_WAITANDOR_MASK) == PSP_EVENT_WAITOR) && ((@event.currentPattern & bits) != 0))
			{
				matched = true;
			}

			if (matched)
			{
				// Write current pattern.
				outBitsAddr.setValue(@event.currentPattern);
				if (log.DebugEnabled && outBitsAddr.NotNull)
				{
					Console.WriteLine(string.Format("checkEventFlag returning outBits=0x{0:X} at {1}", outBitsAddr.getValue(), outBitsAddr));
				}

				if ((wait & PSP_EVENT_WAITCLEARALL) == PSP_EVENT_WAITCLEARALL)
				{
					@event.currentPattern = 0;
				}
				if ((wait & PSP_EVENT_WAITCLEAR) == PSP_EVENT_WAITCLEAR)
				{
					@event.currentPattern &= ~bits;
				}
			}
			return matched;
		}

		public virtual int checkEventFlagID(int uid)
		{
			SceUidManager.checkUidPurpose(uid, "ThreadMan-eventflag", true);
			if (!eventMap.ContainsKey(uid))
			{
				if (uid != 0)
				{
					Console.WriteLine(string.Format("checkEventFlagID unknown uid=0x{0:X}", uid));
				}
				throw new SceKernelErrorException(ERROR_KERNEL_NOT_FOUND_EVENT_FLAG);
			}

			return uid;
		}

		public virtual int sceKernelCreateEventFlag(string name, int attr, int initPattern, TPointer option)
		{
			SceKernelEventFlagInfo @event = new SceKernelEventFlagInfo(name, attr, initPattern, initPattern);
			eventMap[@event.uid] = @event;

			return @event.uid;
		}

		public virtual int sceKernelDeleteEventFlag(int uid)
		{
			SceKernelEventFlagInfo @event = eventMap.Remove(uid);

			if (@event.NumWaitThreads > 0)
			{
				Console.WriteLine(string.Format("sceKernelDeleteEventFlag numWaitThreads {0:D}", @event.NumWaitThreads));
			}
			onEventFlagDeleted(uid);

			return 0;
		}

		public virtual int sceKernelSetEventFlag(int uid, int bitsToSet)
		{
			SceKernelEventFlagInfo @event = eventMap[uid];

			@event.currentPattern |= bitsToSet;
			onEventFlagModified(@event);

			return 0;
		}

		public virtual int sceKernelClearEventFlag(int uid, int bitsToKeep)
		{
			SceKernelEventFlagInfo @event = eventMap[uid];

			@event.currentPattern &= bitsToKeep;

			return 0;
		}

		public virtual int hleKernelWaitEventFlag(int uid, int bits, int wait, TPointer32 outBitsAddr, TPointer32 timeoutAddr, bool doCallbacks)
		{
			if ((wait & ~(PSP_EVENT_WAITOR | PSP_EVENT_WAITCLEAR | PSP_EVENT_WAITCLEARALL)) != 0 || (wait & (PSP_EVENT_WAITCLEAR | PSP_EVENT_WAITCLEARALL)) == (PSP_EVENT_WAITCLEAR | PSP_EVENT_WAITCLEARALL))
			{
				return ERROR_KERNEL_ILLEGAL_MODE;
			}
			if (bits == 0)
			{
				return ERROR_KERNEL_EVENT_FLAG_ILLEGAL_WAIT_PATTERN;
			}
			if (!Modules.ThreadManForUserModule.DispatchThreadEnabled)
			{
				return ERROR_KERNEL_WAIT_CAN_NOT_WAIT;
			}

			SceKernelEventFlagInfo @event = eventMap[uid];
			if (@event.NumWaitThreads >= 1 && (@event.attr & PSP_EVENT_WAITMULTIPLE) != PSP_EVENT_WAITMULTIPLE)
			{
				Console.WriteLine("hleKernelWaitEventFlag already another thread waiting on it");
				return ERROR_KERNEL_EVENT_FLAG_NO_MULTI_PERM;
			}

			if (!checkEventFlag(@event, bits, wait, outBitsAddr))
			{
				// Failed, but it's ok, just wait a little
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleKernelWaitEventFlag - {0} fast check failed", @event));
				}
				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				SceKernelThreadInfo currentThread = threadMan.CurrentThread;
				@event.threadWaitingList.addWaitingThread(currentThread);
				// Wait on a specific event flag
				currentThread.wait.EventFlag_id = uid;
				currentThread.wait.EventFlag_bits = bits;
				currentThread.wait.EventFlag_wait = wait;
				currentThread.wait.EventFlag_outBits_addr = outBitsAddr;

				threadMan.hleKernelThreadEnterWaitState(PSP_WAIT_EVENTFLAG, uid, eventFlagWaitStateChecker, timeoutAddr.Address, doCallbacks);
			}
			else
			{
				// Success, do not reschedule the current thread.
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleKernelWaitEventFlag - {0} fast check succeeded", @event));
				}
			}

			return 0;
		}

		public virtual int sceKernelWaitEventFlag(int uid, int bits, int wait, TPointer32 outBitsAddr, TPointer32 timeoutAddr)
		{
			return hleKernelWaitEventFlag(uid, bits, wait, outBitsAddr, timeoutAddr, false);
		}

		public virtual int sceKernelWaitEventFlagCB(int uid, int bits, int wait, TPointer32 outBitsAddr, TPointer32 timeoutAddr)
		{
			return hleKernelWaitEventFlag(uid, bits, wait, outBitsAddr, timeoutAddr, true);
		}

		public virtual int sceKernelPollEventFlag(int uid, int bits, int wait, TPointer32 outBitsAddr)
		{
			if (bits == 0)
			{
				return ERROR_KERNEL_EVENT_FLAG_ILLEGAL_WAIT_PATTERN;
			}

			SceKernelEventFlagInfo @event = eventMap[uid];
			if (!checkEventFlag(@event, bits, wait, outBitsAddr))
			{
				// Write the outBits, even if the poll failed
				outBitsAddr.setValue(@event.currentPattern);
				return ERROR_KERNEL_EVENT_FLAG_POLL_FAILED;
			}

			return 0;
		}

		public virtual int sceKernelCancelEventFlag(int uid, int newPattern, TPointer32 numWaitThreadAddr)
		{
			SceKernelEventFlagInfo @event = eventMap[uid];

			numWaitThreadAddr.setValue(@event.NumWaitThreads);
			@event.threadWaitingList.removeAllWaitingThreads();
			@event.currentPattern = newPattern;
			onEventFlagCancelled(uid);

			return 0;
		}

		public virtual int sceKernelReferEventFlagStatus(int uid, TPointer addr)
		{
			SceKernelEventFlagInfo @event = eventMap[uid];
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelReferEventFlagStatus event={0}", @event));
			}
			@event.write(addr);

			return 0;
		}

		private class EventFlagWaitStateChecker : IWaitStateChecker
		{
			private readonly EventFlagManager outerInstance;

			public EventFlagWaitStateChecker(EventFlagManager outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				// Check if the thread has to continue its wait state or if the event flag
				// has been set during the callback execution.
				SceKernelEventFlagInfo @event = eventMap[wait.EventFlag_id];
				if (@event == null)
				{
					thread.cpuContext._v0 = ERROR_KERNEL_NOT_FOUND_EVENT_FLAG;
					return false;
				}

				// Check EventFlag.
				if (outerInstance.checkEventFlag(@event, wait.EventFlag_bits, wait.EventFlag_wait, wait.EventFlag_outBits_addr))
				{
					@event.threadWaitingList.removeWaitingThread(thread);
					thread.cpuContext._v0 = 0;
					return false;
				}

				return true;
			}
		}
		public static readonly EventFlagManager singleton = new EventFlagManager();

		private EventFlagManager()
		{
		}
	}
}