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
namespace pspsharp.Allegrex.compiler.nativeCode.arithmetic
{

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class Color : AbstractNativeCodeSequence
	{
		/// <summary>
		/// lv.q       C000.q, 0($s3)
		/// vsat0.q    C000.q, C000.q
		/// viim.s     S010.s, 0x00FF
		/// vscl.q     C000.q, C000.q, S010.s
		/// vf2iz.q    C000.q, C000.q, 23
		/// vi2uc.q    S000.s, C000.q
		/// mfv.s      $a1, S000.s
		/// </summary>
		public static void float2int(int source, int result)
		{
			int addr = getRegisterValue((source >> 21) & 31) + (int)unchecked((short)(source & 0xFFFC));

			int value1 = (int)(Float.intBitsToFloat(Memory.read32(addr)) * 255f);
			value1 = System.Math.Min(255, System.Math.Max(0, value1));
			int value2 = (int)(Float.intBitsToFloat(Memory.read32(addr + 4)) * 255f);
			value2 = System.Math.Min(255, System.Math.Max(0, value2));
			int value3 = (int)(Float.intBitsToFloat(Memory.read32(addr + 8)) * 255f);
			value3 = System.Math.Min(255, System.Math.Max(0, value3));
			int value4 = (int)(Float.intBitsToFloat(Memory.read32(addr + 12)) * 255f);
			value4 = System.Math.Min(255, System.Math.Max(0, value4));

			int value = value1 | (value2 << 8) | (value3 << 16) | (value4 << 24);

			setRegisterValue((result >> 16) & 31, value);
		}
	}

}