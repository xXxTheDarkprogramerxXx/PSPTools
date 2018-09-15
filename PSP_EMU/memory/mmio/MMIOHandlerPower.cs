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

	//using Logger = org.apache.log4j.Logger;

	using scePower = pspsharp.HLE.modules.scePower;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerPower : MMIOHandlerBase
	{
		public static new Logger log = scePower.log;
		private const int STATE_VERSION = 0;
		private readonly PowerObject unknown1 = new PowerObject();
		private readonly PowerObject unknown2 = new PowerObject();
		private readonly PowerObject unknown3 = new PowerObject();

		private class PowerObject
		{
			internal const int STATE_VERSION = 0;
			public int unknown00;
			public int unknown04;
			public int unknown08;
			public int unknown0C;
			public int unknown10;
			public int unknown14;
			public int unknown18;
			public int unknown1C;

			public virtual int read32(int offset)
			{
				int value = 0;
				switch (offset)
				{
					case 0x00:
						value = unknown00;
						break;
					case 0x04:
						value = unknown04;
						break;
					case 0x08:
						value = unknown08;
						break;
					case 0x0C:
						value = unknown0C;
						break;
					case 0x10:
						value = unknown10;
						break;
					case 0x14:
						value = unknown14;
						break;
					case 0x18:
						value = unknown18;
						break;
					case 0x1C:
						value = unknown1C;
						break;
				}

				return value;
			}

			public virtual void write32(int offset, int value)
			{
				switch (offset)
				{
					case 0x00:
						unknown00 = value;
						break;
					case 0x04:
						unknown04 = value;
						break;
					case 0x08:
						unknown08 = value;
						break;
					case 0x0C:
						unknown0C = value;
						break;
					case 0x10:
						unknown10 = value;
						break;
					case 0x14:
						unknown14 = value;
						break;
					case 0x18:
						unknown18 = value;
						break;
					case 0x1C:
						unknown1C = value;
						break;
				}
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
			public virtual void read(StateInputStream stream)
			{
				stream.readVersion(STATE_VERSION);
				unknown00 = stream.readInt();
				unknown04 = stream.readInt();
				unknown08 = stream.readInt();
				unknown0C = stream.readInt();
				unknown10 = stream.readInt();
				unknown14 = stream.readInt();
				unknown18 = stream.readInt();
				unknown1C = stream.readInt();
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
			public virtual void write(StateOutputStream stream)
			{
				stream.writeVersion(STATE_VERSION);
				stream.writeInt(unknown00);
				stream.writeInt(unknown04);
				stream.writeInt(unknown08);
				stream.writeInt(unknown0C);
				stream.writeInt(unknown10);
				stream.writeInt(unknown14);
				stream.writeInt(unknown18);
				stream.writeInt(unknown1C);
			}
		}

		public MMIOHandlerPower(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			unknown1.read(stream);
			unknown2.read(stream);
			unknown3.read(stream);
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			unknown1.write(stream);
			unknown2.write(stream);
			unknown3.write(stream);
			base.write(stream);
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
				case 0x04:
				case 0x08:
				case 0x0C:
				case 0x10:
				case 0x14:
				case 0x18:
				case 0x1C:
					value = unknown1.read32(address - baseAddress);
					break;
				case 0x20:
				case 0x24:
				case 0x28:
				case 0x2C:
				case 0x30:
				case 0x34:
				case 0x38:
				case 0x3C:
					value = unknown2.read32(address - baseAddress - 0x20);
					break;
				case 0x40:
				case 0x44:
				case 0x48:
				case 0x4C:
				case 0x50:
				case 0x54:
				case 0x58:
				case 0x5C:
					value = unknown3.read32(address - baseAddress - 0x40);
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
				case 0x04:
				case 0x08:
				case 0x0C:
				case 0x10:
				case 0x14:
				case 0x18:
				case 0x1C:
					unknown1.write32(address - baseAddress, value);
					break;
				case 0x20:
				case 0x24:
				case 0x28:
				case 0x2C:
				case 0x30:
				case 0x34:
				case 0x38:
				case 0x3C:
					unknown2.write32(address - baseAddress - 0x20, value);
					break;
				case 0x40:
				case 0x44:
				case 0x48:
				case 0x4C:
				case 0x50:
				case 0x54:
				case 0x58:
				case 0x5C:
					unknown3.write32(address - baseAddress - 0x40, value);
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