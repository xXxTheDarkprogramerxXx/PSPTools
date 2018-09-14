using System.Threading;

/*

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
//	import static pspsharp.util.Utilities.sleep;


	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class RendererExecutor
	{
		private const int numberThreads = 1;
		private static RendererExecutor instance;
		private readonly LinkedBlockingQueue<IRenderer> renderersQueue = new LinkedBlockingQueue<IRenderer>();
		private volatile bool ended;
		private volatile int numberThreadsRendering;
		private readonly object numberThreadsRenderingLock = new object();

		public static RendererExecutor Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new RendererExecutor();
				}
    
				return instance;
			}
		}

		private RendererExecutor()
		{
			for (int i = 0; i < numberThreads; i++)
			{
				Thread thread = new ThreadRenderer(this);
				thread.Name = string.Format("Thread SoftwareRenderer #{0:D}", i + 1);
				thread.Daemon = true;
				thread.Start();
			}
		}

		public static void exit()
		{
			if (instance != null)
			{
				instance.ended = true;
			}
		}

		public virtual void render(IRenderer renderer)
		{
			if (numberThreads > 0 && !VideoEngine.log_Renamed.TraceEnabled)
			{
				// Queue for rendering in a ThreadRenderer thread
				renderer = renderer.duplicate();
				renderersQueue.add(renderer);
			}
			else
			{
				// Threads are disabled or capture is active, render immediately
				try
				{
					renderer.render();
				}
				catch (Exception e)
				{
					VideoEngine.log_Renamed.error("Error while rendering", e);
				}
			}
		}

		public virtual void waitForRenderingCompletion()
		{
			if (numberThreads > 0)
			{
				while (!renderersQueue.Empty || numberThreadsRendering > 0)
				{
					sleep(1, 0);
				}
			}
		}

		private class ThreadRenderer : Thread
		{
			private readonly RendererExecutor outerInstance;

			public ThreadRenderer(RendererExecutor outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void run()
			{
				while (!outerInstance.ended)
				{
					IRenderer renderer = null;
					try
					{
						renderer = outerInstance.renderersQueue.poll(100, TimeUnit.MILLISECONDS);
					}
					catch (InterruptedException)
					{
						// Ignore Exception
					}

					if (renderer != null)
					{
						lock (outerInstance.numberThreadsRenderingLock)
						{
							outerInstance.numberThreadsRendering++;
						}

						try
						{
							renderer.render();
						}
						catch (Exception e)
						{
							VideoEngine.log_Renamed.error("Error while rendering", e);
						}

						lock (outerInstance.numberThreadsRenderingLock)
						{
							outerInstance.numberThreadsRendering--;
						}
					}
				}
			}
		}
	}

}