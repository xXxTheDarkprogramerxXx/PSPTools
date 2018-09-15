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

	/// <summary>
	/// Provide a IVirtualFile interface by reading from a part of another virtual file.
	/// E.g. a part of virtual file can be considered as a virtual file itself.
	/// The part of the virtual file is defined by giving a start offset and a Length.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class PartialVirtualFile : AbstractProxyVirtualFile
	{
		private long startPosition;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private long length_Renamed;

		public PartialVirtualFile(IVirtualFile vFile, long startPosition, long Length) : base(vFile)
		{
			this.startPosition = startPosition;
			this.length_Renamed = Length;

			vFile.ioLseek(startPosition);
		}

		private int RestLength
		{
			get
			{
				long restLength = Length() - Position;
				if (restLength > int.MaxValue)
				{
					return int.MaxValue;
				}
    
				return (int) restLength;
			}
		}

		public override int ioRead(TPointer outputPointer, int outputLength)
		{
			outputLength = System.Math.Min(outputLength, RestLength);
			return vFile.ioRead(outputPointer, outputLength);
		}

		public override int ioRead(sbyte[] outputBuffer, int outputOffset, int outputLength)
		{
			outputLength = System.Math.Min(outputLength, RestLength);
			return vFile.ioRead(outputBuffer, outputOffset, outputLength);
		}

		public override long ioLseek(long offset)
		{
			if (offset > Length())
			{
				return AbstractVirtualFileSystem.IO_ERROR;
			}
			long result = vFile.ioLseek(startPosition + offset);
			if (result == AbstractVirtualFileSystem.IO_ERROR)
			{
				return result;
			}

			return result - startPosition;
		}

		public override long Length()
		{
			return length_Renamed;
		}

		public override long Position
		{
			get
			{
				return vFile.Position - startPosition;
			}
		}

		public override IVirtualFile duplicate()
		{
			IVirtualFile vFileDuplicate = vFile.duplicate();
			if (vFileDuplicate == null)
			{
				return null;
			}

			return new PartialVirtualFile(vFileDuplicate, startPosition, length_Renamed);
		}

		public override string ToString()
		{
			return string.Format("PartialVirtualFile[{0}, startPosition=0x{1:X}, Length=0x{2:X}]", vFile, startPosition, length_Renamed);
		}
	}

}