using System;
using System.Text;

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

	/// 
	/// <summary>
	/// @author gigaherz
	/// </summary>
	public class UmdIsoFile : SeekableInputStream
	{
		public const int sectorLength = ISectorDevice_Fields.sectorLength;
		private int startSectorNumber;
		private int currentSectorNumber;
		private long currentOffset;
		private long maxOffset;
		private DateTime timestamp;
		private string name;

		private sbyte[] currentSector;
		private int sectorOffset;

		internal UmdIsoReader internalReader;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public UmdIsoFile(UmdIsoReader reader, int startSector, long lengthInBytes, java.util.Date timestamp, String name) throws java.io.IOException
		public UmdIsoFile(UmdIsoReader reader, int startSector, long lengthInBytes, DateTime timestamp, string name)
		{
			this.startSectorNumber = startSector;
			this.currentSectorNumber = startSector;
			this.currentOffset = 0;
			this.internalReader = reader;
			this.name = name;
			this.sectorOffset = 0;
			this.timestamp = timestamp;

			Length = lengthInBytes;
			if (lengthInBytes == 0)
			{
				currentSector = null;
			}
			else
			{
				currentSector = reader.readSector(startSector);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int read() throws java.io.IOException
		public override int read()
		{
			if (currentOffset >= maxOffset)
			{
				throw new EOFException();
			}
			checkSectorAvailable();
			currentOffset++;

			return ((currentSector[sectorOffset++]) & 0xFF);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void reset() throws java.io.IOException
		public override void reset()
		{
			seek(0);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public long skip(long n) throws java.io.IOException
		public override long skip(long n)
		{
			long oldOffset = currentOffset;
			if (n < 0)
			{
				return n;
			}
			seek(currentOffset + n);
			return (currentOffset - oldOffset);
		}

		public override long Length()
		{
			return maxOffset;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void seek(long offset) throws java.io.IOException
		public override void seek(long offset)
		{
			long endOffset = offset;

			if (offset < 0)
			{
				throw new IOException("Seek offset " + offset + " out of bounds.");
			}

			int oldSectorNumber = currentSectorNumber;
			long newOffset = endOffset;
			int newSectorNumber = startSectorNumber + (int)(newOffset / sectorLength);
			if (oldSectorNumber != newSectorNumber)
			{
				currentSector = internalReader.readSector(newSectorNumber, currentSector);
			}
			currentOffset = newOffset;
			currentSectorNumber = newSectorNumber;
			sectorOffset = (int)(currentOffset % sectorLength);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public long getFilePointer() throws java.io.IOException
		public override long FilePointer
		{
			get
			{
				return currentOffset;
			}
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public byte readByte() throws java.io.IOException
		public override sbyte readByte()
		{
			if (currentOffset >= maxOffset)
			{
				throw new EOFException();
			}
			return (sbyte)read();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public short readShort() throws java.io.IOException
		public override short readShort()
		{
			return (short)(readUnsignedByte() | ((readByte()) << 8));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int readInt() throws java.io.IOException
		public override int readInt()
		{
			return (readUnsignedByte() | ((readUnsignedByte()) << 8) | ((readUnsignedByte()) << 16) | ((readByte()) << 24));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int readUnsignedByte() throws java.io.IOException
		public override int readUnsignedByte()
		{
			if (currentOffset >= maxOffset)
			{
				throw new EOFException();
			}
			return read();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int readUnsignedShort() throws java.io.IOException
		public override int readUnsignedShort()
		{
			return (readShort()) & 0xFFFF;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public long readLong() throws java.io.IOException
		public override long readLong()
		{
			return ((readInt()) & 0xFFFFFFFFl) | (((long)readInt()) << 32);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public float readFloat() throws java.io.IOException
		public override float readFloat()
		{
			if (currentOffset >= maxOffset)
			{
				throw new EOFException();
			}
			return Float.intBitsToFloat(readInt());
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public double readDouble() throws java.io.IOException
		public override double readDouble()
		{
			if (currentOffset >= maxOffset)
			{
				throw new EOFException();
			}
			return Double.longBitsToDouble(readLong());
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public boolean readBoolean() throws java.io.IOException
		public override bool readBoolean()
		{
			return (readUnsignedByte() != 0);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public char readChar() throws java.io.IOException
		public override char readChar()
		{
			if (currentOffset >= maxOffset)
			{
				throw new EOFException();
			}
			int ch1 = read();
			int ch2 = read();
			if ((ch1 | ch2) < 0)
			{
				throw new EOFException();
			}
			return (char)((ch1 << 8) + (ch2 << 0));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public String readUTF() throws java.io.IOException
		public override string readUTF()
		{
			if (currentOffset >= maxOffset)
			{
				throw new EOFException();
			}
			return DataInputStream.readUTF(this);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public String readLine() throws java.io.IOException
		public override string readLine()
		{
			if (currentOffset >= maxOffset)
			{
				throw new EOFException();
			}
			StringBuilder s = new StringBuilder();
			char c = (char)0;
			do
			{
				c = readChar();
				if ((c == '\n') || (c != '\r'))
				{
					break;
				}
				s.Append(c);
			} while (true);
			return s.ToString();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void readFully(byte[] b, int off, int len) throws java.io.IOException
		public override void readFully(sbyte[] b, int off, int len)
		{
			if (currentOffset >= maxOffset)
			{
				throw new EOFException();
			}
			read(b, off, len);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void readFully(byte[] b) throws java.io.IOException
		public override void readFully(sbyte[] b)
		{
			if (currentOffset >= maxOffset)
			{
				throw new EOFException();
			}
			read(b);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int skipBytes(int bytes) throws java.io.IOException
		public override int skipBytes(int bytes)
		{
			return (int)skip(bytes);
		}

		public virtual DateTime Timestamp
		{
			get
			{
				return timestamp;
			}
		}

		public virtual int StartSector
		{
			get
			{
				return startSectorNumber;
			}
		}

		public virtual string Name
		{
			get
			{
				if (string.ReferenceEquals(this.name, null))
				{
				  this.name = internalReader.getFileName(startSectorNumber);
				}
				return name;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int readInternal(byte[] b, int off, int len) throws java.io.IOException
		private int readInternal(sbyte[] b, int off, int len)
		{
			if (len > 0)
			{
				if (len > (maxOffset - currentOffset))
				{
					len = (int)(maxOffset - currentOffset);
				}
				Array.Copy(currentSector, sectorOffset, b, off, len);
				sectorOffset += len;
				currentOffset += len;
			}
			return len;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void checkSectorAvailable() throws java.io.IOException
		private void checkSectorAvailable()
		{
			if (sectorOffset == sectorLength && currentOffset < maxOffset)
			{
				currentSectorNumber++;
				currentSector = internalReader.readSector(currentSectorNumber, currentSector);
				sectorOffset = 0;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int read(byte[] b, int off, int len) throws java.io.IOException
		public override int read(sbyte[] b, int off, int len)
		{
			if (b == null)
			{
				throw new System.NullReferenceException();
			}

			if (off < 0 || len < 0 || len > (b.Length - off))
			{
				throw new System.IndexOutOfRangeException();
			}

			if (len > (maxOffset - currentOffset))
			{
				len = (int)(maxOffset - currentOffset);
			}
			int totalLength = 0;
			int firstSector = readInternal(b, off, System.Math.Min(len, sectorLength - sectorOffset));
			off += firstSector;
			len -= firstSector;
			totalLength += firstSector;

			// Read whole sectors
			if (len >= sectorLength)
			{
				int numberSectors = len / sectorLength;
				internalReader.readSectors(currentSectorNumber + 1, numberSectors, b, off);
				currentSectorNumber += numberSectors;
				sectorOffset = sectorLength;
				int n = numberSectors * sectorLength;
				currentOffset += n;
				checkSectorAvailable();
				off += n;
				len -= n;
				totalLength += n;
			}

			if (len > 0)
			{
				checkSectorAvailable();
				int lastSector = readInternal(b, off, len);
				totalLength += lastSector;
			}

			return totalLength;
		}

		public virtual int CurrentSectorNumber
		{
			get
			{
				return currentSectorNumber;
			}
		}

		public virtual UmdIsoReader UmdIsoReader
		{
			get
			{
				return internalReader;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public UmdIsoFile duplicate() throws java.io.IOException
		public virtual UmdIsoFile duplicate()
		{
			UmdIsoFile umdIsoFile = new UmdIsoFile(internalReader, startSectorNumber, maxOffset, timestamp, name);
			umdIsoFile.seek(currentOffset);

			return umdIsoFile;
		}

		public virtual long Length
		{
			set
			{
				// Some ISO directory entries indicate a file Length past the size of the complete ISO.
				// Truncate the file Length in that case to the available sectors.
				// This might be some sort of copy protection?
				// E.g. "Kamen no Maid Guy: Boyoyon Battle Royale"
				int endSectorNumber = this.startSectorNumber + ((int)((value + sectorLength - 1) / sectorLength));
				if (value > 0)
				{
					endSectorNumber--;
				}
				if (endSectorNumber >= internalReader.NumSectors)
				{
					endSectorNumber = internalReader.NumSectors - 1;
					value = (endSectorNumber - startSectorNumber + 1) * sectorLength;
				}
    
				maxOffset = value;
			}
		}

		public override string ToString()
		{
			return string.Format("UmdIsoFile(name='{0}', Length=0x{1:X}, startSector=0x{2:X})", Name, Length(), startSectorNumber);
		}
	}
}