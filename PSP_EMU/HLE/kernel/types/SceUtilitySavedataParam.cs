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
namespace pspsharp.HLE.kernel.types
{

	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using IVirtualFileSystem = pspsharp.HLE.VFS.IVirtualFileSystem;
	using CryptoEngine = pspsharp.crypto.CryptoEngine;
	using SeekableDataInput = pspsharp.filesystems.SeekableDataInput;
	using SeekableRandomFile = pspsharp.filesystems.SeekableRandomFile;
	using PNG = pspsharp.format.PNG;
	using PSF = pspsharp.format.PSF;
	using MemoryStick = pspsharp.hardware.MemoryStick;
	using Settings = pspsharp.settings.Settings;
	using Utilities = pspsharp.util.Utilities;

	public class SceUtilitySavedataParam : pspUtilityBaseDialog
	{

		// Value returned in "base.error" when the load/save has been cancelled by the user
		public const int ERROR_SAVEDATA_CANCELLED = 1;

		public const string savedataPath = "ms0:/PSP/SAVEDATA/";
		public static readonly string savedataFilePath = Settings.Instance.getDirectoryMapping("ms0") + "PSP/SAVEDATA/";
		public const string icon0FileName = "ICON0.PNG";
		public const string icon1PNGFileName = "ICON1.PNG";
		public const string icon1PMFFileName = "ICON1.PMF";
		public const string pic1FileName = "PIC1.PNG";
		public const string snd0FileName = "SND0.AT3";
		public const string paramSfoFileName = "PARAM.SFO";
		private static readonly string[] systemFileNames = new string[] {paramSfoFileName, icon0FileName, icon1PNGFileName, icon1PMFFileName, pic1FileName, snd0FileName};
		public const string anyFileName = "<>";
		public int mode;
		public const int MODE_AUTOLOAD = 0;
		public const int MODE_AUTOSAVE = 1;
		public const int MODE_LOAD = 2;
		public const int MODE_SAVE = 3;
		public const int MODE_LISTLOAD = 4;
		public const int MODE_LISTSAVE = 5;
		public const int MODE_LISTDELETE = 6;
		public const int MODE_DELETE = 7;
		public const int MODE_SIZES = 8;
		public const int MODE_AUTODELETE = 9;
		public const int MODE_SINGLEDELETE = 10;
		public const int MODE_LIST = 11;
		public const int MODE_FILES = 12;
		public const int MODE_MAKEDATASECURE = 13;
		public const int MODE_MAKEDATA = 14;
		public const int MODE_READSECURE = 15;
		public const int MODE_READ = 16;
		public const int MODE_WRITESECURE = 17;
		public const int MODE_WRITE = 18;
		public const int MODE_ERASESECURE = 19;
		public const int MODE_ERASE = 20;
		public const int MODE_DELETEDATA = 21;
		public const int MODE_GETSIZE = 22;
		public static readonly string[] modeNames = new string[]{"AUTOLOAD", "AUTOSAVE", "LOAD", "SAVE", "LISTLOAD", "LISTSAVE", "LISTDELETE", "DELETE", "SIZES", "AUTODELETE", "SINGLEDELETE", "LIST", "FILES", "MAKEDATASECURE", "MAKEDATA", "READSECURE", "READ", "WRITESECURE", "WRITE", "ERASESECURE", "ERASE", "DELETEDATA", "GETSIZE"};
		public int bind; // Used by certain applications to detect if this save data was created on a different PSP.
		public const int BIND_NOT_USED = 0;
		public const int BIND_IS_OK = 1;
		public const int BIND_IS_REJECTED = 2;
		public const int BIND_IS_NOT_SUPPORTED = 3;
		public bool overwrite;
		public string gameName; // name used from the game for saves, equal for all saves
		public string saveName; // name of the particular save, normally a number
		public string fileName; // name of the data file of the game for example DATA.BIN
		public string[] saveNameList; // used by multiple modes
		public int saveNameListAddr;
		public int dataBuf;
		public int dataBufSize;
		public int dataSize;
		public PspUtilitySavedataSFOParam sfoParam;
		public PspUtilitySavedataFileData icon0FileData;
		public PspUtilitySavedataFileData icon1FileData;
		public PspUtilitySavedataFileData pic1FileData;
		public PspUtilitySavedataFileData snd0FileData;
		internal int newDataAddr;
		public PspUtilitySavedataListSaveNewData newData;
		public int focus;
		public const int FOCUS_UNKNOWN = 0;
		public const int FOCUS_FIRSTLIST = 1; // First in list
		public const int FOCUS_LASTLIST = 2; // Last in list
		public const int FOCUS_LATEST = 3; // Most recent one
		public const int FOCUS_OLDEST = 4; // Oldest one
		public const int FOCUS_FIRSTEMPTY = 7; // First empty slot
		public const int FOCUS_LASTEMPTY = 8; // Last empty slot
		public int abortStatus; // Used by sceUtilityXXXAbort functions.
		public int msFreeAddr; // Address of a buffer to hold MemoryStick free size data (used in MODE_SIZES only).
		public int msDataAddr; // Address of a buffer to hold MemoryStick size data (used in MODE_SIZES only).
		public int utilityDataAddr; // Address of a buffer to hold utility size data (used in MODE_SIZES only).
		public sbyte[] key = new sbyte[0x10]; // Encrypt/decrypt key for saves with firmware >= 2.00.
		public int secureVersion; // 0 - Pre 2.00 (no encrypted files) / 1 - Post 2.00 (encrypted files are now used).
		public int multiStatus; // After 2.00, several modes can be triggered at the same time using this for sync.
		public const int MULTI_STATUS_SINGLE = 0; // Save data is all generated in one call.
		public const int MULTI_STATUS_INIT = 1; // Save data is generated in multiple calls and this is the first one.
		public const int MULTI_STATUS_RELAY = 2; // Save data is generated in multiple calls and this is an intermediate call.
		public const int MULTI_STATUS_FINISH = 3; // Save data is generated in multiple calls and this is the last one.
		public int idListAddr; // Address of a buffer to hold the file IDs generated by MODE_LIST.
		public int fileListAddr; // Address of a buffer to hold the file names generated by MODE_FILES.
		public int sizeAddr; // Address of a buffer to hold the sizes generated by MODE_GETSIZE.

