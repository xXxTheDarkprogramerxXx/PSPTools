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

	using IoFileMgrForUser = pspsharp.HLE.modules.IoFileMgrForUser;

	public interface IVirtualFile
	{
		int ioClose();
		int ioRead(TPointer outputPointer, int outputLength);
		int ioRead(sbyte[] outputBuffer, int outputOffset, int outputLength);
		int ioWrite(TPointer inputPointer, int inputLength);
		int ioWrite(sbyte[] inputBuffer, int inputOffset, int inputLength);
		long ioLseek(long offset);
		int ioIoctl(int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength);
		long length();
		bool SectorBlockMode {get;}
		long Position {get;}
		IVirtualFile duplicate();
		IDictionary<IoFileMgrForUser.IoOperation, IoFileMgrForUser.IoOperationTiming> Timings {get;}
	}

}