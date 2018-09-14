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

	/// <summary>
	/// Interface defining a sector-oriented device.
	/// Only sector-based operations can be performed on this device.
	/// A sector is 2048 bytes long.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public interface ISectorDevice
	{
		/// <summary>
		/// The size in bytes of a sector.
		/// </summary>

		/// <returns> the total number of sectors for this device </returns>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getNumSectors() throws java.io.IOException;
		int NumSectors {get;}

		/// <summary>
		/// Read one sector of the device.
		/// 2048 bytes will be set in the buffer: buffer[offset..offset+2048-1].
		/// </summary>
		/// <param name="sectorNumber">  the sector number to be read. Must be in range 0 to getNumSectors() - 1. </param>
		/// <param name="buffer">        the buffer where to store the read bytes. </param>
		/// <param name="offset">        the offset inside the buffer where to start storing the read bytes. </param>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readSector(int sectorNumber, byte[] buffer, int offset) throws java.io.IOException;
		void readSector(int sectorNumber, sbyte[] buffer, int offset);

		/// <summary>
		/// Read multiple sectors of the device.
		/// 2048 bytes will be set in the buffer for each sector: buffer[offset..offset+numberSectors*2048-1].
		/// </summary>
		/// <param name="sectorNumber">  the sector number of the first sector to be read. Must be in range 0 to getNumSectors() - 1. </param>
		/// <param name="numberSectors"> the number of sectors to be read. Must be in range 0 to getNumSectors() - sectorNumber. </param>
		/// <param name="buffer">        the buffer where to store the read bytes. </param>
		/// <param name="offset">        the offset inside the buffer where to start storing the read bytes.
		/// @return </param>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readSectors(int sectorNumber, int numberSectors, byte[] buffer, int offset) throws java.io.IOException;
		int readSectors(int sectorNumber, int numberSectors, sbyte[] buffer, int offset);

		/// <summary>
		/// Write one sector of the device.
		/// 2048 bytes of the buffer will be written: buffer[offset..offset+2048-1].
		/// Not all the devices support this operation. If the operation is not supported by the device,
		/// an IOException will be raised.
		/// </summary>
		/// <param name="sectorNumber">  the sector number to be written. Must be in range 0 to getNumSectors() - 1. </param>
		/// <param name="buffer">        the buffer storing the bytes to be written. </param>
		/// <param name="offset">        the offset inside the buffer where the bytes are stored. </param>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeSector(int sectorNumber, byte[] buffer, int offset) throws java.io.IOException;
		void writeSector(int sectorNumber, sbyte[] buffer, int offset);

		/// <summary>
		/// Write multiple sectors of the device.
		/// 2048 bytes of the buffer will be written for each sector: buffer[offset..offset+numberSectors*2048-1].
		/// Not all the devices support this operation. If the operation is not supported by the device,
		/// an IOException will be raised.
		/// </summary>
		/// <param name="sectorNumber">  the sector number of the first sector to be written. Must be in range 0 to getNumSectors() - 1. </param>
		/// <param name="numberSectors"> the number of sectors to be written. Must be in range 0 to getNumSectors() - sectorNumber. </param>
		/// <param name="buffer">        the buffer storing the bytes to be written. </param>
		/// <param name="offset">        the offset inside the buffer where the bytes are stored. </param>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeSectors(int sectorNumber, int numberSectors, byte[] buffer, int offset) throws java.io.IOException;
		void writeSectors(int sectorNumber, int numberSectors, sbyte[] buffer, int offset);

		/// <summary>
		/// Close any associated resource.
		/// After a close, the device cannot be used any longer.
		/// </summary>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException;
		void close();
	}

	public static class ISectorDevice_Fields
	{
		public const int sectorLength = 2048;
	}

}