		public class PspUtilitySavedataSFOParam : pspAbstractMemoryMappedStructure
		{

			public string title;
			public string savedataTitle;
			public string detail;
			public int parentalLevel;

			protected internal override void read()
			{
				title = readStringNZ(0x80);
				savedataTitle = readStringNZ(0x80);
				detail = readStringNZ(0x400);
				parentalLevel = read32();
			}

			protected internal override void write()
			{
				writeStringNZ(0x80, title);
				writeStringNZ(0x80, savedataTitle);
				writeStringNZ(0x400, detail);
				write32(parentalLevel);
			}

			public override int @sizeof()
			{
				return 0x80 + 0x80 + 0x400 + 4;
			}
		}

		public class PspUtilitySavedataFileData : pspAbstractMemoryMappedStructure
		{

			public int buf;
			public int bufSize;
			public int size;

			protected internal override void read()
			{
				buf = read32();
				bufSize = read32();
				size = read32();
				readUnknown(4);
			}

			protected internal override void write()
			{
				write32(buf);
				write32(bufSize);
				write32(size);
				writeUnknown(4);
			}

			public override int @sizeof()
			{
				return 4 * 4;
			}
		}

		public class PspUtilitySavedataListSaveNewData : pspAbstractMemoryMappedStructure
		{

			public PspUtilitySavedataFileData icon0;
			public int titleAddr;
			public string title;

			protected internal override void read()
			{
				icon0 = new PspUtilitySavedataFileData();
				read(icon0);
				titleAddr = read32();
				if (titleAddr != 0)
				{
					title = Utilities.readStringZ(mem, titleAddr);
				}
				else
				{
					title = null;
				}
			}

			protected internal override void write()
			{
				write(icon0);
				write32(titleAddr);
				if (titleAddr != 0)
				{
					Utilities.writeStringZ(mem, titleAddr, title);
				}
			}

			public override int @sizeof()
			{
				return icon0.@sizeof() + 4;
			}
		}

		public class PspUtilitySavedataSecureFile
		{

			public const int SIZEOF = 32;
			internal const int FILENAME_LENGTH = 13;
			public string fileName;
			public sbyte[] key = new sbyte[0x10];

			public PspUtilitySavedataSecureFile()
			{
			}

			public PspUtilitySavedataSecureFile(string fileName, sbyte[] key)
			{
				this.fileName = fileName;
				if (key != null)
				{
					Array.Copy(key, 0, this.key, 0, this.key.Length);
				}
			}

			public virtual void write(sbyte[] buffer, int offset)
			{
				sbyte[] fileNameBytes = fileName.GetBytes();
				Array.Copy(fileNameBytes, 0, buffer, offset, fileNameBytes.Length);
				if (fileNameBytes.Length < FILENAME_LENGTH)
				{
					Arrays.fill(buffer, offset + fileNameBytes.Length, offset + FILENAME_LENGTH, (sbyte) 0);
				}
				Array.Copy(key, 0, buffer, offset + FILENAME_LENGTH, key.Length);
			}

			public virtual bool read(sbyte[] buffer, int offset)
			{
				if (offset + PspUtilitySavedataSecureFile.SIZEOF > buffer.Length)
				{
					return false;
				}

				int fileNameLength = FILENAME_LENGTH;
				while (fileNameLength > 0)
				{
					if (buffer[offset + fileNameLength - 1] != (sbyte) 0)
					{
						break;
					}
					fileNameLength--;
				}
				if (fileNameLength <= 0)
				{
					return false;
				}
				fileName = StringHelper.NewString(buffer, offset, fileNameLength);
				Array.Copy(buffer, offset + FILENAME_LENGTH, key, 0, key.Length);

				return true;
			}
		}

		public class PspUtilitySavedataSecureFileList
		{

			public const int NUMBER_FILES = 99;
			public static readonly int SIZEOF = NUMBER_FILES * PspUtilitySavedataSecureFile.SIZEOF;
			public IList<PspUtilitySavedataSecureFile> fileList = new LinkedList<SceUtilitySavedataParam.PspUtilitySavedataSecureFile>();

