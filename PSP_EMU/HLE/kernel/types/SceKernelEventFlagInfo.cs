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
	using ThreadWaitingList = pspsharp.HLE.kernel.managers.ThreadWaitingList;
	using ThreadWaitingListFIFO = pspsharp.HLE.kernel.managers.ThreadWaitingListFIFO;

	public class SceKernelEventFlagInfo : pspAbstractMemoryMappedStructureVariableLength
	{
		public readonly string name;
		public readonly int attr;
		public readonly int initPattern;
		public int currentPattern;
		public readonly ThreadWaitingList threadWaitingList;

		public readonly int uid;

		public SceKernelEventFlagInfo(string name, int attr, int initPattern, int currentPattern)
		{
			this.name = name;
			this.attr = attr;
			this.initPattern = initPattern;
			this.currentPattern = currentPattern;

			uid = SceUidManager.getNewUid("ThreadMan-eventflag");
			// It seems that a FIFO list is always used for EventFlags
			threadWaitingList = new ThreadWaitingListFIFO(SceKernelThreadInfo.PSP_WAIT_EVENTFLAG, uid);
		}

		protected internal override void write()
		{
			base.write();
			writeStringNZ(32, name);
			write32(attr);
			write32(initPattern);
			write32(currentPattern);
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
			return string.Format("SceKernelEventFlagInfo(uid=0x{0:X}, name='{1}', attr=0x{2:X}, initPattern=0x{3:X}, currentPattern=0x{4:X}, numWaitThreads={5:D})", uid, name, attr, initPattern, currentPattern, NumWaitThreads);
		}
	}

}