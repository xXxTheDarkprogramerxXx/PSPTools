using System;
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
namespace pspsharp.filesystems.umdiso
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.filesystems.umdiso.UmdIsoFile.sectorLength;


	using Iso9660Directory = pspsharp.filesystems.umdiso.iso9660.Iso9660Directory;
	using Iso9660File = pspsharp.filesystems.umdiso.iso9660.Iso9660File;
	using Iso9660Handler = pspsharp.filesystems.umdiso.iso9660.Iso9660Handler;
	using Settings = pspsharp.settings.Settings;
	using Utilities = pspsharp.util.Utilities;

	/// 
	/// <summary>
	/// @author gigaherz, gid15
	/// </summary>
	public class UmdIsoReader : IBrowser
	{
		public const int startSector = 16;
		public const int startSectorJoliet = 17;
		private const int headerLength = 24;
		private ISectorDevice sectorDevice;
		private IBrowser browser;
		private readonly Dictionary<string, Iso9660File> fileCache = new Dictionary<string, Iso9660File>();
		private readonly Dictionary<string, Iso9660Directory> dirCache = new Dictionary<string, Iso9660Directory>();
		private int numSectors;
		private static bool doIsoBuffering = false;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool hasJolietExtension_Renamed;
		private bool isPBP;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public UmdIsoReader(String umdFilename) throws java.io.IOException, java.io.FileNotFoundException
		public UmdIsoReader(string umdFilename)
		{
			init(umdFilename, doIsoBuffering);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public UmdIsoReader(String umdFilename, boolean doIsoBuffering) throws java.io.IOException, java.io.FileNotFoundException
		public UmdIsoReader(string umdFilename, bool doIsoBuffering)
		{
			init(umdFilename, doIsoBuffering);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void init(String umdFilename, boolean doIsoBuffering) throws java.io.IOException, java.io.FileNotFoundException
		private void init(string umdFilename, bool doIsoBuffering)
		{
			isPBP = false;
			if (string.ReferenceEquals(umdFilename, null) && doIsoBuffering)
			{
				sectorDevice = null;
			}
			else
			{
				RandomAccessFile fileReader = new RandomAccessFile(umdFilename, "r");

				sbyte[] header = new sbyte[headerLength];
				fileReader.seek(0);
				fileReader.read(header);
				fileReader.seek(0);

				if (header[0] == (sbyte)'C' && header[1] == (sbyte)'I' && header[2] == (sbyte)'S' && header[3] == (sbyte)'O')
				{
					sectorDevice = new CSOFileSectorDevice(fileReader, header);
				}
				else if (header[0] == 0 && header[1] == (sbyte)'P' && header[2] == (sbyte)'B' && header[3] == (sbyte)'P')
				{
					sectorDevice = new PBPFileSectorDevice(fileReader);
					isPBP = true;
				}
				else
				{
					sectorDevice = new ISOFileSectorDevice(fileReader);
				}
			}

			if (doIsoBuffering)
			{
				string tmp = Settings.Instance.TmpDirectory;
				sectorDevice = new BufferedFileSectorDevice(new RandomAccessFile(tmp + "umdbuffer.toc", "rw"), new RandomAccessFile(tmp + "umdbuffer.iso", "rw"), sectorDevice);
			}

			numSectors = sectorDevice.NumSectors;

			setBrowser();

			if (browser == null && !hasIsoHeader())
			{
				throw new IOException(string.Format("Unsupported file format or corrupted file '{0}'.", umdFilename));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public UmdIsoReader(ISectorDevice sectorDevice) throws java.io.IOException
		public UmdIsoReader(ISectorDevice sectorDevice)
		{
			this.sectorDevice = sectorDevice;
			numSectors = sectorDevice.NumSectors;
			setBrowser();
		}

		private void setBrowser()
		{
			if (sectorDevice is IBrowser)
			{
				browser = (IBrowser) sectorDevice;
			}
			else
			{
				browser = null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
		public virtual void close()
		{
			sectorDevice.close();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean hasIsoHeader() throws java.io.IOException
		private bool hasIsoHeader()
		{
			if (numSectors <= 0)
			{
				return false;
			}

			UmdIsoFile f = new UmdIsoFile(this, startSector, sectorLength, null, null);
			sbyte[] header = new sbyte[6];
			int Length = f.read(header);
			f.Dispose();
			if (Length < header.Length)
			{
				return false;
			}

			if (header[1] != (sbyte)'C' || header[2] != (sbyte)'D' || header[3] != (sbyte)'0' || header[4] != (sbyte)'0' || header[5] != (sbyte)'1')
			{
				return false;
			}

			hasJolietExtension_Renamed = false;
			f = new UmdIsoFile(this, startSectorJoliet, sectorLength, null, null);
			Length = f.read(header);
			f.Dispose();
			if (Length == header.Length)
			{
				if (header[0] == 2 && header[1] == (sbyte)'C' && header[2] == (sbyte)'D' && header[3] == (sbyte)'0' && header[4] == (sbyte)'0' && header[5] == (sbyte)'1')
				{
					hasJolietExtension_Renamed = true;
				}
			}

			return true;
		}

		public virtual bool hasJolietExtension()
		{
			return hasJolietExtension_Renamed;
		}

		public virtual int NumSectors
		{
			get
			{
				return numSectors;
			}
		}

		public virtual ISectorDevice SectorDevice
		{
			get
			{
				return sectorDevice;
			}
		}

		/// <summary>
		/// Read sequential sectors into a byte array
		/// </summary>
		/// <param name="sectorNumber"> - the first sector to be read </param>
		/// <param name="numberSectors"> - the number of sectors to be read </param>
		/// <param name="buffer"> - the byte array where to write the sectors </param>
		/// <param name="offset"> - offset into the byte array where to start writing </param>
		/// <returns> the number of sectors read </returns>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readSectors(int sectorNumber, int numberSectors, byte[] buffer, int offset) throws java.io.IOException
		public virtual int readSectors(int sectorNumber, int numberSectors, sbyte[] buffer, int offset)
		{
			if (sectorNumber < 0 || (sectorNumber + numberSectors) > numSectors)
			{
				Arrays.Fill(buffer, offset, offset + numberSectors * sectorLength, (sbyte) 0);
				Emulator.Console.WriteLine(string.Format("Sectors start={0:D}, end={1:D} out of ISO (numSectors={2:D})", sectorNumber, sectorNumber + numberSectors, numSectors));
				return numberSectors;
			}

			return sectorDevice.readSectors(sectorNumber, numberSectors, buffer, offset);
		}

		/// <summary>
		/// Read one sector into a byte array
		/// </summary>
		/// <param name="sectorNumber"> - the sector number to be read </param>
		/// <param name="buffer"> - the byte array where to write </param>
		/// <param name="offset"> - offset into the byte array where to start writing </param>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readSector(int sectorNumber, byte[] buffer, int offset) throws java.io.IOException
		public virtual void readSector(int sectorNumber, sbyte[] buffer, int offset)
		{
			if (sectorNumber < 0 || sectorNumber >= numSectors)
			{
				Arrays.Fill(buffer, offset, offset + sectorLength, (sbyte) 0);
				Emulator.Console.WriteLine(string.Format("Sector number {0:D} out of ISO (numSectors={1:D})", sectorNumber, numSectors));
				return;
			}

			sectorDevice.readSector(sectorNumber, buffer, offset);
		}

		/// <summary>
		/// Read one sector
		/// </summary>
		/// <param name="sectorNumber"> - the sector number to be read </param>
		/// <returns> a new byte array of size sectorLength containing the sector </returns>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte[] readSector(int sectorNumber) throws java.io.IOException
		public virtual sbyte[] readSector(int sectorNumber)
		{
			return readSector(sectorNumber, null);
		}

		/// <summary>
		/// Read one sector
		/// </summary>
		/// <param name="sectorNumber"> - the sector number to be read </param>
		/// <param name="buffer"> - try to reuse this buffer if possible </param>
		/// <returns> a new byte array of size sectorLength containing the sector or
		/// the buffer if it could be reused. </returns>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte[] readSector(int sectorNumber, byte[] buffer) throws java.io.IOException
		public virtual sbyte[] readSector(int sectorNumber, sbyte[] buffer)
		{
			if (buffer == null || buffer.Length != sectorLength)
			{
				buffer = new sbyte[sectorLength];
			}
			readSector(sectorNumber, buffer, 0);

			return buffer;
		}

		private int removePath(string[] path, int index, int Length)
		{
			if (index < 0 || index >= Length)
			{
				return Length;
			}

			for (int i = index + 1; i < Length; i++)
			{
				path[i - 1] = path[i];
			}

			return Length - 1;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private pspsharp.filesystems.umdiso.iso9660.Iso9660File getFileEntry(String filePath) throws java.io.IOException, java.io.FileNotFoundException
		private Iso9660File getFileEntry(string filePath)
		{
			Iso9660File info;

			info = fileCache[filePath];
			if (info != null)
			{
				return info;
			}

			int parentDirectoryIndex = filePath.LastIndexOf('/');
			if (parentDirectoryIndex >= 0)
			{
				string parentDirectory = filePath.Substring(0, parentDirectoryIndex);
				Iso9660Directory dir = dirCache[parentDirectory];
				if (dir != null)
				{
					int index = dir.getFileIndex(filePath.Substring(parentDirectoryIndex + 1));
					info = dir.getEntryByIndex(index);
					if (info != null)
					{
						fileCache[filePath] = info;
						return info;
					}
				}
			}

			Iso9660Directory dir = new Iso9660Handler(this);

			string[] path = filePath.Split("[\\/]", true);

			// First convert the path to a canonical path by removing all the
			// occurrences of "." and "..".
			int pathLength = path.Length;
			for (int i = 0; i < pathLength;)
			{
				if (path[i].Equals("."))
				{
					// Remove "."
					pathLength = removePath(path, i, pathLength);
				}
				else if (path[i].Equals(".."))
				{
					// Remove ".." and its parent
					pathLength = removePath(path, i, pathLength);
					pathLength = removePath(path, i - 1, pathLength);
				}
				else
				{
					i++;
				}
			}

			// walk through the canonical path
			for (int i = 0; i < pathLength;)
			{
				int index = dir.getFileIndex(path[i]);

				info = dir.getEntryByIndex(index);

				if (isDirectory(info))
				{
					dir = new Iso9660Directory(this, info.LBA, info.Size);
					StringBuilder dirPath = new StringBuilder(path[0]);
					for (int j = 1; j <= i; j++)
					{
						dirPath.Append("/").Append(path[j]);
					}
					dirCache[dirPath.ToString()] = dir;
				}
				i++;
			}

			if (info != null)
			{
				fileCache[filePath] = info;
			}

			return info;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public UmdIsoFile getFile(String filePath) throws java.io.IOException, java.io.FileNotFoundException
		public virtual UmdIsoFile getFile(string filePath)
		{
			if (numSectors == 0)
			{
				throw new FileNotFoundException(filePath);
			}
			int fileStart;
			long fileLength;
			DateTime timestamp;
			string fileName = null;

			if (!string.ReferenceEquals(filePath, null) && filePath.StartsWith("sce_lbn", StringComparison.Ordinal))
			{
				//
				// Direct sector access on UMD is using the following file name syntax:
				//     sce_lbnSSSS_sizeLLLL
				// where SSSS is the index of the first sector (in hexadecimal)
				//       LLLL is the Length in bytes (in hexadecimal)
				// The prefix "0x" before each hexadecimal value is optional.
				//
				// E.g.
				//       disc0:/sce_lbn0x5fa0_size0x1428
				//       disc0:/sce_lbn7050_sizeee850
				//
				// Remark: SSSS and LLLL can be followed by any non-hex characters.
				//         These additional characters are simply ignored.
				//
				filePath = filePath.Substring(7);
				int sep = filePath.IndexOf("_size", StringComparison.Ordinal);
				fileStart = (int) Utilities.parseHexLong(filePath.Substring(0, sep), true);
				fileLength = Utilities.parseHexLong(filePath.Substring(sep + 5), true);
				timestamp = DateTime.Now;
				fileName = null;
				if (fileStart < 0 || fileStart >= numSectors)
				{
					throw new IOException("File '" + filePath + "': Invalid Start Sector");
				}
			}
			else if (!string.ReferenceEquals(filePath, null) && filePath.Length == 0)
			{
				fileStart = 0;
				fileLength = ((long) numSectors) * sectorLength;
				timestamp = DateTime.Now;
			}
			else
			{
				Iso9660File info = getFileEntry(filePath);
				if (info != null && isDirectory(info))
				{
					info = null;
				}

				if (info == null)
				{
					throw new FileNotFoundException("File '" + filePath + "' not found or not a file.");
				}

				fileStart = info.LBA;
				fileLength = info.Size;
				timestamp = info.Timestamp;
				fileName = info.FileName;
			}

			return new UmdIsoFile(this, fileStart, fileLength, timestamp, fileName);
		}

		public virtual bool hasFile(string filePath)
		{
			try
			{
				UmdIsoFile umdIsoFile = getFile(filePath);
				if (umdIsoFile != null)
				{
					umdIsoFile.Dispose();
					return true;
				}
			}
			catch (FileNotFoundException)
			{
			}
			catch (IOException)
			{
			}

			return false;
		}

		public virtual string resolveSectorPath(int start, long Length)
		{
			string fileName = null;
			// Scroll back through the sectors until the file's start sector is reached
			// and it's name can be obtained.
			while ((string.ReferenceEquals(fileName, null)) || (start <= startSector))
			{
				fileName = getFileName(start);
				start--;
			}
			return fileName;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String[] listDirectory(String filePath) throws java.io.IOException, java.io.FileNotFoundException
		public virtual string[] listDirectory(string filePath)
		{
			Iso9660Directory dir = null;

			if (filePath.Length == 0)
			{
				dir = new Iso9660Handler(this);
			}
			else
			{
				Iso9660File info = getFileEntry(filePath);
				if (info != null && isDirectory(info))
				{
					dir = new Iso9660Directory(this, info.LBA, info.Size);
				}
			}

			if (dir == null)
			{
				throw new FileNotFoundException("File '" + filePath + "' not found or not a directory.");
			}

			return dir.FileList;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getFileProperties(String filePath) throws java.io.IOException, java.io.FileNotFoundException
		public virtual int getFileProperties(string filePath)
		{
			if (filePath.Length == 0)
			{
				return 2;
			}

			Iso9660File info = getFileEntry(filePath);

			if (info == null)
			{
				throw new FileNotFoundException("File '" + filePath + "' not found.");
			}

			return info.Properties;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean isDirectory(String filePath) throws java.io.IOException, java.io.FileNotFoundException
		public virtual bool isDirectory(string filePath)
		{
			return ((getFileProperties(filePath) & 2) == 2);
		}

		public virtual bool isDirectory(Iso9660File file)
		{
			return (file.Properties & 2) == 2;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private String getFileNameRecursive(int fileStartSector, String path, String[] files) throws java.io.FileNotFoundException, java.io.IOException
		private string getFileNameRecursive(int fileStartSector, string path, string[] files)
		{
			foreach (string file in files)
			{
				string filePath = path + "/" + file;
				Iso9660File info = null;
				if (path.Length == 0)
				{
					filePath = file;
				}
				else
				{
					info = getFileEntry(filePath);
					if (info != null)
					{
						if (info.LBA == fileStartSector)
						{
							return info.FileName;
						}
					}
				}

				if ((info == null || isDirectory(info)) && !file.Equals(".") && !file.Equals("\x0001"))
				{
					try
					{
						string[] childFiles = listDirectory(filePath);
						string fileName = getFileNameRecursive(fileStartSector, filePath, childFiles);
						if (!string.ReferenceEquals(fileName, null))
						{
							return fileName;
						}
					}
					catch (FileNotFoundException)
					{
						// Continue
					}
				}
			}

			return null;
		}

		public virtual string getFileName(int fileStartSector)
		{
			try
			{
				string[] files = listDirectory("");
				return getFileNameRecursive(fileStartSector, "", files);
			}
			catch (FileNotFoundException)
			{
				// Ignore Exception
			}
			catch (IOException)
			{
				// Ignore Exception
			}

			return null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public long dumpIndexRecursive(java.io.PrintWriter out, String path, String[] files) throws java.io.IOException
		public virtual long dumpIndexRecursive(PrintWriter @out, string path, string[] files)
		{
			long size = 0;
			foreach (string file in files)
			{
				string filePath = path + "/" + file;
				Iso9660File info;
				int fileStart = 0;
				long fileLength = 0;

				if (path.Length == 0)
				{
					filePath = file;
				}

				info = getFileEntry(filePath);
				if (info != null)
				{
					fileStart = info.LBA;
					fileLength = info.Size;
					size += (fileLength + 0x7FF) & ~0x7FF;
				}

				// "." isn't a directory (throws an exception)
				// "\01" claims to be a directory but ends up in an infinite loop
				// ignore them here as they do not contribute much to the listing
				if (file.Equals(".") || file.Equals("\x0001"))
				{
					continue;
				}

				if (info == null || isDirectory(info))
				{
					@out.println(string.Format("D {0:X8} {1,10:D} {2}", fileStart, fileLength, filePath));
					string[] childFiles = listDirectory(filePath);
					size += dumpIndexRecursive(@out, filePath, childFiles);
				}
				else
				{
					@out.println(string.Format("  {0:X8} {1,10:D} {2}", fileStart, fileLength, filePath));
				}
			}
			return size;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void dumpIndexFile(String filename) throws java.io.IOException, java.io.FileNotFoundException
		public virtual void dumpIndexFile(string filename)
		{
			PrintWriter @out = new PrintWriter(new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write));
			@out.println("  Start    Size       Name");
			string[] files = listDirectory("");
			long totalSize = dumpIndexRecursive(@out, "", files);
			long imageSize = ((long) numSectors) * sectorLength;
			@out.println(string.Format("Total Size {0,10:D}", totalSize));
			@out.println(string.Format("Image Size {0,10:D}", imageSize));
			@out.println(string.Format("Missing    {0,10:D} ({1:D} sectors)", imageSize - totalSize, numSectors - (totalSize / sectorLength)));
			@out.close();
		}

		public static bool DoIsoBuffering
		{
			set
			{
				UmdIsoReader.doIsoBuffering = value;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private byte[] readFile(String fileName) throws java.io.IOException
		private sbyte[] readFile(string fileName)
		{
			if (!hasFile(fileName))
			{
				return null;
			}

			UmdIsoFile file = getFile(fileName);
			int Length = (int) file.Length();
			sbyte[] buffer = new sbyte[Length];
			int read = file.read(buffer);

			if (read < 0)
			{
				return null;
			}

			// Read less than expected?
			if (read < Length)
			{
				// Shrink the buffer to the read size
				sbyte[] newBuffer = new sbyte[read];
				Array.Copy(buffer, 0, newBuffer, 0, read);
				buffer = newBuffer;
			}

			return buffer;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readParamSFO() throws java.io.IOException
		public virtual sbyte[] readParamSFO()
		{
			if (browser != null)
			{
				return browser.readParamSFO();
			}
			return readFile("PSP_GAME/PARAM.SFO");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readIcon0() throws java.io.IOException
		public virtual sbyte[] readIcon0()
		{
			if (browser != null)
			{
				return browser.readIcon0();
			}
			return readFile("PSP_GAME/ICON0.PNG");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readIcon1() throws java.io.IOException
		public virtual sbyte[] readIcon1()
		{
			if (browser != null)
			{
				return browser.readIcon1();
			}
			return readFile("PSP_GAME/ICON1.PNG");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readPic0() throws java.io.IOException
		public virtual sbyte[] readPic0()
		{
			if (browser != null)
			{
				return browser.readPic0();
			}
			return readFile("PSP_GAME/PIC0.PNG");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readPic1() throws java.io.IOException
		public virtual sbyte[] readPic1()
		{
			if (browser != null)
			{
				return browser.readPic1();
			}
			return readFile("PSP_GAME/PIC1.PNG");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readSnd0() throws java.io.IOException
		public virtual sbyte[] readSnd0()
		{
			if (browser != null)
			{
				return browser.readSnd0();
			}
			return readFile("PSP_GAME/SND0.AT3");
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readPspData() throws java.io.IOException
		public virtual sbyte[] readPspData()
		{
			if (browser != null)
			{
				return browser.readPspData();
			}
			return null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte[] readPsarData() throws java.io.IOException
		public virtual sbyte[] readPsarData()
		{
			if (browser != null)
			{
				return browser.readPsarData();
			}
			return null;
		}

		public virtual bool PBP
		{
			get
			{
				return isPBP;
			}
		}
	}

}