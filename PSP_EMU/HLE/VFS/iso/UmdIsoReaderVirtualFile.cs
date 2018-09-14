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
namespace pspsharp.HLE.VFS.iso
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_FILE_READ_ERROR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.filesystems.umdiso.UmdIsoFile.sectorLength;

	using UmdIsoReader = pspsharp.filesystems.umdiso.UmdIsoReader;

	public class UmdIsoReaderVirtualFile : AbstractVirtualFile
	{
		private UmdIsoReader iso;
		private long position;
		private readonly sbyte[] buffer = new sbyte[sectorLength];

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public UmdIsoReaderVirtualFile(String fileName) throws java.io.IOException
		public UmdIsoReaderVirtualFile(string fileName) : base(null)
		{
			iso = new UmdIsoReader(fileName);
		}

		public override long Position
		{
			get
			{
				return position;
			}
		}

		private int SectorNumber
		{
			get
			{
				return (int)(position / sectorLength);
			}
		}

		private int SectorOffset
		{
			get
			{
				return (int)(position % sectorLength);
			}
		}

		public override int ioRead(TPointer outputPointer, int outputLength)
		{
			int readLength = 0;
			int outputOffset = 0;
			while (outputLength > 0)
			{
				try
				{
					iso.readSector(SectorNumber, buffer);
				}
				catch (IOException e)
				{
					log.error("ioRead", e);
					return ERROR_KERNEL_FILE_READ_ERROR;
				}

				int sectorOffset = SectorOffset;
				int length = System.Math.Min(sectorLength - sectorOffset, outputLength);
				outputPointer.setArray(outputOffset, buffer, sectorOffset, length);

				readLength += length;
				outputOffset += length;
				position += length;
				outputLength -= length;
			}

			return readLength;
		}

		public override int ioRead(sbyte[] outputBuffer, int outputOffset, int outputLength)
		{
			int readLength = 0;
			while (outputLength > 0)
			{
				int sectorOffset = SectorOffset;
				int length;

				// Can we read one or multiple sectors directly into the outputBuffer?
				if (sectorOffset == 0 && outputLength >= sectorLength)
				{
					try
					{
						int numberSectors = outputLength / sectorLength;
						iso.readSectors(SectorNumber, numberSectors, outputBuffer, outputOffset);

						length = numberSectors * sectorLength;
					}
					catch (IOException e)
					{
						log.error("ioRead", e);
						return ERROR_KERNEL_FILE_READ_ERROR;
					}
				}
				else
				{
					try
					{
						iso.readSector(SectorNumber, buffer);

						length = System.Math.Min(sectorLength - sectorOffset, outputLength);
						Array.Copy(buffer, sectorOffset, outputBuffer, outputOffset, length);
					}
					catch (IOException e)
					{
						log.error("ioRead", e);
						return ERROR_KERNEL_FILE_READ_ERROR;
					}
				}

				readLength += length;
				outputOffset += length;
				position += length;
				outputLength -= length;
			}

			return readLength;
		}

		public override long ioLseek(long offset)
		{
			position = offset;

			return offset;
		}

		public override int ioClose()
		{
			try
			{
				iso.close();
			}
			catch (IOException e)
			{
				log.error("ioClose", e);
				return IO_ERROR;
			}

			return 0;
		}

		public override long length()
		{
			return iso.NumSectors * (long) sectorLength;
		}

		public override string ToString()
		{
			return string.Format("{0}, position=0x{1:X}", iso, position);
		}
	}

}