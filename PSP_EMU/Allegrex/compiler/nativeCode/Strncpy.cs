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
	public class Strncpy : AbstractNativeCodeSequence
	{
		public static void call()
		{
			int dstAddr = GprA0;
			int srcAddr = GprA1;
			int n = GprA2;

			int srcLength = getStrlen(srcAddr);
			if (srcLength < n)
			{
				Memory.memcpy(dstAddr, srcAddr, srcLength + 1);
				Memory.memset(dstAddr + srcLength + 1, (sbyte) 0, n - srcLength - 1);
			}
			else
			{
				Memory.memcpy(dstAddr, srcAddr, n);
			}

			GprV0 = dstAddr;
		}
	}

}