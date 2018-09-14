using System;

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
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// Provide a IVirtualFile interface by reading from another virtual file
	/// in blocks on a given size.
	/// No write access is supported.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class BufferedVirtualFile : AbstractProxyVirtualFile
	{
		private sbyte[] buffer;
		private int bufferIndex;
		private int bufferLength;

		protected internal BufferedVirtualFile()
		{
		}

		public BufferedVirtualFile(IVirtualFile vFile, int bufferSize)
		{
			setBufferedVirtualFile(vFile, bufferSize);
		}

		protected internal virtual void setBufferedVirtualFile(IVirtualFile vFile, int bufferSize)
		{
			ProxyVirtualFile = vFile;
			buffer = new sbyte[bufferSize];
			bufferIndex = 0;
			bufferLength = 0;
		}

		private void copyFromBuffer(int outputAddr, int length)
		{
			if (length <= 0)
			{
				return;
			}

			Utilities.writeBytes(outputAddr, length, buffer, bufferIndex);
			bufferIndex += length;
		}

		private void copyFromBuffer(sbyte[] output, int offset, int length)
		{
			if (length <= 0)
			{
				return;
			}

			Array.Copy(buffer, bufferIndex, output, offset, length);
			bufferIndex += length;
		}

		private void checkPopulateBuffer()
		{
			if (bufferIndex < bufferLength)
			{
				return;
			}

			if (bufferLength > 0)
			{
				bufferIndex = 0;
			}
			bufferLength = vFile.ioRead(buffer, 0, buffer.Length);
		}

		public override int ioRead(TPointer outputPointer, int outputLength)
		{
			if (bufferLength < 0)
			{
				return bufferLength;
			}

			int readLength = 0;

			while (bufferLength >= 0 && readLength < outputLength)
			{
				checkPopulateBuffer();
				int length = System.Math.Min(bufferLength - bufferIndex, outputLength - readLength);
				copyFromBuffer(outputPointer.Address + readLength, length);
				readLength += length;
			}

			return readLength;
		}

		public override int ioRead(sbyte[] outputBuffer, int outputOffset, int outputLength)
		{
			if (bufferLength < 0)
			{
				return bufferLength;
			}

			int readLength = 0;

			while (bufferLength >= 0 && readLength < outputLength)
			{
				checkPopulateBuffer();
				int length = System.Math.Min(bufferLength - bufferIndex, outputLength - readLength);
				copyFromBuffer(outputBuffer, outputOffset + readLength, length);
				readLength += length;
			}

			return readLength;
		}

		public override int ioWrite(TPointer inputPointer, int inputLength)
		{
			// Write not supported
			return IO_ERROR;
		}

		public override int ioWrite(sbyte[] inputBuffer, int inputOffset, int inputLength)
		{
			// Write not supported
			return IO_ERROR;
		}

		public override long ioLseek(long offset)
		{
			long virtualFileOffset = (offset / buffer.Length) * buffer.Length;
			long result = vFile.ioLseek(virtualFileOffset);
			if (result == IO_ERROR)
			{
				return result;
			}

			bufferLength = 0;
			bufferIndex = 0;
			if (offset > virtualFileOffset)
			{
				bufferIndex = (int)(offset - virtualFileOffset);
			}

			return offset;
		}

		public override long Position
		{
			get
			{
				if (bufferLength <= 0)
				{
					return vFile.Position;
				}
    
				return vFile.Position - bufferLength + bufferIndex;
			}
		}

		public override IVirtualFile duplicate()
		{
			IVirtualFile vFileDuplicate = vFile.duplicate();
			if (vFileDuplicate == null)
			{
				return null;
			}

			BufferedVirtualFile dup = new BufferedVirtualFile(vFileDuplicate, buffer.Length);
			dup.ioLseek(Position);

			return dup;
		}
	}

}