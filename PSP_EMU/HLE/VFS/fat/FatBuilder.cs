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
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorInt16;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorInt32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorInt8;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.fat.FatUtils.storeSectorString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.pspAbstractMemoryMappedStructure.charset16;

	using Logger = org.apache.log4j.Logger;

	using SceIoDirent = pspsharp.HLE.kernel.types.SceIoDirent;
	using SceIoStat = pspsharp.HLE.kernel.types.SceIoStat;
	using ScePspDateTime = pspsharp.HLE.kernel.types.ScePspDateTime;
	using Utilities = pspsharp.util.Utilities;

	public class FatBuilder
	{
		private static Logger log = FatVirtualFile.log;
		public const int bootSectorNumber = 0;
		public const int numberOfFats = 2;
		public const int reservedSectors = 32;
		public const int directoryTableEntrySize = 32;
		private readonly FatVirtualFile vFile;
		private readonly IVirtualFileSystem vfs;
		private readonly int maxNumberClusters;
		private int firstFreeCluster;

		public FatBuilder(FatVirtualFile vFile, IVirtualFileSystem vfs, int maxNumberClusters)
		{
			this.vFile = vFile;
			this.vfs = vfs;
			this.maxNumberClusters = maxNumberClusters;
		}

		public virtual FatFileInfo scan()
		{
			firstFreeCluster = vFile.FirstFreeCluster;

			FatFileInfo rootDirectory = new FatFileInfo(vFile.DeviceName, null, null, true, false, null, 0);
			rootDirectory.ParentDirectory = rootDirectory;

			scan(null, rootDirectory);

			vFile.RootDirectory = rootDirectory;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Using 0x{0:X} clusters out of 0x{1:X}", firstFreeCluster, maxNumberClusters));
				debugScan(rootDirectory);
			}

			if (firstFreeCluster > maxNumberClusters)
			{
				log.error(string.Format("Too many files in the Fat partition: required clusters=0x{0:X}, max clusters=0x{1:X}", firstFreeCluster, maxNumberClusters));
			}

			return rootDirectory;
		}

		private void debugScan(FatFileInfo fileInfo)
		{
			log.debug(string.Format("scan {0}", fileInfo));
			IList<FatFileInfo> children = fileInfo.Children;
			if (children != null)
			{
				foreach (FatFileInfo child in children)
				{
					debugScan(child);
				}
			}
		}

		public virtual void setClusters(FatFileInfo fileInfo, int[] clusters)
		{
			if (clusters != null)
			{
				for (int i = 0; i < clusters.Length; i++)
				{
					vFile.setFatFileInfoMap(clusters[i], fileInfo);
				}
			}
			fileInfo.Clusters = clusters;
		}

		private int allocateCluster()
		{
			return firstFreeCluster++;
		}

		private int[] allocateClusters(long size)
		{
			int clusterSize = vFile.ClusterSize;
			int numberClusters = (int)((size + clusterSize - 1) / clusterSize);
			if (numberClusters <= 0)
			{
				return null;
			}

			int[] clusters = new int[numberClusters];
			for (int i = 0; i < numberClusters; i++)
			{
				clusters[i] = allocateCluster();
			}

			// Fill the cluster chain in the cluster map
			for (int i = 0; i < numberClusters - 1; i++)
			{
				// Pointing to the next cluster
				vFile.setFatClusterMap(clusters[i], clusters[i + 1]);
			}
			if (numberClusters > 0)
			{
				// Last cluster in file (EOC)
				vFile.setFatClusterMap(clusters[numberClusters - 1], vFile.FatEOC);
			}

			return clusters;
		}

		private void allocateClusters(FatFileInfo fileInfo)
		{
			long dataSize = fileInfo.FileSize;
			if (fileInfo.Directory)
			{
				// Two child entries for "." and ".."
				int directoryTableEntries = 2;

				IList<FatFileInfo> children = fileInfo.Children;
				if (children != null)
				{
					// TODO: take fake entries into account to support "long filename"
					directoryTableEntries += children.Count;
				}

				dataSize = directoryTableEntrySize * directoryTableEntries;
			}

			int[] clusters = allocateClusters(dataSize);
			setClusters(fileInfo, clusters);
		}

		private void scan(string dirName, FatFileInfo parent)
		{
			string[] names = vfs.ioDopen(dirName);
			if (names == null || names.Length == 0)
			{
				return;
			}

			SceIoStat stat = new SceIoStat();
			SceIoDirent dir = new SceIoDirent(stat, null);
			for (int i = 0; i < names.Length; i++)
			{
				dir.filename = names[i];
				if (vfs.ioDread(dirName, dir) >= 0)
				{
					bool directory = (dir.stat.attr & 0x10) != 0;
					bool readOnly = (dir.stat.mode & 0x2) == 0;
					FatFileInfo fileInfo = new FatFileInfo(vFile.DeviceName, dirName, dir.filename, directory, readOnly, dir.stat.mtime, dir.stat.size);

					parent.addChild(fileInfo);

					if (directory)
					{
						if (string.ReferenceEquals(dirName, null))
						{
							scan(dir.filename, fileInfo);
						}
						else
						{
							scan(dirName + "/" + dir.filename, fileInfo);
						}
					}

					// Allocate the clusters after having scanned the children
					allocateClusters(fileInfo);
				}
			}

			IList<FatFileInfo> children = parent.Children;
			if (children != null)
			{
				foreach (FatFileInfo child in children)
				{
					computeFileName83(child, children);
				}
			}
		}

		private void computeFileName83(FatFileInfo fileInfo, IList<FatFileInfo> siblings)
		{
			int collisionIndex = 0;
			string fileName = fileInfo.FileName;
			string fileName83 = convertFileNameTo83(fileName, collisionIndex);

			// Check if the 8.3 file name is not colliding
			// with other 8.3 file names in the same directory.
			bool hasCollision;
			do
			{
				hasCollision = false;
				foreach (FatFileInfo sibling in siblings)
				{
					string siblingFileName83 = sibling.FileName83;
					if (!string.ReferenceEquals(siblingFileName83, null))
					{
						if (fileName83.Equals(siblingFileName83))
						{
							// 8.3 file name collision
							collisionIndex++;
							hasCollision = true;
							fileName83 = convertFileNameTo83(fileName, collisionIndex);
							break;
						}
					}
				}
			} while (hasCollision);

			fileInfo.FileName83 = fileName83;
		}

		public static string convertFileName8_3To83(string fileName8_3)
		{
			string name = fileName8_3;
			string extension = "";
			int dotIndex = name.IndexOf('.');
			if (dotIndex >= 0)
			{
				extension = name.Substring(dotIndex + 1);
				name = name.Substring(0, dotIndex);
			}

			name = (name + "        ").Substring(0, 8);
			extension = (extension + "   ").Substring(0, 3);

			return name + extension;
		}

		// Convert a "long" file name into a 8.3 file name.
		private static string convertFileNameTo8_3(string fileName, int collisionIndex)
		{
			if (string.ReferenceEquals(fileName, null))
			{
				return null;
			}

			// Special character '+' is turned into '_'
			fileName = fileName.Replace("+", "_");
			// File name is upper-cased
			fileName = fileName.ToUpper();

			// Split into the name and extension parts
			int lastDot = fileName.LastIndexOf(".", StringComparison.Ordinal);
			string name;
			string ext;
			if (lastDot < 0)
			{
				name = fileName;
				ext = "";
			}
			else
			{
				name = fileName.Substring(0, lastDot);
				ext = fileName.Substring(lastDot + 1);
			}

			// All dots in name part are dropped
			name = name.Replace(".", "");

			// The file extension is truncated to 3 characters
			if (ext.Length > 3)
			{
				ext = ext.Substring(0, 3);
			}

			if (collisionIndex >= 1)
			{
				if (collisionIndex <= 4)
				{
					// The name is truncated to 6 characters, followed by "~N"
					if (name.Length > 6)
					{
						name = name.Substring(0, 6);
					}
					name += "~" + collisionIndex;
				}
				else
				{
					// The name is truncated to 2 characters,
					// followed by 4 hexadecimal digits derived
					// from an undocumented hash of the filename,
					// followed by a tilde, followed by a single digit.
					if (name.Length > 2)
					{
						name = name.Substring(0, 2);
					}
					name += string.Format("{0:X4}", collisionIndex) + "~1";
				}
			}
			else if (name.Length > 8)
			{
				// The name is truncated to 6 characters (if longer than 8 characters)
				// followed by "~1"
				name = name.Substring(0, 6) + "~1";
			}

			if (ext.Length == 0)
			{
				return name;
			}
			return name + "." + ext;
		}

		private static string convertFileNameTo83(string fileName, int collisionIndex)
		{
			string fileName8_3 = convertFileNameTo8_3(fileName, collisionIndex);
			return convertFileName8_3To83(fileName8_3);
		}

		private static string convertFileName83To8_3(string fileName83)
		{
			int endName = 8;
			for (; endName > 0; endName--)
			{
				if (fileName83[endName - 1] != ' ')
				{
					break;
				}
			}
			string name = fileName83.Substring(0, endName);

			int endExt = 8 + 3;
			for (; endExt > 8; endExt--)
			{
				if (fileName83[endExt - 1] != ' ')
				{
					break;
				}
			}
			string ext = fileName83.Substring(8, endExt - 8);

			if (ext.Length == 0)
			{
				return name;
			}
			return name + "." + ext;
		}

		private sbyte[] addLongFileNameDirectoryEntries(sbyte[] directoryData, string fileName, int fileNameChecksum)
		{
			sbyte[] fileNameBytes = fileName.GetBytes(charset16);
			int numberEntries = System.Math.Max((fileNameBytes.Length + 25) / 26, 1);

			sbyte[] extend = new sbyte[numberEntries * 26 - fileNameBytes.Length];
			if (extend.Length >= 2)
			{
				extend[0] = (sbyte) 0;
				extend[1] = (sbyte) 0;
				for (int i = 2; i < extend.Length; i++)
				{
					extend[i] = unchecked((sbyte) 0xFF);
				}
				fileNameBytes = Utilities.extendArray(fileNameBytes, extend);
			}

			int offset = directoryData.Length;
			directoryData = Utilities.extendArray(directoryData, directoryTableEntrySize * numberEntries);

			for (int i = numberEntries; i > 0; i--)
			{
				int sequenceNumber = i;
				if (i == numberEntries)
				{
					sequenceNumber |= 0x40; // Last LFN entry
				}
				storeSectorInt8(directoryData, offset + 0, sequenceNumber);

				// Name characters (five UCS-2 characters)
				int fileNameBytesOffset = (i - 1) * 26;
				Array.Copy(fileNameBytes, fileNameBytesOffset, directoryData, offset + 1, 10);
				fileNameBytesOffset += 10;

				// Attributes (always 0x0F)
				storeSectorInt8(directoryData, offset + 11, 0x0F);

				// Type (always 0x00 for VFAT LFN)
				storeSectorInt8(directoryData, offset + 12, 0x00);

				// Checksum of DOS file name
				storeSectorInt8(directoryData, offset + 13, fileNameChecksum);

				// Name characters (six UCS-2 characters)
				Array.Copy(fileNameBytes, fileNameBytesOffset, directoryData, offset + 14, 12);
				fileNameBytesOffset += 12;

				// First cluster (always 0)
				storeSectorInt16(directoryData, offset + 26, 0);

				// Name characters (two UCS-2 characters)
				Array.Copy(fileNameBytes, fileNameBytesOffset, directoryData, offset + 28, 4);

				offset += directoryTableEntrySize;
			}

			return directoryData;
		}

		private int getFileNameChecksum(string fileName)
		{
			int checksum = 0;

			for (int i = 0; i < fileName.Length; i++)
			{
				int c = fileName[i] & 0xFF;
				checksum = (((checksum & 1) << 7) + (checksum >> 1) + c) & 0xFF;
			}

			return checksum;
		}

		private bool isLongFileName(string fileName8_3, string fileName)
		{
			return !fileName8_3.Equals(fileName, StringComparison.OrdinalIgnoreCase);
		}

		private sbyte[] addDirectoryEntry(sbyte[] directoryData, FatFileInfo fileInfo)
		{
			string fileName = fileInfo.FileName;
			string fileName83 = fileInfo.FileName83;
			string fileName8_3 = convertFileName83To8_3(fileName83);
			if (isLongFileName(fileName8_3, fileName))
			{
				int checksum = getFileNameChecksum(fileName83);
				directoryData = addLongFileNameDirectoryEntries(directoryData, fileName, checksum);
			}

			int offset = directoryData.Length;
			directoryData = Utilities.extendArray(directoryData, directoryTableEntrySize);

			storeSectorString(directoryData, offset + 0, fileName83, 8 + 3);

			int fileAttributes = 0x20; // Archive attribute
			if (fileInfo.ReadOnly)
			{
				fileAttributes |= 0x01; // Read Only attribute
			}
			if (fileInfo.Directory)
			{
				fileAttributes |= 0x10; // Sub-directory attribute
			}
			storeSectorInt8(directoryData, offset + 11, fileAttributes);

			// Has extended attributes?
			storeSectorInt8(directoryData, offset + 12, 0);

			ScePspDateTime lastModified = fileInfo.LastModified;
			storeSectorInt8(directoryData, offset + 13, 0); // Milliseconds always set to 0 by the PSP

			int createTime = lastModified.hour << 11;
			createTime |= lastModified.minute << 5;
			createTime |= lastModified.second >> 1;
			storeSectorInt16(directoryData, offset + 14, createTime);

			int createDate = (lastModified.year - 1980) << 9;
			createDate |= lastModified.month << 5;
			createDate |= lastModified.day;
			storeSectorInt16(directoryData, offset + 16, createDate);

			storeSectorInt16(directoryData, offset + 18, createDate);

			int[] clusters = fileInfo.Clusters;
			if (clusters != null)
			{
				storeSectorInt16(directoryData, offset + 20, (int)((uint)clusters[0] >> 16));
			}
			else
			{
				storeSectorInt16(directoryData, offset + 20, 0); // Empty file
			}

			storeSectorInt16(directoryData, offset + 22, createTime);
			storeSectorInt16(directoryData, offset + 24, createDate);

			if (clusters != null)
			{
				storeSectorInt16(directoryData, offset + 26, clusters[0] & 0xFFFF);
			}
			else
			{
				storeSectorInt16(directoryData, offset + 26, 0); // Empty file
			}

			int fileSize = (int) fileInfo.FileSize;
			if (fileInfo.Directory)
			{
				fileSize = 0;
			}
			storeSectorInt32(directoryData, offset + 28, fileSize);

			return directoryData;
		}

		private void buildDotDirectoryEntry(sbyte[] directoryData, int offset, FatFileInfo fileInfo, string dotName, ScePspDateTime alternateLastModified)
		{
			storeSectorString(directoryData, offset + 0, dotName, 8 + 3);

			// File attributes: directory
			storeSectorInt8(directoryData, offset + 11, 0x10);

			// Has extended attributes?
			storeSectorInt8(directoryData, offset + 12, 0);

			ScePspDateTime lastModified = fileInfo.LastModified;
			if (lastModified == null)
			{
				// The root directory has no lastModified date/time,
				// rather use the date/time of the sub-directory.
				lastModified = alternateLastModified;
			}
			storeSectorInt8(directoryData, offset + 13, 0); // Milliseconds, always set to 0 by the PSP

			int createTime = lastModified.hour << 11;
			createTime |= lastModified.minute << 5;
			createTime |= lastModified.second >> 1;
			storeSectorInt16(directoryData, offset + 14, createTime);

			int createDate = (lastModified.year - 1980) << 9;
			createDate |= lastModified.month << 5;
			createDate |= lastModified.day;
			storeSectorInt16(directoryData, offset + 16, createDate);

			storeSectorInt16(directoryData, offset + 18, createDate);

			int[] clusters = fileInfo.Clusters;
			if (clusters != null)
			{
				storeSectorInt16(directoryData, offset + 20, (int)((uint)clusters[0] >> 16));
			}
			else
			{
				storeSectorInt16(directoryData, offset + 20, 0); // Empty file
			}

			storeSectorInt16(directoryData, offset + 22, createTime);
			storeSectorInt16(directoryData, offset + 24, createDate);

			if (clusters != null)
			{
				storeSectorInt16(directoryData, offset + 26, clusters[0] & 0xFFFF);
			}
			else
			{
				storeSectorInt16(directoryData, offset + 26, 0); // Empty file
			}

			// File size
			storeSectorInt32(directoryData, offset + 28, 0);
		}

		public virtual sbyte[] buildDirectoryData(FatFileInfo fileInfo)
		{
			sbyte[] directoryData;

			// Is this the root directory?
			if (fileInfo.RootDirectory)
			{
				// The root directory has no "." nor ".." directory entries
				directoryData = new sbyte[0];
			}
			else
			{
				// Non-root directories have "." and ".." directory entries
				directoryData = new sbyte[directoryTableEntrySize * 2];

				buildDotDirectoryEntry(directoryData, 0, fileInfo, ".", fileInfo.LastModified);
				buildDotDirectoryEntry(directoryData, directoryTableEntrySize, fileInfo.ParentDirectory, "..", fileInfo.LastModified);
			}

			IList<FatFileInfo> children = fileInfo.Children;
			if (children != null)
			{
				foreach (FatFileInfo child in children)
				{
					directoryData = addDirectoryEntry(directoryData, child);
				}
			}

			return directoryData;
		}
	}

}