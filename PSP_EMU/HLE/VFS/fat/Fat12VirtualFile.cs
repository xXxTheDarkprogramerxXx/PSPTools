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
//	import static pspsharp.HLE.VFS.fat.FatBuilder.numberOfFats;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatBuilder.reservedSectors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.readSectorInt8;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorInt16;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorInt32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorInt8;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.alignDown;

	public class Fat12VirtualFile : FatVirtualFile
	{
		private const int numberOfRootDirectoryEntries = 0x200;

		public Fat12VirtualFile(string deviceName, IVirtualFileSystem vfs, int totalSectors) : base(deviceName, vfs, totalSectors)
		{
			// FAT12 has no FS Info sector
			FsInfoSectorNumber = -1;

			// The FAT is directly after the boot sector, no reserved sectors present
	//		setFatSectorNumber(FatBuilder.bootSectorNumber + 1);
		}

		protected internal override int ClusterMask
		{
			get
			{
				return 0x00000FFF;
			}
		}

		protected internal override int FatEOC
		{
			get
			{
				return 0xFF8; // Last cluster in file (EOC)
			}
		}

		protected internal override string OEMName
		{
			get
			{
				return "6600-FAT";
			}
		}

		protected internal override int SectorsPerCluster
		{
			get
			{
				return 32;
			}
		}

		protected internal override int getFatSectors(int totalSectors, int sectorsPerCluster)
		{
			return 0x20;
		}

		protected internal override void readBIOSParameterBlock()
		{
			// Bytes per sector
			storeSectorInt16(currentSector, 11, sectorSize);

			// Sectors per cluster
			storeSectorInt8(currentSector, 13, SectorsPerCluster);

			// Reserved sectors
			storeSectorInt16(currentSector, 14, reservedSectors);

			// Number of File Allocation Tables (FATs)
			storeSectorInt8(currentSector, 16, numberOfFats);

			// Max entries in root dir
			storeSectorInt16(currentSector, 17, numberOfRootDirectoryEntries);

			// Total sectors
			storeSectorInt16(currentSector, 19, totalSectors);

			// Media type
			storeSectorInt8(currentSector, 21, 0xF8); // Fixed disk

			// Count of sectors used by the FAT table
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int fatSectors = getFatSectors(totalSectors, getSectorsPerCluster());
			int fatSectors = getFatSectors(totalSectors, SectorsPerCluster);
			storeSectorInt16(currentSector, 22, fatSectors);

			// Sectors per track (default)
			storeSectorInt16(currentSector, 24, 0x20);

			// Number of heads (default)
			storeSectorInt16(currentSector, 26, 0x40);

			// Count of hidden sectors
			storeSectorInt32(currentSector, 28, 0);

			// Total sectors
			storeSectorInt32(currentSector, 32, 4);

			// Physical driver number (0x80 for first fixed disk)
			storeSectorInt8(currentSector, 36, 0x80);

			// Reserved
			storeSectorInt8(currentSector, 37, 0);

			// Extended boot signature
			storeSectorInt8(currentSector, 38, 0x29);

			// Volume ID
			storeSectorInt32(currentSector, 39, 0x06060002);

			// Partition Volume Label
			storeSectorString(currentSector, 43, "NO NAME", 11);

			// File system type
			storeSectorString(currentSector, 54, "FAT12", 8);
		}

		private void storeFatByte(int offset, int value)
		{
			if (offset >= 0 && offset < sectorSize)
			{
				storeSectorInt8(currentSector, offset, value & 0xFF);
			}
		}

		protected internal override void readFatSector(int fatIndex)
		{
			readEmptySector();

			int offset = (fatIndex * sectorSize) / 3 * 2;
			int startIndex = (offset / 2 * 3) - (fatIndex * sectorSize);
			for (int i = startIndex, j = 0; i < sectorSize; j += 2)
			{
				int value = 0;
				if (offset + j < fatClusterMap.Length)
				{
					value = fatClusterMap[offset + j];
					if (offset + j + 1 < fatClusterMap.Length)
					{
						value |= fatClusterMap[offset + j + 1] << 12;
					}
				}

				// Store 3 bytes representing two FAT12 entries
				storeFatByte(i, value);
				i++;
				storeFatByte(i, value >> 8);
				i++;
				storeFatByte(i, value >> 16);
				i++;
			}
		}

		private int readFatEntry0(int offset)
		{
			return readSectorInt8(currentSector, offset) | ((readSectorInt8(currentSector, offset + 1) & 0x0F) << 8);
		}

		private int readFatEntry1(int offset)
		{
			return (readSectorInt8(currentSector, offset + 1) >> 4) | (readSectorInt8(currentSector, offset + 2) << 4);
		}

		protected internal override void writeFatSector(int fatIndex)
		{
			// TODO Implement the change of the FAT cluster number overlapping 2 sectors
			int index = alignDown(fatIndex * sectorSize * 2 / 3, 1);
			int offset = (index / 2 * 3) - (fatIndex * sectorSize);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Fat12VirtualFile.writeFatSector fatIndex=0x{0:X}, index=0x{1:X}, offset=0x{2:X}", fatIndex, index, offset));
			}

			while (offset < sectorSize && index < fatClusterMap.Length)
			{
				if (offset >= 0 && offset + 1 < sectorSize && index < fatClusterMap.Length)
				{
					int fatEntry = readFatEntry0(offset);
					if (fatEntry != fatClusterMap[index])
					{
						writeFatSectorEntry(index, fatEntry);
					}
				}
				index++;

				if (offset >= -1 && offset + 2 < sectorSize && index < fatClusterMap.Length)
				{
					int fatEntry = readFatEntry1(offset);
					if (fatEntry != fatClusterMap[index])
					{
						writeFatSectorEntry(index, fatEntry);
					}
				}
				index++;

				offset += 3;
			}
		}

		protected internal override int FirstDataClusterOffset
		{
			get
			{
				// The first data cluster is starting after the root directory
				return (numberOfRootDirectoryEntries << 5) / sectorSize;
			}
		}

		protected internal override FatFileInfo RootDirectory
		{
			set
			{
				rootDirectoryStartSectorNumber = FatSectorNumber + numberOfFats * fatSectors;
				rootDirectoryEndSectorNumber = rootDirectoryStartSectorNumber + FirstDataClusterOffset - 1;
				this.rootDirectory = value;
			}
		}
	}

}