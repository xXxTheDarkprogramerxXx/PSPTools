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

	using Logger = org.apache.log4j.Logger;

	using ExceptionManager = pspsharp.HLE.kernel.managers.ExceptionManager;
	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using reboot = pspsharp.HLE.modules.reboot;
	using MEProcessor = pspsharp.mediaengine.MEProcessor;
	using METhread = pspsharp.mediaengine.METhread;
	using MMIO = pspsharp.memory.mmio.MMIO;
	using MMIOHandlerInterruptMan = pspsharp.memory.mmio.MMIOHandlerInterruptMan;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class RuntimeContextLLE
	{
		public static Logger log = RuntimeContext.log;
		private const int STATE_VERSION = 0;
		private static readonly bool isLLEActive = reboot.enableReboot;
		private static Memory mmio;
		public volatile static int pendingInterruptIPbits;

		public static bool LLEActive
		{
			get
			{
				return isLLEActive;
			}
		}

		public static void start()
		{
			if (!LLEActive)
			{
				return;
			}

			createMMIO();
		}

		public static void createMMIO()
		{
			if (!LLEActive)
			{
				return;
			}

			if (mmio == null)
			{
				mmio = new MMIO(Emulator.Memory);
				if (mmio.allocate())
				{
					mmio.Initialise();
				}
				else
				{
					mmio = null;
				}
			}
		}

		public static Memory MMIO
		{
			get
			{
				return mmio;
			}
		}

		public static void triggerInterrupt(Processor processor, int interruptNumber)
		{
			if (!LLEActive)
			{
				return;
			}

			MMIOHandlerInterruptMan interruptMan = MMIOHandlerInterruptMan.getInstance(processor);
			if (!interruptMan.hasInterruptTriggered(interruptNumber))
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("triggerInterrupt 0x{0:X}({1})", interruptNumber, IntrManager.getInterruptName(interruptNumber)));
				}

				interruptMan.triggerInterrupt(interruptNumber);
			}
		}

		public static void clearInterrupt(Processor processor, int interruptNumber)
		{
			if (!LLEActive)
			{
				return;
			}

			MMIOHandlerInterruptMan interruptMan = MMIOHandlerInterruptMan.getInstance(processor);
			if (interruptMan.hasInterruptTriggered(interruptNumber))
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("clearInterrupt 0x{0:X}({1})", interruptNumber, IntrManager.getInterruptName(interruptNumber)));
				}

				interruptMan.clearInterrupt(interruptNumber);
			}
		}

		/*
		 * synchronized method as it can be called from different threads (e.g. CoreThreadMMIO)
		 */
		public static void triggerInterruptException(Processor processor, int IPbits)
		{
			lock (typeof(RuntimeContextLLE))
			{
				if (!LLEActive)
				{
					return;
				}
        
				if (processor.cp0.MainCpu)
				{
					pendingInterruptIPbits |= IPbits;
        
					if (log.DebugEnabled)
					{
						log.debug(string.Format("triggerInterruptException IPbits=0x{0:X}, pendingInterruptIPbits=0x{1:X}", IPbits, pendingInterruptIPbits));
					}
				}
			}
		}

		public static int triggerSyscallException(Processor processor, int syscallCode, bool inDelaySlot)
		{
			processor.cp0.SyscallCode = syscallCode << 2;
			int ebase = triggerException(processor, ExceptionManager.EXCEP_SYS, inDelaySlot);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Calling exception handler for Syscall at 0x{0:X8}, epc=0x{1:X8}", ebase, processor.cp0.Epc));
			}

			return ebase;
		}

		public static int triggerBreakException(Processor processor, bool inDelaySlot)
		{
			int ebase = triggerException(processor, ExceptionManager.EXCEP_BP, inDelaySlot);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Calling exception handler for Break at 0x{0:X8}, epc=0x{1:X8}", ebase, processor.cp0.Epc));
			}

			return ebase;
		}

		public static bool MediaEngineCpu
		{
			get
			{
				if (!LLEActive)
				{
					return false;
				}
				return METhread.isMediaEngine(Thread.CurrentThread);
			}
		}

		public static bool MainCpu
		{
			get
			{
				if (!LLEActive)
				{
					return true;
				}
				return !MediaEngineCpu;
			}
		}

		public static Processor MainProcessor
		{
			get
			{
				return Emulator.Processor;
			}
		}

		public static MEProcessor MediaEngineProcessor
		{
			get
			{
				return MEProcessor.Instance;
			}
		}

		public static Processor Processor
		{
			get
			{
				if (MediaEngineCpu)
				{
					return MediaEngineProcessor;
				}
				return MainProcessor;
			}
		}

		public static int triggerException(Processor processor, int exceptionNumber, bool inDelaySlot)
		{
			return prepareExceptionHandlerCall(processor, exceptionNumber, inDelaySlot);
		}

		/*
		 * synchronized method as it can be called from different threads (e.g. CoreThreadMMIO)
		 */
		public static void clearInterruptException(Processor processor, int IPbits)
		{
			lock (typeof(RuntimeContextLLE))
			{
				if (!LLEActive)
				{
					return;
				}
        
				pendingInterruptIPbits &= ~IPbits;
			}
		}

		private static bool isInterruptExceptionAllowed(Processor processor, int IPbits)
		{
			if (IPbits == 0)
			{
				log.debug("IPbits == 0");
				return false;
			}

			if (processor.InterruptsDisabled)
			{
				log.debug("Interrupts disabled");
				return false;
			}

			int status = processor.cp0.Status;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("cp0 Status=0x{0:X}", status));
			}

			// Is the processor already in an exception state?
			if ((status & 0x2) != 0)
			{
				return false;
			}

			// Is the interrupt masked?
			if ((status & 0x1) == 0 || ((IPbits << 8) & status) == 0)
			{
				return false;
			}

			return true;
		}

		private static int prepareExceptionHandlerCall(Processor processor, int exceptionNumber, bool inDelaySlot)
		{
			// Set the exception number and BD flag
			int cause = processor.cp0.Cause;
			cause = (cause & unchecked((int)0xFFFFFF00)) | (exceptionNumber << 2);
			if (inDelaySlot)
			{
				cause |= unchecked((int)0x80000000); // Set BD flag (Branch Delay Slot)
			}
			else
			{
				cause &= ~0x80000000; // Clear BD flag (Branch Delay Slot)
			}
			processor.cp0.Cause = cause;

			int epc = processor.cpu.pc;

			if (inDelaySlot)
			{
				epc -= 4; // The EPC is set to the instruction having the delay slot
			}

			// Set the EPC
			processor.cp0.Epc = epc;

			int ebase = processor.cp0.Ebase;

			// Set the EXL bit
			int status = processor.cp0.Status;
			status |= 0x2; // Set EXL bit
			processor.cp0.Status = status;

			return ebase;
		}

		/*
		 * synchronized method as it is modifying pendingInterruptIPbits which can be updated from different threads
		 */
		public static int checkPendingInterruptException(int returnAddress)
		{
			lock (typeof(RuntimeContextLLE))
			{
				Processor processor = Processor;
				if (isInterruptExceptionAllowed(processor, pendingInterruptIPbits))
				{
					int cause = processor.cp0.Cause;
					cause |= (pendingInterruptIPbits << 8);
					pendingInterruptIPbits = 0;
					processor.cp0.Cause = cause;
        
					// The compiler is only calling this function when
					// we are not in a delay slot
					int ebase = prepareExceptionHandlerCall(processor, ExceptionManager.EXCEP_INT, false);
        
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Calling exception handler for {0} at 0x{1:X8}, epc=0x{2:X8}, cause=0x{3:X}", MMIOHandlerInterruptMan.getInstance(processor).toStringInterruptTriggered(), ebase, processor.cp0.Epc, processor.cp0.Cause));
					}
        
					return ebase;
				}
        
				return returnAddress;
			}
		}

		/*
		 * synchronized method as it is modifying pendingInterruptIPbits which can be updated from different threads
		 */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static synchronized void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public static void read(StateInputStream stream)
		{
			lock (typeof(RuntimeContextLLE))
			{
				stream.readVersion(STATE_VERSION);
				pendingInterruptIPbits = stream.readInt();
			}
		}

		/*
		 * synchronized method as it is reading pendingInterruptIPbits which can be updated from different threads
		 */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static synchronized void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public static void write(StateOutputStream stream)
		{
			lock (typeof(RuntimeContextLLE))
			{
				stream.writeVersion(STATE_VERSION);
				stream.writeInt(pendingInterruptIPbits);
			}
		}
	}

}