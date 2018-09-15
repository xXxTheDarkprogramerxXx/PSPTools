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
namespace pspsharp.graphics.RE
{
	//using Logger = org.apache.log4j.Logger;


	/// <summary>
	/// @author gid15
	/// 
	/// The current values for the Shader uniform variables.
	/// The Shader uniform values have to be updated when switching the active
	/// shader program.
	/// </summary>
	public class ShaderContext
	{
		protected internal static Logger log = VideoEngine.log_Renamed;
		private float zPos;
		private float zScale;
		private int[] matFlags = new int[3];
		private int lightingEnable;
		private int lightMode;
		private int[] lightType = new int[4];
		private int[] lightKind = new int[4];
		private int[] lightEnabled = new int[4];
		private float[] boneMatrix = new float[8 * 16];
		private int numberBones;
		private int texEnable;
		private int texMapMode;
		private int texMapProj;
		private int[] texShade = new int[2];
		private int ctestEnable;
		private int ctestFunc;
		private int[] ctestMsk = new int[3];
		private int[] ctestRef = new int[3];
		private int[] texEnvMode = new int[2];
		private float colorDoubling;
		private int vinfoColor;
		private int vinfoPosition;
		private int vinfoTexture;
		private int vinfoNormal;
		private int vinfoTransform2D;
		private float positionScale;
		private float normalScale;
		private float textureScale;
		private float weightScale;
		private int clutShift;
		private int clutMask;
		private int clutOffset;
		private bool mipmapShareClut;
		private int clut = -1;
		private int texPixelFormat;
		private int tex = 0;
		private int utex = -1;
		private float[] vertexColor = new float[4];
		private int clutIndexHint;
		private int stencilTestEnable;
		private int stencilFunc;
		private int stencilRef;
		private int stencilMask;
		private int stencilOpFail;
		private int stencilOpZFail;
		private int stencilOpZPass;
		private int depthTestEnable;
		private int depthFunc;
		private int depthMask;
		private int fbTex = -1;
		private int colorMaskEnable;
		private int[] colorMask = new int[4];
		private int[] notColorMask = new int[4];
		private int alphaTestEnable;
		private int alphaTestFunc;
		private int alphaTestRef;
		private int alphaTestMask;
		private int blendTestEnable;
		private int blendEquation;
		private int blendSrc;
		private int blendDst;
		private float[] blendSFix = new float[3];
		private float[] blendDFix = new float[3];
		private int copyRedToAlpha;
		private int wrapModeS;
		private int wrapModeT;
		private int fogEnable;
		private float[] fogColor = new float[3];
		private float fogEnd;
		private float fogScale;
		private int clipPlaneEnable;
		private float[] viewportPos = new float[3];
		private float[] viewportScale = new float[3];

