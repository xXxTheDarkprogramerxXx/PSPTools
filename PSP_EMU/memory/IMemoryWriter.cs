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
	public interface IMemoryWriter
	{
		/// <summary>
		/// Writes the next value to memory.
		/// The MemoryWriter can buffer the values before actually writing to the
		/// Memory.
		/// 
		/// MemoryWriters are created by calling the factory
		///   MemoryWriter.getMemoryWriter(...)
		/// 
		/// When writing 8-bit values, only the lowest 8-bits of value are used.
		/// When writing 16-bit values, only the lowest 16-bits of value are used.
		/// 
		/// When the last value has been written, the flush()
		/// method has to be called in order to write any value buffered by the
		/// MemoryWriter.
		/// </summary>
		/// <param name="value"> the value to be written. </param>
		void writeNext(int value);

		/// <summary>
		/// Skip n values when writing to memory.
		/// 
		/// When writing 32-bit values, the next 4*n bytes are skipped and left unchanged.
		/// When writing 16-bit values, the next 2*n bytes are skipped and left unchanged.
		/// When writing 8-bit values, the next n bytes are skipped and left unchanged.
		/// </summary>
		/// <param name="n"> the number of values to be skipped. </param>
		void skip(int n);

		/// <summary>
		/// Write any value buffered by the MemoryWriter.
		/// This method has to be called when all the values has been written,
		/// as the last call to the MemoryWriter.
		/// After calling flush(), it is no longer allowed to call writeNext().
		/// </summary>
		void flush();

		int CurrentAddress {get;}
	}

}