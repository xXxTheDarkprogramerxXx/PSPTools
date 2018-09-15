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
namespace pspsharp.HLE.VFS.local
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_O_CREAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_O_EXCL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_O_RDWR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_O_TRUNC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_O_WRONLY;


	using SceIoDirent = pspsharp.HLE.kernel.types.SceIoDirent;
	using SceIoStat = pspsharp.HLE.kernel.types.SceIoStat;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ScePspDateTime = pspsharp.HLE.kernel.types.ScePspDateTime;
	using IoFileMgrForUser = pspsharp.HLE.modules.IoFileMgrForUser;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;
	using IoOperation = pspsharp.HLE.modules.IoFileMgrForUser.IoOperation;
	using IoOperationTiming = pspsharp.HLE.modules.IoFileMgrForUser.IoOperationTiming;
	using SeekableRandomFile = pspsharp.filesystems.SeekableRandomFile;
	using MemoryStick = pspsharp.hardware.MemoryStick;

	public class LocalVirtualFileSystem : AbstractVirtualFileSystem
	{
		protected internal readonly string localPath;
		private readonly bool useDirExtendedInfo;
		// modeStrings indexed by [0, PSP_O_RDONLY, PSP_O_WRONLY, PSP_O_RDWR]
		// SeekableRandomFile doesn't support write only: take "rw",
		private static readonly string[] modeStrings = new string[] {"r", "r", "rw", "rw"};

		/// <summary>
		/// Get the file name as returned from the memory stick.
		/// In some cases, the name is uppercased.
		/// 
		/// The following cases have been tested:
		/// - "a"                => "A"
		/// - "B"                => "B"
		/// - "b.txt"            => "B.TXT"
		/// - "cC"               => "cC"
		/// - "LongFileName.txt" => "LongFileName.txt"
		/// - "aaaaaaaa"         => "AAAAAAAA"
		/// - "aaaaaaaa.aaa"     => "AAAAAAAA.AAA"
		/// - "aaaaaaaaa"        => "aaaaaaaaa"
		/// - "aaaaaaaa.aaaa"    => "aaaaaaaa.aaaa"
		/// 
		/// It seems that file names in the format 8.3 only containing lowercase characters
		/// are converted to uppercase characters.
		/// </summary>
		public static string getMsFileName(string fileName)
		{
			if (string.ReferenceEquals(fileName, null))
			{
				return fileName;
			}
			if (fileName.matches("[^A-Z]{1,8}(\\.[^A-Z]{1,3})?"))
			{
				return fileName.ToUpper();
			}
			return fileName;
		}

		public LocalVirtualFileSystem(string localPath, bool useDirExtendedInfo)
		{
			this.localPath = localPath;
			this.useDirExtendedInfo = useDirExtendedInfo;
		}

		protected internal virtual File getFile(string fileName)
		{
			return new File(string.ReferenceEquals(fileName, null) ? localPath : localPath + fileName);
		}

		protected internal static string getMode(int mode)
		{
			return modeStrings[mode & PSP_O_RDWR];
		}

		public override IVirtualFile ioOpen(string fileName, int flags, int mode)
		{
			File file = getFile(fileName);
			if (file.exists() && hasFlag(flags, PSP_O_CREAT) && hasFlag(flags, PSP_O_EXCL))
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine("hleIoOpen - file already exists (PSP_O_CREAT + PSP_O_EXCL)");
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_ERRNO_FILE_ALREADY_EXISTS);
			}

			// When PSP_O_CREAT is specified, create the parent directories
			// if they do not yet exist.
			if (!file.exists() && hasFlag(flags, PSP_O_CREAT))
			{
				string parentDir = file.Parent;
				System.IO.Directory.CreateDirectory(parentDir);
			}

			SeekableRandomFile raf;
			try
			{
				raf = new SeekableRandomFile(file, getMode(flags));
			}
			catch (FileNotFoundException)
			{
				return null;
			}

			LocalVirtualFile localVirtualFile = new LocalVirtualFile(raf);

			if (hasFlag(flags, PSP_O_WRONLY) && hasFlag(flags, PSP_O_TRUNC))
			{
				// When writing, PSP_O_TRUNC truncates the file at the position of the first write.
				// E.g.:
				//    open(PSP_O_TRUNC)
				//    seek(0x1000)
				//    write()  -> truncates the file at the position 0x1000 before writing
				localVirtualFile.TruncateAtNextWrite = true;
			}

			return localVirtualFile;
		}

		public override int ioGetstat(string fileName, SceIoStat stat)
		{
			File file = getFile(fileName);
			if (!file.exists())
			{
				return SceKernelErrors.ERROR_ERRNO_FILE_NOT_FOUND;
			}

			// Set attr (dir/file) and copy into mode
			int attr = 0;
			if (file.Directory)
			{
				attr |= 0x10;
			}
			if (file.File)
			{
				attr |= 0x20;
			}

			int mode = (file.canRead() ? 4 : 0) + (file.canWrite() ? 2 : 0) + (file.canExecute() ? 1 : 0);
			// Octal extend into user and group
			mode = mode + (mode << 3) + (mode << 6);
			mode |= attr << 8;

			// Java can't see file create/access time
			ScePspDateTime ctime = ScePspDateTime.fromUnixTime(file.lastModified());
			ScePspDateTime atime = ScePspDateTime.fromUnixTime(0);
			ScePspDateTime mtime = ScePspDateTime.fromUnixTime(file.lastModified());

			stat.init(mode, attr, file.Length(), ctime, atime, mtime);

			return 0;
		}

		public override int ioRemove(string name)
		{
			File file = getFile(name);

			if (!file.delete())
			{
				return IO_ERROR;
			}

			return 0;
		}

		public override string[] ioDopen(string dirName)
		{
			File file = getFile(dirName);

			if (!file.Directory)
			{
				if (file.exists())
				{
					Console.WriteLine(string.Format("ioDopen file '{0}' is not a directory", dirName));
				}
				else
				{
					Console.WriteLine(string.Format("ioDopen directory '{0}' not found", dirName));
				}
				return null;
			}

			string[] files = file.list();
			if (files != null)
			{
				for (int i = 0; i < files.Length; i++)
				{
					files[i] = getMsFileName(files[i]);
				}
			}

			return files;
		}

		public override int ioDread(string dirName, SceIoDirent dir)
		{
			if (dir != null)
			{
				// Use ExtendedInfo for the MemoryStick
				dir.UseExtendedInfo = useDirExtendedInfo;
			}

			return base.ioDread(dirName, dir);
		}

		public override int ioMkdir(string name, int mode)
		{
			File file = getFile(name);

			if (file.exists())
			{
				return SceKernelErrors.ERROR_ERRNO_FILE_ALREADY_EXISTS;
			}
			if (!file.mkdir())
			{
				return IO_ERROR;
			}

			return 0;
		}

		public override int ioRmdir(string name)
		{
			File file = getFile(name);

			if (!file.exists())
			{
				return SceKernelErrors.ERROR_ERRNO_FILE_NOT_FOUND;
			}
			if (!file.delete())
			{
				return IO_ERROR;
			}

			return 0;
		}

		public override int ioChstat(string fileName, SceIoStat stat, int bits)
		{
			File file = getFile(fileName);

			int mode = stat.mode;
			bool successful = true;

			if ((bits & 0x0001) != 0)
			{ // Others execute permission
				if (!file.Directory && !file.setExecutable((mode & 0x0001) != 0))
				{
					successful = false;
				}
			}
			if ((bits & 0x0002) != 0)
			{ // Others write permission
				if (!file.setWritable((mode & 0x0002) != 0))
				{
					successful = false;
				}
			}
			if ((bits & 0x0004) != 0)
			{ // Others read permission
				if (!file.setReadable((mode & 0x0004) != 0))
				{
					successful = false;
				}
			}

			if ((bits & 0x0040) != 0)
			{ // User execute permission
				if (!file.setExecutable((mode & 0x0040) != 0, true))
				{
					successful = false;
				}
			}
			if ((bits & 0x0080) != 0)
			{ // User write permission
				if (!file.setWritable((mode & 0x0080) != 0, true))
				{
					successful = false;
				}
			}
			if ((bits & 0x0100) != 0)
			{ // User read permission
				if (!file.setReadable((mode & 0x0100) != 0, true))
				{
					successful = false;
				}
			}

			return successful ? 0 : IO_ERROR;
		}

		public override int ioRename(string oldFileName, string newFileName)
		{
			File oldFile = getFile(oldFileName);
			File newFile = getFile(newFileName);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("ioRename: renaming file '{0}' to '{1}'", oldFileName, newFileName));
			}

			if (!oldFile.renameTo(newFile))
			{
				Console.WriteLine(string.Format("ioRename failed: '{0}' to '{1}'", oldFileName, newFileName));
				return IO_ERROR;
			}

			return 0;
		}

		public override int ioDevctl(string deviceName, int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			int result;

			switch (command)
			{
				// Register memorystick insert/eject callback (fatms0).
				case 0x02415821:
				{
					Console.WriteLine("sceIoDevctl register memorystick insert/eject callback (fatms0)");
					ThreadManForUser threadMan = Modules.ThreadManForUserModule;
					if (!deviceName.Equals("fatms0:"))
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					else if (inputPointer.AddressGood && inputLength == 4)
					{
						int cbid = inputPointer.getValue32();
						const int callbackType = SceKernelThreadInfo.THREAD_CALLBACK_MEMORYSTICK_FAT;
						if (threadMan.hleKernelRegisterCallback(callbackType, cbid))
						{
							// Trigger the registered callback immediately.
							// Only trigger this one callback, not all the MS callbacks.
							threadMan.hleKernelNotifyCallback(callbackType, cbid, MemoryStick.StateFatMs);
							result = 0; // Success.
						}
						else
						{
							result = SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
						}
					}
					else
					{
						result = SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				// Unregister memorystick insert/eject callback (fatms0).
				case 0x02415822:
				{
					Console.WriteLine("sceIoDevctl unregister memorystick insert/eject callback (fatms0)");
					ThreadManForUser threadMan = Modules.ThreadManForUserModule;
					if (!deviceName.Equals("fatms0:"))
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					else if (inputPointer.AddressGood && inputLength == 4)
					{
						int cbid = inputPointer.getValue32();
						threadMan.hleKernelUnRegisterCallback(SceKernelThreadInfo.THREAD_CALLBACK_MEMORYSTICK_FAT, cbid);
						result = 0; // Success.
					}
					else
					{
						result = SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				// Set if the device is assigned/inserted or not (fatms0).
				case 0x02415823:
				{
					Console.WriteLine("sceIoDevctl set assigned device (fatms0)");
					if (!deviceName.Equals("fatms0:"))
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					else if (inputPointer.AddressGood && inputLength >= 4)
					{
						// 0 - Device is not assigned (callback not registered).
						// 1 - Device is assigned (callback registered).
						MemoryStick.StateFatMs = inputPointer.getValue32();
						result = 0;
					}
					else
					{
						result = IO_ERROR;
					}
					break;
				}
				// Check if the device is write protected (fatms0).
				case 0x02425824:
				{
					Console.WriteLine("sceIoDevctl check write protection (fatms0)");
					if (!deviceName.Equals("fatms0:") && !deviceName.Equals("ms0:"))
					{ // For this command the alias "ms0:" is also supported.
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					else if (outputPointer.AddressGood)
					{
						// 0 - Device is not protected.
						// 1 - Device is protected.
						outputPointer.setValue32(0);
						result = 0;
					}
					else
					{
						result = IO_ERROR;
					}
					break;
				}
				// Get MS capacity (fatms0).
				case 0x02425818:
				{
					Console.WriteLine("sceIoDevctl get MS capacity (fatms0)");
					int sectorSize = 0x200;
					int sectorCount = MemoryStick.SectorSize / sectorSize;
					int maxClusters = (int)((MemoryStick.FreeSize * 95L / 100) / (sectorSize * sectorCount));
					int freeClusters = maxClusters;
					int maxSectors = maxClusters;
					if (inputPointer.AddressGood && inputLength >= 4)
					{
						int addr = inputPointer.getValue32();
						if (Memory.isAddressGood(addr))
						{
							Console.WriteLine("sceIoDevctl refer ms free space");
							Memory mem = Memory.Instance;
							mem.write32(addr, maxClusters);
							mem.write32(addr + 4, freeClusters);
							mem.write32(addr + 8, maxSectors);
							mem.write32(addr + 12, sectorSize);
							mem.write32(addr + 16, sectorCount);
							result = 0;
						}
						else
						{
							Console.WriteLine("sceIoDevctl 0x02425818 bad save address " + string.Format("0x{0:X8}", addr));
							result = IO_ERROR;
						}
					}
					else
					{
						Console.WriteLine("sceIoDevctl 0x02425818 bad param address " + string.Format("0x{0:X8}", inputPointer) + " or size " + inputLength);
						result = IO_ERROR;
					}
					break;
				}
				// Check if the device is assigned/inserted (fatms0).
				case 0x02425823:
				{
					Console.WriteLine("sceIoDevctl check assigned device (fatms0)");
					if (!deviceName.Equals("fatms0:"))
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					else if (outputPointer.AddressGood && outputLength >= 4)
					{
						// 0 - Device is not assigned (callback not registered).
						// 1 - Device is assigned (callback registered).
						outputPointer.setValue32(MemoryStick.StateFatMs);
						result = 0;
					}
					else
					{
						result = IO_ERROR;
					}
					break;
				}
				case 0x00005802:
				{
					if (!"flash1:".Equals(deviceName) || inputLength != 0 || outputLength != 0)
					{
						result = IO_ERROR;
					}
					else
					{
						result = 0;
					}
					break;
				}
				default:
				{
					result = base.ioDevctl(deviceName, command, inputPointer, inputLength, outputPointer, outputLength);
				}
			break;
			}

			return result;
		}

		public override IDictionary<IoFileMgrForUser.IoOperation, IoFileMgrForUser.IoOperationTiming> Timings
		{
			get
			{
				return IoFileMgrForUser.noDelayTimings;
			}
		}
	}

}