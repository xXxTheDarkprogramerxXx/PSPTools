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

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using AbstractAllegrexInterruptHandler = pspsharp.HLE.kernel.types.interrupts.AbstractAllegrexInterruptHandler;
	using AbstractInterruptHandler = pspsharp.HLE.kernel.types.interrupts.AbstractInterruptHandler;
	using AfterSubIntrAction = pspsharp.HLE.kernel.types.interrupts.AfterSubIntrAction;
	using InterruptState = pspsharp.HLE.kernel.types.interrupts.InterruptState;
	using IntrHandler = pspsharp.HLE.kernel.types.interrupts.IntrHandler;
	using SubIntrHandler = pspsharp.HLE.kernel.types.interrupts.SubIntrHandler;
	using VBlankInterruptHandler = pspsharp.HLE.kernel.types.interrupts.VBlankInterruptHandler;
	using Scheduler = pspsharp.scheduler.Scheduler;

	//using Logger = org.apache.log4j.Logger;

	public class IntrManager
	{
		protected internal static Logger log = Modules.getLogger("ThreadManForUser");

		public const int PSP_GPIO_INTR = 4;
		public const int PSP_ATA_INTR = 5;
		public const int PSP_UMD_INTR = 6;
		public const int PSP_MSCM0_INTR = 7;
		public const int PSP_WLAN_INTR = 8;
		public const int PSP_AUDIO_INTR = 10;
		public const int PSP_I2C_INTR = 12;
		public const int PSP_SIRS_INTR = 14;
		public const int PSP_SYSTIMER0_INTR = 15;
		public const int PSP_SYSTIMER1_INTR = 16;
		public const int PSP_SYSTIMER2_INTR = 17;
		public const int PSP_SYSTIMER3_INTR = 18;
		public const int PSP_THREAD0_INTR = 19;
		public const int PSP_NAND_INTR = 20;
		public const int PSP_DMACPLUS_INTR = 21;
		public const int PSP_DMA0_INTR = 22;
		public const int PSP_DMA1_INTR = 23;
		public const int PSP_MEMLMD_INTR = 24;
		public const int PSP_GE_INTR = 25;
		public const int PSP_VBLANK_INTR = 30;
		public const int PSP_MECODEC_INTR = 31;
		public const int PSP_HPREMOTE_INTR = 36;
		public const int PSP_USB_56 = 56;
		public const int PSP_USB_57 = 57;
		public const int PSP_USB_58 = 58;
		public const int PSP_USB_59 = 59;
		public const int PSP_MSCM1_INTR = 60;
		public const int PSP_MSCM2_INTR = 61;
		public const int PSP_THREAD1_INTR = 65;
		public const int PSP_INTERRUPT_INTR = 66;
		public const int PSP_NUMBER_INTERRUPTS = 67;
		private static string[] PSP_INTERRUPT_NAMES;

		public const int VBLANK_SCHEDULE_MICROS = (1000000 + 30) / 60; // 1/60 second (rounded)

		protected internal static IntrManager instance = null;
		private List<LinkedList<AbstractInterruptHandler>> interrupts;
		protected internal IntrHandler[] intrHandlers;
		protected internal bool insideInterrupt;
		protected internal IList<AbstractAllegrexInterruptHandler> allegrexInterruptHandlers;
		// Deferred interrupts are interrupts that were triggered by the scheduler
		// while the interrupts were disabled.
		// They have to be processed as soon as the interrupts are re-enabled.
		protected internal IList<AbstractInterruptHandler> deferredInterrupts;

		private VBlankInterruptHandler vblankInterruptHandler;

		public static IntrManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new IntrManager();
				}
    
				return instance;
			}
		}

		private IntrManager()
		{
			vblankInterruptHandler = new VBlankInterruptHandler();
		}

		public virtual void reset()
		{
			stop();
			installDefaultInterrupts();
		}

		public virtual void stop()
		{
			interrupts = new List<LinkedList<AbstractInterruptHandler>>(PSP_NUMBER_INTERRUPTS);
			interrupts.Capacity = PSP_NUMBER_INTERRUPTS;
			intrHandlers = new IntrHandler[IntrManager.PSP_NUMBER_INTERRUPTS];
			allegrexInterruptHandlers = new LinkedList<AbstractAllegrexInterruptHandler>();

			deferredInterrupts = new LinkedList<AbstractInterruptHandler>();
		}

		public static string getInterruptName(int interruptNumber)
		{
			if (PSP_INTERRUPT_NAMES == null)
			{
				PSP_INTERRUPT_NAMES = new string[PSP_NUMBER_INTERRUPTS];
				PSP_INTERRUPT_NAMES[PSP_GPIO_INTR] = "GPIO";
				PSP_INTERRUPT_NAMES[PSP_ATA_INTR] = "ATA";
				PSP_INTERRUPT_NAMES[PSP_UMD_INTR] = "UMD";
				PSP_INTERRUPT_NAMES[PSP_MSCM0_INTR] = "MSCM0";
				PSP_INTERRUPT_NAMES[PSP_WLAN_INTR] = "WLAN";
				PSP_INTERRUPT_NAMES[PSP_AUDIO_INTR] = "AUDIO";
				PSP_INTERRUPT_NAMES[PSP_I2C_INTR] = "I2C";
				PSP_INTERRUPT_NAMES[PSP_SIRS_INTR] = "SIRS";
				PSP_INTERRUPT_NAMES[PSP_SYSTIMER0_INTR] = "SYSTIMER0";
				PSP_INTERRUPT_NAMES[PSP_SYSTIMER1_INTR] = "SYSTIMER1";
				PSP_INTERRUPT_NAMES[PSP_SYSTIMER2_INTR] = "SYSTIMER2";
				PSP_INTERRUPT_NAMES[PSP_SYSTIMER3_INTR] = "SYSTIMER3";
				PSP_INTERRUPT_NAMES[PSP_THREAD0_INTR] = "THREAD0";
				PSP_INTERRUPT_NAMES[PSP_NAND_INTR] = "NAND";
				PSP_INTERRUPT_NAMES[PSP_DMACPLUS_INTR] = "DMACPLUS";
				PSP_INTERRUPT_NAMES[PSP_DMA0_INTR] = "DMA0";
				PSP_INTERRUPT_NAMES[PSP_DMA1_INTR] = "DMA1";
				PSP_INTERRUPT_NAMES[PSP_MEMLMD_INTR] = "MEMLMD";
				PSP_INTERRUPT_NAMES[PSP_GE_INTR] = "GE";
				PSP_INTERRUPT_NAMES[PSP_VBLANK_INTR] = "VBLANK";
				PSP_INTERRUPT_NAMES[PSP_MECODEC_INTR] = "MECODEC";
				PSP_INTERRUPT_NAMES[PSP_HPREMOTE_INTR] = "HPREMOTE";
				PSP_INTERRUPT_NAMES[PSP_MSCM1_INTR] = "MSCM1";
				PSP_INTERRUPT_NAMES[PSP_MSCM2_INTR] = "MSCM2";
				PSP_INTERRUPT_NAMES[PSP_THREAD1_INTR] = "THREAD1";
				PSP_INTERRUPT_NAMES[PSP_INTERRUPT_INTR] = "INTERRUPT";
			}

			string name = null;
			if (interruptNumber >= 0 && interruptNumber < PSP_INTERRUPT_NAMES.Length)
			{
				name = PSP_INTERRUPT_NAMES[interruptNumber];
			}

			if (string.ReferenceEquals(name, null))
			{
				name = string.Format("INTERRUPT_{0:X}", interruptNumber);
			}

			return name;
		}

		private void installDefaultInterrupts()
		{
			// No default interrupts when running LLE
			if (RuntimeContextLLE.LLEActive)
			{
				return;
			}

			Scheduler scheduler = Emulator.Scheduler;

			// install VBLANK interrupt every 1/60 second
			scheduler.addAction(Scheduler.Now + VBLANK_SCHEDULE_MICROS, vblankInterruptHandler);
		}

		public virtual void addDeferredInterrupt(AbstractInterruptHandler interruptHandler)
		{
			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("addDeferredInterrupt insideInterrupt=%b, interruptsEnabled=%b", isInsideInterrupt(), pspsharp.Emulator.getProcessor().isInterruptsEnabled()));
				Console.WriteLine(string.Format("addDeferredInterrupt insideInterrupt=%b, interruptsEnabled=%b", InsideInterrupt, Emulator.Processor.InterruptsEnabled));
			}
			deferredInterrupts.Add(interruptHandler);
		}

		public virtual bool canExecuteInterruptNow()
		{
			return !InsideInterrupt && Emulator.Processor.InterruptsEnabled;
		}

		public virtual void onInterruptsEnabled()
		{
			if (deferredInterrupts.Count > 0 && canExecuteInterruptNow())
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine("Executing deferred interrupts");
				}

				IList<AbstractInterruptHandler> copyDeferredInterrupts = new LinkedList<AbstractInterruptHandler>(deferredInterrupts);
				deferredInterrupts.Clear();
				executeInterrupts(copyDeferredInterrupts, null, null);
			}
		}

		public virtual LinkedList<AbstractInterruptHandler> getInterruptHandlers(int intrNumber)
		{
			if (intrNumber < 0 || intrNumber >= PSP_NUMBER_INTERRUPTS)
			{
				return null;
			}

			return interrupts[intrNumber];
		}

		public virtual void addInterruptHandler(int interruptNumber, AbstractInterruptHandler interruptHandler)
		{
			if (interruptNumber < 0 || interruptNumber >= PSP_NUMBER_INTERRUPTS)
			{
				return;
			}

			LinkedList<AbstractInterruptHandler> interruptHandlers = interrupts[interruptNumber];
			if (interruptHandlers == null)
			{
				interruptHandlers = new LinkedList<AbstractInterruptHandler>();
				interrupts[interruptNumber] = interruptHandlers;
			}

			interruptHandlers.AddLast(interruptHandler);
		}

		public virtual bool removeInterruptHandler(int intrNumber, AbstractInterruptHandler interruptHandler)
		{
			if (intrNumber < 0 || intrNumber >= PSP_NUMBER_INTERRUPTS)
			{
				return false;
			}

			LinkedList<AbstractInterruptHandler> interruptHandlers = interrupts[intrNumber];
			if (interruptHandlers == null)
			{
				return false;
			}

//JAVA TO C# CONVERTER TODO TASK: There is no .NET LinkedList equivalent to the Java 'remove' method:
			return interruptHandlers.remove(interruptHandler);
		}


		protected internal virtual void onEndOfInterrupt()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine("End of Interrupt");
			}

			allegrexInterruptHandlers.Clear();

			// Schedule to a thread having a higher priority if one is ready to run
			Modules.ThreadManForUserModule.hleRescheduleCurrentThread();
			onInterruptsEnabled();
		}

		public virtual void pushAllegrexInterruptHandler(AbstractAllegrexInterruptHandler allegrexInterruptHandler)
		{
			allegrexInterruptHandlers.Add(allegrexInterruptHandler);
		}

		public virtual void continueCallAllegrexInterruptHandler(InterruptState interruptState, IEnumerator<AbstractAllegrexInterruptHandler> allegrexInterruptHandlersIterator, IAction continueAction)
		{
			bool somethingExecuted = false;
			do
			{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				if (allegrexInterruptHandlersIterator != null && allegrexInterruptHandlersIterator.hasNext())
				{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					AbstractAllegrexInterruptHandler allegrexInterruptHandler = allegrexInterruptHandlersIterator.next();
					if (allegrexInterruptHandler != null)
					{
						//if (log.DebugEnabled)
						{
							Console.WriteLine("Calling InterruptHandler " + allegrexInterruptHandler.ToString());
						}
						allegrexInterruptHandler.copyArgumentsToCpu(Emulator.Processor.cpu);
						Modules.ThreadManForUserModule.callAddress(allegrexInterruptHandler.Address, continueAction, true);
						somethingExecuted = true;
					}
				}
				else
				{
					break;
				}
			} while (!somethingExecuted);

			if (!somethingExecuted)
			{
				// No more handlers, end of interrupt
				InsideInterrupt = interruptState.restore(Emulator.Processor.cpu);
				IAction afterInterruptAction = interruptState.AfterInterruptAction;
				if (afterInterruptAction != null)
				{
					afterInterruptAction.execute();
				}
				onEndOfInterrupt();
			}
		}

		protected internal virtual void executeInterrupts(IList<AbstractInterruptHandler> interruptHandlers, IAction afterInterruptAction, IAction afterHandlerAction)
		{
			if (interruptHandlers != null)
			{
				for (IEnumerator<AbstractInterruptHandler> it = interruptHandlers.GetEnumerator(); it.MoveNext();)
				{
					AbstractInterruptHandler interruptHandler = it.Current;
					if (interruptHandler != null)
					{
						interruptHandler.execute();
					}
				}
			}

			if (allegrexInterruptHandlers.Count == 0)
			{
				if (afterInterruptAction != null)
				{
					afterInterruptAction.execute();
				}
				onEndOfInterrupt();
			}
			else
			{
				InterruptState interruptState = new InterruptState();
				interruptState.save(insideInterrupt, Emulator.Processor.cpu, afterInterruptAction, afterHandlerAction);
				InsideInterrupt = true;

				IEnumerator<AbstractAllegrexInterruptHandler> allegrexInterruptHandlersIterator = allegrexInterruptHandlers.GetEnumerator();
				IAction continueAction = new AfterSubIntrAction(this, interruptState, allegrexInterruptHandlersIterator);

				continueCallAllegrexInterruptHandler(interruptState, allegrexInterruptHandlersIterator, continueAction);
			}
		}

		public virtual void triggerInterrupt(int interruptNumber, IAction afterInterruptAction, IAction afterHandlerAction)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Triggering Interrupt {0}(0x{1:X})", getInterruptName(interruptNumber), interruptNumber));
			}

			executeInterrupts(getInterruptHandlers(interruptNumber), afterInterruptAction, afterHandlerAction);
		}

		public virtual void triggerInterrupt(int interruptNumber, IAction afterInterruptAction, IAction afterHandlerAction, AbstractAllegrexInterruptHandler allegrexInterruptHandler)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Triggering Interrupt {0}(0x{1:X}) at 0x{2:X8}", getInterruptName(interruptNumber), interruptNumber, allegrexInterruptHandler.Address));
			}

			// Trigger only this interrupt handler
			allegrexInterruptHandlers.Add(allegrexInterruptHandler);
			executeInterrupts(null, afterInterruptAction, afterHandlerAction);
		}

		public virtual bool InsideInterrupt
		{
			get
			{
				return insideInterrupt;
			}
			set
			{
				this.insideInterrupt = value;
			}
		}


		public virtual void addVBlankAction(IAction action)
		{
			vblankInterruptHandler.addVBlankAction(action);
		}

		public virtual bool removeVBlankAction(IAction action)
		{
			return vblankInterruptHandler.removeVBlankAction(action);
		}

		public virtual void addVBlankActionOnce(IAction action)
		{
			vblankInterruptHandler.addVBlankActionOnce(action);
		}

		public virtual bool removeVBlankActionOnce(IAction action)
		{
			return vblankInterruptHandler.removeVBlankActionOnce(action);
		}

		public virtual int sceKernelRegisterSubIntrHandler(int intrNumber, int subIntrNumber, TPointer handlerAddress, int handlerArgument)
		{
			if (intrNumber < 0 || intrNumber >= IntrManager.PSP_NUMBER_INTERRUPTS || subIntrNumber < 0)
			{
				return SceKernelErrors.ERROR_KERNEL_INVALID_INTR_NUMBER;
			}

			if (intrHandlers[intrNumber] == null)
			{
				IntrHandler intrHandler = new IntrHandler();
				intrHandlers[intrNumber] = intrHandler;
				addInterruptHandler(intrNumber, intrHandler);
			}
			else if (intrHandlers[intrNumber].getSubIntrHandler(subIntrNumber) != null)
			{
				return SceKernelErrors.ERROR_KERNEL_SUBINTR_ALREADY_REGISTERED;
			}

			SubIntrHandler subIntrHandler = new SubIntrHandler(handlerAddress.Address, subIntrNumber, handlerArgument);
			subIntrHandler.Enabled = false;
			intrHandlers[intrNumber].addSubIntrHandler(subIntrNumber, subIntrHandler);

			return 0;
		}

		public virtual int sceKernelReleaseSubIntrHandler(int intrNumber, int subIntrNumber)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelReleaseSubIntrHandler({0:D}, {1:D})", intrNumber, subIntrNumber));
			}

			if (intrNumber < 0 || intrNumber >= IntrManager.PSP_NUMBER_INTERRUPTS || subIntrNumber < 0)
			{
				return SceKernelErrors.ERROR_KERNEL_INVALID_INTR_NUMBER;
			}

			if (intrHandlers[intrNumber] == null)
			{
				return SceKernelErrors.ERROR_KERNEL_SUBINTR_NOT_REGISTERED;
			}

			if (!intrHandlers[intrNumber].removeSubIntrHandler(subIntrNumber))
			{
				return SceKernelErrors.ERROR_KERNEL_SUBINTR_NOT_REGISTERED;
			}

			return 0;
		}

		protected internal virtual int hleKernelEnableDisableSubIntr(int intrNumber, int subIntrNumber, bool enabled)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernel{0}SubIntr({1:D}, {2:D})", enabled ? "Enable" : "Disable", intrNumber, subIntrNumber));
			}

			if (intrNumber < 0 || intrNumber >= IntrManager.PSP_NUMBER_INTERRUPTS)
			{
				return -1;
			}

			if (intrHandlers[intrNumber] == null)
			{
				return -1;
			}

			SubIntrHandler subIntrHandler = intrHandlers[intrNumber].getSubIntrHandler(subIntrNumber);
			if (subIntrHandler == null)
			{
				return -1;
			}

			subIntrHandler.Enabled = enabled;

			return 0;
		}

		public virtual int sceKernelEnableSubIntr(int intrNumber, int subIntrNumber)
		{
			return hleKernelEnableDisableSubIntr(intrNumber, subIntrNumber, true);
		}

		public virtual int sceKernelDisableSubIntr(int intrNumber, int subIntrNumber)
		{
			return hleKernelEnableDisableSubIntr(intrNumber, subIntrNumber, false);
		}
	}
}