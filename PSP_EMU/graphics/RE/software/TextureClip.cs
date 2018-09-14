/*

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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TWRAP_WRAP_MODE_REPEAT;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class TextureClip
	{
		public static IRandomTextureAccess getTextureClip(GeContext context, int mipmapLevel, IRandomTextureAccess textureAccess, int width, int height)
		{
			bool needClipWidth = false;
			bool needClipHeight = false;

			// No need to clip width if it will be wrapped with "repeat" mode on the required width
			if (context.tex_wrap_s != TWRAP_WRAP_MODE_REPEAT || context.texture_width[mipmapLevel] > width)
			{
				needClipWidth = true;
			}
			// No need to clip height if it will be wrapped with "repeat" mode on the required height
			if (context.tex_wrap_t != TWRAP_WRAP_MODE_REPEAT || context.texture_height[mipmapLevel] > height)
			{
				needClipHeight = true;
			}

			if (needClipWidth)
			{
				if (needClipHeight)
				{
					textureAccess = new TextureClipWidthHeight(textureAccess, width, height);
				}
				else
				{
					textureAccess = new TextureClipWidth(textureAccess, width);
				}
			}
			else
			{
				if (needClipHeight)
				{
					textureAccess = new TextureClipHeight(textureAccess, height);
				}
			}

			return textureAccess;
		}

		private class TextureClipWidth : IRandomTextureAccess
		{
			internal IRandomTextureAccess textureAccess;
			internal int width;

			public TextureClipWidth(IRandomTextureAccess textureAccess, int width)
			{
				this.textureAccess = textureAccess;
				this.width = width;
			}

			public virtual int readPixel(int u, int v)
			{
				if (u < 0 || u >= width)
				{
					return 0;
				}
				return textureAccess.readPixel(u, v);
			}

			public virtual int Width
			{
				get
				{
					return textureAccess.Width;
				}
			}

			public virtual int Height
			{
				get
				{
					return textureAccess.Height;
				}
			}
		}

		private class TextureClipHeight : IRandomTextureAccess
		{
			internal IRandomTextureAccess textureAccess;
			internal int height;

			public TextureClipHeight(IRandomTextureAccess textureAccess, int height)
			{
				this.textureAccess = textureAccess;
				this.height = height;
			}

			public virtual int readPixel(int u, int v)
			{
				if (v < 0 || v >= height)
				{
					return 0;
				}
				return textureAccess.readPixel(u, v);
			}

			public virtual int Width
			{
				get
				{
					return textureAccess.Width;
				}
			}

			public virtual int Height
			{
				get
				{
					return textureAccess.Height;
				}
			}
		}

		private class TextureClipWidthHeight : IRandomTextureAccess
		{
			internal IRandomTextureAccess textureAccess;
			internal int width;
			internal int height;

			public TextureClipWidthHeight(IRandomTextureAccess textureAccess, int width, int height)
			{
				this.textureAccess = textureAccess;
				this.width = width;
				this.height = height;
			}

			public virtual int readPixel(int u, int v)
			{
				if (u < 0 || u >= width || v < 0 || v >= height)
				{
					return 0;
				}
				return textureAccess.readPixel(u, v);
			}

			public virtual int Width
			{
				get
				{
					return textureAccess.Width;
				}
			}

			public virtual int Height
			{
				get
				{
					return textureAccess.Height;
				}
			}
		}
	}

}