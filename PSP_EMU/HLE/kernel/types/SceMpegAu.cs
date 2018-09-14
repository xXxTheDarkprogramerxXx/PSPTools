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
	public class SceMpegAu : pspAbstractMemoryMappedStructure
	{
		// Presentation TimeStamp
		public long pts;
		// Decode TimeStamp
		public long dts;
		// Es buffer address
		public int esBuffer;
		// Size of data in esBuffer
		public int esSize;

		public SceMpegAu()
		{
			pts = -1;
			dts = -1;
			esBuffer = 0;
			esSize = 0;
		}

		protected internal virtual long readTimeStamp()
		{
			int msb = read32();
			int lsb = read32();

			return (((long) msb) << 32) | (((long) lsb) & 0xFFFFFFFFL);
		}

		protected internal virtual void writeTimeStamp(long ts)
		{
			int msb = (int)(ts >> 32);
			int lsb = (int) ts;

			write32(msb);
			write32(lsb);
		}

		protected internal override void read()
		{
			pts = readTimeStamp();
			dts = readTimeStamp();
			esBuffer = read32();
			esSize = read32();
		}

		protected internal override void write()
		{
			writeTimeStamp(pts);
			writeTimeStamp(dts);
			write32(esBuffer);
			write32(esSize);
		}

		public override int @sizeof()
		{
			return 24;
		}

		private static string formatTimestamp(long timestamp)
		{
			if (timestamp == -1L)
			{
				return "-1";
			}
			return string.Format("0x{0:X}", timestamp);
		}

		public override string ToString()
		{
			return string.Format("pts={0}, dts={1}, esBuffer=0x{2:X8}, esSize=0x{3:X}", formatTimestamp(pts), formatTimestamp(dts), esBuffer, esSize);
		}
	}

}