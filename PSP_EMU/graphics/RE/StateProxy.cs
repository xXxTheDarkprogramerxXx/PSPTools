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
namespace pspsharp.graphics.RE
{


	/// <summary>
	/// @author gid15
	/// 
	/// RenderingEngine Proxy class removing redundant calls.
	/// E.g. calls setting multiple times the same value,
	/// or calls with an invalid parameter (e.g. for unused shader uniforms).
	/// This class implements no rendering logic, it just skips unnecessary calls.
	/// </summary>
	public class StateProxy : BaseRenderingEngineProxy
	{
		protected internal bool[] flags;
		protected internal float[][] matrix;
		protected internal const int RE_BONES_MATRIX = 4;
		protected internal const int matrix4Size = 4 * 4;
		public const int maxProgramId = 5000;
		public const int maxUniformId = 200;
		protected internal int[][] uniformInt;
		protected internal int[][][] uniformIntArray;
		protected internal float[][] uniformFloat;
		protected internal float[][][] uniformFloatArray;
		protected internal StateBoolean[] clientState;
		protected internal StateBoolean[] vertexAttribArray;
		protected internal bool colorMaskRed;
		protected internal bool colorMaskGreen;
		protected internal bool colorMaskBlue;
		protected internal bool colorMaskAlpha;
		protected internal int[] colorMask;
		protected internal bool depthMask;
		protected internal int textureFunc;
		protected internal bool textureFuncAlpha;
		protected internal bool textureFuncColorDouble;
		protected internal bool frontFace;
		protected internal int stencilFunc;
		protected internal int stencilFuncRef;
		protected internal int stencilFuncMask;
		protected internal int stencilOpFail;
		protected internal int stencilOpZFail;
		protected internal int stencilOpZPass;
		protected internal int depthFunc;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal int[] bindTexture_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal int[] bindBuffer_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal int useProgram_Renamed;
		protected internal int textureMapMode;
		protected internal int textureProjMapMode;
		protected internal int viewportX;
		protected internal int viewportY;
		protected internal int viewportWidth;
		protected internal int viewportHeight;
		protected internal Dictionary<int, int[]> bufferDataInt;
		protected internal Dictionary<int, TextureState> textureStates;
		protected internal TextureState currentTextureState;
		protected internal int matrixMode;
		protected internal bool fogHintSet;
		protected internal bool lineSmoothHintSet;
		protected internal float[] texEnvf;
		protected internal int[] texEnvi;
		protected internal int pixelStoreRowLength;
		protected internal int pixelStoreAlignment;
		protected internal int scissorX;
		protected internal int scissorY;
		protected internal int scissorWidth;
		protected internal int scissorHeight;
		protected internal int blendEquation;
		protected internal int shadeModel;
		protected internal int alphaFunc;
		protected internal int alphaFuncRef;
		protected internal int alphaFuncMask;
		protected internal float depthRangeZpos;
		protected internal float depthRangeZscale;
		protected internal int depthRangeNear;
		protected internal int depthRangeFar;
		protected internal float[] vertexColor;
		protected internal float[][] lightAmbientColor;
		protected internal float[][] lightDiffuseColor;
		protected internal float[][] lightSpecularColor;
		protected internal float[] lightModelAmbientColor;
		protected internal int lightMode;
		protected internal float[] materialAmbientColor;
		protected internal float[] materialDiffuseColor;
		protected internal float[] materialSpecularColor;
		protected internal float[] materialEmissiveColor;
		protected internal StateBoolean colorMaterialAmbient;
		protected internal StateBoolean colorMaterialDiffuse;
		protected internal StateBoolean colorMaterialSpecular;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal int bindVertexArray_Renamed;
		protected internal int activeTextureUnit;
		protected internal bool useTextureAnisotropicFilter;
		protected internal int dfix;
		protected internal int sfix;
		protected internal int bindFramebufferRead;
		protected internal int bindFramebufferDraw;

		protected internal class StateBoolean
		{
			internal bool undefined = true;
			internal bool value;

			public virtual bool Undefined
			{
				get
				{
					return undefined;
				}
			}

			public virtual void setUndefined()
			{
				undefined = true;
			}

			public virtual bool Value
			{
				get
				{
					return value;
				}
				set
				{
					this.value = value;
					undefined = false;
				}
			}


			public virtual bool True
			{
				get
				{
					return !undefined && value;
				}
			}

			public virtual bool False
			{
				get
				{
					return !undefined && !value;
				}
			}

			public virtual bool isValue(bool value)
			{
				return !undefined && this.value == value;
			}

			public override string ToString()
			{
				if (Undefined)
				{
					return "Undefined";
				}
				return Convert.ToString(Value);
			}
		}

		protected internal class TextureState
		{
			public int textureWrapModeS = -1;
			public int textureWrapModeT = -1;
			public int textureMipmapMinFilter = GeCommands.TFLT_NEAREST_MIPMAP_LINEAR;
			public int textureMipmapMagFilter = GeCommands.TFLT_LINEAR;
			public int textureMipmapMinLevel = 0;
			public int textureMipmapMaxLevel = 1000;
			public float textureAnisotropy = 0;
		}

		public StateProxy(IRenderingEngine proxy) : base(proxy)
		{
			init();
		}

