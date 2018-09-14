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
namespace pspsharp.graphics.RE
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.DirectBufferUtilities.getDirectBuffer;


	using GL15 = org.lwjgl.opengl.GL15;

	/// <summary>
	/// @author gid15
	/// 
	/// A RenderingEngine implementing calls to OpenGL using LWJGL
	/// for OpenGL Version >= 1.5.
	/// The class contains no rendering logic, it just implements the interface to LWJGL.
	/// </summary>
	public class RenderingEngineLwjgl15 : RenderingEngineLwjgl12
	{
		public RenderingEngineLwjgl15()
		{
		}

		public override void deleteBuffer(int buffer)
		{
			GL15.glDeleteBuffers(buffer);
		}

		public override int genBuffer()
		{
			return GL15.glGenBuffers();
		}

		public override void setBufferData(int target, int size, Buffer buffer, int usage)
		{
			if (buffer is ByteBuffer)
			{
				GL15.glBufferData(bufferTargetToGL[target], getDirectBuffer(size, (ByteBuffer) buffer), bufferUsageToGL[usage]);
			}
			else if (buffer is IntBuffer)
			{
				GL15.glBufferData(bufferTargetToGL[target], getDirectBuffer(size, (IntBuffer) buffer), bufferUsageToGL[usage]);
			}
			else if (buffer is ShortBuffer)
			{
				GL15.glBufferData(bufferTargetToGL[target], getDirectBuffer(size, (ShortBuffer) buffer), bufferUsageToGL[usage]);
			}
			else if (buffer is FloatBuffer)
			{
				GL15.glBufferData(bufferTargetToGL[target], getDirectBuffer(size, (FloatBuffer) buffer), bufferUsageToGL[usage]);
			}
			else if (buffer == null)
			{
				GL15.glBufferData(bufferTargetToGL[target], size, bufferUsageToGL[usage]);
			}
			else
			{
				throw new System.ArgumentException();
			}
		}

		public override void setBufferSubData(int target, int offset, int size, Buffer buffer)
		{
			if (buffer is ByteBuffer)
			{
				GL15.glBufferSubData(bufferTargetToGL[target], offset, getDirectBuffer(size, (ByteBuffer) buffer));
			}
			else if (buffer is IntBuffer)
			{
				GL15.glBufferSubData(bufferTargetToGL[target], offset, getDirectBuffer(size, (IntBuffer) buffer));
			}
			else if (buffer is ShortBuffer)
			{
				GL15.glBufferSubData(bufferTargetToGL[target], offset, getDirectBuffer(size, (ShortBuffer) buffer));
			}
			else if (buffer is FloatBuffer)
			{
				GL15.glBufferSubData(bufferTargetToGL[target], offset, getDirectBuffer(size, (FloatBuffer) buffer));
			}
			else
			{
				throw new System.ArgumentException();
			}
		}

		public override void bindBuffer(int target, int buffer)
		{
			GL15.glBindBuffer(bufferTargetToGL[target], buffer);
		}
	}

}