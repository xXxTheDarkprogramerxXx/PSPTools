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
namespace pspsharp.HLE.VFS.iso
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_O_CREAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_O_TRUNC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_O_WRONLY;


	using SceIoStat = pspsharp.HLE.kernel.types.SceIoStat;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using ScePspDateTime = pspsharp.HLE.kernel.types.ScePspDateTime;
	using UmdIsoFile = pspsharp.filesystems.umdiso.UmdIsoFile;
	using UmdIsoReader = pspsharp.filesystems.umdiso.UmdIsoReader;

	public class UmdIsoVirtualFileSystem : AbstractVirtualFileSystem
	{
		protected internal readonly UmdIsoReader iso;

		public UmdIsoVirtualFileSystem(UmdIsoReader iso)
		{
			this.iso = iso;
		}

		public override IVirtualFile ioOpen(string fileName, int flags, int mode)
		{
			if (hasFlag(flags, PSP_O_WRONLY) || hasFlag(flags, PSP_O_CREAT) || hasFlag(flags, PSP_O_TRUNC))
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_ERRNO_READ_ONLY);
			}

			UmdIsoFile file;
			try
			{
				file = iso.getFile(fileName);
			}
			catch (FileNotFoundException)
			{
				return null;
			}
			catch (IOException e)
			{
				Console.WriteLine("ioOpen", e);
				return null;
			}

			// Opening "umd0:" is allowing to read the whole UMD per sectors.
			bool sectorBlockMode = (fileName.Length == 0);

			return new UmdIsoVirtualFile(file, sectorBlockMode, iso);
		}

		public override int ioGetstat(string fileName, SceIoStat stat)
		{
			int mode = 4; // 4 = readable
			int attr = 0;
			long size = 0;
			long timestamp = 0;
			int startSector = 0;
			try
			{
				// Check for files first.
				UmdIsoFile file = iso.getFile(fileName);
				attr |= 0x20; // Is file
				size = file.Length();
				timestamp = file.Timestamp.Ticks;
				startSector = file.StartSector;
			}
			catch (FileNotFoundException)
			{
				// If file wasn't found, try looking for a directory.
				try
				{
					if (iso.isDirectory(fileName))
					{
						attr |= 0x10; // Is directory
						mode |= 1; // 1 = executable
					}
				}
				catch (FileNotFoundException)
				{
					Console.WriteLine(string.Format("ioGetstat - '{0}' umd file/dir not found", fileName));
					return SceKernelErrors.ERROR_ERRNO_FILE_NOT_FOUND;
				}
				catch (IOException e)
				{
					Console.WriteLine("ioGetstat", e);
					return SceKernelErrors.ERROR_ERRNO_FILE_NOT_FOUND;
				}
			}
			catch (IOException e)
			{
				Console.WriteLine("ioGetstat", e);
				return SceKernelErrors.ERROR_ERRNO_FILE_NOT_FOUND;
			}

			// Octal extend into user and group
			mode = mode + (mode << 3) + (mode << 6);
			mode |= attr << 8;

			ScePspDateTime ctime = ScePspDateTime.fromUnixTime(timestamp);
			ScePspDateTime atime = ScePspDateTime.fromUnixTime(0);
			ScePspDateTime mtime = ScePspDateTime.fromUnixTime(timestamp);

			stat.init(mode, attr, size, ctime, atime, mtime);

			if (startSector > 0)
			{
				stat.StartSector = startSector;
			}

			return 0;
		}

		public override string[] ioDopen(string dirName)
		{
			string[] fileNames = null;

			try
			{
				if (iso.isDirectory(dirName))
				{
					fileNames = iso.listDirectory(dirName);
				}
				else
				{
					Console.WriteLine(string.Format("ioDopen file '{0}' is not a directory", dirName));
				}
			}
			catch (FileNotFoundException)
			{
				Console.WriteLine(string.Format("ioDopen directory '{0}' not found", dirName));
			}
			catch (IOException e)
			{
				Console.WriteLine("ioDopen", e);
			}

			return fileNames;
		}

		public override int ioDevctl(string deviceName, int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			int result;

			switch (command)
			{
				// Get UMD disc type.
				case 0x01F20001:
				{
					Console.WriteLine("ioDevctl get disc type");
					if (outputPointer.AddressGood && outputLength >= 8)
					{
						// 0 = No disc.
						// 0x10 = Game disc.
						// 0x20 = Video disc.
						// 0x40 = Audio disc.
						// 0x80 = Cleaning disc.
						int @out;
						if (iso == null)
						{
							@out = 0;
						}
						else
						{
							@out = 0x10; // Always return game disc (if present).
						}
						outputPointer.setValue32(4, @out);
						result = 0;
					}
					else
					{
						result = ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				// Get UMD current LBA.
				case 0x01F20002:
				{
					Console.WriteLine("ioDevctl get current LBA");
					if (outputPointer.AddressGood && outputLength >= 4)
					{
						outputPointer.setValue32(0); // Assume first sector.
						result = 0;
					}
					else
					{
						result = ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				// Seek UMD disc (raw).
				case 0x01F100A3:
				{
					Console.WriteLine("ioDevctl seek UMD disc");
					if (inputPointer.AddressGood && inputLength >= 4)
					{
						int sector = inputPointer.getValue32();
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("ioDevctl seek UMD disc: sector={0:D}", sector));
						}
						result = 0;
					}
					else
					{
						result = ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				// Prepare UMD data into cache.
				case 0x01F100A4:
				{
					Console.WriteLine("ioDevctl prepare UMD data to cache");
					if (inputPointer.AddressGood && inputLength >= 16)
					{
						// UMD cache read struct (16-bytes).
						int unk1 = inputPointer.getValue32(0); // NULL.
						int sector = inputPointer.getValue32(4); // First sector of data to read.
						int unk2 = inputPointer.getValue32(8); // NULL.
						int sectorNum = inputPointer.getValue32(12); // Length of data to read.
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("ioDevctl prepare UMD data to cache: sector={0:D}, sectorNum={1:D}, unk1={2:D}, unk2={3:D}", sector, sectorNum, unk1, unk2));
						}
						result = 0;
					}
					else
					{
						result = ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				// Prepare UMD data into cache and get status.
				case 0x01F300A5:
				{
					Console.WriteLine("ioDevctl prepare UMD data to cache and get status");
					if (inputPointer.AddressGood && inputLength >= 16 && outputPointer.AddressGood && outputLength >= 4)
					{
						// UMD cache read struct (16-bytes).
						int unk1 = inputPointer.getValue32(0); // NULL.
						int sector = inputPointer.getValue32(4); // First sector of data to read.
						int unk2 = inputPointer.getValue32(8); // NULL.
						int sectorNum = inputPointer.getValue32(12); // Length of data to read.
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("ioDevctl prepare UMD data to cache and get status: sector={0:D}, sectorNum={1:D}, unk1={2:D}, unk2={3:D}", sector, sectorNum, unk1, unk2));
						}
						outputPointer.setValue32(1); // Status (unitary index of the requested read, greater or equal to 1).
						result = 0;
					}
					else
					{
						result = ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				default:
					result = base.ioDevctl(deviceName, command, inputPointer, inputLength, outputPointer, outputLength);
				break;
			}

			return result;
		}

		public override void ioExit()
		{
			try
			{
				iso.close();
			}
			catch (IOException)
			{
			}

			base.ioExit();
		}
	}

}