		protected internal virtual void init()
		{
			flags = new bool[IRenderingEngine_Fields.RE_NUMBER_FLAGS];

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: uniformInt = new int[maxProgramId][maxUniformId];
			uniformInt = RectangularArrays.ReturnRectangularIntArray(maxProgramId, maxUniformId);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: uniformFloat = new float[maxProgramId][maxUniformId];
			uniformFloat = RectangularArrays.ReturnRectangularFloatArray(maxProgramId, maxUniformId);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: uniformIntArray = new int[maxProgramId][maxUniformId][];
			uniformIntArray = RectangularArrays.ReturnRectangularIntArray(maxProgramId, maxUniformId, -1);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: uniformFloatArray = new float[maxProgramId][maxUniformId][];
			uniformFloatArray = RectangularArrays.ReturnRectangularFloatArray(maxProgramId, maxUniformId, -1);
			for (int i = 0; i < maxProgramId; i++)
			{
				for (int j = 0; j < maxUniformId; j++)
				{
					uniformInt[i][j] = -1;
					uniformFloat[i][j] = -1f;
				}
			}

			matrix = new float[RE_BONES_MATRIX + 1][];
			matrix[IRenderingEngine_Fields.GU_PROJECTION] = new float[matrix4Size];
			matrix[IRenderingEngine_Fields.GU_VIEW] = new float[matrix4Size];
			matrix[IRenderingEngine_Fields.GU_MODEL] = new float[matrix4Size];
			matrix[IRenderingEngine_Fields.GU_TEXTURE] = new float[matrix4Size];
			matrix[RE_BONES_MATRIX] = new float[8 * matrix4Size];

			clientState = new StateBoolean[4];
			for (int i = 0; i < clientState.Length; i++)
			{
				clientState[i] = new StateBoolean();
			}
			vertexAttribArray = new StateBoolean[maxUniformId];
			for (int i = 0; i < vertexAttribArray.Length; i++)
			{
				vertexAttribArray[i] = new StateBoolean();
			}
			colorMask = new int[4];
			bufferDataInt = new Dictionary<int, int[]>();
			textureStates = new Dictionary<int, TextureState>();
			currentTextureState = new TextureState();
			textureStates[0] = currentTextureState;
			texEnvf = new float[17];
			texEnvi = new int[17];
			vertexColor = new float[4];
			bindBuffer_Renamed = new int[3];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: lightAmbientColor = new float[4][4];
			lightAmbientColor = RectangularArrays.ReturnRectangularFloatArray(4, 4);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: lightSpecularColor = new float[4][4];
			lightSpecularColor = RectangularArrays.ReturnRectangularFloatArray(4, 4);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: lightDiffuseColor = new float[4][4];
			lightDiffuseColor = RectangularArrays.ReturnRectangularFloatArray(4, 4);
			lightModelAmbientColor = new float[4];
			materialAmbientColor = new float[4];
			materialSpecularColor = new float[4];
			materialDiffuseColor = new float[4];
			materialEmissiveColor = new float[4];
			bindTexture_Renamed = new int[4]; // assume max 4 active texture units

			colorMaterialAmbient = new StateBoolean();
			colorMaterialDiffuse = new StateBoolean();
			colorMaterialSpecular = new StateBoolean();
		}