		public virtual void setUniforms(IRenderingEngine re, int shaderProgram)
		{
			re.setUniform(Uniforms.zPos.getId(shaderProgram), zPos);
			re.setUniform(Uniforms.zScale.getId(shaderProgram), zScale);
			re.setUniform3(Uniforms.matFlags.getId(shaderProgram), matFlags);
			re.setUniform4(Uniforms.lightEnabled.getId(shaderProgram), lightEnabled);
			re.setUniform(Uniforms.lightMode.getId(shaderProgram), lightMode);
			re.setUniform4(Uniforms.lightType.getId(shaderProgram), lightType);
			re.setUniform4(Uniforms.lightKind.getId(shaderProgram), lightKind);
			re.setUniform(Uniforms.lightingEnable.getId(shaderProgram), lightingEnable);
			re.setUniformMatrix4(Uniforms.boneMatrix.getId(shaderProgram), numberBones, boneMatrix);
			re.setUniform(Uniforms.numberBones.getId(shaderProgram), numberBones);
			re.setUniform(Uniforms.texEnable.getId(shaderProgram), texEnable);
			re.setUniform(Uniforms.texMapMode.getId(shaderProgram), texMapMode);
			re.setUniform(Uniforms.texMapProj.getId(shaderProgram), texMapProj);
			re.setUniform2(Uniforms.texShade.getId(shaderProgram), texShade);
			re.setUniform(Uniforms.ctestEnable.getId(shaderProgram), ctestEnable);
			re.setUniform(Uniforms.ctestFunc.getId(shaderProgram), ctestFunc);
			re.setUniform3(Uniforms.ctestMsk.getId(shaderProgram), ctestMsk);
			re.setUniform3(Uniforms.ctestRef.getId(shaderProgram), ctestRef);
			re.setUniform2(Uniforms.texEnvMode.getId(shaderProgram), texEnvMode);
			re.setUniform(Uniforms.colorDoubling.getId(shaderProgram), colorDoubling);
			re.setUniform(Uniforms.vinfoColor.getId(shaderProgram), vinfoColor);
			re.setUniform(Uniforms.vinfoPosition.getId(shaderProgram), vinfoPosition);
			re.setUniform(Uniforms.vinfoTransform2D.getId(shaderProgram), vinfoTransform2D);
			re.setUniform(Uniforms.positionScale.getId(shaderProgram), positionScale);
			re.setUniform(Uniforms.normalScale.getId(shaderProgram), normalScale);
			re.setUniform(Uniforms.textureScale.getId(shaderProgram), textureScale);
			re.setUniform(Uniforms.weightScale.getId(shaderProgram), weightScale);
			re.setUniform(Uniforms.clutShift.getId(shaderProgram), clutShift);
			re.setUniform(Uniforms.clutMask.getId(shaderProgram), clutMask);
			re.setUniform(Uniforms.clutOffset.getId(shaderProgram), clutOffset);
			re.setUniform(Uniforms.mipmapShareClut.getId(shaderProgram), mipmapShareClut ? 1 : 0);
			re.setUniform(Uniforms.texPixelFormat.getId(shaderProgram), texPixelFormat);
			re.setUniform4(Uniforms.vertexColor.getId(shaderProgram), vertexColor);
			re.setUniform(Uniforms.vinfoTexture.getId(shaderProgram), vinfoTexture);
			re.setUniform(Uniforms.vinfoNormal.getId(shaderProgram), vinfoNormal);
			re.setUniform(Uniforms.stencilTestEnable.getId(shaderProgram), stencilTestEnable);
			re.setUniform(Uniforms.stencilFunc.getId(shaderProgram), stencilFunc);
			re.setUniform(Uniforms.stencilRef.getId(shaderProgram), stencilRef);
			re.setUniform(Uniforms.stencilMask.getId(shaderProgram), stencilMask);
			re.setUniform(Uniforms.stencilOpFail.getId(shaderProgram), stencilOpFail);
			re.setUniform(Uniforms.stencilOpZFail.getId(shaderProgram), stencilOpZFail);
			re.setUniform(Uniforms.stencilOpZPass.getId(shaderProgram), stencilOpZPass);
			re.setUniform(Uniforms.depthTestEnable.getId(shaderProgram), depthTestEnable);
			re.setUniform(Uniforms.depthFunc.getId(shaderProgram), depthFunc);
			re.setUniform(Uniforms.depthMask.getId(shaderProgram), depthMask);
			re.setUniform(Uniforms.colorMaskEnable.getId(shaderProgram), colorMaskEnable);
			re.setUniform4(Uniforms.colorMask.getId(shaderProgram), colorMask);
			re.setUniform4(Uniforms.notColorMask.getId(shaderProgram), notColorMask);
			re.setUniform(Uniforms.alphaTestEnable.getId(shaderProgram), alphaTestEnable);
			re.setUniform(Uniforms.alphaTestFunc.getId(shaderProgram), alphaTestFunc);
			re.setUniform(Uniforms.alphaTestRef.getId(shaderProgram), alphaTestRef);
			re.setUniform(Uniforms.alphaTestMask.getId(shaderProgram), alphaTestMask);
			re.setUniform(Uniforms.blendTestEnable.getId(shaderProgram), blendTestEnable);
			re.setUniform(Uniforms.blendEquation.getId(shaderProgram), blendEquation);
			re.setUniform(Uniforms.blendSrc.getId(shaderProgram), blendSrc);
			re.setUniform(Uniforms.blendDst.getId(shaderProgram), blendDst);
			re.setUniform3(Uniforms.blendSFix.getId(shaderProgram), blendSFix);
			re.setUniform3(Uniforms.blendDFix.getId(shaderProgram), blendDFix);
			re.setUniform(Uniforms.copyRedToAlpha.getId(shaderProgram), copyRedToAlpha);
			re.setUniform(Uniforms.wrapModeS.getId(shaderProgram), wrapModeS);
			re.setUniform(Uniforms.wrapModeT.getId(shaderProgram), wrapModeT);
			re.setUniform(Uniforms.fogEnable.getId(shaderProgram), fogEnable);
			re.setUniform3(Uniforms.fogColor.getId(shaderProgram), fogColor);
			re.setUniform(Uniforms.fogEnd.getId(shaderProgram), fogEnd);
			re.setUniform(Uniforms.fogScale.getId(shaderProgram), fogScale);
			re.setUniform(Uniforms.clipPlaneEnable.getId(shaderProgram), clipPlaneEnable);
			re.setUniform3(Uniforms.viewportPos.getId(shaderProgram), viewportPos);
			re.setUniform3(Uniforms.viewportScale.getId(shaderProgram), viewportScale);

			setUniformsSamplers(re, shaderProgram);
		}

