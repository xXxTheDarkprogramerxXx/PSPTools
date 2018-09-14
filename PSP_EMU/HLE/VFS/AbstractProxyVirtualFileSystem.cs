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

	using Logger = org.apache.log4j.Logger;

	using SceIoDirent = pspsharp.HLE.kernel.types.SceIoDirent;
	using SceIoStat = pspsharp.HLE.kernel.types.SceIoStat;
	using IoOperation = pspsharp.HLE.modules.IoFileMgrForUser.IoOperation;
	using IoOperationTiming = pspsharp.HLE.modules.IoFileMgrForUser.IoOperationTiming;

	/// <summary>
	/// Proxy all the IVirtualFileSystem interface calls to another virtual file system.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class AbstractProxyVirtualFileSystem : IVirtualFileSystem
	{
		protected internal static Logger log = AbstractVirtualFileSystem.log;
		protected internal IVirtualFileSystem vfs;

		protected internal AbstractProxyVirtualFileSystem()
		{
		}

		protected internal AbstractProxyVirtualFileSystem(IVirtualFileSystem vfs)
		{
			this.vfs = vfs;
		}

		protected internal virtual IVirtualFileSystem ProxyVirtualFileSystem
		{
			set
			{
				this.vfs = value;
			}
		}

		public virtual void ioInit()
		{
			vfs.ioInit();
		}

		public virtual void ioExit()
		{
			vfs.ioExit();
		}

		public virtual IVirtualFile ioOpen(string fileName, int flags, int mode)
		{
			return vfs.ioOpen(fileName, flags, mode);
		}

		public virtual int ioClose(IVirtualFile file)
		{
			return vfs.ioClose(file);
		}

		public virtual int ioRead(IVirtualFile file, TPointer outputPointer, int outputLength)
		{
			return vfs.ioRead(file, outputPointer, outputLength);
		}

		public virtual int ioWrite(IVirtualFile file, TPointer inputPointer, int inputLength)
		{
			return vfs.ioWrite(file, inputPointer, inputLength);
		}

		public virtual long ioLseek(IVirtualFile file, long offset)
		{
			return vfs.ioLseek(file, offset);
		}

		public virtual int ioIoctl(IVirtualFile file, int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			return vfs.ioIoctl(file, command, inputPointer, inputLength, outputPointer, outputLength);
		}

		public virtual int ioRemove(string name)
		{
			return vfs.ioRemove(name);
		}

		public virtual int ioMkdir(string name, int mode)
		{
			return vfs.ioMkdir(name, mode);
		}

		public virtual int ioRmdir(string name)
		{
			return vfs.ioRmdir(name);
		}

		public virtual string[] ioDopen(string dirName)
		{
			return vfs.ioDopen(dirName);
		}

		public virtual int ioDclose(string dirName)
		{
			return vfs.ioDclose(dirName);
		}

		public virtual int ioDread(string dirName, SceIoDirent dir)
		{
			return vfs.ioDread(dirName, dir);
		}

		public virtual int ioGetstat(string fileName, SceIoStat stat)
		{
			return vfs.ioGetstat(fileName, stat);
		}

		public virtual int ioChstat(string fileName, SceIoStat stat, int bits)
		{
			return vfs.ioChstat(fileName, stat, bits);
		}

		public virtual int ioRename(string oldFileName, string newFileName)
		{
			return vfs.ioRename(oldFileName, newFileName);
		}

		public virtual int ioChdir(string directoryName)
		{
			return vfs.ioChdir(directoryName);
		}

		public virtual int ioMount()
		{
			return vfs.ioMount();
		}

		public virtual int ioUmount()
		{
			return vfs.ioUmount();
		}

		public virtual int ioDevctl(string deviceName, int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			return vfs.ioDevctl(deviceName, command, inputPointer, inputLength, outputPointer, outputLength);
		}

		public virtual IDictionary<IoOperation, IoOperationTiming> Timings
		{
			get
			{
				return vfs.Timings;
			}
		}
	}

}