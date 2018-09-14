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
namespace pspsharp.Allegrex.compiler.nativeCode
{
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class Strupr : AbstractNativeCodeSequence
	{
		public static void callCopyWithLength(int errorCode1, int errorCode2)
		{
			int srcAddr = GprA0;
			int dstAddr = GprA1;
			int length = GprA2;

			int lengthSrc = getStrlen(srcAddr);
			if (lengthSrc > length)
			{
				GprV0 = (errorCode1 << 16) | errorCode2;
				return;
			}

			IMemoryReader memoryReader = MemoryReader.getMemoryReader(srcAddr, length, 1);
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(dstAddr, length, 1);
			for (int i = 0; i < lengthSrc; i++)
			{
				int c = toUpperCase[memoryReader.readNext()];
				memoryWriter.writeNext(c);
			}
			memoryWriter.writeNext(0);
			memoryWriter.flush();
		}
	}

}