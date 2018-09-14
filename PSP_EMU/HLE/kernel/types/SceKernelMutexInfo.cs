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
	using MutexManager = pspsharp.HLE.kernel.managers.MutexManager;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using ThreadWaitingList = pspsharp.HLE.kernel.managers.ThreadWaitingList;

	public class SceKernelMutexInfo : pspAbstractMemoryMappedStructureVariableLength
	{
		public readonly string name;
		public readonly int attr;
		public readonly int initCount;
		public int lockedCount;
		public readonly ThreadWaitingList threadWaitingList;

		public readonly int uid;
		public int threadid;

		public SceKernelMutexInfo(string name, int count, int attr)
		{
			this.name = name;
			this.attr = attr;

			initCount = count;
			lockedCount = count;

			// If the initial count is 0, the mutex is not acquired.
			if (count > 0)
			{
				threadid = Modules.ThreadManForUserModule.CurrentThreadID;
			}
			else
			{
				threadid = -1;
			}

			uid = SceUidManager.getNewUid("ThreadMan-Mutex");
			threadWaitingList = ThreadWaitingList.createThreadWaitingList(SceKernelThreadInfo.PSP_WAIT_MUTEX, uid, attr, MutexManager.PSP_MUTEX_ATTR_PRIORITY);
		}

		protected internal override void write()
		{
			base.write();
			writeStringNZ(32, name);
			write32(attr);
			write32(initCount);
			write32(lockedCount);
			write32(threadid);
			write32(NumWaitingThreads);
		}

		public virtual int NumWaitingThreads
		{
			get
			{
				return threadWaitingList.NumWaitingThreads;
			}
		}

		public override string ToString()
		{
			return string.Format("SceKernelMutexInfo(uid=0x{0:X}, name='{1}', attr=0x{2:X}, initCount={3:D}, lockedCount={4:D}, numWaitThreads={5:D})", uid, name, attr, initCount, lockedCount, NumWaitingThreads);
		}
	}
}