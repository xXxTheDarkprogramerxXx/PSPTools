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
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public abstract class GEProxyTexture : GETexture
	{
		private int fboId = -1;
		protected internal GETexture geTexture;

		public GEProxyTexture(GETexture geTexture, int address, int bufferWidth, int width, int height, int pixelFormat, bool useViewportResize) : base(address, Utilities.makePow2(width), width, height, pixelFormat, useViewportResize)
		{
			this.geTexture = geTexture;
		}

		public override void bind(IRenderingEngine re, bool forDrawing)
		{
			base.bind(re, forDrawing);

			if (isUpdateRequired(re))
			{
				// Update the texture each time the GETexture has changed
				if (fboId == -1)
				{
					fboId = re.genFramebuffer();
					re.bindFramebuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FRAMEBUFFER, fboId);
					re.setFramebufferTexture(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FRAMEBUFFER, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_COLOR_ATTACHMENT0, textureId, 0);
				}
				else
				{
					re.bindFramebuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FRAMEBUFFER, fboId);
				}

				updateTexture(re);

				re.bindFramebuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FRAMEBUFFER, 0);
				re.bindTexture(textureId);
				if (forDrawing)
				{
					re.setTextureFormat(pixelFormat, false);
				}

				geTexture.Changed = false;
			}
		}

		protected internal virtual bool isUpdateRequired(IRenderingEngine re)
		{
			return geTexture.hasChanged();
		}

		protected internal override bool hasChanged()
		{
			return geTexture.hasChanged();
		}

		protected internal abstract void updateTexture(IRenderingEngine re);
	}

}