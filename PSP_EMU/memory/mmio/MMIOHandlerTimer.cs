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

	using SystemTimeManager = pspsharp.HLE.kernel.managers.SystemTimeManager;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerTimer : MMIOHandlerBase
	{
		private const int STATE_VERSION = 0;
		private const int TIMER_COUNTER_MASK = 0x003FFFFF;
		private const int TIMER_MODE_SHIFT = 22;
		// Indicates that a time-up handler is set for the specific timer
		public static readonly int TIMER_MODE_HANDLER_REGISTERED = 1 << 0;
		// Indicates that the timer is in use
		public static readonly int TIMER_MODE_IN_USE = 1 << 1;
		// Unknown timer mode
		public static readonly int TIMER_MODE_UNKNOWN = 1 << 9;
		public int timerMode;
		public int timerCounter;
		public int baseTime;
		public int prsclNumerator;
		public int prsclDenominator;

		public MMIOHandlerTimer(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			timerMode = stream.readInt();
			timerCounter = stream.readInt();
			baseTime = stream.readInt();
			prsclNumerator = stream.readInt();
			prsclDenominator = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(timerMode);
			stream.writeInt(timerCounter);
			stream.writeInt(baseTime);
			stream.writeInt(prsclNumerator);
			stream.writeInt(prsclDenominator);
			base.write(stream);
		}

		private int TimerData
		{
			get
			{
				return (timerCounter & TIMER_COUNTER_MASK) | (timerMode << TIMER_MODE_SHIFT);
			}
			set
			{
				timerCounter = value & TIMER_COUNTER_MASK;
				timerMode = (int)((uint)value >> TIMER_MODE_SHIFT);
			}
		}


		private int NowData
		{
			get
			{
				int systemTime = (int) SystemTimeManager.SystemTime;
				return (timerMode << TIMER_MODE_SHIFT) | (systemTime & TIMER_COUNTER_MASK);
			}
		}

		public override int read32(int address)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read32(0x{1:X8}) on {2}", Pc, address, this));
			}

			switch (address - baseAddress)
			{
				case 0:
					return TimerData;
				case 4:
					return baseTime;
				case 8:
					return prsclNumerator;
				case 12:
					return prsclDenominator;
				case 256:
					return NowData;
			}
			return base.read32(address);
		}

		public override void write32(int address, int value)
		{
			switch (address - baseAddress)
			{
				case 0:
					TimerData = value;
					break;
				case 4:
					baseTime = value;
					break;
				case 8:
					prsclNumerator = value;
					break;
				case 12:
					prsclDenominator = value;
					break;
				case 256:
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
			return string.Format("Timer 0x{0:X8}: timerMode=0x{1:X3}, timerCounter=0x{2:X6}, baseTime=0x{3:X8}, prsclNumerator=0x{4:X}, prsckDenominator=0x{5:X}", baseAddress, timerMode, timerCounter, baseTime, prsclNumerator, prsclDenominator);
		}
	}

}