			public virtual sbyte[] Bytes
			{
				get
				{
					sbyte[] bytes = new sbyte[SIZEOF];
					int offset = 0;
					foreach (PspUtilitySavedataSecureFile file in fileList)
					{
						file.write(bytes, offset);
						offset += PspUtilitySavedataSecureFile.SIZEOF;
					}
    
					return bytes;
				}
			}

			public virtual void read(sbyte[] buffer)
			{
				fileList.Clear();
				for (int offset = 0; offset < PspUtilitySavedataSecureFileList.SIZEOF; offset += PspUtilitySavedataSecureFile.SIZEOF)
				{
					PspUtilitySavedataSecureFile file = new PspUtilitySavedataSecureFile();
					if (!file.read(buffer, offset))
					{
						break;
					}
					fileList.Add(file);
				}
			}

			public virtual bool contains(string fileName)
			{
				foreach (PspUtilitySavedataSecureFile file in fileList)
				{
					if (file.fileName.Equals(fileName))
					{
						return true;
					}
				}

				return false;
			}

			public virtual void add(string fileName, sbyte[] key)
			{
				if (contains(fileName))
				{
					return;
				}

				PspUtilitySavedataSecureFile file = new PspUtilitySavedataSecureFile(fileName, key);
				fileList.Add(file);
			}

			public virtual void update(string fileName, sbyte[] key)
			{
				bool found = false;
				foreach (PspUtilitySavedataSecureFile file in fileList)
				{
					if (file.fileName.Equals(fileName))
					{
						file.key = key;
						found = true;
					}
				}

				if (!found)
				{
					PspUtilitySavedataSecureFile file = new PspUtilitySavedataSecureFile(fileName, key);
					fileList.Add(file);
				}
			}
		}

		protected internal override void read()
		{
			@base = new pspUtilityDialogCommon();
			read(@base);
			MaxSize = @base.totalSizeof();

			mode = read32(); // Offset 48
			bind = read32(); // Offset 52
			overwrite = read32() == 0 ? false : true; // Offset 56
			gameName = readStringNZ(13); // Offset 60
			readUnknown(3);
			saveName = readStringNZ(20); // Offset 76
			saveNameListAddr = read32(); // Offset 96
			if (Memory.isAddressGood(saveNameListAddr))
			{
				IList<string> newSaveNameList = new List<string>();
				bool endOfList = false;
				for (int i = 0; !endOfList; i += 20)
				{
					string saveNameItem = Utilities.readStringNZ(mem, saveNameListAddr + i, 20);
					if (string.ReferenceEquals(saveNameItem, null) || saveNameItem.Length == 0)
					{
						endOfList = true;
					}
					else
					{
						newSaveNameList.Add(saveNameItem);
					}
				}
				saveNameList = newSaveNameList.ToArray();
			}
			fileName = readStringNZ(13); // Offset 100
			readUnknown(3);
			dataBuf = read32(); // Offset 116
			dataBufSize = read32(); // Offset 120
			dataSize = read32(); // Offset 124

			sfoParam = new PspUtilitySavedataSFOParam();
			read(sfoParam); // Offset 128
			icon0FileData = new PspUtilitySavedataFileData();
			read(icon0FileData); // Offset 1412
			icon1FileData = new PspUtilitySavedataFileData();
			read(icon1FileData); // Offset 1428
			pic1FileData = new PspUtilitySavedataFileData();
			read(pic1FileData); // Offset 1444
			snd0FileData = new PspUtilitySavedataFileData();
			read(snd0FileData); // Offset 1460

			newDataAddr = read32(); // Offset 1476
			if (newDataAddr != 0)
			{
				newData = new PspUtilitySavedataListSaveNewData();
				newData.read(mem, newDataAddr);
			}
			else
			{
				newData = null;
			}
			focus = read32(); // Offset 1480
			abortStatus = read32(); // Offset 1484
			msFreeAddr = read32(); // Offset 1488
			msDataAddr = read32(); // Offset 1492
			utilityDataAddr = read32(); // Offset 1496
			read8Array(key); // Offset 1500
			secureVersion = read32(); // Offset 1516
			multiStatus = read32(); // Offset 1520
			idListAddr = read32(); // Offset 1524
			fileListAddr = read32(); // Offset 1528
			sizeAddr = read32(); // Offset 1532
		}

		protected internal override void write()
		{
			write(@base);
			MaxSize = @base.totalSizeof();

			write32(mode);
			write32(bind);
			write32(overwrite ? 1 : 0);
			writeStringNZ(13, gameName);
			writeUnknown(3);
			writeStringNZ(20, saveName);
			write32(saveNameListAddr);
			writeStringNZ(13, fileName);
			writeUnknown(3);
			write32(dataBuf);
			write32(dataBufSize);
			write32(dataSize);

			write(sfoParam);
			write(icon0FileData);
			write(icon1FileData);
			write(pic1FileData);
			write(snd0FileData);

			write32(newDataAddr);
			if (newDataAddr != 0)
			{
				newData.write(mem, newDataAddr);
			}
			write32(focus);
			write32(abortStatus);
			write32(msFreeAddr);
			write32(msDataAddr);
			write32(utilityDataAddr);
			write8Array(key);
			write32(secureVersion);
			write32(multiStatus);
			write32(idListAddr);
			write32(fileListAddr);
			write32(sizeAddr);
		}

