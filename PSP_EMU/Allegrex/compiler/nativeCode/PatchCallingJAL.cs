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
//	import static pspsharp.Allegrex.Common._ra;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class PatchCallingJAL : AbstractNativeCodeSequence
	{
		/*
		 * This method is very frequently used by the gpSP homebrew.
		 * Try to optimize it to avoid too much recompilations.
		 */
		public static int call(int addressReg)
		{
			Memory mem = Memory;

			int patchAddr = getRegisterValue(_ra) - 8;

			int jumpAddr = getRegisterValue(addressReg);
			int opcode = (AllegrexOpcodes.JAL << 26) | ((jumpAddr >> 2) & 0x03FFFFFF);
			mem.write32(patchAddr, opcode);

			int delaySlotOpcode = mem.read32(patchAddr + 4);
			interpret(delaySlotOpcode);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("PatchCallingJAL at 0x{0:X8} to 0x{1:X8}", patchAddr, jumpAddr));
			}

			return jumpAddr;
		}
	}

}