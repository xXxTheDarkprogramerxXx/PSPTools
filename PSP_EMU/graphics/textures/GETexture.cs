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
//	import static pspsharp.graphics.GeCommands.TFLT_NEAREST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TWRAP_WRAP_MODE_CLAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.VideoEngine.SIZEOF_FLOAT;


	//using Logger = org.apache.log4j.Logger;

	using Modules = pspsharp.HLE.Modules;
	using sceDisplay = pspsharp.HLE.modules.sceDisplay;
	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;
	using IREBufferManager = pspsharp.graphics.RE.buffer.IREBufferManager;
	using CaptureManager = pspsharp.graphics.capture.CaptureManager;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class GETexture
	{
		protected internal static Logger log = VideoEngine.log_Renamed;
		protected internal int address;
		protected internal int Length;
		protected internal int bufferWidth;
		protected internal int width;
		protected internal int height;
		protected internal int widthPow2;
		protected internal int heightPow2;
		protected internal int pixelFormat;
		protected internal int bytesPerPixel;
		protected internal int textureId = -1;
		protected internal int drawBufferId = -1;
		protected internal float texS;
		protected internal float texT;
		private bool changed;
		protected internal int bufferLength;
		protected internal Buffer buffer;
		protected internal bool useViewportResize;
		protected internal float resizeScale;
		// For copying Stencil to texture Alpha
		private int stencilFboId = -1;
		private int stencilTextureId = -1;
		private const int stencilPixelFormat = pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DEPTH_STENCIL;

		public GETexture(int address, int bufferWidth, int width, int height, int pixelFormat, bool useViewportResize)
		{
			this.address = address;
			this.bufferWidth = bufferWidth;
			this.width = width;
			this.height = height;
			this.pixelFormat = pixelFormat;
			bytesPerPixel = sceDisplay.getPixelFormatBytes(pixelFormat);
			Length = bufferWidth * height * bytesPerPixel;
			widthPow2 = Utilities.makePow2(width);
			heightPow2 = Utilities.makePow2(height);
			this.useViewportResize = useViewportResize;
			changed = true;
			resizeScale = ViewportResizeScaleFactor;
		}

		private int TextureBufferLength
		{
			get
			{
				return TexImageWidth * TexImageHeight * bytesPerPixel;
			}
		}

		private float ViewportResizeScaleFactor
		{
			get
			{
				if (!useViewportResize)
				{
					return 1;
				}
    
				return Modules.sceDisplayModule.ViewportResizeScaleFactor;
			}
		}

		public virtual void bind(IRenderingEngine re, bool forDrawing)
		{
			float viewportResizeScaleFactor = ViewportResizeScaleFactor;
			// Create the texture if not yet created or
			// re-create it if the viewport resize factor has been changed dynamically.
			if (textureId == -1 || viewportResizeScaleFactor != resizeScale)
			{
				// The pspsharp window has been resized. Recreate all the textures using the new size.
				if (textureId != -1)
				{
					re.deleteTexture(textureId);
					textureId = -1;
				}
				if (stencilTextureId != -1)
				{
					re.deleteTexture(stencilTextureId);
					stencilTextureId = -1;
				}
				if (stencilFboId != -1)
				{
					re.deleteFramebuffer(stencilFboId);
					stencilFboId = -1;
				}

				resizeScale = viewportResizeScaleFactor;

				if (useViewportResize)
				{
					texS = sceDisplay.getResizedWidth(width) / (float) TexImageWidth;
					texT = sceDisplay.getResizedHeight(height) / (float) TexImageHeight;
				}
				else
				{
					texS = width / (float) bufferWidth;
					texT = height / (float) heightPow2;
				}

				textureId = re.genTexture();
				re.bindTexture(textureId);
				re.setTexImage(0, pixelFormat, TexImageWidth, TexImageHeight, pixelFormat, pixelFormat, 0, null);
				re.TextureMipmapMinFilter = TFLT_NEAREST;
				re.TextureMipmapMagFilter = TFLT_NEAREST;
				re.TextureMipmapMinLevel = 0;
				re.TextureMipmapMaxLevel = 0;
				re.setTextureWrapMode(TWRAP_WRAP_MODE_CLAMP, TWRAP_WRAP_MODE_CLAMP);
				if (drawBufferId == -1)
				{
					drawBufferId = re.BufferManager.genBuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_ARRAY_BUFFER, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FLOAT, 16, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DYNAMIC_DRAW);
				}
			}
			else
			{
				re.bindTexture(textureId);
			}

			if (forDrawing)
			{
				re.setTextureFormat(pixelFormat, false);
			}
		}

		public virtual int BufferWidth
		{
			get
			{
				return bufferWidth;
			}
		}

		public virtual int TexImageWidth
		{
			get
			{
				return useViewportResize ? sceDisplay.getResizedWidthPow2(bufferWidth) : bufferWidth;
			}
		}

		public virtual int TexImageHeight
		{
			get
			{
				return useViewportResize ? sceDisplay.getResizedHeightPow2(heightPow2) : heightPow2;
			}
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

		public virtual int ResizedWidth
		{
			get
			{
				return useViewportResize ? sceDisplay.getResizedWidth(width) : width;
			}
		}

		public virtual int ResizedHeight
		{
			get
			{
				return useViewportResize ? sceDisplay.getResizedHeight(height) : height;
			}
		}

		public virtual int WidthPow2
		{
			get
			{
				return widthPow2;
			}
		}

		public virtual int HeightPow2
		{
			get
			{
				return heightPow2;
			}
		}

		public virtual int PixelFormat
		{
			get
			{
				return pixelFormat;
			}
		}

		public virtual void copyScreenToTexture(IRenderingEngine re)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("GETexture.copyScreenToTexture {0}", ToString()));
			}

			bind(re, false);

			int texWidth = System.Math.Min(bufferWidth, width);
			int texHeight = height;
			if (useViewportResize)
			{
				texWidth = sceDisplay.getResizedWidth(texWidth);
				texHeight = sceDisplay.getResizedHeight(texHeight);
			}
			re.copyTexSubImage(0, 0, 0, 0, 0, texWidth, texHeight);

			if (Modules.sceDisplayModule.SaveStencilToMemory)
			{
				if (!copyStencilToTextureAlpha(re, texWidth, texHeight))
				{
					Modules.sceDisplayModule.SaveStencilToMemory = false;
				}
			}

			Changed = true;
		}

		public virtual void copyTextureToScreen(IRenderingEngine re)
		{
			copyTextureToScreen(re, 0, 0, width, height, true, true, true, true, true);
		}

		protected internal virtual void copyTextureToScreen(IRenderingEngine re, int x, int y, int projectionWidth, int projectionHeight, bool scaleToCanvas, bool redWriteEnabled, bool greenWriteEnabled, bool blueWriteEnabled, bool alphaWriteEnabled)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("GETexture.copyTextureToScreen {0} at {1:D}x{2:D}", ToString(), x, y));
			}

			bind(re, true);

			drawTexture(re, x, y, projectionWidth, projectionHeight, scaleToCanvas, redWriteEnabled, greenWriteEnabled, blueWriteEnabled, alphaWriteEnabled);
		}

		private void drawTexture(IRenderingEngine re, int x, int y, int projectionWidth, int projectionHeight, bool scaleToCanvas, bool redWriteEnabled, bool greenWriteEnabled, bool blueWriteEnabled, bool alphaWriteEnabled)
		{
			re.startDirectRendering(true, false, true, true, true, projectionWidth, projectionHeight);
			re.setColorMask(redWriteEnabled, greenWriteEnabled, blueWriteEnabled, alphaWriteEnabled);
			if (scaleToCanvas)
			{
				re.setViewport(0, 0, Modules.sceDisplayModule.CanvasWidth, Modules.sceDisplayModule.CanvasHeight);
			}
			else
			{
				re.setViewport(0, 0, projectionWidth, projectionHeight);
			}

			IREBufferManager bufferManager = re.BufferManager;
			ByteBuffer drawByteBuffer = bufferManager.getBuffer(drawBufferId);
			drawByteBuffer.clear();
			FloatBuffer drawFloatBuffer = drawByteBuffer.asFloatBuffer();
			drawFloatBuffer.clear();
			drawFloatBuffer.put(texS);
			drawFloatBuffer.put(texT);
			drawFloatBuffer.put(x + width);
			drawFloatBuffer.put(y + height);

			drawFloatBuffer.put(0.0f);
			drawFloatBuffer.put(texT);
			drawFloatBuffer.put(x);
			drawFloatBuffer.put(y + height);

			drawFloatBuffer.put(0.0f);
			drawFloatBuffer.put(0.0f);
			drawFloatBuffer.put(x);
			drawFloatBuffer.put(y);

			drawFloatBuffer.put(texS);
			drawFloatBuffer.put(0.0f);
			drawFloatBuffer.put(x + width);
			drawFloatBuffer.put(y);

			if (re.VertexArrayAvailable)
			{
				re.bindVertexArray(0);
			}
			re.setVertexInfo(null, false, false, true, -1);
			re.enableClientState(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_TEXTURE);
			re.disableClientState(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_COLOR);
			re.disableClientState(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_NORMAL);
			re.enableClientState(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_VERTEX);
			bufferManager.setTexCoordPointer(drawBufferId, 2, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FLOAT, 4 * SIZEOF_FLOAT, 0);
			bufferManager.setVertexPointer(drawBufferId, 2, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FLOAT, 4 * SIZEOF_FLOAT, 2 * SIZEOF_FLOAT);
			bufferManager.setBufferData(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_ARRAY_BUFFER, drawBufferId, drawFloatBuffer.position() * SIZEOF_FLOAT, drawByteBuffer.rewind(), pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DYNAMIC_DRAW);
			re.drawArrays(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_QUADS, 0, 4);

			re.endDirectRendering();
		}

		protected internal virtual bool Changed
		{
			set
			{
				this.changed = value;
			}
		}

		protected internal virtual bool hasChanged()
		{
			return changed;
		}

		private void prepareBuffer()
		{
			// Is the current buffer large enough?
			if (buffer != null && bufferLength < TextureBufferLength)
			{
				// Reallocate a new larger buffer
				buffer = null;
			}

			if (buffer == null)
			{
				bufferLength = TextureBufferLength;
				ByteBuffer byteBuffer = ByteBuffer.allocateDirect(bufferLength).order(ByteOrder.LITTLE_ENDIAN);
				if (Memory.Instance.MainMemoryByteBuffer is IntBuffer)
				{
					buffer = byteBuffer.asIntBuffer();
				}
				else
				{
					buffer = byteBuffer;
				}
			}
			else
			{
				buffer.clear();
			}
		}

		public virtual void copyTextureToMemory(IRenderingEngine re)
		{
			if (textureId == -1)
			{
				// Texture not yet created... nothing to copy
				return;
			}

			if (!hasChanged())
			{
				// Texture unchanged... don't copy again
				return;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("GETexture.copyTextureToMemory {0}", ToString()));
			}

			Buffer memoryBuffer = Memory.Instance.getBuffer(address, Length);
			prepareBuffer();
			re.bindTexture(textureId);
			re.setTextureFormat(pixelFormat, false);
			re.setPixelStore(bufferWidth, sceDisplay.getPixelFormatBytes(pixelFormat));
			re.getTexImage(0, pixelFormat, pixelFormat, buffer);

			buffer.clear();
			if (buffer is IntBuffer)
			{
				IntBuffer src = (IntBuffer) buffer;
				IntBuffer dst = (IntBuffer) memoryBuffer;
				int pixelsPerElement = 4 / bytesPerPixel;
				int copyWidth = System.Math.Min(width, bufferWidth);
				int widthLimit = (copyWidth + pixelsPerElement - 1) / pixelsPerElement;
				int step = bufferWidth / pixelsPerElement;
				int srcOffset = 0;
				int dstOffset = (height - 1) * step;
				// We have received the texture data upside-down, invert it
				for (int y = 0; y < height; y++, srcOffset += step, dstOffset -= step)
				{
					src.limit(srcOffset + widthLimit);
					src.position(srcOffset);
					dst.position(dstOffset);
					dst.put(src);
				}
			}
			else
			{
				ByteBuffer src = (ByteBuffer) buffer;
				ByteBuffer dst = (ByteBuffer) memoryBuffer;
				int copyWidth = System.Math.Min(width, bufferWidth);
				int widthLimit = copyWidth * bytesPerPixel;
				int step = bufferWidth * bytesPerPixel;
				int srcOffset = 0;
				int dstOffset = (height - 1) * step;
				// We have received the texture data upside-down, invert it
				for (int y = 0; y < height; y++, srcOffset += step, dstOffset -= step)
				{
					src.limit(srcOffset + widthLimit);
					src.position(srcOffset);
					dst.position(dstOffset);
					dst.put(src);
				}
			}

			Changed = false;
		}

		public virtual void delete(IRenderingEngine re)
		{
			if (drawBufferId != -1)
			{
				re.BufferManager.deleteBuffer(drawBufferId);
				drawBufferId = -1;
			}
			if (textureId != -1)
			{
				re.deleteTexture(textureId);
				textureId = -1;
			}
		}

		public virtual int TextureId
		{
			get
			{
				return textureId;
			}
		}

		public virtual bool isCompatible(int width, int height, int bufferWidth, int pixelFormat)
		{
			if (width != this.width || height != this.height || bufferWidth != this.bufferWidth)
			{
				return false;
			}

			if (useViewportResize)
			{
				if (resizeScale != ViewportResizeScaleFactor)
				{
					return false;
				}
			}

			return true;
		}

		protected internal virtual bool copyStencilToTextureAlpha(IRenderingEngine re, int texWidth, int texHeight)
		{
			re.checkAndLogErrors(null);

			if (stencilFboId == -1)
			{
				// Create a FBO
				stencilFboId = re.genFramebuffer();
				re.bindFramebuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DRAW_FRAMEBUFFER, stencilFboId);

				// Create stencil texture and attach it to the FBO
				stencilTextureId = re.genTexture();
				re.bindTexture(stencilTextureId);
				re.checkAndLogErrors("bindTexture");
				re.setTexImage(0, stencilPixelFormat, TexImageWidth, TexImageHeight, stencilPixelFormat, stencilPixelFormat, 0, null);
				if (re.checkAndLogErrors("setTexImage"))
				{
					return false;
				}
				re.TextureMipmapMinFilter = TFLT_NEAREST;
				re.TextureMipmapMagFilter = TFLT_NEAREST;
				re.TextureMipmapMinLevel = 0;
				re.TextureMipmapMaxLevel = 0;
				re.setTextureWrapMode(TWRAP_WRAP_MODE_CLAMP, TWRAP_WRAP_MODE_CLAMP);
				re.setFramebufferTexture(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DRAW_FRAMEBUFFER, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DEPTH_STENCIL_ATTACHMENT, stencilTextureId, 0);
				if (re.checkAndLogErrors("setFramebufferTexture RE_STENCIL_ATTACHMENT"))
				{
					return false;
				}

				// Attach the GE texture to the FBO as well
				re.setFramebufferTexture(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DRAW_FRAMEBUFFER, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_COLOR_ATTACHMENT0, textureId, 0);
				if (re.checkAndLogErrors("setFramebufferTexture RE_COLOR_ATTACHMENT0"))
				{
					return false;
				}
			}
			else
			{
				re.bindFramebuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DRAW_FRAMEBUFFER, stencilFboId);
			}

			// Copy screen stencil buffer to stencil texture:
			// - read framebuffer is screen (0)
			// - draw/write framebuffer is our stencil FBO (stencilFboId)
			re.blitFramebuffer(0, 0, texWidth, texHeight, 0, 0, texWidth, texHeight, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_STENCIL_BUFFER_BIT, GeCommands.TFLT_NEAREST);
			if (re.checkAndLogErrors("blitFramebuffer"))
			{
				return false;
			}

			re.bindTexture(stencilTextureId);

			if (!re.setCopyRedToAlpha(true))
			{
				return false;
			}

			// Draw the stencil texture and update only the alpha channel of the GE texture
			drawTexture(re, 0, 0, width, height, true, false, false, false, true);
			re.checkAndLogErrors("drawTexture");

			// Reset the framebuffer to the default one
			re.bindFramebuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DRAW_FRAMEBUFFER, 0);

			re.CopyRedToAlpha = false;

			// Success
			return true;
		}

		public virtual void capture(IRenderingEngine re)
		{
			if (textureId == -1)
			{
				// Texture not yet created... nothing to capture
				return;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("GETexture.capture {0}", ToString()));
			}

			prepareBuffer();
			re.bindTexture(textureId);
			re.setTextureFormat(pixelFormat, false);
			re.setPixelStore(bufferWidth, sceDisplay.getPixelFormatBytes(pixelFormat));
			re.getTexImage(0, pixelFormat, pixelFormat, buffer);

			CaptureManager.captureImage(address, 0, buffer, width, height, bufferWidth, pixelFormat, false, 0, true, false);
		}

		public override string ToString()
		{
			return string.Format("GETexture[0x{0:X8}-0x{1:X8}, {2:D}x{3:D} (texture {4:D}x{5:D}), bufferWidth={6:D}, pixelFormat={7:D}({8})]", address, address + Length, width, height, TexImageWidth, TexImageHeight, bufferWidth, pixelFormat, VideoEngine.getPsmName(pixelFormat));
		}
	}

}