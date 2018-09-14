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
	using MsgPipeManager = pspsharp.HLE.kernel.managers.MsgPipeManager;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using ThreadWaitingList = pspsharp.HLE.kernel.managers.ThreadWaitingList;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;

	public class SceKernelMppInfo : pspAbstractMemoryMappedStructureVariableLength
	{

		// PSP info
		public readonly string name;
		public readonly int attr;
		public readonly int bufSize;
		public int freeSize;
		public readonly ThreadWaitingList sendThreadWaitingList;
		public readonly ThreadWaitingList receiveThreadWaitingList;

		private readonly SysMemInfo sysMemInfo;
		// Internal info
		public readonly int uid;
		public readonly int partitionid;
		public readonly int address;
		private int head; // relative to address
		private int tail; // relative to address
		private const string uidPurpose = "ThreadMan-MsgPipe";
		public int userAddress;
		public int userSize;

		public SceKernelMppInfo(string name, int partitionid, int attr, int size, int memType)
		{
			this.name = name;
			this.attr = attr;

			bufSize = size;
			freeSize = size;

			if (size != 0)
			{
				sysMemInfo = Modules.SysMemUserForUserModule.malloc(partitionid, "ThreadMan-MsgPipe", memType, size, 0);
				address = sysMemInfo.addr;
			}
			else
			{
				sysMemInfo = null;
				address = 0;
			}

			uid = SceUidManager.getNewUid(uidPurpose);
			sendThreadWaitingList = ThreadWaitingList.createThreadWaitingList(SceKernelThreadInfo.PSP_WAIT_MSGPIPE, uid, attr, MsgPipeManager.PSP_MPP_ATTR_SEND_PRIORITY);
			receiveThreadWaitingList = ThreadWaitingList.createThreadWaitingList(SceKernelThreadInfo.PSP_WAIT_MSGPIPE, uid, attr, MsgPipeManager.PSP_MPP_ATTR_RECEIVE_PRIORITY);
			this.partitionid = partitionid;
			head = 0;
			tail = 0;
		}

		public virtual bool MemoryAllocated
		{
			get
			{
				return bufSize == 0 || sysMemInfo != null;
			}
		}

		public virtual void delete()
		{
			if (sysMemInfo != null)
			{
				Modules.SysMemUserForUserModule.free(sysMemInfo);
			}
			SceUidManager.releaseUid(uid, uidPurpose);
		}

		protected internal override void write()
		{
			base.write();
			writeStringNZ(32, name);
			write32(attr);
			write32(bufSize);
			write32(freeSize);
			write32(NumSendWaitThreads);
			write32(NumReceiveWaitThreads);
		}

		public virtual int availableReadSize()
		{
			if (bufSize == 0)
			{
				return UserSize;
			}
			return bufSize - freeSize;
		}

		public virtual int availableWriteSize()
		{
			return freeSize;
		}

		// this will clobber itself if used carelessly but won't overflow outside of its allocated memory
		public virtual void append(Memory mem, int src, int size)
		{
			int copySize;

			freeSize -= size;

			while (size > 0)
			{
				copySize = System.Math.Min(bufSize - tail, size);
				mem.memcpy(address + tail, src, copySize);
				src += copySize;
				size -= copySize;
				tail = (tail + copySize) % bufSize;
			}
		}

		public virtual void consume(Memory mem, int dst, int size)
		{
			if (bufSize == 0)
			{
				mem.memcpy(dst, userAddress, size);
				userAddress += size;
				userSize -= size;
			}
			else
			{
				freeSize += size;

				while (size > 0)
				{
					int copySize = System.Math.Min(bufSize - head, size);
					mem.memcpy(dst, address + head, copySize);
					dst += copySize;
					size -= copySize;
					head = (head + copySize) % bufSize;
				}
			}
		}

		public virtual int NumSendWaitThreads
		{
			get
			{
				return sendThreadWaitingList.NumWaitingThreads;
			}
		}

		public virtual int NumReceiveWaitThreads
		{
			get
			{
				return receiveThreadWaitingList.NumWaitingThreads;
			}
		}

		public virtual void setUserData(int address, int size)
		{
			userAddress = address;
			userSize = size;
		}

		public virtual int UserSize
		{
			get
			{
				return userSize;
			}
		}

		public override string ToString()
		{
			return string.Format("SceKernelMppInfo(uid=0x{0:X}, name='{1}', attr=0x{2:X}, bufSize=0x{3:X}, freeSize=0x{4:X}, numSendWaitThreads={5:D}, numReceiveWaitThreads={6:D})", uid, name, attr, bufSize, freeSize, NumSendWaitThreads, NumReceiveWaitThreads);
		}
	}
}