		public virtual string BasePath
		{
			get
			{
				return getBasePath(gameName, saveName);
			}
		}

		public virtual string getBasePath(string saveName)
		{
			return getBasePath(gameName, saveName);
		}

		public virtual string getBasePath(string gameName, string saveName)
		{
			string path = savedataPath + gameName;
			if (!string.ReferenceEquals(saveName, null) && !anyFileName.Equals(saveName))
			{
				path += saveName;
			}
			path += "/";
			return path;
		}

		public virtual string getFileName(string saveName, string fileName)
		{
			return getFileName(gameName, saveName, fileName);
		}

		public virtual string getFileName(string gameName, string saveName, string fileName)
		{
			return getBasePath(gameName, saveName) + fileName;
		}

		private static int computeMemoryStickRequiredSpaceKb(int sizeByte)
		{
			int sizeKb = Utilities.getSizeKb(sizeByte);
			int sectorSizeKb = MemoryStick.SectorSizeKb;
			int numberSectors = (sizeKb + sectorSizeKb - 1) / sectorSizeKb;

			return numberSectors * sectorSizeKb;
		}

		public virtual int RequiredSizeKb
		{
			get
			{
				int requiredSpaceKb = 0;
				requiredSpaceKb += MemoryStick.SectorSizeKb; // Assume 1 sector for SFO-Params
				// Add the dataSize only if a fileName has been provided
				if (!string.ReferenceEquals(fileName, null) && fileName.Length > 0)
				{
					requiredSpaceKb += computeMemoryStickRequiredSpaceKb(dataSize + 15);
				}
				requiredSpaceKb += computeMemoryStickRequiredSpaceKb(icon0FileData.size);
				requiredSpaceKb += computeMemoryStickRequiredSpaceKb(icon1FileData.size);
				requiredSpaceKb += computeMemoryStickRequiredSpaceKb(pic1FileData.size);
				requiredSpaceKb += computeMemoryStickRequiredSpaceKb(snd0FileData.size);
    
				return requiredSpaceKb;
			}
		}

		public virtual int getSizeKb(string gameName, string saveName)
		{
			int sizeKb = 0;

			string path = getBasePath(gameName, saveName);
			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = Modules.IoFileMgrForUserModule.getVirtualFileSystem(path, localFileName);
			if (vfs != null)
			{
				string[] fileNames = vfs.ioDopen(localFileName.ToString());
				if (fileNames != null)
				{
					for (int i = 0; i < fileNames.Length; i++)
					{
						SceIoStat stat = new SceIoStat();
						SceIoDirent dirent = new SceIoDirent(stat, fileNames[i]);
						int result = vfs.ioDread(localFileName.ToString(), dirent);
						if (result > 0)
						{
							sizeKb += Utilities.getSizeKb((int) stat.size);
						}
					}
					vfs.ioDclose(localFileName.ToString());
				}
			}

			return sizeKb;
		}

		private SeekableDataInput getDataInput(string path, string name)
		{
			SeekableDataInput fileInput = Modules.IoFileMgrForUserModule.getFile(path + name, pspsharp.HLE.modules.IoFileMgrForUser.PSP_O_RDONLY);

			return fileInput;
		}

		private SeekableRandomFile getDataOutput(string path, string name)
		{
			SeekableDataInput fileInput = Modules.IoFileMgrForUserModule.getFile(path + name, pspsharp.HLE.modules.IoFileMgrForUser.PSP_O_RDWR | pspsharp.HLE.modules.IoFileMgrForUser.PSP_O_CREAT);

			if (fileInput is SeekableRandomFile)
			{
				return (SeekableRandomFile) fileInput;
			}

			return null;
		}

		public virtual bool deleteDir(string path)
		{
			return Modules.IoFileMgrForUserModule.rmdir(path, true);
		}

