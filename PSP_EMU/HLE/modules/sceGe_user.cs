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
namespace pspsharp.HLE.modules
{


	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using PspGeList = pspsharp.HLE.kernel.types.PspGeList;
	using SceKernelCallbackInfo = pspsharp.HLE.kernel.types.SceKernelCallbackInfo;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using pspGeCallbackData = pspsharp.HLE.kernel.types.pspGeCallbackData;
	using pspGeListOptParam = pspsharp.HLE.kernel.types.pspGeListOptParam;
	using GeCallbackInterruptHandler = pspsharp.HLE.kernel.types.interrupts.GeCallbackInterruptHandler;
	using GeInterruptHandler = pspsharp.HLE.kernel.types.interrupts.GeInterruptHandler;
	using GeCommands = pspsharp.graphics.GeCommands;
	using VideoEngine = pspsharp.graphics.VideoEngine;
	using ExternalGE = pspsharp.graphics.RE.externalge.ExternalGE;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class sceGe_user : HLEModule
	{
		public static Logger log = Modules.getLogger("sceGe_user");

		public volatile bool waitingForSync;
		public volatile bool syncDone;
		private Dictionary<int, SceKernelCallbackInfo> signalCallbacks;
		private Dictionary<int, SceKernelCallbackInfo> finishCallbacks;
		private const string geCallbackPurpose = "sceGeCallback";

		// PSP has an array of 64 GE lists
		private const int NUMBER_GE_LISTS = 64;
		private PspGeList[] allGeLists;
		private ConcurrentLinkedQueue<PspGeList> listFreeQueue;

		private ConcurrentLinkedQueue<int> deferredThreadWakeupQueue;

		public const int PSP_GE_LIST_DONE = 0;
		public const int PSP_GE_LIST_QUEUED = 1;
		public const int PSP_GE_LIST_DRAWING = 2;
		public const int PSP_GE_LIST_STALL_REACHED = 3;
		public const int PSP_GE_LIST_END_REACHED = 4;
		public const int PSP_GE_LIST_CANCEL_DONE = 5;
		public static readonly string[] PSP_GE_LIST_STRINGS = new string[] {"PSP_GE_LIST_DONE", "PSP_GE_LIST_QUEUED", "PSP_GE_LIST_DRAWING", "PSP_GE_LIST_STALL_REACHED", "PSP_GE_LIST_END_REACHED", "PSP_GE_LIST_CANCEL_DONE"};

		public const int PSP_GE_SIGNAL_HANDLER_SUSPEND = 0x01;
		public const int PSP_GE_SIGNAL_HANDLER_CONTINUE = 0x02;
		public const int PSP_GE_SIGNAL_HANDLER_PAUSE = 0x03;
		public const int PSP_GE_SIGNAL_SYNC = 0x08;
		public const int PSP_GE_SIGNAL_JUMP = 0x10;
		public const int PSP_GE_SIGNAL_CALL = 0x11;
		public const int PSP_GE_SIGNAL_RETURN = 0x12;
		public const int PSP_GE_SIGNAL_TBP0_REL = 0x20;
		public const int PSP_GE_SIGNAL_TBP1_REL = 0x21;
		public const int PSP_GE_SIGNAL_TBP2_REL = 0x22;
		public const int PSP_GE_SIGNAL_TBP3_REL = 0x23;
		public const int PSP_GE_SIGNAL_TBP4_REL = 0x24;
		public const int PSP_GE_SIGNAL_TBP5_REL = 0x25;
		public const int PSP_GE_SIGNAL_TBP6_REL = 0x26;
		public const int PSP_GE_SIGNAL_TBP7_REL = 0x27;
		public const int PSP_GE_SIGNAL_TBP0_REL_OFFSET = 0x28;
		public const int PSP_GE_SIGNAL_TBP1_REL_OFFSET = 0x29;
		public const int PSP_GE_SIGNAL_TBP2_REL_OFFSET = 0x2A;
		public const int PSP_GE_SIGNAL_TBP3_REL_OFFSET = 0x2B;
		public const int PSP_GE_SIGNAL_TBP4_REL_OFFSET = 0x2C;
		public const int PSP_GE_SIGNAL_TBP5_REL_OFFSET = 0x2D;
		public const int PSP_GE_SIGNAL_TBP6_REL_OFFSET = 0x2E;
		public const int PSP_GE_SIGNAL_TBP7_REL_OFFSET = 0x2F;
		public const int PSP_GE_SIGNAL_BREAK = 0xFF;

		public const int PSP_GE_MATRIX_BONE0 = 0;
		public const int PSP_GE_MATRIX_BONE1 = 1;
		public const int PSP_GE_MATRIX_BONE2 = 2;
		public const int PSP_GE_MATRIX_BONE3 = 3;
		public const int PSP_GE_MATRIX_BONE4 = 4;
		public const int PSP_GE_MATRIX_BONE5 = 5;
		public const int PSP_GE_MATRIX_BONE6 = 6;
		public const int PSP_GE_MATRIX_BONE7 = 7;
		public const int PSP_GE_MATRIX_WORLD = 8;
		public const int PSP_GE_MATRIX_VIEW = 9;
		public const int PSP_GE_MATRIX_PROJECTION = 10;
		public const int PSP_GE_MATRIX_TEXGEN = 11;

		public int eDRAMMemoryWidth;

		public override void start()
		{
			log.debug(string.Format("Starting {0}", Name));

			waitingForSync = false;
			syncDone = false;

			signalCallbacks = new Dictionary<int, SceKernelCallbackInfo>();
			finishCallbacks = new Dictionary<int, SceKernelCallbackInfo>();

			listFreeQueue = new ConcurrentLinkedQueue<PspGeList>();
			allGeLists = new PspGeList[NUMBER_GE_LISTS];
			for (int i = 0; i < NUMBER_GE_LISTS; i++)
			{
				allGeLists[i] = new PspGeList(i);
				listFreeQueue.add(allGeLists[i]);
			}

			deferredThreadWakeupQueue = new ConcurrentLinkedQueue<int>();

			eDRAMMemoryWidth = 1024;

			base.start();
		}

		public override void stop()
		{
			log.debug(string.Format("Stopping {0}", Name));

			if (ExternalGE.Active)
			{
				ExternalGE.onGeUserStop();
			}
		}

		public virtual void step()
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;

			for (int? thid = deferredThreadWakeupQueue.poll(); thid != null; thid = deferredThreadWakeupQueue.poll())
			{
				if (log.DebugEnabled)
				{
					log.debug("really waking thread " + thid.ToString("x") + "(" + threadMan.getThreadName(thid.Value) + ")");
				}
				threadMan.hleUnblockThread(thid);

				ExternalGE.onGeStopWaitList();
			}
		}

