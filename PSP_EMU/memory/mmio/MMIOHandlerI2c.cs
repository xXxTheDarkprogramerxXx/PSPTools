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
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_I2C_INTR;

	//using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using sceI2c = pspsharp.HLE.modules.sceI2c;
	using CY27040 = pspsharp.memory.mmio.cy27040.CY27040;
	using WM8750 = pspsharp.memory.mmio.wm8750.WM8750;
	using Scheduler = pspsharp.scheduler.Scheduler;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerI2c : MMIOHandlerBase
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			completeCommandAction = new CompleteCommandAction(this);
		}

		public static new Logger log = sceI2c.log;
		private const int STATE_VERSION = 0;
		public const int PSP_CY27040_I2C_ADDR = 0xD2;
		public const int PSP_WM8750_I2C_ADDR = 0x34;
		private int i2cAddress;
		private int dataLength;
		private int[] transmitData = new int[16];
		private int[] receiveData = new int[16];
		private int dataIndex = -1;
		private IAction completeCommandAction;

		private class CompleteCommandAction : IAction
		{
			private readonly MMIOHandlerI2c outerInstance;

			public CompleteCommandAction(MMIOHandlerI2c outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.completeCommand();
			}
		}

		public MMIOHandlerI2c(int baseAddress) : base(baseAddress)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			i2cAddress = stream.readInt();
			dataLength = stream.readInt();
			dataIndex = stream.readInt();
			stream.readInts(transmitData);
			stream.readInts(receiveData);
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(i2cAddress);
			stream.writeInt(dataLength);
			stream.writeInt(dataIndex);
			stream.writeInts(transmitData);
			stream.writeInts(receiveData);
			base.write(stream);
		}

		private void writeData(int value)
		{
			if (dataIndex < 0)
			{
				i2cAddress = value;
			}
			else
			{
				transmitData[dataIndex] = value & 0xFF;
			}
			dataIndex++;
		}

		private int readData()
		{
			dataIndex++;
			int value = receiveData[dataIndex];

			return value;
		}

		private void writeDataLength(int value)
		{
			dataLength = value;
			dataIndex = -1;
		}

		private void completeCommand()
		{
			RuntimeContextLLE.triggerInterrupt(Processor, PSP_I2C_INTR);
		}

		private void startCommand(int command)
		{
			int delayCompleteCommand = 0;

			// 0x85 is used by sceI2cMasterTransmitReceive after writing the transmit data (prefixed by the transmit address)
			// 0x8A is used by sceI2cMasterTransmitReceive after writing the receive address
			// 0x87 is used by sceI2cMasterTransmit after writing the transmit data (prefixed by the transmit address)
			switch (command)
			{
				case 0x85:
					// Nothing to do for now
					break;
				case 0x8A:
					// sceI2cMasterTransmitReceive
					// Receiving on the transmit address + 1
					switch (i2cAddress ^ 0x01)
					{
						case PSP_CY27040_I2C_ADDR:
							CY27040.Instance.executeTransmitReceiveCommand(transmitData, receiveData);
							delayCompleteCommand = 10000;
							break;
						case PSP_WM8750_I2C_ADDR:
							WM8750.Instance.executeTransmitReceiveCommand(transmitData, receiveData);
							break;
						default:
							Console.WriteLine(string.Format("MMIOHandlerI2c.startCommand unknown i2cAddress=0x{0:X}", i2cAddress));
							return;
					}
					break;
				case 0x87:
					// sceI2cMasterTransmit
					switch (i2cAddress)
					{
						case PSP_CY27040_I2C_ADDR:
							CY27040.Instance.executeTransmitCommand(transmitData);
							break;
						case PSP_WM8750_I2C_ADDR:
							WM8750.Instance.executeTransmitCommand(transmitData);
							break;
						default:
							Console.WriteLine(string.Format("MMIOHandlerI2c.startCommand unknown i2cAddress=0x{0:X}", i2cAddress));
							return;
					}
					break;
				default:
					Console.WriteLine(string.Format("MMIOHandlerI2c.startCommand unknown command=0x{0:X}", command));
					return;
			}

			if (delayCompleteCommand > 0)
			{
				Scheduler.Instance.addAction(Scheduler.Now + delayCompleteCommand, completeCommandAction);
			}
			else
			{
				completeCommand();
			}
		}

		private void acknowledgeInterrupt(int value)
		{
			if (value == 1)
			{
				RuntimeContextLLE.clearInterrupt(Processor, PSP_I2C_INTR);
			}
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
					value = 0;
					break; // Unknown
					goto case 0x04;
				case 0x04:
					value = 0;
					break; // Unknown
					goto case 0x08;
				case 0x08:
					value = dataLength;
					break;
				case 0x0C:
					value = readData();
					break;
				case 0x10:
					value = 0;
					break; // Unknown
					goto case 0x14;
				case 0x14:
					value = 0;
					break; // Unknown
					goto case 0x1C;
				case 0x1C:
					value = 0;
					break; // Unknown
					goto default;
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
				case 0x04:
					startCommand(value);
					break; // Unknown
					goto case 0x08;
				case 0x08:
					writeDataLength(value);
					break;
				case 0x0C:
					writeData(value);
					break;
				case 0x10:
					break; // Unknown
					goto case 0x14;
				case 0x14:
					break; // Unknown
					goto case 0x1C;
				case 0x1C:
					break; // Unknown
					goto case 0x28;
				case 0x28:
					acknowledgeInterrupt(value);
					break;
				case 0x2C:
					break; // Unknown
					goto default;
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
			return string.Format("i2cAddress=0x{0:X}", i2cAddress);
		}
	}

}