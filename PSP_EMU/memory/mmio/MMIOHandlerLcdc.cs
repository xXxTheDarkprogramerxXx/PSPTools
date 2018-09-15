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

	using sceLcdc = pspsharp.HLE.modules.sceLcdc;
	using Screen = pspsharp.hardware.Screen;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	/// <summary>
	/// LCD Sharp LQ043
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class MMIOHandlerLcdc : MMIOHandlerBase
	{
		public static new Logger log = sceLcdc.log;
		private const int STATE_VERSION = 0;
		private readonly LcdcController controller1 = new LcdcController();
		private readonly LcdcController controller2 = new LcdcController();

		private class LcdcController
		{
			internal const int STATE_VERSION = 0;
			// The register names are based on https://github.com/uofw/upspd/wiki/Hardware-registers
			public int enable;
			public int synchronizationDifference;
			public int unknown008;
			//
			// The Hsync period is divided in back porch, resolution, front porch:
			//
			// <--------------------Hsync period------------------->
			// <-xBackPorch-><-----xResolution------><-xFrontPorch->
			//
			// The Vsync period is divided in back porch, resolution and front porch:
			//
			//     ^              ^
			//     | yBackPorch   |
			//     v              |
			//     ^              |
			//     |              |
			//     | yResolution  | Vsync period
			//     |              |
			//     v              |
			//     ^              |
			//     | yFrontPorch  |
			//     v              v
			public int xBackPorch;
			public int xPulseWidth;
			public int xFrontPorch;
			public int xResolution;
			public int yBackPorch;
			public int yPulseWidth;
			public int yFrontPorch;
			public int yResolution;
			public int yShift;
			public int xShift;
			public int scaledXResolution;
			public int scaledYResolution;

			public virtual void reset()
			{
				enable = 0;
				synchronizationDifference = 0;
				unknown008 = 0;

				xPulseWidth = 41;
				xBackPorch = 2;
				xFrontPorch = 2;
				xResolution = Screen.width;

				yPulseWidth = 10;
				yBackPorch = 2;
				yFrontPorch = 2;
				yResolution = Screen.height;

				yShift = 0x00;
				xShift = 0x00;
				scaledXResolution = Screen.width;
				scaledYResolution = Screen.height;
			}

			public virtual int read32(int offset)
			{
				int value = 0;

				switch (offset)
				{
					case 0x000:
						value = enable;
						break;
					case 0x004:
						value = synchronizationDifference;
						break;
					case 0x008:
						value = unknown008;
						break;
					case 0x010:
						value = xPulseWidth;
						break;
					case 0x014:
						value = xBackPorch;
						break;
					case 0x018:
						value = xFrontPorch;
						break;
					case 0x01C:
						value = xResolution;
						break;
					case 0x020:
						value = yBackPorch;
						break;
					case 0x024:
						value = yFrontPorch;
						break;
					case 0x028:
						value = yPulseWidth;
						break;
					case 0x02C:
						value = yResolution;
						break;
					case 0x040:
						value = yShift;
						break;
					case 0x044:
						value = xShift;
						break;
					case 0x048:
						value = scaledXResolution;
						break;
					case 0x04C:
						value = scaledYResolution;
						break;
					case 0x050:
						value = 0x01;
						break;
				}

				return value;
			}

			public virtual void write32(int offset, int value)
			{
				switch (offset)
				{
					case 0x000:
						enable = value;
						break;
					case 0x004:
						synchronizationDifference = value;
						break;
					case 0x008:
						unknown008 = value;
						break;
					case 0x010:
						xPulseWidth = value;
						break;
					case 0x014:
						xBackPorch = value;
						break;
					case 0x018:
						xFrontPorch = value;
						break;
					case 0x01C:
						xResolution = value;
						break;
					case 0x020:
						yBackPorch = value;
						break;
					case 0x024:
						yFrontPorch = value;
						break;
					case 0x028:
						yPulseWidth = value;
						break;
					case 0x02C:
						yResolution = value;
						break;
					case 0x040:
						yShift = value;
						break;
					case 0x044:
						xShift = value;
						break;
					case 0x048:
						scaledXResolution = value;
						break;
					case 0x04C:
						scaledYResolution = value;
						break;
				}
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
			public virtual void read(StateInputStream stream)
			{
				stream.readVersion(STATE_VERSION);
				enable = stream.readInt();
				synchronizationDifference = stream.readInt();
				unknown008 = stream.readInt();
				xPulseWidth = stream.readInt();
				xBackPorch = stream.readInt();
				xFrontPorch = stream.readInt();
				xResolution = stream.readInt();
				yBackPorch = stream.readInt();
				yFrontPorch = stream.readInt();
				yPulseWidth = stream.readInt();
				yResolution = stream.readInt();
				yShift = stream.readInt();
				xShift = stream.readInt();
				scaledXResolution = stream.readInt();
				scaledYResolution = stream.readInt();
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
			public virtual void write(StateOutputStream stream)
			{
				stream.writeVersion(STATE_VERSION);
				stream.writeInt(enable);
				stream.writeInt(synchronizationDifference);
				stream.writeInt(unknown008);
				stream.writeInt(xPulseWidth);
				stream.writeInt(xBackPorch);
				stream.writeInt(xFrontPorch);
				stream.writeInt(xResolution);
				stream.writeInt(yBackPorch);
				stream.writeInt(yFrontPorch);
				stream.writeInt(yPulseWidth);
				stream.writeInt(yResolution);
				stream.writeInt(yShift);
				stream.writeInt(xShift);
				stream.writeInt(scaledXResolution);
				stream.writeInt(scaledYResolution);
			}
		}

		public MMIOHandlerLcdc(int baseAddress) : base(baseAddress)
		{

			reset();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			controller1.read(stream);
			controller2.read(stream);
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			controller1.write(stream);
			controller2.write(stream);
			base.write(stream);
		}

		private void reset()
		{
			controller1.reset();
			controller2.reset();
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x000:
				case 0x004:
				case 0x008:
				case 0x010:
				case 0x014:
				case 0x018:
				case 0x01C:
				case 0x020:
				case 0x024:
				case 0x028:
				case 0x02C:
				case 0x040:
				case 0x044:
				case 0x048:
				case 0x04C:
				case 0x050:
					value = controller1.read32(address - baseAddress);
					break;
				case 0x100:
				case 0x104:
				case 0x108:
				case 0x110:
				case 0x114:
				case 0x118:
				case 0x11C:
				case 0x120:
				case 0x124:
				case 0x128:
				case 0x12C:
				case 0x140:
				case 0x144:
				case 0x148:
				case 0x14C:
				case 0x150:
					value = controller2.read32(address - baseAddress - 0x100);
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
				case 0x000:
				case 0x004:
				case 0x008:
				case 0x010:
				case 0x014:
				case 0x018:
				case 0x01C:
				case 0x020:
				case 0x024:
				case 0x028:
				case 0x02C:
				case 0x040:
				case 0x044:
				case 0x048:
				case 0x04C:
					controller1.write32(address - baseAddress, value);
					break;
				case 0x100:
				case 0x104:
				case 0x108:
				case 0x110:
				case 0x114:
				case 0x118:
				case 0x11C:
				case 0x120:
				case 0x124:
				case 0x128:
				case 0x12C:
				case 0x140:
				case 0x144:
				case 0x148:
				case 0x14C:
					controller2.write32(address - baseAddress - 0x100, value);
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
			return string.Format("MMIOHandlerLcdc");
		}
	}

}