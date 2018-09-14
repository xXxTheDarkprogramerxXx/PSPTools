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
	public class BoundingBoxRenderer : BasePrimitiveRenderer
	{
		protected internal float[][] boundingBoxPositions;

		public BoundingBoxRenderer(GeContext context)
		{
			init(context, null, false, false);
		}

		public virtual void drawBoundingBox(float[][] boundingBoxPositions)
		{
			this.boundingBoxPositions = boundingBoxPositions;
		}

		public virtual bool prepare(GeContext context)
		{
			for (int i = 0; i < boundingBoxPositions.Length; i++)
			{
				addPosition(boundingBoxPositions[i]);
			}

			if (!insideScissor())
			{
				return false;
			}

			return true;
		}

		public override void render()
		{
		}

		public virtual IRenderer duplicate()
		{
			return this;
		}
	}

}