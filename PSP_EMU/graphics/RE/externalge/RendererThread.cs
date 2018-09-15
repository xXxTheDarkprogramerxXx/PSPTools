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
namespace pspsharp.graphics.RE.externalge
{

	//using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class RendererThread : Thread
	{
		private static readonly Logger log = ExternalGE.log;
		private int lineMask;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private Semaphore sync_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private volatile bool exit_Renamed;
		private Semaphore response;

		public RendererThread(int lineMask)
		{
			this.lineMask = lineMask;
			sync_Renamed = new Semaphore(0);
		}

		public override void run()
		{
			while (!exit_Renamed)
			{
				if (waitForSync(100))
				{
					if (lineMask != 0)
					{
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Starting async rendering lineMask=0x{0:X8}", lineMask));
						}
						NativeUtils.rendererRender(lineMask);
					}

					if (response != null)
					{
						// Be careful to clear the response before releasing it!
						Semaphore responseToBeReleased = response;
						response = null;
						responseToBeReleased.release();
					}
				}
			}
		}

		public virtual void exit()
		{
			exit_Renamed = true;
			sync(null);
		}

		public virtual void sync(Semaphore response)
		{
			this.response = response;
			if (sync_Renamed != null)
			{
				sync_Renamed.release();
			}
		}

		private bool waitForSync(int millis)
		{
			while (true)
			{
				try
				{
					if (sync_Renamed.tryAcquire(millis, TimeUnit.MILLISECONDS))
					{
						break;
					}
					return false;
				}
				catch (InterruptedException)
				{
					// Ignore exception and retry again
				}
			}

			return true;
		}
	}

}