		protected internal virtual void setUniformsSamplers(IRenderingEngine re, int shaderProgram)
		{
			re.setUniform(Uniforms.clut.getId(shaderProgram), clut);
			re.setUniform(Uniforms.tex.getId(shaderProgram), tex);
			re.setUniform(Uniforms.utex.getId(shaderProgram), utex);
			re.setUniform(Uniforms.fbTex.getId(shaderProgram), fbTex);
		}

		public virtual void initShaderProgram(IRenderingEngine re, int shaderProgram)
		{
			// Nothing to do here
		}

		public virtual float ZPos
		{
			get
			{
				return zPos;
			}
			set
			{
				zPos = value;
			}
		}


		public virtual float ZScale
		{
			get
			{
				return zScale;
			}
			set
			{
				zScale = value;
			}
		}


		public virtual int getMatFlags(int index)
		{
			return matFlags[index];
		}

		public virtual void setMatFlags(int index, int matFlags)
		{
			this.matFlags[index] = matFlags;
		}

		public virtual int LightingEnable
		{
			get
			{
				return lightingEnable;
			}
			set
			{
				this.lightingEnable = value;
			}
		}


		public virtual int LightMode
		{
			get
			{
				return lightMode;
			}
			set
			{
				this.lightMode = value;
			}
		}


		public virtual int getLightType(int light)
		{
			return lightType[light];
		}

		public virtual void setLightType(int light, int lightType)
		{
			this.lightType[light] = lightType;
		}

		public virtual int getLightKind(int light)
		{
			return lightKind[light];
		}

		public virtual void setLightKind(int light, int lightKind)
		{
			this.lightKind[light] = lightKind;
		}

		public virtual int getLightEnabled(int light)
		{
			return lightEnabled[light];
		}

		public virtual void setLightEnabled(int light, int lightEnabled)
		{
			this.lightEnabled[light] = lightEnabled;
		}

		public virtual int BoneMatrixLength
		{
			get
			{
				return boneMatrix.Length;
			}
		}

		public virtual float[] BoneMatrix
		{
			get
			{
				return boneMatrix;
			}
		}

		public virtual void setBoneMatrix(int count, float[] boneMatrix)
		{
			if (count > 0)
			{
				Array.Copy(boneMatrix, 0, this.boneMatrix, 0, 16 * count);
			}
		}

		public virtual int NumberBones
		{
			get
			{
				return numberBones;
			}
			set
			{
				this.numberBones = value;
			}
		}


		public virtual int TexEnable
		{
			get
			{
				return texEnable;
			}
			set
			{
				this.texEnable = value;
			}
		}


		public virtual int TexMapMode
		{
			get
			{
				return texMapMode;
			}
			set
			{
				this.texMapMode = value;
			}
		}


		public virtual int TexMapProj
		{
			get
			{
				return texMapProj;
			}
			set
			{
				this.texMapProj = value;
			}
		}


		public virtual int getTexShade(int index)
		{
			return texShade[index];
		}

		public virtual void setTexShade(int index, int texShade)
		{
			this.texShade[index] = texShade;
		}

		public virtual int CtestEnable
		{
			get
			{
				return ctestEnable;
			}
			set
			{
				this.ctestEnable = value;
			}
		}


		public virtual int CtestFunc
		{
			get
			{
				return ctestFunc;
			}
			set
			{
				this.ctestFunc = value;
			}
		}


		public virtual int getCtestMsk(int index)
		{
			return ctestMsk[index];
		}

		public virtual void setCtestMsk(int index, int ctestMsk)
		{
			this.ctestMsk[index] = ctestMsk;
		}

		public virtual int getCtestRef(int index)
		{
			return ctestRef[index];
		}

		public virtual void setCtestRef(int index, int ctestRef)
		{
			this.ctestRef[index] = ctestRef;
		}

		public virtual int getTexEnvMode(int index)
		{
			return texEnvMode[index];
		}

		public virtual void setTexEnvMode(int index, int texEnvMode)
		{
			this.texEnvMode[index] = texEnvMode;
		}

		public virtual float ColorDoubling
		{
			get
			{
				return colorDoubling;
			}
			set
			{
				this.colorDoubling = value;
			}
		}


