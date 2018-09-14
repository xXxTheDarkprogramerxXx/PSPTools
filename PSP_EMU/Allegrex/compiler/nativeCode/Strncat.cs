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
	public class Strncat : AbstractNativeCodeSequence
	{
		public static void call()
		{
			int dstAddr = GprA0;
			int srcAddr = GprA1;
			int length = GprA2;

			int dstLength = getStrlen(dstAddr);
			int srcLength = getStrlen(srcAddr, length);
			length = System.Math.Min(srcLength, length);
			Memory.memcpy(dstAddr + dstLength, srcAddr, length);
			Memory.write8(dstAddr + dstLength + length, (sbyte) 0);

			GprV0 = dstAddr;
		}
	}

}