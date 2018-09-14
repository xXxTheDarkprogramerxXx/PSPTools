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
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_MECODEC_INTR;

	using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using Modules = pspsharp.HLE.Modules;
	using ExceptionManager = pspsharp.HLE.kernel.managers.ExceptionManager;
	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using MemoryStick = pspsharp.hardware.MemoryStick;
	using Usb = pspsharp.hardware.Usb;
	using MEProcessor = pspsharp.mediaengine.MEProcessor;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerSystemControl : MMIOHandlerBase
	{
		public static new Logger log = Logger.getLogger("systemcontrol");
		private const int STATE_VERSION = 0;
		public const int BASE_ADDRESS = unchecked((int)0xBC100000);
		public const int SYSREG_RESET_TOP = 0;
		public const int SYSREG_RESET_SC = 1;
		public const int SYSREG_RESET_ME = 2;
		public const int SYSREG_RESET_AW = 3;
		public const int SYSREG_RESET_VME = 4;
		public const int SYSREG_RESET_AVC = 5;
		public const int SYSREG_RESET_USB = 6;
		public const int SYSREG_RESET_ATA = 7;
		public const int SYSREG_RESET_MSIF0 = 8;
		public const int SYSREG_RESET_MSIF1 = 9;
		public const int SYSREG_RESET_KIRK = 10;
		public const int SYSREG_RESET_ATA_HDD = 12;
		public const int SYSREG_RESET_USB_HOST = 13;
		public const int SYSREG_RESET_UNKNOWN0 = 14;
		public const int SYSREG_RESET_UNKNOWN1 = 15;
		public const int SYSREG_BUSCLK_ME = 0;
		public const int SYSREG_BUSCLK_AWA = 1;
		public const int SYSREG_BUSCLK_AWB = 2;
		public const int SYSREG_BUSCLK_EDRAM = 3;
		public const int SYSREG_BUSCLK_DMACPLUS = 4;
		public const int SYSREG_BUSCLK_DMAC0 = 5;
		public const int SYSREG_BUSCLK_DMAC1 = 6;
		public const int SYSREG_BUSCLK_KIRK = 7;
		public const int SYSREG_BUSCLK_ATA = 8;
		public const int SYSREG_BUSCLK_USB = 9;
		public const int SYSREG_BUSCLK_MSIF0 = 10;
		public const int SYSREG_BUSCLK_MSIF1 = 11;
		public const int SYSREG_BUSCLK_EMCDDR = 12;
		public const int SYSREG_BUSCLK_EMCSM = 13;
		public const int SYSREG_BUSCLK_APB = 14;
		public const int SYSREG_BUSCLK_AUDIO0 = 15;
		public const int SYSREG_BUSCLK_AUDIO1 = 16;
		public const int SYSREG_CLK1_ATA = 0;
		public const int SYSREG_CLK1_USB = 4;
		public const int SYSREG_CLK1_MSIF0 = 8;
		public const int SYSREG_CLK1_MSIF1 = 9;
		public const int SYSREG_CLK_SPI0 = 0;
		public const int SYSREG_CLK_SPI1 = 1;
		public const int SYSREG_CLK_SPI2 = 2;
		public const int SYSREG_CLK_SPI3 = 3;
		public const int SYSREG_CLK_SPI4 = 4;
		public const int SYSREG_CLK_SPI5 = 5;
		public const int SYSREG_CLK_UART0 = 6;
		public const int SYSREG_CLK_UART1 = 7;
		public const int SYSREG_CLK_UART2 = 8;
		public const int SYSREG_CLK_UART3 = 9;
		public const int SYSREG_CLK_UART4 = 10;
		public const int SYSREG_CLK_UART5 = 11;
		public const int SYSREG_CLK_APB_TIMER0 = 12;
		public const int SYSREG_CLK_APB_TIMER1 = 13;
		public const int SYSREG_CLK_APB_TIMER2 = 14;
		public const int SYSREG_CLK_APB_TIMER3 = 15;
		public const int SYSREG_CLK_AUDIO0 = 16;
		public const int SYSREG_CLK_AUDIO1 = 17;
		public const int SYSREG_CLK_UNKNOWN0 = 18;
		public const int SYSREG_CLK_UNKNOWN1 = 19;
		public const int SYSREG_CLK_UNKNOWN2 = 20;
		public const int SYSREG_CLK_UNKNOWN3 = 21;
		public const int SYSREG_CLK_SIRCS = 22;
		public const int SYSREG_CLK_GPIO = 23;
		public const int SYSREG_CLK_AUDIO_CLKOUT = 24;
		public const int SYSREG_CLK_UNKNOWN4 = 25;
		public const int SYSREG_IO_EMCSM = 1;
		public const int SYSREG_IO_USB = 2;
		public const int SYSREG_IO_ATA = 3;
		public const int SYSREG_IO_MSIF0 = 4;
		public const int SYSREG_IO_MSIF1 = 5;
		public const int SYSREG_IO_LCDC = 6;
		public const int SYSREG_IO_AUDIO0 = 7;
		public const int SYSREG_IO_AUDIO1 = 8;
		public const int SYSREG_IO_IIC = 9;
		public const int SYSREG_IO_SIRCS = 10;
		public const int SYSREG_IO_UNK = 11;
		public const int SYSREG_IO_KEY = 12;
		public const int SYSREG_IO_PWM = 13;
		public const int SYSREG_IO_UART0 = 16;
		public const int SYSREG_IO_UART1 = 17;
		public const int SYSREG_IO_UART2 = 18;
		public const int SYSREG_IO_UART3 = 19;
		public const int SYSREG_IO_UART4 = 20;
		public const int SYSREG_IO_UART5 = 21;
		public const int SYSREG_IO_SPI0 = 24;
		public const int SYSREG_IO_SPI1 = 25;
		public const int SYSREG_IO_SPI2 = 26;
		public const int SYSREG_IO_SPI3 = 27;
		public const int SYSREG_IO_SPI4 = 28;
		public const int SYSREG_IO_SPI5 = 29;
		public const int SYSREG_AVC_POWER = 2;
		public const int SYSREG_USBMS_USB_CONNECTED = 0x000001;
		public const int SYSREG_USBMS_USB_INTERRUPT1 = 1;
		public const int SYSREG_USBMS_USB_INTERRUPT2 = 2;
		public const int SYSREG_USBMS_USB_INTERRUPT3 = 3;
		public const int SYSREG_USBMS_USB_INTERRUPT4 = 4;
		public static readonly int SYSREG_USBMS_USB_INTERRUPT_MASK = 0xF << SYSREG_USBMS_USB_INTERRUPT1;
		public const int SYSREG_USBMS_MS0_CONNECTED = 0x000100;
		public const int SYSREG_USBMS_MS0_INTERRUPT_MASK = 0x001E00;
		public const int SYSREG_USBMS_MS1_CONNECTED = 0x010000;
		public const int SYSREG_USBMS_MS1_INTERRUPT_MASK = 0x1E0000;
		public const int RAM_SIZE_16MB = 0;
		public const int RAM_SIZE_32MB = 1;
		public const int RAM_SIZE_64MB = 2;
		public const int RAM_SIZE_128MB = 3;
		private static MMIOHandlerSystemControl instance;
		private int resetDevices;
		private int busClockDevices;
		private int clock1Devices;
		private int clockDevices;
		private int ioDevices;
		private int ramSize;
		private int tachyonVersion;
		private int usbAndMemoryStick;
		private int avcPower;
		private int interrupts;
		private int pllFrequency;
		private int spiClkSelect;
		private int gpioEnable;
		private int ataClkSelect;
		private int unknown00;
		private int unknown3C;
		private int unknown60;
		private int unknown6C;
		private int unknown74;

		public static MMIOHandlerSystemControl Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MMIOHandlerSystemControl(BASE_ADDRESS);
				}
				return instance;
			}
		}

		private MMIOHandlerSystemControl(int baseAddress) : base(baseAddress)
		{

			ramSize = RAM_SIZE_16MB;
			tachyonVersion = Modules.sceSysregModule.sceSysregGetTachyonVersion();

			if (MemoryStick.Inserted)
			{
				usbAndMemoryStick |= SYSREG_USBMS_MS0_CONNECTED;
			}

			if (Usb.CableConnected)
			{
				usbAndMemoryStick |= SYSREG_USBMS_USB_CONNECTED;
				resetDevices |= 1 << SYSREG_RESET_USB;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			resetDevices = stream.readInt();
			busClockDevices = stream.readInt();
			clock1Devices = stream.readInt();
			clockDevices = stream.readInt();
			ioDevices = stream.readInt();
			ramSize = stream.readInt();
			tachyonVersion = stream.readInt();
			usbAndMemoryStick = stream.readInt();
			avcPower = stream.readInt();
			interrupts = stream.readInt();
			pllFrequency = stream.readInt();
			spiClkSelect = stream.readInt();
			gpioEnable = stream.readInt();
			ataClkSelect = stream.readInt();
			unknown00 = stream.readInt();
			unknown3C = stream.readInt();
			unknown60 = stream.readInt();
			unknown6C = stream.readInt();
			unknown74 = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(resetDevices);
			stream.writeInt(busClockDevices);
			stream.writeInt(clock1Devices);
			stream.writeInt(clockDevices);
			stream.writeInt(ioDevices);
			stream.writeInt(ramSize);
			stream.writeInt(tachyonVersion);
			stream.writeInt(usbAndMemoryStick);
			stream.writeInt(avcPower);
			stream.writeInt(interrupts);
			stream.writeInt(pllFrequency);
			stream.writeInt(spiClkSelect);
			stream.writeInt(gpioEnable);
			stream.writeInt(ataClkSelect);
			stream.writeInt(unknown00);
			stream.writeInt(unknown3C);
			stream.writeInt(unknown60);
			stream.writeInt(unknown6C);
			stream.writeInt(unknown74);
			base.write(stream);
		}

		private static string getResetDeviceName(int bit)
		{
			switch (bit)
			{
				case SYSREG_RESET_TOP :
					return "TOP";
				case SYSREG_RESET_SC :
					return "SC";
				case SYSREG_RESET_ME :
					return "ME";
				case SYSREG_RESET_AW :
					return "AW";
				case SYSREG_RESET_VME :
					return "VME";
				case SYSREG_RESET_AVC :
					return "AVC";
				case SYSREG_RESET_USB :
					return "USB";
				case SYSREG_RESET_ATA :
					return "ATA";
				case SYSREG_RESET_MSIF0 :
					return "MSIF0";
				case SYSREG_RESET_MSIF1 :
					return "MSIF1";
				case SYSREG_RESET_KIRK :
					return "KIRK";
				case SYSREG_RESET_ATA_HDD :
					return "ATA_HDD";
				case SYSREG_RESET_USB_HOST:
					return "USB_HOST";
				case SYSREG_RESET_UNKNOWN0:
					return "UNKNOWN0";
				case SYSREG_RESET_UNKNOWN1:
					return "UNKNOWN1";
			}
			return string.Format("SYSREG_RESET_{0:X2}", bit);
		}

		private static string getBusClockDeviceName(int bit)
		{
			switch (bit)
			{
				case SYSREG_BUSCLK_ME :
					return "ME";
				case SYSREG_BUSCLK_AWA :
					return "AWA";
				case SYSREG_BUSCLK_AWB :
					return "AWB";
				case SYSREG_BUSCLK_EDRAM :
					return "EDRAM";
				case SYSREG_BUSCLK_DMACPLUS:
					return "DMACPLUS";
				case SYSREG_BUSCLK_DMAC0 :
					return "DMAC0";
				case SYSREG_BUSCLK_DMAC1 :
					return "DMAC1";
				case SYSREG_BUSCLK_KIRK :
					return "KIRK";
				case SYSREG_BUSCLK_ATA :
					return "ATA";
				case SYSREG_BUSCLK_USB :
					return "USB";
				case SYSREG_BUSCLK_MSIF0 :
					return "MSIF0";
				case SYSREG_BUSCLK_MSIF1 :
					return "MSIF1";
				case SYSREG_BUSCLK_EMCDDR :
					return "EMCDDR";
				case SYSREG_BUSCLK_EMCSM :
					return "EMCSM";
				case SYSREG_BUSCLK_APB :
					return "APB";
				case SYSREG_BUSCLK_AUDIO0 :
					return "AUDIO0";
				case SYSREG_BUSCLK_AUDIO1 :
					return "AUDIO1";
			}
			return string.Format("SYSREG_BUSCLK_{0:X2}", bit);
		}

		private static string getClock1DeviceName(int bit)
		{
			switch (bit)
			{
				case SYSREG_CLK1_ATA :
					return "ATA";
				case SYSREG_CLK1_USB :
					return "USB";
				case SYSREG_CLK1_MSIF0:
					return "MSIF0";
				case SYSREG_CLK1_MSIF1:
					return "MSIF1";
			}
			return string.Format("SYSREG_CLK1_{0:X2}", bit);
		}

		private static string getClockDeviceName(int bit)
		{
			switch (bit)
			{
				case SYSREG_CLK_SPI0 :
					return "SPI0";
				case SYSREG_CLK_SPI1 :
					return "SPI1";
				case SYSREG_CLK_SPI2 :
					return "SPI2";
				case SYSREG_CLK_SPI3 :
					return "SPI3";
				case SYSREG_CLK_SPI4 :
					return "SPI4";
				case SYSREG_CLK_SPI5 :
					return "SPI5";
				case SYSREG_CLK_UART0 :
					return "UART0";
				case SYSREG_CLK_UART1 :
					return "UART1";
				case SYSREG_CLK_UART2 :
					return "UART2";
				case SYSREG_CLK_UART3 :
					return "UART3";
				case SYSREG_CLK_UART4 :
					return "UART4";
				case SYSREG_CLK_UART5 :
					return "UART5";
				case SYSREG_CLK_APB_TIMER0 :
					return "APB_TIMIER0";
				case SYSREG_CLK_APB_TIMER1 :
					return "APB_TIMIER1";
				case SYSREG_CLK_APB_TIMER2 :
					return "APB_TIMIER2";
				case SYSREG_CLK_APB_TIMER3 :
					return "APB_TIMIER2";
				case SYSREG_CLK_AUDIO0 :
					return "AUDIO0";
				case SYSREG_CLK_AUDIO1 :
					return "AUDIO1";
				case SYSREG_CLK_UNKNOWN0 :
					return "UNKNOWN0";
				case SYSREG_CLK_UNKNOWN1 :
					return "UNKNOWN1";
				case SYSREG_CLK_UNKNOWN2 :
					return "UNKNOWN2";
				case SYSREG_CLK_UNKNOWN3 :
					return "UNKNOWN3";
				case SYSREG_CLK_SIRCS :
					return "SIRCS";
				case SYSREG_CLK_GPIO :
					return "GPIO";
				case SYSREG_CLK_AUDIO_CLKOUT:
					return "AUDIO_CLKOUT";
				case SYSREG_CLK_UNKNOWN4 :
					return "UNKNOWN4";
			}
			return string.Format("SYSREG_CLK_{0:X2}", bit);
		}

		private static string getIoDeviceName(int bit)
		{
			switch (bit)
			{
				case SYSREG_IO_EMCSM :
					return "EMCSM";
				case SYSREG_IO_USB :
					return "USB";
				case SYSREG_IO_ATA :
					return "ATA";
				case SYSREG_IO_MSIF0 :
					return "MSIF0";
				case SYSREG_IO_MSIF1 :
					return "MSIF1";
				case SYSREG_IO_LCDC :
					return "LCDC";
				case SYSREG_IO_AUDIO0:
					return "AUDIO0";
				case SYSREG_IO_AUDIO1:
					return "AUDIO1";
				case SYSREG_IO_IIC :
					return "IIC";
				case SYSREG_IO_SIRCS :
					return "SIRCS";
				case SYSREG_IO_UNK :
					return "UNK";
				case SYSREG_IO_KEY :
					return "KEY";
				case SYSREG_IO_PWM :
					return "PWM";
				case SYSREG_IO_UART0 :
					return "UART0";
				case SYSREG_IO_UART1 :
					return "UART1";
				case SYSREG_IO_UART2 :
					return "UART2";
				case SYSREG_IO_UART3 :
					return "UART3";
				case SYSREG_IO_UART4 :
					return "UART4";
				case SYSREG_IO_UART5 :
					return "UART5";
				case SYSREG_IO_SPI0 :
					return "SPI0";
				case SYSREG_IO_SPI1 :
					return "SPI1";
				case SYSREG_IO_SPI2 :
					return "SPI2";
				case SYSREG_IO_SPI3 :
					return "SPI3";
				case SYSREG_IO_SPI4 :
					return "SPI4";
				case SYSREG_IO_SPI5 :
					return "SPI5";
			}
			return string.Format("SYSREG_IO_{0:X2}", bit);
		}

		private void sysregInterruptToOther(int value)
		{
			if (value != 0)
			{
				if (RuntimeContextLLE.MainCpu)
				{
					// Interrupt from the main cpu to the Media Engine cpu
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sysregInterruptToOther to ME on {0}", MMIOHandlerMeCore.Instance.ToString()));
					}
					RuntimeContextLLE.triggerInterrupt(RuntimeContextLLE.MediaEngineProcessor, PSP_MECODEC_INTR);
					RuntimeContextLLE.MediaEngineProcessor.triggerException(ExceptionManager.IP2);
				}
				else
				{
					// Interrupt from the Media Engine cpu to the main cpu
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sysregInterruptToOther from ME on {0}", MMIOHandlerMeCore.Instance.ToString()));
					}
					RuntimeContextLLE.triggerInterrupt(RuntimeContextLLE.MainProcessor, PSP_MECODEC_INTR);
				}
			}
		}

		private bool isFalling(int oldFlag, int newFlag, int bit)
		{
			int mask = 1 << bit;
			return (oldFlag & mask) > (newFlag & mask);
		}

		private bool hasBit(int value, int bit)
		{
			return (value & (1 << bit)) != 0;
		}

		private int ResetDevices
		{
			set
			{
				int oldResetDevices = resetDevices;
				resetDevices = value;
    
				if (isFalling(oldResetDevices, resetDevices, SYSREG_RESET_ME))
				{
					MEProcessor.Instance.triggerReset();
				}
				if (isFalling(oldResetDevices, resetDevices, SYSREG_RESET_USB))
				{
					MMIOHandlerUsb.Instance.triggerReset();
				}
			}
		}

		private int BusClockDevices
		{
			set
			{
				busClockDevices = value;
			}
		}

		private int Clock1Devices
		{
			set
			{
				clock1Devices = value;
			}
		}

		private int ClockDevices
		{
			set
			{
				clockDevices = value;
			}
		}

		private int IoDevices
		{
			set
			{
				ioDevices = value;
			}
		}

		private int RamSize
		{
			set
			{
				ramSize = value & 0x3;
			}
		}

		public virtual void triggerUsbMemoryStickInterrupt(int bit)
		{
			usbAndMemoryStick |= 1 << bit;
		}

		private void clearUsbMemoryStick(int mask)
		{
			int oldUsbAndMemoryStick = usbAndMemoryStick;
			mask &= SYSREG_USBMS_USB_INTERRUPT_MASK | SYSREG_USBMS_MS0_INTERRUPT_MASK | SYSREG_USBMS_MS1_INTERRUPT_MASK;
			usbAndMemoryStick &= ~mask;

			if (isFalling(oldUsbAndMemoryStick, usbAndMemoryStick, SYSREG_USBMS_USB_INTERRUPT1))
			{
				RuntimeContextLLE.clearInterrupt(Processor, IntrManager.PSP_USB_58);
			}
			if (isFalling(oldUsbAndMemoryStick, usbAndMemoryStick, SYSREG_USBMS_USB_INTERRUPT2))
			{
				RuntimeContextLLE.clearInterrupt(Processor, IntrManager.PSP_USB_59);
			}
			if (isFalling(oldUsbAndMemoryStick, usbAndMemoryStick, SYSREG_USBMS_USB_INTERRUPT3))
			{
				RuntimeContextLLE.clearInterrupt(Processor, IntrManager.PSP_USB_57);
			}
			if (isFalling(oldUsbAndMemoryStick, usbAndMemoryStick, SYSREG_USBMS_USB_INTERRUPT4))
			{
				RuntimeContextLLE.clearInterrupt(Processor, IntrManager.PSP_USB_56);
			}
		}

		private int AvcPower
		{
			set
			{
				avcPower = value;
    
				if (hasBit(avcPower, SYSREG_AVC_POWER))
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("MMIOHandlerSystemControl.setAvcPower enabling Avc power"));
					}
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("MMIOHandlerSystemControl.setAvcPower disabling Avc power"));
					}
				}
    
				// Only bit SYSREG_AVC_POWER is known
				if ((value & ~(1 << SYSREG_AVC_POWER)) != 0)
				{
					log.error(string.Format("MMIOHandlerSystemControl.setAvcPower unknown value 0x{0:X}", value));
				}
			}
		}

		private void clearInterrupts(int mask)
		{
			interrupts &= ~mask;
		}

		private int PllFrequency
		{
			set
			{
				if ((value & 0x80) != 0)
				{
					pllFrequency = value & 0xF;
				}
			}
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
					value = unknown00;
					break;
				case 0x3C:
					value = unknown3C;
					break;
				case 0x40:
					value = (tachyonVersion << 8) | ramSize;
					break;
				case 0x4C:
					value = resetDevices;
					break;
				case 0x50:
					value = busClockDevices;
					break;
				case 0x54:
					value = clock1Devices;
					break;
				case 0x58:
					value = clockDevices;
					break;
				case 0x5C:
					value = ataClkSelect;
					break;
				case 0x60:
					value = unknown60;
					break;
				case 0x64:
					value = spiClkSelect;
					break;
				case 0x68:
					value = pllFrequency;
					break;
				case 0x6C:
					value = unknown6C;
					break;
				case 0x70:
					value = avcPower;
					break;
				case 0x74:
					value = unknown74;
					break;
				case 0x78:
					value = ioDevices;
					break;
				case 0x7C:
					value = gpioEnable;
					break;
				case 0x80:
					value = usbAndMemoryStick;
					break;
				case 0x90:
					value = (int) Modules.sceSysregModule.sceSysregGetFuseId();
					break;
				case 0x94:
					value = (int)(Modules.sceSysregModule.sceSysregGetFuseId() >> 32);
					break;
				case 0x98:
					value = Modules.sceSysregModule.sceSysregGetFuseConfig();
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
				case 0x04:
					clearInterrupts(value);
					break;
				case 0x3C:
					unknown3C = value;
					break;
				case 0x40:
					RamSize = value;
					break;
				case 0x44:
					sysregInterruptToOther(value);
					break;
				case 0x4C:
					ResetDevices = value;
					break;
				case 0x50:
					BusClockDevices = value;
					break;
				case 0x5C:
					ataClkSelect = value;
					break;
				case 0x54:
					Clock1Devices = value;
					break;
				case 0x58:
					ClockDevices = value;
					break;
				case 0x60:
					unknown60 = value;
					break;
				case 0x64:
					spiClkSelect = value;
					break;
				case 0x68:
					PllFrequency = value;
					break;
				case 0x6C:
					unknown6C = value;
					break;
				case 0x70:
					AvcPower = value;
					break;
				case 0x74:
					unknown74 = value;
					break;
				case 0x78:
					IoDevices = value;
					break;
				case 0x7C:
					gpioEnable = value;
					break;
				case 0x80:
					clearUsbMemoryStick(value);
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

		private void ToString(StringBuilder sb, int bits, int type, string prefix)
		{
			if (sb.Length > 0)
			{
				sb.Append(", ");
			}
			sb.Append(prefix);
			sb.Append("[");
			bool first = true;
			for (int bit = 0; bit < 32; bit++)
			{
				if (hasBit(bits, bit))
				{
					if (first)
					{
						first = false;
					}
					else
					{
						sb.Append("|");
					}
					switch (type)
					{
						case 0:
							sb.Append(getResetDeviceName(bit));
							break;
						case 1:
							sb.Append(getBusClockDeviceName(bit));
							break;
						case 2:
							sb.Append(getClock1DeviceName(bit));
							break;
						case 3:
							sb.Append(getClockDeviceName(bit));
							break;
						case 4:
							sb.Append(getIoDeviceName(bit));
							break;
					}
				}
			}
			sb.Append("]");
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			ToString(sb, resetDevices, 0, "resetDevices");
			ToString(sb, busClockDevices, 1, "busClockDevices");
			ToString(sb, clock1Devices, 2, "clock1Devices");
			ToString(sb, clockDevices, 3, "clockDevices");
			ToString(sb, ioDevices, 4, "ioDevices");

//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: sb.append(String.format(", USB[connected=%b, interrupt=0x%01X]", (usbAndMemoryStick & SYSREG_USBMS_USB_CONNECTED) != 0, (usbAndMemoryStick & SYSREG_USBMS_USB_INTERRUPT_MASK) >> 1));
			sb.Append(string.Format(", USB[connected=%b, interrupt=0x%01X]", (usbAndMemoryStick & SYSREG_USBMS_USB_CONNECTED) != 0, (usbAndMemoryStick & SYSREG_USBMS_USB_INTERRUPT_MASK) >> 1));
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: sb.append(String.format(", MemoryStick0[connected=%b, interrupt=0x%01X]", (usbAndMemoryStick & SYSREG_USBMS_MS0_CONNECTED) != 0, (usbAndMemoryStick & SYSREG_USBMS_USB_INTERRUPT_MASK) >> 9));
			sb.Append(string.Format(", MemoryStick0[connected=%b, interrupt=0x%01X]", (usbAndMemoryStick & SYSREG_USBMS_MS0_CONNECTED) != 0, (usbAndMemoryStick & SYSREG_USBMS_USB_INTERRUPT_MASK) >> 9));
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: sb.append(String.format(", MemoryStick1[connected=%b, interrupt=0x%01X]", (usbAndMemoryStick & SYSREG_USBMS_MS1_CONNECTED) != 0, (usbAndMemoryStick & SYSREG_USBMS_USB_INTERRUPT_MASK) >> 17));
			sb.Append(string.Format(", MemoryStick1[connected=%b, interrupt=0x%01X]", (usbAndMemoryStick & SYSREG_USBMS_MS1_CONNECTED) != 0, (usbAndMemoryStick & SYSREG_USBMS_USB_INTERRUPT_MASK) >> 17));
			sb.Append(string.Format(", interrupts=0x{0:X}", interrupts));
			sb.Append(string.Format(", pllFrequency=0x{0:X}", pllFrequency));

			return sb.ToString();
		}
	}

}