using System;
using System.Text;

/*

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
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_4BIT_INDEXED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_8BIT_INDEXED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.DirectBufferUtilities.allocateDirectBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.DirectBufferUtilities.copyBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.DirectBufferUtilities.getDirectBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.DirectBufferUtilities.getDirectByteBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.DirectBufferUtilities.getDirectFloatBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.DirectBufferUtilities.getDirectIntBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.DirectBufferUtilities.getDirectShortBuffer;

	using XBRZNativeFilter = pspsharp.plugins.XBRZNativeFilter;


	using ARBBufferObject = org.lwjgl.opengl.ARBBufferObject;
	using ARBFramebufferObject = org.lwjgl.opengl.ARBFramebufferObject;
	using ARBGeometryShader4 = org.lwjgl.opengl.ARBGeometryShader4;
	using ARBUniformBufferObject = org.lwjgl.opengl.ARBUniformBufferObject;
	using ARBVertexArrayObject = org.lwjgl.opengl.ARBVertexArrayObject;
	using EXTMultiDrawArrays = org.lwjgl.opengl.EXTMultiDrawArrays;
	using EXTTextureCompressionS3TC = org.lwjgl.opengl.EXTTextureCompressionS3TC;
	using EXTTextureFilterAnisotropic = org.lwjgl.opengl.EXTTextureFilterAnisotropic;
	using GL11 = org.lwjgl.opengl.GL11;
	using GL12 = org.lwjgl.opengl.GL12;
	using GL13 = org.lwjgl.opengl.GL13;
	using GL14 = org.lwjgl.opengl.GL14;
	using GL15 = org.lwjgl.opengl.GL15;
	using GL20 = org.lwjgl.opengl.GL20;
	using GL30 = org.lwjgl.opengl.GL30;
	using GL32 = org.lwjgl.opengl.GL32;
	using GLContext = org.lwjgl.opengl.GLContext;
	using NVTextureBarrier = org.lwjgl.opengl.NVTextureBarrier;

	/// <summary>
	/// @author gid15
	/// 
	/// An abstract RenderingEngine implementing calls to OpenGL using LWJGL. The
	/// class contains no rendering logic, it just implements the interface to LWJGL.
	/// </summary>
	public class RenderingEngineLwjgl : NullRenderingEngine
	{

		protected internal static readonly int[] flagToGL = new int[] {GL11.GL_ALPHA_TEST, GL11.GL_DEPTH_TEST, GL11.GL_SCISSOR_TEST, GL11.GL_STENCIL_TEST, GL11.GL_BLEND, GL11.GL_CULL_FACE, GL11.GL_DITHER, GL11.GL_FOG, 0, GL11.GL_TEXTURE_2D, GL11.GL_LIGHTING, GL11.GL_LIGHT0, GL11.GL_LIGHT1, GL11.GL_LIGHT2, GL11.GL_LIGHT3, GL11.GL_LINE_SMOOTH, 0, 0, GL11.GL_COLOR_LOGIC_OP, 0, 0, 0, GL11.GL_COLOR_MATERIAL, GL11.GL_TEXTURE_GEN_S, GL11.GL_TEXTURE_GEN_T};
		protected internal static readonly int[] shadeModelToGL = new int[] {GL11.GL_FLAT, GL11.GL_SMOOTH};
		protected internal static readonly int[] colorTypeToGL = new int[] {GL11.GL_AMBIENT, GL11.GL_EMISSION, GL11.GL_DIFFUSE, GL11.GL_SPECULAR};
		protected internal static readonly int[] lightModeToGL = new int[] {GL12.GL_SINGLE_COLOR, GL12.GL_SEPARATE_SPECULAR_COLOR};
		protected internal static readonly int[] blendSrcToGL = new int[] {GL11.GL_DST_COLOR, GL11.GL_ONE_MINUS_DST_COLOR, GL11.GL_SRC_ALPHA, GL11.GL_ONE_MINUS_SRC_ALPHA, GL11.GL_DST_ALPHA, GL11.GL_ONE_MINUS_DST_ALPHA, GL11.GL_SRC_ALPHA, GL11.GL_ONE_MINUS_SRC_ALPHA, GL11.GL_DST_ALPHA, GL11.GL_ONE_MINUS_DST_ALPHA, GL11.GL_CONSTANT_COLOR, GL11.GL_ONE_MINUS_CONSTANT_COLOR, GL11.GL_ZERO, GL11.GL_ONE};
		protected internal static readonly int[] blendDstToGL = new int[] {GL11.GL_SRC_COLOR, GL11.GL_ONE_MINUS_SRC_COLOR, GL11.GL_SRC_ALPHA, GL11.GL_ONE_MINUS_SRC_ALPHA, GL11.GL_DST_ALPHA, GL11.GL_ONE_MINUS_DST_ALPHA, GL11.GL_SRC_ALPHA, GL11.GL_ONE_MINUS_SRC_ALPHA, GL11.GL_DST_ALPHA, GL11.GL_ONE_MINUS_DST_ALPHA, GL11.GL_CONSTANT_COLOR, GL11.GL_ONE_MINUS_CONSTANT_COLOR, GL11.GL_ZERO, GL11.GL_ONE};
		protected internal static readonly int[] logicOpToGL = new int[] {GL11.GL_CLEAR, GL11.GL_AND, GL11.GL_AND_REVERSE, GL11.GL_COPY, GL11.GL_AND_INVERTED, GL11.GL_NOOP, GL11.GL_XOR, GL11.GL_OR, GL11.GL_NOR, GL11.GL_EQUIV, GL11.GL_INVERT, GL11.GL_OR_REVERSE, GL11.GL_COPY_INVERTED, GL11.GL_OR_INVERTED, GL11.GL_NAND, GL11.GL_SET};
		protected internal static readonly int[] wrapModeToGL = new int[] {GL11.GL_REPEAT, GL12.GL_CLAMP_TO_EDGE};
		protected internal static readonly int[] colorMaterialToGL = new int[] {GL11.GL_AMBIENT, GL11.GL_AMBIENT, GL11.GL_DIFFUSE, GL11.GL_AMBIENT_AND_DIFFUSE, GL11.GL_SPECULAR, GL11.GL_AMBIENT, GL11.GL_DIFFUSE, GL11.GL_AMBIENT_AND_DIFFUSE};
		protected internal static readonly int[] depthFuncToGL = new int[] {GL11.GL_NEVER, GL11.GL_ALWAYS, GL11.GL_EQUAL, GL11.GL_NOTEQUAL, GL11.GL_LESS, GL11.GL_LEQUAL, GL11.GL_GREATER, GL11.GL_GEQUAL};
		protected internal static readonly int[] texEnvNameToGL = new int[] {GL13.GL_COMBINE_RGB, GL13.GL_COMBINE_ALPHA, GL13.GL_RGB_SCALE, GL11.GL_ALPHA_SCALE, GL15.GL_SRC0_RGB, GL15.GL_SRC1_RGB, GL15.GL_SRC2_RGB, GL15.GL_SRC0_ALPHA, GL15.GL_SRC1_ALPHA, GL15.GL_SRC2_ALPHA, GL13.GL_OPERAND0_RGB, GL13.GL_OPERAND1_RGB, GL13.GL_OPERAND2_RGB, GL13.GL_OPERAND0_ALPHA, GL13.GL_OPERAND1_ALPHA, GL13.GL_OPERAND2_ALPHA, GL11.GL_TEXTURE_ENV_MODE};
		protected internal static readonly int[] texEnvParamToGL = new int[] {GL11.GL_MODULATE, GL11.GL_DECAL, GL11.GL_BLEND, GL11.GL_REPLACE, GL11.GL_ADD, GL13.GL_INTERPOLATE, GL13.GL_SUBTRACT, GL11.GL_TEXTURE, GL13.GL_CONSTANT, GL13.GL_PREVIOUS, GL11.GL_SRC_COLOR, GL11.GL_SRC_ALPHA, GL13.GL_COMBINE};
		protected internal static readonly int[] shaderTypeToGL = new int[] {GL20.GL_VERTEX_SHADER, GL20.GL_FRAGMENT_SHADER, GL32.GL_GEOMETRY_SHADER};
		protected internal static readonly int[] primitiveToGL = new int[] {GL11.GL_POINTS, GL11.GL_LINES, GL11.GL_LINE_STRIP, GL11.GL_TRIANGLES, GL11.GL_TRIANGLE_STRIP, GL11.GL_TRIANGLE_FAN, GL11.GL_QUADS, GL11.GL_QUADS, GL32.GL_LINES_ADJACENCY, GL32.GL_TRIANGLES_ADJACENCY, GL32.GL_TRIANGLE_STRIP_ADJACENCY};
		protected internal static readonly int[] clientStateToGL = new int[] {GL11.GL_TEXTURE_COORD_ARRAY, GL11.GL_COLOR_ARRAY, GL11.GL_NORMAL_ARRAY, GL11.GL_VERTEX_ARRAY};
		protected internal static readonly int[] pointerTypeToGL = new int[] {GL11.GL_BYTE, GL11.GL_UNSIGNED_BYTE, GL11.GL_SHORT, GL11.GL_UNSIGNED_SHORT, GL11.GL_INT, GL11.GL_UNSIGNED_INT, GL11.GL_FLOAT, GL11.GL_DOUBLE};
		protected internal static readonly int[] bufferUsageToGL = new int[] {GL15.GL_STREAM_DRAW, GL15.GL_STREAM_READ, GL15.GL_STREAM_COPY, GL15.GL_STATIC_DRAW, GL15.GL_STATIC_READ, GL15.GL_STATIC_COPY, GL15.GL_DYNAMIC_DRAW, GL15.GL_DYNAMIC_READ, GL15.GL_DYNAMIC_COPY};
		protected internal static readonly int[] mipmapFilterToGL = new int[] {GL11.GL_NEAREST, GL11.GL_LINEAR, GL11.GL_NEAREST, GL11.GL_NEAREST, GL11.GL_NEAREST_MIPMAP_NEAREST, GL11.GL_LINEAR_MIPMAP_NEAREST, GL11.GL_NEAREST_MIPMAP_LINEAR, GL11.GL_LINEAR_MIPMAP_LINEAR};
		protected internal static readonly int[] textureFormatToGL = new int[] {GL11.GL_RGB, GL11.GL_RGBA, GL11.GL_RGBA, GL11.GL_RGBA, GL30.GL_RED_INTEGER, GL30.GL_RED_INTEGER, GL30.GL_RED_INTEGER, GL30.GL_RED_INTEGER, EXTTextureCompressionS3TC.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT, EXTTextureCompressionS3TC.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT, EXTTextureCompressionS3TC.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT, GL11.GL_RGB, GL11.GL_RGBA, GL11.GL_RGBA, GL11.GL_RGBA, GL11.GL_DEPTH_COMPONENT, GL30.GL_DEPTH_STENCIL, GL30.GL_DEPTH_STENCIL};
		protected internal static readonly int[] textureInternalFormatToGL = new int[] {GL11.GL_RGB, GL11.GL_RGBA, GL11.GL_RGBA, GL11.GL_RGBA, GL30.GL_R8UI, GL30.GL_R8UI, GL30.GL_R16UI, GL30.GL_R32UI, EXTTextureCompressionS3TC.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT, EXTTextureCompressionS3TC.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT, EXTTextureCompressionS3TC.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT, GL11.GL_RGB, GL11.GL_RGBA, GL11.GL_RGBA, GL11.GL_RGBA, GL11.GL_DEPTH_COMPONENT, GL30.GL_DEPTH_STENCIL, GL30.GL_DEPTH_STENCIL};
		protected internal static readonly int[] textureTypeToGL = new int[] {GL12.GL_UNSIGNED_SHORT_5_6_5_REV, GL12.GL_UNSIGNED_SHORT_1_5_5_5_REV, GL12.GL_UNSIGNED_SHORT_4_4_4_4_REV, GL11.GL_UNSIGNED_BYTE, GL11.GL_UNSIGNED_BYTE, GL11.GL_UNSIGNED_BYTE, GL11.GL_UNSIGNED_SHORT, GL11.GL_UNSIGNED_INT, EXTTextureCompressionS3TC.GL_COMPRESSED_RGBA_S3TC_DXT1_EXT, EXTTextureCompressionS3TC.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT, EXTTextureCompressionS3TC.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT, GL12.GL_UNSIGNED_SHORT_5_6_5_REV, GL12.GL_UNSIGNED_SHORT_1_5_5_5_REV, GL12.GL_UNSIGNED_SHORT_4_4_4_4_REV, GL11.GL_UNSIGNED_BYTE, GL11.GL_UNSIGNED_BYTE, GL30.GL_UNSIGNED_INT_24_8, GL30.GL_UNSIGNED_INT_24_8};
		protected internal static readonly int[] stencilOpToGL = new int[] {GL11.GL_KEEP, GL11.GL_ZERO, GL11.GL_REPLACE, GL11.GL_INVERT, GL11.GL_INCR, GL11.GL_DECR};
		protected internal static readonly int[] stencilFuncToGL = new int[] {GL11.GL_NEVER, GL11.GL_ALWAYS, GL11.GL_EQUAL, GL11.GL_NOTEQUAL, GL11.GL_LESS, GL11.GL_LEQUAL, GL11.GL_GREATER, GL11.GL_GEQUAL};
		protected internal static readonly int[] alphaFuncToGL = new int[] {GL11.GL_NEVER, GL11.GL_ALWAYS, GL11.GL_EQUAL, GL11.GL_NOTEQUAL, GL11.GL_LESS, GL11.GL_LEQUAL, GL11.GL_GREATER, GL11.GL_GEQUAL};
		protected internal static readonly int[] blendModeToGL = new int[] {GL14.GL_FUNC_ADD, GL14.GL_FUNC_SUBTRACT, GL14.GL_FUNC_REVERSE_SUBTRACT, GL14.GL_MIN, GL14.GL_MAX, GL14.GL_FUNC_ADD};
		protected internal static readonly int[] programParameterToGL = new int[] {GL32.GL_GEOMETRY_INPUT_TYPE, GL32.GL_GEOMETRY_OUTPUT_TYPE, GL32.GL_GEOMETRY_VERTICES_OUT};
		protected internal static readonly int[] bufferTargetToGL = new int[] {GL15.GL_ARRAY_BUFFER, ARBUniformBufferObject.GL_UNIFORM_BUFFER, GL15.GL_ELEMENT_ARRAY_BUFFER};
		protected internal static readonly int[] matrixModeToGL = new int[] {GL11.GL_PROJECTION, GL11.GL_MODELVIEW, GL11.GL_MODELVIEW, GL11.GL_TEXTURE, GL11.GL_MODELVIEW};
		protected internal static readonly int[] framebufferTargetToGL = new int[] {ARBFramebufferObject.GL_FRAMEBUFFER, ARBFramebufferObject.GL_READ_FRAMEBUFFER, ARBFramebufferObject.GL_DRAW_FRAMEBUFFER};
		protected internal static readonly int[] attachmentToGL = new int[] {ARBFramebufferObject.GL_DEPTH_ATTACHMENT, ARBFramebufferObject.GL_STENCIL_ATTACHMENT, ARBFramebufferObject.GL_DEPTH_STENCIL_ATTACHMENT, ARBFramebufferObject.GL_COLOR_ATTACHMENT0, ARBFramebufferObject.GL_COLOR_ATTACHMENT1, ARBFramebufferObject.GL_COLOR_ATTACHMENT2, ARBFramebufferObject.GL_COLOR_ATTACHMENT3, ARBFramebufferObject.GL_COLOR_ATTACHMENT4, ARBFramebufferObject.GL_COLOR_ATTACHMENT5, ARBFramebufferObject.GL_COLOR_ATTACHMENT6, ARBFramebufferObject.GL_COLOR_ATTACHMENT7};
		protected internal static readonly int[] pixelTransferToGL = new int[] {GL11.GL_MAP_COLOR, GL11.GL_MAP_STENCIL, GL11.GL_INDEX_SHIFT, GL11.GL_INDEX_OFFSET, GL11.GL_RED_SCALE, GL11.GL_GREEN_SCALE, GL11.GL_BLUE_SCALE, GL11.GL_ALPHA_SCALE, GL11.GL_DEPTH_BIAS, GL11.GL_RED_BIAS, GL11.GL_GREEN_BIAS, GL11.GL_BLUE_BIAS, GL11.GL_ALPHA_BIAS, GL11.GL_DEPTH_BIAS};
		protected internal static readonly int[] pixelMapToGL = new int[] {GL11.GL_PIXEL_MAP_I_TO_I, GL11.GL_PIXEL_MAP_S_TO_S, GL11.GL_PIXEL_MAP_I_TO_R, GL11.GL_PIXEL_MAP_I_TO_G, GL11.GL_PIXEL_MAP_I_TO_B, GL11.GL_PIXEL_MAP_I_TO_A, GL11.GL_PIXEL_MAP_R_TO_R, GL11.GL_PIXEL_MAP_G_TO_G, GL11.GL_PIXEL_MAP_B_TO_B, GL11.GL_PIXEL_MAP_A_TO_A};
		protected internal static readonly int[] buffersMaskToGL = new int[] {GL11.GL_COLOR_BUFFER_BIT, GL11.GL_DEPTH_BUFFER_BIT, GL11.GL_STENCIL_BUFFER_BIT};
		protected internal bool vendorIntel;
		protected internal bool hasOpenGL30;

		public static string Version
		{
			get
			{
				return GL11.glGetString(GL11.GL_VERSION);
			}
		}

		public static IRenderingEngine newInstance()
		{
			if (GLContext.Capabilities.OpenGL31)
			{
				log.info("Using RenderingEngineLwjgl31");
				return new RenderingEngineLwjgl31();
			}
			else if (GLContext.Capabilities.OpenGL15)
			{
				log.info("Using RenderingEngineLwjgl15");
				return new RenderingEngineLwjgl15();
			}
			else if (GLContext.Capabilities.OpenGL12)
			{
				log.info("Using RenderingEngineLwjgl12");
				return new RenderingEngineLwjgl12();
			}

			log.info("Using RenderingEngineLwjgl");
			return new RenderingEngineLwjgl();
		}

		public RenderingEngineLwjgl()
		{
			init();
		}

		protected internal virtual void init()
		{
			string openGLVersion = GL11.glGetString(GL11.GL_VERSION);
			string openGLVendor = GL11.glGetString(GL11.GL_VENDOR);
			string openGLRenderer = GL11.glGetString(GL11.GL_RENDERER);
			log.info(string.Format("OpenGL version: {0}, vender: {1}, renderer: {2}", openGLVersion, openGLVendor, openGLRenderer));

			vendorIntel = "Intel".Equals(openGLVendor, StringComparison.OrdinalIgnoreCase);
			hasOpenGL30 = GLContext.Capabilities.OpenGL30;

			if (GLContext.Capabilities.OpenGL20)
			{
				string shadingLanguageVersion = GL11.glGetString(GL20.GL_SHADING_LANGUAGE_VERSION);
				log.info("Shading Language version: " + shadingLanguageVersion);
			}

			if (GLContext.Capabilities.OpenGL30)
			{
				int contextFlags = GL11.glGetInteger(GL30.GL_CONTEXT_FLAGS);
				string s = string.Format("GL_CONTEXT_FLAGS: 0x{0:X}", contextFlags);
				if ((contextFlags & GL30.GL_CONTEXT_FLAG_FORWARD_COMPATIBLE_BIT) != 0)
				{
					s += " (GL_CONTEXT_FLAG_FORWARD_COMPATIBLE_BIT)";
				}
				log.info(s);
			}

			if (GLContext.Capabilities.OpenGL32)
			{
				int contextProfileMask = GL11.glGetInteger(GL32.GL_CONTEXT_PROFILE_MASK);
				string s = string.Format("GL_CONTEXT_PROFILE_MASK: 0x{0:X}", contextProfileMask);
				if ((contextProfileMask & GL32.GL_CONTEXT_CORE_PROFILE_BIT) != 0)
				{
					s += " (GL_CONTEXT_CORE_PROFILE_BIT)";
				}
				if ((contextProfileMask & GL32.GL_CONTEXT_COMPATIBILITY_PROFILE_BIT) != 0)
				{
					s += " (GL_CONTEXT_COMPATIBILITY_PROFILE_BIT)";
				}
				log.info(s);
			}
		}

		public override void disableFlag(int flag)
		{
			int glFlag = flagToGL[flag];
			if (glFlag != 0)
			{
				GL11.glDisable(glFlag);
			}
		}

		public override void enableFlag(int flag)
		{
			int glFlag = flagToGL[flag];
			if (glFlag != 0)
			{
				GL11.glEnable(glFlag);
			}
		}

		public override void setDepthRange(float zpos, float zscale, int near, int far)
		{
			GL11.glDepthRange((zpos - zscale) / 65535f, (zpos + zscale) / 65535f);
		}

		public override int DepthFunc
		{
			set
			{
				GL11.glDepthFunc(depthFuncToGL[value]);
			}
		}

		public override void setViewport(int x, int y, int width, int height)
		{
			GL11.glViewport(x, y, width, height);
		}

		public override int ShadeModel
		{
			set
			{
				GL11.glShadeModel(shadeModelToGL[value]);
			}
		}

		public override float[] MaterialEmissiveColor
		{
			set
			{
				GL11.glMaterial(GL11.GL_FRONT, GL11.GL_EMISSION, getDirectBuffer(value));
			}
		}

		public override float[] MaterialAmbientColor
		{
			set
			{
				GL11.glMaterial(GL11.GL_FRONT, GL11.GL_AMBIENT, getDirectBuffer(value));
			}
		}

		public override float[] MaterialDiffuseColor
		{
			set
			{
				GL11.glMaterial(GL11.GL_FRONT, GL11.GL_DIFFUSE, getDirectBuffer(value));
			}
		}

		public override float[] MaterialSpecularColor
		{
			set
			{
				GL11.glMaterial(GL11.GL_FRONT, GL11.GL_SPECULAR, getDirectBuffer(value));
			}
		}

		public override float[] LightModelAmbientColor
		{
			set
			{
				GL11.glLightModel(GL11.GL_LIGHT_MODEL_AMBIENT, getDirectBuffer(value));
			}
		}

		public override int LightMode
		{
			set
			{
				GL11.glLightModeli(GL12.GL_LIGHT_MODEL_COLOR_CONTROL, lightModeToGL[value]);
			}
		}

		public override void setLightAmbientColor(int light, float[] color)
		{
			GL11.glLight(GL11.GL_LIGHT0 + light, GL11.GL_AMBIENT, getDirectBuffer(color));
		}

		public override void setLightDiffuseColor(int light, float[] color)
		{
			GL11.glLight(GL11.GL_LIGHT0 + light, GL11.GL_DIFFUSE, getDirectBuffer(color));
		}

		public override void setLightSpecularColor(int light, float[] color)
		{
			GL11.glLight(GL11.GL_LIGHT0 + light, GL11.GL_SPECULAR, getDirectBuffer(color));
		}

		public override void setLightConstantAttenuation(int light, float constant)
		{
			GL11.glLightf(GL11.GL_LIGHT0 + light, GL11.GL_CONSTANT_ATTENUATION, constant);
		}

		public override void setLightLinearAttenuation(int light, float linear)
		{
			GL11.glLightf(GL11.GL_LIGHT0 + light, GL11.GL_LINEAR_ATTENUATION, linear);
		}

		public override void setLightQuadraticAttenuation(int light, float quadratic)
		{
			GL11.glLightf(GL11.GL_LIGHT0 + light, GL11.GL_QUADRATIC_ATTENUATION, quadratic);
		}

		public override void setLightDirection(int light, float[] direction)
		{
			GL11.glLight(GL11.GL_LIGHT0 + light, GL11.GL_SPOT_DIRECTION, getDirectBuffer(direction));
		}

		public override void setLightPosition(int light, float[] position)
		{
			GL11.glLight(GL11.GL_LIGHT0 + light, GL11.GL_POSITION, getDirectBuffer(position));
		}

		public override void setLightSpotCutoff(int light, float cutoff)
		{
			GL11.glLightf(GL11.GL_LIGHT0 + light, GL11.GL_SPOT_CUTOFF, cutoff);
		}

		public override void setLightSpotExponent(int light, float exponent)
		{
			GL11.glLightf(GL11.GL_LIGHT0 + light, GL11.GL_SPOT_EXPONENT, exponent);
		}

		public override float[] BlendColor
		{
			set
			{
				try
				{
					GL14.glBlendColor(value[0], value[1], value[2], value[3]);
				}
				catch (System.InvalidOperationException e)
				{
					Console.WriteLine("VideoEngine: " + e.Message);
				}
			}
		}

		public override void setBlendFunc(int src, int dst)
		{
			try
			{
				GL11.glBlendFunc(blendSrcToGL[src], blendDstToGL[dst]);
			}
			catch (System.InvalidOperationException e)
			{
				Console.WriteLine("VideoEngine: " + e.Message);
			}
		}

		public override int LogicOp
		{
			set
			{
				GL11.glLogicOp(logicOpToGL[value]);
			}
		}

		public override void setColorMask(bool redWriteEnabled, bool greenWriteEnabled, bool blueWriteEnabled, bool alphaWriteEnabled)
		{
			GL11.glColorMask(redWriteEnabled, greenWriteEnabled, blueWriteEnabled, alphaWriteEnabled);
		}

		public override bool DepthMask
		{
			set
			{
				GL11.glDepthMask(value);
			}
		}

		public override void setTextureWrapMode(int s, int t)
		{
			GL11.glTexParameteri(GL11.GL_TEXTURE_2D, GL11.GL_TEXTURE_WRAP_S, wrapModeToGL[s]);
			GL11.glTexParameteri(GL11.GL_TEXTURE_2D, GL11.GL_TEXTURE_WRAP_T, wrapModeToGL[t]);
		}

		public override int TextureMipmapMinLevel
		{
			set
			{
				GL11.glTexParameteri(GL11.GL_TEXTURE_2D, GL12.GL_TEXTURE_BASE_LEVEL, value);
			}
		}

		public override int TextureMipmapMaxLevel
		{
			set
			{
				GL11.glTexParameteri(GL11.GL_TEXTURE_2D, GL12.GL_TEXTURE_MAX_LEVEL, value);
			}
		}

		public override int TextureMipmapMinFilter
		{
			set
			{
				GL11.glTexParameteri(GL11.GL_TEXTURE_2D, GL11.GL_TEXTURE_MIN_FILTER, mipmapFilterToGL[value]);
			}
		}

		public override int TextureMipmapMagFilter
		{
			set
			{
				GL11.glTexParameteri(GL11.GL_TEXTURE_2D, GL11.GL_TEXTURE_MAG_FILTER, mipmapFilterToGL[value]);
			}
		}

		public override void setColorMaterial(bool ambient, bool diffuse, bool specular)
		{
			int index = (ambient ? 1 : 0) | (diffuse ? 2 : 0) | (specular ? 4 : 0);
			GL11.glColorMaterial(GL11.GL_FRONT_AND_BACK, colorMaterialToGL[index]);
		}

		public override void setTextureEnvironmentMapping(int u, int v)
		{
			GL11.glTexGeni(GL11.GL_S, GL11.GL_TEXTURE_GEN_MODE, GL11.GL_SPHERE_MAP);
			GL11.glTexGeni(GL11.GL_T, GL11.GL_TEXTURE_GEN_MODE, GL11.GL_SPHERE_MAP);
		}

		public override float[] VertexColor
		{
			set
			{
				GL11.glColor4f(value[0], value[1], value[2], value[3]);
			}
		}

		public override void setUniform(int id, int value)
		{
			GL20.glUniform1i(id, value);
		}

		public override void setUniform(int id, int value1, int value2)
		{
			GL20.glUniform2i(id, value1, value2);
		}

		public override void setUniform(int id, float value)
		{
			GL20.glUniform1f(id, value);
		}

		public override void setUniform2(int id, int[] values)
		{
			GL20.glUniform2i(id, values[0], values[1]);
		}

		public override void setUniform3(int id, int[] values)
		{
			GL20.glUniform3i(id, values[0], values[1], values[2]);
		}

		public override void setUniform3(int id, float[] values)
		{
			GL20.glUniform3f(id, values[0], values[1], values[2]);
		}

		public override void setUniform4(int id, int[] values)
		{
			GL20.glUniform4i(id, values[0], values[1], values[2], values[3]);
		}

		public override void setUniform4(int id, float[] values)
		{
			GL20.glUniform4f(id, values[0], values[1], values[2], values[3]);
		}

		public override void setUniformMatrix4(int id, int count, float[] values)
		{
			GL20.glUniformMatrix4(id, false, getDirectBuffer(values, count * 16));
		}

		public override void setTexEnv(int name, int param)
		{
			GL11.glTexEnvi(GL11.GL_TEXTURE_ENV, texEnvNameToGL[name], texEnvParamToGL[param]);
		}

		public override void setTexEnv(int name, float param)
		{
			GL11.glTexEnvf(GL11.GL_TEXTURE_ENV, texEnvNameToGL[name], param);
		}

		public override void attachShader(int program, int shader)
		{
			GL20.glAttachShader(program, shader);
		}

		public override bool compilerShader(int shader, string source)
		{
			GL20.glShaderSource(shader, source);
			GL20.glCompileShader(shader);
			return GL20.glGetShader(shader, GL20.GL_COMPILE_STATUS) == GL11.GL_TRUE;
		}

		public override int createProgram()
		{
			return GL20.glCreateProgram();
		}

		public override void useProgram(int program)
		{
			GL20.glUseProgram(program);
		}

		public override int createShader(int type)
		{
			return GL20.glCreateShader(shaderTypeToGL[type]);
		}

		public override int getAttribLocation(int program, string name)
		{
			return GL20.glGetAttribLocation(program, name);
		}

		public override void bindAttribLocation(int program, int index, string name)
		{
			GL20.glBindAttribLocation(program, index, name);
		}

		public override int getUniformLocation(int program, string name)
		{
			return GL20.glGetUniformLocation(program, name);
		}

		public override bool linkProgram(int program)
		{
			GL20.glLinkProgram(program);
			return GL20.glGetProgram(program, GL20.GL_LINK_STATUS) == GL11.GL_TRUE;
		}

		public override bool validateProgram(int program)
		{
			GL20.glValidateProgram(program);
			return GL20.glGetProgram(program, GL20.GL_VALIDATE_STATUS) == GL11.GL_TRUE;
		}

		public override string getProgramInfoLog(int program)
		{
			int infoLogLength = GL20.glGetProgram(program, GL20.GL_INFO_LOG_LENGTH);

			if (infoLogLength <= 1)
			{
				return null;
			}

			string infoLog = GL20.glGetProgramInfoLog(program, infoLogLength);

			// Remove ending '\0' byte(s)
			while (infoLog.Length > 0 && infoLog[infoLog.Length - 1] == '\0')
			{
				infoLog = infoLog.Substring(0, infoLog.Length - 1);
			}

			return infoLog;
		}

		public override string getShaderInfoLog(int shader)
		{
			int infoLogLength = GL20.glGetShader(shader, GL20.GL_INFO_LOG_LENGTH);
			if (infoLogLength <= 1)
			{
				return null;
			}

			string infoLog = GL20.glGetShaderInfoLog(shader, infoLogLength);

			// Remove ending '\0' byte(s)
			while (infoLog.Length > 0 && infoLog[infoLog.Length - 1] == '\0')
			{
				infoLog = infoLog.Substring(0, infoLog.Length - 1);
			}

			return infoLog;
		}

		public override bool isExtensionAvailable(string name)
		{
			string extensions = GL11.glGetString(GL11.GL_EXTENSIONS);
			if (string.ReferenceEquals(extensions, null))
			{
				return false;
			}

			// Extensions are space separated
			return (" " + extensions + " ").IndexOf(" " + name + " ") >= 0;
		}

		public override void drawArrays(int primitive, int first, int count)
		{
			GL11.glDrawArrays(primitiveToGL[primitive], first, count);
		}

		public override void deleteBuffer(int buffer)
		{
			ARBBufferObject.glDeleteBuffersARB(buffer);
		}

		public override int genBuffer()
		{
			return ARBBufferObject.glGenBuffersARB();
		}

		public override void setBufferData(int target, int size, Buffer buffer, int usage)
		{
			if (buffer is ByteBuffer)
			{
				ARBBufferObject.glBufferDataARB(bufferTargetToGL[target], getDirectBuffer(size, (ByteBuffer) buffer), bufferUsageToGL[usage]);
			}
			else if (buffer is IntBuffer)
			{
				ARBBufferObject.glBufferDataARB(bufferTargetToGL[target], getDirectBuffer(size, (IntBuffer) buffer), bufferUsageToGL[usage]);
			}
			else if (buffer is ShortBuffer)
			{
				ARBBufferObject.glBufferDataARB(bufferTargetToGL[target], getDirectBuffer(size, (ShortBuffer) buffer), bufferUsageToGL[usage]);
			}
			else if (buffer is FloatBuffer)
			{
				ARBBufferObject.glBufferDataARB(bufferTargetToGL[target], getDirectBuffer(size, (FloatBuffer) buffer), bufferUsageToGL[usage]);
			}
			else if (buffer == null)
			{
				ARBBufferObject.glBufferDataARB(bufferTargetToGL[target], size, bufferUsageToGL[usage]);
			}
			else
			{
				throw new System.ArgumentException();
			}
		}

		public override void setBufferSubData(int target, int offset, int size, Buffer buffer)
		{
			if (buffer is ByteBuffer)
			{
				ARBBufferObject.glBufferSubDataARB(bufferTargetToGL[target], offset, getDirectBuffer(size, (ByteBuffer) buffer));
			}
			else if (buffer is IntBuffer)
			{
				ARBBufferObject.glBufferSubDataARB(bufferTargetToGL[target], offset, getDirectBuffer(size, (IntBuffer) buffer));
			}
			else if (buffer is ShortBuffer)
			{
				ARBBufferObject.glBufferSubDataARB(bufferTargetToGL[target], offset, getDirectBuffer(size, (ShortBuffer) buffer));
			}
			else if (buffer is FloatBuffer)
			{
				ARBBufferObject.glBufferSubDataARB(bufferTargetToGL[target], offset, getDirectBuffer(size, (FloatBuffer) buffer));
			}
			else
			{
				throw new System.ArgumentException();
			}
		}

		public override void enableClientState(int type)
		{
			GL11.glEnableClientState(clientStateToGL[type]);
		}

		public override void enableVertexAttribArray(int id)
		{
			GL20.glEnableVertexAttribArray(id);
		}

		public override void disableClientState(int type)
		{
			GL11.glDisableClientState(clientStateToGL[type]);
		}

		public override void disableVertexAttribArray(int id)
		{
			GL20.glDisableVertexAttribArray(id);
		}

		public override void setColorPointer(int size, int type, int stride, long offset)
		{
			GL11.glColorPointer(size, pointerTypeToGL[type], stride, offset);
		}

		public override void setColorPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			switch (type)
			{
				case IRenderingEngine_Fields.RE_FLOAT:
					GL11.glColorPointer(size, stride, getDirectFloatBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_BYTE:
					GL11.glColorPointer(size, false, stride, getDirectByteBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_UNSIGNED_BYTE:
					GL11.glColorPointer(size, true, stride, getDirectByteBuffer(bufferSize, buffer, 0));
					break;
				default:
					throw new System.ArgumentException();
			}
		}

		public override void setNormalPointer(int type, int stride, long offset)
		{
			GL11.glNormalPointer(pointerTypeToGL[type], stride, offset);
		}

		public override void setNormalPointer(int type, int stride, int bufferSize, Buffer buffer)
		{
			switch (type)
			{
				case IRenderingEngine_Fields.RE_FLOAT:
					GL11.glNormalPointer(stride, getDirectFloatBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_BYTE:
					GL11.glNormalPointer(stride, getDirectByteBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_INT:
					GL11.glNormalPointer(stride, getDirectIntBuffer(bufferSize, buffer, 0));
					break;
				default:
					throw new System.ArgumentException();
			}
		}

		public override void setTexCoordPointer(int size, int type, int stride, long offset)
		{
			GL11.glTexCoordPointer(size, pointerTypeToGL[type], stride, offset);
		}

		public override void setTexCoordPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			switch (type)
			{
				case IRenderingEngine_Fields.RE_FLOAT:
					GL11.glTexCoordPointer(size, stride, getDirectFloatBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_SHORT:
					GL11.glTexCoordPointer(size, stride, getDirectShortBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_INT:
					GL11.glTexCoordPointer(size, stride, getDirectIntBuffer(bufferSize, buffer, 0));
					break;
				default:
					throw new System.ArgumentException();
			}
		}

		public override void setVertexPointer(int size, int type, int stride, long offset)
		{
			GL11.glVertexPointer(size, pointerTypeToGL[type], stride, offset);
		}

		public override void setVertexPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			switch (type)
			{
				case IRenderingEngine_Fields.RE_FLOAT:
					GL11.glVertexPointer(size, stride, getDirectFloatBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_SHORT:
					GL11.glVertexPointer(size, stride, getDirectShortBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_INT:
					GL11.glVertexPointer(size, stride, getDirectIntBuffer(bufferSize, buffer, 0));
					break;
				default:
					throw new System.ArgumentException();
			}
		}

		public override void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, long offset)
		{
			GL20.glVertexAttribPointer(id, size, pointerTypeToGL[type], normalized, stride, offset);
		}

		public override void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, int bufferSize, Buffer buffer)
		{
			switch (type)
			{
				case IRenderingEngine_Fields.RE_FLOAT:
					GL20.glVertexAttribPointer(id, size, false, stride, getDirectFloatBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_SHORT:
					GL20.glVertexAttribPointer(id, size, false, false, stride, getDirectShortBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_UNSIGNED_SHORT:
					GL20.glVertexAttribPointer(id, size, true, false, stride, getDirectShortBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_INT:
					GL20.glVertexAttribPointer(id, size, false, false, stride, getDirectIntBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_UNSIGNED_INT:
					GL20.glVertexAttribPointer(id, size, true, false, stride, getDirectIntBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_BYTE:
					GL20.glVertexAttribPointer(id, size, false, false, stride, getDirectByteBuffer(bufferSize, buffer, 0));
					break;
				case IRenderingEngine_Fields.RE_UNSIGNED_BYTE:
					GL20.glVertexAttribPointer(id, size, true, false, stride, getDirectByteBuffer(bufferSize, buffer, 0));
					break;
				default:
					throw new System.ArgumentException();
			}
		}

		public override void setPixelStore(int rowLength, int alignment)
		{
			if (!VideoEngine.Instance.UsexBRZFilter)
			{
				GL11.glPixelStorei(GL11.GL_UNPACK_ROW_LENGTH, rowLength);
			}
			GL11.glPixelStorei(GL11.GL_UNPACK_ALIGNMENT, alignment);
			GL11.glPixelStorei(GL11.GL_PACK_ROW_LENGTH, rowLength);
			GL11.glPixelStorei(GL11.GL_PACK_ALIGNMENT, alignment);
		}

		public override int genTexture()
		{
			return GL11.glGenTextures();
		}

		public override void bindTexture(int texture)
		{
			GL11.glBindTexture(GL11.GL_TEXTURE_2D, texture);
		}

		public override void deleteTexture(int texture)
		{
			GL11.glDeleteTextures(texture);
		}

		public override void setCompressedTexImage(int level, int internalFormat, int width, int height, int compressedSize, Buffer buffer)
		{
			GL13.glCompressedTexImage2D(GL11.GL_TEXTURE_2D, level, textureInternalFormatToGL[internalFormat], width, height, 0, getDirectByteBuffer(compressedSize, buffer, 0));
		}

		public override void setTexImagexBRZ(int level, int internalFormat, int width, int height, int bufwidth, int format, int type, int textureSize, Buffer buffer)
		{
			if (buffer == null)
			{
				GL11.glTexImage2D(GL11.GL_TEXTURE_2D, level, textureInternalFormatToGL[internalFormat], width, height, 0, textureFormatToGL[format], textureTypeToGL[type], getDirectBuffer(textureSize, (ByteBuffer) buffer));
			}
			else if (buffer is ByteBuffer)
			{
				if (bufwidth != -1)
				{
					ByteBuffer tmpbuf = DirectBufferUtilities.getDirectBuffer(textureSize, (ByteBuffer) buffer);
					int Length = tmpbuf.remaining();
					sbyte[] buf = new sbyte[Length];
					tmpbuf.get(buf);
					XBRZNativeFilter.ScaleandSetTexImage(2, buf, level, textureInternalFormatToGL[internalFormat], width, height, bufwidth, textureFormatToGL[format], textureTypeToGL[type]);
				}
				else
				{
					GL11.glTexImage2D(GL11.GL_TEXTURE_2D, level, textureInternalFormatToGL[internalFormat], width, height, 0, textureFormatToGL[format], textureTypeToGL[type], getDirectBuffer(textureSize, (ByteBuffer) buffer));
				}
			}
			else if (buffer is IntBuffer)
			{
				if (bufwidth != -1)
				{
					IntBuffer tmpbuf = DirectBufferUtilities.getDirectBuffer(textureSize, (IntBuffer) buffer);
					int Length = tmpbuf.remaining();
					int[] buf = new int[Length];
					tmpbuf.get(buf);
					XBRZNativeFilter.ScaleandSetTexImage(2, buf, level, textureInternalFormatToGL[internalFormat], width, height, bufwidth, textureFormatToGL[format], textureTypeToGL[type]);
				}
				else
				{
					GL11.glTexImage2D(GL11.GL_TEXTURE_2D, level, textureInternalFormatToGL[internalFormat], width, height, 0, textureFormatToGL[format], textureTypeToGL[type], getDirectBuffer(textureSize, (IntBuffer) buffer));
				}
			}
			else if (buffer is ShortBuffer)
			{
				if (bufwidth != -1)
				{
					ShortBuffer tmpbuf = DirectBufferUtilities.getDirectBuffer(textureSize, (ShortBuffer) buffer);
					int Length = tmpbuf.remaining();
					short[] buf = new short[Length];
					tmpbuf.get(buf);
					XBRZNativeFilter.ScaleandSetTexImage(2, buf, level, textureInternalFormatToGL[internalFormat], width, height, bufwidth, textureFormatToGL[format], textureTypeToGL[type]);
				}
				else
				{
					GL11.glTexImage2D(GL11.GL_TEXTURE_2D, level, textureInternalFormatToGL[internalFormat], width, height, 0, textureFormatToGL[format], textureTypeToGL[type], getDirectBuffer(textureSize, (ShortBuffer) buffer));
				}
			}
			else if (buffer is FloatBuffer)
			{
				if (bufwidth != -1)
				{
					FloatBuffer tmpbuf = DirectBufferUtilities.getDirectBuffer(textureSize, (FloatBuffer) buffer);
					int Length = tmpbuf.remaining();
					float[] buf = new float[Length];
					tmpbuf.get(buf);
					XBRZNativeFilter.ScaleandSetTexImage(2, buf, level, textureInternalFormatToGL[internalFormat], width, height, bufwidth, textureFormatToGL[format], textureTypeToGL[type]);
				}
				else
				{
					GL11.glTexImage2D(GL11.GL_TEXTURE_2D, level, textureInternalFormatToGL[internalFormat], width, height, 0, textureFormatToGL[format], textureTypeToGL[type], getDirectBuffer(textureSize, (FloatBuffer) buffer));
				}
			}
			else
			{
				throw new System.ArgumentException();
			}
		}

		public override void setTexImage(int level, int internalFormat, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
			if (buffer is ByteBuffer || buffer == null)
			{
				GL11.glTexImage2D(GL11.GL_TEXTURE_2D, level, textureInternalFormatToGL[internalFormat], width, height, 0, textureFormatToGL[format], textureTypeToGL[type], getDirectBuffer(textureSize, (ByteBuffer) buffer));
			}
			else if (buffer is IntBuffer)
			{
				GL11.glTexImage2D(GL11.GL_TEXTURE_2D, level, textureInternalFormatToGL[internalFormat], width, height, 0, textureFormatToGL[format], textureTypeToGL[type], getDirectBuffer(textureSize, (IntBuffer) buffer));
			}
			else if (buffer is ShortBuffer)
			{
				GL11.glTexImage2D(GL11.GL_TEXTURE_2D, level, textureInternalFormatToGL[internalFormat], width, height, 0, textureFormatToGL[format], textureTypeToGL[type], getDirectBuffer(textureSize, (ShortBuffer) buffer));
			}
			else if (buffer is FloatBuffer)
			{
				GL11.glTexImage2D(GL11.GL_TEXTURE_2D, level, textureInternalFormatToGL[internalFormat], width, height, 0, textureFormatToGL[format], textureTypeToGL[type], getDirectBuffer(textureSize, (FloatBuffer) buffer));
			}
			else
			{
				throw new System.ArgumentException();
			}
		}

		public override void setTexSubImage(int level, int xOffset, int yOffset, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
			if (buffer is ByteBuffer || buffer == null)
			{
				GL11.glTexSubImage2D(GL11.GL_TEXTURE_2D, level, xOffset, yOffset, width, height, textureFormatToGL[format], textureTypeToGL[type], getDirectBuffer(textureSize, (ByteBuffer) buffer));
			}
			else if (buffer is IntBuffer)
			{
				GL11.glTexSubImage2D(GL11.GL_TEXTURE_2D, level, xOffset, yOffset, width, height, textureFormatToGL[format], textureTypeToGL[type], getDirectBuffer(textureSize, (IntBuffer) buffer));
			}
			else if (buffer is ShortBuffer)
			{
				GL11.glTexSubImage2D(GL11.GL_TEXTURE_2D, level, xOffset, yOffset, width, height, textureFormatToGL[format], textureTypeToGL[type], getDirectBuffer(textureSize, (ShortBuffer) buffer));
			}
			else if (buffer is FloatBuffer)
			{
				GL11.glTexSubImage2D(GL11.GL_TEXTURE_2D, level, xOffset, yOffset, width, height, textureFormatToGL[format], textureTypeToGL[type], getDirectBuffer(textureSize, (FloatBuffer) buffer));
			}
			else
			{
				throw new System.ArgumentException();
			}
		}

		public override void setStencilOp(int fail, int zfail, int zpass)
		{
			GL11.glStencilOp(stencilOpToGL[fail], stencilOpToGL[zfail], stencilOpToGL[zpass]);
		}

		public override void setStencilFunc(int func, int @ref, int mask)
		{
			GL11.glStencilFunc(stencilFuncToGL[func], @ref, mask);
		}

		public override void setAlphaFunc(int func, int @ref, int mask)
		{
			// mask is not supported by OpenGL
			GL11.glAlphaFunc(alphaFuncToGL[func], @ref / 255.0f);
		}

		public override float[] FogColor
		{
			set
			{
				GL11.glFog(GL11.GL_FOG_COLOR, getDirectBuffer(value));
			}
		}

		public override void setFogDist(float start, float end)
		{
			GL11.glFogf(GL11.GL_FOG_START, start);
			GL11.glFogf(GL11.GL_FOG_END, end);
		}

		public override float[] TextureEnvColor
		{
			set
			{
				GL11.glTexEnv(GL11.GL_TEXTURE_ENV, GL11.GL_TEXTURE_ENV_COLOR, getDirectBuffer(value));
			}
		}

		public override bool FrontFace
		{
			set
			{
				GL11.glFrontFace(value ? GL11.GL_CW : GL11.GL_CCW);
			}
		}

		public override void setScissor(int x, int y, int width, int height)
		{
			GL11.glScissor(x, y, width, height);
		}

		public override int BlendEquation
		{
			set
			{
				try
				{
					GL14.glBlendEquation(blendModeToGL[value]);
				}
				catch (System.InvalidOperationException e)
				{
					Console.WriteLine("VideoEngine: " + e.Message);
				}
			}
		}

		public override void setFogHint()
		{
			GL11.glFogi(GL11.GL_FOG_MODE, GL11.GL_LINEAR);
			GL11.glHint(GL11.GL_FOG_HINT, GL11.GL_DONT_CARE);
		}

		public override void setLineSmoothHint()
		{
			GL11.glHint(GL11.GL_LINE_SMOOTH_HINT, GL11.GL_NICEST);
		}

		public override float MaterialShininess
		{
			set
			{
				GL11.glMaterialf(GL11.GL_FRONT, GL11.GL_SHININESS, value);
			}
		}

		public override void beginQuery(int id)
		{
			GL15.glBeginQuery(GL15.GL_SAMPLES_PASSED, id);
		}

		public override void endQuery()
		{
			GL15.glEndQuery(GL15.GL_SAMPLES_PASSED);
		}

		public override int genQuery()
		{
			return GL15.glGenQueries();
		}

		public override bool BoundingBoxVisible
		{
			get
			{
				return true;
			}
		}

		public override bool getQueryResultAvailable(int id)
		{
			// 0 means result not yet available, 1 means result available
			return GL15.glGetQueryObjecti(id, GL15.GL_QUERY_RESULT_AVAILABLE) != 0;
		}

		public override int getQueryResult(int id)
		{
			return GL15.glGetQueryObjecti(id, GL15.GL_QUERY_RESULT);
		}

		public override void copyTexSubImage(int level, int xOffset, int yOffset, int x, int y, int width, int height)
		{
			GL11.glCopyTexSubImage2D(GL11.GL_TEXTURE_2D, level, xOffset, yOffset, x, y, width, height);
		}

		public override void getTexImage(int level, int format, int type, Buffer buffer)
		{
			if (buffer is ByteBuffer)
			{
				ByteBuffer directBuffer = allocateDirectBuffer((ByteBuffer) buffer);
				GL11.glGetTexImage(GL11.GL_TEXTURE_2D, level, textureFormatToGL[format], textureTypeToGL[type], (ByteBuffer) buffer);
				copyBuffer((ByteBuffer) buffer, directBuffer);
			}
			else if (buffer is IntBuffer)
			{
				IntBuffer directBuffer = allocateDirectBuffer((IntBuffer) buffer);
				GL11.glGetTexImage(GL11.GL_TEXTURE_2D, level, textureFormatToGL[format], textureTypeToGL[type], directBuffer);
				copyBuffer((IntBuffer) buffer, directBuffer);
			}
			else if (buffer is ShortBuffer)
			{
				ShortBuffer directBuffer = allocateDirectBuffer((ShortBuffer) buffer);
				GL11.glGetTexImage(GL11.GL_TEXTURE_2D, level, textureFormatToGL[format], textureTypeToGL[type], (ShortBuffer) buffer);
				copyBuffer((ShortBuffer) buffer, directBuffer);
			}
			else if (buffer is FloatBuffer)
			{
				FloatBuffer directBuffer = allocateDirectBuffer((FloatBuffer) buffer);
				GL11.glGetTexImage(GL11.GL_TEXTURE_2D, level, textureFormatToGL[format], textureTypeToGL[type], (FloatBuffer) buffer);
				copyBuffer((FloatBuffer) buffer, directBuffer);
			}
			else
			{
				throw new System.ArgumentException();
			}
		}

		public override void clear(float red, float green, float blue, float alpha)
		{
			GL11.glClearColor(red, green, blue, alpha);
			GL11.glClear(GL11.GL_COLOR_BUFFER_BIT);
		}

		public override bool canAllNativeVertexInfo()
		{
			return false;
		}

		public override bool canNativeSpritesPrimitive()
		{
			return false;
		}

		public override void setProgramParameter(int program, int parameter, int value)
		{
			if (parameter == IRenderingEngine_Fields.RE_GEOMETRY_INPUT_TYPE || parameter == IRenderingEngine_Fields.RE_GEOMETRY_OUTPUT_TYPE)
			{
				value = primitiveToGL[value];
			}
			ARBGeometryShader4.glProgramParameteriARB(program, programParameterToGL[parameter], value);
		}

		public override bool QueryAvailable
		{
			get
			{
				// glGenQueries is available only if the GL version is 1.5 or greater
				return GLContext.Capabilities.OpenGL15;
			}
		}

		public override bool ShaderAvailable
		{
			get
			{
				// glCreateShader is available only if the GL version is 2.0 or greater
				return GLContext.Capabilities.OpenGL20;
			}
		}

		public override void bindBuffer(int target, int buffer)
		{
			ARBBufferObject.glBindBufferARB(bufferTargetToGL[target], buffer);
		}

		public override void bindBufferBase(int target, int bindingPoint, int buffer)
		{
			ARBUniformBufferObject.glBindBufferBase(bufferTargetToGL[target], bindingPoint, buffer);
		}

		public override int getUniformBlockIndex(int program, string name)
		{
			return ARBUniformBufferObject.glGetUniformBlockIndex(program, name);
		}

		public override void setUniformBlockBinding(int program, int blockIndex, int bindingPoint)
		{
			ARBUniformBufferObject.glUniformBlockBinding(program, blockIndex, bindingPoint);
		}

		public override int getUniformIndex(int program, string name)
		{
			IntBuffer indicesBuffer = DirectBufferUtilities.allocateDirectBuffer(4).asIntBuffer();
			ARBUniformBufferObject.glGetUniformIndices(program, new string[]{name}, indicesBuffer);
			return indicesBuffer.get(0);
		}

		public override int[] getUniformIndices(int program, string[] names)
		{
			IntBuffer indicesBuffer = DirectBufferUtilities.allocateDirectBuffer(names.Length << 2).asIntBuffer();
			ARBUniformBufferObject.glGetUniformIndices(program, names, indicesBuffer);
			int[] indices = new int[names.Length];
			indicesBuffer.get(indices);
			return indices;
		}

		public override int getActiveUniformOffset(int program, int uniformIndex)
		{
			return ARBUniformBufferObject.glGetActiveUniforms(program, uniformIndex, ARBUniformBufferObject.GL_UNIFORM_OFFSET);
		}

		public override float[] ProjectionMatrix
		{
			set
			{
				re.MatrixMode = IRenderingEngine_Fields.GU_PROJECTION;
				re.Matrix = value;
			}
		}

		public override float[] ViewMatrix
		{
			set
			{
				// The View matrix has always to be set BEFORE the Model matrix
				re.MatrixMode = IRenderingEngine_Fields.RE_MODELVIEW;
				Matrix = value;
			}
		}

		public override float[] ModelMatrix
		{
			set
			{
				// The Model matrix has always to be set AFTER the View matrix
				re.MatrixMode = IRenderingEngine_Fields.RE_MODELVIEW;
				re.multMatrix(value);
			}
		}

		public override float[] TextureMatrix
		{
			set
			{
				re.MatrixMode = IRenderingEngine_Fields.GU_TEXTURE;
				re.Matrix = value;
			}
		}

		public override float[] ModelViewMatrix
		{
			set
			{
				re.MatrixMode = IRenderingEngine_Fields.RE_MODELVIEW;
				Matrix = value;
			}
		}

		public override float[] Matrix
		{
			set
			{
				if (value != null)
				{
					GL11.glLoadMatrix(getDirectBuffer(value));
				}
				else
				{
					GL11.glLoadIdentity();
				}
			}
		}

		public override int MatrixMode
		{
			set
			{
				GL11.glMatrixMode(matrixModeToGL[value]);
			}
		}

		public override void multMatrix(float[] values)
		{
			if (values != null)
			{
				GL11.glMultMatrix(getDirectBuffer(values));
			}
		}

		public override int genFramebuffer()
		{
			return ARBFramebufferObject.glGenFramebuffers();
		}

		public override void bindFramebuffer(int target, int framebuffer)
		{
			ARBFramebufferObject.glBindFramebuffer(framebufferTargetToGL[target], framebuffer);
		}

		public override void bindRenderbuffer(int renderbuffer)
		{
			ARBFramebufferObject.glBindRenderbuffer(ARBFramebufferObject.GL_RENDERBUFFER, renderbuffer);
		}

		public override void deleteFramebuffer(int framebuffer)
		{
			ARBFramebufferObject.glDeleteFramebuffers(framebuffer);
		}

		public override void deleteRenderbuffer(int renderbuffer)
		{
			ARBFramebufferObject.glDeleteRenderbuffers(renderbuffer);
		}

		public override int genRenderbuffer()
		{
			return ARBFramebufferObject.glGenRenderbuffers();
		}

		public override void setFramebufferRenderbuffer(int target, int attachment, int renderbuffer)
		{
			ARBFramebufferObject.glFramebufferRenderbuffer(framebufferTargetToGL[target], attachmentToGL[attachment], ARBFramebufferObject.GL_RENDERBUFFER, renderbuffer);
		}

		public override void setRenderbufferStorage(int internalFormat, int width, int height)
		{
			ARBFramebufferObject.glRenderbufferStorage(ARBFramebufferObject.GL_RENDERBUFFER, textureInternalFormatToGL[internalFormat], width, height);
		}

		public override void setFramebufferTexture(int target, int attachment, int texture, int level)
		{
			ARBFramebufferObject.glFramebufferTexture2D(framebufferTargetToGL[target], attachmentToGL[attachment], GL11.GL_TEXTURE_2D, texture, level);
		}

		public override bool FramebufferObjectAvailable
		{
			get
			{
				return GLContext.Capabilities.GL_ARB_framebuffer_object;
			}
		}

		public override void bindVertexArray(int id)
		{
			ARBVertexArrayObject.glBindVertexArray(id);
		}

		public override void deleteVertexArray(int id)
		{
			ARBVertexArrayObject.glDeleteVertexArrays(id);
		}

		public override int genVertexArray()
		{
			return ARBVertexArrayObject.glGenVertexArrays();
		}

		public override bool VertexArrayAvailable
		{
			get
			{
				return GLContext.Capabilities.GL_ARB_vertex_array_object;
			}
		}

		public override void multiDrawArrays(int primitive, IntBuffer first, IntBuffer count)
		{
			// "first" and "count" have to be direct buffers
			//GL14.glMultiDrawArrays(primitive, first, count);
			EXTMultiDrawArrays.glMultiDrawArraysEXT(primitive, first, count);
		}

		public override void drawArraysBurstMode(int primitive, int first, int count)
		{
			drawArrays(primitive, first, count);
		}

		public override void setPixelTransfer(int parameter, int value)
		{
			GL11.glPixelTransferi(pixelTransferToGL[parameter], value);
		}

		public override void setPixelTransfer(int parameter, float value)
		{
			GL11.glPixelTransferf(pixelTransferToGL[parameter], value);
		}

		public override void setPixelTransfer(int parameter, bool value)
		{
			GL11.glPixelTransferi(pixelTransferToGL[parameter], value ? GL11.GL_TRUE : GL11.GL_FALSE);
		}

		public override void setPixelMap(int map, int mapSize, Buffer buffer)
		{
			if (buffer is IntBuffer)
			{
				GL11.glPixelMapu(pixelMapToGL[map], DirectBufferUtilities.getDirectBuffer(mapSize, (IntBuffer) buffer));
			}
			else if (buffer is FloatBuffer)
			{
				GL11.glPixelMap(pixelMapToGL[map], DirectBufferUtilities.getDirectBuffer(mapSize, (FloatBuffer) buffer));
			}
			else if (buffer is ShortBuffer)
			{
				GL11.glPixelMapu(pixelMapToGL[map], DirectBufferUtilities.getDirectBuffer(mapSize, (ShortBuffer) buffer));
			}
			else
			{
				throw new System.ArgumentException();
			}
		}

		public override bool canNativeClut(int textureAddress, int pixelFormat, bool textureSwizzle)
		{
			if (vendorIntel)
			{
				// Intel driver: OpenGL texImage2D with GL_R8UI doesn't work. See the bug report:
				// https://software.intel.com/en-us/forums/graphics-driver-bug-reporting/topic/748843
				if (pixelFormat == TPSM_PIXEL_STORAGE_MODE_4BIT_INDEXED || pixelFormat == TPSM_PIXEL_STORAGE_MODE_8BIT_INDEXED)
				{
					return false;
				}
			}

			// Requires at least OpenGL 3.0
			return hasOpenGL30;
		}

		public override int ActiveTexture
		{
			set
			{
				GL13.glActiveTexture(GL13.GL_TEXTURE0 + value);
			}
		}

		public override float MaxTextureAnisotropy
		{
			get
			{
				return GL11.glGetFloat(EXTTextureFilterAnisotropic.GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT);
			}
		}

		public override float TextureAnisotropy
		{
			set
			{
				GL11.glTexParameterf(GL11.GL_TEXTURE_2D, EXTTextureFilterAnisotropic.GL_TEXTURE_MAX_ANISOTROPY_EXT, value);
			}
		}

		public override string ShadingLanguageVersion
		{
			get
			{
				if (GLContext.Capabilities.OpenGL20)
				{
					return GL11.glGetString(GL20.GL_SHADING_LANGUAGE_VERSION);
				}
    
				return null;
			}
		}

		public override bool canReadAllVertexInfo()
		{
			return false;
		}

		public override void readStencil(int x, int y, int width, int height, int bufferSize, Buffer buffer)
		{
			if (buffer is IntBuffer)
			{
				GL11.glReadPixels(x, y, width, height, GL11.GL_STENCIL_INDEX, GL11.GL_UNSIGNED_BYTE, DirectBufferUtilities.getDirectBuffer(bufferSize, (IntBuffer) buffer));
			}
			else if (buffer is FloatBuffer)
			{
				GL11.glReadPixels(x, y, width, height, GL11.GL_STENCIL_INDEX, GL11.GL_UNSIGNED_BYTE, DirectBufferUtilities.getDirectBuffer(bufferSize, (FloatBuffer) buffer));
			}
			else if (buffer is ShortBuffer)
			{
				GL11.glReadPixels(x, y, width, height, GL11.GL_STENCIL_INDEX, GL11.GL_UNSIGNED_BYTE, DirectBufferUtilities.getDirectBuffer(bufferSize, (ShortBuffer) buffer));
			}
			else if (buffer is ByteBuffer)
			{
				GL11.glReadPixels(x, y, width, height, GL11.GL_STENCIL_INDEX, GL11.GL_UNSIGNED_BYTE, DirectBufferUtilities.getDirectBuffer(bufferSize, (ByteBuffer) buffer));
			}
			else
			{
				throw new System.ArgumentException();
			}
		}

		public override void blitFramebuffer(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, int mask, int filter)
		{
			int maskGL = 0;
			for (int i = 0; i < buffersMaskToGL.Length; i++, mask >>= 1)
			{
				if ((mask & 1) != 0)
				{
					maskGL |= buffersMaskToGL[i];
				}
			}
			GL30.glBlitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, maskGL, mipmapFilterToGL[filter]);
		}

		public override bool checkAndLogErrors(string logComment)
		{
			bool hasError = false;
			while (true)
			{
				int error;
				try
				{
					error = GL11.glGetError();
				}
				catch (System.NullReferenceException)
				{
					// Ignore Exception
					error = GL11.GL_NO_ERROR;
				}

				if (error == GL11.GL_NO_ERROR)
				{
					break;
				}

				hasError = true;
				if (!string.ReferenceEquals(logComment, null))
				{
					string errorComment;
					switch (error)
					{
						case GL11.GL_INVALID_ENUM:
							errorComment = "GL_INVALID_ENUM";
							break;
						case GL11.GL_INVALID_OPERATION:
							errorComment = "GL_INVALID_OPERATION";
							break;
						case GL11.GL_INVALID_VALUE:
							errorComment = "GL_INVALID_VALUE";
							break;
						default:
							errorComment = string.Format("0x{0:X}", error);
							break;
					}

					// Build a stack trace and exclude uninteresting RE stack elements:
					// - exclude this method (first stack trace element)
					// - exclude method checkAndLogErrors
					// - exclude methods from class BaseRenderingEngineProxy
					StackTraceElement[] stackTrace = (new Exception()).StackTrace;
					StringBuilder stackTraceLog = new StringBuilder();
					int count = 0;
					for (int i = 1; i < stackTrace.Length && count < 6; i++)
					{
						string className = stackTrace[i].ClassName;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						if (!typeof(BaseRenderingEngineProxy).FullName.Equals(className) && !typeof(CheckErrorsProxy).FullName.Equals(className))
						{
							stackTraceLog.Append(stackTrace[i]);
							stackTraceLog.Append("\n");
							count++;
						}
					}

					Console.WriteLine(string.Format("Error {0}: {1}\n{2}", logComment, errorComment, stackTraceLog.ToString()));
				}
			}

			return hasError;
		}

		public override bool setCopyRedToAlpha(bool shaderCopyRedToAlpha)
		{
			return true;
		}

		public override void drawElements(int primitive, int count, int indexType, Buffer indices, int indicesOffset)
		{
			switch (indexType)
			{
				case IRenderingEngine_Fields.RE_UNSIGNED_BYTE:
					GL11.glDrawElements(primitiveToGL[primitive], DirectBufferUtilities.getDirectByteBuffer(count, indices, indicesOffset));
					break;
				case IRenderingEngine_Fields.RE_UNSIGNED_SHORT:
					GL11.glDrawElements(primitiveToGL[primitive], DirectBufferUtilities.getDirectShortBuffer(count << 1, indices, indicesOffset));
					break;
				case IRenderingEngine_Fields.RE_UNSIGNED_INT:
					GL11.glDrawElements(primitiveToGL[primitive], DirectBufferUtilities.getDirectIntBuffer(count << 2, indices, indicesOffset));
					break;
				default:
					Console.WriteLine(string.Format("drawElements unknown indexType={0:D}", indexType));
					break;
			}
		}

		public override void drawElements(int primitive, int count, int indexType, long indicesOffset)
		{
			GL11.glDrawElements(primitiveToGL[primitive], count, pointerTypeToGL[indexType], indicesOffset);
		}

		public override void drawElementsBurstMode(int primitive, int count, int indexType, long indicesOffset)
		{
			drawElements(primitive, count, indexType, indicesOffset);
		}

		public override void textureBarrier()
		{
			NVTextureBarrier.glTextureBarrierNV();
		}

		public override bool TextureBarrierAvailable
		{
			get
			{
				return GLContext.Capabilities.GL_NV_texture_barrier;
			}
		}
	}

}