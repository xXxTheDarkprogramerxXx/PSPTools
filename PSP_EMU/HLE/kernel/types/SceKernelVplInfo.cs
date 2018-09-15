using System;
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
//	import static pspsharp.HLE.kernel.managers.VplManager.PSP_VPL_ATTR_ADDR_HIGH;

	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using ThreadWaitingList = pspsharp.HLE.kernel.managers.ThreadWaitingList;
	using VplManager = pspsharp.HLE.kernel.managers.VplManager;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using Utilities = pspsharp.util.Utilities;

	public class SceKernelVplInfo : pspAbstractMemoryMappedStructureVariableLength
	{
		// PSP info
		public readonly string name;
		public readonly int attr;
		public readonly int poolSize;
		public int freeSize;
		public readonly ThreadWaitingList threadWaitingList;

		public const int vplHeaderSize = 32;
		public const int vplBlockHeaderSize = 8;
		public const int vplAddrAlignment = 7;

		private readonly SysMemInfo sysMemInfo;
		// Internal info
		public readonly int uid;
		public readonly int partitionid;
		private readonly int allocAddress;
		private Dictionary<int, int> dataBlockMap; //Hash map to store each data address and respective size.
		private MemoryChunkList freeMemoryChunks;

		private SceKernelVplInfo(string name, int partitionid, int attr, int size, int memType)
		{
			this.name = name;
			this.attr = attr;

			// Strange, the PSP is allocating a size of 0x1000 when requesting a size lower than 0x30...
			if (size <= 0x30)
			{
				size = 0x1000;
			}

			poolSize = size - vplHeaderSize; // 32 bytes overhead per VPL

			freeSize = poolSize;

			dataBlockMap = new Dictionary<int, int>();

			uid = SceUidManager.getNewUid("ThreadMan-Vpl");
			threadWaitingList = ThreadWaitingList.createThreadWaitingList(SceKernelThreadInfo.PSP_WAIT_VPL, uid, attr, VplManager.PSP_VPL_ATTR_PRIORITY);
			this.partitionid = partitionid;

			// Reserve psp memory
			int totalVplSize = Utilities.alignUp(size, vplAddrAlignment); // 8-byte align
			sysMemInfo = Modules.SysMemUserForUserModule.malloc(partitionid, string.Format("ThreadMan-Vpl-0x{0:x}-{1}", uid, name), memType, totalVplSize, 0);
			if (sysMemInfo == null)
			{
				throw new Exception("SceKernelVplInfo: not enough free mem");
			}
			int addr = sysMemInfo.addr;

			// 24 byte header, probably not necessary to mimick this
			Memory mem = Memory.Instance;
			mem.write32(addr, addr - 1);
			mem.write32(addr + 4, size - 8);
			mem.write32(addr + 8, 0); // based on number of allocations
			mem.write32(addr + 12, addr + size - 16);
			mem.write32(addr + 16, 0); // based on allocations/fragmentation
			mem.write32(addr + 20, 0); // based on created size? magic?

			allocAddress = addr;

			MemoryChunk initialMemoryChunk = new MemoryChunk(addr + vplHeaderSize, totalVplSize - vplHeaderSize);
			freeMemoryChunks = new MemoryChunkList(initialMemoryChunk);
		}

		public static SceKernelVplInfo tryCreateVpl(string name, int partitionid, int attr, int size, int memType)
		{
			SceKernelVplInfo info = null;
			int totalVplSize = Utilities.alignUp(size, vplAddrAlignment); // 8-byte align
			int maxFreeSize = Modules.SysMemUserForUserModule.maxFreeMemSize(partitionid);

			if (totalVplSize <= maxFreeSize)
			{
				info = new SceKernelVplInfo(name, partitionid, attr, totalVplSize, memType);
			}
			else
			{
				VplManager.Console.WriteLine(string.Format("tryCreateVpl not enough free mem (want={0:D} ,free={1:D}, diff={2:D})", totalVplSize, maxFreeSize, totalVplSize - maxFreeSize));
			}

			return info;
		}

		public virtual void delete()
		{
			Modules.SysMemUserForUserModule.free(sysMemInfo);
		}

		protected internal override void write()
		{
			base.write();
			writeStringNZ(32, name);
			write32(attr);
			write32(poolSize);
			write32(freeSize);
			write32(NumWaitingThreads);
		}

		/// <returns> true on success </returns>
		public virtual bool free(int addr)
		{
			if (!dataBlockMap.ContainsKey(addr))
			{
				// Address is not in valid range.
				if (VplManager.log.DebugEnabled)
				{
					VplManager.Console.WriteLine(string.Format("Free VPL 0x{0:X8} address not allocated", addr));
				}

				return false;
			}

			// Check block header.
			Memory mem = Memory.Instance;
			int top = mem.read32(addr - vplBlockHeaderSize);
			if (top != allocAddress)
			{
				VplManager.Console.WriteLine(string.Format("Free VPL 0x{0:X8} corrupted header", addr));
				return false;
			}

			// Recover free size from deallocated block.
			int deallocSize = dataBlockMap.Remove(addr);

			// Free the allocated block
			freeSize += deallocSize;
			MemoryChunk memoryChunk = new MemoryChunk(addr - vplBlockHeaderSize, deallocSize);
			freeMemoryChunks.add(memoryChunk);

			if (VplManager.log.DebugEnabled)
			{
				VplManager.Console.WriteLine(string.Format("Free VPL: Block 0x{0:X8} with size={1:D} freed", addr, deallocSize));
			}

			return true;
		}

		public virtual int alloc(int size)
		{
			int addr = 0;
			int allocSize = Utilities.alignUp(size, vplAddrAlignment) + vplBlockHeaderSize;

			if (allocSize <= freeSize)
			{
				if ((attr & PSP_VPL_ATTR_ADDR_HIGH) == PSP_VPL_ATTR_ADDR_HIGH)
				{
					addr = freeMemoryChunks.allocHigh(allocSize, vplAddrAlignment);
				}
				else
				{
					addr = freeMemoryChunks.allocLow(allocSize, vplAddrAlignment);
				}
				if (addr != 0)
				{
					// 8-byte header per data block.
					Memory mem = Memory.Instance;
					mem.write32(addr, allocAddress);
					mem.write32(addr + 4, 0);
					addr += vplBlockHeaderSize;

					freeSize -= allocSize;

					dataBlockMap[addr] = allocSize;
				}
			}

			return addr;
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
			return string.Format("SceKernelVplInfo[uid=0x{0:X}, name='{1}', attr=0x{2:X}, poolSize=0x{3:X}, freeSize=0x{4:X}, numWaitingThreads={5:D}]", uid, name, attr, poolSize, freeSize, NumWaitingThreads);
		}
	}
}