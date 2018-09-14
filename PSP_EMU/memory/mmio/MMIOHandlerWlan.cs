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
//	import static pspsharp.util.Utilities.endianSwap16;


	using Logger = org.apache.log4j.Logger;

	using sceWlan = pspsharp.HLE.modules.sceWlan;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;
	using Utilities = pspsharp.util.Utilities;

	public class MMIOHandlerWlan : MMIOHandlerBase
	{
		public static new Logger log = sceWlan.log;
		private const int STATE_VERSION = 0;
		private int command;
		private object dmaLock = new object();
		private readonly int[] data = new int[4096];
		private int unknown38;
		private int unknown3C;
		private int unknown40;
		private int index;
		private int totalLength;
		private int currentLength;

		public MMIOHandlerWlan(int baseAddress) : base(baseAddress)
		{

			unknown38 = 0x4000 | 0x2000 | 0x1000 | 0x0002;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			command = stream.readInt();
			stream.readInts(data);
			unknown38 = stream.readInt();
			unknown3C = stream.readInt();
			unknown40 = stream.readInt();
			index = stream.readInt();
			totalLength = stream.readInt();
			currentLength = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(command);
			stream.writeInts(data);
			stream.writeInt(unknown38);
			stream.writeInt(unknown3C);
			stream.writeInt(unknown40);
			stream.writeInt(index);
			stream.writeInt(totalLength);
			stream.writeInt(currentLength);
			base.write(stream);
		}

		private int Unknown3C
		{
			set
			{
				unknown3C = value & ~0x200;
			}
		}

		private int CommandCode
		{
			get
			{
				return (int)((uint)command >> 12);
			}
		}

		private int CommandLength
		{
			get
			{
				return command & 0xFFF;
			}
		}

		private int Command
		{
			set
			{
				lock (dmaLock)
				{
					command = value;
					index = 0;
					unknown38 &= unchecked((int)0xFFFFFFF0);
    
					switch (command)
					{
						case 0x7001:
							setData8(0, 0x80);
							totalLength = CommandLength;
							currentLength = 0;
							break;
						case 0x4001:
						case 0x4002:
						case 0x4004:
							totalLength = CommandLength;
							currentLength = 0;
							break;
						case 0x5040:
							clearData(CommandLength);
							// Possible values:
							// 0x0011 0x0001 0x0001
							// 0x0011 0x0001 0x1B18
							// 0x0011 0x0002 0x1B11
							// 0x0011 0x0002 0x0B11
							setData32(0, swap32(0x00010011));
							setData32(4, swap32(0x00001B18));
    
							setData32(8, swap32(0x12340001));
							setData32(12, swap32(0x00000040));
							break;
						case 0x5200:
							clearData(CommandLength);
							break;
						case 0x8004:
							break;
						case 0x9007:
							break;
						case 0xB001:
							break;
						default:
							log.error(string.Format("setCommand unknown command=0x{0:X}", command));
							Arrays.fill(data, 0);
							break;
					}
				}
			}
		}

		private int readData8()
		{
			while (true)
			{
				lock (dmaLock)
				{
					int commandCode = CommandCode;
					if (commandCode == 0x5 || commandCode == 0x7 || commandCode == 0x4)
					{
						if ((unknown38 & 0x3) != 0x2)
						{
							break;
						}
					}
				}
				Utilities.sleep(100);
			}

			int value;
			lock (dmaLock)
			{
				value = data[index++] & 0xFF;

				if (index == CommandLength)
				{
					currentLength += index;

					unknown38 |= 0x0002;
					if (currentLength >= totalLength)
					{
						unknown38 |= 0x0001;
					}
				}
			}

			return value;
		}

		private int readData16()
		{
			return readData8() | (readData8() << 8);
		}

		private int readData32()
		{
			return readData8() | (readData8() << 8) | (readData8() << 16) | (readData8() << 24);
		}

		private int getData16(int offset)
		{
			return data[offset] | (data[offset + 1] << 8);
		}

		private int getData32(int offset)
		{
			return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
		}

		private static int swap32(int value)
		{
			return ((int)((uint)value >> 16)) | (value << 16);
		}

		private void setData8(int offset, int value)
		{
			data[offset] = value & 0xFF;
		}

		private void setData16(int offset, int value)
		{
			setData8(offset++, value);
			setData8(offset, value >> 8);
		}

		private void setData32(int offset, int value)
		{
			setData8(offset++, value);
			setData8(offset++, value >> 8);
			setData8(offset++, value >> 16);
			setData8(offset, value >> 24);
		}

		private void clearData(int offset, int length)
		{
			for (int i = 0; i < length; i++)
			{
				setData8(offset++, 0);
			}
		}

		private void clearData(int length)
		{
			clearData(0, length);
		}

		private void writeData8(int value)
		{
			data[index++] = value & 0xFF;
		}

		private void writeData32(int value)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("writeData32 command=0x{0:X}, data=0x{1:X8}, index=0x{2:X}", command, value, index));
			}
			writeData8(value);
			writeData8(value >> 8);
			writeData8(value >> 16);
			writeData8((int)((uint)value >> 24));

			switch (command)
			{
				case 0x8004:
					if (index >= 8)
					{
						switch (getData32(0))
						{
							case 0x122:
							case 0x123:
							case 0x124:
							case 0x127:
								clearData(8);
								setData8(0, 0x00); // Returning 1 unknown byte
								break;
							case 0x125:
								clearData(8);
								setData8(0, 0x01); // Returning 1 unknown byte
								break;
							case 0x154:
								clearData(8);
								setData8(0, 0xFF); // Returning 1 unknown byte
								break;
							case 0x24A:
								clearData(8);
								setData16(0, endianSwap16(0xC00)); // Returning unknown 16 bit value (it is a size for next request, must be between 4 and 0xC00)
								break;
							case 0x44E:
								clearData(8);
								setData32(0, 0x100F0000); // Returning unknown 32 bit value
								// Possible values are 0xNNNNxxxx, with NNNN being one of the following:
								// 0x1002
								// 0x1008
								// 0x100D
								// 0x100E
								// 0x100F
								// 0x1020
								// 0x1040
								// 0x1080
								// 0x10A0
								// 0x10C0
								break;
							case 0x1100000:
							case 0x1250000:
							case 0x1260000:
							case 0x1540000:
							case 0x15A0000:
							case 0x15C0000:
							case 0x15E0000:
								clearData(8);
								break;
							default:
								log.error(string.Format("writeData32 unknown command=0x{0:X}, data[0]=0x{1:X8}, data[1]=0x{2:X8}", command, getData32(0), getData32(4)));
								Arrays.fill(data, 0);
								break;
						}
					}
					break;
				case 0x9007:
					if (index >= 8)
					{
						totalLength = endianSwap16(getData16(1));
						if (log.DebugEnabled)
						{
							log.debug(string.Format("writeData32 command=0x{0:X}, totalLength=0x{1:X}", command, totalLength));
						}
						currentLength = 0;
						unknown38 |= 0x0002;
						index = 0;
					}
					break;
				case 0xB001:
					if (index >= 8)
					{
						switch (getData32(0))
						{
							default:
								log.error(string.Format("writeData32 unknown command=0x{0:X}, data[0]=0x{1:X8}, data[1]=0x{2:X8}", command, getData32(0), getData32(4)));
								Arrays.fill(data, 0);
								break;
						}
					}
					break;
			}
		}

		public override int read16(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x34:
					value = readData16();
					break;
				default:
					value = base.read16(address);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read16(0x{1:X8}) returning 0x{2:X4}", Pc, address, value));
			}

			return value;
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x30:
					value = command;
					break;
				case 0x34:
					value = readData32();
					break;
				case 0x38:
					value = unknown38;
					break;
				case 0x3C:
					value = unknown3C;
					break;
				case 0x40:
					value = unknown40;
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
				case 0x30:
					Command = value;
					break;
				case 0x34:
					writeData32(value);
					break;
				case 0x3C:
					Unknown3C = value;
					break;
				case 0x40:
					unknown40 = value;
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