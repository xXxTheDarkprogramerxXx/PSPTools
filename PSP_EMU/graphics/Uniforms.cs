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
namespace pspsharp.graphics
{

	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;
	using StateProxy = pspsharp.graphics.RE.StateProxy;

	public sealed class Uniforms
	{
		public static readonly Uniforms zPos = new Uniforms("zPos", InnerEnum.zPos, "psp_zPos");
		public static readonly Uniforms zScale = new Uniforms("zScale", InnerEnum.zScale, "psp_zScale");
		public static readonly Uniforms lightingEnable = new Uniforms("lightingEnable", InnerEnum.lightingEnable, "lightingEnable");
		public static readonly Uniforms lightEnabled = new Uniforms("lightEnabled", InnerEnum.lightEnabled, "psp_lightEnabled");
		public static readonly Uniforms lightType = new Uniforms("lightType", InnerEnum.lightType, "psp_lightType");
		public static readonly Uniforms lightKind = new Uniforms("lightKind", InnerEnum.lightKind, "psp_lightKind");
		public static readonly Uniforms lightMode = new Uniforms("lightMode", InnerEnum.lightMode, "colorAddition");
		public static readonly Uniforms matFlags = new Uniforms("matFlags", InnerEnum.matFlags, "psp_matFlags");
		public static readonly Uniforms tex = new Uniforms("tex", InnerEnum.tex, "tex");
		public static readonly Uniforms texEnable = new Uniforms("texEnable", InnerEnum.texEnable, "texEnable");
		public static readonly Uniforms texEnvMode = new Uniforms("texEnvMode", InnerEnum.texEnvMode, "texEnvMode");
		public static readonly Uniforms texMapMode = new Uniforms("texMapMode", InnerEnum.texMapMode, "texMapMode");
		public static readonly Uniforms texMapProj = new Uniforms("texMapProj", InnerEnum.texMapProj, "texMapProj");
		public static readonly Uniforms texShade = new Uniforms("texShade", InnerEnum.texShade, "texShade");
		public static readonly Uniforms colorDoubling = new Uniforms("colorDoubling", InnerEnum.colorDoubling, "colorDoubling");
		public static readonly Uniforms ctestEnable = new Uniforms("ctestEnable", InnerEnum.ctestEnable, "ctestEnable");
		public static readonly Uniforms ctestFunc = new Uniforms("ctestFunc", InnerEnum.ctestFunc, "ctestFunc");
		public static readonly Uniforms ctestRef = new Uniforms("ctestRef", InnerEnum.ctestRef, "ctestRef");
		public static readonly Uniforms ctestMsk = new Uniforms("ctestMsk", InnerEnum.ctestMsk, "ctestMsk");
		public static readonly Uniforms boneMatrix = new Uniforms("boneMatrix", InnerEnum.boneMatrix, "psp_boneMatrix");
		public static readonly Uniforms weights = new Uniforms("weights", InnerEnum.weights, "psp_weights");
		public static readonly Uniforms numberBones = new Uniforms("numberBones", InnerEnum.numberBones, "psp_numberBones");
		public static readonly Uniforms vinfoColor = new Uniforms("vinfoColor", InnerEnum.vinfoColor, "vinfoColor");
		public static readonly Uniforms vinfoPosition = new Uniforms("vinfoPosition", InnerEnum.vinfoPosition, "vinfoPosition");
		public static readonly Uniforms vinfoTransform2D = new Uniforms("vinfoTransform2D", InnerEnum.vinfoTransform2D, "vinfoTransform2D");
		public static readonly Uniforms positionScale = new Uniforms("positionScale", InnerEnum.positionScale, "positionScale");
		public static readonly Uniforms normalScale = new Uniforms("normalScale", InnerEnum.normalScale, "normalScale");
		public static readonly Uniforms textureScale = new Uniforms("textureScale", InnerEnum.textureScale, "textureScale");
		public static readonly Uniforms weightScale = new Uniforms("weightScale", InnerEnum.weightScale, "weightScale");
		public static readonly Uniforms clutShift = new Uniforms("clutShift", InnerEnum.clutShift, "clutShift");
		public static readonly Uniforms clutMask = new Uniforms("clutMask", InnerEnum.clutMask, "clutMask");
		public static readonly Uniforms clutOffset = new Uniforms("clutOffset", InnerEnum.clutOffset, "clutOffset");
		public static readonly Uniforms clut = new Uniforms("clut", InnerEnum.clut, "clut");
		public static readonly Uniforms mipmapShareClut = new Uniforms("mipmapShareClut", InnerEnum.mipmapShareClut, "mipmapShareClut");
		public static readonly Uniforms texPixelFormat = new Uniforms("texPixelFormat", InnerEnum.texPixelFormat, "texPixelFormat");
		public static readonly Uniforms utex = new Uniforms("utex", InnerEnum.utex, "utex");
		public static readonly Uniforms endOfUBO = new Uniforms("endOfUBO", InnerEnum.endOfUBO, "endOfUBO");
		public static readonly Uniforms vertexColor = new Uniforms("vertexColor", InnerEnum.vertexColor, "vertexColor");
		public static readonly Uniforms vinfoTexture = new Uniforms("vinfoTexture", InnerEnum.vinfoTexture, "vinfoTexture");
		public static readonly Uniforms vinfoNormal = new Uniforms("vinfoNormal", InnerEnum.vinfoNormal, "vinfoNormal");
		public static readonly Uniforms stencilTestEnable = new Uniforms("stencilTestEnable", InnerEnum.stencilTestEnable, "stencilTestEnable");
		public static readonly Uniforms stencilFunc = new Uniforms("stencilFunc", InnerEnum.stencilFunc, "stencilFunc");
		public static readonly Uniforms stencilRef = new Uniforms("stencilRef", InnerEnum.stencilRef, "stencilRef");
		public static readonly Uniforms stencilMask = new Uniforms("stencilMask", InnerEnum.stencilMask, "stencilMask");
		public static readonly Uniforms stencilOpFail = new Uniforms("stencilOpFail", InnerEnum.stencilOpFail, "stencilOpFail");
		public static readonly Uniforms stencilOpZFail = new Uniforms("stencilOpZFail", InnerEnum.stencilOpZFail, "stencilOpZFail");
		public static readonly Uniforms stencilOpZPass = new Uniforms("stencilOpZPass", InnerEnum.stencilOpZPass, "stencilOpZPass");
		public static readonly Uniforms depthTestEnable = new Uniforms("depthTestEnable", InnerEnum.depthTestEnable, "depthTestEnable");
		public static readonly Uniforms depthFunc = new Uniforms("depthFunc", InnerEnum.depthFunc, "depthFunc");
		public static readonly Uniforms depthMask = new Uniforms("depthMask", InnerEnum.depthMask, "depthMask");
		public static readonly Uniforms fbTex = new Uniforms("fbTex", InnerEnum.fbTex, "fbTex");
		public static readonly Uniforms colorMaskEnable = new Uniforms("colorMaskEnable", InnerEnum.colorMaskEnable, "colorMaskEnable");
		public static readonly Uniforms colorMask = new Uniforms("colorMask", InnerEnum.colorMask, "colorMask");
		public static readonly Uniforms notColorMask = new Uniforms("notColorMask", InnerEnum.notColorMask, "notColorMask");
		public static readonly Uniforms alphaTestEnable = new Uniforms("alphaTestEnable", InnerEnum.alphaTestEnable, "alphaTestEnable");
		public static readonly Uniforms alphaTestFunc = new Uniforms("alphaTestFunc", InnerEnum.alphaTestFunc, "alphaTestFunc");
		public static readonly Uniforms alphaTestRef = new Uniforms("alphaTestRef", InnerEnum.alphaTestRef, "alphaTestRef");
		public static readonly Uniforms alphaTestMask = new Uniforms("alphaTestMask", InnerEnum.alphaTestMask, "alphaTestMask");
		public static readonly Uniforms blendTestEnable = new Uniforms("blendTestEnable", InnerEnum.blendTestEnable, "blendTestEnable");
		public static readonly Uniforms blendEquation = new Uniforms("blendEquation", InnerEnum.blendEquation, "blendEquation");
		public static readonly Uniforms blendSrc = new Uniforms("blendSrc", InnerEnum.blendSrc, "blendSrc");
		public static readonly Uniforms blendDst = new Uniforms("blendDst", InnerEnum.blendDst, "blendDst");
		public static readonly Uniforms blendSFix = new Uniforms("blendSFix", InnerEnum.blendSFix, "blendSFix");
		public static readonly Uniforms blendDFix = new Uniforms("blendDFix", InnerEnum.blendDFix, "blendDFix");
		public static readonly Uniforms copyRedToAlpha = new Uniforms("copyRedToAlpha", InnerEnum.copyRedToAlpha, "copyRedToAlpha");
		public static readonly Uniforms wrapModeS = new Uniforms("wrapModeS", InnerEnum.wrapModeS, "wrapModeS");
		public static readonly Uniforms wrapModeT = new Uniforms("wrapModeT", InnerEnum.wrapModeT, "wrapModeT");
		public static readonly Uniforms fogEnable = new Uniforms("fogEnable", InnerEnum.fogEnable, "fogEnable");
		public static readonly Uniforms fogColor = new Uniforms("fogColor", InnerEnum.fogColor, "fogColor");
		public static readonly Uniforms fogEnd = new Uniforms("fogEnd", InnerEnum.fogEnd, "fogEnd");
		public static readonly Uniforms fogScale = new Uniforms("fogScale", InnerEnum.fogScale, "fogScale");
		public static readonly Uniforms clipPlaneEnable = new Uniforms("clipPlaneEnable", InnerEnum.clipPlaneEnable, "clipPlaneEnable");
		public static readonly Uniforms viewportPos = new Uniforms("viewportPos", InnerEnum.viewportPos, "viewportPos");
		public static readonly Uniforms viewportScale = new Uniforms("viewportScale", InnerEnum.viewportScale, "viewportScale");

