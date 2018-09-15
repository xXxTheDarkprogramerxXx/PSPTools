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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.RuntimeContext.setLog4jMDC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.ExternalGE.numberRendererThread;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.NativeUtils.INTR_STAT_END;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.NativeUtils.coreInterpret;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.NativeUtils.getCoreInterrupt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.NativeUtils.getCoreMadr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.NativeUtils.getCoreSadr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.NativeUtils.getRendererIndexCount;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.NativeUtils.isCoreCtrlActive;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.externalge.NativeUtils.updateMemoryUnsafeAddr;


	//using Logger = org.apache.log4j.Logger;

	using MMIOHandlerGe = pspsharp.memory.mmio.MMIOHandlerGe;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class CoreThreadMMIO : Thread
	{
		protected internal static Logger log = ExternalGE.log;
		private static CoreThreadMMIO instance;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private volatile bool exit_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private Semaphore sync_Renamed;

		public static CoreThreadMMIO Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CoreThreadMMIO();
					instance.Daemon = true;
					instance.Name = "ExternalGE - Core Thread for MMIO";
					instance.Start();
				}
    
				return instance;
			}
		}

		public static void exit()
		{
			if (instance != null)
			{
				instance.exit_Renamed = true;
				instance = null;
			}
		}

		private CoreThreadMMIO()
		{
			sync_Renamed = new Semaphore(0);
		}

		public override void run()
		{
			setLog4jMDC();
			while (!exit_Renamed)
			{
				if (!CoreCtrlActive || CoreMadr == CoreSadr)
				{
					if (!Emulator.pause && log.TraceEnabled)
					{
						log.trace(string.Format("CoreThreadMMIO not active... waiting"));
					}

					waitForSync(1000);
				}
				else
				{
					updateMemoryUnsafeAddr();

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("CoreThreadMMIO processing 0x{0:X8}", NativeUtils.CoreMadr));
					}

					while (coreInterpret())
					{
						updateMemoryUnsafeAddr();

						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("CoreThreadMMIO looping at 0x{0:X8}", NativeUtils.CoreMadr));
						}

						if (numberRendererThread > 0 && RendererIndexCount > 0)
						{
							break;
						}
					}

					int interrupt = CoreInterrupt;
					if ((interrupt & INTR_STAT_END) != 0)
					{
						MMIOHandlerGe.Instance.onGeInterrupt();
					}

					if (numberRendererThread > 0 && RendererIndexCount > 0)
					{
						ExternalGE.render();
					}
				}
			}

			log.info(string.Format("CoreThreadMMIO exited"));
		}

		public virtual void sync()
		{
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
					Console.WriteLine(string.Format("CoreThreadMMIO waitForSync {0}", e));
				}
			}

			return true;
		}
	}

}