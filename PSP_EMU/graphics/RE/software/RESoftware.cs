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

	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using ImageReader = pspsharp.memory.ImageReader;
	using DurationStatistics = pspsharp.util.DurationStatistics;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// This RenderingEngine class implements a software-based rendering,
	/// not using OpenGL or any GPU.
	/// This is probably the most accurate implementation but also the slowest one.
	/// </summary>
	public class RESoftware : NullRenderingEngine
	{
		private const bool useTextureCache = true;
		protected internal int genTextureId;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal int bindTexture_Renamed;
		protected internal VertexState v1 = new VertexState();
		protected internal VertexState v2 = new VertexState();
		protected internal VertexState v3 = new VertexState();
		protected internal VertexState v4 = new VertexState();
		protected internal VertexState v5 = new VertexState();
		protected internal VertexState v6 = new VertexState();
		protected internal RendererExecutor rendererExecutor;
		protected internal Dictionary<int, CachedTextureResampled> cachedTextures = new Dictionary<int, CachedTextureResampled>();
		protected internal int textureBufferWidth;
		protected internal static DurationStatistics drawArraysStatistics = new DurationStatistics("RESoftware drawArrays");
		public static DurationStatistics triangleRender3DStatistics = new DurationStatistics("RESoftware TriangleRender3D");
		public static DurationStatistics triangleRender2DStatistics = new DurationStatistics("RESoftware TriangleRender2D");
		public static DurationStatistics spriteRenderStatistics = new DurationStatistics("RESoftware SpriteRender");
		protected internal static DurationStatistics cachedTextureStatistics = new DurationStatistics("RESoftware CachedTexture");
		public static DurationStatistics textureResamplingStatistics = new DurationStatistics("RESftware Texture resampling");
		protected internal BoundingBoxRenderer boundingBoxRenderer;
		protected internal bool boundingBoxVisible;
		protected internal BufferVertexReader bufferVertexReader;
		protected internal bool useVertexTexture;

		public RESoftware()
		{
			log.info("Using SoftwareRenderer");
		}

		public override void exit()
		{
			if (DurationStatistics.collectStatistics)
			{
				log.info(drawArraysStatistics);
				log.info(triangleRender3DStatistics);
				log.info(triangleRender2DStatistics);
				log.info(spriteRenderStatistics);
				log.info(cachedTextureStatistics);
				log.info(textureResamplingStatistics);
			}
		}

		public override void startDisplay()
		{
			context = VideoEngine.Instance.Context;
			rendererExecutor = RendererExecutor.Instance;
		}

		public override int setBones(int count, float[] values)
		{
			return count;
		}

		protected internal virtual void render(IRenderer renderer)
		{
			if (renderer.prepare(context))
			{
				rendererExecutor.render(renderer);
			}
		}

		protected internal virtual void drawSprite(SpriteRenderer spriteRenderer, VertexState v1, VertexState v2)
		{
			spriteRenderer.setVertex(v1, v2);
			render(spriteRenderer);
		}

		protected internal virtual CachedTextureResampled CachedTexture
		{
			get
			{
				CachedTextureResampled cachedTexture = cachedTextures[bindTexture_Renamed];
				if (cachedTexture != null)
				{
					cachedTexture.setClut();
				}
    
				return cachedTexture;
			}
		}

		protected internal virtual void drawArraysSprites(int first, int count)
		{
			CachedTextureResampled cachedTexture = CachedTexture;
			SpriteRenderer spriteRenderer = new SpriteRenderer(context, cachedTexture, useVertexTexture);
			bool readTexture = context.textureFlag.Enabled && !context.clearMode;
			Memory mem = Memory.Instance;
			for (int i = first; i < count - 1; i += 2)
			{
				int addr1 = context.vinfo.getAddress(mem, i);
				int addr2 = context.vinfo.getAddress(mem, i + 1);
				context.vinfo.readVertex(mem, addr1, v1, readTexture, VideoEngine.Instance.DoubleTexture2DCoords);
				context.vinfo.readVertex(mem, addr2, v2, readTexture, VideoEngine.Instance.DoubleTexture2DCoords);

				drawSprite(spriteRenderer, v1, v2);
			}
		}

		protected internal virtual void drawTriangle(TriangleRenderer triangleRenderer, VertexState v1, VertexState v2, VertexState v3, bool invertedFrontFace)
		{
			triangleRenderer.setVertex(v1, v2, v3);
			if (!triangleRenderer.isCulled(invertedFrontFace))
			{
				render(triangleRenderer);
			}
		}

		protected internal virtual bool isSprite(VertexInfo vinfo, VertexState tv1, VertexState tv2, VertexState tv3, VertexState tv4)
		{
			// Sprites are only available in 2D
			if (!vinfo.transform2D)
			{
				return false;
			}

			// Sprites are not culled. Keep triangles when the back face culling is enabled.
			if (!context.clearMode && context.cullFaceFlag.Enabled)
			{
				return false;
			}

			// Sprites have no normal
			if (vinfo.normal != 0)
			{
				return false;
			}

			// Color doubling not correctly handled on sprites
			if (context.textureColorDoubled)
			{
				return false;
			}

			if (vinfo.color != 0)
			{
				// Color of 4 vertex must be equal
				if (!Utilities.sameColor(tv1.c, tv2.c, tv3.c, tv4.c))
				{
					return false;
				}
			}

			// x1 == x2 && y1 == y3 && x4 == x3 && y4 == y2
			if (tv1.p[0] == tv2.p[0] && tv1.p[1] == tv3.p[1] && tv4.p[0] == tv3.p[0] && tv4.p[1] == tv2.p[1])
			{
				// z1 == z2 && z1 == z3 && z1 == z4
				if (tv1.p[2] == tv2.p[2] && tv1.p[2] == tv3.p[2] && tv1.p[2] == tv3.p[2])
				{
					if (vinfo.texture == 0)
					{
						return true;
					}
					// u1 == u2 && v1 == v3 && u4 == u3 && v4 == v2
					if (tv1.t[0] == tv2.t[0] && tv1.t[1] == tv3.t[1] && tv4.t[0] == tv3.t[0] && tv4.t[1] == tv2.t[1])
					{
						return true;
					}
					// v1 == v2 && u1 == u3 && v4 == v3 && u4 == u2
	//				if (tv1.t[1] == tv2.t[1] && tv1.t[0] == tv3.t[0] && tv4.t[1] == tv3.t[1] && tv4.t[0] == tv2.t[0]) {
	//					return true;
	//				}
				}
			}

			// y1 == y2 && x1 == x3 && y4 == y3 && x4 == x2
			if (tv1.p[1] == tv2.p[1] && tv1.p[0] == tv3.p[0] && tv4.p[1] == tv3.p[1] && tv4.p[0] == tv2.p[0])
			{
				// z1 == z2 && z1 == z3 && z1 == z4
				if (tv1.p[2] == tv2.p[2] && tv1.p[2] == tv3.p[2] && tv1.p[2] == tv3.p[2])
				{
					if (vinfo.texture == 0)
					{
						return true;
					}
					// v1 == v2 && u1 == u3 && v4 == v3 && u4 == u2
					if (tv1.t[1] == tv2.t[1] && tv1.t[0] == tv3.t[0] && tv4.t[1] == tv3.t[1] && tv4.t[0] == tv2.t[0])
					{
						return true;
					}
					// u1 == u2 && v1 == v3 && u4 == u3 && v4 == v2
	//				if (tv1.t[0] == tv2.t[0] && tv1.t[1] == tv3.t[1] && tv4.t[0] == tv3.t[0] && tv4.t[1] == tv2.t[1]) {
	//					return true;
	//				}
				}
			}

			return false;
		}

		protected internal virtual void resetBufferVertexReader()
		{
			bufferVertexReader = null;
		}

		protected internal virtual void readVertex(Memory mem, int index, VertexState v, bool readTexture)
		{
			if (bufferVertexReader == null)
			{
				int addr = context.vinfo.getAddress(mem, index);
				context.vinfo.readVertex(mem, addr, v, readTexture, VideoEngine.Instance.DoubleTexture2DCoords);
			}
			else
			{
				// This is used for spline and bezier curves:
				// the VideoEngine is computing the vertices and is pushing them into a buffer.
				bufferVertexReader.readVertex(index, v);
			}
			if (context.vinfo.weight != 0)
			{
				VideoEngine.doSkinning(context.bone_uploaded_matrix, context.vinfo, v);
			}
		}

		protected internal virtual void drawArraysTriangleStrips(int first, int count)
		{
			Memory mem = Memory.Instance;
			CachedTextureResampled cachedTexture = CachedTexture;
			TriangleRenderer triangleRenderer = new TriangleRenderer(context, cachedTexture, useVertexTexture);
			SpriteRenderer spriteRenderer = null;
			VertexState tv1 = null;
			VertexState tv2 = null;
			VertexState tv3 = null;
			VertexState tv4 = v1;
			bool readTexture = context.textureFlag.Enabled && !context.clearMode;
			for (int i = 0; i < count; i++)
			{
				readVertex(mem, first + i, tv4, readTexture);
				if (tv3 != null)
				{
					// Displaying a sprite (i.e. rectangular area) is faster.
					// Try to merge adjacent triangles if they form a sprite.
					if (isSprite(context.vinfo, tv1, tv2, tv3, tv4))
					{
						if (spriteRenderer == null)
						{
							spriteRenderer = new SpriteRenderer(context, cachedTexture, useVertexTexture);
						}
						drawSprite(spriteRenderer, tv1, tv4);
						v5.copy(tv3);
						v6.copy(tv4);
						v1.copy(v5);
						v2.copy(v6);
						tv1 = v1;
						tv2 = v2;
						tv3 = null;
						tv4 = v3;
					}
					else
					{
						// The Front face direction is inverted every 2 triangles in the strip.
						drawTriangle(triangleRenderer, tv1, tv2, tv3, ((i - 3) & 1) != 0);
						VertexState v = tv1;
						tv1 = tv2;
						tv2 = tv3;
						tv3 = tv4;
						tv4 = v;
					}
				}
				else if (tv1 == null)
				{
					tv1 = tv4;
					tv4 = v2;
				}
				else if (tv2 == null)
				{
					tv2 = tv4;
					tv4 = v3;
				}
				else
				{
					tv3 = tv4;
					tv4 = v4;
				}
			}

			if (tv3 != null)
			{
				// The Front face direction is inverted every 2 triangles in the strip.
				drawTriangle(triangleRenderer, tv1, tv2, tv3, (count & 1) == 0);
			}
		}

		protected internal virtual void drawArraysTriangles(int first, int count)
		{
			Memory mem = Memory.Instance;
			CachedTextureResampled cachedTexture = CachedTexture;
			TriangleRenderer triangleRenderer = new TriangleRenderer(context, cachedTexture, useVertexTexture);
			bool readTexture = context.textureFlag.Enabled && !context.clearMode;
			for (int i = 0; i < count; i += 3)
			{
				readVertex(mem, first + i, v1, readTexture);
				readVertex(mem, first + i + 1, v2, readTexture);
				readVertex(mem, first + i + 2, v3, readTexture);

				drawTriangle(triangleRenderer, v1, v2, v3, false);
			}
		}

		protected internal virtual void drawArraysTriangleFan(int first, int count)
		{
			Memory mem = Memory.Instance;
			CachedTextureResampled cachedTexture = CachedTexture;
			TriangleRenderer triangleRenderer = new TriangleRenderer(context, cachedTexture, useVertexTexture);
			VertexState tv1 = null;
			VertexState tv2 = null;
			VertexState tv3 = v1;
			bool readTexture = context.textureFlag.Enabled && !context.clearMode;
			for (int i = 0; i < count; i++)
			{
				readVertex(mem, first + i, tv3, readTexture);
				if (tv2 != null)
				{
					drawTriangle(triangleRenderer, tv1, tv2, tv3, false);
					VertexState v = tv2;
					tv2 = tv3;
					tv3 = v;
				}
				else if (tv1 == null)
				{
					tv1 = tv3;
					tv3 = v2;
				}
				else
				{
					tv2 = tv3;
					tv3 = v3;
				}
			}
		}

		public override void drawArrays(int primitive, int first, int count)
		{
			drawArraysStatistics.start();
			switch (primitive)
			{
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_SPRITES:
					drawArraysSprites(first, count);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_TRIANGLE_STRIP:
					drawArraysTriangleStrips(first, count);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_TRIANGLES:
					drawArraysTriangles(first, count);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_TRIANGLE_FAN:
					drawArraysTriangleFan(first, count);
					break;
			}
			drawArraysStatistics.end();
		}

		public override void setTexCoordPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			if (bufferVertexReader == null)
			{
				bufferVertexReader = new BufferVertexReader();
			}
			bufferVertexReader.setTextureComponentInfo(size, type, stride, bufferSize, buffer);
		}

		public override void setColorPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			if (bufferVertexReader == null)
			{
				bufferVertexReader = new BufferVertexReader();
			}
			bufferVertexReader.setColorComponentInfo(size, type, stride, bufferSize, buffer);
		}

		public override void setVertexPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			if (bufferVertexReader == null)
			{
				bufferVertexReader = new BufferVertexReader();
			}
			bufferVertexReader.setVertexComponentInfo(size, type, stride, bufferSize, buffer);
		}

		public override void setNormalPointer(int type, int stride, int bufferSize, Buffer buffer)
		{
			if (bufferVertexReader == null)
			{
				bufferVertexReader = new BufferVertexReader();
			}
			bufferVertexReader.setNormalComponentInfo(type, stride, bufferSize, buffer);
		}

		public override void setWeightPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			if (bufferVertexReader == null)
			{
				bufferVertexReader = new BufferVertexReader();
			}
			bufferVertexReader.setWeightComponentInfo(size, type, stride, bufferSize, buffer);
		}

		public override void setPixelStore(int rowLength, int alignment)
		{
			textureBufferWidth = rowLength;
		}

		public override int genTexture()
		{
			return genTextureId++;
		}

		public override void bindTexture(int texture)
		{
			bindTexture_Renamed = texture;
		}

		public override void deleteTexture(int texture)
		{
			cachedTextures.Remove(texture);
		}

		public override void setCompressedTexImage(int level, int internalFormat, int width, int height, int compressedSize, Buffer buffer)
		{
			if (useTextureCache)
			{
				cachedTextureStatistics.start();
				// TODO Cache all the texture levels
				if (level == 0)
				{
					int bufferWidth = context.texture_buffer_width[level];
					IMemoryReader imageReader = ImageReader.getImageReader(context.texture_base_pointer[level], width, height, bufferWidth, internalFormat, false, 0, 0, 0, 0, 0, 0, null, null);
					CachedTexture cachedTexture = CachedTexture.getCachedTexture(System.Math.Min(width, bufferWidth), height, internalFormat, imageReader);
					CachedTextureResampled cachedTextureResampled = new CachedTextureResampled(cachedTexture);
					cachedTextures[bindTexture_Renamed] = cachedTextureResampled;
				}
				cachedTextureStatistics.end();
			}
		}

		public override void setTexImage(int level, int internalFormat, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
			if (useTextureCache)
			{
				cachedTextureStatistics.start();
				// TODO Cache all the texture levels
				if (level == 0)
				{
					CachedTexture cachedTexture = null;
					if (buffer is IntBuffer)
					{
						cachedTexture = CachedTexture.getCachedTexture(textureBufferWidth, height, format, ((IntBuffer) buffer).array(), buffer.arrayOffset(), textureSize >> 2);
					}
					else if (buffer is ShortBuffer)
					{
						cachedTexture = CachedTexture.getCachedTexture(textureBufferWidth, height, format, ((ShortBuffer) buffer).array(), buffer.arrayOffset(), textureSize >> 1);
					}
					CachedTextureResampled cachedTextureResampled = new CachedTextureResampled(cachedTexture);
					cachedTextures[bindTexture_Renamed] = cachedTextureResampled;
				}
				cachedTextureStatistics.end();
			}
		}

		public override void beginBoundingBox(int numberOfVertexBoundingBox)
		{
			boundingBoxRenderer = new BoundingBoxRenderer(context);
			boundingBoxVisible = true;
		}

		public override void drawBoundingBox(float[][] values)
		{
			if (boundingBoxVisible)
			{
				boundingBoxRenderer.drawBoundingBox(values);
				if (!boundingBoxRenderer.prepare(context))
				{
					boundingBoxVisible = false;
				}
			}
		}

		public override void endBoundingBox(VertexInfo vinfo)
		{
		}

		public override bool BoundingBoxVisible
		{
			get
			{
				return boundingBoxVisible;
			}
		}

		public override bool canAllNativeVertexInfo()
		{
			return true;
		}

		public override bool canNativeSpritesPrimitive()
		{
			return true;
		}

		public override void setVertexInfo(VertexInfo vinfo, bool allNativeVertexInfo, bool useVertexColor, bool useTexture, int type)
		{
			this.useVertexTexture = useTexture;
			resetBufferVertexReader();
		}

		public override bool canNativeClut(int textureAddress, int pixelFormat, bool textureSwizzle)
		{
			if (Memory.isVRAM(textureAddress) && !textureSwizzle)
			{
				return true;
			}
			return !useTextureCache;
		}

		public override void waitForRenderingCompletion()
		{
			rendererExecutor.waitForRenderingCompletion();
		}

		public override bool canReadAllVertexInfo()
		{
			// drawArrays doesn't need vertex infos in buffers, it can read directly from memory.
			return true;
		}

		public override bool setCopyRedToAlpha(bool copyRedToAlpha)
		{
			return true;
		}

		public override float[] VertexColor
		{
			set
			{
				for (int i = 0; i < context.vertexColor.Length; i++)
				{
					context.vertexColor[i] = value[i];
				}
			}
		}
	}

}