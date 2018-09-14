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
	public class MultDiv : AbstractNativeCodeSequence
	{
		public static void call()
		{
			int mult1 = GprA0;
			int mult2 = GprA1;
			int div1 = GprA2;
			int result = (int)((mult1 * (long) mult2) / div1);
			GprV0 = result;
		}
	}

}