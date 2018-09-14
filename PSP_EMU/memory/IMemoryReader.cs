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
namespace pspsharp.memory
{
	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public interface IMemoryReader
	{
		/// <summary>
		/// Reads the next value from memory.
		/// 
		/// When reading 8-bit or 16-bit values, an unsigned value is returned
		/// (i.e. masked using 0xFF or 0xFFFF).
		/// 
		/// MemoryReaders are created using the factory
		///   MemoryReader.getMemoryReader(...)
		/// </summary>
		/// <returns> the next value from memory. </returns>
		int readNext();

		/// <summary>
		/// Skip n values when reading from memory.
		/// 
		/// When reading 32-bit values, the next 4*n bytes are skipped.
		/// When reading 16-bit values, the next 2*n bytes are skipped.
		/// When reading 8-bit values, the next n bytes are skipped.
		/// </summary>
		/// <param name="n"> the number of values to be skipped. </param>
		void skip(int n);

		int CurrentAddress {get;}
	}

}