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

	using IREBufferManager = pspsharp.graphics.RE.buffer.IREBufferManager;
	using CpuDurationStatistics = pspsharp.util.CpuDurationStatistics;
	using DurationStatistics = pspsharp.util.DurationStatistics;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class StatisticsProxy : BaseRenderingEngineProxy
	{
		private DurationStatistics[] statistics;

		public StatisticsProxy(IRenderingEngine proxy) : base(proxy)
		{

			addStatistic("attachShader", 0);
			addStatistic("beginBoundingBox", 1);
			addStatistic("beginQuery", 3);
			addStatistic("bindBuffer", 4);
			addStatistic("bindBufferBase", 5);
			addStatistic("bindFramebuffer", 6);
			addStatistic("bindRenderbuffer", 7);
			addStatistic("bindTexture", 8);
			addStatistic("canAllNativeVertexInfo", 9);
			addStatistic("canNativeSpritesPrimitive", 10);
			addStatistic("clear", 11);
			addStatistic("compilerShader", 12);
			addStatistic("copyTexSubImage", 13);
			addStatistic("createProgram", 14);
			addStatistic("createShader", 15);
			addStatistic("deleteBuffer", 16);
			addStatistic("deleteFramebuffer", 17);
			addStatistic("deleteRenderbuffer", 18);
			addStatistic("deleteTexture", 19);
			addStatistic("disableClientState", 20);
			addStatistic("disableFlag", 21);
			addStatistic("disableVertexAttribArray", 22);
			addStatistic("drawArrays", 23);
			addStatistic("drawBoundingBox", 24);
			addStatistic("enableClientState", 31);
			addStatistic("enableFlag", 32);
			addStatistic("enableVertexAttribArray", 33);
			addStatistic("endBoundingBox", 34);
			addStatistic("endClearMode", 35);
			addStatistic("endDirectRendering", 36);
			addStatistic("endDisplay", 37);
			addStatistic("endModelViewMatrixUpdate", 39);
			addStatistic("endQuery", 40);
			addStatistic("genBuffer", 41);
			addStatistic("genFramebuffer", 42);
			addStatistic("genQuery", 43);
			addStatistic("genRenderbuffer", 44);
			addStatistic("genTexture", 45);
			addStatistic("getAttribLocation", 46);
			addStatistic("getBufferManager", 47);
			addStatistic("getProgramInfoLog", 48);
			addStatistic("getQueryResult", 49);
			addStatistic("getQueryResultAvailable", 50);
			addStatistic("getShaderInfoLog", 51);
			addStatistic("getTexImage", 52);
			addStatistic("getUniformBlockIndex", 53);
			addStatistic("getUniformLocation", 54);
			addStatistic("isBoundingBoxVisible", 56);
			addStatistic("isExtensionAvailable", 57);
			addStatistic("isFramebufferObjectAvailable", 58);
			addStatistic("isQueryAvailable", 59);
			addStatistic("isShaderAvailable", 60);
			addStatistic("linkProgram", 61);
			addStatistic("setAlphaFunc", 63);
			addStatistic("setBlendColor", 64);
			addStatistic("setBlendEquation", 65);
			addStatistic("setBlendFunc", 66);
			addStatistic("setBones", 67);
			addStatistic("setBufferData", 68);
			addStatistic("setBufferSubData", 69);
			addStatistic("setColorMask", 70);
			addStatistic("setColorMask", 71);
			addStatistic("setColorMaterial", 72);
			addStatistic("setColorPointer", 73);
			addStatistic("setColorPointer", 74);
			addStatistic("setColorTestFunc", 75);
			addStatistic("setColorTestMask", 76);
			addStatistic("setColorTestReference", 77);
			addStatistic("setCompressedTexImage", 78);
			addStatistic("setDepthFunc", 79);
			addStatistic("setDepthMask", 80);
			addStatistic("setDepthRange", 81);
			addStatistic("setFogColor", 82);
			addStatistic("setFogDist", 83);
			addStatistic("setFogHint", 84);
			addStatistic("setFramebufferRenderbuffer", 85);
			addStatistic("setFramebufferTexture", 86);
			addStatistic("setFrontFace", 87);
			addStatistic("setGeContext", 88);
			addStatistic("setLightAmbientColor", 89);
			addStatistic("setLightColor", 90);
			addStatistic("setLightConstantAttenuation", 91);
			addStatistic("setLightDiffuseColor", 92);
			addStatistic("setLightDirection", 93);
			addStatistic("setLightLinearAttenuation", 94);
			addStatistic("setLightMode", 95);
			addStatistic("setLightModelAmbientColor", 96);
			addStatistic("setLightPosition", 97);
			addStatistic("setLightQuadraticAttenuation", 98);
			addStatistic("setLightSpecularColor", 99);
			addStatistic("setLightSpotCutoff", 100);
			addStatistic("setLightSpotExponent", 101);
			addStatistic("setLightType", 102);
			addStatistic("setLineSmoothHint", 103);
			addStatistic("setLogicOp", 104);
			addStatistic("setMaterialAmbientColor", 105);
			addStatistic("setMaterialColor", 106);
			addStatistic("setMaterialDiffuseColor", 107);
			addStatistic("setMaterialEmissiveColor", 108);
			addStatistic("setMaterialShininess", 109);
			addStatistic("setMaterialSpecularColor", 110);
			addStatistic("setMatrix", 111);
			addStatistic("setModelMatrix", 112);
			addStatistic("setModelViewMatrix", 113);
			addStatistic("setMorphWeight", 114);
			addStatistic("setNormalPointer", 115);
			addStatistic("setNormalPointer", 116);
			addStatistic("setPatchDiv", 117);
			addStatistic("setPatchPrim", 118);
			addStatistic("setPixelStore", 119);
			addStatistic("setProgramParameter", 120);
			addStatistic("setProjectionMatrix", 121);
			addStatistic("setRenderbufferStorage", 122);
			addStatistic("setRenderingEngine", 123);
			addStatistic("setScissor", 124);
			addStatistic("setShadeModel", 125);
			addStatistic("setStencilFunc", 126);
			addStatistic("setStencilOp", 127);
			addStatistic("setTexCoordPointer", 128);
			addStatistic("setTexCoordPointer", 129);
			addStatistic("setTexEnv", 130);
			addStatistic("setTexEnv", 131);
			addStatistic("setTexImage", 132);
			addStatistic("setTexSubImage", 133);
			addStatistic("setTextureEnvColor", 134);
			addStatistic("setTextureEnvironmentMapping", 135);
			addStatistic("setTextureFunc", 136);
			addStatistic("setTextureMapMode", 137);
			addStatistic("setTextureMatrix", 138);
			addStatistic("setTextureMipmapMagFilter", 139);
			addStatistic("setTextureMipmapMaxLevel", 140);
			addStatistic("setTextureMipmapMinFilter", 141);
			addStatistic("setTextureMipmapMinLevel", 142);
			addStatistic("setTextureWrapMode", 143);
			addStatistic("setUniform", 144);
			addStatistic("setUniform", 145);
			addStatistic("setUniform", 146);
			addStatistic("setUniform2", 147);
			addStatistic("setUniform3", 148);
			addStatistic("setUniform4", 149);
			addStatistic("setUniformBlockBinding", 150);
			addStatistic("setUniformMatrix4", 151);
			addStatistic("setVertexAttribPointer", 152);
			addStatistic("setVertexAttribPointer", 153);
			addStatistic("setVertexColor", 154);
			addStatistic("setVertexInfo", 155);
			addStatistic("setVertexPointer", 156);
			addStatistic("setVertexPointer", 157);
			addStatistic("setViewMatrix", 158);
			addStatistic("setViewport", 159);
			addStatistic("setWeightPointer", 160);
			addStatistic("setWeightPointer", 161);
			addStatistic("startClearMode", 162);
			addStatistic("startDirectRendering", 163);
			addStatistic("startDisplay", 164);
			addStatistic("useProgram", 165);
			addStatistic("validateProgram", 166);
			addStatistic("bindVertexArray", 167);
			addStatistic("deleteVertexArray", 168);
			addStatistic("genVertexArray", 169);
			addStatistic("isVertexArrayAvailable", 170);
			addStatistic("multiDrawArrays", 171);
			addStatistic("multMatrix", 172);
			addStatistic("setMatrix", 173);
			addStatistic("setMatrixMode", 174);
			addStatistic("setPixelTransfer", 175);
			addStatistic("setPixelTransfer", 176);
			addStatistic("setPixelTransfer", 177);
			addStatistic("setPixelMap", 178);
			addStatistic("canNativeClut", 179);
			addStatistic("setActiveTexture", 180);
			addStatistic("setTextureFormat", 181);
			addStatistic("getUniformIndex", 182);
			addStatistic("getUniformIndices", 183);
			addStatistic("getActiveUniformOffset", 184);
			addStatistic("bindAttribLocation", 185);
			addStatistic("setUniform4", 186);
			addStatistic("drawArraysBurstMode", 187);
			addStatistic("bindActiveTexture", 188);
			addStatistic("setTextureAnisotropy", 189);
			addStatistic("getMaxTextureAnisotropy", 190);
			addStatistic("getShadingLanguageVersion", 191);
			addStatistic("setBlendSFix", 192);
			addStatistic("setBlendDFix", 193);
			addStatistic("setUniform3", 194);
			addStatistic("waitForRenderingCompletion", 195);
			addStatistic("canReadAllVertexInfo", 196);
			addStatistic("readStencil", 197);
			addStatistic("blitFramebuffer", 198);
			addStatistic("checkErrors", 199);
			addStatistic("setCopyRedToAlpha", 200);
			addStatistic("drawElements", 201);
			addStatistic("drawElements", 202);
			addStatistic("multiDrawElements", 203);
			addStatistic("drawElementsBurstMode", 204);
		}

		private void addStatistic(string name, int index)
		{
			if (statistics == null || index >= statistics.Length)
			{
				DurationStatistics[] newStatistics = new DurationStatistics[index + 1];
				if (statistics != null)
				{
					Array.Copy(statistics, 0, newStatistics, 0, statistics.Length);
				}
				statistics = newStatistics;
			}
			statistics[index] = new CpuDurationStatistics(string.Format("{0,-30}", name));
		}

		public override void exit()
		{
			Array.Sort(statistics);
			VideoEngine.log_Renamed.info("RenderingEngine methods:");

			int lastStatistics = -1;
			for (int i = statistics.Length - 1; i >= 0; i--)
			{
				if (statistics[i].numberCalls > 0)
				{
					lastStatistics = i;
					break;
				}
			}

			for (int i = 0; i <= lastStatistics; i++)
			{
				DurationStatistics statistic = statistics[i];
				if (statistic.numberCalls == 0)
				{
					break;
				}
				VideoEngine.log_Renamed.info("    " + statistic);
			}

			base.exit();
		}

		public override void attachShader(int program, int shader)
		{
			DurationStatistics statistic = statistics[0];
			statistic.start();
			base.attachShader(program, shader);
			statistic.end();
		}

		public override void beginBoundingBox(int numberOfVertexBoundingBox)
		{
			DurationStatistics statistic = statistics[1];
			statistic.start();
			base.beginBoundingBox(numberOfVertexBoundingBox);
			statistic.end();
		}

		public override void beginQuery(int id)
		{
			DurationStatistics statistic = statistics[3];
			statistic.start();
			base.beginQuery(id);
			statistic.end();
		}

		public override void bindBuffer(int target, int buffer)
		{
			DurationStatistics statistic = statistics[4];
			statistic.start();
			base.bindBuffer(target, buffer);
			statistic.end();
		}

		public override void bindBufferBase(int target, int bindingPoint, int buffer)
		{
			DurationStatistics statistic = statistics[5];
			statistic.start();
			base.bindBufferBase(target, bindingPoint, buffer);
			statistic.end();
		}

		public override void bindFramebuffer(int target, int framebuffer)
		{
			DurationStatistics statistic = statistics[6];
			statistic.start();
			base.bindFramebuffer(target, framebuffer);
			statistic.end();
		}

		public override void bindRenderbuffer(int renderbuffer)
		{
			DurationStatistics statistic = statistics[7];
			statistic.start();
			base.bindRenderbuffer(renderbuffer);
			statistic.end();
		}

		public override void bindTexture(int texture)
		{
			DurationStatistics statistic = statistics[8];
			statistic.start();
			base.bindTexture(texture);
			statistic.end();
		}

		public override bool canAllNativeVertexInfo()
		{
			DurationStatistics statistic = statistics[9];
			statistic.start();
			bool value = base.canAllNativeVertexInfo();
			statistic.end();
			return value;
		}

		public override bool canNativeSpritesPrimitive()
		{
			DurationStatistics statistic = statistics[10];
			statistic.start();
			bool value = base.canNativeSpritesPrimitive();
			statistic.end();
			return value;
		}

		public override void clear(float red, float green, float blue, float alpha)
		{
			DurationStatistics statistic = statistics[11];
			statistic.start();
			base.clear(red, green, blue, alpha);
			statistic.end();
		}

		public override bool compilerShader(int shader, string source)
		{
			DurationStatistics statistic = statistics[12];
			statistic.start();
			bool value = base.compilerShader(shader, source);
			statistic.end();
			return value;
		}

		public override void copyTexSubImage(int level, int offset, int offset2, int x, int y, int width, int height)
		{
			DurationStatistics statistic = statistics[13];
			statistic.start();
			base.copyTexSubImage(level, offset, offset2, x, y, width, height);
			statistic.end();
		}

		public override int createProgram()
		{
			DurationStatistics statistic = statistics[14];
			statistic.start();
			int value = base.createProgram();
			statistic.end();
			return value;
		}

		public override int createShader(int type)
		{
			DurationStatistics statistic = statistics[15];
			statistic.start();
			int value = base.createShader(type);
			statistic.end();
			return value;
		}

		public override void deleteBuffer(int buffer)
		{
			DurationStatistics statistic = statistics[16];
			statistic.start();
			base.deleteBuffer(buffer);
			statistic.end();
		}

		public override void deleteFramebuffer(int framebuffer)
		{
			DurationStatistics statistic = statistics[17];
			statistic.start();
			base.deleteFramebuffer(framebuffer);
			statistic.end();
		}

		public override void deleteRenderbuffer(int renderbuffer)
		{
			DurationStatistics statistic = statistics[18];
			statistic.start();
			base.deleteRenderbuffer(renderbuffer);
			statistic.end();
		}

		public override void deleteTexture(int texture)
		{
			DurationStatistics statistic = statistics[19];
			statistic.start();
			base.deleteTexture(texture);
			statistic.end();
		}

		public override void disableClientState(int type)
		{
			DurationStatistics statistic = statistics[20];
			statistic.start();
			base.disableClientState(type);
			statistic.end();
		}

		public override void disableFlag(int flag)
		{
			DurationStatistics statistic = statistics[21];
			statistic.start();
			base.disableFlag(flag);
			statistic.end();
		}

		public override void disableVertexAttribArray(int id)
		{
			DurationStatistics statistic = statistics[22];
			statistic.start();
			base.disableVertexAttribArray(id);
			statistic.end();
		}

		public override void drawArrays(int type, int first, int count)
		{
			DurationStatistics statistic = statistics[23];
			statistic.start();
			base.drawArrays(type, first, count);
			statistic.end();
		}

		public override void drawBoundingBox(float[][] values)
		{
			DurationStatistics statistic = statistics[24];
			statistic.start();
			base.drawBoundingBox(values);
			statistic.end();
		}

		public override void enableClientState(int type)
		{
			DurationStatistics statistic = statistics[31];
			statistic.start();
			base.enableClientState(type);
			statistic.end();
		}

		public override void enableFlag(int flag)
		{
			DurationStatistics statistic = statistics[32];
			statistic.start();
			base.enableFlag(flag);
			statistic.end();
		}

		public override void enableVertexAttribArray(int id)
		{
			DurationStatistics statistic = statistics[33];
			statistic.start();
			base.enableVertexAttribArray(id);
			statistic.end();
		}

		public override void endBoundingBox(VertexInfo vinfo)
		{
			DurationStatistics statistic = statistics[34];
			statistic.start();
			base.endBoundingBox(vinfo);
			statistic.end();
		}

		public override void endClearMode()
		{
			DurationStatistics statistic = statistics[35];
			statistic.start();
			base.endClearMode();
			statistic.end();
		}

		public override void endDirectRendering()
		{
			DurationStatistics statistic = statistics[36];
			statistic.start();
			base.endDirectRendering();
			statistic.end();
		}

		public override void endDisplay()
		{
			DurationStatistics statistic = statistics[37];
			statistic.start();
			base.endDisplay();
			statistic.end();
		}

		public override void endModelViewMatrixUpdate()
		{
			DurationStatistics statistic = statistics[39];
			statistic.start();
			base.endModelViewMatrixUpdate();
			statistic.end();
		}

		public override void endQuery()
		{
			DurationStatistics statistic = statistics[40];
			statistic.start();
			base.endQuery();
			statistic.end();
		}

		public override int genBuffer()
		{
			DurationStatistics statistic = statistics[41];
			statistic.start();
			int value = base.genBuffer();
			statistic.end();
			return value;
		}

		public override int genFramebuffer()
		{
			DurationStatistics statistic = statistics[42];
			statistic.start();
			int value = base.genFramebuffer();
			statistic.end();
			return value;
		}

		public override int genQuery()
		{
			DurationStatistics statistic = statistics[43];
			statistic.start();
			int value = base.genQuery();
			statistic.end();
			return value;
		}

		public override int genRenderbuffer()
		{
			DurationStatistics statistic = statistics[44];
			statistic.start();
			int value = base.genRenderbuffer();
			statistic.end();
			return value;
		}

		public override int genTexture()
		{
			DurationStatistics statistic = statistics[45];
			statistic.start();
			int value = base.genTexture();
			statistic.end();
			return value;
		}

		public override int getAttribLocation(int program, string name)
		{
			DurationStatistics statistic = statistics[46];
			statistic.start();
			int value = base.getAttribLocation(program, name);
			statistic.end();
			return value;
		}

		public override IREBufferManager BufferManager
		{
			get
			{
				DurationStatistics statistic = statistics[47];
				statistic.start();
				IREBufferManager value = base.BufferManager;
				statistic.end();
				return value;
			}
		}

		public override string getProgramInfoLog(int program)
		{
			DurationStatistics statistic = statistics[48];
			statistic.start();
			string value = base.getProgramInfoLog(program);
			statistic.end();
			return value;
		}

		public override int getQueryResult(int id)
		{
			DurationStatistics statistic = statistics[49];
			statistic.start();
			int value = base.getQueryResult(id);
			statistic.end();
			return value;
		}

		public override bool getQueryResultAvailable(int id)
		{
			DurationStatistics statistic = statistics[50];
			statistic.start();
			bool value = base.getQueryResultAvailable(id);
			statistic.end();
			return value;
		}

		public override string getShaderInfoLog(int shader)
		{
			DurationStatistics statistic = statistics[51];
			statistic.start();
			string value = base.getShaderInfoLog(shader);
			statistic.end();
			return value;
		}

		public override void getTexImage(int level, int format, int type, Buffer buffer)
		{
			DurationStatistics statistic = statistics[52];
			statistic.start();
			base.getTexImage(level, format, type, buffer);
			statistic.end();
		}

		public override int getUniformBlockIndex(int program, string name)
		{
			DurationStatistics statistic = statistics[53];
			statistic.start();
			int value = base.getUniformBlockIndex(program, name);
			statistic.end();
			return value;
		}

		public override int getUniformLocation(int program, string name)
		{
			DurationStatistics statistic = statistics[54];
			statistic.start();
			int value = base.getUniformLocation(program, name);
			statistic.end();
			return value;
		}

		public override bool BoundingBoxVisible
		{
			get
			{
				DurationStatistics statistic = statistics[56];
				statistic.start();
				bool value = base.BoundingBoxVisible;
				statistic.end();
				return value;
			}
		}

		public override bool isExtensionAvailable(string name)
		{
			DurationStatistics statistic = statistics[57];
			statistic.start();
			bool value = base.isExtensionAvailable(name);
			statistic.end();
			return value;
		}

		public override bool FramebufferObjectAvailable
		{
			get
			{
				DurationStatistics statistic = statistics[58];
				statistic.start();
				bool value = base.FramebufferObjectAvailable;
				statistic.end();
				return value;
			}
		}

		public override bool QueryAvailable
		{
			get
			{
				DurationStatistics statistic = statistics[59];
				statistic.start();
				bool value = base.QueryAvailable;
				statistic.end();
				return value;
			}
		}

		public override bool ShaderAvailable
		{
			get
			{
				DurationStatistics statistic = statistics[60];
				statistic.start();
				bool value = base.ShaderAvailable;
				statistic.end();
				return value;
			}
		}

		public override bool linkProgram(int program)
		{
			DurationStatistics statistic = statistics[61];
			statistic.start();
			bool value = base.linkProgram(program);
			statistic.end();
			return value;
		}

		public override void setAlphaFunc(int func, int @ref, int mask)
		{
			DurationStatistics statistic = statistics[63];
			statistic.start();
			base.setAlphaFunc(func, @ref, mask);
			statistic.end();
		}

		public override float[] BlendColor
		{
			set
			{
				DurationStatistics statistic = statistics[64];
				statistic.start();
				base.BlendColor = value;
				statistic.end();
			}
		}

		public override int BlendEquation
		{
			set
			{
				DurationStatistics statistic = statistics[65];
				statistic.start();
				base.BlendEquation = value;
				statistic.end();
			}
		}

		public override void setBlendFunc(int src, int dst)
		{
			DurationStatistics statistic = statistics[66];
			statistic.start();
			base.setBlendFunc(src, dst);
			statistic.end();
		}

		public override int setBones(int count, float[] values)
		{
			DurationStatistics statistic = statistics[67];
			statistic.start();
			int value = base.setBones(count, values);
			statistic.end();
			return value;
		}

		public override void setBufferData(int target, int size, Buffer buffer, int usage)
		{
			DurationStatistics statistic = statistics[68];
			statistic.start();
			base.setBufferData(target, size, buffer, usage);
			statistic.end();
		}

		public override void setBufferSubData(int target, int offset, int size, Buffer buffer)
		{
			DurationStatistics statistic = statistics[69];
			statistic.start();
			base.setBufferSubData(target, offset, size, buffer);
			statistic.end();
		}

		public override void setColorMask(bool redWriteEnabled, bool greenWriteEnabled, bool blueWriteEnabled, bool alphaWriteEnabled)
		{
			DurationStatistics statistic = statistics[70];
			statistic.start();
			base.setColorMask(redWriteEnabled, greenWriteEnabled, blueWriteEnabled, alphaWriteEnabled);
			statistic.end();
		}

		public override void setColorMask(int redMask, int greenMask, int blueMask, int alphaMask)
		{
			DurationStatistics statistic = statistics[71];
			statistic.start();
			base.setColorMask(redMask, greenMask, blueMask, alphaMask);
			statistic.end();
		}

		public override void setColorMaterial(bool ambient, bool diffuse, bool specular)
		{
			DurationStatistics statistic = statistics[72];
			statistic.start();
			base.setColorMaterial(ambient, diffuse, specular);
			statistic.end();
		}

		public override void setColorPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			DurationStatistics statistic = statistics[73];
			statistic.start();
			base.setColorPointer(size, type, stride, bufferSize, buffer);
			statistic.end();
		}

		public override void setColorPointer(int size, int type, int stride, long offset)
		{
			DurationStatistics statistic = statistics[74];
			statistic.start();
			base.setColorPointer(size, type, stride, offset);
			statistic.end();
		}

		public override int ColorTestFunc
		{
			set
			{
				DurationStatistics statistic = statistics[75];
				statistic.start();
				base.ColorTestFunc = value;
				statistic.end();
			}
		}

		public override int[] ColorTestMask
		{
			set
			{
				DurationStatistics statistic = statistics[76];
				statistic.start();
				base.ColorTestMask = value;
				statistic.end();
			}
		}

		public override int[] ColorTestReference
		{
			set
			{
				DurationStatistics statistic = statistics[77];
				statistic.start();
				base.ColorTestReference = value;
				statistic.end();
			}
		}

		public override void setCompressedTexImage(int level, int internalFormat, int width, int height, int compressedSize, Buffer buffer)
		{
			DurationStatistics statistic = statistics[78];
			statistic.start();
			base.setCompressedTexImage(level, internalFormat, width, height, compressedSize, buffer);
			statistic.end();
		}

		public override int DepthFunc
		{
			set
			{
				DurationStatistics statistic = statistics[79];
				statistic.start();
				base.DepthFunc = value;
				statistic.end();
			}
		}

		public override bool DepthMask
		{
			set
			{
				DurationStatistics statistic = statistics[80];
				statistic.start();
				base.DepthMask = value;
				statistic.end();
			}
		}

		public override void setDepthRange(float zpos, float zscale, int near, int far)
		{
			DurationStatistics statistic = statistics[81];
			statistic.start();
			base.setDepthRange(zpos, zscale, near, far);
			statistic.end();
		}

		public override float[] FogColor
		{
			set
			{
				DurationStatistics statistic = statistics[82];
				statistic.start();
				base.FogColor = value;
				statistic.end();
			}
		}

		public override void setFogDist(float start, float end)
		{
			DurationStatistics statistic = statistics[83];
			statistic.start();
			base.setFogDist(start, end);
			statistic.end();
		}

		public override void setFogHint()
		{
			DurationStatistics statistic = statistics[84];
			statistic.start();
			base.setFogHint();
			statistic.end();
		}

		public override void setFramebufferRenderbuffer(int target, int attachment, int renderbuffer)
		{
			DurationStatistics statistic = statistics[85];
			statistic.start();
			base.setFramebufferRenderbuffer(target, attachment, renderbuffer);
			statistic.end();
		}

		public override void setFramebufferTexture(int target, int attachment, int texture, int level)
		{
			DurationStatistics statistic = statistics[86];
			statistic.start();
			base.setFramebufferTexture(target, attachment, texture, level);
			statistic.end();
		}

		public override bool FrontFace
		{
			set
			{
				DurationStatistics statistic = statistics[87];
				statistic.start();
				base.FrontFace = value;
				statistic.end();
			}
		}

		public override GeContext GeContext
		{
			set
			{
				DurationStatistics statistic = statistics[88];
				statistic.start();
				base.GeContext = value;
				statistic.end();
			}
		}

		public override void setLightAmbientColor(int light, float[] color)
		{
			DurationStatistics statistic = statistics[89];
			statistic.start();
			base.setLightAmbientColor(light, color);
			statistic.end();
		}

		public override void setLightConstantAttenuation(int light, float constant)
		{
			DurationStatistics statistic = statistics[91];
			statistic.start();
			base.setLightConstantAttenuation(light, constant);
			statistic.end();
		}

		public override void setLightDiffuseColor(int light, float[] color)
		{
			DurationStatistics statistic = statistics[92];
			statistic.start();
			base.setLightDiffuseColor(light, color);
			statistic.end();
		}

		public override void setLightDirection(int light, float[] direction)
		{
			DurationStatistics statistic = statistics[93];
			statistic.start();
			base.setLightDirection(light, direction);
			statistic.end();
		}

		public override void setLightLinearAttenuation(int light, float linear)
		{
			DurationStatistics statistic = statistics[94];
			statistic.start();
			base.setLightLinearAttenuation(light, linear);
			statistic.end();
		}

		public override int LightMode
		{
			set
			{
				DurationStatistics statistic = statistics[95];
				statistic.start();
				base.LightMode = value;
				statistic.end();
			}
		}

		public override float[] LightModelAmbientColor
		{
			set
			{
				DurationStatistics statistic = statistics[96];
				statistic.start();
				base.LightModelAmbientColor = value;
				statistic.end();
			}
		}

		public override void setLightPosition(int light, float[] position)
		{
			DurationStatistics statistic = statistics[97];
			statistic.start();
			base.setLightPosition(light, position);
			statistic.end();
		}

		public override void setLightQuadraticAttenuation(int light, float quadratic)
		{
			DurationStatistics statistic = statistics[98];
			statistic.start();
			base.setLightQuadraticAttenuation(light, quadratic);
			statistic.end();
		}

		public override void setLightSpecularColor(int light, float[] color)
		{
			DurationStatistics statistic = statistics[99];
			statistic.start();
			base.setLightSpecularColor(light, color);
			statistic.end();
		}

		public override void setLightSpotCutoff(int light, float cutoff)
		{
			DurationStatistics statistic = statistics[100];
			statistic.start();
			base.setLightSpotCutoff(light, cutoff);
			statistic.end();
		}

		public override void setLightSpotExponent(int light, float exponent)
		{
			DurationStatistics statistic = statistics[101];
			statistic.start();
			base.setLightSpotExponent(light, exponent);
			statistic.end();
		}

		public override void setLightType(int light, int type, int kind)
		{
			DurationStatistics statistic = statistics[102];
			statistic.start();
			base.setLightType(light, type, kind);
			statistic.end();
		}

		public override void setLineSmoothHint()
		{
			DurationStatistics statistic = statistics[103];
			statistic.start();
			base.setLineSmoothHint();
			statistic.end();
		}

		public override int LogicOp
		{
			set
			{
				DurationStatistics statistic = statistics[104];
				statistic.start();
				base.LogicOp = value;
				statistic.end();
			}
		}

		public override float[] MaterialAmbientColor
		{
			set
			{
				DurationStatistics statistic = statistics[105];
				statistic.start();
				base.MaterialAmbientColor = value;
				statistic.end();
			}
		}

		public override float[] MaterialDiffuseColor
		{
			set
			{
				DurationStatistics statistic = statistics[107];
				statistic.start();
				base.MaterialDiffuseColor = value;
				statistic.end();
			}
		}

		public override float[] MaterialEmissiveColor
		{
			set
			{
				DurationStatistics statistic = statistics[108];
				statistic.start();
				base.MaterialEmissiveColor = value;
				statistic.end();
			}
		}

		public override float MaterialShininess
		{
			set
			{
				DurationStatistics statistic = statistics[109];
				statistic.start();
				base.MaterialShininess = value;
				statistic.end();
			}
		}

		public override float[] MaterialSpecularColor
		{
			set
			{
				DurationStatistics statistic = statistics[110];
				statistic.start();
				base.MaterialSpecularColor = value;
				statistic.end();
			}
		}

		public override float[] ModelMatrix
		{
			set
			{
				DurationStatistics statistic = statistics[112];
				statistic.start();
				base.ModelMatrix = value;
				statistic.end();
			}
		}

		public override float[] ModelViewMatrix
		{
			set
			{
				DurationStatistics statistic = statistics[113];
				statistic.start();
				base.ModelViewMatrix = value;
				statistic.end();
			}
		}

		public override void setMorphWeight(int index, float value)
		{
			DurationStatistics statistic = statistics[114];
			statistic.start();
			base.setMorphWeight(index, value);
			statistic.end();
		}

		public override void setNormalPointer(int type, int stride, int bufferSize, Buffer buffer)
		{
			DurationStatistics statistic = statistics[115];
			statistic.start();
			base.setNormalPointer(type, stride, bufferSize, buffer);
			statistic.end();
		}

		public override void setNormalPointer(int type, int stride, long offset)
		{
			DurationStatistics statistic = statistics[116];
			statistic.start();
			base.setNormalPointer(type, stride, offset);
			statistic.end();
		}

		public override void setPatchDiv(int s, int t)
		{
			DurationStatistics statistic = statistics[117];
			statistic.start();
			base.setPatchDiv(s, t);
			statistic.end();
		}

		public override int PatchPrim
		{
			set
			{
				DurationStatistics statistic = statistics[118];
				statistic.start();
				base.PatchPrim = value;
				statistic.end();
			}
		}

		public override void setPixelStore(int rowLength, int alignment)
		{
			DurationStatistics statistic = statistics[119];
			statistic.start();
			base.setPixelStore(rowLength, alignment);
			statistic.end();
		}

		public override void setProgramParameter(int program, int parameter, int value)
		{
			DurationStatistics statistic = statistics[120];
			statistic.start();
			base.setProgramParameter(program, parameter, value);
			statistic.end();
		}

		public override float[] ProjectionMatrix
		{
			set
			{
				DurationStatistics statistic = statistics[121];
				statistic.start();
				base.ProjectionMatrix = value;
				statistic.end();
			}
		}

		public override void setRenderbufferStorage(int internalFormat, int width, int height)
		{
			DurationStatistics statistic = statistics[122];
			statistic.start();
			base.setRenderbufferStorage(internalFormat, width, height);
			statistic.end();
		}

		public override IRenderingEngine RenderingEngine
		{
			set
			{
				DurationStatistics statistic = statistics[123];
				statistic.start();
				base.RenderingEngine = value;
				statistic.end();
			}
		}

		public override void setScissor(int x, int y, int width, int height)
		{
			DurationStatistics statistic = statistics[124];
			statistic.start();
			base.setScissor(x, y, width, height);
			statistic.end();
		}

		public override int ShadeModel
		{
			set
			{
				DurationStatistics statistic = statistics[125];
				statistic.start();
				base.ShadeModel = value;
				statistic.end();
			}
		}

		public override void setStencilFunc(int func, int @ref, int mask)
		{
			DurationStatistics statistic = statistics[126];
			statistic.start();
			base.setStencilFunc(func, @ref, mask);
			statistic.end();
		}

		public override void setStencilOp(int fail, int zfail, int zpass)
		{
			DurationStatistics statistic = statistics[127];
			statistic.start();
			base.setStencilOp(fail, zfail, zpass);
			statistic.end();
		}

		public override void setTexCoordPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			DurationStatistics statistic = statistics[128];
			statistic.start();
			base.setTexCoordPointer(size, type, stride, bufferSize, buffer);
			statistic.end();
		}

		public override void setTexCoordPointer(int size, int type, int stride, long offset)
		{
			DurationStatistics statistic = statistics[129];
			statistic.start();
			base.setTexCoordPointer(size, type, stride, offset);
			statistic.end();
		}

		public override void setTexEnv(int name, float param)
		{
			DurationStatistics statistic = statistics[130];
			statistic.start();
			base.setTexEnv(name, param);
			statistic.end();
		}

		public override void setTexEnv(int name, int param)
		{
			DurationStatistics statistic = statistics[131];
			statistic.start();
			base.setTexEnv(name, param);
			statistic.end();
		}

		public override void setTexImage(int level, int internalFormat, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
			DurationStatistics statistic = statistics[132];
			statistic.start();
			base.setTexImage(level, internalFormat, width, height, format, type, textureSize, buffer);
			statistic.end();
		}

		public override void setTexSubImage(int level, int offset, int offset2, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
			DurationStatistics statistic = statistics[133];
			statistic.start();
			base.setTexSubImage(level, offset, offset2, width, height, format, type, textureSize, buffer);
			statistic.end();
		}

		public override float[] TextureEnvColor
		{
			set
			{
				DurationStatistics statistic = statistics[134];
				statistic.start();
				base.TextureEnvColor = value;
				statistic.end();
			}
		}

		public override void setTextureEnvironmentMapping(int u, int v)
		{
			DurationStatistics statistic = statistics[135];
			statistic.start();
			base.setTextureEnvironmentMapping(u, v);
			statistic.end();
		}

		public override void setTextureFunc(int func, bool alphaUsed, bool colorDoubled)
		{
			DurationStatistics statistic = statistics[136];
			statistic.start();
			base.setTextureFunc(func, alphaUsed, colorDoubled);
			statistic.end();
		}

		public override void setTextureMapMode(int mode, int proj)
		{
			DurationStatistics statistic = statistics[137];
			statistic.start();
			base.setTextureMapMode(mode, proj);
			statistic.end();
		}

		public override float[] TextureMatrix
		{
			set
			{
				DurationStatistics statistic = statistics[138];
				statistic.start();
				base.TextureMatrix = value;
				statistic.end();
			}
		}

		public override int TextureMipmapMagFilter
		{
			set
			{
				DurationStatistics statistic = statistics[139];
				statistic.start();
				base.TextureMipmapMagFilter = value;
				statistic.end();
			}
		}

		public override int TextureMipmapMaxLevel
		{
			set
			{
				DurationStatistics statistic = statistics[140];
				statistic.start();
				base.TextureMipmapMaxLevel = value;
				statistic.end();
			}
		}

		public override int TextureMipmapMinFilter
		{
			set
			{
				DurationStatistics statistic = statistics[141];
				statistic.start();
				base.TextureMipmapMinFilter = value;
				statistic.end();
			}
		}

		public override int TextureMipmapMinLevel
		{
			set
			{
				DurationStatistics statistic = statistics[142];
				statistic.start();
				base.TextureMipmapMinLevel = value;
				statistic.end();
			}
		}

		public override void setTextureWrapMode(int s, int t)
		{
			DurationStatistics statistic = statistics[143];
			statistic.start();
			base.setTextureWrapMode(s, t);
			statistic.end();
		}

		public override void setUniform(int id, float value)
		{
			DurationStatistics statistic = statistics[144];
			statistic.start();
			base.setUniform(id, value);
			statistic.end();
		}

		public override void setUniform(int id, int value1, int value2)
		{
			DurationStatistics statistic = statistics[145];
			statistic.start();
			base.setUniform(id, value1, value2);
			statistic.end();
		}

		public override void setUniform(int id, int value)
		{
			DurationStatistics statistic = statistics[146];
			statistic.start();
			base.setUniform(id, value);
			statistic.end();
		}

		public override void setUniform2(int id, int[] values)
		{
			DurationStatistics statistic = statistics[147];
			statistic.start();
			base.setUniform2(id, values);
			statistic.end();
		}

		public override void setUniform3(int id, int[] values)
		{
			DurationStatistics statistic = statistics[148];
			statistic.start();
			base.setUniform3(id, values);
			statistic.end();
		}

		public override void setUniform4(int id, int[] values)
		{
			DurationStatistics statistic = statistics[149];
			statistic.start();
			base.setUniform4(id, values);
			statistic.end();
		}

		public override void setUniformBlockBinding(int program, int blockIndex, int bindingPoint)
		{
			DurationStatistics statistic = statistics[150];
			statistic.start();
			base.setUniformBlockBinding(program, blockIndex, bindingPoint);
			statistic.end();
		}

		public override void setUniformMatrix4(int id, int count, float[] values)
		{
			DurationStatistics statistic = statistics[151];
			statistic.start();
			base.setUniformMatrix4(id, count, values);
			statistic.end();
		}

		public override void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, int bufferSize, Buffer buffer)
		{
			DurationStatistics statistic = statistics[152];
			statistic.start();
			base.setVertexAttribPointer(id, size, type, normalized, stride, bufferSize, buffer);
			statistic.end();
		}

		public override void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, long offset)
		{
			DurationStatistics statistic = statistics[153];
			statistic.start();
			base.setVertexAttribPointer(id, size, type, normalized, stride, offset);
			statistic.end();
		}

		public override float[] VertexColor
		{
			set
			{
				DurationStatistics statistic = statistics[154];
				statistic.start();
				base.VertexColor = value;
				statistic.end();
			}
		}

		public override void setVertexInfo(VertexInfo vinfo, bool allNativeVertexInfo, bool useVertexColor, bool useTexture, int type)
		{
			DurationStatistics statistic = statistics[155];
			statistic.start();
			base.setVertexInfo(vinfo, allNativeVertexInfo, useVertexColor, useTexture, type);
			statistic.end();
		}

		public override void setVertexPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			DurationStatistics statistic = statistics[156];
			statistic.start();
			base.setVertexPointer(size, type, stride, bufferSize, buffer);
			statistic.end();
		}

		public override void setVertexPointer(int size, int type, int stride, long offset)
		{
			DurationStatistics statistic = statistics[157];
			statistic.start();
			base.setVertexPointer(size, type, stride, offset);
			statistic.end();
		}

		public override float[] ViewMatrix
		{
			set
			{
				DurationStatistics statistic = statistics[158];
				statistic.start();
				base.ViewMatrix = value;
				statistic.end();
			}
		}

		public override void setViewport(int x, int y, int width, int height)
		{
			DurationStatistics statistic = statistics[159];
			statistic.start();
			base.setViewport(x, y, width, height);
			statistic.end();
		}

		public override void setWeightPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			DurationStatistics statistic = statistics[160];
			statistic.start();
			base.setWeightPointer(size, type, stride, bufferSize, buffer);
			statistic.end();
		}

		public override void setWeightPointer(int size, int type, int stride, long offset)
		{
			DurationStatistics statistic = statistics[161];
			statistic.start();
			base.setWeightPointer(size, type, stride, offset);
			statistic.end();
		}

		public override void startClearMode(bool color, bool stencil, bool depth)
		{
			DurationStatistics statistic = statistics[162];
			statistic.start();
			base.startClearMode(color, stencil, depth);
			statistic.end();
		}

		public override void startDirectRendering(bool textureEnabled, bool depthWriteEnabled, bool colorWriteEnabled, bool setOrthoMatrix, bool orthoInverted, int width, int height)
		{
			DurationStatistics statistic = statistics[163];
			statistic.start();
			base.startDirectRendering(textureEnabled, depthWriteEnabled, colorWriteEnabled, setOrthoMatrix, orthoInverted, width, height);
			statistic.end();
		}

		public override void startDisplay()
		{
			DurationStatistics statistic = statistics[164];
			statistic.start();
			base.startDisplay();
			statistic.end();
		}

		public override void useProgram(int program)
		{
			DurationStatistics statistic = statistics[165];
			statistic.start();
			base.useProgram(program);
			statistic.end();
		}

		public override bool validateProgram(int program)
		{
			DurationStatistics statistic = statistics[166];
			statistic.start();
			bool value = base.validateProgram(program);
			statistic.end();
			return value;
		}

		public override void bindVertexArray(int id)
		{
			DurationStatistics statistic = statistics[167];
			statistic.start();
			base.bindVertexArray(id);
			statistic.end();
		}

		public override void deleteVertexArray(int id)
		{
			DurationStatistics statistic = statistics[168];
			statistic.start();
			base.deleteVertexArray(id);
			statistic.end();
		}

		public override int genVertexArray()
		{
			DurationStatistics statistic = statistics[169];
			statistic.start();
			int value = base.genVertexArray();
			statistic.end();
			return value;
		}

		public override bool VertexArrayAvailable
		{
			get
			{
				DurationStatistics statistic = statistics[170];
				statistic.start();
				bool value = base.VertexArrayAvailable;
				statistic.end();
				return value;
			}
		}

		public override void multiDrawArrays(int primitive, IntBuffer first, IntBuffer count)
		{
			DurationStatistics statistic = statistics[171];
			statistic.start();
			base.multiDrawArrays(primitive, first, count);
			statistic.end();
		}

		public override void multMatrix(float[] values)
		{
			DurationStatistics statistic = statistics[172];
			statistic.start();
			base.multMatrix(values);
			statistic.end();
		}

		public override float[] Matrix
		{
			set
			{
				DurationStatistics statistic = statistics[173];
				statistic.start();
				base.Matrix = value;
				statistic.end();
			}
		}

		public override int MatrixMode
		{
			set
			{
				DurationStatistics statistic = statistics[174];
				statistic.start();
				base.MatrixMode = value;
				statistic.end();
			}
		}

		public override void setPixelTransfer(int parameter, float value)
		{
			DurationStatistics statistic = statistics[175];
			statistic.start();
			base.setPixelTransfer(parameter, value);
			statistic.end();
		}

		public override void setPixelTransfer(int parameter, int value)
		{
			DurationStatistics statistic = statistics[176];
			statistic.start();
			base.setPixelTransfer(parameter, value);
			statistic.end();
		}

		public override void setPixelTransfer(int parameter, bool value)
		{
			DurationStatistics statistic = statistics[177];
			statistic.start();
			base.setPixelTransfer(parameter, value);
			statistic.end();
		}

		public override void setPixelMap(int map, int mapSize, Buffer buffer)
		{
			DurationStatistics statistic = statistics[178];
			statistic.start();
			base.setPixelMap(map, mapSize, buffer);
			statistic.end();
		}

		public override bool canNativeClut(int textureAddress, int pixelFormat, bool textureSwizzle)
		{
			DurationStatistics statistic = statistics[179];
			statistic.start();
			bool value = base.canNativeClut(textureAddress, pixelFormat, textureSwizzle);
			statistic.end();
			return value;
		}

		public override int ActiveTexture
		{
			set
			{
				DurationStatistics statistic = statistics[180];
				statistic.start();
				base.ActiveTexture = value;
				statistic.end();
			}
		}

		public override void setTextureFormat(int pixelFormat, bool swizzle)
		{
			DurationStatistics statistic = statistics[181];
			statistic.start();
			base.setTextureFormat(pixelFormat, swizzle);
			statistic.end();
		}

		public override int getUniformIndex(int program, string name)
		{
			DurationStatistics statistic = statistics[182];
			statistic.start();
			int value = base.getUniformIndex(program, name);
			statistic.end();
			return value;
		}

		public override int[] getUniformIndices(int program, string[] names)
		{
			DurationStatistics statistic = statistics[183];
			statistic.start();
			int[] value = base.getUniformIndices(program, names);
			statistic.end();
			return value;
		}

		public override int getActiveUniformOffset(int program, int uniformIndex)
		{
			DurationStatistics statistic = statistics[184];
			statistic.start();
			int value = base.getActiveUniformOffset(program, uniformIndex);
			statistic.end();
			return value;
		}

		public override void bindAttribLocation(int program, int index, string name)
		{
			DurationStatistics statistic = statistics[185];
			statistic.start();
			base.bindAttribLocation(program, index, name);
			statistic.end();
		}

		public override void setUniform4(int id, float[] values)
		{
			DurationStatistics statistic = statistics[186];
			statistic.start();
			base.setUniform4(id, values);
			statistic.end();
		}

		public override void drawArraysBurstMode(int primitive, int first, int count)
		{
			DurationStatistics statistic = statistics[187];
			statistic.start();
			base.drawArraysBurstMode(primitive, first, count);
			statistic.end();
		}

		public override void bindActiveTexture(int index, int texture)
		{
			DurationStatistics statistic = statistics[188];
			statistic.start();
			base.bindActiveTexture(index, texture);
			statistic.end();
		}

		public override float TextureAnisotropy
		{
			set
			{
				DurationStatistics statistic = statistics[189];
				statistic.start();
				base.TextureAnisotropy = value;
				statistic.end();
			}
		}

		public override float MaxTextureAnisotropy
		{
			get
			{
				DurationStatistics statistic = statistics[190];
				statistic.start();
				float value = base.MaxTextureAnisotropy;
				statistic.end();
				return value;
			}
		}

		public override string ShadingLanguageVersion
		{
			get
			{
				DurationStatistics statistic = statistics[191];
				statistic.start();
				string value = base.ShadingLanguageVersion;
				statistic.end();
				return value;
			}
		}

		public override void setBlendSFix(int sfix, float[] color)
		{
			DurationStatistics statistic = statistics[192];
			statistic.start();
			base.setBlendSFix(sfix, color);
			statistic.end();
		}

		public override void setBlendDFix(int dfix, float[] color)
		{
			DurationStatistics statistic = statistics[193];
			statistic.start();
			base.setBlendDFix(dfix, color);
			statistic.end();
		}

		public override void setUniform3(int id, float[] values)
		{
			DurationStatistics statistic = statistics[194];
			statistic.start();
			base.setUniform3(id, values);
			statistic.end();
		}

		public override void waitForRenderingCompletion()
		{
			DurationStatistics statistic = statistics[195];
			statistic.start();
			base.waitForRenderingCompletion();
			statistic.end();
		}

		public override bool canReadAllVertexInfo()
		{
			DurationStatistics statistic = statistics[196];
			statistic.start();
			bool value = base.canReadAllVertexInfo();
			statistic.end();
			return value;
		}

		public override void readStencil(int x, int y, int width, int height, int bufferSize, Buffer buffer)
		{
			DurationStatistics statistic = statistics[197];
			statistic.start();
			base.readStencil(x, y, width, height, bufferSize, buffer);
			statistic.end();
		}

		public override void blitFramebuffer(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, int mask, int filter)
		{
			DurationStatistics statistic = statistics[198];
			statistic.start();
			base.blitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter);
			statistic.end();
		}

		public override bool checkAndLogErrors(string logComment)
		{
			DurationStatistics statistic = statistics[199];
			statistic.start();
			bool value = base.checkAndLogErrors(logComment);
			statistic.end();
			return value;
		}

		public override bool setCopyRedToAlpha(bool copyRedToAlpha)
		{
			DurationStatistics statistic = statistics[200];
			statistic.start();
			bool value = base.setCopyRedToAlpha(copyRedToAlpha);
			statistic.end();
			return value;
		}

		public override void drawElements(int primitive, int count, int indexType, Buffer indices, int indicesOffset)
		{
			DurationStatistics statistic = statistics[201];
			statistic.start();
			base.drawElements(primitive, count, indexType, indices, indicesOffset);
			statistic.end();
		}

		public override void drawElements(int primitive, int count, int indexType, long indicesOffset)
		{
			DurationStatistics statistic = statistics[202];
			statistic.start();
			base.drawElements(primitive, count, indexType, indicesOffset);
			statistic.end();
		}

		public override void multiDrawElements(int primitive, IntBuffer first, IntBuffer count, int indexType, long indicesOffset)
		{
			DurationStatistics statistic = statistics[203];
			statistic.start();
			base.multiDrawElements(primitive, first, count, indexType, indicesOffset);
			statistic.end();
		}

		public override void drawElementsBurstMode(int primitive, int count, int indexType, long indicesOffset)
		{
			DurationStatistics statistic = statistics[204];
			statistic.start();
			base.drawElementsBurstMode(primitive, count, indexType, indicesOffset);
			statistic.end();
		}
	}

}