		private static readonly IList<Uniforms> valueList = new List<Uniforms>();

		static Uniforms()
		{
			valueList.Add(zPos);
			valueList.Add(zScale);
			valueList.Add(lightingEnable);
			valueList.Add(lightEnabled);
			valueList.Add(lightType);
			valueList.Add(lightKind);
			valueList.Add(lightMode);
			valueList.Add(matFlags);
			valueList.Add(tex);
			valueList.Add(texEnable);
			valueList.Add(texEnvMode);
			valueList.Add(texMapMode);
			valueList.Add(texMapProj);
			valueList.Add(texShade);
			valueList.Add(colorDoubling);
			valueList.Add(ctestEnable);
			valueList.Add(ctestFunc);
			valueList.Add(ctestRef);
			valueList.Add(ctestMsk);
			valueList.Add(boneMatrix);
			valueList.Add(weights);
			valueList.Add(numberBones);
			valueList.Add(vinfoColor);
			valueList.Add(vinfoPosition);
			valueList.Add(vinfoTransform2D);
			valueList.Add(positionScale);
			valueList.Add(normalScale);
			valueList.Add(textureScale);
			valueList.Add(weightScale);
			valueList.Add(clutShift);
			valueList.Add(clutMask);
			valueList.Add(clutOffset);
			valueList.Add(clut);
			valueList.Add(mipmapShareClut);
			valueList.Add(texPixelFormat);
			valueList.Add(utex);
			valueList.Add(endOfUBO);
			valueList.Add(vertexColor);
			valueList.Add(vinfoTexture);
			valueList.Add(vinfoNormal);
			valueList.Add(stencilTestEnable);
			valueList.Add(stencilFunc);
			valueList.Add(stencilRef);
			valueList.Add(stencilMask);
			valueList.Add(stencilOpFail);
			valueList.Add(stencilOpZFail);
			valueList.Add(stencilOpZPass);
			valueList.Add(depthTestEnable);
			valueList.Add(depthFunc);
			valueList.Add(depthMask);
			valueList.Add(fbTex);
			valueList.Add(colorMaskEnable);
			valueList.Add(colorMask);
			valueList.Add(notColorMask);
			valueList.Add(alphaTestEnable);
			valueList.Add(alphaTestFunc);
			valueList.Add(alphaTestRef);
			valueList.Add(alphaTestMask);
			valueList.Add(blendTestEnable);
			valueList.Add(blendEquation);
			valueList.Add(blendSrc);
			valueList.Add(blendDst);
			valueList.Add(blendSFix);
			valueList.Add(blendDFix);
			valueList.Add(copyRedToAlpha);
			valueList.Add(wrapModeS);
			valueList.Add(wrapModeT);
			valueList.Add(fogEnable);
			valueList.Add(fogColor);
			valueList.Add(fogEnd);
			valueList.Add(fogScale);
			valueList.Add(clipPlaneEnable);
			valueList.Add(viewportPos);
			valueList.Add(viewportScale);
		}

