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

	using pspUsbGpsData = pspsharp.HLE.kernel.types.pspUsbGpsData;
	using pspUsbGpsSatData = pspsharp.HLE.kernel.types.pspUsbGpsSatData;
	using GPS = pspsharp.hardware.GPS;

	public class sceUsbGps : HLEModule
	{
		public static Logger log = Modules.getLogger("sceUsbGps");
		public const int GPS_STATE_OFF = 0;
		// "MapThis!" is describing both following states as "Activating"
		public const int GPS_STATE_ACTIVATING1 = 1;
		public const int GPS_STATE_ACTIVATING2 = 2;
		public const int GPS_STATE_ON = 3;
		protected internal int gpsState;

		public override void start()
		{
			gpsState = GPS_STATE_ON;
			base.start();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x268F95CA, version = 271) public int sceUsbGpsSetInitDataLocation(long unknown)
		[HLEFunction(nid : 0x268F95CA, version : 271)]
		public virtual int sceUsbGpsSetInitDataLocation(long unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x31F95CDE, version = 271) public int sceUsbGpsGetPowerSaveMode()
		[HLEFunction(nid : 0x31F95CDE, version : 271)]
		public virtual int sceUsbGpsGetPowerSaveMode()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x54D26AA4, version = 271) public int sceUsbGpsGetInitDataLocation(pspsharp.HLE.TPointer64 unknown)
		[HLEFunction(nid : 0x54D26AA4, version : 271)]
		public virtual int sceUsbGpsGetInitDataLocation(TPointer64 unknown)
		{
			unknown.Value = 0;
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x63D1F89D, version = 271) public int sceUsbGpsResetInitialPosition()
		[HLEFunction(nid : 0x63D1F89D, version : 271)]
		public virtual int sceUsbGpsResetInitialPosition()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x69E4AAA8, version = 271) public int sceUsbGpsSaveInitData()
		[HLEFunction(nid : 0x69E4AAA8, version : 271)]
		public virtual int sceUsbGpsSaveInitData()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6EED4811, version = 271) public int sceUsbGpsClose()
		[HLEFunction(nid : 0x6EED4811, version : 271)]
		public virtual int sceUsbGpsClose()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7C16AC3A, version = 271) public int sceUsbGpsGetState(pspsharp.HLE.TPointer32 stateAddr)
		[HLEFunction(nid : 0x7C16AC3A, version : 271)]
		public virtual int sceUsbGpsGetState(TPointer32 stateAddr)
		{
			stateAddr.setValue(gpsState);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x934EC2B2, version = 271) public int sceUsbGpsGetData(@CanBeNull pspsharp.HLE.TPointer gpsDataAddr, @CanBeNull pspsharp.HLE.TPointer satDataAddr)
		[HLEFunction(nid : 0x934EC2B2, version : 271)]
		public virtual int sceUsbGpsGetData(TPointer gpsDataAddr, TPointer satDataAddr)
		{
			if (gpsDataAddr.NotNull)
			{
				pspUsbGpsData gpsData = new pspUsbGpsData();
				gpsData.Calendar = DateTime.getInstance(TimeZone.getTimeZone("GMT"));
				gpsData.hdop = 1f;

				// Return location of New-York City
				gpsData.latitude = GPS.PositionLatitude;
				gpsData.longitude = GPS.PositionLongitude;
				gpsData.altitude = GPS.PositionAltitude;

				gpsData.speed = 0f;
				gpsData.bearing = 180f;
				gpsData.write(gpsDataAddr);
			}

			if (satDataAddr.NotNull)
			{
				pspUsbGpsSatData satData = new pspUsbGpsSatData();

				// Simulate 6 satellites in view
				const int satellitesInView = 6;
				satData.SatellitesInView = satellitesInView;
				for (int i = 0; i < satellitesInView; i++)
				{
					satData.satInfo[i].id = i;
					satData.satInfo[i].elevation = 0;
					satData.satInfo[i].azimuth = (short)(i * (360 / satellitesInView)); // Assume azimuth is in range 0..360
					satData.satInfo[i].snr = 0;
					satData.satInfo[i].good = 1;
				}
				satData.write(satDataAddr);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9D8F99E8, version = 271) public int sceUsbGpsSetPowerSaveMode(int unknown1, int unknown2)
		[HLEFunction(nid : 0x9D8F99E8, version : 271)]
		public virtual int sceUsbGpsSetPowerSaveMode(int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9F267D34, version = 271) public int sceUsbGpsOpen()
		[HLEFunction(nid : 0x9F267D34, version : 271)]
		public virtual int sceUsbGpsOpen()
		{
			GPS.initialize();

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA259CD67, version = 271) public int sceUsbGpsReset(int unknown)
		[HLEFunction(nid : 0xA259CD67, version : 271)]
		public virtual int sceUsbGpsReset(int unknown)
		{
			return 0;
		}
	}

}