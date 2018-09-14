using System;
using System.Collections.Generic;

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
//	import static pspsharp.HLE.VFS.AbstractVirtualFileSystem.IO_ERROR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatBuilder.bootSectorNumber;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatBuilder.directoryTableEntrySize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatBuilder.numberOfFats;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatBuilder.reservedSectors;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.getSectorNumber;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.getSectorOffset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.readSectorInt16;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.readSectorInt32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.readSectorString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorInt32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorInt8;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.pspAbstractMemoryMappedStructure.charset16;


	using Logger = org.apache.log4j.Logger;

	using ScePspDateTime = pspsharp.HLE.kernel.types.ScePspDateTime;
	using IoFileMgrForUser = pspsharp.HLE.modules.IoFileMgrForUser;
	using IoOperation = pspsharp.HLE.modules.IoFileMgrForUser.IoOperation;
	using IoOperationTiming = pspsharp.HLE.modules.IoFileMgrForUser.IoOperationTiming;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;
	using Utilities = pspsharp.util.Utilities;

	// See format description: https://en.wikipedia.org/wiki/Design_of_the_FAT_file_system
	public abstract class FatVirtualFile : IVirtualFile
	{
		public static Logger log = Logger.getLogger("fat");
		private const int STATE_VERSION = 0;
		public const int sectorSize = 512;
		protected internal const int firstClusterNumber = 2;
		protected internal readonly sbyte[] currentSector = new sbyte[sectorSize];
		private static readonly sbyte[] emptySector = new sbyte[sectorSize];
		private string deviceName;
		private IVirtualFileSystem vfs;
		private long position;
		protected internal int totalSectors;
		protected internal int fatSectors;
		protected internal int[] fatClusterMap;
		private FatFileInfo[] fatFileInfoMap;
		private sbyte[] pendingCreateDirectoryEntryLFN;
		private readonly IDictionary<int, sbyte[]> pendingWriteSectors = new Dictionary<int, sbyte[]>();
		private readonly IDictionary<int, FatFileInfo> pendingDeleteFiles = new Dictionary<int, FatFileInfo>();
		private FatBuilder builder;
		private int fatSectorNumber = bootSectorNumber + reservedSectors;
		private int fsInfoSectorNumber = bootSectorNumber + 1;
		protected internal FatFileInfo rootDirectory;
		protected internal int rootDirectoryStartSectorNumber = -1;
		protected internal int rootDirectoryEndSectorNumber = -1;
		private readonly sbyte[] secondFat;

		protected internal FatVirtualFile(string deviceName, IVirtualFileSystem vfs, int totalSectors)
		{
			this.deviceName = deviceName;
			this.vfs = vfs;

			this.totalSectors = totalSectors;
			fatSectors = getFatSectors(totalSectors, SectorsPerCluster);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("totalSectors=0x{0:X}, fatSectors=0x{1:X}", totalSectors, fatSectors));
			}

			int usedSectors = reservedSectors + fatSectors * numberOfFats;
			usedSectors += FirstDataClusterOffset;
			int maxNumberClusters = (totalSectors - usedSectors) / SectorsPerCluster;
			// Allocate the FAT cluster map
			fatClusterMap = new int[maxNumberClusters];
			// First 2 special entries in the cluster map
			fatClusterMap[0] = unchecked((int)0xFFFFFFF8) & ClusterMask; // 0xF8 is matching the boot sector Media type field
			fatClusterMap[1] = unchecked((int)0xFFFFFFFF) & ClusterMask;

			// Allocate the FAT file info map
			fatFileInfoMap = new FatFileInfo[maxNumberClusters];

			secondFat = new sbyte[fatSectors * sectorSize];

			builder = new FatBuilder(this, vfs, maxNumberClusters);
		}

		protected internal abstract int ClusterMask {get;}
		protected internal abstract int SectorsPerCluster {get;}
		protected internal abstract int FatEOC {get;}
		protected internal abstract int getFatSectors(int totalSectors, int sectorsPerCluster);
		protected internal abstract void readBIOSParameterBlock();
		protected internal abstract int FirstDataClusterOffset {get;}
		protected internal abstract FatFileInfo RootDirectory {set;}

		protected internal virtual int FirstFreeCluster
		{
			get
			{
				return firstClusterNumber + FirstDataClusterOffset / SectorsPerCluster;
			}
		}

		protected internal virtual int ClusterSize
		{
			get
			{
				return sectorSize * SectorsPerCluster;
			}
		}

		public virtual int FsInfoSectorNumber
		{
			get
			{
				return fsInfoSectorNumber;
			}
			set
			{
				this.fsInfoSectorNumber = value;
			}
		}


		public virtual int FatSectorNumber
		{
			get
			{
				return fatSectorNumber;
			}
			set
			{
				this.fatSectorNumber = value;
			}
		}


		public virtual void scan()
		{
			builder.scan();
		}

		private void extendClusterMap(int clusterNumber)
		{
			int extend = clusterNumber + 1 - fatClusterMap.Length;
			if (extend > 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("extendClusterMap clusterNumber=0x{0:X}, extend=0x{1:X}", clusterNumber, extend));
				}
				fatClusterMap = Utilities.extendArray(fatClusterMap, extend);
				fatFileInfoMap = FatUtils.extendArray(fatFileInfoMap, extend);
			}
		}

		public virtual void setFatFileInfoMap(int clusterNumber, FatFileInfo fileInfo)
		{
			if (clusterNumber >= fatFileInfoMap.Length)
			{
				extendClusterMap(clusterNumber);
			}
			fatFileInfoMap[clusterNumber] = fileInfo;
		}

		public virtual void setFatClusterMap(int clusterNumber, int value)
		{
			if (clusterNumber >= fatClusterMap.Length)
			{
				extendClusterMap(clusterNumber);
			}
			fatClusterMap[clusterNumber] = value;
		}

		private int getClusterNumber(int sectorNumber)
		{
			sectorNumber -= fatSectorNumber;
			sectorNumber -= numberOfFats * fatSectors;
			sectorNumber -= FirstDataClusterOffset;
			return firstClusterNumber + (sectorNumber / SectorsPerCluster);
		}

		private int getSectorNumberFromCluster(int clusterNumber)
		{
			int sectorNumber = (clusterNumber - firstClusterNumber) * SectorsPerCluster;
			sectorNumber += fatSectorNumber;
			sectorNumber += numberOfFats * fatSectors;
			sectorNumber += FirstDataClusterOffset;
			return sectorNumber;
		}

		private int getSectorOffsetInCluster(int sectorNumber)
		{
			sectorNumber -= fatSectorNumber;
			sectorNumber -= numberOfFats * fatSectors;
			sectorNumber -= FirstDataClusterOffset;
			return sectorNumber % SectorsPerCluster;
		}

		private bool isFreeClusterNumber(int clusterNumber)
		{
			clusterNumber &= ClusterMask;
			return clusterNumber == 0;
		}

		private bool isDataClusterNumber(int clusterNumber)
		{
			clusterNumber &= ClusterMask;
			return clusterNumber >= 2 && clusterNumber <= 0x0FFFFFEF;
		}

		protected internal virtual string OEMName
		{
			get
			{
				return "";
			}
		}

		private void readBootSector()
		{
			readEmptySector();

			// Jump Code
			storeSectorInt8(currentSector, 0, 0xEB);
			storeSectorInt8(currentSector, 1, 0x58);
			storeSectorInt8(currentSector, 2, 0x90);

			// OEM Name
			storeSectorString(currentSector, 3, OEMName, 8);

			// The format of BIOS Parameter Block is depending on the
			// fat format (FAT12, FAT16 or FAT32)
			readBIOSParameterBlock();

			// Signature
			storeSectorInt8(currentSector, 510, 0x55);
			storeSectorInt8(currentSector, 511, 0xAA);
		}

		private void readFsInfoSector()
		{
			readEmptySector();

			// FS Information sector signature
			storeSectorInt8(currentSector, 0, 0x52);
			storeSectorInt8(currentSector, 1, 0x52);
			storeSectorInt8(currentSector, 2, 0x61);
			storeSectorInt8(currentSector, 3, 0x41);

			// FS Information sector signature
			storeSectorInt8(currentSector, 484, 0x72);
			storeSectorInt8(currentSector, 485, 0x72);
			storeSectorInt8(currentSector, 486, 0x41);
			storeSectorInt8(currentSector, 487, 0x61);

			// Last known number of free data clusters, or 0xFFFFFFFF if unknown
			storeSectorInt32(currentSector, 488, 0xFFFFFFFF);

			// Number of the most recently known to be allocated data cluster.
			// Should be set to 0xFFFFFFFF during format.
			storeSectorInt32(currentSector, 492, 0xFFFFFFFF);

			// FS Information sector signature
			storeSectorInt8(currentSector, 510, 0x55);
			storeSectorInt8(currentSector, 511, 0xAA);
		}

		protected internal abstract void readFatSector(int fatIndex);

		private void readDataSector(int sectorNumber, int clusterNumber, int sectorOffsetInCluster, FatFileInfo fileInfo)
		{
			readEmptySector();

			if (fileInfo == null)
			{
				log.warn(string.Format("readDataSector unknown sectorNumber=0x{0:X}, clusterNumber=0x{1:X}", sectorNumber, clusterNumber));
				return;
			}
			if (log.DebugEnabled)
			{
				log.debug(string.Format("readDataSector clusterNumber=0x{0:X}(sector=0x{1:X}), fileInfo={2}", clusterNumber, sectorOffsetInCluster, fileInfo));
			}

			if (fileInfo.Directory)
			{
				sbyte[] directoryData = fileInfo.FileData;
				if (directoryData == null)
				{
					directoryData = builder.buildDirectoryData(fileInfo);
					fileInfo.FileData = directoryData;
				}

				int byteOffset = sectorOffsetInCluster * sectorSize;
				if (byteOffset < directoryData.Length)
				{
					int length = System.Math.Min(directoryData.Length - byteOffset, sectorSize);
					Array.Copy(directoryData, byteOffset, currentSector, 0, length);
				}
			}
			else
			{
				IVirtualFile vFile = fileInfo.getVirtualFile(vfs);
				if (vFile == null)
				{
					log.warn(string.Format("readDataSector cannot read file '{0}'", fileInfo));
					return;
				}

				long byteOffset = sectorOffsetInCluster * (long) sectorSize;
				int[] clusters = fileInfo.Clusters;
				if (clusters != null)
				{
					for (int i = 0; i < clusters.Length; i++)
					{
						if (clusters[i] == clusterNumber)
						{
							break;
						}
						byteOffset += SectorsPerCluster * sectorSize;
					}
				}

				if (byteOffset < fileInfo.FileSize)
				{
					if (vFile.ioLseek(byteOffset) != byteOffset)
					{
						log.warn(string.Format("readDataSector cannot seek file '{0}' to 0x{1:X}", fileInfo, byteOffset));
						return;
					}

					int length = (int) System.Math.Min(fileInfo.FileSize - byteOffset, (long) sectorSize);
					int readLength = vFile.ioRead(currentSector, 0, length);
					if (log.DebugEnabled)
					{
						log.debug(string.Format("readDataSector readLength=0x{0:X}", readLength));
					}
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("readDataSector trying to read at offset 0x{0:X} past end of file", byteOffset, fileInfo));
					}
				}
			}
		}

		private void readRootDirectory(int sectorNumber)
		{
			readDataSector(rootDirectoryStartSectorNumber, 0, sectorNumber, rootDirectory);
		}

		private void readDataSector(int sectorNumber)
		{
			readEmptySector();

			int clusterNumber = getClusterNumber(sectorNumber);
			if (clusterNumber >= fatFileInfoMap.Length)
			{
				// Reading out of the allocated fat files
				return;
			}
			FatFileInfo fileInfo = fatFileInfoMap[clusterNumber];
			int sectorOffsetInCluster = getSectorOffsetInCluster(sectorNumber);

			readDataSector(sectorNumber, clusterNumber, sectorOffsetInCluster, fileInfo);
		}

		private void readSecondFatSector(int fatIndex)
		{
			Array.Copy(secondFat, fatIndex * sectorSize, currentSector, 0, sectorSize);
		}

		protected internal virtual void readEmptySector()
		{
			Array.Copy(emptySector, 0, currentSector, 0, sectorSize);
		}

		private void readSector(int sectorNumber)
		{
			sbyte[] pendingWriteSector = pendingWriteSectors[sectorNumber];
			if (pendingWriteSector != null)
			{
				Array.Copy(pendingWriteSector, 0, currentSector, 0, sectorSize);
				return;
			}

			if (sectorNumber == bootSectorNumber)
			{
				readBootSector();
			}
			else if (sectorNumber == fsInfoSectorNumber)
			{
				readFsInfoSector();
			}
			else if (sectorNumber < fatSectorNumber)
			{
				readEmptySector();
			}
			else if (sectorNumber >= fatSectorNumber && sectorNumber < fatSectorNumber + fatSectors)
			{
				readFatSector(sectorNumber - fatSectorNumber);
			}
			else if (sectorNumber >= fatSectorNumber + fatSectors && sectorNumber < fatSectorNumber + numberOfFats * fatSectors)
			{
				// Reading from the second FAT table
				readSecondFatSector(sectorNumber - fatSectorNumber - fatSectors);
			}
			else if (sectorNumber >= rootDirectoryStartSectorNumber && sectorNumber <= rootDirectoryEndSectorNumber)
			{
				readRootDirectory(sectorNumber - rootDirectoryStartSectorNumber);
			}
			else
			{
				readDataSector(sectorNumber);
			}
		}

		private void writeBootSector()
		{
			log.warn(string.Format("Writing to the MemoryStick boot sector!"));
			writeEmptySector();
		}

		private void writeFsInfoSector()
		{
			log.warn(string.Format("Writing to the MemoryStick FsInfo sector!"));
			writeEmptySector();
		}

		protected internal virtual void writeFatSectorEntry(int clusterNumber, int value)
		{
			// One entry of the FAT has been updated
			if (log.DebugEnabled)
			{
				log.debug(string.Format("writeFatSectorEntry[0x{0:X}]=0x{1:X8}", clusterNumber, value));
			}

			fatClusterMap[clusterNumber] = value;

			FatFileInfo fileInfo = fatFileInfoMap[clusterNumber];
			if (fileInfo != null)
			{
				// Freeing the cluster?
				if (isFreeClusterNumber(value))
				{
					if (pendingDeleteFiles[clusterNumber] == fileInfo)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Deleting the file '{0}'", fileInfo.FullFileName));
						}

						// Close the file before deleting it
						closeTree(fileInfo);

						int result = vfs.ioRemove(fileInfo.FullFileName);
						if (result < 0)
						{
							log.warn(string.Format("Cannot delete the file '{0}'", fileInfo.FullFileName));
						}
						pendingDeleteFiles.Remove(clusterNumber);
					}
				}
				else
				{
					// Setting a new data cluster number?
					if (isDataClusterNumber(value))
					{
						int newClusterNumber = value & ClusterMask;
						if (!fileInfo.hasCluster(newClusterNumber))
						{
							fileInfo.addCluster(newClusterNumber);
							fatFileInfoMap[newClusterNumber] = fileInfo;
						}
					}
					checkPendingWriteSectors(fileInfo);
				}
			}
		}

		protected internal abstract void writeFatSector(int fatIndex);

		private void deleteDirectoryEntry(FatFileInfo fileInfo, sbyte[] directoryData, int offset)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("deleteDirectoryEntry on {0}: {1}", fileInfo, Utilities.getMemoryDump(directoryData, offset, directoryTableEntrySize)));
			}

			if (!isLongFileNameDirectoryEntry(directoryData, offset))
			{
				string fileName83 = Utilities.readStringNZ(directoryData, offset + 0, 8 + 3);

				FatFileInfo childFileInfo = fileInfo.getChildByFileName83(fileName83);
				if (childFileInfo == null)
				{
					log.warn(string.Format("deleteDirectoryEntry cannot find child entry '{0}' in {1}", fileName83, fileInfo));
				}
				else
				{
					pendingDeleteFiles[childFileInfo.FirstCluster] = childFileInfo;
				}
			}
		}

		private void deleteDirectoryEntries(FatFileInfo fileInfo, sbyte[] directoryData, int offset, int length)
		{
			if (directoryData == null || length <= 0 || offset >= directoryData.Length)
			{
				return;
			}

			for (int i = 0; i < length; i += directoryTableEntrySize)
			{
				deleteDirectoryEntry(fileInfo, directoryData, offset + i);
			}
		}

		private string getFileNameLFN(sbyte[] lfn)
		{
			bool last = false;
			sbyte[] fileNameBytes = null;
			for (int sequenceNumber = 1; !last; sequenceNumber++)
			{
				for (int i = 0; i < lfn.Length; i += directoryTableEntrySize)
				{
					if ((lfn[i + 0] & 0x1F) == sequenceNumber)
					{
						if ((lfn[i + 0] & 0x40) != 0)
						{
							last = true;
						}
						fileNameBytes = Utilities.extendArray(fileNameBytes, lfn, i + 1, 10);
						fileNameBytes = Utilities.extendArray(fileNameBytes, lfn, i + 14, 12);
						fileNameBytes = Utilities.extendArray(fileNameBytes, lfn, i + 28, 4);
						break;
					}
				}
			}

			if (fileNameBytes == null)
			{
				return "";
			}

			for (int i = 0; i < fileNameBytes.Length; i += 2)
			{
				if (fileNameBytes[i] == ((sbyte) 0) && fileNameBytes[i + 1] == (sbyte) 0)
				{
					return StringHelper.NewString(fileNameBytes, 0, i, charset16);
				}
			}

			return StringHelper.NewString(fileNameBytes, charset16);
		}

		private string getFileName(sbyte[] sector, int offset, sbyte[] lfn)
		{
			string fileName;

			if (lfn != null)
			{
				fileName = getFileNameLFN(lfn);
			}
			else
			{
				string name = readSectorString(sector, offset + 0, 8);
				string ext = readSectorString(sector, offset + 8, 3);
				if (ext.Length == 0)
				{
					fileName = name;
				}
				else
				{
					fileName = name + '.' + ext;
				}
			}

			return fileName;
		}

		private void closeTree(FatFileInfo fileInfo)
		{
			if (fileInfo != null)
			{
				fileInfo.closeVirtualFile();

				IList<FatFileInfo> children = fileInfo.Children;
				if (children != null)
				{
					foreach (FatFileInfo child in children)
					{
						closeTree(child);
					}
				}
			}
		}

		private int getMode(bool directory, bool readOnly)
		{
			// Always readable
			int mode = 0x124; // Readable

			// Only directories are executable
			if (directory)
			{
				mode |= 0x49; // Executable
			}

			// Writable when not read-only
			if (!readOnly)
			{
				mode |= 0x92; // Writable
			}

			return mode;
		}

		private void createDirectoryEntry(FatFileInfo fileInfo, sbyte[] sector, int offset)
		{
			if (offset >= sector.Length)
			{
				return;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("createDirectoryEntry on {0}: {1}", fileInfo, Utilities.getMemoryDump(sector, offset, directoryTableEntrySize)));
			}

			if (isLongFileNameDirectoryEntry(sector, offset))
			{
				pendingCreateDirectoryEntryLFN = Utilities.extendArray(pendingCreateDirectoryEntryLFN, sector, offset, directoryTableEntrySize);
			}
			else
			{
				string fileName = getFileName(sector, offset, pendingCreateDirectoryEntryLFN);
				bool readOnly = (sector[offset + 11] & 0x01) != 0;
				bool directory = (sector[offset + 11] & 0x10) != 0;
				int clusterNumber = readSectorInt16(sector, offset + 20) << 16;
				clusterNumber |= readSectorInt16(sector, offset + 26);
				long fileSize = readSectorInt32(sector, offset + 28) & 0xFFFFFFFFL;
				int time = readSectorInt16(sector, offset + 22);
				time |= readSectorInt16(sector, offset + 24) << 16;
				ScePspDateTime lastModified = ScePspDateTime.fromMSDOSTime(time);
				if (log.DebugEnabled)
				{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("createDirectoryEntry fileName='%s', readOnly=%b, directory=%b, clusterNumber=0x%X, fileSize=0x%X", fileName, readOnly, directory, clusterNumber, fileSize));
					log.debug(string.Format("createDirectoryEntry fileName='%s', readOnly=%b, directory=%b, clusterNumber=0x%X, fileSize=0x%X", fileName, readOnly, directory, clusterNumber, fileSize));
				}

				FatFileInfo pendingDeleteFile = pendingDeleteFiles.Remove(clusterNumber);
				if (pendingDeleteFile != null)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Renaming directory entry {0} into '{1}'", pendingDeleteFile, fileName));
					}
					if (readOnly != pendingDeleteFile.ReadOnly)
					{
						log.warn(string.Format("Cannot change read-only attribute of {0}", pendingDeleteFile));
					}
					if (directory != pendingDeleteFile.Directory)
					{
						log.warn(string.Format("Cannot change directory attribute of {0}", pendingDeleteFile));
					}
					if (fileSize != pendingDeleteFile.FileSize)
					{
						log.warn(string.Format("Cannot change file size of {0}", pendingDeleteFile));
					}
					string oldFullFileName = pendingDeleteFile.FullFileName;
					pendingDeleteFile.FileName = fileName;
					string newFullFileName = pendingDeleteFile.FullFileName;

					// Close all the files in the directory before renaming it
					closeTree(pendingDeleteFile);

					int result = vfs.ioRename(oldFullFileName, newFullFileName);
					if (result < 0)
					{
						log.warn(string.Format("Cannot rename file '{0}' into '{1}'", oldFullFileName, newFullFileName));
					}
					pendingDeleteFile.Directory = directory;
					pendingDeleteFile.ReadOnly = readOnly;
					pendingDeleteFile.FileSize = fileSize;
					pendingDeleteFile.LastModified = lastModified;
				}
				else
				{
					FatFileInfo newFileInfo = new FatFileInfo(deviceName, fileInfo.FullFileName, fileName, directory, readOnly, lastModified, fileSize);
					newFileInfo.FileName83 = Utilities.readStringNZ(sector, offset + 0, 8 + 3);
					if (clusterNumber != 0)
					{
						int[] clusters = getClusters(clusterNumber);
						builder.setClusters(newFileInfo, clusters);
					}

					fileInfo.addChild(newFileInfo);

					if (directory)
					{
						vfs.ioMkdir(newFileInfo.FullFileName, getMode(directory, readOnly));
					}
				}

				pendingCreateDirectoryEntryLFN = null;
			}
		}

		private void checkPendingWriteSectors(FatFileInfo fileInfo)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("checkPendingWriteSectors for {0}", fileInfo));
			}

			if (pendingWriteSectors.Count == 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("checkPendingWriteSectors - no pending sectors"));
				}
				return;
			}

			int[] clusters = fileInfo.Clusters;
			if (clusters == null)
			{
				return;
			}

			if (log.DebugEnabled)
			{
				foreach (int sectorNumber in pendingWriteSectors.Keys)
				{
					log.debug(string.Format("checkPendingWriteSectors pending sectorNumber=0x{0:X}", sectorNumber));
				}
			}

			for (int i = 0; i < clusters.Length; i++)
			{
				int clusterNumber = clusters[i];
				int sectorNumber = getSectorNumberFromCluster(clusterNumber);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("checkPendingWriteSectors checking for clusterNumber=0x{0:X}, sectorNumber=0x{1:X}-0x{2:X}", clusterNumber, sectorNumber, sectorNumber + SectorsPerCluster - 1));
				}
				for (int j = 0; j < SectorsPerCluster; j++, sectorNumber++)
				{
					sbyte[] pendingWriteSector = pendingWriteSectors.Remove(sectorNumber);
					if (pendingWriteSector != null)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("checkPendingWriteSectors writing pending sectorNumber=0x{0:X} for {1}", sectorNumber, fileInfo));
						}
						writeFileSector(fileInfo, sectorNumber, pendingWriteSector);
					}
				}
			}
		}

		private int[] getClusters(int clusterNumber)
		{
			int[] clusters = new int[] {clusterNumber};

			while (clusterNumber < fatClusterMap.Length)
			{
				int nextCluster = fatClusterMap[clusterNumber];
				if (!isDataClusterNumber(nextCluster))
				{
					break;
				}

				// Add the nextCluster to the clusters array
				clusters = Utilities.extendArray(clusters, 1);
				clusterNumber = nextCluster & ClusterMask;
				clusters[clusters.Length - 1] = clusterNumber;
			}

			return clusters;
		}

		private void updateDirectoryEntry(FatFileInfo fileInfo, sbyte[] directoryData, int directoryDataOffset, sbyte[] sector, int sectorOffset)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("updateDirectoryEntry on {0}: from {1}, to {2}", fileInfo, Utilities.getMemoryDump(directoryData, directoryDataOffset, directoryTableEntrySize), Utilities.getMemoryDump(sector, sectorOffset, directoryTableEntrySize)));
			}

			bool oldLFN = isLongFileNameDirectoryEntry(directoryData, directoryDataOffset);
			bool newLFN = isLongFileNameDirectoryEntry(sector, sectorOffset);
			if (oldLFN != newLFN)
			{
				log.error(string.Format("updateDirectoryEntry changing LongFileName entries not implemented: {0} from {1}, to {2}", fileInfo, Utilities.getMemoryDump(directoryData, directoryDataOffset, directoryTableEntrySize), Utilities.getMemoryDump(sector, sectorOffset, directoryTableEntrySize)));
			}
			else if (newLFN)
			{
				log.error(string.Format("updateDirectoryEntry updating LongFileName entries not implemented: {0} from {1}, to {2}", fileInfo, Utilities.getMemoryDump(directoryData, directoryDataOffset, directoryTableEntrySize), Utilities.getMemoryDump(sector, sectorOffset, directoryTableEntrySize)));
			}
			else
			{
				int oldClusterNumber = readSectorInt16(directoryData, directoryDataOffset + 20) << 16;
				oldClusterNumber |= readSectorInt16(directoryData, directoryDataOffset + 26);

				int newClusterNumber = readSectorInt16(sector, sectorOffset + 20) << 16;
				newClusterNumber |= readSectorInt16(sector, sectorOffset + 26);
				long newFileSize = readSectorInt32(sector, sectorOffset + 28) & 0xFFFFFFFFL;
				if (log.DebugEnabled)
				{
					log.debug(string.Format("updateDirectoryEntry oldClusterNumber=0x{0:X}, newClusterNumber=0x{1:X}, newFileSize=0x{2:X}", oldClusterNumber, newClusterNumber, newFileSize));
				}

				string oldFileName83 = Utilities.readStringNZ(directoryData, directoryDataOffset + 0, 8 + 3);
				string newFileName83 = Utilities.readStringNZ(sector, sectorOffset + 0, 8 + 3);

				if (!oldFileName83.Equals(newFileName83))
				{
					// TODO
					log.warn(string.Format("updateDirectoryEntry unimplemented change of 8.3. file name: from '{0}' to '{1}'", oldFileName83, newFileName83));
				}

				FatFileInfo childFileInfo = fileInfo.getChildByFileName83(oldFileName83);
				if (childFileInfo == null)
				{
					log.warn(string.Format("updateDirectoryEntry child '{0}' not found", oldFileName83));
				}
				else
				{
					// Update the file size.
					// Rem.: this must be done before calling checkPendingWriteSectors
					childFileInfo.FileSize = newFileSize;

					// Update the clusterNumber
					if (oldClusterNumber != newClusterNumber)
					{
						int[] clusters = getClusters(newClusterNumber);
						builder.setClusters(childFileInfo, clusters);

						// The clusters have been updated for this file,
						// check if there were pending sector writes in the new
						// clusters
						checkPendingWriteSectors(childFileInfo);
					}
				}
			}
		}

		private static bool isLongFileNameDirectoryEntry(sbyte[] directoryData, int offset)
		{
			// Attributes (always 0x0F for LFN)
			return directoryData[offset + 11] == 0x0F;
		}

		private void writeFileSector(FatFileInfo fileInfo, int sectorNumber, sbyte[] sector)
		{
			int clusterNumber = getClusterNumber(sectorNumber);
			int sectorOffsetInCluster = getSectorOffsetInCluster(sectorNumber);

			IVirtualFile vFile = fileInfo.getVirtualFile(vfs);
			if (vFile == null)
			{
				log.warn(string.Format("writeFileSector cannot write file '{0}'", fileInfo));
				return;
			}

			long byteOffset = sectorOffsetInCluster * (long) sectorSize;
			int[] clusters = fileInfo.Clusters;
			if (clusters != null)
			{
				for (int i = 0; i < clusters.Length; i++)
				{
					if (clusters[i] == clusterNumber)
					{
						break;
					}
					byteOffset += ClusterSize;
				}
			}

			if (byteOffset < fileInfo.FileSize)
			{
				if (vFile.ioLseek(byteOffset) != byteOffset)
				{
					log.warn(string.Format("writeFileSector cannot seek file '{0}' to 0x{1:X}", fileInfo, byteOffset));
					return;
				}

				int length = (int) System.Math.Min(fileInfo.FileSize - byteOffset, (long) sectorSize);
				int writeLength = vFile.ioWrite(sector, 0, length);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("writeFileSector writeLength=0x{0:X}", writeLength));
				}
			}
		}

		private void writeDataSector(int sectorNumber, int clusterNumber, int sectorOffsetInCluster, FatFileInfo fileInfo)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("writeDataSector clusterNumber=0x{0:X}(sector=0x{1:X}), fileInfo={2}", clusterNumber, sectorOffsetInCluster, fileInfo));
			}

			if (fileInfo.Directory)
			{
				sbyte[] directoryData = fileInfo.FileData;
				if (directoryData == null)
				{
					directoryData = builder.buildDirectoryData(fileInfo);
					fileInfo.FileData = directoryData;
				}

				int byteOffset = sectorOffsetInCluster * sectorSize;
				int sectorLength = sectorSize;
				for (int i = 0; i < sectorSize; i += directoryTableEntrySize)
				{
					// End of directory table?
					if (currentSector[i + 0] == (sbyte) 0)
					{
						// Delete the remaining directory entries of the current directory
						deleteDirectoryEntries(fileInfo, directoryData, byteOffset + i, directoryData.Length - (byteOffset + i));
						sectorLength = i;
						break;
					}

					if (currentSector[i + 0] == unchecked((sbyte) 0xE5))
					{
						// Deleted file
						if (byteOffset + i < directoryData.Length && directoryData[byteOffset + i + 0] != unchecked((sbyte) 0xE5))
						{
							deleteDirectoryEntry(fileInfo, directoryData, byteOffset + i);
						}
					}
					else if (byteOffset + i >= directoryData.Length)
					{
						createDirectoryEntry(fileInfo, currentSector, i);
					}
					else if (!Utilities.Equals(directoryData, byteOffset + i, currentSector, i, directoryTableEntrySize))
					{
						updateDirectoryEntry(fileInfo, directoryData, byteOffset + i, currentSector, i);
					}
				}

				directoryData = Utilities.copyToArrayAndExtend(directoryData, byteOffset, currentSector, 0, sectorLength);
				fileInfo.FileData = directoryData;
			}
			else
			{
				writeFileSector(fileInfo, sectorNumber, currentSector);
			}
		}

		private void writeRootDirectory(int sectorNumber)
		{
			writeDataSector(rootDirectoryStartSectorNumber, 0, sectorNumber, rootDirectory);
		}

		private void writeDataSector(int sectorNumber)
		{
			int clusterNumber = getClusterNumber(sectorNumber);
			int sectorOffsetInCluster = getSectorOffsetInCluster(sectorNumber);
			FatFileInfo fileInfo = fatFileInfoMap[clusterNumber];
			if (fileInfo == null)
			{
				pendingWriteSectors[sectorNumber] = currentSector.Clone();
				if (log.DebugEnabled)
				{
					log.debug(string.Format("writeDataSector pending sectorNumber=0x{0:X}, clusterNumber=0x{1:X}", sectorNumber, clusterNumber));
				}
				return;
			}

			writeDataSector(sectorNumber, clusterNumber, sectorOffsetInCluster, fileInfo);
		}

		private void writeSecondFatSector(int fatIndex)
		{
			Array.Copy(currentSector, 0, secondFat, fatIndex * sectorSize, sectorSize);
		}

		private void writeEmptySector()
		{
		}

		public virtual string DeviceName
		{
			get
			{
				return deviceName;
			}
		}

		private void writeSector(int sectorNumber)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("writeSector 0x{0:X}", sectorNumber));
			}

			if (sectorNumber == bootSectorNumber)
			{
				writeBootSector();
			}
			else if (sectorNumber == fsInfoSectorNumber)
			{
				writeFsInfoSector();
			}
			else if (sectorNumber < fatSectorNumber)
			{
				writeEmptySector();
			}
			else if (sectorNumber >= fatSectorNumber && sectorNumber < fatSectorNumber + fatSectors)
			{
				writeFatSector(sectorNumber - fatSectorNumber);
			}
			else if (sectorNumber >= fatSectorNumber + fatSectors && sectorNumber < fatSectorNumber + numberOfFats * fatSectors)
			{
				// Writing to the second FAT table
				writeSecondFatSector(sectorNumber - fatSectorNumber - fatSectors);
			}
			else if (sectorNumber >= rootDirectoryStartSectorNumber && sectorNumber <= rootDirectoryEndSectorNumber)
			{
				writeRootDirectory(sectorNumber - rootDirectoryStartSectorNumber);
			}
			else
			{
				writeDataSector(sectorNumber);
			}
		}

		public virtual int ioRead(TPointer outputPointer, int outputLength)
		{
			int readLength = 0;
			int outputOffset = 0;
			while (outputLength > 0)
			{
				int sectorNumber = getSectorNumber(position);
				readSector(sectorNumber);
				int sectorOffset = getSectorOffset(position);
				int sectorLength = sectorSize - sectorOffset;
				int length = System.Math.Min(sectorLength, outputLength);

				outputPointer.setArray(outputOffset, currentSector, sectorOffset, length);

				outputLength -= length;
				outputOffset += length;
				position += length;
				readLength += length;
			}

			return readLength;
		}

		public virtual int ioRead(sbyte[] outputBuffer, int outputOffset, int outputLength)
		{
			int readLength = 0;
			while (outputLength > 0)
			{
				int sectorNumber = getSectorNumber(position);
				readSector(sectorNumber);
				int sectorOffset = getSectorOffset(position);
				int sectorLength = sectorSize - sectorOffset;
				int length = System.Math.Min(sectorLength, outputLength);

				Array.Copy(currentSector, sectorOffset, outputBuffer, outputOffset, length);

				outputLength -= length;
				outputOffset += length;
				position += length;
				readLength += length;
			}

			return readLength;
		}

		public virtual int ioWrite(TPointer inputPointer, int inputLength)
		{
			int writeLength = 0;
			int inputOffset = 0;
			while (inputLength > 0)
			{
				int sectorOffset = getSectorOffset(position);
				int sectorLength = sectorSize - sectorOffset;
				int length = System.Math.Min(sectorLength, inputLength);

				if (length != sectorSize)
				{
					// Not writing a complete sector, read the current sector
					int sectorNumber = getSectorNumber(position);
					readSector(sectorNumber);
				}

				Array.Copy(inputPointer.getArray8(inputOffset, length), 0, currentSector, sectorOffset, length);

				int sectorNumber = getSectorNumber(position);
				writeSector(sectorNumber);

				inputLength -= length;
				inputOffset += length;
				position += length;
				writeLength += length;
			}

			return writeLength;
		}

		public virtual int ioWrite(sbyte[] inputBuffer, int inputOffset, int inputLength)
		{
			return IO_ERROR;
		}

		public virtual long ioLseek(long offset)
		{
			position = offset;

			return position;
		}

		public virtual int ioIoctl(int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			return IO_ERROR;
		}

		public virtual long length()
		{
			return IO_ERROR;
		}

		public virtual bool SectorBlockMode
		{
			get
			{
				return false;
			}
		}

		public virtual long Position
		{
			get
			{
				return position;
			}
		}

		public virtual IVirtualFile duplicate()
		{
			return null;
		}

		public virtual IDictionary<IoFileMgrForUser.IoOperation, IoFileMgrForUser.IoOperationTiming> Timings
		{
			get
			{
				return IoFileMgrForUser.noDelayTimings;
			}
		}

		public virtual int ioClose()
		{
			vfs.ioExit();
			vfs = null;

			return 0;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			deviceName = stream.readString();
			position = stream.readLong();
			totalSectors = stream.readInt();
			fatSectors = stream.readInt();

			fatClusterMap = stream.readIntsWithLength();

			// Read the fatFileInfoMap in the format: index, alreadyReadIndex, [object,] index, alreadyReadIndex, [object]..., -1
			fatFileInfoMap = new FatFileInfo[fatClusterMap.Length];
			IList<FatFileInfo> fatFileInfoList = new LinkedList<FatFileInfo>();
			while (true)
			{
				int i = stream.readInt();
				if (i < 0)
				{
					// End of the fatFileInfoMap
					break;
				}

				int alreadyReadIndex = stream.readInt();
				if (alreadyReadIndex < 0)
				{
					FatFileInfo fatFileInfo = new FatFileInfo();
					fatFileInfoMap[i] = fatFileInfo;
					fatFileInfo.read(stream);
					fatFileInfoList.Add(fatFileInfo);
				}
				else
				{
					fatFileInfoMap[i] = fatFileInfoMap[alreadyReadIndex];
				}
			}

			// Read the parent-children relations
			foreach (FatFileInfo fatFileInfo in fatFileInfoList)
			{
				fatFileInfo.read(stream, this);
			}

			// Read the pendingWriteSectors
			pendingWriteSectors.Clear();
			while (true)
			{
				int sectorNumber = stream.readInt();
				if (sectorNumber < 0)
				{
					// End of the pendingWriteSectors
					break;
				}
				sbyte[] bytes = stream.readBytesWithLength();
				pendingWriteSectors[sectorNumber] = bytes;
			}

			// Read the pendingDeleteFiles
			pendingDeleteFiles.Clear();
			while (true)
			{
				int clusterNumber = stream.readInt();
				if (clusterNumber < 0)
				{
					// End of the pendingDeleteFiles
					break;
				}

				FatFileInfo info = readFatFileInfo(stream);
				pendingDeleteFiles[clusterNumber] = info;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeString(deviceName);
			stream.writeLong(position);
			stream.writeInt(totalSectors);
			stream.writeInt(fatSectors);

			stream.writeIntsWithLength(fatClusterMap);

			// Write the fatFileInfoMap in the format: index, alreadyWrittenIndex, [object,] index, alreadyWrittenIndex, [object]..., -1
			Dictionary<FatFileInfo, int> alreadyWritten = new Dictionary<FatFileInfo, int>();
			IList<FatFileInfo> fatFileInfoList = new LinkedList<FatFileInfo>();
			for (int i = 0; i < fatFileInfoMap.Length; i++)
			{
				FatFileInfo fatFileInfo = fatFileInfoMap[i];
				if (fatFileInfo != null)
				{
					stream.writeInt(i);
					int? alreadyWrittenIndex = alreadyWritten[fatFileInfo];
					if (alreadyWrittenIndex != null)
					{
						stream.writeInt(alreadyWrittenIndex.Value);
					}
					else
					{
						stream.writeInt(-1);
						fatFileInfo.write(stream);
						alreadyWritten[fatFileInfo] = i;
						fatFileInfoList.Add(fatFileInfo);
					}
				}
			}
			// End of the fatFileInfoMap
			stream.writeInt(-1);

			// Write the parent-children relations
			foreach (FatFileInfo fatFileInfo in fatFileInfoList)
			{
				fatFileInfo.write(stream, this);
			}

			// Write the pendingWriteSectors, followed by -1 to mark the end
			foreach (int sectorNumber in pendingWriteSectors.Keys)
			{
				stream.writeInt(sectorNumber);
				stream.writeBytesWithLength(pendingWriteSectors[sectorNumber]);
			}
			stream.writeInt(-1);

			// Write the pendingDeleteFiles, followed by -1 to mark the end
			foreach (int clusterNumber in pendingDeleteFiles.Keys)
			{
				stream.writeInt(clusterNumber);
				writeFatFileInfo(stream, pendingDeleteFiles[clusterNumber]);
			}
			stream.writeInt(-1);
		}

		private int getFatFileInfoMapIndex(FatFileInfo info)
		{
			if (info != null)
			{
				for (int i = 0; i < fatFileInfoMap.Length; i++)
				{
					if (info == fatFileInfoMap[i])
					{
						return i;
					}
				}
			}

			return -1;
		}

		private FatFileInfo getFatFileInfoFromMapIndex(int index)
		{
			if (index < 0 || index >= fatFileInfoMap.Length)
			{
				return null;
			}
			return fatFileInfoMap[index];
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public FatFileInfo readFatFileInfo(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual FatFileInfo readFatFileInfo(StateInputStream stream)
		{
			int mapIndex = stream.readInt();
			return getFatFileInfoFromMapIndex(mapIndex);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeFatFileInfo(pspsharp.state.StateOutputStream stream, FatFileInfo info) throws java.io.IOException
		public virtual void writeFatFileInfo(StateOutputStream stream, FatFileInfo info)
		{
			int mapIndex = getFatFileInfoMapIndex(info);
			stream.writeInt(mapIndex);
		}

		public override string ToString()
		{
			return string.Format("{0}{1}", string.ReferenceEquals(deviceName, null) ? "" : deviceName, vfs);
		}
	}

}