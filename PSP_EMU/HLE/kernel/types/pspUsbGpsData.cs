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
namespace pspsharp.HLE.kernel.types
{

	/*
	 * GPS Data Structure for sceUsbGpsGetData().
	 * Based on MapThis! homebrew v5.2
	 */
	public class pspUsbGpsData : pspAbstractMemoryMappedStructure
	{
		public short year;
		public short month;
		public short date;
		public short hour;
		public short minute;
		public short second;
		public float garbage1;
		public float hdop; // Horizontal Dilution of Precision: <1 ideal, 1-2 excellent, 2-5 good, 5-10 moderate, 10-20 fair, >20 poor
		public float garbage2;
		public float latitude;
		public float longitude;
		public float altitude; // in meters
		public float garbage3;
		public float speed; // in meters/second ??
		public float bearing; // 0..360

		protected internal override void read()
		{
			year = (short) read16();
			month = (short) read16();
			date = (short) read16();
			hour = (short) read16();
			minute = (short) read16();
			second = (short) read16();
			garbage1 = readFloat();
			hdop = readFloat();
			garbage2 = readFloat();
			latitude = readFloat();
			longitude = readFloat();
			altitude = readFloat();
			garbage3 = readFloat();
			speed = readFloat();
			bearing = readFloat();
		}

		protected internal override void write()
		{
			write16(year);
			write16(month);
			write16(date);
			write16(hour);
			write16(minute);
			write16(second);
			writeFloat(garbage1);
			writeFloat(hdop);
			writeFloat(garbage2);
			writeFloat(latitude);
			writeFloat(longitude);
			writeFloat(altitude);
			writeFloat(garbage3);
			writeFloat(speed);
			writeFloat(bearing);
		}

		public virtual DateTime Calendar
		{
			set
			{
				year = (short) value.Year;
				month = (short) value.Month;
				date = (short) value.Day;
				hour = (short) value.Hour;
				minute = (short) value.Minute;
				second = (short) value.Second;
			}
		}

		public override int @sizeof()
		{
			return 48;
		}
	}

}