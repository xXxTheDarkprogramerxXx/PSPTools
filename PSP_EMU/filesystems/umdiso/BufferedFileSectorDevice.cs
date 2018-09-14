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
namespace pspsharp.filesystems.umdiso
{

	public class BufferedFileSectorDevice : AbstractFileSectorDevice
	{
		protected internal RandomAccessFile tocFile;
		protected internal ISectorDevice sectorDevice;
		protected internal Dictionary<int, int> toc;
		protected internal bool tocDirty;
		protected internal int nextFreeBufferedSectorNumber;
		protected internal int numSectors;

		public BufferedFileSectorDevice(RandomAccessFile tocFile, RandomAccessFile fileAccess, ISectorDevice sectorDevice) : base(fileAccess)
		{
			this.tocFile = tocFile;
			this.sectorDevice = sectorDevice;
			readToc();
		}

		protected internal virtual void readToc()
		{
			toc = new Dictionary<int, int>();
			nextFreeBufferedSectorNumber = 0;
			tocDirty = false;
			try
			{
				tocFile.seek(0);
				long length = tocFile.length();
				if (length >= 4)
				{
					numSectors = tocFile.readInt();
					for (long i = 4; i < length; i += 8)
					{
						int sectorNumber = tocFile.readInt();
						int bufferedSectorNumber = tocFile.readInt();
						toc[sectorNumber] = bufferedSectorNumber;
						nextFreeBufferedSectorNumber = System.Math.Max(nextFreeBufferedSectorNumber, bufferedSectorNumber + 1);
					}
				}
				else if (sectorDevice != null)
				{
					numSectors = sectorDevice.NumSectors;
				}
			}
			catch (IOException e)
			{
				log.error("readToc", e);
			}
		}

		protected internal virtual void writeToc()
		{
			if (tocDirty)
			{
				try
				{
					tocFile.seek(0);
					tocFile.writeInt(NumSectors);
					foreach (int? sectorNumber in toc.Keys)
					{
						int? bufferedSectorNumber = toc[sectorNumber];
						tocFile.writeInt(sectorNumber.Value);
						tocFile.writeInt(bufferedSectorNumber.Value);
					}
					tocDirty = false;
				}
				catch (IOException e)
				{
					log.error("writeToc", e);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void readSector(int sectorNumber, byte[] buffer, int offset) throws java.io.IOException
		public override void readSector(int sectorNumber, sbyte[] buffer, int offset)
		{
			int? bufferedSectorNumber = toc[sectorNumber];
			if (bufferedSectorNumber != null)
			{
				fileAccess.seek(((long) ISectorDevice_Fields.sectorLength) * bufferedSectorNumber.Value);
				fileAccess.read(buffer, offset, ISectorDevice_Fields.sectorLength);
				return;
			}

			if (sectorDevice == null)
			{
				log.warn(string.Format("Reading outside the UMD buffer file (sector=0x{0:X})", sectorNumber));
				Arrays.fill(buffer, offset, offset + ISectorDevice_Fields.sectorLength, (sbyte) 0);
			}
			else
			{
				sectorDevice.readSector(sectorNumber, buffer, offset);
				fileAccess.seek(((long) ISectorDevice_Fields.sectorLength) * nextFreeBufferedSectorNumber);
				fileAccess.write(buffer, offset, ISectorDevice_Fields.sectorLength);
				toc[sectorNumber] = nextFreeBufferedSectorNumber;

				nextFreeBufferedSectorNumber++;
				tocDirty = true;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int getNumSectors() throws java.io.IOException
		public override int NumSectors
		{
			get
			{
				return numSectors;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void close()
		{
			base.close();

			if (sectorDevice != null)
			{
				sectorDevice.close();
				sectorDevice = null;
			}

			writeToc();

			tocFile.close();
			tocFile = null;

			toc = null;
		}
	}

}