using System.Collections.Generic;
using System.Text;

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
//	import static pspsharp.graphics.VideoEngine.SIZEOF_FLOAT;


	using Settings = pspsharp.settings.Settings;

	/// <summary>
	/// @author gid15
	/// 
	/// Implementation of the ShaderContext using Uniform Buffer Object (UBO)
	/// to allow a faster shader program switch.
	/// </summary>
	public class ShaderContextUBO : ShaderContext
	{
		private ShaderUniformInfo lightType;
		private ShaderUniformInfo lightKind;
		private ShaderUniformInfo lightEnabled;
		private ShaderUniformInfo vertexColor;
		private ShaderUniformInfo colorMask;
		private ShaderUniformInfo notColorMask;
		private ShaderUniformInfo matFlags;
		private ShaderUniformInfo ctestRef;
		private ShaderUniformInfo ctestMsk;
		private ShaderUniformInfo texShade;
		private ShaderUniformInfo texEnvMode;
		private ShaderUniformInfo ctestFunc;
		private ShaderUniformInfo texMapMode;
		private ShaderUniformInfo texMapProj;
		private ShaderUniformInfo vinfoColor;
		private ShaderUniformInfo vinfoPosition;
		private ShaderUniformInfo vinfoTexture;
		private ShaderUniformInfo vinfoNormal;
		private ShaderUniformInfo positionScale;
		private ShaderUniformInfo normalScale;
		private ShaderUniformInfo textureScale;
		private ShaderUniformInfo weightScale;
		private ShaderUniformInfo colorDoubling;
		private ShaderUniformInfo texEnable;
		private ShaderUniformInfo lightingEnable;
		private ShaderUniformInfo vinfoTransform2D;
		private ShaderUniformInfo ctestEnable;
		private ShaderUniformInfo lightMode;
		private ShaderUniformInfo clutShift;
		private ShaderUniformInfo clutMask;
		private ShaderUniformInfo clutOffset;
		private ShaderUniformInfo mipmapShareClut;
		private ShaderUniformInfo texPixelFormat;
		private ShaderUniformInfo stencilTestEnable;
		private ShaderUniformInfo stencilFunc;
		private ShaderUniformInfo stencilRef;
		private ShaderUniformInfo stencilMask;
		private ShaderUniformInfo stencilOpFail;
		private ShaderUniformInfo stencilOpZFail;
		private ShaderUniformInfo stencilOpZPass;
		private ShaderUniformInfo depthTestEnable;
		private ShaderUniformInfo depthFunc;
		private ShaderUniformInfo depthMask;
		private ShaderUniformInfo alphaTestEnable;
		private ShaderUniformInfo alphaTestFunc;
		private ShaderUniformInfo alphaTestRef;
		private ShaderUniformInfo alphaTestMask;
		private ShaderUniformInfo blendTestEnable;
		private ShaderUniformInfo blendEquation;
		private ShaderUniformInfo blendSrc;
		private ShaderUniformInfo blendDst;
		private ShaderUniformInfo blendSFix;
		private ShaderUniformInfo blendDFix;
		private ShaderUniformInfo colorMaskEnable;
		private ShaderUniformInfo wrapModeS;
		private ShaderUniformInfo wrapModeT;
		private ShaderUniformInfo copyRedToAlpha;
		private ShaderUniformInfo fogEnable;
		private ShaderUniformInfo fogColor;
		private ShaderUniformInfo fogEnd;
		private ShaderUniformInfo fogScale;
		private ShaderUniformInfo clipPlaneEnable;
		private ShaderUniformInfo viewportPos;
		private ShaderUniformInfo viewportScale;
		private ShaderUniformInfo numberBones;
		private ShaderUniformInfo boneMatrix;
		private ShaderUniformInfo endOfUBO;
		private int bufferSize;
		protected internal const int bindingPoint = 1;
		protected internal const string uniformBlockName = "psp";
		protected internal const string uniformMemoryLayout = "std140";
		protected internal int buffer;
		protected internal ByteBuffer data;
		private int startUpdate;
		private int endUpdate;
		private string shaderUniformText;
		private List<ShaderUniformInfo> shaderUniformInfos;

		private class ShaderUniformInfo
		{
			internal string name;
			internal string structureName;
			internal string type;
			internal int offset;
			internal int matrixSize;
			internal bool used;

			public ShaderUniformInfo(Uniforms uniform, string type)
			{
				name = uniform.UniformString;
				structureName = this.name;
				this.type = type;
				used = true;
				matrixSize = 0;
			}

			public ShaderUniformInfo(Uniforms uniform, string type, int matrixSize)
			{
				name = uniform.UniformString;
				structureName = string.Format("{0}[{1:D}]", name, matrixSize);
				this.type = type;
				this.matrixSize = matrixSize;
				used = true;
			}

			public virtual string Name
			{
				get
				{
					return name;
				}
			}

			public virtual string StructureName
			{
				get
				{
					return structureName;
				}
			}

			public virtual int Offset
			{
				get
				{
					return offset;
				}
				set
				{
					this.offset = value;
				}
			}


			internal virtual string Type
			{
				get
				{
					return type;
				}
			}

			public virtual bool Used
			{
				get
				{
					return used;
				}
			}

			public virtual void setUnused()
			{
				used = false;
			}

			public virtual int MatrixSize
			{
				get
				{
					return matrixSize;
				}
			}

			public override string ToString()
			{
				if (!Used)
				{
					return string.Format("{0}(unused)", Name);
				}
				return string.Format("{0}(offset={1:D})", Name, Offset);
			}
		}

		public static bool useUBO(IRenderingEngine re)
		{
			return !Settings.Instance.readBool("emu.disableubo") && re.isExtensionAvailable("GL_ARB_uniform_buffer_object");
		}

		public ShaderContextUBO(IRenderingEngine re)
		{
			shaderUniformInfos = new List<ShaderUniformInfo>();

			// Add all the shader uniform objects
			// in the order they have to be defined in the shader structure
			lightType = addShaderUniform(Uniforms.lightType, "ivec4");
			lightKind = addShaderUniform(Uniforms.lightKind, "ivec4");
			lightEnabled = addShaderUniform(Uniforms.lightEnabled, "ivec4");
			vertexColor = addShaderUniform(Uniforms.vertexColor, "vec4");
			colorMask = addShaderUniform(Uniforms.colorMask, "ivec4");
			notColorMask = addShaderUniform(Uniforms.notColorMask, "ivec4");
			blendSFix = addShaderUniform(Uniforms.blendSFix, "vec3");
			blendDFix = addShaderUniform(Uniforms.blendDFix, "vec3");
			matFlags = addShaderUniform(Uniforms.matFlags, "ivec3");
			ctestRef = addShaderUniform(Uniforms.ctestRef, "ivec3");
			ctestMsk = addShaderUniform(Uniforms.ctestMsk, "ivec3");
			texShade = addShaderUniform(Uniforms.texShade, "ivec2");
			texEnvMode = addShaderUniform(Uniforms.texEnvMode, "ivec2");
			ctestFunc = addShaderUniform(Uniforms.ctestFunc, "int");
			texMapMode = addShaderUniform(Uniforms.texMapMode, "int");
			texMapProj = addShaderUniform(Uniforms.texMapProj, "int");
			vinfoColor = addShaderUniform(Uniforms.vinfoColor, "int");
			vinfoPosition = addShaderUniform(Uniforms.vinfoPosition, "int");
			vinfoTexture = addShaderUniform(Uniforms.vinfoTexture, "int");
			vinfoNormal = addShaderUniform(Uniforms.vinfoNormal, "int");
			positionScale = addShaderUniform(Uniforms.positionScale, "float");
			normalScale = addShaderUniform(Uniforms.normalScale, "float");
			textureScale = addShaderUniform(Uniforms.textureScale, "float");
			weightScale = addShaderUniform(Uniforms.weightScale, "float");
			colorDoubling = addShaderUniform(Uniforms.colorDoubling, "float");
			texEnable = addShaderUniform(Uniforms.texEnable, "bool");
			lightingEnable = addShaderUniform(Uniforms.lightingEnable, "bool");
			vinfoTransform2D = addShaderUniform(Uniforms.vinfoTransform2D, "bool");
			ctestEnable = addShaderUniform(Uniforms.ctestEnable, "bool");
			lightMode = addShaderUniform(Uniforms.lightMode, "bool");
			clutShift = addShaderUniform(Uniforms.clutShift, "int");
			clutMask = addShaderUniform(Uniforms.clutMask, "int");
			clutOffset = addShaderUniform(Uniforms.clutOffset, "int");
			mipmapShareClut = addShaderUniform(Uniforms.mipmapShareClut, "bool");
			texPixelFormat = addShaderUniform(Uniforms.texPixelFormat, "int");
			stencilTestEnable = addShaderUniform(Uniforms.stencilTestEnable, "bool");
			stencilFunc = addShaderUniform(Uniforms.stencilFunc, "int");
			stencilRef = addShaderUniform(Uniforms.stencilRef, "int");
			stencilMask = addShaderUniform(Uniforms.stencilMask, "int");
			stencilOpFail = addShaderUniform(Uniforms.stencilOpFail, "int");
			stencilOpZFail = addShaderUniform(Uniforms.stencilOpZFail, "int");
			stencilOpZPass = addShaderUniform(Uniforms.stencilOpZPass, "int");
			depthTestEnable = addShaderUniform(Uniforms.depthTestEnable, "bool");
			depthFunc = addShaderUniform(Uniforms.depthFunc, "int");
			depthMask = addShaderUniform(Uniforms.depthMask, "int");
			alphaTestEnable = addShaderUniform(Uniforms.alphaTestEnable, "bool");
			alphaTestFunc = addShaderUniform(Uniforms.alphaTestFunc, "int");
			alphaTestRef = addShaderUniform(Uniforms.alphaTestRef, "int");
			alphaTestMask = addShaderUniform(Uniforms.alphaTestMask, "int");
			blendTestEnable = addShaderUniform(Uniforms.blendTestEnable, "bool");
			blendEquation = addShaderUniform(Uniforms.blendEquation, "int");
			blendSrc = addShaderUniform(Uniforms.blendSrc, "int");
			blendDst = addShaderUniform(Uniforms.blendDst, "int");
			colorMaskEnable = addShaderUniform(Uniforms.colorMaskEnable, "bool");
			wrapModeS = addShaderUniform(Uniforms.wrapModeS, "int");
			wrapModeT = addShaderUniform(Uniforms.wrapModeT, "int");
			copyRedToAlpha = addShaderUniform(Uniforms.copyRedToAlpha, "bool");
			fogEnable = addShaderUniform(Uniforms.fogEnable, "bool");
			fogColor = addShaderUniform(Uniforms.fogColor, "vec3");
			fogEnd = addShaderUniform(Uniforms.fogEnd, "float");
			fogScale = addShaderUniform(Uniforms.fogScale, "float");
			clipPlaneEnable = addShaderUniform(Uniforms.clipPlaneEnable, "bool");
			viewportPos = addShaderUniform(Uniforms.viewportPos, "vec3");
			viewportScale = addShaderUniform(Uniforms.viewportScale, "vec3");
			numberBones = addShaderUniform(Uniforms.numberBones, "int");
			boneMatrix = addShaderUniform(Uniforms.boneMatrix, "mat4", 8);
			// The following entry has always to be the last one
			endOfUBO = addShaderUniform(Uniforms.endOfUBO, "int");

			StringBuilder s = new StringBuilder();
			s.Append(string.Format("layout({0}) uniform {1}\n", uniformMemoryLayout, uniformBlockName));
			s.Append(string.Format("{\n"));
			foreach (ShaderUniformInfo shaderUniformInfo in shaderUniformInfos)
			{
				s.Append(string.Format("   {0} {1};\n", shaderUniformInfo.Type, shaderUniformInfo.StructureName));
			}
			s.Append(string.Format("};\n"));

			shaderUniformText = s.ToString();
		}

		protected internal virtual ShaderUniformInfo addShaderUniform(Uniforms uniform, string type)
		{
			ShaderUniformInfo shaderUniformInfo = new ShaderUniformInfo(uniform, type);
			shaderUniformInfos.Add(shaderUniformInfo);

			return shaderUniformInfo;
		}

		protected internal virtual ShaderUniformInfo addShaderUniform(Uniforms uniform, string type, int matrixSize)
		{
			ShaderUniformInfo shaderUniformInfo = new ShaderUniformInfo(uniform, type, matrixSize);
			shaderUniformInfos.Add(shaderUniformInfo);

			return shaderUniformInfo;
		}

		public virtual string ShaderUniformText
		{
			get
			{
				return shaderUniformText;
			}
		}

		public override void initShaderProgram(IRenderingEngine re, int shaderProgram)
		{
			int blockIndex = re.getUniformBlockIndex(shaderProgram, uniformBlockName);
			// The uniform block might have been eliminated by the shader compiler
			// if it was not used at all.
			if (blockIndex >= 0)
			{
				re.setUniformBlockBinding(shaderProgram, blockIndex, bindingPoint);
			}

			if (data == null)
			{
				int previousOffset = -1;
				foreach (ShaderUniformInfo shaderUniformInfo in shaderUniformInfos)
				{
					int index = re.getUniformIndex(shaderProgram, shaderUniformInfo.Name);
					int offset = re.getActiveUniformOffset(shaderProgram, index);

					// Nvidia workaround: the offset of the first uniform is returned as 1 instead of 0.
					if (offset == 1)
					{
						offset = 0;
					}

					shaderUniformInfo.Offset = offset;

					// An unused uniform has the same offset as its previous uniform.
					// An unused uniform should not be copied into the UBO buffer,
					// otherwise it would overwrite the previous uniform value.
					if (offset < 0 || offset == previousOffset)
					{
						shaderUniformInfo.setUnused();
					}

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Uniform {0}", shaderUniformInfo));
					}

					previousOffset = offset;
				}

				// The size returned by
				//    glGetActiveUniformBlock(program, blockIndex, ARBUniformBufferObject.GL_UNIFORM_BLOCK_DATA_SIZE)
				// is not reliable as the driver is free to reduce array sizes when they
				// are not used in the shader.
				// Use a dummy element of the structure to find the total structure size.
				int lastOffset;
				if (endOfUBO.Offset <= 0 || !endOfUBO.Used)
				{
					// If the endOfUBO uniform has been eliminated by the shader compiler,
					// estimate the end of the buffer by using the offset of the boneMatrix uniform.
					lastOffset = boneMatrix.Offset + boneMatrix.MatrixSize * 4 * 4 * SIZEOF_FLOAT;
				}
				else
				{
					lastOffset = endOfUBO.Offset;
				}
				bufferSize = lastOffset + 4;

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("UBO Structure size: {0:D} (including endOfUBO)", bufferSize));
				}

				buffer = re.genBuffer();
				re.bindBuffer(IRenderingEngine_Fields.RE_UNIFORM_BUFFER, buffer);

				data = ByteBuffer.allocateDirect(bufferSize).order(ByteOrder.nativeOrder());
				// Initialize the buffer to 0's
				for (int i = 0; i < bufferSize; i++)
				{
					data.put(i, (sbyte) 0);
				}
				re.setBufferData(IRenderingEngine_Fields.RE_UNIFORM_BUFFER, bufferSize, data, IRenderingEngine_Fields.RE_DYNAMIC_DRAW);

				// On AMD hardware, the buffer data has to be set (setBufferData) before calling bindBufferBase
				re.bindBufferBase(IRenderingEngine_Fields.RE_UNIFORM_BUFFER, bindingPoint, buffer);

				startUpdate = 0;
				endUpdate = bufferSize;
			}

			base.initShaderProgram(re, shaderProgram);
		}

		public override void setUniforms(IRenderingEngine re, int shaderProgram)
		{
			if (startUpdate < endUpdate)
			{
				re.bindBuffer(IRenderingEngine_Fields.RE_UNIFORM_BUFFER, buffer);

				data.position(startUpdate);
				re.setBufferSubData(IRenderingEngine_Fields.RE_UNIFORM_BUFFER, startUpdate, endUpdate - startUpdate, data);
				data.limit(data.capacity());
				startUpdate = bufferSize;
				endUpdate = 0;
			}

			// Samplers can only be passed as uniforms
			setUniformsSamplers(re, shaderProgram);
		}

		protected internal virtual void prepareCopy(int offset, int Length)
		{
			data.position(offset);

			if (offset < startUpdate)
			{
				startUpdate = offset;
			}
			if (offset + Length > endUpdate)
			{
				endUpdate = offset + Length;
			}
		}

		protected internal virtual void copy(int value, ShaderUniformInfo shaderUniformInfo)
		{
			// Do not copy unused uniform, to avoid overwriting other used uniforms
			if (shaderUniformInfo.Used)
			{
				prepareCopy(shaderUniformInfo.Offset, 4);
				data.putInt(value);
			}
		}

		protected internal virtual void copy(int value, ShaderUniformInfo shaderUniformInfo, int index)
		{
			if (shaderUniformInfo.Used)
			{
				prepareCopy(shaderUniformInfo.Offset + index * 4, 4);
				data.putInt(value);
			}
		}

		protected internal virtual void copy(float value, ShaderUniformInfo shaderUniformInfo)
		{
			if (shaderUniformInfo.Used)
			{
				prepareCopy(shaderUniformInfo.Offset, 4);
				data.putFloat(value);
			}
		}

		protected internal virtual void copy(float value, ShaderUniformInfo shaderUniformInfo, int index)
		{
			if (shaderUniformInfo.Used)
			{
				prepareCopy(shaderUniformInfo.Offset + index * 4, 4);
				data.putFloat(value);
			}
		}

		protected internal virtual void copy(float[] values, ShaderUniformInfo shaderUniformInfo, int start, int end)
		{
			if (shaderUniformInfo.Used)
			{
				prepareCopy(shaderUniformInfo.Offset + start * 4, (end - start) * 4);
				for (int i = start; i < end; i++)
				{
					float value = values[i];
					if (float.IsNaN(value))
					{
						value = 0f;
					}
					data.putFloat(value);
				}
			}
		}

		protected internal virtual void copy(int[] values, ShaderUniformInfo shaderUniformInfo, int start, int end)
		{
			if (shaderUniformInfo.Used)
			{
				prepareCopy(shaderUniformInfo.Offset + start * 4, (end - start) * 4);
				for (int i = start; i < end; i++)
				{
					data.putInt(values[i]);
				}
			}
		}

		protected internal virtual void copy(bool value, ShaderUniformInfo shaderUniformInfo)
		{
			copy(value ? 1 : 0, shaderUniformInfo);
		}

		public override int TexEnable
		{
			set
			{
				if (value != TexEnable)
				{
					copy(value, this.texEnable);
					base.TexEnable = value;
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void setBoneMatrix(final int count, final float[] boneMatrix)
		public override void setBoneMatrix(int count, float[] boneMatrix)
		{
			if (count > 0)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float[] previousBoneMatrix = getBoneMatrix();
				float[] previousBoneMatrix = BoneMatrix;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int Length = 16 * count;
				int Length = 16 * count;
				int start = -1;
				for (int i = 0; i < Length; i++)
				{
					if (previousBoneMatrix[i] != boneMatrix[i])
					{
						start = i;
						break;
					}
				}

				if (start >= 0)
				{
					int end = start + 1;
					for (int i = Length - 1; i > start; i--)
					{
						if (previousBoneMatrix[i] != boneMatrix[i])
						{
							end = i + 1;
							break;
						}
					}
					copy(boneMatrix, this.boneMatrix, start, end);

					base.setBoneMatrix(count, boneMatrix);
				}
			}
		}

		public override float ColorDoubling
		{
			set
			{
				if (value != ColorDoubling)
				{
					copy(value, this.colorDoubling);
					base.ColorDoubling = value;
				}
			}
		}

		public override int CtestEnable
		{
			set
			{
				if (value != CtestEnable)
				{
					copy(value, this.ctestEnable);
					base.CtestEnable = value;
				}
			}
		}

		public override int CtestFunc
		{
			set
			{
				if (value != CtestFunc)
				{
					copy(value, this.ctestFunc);
					base.CtestFunc = value;
				}
			}
		}

		public override void setCtestMsk(int index, int ctestMsk)
		{
			if (ctestMsk != getCtestMsk(index))
			{
				copy(ctestMsk, this.ctestMsk, index);
				base.setCtestMsk(index, ctestMsk);
			}
		}

		public override void setCtestRef(int index, int ctestRef)
		{
			if (ctestRef != getCtestRef(index))
			{
				copy(ctestRef, this.ctestRef, index);
				base.setCtestRef(index, ctestRef);
			}
		}

		public override void setLightEnabled(int light, int lightEnabled)
		{
			if (lightEnabled != getLightEnabled(light))
			{
				copy(lightEnabled, this.lightEnabled, light);
				base.setLightEnabled(light, lightEnabled);
			}
		}

		public override int LightingEnable
		{
			set
			{
				if (value != LightingEnable)
				{
					copy(value, this.lightingEnable);
					base.LightingEnable = value;
				}
			}
		}

		public override void setLightKind(int light, int lightKind)
		{
			if (lightKind != getLightKind(light))
			{
				copy(lightKind, this.lightKind, light);
				base.setLightKind(light, lightKind);
			}
		}

		public override int LightMode
		{
			set
			{
				if (value != LightMode)
				{
					copy(value, this.lightMode);
					base.LightMode = value;
				}
			}
		}

		public override void setLightType(int light, int lightType)
		{
			if (lightType != getLightType(light))
			{
				copy(lightType, this.lightType, light);
				base.setLightType(light, lightType);
			}
		}

		public override void setMatFlags(int index, int matFlags)
		{
			if (matFlags != getMatFlags(index))
			{
				copy(matFlags, this.matFlags, index);
				base.setMatFlags(index, matFlags);
			}
		}

		public override float NormalScale
		{
			set
			{
				if (value != NormalScale)
				{
					copy(value, this.normalScale);
					base.NormalScale = value;
				}
			}
		}

		public override int NumberBones
		{
			set
			{
				if (value != NumberBones)
				{
					copy(value, this.numberBones);
					base.NumberBones = value;
				}
			}
		}

		public override float PositionScale
		{
			set
			{
				if (value != PositionScale)
				{
					copy(value, this.positionScale);
					base.PositionScale = value;
				}
			}
		}

		public override void setTexEnvMode(int index, int texEnvMode)
		{
			if (texEnvMode != getTexEnvMode(index))
			{
				copy(texEnvMode, this.texEnvMode, index);
				base.setTexEnvMode(index, texEnvMode);
			}
		}

		public override int TexMapMode
		{
			set
			{
				if (value != TexMapMode)
				{
					copy(value, this.texMapMode);
					base.TexMapMode = value;
				}
			}
		}

		public override int TexMapProj
		{
			set
			{
				if (value != TexMapProj)
				{
					copy(value, this.texMapProj);
					base.TexMapProj = value;
				}
			}
		}

		public override void setTexShade(int index, int texShade)
		{
			if (texShade != getTexShade(index))
			{
				copy(texShade, this.texShade, index);
				base.setTexShade(index, texShade);
			}
		}

		public override float TextureScale
		{
			set
			{
				if (value != TextureScale)
				{
					copy(value, this.textureScale);
					base.TextureScale = value;
				}
			}
		}

		public override int VinfoColor
		{
			set
			{
				if (value != VinfoColor)
				{
					copy(value, this.vinfoColor);
					base.VinfoColor = value;
				}
			}
		}

		public override int VinfoPosition
		{
			set
			{
				if (value != VinfoPosition)
				{
					copy(value, this.vinfoPosition);
					base.VinfoPosition = value;
				}
			}
		}

		public override int VinfoTransform2D
		{
			set
			{
				if (value != VinfoTransform2D)
				{
					copy(value, this.vinfoTransform2D);
					base.VinfoTransform2D = value;
				}
			}
		}

		public override float WeightScale
		{
			set
			{
				if (value != WeightScale)
				{
					copy(value, this.weightScale);
					base.WeightScale = value;
				}
			}
		}

		public override int ClutShift
		{
			set
			{
				if (value != ClutShift)
				{
					copy(value, this.clutShift);
					base.ClutShift = value;
				}
			}
		}

		public override int ClutMask
		{
			set
			{
				if (value != ClutMask)
				{
					copy(value, this.clutMask);
					base.ClutMask = value;
				}
			}
		}

		public override int ClutOffset
		{
			set
			{
				if (value != ClutOffset)
				{
					copy(value, this.clutOffset);
					base.ClutOffset = value;
				}
			}
		}

		public override bool MipmapShareClut
		{
			set
			{
				if (value != MipmapShareClut)
				{
					copy(value, this.mipmapShareClut);
					base.MipmapShareClut = value;
				}
			}
		}

		public override int TexPixelFormat
		{
			set
			{
				if (value != TexPixelFormat)
				{
					copy(value, this.texPixelFormat);
					base.TexPixelFormat = value;
				}
			}
		}

		public override float[] VertexColor
		{
			set
			{
				float[] currentVertexColor = VertexColor;
				if (value[0] != currentVertexColor[0] || value[1] != currentVertexColor[1] || value[2] != currentVertexColor[2] || value[3] != currentVertexColor[3])
				{
					copy(value, this.vertexColor, 0, 4);
					base.VertexColor = value;
				}
			}
		}

		public override int VinfoTexture
		{
			set
			{
				if (value != VinfoTexture)
				{
					copy(value, this.vinfoTexture);
					base.VinfoTexture = value;
				}
			}
		}

		public override int VinfoNormal
		{
			set
			{
				if (value != VinfoNormal)
				{
					copy(value, this.vinfoNormal);
					base.VinfoNormal = value;
				}
			}
		}

		public override int StencilTestEnable
		{
			set
			{
				if (value != StencilTestEnable)
				{
					copy(value, this.stencilTestEnable);
					base.StencilTestEnable = value;
				}
			}
		}

		public override int StencilFunc
		{
			set
			{
				if (value != StencilFunc)
				{
					copy(value, this.stencilFunc);
					base.StencilFunc = value;
				}
			}
		}

		public override int StencilMask
		{
			set
			{
				if (value != StencilMask)
				{
					copy(value, this.stencilMask);
					base.StencilMask = value;
				}
			}
		}

		public override int StencilOpFail
		{
			set
			{
				if (value != StencilOpFail)
				{
					copy(value, this.stencilOpFail);
					base.StencilOpFail = value;
				}
			}
		}

		public override int StencilOpZFail
		{
			set
			{
				if (value != StencilOpZFail)
				{
					copy(value, this.stencilOpZFail);
					base.StencilOpZFail = value;
				}
			}
		}

		public override int StencilOpZPass
		{
			set
			{
				if (value != StencilOpZPass)
				{
					copy(value, this.stencilOpZPass);
					base.StencilOpZPass = value;
				}
			}
		}

		public override int DepthTestEnable
		{
			set
			{
				if (value != DepthTestEnable)
				{
					copy(value, this.depthTestEnable);
					base.DepthTestEnable = value;
				}
			}
		}

		public override int DepthFunc
		{
			set
			{
				if (value != DepthFunc)
				{
					copy(value, this.depthFunc);
					base.DepthFunc = value;
				}
			}
		}

		public override int DepthMask
		{
			set
			{
				if (value != DepthMask)
				{
					copy(value, this.depthMask);
					base.DepthMask = value;
				}
			}
		}

		public override int StencilRef
		{
			set
			{
				if (value != StencilRef)
				{
					copy(value, this.stencilRef);
					base.StencilRef = value;
				}
			}
		}

		public override int ColorMaskEnable
		{
			set
			{
				if (value != ColorMaskEnable)
				{
					copy(value, this.colorMaskEnable);
					base.ColorMaskEnable = value;
				}
			}
		}

		public override void setColorMask(int redMask, int greenMask, int blueMask, int alphaMask)
		{
			int[] currentColorMask = ColorMask;
			if (redMask != currentColorMask[0] || greenMask != currentColorMask[1] || blueMask != currentColorMask[2] || alphaMask != currentColorMask[3])
			{
				copy(new int[] {redMask, greenMask, blueMask, alphaMask}, this.colorMask, 0, 4);
				base.setColorMask(redMask, greenMask, blueMask, alphaMask);
			}
		}

		public override void setNotColorMask(int notRedMask, int notGreenMask, int notBlueMask, int notAlphaMask)
		{
			int[] currentNotColorMask = NotColorMask;
			if (notRedMask != currentNotColorMask[0] || notGreenMask != currentNotColorMask[1] || notBlueMask != currentNotColorMask[2] || notAlphaMask != currentNotColorMask[3])
			{
				copy(new int[] {notRedMask, notGreenMask, notBlueMask, notAlphaMask}, this.notColorMask, 0, 4);
				base.setNotColorMask(notRedMask, notGreenMask, notBlueMask, notAlphaMask);
			}
		}

		public override int AlphaTestEnable
		{
			set
			{
				if (value != AlphaTestEnable)
				{
					copy(value, this.alphaTestEnable);
					base.AlphaTestEnable = value;
				}
			}
		}

		public override int AlphaTestFunc
		{
			set
			{
				if (value != AlphaTestFunc)
				{
					copy(value, this.alphaTestFunc);
					base.AlphaTestFunc = value;
				}
			}
		}

		public override int AlphaTestRef
		{
			set
			{
				if (value != AlphaTestRef)
				{
					copy(value, this.alphaTestRef);
					base.AlphaTestRef = value;
				}
			}
		}

		public override int AlphaTestMask
		{
			set
			{
				if (value != AlphaTestMask)
				{
					copy(value, this.alphaTestMask);
					base.AlphaTestMask = value;
				}
			}
		}

		public override int BlendTestEnable
		{
			set
			{
				if (value != BlendTestEnable)
				{
					copy(value, this.blendTestEnable);
					base.BlendTestEnable = value;
				}
			}
		}

		public override int BlendEquation
		{
			set
			{
				if (value != BlendEquation)
				{
					copy(value, this.blendEquation);
					base.BlendEquation = value;
				}
			}
		}

		public override int BlendSrc
		{
			set
			{
				if (value != BlendSrc)
				{
					copy(value, this.blendSrc);
					base.BlendSrc = value;
				}
			}
		}

		public override int BlendDst
		{
			set
			{
				if (value != BlendDst)
				{
					copy(value, this.blendDst);
					base.BlendDst = value;
				}
			}
		}

		public override float[] BlendSFix
		{
			set
			{
				float[] sfix = BlendSFix;
				if (value[0] != sfix[0] || value[1] != sfix[1] || value[2] != sfix[2])
				{
					copy(value, this.blendSFix, 0, 3);
					base.BlendSFix = value;
				}
			}
		}

		public override float[] BlendDFix
		{
			set
			{
				float[] dfix = BlendDFix;
				if (value[0] != dfix[0] || value[1] != dfix[1] || value[2] != dfix[2])
				{
					copy(value, this.blendDFix, 0, 3);
					base.BlendDFix = value;
				}
			}
		}

		public override int CopyRedToAlpha
		{
			set
			{
				if (value != CopyRedToAlpha)
				{
					copy(value, this.copyRedToAlpha);
					base.CopyRedToAlpha = value;
				}
			}
		}

		public override int WrapModeS
		{
			set
			{
				if (value != WrapModeS)
				{
					copy(value, this.wrapModeS);
					base.WrapModeS = value;
				}
			}
		}

		public override int WrapModeT
		{
			set
			{
				if (value != WrapModeT)
				{
					copy(value, this.wrapModeT);
					base.WrapModeT = value;
				}
			}
		}

		public override int FogEnable
		{
			set
			{
				if (value != FogEnable)
				{
					copy(value, this.fogEnable);
					base.FogEnable = value;
				}
			}
		}

		public override float[] FogColor
		{
			set
			{
				float[] currentFogColor = FogColor;
				if (value[0] != currentFogColor[0] || value[1] != currentFogColor[1] || value[2] != currentFogColor[2])
				{
					copy(value, this.fogColor, 0, 3);
					base.FogColor = value;
				}
			}
		}

		public override float FogEnd
		{
			set
			{
				if (value != FogEnd)
				{
					copy(value, this.fogEnd);
					base.FogEnd = value;
				}
			}
		}

		public override float FogScale
		{
			set
			{
				if (value != FogScale)
				{
					copy(value, this.fogScale);
					base.FogScale = value;
				}
			}
		}

		public override int ClipPlaneEnable
		{
			set
			{
				if (value != ClipPlaneEnable)
				{
					copy(value, this.clipPlaneEnable);
					base.ClipPlaneEnable = value;
				}
			}
		}

		public override void setViewportPos(float x, float y, float z)
		{
			float[] currentViewportPos = ViewportPos;
			if (x != currentViewportPos[0] || y != currentViewportPos[1] || z != currentViewportPos[2])
			{
				copy(new float[] {x, y, z}, this.viewportPos, 0, 3);
				base.setViewportPos(x, y, z);
			}
		}

		public override void setViewportScale(float sx, float sy, float sz)
		{
			float[] currentViewportScale = ViewportScale;
			if (sx != currentViewportScale[0] || sy != currentViewportScale[1] || sz != currentViewportScale[2])
			{
				copy(new float[] {sx, sy, sz}, this.viewportScale, 0, 3);
				base.setViewportScale(sx, sy, sz);
			}
		}
	}
}