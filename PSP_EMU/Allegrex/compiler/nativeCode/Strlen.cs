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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._a0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._a1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._a2;

	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class Strlen : AbstractNativeCodeSequence
	{

		public static void call()
		{
			int srcAddr = GprA0;

			int srcLength = getStrlen(srcAddr);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("strlen src=0x{0:X8}('{1}') returning 0x{2:X}", srcAddr, Utilities.readStringZ(srcAddr), srcLength));
			}

			GprV0 = srcLength;
			Cpu._a0 = srcAddr + srcLength;
			Cpu._a1 = srcAddr;
			Cpu._a2 = 0;
		}

		public static void call(int lastUpdatedRegister)
		{
			int srcAddr = GprA0;

			int srcLength = getStrlen(srcAddr);
			GprV0 = srcLength;

			// Some games are also assuming that the other registers
			// have been modified... dirty programming
			if (lastUpdatedRegister >= _a0)
			{
				Cpu._a0 = srcAddr + srcLength;
				if (lastUpdatedRegister >= _a1)
				{
					Cpu._a1 = srcAddr;
					if (lastUpdatedRegister >= _a2)
					{
						Cpu._a2 = 0;
					}
				}
			}
		}
	}

}