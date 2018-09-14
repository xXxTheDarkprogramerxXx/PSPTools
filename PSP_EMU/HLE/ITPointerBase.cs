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
namespace pspsharp.HLE
{

	public interface ITPointerBase
	{
		/// <summary>
		/// Equivalent to
		///    Memory.isAddressGood(getAddress()).
		/// </summary>
		/// <returns> true  if the pointer address is good/valid.
		///         false if the pointer address is not good/valid. </returns>
		bool AddressGood {get;}

		/// <summary>
		/// Tests if the pointer address is aligned on a given size.
		/// </summary>
		/// <param name="offset">  size of the alignment in bytes (e.g. 2 or 4) </param>
		/// <returns> true  if the pointer address is aligned on offset.
		///         false if the pointer address is not aligned on offset. </returns>
		bool isAlignedTo(int offset);

		/// <returns> the pointer address </returns>
		int Address {get;}

		/// <summary>
		/// Tests if the pointer address is NULL.
		/// Equivalent to
		///    getAddress() == 0
		/// </summary>
		/// <returns> true  if the pointer address is NULL.
		///         false if the pointer address is not NULL. </returns>
		bool Null {get;}

		/// <summary>
		/// Tests if the pointer address is not NULL.
		/// Equivalent to
		///    getAddress() != 0
		/// </summary>
		/// <returns> true  if the pointer address is not NULL.
		///         false if the pointer address is NULL. </returns>
		bool NotNull {get;}

		/// <summary>
		/// Returns the Memory instance.
		/// </summary>
		/// <returns> the Memory instance </returns>
		Memory Memory {get;}
	}

}