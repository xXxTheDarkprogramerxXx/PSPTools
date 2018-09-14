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
	public class GEIndexedTexture : GEResizedTexture
	{
		public GEIndexedTexture(GETexture geTexture, int address, int bufferWidth, int width, int height, int pixelFormat) : base(geTexture, address, bufferWidth, width, height, pixelFormat)
		{

			// Map the pixel format:
			// TPSM_PIXEL_STORAGE_MODE_16BIT_INDEXED -> RE_PIXEL_STORAGE_16BIT_INDEXED_BGR5650
			//                                          RE_PIXEL_STORAGE_16BIT_INDEXED_ABGR5651
			//                                          RE_PIXEL_STORAGE_16BIT_INDEXED_ABGR4444
			// TPSM_PIXEL_STORAGE_MODE_32BIT_INDEXED -> RE_PIXEL_STORAGE_32BIT_INDEXED_ABGR8888
			switch (pixelFormat)
			{
				case GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_INDEXED:
					this.pixelFormat = geTexture.pixelFormat - GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650 + pspsharp.graphics.RE.IRenderingEngine_Fields.RE_PIXEL_STORAGE_16BIT_INDEXED_BGR5650;
					break;
				case GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_INDEXED:
					this.pixelFormat = pspsharp.graphics.RE.IRenderingEngine_Fields.RE_PIXEL_STORAGE_32BIT_INDEXED_ABGR8888;
					break;
			}
		}

		protected internal override void updateTexture(IRenderingEngine re)
		{
			re.setTextureFormat(pixelFormat, false);
			base.updateTexture(re);
		}

		public override string ToString()
		{
			return string.Format("GEIndexedTexture[{0:D}x{1:D}, base={2}]", Width, Height, geTexture.ToString());
		}
	}

}