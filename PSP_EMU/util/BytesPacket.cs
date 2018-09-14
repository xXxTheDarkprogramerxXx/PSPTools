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
namespace pspsharp.util
{

	public class BytesPacket
	{
		private sbyte[] buffer;
		private int offset;
		private int length;
		private bool littleEndian;
		private int bufferBits;
		private int bit;

		public BytesPacket(int length)
		{
			buffer = new sbyte[length];
			this.length = length;
		}

		public BytesPacket(sbyte[] buffer)
		{
			this.buffer = buffer;
			length = buffer.Length;
		}

		public BytesPacket(sbyte[] buffer, int length)
		{
			this.buffer = buffer;
			this.length = length;
		}

		public BytesPacket(sbyte[] buffer, int offset, int length)
		{
			this.buffer = buffer;
			this.offset = offset;
			this.length = length;
		}

		public virtual void setLittleEndian()
		{
			littleEndian = true;
		}

		public virtual void setBigEndian()
		{
			littleEndian = false;
		}

		public virtual sbyte[] Buffer
		{
			get
			{
				return buffer;
			}
		}

		public virtual int Offset
		{
			get
			{
				return offset;
			}
		}

		public virtual int Length
		{
			get
			{
				return length;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte readByte() throws java.io.EOFException
		public virtual sbyte readByte()
		{
			if (length <= 0)
			{
				throw new EOFException();
			}

			length--;
			return buffer[offset++];
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read8() throws java.io.EOFException
		public virtual int read8()
		{
			return readByte() & 0xFF;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read16() throws java.io.EOFException
		public virtual int read16()
		{
			if (littleEndian)
			{
				return read8() | (read8() << 8);
			}

			return (read8() << 8) | read8();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read32() throws java.io.EOFException
		public virtual int read32()
		{
			if (littleEndian)
			{
				return read8() | (read8() << 8) | (read8() << 16) | (read8() << 24);
			}

			return (read8() << 24) | (read8() << 16) | (read8() << 8) | read8();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read1() throws java.io.EOFException
		public virtual int read1()
		{
			if (bit <= 0)
			{
				bufferBits = read8();
				bit = 8;
			}

			bit--;
			return (bufferBits >> bit) & 1;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readBits(int n) throws java.io.EOFException
		public virtual int readBits(int n)
		{
			if (n <= bit)
			{
				bit -= n;
				return (bufferBits >> bit) & ((1 << n) - 1);
			}

			int value = 0;
			for (int i = 0; i < n; i++)
			{
				value = (value << 1) | read1();
			}

			return value;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean readBoolean() throws java.io.EOFException
		public virtual bool readBoolean()
		{
			return read1() != 0;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte[] readBytes(byte[] dataBuffer) throws java.io.EOFException
		public virtual sbyte[] readBytes(sbyte[] dataBuffer)
		{
			return readBytes(dataBuffer, 0, dataBuffer.Length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte[] readBytes(int dataLength) throws java.io.EOFException
		public virtual sbyte[] readBytes(int dataLength)
		{
			return readBytes(new sbyte[dataLength], 0, dataLength);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte[] readBytes(byte[] dataBuffer, int dataOffset, int dataLength) throws java.io.EOFException
		public virtual sbyte[] readBytes(sbyte[] dataBuffer, int dataOffset, int dataLength)
		{
			for (int i = 0; i < dataLength; i++)
			{
				dataBuffer[dataOffset + i] = readByte();
			}

			return dataBuffer;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public char readAsciiChar() throws java.io.EOFException
		public virtual char readAsciiChar()
		{
			return (char) read8();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void skip8() throws java.io.EOFException
		public virtual void skip8()
		{
			skip8(1);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void skip8(int n) throws java.io.EOFException
		public virtual void skip8(int n)
		{
			if (n > 0)
			{
				if (length < n)
				{
					offset += length;
					length = 0;
					throw new EOFException();
				}

				offset += n;
				length -= n;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String readStringNZ(int n) throws java.io.EOFException
		public virtual string readStringNZ(int n)
		{
			StringBuilder s = new StringBuilder();

			while (n > 0)
			{
				n--;
				int c = read8();
				if (c == 0)
				{
					break;
				}
				s.Append((char) c);
			}
			skip8(n);

			return s.ToString();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeByte(byte b) throws java.io.EOFException
		public virtual void writeByte(sbyte b)
		{
			if (length <= 0)
			{
				throw new EOFException();
			}

			length--;
			buffer[offset++] = b;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeBytesZero(int n) throws java.io.EOFException
		public virtual void writeBytesZero(int n)
		{
			for (; n > 0; n--)
			{
				writeByte((sbyte) 0);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write8(int n) throws java.io.EOFException
		public virtual void write8(int n)
		{
			writeByte(unchecked((sbyte)(n & 0xFF)));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write16(int n) throws java.io.EOFException
		public virtual void write16(int n)
		{
			if (littleEndian)
			{
				write8(n);
				write8(n >> 8);
			}
			else
			{
				write8(n >> 8);
				write8(n);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write32(int n) throws java.io.EOFException
		public virtual void write32(int n)
		{
			if (littleEndian)
			{
				write8(n);
				write8(n >> 8);
				write8(n >> 16);
				write8(n >> 24);
			}
			else
			{
				write8(n >> 24);
				write8(n >> 16);
				write8(n >> 8);
				write8(n);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeBytes(byte[] dataBuffer) throws java.io.EOFException
		public virtual void writeBytes(sbyte[] dataBuffer)
		{
			if (dataBuffer != null)
			{
				writeBytes(dataBuffer, 0, dataBuffer.Length);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeBytes(byte[] dataBuffer, int dataOffset, int dataLength) throws java.io.EOFException
		public virtual void writeBytes(sbyte[] dataBuffer, int dataOffset, int dataLength)
		{
			if (dataBuffer != null)
			{
				int copyLength = System.Math.Min(dataLength, dataBuffer.Length - dataOffset);
				for (int i = 0; i < copyLength; i++)
				{
					writeByte(dataBuffer[dataOffset + i]);
				}
				dataLength -= copyLength;
			}

			writeBytesZero(dataLength);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write1(int n) throws java.io.EOFException
		public virtual void write1(int n)
		{
			if (bit == 0)
			{
				bufferBits = 0;
			}
			bit++;
			bufferBits |= (n & 1) << (8 - bit);

			if (bit == 8)
			{
				write8(bufferBits);
				bit = 0;
				bufferBits = 0;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeBoolean(boolean b) throws java.io.EOFException
		public virtual void writeBoolean(bool b)
		{
			write1(b ? 1 : 0);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeBits(int b, int n) throws java.io.EOFException
		public virtual void writeBits(int b, int n)
		{
			for (int i = n - 1; i >= 0; i--)
			{
				write1(b >> i);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeAsciiChar(char c) throws java.io.EOFException
		public virtual void writeAsciiChar(char c)
		{
			writeByte((sbyte) c);
		}

		public virtual void rewind(int newOffset)
		{
			if (newOffset < offset)
			{
				length += offset - newOffset;
				offset = newOffset;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeStringNZ(String s, int n) throws java.io.EOFException
		public virtual void writeStringNZ(string s, int n)
		{
			if (!string.ReferenceEquals(s, null))
			{
				int copyLength = System.Math.Min(s.Length, n);
				for (int i = 0; i < copyLength; i++)
				{
					writeAsciiChar(s[i]);
				}
				n -= copyLength;
			}

			writeBytesZero(n);
		}
	}

}