		public override void startDisplay()
		{
			// The following properties are lost when starting a new display
			for (int i = 0; i < clientState.Length; i++)
			{
				clientState[i].setUndefined();
			}

			for (int i = 0; i < flags.Length; i++)
			{
				flags[i] = true;
			}
			flags[IRenderingEngine_Fields.GU_TEXTURE_2D] = false;

			Array.Copy(identityMatrix, 0, matrix[IRenderingEngine_Fields.GU_PROJECTION], 0, matrix4Size);
			Array.Copy(identityMatrix, 0, matrix[IRenderingEngine_Fields.GU_VIEW], 0, matrix4Size);
			Array.Copy(identityMatrix, 0, matrix[IRenderingEngine_Fields.GU_MODEL], 0, matrix4Size);
			Array.Copy(identityMatrix, 0, matrix[IRenderingEngine_Fields.GU_TEXTURE], 0, matrix4Size);
			colorMaskRed = true;
			colorMaskGreen = true;
			colorMaskBlue = true;
			colorMaskAlpha = true;
			depthMask = true;
			textureFunc = -1;
			textureFuncAlpha = true;
			textureFuncColorDouble = false;
			frontFace = true;
			stencilFunc = -1;
			stencilFuncRef = -1;
			stencilFuncMask = -1;
			stencilOpFail = -1;
			stencilOpZFail = -1;
			stencilOpZPass = -1;
			depthFunc = -1;
			for (int i = 0; i < bindTexture_Renamed.Length; i++)
			{
				bindTexture_Renamed[i] = -1;
			}
			currentTextureState = textureStates[0];
			for (int i = 0; i < bindBuffer_Renamed.Length; i++)
			{
				bindBuffer_Renamed[i] = -1;
			}
			activeTextureUnit = 0;
			frontFace = false;
			useProgram_Renamed = 0;
			textureMapMode = -1;
			textureProjMapMode = -1;
			viewportX = -1;
			viewportY = -1;
			viewportWidth = -1;
			viewportHeight = -1;
			matrixMode = -1;
			fogHintSet = false;
			lineSmoothHintSet = false;
			for (int i = 0; i < texEnvi.Length; i++)
			{
				texEnvi[i] = -1;
			}
			for (int i = 0; i < texEnvf.Length; i++)
			{
				texEnvf[i] = -1;
			}
			// Default OpenGL texEnv values
			texEnvf[IRenderingEngine_Fields.RE_TEXENV_RGB_SCALE] = 1.0f;
			texEnvi[IRenderingEngine_Fields.RE_TEXENV_ENV_MODE] = IRenderingEngine_Fields.RE_TEXENV_MODULATE;
			pixelStoreRowLength = -1;
			pixelStoreAlignment = -1;
			scissorX = -1;
			scissorY = -1;
			scissorWidth = -1;
			scissorHeight = -1;
			blendEquation = -1;
			shadeModel = -1;
			alphaFunc = -1;
			alphaFuncRef = -1;
			depthRangeZpos = 0.0f;
			depthRangeZscale = 0.0f;
			depthRangeNear = -1;
			depthRangeFar = -1;
			vertexColor[0] = -1.0f;
			for (int i = 0; i < lightAmbientColor.Length; i++)
			{
				lightAmbientColor[i][0] = -1.0f;
				lightDiffuseColor[i][0] = -1.0f;
				lightSpecularColor[i][0] = -1.0f;
			}
			lightModelAmbientColor[0] = -1.0f;
			lightMode = -1;
			materialAmbientColor[0] = -10000.0f;
			materialDiffuseColor[0] = -1.0f;
			materialSpecularColor[0] = -1.0f;
			materialEmissiveColor[0] = -1.0f;
			colorMaterialAmbient.setUndefined();
			colorMaterialDiffuse.setUndefined();
			colorMaterialSpecular.setUndefined();
			bindVertexArray_Renamed = 0;
			bindFramebufferRead = -1;
			bindFramebufferDraw = -1;

			if (VideoEngine.Instance.UseTextureAnisotropicFilter != useTextureAnisotropicFilter)
			{
				// The texture anisotropic filter has been changed,
				// invalidate all the texture magnification filters
				foreach (TextureState textureState in textureStates.Values)
				{
					textureState.textureMipmapMagFilter = -1;
				}
				useTextureAnisotropicFilter = VideoEngine.Instance.UseTextureAnisotropicFilter;
			}

			base.startDisplay();
		}

		public override void disableFlag(int flag)
		{
			if (flags[flag])
			{
				base.disableFlag(flag);
				flags[flag] = false;
			}
		}

		public override void enableFlag(int flag)
		{
			if (!flags[flag])
			{
				base.enableFlag(flag);
				flags[flag] = true;
			}
		}

		public override void setUniform(int id, int value)
		{
			// An unused uniform as an id == -1
			if (id >= 0 && id <= maxUniformId)
			{
				if (uniformInt[useProgram_Renamed][id] != value)
				{
					base.setUniform(id, value);
					uniformInt[useProgram_Renamed][id] = value;
				}
			}
		}

		public override void setUniform(int id, float value)
		{
			if (id >= 0 && id <= maxUniformId)
			{
				if (uniformFloat[useProgram_Renamed][id] != value)
				{
					base.setUniform(id, value);
					uniformFloat[useProgram_Renamed][id] = value;
				}
			}
		}

		public override void setUniform(int id, int value1, int value2)
		{
			if (id >= 0 && id <= maxUniformId)
			{
				int[] oldValues = uniformIntArray[useProgram_Renamed][id];
				if (oldValues == null || oldValues.Length != 2)
				{
					base.setUniform(id, value1, value2);
					uniformIntArray[useProgram_Renamed][id] = new int[] {value1, value2};
				}
				else
				{
					if (oldValues[0] != value1 || oldValues[1] != value2)
					{
						base.setUniform(id, value1, value2);
						oldValues[0] = value1;
						oldValues[1] = value2;
					}
				}
			}
		}

		public override void setUniform2(int id, int[] values)
		{
			if (id >= 0 && id <= maxUniformId)
			{
				int[] oldValues = uniformIntArray[useProgram_Renamed][id];
				if (oldValues == null || oldValues.Length != 2)
				{
					base.setUniform2(id, values);
					oldValues = new int[2];
					oldValues[0] = values[0];
					oldValues[1] = values[1];
					uniformIntArray[useProgram_Renamed][id] = oldValues;
				}
				else if (oldValues[0] != values[0] || oldValues[1] != values[1])
				{
					base.setUniform2(id, values);
					oldValues[0] = values[0];
					oldValues[1] = values[1];
				}
			}
		}

		public override void setUniform3(int id, int[] values)
		{
			if (id >= 0 && id <= maxUniformId)
			{
				int[] oldValues = uniformIntArray[useProgram_Renamed][id];
				if (oldValues == null || oldValues.Length != 3)
				{
					base.setUniform3(id, values);
					oldValues = new int[3];
					oldValues[0] = values[0];
					oldValues[1] = values[1];
					oldValues[2] = values[2];
					uniformIntArray[useProgram_Renamed][id] = oldValues;
				}
				else if (oldValues[0] != values[0] || oldValues[1] != values[1] || oldValues[2] != values[2])
				{
					base.setUniform3(id, values);
					oldValues[0] = values[0];
					oldValues[1] = values[1];
					oldValues[2] = values[2];
				}
			}
		}

