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
	using CpuDurationStatistics = pspsharp.util.CpuDurationStatistics;
	using DurationStatistics = pspsharp.util.DurationStatistics;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class VertexBufferManager
	{
		private static VertexBufferManager instance;
		private const int maxSize = 1000;
		private IList<VertexBuffer> vertexBuffers = new LinkedList<VertexBuffer>();
		private const int allowedVertexGapSize = 4 * 1024;
		private CpuDurationStatistics statistics = new CpuDurationStatistics("VertexBufferManager");
		private Dictionary<int, VertexBuffer> fastLookup = new Dictionary<int, VertexBuffer>();

		public static VertexBufferManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new VertexBufferManager();
				}
    
				return instance;
			}
		}

		public static void exit()
		{
			if (instance != null)
			{
				if (DurationStatistics.collectStatistics)
				{
					VideoEngine.log_Renamed.info(instance.statistics);
				}
			}
		}

		private static int getFastLookupKey(int address, int Length)
		{
			return address;
		}

		private static int getFastLookupKey(int address, int Length, int stride)
		{
			return address + (stride << 24);
		}

		public virtual VertexBuffer getVertexBuffer(IRenderingEngine re, int address, int Length, int stride, bool strideAligned)
		{
			lock (this)
			{
				statistics.start();
				if (strideAligned)
				{
					int fastLookupKey = getFastLookupKey(address, Length, stride);
					VertexBuffer vertexBuffer = fastLookup[fastLookupKey];
					if (vertexBuffer != null)
					{
						if (stride == vertexBuffer.Stride && (vertexBuffer.getBufferOffset(address) % stride) == 0)
						{
							if (vertexBuffer.isAddressInside(address, Length, allowedVertexGapSize))
							{
								statistics.end();
								return vertexBuffer;
							}
						}
					}
        
					bool first = true;
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
					for (IEnumerator<VertexBuffer> lit = vertexBuffers.GetEnumerator(); lit.MoveNext();)
					{
						vertexBuffer = lit.Current;
						if (stride == vertexBuffer.Stride && (vertexBuffer.getBufferOffset(address) % stride) == 0)
						{
							if (vertexBuffer.isAddressInside(address, Length, allowedVertexGapSize))
							{
								if (!first)
								{
									// Move the VertexBuffer to the head of the list
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
									lit.remove();
									vertexBuffers.Insert(0, vertexBuffer);
								}
								fastLookup[fastLookupKey] = vertexBuffer;
								statistics.end();
        
								return vertexBuffer;
							}
						}
						first = false;
					}
				}
				else
				{
					int fastLookupKey = getFastLookupKey(address, Length);
					VertexBuffer vertexBuffer = fastLookup[fastLookupKey];
					if (vertexBuffer != null)
					{
						if (vertexBuffer.isAddressInside(address, Length, allowedVertexGapSize))
						{
							statistics.end();
							return vertexBuffer;
						}
					}
        
					bool first = true;
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
					for (IEnumerator<VertexBuffer> lit = vertexBuffers.GetEnumerator(); lit.MoveNext();)
					{
						vertexBuffer = lit.Current;
						if (vertexBuffer.isAddressInside(address, Length, allowedVertexGapSize))
						{
							if (!first)
							{
								// Move the VertexBuffer to the head of the list
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
								lit.remove();
								vertexBuffers.Insert(0, vertexBuffer);
							}
							fastLookup[fastLookupKey] = vertexBuffer;
							statistics.end();
        
							return vertexBuffer;
						}
						first = false;
					}
				}
        
				VertexBuffer vertexBuffer = new VertexBuffer(address, stride);
				vertexBuffers.Insert(0, vertexBuffer);
        
				if (vertexBuffers.Count > maxSize && re != null)
				{
					VertexBuffer toBeDeleted = vertexBuffers.Remove(vertexBuffers.Count - 1);
					if (re.VertexArrayAvailable)
					{
						VertexArrayManager.Instance.onVertexBufferDeleted(re, toBeDeleted);
					}
					toBeDeleted.delete(re);
				}
        
				statistics.end();
        
				return vertexBuffer;
			}
		}

		public virtual void resetAddressAlreadyChecked()
		{
			lock (this)
			{
				foreach (VertexBuffer vertexBuffer in vertexBuffers)
				{
					vertexBuffer.resetAddressAlreadyChecked();
				}
			}
		}

		protected internal virtual void displayStatistics()
		{
			int Length = 0;
			foreach (VertexBuffer vertexBuffer in vertexBuffers)
			{
				VideoEngine.log_Renamed.info(vertexBuffer);
				Length += vertexBuffer.Length;
			}

			VideoEngine.log_Renamed.info(string.Format("VertexBufferManager: {0:D} buffers, total Length {1:D}", vertexBuffers.Count, Length));
		}

		public virtual void reset(IRenderingEngine re)
		{
			lock (this)
			{
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
				for (IEnumerator<VertexBuffer> lit = vertexBuffers.GetEnumerator(); lit.MoveNext();)
				{
					VertexBuffer vertexBuffer = lit.Current;
					vertexBuffer.delete(re);
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					lit.remove();
				}
			}
		}
	}

}