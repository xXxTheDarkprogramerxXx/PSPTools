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
namespace pspsharp.memory.mmio
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Emulator.getScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_VBLANK_INTR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.scheduler.Scheduler.getNow;

	using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using sceDisplay = pspsharp.HLE.modules.sceDisplay;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerDisplayController : MMIOHandlerBase
	{
		public static new Logger log = sceDisplay.log;
		private const int STATE_VERSION = 0;
		public const int BASE_ADDRESS = unchecked((int)0xBE740000);
		private static MMIOHandlerDisplayController instance;
		private long baseTimeMicros;
		private const int displaySyncMicros = (1000000 + 30) / 60;
		private const int numberDisplayRows = 286;
		private static readonly int rowSyncMicros = (displaySyncMicros + (numberDisplayRows / 2)) / numberDisplayRows;
		private TriggerVblankInterruptAction triggerVblankInterruptAction;
		// Used for debugging: limit the number of VBLANK interrupts being triggered
		private static int maxVblankInterrupts = 0;

		private class TriggerVblankInterruptAction : IAction
		{
			private readonly MMIOHandlerDisplayController outerInstance;

			public TriggerVblankInterruptAction(MMIOHandlerDisplayController outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.triggerVblankInterrupt();
			}
		}

		public static MMIOHandlerDisplayController Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MMIOHandlerDisplayController(BASE_ADDRESS);
				}
				return instance;
			}
		}

		private MMIOHandlerDisplayController(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			maxVblankInterrupts = stream.readInt();
			baseTimeMicros = Now - stream.readLong();
			base.read(stream);

			startVblankInterrupts();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(maxVblankInterrupts);
			stream.writeLong(TimeMicros);
			base.write(stream);
		}

		private long TimeMicros
		{
			get
			{
				return Now - baseTimeMicros;
			}
		}

		private int getDisplayRowSync(long time)
		{
			return getDisplaySync(time) + (((int)(time / rowSyncMicros) + 1) % numberDisplayRows);
		}

		private int getDisplaySync(long time)
		{
			return (int)(time / displaySyncMicros) * numberDisplayRows;
		}

		private long PreviousVblankSchedule
		{
			get
			{
				return TimeMicros / displaySyncMicros * displaySyncMicros + baseTimeMicros;
			}
		}

		private long NextVblankSchedule
		{
			get
			{
				return PreviousVblankSchedule + displaySyncMicros;
			}
		}

		public static int MaxVblankInterrupts
		{
			set
			{
				MMIOHandlerDisplayController.maxVblankInterrupts = value;
			}
		}

		private void scheduleNextVblankInterrupt()
		{
			if (maxVblankInterrupts == 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Skipping Vblank interrupt {0}", this));
				}
				return;
			}
			if (maxVblankInterrupts > 0)
			{
				maxVblankInterrupts--;
			}

			long schedule = NextVblankSchedule;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Scheduling next Vblank at 0x{0:X}, {1}", schedule, this));
			}
			Scheduler.addAction(schedule, triggerVblankInterruptAction);
		}

		public virtual void triggerVblankInterrupt()
		{
			scheduleNextVblankInterrupt();
			RuntimeContextLLE.triggerInterrupt(Processor, PSP_VBLANK_INTR);
		}

		private void startVblankInterrupts()
		{
			if (triggerVblankInterruptAction == null)
			{
				triggerVblankInterruptAction = new TriggerVblankInterruptAction(this);
				scheduleNextVblankInterrupt();
			}
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x04:
					value = getDisplayRowSync(TimeMicros);
					break;
				case 0x08:
					value = getDisplaySync(TimeMicros);
					break;
				case 0x20:
					value = 0;
					break;
				default:
					value = base.read32(address);
					break;
			}
			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read32(0x{1:X8}) returning 0x{2:X8}", Pc, address, value));
			}

			return value;
		}

		public override void write32(int address, int value)
		{
			switch (address - baseAddress)
			{
				case 0x00:
					break;
				case 0x04:
					baseTimeMicros = Now;
					startVblankInterrupts();
					break;
				case 0x0C:
					break;
				case 0x10:
					break;
				case 0x14:
					break;
				case 0x24:
					break;
				default:
					base.write32(address, value);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write32(0x{1:X8}, 0x{2:X8}) on {3}", Pc, address, value, this));
			}
		}

		public override string ToString()
		{
			return string.Format("MMIOHandlerDisplayController rowSync=0x{0:X}, displaySync=0x{1:X}, baseTime=0x{2:X}, now=0x{3:X}", getDisplayRowSync(TimeMicros), getDisplaySync(TimeMicros), baseTimeMicros, Now);
		}
	}

}