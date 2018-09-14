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

	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerCpuBusFrequency : MMIOHandlerBase
	{
		private const int STATE_VERSION = 0;
		private int cpuFrequencyNumerator;
		private int cpuFrequencyDenominator;
		private int busFrequencyNumerator;
		private int busFrequencyDenominator;

		public MMIOHandlerCpuBusFrequency(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			cpuFrequencyNumerator = stream.readInt();
			cpuFrequencyDenominator = stream.readInt();
			busFrequencyNumerator = stream.readInt();
			busFrequencyDenominator = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(cpuFrequencyNumerator);
			stream.writeInt(cpuFrequencyDenominator);
			stream.writeInt(busFrequencyNumerator);
			stream.writeInt(busFrequencyDenominator);
			base.write(stream);
		}

		private int getFrequency(int numerator, int denominator)
		{
			return (numerator << 16) | denominator;
		}

		private int getNumerator(int value)
		{
			return (value >> 16) & 0x1FF;
		}

		private int getDenominator(int value)
		{
			return value & 0x1FF;
		}

		private int CpuFrequency
		{
			get
			{
				return getFrequency(cpuFrequencyNumerator, cpuFrequencyDenominator);
			}
			set
			{
				cpuFrequencyNumerator = getNumerator(value);
				cpuFrequencyDenominator = getDenominator(value);
			}
		}

		private int BusFrequency
		{
			get
			{
				return getFrequency(busFrequencyNumerator, busFrequencyDenominator);
			}
			set
			{
				busFrequencyNumerator = getNumerator(value);
				busFrequencyDenominator = getDenominator(value);
			}
		}



		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
					value = CpuFrequency;
					break;
				case 0x04:
					value = BusFrequency;
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
					CpuFrequency = value;
					break;
				case 0x04:
					BusFrequency = value;
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
			return string.Format("CPU frequency={0:D}/{1:D}, Bus frequency={2:D}/{3:D}", cpuFrequencyNumerator, cpuFrequencyDenominator, busFrequencyNumerator, busFrequencyDenominator);
		}
	}

}