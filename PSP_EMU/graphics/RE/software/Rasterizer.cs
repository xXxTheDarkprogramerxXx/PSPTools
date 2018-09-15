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
//	import static pspsharp.util.Utilities.maxInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.minInt;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class Rasterizer
	{
		// There is no advantage of using a Rasterizer when rendering an area smaller
		// than the given width and height.
		// The overhead of the Rasterizer would be too high.
		public const int MINIMUM_WIDTH = 4;
		public const int MINIMUM_HEIGHT = 4;
		private Edge[] edges = new Edge[3];
		private int longEdge;
		private int shortEdge1;
		private int shortEdge2;
		private int currentEdge;
		internal float xdiff1;
		internal float xdiff2;
		internal float factor1;
		internal float factorStep1;
		internal float factor2;
		internal float factorStep2;
		internal float y2;

		public Rasterizer(float x1, float y1, float x2, float y2, float x3, float y3, int yMin, int yMax)
		{
			edges[0] = new Edge(x1, y1, x2, y2);
			edges[1] = new Edge(x2, y2, x3, y3);
			edges[2] = new Edge(x3, y3, x1, y1);

			longEdge = 0;
			float maxLength = edges[longEdge].LengthY;
			for (int i = 1; i < edges.Length; i++)
			{
				float Length = edges[i].LengthY;
				if (Length > maxLength)
				{
					maxLength = Length;
					longEdge = i;
				}
			}

			shortEdge1 = (longEdge + 1) % edges.Length;
			shortEdge2 = (longEdge + 2) % edges.Length;

			if (edges[shortEdge1].y1 > edges[shortEdge2].y1)
			{
				// Switch short edges to start with the one with the lowest "y"
				int tmp = shortEdge1;
				shortEdge1 = shortEdge2;
				shortEdge2 = tmp;
			}

			currentEdge = shortEdge1;
			if (!init(longEdge, currentEdge))
			{
				currentEdge = shortEdge2;
				init(longEdge, currentEdge);
			}
		}

		public virtual float Y
		{
			set
			{
				if (value == y2)
				{
					return;
				}
				if (currentEdge == shortEdge1 && value > edges[currentEdge].y2)
				{
					currentEdge = shortEdge2;
					init(longEdge, currentEdge);
					if (y2 >= value)
					{
						return;
					}
				}
				float diff = value - y2;
				factor1 += diff * factorStep1;
				factor2 += diff * factorStep2;
				y2 = value;
			}
		}

		private bool init(int edge1, int edge2)
		{
			if (edges[edge2].LengthY <= 0)
			{
				y2 = edges[edge2].y2;
				return false;
			}
			float ydiff1 = edges[edge1].y2 - edges[edge1].y1;
			float ydiff2 = edges[edge2].y2 - edges[edge2].y1;
			xdiff1 = edges[edge1].x2 - edges[edge1].x1;
			xdiff2 = edges[edge2].x2 - edges[edge2].x1;
			factor1 = (edges[edge2].y1 - edges[edge1].y1) / ydiff1;
			factorStep1 = 1.0f / ydiff1;
			factor2 = 0.0f;
			factorStep2 = 1.0f / ydiff2;
			y2 = edges[edge2].y1;

			return true;
		}

		public virtual void getNextRange(Range range)
		{
			if (y2 >= edges[currentEdge].y2)
			{
				if (currentEdge == shortEdge2)
				{
					range.clear();
					return;
				}
				currentEdge = shortEdge2;
				if (!init(longEdge, currentEdge))
				{
					range.clear();
					return;
				}
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int edge1 = longEdge;
			int edge1 = longEdge;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int edge2 = currentEdge;
			int edge2 = currentEdge;
			float x1 = edges[edge1].x1 + xdiff1 * factor1;
			float x2 = edges[edge2].x1 + xdiff2 * factor2;
			factor1 += factorStep1;
			factor2 += factorStep2;

			range.setRange(x1, x2);
			y2++;
		}

		public class Range
		{
			public int xMin;
			public int xMax;

			public virtual void clear()
			{
				xMin = 0;
				xMax = 0;
			}

			public virtual void setRange(float x1, float x2)
			{
				xMin = minInt(x1, x2); // minimum value rounded down
				xMax = maxInt(x1, x2); // maximum value rounded up
			}

			public override string ToString()
			{
				return string.Format("[{0:D}-{1:D}]", xMin, xMax);
			}
		}

		private class Edge
		{
			protected internal float x1, y1;
			protected internal float x2, y2;

			public Edge(float x1, float y1, float x2, float y2)
			{
				if (y1 <= y2)
				{
					this.x1 = x1;
					this.y1 = y1;
					this.x2 = x2;
					this.y2 = y2;
				}
				else
				{
					this.x1 = x2;
					this.y1 = y2;
					this.x2 = x1;
					this.y2 = y1;
				}
			}

			public virtual float LengthY
			{
				get
				{
					return y2 - y1;
				}
			}

			public override string ToString()
			{
				return string.Format("({0:D},{1:D})-({2:D},{3:D})", x1, y1, x2, y2);
			}
		}
	}

}