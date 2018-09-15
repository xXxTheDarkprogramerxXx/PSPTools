using System.Collections.Generic;

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

	//using Logger = org.apache.log4j.Logger;

	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;
	using CacheStatistics = pspsharp.util.CacheStatistics;

	public class TextureCache
	{
		public const int cacheMaxSize = 1000;
		public const float cacheLoadFactor = 0.75f;
		private static Logger log = VideoEngine.log_Renamed;
		private static TextureCache instance = null;
		private LinkedHashMap<int, Texture> cache;
		public CacheStatistics statistics = new CacheStatistics("Texture", cacheMaxSize);
		// Remember which textures have already been hashed during one display
		// (for applications reusing the same texture multiple times in one display)
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private ISet<int> textureAlreadyHashed_Renamed;
		// Remember which textures are located in VRAM. Only these textures have to be
		// scanned when checking for textures updated while rendering to GE.
		private LinkedList<Texture> vramTextures = new LinkedList<Texture>();

		public static TextureCache Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new TextureCache();
				}
    
				return instance;
			}
		}

		private TextureCache()
		{
			//
			// Create a cache having
			// - initial size large enough so that no rehash will occur
			// - the LinkedList is based on access-order for LRU
			//
			cache = new LinkedHashMap<int, Texture>((int)(cacheMaxSize / cacheLoadFactor) + 1, cacheLoadFactor, true);
			textureAlreadyHashed_Renamed = new HashSet<int>();
		}

		private int? getKey(int addr, int clutAddr, int clutStart, int clutMode)
		{
			// Some games use the same texture address with different cluts.
			// Keep a combination of both texture address and clut address in the cache.
			// Also, use the clutStart as this parameter can be used to offset the clut address.
			int clutEntrySize = clutMode == GeCommands.CMODE_FORMAT_32BIT_ABGR8888 ? 4 : 2;
			return new int?(addr + clutAddr + (clutStart << 4) * clutEntrySize);
		}

		public virtual bool hasTexture(int addr, int clutAddr, int clutStart, int clutMode)
		{
			return cache.containsKey(getKey(addr, clutAddr, clutStart, clutMode));
		}

		private Texture getTexture(int addr, int clutAddr, int clutStart, int clutMode)
		{
			return cache.get(getKey(addr, clutAddr, clutStart, clutMode));
		}

		public virtual void addTexture(IRenderingEngine re, Texture texture)
		{
			int? key = getKey(texture.Addr, texture.ClutAddr, texture.ClutStart, texture.ClutMode);
			Texture previousTexture = cache.get(key);
			if (previousTexture != null)
			{
				previousTexture.deleteTexture(re);
//JAVA TO C# CONVERTER TODO TASK: There is no .NET LinkedList equivalent to the Java 'remove' method:
				vramTextures.remove(previousTexture);
			}
			else
			{
				// Check if the cache is not growing too large
				if (cache.size() >= cacheMaxSize)
				{
					// Remove the LRU cache entry
					IEnumerator<KeyValuePair<int, Texture>> it = cache.entrySet().GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if (it.hasNext())
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						KeyValuePair<int, Texture> entry = it.next();
						Texture lruTexture = entry.Value;
						lruTexture.deleteTexture(re);
//JAVA TO C# CONVERTER TODO TASK: There is no .NET LinkedList equivalent to the Java 'remove' method:
						vramTextures.remove(lruTexture);
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						it.remove();

						statistics.entriesRemoved++;
					}
				}
			}

			cache.put(key, texture);
			if (isVramTexture(texture))
			{
				vramTextures.AddLast(texture);
			}

			if (cache.size() > statistics.maxSizeUsed)
			{
				statistics.maxSizeUsed = cache.size();
			}
		}

		public virtual Texture getTexture(int addr, int lineWidth, int width, int height, int pixelStorage, int clutAddr, int clutMode, int clutStart, int clutShift, int clutMask, int clutNumBlocks, int mipmapLevels, bool mipmapShareClut, short[] values16, int[] values32)
		{
			statistics.totalHits++;
			Texture texture = getTexture(addr, clutAddr, clutStart, clutMode);

			if (texture == null)
			{
				statistics.notPresentHits++;
				return texture;
			}

			if (texture.Equals(addr, lineWidth, width, height, pixelStorage, clutAddr, clutMode, clutStart, clutShift, clutMask, clutNumBlocks, mipmapLevels, mipmapShareClut, values16, values32))
			{
				statistics.successfulHits++;
				return texture;
			}

			statistics.changedHits++;
			return null;
		}

		public virtual void resetTextureAlreadyHashed()
		{
			textureAlreadyHashed_Renamed.Clear();
		}

		public virtual bool textureAlreadyHashed(int addr, int clutAddr, int clutStart, int clutMode)
		{
			return textureAlreadyHashed_Renamed.Contains(getKey(addr, clutAddr, clutStart, clutMode));
		}

		public virtual void setTextureAlreadyHashed(int addr, int clutAddr, int clutStart, int clutMode)
		{
			textureAlreadyHashed_Renamed.Add(getKey(addr, clutAddr, clutStart, clutMode));
		}

		public virtual void resetTextureAlreadyHashed(int addr, int clutAddr, int clutStart, int clutMode)
		{
			textureAlreadyHashed_Renamed.remove(getKey(addr, clutAddr, clutStart, clutMode));
		}

		public virtual void reset(IRenderingEngine re)
		{
			foreach (Texture texture in cache.values())
			{
				texture.deleteTexture(re);
			}
			cache.clear();
			resetTextureAlreadyHashed();
		}

		private bool isVramTexture(Texture texture)
		{
			return Memory.isVRAM(texture.Addr);
		}

		public virtual void deleteVramTextures(IRenderingEngine re, int addr, int Length)
		{
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
			for (IEnumerator<Texture> lit = vramTextures.GetEnumerator(); lit.MoveNext();)
			{
				Texture texture = lit.Current;
				if (texture.isInsideMemory(addr, addr + Length))
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Delete VRAM texture inside GE {0}", texture.ToString()));
					}
					texture.deleteTexture(re);
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					lit.remove();
					int? key = getKey(texture.Addr, texture.ClutAddr, texture.ClutStart, texture.ClutMode);
					cache.remove(key);
					statistics.entriesRemoved++;
				}
			}
		}
	}

}