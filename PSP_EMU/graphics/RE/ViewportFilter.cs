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
namespace pspsharp.graphics.RE
{
	using sceDisplay = pspsharp.HLE.modules.sceDisplay;

	public class ViewportFilter : BaseRenderingEngineProxy
	{
		private bool isDirectRendering;

		public ViewportFilter(IRenderingEngine proxy) : base(proxy)
		{
		}

		public override void startDisplay()
		{
			isDirectRendering = false;
			base.startDisplay();
		}

		public override IRenderingEngine RenderingEngine
		{
			set
			{
				base.RenderingEngine = value;
			}
		}

		public override void setViewport(int x, int y, int width, int height)
		{
			// No viewport resizing when rendering in direct mode
			if (!isDirectRendering)
			{
				x = sceDisplay.getResizedWidth(x);
				y = sceDisplay.getResizedHeight(y);
				width = sceDisplay.getResizedWidth(width);
				height = sceDisplay.getResizedHeight(height);
			}
			base.setViewport(x, y, width, height);
		}

		public override void setScissor(int x, int y, int width, int height)
		{
			// No viewport resizing when rendering in direct mode
			if (!isDirectRendering)
			{
				x = sceDisplay.getResizedWidth(x);
				y = sceDisplay.getResizedHeight(y);
				width = sceDisplay.getResizedWidth(width);
				height = sceDisplay.getResizedHeight(height);
			}
			base.setScissor(x, y, width, height);
		}

		public override void endDirectRendering()
		{
			isDirectRendering = false;
			base.endDirectRendering();
		}

		public override void startDirectRendering(bool textureEnabled, bool depthWriteEnabled, bool colorWriteEnabled, bool setOrthoMatrix, bool orthoInverted, int width, int height)
		{
			isDirectRendering = true;
			base.startDirectRendering(textureEnabled, depthWriteEnabled, colorWriteEnabled, setOrthoMatrix, orthoInverted, width, height);
		}
	}

}