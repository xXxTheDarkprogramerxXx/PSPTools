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
namespace pspsharp.graphics.RE.software
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.CMODE_FORMAT_16BIT_ABGR4444;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.CMODE_FORMAT_16BIT_ABGR5551;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.CMODE_FORMAT_16BIT_BGR5650;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.CMODE_FORMAT_32BIT_ABGR8888;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR4444;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR5551;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_INDEXED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_INDEXED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_8BIT_INDEXED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.ImageReader.color4444to8888;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.ImageReader.color5551to8888;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.ImageReader.color565to8888;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.getPower2;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using Modules = pspsharp.HLE.Modules;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// A cached texture for the software Rendering Engine (RESoftware).
	/// The whole texture is read at initialization.
	/// </summary>
	public abstract class CachedTexture : IRandomTextureAccess
	{
		public abstract int readPixel(int u, int v);
		protected internal int width;
		protected internal int height;
		protected internal readonly int pixelFormat;
		protected internal readonly int widthPower2;
		protected internal readonly int heightPower2;
		protected internal readonly int offset;
		protected internal int[] buffer;
		protected internal bool useTextureClut;
		protected internal int[] clut;
		protected internal bool isVRAMTexture;

		public static CachedTexture getCachedTexture(int width, int height, int pixelFormat, IMemoryReader imageReader)
		{
			CachedTexture cachedTexture = getCachedTexture(width, height, pixelFormat, 0);
			cachedTexture.Buffer = imageReader;

			return cachedTexture;
		}

		public static CachedTexture getCachedTexture(int width, int height, int pixelFormat, int[] buffer, int bufferOffset, int bufferLength)
		{
			int offset = 0;
			// When the texture is directly available from the memory,
			// we can reuse the memory array and do not need to copy the whole texture.
			if (buffer == RuntimeContext.MemoryInt)
			{
				if (pixelFormat == TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888 || pspsharp.graphics.RE.IRenderingEngine_Fields.isTextureTypeIndexed[pixelFormat])
				{
					// Do not reuse the memory buffer when the texture is inside the current GE,
					// copy the texture (this is better matching the PSP texture cache behavior).
					int textureAddress = bufferOffset << 2;
					if (!Modules.sceDisplayModule.isGeAddress(textureAddress))
					{
						offset = bufferOffset;
					}
				}
			}

			CachedTexture cachedTexture = getCachedTexture(width, height, pixelFormat, offset);
			cachedTexture.setBuffer(buffer, bufferOffset, bufferLength);

			if (buffer == RuntimeContext.MemoryInt)
			{
				int textureAddress = bufferOffset << 2;
				if (Memory.isVRAM(textureAddress))
				{
					cachedTexture.VRAMTexture = true;
				}
			}

			return cachedTexture;
		}

		public static CachedTexture getCachedTexture(int width, int height, int pixelFormat, short[] buffer, int bufferOffset, int bufferLength)
		{
			CachedTexture cachedTexture = getCachedTexture(width, height, pixelFormat, 0);
			cachedTexture.setBuffer(buffer, bufferOffset, bufferLength);

			return cachedTexture;
		}

		private static CachedTexture getCachedTexture(int width, int height, int pixelFormat, int offset)
		{
			CachedTexture cachedTexture;

			if (pspsharp.graphics.RE.IRenderingEngine_Fields.isTextureTypeIndexed[pixelFormat])
			{
				switch (pixelFormat)
				{
					case TPSM_PIXEL_STORAGE_MODE_8BIT_INDEXED:
						cachedTexture = new CachedTextureIndexed8Bit(width, height, pixelFormat, offset);
						break;
					case TPSM_PIXEL_STORAGE_MODE_16BIT_INDEXED:
						cachedTexture = new CachedTextureIndexed16Bit(width, height, pixelFormat, offset);
						break;
					case TPSM_PIXEL_STORAGE_MODE_32BIT_INDEXED:
						cachedTexture = new CachedTextureIndexed32Bit(width, height, pixelFormat, offset);
						break;
					default:
						VideoEngine.log_Renamed.error(string.Format("CachedTexture: unsupported indexed texture format {0:D}", pixelFormat));
						return null;
				}
				cachedTexture.setClut();
			}
			else if (width == Utilities.makePow2(width))
			{
				if (offset == 0)
				{
					cachedTexture = new CachedTexturePow2(width, height, pixelFormat);
				}
				else
				{
					cachedTexture = new CachedTextureOffsetPow2(width, height, pixelFormat, offset);
				}
			}
			else
			{
				if (offset == 0)
				{
					cachedTexture = new CachedTextureNonPow2(width, height, pixelFormat);
				}
				else
				{
					cachedTexture = new CachedTextureOffsetNonPow2(width, height, pixelFormat, offset);
				}
			}

			return cachedTexture;
		}

		protected internal CachedTexture(int width, int height, int pixelFormat, int offset)
		{
			this.width = width;
			this.height = height;
			this.pixelFormat = pixelFormat;
			this.offset = offset;
			widthPower2 = getPower2(width);
			heightPower2 = getPower2(height);
		}

		protected internal virtual IMemoryReader Buffer
		{
			set
			{
				buffer = new int[width * height];
				for (int i = 0; i < buffer.Length; i++)
				{
					buffer[i] = value.readNext();
				}
			}
		}

		protected internal virtual void setBuffer(int[] buffer, int bufferOffset, int bufferLength)
		{
			switch (pixelFormat)
			{
				case TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650:
					this.buffer = new int[bufferLength * 2];
					for (int i = 0, j = 0; i < bufferLength; i++)
					{
						int color = buffer[bufferOffset + i];
						this.buffer[j++] = color565to8888(color & 0xFFFF);
						this.buffer[j++] = color565to8888((int)((uint)color >> 16));
					}
					break;
				case TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR5551:
					this.buffer = new int[bufferLength * 2];
					for (int i = 0, j = 0; i < bufferLength; i++)
					{
						int color = buffer[bufferOffset + i];
						this.buffer[j++] = color5551to8888(color & 0xFFFF);
						this.buffer[j++] = color5551to8888((int)((uint)color >> 16));
					}
					break;
				case TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR4444:
					this.buffer = new int[bufferLength * 2];
					for (int i = 0, j = 0; i < bufferLength; i++)
					{
						int color = buffer[bufferOffset + i];
						this.buffer[j++] = color4444to8888(color & 0xFFFF);
						this.buffer[j++] = color4444to8888((int)((uint)color >> 16));
					}
					break;
				case TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888:
				case TPSM_PIXEL_STORAGE_MODE_8BIT_INDEXED:
				case TPSM_PIXEL_STORAGE_MODE_16BIT_INDEXED:
				case TPSM_PIXEL_STORAGE_MODE_32BIT_INDEXED:
					// Is the texture directly available from the memory array?
					if (buffer == RuntimeContext.MemoryInt && offset == bufferOffset)
					{
						// We do not need to copy the whole texture, we can reuse the memory array
						this.buffer = buffer;
					}
					else
					{
						this.buffer = new int[bufferLength];
						Array.Copy(buffer, bufferOffset, this.buffer, 0, bufferLength);
					}
					break;
				default:
					VideoEngine.log_Renamed.error(string.Format("CachedTexture setBuffer int unsupported pixel format {0:D}", pixelFormat));
					break;
			}
		}

		protected internal virtual void setClut()
		{
		}

		protected internal virtual void setBuffer(short[] buffer, int bufferOffset, int bufferLength)
		{
			switch (pixelFormat)
			{
				case TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650:
					this.buffer = new int[bufferLength];
					for (int i = 0; i < bufferLength; i++)
					{
						this.buffer[i] = color565to8888(buffer[bufferOffset + i] & 0xFFFF);
					}
					break;
				case TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR5551:
					this.buffer = new int[bufferLength];
					for (int i = 0; i < bufferLength; i++)
					{
						this.buffer[i] = color5551to8888(buffer[bufferOffset + i] & 0xFFFF);
					}
					break;
				case TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR4444:
					this.buffer = new int[bufferLength];
					for (int i = 0; i < bufferLength; i++)
					{
						this.buffer[i] = color4444to8888(buffer[bufferOffset + i] & 0xFFFF);
					}
					break;
				case TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888:
					this.buffer = new int[bufferLength / 2];
					for (int i = 0, j = bufferOffset; i < this.buffer.Length; i++)
					{
						this.buffer[i] = (buffer[j++] & 0xFFFF) | (buffer[j++] << 16);
					}
					goto default;
				default:
					VideoEngine.log_Renamed.error(string.Format("CachedTexture setBuffer short unsupported pixel format {0:D}", pixelFormat));
					break;
			}
		}

		public virtual int Width
		{
			get
			{
				return width;
			}
		}

		public virtual int Height
		{
			get
			{
				return height;
			}
		}

		public virtual int PixelFormat
		{
			get
			{
				return pixelFormat;
			}
		}

		public virtual bool VRAMTexture
		{
			get
			{
				return isVRAMTexture;
			}
			set
			{
				this.isVRAMTexture = value;
			}
		}


		/// <summary>
		/// @author gid15
		/// 
		/// A specialized class when the width is a power of 2 (faster).
		/// </summary>
		protected internal class CachedTexturePow2 : CachedTexture
		{
			public CachedTexturePow2(int widthPow2, int heightPow2, int width, int height, int pixelFormat) : base(widthPow2, heightPow2, pixelFormat, 0)
			{
				this.width = width;
				this.height = height;
			}

			public CachedTexturePow2(int width, int height, int pixelFormat) : base(width, height, pixelFormat, 0)
			{
			}

			public override int readPixel(int u, int v)
			{
				return buffer[(v << widthPower2) + u];
			}
		}

		/// <summary>
		/// @author gid15
		/// 
		/// A specialized class when the width is a power of 2 (faster)
		/// and using an array offset.
		/// </summary>
		private class CachedTextureOffsetPow2 : CachedTexture
		{
			public CachedTextureOffsetPow2(int width, int height, int pixelFormat, int offset) : base(width, height, pixelFormat, offset)
			{
			}

			public override int readPixel(int u, int v)
			{
				return buffer[(v << widthPower2) + u + offset];
			}
		}

		/// <summary>
		/// @author gid15
		/// 
		/// A specialized class when the width is not a power of 2.
		/// </summary>
		private class CachedTextureNonPow2 : CachedTexture
		{
			public CachedTextureNonPow2(int width, int height, int pixelFormat) : base(width, height, pixelFormat, 0)
			{
			}

			public override int readPixel(int u, int v)
			{
				return buffer[v * width + u];
			}
		}

		/// <summary>
		/// @author gid15
		/// 
		/// A specialized class when the width is not a power of 2
		/// and using an array offset.
		/// </summary>
		private class CachedTextureOffsetNonPow2 : CachedTexture
		{
			public CachedTextureOffsetNonPow2(int width, int height, int pixelFormat, int offset) : base(width, height, pixelFormat, offset)
			{
			}

			public override int readPixel(int u, int v)
			{
				return buffer[v * width + u + offset];
			}
		}

		private abstract class CachedTextureIndexed : CachedTexture
		{
			internal new int[] clut;
			internal int shift;
			internal int mask;
			internal int start;

			protected internal CachedTextureIndexed(int width, int height, int pixelFormat, int offset) : base(width, height, pixelFormat, offset)
			{
			}

			protected internal virtual int getClut(int index)
			{
				return clut[((index >> shift) & mask) | (start << 4)];
			}

			public virtual void setClut(int[] clut, int shift, int mask, int start)
			{
				this.clut = clut;
				this.shift = shift;
				this.mask = mask;
				this.start = start;
			}

			public override void setClut()
			{
				VideoEngine videoEngine = VideoEngine.Instance;
				GeContext context = videoEngine.Context;
				int clutNumEntries = videoEngine.ClutNumEntries;
				int[] clut = null;
				short[] shortClut;
				switch (context.tex_clut_mode)
				{
					case CMODE_FORMAT_16BIT_BGR5650:
						shortClut = videoEngine.readClut16(0);
						clut = new int[clutNumEntries];
						for (int i = 0; i < clut.Length; i++)
						{
							clut[i] = color565to8888(shortClut[i]);
						}
						break;
					case CMODE_FORMAT_16BIT_ABGR5551:
						shortClut = videoEngine.readClut16(0);
						clut = new int[clutNumEntries];
						for (int i = 0; i < clut.Length; i++)
						{
							clut[i] = color5551to8888(shortClut[i]);
						}
						break;
					case CMODE_FORMAT_16BIT_ABGR4444:
						shortClut = videoEngine.readClut16(0);
						clut = new int[clutNumEntries];
						for (int i = 0; i < clut.Length; i++)
						{
							clut[i] = color4444to8888(shortClut[i]);
						}
						break;
					case CMODE_FORMAT_32BIT_ABGR8888:
						int[] intClut = videoEngine.readClut32(0);
						clut = new int[clutNumEntries];
						Array.Copy(intClut, 0, clut, 0, clut.Length);
						break;
				}

				if (clut != null)
				{
					setClut(clut, context.tex_clut_shift, context.tex_clut_mask, context.tex_clut_start);
				}
			}
		}

		private class CachedTextureIndexed8Bit : CachedTextureIndexed
		{
			internal static readonly int[] shift8Bit = new int[] {0, 8, 16, 24};

			protected internal CachedTextureIndexed8Bit(int width, int height, int pixelFormat, int offset) : base(width, height, pixelFormat, offset)
			{
			}

			public virtual int readPixel(int u, int v)
			{
				int pixelIndex = v * width + u;
				int index = buffer[(pixelIndex >> 2) + offset] >> shift8Bit[pixelIndex & 3];
				return getClut(index & 0xFF);
			}
		}

		private class CachedTextureIndexed16Bit : CachedTextureIndexed
		{
			internal static readonly int[] shift16Bit = new int[] {0, 16};

			protected internal CachedTextureIndexed16Bit(int width, int height, int pixelFormat, int offset) : base(width, height, pixelFormat, offset)
			{
			}

			public virtual int readPixel(int u, int v)
			{
				int pixelIndex = v * width + u;
				int index = buffer[(pixelIndex >> 1) + offset] >> shift16Bit[pixelIndex & 1];
				return getClut(index & 0xFFFF);
			}
		}

		private class CachedTextureIndexed32Bit : CachedTextureIndexed
		{
			protected internal CachedTextureIndexed32Bit(int width, int height, int pixelFormat, int offset) : base(width, height, pixelFormat, offset)
			{
			}

			public virtual int readPixel(int u, int v)
			{
				int pixelIndex = v * width + u;
				int index = buffer[pixelIndex + offset];
				return getClut(index);
			}
		}

		public override string ToString()
		{
			return string.Format("CachedTexture[({0:D} x {1:D}), {2}]", Width, Height, VideoEngine.getPsmName(PixelFormat));
		}
	}

}