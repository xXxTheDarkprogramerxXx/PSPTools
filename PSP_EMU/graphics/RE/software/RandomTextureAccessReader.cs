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
	using IMemoryReader = pspsharp.memory.IMemoryReader;

	/// <summary>
	/// @author gid15
	/// 
	/// Provide a random access for a texture provided having only a sequential access (IMemoryReader).
	/// </summary>
	public class RandomTextureAccessReader : IRandomTextureAccess
	{
		protected internal int width;
		protected internal int height;
		protected internal readonly int[] pixels;

		public RandomTextureAccessReader(IMemoryReader imageReader, int width, int height)
		{
			this.width = width;
			this.height = height;
			// Read the whole texture into the "pixels" array
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int Length = width * height;
			int Length = width * height;
			pixels = new int[Length];
			for (int i = 0; i < Length; i++)
			{
				pixels[i] = imageReader.readNext();
			}
		}

		public virtual int readPixel(int u, int v)
		{
			return pixels[v * width + u];
		}

		public virtual int Width
		{
			get
			{
				return width;
			}
		}

		public virtual int Height
		{
			get
			{
				return height;
			}
		}
	}

}