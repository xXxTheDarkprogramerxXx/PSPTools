using System;
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
	/// Provide a IVirtualFile interface by reading from a byte array.
	/// No write access is allowed.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class ByteArrayVirtualFile : IVirtualFile
	{
		private sbyte[] buffer;
		private int offset;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private int length_Renamed;
		private int currentIndex;

		public ByteArrayVirtualFile(sbyte[] buffer)
		{
			this.buffer = buffer;
			offset = 0;
			length_Renamed = buffer.Length;
		}

		public ByteArrayVirtualFile(sbyte[] buffer, int offset, int Length)
		{
			this.buffer = buffer;
			this.offset = offset;
			this.length_Renamed = Length;
		}

		public virtual int ioClose()
		{
			buffer = null;
			return 0;
		}

		public virtual int ioRead(TPointer outputPointer, int outputLength)
		{
			outputLength = System.Math.Min(length_Renamed - (currentIndex - offset), outputLength);
			Utilities.writeBytes(outputPointer.Address, outputLength, buffer, currentIndex);
			currentIndex += outputLength;

			return outputLength;
		}

		public virtual int ioRead(sbyte[] outputBuffer, int outputOffset, int outputLength)
		{
			outputLength = System.Math.Min(length_Renamed - (currentIndex - offset), outputLength);
			Array.Copy(buffer, currentIndex, outputBuffer, outputOffset, outputLength);
			currentIndex += outputLength;

			return outputLength;
		}

		public virtual int ioWrite(TPointer inputPointer, int inputLength)
		{
			return IO_ERROR;
		}

		public virtual int ioWrite(sbyte[] inputBuffer, int inputOffset, int inputLength)
		{
			return IO_ERROR;
		}

		public virtual long ioLseek(long offset)
		{
			currentIndex = this.offset + System.Math.Min(length_Renamed, (int) offset);
			return Position;
		}

		public virtual int ioIoctl(int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			return IO_ERROR;
		}

		public virtual long Length()
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
				return currentIndex - offset;
			}
		}

		public virtual IVirtualFile duplicate()
		{
			IVirtualFile duplicate = new ByteArrayVirtualFile(buffer, offset, length_Renamed);
			duplicate.ioLseek(Position);

			return duplicate;
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