		private void triggerAsyncCallback(int cbid, int listId, int listPc, int behavior, int signalId, Dictionary<int, SceKernelCallbackInfo> callbacks)
		{
			SceKernelCallbackInfo callback = callbacks[cbid];
			if (callback != null && callback.hasCallbackFunction())
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Scheduling Async Callback {0}, listId=0x{1:X}, listPc=0x{2:X8}, behavior={3:D}, signalId=0x{4:X}", callback.ToString(), listId, listPc, behavior, signalId));
				}
				GeCallbackInterruptHandler geCallbackInterruptHandler = new GeCallbackInterruptHandler(callback.CallbackFunction, callback.CallbackArgument, listPc);
				GeInterruptHandler geInterruptHandler = new GeInterruptHandler(geCallbackInterruptHandler, listId, behavior, signalId);
				Emulator.Scheduler.addAction(geInterruptHandler);
			}
			else
			{
				hleGeOnAfterCallback(listId, behavior, false);
			}
		}

		private void blockCurrentThreadOnList(PspGeList list, IAction action)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;

			bool blockCurrentThread = false;
			bool executeAction = false;

			lock (this)
			{
				int currentThreadId = threadMan.CurrentThreadID;
				if (list.Done)
				{
					// There has been some race condition: the list has just completed
					// do not block the thread
					if (log.DebugEnabled)
					{
						log.debug("blockCurrentThreadOnList not blocking thread " + currentThreadId.ToString("x") + ", list completed " + list);
					}
					executeAction = true;
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug("blockCurrentThreadOnList blocking thread " + currentThreadId.ToString("x") + " on list " + list);
					}
					list.blockedThreadIds.Add(currentThreadId);
					blockCurrentThread = true;
				}
			}

			// Execute the action outside of the synchronized block
			if (executeAction && action != null)
			{
				action.execute();
			}

			// Block the thread outside of the synchronized block
			if (blockCurrentThread)
			{
				// Block the thread, but do not execute callbacks.
				threadMan.hleBlockCurrentThread(SceKernelThreadInfo.JPCSP_WAIT_GE_LIST, list.id, false, action, new ListSyncWaitStateChecker(list));

				ExternalGE.onGeStartWaitList();
			}
		}

		// sceGeDrawSync is resetting all the lists having status PSP_GE_LIST_DONE
		private void hleGeAfterDrawSyncAction()
		{
			lock (this)
			{
				for (int i = 0; i < NUMBER_GE_LISTS; i++)
				{
					if (allGeLists[i].status == PSP_GE_LIST_DONE)
					{
						allGeLists[i].reset();
					}
				}
			}
		}

		/// <summary>
		/// Called from VideoEngine </summary>
		public virtual void hleGeListSyncDone(PspGeList list)
		{
			if (log.DebugEnabled)
			{
				string msg = "hleGeListSyncDone list " + list;

				if (list.Done)
				{
					msg += ", done";
				}
				else
				{
					msg += ", NOT done";
				}

				if (list.blockedThreadIds.Count > 0 && list.status != PSP_GE_LIST_END_REACHED)
				{
					msg += ", waking thread";
					foreach (int threadId in list.blockedThreadIds)
					{
						msg += " " + threadId.ToString("x");
					}
				}

				log.debug(msg);
			}

			lock (this)
			{
				if (list.blockedThreadIds.Count > 0 && list.status != PSP_GE_LIST_END_REACHED)
				{
					// things might go wrong if the thread already exists in the queue
					deferredThreadWakeupQueue.addAll(list.blockedThreadIds);
				}

				if (list.Done)
				{
					listFreeQueue.add(list);
				}
			}
		}

		public virtual void hleGeOnAfterCallback(int listId, int behavior, bool hasCallback)
		{
			// (gid15) I could not make any difference between
			//    PSP_GE_BEHAVIOR_CONTINUE and PSP_GE_BEHAVIOR_SUSPEND
			// Both wait for the completion of the callback before continuing
			// the list processing...
			if (behavior == PSP_GE_SIGNAL_HANDLER_CONTINUE || behavior == PSP_GE_SIGNAL_HANDLER_SUSPEND || !hasCallback)
			{
				if (listId >= 0 && listId < NUMBER_GE_LISTS)
				{
					PspGeList list = allGeLists[listId];
					if (log.DebugEnabled)
					{
						log.debug("hleGeOnAfterCallback restarting list " + list);
					}

					list.restartList();
				}
			}
		}

		/// <summary>
		/// safe to call from the Async display thread </summary>
		public virtual void triggerFinishCallback(int cbid, int listId, int listPc, int callbackNotifyArg1)
		{
			triggerAsyncCallback(cbid, listId, listPc, PSP_GE_SIGNAL_HANDLER_SUSPEND, callbackNotifyArg1, finishCallbacks);
		}

		/// <summary>
		/// safe to call from the Async display thread </summary>
		public virtual void triggerSignalCallback(int cbid, int listId, int listPc, int behavior, int callbackNotifyArg1)
		{
			triggerAsyncCallback(cbid, listId, listPc, behavior, callbackNotifyArg1, signalCallbacks);
		}

		public virtual PspGeList getGeList(int id)
		{
			if (id < 0 || id >= NUMBER_GE_LISTS)
			{
				return null;
			}
			return allGeLists[id];
		}

		internal class DeferredCallbackInfo
		{
			public readonly int cbid;
			public readonly int callbackIndex;
			public readonly int listId;
			public readonly int behavior;
			public readonly int callbackNotifyArg1;

			public DeferredCallbackInfo(int cbid, int callbackIndex, int callbackNotifyArg1)
			{
				this.cbid = cbid;
				this.callbackIndex = callbackIndex;
				this.listId = -1;
				this.behavior = PSP_GE_SIGNAL_HANDLER_SUSPEND;
				this.callbackNotifyArg1 = callbackNotifyArg1;
			}

			public DeferredCallbackInfo(int cbid, int callbackIndex, int listId, int behavior, int callbackNotifyArg1)
			{
				this.cbid = cbid;
				this.callbackIndex = callbackIndex;
				this.listId = listId;
				this.behavior = behavior;
				this.callbackNotifyArg1 = callbackNotifyArg1;
			}
		}

		private class HLEAfterDrawSyncAction : IAction
		{
			private readonly sceGe_user outerInstance;

			public HLEAfterDrawSyncAction(sceGe_user outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.hleGeAfterDrawSyncAction();
			}
		}

		private class ListSyncWaitStateChecker : IWaitStateChecker
		{
			internal PspGeList list;

			public ListSyncWaitStateChecker(PspGeList list)
			{
				this.list = list;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				// Continue the wait state until the list is done
				bool contineWait = !list.Done;

				if (!contineWait)
				{
					ExternalGE.onGeStopWaitList();
				}

				return contineWait;
			}
		}

		public virtual int checkListId(int id)
		{
			if (id < 0 || id >= NUMBER_GE_LISTS)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_ID);
			}

			return id;
		}

		public virtual int checkMode(int mode)
		{
			if (mode < 0 || mode > 1)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_MODE);
			}

			return mode;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public int hleGeListEnQueue(pspsharp.HLE.TPointer listAddr, @CanBeNull pspsharp.HLE.TPointer stallAddr, int cbid, @CanBeNull pspsharp.HLE.TPointer argAddr, int saveContextAddr, boolean enqueueHead)
		public virtual int hleGeListEnQueue(TPointer listAddr, TPointer stallAddr, int cbid, TPointer argAddr, int saveContextAddr, bool enqueueHead)
		{
			pspGeListOptParam optParams = null;
			int stackAddr = 0;
			if (argAddr.NotNull)
			{
				optParams = new pspGeListOptParam();
				optParams.read(argAddr);
				stackAddr = optParams.stackAddr;
				saveContextAddr = optParams.contextAddr;
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleGeListEnQueue optParams={0}", optParams));
				}
			}

			bool useCachedMemory = false;
			if (Modules.SysMemUserForUserModule.hleKernelGetCompiledSdkVersion() >= 0x02000000)
			{
				bool isBusy;
				if (ExternalGE.Active)
				{
					isBusy = ExternalGE.hasDrawList(listAddr.Address, stackAddr);
				}
				else
				{
					isBusy = VideoEngine.Instance.hasDrawList(listAddr.Address, stackAddr);
				}
				if (isBusy)
				{
					log.warn(string.Format("hleGeListEnQueue can't enqueue duplicate list address {0}, stack 0x{1:X8}", listAddr, stackAddr));
					return SceKernelErrors.ERROR_BUSY;
				}
			}
			else
			{
				// Old games (i.e. having PSP SDK version < 2.00) are sometimes
				// reusing the same address for multiple lists, without waiting
				// for the previous list to complete. They assume that the lists
				// are being executed quite quickly, which is not the case when
				// using the OpenGL rendering engine. There is some delay before
				// the OpenGL frame refresh is being processed.
				useCachedMemory = true;
			}

			// No need to cache any memory when using the external software renderer
			if (ExternalGE.Active)
			{
				useCachedMemory = false;
			}

			int result;
			lock (this)
			{
				PspGeList list = listFreeQueue.poll();
				if (list == null)
				{
					log.warn("hleGeListEnQueue no more free list available!");
					if (log.DebugEnabled)
					{
						for (int i = 0; i < NUMBER_GE_LISTS; i++)
						{
							log.debug(string.Format("List#{0:D}: {1}", i, allGeLists[i]));
						}
					}
					return SceKernelErrors.ERROR_OUT_OF_MEMORY;
				}

				list.init(listAddr.Address, stallAddr.Address, cbid, optParams);
				list.SaveContextAddr = saveContextAddr;
				if (useCachedMemory)
				{
					setStallAddressWithCachedMemory(list, stallAddr.Address);
				}
				if (enqueueHead)
				{
					// Send the list to the VideoEngine at the head of the queue.
					list.startListHead();
				}
				else
				{
					// Send the list to the VideoEngine before triggering the display (setting GE dirty)
					list.startList();
				}
				Modules.sceDisplayModule.GeDirty = true;
				result = list.id;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleGeListEnQueue returning 0x{0:X}", result));
			}

			return result;
		}

		public virtual int hleGeListSync(int id)
		{
			if (id < 0 || id >= NUMBER_GE_LISTS)
			{
				return -1;
			}

			PspGeList list = null;
			int result;
			lock (this)
			{
				list = allGeLists[id];
				result = list.status;
			}

			return result;
		}

		private void setStallAddressWithCachedMemory(PspGeList list, int stallAddr)
		{
			int startAddress = list.list_addr;
			int length;
			if (stallAddr != 0)
			{
				length = stallAddr - startAddress;
			}
			else
			{
				// The list has no stall address, scan for the FINISH command
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(startAddress, 4);
				length = 0;
				while (true)
				{
					int instruction = memoryReader.readNext();
					int command = VideoEngine.command(instruction);
					if (command == GeCommands.FINISH)
					{
						// Add 4 to include the END command that follows the FINISH command
						length = memoryReader.CurrentAddress - startAddress + 4;
						break;
					}
				}
			}

			if (length >= 0)
			{
				int[] baseMemoryInts = Utilities.readInt32(startAddress, length);
				list.setStallAddr(stallAddr, MemoryReader.getMemoryReader(startAddress, baseMemoryInts, 0, length), startAddress, startAddress + length);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("setStallAddressWithCachedMemory [0x{0:X8}-0x{1:X8}] {2}", startAddress, startAddress + length, list));
				}
			}
			else
			{
				list.StallAddr = stallAddr;
			}
		}

		[HLEFunction(nid : 0x1F6752AD, version : 150)]
		public virtual int sceGeEdramGetSize()
		{
			return MemoryMap.SIZE_VRAM;
		}

		[HLEFunction(nid : 0xE47E40E4, version : 150)]
		public virtual int sceGeEdramGetAddr()
		{
			return MemoryMap.START_VRAM;
		}

		[HLEFunction(nid : 0xB77905EA, version : 150)]
		public virtual int sceGeEdramSetAddrTranslation(int size)
		{
			// Faking. There's no need for real memory width conversion.
			int previousWidth = eDRAMMemoryWidth;
			eDRAMMemoryWidth = size;

			return previousWidth;
		}

		[HLEFunction(nid : 0xDC93CFEF, version : 150)]
		public virtual int sceGeGetCmd(int cmd)
		{
			VideoEngine ve = VideoEngine.Instance;
			int value;
			if (ExternalGE.Active)
			{
				value = ExternalGE.getCmd(cmd);
			}
			else
			{
				value = ve.getCommandValue(cmd);
			}

			if (log.InfoEnabled)
			{
				log.info(string.Format("sceGeGetCmd {0}: cmd=0x{1:X}, value=0x{2:X6}", ve.commandToString(cmd).ToUpper(), cmd, value));
			}

			return value;
		}

		[HLEFunction(nid : 0x57C8945B, version : 150)]
		public virtual int sceGeGetMtx(int mtxType, TPointer mtxAddr)
		{
			if (mtxType < 0 || mtxType > PSP_GE_MATRIX_TEXGEN)
			{
				log.warn(string.Format("sceGeGetMtx invalid type mtxType={0:D}", mtxType));
				return SceKernelErrors.ERROR_INVALID_INDEX;
			}

			float[] mtx;
			if (ExternalGE.Active)
			{
				mtx = ExternalGE.getMatrix(mtxType);
			}
			else
			{
				mtx = VideoEngine.Instance.getMatrix(mtxType);
			}

			for (int i = 0; i < mtx.Length; i++)
			{
				// Float value is returned in lower 24 bits.
				mtxAddr.setValue32(i << 2, (int)((uint)Float.floatToRawIntBits(mtx[i]) >> 8));
			}

			if (log.InfoEnabled)
			{
				log.info(string.Format("sceGeGetMtx mtxType={0:D}, mtxAddr={1}, mtx={2}", mtxType, mtxAddr, mtx));
			}

			return 0;
		}

		[HLEFunction(nid : 0x438A385A, version : 150)]
		public virtual int sceGeSaveContext(TPointer contextAddr)
		{
			if (ExternalGE.Active)
			{
				return ExternalGE.saveContext(contextAddr.Address);
			}

			VideoEngine.Instance.hleSaveContext(contextAddr.Address);

			return 0;
		}

		[HLEFunction(nid : 0x0BF608FB, version : 150)]
		public virtual int sceGeRestoreContext(TPointer contextAddr)
		{
			if (ExternalGE.Active)
			{
				return ExternalGE.restoreContext(contextAddr.Address);
			}

			VideoEngine.Instance.hleRestoreContext(contextAddr.Address);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xAB49E76A, version = 150) public int sceGeListEnQueue(pspsharp.HLE.TPointer listAddr, @CanBeNull pspsharp.HLE.TPointer stallAddr, int cbid, @CanBeNull pspsharp.HLE.TPointer argAddr)
		[HLEFunction(nid : 0xAB49E76A, version : 150)]
		public virtual int sceGeListEnQueue(TPointer listAddr, TPointer stallAddr, int cbid, TPointer argAddr)
		{
			return hleGeListEnQueue(listAddr, stallAddr, cbid, argAddr, 0, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x1C0D95A6, version = 150) public int sceGeListEnQueueHead(pspsharp.HLE.TPointer listAddr, @CanBeNull pspsharp.HLE.TPointer stallAddr, int cbid, @CanBeNull pspsharp.HLE.TPointer argAddr)
		[HLEFunction(nid : 0x1C0D95A6, version : 150)]
		public virtual int sceGeListEnQueueHead(TPointer listAddr, TPointer stallAddr, int cbid, TPointer argAddr)
		{
			return hleGeListEnQueue(listAddr, stallAddr, cbid, argAddr, 0, true);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x5FB86AB0, version = 150) public int sceGeListDeQueue(@CheckArgument("checkListId") int id)
		[HLEFunction(nid : 0x5FB86AB0, version : 150)]
		public virtual int sceGeListDeQueue(int id)
		{
			lock (this)
			{
				PspGeList list = allGeLists[id];
				list.reset();
				if (!listFreeQueue.contains(list))
				{
					listFreeQueue.add(list);
				}
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE0D68148, version = 150) public int sceGeListUpdateStallAddr(@CheckArgument("checkListId") int id, @CanBeNull pspsharp.HLE.TPointer stallAddr)
		[HLEFunction(nid : 0xE0D68148, version : 150)]
		public virtual int sceGeListUpdateStallAddr(int id, TPointer stallAddr)
		{
			lock (this)
			{
				PspGeList list = allGeLists[id];
				if (list.StallAddr != stallAddr.Address)
				{
					if (list.hasBaseMemoryReader())
					{
						setStallAddressWithCachedMemory(list, stallAddr.Address);
					}
					else
					{
						list.StallAddr = stallAddr.Address;
					}
					Modules.sceDisplayModule.GeDirty = true;
				}
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x03444EB4, version = 150) public int sceGeListSync(@CheckArgument("checkListId") int id, @CheckArgument("checkMode") int mode)
		[HLEFunction(nid : 0x03444EB4, version : 150)]
		public virtual int sceGeListSync(int id, int mode)
		{
			if (mode == 0 && IntrManager.Instance.InsideInterrupt)
			{
				log.debug("sceGeListSync (mode==0) cannot be called inside an interrupt handler!");
				return SceKernelErrors.ERROR_KERNEL_CANNOT_BE_CALLED_FROM_INTERRUPT;
			}

			PspGeList list = null;
			bool blockCurrentThread = false;
			int result;
			lock (this)
			{
				list = allGeLists[id];
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceGeListSync on list: {0}", list));
				}

				if (list.Reset)
				{
					throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_ID);
				}

				if (mode == 0 && !list.Done)
				{
					result = 0;
					blockCurrentThread = true;
				}
				else
				{
					result = list.SyncStatus;
				}
			}

			// Block the current thread outside of the synchronized block
			if (blockCurrentThread)
			{
				blockCurrentThreadOnList(list, null);
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB287BD61, version = 150) public int sceGeDrawSync(@CheckArgument("checkMode") int mode)
		[HLEFunction(nid : 0xB287BD61, version : 150)]
		public virtual int sceGeDrawSync(int mode)
		{
			if (mode == 0 && IntrManager.Instance.InsideInterrupt)
			{
				log.debug("sceGeDrawSync (mode==0) cannot be called inside an interrupt handler!");
				return SceKernelErrors.ERROR_KERNEL_CANNOT_BE_CALLED_FROM_INTERRUPT;
			}

			// no synchronization on "this" required because we are not accessing
			// local data, only list information from the VideoEngine.
			int result = 0;
			if (mode == 0)
			{
				PspGeList lastList;
				if (ExternalGE.Active)
				{
					lastList = ExternalGE.LastDrawList;
				}
				else
				{
					lastList = VideoEngine.Instance.LastDrawList;
				}

				if (lastList != null)
				{
					blockCurrentThreadOnList(lastList, new HLEAfterDrawSyncAction(this));
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug("sceGeDrawSync all lists completed, not waiting");
					}
					hleGeAfterDrawSyncAction();
					Modules.ThreadManForUserModule.hleRescheduleCurrentThread();
				}
			}
			else if (mode == 1)
			{
				PspGeList currentList;
				if (ExternalGE.Active)
				{
					currentList = ExternalGE.FirstDrawList;
				}
				else
				{
					currentList = VideoEngine.Instance.FirstDrawList;
				}
				if (currentList != null)
				{
					result = currentList.SyncStatus;
				}
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceGeDrawSync mode={0:D}, returning {1:D}", mode, result));
				}
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB448EC0D, version = 150) public int sceGeBreak(@CheckArgument("checkMode") int mode, pspsharp.HLE.TPointer brk_addr)
		[HLEFunction(nid : 0xB448EC0D, version : 150)]
		public virtual int sceGeBreak(int mode, TPointer brk_addr)
		{
			int result = 0;

			PspGeList list;
			if (ExternalGE.Active)
			{
				list = ExternalGE.CurrentList;
			}
			else
			{
				list = VideoEngine.Instance.CurrentList;
			}

			if (mode == 0)
			{ // Pause the current list only.
				if (list != null)
				{
					list.pauseList();
					result = list.id;
				}
			}
			else if (mode == 1)
			{ // Pause the current list and cancel the rest of the queue.
				if (list != null)
				{
					list.pauseList();
					for (int i = 0; i < NUMBER_GE_LISTS; i++)
					{
						allGeLists[i].status = PSP_GE_LIST_CANCEL_DONE;
					}
					result = list.id;
				}
			}

			return result;
		}

		[HLEFunction(nid : 0x4C06E472, version : 150)]
		public virtual int sceGeContinue()
		{
			PspGeList list;
			if (ExternalGE.Active)
			{
				list = ExternalGE.CurrentList;
			}
			else
			{
				list = VideoEngine.Instance.CurrentList;
			}

			if (list != null)
			{
				lock (this)
				{
					if (list.status == PSP_GE_LIST_END_REACHED)
					{
						Memory mem = Memory.Instance;
						if (mem.read32(list.Pc) == (GeCommands.FINISH << 24) && mem.read32(list.Pc + 4) == (GeCommands.END << 24))
						{
							list.readNextInstruction();
							list.readNextInstruction();
						}
					}
					list.restartList();
				}
			}

			return 0;
		}

		[HLEFunction(nid : 0xA4FC06A4, version : 150, checkInsideInterrupt : true)]
		public virtual int sceGeSetCallback(TPointer cbdata_addr)
		{
			pspGeCallbackData cbdata = new pspGeCallbackData();
			cbdata.read(cbdata_addr);

			// The cbid returned has a value in the range [0..15].
			int cbid = SceUidManager.getNewId(geCallbackPurpose, 0, 15);
			if (cbid == SceUidManager.INVALID_ID)
			{
				log.warn(string.Format("sceGeSetCallback no more callback ID available"));
				return SceKernelErrors.ERROR_OUT_OF_MEMORY;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceGeSetCallback signalFunc=0x{0:X8}, signalArg=0x{1:X8}, finishFunc=0x{2:X8}, finishArg=0x{3:X8}, result cbid=0x{4:X}", cbdata.signalFunction, cbdata.signalArgument, cbdata.finishFunction, cbdata.finishArgument, cbid));
			}

			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			SceKernelCallbackInfo callbackSignal = threadMan.hleKernelCreateCallback("GeCallbackSignal", cbdata.signalFunction, cbdata.signalArgument);
			SceKernelCallbackInfo callbackFinish = threadMan.hleKernelCreateCallback("GeCallbackFinish", cbdata.finishFunction, cbdata.finishArgument);
			signalCallbacks[cbid] = callbackSignal;
			finishCallbacks[cbid] = callbackFinish;

			return cbid;
		}

		[HLEFunction(nid : 0x05DB22CE, version : 150, checkInsideInterrupt : true)]
		public virtual int sceGeUnsetCallback(int cbid)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			SceKernelCallbackInfo callbackSignal = signalCallbacks.Remove(cbid);
			SceKernelCallbackInfo callbackFinish = finishCallbacks.Remove(cbid);
			if (callbackSignal != null)
			{
				threadMan.hleKernelDeleteCallback(callbackSignal.Uid);
			}
			if (callbackFinish != null)
			{
				threadMan.hleKernelDeleteCallback(callbackFinish.Uid);
			}
			SceUidManager.releaseId(cbid, geCallbackPurpose);

			return 0;
		}

		/// <summary>
		/// Sets the EDRAM size.
		/// </summary>
		/// <param name="size"> The size (0x200000 or 0x400000).
		/// </param>
		/// <returns> Zero on success, otherwise less than zero. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5BAA5439, version = 150) public int sceGeEdramSetSize(int size)
		[HLEFunction(nid : 0x5BAA5439, version : 150)]
		public virtual int sceGeEdramSetSize(int size)
		{
			return 0;
		}

		/// <summary>
		/// Gets the EDRAM physical size.
		/// </summary>
		/// <returns> The EDRAM physical size. </returns>
		[HLEFunction(nid : 0x547EC5F0, version : 660)]
		public virtual int sceGeEdramGetHwSize()
		{
			return MemoryMap.SIZE_VRAM;
		}
	}
}