		public enum InnerEnum
		{
			zPos,
			zScale,
			lightingEnable,
			lightEnabled,
			lightType,
			lightKind,
			lightMode,
			matFlags,
			tex,
			texEnable,
			texEnvMode,
			texMapMode,
			texMapProj,
			texShade,
			colorDoubling,
			ctestEnable,
			ctestFunc,
			ctestRef,
			ctestMsk,
			boneMatrix,
			weights,
			numberBones,
			vinfoColor,
			vinfoPosition,
			vinfoTransform2D,
			positionScale,
			normalScale,
			textureScale,
			weightScale,
			clutShift,
			clutMask,
			clutOffset,
			clut,
			mipmapShareClut,
			texPixelFormat,
			utex,
			endOfUBO,
			vertexColor,
			vinfoTexture,
			vinfoNormal,
			stencilTestEnable,
			stencilFunc,
			stencilRef,
			stencilMask,
			stencilOpFail,
			stencilOpZFail,
			stencilOpZPass,
			depthTestEnable,
			depthFunc,
			depthMask,
			fbTex,
			colorMaskEnable,
			colorMask,
			notColorMask,
			alphaTestEnable,
			alphaTestFunc,
			alphaTestRef,
			alphaTestMask,
			blendTestEnable,
			blendEquation,
			blendSrc,
			blendDst,
			blendSFix,
			blendDFix,
			copyRedToAlpha,
			wrapModeS,
			wrapModeT,
			fogEnable,
			fogColor,
			fogEnd,
			fogScale,
			clipPlaneEnable,
			viewportPos,
			viewportScale
		}

		public readonly InnerEnum innerEnumValue;
		private readonly string nameValue;
		private readonly int ordinalValue;
		private static int nextOrdinal = 0;

		internal string uniformString;
		internal int[] uniformId = new int[pspsharp.graphics.RE.StateProxy.maxProgramId];

		private Uniforms(string name, InnerEnum innerEnum, string uniformString)
		{
			Arrays.Fill(uniformId, -1);
			this.uniformString = uniformString;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public int getId(int shaderProgram)
		{
			return uniformId[shaderProgram];
		}

		public string UniformString
		{
			get
			{
				return uniformString;
			}
		}

		public void allocateId(pspsharp.graphics.RE.IRenderingEngine re, int shaderProgram)
		{
			uniformId[shaderProgram] = re.getUniformLocation(shaderProgram, uniformString);
		}

		public static IList<Uniforms> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static Uniforms valueOf(string name)
		{
			foreach (Uniforms enumInstance in Uniforms.valueList)
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}
}