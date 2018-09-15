using System;

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
	using FplManager = pspsharp.HLE.kernel.managers.FplManager;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using ThreadWaitingList = pspsharp.HLE.kernel.managers.ThreadWaitingList;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using Utilities = pspsharp.util.Utilities;

	public class SceKernelFplInfo : pspAbstractMemoryMappedStructureVariableLength
	{
		// PSP info
		public readonly string name;
		public readonly int attr;
		public readonly int blockSize;
		public readonly int numBlocks;
		public int freeBlocks;
		public readonly ThreadWaitingList threadWaitingList;

		private readonly SysMemInfo sysMemInfo;
		// Internal info
		public readonly int uid;
		public readonly int partitionid;
		public int[] blockAddress;
		public bool[] blockAllocated;

		/// <summary>
		/// do not instantiate unless there is enough free mem.
		/// use the static helper function tryCreateFpl. 
		/// </summary>
		private SceKernelFplInfo(string name, int partitionid, int attr, int blockSize, int numBlocks, int memType, int memAlign)
		{
			this.name = name;
			this.attr = attr;
			this.blockSize = blockSize;
			this.numBlocks = numBlocks;

			freeBlocks = numBlocks;

			uid = SceUidManager.getNewUid("ThreadMan-Fpl");
			this.partitionid = partitionid;
			blockAddress = new int[numBlocks];
			blockAllocated = new bool[numBlocks];
			for (int i = 0; i < numBlocks; i++)
			{
				blockAllocated[i] = false;
			}

			// Reserve psp memory
			int alignedBlockSize = memAlign == 0 ? blockSize : Utilities.alignUp(blockSize, memAlign - 1);
			int totalFplSize = alignedBlockSize * numBlocks;
			sysMemInfo = Modules.SysMemUserForUserModule.malloc(partitionid, string.Format("ThreadMan-Fpl-0x{0:x}-{1}", uid, name), memType, totalFplSize, 0);
			if (sysMemInfo == null)
			{
				throw new Exception("SceKernelFplInfo: not enough free mem");
			}

			// Initialise the block addresses
			for (int i = 0; i < numBlocks; i++)
			{
				blockAddress[i] = sysMemInfo.addr + alignedBlockSize * i;
			}

			threadWaitingList = ThreadWaitingList.createThreadWaitingList(SceKernelThreadInfo.PSP_WAIT_FPL, uid, attr, FplManager.PSP_FPL_ATTR_PRIORITY);
		}

		public static SceKernelFplInfo tryCreateFpl(string name, int partitionid, int attr, int blockSize, int numBlocks, int memType, int memAlign)
		{
			SceKernelFplInfo info = null;
			int alignedBlockSize = memAlign == 0 ? blockSize : Utilities.alignUp(blockSize, memAlign - 1);
			int totalFplSize = alignedBlockSize * numBlocks;
			int maxFreeSize = Modules.SysMemUserForUserModule.maxFreeMemSize(partitionid);

			if (totalFplSize <= maxFreeSize)
			{
				info = new SceKernelFplInfo(name, partitionid, attr, blockSize, numBlocks, memType, memAlign);
			}
			else
			{
				Modules.Console.WriteLine("tryCreateFpl not enough free mem (want=" + totalFplSize + ", free=" + maxFreeSize + ", diff=" + (totalFplSize - maxFreeSize) + ")");
			}

			return info;
		}

		protected internal override void write()
		{
			base.write();
			writeStringNZ(32, name);
			write32(attr);
			write32(blockSize);
			write32(numBlocks);
			write32(freeBlocks);
			write32(NumWaitThreads);
		}

		public virtual bool isBlockAllocated(int blockId)
		{
			return blockAllocated[blockId];
		}

		public virtual void freeBlock(int blockId)
		{
			if (!isBlockAllocated(blockId))
			{
				throw new System.ArgumentException("Block " + blockId + " is not allocated");
			}

			blockAllocated[blockId] = false;
			freeBlocks++;
		}

		/// <returns> the address of the allocated block </returns>
		public virtual int allocateBlock(int blockId)
		{
			if (isBlockAllocated(blockId))
			{
				throw new System.ArgumentException("Block " + blockId + " is already allocated");
			}

			blockAllocated[blockId] = true;
			freeBlocks--;

			return blockAddress[blockId];
		}

		/// <returns> the block index or -1 on failure </returns>
		public virtual int findFreeBlock()
		{
			for (int i = 0; i < numBlocks; i++)
			{
				if (!isBlockAllocated(i))
				{
					return i;
				}
			}
			return -1;
		}

		/// <returns> the block index or -1 on failure </returns>
		public virtual int findBlockByAddress(int addr)
		{
			for (int i = 0; i < numBlocks; i++)
			{
				if (blockAddress[i] == addr)
				{
					return i;
				}
			}
			return -1;
		}

		public virtual void deleteSysMemInfo()
		{
			Modules.SysMemUserForUserModule.free(sysMemInfo);
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
			return string.Format("SceKernelFplInfo[uid=0x{0:X}, name='{1}', attr=0x{2:X}, blockSize=0x{3:X}, numBlocks=0x{4:X}, freeBlocks=0x{5:X}, numWaitThreads={6:D}]", uid, name, attr, blockSize, numBlocks, freeBlocks, NumWaitThreads);
		}
	}
}