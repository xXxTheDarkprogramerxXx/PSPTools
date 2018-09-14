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
namespace pspsharp.graphics
{

	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;
	using CacheStatistics = pspsharp.util.CacheStatistics;
	using DurationStatistics = pspsharp.util.DurationStatistics;

	public class VertexCache
	{
		public const int cacheMaxSize = 30000;
		public const float cacheLoadFactor = 0.75f;
		protected internal static VertexCache instance = null;
		private LinkedHashMap<int, VertexInfo> cache;
		protected internal CacheStatistics statistics = new CacheStatistics("Vertex", cacheMaxSize);
		// Remember which vertex have already been checked during one display
		// (for applications reusing the same vertex multiple times in one display)
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private ISet<int> vertexAlreadyChecked_Renamed;

		public static VertexCache Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new VertexCache();
				}
    
				return instance;
			}
		}

		protected internal VertexCache()
		{
			//
			// Create a cache having
			// - initial size large enough so that no rehash will occur
			// - the LinkedList is based on access-order for LRU
			//
			cache = new LinkedHashMap<int, VertexInfo>((int)(cacheMaxSize / cacheLoadFactor) + 1, cacheLoadFactor, true);
			vertexAlreadyChecked_Renamed = new HashSet<int>();
		}

		public virtual void exit()
		{
			if (DurationStatistics.collectStatistics)
			{
				VideoEngine.log_Renamed.info(statistics);
			}
		}

		private static int? getKey(VertexInfo vertexInfo)
		{
			return new int?(vertexInfo.ptr_vertex + vertexInfo.ptr_index);
		}

		public virtual bool hasVertex(VertexInfo vertexInfo)
		{
			return cache.containsKey(getKey(vertexInfo));
		}

		protected internal virtual VertexInfo getVertex(VertexInfo vertexInfo)
		{
			lock (this)
			{
				return cache.get(getKey(vertexInfo));
			}
		}

		public virtual void addVertex(IRenderingEngine re, VertexInfo vertexInfo, int numberOfVertex, float[][] boneMatrix, int numberOfWeightsForShader)
		{
			lock (this)
			{
				int? key = getKey(vertexInfo);
				VertexInfo previousVertex = cache.get(key);
				if (previousVertex != null)
				{
					vertexInfo.reuseCachedBuffer(previousVertex);
					previousVertex.deleteVertex(re);
				}
				else
				{
					// Check if the cache is not growing too large
					if (cache.size() >= cacheMaxSize)
					{
						// Remove the LRU cache entry
						IEnumerator<KeyValuePair<int, VertexInfo>> it = cache.entrySet().GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						if (it.hasNext())
						{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							KeyValuePair<int, VertexInfo> entry = it.next();
							entry.Value.deleteVertex(re);
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
							it.remove();
        
							statistics.entriesRemoved++;
						}
					}
				}
        
				vertexInfo.prepareForCache(this, numberOfVertex, boneMatrix, numberOfWeightsForShader);
				cache.put(key, vertexInfo);
        
				if (cache.size() > statistics.maxSizeUsed)
				{
					statistics.maxSizeUsed = cache.size();
				}
			}
		}

		public virtual VertexInfo getVertex(VertexInfo vertexInfo, int numberOfVertex, float[][] boneMatrix, int numberOfWeightsForShader)
		{
			statistics.totalHits++;
			VertexInfo vertex = getVertex(vertexInfo);

			if (vertex == null)
			{
				statistics.notPresentHits++;
				return vertex;
			}

			if (vertex.Equals(vertexInfo, numberOfVertex, boneMatrix, numberOfWeightsForShader))
			{
				statistics.successfulHits++;
				return vertex;
			}

			statistics.changedHits++;
			return null;
		}

		public virtual void resetVertexAlreadyChecked()
		{
			vertexAlreadyChecked_Renamed.Clear();
		}

		public virtual bool vertexAlreadyChecked(VertexInfo vertexInfo)
		{
			return vertexAlreadyChecked_Renamed.Contains(getKey(vertexInfo));
		}

		public virtual VertexInfo VertexAlreadyChecked
		{
			set
			{
				vertexAlreadyChecked_Renamed.Add(getKey(value));
			}
		}

		public virtual void reset(IRenderingEngine re)
		{
			lock (this)
			{
				foreach (VertexInfo vertexInfo in cache.values())
				{
					vertexInfo.deleteVertex(re);
				}
        
				cache.clear();
			}
		}
	}

}