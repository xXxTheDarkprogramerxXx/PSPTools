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

	public class ScePspDateTime : pspAbstractMemoryMappedStructure
	{
		public static readonly TimeZone GMT = TimeZone.getTimeZone("GMT");
		public const int SIZEOF = 16;
		public int year;
		public int month;
		public int day;
		public int hour;
		public int minute;
		public int second;
		public int microsecond;

		/// <summary>
		/// All fields will be initialised to the time the object was created. </summary>
		public ScePspDateTime()
		{
			DateTime cal = new DateTime();

			year = cal.Year;
			month = 1 + cal.Month;
			day = cal.Day;
			hour = cal.Hour;
			minute = cal.Minute;
			second = cal.Second;
			microsecond = cal.Millisecond * 1000;
		}

		public ScePspDateTime(int timezone)
		{
			DateTime cal = new DateTime();
			int minutes = timezone;
			int hours = 0;
			while (minutes > 59)
			{
				hours++;
				minutes -= 60;
			}

			string timeString = string.Format("UTC+{0:D2}{1:D2}", hours, minutes);
			TimeZone tz = TimeZone.getTimeZone(timeString);
			cal.TimeZone = tz;

			year = cal.Year;
			month = 1 + cal.Month;
			day = cal.Day;
			hour = cal.Hour;
			minute = cal.Minute;
			second = cal.Second;
			microsecond = cal.Millisecond * 1000;
		}

		public ScePspDateTime(int year, int month, int day, int hour, int minute, int second, int microsecond)
		{
			this.year = year;
			this.month = month;
			this.day = day;
			this.hour = hour;
			this.minute = minute;
			this.second = second;
			this.microsecond = microsecond;
		}

		/// <param name="time"> MSDOS time, as coded in FAT directory entries. </param>
		public static ScePspDateTime fromMSDOSTime(int time)
		{
			int year = 1980 + ((time >> 25) & 0x7F);
			int month = (time >> 21) & 0xF;
			int day = (time >> 16) & 0x1F;
			int hour = (time >> 11) & 0x1F;
			int minute = (time >> 5) & 0x3F;
			int second = ((time >> 0) & 0x1F) << 1; // 2 seconds resolution
			int microsecond = 0;

			return new ScePspDateTime(year, month, day, hour, minute, second, microsecond);
		}

		public virtual int toMSDOSTime()
		{
			int time = 0;
			time |= ((year - 1980) & 0x7F) << 25;
			time |= (month & 0xF) << 21;
			time |= (day & 0x1F) << 16;
			time |= (hour & 0x1F) << 11;
			time |= (minute & 0x3F) << 5;
			time |= ((second >> 1) & 0x1F) << 0; // 2 seconds resolution

			return time;
		}

		/// <param name="time"> FILETIME time, 100 nanoseconds since the epoch/1601. </param>
		public static ScePspDateTime fromFILETIMETime(long time)
		{
			// Calculate each time parameter.
			long milliseconds = time / 10000;
			long days = milliseconds / (24 * 60 * 60 * 1000);
			milliseconds -= days * (24 * 60 * 60 * 1000);
			long hours = milliseconds / (60 * 60 * 1000);
			milliseconds -= hours * (60 * 60 * 1000);
			long minutes = milliseconds / (60 * 1000);
			milliseconds -= minutes * (60 * 1000);
			long seconds = milliseconds / 1000;
			milliseconds -= seconds * 1000;
			// Initialize a new calendar and set it for the right epoch.
			DateTime cal = new DateTime();
			cal = new DateTime(1601, 1, 1, 0, 0, 0);
			cal.AddDays((int)days);
			cal.AddHours((int)hours);
			cal.AddMinutes((int)minutes);
			cal.AddSeconds((int)seconds);
			cal.AddMilliseconds((int)milliseconds);

			int year = cal.Year;
			int month = 1 + cal.Month;
			int day = cal.Day;
			int hour = cal.Hour;
			int minute = cal.Minute;
			int second = cal.Second;
			int microsecond = cal.Millisecond * 1000;

			return new ScePspDateTime(year, month, day, hour, minute, second, microsecond);
		}

		/// <param name="time"> Unix time, seconds since the epoch/1970. </param>
		public static ScePspDateTime fromUnixTime(long time)
		{
			DateTime cal = new DateTime();
			DateTime date = new DateTime(time);
			cal = new DateTime(date);

			int year = cal.Year;
			int month = 1 + cal.Month;
			int day = cal.Day;
			int hour = cal.Hour;
			int minute = cal.Minute;
			int second = cal.Second;
			int microsecond = cal.Millisecond * 1000;

			return new ScePspDateTime(year, month, day, hour, minute, second, microsecond);
		}

		/// <param name="microseconds"> </param>
		public static ScePspDateTime fromMicros(long micros)
		{
			DateTime cal = DateTime.getInstance(GMT);
			DateTime date = new DateTime(micros / 1000L);
			cal = new DateTime(date);

			int year = cal.Year;
			int month = 1 + cal.Month;
			int day = cal.Day;
			int hour = cal.Hour;
			int minute = cal.Minute;
			int second = cal.Second;
			int microsecond = (int)(micros % 1000000L);

			return new ScePspDateTime(year, month, day, hour, minute, second, microsecond);
		}

		protected internal override void read()
		{
			year = read16();
			month = read16();
			day = read16();
			hour = read16();
			minute = read16();
			second = read16();
			microsecond = read32();
		}

		protected internal override void write()
		{
			write16((short) year);
			write16((short) month);
			write16((short) day);
			write16((short) hour);
			write16((short) minute);
			write16((short) second);
			write32(microsecond);
		}

		public override int @sizeof()
		{
			return SIZEOF;
		}

		public override string ToString()
		{
			return string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2} micros={6:D}", year, month, day, hour, minute, second, microsecond);
		}
	}
}