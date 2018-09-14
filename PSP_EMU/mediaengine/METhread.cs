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
namespace pspsharp.mediaengine
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.RuntimeContext.setLog4jMDC;


	/// <summary>
	/// Thread running the Media Engine processor.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class METhread : Thread
	{
		private static METhread instance;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private volatile bool exit_Renamed;
		private MEProcessor processor;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private readonly Semaphore sync_Renamed;

		public static METhread Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new METhread();
				}
				return instance;
			}
		}

		public static bool isMediaEngine(Thread thread)
		{
			return thread == instance;
		}

		public static void exit()
		{
			if (instance != null)
			{
				instance.exit_Renamed = true;
			}
		}

		private METhread()
		{
			sync_Renamed = new Semaphore(0);

			Name = "Media Engine Thread";
			Daemon = true;
			start();
		}

		public virtual MEProcessor Processor
		{
			set
			{
				this.processor = value;
			}
		}

		public override void run()
		{
			setLog4jMDC();
			while (!exit_Renamed)
			{
				if (waitForSync(100))
				{
					processor.run();
				}
			}
		}

		private bool waitForSync(int millis)
		{
			while (true)
			{
				try
				{
					int availablePermits = sync_Renamed.drainPermits();
					if (availablePermits > 0)
					{
						break;
					}

					if (sync_Renamed.tryAcquire(millis, TimeUnit.MILLISECONDS))
					{
						break;
					}
					return false;
				}
				catch (InterruptedException e)
				{
					// Ignore exception and retry again
					processor.Logger.debug(string.Format("METhread.waitForSync {0}", e));
				}
			}

			return true;
		}

		public virtual void sync()
		{
			if (sync_Renamed != null)
			{
				sync_Renamed.release();
			}
		}
	}

}