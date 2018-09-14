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
namespace pspsharp.HLE.kernel.types
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceGe_user.PSP_GE_LIST_CANCEL_DONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceGe_user.PSP_GE_LIST_DONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceGe_user.PSP_GE_LIST_DRAWING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceGe_user.PSP_GE_LIST_QUEUED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceGe_user.PSP_GE_LIST_STALL_REACHED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceGe_user.PSP_GE_LIST_STRINGS;


	using sceGe_user = pspsharp.HLE.modules.sceGe_user;
	using VideoEngine = pspsharp.graphics.VideoEngine;
	using ExternalGE = pspsharp.graphics.RE.externalge.ExternalGE;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;

	public class PspGeList
	{
		private VideoEngine videoEngine;
		private static readonly int pcAddressMask = unchecked((int)0xFFFFFFFC) & Memory.addressMask;
		public int list_addr;
		private int stall_addr;
		public int cbid;
		public pspGeListOptParam optParams;
		private int stackAddr;

		private int pc;

		// a stack entry contains the PC and the baseOffset
		private int[] stack = new int[32 * 2];
		private int stackIndex;

		public int status;
		public int id;

		public IList<int> blockedThreadIds; // the threads we are blocking
		private bool finished;
		private bool paused;
		private bool ended;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool reset_Renamed;
		private bool restarted;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private Semaphore sync_Renamed; // Used for async display
		private IMemoryReader memoryReader;
		private int saveContextAddr;
		private IMemoryReader baseMemoryReader;
		private int baseMemoryReaderStartAddress;
		private int baseMemoryReaderEndAddress;

		public PspGeList(int id)
		{
			videoEngine = VideoEngine.Instance;
			this.id = id;
			blockedThreadIds = new LinkedList<int>();
			reset();
		}

		private void init()
		{
			stackIndex = 0;
			blockedThreadIds.Clear();
			finished = true;
			paused = false;
			reset_Renamed = true;
			ended = true;
			restarted = false;
			memoryReader = null;
			baseMemoryReader = null;
			baseMemoryReaderStartAddress = 0;
			baseMemoryReaderEndAddress = 0;
			pc = 0;
			saveContextAddr = 0;
		}

		public virtual void init(int list_addr, int stall_addr, int cbid, pspGeListOptParam optParams)
		{
			init();

			list_addr &= pcAddressMask;
			stall_addr &= pcAddressMask;

			this.list_addr = list_addr;
			this.stall_addr = stall_addr;
			this.cbid = cbid;
			this.optParams = optParams;

			if (optParams != null)
			{
				stackAddr = optParams.stackAddr;
			}
			else
			{
				stackAddr = 0;
			}
			Pc = list_addr;
			status = (pc == stall_addr) ? PSP_GE_LIST_STALL_REACHED : PSP_GE_LIST_QUEUED;
			finished = false;
			reset_Renamed = false;
			ended = false;

			sync_Renamed = new Semaphore(0);
		}

		public virtual void reset()
		{
			status = PSP_GE_LIST_DONE;
			init();
		}

		public virtual void pushSignalCallback(int listId, int behavior, int signal)
		{
			int listPc = Pc;
			if (!ExternalGE.Active)
			{
				// PC address after the END command
				listPc += 4;
			}
			Modules.sceGe_userModule.triggerSignalCallback(cbid, listId, listPc, behavior, signal);
		}

		public virtual void pushFinishCallback(int listId, int arg)
		{
			int listPc = Pc;
			if (!ExternalGE.Active)
			{
				// PC address after the END command
				listPc += 4;
			}
			Modules.sceGe_userModule.triggerFinishCallback(cbid, listId, listPc, arg);
		}

		private void pushStack(int value)
		{
			stack[stackIndex++] = value;
		}

		private int popStack()
		{
			return stack[--stackIndex];
		}

		public virtual int getAddressRel(int argument)
		{
			return Memory.normalizeAddress((videoEngine.Base | argument));
		}

		public virtual int getAddressRelOffset(int argument)
		{
			return Memory.normalizeAddress((videoEngine.Base | argument) + videoEngine.BaseOffset);
		}

		public virtual bool StackEmpty
		{
			get
			{
				return stackIndex <= 0;
			}
		}

		public virtual int Pc
		{
			set
			{
				value &= pcAddressMask;
				if (this.pc != value)
				{
					int oldPc = this.pc;
					this.pc = value;
					resetMemoryReader(oldPc);
				}
			}
			get
			{
				return pc;
			}
		}


		public virtual void jumpAbsolute(int argument)
		{
			Pc = Memory.normalizeAddress(argument);
		}

		public virtual void jumpRelative(int argument)
		{
			Pc = getAddressRel(argument);
		}

		public virtual void jumpRelativeOffset(int argument)
		{
			Pc = getAddressRelOffset(argument);
		}

		public virtual void callAbsolute(int argument)
		{
			pushStack(pc);
			pushStack(videoEngine.BaseOffset);
			jumpAbsolute(argument);
		}

		public virtual void callRelative(int argument)
		{
			pushStack(pc);
			pushStack(videoEngine.BaseOffset);
			jumpRelative(argument);
		}

		public virtual void callRelativeOffset(int argument)
		{
			pushStack(pc);
			pushStack(videoEngine.BaseOffset);
			jumpRelativeOffset(argument);
		}

		public virtual void ret()
		{
			if (!StackEmpty)
			{
				videoEngine.BaseOffset = popStack();
				Pc = popStack();
			}
		}

		public virtual void sync()
		{
			if (sync_Renamed != null)
			{
				sync_Renamed.release();
			}
		}

		public virtual bool waitForSync(int millis)
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
					sceGe_user.log.debug(string.Format("PspGeList waitForSync {0}", e));
				}
			}

			return true;
		}

		public virtual int StallAddr
		{
			set
			{
				value &= pcAddressMask;
				if (this.stall_addr != value)
				{
					this.stall_addr = value;
					ExternalGE.onStallAddrUpdated(this);
					sync();
				}
			}
			get
			{
				return stall_addr;
			}
		}

		public virtual void setStallAddr(int stall_addr, IMemoryReader baseMemoryReader, int startAddress, int endAddress)
		{
			lock (this)
			{
				// Both the stall address and the base memory reader need to be set at the same
				// time in a synchronized call in order to avoid any race condition
				// with the GUI thread (VideoEngine).
				StallAddr = stall_addr;
        
				this.baseMemoryReader = baseMemoryReader;
				this.baseMemoryReaderStartAddress = startAddress;
				this.baseMemoryReaderEndAddress = endAddress;
				resetMemoryReader(pc);
			}
		}


		public virtual bool StallReached
		{
			get
			{
				return pc == stall_addr && stall_addr != 0;
			}
		}

		public virtual bool hasStallAddr()
		{
			return stall_addr != 0;
		}

		public virtual bool StalledAtStart
		{
			get
			{
				return StallReached && pc == list_addr;
			}
		}

		public virtual void startList()
		{
			paused = false;
			ExternalGE.onGeStartList(this);
			if (ExternalGE.Active)
			{
				ExternalGE.startList(this);
			}
			else
			{
				videoEngine.pushDrawList(this);
			}
			sync();
		}

		public virtual void startListHead()
		{
			paused = false;
			ExternalGE.onGeStartList(this);
			if (ExternalGE.Active)
			{
				ExternalGE.startListHead(this);
			}
			else
			{
				videoEngine.pushDrawListHead(this);
			}
		}

		public virtual void pauseList()
		{
			paused = true;
		}

		public virtual void restartList()
		{
			paused = false;
			restarted = true;
			sync();
			ExternalGE.onRestartList(this);
		}

		public virtual void clearRestart()
		{
			restarted = false;
		}

		public virtual void clearPaused()
		{
			paused = false;
		}

		public virtual bool Restarted
		{
			get
			{
				return restarted;
			}
		}

		public virtual bool Paused
		{
			get
			{
				return paused;
			}
		}

		public virtual bool Finished
		{
			get
			{
				return finished;
			}
		}

		public virtual bool Ended
		{
			get
			{
				return ended;
			}
		}

		public virtual void finishList()
		{
			finished = true;
			ExternalGE.onGeFinishList(this);
		}

		public virtual void endList()
		{
			if (Finished)
			{
				ended = true;
			}
			else
			{
				ended = false;
			}
		}

		public virtual bool Done
		{
			get
			{
				return status == PSP_GE_LIST_DONE || status == PSP_GE_LIST_CANCEL_DONE;
			}
		}

		public virtual bool Reset
		{
			get
			{
				return reset_Renamed;
			}
		}

		public virtual bool Drawing
		{
			get
			{
				return status == PSP_GE_LIST_DRAWING;
			}
		}

		private void resetMemoryReader(int oldPc)
		{
			if (pc >= baseMemoryReaderStartAddress && pc < baseMemoryReaderEndAddress)
			{
				memoryReader = baseMemoryReader;
				memoryReader.skip((pc - baseMemoryReader.CurrentAddress) >> 2);
			}
			else if (memoryReader == null || memoryReader == baseMemoryReader || pc < oldPc)
			{
				memoryReader = MemoryReader.getMemoryReader(pc, 4);
			}
			else if (oldPc < MemoryMap.START_RAM && pc >= MemoryMap.START_RAM)
			{
				// Jumping from VRAM to RAM
				memoryReader = MemoryReader.getMemoryReader(pc, 4);
			}
			else
			{
				memoryReader.skip((pc - oldPc) >> 2);
			}
		}

		public virtual IMemoryReader MemoryReader
		{
			set
			{
				lock (this)
				{
					this.memoryReader = value;
				}
			}
		}

		public virtual bool hasBaseMemoryReader()
		{
			return baseMemoryReader != null;
		}

		public virtual int readNextInstruction()
		{
			lock (this)
			{
				pc += 4;
				return memoryReader.readNext();
			}
		}

		public virtual int readPreviousInstruction()
		{
			lock (this)
			{
				memoryReader.skip(-2);
				int previousInstruction = memoryReader.readNext();
				memoryReader.skip(1);
        
				return previousInstruction;
			}
		}

		public virtual void undoRead()
		{
			lock (this)
			{
				undoRead(1);
			}
		}

		public virtual void undoRead(int n)
		{
			lock (this)
			{
				memoryReader.skip(-n);
			}
		}

		public virtual int SaveContextAddr
		{
			get
			{
				return saveContextAddr;
			}
			set
			{
				this.saveContextAddr = value;
			}
		}


		public virtual bool hasSaveContextAddr()
		{
			return saveContextAddr != 0;
		}

		public virtual bool isInUse(int listAddr, int stackAddr)
		{
			if (list_addr == listAddr)
			{
				return true;
			}
			if (stackAddr != 0 && this.stackAddr == stackAddr)
			{
				return true;
			}

			return false;
		}

		public virtual int SyncStatus
		{
			get
			{
				// Return the status PSP_GE_LIST_STALL_REACHED only when the stall address is reached.
				// I.e. return PSP_GE_LIST_DRAWING when the stall address has been recently updated
				// but the list processing has not yet been resumed and the status is still left
				// at the value PSP_GE_LIST_STALL_REACHED.
				if (status == PSP_GE_LIST_STALL_REACHED)
				{
					if (!StallReached)
					{
						return PSP_GE_LIST_DRAWING;
					}
				}
    
				return status;
			}
		}

		public override string ToString()
		{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("PspGeList[id=0x%X, status=%s, list=0x%08X, pc=0x%08X, stall=0x%08X, cbid=0x%X, ended=%b, finished=%b, paused=%b, restarted=%b, reset=%b]", id, PSP_GE_LIST_STRINGS[status], list_addr, pc, stall_addr, cbid, ended, finished, paused, restarted, reset);
			return string.Format("PspGeList[id=0x%X, status=%s, list=0x%08X, pc=0x%08X, stall=0x%08X, cbid=0x%X, ended=%b, finished=%b, paused=%b, restarted=%b, reset=%b]", id, PSP_GE_LIST_STRINGS[status], list_addr, pc, stall_addr, cbid, ended, finished, paused, restarted, reset_Renamed);
		}
	}
}