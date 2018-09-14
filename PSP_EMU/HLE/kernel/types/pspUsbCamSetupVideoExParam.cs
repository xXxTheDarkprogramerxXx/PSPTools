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
	 * Parameter Structure for sceUsbCamSetupVideoEx().
	 */
	public class pspUsbCamSetupVideoExParam : pspAbstractMemoryMappedStructureVariableLength
	{
		public int unknown1; // Unknown. Set it to 9 at the moment.
		public int resolution; // Resolution. One of PSP_USBCAM_RESOLUTION_EX_*
		public int framerate; // Framerate. One of PSP_USBCAM_FRAMERATE_*
		public int unknown2; // Unknown. Set it to 2 at the moment.
		public int unknown3; // Unknown. Set it to 3 at the moment.
		public int wb; // White balance. One of PSP_USBCAM_WB_*
		public int saturation; // Saturation (0-255)
		public int brightness; // Brightness (0-255)
		public int contrast; // Contrast (0-255)
		public int sharpness; // Sharpness (0-255)
		public int unknown4; // Unknown. Set it to 0 at the moment.
		public int unknown5; // Unknown. Set it to 1 at the moment.
		public int unknown6; // Unknown. Set it to 0 at the moment.
		public int unknown7; // Unknown. Set it to 0 at the moment.
		public int unknown8; // Unknown. Set it to 0 at the moment.
		public int effectmode; // Effect mode. One of PSP_USBCAM_EFFECTMODE_*
		public int unknown9; // Unknown. Set it to 1 at the moment.
		public int unknown10; // Unknown. Set it to 10 at the moment.
		public int unknown11; // Unknown. Set it to 2 at the moment.
		public int unknown12; // Unknown. Set it to 500 at the moment.
		public int unknown13; // Unknown. Set it to 1000 at the moment.
		public int framesize; // Size of jpeg video frame
		public int unknown14; // Unknown. Set it to 0 at the moment.
		public int evlevel; // Exposure value. One of PSP_USBCAM_EVLEVEL_*

		protected internal override void read()
		{
			base.read();
			unknown1 = read32();
			resolution = read32();
			framerate = read32();
			unknown2 = read32();
			unknown3 = read32();
			wb = read32();
			saturation = read32();
			brightness = read32();
			contrast = read32();
			sharpness = read32();
			unknown4 = read32();
			unknown5 = read32();
			unknown6 = read32();
			unknown7 = read32();
			unknown8 = read32();
			effectmode = read32();
			unknown9 = read32();
			unknown10 = read32();
			unknown11 = read32();
			unknown12 = read32();
			unknown13 = read32();
			framesize = read32();
			unknown14 = read32();
			evlevel = read32();
		}

		protected internal override void write()
		{
			base.write();
			write32(unknown1);
			write32(resolution);
			write32(framerate);
			write32(unknown2);
			write32(unknown3);
			write32(wb);
			write32(saturation);
			write32(brightness);
			write32(contrast);
			write32(sharpness);
			write32(unknown4);
			write32(unknown5);
			write32(unknown6);
			write32(unknown7);
			write32(unknown8);
			write32(effectmode);
			write32(unknown9);
			write32(unknown10);
			write32(unknown11);
			write32(unknown12);
			write32(unknown13);
			write32(framesize);
			write32(unknown14);
			write32(evlevel);
		}

		public override string ToString()
		{
			return string.Format("pspUsbCamSetupVideoExParam[size={0:D}, resolution={1:D}, framerate={2:D}, wb={3:D}, saturation={4:D}, brightness={5:D}, contrast={6:D}, sharpness={7:D}, effectmode={8:D}, framesize={9:D}, evlevel={10:D}]", @sizeof(), resolution, framerate, wb, saturation, brightness, contrast, sharpness, effectmode, framesize, evlevel);
		}
	}

}