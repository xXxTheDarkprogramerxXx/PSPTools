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
	public class MathHalfFloatToFloat : AbstractNativeCodeSequence
	{
		public static void call()
		{
			int halfFloat = GprA0;
			int e = (halfFloat >> 10) & 0x1F;

			if ((halfFloat & 0x7FFF) != 0)
			{
				e += 112;
			}

			FprF0 = Float.intBitsToFloat(((halfFloat & 0x8000) << 16) | (e << 23) | ((halfFloat & 0x03FF) << 13));
		}
	}

}