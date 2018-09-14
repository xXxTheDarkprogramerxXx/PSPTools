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
namespace pspsharp.filesystems
{

	/// 
	/// <summary>
	/// @author gigaherz
	/// </summary>
	public abstract class SeekableInputStream : InputStream, SeekableDataInput
	{
		public abstract long FilePointer {get;}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract void seek(long position) throws java.io.IOException;
		public override abstract void seek(long position);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract long length() throws java.io.IOException;
		public override abstract long length();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract int read() throws java.io.IOException;
		public override abstract int read();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract byte readByte() throws java.io.IOException;
		public override abstract sbyte readByte();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract short readShort() throws java.io.IOException;
		public override abstract short readShort();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract int readInt() throws java.io.IOException;
		public override abstract int readInt();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract int readUnsignedByte() throws java.io.IOException;
		public override abstract int readUnsignedByte();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract int readUnsignedShort() throws java.io.IOException;
		public override abstract int readUnsignedShort();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract long readLong() throws java.io.IOException;
		public override abstract long readLong();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract float readFloat() throws java.io.IOException;
		public override abstract float readFloat();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract double readDouble() throws java.io.IOException;
		public override abstract double readDouble();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract boolean readBoolean() throws java.io.IOException;
		public override abstract bool readBoolean();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract char readChar() throws java.io.IOException;
		public override abstract char readChar();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract String readUTF() throws java.io.IOException;
		public override abstract string readUTF();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract String readLine() throws java.io.IOException;
		public override abstract string readLine();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract void readFully(byte[] b, int off, int len) throws java.io.IOException;
		public override abstract void readFully(sbyte[] b, int off, int len);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract void readFully(byte[] b) throws java.io.IOException;
		public override abstract void readFully(sbyte[] b);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public abstract int skipBytes(int bytes) throws java.io.IOException;
		public override abstract int skipBytes(int bytes);
	}
}