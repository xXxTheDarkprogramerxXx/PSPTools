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
//	import static pspsharp.graphics.GeCommands.LIGHT_AMBIENT_DIFFUSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.LIGHT_DIRECTIONAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.LIGHT_POWER_DIFFUSE_SPECULAR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.LIGHT_SPOT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.ZERO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.addBGR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.getAlpha;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.getColor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.multiplyBGR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.multiplyComponent;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.setAlpha;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.VideoEngine.NUM_LIGHTS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.clamp;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.dot3;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.length3;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.normalize3;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.Pow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.vectorMult34;
	using EnableDisableFlag = pspsharp.graphics.GeContext.EnableDisableFlag;

	//using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public sealed class Lighting
	{
		protected internal static readonly Logger log = VideoEngine.log_Renamed;
		protected internal const bool disableLighting = false;

		private readonly int materialEmission;
		private readonly int ambient;
		private readonly int ambientAlpha;
		private readonly bool[] lightEnabled = new bool[NUM_LIGHTS];
		private readonly bool someLightsEnabled;
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: private readonly float[][] ecLightPosition = new float[NUM_LIGHTS][3];
		private readonly float[][] ecLightPosition = RectangularArrays.ReturnRectangularFloatArray(NUM_LIGHTS, 3);
		private readonly int[] lightKind = new int[NUM_LIGHTS];
		private readonly int[] lightAmbientColor = new int[NUM_LIGHTS];
		private readonly int[] lightDiffuseColor = new int[NUM_LIGHTS];
		private readonly int[] lightSpecularColor = new int[NUM_LIGHTS];
		private readonly float[] constantAttenuation = new float[NUM_LIGHTS];
		private readonly float[] linearAttenuation = new float[NUM_LIGHTS];
		private readonly float[] quadraticAttenuation = new float[NUM_LIGHTS];
		private readonly float[] spotCutoff = new float[NUM_LIGHTS];
		private readonly float[] spotCosCutoff = new float[NUM_LIGHTS];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: private readonly float[][] ecSpotDirection = new float[NUM_LIGHTS][3];
		private readonly float[][] ecSpotDirection = RectangularArrays.ReturnRectangularFloatArray(NUM_LIGHTS, 3);
		private readonly float[] spotExponent = new float[NUM_LIGHTS];
		private float shininess;
		private bool separateSpecularColor;
		private bool[] isSpotLight = new bool[NUM_LIGHTS];
		private bool[] isDirectionalLight = new bool[NUM_LIGHTS];
		private bool hasSomeNonDirectionalLight;
		private readonly float[] L = new float[3];
		private readonly float[] H = new float[3];
		private readonly float[] nL = new float[3];
		private readonly float[] nH = new float[3];
		private readonly float[] nSD = new float[3];
		private readonly float[] Ve = new float[3];
		private readonly float[] Ne = new float[3];
		private int Al;
		private int Dl;
		private int Sl;
		private bool hasNormal;

		public Lighting(float[] viewMatrix, float[] materialEmission, float[] ambient, EnableDisableFlag[] lightEnabled, float[][] lightPosition, int[] lightKind, int[] lightType, float[][] lightAmbientColor, float[][] lightDiffuseColor, float[][] lightSpecularColor, float[] constantAttenuation, float[] linearAttenuation, float[] quadraticAttenuation, float[] spotCutoff, float[] spotCosCutoff, float[][] spotDirection, float[] spotExponent, float shininess, int lightMode, bool hasNormal)
		{
			this.materialEmission = getColor(materialEmission);
			this.ambient = getColor(ambient);
			this.ambientAlpha = getAlpha(this.ambient);
			this.shininess = shininess;
			this.separateSpecularColor = lightMode == GeCommands.LMODE_SEPARATE_SPECULAR_COLOR;
			this.hasNormal = hasNormal;

			bool someLightsEnabled = false;
			bool hasSomeNonDirectionalLight = false;
			for (int l = 0; l < NUM_LIGHTS; l++)
			{
				bool isLightEnabled = lightEnabled[l].Enabled && !disableLighting;
				this.lightEnabled[l] = isLightEnabled;
				if (isLightEnabled)
				{
					someLightsEnabled |= isLightEnabled;
					this.lightKind[l] = lightKind[l];
					this.lightAmbientColor[l] = getColor(lightAmbientColor[l]);
					this.lightDiffuseColor[l] = getColor(lightDiffuseColor[l]);
					this.lightSpecularColor[l] = getColor(lightSpecularColor[l]);
					this.constantAttenuation[l] = constantAttenuation[l];
					this.linearAttenuation[l] = linearAttenuation[l];
					this.quadraticAttenuation[l] = quadraticAttenuation[l];
					this.spotCutoff[l] = spotCutoff[l];
					this.spotCosCutoff[l] = spotCosCutoff[l];
					this.spotExponent[l] = spotExponent[l];
					isSpotLight[l] = lightType[l] == LIGHT_SPOT && spotCutoff[l] < 180.0f;
					isDirectionalLight[l] = lightType[l] == LIGHT_DIRECTIONAL;
					hasSomeNonDirectionalLight |= !isDirectionalLight[l];
					// The Model transformation does not apply to the light positions and directions.
					// Only apply the View transformation to map to the Eye Coordinate system.
					vectorMult34(ecLightPosition[l], viewMatrix, lightPosition[l]);
					vectorMult34(ecSpotDirection[l], viewMatrix, spotDirection[l]);
				}
			}
			this.someLightsEnabled = someLightsEnabled;
			this.hasSomeNonDirectionalLight = hasSomeNonDirectionalLight;
		}

		/// <summary>
		/// This is the equivalent of the vertex shader implementation:
		///     shader.vert: ComputeLight
		/// </summary>
		/// <param name="pixel"> </param>
		/// <param name="l"> </param>
		private void computeLight(int l)
		{
			bool isDirectionalLight = this.isDirectionalLight[l];

			if (!hasNormal && isDirectionalLight)
			{
				// A simple case...
				Al = addBGR(Al, lightAmbientColor[l]);
				return;
			}

			float att = 1.0f;
			L[0] = ecLightPosition[l][0];
			L[1] = ecLightPosition[l][1];
			L[2] = ecLightPosition[l][2];
			if (!isDirectionalLight)
			{
				L[0] -= Ve[0];
				L[1] -= Ve[1];
				L[2] -= Ve[2];

				float d = length3(L);
				att = clamp(1.0f / (constantAttenuation[l] + (linearAttenuation[l] + quadraticAttenuation[l] * d) * d), 0.0f, 1.0f);
				if (isSpotLight[l])
				{
					normalize3(nSD, ecSpotDirection[l]);
					float spot = dot3(nSD, -L[0], -L[1], -L[2]);
					att *= spot < spotCosCutoff[l] ? 0.0f : Pow(spot, spotExponent[l]);
				}
			}

			if (hasNormal)
			{
				H[0] = L[0];
				H[1] = L[1];
				H[2] = L[2] + 1.0f;
				normalize3(nL, L);
				float NdotL = max(dot3(nL, Ne), 0.0f);
				normalize3(nH, H);
				float NdotH = max(dot3(nH, Ne), 0.0f);
				float k = shininess;
				float Dk = lightKind[l] == LIGHT_POWER_DIFFUSE_SPECULAR ? max(Pow(NdotL, k), 0.0f) : NdotL;
				float Sk = lightKind[l] != LIGHT_AMBIENT_DIFFUSE ? max(Pow(NdotH, k), 0.0f) : 0.0f;

				Dl = addBGR(Dl, multiplyBGR(lightDiffuseColor[l], att * Dk));
				Sl = addBGR(Sl, multiplyBGR(lightSpecularColor[l], att * Sk));
			}
			Al = addBGR(Al, multiplyBGR(lightAmbientColor[l], att));
		}

		/// <summary>
		/// Apply the PSP lighting model.
		/// The implementation is based on the vertex shader implementation:
		///     shader.vert: ApplyLighting and ComputeLight
		/// </summary>
		/// <param name="colors">   the primary and secondary colors will be returned in this object </param>
		/// <param name="pixel">    the current pixel values. The following values are used:
		///                 - material colors: materialAmbient, materialDiffuse, materialSpecular
		///                 - normal (in eye coordinates): normalizedNe
		///                 - vertex (in eye coordinates): Ve </param>
		public void applyLighting(PrimarySecondaryColors colors, PixelState pixel)
		{
			if (ambient == unchecked((int)0xFFFFFFFF) && pixel.materialAmbient == unchecked((int)0xFFFFFFFF))
			{
				if (!someLightsEnabled || !separateSpecularColor || !hasNormal)
				{
					// A very simple case...
					colors.primaryColor = ambient;
					colors.secondaryColor = ZERO;
					return;
				}
			}

			int primary = materialEmission;
			int secondary = ZERO;

			Al = ambient;

			if (someLightsEnabled)
			{
				// Get the vector and normal in the eye coordinates
				// (only if they will be used by some light)
				if (hasSomeNonDirectionalLight)
				{
					pixel.getVe(Ve);
				}
				if (hasNormal)
				{
					pixel.getNormalizedNe(Ne);
				}

				Dl = ZERO;
				Sl = ZERO;

				for (int l = 0; l < NUM_LIGHTS; l++)
				{
					if (lightEnabled[l])
					{
						computeLight(l);
					}
				}

				if (Dl != ZERO)
				{
					primary = addBGR(primary, multiplyBGR(Dl, pixel.materialDiffuse));
				}

				if (Sl != ZERO)
				{
					if (separateSpecularColor)
					{
						secondary = multiplyBGR(Sl, pixel.materialSpecular);
					}
					else
					{
						primary = addBGR(primary, multiplyBGR(Sl, pixel.materialSpecular));
					}
				}
			}

			if (Al != ZERO)
			{
				primary = addBGR(primary, multiplyBGR(Al, pixel.materialAmbient));
			}

			primary = setAlpha(primary, multiplyComponent(ambientAlpha, getAlpha(pixel.materialAmbient)));

			colors.primaryColor = primary;
			colors.secondaryColor = secondary;
		}
	}

}