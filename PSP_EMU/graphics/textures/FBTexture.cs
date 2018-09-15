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
	/// A texture being used as a render target, using OpenGL FrameBuffer Object (FBO).
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class FBTexture : GETexture
	{
		private int fboId = -1;
		private int depthRenderBufferId = -1;

		public FBTexture(int address, int bufferWidth, int width, int height, int pixelFormat) : base(address, bufferWidth, width, height, pixelFormat, true)
		{
		}

		public FBTexture(FBTexture copy) : base(copy.address, copy.bufferWidth, copy.width, copy.height, copy.pixelFormat, copy.useViewportResize)
		{
		}

		public override void bind(IRenderingEngine re, bool forDrawing)
		{
			if (forDrawing)
			{
				// We are copying the texture back to the main frame buffer,
				// bind the texture, not the FBO.
				re.bindFramebuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FRAMEBUFFER, 0);
				base.bind(re, forDrawing);
			}
			else
			{
				if (fboId == -1)
				{
					createFBO(re, forDrawing);
				}
				else
				{
					// Bind the FBO
					re.bindFramebuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FRAMEBUFFER, fboId);
				}
			}
		}

		protected internal virtual void createFBO(IRenderingEngine re, bool forDrawing)
		{
			// Create the FBO and associate it to the texture
			fboId = re.genFramebuffer();
			re.bindFramebuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FRAMEBUFFER, fboId);

			// Create a render buffer for the depth buffer
			depthRenderBufferId = re.genRenderbuffer();
			re.bindRenderbuffer(depthRenderBufferId);
			re.setRenderbufferStorage(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DEPTH_COMPONENT, TexImageWidth, TexImageHeight);

			// Create the texture
			base.bind(re, forDrawing);

			// Attach the texture to the FBO
			re.setFramebufferTexture(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FRAMEBUFFER, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_COLOR_ATTACHMENT0, textureId, 0);
			// Attach the depth buffer to the FBO
			re.setFramebufferRenderbuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FRAMEBUFFER, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DEPTH_ATTACHMENT, depthRenderBufferId);
		}

		public override void delete(IRenderingEngine re)
		{
			if (fboId != -1)
			{
				re.deleteFramebuffer(fboId);
				fboId = -1;
			}
			if (depthRenderBufferId != -1)
			{
				re.deleteRenderbuffer(depthRenderBufferId);
				depthRenderBufferId = -1;
			}
			base.delete(re);
		}

		public virtual void blitFrom(IRenderingEngine re, FBTexture src)
		{
			if (fboId == -1)
			{
				createFBO(re, false);
			}

			// Bind the source and destination FBOs
			re.bindFramebuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_READ_FRAMEBUFFER, src.fboId);
			re.bindFramebuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DRAW_FRAMEBUFFER, fboId);

			// Copy the source FBO to the destination FBO
			re.blitFramebuffer(0, 0, src.ResizedWidth, src.ResizedHeight, 0, 0, ResizedWidth, ResizedHeight, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_COLOR_BUFFER_BIT, GeCommands.TFLT_NEAREST);

			// Re-bind the source FBO
			re.bindFramebuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FRAMEBUFFER, src.fboId);
		}

		public override string ToString()
		{
			return string.Format("FBTexture[0x{0:X8}-0x{1:X8}, {2:D}x{3:D}, bufferWidth={4:D}, pixelFormat={5:D}({6})]", address, address + Length, width, height, bufferWidth, pixelFormat, VideoEngine.getPsmName(pixelFormat));
		}
	}

}