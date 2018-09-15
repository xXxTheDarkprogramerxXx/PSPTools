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

	//using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// Implements a SectorDevice where the sectors are stored in random
	/// order in another SectorDevice. Not all the sectors need to be available.
	/// Writing a new sectors is also supported by adding the new sectors at the
	/// end of the mapped SectorDevice.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class MappingSectorDevice : ISectorDevice
	{
		protected internal static Logger log = Emulator.log;
		protected internal const int freeSectorNumber = -1;
		protected internal ISectorDevice sectorDevice;
		protected internal int[] sectorMapping;
		protected internal File mappingFile;
		protected internal bool sectorMappingDirty;

		public MappingSectorDevice(ISectorDevice sectorDevice, File mappingFile)
		{
			this.sectorDevice = sectorDevice;
			this.mappingFile = mappingFile;

			// Create a default empty mapping in case the mapping file cannot be read
			sectorMapping = new int[0];
			sectorMappingDirty = true;

			try
			{
				readMappingFile();
			}
			catch (FileNotFoundException e)
			{
				Console.WriteLine("Mapping file not found, creating it", e);
			}
			catch (IOException e)
			{
				Console.WriteLine("Error reading mapping file", e);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setNumSectors(int numSectors) throws java.io.IOException
		public virtual int NumSectors
		{
			set
			{
				int previousNumSectors = NumSectors;
				if (value != previousNumSectors)
				{
					// Shrink or extend the sectorMapping array
					sectorMapping = Array.copyOf(sectorMapping, value);
					if (value > previousNumSectors)
					{
						// Extending the sector mapping with -1 values
						Arrays.Fill(sectorMapping, previousNumSectors, sectorMapping.Length, freeSectorNumber);
					}
					sectorMappingDirty = true;
				}
			}
			get
			{
				return sectorMapping.Length;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void readMappingFile() throws java.io.IOException
		protected internal virtual void readMappingFile()
		{
			System.IO.Stream mappingFileReader = new System.IO.FileStream(mappingFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			int mappingSize = (int)(mappingFile.Length() / 4);
			sectorMapping = new int[mappingSize];
			sbyte[] buffer = new sbyte[4];
			IntBuffer intBuffer = ByteBuffer.wrap(buffer).asIntBuffer();
			for (int i = 0; i < mappingSize; i++)
			{
				mappingFileReader.Read(buffer, 0, buffer.Length);
				sectorMapping[i] = intBuffer.get(0);
			}
			mappingFileReader.Close();

			sectorMappingDirty = false;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void writeMappingFile() throws java.io.IOException
		protected internal virtual void writeMappingFile()
		{
			System.IO.Stream mappingFileWriter = new System.IO.FileStream(mappingFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
			sbyte[] buffer = new sbyte[4];
			IntBuffer intBuffer = ByteBuffer.wrap(buffer).asIntBuffer();
			int numSectors = NumSectors;
			for (int i = 0; i < numSectors; i++)
			{
				intBuffer.put(0, sectorMapping[i]);
				mappingFileWriter.Write(buffer, 0, buffer.Length);
			}
			mappingFileWriter.Close();

			sectorMappingDirty = false;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected int mapSector(int sectorNumber) throws java.io.IOException
		protected internal virtual int mapSector(int sectorNumber)
		{
			if (sectorNumber >= 0 && sectorNumber < NumSectors)
			{
				return sectorMapping[sectorNumber];
			}

			return freeSectorNumber;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected int getFreeSectorNumber() throws java.io.IOException
		protected internal virtual int FreeSectorNumber
		{
			get
			{
				int numSectors = NumSectors;
				for (int i = 0; i < numSectors; i++)
				{
					if (sectorMapping[i] == freeSectorNumber)
					{
						return i;
					}
				}
    
				return freeSectorNumber;
			}
		}

		protected internal virtual void setSectorMapping(int sectorNumber, int mappedSectorNumber)
		{
			sectorMapping[sectorNumber] = mappedSectorNumber;
			sectorMappingDirty = true;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void readSector(int sectorNumber, byte[] buffer, int offset) throws java.io.IOException
		public virtual void readSector(int sectorNumber, sbyte[] buffer, int offset)
		{
			int mappedSectorNumber = mapSector(sectorNumber);
			if (mappedSectorNumber >= 0)
			{
				sectorDevice.readSector(mappedSectorNumber, buffer, offset);
			}
			else
			{
				Arrays.Fill(buffer, offset, offset + ISectorDevice_Fields.sectorLength, (sbyte) 0);
			}
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
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public virtual void close()
		{
			// Write back the mapping file if it has been changed
			if (sectorMappingDirty)
			{
				writeMappingFile();
			}

			sectorDevice.close();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeSector(int sectorNumber, byte[] buffer, int offset) throws java.io.IOException
		public virtual void writeSector(int sectorNumber, sbyte[] buffer, int offset)
		{
			int freeSectorNumber = FreeSectorNumber;
			if (freeSectorNumber < 0)
			{
				throw new IOException(string.Format("Sector Device '{0}' is full", mappingFile));
			}

			sectorDevice.writeSector(freeSectorNumber, buffer, offset);
			setSectorMapping(freeSectorNumber, sectorNumber);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeSectors(int sectorNumber, int numberSectors, byte[] buffer, int offset) throws java.io.IOException
		public virtual void writeSectors(int sectorNumber, int numberSectors, sbyte[] buffer, int offset)
		{
			for (int i = 0; i < numberSectors; i++)
			{
				writeSector(sectorNumber + i, buffer, offset + i * ISectorDevice_Fields.sectorLength);
			}
		}
	}

}