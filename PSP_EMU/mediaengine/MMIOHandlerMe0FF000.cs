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
namespace pspsharp.mediaengine
{

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;
	using Utilities = pspsharp.util.Utilities;

	public class MMIOHandlerMe0FF000 : MMIOHandlerMeBase
	{
		private const int STATE_VERSION = 0;
		private int status;
		private int power;
		private int command;
		private int unknown10;
		private int unknown14;
		private int unknown18;
		private int unknown1C;
		private int unknown20;
		private int unknown24;
		private int unknown28;
		private int unknown2C;

		public MMIOHandlerMe0FF000(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			status = stream.readInt();
			power = stream.readInt();
			command = stream.readInt();
			unknown10 = stream.readInt();
			unknown14 = stream.readInt();
			unknown18 = stream.readInt();
			unknown1C = stream.readInt();
			unknown20 = stream.readInt();
			unknown24 = stream.readInt();
			unknown28 = stream.readInt();
			unknown2C = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(status);
			stream.writeInt(power);
			stream.writeInt(command);
			stream.writeInt(unknown10);
			stream.writeInt(unknown14);
			stream.writeInt(unknown18);
			stream.writeInt(unknown1C);
			stream.writeInt(unknown20);
			stream.writeInt(unknown24);
			stream.writeInt(unknown28);
			stream.writeInt(unknown2C);
			base.write(stream);
		}

