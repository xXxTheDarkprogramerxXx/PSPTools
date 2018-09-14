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
namespace pspsharp.HLE.VFS.fat
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.Fat32VirtualFile.sectorSize;

	using Utilities = pspsharp.util.Utilities;

	public class FatUtils
	{
		public static int getSectorNumber(long position)
		{
			return (int)(position / sectorSize);
		}

		public static int getSectorOffset(long position)
		{
			return (int)(position % sectorSize);
		}

		public static void storeSectorInt32(sbyte[] sector, int offset, int value)
		{
			Utilities.writeUnaligned32(sector, offset, value);
		}

		public static void storeSectorInt8(sbyte[] sector, int offset, int value)
		{
			sector[offset] = (sbyte) value;
		}

		public static void storeSectorInt16(sbyte[] sector, int offset, int value)
		{
			Utilities.writeUnaligned16(sector, offset, value);
		}

		public static int readSectorInt32(sbyte[] sector, int offset)
		{
			return Utilities.readUnaligned32(sector, offset);
		}

		public static int readSectorInt16(sbyte[] sector, int offset)
		{
			return Utilities.readUnaligned16(sector, offset);
		}

		public static int readSectorInt8(sbyte[] sector, int offset)
		{
			return Utilities.read8(sector, offset);
		}

		public static string readSectorString(sbyte[] sector, int offset, int length)
		{
			string s = "";
			// Skip any trailing spaces
			for (int i = length - 1; i >= 0; i--)
			{
				if (sector[offset + i] != (sbyte) ' ')
				{
					s = StringHelper.NewString(sector, offset, i + 1);
					break;
				}
			}

			return s;
		}

		public static void storeSectorString(sbyte[] sector, int offset, string value, int length)
		{
			int stringLength = System.Math.Min(value.Length, length);
			Utilities.writeStringNZ(sector, offset, stringLength, value);

			// Fill rest with spaces
			for (int i = stringLength; i < length; i++)
			{
				sector[offset + i] = (sbyte) ' ';
			}
		}

		public static FatFileInfo[] extendArray(FatFileInfo[] array, int extend)
		{
			if (array == null)
			{
				return new FatFileInfo[extend];
			}

			FatFileInfo[] newArray = new FatFileInfo[array.Length + extend];
			Array.Copy(array, 0, newArray, 0, array.Length);

			return newArray;
		}
	}

}