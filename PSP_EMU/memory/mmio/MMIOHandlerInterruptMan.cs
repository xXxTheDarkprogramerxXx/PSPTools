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
//	import static pspsharp.Allegrex.compiler.RuntimeContextLLE.clearInterruptException;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.RuntimeContextLLE.triggerInterruptException;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.ExceptionManager.IP2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_ATA_INTR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_MECODEC_INTR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_VBLANK_INTR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.clearBit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.hasBit;

	//using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using InterruptManager = pspsharp.HLE.modules.InterruptManager;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerInterruptMan : MMIOHandlerBase
	{
		public static new Logger log = InterruptManager.log;
		private const int STATE_VERSION = 0;
		private static MMIOHandlerProxyOnCpu instance;
		public const int BASE_ADDRESS = unchecked((int)0xBC300000);
		private const int NUMBER_INTERRUPTS = 64;
		private readonly bool[] interruptTriggered = new bool[NUMBER_INTERRUPTS];
		private readonly bool[] interruptEnabled = new bool[NUMBER_INTERRUPTS];
		private readonly bool[] interruptOccurred = new bool[NUMBER_INTERRUPTS];
		private readonly Processor processor;

		public static MMIOHandlerInterruptMan getInstance(Processor processor)
		{
			return (MMIOHandlerInterruptMan) ProxyInstance.getInstance(processor);
		}

		public static MMIOHandlerProxyOnCpu ProxyInstance
		{
			get
			{
				if (instance == null)
				{
					MMIOHandlerInterruptMan mainInstance = new MMIOHandlerInterruptMan(BASE_ADDRESS, RuntimeContextLLE.MainProcessor);
					MMIOHandlerInterruptMan meInstance = new MMIOHandlerInterruptMan(BASE_ADDRESS, RuntimeContextLLE.MediaEngineProcessor);
					instance = new MMIOHandlerProxyOnCpu(mainInstance, meInstance);
				}
				return instance;
			}
		}

		private MMIOHandlerInterruptMan(int baseAddress, Processor processor) : base(baseAddress)
		{
			this.processor = processor;
		}

		protected internal override Processor Processor
		{
			get
			{
				return processor;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			stream.readBooleans(interruptTriggered);
			stream.readBooleans(interruptEnabled);
			stream.readBooleans(interruptOccurred);
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeBooleans(interruptTriggered);
			stream.writeBooleans(interruptEnabled);
			stream.writeBooleans(interruptOccurred);
			base.write(stream);
		}

		public virtual void triggerInterrupt(int interruptNumber)
		{
			if (!hasInterruptTriggered(interruptNumber))
			{
				interruptTriggered[interruptNumber] = true;
				checkException();
			}
		}

		public virtual void clearInterrupt(int interruptNumber)
		{
			if (hasInterruptTriggered(interruptNumber))
			{
				interruptTriggered[interruptNumber] = false;
				checkException();
			}
		}

		public virtual bool hasInterruptTriggered(int interruptNumber)
		{
			return interruptTriggered[interruptNumber];
		}

		private void checkException()
		{
			if (doTriggerException())
			{
				triggerInterruptException(Processor, IP2);
			}
			else
			{
				clearInterruptException(Processor, IP2);
			}
		}

		public virtual bool doTriggerException()
		{
			for (int i = 0; i < NUMBER_INTERRUPTS; i++)
			{
				if (interruptTriggered[i] && interruptEnabled[i])
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("doTriggerException on {0}", this));
					}
					return true;
				}
			}

			return false;
		}

		private void setBits(bool[] values, int value, int offset, int mask)
		{
			for (int i = 0; mask != 0; i++, value = (int)((uint)value >> 1), mask >> >= 1)
			{
				if ((mask & 1) != 0)
				{
					values[offset + i] = (value & 1) != 0;
				}
			}
			checkException();
		}

		private void setBits1(bool[] values, int value)
		{
			setBits(values, value, 0, unchecked((int)0xDFFFFFF0));
		}

		private void setBits2(bool[] values, int value)
		{
			setBits(values, value, 32, unchecked((int)0xFFFF3F3F));
		}

		private void setBits3(bool[] values, int value)
		{
			int value3 = (value & 0xC0) | ((value >> 2) & 0xC000);
			setBits(values, value3, 32, 0x0000C0C0);
		}

		private int getBits(bool[] values, int offset)
		{
			int value = 0;
			for (int i = 31; i >= 0; i--)
			{
				value <<= 1;
				if (values[offset + i])
				{
					value |= 1;
				}
			}

			return value;
		}

		private int getBits1(bool[] values)
		{
			return getBits(values, 0);
		}

		private int getBits2(bool[] values)
		{
			return getBits(values, 32) & unchecked((int)0xFFFF3F3F);
		}

		private int getBits3(bool[] values)
		{
			int value3 = getBits(values, 32);
			value3 = (value3 & 0xC0) | ((value3 & 0xC000) << 2);
			return value3;
		}

		public override int read32(int address)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read32(0x{1:X8}) on {2}", Pc, address, this));
			}

			switch (address - baseAddress)
			{
				// Interrupt triggered:
				case 0x00:
					return getBits1(interruptTriggered);
				case 0x10:
					return getBits2(interruptTriggered);
				case 0x20:
					return getBits3(interruptTriggered);
				// Interrupt occurred (read only inside sceKernelIsInterruptOccurred, never written):
				case 0x04:
					return getBits1(interruptOccurred);
				case 0x14:
					return getBits2(interruptOccurred);
				case 0x24:
					return getBits3(interruptOccurred);
				// Interrupt enabled:
				case 0x08:
					return getBits1(interruptEnabled);
				case 0x18:
					return getBits2(interruptEnabled);
				case 0x28:
					return getBits3(interruptEnabled);
			}
			return base.read32(address);
		}

		public override void write32(int address, int value)
		{
			switch (address - baseAddress)
			{
				// Interrupt triggered:
				case 0 :
					// interruptman.prx is only writing the values 0x80000000 and 0x40000000
					// which seems to have the effect of clearing the triggers for these interrupts.
					// The Media Engine is also writing the value 0x00000020.
					if (hasBit(value, PSP_VBLANK_INTR))
					{
						clearInterrupt(PSP_VBLANK_INTR);
						value = clearBit(value, PSP_VBLANK_INTR);
					}
					if (hasBit(value, PSP_MECODEC_INTR))
					{
						clearInterrupt(PSP_MECODEC_INTR);
						value = clearBit(value, PSP_MECODEC_INTR);
					}
					if (hasBit(value, PSP_ATA_INTR))
					{
						clearInterrupt(PSP_ATA_INTR);
						value = clearBit(value, PSP_ATA_INTR);
					}
					if (value != 0)
					{
						base.write32(address, value);
					}
					break;
				// Interrupt enabled:
				case 8 :
					setBits1(interruptEnabled, value);
					break;
				case 24:
					setBits2(interruptEnabled, value);
					break;
				case 40:
					setBits3(interruptEnabled, value);
					break;
				// Unknown:
				default:
					base.write32(address, value);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write32(0x{1:X8}, 0x{2:X8}) on {3}", Pc, address, value, this));
			}
		}

		private void ToString(StringBuilder sb, string name, bool[] values)
		{
			if (sb.Length > 0)
			{
				sb.Append(", ");
			}
			sb.Append(name);
			sb.Append("[");
			bool first = true;
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i])
				{
					if (first)
					{
						first = false;
					}
					else
					{
						sb.Append("|");
					}
					sb.Append(IntrManager.getInterruptName(i));
				}
			}
			sb.Append("]");
		}

		public virtual string toStringInterruptTriggered()
		{
			StringBuilder sb = new StringBuilder();
			ToString(sb, "", interruptTriggered);

			return sb.ToString();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			ToString(sb, "interruptTriggered", interruptTriggered);
			ToString(sb, "interruptOccurred", interruptOccurred);
			ToString(sb, "interruptEnabled", interruptEnabled);

			return sb.ToString();
		}
	}

}