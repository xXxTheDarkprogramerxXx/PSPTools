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
	using MemoryReader = pspsharp.memory.MemoryReader;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class Strrchr : AbstractNativeCodeSequence
	{
		public static void call()
		{
			int srcAddr = GprA0;
			int c1 = GprA1 & 0xFF;
			int result = 0;

			IMemoryReader memoryReader = MemoryReader.getMemoryReader(srcAddr, 1);
			if (memoryReader != null)
			{
				for (int i = 0; true; i++)
				{
					int c2 = memoryReader.readNext();
					if (c1 == c2)
					{
						// Character found
						result = srcAddr + i;
					}
					else if (c2 == 0)
					{
						// End of Src string found
						break;
					}
				}
			}

			GprV0 = result;
		}
	}

}