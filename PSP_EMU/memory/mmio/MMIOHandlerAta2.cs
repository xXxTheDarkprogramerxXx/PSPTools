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

	using sceAta = pspsharp.HLE.modules.sceAta;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerAta2 : MMIOHandlerBase
	{
		public static new Logger log = sceAta.log;
		private const int STATE_VERSION = 0;

		public MMIOHandlerAta2(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			base.write(stream);
		}

		private void writeReset(int value)
		{
			if (value != 0)
			{
				MMIOHandlerAta.Instance.reset();
			}
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
					value = 0x00010033;
					break; // Unknown value
					goto case 0x34;
				case 0x34:
					value = 0;
					break; // Unknown value
					goto case 0x40;
				case 0x40:
					value = 0;
					break; // Unknown value, flag 0x2 is being tested
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

		private void writeUnknown44(int value)
		{
			int unknownValue = (int)((uint)(~value) >> 16);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("writeUnknown44 0x{0:X4}", unknownValue));
			}
		}

		public override void write32(int address, int value)
		{
			switch (address - baseAddress)
			{
				case 0x04:
					if (value != 0x04028002)
					{
						base.write32(address, value);
					}
					break; // Unknown value
					goto case 0x10;
				case 0x10:
					writeReset(value);
					break;
				case 0x1C:
					if (value != 0x00020A0C)
					{
						base.write32(address, value);
					}
					break; // Unknown value
					goto case 0x14;
				case 0x14:
					break; // Unknown value
					goto case 0x34;
				case 0x34:
					if (value != 0)
					{
						base.write32(address, value);
					}
					break; // Unknown value
					goto case 0x38;
				case 0x38:
					if (value != 0x00010100)
					{
						base.write32(address, value);
					}
					break; // Unknown value
					goto case 0x40;
				case 0x40:
					if (value != 1)
					{
						base.write32(address, value);
					}
					break; // Unknown value
					goto case 0x44;
				case 0x44:
					writeUnknown44(value);
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