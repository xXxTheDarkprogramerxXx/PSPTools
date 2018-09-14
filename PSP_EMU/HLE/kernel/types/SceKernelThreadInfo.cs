using System.Collections.Generic;
using System.Text;

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
namespace pspsharp.HLE.kernel.types
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._ra;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._zr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_THREAD_ALREADY_DORMANT;


	using Common = pspsharp.Allegrex.Common;
	using CpuState = pspsharp.Allegrex.CpuState;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using SysMemUserForUser = pspsharp.HLE.modules.SysMemUserForUser;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using Callback = pspsharp.HLE.modules.ThreadManForUser.Callback;

	public class SceKernelThreadInfo : pspAbstractMemoryMappedStructureVariableLength, Comparator<SceKernelThreadInfo>
	{

		public const int PSP_MODULE_USER = 0;
		public const int PSP_MODULE_NO_STOP = 0x00000001;
		public const int PSP_MODULE_SINGLE_LOAD = 0x00000002;
		public const int PSP_MODULE_SINGLE_START = 0x00000004;
		public const int PSP_MODULE_POPS = 0x00000200;
		public const int PSP_MODULE_DEMO = 0x00000200; // same as PSP_MODULE_POPS
		public const int PSP_MODULE_GAMESHARING = 0x00000400;
		public const int PSP_MODULE_VSH = 0x00000800; // can only be loaded from kernel mode?
		public const int PSP_MODULE_KERNEL = 0x00001000;
		public const int PSP_MODULE_USE_MEMLMD_LIB = 0x00002000;
		public const int PSP_MODULE_USE_SEMAPHORE_LIB = 0x00004000; // not kernel semaphores, but a fake name (actually security stuff)
		public const int PSP_THREAD_ATTR_USER = unchecked((int)0x80000000); // module attr 0, thread attr: 0x800000FF?
		public const int PSP_THREAD_ATTR_USBWLAN = unchecked((int)0xa0000000);
		public const int PSP_THREAD_ATTR_VSH = unchecked((int)0xc0000000);
		public const int PSP_THREAD_ATTR_KERNEL = 0x00001000; // module attr 0x1000, thread attr: 0?
		public const int PSP_THREAD_ATTR_VFPU = 0x00004000;
		public const int PSP_THREAD_ATTR_SCRATCH_SRAM = 0x00008000;
		public const int PSP_THREAD_ATTR_NO_FILLSTACK = 0x00100000; // Disables filling the stack with 0xFF on creation.
		public const int PSP_THREAD_ATTR_CLEAR_STACK = 0x00200000; // Clear the stack when the thread is deleted.
		public const int PSP_THREAD_ATTR_LOW_MEM_STACK = 0x00400000; // Allocate the stack in low memory instead of high memory
		// PspThreadStatus
		public const int PSP_THREAD_RUNNING = 0x00000001;
		public const int PSP_THREAD_READY = 0x00000002;
		public const int PSP_THREAD_WAITING = 0x00000004;
		public const int PSP_THREAD_SUSPEND = 0x00000008;
		public const int PSP_THREAD_WAITING_SUSPEND = PSP_THREAD_WAITING | PSP_THREAD_SUSPEND;
		public const int PSP_THREAD_STOPPED = 0x00000010;
		public const int PSP_THREAD_KILLED = 0x00000020;
		// Wait types
		public const int PSP_WAIT_NONE = 0x00;
		public const int PSP_WAIT_SLEEP = 0x01; // Wait on sleep thread.
		public const int PSP_WAIT_DELAY = 0x02; // Wait on delay thread.
		public const int PSP_WAIT_SEMA = 0x03; // Wait on sema.
		public const int PSP_WAIT_EVENTFLAG = 0x04; // Wait on event flag.
		public const int PSP_WAIT_MBX = 0x05; // Wait on mbx.
		public const int PSP_WAIT_VPL = 0x06; // Wait on vpl.
		public const int PSP_WAIT_FPL = 0x07; // Wait on fpl.
		public const int PSP_WAIT_MSGPIPE = 0x08; // Wait on msg pipe (send and receive).
		public const int PSP_WAIT_THREAD_END = 0x09; // Wait on thread end.
		public const int PSP_WAIT_EVENTHANDLER = 0x0a; // Wait on event handler release.
		public const int PSP_WAIT_CALLBACK_DELETE = 0x0b; // Wait on callback delete.
		public const int PSP_WAIT_MUTEX = 0x0c; // Wait on mutex.
		public const int PSP_WAIT_LWMUTEX = 0x0d; // Wait on lwmutex.
		// These wait types are only used internally in pspsharp and are not real PSP wait types.
		public const int JPCSP_FIRST_INTERNAL_WAIT_TYPE = 0x100;
		public const int JPCSP_WAIT_IO = JPCSP_FIRST_INTERNAL_WAIT_TYPE; // Wait on IO.
		public static readonly int JPCSP_WAIT_UMD = JPCSP_WAIT_IO + 1; // Wait on UMD.
		public static readonly int JPCSP_WAIT_GE_LIST = JPCSP_WAIT_UMD + 1; // Wait on GE list.
		public static readonly int JPCSP_WAIT_NET = JPCSP_WAIT_GE_LIST + 1; // Wait on Network.
		public static readonly int JPCSP_WAIT_AUDIO = JPCSP_WAIT_NET + 1; // Wait on Audio.
		public static readonly int JPCSP_WAIT_DISPLAY_VBLANK = JPCSP_WAIT_AUDIO + 1; // Wait on Display Vblank.
		public static readonly int JPCSP_WAIT_CTRL = JPCSP_WAIT_DISPLAY_VBLANK + 1; // Wait on Control
		public static readonly int JPCSP_WAIT_USB = JPCSP_WAIT_CTRL + 1; // Wait on USB
		public static readonly int JPCSP_WAIT_VIDEO_DECODER = JPCSP_WAIT_USB + 1; // Wait for sceMpeg video decoder
		// SceKernelThreadInfo.
		public readonly string name;
		public int attr;
		public int status; // it's a bitfield but I don't think we ever use more than 1 bit at once
		public int entry_addr;
		private int stackAddr; // using low address, no need to add stackSize to the pointer returned by malloc
		public int stackSize;
		public int gpReg_addr;
		public readonly int initPriority; // lower numbers mean higher priority
		public int currentPriority;
		public int waitType;
		public int waitId; // the uid of the wait object
		public int wakeupCount; // number of sceKernelWakeupThread() calls pending
		public int exitStatus;
		public TPointer32 exitStatusAddr; // Store the exitStatus at this address if specified
		public long runClocks;
		public int intrPreemptCount;
		public int threadPreemptCount;
		public int releaseCount;
		public int notifyCallback; // Used by sceKernelNotifyCallback to check if a callback has been called or not.
		public int errno; // used by sceNetInet
		private SysMemUserForUser.SysMemInfo stackSysMemInfo;
		// internal variables
		public readonly int uid;
		public int moduleid;
		public CpuState cpuContext;
		public bool doDelete;
		public IAction doDeleteAction;
		public bool unloadModuleAtDeletion;
		public bool doCallbacks;
		public readonly ThreadWaitInfo wait;
		public int displayLastWaitVcount;
		public long javaThreadId = -1;
		public long javaThreadCpuTimeNanos = -1;
		// Callbacks, only 1 of each type can be registered per thread.
		public const int THREAD_CALLBACK_UMD = 0;
		public const int THREAD_CALLBACK_IO = 1;
		public const int THREAD_CALLBACK_MEMORYSTICK = 2;
		public const int THREAD_CALLBACK_MEMORYSTICK_FAT = 3;
		public const int THREAD_CALLBACK_POWER = 4;
		public const int THREAD_CALLBACK_EXIT = 5;
		public const int THREAD_CALLBACK_USB = 6;
		public const int THREAD_CALLBACK_USER_DEFINED = 7;
		public const int THREAD_CALLBACK_SIZE = 8;
		private RegisteredCallbacks[] registeredCallbacks;
		public LinkedList<Callback> pendingCallbacks = new LinkedList<Callback>();
		public LinkedList<IAction> pendingActions = new LinkedList<IAction>();
		// Used by sceKernelExtendThreadStack
		private IList<SysMemUserForUser.SysMemInfo> extendedStackSysMemInfos;
		public bool preserveStack;
		private IAction onThreadStartAction;

		public class RegisteredCallbacks
		{
			internal int type;
			internal IList<pspBaseCallback> callbacks;
			internal IList<pspBaseCallback> readyCallbacks;
			// THREAD_CALLBACK_MEMORYSTICK and THREAD_CALLBACK_MEMORYSTICK_FAT have
			// a maximum of 32 registered callbacks each.
			// Don't know yet for the other types, assuming also 32.
			internal int maxNumberOfCallbacks = 32;
			// Registering a new callback overwrites the previous one.
			internal bool registerOnlyLastCallback = false;

			public RegisteredCallbacks(int type)
			{
				this.type = type;
				callbacks = new LinkedList<pspBaseCallback>();
				readyCallbacks = new LinkedList<pspBaseCallback>();
			}

			public virtual bool hasCallbacks()
			{
				return callbacks.Count > 0;
			}

			public virtual pspBaseCallback getCallbackInfoByUid(int cbid)
			{
				foreach (pspBaseCallback callback in callbacks)
				{
					if (callback.Uid == cbid)
					{
						return callback;
					}
				}

				return null;
			}

			public virtual bool hasCallback(int cbid)
			{
				return getCallbackInfoByUid(cbid) != null;
			}

			public virtual bool hasCallback(pspBaseCallback callback)
			{
				return callbacks.Contains(callback);
			}

			public virtual bool addCallback(pspBaseCallback callback)
			{
				if (hasCallback(callback))
				{
					return true;
				}

				if (NumberOfCallbacks >= maxNumberOfCallbacks)
				{
					return false;
				}

				if (registerOnlyLastCallback)
				{
					callbacks.Clear();
				}

				callbacks.Add(callback);

				return true;
			}

			public virtual pspBaseCallback CallbackReady
			{
				set
				{
					if (hasCallback(value) && !isCallbackReady(value))
					{
						readyCallbacks.Add(value);
					}
				}
			}

			public virtual bool isCallbackReady(pspBaseCallback callback)
			{
				return readyCallbacks.Contains(callback);
			}

			public virtual pspBaseCallback removeCallback(pspBaseCallback callback)
			{
				if (!callbacks.Remove(callback))
				{
					return null;
				}

				readyCallbacks.Remove(callback);

				return callback;
			}

			public virtual pspBaseCallback NextReadyCallback
			{
				get
				{
					if (readyCallbacks.Count == 0)
					{
						return null;
					}
    
					return readyCallbacks.RemoveAt(0);
				}
			}

			public virtual int NumberOfCallbacks
			{
				get
				{
					return callbacks.Count;
				}
			}

			public virtual pspBaseCallback getCallbackByIndex(int index)
			{
				return callbacks[index];
			}

			public virtual void setRegisterOnlyLastCallback()
			{
				registerOnlyLastCallback = true;
			}

			public override string ToString()
			{
				return string.Format("RegisteredCallbacks[type {0:D}, count {1:D}, ready {2:D}]", type, callbacks.Count, readyCallbacks.Count);
			}
		}

		public SceKernelThreadInfo(string name, int entry_addr, int initPriority, int stackSize, int attr, int mpidStack)
		{
			if (stackSize < 512)
			{
				// 512 byte min. (required for interrupts)
				stackSize = 512;
			}
			else
			{
				// 256 byte size alignment.
				stackSize = (stackSize + 0xFF) & ~0xFF;
			}

			if (mpidStack == 0)
			{
				mpidStack = SysMemUserForUser.USER_PARTITION_ID;
			}

			this.name = name;
			this.entry_addr = entry_addr;
			this.initPriority = initPriority;
			this.stackSize = stackSize;
			this.attr = attr;
			uid = SceUidManager.getNewUid("ThreadMan-thread");
			// Setup the stack.
			int stackMemoryType = (attr & PSP_THREAD_ATTR_LOW_MEM_STACK) != 0 ? SysMemUserForUser.PSP_SMEM_Low : SysMemUserForUser.PSP_SMEM_High;
			stackSysMemInfo = Modules.SysMemUserForUserModule.malloc(mpidStack, string.Format("ThreadMan-Stack-0x{0:x}-{1}", uid, name), stackMemoryType, stackSize, 0);
			if (stackSysMemInfo == null)
			{
				stackAddr = 0;
			}
			else
			{
				stackAddr = stackSysMemInfo.addr;
			}

			// Inherit gpReg.
			gpReg_addr = Emulator.Processor.cpu._gp;
			// Inherit context.
			cpuContext = new CpuState(Emulator.Processor.cpu);
			wait = new ThreadWaitInfo();
			reset();
		}

		public virtual void reset()
		{
			status = PSP_THREAD_STOPPED;

			int k0 = stackAddr + stackSize - 0x100; // setup k0
			Memory mem = Memory.Instance;
			if (stackAddr != 0 && stackSize > 0 && !preserveStack)
			{
				// set stack to 0xFF
				if ((attr & PSP_THREAD_ATTR_NO_FILLSTACK) != PSP_THREAD_ATTR_NO_FILLSTACK)
				{
					mem.memset(stackAddr, unchecked((sbyte) 0xFF), stackSize);
				}

				// setup k0
				mem.memset(k0, (sbyte) 0x0, 0x100);
				mem.write32(k0 + 0xc0, stackAddr);
				mem.write32(k0 + 0xca, uid);
				mem.write32(k0 + 0xf8, unchecked((int)0xffffffff));
				mem.write32(k0 + 0xfc, unchecked((int)0xffffffff));

				mem.write32(stackAddr, uid);
			}

			currentPriority = initPriority;
			waitType = PSP_WAIT_NONE;
			waitId = 0;
			wakeupCount = 0;
			exitStatus = ERROR_KERNEL_THREAD_ALREADY_DORMANT; // Threads start with DORMANT and not NOT_DORMANT (tested and checked).
			exitStatusAddr = null;
			runClocks = 0;
			intrPreemptCount = 0;
			threadPreemptCount = 0;
			releaseCount = 0;
			notifyCallback = 0;

			// Thread specific registers
			cpuContext.pc = entry_addr;
			cpuContext.npc = entry_addr; // + 4;

			// Reset all the registers to DEADBEEF value
			for (int i = _ra; i > _zr; i--)
			{
				cpuContext.setRegister(i, unchecked((int)0xDEADBEEF));
			}
			cpuContext._k0 = 0;
			cpuContext._k1 = 0;
			if (UserMode)
			{
				cpuContext._k1 |= 0x100000;
			}
	cpuContext._k1 |= 0x100000;
			int intNanValue = 0x7F800001;
			float nanValue = Float.intBitsToFloat(intNanValue);
			for (int i = Common._f31; i >= Common._f0; i--)
			{
				cpuContext.fpr[i] = nanValue;
			}
			cpuContext.hilo = unchecked((long)0xDEADBEEFDEADBEEFL);

			if ((attr & PSP_THREAD_ATTR_VFPU) != 0)
			{
				// Reset the VFPU context
				for (int m = 0; m < 8; m++)
				{
					for (int c = 0; c < 4; c++)
					{
						for (int r = 0; r < 4; r++)
						{
							cpuContext.setVprInt(m, c, r, intNanValue);
						}
					}
				}
				for (int i = 0; i < cpuContext.vcr.cc.Length; i++)
				{
					cpuContext.vcr.cc[i] = true;
				}
				cpuContext.vcr.pfxs.reset();
				cpuContext.vcr.pfxt.reset();
				cpuContext.vcr.pfxd.reset();
			}

			// sp, 512 byte padding at the top for user data, this will get re-jigged when we call start thread
			cpuContext._sp = stackAddr + stackSize - 512;
			cpuContext._k0 = k0;

			// We'll hook "jr $ra" where $ra == address of HLE syscall hleKernelExitThread
			// when the thread is exiting
			cpuContext._ra = pspsharp.HLE.modules.ThreadManForUser.THREAD_EXIT_HANDLER_ADDRESS;

			doDelete = false;
			doCallbacks = false;

			registeredCallbacks = new RegisteredCallbacks[THREAD_CALLBACK_SIZE];
			for (int i = 0; i < registeredCallbacks.Length; i++)
			{
				registeredCallbacks[i] = new RegisteredCallbacks(i);
			}
			// The UMD callback registers only the last callback.
			registeredCallbacks[THREAD_CALLBACK_UMD].setRegisterOnlyLastCallback();
		}

		public virtual void saveContext()
		{
			cpuContext = Emulator.Processor.cpu;
		}

		public virtual void restoreContext()
		{
			// Assuming context switching only happens on syscall,
			// we always execute npc after a syscall,
			// so we can set pc = npc regardless of cop0.status.bd.
			cpuContext.pc = cpuContext.npc;

			Emulator.Processor.Cpu = cpuContext;
			RuntimeContext.update();
		}

		/// <summary>
		/// For use in the scheduler </summary>
		public virtual int Compare(SceKernelThreadInfo o1, SceKernelThreadInfo o2)
		{
			return o1.currentPriority - o2.currentPriority;
		}

		private int PSPWaitType
		{
			get
			{
				if (waitType >= 0x100)
				{
					// A blocked thread (e.g. a thread blocked due to audio output or
					// wait for vblank or sceCtrl sample reading) is implemented like
					// a "wait for Event Flag". This is the closest implementation to a real PSP,
					// as event flags are usually used by a PSP to implement these wait
					// functions.
					// pspsharp internal wait types are best matched to PSP_WAIT_EVENTFLAG.
					return PSP_WAIT_EVENTFLAG;
				}
				return waitType;
			}
		}

		protected internal override void write()
		{
			base.write();
			writeStringNZ(32, name);
			write32(attr);
			write32(status);
			write32(entry_addr);
			write32(stackAddr);
			write32(stackSize);
			write32(gpReg_addr);
			write32(initPriority);
			write32(currentPriority);
			write32(PSPWaitType);
			write32(waitId);
			write32(wakeupCount);
			write32(exitStatus);
			write64(runClocks);
			write32(intrPreemptCount);
			write32(threadPreemptCount);
			write32(releaseCount);
		}

		// SceKernelThreadRunStatus.
		// Represents a smaller subset of SceKernelThreadInfo containing only the most volatile parts
		// of the thread (mostly used for debugging).
		public virtual void writeRunStatus(TPointer pointer)
		{
			start(pointer.Memory, pointer.Address);
			base.write();
			write32(status);
			write32(currentPriority);
			write32(waitType);
			write32(waitId);
			write32(wakeupCount);
			write64(runClocks);
			write32(intrPreemptCount);
			write32(threadPreemptCount);
			write32(releaseCount);
		}

		public virtual void setSystemStack(int stackAddr, int stackSize)
		{
			freeStack();
			this.stackAddr = stackAddr;
			this.stackSize = stackSize;
		}

		public virtual void freeStack()
		{
			if (stackSysMemInfo != null)
			{
				Modules.SysMemUserForUserModule.free(stackSysMemInfo);
				stackSysMemInfo = null;
				stackAddr = 0;
			}
			freeExtendedStack();
		}

		public virtual void freeExtendedStack(SysMemUserForUser.SysMemInfo extendedStackSysMemInfo)
		{
			if (extendedStackSysMemInfos != null)
			{
				if (extendedStackSysMemInfos.Remove(extendedStackSysMemInfo))
				{
					Modules.SysMemUserForUserModule.free(extendedStackSysMemInfo);
				}

				if (extendedStackSysMemInfos.Count == 0)
				{
					extendedStackSysMemInfos = null;
				}
			}
		}

		public virtual void freeExtendedStack()
		{
			if (extendedStackSysMemInfos != null)
			{
				foreach (SysMemUserForUser.SysMemInfo extendedStackSysMemInfo in extendedStackSysMemInfos)
				{
					Modules.SysMemUserForUserModule.free(extendedStackSysMemInfo);
				}
				extendedStackSysMemInfos = null;
			}
		}

		public virtual SysMemUserForUser.SysMemInfo extendStack(int size)
		{
			SysMemUserForUser.SysMemInfo extendedStackSysMemInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.USER_PARTITION_ID, string.Format("ThreadMan-ExtendedStack-0x{0:x}-{1}", uid, name), SysMemUserForUser.PSP_SMEM_High, size, 0);
			if (extendedStackSysMemInfos == null)
			{
				extendedStackSysMemInfos = new LinkedList<SysMemUserForUser.SysMemInfo>();
			}
			extendedStackSysMemInfos.Add(extendedStackSysMemInfo);

			return extendedStackSysMemInfo;
		}

		public virtual int StackAddr
		{
			get
			{
				if (extendedStackSysMemInfos != null)
				{
					SysMemUserForUser.SysMemInfo extendedStackSysMemInfo = extendedStackSysMemInfos[extendedStackSysMemInfos.Count - 1];
					return extendedStackSysMemInfo.addr;
				}
    
				return stackAddr;
			}
		}

		public static string getStatusName(int status)
		{
			StringBuilder s = new StringBuilder();

			// A thread status is a bitfield so it could be in multiple states
			if ((status & PSP_THREAD_RUNNING) == PSP_THREAD_RUNNING)
			{
				s.Append(" | PSP_THREAD_RUNNING");
			}

			if ((status & PSP_THREAD_READY) == PSP_THREAD_READY)
			{
				s.Append(" | PSP_THREAD_READY");
			}

			if ((status & PSP_THREAD_WAITING) == PSP_THREAD_WAITING)
			{
				s.Append(" | PSP_THREAD_WAITING");
			}

			if ((status & PSP_THREAD_SUSPEND) == PSP_THREAD_SUSPEND)
			{
				s.Append(" | PSP_THREAD_SUSPEND");
			}

			if ((status & PSP_THREAD_STOPPED) == PSP_THREAD_STOPPED)
			{
				s.Append(" | PSP_THREAD_STOPPED");
			}

			if ((status & PSP_THREAD_KILLED) == PSP_THREAD_KILLED)
			{
				s.Append(" | PSP_THREAD_KILLED");
			}

			// Strip off leading " | "
			if (s.Length > 0)
			{
				s.Remove(0, 3);
			}
			else
			{
				s.Append("UNKNOWN");
			}

			return s.ToString();
		}

		public virtual string StatusName
		{
			get
			{
				return getStatusName(status);
			}
		}

		public static string getWaitName(int waitType)
		{
			switch (waitType)
			{
				case PSP_WAIT_NONE:
					return "None";
				case PSP_WAIT_SLEEP:
					return "Sleep";
				case PSP_WAIT_DELAY:
					return "Delay";
				case PSP_WAIT_THREAD_END:
					return "ThreadEnd";
				case PSP_WAIT_EVENTFLAG:
					return "EventFlag";
				case PSP_WAIT_SEMA:
					return "Semaphore";
				case PSP_WAIT_MUTEX:
					return "Mutex";
				case PSP_WAIT_LWMUTEX:
					return "LwMutex";
				case PSP_WAIT_MBX:
					return "Mbx";
				case PSP_WAIT_VPL:
					return "Vpl";
				case PSP_WAIT_FPL:
					return "Fpl";
				case PSP_WAIT_MSGPIPE:
					return "MsgPipe";
				case PSP_WAIT_EVENTHANDLER:
					return "EventHandler";
				case PSP_WAIT_CALLBACK_DELETE:
					return "CallBackDelete";
				case JPCSP_WAIT_IO:
					return "Io";
				case JPCSP_WAIT_UMD:
					return "Umd";
				case JPCSP_WAIT_GE_LIST:
					return "Ge List";
				case JPCSP_WAIT_NET:
					return "Network";
				case JPCSP_WAIT_AUDIO:
					return "Audio";
				case JPCSP_WAIT_DISPLAY_VBLANK:
					return "Display Vblank";
				case JPCSP_WAIT_CTRL:
					return "Ctrl";
				case JPCSP_WAIT_USB:
					return "Usb";
				case JPCSP_WAIT_VIDEO_DECODER:
					return "VideoDecoder";
			}
			return string.Format("Unknown waitType={0:D}", waitType);
		}

		public static string getWaitName(int waitType, int waitId, ThreadWaitInfo wait, int status)
		{
			StringBuilder s = new StringBuilder();

			switch (waitType)
			{
				case PSP_WAIT_NONE:
					s.Append(string.Format("None"));
					break;
				case PSP_WAIT_SLEEP:
					s.Append(string.Format("Sleep"));
					break;
				case PSP_WAIT_DELAY:
					s.Append(string.Format("Delay"));
					break;
				case PSP_WAIT_THREAD_END:
					s.Append(string.Format("ThreadEnd (0x{0:X4})", wait.ThreadEnd_id));
					break;
				case PSP_WAIT_EVENTFLAG:
					s.Append(string.Format("EventFlag (0x{0:X4})", wait.EventFlag_id));
					break;
				case PSP_WAIT_SEMA:
					s.Append(string.Format("Semaphore (0x{0:X4})", wait.Semaphore_id));
					break;
				case PSP_WAIT_MUTEX:
					s.Append(string.Format("Mutex (0x{0:X4})", wait.Mutex_id));
					break;
				case PSP_WAIT_LWMUTEX:
					s.Append(string.Format("LwMutex (0x{0:X4})", wait.LwMutex_id));
					break;
				case PSP_WAIT_MBX:
					s.Append(string.Format("Mbx (0x{0:X4})", wait.Mbx_id));
					break;
				case PSP_WAIT_VPL:
					s.Append(string.Format("Vpl (0x{0:X4})", wait.Vpl_id));
					break;
				case PSP_WAIT_FPL:
					s.Append(string.Format("Fpl (0x{0:X4})", wait.Fpl_id));
					break;
				case PSP_WAIT_MSGPIPE:
					s.Append(string.Format("MsgPipe (0x{0:X4})", wait.MsgPipe_id));
					break;
				case PSP_WAIT_EVENTHANDLER:
					s.Append(string.Format("EventHandler"));
					break;
				case PSP_WAIT_CALLBACK_DELETE:
					s.Append(string.Format("CallBackDelete"));
					break;
				case JPCSP_WAIT_IO:
					s.Append(string.Format("Io (0x{0:X4})", wait.Io_id));
					break;
				case JPCSP_WAIT_UMD:
					s.Append(string.Format("Umd (0x{0:X2})", wait.wantedUmdStat));
					break;
				case JPCSP_WAIT_GE_LIST:
					s.Append(string.Format("Ge List ({0})", Modules.sceGe_userModule.getGeList(waitId)));
					break;
				case JPCSP_WAIT_NET:
					s.Append(string.Format("Network"));
					break;
				case JPCSP_WAIT_AUDIO:
					s.Append(string.Format("Audio"));
					break;
				case JPCSP_WAIT_DISPLAY_VBLANK:
					s.Append(string.Format("Display Vblank (vcount={0:D}, current={1:D})", waitId, Modules.sceDisplayModule.Vcount));
					break;
				case JPCSP_WAIT_CTRL:
					s.Append(string.Format("Ctrl"));
					break;
				case JPCSP_WAIT_USB:
					s.Append(string.Format("Usb"));
					break;
				case JPCSP_WAIT_VIDEO_DECODER:
					s.Append(string.Format("VideoDecoder"));
					break;
				default:
					s.Append(string.Format("Unknown waitType={0:D}", waitType));
					break;
			}

			if ((status & PSP_THREAD_WAITING) != 0)
			{
				if (wait.forever)
				{
					s.Append(" (forever)");
				}
				else
				{
					int restDelay = (int)(wait.microTimeTimeout - Emulator.Clock.microTime());
					if (restDelay < 0)
					{
						restDelay = 0;
					}
					s.Append(string.Format(" (delay {0:D} us, rest {1:D} us)", wait.micros, restDelay));
				}
			}

			return s.ToString();
		}

		public virtual string WaitName
		{
			get
			{
				return getWaitName(waitType, waitId, wait, status);
			}
		}

		public virtual bool Suspended
		{
			get
			{
				return (status & PSP_THREAD_SUSPEND) != 0;
			}
		}

		public virtual bool Waiting
		{
			get
			{
				return isWaitingStatus(status);
			}
		}

		public static bool isWaitingStatus(int status)
		{
			return (status & PSP_THREAD_WAITING) != 0;
		}

		public virtual bool isWaitingForType(int waitType)
		{
			// Check if the thread is still in a WAITING state, but not WAITING_SUSPEND
			if (!Waiting || Suspended)
			{
				return false;
			}
			return this.waitType == waitType;
		}

		public virtual bool isWaitingFor(int waitType, int waitId)
		{
			if (!isWaitingForType(waitType))
			{
				return false;
			}

			return this.waitId == waitId;
		}

		public virtual bool Running
		{
			get
			{
				return (status & PSP_THREAD_RUNNING) != 0;
			}
		}

		public virtual bool Ready
		{
			get
			{
				return (status & PSP_THREAD_READY) != 0;
			}
		}

		public virtual bool Stopped
		{
			get
			{
				return (status & PSP_THREAD_STOPPED) != 0;
			}
		}

		public static bool isKernelMode(int attr)
		{
			return (attr & PSP_THREAD_ATTR_KERNEL) != 0;
		}

		public static bool isUserMode(int attr)
		{
			return (attr & PSP_THREAD_ATTR_USER) != 0;
		}

		public virtual bool KernelMode
		{
			get
			{
				return isKernelMode(attr);
			}
		}

		public virtual bool UserMode
		{
			get
			{
				return isUserMode(attr);
			}
		}

		public virtual RegisteredCallbacks getRegisteredCallbacks(int type)
		{
			return registeredCallbacks[type];
		}

		public virtual bool deleteCallback(SceKernelCallbackInfo callback)
		{
			bool deleted = false;
			for (int i = 0; i < registeredCallbacks.Length; i++)
			{
				if (registeredCallbacks[i].removeCallback(callback) != null)
				{
					deleted = true;
				}
			}

			return deleted;
		}

		public virtual int ExitStatus
		{
			set
			{
				this.exitStatus = value;
				if (exitStatusAddr != null)
				{
					exitStatusAddr.setValue(value);
				}
			}
		}

		public virtual bool isStackAddress(int address)
		{
			if (stackAddr == 0 || stackSize <= 0)
			{
				return false;
			}

			return address >= stackAddr && address < (stackAddr + stackSize);
		}

		public virtual IAction OnThreadStartAction
		{
			set
			{
				this.onThreadStartAction = value;
			}
		}

		public virtual void onThreadStart()
		{
			if (onThreadStartAction != null)
			{
				onThreadStartAction.execute();
			}
		}

		public override string ToString()
		{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("%s(uid=0x%X, Status=%s, Priority=0x%X, Wait=%s, doCallbacks=%b)", name, uid, getStatusName(), currentPriority, getWaitName(), doCallbacks);
			return string.Format("%s(uid=0x%X, Status=%s, Priority=0x%X, Wait=%s, doCallbacks=%b)", name, uid, StatusName, currentPriority, WaitName, doCallbacks);
		}
	}
}