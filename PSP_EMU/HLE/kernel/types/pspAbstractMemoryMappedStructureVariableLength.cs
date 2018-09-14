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
namespace pspsharp.HLE.kernel.types
{
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// Abstract class representing a memory based structure starting
	/// with a 32-bit value indicating the maximum memory length available
	/// for the structure values.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public abstract class pspAbstractMemoryMappedStructureVariableLength : pspAbstractMemoryMappedStructure
	{
		private int length;

		protected internal override void read()
		{
			readLength();
		}

		protected internal override void write()
		{
			readLength();
		}

		private void readLength()
		{
			length = read32();
			MaxSize = length;
		}

		public override int @sizeof()
		{
			return length;
		}

		public override string ToString()
		{
			return Utilities.getMemoryDump(BaseAddress, length);
		}
	}

}