using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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
namespace pspsharp.Allegrex.compiler
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Memory.addressMask;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.addAddressHex;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.addHex;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.sleep;


	using Instruction = pspsharp.Allegrex.Common.Instruction;
	using Modules = pspsharp.HLE.Modules;
	using PspString = pspsharp.HLE.PspString;
	using SyscallHandler = pspsharp.HLE.SyscallHandler;
	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;
	using reboot = pspsharp.HLE.modules.reboot;
	using sceDisplay = pspsharp.HLE.modules.sceDisplay;
	using MEProcessor = pspsharp.mediaengine.MEProcessor;
	using DebuggerMemory = pspsharp.memory.DebuggerMemory;
	using FastMemory = pspsharp.memory.FastMemory;
	using MMIOHandlerDisplayController = pspsharp.memory.mmio.MMIOHandlerDisplayController;
	using Scheduler = pspsharp.scheduler.Scheduler;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using Settings = pspsharp.settings.Settings;
	using CpuDurationStatistics = pspsharp.util.CpuDurationStatistics;
	using DurationStatistics = pspsharp.util.DurationStatistics;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;
	using MDC = org.apache.log4j.MDC;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class RuntimeContext
	{
		public static Logger log = Logger.getLogger("runtime");
		private static bool compilerEnabled = true;
		public static float[] fpr;
		public static float[] vprFloat;
		public static int[] vprInt;
		public static int[] memoryInt;
		public static Processor processor;
		public static CpuState cpu;
		public static Memory memory;
		public static bool enableDebugger = true;
		public const string debuggerName = "syncDebugger";
		public static bool debugCodeBlockCalls = false;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		public const string debugCodeBlockStart_Renamed = "debugCodeBlockStart";
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		public const string debugCodeBlockEnd_Renamed = "debugCodeBlockEnd";
		private const int debugCodeBlockNumberOfParameters = 6;
		private static readonly IDictionary<int, int> debugCodeBlocks = new Dictionary<int, int>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		public const bool debugCodeInstruction_Renamed = false;
		public const string debugCodeInstructionName = "debugCodeInstruction";
		public const bool debugMemoryRead = false;
		public const bool debugMemoryWrite = false;
		public const bool debugMemoryReadWriteNoSP = true;
		public const bool enableInstructionTypeCounting = false;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		public const string instructionTypeCount_Renamed = "instructionTypeCount";
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		public const string logInfo_Renamed = "logInfo";
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		public const string pauseEmuWithStatus_Renamed = "pauseEmuWithStatus";
		public const bool enableLineNumbers = true;
		public const bool checkCodeModification = false;
		private const bool invalidateAllCodeBlocks = false;
		private const int idleSleepMicros = 1000;
		private static readonly IDictionary<int, CodeBlock> codeBlocks = Collections.synchronizedMap(new Dictionary<int, CodeBlock>());
		private static int codeBlocksLowestAddress = int.MaxValue;
		private static int codeBlocksHighestAddress = int.MinValue;
		// A fast lookup array for executables (to improve the performance of the Allegrex instruction jalr)
		private static IExecutable[] fastExecutableLookup;
		// A fast lookup for the Allegrex instruction ICACHE HIT INVALIDATE
		private static CodeBlockList[] fastCodeBlockLookup;
		private const int fastCodeBlockLookupShift = 8;
		private const int fastCodeBlockSize = 64; // Matching the size used by the Allegrex instruction ICACHE HIT INVALIDATE
		private static readonly IDictionary<SceKernelThreadInfo, RuntimeThread> threads = Collections.synchronizedMap(new Dictionary<SceKernelThreadInfo, RuntimeThread>());
		private static readonly IDictionary<SceKernelThreadInfo, RuntimeThread> toBeStoppedThreads = Collections.synchronizedMap(new Dictionary<SceKernelThreadInfo, RuntimeThread>());
		private static readonly IDictionary<SceKernelThreadInfo, RuntimeThread> alreadyStoppedThreads = Collections.synchronizedMap(new Dictionary<SceKernelThreadInfo, RuntimeThread>());
		private static readonly IList<Thread> alreadySwitchedStoppedThreads = Collections.synchronizedList(new List<Thread>());
		private static readonly IDictionary<SceKernelThreadInfo, RuntimeThread> toBeDeletedThreads = Collections.synchronizedMap(new Dictionary<SceKernelThreadInfo, RuntimeThread>());
		public static volatile SceKernelThreadInfo currentThread = null;
		private static volatile RuntimeThread currentRuntimeThread = null;
		private static readonly object waitForEnd = new object();
		private static volatile Emulator emulator;
		private static volatile bool isIdle = false;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private static volatile bool reset_Renamed = false;
		public static CpuDurationStatistics idleDuration = new CpuDurationStatistics("Idle Time");
		private static IDictionary<Common.Instruction, int> instructionTypeCounts = Collections.synchronizedMap(new Dictionary<Common.Instruction, int>());
		public const bool enableDaemonThreadSync = true;
		public const string syncName = "sync";
		public static volatile bool wantSync = false;
		private static RuntimeSyncThread runtimeSyncThread = null;
		private static RuntimeThread syscallRuntimeThread;
		private static sceDisplay sceDisplayModule;
		private static readonly object idleSyncObject = new object();
		public static int firmwareVersion;
		private static bool isHomebrew = false;

		private class CompilerEnabledSettingsListerner : AbstractBoolSettingsListener
		{
			protected internal override void settingsValueChanged(bool value)
			{
				CompilerEnabled = value;
			}
		}

		private class CodeBlockList : LinkedList<CodeBlock>
		{
			internal const long serialVersionUID = 7370950118403866860L;
		}

		private static bool CompilerEnabled
		{
			set
			{
				compilerEnabled = value;
			}
			get
			{
				return compilerEnabled;
			}
		}


		public static void execute(Common.Instruction insn, int opcode)
		{
			insn.interpret(processor, opcode);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static int jumpCall(int address) throws Exception
		private static int jumpCall(int address)
		{
			IExecutable executable = getExecutable(address);
			if (executable == null)
			{
				// TODO Return to interpreter
				log.error("jumpCall - Cannot find executable");
				throw new Exception("Cannot find executable");
			}

			int returnValue;
			int sp = cpu._sp;
			RuntimeThread stackThread = currentRuntimeThread;

			if (stackThread != null && stackThread.StackMaxSize)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("jumpCall stack already reached maxSize, returning 0x{0:X8}", address));
				}
				throw new StackPopException(address);
			}

			try
			{
				if (stackThread != null)
				{
					stackThread.increaseStackSize();
				}
				returnValue = executable.exec();
			}
			catch (StackOverflowError e)
			{
				log.error(string.Format("StackOverflowError stackSize={0:D}", stackThread.StackSize));
				throw e;
			}
			finally
			{
				if (stackThread != null)
				{
					stackThread.decreaseStackSize();
				}
			}

			if (stackThread != null && stackThread.StackMaxSize && cpu._sp > sp)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("jumpCall returning 0x{0:X8} with $sp=0x{1:X8}, start $sp=0x{2:X8}", returnValue, cpu._sp, sp));
				}
				throw new StackPopException(returnValue);
			}

			if (debugCodeBlockCalls && log.DebugEnabled)
			{
				log.debug(string.Format("jumpCall returning 0x{0:X8}", returnValue));
			}

			return returnValue;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void jump(int address, int returnAddress) throws Exception
		public static void jump(int address, int returnAddress)
		{
			if (debugCodeBlockCalls && log.DebugEnabled)
			{
				log.debug(string.Format("jump starting address=0x{0:X8}, returnAddress=0x{1:X8}, $sp=0x{2:X8}", address, returnAddress, cpu._sp));
			}

			int sp = cpu._sp;
			while ((address & addressMask) != (returnAddress & addressMask))
			{
				try
				{
					address = jumpCall(address);
				}
				catch (StackPopException e)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("jump catching StackPopException 0x{0:X8} with $sp=0x{1:X8}, start $sp=0x{2:X8}", e.Ra, cpu._sp, sp));
					}
					if ((e.Ra & addressMask) != (returnAddress & addressMask))
					{
						throw e;
					}
					break;
				}
			}

			if (debugCodeBlockCalls && log.DebugEnabled)
			{
				log.debug(string.Format("jump returning address=0x{0:X8}, returnAddress=0x{1:X8}, $sp=0x{2:X8}", address, returnAddress, cpu._sp));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int call(int address) throws Exception
		public static int call(int address)
		{
			if (debugCodeBlockCalls && log.DebugEnabled)
			{
				log.debug(string.Format("call address=0x{0:X8}, $ra=0x{1:X8}", address, cpu._ra));
			}
			int returnValue = jumpCall(address);

			return returnValue;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int executeInterpreter(int address) throws Exception
		public static int executeInterpreter(int address)
		{
			if (debugCodeBlockCalls && log.DebugEnabled)
			{
				log.debug(string.Format("executeInterpreter address=0x{0:X8}", address));
			}

			bool useMMIO = false;
			if (!Memory.isAddressGood(address))
			{
				Memory mmio = RuntimeContextLLE.MMIO;
				if (mmio != null)
				{
					useMMIO = true;
					cpu.Memory = mmio;
				}
			}

			bool interpret = true;
			cpu.pc = address;
			int returnValue = 0;
			while (interpret)
			{
				Common.Instruction insn = processor.interpret();
				if (insn.hasFlags(Common.Instruction.FLAG_STARTS_NEW_BLOCK))
				{
					if (useMMIO)
					{
						cpu.Memory = memory;
					}
					cpu.pc = jumpCall(cpu.pc);
					if (useMMIO)
					{
						cpu.Memory = RuntimeContextLLE.MMIO;
					}
				}
				else if (insn.hasOneFlag(Common.Instruction.FLAG_ENDS_BLOCK | Common.Instruction.FLAG_TRIGGERS_EXCEPTION) && !insn.hasFlags(Common.Instruction.FLAG_IS_CONDITIONAL))
				{
					interpret = false;
					returnValue = cpu.pc;
				}
			}

			if (useMMIO)
			{
				cpu.Memory = memory;
			}

			return returnValue;
		}

		public static void execute(int opcode)
		{
			Common.Instruction insn = Decoder.instruction(opcode);
			execute(insn, opcode);
		}

		private static string getDebugCodeBlockStart(CpuState cpu, int address)
		{
			// Do not build the string using "String.format()" for improved performance of this time-critical function
			StringBuilder s = new StringBuilder("Starting CodeBlock 0x");
			addAddressHex(s, address);

			int syscallAddress = address + 4;
			if (Memory.isAddressGood(syscallAddress))
			{
				int syscallOpcode = cpu.memory.read32(syscallAddress);
				Common.Instruction syscallInstruction = Decoder.instruction(syscallOpcode);
				if (syscallInstruction == Instructions.SYSCALL)
				{
					string syscallDisasm = syscallInstruction.disasm(syscallAddress, syscallOpcode);
					s.Append(syscallDisasm.Substring(19));
				}
			}

			int numberOfParameters = debugCodeBlockNumberOfParameters;
			if (debugCodeBlocks.Count > 0)
			{
				int? numberOfParametersValue = debugCodeBlocks[address];
				if (numberOfParametersValue != null)
				{
					numberOfParameters = numberOfParametersValue.Value;
				}
			}

			if (numberOfParameters > 0)
			{
				int maxRegisterParameters = System.Math.Min(numberOfParameters, 8);
				for (int i = 0; i < maxRegisterParameters; i++)
				{
					int register = Common._a0 + i;
					int parameterValue = cpu.getRegister(register);

					s.Append(", ");
					s.Append(Common.gprNames[register]);
					s.Append("=0x");
					if (Memory.isAddressGood(parameterValue))
					{
						addAddressHex(s, parameterValue);
					}
					else
					{
						addHex(s, parameterValue);
					}
				}
			}

			s.Append(", $ra=0x");
			addAddressHex(s, cpu._ra);
			s.Append(", $sp=0x");
			addAddressHex(s, cpu._sp);

			return s.ToString();
		}

		public static void debugCodeBlockStart(int address)
		{
			if (debugCodeBlocks.Count > 0 && debugCodeBlocks.ContainsKey(address))
			{
				if (log.InfoEnabled)
				{
					log.info(getDebugCodeBlockStart(cpu, address));
				}
			}
			else if (log.DebugEnabled)
			{
				log.debug(getDebugCodeBlockStart(cpu, address));
			}
		}

		public static void debugCodeBlockStart(CpuState cpu, int address)
		{
			if (debugCodeBlocks.Count > 0 && debugCodeBlocks.ContainsKey(address))
			{
				if (log.InfoEnabled)
				{
					log.info(getDebugCodeBlockStart(cpu, address));
				}
			}
			else if (log.DebugEnabled)
			{
				log.debug(getDebugCodeBlockStart(cpu, address));
			}
		}

		public static void debugCodeBlockEnd(int address, int returnAddress)
		{
			if (log.DebugEnabled)
			{
				debugCodeBlockEnd(cpu, address, returnAddress);
			}
		}

		public static void debugCodeBlockEnd(CpuState cpu, int address, int returnAddress)
		{
			if (log.DebugEnabled)
			{
				// Do not build the string using "String.format()" for improved performance of this time-critical function
				StringBuilder s = new StringBuilder("Returning from CodeBlock 0x");
				addAddressHex(s, address);
				s.Append(" to 0x");
				addAddressHex(s, returnAddress);
				s.Append(", $sp=0x");
				addAddressHex(s, cpu._sp);
				s.Append(", $v0=0x");
				addAddressHex(s, cpu._v0);
				log.debug(s.ToString());
			}
		}

		public static void debugCodeInstruction(int address, int opcode)
		{
			if (log.TraceEnabled)
			{
				cpu.pc = address;
				Common.Instruction insn = Decoder.instruction(opcode);

				// Do not build the string using "String.format()" for improved performance of this time-critical function
				StringBuilder s = new StringBuilder("Executing 0x");
				addAddressHex(s, address);
				s.Append(insn.hasFlags(Common.Instruction.FLAG_INTERPRETED) ? " I - " : " C - ");
				s.Append(insn.disasm(address, opcode));
				log.trace(s.ToString());
			}
		}

		private static bool initialise()
		{
			if (!compilerEnabled)
			{
				return false;
			}

			if (enableDaemonThreadSync && runtimeSyncThread == null)
			{
				runtimeSyncThread = new RuntimeSyncThread();
				runtimeSyncThread.Name = "Sync Daemon";
				runtimeSyncThread.Daemon = true;
				runtimeSyncThread.Start();
			}

			updateMemory();

			if (State.debugger != null || (memory is DebuggerMemory) || debugMemoryRead || debugMemoryWrite)
			{
				enableDebugger = true;
			}
			else
			{
				enableDebugger = false;
			}

			Profiler.initialise();

			sceDisplayModule = Modules.sceDisplayModule;

			fastExecutableLookup = new IExecutable[MemoryMap.SIZE_RAM >> 2];
			fastCodeBlockLookup = new CodeBlockList[MemoryMap.SIZE_RAM >> fastCodeBlockLookupShift];

			return true;
		}

		public static bool canExecuteCallback(SceKernelThreadInfo callbackThread)
		{
			if (!compilerEnabled)
			{
				return true;
			}

			// Can the callback be executed in any thread (e.g. is an interrupt)?
			if (callbackThread == null)
			{
				return true;
			}

			if (Modules.ThreadManForUserModule.isIdleThread(callbackThread))
			{
				return true;
			}

			Thread currentThread = Thread.CurrentThread;
			if (currentThread is RuntimeThread)
			{
				RuntimeThread currentRuntimeThread = (RuntimeThread) currentThread;
				if (callbackThread == currentRuntimeThread.ThreadInfo)
				{
					return true;
				}
			}

			return false;
		}

		private static void checkPendingCallbacks()
		{
			if (Modules.ThreadManForUserModule.checkPendingActions())
			{
				// if some action has been executed, the current thread might be changed. Resync.
				if (log.DebugEnabled)
				{
					log.debug(string.Format("A pending action has been executed for the thread"));
				}
				wantSync = true;
			}

			Modules.ThreadManForUserModule.checkPendingCallbacks();
		}

		public static void executeCallback()
		{
			int pc = cpu.pc;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Start of Callback 0x{0:X8}", pc));
			}

			// Switch to the real active thread, even if it is an idle thread
			switchRealThread(Modules.ThreadManForUserModule.CurrentThread);

			bool callbackExited = executeFunction(pc);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("End of Callback 0x{0:X8}", pc));
			}

			if (cpu.pc == ThreadManForUser.CALLBACK_EXIT_HANDLER_ADDRESS || callbackExited)
			{
				Modules.ThreadManForUserModule.hleKernelExitCallback(Emulator.Processor);

				// Re-sync the runtime, the current thread might have been rescheduled
				wantSync = true;
			}

			update();
		}

		private static void updateStaticVariables()
		{
			emulator = Emulator.Instance;
			processor = Emulator.Processor;
			cpu = processor.cpu;
			if (cpu != null)
			{
				fpr = processor.cpu.fpr;
				vprFloat = processor.cpu.vprFloat;
				vprInt = processor.cpu.vprInt;
			}
		}

		public static void updateMemory()
		{
			memory = Emulator.Memory;
			if (memory is FastMemory)
			{
				memoryInt = ((FastMemory) memory).All;
			}
			else
			{
				memoryInt = null;
			}
		}

		public static void update()
		{
			if (!compilerEnabled)
			{
				return;
			}

			updateStaticVariables();

			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			if (IntrManager.Instance.canExecuteInterruptNow())
			{
				SceKernelThreadInfo newThread = threadMan.CurrentThread;
				if (newThread != null && newThread != currentThread)
				{
					switchThread(newThread);
				}
			}
		}

		private static void switchRealThread(SceKernelThreadInfo threadInfo)
		{
			RuntimeThread thread = threads[threadInfo];
			if (thread == null)
			{
				thread = new RuntimeThread(threadInfo);
				threads[threadInfo] = thread;
				thread.Start();
			}

			currentThread = threadInfo;
			currentRuntimeThread = thread;
			isIdle = false;
		}

		private static void switchThread(SceKernelThreadInfo threadInfo)
		{
			if (log.DebugEnabled)
			{
				string name;
				if (threadInfo == null)
				{
					name = "Idle";
				}
				else
				{
					name = threadInfo.name;
				}

				if (currentThread == null)
				{
					log.debug("Switching to Thread " + name);
				}
				else
				{
					log.debug("Switching from Thread " + currentThread.name + " to " + name);
				}
			}

			if (threadInfo == null || Modules.ThreadManForUserModule.isIdleThread(threadInfo))
			{
				isIdle = true;
				currentThread = null;
				currentRuntimeThread = null;
			}
			else if (toBeStoppedThreads.ContainsKey(threadInfo))
			{
				// This thread must stop immediately
				isIdle = true;
				currentThread = null;
				currentRuntimeThread = null;
			}
			else
			{
				switchRealThread(threadInfo);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void syncIdle() throws StopThreadException
		private static void syncIdle()
		{
			if (isIdle)
			{
				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				Scheduler scheduler = Emulator.Scheduler;

				log.debug("Starting Idle State...");
				idleDuration.start();
				while (isIdle)
				{
					checkStoppedThread();
					{
						// Do not take the duration of sceDisplay into idleDuration
						idleDuration.end();
						syncEmulator(true);
						idleDuration.start();
					}
					syncPause();
					checkPendingCallbacks();
					scheduler.step();
					if (threadMan.isIdleThread(threadMan.CurrentThread))
					{
						threadMan.checkCallbacks();
						threadMan.hleRescheduleCurrentThread();
					}

					if (isIdle)
					{
						// While being idle, try to reduce the load on the host CPU
						// by sleeping as much as possible.
						// We can now sleep until the next scheduler action need to be executed.
						//
						// If the scheduler is receiving from another thread, a new action
						// to be executed earlier, the wait state of this thread
						// will be interrupted (see onNextScheduleModified()).
						// This is for example the case when a GE list is ending (FINISH/SIGNAL + END)
						// and a GE callback has to be executed immediately.
						long delay = scheduler.getNextActionDelay(idleSleepMicros);
						if (delay > 0)
						{
							int intDelay;
							if (delay >= idleSleepMicros)
							{
								intDelay = idleSleepMicros;
							}
							else
							{
								intDelay = (int) delay;
							}

							try
							{
								// Wait for intDelay milliseconds.
								// The wait state will be terminated whenever the scheduler
								// is receiving a new scheduler action (see onNextScheduleModified()).
								lock (idleSyncObject)
								{
									Monitor.Wait(idleSyncObject, TimeSpan.FromMilliseconds((intDelay / 1000) + (intDelay % 1000) / 1000d));
								}
							}
							catch (InterruptedException)
							{
								// Ignore exception
							}
						}
					}
				}
				idleDuration.end();
				log.debug("Ending Idle State");
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void syncThreadImmediately() throws StopThreadException
		private static void syncThreadImmediately()
		{
			Thread currentThread = Thread.CurrentThread;
			if (currentRuntimeThread != null && currentThread != currentRuntimeThread && !alreadySwitchedStoppedThreads.Contains(currentThread))
			{
				currentRuntimeThread.continueRuntimeExecution();

				if (currentThread is RuntimeThread)
				{
					RuntimeThread runtimeThread = (RuntimeThread) currentThread;
					if (!alreadyStoppedThreads.ContainsValue(runtimeThread))
					{
						log.debug("Waiting to be scheduled...");
						runtimeThread.suspendRuntimeExecution();
						log.debug("Scheduled, restarting...");
						checkStoppedThread();

						updateStaticVariables();
					}
					else
					{
						alreadySwitchedStoppedThreads.Add(currentThread);
					}
				}
			}

			checkPendingCallbacks();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void syncThread() throws StopThreadException
		private static void syncThread()
		{
			syncIdle();

			if (toBeDeletedThreads.ContainsValue(RuntimeThread))
			{
				return;
			}

			Thread currentThread = Thread.CurrentThread;
			if (log.DebugEnabled)
			{
				log.debug("syncThread currentThread=" + currentThread.Name + ", currentRuntimeThread=" + currentRuntimeThread.Name);
			}
			syncThreadImmediately();
		}

		private static RuntimeThread RuntimeThread
		{
			get
			{
				Thread currentThread = Thread.CurrentThread;
				if (currentThread is RuntimeThread)
				{
					return (RuntimeThread) currentThread;
				}
    
				return null;
			}
		}

		private static bool StoppedThread
		{
			get
			{
				if (toBeStoppedThreads.Count == 0)
				{
					return false;
				}
    
				RuntimeThread runtimeThread = RuntimeThread;
				if (runtimeThread != null && toBeStoppedThreads.ContainsValue(runtimeThread))
				{
					if (!alreadyStoppedThreads.ContainsValue(runtimeThread))
					{
						return true;
					}
				}
    
				return false;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void checkStoppedThread() throws StopThreadException
		private static void checkStoppedThread()
		{
			if (StoppedThread)
			{
				throw new StopThreadException("Stopping Thread " + Thread.CurrentThread.Name);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void syncPause() throws StopThreadException
		private static void syncPause()
		{
			if (Emulator.pause)
			{
				Emulator.Clock.pause();
				try
				{
					lock (emulator)
					{
					   while (Emulator.pause)
					   {
						   checkStoppedThread();
						   Monitor.Wait(emulator);
					   }
					}
				}
				catch (InterruptedException)
				{
					// Ignore Exception
				}
				finally
				{
					Emulator.Clock.resume();
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void syncDebugger(int pc) throws StopThreadException
		public static void syncDebugger(int pc)
		{
			processor.cpu.pc = pc;
			if (State.debugger != null)
			{
				syncDebugger();
				syncPause();
			}
			else if (Emulator.pause)
			{
				syncPause();
			}
		}

		private static void syncDebugger()
		{
			if (State.debugger != null)
			{
				State.debugger.step();
			}
		}

		private static void syncEmulator(bool immediately)
		{
			if (log.DebugEnabled)
			{
				log.debug("syncEmulator immediately=" + immediately);
			}

			Modules.sceGe_userModule.step();
			Modules.sceDisplayModule.step(immediately);
		}

		private static void syncFast()
		{
			// Always sync the display to trigger the GE list processing
			Modules.sceDisplayModule.step();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void sync() throws StopThreadException
		public static void sync()
		{
			do
			{
				wantSync = false;

				if (!IntrManager.Instance.canExecuteInterruptNow() && !RuntimeContextLLE.LLEActive)
				{
					syncFast();
				}
				else
				{
					syncPause();
					Emulator.Scheduler.step();
					if (processor.InterruptsEnabled)
					{
						Modules.ThreadManForUserModule.hleRescheduleCurrentThread();
					}
					syncThread();
					syncEmulator(false);
					syncDebugger();
					syncPause();
					checkStoppedThread();
				}
			// Check if a new sync request has been received in the meantime
			} while (wantSync);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void preSyscall() throws StopThreadException
		public static void preSyscall()
		{
			if (IntrManager.Instance.canExecuteInterruptNow())
			{
				syscallRuntimeThread = RuntimeThread;
				if (syscallRuntimeThread != null)
				{
					syscallRuntimeThread.InSyscall = true;
				}
				checkStoppedThread();
				syncPause();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void postSyscall() throws StopThreadException
		public static void postSyscall()
		{
			if (!IntrManager.Instance.canExecuteInterruptNow())
			{
				postSyscallFast();
			}
			else
			{
				checkStoppedThread();
				sync();
				if (syscallRuntimeThread != null)
				{
					syscallRuntimeThread.InSyscall = false;
				}
			}
		}

		public static void postSyscallFast()
		{
			syncFast();
		}

		public static void postSyscallLLE()
		{
			Modules.sceDisplayModule.step();
			checkSync();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int syscallFast(int code, boolean inDelaySlot) throws Exception
		public static int syscallFast(int code, bool inDelaySlot)
		{
			// Fast syscall: no context switching
			int continueAddress = SyscallHandler.syscall(code, inDelaySlot);
			postSyscallFast();

			return continueAddress;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int syscall(int code, boolean inDelaySlot) throws Exception
		public static int syscall(int code, bool inDelaySlot)
		{
			preSyscall();
			int continueAddress = SyscallHandler.syscall(code, inDelaySlot);
			postSyscall();

			return continueAddress;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int syscallLLE(int code, boolean inDelaySlot) throws Exception
		public static int syscallLLE(int code, bool inDelaySlot)
		{
			int continueAddress = SyscallHandler.syscall(code, inDelaySlot);
			postSyscallLLE();

			return continueAddress;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void execWithReturnAddress(IExecutable executable, int returnAddress) throws Exception
		private static void execWithReturnAddress(IExecutable executable, int returnAddress)
		{
			while (true)
			{
				try
				{
					int address = executable.exec();
					if (address != returnAddress)
					{
						jump(address, returnAddress);
					}
					break;
				}
				catch (StackPopException e)
				{
					log.info("Stack exceeded maximum size, shrinking to top level");
					executable = getExecutable(e.Ra);
					if (executable == null)
					{
						throw e;
					}
				}
			}
		}

		public static bool executeFunction(int address)
		{
			IExecutable executable = getExecutable(address);
			int newPc = 0;
			int returnAddress = cpu._ra;
			bool exception = false;
			try
			{
				execWithReturnAddress(executable, returnAddress);
				newPc = returnAddress;
			}
			catch (StopThreadException)
			{
				// Ignore exception
			}
			catch (Exception e)
			{
				log.error("Catched Throwable in executeCallback:", e);
				exception = true;
			}
			cpu.pc = newPc;
			cpu.npc = newPc; // npc is used when context switching

			return exception;
		}

		public static void runThread(RuntimeThread thread)
		{
			setLog4jMDC();

			thread.InSyscall = true;

			if (StoppedThread)
			{
				// This thread has already been stopped before it is really starting...
				return;
			}

			thread.suspendRuntimeExecution();

			if (StoppedThread)
			{
				// This thread has already been stopped before it is really starting...
				return;
			}

			thread.onThreadStart();

			ThreadManForUser threadMan = Modules.ThreadManForUserModule;

			IExecutable executable = getExecutable(processor.cpu.pc);
			thread.InSyscall = false;
			try
			{
				updateStaticVariables();

				// Execute any thread event handler for THREAD_EVENT_START
				// in the thread context, before starting the thread execution.
				threadMan.checkPendingCallbacks();

				execWithReturnAddress(executable, ThreadManForUser.THREAD_EXIT_HANDLER_ADDRESS);
				// NOTE: When a thread exits by itself (without calling sceKernelExitThread),
				// it's exitStatus becomes it's return value.
				threadMan.hleKernelExitThread(processor.cpu._v0);
			}
			catch (StopThreadException)
			{
				// Ignore Exception
			}
			catch (Exception e)
			{
				// Do not spam exceptions when exiting...
				if (!Modules.ThreadManForUserModule.exitCalled)
				{
					// Log error in log file and command box
					log.error("Catched Throwable in RuntimeThread:", e);
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}

			SceKernelThreadInfo threadInfo = thread.ThreadInfo;
			alreadyStoppedThreads[threadInfo] = thread;

			if (log.DebugEnabled)
			{
				log.debug("End of Thread " + threadInfo.name + " - stopped");
			}

			// Tell stopAllThreads that this thread is stopped.
			thread.InSyscall = true;

			threads.Remove(threadInfo);
			toBeStoppedThreads.Remove(threadInfo);
			toBeDeletedThreads.Remove(threadInfo);

			if (!reset_Renamed)
			{
				// Switch to the currently active thread
				try
				{
					if (log.DebugEnabled)
					{
						log.debug("End of Thread " + threadInfo.name + " - sync");
					}

					// Be careful to not execute Interrupts or Callbacks by this thread,
					// as it is already stopped and the next active thread
					// will be resumed immediately.
					syncIdle();
					syncThreadImmediately();
				}
				catch (StopThreadException)
				{
				}
			}

			alreadyStoppedThreads.Remove(threadInfo);
			alreadySwitchedStoppedThreads.Remove(thread);

			if (log.DebugEnabled)
			{
				log.debug("End of Thread " + thread.Name);
			}

			lock (waitForEnd)
			{
				Monitor.Pulse(waitForEnd);
			}
		}

		private static void computeCodeBlocksRange()
		{
			codeBlocksLowestAddress = int.MaxValue;
			codeBlocksHighestAddress = int.MinValue;
			foreach (CodeBlock codeBlock in codeBlocks.Values)
			{
				if (!codeBlock.Internal)
				{
					codeBlocksLowestAddress = System.Math.Min(codeBlocksLowestAddress, codeBlock.LowestAddress);
					codeBlocksHighestAddress = System.Math.Max(codeBlocksHighestAddress, codeBlock.HighestAddress);
				}
			}
		}

		public static void addCodeBlock(int address, CodeBlock codeBlock)
		{
			int maskedAddress = address & addressMask;
			CodeBlock previousCodeBlock = codeBlocks[maskedAddress] = codeBlock;

			if (!codeBlock.Internal)
			{
				if (previousCodeBlock != null)
				{
					// One code block has been deleted, recompute the whole code blocks range
					computeCodeBlocksRange();

					int fastExecutableLoopukIndex = (maskedAddress - MemoryMap.START_RAM) >> 2;
					if (fastExecutableLoopukIndex >= 0 && fastExecutableLoopukIndex < fastExecutableLookup.Length)
					{
						fastExecutableLookup[fastExecutableLoopukIndex] = null;
					}
				}
				else
				{
					// One new code block has been added, update the code blocks range
					codeBlocksLowestAddress = System.Math.Min(codeBlocksLowestAddress, codeBlock.LowestAddress);
					codeBlocksHighestAddress = System.Math.Max(codeBlocksHighestAddress, codeBlock.HighestAddress);
				}

				int startIndex = ((codeBlock.LowestAddress & addressMask) - MemoryMap.START_RAM) >> fastCodeBlockLookupShift;
				int endIndex = ((codeBlock.HighestAddress & addressMask) - MemoryMap.START_RAM) >> fastCodeBlockLookupShift;
				for (int i = startIndex; i <= endIndex; i++)
				{
					if (i >= 0 && i < fastCodeBlockLookup.Length)
					{
						CodeBlockList codeBlockList = fastCodeBlockLookup[i];
						if (codeBlockList != null)
						{
							if (previousCodeBlock != null)
							{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET LinkedList equivalent to the Java 'remove' method:
								codeBlockList.remove(previousCodeBlock);
							}
							int addr = (i << fastCodeBlockLookupShift) + MemoryMap.START_RAM;
							int size = 1 << fastCodeBlockLookupShift;
							if (codeBlock.isOverlappingWithAddressRange(addr, size))
							{
								codeBlockList.AddLast(codeBlock);
							}
						}
					}
				}
			}
		}

		public static CodeBlock getCodeBlock(int address)
		{
			return codeBlocks[address & addressMask];
		}

		public static bool hasCodeBlock(int address)
		{
			return codeBlocks.ContainsKey(address & addressMask);
		}

		public static IDictionary<int, CodeBlock> CodeBlocks
		{
			get
			{
				return codeBlocks;
			}
		}

		public static IExecutable getExecutable(int address)
		{
			int maskedAddress = address & addressMask;
			// Check if we have already the executable in the fastExecutableLookup array
			int fastExecutableLoopukIndex = (maskedAddress - MemoryMap.START_RAM) >> 2;
			IExecutable executable;
			if (fastExecutableLoopukIndex >= 0 && fastExecutableLoopukIndex < fastExecutableLookup.Length)
			{
				executable = fastExecutableLookup[fastExecutableLoopukIndex];
			}
			else
			{
				executable = null;
			}

			if (executable == null)
			{
				CodeBlock codeBlock = getCodeBlock(address);
				if (codeBlock == null)
				{
					executable = Compiler.Instance.compile(address);
				}
				else
				{
					executable = codeBlock.Executable;
				}

				// Store the executable in the fastExecutableLookup array
				if (fastExecutableLoopukIndex >= 0 && fastExecutableLoopukIndex < fastExecutableLookup.Length)
				{
					fastExecutableLookup[fastExecutableLoopukIndex] = executable;
				}
			}

			return executable;
		}

		public static void start()
		{
			Settings.Instance.registerSettingsListener("RuntimeContext", "emu.compiler", new CompilerEnabledSettingsListerner());
		}

		public static void run()
		{
			if (Modules.ThreadManForUserModule.exitCalled)
			{
				return;
			}

			if (!initialise())
			{
				compilerEnabled = false;
				return;
			}

			log.info("Using Compiler");

			while (toBeStoppedThreads.Count > 0)
			{
				wakeupToBeStoppedThreads();
				sleep(idleSleepMicros);
			}

			reset_Renamed = false;

			if (currentRuntimeThread == null)
			{
				try
				{
					syncIdle();
				}
				catch (StopThreadException)
				{
					// Thread is stopped, return immediately
					return;
				}

				if (currentRuntimeThread == null)
				{
					log.error("RuntimeContext.run: nothing to run!");
					Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_UNKNOWN);
					return;
				}
			}

			update();

			if (processor.cpu.pc == 0)
			{
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_UNKNOWN);
				return;
			}

			currentRuntimeThread.continueRuntimeExecution();

			while (threads.Count > 0 && !reset_Renamed)
			{
				lock (waitForEnd)
				{
					try
					{
						Monitor.Wait(waitForEnd);
					}
					catch (InterruptedException)
					{
					}
				}
			}

			log.debug("End of run");
		}

		private static IList<RuntimeThread> wakeupToBeStoppedThreads()
		{
			IList<RuntimeThread> threadList = new LinkedList<RuntimeThread>();
			lock (toBeStoppedThreads)
			{
				foreach (KeyValuePair<SceKernelThreadInfo, RuntimeThread> entry in toBeStoppedThreads.SetOfKeyValuePairs())
				{
					threadList.Add(entry.Value);
				}
			}

			// Trigger the threads to start execution again.
			// Loop on a local list to avoid concurrent modification on toBeStoppedThreads.
			foreach (RuntimeThread runtimeThread in threadList)
			{
				Thread.State threadState = runtimeThread.State;
				log.debug("Thread " + runtimeThread.Name + ", State=" + threadState);
				if (threadState == Thread.State.TERMINATED)
				{
					toBeStoppedThreads.Remove(runtimeThread.ThreadInfo);
				}
				else if (threadState == Thread.State.WAITING)
				{
					runtimeThread.continueRuntimeExecution();
				}
			}

			lock (Emulator.Instance)
			{
				Monitor.PulseAll(Emulator.Instance);
			}

			return threadList;
		}

		public static void onThreadDeleted(SceKernelThreadInfo thread)
		{
			RuntimeThread runtimeThread = threads[thread];
			if (runtimeThread != null)
			{
				if (log.DebugEnabled)
				{
					log.debug("Deleting Thread " + thread.ToString());
				}
				toBeStoppedThreads[thread] = runtimeThread;
				if (runtimeThread.InSyscall && Thread.CurrentThread != runtimeThread)
				{
					toBeDeletedThreads[thread] = runtimeThread;
					log.debug("Continue Thread " + runtimeThread.Name);
					runtimeThread.continueRuntimeExecution();
				}
			}
		}

		public static void onThreadExit(SceKernelThreadInfo thread)
		{
			RuntimeThread runtimeThread = threads[thread];
			if (runtimeThread != null)
			{
				if (log.DebugEnabled)
				{
					log.debug("Exiting Thread " + thread.ToString());
				}
				toBeStoppedThreads[thread] = runtimeThread;
				threads.Remove(thread);
			}
		}

		public static void onThreadStart(SceKernelThreadInfo thread)
		{
			// The thread is starting, if a stop was still pending, cancel the stop.
			toBeStoppedThreads.Remove(thread);
			toBeDeletedThreads.Remove(thread);
		}

		private static void stopAllThreads()
		{
			lock (threads)
			{
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
				toBeStoppedThreads.putAll(threads);
				threads.Clear();
			}

			IList<RuntimeThread> threadList = wakeupToBeStoppedThreads();

			// Wait for all threads to enter a syscall.
			// When a syscall is entered, the thread will exit
			// automatically by calling checkStoppedThread()
			bool waitForThreads = true;
			while (waitForThreads)
			{
				waitForThreads = false;
				foreach (RuntimeThread runtimeThread in threadList)
				{
					if (!runtimeThread.InSyscall)
					{
						waitForThreads = true;
						break;
					}
				}

				if (waitForThreads)
				{
					sleep(idleSleepMicros);
				}
			}
		}

		public static void exit()
		{
			if (compilerEnabled)
			{
				log.debug("RuntimeContext.exit");
				stopAllThreads();
				if (DurationStatistics.collectStatistics)
				{
					log.info(idleDuration);
				}

				if (enableInstructionTypeCounting)
				{
					long totalCount = 0;
					foreach (Common.Instruction insn in instructionTypeCounts.Keys)
					{
						int count = instructionTypeCounts[insn];
						totalCount += count;
					}

					while (instructionTypeCounts.Count > 0)
					{
						Common.Instruction highestCountInsn = null;
						int highestCount = -1;
						foreach (Common.Instruction insn in instructionTypeCounts.Keys)
						{
							int count = instructionTypeCounts[insn];
							if (count > highestCount)
							{
								highestCount = count;
								highestCountInsn = insn;
							}
						}
						instructionTypeCounts.Remove(highestCountInsn);
						log.info(string.Format("  {0,10} {1} {2:D} ({3,2:F2}%)", highestCountInsn.name(), (highestCountInsn.hasFlags(Common.Instruction.FLAG_INTERPRETED) ? "I" : "C"), highestCount, highestCount * 100.0 / totalCount));
					}
				}
			}
		}

		public static void reset()
		{
			if (compilerEnabled)
			{
				log.debug("RuntimeContext.reset");
				Compiler.Instance.reset();
				codeBlocks.Clear();
				if (fastExecutableLookup != null)
				{
					Arrays.fill(fastExecutableLookup, null);
				}
				if (fastCodeBlockLookup != null)
				{
					Arrays.fill(fastCodeBlockLookup, null);
				}
				currentThread = null;
				currentRuntimeThread = null;
				reset_Renamed = true;
				stopAllThreads();
				lock (waitForEnd)
				{
					Monitor.Pulse(waitForEnd);
				}
			}
		}

		public static void invalidateAll()
		{
			if (compilerEnabled)
			{
				if (invalidateAllCodeBlocks)
				{
					// Simple method: invalidate all the code blocks,
					// independently if their were modified or not.
					log.debug("RuntimeContext.invalidateAll simple");
					codeBlocks.Clear();
					Arrays.fill(fastExecutableLookup, null);
					Arrays.fill(fastCodeBlockLookup, null);
					Compiler.Instance.invalidateAll();
				}
				else
				{
					// Advanced method: check all the code blocks for a modification
					// of their opcodes and invalidate only those code blocks that
					// have been modified.
					log.debug("RuntimeContext.invalidateAll advanced");
					Compiler compiler = Compiler.Instance;
					foreach (CodeBlock codeBlock in codeBlocks.Values)
					{
						if (log.DebugEnabled)
						{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("invalidateAll %s: opcodes changed %b", codeBlock, codeBlock.areOpcodesChanged()));
							log.debug(string.Format("invalidateAll %s: opcodes changed %b", codeBlock, codeBlock.areOpcodesChanged()));
						}

						if (codeBlock.areOpcodesChanged())
						{
							compiler.invalidateCodeBlock(codeBlock);
						}
					}
				}
			}
		}

		private static void invalidateRangeFullCheck(int addr, int size)
		{
			Compiler compiler = Compiler.Instance;
			foreach (CodeBlock codeBlock in codeBlocks.Values)
			{
				if (size == 0x4000 && codeBlock.HighestAddress >= addr)
				{
					// Some applications do not clear more than 16KB as this is the size of the complete Instruction Cache.
					// Be conservative in this case and check any code block above the given address.
					compiler.checkCodeBlockValidity(codeBlock);
				}
				else if (codeBlock.isOverlappingWithAddressRange(addr, size))
				{
					compiler.checkCodeBlockValidity(codeBlock);
				}
			}
		}

		private static CodeBlockList fillFastCodeBlockList(int index)
		{
			int startAddr = (index << fastCodeBlockLookupShift) + MemoryMap.START_RAM;
			int size = 1 << fastCodeBlockLookupShift;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Creating new fastCodeBlockList for 0x{0:X8}", startAddr));
			}

			CodeBlockList codeBlockList = new CodeBlockList();
			foreach (CodeBlock codeBlock in codeBlocks.Values)
			{
				if (codeBlock.isOverlappingWithAddressRange(startAddr, size))
				{
					codeBlockList.AddLast(codeBlock);
				}
			}
			fastCodeBlockLookup[index] = codeBlockList;

			return codeBlockList;
		}

		public static void invalidateRange(int addr, int size)
		{
			if (compilerEnabled)
			{
				addr &= Memory.addressMask;

				if (log.DebugEnabled)
				{
					log.debug(string.Format("RuntimeContext.invalidateRange(addr=0x{0:X8}, size={1:D})", addr, size));
				}

				// Fast check: if the address range is outside the largest code blocks range,
				// there is noting to do.
				if (addr + size < codeBlocksLowestAddress || addr > codeBlocksHighestAddress)
				{
					return;
				}

				// Check if the code blocks located in the given range have to be invalidated
				if (size == fastCodeBlockSize)
				{
					// This is a fast track to avoid checking all the code blocks
					int startIndex = (addr - MemoryMap.START_RAM) >> fastCodeBlockLookupShift;
					int endIndex = (addr + size - MemoryMap.START_RAM) >> fastCodeBlockLookupShift;
					if (startIndex >= 0 && endIndex <= fastCodeBlockLookup.Length)
					{
						for (int index = startIndex; index <= endIndex; index++)
						{
							CodeBlockList codeBlockList = fastCodeBlockLookup[index];
							if (codeBlockList == null)
							{
								codeBlockList = fillFastCodeBlockList(index);
							}
							else
							{
								if (log.DebugEnabled)
								{
									log.debug(string.Format("Reusing fastCodeBlockList for 0x{0:X8} (size={1:D})", addr, codeBlockList.Count));
								}
							}

							Compiler compiler = Compiler.Instance;
							foreach (CodeBlock codeBlock in codeBlockList)
							{
								if (codeBlock.isOverlappingWithAddressRange(addr, size))
								{
									compiler.checkCodeBlockValidity(codeBlock);
								}
							}
						}
					}
					else
					{
						invalidateRangeFullCheck(addr, size);
					}
				}
				else
				{
					invalidateRangeFullCheck(addr, size);
				}
			}
		}

		public static void instructionTypeCount(Common.Instruction insn, int opcode)
		{
			int count = 0;
			if (instructionTypeCounts.ContainsKey(insn))
			{
				count = instructionTypeCounts[insn];
			}
			count++;
			instructionTypeCounts[insn] = count;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void pauseEmuWithStatus(int status) throws StopThreadException
		public static void pauseEmuWithStatus(int status)
		{
			Emulator.PauseEmuWithStatus(status);
			syncPause();
		}

		public static void logInfo(string message)
		{
			log.info(message);
		}

		public static bool checkMemoryPointer(int address)
		{
			if (!Memory.isAddressGood(address))
			{
				if (!Memory.isRawAddressGood(Memory.normalizeAddress(address)))
				{
					return false;
				}
			}

			return true;
		}

		public static string readStringNZ(int address, int maxLength)
		{
			if (address == 0)
			{
				return null;
			}
			return Utilities.readStringNZ(address, maxLength);
		}

		public static PspString readPspStringNZ(int address, int maxLength, bool canBeNull)
		{
			return new PspString(address, maxLength, canBeNull);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int checkMemoryRead32(int address, int pc) throws StopThreadException
		public static int checkMemoryRead32(int address, int pc)
		{
			int rawAddress = address & Memory.addressMask;
			if (!Memory.isRawAddressGood(rawAddress))
			{
				if (memory.read32AllowedInvalidAddress(rawAddress))
				{
					rawAddress = 0;
				}
				else
				{
					int normalizedAddress = Memory.normalizeAddress(address);
					if (Memory.isRawAddressGood(normalizedAddress))
					{
						rawAddress = normalizedAddress;
					}
					else
					{
						processor.cpu.pc = pc;
						memory.invalidMemoryAddress(address, "read32", Emulator.EMU_STATUS_MEM_READ);
						syncPause();
						rawAddress = 0;
					}
				}
			}

			return rawAddress;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int checkMemoryRead16(int address, int pc) throws StopThreadException
		public static int checkMemoryRead16(int address, int pc)
		{
			int rawAddress = address & Memory.addressMask;
			if (!Memory.isRawAddressGood(rawAddress))
			{
				int normalizedAddress = Memory.normalizeAddress(address);
				if (Memory.isRawAddressGood(normalizedAddress))
				{
					rawAddress = normalizedAddress;
				}
				else
				{
					processor.cpu.pc = pc;
					memory.invalidMemoryAddress(address, "read16", Emulator.EMU_STATUS_MEM_READ);
					syncPause();
					rawAddress = 0;
				}
			}

			return rawAddress;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int checkMemoryRead8(int address, int pc) throws StopThreadException
		public static int checkMemoryRead8(int address, int pc)
		{
			int rawAddress = address & Memory.addressMask;
			if (!Memory.isRawAddressGood(rawAddress))
			{
				int normalizedAddress = Memory.normalizeAddress(address);
				if (Memory.isRawAddressGood(normalizedAddress))
				{
					rawAddress = normalizedAddress;
				}
				else
				{
					processor.cpu.pc = pc;
					memory.invalidMemoryAddress(address, "read8", Emulator.EMU_STATUS_MEM_READ);
					syncPause();
					rawAddress = 0;
				}
			}

			return rawAddress;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int checkMemoryWrite32(int address, int pc) throws StopThreadException
		public static int checkMemoryWrite32(int address, int pc)
		{
			int rawAddress = address & Memory.addressMask;
			if (!Memory.isRawAddressGood(rawAddress))
			{
				int normalizedAddress = Memory.normalizeAddress(address);
				if (Memory.isRawAddressGood(normalizedAddress))
				{
					rawAddress = normalizedAddress;
				}
				else
				{
					processor.cpu.pc = pc;
					memory.invalidMemoryAddress(address, "write32", Emulator.EMU_STATUS_MEM_WRITE);
					syncPause();
					rawAddress = 0;
				}
			}

			sceDisplayModule.write32(rawAddress);

			return rawAddress;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int checkMemoryWrite16(int address, int pc) throws StopThreadException
		public static int checkMemoryWrite16(int address, int pc)
		{
			int rawAddress = address & Memory.addressMask;
			if (!Memory.isRawAddressGood(rawAddress))
			{
				int normalizedAddress = Memory.normalizeAddress(address);
				if (Memory.isRawAddressGood(normalizedAddress))
				{
					rawAddress = normalizedAddress;
				}
				else
				{
					processor.cpu.pc = pc;
					memory.invalidMemoryAddress(address, "write16", Emulator.EMU_STATUS_MEM_WRITE);
					syncPause();
					rawAddress = 0;
				}
			}

			sceDisplayModule.write16(rawAddress);

			return rawAddress;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int checkMemoryWrite8(int address, int pc) throws StopThreadException
		public static int checkMemoryWrite8(int address, int pc)
		{
			int rawAddress = address & Memory.addressMask;
			if (!Memory.isRawAddressGood(rawAddress))
			{
				int normalizedAddress = Memory.normalizeAddress(address);
				if (Memory.isRawAddressGood(normalizedAddress))
				{
					rawAddress = normalizedAddress;
				}
				else
				{
					processor.cpu.pc = pc;
					memory.invalidMemoryAddress(address, "write8", Emulator.EMU_STATUS_MEM_WRITE);
					syncPause();
					rawAddress = 0;
				}
			}

			sceDisplayModule.write8(rawAddress);

			return rawAddress;
		}

		public static void debugMemoryReadWrite(int address, int value, int pc, bool isRead, int width)
		{
			if (log.TraceEnabled)
			{
				StringBuilder message = new StringBuilder();
				message.Append(string.Format("0x{0:X8} - ", pc));
				if (isRead)
				{
					message.Append(string.Format("read{0:D}(0x{1:X8})=0x", width, address));
					if (width == 8)
					{
						message.Append(string.Format("{0:X2}", memory.read8(address)));
					}
					else if (width == 16)
					{
						message.Append(string.Format("{0:X4}", memory.read16(address)));
					}
					else if (width == 32)
					{
						message.Append(string.Format("{0:X8} ({1:F})", memory.read32(address), Float.intBitsToFloat(memory.read32(address))));
					}
				}
				else
				{
					message.Append(string.Format("write{0:D}(0x{1:X8}, 0x", width, address));
					if (width == 8)
					{
						message.Append(string.Format("{0:X2}", value));
					}
					else if (width == 16)
					{
						message.Append(string.Format("{0:X4}", value));
					}
					else if (width == 32)
					{
						message.Append(string.Format("{0:X8} ({1:F})", value, Float.intBitsToFloat(value)));
					}
					message.Append(")");
				}
				log.trace(message.ToString());
			}
		}

		public static void onNextScheduleModified()
		{
			checkSync();

			if (!RuntimeContextLLE.LLEActive)
			{
				// Notify the thread waiting on the idleSyncObject that
				// the scheduler has now received a new schedule.
				lock (idleSyncObject)
				{
					Monitor.PulseAll(idleSyncObject);
				}
			}
		}

		public static void checkSync()
		{
			if (log.TraceEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.trace(String.format("checkSync wantSync=%b, now=0x%X", wantSync, pspsharp.scheduler.Scheduler.getNow()));
				log.trace(string.Format("checkSync wantSync=%b, now=0x%X", wantSync, Scheduler.Now));
			}

			if (!wantSync)
			{
				long delay = Emulator.Scheduler.getNextActionDelay(idleSleepMicros);
				if (delay <= 0)
				{
					wantSync = true;

					if (log.TraceEnabled)
					{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.trace(String.format("checkSync wantSync=%b, now=0x%X", wantSync, pspsharp.scheduler.Scheduler.getNow()));
						log.trace(string.Format("checkSync wantSync=%b, now=0x%X", wantSync, Scheduler.Now));
					}
				}
			}
		}

		public static void checkSyncWithSleep()
		{
			long delay = Emulator.Scheduler.getNextActionDelay(idleSleepMicros);

			if (delay > 0)
			{
				int intDelay = (int) delay;
				if (intDelay < 0)
				{
					intDelay = idleSleepMicros;
				}
				sleep(intDelay / 1000, intDelay % 1000);
			}
			else if (wantSync)
			{
				sleep(idleSleepMicros);
			}
			else
			{
				wantSync = true;
			}
		}

		public static bool syncDaemonStep()
		{
			checkSyncWithSleep();

			return enableDaemonThreadSync;
		}

		public static void exitSyncDaemon()
		{
			runtimeSyncThread = null;
		}

		public static bool IsHomebrew
		{
			set
			{
				RuntimeContext.isHomebrew = value;
			}
		}

		public static bool Homebrew
		{
			get
			{
				return isHomebrew;
			}
		}

		public static void onCodeModification(int pc, int opcode)
		{
			cpu.pc = pc;
			log.error(string.Format("Code instruction at 0x{0:X8} has been modified, expected 0x{1:X8}, current 0x{2:X8}", pc, opcode, memory.read32(pc)));
			Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_MEM_WRITE);
		}

		public static void debugMemory(int address, int length)
		{
			if (memory is DebuggerMemory)
			{
				DebuggerMemory debuggerMemory = (DebuggerMemory) memory;
				debuggerMemory.addRangeReadWriteBreakpoint(address, address + length - 1);
			}
		}

		public static void debugCodeBlock(int address, int numberOfArguments)
		{
			if (debugCodeBlockCalls)
			{
				debugCodeBlocks[address] = numberOfArguments;
			}
		}

		public static int FirmwareVersion
		{
			set
			{
				RuntimeContext.firmwareVersion = value;
			}
		}

		public static bool hasMemoryInt()
		{
			return memoryInt != null;
		}

		public static bool hasMemoryInt(int address)
		{
			return hasMemoryInt() && Memory.isAddressGood(address);
		}

		public static int[] MemoryInt
		{
			get
			{
				return memoryInt;
			}
		}

		public static int Pc
		{
			get
			{
				return Emulator.Processor.cpu.pc;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int executeEret() throws Exception
		public static int executeEret()
		{
			int epc = processor.cpu.doERET(processor);

			reboot.setLog4jMDC();
			reboot.dumpAllThreads();

			return epc;
		}

		private static int haltCount = 0;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void executeHalt(pspsharp.Processor processor) throws StopThreadException
		public static void executeHalt(Processor processor)
		{
			if (reboot.enableReboot)
			{
				// This playground implementation is related to the investigation
				// for the reboot process (flash0:/reboot.bin).
				if (processor.cp0.MediaEngineCpu)
				{
					((MEProcessor) processor).halt();
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Allegrex halt pendingInterruptIPbits=0x{0:X}", RuntimeContextLLE.pendingInterruptIPbits));
					}
					reboot.dumpAllThreads();
					if (false)
					{
						reboot.dumpAllModulesAndLibraries();
					}

					// Simulate an interrupt exception
					switch (haltCount)
					{
						case 0:
							// The module_start of display_01g.prx is requiring at least one VBLANK interrupt
							// as it is executing sceDisplayWaitVblankStart().
							MMIOHandlerDisplayController.Instance.triggerVblankInterrupt();
							break;
						case 1:
							// The init callback (sub_00000B08 registered by sceKernelSetInitCallback())
							// of display_01g.prx is requiring at least one VBLANK interrupt
							// as it is executing sceDisplayWaitVblankStart().
							MMIOHandlerDisplayController.Instance.triggerVblankInterrupt();
							break;
						case 2:
							// The thread SCE_VSH_GRAPHICS is calling sceDisplayWaitVblankStart().
							MMIOHandlerDisplayController.Instance.triggerVblankInterrupt();
							break;
						case 3:
							// The thread SCE_VSH_GRAPHICS is calling a function from paf.prx waiting for a vblank.
							MMIOHandlerDisplayController.MaxVblankInterrupts = -1;
							MMIOHandlerDisplayController.Instance.triggerVblankInterrupt();
							break;
						default:
							break;
					}
					haltCount++;

					idle();
				}
			}
			else
			{
				log.error("Allegrex halt");
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_HALT);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void idle() throws StopThreadException
		public static void idle()
		{
			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("idle wantSync=%b", wantSync));
				log.debug(string.Format("idle wantSync=%b", wantSync));
			}

			if (RuntimeContextLLE.LLEActive)
			{
				if (toBeStoppedThreads.Count > 0)
				{
					wantSync = true;
				}
				Emulator.Scheduler.step();
			}

			if (wantSync)
			{
				sync();
			}
			else
			{
				Utilities.sleep(1);
			}
		}

		public static void setLog4jMDC()
		{
			Log4jMDC = Thread.CurrentThread.Name;
		}

		public static string Log4jMDC
		{
			set
			{
				setLog4jMDC(value, 0);
			}
		}

		public static void setLog4jMDC(string threadName, int threadUid)
		{
			MDC.put("LLE-thread-name", threadName);

			if (threadUid != 0)
			{
				MDC.put("LLE-thread-uid", string.Format("0x{0:X}", threadUid));
				MDC.put("LLE-thread", string.Format("{0}_0x{1:X}", threadName, threadUid));
			}
			else
			{
				MDC.put("LLE-thread-uid", "");
				MDC.put("LLE-thread", threadName);
			}
		}
	}

}