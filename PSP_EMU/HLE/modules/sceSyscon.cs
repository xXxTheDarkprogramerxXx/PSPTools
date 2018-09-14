using System;

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
namespace pspsharp.HLE.modules
{

	using Logger = org.apache.log4j.Logger;

	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using Battery = pspsharp.hardware.Battery;
	using LED = pspsharp.hardware.LED;
	using MemoryStick = pspsharp.hardware.MemoryStick;
	using Model = pspsharp.hardware.Model;
	using UMDDrive = pspsharp.hardware.UMDDrive;
	using Wlan = pspsharp.hardware.Wlan;
	using Utilities = pspsharp.util.Utilities;

	public class sceSyscon : HLEModule
	{
		public static Logger log = Modules.getLogger("sceSyscon");
		public const int PSP_SYSCON_CMD_NOP = 0x00;
		public const int PSP_SYSCON_CMD_GET_BARYON = 0x01;
		public const int PSP_SYSCON_CMD_GET_DIGITAL_KEY = 0x02;
		public const int PSP_SYSCON_CMD_GET_ANALOG = 0x03;
		public const int PSP_SYSCON_CMD_GET_DIGITAL_KEY_ANALOG = 0x06;
		public const int PSP_SYSCON_CMD_GET_KERNEL_DIGITAL_KEY = 0x07;
		public const int PSP_SYSCON_CMD_GET_KERNEL_DIGITAL_KEY_ANALOG = 0x08;
		public const int PSP_SYSCON_CMD_READ_CLOCK = 0x09;
		public const int PSP_SYSCON_CMD_READ_ALARM = 0x0A;
		public const int PSP_SYSCON_CMD_GET_POWER_SUPPLY_STATUS = 0x0B;
		public const int PSP_SYSCON_CMD_GET_TACHYON_WDT_STATUS = 0x0C;
		public const int PSP_SYSCON_CMD_GET_BATT_VOLT = 0x0D;
		public const int PSP_SYSCON_CMD_GET_WAKE_UP_FACTOR = 0x0E;
		public const int PSP_SYSCON_CMD_GET_WAKE_UP_REQ = 0x0F;
		public const int PSP_SYSCON_CMD_GET_STATUS2 = 0x10;
		public const int PSP_SYSCON_CMD_GET_TIMESTAMP = 0x11;
		public const int PSP_SYSCON_CMD_GET_VIDEO_CABLE = 0x12;
		public const int PSP_SYSCON_CMD_WRITE_CLOCK = 0x20;
		public const int PSP_SYSCON_CMD_SET_USB_STATUS = 0x21;
		public const int PSP_SYSCON_CMD_WRITE_ALARM = 0x22;
		public const int PSP_SYSCON_CMD_WRITE_SCRATCHPAD = 0x23;
		public const int PSP_SYSCON_CMD_READ_SCRATCHPAD = 0x24;
		public const int PSP_SYSCON_CMD_SEND_SETPARAM = 0x25;
		public const int PSP_SYSCON_CMD_RECEIVE_SETPARAM = 0x26;
		public const int PSP_SYSCON_CMD_CTRL_BT_POWER_UNK1 = 0x29;
		public const int PSP_SYSCON_CMD_CTRL_BT_POWER_UNK2 = 0x2A;
		public const int PSP_SYSCON_CMD_CTRL_TACHYON_WDT = 0x31;
		public const int PSP_SYSCON_CMD_RESET_DEVICE = 0x32;
		public const int PSP_SYSCON_CMD_CTRL_ANALOG_XY_POLLING = 0x33;
		public const int PSP_SYSCON_CMD_CTRL_HR_POWER = 0x34;
		public const int PSP_SYSCON_CMD_GET_BATT_VOLT_AD = 0x37;
		public const int PSP_SYSCON_CMD_GET_POMMEL_VERSION = 0x40;
		public const int PSP_SYSCON_CMD_GET_POLESTAR_VERSION = 0x41;
		public const int PSP_SYSCON_CMD_CTRL_VOLTAGE = 0x42;
		public const int PSP_SYSCON_CMD_CTRL_POWER = 0x45;
		public const int PSP_SYSCON_CMD_GET_POWER_STATUS = 0x46;
		public const int PSP_SYSCON_CMD_CTRL_LED = 0x47;
		public const int PSP_SYSCON_CMD_WRITE_POMMEL_REG = 0x48;
		public const int PSP_SYSCON_CMD_READ_POMMEL_REG = 0x49;
		public const int PSP_SYSCON_CMD_CTRL_HDD_POWER = 0x4A;
		public const int PSP_SYSCON_CMD_CTRL_LEPTON_POWER = 0x4B;
		public const int PSP_SYSCON_CMD_CTRL_MS_POWER = 0x4C;
		public const int PSP_SYSCON_CMD_CTRL_WLAN_POWER = 0x4D;
		public const int PSP_SYSCON_CMD_WRITE_POLESTAR_REG = 0x4E;
		public const int PSP_SYSCON_CMD_READ_POLESTAR_REG = 0x4F;
		public const int PSP_SYSCON_CMD_CTRL_DVE_POWER = 0x52;
		public const int PSP_SYSCON_CMD_CTRL_BT_POWER = 0x53;
		public const int PSP_SYSCON_CMD_CTRL_USB_POWER = 0x55;
		public const int PSP_SYSCON_CMD_CTRL_CHARGE = 0x56;
		public const int PSP_SYSCON_CMD_BATTERY_NOP = 0x60;
		public const int PSP_SYSCON_CMD_BATTERY_GET_STATUS_CAP = 0x61;
		public const int PSP_SYSCON_CMD_BATTERY_GET_TEMP = 0x62;
		public const int PSP_SYSCON_CMD_BATTERY_GET_VOLT = 0x63;
		public const int PSP_SYSCON_CMD_BATTERY_GET_ELEC = 0x64;
		public const int PSP_SYSCON_CMD_BATTERY_GET_RCAP = 0x65;
		public const int PSP_SYSCON_CMD_BATTERY_GET_CAP = 0x66;
		public const int PSP_SYSCON_CMD_BATTERY_GET_FULL_CAP = 0x67;
		public const int PSP_SYSCON_CMD_BATTERY_GET_IFC = 0x68;
		public const int PSP_SYSCON_CMD_BATTERY_GET_LIMIT_TIME = 0x69;
		public const int PSP_SYSCON_CMD_BATTERY_GET_STATUS = 0x6A;
		public const int PSP_SYSCON_CMD_BATTERY_GET_CYCLE = 0x6B;
		public const int PSP_SYSCON_CMD_BATTERY_GET_SERIAL = 0x6C;
		public const int PSP_SYSCON_CMD_BATTERY_GET_INFO = 0x6D;
		public const int PSP_SYSCON_CMD_BATTERY_GET_TEMP_AD = 0x6E;
		public const int PSP_SYSCON_CMD_BATTERY_GET_VOLT_AD = 0x6F;
		public const int PSP_SYSCON_CMD_BATTERY_GET_ELEC_AD = 0x70;
		public const int PSP_SYSCON_CMD_BATTERY_GET_TOTAL_ELEC = 0x71;
		public const int PSP_SYSCON_CMD_BATTERY_GET_CHARGE_TIME = 0x72;
		private static string[] cmdNames;
		public const int PSP_SYSCON_LED_MS = 0; // Memory-Stick LED
		public const int PSP_SYSCON_LED_WLAN = 1; // W-LAN LED
		public const int PSP_SYSCON_LED_POWER = 2; // Power LED
		public const int PSP_SYSCON_LED_BT = 3; // Bluetooth LED (only PSP GO)
		private readonly int[] scratchPad = new int[32];
		private int alarm;

