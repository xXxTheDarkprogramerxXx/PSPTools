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
namespace pspsharp.graphics.textures
{
	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;
	using Hash = pspsharp.util.Hash;

	public class Texture
	{
		private int addr;
		private int lineWidth;
		private int width;
		private int height;
		private int pixelStorage;
		private int clutAddr;
		private int clutMode;
		private int clutStart;
		private int clutShift;
		private int clutMask;
		private int clutNumBlocks;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private int hashCode_Renamed;
		private int mipmapLevels;
		private bool mipmapShareClut;
		private int textureId = -1; // id created by genTexture
		private bool loaded = false; // is the texture already loaded?
		private TextureCache textureCache;
		private const int defaultHashStride = 64 + 8;
		private const int smallHashStride = 12;
		private short[] cachedValues16;
		private int[] cachedValues32;
		private int bufferLengthInBytes;
		private int lineWidthInBytes;
		private int hashStrideInBytes;

		public Texture(TextureCache textureCache, int addr, int lineWidth, int width, int height, int pixelStorage, int clutAddr, int clutMode, int clutStart, int clutShift, int clutMask, int clutNumBlocks, int mipmapLevels, bool mipmapShareClut, short[] values16, int[] values32)
		{
			this.textureCache = textureCache;
			this.addr = addr;
			this.lineWidth = lineWidth;
			this.width = width;
			this.height = height;
			this.pixelStorage = pixelStorage;
			this.clutAddr = clutAddr;
			this.clutMode = clutMode;
			this.clutStart = clutStart;
			this.clutShift = clutShift;
			this.clutMask = clutMask;
			this.clutNumBlocks = clutNumBlocks;
			this.mipmapLevels = mipmapLevels;
			this.mipmapShareClut = mipmapShareClut;

			bufferLengthInBytes = lineWidth * height;
			lineWidthInBytes = lineWidth;
			hashStrideInBytes = defaultHashStride;
			int bytesPerPixel = pspsharp.graphics.RE.IRenderingEngine_Fields.sizeOfTextureType[pixelStorage];
			if (bytesPerPixel <= 0)
			{
				// Special texture types
				switch (pixelStorage)
				{
					case GeCommands.TPSM_PIXEL_STORAGE_MODE_DXT1:
						bufferLengthInBytes = VideoEngine.getCompressedTextureSize(lineWidth, height, 8);
						break;
					case GeCommands.TPSM_PIXEL_STORAGE_MODE_DXT3:
					case GeCommands.TPSM_PIXEL_STORAGE_MODE_DXT5:
						bufferLengthInBytes = VideoEngine.getCompressedTextureSize(lineWidth, height, 4);
						break;
					case GeCommands.TPSM_PIXEL_STORAGE_MODE_4BIT_INDEXED:
						bufferLengthInBytes >>= 1;
						lineWidthInBytes >>= 1;
						// Take a smaller hash stride for 4-bit indexed textures to better detect small texture changes
						// (e.g. for textures representing text)
						hashStrideInBytes = smallHashStride;
						break;
				}
			}
			else
			{
				bufferLengthInBytes *= bytesPerPixel;
				lineWidthInBytes *= bytesPerPixel;
			}

			if (values16 != null)
			{
				cachedValues16 = new short[lineWidth];
				Array.Copy(values16, 0, cachedValues16, 0, lineWidth);
			}
			else if (values32 != null)
			{
				cachedValues32 = new int[lineWidth];
				Array.Copy(values32, 0, cachedValues32, 0, lineWidth);
			}
			else
			{
				if (lineWidthInBytes < hashStrideInBytes)
				{
					if (lineWidthInBytes <= 32)
					{
						// No stride at all for narrow textures
						hashStrideInBytes = 0;
					}
					else
					{
						hashStrideInBytes = lineWidthInBytes - 4;
					}
				}
				hashCode_Renamed = GetHashCode(addr, bufferLengthInBytes, lineWidthInBytes, hashStrideInBytes, clutAddr, clutNumBlocks, mipmapLevels);
			}
		}

