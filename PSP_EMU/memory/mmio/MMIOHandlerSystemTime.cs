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
//	import static pspsharp.Allegrex.compiler.RuntimeContextLLE.clearInterrupt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.RuntimeContextLLE.triggerInterrupt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Emulator.getScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_THREAD0_INTR;

	using SystemTimeManager = pspsharp.HLE.kernel.managers.SystemTimeManager;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using Scheduler = pspsharp.scheduler.Scheduler;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerSystemTime : MMIOHandlerBase
	{
		private const int STATE_VERSION = 0;
		private long alarm;
		private TriggerAlarmInterruptAction triggerAlarmInterruptAction;

		private class TriggerAlarmInterruptAction : IAction
		{
			private readonly MMIOHandlerSystemTime outerInstance;

			public TriggerAlarmInterruptAction(MMIOHandlerSystemTime outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.triggerAlarmInterrupt();
			}
		}

		public MMIOHandlerSystemTime(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			Alarm = (int)(stream.readLong() + SystemTime);
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeLong(alarm - SystemTime);
			base.write(stream);
		}

		private int SystemTime
		{
			get
			{
				return (int) SystemTimeManager.SystemTime;
			}
		}

		private int Alarm
		{
			set
			{
				Scheduler scheduler = Scheduler;
    
				if (triggerAlarmInterruptAction == null)
				{
					triggerAlarmInterruptAction = new TriggerAlarmInterruptAction(this);
				}
				else
				{
					scheduler.removeAction(this.alarm, triggerAlarmInterruptAction);
					clearInterrupt(Processor, PSP_THREAD0_INTR);
				}
    
				this.alarm = value & 0xFFFFFFFFL;
    
				scheduler.addAction(this.alarm, triggerAlarmInterruptAction);
			}
		}

		private void triggerAlarmInterrupt()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Triggering PSP_THREAD0_INTR interrupt for {0}", this));
			}
			triggerInterrupt(Processor, PSP_THREAD0_INTR);
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
					value = SystemTime;
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
					// Value is set to 0 at boot time
					if (value != 0)
					{
						base.write32(address, value);
					}
					break;
				case 0x04:
					Alarm = value;
					break;
				case 0x08:
					// Value is set to 0x30 at boot time
					if (value != 0x30)
					{
						base.write32(address, value);
					}
					break;
				case 0x0C:
					// Value is set to 0x1 at boot time
					if (value != 0x1)
					{
						base.write32(address, value);
					}
					break;
				case 0x10:
					// Value is set to 0 at boot time
					if (value != 0)
					{
						base.write32(address, value);
					}
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
			return string.Format("MMIOHandlerSystemTime systemTime=0x{0:X8}, alarm=0x{1:X8}", SystemTime, alarm);
		}
	}

}