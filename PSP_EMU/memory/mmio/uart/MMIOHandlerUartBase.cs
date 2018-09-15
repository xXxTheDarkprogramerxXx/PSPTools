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
namespace pspsharp.memory.mmio.uart
{

	//using Logger = org.apache.log4j.Logger;

	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerUartBase : MMIOHandlerBase
	{
		public static new Logger log = Logger.getLogger("uart");
		private const int STATE_VERSION = 0;
		public const int SIZE_OF = 0x48;
		public const int UART_STATUS_RXEMPTY = 0x10;
		public const int UART_STATUS_TXFULL = 0x20;
		private int data;
		private int status;
		private long baudrateDivisor;
		private int control;
		private int unknown04;
		private int unknown30;
		private int unknown34;
		private int unknown38;
		private int interrupt;

		public MMIOHandlerUartBase(int baseAddress) : base(baseAddress)
		{

			status = UART_STATUS_RXEMPTY;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			data = stream.readInt();
			status = stream.readInt();
			baudrateDivisor = stream.readLong();
			control = stream.readInt();
			unknown04 = stream.readInt();
			unknown30 = stream.readInt();
			unknown34 = stream.readInt();
			unknown34 = stream.readInt();
			interrupt = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(data);
			stream.writeInt(status);
			stream.writeLong(baudrateDivisor);
			stream.writeInt(control);
			stream.writeInt(unknown04);
			stream.writeInt(unknown30);
			stream.writeInt(unknown34);
			stream.writeInt(unknown38);
			stream.writeInt(interrupt);
			base.write(stream);
		}

		private void clearInterrupt(int mask)
		{
			interrupt &= ~mask;
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
					value = data;
					break;
				case 0x04:
					value = unknown04;
					break;
				case 0x18:
					value = status;
					break;
				case 0x2C:
					value = control;
					break;
				case 0x30:
					value = unknown30;
					break;
				case 0x34:
					value = unknown34;
					break;
				case 0x38:
					value = unknown38;
					break;
				case 0x44:
					value = interrupt;
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
					data = value;
					break;
				case 0x04:
					unknown04 = value;
					break;
				case 0x18:
					status = value;
					break;
				case 0x24:
					baudrateDivisor = (baudrateDivisor & 0x3FL) | (((long) value) << 6);
					break;
				case 0x28:
					baudrateDivisor = (baudrateDivisor & ~0x3FL) | (value & 0x3F);
					break;
				case 0x2C:
					control = value;
					break;
				case 0x30:
					unknown30 = value;
					break;
				case 0x34:
					unknown34 = value;
					break;
				case 0x38:
					unknown38 = value;
					break;
				case 0x44:
					clearInterrupt(value);
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
	}

}