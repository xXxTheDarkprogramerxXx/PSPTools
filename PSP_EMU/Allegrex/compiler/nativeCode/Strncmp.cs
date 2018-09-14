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
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class Strncmp : AbstractNativeCodeSequence
	{
		public static void call()
		{
			int src1Addr = GprA0;
			int src2Addr = GprA1;
			int n = GprA2;

			if (log.TraceEnabled)
			{
				log.trace(string.Format("strncmp src1=0x{0:X8}('{1}'), src2=0x{2:X8}('{3}'), n=0x{4:X}", src1Addr, Utilities.readStringNZ(src1Addr, n), src2Addr, Utilities.readStringNZ(src2Addr, n), n));
			}

			if (n > 0)
			{
				IMemoryReader memoryReader1 = MemoryReader.getMemoryReader(src1Addr, n, 1);
				IMemoryReader memoryReader2 = MemoryReader.getMemoryReader(src2Addr, n, 1);

				if (memoryReader1 != null && memoryReader2 != null)
				{
					for (int i = 0; i < n; i++)
					{
						int c1 = memoryReader1.readNext();
						int c2 = memoryReader2.readNext();
						if (c1 != c2)
						{
							GprV0 = c1 - c2;
							return;
						}
						else if (c1 == 0)
						{
							// c1 == 0 and c2 == 0
							break;
						}
					}
				}
			}

			GprV0 = 0;
		}
	}

}