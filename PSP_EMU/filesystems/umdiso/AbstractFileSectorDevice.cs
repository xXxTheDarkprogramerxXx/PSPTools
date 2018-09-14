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

	using Logger = org.apache.log4j.Logger;

	public abstract class AbstractFileSectorDevice : ISectorDevice
	{
		public abstract void readSector(int sectorNumber, sbyte[] buffer, int offset);
		protected internal static Logger log = Emulator.log;
		protected internal RandomAccessFile fileAccess;

		public AbstractFileSectorDevice(RandomAccessFile fileAccess)
		{
			this.fileAccess = fileAccess;
		}

		protected internal virtual int getNumSectors(long lengthInBytes, int ISectorDevice_Fields)
		{
			// Allow a last sector only partially available
			return (int)((lengthInBytes + ISectorDevice_Fields.sectorLength - 1) / ISectorDevice_Fields.sectorLength);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int getNumSectors() throws java.io.IOException
		public virtual int NumSectors
		{
			get
			{
				return getNumSectors(fileAccess.length(), ISectorDevice_Fields.sectorLength);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public virtual void close()
		{
			fileAccess.close();
			fileAccess = null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int readSectors(int sectorNumber, int numberSectors, byte[] buffer, int offset) throws java.io.IOException
		public virtual int readSectors(int sectorNumber, int numberSectors, sbyte[] buffer, int offset)
		{
			for (int i = 0; i < numberSectors; i++)
			{
				readSector(sectorNumber + i, buffer, offset + i * ISectorDevice_Fields.sectorLength);
			}

			return numberSectors;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeSector(int sectorNumber, byte[] buffer, int offset) throws java.io.IOException
		public virtual void writeSector(int sectorNumber, sbyte[] buffer, int offset)
		{
			throw new IOException("Device is read-only");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeSectors(int sectorNumber, int numberSectors, byte[] buffer, int offset) throws java.io.IOException
		public virtual void writeSectors(int sectorNumber, int numberSectors, sbyte[] buffer, int offset)
		{
			throw new IOException("Device is read-only");
		}
	}

}