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
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using Battery = pspsharp.hardware.Battery;
	using Model = pspsharp.hardware.Model;

	//using Logger = org.apache.log4j.Logger;

	public class scePower : HLEModule
	{
		//public static Logger log = Modules.getLogger("scePower");

		/// <summary>
		/// Power callback flags
		/// </summary>
		// indicates the power switch it pushed, putting the unit into suspend mode
		public const int PSP_POWER_CB_POWER_SWITCH = unchecked((int)0x80000000);
		// indicates the hold switch is on
		public const int PSP_POWER_CB_HOLD_SWITCH = 0x40000000;
		// what is standby mode?
		public const int PSP_POWER_CB_STANDBY = 0x00080000;
		// indicates the resume process has been completed (only seems to be triggered when another event happens)
		public const int PSP_POWER_CB_RESUME_COMPLETE = 0x00040000;
		// indicates the unit is resuming from suspend mode
		public const int PSP_POWER_CB_RESUMING = 0x00020000;
		// indicates the unit is suspending, seems to occur due to inactivity
		public const int PSP_POWER_CB_SUSPENDING = 0x00010000;
		// indicates the unit is plugged into an AC outlet
		public const int PSP_POWER_CB_AC_POWER = 0x00001000;
		// indicates the battery charge level is low
		public const int PSP_POWER_CB_BATTERY_LOW = 0x00000100;
		// indicates there is a battery present in the unit
		public const int PSP_POWER_CB_BATTERY_EXIST = 0x00000080;
		// unknown
		public const int PSP_POWER_CB_BATTPOWER = 0x0000007F;

		/// <summary>
		/// Power callback slots
		/// </summary>
		public const int PSP_POWER_CB_SLOT_AUTO = -1;
		protected internal int[] powerCBSlots = new int[16];

		// PLL clock:
		// Operates at fixed rates of 148MHz, 190MHz, 222MHz, 266MHz, 333MHz.
		// Starts at 222MHz.
		protected internal int pllClock = 222;
		// CPU clock:
		// Operates at variable rates from 1MHz to 333MHz.
		// Starts at 222MHz.
		// Note: Cannot have a higher frequency than the PLL clock's frequency.
		protected internal int cpuClock = 222;
		// BUS clock:
		// Operates at variable rates from 37MHz to 166MHz.
		// Starts at 111MHz.
		// Note: Cannot have a higher frequency than 1/2 of the PLL clock's frequency
		// or lower than 1/4 of the PLL clock's frequency.
		protected internal int busClock = 111;
		protected internal const int backlightMaximum = 4;
		protected internal int tachyonVoltage1;
		protected internal int tachyonVoltage2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2B51FE2F, version = 150) public int scePower_2B51FE2F()
		[HLEFunction(nid : 0x2B51FE2F, version : 150)]
		public virtual int scePower_2B51FE2F()
		{
			return 0;
		}

		[HLEFunction(nid : 0x442BFBAC, version : 150)]
		public virtual int scePowerGetBacklightMaximum()
		{
			return backlightMaximum;
		}

		[HLELogging(level:"trace"), HLEFunction(nid : 0xEFD3C963, version : 150)]
		public virtual int scePowerTick(int flag)
		{
			return Modules.sceSuspendForUserModule.hleKernelPowerTick(flag);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEDC13FE5, version = 150) public int scePowerGetIdleTimer()
		[HLEFunction(nid : 0xEDC13FE5, version : 150)]
		public virtual int scePowerGetIdleTimer()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7F30B3B1, version = 150) public int scePowerIdleTimerEnable()
		[HLEFunction(nid : 0x7F30B3B1, version : 150)]
		public virtual int scePowerIdleTimerEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x972CE941, version = 150) public int scePowerIdleTimerDisable()
		[HLEFunction(nid : 0x972CE941, version : 150)]
		public virtual int scePowerIdleTimerDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x27F3292C, version = 150) public int scePowerBatteryUpdateInfo()
		[HLEFunction(nid : 0x27F3292C, version : 150)]
		public virtual int scePowerBatteryUpdateInfo()
		{
			return 0;
		}

		[HLEFunction(nid : 0xE8E4E204, version : 150)]
		public virtual int scePowerGetForceSuspendCapacity()
		{
			int forceSuspendCapacity = (Battery.ForceSuspendPercent * Battery.FullCapacity) / 100;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetForceSuspendCapacity returning {0:D} mAh", forceSuspendCapacity));
			}

			return forceSuspendCapacity;
		}

		[HLEFunction(nid : 0xB999184C, version : 150)]
		public virtual int scePowerGetLowBatteryCapacity()
		{
			int lowBatteryCapacity = (Battery.LowPercent * Battery.FullCapacity) / 100;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetLowBatteryCapacity returning {0:D} mAh", lowBatteryCapacity));
			}

			return lowBatteryCapacity;
		}

		[HLEFunction(nid : 0x87440F5E, version : 150)]
		public virtual bool scePowerIsPowerOnline()
		{
			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("scePowerIsPowerOnline returning %b", pspsharp.hardware.Battery.isPluggedIn()));
				Console.WriteLine(string.Format("scePowerIsPowerOnline returning %b", Battery.PluggedIn));
			}

			return Battery.PluggedIn;
		}

		[HLEFunction(nid : 0x0AFD0D8B, version : 150)]
		public virtual bool scePowerIsBatteryExist()
		{
			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("scePowerIsBatteryExist returning %b", pspsharp.hardware.Battery.isPresent()));
				Console.WriteLine(string.Format("scePowerIsBatteryExist returning %b", Battery.Present));
			}

			return Battery.Present;
		}

		[HLEFunction(nid : 0x1E490401, version : 150)]
		public virtual bool scePowerIsBatteryCharging()
		{
			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("scePowerIsBatteryCharging returning %b", pspsharp.hardware.Battery.isCharging()));
				Console.WriteLine(string.Format("scePowerIsBatteryCharging returning %b", Battery.Charging));
			}

			return Battery.Charging;
		}

		[HLEFunction(nid : 0xB4432BC8, version : 150)]
		public virtual int scePowerGetBatteryChargingStatus()
		{
			int status = 0;
			if (Battery.Present)
			{
				status |= PSP_POWER_CB_BATTERY_EXIST;
			}
			if (Battery.PluggedIn)
			{
				status |= PSP_POWER_CB_AC_POWER;
			}
			if (Battery.Charging)
			{
				// I don't know exactly what to return under PSP_POWER_CB_BATTPOWER
				status |= PSP_POWER_CB_BATTPOWER;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetBatteryChargingStatus returning 0x{0:X}", status));
			}

			return status;
		}

		[HLEFunction(nid : 0xD3075926, version : 150)]
		public virtual bool scePowerIsLowBattery()
		{
			bool isLow = Battery.CurrentPowerPercent <= Battery.LowPercent;
			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("scePowerIsLowBattery returning %b", isLow));
				Console.WriteLine(string.Format("scePowerIsLowBattery returning %b", isLow));
			}

			return isLow;
		}

		/// <summary>
		/// Check if suspend is requided
		/// 
		/// @note: This function return 1 only when
		/// the battery charge is low and
		/// go in suspend mode!
		/// </summary>
		/// <returns> 1 if suspend is requided, otherwise 0 </returns>
		[HLEFunction(nid : 0x78A1A796, version : 150)]
		public virtual bool scePowerIsSuspendRequired()
		{
			bool isSuspendRequired = Battery.CurrentPowerPercent <= Battery.ForceSuspendPercent;
			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("scePowerIsSuspendRequired returning %b", isSuspendRequired));
				Console.WriteLine(string.Format("scePowerIsSuspendRequired returning %b", isSuspendRequired));
			}

			return isSuspendRequired;
		}

		[HLEFunction(nid : 0x94F5A53F, version : 150)]
		public virtual int scePowerGetBatteryRemainCapacity()
		{
			int batteryRemainCapacity = (Battery.CurrentPowerPercent * Battery.FullCapacity) / 100;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetBatteryRemainCapacity returning {0:D} mAh", batteryRemainCapacity));
			}

			return batteryRemainCapacity;
		}

		[HLEFunction(nid : 0xFD18A0FF, version : 150)]
		public virtual int scePowerGetBatteryFullCapacity()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetBatteryFullCapacity returning {0:D} mAh", Battery.FullCapacity));
			}

			return Battery.FullCapacity;
		}

		[HLEFunction(nid : 0x2085D15D, version : 150)]
		public virtual int scePowerGetBatteryLifePercent()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetBatteryLifePercent returning {0:D} %", Battery.CurrentPowerPercent));
			}

			return Battery.CurrentPowerPercent;
		}

		[HLEFunction(nid : 0x8EFB3FA2, version : 150)]
		public virtual int scePowerGetBatteryLifeTime()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetBatteryLifeTime returning {0:D}", Battery.LifeTime));
			}

			return Battery.LifeTime;
		}

		[HLEFunction(nid : 0x28E12023, version : 150)]
		public virtual int scePowerGetBatteryTemp()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetBatteryTemp returning {0:D} C", Battery.Temperature));
			}

			return Battery.Temperature;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x862AE1A6, version = 150) public int scePowerGetBatteryElec()
		[HLEFunction(nid : 0x862AE1A6, version : 150)]
		public virtual int scePowerGetBatteryElec()
		{
			return 0;
		}

		[HLEFunction(nid : 0x483CE86B, version : 150)]
		public virtual int scePowerGetBatteryVolt()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetBatteryVolt {0:D}", Battery.Voltage));
			}

			return Battery.Voltage;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x23436A4A, version = 150) public int scePower_23436A4A()
		[HLEFunction(nid : 0x23436A4A, version : 150)]
		public virtual int scePower_23436A4A()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0CD21B1F, version = 150) public int scePowerSetPowerSwMode()
		[HLEFunction(nid : 0x0CD21B1F, version : 150)]
		public virtual int scePowerSetPowerSwMode()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x165CE085, version = 150) public int scePowerGetPowerSwMode()
		[HLEFunction(nid : 0x165CE085, version : 150)]
		public virtual int scePowerGetPowerSwMode()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x23C31FFE, version = 150) public int scePowerVolatileMemLock()
		[HLEFunction(nid : 0x23C31FFE, version : 150)]
		public virtual int scePowerVolatileMemLock()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFA97A599, version = 150) public int scePowerVolatileMemTryLock()
		[HLEFunction(nid : 0xFA97A599, version : 150)]
		public virtual int scePowerVolatileMemTryLock()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB3EDD801, version = 150) public int scePowerVolatileMemUnlock()
		[HLEFunction(nid : 0xB3EDD801, version : 150)]
		public virtual int scePowerVolatileMemUnlock()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD6D016EF, version = 150) public int scePowerLock()
		[HLEFunction(nid : 0xD6D016EF, version : 150)]
		public virtual int scePowerLock()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCA3D34C1, version = 150) public int scePowerUnlock()
		[HLEFunction(nid : 0xCA3D34C1, version : 150)]
		public virtual int scePowerUnlock()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDB62C9CF, version = 150) public int scePowerCancelRequest()
		[HLEFunction(nid : 0xDB62C9CF, version : 150)]
		public virtual int scePowerCancelRequest()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7FA406DD, version = 150) public int scePowerIsRequest()
		[HLEFunction(nid : 0x7FA406DD, version : 150)]
		public virtual int scePowerIsRequest()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2B7C7CF4, version = 150) public int scePowerRequestStandby()
		[HLEFunction(nid : 0x2B7C7CF4, version : 150)]
		public virtual int scePowerRequestStandby()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAC32C9CC, version = 150) public int scePowerRequestSuspend()
		[HLEFunction(nid : 0xAC32C9CC, version : 150)]
		public virtual int scePowerRequestSuspend()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2875994B, version = 150) public int scePower_2875994B()
		[HLEFunction(nid : 0x2875994B, version : 150)]
		public virtual int scePower_2875994B()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3951AF53, version = 150) public int scePowerWaitRequestCompletion()
		[HLEFunction(nid : 0x3951AF53, version : 150)]
		public virtual int scePowerWaitRequestCompletion()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0074EF9B, version = 150) public int scePowerGetResumeCount()
		[HLEFunction(nid : 0x0074EF9B, version : 150)]
		public virtual int scePowerGetResumeCount()
		{
			return 0;
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x04B7766E, version : 150)]
		public virtual int scePowerRegisterCallback(int slot, int uid)
		{
			bool notifyCallback = false;
			int result;

			// Multiple power callbacks (up to 16) can be assigned for multiple threads.
			if (slot == PSP_POWER_CB_SLOT_AUTO)
			{
				// Return ERROR_OUT_OF_MEMORY when no free slot found
				result = SceKernelErrors.ERROR_OUT_OF_MEMORY;

				for (int i = 0; i < powerCBSlots.Length; i++)
				{
					if (powerCBSlots[i] == 0)
					{
						powerCBSlots[i] = uid;
						result = i;
						notifyCallback = true;
						break;
					}
				}
			}
			else if (slot >= 0 && slot < powerCBSlots.Length)
			{
				if (powerCBSlots[slot] == 0)
				{
					powerCBSlots[slot] = uid;
					result = 0;
					notifyCallback = true;
				}
				else
				{
					result = SceKernelErrors.ERROR_ALREADY;
				}
			}
			else
			{
				result = -1;
			}

			if (notifyCallback)
			{
				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				if (threadMan.hleKernelRegisterCallback(SceKernelThreadInfo.THREAD_CALLBACK_POWER, uid))
				{
					// Start by notifying the POWER callback that we're using AC power.
					threadMan.hleKernelNotifyCallback(SceKernelThreadInfo.THREAD_CALLBACK_POWER, uid, PSP_POWER_CB_AC_POWER);
				}
			}

			return result;
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0xDFA8BAF8, version : 150)]
		public virtual int scePowerUnregisterCallback(int slot)
		{
			if (slot < 0 || slot >= powerCBSlots.Length)
			{
				return -1;
			}

			if (powerCBSlots[slot] != 0)
			{
				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				threadMan.hleKernelUnRegisterCallback(SceKernelThreadInfo.THREAD_CALLBACK_POWER, powerCBSlots[slot]);
				powerCBSlots[slot] = 0;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDB9D28DD, version = 150) public int scePowerUnregisterCallback()
		[HLEFunction(nid : 0xDB9D28DD, version : 150)]
		public virtual int scePowerUnregisterCallback()
		{
			return 0;
		}

		[HLEFunction(nid : 0x843FBF43, version : 150, checkInsideInterrupt : true)]
		public virtual int scePowerSetCpuClockFrequency(int freq)
		{
			cpuClock = freq;

			return 0;
		}

		[HLEFunction(nid : 0xB8D7B3FB, version : 150, checkInsideInterrupt : true)]
		public virtual int scePowerSetBusClockFrequency(int freq)
		{
			busClock = freq;

			return 0;
		}

		[HLEFunction(nid : 0xFEE03A2F, version : 150)]
		public virtual int scePowerGetCpuClockFrequency()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetCpuClockFrequency returning 0x{0:X}", cpuClock));
			}

			return cpuClock;
		}

		[HLEFunction(nid : 0x478FE6F5, version : 150)]
		public virtual int scePowerGetBusClockFrequency()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetBusClockFrequency returning 0x{0:X}", busClock));
			}

			return busClock;
		}

		[HLEFunction(nid : 0xFDB5BFE9, version : 150)]
		public virtual int scePowerGetCpuClockFrequencyInt()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetCpuClockFrequencyInt returning 0x{0:X}", cpuClock));
			}

			return cpuClock;
		}

		[HLEFunction(nid : 0xBD681969, version : 150)]
		public virtual int scePowerGetBusClockFrequencyInt()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetBusClockFrequencyInt returning 0x{0:X}", busClock));
			}

			return busClock;
		}

		[HLEFunction(nid : 0x34F9C463, version : 150)]
		public virtual int scePowerGetPllClockFrequencyInt()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetPllClockFrequencyInt returning 0x{0:X}", pllClock));
			}

			return pllClock;
		}

		[HLEFunction(nid : 0xB1A52C83, version : 150)]
		public virtual float scePowerGetCpuClockFrequencyFloat()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetCpuClockFrequencyFloat returning {0:F}", (float) cpuClock));
			}

			return (float) cpuClock;
		}

		[HLEFunction(nid : 0x9BADB3EB, version : 150)]
		public virtual float scePowerGetBusClockFrequencyFloat()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetBusClockFrequencyFloat returning {0:F}", (float) busClock));
			}

			return (float) busClock;
		}

		[HLEFunction(nid : 0xEA382A27, version : 150)]
		public virtual float scePowerGetPllClockFrequencyFloat()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("scePowerGetPllClockFrequencyFloat returning {0:F}", (float) pllClock));
			}

			return (float) pllClock;
		}

		[HLEFunction(nid : 0x737486F2, version : 150)]
		public virtual int scePowerSetClockFrequency(int pllClock, int cpuClock, int busClock)
		{
			if (cpuClock == 0 || cpuClock > 333)
			{
				Console.WriteLine(string.Format("scePowerSetClockFrequency invalid frequency pllClock {0:D} cpuClock {1:D} busClock {2:D}",pllClock,cpuClock,busClock));
				return SceKernelErrors.ERROR_INVALID_VALUE;
			}

			log.info(string.Format("scePowerSetClockFrequency pllClock {0:D} cpuClock {1:D} busClock {2:D}",pllClock,cpuClock,busClock));
			this.cpuClock = cpuClock;
			this.busClock = busClock;
			if (this.pllClock != pllClock)
			{
				this.pllClock = pllClock;

				Modules.ThreadManForUserModule.hleKernelDelayThread(150000, false);
			}
			return 0;
		}

		[HLEFunction(nid : 0xEBD177D6, version : 150)]
		public virtual int scePower_EBD177D6(int pllClock, int cpuClock, int busClock)
		{
			// Identical to scePowerSetClockFrequency.
			if (cpuClock == 0 || cpuClock > 333)
			{
				Console.WriteLine(string.Format("scePower_EBD177D6 invalid frequency pllClock {0:D} cpuClock {1:D} busClock {2:D}",pllClock,cpuClock,busClock));
				return SceKernelErrors.ERROR_INVALID_VALUE;
			}

			log.info(string.Format("scePower_EBD177D6 pllClock {0:D} cpuClock {1:D} busClock {2:D}",pllClock,cpuClock,busClock));
			this.cpuClock = cpuClock;
			this.busClock = busClock;
			if (this.pllClock != pllClock)
			{
				this.pllClock = pllClock;

				Modules.ThreadManForUserModule.hleKernelDelayThread(150000, false);
			}
			return 0;
		}

		[HLEFunction(nid : 0x469989AD, version : 630)]
		public virtual int scePower_469989AD(int pllClock, int cpuClock, int busClock)
		{
			// Identical to scePowerSetClockFrequency.
			if (cpuClock == 0 || cpuClock > 333)
			{
				Console.WriteLine(string.Format("scePower_469989AD invalid frequency pllClock {0:D} cpuClock {1:D} busClock {2:D}",pllClock,cpuClock,busClock));
				return SceKernelErrors.ERROR_INVALID_VALUE;
			}

			log.info(string.Format("scePower_469989AD pllClock {0:D} cpuClock {1:D} busClock {2:D}",pllClock,cpuClock,busClock));
			this.cpuClock = cpuClock;
			this.busClock = busClock;
			if (this.pllClock != pllClock)
			{
				this.pllClock = pllClock;

				Modules.ThreadManForUserModule.hleKernelDelayThread(150000, false);
			}
			return 0;
		}

		[HLEFunction(nid : 0xA85880D0, version : 630)]
		public virtual bool scePower_A85880D0()
		{
			int model = Model.Model;

			// Returning 0 for a PSP fat, 1 otherwise
			bool result = model != Model.MODEL_PSP_FAT;

			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("scePower_A85880D0 returning %b", result));
				Console.WriteLine(string.Format("scePower_A85880D0 returning %b", result));
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBA566CD0, version = 660) public int scePowerSetWakeupCondition(int condition)
		[HLEFunction(nid : 0xBA566CD0, version : 660)]
		public virtual int scePowerSetWakeupCondition(int condition)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3234844A, version = 150) public int scePower_driver_3234844A()
		[HLEFunction(nid : 0x3234844A, version : 150)]
		public virtual int scePower_driver_3234844A()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5F5006D2, version = 660) public int scePower_driver_5F5006D2()
		[HLEFunction(nid : 0x5F5006D2, version : 660)]
		public virtual int scePower_driver_5F5006D2()
		{
			return scePower_driver_3234844A();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x315B8CB6, version = 150) public int scePowerUnregisterCallback_660()
		[HLEFunction(nid : 0x315B8CB6, version : 150)]
		public virtual int scePowerUnregisterCallback_660()
		{
			return 0;
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x766CD857, version : 150)]
		public virtual int scePowerRegisterCallback_660(int slot, int uid)
		{
			return scePowerRegisterCallback(slot, uid);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x55D2D789, version = 150) public int scePowerGetTachyonVoltage(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown1, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown2)
		[HLEFunction(nid : 0x55D2D789, version : 150)]
		public virtual int scePowerGetTachyonVoltage(TPointer32 unknown1, TPointer32 unknown2)
		{
			unknown1.setValue(tachyonVoltage1);
			unknown2.setValue(tachyonVoltage2);
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBADA8332, version = 660) public int scePowerGetTachyonVoltage_660(@BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown1, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 unknown2)
		[HLEFunction(nid : 0xBADA8332, version : 660)]
		public virtual int scePowerGetTachyonVoltage_660(TPointer32 unknown1, TPointer32 unknown2)
		{
			return scePowerGetTachyonVoltage(unknown1, unknown2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDD27F119, version = 150) public int scePowerSetTachyonVoltage(int unknown1, int unknown2)
		[HLEFunction(nid : 0xDD27F119, version : 150)]
		public virtual int scePowerSetTachyonVoltage(int unknown1, int unknown2)
		{
			if (unknown1 != -1)
			{
				tachyonVoltage1 = unknown1 & 0xFFFF;
			}
			if (unknown2 != -1)
			{
				tachyonVoltage2 = unknown2 & 0xFFFF;
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x12F8302D, version = 660) public int scePowerSetTachyonVoltage_660(int unknown1, int unknown2)
		[HLEFunction(nid : 0x12F8302D, version : 660)]
		public virtual int scePowerSetTachyonVoltage_660(int unknown1, int unknown2)
		{
			return scePowerSetTachyonVoltage(unknown1, unknown2);
		}
	}
}