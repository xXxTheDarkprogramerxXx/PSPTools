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
	/// This RenderingEngine-Proxy class checks and logs
	/// after each RE call any error that has occurred
	/// during that particular call.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class CheckErrorsProxy : BaseRenderingEngineProxy
	{
		public CheckErrorsProxy(IRenderingEngine proxy) : base(proxy)
		{
		}

		public override bool checkAndLogErrors(string logComment)
		{
			return base.checkAndLogErrors(logComment);
		}

		public override void exit()
		{
			base.exit();
			// Do not check the errors on exit as we don't have a valid OpenGL context in the current thread.
			//re.checkAndLogErrors("exit");
		}

		public override IRenderingEngine RenderingEngine
		{
			set
			{
				base.RenderingEngine = value;
				value.checkAndLogErrors("setRenderingEngine");
			}
		}

		public override GeContext GeContext
		{
			set
			{
				base.GeContext = value;
				re.checkAndLogErrors("setGeContext");
			}
		}

		public override void endDirectRendering()
		{
			base.endDirectRendering();
			re.checkAndLogErrors("endDirectRendering");
		}

		public override void startDirectRendering(bool textureEnabled, bool depthWriteEnabled, bool colorWriteEnabled, bool setOrthoMatrix, bool orthoInverted, int width, int height)
		{
			base.startDirectRendering(textureEnabled, depthWriteEnabled, colorWriteEnabled, setOrthoMatrix, orthoInverted, width, height);
			re.checkAndLogErrors("startDirectRendering");
		}

		public override void startDisplay()
		{
			base.startDisplay();
			re.checkAndLogErrors("startDisplay");
		}

		public override void endDisplay()
		{
			base.endDisplay();
			re.checkAndLogErrors("endDisplay");
		}

		public override void disableFlag(int flag)
		{
			base.disableFlag(flag);
			re.checkAndLogErrors("disableFlag");
		}

		public override void enableFlag(int flag)
		{
			base.enableFlag(flag);
			re.checkAndLogErrors("enableFlag");
		}

		public override float[] BlendColor
		{
			set
			{
				base.BlendColor = value;
				re.checkAndLogErrors("setBlendColor");
			}
		}

		public override void setBlendFunc(int src, int dst)
		{
			base.setBlendFunc(src, dst);
			re.checkAndLogErrors("setBlendFunc");
		}

		public override void setColorMask(int redMask, int greenMask, int blueMask, int alphaMask)
		{
			base.setColorMask(redMask, greenMask, blueMask, alphaMask);
			re.checkAndLogErrors("setColorMask");
		}

		public override void setColorMask(bool redWriteEnabled, bool greenWriteEnabled, bool blueWriteEnabled, bool alphaWriteEnabled)
		{
			base.setColorMask(redWriteEnabled, greenWriteEnabled, blueWriteEnabled, alphaWriteEnabled);
			re.checkAndLogErrors("setColorMask");
		}

		public override void setColorMaterial(bool ambient, bool diffuse, bool specular)
		{
			base.setColorMaterial(ambient, diffuse, specular);
			re.checkAndLogErrors("setColorMaterial");
		}

		public override int DepthFunc
		{
			set
			{
				base.DepthFunc = value;
				re.checkAndLogErrors("setDepthFunc");
			}
		}

		public override bool DepthMask
		{
			set
			{
				base.DepthMask = value;
				re.checkAndLogErrors("setDepthMask");
			}
		}

		public override void setDepthRange(float zpos, float zscale, int near, int far)
		{
			base.setDepthRange(zpos, zscale, near, far);
			re.checkAndLogErrors("setDepthRange");
		}

		public override void setLightAmbientColor(int light, float[] color)
		{
			base.setLightAmbientColor(light, color);
			re.checkAndLogErrors("setLightAmbientColor");
		}

		public override void setLightConstantAttenuation(int light, float constant)
		{
			base.setLightConstantAttenuation(light, constant);
			re.checkAndLogErrors("setLightConstantAttenuation");
		}

		public override void setLightDiffuseColor(int light, float[] color)
		{
			base.setLightDiffuseColor(light, color);
			re.checkAndLogErrors("setLightDiffuseColor");
		}

		public override void setLightDirection(int light, float[] direction)
		{
			base.setLightDirection(light, direction);
			re.checkAndLogErrors("setLightDirection");
		}

		public override void setLightLinearAttenuation(int light, float linear)
		{
			base.setLightLinearAttenuation(light, linear);
			re.checkAndLogErrors("setLightLinearAttenuation");
		}

		public override int LightMode
		{
			set
			{
				base.LightMode = value;
				re.checkAndLogErrors("setLightMode");
			}
		}

		public override float[] LightModelAmbientColor
		{
			set
			{
				base.LightModelAmbientColor = value;
				re.checkAndLogErrors("setLightModelAmbientColor");
			}
		}

		public override void setLightPosition(int light, float[] position)
		{
			base.setLightPosition(light, position);
			re.checkAndLogErrors("setLightPosition");
		}

		public override void setLightQuadraticAttenuation(int light, float quadratic)
		{
			base.setLightQuadraticAttenuation(light, quadratic);
			re.checkAndLogErrors("setLightQuadraticAttenuation");
		}

		public override void setLightSpecularColor(int light, float[] color)
		{
			base.setLightSpecularColor(light, color);
			re.checkAndLogErrors("setLightSpecularColor");
		}

		public override void setLightSpotCutoff(int light, float cutoff)
		{
			base.setLightSpotCutoff(light, cutoff);
			re.checkAndLogErrors("setLightSpotCutoff");
		}

		public override void setLightSpotExponent(int light, float exponent)
		{
			base.setLightSpotExponent(light, exponent);
			re.checkAndLogErrors("setLightSpotExponent");
		}

		public override void setLightType(int light, int type, int kind)
		{
			base.setLightType(light, type, kind);
			re.checkAndLogErrors("setLightType");
		}

		public override int LogicOp
		{
			set
			{
				base.LogicOp = value;
				re.checkAndLogErrors("setLogicOp");
			}
		}

		public override float[] MaterialAmbientColor
		{
			set
			{
				base.MaterialAmbientColor = value;
				re.checkAndLogErrors("setMaterialAmbientColor");
			}
		}

		public override float[] MaterialDiffuseColor
		{
			set
			{
				base.MaterialDiffuseColor = value;
				re.checkAndLogErrors("setMaterialDiffuseColor");
			}
		}

		public override float[] MaterialEmissiveColor
		{
			set
			{
				base.MaterialEmissiveColor = value;
				re.checkAndLogErrors("setMaterialEmissiveColor");
			}
		}

		public override float[] MaterialSpecularColor
		{
			set
			{
				base.MaterialSpecularColor = value;
				re.checkAndLogErrors("setMaterialSpecularColor");
			}
		}

		public override float[] Matrix
		{
			set
			{
				base.Matrix = value;
				re.checkAndLogErrors("setMatrix");
			}
		}

		public override int MatrixMode
		{
			set
			{
				base.MatrixMode = value;
				re.checkAndLogErrors("setMatrixMode");
			}
		}

		public override void multMatrix(float[] values)
		{
			base.multMatrix(values);
			re.checkAndLogErrors("multMatrix");
		}

		public override float[] ModelMatrix
		{
			set
			{
				base.ModelMatrix = value;
				re.checkAndLogErrors("setModelMatrix");
			}
		}

		public override void endModelViewMatrixUpdate()
		{
			base.endModelViewMatrixUpdate();
			re.checkAndLogErrors("endModelViewMatrixUpdate");
		}

		public override float[] ModelViewMatrix
		{
			set
			{
				base.ModelViewMatrix = value;
				re.checkAndLogErrors("setModelViewMatrix");
			}
		}

		public override void setMorphWeight(int index, float value)
		{
			base.setMorphWeight(index, value);
			re.checkAndLogErrors("setMorphWeight");
		}

		public override void setPatchDiv(int s, int t)
		{
			base.setPatchDiv(s, t);
			re.checkAndLogErrors("setPatchDiv");
		}

		public override int PatchPrim
		{
			set
			{
				base.PatchPrim = value;
				re.checkAndLogErrors("setPatchPrim");
			}
		}

		public override float[] ProjectionMatrix
		{
			set
			{
				base.ProjectionMatrix = value;
				re.checkAndLogErrors("setProjectionMatrix");
			}
		}

		public override int ShadeModel
		{
			set
			{
				base.ShadeModel = value;
				re.checkAndLogErrors("setShadeModel");
			}
		}

		public override void setTextureEnvironmentMapping(int u, int v)
		{
			base.setTextureEnvironmentMapping(u, v);
			re.checkAndLogErrors("setTextureEnvironmentMapping");
		}

		public override float[] TextureMatrix
		{
			set
			{
				base.TextureMatrix = value;
				re.checkAndLogErrors("setTextureMatrix");
			}
		}

		public override int TextureMipmapMaxLevel
		{
			set
			{
				base.TextureMipmapMaxLevel = value;
				re.checkAndLogErrors("setTextureMipmapMaxLevel");
			}
		}

		public override int TextureMipmapMinLevel
		{
			set
			{
				base.TextureMipmapMinLevel = value;
				re.checkAndLogErrors("setTextureMipmapMinLevel");
			}
		}

		public override int TextureMipmapMinFilter
		{
			set
			{
				base.TextureMipmapMinFilter = value;
				re.checkAndLogErrors("setTextureMipmapMinFilter");
			}
		}

		public override int TextureMipmapMagFilter
		{
			set
			{
				base.TextureMipmapMagFilter = value;
				re.checkAndLogErrors("setTextureMipmapMagFilter");
			}
		}

		public override void setTextureWrapMode(int s, int t)
		{
			base.setTextureWrapMode(s, t);
			re.checkAndLogErrors("setTextureWrapMode");
		}

		public override float[] VertexColor
		{
			set
			{
				base.VertexColor = value;
				re.checkAndLogErrors("setVertexColor");
			}
		}

		public override float[] ViewMatrix
		{
			set
			{
				base.ViewMatrix = value;
				re.checkAndLogErrors("setViewMatrix");
			}
		}

		public override void setViewport(int x, int y, int width, int height)
		{
			base.setViewport(x, y, width, height);
			re.checkAndLogErrors("setViewport");
		}

		public override void setUniform(int id, int value)
		{
			base.setUniform(id, value);
			re.checkAndLogErrors("setUniform");
		}

		public override void setUniform(int id, int value1, int value2)
		{
			base.setUniform(id, value1, value2);
			re.checkAndLogErrors("setUniform");
		}

		public override void setUniform(int id, float value)
		{
			base.setUniform(id, value);
			re.checkAndLogErrors("setUniform");
		}

		public override void setUniform2(int id, int[] values)
		{
			base.setUniform2(id, values);
			re.checkAndLogErrors("setUniform2");
		}

		public override void setUniform3(int id, int[] values)
		{
			base.setUniform3(id, values);
			re.checkAndLogErrors("setUniform3");
		}

		public override void setUniform3(int id, float[] values)
		{
			base.setUniform3(id, values);
			re.checkAndLogErrors("setUniform3");
		}

		public override void setUniform4(int id, int[] values)
		{
			base.setUniform4(id, values);
			re.checkAndLogErrors("setUniform4");
		}

		public override void setUniform4(int id, float[] values)
		{
			base.setUniform4(id, values);
			re.checkAndLogErrors("setUniform4");
		}

		public override void setUniformMatrix4(int id, int count, float[] values)
		{
			base.setUniformMatrix4(id, count, values);
			re.checkAndLogErrors("setUniformMatrix4");
		}

		public override int ColorTestFunc
		{
			set
			{
				base.ColorTestFunc = value;
				re.checkAndLogErrors("setColorTestFunc");
			}
		}

		public override int[] ColorTestMask
		{
			set
			{
				base.ColorTestMask = value;
				re.checkAndLogErrors("setColorTestMask");
			}
		}

		public override int[] ColorTestReference
		{
			set
			{
				base.ColorTestReference = value;
				re.checkAndLogErrors("setColorTestReference");
			}
		}

		public override void setTextureFunc(int func, bool alphaUsed, bool colorDoubled)
		{
			base.setTextureFunc(func, alphaUsed, colorDoubled);
			re.checkAndLogErrors("setTextureFunc");
		}

		public override int setBones(int count, float[] values)
		{
			int value = base.setBones(count, values);
			re.checkAndLogErrors("setBones");
			return value;
		}

		public override void setTextureMapMode(int mode, int proj)
		{
			base.setTextureMapMode(mode, proj);
			re.checkAndLogErrors("setTextureMapMode");
		}

		public override void setTexEnv(int name, int param)
		{
			base.setTexEnv(name, param);
			re.checkAndLogErrors("setTexEnv");
		}

		public override void setTexEnv(int name, float param)
		{
			base.setTexEnv(name, param);
			re.checkAndLogErrors("setTexEnv");
		}

		public override void endClearMode()
		{
			base.endClearMode();
			re.checkAndLogErrors("endClearMode");
		}

		public override void startClearMode(bool color, bool stencil, bool depth)
		{
			base.startClearMode(color, stencil, depth);
			re.checkAndLogErrors("startClearMode");
		}

		public override void attachShader(int program, int shader)
		{
			base.attachShader(program, shader);
			re.checkAndLogErrors("attachShader");
		}

		public override bool compilerShader(int shader, string source)
		{
			bool value = base.compilerShader(shader, source);
			re.checkAndLogErrors("compilerShader");
			return value;
		}

		public override int createProgram()
		{
			int value = base.createProgram();
			re.checkAndLogErrors("createProgram");
			return value;
		}

		public override void useProgram(int program)
		{
			base.useProgram(program);
			re.checkAndLogErrors("useProgram");
		}

		public override int createShader(int type)
		{
			int value = base.createShader(type);
			re.checkAndLogErrors("createShader");
			return value;
		}

		public override int getAttribLocation(int program, string name)
		{
			int value = base.getAttribLocation(program, name);
			re.checkAndLogErrors("getAttribLocation");
			return value;
		}

		public override void bindAttribLocation(int program, int index, string name)
		{
			base.bindAttribLocation(program, index, name);
			re.checkAndLogErrors("bindAttribLocation");
		}

		public override string getProgramInfoLog(int program)
		{
			string value = base.getProgramInfoLog(program);
			re.checkAndLogErrors("getProgramInfoLog");
			return value;
		}

		public override string getShaderInfoLog(int shader)
		{
			string value = base.getShaderInfoLog(shader);
			re.checkAndLogErrors("getShaderInfoLog");
			return value;
		}

		public override int getUniformLocation(int program, string name)
		{
			int value = base.getUniformLocation(program, name);
			re.checkAndLogErrors("getUniformLocation");
			return value;
		}

		public override bool linkProgram(int program)
		{
			bool value = base.linkProgram(program);
			re.checkAndLogErrors("linkProgram");
			return value;
		}

		public override bool validateProgram(int program)
		{
			bool value = base.validateProgram(program);
			re.checkAndLogErrors("validateProgram");
			return value;
		}

		public override bool isExtensionAvailable(string name)
		{
			bool value = base.isExtensionAvailable(name);
			re.checkAndLogErrors("isExtensionAvailable");
			return value;
		}

		public override void drawArrays(int type, int first, int count)
		{
			base.drawArrays(type, first, count);
			re.checkAndLogErrors("drawArrays");
		}

		public override void deleteBuffer(int buffer)
		{
			base.deleteBuffer(buffer);
			re.checkAndLogErrors("deleteBuffer");
		}

		public override int genBuffer()
		{
			int value = base.genBuffer();
			re.checkAndLogErrors("genBuffer");
			return value;
		}

		public override void setBufferData(int target, int size, Buffer buffer, int usage)
		{
			base.setBufferData(target, size, buffer, usage);
			re.checkAndLogErrors("setBufferData");
		}

		public override void setBufferSubData(int target, int offset, int size, Buffer buffer)
		{
			base.setBufferSubData(target, offset, size, buffer);
			re.checkAndLogErrors("setBufferSubData");
		}

		public override void bindBuffer(int target, int buffer)
		{
			base.bindBuffer(target, buffer);
			re.checkAndLogErrors("bindBuffer");
		}

		public override void enableClientState(int type)
		{
			base.enableClientState(type);
			re.checkAndLogErrors("enableClientState");
		}

		public override void enableVertexAttribArray(int id)
		{
			base.enableVertexAttribArray(id);
			re.checkAndLogErrors("enableVertexAttribArray");
		}

		public override void disableClientState(int type)
		{
			base.disableClientState(type);
			re.checkAndLogErrors("disableClientState");
		}

		public override void disableVertexAttribArray(int id)
		{
			base.disableVertexAttribArray(id);
			re.checkAndLogErrors("disableVertexAttribArray");
		}

		public override void setColorPointer(int size, int type, int stride, long offset)
		{
			base.setColorPointer(size, type, stride, offset);
			re.checkAndLogErrors("setColorPointer");
		}

		public override void setColorPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			base.setColorPointer(size, type, stride, bufferSize, buffer);
			re.checkAndLogErrors("setColorPointer");
		}

		public override void setNormalPointer(int type, int stride, long offset)
		{
			base.setNormalPointer(type, stride, offset);
			re.checkAndLogErrors("setNormalPointer");
		}

		public override void setNormalPointer(int type, int stride, int bufferSize, Buffer buffer)
		{
			base.setNormalPointer(type, stride, bufferSize, buffer);
			re.checkAndLogErrors("setNormalPointer");
		}

		public override void setTexCoordPointer(int size, int type, int stride, long offset)
		{
			base.setTexCoordPointer(size, type, stride, offset);
			re.checkAndLogErrors("setTexCoordPointer");
		}

		public override void setTexCoordPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			base.setTexCoordPointer(size, type, stride, bufferSize, buffer);
			re.checkAndLogErrors("setTexCoordPointer");
		}

		public override void setVertexPointer(int size, int type, int stride, long offset)
		{
			base.setVertexPointer(size, type, stride, offset);
			re.checkAndLogErrors("setVertexPointer");
		}

		public override void setVertexPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			base.setVertexPointer(size, type, stride, bufferSize, buffer);
			re.checkAndLogErrors("setVertexPointer");
		}

		public override void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, long offset)
		{
			base.setVertexAttribPointer(id, size, type, normalized, stride, offset);
			re.checkAndLogErrors("setVertexAttribPointer");
		}

		public override void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, int bufferSize, Buffer buffer)
		{
			base.setVertexAttribPointer(id, size, type, normalized, stride, bufferSize, buffer);
			re.checkAndLogErrors("setVertexAttribPointer");
		}

		public override void setPixelStore(int rowLength, int alignment)
		{
			base.setPixelStore(rowLength, alignment);
			re.checkAndLogErrors("setPixelStore");
		}

		public override int genTexture()
		{
			int value = base.genTexture();
			re.checkAndLogErrors("genTexture");
			return value;
		}

		public override void bindTexture(int texture)
		{
			base.bindTexture(texture);
			re.checkAndLogErrors("bindTexture");
		}

		public override void deleteTexture(int texture)
		{
			base.deleteTexture(texture);
			re.checkAndLogErrors("deleteTexture");
		}

		public override void setCompressedTexImage(int level, int internalFormat, int width, int height, int compressedSize, Buffer buffer)
		{
			base.setCompressedTexImage(level, internalFormat, width, height, compressedSize, buffer);
			re.checkAndLogErrors("setCompressedTexImage");
		}

		public override void setTexImage(int level, int internalFormat, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
			base.setTexImage(level, internalFormat, width, height, format, type, textureSize, buffer);
			re.checkAndLogErrors("setTexImage");
		}

		public override void setStencilOp(int fail, int zfail, int zpass)
		{
			base.setStencilOp(fail, zfail, zpass);
			re.checkAndLogErrors("setStencilOp");
		}

		public override void setStencilFunc(int func, int @ref, int mask)
		{
			base.setStencilFunc(func, @ref, mask);
			re.checkAndLogErrors("setStencilFunc");
		}

		public override void setAlphaFunc(int func, int @ref, int mask)
		{
			base.setAlphaFunc(func, @ref, mask);
			re.checkAndLogErrors("setAlphaFunc");
		}

		public override int BlendEquation
		{
			set
			{
				base.BlendEquation = value;
				re.checkAndLogErrors("setBlendEquation");
			}
		}

		public override float[] FogColor
		{
			set
			{
				base.FogColor = value;
				re.checkAndLogErrors("setFogColor");
			}
		}

		public override void setFogDist(float start, float end)
		{
			base.setFogDist(start, end);
			re.checkAndLogErrors("setFogDist");
		}

		public override bool FrontFace
		{
			set
			{
				base.FrontFace = value;
				re.checkAndLogErrors("setFrontFace");
			}
		}

		public override void setScissor(int x, int y, int width, int height)
		{
			base.setScissor(x, y, width, height);
			re.checkAndLogErrors("setScissor");
		}

		public override float[] TextureEnvColor
		{
			set
			{
				base.TextureEnvColor = value;
				re.checkAndLogErrors("setTextureEnvColor");
			}
		}

		public override void setFogHint()
		{
			base.setFogHint();
			re.checkAndLogErrors("setFogHint");
		}

		public override void setLineSmoothHint()
		{
			base.setLineSmoothHint();
			re.checkAndLogErrors("setLineSmoothHint");
		}

		public override float MaterialShininess
		{
			set
			{
				base.MaterialShininess = value;
				re.checkAndLogErrors("setMaterialShininess");
			}
		}

		public override void setTexSubImage(int level, int xOffset, int yOffset, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
			base.setTexSubImage(level, xOffset, yOffset, width, height, format, type, textureSize, buffer);
			re.checkAndLogErrors("setTexSubImage");
		}

		public override void beginQuery(int id)
		{
			base.beginQuery(id);
			re.checkAndLogErrors("beginQuery");
		}

		public override void endQuery()
		{
			base.endQuery();
			re.checkAndLogErrors("endQuery");
		}

		public override int genQuery()
		{
			int value = base.genQuery();
			re.checkAndLogErrors("genQuery");
			return value;
		}

		public override void drawBoundingBox(float[][] values)
		{
			base.drawBoundingBox(values);
			re.checkAndLogErrors("drawBoundingBox");
		}

		public override void endBoundingBox(VertexInfo vinfo)
		{
			base.endBoundingBox(vinfo);
			re.checkAndLogErrors("endBoundingBox");
		}

		public override void beginBoundingBox(int numberOfVertexBoundingBox)
		{
			base.beginBoundingBox(numberOfVertexBoundingBox);
			re.checkAndLogErrors("beginBoundingBox");
		}

		public override bool BoundingBoxVisible
		{
			get
			{
				bool value = base.BoundingBoxVisible;
				re.checkAndLogErrors("isBoundingBoxVisible");
				return value;
			}
		}

		public override int getQueryResult(int id)
		{
			int value = base.getQueryResult(id);
			re.checkAndLogErrors("getQueryResult");
			return value;
		}

		public override bool getQueryResultAvailable(int id)
		{
			bool value = base.getQueryResultAvailable(id);
			re.checkAndLogErrors("getQueryResultAvailable");
			return value;
		}

		public override void clear(float red, float green, float blue, float alpha)
		{
			base.clear(red, green, blue, alpha);
			re.checkAndLogErrors("clear");
		}

		public override void copyTexSubImage(int level, int xOffset, int yOffset, int x, int y, int width, int height)
		{
			base.copyTexSubImage(level, xOffset, yOffset, x, y, width, height);
			re.checkAndLogErrors("copyTexSubImage");
		}

		public override void getTexImage(int level, int format, int type, Buffer buffer)
		{
			base.getTexImage(level, format, type, buffer);
			re.checkAndLogErrors("getTexImage");
		}

		public override void setWeightPointer(int size, int type, int stride, long offset)
		{
			base.setWeightPointer(size, type, stride, offset);
			re.checkAndLogErrors("setWeightPointer");
		}

		public override void setWeightPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			base.setWeightPointer(size, type, stride, bufferSize, buffer);
			re.checkAndLogErrors("setWeightPointer");
		}

		public override IREBufferManager BufferManager
		{
			get
			{
				IREBufferManager value = base.BufferManager;
				re.checkAndLogErrors("getBufferManager");
				return value;
			}
		}

		public override bool canAllNativeVertexInfo()
		{
			bool value = base.canAllNativeVertexInfo();
			re.checkAndLogErrors("canAllNativeVertexInfo");
			return value;
		}

		public override bool canNativeSpritesPrimitive()
		{
			bool value = base.canNativeSpritesPrimitive();
			re.checkAndLogErrors("canNativeSpritesPrimitive");
			return value;
		}

		public override void setVertexInfo(VertexInfo vinfo, bool allNativeVertexInfo, bool useVertexColor, bool useTexture, int type)
		{
			base.setVertexInfo(vinfo, allNativeVertexInfo, useVertexColor, useTexture, type);
			re.checkAndLogErrors("setVertexInfo");
		}

		public override void setProgramParameter(int program, int parameter, int value)
		{
			base.setProgramParameter(program, parameter, value);
			re.checkAndLogErrors("setProgramParameter");
		}

		public override bool QueryAvailable
		{
			get
			{
				bool value = base.QueryAvailable;
				re.checkAndLogErrors("isQueryAvailable");
				return value;
			}
		}

		public override bool ShaderAvailable
		{
			get
			{
				bool value = base.ShaderAvailable;
				re.checkAndLogErrors("isShaderAvailable");
				return value;
			}
		}

		public override void bindBufferBase(int target, int bindingPoint, int buffer)
		{
			base.bindBufferBase(target, bindingPoint, buffer);
			re.checkAndLogErrors("bindBufferBase");
		}

		public override int getUniformBlockIndex(int program, string name)
		{
			int value = base.getUniformBlockIndex(program, name);
			re.checkAndLogErrors("getUniformBlockIndex");
			return value;
		}

		public override void setUniformBlockBinding(int program, int blockIndex, int bindingPoint)
		{
			base.setUniformBlockBinding(program, blockIndex, bindingPoint);
			re.checkAndLogErrors("setUniformBlockBinding");
		}

		public override int getUniformIndex(int program, string name)
		{
			int value = base.getUniformIndex(program, name);
			re.checkAndLogErrors("getUniformIndex");
			return value;
		}

		public override int[] getUniformIndices(int program, string[] names)
		{
			int[] value = base.getUniformIndices(program, names);
			re.checkAndLogErrors("getUniformIndices");
			return value;
		}

		public override int getActiveUniformOffset(int program, int uniformIndex)
		{
			int value = base.getActiveUniformOffset(program, uniformIndex);
			re.checkAndLogErrors("getActiveUniformOffset");
			return value;
		}

		public override void bindFramebuffer(int target, int framebuffer)
		{
			base.bindFramebuffer(target, framebuffer);
			re.checkAndLogErrors("bindFramebuffer");
		}

		public override void bindRenderbuffer(int renderbuffer)
		{
			base.bindRenderbuffer(renderbuffer);
			re.checkAndLogErrors("bindRenderbuffer");
		}

		public override void deleteFramebuffer(int framebuffer)
		{
			base.deleteFramebuffer(framebuffer);
			re.checkAndLogErrors("deleteFramebuffer");
		}

		public override void deleteRenderbuffer(int renderbuffer)
		{
			base.deleteRenderbuffer(renderbuffer);
			re.checkAndLogErrors("deleteRenderbuffer");
		}

		public override int genFramebuffer()
		{
			int value = base.genFramebuffer();
			re.checkAndLogErrors("genFramebuffer");
			return value;
		}

		public override int genRenderbuffer()
		{
			int value = base.genRenderbuffer();
			re.checkAndLogErrors("genRenderbuffer");
			return value;
		}

		public override bool FramebufferObjectAvailable
		{
			get
			{
				bool value = base.FramebufferObjectAvailable;
				re.checkAndLogErrors("isFramebufferObjectAvailable");
				return value;
			}
		}

		public override void setFramebufferRenderbuffer(int target, int attachment, int renderbuffer)
		{
			base.setFramebufferRenderbuffer(target, attachment, renderbuffer);
			re.checkAndLogErrors("setFramebufferRenderbuffer");
		}

		public override void setFramebufferTexture(int target, int attachment, int texture, int level)
		{
			base.setFramebufferTexture(target, attachment, texture, level);
			re.checkAndLogErrors("setFramebufferTexture");
		}

		public override void setRenderbufferStorage(int internalFormat, int width, int height)
		{
			base.setRenderbufferStorage(internalFormat, width, height);
			re.checkAndLogErrors("setRenderbufferStorage");
		}

		public override void bindVertexArray(int id)
		{
			base.bindVertexArray(id);
			re.checkAndLogErrors("bindVertexArray");
		}

		public override void deleteVertexArray(int id)
		{
			base.deleteVertexArray(id);
			re.checkAndLogErrors("deleteVertexArray");
		}

		public override int genVertexArray()
		{
			int value = base.genVertexArray();
			re.checkAndLogErrors("genVertexArray");
			return value;
		}

		public override bool VertexArrayAvailable
		{
			get
			{
				bool value = base.VertexArrayAvailable;
				re.checkAndLogErrors("isVertexArrayAvailable");
				return value;
			}
		}

		public override void multiDrawArrays(int primitive, IntBuffer first, IntBuffer count)
		{
			base.multiDrawArrays(primitive, first, count);
			re.checkAndLogErrors("multiDrawArrays");
		}

		public override void drawArraysBurstMode(int primitive, int first, int count)
		{
			base.drawArraysBurstMode(primitive, first, count);
			re.checkAndLogErrors("drawArraysBurstMode");
		}

		public override void setPixelTransfer(int parameter, int value)
		{
			base.setPixelTransfer(parameter, value);
			re.checkAndLogErrors("setPixelTransfer");
		}

		public override void setPixelTransfer(int parameter, float value)
		{
			base.setPixelTransfer(parameter, value);
			re.checkAndLogErrors("setPixelTransfer");
		}

		public override void setPixelTransfer(int parameter, bool value)
		{
			base.setPixelTransfer(parameter, value);
			re.checkAndLogErrors("setPixelTransfer");
		}

		public override void setPixelMap(int map, int mapSize, Buffer buffer)
		{
			base.setPixelMap(map, mapSize, buffer);
			re.checkAndLogErrors("setPixelMap");
		}

		public override bool canNativeClut(int textureAddress, int pixelFormat, bool textureSwizzle)
		{
			bool value = base.canNativeClut(textureAddress, pixelFormat, textureSwizzle);
			re.checkAndLogErrors("canNativeClut");
			return value;
		}

		public override int ActiveTexture
		{
			set
			{
				base.ActiveTexture = value;
				re.checkAndLogErrors("setActiveTexture");
			}
		}

		public override void setTextureFormat(int pixelFormat, bool swizzle)
		{
			base.setTextureFormat(pixelFormat, swizzle);
			re.checkAndLogErrors("setTextureFormat");
		}

		public override void bindActiveTexture(int index, int texture)
		{
			base.bindActiveTexture(index, texture);
			re.checkAndLogErrors("bindActiveTexture");
		}

		public override float MaxTextureAnisotropy
		{
			get
			{
				float value = base.MaxTextureAnisotropy;
				re.checkAndLogErrors("getMaxTextureAnisotropy");
				return value;
			}
		}

		public override float TextureAnisotropy
		{
			set
			{
				base.TextureAnisotropy = value;
				re.checkAndLogErrors("setTextureAnisotropy");
			}
		}

		public override string ShadingLanguageVersion
		{
			get
			{
				string value = base.ShadingLanguageVersion;
				re.checkAndLogErrors("getShadingLanguageVersion");
				return value;
			}
		}

		public override void setBlendDFix(int sfix, float[] color)
		{
			base.setBlendDFix(sfix, color);
			re.checkAndLogErrors("setBlendDFix");
		}

		public override void setBlendSFix(int dfix, float[] color)
		{
			base.setBlendSFix(dfix, color);
			re.checkAndLogErrors("setBlendSFix");
		}

		public override void waitForRenderingCompletion()
		{
			base.waitForRenderingCompletion();
			re.checkAndLogErrors("waitForRenderingCompletion");
		}

		public override bool canReadAllVertexInfo()
		{
			bool value = base.canReadAllVertexInfo();
			re.checkAndLogErrors("canReadAllVertexInfo");
			return value;
		}

		public override void readStencil(int x, int y, int width, int height, int bufferSize, Buffer buffer)
		{
			base.readStencil(x, y, width, height, bufferSize, buffer);
			re.checkAndLogErrors("readStencil");
		}

		public override void blitFramebuffer(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, int mask, int filter)
		{
			base.blitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter);
			re.checkAndLogErrors("blitFramebuffer");
		}

		public override bool setCopyRedToAlpha(bool copyRedToAlpha)
		{
			bool value = base.setCopyRedToAlpha(copyRedToAlpha);
			re.checkAndLogErrors("setCopyRedToAlpha");
			return value;
		}

		public override void drawElements(int primitive, int count, int indexType, Buffer indices, int indicesOffset)
		{
			base.drawElements(primitive, count, indexType, indices, indicesOffset);
			re.checkAndLogErrors("drawElements");
		}

		public override void drawElements(int primitive, int count, int indexType, long indicesOffset)
		{
			base.drawElements(primitive, count, indexType, indicesOffset);
			re.checkAndLogErrors("drawElements");
		}

		public override void multiDrawElements(int primitive, IntBuffer first, IntBuffer count, int indexType, long indicesOffset)
		{
			base.multiDrawElements(primitive, first, count, indexType, indicesOffset);
			re.checkAndLogErrors("multiDrawElements");
		}

		public override void drawElementsBurstMode(int primitive, int count, int indexType, long indicesOffset)
		{
			base.drawElementsBurstMode(primitive, count, indexType, indicesOffset);
			re.checkAndLogErrors("drawElementsBurstMode");
		}
	}

}