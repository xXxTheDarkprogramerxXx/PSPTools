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

	using IoFileMgrForUser = pspsharp.HLE.modules.IoFileMgrForUser;
	using IoOperation = pspsharp.HLE.modules.IoFileMgrForUser.IoOperation;
	using IoOperationTiming = pspsharp.HLE.modules.IoFileMgrForUser.IoOperationTiming;

	public class TmpLocalVirtualFile : AbstractProxyVirtualFile
	{
		protected internal IVirtualFile ioctl;

		public TmpLocalVirtualFile(IVirtualFile vFile, IVirtualFile ioctl) : base(vFile)
		{
			this.ioctl = ioctl;
		}

		public override int ioIoctl(int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			if (ioctl != null)
			{
				return ioctl.ioIoctl(command, inputPointer, inputLength, outputPointer, outputLength);
			}
			return base.ioIoctl(command, inputPointer, inputLength, outputPointer, outputLength);
		}

		public override int ioClose()
		{
			if (ioctl != null)
			{
				ioctl.ioClose();
			}
			return base.ioClose();
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