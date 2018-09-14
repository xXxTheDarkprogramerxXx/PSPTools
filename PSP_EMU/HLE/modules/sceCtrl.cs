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
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;


	using Managers = pspsharp.HLE.kernel.Managers;
	using SystemTimeManager = pspsharp.HLE.kernel.managers.SystemTimeManager;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;

	using Logger = org.apache.log4j.Logger;

	public class sceCtrl : HLEModule
	{
		public static Logger log = Modules.getLogger("sceCtrl");

		private int cycle;
		private int mode;
		private int uiMake;
		private int uiBreak;
		private int uiPress;
		private int uiRelease;
		private int TimeStamp; // microseconds
		private sbyte Lx;
		private sbyte Ly;
		private sbyte Rx;
		private sbyte Ry;
		private int Buttons;

		// IdleCancelThreshold
		private int idlereset;
		private int idleback;
		public const int PSP_CTRL_SELECT = 0x0000001;
		public const int PSP_CTRL_START = 0x0000008;
		public const int PSP_CTRL_UP = 0x0000010;
		public const int PSP_CTRL_RIGHT = 0x0000020;
		public const int PSP_CTRL_DOWN = 0x0000040;
		public const int PSP_CTRL_LEFT = 0x0000080;
		public const int PSP_CTRL_LTRIGGER = 0x0000100;
		public const int PSP_CTRL_RTRIGGER = 0x0000200;
		public const int PSP_CTRL_TRIANGLE = 0x0001000;
		public const int PSP_CTRL_CIRCLE = 0x0002000;
		public const int PSP_CTRL_CROSS = 0x0004000;
		public const int PSP_CTRL_SQUARE = 0x0008000;
		public const int PSP_CTRL_HOME = 0x0010000;
		public const int PSP_CTRL_HOLD = 0x0020000;
		public const int PSP_CTRL_WLAN_UP = 0x0040000;
		public const int PSP_CTRL_REMOTE = 0x0080000;
		public const int PSP_CTRL_VOLUP = 0x0100000;
		public const int PSP_CTRL_VOLDOWN = 0x0200000;
		public const int PSP_CTRL_SCREEN = 0x0400000;
		public const int PSP_CTRL_NOTE = 0x0800000;
		public const int PSP_CTRL_DISC = 0x1000000;
		public const int PSP_CTRL_MS = 0x2000000;

		// PspCtrlMode
		public const int PSP_CTRL_MODE_DIGITAL = 0;
		public const int PSP_CTRL_MODE_ANALOG = 1;
		protected internal IAction sampleAction = null;
		protected internal Sample[] samples;
		protected internal int currentSamplingIndex;
		protected internal int currentReadingIndex;
		protected internal int latchSamplingCount;
		// PSP remembers the last 64 samples.
		protected internal const int SAMPLE_BUFFER_SIZE = 64;
		protected internal IList<ThreadWaitingForSampling> threadsWaitingForSampling;

		public virtual bool ModeDigital
		{
			get
			{
				if (mode == PSP_CTRL_MODE_DIGITAL)
				{
					return true;
				}
				return false;
			}
		}

		public virtual int SamplingMode
		{
			set
			{
				this.mode = value;
			}
		}

		private static int Timestamp
		{
			get
			{
				return ((int) SystemTimeManager.SystemTime) & 0x7FFFFFFF;
			}
		}

		private void setButtons(sbyte Lx, sbyte Ly, sbyte Rx, sbyte Ry, int Buttons, bool hasRightAnalogController)
		{
			int oldButtons = this.Buttons;

			this.TimeStamp = Timestamp;
			this.Lx = Lx;
			this.Ly = Ly;
			if (hasRightAnalogController)
			{
				this.Rx = Rx;
				this.Ry = Ry;
			}
			else
			{
				this.Rx = 0;
				this.Ry = 0;
			}
			this.Buttons = Buttons;

			if (ModeDigital)
			{
				// PSP_CTRL_MODE_DIGITAL
				// moving the analog stick has no effect and always returns 128,128
				this.Lx = Controller.analogCenter;
				this.Ly = Controller.analogCenter;
				if (hasRightAnalogController)
				{
					this.Rx = Controller.analogCenter;
					this.Ry = Controller.analogCenter;
				}
			}

			int changed = oldButtons ^ Buttons;
			int unpressed = ~Buttons;

			uiMake |= Buttons & changed;
			uiBreak |= unpressed & changed;
			uiPress |= Buttons;
			uiRelease |= unpressed;
		}

		public override void start()
		{
			uiMake = 0;
			uiBreak = 0;
			uiPress = 0;
			uiRelease = ~uiPress;

			Lx = Controller.analogCenter;
			Ly = Controller.analogCenter;
			Rx = Controller.analogCenter;
			Ry = Controller.analogCenter;
			Buttons = 0;

			idlereset = -1;
			idleback = -1;

			mode = PSP_CTRL_MODE_DIGITAL; // check initial mode
			cycle = 0;

			// Allocate 1 more entry because we always leave 1 entry free
			// for the internal management
			// (to differentiate a full buffer from an empty one).
			samples = new Sample[SAMPLE_BUFFER_SIZE + 1];
			for (int i = 0; i < samples.Length; i++)
			{
				samples[i] = new Sample();
				samples[i].setValues(0, Lx, Ly, Rx, Ry, Buttons);
			}
			currentSamplingIndex = 0;
			currentReadingIndex = 0;
			latchSamplingCount = 0;

			threadsWaitingForSampling = new LinkedList<ThreadWaitingForSampling>();

			if (sampleAction == null)
			{
				sampleAction = new SamplingAction(this);
				Managers.intr.addVBlankAction(sampleAction);
			}

			base.start();
		}

		protected internal class SamplingAction : IAction
		{
			private readonly sceCtrl outerInstance;

			public SamplingAction(sceCtrl outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.hleCtrlExecuteSampling();
			}
		}

		protected internal class ThreadWaitingForSampling
		{
			internal SceKernelThreadInfo thread;
			internal int readAddr;
			internal int readCount;
			internal bool readPositive;

			public ThreadWaitingForSampling(SceKernelThreadInfo thread, int readAddr, int readCount, bool readPositive)
			{
				this.thread = thread;
				this.readAddr = readAddr;
				this.readCount = readCount;
				this.readPositive = readPositive;
			}
		}

		protected internal class Sample
		{
			internal int TimeStamp; // microseconds
			internal sbyte Lx;
			internal sbyte Ly;
			internal sbyte Rx;
			internal sbyte Ry;
			internal int Buttons;

			public virtual void setValues(int TimeStamp, sbyte Lx, sbyte Ly, sbyte Rx, sbyte Ry, int Buttons)
			{
				this.TimeStamp = TimeStamp;
				this.Lx = Lx;
				this.Ly = Ly;
				this.Rx = Rx;
				this.Ry = Ry;
				this.Buttons = Buttons;
			}

			public virtual int write(Memory mem, int addr, bool positive)
			{
				mem.write32(addr, TimeStamp);
				mem.write32(addr + 4, positive ? Buttons :~Buttons);
				mem.write8(addr + 8, Lx);
				mem.write8(addr + 9, Ly);

				// These 2 values are always set to 0 on a PSP,
				// but are used for a second analog stick on the PS3 PSP emulator (for HD remaster)
				mem.write8(addr + 10, Rx);
				mem.write8(addr + 11, Ry);

				// Always set to 0
				mem.write8(addr + 12, (sbyte) 0);
				mem.write8(addr + 13, (sbyte) 0);
				mem.write8(addr + 14, (sbyte) 0);
				mem.write8(addr + 15, (sbyte) 0);

				return addr + 16;
			}

			public override string ToString()
			{
				return string.Format("TimeStamp={0:D},Lx={1:D},Ly={2:D},Rx={3:D},Ry={4:D},Buttons={5:X7}", TimeStamp, Lx, Ly, Rx, Ry, Buttons);
			}
		}

		/// <summary>
		/// Increment (or decrement) the given Sample Index
		/// (currentSamplingIndex or currentReadingIndex).
		/// </summary>
		/// <param name="index"> the current index value
		///              0 <= index < samples.length </param>
		/// <param name="count"> the increment (or decrement) value.
		///              -samples.length <= count <= samples.length </param>
		/// <returns>      the incremented index value
		///              0 <= returned value < samples.length </returns>
		protected internal virtual int incrementSampleIndex(int index, int count)
		{
			index += count;
			if (index >= samples.Length)
			{
				index -= samples.Length;
			}
			else if (index < 0)
			{
				index += samples.Length;
			}

			return index;
		}

		/// <summary>
		/// Increment the given Sample Index by 1
		/// (currentSamplingIndex or currentReadingIndex).
		/// </summary>
		/// <param name="index"> the current index value
		///              0 <= index < samples.length </param>
		/// <returns>      the index value incremented by 1
		///              0 <= returned value < samples.length </returns>
		protected internal virtual int incrementSampleIndex(int index)
		{
			return incrementSampleIndex(index, 1);
		}

		protected internal virtual int NumberOfAvailableSamples
		{
			get
			{
				int n = currentSamplingIndex - currentReadingIndex;
				if (n < 0)
				{
					n += samples.Length;
				}
    
				return n;
			}
		}

		public virtual int checkThreshold(int threshold)
		{
			if (threshold < -1 || threshold > 128)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_VALUE);
			}

			return threshold;
		}

		public virtual int checkMode(int mode)
		{
			if (mode != PSP_CTRL_MODE_DIGITAL && mode != PSP_CTRL_MODE_ANALOG)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_MODE);
			}

			return mode;
		}

		public virtual int checkCycle(int cycle)
		{
			if (cycle < 0 || (cycle > 0 && cycle < 5555) || cycle > 20000)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_VALUE);
			}
			return cycle;
		}

		public virtual void hleCtrlExecuteSampling()
		{
			if (log.DebugEnabled)
			{
				log.debug("hleCtrlExecuteSampling");
			}

			Controller controller = State.controller;
			controller.hleControllerPoll();

			setButtons(controller.Lx, controller.Ly, controller.Rx, controller.Ry, controller.Buttons, controller.hasRightAnalogController());

			latchSamplingCount++;

			Sample currentSampling = samples[currentSamplingIndex];
			currentSampling.setValues(TimeStamp, Lx, Ly, Rx, Ry, Buttons);

			currentSamplingIndex = incrementSampleIndex(currentSamplingIndex);
			if (currentSamplingIndex == currentReadingIndex)
			{
				currentReadingIndex = incrementSampleIndex(currentReadingIndex);
			}

			while (threadsWaitingForSampling.Count > 0)
			{
				ThreadWaitingForSampling wait = threadsWaitingForSampling.RemoveAt(0);
				if (wait.thread.isWaitingForType(SceKernelThreadInfo.JPCSP_WAIT_CTRL))
				{
					if (log.DebugEnabled)
					{
						log.debug("hleExecuteSampling waiting up thread " + wait.thread);
					}
					wait.thread.cpuContext._v0 = hleCtrlReadBufferImmediately(wait.readAddr, wait.readCount, wait.readPositive, false);
					Modules.ThreadManForUserModule.hleUnblockThread(wait.thread.uid);
					break;
				}

				if (log.DebugEnabled)
				{
					log.debug("hleExecuteSampling thread " + wait.thread + " was no longer blocked");
				}
			}
		}

		protected internal virtual int hleCtrlReadBufferImmediately(int addr, int count, bool positive, bool peek)
		{
			if (count < 0 || count > SAMPLE_BUFFER_SIZE)
			{
				return SceKernelErrors.ERROR_INVALID_SIZE;
			}

			Memory mem = Memory.Instance;

			// If more samples are available than requested, read the more recent ones
			int available = NumberOfAvailableSamples;
			int readIndex;
			if (available > count || peek)
			{
				readIndex = incrementSampleIndex(currentSamplingIndex, -count);
			}
			else
			{
				count = available;
				readIndex = currentReadingIndex;
			}

			if (!peek)
			{
				// Forget the remaining samples if they are not read now
				currentReadingIndex = currentSamplingIndex;
			}

			for (int ctrlCount = 0; ctrlCount < count; ctrlCount++)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("now={0:D}, samples[{1:D}]={2}", Timestamp, readIndex, samples[readIndex]));
				}
				addr = samples[readIndex].write(mem, addr, positive);
				readIndex = incrementSampleIndex(readIndex);
			}

			if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("hleCtrlReadBufferImmediately(positive=%b, peek=%b) returning %d", positive, peek, count));
				log.debug(string.Format("hleCtrlReadBufferImmediately(positive=%b, peek=%b) returning %d", positive, peek, count));
			}

			return count;
		}

		protected internal virtual int hleCtrlReadBuffer(int addr, int count, bool positive)
		{
			if (count < 0 || count > SAMPLE_BUFFER_SIZE)
			{
				return SceKernelErrors.ERROR_INVALID_SIZE;
			}

			// Some data available in sample buffer?
			if (NumberOfAvailableSamples > 0)
			{
				// Yes, read immediately
				return hleCtrlReadBufferImmediately(addr, count, positive, false);
			}

			// No, wait for next sampling
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			SceKernelThreadInfo currentThread = threadMan.CurrentThread;
			ThreadWaitingForSampling threadWaitingForSampling = new ThreadWaitingForSampling(currentThread, addr, count, positive);
			threadsWaitingForSampling.Add(threadWaitingForSampling);
			threadMan.hleBlockCurrentThread(SceKernelThreadInfo.JPCSP_WAIT_CTRL);

			if (log.DebugEnabled)
			{
				log.debug("hleCtrlReadBuffer waiting for sample");
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x6A2774F3, version = 150, checkInsideInterrupt = true) public int sceCtrlSetSamplingCycle(@CheckArgument("checkCycle") int newCycle)
		[HLEFunction(nid : 0x6A2774F3, version : 150, checkInsideInterrupt : true)]
		public virtual int sceCtrlSetSamplingCycle(int newCycle)
		{
			int oldCycle = cycle;
			this.cycle = newCycle;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCtrlSetSamplingCycle cycle={0:D} returning {1:D}", newCycle, oldCycle));
			}

			return oldCycle;
		}

		[HLEFunction(nid : 0x02BAAD91, version : 150, checkInsideInterrupt : true)]
		public virtual int sceCtrlGetSamplingCycle(TPointer32 cycleAddr)
		{
			cycleAddr.setValue(cycle);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1F4011E6, version = 150, checkInsideInterrupt = true) public int sceCtrlSetSamplingMode(@CheckArgument("checkMode") int newMode)
		[HLEFunction(nid : 0x1F4011E6, version : 150, checkInsideInterrupt : true)]
		public virtual int sceCtrlSetSamplingMode(int newMode)
		{
			int oldMode = mode;
			SamplingMode = newMode;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCtrlSetSamplingMode mode={0:D} returning {1:D}", newMode, oldMode));
			}

			return oldMode;
		}

		[HLEFunction(nid : 0xDA6B76A1, version : 150)]
		public virtual int sceCtrlGetSamplingMode(TPointer32 modeAddr)
		{
			modeAddr.setValue(mode);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x3A622550, version = 150) public int sceCtrlPeekBufferPositive(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=16, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer dataAddr, int numBuf)
		[HLEFunction(nid : 0x3A622550, version : 150)]
		public virtual int sceCtrlPeekBufferPositive(TPointer dataAddr, int numBuf)
		{
			return hleCtrlReadBufferImmediately(dataAddr.Address, numBuf, true, true);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC152080A, version = 150) public int sceCtrlPeekBufferNegative(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=16, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer dataAddr, int numBuf)
		[HLEFunction(nid : 0xC152080A, version : 150)]
		public virtual int sceCtrlPeekBufferNegative(TPointer dataAddr, int numBuf)
		{
			return hleCtrlReadBufferImmediately(dataAddr.Address, numBuf, false, true);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1F803938, version = 150, checkInsideInterrupt = true) public int sceCtrlReadBufferPositive(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=16, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer dataAddr, int numBuf)
		[HLEFunction(nid : 0x1F803938, version : 150, checkInsideInterrupt : true)]
		public virtual int sceCtrlReadBufferPositive(TPointer dataAddr, int numBuf)
		{
			return hleCtrlReadBuffer(dataAddr.Address, numBuf, true);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x60B81F86, version = 150, checkInsideInterrupt = true) public int sceCtrlReadBufferNegative(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=16, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer dataAddr, int numBuf)
		[HLEFunction(nid : 0x60B81F86, version : 150, checkInsideInterrupt : true)]
		public virtual int sceCtrlReadBufferNegative(TPointer dataAddr, int numBuf)
		{
			return hleCtrlReadBuffer(dataAddr.Address, numBuf, false);
		}

		[HLEFunction(nid : 0xB1D0E5CD, version : 150)]
		public virtual int sceCtrlPeekLatch(TPointer32 latchAddr)
		{
			latchAddr.setValue(0, uiMake);
			latchAddr.setValue(4, uiBreak);
			latchAddr.setValue(8, uiPress);
			latchAddr.setValue(12, uiRelease);

			return latchSamplingCount;
		}

		[HLEFunction(nid : 0x0B588501, version : 150)]
		public virtual int sceCtrlReadLatch(TPointer32 latchAddr)
		{
			latchAddr.setValue(0, uiMake);
			latchAddr.setValue(4, uiBreak);
			latchAddr.setValue(8, uiPress);
			latchAddr.setValue(12, uiRelease);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCtrlReadLatch uiMake=0x{0:X6}, uiBreak=0x{1:X6}, uiPress=0x{2:X6}, uiRelease=0x{3:X6}, returning {4:D}", uiMake, uiBreak, uiPress, uiRelease, latchSamplingCount));
			}

			uiMake = 0;
			uiBreak = 0;
			uiPress = 0;
			uiRelease = 0;

			int prevLatchSamplingCount = latchSamplingCount;
			latchSamplingCount = 0;

			return prevLatchSamplingCount;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA7144800, version = 150) public int sceCtrlSetIdleCancelThreshold(@CheckArgument("checkThreshold") int idlereset, @CheckArgument("checkThreshold") int idleback)
		[HLEFunction(nid : 0xA7144800, version : 150)]
		public virtual int sceCtrlSetIdleCancelThreshold(int idlereset, int idleback)
		{
			this.idlereset = idlereset;
			this.idleback = idleback;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x687660FA, version = 150) public int sceCtrlGetIdleCancelThreshold(@CanBeNull pspsharp.HLE.TPointer32 idleresetAddr, @CanBeNull pspsharp.HLE.TPointer32 idlebackAddr)
		[HLEFunction(nid : 0x687660FA, version : 150)]
		public virtual int sceCtrlGetIdleCancelThreshold(TPointer32 idleresetAddr, TPointer32 idlebackAddr)
		{
			idleresetAddr.setValue(idlereset);
			idlebackAddr.setValue(idleback);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x348D99D4, version = 150) public int sceCtrl_348D99D4()
		[HLEFunction(nid : 0x348D99D4, version : 150)]
		public virtual int sceCtrl_348D99D4()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAF5960F3, version = 150) public int sceCtrl_AF5960F3()
		[HLEFunction(nid : 0xAF5960F3, version : 150)]
		public virtual int sceCtrl_AF5960F3()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA68FD260, version = 150) public int sceCtrlClearRapidFire()
		[HLEFunction(nid : 0xA68FD260, version : 150)]
		public virtual int sceCtrlClearRapidFire()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6841BE1A, version = 150) public int sceCtrlSetRapidFire()
		[HLEFunction(nid : 0x6841BE1A, version : 150)]
		public virtual int sceCtrlSetRapidFire()
		{
			return 0;
		}

		[HLEFunction(nid : 0xC4AAD55F, version : 371)]
		public virtual int sceCtrlPeekBufferPositive371(TPointer dataAddr, int numBuf)
		{
			return hleCtrlReadBufferImmediately(dataAddr.Address, numBuf, true, true);
		}

		[HLEFunction(nid : 0x454455AC, version : 371)]
		public virtual int sceCtrlReadBufferPositive371(TPointer dataAddr, int numBuf)
		{
			return hleCtrlReadBuffer(dataAddr.Address, numBuf, true);
		}

		[HLEFunction(nid : 0xD073ECA4, version : 620)]
		public virtual int sceCtrlReadBufferPositive_620(TPointer dataAddr, int numBuf)
		{
			return hleCtrlReadBuffer(dataAddr.Address, numBuf, true);
		}

		[HLEFunction(nid : 0x9F3038AC, version : 639)]
		public virtual int sceCtrlReadBufferPositive_639(TPointer dataAddr, int numBuf)
		{
			return hleCtrlReadBuffer(dataAddr.Address, numBuf, true);
		}

		[HLEFunction(nid : 0xBE30CED0, version : 660)]
		public virtual int sceCtrlReadBufferPositive_660(TPointer dataAddr, int numBuf)
		{
			return hleCtrlReadBuffer(dataAddr.Address, numBuf, true);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF6E94EA3, version = 150, checkInsideInterrupt = true) public int sceCtrlSetSamplingMode_660(@CheckArgument("checkMode") int newMode)
		[HLEFunction(nid : 0xF6E94EA3, version : 150, checkInsideInterrupt : true)]
		public virtual int sceCtrlSetSamplingMode_660(int newMode)
		{
			return sceCtrlSetSamplingMode(newMode);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6C86AF22, version = 660) public int sceCtrl_driver_6C86AF22(int unknown)
		[HLEFunction(nid : 0x6C86AF22, version : 660)]
		public virtual int sceCtrl_driver_6C86AF22(int unknown)
		{
			return 0;
		}

		[HLEFunction(nid : 0x2BA616AF, version : 150)]
		public virtual int sceCtrlPeekBufferPositive_660(TPointer dataAddr, int numBuf)
		{
			return sceCtrlPeekBufferPositive371(dataAddr, numBuf);
		}

		[HLEFunction(nid : 0xF8EC18BD, version : 150)]
		public virtual int sceCtrlGetSamplingMode_660(TPointer32 modeAddr)
		{
			return sceCtrlGetSamplingMode(modeAddr);
		}
	}
}