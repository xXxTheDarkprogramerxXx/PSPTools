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
//	import static pspsharp.HLE.modules.sceGe_user.PSP_GE_MATRIX_PROJECTION;


	using Level = org.apache.log4j.Level;
	//using Logger = org.apache.log4j.Logger;

	using Modules = pspsharp.HLE.Modules;
	using PspGeList = pspsharp.HLE.kernel.types.PspGeList;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using sceDisplay = pspsharp.HLE.modules.sceDisplay;
	using sceGe_user = pspsharp.HLE.modules.sceGe_user;
	using CaptureManager = pspsharp.graphics.capture.CaptureManager;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using Settings = pspsharp.settings.Settings;
	using DurationStatistics = pspsharp.util.DurationStatistics;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class ExternalGE
	{
		public const int numberRendererThread = 4;
		public static bool activateWhenAvailable = false;
		public const bool useUnsafe = false;
		//public static Logger log = Logger.getLogger("externalge");
		private static ConcurrentLinkedQueue<PspGeList> drawListQueue;
		private static volatile PspGeList currentList;
		private static RendererThread[] rendererThreads;
		private static Semaphore rendererThreadsDone;
		private static Level logLevel;
		private static SetLogLevelThread setLogLevelThread;
		private static int screenScale = 1;
		private static object screenScaleLock = new object();
		private static ExternalGESettingsListerner externalGESettingsListerner;

		private class SetLogLevelThread : Thread
		{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal volatile bool exit_Renamed;

			public virtual void exit()
			{
				exit_Renamed = true;
			}

			public override void run()
			{
				while (!exit_Renamed)
				{
					NativeUtils.setLogLevel();
					Utilities.sleep(100);
				}
			}
		}

		private class ExternalGESettingsListerner : AbstractBoolSettingsListener
		{
			protected internal override void settingsValueChanged(bool value)
			{
				activateWhenAvailable = value;
				init();
			}
		}

		private static void activate()
		{
			drawListQueue = new ConcurrentLinkedQueue<PspGeList>();

			setLogLevelThread = new SetLogLevelThread();
			setLogLevelThread.setName("ExternelGE Set Log Level Thread");
			setLogLevelThread.setDaemon(true);
			setLogLevelThread.Start();

			if (numberRendererThread > 0)
			{
				rendererThreads = new RendererThread[numberRendererThread];
				int[] lineMasks = new int[numberRendererThread];
				switch (numberRendererThread)
				{
					case 1:
						lineMasks[0] = unchecked((int)0xFFFFFFFF);
						break;
					case 2:
						lineMasks[0] = unchecked((int)0xFF00FF00);
						lineMasks[1] = 0x00FF00FF;
						break;
					case 3:
						lineMasks[0] = unchecked((int)0xF801F001);
						lineMasks[1] = 0x07C00F80;
						lineMasks[3] = 0x003E007E;
						break;
					case 4:
					case 5:
					case 6:
					case 7:
						lineMasks[0] = unchecked((int)0xFF000000);
						lineMasks[1] = 0x00FF0000;
						lineMasks[2] = 0x0000FF00;
						lineMasks[3] = 0x000000FF;
						break;
					case 8:
					default:
						lineMasks[0] = unchecked((int)0xC000C000);
						lineMasks[1] = 0x30003000;
						lineMasks[2] = 0x0C000C00;
						lineMasks[3] = 0x03000300;
						lineMasks[4] = 0x00C000C0;
						lineMasks[5] = 0x00300030;
						lineMasks[6] = 0x000C000C;
						lineMasks[7] = 0x00030003;
						break;
				}

				int allLineMasks = 0;
				for (int i = 0; i < rendererThreads.Length; i++)
				{
					int lineMask = lineMasks[i];
					rendererThreads[i] = new RendererThread(lineMask);
					rendererThreads[i].Name = string.Format("Renderer Thread #{0:D}", i);
					rendererThreads[i].Start();

					if ((allLineMasks & lineMask) != 0)
					{
						Console.WriteLine(string.Format("Incorrect line masks for the renderer threads (number={0:D})", numberRendererThread));
					}
					allLineMasks |= lineMask;
				}
				if (allLineMasks != unchecked((int)0xFFFFFFFF))
				{
					Console.WriteLine(string.Format("Incorrect line masks for the renderer threads (number={0:D})", numberRendererThread));
				}

				rendererThreadsDone = new Semaphore(0);
			}
			NativeUtils.RendererAsyncRendering = numberRendererThread > 0;
			ScreenScale = sceDisplay.getResizedWidthPow2(1);
			lock (screenScaleLock)
			{
				NativeUtils.ScreenScale = ScreenScale;
			}

			// Used by HD Remaster
			int maxTextureSize = Settings.Instance.readInt("maxTextureSize", 512);
			int maxTextureSizeLog2 = 31 - Integer.numberOfLeadingZeros(maxTextureSize);
			NativeUtils.MaxTextureSizeLog2 = maxTextureSizeLog2;
			bool doubleTexture2DCoords = Settings.Instance.readBool("doubleTexture2DCoords");
			NativeUtils.DoubleTexture2DCoords = doubleTexture2DCoords;
		}

		private static void deactivate()
		{
			drawListQueue = null;

			if (setLogLevelThread != null)
			{
				setLogLevelThread.exit();
				setLogLevelThread = null;
			}

			CoreThread.exit();

			if (rendererThreads != null)
			{
				for (int i = 0; i < rendererThreads.Length; i++)
				{
					rendererThreads[i].exit();
				}
				rendererThreads = null;
			}
		}

		public static void init()
		{
			if (externalGESettingsListerner == null)
			{
				externalGESettingsListerner = new ExternalGESettingsListerner();
				Settings.Instance.registerSettingsListener("ExternalGE", "emu.useExternalSoftwareRenderer", externalGESettingsListerner);
			}

			if (activateWhenAvailable)
			{
				NativeUtils.init();
				if (Available)
				{
					activate();
				}
			}
			else
			{
				deactivate();
			}
		}

		public static void exit()
		{
			if (externalGESettingsListerner != null)
			{
				Settings.Instance.removeSettingsListener("ExternalGE");
				externalGESettingsListerner = null;
			}

			if (Active)
			{
				NativeUtils.exit();
				NativeCallbacks.exit();
				CoreThread.exit();
				setLogLevelThread.exit();
				if (numberRendererThread > 0)
				{
					for (int i = 0; i < rendererThreads.Length; i++)
					{
						rendererThreads[i].exit();
					}
				}
			}
		}

		public static bool Active
		{
			get
			{
				return activateWhenAvailable && Available;
			}
		}

		public static bool Available
		{
			get
			{
				return NativeUtils.Available;
			}
		}

		public static void startList(PspGeList list)
		{
			if (list == null)
			{
				return;
			}

			lock (drawListQueue)
			{
				if (currentList == null)
				{
					if (State.captureGeNextFrame)
					{
						State.captureGeNextFrame = false;
						CaptureManager.captureInProgress = true;
						NativeUtils.DumpFrames = true;
						NativeUtils.DumpTextures = true;
						logLevel = log.Level;
						log.Level = Level.TRACE;
					}

					// Save the context at the beginning of the list processing to the given address (used by sceGu).
					if (list.hasSaveContextAddr())
					{
						saveContext(list.SaveContextAddr);
					}

					list.status = sceGe_user.PSP_GE_LIST_DRAWING;
					NativeUtils.setLogLevel();
					NativeUtils.CoreSadr = list.StallAddr;
					NativeUtils.setCoreCtrlActive();
					lock (screenScaleLock)
					{
						// Update the screen scale only at the start of a new list
						NativeUtils.ScreenScale = ScreenScale;
					}
					currentList = list;
					currentList.sync();
					CoreThread.Instance.sync();
				}
				else
				{
					drawListQueue.add(list);
				}
			}
		}

		private static void addListToHead(PspGeList list)
		{
			lock (drawListQueue)
			{
				// The ConcurrentLinkedQueue type doesn't allow adding
				// objects directly at the head of the queue.

				// This function creates a new array using the given list as it's head
				// and constructs a new ConcurrentLinkedQueue based on it.
				// The actual drawListQueue is then replaced by this new one.
				int arraySize = drawListQueue.size();

				if (arraySize > 0)
				{
					PspGeList[] array = drawListQueue.toArray(new PspGeList[arraySize]);

					ConcurrentLinkedQueue<PspGeList> newQueue = new ConcurrentLinkedQueue<PspGeList>();
					PspGeList[] newArray = new PspGeList[arraySize + 1];

					newArray[0] = list;
					for (int i = 0; i < arraySize; i++)
					{
						newArray[i + 1] = array[i];
						newQueue.add(newArray[i]);
					}

					drawListQueue = newQueue;
				}
				else
				{ // If the queue is empty.
					drawListQueue.add(list);
				}
			}
		}

		public static void startListHead(PspGeList list)
		{
			if (list == null)
			{
				return;
			}

			if (currentList == null)
			{
				startList(list);
	//		} else if (!currentList.isDrawing()) {
	//			if (!drawListQueue.contains(currentList)) {
	//				addListToHead(currentList);
	//			}
	//			currentList = null;
	//			startList(list);
			}
			else
			{
				addListToHead(list);
			}
		}

		public static void onStallAddrUpdated(PspGeList list)
		{
			if (Available && DurationStatistics.collectStatistics)
			{
				NativeUtils.stopEvent(NativeUtils.EVENT_GE_UPDATE_STALL_ADDR);
			}

			if (Active)
			{
				if (list == null)
				{
					return;
				}

				if (list == currentList)
				{
					NativeUtils.CoreSadr = list.StallAddr;
					CoreThread.Instance.sync();
				}
			}
		}

		public static void onRestartList(PspGeList list)
		{
			if (Active)
			{
				if (list == null || list.Finished)
				{
					return;
				}

				lock (drawListQueue)
				{
					if (list == currentList)
					{
						list.status = sceGe_user.PSP_GE_LIST_DRAWING;
						NativeUtils.setCoreCtrlActive();
						CoreThread.Instance.sync();
						list.sync();
					}
				}
			}
		}

		public static void finishList(PspGeList list)
		{
			Modules.sceGe_userModule.hleGeListSyncDone(list);

			lock (drawListQueue)
			{
				if (list == currentList)
				{
					if (CaptureManager.captureInProgress)
					{
						log.Level = logLevel;
						NativeUtils.DumpFrames = false;
						NativeUtils.DumpTextures = false;
						NativeUtils.setLogLevel();
						CaptureManager.captureInProgress = false;
						Emulator.PauseEmu();
					}

					// Restore the context to the state at the beginning of the list processing (used by sceGu).
					if (list.hasSaveContextAddr())
					{
						restoreContext(list.SaveContextAddr);
					}

					currentList = null;
				}
				else
				{
					drawListQueue.remove(list);
				}
			}

			if (currentList == null)
			{
				startList(drawListQueue.poll());
			}
		}

		public static PspGeList LastDrawList
		{
			get
			{
				PspGeList lastList = null;
    
				lock (drawListQueue)
				{
					foreach (PspGeList list in drawListQueue)
					{
						if (list != null)
						{
							lastList = list;
						}
					}
    
					if (lastList == null)
					{
						lastList = currentList;
					}
				}
    
				return lastList;
			}
		}

		public static PspGeList FirstDrawList
		{
			get
			{
				PspGeList firstList;
    
				lock (drawListQueue)
				{
					firstList = currentList;
					if (firstList == null)
					{
						firstList = drawListQueue.peek();
					}
				}
    
				return firstList;
			}
		}

		public static PspGeList CurrentList
		{
			get
			{
				return currentList;
			}
		}

		public static void onGeStartWaitList()
		{
			if (Available && DurationStatistics.collectStatistics)
			{
				NativeUtils.startEvent(NativeUtils.EVENT_GE_WAIT_FOR_LIST);
			}
		}

		public static void onGeStopWaitList()
		{
			if (Available && DurationStatistics.collectStatistics)
			{
				NativeUtils.stopEvent(NativeUtils.EVENT_GE_WAIT_FOR_LIST);
			}
		}

		public static void onDisplayStartWaitVblank()
		{
			if (Available && DurationStatistics.collectStatistics)
			{
				NativeUtils.startEvent(NativeUtils.EVENT_DISPLAY_WAIT_VBLANK);
			}
		}

		public static void onDisplayStopWaitVblank()
		{
			if (Available && DurationStatistics.collectStatistics)
			{
				NativeUtils.stopEvent(NativeUtils.EVENT_DISPLAY_WAIT_VBLANK);
			}
		}

		public static void onDisplayVblank()
		{
			if (Available && DurationStatistics.collectStatistics)
			{
				NativeUtils.notifyEvent(NativeUtils.EVENT_DISPLAY_VBLANK);
			}
		}

		public static void onGeStartList(PspGeList list)
		{
			if (Available && DurationStatistics.collectStatistics)
			{
				NativeUtils.notifyEvent(NativeUtils.EVENT_GE_START_LIST);
			}
		}

		public static void onGeFinishList(PspGeList list)
		{
			if (Available && DurationStatistics.collectStatistics)
			{
				NativeUtils.notifyEvent(NativeUtils.EVENT_GE_FINISH_LIST);
			}
		}

		public static void render()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("ExternalGE starting rendering"));
			}

			for (int i = 0; i < rendererThreads.Length; i++)
			{
				rendererThreads[i].sync(rendererThreadsDone);
			}

			try
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Waiting for async rendering completion"));
				}
				rendererThreadsDone.acquire(rendererThreads.Length);
			}
			catch (InterruptedException e)
			{
				Console.WriteLine("render", e);
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Async rendering completion"));
			}

			NativeUtils.rendererTerminate();

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("ExternalGE terminating rendering"));
			}
		}

		public static int saveContext(int addr)
		{
			if (NativeUtils.CoreCtrlActive)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Saving Core context to 0x{0:X8} - Core busy", addr));
				}
				return -1;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Saving Core context to 0x{0:X8}", addr));
			}

			NativeUtils.saveCoreContext(addr);

			return 0;
		}

		public static int restoreContext(int addr)
		{
			if (NativeUtils.CoreCtrlActive)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Restoring Core context from 0x{0:X8} - Core busy", addr));
				}
				return SceKernelErrors.ERROR_BUSY;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Restoring Core context from 0x{0:X8}", addr));
			}

			NativeUtils.restoreCoreContext(addr);

			return 0;
		}

		public static int getCmd(int cmd)
		{
			return NativeUtils.getCoreCmdArray(cmd);
		}

		public static void setCmd(int cmd, int value)
		{
			NativeUtils.setCoreCmdArray(cmd, value);
		}

		public static void interpretCmd(int cmd, int value)
		{
			NativeUtils.interpretCoreCmd(cmd, value, NativeUtils.CoreMadr);
		}

		private static int getMatrixOffset(int mtxType)
		{
			int offset = mtxType * 12;

			if (mtxType > PSP_GE_MATRIX_PROJECTION)
			{
				// Projection matrix has 4 elements more
				offset += 4;
			}

			return offset;
		}

		private static int getMatrixSize(int mtxType)
		{
			// Only the projection matrix has 16 elements
			return mtxType == PSP_GE_MATRIX_PROJECTION ? 16 : 12;
		}

		public static float[] getMatrix(int mtxType)
		{
			int size = getMatrixSize(mtxType);
			int offset = getMatrixOffset(mtxType);

			float[] mtx = new float[size];
			for (int i = 0; i < size; i++)
			{
				mtx[i] = NativeUtils.getCoreMtxArray(offset + i);
			}

			return mtx;
		}

		public static void setMatrix(int mtxType, int offset, float value)
		{
			NativeUtils.setCoreMtxArray(getMatrixOffset(mtxType) + offset, value);
		}

		public static int ScreenScale
		{
			get
			{
				return screenScale;
			}
			set
			{
				log.info(string.Format("Setting screen scale to factor {0:D}", value));
				ExternalGE.screenScale = value;
			}
		}


		public static ByteBuffer getScaledScreen(int address, int bufferWidth, int height, int pixelFormat)
		{
			lock (screenScaleLock)
			{
				return NativeUtils.getScaledScreen(address, bufferWidth, height, pixelFormat);
			}
		}

		public static void addVideoTexture(int destinationAddress, int sourceAddress, int Length)
		{
			NativeUtils.addVideoTexture(destinationAddress, sourceAddress, Length);
		}

		public static void onGeUserStop()
		{
			lock (drawListQueue)
			{
				drawListQueue.clear();
				if (currentList != null)
				{
					currentList.sync();
				}
				currentList = null;
				CoreThread.Instance.sync();
			}
		}

		public static bool hasDrawList(int listAddr, int stackAddr)
		{
			bool result = false;
			bool waitAndRetry = false;

			lock (drawListQueue)
			{
				if (currentList != null && currentList.isInUse(listAddr, stackAddr))
				{
					result = true;
					// The current list has already reached the FINISH command,
					// but the list processing is not yet completed.
					// Wait a little for the list to complete.
					if (currentList.Finished)
					{
						waitAndRetry = true;
					}
				}
				else
				{
					foreach (PspGeList list in drawListQueue)
					{
						if (list != null && list.isInUse(listAddr, stackAddr))
						{
							result = true;
							break;
						}
					}
				}
			}

			if (waitAndRetry)
			{
				// The current list is already finished but its processing is not yet
				// completed. Wait a little (100ms) and check again to avoid
				// the "can't enqueue duplicate list address" error.
				for (int i = 0; i < 100; i++)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("hasDrawList(0x{0:X8}) waiting on finished list {1}", listAddr, currentList));
					}
					Utilities.sleep(1, 0);
					lock (drawListQueue)
					{
						if (currentList == null || currentList.list_addr != listAddr)
						{
							result = false;
							break;
						}
					}
				}
			}

			return result;
		}

		public static bool isGeAddress(int address)
		{
			return Memory.isVRAM(address);
		}

		public static bool InsideRendering
		{
			get
			{
				if (CoreThread.Instance.InsideRendering)
				{
					return true;
				}
				if (currentList == null)
				{
					return false;
				}
				if (currentList.StallReached)
				{
					return false;
				}
				if (currentList.status == sceGe_user.PSP_GE_LIST_END_REACHED)
				{
					return false;
				}
    
				return true;
			}
		}
	}

}