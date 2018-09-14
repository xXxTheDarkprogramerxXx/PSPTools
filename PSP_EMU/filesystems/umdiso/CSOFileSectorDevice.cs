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
	using FileUtil = pspsharp.util.FileUtil;


	public class CSOFileSectorDevice : AbstractFileSectorDevice
	{
		protected internal int offsetShift;
		protected internal int numSectors;
		protected internal long[] sectorOffsets;
		private const long sectorOffsetMask = 0x7FFFFFFFL;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public CSOFileSectorDevice(java.io.RandomAccessFile fileAccess, byte[] header) throws java.io.IOException
		public CSOFileSectorDevice(RandomAccessFile fileAccess, sbyte[] header) : base(fileAccess)
		{
			ByteBuffer byteBuffer = ByteBuffer.wrap(header).order(ByteOrder.LITTLE_ENDIAN);

			/*
				u32 'CISO'
				u64 image size in bytes (first u32 is highest 32-bit, second u32 is lowest 32-bit)
				u32 sector size? (00000800 = 2048 = sector size)
				u32 ? (1)
				u32[] sector offsets (as many as image size / sector size, I guess)
			 */
			long lengthInBytes = (((long) byteBuffer.getInt(4)) << 32) | (byteBuffer.getInt(8) & 0xFFFFFFFFL);
			int sectorSize = byteBuffer.getInt(16);
			offsetShift = byteBuffer.get(21) & 0xFF;
			numSectors = getNumSectors(lengthInBytes, sectorSize);
			sectorOffsets = new long[numSectors + 1];

			sbyte[] offsetData = new sbyte[(numSectors + 1) * 4];
			fileAccess.seek(24);
			fileAccess.readFully(offsetData);
			ByteBuffer offsetBuffer = ByteBuffer.wrap(offsetData).order(ByteOrder.LITTLE_ENDIAN);

			for (int i = 0; i <= numSectors; i++)
			{
				sectorOffsets[i] = offsetBuffer.getInt(i * 4) & 0xFFFFFFFFL;
				if (i > 0)
				{
					if ((sectorOffsets[i] & sectorOffsetMask) < (sectorOffsets[i - 1] & sectorOffsetMask))
					{
						log.error(string.Format("Corrupted CISO - Invalid offset [{0:D}]: 0x{1:X8} < 0x{2:X8}", i, sectorOffsets[i], sectorOffsets[i - 1]));
					}
				}
			}
		}

		public override int NumSectors
		{
			get
			{
				return numSectors;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void readSector(int sectorNumber, byte[] buffer, int offset) throws java.io.IOException
		public override void readSector(int sectorNumber, sbyte[] buffer, int offset)
		{
			long sectorOffset = sectorOffsets[sectorNumber];
			long sectorEnd = sectorOffsets[sectorNumber + 1];

			if ((sectorOffset & 0x80000000) != 0)
			{
				long realOffset = (sectorOffset & sectorOffsetMask) << offsetShift;
				fileAccess.seek(realOffset);
				fileAccess.read(buffer, offset, ISectorDevice_Fields.sectorLength);
			}
			else
			{
				sectorEnd = (sectorEnd & sectorOffsetMask) << offsetShift;
				sectorOffset = (sectorOffset & sectorOffsetMask) << offsetShift;

				int compressedLength = (int)(sectorEnd - sectorOffset);
				if (compressedLength < 0)
				{
					Arrays.fill(buffer, offset, offset + ISectorDevice_Fields.sectorLength, (sbyte) 0);
				}
				else
				{
					sbyte[] compressedData = new sbyte[compressedLength];
					fileAccess.seek(sectorOffset);
					fileAccess.read(compressedData);

					try
					{
						Inflater inf = new Inflater(true);
						using (System.IO.Stream s = new InflaterInputStream(new System.IO.MemoryStream(compressedData), inf))
						{
							FileUtil.readAll(s, buffer, offset, ISectorDevice_Fields.sectorLength);
						}
					}
					catch (IOException)
					{
						throw new IOException(string.Format("Exception while uncompressing sector {0:D}", sectorNumber));
					}
				}
			}
		}
	}

}