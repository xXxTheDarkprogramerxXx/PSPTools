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

	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class MemoryInputStream : InputStream
	{
		private IMemoryReader memoryReader;

		public MemoryInputStream(int address)
		{
			memoryReader = MemoryReader.getMemoryReader(address, 1);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int read() throws java.io.IOException
		public override int read()
		{
			return memoryReader.readNext();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int read(byte[] buffer, int offset, int Length) throws java.io.IOException
		public override int read(sbyte[] buffer, int offset, int Length)
		{
			for (int i = 0; i < Length; i++)
			{
				buffer[offset + i] = (sbyte) memoryReader.readNext();
			}

			return Length;
		}
	}

}