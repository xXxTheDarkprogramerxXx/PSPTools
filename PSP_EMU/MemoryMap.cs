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
namespace pspsharp
{
	public class MemoryMap
	{
		public const int START_SCRATCHPAD = 0x00010000;
		public const int END_SCRATCHPAD = 0x00013FFF;
		public static readonly int SIZE_SCRATCHPAD = END_SCRATCHPAD - START_SCRATCHPAD + 1;

		public const int START_VRAM = 0x04000000; // KU0
		public const int END_VRAM = 0x041FFFFF; // KU0
		public static readonly int SIZE_VRAM = END_VRAM - START_VRAM + 1;

		public const int START_RAM = 0x08000000;
		public const int END_RAM_32MB = 0x09FFFFFF;
		public const int END_RAM_64MB = 0x0BFFFFFF;
		public static int END_RAM = END_RAM_32MB;
		public static int SIZE_RAM = END_RAM - START_RAM + 1;

		public const int START_IO_0 = unchecked((int)0xBC000000);
		public const int END_IO_0 = unchecked((int)0xBFBFFFFF);

		public const int START_IO_1 = unchecked((int)0xBFD00000);
		public const int END_IO_1 = unchecked((int)0xBFFFFFFF);

		public const int START_EXCEPTIO_VEC = unchecked((int)0xBFC00000);
		public const int END_EXCEPTIO_VEC = unchecked((int)0xBFCFFFFF);

		public const int START_KERNEL = unchecked((int)0x88000000); // K0
		public const int END_KERNEL = unchecked((int)0x887FFFFF); // K0

		public const int START_USERSPACE = 0x08800000; // KU0
		public const int END_USERSPACE_32MB = 0x09FFFFFF;
		public const int END_USERSPACE_64MB = 0x0BFFFFFF;
		public static int END_USERSPACE = END_USERSPACE_32MB; // KU0

		public const int START_UNCACHED_RAM_VIDEO = 0x44000000;
		public const int END_UNCACHED_RAM_VIDEO = 0x441FFFFF;
	}

}