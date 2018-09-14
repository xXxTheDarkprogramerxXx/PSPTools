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

	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerGeEdram : MMIOHandlerBase
	{
		public static new Logger log = MMIOHandlerGe.log;
		private const int STATE_VERSION = 0;
		private int unknown00;
		private int unknown20;
		private int unknown30;
		private int unknown40;
		private int unknown70;
		private int unknown80;
		private int unknown90;

		public MMIOHandlerGeEdram(int baseAddress) : base(baseAddress)
		{

			unknown00 = 0x00012223;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			unknown00 = stream.readInt();
			unknown20 = stream.readInt();
			unknown30 = stream.readInt();
			unknown40 = stream.readInt();
			unknown70 = stream.readInt();
			unknown80 = stream.readInt();
			unknown90 = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(unknown00);
			stream.writeInt(unknown20);
			stream.writeInt(unknown30);
			stream.writeInt(unknown40);
			stream.writeInt(unknown70);
			stream.writeInt(unknown80);
			stream.writeInt(unknown90);
			base.write(stream);
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
					value = unknown00;
					break;
				case 0x10:
					value = 0;
					break;
				case 0x20:
					value = unknown20;
					break;
				case 0x30:
					value = unknown30;
					break;
				case 0x40:
					value = unknown40;
					break;
				case 0x70:
					value = unknown70;
					break;
				case 0x80:
					value = unknown80;
					break;
				case 0x90:
					value = unknown90;
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
					unknown00 = value;
					break;
				case 0x10:
					if (value != 0 && value != 1 && value != 2)
					{
						base.write32(address, value);
					};
					break;
				case 0x20:
					unknown20 = value;
					break;
				case 0x30:
					unknown30 = value;
					break;
				case 0x40:
					unknown40 = value;
					break;
				case 0x70:
					unknown70 = value;
					break;
				case 0x80:
					unknown80 = value;
					break;
				case 0x90:
					unknown90 = value;
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