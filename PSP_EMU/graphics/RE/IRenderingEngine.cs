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

	using IREBufferManager = pspsharp.graphics.RE.buffer.IREBufferManager;

	/// <summary>
	/// @author gid15
	/// 
	/// The interface for a RenderingEngine pipeline elements.
	/// </summary>
	public interface IRenderingEngine
	{

		// Additional Texture types

		// Flags:

		// Primitive types:

		// Matrix modes:

		// Shade models:

		// Color types:

		// Light modes:

		// Blend functions:

		// setTexEnv names:

		// setTexEnv params:
		// values [0..4] are TFUNC_FRAGMENT_DOUBLE_TEXTURE_EFECT_xxx

		// Shader types:

		// Client State types:

		// Pointer types:

		// Buffer usage:

		// Program parameters

		// Buffer Target

		// Framebuffer Target

		// Framebuffer Attachment

		// Pixel Transfer parameter

		// Pixel map

		// Clut Index Hint

		// Buffers flag

		IRenderingEngine RenderingEngine {set;}
		GeContext GeContext {set;}
		void exit();
		void startDirectRendering(bool textureEnabled, bool depthWriteEnabled, bool colorWriteEnabled, bool setOrthoMatrix, bool orthoInverted, int width, int height);
		void endDirectRendering();
		void startDisplay();
		void endDisplay();
		void enableFlag(int flag);
		void disableFlag(int flag);
		void setMorphWeight(int index, float value);
		void setPatchDiv(int s, int t);
		int PatchPrim {set;}
		int MatrixMode {set;}
		float[] Matrix {set;}
		void multMatrix(float[] values);
		float[] ProjectionMatrix {set;}
		float[] ViewMatrix {set;}
		float[] ModelMatrix {set;}
		float[] ModelViewMatrix {set;}
		float[] TextureMatrix {set;}
		void endModelViewMatrixUpdate();
		void setViewport(int x, int y, int width, int height);
		void setDepthRange(float zpos, float zscale, int near, int far);
		int DepthFunc {set;}
		int ShadeModel {set;}
		float[] MaterialEmissiveColor {set;}
		float[] MaterialAmbientColor {set;}
		float[] MaterialDiffuseColor {set;}
		float[] MaterialSpecularColor {set;}
		float MaterialShininess {set;}
		float[] LightModelAmbientColor {set;}
		int LightMode {set;}
		void setLightPosition(int light, float[] position);
		void setLightDirection(int light, float[] direction);
		void setLightSpotExponent(int light, float exponent);
		void setLightSpotCutoff(int light, float cutoff);
		void setLightConstantAttenuation(int light, float constant);
		void setLightLinearAttenuation(int light, float linear);
		void setLightQuadraticAttenuation(int light, float quadratic);
		void setLightAmbientColor(int light, float[] color);
		void setLightDiffuseColor(int light, float[] color);
		void setLightSpecularColor(int light, float[] color);
		void setLightType(int light, int type, int kind);
		void setBlendFunc(int src, int dst);
		float[] BlendColor {set;}
		int LogicOp {set;}
		bool DepthMask {set;}
		void setColorMask(int redMask, int greenMask, int blueMask, int alphaMask);
		void setColorMask(bool redWriteEnabled, bool greenWriteEnabled, bool blueWriteEnabled, bool alphaWriteEnabled);
		void setTextureWrapMode(int s, int t);
		int TextureMipmapMinLevel {set;}
		int TextureMipmapMaxLevel {set;}
		int TextureMipmapMinFilter {set;}
		int TextureMipmapMagFilter {set;}
		void setColorMaterial(bool ambient, bool diffuse, bool specular);
		void setTextureMapMode(int mode, int proj);
		void setTextureEnvironmentMapping(int u, int v);
		float[] VertexColor {set;}
		void setUniform(int id, int value);
		void setUniform(int id, int value1, int value2);
		void setUniform(int id, float value);
		void setUniform2(int id, int[] values);
		void setUniform3(int id, int[] values);
		void setUniform3(int id, float[] values);
		void setUniform4(int id, int[] values);
		void setUniform4(int id, float[] values);
		void setUniformMatrix4(int id, int count, float[] values);
		int ColorTestFunc {set;}
		int[] ColorTestReference {set;}
		int[] ColorTestMask {set;}
		void setTextureFunc(int func, bool alphaUsed, bool colorDoubled);
		int setBones(int count, float[] values);
		void setTexEnv(int name, int param);
		void setTexEnv(int name, float param);
		void startClearMode(bool color, bool stencil, bool depth);
		void endClearMode();
		int createShader(int type);
		bool compilerShader(int shader, string source);
		int createProgram();
		void useProgram(int program);
		void attachShader(int program, int shader);
		bool linkProgram(int program);
		bool validateProgram(int program);
		int getUniformLocation(int program, string name);
		int getAttribLocation(int program, string name);
		void bindAttribLocation(int program, int index, string name);
		string getShaderInfoLog(int shader);
		string getProgramInfoLog(int program);
		bool isExtensionAvailable(string name);
		void drawArrays(int primitive, int first, int count);
		void drawElements(int primitive, int count, int indexType, Buffer indices, int indicesOffset);
		void drawElements(int primitive, int count, int indexType, long indicesOffset);
		int genBuffer();
		void deleteBuffer(int buffer);
		void setBufferData(int target, int size, Buffer buffer, int usage);
		void setBufferSubData(int target, int offset, int size, Buffer buffer);
		void bindBuffer(int target, int buffer);
		void enableClientState(int type);
		void disableClientState(int type);
		void enableVertexAttribArray(int id);
		void disableVertexAttribArray(int id);
		void setTexCoordPointer(int size, int type, int stride, long offset);
		void setTexCoordPointer(int size, int type, int stride, int bufferSize, Buffer buffer);
		void setColorPointer(int size, int type, int stride, long offset);
		void setColorPointer(int size, int type, int stride, int bufferSize, Buffer buffer);
		void setVertexPointer(int size, int type, int stride, long offset);
		void setVertexPointer(int size, int type, int stride, int bufferSize, Buffer buffer);
		void setNormalPointer(int type, int stride, long offset);
		void setNormalPointer(int type, int stride, int bufferSize, Buffer buffer);
		void setWeightPointer(int size, int type, int stride, long offset);
		void setWeightPointer(int size, int type, int stride, int bufferSize, Buffer buffer);
		void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, long offset);
		void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, int bufferSize, Buffer buffer);
		void setPixelStore(int rowLength, int alignment);
		int genTexture();
		void bindTexture(int texture);
		void deleteTexture(int texture);
		void setCompressedTexImage(int level, int internalFormat, int width, int height, int compressedSize, Buffer buffer);
		void setTexImage(int level, int internalFormat, int width, int height, int format, int type, int textureSize, Buffer buffer);
		void setTexImagexBRZ(int level, int internalFormat, int width, int height, int bufwidth, int format, int type, int textureSize, Buffer buffer);
			void setTexSubImage(int level, int xOffset, int yOffset, int width, int height, int format, int type, int textureSize, Buffer buffer);
		void getTexImage(int level, int format, int type, Buffer buffer);
		void copyTexSubImage(int level, int xOffset, int yOffset, int x, int y, int width, int height);
		void setStencilOp(int fail, int zfail, int zpass);
		void setStencilFunc(int func, int @ref, int mask);
		void setAlphaFunc(int func, int @ref, int mask);
		void setFogHint();
		float[] FogColor {set;}
		void setFogDist(float end, float scale);
		float[] TextureEnvColor {set;}
		bool FrontFace {set;}
		void setScissor(int x, int y, int width, int height);
		int BlendEquation {set;}
		void setLineSmoothHint();
		void beginBoundingBox(int numberOfVertexBoundingBox);
		void drawBoundingBox(float[][] values);
		void endBoundingBox(VertexInfo vinfo);
		bool BoundingBoxVisible {get;}
		int genQuery();
		void beginQuery(int id);
		void endQuery();
		bool getQueryResultAvailable(int id);
		int getQueryResult(int id);
		void clear(float red, float green, float blue, float alpha);
		IREBufferManager BufferManager {get;}
		bool canAllNativeVertexInfo();
		bool canNativeSpritesPrimitive();
		void setVertexInfo(VertexInfo vinfo, bool allNativeVertexInfo, bool useVertexColor, bool useTexture, int type);
		void setProgramParameter(int program, int parameter, int value);
		bool QueryAvailable {get;}
		bool ShaderAvailable {get;}
		int getUniformBlockIndex(int program, string name);
		void bindBufferBase(int target, int bindingPoint, int buffer);
		void setUniformBlockBinding(int program, int blockIndex, int bindingPoint);
		int getUniformIndex(int program, string name);
		int[] getUniformIndices(int program, string[] names);
		int getActiveUniformOffset(int program, int uniformIndex);
		bool FramebufferObjectAvailable {get;}
		int genFramebuffer();
		int genRenderbuffer();
		void deleteFramebuffer(int framebuffer);
		void deleteRenderbuffer(int renderbuffer);
		void bindFramebuffer(int target, int framebuffer);
		void bindRenderbuffer(int renderbuffer);
		void setRenderbufferStorage(int internalFormat, int width, int height);
		void setFramebufferRenderbuffer(int target, int attachment, int renderbuffer);
		void setFramebufferTexture(int target, int attachment, int texture, int level);
		int genVertexArray();
		void bindVertexArray(int id);
		void deleteVertexArray(int id);
		bool VertexArrayAvailable {get;}
		void multiDrawArrays(int primitive, IntBuffer first, IntBuffer count);
		void drawArraysBurstMode(int primitive, int first, int count);
		void multiDrawElements(int primitive, IntBuffer first, IntBuffer count, int indexType, long indicesOffset);
		void drawElementsBurstMode(int primitive, int count, int indexType, long indicesOffset);
		void setPixelTransfer(int parameter, int value);
		void setPixelTransfer(int parameter, float value);
		void setPixelTransfer(int parameter, bool value);
		void setPixelMap(int map, int mapSize, Buffer buffer);
		bool canNativeClut(int textureAddress, int pixelFormat, bool textureSwizzle);
		int ActiveTexture {set;}
		void setTextureFormat(int pixelFormat, bool swizzle);
		void bindActiveTexture(int index, int texture);
		float TextureAnisotropy {set;}
		float MaxTextureAnisotropy {get;}
		string ShadingLanguageVersion {get;}
		void setBlendSFix(int sfix, float[] color);
		void setBlendDFix(int dfix, float[] color);
		void waitForRenderingCompletion();
		bool canReadAllVertexInfo();
		void readStencil(int x, int y, int width, int height, int bufferSize, Buffer buffer);
		void blitFramebuffer(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, int mask, int filter);
		bool checkAndLogErrors(string logComment);
		bool setCopyRedToAlpha(bool copyRedToAlpha);
		bool TextureBarrierAvailable {get;}
		void textureBarrier();
		bool canDiscardVertices();
		void setViewportPos(float x, float y, float z);
		void setViewportScale(float sx, float sy, float sz);
	}

	public static class IRenderingEngine_Fields
	{
		public static readonly int[] sizeOfTextureType = new int[] {2, 2, 2, 4, 0, 1, 2, 4, 0, 0, 0, 2, 2, 2, 4, 4, 4, 4};
		public static readonly int[] alignementOfTextureBufferWidth = new int[] {8, 8, 8, 4, 32, 16, 8, 4, 1, 1, 1, 8, 8, 8, 4, 4, 4, 4};
		public static readonly bool[] isTextureTypeIndexed = new bool[] {false, false, false, false, true, true, true, true, false, false, false, true, true, true, true, false, false, false};
		public const int RE_PIXEL_STORAGE_16BIT_INDEXED_BGR5650 = 11;
		public const int RE_PIXEL_STORAGE_16BIT_INDEXED_ABGR5551 = 12;
		public const int RE_PIXEL_STORAGE_16BIT_INDEXED_ABGR4444 = 13;
		public const int RE_PIXEL_STORAGE_32BIT_INDEXED_ABGR8888 = 14;
		public const int RE_DEPTH_COMPONENT = 15;
		public const int RE_STENCIL_INDEX = 16;
		public const int RE_DEPTH_STENCIL = 17;
		public const int GU_ALPHA_TEST = 0;
		public const int GU_DEPTH_TEST = 1;
		public const int GU_SCISSOR_TEST = 2;
		public const int GU_STENCIL_TEST = 3;
		public const int GU_BLEND = 4;
		public const int GU_CULL_FACE = 5;
		public const int GU_DITHER = 6;
		public const int GU_FOG = 7;
		public const int GU_CLIP_PLANES = 8;
		public const int GU_TEXTURE_2D = 9;
		public const int GU_LIGHTING = 10;
		public const int GU_LIGHT0 = 11;
		public const int GU_LIGHT1 = 12;
		public const int GU_LIGHT2 = 13;
		public const int GU_LIGHT3 = 14;
		public const int GU_LINE_SMOOTH = 15;
		public const int GU_PATCH_CULL_FACE = 16;
		public const int GU_COLOR_TEST = 17;
		public const int GU_COLOR_LOGIC_OP = 18;
		public const int GU_FACE_NORMAL_REVERSE = 19;
		public const int GU_PATCH_FACE = 20;
		public const int GU_FRAGMENT_2X = 21;
		public const int RE_COLOR_MATERIAL = 22;
		public const int RE_TEXTURE_GEN_S = 23;
		public const int RE_TEXTURE_GEN_T = 24;
		public const int RE_NUMBER_FLAGS = 25;
		public const int GU_POINTS = 0;
		public const int GU_LINES = 1;
		public const int GU_LINE_STRIP = 2;
		public const int GU_TRIANGLES = 3;
		public const int GU_TRIANGLE_STRIP = 4;
		public const int GU_TRIANGLE_FAN = 5;
		public const int GU_SPRITES = 6;
		public const int RE_QUADS = 7;
		public const int RE_LINES_ADJACENCY = 8;
		public const int RE_TRIANGLES_ADJACENCY = 9;
		public const int RE_TRIANGLE_STRIP_ADJACENCY = 10;
		public const int GU_PROJECTION = 0;
		public const int GU_VIEW = 1;
		public const int GU_MODEL = 2;
		public const int GU_TEXTURE = 3;
		public const int RE_MODELVIEW = 4;
		public const int GU_FLAT = 0;
		public const int GU_SMOOTH = 1;
		public const int RE_AMBIENT = 0;
		public const int RE_EMISSIVE = 1;
		public const int RE_DIFFUSE = 2;
		public const int RE_SPECULAR = 3;
		public const int GU_SINGLE_COLOR = 0;
		public const int GU_SEPARATE_SPECULAR_COLOR = 1;
		public const int GU_FIX_BLEND_COLOR = 10;
		public const int GU_FIX_BLEND_ONE_MINUS_COLOR = 11;
		public const int GU_FIX_BLACK = 12;
		public const int GU_FIX_WHITE = 13;
		public const int RE_TEXENV_COMBINE_RGB = 0;
		public const int RE_TEXENV_COMBINE_ALPHA = 1;
		public const int RE_TEXENV_RGB_SCALE = 2;
		public const int RE_TEXENV_ALPHA_SCALE = 3;
		public const int RE_TEXENV_SRC0_RGB = 4;
		public const int RE_TEXENV_SRC1_RGB = 5;
		public const int RE_TEXENV_SRC2_RGB = 6;
		public const int RE_TEXENV_SRC0_ALPHA = 7;
		public const int RE_TEXENV_SRC1_ALPHA = 8;
		public const int RE_TEXENV_SRC2_ALPHA = 9;
		public const int RE_TEXENV_OPERAND0_RGB = 10;
		public const int RE_TEXENV_OPERAND1_RGB = 11;
		public const int RE_TEXENV_OPERAND2_RGB = 12;
		public const int RE_TEXENV_OPERAND0_ALPHA = 13;
		public const int RE_TEXENV_OPERAND1_ALPHA = 14;
		public const int RE_TEXENV_OPERAND2_ALPHA = 15;
		public const int RE_TEXENV_ENV_MODE = 16;
		public const int RE_TEXENV_MODULATE = 0;
		public const int RE_TEXENV_DECAL = 1;
		public const int RE_TEXENV_BLEND = 2;
		public const int RE_TEXENV_REPLACE = 3;
		public const int RE_TEXENV_ADD = 4;
		public const int RE_TEXENV_INTERPOLATE = 5;
		public const int RE_TEXENV_SUBTRACT = 6;
		public const int RE_TEXENV_TEXTURE = 7;
		public const int RE_TEXENV_CONSTANT = 8;
		public const int RE_TEXENV_PREVIOUS = 9;
		public const int RE_TEXENV_SRC_COLOR = 10;
		public const int RE_TEXENV_SRC_ALPHA = 11;
		public const int RE_TEXENV_COMBINE = 12;
		public const int RE_VERTEX_SHADER = 0;
		public const int RE_FRAGMENT_SHADER = 1;
		public const int RE_GEOMETRY_SHADER = 2;
		public const int RE_TEXTURE = 0;
		public const int RE_COLOR = 1;
		public const int RE_NORMAL = 2;
		public const int RE_VERTEX = 3;
		public const int RE_BYTE = 0;
		public const int RE_UNSIGNED_BYTE = 1;
		public const int RE_SHORT = 2;
		public const int RE_UNSIGNED_SHORT = 3;
		public const int RE_INT = 4;
		public const int RE_UNSIGNED_INT = 5;
		public const int RE_FLOAT = 6;
		public const int RE_DOUBLE = 7;
		public static readonly int[] sizeOfType = new int[] {1, 1, 2, 2, 4, 4, 4, 8};
		public const int RE_STREAM_DRAW = 0;
		public const int RE_STREAM_READ = 1;
		public const int RE_STREAM_COPY = 2;
		public const int RE_STATIC_DRAW = 3;
		public const int RE_STATIC_READ = 4;
		public const int RE_STATIC_COPY = 5;
		public const int RE_DYNAMIC_DRAW = 6;
		public const int RE_DYNAMIC_READ = 7;
		public const int RE_DYNAMIC_COPY = 8;
		public const int RE_GEOMETRY_INPUT_TYPE = 0;
		public const int RE_GEOMETRY_OUTPUT_TYPE = 1;
		public const int RE_GEOMETRY_VERTICES_OUT = 2;
		public const int RE_ARRAY_BUFFER = 0;
		public const int RE_UNIFORM_BUFFER = 1;
		public const int RE_ELEMENT_ARRAY_BUFFER = 2;
		public const int RE_FRAMEBUFFER = 0;
		public const int RE_READ_FRAMEBUFFER = 1;
		public const int RE_DRAW_FRAMEBUFFER = 2;
		public const int RE_DEPTH_ATTACHMENT = 0;
		public const int RE_STENCIL_ATTACHMENT = 1;
		public const int RE_DEPTH_STENCIL_ATTACHMENT = 2;
		public const int RE_COLOR_ATTACHMENT0 = 3;
		public const int RE_COLOR_ATTACHMENT1 = 4;
		public const int RE_COLOR_ATTACHMENT2 = 5;
		public const int RE_COLOR_ATTACHMENT3 = 6;
		public const int RE_COLOR_ATTACHMENT4 = 7;
		public const int RE_COLOR_ATTACHMENT5 = 8;
		public const int RE_COLOR_ATTACHMENT6 = 9;
		public const int RE_COLOR_ATTACHMENT7 = 10;
		public const int RE_MAP_COLOR = 0;
		public const int RE_MAP_STENCIL = 1;
		public const int RE_INDEX_SHIFT = 2;
		public const int RE_INDEX_OFFSET = 3;
		public const int RE_RED_SCALE = 4;
		public const int RE_GREEN_SCALE = 5;
		public const int RE_BLUE_SCALE = 6;
		public const int RE_ALPHA_SCALE = 7;
		public const int RE_DEPTH_SCALE = 8;
		public const int RE_RED_BIAS = 9;
		public const int RE_GREEN_BIAS = 10;
		public const int RE_BLUE_BIAS = 11;
		public const int RE_ALPHA_BIAS = 12;
		public const int RE_DEPTH_BIAS = 13;
		public const int RE_PIXEL_MAP_I_TO_I = 0;
		public const int RE_PIXEL_MAP_S_TO_S = 1;
		public const int RE_PIXEL_MAP_I_TO_R = 2;
		public const int RE_PIXEL_MAP_I_TO_G = 3;
		public const int RE_PIXEL_MAP_I_TO_B = 4;
		public const int RE_PIXEL_MAP_I_TO_A = 5;
		public const int RE_PIXEL_MAP_R_TO_R = 6;
		public const int RE_PIXEL_MAP_G_TO_G = 7;
		public const int RE_PIXEL_MAP_B_TO_B = 8;
		public const int RE_PIXEL_MAP_A_TO_A = 9;
		public const int RE_CLUT_INDEX_NO_HINT = 0;
		public const int RE_CLUT_INDEX_RED_ONLY = 1;
		public const int RE_CLUT_INDEX_GREEN_ONLY = 2;
		public const int RE_CLUT_INDEX_BLUE_ONLY = 3;
		public const int RE_CLUT_INDEX_ALPHA_ONLY = 4;
		public static readonly int RE_COLOR_BUFFER_BIT = (1 << 0);
		public static readonly int RE_DEPTH_BUFFER_BIT = (1 << 1);
		public static readonly int RE_STENCIL_BUFFER_BIT = (1 << 2);
	}

}