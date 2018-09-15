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
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_DMA0_INTR;

	//using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using sceDmac = pspsharp.HLE.modules.sceDmac;
	using DmacProcessor = pspsharp.memory.mmio.dmac.DmacProcessor;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerDmac : MMIOHandlerBase
	{
		public static new Logger log = sceDmac.log;
		private const int STATE_VERSION = 0;
		private readonly DmacProcessor[] dmacProcessors = new DmacProcessor[8];
		private int flagsCompleted;
		private int flagsError;

		private class DmacCompletedAction : IAction
		{
			private readonly MMIOHandlerDmac outerInstance;

			internal int flagCompleted;

			public DmacCompletedAction(MMIOHandlerDmac outerInstance, int flagCompleted)
			{
				this.outerInstance = outerInstance;
				this.flagCompleted = flagCompleted;
			}

			public virtual void execute()
			{
				outerInstance.memcpyCompleted(flagCompleted);
			}
		}

		public MMIOHandlerDmac(int baseAddress) : base(baseAddress)
		{

			for (int i = 0; i < dmacProcessors.Length; i++)
			{
				dmacProcessors[i] = new DmacProcessor(Memory, Memory, baseAddress + 0x100 + i * 0x20, new DmacCompletedAction(this, 1 << i));
			}
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
			base.read(stream);
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
			base.write(stream);
		}

		private void memcpyCompleted(int flagCompleted)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("memcpyCompleted 0x{0:X}", flagCompleted));
			}
			flagsCompleted |= flagCompleted;

			checkInterrupt();
		}

		private void checkInterrupt()
		{
			if (flagsCompleted != 0)
			{
				RuntimeContextLLE.triggerInterrupt(Processor, PSP_DMA0_INTR);
			}
			else
			{
				RuntimeContextLLE.clearInterrupt(Processor, PSP_DMA0_INTR);
			}
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
				case 0x030:
					value = 0;
					break; // Unknown
					goto case 0x100;
				case 0x100:
				case 0x104:
				case 0x108:
				case 0x10C:
				case 0x110:
					value = dmacProcessors[0].read32(address - baseAddress - 0x100);
					break;
				case 0x120:
				case 0x124:
				case 0x128:
				case 0x12C:
				case 0x130:
					value = dmacProcessors[1].read32(address - baseAddress - 0x120);
					break;
				case 0x140:
				case 0x144:
				case 0x148:
				case 0x14C:
				case 0x150:
					value = dmacProcessors[2].read32(address - baseAddress - 0x140);
					break;
				case 0x160:
				case 0x164:
				case 0x168:
				case 0x16C:
				case 0x170:
					value = dmacProcessors[3].read32(address - baseAddress - 0x160);
					break;
				case 0x180:
				case 0x184:
				case 0x188:
				case 0x18C:
				case 0x190:
					value = dmacProcessors[4].read32(address - baseAddress - 0x180);
					break;
				case 0x1A0:
				case 0x1A4:
				case 0x1A8:
				case 0x1AC:
				case 0x1B0:
					value = dmacProcessors[5].read32(address - baseAddress - 0x1A0);
					break;
				case 0x1C0:
				case 0x1C4:
				case 0x1C8:
				case 0x1CC:
				case 0x1D0:
					value = dmacProcessors[6].read32(address - baseAddress - 0x1C0);
					break;
				case 0x1E0:
				case 0x1E4:
				case 0x1E8:
				case 0x1EC:
				case 0x1F0:
					value = dmacProcessors[7].read32(address - baseAddress - 0x1E0);
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
				case 0x030:
					if (value != 0 && value != 1)
					{
						base.write32(address, value);
					}
					break; // Unknown
					goto case 0x034;
				case 0x034:
					if (value != 0)
					{
						base.write32(address, value);
					}
					break; // Unknown
					goto case 0x100;
				case 0x100:
				case 0x104:
				case 0x108:
				case 0x10C:
				case 0x110:
					dmacProcessors[0].write32(address - baseAddress - 0x100, value);
					break;
				case 0x120:
				case 0x124:
				case 0x128:
				case 0x12C:
				case 0x130:
					dmacProcessors[1].write32(address - baseAddress - 0x120, value);
					break;
				case 0x140:
				case 0x144:
				case 0x148:
				case 0x14C:
				case 0x150:
					dmacProcessors[2].write32(address - baseAddress - 0x140, value);
					break;
				case 0x160:
				case 0x164:
				case 0x168:
				case 0x16C:
				case 0x170:
					dmacProcessors[3].write32(address - baseAddress - 0x160, value);
					break;
				case 0x180:
				case 0x184:
				case 0x188:
				case 0x18C:
				case 0x190:
					dmacProcessors[4].write32(address - baseAddress - 0x180, value);
					break;
				case 0x1A0:
				case 0x1A4:
				case 0x1A8:
				case 0x1AC:
				case 0x1B0:
					dmacProcessors[5].write32(address - baseAddress - 0x1A0, value);
					break;
				case 0x1C0:
				case 0x1C4:
				case 0x1C8:
				case 0x1CC:
				case 0x1D0:
					dmacProcessors[6].write32(address - baseAddress - 0x1C0, value);
					break;
				case 0x1E0:
				case 0x1E4:
				case 0x1E8:
				case 0x1EC:
				case 0x1F0:
					dmacProcessors[7].write32(address - baseAddress - 0x1E0, value);
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