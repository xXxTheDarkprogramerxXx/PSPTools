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

	using SceIoDirent = pspsharp.HLE.kernel.types.SceIoDirent;
	using SceIoStat = pspsharp.HLE.kernel.types.SceIoStat;
	using IoFileMgrForUser = pspsharp.HLE.modules.IoFileMgrForUser;

	public interface IVirtualFileSystem
	{
		void ioInit();
		void ioExit();
		IVirtualFile ioOpen(string fileName, int flags, int mode);
		int ioClose(IVirtualFile file);
		int ioRead(IVirtualFile file, TPointer outputPointer, int outputLength);
		int ioWrite(IVirtualFile file, TPointer inputPointer, int inputLength);
		long ioLseek(IVirtualFile file, long offset);
		int ioIoctl(IVirtualFile file, int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength);
		int ioRemove(string name);
		int ioMkdir(string name, int mode);
		int ioRmdir(string name);
		string[] ioDopen(string dirName);
		int ioDclose(string dirName);
		int ioDread(string dirName, SceIoDirent dir);
		int ioGetstat(string fileName, SceIoStat stat);
		int ioChstat(string fileName, SceIoStat stat, int bits);
		int ioRename(string oldFileName, string newFileName);
		int ioChdir(string directoryName);
		int ioMount();
		int ioUmount();
		int ioDevctl(string deviceName, int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength);
		IDictionary<IoFileMgrForUser.IoOperation, IoFileMgrForUser.IoOperationTiming> Timings {get;}
	}

}