		public virtual bool deleteFile(string filename)
		{
			bool success = false;
			if (!string.ReferenceEquals(filename, null) && filename.Length > 0)
			{
				File f = new File(BasePath.Replace(":", "/") + filename);
				success = f.delete();
			}
			return success;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void load(pspsharp.Memory mem) throws java.io.IOException
		public virtual void load(Memory mem)
		{
			string path = BasePath;

			// Read the main data.
			// The data has to be decrypted if the SFO is marked for encryption and
			// the file is listed in the SFO as a secure file (SAVEDATA_FILE_LIST).
			if (checkParamSFOEncryption(path, paramSfoFileName) && isSecureFile(fileName))
			{
				dataSize = loadEncryptedFile(mem, path, fileName, dataBuf, dataBufSize, key);
			}
			else
			{
				dataSize = loadFile(mem, path, fileName, dataBuf, dataBufSize);
			}

			// Read ICON0.PNG
			safeLoad(mem, icon0FileName, icon0FileData);

			// Check and read ICON1.PMF or ICON1.PNG
			if (icon1FileData.buf == 0)
			{
				icon1FileData.size = 0;
			}
			else
			{
				string icon1FileName;
				if (mem.read8(icon1FileData.buf) != 0x89)
				{
					icon1FileName = icon1PMFFileName;
				}
				else
				{
					icon1FileName = icon1PNGFileName;
				}
				safeLoad(mem, icon1FileName, icon1FileData);
			}

			// Read PIC1.PNG
			safeLoad(mem, pic1FileName, pic1FileData);

			// Read SND0.AT3
			safeLoad(mem, snd0FileName, snd0FileData);

			// Read PARAM.SFO
			loadPsf(mem, path, paramSfoFileName, sfoParam);

			bind = BIND_IS_OK;
			abortStatus = 0;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void safeLoad(pspsharp.Memory mem, String filename, PspUtilitySavedataFileData fileData) throws java.io.IOException
		private void safeLoad(Memory mem, string filename, PspUtilitySavedataFileData fileData)
		{
			string path = BasePath;

			try
			{
				fileData.size = loadFile(mem, path, filename, fileData.buf, fileData.bufSize);
			}
			catch (FileNotFoundException)
			{
				// ignore
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void save(pspsharp.Memory mem, boolean secure) throws java.io.IOException
		public virtual void save(Memory mem, bool secure)
		{
			string path = BasePath;

			Modules.IoFileMgrForUserModule.mkdirs(path);

			// Copy the original SAVEDATA key.
			sbyte[] sdkey = key;

			// Write main data.
			if (CryptoEngine.SavedataCryptoStatus && secure)
			{
				if (CryptoEngine.ExtractSavedataKeyStatus)
				{
					string tmpPath = Settings.Instance.DiscTmpDirectory;
					System.IO.Directory.CreateDirectory(tmpPath);
					SeekableRandomFile keyFileOutput = new SeekableRandomFile(tmpPath + "SDKEY.bin", "rw");
					keyFileOutput.write(sdkey, 0, sdkey.Length);
					keyFileOutput.Dispose();
				}
				writeEncryptedFile(mem, path, fileName, dataBuf, dataSize, key);
			}
			else
			{
				writeFile(mem, path, fileName, dataBuf, dataSize);
			}

			// Write ICON0.PNG
			writePNG(mem, path, icon0FileName, icon0FileData.buf, icon0FileData.size);

			// Check and write ICON1.PMF or ICON1.PNG
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(icon1FileData.buf, 1);
			string icon1FileName;
			if ((sbyte) memoryReader.readNext() != unchecked((sbyte) 0x89))
			{
				icon1FileName = icon1PMFFileName;
			}
			else
			{
				icon1FileName = icon1PNGFileName;
			}
			writePNG(mem, path, icon1FileName, icon1FileData.buf, icon1FileData.size);

			// Write PIC1.PNG
			writePNG(mem, path, pic1FileName, pic1FileData.buf, pic1FileData.size);

			// Write SND0.AT3
			writeFile(mem, path, snd0FileName, snd0FileData.buf, snd0FileData.size);

			// Write PARAM.SFO
			writePsf(mem, path, paramSfoFileName, sfoParam, CryptoEngine.SavedataCryptoStatus, fileName, sdkey, key);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int loadFile(pspsharp.Memory mem, String path, String name, int address, int maxLength) throws java.io.IOException
		private int loadFile(Memory mem, string path, string name, int address, int maxLength)
		{
			if (string.ReferenceEquals(name, null) || name.Length <= 0)
			{
				return 0;
			}

			int fileSize = 0;
			SeekableDataInput fileInput = null;
			try
			{
				fileInput = getDataInput(path, name);
				if (fileInput == null)
				{
					throw new FileNotFoundException("File not found '" + path + "' '" + name + "'");
				}

				// Some applications set dataBufSize to -1 on purpose. The reason behind this
				// is still unknown, but, for these cases, ignore maxLength.
				fileSize = (int) fileInput.length();
				if (fileSize > maxLength && maxLength > 0)
				{
					fileSize = maxLength;
				}
				else if (address == 0)
				{
					fileSize = 0;
				}

				Utilities.readFully(fileInput, address, fileSize);
			}
			finally
			{
				if (fileInput != null)
				{
					fileInput.Dispose();
				}
			}

			return fileSize;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeEncryptedFile(pspsharp.Memory mem, String path, String name, int address, int length, byte[] key) throws java.io.IOException
		private void writeEncryptedFile(Memory mem, string path, string name, int address, int length, sbyte[] key)
		{
			if (string.ReferenceEquals(name, null) || name.Length <= 0 || address == 0)
			{
				return;
			}

			SeekableRandomFile fileOutput = null;
			try
			{
				fileOutput = getDataOutput(path, name);
				if (fileOutput != null)
				{
					sbyte[] inBuf = new sbyte[length + 0x10];

					IMemoryReader memoryReader = MemoryReader.getMemoryReader(address, 1);
					for (int i = 0; i < length; i++)
					{
						inBuf[i] = (sbyte) memoryReader.readNext();
					}

					// Replace the key with the generated hash.
					CryptoEngine crypto = new CryptoEngine();
					this.key = crypto.SAVEDATAEngine.EncryptSavedata(inBuf, length, key);

					fileOutput.Channel.truncate(inBuf.Length); // Avoid writing leftover bytes from previous encryption.
					fileOutput.write(inBuf, 0, inBuf.Length);
				}
			}
			finally
			{
				if (fileOutput != null)
				{
					fileOutput.Dispose();
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int loadEncryptedFile(pspsharp.Memory mem, String path, String name, int address, int maxLength, byte[] key) throws java.io.IOException
		private int loadEncryptedFile(Memory mem, string path, string name, int address, int maxLength, sbyte[] key)
		{
			if (string.ReferenceEquals(name, null) || name.Length <= 0 || address == 0)
			{
				return 0;
			}

			int length = 0;
			SeekableDataInput fileInput = null;
			try
			{
				fileInput = getDataInput(path, name);
				if (fileInput == null)
				{
					throw new FileNotFoundException("File not found '" + path + "' '" + name + "'");
				}

				int fileSize = (int) fileInput.length();
				sbyte[] inBuf = new sbyte[fileSize];
				fileInput.readFully(inBuf);

				CryptoEngine crypto = new CryptoEngine();
				sbyte[] outBuf = crypto.SAVEDATAEngine.DecryptSavedata(inBuf, fileSize, key);

				IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(address, 1);
				length = System.Math.Min(outBuf.Length, maxLength);
				for (int i = 0; i < length; i++)
				{
					memoryWriter.writeNext(outBuf[i]);
				}
				memoryWriter.flush();
			}
			finally
			{
				if (fileInput != null)
				{
					fileInput.Dispose();
				}
			}

			return length;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writeFile(pspsharp.Memory mem, String path, String name, int address, int length) throws java.io.IOException
		private void writeFile(Memory mem, string path, string name, int address, int length)
		{
			if (string.ReferenceEquals(name, null) || name.Length <= 0 || address == 0)
			{
				return;
			}

			SeekableRandomFile fileOutput = null;
			try
			{
				fileOutput = getDataOutput(path, name);
				if (fileOutput != null)
				{
					fileOutput.Channel.truncate(length); // Avoid writing leftover bytes from previous encryption.
					Utilities.write(fileOutput, address, length);
				}
			}
			finally
			{
				if (fileOutput != null)
				{
					fileOutput.Dispose();
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writePNG(pspsharp.Memory mem, String path, String name, int address, int length) throws java.io.IOException
		private void writePNG(Memory mem, string path, string name, int address, int length)
		{
			// The PSP is saving only the real size of the PNG file,
			// which could be smaller than the buffer size
			length = PNG.getEndOfPNG(mem, address, length);

			writeFile(mem, path, name, address, length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean checkParamSFOEncryption(String path, String name) throws java.io.IOException
		private bool checkParamSFOEncryption(string path, string name)
		{
			bool isEncrypted = false;
			SeekableDataInput fileInput = null;
			try
			{
				fileInput = getDataInput(path, name);
				if (fileInput != null && fileInput.length() > 0)
				{
					sbyte[] buffer = new sbyte[(int) fileInput.length()];
					fileInput.readFully(buffer);
					fileInput.Dispose();

					// SAVEDATA PARAM.SFO has a fixed size of 0x1330 bytes.
					// In order to determine if the SAVEDATA is encrypted or not,
					// we must check if the check bit at 0x11B0 is set (an identical check
					// is performed on a real PSP).
					if (buffer.Length == 0x1330)
					{
						if (buffer[0x11B0] != 0)
						{
							isEncrypted = true;
						}
					}
				}
			}
			finally
			{
				if (fileInput != null)
				{
					fileInput.Dispose();
				}
			}

			return isEncrypted;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private pspsharp.format.PSF readPsf(String path, String name) throws java.io.IOException
		private PSF readPsf(string path, string name)
		{
			PSF psf = null;
			SeekableDataInput fileInput = null;
			try
			{
				fileInput = getDataInput(path, name);
				if (fileInput != null && fileInput.length() > 0)
				{
					sbyte[] buffer = new sbyte[(int) fileInput.length()];
					fileInput.readFully(buffer);
					fileInput.Dispose();

					psf = new PSF();
					psf.read(ByteBuffer.wrap(buffer));
				}
			}
			finally
			{
				if (fileInput != null)
				{
					fileInput.Dispose();
				}
			}

			return psf;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void loadPsf(pspsharp.Memory mem, String path, String name, PspUtilitySavedataSFOParam sfoParam) throws java.io.IOException
		private void loadPsf(Memory mem, string path, string name, PspUtilitySavedataSFOParam sfoParam)
		{
			PSF psf = readPsf(path, name);
			if (psf != null)
			{
				sfoParam.parentalLevel = psf.getNumeric("PARENTAL_LEVEL");
				sfoParam.title = psf.getString("TITLE");
				sfoParam.detail = psf.getString("SAVEDATA_DETAIL");
				sfoParam.savedataTitle = psf.getString("SAVEDATA_TITLE");
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void writePsf(pspsharp.Memory mem, String path, String psfName, PspUtilitySavedataSFOParam sfoParam, boolean cryptoMode, String dataName, byte[] key, byte[] hash) throws java.io.IOException
		private void writePsf(Memory mem, string path, string psfName, PspUtilitySavedataSFOParam sfoParam, bool cryptoMode, string dataName, sbyte[] key, sbyte[] hash)
		{
			SeekableRandomFile psfOutput = null;
			try
			{
				psfOutput = getDataOutput(path, psfName);
				if (psfOutput == null)
				{
					return;
				}

				// Generate different PSF instances for plain PSF and encrypted PSF (with hashes).
				PSF psf = new PSF();
				PSF encryptedPsf = new PSF();

				// Test if a PARAM.SFO already exists and save it's SAVEDATA_PARAMS.
				sbyte[] savedata_params_old = new sbyte[128];
				PSF oldPsf = readPsf(path, psfName);
				if (oldPsf != null)
				{
					savedata_params_old = (sbyte[]) oldPsf.get("SAVEDATA_PARAMS");
				}

				// Insert CATEGORY.
				psf.put("CATEGORY", "MS", 4);
				encryptedPsf.put("CATEGORY", "MS", 4);

				// Insert PARENTAL_LEVEL.
				psf.put("PARENTAL_LEVEL", sfoParam.parentalLevel);
				encryptedPsf.put("PARENTAL_LEVEL", sfoParam.parentalLevel);

				// Insert SAVEDATA_DETAIL.
				psf.put("SAVEDATA_DETAIL", sfoParam.detail, 1024);
				encryptedPsf.put("SAVEDATA_DETAIL", sfoParam.detail, 1024);

				// Insert SAVEDATA_DIRECTORY.
				if (saveName.Equals("<>"))
				{
					// Do not write the saveName if it's "<>".
					psf.put("SAVEDATA_DIRECTORY", gameName, 64);
					encryptedPsf.put("SAVEDATA_DIRECTORY", gameName, 64);
				}
				else
				{
					psf.put("SAVEDATA_DIRECTORY", gameName + saveName, 64);
					encryptedPsf.put("SAVEDATA_DIRECTORY", gameName + saveName, 64);
				}

				// Insert SAVEDATA_FILE_LIST.
				PspUtilitySavedataSecureFileList secureFileList = getSecureFileList(null);
				// Even if the main data file is not being saved by a secure method, if the
				// hash is not null then the file is saved in the file list.
				if (hash != null)
				{
					// Add the current dataName as a secure file name
					if (secureFileList == null)
					{
						secureFileList = new PspUtilitySavedataSecureFileList();
					}
					// Only add the file hash if using encryption.
					if (cryptoMode)
					{
						secureFileList.update(dataName, hash);
					}
					else
					{
						sbyte[] clearHash = new sbyte[0x10];
						secureFileList.update(dataName, clearHash);
					}
				}
				if (secureFileList != null)
				{
					psf.put("SAVEDATA_FILE_LIST", secureFileList.Bytes);
					encryptedPsf.put("SAVEDATA_FILE_LIST", secureFileList.Bytes);
				}

				// Generate blank SAVEDATA_PARAMS for plain PSF.
				sbyte[] savedata_params = new sbyte[128];
				psf.put("SAVEDATA_PARAMS", savedata_params);

				// Insert the remaining params for plain PSF.
				psf.put("SAVEDATA_TITLE", sfoParam.savedataTitle, 128);
				psf.put("TITLE", sfoParam.title, 128);

				// Setup a temporary buffer for encryption (PARAM.SFO size is 0x1330).
				ByteBuffer buf = ByteBuffer.allocate(0x1330);

				// Save back the PARAM.SFO data to be encrypted.
				psf.write(buf);

				// Generate a new PARAM.SFO and update file hashes.
				if (cryptoMode)
				{
					CryptoEngine crypto = new CryptoEngine();
					int sfoSize = buf.array().length;
					sbyte[] sfoData = buf.array();

					// Generate the final SAVEDATA_PARAMS (encrypted).
					crypto.SAVEDATAEngine.UpdateSavedataHashes(encryptedPsf, sfoData, sfoSize, savedata_params_old, key);

					// Insert the remaining params for encrypted PSF.
					encryptedPsf.put("SAVEDATA_TITLE", sfoParam.savedataTitle, 128);
					encryptedPsf.put("TITLE", sfoParam.title, 128);

					// Write the new encrypted PARAM.SFO (with hashes) from the encrypted PSF.
					encryptedPsf.write(psfOutput);
				}
				else
				{
					// Write the new PARAM.SFO (without hashes) from the plain PSF.
					psf.write(psfOutput);
				}
			}
			finally
			{
				if (psfOutput != null)
				{
					// Close the PARAM.SFO file stream.
					psfOutput.Dispose();
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean test(pspsharp.Memory mem) throws java.io.IOException
		public virtual bool test(Memory mem)
		{
			string path = BasePath;

			bool result = testFile(mem, path, fileName);

			return result;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean testFile(pspsharp.Memory mem, String path, String name) throws java.io.IOException
		private bool testFile(Memory mem, string path, string name)
		{
			if (string.ReferenceEquals(name, null) || name.Length <= 0)
			{
				return false;
			}

			SeekableDataInput fileInput = null;
			try
			{
				fileInput = getDataInput(path, name);
				if (fileInput == null)
				{
					throw new FileNotFoundException("File not found '" + path + "' '" + name + "'");
				}
			}
			finally
			{
				if (fileInput != null)
				{
					fileInput.Dispose();
				}
			}

			return true;
		}

		public virtual string getAnySaveName(string gameName, string saveName)
		{
			// NULL can also be sent in saveName (seen in MODE_SIZES).
			// It means any save from the current game, since all saves share a common
			// save data file.
			if (string.ReferenceEquals(saveName, null) || saveName.Length <= 0 || anyFileName.Equals(saveName))
			{
				File f = new File(savedataFilePath);
				string[] entries = f.list();
				if (entries != null)
				{
					for (int i = 0; i < f.list().length; i++)
					{
						if (entries[i].StartsWith(gameName, StringComparison.Ordinal))
						{
							saveName = entries[i].Replace(gameName, "");
							break;
						}
					}
				}
			}

			return saveName;
		}

		public virtual bool isDirectoryPresent(string gameName, string saveName)
		{
			saveName = getAnySaveName(gameName, saveName);
			string path = getBasePath(gameName, saveName);
			SceIoStat stat = Modules.IoFileMgrForUserModule.statFile(path);
			if (stat != null && (stat.attr & 0x20) == 0)
			{
				return true;
			}

			return false;
		}

		public virtual bool isPresent(string gameName, string saveName)
		{
			saveName = getAnySaveName(gameName, saveName);

			// When NULL is sent in fileName, it means any file inside the savedata folder.
			if (string.ReferenceEquals(fileName, null) || fileName.Length <= 0)
			{
				File f = new File(savedataFilePath + gameName + saveName);
				if (f.list() == null)
				{
					return false;
				}
				return true;
			}

			string path = getBasePath(gameName, saveName);
			try
			{
				SeekableDataInput fileInput = getDataInput(path, fileName);
				if (fileInput != null)
				{
					fileInput.Dispose();
					return true;
				}
			}
			catch (IOException)
			{
			}

			return false;
		}

		public virtual bool Present
		{
			get
			{
				return isPresent(gameName, saveName);
			}
		}

		public virtual bool GameDirectoryPresent
		{
			get
			{
				string path = BasePath;
				SceIoStat gameDirectoryStat = Modules.IoFileMgrForUserModule.statFile(path);
    
				return gameDirectoryStat != null;
			}
		}

		public virtual long getTimestamp(string gameName, string saveName)
		{
			string sfoFileName = getFileName(gameName, saveName, paramSfoFileName);
			SceIoStat sfoStat = Modules.IoFileMgrForUserModule.statFile(sfoFileName);
			if (sfoStat != null)
			{
				DateTime cal = new DateTime();
				ScePspDateTime pspTime = sfoStat.mtime;
				cal = new DateTime(pspTime.year, pspTime.month, pspTime.day, pspTime.hour, pspTime.minute, pspTime.second);
				return cal.Ticks;
			}

			return 0;
		}

		public virtual DateTime SavedTime
		{
			get
			{
				return getSavedTime(saveName);
			}
		}

		public virtual DateTime getSavedTime(string saveName)
		{
			string sfoFileName = getFileName(saveName, SceUtilitySavedataParam.paramSfoFileName);
			SceIoStat sfoStat = Modules.IoFileMgrForUserModule.statFile(sfoFileName);
			if (sfoStat == null)
			{
				return null;
			}

			ScePspDateTime pspTime = sfoStat.mtime;

			DateTime savedTime = new DateTime();
			// pspTime.month has a value in range [1..12], Calendar requires a value in range [0..11]
			savedTime = new DateTime(pspTime.year, pspTime.month - 1, pspTime.day, pspTime.hour, pspTime.minute, pspTime.second);

			return savedTime;
		}

		public override int @sizeof()
		{
			return @base.totalSizeof();
		}

		public virtual string ModeName
		{
			get
			{
				if (mode < 0 || mode >= modeNames.Length)
				{
					return string.Format("UNKNOWN_MODE{0:D}", mode);
				}
				return modeNames[mode];
			}
		}

		public static bool isSystemFile(string fileName)
		{
			for (int i = 0; i < systemFileNames.Length; i++)
			{
				if (systemFileNames[i].Equals(fileName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private PspUtilitySavedataSecureFileList getSecureFileList(string fileName)
		{
			PSF psf = null;
			try
			{
				psf = readPsf(BasePath, paramSfoFileName);
			}
			catch (IOException)
			{
			}

			if (psf == null)
			{
				return null;
			}

			object savedataFileList = psf.get("SAVEDATA_FILE_LIST");
			if (savedataFileList == null || !(savedataFileList is sbyte[]))
			{
				return null;
			}

			PspUtilitySavedataSecureFileList fileList = null;
			if (savedataFileList != null)
			{
				fileList = new PspUtilitySavedataSecureFileList();
				fileList.read((sbyte[]) savedataFileList);
			}

			return fileList;
		}

		public virtual bool isSecureFile(string fileName)
		{
			PspUtilitySavedataSecureFileList fileList = getSecureFileList(fileName);
			if (fileList == null)
			{
				return false;
			}

			return fileList.contains(fileName);
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			s.Append(string.Format("Address 0x{0:X8}, mode={1:D}({2}), gameName={3}, saveName={4}, fileName={5}, secureVersion={6:D}", BaseAddress, mode, ModeName, gameName, saveName, fileName, secureVersion));
			for (int i = 0; saveNameList != null && i < saveNameList.Length; i++)
			{
				if (i == 0)
				{
					s.Append(", saveNameList=[");
				}
				else
				{
					s.Append(", ");
				}
				s.Append(saveNameList[i]);
				if (i == saveNameList.Length - 1)
				{
					s.Append("]");
				}
			}

			return s.ToString();
		}
	}

}