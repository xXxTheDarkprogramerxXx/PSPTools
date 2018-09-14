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
//	import static pspsharp.HLE.VFS.AbstractVirtualFileSystem.IO_ERROR;

	using IoFileMgrForUser = pspsharp.HLE.modules.IoFileMgrForUser;
	using IoOperation = pspsharp.HLE.modules.IoFileMgrForUser.IoOperation;
	using IoOperationTiming = pspsharp.HLE.modules.IoFileMgrForUser.IoOperationTiming;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// Provide a IVirtualFile interface by reading from memory.
	/// Write access is allowed.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class MemoryVirtualFile : IVirtualFile
	{
		private readonly int startAddress;
		private int address;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly int length_Renamed;

		public MemoryVirtualFile(int address, int length)
		{
			this.startAddress = address;
			this.address = address;
			this.length_Renamed = length;
		}

		public virtual int ioClose()
		{
			address = 0;

			return 0;
		}

		public virtual int ioRead(TPointer outputPointer, int outputLength)
		{
			outputLength = System.Math.Min(outputLength, length_Renamed - (address - startAddress));
			outputPointer.Memory.memcpy(outputPointer.Address, address, outputLength);
			address += outputLength;

			return outputLength;
		}

		public virtual int ioRead(sbyte[] outputBuffer, int outputOffset, int outputLength)
		{
			outputLength = System.Math.Min(outputLength, length_Renamed - (address - startAddress));
			Utilities.readBytes(address, outputLength, outputBuffer, outputOffset);
			address += outputLength;

			return outputLength;
		}

		public virtual int ioWrite(TPointer inputPointer, int inputLength)
		{
			inputLength = System.Math.Min(inputLength, length_Renamed - (address - startAddress));
			inputPointer.Memory.memcpy(address, inputPointer.Address, inputLength);
			address += inputLength;

			return inputLength;
		}

		public virtual int ioWrite(sbyte[] inputBuffer, int inputOffset, int inputLength)
		{
			inputLength = System.Math.Min(inputLength, length_Renamed - (address - startAddress));
			Utilities.writeBytes(address, inputLength, inputBuffer, inputOffset);
			address += inputLength;

			return inputLength;
		}

		public virtual long ioLseek(long offset)
		{
			address = startAddress + (int) offset;

			return offset;
		}

		public virtual int ioIoctl(int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			return IO_ERROR;
		}

		public virtual long length()
		{
			return length_Renamed;
		}

		public virtual bool SectorBlockMode
		{
			get
			{
				return false;
			}
		}

		public virtual long Position
		{
			get
			{
				return address - startAddress;
			}
		}

		public virtual IVirtualFile duplicate()
		{
			MemoryVirtualFile vFile = new MemoryVirtualFile(startAddress, length_Renamed);
			vFile.ioLseek(Position);

			return vFile;
		}

		public virtual IDictionary<IoFileMgrForUser.IoOperation, IoFileMgrForUser.IoOperationTiming> Timings
		{
			get
			{
				return IoFileMgrForUser.defaultTimings;
			}
		}

		public override string ToString()
		{
			return string.Format("MemoryVirtualFile 0x{0:X8}-0x{1:X8} (length=0x{2:X})", startAddress, startAddress + length_Renamed, length_Renamed);
		}
	}

}