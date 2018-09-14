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
namespace pspsharp.graphics.RE.software
{

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public interface IRenderer
	{
		/// <summary>
		/// Prepare the renderer so that the rendering can be
		/// performed asynchronously, possibly in a different thread.
		/// After the preparation, the context cannot be accessed any more.
		/// </summary>
		/// <returns>            true if something has to be rendered
		///                    false if nothing has to be rendered. It is not
		///                          valid to call render() when this prepare
		///                          method has returned false. </returns>
		bool prepare(GeContext context);

		/// <summary>
		/// Render the primitive. This method is only allowed to access class
		/// variables. The GeContext cannot be accessed.
		/// This method can be called asynchronously and in a different thread.
		/// </summary>
		void render();

		IRenderer duplicate();
	}

}