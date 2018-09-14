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
	public class Memcmp : AbstractNativeCodeSequence
	{
		public static void call()
		{
			int src1Addr = GprA0;
			int src2Addr = GprA1;
			int n = GprA2;

			if (log.TraceEnabled)
			{
				log.trace(string.Format("memcmp src1={0}, src2={1}, n=0x{2:X}", Utilities.getMemoryDump(src1Addr, n), Utilities.getMemoryDump(src2Addr, n), n));
			}
			IMemoryReader memoryReader1 = MemoryReader.getMemoryReader(src1Addr, n, 1);
			IMemoryReader memoryReader2 = MemoryReader.getMemoryReader(src2Addr, n, 1);
			for (int i = 0; i < n; i++)
			{
				int c1 = memoryReader1.readNext();
				int c2 = memoryReader2.readNext();
				if (c1 != c2)
				{
					GprV0 = c1 - c2;
					return;
				}
			}

			GprV0 = 0;
		}

		public static void callOneMinusOne()
		{
			int src1Addr = GprA0;
			int src2Addr = GprA1;
			int n = GprA2;

			IMemoryReader memoryReader1 = MemoryReader.getMemoryReader(src1Addr, n, 1);
			IMemoryReader memoryReader2 = MemoryReader.getMemoryReader(src2Addr, n, 1);
			for (int i = 0; i < n; i++)
			{
				int c1 = memoryReader1.readNext();
				int c2 = memoryReader2.readNext();
				if (c1 != c2)
				{
					GprV0 = c1 < c2 ? -1 : 1;
					return;
				}
			}

			GprV0 = 0;
		}

		public static void call(int src1AddrReg, int src2AddrReg, int n, int resultReg, int equalValue, int notEqualValue)
		{
			int src1Addr = getRegisterValue(src1AddrReg);
			int src2Addr = getRegisterValue(src2AddrReg);

			IMemoryReader memoryReader1 = MemoryReader.getMemoryReader(src1Addr, n, 4);
			IMemoryReader memoryReader2 = MemoryReader.getMemoryReader(src2Addr, n, 4);
			for (int i = 0; i < n; i += 4)
			{
				int value1 = memoryReader1.readNext();
				int value2 = memoryReader2.readNext();
				if (value1 != value2)
				{
					setRegisterValue(resultReg, notEqualValue);
					return;
				}
			}

			setRegisterValue(resultReg, equalValue);
		}
	}

}