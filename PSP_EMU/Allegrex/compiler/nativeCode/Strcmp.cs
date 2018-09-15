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
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class Strcmp : AbstractNativeCodeSequence
	{
		public static void call()
		{
			int str1 = GprA0;
			int str2 = GprA1;
			if (str1 == 0 || str2 == 0)
			{
				if (str1 == str2)
				{
					GprV0 = 0;
				}
				if (str1 != 0)
				{
					GprV0 = 1;
				}
				else
				{
					GprV0 = -1;
				}
			}
			else
			{
				if (!Memory.isAddressGood(str1))
				{
					Memory.invalidMemoryAddress(str1, "strcmp", Emulator.EMU_STATUS_MEM_READ);
					return;
				}
				if (!Memory.isAddressGood(str2))
				{
					Memory.invalidMemoryAddress(str2, "strcmp", Emulator.EMU_STATUS_MEM_READ);
					return;
				}

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("strcmp src1={0}, src2={1}", Utilities.getMemoryDump(str1, getStrlen(str1)), Utilities.getMemoryDump(str2, getStrlen(str2))));
				}

				GprV0 = strcmp(str1, str2);
			}
		}

		public static void call(int valueEqual, int valueLower, int valueHigher)
		{
			int str1 = GprA0;
			int str2 = GprA1;
			if (str1 == 0 || str2 == 0)
			{
				if (str1 == str2)
				{
					GprV0 = valueEqual;
				}
				if (str1 != 0)
				{
					GprV0 = valueHigher;
				}
				else
				{
					GprV0 = valueLower;
				}
			}
			else
			{
				if (!Memory.isAddressGood(str1))
				{
					Memory.invalidMemoryAddress(str1, "strcmp", Emulator.EMU_STATUS_MEM_READ);
					return;
				}
				if (!Memory.isAddressGood(str2))
				{
					Memory.invalidMemoryAddress(str2, "strcmp", Emulator.EMU_STATUS_MEM_READ);
					return;
				}

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("strcmp src1={0}, src2={1}", Utilities.getMemoryDump(str1, getStrlen(str1)), Utilities.getMemoryDump(str2, getStrlen(str2))));
				}

				int cmp = strcmp(str1, str2);
				if (cmp < 0)
				{
					GprV0 = valueLower;
				}
				else if (cmp > 0)
				{
					GprV0 = valueHigher;
				}
				else
				{
					GprV0 = valueEqual;
				}
			}
		}
	}

}