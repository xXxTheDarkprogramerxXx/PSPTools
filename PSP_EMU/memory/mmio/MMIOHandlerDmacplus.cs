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
//	import static pspsharp.HLE.Modules.sceDisplayModule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_DMACPLUS_INTR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceDisplay.PSP_DISPLAY_SETBUF_IMMEDIATE;

	using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using sceDmacplus = pspsharp.HLE.modules.sceDmacplus;
	using MEProcessor = pspsharp.mediaengine.MEProcessor;
	using DmacProcessor = pspsharp.memory.mmio.dmac.DmacProcessor;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerDmacplus : MMIOHandlerBase
	{
		public static new Logger log = sceDmacplus.log;
		private const int STATE_VERSION = 0;
		public const int COMPLETED_FLAG_UNKNOWN = 0x01;
		public const int COMPLETED_FLAG_AVC = 0x02;
		public const int COMPLETED_FLAG_SC2ME = 0x04;
		public const int COMPLETED_FLAG_ME2SC = 0x08;
		public const int COMPLETED_FLAG_SC128_MEMCPY = 0x10;
		private readonly DmacProcessor[] dmacProcessors = new DmacProcessor[3];
		// flagsCompleted:
		// - 0x01: not used
		// - 0x02: triggers call to sceLowIO_Driver.sub_000063DC (sceKernelSetEventFlag name=SceDmacplusAvc, bits=1)
		// - 0x04: triggers call to sceLowIO_Driver.sub_00006898 (sceKernelSetEventFlag name=SceDmacplusSc2Me, bits=1)
		// - 0x08: triggers call to sceLowIO_Driver.sub_00006D20 (sceKernelSetEventFlag name=SceDmacplusMe2Sc, bits=1)
		// - 0x10: triggers call to sceLowIO_Driver.sub_00006DDC (sceKernelSetEventFlag name=SceDmacplusSc128, bits=1, sceKernelSignalSema name=SceDmacplusSc128, signal=1)
		private int flagsCompleted;
		// flagsError:
		// - 0x01: triggers call to sceLowIO_Driver.sub_00005A40 (accessing 0xBC800110)
		// - 0x02: triggers call to sceLowIO_Driver.sub_000062DC (accessing 0xBC800160 and 0xBC800120-0xBC80014C, sceKernelSetEventFlag name=SceDmacplusAvc, bits=2)
		// - 0x04: triggers call to sceLowIO_Driver.sub_00006818 (accessing 0xBC800190 and 0xBC800180-0xBC80018C, sceKernelSetEventFlag name=SceDmacplusSc2Me, bits=2)
		// - 0x08: triggers call to sceLowIO_Driver.sub_00006CA0 (accessing 0xBC8001B0 and 0xBC8001A0-0xBC8001AC, sceKernelSetEventFlag name=SceDmacplusMe2Sc, bits=2)
		// - 0x10: triggers call to sceLowIO_Driver.sub_00006E30 (accessing 0xBC8001C0-0xBC8001CC, sceKernelSetEventFlag name=SceDmacplusSc128, bits=2)
		private int flagsError;
		private int displayFrameBufferAddr;
		private int displayWidth; // E.g. 480, must be a multiple of 8
		private int displayFrameBufferWidth; // E.g. 512, must be a multiple of 64
		private int displayPixelFormatCoded; // Values: [0..3]
		public const int DISPLAY_FLAG_ENABLED = 0x1;
		public const int DISPLAY_FLAG_UNKNOWN = 0x2;
		private int displayFlags;

		private class DmacCompletedAction : IAction
		{
			private readonly MMIOHandlerDmacplus outerInstance;

			internal int flagCompleted;

			public DmacCompletedAction(MMIOHandlerDmacplus outerInstance, int flagCompleted)
			{
				this.outerInstance = outerInstance;
				this.flagCompleted = flagCompleted;
			}

			public virtual void execute()
			{
				outerInstance.memcpyCompleted(flagCompleted);
			}
		}

		public MMIOHandlerDmacplus(int baseAddress) : base(baseAddress)
		{

			Memory meMemory = MEProcessor.Instance.MEMemory;
			Memory scMemory = Memory;
			dmacProcessors[0] = new DmacProcessor(scMemory, meMemory, baseAddress + 0x180, new DmacCompletedAction(this, COMPLETED_FLAG_SC2ME));
			dmacProcessors[1] = new DmacProcessor(meMemory, scMemory, baseAddress + 0x1A0, new DmacCompletedAction(this, COMPLETED_FLAG_ME2SC));
			dmacProcessors[2] = new DmacProcessor(scMemory, scMemory, baseAddress + 0x1C0, new DmacCompletedAction(this, COMPLETED_FLAG_SC128_MEMCPY));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			flagsCompleted = stream.readInt();
			flagsError = stream.readInt();
			for (int i = 0; i < dmacProcessors.Length; i++)
			{
				dmacProcessors[i].read(stream);
			}
			displayFrameBufferAddr = stream.readInt();
			displayWidth = stream.readInt();
			displayFrameBufferWidth = stream.readInt();
			displayPixelFormatCoded = stream.readInt();
			displayFlags = stream.readInt();
			base.read(stream);

			updateDisplay();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(flagsCompleted);
			stream.writeInt(flagsError);
			for (int i = 0; i < dmacProcessors.Length; i++)
			{
				dmacProcessors[i].write(stream);
			}
			stream.writeInt(displayFrameBufferAddr);
			stream.writeInt(displayWidth);
			stream.writeInt(displayFrameBufferWidth);
			stream.writeInt(displayPixelFormatCoded);
			stream.writeInt(displayFlags);
			base.write(stream);
		}

		private void memcpyCompleted(int flagCompleted)
		{
			flagsCompleted |= flagCompleted;

			checkInterrupt();
		}

		private void checkInterrupt()
		{
			if (flagsCompleted != 0)
			{
				RuntimeContextLLE.triggerInterrupt(Processor, PSP_DMACPLUS_INTR);
			}
			else
			{
				RuntimeContextLLE.clearInterrupt(Processor, PSP_DMACPLUS_INTR);
			}
		}

		public virtual int DisplayFrameBufferAddr
		{
			set
			{
				if (this.displayFrameBufferAddr != value)
				{
					this.displayFrameBufferAddr = value;
					updateDisplay();
				}
			}
		}

		public virtual int DisplayWidth
		{
			set
			{
				this.displayWidth = value;
			}
		}

		public virtual int DisplayFrameBufferWidth
		{
			set
			{
				if (this.displayFrameBufferWidth != value)
				{
					this.displayFrameBufferWidth = value;
					updateDisplay();
				}
			}
		}

		public virtual int DisplayPixelFormat
		{
			set
			{
				if (this.displayPixelFormatCoded != value)
				{
					this.displayPixelFormatCoded = value;
					updateDisplay();
				}
			}
		}

		public virtual int DisplayFlags
		{
			set
			{
				if (this.displayFlags != value)
				{
					this.displayFlags = value;
					updateDisplay();
				}
			}
		}

		private void updateDisplay()
		{
			int displayPixelFormat = sceDmacplus.pixelFormatFromCode[displayPixelFormatCoded & 0x3];
			int frameBufferAddr = displayFrameBufferAddr;
			if ((displayFlags & DISPLAY_FLAG_ENABLED) == 0)
			{
				frameBufferAddr = 0;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("updateDisplay.hleDisplaySetFrameBuf frameBufferAddr=0x{0:X8}, displayFrameBufferWidth=0x{1:X}, displayPixelFormat=0x{2:X}, displayFlags=0x{3:X}", frameBufferAddr, displayFrameBufferWidth, displayPixelFormat, displayFlags));
			}

			sceDisplayModule.hleDisplaySetFrameBuf(frameBufferAddr, displayFrameBufferWidth, displayPixelFormat, PSP_DISPLAY_SETBUF_IMMEDIATE);
			sceDisplayModule.step();
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x004:
					value = flagsCompleted;
					break;
				case 0x00C:
					value = flagsError;
					break;
				case 0x100:
					value = displayFrameBufferAddr;
					break;
				case 0x104:
					value = displayPixelFormatCoded;
					break;
				case 0x108:
					value = displayWidth;
					break;
				case 0x10C:
					value = displayFrameBufferWidth;
					break;
				case 0x110:
					value = displayFlags;
					break;
				case 0x150:
					value = 0;
					break; // TODO Unknown
					goto case 0x154;
				case 0x154:
					value = 0;
					break; // TODO Unknown
					goto case 0x158;
				case 0x158:
					value = 0;
					break; // TODO Unknown
					goto case 0x15C;
				case 0x15C:
					value = 0;
					break; // TODO Unknown
					goto case 0x180;
				case 0x180:
				case 0x184:
				case 0x188:
				case 0x18C:
				case 0x190:
					value = dmacProcessors[0].read32(address - baseAddress - 0x180);
					break;
				case 0x1A0:
				case 0x1A4:
				case 0x1A8:
				case 0x1AC:
				case 0x1B0:
					value = dmacProcessors[1].read32(address - baseAddress - 0x1A0);
					break;
				case 0x1C0:
				case 0x1C4:
				case 0x1C8:
				case 0x1CC:
				case 0x1D0:
					value = dmacProcessors[2].read32(address - baseAddress - 0x1C0);
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
				case 0x008:
					flagsCompleted &= ~value;
					checkInterrupt();
					break;
				case 0x010:
					flagsError &= ~value;
					break;
				case 0x100:
					DisplayFrameBufferAddr = value;
					break;
				case 0x104:
					DisplayPixelFormat = value;
					break;
				case 0x108:
					DisplayWidth = value;
					break;
				case 0x10C:
					DisplayFrameBufferWidth = value;
					break;
				case 0x110:
					DisplayFlags = value;
					break;
				case 0x160:
					break; // TODO reset?
					goto case 0x180;
				case 0x180:
				case 0x184:
				case 0x188:
				case 0x18C:
				case 0x190:
					dmacProcessors[0].write32(address - baseAddress - 0x180, value);
					break;
				case 0x1A0:
				case 0x1A4:
				case 0x1A8:
				case 0x1AC:
				case 0x1B0:
					dmacProcessors[1].write32(address - baseAddress - 0x1A0, value);
					break;
				case 0x1C0:
				case 0x1C4:
				case 0x1C8:
				case 0x1CC:
				case 0x1D0:
					dmacProcessors[2].write32(address - baseAddress - 0x1C0, value);
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