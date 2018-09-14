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
namespace pspsharp.graphics.textures
{
	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class GEResizedTexture : GEProxyTexture
	{
		protected internal int x;
		protected internal int y;

		public GEResizedTexture(GETexture geTexture, int address, int bufferWidth, int width, int height, int pixelFormat) : base(geTexture, address, bufferWidth, width, height, pixelFormat, false)
		{

			x = 0;
			y = height - geTexture.Height;
		}

		protected internal override void updateTexture(IRenderingEngine re)
		{
			// Resize the GETexture to the requested texture size.
			// This has to be performed each time the base GETexture has changed.
			geTexture.copyTextureToScreen(re, x, y, Width, Height, false, true, true, true, true);
		}

		public override string ToString()
		{
			return string.Format("GEResizedTexture[{0:D}x{1:D}, base={2}]", Width, Height, geTexture.ToString());
		}
	}

}