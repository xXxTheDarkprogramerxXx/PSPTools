using System;

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
namespace pspsharp.graphics
{
	// Based on soywiz/pspemulator
	public class VertexState
	{
		public float[] boneWeights = new float[8];
		public float[] c = new float[4]; // R, G, B, A
		public float[] p = new float[3]; // X, Y, Z
		public float[] n = new float[3]; // NX, NY, NZ
		public float[] t = new float[2]; // U, V

		public virtual void copy(VertexState from)
		{
			if (from != this)
			{
				Array.Copy(from.boneWeights, 0, boneWeights, 0, boneWeights.Length);
				Array.Copy(from.c, 0, c, 0, c.Length);
				Array.Copy(from.p, 0, p, 0, p.Length);
				Array.Copy(from.n, 0, n, 0, n.Length);
				Array.Copy(from.t, 0, t, 0, t.Length);
			}
		}

		public virtual void clear()
		{
			c[0] = c[1] = c[2] = c[3] = 0.0f;
			p[0] = p[1] = p[2] = 0.0f;
			n[0] = n[1] = n[2] = 0.0f;
			t[0] = t[1] = 0.0f;
		}
	}

}