		public override void setUniform4(int id, int[] values)
		{
			if (id >= 0 && id <= maxUniformId)
			{
				int[] oldValues = uniformIntArray[useProgram_Renamed][id];
				if (oldValues == null || oldValues.Length != 4)
				{
					base.setUniform4(id, values);
					oldValues = new int[4];
					oldValues[0] = values[0];
					oldValues[1] = values[1];
					oldValues[2] = values[2];
					oldValues[3] = values[3];
					uniformIntArray[useProgram_Renamed][id] = oldValues;
				}
				else if (oldValues[0] != values[0] || oldValues[1] != values[1] || oldValues[2] != values[2] || oldValues[3] != values[3])
				{
					base.setUniform4(id, values);
					oldValues[0] = values[0];
					oldValues[1] = values[1];
					oldValues[2] = values[2];
					oldValues[3] = values[3];
				}
			}
		}

		public override void setUniformMatrix4(int id, int count, float[] values)
		{
			if (id >= 0 && id <= maxUniformId && count > 0)
			{
				float[] oldValues = uniformFloatArray[useProgram_Renamed][id];
				int length = count * matrix4Size;
				if (oldValues == null || oldValues.Length < length)
				{
					base.setUniformMatrix4(id, count, values);
					oldValues = new float[length];
					Array.Copy(values, 0, oldValues, 0, length);
					uniformFloatArray[useProgram_Renamed][id] = oldValues;
				}
				else
				{
					bool differ = false;
					for (int i = 0; i < length; i++)
					{
						if (oldValues[i] != values[i])
						{
							differ = true;
							break;
						}
					}

					if (differ)
					{
						base.setUniformMatrix4(id, count, values);
						Array.Copy(values, 0, oldValues, 0, length);
					}
				}
			}
		}

