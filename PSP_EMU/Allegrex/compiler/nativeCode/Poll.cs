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
	public class Poll : AbstractNativeCodeSequence
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void poll1(int address1, int address2) throws pspsharp.Allegrex.compiler.StopThreadException
		public static void poll1(int address1, int address2)
		{
			int address = getRelocatedAddress(address1, address2);

			int value;
			do
			{
				value = Memory.read8(address);
				if (value == 0)
				{
					if (RuntimeContext.wantSync)
					{
						RuntimeContext.sync();
					}
					else
					{
						Utilities.sleep(1);
					}
				}
			} while (value == 0);

			GprV0 = value;
		}
	}

}