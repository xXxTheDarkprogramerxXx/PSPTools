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

	////using Logger = org.apache.log4j.Logger;

	using IoOperation = pspsharp.HLE.modules.IoFileMgrForUser.IoOperation;
	using IoOperationTiming = pspsharp.HLE.modules.IoFileMgrForUser.IoOperationTiming;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// Proxy all the IVirtualFile interface calls to another virtual file.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public abstract class AbstractProxyVirtualFile : IVirtualFile
	{
		//protected internal static Logger log = AbstractVirtualFileSystem.log;
		protected internal IVirtualFile vFile;

		protected internal AbstractProxyVirtualFile()
		{
		}

		protected internal AbstractProxyVirtualFile(IVirtualFile vFile)
		{
			this.vFile = vFile;
		}

		protected internal virtual IVirtualFile ProxyVirtualFile
		{
			set
			{
				this.vFile = value;
			}
		}

		public virtual int ioClose()
		{
			return vFile.ioClose();
		}

		public virtual int ioRead(TPointer outputPointer, int outputLength)
		{
			return vFile.ioRead(outputPointer, outputLength);
		}

		/*
		 * Perform the ioRead in PSP memory using the ioRead for a byte array.
		 */
		protected internal virtual int ioReadBuf(TPointer outputPointer, int outputLength)
		{
			if (outputLength <= 0)
			{
				return 0;
			}

			sbyte[] outputBuffer = new sbyte[outputLength];
			int readLength = ioRead(outputBuffer, 0, outputLength);
			Utilities.writeBytes(outputPointer.Address, readLength, outputBuffer, 0);

			return readLength;
		}

		public virtual int ioRead(sbyte[] outputBuffer, int outputOffset, int outputLength)
		{
			return vFile.ioRead(outputBuffer, outputOffset, outputLength);
		}

		public virtual int ioWrite(TPointer inputPointer, int inputLength)
		{
			return vFile.ioWrite(inputPointer, inputLength);
		}

		public virtual int ioWrite(sbyte[] inputBuffer, int inputOffset, int inputLength)
		{
			return vFile.ioWrite(inputBuffer, inputOffset, inputLength);
		}

		public virtual long ioLseek(long offset)
		{
			return vFile.ioLseek(offset);
		}

		public virtual int ioIoctl(int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			return vFile.ioIoctl(command, inputPointer, inputLength, outputPointer, outputLength);
		}

		public virtual long Length()
		{
			return vFile.Length();
		}

		public virtual bool SectorBlockMode
		{
			get
			{
				return vFile.SectorBlockMode;
			}
		}

		public virtual long Position
		{
			get
			{
				return vFile.Position;
			}
		}

		public virtual IVirtualFile duplicate()
		{
			return vFile.duplicate();
		}

		public virtual IDictionary<IoOperation, IoOperationTiming> Timings
		{
			get
			{
				return vFile.Timings;
			}
		}
	}

}