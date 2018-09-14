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
	public class SceMpegYCrCbBufferSrc : pspAbstractMemoryMappedStructure
	{
		public int frameHeight;
		public int frameWidth;
		public int unknown1;
		public int unknown2;
		public int bufferY;
		public int bufferY2;
		public int bufferCr;
		public int bufferCb;
		public int bufferCr2;
		public int bufferCb2;

		protected internal override void read()
		{
			frameHeight = read32();
			frameWidth = read32();
			unknown1 = read32();
			unknown2 = read32();
			bufferY = read32();
			bufferY2 = read32();
			bufferCr = read32();
			bufferCb = read32();
			bufferCr2 = read32();
			bufferCb2 = read32();
		}

		protected internal override void write()
		{
			write32(frameHeight);
			write32(frameWidth);
			write32(unknown1);
			write32(unknown2);
			write32(bufferY);
			write32(bufferY2);
			write32(bufferCr);
			write32(bufferCb);
			write32(bufferCr2);
			write32(bufferCb2);
		}

		public override int @sizeof()
		{
			return 40;
		}

		public override string ToString()
		{
			return string.Format("height={0:D}, width={1:D}, bufferY=0x{2:X8}, bufferY2=0x{3:X8}, bufferCr=0x{4:X8}, bufferCb=0x{5:X8}, bufferCr2=0x{6:X8}, bufferCb2=0x{7:X8}", frameHeight, frameWidth, bufferY, bufferY2, bufferCr, bufferCb, bufferCr2, bufferCb2);
		}
	}

}