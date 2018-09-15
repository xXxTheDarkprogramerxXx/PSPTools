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
namespace pspsharp.HLE.VFS
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_UNSUPPORTED_OPERATION;

	using Level = org.apache.log4j.Level;
	//using Logger = org.apache.log4j.Logger;

	using SceIoDirent = pspsharp.HLE.kernel.types.SceIoDirent;
	using SceIoStat = pspsharp.HLE.kernel.types.SceIoStat;
	using IoFileMgrForUser = pspsharp.HLE.modules.IoFileMgrForUser;
	using Utilities = pspsharp.util.Utilities;

	public abstract class AbstractVirtualFileSystem : IVirtualFileSystem
	{
		protected internal static Logger log = Logger.getLogger("vfs");
		public const int IO_ERROR = -1;

		protected internal static bool hasFlag(int mode, int flag)
		{
			return (mode & flag) == flag;
		}

		public virtual void ioInit()
		{
		}

		public virtual void ioExit()
		{
		}

		public virtual IVirtualFile ioOpen(string fileName, int flags, int mode)
		{
			return null;
		}

		public virtual int ioRead(IVirtualFile file, TPointer outputPointer, int outputLength)
		{
			return file.ioRead(outputPointer, outputLength);
		}

		public virtual int ioWrite(IVirtualFile file, TPointer inputPointer, int inputLength)
		{
			return file.ioWrite(inputPointer, inputLength);
		}

		public virtual long ioLseek(IVirtualFile file, long offset)
		{
			return file.ioLseek(offset);
		}

		public virtual int ioIoctl(IVirtualFile file, int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			return file.ioIoctl(command, inputPointer, inputLength, outputPointer, outputLength);
		}

		public virtual int ioClose(IVirtualFile file)
		{
			return file.ioClose();
		}

		public virtual int ioRemove(string name)
		{
			return IO_ERROR;
		}

		public virtual int ioMkdir(string name, int mode)
		{
			return IO_ERROR;
		}

		public virtual int ioRmdir(string name)
		{
			return IO_ERROR;
		}

		public virtual string[] ioDopen(string dirName)
		{
			return null;
		}

		public virtual int ioDread(string dirName, SceIoDirent dir)
		{
			// Return the Getstat on the given directory file
			string fileName;
			if (string.ReferenceEquals(dirName, null) || dirName.Length == 0)
			{
				fileName = dir.filename;
			}
			else
			{
				fileName = dirName + "/" + dir.filename;
			}

			int result = ioGetstat(fileName, dir.stat);
			if (result == 0)
			{
				// Success is 1 for sceIoDread
				return 1;
			}
			return result;
		}

		public virtual int ioDclose(string dirName)
		{
			return 0;
		}

		public virtual int ioGetstat(string fileName, SceIoStat stat)
		{
			return IO_ERROR;
		}

		public virtual int ioChstat(string fileName, SceIoStat stat, int bits)
		{
			return IO_ERROR;
		}

		public virtual int ioRename(string oldFileName, string newFileName)
		{
			return IO_ERROR;
		}

		public virtual int ioChdir(string directoryName)
		{
			return IO_ERROR;
		}

		public virtual int ioMount()
		{
			return IO_ERROR;
		}

		public virtual int ioUmount()
		{
			return IO_ERROR;
		}

		public virtual int ioDevctl(string deviceName, int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			if (log.isEnabledFor(Level.WARN))
			{
				Console.WriteLine(string.Format("ioDevctl on '{0}', 0x{1:X8} unsupported command, inlen={2:D}, outlen={3:D}", deviceName, command, inputLength, outputLength));
				if (inputPointer.AddressGood)
				{
					Console.WriteLine(string.Format("ioDevctl indata: {0}", Utilities.getMemoryDump(inputPointer.Address, inputLength)));
				}
				if (outputPointer.AddressGood)
				{
					Console.WriteLine(string.Format("ioDevctl outdata: {0}", Utilities.getMemoryDump(outputPointer.Address, outputLength)));
				}
			}

			return ERROR_KERNEL_UNSUPPORTED_OPERATION;
		}

		public virtual IDictionary<IoFileMgrForUser.IoOperation, IoFileMgrForUser.IoOperationTiming> Timings
		{
			get
			{
				return IoFileMgrForUser.defaultTimings;
			}
		}
	}

}