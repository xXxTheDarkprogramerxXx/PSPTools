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
//	import static pspsharp.HLE.VFS.fat.FatUtils.readSectorInt32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorInt16;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorInt32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorInt8;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorString;

	using MemoryStick = pspsharp.hardware.MemoryStick;

	public class Fat32VirtualFile : FatVirtualFile
	{
		private readonly int[] rootDirectoryClusters = new int[1];

		public Fat32VirtualFile(string deviceName, IVirtualFileSystem vfs) : base(deviceName, vfs, (int)(MemoryStick.TotalSize / sectorSize))
		{
		}

		protected internal override int ClusterMask
		{
			get
			{
				return 0x0FFFFFFF;
			}
		}

		protected internal override int FatEOC
		{
			get
			{
				return 0x0FFFFFFF; // Last cluster in file (EOC)
			}
		}

		protected internal override int SectorsPerCluster
		{
			get
			{
				return 64;
			}
		}

		protected internal override int getFatSectors(int totalSectors, int sectorsPerCluster)
		{
			int totalClusters = (totalSectors / sectorsPerCluster) + 1;
			int fatSectors = (totalClusters / (sectorSize / 4)) + 1;

			return fatSectors;
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

			// Max entries in root dir (0 for FAT32)
			storeSectorInt16(currentSector, 17, 0);

			// Total sectors (use FAT32 count instead)
			storeSectorInt16(currentSector, 19, 0);

			// Media type
			storeSectorInt8(currentSector, 21, 0xF8); // Fixed disk

			// Count of sectors used by the FAT table (0 for FAT32)
			storeSectorInt16(currentSector, 22, 0);

			// Sectors per track (default)
			storeSectorInt16(currentSector, 24, 0x3F);

			// Number of heads (default)
			storeSectorInt16(currentSector, 26, 0xFF);

			// Count of hidden sectors
			storeSectorInt32(currentSector, 28, 0);

			// Total sectors
			storeSectorInt32(currentSector, 32, totalSectors);

			// Sectors per FAT
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int fatSectors = getFatSectors(totalSectors, getSectorsPerCluster());
			int fatSectors = getFatSectors(totalSectors, SectorsPerCluster);
			storeSectorInt32(currentSector, 36, fatSectors);

			// Drive description / mirroring flags
			storeSectorInt16(currentSector, 40, 0);

			// Version
			storeSectorInt16(currentSector, 42, 0);

			// Cluster number of root directory start
			const int rootDirFirstCluster = 2;
			storeSectorInt32(currentSector, 44, rootDirFirstCluster);

			// Sector number of FS Information Sector
			storeSectorInt16(currentSector, 48, FsInfoSectorNumber);

			// First sector number of a copy of the three FAT32 boot sectors, typically 6.
			storeSectorInt16(currentSector, 50, 6);

			// Drive number
			storeSectorInt8(currentSector, 64, 0);

			// Extended boot signature
			storeSectorInt8(currentSector, 66, 0x29);

			// Volume ID
			storeSectorInt32(currentSector, 67, 0x00000000);

			// Volume label
			storeSectorString(currentSector, 71, "", 11);

			// File system type
			storeSectorString(currentSector, 82, "FAT32", 8);
		}

		protected internal override void readFatSector(int fatIndex)
		{
			readEmptySector();

			int offset = (fatIndex * sectorSize) >> 2;
			int maxSize = System.Math.Min(sectorSize, (fatClusterMap.Length - offset) << 2);
			for (int i = 0, j = 0; i < maxSize; i += 4, j++)
			{
				storeSectorInt32(currentSector, i, fatClusterMap[offset + j]);
			}
		}

		protected internal override void writeFatSector(int fatIndex)
		{
			int offset = (fatIndex * sectorSize) >> 2;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Fat32VirtualFile.writeFatSector fatIndex=0x{0:X}, offset=0x{1:X}", fatIndex, offset));
			}

			for (int i = 0, j = 0; i < sectorSize; i += 4, j++)
			{
				int fatEntry = readSectorInt32(currentSector, i);
				if (fatEntry != fatClusterMap[offset + j])
				{
					writeFatSectorEntry(offset + j, fatEntry);
				}
			}
		}

		protected internal override int FirstDataClusterOffset
		{
			get
			{
				// The first data cluster is starting at the root directory
				return 0;
			}
		}

		protected internal override int FirstFreeCluster
		{
			get
			{
				// Allocate the first cluster(s) for the root directory
				int clusterNumber = base.FirstFreeCluster;
				for (int i = 0; i < rootDirectoryClusters.Length; i++)
				{
					rootDirectoryClusters[i] = clusterNumber++;
				}
    
				return clusterNumber;
			}
		}

		protected internal override FatFileInfo RootDirectory
		{
			set
			{
				for (int i = 0; i < rootDirectoryClusters.Length; i++)
				{
					setFatFileInfoMap(rootDirectoryClusters[i], value);
					int nextCluster = i < rootDirectoryClusters.Length - 1 ? rootDirectoryClusters[i + 1] : FatEOC;
					setFatClusterMap(rootDirectoryClusters[i], nextCluster);
				}
				value.Clusters = rootDirectoryClusters;
			}
		}
	}

}