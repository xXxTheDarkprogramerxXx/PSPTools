using System.Threading;

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

	using CpuDurationStatistics = pspsharp.util.CpuDurationStatistics;
	using DurationStatistics = pspsharp.util.DurationStatistics;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class AsyncVertexCache : VertexCache
	{
		internal AsyncVertexCacheThread asyncVertexCacheThread;
		private bool useVertexArray = false;
		private VertexInfo vinfo = new VertexInfo();

		public static AsyncVertexCache Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new AsyncVertexCache();
				}
    
				return (AsyncVertexCache) instance;
			}
		}

		protected internal AsyncVertexCache()
		{
			asyncVertexCacheThread = new AsyncVertexCacheThread(this);
			asyncVertexCacheThread.Name = "Async Vertex Cache Thread";
			asyncVertexCacheThread.Daemon = true;
			asyncVertexCacheThread.Start();
		}

		private void asyncLoadVertex(VertexInfo vertexInfo, AsyncEntry asyncEntry)
		{
			// TODO
		}

		private void asyncCheckVertex(VertexInfo vertex, VertexInfo vertexInfo, AsyncEntry asyncEntry)
		{
			if (!vertex.Equals(vertexInfo, asyncEntry.count))
			{
				VertexAlreadyChecked = vertexInfo;
				vertex.setDirty();
				asyncLoadVertex(vertexInfo, asyncEntry);
			}
		}

		public virtual void asyncCheck(AsyncEntry asyncEntry)
		{
			vinfo.setDirty();
			vinfo.processType(asyncEntry.vtype);
			vinfo.ptr_vertex = asyncEntry.vertices;
			vinfo.ptr_index = asyncEntry.indices;
			vinfo.prepareForCache(this, asyncEntry.count, null, 0);

			if (useVertexArray)
			{
				int address = vinfo.ptr_vertex;
				int Length = vinfo.vertexSize * asyncEntry.count;
				if (Length > 0)
				{
					VertexBuffer vertexBuffer = VertexBufferManager.Instance.getVertexBuffer(null, address, Length, vinfo.vertexSize, useVertexArray);
					if (vertexBuffer != null)
					{
						Buffer buffer = Memory.Instance.getBuffer(address, Length);
						vertexBuffer.preLoad(buffer, address, Length);
					}
				}
			}
			else
			{
				VertexInfo vertex = getVertex(vinfo);

				if (vertex == null)
				{
					asyncLoadVertex(vinfo, asyncEntry);
				}
				else
				{
					asyncCheckVertex(vertex, vinfo, asyncEntry);
				}
			}
		}

		public virtual void addAsyncCheck(int prim, int vtype, int count, int indices, int vertices)
		{
			if (Memory.isAddressGood(vertices))
			{
				AsyncEntry asyncEntry = new AsyncEntry(prim, vtype, count, indices, vertices);
				asyncVertexCacheThread.addAsyncEntry(asyncEntry);
			}
		}

		public override void exit()
		{
			base.exit();
			asyncVertexCacheThread.exit();
		}

		public virtual bool UseVertexArray
		{
			set
			{
				this.useVertexArray = value;
			}
		}

		private class AsyncVertexCacheThread : Thread
		{
			internal AsyncVertexCache asyncVertexCache;
			internal BlockingQueue<AsyncEntry> asyncEntries = new LinkedBlockingQueue<AsyncEntry>();
			internal volatile bool done = false;
			public CpuDurationStatistics statistics = new CpuDurationStatistics("Async Vertex Cache Thread");

			public AsyncVertexCacheThread(AsyncVertexCache asyncVertexCache)
			{
				this.asyncVertexCache = asyncVertexCache;
			}

			public virtual void exit()
			{
				done = true;
				// Add a dummy entry to allow the thread to exit
				asyncEntries.add(new AsyncEntry());
				if (DurationStatistics.collectStatistics)
				{
					VideoEngine.log_Renamed.info(statistics);
				}
			}

			public override void run()
			{
				while (!done)
				{
					try
					{
						AsyncEntry asyncEntry = asyncEntries.take();
						if (asyncEntry != null && !done)
						{
							statistics.start();
							asyncVertexCache.asyncCheck(asyncEntry);
							statistics.end();
						}
					}
					catch (InterruptedException)
					{
						// Ignore Exception
					}
				}
			}

			public virtual void addAsyncEntry(AsyncEntry asyncEntry)
			{
				asyncEntries.add(asyncEntry);
			}
		}

		private class AsyncEntry
		{
			public int prim;
			public int vtype;
			public int count;
			public int indices;
			public int vertices;

			public AsyncEntry()
			{
			}

			public AsyncEntry(int prim, int vtype, int count, int indices, int vertices)
			{
				this.prim = prim;
				this.vtype = vtype;
				this.count = count;
				this.indices = indices;
				this.vertices = vertices;
			}

			public override string ToString()
			{
				return string.Format("AsyncEntry(prim={0:D}, vtype=0x{1:X}, count={2:D}, indices=0x{3:X8}, vertices=0x{4:X8}", prim, vtype, count, indices, vertices);
			}
		}
	}

}