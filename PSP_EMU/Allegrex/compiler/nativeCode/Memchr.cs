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
	public class Memchr : AbstractNativeCodeSequence
	{
		public static void call()
		{
			int srcAddr = GprA0;
			int c1 = GprA1 & 0xFF;
			int n = GprA2;

			IMemoryReader memoryReader = MemoryReader.getMemoryReader(srcAddr, n, 1);
			if (memoryReader != null)
			{
				for (int i = 0; i < n; i++)
				{
					int c2 = memoryReader.readNext();
					if (c1 == c2)
					{
						GprV0 = srcAddr + i;
						return;
					}
				}
			}

			GprV0 = 0;
		}

		// Returns index of char found or "n" if not found
		public static void call(int srcAddrReg, int cReg, int nReg)
		{
			int srcAddr = getRegisterValue(srcAddrReg);
			int c1 = getRegisterValue(cReg) & 0xFF;
			int n = getRegisterValue(nReg);

			IMemoryReader memoryReader = MemoryReader.getMemoryReader(srcAddr, n, 1);
			if (memoryReader != null)
			{
				for (int i = 0; i < n; i++)
				{
					int c2 = memoryReader.readNext();
					if (c1 == c2)
					{
						GprV0 = i;
						return;
					}
				}
			}

			GprV0 = n;
		}
	}

}