		public override void start()
		{
			Arrays.fill(scratchPad, 0);

			// Unknown 4-bytes value at offset 8
			int scratchPad8 = 0;
			for (int i = 0; i < 5; i++, scratchPad8 >>= 8)
			{
				scratchPad[i + 8] = scratchPad8 & 0xFF;
			}

			// 5-bytes value at offset 16, used to initialize the clock.
			// Set this value to 0 to force the clock initialization at boot time.
			long scratchPad16 = Modules.sceRtcModule.hleGetCurrentTick() >> 19;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Initializing scratchPad16=0x{0:X}", scratchPad16));
			}
			for (int i = 0; i < 5; i++, scratchPad16 >>= 8)
			{
				scratchPad[i + 16] = (int) scratchPad16 & 0xFF;
			}

			// Unknown 5-bytes value at offset 24
			long scratchPad24 = 0L;
			for (int i = 0; i < 5; i++, scratchPad24 >>= 8)
			{
				scratchPad[i + 24] = (int) scratchPad24 & 0xFF;
			}

			alarm = 0;

			base.start();
		}

		public static string getSysconCmdName(int cmd)
		{
			if (cmdNames == null)
			{
				cmdNames = new string[256];
				cmdNames[PSP_SYSCON_CMD_NOP] = "NOP";
				cmdNames[PSP_SYSCON_CMD_GET_BARYON] = "GET_BARYON";
				cmdNames[PSP_SYSCON_CMD_GET_DIGITAL_KEY] = "GET_DIGITAL_KEY";
				cmdNames[PSP_SYSCON_CMD_GET_ANALOG] = "GET_ANALOG";
				cmdNames[PSP_SYSCON_CMD_GET_DIGITAL_KEY_ANALOG] = "GET_DIGITAL_KEY_ANALOG";
				cmdNames[PSP_SYSCON_CMD_GET_KERNEL_DIGITAL_KEY] = "GET_KERNEL_DIGITAL_KEY";
				cmdNames[PSP_SYSCON_CMD_GET_KERNEL_DIGITAL_KEY_ANALOG] = "GET_KERNEL_DIGITAL_KEY_ANALOG";
				cmdNames[PSP_SYSCON_CMD_READ_CLOCK] = "READ_CLOCK";
				cmdNames[PSP_SYSCON_CMD_READ_ALARM] = "READ_ALARM";
				cmdNames[PSP_SYSCON_CMD_GET_POWER_SUPPLY_STATUS] = "GET_POWER_SUPPLY_STATUS";
				cmdNames[PSP_SYSCON_CMD_GET_TACHYON_WDT_STATUS] = "GET_TACHYON_WDT_STATUS";
				cmdNames[PSP_SYSCON_CMD_GET_BATT_VOLT] = "GET_BATT_VOLT";
				cmdNames[PSP_SYSCON_CMD_GET_WAKE_UP_FACTOR] = "GET_WAKE_UP_FACTOR";
				cmdNames[PSP_SYSCON_CMD_GET_WAKE_UP_REQ] = "GET_WAKE_UP_REQ";
				cmdNames[PSP_SYSCON_CMD_GET_STATUS2] = "GET_STATUS2";
				cmdNames[PSP_SYSCON_CMD_GET_TIMESTAMP] = "GET_TIMESTAMP";
				cmdNames[PSP_SYSCON_CMD_GET_VIDEO_CABLE] = "GET_VIDEO_CABLE";
				cmdNames[PSP_SYSCON_CMD_WRITE_CLOCK] = "WRITE_CLOCK";
				cmdNames[PSP_SYSCON_CMD_SET_USB_STATUS] = "SET_USB_STATUS";
				cmdNames[PSP_SYSCON_CMD_WRITE_ALARM] = "WRITE_ALARM";
				cmdNames[PSP_SYSCON_CMD_WRITE_SCRATCHPAD] = "WRITE_SCRATCHPAD";
				cmdNames[PSP_SYSCON_CMD_READ_SCRATCHPAD] = "READ_SCRATCHPAD";
				cmdNames[PSP_SYSCON_CMD_SEND_SETPARAM] = "SEND_SETPARAM";
				cmdNames[PSP_SYSCON_CMD_RECEIVE_SETPARAM] = "RECEIVE_SETPARAM";
				cmdNames[PSP_SYSCON_CMD_CTRL_BT_POWER_UNK1] = "CTRL_BT_POWER_UNK1";
				cmdNames[PSP_SYSCON_CMD_CTRL_BT_POWER_UNK2] = "CTRL_BT_POWER_UNK2";
				cmdNames[PSP_SYSCON_CMD_CTRL_TACHYON_WDT] = "CTRL_TACHYON_WDT";
				cmdNames[PSP_SYSCON_CMD_RESET_DEVICE] = "RESET_DEVICE";
				cmdNames[PSP_SYSCON_CMD_CTRL_ANALOG_XY_POLLING] = "CTRL_ANALOG_XY_POLLING";
				cmdNames[PSP_SYSCON_CMD_CTRL_HR_POWER] = "CTRL_HR_POWER";
				cmdNames[PSP_SYSCON_CMD_GET_BATT_VOLT_AD] = "GET_BATT_VOLT_AD";
				cmdNames[PSP_SYSCON_CMD_GET_POMMEL_VERSION] = "GET_POMMEL_VERSION";
				cmdNames[PSP_SYSCON_CMD_GET_POLESTAR_VERSION] = "GET_POLESTAR_VERSION";
				cmdNames[PSP_SYSCON_CMD_CTRL_VOLTAGE] = "CTRL_VOLTAGE";
				cmdNames[PSP_SYSCON_CMD_CTRL_POWER] = "CTRL_POWER";
				cmdNames[PSP_SYSCON_CMD_GET_POWER_STATUS] = "GET_POWER_STATUS";
				cmdNames[PSP_SYSCON_CMD_CTRL_LED] = "CTRL_LED";
				cmdNames[PSP_SYSCON_CMD_WRITE_POMMEL_REG] = "WRITE_POMMEL_REG";
				cmdNames[PSP_SYSCON_CMD_READ_POMMEL_REG] = "READ_POMMEL_REG";
				cmdNames[PSP_SYSCON_CMD_CTRL_HDD_POWER] = "CTRL_HDD_POWER";
				cmdNames[PSP_SYSCON_CMD_CTRL_LEPTON_POWER] = "CTRL_LEPTON_POWER";
				cmdNames[PSP_SYSCON_CMD_CTRL_MS_POWER] = "CTRL_MS_POWER";
				cmdNames[PSP_SYSCON_CMD_CTRL_WLAN_POWER] = "CTRL_WLAN_POWER";
				cmdNames[PSP_SYSCON_CMD_WRITE_POLESTAR_REG] = "WRITE_POLESTAR_REG";
				cmdNames[PSP_SYSCON_CMD_READ_POLESTAR_REG] = "READ_POLESTAR_REG";
				cmdNames[PSP_SYSCON_CMD_CTRL_DVE_POWER] = "CTRL_DVE_POWER";
				cmdNames[PSP_SYSCON_CMD_CTRL_BT_POWER] = "CTRL_BT_POWER";
				cmdNames[PSP_SYSCON_CMD_CTRL_USB_POWER] = "CTRL_USB_POWER";
				cmdNames[PSP_SYSCON_CMD_CTRL_CHARGE] = "CTRL_CHARGE";
				cmdNames[PSP_SYSCON_CMD_BATTERY_NOP] = "BATTERY_NOP";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_STATUS_CAP] = "BATTERY_GET_STATUS_CAP";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_TEMP] = "BATTERY_GET_TEMP";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_VOLT] = "BATTERY_GET_VOLT";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_ELEC] = "BATTERY_GET_ELEC";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_RCAP] = "BATTERY_GET_RCAP";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_CAP] = "BATTERY_GET_CAP";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_FULL_CAP] = "BATTERY_GET_FULL_CAP";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_IFC] = "BATTERY_GET_IFC";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_LIMIT_TIME] = "BATTERY_GET_LIMIT_TIME";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_STATUS] = "BATTERY_GET_STATUS";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_CYCLE] = "BATTERY_GET_CYCLE";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_SERIAL] = "BATTERY_GET_SERIAL";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_INFO] = "BATTERY_GET_INFO";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_TEMP_AD] = "BATTERY_GET_TEMP_AD";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_VOLT_AD] = "BATTERY_GET_VOLT_AD";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_ELEC_AD] = "BATTERY_GET_ELEC_AD";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_TOTAL_ELEC] = "BATTERY_GET_TOTAL_ELEC";
				cmdNames[PSP_SYSCON_CMD_BATTERY_GET_CHARGE_TIME] = "BATTERY_GET_CHARGE_TIME";

				for (int i = 0; i < cmdNames.Length; i++)
				{
					if (string.ReferenceEquals(cmdNames[i], null))
					{
						cmdNames[i] = string.Format("UNKNOWN_CMD_0x{0:X2}", i);
					}
				}
			}

			return cmdNames[cmd];
		}

		public virtual int PowerSupplyStatus
		{
			get
			{
				int powerSupplyStatus = 0xC0; // Unknown value
    
				if (Battery.Present)
				{
					powerSupplyStatus |= 0x02; // Flag indicating that there is a battery present
				}
    
				return powerSupplyStatus;
			}
		}

		public virtual int BatteryStatusCap1
		{
			get
			{
				return 0;
			}
		}

		public virtual int BatteryStatusCap2
		{
			get
			{
				return 0;
			}
		}

		public virtual int BatteryCycle
		{
			get
			{
				return 0;
			}
		}

		public virtual int BatteryLimitTime
		{
			get
			{
				return 0;
			}
		}

		public virtual int BatteryElec
		{
			get
			{
				return 0;
			}
		}

		public virtual int PommelVersion
		{
			get
			{
				return 0;
			}
		}

		public virtual int PowerStatus
		{
			get
			{
				return 0;
			}
		}

		public virtual int[] TimeStamp
		{
			get
			{
				return new int[12];
			}
		}

		public virtual void readScratchpad(int src, int[] values, int size)
		{
			Array.Copy(scratchPad, src, values, 0, size);
		}

		public virtual void writeScratchpad(int dst, int[] src, int size)
		{
		}

		public virtual int readClock()
		{
			return 0;
		}

		public virtual void writeClock(int clock)
		{
		}

		public virtual int readAlarm()
		{
			return alarm;
		}

		public virtual void writeAlarm(int alarm)
		{
			this.alarm = alarm;
		}

		/// <summary>
		/// Set the wlan switch callback, that will be ran when the wlan switch changes.
		/// </summary>
		/// <param name="callback"> The callback function. </param>
		/// <param name="argp"> The second argument that will be passed to the callback.
		/// </param>
		/// <returns> 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x50446BE5, version = 150) public int sceSysconSetWlanSwitchCallback(pspsharp.HLE.TPointer callback, int argp)
		[HLEFunction(nid : 0x50446BE5, version : 150)]
		public virtual int sceSysconSetWlanSwitchCallback(TPointer callback, int argp)
		{
			return 0;
		}

		/// <summary>
		/// Check if the battery is low.
		/// </summary>
		/// <returns> 1 if it is low, 0 otherwise. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1605847F, version = 150) public boolean sceSysconIsLowBattery()
		[HLEFunction(nid : 0x1605847F, version : 150)]
		public virtual bool sceSysconIsLowBattery()
		{
			return Battery.CurrentPowerPercent <= Battery.LowPercent;
		}

		/// <summary>
		/// Get the wlan switch state.
		/// </summary>
		/// <returns> 1 if wlan is activated, 0 otherwise. </returns>
		[HLEFunction(nid : 0x2D510164, version : 150)]
		public virtual int sceSysconGetWlanSwitch()
		{
			return Wlan.SwitchState;
		}

		[HLEFunction(nid : 0x0B51E34D, version : 150)]
		public virtual int sceSysconSetWlanSwitch(int switchState)
		{
			int oldSwitchState = Wlan.SwitchState;
			Wlan.SwitchState = switchState;

			return oldSwitchState;
		}

		/// <summary>
		/// Set the wlan power.
		/// </summary>
		/// <param name="power"> The new power value.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x48448373, version = 150) public int sceSysconCtrlWlanPower(boolean power)
		[HLEFunction(nid : 0x48448373, version : 150)]
		public virtual int sceSysconCtrlWlanPower(bool power)
		{
			return 0;
		}

		/// <summary>
		/// Get the wlan power status.
		/// </summary>
		/// <returns> 1 if the power is on, 0 otherwise. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7216917F, version = 150) public int sceSysconGetWlanPowerStatus()
		[HLEFunction(nid : 0x7216917F, version : 150)]
		public virtual int sceSysconGetWlanPowerStatus()
		{
			return 1;
		}

		/// <summary>
		/// Get the wake up req (?).
		/// </summary>
		/// <param name="req"> Pointer to a buffer where the req will be stored.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9D88A8DE, version = 150) public int sceSysconGetWakeUpReq(pspsharp.HLE.TPointer req)
		[HLEFunction(nid : 0x9D88A8DE, version : 150)]
		public virtual int sceSysconGetWakeUpReq(TPointer req)
		{
			return 0;
		}

		/// <summary>
		/// Get the baryon version.
		/// </summary>
		/// <returns> The baryon version. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFD5C58CB, version = 150) public int _sceSysconGetBaryonVersion()
		[HLEFunction(nid : 0xFD5C58CB, version : 150)]
		public virtual int _sceSysconGetBaryonVersion()
		{
			// Tachyon = 0x00140000, Baryon = 0x00030600 TA-079 v1 1g
			// Tachyon = 0x00200000, Baryon = 0x00030600 TA-079 v2 1g
			// Tachyon = 0x00200000, Baryon = 0x00040600 TA-079 v3 1g
			// Tachyon = 0x00300000, Baryon = 0x00040600 TA-081 1g
			// Tachyon = 0x00400000, Baryon = 0x00114000 TA-082 1g
			// Tachyon = 0x00400000, Baryon = 0x00121000 TA-086 1g
			// Tachyon = 0x00500000, Baryon = 0x0022B200 TA-085 2g
			// Tachyon = 0x00500000, Baryon = 0x00234000 TA-085 2g
			int baryon = 0;
			switch (Model.Model)
			{
				case Model.MODEL_PSP_FAT :
					baryon = 0x00030600;
					break;
				case Model.MODEL_PSP_SLIM:
					baryon = 0x0022B200;
					break;
				default:
					log.warn(string.Format("_sceSysconGetBaryonVersion unknown baryon version for PSP Model {0}", Model.getModelName(Model.Model)));
					break;
			}
			return baryon;
		}

		/// <summary>
		/// Get the baryon version from the syscon.
		/// </summary>
		/// <param name="baryonVersionAddr"> Pointer to a s32 where the baryon version will be stored. </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7EC5A957, version = 150) public int sceSysconGetBaryonVersion(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 baryonVersionAddr)
		[HLEFunction(nid : 0x7EC5A957, version : 150)]
		public virtual int sceSysconGetBaryonVersion(TPointer32 baryonVersionAddr)
		{
			int baryon = _sceSysconGetBaryonVersion();
			baryonVersionAddr.setValue(baryon);

			return 0;
		}

		/// <summary>
		/// Reset the device.
		/// </summary>
		/// <param name="reset"> The reset value, passed to the syscon. </param>
		/// <param name="mode"> The resetting mode (?).
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8CBC7987, version = 150) public int sceSysconResetDevice(int reset, int mode)
		[HLEFunction(nid : 0x8CBC7987, version : 150)]
		public virtual int sceSysconResetDevice(int reset, int mode)
		{
			return 0;
		}

		/// <summary>
		/// Get the Memory Stick power control state.
		/// </summary>
		/// <returns> 1 if powered, 0 otherwise </returns>
		[HLEFunction(nid : 0x7672103B, version : 150)]
		public virtual bool sceSysconGetMsPowerCtrl()
		{
			return MemoryStick.hasMsPower();
		}

		/// <summary>
		/// Set the memory stick power.
		/// </summary>
		/// <param name="power"> The new power value.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1088ABA8, version = 150) public int sceSysconCtrlMsPower(boolean power)
		[HLEFunction(nid : 0x1088ABA8, version : 150)]
		public virtual int sceSysconCtrlMsPower(bool power)
		{
			MemoryStick.MsPower = power;

			return 0;
		}

		/// <summary>
		/// Get the UMD drive power control state.
		/// </summary>
		/// <returns> 1 if powered, 0 otherwise </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x577C5771, version = 660) public boolean sceSysconGetLeptonPowerCtrl()
		[HLEFunction(nid : 0x577C5771, version : 660)]
		public virtual bool sceSysconGetLeptonPowerCtrl()
		{
			return UMDDrive.hasUmdPower();
		}

		/// <summary>
		/// Set the lepton power.
		/// </summary>
		/// <param name="power"> The new power value.
		/// </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8A4519F5, version = 660) public int sceSysconCtrlLeptonPower(boolean power)
		[HLEFunction(nid : 0x8A4519F5, version : 660)]
		public virtual int sceSysconCtrlLeptonPower(bool power)
		{
			UMDDrive.UmdPower = power;

			return 0;
		}

		/// <summary>
		/// Execute synchronously a syscon packet.
		/// </summary>
		/// <param name="packet">   The packet to execute. Its tx member needs to be initialized. </param>
		/// <param name="flags">    The packet flags. Check SceSysconPacketFlags. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5B9ACC97, version = 150) public int sceSysconCmdExec(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=96, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer packet, int flags)
		[HLEFunction(nid : 0x5B9ACC97, version : 150)]
		public virtual int sceSysconCmdExec(TPointer packet, int flags)
		{
			int cmd = packet.getValue8(12);
			int len = packet.getValue8(13);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceSysconCmdExec cmd=0x{0:X2}, len=0x{1:X2}, txData: {2}", cmd, len, Utilities.getMemoryDump(packet.Address + 14, len - 2)));
			}
			return 0;
		}

		/// <summary>
		/// Execute asynchronously a syscon packet.
		/// </summary>
		/// <param name="packet">   The packet to execute. Its tx member needs to be initialized. </param>
		/// <param name="flags">    The packet flags. Check SceSysconPacketFlags. </param>
		/// <param name="callback"> The packet callback. Check the callback member of SceSysconPacket. </param>
		/// <param name="argp">     The second argument that will be passed to the callback when executed. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3AC3D2A4, version = 150) public int sceSysconCmdExecAsync(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=96, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer packet, int flags, pspsharp.HLE.TPointer callback, int argp)
		[HLEFunction(nid : 0x3AC3D2A4, version : 150)]
		public virtual int sceSysconCmdExecAsync(TPointer packet, int flags, TPointer callback, int argp)
		{
			int cmd = packet.getValue8(12);
			int len = packet.getValue8(13);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceSysconCmdExecAsync cmd=0x{0:X2}, len=0x{1:X2}, txData: {2}", cmd, len, Utilities.getMemoryDump(packet.Address + 14, len - 2)));
			}
			return 0;
		}

		/// <summary>
		/// Get the baryon timestamp string.
		/// </summary>
		/// <param name="timeStampAddr"> A pointer to a string at least 12 bytes long. </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7BCC5EAE, version = 150) public int sceSysconGetTimeStamp(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=12, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer timeStampAddr)
		[HLEFunction(nid : 0x7BCC5EAE, version : 150)]
		public virtual int sceSysconGetTimeStamp(TPointer timeStampAddr)
		{
			int[] timeStamp = TimeStamp;
			for (int i = 0; i < timeStamp.Length; i++)
			{
				timeStampAddr.setValue8(i, (sbyte) timeStamp[i]);
			}

			return 0;
		}

		/// <summary>
		/// Get the pommel version.
		/// </summary>
		/// <param name="pommelAddr"> Pointer to a s32 where the pommel version will be stored. </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE7E87741, version = 150) public int sceSysconGetPommelVersion(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 pommelAddr)
		[HLEFunction(nid : 0xE7E87741, version : 150)]
		public virtual int sceSysconGetPommelVersion(TPointer32 pommelAddr)
		{
			pommelAddr.setValue(PommelVersion);

			return 0;
		}

		/// <summary>
		/// Get the power status.
		/// </summary>
		/// <param name="statusAddr"> Pointer to a s32 where the power status will be stored. </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x28363C97, version = 150) public int sceSysconGetPowerStatus(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 statusAddr)
		[HLEFunction(nid : 0x28363C97, version : 150)]
		public virtual int sceSysconGetPowerStatus(TPointer32 statusAddr)
		{
			statusAddr.setValue(PowerStatus);

			return 0;
		}

		/// <summary>
		/// Read data from the scratchpad.
		/// </summary>
		/// <param name="src">  The scratchpad address to read from. </param>
		/// <param name="dst">  A pointer where will be copied the read data. </param>
		/// <param name="size"> The size of the data to read from the scratchpad. </param>
		/// <returns> 0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEB277C88, version = 150) public int sceSysconReadScratchPad(int src, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer dst, int size)
		[HLEFunction(nid : 0xEB277C88, version : 150)]
		public virtual int sceSysconReadScratchPad(int src, TPointer dst, int size)
		{
			int[] values = new int[size];
			readScratchpad(src, values, size);
			for (int i = 0; i < scratchPad.Length; i++)
			{
				dst.setValue8(i, (sbyte) values[i]);
			}

			return 0;
		}

		/// <summary>
		/// Write data to the scratchpad.
		/// </summary>
		/// <param name="dst">  The scratchpad address to write to. </param>
		/// <param name="src">  A pointer to the data to copy to the scratchpad. </param>
		/// <param name="size"> The size of the data to copy. </param>
		/// <returns>     0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x65EB6096, version = 150) public int sceSysconWriteScratchPad(int dst, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer src, int size)
		[HLEFunction(nid : 0x65EB6096, version : 150)]
		public virtual int sceSysconWriteScratchPad(int dst, TPointer src, int size)
		{
			int[] values = new int[size];
			for (int i = 0; i < size; i++)
			{
				values[i] = src.getValue8(i);
			}
			writeScratchpad(dst, values, size);

			return 0;
		}

		/// <summary>
		/// Control an LED.
		/// </summary>
		/// <param name="led">   The led to toggle (PSP_SYSCON_LED_xxx) </param>
		/// <param name="state"> Whether to turn on or off </param>
		/// <returns>      < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x18BFBE65, version = 150) public int sceSysconCtrlLED(int led, boolean state)
		[HLEFunction(nid : 0x18BFBE65, version : 150)]
		public virtual int sceSysconCtrlLED(int led, bool state)
		{
			switch (led)
			{
				case PSP_SYSCON_LED_MS:
					LED.LedMemoryStickOn = state;
					break;
				case PSP_SYSCON_LED_WLAN:
					LED.LedWlanOn = state;
					break;
				case PSP_SYSCON_LED_POWER:
					LED.LedPowerOn = state;
					break;
				case PSP_SYSCON_LED_BT:
					LED.LedBluetoothOn = state;
					break;
				default:
					return SceKernelErrors.ERROR_INVALID_INDEX;
			}

			return 0;
		}

		/// <summary>
		/// Receive a parameter (used by power).
		/// </summary>
		/// <param name="id">    The parameter ID. </param>
		/// <param name="param"> Pointer to a buffer (length 8) where will be copied the parameter. </param>
		/// <returns>      0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x08234E6D, version = 150) public int sceSysconReceiveSetParam(int id, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=8, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer param)
		[HLEFunction(nid : 0x08234E6D, version : 150)]
		public virtual int sceSysconReceiveSetParam(int id, TPointer param)
		{
			return 0;
		}

		/// <summary>
		/// Set a parameter (used by power).
		/// </summary>
		/// <param name="id">    The parameter ID. </param>
		/// <param name="param"> Pointer to a buffer (length 8) the parameter will be set to. </param>
		/// <returns>      0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x992C22C2, version = 150) public int sceSysconSendSetParam(int id, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=8, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer param)
		[HLEFunction(nid : 0x992C22C2, version : 150)]
		public virtual int sceSysconSendSetParam(int id, TPointer param)
		{
			return 0;
		}

		/// <summary>
		/// Control the remote control power.
		/// </summary>
		/// <param name="power">  1 is on, 0 is off </param>
		/// <returns>       < 0 on error  </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x44439604, version = 150) public int sceSysconCtrlHRPower(boolean power)
		[HLEFunction(nid : 0x44439604, version : 150)]
		public virtual int sceSysconCtrlHRPower(bool power)
		{
			return 0;
		}

		/// <summary>
		/// Get the power supply status.
		/// </summary>
		/// <param name="statusAddr"> Pointer to a s32 where the power supply status will be stored. </param>
		/// <returns>           0 on success.  </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFC32141A, version = 150) public int sceSysconGetPowerSupplyStatus(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 statusAddr)
		[HLEFunction(nid : 0xFC32141A, version : 150)]
		public virtual int sceSysconGetPowerSupplyStatus(TPointer32 statusAddr)
		{
			statusAddr.setValue(PowerSupplyStatus);
			return 0;
		}

		/// <summary>
		/// Get the power supply status.
		/// </summary>
		/// <param name="statusAddr"> Pointer to a s32 where the power supply status will be stored. </param>
		/// <returns>           0 on success.  </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x22240B41, version = 660) public int sceSysconGetPowerSupplyStatus_660(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 statusAddr)
		[HLEFunction(nid : 0x22240B41, version : 660)]
		public virtual int sceSysconGetPowerSupplyStatus_660(TPointer32 statusAddr)
		{
			return sceSysconGetPowerSupplyStatus(statusAddr);
		}

		/// <summary>
		/// Get the battery status cap.
		/// </summary>
		/// <param name="unknown1"> Pointer to an unknown s32 where a value will be stored. </param>
		/// <param name="unknown2"> Pointer to an unknown s32 where a value will be stored. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6A53F3F8, version = 150) public int sceSysconBatteryGetStatusCap(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown1, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown2)
		[HLEFunction(nid : 0x6A53F3F8, version : 150)]
		public virtual int sceSysconBatteryGetStatusCap(TPointer32 unknown1, TPointer32 unknown2)
		{
			unknown1.setValue(BatteryStatusCap1);
			unknown2.setValue(BatteryStatusCap2);
			return 0;
		}

		/// <summary>
		/// Get the battery status cap.
		/// </summary>
		/// <param name="unknown1"> Pointer to an unknown s32 where a value will be stored. </param>
		/// <param name="unknown2"> Pointer to an unknown s32 where a value will be stored. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x85F5F601, version = 660) public int sceSysconBatteryGetStatusCap_660(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown1, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown2)
		[HLEFunction(nid : 0x85F5F601, version : 660)]
		public virtual int sceSysconBatteryGetStatusCap_660(TPointer32 unknown1, TPointer32 unknown2)
		{
			return sceSysconBatteryGetStatusCap(unknown1, unknown2);
		}

		/// <summary>
		/// Get the battery full capacity.
		/// </summary>
		/// <param name="capAddr"> Pointer to a s32 where the capacity will be stored. </param>
		/// <returns>        0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x71135D7D, version = 150) public int sceSysconBatteryGetFullCap(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 capAddr)
		[HLEFunction(nid : 0x71135D7D, version : 150)]
		public virtual int sceSysconBatteryGetFullCap(TPointer32 capAddr)
		{
			capAddr.setValue(Battery.FullCapacity);
			return 0;
		}

		/// <summary>
		/// Get the battery full capacity.
		/// </summary>
		/// <param name="capAddr"> Pointer to a s32 where the capacity will be stored. </param>
		/// <returns>        0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4C871BEA, version = 660) public int sceSysconBatteryGetFullCap_660(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 capAddr)
		[HLEFunction(nid : 0x4C871BEA, version : 660)]
		public virtual int sceSysconBatteryGetFullCap_660(TPointer32 capAddr)
		{
			return sceSysconBatteryGetFullCap(capAddr);
		}

		/// <summary>
		/// Get the battery cycle.
		/// </summary>
		/// <param name="cycleAddr"> Pointer to a s32 where the cycle will be stored. </param>
		/// <returns>          0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB5105D51, version = 150) public int sceSysconBatteryGetCycle(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 cycleAddr)
		[HLEFunction(nid : 0xB5105D51, version : 150)]
		public virtual int sceSysconBatteryGetCycle(TPointer32 cycleAddr)
		{
			cycleAddr.setValue(BatteryCycle);
			return 0;
		}

		/// <summary>
		/// Get the battery cycle.
		/// </summary>
		/// <param name="cycleAddr"> Pointer to a s32 where the cycle will be stored. </param>
		/// <returns>          0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x68AF19F1, version = 660) public int sceSysconBatteryGetCycle_660(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 cycleAddr)
		[HLEFunction(nid : 0x68AF19F1, version : 660)]
		public virtual int sceSysconBatteryGetCycle_660(TPointer32 cycleAddr)
		{
			return sceSysconBatteryGetCycle(cycleAddr);
		}

		/// <summary>
		/// Get the battery limit time.
		/// </summary>
		/// <param name="timeAddr"> Pointer to a s32 where the limit time will be stored. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x284FE366, version = 150) public int sceSysconBatteryGetLimitTime(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 timeAddr)
		[HLEFunction(nid : 0x284FE366, version : 150)]
		public virtual int sceSysconBatteryGetLimitTime(TPointer32 timeAddr)
		{
			timeAddr.setValue(BatteryLimitTime);
			return 0;
		}

		/// <summary>
		/// Get the battery limit time.
		/// </summary>
		/// <param name="timeAddr"> Pointer to a s32 where the limit time will be stored. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4D5A19BB, version = 660) public int sceSysconBatteryGetLimitTime_660(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 timeAddr)
		[HLEFunction(nid : 0x4D5A19BB, version : 660)]
		public virtual int sceSysconBatteryGetLimitTime_660(TPointer32 timeAddr)
		{
			return sceSysconBatteryGetLimitTime(timeAddr);
		}

		/// <summary>
		/// Get the battery temperature.
		/// </summary>
		/// <param name="tempAddr"> Pointer to a s32 where the temperature will be stored. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x70C10E61, version = 150) public int sceSysconBatteryGetTemp(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 tempAddr)
		[HLEFunction(nid : 0x70C10E61, version : 150)]
		public virtual int sceSysconBatteryGetTemp(TPointer32 tempAddr)
		{
			tempAddr.setValue(Battery.Temperature);
			return 0;
		}

		/// <summary>
		/// Get the battery temperature.
		/// </summary>
		/// <param name="tempAddr"> Pointer to a s32 where the temperature will be stored. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCE8B6633, version = 150) public int sceSysconBatteryGetTemp_660(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 tempAddr)
		[HLEFunction(nid : 0xCE8B6633, version : 150)]
		public virtual int sceSysconBatteryGetTemp_660(TPointer32 tempAddr)
		{
			return sceSysconBatteryGetTemp(tempAddr);
		}

		/// <summary>
		/// Get the battery electric charge.
		/// </summary>
		/// <param name="elecAddr"> Pointer to a s32 where the charge will be stored. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x373EC933, version = 150) public int sceSysconBatteryGetElec(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 elecAddr)
		[HLEFunction(nid : 0x373EC933, version : 150)]
		public virtual int sceSysconBatteryGetElec(TPointer32 elecAddr)
		{
			elecAddr.setValue(BatteryElec);
			return 0;
		}

		/// <summary>
		/// Get the battery electric charge.
		/// </summary>
		/// <param name="elecAddr"> Pointer to a s32 where the charge will be stored. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x483088B0, version = 660) public int sceSysconBatteryGetElec_660(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 elecAddr)
		[HLEFunction(nid : 0x483088B0, version : 660)]
		public virtual int sceSysconBatteryGetElec_660(TPointer32 elecAddr)
		{
			return sceSysconBatteryGetElec(elecAddr);
		}

		/// <summary>
		/// Get the battery voltage.
		/// </summary>
		/// <param name="voltAddr"> Pointer to a s32 where the voltage will be stored. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8BDEBB1E, version = 150) public int sceSysconBatteryGetVolt(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 voltAddr)
		[HLEFunction(nid : 0x8BDEBB1E, version : 150)]
		public virtual int sceSysconBatteryGetVolt(TPointer32 voltAddr)
		{
			voltAddr.setValue(Battery.Voltage);
			return 0;
		}

		/// <summary>
		/// Get the battery voltage.
		/// </summary>
		/// <param name="voltAddr"> Pointer to a s32 where the voltage will be stored. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA7DB34BB, version = 660) public int sceSysconBatteryGetVolt_660(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 voltAddr)
		[HLEFunction(nid : 0xA7DB34BB, version : 660)]
		public virtual int sceSysconBatteryGetVolt_660(TPointer32 voltAddr)
		{
			return sceSysconBatteryGetVolt(voltAddr);
		}

		/// <summary>
		/// Read the PSP clock.
		/// </summary>
		/// <param name="clockAddr"> Pointer to a s32 where the clock will be stored. </param>
		/// <returns>          0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC4D66C1D, version = 150) public int sceSysconReadClock(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 clockAddr)
		[HLEFunction(nid : 0xC4D66C1D, version : 150)]
		public virtual int sceSysconReadClock(TPointer32 clockAddr)
		{
			clockAddr.setValue(readClock());
			return 0;
		}

		/// <summary>
		/// Read the PSP clock.
		/// </summary>
		/// <param name="clockAddr"> Pointer to a s32 where the clock will be stored. </param>
		/// <returns>          0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF436BB12, version = 150) public int sceSysconReadClock_660(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 clockAddr)
		[HLEFunction(nid : 0xF436BB12, version : 150)]
		public virtual int sceSysconReadClock_660(TPointer32 clockAddr)
		{
			return sceSysconReadClock(clockAddr);
		}

		/// <summary>
		/// Read the PSP alarm.
		/// </summary>
		/// <param name="alarmAddr"> Pointer to a s32 where the alarm will be stored. </param>
		/// <returns>          0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7A805EE4, version = 150) public int sceSysconReadAlarm(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 alarmAddr)
		[HLEFunction(nid : 0x7A805EE4, version : 150)]
		public virtual int sceSysconReadAlarm(TPointer32 alarmAddr)
		{
			alarmAddr.setValue(readAlarm());
			return 0;
		}

		/// <summary>
		/// Read the PSP alarm.
		/// </summary>
		/// <param name="alarmAddr"> Pointer to a s32 where the alarm will be stored. </param>
		/// <returns>          0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF2AE6D5E, version = 660) public int sceSysconReadAlarm_660(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 alarmAddr)
		[HLEFunction(nid : 0xF2AE6D5E, version : 660)]
		public virtual int sceSysconReadAlarm_660(TPointer32 alarmAddr)
		{
			return sceSysconReadAlarm(alarmAddr);
		}

		/// <summary>
		/// Set the PSP alarm.
		/// </summary>
		/// <param name="alarm"> The alarm value to set the PSP alarm to. </param>
		/// <returns>      0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6C911742, version = 150) public int sceSysconWriteAlarm(int alarm)
		[HLEFunction(nid : 0x6C911742, version : 150)]
		public virtual int sceSysconWriteAlarm(int alarm)
		{
			writeAlarm(alarm);
			return 0;
		}

		/// <summary>
		/// Set the PSP alarm.
		/// </summary>
		/// <param name="alarm"> The alarm value to set the PSP alarm to. </param>
		/// <returns>      0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x80711575, version = 150) public int sceSysconWriteAlarm_660(int alarm)
		[HLEFunction(nid : 0x80711575, version : 150)]
		public virtual int sceSysconWriteAlarm_660(int alarm)
		{
			return sceSysconWriteAlarm(alarm);
		}

		/// <summary>
		/// Send a command to the syscon doing nothing.
		/// </summary>
		/// <returns>      0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE6B74CB9, version = 150) public int sceSysconNop()
		[HLEFunction(nid : 0xE6B74CB9, version : 150)]
		public virtual int sceSysconNop()
		{
			return 0;
		}

		/// <summary>
		/// Set the low battery callback, that will be ran when the battery is low.
		/// </summary>
		/// <param name="callback">         The callback function. </param>
		/// <param name="callbackArgument"> The second argument that will be passed to the callback. </param>
		/// <returns>                 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAD555CE5, version = 150) public int sceSysconSetLowBatteryCallback(pspsharp.HLE.TPointer callback, int callbackArgument)
		[HLEFunction(nid : 0xAD555CE5, version : 150)]
		public virtual int sceSysconSetLowBatteryCallback(TPointer callback, int callbackArgument)
		{
			return 0;
		}

		/// <summary>
		/// Set the low battery callback, that will be ran when the battery is low.
		/// </summary>
		/// <param name="callback">         The callback function. </param>
		/// <param name="callbackArgument"> The second argument that will be passed to the callback. </param>
		/// <returns>                 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x599EB8A0, version = 660) public int sceSysconSetLowBatteryCallback_660(pspsharp.HLE.TPointer callback, int callbackArgument)
		[HLEFunction(nid : 0x599EB8A0, version : 660)]
		public virtual int sceSysconSetLowBatteryCallback_660(TPointer callback, int callbackArgument)
		{
			return sceSysconSetLowBatteryCallback(callback, callbackArgument);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA3406117, version = 150) public boolean sceSysconIsAcSupplied()
		[HLEFunction(nid : 0xA3406117, version : 150)]
		public virtual bool sceSysconIsAcSupplied()
		{
			// Has no parameters
			return true;
		}

		/// <summary>
		/// Set the Ac supply callback, that will be ran when the PSP Ac power is (dis)connected (probably).
		/// </summary>
		/// <param name="callback">         The callback function. </param>
		/// <param name="callbackArgument"> The second argument that will be passed to the callback. </param>
		/// <returns>                 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE540E532, version = 150) public int sceSysconSetAcSupplyCallback(pspsharp.HLE.TPointer callback, int callbackArgument)
		[HLEFunction(nid : 0xE540E532, version : 150)]
		public virtual int sceSysconSetAcSupplyCallback(TPointer callback, int callbackArgument)
		{
			return 0;
		}

		/// <summary>
		/// Set the Ac supply callback, that will be ran when the PSP Ac power is (dis)connected (probably).
		/// </summary>
		/// <param name="callback">         The callback function. </param>
		/// <param name="callbackArgument"> The second argument that will be passed to the callback. </param>
		/// <returns>                 0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x657DCEF7, version = 660) public int sceSysconSetAcSupplyCallback_660(pspsharp.HLE.TPointer callback, int callbackArgument)
		[HLEFunction(nid : 0x657DCEF7, version : 660)]
		public virtual int sceSysconSetAcSupplyCallback_660(TPointer callback, int callbackArgument)
		{
			return sceSysconSetAcSupplyCallback(callback, callbackArgument);
		}

		/// <summary>
		/// Set the power control
		/// </summary>
		/// <param name="unknown1"> Unknown. </param>
		/// <param name="unknown2"> Unknown. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBE27FE66, version = 150) public int sceSysconCtrlPower(int unknown1, int unknown2)
		[HLEFunction(nid : 0xBE27FE66, version : 150)]
		public virtual int sceSysconCtrlPower(int unknown1, int unknown2)
		{
			return 0;
		}

		/// <summary>
		/// Set the power control
		/// </summary>
		/// <param name="unknown1"> Unknown. </param>
		/// <param name="unknown2"> Unknown. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEDD3AB8B, version = 660) public int sceSysconCtrlPower_660(int unknown1, int unknown2)
		[HLEFunction(nid : 0xEDD3AB8B, version : 660)]
		public virtual int sceSysconCtrlPower_660(int unknown1, int unknown2)
		{
			return sceSysconCtrlPower(unknown1, unknown2);
		}

		/// <summary>
		/// Set the voltage.
		/// </summary>
		/// <param name="unknown1"> Unknown. </param>
		/// <param name="unknown2"> Unknown. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x01677F91, version = 150) public int sceSysconCtrlVoltage(int unknown1, int unknown2)
		[HLEFunction(nid : 0x01677F91, version : 150)]
		public virtual int sceSysconCtrlVoltage(int unknown1, int unknown2)
		{
			return 0;
		}

		/// <summary>
		/// Set the voltage.
		/// </summary>
		/// <param name="unknown1"> Unknown. </param>
		/// <param name="unknown2"> Unknown. </param>
		/// <returns>         0 on success. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF7BCD2A6, version = 660) public int sceSysconCtrlVoltage_660(int unknown1, int unknown2)
		[HLEFunction(nid : 0xF7BCD2A6, version : 660)]
		public virtual int sceSysconCtrlVoltage_660(int unknown1, int unknown2)
		{
			return sceSysconCtrlVoltage(unknown1, unknown2);
		}
	}

}