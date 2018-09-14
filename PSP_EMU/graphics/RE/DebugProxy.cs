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

	using PixelColor = pspsharp.graphics.RE.software.PixelColor;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class DebugProxy : BaseRenderingEngineProxy
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal int useProgram_Renamed;
		protected internal bool isLogDebugEnabled;

		public DebugProxy(IRenderingEngine proxy) : base(proxy)
		{
			isLogDebugEnabled = log.DebugEnabled;
		}

		protected internal virtual string getEnabledDisabled(bool enabled)
		{
			return enabled ? "enabled" : "disabled";
		}

		public override void enableFlag(int flag)
		{
			if (isLogDebugEnabled)
			{
				if (flag < context.flags.Count)
				{
					log.debug(string.Format("enableFlag {0}", context.flags[flag].ToString()));
				}
				else
				{
					log.debug(string.Format("enableFlag {0:D}", flag));
				}
			}
			base.enableFlag(flag);
		}

		public override void disableFlag(int flag)
		{
			if (isLogDebugEnabled)
			{
				if (flag < context.flags.Count)
				{
					log.debug(string.Format("disableFlag {0}", context.flags[flag].ToString()));
				}
				else
				{
					log.debug(string.Format("disableFlag {0:D}", flag));
				}
			}
			base.disableFlag(flag);
		}

		public override void setAlphaFunc(int func, int @ref, int mask)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setAlphaFunc func={0:D}, ref=0x{1:X2}, mask=0x{2:X2}", func, @ref, mask));
			}
			base.setAlphaFunc(func, @ref, mask);
		}

		public override void setTextureFunc(int func, bool alphaUsed, bool colorDoubled)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setTextureFunc func={0:D}{1}{2}", func, alphaUsed ? " ALPHA" : "", colorDoubled ? " COLORx2" : ""));
			}
			base.setTextureFunc(func, alphaUsed, colorDoubled);
		}

		public override void setBlendFunc(int src, int dst)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setBlendFunc src={0:D}, dst={1:D}", src, dst));
			}
			base.setBlendFunc(src, dst);
		}

		public override float[] BlendColor
		{
			set
			{
				if (isLogDebugEnabled)
				{
					log.debug(string.Format("setBlendColor color=0x{0:X8}", PixelColor.getColor(value)));
				}
				base.BlendColor = value;
			}
		}

		public override int BlendEquation
		{
			set
			{
				if (isLogDebugEnabled)
				{
					log.debug(string.Format("setBlendEquation mode={0:D}", value));
				}
				base.BlendEquation = value;
			}
		}

		protected internal virtual void debugMatrix(string name, float[] matrix, int offset)
		{
			if (matrix == null)
			{
				matrix = identityMatrix;
			}
			for (int y = 0; y < 4; y++)
			{
				log.debug(string.Format("{0} {1:F3} {2:F3} {3:F3} {4:F3}", name, matrix[offset + 0 + y * 4], matrix[offset + 1 + y * 4], matrix[offset + 2 + y * 4], matrix[offset + 3 + y * 4]));
			}
		}

		protected internal virtual void debugMatrix(string name, float[] matrix)
		{
			debugMatrix(name, matrix, 0);
		}

		public override float[] ModelMatrix
		{
			set
			{
				if (isLogDebugEnabled)
				{
					debugMatrix("setModelMatrix", value);
				}
				base.ModelMatrix = value;
			}
		}

		public override float[] ModelViewMatrix
		{
			set
			{
				if (isLogDebugEnabled)
				{
					debugMatrix("setModelViewMatrix", value);
				}
				base.ModelViewMatrix = value;
			}
		}

		public override float[] ProjectionMatrix
		{
			set
			{
				if (isLogDebugEnabled)
				{
					debugMatrix("setProjectionMatrix", value);
				}
				base.ProjectionMatrix = value;
			}
		}

		public override float[] TextureMatrix
		{
			set
			{
				if (isLogDebugEnabled)
				{
					debugMatrix("setTextureMatrix", value);
				}
				base.TextureMatrix = value;
			}
		}

		public override float[] ViewMatrix
		{
			set
			{
				if (isLogDebugEnabled)
				{
					debugMatrix("setViewMatrix", value);
				}
				base.ViewMatrix = value;
			}
		}

		public override int TextureMipmapMinLevel
		{
			set
			{
				if (isLogDebugEnabled)
				{
					log.debug(string.Format("setTextureMipmapMinLevel {0:D}", value));
				}
				base.TextureMipmapMinLevel = value;
			}
		}

		public override int TextureMipmapMaxLevel
		{
			set
			{
				if (isLogDebugEnabled)
				{
					log.debug(string.Format("setTextureMipmapMaxLevel {0:D}", value));
				}
				base.TextureMipmapMaxLevel = value;
			}
		}

		public override int TextureMipmapMinFilter
		{
			set
			{
				if (isLogDebugEnabled)
				{
					log.debug(string.Format("setTextureMipmapMinFilter {0:D}", value));
				}
				base.TextureMipmapMinFilter = value;
			}
		}

		public override int TextureMipmapMagFilter
		{
			set
			{
				if (isLogDebugEnabled)
				{
					log.debug(string.Format("setTextureMipmapMagFilter {0:D}", value));
				}
				base.TextureMipmapMagFilter = value;
			}
		}

		public override void setPixelStore(int rowLength, int alignment)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setPixelStore rowLength={0:D}, alignment={1:D}", rowLength, alignment));
			}
			base.setPixelStore(rowLength, alignment);
		}

		public override void setCompressedTexImage(int level, int internalFormat, int width, int height, int compressedSize, Buffer buffer)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setCompressedTexImage level={0:D}, internalFormat={1:D}, {2:D}x{3:D}, compressedSize=0x{4:X}", level, internalFormat, width, height, compressedSize));
			}
			base.setCompressedTexImage(level, internalFormat, width, height, compressedSize, buffer);
		}

		public override void setTexImage(int level, int internalFormat, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setTexImage level={0:D}, internalFormat={1:D}, {2:D}x{3:D}, format={4:D}, type={5:D}, textureSize={6:D}", level, internalFormat, width, height, format, type, textureSize));
			}
			base.setTexImage(level, internalFormat, width, height, format, type, textureSize, buffer);
		}

		public override void startClearMode(bool color, bool stencil, bool depth)
		{
			isLogDebugEnabled = log.DebugEnabled;
			if (isLogDebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("startClearMode color=%b, stencil=%b, depth=%b", color, stencil, depth));
				log.debug(string.Format("startClearMode color=%b, stencil=%b, depth=%b", color, stencil, depth));
			}
			base.startClearMode(color, stencil, depth);
		}

		public override void endClearMode()
		{
			if (isLogDebugEnabled)
			{
				log.debug("endClearMode");
			}
			base.endClearMode();
		}

		public override void startDirectRendering(bool textureEnabled, bool depthWriteEnabled, bool colorWriteEnabled, bool setOrthoMatrix, bool orthoInverted, int width, int height)
		{
			if (isLogDebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("startDirectRendering texture=%b, depth=%b, color=%b, ortho=%b, inverted=%b, %dx%d", textureEnabled, depthWriteEnabled, colorWriteEnabled, setOrthoMatrix, orthoInverted, width, height));
				log.debug(string.Format("startDirectRendering texture=%b, depth=%b, color=%b, ortho=%b, inverted=%b, %dx%d", textureEnabled, depthWriteEnabled, colorWriteEnabled, setOrthoMatrix, orthoInverted, width, height));
			}
			base.startDirectRendering(textureEnabled, depthWriteEnabled, colorWriteEnabled, setOrthoMatrix, orthoInverted, width, height);
		}

		public override void endDirectRendering()
		{
			if (isLogDebugEnabled)
			{
				log.debug("endDirectRendering");
			}
			base.endDirectRendering();
		}

		public override void bindTexture(int texture)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("bindTexture {0:D}", texture));
			}
			base.bindTexture(texture);
		}

		public override void drawArrays(int type, int first, int count)
		{
			isLogDebugEnabled = log.DebugEnabled;
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("drawArrays type={0:D}, first={1:D}, count={2:D}", type, first, count));
			}
			base.drawArrays(type, first, count);
		}

		public override void drawArraysBurstMode(int type, int first, int count)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("drawArraysBurstMode type={0:D}, first={1:D}, count={2:D}", type, first, count));
			}
			base.drawArraysBurstMode(type, first, count);
		}

		public override int DepthFunc
		{
			set
			{
				if (isLogDebugEnabled)
				{
					log.debug(string.Format("setDepthFunc {0:D}", value));
				}
				base.DepthFunc = value;
			}
		}

		public override bool DepthMask
		{
			set
			{
				if (isLogDebugEnabled)
				{
					log.debug(string.Format("setDepthMask {0}", getEnabledDisabled(value)));
				}
				base.DepthMask = value;
			}
		}

		public override void setDepthRange(float zpos, float zscale, int near, int far)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setDepthRange zpos={0:F}, zscale={1:F}, near=0x{2:X4}, far=0x{3:X4}", zpos, zscale, near, far));
			}
			base.setDepthRange(zpos, zscale, near, far);
		}

		public override void setStencilFunc(int func, int @ref, int mask)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setStencilFunc func={0:D}, ref=0x{1:X2}, mask=0x{2:X2}", func, @ref, mask));
			}
			base.setStencilFunc(func, @ref, mask);
		}

		public override void setStencilOp(int fail, int zfail, int zpass)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setStencilOp fail={0:D}, zfail={1:D}, zpass={2:D}", fail, zfail, zpass));
			}
			base.setStencilOp(fail, zfail, zpass);
		}

		public override int setBones(int count, float[] values)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setBones count={0:D}", count));
				for (int i = 0; i < count; i++)
				{
					debugMatrix("setBones[" + i + "]", values, i * 16);
				}
			}
			return base.setBones(count, values);
		}

		public override void setUniformMatrix4(int id, int count, float[] values)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setUniformMatrix4 id={0:D}, count={1:D}", id, count));
				for (int i = 0; i < count; i++)
				{
					debugMatrix("setUniformMatrix4[" + i + "]", values, i * 16);
				}
			}
			base.setUniformMatrix4(id, count, values);
		}

		protected internal virtual string getUniformName(int id)
		{
			if (id < 0)
			{
				return "Unused Uniform";
			}

			foreach (Uniforms uniform in Uniforms.values())
			{
				if (uniform.getId(useProgram_Renamed) == id)
				{
					return uniform.name();
				}
			}

			return "Uniform " + id;
		}

		public override void enableVertexAttribArray(int id)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("enableVertexAttribArray {0:D}", id));
			}
			base.enableVertexAttribArray(id);
		}

		public override void disableVertexAttribArray(int id)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("disableVertexAttribArray {0:D}", id));
			}
			base.disableVertexAttribArray(id);
		}

		public override void setUniform(int id, float value)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setUniform {0}={1:F}", getUniformName(id), value));
			}
			base.setUniform(id, value);
		}

		public override void setUniform(int id, int value1, int value2)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setUniform {0}={1:D}, {2:D}", getUniformName(id), value1, value2));
			}
			base.setUniform(id, value1, value2);
		}

		public override void setUniform2(int id, int[] values)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setUniform2 {0}={1:D}, {2:D}", getUniformName(id), values[0], values[1]));
			}
			base.setUniform2(id, values);
		}

		public override void setUniform3(int id, int[] values)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setUniform3 {0}={1:D}, {2:D}, {3:D}", getUniformName(id), values[0], values[1], values[2]));
			}
			base.setUniform3(id, values);
		}

		public override void setUniform4(int id, int[] values)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setUniform4 {0}={1:D}, {2:D}, {3:D}, {4:D}", getUniformName(id), values[0], values[1], values[2], values[3]));
			}
			base.setUniform4(id, values);
		}

		public override void setUniform(int id, int value)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setUniform {0}={1:D}", getUniformName(id), value));
			}
			base.setUniform(id, value);
		}

		public override void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, int bufferSize, Buffer buffer)
		{
			if (isLogDebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("setVertexAttribPointer id=%d, size=%d, type=%d, normalized=%b, stride=%d, bufferSize=%d", id, size, type, normalized, stride, bufferSize));
				log.debug(string.Format("setVertexAttribPointer id=%d, size=%d, type=%d, normalized=%b, stride=%d, bufferSize=%d", id, size, type, normalized, stride, bufferSize));
			}
			base.setVertexAttribPointer(id, size, type, normalized, stride, bufferSize, buffer);
		}

		public override void setVertexAttribPointer(int id, int size, int type, bool normalized, int stride, long offset)
		{
			if (isLogDebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("setVertexAttribPointer id=%d, size=%d, type=%d, normalized=%b, stride=%d, offset=%d", id, size, type, normalized, stride, offset));
				log.debug(string.Format("setVertexAttribPointer id=%d, size=%d, type=%d, normalized=%b, stride=%d, offset=%d", id, size, type, normalized, stride, offset));
			}
			base.setVertexAttribPointer(id, size, type, normalized, stride, offset);
		}

		public override void enableClientState(int type)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("enableClientState {0:D}", type));
			}
			base.enableClientState(type);
		}

		public override void disableClientState(int type)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("disableClientState {0:D}", type));
			}
			base.disableClientState(type);
		}

		public override void setColorMask(bool redWriteEnabled, bool greenWriteEnabled, bool blueWriteEnabled, bool alphaWriteEnabled)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setColorMask red {0}, green {1}, blue {2}, alpha {3}", getEnabledDisabled(redWriteEnabled), getEnabledDisabled(greenWriteEnabled), getEnabledDisabled(blueWriteEnabled), getEnabledDisabled(alphaWriteEnabled)));
			}
			base.setColorMask(redWriteEnabled, greenWriteEnabled, blueWriteEnabled, alphaWriteEnabled);
		}

		public override void setColorMask(int redMask, int greenMask, int blueMask, int alphaMask)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setColorMask red 0x{0:X2}, green 0x{1:X2}, blue 0x{2:X2}, alpha 0x{3:X2}", redMask, greenMask, blueMask, alphaMask));
			}
			base.setColorMask(redMask, greenMask, blueMask, alphaMask);
		}

		public override void setTextureWrapMode(int s, int t)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setTextureWrapMode {0:D}, {1:D}", s, t));
			}
			base.setTextureWrapMode(s, t);
		}

		public override void deleteTexture(int texture)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("deleteTexture {0:D}", texture));
			}
			base.deleteTexture(texture);
		}

		public override void endDisplay()
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("endDisplay"));
			}
			base.endDisplay();
		}

		public override int genTexture()
		{
			int value = base.genTexture();
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("genTexture {0:D}", value));
			}
			return value;
		}

		public override void startDisplay()
		{
			isLogDebugEnabled = log.DebugEnabled;
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("startDisplay"));
			}
			base.startDisplay();
		}

		public override void setTextureMapMode(int mode, int proj)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setTextureMapMode mode={0:D}, proj={1:D}", mode, proj));
			}
			base.setTextureMapMode(mode, proj);
		}

		public override bool FrontFace
		{
			set
			{
				if (isLogDebugEnabled)
				{
					log.debug(string.Format("setFrontFace {0}", value ? "clockwise" : "counter-clockwise"));
				}
				base.FrontFace = value;
			}
		}

		public override void setColorPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setColorPointer size={0:D}, type={1:D}, stride={2:D}, bufferSize={3:D}, buffer offset={4:D}", size, type, stride, bufferSize, buffer.position()));
			}
			base.setColorPointer(size, type, stride, bufferSize, buffer);
		}

		public override void setColorPointer(int size, int type, int stride, long offset)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setColorPointer size={0:D}, type={1:D}, stride={2:D}, offset={3:D}", size, type, stride, offset));
			}
			base.setColorPointer(size, type, stride, offset);
		}

		public override void setNormalPointer(int type, int stride, int bufferSize, Buffer buffer)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setNormalPointer type={0:D}, stride={1:D}, bufferSize={2:D}, buffer offset={3:D}", type, stride, bufferSize, buffer.position()));
			}
			base.setNormalPointer(type, stride, bufferSize, buffer);
		}

		public override void setNormalPointer(int type, int stride, long offset)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setNormalPointer type={0:D}, stride={1:D}, offset={2:D}", type, stride, offset));
			}
			base.setNormalPointer(type, stride, offset);
		}

		public override void setTexCoordPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setTexCoordPointer size={0:D}, type={1:D}, stride={2:D}, bufferSize={3:D}, buffer offset={4:D}", size, type, stride, bufferSize, buffer.position()));
			}
			base.setTexCoordPointer(size, type, stride, bufferSize, buffer);
		}

		public override void setTexCoordPointer(int size, int type, int stride, long offset)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setTexCoordPointer size={0:D}, type={1:D}, stride={2:D}, offset={3:D}", size, type, stride, offset));
			}
			base.setTexCoordPointer(size, type, stride, offset);
		}

		public override void setVertexPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setVertexPointer size={0:D}, type={1:D}, stride={2:D}, bufferSize={3:D}, buffer offset={4:D}", size, type, stride, bufferSize, buffer.position()));
			}
			base.setVertexPointer(size, type, stride, bufferSize, buffer);
		}

		public override void setVertexPointer(int size, int type, int stride, long offset)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setVertexPointer size={0:D}, type={1:D}, stride={2:D}, offset={3:D}", size, type, stride, offset));
			}
			base.setVertexPointer(size, type, stride, offset);
		}

		public override void setWeightPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setWeightPointer size={0:D}, type={1:D}, stride={2:D}, bufferSize={3:D}, buffer offset={4:D}", size, type, stride, bufferSize, buffer.position()));
			}
			base.setWeightPointer(size, type, stride, bufferSize, buffer);
		}

		public override void setWeightPointer(int size, int type, int stride, long offset)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setWeightPointer size={0:D}, type={1:D}, stride={2:D}, offset={3:D}", size, type, stride, offset));
			}
			base.setWeightPointer(size, type, stride, offset);
		}

		public override void setBufferData(int target, int size, Buffer buffer, int usage)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setBufferData target={0:D}, size={1:D}, buffer size={2:D}, usage={3:D}", target, size, buffer == null ? 0 : buffer.capacity(), usage));
			}
			base.setBufferData(target, size, buffer, usage);
		}

		public override void setBufferSubData(int target, int offset, int size, Buffer buffer)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setBufferSubData target={0:D}, offset={1:D}, size={2:D}, buffer size={3:D}", target, offset, size, buffer == null ? 0 : buffer.capacity()));
			}
			base.setBufferSubData(target, offset, size, buffer);
		}

		public override void bindBuffer(int target, int buffer)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("bindBuffer {0:D}, {1:D}", target, buffer));
			}
			base.bindBuffer(target, buffer);
		}

		public override void useProgram(int program)
		{
			useProgram_Renamed = program;
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("useProgram {0:D}", program));
			}
			base.useProgram(program);
		}

		public override void setViewport(int x, int y, int width, int height)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setViewport x={0:D}, y={1:D}, width={2:D}, height={3:D}", x, y, width, height));
			}
			base.setViewport(x, y, width, height);
		}

		public override void setScissor(int x, int y, int width, int height)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setScissor x={0:D}, y={1:D}, width={2:D}, height={3:D}", x, y, width, height));
			}
			base.setScissor(x, y, width, height);
		}

		public override void setTexSubImage(int level, int xOffset, int yOffset, int width, int height, int format, int type, int textureSize, Buffer buffer)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setTexSubImage level={0:D}, xOffset={1:D}, yOffset={2:D}, width={3:D}, height={4:D}, format={5:D}, type={6:D}, textureSize={7:D}", level, xOffset, yOffset, width, height, format, type, textureSize));
			}
			base.setTexSubImage(level, xOffset, yOffset, width, height, format, type, textureSize, buffer);
		}

		public override void copyTexSubImage(int level, int xOffset, int yOffset, int x, int y, int width, int height)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("copyTexSubImage level={0:D}, xOffset={1:D}, yOffset={2:D}, x={3:D}, y={4:D}, width={5:D}, height={6:D}", level, xOffset, yOffset, x, y, width, height));
			}
			base.copyTexSubImage(level, xOffset, yOffset, x, y, width, height);
		}

		public override void getTexImage(int level, int format, int type, Buffer buffer)
		{
			if (isLogDebugEnabled)
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				log.debug(string.Format("getTexImage level={0:D}, format={1:D}, type={2:D}, buffer remaining={3:D}, buffer class={4}", level, format, type, buffer.remaining(), buffer.GetType().FullName));
			}
			base.getTexImage(level, format, type, buffer);
		}

		public override void bindVertexArray(int id)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("bindVertexArray {0:D}", id));
			}
			base.bindVertexArray(id);
		}

		public override void multiDrawArrays(int primitive, IntBuffer first, IntBuffer count)
		{
			if (isLogDebugEnabled)
			{
				StringBuilder s = new StringBuilder();
				int n = first.remaining();
				int p = first.position();
				for (int i = 0; i < n; i++)
				{
					s.Append(string.Format(" ({0:D},{1:D})", first.get(p + i), count.get(p + i)));
				}
				log.debug(string.Format("multiDrawArrays primitive={0:D}, count={1:D},{2}", primitive, n, s.ToString()));
			}
			base.multiDrawArrays(primitive, first, count);
		}

		public override int ActiveTexture
		{
			set
			{
				if (isLogDebugEnabled)
				{
					log.debug(string.Format("setActiveTexture {0:D}", value));
				}
				base.ActiveTexture = value;
			}
		}

		public override void setTextureFormat(int pixelFormat, bool swizzle)
		{
			if (isLogDebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("setTextureFormat pixelFormat=%d(%s), swizzle=%b", pixelFormat, pspsharp.graphics.VideoEngine.getPsmName(pixelFormat), swizzle));
				log.debug(string.Format("setTextureFormat pixelFormat=%d(%s), swizzle=%b", pixelFormat, VideoEngine.getPsmName(pixelFormat), swizzle));
			}
			base.setTextureFormat(pixelFormat, swizzle);
		}

		public override int createProgram()
		{
			int program = base.createProgram();
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("createProgram {0:D}", program));
			}
			return program;
		}

		public override float[] VertexColor
		{
			set
			{
				if (isLogDebugEnabled)
				{
					log.debug(string.Format("setVertexColor (r={0:F3}, b={1:F3}, g={2:F3}, a={3:F3})", value[0], value[1], value[2], value[3]));
				}
				base.VertexColor = value;
			}
		}

		public override void setUniform4(int id, float[] values)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setUniform4 {0}={1:F}, {2:F}, {3:F}, {4:F}", getUniformName(id), values[0], values[1], values[2], values[3]));
			}
			base.setUniform4(id, values);
		}

		public override void setColorMaterial(bool ambient, bool diffuse, bool specular)
		{
			if (isLogDebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("setColorMaterial ambient=%b, diffuse=%b, specular=%b", ambient, diffuse, specular));
				log.debug(string.Format("setColorMaterial ambient=%b, diffuse=%b, specular=%b", ambient, diffuse, specular));
			}
			base.setColorMaterial(ambient, diffuse, specular);
		}

		public override void bindFramebuffer(int target, int framebuffer)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("bindFramebuffer target={0:D}, framebuffer={1:D}", target, framebuffer));
			}
			base.bindFramebuffer(target, framebuffer);
		}

		public override void bindActiveTexture(int index, int texture)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("bindActiveTexture index={0:D}, texture={1:D}", index, texture));
			}
			base.bindActiveTexture(index, texture);
		}

		public override void blitFramebuffer(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, int mask, int filter)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("blitFramebuffer src=({0:D},{1:D})-({2:D},{3:D}), dst=({4:D},{5:D})-({6:D},{7:D}), mask=0x{8:X}, filter={9:D}", srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter));
			}
			base.blitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter);
		}

		public override bool setCopyRedToAlpha(bool copyRedToAlpha)
		{
			if (isLogDebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("setCopyRedToAlpha %b", copyRedToAlpha));
				log.debug(string.Format("setCopyRedToAlpha %b", copyRedToAlpha));
			}
			return base.setCopyRedToAlpha(copyRedToAlpha);
		}

		public override void drawElements(int primitive, int count, int indexType, Buffer indices, int indicesOffset)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("drawElements primitive={0:D}, count={1:D}, indexType={2:D}, indicesOffset={3:D}", primitive, count, indexType, indicesOffset));
			}
			base.drawElements(primitive, count, indexType, indices, indicesOffset);
		}

		public override void drawElements(int primitive, int count, int indexType, long indicesOffset)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("drawElements primitive={0:D}, count={1:D}, indexType={2:D}, indicesOffset={3:D}", primitive, count, indexType, indicesOffset));
			}
			base.drawElements(primitive, count, indexType, indicesOffset);
		}

		public override void multiDrawElements(int primitive, IntBuffer first, IntBuffer count, int indexType, long indicesOffset)
		{
			if (isLogDebugEnabled)
			{
				StringBuilder s = new StringBuilder();
				int n = first.remaining();
				int p = first.position();
				for (int i = 0; i < n; i++)
				{
					s.Append(string.Format(" ({0:D},{1:D})", first.get(p + i), count.get(p + i)));
				}
				log.debug(string.Format("multiDrawElements primitive={0:D}, count={1:D},{2}, indexType={3:D}, indicesOffset={4:D}", primitive, n, s.ToString(), indexType, indicesOffset));
			}
			base.multiDrawElements(primitive, first, count, indexType, indicesOffset);
		}

		public override void drawElementsBurstMode(int primitive, int count, int indexType, long indicesOffset)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("drawElementsBurstMode primitive={0:D}, count={1:D}, indexType={2:D}, indicesOffset={3:D}", primitive, count, indexType, indicesOffset));
			}
			base.drawElementsBurstMode(primitive, count, indexType, indicesOffset);
		}

		public override int genFramebuffer()
		{
			int value = base.genFramebuffer();
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("genFramebuffer {0:D}", value));
			}
			return value;
		}

		public override int genRenderbuffer()
		{
			int value = base.genRenderbuffer();
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("genRenderbuffer {0:D}", value));
			}
			return value;
		}

		public override void bindRenderbuffer(int renderbuffer)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("bindRenderbuffer renderbuffer={0:D}", renderbuffer));
			}
			base.bindRenderbuffer(renderbuffer);
		}

		public override void setRenderbufferStorage(int internalFormat, int width, int height)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setRenderbufferStorage internalFormat={0:D}, width={1:D}, height={2:D}", internalFormat, width, height));
			}
			base.setRenderbufferStorage(internalFormat, width, height);
		}

		public override void setFramebufferRenderbuffer(int target, int attachment, int renderbuffer)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setFramebufferRenderbuffer target={0:D}, attachment={1:D}, renderbuffer={2:D}", target, attachment, renderbuffer));
			}
			base.setFramebufferRenderbuffer(target, attachment, renderbuffer);
		}

		public override void setFramebufferTexture(int target, int attachment, int texture, int level)
		{
			if (isLogDebugEnabled)
			{
				log.debug(string.Format("setFramebufferTexture target={0:D}, attachment={1:D}, texture={2:D}, level={3:D}", target, attachment, texture, level));
			}
			base.setFramebufferTexture(target, attachment, texture, level);
		}

		public override void textureBarrier()
		{
			if (isLogDebugEnabled)
			{
				log.debug("textureBarrier");
			}
			base.textureBarrier();
		}

		public override int LogicOp
		{
			set
			{
				if (isLogDebugEnabled)
				{
					log.debug(string.Format("setLogicOp logicOp={0:D}", value));
				}
				base.LogicOp = value;
			}
		}
	}

}