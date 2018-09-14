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
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using SemaManager = pspsharp.HLE.kernel.managers.SemaManager;
	using ThreadWaitingList = pspsharp.HLE.kernel.managers.ThreadWaitingList;

	public class SceKernelSemaInfo : pspAbstractMemoryMappedStructureVariableLength
	{
		public readonly string name;
		public readonly int attr;
		public readonly int initCount;
		public int currentCount;
		public readonly int maxCount;
		public readonly ThreadWaitingList threadWaitingList;

		public readonly int uid;

		public SceKernelSemaInfo(string name, int attr, int initCount, int maxCount)
		{
			this.name = name;
			this.attr = attr;
			this.initCount = initCount;
			this.currentCount = initCount;
			this.maxCount = maxCount;

			uid = SceUidManager.getNewUid("ThreadMan-sema");
			threadWaitingList = ThreadWaitingList.createThreadWaitingList(SceKernelThreadInfo.PSP_WAIT_SEMA, uid, attr, SemaManager.PSP_SEMA_ATTR_PRIORITY);
		}

		protected internal override void write()
		{
			base.write();
			writeStringNZ(32, name);
			write32(attr);
			write32(initCount);
			write32(currentCount);
			write32(maxCount);
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
			return string.Format("SceKernelSemaInfo(uid=0x{0:X}, name='{1}', attr=0x{2:X}, currentCount={3:D}, maxCount={4:D}, numWaitThreads={5:D})", uid, name, attr, currentCount, maxCount, NumWaitThreads);
		}
	}

}