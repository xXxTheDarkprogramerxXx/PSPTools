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
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryWriter = pspsharp.memory.MemoryWriter;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class Memset : AbstractNativeCodeSequence
	{
		// Memset CodeBlock
		public static void call()
		{
			int dstAddr = GprA0;
			int c = GprA1 & 0xFF;
			int n = GprA2;

			Memory.memsetWithVideoCheck(dstAddr, (sbyte) c, n);

			GprV0 = dstAddr;
		}

		// Memset CodeSequence
		public static void call(int dstAddrReg, int cReg, int nReg)
		{
			call(dstAddrReg, cReg, nReg, 0);
		}

		// Memset CodeSequence
		public static void call(int dstAddrReg, int cReg, int nReg, int endValue)
		{
			int dstAddr = getRegisterValue(dstAddrReg);
			int c = getRegisterValue(cReg);
			int n = getRegisterValue(nReg) - endValue;

			Memory.memsetWithVideoCheck(dstAddr, (sbyte) c, n);

			setRegisterValue(dstAddrReg, dstAddr + n);
			setRegisterValue(nReg, endValue);
		}

		/// <summary>
		/// Set memory range to a fixed value </summary>
		/// <param name="dstAddrReg">	register number containing the start address </param>
		/// <param name="cReg">			register number containing the value </param>
		/// <param name="nStartReg">		register number giving the start value of the counter </param>
		/// <param name="nEndReg">		register number giving the end value of the counter </param>
		/// <param name="cLength">		2: take only the lower 16bit of the value
		///                      4: take the 32bit of the value </param>
		public static void call(int dstAddrReg, int cReg, int nStartReg, int cLength, int nEndReg)
		{
			int dstAddr = getRegisterValue(dstAddrReg);
			int c = getRegisterValue(cReg);
			int nStart = getRegisterValue(nStartReg);
			int nEnd = getRegisterValue(nEndReg);
			int n = nEnd - nStart;

			if (n == 0)
			{
				return;
			}

			if (cLength == 2)
			{
				// Both bytes identical?
				if ((c & 0xFF) == ((c >> 8) & 0xFF))
				{
					// This is equivalent to a normal memset
					Memory.memsetWithVideoCheck(dstAddr, (sbyte) c, n * 2);
				}
				else
				{
					// We have currently no built-in memset for 16bit values
					// do it manually...
					Memory mem = Memory;
					int value32 = (c & 0xFFFF) | (c << 16);
					short value16 = unchecked((short)(c & 0xFFFF));
					if (n > 0 && (dstAddr & 3) != 0)
					{
						mem.write16(dstAddr, value16);
						dstAddr += 2;
						n--;
					}
					IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(dstAddr, n * 2, 4);
					for (int i = 0; i < n; i += 2, dstAddr += 4)
					{
						memoryWriter.writeNext(value32);
					}
					memoryWriter.flush();
					if ((n & 1) != 0)
					{
						mem.write16(dstAddr, value16);
					}
				}
			}
			else if (cLength == 4)
			{
				// All bytes identical?
				if ((c & 0xFF) == ((c >> 8) & 0xFF) && (c & 0xFFFF) == ((c >> 16) & 0xFFFF))
				{
					// This is equivalent to a normal memset
					Memory.memsetWithVideoCheck(dstAddr, (sbyte) c, n * 4);
				}
				else
				{
					IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(dstAddr, n * 4, 4);
					for (int i = 0; i < n; i++)
					{
						memoryWriter.writeNext(c);
					}
					memoryWriter.flush();
				}
			}
			else
			{
				Compiler.Console.WriteLine("Memset.call: unsupported cLength=0x" + cLength.ToString("x"));
			}

			setRegisterValue(dstAddrReg, getRegisterValue(dstAddrReg) + n * cLength);
			setRegisterValue(nStartReg, nEnd);
		}

		// Memset CodeSequence
		public static void callWithStep(int dstAddrReg, int cReg, int nReg, int endValue, int direction, int step)
		{
			int dstAddr = getRegisterValue(dstAddrReg);
			int c = getRegisterValue(cReg);
			int n = (endValue - getRegisterValue(nReg)) * direction * step;

			Memory.memsetWithVideoCheck(dstAddr, (sbyte) c, n);

			setRegisterValue(dstAddrReg, dstAddr + n);
			setRegisterValue(nReg, endValue);
		}

		// Memset CodeSequence
		public static void callWithStepReg(int dstAddrReg, int cReg, int nReg, int endValueReg, int direction, int step)
		{
			int endValue = getRegisterValue(endValueReg);
			callWithStep(dstAddrReg, cReg, nReg, endValue, direction, step);
		}
	}

}