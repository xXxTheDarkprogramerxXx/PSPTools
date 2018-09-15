using System;
using System.Collections.Generic;

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
//	import static pspsharp.graphics.RE.software.PixelColor.doubleColor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.getColor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.invertMatrix3x3;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.matrixMult;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.maxInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.minInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.round;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.transposeMatrix3x3;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.vectorMult44;


	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using DurationStatistics = pspsharp.util.DurationStatistics;
	using LongLongKey = pspsharp.util.LongLongKey;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// This class extends the BaseRenderer class to include
	/// vertex specific information.
	/// The methods from this class can be used to set the vertex
	/// information specific for the rendering of one primitive (e.g. one triangle)
	/// </summary>
	public abstract class BasePrimitiveRenderer : BaseRenderer
	{
		private static Dictionary<int, DurationStatistics> pixelsStatistics = new Dictionary<int, DurationStatistics>();
		private DurationStatistics pixelStatistics = new DurationStatistics();
		public readonly PixelState pixel = new PixelState();
		public PrimitiveState prim = new PrimitiveState();
		protected internal bool needScissoringX;
		protected internal bool needScissoringY;
		protected internal bool needSourceDepthRead;
		protected internal bool needDestinationDepthRead;
		protected internal bool needDepthWrite;
		protected internal bool needTextureUV;
		protected internal bool swapTextureUV;
		protected internal bool simpleTextureUV;
		protected internal bool needTextureWrapU;
		protected internal bool needTextureWrapV;
		protected internal bool sameVertexColor;
		protected internal bool needSourceDepthClamp;
		public int fbAddress;
		public int depthAddress;
		public IRendererWriter rendererWriter;

		protected internal virtual void copy(BasePrimitiveRenderer from)
		{
			base.copy(from);
			pixel.copy(from.pixel);
			prim.copy(from.prim);
			needScissoringX = from.needScissoringX;
			needScissoringY = from.needScissoringY;
			needSourceDepthRead = from.needSourceDepthRead;
			needDestinationDepthRead = from.needDestinationDepthRead;
			needDepthWrite = from.needDepthWrite;
			needTextureUV = from.needTextureUV;
			simpleTextureUV = from.simpleTextureUV;
			needTextureWrapU = from.needTextureWrapU;
			needTextureWrapV = from.needTextureWrapV;
			sameVertexColor = from.sameVertexColor;
			fbAddress = from.fbAddress;
			depthAddress = from.depthAddress;
			rendererWriter = from.rendererWriter;
			needSourceDepthClamp = from.needSourceDepthClamp;
		}

		protected internal override void init(GeContext context, CachedTextureResampled texture, bool useVertexTexture, bool isTriangle)
		{
			base.init(context, texture, useVertexTexture, isTriangle);

			prim.pxMax = int.MinValue;
			prim.pxMin = int.MaxValue;
			prim.pyMax = int.MinValue;
			prim.pyMin = int.MaxValue;
			prim.pzMax = int.MinValue;
			prim.pzMin = int.MaxValue;

			if (!transform2D)
			{
				// Pre-compute the Model-View matrix
				matrixMult(pixel.modelViewMatrix, context.view_uploaded_matrix, context.model_uploaded_matrix);

				// Pre-compute the Model-View-Projection matrix
				matrixMult(pixel.modelViewProjectionMatrix, context.proj_uploaded_matrix, pixel.modelViewMatrix);
			}
		}

		protected internal override void initRendering(GeContext context)
		{
			if (renderingInitialized)
			{
				return;
			}

			base.initRendering(context);

			pixel.materialAmbient = getColor(context.mat_ambient);
			pixel.materialDiffuse = getColor(context.mat_diffuse);
			pixel.materialSpecular = getColor(context.mat_specular);

			if (!transform2D)
			{
				if (context.tex_map_mode == GeCommands.TMAP_TEXTURE_MAP_MODE_TEXTURE_MATRIX)
				{
					// Copy the Texture matrix
					Array.Copy(context.texture_uploaded_matrix, 0, pixel.textureMatrix, 0, pixel.textureMatrix.Length);
				}

				pixel.hasNormal = context.vinfo.normal != 0;

				if (pixel.hasNormal)
				{
					// Pre-compute the matrix to transform a normal to the eye coordinates
					// See http://www.lighthouse3d.com/tutorials/glsl-tutorial/the-normal-matrix/
					float[] invertedModelViewMatrix = new float[16];
					if (invertMatrix3x3(invertedModelViewMatrix, pixel.modelViewMatrix))
					{
						transposeMatrix3x3(pixel.normalMatrix, invertedModelViewMatrix);
					}
					else
					{
						// What is using the PSP in this case? Assume it just takes the Model-View matrix
						Array.Copy(pixel.modelViewMatrix, 0, pixel.normalMatrix, 0, pixel.normalMatrix.Length);
						if (isLogDebugEnabled)
						{
							Console.WriteLine(string.Format("ModelView matrix cannot be inverted, taking the Model-View matrix itself!"));
						}
					}
				}
			}
		}

		protected internal virtual void addPosition(float[] p)
		{
			float[] screenCoordinates = new float[4];
			getScreenCoordinates(screenCoordinates, p);
			prim.pxMax = maxInt(prim.pxMax, screenCoordinates[0]);
			prim.pxMin = minInt(prim.pxMin, screenCoordinates[0]);
			prim.pyMax = maxInt(prim.pyMax, screenCoordinates[1]);
			prim.pyMin = minInt(prim.pyMin, screenCoordinates[1]);
			prim.pzMax = maxInt(prim.pzMax, screenCoordinates[2]);
			prim.pzMin = minInt(prim.pzMin, screenCoordinates[2]);
		}

		protected internal virtual void setVertexPositions(VertexState v1, VertexState v2, VertexState v3)
		{
			setPositions(v1, v2, v3);
		}

		protected internal virtual void setVertexPositions(VertexState v1, VertexState v2)
		{
			setPositions(v1, v2);
		}

		protected internal virtual void setVertexTextures(GeContext context, VertexState v1, VertexState v2, VertexState v3)
		{
			setTextures(v1, v2, v3);
			setVertexTextures(context, v1.c, v2.c, v3.c);
		}

		protected internal virtual void setVertexTextures(GeContext context, VertexState v1, VertexState v2)
		{
			setTextures(v1, v2);
			setVertexTextures(context, v1.c, v2.c, null);
		}

		private void setVertexTextures(GeContext context, float[] c1, float[] c2, float[] c3)
		{
			textureWidth = context.texture_width[mipmapLevel];
			textureHeight = context.texture_height[mipmapLevel];

			// The rendering will be performed into the following ranges:
			// 3D:
			//   - x: [pxMin..pxMax] (min and max included)
			//   - y: [pxMin..pxMax] (min and max included)
			// 2D:
			//   - x: [pxMin..pxMax-1] (min included but max excluded)
			//   - y: [pxMin..pxMax-1] (min included but max excluded)
			if (transform2D)
			{
				prim.pxMax--;
				prim.pyMax--;
			}
			else
			{
				// Restrict the drawn area to the scissor area.
				// We can just update the min/max values, the TextureMapping filter
				// will take are of the correct texture mapping.
				// We do no longer need a scissoring filter.
				if (needScissoringX)
				{
					prim.pxMin = max(prim.pxMin, scissorX1);
					prim.pxMax = min(prim.pxMax, scissorX2);
					needScissoringX = false;
				}
				if (needScissoringY)
				{
					prim.pyMin = max(prim.pyMin, scissorY1);
					prim.pyMax = min(prim.pyMax, scissorY2);
					needScissoringY = false;
				}
			}
			prim.destinationWidth = prim.pxMax - prim.pxMin + 1;
			prim.destinationHeight = prim.pyMax - prim.pyMin + 1;

			if (isUsingTexture(context))
			{
				simpleTextureUV = !isTriangle;

				if (!simpleTextureUV && isTriangle && transform2D)
				{
					// Check if the 2D triangle can be rendered using a simple texture UV mapping:
					// this is only possible when the triangle has a square angle.
					//
					// 1---2     1---2     1             1
					// |  /       \  |     | \         / |
					// | /         \ |     |  \       /  |
					// 3             3     3---2     3---2
					//
					// 1---3     1---3     1             1
					// |  /       \  |     | \         / |
					// | /         \ |     |  \       /  |
					// 2             2     2---3     2---3
					//
					if (prim.p1x == prim.p2x && prim.t1u == prim.t2u)
					{
						if (prim.p1y == prim.p3y && prim.t1v == prim.t3v)
						{
							simpleTextureUV = true;
						}
						else if (prim.p2y == prim.p3y && prim.t2v == prim.t3v)
						{
							simpleTextureUV = true;
						}
					}
					else if (prim.p1x == prim.p3x && prim.t1u == prim.t3u)
					{
						if (prim.p1y == prim.p2y && prim.t1v == prim.t2v)
						{
							simpleTextureUV = true;
						}
						else if (prim.p2y == prim.p3y && prim.t2v == prim.t3v)
						{
							simpleTextureUV = true;
						}
					}
					else if (prim.p2x == prim.p3x && prim.t2u == prim.t3u)
					{
						if (prim.p1y == prim.p2y && prim.t1v == prim.t2v)
						{
							simpleTextureUV = true;
						}
						else if (prim.p1y == prim.p1y && prim.t2v == prim.t3v)
						{
							simpleTextureUV = true;
						}
					}
				}

				if (simpleTextureUV)
				{
					if (transform2D)
					{
						bool flipX = false;
						bool flipY = false;
						if (isTriangle)
						{
							// Compute texture flips for a triangle
							if (prim.t1u != prim.t2u)
							{
								flipX = (prim.t1u > prim.t2u) ^ (prim.p1x > prim.p2x);
							}
							if (prim.t1v != prim.t2v)
							{
								flipY = (prim.t1v > prim.t2v) ^ (prim.p1y > prim.p2y);
							}
							if (!flipX && prim.t2u != prim.t3u)
							{
								flipX = (prim.t2u > prim.t3u) ^ (prim.p2x > prim.p3x);
							}
							if (!flipY && prim.t2v != prim.t3v)
							{
								flipY = (prim.t2v > prim.t3v) ^ (prim.p2y > prim.p3y);
							}
						}
						else
						{
							// Compute texture flips for a sprite
							flipX = (prim.t1u > prim.t2u) ^ (prim.p1x > prim.p2x);
							flipY = (prim.t1v > prim.t2v) ^ (prim.p1y > prim.p2y);
							if (flipX && flipY)
							{
								swapTextureUV = true;
								flipX = false;
								flipY = false;
							}
						}
						if (isLogTraceEnabled)
						{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.trace(String.format("2D texture flipX=%b, flipY=%b, swapUV=%b, point (%d,%d)-(%d,%d), texture (%d,%d)-(%d,%d)", flipX, flipY, swapTextureUV, prim.pxMin, prim.pyMin, prim.pxMax, prim.pyMax, prim.tuMin, prim.tvMin, prim.tuMax, prim.tvMax));
							log.trace(string.Format("2D texture flipX=%b, flipY=%b, swapUV=%b, point (%d,%d)-(%d,%d), texture (%d,%d)-(%d,%d)", flipX, flipY, swapTextureUV, prim.pxMin, prim.pyMin, prim.pxMax, prim.pyMax, prim.tuMin, prim.tvMin, prim.tuMax, prim.tvMax));
						}
						prim.uStart = flipX ? prim.tuMax : prim.tuMin;
						float uEnd = flipX ? prim.tuMin : prim.tuMax;
						prim.vStart = flipY ? prim.tvMax : prim.tvMin;
						float vEnd = flipY ? prim.tvMin : prim.tvMax;
						prim.uStep = (uEnd - prim.uStart) / (swapTextureUV ? prim.destinationHeight : prim.destinationWidth);
						prim.vStep = (vEnd - prim.vStart) / (swapTextureUV ? prim.destinationWidth : prim.destinationHeight);
					}
					else
					{
						// 3D sprite
						prim.uStart = prim.t1u;
						float uEnd = prim.t2u;
						prim.vStart = prim.t1v;
						float vEnd = prim.t2v;
						if (prim.p1x == prim.p2x)
						{
							prim.uStep = 1f;
						}
						else
						{
							prim.uStep = (uEnd - prim.uStart) / System.Math.Abs(prim.p2x - prim.p1x);
						}
						if (prim.p1y == prim.p2y)
						{
							prim.vStep = 1f;
						}
						else
						{
							prim.vStep = (vEnd - prim.vStart) / System.Math.Abs(prim.p2y - prim.p1y);
						}
						if (isLogTraceEnabled)
						{
							log.trace(string.Format("3D sprite uStart={0:F}, uStep={1:F}, vStart={2:F}, vStep={3:F}, texTranslateX={4:F}, texTranslateY={5:F}, texScaleX={6:F}, texScaleY={7:F}, point ({8:D},{9:D})-({10:D},{11:D}), texture ({12:D},{13:D})-({14:D},{15:D})", prim.uStart, prim.uStep, prim.vStart, prim.vStep, texTranslateX, texTranslateY, texScaleX, texScaleY, prim.pxMin, prim.pyMin, prim.pxMax, prim.pyMax, prim.tuMin, prim.tvMin, prim.tuMax, prim.tvMax));
						}
					}

					// Perform scissoring and update uStart/uStep and vStart/vStep
					if (needScissoringX)
					{
						int deltaX = scissorX1 - prim.pxMin;
						if (deltaX > 0)
						{
							prim.uStart += prim.uStep * deltaX;
							prim.pxMin += deltaX;
							if (transform2D)
							{
								prim.tuMin += round(prim.uStep * deltaX);
							}
						}
						deltaX = prim.pxMax - scissorX2;
						if (deltaX > 0)
						{
							prim.pxMax -= deltaX;
							if (transform2D)
							{
								prim.tuMax -= round(prim.uStep * deltaX);
							}
						}
						prim.destinationWidth = prim.pxMax - prim.pxMin + 1;
						needScissoringX = false;
					}
					if (needScissoringY)
					{
						int deltaY = scissorY1 - prim.pyMin;
						if (deltaY > 0)
						{
							prim.vStart += prim.vStep * deltaY;
							prim.pyMin += deltaY;
							if (transform2D)
							{
								prim.tvMin += round(prim.vStep * deltaY);
							}
						}
						deltaY = prim.pyMax - scissorY2;
						if (deltaY > 0)
						{
							prim.pyMax -= deltaY;
							if (transform2D)
							{
								prim.tvMax -= round(prim.vStep * deltaY);
							}
						}
						prim.destinationHeight = prim.pyMax - prim.pyMin + 1;
						needScissoringY = false;
					}
				}
			}

			if (setVertexPrimaryColor)
			{
				if (c3 != null)
				{
					pixel.c1a = getColor(c1[3]);
					pixel.c1b = getColor(c1[2]);
					pixel.c1g = getColor(c1[1]);
					pixel.c1r = getColor(c1[0]);
					pixel.c2a = getColor(c2[3]);
					pixel.c2b = getColor(c2[2]);
					pixel.c2g = getColor(c2[1]);
					pixel.c2r = getColor(c2[0]);
					pixel.c3a = getColor(c3[3]);
					pixel.c3b = getColor(c3[2]);
					pixel.c3g = getColor(c3[1]);
					pixel.c3r = getColor(c3[0]);
					pixel.c3 = getColor(c3);
				}
				if (isTriangle)
				{
					if (context.shadeModel == GeCommands.SHADE_TYPE_FLAT)
					{
						// Flat shade model: always use the color of the 3rd triangle vertex
						sameVertexColor = true;
					}
					else
					{
						sameVertexColor = Utilities.sameColor(c1, c2, c3);
					}
					// For triangles, take the weighted color from the 3 vertices.
				}
				else
				{
					// For sprites, take only the color from the 2nd vertex
					primaryColor = getColor(c2);
					if (context.textureColorDoubled)
					{
						primaryColor = doubleColor(primaryColor);
					}
				}
			}

			// Try to avoid to compute expensive values
			needDepthWrite = getNeedDepthWrite(context);
			needSourceDepthRead = needDepthWrite || getNeedSourceDepthRead(context);
			needDestinationDepthRead = getNeedDestinationDepthRead(context, needDepthWrite);
			if (zbw <= 0)
			{
				needDepthWrite = false;
				needSourceDepthRead = false;
				needDestinationDepthRead = false;
			}
			needTextureUV = getNeedTextureUV(context);
			if (transform2D)
			{
				needTextureWrapU = prim.tuMin < 0 || prim.tuMax >= context.texture_width[mipmapLevel];
				needTextureWrapV = prim.tvMin < 0 || prim.tvMax >= context.texture_height[mipmapLevel];
			}
			else
			{
				if (context.tex_map_mode != GeCommands.TMAP_TEXTURE_MAP_MODE_TEXTURE_COORDIATES_UV)
				{
					needTextureWrapU = true;
					needTextureWrapV = true;
				}
				else if (isTriangle && (prim.p1w <= 0f || prim.p2w <= 0f || prim.p3w <= 0f))
				{
					// Need texture wrapping if one triangle point is behind the eye:
					// the texture coordinates might exceed the calculated range due to the perspective correction.
					needTextureWrapU = true;
					needTextureWrapV = true;
				}
				else
				{
					float tuMin, tuMax;
					float tvMin, tvMax;
					if (isTriangle)
					{
						tuMin = Utilities.min(prim.t1u, Utilities.min(prim.t2u, prim.t3u));
						tuMax = Utilities.max(prim.t1u, Utilities.max(prim.t2u, prim.t3u));
						tvMin = Utilities.min(prim.t1v, Utilities.min(prim.t2v, prim.t3v));
						tvMax = Utilities.max(prim.t1v, Utilities.max(prim.t2v, prim.t3v));
					}
					else
					{
						tuMin = Utilities.min(prim.t1u, prim.t2u);
						tuMax = Utilities.max(prim.t1u, prim.t2u);
						tvMin = Utilities.min(prim.t1v, prim.t2v);
						tvMax = Utilities.max(prim.t1v, prim.t2v);
					}
					tuMin = tuMin * texScaleX + texTranslateX;
					tuMax = tuMax * texScaleX + texTranslateX;
					tvMin = tvMin * texScaleY + texTranslateY;
					tvMax = tvMax * texScaleY + texTranslateY;
					needTextureWrapU = tuMin < 0f || tuMin >= 0.99999f || tuMax < 0f || tuMax >= 0.99999f;
					needTextureWrapV = tvMin < 0f || tvMin >= 0.99999f || tvMax < 0f || tvMax >= 0.99999f;
				}
			}
			needSourceDepthClamp = false;
			if (needDepthWrite && needSourceDepthRead && isTriangle)
			{
				if (prim.p1z < 0f || prim.p2z < 0f || prim.p3z < 0f)
				{
					needSourceDepthClamp = true;
				}
				else if (prim.p1z > 65535f || prim.p2z > 65535f || prim.p3z > 65535f)
				{
					needSourceDepthClamp = true;
				}
			}

			prepareWriters();

			LongLongKey rendererKey = RendererKey;
			if (compiledRenderer == null || !rendererKey.Equals(compiledRendererKey))
			{
				compiledRendererKey = rendererKey;
				compiledRenderer = FilterCompiler.Instance.getCompiledRenderer(this, rendererKey, context);
				if (isLogTraceEnabled)
				{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					log.trace(string.Format("Rendering using compiled renderer {0}", compiledRenderer.GetType().FullName));
				}
			}

			if (c3 != null)
			{
				prim.preComputeTriangleWeights();
			}
		}

		private bool getNeedSourceDepthRead(GeContext context)
		{
			if (!clearMode && context.depthTestFlag.Enabled)
			{
				if (context.depthFunc != GeCommands.ZTST_FUNCTION_NEVER_PASS_PIXEL && context.depthFunc != GeCommands.ZTST_FUNCTION_ALWAYS_PASS_PIXEL)
				{
					return true;
				}
			}

			if (!clearMode && !transform2D)
			{
				if (nearZ != 0x0000 || farZ != 0xFFFF)
				{
					return true;
				}
			}

			return false;
		}

		private bool getNeedDestinationDepthRead(GeContext context, bool needDepthWrite)
		{
			if (!clearMode && context.depthTestFlag.Enabled)
			{
				if (context.depthFunc != GeCommands.ZTST_FUNCTION_NEVER_PASS_PIXEL && context.depthFunc != GeCommands.ZTST_FUNCTION_ALWAYS_PASS_PIXEL)
				{
					return true;
				}
			}

			if (clearMode)
			{
				// Depth writes disabled
				if (!context.clearModeDepth)
				{
					return true;
				}
			}
			else if (!context.depthTestFlag.Enabled && needDepthWrite)
			{
				// Depth writes are disabled when the depth test is not enabled.
				return true;
			}
			else if (!context.depthMask && needDepthWrite)
			{
				// Depth writes disabled
				return true;
			}

			return false;
		}

		private bool getNeedDepthWrite(GeContext context)
		{
			if (clearMode)
			{
				// Depth writes disabled
				if (!context.clearModeDepth)
				{
					return false;
				}
			}
			else if (!context.depthTestFlag.Enabled)
			{
				// Depth writes are disabled when the depth test is not enabled.
				return false;
			}
			else if (!context.depthMask)
			{
				// Depth writes disabled
				return false;
			}

			return true;
		}

		private bool getNeedTextureUV(GeContext context)
		{
			if (context.textureFlag.Enabled && (!transform2D || useVertexTexture) && !clearMode)
			{
				// UV always required by the texture reader
				return true;
			}

			return false;
		}

		/// <summary>
		/// Transform a floating-point 2D position coordinate to a screen coordinate.
		/// The PSP (tested using 3DStudio) is applying the following transformation:
		///    0.562 transformed to 0
		///    0.563 transformed to 1
		/// </summary>
		/// <param name="value">  floating-point 2D position coordinate </param>
		/// <returns>       screen coordinate </returns>
		protected internal static int positionCoordinate2D(float value)
		{
			return (int)(value + 0.437f);
		}

		private void setPositions(VertexState v1, VertexState v2)
		{
			pixel.v1x = v1.p[0];
			pixel.v1y = v1.p[1];
			pixel.v1z = v1.p[2];
			pixel.n1x = v1.n[0];
			pixel.n1y = v1.n[1];
			pixel.n1z = v1.n[2];

			pixel.v2x = v2.p[0];
			pixel.v2y = v2.p[1];
			pixel.v2z = v2.p[2];
			pixel.n2x = v2.n[0];
			pixel.n2y = v2.n[1];
			pixel.n2z = v2.n[2];

			if (transform2D)
			{
				prim.p1x = pixel.v1x;
				prim.p1y = pixel.v1y;
				prim.p1z = pixel.v1z;
				prim.p2x = pixel.v2x;
				prim.p2y = pixel.v2y;
				prim.p2z = pixel.v2z;

				// TODO Need more investigation
	//        	prim.pxMax = max(positionCoordinate2D(prim.p1x), positionCoordinate2D(prim.p2x));
	//        	prim.pxMin = min(positionCoordinate2D(prim.p1x), positionCoordinate2D(prim.p2x));
	//        	prim.pyMax = max(positionCoordinate2D(prim.p1y), positionCoordinate2D(prim.p2y));
	//        	prim.pyMin = min(positionCoordinate2D(prim.p1y), positionCoordinate2D(prim.p2y));
	//        	prim.pzMax = max(positionCoordinate2D(prim.p1z), positionCoordinate2D(prim.p2z));
	//        	prim.pzMin = min(positionCoordinate2D(prim.p1z), positionCoordinate2D(prim.p2z));
				prim.pxMax = maxInt(prim.p1x, prim.p2x);
				prim.pxMin = minInt(prim.p1x, prim.p2x);
				prim.pyMax = maxInt(prim.p1y, prim.p2y);
				prim.pyMin = minInt(prim.p1y, prim.p2y);
				prim.pzMax = maxInt(prim.p1z, prim.p2z);
				prim.pzMin = minInt(prim.p1z, prim.p2z);
			}
			else
			{
				float[] screenCoordinates = new float[4];
				getScreenCoordinates(screenCoordinates, pixel.v1x, pixel.v1y, pixel.v1z);
				prim.p1x = screenCoordinates[0];
				prim.p1y = screenCoordinates[1];
				prim.p1z = screenCoordinates[2];
				prim.p1w = screenCoordinates[3];
				prim.p1wInverted = 1.0f / prim.p1w;
				getScreenCoordinates(screenCoordinates, pixel.v2x, pixel.v2y, pixel.v2z);
				prim.p2x = screenCoordinates[0];
				prim.p2y = screenCoordinates[1];
				prim.p2z = screenCoordinates[2];
				prim.p2w = screenCoordinates[3];
				prim.p2wInverted = 1.0f / prim.p2w;

				prim.pxMax = maxInt(prim.p1x, prim.p2x);
				prim.pxMin = minInt(prim.p1x, prim.p2x);
				prim.pyMax = maxInt(prim.p1y, prim.p2y);
				prim.pyMin = minInt(prim.p1y, prim.p2y);
				prim.pzMax = maxInt(prim.p1z, prim.p2z);
				prim.pzMin = minInt(prim.p1z, prim.p2z);
			}
		}

		private void setPositions(VertexState v1, VertexState v2, VertexState v3)
		{
			setPositions(v1, v2);

			pixel.v3x = v3.p[0];
			pixel.v3y = v3.p[1];
			pixel.v3z = v3.p[2];
			pixel.n3x = v3.n[0];
			pixel.n3y = v3.n[1];
			pixel.n3z = v3.n[2];

			if (transform2D)
			{
				prim.p3x = pixel.v3x;
				prim.p3y = pixel.v3y;
				prim.p3z = pixel.v3z;

				// TODO Need more investigation
	//        	prim.pxMax = max(prim.pxMax, positionCoordinate2D(prim.p3x));
	//        	prim.pxMin = min(prim.pxMin, positionCoordinate2D(prim.p3x));
	//        	prim.pyMax = max(prim.pyMax, positionCoordinate2D(prim.p3y));
	//        	prim.pyMin = min(prim.pyMin, positionCoordinate2D(prim.p3y));
	//        	prim.pzMax = max(prim.pzMax, positionCoordinate2D(prim.p3z));
	//        	prim.pzMin = min(prim.pzMin, positionCoordinate2D(prim.p3z));
				prim.pxMax = maxInt(prim.pxMax, prim.p3x);
				prim.pxMin = minInt(prim.pxMin, prim.p3x);
				prim.pyMax = maxInt(prim.pyMax, prim.p3y);
				prim.pyMin = minInt(prim.pyMin, prim.p3y);
				prim.pzMax = maxInt(prim.pzMax, prim.p3z);
				prim.pzMin = minInt(prim.pzMin, prim.p3z);
			}
			else
			{
				float[] screenCoordinates = new float[4];
				getScreenCoordinates(screenCoordinates, pixel.v3x, pixel.v3y, pixel.v3z);
				prim.p3x = screenCoordinates[0];
				prim.p3y = screenCoordinates[1];
				prim.p3z = screenCoordinates[2];
				prim.p3w = screenCoordinates[3];
				prim.p3wInverted = 1.0f / prim.p3w;

				prim.pxMax = maxInt(prim.pxMax, prim.p3x);
				prim.pxMin = minInt(prim.pxMin, prim.p3x);
				prim.pyMax = maxInt(prim.pyMax, prim.p3y);
				prim.pyMin = minInt(prim.pyMin, prim.p3y);
				prim.pzMax = maxInt(prim.pzMax, prim.p3z);
				prim.pzMin = minInt(prim.pzMin, prim.p3z);
			}
		}

		private void setTextures(VertexState v1, VertexState v2)
		{
			prim.t1u = v1.t[0];
			prim.t1v = v1.t[1];
			prim.t2u = v2.t[0];
			prim.t2v = v2.t[1];

			if (transform2D)
			{
				prim.tuMax = max(round(prim.t1u), round(prim.t2u));
				prim.tuMin = min(round(prim.t1u), round(prim.t2u));
				prim.tvMax = max(round(prim.t1v), round(prim.t2v));
				prim.tvMin = min(round(prim.t1v), round(prim.t2v));
			}
		}

		private void setTextures(VertexState v1, VertexState v2, VertexState v3)
		{
			setTextures(v1, v2);

			prim.t3u = v3.t[0];
			prim.t3v = v3.t[1];

			if (transform2D)
			{
				prim.tuMax = max(prim.tuMax, round(prim.t3u));
				prim.tuMin = min(prim.tuMin, round(prim.t3u));
				prim.tvMax = max(prim.tvMax, round(prim.t3v));
				prim.tvMin = min(prim.tvMin, round(prim.t3v));
			}
		}

		private void prepareWriters()
		{
			fbAddress = getTextureAddress(fbp, prim.pxMin, prim.pyMin, fbw, psm);
			depthAddress = getTextureAddress(zbp, prim.pxMin, prim.pyMin, zbw, depthBufferPixelFormat);
			if (!RendererTemplate.isRendererWriterNative(RuntimeContext.MemoryInt, psm))
			{
				rendererWriter = RendererWriter.getRendererWriter(fbAddress, fbw, psm, depthAddress, zbw, depthBufferPixelFormat, needDestinationDepthRead, needDepthWrite);
			}
			imageWriterSkipEOL = fbw - prim.destinationWidth;
			depthWriterSkipEOL = zbw - prim.destinationWidth;
		}

		protected internal virtual bool Visible
		{
			get
			{
				if (!transform2D)
				{
					// Each vertex screen coordinates (without offset) has to be in the range:
					// - x: [0..4095]
					// - y: [0..4095]
					// - z: [..65535]
					// If one of the vertex coordinate is not in the valid range, the whole
					// primitive is discarded.
					if ((prim.pxMin + screenOffsetX) < 0 || (prim.pxMax + screenOffsetX) >= 4096 || (prim.pyMin + screenOffsetY) < 0 || (prim.pyMax + screenOffsetY) >= 4096 || prim.pzMax >= 65536)
					{
						if (isLogTraceEnabled)
						{
							log.trace(string.Format("Screen coordinates outside valid range {0:D}-{1:D}, {2:D}-{3:D}, {4:D}", prim.pxMin + screenOffsetX, prim.pxMax + screenOffsetX, prim.pyMin + screenOffsetY, prim.pyMax + screenOffsetY, prim.pzMax));
						}
						return false;
					}
    
					// This is probably a rounding error when one triangle
					// extends from back to front over a very large distance
					// (more than the allowed range for Z values).
					if (prim.pzMin < 0 && prim.pzMax > 0 && prim.pzMax - prim.pzMin > 65536)
					{
						if (isLogTraceEnabled)
						{
							log.trace(string.Format("Z range too large: {0:D}, {1:D}", prim.pzMin, prim.pzMax));
						}
						return false;
					}
    
					if (!clipPlanesEnabled)
					{
						// The primitive is discarded when one of the vertex is behind the viewpoint
						// (only the the ClipPlanes flag is not enabled).
						if (prim.pzMin < 0)
						{
							if (isLogTraceEnabled)
							{
								log.trace(string.Format("Z behind clip plane {0:D}", prim.pzMin));
							}
							return false;
						}
					}
					else
					{
						// TODO Implement proper triangle clipping against the near plane
						if (prim.p1w < 0f || prim.p2w < 0f || prim.p3w < 0f)
						{
							if (isLogTraceEnabled)
							{
								log.trace(string.Format("W negative {0:F}, {1:F}, {2:F}", prim.p1w, prim.p2w, prim.p3w));
							}
							return false;
						}
					}
				}
    
				if (!useVertexTexture)
				{
					prim.pxMin = System.Math.Max(prim.pxMin, scissorX1);
					prim.pxMax = System.Math.Min(prim.pxMax, System.Math.Min(scissorX2 + 1, fbw));
					prim.pyMin = System.Math.Max(prim.pyMin, scissorY1);
					prim.pyMax = System.Math.Min(prim.pyMax, scissorY2 + 1);
				}
    
				if (prim.pxMin == prim.pxMax || prim.pyMin == prim.pyMax)
				{
					// Empty area to be displayed
					if (isLogTraceEnabled)
					{
						log.trace("Empty area");
					}
					return false;
				}
    
				if (isTriangle)
				{
					if ((pixel.v1x == pixel.v2x && pixel.v1y == pixel.v2y && pixel.v1z == pixel.v2z) || (pixel.v1x == pixel.v3x && pixel.v1y == pixel.v3y && pixel.v1z == pixel.v3z) || (pixel.v2x == pixel.v3x && pixel.v2y == pixel.v3y && pixel.v2z == pixel.v3z))
					{
						// 2 vertices are equal in the triangle, nothing has to be displayed
						if (isLogTraceEnabled)
						{
							log.trace("2 vertices equal in triangle");
						}
						return false;
					}
				}
    
				if (!insideScissor())
				{
					if (isLogTraceEnabled)
					{
						log.trace("Not inside scissor");
					}
					return false;
				}
    
				return true;
			}
		}

		protected internal virtual bool insideScissor()
		{
			needScissoringX = false;
			needScissoringY = false;

			// Scissoring (also applied in clear mode)
			if (prim.pxMax < scissorX1 || prim.pxMin > scissorX2)
			{
				// Completely outside the scissor area, skip
				if (isLogTraceEnabled)
				{
					log.trace(string.Format("X outside scissor area {0:D}-{1:D}, {2:D}-{3:D}", prim.pxMin, prim.pxMax, scissorX1, scissorX2));
				}
				return false;
			}
			if (prim.pyMax < scissorY1 || prim.pyMin > scissorY2)
			{
				// Completely outside the scissor area, skip
				if (isLogTraceEnabled)
				{
					log.trace(string.Format("Y outside scissor area {0:D}-{1:D}, {2:D}-{3:D}", prim.pyMin, prim.pyMax, scissorY1, scissorY2));
				}
				return false;
			}
			if (!transform2D)
			{
				if ((nearZ > 0x0000 && prim.pzMax < nearZ) || (farZ < 0xFFFF && prim.pzMin > farZ))
				{
					// Completely outside the view area, skip
					if (isLogTraceEnabled)
					{
						log.trace(string.Format("Z outside view area {0:D}-{1:D}, {2:D}-{3:D}", prim.pzMin, prim.pzMax, nearZ, farZ));
					}
					return false;
				}
			}

			if (prim.pxMin < scissorX1 || prim.pxMax > scissorX2)
			{
				// partially outside the scissor area, use the scissoring filter
				needScissoringX = true;
			}
			if (prim.pyMin < scissorY1 || prim.pyMax > scissorY2)
			{
				// partially outside the scissor area, use the scissoring filter
				needScissoringY = true;
			}

			return true;
		}

		private void getScreenCoordinates(float[] screenCoordinates, float[] position)
		{
			getScreenCoordinates(screenCoordinates, position[0], position[1], position[2]);
		}

		private void getScreenCoordinates(float[] screenCoordinates, float x, float y, float z)
		{
			float[] position4 = new float[4];
			position4[0] = x;
			position4[1] = y;
			position4[2] = z;
			position4[3] = 1.0f;
			float[] projectedCoordinates = new float[4];
			vectorMult44(projectedCoordinates, pixel.modelViewProjectionMatrix, position4);
			float w = projectedCoordinates[3];
			float wInverted = 1.0f / w;
			screenCoordinates[0] = projectedCoordinates[0] * wInverted * viewportWidth + viewportX - screenOffsetX;
			screenCoordinates[1] = projectedCoordinates[1] * wInverted * viewportHeight + viewportY - screenOffsetY;
			screenCoordinates[2] = projectedCoordinates[2] * wInverted * zscale + zpos;
			screenCoordinates[3] = w;

			if (isLogTraceEnabled)
			{
				log.trace(string.Format("X,Y,Z = {0:F}, {1:F}, {2:F}, projected X,Y,Z,W = {3:F}, {4:F}, {5:F}, {6:F} -> Screen {7:F3}, {8:F3}, {9:F3}", x, y, z, projectedCoordinates[0] / w, projectedCoordinates[1] / w, projectedCoordinates[2] / w, w, screenCoordinates[0], screenCoordinates[1], screenCoordinates[2]));
			}
		}

		public override void postRender()
		{
			if (DurationStatistics.collectStatistics && isLogInfoEnabled)
			{
				pixelStatistics.end();
				const int pixelsGrouping = 1000;
				int n = pixel.NumberPixels / pixelsGrouping;
				if (!pixelsStatistics.ContainsKey(n))
				{
					pixelsStatistics[n] = new DurationStatistics(string.Format("Pixels count={0:D}", n * pixelsGrouping));
				}
				if (isLogTraceEnabled)
				{
					log.trace(string.Format("Pixels statistics count={0:D}, real count={1:D}", n * pixelsGrouping, pixel.NumberPixels));
				}
				pixelsStatistics[n].add(pixelStatistics);
			}

			if (rendererWriter != null)
			{
				rendererWriter.flush();
			}

			base.postRender();

			statisticsFilters(pixel.NumberPixels);
		}

		public override void preRender()
		{
			pixel.reset();

			base.preRender();

			if (DurationStatistics.collectStatistics && isLogInfoEnabled)
			{
				pixelStatistics.reset();
				pixelStatistics.start();
			}
		}

		protected internal virtual void writerSkip(int count)
		{
			rendererWriter.skip(count, count);
		}

		protected internal virtual void writerSkipEOL(int count)
		{
			rendererWriter.skip(count + imageWriterSkipEOL, count + depthWriterSkipEOL);
		}

		protected internal virtual void writerSkipEOL()
		{
			rendererWriter.skip(imageWriterSkipEOL, depthWriterSkipEOL);
		}

		public override void render()
		{
			compiledRenderer.render(this);
		}

		public static void exit()
		{
			if (!log.InfoEnabled || pixelsStatistics.Count == 0)
			{
				return;
			}

			DurationStatistics[] sortedPixelsStatistics = pixelsStatistics.Values.toArray(new DurationStatistics[pixelsStatistics.Count]);
			Array.Sort(sortedPixelsStatistics);
			foreach (DurationStatistics durationStatistics in sortedPixelsStatistics)
			{
				log.info(durationStatistics);
			}
		}

		protected internal virtual LongLongKey RendererKey
		{
			get
			{
				LongLongKey key = new LongLongKey(baseRendererKey);
    
				key.addKeyComponent(needSourceDepthRead);
				key.addKeyComponent(needDestinationDepthRead);
				key.addKeyComponent(needDepthWrite);
				key.addKeyComponent(needTextureUV);
				key.addKeyComponent(simpleTextureUV);
				key.addKeyComponent(swapTextureUV);
				key.addKeyComponent(needScissoringX);
				key.addKeyComponent(needScissoringY);
				key.addKeyComponent(needTextureWrapU);
				key.addKeyComponent(needTextureWrapV);
				key.addKeyComponent(sameVertexColor);
				key.addKeyComponent(needSourceDepthClamp);
    
				return key;
			}
		}
	}

}