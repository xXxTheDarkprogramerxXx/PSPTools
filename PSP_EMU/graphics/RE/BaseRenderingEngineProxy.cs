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

	//using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// @author gid15
	/// 
	/// Base class for a RenderingEngine implementing a proxy functionality where
	/// all the calls are forwarded to a proxy.
	/// </summary>
	public class BaseRenderingEngineProxy : IRenderingEngine
	{
		protected internal static readonly Logger log = VideoEngine.log_Renamed;
		protected internal IRenderingEngine re;
		protected internal IRenderingEngine proxy;
		protected internal GeContext context;
		protected internal static readonly float[] identityMatrix = new float[] {1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1};

		public BaseRenderingEngineProxy(IRenderingEngine proxy)
		{
			this.proxy = proxy;
			this.re = this;
			proxy.RenderingEngine = this;
		}

		public virtual IRenderingEngine RenderingEngine
		{
			set
			{
				this.re = value;
				proxy.RenderingEngine = value;
			}
		}

		public virtual GeContext GeContext
		{
			set
			{
				this.context = value;
				proxy.GeContext = value;
			}
		}

		public virtual void exit()
		{
			proxy.exit();
		}

		public virtual void endDirectRendering()
		{
			proxy.endDirectRendering();
		}

		public virtual void startDirectRendering(bool textureEnabled, bool depthWriteEnabled, bool colorWriteEnabled, bool setOrthoMatrix, bool orthoInverted, int width, int height)
		{
			proxy.startDirectRendering(textureEnabled, depthWriteEnabled, colorWriteEnabled, setOrthoMatrix, orthoInverted, width, height);
		}

		public virtual void startDisplay()
		{
			proxy.startDisplay();
		}

		public virtual void endDisplay()
		{
			proxy.endDisplay();
		}

		public virtual void disableFlag(int flag)
		{
			proxy.disableFlag(flag);
		}

		public virtual void enableFlag(int flag)
		{
			proxy.enableFlag(flag);
		}

		public virtual float[] BlendColor
		{
			set
			{
				proxy.BlendColor = value;
			}
		}

		public virtual void setBlendFunc(int src, int dst)
		{
			proxy.setBlendFunc(src, dst);
		}

		public virtual void setColorMask(int redMask, int greenMask, int blueMask, int alphaMask)
		{
			proxy.setColorMask(redMask, greenMask, blueMask, alphaMask);
		}

		public virtual void setColorMask(bool redWriteEnabled, bool greenWriteEnabled, bool blueWriteEnabled, bool alphaWriteEnabled)
		{
			proxy.setColorMask(redWriteEnabled, greenWriteEnabled, blueWriteEnabled, alphaWriteEnabled);
		}

		public virtual void setColorMaterial(bool ambient, bool diffuse, bool specular)
		{
			proxy.setColorMaterial(ambient, diffuse, specular);
		}

		public virtual int DepthFunc
		{
			set
			{
				proxy.DepthFunc = value;
			}
		}

		public virtual bool DepthMask
		{
			set
			{
				proxy.DepthMask = value;
			}
		}

		public virtual void setDepthRange(float zpos, float zscale, int near, int far)
		{
			proxy.setDepthRange(zpos, zscale, near, far);
		}

		public virtual void setLightAmbientColor(int light, float[] color)
		{
			proxy.setLightAmbientColor(light, color);
		}

		public virtual void setLightConstantAttenuation(int light, float constant)
		{
			proxy.setLightConstantAttenuation(light, constant);
		}

		public virtual void setLightDiffuseColor(int light, float[] color)
		{
			proxy.setLightDiffuseColor(light, color);
		}

		public virtual void setLightDirection(int light, float[] direction)
		{
			proxy.setLightDirection(light, direction);
		}

		public virtual void setLightLinearAttenuation(int light, float linear)
		{
			proxy.setLightLinearAttenuation(light, linear);
		}

		public virtual int LightMode
		{
			set
			{
				proxy.LightMode = value;
			}
		}

		public virtual float[] LightModelAmbientColor
		{
			set
			{
				proxy.LightModelAmbientColor = value;
			}
		}

		public virtual void setLightPosition(int light, float[] position)
		{
			proxy.setLightPosition(light, position);
		}

		public virtual void setLightQuadraticAttenuation(int light, float quadratic)
		{
			proxy.setLightQuadraticAttenuation(light, quadratic);
		}

		public virtual void setLightSpecularColor(int light, float[] color)
		{
			proxy.setLightSpecularColor(light, color);
		}

		public virtual void setLightSpotCutoff(int light, float cutoff)
		{
			proxy.setLightSpotCutoff(light, cutoff);
		}

		public virtual void setLightSpotExponent(int light, float exponent)
		{
			proxy.setLightSpotExponent(light, exponent);
		}

		public virtual void setLightType(int light, int type, int kind)
		{
			proxy.setLightType(light, type, kind);
		}

		public virtual int LogicOp
		{
			set
			{
				proxy.LogicOp = value;
			}
		}

		public virtual float[] MaterialAmbientColor
		{
			set
			{
				proxy.MaterialAmbientColor = value;
			}
		}

		public virtual float[] MaterialDiffuseColor
		{
			set
			{
				proxy.MaterialDiffuseColor = value;
			}
		}

		public virtual float[] MaterialEmissiveColor
		{
			set
			{
				proxy.MaterialEmissiveColor = value;
			}
		}

		public virtual float[] MaterialSpecularColor
		{
			set
			{
				proxy.MaterialSpecularColor = value;
			}
		}

		public virtual float[] Matrix
		{
			set
			{
				proxy.Matrix = value;
			}
		}

		public virtual int MatrixMode
		{
			set
			{
				proxy.MatrixMode = value;
			}
		}

		public virtual void multMatrix(float[] values)
		{
			proxy.multMatrix(values);
		}

		public virtual float[] ModelMatrix
		{
			set
			{
				proxy.ModelMatrix = value;
			}
		}

		public virtual void endModelViewMatrixUpdate()
		{
			proxy.endModelViewMatrixUpdate();
		}

		public virtual float[] ModelViewMatrix
		{
			set
			{
				proxy.ModelViewMatrix = value;
			}
		}

		public virtual void setMorphWeight(int index, float value)
		{
			proxy.setMorphWeight(index, value);
		}

		public virtual void setPatchDiv(int s, int t)
		{
			proxy.setPatchDiv(s, t);
		}

		public virtual int PatchPrim
		{
			set
			{
				proxy.PatchPrim = value;
			}
		}

		public virtual float[] ProjectionMatrix
		{
			set
			{
				proxy.ProjectionMatrix = value;
			}
		}

		public virtual int ShadeModel
		{
			set
			{
				proxy.ShadeModel = value;
			}
		}

		public virtual void setTextureEnvironmentMapping(int u, int v)
		{
			proxy.setTextureEnvironmentMapping(u, v);
		}

		public virtual float[] TextureMatrix
		{
			set
			{
				proxy.TextureMatrix = value;
			}
		}

		public virtual int TextureMipmapMaxLevel
		{
			set
			{
				proxy.TextureMipmapMaxLevel = value;
			}
		}

		public virtual int TextureMipmapMinLevel
		{
			set
			{
				proxy.TextureMipmapMinLevel = value;
			}
		}

		public virtual int TextureMipmapMinFilter
		{
			set
			{
				proxy.TextureMipmapMinFilter = value;
			}
		}

		public virtual int TextureMipmapMagFilter
		{
			set
			{
				proxy.TextureMipmapMagFilter = value;
			}
		}

		public virtual void setTextureWrapMode(int s, int t)
		{
			proxy.setTextureWrapMode(s, t);
		}

		public virtual float[] VertexColor
		{
			set
			{
				proxy.VertexColor = value;
			}
		}

		public virtual float[] ViewMatrix
		{
			set
			{
				proxy.ViewMatrix = value;
			}
		}

		public virtual void setViewport(int x, int y, int width, int height)
		{
			proxy.setViewport(x, y, width, height);
		}

		public virtual void setUniform(int id, int value)
		{
			proxy.setUniform(id, value);
		}

		public virtual void setUniform(int id, int value1, int value2)
		{
			proxy.setUniform(id, value1, value2);
		}

		public virtual void setUniform(int id, float value)
		{
			proxy.setUniform(id, value);
		}

		public virtual void setUniform2(int id, int[] values)
		{
			proxy.setUniform2(id, values);
		}

		public virtual void setUniform3(int id, int[] values)
		{
			proxy.setUniform3(id, values);
		}

		public virtual void setUniform3(int id, float[] values)
		{
			proxy.setUniform3(id, values);
		}

		public virtual void setUniform4(int id, int[] values)
		{
			proxy.setUniform4(id, values);
		}

		public virtual void setUniform4(int id, float[] values)
		{
			proxy.setUniform4(id, values);
		}

		public virtual void setUniformMatrix4(int id, int count, float[] values)
		{
			proxy.setUniformMatrix4(id, count, values);
		}

		public virtual int ColorTestFunc
		{
			set
			{
				proxy.ColorTestFunc = value;
			}
		}

		public virtual int[] ColorTestMask
		{
			set
			{
				proxy.ColorTestMask = value;
			}
		}

		public virtual int[] ColorTestReference
		{
			set
			{
				proxy.ColorTestReference = value;
			}
		}

		public virtual void setTextureFunc(int func, bool alphaUsed, bool colorDoubled)
		{
			proxy.setTextureFunc(func, alphaUsed, colorDoubled);
		}

		public virtual int setBones(int count, float[] values)
		{
			return proxy.setBones(count, values);
		}

		public virtual void setTextureMapMode(int mode, int proj)
		{
			proxy.setTextureMapMode(mode, proj);
		}

		public virtual void setTexEnv(int name, int param)
		{
			proxy.setTexEnv(name, param);
		}

		public virtual void setTexEnv(int name, float param)
		{
			proxy.setTexEnv(name, param);
		}

		public virtual void endClearMode()
		{
			proxy.endClearMode();
		}

		public virtual void startClearMode(bool color, bool stencil, bool depth)
		{
			proxy.startClearMode(color, stencil, depth);
		}

		public virtual void attachShader(int program, int shader)
		{
			proxy.attachShader(program, shader);
		}

		public virtual bool compilerShader(int shader, string source)
		{
			return proxy.compilerShader(shader, source);
		}

		public virtual int createProgram()
		{
			return proxy.createProgram();
		}

		public virtual void useProgram(int program)
		{
			proxy.useProgram(program);
		}

		public virtual int createShader(int type)
		{
			return proxy.createShader(type);
		}

		public virtual int getAttribLocation(int program, string name)
		{
			return proxy.getAttribLocation(program, name);
		}

		public virtual void bindAttribLocation(int program, int index, string name)
		{
			proxy.bindAttribLocation(program, index, name);
		}

		public virtual string getProgramInfoLog(int program)
		{
			return proxy.getProgramInfoLog(program);
		}

		public virtual string getShaderInfoLog(int shader)
		{
			return proxy.getShaderInfoLog(shader);
		}

		public virtual int getUniformLocation(int program, string name)
		{
			return proxy.getUniformLocation(program, name);
		}

		public virtual bool linkProgram(int program)
		{
			return proxy.linkProgram(program);
		}

		public virtual bool validateProgram(int program)
		{
			return proxy.validateProgram(program);
		}

		public virtual bool isExtensionAvailable(string name)
		{
			return proxy.isExtensionAvailable(name);
		}

		public virtual void drawArrays(int type, int first, int count)
		{
			proxy.drawArrays(type, first, count);
		}

		public virtual void deleteBuffer(int buffer)
		{
			proxy.deleteBuffer(buffer);
		}

		public virtual int genBuffer()
		{
			return proxy.genBuffer();
		}

		public virtual void setBufferData(int target, int size, Buffer buffer, int usage)
		{
			proxy.setBufferData(target, size, buffer, usage);
		}

		public virtual void setBufferSubData(int target, int offset, int size, Buffer buffer)
		{
			proxy.setBufferSubData(target, offset, size, buffer);
		}

		public virtual void bindBuffer(int target, int buffer)
		{
			proxy.bindBuffer(target, buffer);
		}

		public virtual void enableClientState(int type)
		{
			proxy.enableClientState(type);
		}

		public virtual void enableVertexAttribArray(int id)
		{
			proxy.enableVertexAttribArray(id);
		}

		public virtual void disableClientState(int type)
		{
			proxy.disableClientState(type);
		}

		public virtual void disableVertexAttribArray(int id)
		{
			proxy.disableVertexAttribArray(id);
		}

		public virtual void setColorPointer(int size, int type, int stride, long offset)
		{
			proxy.setColorPointer(size, type, stride, offset);
		}

		public virtual void setColorPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			proxy.setColorPointer(size, type, stride, bufferSize, buffer);
		}

		public virtual void setNormalPointer(int type, int stride, long offset)
		{
			proxy.setNormalPointer(type, stride, offset);
		}

		public virtual void setNormalPointer(int type, int stride, int bufferSize, Buffer buffer)
		{
			proxy.setNormalPointer(type, stride, bufferSize, buffer);
		}

		public virtual void setTexCoordPointer(int size, int type, int stride, long offset)
		{
			proxy.setTexCoordPointer(size, type, stride, offset);
		}

		public virtual void setTexCoordPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			proxy.setTexCoordPointer(size, type, stride, bufferSize, buffer);
		}

		public virtual void setVertexPointer(int size, int type, int stride, long offset)
		{
			proxy.setVertexPointer(size, type, stride, offset);
		}

		public virtual void setVertexPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			proxy.setVertexPointer(size, type, stride, bufferSize, buffer);
		}

		public virtual void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, long offset)
		{
			proxy.setVertexAttribPointer(id, size, type, normalized, stride, offset);
		}

		public virtual void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, int bufferSize, Buffer buffer)
		{
			proxy.setVertexAttribPointer(id, size, type, normalized, stride, bufferSize, buffer);
		}

		public virtual void setPixelStore(int rowLength, int alignment)
		{
			proxy.setPixelStore(rowLength, alignment);
		}

		public virtual int genTexture()
		{
			return proxy.genTexture();
		}

		public virtual void bindTexture(int texture)
		{
			proxy.bindTexture(texture);
		}

		public virtual void deleteTexture(int texture)
		{
			proxy.deleteTexture(texture);
		}

		public virtual void setCompressedTexImage(int level, int internalFormat, int width, int height, int compressedSize, Buffer buffer)
		{
			proxy.setCompressedTexImage(level, internalFormat, width, height, compressedSize, buffer);
		}

		public virtual void setTexImage(int level, int internalFormat, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
			proxy.setTexImage(level, internalFormat, width, height, format, type, textureSize, buffer);
		}

			public virtual void setTexImagexBRZ(int level, int internalFormat, int width, int height, int bufwidth, int format, int type, int textureSize, Buffer buffer)
			{
				proxy.setTexImagexBRZ(level, internalFormat, width, height, bufwidth, format, type, textureSize, buffer);
			}

		public virtual void setStencilOp(int fail, int zfail, int zpass)
		{
			proxy.setStencilOp(fail, zfail, zpass);
		}

		public virtual void setStencilFunc(int func, int @ref, int mask)
		{
			proxy.setStencilFunc(func, @ref, mask);
		}

		public virtual void setAlphaFunc(int func, int @ref, int mask)
		{
			proxy.setAlphaFunc(func, @ref, mask);
		}

		public virtual int BlendEquation
		{
			set
			{
				proxy.BlendEquation = value;
			}
		}

		public virtual float[] FogColor
		{
			set
			{
				proxy.FogColor = value;
			}
		}

		public virtual void setFogDist(float start, float end)
		{
			proxy.setFogDist(start, end);
		}

		public virtual bool FrontFace
		{
			set
			{
				proxy.FrontFace = value;
			}
		}

		public virtual void setScissor(int x, int y, int width, int height)
		{
			proxy.setScissor(x, y, width, height);
		}

		public virtual float[] TextureEnvColor
		{
			set
			{
				proxy.TextureEnvColor = value;
			}
		}

		public virtual void setFogHint()
		{
			proxy.setFogHint();
		}

		public virtual void setLineSmoothHint()
		{
			proxy.setLineSmoothHint();
		}

		public virtual float MaterialShininess
		{
			set
			{
				proxy.MaterialShininess = value;
			}
		}

		public virtual void setTexSubImage(int level, int xOffset, int yOffset, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
			proxy.setTexSubImage(level, xOffset, yOffset, width, height, format, type, textureSize, buffer);
		}

		public virtual void beginQuery(int id)
		{
			proxy.beginQuery(id);
		}

		public virtual void endQuery()
		{
			proxy.endQuery();
		}

		public virtual int genQuery()
		{
			return proxy.genQuery();
		}

		public virtual void drawBoundingBox(float[][] values)
		{
			proxy.drawBoundingBox(values);
		}

		public virtual void endBoundingBox(VertexInfo vinfo)
		{
			proxy.endBoundingBox(vinfo);
		}

		public virtual void beginBoundingBox(int numberOfVertexBoundingBox)
		{
			proxy.beginBoundingBox(numberOfVertexBoundingBox);
		}

		public virtual bool BoundingBoxVisible
		{
			get
			{
				return proxy.BoundingBoxVisible;
			}
		}

		public virtual int getQueryResult(int id)
		{
			return proxy.getQueryResult(id);
		}

		public virtual bool getQueryResultAvailable(int id)
		{
			return proxy.getQueryResultAvailable(id);
		}

		public virtual void clear(float red, float green, float blue, float alpha)
		{
			proxy.clear(red, green, blue, alpha);
		}

		public virtual void copyTexSubImage(int level, int xOffset, int yOffset, int x, int y, int width, int height)
		{
			proxy.copyTexSubImage(level, xOffset, yOffset, x, y, width, height);
		}

		public virtual void getTexImage(int level, int format, int type, Buffer buffer)
		{
			proxy.getTexImage(level, format, type, buffer);
		}

		public virtual void setWeightPointer(int size, int type, int stride, long offset)
		{
			proxy.setWeightPointer(size, type, stride, offset);
		}

		public virtual void setWeightPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			proxy.setWeightPointer(size, type, stride, bufferSize, buffer);
		}

		public virtual IREBufferManager BufferManager
		{
			get
			{
				return proxy.BufferManager;
			}
		}

		public virtual bool canAllNativeVertexInfo()
		{
			return proxy.canAllNativeVertexInfo();
		}

		public virtual bool canNativeSpritesPrimitive()
		{
			return proxy.canNativeSpritesPrimitive();
		}

		public virtual void setVertexInfo(VertexInfo vinfo, bool allNativeVertexInfo, bool useVertexColor, bool useTexture, int type)
		{
			proxy.setVertexInfo(vinfo, allNativeVertexInfo, useVertexColor, useTexture, type);
		}

		public virtual void setProgramParameter(int program, int parameter, int value)
		{
			proxy.setProgramParameter(program, parameter, value);
		}

		public virtual bool QueryAvailable
		{
			get
			{
				return proxy.QueryAvailable;
			}
		}

		public virtual bool ShaderAvailable
		{
			get
			{
				return proxy.ShaderAvailable;
			}
		}

		public virtual void bindBufferBase(int target, int bindingPoint, int buffer)
		{
			proxy.bindBufferBase(target, bindingPoint, buffer);
		}

		public virtual int getUniformBlockIndex(int program, string name)
		{
			return proxy.getUniformBlockIndex(program, name);
		}

		public virtual void setUniformBlockBinding(int program, int blockIndex, int bindingPoint)
		{
			proxy.setUniformBlockBinding(program, blockIndex, bindingPoint);
		}

		public virtual int getUniformIndex(int program, string name)
		{
			return proxy.getUniformIndex(program, name);
		}

		public virtual int[] getUniformIndices(int program, string[] names)
		{
			return proxy.getUniformIndices(program, names);
		}

		public virtual int getActiveUniformOffset(int program, int uniformIndex)
		{
			return proxy.getActiveUniformOffset(program, uniformIndex);
		}

		public virtual void bindFramebuffer(int target, int framebuffer)
		{
			proxy.bindFramebuffer(target, framebuffer);
		}

		public virtual void bindRenderbuffer(int renderbuffer)
		{
			proxy.bindRenderbuffer(renderbuffer);
		}

		public virtual void deleteFramebuffer(int framebuffer)
		{
			proxy.deleteFramebuffer(framebuffer);
		}

		public virtual void deleteRenderbuffer(int renderbuffer)
		{
			proxy.deleteRenderbuffer(renderbuffer);
		}

		public virtual int genFramebuffer()
		{
			return proxy.genFramebuffer();
		}

		public virtual int genRenderbuffer()
		{
			return proxy.genRenderbuffer();
		}

		public virtual bool FramebufferObjectAvailable
		{
			get
			{
				return proxy.FramebufferObjectAvailable;
			}
		}

		public virtual void setFramebufferRenderbuffer(int target, int attachment, int renderbuffer)
		{
			proxy.setFramebufferRenderbuffer(target, attachment, renderbuffer);
		}

		public virtual void setFramebufferTexture(int target, int attachment, int texture, int level)
		{
			proxy.setFramebufferTexture(target, attachment, texture, level);
		}

		public virtual void setRenderbufferStorage(int internalFormat, int width, int height)
		{
			proxy.setRenderbufferStorage(internalFormat, width, height);
		}

		public virtual void bindVertexArray(int id)
		{
			proxy.bindVertexArray(id);
		}

		public virtual void deleteVertexArray(int id)
		{
			proxy.deleteVertexArray(id);
		}

		public virtual int genVertexArray()
		{
			return proxy.genVertexArray();
		}

		public virtual bool VertexArrayAvailable
		{
			get
			{
				return proxy.VertexArrayAvailable;
			}
		}

		public virtual void multiDrawArrays(int primitive, IntBuffer first, IntBuffer count)
		{
			proxy.multiDrawArrays(primitive, first, count);
		}

		public virtual void drawArraysBurstMode(int primitive, int first, int count)
		{
			proxy.drawArraysBurstMode(primitive, first, count);
		}

		public virtual void setPixelTransfer(int parameter, int value)
		{
			proxy.setPixelTransfer(parameter, value);
		}

		public virtual void setPixelTransfer(int parameter, float value)
		{
			proxy.setPixelTransfer(parameter, value);
		}

		public virtual void setPixelTransfer(int parameter, bool value)
		{
			proxy.setPixelTransfer(parameter, value);
		}

		public virtual void setPixelMap(int map, int mapSize, Buffer buffer)
		{
			proxy.setPixelMap(map, mapSize, buffer);
		}

		public virtual bool canNativeClut(int textureAddress, int pixelFormat, bool textureSwizzle)
		{
			return proxy.canNativeClut(textureAddress, pixelFormat, textureSwizzle);
		}

		public virtual int ActiveTexture
		{
			set
			{
				proxy.ActiveTexture = value;
			}
		}

		public virtual void setTextureFormat(int pixelFormat, bool swizzle)
		{
			proxy.setTextureFormat(pixelFormat, swizzle);
		}

		public virtual void bindActiveTexture(int index, int texture)
		{
			proxy.bindActiveTexture(index, texture);
		}

		public virtual float MaxTextureAnisotropy
		{
			get
			{
				return proxy.MaxTextureAnisotropy;
			}
		}

		public virtual float TextureAnisotropy
		{
			set
			{
				proxy.TextureAnisotropy = value;
			}
		}

		public virtual string ShadingLanguageVersion
		{
			get
			{
				return proxy.ShadingLanguageVersion;
			}
		}

		public virtual void setBlendDFix(int sfix, float[] color)
		{
			proxy.setBlendDFix(sfix, color);
		}

		public virtual void setBlendSFix(int dfix, float[] color)
		{
			proxy.setBlendSFix(dfix, color);
		}

		public virtual void waitForRenderingCompletion()
		{
			proxy.waitForRenderingCompletion();
		}

		public virtual bool canReadAllVertexInfo()
		{
			return proxy.canReadAllVertexInfo();
		}

		public virtual void readStencil(int x, int y, int width, int height, int bufferSize, Buffer buffer)
		{
			proxy.readStencil(x, y, width, height, bufferSize, buffer);
		}

		public virtual void blitFramebuffer(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, int mask, int filter)
		{
			proxy.blitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter);
		}

		public virtual bool checkAndLogErrors(string logComment)
		{
			return proxy.checkAndLogErrors(logComment);
		}

		public virtual bool setCopyRedToAlpha(bool copyRedToAlpha)
		{
			return proxy.setCopyRedToAlpha(copyRedToAlpha);
		}

		public virtual void drawElements(int primitive, int count, int indexType, Buffer indices, int indicesOffset)
		{
			proxy.drawElements(primitive, count, indexType, indices, indicesOffset);
		}

		public virtual void drawElements(int primitive, int count, int indexType, long indicesOffset)
		{
			proxy.drawElements(primitive, count, indexType, indicesOffset);
		}

		public virtual void multiDrawElements(int primitive, IntBuffer first, IntBuffer count, int indexType, long indicesOffset)
		{
			proxy.multiDrawElements(primitive, first, count, indexType, indicesOffset);
		}

		public virtual void drawElementsBurstMode(int primitive, int count, int indexType, long indicesOffset)
		{
			proxy.drawElementsBurstMode(primitive, count, indexType, indicesOffset);
		}

		public virtual void textureBarrier()
		{
			proxy.textureBarrier();
		}

		public virtual bool TextureBarrierAvailable
		{
			get
			{
				return proxy.TextureBarrierAvailable;
			}
		}

		public virtual bool canDiscardVertices()
		{
			return proxy.canDiscardVertices();
		}

		public virtual void setViewportPos(float x, float y, float z)
		{
			proxy.setViewportPos(x, y, z);
		}

		public virtual void setViewportScale(float sx, float sy, float sz)
		{
			proxy.setViewportScale(sx, sy, sz);
		}
	}

}