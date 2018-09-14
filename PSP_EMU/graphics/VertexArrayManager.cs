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
	public class VertexArrayManager
	{
		private static VertexArrayManager instance;
		private const int maxSize = 1000;
		private IList<VertexArray> vertexArrays = new LinkedList<VertexArray>();
		private CpuDurationStatistics statistics = new CpuDurationStatistics("VertexArrayManager");
		private Dictionary<int, VertexArray> fastLookup = new Dictionary<int, VertexArray>();

		public static VertexArrayManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new VertexArrayManager();
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

		private static int getFastLookupKey(int vtype, VertexBuffer vertexBuffer, int address, int stride)
		{
			return vertexBuffer.Id + (vtype << 8);
		}

		public virtual VertexArray getVertexArray(IRenderingEngine re, int vtype, VertexBuffer vertexBuffer, int address, int stride)
		{
			statistics.start();
			int fastLookupKey = getFastLookupKey(vtype, vertexBuffer, address, stride);
			VertexArray vertexArray = fastLookup[fastLookupKey];
			if (vertexArray != null)
			{
				if (vertexArray.isMatching(vtype, vertexBuffer, address, stride))
				{
					statistics.end();
					return vertexArray;
				}
			}

			bool first = true;
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
			for (IEnumerator<VertexArray> lit = vertexArrays.GetEnumerator(); lit.MoveNext();)
			{
				vertexArray = lit.Current;
				if (vertexArray.isMatching(vtype, vertexBuffer, address, stride))
				{
					if (!first)
					{
						// Move the VertexArray to the head of the list
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						lit.remove();
						vertexArrays.Insert(0, vertexArray);
					}
					fastLookup[fastLookupKey] = vertexArray;
					statistics.end();

					return vertexArray;
				}
				first = false;
			}

			vertexArray = new VertexArray(vtype, vertexBuffer, stride);
			vertexArrays.Insert(0, vertexArray);

			if (vertexArrays.Count > maxSize)
			{
				VertexArray toBeDeleted = vertexArrays.Remove(vertexArrays.Count - 1);
				if (toBeDeleted != null)
				{
					toBeDeleted.delete(re);
				}
			}

			statistics.end();

			return vertexArray;
		}

		public virtual void onVertexBufferDeleted(IRenderingEngine re, VertexBuffer vertexBuffer)
		{
			// Delete all the VertexArray using the deleted VertexBuffer
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
			for (IEnumerator<VertexArray> lit = vertexArrays.GetEnumerator(); lit.MoveNext();)
			{
				VertexArray vertexArray = lit.Current;
				if (vertexArray.VertexBuffer == vertexBuffer)
				{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					lit.remove();
					vertexArray.delete(re);
				}
			}
		}

		public virtual void forceReloadAllVertexArrays()
		{
			foreach (VertexArray vertexArray in vertexArrays)
			{
				vertexArray.forceReload();
			}
		}

		protected internal virtual void displayStatistics()
		{
			foreach (VertexArray vertexArray in vertexArrays)
			{
				VideoEngine.log_Renamed.info(vertexArray);
			}

			VideoEngine.log_Renamed.info(string.Format("VertexArrayManager: {0:D} VAOs", vertexArrays.Count));
		}
	}

}