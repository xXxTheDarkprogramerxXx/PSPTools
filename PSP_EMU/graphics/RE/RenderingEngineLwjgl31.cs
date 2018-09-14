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
	using GL15 = org.lwjgl.opengl.GL15;
	using GL30 = org.lwjgl.opengl.GL30;
	using GL31 = org.lwjgl.opengl.GL31;

	/// <summary>
	/// @author gid15
	/// 
	/// A RenderingEngine implementing calls to OpenGL using LWJGL
	/// for OpenGL Version >= 3.1.
	/// The class contains no rendering logic, it just implements the interface to LWJGL.
	/// </summary>
	public class RenderingEngineLwjgl31 : RenderingEngineLwjgl15
	{
		protected internal new static readonly int[] bufferTargetToGL = new int[] {GL15.GL_ARRAY_BUFFER, GL31.GL_UNIFORM_BUFFER};

		public override void bindBufferBase(int target, int bindingPoint, int buffer)
		{
			GL30.glBindBufferBase(bufferTargetToGL[target], bindingPoint, buffer);
		}

		public override int getUniformBlockIndex(int program, string name)
		{
			return GL31.glGetUniformBlockIndex(program, name);
		}

		public override void setUniformBlockBinding(int program, int blockIndex, int bindingPoint)
		{
			GL31.glUniformBlockBinding(program, blockIndex, bindingPoint);
		}
	}

}