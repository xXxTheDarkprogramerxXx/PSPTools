using System.Collections.Generic;
using System.Text;

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
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_O_RDWR;


	using ScePspDateTime = pspsharp.HLE.kernel.types.ScePspDateTime;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;
	using Utilities = pspsharp.util.Utilities;

	public class FatFileInfo
	{
		private const int STATE_VERSION = 0;
		private string deviceName;
		private string dirName;
		private string fileName;
		private string fileName83;
		private bool directory;
		private bool readOnly;
		private ScePspDateTime lastModified;
		private long fileSize;
		private int[] clusters;
		private IList<FatFileInfo> children;
		private IVirtualFile vFile;
		private bool vFileOpen;
		private sbyte[] fileData;
		private FatFileInfo parentDirectory;

		public FatFileInfo()
		{
		}

		public FatFileInfo(string deviceName, string dirName, string fileName, bool directory, bool readOnly, ScePspDateTime lastModified, long fileSize)
		{
			this.deviceName = deviceName;
			this.dirName = dirName;
			this.fileName = fileName;
			this.directory = directory;
			this.readOnly = readOnly;
			this.lastModified = lastModified;
			this.fileSize = fileSize;
		}

		public virtual string DirName
		{
			get
			{
				return dirName;
			}
			set
			{
				this.dirName = value;
			}
		}


		public virtual string FileName
		{
			get
			{
				return fileName;
			}
			set
			{
				this.fileName = value;
			}
		}


		public virtual string FullFileName
		{
			get
			{
				if (string.ReferenceEquals(dirName, null))
				{
					return fileName;
				}
				return dirName + '/' + fileName;
			}
		}

		public virtual bool Directory
		{
			get
			{
				return directory;
			}
			set
			{
				this.directory = value;
			}
		}


		public virtual bool ReadOnly
		{
			get
			{
				return readOnly;
			}
			set
			{
				this.readOnly = value;
			}
		}


		public virtual ScePspDateTime LastModified
		{
			get
			{
				return lastModified;
			}
			set
			{
				this.lastModified = value;
			}
		}


		public virtual long FileSize
		{
			get
			{
				return fileSize;
			}
			set
			{
				this.fileSize = value;
			}
		}


		public virtual int[] Clusters
		{
			get
			{
				return clusters;
			}
			set
			{
				this.clusters = value;
			}
		}


		public virtual void addChild(FatFileInfo fileInfo)
		{
			if (children == null)
			{
				children = new LinkedList<FatFileInfo>();
			}

			children.Add(fileInfo);
			fileInfo.ParentDirectory = this;
		}

		public virtual IList<FatFileInfo> Children
		{
			get
			{
				return children;
			}
		}

		public virtual IVirtualFile getVirtualFile(IVirtualFileSystem vfs)
		{
			if (!vFileOpen)
			{
				vFile = vfs.ioOpen(FullFileName, PSP_O_RDWR, 0);
				vFileOpen = true;
			}

			return vFile;
		}

		public virtual void closeVirtualFile()
		{
			if (vFileOpen)
			{
				if (vFile != null)
				{
					vFile.ioClose();
					vFile = null;
				}
				vFileOpen = false;
			}
		}

		public virtual sbyte[] FileData
		{
			get
			{
				return fileData;
			}
			set
			{
				this.fileData = value;
			}
		}


		public virtual FatFileInfo ParentDirectory
		{
			get
			{
				return parentDirectory;
			}
			set
			{
				this.parentDirectory = value;
			}
		}


		public virtual bool RootDirectory
		{
			get
			{
				return Directory && string.ReferenceEquals(dirName, null) && string.ReferenceEquals(fileName, null);
			}
		}

		public virtual string FileName83
		{
			get
			{
				return fileName83;
			}
			set
			{
				this.fileName83 = value;
			}
		}


		public virtual FatFileInfo getChildByFileName83(string fileName83)
		{
			if (children != null)
			{
				foreach (FatFileInfo child in children)
				{
					string childFileName83 = child.FileName83;
					if (!string.ReferenceEquals(childFileName83, null) && fileName83.Equals(childFileName83))
					{
						return child;
					}
				}
			}

			return null;
		}

		public virtual bool hasCluster(int cluster)
		{
			if (clusters != null)
			{
				for (int i = 0; i < clusters.Length; i++)
				{
					if (clusters[i] == cluster)
					{
						return true;
					}
				}
			}

			return false;
		}

		public virtual void addCluster(int cluster)
		{
			clusters = Utilities.extendArray(clusters, 1);
			clusters[clusters.Length - 1] = cluster;
		}

		public virtual int FirstCluster
		{
			get
			{
				if (clusters == null || clusters.Length == 0)
				{
					return 0;
				}
    
				return clusters[0];
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			deviceName = stream.readString();
			dirName = stream.readString();
			fileName = stream.readString();
			fileName83 = stream.readString();
			directory = stream.readBoolean();
			readOnly = stream.readBoolean();
			int time = stream.readInt();
			if (time == 0)
			{
				lastModified = null;
			}
			else
			{
				lastModified = ScePspDateTime.fromMSDOSTime(time);
			}
			fileSize = stream.readLong();
			clusters = stream.readIntsWithLength();
			fileData = stream.readBytesWithLength();
			closeVirtualFile();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(pspsharp.state.StateInputStream stream, FatVirtualFile fatVirtualFile) throws java.io.IOException
		public virtual void read(StateInputStream stream, FatVirtualFile fatVirtualFile)
		{
			parentDirectory = fatVirtualFile.readFatFileInfo(stream);

			// Read the children
			children = null;
			int countChildren = stream.readInt();
			for (int i = 0; i < countChildren; i++)
			{
				FatFileInfo child = fatVirtualFile.readFatFileInfo(stream);
				if (child != null)
				{
					addChild(child);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeString(deviceName);
			stream.writeString(dirName);
			stream.writeString(fileName);
			stream.writeString(fileName83);
			stream.writeBoolean(directory);
			stream.writeBoolean(readOnly);
			if (lastModified == null)
			{
				stream.writeInt(0);
			}
			else
			{
				stream.writeInt(lastModified.toMSDOSTime());
			}
			stream.writeLong(fileSize);
			stream.writeIntsWithLength(clusters);
			stream.writeBytesWithLength(fileData);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(pspsharp.state.StateOutputStream stream, FatVirtualFile fatVirtualFile) throws java.io.IOException
		public virtual void write(StateOutputStream stream, FatVirtualFile fatVirtualFile)
		{
			fatVirtualFile.writeFatFileInfo(stream, parentDirectory);

			// Write the children
			if (children == null)
			{
				stream.writeInt(0);
			}
			else
			{
				stream.writeInt(children.Count);
				foreach (FatFileInfo child in children)
				{
					fatVirtualFile.writeFatFileInfo(stream, child);
				}
			}
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();

			if (!string.ReferenceEquals(deviceName, null))
			{
				s.Append(deviceName);
			}

			if (string.ReferenceEquals(FullFileName, null))
			{
				s.Append("[ROOT]");
			}
			else
			{
				s.Append("/");
				s.Append(FullFileName);
			}

			if (!string.ReferenceEquals(fileName83, null))
			{
				s.Append(string.Format("('{0}')", fileName83));
			}

			if (directory)
			{
				s.Append(", directory");
			}

			if (readOnly)
			{
				s.Append(", readOnly");
			}

			s.Append(string.Format(", size=0x{0:X}", fileSize));

			if (clusters != null)
			{
				s.Append(", clusters=[");
				for (int i = 0; i < clusters.Length; i++)
				{
					if (i > 0)
					{
						s.Append(", ");
					}
					s.Append(string.Format("0x{0:X}", clusters[i]));
				}
				s.Append("]");
			}

			return s.ToString();
		}
	}

}