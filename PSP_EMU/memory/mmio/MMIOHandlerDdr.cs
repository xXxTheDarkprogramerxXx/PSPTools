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

	using Logger = org.apache.log4j.Logger;

	using IAction = pspsharp.HLE.kernel.types.IAction;
	using sceDdr = pspsharp.HLE.modules.sceDdr;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerDdr : MMIOHandlerBase
	{
		public static new Logger log = sceDdr.log;
		private const int STATE_VERSION = 0;
		public const int BASE_ADDRESS = unchecked((int)0xBD000000);
		public const int DDR_FLUSH_DMAC = 4;
		private static MMIOHandlerDdr instance;
		private int unknown40;
		private readonly IAction[] flushActions = new IAction[16];
		private readonly bool[] flushDone = new bool[16];

		public static MMIOHandlerDdr Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MMIOHandlerDdr(BASE_ADDRESS);
				}
				return instance;
			}
		}

		private MMIOHandlerDdr(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			unknown40 = stream.readInt();
			stream.readBooleans(flushDone);
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(unknown40);
			stream.writeBooleans(flushDone);
			base.write(stream);
		}

		public virtual bool checkAndClearFlushDone(int value)
		{
			bool check = flushDone[value];
			flushDone[value] = false;

			return check;
		}

		public virtual void setFlushAction(int value, IAction action)
		{
			flushActions[value] = action;
		}

		private void doFlush(int value)
		{
			flushDone[value] = true;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("MMIOHandlerDdr.doFlush 0x{0:X1}", value));
			}

			if (flushActions[value] != null)
			{
				flushActions[value].execute();
			}
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x04:
					value = 0;
					break;
				case 0x30:
					value = 0;
					break; // Unknown, used during sceDdrChangePllClock()
					goto case 0x40;
				case 0x40:
					value = unknown40;
					unknown40 ^= 0x100;
					break; // Unknown, used during sceDdrChangePllClock()
					goto default;
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
				case 0x04:
					doFlush(value);
					break;
				case 0x30:
					break; // Unknown, used during sceDdrChangePllClock()
					goto case 0x34;
				case 0x34:
					break; // Unknown, used during sceDdrChangePllClock()
					goto case 0x40;
				case 0x40:
					break; // Unknown, used during sceDdrChangePllClock()
					goto case 0x44;
				case 0x44:
					break; // Unknown, used during sceDdrChangePllClock()
					goto default;
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