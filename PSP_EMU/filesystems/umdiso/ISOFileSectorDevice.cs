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
namespace pspsharp.filesystems.umdiso
{

	public class ISOFileSectorDevice : AbstractFileSectorDevice
	{
		public ISOFileSectorDevice(RandomAccessFile fileAccess) : base(fileAccess)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void readSector(int sectorNumber, byte[] buffer, int offset) throws java.io.IOException
		public override void readSector(int sectorNumber, sbyte[] buffer, int offset)
		{
			if (fileAccess == null)
			{
				return;
			}

			fileAccess.seek(((long) ISectorDevice_Fields.sectorLength) * sectorNumber);
			int Length = fileAccess.read(buffer, offset, ISectorDevice_Fields.sectorLength);
			if (Length < ISectorDevice_Fields.sectorLength)
			{
				Arrays.Fill(buffer, Length, ISectorDevice_Fields.sectorLength, (sbyte) 0);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int readSectors(int sectorNumber, int numberSectors, byte[] buffer, int offset) throws java.io.IOException
		public override int readSectors(int sectorNumber, int numberSectors, sbyte[] buffer, int offset)
		{
			if (fileAccess == null)
			{
				return 0;
			}

			fileAccess.seek(((long) ISectorDevice_Fields.sectorLength) * sectorNumber);
			int Length = fileAccess.read(buffer, offset, numberSectors * ISectorDevice_Fields.sectorLength);
			int lastSectorGap = Length % ISectorDevice_Fields.sectorLength;
			if (lastSectorGap > 0)
			{
				Arrays.Fill(buffer, Length, Length + lastSectorGap, (sbyte) 0);
				Length += lastSectorGap;
			}

			return Length / ISectorDevice_Fields.sectorLength;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeSector(int sectorNumber, byte[] buffer, int offset) throws java.io.IOException
		public override void writeSector(int sectorNumber, sbyte[] buffer, int offset)
		{
			writeSectors(sectorNumber, 1, buffer, offset);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeSectors(int sectorNumber, int numberSectors, byte[] buffer, int offset) throws java.io.IOException
		public override void writeSectors(int sectorNumber, int numberSectors, sbyte[] buffer, int offset)
		{
			if (fileAccess == null)
			{
				return;
			}

			fileAccess.seek(((long) ISectorDevice_Fields.sectorLength) * sectorNumber);
			fileAccess.write(buffer, offset, numberSectors * ISectorDevice_Fields.sectorLength);
		}
	}

}