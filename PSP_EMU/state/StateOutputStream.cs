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
namespace pspsharp.state
{

	public class StateOutputStream : ObjectOutputStream
	{
		public const int NULL_ARRAY_LENGTH = -1;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateOutputStream(java.io.OutputStream out) throws java.io.IOException
		public StateOutputStream(System.IO.Stream @out) : base(@out)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeVersion(int version) throws java.io.IOException
		public virtual void writeVersion(int version)
		{
			writeInt(version);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeInts(int[] a) throws java.io.IOException
		public virtual void writeInts(int[] a)
		{
			writeInts(a, 0, a.Length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeIntsWithLength(int[] a) throws java.io.IOException
		public virtual void writeIntsWithLength(int[] a)
		{
			if (a == null)
			{
				writeInt(NULL_ARRAY_LENGTH);
			}
			else
			{
				writeInt(a.Length);
				writeInts(a, 0, a.Length);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeInts(int[] a, int offset, int Length) throws java.io.IOException
		public virtual void writeInts(int[] a, int offset, int Length)
		{
			for (int i = 0; i < Length; i++)
			{
				writeInt(a[i + offset]);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeFloats(float[] a) throws java.io.IOException
		public virtual void writeFloats(float[] a)
		{
			for (int i = 0; i < a.Length; i++)
			{
				writeFloat(a[i]);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeBooleans(boolean[] a) throws java.io.IOException
		public virtual void writeBooleans(bool[] a)
		{
			for (int i = 0; i < a.Length; i++)
			{
				writeBoolean(a[i]);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeBytes(byte[] a) throws java.io.IOException
		public virtual void writeBytes(sbyte[] a)
		{
			writeBytes(a, 0, a.Length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeBytesWithLength(byte[] a) throws java.io.IOException
		public virtual void writeBytesWithLength(sbyte[] a)
		{
			if (a == null)
			{
				writeInt(NULL_ARRAY_LENGTH);
			}
			else
			{
				writeInt(a.Length);
				writeBytes(a);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeBytes(byte[] a, int offset, int Length) throws java.io.IOException
		public virtual void writeBytes(sbyte[] a, int offset, int Length)
		{
			for (int i = 0; i < Length; i++)
			{
				writeByte(a[i + offset]);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeString(String s) throws java.io.IOException
		public virtual void writeString(string s)
		{
			writeObject(s);
		}
	}

}