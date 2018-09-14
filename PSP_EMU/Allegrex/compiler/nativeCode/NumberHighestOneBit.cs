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
	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class NumberHighestOneBit : AbstractNativeCodeSequence
	{
		private static readonly int[] precomputedSpecial1 = new int[256];

		static NumberHighestOneBit()
		{
			for (int n = 0; n < precomputedSpecial1.Length; n++)
			{
				precomputedSpecial1[n] = computeSpecial1(n);
			}
		}

		public static void call(int valueReg, int bitReg)
		{
			int value = getRegisterValue(valueReg);
			int bit = getRegisterValue(bitReg);

			for (; bit > 0; bit--)
			{
				if ((((int)((uint)value >> bit)) & 1) != 0)
				{
					break;
				}
			}

			setRegisterValue(bitReg, bit);
		}

		private static int computeSpecial1(int n)
		{
			if ((n & 255) == 0)
			{
				return 0;
			}
			if ((n & 128) == 0)
			{
				return 1;
			}
			if ((n & 64) == 0)
			{
				return 0;
			}
			if ((n & 32) == 0)
			{
				return 2;
			}
			if ((n & 16) == 0)
			{
				return 3;
			}
			if ((n & 8) == 0)
			{
				return 4;
			}
			return 0;
		}

		public static void callSpecial1(int valueReg)
		{
			int value = getRegisterValue(valueReg);
			int result = precomputedSpecial1[value & 0xFF];
			GprV0 = result;
		}
	}

}