		public virtual int VinfoColor
		{
			get
			{
				return vinfoColor;
			}
			set
			{
				this.vinfoColor = value;
			}
		}


		public virtual int VinfoPosition
		{
			get
			{
				return vinfoPosition;
			}
			set
			{
				this.vinfoPosition = value;
			}
		}


		public virtual int VinfoTransform2D
		{
			get
			{
				return vinfoTransform2D;
			}
			set
			{
				this.vinfoTransform2D = value;
			}
		}


		public virtual float PositionScale
		{
			get
			{
				return positionScale;
			}
			set
			{
				this.positionScale = value;
			}
		}


		public virtual float NormalScale
		{
			get
			{
				return normalScale;
			}
			set
			{
				this.normalScale = value;
			}
		}


		public virtual float TextureScale
		{
			get
			{
				return textureScale;
			}
			set
			{
				this.textureScale = value;
			}
		}


		public virtual float WeightScale
		{
			get
			{
				return weightScale;
			}
			set
			{
				this.weightScale = value;
			}
		}


		public virtual int ClutShift
		{
			get
			{
				return clutShift;
			}
			set
			{
				this.clutShift = value;
			}
		}


		public virtual int ClutMask
		{
			get
			{
				return clutMask;
			}
			set
			{
				this.clutMask = value;
			}
		}


		public virtual int ClutOffset
		{
			get
			{
				return clutOffset;
			}
			set
			{
				this.clutOffset = value;
			}
		}


		public virtual bool MipmapShareClut
		{
			get
			{
				return mipmapShareClut;
			}
			set
			{
				this.mipmapShareClut = value;
			}
		}


		public virtual int Clut
		{
			get
			{
				return clut;
			}
			set
			{
				this.clut = value;
			}
		}


		public virtual int TexPixelFormat
		{
			get
			{
				return texPixelFormat;
			}
			set
			{
				this.texPixelFormat = value;
			}
		}


		public virtual int Tex
		{
			get
			{
				return tex;
			}
			set
			{
				this.tex = value;
			}
		}


		public virtual int Utex
		{
			get
			{
				return utex;
			}
			set
			{
				this.utex = value;
			}
		}


		public virtual float[] VertexColor
		{
			get
			{
				return vertexColor;
			}
			set
			{
				this.vertexColor[0] = value[0];
				this.vertexColor[1] = value[1];
				this.vertexColor[2] = value[2];
				this.vertexColor[3] = value[3];
			}
		}


		public virtual int ClutIndexHint
		{
			get
			{
				return clutIndexHint;
			}
			set
			{
				this.clutIndexHint = value;
			}
		}


		public virtual int VinfoTexture
		{
			get
			{
				return vinfoTexture;
			}
			set
			{
				this.vinfoTexture = value;
			}
		}


		public virtual int VinfoNormal
		{
			get
			{
				return vinfoNormal;
			}
			set
			{
				this.vinfoNormal = value;
			}
		}


		public virtual int StencilTestEnable
		{
			get
			{
				return stencilTestEnable;
			}
			set
			{
				this.stencilTestEnable = value;
			}
		}


		public virtual int StencilFunc
		{
			get
			{
				return stencilFunc;
			}
			set
			{
				this.stencilFunc = value;
			}
		}


		public virtual int StencilRef
		{
			get
			{
				return stencilRef;
			}
			set
			{
				this.stencilRef = value;
			}
		}


		public virtual int StencilMask
		{
			get
			{
				return stencilMask;
			}
			set
			{
				this.stencilMask = value;
			}
		}


		public virtual int StencilOpFail
		{
			get
			{
				return stencilOpFail;
			}
			set
			{
				this.stencilOpFail = value;
			}
		}


		public virtual int StencilOpZFail
		{
			get
			{
				return stencilOpZFail;
			}
			set
			{
				this.stencilOpZFail = value;
			}
		}


		public virtual int StencilOpZPass
		{
			get
			{
				return stencilOpZPass;
			}
			set
			{
				this.stencilOpZPass = value;
			}
		}


		public virtual int DepthTestEnable
		{
			get
			{
				return depthTestEnable;
			}
			set
			{
				this.depthTestEnable = value;
			}
		}


		public virtual int DepthFunc
		{
			get
			{
				return depthFunc;
			}
			set
			{
				this.depthFunc = value;
			}
		}


		public virtual int DepthMask
		{
			get
			{
				return depthMask;
			}
			set
			{
				this.depthMask = value;
			}
		}


		public virtual int FbTex
		{
			get
			{
				return fbTex;
			}
			set
			{
				this.fbTex = value;
			}
		}


