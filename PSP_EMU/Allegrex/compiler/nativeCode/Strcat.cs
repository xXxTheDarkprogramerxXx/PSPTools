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
	public class Strcat : AbstractNativeCodeSequence
	{
		public static void call()
		{
			int dstAddr = GprA0;
			int srcAddr = GprA1;

			int dstLength = getStrlen(dstAddr);
			int srcLength = getStrlen(srcAddr);
			Memory.memcpy(dstAddr + dstLength, srcAddr, srcLength + 1);

			GprV0 = dstAddr;
		}
	}

}