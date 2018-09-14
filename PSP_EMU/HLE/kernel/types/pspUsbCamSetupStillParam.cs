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
	 * Parameter Structure for sceUsbCamSetupStill().
	 */
	public class pspUsbCamSetupStillParam : pspAbstractMemoryMappedStructureVariableLength
	{
		public int resolution; // Resolution. One of PSP_USBCAM_RESOLUTION_*
		public int jpegsize; // Size of the jpeg image
		public int reverseflags; // Reverse effect to apply. Zero or more of PSP_USBCAM_FLIP, PSP_USBCAM_MIRROR
		public int delay; // Delay to apply to take the picture. One of PSP_USBCAM_DELAY_*
		public int complevel; // JPEG compression level, a value from 1-63.
								 // 1 -> less compression, better quality;
								 // 63 -> max compression, worse quality

		protected internal override void read()
		{
			base.read();
			resolution = read32();
			jpegsize = read32();
			reverseflags = read32();
			delay = read32();
			complevel = read32();
		}

		protected internal override void write()
		{
			base.write();
			write32(resolution);
			write32(jpegsize);
			write32(reverseflags);
			write32(delay);
			write32(complevel);
		}

		public override string ToString()
		{
			return string.Format("pspUsbCamSetupStillParam[size={0:D}, resolution={1:D}, jpegsize={2:D}, reverseflags={3:D}, delay={4:D}, complevel={5:D}]", @sizeof(), resolution, jpegsize, reverseflags, delay, complevel);
		}
	}

}