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
	using EXTTextureCompressionS3TC = org.lwjgl.opengl.EXTTextureCompressionS3TC;
	using GL11 = org.lwjgl.opengl.GL11;
	using GL12 = org.lwjgl.opengl.GL12;

	/// <summary>
	/// @author gid15
	/// 
	/// A RenderingEngine implementing calls to OpenGL using LWJGL
	/// for OpenGL Version >= 1.2.
	/// The class contains no rendering logic, it just implements the interface to LWJGL.
	/// </summary>
	public class RenderingEngineLwjgl12 : RenderingEngineLwjgl
	{
		protected internal new static readonly int[] textureTypeToGL = new int[] {GL12.GL_UNSIGNED_SHORT_5_6_5_REV, GL12.GL_UNSIGNED_SHORT_1_5_5_5_REV, GL12.GL_UNSIGNED_SHORT_4_4_4_4_REV, GL12.GL_UNSIGNED_INT_8_8_8_8_REV, GL11.GL_UNSIGNED_BYTE, GL11.GL_UNSIGNED_BYTE, GL11.GL_UNSIGNED_SHORT, GL11.GL_UNSIGNED_INT, EXTTextureCompressionS3TC.GL_COMPRESSED_RGB_S3TC_DXT1_EXT, EXTTextureCompressionS3TC.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT, EXTTextureCompressionS3TC.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT, GL12.GL_UNSIGNED_SHORT_5_6_5_REV, GL12.GL_UNSIGNED_SHORT_1_5_5_5_REV, GL12.GL_UNSIGNED_SHORT_4_4_4_4_REV, GL12.GL_UNSIGNED_INT_8_8_8_8_REV};

		public RenderingEngineLwjgl12()
		{
		}
	}

}