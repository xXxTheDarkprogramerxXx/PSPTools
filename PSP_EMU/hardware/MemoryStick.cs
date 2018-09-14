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
namespace pspsharp.hardware
{
	using Utilities = pspsharp.util.Utilities;

	public class MemoryStick
	{
		// States for mscmhc0 (used in callbacks).
		public const int PSP_MEMORYSTICK_STATE_DRIVER_READY = 1;
		public const int PSP_MEMORYSTICK_STATE_DRIVER_BUSY = 2;
		public const int PSP_MEMORYSTICK_STATE_DEVICE_INSERTED = 4;
		public const int PSP_MEMORYSTICK_STATE_DEVICE_REMOVED = 8;
		// States for fatms0 (used in callbacks).
		public const int PSP_FAT_MEMORYSTICK_STATE_UNASSIGNED = 0;
		public const int PSP_FAT_MEMORYSTICK_STATE_ASSIGNED = 1;
		public const int PSP_FAT_MEMORYSTICK_STATE_REMOVED = 2;
		// MS and FatMS states.
		private static int msState = PSP_MEMORYSTICK_STATE_DRIVER_READY;
		private static int fatMsState = PSP_FAT_MEMORYSTICK_STATE_ASSIGNED;

		// Memory Stick power
		private static bool msPower = true;

		// Total size of the memory stick, in bytes
	//    private static long totalSize = 64L * 1024 * 1024; // 64MB
		private static long totalSize = 16L * 1024 * 1024 * 1024; // 16GB
		// Free size on memory stick, in bytes
		private static long freeSize = 1L * 1024 * 1024 * 1024; // 1GB
		private const int sectorSize = 32 * 1024; // 32KB

		private static bool locked = false;

		public static int StateMs
		{
			get
			{
				return msState;
			}
			set
			{
				MemoryStick.msState = value;
			}
		}


		public static int StateFatMs
		{
			get
			{
				return fatMsState;
			}
			set
			{
				MemoryStick.fatMsState = value;
			}
		}


		public static bool Inserted
		{
			get
			{
				return fatMsState != PSP_FAT_MEMORYSTICK_STATE_REMOVED;
			}
		}

		public static long FreeSize
		{
			get
			{
				return freeSize;
			}
		}

		public static int FreeSizeKb
		{
			get
			{
				return Utilities.getSizeKb(FreeSize);
			}
		}

		public static int SectorSize
		{
			get
			{
				return sectorSize;
			}
		}

		public static int SectorSizeKb
		{
			get
			{
				return Utilities.getSizeKb(SectorSize);
			}
		}

		public static int getSize32Kb(int sizeKb)
		{
			return (sizeKb + 31) & ~31;
		}

		public static string getSizeKbString(int sizeKb)
		{
			if (sizeKb < 3 * 1024)
			{
				return string.Format("{0:D} KB", sizeKb);
			}
			sizeKb /= 1024;
			if (sizeKb < 3 * 1024)
			{
				return string.Format("{0:D} MB", sizeKb);
			}
			sizeKb /= 1024;
			return string.Format("{0:D} GB", sizeKb);
		}

		public static bool Locked
		{
			get
			{
				return locked;
			}
			set
			{
				MemoryStick.locked = value;
			}
		}


		public static bool hasMsPower()
		{
			return msPower;
		}

		public static bool MsPower
		{
			set
			{
				MemoryStick.msPower = value;
			}
		}

		public static long TotalSize
		{
			get
			{
				return totalSize;
			}
			set
			{
				MemoryStick.totalSize = value;
				if (freeSize > value)
				{
					freeSize = value;
				}
			}
		}

	}
}