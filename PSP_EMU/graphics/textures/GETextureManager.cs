using System.Collections.Generic;

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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceDisplay.getTexturePixelFormat;

	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class GETextureManager
	{
		private static GETextureManager instance;
		private Dictionary<long, GETexture> geTextures = new Dictionary<long, GETexture>();

		public static GETextureManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new GETextureManager();
				}
				return instance;
			}
		}

		private long? getKey(int address, int bufferWidth, int width, int height, int pixelFormat)
		{
			return address + (((long) bufferWidth) << 30) + (((long) width) << 40) + (((long) height) << 50) + (((long) pixelFormat) << 60);
		}

		public virtual GETexture checkGETexture(int address, int bufferWidth, int width, int height, int pixelFormat)
		{
			long? key = getKey(address, bufferWidth, width, height, pixelFormat);
			return geTextures[key];
		}

		private GETexture checkGETexturePSM8888(int address, int bufferWidth, int width, int height, int pixelFormat)
		{
			GETexture geTexture = null;

			if (pixelFormat == GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888)
			{
				geTexture = checkGETexture(address, bufferWidth << 1, width, height, GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650);
				if (geTexture != null)
				{
					long? key = getKey(address, bufferWidth, width, height, pixelFormat);
					geTextures.Remove(key);
				}
			}

			return geTexture;
		}

		public virtual GETexture getGETexture(IRenderingEngine re, int address, int bufferWidth, int width, int height, int pixelFormat, bool useViewportResize)
		{
			int gePixelFormat = getTexturePixelFormat(pixelFormat);
			GETexture geTexture = checkGETexturePSM8888(address, bufferWidth, width, height, pixelFormat);
			if (geTexture == null)
			{
				geTexture = checkGETexture(address, bufferWidth, width, height, pixelFormat);
			}

			if (geTexture == null)
			{
				long? key = getKey(address, bufferWidth, width, height, pixelFormat);
				geTexture = new GETexture(address, bufferWidth, width, height, gePixelFormat, useViewportResize);
				geTextures[key] = geTexture;
			}

			return geTexture;
		}

		public virtual GETexture getGEResizedTexture(IRenderingEngine re, GETexture baseGETexture, int address, int bufferWidth, int width, int height, int pixelFormat)
		{
			int gePixelFormat = getTexturePixelFormat(pixelFormat);
			GETexture geTexture = checkGETexturePSM8888(address, bufferWidth, width, height, pixelFormat);
			if (geTexture == null)
			{
				geTexture = checkGETexture(address, bufferWidth, width, height, pixelFormat);
			}

			if (geTexture == null)
			{
				long? key = getKey(address, bufferWidth, width, height, pixelFormat);
				geTexture = new GEResizedTexture(baseGETexture, address, bufferWidth, width, height, gePixelFormat);
				geTextures[key] = geTexture;
			}

			return geTexture;
		}

		public virtual GETexture getGEIndexedTexture(IRenderingEngine re, GETexture baseGETexture, int address, int bufferWidth, int width, int height, int pixelFormat)
		{
			GETexture geTexture = checkGETexture(address, bufferWidth, width, height, pixelFormat);

			if (geTexture == null)
			{
				long? key = getKey(address, bufferWidth, width, height, pixelFormat);
				geTexture = new GEIndexedTexture(baseGETexture, address, bufferWidth, width, height, pixelFormat);
				geTextures[key] = geTexture;
			}

			return geTexture;
		}

		public virtual void reset(IRenderingEngine re)
		{
			foreach (GETexture geTexture in geTextures.Values)
			{
				geTexture.delete(re);
			}

			geTextures.Clear();
		}
	}

}