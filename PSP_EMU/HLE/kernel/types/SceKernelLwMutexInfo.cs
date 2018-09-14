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
	using LwMutexManager = pspsharp.HLE.kernel.managers.LwMutexManager;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using ThreadWaitingList = pspsharp.HLE.kernel.managers.ThreadWaitingList;

	public class SceKernelLwMutexInfo : pspAbstractMemoryMappedStructureVariableLength
	{
		public readonly string name;
		public readonly int attr;
		public readonly int lwMutexUid;
		public readonly TPointer lwMutexOpaqueWorkAreaAddr;
		public readonly int initCount;
		public int lockedCount;
		public readonly ThreadWaitingList threadWaitingList;

		public readonly int uid;
		public int threadid;

		public SceKernelLwMutexInfo(TPointer workArea, string name, int count, int attr)
		{
			this.lwMutexUid = 0;
			this.lwMutexOpaqueWorkAreaAddr = workArea;
			this.name = name;
			this.attr = attr;

			initCount = count;
			lockedCount = count;

			// If the initial count is 0, the lwmutex is not acquired.
			if (count > 0)
			{
				threadid = Modules.ThreadManForUserModule.CurrentThreadID;
			}
			else
			{
				threadid = -1;
			}

			uid = SceUidManager.getNewUid("ThreadMan-LwMutex");
			threadWaitingList = ThreadWaitingList.createThreadWaitingList(SceKernelThreadInfo.PSP_WAIT_LWMUTEX, uid, attr, LwMutexManager.PSP_LWMUTEX_ATTR_PRIORITY);

			lwMutexOpaqueWorkAreaAddr.setValue32(uid);
		}

		protected internal override void write()
		{
			base.write();
			writeStringNZ(32, name);
			write32(attr);
			write32(lwMutexUid);
			write32(lwMutexOpaqueWorkAreaAddr.Address);
			write32(initCount);
			write32(lockedCount);
			write32(threadid);
			write32(NumWaitThreads);
		}

		public virtual int NumWaitThreads
		{
			get
			{
				return threadWaitingList.NumWaitingThreads;
			}
		}

		public override string ToString()
		{
			return string.Format("SceKernelLwMutexInfo(uid=0x{0:X}, name={1}, mutexUid=0x{2:X}, lwMutexOpaqueWorkAreaAddr={3}, initCount={4:D}, lockedCount={5:D}, numWaitThreads={6:D}, attr=0x{7:X}, threadid=0x{8:X})", uid, name, lwMutexUid, lwMutexOpaqueWorkAreaAddr, initCount, lockedCount, NumWaitThreads, attr, threadid);
		}
	}
}