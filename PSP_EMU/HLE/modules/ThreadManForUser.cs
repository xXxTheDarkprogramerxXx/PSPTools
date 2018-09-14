using System;
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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._a0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._ra;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._s0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._v0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._zr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.HLEModuleManager.HLESyscallNid;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_ILLEGAL_ARGUMENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_ILLEGAL_PRIORITY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_ILLEGAL_THREAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NOT_FOUND_ALARM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NOT_FOUND_THREAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NOT_FOUND_THREAD_EVENT_HANDLER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NOT_FOUND_VTIMER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_THREAD_ALREADY_DORMANT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_THREAD_ALREADY_SUSPEND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_THREAD_IS_NOT_DORMANT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_THREAD_IS_NOT_SUSPEND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_THREAD_IS_TERMINATED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_STATUS_RELEASED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_TIMEOUT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_OUT_OF_MEMORY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadEventHandlerInfo.THREAD_EVENT_CREATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadEventHandlerInfo.THREAD_EVENT_DELETE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadEventHandlerInfo.THREAD_EVENT_EXIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadEventHandlerInfo.THREAD_EVENT_START;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.JPCSP_WAIT_AUDIO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.JPCSP_WAIT_CTRL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.JPCSP_WAIT_DISPLAY_VBLANK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.JPCSP_WAIT_GE_LIST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.JPCSP_WAIT_NET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.JPCSP_WAIT_UMD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.JPCSP_WAIT_USB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_ATTR_KERNEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_ATTR_USER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_READY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_RUNNING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_STOPPED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_SUSPEND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_WAITING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_WAITING_SUSPEND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_DELAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_EVENTFLAG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_MSGPIPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_MUTEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_NONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_SLEEP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_THREAD_END;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_FPL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_LWMUTEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_MBX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_SEMA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_VPL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.KERNEL_PARTITION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.USER_PARTITION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Memory.addressMask;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.MemoryMap.END_KERNEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.MemoryMap.START_KERNEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.alignUp;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeStringZ;


	using CpuState = pspsharp.Allegrex.CpuState;
	using Decoder = pspsharp.Allegrex.Decoder;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using DumpDebugState = pspsharp.Debugger.DumpDebugState;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using Managers = pspsharp.HLE.kernel.Managers;
	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using SystemTimeManager = pspsharp.HLE.kernel.managers.SystemTimeManager;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using SceKernelAlarmInfo = pspsharp.HLE.kernel.types.SceKernelAlarmInfo;
	using SceKernelCallbackInfo = pspsharp.HLE.kernel.types.SceKernelCallbackInfo;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelSystemStatus = pspsharp.HLE.kernel.types.SceKernelSystemStatus;
	using SceKernelThreadEventHandlerInfo = pspsharp.HLE.kernel.types.SceKernelThreadEventHandlerInfo;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceKernelThreadOptParam = pspsharp.HLE.kernel.types.SceKernelThreadOptParam;
	using SceKernelTls = pspsharp.HLE.kernel.types.SceKernelTls;
	using SceKernelVTimerInfo = pspsharp.HLE.kernel.types.SceKernelVTimerInfo;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using pspBaseCallback = pspsharp.HLE.kernel.types.pspBaseCallback;
	using RegisteredCallbacks = pspsharp.HLE.kernel.types.SceKernelThreadInfo.RegisteredCallbacks;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Scheduler = pspsharp.scheduler.Scheduler;
	using DurationStatistics = pspsharp.util.DurationStatistics;

	using Logger = org.apache.log4j.Logger;

	/*
	 * Thread scheduling on PSP:
	 * - when a thread having a higher priority than the current thread, switches to the
	 *   READY state, the current thread is interrupted immediately and is loosing the
	 *   RUNNING state. The new thread then moves to the RUNNING state.
	 * - when a thread having the same or a lower priority than the current thread,
	 *   switches to the READY state, the current thread is not interrupted and is keeping
	 *   the RUNNING state.
	 * - a RUNNING thread can only yield to a thread having the same priority by calling
	 *   sceKernelRotateThreadReadyQueue(0).
	 * - the clock precision when interrupting a RUNNING thread is about 200 microseconds.
	 *   i.e., it can take up to 200us when a high priority thread moves to the READY
	 *   state before it changes to the RUNNING state.
	 * - sceKernelStartThread is always resuming the thread dispatching.
	 *
	 * Thread scheduling on pspsharp:
	 * - the rules for moving between states are implemented in hleChangeThreadState()
	 * - the rules for choosing the thread in the RUNNING state are implemented in
	 *   hleRescheduleCurrentThread()
	 * - the clock precision for interrupting a RUNNING thread is about 1000 microseconds.
	 *   This is due to a restriction of the Java timers used by the Thread.sleep() methods.
	 *   Even the Thread.sleep(millis, nanos) seems to have the same restriction
	 *   (at least on windows).
	 * - preemptive scheduling is implemented in RuntimeContext by a separate
	 *   Java thread (RuntimeSyncThread). This thread sets the flag RuntimeContext.wantSync
	 *   when a scheduler action has to be executed. This flag is checked by the compiled
	 *   code at each back branch (i.e. a branch to a lower address, usually a loop).
	 *
	 * Test application:
	 * - taskScheduler.prx: testing the scheduler rules between threads having higher,
	 *                      lower or the same priority.
	 *                      The clock precision of 200us on the PSP can be observed here.
	 */
	public class ThreadManForUser : HLEModule
	{
		public static Logger log = Modules.getLogger("ThreadManForUser");

		private Dictionary<int, SceKernelThreadInfo> threadMap;
		private Dictionary<int, SceKernelThreadEventHandlerInfo> threadEventHandlers;
		private LinkedList<SceKernelThreadInfo> readyThreads;
		private SceKernelThreadInfo currentThread;
		private SceKernelThreadInfo idle0, idle1;
		public Statistics statistics;
		private bool dispatchThreadEnabled;
		private const int SCE_KERNEL_DISPATCHTHREAD_STATE_DISABLED = 0;
		private const int SCE_KERNEL_DISPATCHTHREAD_STATE_ENABLED = 1;
		private const string rootThreadName = "root";

		// The PSP seems to have a resolution of 200us
		protected internal const int THREAD_DELAY_MINIMUM_MICROS = 200;

		protected internal static readonly int CALLBACKID_REGISTER = _s0;
		protected internal CallbackManager callbackManager = new CallbackManager();
		public const int INTERNAL_THREAD_ADDRESS_START = MemoryMap.START_RAM;
		protected internal const int IDLE_THREAD_ADDRESS = INTERNAL_THREAD_ADDRESS_START;
		public static readonly int THREAD_EXIT_HANDLER_ADDRESS = INTERNAL_THREAD_ADDRESS_START + 0x10;
		public static readonly int CALLBACK_EXIT_HANDLER_ADDRESS = INTERNAL_THREAD_ADDRESS_START + 0x20;
		public static readonly int ASYNC_LOOP_ADDRESS = INTERNAL_THREAD_ADDRESS_START + 0x30;
		public static readonly int NET_APCTL_LOOP_ADDRESS = INTERNAL_THREAD_ADDRESS_START + 0x40;
		public static readonly int NET_ADHOC_MATCHING_EVENT_LOOP_ADDRESS = INTERNAL_THREAD_ADDRESS_START + 0x50;
		public static readonly int NET_ADHOC_MATCHING_INPUT_LOOP_ADDRESS = INTERNAL_THREAD_ADDRESS_START + 0x60;
		public static readonly int NET_ADHOC_CTL_LOOP_ADDRESS = INTERNAL_THREAD_ADDRESS_START + 0x70;
		public static readonly int UTILITY_LOOP_ADDRESS = INTERNAL_THREAD_ADDRESS_START + 0x80;
		public static readonly int WLAN_SEND_CALLBACK_ADDRESS = INTERNAL_THREAD_ADDRESS_START + 0x90;
		public static readonly int WLAN_UP_CALLBACK_ADDRESS = INTERNAL_THREAD_ADDRESS_START + 0xA0;
		public static readonly int WLAN_DOWN_CALLBACK_ADDRESS = INTERNAL_THREAD_ADDRESS_START + 0xB0;
		public static readonly int WLAN_IOCTL_CALLBACK_ADDRESS = INTERNAL_THREAD_ADDRESS_START + 0xC0;
		public static readonly int WLAN_LOOP_ADDRESS = INTERNAL_THREAD_ADDRESS_START + 0xD0;
		public static readonly int INTERNAL_THREAD_ADDRESS_END = INTERNAL_THREAD_ADDRESS_START + 0xE0;
		public static readonly int INTERNAL_THREAD_ADDRESS_SIZE = INTERNAL_THREAD_ADDRESS_END - INTERNAL_THREAD_ADDRESS_START;
		private Dictionary<int, pspBaseCallback> callbackMap;
		private const bool LOG_CONTEXT_SWITCHING = true;
		private const bool LOG_INSTRUCTIONS = false;
		public bool exitCalled = false;
		private int freeInternalUserMemoryStart;
		private int freeInternalUserMemoryEnd;

		// see sceKernelGetThreadmanIdList
		public const int SCE_KERNEL_TMID_Thread = 1;
		public const int SCE_KERNEL_TMID_Semaphore = 2;
		public const int SCE_KERNEL_TMID_EventFlag = 3;
		public const int SCE_KERNEL_TMID_Mbox = 4;
		public const int SCE_KERNEL_TMID_Vpl = 5;
		public const int SCE_KERNEL_TMID_Fpl = 6;
		public const int SCE_KERNEL_TMID_Mpipe = 7;
		public const int SCE_KERNEL_TMID_Callback = 8;
		public const int SCE_KERNEL_TMID_ThreadEventHandler = 9;
		public const int SCE_KERNEL_TMID_Alarm = 10;
		public const int SCE_KERNEL_TMID_VTimer = 11;
		public const int SCE_KERNEL_TMID_Mutex = 12;
		public const int SCE_KERNEL_TMID_LwMutex = 13;
		public const int SCE_KERNEL_TMID_SleepThread = 64;
		public const int SCE_KERNEL_TMID_DelayThread = 65;
		public const int SCE_KERNEL_TMID_SuspendThread = 66;
		public const int SCE_KERNEL_TMID_DormantThread = 67;
		protected internal const int INTR_NUMBER = IntrManager.PSP_SYSTIMER0_INTR;
		protected internal IDictionary<int, SceKernelAlarmInfo> alarms;
		protected internal IDictionary<int, SceKernelVTimerInfo> vtimers;
		protected internal bool needThreadReschedule;
		protected internal WaitThreadEndWaitStateChecker waitThreadEndWaitStateChecker;
		protected internal TimeoutThreadWaitStateChecker timeoutThreadWaitStateChecker;
		protected internal SleepThreadWaitStateChecker sleepThreadWaitStateChecker;

		public const int PSP_ATTR_ADDR_HIGH = 0x4000;
		protected internal Dictionary<int, SceKernelTls> tlsMap;


		public class Statistics
		{
			internal List<ThreadStatistics> threads = new List<ThreadStatistics>();
			public long allCycles = 0;
			public long startTimeMillis;
			public long endTimeMillis;
			public long allCpuMillis = 0;

			public Statistics()
			{
				startTimeMillis = DateTimeHelper.CurrentUnixTimeMillis();
			}

			public virtual void exit()
			{
				endTimeMillis = DateTimeHelper.CurrentUnixTimeMillis();
			}

			public virtual long DurationMillis
			{
				get
				{
					return endTimeMillis - startTimeMillis;
				}
			}

			internal virtual void addThreadStatistics(SceKernelThreadInfo thread)
			{
				if (!DurationStatistics.collectStatistics)
				{
					return;
				}

				ThreadStatistics threadStatistics = new ThreadStatistics();
				threadStatistics.name = thread.name;
				threadStatistics.runClocks = thread.runClocks;
				threads.Add(threadStatistics);

				allCycles += thread.runClocks;

				if (thread.javaThreadId > 0)
				{
					ThreadMXBean threadMXBean = ManagementFactory.ThreadMXBean;
					if (threadMXBean.ThreadCpuTimeEnabled)
					{
						long threadCpuTimeNanos = thread.javaThreadCpuTimeNanos;
						if (threadCpuTimeNanos < 0)
						{
							threadCpuTimeNanos = threadMXBean.getThreadCpuTime(thread.javaThreadId);
						}
						if (threadCpuTimeNanos > 0)
						{
							allCpuMillis += threadCpuTimeNanos / 1000000L;
						}
					}
				}
			}

			private class ThreadStatistics : IComparable<ThreadStatistics>
			{
				public string name;
				public long runClocks;

				public virtual int CompareTo(ThreadStatistics o)
				{
					return -((new long?(runClocks)).compareTo(o.runClocks));
				}

				public virtual string QuotedName
				{
					get
					{
						return "'" + name + "'";
					}
				}
			}
		}

		public class CallbackManager
		{
			internal IDictionary<int, Callback> callbacks;
			internal int currentCallbackId;

			public virtual void Initialize()
			{
				callbacks = new Dictionary<int, Callback>();
				currentCallbackId = 1;
			}

			public virtual void addCallback(Callback callback)
			{
				callbacks[callback.Id] = callback;
			}

			public virtual Callback remove(int id)
			{
				Callback callback = callbacks.Remove(id);

				return callback;
			}

			public virtual int NewCallbackId
			{
				get
				{
					return currentCallbackId++;
				}
			}
		}

		public class Callback
		{
			internal int id;
			internal int address;
			internal int[] parameters;
			internal int savedIdRegister;
			internal int savedRa;
			internal int savedPc;
			internal int savedV0;
			internal int savedV1;
			internal IAction afterAction;
			internal bool returnVoid;
			internal bool preserveCpuState;
			internal CpuState savedCpuState;

			public Callback(int id, int address, int[] parameters, IAction afterAction, bool returnVoid, bool preserveCpuState)
			{
				this.id = id;
				this.address = address;
				this.parameters = parameters;
				this.afterAction = afterAction;
				this.returnVoid = returnVoid;
				this.preserveCpuState = preserveCpuState;
			}

			public virtual int Id
			{
				get
				{
					return id;
				}
			}

			public virtual void execute(SceKernelThreadInfo thread)
			{
				CpuState cpu = thread.cpuContext;

				savedIdRegister = cpu.getRegister(CALLBACKID_REGISTER);
				savedRa = cpu._ra;
				savedPc = cpu.pc;
				savedV0 = cpu._v0;
				savedV1 = cpu._v1;
				if (preserveCpuState)
				{
					savedCpuState = new CpuState(cpu);
				}

				// Copy parameters ($a0, $a1, ...) to the cpu
				if (parameters != null)
				{
					for (int i = 0; i < parameters.Length; i++)
					{
						cpu.setRegister(_a0 + i, parameters[i]);
					}
				}

				cpu.setRegister(CALLBACKID_REGISTER, id);
				cpu._ra = CALLBACK_EXIT_HANDLER_ADDRESS;
				cpu.pc = address;

				RuntimeContext.executeCallback();
			}

			public virtual void executeExit(CpuState cpu)
			{
				cpu.setRegister(CALLBACKID_REGISTER, savedIdRegister);
				cpu._ra = savedRa;
				cpu.pc = savedPc;

				if (afterAction != null)
				{
					afterAction.execute();
				}

				if (preserveCpuState)
				{
					cpu.copy(savedCpuState);
				}
				else
				{
					// Do we need to restore $v0/$v1?
					if (returnVoid)
					{
						cpu._v0 = savedV0;
						cpu._v1 = savedV1;
					}
				}
			}

			public virtual IAction AfterAction
			{
				set
				{
					this.afterAction = value;
				}
			}

			public override string ToString()
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("Callback address=0x%08X, id=%d, returnVoid=%b, preserveCpuState=%b", address, getId(), returnVoid, preserveCpuState);
				return string.Format("Callback address=0x%08X, id=%d, returnVoid=%b, preserveCpuState=%b", address, Id, returnVoid, preserveCpuState);
			}
		}

		private class AfterCallAction : IAction
		{
			private readonly ThreadManForUser outerInstance;

			internal SceKernelThreadInfo thread;
			internal int status;
			internal int waitType;
			internal int waitId;
			internal ThreadWaitInfo threadWaitInfo;
			internal bool doCallbacks;
			internal IAction afterAction;

			public AfterCallAction(ThreadManForUser outerInstance, SceKernelThreadInfo thread, IAction afterAction)
			{
				this.outerInstance = outerInstance;
				this.thread = thread;
				status = thread.status;
				waitType = thread.waitType;
				waitId = thread.waitId;
				threadWaitInfo = new ThreadWaitInfo(thread.wait);
				doCallbacks = thread.doCallbacks;
				this.afterAction = afterAction;
			}

			public virtual void execute()
			{
				bool restoreWaitState = true;

				// After calling a callback, check if the waiting state of the thread
				// is still valid, i.e. if the thread must continue to wait or if the
				// wait condition has been reached.
				if (threadWaitInfo.waitStateChecker != null)
				{
					if (!threadWaitInfo.waitStateChecker.continueWaitState(thread, threadWaitInfo))
					{
						restoreWaitState = false;
					}
				}

				if (restoreWaitState)
				{
					if (status == PSP_THREAD_RUNNING)
					{
						doCallbacks = false;
					}
					if (log.DebugEnabled)
					{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("AfterCallAction: restoring wait state for thread '%s' to %s, %s, doCallbacks %b", thread.toString(), pspsharp.HLE.kernel.types.SceKernelThreadInfo.getStatusName(status), pspsharp.HLE.kernel.types.SceKernelThreadInfo.getWaitName(waitType, waitId, threadWaitInfo, status), doCallbacks));
						log.debug(string.Format("AfterCallAction: restoring wait state for thread '%s' to %s, %s, doCallbacks %b", thread.ToString(), SceKernelThreadInfo.getStatusName(status), SceKernelThreadInfo.getWaitName(waitType, waitId, threadWaitInfo, status), doCallbacks));
					}

					// Restore the wait state of the thread
					thread.waitType = waitType;
					thread.waitId = waitId;
					thread.wait.copy(threadWaitInfo);

					outerInstance.hleChangeThreadState(thread, status);
				}
				else if (thread.Running)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("AfterCallAction: leaving thread in RUNNING state: {0}", thread));
					}
					doCallbacks = false;
				}
				else if (!thread.Ready)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("AfterCallAction: set thread to READY state: {0}", thread));
					}

					outerInstance.hleChangeThreadState(thread, PSP_THREAD_READY);
					doCallbacks = false;
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("AfterCallAction: leaving thread in READY state: {0}", thread));
					}
					doCallbacks = false;
				}

				thread.doCallbacks = doCallbacks;
				outerInstance.hleRescheduleCurrentThread();

				if (afterAction != null)
				{
					afterAction.execute();
				}
			}
		}

		public class TimeoutThreadAction : IAction
		{
			private readonly ThreadManForUser outerInstance;

			internal SceKernelThreadInfo thread;

			public TimeoutThreadAction(ThreadManForUser outerInstance, SceKernelThreadInfo thread)
			{
				this.outerInstance = outerInstance;
				this.thread = thread;
			}

			public virtual void execute()
			{
				outerInstance.hleThreadWaitTimeout(thread);
			}
		}

		public class TimeoutThreadWaitStateChecker : IWaitStateChecker
		{
			private readonly ThreadManForUser outerInstance;

			public TimeoutThreadWaitStateChecker(ThreadManForUser outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				// Waiting forever?
				if (wait.forever)
				{
					return true;
				}

				if (wait.microTimeTimeout <= Emulator.Clock.microTime())
				{
					outerInstance.hleThreadWaitTimeout(thread);
					return false;
				}

				// The waitTimeoutAction has been deleted by hleChangeThreadState while
				// leaving the WAIT state. It has to be restored.
				if (wait.waitTimeoutAction == null)
				{
					wait.waitTimeoutAction = new TimeoutThreadAction(outerInstance, thread);
				}

				return true;
			}
		}

		public class DeleteThreadAction : IAction
		{
			private readonly ThreadManForUser outerInstance;

			internal SceKernelThreadInfo thread;

			public DeleteThreadAction(ThreadManForUser outerInstance, SceKernelThreadInfo thread)
			{
				this.outerInstance = outerInstance;
				this.thread = thread;
			}

			public virtual void execute()
			{
				outerInstance.hleDeleteThread(thread);
			}
		}

		public class WaitThreadEndWaitStateChecker : IWaitStateChecker
		{
			private readonly ThreadManForUser outerInstance;

			public WaitThreadEndWaitStateChecker(ThreadManForUser outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				// Check if the thread has to continue its wait state or if the thread
				// has exited during the callback execution.
				SceKernelThreadInfo threadEnd = outerInstance.getThreadById(wait.ThreadEnd_id);
				if (threadEnd == null)
				{
					// The thread has completely disappeared during the callback execution...
					thread.cpuContext._v0 = ERROR_KERNEL_NOT_FOUND_THREAD;
					return false;
				}

				if (threadEnd.Stopped)
				{
					// Return exit status of stopped thread
					thread.cpuContext._v0 = threadEnd.exitStatus;
					return false;
				}

				return true;
			}
		}

		public class SleepThreadWaitStateChecker : IWaitStateChecker
		{
			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				if (thread.wakeupCount > 0)
				{
					// sceKernelWakeupThread() has been called while the thread was waiting
					thread.wakeupCount--;
					// Return 0
					thread.cpuContext._v0 = 0;
					return false;
				}

				return true;
			}
		}

		/// <summary>
		/// A callback is deleted when its return value is non-zero.
		/// </summary>
		private class CheckCallbackReturnValue : IAction
		{
			private readonly ThreadManForUser outerInstance;

			internal SceKernelThreadInfo thread;
			internal int callbackUid;

			public CheckCallbackReturnValue(ThreadManForUser outerInstance, SceKernelThreadInfo thread, int callbackUid)
			{
				this.outerInstance = outerInstance;
				this.thread = thread;
				this.callbackUid = callbackUid;
			}

			public virtual void execute()
			{
				int callbackReturnValue = thread.cpuContext._v0;
				if (callbackReturnValue != 0)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Callback uid=0x{0:X} has returned value 0x{1:X8}: deleting the callback", callbackUid, callbackReturnValue));
					}
					outerInstance.hleKernelDeleteCallback(callbackUid);
				}
			}
		}

		private class AfterSceKernelExtendThreadStackAction : IAction
		{
			internal SceKernelThreadInfo thread;
			internal int savedPc;
			internal int savedSp;
			internal int savedRa;
			internal int returnValue;
			internal SysMemInfo extendedStackSysMemInfo;

			public AfterSceKernelExtendThreadStackAction(SceKernelThreadInfo thread, int savedPc, int savedSp, int savedRa, SysMemInfo extendedStackSysMemInfo)
			{
				this.thread = thread;
				this.savedPc = savedPc;
				this.savedSp = savedSp;
				this.savedRa = savedRa;
				this.extendedStackSysMemInfo = extendedStackSysMemInfo;
			}

			public virtual void execute()
			{
				CpuState cpu = Emulator.Processor.cpu;

				if (log.DebugEnabled)
				{
					log.debug(string.Format("AfterSceKernelExtendThreadStackAction savedSp=0x{0:X8}, savedRa=0x{1:X8}, $v0=0x{2:X8}", savedSp, savedRa, cpu._v0));
				}

				cpu.pc = savedPc;
				cpu._sp = savedSp;
				cpu._ra = savedRa;
				returnValue = cpu._v0;

				// The return value in $v0 of the entryAdd is passed back as return value
				// of sceKernelExtendThreadStack.
				thread.freeExtendedStack(extendedStackSysMemInfo);
			}

			public virtual int ReturnValue
			{
				get
				{
					return returnValue;
				}
			}
		}

		public ThreadManForUser()
		{
		}

		public override void start()
		{
			currentThread = null;
			threadMap = new Dictionary<int, SceKernelThreadInfo>();
			threadEventHandlers = new Dictionary<int, SceKernelThreadEventHandlerInfo>();
			readyThreads = new LinkedList<SceKernelThreadInfo>();
			statistics = new Statistics();

			callbackMap = new Dictionary<int, pspBaseCallback>();
			callbackManager.Initialize();

			reserveInternalMemory();
			installIdleThreads();
			installThreadExitHandler();
			installCallbackExitHandler();
			installAsyncLoopHandler();
			installNetApctlLoopHandler();
			installNetAdhocMatchingEventLoopHandler();
			installNetAdhocMatchingInputLoopHandler();
			installNetAdhocCtlLoopHandler();
			installUtilityLoopHandler();
			installWlanSendCallback();
			installWlanUpCallback();
			installWlanDownCallback();
			installWlanIoctlCallback();
			installWlanLoopHandler();

			alarms = new Dictionary<int, SceKernelAlarmInfo>();
			vtimers = new Dictionary<int, SceKernelVTimerInfo>();

			dispatchThreadEnabled = true;
			needThreadReschedule = true;

			ThreadMXBean threadMXBean = ManagementFactory.ThreadMXBean;
			if (threadMXBean.ThreadCpuTimeSupported)
			{
				threadMXBean.ThreadCpuTimeEnabled = true;
			}

			waitThreadEndWaitStateChecker = new WaitThreadEndWaitStateChecker(this);
			timeoutThreadWaitStateChecker = new TimeoutThreadWaitStateChecker(this);
			sleepThreadWaitStateChecker = new SleepThreadWaitStateChecker();

			tlsMap = new Dictionary<int, SceKernelTls>();

			base.start();
		}

		public override void stop()
		{
			alarms = null;
			vtimers = null;
			foreach (SceKernelThreadInfo thread in threadMap.Values)
			{
				terminateThread(thread);
			}

			base.stop();
		}

		public virtual IEnumerator<SceKernelThreadInfo> iterator()
		{
			return threadMap.Values.GetEnumerator();
		}

		public virtual IEnumerator<SceKernelThreadInfo> iteratorByPriority()
		{
			Dictionary<int, SceKernelThreadInfo>.ValueCollection c = threadMap.Values;
			IList<SceKernelThreadInfo> list = new LinkedList<SceKernelThreadInfo>(c);
			list.Sort(idle0); // We need an instance of SceKernelThreadInfo for the comparator, so we use idle0
			return list.GetEnumerator();
		}

		public virtual SceKernelThreadInfo getRootThread(SceModule module)
		{
			if (threadMap != null)
			{
				foreach (SceKernelThreadInfo thread in threadMap.Values)
				{
					if (rootThreadName.Equals(thread.name) && (module == null || thread.moduleid == module.modid))
					{
						return thread;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// call this when resetting the emulator </summary>
		/// <param name="entry_addr"> entry from ELF header </param>
		/// <param name="attr"> from sceModuleInfo ELF section header  </param>
		public virtual void Initialise(SceModule module, int entry_addr, int attr, string pspfilename, int moduleid, int gp, bool fromSyscall)
		{
			// Create a thread the program will run inside

			// The stack size seems to be 0x40000 when starting the application from the VSH
			// and smaller when starting the application with sceKernelLoadExec() - guess: 0x8000.
			// This could not be reproduced on a PSP.
			int rootStackSize = (fromSyscall ? 0x8000 : 0x40000);
			// Use the module_start_thread_stacksize when this information was present in the ELF file
			if (module != null && module.module_start_thread_stacksize > 0)
			{
				rootStackSize = module.module_start_thread_stacksize;
			}

			// For a kernel module, the stack is allocated in the kernel partition
			int rootMpidStack = module != null && module.mpiddata > 0 ? module.mpiddata : USER_PARTITION_ID;

			int rootInitPriority = 0x20;
			// Use the module_start_thread_priority when this information was present in the ELF file
			if (module != null && module.module_start_thread_priority > 0)
			{
				rootInitPriority = module.module_start_thread_priority;
			}
			SceKernelThreadInfo rootThread = new SceKernelThreadInfo(rootThreadName, entry_addr, rootInitPriority, rootStackSize, attr, rootMpidStack);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Creating root thread: uid=0x{0:X}, entry=0x{1:X8}, priority={2:D}, stackSize=0x{3:X}, attr=0x{4:X}", rootThread.uid, entry_addr, rootInitPriority, rootStackSize, attr));
			}
			rootThread.moduleid = moduleid;
			threadMap[rootThread.uid] = rootThread;

			// Set user mode bit if kernel mode bit is not present
			if (!rootThread.KernelMode)
			{
				rootThread.attr |= PSP_THREAD_ATTR_USER;
			}

			// Setup args by copying them onto the stack
			hleKernelSetThreadArguments(rootThread, pspfilename);

			// Setup threads $gp
			rootThread.cpuContext._gp = gp;
			idle0.cpuContext._gp = gp;
			idle1.cpuContext._gp = gp;

			hleChangeThreadState(rootThread, PSP_THREAD_READY);

			hleRescheduleCurrentThread();
		}

		public virtual void hleKernelSetThreadArguments(SceKernelThreadInfo thread, string argument)
		{
			int address = prepareThreadArguments(thread, argument.Length + 1);
			writeStringZ(Memory.Instance, address, argument);
		}

		public virtual void hleKernelSetThreadArguments(SceKernelThreadInfo thread, sbyte[] argument, int argumentSize)
		{
			int address = prepareThreadArguments(thread, argumentSize);
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(address, argumentSize, 1);
			for (int i = 0; i < argumentSize; i++)
			{
				memoryWriter.writeNext(argument[i] & 0xFF);
			}
			memoryWriter.flush();
		}

		public virtual void hleKernelSetThreadArguments(SceKernelThreadInfo thread, int argumentAddr, int argumentSize)
		{
			int address = prepareThreadArguments(thread, argumentAddr == 0 ? -1 : argumentSize);
			if (argumentAddr != 0)
			{
				Memory.Instance.memcpy(address, argumentAddr, argumentSize);
			}
		}

		private int prepareThreadArguments(SceKernelThreadInfo thread, int argumentSize)
		{
			// 256 bytes padding between user data top and real stack top
			int address = (thread.StackAddr + thread.stackSize - 0x100) - ((argumentSize + 0xF) & ~0xF);
			if (argumentSize < 0)
			{
				// Set the pointer to NULL when none is provided
				thread.cpuContext._a0 = 0; // a0 = user data len
				thread.cpuContext._a1 = 0; // a1 = pointer to arg data in stack
			}
			else
			{
				thread.cpuContext._a0 = argumentSize; // a0 = user data len
				thread.cpuContext._a1 = address; // a1 = pointer to arg data in stack
			}
			// 64 bytes padding between program stack top and user data
			thread.cpuContext._sp = address - 0x40;

			return address;
		}

		public static int NOP()
		{
			// sll $zr, $zr, 0 <=> nop
			return (AllegrexOpcodes.SLL << 26) | (_zr << 16) | (_zr << 11) | (0 << 6);
		}

		public static int MOVE(int rd, int rs)
		{
			// addu rd, rs, $zr <=> move rd, rs
			return AllegrexOpcodes.ADDU | (rd << 11) | (_zr << 16) | (rs << 21);
		}

		public static int LUI(int rd, int imm16)
		{
			return (AllegrexOpcodes.LUI << 26) | (rd << 16) | (imm16 & 0xFFFF);
		}

		public static int ADDIU(int rt, int rs, int imm16)
		{
			return (AllegrexOpcodes.ADDIU << 26) | (rs << 21) | (rt << 16) | (imm16 & 0xFFFF);
		}

		public static int ORI(int rt, int rs, int imm16)
		{
			return (AllegrexOpcodes.ORI << 26) | (rs << 21) | (rt << 16) | (imm16 & 0xFFFF);
		}

		public static int SW(int rt, int @base, int imm16)
		{
			return (AllegrexOpcodes.SW << 26) | (@base << 21) | (rt << 16) | (imm16 & 0xFFFF);
		}

		public static int SB(int rt, int rs, int imm16)
		{
			return (AllegrexOpcodes.SB << 26) | (rs << 21) | (rt << 16) | (imm16 & 0xFFFF);
		}

		public static int LW(int rt, int @base, int imm16)
		{
			return (AllegrexOpcodes.LW << 26) | (@base << 21) | (rt << 16) | (imm16 & 0xFFFF);
		}

		public static int JAL(int address)
		{
			return (AllegrexOpcodes.JAL << 26) | ((address >> 2) & 0x03FFFFFF);
		}

		public static int J(int address)
		{
			return (AllegrexOpcodes.J << 26) | ((address >> 2) & 0x03FFFFFF);
		}

		public static int SYSCALL(int syscallCode)
		{
			return (AllegrexOpcodes.SPECIAL << 26) | AllegrexOpcodes.SYSCALL | (syscallCode << 6);
		}

		public static int SYSCALL(HLEModule hleModule, string functionName)
		{
			HLEModuleFunction hleModuleFunction = hleModule.getHleFunctionByName(functionName);
			if (hleModuleFunction == null)
			{
				return SYSCALL(SyscallHandler.syscallUnmappedImport);
			}

			// syscall [functionName]
			return SYSCALL(hleModuleFunction.SyscallCode);
		}

		private int SYSCALL(string functionName)
		{
			return SYSCALL(this, functionName);
		}

		public static int JR()
		{
			// jr $ra
			return (AllegrexOpcodes.SPECIAL << 26) | AllegrexOpcodes.JR | (_ra << 21);
		}

		public static int B(int destination)
		{
			// beq $zr, $zr, destination <=> b destination
			return (AllegrexOpcodes.BEQ << 26) | (_zr << 21) | (_zr << 16) | (destination & 0x0000FFFF);
		}

		private void reserveInternalMemory()
		{
			// Reserve the memory used by the internal handlers
			SysMemInfo internalMemInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "ThreadMan-InternalHandlers", SysMemUserForUser.PSP_SMEM_Addr, INTERNAL_THREAD_ADDRESS_SIZE, INTERNAL_THREAD_ADDRESS_START);
			if (internalMemInfo == null)
			{
				log.error(string.Format("Cannot reserve internal memory at 0x{0:X8}", INTERNAL_THREAD_ADDRESS_START));
			}

			// This memory is always reserved on a real PSP
			int internalUserMemorySize = 0x4000;
			SysMemInfo rootMemInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.USER_PARTITION_ID, "ThreadMan-RootMem", SysMemUserForUser.PSP_SMEM_Addr, internalUserMemorySize, MemoryMap.START_USERSPACE);
			freeInternalUserMemoryStart = rootMemInfo.addr;
			freeInternalUserMemoryEnd = freeInternalUserMemoryStart + internalUserMemorySize;
		}

		public virtual int allocateInternalUserMemory(int size)
		{
			// Align on a multiple of 4 bytes
			size = alignUp(size, 3);

			if (freeInternalUserMemoryStart + size > freeInternalUserMemoryEnd)
			{
				log.error(string.Format("allocateInternalUserMemory not enough free memory available, requested size=0x{0:X}, available size=0x{1:X}", size, freeInternalUserMemoryEnd - freeInternalUserMemoryStart));
				return 0;
			}

			int allocatedMem = freeInternalUserMemoryStart;
			freeInternalUserMemoryStart += size;

			return allocatedMem;
		}

		/// <summary>
		/// Generate 2 idle threads which can toggle between each other when there are no ready threads
		/// </summary>
		private void installIdleThreads()
		{
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(IDLE_THREAD_ADDRESS, 0x20, 4);
			memoryWriter.writeNext(MOVE(_a0, _zr));
			memoryWriter.writeNext(B(-2));
			memoryWriter.writeNext(SYSCALL("sceKernelDelayThread"));
			memoryWriter.flush();

			int idleThreadStackSize = 0x1000;
			// Lowest allowed priority is 0x77, so we are fine at 0x7F.
			// Allocate a stack because interrupts can be processed by the
			// idle thread, using its stack.
			// The stack is allocated into the reservedMem area.
			idle0 = new SceKernelThreadInfo("idle0", IDLE_THREAD_ADDRESS | unchecked((int)0x80000000), 0x7F, 0, PSP_THREAD_ATTR_KERNEL, KERNEL_PARTITION_ID);
			idle0.setSystemStack(allocateInternalUserMemory(idleThreadStackSize), idleThreadStackSize);
			idle0.reset();
			idle0.exitStatus = ERROR_KERNEL_THREAD_IS_NOT_DORMANT;
			threadMap[idle0.uid] = idle0;
			hleChangeThreadState(idle0, PSP_THREAD_READY);

			idle1 = new SceKernelThreadInfo("idle1", IDLE_THREAD_ADDRESS | unchecked((int)0x80000000), 0x7F, 0, PSP_THREAD_ATTR_KERNEL, KERNEL_PARTITION_ID);
			idle1.setSystemStack(allocateInternalUserMemory(idleThreadStackSize), idleThreadStackSize);
			idle1.reset();
			idle1.exitStatus = ERROR_KERNEL_THREAD_IS_NOT_DORMANT;
			threadMap[idle1.uid] = idle1;
			hleChangeThreadState(idle1, PSP_THREAD_READY);
		}

		private void installThreadExitHandler()
		{
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(THREAD_EXIT_HANDLER_ADDRESS, 0x10, 4);
			memoryWriter.writeNext(MOVE(_a0, _v0));
			memoryWriter.writeNext(JR());
			memoryWriter.writeNext(SYSCALL("hleKernelExitThread"));
			memoryWriter.flush();
		}

		public static void installHLESyscall(int address, HLEModule hleModule, string name)
		{
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(address, 12, 4);
			memoryWriter.writeNext(JR());
			memoryWriter.writeNext(SYSCALL(hleModule, name));
			memoryWriter.flush();
		}

		private void installHLESyscall(int address, string name)
		{
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(address, 12, 4);
			memoryWriter.writeNext(JR());
			memoryWriter.writeNext(SYSCALL(name));
			memoryWriter.flush();
		}

		private void installCallbackExitHandler()
		{
			installHLESyscall(CALLBACK_EXIT_HANDLER_ADDRESS, "hleKernelExitCallback");
		}

		private void installLoopHandler(string hleFunctionName, int address)
		{
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(address, 0x20, 4);
			memoryWriter.writeNext(B(-1));
			memoryWriter.writeNext(SYSCALL(hleFunctionName));
			memoryWriter.flush();
		}

		private void installAsyncLoopHandler()
		{
			installLoopHandler("hleKernelAsyncLoop", ASYNC_LOOP_ADDRESS);
		}

		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual void hleKernelAsyncLoop(Processor processor)
		{
			Modules.IoFileMgrForUserModule.hleAsyncThread(processor);
		}

		private void installNetApctlLoopHandler()
		{
			installLoopHandler("hleKernelNetApctlLoop", NET_APCTL_LOOP_ADDRESS);
		}

		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual void hleKernelNetApctlLoop(Processor processor)
		{
			Modules.sceNetApctlModule.hleNetApctlThread(processor);
		}

		private void installNetAdhocMatchingEventLoopHandler()
		{
			installLoopHandler("hleKernelNetAdhocMatchingEventLoop", NET_ADHOC_MATCHING_EVENT_LOOP_ADDRESS);
		}

		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual void hleKernelNetAdhocMatchingEventLoop(Processor processor)
		{
			Modules.sceNetAdhocMatchingModule.hleNetAdhocMatchingEventThread(processor);
		}

		private void installNetAdhocMatchingInputLoopHandler()
		{
			installLoopHandler("hleKernelNetAdhocMatchingInputLoop", NET_ADHOC_MATCHING_INPUT_LOOP_ADDRESS);
		}

		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual void hleKernelNetAdhocMatchingInputLoop(Processor processor)
		{
			Modules.sceNetAdhocMatchingModule.hleNetAdhocMatchingInputThread(processor);
		}

		private void installNetAdhocCtlLoopHandler()
		{
			installLoopHandler("hleKernelNetAdhocctlLoop", NET_ADHOC_CTL_LOOP_ADDRESS);
		}

		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual void hleUtilityLoop(Processor processor)
		{
			Modules.sceUtilityModule.hleUtilityThread(processor);
		}

		private void installUtilityLoopHandler()
		{
			installLoopHandler("hleUtilityLoop", UTILITY_LOOP_ADDRESS);
		}

		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual void hleKernelNetAdhocctlLoop(Processor processor)
		{
			Modules.sceNetAdhocctlModule.hleNetAdhocctlThread(processor);
		}

		private void installWlanSendCallback()
		{
			installHLESyscall(WLAN_SEND_CALLBACK_ADDRESS, "hleWlanSendCallback");
		}

		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleWlanSendCallback(TPointer handleAddr)
		{
			return Modules.sceWlanModule.hleWlanSendCallback(handleAddr);
		}

		private void installWlanUpCallback()
		{
			installHLESyscall(WLAN_UP_CALLBACK_ADDRESS, "hleWlanUpCallback");
		}

		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleWlanUpCallback(TPointer handleAddr)
		{
			return Modules.sceWlanModule.hleWlanUpCallback(handleAddr);
		}

		private void installWlanDownCallback()
		{
			installHLESyscall(WLAN_DOWN_CALLBACK_ADDRESS, "hleWlanDownCallback");
		}

		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleWlanDownCallback(TPointer handleAddr)
		{
			return Modules.sceWlanModule.hleWlanDownCallback(handleAddr);
		}

		private void installWlanIoctlCallback()
		{
			installHLESyscall(WLAN_IOCTL_CALLBACK_ADDRESS, "hleWlanIoctlCallback");
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = HLESyscallNid, version = 150) public int hleWlanIoctlCallback(pspsharp.HLE.TPointer handleAddr, int cmd, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=32, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer unknown, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=8, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 buffersAddr)
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleWlanIoctlCallback(TPointer handleAddr, int cmd, TPointer unknown, TPointer32 buffersAddr)
		{
			return Modules.sceWlanModule.hleWlanIoctlCallback(handleAddr, cmd, unknown, buffersAddr);
		}

		private void installWlanLoopHandler()
		{
			installLoopHandler("hleKernelWlanLoop", WLAN_LOOP_ADDRESS);
		}

		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual void hleKernelWlanLoop()
		{
			Modules.sceWlanModule.hleWlanThread();
		}

		/// <summary>
		/// to be called when exiting the emulation </summary>
		public virtual void exit()
		{
			exitCalled = true;

			if (threadMap != null)
			{
				// Delete all the threads to collect statistics
				deleteAllThreads();

				log.info("----------------------------- ThreadMan exit -----------------------------");

				if (DurationStatistics.collectStatistics)
				{
					statistics.exit();
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.info(String.format("ThreadMan Statistics (%,d cycles in %.3fs):", statistics.allCycles, statistics.getDurationMillis() / 1000.0));
					log.info(string.Format("ThreadMan Statistics (%,d cycles in %.3fs):", statistics.allCycles, statistics.DurationMillis / 1000.0));
					statistics.threads.Sort();
					foreach (Statistics.ThreadStatistics threadStatistics in statistics.threads)
					{
						double percentage = 0;
						if (statistics.allCycles != 0)
						{
							percentage = (threadStatistics.runClocks / (double) statistics.allCycles) * 100;
						}
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.info(String.format("    Thread %-30s %,12d cycles (%5.2f%%)", threadStatistics.getQuotedName(), threadStatistics.runClocks, percentage));
						log.info(string.Format("    Thread %-30s %,12d cycles (%5.2f%%)", threadStatistics.QuotedName, threadStatistics.runClocks, percentage));
					}
				}
			}
		}

		/// <summary>
		/// To be called from the main emulation loop
		///  This is only used when running in interpreter mode,
		///  i.e. it is no longer used when the Compiler is enabled.
		/// </summary>
		public virtual void step()
		{
			if (LOG_INSTRUCTIONS)
			{
				if (log.TraceEnabled)
				{
					CpuState cpu = Emulator.Processor.cpu;

					if (!isIdleThread(currentThread) && cpu.pc != 0)
					{
						int address = cpu.pc - 4;
						int opcode = Memory.Instance.read32(address);
						log.trace(string.Format("Executing {0:X8} {1}", address, Decoder.instruction(opcode).disasm(address, opcode)));
					}
				}
			}

			if (currentThread != null)
			{
				currentThread.runClocks++;
			}
			else if (!exitCalled)
			{
				// We always need to be in a thread! we shouldn't get here.
				log.error("No ready threads!");
			}
		}

		private void internalContextSwitch(SceKernelThreadInfo newThread)
		{
			if (currentThread != null)
			{
				// Switch out old thread
				if (currentThread.status == PSP_THREAD_RUNNING)
				{
					hleChangeThreadState(currentThread, PSP_THREAD_READY);
				}

				// save registers
				currentThread.saveContext();
			}

			if (newThread != null)
			{
				// Switch in new thread
				hleChangeThreadState(newThread, PSP_THREAD_RUNNING);
				// restore registers
				newThread.restoreContext();

				if (LOG_CONTEXT_SWITCHING && log.DebugEnabled && !isIdleThread(newThread))
				{
					log.debug(string.Format("----- {0}, now={1:D}", newThread, Emulator.Clock.microTime()));
				}
			}
			else
			{
				// When running under compiler mode this gets triggered by exit()
				if (!exitCalled)
				{
					DumpDebugState.dumpDebugState();

					log.info("No ready threads - pausing emulator. caller:" + CallingFunction);
					Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_UNKNOWN);
				}
			}

			currentThread = newThread;

			RuntimeContext.update();
		}

		/// <param name="newThread"> The thread to switch in. </param>
		private bool contextSwitch(SceKernelThreadInfo newThread)
		{
			if (IntrManager.Instance.InsideInterrupt)
			{
				// No context switching inside an interrupt
				if (log.DebugEnabled)
				{
					log.debug("Inside an interrupt, not context switching to " + newThread);
				}
				return false;
			}

			if (Emulator.Processor.InterruptsDisabled)
			{
				// No context switching when interrupts are disabled
				if (log.DebugEnabled)
				{
					log.debug("Interrupts are disabled, not context switching to " + newThread);
				}
				return false;
			}

			if (!dispatchThreadEnabled)
			{
				log.info("DispatchThread disabled, not context switching to " + newThread);
				return false;
			}

			internalContextSwitch(newThread);

			checkThreadCallbacks(currentThread);

			executePendingCallbacks(currentThread);

			return true;
		}

		private void executePendingCallbacks(SceKernelThreadInfo thread)
		{
			if (thread.pendingCallbacks.Count > 0)
			{
				if (RuntimeContext.canExecuteCallback(thread))
				{
					Callback callback = thread.pendingCallbacks.RemoveFirst();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Executing pending callback '{0}' for thread '{1}'", callback, thread));
					}
					callback.execute(thread);
				}
			}
		}

		public virtual void checkPendingCallbacks()
		{
			executePendingCallbacks(currentThread);
		}

		private bool executePendingActions(SceKernelThreadInfo thread)
		{
			bool actionExecuted = false;
			if (thread.pendingActions.Count > 0)
			{
				if (currentThread == thread)
				{
					IAction action = thread.pendingActions.RemoveFirst();
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Executing pending action '{0}' for thread '{1}'", action, thread));
					}
					action.execute();

					actionExecuted = true;
				}
			}

			return actionExecuted;
		}

		public virtual bool checkPendingActions()
		{
			return executePendingActions(currentThread);
		}

		public virtual void pushActionForThread(SceKernelThreadInfo thread, IAction action)
		{
			thread.pendingActions.AddLast(action);
		}

		/// <summary>
		/// This function must have the property of never returning currentThread,
		/// unless currentThread is already null. </summary>
		/// <returns> The next thread to schedule (based on thread priorities).  </returns>
		private SceKernelThreadInfo nextThread()
		{
			// Find the thread with status PSP_THREAD_READY and the highest priority.
			// In this implementation low priority threads can get starved.
			// Remark: the currentThread is not present in the readyThreads List.
			SceKernelThreadInfo found = null;
			lock (readyThreads)
			{
				foreach (SceKernelThreadInfo thread in readyThreads)
				{
					if (found == null || thread.currentPriority < found.currentPriority)
					{
						found = thread;
					}
				}
			}
			return found;
		}

		/// <summary>
		/// Switch to the thread with status PSP_THREAD_READY and having the highest priority.
		/// If the current thread is in status PSP_THREAD_READY and
		/// still having the highest priority, nothing is changed.
		/// If the current thread is having the same priority as the highest priority,
		/// nothing is changed (no yielding to threads having the same priority).
		/// </summary>
		public virtual void hleRescheduleCurrentThread()
		{
			if (needThreadReschedule)
			{
				SceKernelThreadInfo newThread = nextThread();
				if (newThread != null && (currentThread == null || currentThread.status != PSP_THREAD_RUNNING || currentThread.currentPriority > newThread.currentPriority))
				{
					if (LOG_CONTEXT_SWITCHING && Modules.log.DebugEnabled)
					{
						log.debug("Context switching to '" + newThread + "' after reschedule");
					}

					if (contextSwitch(newThread))
					{
						needThreadReschedule = false;
					}
				}
				else
				{
					needThreadReschedule = false;
				}
			}
		}

		/// <summary>
		/// Same behavior as hleRescheduleCurrentThread()
		/// excepted that it executes callbacks when doCallbacks == true
		/// </summary>
		public virtual void hleRescheduleCurrentThread(bool doCallbacks)
		{
			SceKernelThreadInfo thread = currentThread;
			if (doCallbacks)
			{
				if (thread != null)
				{
					thread.doCallbacks = doCallbacks;
				}
				checkCallbacks();
			}

			hleRescheduleCurrentThread();

			if (currentThread == thread && doCallbacks)
			{
				if (thread.Running)
				{
					thread.doCallbacks = false;
				}
			}
		}

		public virtual void hleYieldCurrentThread()
		{
			hleKernelDelayThread(100, false);
		}

		public virtual int CurrentThreadID
		{
			get
			{
				if (currentThread == null)
				{
					return -1;
				}
    
				return currentThread.uid;
			}
		}

		public virtual SceKernelThreadInfo CurrentThread
		{
			get
			{
				return currentThread;
			}
		}

		public virtual bool isIdleThread(SceKernelThreadInfo thread)
		{
			return (thread == idle0 || thread == idle1);
		}

		public virtual bool isIdleThread(int uid)
		{
			return uid == idle0.uid || uid == idle1.uid;
		}

		public virtual bool KernelMode
		{
			get
			{
				// Running code in the kernel memory area?
				int pc = RuntimeContext.Pc & addressMask;
				if (pc >= (START_KERNEL & addressMask) && pc <= (END_KERNEL & addressMask))
				{
					return true;
				}
    
				return currentThread.KernelMode;
			}
		}

		public virtual string getThreadName(int uid)
		{
			SceKernelThreadInfo thread = threadMap[uid];
			if (thread == null)
			{
				return "NOT A THREAD";
			}
			return thread.name;
		}

		public virtual bool DispatchThreadEnabled
		{
			get
			{
				return dispatchThreadEnabled;
			}
		}

		public virtual SceKernelCallbackInfo getCallbackInfo(int uid)
		{
			pspBaseCallback callback = callbackMap[uid];
			if (callback != null && callback is SceKernelCallbackInfo)
			{
				return (SceKernelCallbackInfo) callback;
			}

			return null;
		}

		public virtual bool isCurrentThreadStackAddress(int address)
		{
			return currentThread.isStackAddress(address & Memory.addressMask);
		}

		/// <summary>
		/// Enter the current thread in a wait state.
		/// </summary>
		/// <param name="waitType">         the wait type (one of SceKernelThreadInfo.PSP_WAIT_xxx) </param>
		/// <param name="waitId">           the uid of the wait object </param>
		/// <param name="waitStateChecker"> this wait state checked will be called after the
		///                         execution of a callback in the waiting thread to check
		///                         if the thread has to return to its wait state (i.e. if
		///                         the wait condition is still valid). </param>
		/// <param name="timeoutAddr">      0 when the thread is waiting forever
		///                         otherwise, a valid address containing a timeout value
		///                         in microseconds. </param>
		/// <param name="callbacks">        true if callback can be executed while waiting.
		///                         false if callback cannot be execute while waiting. </param>
		public virtual void hleKernelThreadEnterWaitState(int waitType, int waitId, IWaitStateChecker waitStateChecker, int timeoutAddr, bool callbacks)
		{
			hleKernelThreadEnterWaitState(currentThread, waitType, waitId, waitStateChecker, timeoutAddr, callbacks);
		}

		/// <summary>
		/// Enter a thread in a wait state.
		/// </summary>
		/// <param name="thread">           the thread entering the wait state </param>
		/// <param name="waitType">         the wait type (one of SceKernelThreadInfo.PSP_WAIT_xxx) </param>
		/// <param name="waitId">           the uid of the wait object </param>
		/// <param name="waitStateChecker"> this wait state checked will be called after the
		///                         execution of a callback in the waiting thread to check
		///                         if the thread has to return to its wait state (i.e. if
		///                         the wait condition is still valid). </param>
		/// <param name="timeoutAddr">      0 when the thread is waiting forever
		///                         otherwise, a valid address containing a timeout value
		///                         in microseconds. </param>
		/// <param name="callbacks">        true if callback can be executed while waiting.
		///                         false if callback cannot be execute while waiting. </param>
		public virtual void hleKernelThreadEnterWaitState(SceKernelThreadInfo thread, int waitType, int waitId, IWaitStateChecker waitStateChecker, int timeoutAddr, bool callbacks)
		{
			int micros = 0;
			bool forever = true;
			if (Memory.isAddressGood(timeoutAddr))
			{
				micros = Memory.Instance.read32(timeoutAddr);
				forever = false;
			}
			hleKernelThreadEnterWaitState(thread, waitType, waitId, waitStateChecker, micros, forever, callbacks);
		}

		/// <summary>
		/// Enter the current thread in a wait state.
		/// The thread will wait without timeout, i.e. forever.
		/// </summary>
		/// <param name="waitType">         the wait type (one of SceKernelThreadInfo.PSP_WAIT_xxx) </param>
		/// <param name="waitId">           the uid of the wait object </param>
		/// <param name="waitStateChecker"> this wait state checked will be called after the
		///                         execution of a callback in the waiting thread to check
		///                         if the thread has to return to its wait state (i.e. if
		///                         the wait condition is still valid). </param>
		/// <param name="callbacks">        true if callback can be executed while waiting.
		///                         false if callback cannot be execute while waiting. </param>
		public virtual void hleKernelThreadEnterWaitState(int waitType, int waitId, IWaitStateChecker waitStateChecker, bool callbacks)
		{
			hleKernelThreadEnterWaitState(currentThread, waitType, waitId, waitStateChecker, 0, true, callbacks);
		}

		/// <summary>
		/// Enter a thread in a wait state.
		/// </summary>
		/// <param name="thread">           the thread entering the wait state </param>
		/// <param name="waitType">         the wait type (one of SceKernelThreadInfo.PSP_WAIT_xxx) </param>
		/// <param name="waitId">           the uid of the wait object </param>
		/// <param name="waitStateChecker"> this wait state checked will be called after the
		///                         execution of a callback in the waiting thread to check
		///                         if the thread has to return to its wait state (i.e. if
		///                         the wait condition is still valid). </param>
		/// <param name="micros">           a timeout value in microseconds, only relevant
		///                         when the "forever" parameter is false. </param>
		/// <param name="forever">          true when the thread is waiting without a timeout,
		///                         false when the thread is waiting with a timeout
		///                         (see the "micros" parameter). </param>
		/// <param name="callbacks">        true if callback can be executed while waiting.
		///                         false if callback cannot be execute while waiting. </param>
		public virtual void hleKernelThreadEnterWaitState(SceKernelThreadInfo thread, int waitType, int waitId, IWaitStateChecker waitStateChecker, int micros, bool forever, bool callbacks)
		{
			// wait state
			thread.waitType = waitType;
			thread.waitId = waitId;
			thread.wait.waitStateChecker = waitStateChecker;

			// Go to wait state
			hleKernelThreadWait(thread, micros, forever);
			hleChangeThreadState(thread, PSP_THREAD_WAITING);
			hleRescheduleCurrentThread(callbacks);
		}

		public virtual void hleBlockThread(SceKernelThreadInfo thread, int waitType, int waitId, bool doCallbacks, IAction onUnblockAction, IWaitStateChecker waitStateChecker)
		{
			if (!thread.Waiting)
			{
				thread.doCallbacks = doCallbacks;
				thread.wait.onUnblockAction = onUnblockAction;
				thread.waitType = waitType;
				thread.waitId = waitId;
				thread.wait.waitStateChecker = waitStateChecker;
				thread.wait.forever = true;
				hleChangeThreadState(thread, thread.Suspended ? PSP_THREAD_WAITING_SUSPEND : PSP_THREAD_WAITING);
			}

			hleRescheduleCurrentThread(doCallbacks);
		}

		public virtual void hleBlockCurrentThread(int waitType, int waitId, bool doCallbacks, IAction onUnblockAction, IWaitStateChecker waitStateChecker)
		{
			if (LOG_CONTEXT_SWITCHING && Modules.log.DebugEnabled)
			{
				log.debug("-------------------- block SceUID=" + currentThread.uid.ToString("x") + " name:'" + currentThread.name + "' caller:" + CallingFunction);
			}

			hleBlockThread(currentThread, waitType, waitId, doCallbacks, onUnblockAction, waitStateChecker);
		}

		public virtual void hleBlockCurrentThread(int waitType)
		{
			hleBlockCurrentThread(waitType, 0, false, null, null);
		}

		public virtual void hleBlockCurrentThread(int waitType, IAction onUnblockAction)
		{
			hleBlockCurrentThread(waitType, 0, false, onUnblockAction, null);
		}

		public virtual SceKernelThreadInfo getThreadById(int uid)
		{
			return threadMap[uid];
		}

		public virtual SceKernelThreadInfo getThreadByName(string name)
		{
			foreach (SceKernelThreadInfo thread in threadMap.Values)
			{
				if (name.Equals(thread.name))
				{
					return thread;
				}
			}

			return null;
		}

		public virtual void hleUnblockThread(int uid)
		{
			if (SceUidManager.checkUidPurpose(uid, "ThreadMan-thread", false))
			{
				SceKernelThreadInfo thread = threadMap[uid];
				// Remove PSP_THREAD_WAITING from the thread state,
				// i.e. change the thread state
				// - from PSP_THREAD_WAITING_SUSPEND to PSP_THREAD_SUSPEND
				// - from PSP_THREAD_WAITING to PSP_THREAD_READY
				hleChangeThreadState(thread, thread.Suspended ? PSP_THREAD_SUSPEND : PSP_THREAD_READY);

				if (LOG_CONTEXT_SWITCHING && thread != null && Modules.log.DebugEnabled)
				{
					log.debug("-------------------- unblock SceUID=" + thread.uid.ToString("x") + " name:'" + thread.name + "' caller:" + CallingFunction);
				}
			}
		}

		private string CallingFunction
		{
			get
			{
				string msg = "";
				StackTraceElement[] lines = (new Exception()).StackTrace;
				if (lines.Length >= 3)
				{
					msg = lines[2].ToString();
					msg = msg.Substring(0, msg.IndexOf("(", StringComparison.Ordinal));
					//msg = "'" + msg.Substring(msg.lastIndexOf(".") + 1, msg.length()) + "'";
					string[] parts = msg.Split("\\.", true);
					msg = "'" + parts[parts.Length - 2] + "." + parts[parts.Length - 1] + "'";
				}
				else
				{
					foreach (StackTraceElement e in lines)
					{
						string line = e.ToString();
						if (line.StartsWith("pspsharp.Allegrex", StringComparison.Ordinal) || line.StartsWith("pspsharp.Processor", StringComparison.Ordinal))
						{
							break;
						}
						msg += "\n" + line;
					}
				}
				return msg;
			}
		}

		public virtual void hleThreadWaitTimeout(SceKernelThreadInfo thread)
		{
			if (thread.waitType == PSP_WAIT_NONE)
			{
				// The thread is no longer waiting...
			}
			else
			{
				onWaitTimeout(thread);

				// Remove PSP_THREAD_WAITING from the thread state,
				// i.e. change the thread state
				// - from PSP_THREAD_WAITING_SUSPEND to PSP_THREAD_SUSPEND
				// - from PSP_THREAD_WAITING to PSP_THREAD_READY
				hleChangeThreadState(thread, thread.Suspended ? PSP_THREAD_SUSPEND : PSP_THREAD_READY);
			}
		}

		/// <summary>
		/// Call this when a thread's wait timeout has expired.
		/// You can assume the calling function will set thread.status = ready. 
		/// </summary>
		private void onWaitTimeout(SceKernelThreadInfo thread)
		{
			switch (thread.waitType)
			{
				case PSP_WAIT_THREAD_END:
					// Return WAIT_TIMEOUT
					if (thread.wait.ThreadEnd_returnExitStatus)
					{
						thread.cpuContext._v0 = ERROR_KERNEL_WAIT_TIMEOUT;
					}
					break;
				case PSP_WAIT_EVENTFLAG:
					Managers.eventFlags.onThreadWaitTimeout(thread);
					break;
				case PSP_WAIT_SEMA:
					Managers.semas.onThreadWaitTimeout(thread);
					break;
				case JPCSP_WAIT_UMD:
					Modules.sceUmdUserModule.onThreadWaitTimeout(thread);
					break;
				case PSP_WAIT_MUTEX:
					Managers.mutex.onThreadWaitTimeout(thread);
					break;
				case PSP_WAIT_LWMUTEX:
					Managers.lwmutex.onThreadWaitTimeout(thread);
					break;
				case PSP_WAIT_MSGPIPE:
					Managers.msgPipes.onThreadWaitTimeout(thread);
					break;
				case PSP_WAIT_MBX:
					Managers.mbx.onThreadWaitTimeout(thread);
					break;
				case PSP_WAIT_FPL:
					Managers.fpl.onThreadWaitTimeout(thread);
					break;
				case PSP_WAIT_VPL:
					Managers.vpl.onThreadWaitTimeout(thread);
					break;
			}
		}

		private void hleThreadWaitRelease(SceKernelThreadInfo thread)
		{
			// Thread was in a WAITING SUSPEND state?
			if (thread.Suspended)
			{
				// Go back to the SUSPEND state
				hleChangeThreadState(thread, PSP_THREAD_SUSPEND);
			}
			else if (thread.waitType != PSP_WAIT_NONE)
			{
				onWaitReleased(thread);
				hleChangeThreadState(thread, PSP_THREAD_READY);
			}
		}

		/// <summary>
		/// Call this when a thread's wait has been released. </summary>
		private void onWaitReleased(SceKernelThreadInfo thread)
		{
			switch (thread.waitType)
			{
				case PSP_WAIT_THREAD_END:
					// Return ERROR_WAIT_STATUS_RELEASED
					if (thread.wait.ThreadEnd_returnExitStatus)
					{
						thread.cpuContext._v0 = ERROR_KERNEL_WAIT_STATUS_RELEASED;
					}
					break;
				case PSP_WAIT_EVENTFLAG:
					Managers.eventFlags.onThreadWaitReleased(thread);
					break;
				case PSP_WAIT_SEMA:
					Managers.semas.onThreadWaitReleased(thread);
					break;
				case JPCSP_WAIT_UMD:
					Modules.sceUmdUserModule.onThreadWaitReleased(thread);
					break;
				case PSP_WAIT_MUTEX:
					Managers.mutex.onThreadWaitReleased(thread);
					break;
				case PSP_WAIT_LWMUTEX:
					Managers.lwmutex.onThreadWaitReleased(thread);
					break;
				case PSP_WAIT_MSGPIPE:
					Managers.msgPipes.onThreadWaitReleased(thread);
					break;
				case PSP_WAIT_MBX:
					Managers.mbx.onThreadWaitReleased(thread);
					break;
				case PSP_WAIT_FPL:
					Managers.fpl.onThreadWaitReleased(thread);
					break;
				case PSP_WAIT_VPL:
					Managers.vpl.onThreadWaitReleased(thread);
					break;
				case JPCSP_WAIT_GE_LIST:
				case JPCSP_WAIT_NET:
				case JPCSP_WAIT_AUDIO:
				case JPCSP_WAIT_DISPLAY_VBLANK:
				case JPCSP_WAIT_CTRL:
				case JPCSP_WAIT_USB:
					thread.cpuContext._v0 = ERROR_KERNEL_WAIT_STATUS_RELEASED;
					break;
			}
		}

		private void deleteAllThreads()
		{
			// Copy the list of threads into a new list to avoid ConcurrentModificationException
			IList<SceKernelThreadInfo> threadsToBeDeleted = null;
			do
			{
				try
				{
					threadsToBeDeleted = new LinkedList<SceKernelThreadInfo>(threadMap.Values);
				}
				catch (ConcurrentModificationException)
				{
					// Exception occurred in LinkedList.addAll() method, retry
					threadsToBeDeleted = null;
				}
			} while (threadsToBeDeleted == null);

			foreach (SceKernelThreadInfo thread in threadsToBeDeleted)
			{
				hleDeleteThread(thread);
			}
		}

		public virtual void hleDeleteThread(SceKernelThreadInfo thread)
		{
			if (!threadMap.ContainsKey(thread.uid))
			{
				log.debug(string.Format("Thread {0} already deleted", thread.ToString()));
				return;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("really deleting thread:'{0}'", thread.name));
			}

			// cleanup thread - free the stack
			if (log.DebugEnabled)
			{
				log.debug(string.Format("thread:'{0}' freeing stack 0x{1:X8}", thread.name, thread.StackAddr));
			}
			thread.freeStack();

			Managers.eventFlags.onThreadDeleted(thread);
			Managers.semas.onThreadDeleted(thread);
			Managers.mutex.onThreadDeleted(thread);
			Managers.lwmutex.onThreadDeleted(thread);
			Managers.msgPipes.onThreadDeleted(thread);
			Managers.mbx.onThreadDeleted(thread);
			Managers.fpl.onThreadDeleted(thread);
			Managers.vpl.onThreadDeleted(thread);
			Modules.sceUmdUserModule.onThreadDeleted(thread);
			RuntimeContext.onThreadDeleted(thread);

			cancelThreadWait(thread);
			threadMap.Remove(thread.uid);
			if (thread.unloadModuleAtDeletion)
			{
				SceModule module = Managers.modules.getModuleByUID(thread.moduleid);
				if (module != null)
				{
					module.stop();
					module.unload();
				}
			}
			SceUidManager.releaseUid(thread.uid, "ThreadMan-thread");

			statistics.addThreadStatistics(thread);
		}

		private void removeFromReadyThreads(SceKernelThreadInfo thread)
		{
			lock (readyThreads)
			{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET LinkedList equivalent to the Java 'remove' method:
				readyThreads.remove(thread);
				needThreadReschedule = true;
			}
		}

		private void addToReadyThreads(SceKernelThreadInfo thread, bool addFirst)
		{
			lock (readyThreads)
			{
				if (addFirst)
				{
					readyThreads.AddFirst(thread);
				}
				else
				{
					readyThreads.AddLast(thread);
				}
				needThreadReschedule = true;
			}
		}

		private SceKernelThreadInfo ToBeDeletedThread
		{
			set
			{
				value.doDelete = true;
    
				if (value.Stopped)
				{
					// It's possible for a game to request the same value to be deleted multiple times.
					// We only mark for deferred deletion.
					// Example:
					// - main value calls sceKernelDeleteThread on child value
					// - child value calls sceKernelExitDeleteThread
					if (value.doDeleteAction == null)
					{
						value.doDeleteAction = new DeleteThreadAction(this, value);
						Scheduler.Instance.addAction(value.doDeleteAction);
					}
				}
			}
		}

		private void triggerThreadEvent(SceKernelThreadInfo thread, SceKernelThreadInfo contextThread, int @event)
		{
			foreach (SceKernelThreadEventHandlerInfo handler in threadEventHandlers.Values)
			{
				if (handler.appliesFor(CurrentThread, thread, @event))
				{
					handler.triggerThreadEventHandler(contextThread, @event);
				}
			}
		}

		public virtual void hleKernelChangeThreadPriority(SceKernelThreadInfo thread, int newPriority)
		{
			if (thread == null)
			{
				return;
			}

			thread.currentPriority = newPriority;
			if (thread.Running)
			{
				// The current thread will be moved to the front of the ready queue
				hleChangeThreadState(thread, PSP_THREAD_READY);
			}

			if (thread.Ready)
			{
				// A ready thread is yielding when changing priority and moved to the end of the ready thread list.
				if (log.DebugEnabled)
				{
					log.debug("hleKernelChangeThreadPriority rescheduling ready thread");
				}
				removeFromReadyThreads(thread);
				addToReadyThreads(thread, false);
				needThreadReschedule = true;
				hleRescheduleCurrentThread();
			}
		}

		/// <summary>
		/// Change to state of a thread.
		/// This function must be used when changing the state of a thread as
		/// it updates the ThreadMan internal data structures and implements
		/// the PSP behavior on status change.
		/// </summary>
		/// <param name="thread">    the thread to be updated </param>
		/// <param name="newStatus"> the new thread status </param>
		public virtual void hleChangeThreadState(SceKernelThreadInfo thread, int newStatus)
		{
			if (thread == null)
			{
				return;
			}

			if (thread.status == newStatus)
			{
				// Thread status not changed, nothing to do
				return;
			}

			if (!dispatchThreadEnabled && thread == currentThread && newStatus != PSP_THREAD_RUNNING)
			{
				log.info("DispatchThread disabled, not changing thread state of " + thread + " to " + newStatus);
				return;
			}

			bool addReadyThreadsFirst = false;

			// Moving out of the following states...
			if (thread.status == PSP_THREAD_WAITING && newStatus != PSP_THREAD_WAITING_SUSPEND)
			{
				if (thread.wait.waitTimeoutAction != null)
				{
					Scheduler.Instance.removeAction(thread.wait.microTimeTimeout, thread.wait.waitTimeoutAction);
					thread.wait.waitTimeoutAction = null;
				}
				if (thread.wait.onUnblockAction != null)
				{
					thread.wait.onUnblockAction.execute();
					thread.wait.onUnblockAction = null;
				}
				thread.doCallbacks = false;
			}
			else if (thread.Stopped)
			{
				if (thread.doDeleteAction != null)
				{
					Scheduler.Instance.removeAction(0, thread.doDeleteAction);
					thread.doDeleteAction = null;
				}
			}
			else if (thread.Ready)
			{
				removeFromReadyThreads(thread);
			}
			else if (thread.Suspended)
			{
				thread.doCallbacks = false;
			}
			else if (thread.Running)
			{
				needThreadReschedule = true;
				// When a running thread has to yield to a thread having a higher
				// priority, the thread stays in front of the ready threads having
				// the same priority (no yielding to threads having the same priority).
				addReadyThreadsFirst = true;
			}

			thread.status = newStatus;

			// Moving to the following states...
			if (thread.status == PSP_THREAD_WAITING)
			{
				if (thread.wait.waitTimeoutAction != null)
				{
					Scheduler.Instance.addAction(thread.wait.microTimeTimeout, thread.wait.waitTimeoutAction);
				}

				// debug
				if (thread.waitType == PSP_WAIT_NONE)
				{
					log.warn("changeThreadState thread '" + thread.name + "' => PSP_THREAD_WAITING. waitType should NOT be PSP_WAIT_NONE. caller:" + CallingFunction);
				}
			}
			else if (thread.Stopped)
			{
				// HACK auto delete module mgr threads
				if (thread.name.Equals(rootThreadName) || thread.name.Equals("SceModmgrStart") || thread.name.Equals("SceModmgrStop"))
				{
					thread.doDelete = true;
				}

				if (thread.doDelete)
				{
					if (thread.doDeleteAction == null)
					{
						thread.doDeleteAction = new DeleteThreadAction(this, thread);
						Scheduler.Instance.addAction(0, thread.doDeleteAction);
					}
				}
				onThreadStopped(thread);
			}
			else if (thread.Ready)
			{
				addToReadyThreads(thread, addReadyThreadsFirst);
				thread.waitType = PSP_WAIT_NONE;
				thread.wait.waitTimeoutAction = null;
				thread.wait.waitStateChecker = null;
				thread.wait.onUnblockAction = null;
				thread.doCallbacks = false;
			}
			else if (thread.Running)
			{
				// debug
				if (thread.waitType != PSP_WAIT_NONE && !isIdleThread(thread))
				{
					log.error(string.Format("changeThreadState thread {0} => PSP_THREAD_RUNNING. waitType should be PSP_WAIT_NONE. caller: {1}", thread, CallingFunction));
				}
			}
		}

		private void cancelThreadWait(SceKernelThreadInfo thread)
		{
			// Cancel all waiting actions
			thread.wait.onUnblockAction = null;
			thread.wait.waitStateChecker = null;
			thread.waitType = PSP_WAIT_NONE;
			if (thread.wait.waitTimeoutAction != null)
			{
				Scheduler.Instance.removeAction(thread.wait.microTimeTimeout, thread.wait.waitTimeoutAction);
				thread.wait.waitTimeoutAction = null;
			}
		}

		private void terminateThread(SceKernelThreadInfo thread)
		{
			hleChangeThreadState(thread, PSP_THREAD_STOPPED); // PSP_THREAD_STOPPED (checked)
			cancelThreadWait(thread);
			RuntimeContext.onThreadExit(thread);
			if (thread == currentThread)
			{
				hleRescheduleCurrentThread();
			}
		}

		private void onThreadStopped(SceKernelThreadInfo stoppedThread)
		{
			foreach (SceKernelThreadInfo thread in threadMap.Values)
			{
				// Wakeup threads that are in sceKernelWaitThreadEnd
				// We're assuming if waitingOnThreadEnd is set then thread.status = waiting
				if (thread.isWaitingForType(PSP_WAIT_THREAD_END) && thread.wait.ThreadEnd_id == stoppedThread.uid)
				{
					hleThreadWaitRelease(thread);
					if (thread.wait.ThreadEnd_returnExitStatus)
					{
						// Return exit status of stopped thread
						thread.cpuContext._v0 = stoppedThread.exitStatus;
					}
				}
			}
		}

		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual void hleKernelExitCallback(Processor processor)
		{
			CpuState cpu = processor.cpu;

			int callbackId = cpu.getRegister(CALLBACKID_REGISTER);
			Callback callback = callbackManager.remove(callbackId);
			if (callback != null)
			{
				if (log.TraceEnabled)
				{
					log.trace("End of callback " + callback);
				}
				callback.executeExit(cpu);
			}
		}

		/// <summary>
		/// Execute the code at the given address.
		/// The code is executed in the context of the currentThread.
		/// Parameters ($a0, $a1, ...) may have been copied to the current CpuState
		/// before calling this method.
		/// This call can return before the completion of the code. Use the
		/// "afterAction" parameter to trigger some actions that need to be executed
		/// after the code (e.g. to evaluate a return value in $v0).
		/// </summary>
		/// <param name="address">     the address to be called </param>
		/// <param name="afterAction"> the action to be executed after the completion of the code </param>
		/// <param name="returnVoid">  the code has a void return value, i.e. $v0/$v1 have to be restored </param>
		public virtual void callAddress(int address, IAction afterAction, bool returnVoid)
		{
			callAddress(null, address, afterAction, returnVoid, false, null);
		}

		private void callAddress(SceKernelThreadInfo thread, int address, IAction afterAction, bool returnVoid, bool preserveCpuState, int[] parameters)
		{
			if (thread != null)
			{
				// Save the wait state of the thread to restore it after the call
				afterAction = new AfterCallAction(this, thread, afterAction);

				// Terminate the thread wait state
				thread.waitType = PSP_WAIT_NONE;
				thread.wait.onUnblockAction = null;

				hleChangeThreadState(thread, PSP_THREAD_READY);
			}

			int callbackId = callbackManager.NewCallbackId;
			Callback callback = new Callback(callbackId, address, parameters, afterAction, returnVoid, preserveCpuState);

			callbackManager.addCallback(callback);

			bool callbackCalled = false;
			if (thread == null || thread == currentThread)
			{
				if (RuntimeContext.canExecuteCallback(thread))
				{
					thread = currentThread;

					if (thread.waitType != PSP_WAIT_NONE)
					{
						afterAction = new AfterCallAction(this, thread, afterAction);
						callback.AfterAction = afterAction;

						// Terminate the thread wait state
						thread.waitType = PSP_WAIT_NONE;
					}

					hleChangeThreadState(thread, PSP_THREAD_RUNNING);
					callback.execute(thread);
					callbackCalled = true;
				}
			}

			if (!callbackCalled)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Pushing pending callback '{0}' for thread '{1}'", callback, thread));
				}
				thread.pendingCallbacks.AddLast(callback);
			}
		}

		/// <summary>
		/// Trigger a call to a callback in the context of a thread.
		/// This call can return before the completion of the callback. Use the
		/// "afterAction" parameter to trigger some actions that need to be executed
		/// after the callback (e.g. to evaluate a return value in $v0).
		/// </summary>
		/// <param name="thread">      the callback has to be executed by this thread (null means the currentThread) </param>
		/// <param name="address">     address of the callback </param>
		/// <param name="afterAction"> action to be executed after the completion of the callback </param>
		/// <param name="returnVoid">  the callback has a void return value, i.e. $v0/$v1 have to be restored </param>
		public virtual void executeCallback(SceKernelThreadInfo thread, int address, IAction afterAction, bool returnVoid)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("Execute callback 0x%08X, afterAction=%s, returnVoid=%b", address, afterAction, returnVoid));
				log.debug(string.Format("Execute callback 0x%08X, afterAction=%s, returnVoid=%b", address, afterAction, returnVoid));
			}

			callAddress(thread, address, afterAction, returnVoid, false, null);
		}

		/// <summary>
		/// Trigger a call to a callback in the context of a thread.
		/// This call can return before the completion of the callback. Use the
		/// "afterAction" parameter to trigger some actions that need to be executed
		/// after the callback (e.g. to evaluate a return value in cpu.gpr[2]).
		/// </summary>
		/// <param name="thread">      the callback has to be executed by this thread (null means the currentThread) </param>
		/// <param name="address">     address of the callback </param>
		/// <param name="afterAction"> action to be executed after the completion of the callback </param>
		/// <param name="returnVoid">  the callback has a void return value, i.e. $v0/$v1 have to be restored </param>
		/// <param name="registerA0">  first parameter of the callback ($a0) </param>
		public virtual void executeCallback(SceKernelThreadInfo thread, int address, IAction afterAction, bool returnVoid, int registerA0)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("Execute callback 0x%08X($a0=0x%08X), afterAction=%s, returnVoid=%b", address, registerA0, afterAction, returnVoid));
				log.debug(string.Format("Execute callback 0x%08X($a0=0x%08X), afterAction=%s, returnVoid=%b", address, registerA0, afterAction, returnVoid));
			}

			callAddress(thread, address, afterAction, returnVoid, false, new int[]{registerA0});
		}

		/// <summary>
		/// Trigger a call to a callback in the context of a thread.
		/// This call can return before the completion of the callback. Use the
		/// "afterAction" parameter to trigger some actions that need to be executed
		/// after the callback (e.g. to evaluate a return value in cpu.gpr[2]).
		/// </summary>
		/// <param name="thread">      the callback has to be executed by this thread (null means the currentThread) </param>
		/// <param name="address">     address of the callback </param>
		/// <param name="afterAction"> action to be executed after the completion of the callback </param>
		/// <param name="returnVoid">  the callback has a void return value, i.e. $v0/$v1 have to be restored </param>
		/// <param name="registerA0">  first parameter of the callback ($a0) </param>
		/// <param name="registerA1">  second parameter of the callback ($a1) </param>
		public virtual void executeCallback(SceKernelThreadInfo thread, int address, IAction afterAction, bool returnVoid, int registerA0, int registerA1)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("Execute callback 0x%08X($a0=0x%08X, $a1=0x%08X), afterAction=%s, returnVoid=%b", address, registerA0, registerA1, afterAction, returnVoid));
				log.debug(string.Format("Execute callback 0x%08X($a0=0x%08X, $a1=0x%08X), afterAction=%s, returnVoid=%b", address, registerA0, registerA1, afterAction, returnVoid));
			}

			callAddress(thread, address, afterAction, returnVoid, false, new int[]{registerA0, registerA1});
		}

		/// <summary>
		/// Trigger a call to a callback in the context of a thread.
		/// This call can return before the completion of the callback. Use the
		/// "afterAction" parameter to trigger some actions that need to be executed
		/// after the callback (e.g. to evaluate a return value in cpu.gpr[2]).
		/// </summary>
		/// <param name="thread">      the callback has to be executed by this thread (null means the currentThread) </param>
		/// <param name="address">     address of the callback </param>
		/// <param name="afterAction"> action to be executed after the completion of the callback </param>
		/// <param name="returnVoid">  the callback has a void return value, i.e. $v0/$v1 have to be restored </param>
		/// <param name="registerA0">  first parameter of the callback ($a0) </param>
		/// <param name="registerA1">  second parameter of the callback ($a1) </param>
		/// <param name="registerA2">  third parameter of the callback ($a2) </param>
		public virtual void executeCallback(SceKernelThreadInfo thread, int address, IAction afterAction, bool returnVoid, int registerA0, int registerA1, int registerA2)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("Execute callback 0x%08X($a0=0x%08X, $a1=0x%08X, $a2=0x%08X), afterAction=%s, returnVoid=%b", address, registerA0, registerA1, registerA2, afterAction, returnVoid));
				log.debug(string.Format("Execute callback 0x%08X($a0=0x%08X, $a1=0x%08X, $a2=0x%08X), afterAction=%s, returnVoid=%b", address, registerA0, registerA1, registerA2, afterAction, returnVoid));
			}

			callAddress(thread, address, afterAction, returnVoid, false, new int[]{registerA0, registerA1, registerA2});
		}

		/// <summary>
		/// Trigger a call to a callback in the context of a thread.
		/// This call can return before the completion of the callback. Use the
		/// "afterAction" parameter to trigger some actions that need to be executed
		/// after the callback (e.g. to evaluate a return value in cpu.gpr[2]).
		/// </summary>
		/// <param name="thread">      the callback has to be executed by this thread (null means the currentThread) </param>
		/// <param name="address">     address of the callback </param>
		/// <param name="afterAction"> action to be executed after the completion of the callback </param>
		/// <param name="returnVoid">  the callback has a void return value, i.e. $v0/$v1 have to be restored </param>
		/// <param name="preserverCpuState"> preserve the complete CpuState while executing the callback.
		///                    All the registers will be restored after the callback execution. </param>
		/// <param name="registerA0">  first parameter of the callback ($a0) </param>
		/// <param name="registerA1">  second parameter of the callback ($a1) </param>
		/// <param name="registerA2">  third parameter of the callback ($a2) </param>
		public virtual void executeCallback(SceKernelThreadInfo thread, int address, IAction afterAction, bool returnVoid, bool preserverCpuState, int registerA0, int registerA1, int registerA2)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("Execute callback 0x%08X($a0=0x%08X, $a1=0x%08X, $a2=0x%08X), afterAction=%s, returnVoid=%b, preserverCpuState=%b", address, registerA0, registerA1, registerA2, afterAction, returnVoid, preserverCpuState));
				log.debug(string.Format("Execute callback 0x%08X($a0=0x%08X, $a1=0x%08X, $a2=0x%08X), afterAction=%s, returnVoid=%b, preserverCpuState=%b", address, registerA0, registerA1, registerA2, afterAction, returnVoid, preserverCpuState));
			}

			callAddress(thread, address, afterAction, returnVoid, preserverCpuState, new int[]{registerA0, registerA1, registerA2});
		}

		/// <summary>
		/// Trigger a call to a callback in the context of a thread.
		/// This call can return before the completion of the callback. Use the
		/// "afterAction" parameter to trigger some actions that need to be executed
		/// after the callback (e.g. to evaluate a return value in cpu.gpr[2]).
		/// </summary>
		/// <param name="thread">      the callback has to be executed by this thread (null means the currentThread) </param>
		/// <param name="address">     address of the callback </param>
		/// <param name="afterAction"> action to be executed after the completion of the callback </param>
		/// <param name="returnVoid">  the callback has a void return value, i.e. $v0/$v1 have to be restored </param>
		/// <param name="registerA0">  first parameter of the callback ($a0) </param>
		/// <param name="registerA1">  second parameter of the callback ($a1) </param>
		/// <param name="registerA2">  third parameter of the callback ($a2) </param>
		/// <param name="registerA3">  fourth parameter of the callback ($a3) </param>
		public virtual void executeCallback(SceKernelThreadInfo thread, int address, IAction afterAction, bool returnVoid, int registerA0, int registerA1, int registerA2, int registerA3)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("Execute callback 0x%08X($a0=0x%08X, $a1=0x%08X, $a2=0x%08X, $a3=0x%08X), afterAction=%s, returnVoid=%b", address, registerA0, registerA1, registerA2, registerA3, afterAction, returnVoid));
				log.debug(string.Format("Execute callback 0x%08X($a0=0x%08X, $a1=0x%08X, $a2=0x%08X, $a3=0x%08X), afterAction=%s, returnVoid=%b", address, registerA0, registerA1, registerA2, registerA3, afterAction, returnVoid));
			}

			callAddress(thread, address, afterAction, returnVoid, false, new int[]{registerA0, registerA1, registerA2, registerA3});
		}

		/// <summary>
		/// Trigger a call to a callback in the context of a thread.
		/// This call can return before the completion of the callback. Use the
		/// "afterAction" parameter to trigger some actions that need to be executed
		/// after the callback (e.g. to evaluate a return value in cpu.gpr[2]).
		/// </summary>
		/// <param name="thread">      the callback has to be executed by this thread (null means the currentThread) </param>
		/// <param name="address">     address of the callback </param>
		/// <param name="afterAction"> action to be executed after the completion of the callback </param>
		/// <param name="returnVoid">  the callback has a void return value, i.e. $v0/$v1 have to be restored </param>
		/// <param name="registerA0">  first parameter of the callback ($a0) </param>
		/// <param name="registerA1">  second parameter of the callback ($a1) </param>
		/// <param name="registerA2">  third parameter of the callback ($a2) </param>
		/// <param name="registerT0">  fifth parameter of the callback ($t0) </param>
		public virtual void executeCallback(SceKernelThreadInfo thread, int address, IAction afterAction, bool returnVoid, int registerA0, int registerA1, int registerA2, int registerA3, int registerT0)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("Execute callback 0x%08X($a0=0x%08X, $a1=0x%08X, $a2=0x%08X, $a3=0x%08X, $t0=0x%08X), afterAction=%s, returnVoid=%b", address, registerA0, registerA1, registerA2, registerA3, registerT0, afterAction, returnVoid));
				log.debug(string.Format("Execute callback 0x%08X($a0=0x%08X, $a1=0x%08X, $a2=0x%08X, $a3=0x%08X, $t0=0x%08X), afterAction=%s, returnVoid=%b", address, registerA0, registerA1, registerA2, registerA3, registerT0, afterAction, returnVoid));
			}

			callAddress(thread, address, afterAction, returnVoid, false, new int[]{registerA0, registerA1, registerA2, registerA3, registerT0});
		}

		/// <summary>
		/// Trigger a call to a callback in the context of a thread.
		/// This call can return before the completion of the callback. Use the
		/// "afterAction" parameter to trigger some actions that need to be executed
		/// after the callback (e.g. to evaluate a return value in cpu.gpr[2]).
		/// </summary>
		/// <param name="thread">      the callback has to be executed by this thread (null means the currentThread) </param>
		/// <param name="address">     address of the callback </param>
		/// <param name="afterAction"> action to be executed after the completion of the callback </param>
		/// <param name="returnVoid">  the callback has a void return value, i.e. $v0/$v1 have to be restored </param>
		/// <param name="registerA0">  first parameter of the callback ($a0) </param>
		/// <param name="registerA1">  second parameter of the callback ($a1) </param>
		/// <param name="registerA2">  third parameter of the callback ($a2) </param>
		/// <param name="registerT0">  fifth parameter of the callback ($t0) </param>
		/// <param name="registerT1">  sixth parameter of the callback ($t1) </param>
		public virtual void executeCallback(SceKernelThreadInfo thread, int address, IAction afterAction, bool returnVoid, int registerA0, int registerA1, int registerA2, int registerA3, int registerT0, int registerT1)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("Execute callback 0x%08X($a0=0x%08X, $a1=0x%08X, $a2=0x%08X, $a3=0x%08X, $t0=0x%08X, $t1=0x%08X), afterAction=%s, returnVoid=%b", address, registerA0, registerA1, registerA2, registerA3, registerT0, registerT1, afterAction, returnVoid));
				log.debug(string.Format("Execute callback 0x%08X($a0=0x%08X, $a1=0x%08X, $a2=0x%08X, $a3=0x%08X, $t0=0x%08X, $t1=0x%08X), afterAction=%s, returnVoid=%b", address, registerA0, registerA1, registerA2, registerA3, registerT0, registerT1, afterAction, returnVoid));
			}

			callAddress(thread, address, afterAction, returnVoid, false, new int[]{registerA0, registerA1, registerA2, registerA3, registerT0, registerT1});
		}

		/// <summary>
		/// Trigger a call to a callback in the context of a thread.
		/// This call can return before the completion of the callback. Use the
		/// "afterAction" parameter to trigger some actions that need to be executed
		/// after the callback (e.g. to evaluate a return value in cpu.gpr[2]).
		/// </summary>
		/// <param name="thread">      the callback has to be executed by this thread (null means the currentThread) </param>
		/// <param name="address">     address of the callback </param>
		/// <param name="afterAction"> action to be executed after the completion of the callback </param>
		/// <param name="returnVoid">  the callback has a void return value, i.e. $v0/$v1 have to be restored </param>
		/// <param name="registers">   parameters of the callback </param>
		public virtual void executeCallback(SceKernelThreadInfo thread, int address, IAction afterAction, bool returnVoid, int[] registers)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("Execute callback 0x%08X, afterAction=%s, returnVoid=%b", address, afterAction, returnVoid));
				log.debug(string.Format("Execute callback 0x%08X, afterAction=%s, returnVoid=%b", address, afterAction, returnVoid));
			}

			callAddress(thread, address, afterAction, returnVoid, false, registers);
		}

		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual void hleKernelExitThread(int exitStatus)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Thread exit detected SceUID={0:x} name='{1}' return:0x{2:X8}", currentThread.uid, currentThread.name, exitStatus));
			}
			sceKernelExitThread(exitStatus);
		}

		public virtual int hleKernelExitDeleteThread(int exitStatus)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleKernelExitDeleteThread SceUID={0:x} name='{1}' return:0x{2:X8}", currentThread.uid, currentThread.name, exitStatus));
			}

			return sceKernelExitDeleteThread(exitStatus);
		}

		public virtual int hleKernelExitDeleteThread()
		{
			int exitStatus = Emulator.Processor.cpu._v0;
			return hleKernelExitDeleteThread(exitStatus);
		}

		/// <summary>
		/// Check the validity of the thread UID.
		/// Do not allow uid=0.
		/// </summary>
		/// <param name="uid">   thread UID to be checked </param>
		/// <returns>      valid thread UID </returns>
		public virtual int checkThreadID(int uid)
		{
			if (uid == 0)
			{
				log.warn("checkThreadID illegal thread uid=0");
				throw new SceKernelErrorException(ERROR_KERNEL_ILLEGAL_THREAD);
			}
			return checkThreadIDAllow0(uid);
		}

		/// <summary>
		/// Check the validity of the thread UID.
		/// Allow uid=0.
		/// </summary>
		/// <param name="uid">   thread UID to be checked </param>
		/// <returns>      valid thread UID (0 has been replaced by the UID of the currentThread) </returns>
		public virtual int checkThreadIDAllow0(int uid)
		{
			if (uid == 0)
			{
				uid = currentThread.uid;
			}
			if (!threadMap.ContainsKey(uid))
			{
				log.warn(string.Format("checkThreadID not found thread 0x{0:X8}", uid));
				throw new SceKernelErrorException(ERROR_KERNEL_NOT_FOUND_THREAD);
			}

			if (!SceUidManager.checkUidPurpose(uid, "ThreadMan-thread", true))
			{
				throw new SceKernelErrorException(ERROR_KERNEL_NOT_FOUND_THREAD);
			}

			return uid;
		}

		/// <summary>
		/// Check the validity of the thread UID.
		/// No special check on uid=0, i.e. return ERROR_KERNEL_NOT_FOUND_THREAD for uid=0.
		/// </summary>
		/// <param name="uid">   thread UID to be checked </param>
		/// <returns>      valid thread UID </returns>
		public virtual int checkThreadIDNoCheck0(int uid)
		{
			if (uid == 0)
			{
				log.warn(string.Format("checkThreadID not found thread 0x{0:X8}", uid));
				throw new SceKernelErrorException(ERROR_KERNEL_NOT_FOUND_THREAD);
			}
			return checkThreadIDAllow0(uid);
		}

		/// <summary>
		/// Check the validity of the VTimer UID.
		/// </summary>
		/// <param name="uid">   VTimer UID to be checked </param>
		/// <returns>      valid VTimer UID </returns>
		public virtual int checkVTimerID(int uid)
		{
			if (!vtimers.ContainsKey(uid))
			{
				throw new SceKernelErrorException(ERROR_KERNEL_NOT_FOUND_VTIMER);
			}

			return uid;
		}

		public virtual int checkSemaID(int uid)
		{
			return Managers.semas.checkSemaID(uid);
		}

		public virtual int checkEventFlagID(int uid)
		{
			return Managers.eventFlags.checkEventFlagID(uid);
		}

		public virtual int checkMbxID(int uid)
		{
			return Managers.mbx.checkMbxID(uid);
		}

		public virtual int checkMsgPipeID(int uid)
		{
			return Managers.msgPipes.checkMsgPipeID(uid);
		}

		public virtual int checkVplID(int uid)
		{
			return Managers.vpl.checkVplID(uid);
		}

		public virtual int checkFplID(int uid)
		{
			return Managers.fpl.checkFplID(uid);
		}

		public virtual int checkAlarmID(int uid)
		{
			if (!alarms.ContainsKey(uid))
			{
				log.warn(string.Format("checkAlarmID unknown uid=0x{0:x}", uid));
				throw new SceKernelErrorException(ERROR_KERNEL_NOT_FOUND_ALARM);
			}

			return uid;
		}

		public virtual int checkCallbackID(int uid)
		{
			if (!callbackMap.ContainsKey(uid))
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_KERNEL_NOT_FOUND_CALLBACK);
			}

			return uid;
		}

		public virtual int checkPartitionID(int id)
		{
			if (id < 1 || id > 9 || id == 7)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_ARGUMENT);
			}

			if (id == 6)
			{
				// Partition ID 6 is accepted by the PSP...
				id = SysMemUserForUser.USER_PARTITION_ID;
			}

			if (id != SysMemUserForUser.USER_PARTITION_ID && id != SysMemUserForUser.VSHELL_PARTITION_ID)
			{
				// Accept KERNEL_PARTITION_ID for threads running in kernel mode.
				if (id != SysMemUserForUser.KERNEL_PARTITION_ID || !KernelMode)
				{
					throw new SceKernelErrorException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_PERMISSION);
				}
			}

			return id;
		}

		public virtual SceKernelThreadInfo hleKernelCreateThread(string name, int entry_addr, int initPriority, int stackSize, int attr, int option_addr, int mpidStack)
		{
			if (option_addr != 0)
			{
				SceKernelThreadOptParam sceKernelThreadOptParam = new SceKernelThreadOptParam();
				sceKernelThreadOptParam.read(Emulator.Memory, option_addr);
				if (sceKernelThreadOptParam.@sizeof() >= 8)
				{
					mpidStack = sceKernelThreadOptParam.stackMpid;
				}
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceKernelCreateThread options: {0}", sceKernelThreadOptParam));
				}
			}

			SceKernelThreadInfo thread = new SceKernelThreadInfo(name, entry_addr, initPriority, stackSize, attr, mpidStack);
			threadMap[thread.uid] = thread;

			// inherit module id
			if (currentThread != null)
			{
				thread.moduleid = currentThread.moduleid;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleKernelCreateThread SceUID=0x{0:X}, name='{1}', PC=0x{2:X8}, attr=0x{3:X}, priority=0x{4:X}, stackSize=0x{5:X}", thread.uid, thread.name, thread.cpuContext.pc, attr, initPriority, stackSize));
			}

			return thread;
		}

		public virtual void hleKernelStartThread(SceKernelThreadInfo thread, int userDataLength, int userDataAddr, int gp)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleKernelStartThread SceUID=0x{0:X}, name='{1}', dataLen=0x{2:X}, data=0x{3:X8}, gp=0x{4:X8}", thread.uid, thread.name, userDataLength, userDataAddr, gp));
			}
			// Reset all thread parameters: a thread can be restarted when it has exited.
			thread.reset();

			// Setup args by copying them onto the stack
			hleKernelSetThreadArguments(thread, userDataAddr, userDataLength);

			// Set thread $gp
			thread.cpuContext._gp = gp;

			// Update the exit status.
			thread.exitStatus = ERROR_KERNEL_THREAD_IS_NOT_DORMANT;

			// switch in the target thread if it's higher priority
			hleChangeThreadState(thread, PSP_THREAD_READY);

			// Execute the event in the context of the starting thread
			triggerThreadEvent(thread, thread, THREAD_EVENT_START);

			// sceKernelStartThread is always resuming the thread dispatching (tested on PSP using taskScheduler.prx).
			// Assuming here that the other syscalls starting a thread
			// (sceKernelStartModule, sceKernelStopModule, sceNetAdhocctlInit...)
			// have the same behavior.
			hleKernelResumeDispatchThread();

			RuntimeContext.onThreadStart(thread);

			if (currentThread == null || thread.currentPriority < currentThread.currentPriority)
			{
				if (log.DebugEnabled)
				{
					log.debug("hleKernelStartThread switching in thread immediately");
				}
				hleRescheduleCurrentThread();
			}
		}

		public virtual int hleKernelSleepThread(bool doCallbacks)
		{
			if (currentThread.wakeupCount > 0)
			{
				// sceKernelWakeupThread() has been called before, do not sleep
				currentThread.wakeupCount--;
			}
			else
			{
				// Go to wait state and wait forever (another thread will call sceKernelWakeupThread)
				hleKernelThreadEnterWaitState(PSP_WAIT_SLEEP, 0, sleepThreadWaitStateChecker, doCallbacks);
			}

			return 0;
		}

		public virtual void hleKernelWakeupThread(SceKernelThreadInfo thread)
		{
			if (!thread.Waiting || thread.waitType != PSP_WAIT_SLEEP)
			{
				thread.wakeupCount++;
				if (log.DebugEnabled)
				{
					log.debug("sceKernelWakeupThread SceUID=" + thread.uid.ToString("x") + " name:'" + thread.name + "' not sleeping/waiting (status=0x" + thread.status.ToString("x") + "), incrementing wakeupCount to " + thread.wakeupCount);
				}
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug("sceKernelWakeupThread SceUID=" + thread.uid.ToString("x") + " name:'" + thread.name + "'");
				}
				hleThreadWaitRelease(thread);

				// Check if we have to switch in the target thread
				// e.g. if if has a higher priority
				hleRescheduleCurrentThread();
			}
		}

		public virtual int hleKernelWaitThreadEnd(SceKernelThreadInfo waitingThread, int uid, TPointer32 timeoutAddr, bool callbacks, bool returnExitStatus)
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("hleKernelWaitThreadEnd SceUID=0x%X, callbacks=%b", uid, callbacks));
				log.debug(string.Format("hleKernelWaitThreadEnd SceUID=0x%X, callbacks=%b", uid, callbacks));
			}

			SceKernelThreadInfo thread = threadMap[uid];
			if (thread == null)
			{
				log.warn(string.Format("hleKernelWaitThreadEnd unknown thread 0x{0:X}", uid));
				return ERROR_KERNEL_NOT_FOUND_THREAD;
			}

			int result = 0;
			if (thread.Stopped)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleKernelWaitThreadEnd {0} thread already stopped, not waiting, exitStatus=0x{1:X8}", thread, thread.exitStatus));
				}
				if (returnExitStatus)
				{
					// Return the thread exit status
					result = thread.exitStatus;
				}
				hleRescheduleCurrentThread();
			}
			else
			{
				// Wait on a specific thread end
				waitingThread.wait.ThreadEnd_id = uid;
				waitingThread.wait.ThreadEnd_returnExitStatus = returnExitStatus;
				hleKernelThreadEnterWaitState(waitingThread, PSP_WAIT_THREAD_END, uid, waitThreadEndWaitStateChecker, timeoutAddr.Address, callbacks);
			}

			return result;
		}

		/// <summary>
		/// Set the wait timeout for a thread. The state of the thread is not changed.
		/// </summary>
		/// <param name="thread">  the thread </param>
		/// <param name="wait">    the same as thread.wait </param>
		/// <param name="micros">  the timeout in microseconds (this is an unsigned value: SceUInt32) </param>
		/// <param name="forever"> true if the thread has to wait forever (micros in then ignored) </param>
		public virtual void hleKernelThreadWait(SceKernelThreadInfo thread, int micros, bool forever)
		{
			thread.wait.forever = forever;
			thread.wait.micros = micros; // for debugging
			if (forever)
			{
				thread.wait.microTimeTimeout = 0;
				thread.wait.waitTimeoutAction = null;
			}
			else
			{
				if (micros < THREAD_DELAY_MINIMUM_MICROS && !isIdleThread(thread))
				{
					micros = THREAD_DELAY_MINIMUM_MICROS * 2;
				}

				long longMicros = ((long) micros) & 0xFFFFFFFFL;
				thread.wait.microTimeTimeout = Emulator.Clock.microTime() + longMicros;
				thread.wait.waitTimeoutAction = new TimeoutThreadAction(this, thread);
				thread.wait.waitStateChecker = timeoutThreadWaitStateChecker;
			}

			if (LOG_CONTEXT_SWITCHING && log.DebugEnabled && !isIdleThread(thread))
			{
				log.debug("-------------------- hleKernelThreadWait micros=" + micros + " forever:" + forever + " thread:'" + thread.name + "' caller:" + CallingFunction);
			}
		}

		public virtual void hleKernelDelayThread(int micros, bool doCallbacks)
		{
			hleKernelDelayThread(currentThread.uid, micros, doCallbacks);
		}

		public virtual void hleKernelDelayThread(int uid, int micros, bool doCallbacks)
		{
			if (micros < THREAD_DELAY_MINIMUM_MICROS && !isIdleThread(uid))
			{
				micros = THREAD_DELAY_MINIMUM_MICROS;
			}

			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("hleKernelDelayThread micros=%d, callbacks=%b", micros, doCallbacks));
				log.debug(string.Format("hleKernelDelayThread micros=%d, callbacks=%b", micros, doCallbacks));
			}

			SceKernelThreadInfo thread = getThreadById(uid);
			if (thread != null)
			{
				hleKernelThreadEnterWaitState(thread, PSP_WAIT_DELAY, 0, null, micros, false, doCallbacks);
			}
		}

		public virtual SceKernelCallbackInfo hleKernelCreateCallback(string name, int func_addr, int user_arg_addr)
		{
			SceKernelCallbackInfo callback = new SceKernelCallbackInfo(name, currentThread.uid, func_addr, user_arg_addr);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleKernelCreateCallback {0}", callback));
			}

			callbackMap[callback.Uid] = callback;

			return callback;
		}

		public virtual pspBaseCallback hleKernelCreateCallback(int callbackFunction, int numberArguments)
		{
			pspBaseCallback callback = new pspBaseCallback(callbackFunction, numberArguments);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleKernelCreateCallback {0}", callback));
			}

			callbackMap[callback.Uid] = callback;

			return callback;
		}

		/// <returns> true if successful. </returns>
		public virtual void hleKernelDeleteCallback(int uid)
		{
			SceKernelCallbackInfo callback = getCallbackInfo(uid);
			if (callback != null)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleKernelDeleteCallback {0}", callback));
				}
				callbackMap.Remove(uid);
				SceKernelThreadInfo thread = getThreadById(callback.ThreadId);
				if (thread != null)
				{
					thread.deleteCallback(callback);
				}
			}
			else
			{
				log.warn(string.Format("hleKernelDeleteCallback not a callback uid 0x{0:X}", uid));
			}
		}

		protected internal virtual int getThreadCurrentStackSize(Processor processor)
		{
			int size = processor.cpu._sp - currentThread.StackAddr;
			if (size < 0)
			{
				size = 0;
			}
			return size;
		}

		private bool userCurrentThreadTryingToSwitchToKernelMode(int newAttr)
		{
			return currentThread.UserMode && !SceKernelThreadInfo.isUserMode(newAttr);
		}

		private bool userThreadCalledKernelCurrentThread(SceKernelThreadInfo thread)
		{
			return !isIdleThread(thread) && (!thread.KernelMode || currentThread.KernelMode);
		}

		private int DispatchThreadState
		{
			get
			{
				return dispatchThreadEnabled ? SCE_KERNEL_DISPATCHTHREAD_STATE_ENABLED : SCE_KERNEL_DISPATCHTHREAD_STATE_DISABLED;
			}
		}

		private void hleKernelResumeDispatchThread()
		{
			if (!dispatchThreadEnabled)
			{
				dispatchThreadEnabled = true;
				hleRescheduleCurrentThread();
			}
		}

		public virtual bool hleKernelRegisterCallback(int callbackType, pspBaseCallback callback)
		{
			return hleKernelRegisterCallback(CurrentThread, callbackType, callback);
		}

		public virtual bool hleKernelRegisterCallback(SceKernelThreadInfo thread, int callbackType, pspBaseCallback callback)
		{
			SceKernelThreadInfo.RegisteredCallbacks registeredCallbacks = thread.getRegisteredCallbacks(callbackType);

			return registeredCallbacks.addCallback(callback);
		}

		/// <summary>
		/// Registers a callback on the thread that created the callback. </summary>
		/// <returns> true on success (the cbid was a valid callback uid)  </returns>
		public virtual bool hleKernelRegisterCallback(int callbackType, int cbid)
		{
			SceKernelCallbackInfo callback = getCallbackInfo(cbid);
			if (callback == null)
			{
				log.warn("hleKernelRegisterCallback(type=" + callbackType + ") unknown uid " + cbid.ToString("x"));
				return false;
			}

			SceKernelThreadInfo thread = getThreadById(callback.ThreadId);
			if (thread == null)
			{
				log.warn("hleKernelRegisterCallback(type=" + callbackType + ") unknown thread uid " + callback.ThreadId.ToString("x"));
				return false;
			}
			SceKernelThreadInfo.RegisteredCallbacks registeredCallbacks = thread.getRegisteredCallbacks(callbackType);
			if (!registeredCallbacks.addCallback(callback))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Unregisters a callback by type and cbid. May not be on the current thread. </summary>
		/// <param name="callbackType"> See SceKernelThreadInfo. </param>
		/// <param name="cbid"> The UID of the callback to unregister. </param>
		/// <returns> true if the callback has been removed, or false if it couldn't be found.
		///  </returns>
		public virtual bool hleKernelUnRegisterCallback(int callbackType, int cbid)
		{
			bool found = false;

			foreach (SceKernelThreadInfo thread in threadMap.Values)
			{
				SceKernelThreadInfo.RegisteredCallbacks registeredCallbacks = thread.getRegisteredCallbacks(callbackType);

				pspBaseCallback callback = registeredCallbacks.getCallbackInfoByUid(cbid);
				if (callback != null)
				{
					found = true;
					// Warn if we are removing a pending callback, this a callback
					// that has been pushed but not yet executed.
					if (registeredCallbacks.isCallbackReady(callback))
					{
						log.warn("hleKernelUnRegisterCallback(type=" + callbackType + ") removing pending callback");
					}

					registeredCallbacks.removeCallback(callback);
					break;
				}
			}

			if (!found)
			{
				log.warn("hleKernelUnRegisterCallback(type=" + callbackType + ") cbid=" + cbid.ToString("x") + " no matching callbacks found");
			}

			return found;
		}

		public virtual void hleKernelNotifyCallback(int callbackType, pspBaseCallback callback)
		{
			hleKernelNotifyCallback(callbackType, callback, CurrentThread);
		}

		public virtual void hleKernelNotifyCallback(int callbackType, pspBaseCallback callback, SceKernelThreadInfo thread)
		{
			SceKernelThreadInfo.RegisteredCallbacks registeredCallbacks = thread.getRegisteredCallbacks(callbackType);

			if (registeredCallbacks.hasCallback(callback))
			{
				registeredCallbacks.CallbackReady = callback;
			}
		}

		/// <summary>
		/// push callback to all threads </summary>
		public virtual void hleKernelNotifyCallback(int callbackType, int notifyArg)
		{
			hleKernelNotifyCallback(callbackType, -1, notifyArg);
		}

		private void notifyCallback(SceKernelThreadInfo thread, SceKernelCallbackInfo callback, int callbackType, int notifyArg)
		{
			if (callback.NotifyCount > 0)
			{
				log.warn("hleKernelNotifyCallback(type=" + callbackType + ") thread:'" + thread.name + "' overwriting previous notifyArg 0x" + callback.NotifyArg.ToString("x") + " -> 0x" + notifyArg.ToString("x") + ", newCount=" + (callback.NotifyCount + 1));
			}

			callback.NotifyArg = notifyArg;
			thread.getRegisteredCallbacks(callbackType).CallbackReady = callback;
		}

		/// <param name="cbid"> If cbid is -1, then push callback to all threads
		/// if cbid is not -1 then only trigger that specific cbid provided it is
		/// also of type callbackType.  </param>
		public virtual void hleKernelNotifyCallback(int callbackType, int cbid, int notifyArg)
		{
			bool pushed = false;

			foreach (SceKernelThreadInfo thread in threadMap.Values)
			{
				SceKernelThreadInfo.RegisteredCallbacks registeredCallbacks = thread.getRegisteredCallbacks(callbackType);

				if (!registeredCallbacks.hasCallbacks())
				{
					continue;
				}

				if (cbid != -1)
				{
					pspBaseCallback callback = registeredCallbacks.getCallbackInfoByUid(cbid);
					if (callback == null || !(callback is SceKernelCallbackInfo))
					{
						continue;
					}

					notifyCallback(thread, (SceKernelCallbackInfo) callback, callbackType, notifyArg);
				}
				else
				{
					int numberOfCallbacks = registeredCallbacks.NumberOfCallbacks;
					for (int i = 0; i < numberOfCallbacks; i++)
					{
						pspBaseCallback callback = registeredCallbacks.getCallbackByIndex(i);
						if (callback is SceKernelCallbackInfo)
						{
							notifyCallback(thread, (SceKernelCallbackInfo) callback, callbackType, notifyArg);
						}
					}
				}

				pushed = true;
			}

			if (pushed)
			{
				// Enter callbacks immediately,
				// except those registered to the current thread. The app must explictly
				// call sceKernelCheckCallback or a waitCB function to do that.
				if (log.DebugEnabled)
				{
					log.debug("hleKernelNotifyCallback(type=" + callbackType + ") calling checkCallbacks");
				}
				checkCallbacks();
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleKernelNotifyCallback(type={0:D}) no registered callbacks to push", callbackType));
				}
			}
		}

		/// <summary>
		/// runs callbacks. Check the thread doCallbacks flag. </summary>
		/// <returns> true if we switched into a callback.  </returns>
		private bool checkThreadCallbacks(SceKernelThreadInfo thread)
		{
			bool handled = false;

			if (thread == null || !thread.doCallbacks)
			{
				return handled;
			}

			for (int callbackType = 0; callbackType < SceKernelThreadInfo.THREAD_CALLBACK_SIZE; callbackType++)
			{
				SceKernelThreadInfo.RegisteredCallbacks registeredCallbacks = thread.getRegisteredCallbacks(callbackType);
				pspBaseCallback callback = registeredCallbacks.NextReadyCallback;
				if (callback != null)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Entering callback type {0:D} {1} for thread {2} (current thread is {3})", callbackType, callback.ToString(), thread.ToString(), currentThread.ToString()));
					}

					CheckCallbackReturnValue checkCallbackReturnValue = new CheckCallbackReturnValue(this, thread, callback.Uid);
					callback.call(thread, checkCallbackReturnValue);
					handled = true;
					break;
				}
			}

			return handled;
		}

		public virtual void cancelAlarm(SceKernelAlarmInfo sceKernelAlarmInfo)
		{
			Scheduler.Instance.removeAction(sceKernelAlarmInfo.schedule, sceKernelAlarmInfo.alarmInterruptAction);
			sceKernelAlarmInfo.schedule = 0;
			sceKernelAlarmInfo.delete();
			alarms.Remove(sceKernelAlarmInfo.uid);
		}

		public virtual void rescheduleAlarm(SceKernelAlarmInfo sceKernelAlarmInfo, int delay)
		{
			if (delay < 0)
			{
				delay = 100;
			}

			sceKernelAlarmInfo.schedule += delay;
			scheduleAlarm(sceKernelAlarmInfo);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("New Schedule for Alarm uid={0:x}: {1:D}", sceKernelAlarmInfo.uid, sceKernelAlarmInfo.schedule));
			}
		}

		private void scheduleAlarm(SceKernelAlarmInfo sceKernelAlarmInfo)
		{
			Scheduler.Instance.addAction(sceKernelAlarmInfo.schedule, sceKernelAlarmInfo.alarmInterruptAction);
		}

		protected internal virtual int hleKernelSetAlarm(long delayUsec, TPointer handlerAddress, int handlerArgument)
		{
			long now = Scheduler.Now;
			long schedule = now + delayUsec;
			SceKernelAlarmInfo sceKernelAlarmInfo = new SceKernelAlarmInfo(schedule, handlerAddress.Address, handlerArgument);
			alarms[sceKernelAlarmInfo.uid] = sceKernelAlarmInfo;

			scheduleAlarm(sceKernelAlarmInfo);

			return sceKernelAlarmInfo.uid;
		}

		protected internal virtual long SystemTime
		{
			get
			{
				return SystemTimeManager.SystemTime;
			}
		}

		protected internal virtual long getVTimerScheduleForScheduler(SceKernelVTimerInfo sceKernelVTimerInfo)
		{
			return sceKernelVTimerInfo.@base + sceKernelVTimerInfo.schedule;
		}

		protected internal virtual long setVTimer(SceKernelVTimerInfo sceKernelVTimerInfo, long time)
		{
			long current = sceKernelVTimerInfo.CurrentTime;
			sceKernelVTimerInfo.@base = sceKernelVTimerInfo.@base + sceKernelVTimerInfo.CurrentTime - time;
			sceKernelVTimerInfo.current = 0;

			return current;
		}

		protected internal virtual void startVTimer(SceKernelVTimerInfo sceKernelVTimerInfo)
		{
			sceKernelVTimerInfo.active = SceKernelVTimerInfo.ACTIVE_RUNNING;
			sceKernelVTimerInfo.@base = SystemTime;

			if (sceKernelVTimerInfo.handlerAddress != 0)
			{
				scheduleVTimer(sceKernelVTimerInfo, sceKernelVTimerInfo.schedule);
			}
		}

		protected internal virtual void stopVTimer(SceKernelVTimerInfo sceKernelVTimerInfo)
		{
			Scheduler.Instance.removeAction(getVTimerScheduleForScheduler(sceKernelVTimerInfo), sceKernelVTimerInfo.vtimerInterruptAction);
			// Sum the elapsed time (multiple Start/Stop sequences are added)
			sceKernelVTimerInfo.current = sceKernelVTimerInfo.CurrentTime;
			sceKernelVTimerInfo.active = SceKernelVTimerInfo.ACTIVE_STOPPED;
			sceKernelVTimerInfo.@base = 0;
		}

		protected internal virtual void scheduleVTimer(SceKernelVTimerInfo sceKernelVTimerInfo, long schedule)
		{
			// Remove any previous schedule
			Scheduler.Instance.removeAction(getVTimerScheduleForScheduler(sceKernelVTimerInfo), sceKernelVTimerInfo.vtimerInterruptAction);

			sceKernelVTimerInfo.schedule = schedule;

			if (sceKernelVTimerInfo.active == SceKernelVTimerInfo.ACTIVE_RUNNING && sceKernelVTimerInfo.handlerAddress != 0)
			{
				Scheduler scheduler = Scheduler.Instance;
				long schedulerSchedule = getVTimerScheduleForScheduler(sceKernelVTimerInfo);
				scheduler.addAction(schedulerSchedule, sceKernelVTimerInfo.vtimerInterruptAction);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Scheduling VTimer {0} at {1:D}(now={2:D})", sceKernelVTimerInfo, schedulerSchedule, Scheduler.Now));
				}
			}
		}

		public virtual void cancelVTimer(SceKernelVTimerInfo sceKernelVTimerInfo)
		{
			Scheduler.Instance.removeAction(getVTimerScheduleForScheduler(sceKernelVTimerInfo), sceKernelVTimerInfo.vtimerInterruptAction);
			sceKernelVTimerInfo.schedule = 0;
			sceKernelVTimerInfo.handlerAddress = 0;
			sceKernelVTimerInfo.handlerArgument = 0;
		}

		public virtual void rescheduleVTimer(SceKernelVTimerInfo sceKernelVTimerInfo, int delay)
		{
			if (delay < 0)
			{
				delay = 100;
			}

			long schedule = sceKernelVTimerInfo.schedule + delay;
			scheduleVTimer(sceKernelVTimerInfo, schedule);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("New Schedule for VTimer uid={0:x}: {1:D}", sceKernelVTimerInfo.uid, sceKernelVTimerInfo.schedule));
			}
		}

		/// <summary>
		/// Iterates waiting threads, making sure doCallbacks is set before
		/// checking for pending callbacks.
		/// Handles sceKernelCheckCallback when doCallbacks is set on currentThread.
		/// Handles redirects to yieldCB (from fake waitCB) on the thread that called waitCB.
		/// 
		/// We currently call checkCallbacks() at the end of each waitCB function
		/// since this has less overhead than checking on every step.
		/// 
		/// Some trickery is used in yieldCurrentThreadCB(). By the time we get
		/// inside the checkCallbacks() function the thread that called yieldCB is
		/// no longer the current thread. Also the thread that called yieldCB is
		/// not in the wait state (it's in the ready state). so what we do is check
		/// every thread, not just the waiting threads for the doCallbacks flag.
		/// Also the waitingThreads list only contains waiting threads that have a
		/// finite wait period, so we have to iterate on all threads anyway.
		/// 
		/// It is probably unsafe to call contextSwitch() when insideCallback is true.
		/// insideCallback may become true after a call to checkCallbacks().
		/// </summary>
		public virtual void checkCallbacks()
		{
			if (log.TraceEnabled)
			{
				log.trace("checkCallbacks current thread is '" + currentThread.name + "' doCallbacks:" + currentThread.doCallbacks + " caller:" + CallingFunction);
			}

			bool handled;
			SceKernelThreadInfo checkCurrentThread = currentThread;
			do
			{
				handled = false;
				foreach (SceKernelThreadInfo thread in threadMap.Values)
				{
					if (thread.doCallbacks && checkThreadCallbacks(thread))
					{
						handled = true;
						break;
					}
				}
				// Continue until there is no more callback to be executed or
				// we have switched to another thread.
			} while (handled && checkCurrentThread == currentThread);
		}

		public virtual int checkStackSize(int size)
		{
			if (size < 0x200)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_STACK_SIZE);
			}

			// Size is rounded up to a multiple of 256
			return (size + 0xFF) & ~0xFF;
		}

		public virtual SceKernelTls getKernelTls(int uid)
		{
			return tlsMap[uid];
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6E9EA350, version = 150) public int _sceKernelReturnFromCallback()
		[HLEFunction(nid : 0x6E9EA350, version : 150)]
		public virtual int _sceKernelReturnFromCallback()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0C106E53, version = 150, checkInsideInterrupt = true) public int sceKernelRegisterThreadEventHandler(@StringInfo(maxLength = 32) String name, int thid, int mask, pspsharp.HLE.TPointer handlerFunc, int commonAddr)
		[HLEFunction(nid : 0x0C106E53, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelRegisterThreadEventHandler(string name, int thid, int mask, TPointer handlerFunc, int commonAddr)
		{
			switch (thid)
			{
				case SceKernelThreadEventHandlerInfo.THREAD_EVENT_ID_CURRENT:
					// Only allowed for THREAD_EVENT_EXIT (doesn't make sense for the other events).
					if (mask != SceKernelThreadEventHandlerInfo.THREAD_EVENT_EXIT)
					{
						return SceKernelErrors.ERROR_KERNEL_OUT_OF_RANGE;
					}
					thid = CurrentThreadID;
					break;
				case SceKernelThreadEventHandlerInfo.THREAD_EVENT_ID_USER:
					// Always allowed
					break;
				case SceKernelThreadEventHandlerInfo.THREAD_EVENT_ID_KERN:
				case SceKernelThreadEventHandlerInfo.THREAD_EVENT_ID_ALL:
					// Only allowed in kernel mode
					if (!KernelMode)
					{
						return ERROR_KERNEL_NOT_FOUND_THREAD;
					}
					break;
				default:
					SceKernelThreadInfo thread = getThreadById(thid);
					if (thread == null)
					{
						return ERROR_KERNEL_NOT_FOUND_THREAD;
					}
					break;
			}

			SceKernelThreadEventHandlerInfo handler = new SceKernelThreadEventHandlerInfo(name, thid, mask, handlerFunc.Address, commonAddr);
			threadEventHandlers[handler.uid] = handler;

			return handler.uid;
		}

		[HLEFunction(nid : 0x72F3C145, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelReleaseThreadEventHandler(int uid)
		{
			if (!threadEventHandlers.ContainsKey(uid))
			{
				return ERROR_KERNEL_NOT_FOUND_THREAD_EVENT_HANDLER;
			}

			SceKernelThreadEventHandlerInfo handler = threadEventHandlers.Remove(uid);
			handler.release();
			return 0;
		}

		[HLEFunction(nid : 0x369EEB6B, version : 150)]
		public virtual int sceKernelReferThreadEventHandlerStatus(int uid, TPointer statusPointer)
		{
			if (!threadEventHandlers.ContainsKey(uid))
			{
				return ERROR_KERNEL_NOT_FOUND_THREAD_EVENT_HANDLER;
			}

			threadEventHandlers[uid].write(statusPointer);
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE81CAF8F, version = 150, checkInsideInterrupt = true) public int sceKernelCreateCallback(@StringInfo(maxLength = 32) String name, pspsharp.HLE.TPointer func_addr, int user_arg_addr)
		[HLEFunction(nid : 0xE81CAF8F, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelCreateCallback(string name, TPointer func_addr, int user_arg_addr)
		{
			SceKernelCallbackInfo callback = hleKernelCreateCallback(name, func_addr.Address, user_arg_addr);
			return callback.Uid;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xEDBA5844, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelDeleteCallback(@CheckArgument("checkCallbackID") int uid)
		[HLEFunction(nid : 0xEDBA5844, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelDeleteCallback(int uid)
		{
			hleKernelDeleteCallback(uid);

			return 0;
		}

		/// <summary>
		/// Manually notifies a callback. Mostly used for exit callbacks,
		/// and shouldn't be used at all (only some old homebrews use this, anyway).
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC11BA8C4, version = 150) public int sceKernelNotifyCallback(@CheckArgument("checkCallbackID") int uid, int arg)
		[HLEFunction(nid : 0xC11BA8C4, version : 150)]
		public virtual int sceKernelNotifyCallback(int uid, int arg)
		{
			SceKernelCallbackInfo callback = getCallbackInfo(uid);

			bool foundCallback = false;
			for (int i = 0; i < SceKernelThreadInfo.THREAD_CALLBACK_SIZE; i++)
			{
				SceKernelThreadInfo.RegisteredCallbacks registeredCallbacks = CurrentThread.getRegisteredCallbacks(i);
				if (registeredCallbacks.hasCallback(callback))
				{
					hleKernelNotifyCallback(i, uid, arg);
					foundCallback = true;
					break;
				}
			}
			if (!foundCallback)
			{
				// Register the callback as a temporary THREAD_CALLBACK_USER_DEFINED
				if (hleKernelRegisterCallback(SceKernelThreadInfo.THREAD_CALLBACK_USER_DEFINED, uid))
				{
					hleKernelNotifyCallback(SceKernelThreadInfo.THREAD_CALLBACK_USER_DEFINED, uid, arg);
				}
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xBA4051D6, version = 150) public int sceKernelCancelCallback(@CheckArgument("checkCallbackID") int uid)
		[HLEFunction(nid : 0xBA4051D6, version : 150)]
		public virtual int sceKernelCancelCallback(int uid)
		{
			SceKernelCallbackInfo callback = getCallbackInfo(uid);

			callback.cancel();

			return 0;
		}

		/// <summary>
		/// Return the current notifyCount for a specific callback </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x2A3D44FF, version = 150) public int sceKernelGetCallbackCount(@CheckArgument("checkCallbackID") int uid)
		[HLEFunction(nid : 0x2A3D44FF, version : 150)]
		public virtual int sceKernelGetCallbackCount(int uid)
		{
			SceKernelCallbackInfo callback = getCallbackInfo(uid);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelGetCallbackCount returning count={0:D}", callback.NotifyCount));
			}

			return callback.NotifyCount;
		}

		/// <summary>
		/// Check callbacks, only on the current thread. </summary>
		[HLEFunction(nid : 0x349D6D6C, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelCheckCallback()
		{
			// Remember the currentThread, as it might have changed after
			// the execution of a callback.
			SceKernelThreadInfo thread = currentThread;
			bool doCallbacks = thread.doCallbacks;
			thread.doCallbacks = true; // Force callbacks execution.

			// 0 - The calling thread has no reported callbacks.
			// 1 - The calling thread has reported callbacks which were executed successfully.
			int result = checkThreadCallbacks(thread) ? 1 : 0;

			thread.doCallbacks = doCallbacks; // Reset to the previous value.

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x730ED8BC, version = 150) public int sceKernelReferCallbackStatus(@CheckArgument("checkCallbackID") int uid, pspsharp.HLE.TPointer infoAddr)
		[HLEFunction(nid : 0x730ED8BC, version : 150)]
		public virtual int sceKernelReferCallbackStatus(int uid, TPointer infoAddr)
		{
			SceKernelCallbackInfo info = getCallbackInfo(uid);

			info.write(infoAddr);

			return 0;
		}

		/// <summary>
		/// sleep the current thread (using wait) </summary>
		[HLEFunction(nid : 0x9ACE131E, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelSleepThread()
		{
			return hleKernelSleepThread(false);
		}

		/// <summary>
		/// sleep the current thread and handle callbacks (using wait)
		/// in our implementation we have to use wait, not suspend otherwise we don't handle callbacks. 
		/// </summary>
		[HLEFunction(nid : 0x82826F70, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelSleepThreadCB()
		{
			int result = hleKernelSleepThread(true);
			checkCallbacks();

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD59EAD2F, version = 150) public int sceKernelWakeupThread(@CheckArgument("checkThreadID") int uid)
		[HLEFunction(nid : 0xD59EAD2F, version : 150)]
		public virtual int sceKernelWakeupThread(int uid)
		{
			SceKernelThreadInfo thread = threadMap[uid];

			hleKernelWakeupThread(thread);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xFCCFAD26, version = 150) public int sceKernelCancelWakeupThread(@CheckArgument("checkThreadIDAllow0") int uid)
		[HLEFunction(nid : 0xFCCFAD26, version : 150)]
		public virtual int sceKernelCancelWakeupThread(int uid)
		{
			SceKernelThreadInfo thread = getThreadById(uid);

			int result = thread.wakeupCount;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelCancelWakeupThread thread={0} returning {1:D}", thread, result));
			}

			thread.wakeupCount = 0;

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x9944F31F, version = 150) public int sceKernelSuspendThread(@CheckArgument("checkThreadID") int uid)
		[HLEFunction(nid : 0x9944F31F, version : 150)]
		public virtual int sceKernelSuspendThread(int uid)
		{
			SceKernelThreadInfo thread = getThreadCurrentIsInvalid(uid);

			if (thread.Suspended)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceKernelSuspendThread thread already suspended: thread={0}", thread.ToString()));
				}
				return ERROR_KERNEL_THREAD_ALREADY_SUSPEND;
			}

			if (thread.Stopped)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceKernelSuspendThread thread already stopped: thread={0}", thread.ToString()));
				}
				return ERROR_KERNEL_THREAD_ALREADY_DORMANT;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelSuspendThread thread before suspend: {0}", thread));
			}

			if (thread.Waiting)
			{
				hleChangeThreadState(thread, PSP_THREAD_WAITING_SUSPEND);
			}
			else
			{
				hleChangeThreadState(thread, PSP_THREAD_SUSPEND);
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelSuspendThread thread after suspend: {0}", thread));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x75156E8F, version = 150) public int sceKernelResumeThread(@CheckArgument("checkThreadID") int uid)
		[HLEFunction(nid : 0x75156E8F, version : 150)]
		public virtual int sceKernelResumeThread(int uid)
		{
			SceKernelThreadInfo thread = getThreadById(uid);

			if (!thread.Suspended)
			{
				log.warn("sceKernelResumeThread SceUID=" + uid.ToString("x") + " not suspended (status=" + thread.status + ")");
				return ERROR_KERNEL_THREAD_IS_NOT_SUSPEND;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelResumeThread thread before resume: {0}", thread));
			}

			if (thread.Waiting)
			{
				hleChangeThreadState(thread, PSP_THREAD_WAITING);
			}
			else
			{
				hleChangeThreadState(thread, PSP_THREAD_READY);
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelResumeThread thread after resume: {0}", thread));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x278C0DF5, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelWaitThreadEnd(@CheckArgument("checkThreadID") int uid, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x278C0DF5, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelWaitThreadEnd(int uid, TPointer32 timeoutAddr)
		{
			return hleKernelWaitThreadEnd(currentThread, uid, timeoutAddr, false, true);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x840E8133, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelWaitThreadEndCB(@CheckArgument("checkThreadID") int uid, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x840E8133, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelWaitThreadEndCB(int uid, TPointer32 timeoutAddr)
		{
			int result = hleKernelWaitThreadEnd(currentThread, uid, timeoutAddr, true, true);
			checkCallbacks();

			return result;
		}

		/// <summary>
		/// wait the current thread for a certain number of microseconds </summary>
		[HLEFunction(nid : 0xCEADEB47, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelDelayThread(int micros)
		{
			hleKernelDelayThread(micros, false);
			return 0;
		}

		/// <summary>
		/// wait the current thread for a certain number of microseconds </summary>
		[HLEFunction(nid : 0x68DA9E36, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelDelayThreadCB(int micros)
		{
			hleKernelDelayThread(micros, true);
			return 0;
		}

		/// <summary>
		/// Delay the current thread by a specified number of sysclocks
		/// </summary>
		/// <param name="sysclocksPointer"> - Address of delay in sysclocks
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
		[HLEFunction(nid : 0xBD123D9E, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelDelaySysClockThread(TPointer64 sysclocksPointer)
		{
			long sysclocks = sysclocksPointer.Value;
			int micros = SystemTimeManager.hleSysClock2USec32(sysclocks);
			hleKernelDelayThread(micros, false);
			return 0;
		}

		/// <summary>
		/// Delay the current thread by a specified number of sysclocks handling callbacks
		/// </summary>
		/// <param name="sysclocks_addr"> - Address of delay in sysclocks
		/// </param>
		/// <returns> 0 on success, < 0 on error
		///  </returns>
		[HLEFunction(nid : 0x1181E963, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelDelaySysClockThreadCB(TPointer64 sysclocksAddr)
		{
			long sysclocks = sysclocksAddr.Value;
			int micros = SystemTimeManager.hleSysClock2USec32(sysclocks);
			hleKernelDelayThread(micros, true);
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD6DA4BA1, version = 150, checkInsideInterrupt = true) public int sceKernelCreateSema(String name, int attr, int initVal, int maxVal, @CanBeNull pspsharp.HLE.TPointer option)
		[HLEFunction(nid : 0xD6DA4BA1, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelCreateSema(string name, int attr, int initVal, int maxVal, TPointer option)
		{
			return Managers.semas.sceKernelCreateSema(name, attr, initVal, maxVal, option);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x28B6489C, version = 150, checkInsideInterrupt = true) public int sceKernelDeleteSema(@CheckArgument("checkSemaID") int semaid)
		[HLEFunction(nid : 0x28B6489C, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelDeleteSema(int semaid)
		{
			return Managers.semas.sceKernelDeleteSema(semaid);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x3F53E640, version = 150) public int sceKernelSignalSema(@CheckArgument("checkSemaID") int semaid, int signal)
		[HLEFunction(nid : 0x3F53E640, version : 150)]
		public virtual int sceKernelSignalSema(int semaid, int signal)
		{
			return Managers.semas.sceKernelSignalSema(semaid, signal);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x4E3A1105, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelWaitSema(@CheckArgument("checkSemaID") int semaid, int signal, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x4E3A1105, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelWaitSema(int semaid, int signal, TPointer32 timeoutAddr)
		{
			return Managers.semas.sceKernelWaitSema(semaid, signal, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x6D212BAC, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelWaitSemaCB(@CheckArgument("checkSemaID") int semaid, int signal, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x6D212BAC, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelWaitSemaCB(int semaid, int signal, TPointer32 timeoutAddr)
		{
			return Managers.semas.sceKernelWaitSemaCB(semaid, signal, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x58B1F937, version = 150) public int sceKernelPollSema(@CheckArgument("checkSemaID") int semaid, int signal)
		[HLEFunction(nid : 0x58B1F937, version : 150)]
		public virtual int sceKernelPollSema(int semaid, int signal)
		{
			return Managers.semas.sceKernelPollSema(semaid, signal);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8FFDF9A2, version = 150) public int sceKernelCancelSema(@CheckArgument("checkSemaID") int semaid, int newcount, @CanBeNull pspsharp.HLE.TPointer32 numWaitThreadAddr)
		[HLEFunction(nid : 0x8FFDF9A2, version : 150)]
		public virtual int sceKernelCancelSema(int semaid, int newcount, TPointer32 numWaitThreadAddr)
		{
			return Managers.semas.sceKernelCancelSema(semaid, newcount, numWaitThreadAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xBC6FEBC5, version = 150) public int sceKernelReferSemaStatus(@CheckArgument("checkSemaID") int semaid, pspsharp.HLE.TPointer addr)
		[HLEFunction(nid : 0xBC6FEBC5, version : 150)]
		public virtual int sceKernelReferSemaStatus(int semaid, TPointer addr)
		{
			return Managers.semas.sceKernelReferSemaStatus(semaid, addr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x55C20A00, version = 150, checkInsideInterrupt = true) public int sceKernelCreateEventFlag(String name, int attr, int initPattern, @CanBeNull pspsharp.HLE.TPointer option)
		[HLEFunction(nid : 0x55C20A00, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelCreateEventFlag(string name, int attr, int initPattern, TPointer option)
		{
			return Managers.eventFlags.sceKernelCreateEventFlag(name, attr, initPattern, option);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xEF9E4C70, version = 150, checkInsideInterrupt = true) public int sceKernelDeleteEventFlag(@CheckArgument("checkEventFlagID") int uid)
		[HLEFunction(nid : 0xEF9E4C70, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelDeleteEventFlag(int uid)
		{
			return Managers.eventFlags.sceKernelDeleteEventFlag(uid);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1FB15A32, version = 150) public int sceKernelSetEventFlag(@CheckArgument("checkEventFlagID") int uid, int bitsToSet)
		[HLEFunction(nid : 0x1FB15A32, version : 150)]
		public virtual int sceKernelSetEventFlag(int uid, int bitsToSet)
		{
			return Managers.eventFlags.sceKernelSetEventFlag(uid, bitsToSet);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x812346E4, version = 150) public int sceKernelClearEventFlag(@CheckArgument("checkEventFlagID") int uid, int bitsToKeep)
		[HLEFunction(nid : 0x812346E4, version : 150)]
		public virtual int sceKernelClearEventFlag(int uid, int bitsToKeep)
		{
			return Managers.eventFlags.sceKernelClearEventFlag(uid, bitsToKeep);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x402FCF22, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelWaitEventFlag(@CheckArgument("checkEventFlagID") int uid, int bits, int wait, @CanBeNull pspsharp.HLE.TPointer32 outBitsAddr, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x402FCF22, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelWaitEventFlag(int uid, int bits, int wait, TPointer32 outBitsAddr, TPointer32 timeoutAddr)
		{
			return Managers.eventFlags.sceKernelWaitEventFlag(uid, bits, wait, outBitsAddr, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x328C546A, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelWaitEventFlagCB(@CheckArgument("checkEventFlagID") int uid, int bits, int wait, @CanBeNull pspsharp.HLE.TPointer32 outBitsAddr, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x328C546A, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelWaitEventFlagCB(int uid, int bits, int wait, TPointer32 outBitsAddr, TPointer32 timeoutAddr)
		{
			return Managers.eventFlags.sceKernelWaitEventFlagCB(uid, bits, wait, outBitsAddr, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x30FD48F0, version = 150) public int sceKernelPollEventFlag(@CheckArgument("checkEventFlagID") int uid, int bits, int wait, @CanBeNull pspsharp.HLE.TPointer32 outBitsAddr)
		[HLEFunction(nid : 0x30FD48F0, version : 150)]
		public virtual int sceKernelPollEventFlag(int uid, int bits, int wait, TPointer32 outBitsAddr)
		{
			return Managers.eventFlags.sceKernelPollEventFlag(uid, bits, wait, outBitsAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xCD203292, version = 150) public int sceKernelCancelEventFlag(@CheckArgument("checkEventFlagID") int uid, int newPattern, @CanBeNull pspsharp.HLE.TPointer32 numWaitThreadAddr)
		[HLEFunction(nid : 0xCD203292, version : 150)]
		public virtual int sceKernelCancelEventFlag(int uid, int newPattern, TPointer32 numWaitThreadAddr)
		{
			return Managers.eventFlags.sceKernelCancelEventFlag(uid, newPattern, numWaitThreadAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA66B0120, version = 150) public int sceKernelReferEventFlagStatus(@CheckArgument("checkEventFlagID") int uid, pspsharp.HLE.TPointer addr)
		[HLEFunction(nid : 0xA66B0120, version : 150)]
		public virtual int sceKernelReferEventFlagStatus(int uid, TPointer addr)
		{
			return Managers.eventFlags.sceKernelReferEventFlagStatus(uid, addr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8125221D, version = 150, checkInsideInterrupt = true) public int sceKernelCreateMbx(String name, int attr, @CanBeNull pspsharp.HLE.TPointer option)
		[HLEFunction(nid : 0x8125221D, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelCreateMbx(string name, int attr, TPointer option)
		{
			return Managers.mbx.sceKernelCreateMbx(name, attr, option);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x86255ADA, version = 150, checkInsideInterrupt = true) public int sceKernelDeleteMbx(@CheckArgument("checkMbxID") int uid)
		[HLEFunction(nid : 0x86255ADA, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelDeleteMbx(int uid)
		{
			return Managers.mbx.sceKernelDeleteMbx(uid);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE9B3061E, version = 150) public int sceKernelSendMbx(@CheckArgument("checkMbxID") int uid, pspsharp.HLE.TPointer msgAddr)
		[HLEFunction(nid : 0xE9B3061E, version : 150)]
		public virtual int sceKernelSendMbx(int uid, TPointer msgAddr)
		{
			return Managers.mbx.sceKernelSendMbx(uid, msgAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x18260574, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelReceiveMbx(@CheckArgument("checkMbxID") int uid, pspsharp.HLE.TPointer32 addrMsgAddr, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x18260574, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelReceiveMbx(int uid, TPointer32 addrMsgAddr, TPointer32 timeoutAddr)
		{
			return Managers.mbx.sceKernelReceiveMbx(uid, addrMsgAddr, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF3986382, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelReceiveMbxCB(@CheckArgument("checkMbxID") int uid, pspsharp.HLE.TPointer32 addrMsgAddr, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0xF3986382, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelReceiveMbxCB(int uid, TPointer32 addrMsgAddr, TPointer32 timeoutAddr)
		{
			return Managers.mbx.sceKernelReceiveMbxCB(uid, addrMsgAddr, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0D81716A, version = 150) public int sceKernelPollMbx(@CheckArgument("checkMbxID") int uid, pspsharp.HLE.TPointer32 addrMsgAddr)
		[HLEFunction(nid : 0x0D81716A, version : 150)]
		public virtual int sceKernelPollMbx(int uid, TPointer32 addrMsgAddr)
		{
			return Managers.mbx.sceKernelPollMbx(uid, addrMsgAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x87D4DD36, version = 150) public int sceKernelCancelReceiveMbx(@CheckArgument("checkMbxID") int uid, @CanBeNull pspsharp.HLE.TPointer32 pnumAddr)
		[HLEFunction(nid : 0x87D4DD36, version : 150)]
		public virtual int sceKernelCancelReceiveMbx(int uid, TPointer32 pnumAddr)
		{
			return Managers.mbx.sceKernelCancelReceiveMbx(uid, pnumAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA8E8C846, version = 150) public int sceKernelReferMbxStatus(@CheckArgument("checkMbxID") int uid, pspsharp.HLE.TPointer infoAddr)
		[HLEFunction(nid : 0xA8E8C846, version : 150)]
		public virtual int sceKernelReferMbxStatus(int uid, TPointer infoAddr)
		{
			return Managers.mbx.sceKernelReferMbxStatus(uid, infoAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7C0DC2A0, version = 150, checkInsideInterrupt = true) public int sceKernelCreateMsgPipe(String name, int partitionid, int attr, int size, @CanBeNull pspsharp.HLE.TPointer option)
		[HLEFunction(nid : 0x7C0DC2A0, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelCreateMsgPipe(string name, int partitionid, int attr, int size, TPointer option)
		{
			return Managers.msgPipes.sceKernelCreateMsgPipe(name, partitionid, attr, size, option);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF0B7DA1C, version = 150, checkInsideInterrupt = true) public int sceKernelDeleteMsgPipe(@CheckArgument("checkMsgPipeID") int uid)
		[HLEFunction(nid : 0xF0B7DA1C, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelDeleteMsgPipe(int uid)
		{
			return Managers.msgPipes.sceKernelDeleteMsgPipe(uid);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x876DBFAD, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelSendMsgPipe(@CheckArgument("checkMsgPipeID") int uid, pspsharp.HLE.TPointer msgAddr, int size, int waitMode, @CanBeNull pspsharp.HLE.TPointer32 resultSizeAddr, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x876DBFAD, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelSendMsgPipe(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr, TPointer32 timeoutAddr)
		{
			return Managers.msgPipes.sceKernelSendMsgPipe(uid, msgAddr, size, waitMode, resultSizeAddr, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7C41F2C2, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelSendMsgPipeCB(@CheckArgument("checkMsgPipeID") int uid, pspsharp.HLE.TPointer msgAddr, int size, int waitMode, @CanBeNull pspsharp.HLE.TPointer32 resultSizeAddr, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x7C41F2C2, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelSendMsgPipeCB(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr, TPointer32 timeoutAddr)
		{
			return Managers.msgPipes.sceKernelSendMsgPipeCB(uid, msgAddr, size, waitMode, resultSizeAddr, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x884C9F90, version = 150) public int sceKernelTrySendMsgPipe(@CheckArgument("checkMsgPipeID") int uid, pspsharp.HLE.TPointer msgAddr, int size, int waitMode, @CanBeNull pspsharp.HLE.TPointer32 resultSizeAddr)
		[HLEFunction(nid : 0x884C9F90, version : 150)]
		public virtual int sceKernelTrySendMsgPipe(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr)
		{
			return Managers.msgPipes.sceKernelTrySendMsgPipe(uid, msgAddr, size, waitMode, resultSizeAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x74829B76, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelReceiveMsgPipe(@CheckArgument("checkMsgPipeID") int uid, pspsharp.HLE.TPointer msgAddr, int size, int waitMode, @CanBeNull pspsharp.HLE.TPointer32 resultSizeAddr, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x74829B76, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelReceiveMsgPipe(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr, TPointer32 timeoutAddr)
		{
			return Managers.msgPipes.sceKernelReceiveMsgPipe(uid, msgAddr, size, waitMode, resultSizeAddr, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xFBFA697D, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelReceiveMsgPipeCB(@CheckArgument("checkMsgPipeID") int uid, pspsharp.HLE.TPointer msgAddr, int size, int waitMode, @CanBeNull pspsharp.HLE.TPointer32 resultSizeAddr, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0xFBFA697D, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelReceiveMsgPipeCB(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr, TPointer32 timeoutAddr)
		{
			return Managers.msgPipes.sceKernelReceiveMsgPipeCB(uid, msgAddr, size, waitMode, resultSizeAddr, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xDF52098F, version = 150) public int sceKernelTryReceiveMsgPipe(@CheckArgument("checkMsgPipeID") int uid, pspsharp.HLE.TPointer msgAddr, int size, int waitMode, @CanBeNull pspsharp.HLE.TPointer32 resultSizeAddr)
		[HLEFunction(nid : 0xDF52098F, version : 150)]
		public virtual int sceKernelTryReceiveMsgPipe(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr)
		{
			return Managers.msgPipes.sceKernelTryReceiveMsgPipe(uid, msgAddr, size, waitMode, resultSizeAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x349B864D, version = 150) public int sceKernelCancelMsgPipe(@CheckArgument("checkMsgPipeID") int uid, @CanBeNull pspsharp.HLE.TPointer32 sendAddr, @CanBeNull pspsharp.HLE.TPointer32 recvAddr)
		[HLEFunction(nid : 0x349B864D, version : 150)]
		public virtual int sceKernelCancelMsgPipe(int uid, TPointer32 sendAddr, TPointer32 recvAddr)
		{
			return Managers.msgPipes.sceKernelCancelMsgPipe(uid, sendAddr, recvAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x33BE4024, version = 150) public int sceKernelReferMsgPipeStatus(@CheckArgument("checkMsgPipeID") int uid, pspsharp.HLE.TPointer infoAddr)
		[HLEFunction(nid : 0x33BE4024, version : 150)]
		public virtual int sceKernelReferMsgPipeStatus(int uid, TPointer infoAddr)
		{
			return Managers.msgPipes.sceKernelReferMsgPipeStatus(uid, infoAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x56C039B5, version = 150, checkInsideInterrupt = true) public int sceKernelCreateVpl(pspsharp.HLE.PspString name, @CheckArgument("checkPartitionID") int partitionid, int attr, int size, @CanBeNull pspsharp.HLE.TPointer option)
		[HLEFunction(nid : 0x56C039B5, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelCreateVpl(PspString name, int partitionid, int attr, int size, TPointer option)
		{
			return Managers.vpl.sceKernelCreateVpl(name, partitionid, attr, size, option);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x89B3D48C, version = 150, checkInsideInterrupt = true) public int sceKernelDeleteVpl(@CheckArgument("checkVplID") int uid)
		[HLEFunction(nid : 0x89B3D48C, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelDeleteVpl(int uid)
		{
			return Managers.vpl.sceKernelDeleteVpl(uid);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xBED27435, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelAllocateVpl(@CheckArgument("checkVplID") int uid, int size, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 dataAddr, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0xBED27435, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelAllocateVpl(int uid, int size, TPointer32 dataAddr, TPointer32 timeoutAddr)
		{
			return Managers.vpl.sceKernelAllocateVpl(uid, size, dataAddr, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xEC0A693F, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelAllocateVplCB(@CheckArgument("checkVplID") int uid, int size, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 dataAddr, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0xEC0A693F, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelAllocateVplCB(int uid, int size, TPointer32 dataAddr, TPointer32 timeoutAddr)
		{
			return Managers.vpl.sceKernelAllocateVplCB(uid, size, dataAddr, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xAF36D708, version = 150) public int sceKernelTryAllocateVpl(@CheckArgument("checkVplID") int uid, int size, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 dataAddr)
		[HLEFunction(nid : 0xAF36D708, version : 150)]
		public virtual int sceKernelTryAllocateVpl(int uid, int size, TPointer32 dataAddr)
		{
			return Managers.vpl.sceKernelTryAllocateVpl(uid, size, dataAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB736E9FF, version = 150, checkInsideInterrupt = true) public int sceKernelFreeVpl(@CheckArgument("checkVplID") int uid, pspsharp.HLE.TPointer dataAddr)
		[HLEFunction(nid : 0xB736E9FF, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelFreeVpl(int uid, TPointer dataAddr)
		{
			return Managers.vpl.sceKernelFreeVpl(uid, dataAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1D371B8A, version = 150) public int sceKernelCancelVpl(@CheckArgument("checkVplID") int uid, @CanBeNull pspsharp.HLE.TPointer32 numWaitThreadAddr)
		[HLEFunction(nid : 0x1D371B8A, version : 150)]
		public virtual int sceKernelCancelVpl(int uid, TPointer32 numWaitThreadAddr)
		{
			return Managers.vpl.sceKernelCancelVpl(uid, numWaitThreadAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x39810265, version = 150) public int sceKernelReferVplStatus(@CheckArgument("checkVplID") int uid, pspsharp.HLE.TPointer infoAddr)
		[HLEFunction(nid : 0x39810265, version : 150)]
		public virtual int sceKernelReferVplStatus(int uid, TPointer infoAddr)
		{
			return Managers.vpl.sceKernelReferVplStatus(uid, infoAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC07BB470, version = 150, checkInsideInterrupt = true) public int sceKernelCreateFpl(@CanBeNull pspsharp.HLE.PspString name, @CheckArgument("checkPartitionID") int partitionid, int attr, int blocksize, int blocks, @CanBeNull pspsharp.HLE.TPointer option)
		[HLEFunction(nid : 0xC07BB470, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelCreateFpl(PspString name, int partitionid, int attr, int blocksize, int blocks, TPointer option)
		{
			return Managers.fpl.sceKernelCreateFpl(name, partitionid, attr, blocksize, blocks, option);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xED1410E0, version = 150, checkInsideInterrupt = true) public int sceKernelDeleteFpl(@CheckArgument("checkFplID") int uid)
		[HLEFunction(nid : 0xED1410E0, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelDeleteFpl(int uid)
		{
			return Managers.fpl.sceKernelDeleteFpl(uid);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD979E9BF, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelAllocateFpl(@CheckArgument("checkFplID") int uid, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 dataAddr, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0xD979E9BF, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelAllocateFpl(int uid, TPointer32 dataAddr, TPointer32 timeoutAddr)
		{
			return Managers.fpl.sceKernelAllocateFpl(uid, dataAddr, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE7282CB6, version = 150, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelAllocateFplCB(@CheckArgument("checkFplID") int uid, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 dataAddr, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0xE7282CB6, version : 150, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelAllocateFplCB(int uid, TPointer32 dataAddr, TPointer32 timeoutAddr)
		{
			return Managers.fpl.sceKernelAllocateFplCB(uid, dataAddr, timeoutAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x623AE665, version = 150) public int sceKernelTryAllocateFpl(@CheckArgument("checkFplID") int uid, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 dataAddr)
		[HLEFunction(nid : 0x623AE665, version : 150)]
		public virtual int sceKernelTryAllocateFpl(int uid, TPointer32 dataAddr)
		{
			return Managers.fpl.sceKernelTryAllocateFpl(uid, dataAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF6414A71, version = 150, checkInsideInterrupt = true) public int sceKernelFreeFpl(@CheckArgument("checkFplID") int uid, pspsharp.HLE.TPointer dataAddr)
		[HLEFunction(nid : 0xF6414A71, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelFreeFpl(int uid, TPointer dataAddr)
		{
			return Managers.fpl.sceKernelFreeFpl(uid, dataAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA8AA591F, version = 150) public int sceKernelCancelFpl(@CheckArgument("checkFplID") int uid, @CanBeNull pspsharp.HLE.TPointer32 numWaitThreadAddr)
		[HLEFunction(nid : 0xA8AA591F, version : 150)]
		public virtual int sceKernelCancelFpl(int uid, TPointer32 numWaitThreadAddr)
		{
			return Managers.fpl.sceKernelCancelFpl(uid, numWaitThreadAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD8199E4C, version = 150) public int sceKernelReferFplStatus(@CheckArgument("checkFplID") int uid, pspsharp.HLE.TPointer infoAddr)
		[HLEFunction(nid : 0xD8199E4C, version : 150)]
		public virtual int sceKernelReferFplStatus(int uid, TPointer infoAddr)
		{
			return Managers.fpl.sceKernelReferFplStatus(uid, infoAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0E927AED, version = 150) public int _sceKernelReturnFromTimerHandler()
		[HLEFunction(nid : 0x0E927AED, version : 150)]
		public virtual int _sceKernelReturnFromTimerHandler()
		{
			return 0;
		}

		[HLEFunction(nid : 0x110DEC9A, version : 150)]
		public virtual int sceKernelUSec2SysClock(int usec, TPointer64 sysClockAddr)
		{
			return Managers.systime.sceKernelUSec2SysClock(usec, sysClockAddr);
		}

		[HLEFunction(nid : 0xC8CD158C, version : 150)]
		public virtual long sceKernelUSec2SysClockWide(int usec)
		{
			return Managers.systime.sceKernelUSec2SysClockWide(usec);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xBA6B92E2, version = 150) public int sceKernelSysClock2USec(pspsharp.HLE.TPointer64 sysClockAddr, @CanBeNull pspsharp.HLE.TPointer32 secAddr, @CanBeNull pspsharp.HLE.TPointer32 microSecAddr)
		[HLEFunction(nid : 0xBA6B92E2, version : 150)]
		public virtual int sceKernelSysClock2USec(TPointer64 sysClockAddr, TPointer32 secAddr, TPointer32 microSecAddr)
		{
			return Managers.systime.sceKernelSysClock2USec(sysClockAddr, secAddr, microSecAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE1619D7C, version = 150) public int sceKernelSysClock2USecWide(long sysClock, @CanBeNull pspsharp.HLE.TPointer32 secAddr, @CanBeNull pspsharp.HLE.TPointer32 microSecAddr)
		[HLEFunction(nid : 0xE1619D7C, version : 150)]
		public virtual int sceKernelSysClock2USecWide(long sysClock, TPointer32 secAddr, TPointer32 microSecAddr)
		{
			return Managers.systime.sceKernelSysClock2USecWide(sysClock, secAddr, microSecAddr);
		}

		[HLEFunction(nid : 0xDB738F35, version : 150)]
		public virtual int sceKernelGetSystemTime(TPointer64 timeAddr)
		{
			return Managers.systime.sceKernelGetSystemTime(timeAddr);
		}

		[HLEFunction(nid : 0x82BC5777, version : 150)]
		public virtual long sceKernelGetSystemTimeWide()
		{
			return Managers.systime.sceKernelGetSystemTimeWide();
		}

		[HLEFunction(nid : 0x369ED59D, version : 150)]
		public virtual int sceKernelGetSystemTimeLow()
		{
			return Managers.systime.sceKernelGetSystemTimeLow();
		}

		/// <summary>
		/// Set an alarm. </summary>
		/// <param name="delayUsec"> - The number of micro seconds till the alarm occurs. </param>
		/// <param name="handlerAddress"> - Pointer to a ::SceKernelAlarmHandler </param>
		/// <param name="handlerArgument"> - Common pointer for the alarm handler
		/// </param>
		/// <returns> A UID representing the created alarm, < 0 on error. </returns>
		[HLEFunction(nid : 0x6652B8CA, version : 150)]
		public virtual int sceKernelSetAlarm(int delayUsec, TPointer handlerAddress, int handlerArgument)
		{
			// delayUsec is an unsigned 32-bit value
			return hleKernelSetAlarm(delayUsec & 0xFFFFFFFFL, handlerAddress, handlerArgument);
		}

		/// <summary>
		/// Set an alarm using a ::SceKernelSysClock structure for the time
		/// </summary>
		/// <param name="delaySysclockAddr"> - Pointer to a ::SceKernelSysClock structure </param>
		/// <param name="handlerAddress"> - Pointer to a ::SceKernelAlarmHandler </param>
		/// <param name="handlerArgument"> - Common pointer for the alarm handler.
		/// </param>
		/// <returns> A UID representing the created alarm, < 0 on error. </returns>
		[HLEFunction(nid : 0xB2C25152, version : 150)]
		public virtual int sceKernelSetSysClockAlarm(TPointer64 delaySysclockAddr, TPointer handlerAddress, int handlerArgument)
		{
			long delaySysclock = delaySysclockAddr.Value;
			long delayUsec = SystemTimeManager.hleSysClock2USec(delaySysclock);

			return hleKernelSetAlarm(delayUsec, handlerAddress, handlerArgument);
		}

		/// <summary>
		/// Cancel a pending alarm.
		/// </summary>
		/// <param name="alarmUid"> - UID of the alarm to cancel.
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7E65B999, version = 150) public int sceKernelCancelAlarm(@CheckArgument("checkAlarmID") int alarmUid)
		[HLEFunction(nid : 0x7E65B999, version : 150)]
		public virtual int sceKernelCancelAlarm(int alarmUid)
		{
			SceKernelAlarmInfo sceKernelAlarmInfo = alarms[alarmUid];
			cancelAlarm(sceKernelAlarmInfo);

			return 0;
		}

		/// <summary>
		/// Refer the status of a created alarm.
		/// </summary>
		/// <param name="alarmUid"> - UID of the alarm to get the info of </param>
		/// <param name="infoAddr"> - Pointer to a ::SceKernelAlarmInfo structure
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xDAA3F564, version = 150) public int sceKernelReferAlarmStatus(@CheckArgument("checkAlarmID") int alarmUid, pspsharp.HLE.TPointer infoAddr)
		[HLEFunction(nid : 0xDAA3F564, version : 150)]
		public virtual int sceKernelReferAlarmStatus(int alarmUid, TPointer infoAddr)
		{
			SceKernelAlarmInfo sceKernelAlarmInfo = alarms[alarmUid];
			sceKernelAlarmInfo.write(infoAddr);

			return 0;
		}

		/// <summary>
		/// Create a virtual timer
		/// </summary>
		/// <param name="nameAddr"> - Name for the timer. </param>
		/// <param name="optAddr">  - Pointer to an ::SceKernelVTimerOptParam (pass NULL)
		/// </param>
		/// <returns> The VTimer's UID or < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x20FFF560, version = 150, checkInsideInterrupt = true) public int sceKernelCreateVTimer(String name, @CanBeNull pspsharp.HLE.TPointer optAddr)
		[HLEFunction(nid : 0x20FFF560, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelCreateVTimer(string name, TPointer optAddr)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = new SceKernelVTimerInfo(name);
			vtimers[sceKernelVTimerInfo.uid] = sceKernelVTimerInfo;

			return sceKernelVTimerInfo.uid;
		}

		/// <summary>
		/// Delete a virtual timer
		/// </summary>
		/// <param name="vtimerUid"> - The UID of the timer
		/// </param>
		/// <returns> < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x328F9E52, version = 150, checkInsideInterrupt = true) public int sceKernelDeleteVTimer(@CheckArgument("checkVTimerID") int vtimerUid)
		[HLEFunction(nid : 0x328F9E52, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelDeleteVTimer(int vtimerUid)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = vtimers.Remove(vtimerUid);
			sceKernelVTimerInfo.delete();

			return 0;
		}

		/// <summary>
		/// Get the timer base
		/// </summary>
		/// <param name="vtimerUid"> - UID of the vtimer </param>
		/// <param name="baseAddr"> - Pointer to a ::SceKernelSysClock structure
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB3A59970, version = 150) public int sceKernelGetVTimerBase(@CheckArgument("checkVTimerID") int vtimerUid, pspsharp.HLE.TPointer64 baseAddr)
		[HLEFunction(nid : 0xB3A59970, version : 150)]
		public virtual int sceKernelGetVTimerBase(int vtimerUid, TPointer64 baseAddr)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = vtimers[vtimerUid];
			baseAddr.Value = sceKernelVTimerInfo.@base;

			return 0;
		}

		/// <summary>
		/// Get the timer base (wide format)
		/// </summary>
		/// <param name="vtimerUid"> - UID of the vtimer
		/// </param>
		/// <returns> The 64bit timer base </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB7C18B77, version = 150) public long sceKernelGetVTimerBaseWide(@CheckArgument("checkVTimerID") int vtimerUid)
		[HLEFunction(nid : 0xB7C18B77, version : 150)]
		public virtual long sceKernelGetVTimerBaseWide(int vtimerUid)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = vtimers[vtimerUid];

			return sceKernelVTimerInfo.@base;
		}

		/// <summary>
		/// Get the timer time
		/// </summary>
		/// <param name="vtimerUid"> - UID of the vtimer </param>
		/// <param name="timeAddr"> - Pointer to a ::SceKernelSysClock structure
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x034A921F, version = 150) public int sceKernelGetVTimerTime(@CheckArgument("checkVTimerID") int vtimerUid, pspsharp.HLE.TPointer64 timeAddr)
		[HLEFunction(nid : 0x034A921F, version : 150)]
		public virtual int sceKernelGetVTimerTime(int vtimerUid, TPointer64 timeAddr)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = vtimers[vtimerUid];
			long time = sceKernelVTimerInfo.CurrentTime;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelGetVTimerTime returning {0:D}", time));
			}
			timeAddr.Value = time;

			return 0;
		}

		/// <summary>
		/// Get the timer time (wide format)
		/// </summary>
		/// <param name="vtimerUid"> - UID of the vtimer
		/// </param>
		/// <returns> The 64bit timer time </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC0B3FFD2, version = 150) public long sceKernelGetVTimerTimeWide(@CheckArgument("checkVTimerID") int vtimerUid)
		[HLEFunction(nid : 0xC0B3FFD2, version : 150)]
		public virtual long sceKernelGetVTimerTimeWide(int vtimerUid)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = vtimers[vtimerUid];
			long time = sceKernelVTimerInfo.CurrentTime;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelGetVTimerTimeWide returning {0:D}", time));
			}

			return time;
		}

		/// <summary>
		/// Set the timer time
		/// </summary>
		/// <param name="vtimerUid"> - UID of the vtimer </param>
		/// <param name="timeAddr"> - Pointer to a ::SceKernelSysClock structure
		///                   The previous value of the vtimer is returned back in this structure.
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x542AD630, version = 150, checkInsideInterrupt = true) public int sceKernelSetVTimerTime(@CheckArgument("checkVTimerID") int vtimerUid, pspsharp.HLE.TPointer64 timeAddr)
		[HLEFunction(nid : 0x542AD630, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelSetVTimerTime(int vtimerUid, TPointer64 timeAddr)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = vtimers[vtimerUid];
			long time = timeAddr.Value;
			timeAddr.Value = setVTimer(sceKernelVTimerInfo, time);

			return 0;
		}

		/// <summary>
		/// Set the timer time (wide format)
		/// </summary>
		/// <param name="vtimerUid"> - UID of the vtimer </param>
		/// <param name="time"> - a ::SceKernelSysClock structure
		/// </param>
		/// <returns> the last time of the vtimer or -1 if the vtimerUid is invalid </returns>
		[HLEFunction(nid : 0xFB6425C3, version : 150, checkInsideInterrupt : true)]
		public virtual long sceKernelSetVTimerTimeWide(int vtimerUid, long time)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = vtimers[vtimerUid];
			if (sceKernelVTimerInfo == null)
			{
				// sceKernelSetVTimerTimeWide returns -1 instead of ERROR_KERNEL_NOT_FOUND_VTIMER
				// when the vtimerUid is invalid.
				return -1;
			}

			return setVTimer(sceKernelVTimerInfo, time);
		}

		/// <summary>
		/// Start a virtual timer
		/// </summary>
		/// <param name="vtimerUid"> - The UID of the timer
		/// </param>
		/// <returns> < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC68D9437, version = 150) public int sceKernelStartVTimer(@CheckArgument("checkVTimerID") int vtimerUid)
		[HLEFunction(nid : 0xC68D9437, version : 150)]
		public virtual int sceKernelStartVTimer(int vtimerUid)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = vtimers[vtimerUid];
			if (sceKernelVTimerInfo.active == SceKernelVTimerInfo.ACTIVE_RUNNING)
			{
				return 1; // already started
			}

			startVTimer(sceKernelVTimerInfo);

			return 0;
		}

		/// <summary>
		/// Stop a virtual timer
		/// </summary>
		/// <param name="vtimerUid"> - The UID of the timer
		/// </param>
		/// <returns> < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD0AEEE87, version = 150) public int sceKernelStopVTimer(@CheckArgument("checkVTimerID") int vtimerUid)
		[HLEFunction(nid : 0xD0AEEE87, version : 150)]
		public virtual int sceKernelStopVTimer(int vtimerUid)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = vtimers[vtimerUid];
			if (sceKernelVTimerInfo.active == SceKernelVTimerInfo.ACTIVE_STOPPED)
			{
				return 0; // already stopped
			}

			stopVTimer(sceKernelVTimerInfo);

			return 1;
		}

		/// <summary>
		/// Set the timer handler
		/// </summary>
		/// <param name="vtimerUid"> - UID of the vtimer </param>
		/// <param name="scheduleAddr"> - Time to call the handler </param>
		/// <param name="handlerAddress"> - The timer handler </param>
		/// <param name="handlerArgument">  - Common pointer
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD8B299AE, version = 150) public int sceKernelSetVTimerHandler(@CheckArgument("checkVTimerID") int vtimerUid, pspsharp.HLE.TPointer64 scheduleAddr, @CanBeNull pspsharp.HLE.TPointer handlerAddress, int handlerArgument)
		[HLEFunction(nid : 0xD8B299AE, version : 150)]
		public virtual int sceKernelSetVTimerHandler(int vtimerUid, TPointer64 scheduleAddr, TPointer handlerAddress, int handlerArgument)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = vtimers[vtimerUid];
			long schedule = scheduleAddr.Value;
			sceKernelVTimerInfo.handlerAddress = handlerAddress.Address;
			sceKernelVTimerInfo.handlerArgument = handlerArgument;
			scheduleVTimer(sceKernelVTimerInfo, schedule);

			return 0;
		}

		/// <summary>
		/// Set the timer handler (wide mode)
		/// </summary>
		/// <param name="vtimerUid"> - UID of the vtimer </param>
		/// <param name="schedule"> - Time to call the handler </param>
		/// <param name="handlerAddress"> - The timer handler </param>
		/// <param name="handlerArgument">  - Common pointer
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x53B00E9A, version = 150) public int sceKernelSetVTimerHandlerWide(@CheckArgument("checkVTimerID") int vtimerUid, long schedule, pspsharp.HLE.TPointer handlerAddress, int handlerArgument)
		[HLEFunction(nid : 0x53B00E9A, version : 150)]
		public virtual int sceKernelSetVTimerHandlerWide(int vtimerUid, long schedule, TPointer handlerAddress, int handlerArgument)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = vtimers[vtimerUid];
			sceKernelVTimerInfo.handlerAddress = handlerAddress.Address;
			sceKernelVTimerInfo.handlerArgument = handlerArgument;
			scheduleVTimer(sceKernelVTimerInfo, schedule);

			return 0;
		}

		/// <summary>
		/// Cancel the timer handler
		/// </summary>
		/// <param name="vtimerUid"> - The UID of the vtimer
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xD2D615EF, version = 150) public int sceKernelCancelVTimerHandler(@CheckArgument("checkVTimerID") int vtimerUid)
		[HLEFunction(nid : 0xD2D615EF, version : 150)]
		public virtual int sceKernelCancelVTimerHandler(int vtimerUid)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = vtimers[vtimerUid];
			cancelVTimer(sceKernelVTimerInfo);

			return 0;
		}

		/// <summary>
		/// Get the status of a VTimer
		/// </summary>
		/// <param name="vtimerUid"> - The uid of the VTimer </param>
		/// <param name="infoAddr"> - Pointer to a ::SceKernelVTimerInfo structure
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x5F32BEAA, version = 150) public int sceKernelReferVTimerStatus(@CheckArgument("checkVTimerID") int vtimerUid, pspsharp.HLE.TPointer infoAddr)
		[HLEFunction(nid : 0x5F32BEAA, version : 150)]
		public virtual int sceKernelReferVTimerStatus(int vtimerUid, TPointer infoAddr)
		{
			SceKernelVTimerInfo sceKernelVTimerInfo = vtimers[vtimerUid];
			sceKernelVTimerInfo.write(infoAddr);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x446D8DE6, version = 150) public int sceKernelCreateThread(@StringInfo(maxLength = 32) String name, int entry_addr, int initPriority, int stackSize, int attr, int option_addr)
		[HLEFunction(nid : 0x446D8DE6, version : 150)]
		public virtual int sceKernelCreateThread(string name, int entry_addr, int initPriority, int stackSize, int attr, int option_addr)
		{
			int mpidStack = USER_PARTITION_ID;
			// Inherit kernel mode if user mode bit is not set
			if (currentThread.KernelMode && !SceKernelThreadInfo.isUserMode(attr))
			{
				log.debug("sceKernelCreateThread inheriting kernel mode");
				attr |= PSP_THREAD_ATTR_KERNEL;
				mpidStack = KERNEL_PARTITION_ID;
			}

			SceKernelThreadInfo thread = hleKernelCreateThread(name, entry_addr, initPriority, stackSize, attr, option_addr, mpidStack);

			if (thread.stackSize > 0 && thread.StackAddr == 0)
			{
				log.warn("sceKernelCreateThread not enough memory to create the stack");
				hleDeleteThread(thread);
				return SceKernelErrors.ERROR_KERNEL_NO_MEMORY;
			}

			// Inherit user mode
			if (currentThread.UserMode)
			{
				if (!SceKernelThreadInfo.isUserMode(thread.attr))
				{
					log.debug("sceKernelCreateThread inheriting user mode");
				}
				thread.attr |= PSP_THREAD_ATTR_USER;
				// Always remove kernel mode bit
				thread.attr &= ~PSP_THREAD_ATTR_KERNEL;
			}

			triggerThreadEvent(thread, currentThread, THREAD_EVENT_CREATE);

			return thread.uid;
		}

		/// <summary>
		/// mark a thread for deletion. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x9FA03CD3, version = 150, checkInsideInterrupt = true) public int sceKernelDeleteThread(@CheckArgument("checkThreadIDAllow0") int uid)
		[HLEFunction(nid : 0x9FA03CD3, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelDeleteThread(int uid)
		{
			SceKernelThreadInfo thread = threadMap[uid];
			if (!thread.Stopped)
			{
				// This error is also returned for the current thread or thread id 0 (the current thread isn't stopped).
				return ERROR_KERNEL_THREAD_IS_NOT_DORMANT;
			}

			// Mark thread for deletion
			ToBeDeletedThread = thread;

			triggerThreadEvent(thread, currentThread, THREAD_EVENT_DELETE);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF475845D, version = 150, checkInsideInterrupt = true) public int sceKernelStartThread(@CheckArgument("checkThreadID") int uid, int len, int data_addr)
		[HLEFunction(nid : 0xF475845D, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelStartThread(int uid, int len, int data_addr)
		{
			SceKernelThreadInfo thread = threadMap[uid];

			if (!thread.Stopped)
			{
				return ERROR_KERNEL_THREAD_IS_NOT_DORMANT;
			}

			hleKernelStartThread(thread, len, data_addr, thread.gpReg_addr);

			return 0;
		}

		[HLEFunction(nid : 0x532A522E, version : 150)]
		public virtual int _sceKernelExitThread(int exitStatus)
		{
			// _sceKernelExitThread is equivalent to sceKernelExitThread
			return sceKernelExitThread(exitStatus);
		}

		/// <summary>
		/// exit the current thread </summary>
		[HLEFunction(nid : 0xAA73C935, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelExitThread(int exitStatus)
		{
			// PSP is only returning an error for a SDK after 3.07
			if (!DispatchThreadEnabled && Modules.SysMemUserForUserModule.hleKernelGetCompiledSdkVersion() > 0x0307FFFF)
			{
				return SceKernelErrors.ERROR_KERNEL_WAIT_CAN_NOT_WAIT;
			}

			SceKernelThreadInfo thread = currentThread;

			if (exitStatus < 0)
			{
				thread.ExitStatus = ERROR_KERNEL_ILLEGAL_ARGUMENT;
			}
			else
			{
				thread.ExitStatus = exitStatus;
			}

			triggerThreadEvent(thread, currentThread, THREAD_EVENT_EXIT);

			hleChangeThreadState(thread, PSP_THREAD_STOPPED);
			RuntimeContext.onThreadExit(thread);
			hleRescheduleCurrentThread();

			return 0;
		}

		/// <summary>
		/// exit the current thread, then delete it </summary>
		[HLEFunction(nid : 0x809CE29B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelExitDeleteThread(int exitStatus)
		{
			// PSP is only returning an error for a SDK after 3.07
			if (!DispatchThreadEnabled && Modules.SysMemUserForUserModule.hleKernelGetCompiledSdkVersion() > 0x0307FFFF)
			{
				return SceKernelErrors.ERROR_KERNEL_WAIT_CAN_NOT_WAIT;
			}

			SceKernelThreadInfo thread = currentThread;
			thread.ExitStatus = exitStatus;

			triggerThreadEvent(thread, currentThread, THREAD_EVENT_EXIT);
			triggerThreadEvent(thread, currentThread, THREAD_EVENT_DELETE);

			hleChangeThreadState(thread, PSP_THREAD_STOPPED);
			RuntimeContext.onThreadExit(thread);
			ToBeDeletedThread = thread;
			hleRescheduleCurrentThread();

			return 0;
		}

		/// <summary>
		/// terminate thread </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x616403BA, version = 150) public int sceKernelTerminateThread(@CheckArgument("checkThreadID") int uid)
		[HLEFunction(nid : 0x616403BA, version : 150)]
		public virtual int sceKernelTerminateThread(int uid)
		{
			// PSP is only returning an error for a SDK after 3.07
			if (IntrManager.Instance.InsideInterrupt && Modules.SysMemUserForUserModule.hleKernelGetCompiledSdkVersion() > 0x0307FFFF)
			{
				return SceKernelErrors.ERROR_KERNEL_CANNOT_BE_CALLED_FROM_INTERRUPT;
			}
			// PSP is only returning an error for a SDK after 3.07
			if (!DispatchThreadEnabled && Modules.SysMemUserForUserModule.hleKernelGetCompiledSdkVersion() > 0x0307FFFF)
			{
				return SceKernelErrors.ERROR_KERNEL_WAIT_CAN_NOT_WAIT;
			}

			SceKernelThreadInfo thread = getThreadCurrentIsInvalid(uid);

			triggerThreadEvent(thread, currentThread, THREAD_EVENT_EXIT);

			// Return this exit status to threads currently waiting on the thread end
			thread.ExitStatus = ERROR_KERNEL_THREAD_IS_TERMINATED;

			terminateThread(thread);

			// Return this exit status to threads that will wait on this thread end later on
			thread.ExitStatus = ERROR_KERNEL_THREAD_ALREADY_DORMANT;

			return 0;
		}

		/// <summary>
		/// terminate thread, then mark it for deletion </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x383F7BCC, version = 150, checkInsideInterrupt = true) public int sceKernelTerminateDeleteThread(@CheckArgument("checkThreadID") int uid)
		[HLEFunction(nid : 0x383F7BCC, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelTerminateDeleteThread(int uid)
		{
			// PSP is only returning an error for a SDK after 3.07
			if (!DispatchThreadEnabled && Modules.SysMemUserForUserModule.hleKernelGetCompiledSdkVersion() > 0x0307FFFF)
			{
				return SceKernelErrors.ERROR_KERNEL_WAIT_CAN_NOT_WAIT;
			}

			SceKernelThreadInfo thread = getThreadCurrentIsInvalid(uid);

			triggerThreadEvent(thread, currentThread, THREAD_EVENT_EXIT);
			triggerThreadEvent(thread, currentThread, THREAD_EVENT_DELETE);

			terminateThread(thread);
			ToBeDeletedThread = thread;

			return 0;
		}

		/// <summary>
		/// Suspend the dispatch thread
		/// </summary>
		/// <returns> The current state of the dispatch thread, < 0 on error </returns>
		[HLEFunction(nid : 0x3AD58B8C, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelSuspendDispatchThread(Processor processor)
		{
			int state = DispatchThreadState;

			if (log.DebugEnabled)
			{
				log.debug("sceKernelSuspendDispatchThread() state=" + state);
			}

			if (processor.InterruptsDisabled)
			{
				return SceKernelErrors.ERROR_KERNEL_INTERRUPTS_ALREADY_DISABLED;
			}

			dispatchThreadEnabled = false;
			return state;
		}

		/// <summary>
		/// Resume the dispatch thread
		/// </summary>
		/// <param name="state"> - The state of the dispatch thread
		///                (from sceKernelSuspendDispatchThread) </param>
		/// <returns> 0 on success, < 0 on error </returns>
		[HLEFunction(nid : 0x27E22EC2, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelResumeDispatchThread(Processor processor, int state)
		{
			bool isInterruptsDisabled = processor.InterruptsDisabled;

			if (state == SCE_KERNEL_DISPATCHTHREAD_STATE_ENABLED)
			{
				hleKernelResumeDispatchThread();
			}

			if (isInterruptsDisabled)
			{
				return SceKernelErrors.ERROR_KERNEL_INTERRUPTS_ALREADY_DISABLED;
			}

			return 0;
		}

		[HLEFunction(nid : 0xEA748E31, version : 150)]
		public virtual int sceKernelChangeCurrentThreadAttr(int removeAttr, int addAttr)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelChangeCurrentThreadAttr removeAttr=0x{0:X}, addAttr=0x{1:X}, currentAttr=0x{2:X}", removeAttr, addAttr, currentThread.attr));
			}

			int newAttr = (currentThread.attr & ~removeAttr) | addAttr;
			// Don't allow switching into kernel mode!
			if (userCurrentThreadTryingToSwitchToKernelMode(newAttr))
			{
				log.debug("sceKernelChangeCurrentThreadAttr forcing user mode");
				newAttr |= PSP_THREAD_ATTR_USER;
				newAttr &= ~PSP_THREAD_ATTR_KERNEL;
			}
			currentThread.attr = newAttr;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x71BC9871, version = 150) public int sceKernelChangeThreadPriority(@CheckArgument("checkThreadIDAllow0") int uid, @CheckArgument("checkThreadPriority") int priority)
		[HLEFunction(nid : 0x71BC9871, version : 150)]
		public virtual int sceKernelChangeThreadPriority(int uid, int priority)
		{
			SceKernelThreadInfo thread = getThreadById(uid);

			if (thread.Stopped)
			{
				// Tested on PSP:
				// If the thread is stopped, it's current priority is replaced by it's initial priority.
				thread.currentPriority = thread.initPriority;
				return ERROR_KERNEL_THREAD_ALREADY_DORMANT;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelChangeThreadPriority thread={0}, newPriority=0x{1:X}, oldPriority=0x{2:X}", thread, priority, thread.currentPriority));
			}
			hleKernelChangeThreadPriority(thread, priority);

			return 0;
		}

		public virtual int checkThreadPriority(int priority)
		{
			// Priority 0 means priority of the calling thread
			if (priority == 0)
			{
				priority = currentThread.currentPriority;
			}

			if (currentThread.UserMode)
			{
				// Value priority range in user mode: [8..119]
				if (priority < 8 || priority >= 120)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("checkThreadPriority priority:0x{0:x} is outside of valid range in user mode", priority));
					}
					throw (new SceKernelErrorException(ERROR_KERNEL_ILLEGAL_PRIORITY));
				}
			}

			if (currentThread.KernelMode)
			{
				// Value priority range in kernel mode: [1..126]
				if (priority < 1 || priority >= 127)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("checkThreadPriority priority:0x{0:x} is outside of valid range in kernel mode", priority));
					}
					throw (new SceKernelErrorException(ERROR_KERNEL_ILLEGAL_PRIORITY));
				}
			}

			return priority;
		}

		protected internal virtual SceKernelThreadInfo getThreadCurrentIsInvalid(int uid)
		{
			SceKernelThreadInfo thread = getThreadById(uid);
			if (thread == currentThread)
			{
				throw (new SceKernelErrorException(ERROR_KERNEL_ILLEGAL_THREAD));
			}
			return thread;
		}

		/// <summary>
		/// Rotate thread ready queue at a set priority
		/// </summary>
		/// <param name="priority"> - The priority of the queue
		/// </param>
		/// <returns> 0 on success, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x912354A7, version = 150) public int sceKernelRotateThreadReadyQueue(@CheckArgument("checkThreadPriority") int priority)
		[HLEFunction(nid : 0x912354A7, version : 150)]
		public virtual int sceKernelRotateThreadReadyQueue(int priority)
		{
			lock (readyThreads)
			{
				foreach (SceKernelThreadInfo thread in readyThreads)
				{
					if (thread.currentPriority == priority)
					{
						// When rotating the ready queue of the current thread,
						// the current thread yields and is moved to the end of its
						// ready queue.
						if (priority == currentThread.currentPriority)
						{
							thread = currentThread;
							// The current thread will be moved to the front of the ready queue
							hleChangeThreadState(thread, PSP_THREAD_READY);
						}
						// Move the thread to the end of the ready queue
						removeFromReadyThreads(thread);
						addToReadyThreads(thread, false);
						hleRescheduleCurrentThread();
						break;
					}
				}
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x2C34E053, version = 150) public int sceKernelReleaseWaitThread(@CheckArgument("checkThreadID") int uid)
		[HLEFunction(nid : 0x2C34E053, version : 150)]
		public virtual int sceKernelReleaseWaitThread(int uid)
		{
			SceKernelThreadInfo thread = getThreadCurrentIsInvalid(uid);

			if (!thread.Waiting)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceKernelReleaseWaitThread(0x{0:X}): thread not waiting: {1}", uid, thread));
				}
				return SceKernelErrors.ERROR_KERNEL_THREAD_IS_NOT_WAIT;
			}

			// If the application is waiting on a internal condition,
			// return an illegal permission
			// (e.g. on a real PSP, it would be the case for a
			//  sceKernelWaitEventFlag issued internally by a syscall).
			if (thread.waitType >= SceKernelThreadInfo.JPCSP_FIRST_INTERNAL_WAIT_TYPE)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceKernelReleaseWaitThread(0x{0:X}): thread waiting in privileged status: waitType=0x{1:X}", uid, thread.waitType));
				}
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_PERMISSION;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelReleaseWaitThread(0x{0:X}): releasing waiting thread: {1}", uid, thread));
			}

			hleThreadWaitRelease(thread);

			// Check if we need to switch to the released thread
			// (e.g. has a higher priority)
			hleRescheduleCurrentThread();

			return 0;
		}

		/// <summary>
		/// Get the current thread Id </summary>
		[HLEFunction(nid : 0x293B45B8, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelGetThreadId()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelGetThreadId returning uid=0x{0:X}", currentThread.uid));
			}

			return currentThread.uid;
		}

		[HLEFunction(nid : 0x94AA61EE, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelGetThreadCurrentPriority()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelGetThreadCurrentPriority returning currentPriority={0:D}", currentThread.currentPriority));
			}

			return currentThread.currentPriority;
		}

		/// <returns> ERROR_NOT_FOUND_THREAD on uid < 0, uid == 0 and thread not found </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x3B183E26, version = 150) public int sceKernelGetThreadExitStatus(@CheckArgument("checkThreadIDNoCheck0") int uid)
		[HLEFunction(nid : 0x3B183E26, version : 150)]
		public virtual int sceKernelGetThreadExitStatus(int uid)
		{
			SceKernelThreadInfo thread = getThreadById(uid);
			if (!thread.Stopped)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceKernelGetThreadExitStatus not stopped uid=0x{0:x}", uid));
				}
				return ERROR_KERNEL_THREAD_IS_NOT_DORMANT;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelGetThreadExitStatus thread={0} returning exitStatus=0x{1:X8}", thread, thread.exitStatus));
			}

			return thread.exitStatus;
		}

		/// <returns> amount of free stack space. </returns>
		[HLEFunction(nid : 0xD13BDE95, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelCheckThreadStack(Processor processor)
		{
			int size = getThreadCurrentStackSize(processor);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelCheckThreadStack returning size=0x{0:X}", size));
			}

			return size;
		}

		/// <returns> amount of unused stack space of a thread.
		///  </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x52089CA1, version = 150, checkInsideInterrupt = true) public int sceKernelGetThreadStackFreeSize(@CheckArgument("checkThreadIDAllow0") int uid)
		[HLEFunction(nid : 0x52089CA1, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelGetThreadStackFreeSize(int uid)
		{
			SceKernelThreadInfo thread = getThreadById(uid);

			// The stack is filled with 0xFF when the thread starts.
			// Scan for the unused stack space by looking for the first 32-bit value
			// differing from 0xFFFFFFFF.
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(thread.StackAddr, thread.stackSize, 4);
			int unusedStackSize = thread.stackSize;
			for (int i = 0; i < thread.stackSize; i += 4)
			{
				int stackValue = memoryReader.readNext();
				if (stackValue != -1)
				{
					unusedStackSize = i;
					break;
				}
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelGetThreadStackFreeSize returning size=0x{0:X}", unusedStackSize));
			}

			return unusedStackSize;
		}

		/// <summary>
		/// Get the status information for the specified thread
		/// 
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x17C1684E, version = 150) public int sceKernelReferThreadStatus(@CheckArgument("checkThreadIDAllow0") int uid, pspsharp.HLE.TPointer addr)
		[HLEFunction(nid : 0x17C1684E, version : 150)]
		public virtual int sceKernelReferThreadStatus(int uid, TPointer addr)
		{
			SceKernelThreadInfo thread = getThreadById(uid);
			thread.write(addr);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xFFC36A14, version = 150, checkInsideInterrupt = true) public int sceKernelReferThreadRunStatus(@CheckArgument("checkThreadIDAllow0") int uid, pspsharp.HLE.TPointer addr)
		[HLEFunction(nid : 0xFFC36A14, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelReferThreadRunStatus(int uid, TPointer addr)
		{
			SceKernelThreadInfo thread = getThreadById(uid);
			thread.writeRunStatus(addr);

			return 0;
		}

		/// <summary>
		/// Get the current system status.
		/// </summary>
		/// <param name="status"> - Pointer to a ::SceKernelSystemStatus structure.
		/// </param>
		/// <returns> < 0 on error. </returns>
		[HLEFunction(nid : 0x627E6F3A, version : 150)]
		public virtual int sceKernelReferSystemStatus(TPointer statusPtr)
		{
			SceKernelSystemStatus status = new SceKernelSystemStatus();
			status.read(statusPtr);
			status.status = 0;
			status.write(statusPtr);

			return 0;
		}

		/// <summary>
		/// Write uid's to buffer
		/// return written count
		/// save full count to idcount_addr 
		/// </summary>
		[HLEFunction(nid : 0x94416130, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelGetThreadmanIdList(int type, TPointer32 readBufPtr, int readBufSize, TPointer32 idCountPtr)
		{
			if (type != SCE_KERNEL_TMID_Thread)
			{
				log.warn(string.Format("UNIMPLEMENTED:sceKernelGetThreadmanIdList type={0:D}", type));
				idCountPtr.setValue(0);
				return 0;
			}

			int saveCount = 0;
			int fullCount = 0;

			foreach (SceKernelThreadInfo thread in threadMap.Values)
			{
				// Hide kernel mode threads when called from a user mode thread
				if (userThreadCalledKernelCurrentThread(thread))
				{
					if (saveCount < readBufSize)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceKernelGetThreadmanIdList adding thread {0}", thread));
						}
						readBufPtr.setValue(saveCount << 2, thread.uid);
						saveCount++;
					}
					else
					{
						log.warn(string.Format("sceKernelGetThreadmanIdList NOT adding thread {0} (no more space)", thread));
					}
					fullCount++;
				}
			}

			idCountPtr.setValue(fullCount);

			return 0;
		}

		[HLEFunction(nid : 0x57CF62DD, version : 150)]
		public virtual int sceKernelGetThreadmanIdType(int uid)
		{
			if (SceUidManager.checkUidPurpose(uid, "ThreadMan-thread", false))
			{
				return SCE_KERNEL_TMID_Thread;
			}

			if (SceUidManager.checkUidPurpose(uid, "ThreadMan-sema", false))
			{
				return SCE_KERNEL_TMID_Semaphore;
			}

			if (SceUidManager.checkUidPurpose(uid, "ThreadMan-eventflag", false))
			{
				return SCE_KERNEL_TMID_EventFlag;
			}

			if (SceUidManager.checkUidPurpose(uid, "ThreadMan-Mbx", false))
			{
				return SCE_KERNEL_TMID_Mbox;
			}

			if (SceUidManager.checkUidPurpose(uid, "ThreadMan-Vpl", false))
			{
				return SCE_KERNEL_TMID_Vpl;
			}

			if (SceUidManager.checkUidPurpose(uid, "ThreadMan-Fpl", false))
			{
				return SCE_KERNEL_TMID_Fpl;
			}

			if (SceUidManager.checkUidPurpose(uid, "ThreadMan-MsgPipe", false))
			{
				return SCE_KERNEL_TMID_Mpipe;
			}

			if (SceUidManager.checkUidPurpose(uid, pspBaseCallback.callbackUidPurpose, false))
			{
				return SCE_KERNEL_TMID_Callback;
			}

			if (SceUidManager.checkUidPurpose(uid, "ThreadMan-ThreadEventHandler", false))
			{
				return SCE_KERNEL_TMID_ThreadEventHandler;
			}

			if (SceUidManager.checkUidPurpose(uid, "ThreadMan-Alarm", false))
			{
				return SCE_KERNEL_TMID_Alarm;
			}

			if (SceUidManager.checkUidPurpose(uid, "ThreadMan-VTimer", false))
			{
				return SCE_KERNEL_TMID_VTimer;
			}

			if (SceUidManager.checkUidPurpose(uid, "ThreadMan-Mutex", false))
			{
				return SCE_KERNEL_TMID_Mutex;
			}

			if (SceUidManager.checkUidPurpose(uid, "ThreadMan-LwMutex", false))
			{
				return SCE_KERNEL_TMID_LwMutex;
			}

			return SceKernelErrors.ERROR_KERNEL_ILLEGAL_ARGUMENT;
		}

		[HLEFunction(nid : 0x64D4540E, version : 150)]
		public virtual int sceKernelReferThreadProfiler()
		{
			// Can be safely ignored. Only valid in debug mode on a real PSP.
			return 0;
		}

		[HLEFunction(nid : 0x8218B4DD, version : 150)]
		public virtual int sceKernelReferGlobalProfiler()
		{
			// Can be safely ignored. Only valid in debug mode on a real PSP.
			return 0;
		}

		[HLEFunction(nid : 0x0DDCD2C9, version : 271, checkInsideInterrupt : true)]
		public virtual int sceKernelTryLockMutex(int uid, int count)
		{
			return Managers.mutex.sceKernelTryLockMutex(uid, count);
		}

		[HLEFunction(nid : 0x5BF4DD27, version : 271, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelLockMutexCB(int uid, int count, int timeout_addr)
		{
			return Managers.mutex.sceKernelLockMutexCB(uid, count, timeout_addr);
		}

		[HLEFunction(nid : 0x6B30100F, version : 271, checkInsideInterrupt : true)]
		public virtual int sceKernelUnlockMutex(int uid, int count)
		{
			return Managers.mutex.sceKernelUnlockMutex(uid, count);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x87D9223C, version = 271) public int sceKernelCancelMutex(int uid, int newcount, @CanBeNull pspsharp.HLE.TPointer32 numWaitThreadAddr)
		[HLEFunction(nid : 0x87D9223C, version : 271)]
		public virtual int sceKernelCancelMutex(int uid, int newcount, TPointer32 numWaitThreadAddr)
		{
			return Managers.mutex.sceKernelCancelMutex(uid, newcount, numWaitThreadAddr);
		}

		[HLEFunction(nid : 0xA9C2CB9A, version : 271)]
		public virtual int sceKernelReferMutexStatus(int uid, TPointer addr)
		{
			return Managers.mutex.sceKernelReferMutexStatus(uid, addr);
		}

		[HLEFunction(nid : 0xB011B11F, version : 271, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelLockMutex(int uid, int count, int timeout_addr)
		{
			return Managers.mutex.sceKernelLockMutex(uid, count, timeout_addr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB7D098C6, version = 271, checkInsideInterrupt = true) public int sceKernelCreateMutex(@CanBeNull pspsharp.HLE.PspString name, int attr, int count, int option_addr)
		[HLEFunction(nid : 0xB7D098C6, version : 271, checkInsideInterrupt : true)]
		public virtual int sceKernelCreateMutex(PspString name, int attr, int count, int option_addr)
		{
			return Managers.mutex.sceKernelCreateMutex(name, attr, count, option_addr);
		}

		[HLEFunction(nid : 0xF8170FBE, version : 271, checkInsideInterrupt : true)]
		public virtual int sceKernelDeleteMutex(int uid)
		{
			return Managers.mutex.sceKernelDeleteMutex(uid);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x19CFF145, version = 150, checkInsideInterrupt = true) public int sceKernelCreateLwMutex(pspsharp.HLE.TPointer workAreaAddr, String name, int attr, int count, @CanBeNull pspsharp.HLE.TPointer option)
		[HLEFunction(nid : 0x19CFF145, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelCreateLwMutex(TPointer workAreaAddr, string name, int attr, int count, TPointer option)
		{
			return Managers.lwmutex.sceKernelCreateLwMutex(workAreaAddr, name, attr, count, option);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1AF94D03, version = 380, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int sceKernelDonateWakeupThread()
		[HLEFunction(nid : 0x1AF94D03, version : 380, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int sceKernelDonateWakeupThread()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x31327F19, version = 380, checkInsideInterrupt = true, checkDispatchThreadEnabled = true) public int ThreadManForUser_31327F19(int unkown1, int unknown2, int unknown3)
		[HLEFunction(nid : 0x31327F19, version : 380, checkInsideInterrupt : true, checkDispatchThreadEnabled : true)]
		public virtual int ThreadManForUser_31327F19(int unkown1, int unknown2, int unknown3)
		{
			return 0;
		}

		[HLEFunction(nid : 0x4C145944, version : 380)]
		public virtual int sceKernelReferLwMutexStatusByID(int uid, TPointer addr)
		{
			return Managers.lwmutex.sceKernelReferLwMutexStatusByID(uid, addr);
		}

		[HLEFunction(nid : 0x60107536, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelDeleteLwMutex(TPointer workAreaAddr)
		{
			return Managers.lwmutex.sceKernelDeleteLwMutex(workAreaAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x71040D5C, version = 380) public int ThreadManForUser_71040D5C()
		[HLEFunction(nid : 0x71040D5C, version : 380)]
		public virtual int ThreadManForUser_71040D5C()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7CFF8CF3, version = 380) public int ThreadManForUser_7CFF8CF3()
		[HLEFunction(nid : 0x7CFF8CF3, version : 380)]
		public virtual int ThreadManForUser_7CFF8CF3()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBEED3A47, version = 380) public int ThreadManForUser_BEED3A47()
		[HLEFunction(nid : 0xBEED3A47, version : 380)]
		public virtual int ThreadManForUser_BEED3A47()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xBC80EC7C, version = 620, checkInsideInterrupt = true) public int sceKernelExtendThreadStack(pspsharp.Allegrex.CpuState cpu, @CheckArgument("checkStackSize") int size, pspsharp.HLE.TPointer entryAddr, int entryParameter)
		[HLEFunction(nid : 0xBC80EC7C, version : 620, checkInsideInterrupt : true)]
		public virtual int sceKernelExtendThreadStack(CpuState cpu, int size, TPointer entryAddr, int entryParameter)
		{
			// sceKernelExtendThreadStack executes the code at entryAddr using a larger
			// stack. The entryParameter is  passed as the only parameter ($a0) to
			// the code at entryAddr.
			// When the code at entryAddr returns, sceKernelExtendThreadStack also returns
			// with the return value of entryAddr.
			SceKernelThreadInfo thread = CurrentThread;
			SysMemInfo extendedStackSysMemInfo = thread.extendStack(size);
			if (extendedStackSysMemInfo == null)
			{
				return ERROR_OUT_OF_MEMORY;
			}
			AfterSceKernelExtendThreadStackAction afterAction = new AfterSceKernelExtendThreadStackAction(thread, cpu.pc, cpu._sp, cpu._ra, extendedStackSysMemInfo);
			cpu._a0 = entryParameter;
			cpu._sp = extendedStackSysMemInfo.addr + size;
			callAddress(entryAddr.Address, afterAction, false);

			return afterAction.ReturnValue;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xBC31C1B9, version = 150, checkInsideInterrupt = true) public int sceKernelExtendKernelStack(pspsharp.Allegrex.CpuState cpu, @CheckArgument("checkStackSize") int size, pspsharp.HLE.TPointer entryAddr, int entryParameter)
		[HLEFunction(nid : 0xBC31C1B9, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelExtendKernelStack(CpuState cpu, int size, TPointer entryAddr, int entryParameter)
		{
			// sceKernelExtendKernelStack executes the code at entryAddr using a larger
			// stack. The entryParameter is passed as the only parameter ($a0) to
			// the code at entryAddr.
			// When the code at entryAddr returns, sceKernelExtendKernelStack also returns
			// with the return value of entryAddr.
			SceKernelThreadInfo thread = CurrentThread;
			SysMemInfo extendedStackSysMemInfo = thread.extendStack(size);
			if (extendedStackSysMemInfo == null)
			{
				return ERROR_OUT_OF_MEMORY;
			}
			AfterSceKernelExtendThreadStackAction afterAction = new AfterSceKernelExtendThreadStackAction(thread, cpu.pc, cpu._sp, cpu._ra, extendedStackSysMemInfo);
			cpu._a0 = entryParameter;
			cpu._sp = extendedStackSysMemInfo.addr + size;
			callAddress(entryAddr.Address, afterAction, false);

			return afterAction.ReturnValue;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8DAFF657, version = 620) public int sceKernelCreateTlspl(String name, int partitionId, int attr, int blockSize, int numberBlocks, @CanBeNull pspsharp.HLE.TPointer optionsAddr)
		[HLEFunction(nid : 0x8DAFF657, version : 620)]
		public virtual int sceKernelCreateTlspl(string name, int partitionId, int attr, int blockSize, int numberBlocks, TPointer optionsAddr)
		{
			int alignment = 0;
			if (optionsAddr.NotNull)
			{
				int length = optionsAddr.getValue32(0);
				if (length >= 8)
				{
					alignment = optionsAddr.getValue32(4);
				}
			}

			int alignedBlockSize = alignUp(blockSize, 3);
			if (alignment != 0)
			{
				if ((alignment & (alignment - 1)) != 0)
				{
					return SceKernelErrors.ERROR_KERNEL_ILLEGAL_ARGUMENT;
				}
				alignment = System.Math.Max(alignment, 4);
				alignedBlockSize = alignUp(alignedBlockSize, alignment - 1);
			}

			SceKernelTls tls = new SceKernelTls(name, partitionId, attr, blockSize, alignedBlockSize, numberBlocks, alignment);
			if (tls.BaseAddress == 0)
			{
				return SceKernelErrors.ERROR_OUT_OF_MEMORY;
			}
			tlsMap[tls.uid] = tls;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelCreateTlspl returning 0x{0:X}, baseAddress=0x{1:X8}", tls.uid, tls.BaseAddress));
			}

			return tls.uid;
		}

		[HLEFunction(nid : 0x32BF938E, version : 620)]
		public virtual int sceKernelDeleteTlspl(int uid)
		{
			SceKernelTls tls = tlsMap.Remove(uid);
			if (tls == null)
			{
				return -1;
			}

			tls.free();

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4A719FB2, version = 620) public int sceKernelFreeTlspl()
		[HLEFunction(nid : 0x4A719FB2, version : 620)]
		public virtual int sceKernelFreeTlspl()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x65F54FFB, version = 620) public int _sceKernelAllocateTlspl()
		[HLEFunction(nid : 0x65F54FFB, version : 620)]
		public virtual int _sceKernelAllocateTlspl()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x721067F3, version = 620) public int sceKernelReferTlsplStatus()
		[HLEFunction(nid : 0x721067F3, version : 620)]
		public virtual int sceKernelReferTlsplStatus()
		{
			return 0;
		}

		/*
		 * Suspend all user mode threads in the system.
		 */
		[HLEFunction(nid : 0x8FD9F70C, version : 150)]
		public virtual int sceKernelSuspendAllUserThreads()
		{
			foreach (SceKernelThreadInfo thread in threadMap.Values)
			{
				if (thread != currentThread && thread.UserMode)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceKernelSuspendAllUserThreads suspending {0}", thread));
					}

					if (thread.Waiting)
					{
						hleChangeThreadState(thread, PSP_THREAD_WAITING_SUSPEND);
					}
					else
					{
						hleChangeThreadState(thread, PSP_THREAD_SUSPEND);
					}
				}
			}

			return 0;
		}

		[HLEFunction(nid : 0xF6427665, version : 150)]
		public virtual int sceKernelGetUserLevel()
		{
			return 4;
		}
	}
}