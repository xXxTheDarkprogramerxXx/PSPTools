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
	 * GPS Satellite Data Structure for sceUsbGpsGetData().
	 * Based on MapThis! homebrew v5.2
	 */
	public class pspUsbGpsSatData : pspAbstractMemoryMappedStructure
	{
		public const int MAX_SATELLITES = 24;
		public short satellitesInView;
		public short garbage;
		public pspUsbGpsSatInfo[] satInfo;

		protected internal override void read()
		{
			satellitesInView = (short) read16();
			garbage = (short) read16();
			satInfo = new pspUsbGpsSatInfo[System.Math.Min(satellitesInView, MAX_SATELLITES)];
			for (int i = 0; i < satInfo.Length; i++)
			{
				satInfo[i] = new pspUsbGpsSatInfo();
				read(satInfo[i]);
			}
		}

		protected internal override void write()
		{
			write16(satellitesInView);
			write16(garbage);
			for (int i = 0; satInfo != null && i < satInfo.Length; i++)
			{
				write(satInfo[i]);
			}
		}

		public virtual int SatellitesInView
		{
			set
			{
				this.satellitesInView = (short) value;
				satInfo = new pspUsbGpsSatInfo[value];
				for (int i = 0; i < value; i++)
				{
					satInfo[i] = new pspUsbGpsSatInfo();
				}
			}
		}

		public override int @sizeof()
		{
			return 4 + MAX_SATELLITES * pspUsbGpsSatInfo.SIZEOF;
		}
	}

}