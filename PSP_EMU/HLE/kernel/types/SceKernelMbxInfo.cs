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
//	import static pspsharp.HLE.kernel.types.SceKernelMsgPacket.readNext;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelMsgPacket.writeNext;
	using MbxManager = pspsharp.HLE.kernel.managers.MbxManager;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using ThreadWaitingList = pspsharp.HLE.kernel.managers.ThreadWaitingList;

	public class SceKernelMbxInfo : pspAbstractMemoryMappedStructureVariableLength
	{
		//Mbx info
		public readonly string name;
		public readonly int attr;
		public readonly ThreadWaitingList threadWaitingList;
		private int numMessages;
		private int firstMessageAddr;

		// Internal info
		public readonly int uid;
		public int lastMessageAddr;

		public SceKernelMbxInfo(string name, int attr)
		{
			this.name = name;
			this.attr = attr;

			numMessages = 0;
			firstMessageAddr = 0;
			lastMessageAddr = 0;

			uid = SceUidManager.getNewUid("ThreadMan-Mbx");
			threadWaitingList = ThreadWaitingList.createThreadWaitingList(SceKernelThreadInfo.PSP_WAIT_MBX, uid, attr, MbxManager.PSP_MBX_ATTR_PRIORITY);
		}

		protected internal override void write()
		{
			base.write();
			writeStringNZ(32, name);
			write32(attr);
			write32(NumWaitThreads);
			write32(numMessages);
			write32(firstMessageAddr);
		}

		private void setFirstMessageAddr(Memory mem, int firstMessageAddr)
		{
			this.firstMessageAddr = firstMessageAddr;
			if (firstMessageAddr == 0)
			{
				lastMessageAddr = 0;
			}
			else
			{
				if (lastMessageAddr == 0)
				{
					lastMessageAddr = firstMessageAddr;
				}
				// The last packet is always pointing to the first one (circular list).
				writeNext(mem, lastMessageAddr, firstMessageAddr);
			}
		}

		public virtual int removeMsg(Memory mem)
		{
			int msgAddr = firstMessageAddr;
			if (msgAddr != 0)
			{
				int nextMessageAddr = readNext(mem, msgAddr);
				if (nextMessageAddr == msgAddr)
				{
					setFirstMessageAddr(mem, 0);
				}
				else
				{
					setFirstMessageAddr(mem, nextMessageAddr);
				}
				numMessages--;
			}
			return msgAddr;
		}

		private void insertMsgAfter(Memory mem, int msgAddr, int refMsgAddr)
		{
			if (lastMessageAddr == 0)
			{
				// Insert into an empty queue.
				setFirstMessageAddr(mem, msgAddr);
			}
			else if (refMsgAddr == 0)
			{
				// Insert in front of the queue
				writeNext(mem, msgAddr, firstMessageAddr);
				setFirstMessageAddr(mem, msgAddr);
			}
			else
			{
				// Insert in the middle of the queue
				writeNext(mem, msgAddr, readNext(mem, refMsgAddr));
				writeNext(mem, refMsgAddr, msgAddr);

				if (lastMessageAddr == refMsgAddr)
				{
					// Insert at the end of the queue
					lastMessageAddr = msgAddr;
				}
			}

			numMessages++;
		}

		public virtual void addMsg(Memory mem, int msgAddr)
		{
			if (msgAddr != 0)
			{
				insertMsgAfter(mem, msgAddr, lastMessageAddr);
			}
		}

		public virtual void addMsgByPriority(Memory mem, int msgAddr)
		{
			if (msgAddr != 0)
			{
				int currentMsgAddr = firstMessageAddr;
				int previousMsgAddr = 0;
				for (int i = 0; i < numMessages; i++)
				{
					if (SceKernelMsgPacket.compare(mem, msgAddr, currentMsgAddr) < 0)
					{
						break;
					}
					previousMsgAddr = currentMsgAddr;
					currentMsgAddr = readNext(mem, currentMsgAddr);
				}
				insertMsgAfter(mem, msgAddr, previousMsgAddr);
			}
		}

		public virtual bool hasMessage()
		{
			return firstMessageAddr != 0;
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
			return string.Format("SceKernelMbxInfo[uid=0x{0:X}, name='{1}', attr=0x{2:X}, numWaitingThreads={3:D}, numMessages={4:D}]", uid, name, attr, NumWaitThreads, numMessages);
		}
	}
}