		/// <summary>
		/// Compute the Texture hashCode value,
		/// based on the pixel buffer and the clut table.
		/// </summary>
		/// <param name="addr">                pixel buffer </param>
		/// <param name="bufferLengthInBytes"> texture buffer length in bytes </param>
		/// <param name="lineWidthInBytes">    texture buffer line width in bytes </param>
		/// <param name="clutAddr">            clut table address </param>
		/// <param name="clutNumBlocks">       clut number of blocks </param>
		/// <param name="mipmapLevels">        number of mipmaps </param>
		/// <returns>                    hashcode value </returns>
		private static int GetHashCode(int addr, int bufferLengthInBytes, int lineWidthInBytes, int strideInBytes, int clutAddr, int clutNumBlocks, int mipmapLevels)
		{
			int hashCode = mipmapLevels;

			if (addr != 0)
			{
				if (VideoEngine.log_Renamed.DebugEnabled)
				{
					VideoEngine.log_Renamed.debug("Texture.hashCode: " + bufferLengthInBytes + " bytes");
				}

				hashCode = Hash.getHashCode(hashCode, addr, bufferLengthInBytes, strideInBytes);
			}

			if (clutAddr != 0)
			{
				hashCode = Hash.getHashCode(hashCode, clutAddr, clutNumBlocks * 32);
			}

			return hashCode;
		}

		public override int GetHashCode()
		{
			return hashCode_Renamed;
		}

		public virtual bool Equals(int addr, int lineWidth, int width, int height, int pixelStorage, int clutAddr, int clutMode, int clutStart, int clutShift, int clutMask, int clutNumBlocks, int mipmapLevels, bool mipmapShareClut, short[] values16, int[] values32)
		{
			if (this.addr != addr || this.lineWidth != lineWidth || this.width != width || this.height != height || this.pixelStorage != pixelStorage || this.clutAddr != clutAddr || this.clutMode != clutMode || this.clutStart != clutStart || this.clutShift != clutShift || this.clutMask != clutMask || this.clutNumBlocks != clutNumBlocks || this.mipmapLevels != mipmapLevels || this.mipmapShareClut != mipmapShareClut)
			{
				return false;
			}

			// Do not compute the hashCode of the new texture if it has already
			// been checked during this display cycle
			if (!textureCache.textureAlreadyHashed(addr, clutAddr, clutStart, clutMode))
			{
				if (values16 != null)
				{
					return Equals(values16);
				}
				if (values32 != null)
				{
					return Equals(values32);
				}
				int hashCode = Texture.GetHashCode(addr, bufferLengthInBytes, lineWidthInBytes, hashStrideInBytes, clutAddr, clutNumBlocks, mipmapLevels);
				if (hashCode != this.GetHashCode())
				{
					return false;
				}
				textureCache.setTextureAlreadyHashed(addr, clutAddr, clutStart, clutMode);
			}

			return true;
		}

		private bool Equals(short[] values16)
		{
			if (cachedValues16 == null)
			{
				return false;
			}

			for (int i = 0; i < lineWidth; i++)
			{
				if (values16[i] != cachedValues16[i])
				{
					return false;
				}
			}

			return true;
		}

		private bool Equals(int[] values32)
		{
			if (cachedValues32 == null)
			{
				return false;
			}

			for (int i = 0; i < lineWidth; i++)
			{
				if (values32[i] != cachedValues32[i])
				{
					return false;
				}
			}

			return true;
		}

		public virtual void bindTexture(IRenderingEngine re)
		{
			re.bindTexture(getTextureId(re));
		}

		public virtual int getTextureId(IRenderingEngine re)
		{
			if (textureId == -1)
			{
				textureId = re.genTexture();
			}
			return textureId;
		}

		public virtual void deleteTexture(IRenderingEngine re)
		{
			if (textureId != -1)
			{
				re.deleteTexture(textureId);
				textureId = -1;
			}

			Loaded = false;
		}

		public virtual bool Loaded
		{
			get
			{
				return loaded;
			}
			set
			{
				this.loaded = value;
			}
		}

		public virtual void setIsLoaded()
		{
			Loaded = true;
		}


		public virtual int Addr
		{
			get
			{
				return addr;
			}
		}

		public virtual int ClutAddr
		{
			get
			{
				return clutAddr;
			}
		}

		public virtual int ClutMode
		{
			get
			{
				return clutMode;
			}
		}

		public virtual int ClutStart
		{
			get
			{
				return clutStart;
			}
		}

		public virtual int GlId
		{
			get
			{
				return textureId;
			}
		}

		public virtual int MipmapLevels
		{
			get
			{
				return mipmapLevels;
			}
		}

		public virtual bool isInsideMemory(int fromAddr, int toAddr)
		{
			if (addr >= fromAddr && addr < toAddr)
			{
				return true;
			}
			if (addr + bufferLengthInBytes >= fromAddr && addr + bufferLengthInBytes < toAddr)
			{
				return true;
			}

			return false;
		}

		public override string ToString()
		{
			return string.Format("Texture[0x{0:X8}, {1:D}x{2:D}, bufferWidth={3:D}, {4}]", addr, width, height, lineWidth, VideoEngine.getPsmName(pixelStorage));
		}
	}

}