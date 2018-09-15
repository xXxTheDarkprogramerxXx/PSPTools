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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.state.StateOutputStream.NULL_ARRAY_LENGTH;


	public class StateInputStream : ObjectInputStream
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public StateInputStream(java.io.InputStream in) throws java.io.IOException
		public StateInputStream(System.IO.Stream @in) : base(@in)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int readVersion(int maxVersion) throws java.io.IOException
		public virtual int readVersion(int maxVersion)
		{
			int version = readInt();
			if (version > maxVersion)
			{
				throw new InvalidStateException(string.Format("Unsupported State version {0:D}(maxVersion={1:D})", version, maxVersion));
			}

			return version;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readInts(int[] a) throws java.io.IOException
		public virtual void readInts(int[] a)
		{
			readInts(a, 0, a.Length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int[] readIntsWithLength() throws java.io.IOException
		public virtual int[] readIntsWithLength()
		{
			int Length = readInt();
			if (Length == NULL_ARRAY_LENGTH)
			{
				return null;
			}
			int[] a = new int[Length];
			readInts(a);

			return a;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readInts(int[] a, int offset, int Length) throws java.io.IOException
		public virtual void readInts(int[] a, int offset, int Length)
		{
			for (int i = 0; i < Length; i++)
			{
				a[i + offset] = readInt();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readFloats(float[] a) throws java.io.IOException
		public virtual void readFloats(float[] a)
		{
			for (int i = 0; i < a.Length; i++)
			{
				a[i] = readFloat();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readBooleans(boolean[] a) throws java.io.IOException
		public virtual void readBooleans(bool[] a)
		{
			for (int i = 0; i < a.Length; i++)
			{
				a[i] = readBoolean();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readBytes(byte[] a) throws java.io.IOException
		public virtual void readBytes(sbyte[] a)
		{
			readBytes(a, 0, a.Length);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readBytes(byte[] a, int offset, int Length) throws java.io.IOException
		public virtual void readBytes(sbyte[] a, int offset, int Length)
		{
			for (int i = 0; i < Length; i++)
			{
				a[i + offset] = readByte();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte[] readBytesWithLength() throws java.io.IOException
		public virtual sbyte[] readBytesWithLength()
		{
			int Length = readInt();
			if (Length == NULL_ARRAY_LENGTH)
			{
				return null;
			}
			sbyte[] a = new sbyte[Length];
			readBytes(a);

			return a;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String readString() throws java.io.IOException
		public virtual string readString()
		{
			try
			{
				object a = readObject();
				if (a == null)
				{
					return null;
				}
				return a.ToString();
			}
			catch (ClassNotFoundException e)
			{
				Emulator.Console.WriteLine("readString", e);
				return null;
			}
		}
	}

}