		private int Command
		{
			set
			{
				this.command = value;
    
				switch (value)
				{
					case 0x02:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}", value, status));
						}
						break;
					case 0x03:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}", value, status));
						}
						break;
					case 0x04:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}", value, status, unknown10, unknown14, unknown18));
						}
						break;
					case 0x05:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}", value, status, unknown10));
							Console.WriteLine(Utilities.getMemoryDump(Memory, unknown10, 0x1A4, 4, 16));
						}
						break;
					case 0x08:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}", value, status));
						}
						break;
					case 0x18:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}", value, status));
						}
						break;
					case 0x1D:
						// Used at startup
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, power=0x{2:X}, unknown10=0x{3:X8}", value, status, power, unknown10));
							Console.WriteLine(string.Format("unknown10: {0}", Utilities.getMemoryDump(Memory, unknown10, 0x2000, 4, 16)));
						}
						break;
					case 0x20:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown14=0x{2:X}, unknown18=0x{3:X}, unknown1C=0x{4:X}, unknown2C=0x{5:X}", value, status, unknown14, unknown18, unknown1C, unknown2C));
						}
						break;
					case 0x21:
						// Used during decodeSpectrum
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown14=0x{2:X}, unknown18=0x{3:X}, unknown1C=0x{4:X}", value, status, unknown14, unknown18, unknown1C));
						}
						break;
					case 0x28:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown14=0x{2:X}, unknown18=0x{3:X}", value, status, unknown14, unknown18));
						}
						break;
					case 0x40:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}", value, status, unknown10, unknown14, unknown18));
							Console.WriteLine(string.Format("unknown10: {0}", Utilities.getMemoryDump(Memory, unknown10, (unknown14 + 1) << 2), 4, 16));
						}
						break;
					case 0x42:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}, unknown1C=0x{5:X}", value, status, unknown10, unknown14, unknown18, unknown1C));
						}
						break;
					case 0x45:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}", value, status, unknown10, unknown14, unknown18));
						}
						break;
					case 0x48:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}", value, status, unknown10, unknown14, unknown18));
						}
						break;
					case 0x4D:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}", value, status, unknown10, unknown14, unknown18));
						}
						break;
					case 0x50:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}, unknown20=0x{5:X}, unknown24=0x{6:X}, unknown28=0x{7:X}", value, status, unknown10, unknown14, unknown18, unknown20, unknown24, unknown28));
						}
						break;
					case 0x52:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}, unknown20=0x{5:X}, unknown24=0x{6:X}, unknown28=0x{7:X}", value, status, unknown10, unknown14, unknown18, unknown20, unknown24, unknown28));
						}
						break;
					case 0x54:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}, unknown20=0x{5:X}, unknown24=0x{6:X}, unknown28=0x{7:X}", value, status, unknown10, unknown14, unknown18, unknown20, unknown24, unknown28));
						}
						break;
					case 0x58:
						if ((unknown14 & 0xFFFF0000) != 0)
						{
							Console.WriteLine(string.Format("Unknown Length 0x{0:X} in command 0x{1:X}, status=0x{2:X}, unknown10=0x{3:X8}, unknown14=0x{4:X}, unknown18=0x{5:X}, unknown20=0x{6:X}, unknown24=0x{7:X}, unknown28=0x{8:X}", unknown14, value, status, unknown10, unknown14, unknown18, unknown20, unknown24, unknown28));
						}
						else if (unknown18 != 0x9C00 || unknown20 != 0 || unknown24 != 0xFFFE || unknown28 != 0)
						{
							Console.WriteLine(string.Format("Unknown parameters in command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}, unknown20=0x{5:X}, unknown24=0x{6:X}, unknown28=0x{7:X}", value, status, unknown10, unknown14, unknown18, unknown20, unknown24, unknown28));
						}
						else
						{
							IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(Memory, unknown10, (unknown14 + 1) << 2, 4);
							for (int i = 0; i <= unknown14; i++)
							{
								memoryWriter.writeNext(0);
							}
							memoryWriter.flush();
						}
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}, unknown20=0x{5:X}, unknown24=0x{6:X}, unknown28=0x{7:X}", value, status, unknown10, unknown14, unknown18, unknown20, unknown24, unknown28));
						}
						break;
					case 0x5A:
						if (unknown18 != 0x548 || unknown20 != 4 || unknown24 != 3 || unknown28 != 0x10800)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}, unknown20=0x{5:X}, unknown24=0x{6:X}, unknown28=0x{7:X}", value, status, unknown10, unknown14, unknown18, unknown20, unknown24, unknown28));
						}
						else if (unknown14 == 0x07FF0000)
						{
							// Interlaced left and right samples
							int numberOfSamples = unknown28 & 0xFFFF;
							Memory mem = Memory;
							for (int i = 0; i < numberOfSamples; i++)
							{
								mem.write16(unknown10 + i * 4, (short) 0x1234);
							}
						}
						else if (unknown14 == 0x000103FF)
						{
							// Non-interlaced left and right samples
							int numberOfSamples = unknown28 & 0xFFFF;
							Memory mem = Memory;
							for (int i = 0; i < numberOfSamples; i++)
							{
								mem.write16(unknown10 + i * 2, (short) 0x1234);
							}
						}
						else
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}, unknown20=0x{5:X}, unknown24=0x{6:X}, unknown28=0x{7:X}", value, status, unknown10, unknown14, unknown18, unknown20, unknown24, unknown28));
						}
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}, unknown20=0x{5:X}, unknown24=0x{6:X}, unknown28=0x{7:X}", value, status, unknown10, unknown14, unknown18, unknown20, unknown24, unknown28));
						}
						break;
					case 0x5B:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}, unknown20=0x{5:X}, unknown24=0x{6:X}, unknown28=0x{7:X}", value, status, unknown10, unknown14, unknown18, unknown20, unknown24, unknown28));
						}
						break;
					case 0x5D:
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}, unknown10=0x{2:X8}, unknown14=0x{3:X}, unknown18=0x{4:X}, unknown20=0x{5:X}, unknown24=0x{6:X}, unknown28=0x{7:X}", value, status, unknown10, unknown14, unknown18, unknown20, unknown24, unknown28));
						}
						break;
					default:
						Console.WriteLine(string.Format("Unknown command 0x{0:X}, status=0x{1:X}", value, status));
						break;
				}
    
				RuntimeContextLLE.triggerInterrupt(Processor, IntrManager.PSP_ATA_INTR);
			}
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
					value = status;
					break;
				default:
					value = base.read32(address);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read32(0x{1:X8}) returning 0x{2:X8}", Pc - 4, address, value));
			}

			return value;
		}

		public override void write32(int address, int value)
		{
			switch (address - baseAddress)
			{
				case 0x00:
					status = value;
					break;
				case 0x04:
					power = value;
					break;
				case 0x08:
					Command = value;
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
				case 0x20:
					unknown20 = value;
					break;
				case 0x24:
					unknown24 = value;
					break;
				case 0x28:
					unknown28 = value;
					break;
				case 0x2C:
					unknown2C = value;
					break;
				default:
					base.write32(address, value);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write32(0x{1:X8}, 0x{2:X8}) on {3}", Pc - 4, address, value, this));
			}
		}

		public override string ToString()
		{
			return string.Format("MMIOHandlerMe0FF000 unknown00=0x{0:X}, power=0x{1:X}, control=0x{2:X}, unknown10=0x{3:X}, unknown14=0x{4:X}, unknown18=0x{5:X}, unknown1C=0x{6:X}, unknown20=0x{7:X}, unknown24=0x{8:X}, unknown28=0x{9:X}", status, power, command, unknown10, unknown14, unknown18, unknown1C, unknown20, unknown24, unknown28);
		}
	}

}