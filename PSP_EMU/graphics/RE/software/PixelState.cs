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
//	import static System.arraycopy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.getColor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.normalize3;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.round;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.vectorMult33;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.vectorMult34;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public sealed class PixelState
	{
		public int materialAmbient;
		public int materialDiffuse;
		public int materialSpecular;
		public bool hasNormal;
		private bool computedV;
		private bool computedN;
		private bool computedNormalizedN;
		private bool computedVe;
		private bool computedNe;
		private bool computedNormalizedNe;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly float[] V_Renamed = new float[] {0.0f, 0.0f, 0.0f, 1.0f};
		private readonly float[] Ve = new float[3];
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly float[] N_Renamed = new float[] {0.0f, 0.0f, 1.0f};
		private readonly float[] normalizedN = new float[] {0.0f, 0.0f, 1.0f};
		private readonly float[] Ne = new float[] {0.0f, 0.0f, 1.0f};
		private readonly float[] normalizedNe = new float[] {0.0f, 0.0f, 1.0f};
		public float v1x, v1y, v1z;
		public float v2x, v2y, v2z;
		public float v3x, v3y, v3z;
		public float n1x, n1y, n1z;
		public float n2x, n2y, n2z;
		public float n3x, n3y, n3z;
		public int c1a, c1b, c1g, c1r;
		public int c2a, c2b, c2g, c2r;
		public int c3a, c3b, c3g, c3r, c3;
		public readonly float[] textureMatrix = new float[16];
		public readonly float[] modelViewMatrix = new float[16];
		public readonly float[] modelViewProjectionMatrix = new float[16];
		public readonly float[] normalMatrix = new float[16];
		private int numberPixels;

		public float triangleWeight1;
		public float triangleWeight2;
		public float triangleWeight3;

		protected internal void copy(PixelState from)
		{
			materialAmbient = from.materialAmbient;
			materialDiffuse = from.materialDiffuse;
			materialSpecular = from.materialSpecular;
			hasNormal = from.hasNormal;
			v1x = from.v1x;
			v1y = from.v1y;
			v1z = from.v1z;
			v2x = from.v2x;
			v2y = from.v2y;
			v2z = from.v2z;
			v3x = from.v3x;
			v3y = from.v3y;
			v3z = from.v3z;
			n1x = from.n1x;
			n1y = from.n1y;
			n1z = from.n1z;
			n2x = from.n2x;
			n2y = from.n2y;
			n2z = from.n2z;
			n3x = from.n3x;
			n3y = from.n3y;
			n3z = from.n3z;
			c1a = from.c1a;
			c1b = from.c1b;
			c1g = from.c1g;
			c1r = from.c1r;
			c2a = from.c2a;
			c2b = from.c2b;
			c2g = from.c2g;
			c2r = from.c2r;
			c3a = from.c3a;
			c3b = from.c3b;
			c3g = from.c3g;
			c3r = from.c3r;
			c3 = from.c3;
			arraycopy(from.textureMatrix, 0, textureMatrix, 0, textureMatrix.Length);
			arraycopy(from.modelViewMatrix, 0, modelViewMatrix, 0, modelViewMatrix.Length);
			arraycopy(from.modelViewProjectionMatrix, 0, modelViewProjectionMatrix, 0, modelViewProjectionMatrix.Length);
			if (hasNormal)
			{
				arraycopy(from.normalMatrix, 0, normalMatrix, 0, normalMatrix.Length);
			}
		}

		public float getTriangleWeightedValue(float value1, float value2, float value3)
		{
			return triangleWeight1 * value1 + triangleWeight2 * value2 + triangleWeight3 * value3;
		}

		public int getTriangleWeightedValue(int value1, int value2, int value3)
		{
			return round(triangleWeight1 * value1 + triangleWeight2 * value2 + triangleWeight3 * value3);
		}

		public int TriangleColorWeightedValue
		{
			get
			{
				int a = getTriangleWeightedValue(c1a, c2a, c3a);
				int b = getTriangleWeightedValue(c1b, c2b, c3b);
				int g = getTriangleWeightedValue(c1g, c2g, c3g);
				int r = getTriangleWeightedValue(c1r, c2r, c3r);
				return getColor(a, b, g, r);
			}
		}

		public bool InsideTriangle
		{
			get
			{
				const float limit = -1e-5f; // The limit should be 0.0f. Allowing small rounding errors.
				return triangleWeight1 >= limit && triangleWeight2 >= limit && triangleWeight3 >= limit;
			}
		}

		public void newPixel2D()
		{
			numberPixels++;
		}

		public void newPixel3D()
		{
			newPixel2D();

			computedV = false;
			computedN = false;
			computedNormalizedN = false;
			computedVe = false;
			computedNe = false;
			computedNormalizedNe = false;
		}

		public void reset()
		{
			numberPixels = 0;
		}

		public int NumberPixels
		{
			get
			{
				return numberPixels;
			}
		}

		private void computeV()
		{
			if (!computedV)
			{
				V_Renamed[0] = getTriangleWeightedValue(v1x, v2x, v3x);
				V_Renamed[1] = getTriangleWeightedValue(v1y, v2y, v3y);
				V_Renamed[2] = getTriangleWeightedValue(v1z, v2z, v3z);
				computedV = true;
			}
		}

		public float[] V
		{
			get
			{
				computeV();
				return V_Renamed;
			}
		}

		public void getV(float[] V)
		{
			computeV();
			V[0] = this.V_Renamed[0];
			V[1] = this.V_Renamed[1];
			V[2] = this.V_Renamed[2];
		}

		private void computeN()
		{
			if (!computedN && hasNormal)
			{
				N_Renamed[0] = getTriangleWeightedValue(n1x, n2x, n3x);
				N_Renamed[1] = getTriangleWeightedValue(n1y, n2y, n3y);
				N_Renamed[2] = getTriangleWeightedValue(n1z, n2z, n3z);
				computedN = true;
			}
		}

		public float[] N
		{
			get
			{
				computeN();
				return N_Renamed;
			}
		}

		public void getN(float[] N)
		{
			computeN();
			N[0] = this.N_Renamed[0];
			N[1] = this.N_Renamed[1];
			N[2] = this.N_Renamed[2];
		}

		private void computeNormalizedN()
		{
			if (!computedNormalizedN && hasNormal)
			{
				computeN();
				normalize3(normalizedN, N_Renamed);
				computedNormalizedN = true;
			}
		}

		public float[] NormalizedN
		{
			get
			{
				computeNormalizedN();
				return normalizedN;
			}
		}

		public void getNormalizedN(float[] normalizedN)
		{
			computeNormalizedN();
			normalizedN[0] = this.normalizedN[0];
			normalizedN[1] = this.normalizedN[1];
			normalizedN[2] = this.normalizedN[2];
		}

		private void computeVe()
		{
			if (!computedVe)
			{
				computeV();
				vectorMult34(Ve, modelViewMatrix, V_Renamed);
				computedVe = true;
			}
		}

		public void getVe(float[] Ve)
		{
			computeVe();
			Ve[0] = this.Ve[0];
			Ve[1] = this.Ve[1];
			Ve[2] = this.Ve[2];
		}

		private void computeNe()
		{
			if (!computedNe && hasNormal)
			{
				computeN();
				vectorMult33(Ne, normalMatrix, N_Renamed);
				computedNe = true;
			}
		}

		public void getNe(float[] Ne)
		{
			computeNe();
			Ne[0] = this.Ne[0];
			Ne[1] = this.Ne[1];
			Ne[2] = this.Ne[2];
		}

		private void computeNormalizedNe()
		{
			if (!computedNormalizedNe && hasNormal)
			{
				computeNe();
				normalize3(normalizedNe, Ne);
				computedNormalizedNe = true;
			}
		}

		public void getNormalizedNe(float[] normalizedNe)
		{
			computeNormalizedNe();
			normalizedNe[0] = this.normalizedNe[0];
			normalizedNe[1] = this.normalizedNe[1];
			normalizedNe[2] = this.normalizedNe[2];
		}
	}

}