		public virtual int ColorMaskEnable
		{
			get
			{
				return colorMaskEnable;
			}
			set
			{
				this.colorMaskEnable = value;
			}
		}


		public virtual int[] ColorMask
		{
			get
			{
				return colorMask;
			}
		}

		public virtual void setColorMask(int redMask, int greenMask, int blueMask, int alphaMask)
		{
			this.colorMask[0] = redMask;
			this.colorMask[1] = greenMask;
			this.colorMask[2] = blueMask;
			this.colorMask[3] = alphaMask;
		}

		public virtual int[] NotColorMask
		{
			get
			{
				return notColorMask;
			}
		}

		public virtual void setNotColorMask(int notRedMask, int notGreenMask, int notBlueMask, int notAlphaMask)
		{
			this.notColorMask[0] = notRedMask;
			this.notColorMask[1] = notGreenMask;
			this.notColorMask[2] = notBlueMask;
			this.notColorMask[3] = notAlphaMask;
		}

		public virtual int AlphaTestEnable
		{
			get
			{
				return alphaTestEnable;
			}
			set
			{
				this.alphaTestEnable = value;
			}
		}


		public virtual int AlphaTestFunc
		{
			get
			{
				return alphaTestFunc;
			}
			set
			{
				this.alphaTestFunc = value;
			}
		}


		public virtual int AlphaTestRef
		{
			get
			{
				return alphaTestRef;
			}
			set
			{
				this.alphaTestRef = value;
			}
		}


		public virtual int AlphaTestMask
		{
			get
			{
				return alphaTestMask;
			}
			set
			{
				this.alphaTestMask = value;
			}
		}


		public virtual int BlendTestEnable
		{
			get
			{
				return blendTestEnable;
			}
			set
			{
				this.blendTestEnable = value;
			}
		}


		public virtual int BlendEquation
		{
			get
			{
				return blendEquation;
			}
			set
			{
				this.blendEquation = value;
			}
		}


		public virtual int BlendSrc
		{
			get
			{
				return blendSrc;
			}
			set
			{
				this.blendSrc = value;
			}
		}


		public virtual int BlendDst
		{
			get
			{
				return blendDst;
			}
			set
			{
				this.blendDst = value;
			}
		}


		public virtual float[] BlendSFix
		{
			get
			{
				return blendSFix;
			}
			set
			{
				this.blendSFix[0] = value[0];
				this.blendSFix[1] = value[1];
				this.blendSFix[2] = value[2];
			}
		}


		public virtual float[] BlendDFix
		{
			get
			{
				return blendDFix;
			}
			set
			{
				this.blendDFix[0] = value[0];
				this.blendDFix[1] = value[1];
				this.blendDFix[2] = value[2];
			}
		}


		public virtual int CopyRedToAlpha
		{
			get
			{
				return copyRedToAlpha;
			}
			set
			{
				this.copyRedToAlpha = value;
			}
		}


		public virtual int WrapModeS
		{
			get
			{
				return wrapModeS;
			}
			set
			{
				this.wrapModeS = value;
			}
		}


		public virtual int WrapModeT
		{
			get
			{
				return wrapModeT;
			}
			set
			{
				this.wrapModeT = value;
			}
		}


		public virtual int FogEnable
		{
			get
			{
				return fogEnable;
			}
			set
			{
				this.fogEnable = value;
			}
		}


		public virtual float[] FogColor
		{
			get
			{
				return fogColor;
			}
			set
			{
				this.fogColor[0] = value[0];
				this.fogColor[1] = value[1];
				this.fogColor[2] = value[2];
			}
		}


		public virtual float FogEnd
		{
			get
			{
				return fogEnd;
			}
			set
			{
				this.fogEnd = value;
			}
		}


		public virtual float FogScale
		{
			get
			{
				return fogScale;
			}
			set
			{
				this.fogScale = value;
			}
		}


		public virtual int ClipPlaneEnable
		{
			get
			{
				return clipPlaneEnable;
			}
			set
			{
				this.clipPlaneEnable = value;
			}
		}


		public virtual float[] ViewportPos
		{
			get
			{
				return viewportPos;
			}
		}

		public virtual void setViewportPos(float x, float y, float z)
		{
			this.viewportPos[0] = x;
			this.viewportPos[1] = y;
			this.viewportPos[2] = z;
		}

		public virtual float[] ViewportScale
		{
			get
			{
				return viewportScale;
			}
		}

		public virtual void setViewportScale(float sx, float sy, float sz)
		{
			this.viewportScale[0] = sx;
			this.viewportScale[1] = sy;
			this.viewportScale[2] = sz;
		}
	}
}