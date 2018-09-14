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
	/// Unsigned long integer (64-bit) arithmetic.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class ULongInteger : AbstractNativeCodeSequence
	{
		public static void mult()
		{
			long a = getLong(GprA0, GprA1);
			long b = getLong(GprA2, GprA3);

			long result = a * b;

			GprV0V1 = result;
		}

		public static void div()
		{
			long a = getLong(GprA0, GprA1);
			long b = getLong(GprA2, GprA3);

			long result;
			if (a >= 0 && b >= 0)
			{
				// If both operands are positive, we can use normal "long" arithmetic
				result = a / b;
			}
			else
			{
				// If one of the operands is negative, we have to use BigInteger arithmetic.
				// Only BigInteger handles correctly value above 0x80000000000

				// Input bytes for BigInteger are assumed to be in big-endian byte-order.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] bytesA = new byte[8];
				sbyte[] bytesA = new sbyte[8];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] bytesB = new byte[8];
				sbyte[] bytesB = new sbyte[8];
				for (int i = 7; i >= 0; i--)
				{
					bytesA[i] = (sbyte) a;
					bytesB[i] = (sbyte) b;
					a >>= 8;
					b >>= 8;
				}

				// Create positive BigInteger values, the signum is always 1 for positive values.
				System.Numerics.BigInteger bigA = new BigInteger(1, bytesA);
				System.Numerics.BigInteger bigB = new BigInteger(1, bytesB);

				// Compute the division
				System.Numerics.BigInteger bigResult = bigA / bigB;

				// and convert the result to a long value.
				result = bigResult.longValue();
			}

			GprV0V1 = result;
		}
	}

}