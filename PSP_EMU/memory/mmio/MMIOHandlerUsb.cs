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

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using sceUsb = pspsharp.HLE.modules.sceUsb;
	using Scheduler = pspsharp.scheduler.Scheduler;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerUsb : MMIOHandlerBase
	{
		public static new Logger log = sceUsb.log;
		private const int STATE_VERSION = 0;
		public const int BASE_ADDRESS = unchecked((int)0xBD800000);
		private static MMIOHandlerUsb instance;
		private int unknown400;
		private int unknown404;
		private int unknown40C;
		private int unknown410;
		private int unknown414;
		private int unknown418;
		private int unknown41C;

		private class UsbReset : IAction
		{
			private readonly MMIOHandlerUsb outerInstance;

			public UsbReset(MMIOHandlerUsb outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				MMIOHandlerSystemControl.Instance.triggerUsbMemoryStickInterrupt(MMIOHandlerSystemControl.SYSREG_USBMS_USB_INTERRUPT3);
				RuntimeContextLLE.triggerInterrupt(outerInstance.Processor, IntrManager.PSP_USB_57);
			}
		}

		public static MMIOHandlerUsb Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MMIOHandlerUsb(BASE_ADDRESS);
				}
				return instance;
			}
		}

		private MMIOHandlerUsb(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			unknown400 = stream.readInt();
			unknown404 = stream.readInt();
			unknown40C = stream.readInt();
			unknown410 = stream.readInt();
			unknown414 = stream.readInt();
			unknown418 = stream.readInt();
			unknown41C = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(unknown400);
			stream.writeInt(unknown404);
			stream.writeInt(unknown40C);
			stream.writeInt(unknown410);
			stream.writeInt(unknown414);
			stream.writeInt(unknown418);
			stream.writeInt(unknown41C);
			base.write(stream);
		}

		public virtual void triggerReset()
		{
			Emulator.Scheduler.addAction(Scheduler.Now + 1000, new UsbReset(this));
		}

		private void clearUnknown40C(int mask)
		{
			unknown40C &= ~mask;
		}

		private void clearUnknown414(int mask)
		{
			unknown414 &= ~mask;
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x400:
					value = unknown400;
					break;
				case 0x404:
					value = unknown404;
					break;
				case 0x40C:
					value = unknown40C;
					break;
				case 0x410:
					value = unknown410;
					break;
				case 0x414:
					value = unknown414;
					break;
				case 0x418:
					value = unknown418;
					break;
				case 0x41C:
					value = unknown41C;
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
				case 0x400:
					unknown400 = value;
					break;
				case 0x404:
					unknown404 = value;
					break;
				case 0x40C:
					clearUnknown40C(value);
					break;
				case 0x410:
					unknown410 = value;
					break;
				case 0x414:
					clearUnknown414(value);
					break;
				case 0x418:
					unknown418 = value;
					break;
				case 0x41C:
					unknown41C = value;
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