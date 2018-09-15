using System.Text;

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
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_GPIO_INTR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.clearBit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.hasBit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.setBit;

	//using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using sceGpio = pspsharp.HLE.modules.sceGpio;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerGpio : MMIOHandlerBase
	{
		public static new Logger log = sceGpio.log;
		private const int STATE_VERSION = 0;
		public const int BASE_ADDRESS = unchecked((int)0xBE240000);
		private static MMIOHandlerGpio instance;
		public const int GPIO_PORT_DISPLAY = 0x00;
		public const int GPIO_PORT_WM8750_Port1 = 0x01;
		public const int GPIO_PORT_SYSCON_START_CMD = 0x03;
		public const int GPIO_PORT_SYSCON_END_CMD = 0x04;
		public const int GPIO_PORT_WM8750_Port5 = 0x05;
		public const int GPIO_PORT_LED_MS = 0x06;
		public const int GPIO_PORT_LED_WLAN = 0x07;
		public const int GPIO_PORT_USB = 0x17;
		public const int GPIO_PORT_BLUETOOTH = 0x18;
		public const int GPIO_PORT_UMD = 0x1A;
		private const int NUMBER_PORTS = 32;
		private int ports;
		private int isOutput;
		private int isInputOn;
		private int isInterruptEnabled;
		private int isInterruptTriggered;
		private int isEdgeDetection;
		private int isRisingEdge;
		private int isFallingEdge;
		private int isCapturePort;
		private int isTimerCaptureEnabled;

		public static MMIOHandlerGpio Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MMIOHandlerGpio(BASE_ADDRESS);
				}
				return instance;
			}
		}

		private MMIOHandlerGpio(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			ports = stream.readInt();
			isOutput = stream.readInt();
			isInputOn = stream.readInt();
			isInterruptEnabled = stream.readInt();
			isInterruptTriggered = stream.readInt();
			isEdgeDetection = stream.readInt();
			isRisingEdge = stream.readInt();
			isFallingEdge = stream.readInt();
			isCapturePort = stream.readInt();
			isTimerCaptureEnabled = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(ports);
			stream.writeInt(isOutput);
			stream.writeInt(isInputOn);
			stream.writeInt(isInterruptEnabled);
			stream.writeInt(isInterruptTriggered);
			stream.writeInt(isEdgeDetection);
			stream.writeInt(isRisingEdge);
			stream.writeInt(isFallingEdge);
			stream.writeInt(isCapturePort);
			stream.writeInt(isTimerCaptureEnabled);
			base.write(stream);
		}

		private static string getPortName(int port)
		{
			switch (port)
			{
				case GPIO_PORT_DISPLAY:
					return "DISPLAY";
				case GPIO_PORT_WM8750_Port1:
					return "WM8750_Port1";
				case GPIO_PORT_SYSCON_START_CMD:
					return "SYSCON_START_CMD";
				case GPIO_PORT_SYSCON_END_CMD:
					return "SYSCON_END_CMD";
				case GPIO_PORT_WM8750_Port5:
					return "WM8750_Port5";
				case GPIO_PORT_LED_MS:
					return "LED_MS";
				case GPIO_PORT_LED_WLAN:
					return "LED_WLAN";
				case GPIO_PORT_USB:
					return "USB";
				case GPIO_PORT_BLUETOOTH:
					return "BLUETOOTH";
				case GPIO_PORT_UMD:
					return "UMD";
			}

			return string.Format("GPIO_UNKNOWN_PORT_0x{0:X}", port);
		}

		public virtual int Port
		{
			set
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("MMIOHandlerGpio.setPort 0x{0:X}({1}) on {2}", value, getPortName(value), this));
				}
    
				if (!hasBit(ports, value))
				{
					if (hasBit(isRisingEdge, value))
					{
						triggerInterrupt(value);
					}
					ports = setBit(ports, value);
				}
			}
		}

		public virtual void clearPort(int port)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("MMIOHandlerGpio.clearPort 0x{0:X}({1}) on {2}", port, getPortName(port), this));
			}

			if (hasBit(ports, port))
			{
				if (hasBit(isFallingEdge, port))
				{
					triggerInterrupt(port);
				}
				ports = clearBit(ports, port);
			}
		}

		private void triggerInterrupt(int bit)
		{
			if (!hasBit(isInterruptTriggered, bit))
			{
				isInterruptTriggered = setBit(isInterruptTriggered, bit);
				checkInterrupt();
			}
		}

		private void checkInterrupt()
		{
			if ((isInterruptTriggered & isInterruptEnabled) != 0)
			{
				RuntimeContextLLE.triggerInterrupt(Processor, PSP_GPIO_INTR);
			}
			else
			{
				RuntimeContextLLE.clearInterrupt(Processor, PSP_GPIO_INTR);
			}
		}

		private int Ports
		{
			set
			{
				if (value != 0)
				{
					for (int i = 0; i < NUMBER_PORTS; i++)
					{
						if (hasBit(value, i))
						{
							Port = i;
						}
					}
				}
			}
		}

		private void clearPorts(int value)
		{
			if (value != 0)
			{
				for (int i = 0; i < NUMBER_PORTS; i++)
				{
					if (hasBit(value, i))
					{
						clearPort(i);
					}
				}
			}
		}

		private void acknowledgeInterrupt(int value)
		{
			if (value != 0 && isInterruptTriggered != 0)
			{
				isInterruptTriggered &= ~value;
				checkInterrupt();
			}
		}

		private int InterruptEnabled
		{
			set
			{
				if (this.isInterruptEnabled != value)
				{
					this.isInterruptEnabled = value;
					checkInterrupt();
				}
			}
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
					value = isOutput;
					break;
				case 0x04:
					value = ports;
					break;
				case 0x10:
					value = isEdgeDetection;
					break;
				case 0x14:
					value = isFallingEdge;
					break;
				case 0x18:
					value = isRisingEdge;
					break;
				case 0x1C:
					value = isInterruptEnabled;
					break;
				case 0x20:
					value = isInterruptTriggered;
					break;
				case 0x30:
					value = isCapturePort;
					break;
				case 0x34:
					value = isTimerCaptureEnabled;
					break;
				case 0x40:
					value = isInputOn;
					break;
				case 0x48:
					value = 0;
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
					isOutput = value;
					break;
				case 0x08:
					Ports = value;
					break;
				case 0x0C:
					clearPorts(value);
					break;
				case 0x10:
					isEdgeDetection = value;
					break;
				case 0x14:
					isFallingEdge = value;
					break;
				case 0x18:
					isRisingEdge = value;
					break;
				case 0x1C:
					InterruptEnabled = value;
					break;
				case 0x24:
					acknowledgeInterrupt(value);
					break;
				case 0x30:
					isCapturePort = value;
					break;
				case 0x34:
					isTimerCaptureEnabled = value;
					break;
				case 0x40:
					isInputOn = value;
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

		private static string getPortNames(int bits)
		{
			StringBuilder s = new StringBuilder();
			for (int i = 0; i < NUMBER_PORTS; i++)
			{
				if (hasBit(bits, i))
				{
					if (s.Length > 0)
					{
						s.Append("|");
					}
					s.Append(getPortName(i));
				}
			}

			return s.ToString();
		}

		public override string ToString()
		{
			return string.Format("MMIOHandlerGpio ports=0x{0:X8}({1}), isInterruptEnabled=0x{2:X8}({3}), isInterruptTriggered=0x{4:X8}({5}), isOutput=0x{6:X8}({7}), isEdgeDetection=0x{8:X8}, isFallingEdge=0x{9:X8}, isRisingEdge=0x{10:X8}, isInputOn=0x{11:X8}", ports, getPortNames(ports), isInterruptEnabled, getPortNames(isInterruptEnabled), isInterruptTriggered, getPortNames(isInterruptTriggered), isOutput, getPortNames(isOutput), isEdgeDetection, isFallingEdge, isRisingEdge, isInputOn);
		}
	}

}