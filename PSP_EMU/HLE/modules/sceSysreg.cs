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
	//using Logger = org.apache.log4j.Logger;

	using Model = pspsharp.hardware.Model;

	public class sceSysreg : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceSysreg");
		private long fuseId = 0x12345678ABCDEFL; // Dummy Fuse ID
		private int fuseConfig = 0x2400; // Value retrieved from a real PSP

		public virtual long FuseId
		{
			set
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("setFuseId 0x{0:X}", value));
				}
				this.fuseId = value;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0143E8A8, version = 150) public int sceSysregSemaTryLock()
		[HLEFunction(nid : 0x0143E8A8, version : 150)]
		public virtual int sceSysregSemaTryLock()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x018F913A, version = 150) public int sceSysregAtahddClkEnable()
		[HLEFunction(nid : 0x018F913A, version : 150)]
		public virtual int sceSysregAtahddClkEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0607A4C4, version = 150) public int sceSysreg_driver_0607A4C4()
		[HLEFunction(nid : 0x0607A4C4, version : 150)]
		public virtual int sceSysreg_driver_0607A4C4()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x08FE40F5, version = 150) public int sceSysregUsbhostClkEnable()
		[HLEFunction(nid : 0x08FE40F5, version : 150)]
		public virtual int sceSysregUsbhostClkEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x092AF6A9, version = 150) public int sceSysregAudioClkDisable()
		[HLEFunction(nid : 0x092AF6A9, version : 150)]
		public virtual int sceSysregAudioClkDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0995F8F6, version = 150) public int sceSysreg_driver_0995F8F6()
		[HLEFunction(nid : 0x0995F8F6, version : 150)]
		public virtual int sceSysreg_driver_0995F8F6()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0AE8E549, version = 150) public int sceSysregAvcResetEnable()
		[HLEFunction(nid : 0x0AE8E549, version : 150)]
		public virtual int sceSysregAvcResetEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0BBD4ED6, version = 150) public int sceSysregEmcddrBusClockEnable()
		[HLEFunction(nid : 0x0BBD4ED6, version : 150)]
		public virtual int sceSysregEmcddrBusClockEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x14EB1393, version = 150) public int sceSysreg_driver_14EB1393()
		[HLEFunction(nid : 0x14EB1393, version : 150)]
		public virtual int sceSysreg_driver_14EB1393()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1561BCD2, version = 150) public int sceSysreg_driver_1561BCD2()
		[HLEFunction(nid : 0x1561BCD2, version : 150)]
		public virtual int sceSysreg_driver_1561BCD2()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x15786501, version = 150) public int sceSysreg_driver_15786501()
		[HLEFunction(nid : 0x15786501, version : 150)]
		public virtual int sceSysreg_driver_15786501()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x15DC34BC, version = 150) public int sceSysregGpioIoDisable(int port)
		[HLEFunction(nid : 0x15DC34BC, version : 150)]
		public virtual int sceSysregGpioIoDisable(int port)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x176E590A, version = 150) public int sceSysregMsifIoDisable()
		[HLEFunction(nid : 0x176E590A, version : 150)]
		public virtual int sceSysregMsifIoDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x187F651D, version = 150) public int sceSysregPllGetOutSelect()
		[HLEFunction(nid : 0x187F651D, version : 150)]
		public virtual int sceSysregPllGetOutSelect()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x191D7461, version = 150) public int sceSysregApbBusClockEnable()
		[HLEFunction(nid : 0x191D7461, version : 150)]
		public virtual int sceSysregApbBusClockEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x19A6E54B, version = 150) public int sceSysregPllSetOutSelect()
		[HLEFunction(nid : 0x19A6E54B, version : 150)]
		public virtual int sceSysregPllSetOutSelect()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x19F4C92D, version = 150) public int sceSysreg_driver_19F4C92D()
		[HLEFunction(nid : 0x19F4C92D, version : 150)]
		public virtual int sceSysreg_driver_19F4C92D()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1A27B224, version = 150) public int sceSysregVmeResetEnable()
		[HLEFunction(nid : 0x1A27B224, version : 150)]
		public virtual int sceSysregVmeResetEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1D233EF9, version = 150) public int sceSysregUsbBusClockDisable()
		[HLEFunction(nid : 0x1D233EF9, version : 150)]
		public virtual int sceSysregUsbBusClockDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1E111B75, version = 150) public int sceSysregMsifClkDisable()
		[HLEFunction(nid : 0x1E111B75, version : 150)]
		public virtual int sceSysregMsifClkDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1E62714E, version = 150) public int sceSysreg_driver_1E62714E()
		[HLEFunction(nid : 0x1E62714E, version : 150)]
		public virtual int sceSysreg_driver_1E62714E()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1E881843, version = 150) public int sceSysregIntrInit()
		[HLEFunction(nid : 0x1E881843, version : 150)]
		public virtual int sceSysregIntrInit()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x20B1D0A9, version = 150) public int sceSysreg_driver_20B1D0A9()
		[HLEFunction(nid : 0x20B1D0A9, version : 150)]
		public virtual int sceSysreg_driver_20B1D0A9()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x20DF8278, version = 150) public int sceSysregMsifGetConnectStatus(int memoryStickNumber)
		[HLEFunction(nid : 0x20DF8278, version : 150)]
		public virtual int sceSysregMsifGetConnectStatus(int memoryStickNumber)
		{
			return 1;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x231EE757, version = 150) public int sceSysregPllUpdateFrequency()
		[HLEFunction(nid : 0x231EE757, version : 150)]
		public virtual int sceSysregPllUpdateFrequency()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2458B6AC, version = 150) public int sceSysreg_driver_2458B6AC()
		[HLEFunction(nid : 0x2458B6AC, version : 150)]
		public virtual int sceSysreg_driver_2458B6AC()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x258782A3, version = 150) public int sceSysregAwEdramBusClockDisable()
		[HLEFunction(nid : 0x258782A3, version : 150)]
		public virtual int sceSysregAwEdramBusClockDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x283D7A95, version = 150) public int sceSysregGpioClkEnable()
		[HLEFunction(nid : 0x283D7A95, version : 150)]
		public virtual int sceSysregGpioClkEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2F9B03E0, version = 150) public int sceSysregKirkResetDisable()
		[HLEFunction(nid : 0x2F9B03E0, version : 150)]
		public virtual int sceSysregKirkResetDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x30C0A141, version = 150) public int sceSysregUsbResetEnable()
		[HLEFunction(nid : 0x30C0A141, version : 150)]
		public virtual int sceSysregUsbResetEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x32E02FDF, version = 150) public int sceSysreg_driver_32E02FDF()
		[HLEFunction(nid : 0x32E02FDF, version : 150)]
		public virtual int sceSysreg_driver_32E02FDF()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x33EE43F0, version = 150) public int sceSysreg_driver_33EE43F0()
		[HLEFunction(nid : 0x33EE43F0, version : 150)]
		public virtual int sceSysreg_driver_33EE43F0()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x35C23493, version = 150) public int sceSysregUsbhostBusClockEnable()
		[HLEFunction(nid : 0x35C23493, version : 150)]
		public virtual int sceSysregUsbhostBusClockEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x370419AD, version = 150) public int sceSysregMsifResetEnable()
		[HLEFunction(nid : 0x370419AD, version : 150)]
		public virtual int sceSysregMsifResetEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x37FBACA5, version = 150) public int sceSysregGpioIoEnable(int port)
		[HLEFunction(nid : 0x37FBACA5, version : 150)]
		public virtual int sceSysregGpioIoEnable(int port)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x38527743, version = 150) public int sceSysregMeBusClockEnable()
		[HLEFunction(nid : 0x38527743, version : 150)]
		public virtual int sceSysregMeBusClockEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3BB0B2C8, version = 150) public int sceSysreg_driver_3BB0B2C8()
		[HLEFunction(nid : 0x3BB0B2C8, version : 150)]
		public virtual int sceSysreg_driver_3BB0B2C8()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3D2CE374, version = 150) public int sceSysregApbBusClockDisable()
		[HLEFunction(nid : 0x3D2CE374, version : 150)]
		public virtual int sceSysregApbBusClockDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3E95AB4D, version = 150) public int sceSysregAtaIoEnable()
		[HLEFunction(nid : 0x3E95AB4D, version : 150)]
		public virtual int sceSysregAtaIoEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3F6F2CC7, version = 150) public int sceSysreg_driver_3F6F2CC7(int cpuFreqNumerator, int cpuFreqDenominator)
		[HLEFunction(nid : 0x3F6F2CC7, version : 150)]
		public virtual int sceSysreg_driver_3F6F2CC7(int cpuFreqNumerator, int cpuFreqDenominator)
		{
			// Sets the CPU Frequency by ratio
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x41B0337B, version = 150) public int sceSysregAudioClkoutClkSelect()
		[HLEFunction(nid : 0x41B0337B, version : 150)]
		public virtual int sceSysregAudioClkoutClkSelect()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x434E8AF1, version = 150) public int sceSysreg_driver_434E8AF1()
		[HLEFunction(nid : 0x434E8AF1, version : 150)]
		public virtual int sceSysreg_driver_434E8AF1()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x44277D0D, version = 150) public int sceSysregAwRegBBusClockEnable()
		[HLEFunction(nid : 0x44277D0D, version : 150)]
		public virtual int sceSysregAwRegBBusClockEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4479C9BD, version = 150) public int sceSysregUsbhostBusClockDisable()
		[HLEFunction(nid : 0x4479C9BD, version : 150)]
		public virtual int sceSysregUsbhostBusClockDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x457FEBA9, version = 150) public int sceSysregMeResetEnable()
		[HLEFunction(nid : 0x457FEBA9, version : 150)]
		public virtual int sceSysregMeResetEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4634C9DC, version = 150) public int sceSysregAudioIoEnable()
		[HLEFunction(nid : 0x4634C9DC, version : 150)]
		public virtual int sceSysregAudioIoEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x47C971B2, version = 150) public int sceSysregTopResetEnable()
		[HLEFunction(nid : 0x47C971B2, version : 150)]
		public virtual int sceSysregTopResetEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x48124AFE, version = 150) public int sceSysregMsifClkSelect()
		[HLEFunction(nid : 0x48124AFE, version : 150)]
		public virtual int sceSysregMsifClkSelect()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4841B2D2, version = 150) public int sceSysreg_driver_4841B2D2()
		[HLEFunction(nid : 0x4841B2D2, version : 150)]
		public virtual int sceSysreg_driver_4841B2D2()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x48CF8E69, version = 150) public int sceSysregAtahddClkSelect()
		[HLEFunction(nid : 0x48CF8E69, version : 150)]
		public virtual int sceSysregAtahddClkSelect()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x48F1C4AD, version = 150) public int sceSysregMeResetDisable()
		[HLEFunction(nid : 0x48F1C4AD, version : 150)]
		public virtual int sceSysregMeResetDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4A433DC3, version = 150) public int sceSysregUsbhostResetEnable()
		[HLEFunction(nid : 0x4A433DC3, version : 150)]
		public virtual int sceSysregUsbhostResetEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4B4CCE80, version = 150) public int sceSysregAudioClkoutIoEnable()
		[HLEFunction(nid : 0x4B4CCE80, version : 150)]
		public virtual int sceSysregAudioClkoutIoEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4C0BED71, version = 150) public int sceSysreg_driver_4C0BED71()
		[HLEFunction(nid : 0x4C0BED71, version : 150)]
		public virtual int sceSysreg_driver_4C0BED71()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4DB0C55D, version = 150) public int sceSysregMsifClkEnable()
		[HLEFunction(nid : 0x4DB0C55D, version : 150)]
		public virtual int sceSysregMsifClkEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4E5C86AA, version = 150) public int sceSysreg_driver_4E5C86AA()
		[HLEFunction(nid : 0x4E5C86AA, version : 150)]
		public virtual int sceSysreg_driver_4E5C86AA()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x51571E8F, version = 150) public int sceSysregAwRegABusClockEnable()
		[HLEFunction(nid : 0x51571E8F, version : 150)]
		public virtual int sceSysregAwRegABusClockEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x518E3F29, version = 150) public int sceSysregMsifIoEnable()
		[HLEFunction(nid : 0x518E3F29, version : 150)]
		public virtual int sceSysregMsifIoEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x52B74976, version = 150) public int sceSysregAwRegABusClockDisable()
		[HLEFunction(nid : 0x52B74976, version : 150)]
		public virtual int sceSysregAwRegABusClockDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x55373EE4, version = 150) public int sceSysregAtahddClkDisable()
		[HLEFunction(nid : 0x55373EE4, version : 150)]
		public virtual int sceSysregAtahddClkDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x554E97F7, version = 150) public int sceSysreg_driver_554E97F7()
		[HLEFunction(nid : 0x554E97F7, version : 150)]
		public virtual int sceSysreg_driver_554E97F7()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x55FF02E9, version = 150) public int sceSysregAvcResetDisable()
		[HLEFunction(nid : 0x55FF02E9, version : 150)]
		public virtual int sceSysregAvcResetDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x56E95BB6, version = 150) public int sceSysregMsifAcquireConnectIntr(int memoryStickNumber, int mask)
		[HLEFunction(nid : 0x56E95BB6, version : 150)]
		public virtual int sceSysregMsifAcquireConnectIntr(int memoryStickNumber, int mask)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5D5118CD, version = 150) public int sceSysregAtahddIoEnable()
		[HLEFunction(nid : 0x5D5118CD, version : 150)]
		public virtual int sceSysregAtahddIoEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5F179286, version = 150) public int sceSysregAtahddBusClockDisable()
		[HLEFunction(nid : 0x5F179286, version : 150)]
		public virtual int sceSysregAtahddBusClockDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x61FAE917, version = 150) public int sceSysregInterruptToOther()
		[HLEFunction(nid : 0x61FAE917, version : 150)]
		public virtual int sceSysregInterruptToOther()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x63E1EE9C, version = 150) public int sceSysreg_driver_63E1EE9C()
		[HLEFunction(nid : 0x63E1EE9C, version : 150)]
		public virtual int sceSysreg_driver_63E1EE9C()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x64C8E8DD, version = 150) public int sceSysregAtaResetEnable()
		[HLEFunction(nid : 0x64C8E8DD, version : 150)]
		public virtual int sceSysregAtaResetEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x655C9CFC, version = 150) public int sceSysregScResetEnable()
		[HLEFunction(nid : 0x655C9CFC, version : 150)]
		public virtual int sceSysregScResetEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6596EBC3, version = 150) public int sceSysreg_driver_6596EBC3()
		[HLEFunction(nid : 0x6596EBC3, version : 150)]
		public virtual int sceSysreg_driver_6596EBC3()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x66899952, version = 150) public int sceSysregAwResetEnable()
		[HLEFunction(nid : 0x66899952, version : 150)]
		public virtual int sceSysregAwResetEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x68AE6434, version = 150) public int sceSysreg_driver_68AE6434()
		[HLEFunction(nid : 0x68AE6434, version : 150)]
		public virtual int sceSysreg_driver_68AE6434()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6A9B0426, version = 150) public int sceSysregAtaClkEnable()
		[HLEFunction(nid : 0x6A9B0426, version : 150)]
		public virtual int sceSysregAtaClkEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6B3A3417, version = 150) public int sceSysregPllGetBaseFrequency()
		[HLEFunction(nid : 0x6B3A3417, version : 150)]
		public virtual int sceSysregPllGetBaseFrequency()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6C0EE043, version = 150) public int sceSysregUsbClkDisable()
		[HLEFunction(nid : 0x6C0EE043, version : 150)]
		public virtual int sceSysregUsbClkDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6DA9347D, version = 150) public int sceSysreg_driver_6DA9347D()
		[HLEFunction(nid : 0x6DA9347D, version : 150)]
		public virtual int sceSysreg_driver_6DA9347D()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6F3B6D7D, version = 150) public int sceSysreg_driver_6F3B6D7D()
		[HLEFunction(nid : 0x6F3B6D7D, version : 150)]
		public virtual int sceSysreg_driver_6F3B6D7D()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x72887197, version = 150) public int sceSysreg_driver_72887197()
		[HLEFunction(nid : 0x72887197, version : 150)]
		public virtual int sceSysreg_driver_72887197()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x72C1CA96, version = 150) public int sceSysregUsbGetConnectStatus()
		[HLEFunction(nid : 0x72C1CA96, version : 150)]
		public virtual int sceSysregUsbGetConnectStatus()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x73B3E52D, version = 150) public int sceSysreg_driver_73B3E52D()
		[HLEFunction(nid : 0x73B3E52D, version : 150)]
		public virtual int sceSysreg_driver_73B3E52D()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x73EBC752, version = 150) public int sceSysreg_driver_73EBC752()
		[HLEFunction(nid : 0x73EBC752, version : 150)]
		public virtual int sceSysreg_driver_73EBC752()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x74C6F776, version = 150) public int sceSysregApbTimerClkEnable()
		[HLEFunction(nid : 0x74C6F776, version : 150)]
		public virtual int sceSysregApbTimerClkEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x76E57DC6, version = 150) public int sceSysreg_driver_76E57DC6()
		[HLEFunction(nid : 0x76E57DC6, version : 150)]
		public virtual int sceSysreg_driver_76E57DC6()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7AA8A8BE, version = 150) public int sceSysregIntrEnd()
		[HLEFunction(nid : 0x7AA8A8BE, version : 150)]
		public virtual int sceSysregIntrEnd()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7CF05E81, version = 150) public int sceSysreg_driver_7CF05E81()
		[HLEFunction(nid : 0x7CF05E81, version : 150)]
		public virtual int sceSysreg_driver_7CF05E81()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7DD0CBEE, version = 150) public int sceSysregMsifResetDisable()
		[HLEFunction(nid : 0x7DD0CBEE, version : 150)]
		public virtual int sceSysregMsifResetDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7E1B1F28, version = 150) public int sceSysregAwRegBBusClockDisable()
		[HLEFunction(nid : 0x7E1B1F28, version : 150)]
		public virtual int sceSysregAwRegBBusClockDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x803E5F37, version = 150) public int sceSysreg_driver_803E5F37()
		[HLEFunction(nid : 0x803E5F37, version : 150)]
		public virtual int sceSysreg_driver_803E5F37()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x80944C8A, version = 150) public int sceSysreg_driver_80944C8A()
		[HLEFunction(nid : 0x80944C8A, version : 150)]
		public virtual int sceSysreg_driver_80944C8A()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x84A279A4, version = 150) public int sceSysregUsbClkEnable()
		[HLEFunction(nid : 0x84A279A4, version : 150)]
		public virtual int sceSysregUsbClkEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x84E0F197, version = 150) public int sceSysreg_driver_84E0F197()
		[HLEFunction(nid : 0x84E0F197, version : 150)]
		public virtual int sceSysreg_driver_84E0F197()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x85B74FDA, version = 150) public int sceSysreg_driver_85B74FDA()
		[HLEFunction(nid : 0x85B74FDA, version : 150)]
		public virtual int sceSysreg_driver_85B74FDA()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x85BA0C0B, version = 150) public int sceSysregAudioBusClockDisable()
		[HLEFunction(nid : 0x85BA0C0B, version : 150)]
		public virtual int sceSysregAudioBusClockDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x866EEB74, version = 150) public int sceSysregAtahddResetEnable()
		[HLEFunction(nid : 0x866EEB74, version : 150)]
		public virtual int sceSysregAtahddResetEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x867BD103, version = 150) public int sceSysregKirkBusClockDisable()
		[HLEFunction(nid : 0x867BD103, version : 150)]
		public virtual int sceSysregKirkBusClockDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x87B61303, version = 150) public int sceSysreg_driver_87B61303()
		[HLEFunction(nid : 0x87B61303, version : 150)]
		public virtual int sceSysreg_driver_87B61303()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8B1DD83A, version = 150) public int sceSysregAudioBusClockEnable()
		[HLEFunction(nid : 0x8B1DD83A, version : 150)]
		public virtual int sceSysregAudioBusClockEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8CFD0DCA, version = 150) public int sceSysregAtaResetDisable()
		[HLEFunction(nid : 0x8CFD0DCA, version : 150)]
		public virtual int sceSysregAtaResetDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9057C9E2, version = 150) public int sceSysregAtaClkSelect()
		[HLEFunction(nid : 0x9057C9E2, version : 150)]
		public virtual int sceSysregAtaClkSelect()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x915B3772, version = 150) public int sceSysregUsbhostAcquireIntr()
		[HLEFunction(nid : 0x915B3772, version : 150)]
		public virtual int sceSysregUsbhostAcquireIntr()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9275DD37, version = 150) public int sceSysreg_driver_9275DD37()
		[HLEFunction(nid : 0x9275DD37, version : 150)]
		public virtual int sceSysreg_driver_9275DD37()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9306F27B, version = 150) public int sceSysregUsbResetDisable()
		[HLEFunction(nid : 0x9306F27B, version : 150)]
		public virtual int sceSysregUsbResetDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x93F96D8F, version = 150) public int sceSysregMsifBusClockEnable()
		[HLEFunction(nid : 0x93F96D8F, version : 150)]
		public virtual int sceSysregMsifBusClockEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x94B89638, version = 150) public int sceSysregAtahddBusClockEnable()
		[HLEFunction(nid : 0x94B89638, version : 150)]
		public virtual int sceSysregAtahddBusClockEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x95CA8AA1, version = 150) public int sceSysregUsbIoEnable()
		[HLEFunction(nid : 0x95CA8AA1, version : 150)]
		public virtual int sceSysregUsbIoEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x96D74557, version = 150) public float sceSysreg_driver_96D74557()
		[HLEFunction(nid : 0x96D74557, version : 150)]
		public virtual float sceSysreg_driver_96D74557()
		{
			return 0f;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9746F3B2, version = 150) public int sceSysreg_driver_9746F3B2()
		[HLEFunction(nid : 0x9746F3B2, version : 150)]
		public virtual int sceSysreg_driver_9746F3B2()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9A6E7BB8, version = 150) public int sceSysregUsbQueryIntr()
		[HLEFunction(nid : 0x9A6E7BB8, version : 150)]
		public virtual int sceSysregUsbQueryIntr()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9B710D3C, version = 150) public int sceSysregUsbhostResetDisable()
		[HLEFunction(nid : 0x9B710D3C, version : 150)]
		public virtual int sceSysregUsbhostResetDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9CD29D6C, version = 150) public int sceSysregSetMasterPriv(int privilege, boolean on)
		[HLEFunction(nid : 0x9CD29D6C, version : 150)]
		public virtual int sceSysregSetMasterPriv(int privilege, bool on)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9E2F8FD5, version = 150) public int sceSysreg_driver_9E2F8FD5()
		[HLEFunction(nid : 0x9E2F8FD5, version : 150)]
		public virtual int sceSysreg_driver_9E2F8FD5()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9EB8C49E, version = 150) public int sceSysregAtahddResetDisable()
		[HLEFunction(nid : 0x9EB8C49E, version : 150)]
		public virtual int sceSysregAtahddResetDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA4706857, version = 150) public int sceSysregApbTimerClkSelect()
		[HLEFunction(nid : 0xA4706857, version : 150)]
		public virtual int sceSysregApbTimerClkSelect()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA5CC6025, version = 150) public float sceSysregPllGetFrequency()
		[HLEFunction(nid : 0xA5CC6025, version : 150)]
		public virtual float sceSysregPllGetFrequency()
		{
			return 0f;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA781B599, version = 150) public int sceSysregUsbhostClkDisable()
		[HLEFunction(nid : 0xA781B599, version : 150)]
		public virtual int sceSysregUsbhostClkDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA7C82BDD, version = 150) public int sceSysreg_driver_A7C82BDD()
		[HLEFunction(nid : 0xA7C82BDD, version : 150)]
		public virtual int sceSysreg_driver_A7C82BDD()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA9EE3124, version = 150) public int sceSysregGpioClkDisable()
		[HLEFunction(nid : 0xA9EE3124, version : 150)]
		public virtual int sceSysregGpioClkDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAEB8DBD1, version = 150) public int sceSysregAwResetDisable()
		[HLEFunction(nid : 0xAEB8DBD1, version : 150)]
		public virtual int sceSysregAwResetDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAEC87DFD, version = 150) public int sceSysregSetAwEdramSize()
		[HLEFunction(nid : 0xAEC87DFD, version : 150)]
		public virtual int sceSysregSetAwEdramSize()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAFE47914, version = 150) public int sceSysregDoTimerEvent()
		[HLEFunction(nid : 0xAFE47914, version : 150)]
		public virtual int sceSysregDoTimerEvent()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB1751B06, version = 150) public int sceSysregAudioClkEnable()
		[HLEFunction(nid : 0xB1751B06, version : 150)]
		public virtual int sceSysregAudioClkEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB413B041, version = 150) public int sceSysreg_driver_B413B041()
		[HLEFunction(nid : 0xB413B041, version : 150)]
		public virtual int sceSysreg_driver_B413B041()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB6296512, version = 150) public int sceSysreg_driver_B6296512()
		[HLEFunction(nid : 0xB6296512, version : 150)]
		public virtual int sceSysreg_driver_B6296512()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB73D3619, version = 150) public int sceSysregVmeResetDisable()
		[HLEFunction(nid : 0xB73D3619, version : 150)]
		public virtual int sceSysregVmeResetDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBB6BAA00, version = 150) public int sceSysregMsifQueryConnectIntr()
		[HLEFunction(nid : 0xBB6BAA00, version : 150)]
		public virtual int sceSysregMsifQueryConnectIntr()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBBC721EA, version = 150) public int sceSysregKirkBusClockEnable()
		[HLEFunction(nid : 0xBBC721EA, version : 150)]
		public virtual int sceSysregKirkBusClockEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBD7B035B, version = 150) public int sceSysreg_driver_BD7B035B()
		[HLEFunction(nid : 0xBD7B035B, version : 150)]
		public virtual int sceSysreg_driver_BD7B035B()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBE03D832, version = 150) public int sceSysregAudioClkoutClkEnable()
		[HLEFunction(nid : 0xBE03D832, version : 150)]
		public virtual int sceSysregAudioClkoutClkEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBE1FF8BD, version = 150) public int sceSysreg_driver_BE1FF8BD()
		[HLEFunction(nid : 0xBE1FF8BD, version : 150)]
		public virtual int sceSysreg_driver_BE1FF8BD()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC11B5C0D, version = 150) public int sceSysregUsbIoDisable()
		[HLEFunction(nid : 0xC11B5C0D, version : 150)]
		public virtual int sceSysregUsbIoDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC1A37B37, version = 150) public int sceSysregKirkResetEnable()
		[HLEFunction(nid : 0xC1A37B37, version : 150)]
		public virtual int sceSysregKirkResetEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC2E0E869, version = 150) public int sceSysregAwEdramBusClockEnable()
		[HLEFunction(nid : 0xC2E0E869, version : 150)]
		public virtual int sceSysregAwEdramBusClockEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC2F3061F, version = 150) public int sceSysreg_driver_C2F3061F()
		[HLEFunction(nid : 0xC2F3061F, version : 150)]
		public virtual int sceSysreg_driver_C2F3061F()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC36775AD, version = 150) public int sceSysregAudioClkSelect()
		[HLEFunction(nid : 0xC36775AD, version : 150)]
		public virtual int sceSysregAudioClkSelect()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC4C21CAB, version = 150) public int sceSysregMeBusClockDisable()
		[HLEFunction(nid : 0xC4C21CAB, version : 150)]
		public virtual int sceSysregMeBusClockDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC60FAFB4, version = 150) public int sceSysregAudioIoDisable()
		[HLEFunction(nid : 0xC60FAFB4, version : 150)]
		public virtual int sceSysregAudioIoDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC6C75585, version = 150) public int sceSysreg_driver_C6C75585()
		[HLEFunction(nid : 0xC6C75585, version : 150)]
		public virtual int sceSysreg_driver_C6C75585()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC9585F8E, version = 150) public int sceSysreg_driver_C9585F8E()
		[HLEFunction(nid : 0xC9585F8E, version : 150)]
		public virtual int sceSysreg_driver_C9585F8E()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD3E23912, version = 150) public int sceSysregUsbhostQueryIntr()
		[HLEFunction(nid : 0xD3E23912, version : 150)]
		public virtual int sceSysregUsbhostQueryIntr()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD3E8F2AF, version = 150) public int sceSysregMsifDelaySelect()
		[HLEFunction(nid : 0xD3E8F2AF, version : 150)]
		public virtual int sceSysregMsifDelaySelect()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD507A82D, version = 150) public int sceSysregAudioClkoutClkDisable()
		[HLEFunction(nid : 0xD507A82D, version : 150)]
		public virtual int sceSysregAudioClkoutClkDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD7AD9705, version = 150) public int sceSysregUsbBusClockEnable()
		[HLEFunction(nid : 0xD7AD9705, version : 150)]
		public virtual int sceSysregUsbBusClockEnable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD8E6CAE0, version = 150) public int sceSysregRequestIntr()
		[HLEFunction(nid : 0xD8E6CAE0, version : 150)]
		public virtual int sceSysregRequestIntr()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDA5B5ED9, version = 150) public int sceSysreg_driver_DA5B5ED9()
		[HLEFunction(nid : 0xDA5B5ED9, version : 150)]
		public virtual int sceSysreg_driver_DA5B5ED9()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDB97C70E, version = 150) public int sceSysregSemaUnlock()
		[HLEFunction(nid : 0xDB97C70E, version : 150)]
		public virtual int sceSysregSemaUnlock()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDED12806, version = 150) public int sceSysregApbTimerClkDisable()
		[HLEFunction(nid : 0xDED12806, version : 150)]
		public virtual int sceSysregApbTimerClkDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE5764EAC, version = 150) public int sceSysregAudioClkoutIoDisable()
		[HLEFunction(nid : 0xE5764EAC, version : 150)]
		public virtual int sceSysregAudioClkoutIoDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEB5C723A, version = 150) public int sceSysregAtaIoDisable()
		[HLEFunction(nid : 0xEB5C723A, version : 150)]
		public virtual int sceSysregAtaIoDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEC03F6E2, version = 150) public int sceSysregUsbAcquireIntr(int mask)
		[HLEFunction(nid : 0xEC03F6E2, version : 150)]
		public virtual int sceSysregUsbAcquireIntr(int mask)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF1924607, version = 150) public int sceSysregEmcddrBusClockDisable()
		[HLEFunction(nid : 0xF1924607, version : 150)]
		public virtual int sceSysregEmcddrBusClockDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF288E58E, version = 150) public int sceSysregMsifBusClockDisable()
		[HLEFunction(nid : 0xF288E58E, version : 150)]
		public virtual int sceSysregMsifBusClockDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF4A3C03A, version = 150) public int sceSysregAtahddIoDisable()
		[HLEFunction(nid : 0xF4A3C03A, version : 150)]
		public virtual int sceSysregAtahddIoDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF9C93DD4, version = 150) public int sceSysreg_driver_F9C93DD4()
		[HLEFunction(nid : 0xF9C93DD4, version : 150)]
		public virtual int sceSysreg_driver_F9C93DD4()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFC0131A7, version = 150) public int sceSysreg_driver_FC0131A7()
		[HLEFunction(nid : 0xFC0131A7, version : 150)]
		public virtual int sceSysreg_driver_FC0131A7()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFC5CDD48, version = 150) public int sceSysregAtaClkDisable()
		[HLEFunction(nid : 0xFC5CDD48, version : 150)]
		public virtual int sceSysregAtaClkDisable()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFF0E07B1, version = 150) public int sceSysreg_driver_FF0E07B1()
		[HLEFunction(nid : 0xFF0E07B1, version : 150)]
		public virtual int sceSysreg_driver_FF0E07B1()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x16909002, version = 150) public int sceSysregAtaBusClockEnable()
		[HLEFunction(nid : 0x16909002, version : 150)]
		public virtual int sceSysregAtaBusClockEnable()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE2A5D1EE, version = 150) public int sceSysregGetTachyonVersion()
		[HLEFunction(nid : 0xE2A5D1EE, version : 150)]
		public virtual int sceSysregGetTachyonVersion()
		{
			// Tachyon = 0x00140000, Baryon = 0x00030600 TA-079 v1 1g
			// Tachyon = 0x00200000, Baryon = 0x00030600 TA-079 v2 1g
			// Tachyon = 0x00200000, Baryon = 0x00040600 TA-079 v3 1g
			// Tachyon = 0x00300000, Baryon = 0x00040600 TA-081 1g
			// Tachyon = 0x00400000, Baryon = 0x00114000 TA-082 1g
			// Tachyon = 0x00400000, Baryon = 0x00121000 TA-086 1g
			// Tachyon = 0x00500000, Baryon = 0x0022B200 TA-085 2g
			// Tachyon = 0x00500000, Baryon = 0x00234000 TA-085 2g
			int tachyon = 0;
			switch (Model.Model)
			{
				case Model.MODEL_PSP_FAT :
					tachyon = 0x00300000;
					break;
				case Model.MODEL_PSP_SLIM:
					tachyon = 0x00500000;
					break;
				default:
					Console.WriteLine(string.Format("sceSysregGetTachyonVersion unknown tachyon version for PSP Model {0}", Model.getModelName(Model.Model)));
					break;
			}
			return tachyon;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4F46EEDE, version = 150) public long sceSysregGetFuseId()
		[HLEFunction(nid : 0x4F46EEDE, version : 150)]
		public virtual long sceSysregGetFuseId()
		{
			// Has no parameters
			return fuseId;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8F4F4E96, version = 150) public int sceSysregGetFuseConfig()
		[HLEFunction(nid : 0x8F4F4E96, version : 150)]
		public virtual int sceSysregGetFuseConfig()
		{
			// Has no parameters
			return fuseConfig;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8835D1E1, version = 150) public int sceSysregSpiClkEnable(int spiId)
		[HLEFunction(nid : 0x8835D1E1, version : 150)]
		public virtual int sceSysregSpiClkEnable(int spiId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8B90B8B5, version = 150) public int sceSysregSpiClkDisable(int spiId)
		[HLEFunction(nid : 0x8B90B8B5, version : 150)]
		public virtual int sceSysregSpiClkDisable(int spiId)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD6628A48, version = 150) public int sceSysregSpiClkSelect(int unknown1, int unknown2)
		[HLEFunction(nid : 0xD6628A48, version : 150)]
		public virtual int sceSysregSpiClkSelect(int unknown1, int unknown2)
		{
			return 0;
		}
	}

}