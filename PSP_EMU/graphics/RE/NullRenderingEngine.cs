namespace pspsharp.graphics.RE
{

	using Logger = org.apache.log4j.Logger;

	using BufferManagerFactory = pspsharp.graphics.RE.buffer.BufferManagerFactory;
	using IREBufferManager = pspsharp.graphics.RE.buffer.IREBufferManager;

	public class NullRenderingEngine : IRenderingEngine
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			re = this;
		}

		protected internal static readonly Logger log = VideoEngine.log_Renamed;
		protected internal IRenderingEngine re;
		protected internal GeContext context;
		protected internal IREBufferManager bufferManager;

		public NullRenderingEngine()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			bufferManager = BufferManagerFactory.createBufferManager(this);
			bufferManager.RenderingEngine = this;
		}

		public virtual IRenderingEngine RenderingEngine
		{
			set
			{
				this.re = value;
				bufferManager.RenderingEngine = value;
			}
		}

		public virtual GeContext GeContext
		{
			set
			{
				this.context = value;
			}
		}

		public virtual IREBufferManager BufferManager
		{
			get
			{
				return bufferManager;
			}
		}

		public virtual void exit()
		{
		}

		public virtual void startDirectRendering(bool textureEnabled, bool depthWriteEnabled, bool colorWriteEnabled, bool setOrthoMatrix, bool orthoInverted, int width, int height)
		{
		}

		public virtual void endDirectRendering()
		{
		}

		public virtual void startDisplay()
		{
		}

		public virtual void endDisplay()
		{
		}

		public virtual void enableFlag(int flag)
		{
		}

		public virtual void disableFlag(int flag)
		{
		}

		public virtual void setMorphWeight(int index, float value)
		{
		}

		public virtual void setPatchDiv(int s, int t)
		{
		}

		public virtual int PatchPrim
		{
			set
			{
			}
		}

		public virtual int MatrixMode
		{
			set
			{
			}
		}

		public virtual float[] Matrix
		{
			set
			{
			}
		}

		public virtual void multMatrix(float[] values)
		{
		}

		public virtual float[] ProjectionMatrix
		{
			set
			{
			}
		}

		public virtual float[] ViewMatrix
		{
			set
			{
			}
		}

		public virtual float[] ModelMatrix
		{
			set
			{
			}
		}

		public virtual float[] ModelViewMatrix
		{
			set
			{
			}
		}

		public virtual float[] TextureMatrix
		{
			set
			{
			}
		}

		public virtual void endModelViewMatrixUpdate()
		{
		}

		public virtual void setViewport(int x, int y, int width, int height)
		{
		}

		public virtual void setDepthRange(float zpos, float zscale, int near, int far)
		{
		}

		public virtual int DepthFunc
		{
			set
			{
			}
		}

		public virtual int ShadeModel
		{
			set
			{
			}
		}

		public virtual float[] MaterialEmissiveColor
		{
			set
			{
			}
		}

		public virtual float[] MaterialAmbientColor
		{
			set
			{
			}
		}

		public virtual float[] MaterialDiffuseColor
		{
			set
			{
			}
		}

		public virtual float[] MaterialSpecularColor
		{
			set
			{
			}
		}

		public virtual float MaterialShininess
		{
			set
			{
			}
		}

		public virtual float[] LightModelAmbientColor
		{
			set
			{
			}
		}

		public virtual int LightMode
		{
			set
			{
			}
		}

		public virtual void setLightPosition(int light, float[] position)
		{
		}

		public virtual void setLightDirection(int light, float[] direction)
		{
		}

		public virtual void setLightSpotExponent(int light, float exponent)
		{
		}

		public virtual void setLightSpotCutoff(int light, float cutoff)
		{
		}

		public virtual void setLightConstantAttenuation(int light, float constant)
		{
		}

		public virtual void setLightLinearAttenuation(int light, float linear)
		{
		}

		public virtual void setLightQuadraticAttenuation(int light, float quadratic)
		{
		}

		public virtual void setLightAmbientColor(int light, float[] color)
		{
		}

		public virtual void setLightDiffuseColor(int light, float[] color)
		{
		}

		public virtual void setLightSpecularColor(int light, float[] color)
		{
		}

		public virtual void setLightType(int light, int type, int kind)
		{
		}

		public virtual void setBlendFunc(int src, int dst)
		{
		}

		public virtual float[] BlendColor
		{
			set
			{
			}
		}

		public virtual int LogicOp
		{
			set
			{
			}
		}

		public virtual bool DepthMask
		{
			set
			{
			}
		}

		public virtual void setColorMask(int redMask, int greenMask, int blueMask, int alphaMask)
		{
		}

		public virtual void setColorMask(bool redWriteEnabled, bool greenWriteEnabled, bool blueWriteEnabled, bool alphaWriteEnabled)
		{
		}

		public virtual void setTextureWrapMode(int s, int t)
		{
		}

		public virtual int TextureMipmapMinLevel
		{
			set
			{
			}
		}

		public virtual int TextureMipmapMaxLevel
		{
			set
			{
			}
		}

		public virtual int TextureMipmapMinFilter
		{
			set
			{
			}
		}

		public virtual int TextureMipmapMagFilter
		{
			set
			{
			}
		}

		public virtual void setColorMaterial(bool ambient, bool diffuse, bool specular)
		{
		}

		public virtual void setTextureMapMode(int mode, int proj)
		{
		}

		public virtual void setTextureEnvironmentMapping(int u, int v)
		{
		}

		public virtual float[] VertexColor
		{
			set
			{
			}
		}

		public virtual void setUniform(int id, int value)
		{
		}

		public virtual void setUniform(int id, int value1, int value2)
		{
		}

		public virtual void setUniform(int id, float value)
		{
		}

		public virtual void setUniform2(int id, int[] values)
		{
		}

		public virtual void setUniform3(int id, int[] values)
		{
		}

		public virtual void setUniform3(int id, float[] values)
		{
		}

		public virtual void setUniform4(int id, int[] values)
		{
		}

		public virtual void setUniform4(int id, float[] values)
		{
		}

		public virtual void setUniformMatrix4(int id, int count, float[] values)
		{
		}

		public virtual int ColorTestFunc
		{
			set
			{
			}
		}

		public virtual int[] ColorTestReference
		{
			set
			{
			}
		}

		public virtual int[] ColorTestMask
		{
			set
			{
			}
		}

		public virtual void setTextureFunc(int func, bool alphaUsed, bool colorDoubled)
		{
		}

		public virtual int setBones(int count, float[] values)
		{
			// Bones are not supported
			return 0;
		}

		public virtual void setTexEnv(int name, int param)
		{
		}

		public virtual void setTexEnv(int name, float param)
		{
		}

		public virtual void startClearMode(bool color, bool stencil, bool depth)
		{
		}

		public virtual void endClearMode()
		{
		}

		public virtual int createShader(int type)
		{
			return 0;
		}

		public virtual bool compilerShader(int shader, string source)
		{
			return false;
		}

		public virtual int createProgram()
		{
			return 0;
		}

		public virtual void useProgram(int program)
		{
		}

		public virtual void attachShader(int program, int shader)
		{
		}

		public virtual bool linkProgram(int program)
		{
			return false;
		}

		public virtual bool validateProgram(int program)
		{
			return false;
		}

		public virtual int getUniformLocation(int program, string name)
		{
			return -1;
		}

		public virtual int getAttribLocation(int program, string name)
		{
			return -1;
		}

		public virtual void bindAttribLocation(int program, int index, string name)
		{
		}

		public virtual string getShaderInfoLog(int shader)
		{
			return null;
		}

		public virtual string getProgramInfoLog(int program)
		{
			return null;
		}

		public virtual bool isExtensionAvailable(string name)
		{
			return false;
		}

		public virtual void drawArrays(int primitive, int first, int count)
		{
		}

		public virtual void drawElements(int primitive, int count, int indexType, Buffer indices, int indicesOffset)
		{
		}

		public virtual void drawElements(int primitive, int count, int indexType, long indicesOffset)
		{
		}

		public virtual int genBuffer()
		{
			return 0;
		}

		public virtual void deleteBuffer(int buffer)
		{
		}

		public virtual void setBufferData(int target, int size, Buffer buffer, int usage)
		{
		}

		public virtual void setBufferSubData(int target, int offset, int size, Buffer buffer)
		{
		}

		public virtual void bindBuffer(int target, int buffer)
		{
		}

		public virtual void enableClientState(int type)
		{
		}

		public virtual void disableClientState(int type)
		{
		}

		public virtual void enableVertexAttribArray(int id)
		{
		}

		public virtual void disableVertexAttribArray(int id)
		{
		}

		public virtual void setTexCoordPointer(int size, int type, int stride, long offset)
		{
		}

		public virtual void setTexCoordPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
		}

		public virtual void setColorPointer(int size, int type, int stride, long offset)
		{
		}

		public virtual void setColorPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
		}

		public virtual void setVertexPointer(int size, int type, int stride, long offset)
		{
		}

		public virtual void setVertexPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
		}

		public virtual void setNormalPointer(int type, int stride, long offset)
		{
		}

		public virtual void setNormalPointer(int type, int stride, int bufferSize, Buffer buffer)
		{
		}

		public virtual void setWeightPointer(int size, int type, int stride, long offset)
		{
		}

		public virtual void setWeightPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
		}

		public virtual void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, long offset)
		{
		}

		public virtual void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, int bufferSize, Buffer buffer)
		{
		}

		public virtual void setPixelStore(int rowLength, int alignment)
		{
		}

		public virtual int genTexture()
		{
			return 0;
		}

		public virtual void bindTexture(int texture)
		{
		}

		public virtual void deleteTexture(int texture)
		{
		}

		public virtual void setCompressedTexImage(int level, int internalFormat, int width, int height, int compressedSize, Buffer buffer)
		{
		}

		public virtual void setTexImage(int level, int internalFormat, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
		}

			public virtual void setTexImagexBRZ(int level, int internalFormat, int width, int height, int bufwidth, int format, int type, int textureSize, Buffer buffer)
			{
			}

		public virtual void setTexSubImage(int level, int xOffset, int yOffset, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
		}

		public virtual void getTexImage(int level, int format, int type, Buffer buffer)
		{
		}

		public virtual void copyTexSubImage(int level, int xOffset, int yOffset, int x, int y, int width, int height)
		{
		}

		public virtual void setStencilOp(int fail, int zfail, int zpass)
		{
		}

		public virtual void setStencilFunc(int func, int @ref, int mask)
		{
		}

		public virtual void setAlphaFunc(int func, int @ref, int mask)
		{
		}

		public virtual void setFogHint()
		{
		}

		public virtual float[] FogColor
		{
			set
			{
			}
		}

		public virtual void setFogDist(float start, float end)
		{
		}

		public virtual float[] TextureEnvColor
		{
			set
			{
			}
		}

		public virtual bool FrontFace
		{
			set
			{
			}
		}

		public virtual void setScissor(int x, int y, int width, int height)
		{
		}

		public virtual int BlendEquation
		{
			set
			{
			}
		}

		public virtual void setLineSmoothHint()
		{
		}

		public virtual void beginBoundingBox(int numberOfVertexBoundingBox)
		{
		}

		public virtual void drawBoundingBox(float[][] values)
		{
		}

		public virtual void endBoundingBox(VertexInfo vinfo)
		{
		}

		public virtual bool BoundingBoxVisible
		{
			get
			{
				return false;
			}
		}

		public virtual int genQuery()
		{
			return 0;
		}

		public virtual void beginQuery(int id)
		{
		}

		public virtual void endQuery()
		{
		}

		public virtual bool getQueryResultAvailable(int id)
		{
			return false;
		}

		public virtual int getQueryResult(int id)
		{
			return 0;
		}

		public virtual void clear(float red, float green, float blue, float alpha)
		{
		}

		public virtual bool canAllNativeVertexInfo()
		{
			return false;
		}

		public virtual bool canNativeSpritesPrimitive()
		{
			return false;
		}

		public virtual void setVertexInfo(VertexInfo vinfo, bool allNativeVertexInfo, bool useVertexColor, bool useTexture, int type)
		{
		}

		public virtual void setProgramParameter(int program, int parameter, int value)
		{
		}

		public virtual bool QueryAvailable
		{
			get
			{
				return false;
			}
		}

		public virtual bool ShaderAvailable
		{
			get
			{
				return false;
			}
		}

		public virtual int getUniformBlockIndex(int program, string name)
		{
			return -1;
		}

		public virtual void bindBufferBase(int target, int bindingPoint, int buffer)
		{
		}

		public virtual void setUniformBlockBinding(int program, int blockIndex, int bindingPoint)
		{
		}

		public virtual int getUniformIndex(int program, string name)
		{
			return -1;
		}

		public virtual int[] getUniformIndices(int program, string[] names)
		{
			return null;
		}

		public virtual int getActiveUniformOffset(int program, int uniformIndex)
		{
			return 0;
		}

		public virtual bool FramebufferObjectAvailable
		{
			get
			{
				return false;
			}
		}

		public virtual int genFramebuffer()
		{
			return 0;
		}

		public virtual int genRenderbuffer()
		{
			return 0;
		}

		public virtual void deleteFramebuffer(int framebuffer)
		{
		}

		public virtual void deleteRenderbuffer(int renderbuffer)
		{
		}

		public virtual void bindFramebuffer(int target, int framebuffer)
		{
		}

		public virtual void bindRenderbuffer(int renderbuffer)
		{
		}

		public virtual void setRenderbufferStorage(int internalFormat, int width, int height)
		{
		}

		public virtual void setFramebufferRenderbuffer(int target, int attachment, int renderbuffer)
		{
		}

		public virtual void setFramebufferTexture(int target, int attachment, int texture, int level)
		{
		}

		public virtual int genVertexArray()
		{
			return 0;
		}

		public virtual void bindVertexArray(int id)
		{
		}

		public virtual void deleteVertexArray(int id)
		{
		}

		public virtual bool VertexArrayAvailable
		{
			get
			{
				return false;
			}
		}

		public virtual void multiDrawArrays(int primitive, IntBuffer first, IntBuffer count)
		{
		}

		public virtual void drawArraysBurstMode(int primitive, int first, int count)
		{
		}

		public virtual void multiDrawElements(int primitive, IntBuffer first, IntBuffer count, int indexType, long indicesOffset)
		{
		}

		public virtual void drawElementsBurstMode(int primitive, int count, int indexType, long indicesOffset)
		{
		}

		public virtual void setPixelTransfer(int parameter, int value)
		{
		}

		public virtual void setPixelTransfer(int parameter, float value)
		{
		}

		public virtual void setPixelTransfer(int parameter, bool value)
		{
		}

		public virtual void setPixelMap(int map, int mapSize, Buffer buffer)
		{
		}

		public virtual bool canNativeClut(int textureAddress, int pixelFormat, bool textureSwizzle)
		{
			return false;
		}

		public virtual int ActiveTexture
		{
			set
			{
			}
		}

		public virtual void setTextureFormat(int pixelFormat, bool swizzle)
		{
		}

		public virtual void bindActiveTexture(int index, int texture)
		{
		}

		public virtual float TextureAnisotropy
		{
			set
			{
			}
		}

		public virtual float MaxTextureAnisotropy
		{
			get
			{
				return 0;
			}
		}

		public virtual string ShadingLanguageVersion
		{
			get
			{
				return null;
			}
		}

		public virtual void setBlendSFix(int sfix, float[] color)
		{
		}

		public virtual void setBlendDFix(int dfix, float[] color)
		{
		}

		public virtual void waitForRenderingCompletion()
		{
		}

		public virtual bool canReadAllVertexInfo()
		{
			return false;
		}

		public virtual void readStencil(int x, int y, int width, int height, int bufferSize, Buffer buffer)
		{
		}

		public virtual void blitFramebuffer(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, int mask, int filter)
		{
		}

		public virtual bool checkAndLogErrors(string logComment)
		{
			// No error
			return false;
		}

		public virtual bool setCopyRedToAlpha(bool copyRedToAlpha)
		{
			return false;
		}

		public virtual void textureBarrier()
		{
		}

		public virtual bool TextureBarrierAvailable
		{
			get
			{
				return false;
			}
		}

		public virtual bool canDiscardVertices()
		{
			return false;
		}

		public virtual void setViewportPos(float x, float y, float z)
		{
		}

		public virtual void setViewportScale(float sx, float sy, float sz)
		{
		}
	}

}