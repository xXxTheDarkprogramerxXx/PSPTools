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

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_High;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_Low;
	using MemoryChunk = pspsharp.HLE.kernel.types.MemoryChunk;
	using MemoryChunkList = pspsharp.HLE.kernel.types.MemoryChunkList;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;

	//using Logger = org.apache.log4j.Logger;

	public class sceHeap : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceHeap");

		protected internal const int PSP_HEAP_ATTR_ADDR_HIGH = 0x4000; // Create the heap in high memory.
		protected internal const int PSP_HEAP_ATTR_EXT = 0x8000; // Automatically extend the heap's memory.
		private Dictionary<int, HeapInfo> heapMap;
		private const int defaultAllocAlignment = 4;

		private class HeapInfo
		{
			internal SysMemInfo sysMemInfo;
			internal MemoryChunkList freeMemoryChunks;
			internal Dictionary<int, MemoryChunk> allocatedMemoryChunks;
			internal int allocType;

			public HeapInfo(SysMemInfo sysMemInfo)
			{
				this.sysMemInfo = sysMemInfo;
				MemoryChunk memoryChunk = new MemoryChunk(sysMemInfo.addr, sysMemInfo.size);
				freeMemoryChunks = new MemoryChunkList(memoryChunk);
				allocatedMemoryChunks = new Dictionary<int, MemoryChunk>();
				allocType = sysMemInfo.type;
			}

			public virtual int alloc(int size, int alignment)
			{
				int allocatedAddr = 0;
				switch (allocType)
				{
					case PSP_SMEM_Low:
						allocatedAddr = freeMemoryChunks.allocLow(size, alignment - 1);
						break;
					case PSP_SMEM_High:
						allocatedAddr = freeMemoryChunks.allocHigh(size, alignment - 1);
						break;
				}

				if (allocatedAddr == 0)
				{
					return 0;
				}

				MemoryChunk memoryChunk = new MemoryChunk(allocatedAddr, size);
				allocatedMemoryChunks[allocatedAddr] = memoryChunk;

				return allocatedAddr;
			}

			public virtual bool free(int addr)
			{
				MemoryChunk memoryChunk = allocatedMemoryChunks.Remove(addr);
				if (memoryChunk == null)
				{
					return false;
				}

				freeMemoryChunks.add(memoryChunk);

				return true;
			}

			public virtual void delete()
			{
				Modules.SysMemUserForUserModule.free(sysMemInfo);
			}

			public override string ToString()
			{
				return string.Format(string.Format("HeapInfo 0x{0:X8}, free=[{1}]", sysMemInfo.addr, freeMemoryChunks));
			}
		}

		public override void start()
		{
			heapMap = new Dictionary<int, sceHeap.HeapInfo>();
			base.start();
		}

		public override void stop()
		{
			foreach (HeapInfo heapInfo in heapMap.Values)
			{
				heapInfo.delete();
			}
			heapMap.Clear();

			base.stop();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0E875980, version = 500, checkInsideInterrupt = true) public int sceHeapReallocHeapMemory(pspsharp.HLE.TPointer heapAddr, pspsharp.HLE.TPointer memAddr, int memSize)
		[HLEFunction(nid : 0x0E875980, version : 500, checkInsideInterrupt : true)]
		public virtual int sceHeapReallocHeapMemory(TPointer heapAddr, TPointer memAddr, int memSize)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1C84B58D, version = 500, checkInsideInterrupt = true) public int sceHeapReallocHeapMemoryWithOption(pspsharp.HLE.TPointer heapAddr, pspsharp.HLE.TPointer memAddr, int memSize, pspsharp.HLE.TPointer paramAddr)
		[HLEFunction(nid : 0x1C84B58D, version : 500, checkInsideInterrupt : true)]
		public virtual int sceHeapReallocHeapMemoryWithOption(TPointer heapAddr, TPointer memAddr, int memSize, TPointer paramAddr)
		{
			return 0;
		}

		[HLEFunction(nid : 0x2ABADC63, version : 500, checkInsideInterrupt : true)]
		public virtual int sceHeapFreeHeapMemory(TPointer heapAddr, TPointer memAddr)
		{
			// Try to free memory back to the heap.
			HeapInfo heapInfo = heapMap[heapAddr.Address];
			if (heapInfo == null)
			{
				return SceKernelErrors.ERROR_INVALID_ID;
			}

			if (!heapInfo.free(memAddr.Address))
			{
				return SceKernelErrors.ERROR_INVALID_POINTER;
			}
			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceHeapFreeHeapMemory after free: {0}", heapInfo));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2A0C2009, version = 500, checkInsideInterrupt = true) public int sceHeapGetMallinfo(pspsharp.HLE.TPointer heapAddr, pspsharp.HLE.TPointer infoAddr)
		[HLEFunction(nid : 0x2A0C2009, version : 500, checkInsideInterrupt : true)]
		public virtual int sceHeapGetMallinfo(TPointer heapAddr, TPointer infoAddr)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x2B7299D8, version = 500, checkInsideInterrupt = true) public int sceHeapAllocHeapMemoryWithOption(pspsharp.HLE.TPointer heapAddr, int memSize, @CanBeNull pspsharp.HLE.TPointer32 paramAddr)
		[HLEFunction(nid : 0x2B7299D8, version : 500, checkInsideInterrupt : true)]
		public virtual int sceHeapAllocHeapMemoryWithOption(TPointer heapAddr, int memSize, TPointer32 paramAddr)
		{
			int alignment = defaultAllocAlignment;

			if (paramAddr.NotNull)
			{
				int paramSize = paramAddr.getValue(0);
				if (paramSize == 8)
				{
					alignment = paramAddr.getValue(4);
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("sceHeapAllocHeapMemoryWithOption options: struct size={0:D}, alignment=0x{1:X}", paramSize, alignment));
					}
				}
				else
				{
					Console.WriteLine(string.Format("sceHeapAllocHeapMemoryWithOption option at {0}(size={1:D})", paramAddr, paramSize));
				}
			}

			// Try to allocate memory from the heap and return it's address.
			HeapInfo heapInfo = heapMap[heapAddr.Address];
			if (heapInfo == null)
			{
				return 0;
			}

			int allocatedAddr = heapInfo.alloc(memSize, alignment);
			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceHeapAllocHeapMemoryWithOption returns 0x{0:X8}, after allocation: {1}", allocatedAddr, heapInfo));
			}

			return allocatedAddr;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4929B40D, version = 500, checkInsideInterrupt = true) public int sceHeapGetTotalFreeSize(pspsharp.HLE.TPointer heapAddr)
		[HLEFunction(nid : 0x4929B40D, version : 500, checkInsideInterrupt : true)]
		public virtual int sceHeapGetTotalFreeSize(TPointer heapAddr)
		{
			return 0;
		}

		[HLEFunction(nid : 0x7012BBDD, version : 500, checkInsideInterrupt : true)]
		public virtual int sceHeapIsAllocatedHeapMemory(TPointer heapAddr, TPointer memAddr)
		{
			if (!heapMap.ContainsKey(heapAddr.Address))
			{
				return SceKernelErrors.ERROR_INVALID_ID;
			}

			HeapInfo heapInfo = heapMap[heapAddr.Address];
			if (heapInfo.allocatedMemoryChunks.ContainsKey(memAddr.Address))
			{
				return 1;
			}
			return 0;
		}

		[HLEFunction(nid : 0x70210B73, version : 500, checkInsideInterrupt : true)]
		public virtual int sceHeapDeleteHeap(TPointer heapAddr)
		{
			HeapInfo heapInfo = heapMap.Remove(heapAddr.Address);
			if (heapInfo == null)
			{
				return SceKernelErrors.ERROR_INVALID_ID;
			}

			heapInfo.delete();

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7DE281C2, version = 500, checkInsideInterrupt = true) public int sceHeapCreateHeap(pspsharp.HLE.PspString name, int heapSize, int attr, @CanBeNull pspsharp.HLE.TPointer paramAddr)
		[HLEFunction(nid : 0x7DE281C2, version : 500, checkInsideInterrupt : true)]
		public virtual int sceHeapCreateHeap(PspString name, int heapSize, int attr, TPointer paramAddr)
		{
			if (paramAddr.NotNull)
			{
				Console.WriteLine(string.Format("sceHeapCreateHeap unknown option at {0}", paramAddr));
			}

			int memType = PSP_SMEM_Low;
			if ((attr & PSP_HEAP_ATTR_ADDR_HIGH) == PSP_HEAP_ATTR_ADDR_HIGH)
			{
				memType = PSP_SMEM_High;
			}

			// Allocate a virtual heap memory space and return it's address.
			SysMemInfo info = null;
			int alignment = 4;
			int totalHeapSize = (heapSize + (alignment - 1)) & (~(alignment - 1));
			int partitionId = SysMemUserForUser.USER_PARTITION_ID;
			int maxFreeSize = Modules.SysMemUserForUserModule.maxFreeMemSize(partitionId);
			if (totalHeapSize <= maxFreeSize)
			{
				info = Modules.SysMemUserForUserModule.malloc(partitionId, name.String, memType, totalHeapSize, 0);
			}
			else
			{
				Console.WriteLine(string.Format("sceHeapCreateHeap not enough free mem (want={0:D}, free={1:D}, diff={2:D})", totalHeapSize, maxFreeSize, totalHeapSize - maxFreeSize));
			}
			if (info == null)
			{
				return 0; // Returns NULL on error.
			}

			HeapInfo heapInfo = new HeapInfo(info);
			heapMap[info.addr] = heapInfo;

			return info.addr;
		}

		[HLEFunction(nid : 0xA8E102A0, version : 500, checkInsideInterrupt : true)]
		public virtual int sceHeapAllocHeapMemory(TPointer heapAddr, int memSize)
		{
			// Try to allocate memory from the heap and return it's address.
			HeapInfo heapInfo = heapMap[heapAddr.Address];
			if (heapInfo == null)
			{
				return 0;
			}

			int allocatedAddr = heapInfo.alloc(memSize, defaultAllocAlignment);
			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceHeapAllocHeapMemory returns 0x{0:X8}, after allocation: {1}", allocatedAddr, heapInfo));
			}

			return allocatedAddr;
		}
	}
}