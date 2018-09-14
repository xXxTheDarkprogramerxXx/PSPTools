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
namespace pspsharp.graphics.RE.software
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.divideBy2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.divideBy4;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.multiply;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.pixelToTexel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.makePow2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.round;

	using CachedTexturePow2 = pspsharp.graphics.RE.software.CachedTexture.CachedTexturePow2;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class CachedTextureResampled
	{
		private const bool disableResampleAllTextures = false;
		private const bool disableResampleVRAMTexture = true;
		protected internal LinkedList<ResampleInfo> resampleInfos = new LinkedList<CachedTextureResampled.ResampleInfo>();
		protected internal CachedTexture cachedTextureOriginal;

		public CachedTextureResampled(CachedTexture cachedTexture)
		{
			cachedTextureOriginal = cachedTexture;
			ResampleInfo resampleInfo = new ResampleInfo(cachedTexture.width, cachedTexture.height, cachedTexture);
			resampleInfos.AddLast(resampleInfo);
		}

		public virtual CachedTexture OriginalTexture
		{
			get
			{
				return cachedTextureOriginal;
			}
		}

		public virtual bool canResample(float widthFactor, float heightFactor)
		{
			if (disableResampleAllTextures)
			{
				return false;
			}

			if (disableResampleVRAMTexture && cachedTextureOriginal.VRAMTexture)
			{
				// VRAM textures are often minimized or magnified by a factor of 2.
				// Allow these resamplings.
				if ((widthFactor == .5f && heightFactor == .5f) || (widthFactor == 2f && heightFactor == 2f))
				{
					return true;
				}
				return false;
			}

			return widthFactor >= .5f && heightFactor >= .5f && widthFactor <= 2f && heightFactor <= 2f;
		}

		public virtual CachedTexture resample(float widthFactor, float heightFactor)
		{
			if (widthFactor == 1f && heightFactor == 1f)
			{
				return cachedTextureOriginal;
			}

			int width = round(widthFactor * cachedTextureOriginal.width);
			int height = round(heightFactor * cachedTextureOriginal.height);

			return resample(width, height);
		}

		/// <summary>
		/// This method has to be synchronized because it can be used but multiple
		/// renderer threads in parallel (see RendererExecutor).
		/// </summary>
		private CachedTexture resample(int width, int height)
		{
			lock (this)
			{
				// Was the texture already resampled at the given size?
				foreach (ResampleInfo resampleInfo in resampleInfos)
				{
					if (resampleInfo.matches(width, height))
					{
						return resampleInfo.CachedTextureResampled;
					}
				}
        
				// A resampled texture was not yet available, compute one.
				return resampleTexture(width, height);
			}
		}

		private CachedTexture resampleTexture(int width, int height)
		{
			if (resampleInfos.Count >= 5 && VideoEngine.log_Renamed.InfoEnabled)
			{
				VideoEngine.log_Renamed.info(string.Format("Resampling texture from ({0:D},{1:D}) to ({2:D},{3:D}), pixelFormat={4:D}, resampled {5:D} times", cachedTextureOriginal.width, cachedTextureOriginal.height, width, height, cachedTextureOriginal.pixelFormat, resampleInfos.Count));
			}
			else if (VideoEngine.log_Renamed.DebugEnabled)
			{
				VideoEngine.log_Renamed.debug(string.Format("Resampling texture from ({0:D},{1:D}) to ({2:D},{3:D}), pixelFormat={4:D}, resampled {5:D} times", cachedTextureOriginal.width, cachedTextureOriginal.height, width, height, cachedTextureOriginal.pixelFormat, resampleInfos.Count));
			}

			RESoftware.textureResamplingStatistics.start();

			int widthPow2 = makePow2(width);
			int heightPow2 = makePow2(height);
			int[] buffer = new int[widthPow2 * heightPow2];
			int widthSkipEOL = widthPow2 - width;
			if (cachedTextureOriginal.width == (width << 1) && cachedTextureOriginal.height == (height << 1))
			{
				// Optimized common case: minimize texture by a factor of 2
				resampleTextureMinimize2(buffer, width, height, widthSkipEOL);
			}
			else if ((cachedTextureOriginal.width << 1) == width && (cachedTextureOriginal.height << 1) == height)
			{
				// Optimized common case: magnify texture by a factor of 2
				resampleTextureMagnify2(buffer, width, height, widthSkipEOL);
			}
			else
			{
				// Generic case: magnify/minimize by arbitrary factors
				float widthFactor = cachedTextureOriginal.width / (float) width;
				float heightFactor = cachedTextureOriginal.height / (float) height;
				resampleTexture(buffer, width, height, widthSkipEOL, widthFactor, heightFactor);
			}

			CachedTexture cachedTextureResampled = new CachedTexturePow2(widthPow2, heightPow2, width, height, GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888);
			cachedTextureResampled.setBuffer(buffer, 0, buffer.Length);

			ResampleInfo resampleInfo = new ResampleInfo(width, height, cachedTextureResampled);
			resampleInfos.AddLast(resampleInfo);

			RESoftware.textureResamplingStatistics.end();

			return cachedTextureResampled;
		}

		private void resampleTexture(int[] buffer, int width, int height, int widthSkipEOL, float widthFactor, float heightFactor)
		{
			float v = 0f;
			int index = 0;
			for (int y = 0; y < height; y++)
			{
				float u = 0f;
				for (int x = 0; x < width; x++)
				{
					buffer[index++] = readTexturePixelInterpolated(u, v, widthFactor, heightFactor);
					u += widthFactor;
				}
				index += widthSkipEOL;
				v += heightFactor;
			}
		}

		private void resampleTextureMinimize2(int[] buffer, int width, int height, int widthSkipEOL)
		{
			int index = 0;
			int pixel;
			for (int y = 0, v = 0; y < height; y++, v += 2)
			{
				for (int x = 0, u = 0; x < width; x++, u += 2)
				{
					pixel = divideBy4(cachedTextureOriginal.readPixel(u, v));
					pixel += divideBy4(cachedTextureOriginal.readPixel(u + 1, v));
					pixel += divideBy4(cachedTextureOriginal.readPixel(u, v + 1));
					pixel += divideBy4(cachedTextureOriginal.readPixel(u + 1, v + 1));
					buffer[index++] = pixel;
				}
				index += widthSkipEOL;
			}
		}

		private void resampleTextureMagnify2(int[] buffer, int width, int height, int widthSkipEOL)
		{
			int index = 0;
			int pixel;
			height -= 2;
			width -= 2;
			int lastU = width / 2;
			int lastV = height / 2;
			for (int y = 0, v = 0; y < height; y += 2, v++)
			{
				int currentPixel = cachedTextureOriginal.readPixel(0, v);
				for (int x = 0, u = 1; x < width; x += 2, u++)
				{
					buffer[index++] = currentPixel;
					pixel = divideBy2(currentPixel);
					currentPixel = cachedTextureOriginal.readPixel(u, v);
					pixel += divideBy2(currentPixel);
					buffer[index++] = pixel;
				}

				int pixelLastU = cachedTextureOriginal.readPixel(lastU, v);
				buffer[index++] = pixelLastU;
				buffer[index++] = pixelLastU;

				index += widthSkipEOL;

				for (int x = 0, u = 0; x < width; x += 2, u++)
				{
					pixel = divideBy2(cachedTextureOriginal.readPixel(u, v));
					pixel += divideBy2(cachedTextureOriginal.readPixel(u, v + 1));
					buffer[index++] = pixel;
					pixel = divideBy2(pixel);
					pixel += divideBy4(cachedTextureOriginal.readPixel(u + 1, v));
					pixel += divideBy4(cachedTextureOriginal.readPixel(u + 1, v + 1));
					buffer[index++] = pixel;
				}

				pixel = divideBy2(pixelLastU);
				pixel += divideBy2(cachedTextureOriginal.readPixel(lastU, v + 1));
				buffer[index++] = pixel;
				buffer[index++] = pixel;

				index += widthSkipEOL;
			}

			int currentPixel = cachedTextureOriginal.readPixel(0, lastV);
			int index2 = index + width + widthSkipEOL + 2;
			for (int x = 0, u = 0; x < width; x += 2, u++)
			{
				buffer[index++] = currentPixel;
				buffer[index2++] = currentPixel;
				pixel = divideBy2(currentPixel);
				currentPixel = cachedTextureOriginal.readPixel(u, lastV);
				pixel += divideBy2(currentPixel);
				buffer[index++] = pixel;
				buffer[index2++] = pixel;
			}

			int pixelLastU = cachedTextureOriginal.readPixel(lastU, lastV);
			buffer[index++] = pixelLastU;
			buffer[index] = pixelLastU;
			buffer[index2++] = pixelLastU;
			buffer[index2] = pixelLastU;
		}

		/// <summary>
		/// Interpolate a texture value at position (u,v) based on its 4 neighboring texels.
		/// 
		/// (u0,v0)-------(u1,v0)
		///    |   \     /   |
		///    |    (u,v)    |
		///    |   /     \   |
		/// (u0,v1)-------(u1,v1)
		/// 
		/// Example: for the pixel at (u=1.3, v=1.6), the following texture value is returned:
		///      texel(1,1) * 0.7 * 0.4 +
		///      texel(1,2) * 0.7 * 0.6 +
		///      texel(2,1) * 0.3 * 0.4 +
		///      texel(2,2) * 0.3 * 0.6
		/// </summary>
		/// <param name="u">            pixel position along X-axis </param>
		/// <param name="v">            pixel position along Y-axis </param>
		/// <param name="widthFactor">  factor between original and resampled width </param>
		/// <param name="heightFactor"> factor between original and resampled height </param>
		/// <returns>             interpolated texture value at (u,v) </returns>
		private int readTexturePixelInterpolated(float u, float v, float widthFactor, float heightFactor)
		{
			int texelU0 = pixelToTexel(u);
			int texelV0 = pixelToTexel(v);
			int texelU1 = texelU0 + 1;
			int texelV1 = texelV0 + 1;
			if (texelU1 >= cachedTextureOriginal.width)
			{
				texelU1 = texelU0;
			}
			if (texelV1 >= cachedTextureOriginal.height)
			{
				texelV1 = texelV0;
			}

			float factorU1 = u - texelU0;
			float factorV1 = v - texelV0;
			float factorU0 = 1f - factorU1;
			float factorV0 = 1f - factorV1;

			// If we fall exactly on one texel, take also the next texel into account
			// if we are minimizing the texture
			// (i.e. if the resampling factor is larger than 1)
			if (factorU1 == 0f && widthFactor > 1f)
			{
				factorU1 = (widthFactor - 1f) / widthFactor;
				factorU0 = 1f / widthFactor;
			}
			if (factorV1 == 0f && heightFactor > 1f)
			{
				factorV1 = (heightFactor - 1f) / heightFactor;
				factorV0 = 1f / heightFactor;
			}

			int pixel;
			if (factorU0 > 0f && factorV0 > 0f)
			{
				pixel = multiply(cachedTextureOriginal.readPixel(texelU0, texelV0), factorU0 * factorV0);
			}
			else
			{
				pixel = 0;
			}
			if (factorU1 > 0f && factorV0 > 0f)
			{
				pixel += multiply(cachedTextureOriginal.readPixel(texelU1, texelV0), factorU1 * factorV0);
			}
			if (factorU0 > 0f && factorV1 > 0f)
			{
				pixel += multiply(cachedTextureOriginal.readPixel(texelU0, texelV1), factorU0 * factorV1);
			}
			if (factorU1 > 0f && factorV1 > 0f)
			{
				pixel += multiply(cachedTextureOriginal.readPixel(texelU1, texelV1), factorU1 * factorV1);
			}

			return pixel;
		}

		public virtual void setClut()
		{
			cachedTextureOriginal.setClut();
		}

		private class ResampleInfo
		{
			internal int resampleWidth;
			internal int resampleHeight;
			internal CachedTexture cachedTextureResampled;

			public ResampleInfo(int resampleWidth, int resampleHeight, CachedTexture cachedTextureResampled)
			{
				this.resampleWidth = resampleWidth;
				this.resampleHeight = resampleHeight;
				this.cachedTextureResampled = cachedTextureResampled;
			}

			public virtual bool matches(int width, int height)
			{
				return width == resampleWidth && height == resampleHeight;
			}

			public virtual CachedTexture CachedTextureResampled
			{
				get
				{
					return cachedTextureResampled;
				}
			}
		}
	}

}