		protected internal virtual int matrixFirstUpdated(int id, float[] values)
		{
			if (values == null)
			{
				values = identityMatrix;
			}

			float[] oldValues = matrix[id];
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i] != oldValues[i])
				{
					// Update the remaining values
					Array.Copy(values, i, oldValues, i, values.Length - i);
					return i;
				}
			}

			return values.Length;
		}

		protected internal virtual int matrixLastUpdated(int id, float[] values, int length)
		{
			float[] oldValues = matrix[id];

			if (values == null)
			{
				values = identityMatrix;
			}

			for (int i = length - 1; i >= 0; i--)
			{
				if (oldValues[i] != values[i])
				{
					// Update the remaining values
					Array.Copy(values, 0, oldValues, 0, i + 1);
					return i;
				}
			}

			return 0;
		}

		protected internal virtual bool isIdentityMatrix(float[] values)
		{
			if (values == null)
			{
				return true;
			}

			if (values.Length != identityMatrix.Length)
			{
				return false;
			}

			for (int i = 0; i < identityMatrix.Length; i++)
			{
				if (values[i] != identityMatrix[i])
				{
					return false;
				}
			}

			return true;
		}

		public override void disableClientState(int type)
		{
			StateBoolean state = clientState[type];
			if (!state.False)
			{
				base.disableClientState(type);
				state.Value = false;
			}
		}

		public override void enableClientState(int type)
		{
			// enableClientState(RE_VERTEX) cannot be cached: it is required each time
			// by OpenGL and seems to trigger the correct Vertex generation.
			StateBoolean state = clientState[type];
			if (type == IRenderingEngine_Fields.RE_VERTEX || !state.True)
			{
				base.enableClientState(type);
				state.Value = true;
			}
		}

		public override void disableVertexAttribArray(int id)
		{
			if (id >= 0 && id <= maxUniformId)
			{
				StateBoolean state = vertexAttribArray[id];
				if (!state.False)
				{
					base.disableVertexAttribArray(id);
					state.Value = false;
				}
			}
		}

		public override void enableVertexAttribArray(int id)
		{
			if (id >= 0 && id <= maxUniformId)
			{
				StateBoolean state = vertexAttribArray[id];
				if (!state.True)
				{
					base.enableVertexAttribArray(id);
					state.Value = true;
				}
			}
		}

		public override void setColorMask(bool redWriteEnabled, bool greenWriteEnabled, bool blueWriteEnabled, bool alphaWriteEnabled)
		{
			if (redWriteEnabled != colorMaskRed || greenWriteEnabled != colorMaskGreen || blueWriteEnabled != colorMaskBlue || alphaWriteEnabled != colorMaskAlpha)
			{
				base.setColorMask(redWriteEnabled, greenWriteEnabled, blueWriteEnabled, alphaWriteEnabled);
				colorMaskRed = redWriteEnabled;
				colorMaskGreen = greenWriteEnabled;
				colorMaskBlue = blueWriteEnabled;
				colorMaskAlpha = alphaWriteEnabled;
				// Force a reload of the real color mask
				colorMask[0] = -1;
				colorMask[1] = -1;
				colorMask[2] = -1;
				colorMask[3] = -1;
			}
		}

		public override void setColorMask(int redMask, int greenMask, int blueMask, int alphaMask)
		{
			if (redMask != colorMask[0] || greenMask != colorMask[1] || blueMask != colorMask[2] || alphaMask != colorMask[3])
			{
				base.setColorMask(redMask, greenMask, blueMask, alphaMask);
	//			colorMaskRed = redMask != 0xFF;
	//			colorMaskGreen = greenMask != 0xFF;
	//			colorMaskBlue = blueMask != 0xFF;
	//			colorMaskAlpha = alphaMask != 0xFF;
				colorMask[0] = redMask;
				colorMask[1] = greenMask;
				colorMask[2] = blueMask;
				colorMask[3] = alphaMask;
			}
		}

		public override bool DepthMask
		{
			set
			{
				if (value != depthMask)
				{
					base.DepthMask = value;
					depthMask = value;
				}
			}
		}

		public override bool FrontFace
		{
			set
			{
				if (value != frontFace)
				{
					base.FrontFace = value;
					frontFace = value;
				}
			}
		}

		public override void setTextureFunc(int func, bool alphaUsed, bool colorDoubled)
		{
			if (func != textureFunc || alphaUsed != textureFuncAlpha || colorDoubled != textureFuncColorDouble)
			{
				base.setTextureFunc(func, alphaUsed, colorDoubled);
				textureFunc = func;
				textureFuncAlpha = alphaUsed;
				textureFuncColorDouble = colorDoubled;
			}
		}

		public override int TextureMipmapMinFilter
		{
			set
			{
				if (value != currentTextureState.textureMipmapMinFilter)
				{
					base.TextureMipmapMinFilter = value;
					currentTextureState.textureMipmapMinFilter = value;
				}
			}
		}

		public override int TextureMipmapMagFilter
		{
			set
			{
				if (value != currentTextureState.textureMipmapMagFilter)
				{
					base.TextureMipmapMagFilter = value;
					currentTextureState.textureMipmapMagFilter = value;
				}
			}
		}

		public override int TextureMipmapMinLevel
		{
			set
			{
				if (value != currentTextureState.textureMipmapMinLevel)
				{
					base.TextureMipmapMinLevel = value;
					currentTextureState.textureMipmapMinLevel = value;
				}
			}
		}

		public override int TextureMipmapMaxLevel
		{
			set
			{
				if (value != currentTextureState.textureMipmapMaxLevel)
				{
					base.TextureMipmapMaxLevel = value;
					currentTextureState.textureMipmapMaxLevel = value;
				}
			}
		}

		public override void setTextureWrapMode(int s, int t)
		{
			if (s != currentTextureState.textureWrapModeS || t != currentTextureState.textureWrapModeT)
			{
				base.setTextureWrapMode(s, t);
				currentTextureState.textureWrapModeS = s;
				currentTextureState.textureWrapModeT = t;
			}
		}

		public override void bindTexture(int texture)
		{
			if (texture != bindTexture_Renamed[activeTextureUnit])
			{
				base.bindTexture(texture);
				bindTexture_Renamed[activeTextureUnit] = texture;
				// Binding a new texture change the OpenGL texture wrap mode and min/mag filters
				currentTextureState = textureStates[texture];
				if (currentTextureState == null)
				{
					currentTextureState = new TextureState();
					textureStates[texture] = currentTextureState;
				}
			}
		}

		public override int DepthFunc
		{
			set
			{
				if (value != depthFunc)
				{
					base.DepthFunc = value;
					depthFunc = value;
				}
			}
		}

		public override void setStencilFunc(int func, int @ref, int mask)
		{
			if (func != stencilFunc || @ref != stencilFuncRef || mask != stencilFuncMask)
			{
				base.setStencilFunc(func, @ref, mask);
				stencilFunc = func;
				stencilFuncRef = @ref;
				stencilFuncMask = mask;
			}
		}

		public override void setStencilOp(int fail, int zfail, int zpass)
		{
			if (fail != stencilOpFail || zfail != stencilOpZFail || zpass != stencilOpZPass)
			{
				base.setStencilOp(fail, zfail, zpass);
				stencilOpFail = fail;
				stencilOpZFail = zfail;
				stencilOpZPass = zpass;
			}
		}

		public override void deleteTexture(int texture)
		{
			textureStates.Remove(texture);
			// When deleting the current texture, the current binding is reset to 0
			for (int i = 0; i < bindTexture_Renamed.Length; i++)
			{
				if (texture == bindTexture_Renamed[i])
				{
					bindTexture_Renamed[i] = 0;
					if (i == activeTextureUnit)
					{
						currentTextureState = textureStates[bindTexture_Renamed[activeTextureUnit]];
					}
				}
			}
			base.deleteTexture(texture);
		}

		public override void bindBuffer(int target, int buffer)
		{
			if (bindBuffer_Renamed[target] != buffer)
			{
				base.bindBuffer(target, buffer);
				bindBuffer_Renamed[target] = buffer;
			}
		}

		public override void deleteBuffer(int buffer)
		{
			// When deleting the current buffer, the current binding is reset to 0
			for (int target = 0; target < bindBuffer_Renamed.Length; target++)
			{
				if (buffer == bindBuffer_Renamed[target])
				{
					bindBuffer_Renamed[target] = 0;
				}
			}
			base.deleteBuffer(buffer);
		}

		public override void setViewport(int x, int y, int width, int height)
		{
			// Negative x and y values are valid values
			if (width >= 0 && height >= 0)
			{
				if (x != viewportX || y != viewportY || width != viewportWidth || height != viewportHeight)
				{
					base.setViewport(x, y, width, height);
					viewportX = x;
					viewportY = y;
					viewportWidth = width;
					viewportHeight = height;
				}
			}
		}

		public override void useProgram(int program)
		{
			if (useProgram_Renamed != program)
			{
				base.useProgram(program);
				useProgram_Renamed = program;
			}
		}

		public override void setTextureMapMode(int mode, int proj)
		{
			if (mode != textureMapMode || proj != textureProjMapMode)
			{
				base.setTextureMapMode(mode, proj);
				textureMapMode = mode;
				textureProjMapMode = proj;
			}
		}

		private void setBufferData(int target, int size, IntBuffer buffer, int usage)
		{
			int[] oldData = bufferDataInt[bindBuffer_Renamed[target]];
			int[] newData = new int[size / 4];
			int position = buffer.position();
			buffer.get(newData);
			buffer.position(position);

			bool differ = true;
			bool setBufferData = true;
			if (oldData != null && newData.Length == oldData.Length)
			{
				differ = false;
				setBufferData = false;
				int limit = buffer.limit();
				for (int i = 0; i < newData.Length; i++)
				{
					if (newData[i] != oldData[i])
					{
						differ = true;
						int end = i + 1;
						for (int j = i + 1; j < newData.Length; j++)
						{
							if (newData[j] == oldData[j])
							{
								end = j;
								break;
							}
						}
						buffer.position(i);
						base.setBufferSubData(target, i * 4, (end - i) * 4, buffer);
						buffer.limit(limit);
						i = end;
					}
				}
			}

			if (setBufferData)
			{
				base.setBufferData(target, size, buffer, usage);
			}
			if (differ)
			{
				bufferDataInt[bindBuffer_Renamed[target]] = newData;
			}
		}

		public override void setBufferData(int target, int size, Buffer buffer, int usage)
		{
			if (target == IRenderingEngine_Fields.RE_UNIFORM_BUFFER && size <= 1024 && (size & 3) == 0 && buffer is IntBuffer)
			{
				setBufferData(target, size, (IntBuffer) buffer, usage);
			}
			else if (target == IRenderingEngine_Fields.RE_UNIFORM_BUFFER && size <= 1024 && (size & 3) == 0 && buffer is ByteBuffer)
			{
				setBufferData(target, size, ((ByteBuffer) buffer).asIntBuffer(), usage);
			}
			else
			{
				base.setBufferData(target, size, buffer, usage);
			}
		}

		public override int MatrixMode
		{
			set
			{
				if (value != matrixMode)
				{
					base.MatrixMode = value;
					matrixMode = value;
				}
			}
		}

		public override float[] Matrix
		{
			set
			{
				if (matrixFirstUpdated(matrixMode, value) < matrix4Size)
				{
					if (isIdentityMatrix(value))
					{
						// Identity Matrix is identified by the special value "null"
						base.Matrix = null;
					}
					else
					{
						base.Matrix = value;
					}
				}
			}
		}

		public override void multMatrix(float[] values)
		{
			if (!isIdentityMatrix(values))
			{
				base.multMatrix(values);
			}
		}

		public override void setFogHint()
		{
			if (!fogHintSet)
			{
				base.setFogHint();
				fogHintSet = true;
			}
		}

		public override void setLineSmoothHint()
		{
			if (!lineSmoothHintSet)
			{
				base.setLineSmoothHint();
				lineSmoothHintSet = true;
			}
		}

		public override void setTexEnv(int name, float param)
		{
			if (texEnvf[name] != param)
			{
				base.setTexEnv(name, param);
				texEnvf[name] = param;
			}
		}

		public override void setTexEnv(int name, int param)
		{
			if (texEnvi[name] != param)
			{
				base.setTexEnv(name, param);
				texEnvi[name] = param;
			}
		}

		public override void setPixelStore(int rowLength, int alignment)
		{
			if (pixelStoreRowLength != rowLength || pixelStoreAlignment != alignment)
			{
				base.setPixelStore(rowLength, alignment);
				pixelStoreRowLength = rowLength;
				pixelStoreAlignment = alignment;
			}
		}

		public override void setScissor(int x, int y, int width, int height)
		{
			if (x >= 0 && y >= 0 && width >= 0 && height >= 0)
			{
				if (x != scissorX || y != scissorY || width != scissorWidth || height != scissorHeight)
				{
					base.setScissor(x, y, width, height);
					scissorX = x;
					scissorY = y;
					scissorWidth = width;
					scissorHeight = height;
				}
			}
		}

		public override int BlendEquation
		{
			set
			{
				if (blendEquation != value)
				{
					base.BlendEquation = value;
					blendEquation = value;
				}
			}
		}

		public override int ShadeModel
		{
			set
			{
				if (shadeModel != value)
				{
					base.ShadeModel = value;
					shadeModel = value;
				}
			}
		}

		public override void setAlphaFunc(int func, int @ref, int mask)
		{
			if (alphaFunc != func || alphaFuncRef != @ref || alphaFuncMask != mask)
			{
				base.setAlphaFunc(func, @ref, mask);
				alphaFunc = func;
				alphaFuncRef = @ref;
				alphaFuncMask = mask;
			}
		}

		public override void setDepthRange(float zpos, float zscale, int near, int far)
		{
			if (depthRangeZpos != zpos || depthRangeZscale != zscale || depthRangeNear != near || depthRangeFar != far)
			{
				base.setDepthRange(zpos, zscale, near, far);
				depthRangeZpos = zpos;
				depthRangeZscale = zscale;
				depthRangeNear = near;
				depthRangeFar = far;
			}
		}

		public override float[] VertexColor
		{
			set
			{
				if (vertexColor[0] != value[0] || vertexColor[1] != value[1] || vertexColor[2] != value[2] || vertexColor[3] != value[3])
				{
					base.VertexColor = value;
					vertexColor[0] = value[0];
					vertexColor[1] = value[1];
					vertexColor[2] = value[2];
					vertexColor[3] = value[3];
				}
			}
		}

		public override void setLightAmbientColor(int light, float[] color)
		{
			float[] stateColor = lightAmbientColor[light];
			if (stateColor[0] != color[0] || stateColor[1] != color[1] || stateColor[2] != color[2] || stateColor[3] != color[3])
			{
				base.setLightAmbientColor(light, color);
				stateColor[0] = color[0];
				stateColor[1] = color[1];
				stateColor[2] = color[2];
				stateColor[3] = color[3];
			}
		}

		public override void setLightDiffuseColor(int light, float[] color)
		{
			float[] stateColor = lightDiffuseColor[light];
			if (stateColor[0] != color[0] || stateColor[1] != color[1] || stateColor[2] != color[2] || stateColor[3] != color[3])
			{
				base.setLightDiffuseColor(light, color);
				stateColor[0] = color[0];
				stateColor[1] = color[1];
				stateColor[2] = color[2];
				stateColor[3] = color[3];
			}
		}

		public override void setLightSpecularColor(int light, float[] color)
		{
			float[] stateColor = lightSpecularColor[light];
			if (stateColor[0] != color[0] || stateColor[1] != color[1] || stateColor[2] != color[2] || stateColor[3] != color[3])
			{
				base.setLightSpecularColor(light, color);
				stateColor[0] = color[0];
				stateColor[1] = color[1];
				stateColor[2] = color[2];
				stateColor[3] = color[3];
			}
		}

		public override float[] MaterialAmbientColor
		{
			set
			{
				if (materialAmbientColor[0] != value[0] || materialAmbientColor[1] != value[1] || materialAmbientColor[2] != value[2] || materialAmbientColor[3] != value[3])
				{
					base.MaterialAmbientColor = value;
					materialAmbientColor[0] = value[0];
					materialAmbientColor[1] = value[1];
					materialAmbientColor[2] = value[2];
					materialAmbientColor[3] = value[3];
				}
			}
		}

		public override float[] MaterialDiffuseColor
		{
			set
			{
				if (materialDiffuseColor[0] != value[0] || materialDiffuseColor[1] != value[1] || materialDiffuseColor[2] != value[2] || materialDiffuseColor[3] != value[3])
				{
					base.MaterialDiffuseColor = value;
					materialDiffuseColor[0] = value[0];
					materialDiffuseColor[1] = value[1];
					materialDiffuseColor[2] = value[2];
					materialDiffuseColor[3] = value[3];
				}
			}
		}

		public override float[] MaterialEmissiveColor
		{
			set
			{
				if (materialEmissiveColor[0] != value[0] || materialEmissiveColor[1] != value[1] || materialEmissiveColor[2] != value[2] || materialEmissiveColor[3] != value[3])
				{
					base.MaterialEmissiveColor = value;
					materialEmissiveColor[0] = value[0];
					materialEmissiveColor[1] = value[1];
					materialEmissiveColor[2] = value[2];
					materialEmissiveColor[3] = value[3];
				}
			}
		}

		public override float[] MaterialSpecularColor
		{
			set
			{
				if (materialSpecularColor[0] != value[0] || materialSpecularColor[1] != value[1] || materialSpecularColor[2] != value[2] || materialSpecularColor[3] != value[3])
				{
					base.MaterialSpecularColor = value;
					materialSpecularColor[0] = value[0];
					materialSpecularColor[1] = value[1];
					materialSpecularColor[2] = value[2];
					materialSpecularColor[3] = value[3];
				}
			}
		}

		public override void setColorMaterial(bool ambient, bool diffuse, bool specular)
		{
			if (!colorMaterialAmbient.isValue(ambient) || !colorMaterialDiffuse.isValue(diffuse) || !colorMaterialSpecular.isValue(specular))
			{
				base.setColorMaterial(ambient, diffuse, specular);
				colorMaterialAmbient.Value = ambient;
				colorMaterialDiffuse.Value = diffuse;
				colorMaterialSpecular.Value = specular;
			}
		}

		private void invalidateMaterialColors()
		{
			// Drawing with "color material" enabled overwrites the material colors
			if (flags[IRenderingEngine_Fields.RE_COLOR_MATERIAL])
			{
				if (colorMaterialAmbient.True)
				{
					materialAmbientColor[0] = -1.0f;
				}
				if (colorMaterialDiffuse.True)
				{
					materialDiffuseColor[0] = -1.0f;
				}
				if (colorMaterialSpecular.True)
				{
					materialSpecularColor[0] = -1.0f;
				}
			}
		}

		public override void drawArrays(int type, int first, int count)
		{
			invalidateMaterialColors();
			base.drawArrays(type, first, count);
		}

		public override int LightMode
		{
			set
			{
				if (lightMode != value)
				{
					base.LightMode = value;
					lightMode = value;
				}
			}
		}

		public override float[] LightModelAmbientColor
		{
			set
			{
				if (lightModelAmbientColor[0] != value[0] || lightModelAmbientColor[1] != value[1] || lightModelAmbientColor[2] != value[2] || lightModelAmbientColor[3] != value[3])
				{
					base.LightModelAmbientColor = value;
					lightModelAmbientColor[0] = value[0];
					lightModelAmbientColor[1] = value[1];
					lightModelAmbientColor[2] = value[2];
					lightModelAmbientColor[3] = value[3];
				}
			}
		}

		private void onVertexArrayChanged()
		{
			for (int i = 0; i < clientState.Length; i++)
			{
				clientState[i].setUndefined();
			}
			for (int i = 0; i < vertexAttribArray.Length; i++)
			{
				vertexAttribArray[i].setUndefined();
			}
			bindBuffer_Renamed[IRenderingEngine_Fields.RE_ELEMENT_ARRAY_BUFFER] = -1;
		}

		public override void bindVertexArray(int id)
		{
			if (id != bindVertexArray_Renamed)
			{
				onVertexArrayChanged();
				base.bindVertexArray(id);
				bindVertexArray_Renamed = id;
			}
		}

		public override void deleteVertexArray(int id)
		{
			// When deleting the current vertex array, the current binding is reset to 0
			if (id == bindVertexArray_Renamed)
			{
				onVertexArrayChanged();
				bindVertexArray_Renamed = 0;
			}
			base.deleteVertexArray(id);
		}

		public override int ActiveTexture
		{
			set
			{
				if (value != activeTextureUnit)
				{
					base.ActiveTexture = value;
					activeTextureUnit = value;
					currentTextureState = textureStates[bindTexture_Renamed[activeTextureUnit]];
				}
			}
		}

		public override void bindActiveTexture(int index, int texture)
		{
			if (texture != bindTexture_Renamed[index])
			{
				base.bindActiveTexture(index, texture);
				bindTexture_Renamed[index] = texture;
				if (index == activeTextureUnit)
				{
					// Binding a new texture change the OpenGL texture wrap mode and min/mag filters
					currentTextureState = textureStates[texture];
					if (currentTextureState == null)
					{
						currentTextureState = new TextureState();
						textureStates[texture] = currentTextureState;
					}
				}
			}
		}

		public override float TextureAnisotropy
		{
			set
			{
				if (value != currentTextureState.textureAnisotropy)
				{
					base.TextureAnisotropy = value;
					currentTextureState.textureAnisotropy = value;
				}
			}
		}

		public override void setBlendSFix(int sfix, float[] color)
		{
			if (this.sfix != sfix)
			{
				base.setBlendSFix(sfix, color);
				this.sfix = sfix;
			}
		}

		public override void setBlendDFix(int dfix, float[] color)
		{
			if (this.dfix != dfix)
			{
				base.setBlendDFix(dfix, color);
				this.dfix = dfix;
			}
		}

		public override void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, long offset)
		{
			// id==-1 is a non-existing vertex attrib
			if (id >= 0)
			{
				base.setVertexAttribPointer(id, size, type, normalized, stride, offset);
			}
		}

		public override void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, int bufferSize, Buffer buffer)
		{
			// id==-1 is a non-existing vertex attrib
			if (id >= 0)
			{
				base.setVertexAttribPointer(id, size, type, normalized, stride, bufferSize, buffer);
			}
		}

		public override void bindFramebuffer(int target, int framebuffer)
		{
			switch (target)
			{
				case IRenderingEngine_Fields.RE_FRAMEBUFFER:
					if (framebuffer != bindFramebufferRead || framebuffer != bindFramebufferDraw)
					{
						base.bindFramebuffer(target, framebuffer);
						bindFramebufferRead = framebuffer;
						bindFramebufferDraw = framebuffer;
					}
					break;
				case IRenderingEngine_Fields.RE_READ_FRAMEBUFFER:
					if (framebuffer != bindFramebufferRead)
					{
						base.bindFramebuffer(target, framebuffer);
						bindFramebufferRead = framebuffer;
					}
					break;
				case IRenderingEngine_Fields.RE_DRAW_FRAMEBUFFER:
					if (framebuffer != bindFramebufferDraw)
					{
						base.bindFramebuffer(target, framebuffer);
						bindFramebufferDraw = framebuffer;
					}
					break;
				default:
					base.bindFramebuffer(target, framebuffer);
					break;
			}
		}
	}

}