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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.abs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TFLT_NEAREST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TMAP_TEXTURE_MAP_MODE_TEXTURE_COORDIATES_UV;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TMAP_TEXTURE_PROJECTION_MODE_TEXTURE_COORDINATES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TWRAP_WRAP_MODE_CLAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.ONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.getBlue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.getColorBGR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.getGreen;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.getRed;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.matrixMult;


	using EnableDisableFlag = pspsharp.graphics.GeContext.EnableDisableFlag;
	using Settings = pspsharp.settings.Settings;

	/// <summary>
	/// @author gid15
	/// 
	/// Base class the RenderingEngine providing basic functionalities:
	/// - generic clear mode handling: implementation matching the PSP behavior,
	///   but probably not the most efficient implementation.
	/// - merge of View and Model matrix for OpenGL supporting
	///   only combined model-view matrix
	/// - direct rendering mode
	/// - implementation of the bounding box processing
	///   (using OpenGL Query with a partial software implementation)
	/// - mapping of setColorMask(int, int, int, int) to setColorMask(bool, bool, bool, bool)
	/// - bug fix for glMultiDrawArrays
	/// 
	/// The partial software implementation for Bounding Boxes tries to find out
	/// if a bounding box is visible or not without using an OpenGL Query.
	/// The OpenGL queries have quite a large overhead to be setup and the software
	/// implementation solves the following bounding box cases:
	/// - if at least one bounding box vertex is visible,
	///   the complete bounding box is visible
	/// - if all the bounding box vertices are not visible and are all placed on the
	///   same side of a frustum plane, then the complete bounding box is not visible.
	///   E.g.: all the vertices are hidden on the left side of the frustum.
	/// 
	///   If some vertices are hidden on different sides of the frustum (e.g. one on
	///   the left side and one on the right side), the implementation cannot determine
	///   if some pixels in between are visible or not. A complete intersection test is
	///   necessary in that case. Remark: this could be implemented in software too.
	/// 
	/// In all other cases, the OpenGL query has to be used to determine if the bounding
	/// box is visible or not.
	/// </summary>
	public class BaseRenderingEngineFunction : BaseRenderingEngineProxy
	{

		protected internal const bool useWorkaroundForMultiDrawArrays = true;
		protected internal bool useVertexArray;
		internal ClearModeContext clearModeContext = new ClearModeContext();
		protected internal bool directMode;
		protected internal bool directModeSetOrthoMatrix;
		protected internal static readonly bool[] flagsValidInClearMode = new bool[]{false, false, true, false, false, false, true, false, true, false, true, true, true, true, true, true, true, false, false, true, true, true, true, true, true};

		protected internal class ClearModeContext
		{

			public bool color;
			public bool stencil;
			public bool depth;
			public int alphaFunc;
			public int depthFunc;
			public int textureFunc;
			public bool textureAlphaUsed;
			public bool textureColorDoubled;
			public int stencilFunc;
			public int stencilRef;
			public int stencilMask;
			public int stencilOpFail;
			public int stencilOpZFail;
			public int stencilOpZPass;
		}
		protected internal bool viewMatrixLoaded = false;
		protected internal bool modelMatrixLoaded = false;
		protected internal bool bboxVisible;
		protected internal int activeTextureUnit = 0;
		private bool[] colorMask = new bool[4];
		private static ByteBuffer emptyBuffer;
		protected internal int logicOp = -1;

		public BaseRenderingEngineFunction(IRenderingEngine proxy) : base(proxy)
		{
			init();
		}

		protected internal virtual void init()
		{
			useVertexArray = Settings.Instance.readBool("emu.enablevao") && base.VertexArrayAvailable;
			if (useVertexArray)
			{
				log.info("Using VAO (Vertex Array Object)");
			}
		}

		/// <summary>
		/// Allocate an empty direct buffer of at least the given capacity.
		/// </summary>
		/// <param name="capacity">  the minimum required buffer capacity </param>
		/// <returns>          an empty direct buffer of the given capacity </returns>
		private static ByteBuffer getEmptyBuffer(int capacity)
		{
			if (emptyBuffer == null || emptyBuffer.capacity() < capacity)
			{
				emptyBuffer = ByteBuffer.allocateDirect(capacity);
			}

			emptyBuffer.clear();

			return emptyBuffer;
		}

		public override IRenderingEngine RenderingEngine
		{
			set
			{
				BufferManager.RenderingEngine = value;
				base.RenderingEngine = value;
			}
		}

		protected internal virtual EnableDisableFlag Flag
		{
			set
			{
				if (value.Enabled)
				{
					re.enableFlag(value.ReFlag);
				}
				else
				{
					re.disableFlag(value.ReFlag);
				}
			}
		}

		protected internal virtual void setClearModeSettings(bool color, bool stencil, bool depth)
		{
			// Disable all the flags invalid in clear mode
			for (int i = 0; i < flagsValidInClearMode.Length; i++)
			{
				if (!flagsValidInClearMode[i])
				{
					re.disableFlag(i);
				}
			}

			if (stencil)
			{
				re.enableFlag(IRenderingEngine_Fields.GU_STENCIL_TEST);
				re.setStencilFunc(GeCommands.STST_FUNCTION_ALWAYS_PASS_STENCIL_TEST, 0, 0);
				re.setStencilOp(GeCommands.SOP_KEEP_STENCIL_VALUE, GeCommands.SOP_KEEP_STENCIL_VALUE, GeCommands.SOP_ZERO_STENCIL_VALUE);
			}

			if (depth)
			{
				re.enableFlag(IRenderingEngine_Fields.GU_DEPTH_TEST);
				context.depthFunc = GeCommands.ZTST_FUNCTION_ALWAYS_PASS_PIXEL;
				re.DepthFunc = context.depthFunc;
			}

			// Update color, stencil and depth masks.
			re.DepthMask = depth;
			re.setColorMask(color, color, color, stencil);
			re.setTextureFunc(GeCommands.TFUNC_FRAGMENT_DOUBLE_TEXTURE_EFECT_REPLACE, true, false);
			re.setBones(0, null);
		}

		public override void startClearMode(bool color, bool stencil, bool depth)
		{
			// Clear mode flags.
			clearModeContext.color = color;
			clearModeContext.stencil = stencil;
			clearModeContext.depth = depth;

			// Save depth.
			clearModeContext.depthFunc = context.depthFunc;

			// Save texture.     
			clearModeContext.textureFunc = context.textureFunc;
			clearModeContext.textureAlphaUsed = context.textureAlphaUsed;
			clearModeContext.textureColorDoubled = context.textureColorDoubled;

			// Save stencil.
			clearModeContext.stencilFunc = context.stencilFunc;
			clearModeContext.stencilRef = context.stencilRef;
			clearModeContext.stencilMask = context.stencilMask;
			clearModeContext.stencilOpFail = context.stencilOpFail;
			clearModeContext.stencilOpZFail = context.stencilOpZFail;
			clearModeContext.stencilOpZPass = context.stencilOpZPass;

			setClearModeSettings(color, stencil, depth);
			base.startClearMode(color, stencil, depth);
		}

		public override void endClearMode()
		{
			// Reset all the flags disabled in CLEAR mode
			foreach (EnableDisableFlag flag in context.flags)
			{
				if (!flagsValidInClearMode[flag.ReFlag])
				{
					Flag = flag;
				}
			}

			// Restore depth.
			context.depthFunc = clearModeContext.depthFunc;
			re.DepthFunc = context.depthFunc;

			// Restore texture.
			context.textureFunc = clearModeContext.textureFunc;
			context.textureAlphaUsed = clearModeContext.textureAlphaUsed;
			context.textureColorDoubled = clearModeContext.textureColorDoubled;
			re.setTextureFunc(context.textureFunc, context.textureAlphaUsed, context.textureColorDoubled);

			// Restore stencil.
			context.stencilFunc = clearModeContext.stencilFunc;
			context.stencilRef = clearModeContext.stencilRef;
			context.stencilMask = clearModeContext.stencilMask;
			re.setStencilFunc(context.stencilFunc, context.stencilRef, context.stencilRef);

			context.stencilOpFail = clearModeContext.stencilOpFail;
			context.stencilOpZFail = clearModeContext.stencilOpZFail;
			context.stencilOpZPass = clearModeContext.stencilOpZPass;
			re.setStencilOp(context.stencilOpFail, context.stencilOpZFail, context.stencilOpZPass);

			re.DepthMask = context.depthMask;
			re.setColorMask(true, true, true, context.stencilTestFlag.Enabled);
			re.setColorMask(context.colorMask[0], context.colorMask[1], context.colorMask[2], context.colorMask[3]);

			base.endClearMode();
		}

		protected internal virtual bool ClearMode
		{
			get
			{
				return context.clearMode;
			}
		}

		protected internal virtual bool canUpdateFlag(int flag)
		{
			return !context.clearMode || directMode || flagsValidInClearMode[flag];
		}

		protected internal virtual bool canUpdate()
		{
			return !context.clearMode || directMode;
		}

		protected internal static bool getBooleanColorMask(string name, int bitMask)
		{
			if (bitMask == 0xFF)
			{
				return false;
			}
			else if (bitMask != 0x00)
			{
				Console.WriteLine(string.Format("Unimplemented {0} 0x{1:X2}", name, bitMask));
			}

			return true;
		}

		public override void setColorMask(bool redWriteEnabled, bool greenWriteEnabled, bool blueWriteEnabled, bool alphaWriteEnabled)
		{
			colorMask[0] = redWriteEnabled;
			colorMask[1] = greenWriteEnabled;
			colorMask[2] = blueWriteEnabled;
			colorMask[3] = alphaWriteEnabled;
			base.setColorMask(redWriteEnabled, greenWriteEnabled, blueWriteEnabled, alphaWriteEnabled);
		}

		public override void setColorMask(int redMask, int greenMask, int blueMask, int alphaMask)
		{
			bool redWriteEnabled = getBooleanColorMask("Red color mask", redMask);
			bool greenWriteEnabled = getBooleanColorMask("Green color mask", greenMask);
			bool blueWriteEnabled = getBooleanColorMask("Blue color mask", blueMask);
			// boolean alphaWriteEnabled = getBooleanColorMask("Alpha mask", alphaMask);
			re.setColorMask(redWriteEnabled, greenWriteEnabled, blueWriteEnabled, colorMask[3]);
			base.setColorMask(redMask, greenMask, blueMask, alphaMask);
		}

		public override bool DepthMask
		{
			set
			{
				if (canUpdate())
				{
					base.DepthMask = value;
				}
			}
		}

		public override float[] ViewMatrix
		{
			set
			{
				base.ViewMatrix = value;
				// Reload the Model matrix if it was loaded before the View matrix (wrong order)
				if (modelMatrixLoaded)
				{
					re.ModelMatrix = context.model_uploaded_matrix;
				}
				viewMatrixLoaded = true;
			}
		}

		public override float[] ModelMatrix
		{
			set
			{
				if (!viewMatrixLoaded)
				{
					re.ViewMatrix = context.view_uploaded_matrix;
				}
				base.ModelMatrix = value;
				modelMatrixLoaded = true;
			}
		}

		public override void endModelViewMatrixUpdate()
		{
			if (!viewMatrixLoaded)
			{
				re.ViewMatrix = context.view_uploaded_matrix;
			}
			if (!modelMatrixLoaded)
			{
				re.ModelMatrix = context.model_uploaded_matrix;
			}
			base.endModelViewMatrixUpdate();
			viewMatrixLoaded = false;
			modelMatrixLoaded = false;
		}

		public override void startDirectRendering(bool textureEnabled, bool depthWriteEnabled, bool colorWriteEnabled, bool setOrthoMatrix, bool orthoInverted, int width, int height)
		{
			directMode = true;

			re.disableFlag(IRenderingEngine_Fields.GU_DEPTH_TEST);
			re.disableFlag(IRenderingEngine_Fields.GU_BLEND);
			re.disableFlag(IRenderingEngine_Fields.GU_ALPHA_TEST);
			re.disableFlag(IRenderingEngine_Fields.GU_FOG);
			re.disableFlag(IRenderingEngine_Fields.GU_LIGHTING);
			re.disableFlag(IRenderingEngine_Fields.GU_COLOR_LOGIC_OP);
			re.disableFlag(IRenderingEngine_Fields.GU_STENCIL_TEST);
			re.disableFlag(IRenderingEngine_Fields.GU_CULL_FACE);
			re.disableFlag(IRenderingEngine_Fields.GU_SCISSOR_TEST);
			if (textureEnabled)
			{
				re.enableFlag(IRenderingEngine_Fields.GU_TEXTURE_2D);
			}
			else
			{
				re.disableFlag(IRenderingEngine_Fields.GU_TEXTURE_2D);
			}
			re.TextureMipmapMinFilter = TFLT_NEAREST;
			re.TextureMipmapMagFilter = TFLT_NEAREST;
			re.TextureMipmapMinLevel = 0;
			re.TextureMipmapMaxLevel = 0;
			re.setTextureWrapMode(TWRAP_WRAP_MODE_CLAMP, TWRAP_WRAP_MODE_CLAMP);
			int colorMask = colorWriteEnabled ? 0x00 : 0xFF;
			re.setColorMask(colorMask, colorMask, colorMask, colorMask);
			re.setColorMask(colorWriteEnabled, colorWriteEnabled, colorWriteEnabled, colorWriteEnabled);
			re.DepthMask = depthWriteEnabled;
			re.setTextureFunc(IRenderingEngine_Fields.RE_TEXENV_REPLACE, true, false);
			re.setTextureMapMode(TMAP_TEXTURE_MAP_MODE_TEXTURE_COORDIATES_UV, TMAP_TEXTURE_PROJECTION_MODE_TEXTURE_COORDINATES);
			re.FrontFace = true;
			re.setBones(0, null);

			directModeSetOrthoMatrix = setOrthoMatrix;
			if (setOrthoMatrix)
			{
				float[] orthoMatrix;
				if (orthoInverted)
				{
					orthoMatrix = VideoEngine.getOrthoMatrix(0, width, 0, height, -1, 1);
				}
				else
				{
					orthoMatrix = VideoEngine.getOrthoMatrix(0, width, height, 0, -1, 1);
				}
				re.ProjectionMatrix = orthoMatrix;
				re.ModelViewMatrix = null;
				re.TextureMatrix = null;
			}

			base.startDirectRendering(textureEnabled, depthWriteEnabled, colorWriteEnabled, setOrthoMatrix, orthoInverted, width, height);
		}

		public override void endDirectRendering()
		{
			// Restore all the values according to the context or the clearMode
			re.setColorMask(context.colorMask[0], context.colorMask[1], context.colorMask[2], context.colorMask[3]);
			if (context.clearMode)
			{
				setClearModeSettings(clearModeContext.color, clearModeContext.stencil, clearModeContext.depth);
			}
			else
			{
				context.depthTestFlag.updateEnabled();
				context.blendFlag.updateEnabled();
				context.alphaTestFlag.updateEnabled();
				context.fogFlag.updateEnabled();
				context.colorLogicOpFlag.updateEnabled();
				context.stencilTestFlag.updateEnabled();
				context.cullFaceFlag.updateEnabled();
				context.textureFlag.update();
				re.DepthMask = context.depthMask;
				re.setTextureFunc(context.textureFunc, context.textureAlphaUsed, context.textureColorDoubled);
			}
			re.setTextureMapMode(context.tex_map_mode, context.tex_proj_map_mode);
			context.scissorTestFlag.updateEnabled();
			context.lightingFlag.updateEnabled();
			re.TextureMipmapMagFilter = context.tex_mag_filter;
			re.TextureMipmapMinFilter = context.tex_min_filter;
			re.setTextureWrapMode(context.tex_wrap_s, context.tex_wrap_t);
			re.FrontFace = context.frontFaceCw;

			if (directModeSetOrthoMatrix)
			{
				VideoEngine videoEngine = VideoEngine.Instance;
				videoEngine.projectionMatrixUpload.Changed = true;
				videoEngine.viewMatrixUpload.Changed = true;
				videoEngine.modelMatrixUpload.Changed = true;
				videoEngine.textureMatrixUpload.Changed = true;
			}

			base.endDirectRendering();

			directMode = false;
		}

		public override void beginBoundingBox(int numberOfVertexBoundingBox)
		{
			bboxVisible = true;

			base.beginBoundingBox(numberOfVertexBoundingBox);
		}

		public override void drawBoundingBox(float[][] values)
		{
			if (bboxVisible)
			{
				// Pre-compute the Model-View-Projection matrix
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float[] modelViewMatrix = new float[16];
				float[] modelViewMatrix = new float[16];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float[] modelViewProjectionMatrix = new float[16];
				float[] modelViewProjectionMatrix = new float[16];
				matrixMult(modelViewMatrix, context.view_uploaded_matrix, context.model_uploaded_matrix);
				matrixMult(modelViewProjectionMatrix, context.proj_uploaded_matrix, modelViewMatrix);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float viewportX = context.viewport_cx - context.offset_x;
				float viewportX = context.viewport_cx - context.offset_x;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float viewportY = context.viewport_cy - context.offset_y;
				float viewportY = context.viewport_cy - context.offset_y;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float viewportWidth = context.viewport_width;
				float viewportWidth = context.viewport_width;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float viewportHeight = context.viewport_height;
				float viewportHeight = context.viewport_height;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float[] mvpVertex = new float[4];
				float[] mvpVertex = new float[4];
				float minX = 0f;
				float minY = 0f;
				float maxX = 0f;
				float maxY = 0f;
				float minW = 0f;
				float maxW = 0f;

				for (int i = 0; i < values.Length; i++)
				{
					multMatrix44(mvpVertex, modelViewProjectionMatrix, values[i]);

					float w = mvpVertex[3];
					float x = minX;
					float y = minY;
					if (w != 0.0f)
					{
						x = mvpVertex[0] / w * viewportWidth + viewportX;
						y = mvpVertex[1] / w * viewportHeight + viewportY;
					}
					if (i == 0)
					{
						minX = maxX = x;
						minY = maxY = y;
						minW = maxW = w;
					}
					else
					{
						minX = System.Math.Min(minX, x);
						maxX = System.Math.Max(maxX, x);
						minY = System.Math.Min(minY, y);
						maxY = System.Math.Max(maxY, y);
						minW = System.Math.Min(minW, w);
						maxW = System.Math.Max(maxW, w);
					}

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("drawBoundingBox vertex#{0:D} x={1:F}, y={2:F}, w={3:F}", i, x, y, w));
					}
				}

				// The Bounding Box is not visible when all vertices are outside the drawing region.
				if (maxX < context.region_x1 || maxY < context.region_y1 || minX > context.region_x2 || minY > context.region_y2)
				{
					// When the bounding box is partially before and behind the viewpoint,
					// assume the bounding box is visible. Rejecting the bounding box in
					// such cases is leading to incorrect results.
					if (minW >= 0f || maxW <= 0f)
					{
						bboxVisible = false;
					}
				}
			}

			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("drawBoundingBox visible=%b", bboxVisible));
				Console.WriteLine(string.Format("drawBoundingBox visible=%b", bboxVisible));
			}

			base.drawBoundingBox(values);
		}

		public override bool BoundingBoxVisible
		{
			get
			{
				if (!bboxVisible)
				{
					return false;
				}
				return base.BoundingBoxVisible;
			}
		}

		public override void setTextureFunc(int func, bool alphaUsed, bool colorDoubled)
		{
			re.setTexEnv(IRenderingEngine_Fields.RE_TEXENV_RGB_SCALE, colorDoubled ? 2.0f : 1.0f);
			re.setTexEnv(IRenderingEngine_Fields.RE_TEXENV_ENV_MODE, func);
			base.setTextureFunc(func, alphaUsed, colorDoubled);
		}

		protected internal static void multMatrix44(float[] result4, float[] matrix44, float[] vector4)
		{
			float x = vector4[0];
			float y = vector4[1];
			float z = vector4[2];
			float w = vector4.Length < 4 ? 1.0f : vector4[3];
			result4[0] = matrix44[0] * x + matrix44[4] * y + matrix44[8] * z + matrix44[12] * w;
			result4[1] = matrix44[1] * x + matrix44[5] * y + matrix44[9] * z + matrix44[13] * w;
			result4[2] = matrix44[2] * x + matrix44[6] * y + matrix44[10] * z + matrix44[14] * w;
			result4[3] = matrix44[3] * x + matrix44[7] * y + matrix44[11] * z + matrix44[15] * w;
		}

		public override void startDisplay()
		{
			for (int light = 0; light < context.lightFlags.Length; light++)
			{
				context.lightFlags[light].update();
			}
			base.startDisplay();
		}

		public override bool VertexArrayAvailable
		{
			get
			{
				return useVertexArray;
			}
		}

		public override void multiDrawArrays(int primitive, IntBuffer first, IntBuffer count)
		{
			// (gid15) I don't know why, but glMultiDrawArrays doesn't seem to work
			// as expected... is it a bug in LWJGL or did I misunderstood the effect
			// of the function?
			// Workaround using glDrawArrays provided.
			if (useWorkaroundForMultiDrawArrays)
			{
				int primitiveCount = first.remaining();
				int positionFirst = first.position();
				int positionCount = count.position();
				if (primitive == GeCommands.PRIM_POINT || primitive == GeCommands.PRIM_LINE || primitive == GeCommands.PRIM_TRIANGLE || primitive == IRenderingEngine_Fields.RE_QUADS)
				{
					// Independent elements can be rendered in one drawArrays call
					// if all the elements are sequentially defined
					bool sequential = true;
					int firstIndex = first.get(positionFirst);
					int currentIndex = firstIndex;
					for (int i = 1; i < primitiveCount; i++)
					{
						currentIndex += count.get(positionCount + i - 1);
						if (currentIndex != first.get(positionFirst + i))
						{
							sequential = false;
							break;
						}
					}

					if (sequential)
					{
						re.drawArrays(primitive, firstIndex, currentIndex - firstIndex + count.get(positionCount + primitiveCount - 1));
						return;
					}
				}

				// Implement multiDrawArrays using multiple drawArrays.
				// The first call is using drawArrays and the subsequent calls,
				// drawArraysBurstMode (allowing a faster implementation).
				re.drawArrays(primitive, first.get(positionFirst), count.get(positionCount));
				for (int i = 1; i < primitiveCount; i++)
				{
					re.drawArraysBurstMode(primitive, first.get(positionFirst + i), count.get(positionCount + i));
				}
			}
			else
			{
				base.multiDrawArrays(primitive, first, count);
			}
		}

		public override void bindActiveTexture(int index, int texture)
		{
			int previousActiveTextureUnit = activeTextureUnit;
			re.ActiveTexture = index;
			re.bindTexture(texture);
			re.ActiveTexture = previousActiveTextureUnit;
		}

		public override int ActiveTexture
		{
			set
			{
				activeTextureUnit = value;
				base.ActiveTexture = value;
			}
		}

		protected internal virtual bool AlphaMask
		{
			set
			{
				if (colorMask[3] != value)
				{
					colorMask[3] = value;
					re.setColorMask(colorMask[0], colorMask[1], colorMask[2], colorMask[3]);
				}
			}
		}

		public override void disableFlag(int flag)
		{
			if (flag == IRenderingEngine_Fields.GU_STENCIL_TEST)
			{
				AlphaMask = false;
			}
			base.disableFlag(flag);
		}

		public override void enableFlag(int flag)
		{
			if (flag == IRenderingEngine_Fields.GU_STENCIL_TEST)
			{
				AlphaMask = true;
			}

			// Setting the logical operation to LOP_COPY is equivalent
			// to disabling the logical operation step.
			if (flag == IRenderingEngine_Fields.GU_COLOR_LOGIC_OP && logicOp == GeCommands.LOP_COPY)
			{
				disableFlag(flag);
			}
			else
			{
				base.enableFlag(flag);
			}
		}

		private int getBlendFix(int fixColor)
		{
			if (fixColor == 0x000000)
			{
				return IRenderingEngine_Fields.GU_FIX_BLACK;
			}
			else if (fixColor == 0xFFFFFF)
			{
				return IRenderingEngine_Fields.GU_FIX_WHITE;
			}
			else
			{
				return IRenderingEngine_Fields.GU_FIX_BLEND_COLOR;
			}
		}

		/// <summary>
		/// Return the distance between 2 colors.
		/// The distance is the sum of the color component differences.
		/// </summary>
		/// <param name="color1"> </param>
		/// <param name="color2">
		/// @return </param>
		private int colorDistance(int color1, int color2)
		{
			int blueDistance = abs(getBlue(color1) - getBlue(color2));
			int greenDistance = abs(getGreen(color1) - getGreen(color2));
			int redDistance = abs(getRed(color1) - getRed(color2));

			return redDistance + greenDistance + blueDistance;
		}

		private int oneMinusColor(int color)
		{
			int b = ONE - getBlue(color);
			int g = ONE - getGreen(color);
			int r = ONE - getRed(color);
			return getColorBGR(b, g, r);
		}

		/// <summary>
		/// Return the best distance that could be used with a blend factor
		/// for a given color and blend color.
		/// The possible blend factors are
		/// - BLACK
		/// - WHITE
		/// - the blend color
		/// - one minus the blend color
		/// </summary>
		/// <param name="blendColor"> </param>
		/// <param name="color">
		/// @return </param>
		private int getBestColorDistance(int blendColor, int color)
		{
			int bestDistance = colorDistance(color, blendColor);
			bestDistance = System.Math.Min(bestDistance, colorDistance(color, oneMinusColor(blendColor)));
			bestDistance = System.Math.Min(bestDistance, colorDistance(color, 0x000000));
			bestDistance = System.Math.Min(bestDistance, colorDistance(color, 0xFFFFFF));

			return bestDistance;
		}

		/// <summary>
		/// Find the best blend factor for a color given a blend color.
		/// </summary>
		/// <param name="blendColor"> </param>
		/// <param name="oneMinusBlendColor"> </param>
		/// <param name="color">
		/// @return </param>
		private int getBestBlend(int blendColor, int oneMinusBlendColor, int color)
		{
			// Simple cases...
			if (blendColor == 0x000000)
			{
				return IRenderingEngine_Fields.GU_FIX_BLACK;
			}
			if (blendColor == 0xFFFFFF)
			{
				return IRenderingEngine_Fields.GU_FIX_WHITE;
			}
			if (blendColor == color)
			{
				return IRenderingEngine_Fields.GU_FIX_BLEND_COLOR;
			}
			if (blendColor == oneMinusBlendColor)
			{
				return IRenderingEngine_Fields.GU_FIX_BLEND_ONE_MINUS_COLOR;
			}

			// Complex case: test which blend would be the closest to the given color
			int bestDistance = colorDistance(color, blendColor);
			int bestBlend = IRenderingEngine_Fields.GU_FIX_BLEND_COLOR;

			int distance = colorDistance(color, oneMinusBlendColor);
			if (distance < bestDistance)
			{
				bestDistance = distance;
				bestBlend = IRenderingEngine_Fields.GU_FIX_BLEND_ONE_MINUS_COLOR;
			}

			distance = colorDistance(color, 0x000000);
			if (distance < bestDistance)
			{
				bestDistance = distance;
				bestBlend = IRenderingEngine_Fields.GU_FIX_BLACK;
			}

			distance = colorDistance(color, 0xFFFFFF);
			if (distance < bestDistance)
			{
				bestDistance = distance;
				bestBlend = IRenderingEngine_Fields.GU_FIX_WHITE;
			}

			return bestBlend;
		}

		private int getColorFromBlend(int blend, int blendColor, int oneMinusBlendColor)
		{
			switch (blend)
			{
				case IRenderingEngine_Fields.GU_FIX_BLACK:
					return 0x000000;
				case IRenderingEngine_Fields.GU_FIX_WHITE:
					return 0xFFFFFF;
				case IRenderingEngine_Fields.GU_FIX_BLEND_COLOR:
					return blendColor;
				case IRenderingEngine_Fields.GU_FIX_BLEND_ONE_MINUS_COLOR:
					return oneMinusBlendColor;
			}

			// Unknown blend...
			return -1;
		}

		public override void setBlendFunc(int src, int dst)
		{
			if (src == 10)
			{ // GU_FIX
				src = getBlendFix(context.sfix);
			}

			if (dst == 10)
			{ // GU_FIX
				if (src == IRenderingEngine_Fields.GU_FIX_BLEND_COLOR && (context.sfix + context.dfix == 0xFFFFFF))
				{
					dst = IRenderingEngine_Fields.GU_FIX_BLEND_ONE_MINUS_COLOR;
				}
				else
				{
					dst = getBlendFix(context.dfix);
				}
			}

			float[] blend_color = null;
			if (src == IRenderingEngine_Fields.GU_FIX_BLEND_COLOR)
			{
				blend_color = context.sfix_color;
				if (dst == IRenderingEngine_Fields.GU_FIX_BLEND_COLOR)
				{
					if (context.sfix != context.dfix)
					{
						// We cannot set the correct FIX blend colors.
						// Try to find the best approximation...
						int blendColor;
						// Check which blend color, sfix or dfix, would give the best results
						// (i.e. would have the smallest distance)
						if (getBestColorDistance(context.sfix, context.dfix) <= getBestColorDistance(context.dfix, context.sfix))
						{
							// Taking sfix as the blend color leads to the best results
							blendColor = context.sfix;
							blend_color = context.sfix_color;
						}
						else
						{
							// Taking dfix as the blend color leads to the best results
							blendColor = context.dfix;
							blend_color = context.dfix_color;
						}
						int oneMinusBlendColor = oneMinusColor(blendColor);

						// Now that we have decided which blend color to take,
						// find the optimum blend factor for both the source and destination
						src = getBestBlend(blendColor, oneMinusBlendColor, context.sfix);
						dst = getBestBlend(blendColor, oneMinusBlendColor, context.dfix);

						if (log.InfoEnabled)
						{
							Console.WriteLine(string.Format("UNSUPPORTED: Both different SFIX (0x{0:X6}) and DFIX (0x{1:X6}) are not supported (blend equation={2:D}), approximating with 0x{3:X6}/0x{4:X6}", context.sfix, context.dfix, context.blendEquation, getColorFromBlend(src, blendColor, oneMinusBlendColor), getColorFromBlend(dst, blendColor, oneMinusBlendColor)));
						}
					}
				}
			}
			else if (dst == IRenderingEngine_Fields.GU_FIX_BLEND_COLOR)
			{
				blend_color = context.dfix_color;
			}

			if (blend_color != null)
			{
				re.BlendColor = blend_color;
			}

			base.setBlendFunc(src, dst);
		}

		public override void setBlendDFix(int sfix, float[] color)
		{
			// Update the blend color and functions when the DFIX is changing
			setBlendFunc(context.blend_src, context.blend_dst);

			base.setBlendDFix(sfix, color);
		}

		public override void setBlendSFix(int dfix, float[] color)
		{
			// Update the blend color and functions when the SFIX is changing
			setBlendFunc(context.blend_src, context.blend_dst);

			base.setBlendSFix(dfix, color);
		}

		public override void multiDrawElements(int primitive, IntBuffer first, IntBuffer count, int indexType, long indicesOffset)
		{
			int primitiveCount = first.remaining();
			int positionFirst = first.position();
			int positionCount = count.position();
			if (primitive == GeCommands.PRIM_POINT || primitive == GeCommands.PRIM_LINE || primitive == GeCommands.PRIM_TRIANGLE || primitive == IRenderingEngine_Fields.RE_QUADS)
			{
				// Independent elements can be rendered in one drawElements call
				// if all the elements are sequentially defined and the first
				// index is 0
				bool sequential = true;
				int firstIndex = first.get(positionFirst);
				int currentIndex = firstIndex;
				if (firstIndex != 0)
				{
					sequential = false;
				}
				else
				{
					for (int i = 1; i < primitiveCount; i++)
					{
						currentIndex += count.get(positionCount + i - 1);
						if (currentIndex != first.get(positionFirst + i))
						{
							sequential = false;
							break;
						}
					}
				}

				if (sequential)
				{
					re.drawElements(primitive, currentIndex - firstIndex + count.get(positionCount + primitiveCount - 1), indexType, indicesOffset);
					return;
				}
			}

			// Implement multiDrawElements using multiple drawElements.
			// The first call is using drawElements and the subsequent calls,
			// drawElementsBurstMode (allowing a faster implementation).
			int bytesPerIndex = IRenderingEngine_Fields.sizeOfType[indexType];
			re.drawElements(primitive, count.get(positionCount), indexType, indicesOffset + first.get(positionFirst) * bytesPerIndex);
			for (int i = 1; i < primitiveCount; i++)
			{
				re.drawElementsBurstMode(primitive, count.get(positionCount + i), indexType, indicesOffset + first.get(positionFirst + i) * bytesPerIndex);
			}
		}

		public override void setTexImage(int level, int internalFormat, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
			// When specifying no texture buffer, garbage data is used by some OpenGL drivers for
			// the initial texture content. This can result in a short display of garbage data on screen.
			// Avoid this by setting the texture content to an empty buffer (i.e. all black).
			if (buffer == null)
			{
				textureSize = width * height * IRenderingEngine_Fields.sizeOfTextureType[format];
				// Some video drivers sometimes crash when allocating a buffer exactly of the textureSize.
				// Allocating double the required size seems to workaround this issue...
				// This memory waste has no real negative impact on the memory usage
				// as we have only a single instance of the empty buffer.
				buffer = getEmptyBuffer(textureSize * 2);

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("setTexImage using an empty buffer of size 0x{0:X}", textureSize));
				}
			}

			base.setTexImage(level, internalFormat, width, height, format, type, textureSize, buffer);
		}

		public override int LogicOp
		{
			set
			{
				if (this.logicOp != value)
				{
					// Setting the logical operation to LOP_COPY is equivalent
					// to disabling the logical operation step.
					if (value == GeCommands.LOP_COPY)
					{
						disableFlag(IRenderingEngine_Fields.GU_COLOR_LOGIC_OP);
					}
					else
					{
						base.LogicOp = value;
					}
					this.logicOp = value;
				}
			}
		}
	}
}