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
//	import static pspsharp.HLE.Modules.scePowerModule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.Modules.sceSysconModule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_BATTERY_GET_CYCLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_BATTERY_GET_ELEC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_BATTERY_GET_FULL_CAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_BATTERY_GET_LIMIT_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_BATTERY_GET_STATUS_CAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_BATTERY_GET_TEMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_BATTERY_GET_VOLT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_CTRL_ANALOG_XY_POLLING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_CTRL_HR_POWER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_CTRL_LED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_CTRL_LEPTON_POWER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_CTRL_MS_POWER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_CTRL_POWER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_CTRL_VOLTAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_CTRL_WLAN_POWER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_GET_ANALOG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_GET_BARYON;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_GET_DIGITAL_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_GET_DIGITAL_KEY_ANALOG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_GET_KERNEL_DIGITAL_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_GET_KERNEL_DIGITAL_KEY_ANALOG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_GET_POMMEL_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_GET_POWER_STATUS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_GET_POWER_SUPPLY_STATUS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_GET_TIMESTAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_NOP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_READ_ALARM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_READ_CLOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_READ_SCRATCHPAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_RECEIVE_SETPARAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_RESET_DEVICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_SEND_SETPARAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_WRITE_ALARM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_WRITE_CLOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.PSP_SYSCON_CMD_WRITE_SCRATCHPAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSyscon.getSysconCmdName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.mmio.MMIOHandlerGpio.GPIO_PORT_SYSCON_END_CMD;


	//using Logger = org.apache.log4j.Logger;

	using Modules = pspsharp.HLE.Modules;
	using sceSyscon = pspsharp.HLE.modules.sceSyscon;
	using Battery = pspsharp.hardware.Battery;
	using LED = pspsharp.hardware.LED;
	using MemoryStick = pspsharp.hardware.MemoryStick;
	using Model = pspsharp.hardware.Model;
	using UMDDrive = pspsharp.hardware.UMDDrive;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;
	using Utilities = pspsharp.util.Utilities;

	public class MMIOHandlerSyscon : MMIOHandlerBase
	{
		public static new Logger log = sceSyscon.log;
		private const int STATE_VERSION = 0;
		private static MMIOHandlerSyscon instance;
		public const int BASE_ADDRESS = unchecked((int)0xBE580000);
		public const int PSP_SYSCON_RX_STATUS = 0;
		public const int PSP_SYSCON_RX_LEN = 1;
		public const int PSP_SYSCON_RX_RESPONSE = 2;
		public const int PSP_SYSCON_TX_CMD = 0;
		public const int PSP_SYSCON_TX_LEN = 1;
		public const int PSP_SYSCON_TX_DATA = 2;
		public const int BARYON_STATUS_AC_SUPPLY = 0x01;
		public const int BARYON_STATUS_WLAN_POWER = 0x02;
		public const int BARYON_STATUS_HR_POWER = 0x04;
		public const int BARYON_STATUS_ALARM = 0x08;
		public const int BARYON_STATUS_POWER_SWITCH = 0x10;
		public const int BARYON_STATUS_LOW_BATTERY = 0x20;
		public const int BARYON_STATUS_GSENSOR = 0x80;
		private const int MAX_DATA_LENGTH = 16;
		private int[] data = new int[MAX_DATA_LENGTH];
		private int dataIndex;
		private bool endDataIndex;
		private int error;

		public static MMIOHandlerSyscon Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MMIOHandlerSyscon(BASE_ADDRESS);
				}
				return instance;
			}
		}

		private MMIOHandlerSyscon(int baseAddress) : base(baseAddress)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			stream.readInts(data);
			dataIndex = stream.readInt();
			endDataIndex = stream.readBoolean();
			error = stream.readInt();
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInts(data);
			stream.writeInt(dataIndex);
			stream.writeBoolean(endDataIndex);
			stream.writeInt(error);
			base.write(stream);
		}

		private void clearData()
		{
			Arrays.Fill(data, 0);
		}

		private void setDataValue(int offset, int value)
		{
			data[offset] = value & 0xFF;
		}

		private void addHashValue()
		{
			int hash = 0;
			int Length = data[PSP_SYSCON_RX_LEN];
			for (int i = 0; i < Length; i++)
			{
				hash = (hash + data[i]) & 0xFF;
			}
			data[Length] = (~hash) & 0xFF;
		}

		private int[] addResponseData16(int[] responseData, int value)
		{
			responseData = Utilities.add(responseData, value & 0xFF);
			responseData = Utilities.add(responseData, (value >> 8) & 0xFF);

			return responseData;
		}

		private int[] addResponseData32(int[] responseData, int value)
		{
			responseData = Utilities.add(responseData, value & 0xFF);
			responseData = Utilities.add(responseData, (value >> 8) & 0xFF);
			responseData = Utilities.add(responseData, (value >> 16) & 0xFF);
			responseData = Utilities.add(responseData, (value >> 24) & 0xFF);

			return responseData;
		}

		private int getData32(int[] responseData, int offset)
		{
			int value = responseData[offset] & 0xFF;
			value |= (responseData[offset + 1] & 0xFF) << 8;
			value |= (responseData[offset + 2] & 0xFF) << 16;
			value |= (responseData[offset + 3] & 0xFF) << 24;

			return value;
		}

		private int getData24(int[] responseData, int offset)
		{
			int value = responseData[offset] & 0xFF;
			value |= (responseData[offset + 1] & 0xFF) << 8;
			value |= (responseData[offset + 2] & 0xFF) << 16;

			return value;
		}

		private int BaryonStatus
		{
			get
			{
				int baryonStatus = 0;
    
				baryonStatus |= BARYON_STATUS_AC_SUPPLY;
				baryonStatus |= BARYON_STATUS_WLAN_POWER;
				baryonStatus |= BARYON_STATUS_POWER_SWITCH;
    
				return baryonStatus;
			}
		}

		private int[] addButtonsResponseData(int[] responseData, bool kernel)
		{
			int buttons = State.controller.Buttons;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("addButtonsResponseData buttons=0x{0:X8}", buttons));
			}
			buttons ^= kernel ? 0x20F7F3F9 : 0x7F3F9;
			responseData = Utilities.add(responseData, ((buttons & 0xF000) >> 8) | ((buttons & 0xF0) >> 4));
			responseData = Utilities.add(responseData, ((buttons & 0xF0000) >> 12) | ((buttons & 0x300) >> 7) | (buttons & 0x9));
			if (kernel)
			{
				responseData = Utilities.add(responseData, (buttons & 0xBF00000) >> 20);
				responseData = Utilities.add(responseData, (buttons & 0x30000000) >> 28);
			}

			return responseData;
		}

		private int[] addAnalogResponseData(int[] responseData)
		{
			sbyte lx = State.controller.Lx;
			sbyte ly = State.controller.Ly;

			if (Modules.sceCtrlModule.ModeDigital)
			{
				// PSP_CTRL_MODE_DIGITAL
				// moving the analog stick has no effect and always returns 128,128
				lx = Controller.analogCenter;
				ly = Controller.analogCenter;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("addAnalogResponseData lx=0x{0:X2}, ly=0x{1:X2}", lx & 0xFF, ly & 0xFF));
			}
			responseData = Utilities.add(responseData, lx & 0xFF);
			responseData = Utilities.add(responseData, ly & 0xFF);

			return responseData;
		}

		private void startSysconCmd()
		{
			int cmd = data[PSP_SYSCON_TX_CMD];
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("startSysconCmd cmd=0x{0:X2}({1}), {2}", cmd, getSysconCmdName(cmd), this));
			}

			// The default response
			int[] responseData = new int[] {0x82};

			int unknown;
			switch (cmd)
			{
				case PSP_SYSCON_CMD_NOP:
					// Doing nothing
					break;
				case PSP_SYSCON_CMD_CTRL_LEPTON_POWER:
					UMDDrive.UmdPower = data[PSP_SYSCON_TX_DATA] != 0;
					break;
				case PSP_SYSCON_CMD_RESET_DEVICE:
					int device = data[PSP_SYSCON_TX_DATA] & 0x7F;
					bool reset = (data[PSP_SYSCON_TX_DATA] & 0x80) != 0;
					//if (log.DebugEnabled)
					{
						string deviceName = "Unknown";
						if (device == 2)
						{
							deviceName = "UMD Drive";
						}
						else if (device == 4)
						{
							deviceName = "WLAN";
						}
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("PSP_SYSCON_CMD_RESET_DEVICE device=0x%X(%s), reset=%b", device, deviceName, reset));
						Console.WriteLine(string.Format("PSP_SYSCON_CMD_RESET_DEVICE device=0x%X(%s), reset=%b", device, deviceName, reset));
					}
					break;
				case PSP_SYSCON_CMD_GET_DIGITAL_KEY:
					State.controller.hleControllerPoll();
					responseData = addButtonsResponseData(responseData, false);
					break;
				case PSP_SYSCON_CMD_GET_ANALOG:
					State.controller.hleControllerPoll();
					responseData = addAnalogResponseData(responseData);
					break;
				case PSP_SYSCON_CMD_GET_DIGITAL_KEY_ANALOG:
					State.controller.hleControllerPoll();
					responseData = addButtonsResponseData(responseData, false);
					responseData = addAnalogResponseData(responseData);
					break;
				case PSP_SYSCON_CMD_GET_KERNEL_DIGITAL_KEY:
					State.controller.hleControllerPoll();
					responseData = addButtonsResponseData(responseData, true);
					break;
				case PSP_SYSCON_CMD_GET_KERNEL_DIGITAL_KEY_ANALOG:
					State.controller.hleControllerPoll();
					responseData = addButtonsResponseData(responseData, true);
					responseData = addAnalogResponseData(responseData);
					break;
				case PSP_SYSCON_CMD_CTRL_ANALOG_XY_POLLING:
					Modules.sceCtrlModule.SamplingMode = data[PSP_SYSCON_TX_DATA];
					break;
				case PSP_SYSCON_CMD_CTRL_LED:
					int flag = data[PSP_SYSCON_TX_DATA];
					bool setOn;
					int led;
					if (Model.Model == Model.MODEL_PSP_GO)
					{
						setOn = (flag & 0x01) != 0;
						led = flag & 0xF0;
					}
					else
					{
						setOn = (flag & 0x10) != 0;
						led = flag & 0xE0;
					}

					switch (led)
					{
						case 0x40:
							LED.LedMemoryStickOn = setOn;
							break;
						case 0x80:
							LED.LedWlanOn = setOn;
							break;
						case 0x20:
							LED.LedPowerOn = setOn;
							break;
						case 0x10:
							LED.LedBluetoothOn = setOn;
							break;
						default:
							Console.WriteLine(string.Format("startSysconCmd PSP_SYSCON_CMD_CTRL_LED unknown flag value 0x{0:X2}", flag));
							break;
					}
					break;
				case PSP_SYSCON_CMD_RECEIVE_SETPARAM:
				{
					int parameterId = 0;
					// Depending on the Baryon version, there is a parameter or not
					if (data[PSP_SYSCON_TX_LEN] >= 3)
					{
						parameterId = data[PSP_SYSCON_TX_DATA];
					}

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("startSysconCmd PSP_SYSCON_CMD_RECEIVE_SETPARAM parameterId=0x{0:X}", parameterId));
					}

					// 8 bytes response data:
					// - 2 bytes scePowerGetForceSuspendCapacity() (usually 72)
					// - 6 bytes unknown
					responseData = addResponseData16(responseData, scePowerModule.scePowerGetForceSuspendCapacity());
					for (int i = 2; i < 8; i++)
					{
						responseData = Utilities.add(responseData, 0);
					}
					break;
				}
				case PSP_SYSCON_CMD_SEND_SETPARAM:
				{
					int parameterId = 0;
					if (data[PSP_SYSCON_TX_LEN] >= 11)
					{
						parameterId = data[PSP_SYSCON_TX_DATA + 10];
					}

					int forceSuspendCapacity = data[PSP_SYSCON_TX_DATA + 0];
					forceSuspendCapacity |= data[PSP_SYSCON_TX_DATA + 1] << 8;

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("startSysconCmd PSP_SYSCON_CMD_SEND_SETPARAM parameterId=0x{0:X}, forceSuspendCapacity=0x{1:X}", parameterId, forceSuspendCapacity));
					}
					break;
				}
				case PSP_SYSCON_CMD_CTRL_HR_POWER:
				{
					bool power = data[PSP_SYSCON_TX_DATA] != 0;
					Modules.sceSysconModule.sceSysconCtrlHRPower(power);
					break;
				}
				case PSP_SYSCON_CMD_CTRL_WLAN_POWER:
				{
					bool power = data[PSP_SYSCON_TX_DATA] != 0;
					Modules.sceSysconModule.sceSysconCtrlWlanPower(power);
					break;
				}
				case PSP_SYSCON_CMD_GET_POWER_SUPPLY_STATUS:
					responseData = addResponseData32(responseData, sceSysconModule.PowerSupplyStatus);
					break;
				case PSP_SYSCON_CMD_BATTERY_GET_STATUS_CAP:
					responseData = addResponseData16(responseData, sceSysconModule.BatteryStatusCap1);
					responseData = addResponseData16(responseData, sceSysconModule.BatteryStatusCap2);
					break;
				case PSP_SYSCON_CMD_BATTERY_GET_FULL_CAP:
					responseData = addResponseData32(responseData, Battery.FullCapacity);
					break;
				case PSP_SYSCON_CMD_BATTERY_GET_CYCLE:
					responseData = addResponseData32(responseData, sceSysconModule.BatteryCycle);
					break;
				case PSP_SYSCON_CMD_BATTERY_GET_LIMIT_TIME:
					responseData = addResponseData32(responseData, sceSysconModule.BatteryLimitTime);
					break;
				case PSP_SYSCON_CMD_BATTERY_GET_TEMP:
					responseData = addResponseData32(responseData, Battery.Temperature);
					break;
				case PSP_SYSCON_CMD_BATTERY_GET_ELEC:
					responseData = addResponseData32(responseData, sceSysconModule.BatteryElec);
					break;
				case PSP_SYSCON_CMD_BATTERY_GET_VOLT:
					responseData = addResponseData32(responseData, Battery.Voltage);
					break;
				case PSP_SYSCON_CMD_GET_BARYON:
					responseData = addResponseData32(responseData, sceSysconModule._sceSysconGetBaryonVersion());
					break;
				case PSP_SYSCON_CMD_GET_POMMEL_VERSION:
					responseData = addResponseData32(responseData, sceSysconModule.PommelVersion);
					break;
				case PSP_SYSCON_CMD_GET_POWER_STATUS:
					responseData = addResponseData32(responseData, sceSysconModule.PowerStatus);
					break;
				case PSP_SYSCON_CMD_GET_TIMESTAMP:
					int[] timeStamp = sceSysconModule.TimeStamp;
					for (int i = 0; i < timeStamp.Length; i++)
					{
						responseData = Utilities.add(responseData, timeStamp[i] & 0xFF);
					}
					break;
				case PSP_SYSCON_CMD_READ_SCRATCHPAD:
				{
					int src = (data[PSP_SYSCON_TX_DATA] & 0xFC) >> 2;
					int size = 1 << (data[PSP_SYSCON_TX_DATA] & 0x03);
					int[] values = new int[size];
					sceSysconModule.readScratchpad(src, values, size);
					for (int i = 0; i < size; i++)
					{
						responseData = Utilities.add(responseData, values[i] & 0xFF);
					}
					break;
				}
				case PSP_SYSCON_CMD_WRITE_SCRATCHPAD:
				{
					int dst = (data[PSP_SYSCON_TX_DATA] & 0xFC) >> 2;
					int size = 1 << (data[PSP_SYSCON_TX_DATA] & 0x03);
					int[] values = new int[size];
					for (int i = 0; i < size; i++)
					{
						values[i] = data[PSP_SYSCON_TX_DATA + 1 + i];
					}
					sceSysconModule.writeScratchpad(dst, values, size);
					break;
				}
				case PSP_SYSCON_CMD_READ_CLOCK:
					responseData = addResponseData32(responseData, sceSysconModule.readClock());
					break;
				case PSP_SYSCON_CMD_WRITE_CLOCK:
					int clock = getData32(data, PSP_SYSCON_TX_DATA);
					sceSysconModule.writeClock(clock);
					break;
				case PSP_SYSCON_CMD_READ_ALARM:
					responseData = addResponseData32(responseData, sceSysconModule.readAlarm());
					break;
				case PSP_SYSCON_CMD_WRITE_ALARM:
					int alarm = getData32(data, PSP_SYSCON_TX_DATA);
					sceSysconModule.writeAlarm(alarm);
					break;
				case PSP_SYSCON_CMD_CTRL_MS_POWER:
					bool power = getData32(data, PSP_SYSCON_TX_DATA) != 0;
					MemoryStick.MsPower = power;
					break;
				case PSP_SYSCON_CMD_CTRL_POWER:
					unknown = getData24(data, PSP_SYSCON_TX_DATA);
					sceSysconModule.sceSysconCtrlPower(unknown & 0x3FFFFF, (unknown >> 23) & 0x1);
					break;
				case PSP_SYSCON_CMD_CTRL_VOLTAGE:
					unknown = getData24(data, PSP_SYSCON_TX_DATA);
					sceSysconModule.sceSysconCtrlVoltage(unknown & 0xFF, (unknown >> 8) & 0xFFFF);
					break;
				default:
					Console.WriteLine(string.Format("startSysconCmd: unknown cmd=0x{0:X2}({1}), {2}", cmd, getSysconCmdName(cmd), this));
					break;
			}

			setResponseData(BaryonStatus, responseData, 0, responseData.Length);

			endSysconCmd();
		}

		private void endSysconCmd()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("endSysconCmd {0}", this));
			}
			MMIOHandlerGpio.Instance.Port = GPIO_PORT_SYSCON_END_CMD;
		}

		public virtual void setResponseData(int status, int[] responseData, int offset, int Length)
		{
			clearData();
			if (Length >= 0 && Length <= MAX_DATA_LENGTH - 3)
			{
				setDataValue(PSP_SYSCON_RX_STATUS, status);
				setDataValue(PSP_SYSCON_RX_LEN, Length + 2);
				for (int i = 0; i < Length; i++)
				{
					setDataValue(PSP_SYSCON_RX_RESPONSE + i, responseData[offset + i]);
				}
				addHashValue();
			}
		}

		private int readData16()
		{
			int value = ((data[dataIndex++] & 0xFF) << 8) | (data[dataIndex++] & 0xFF);
			if (dataIndex >= data[PSP_SYSCON_RX_LEN])
			{
				endDataIndex = true;
			}
			return value;
		}

		private void writeData16(int value)
		{
			data[dataIndex++] = (value >> 8) & 0xFF;
			data[dataIndex++] = value & 0xFF;
			if (dataIndex >= MAX_DATA_LENGTH)
			{
				endDataIndex = true;
			}
		}

		private int Flags0C
		{
			get
			{
				int flags = 0;
    
				if (endDataIndex)
				{
					dataIndex = 0;
					endDataIndex = false;
				}
				else
				{
					flags |= 4;
				}
    
				if (error == 0)
				{
					flags |= 1;
				}
    
				return flags;
			}
		}

		private int Flags04
		{
			set
			{
				if ((value & 4) != 0)
				{
					dataIndex = 0;
					endDataIndex = false;
				}
    
				if ((value & 2) != 0)
				{
					startSysconCmd();
				}
				else
				{
					MMIOHandlerGpio.Instance.clearPort(GPIO_PORT_SYSCON_END_CMD);
				}
			}
		}

		private int Flags20
		{
			set
			{
				// TODO Unknown value: clear error status?
				if ((value & 3) != 0)
				{
					error = 0;
				}
			}
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x08:
					value = readData16();
					break;
				case 0x0C:
					value = Flags0C;
					break;
				case 0x18:
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
					if (value != 0xCF)
					{
						base.write32(address, value);
					}
					break;
				case 0x04:
					Flags04 = value;
					break;
				case 0x08:
					writeData16(value);
					break;
				case 0x14:
					if (value != 0)
					{
						base.write32(address, value);
					}
					break;
				case 0x20:
					Flags20 = value;
					break;
				case 0x24:
					if (value != 0)
					{
						base.write32(address, value);
					}
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
			StringBuilder sb = new StringBuilder();
			sb.Append(string.Format("MMIOHandlerSyscon dataIndex=0x{0:X}, data: [", dataIndex));
			for (int i = 0; i < MAX_DATA_LENGTH; i++)
			{
				if (i > 0)
				{
					sb.Append(", ");
				}
				sb.Append(string.Format("0x{0:X2}", data[i]));
			}
			sb.Append("]");

			return sb.ToString();
		}
	}

}