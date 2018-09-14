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
namespace pspsharp.graphics.RE.buffer
{

	/// <summary>
	/// @author gid15
	/// 
	/// The interface for a RenderingEngine buffer manager.
	/// </summary>
	public interface IREBufferManager
	{
		IRenderingEngine RenderingEngine {set;}
		bool useVBO();
		int genBuffer(int target, int type, int size, int usage);
		void bindBuffer(int target, int buffer);
		void deleteBuffer(int buffer);
		ByteBuffer getBuffer(int buffer);
		void setTexCoordPointer(int buffer, int size, int type, int stride, int offset);
		void setColorPointer(int buffer, int size, int type, int stride, int offset);
		void setVertexPointer(int buffer, int size, int type, int stride, int offset);
		void setNormalPointer(int buffer, int type, int stride, int offset);
		void setWeightPointer(int buffer, int size, int type, int stride, int offset);
		void setVertexAttribPointer(int buffer, int id, int size, int type, bool normalized, int stride, int offset);
		void setBufferData(int target, int buffer, int size, Buffer data, int usage);
		void setBufferSubData(int target, int buffer, int offset, int size, Buffer data, int usage);
	}

}