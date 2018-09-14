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

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class SpriteRenderer : BasePrimitiveRenderer
	{
		protected internal VertexState v1;
		protected internal VertexState v2;
		protected internal int sourceDepth;

		protected internal virtual void copy(SpriteRenderer from)
		{
			base.copy(from);
			sourceDepth = from.sourceDepth;
		}

		private SpriteRenderer()
		{
		}

		public SpriteRenderer(GeContext context, CachedTextureResampled texture, bool useVertexTexture)
		{
			init(context, texture, useVertexTexture, false);
		}

		public virtual void setVertex(VertexState v1, VertexState v2)
		{
			this.v1 = v1;
			this.v2 = v2;
			setVertexPositions(v1, v2);
		}

		public virtual bool prepare(GeContext context)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("SpriteRenderer"));
			}

			if (!Visible)
			{
				return false;
			}

			initRendering(context);

			setVertexTextures(context, v1, v2);

			if (transform2D)
			{
				sourceDepth = (int) v2.p[2];
			}
			else
			{
				sourceDepth = (int) prim.p2z;
			}

			return true;
		}

		public virtual IRenderer duplicate()
		{
			SpriteRenderer spriteRenderer = new SpriteRenderer();
			spriteRenderer.copy(this);

			return spriteRenderer;
		}
	}

}