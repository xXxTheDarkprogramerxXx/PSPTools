using System;
using System.Collections.Generic;
using System.Text;

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
//	import static pspsharp.Allegrex.Common._a1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._t3;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSuspendForUser.KERNEL_VOLATILE_MEM_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSuspendForUser.KERNEL_VOLATILE_MEM_START;

	using Usage = pspsharp.HLE.BufferInfo.Usage;


	using CpuState = pspsharp.Allegrex.CpuState;
	using DumpDebugState = pspsharp.Debugger.DumpDebugState;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using MemoryChunk = pspsharp.HLE.kernel.types.MemoryChunk;
	using MemoryChunkList = pspsharp.HLE.kernel.types.MemoryChunkList;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	/*
	 * TODO list:
	 * 1. Use the partitionid in functions that use it as a parameter.
	 *  -> Info:
	 *      1 = kernel, 2 = user, 3 = me, 4 = kernel mirror (from potemkin/dash)
	 *      http://forums.ps2dev.org/viewtopic.php?p=75341#75341
	 *      8 = slim, topaddr = 0x8A000000, size = 0x1C00000 (28 MB), attr = 0x0C
	 *      8 = slim, topaddr = 0x8BC00000, size = 0x400000 (4 MB), attr = 0x0C
	 *
	 * 2. Implement format string parsing and reading variable number of parameters
	 * in sceKernelPrintf.
	 */
	public class SysMemUserForUser : HLEModule
	{
		public static Logger log = Modules.getLogger("SysMemUserForUser");
		protected internal static Logger stdout = Logger.getLogger("stdout");
		protected internal static Dictionary<int, SysMemInfo> blockList;
		protected internal static MemoryChunkList[] freeMemoryChunks;
		protected internal int firmwareVersion = 150;
		public const int defaultSizeAlignment = 256;

		// PspSysMemBlockTypes
		public const int PSP_SMEM_Low = 0;
		public const int PSP_SMEM_High = 1;
		public const int PSP_SMEM_Addr = 2;
		public const int PSP_SMEM_LowAligned = 3;
		public const int PSP_SMEM_HighAligned = 4;

		public const int KERNEL_PARTITION_ID = 1;
		public const int USER_PARTITION_ID = 2;
		public const int VSHELL_PARTITION_ID = 5;

		protected internal bool started = false;
		private int compiledSdkVersion;
		private int compilerVersion;

		public override void load()
		{
			reset();

			base.load();
		}

		public override void start()
		{
			if (!started)
			{
				reset();
				started = true;
			}

			compiledSdkVersion = 0;
			compilerVersion = 0;

			base.start();
		}

		public override void stop()
		{
			started = false;

			base.stop();
		}

		private MemoryChunkList createMemoryChunkList(int startAddr, int endAddr)
		{
			startAddr &= Memory.addressMask;
			endAddr &= Memory.addressMask;

			MemoryChunk initialMemory = new MemoryChunk(startAddr, endAddr - startAddr + 1);

			return new MemoryChunkList(initialMemory);
		}

		public virtual void reset()
		{
			reset(false);
		}

		public virtual void reset(bool preserveKernelMemory)
		{
			if (blockList == null || freeMemoryChunks == null)
			{
				preserveKernelMemory = false;
			}

			if (preserveKernelMemory)
			{
				IList<SysMemInfo> toBeFreed = new LinkedList<SysMemInfo>();
				foreach (SysMemInfo sysMemInfo in blockList.Values)
				{
					if (sysMemInfo.partitionid == USER_PARTITION_ID)
					{
						toBeFreed.Add(sysMemInfo);
					}
				}

				foreach (SysMemInfo sysMemInfo in toBeFreed)
				{
					sysMemInfo.free();
				}
			}
			else
			{
				blockList = new Dictionary<int, SysMemInfo>();
			}

			if (!preserveKernelMemory)
			{
				// free memory chunks for each partition
				freeMemoryChunks = new MemoryChunkList[6];
				freeMemoryChunks[KERNEL_PARTITION_ID] = createMemoryChunkList(MemoryMap.START_KERNEL, KERNEL_VOLATILE_MEM_START - 1);
				freeMemoryChunks[VSHELL_PARTITION_ID] = createMemoryChunkList(KERNEL_VOLATILE_MEM_START, KERNEL_VOLATILE_MEM_START + KERNEL_VOLATILE_MEM_SIZE - 1);
			}
			freeMemoryChunks[USER_PARTITION_ID] = createMemoryChunkList(MemoryMap.START_USERSPACE, MemoryMap.END_USERSPACE);
		}

		public virtual bool Memory64MB
		{
			set
			{
				if (value)
				{
					MemorySize = MemoryMap.END_RAM_64MB - MemoryMap.START_RAM + 1; // 60 MB
				}
				else
				{
					MemorySize = MemoryMap.END_RAM_32MB - MemoryMap.START_RAM + 1; // 32 MB
				}
			}
		}

		public virtual int MemorySize
		{
			set
			{
				if (MemoryMap.SIZE_RAM != value)
				{
					int previousMemorySize = MemoryMap.SIZE_RAM;
					MemoryMap.END_RAM = MemoryMap.START_RAM + value - 1;
					MemoryMap.END_USERSPACE = MemoryMap.END_RAM;
					MemoryMap.SIZE_RAM = MemoryMap.END_RAM - MemoryMap.START_RAM + 1;
    
					int kernelSize32 = (MemoryMap.END_KERNEL - MemoryMap.START_KERNEL + 1) >> 2;
					int[] savedKernelMemory = new int[kernelSize32];
					IMemoryReader memoryReader = MemoryReader.getMemoryReader(MemoryMap.START_KERNEL, 4);
					for (int i = 0; i < kernelSize32; i++)
					{
						savedKernelMemory[i] = memoryReader.readNext();
					}
    
					if (!Memory.Instance.allocate())
					{
						log.error(string.Format("Failed to resize the PSP memory from 0x{0:X} to 0x{1:X}", previousMemorySize, value));
						Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_MEM_ANY);
					}
    
					IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(MemoryMap.START_KERNEL, 4);
					for (int i = 0; i < kernelSize32; i++)
					{
						memoryWriter.writeNext(savedKernelMemory[i]);
					}
    
					reset(true);
				}
			}
		}

		public class SysMemInfo : IComparable<SysMemInfo>
		{

			public readonly int uid;
			public readonly int partitionid;
			public readonly string name;
			public readonly int type;
			public int size;
			public int allocatedSize;
			public int addr;

			public SysMemInfo(int partitionid, string name, int type, int size, int allocatedSize, int addr)
			{
				this.partitionid = partitionid;
				this.name = name;
				this.type = type;
				this.size = size;
				this.allocatedSize = allocatedSize;
				this.addr = addr;

				uid = SceUidManager.getNewUid("SysMem");
				blockList[uid] = this;
			}

			public override string ToString()
			{
				return string.Format("SysMemInfo[addr=0x{0:X8}-0x{1:X8}, uid=0x{2:X}, partition={3:D}, name='{4}', type={5}, size=0x{6:X} (allocated=0x{7:X})]", addr, addr + allocatedSize, uid, partitionid, name, getTypeName(type), size, allocatedSize);
			}

			public virtual void free()
			{
				blockList.Remove(uid);
			}

			public virtual int CompareTo(SysMemInfo o)
			{
				if (addr == o.addr)
				{
					log.warn("Set invariant broken for SysMemInfo " + this);
					return 0;
				}
				return addr < o.addr ? -1 : 1;
			}
		}

		protected internal static string getTypeName(int type)
		{
			string typeName;

			switch (type)
			{
				case PSP_SMEM_Low:
					typeName = "PSP_SMEM_Low";
					break;
				case PSP_SMEM_High:
					typeName = "PSP_SMEM_High";
					break;
				case PSP_SMEM_Addr:
					typeName = "PSP_SMEM_Addr";
					break;
				case PSP_SMEM_LowAligned:
					typeName = "PSP_SMEM_LowAligned";
					break;
				case PSP_SMEM_HighAligned:
					typeName = "PSP_SMEM_HighAligned";
					break;
				default:
					typeName = "UNHANDLED " + type;
					break;
			}

			return typeName;
		}

		private bool isValidPartitionId(int partitionid)
		{
			return partitionid >= 0 && partitionid < freeMemoryChunks.Length && freeMemoryChunks[partitionid] != null;
		}

		// Allocates to 256-byte alignment
		public virtual SysMemInfo malloc(int partitionid, string name, int type, int size, int addr)
		{
			if (freeMemoryChunks == null)
			{
				return null;
			}

			int allocatedAddress = 0;
			int allocatedSize = 0;

			if (isValidPartitionId(partitionid))
			{
				MemoryChunkList freeMemoryChunk = freeMemoryChunks[partitionid];
				int alignment = defaultSizeAlignment - 1;

				// The allocated size has not to be aligned to the requested alignment
				// (for PSP_SMEM_LowAligned or PSP_SMEM_HighAligned),
				// it is only aligned to the default size alignment.
				allocatedSize = Utilities.alignUp(size, alignment);

				if (type == PSP_SMEM_LowAligned || type == PSP_SMEM_HighAligned)
				{
					// Use the alignment provided in the addr parameter
					alignment = addr - 1;
				}

				switch (type)
				{
					case PSP_SMEM_Low:
					case PSP_SMEM_LowAligned:
						allocatedAddress = freeMemoryChunk.allocLow(allocatedSize, alignment);
						break;
					case PSP_SMEM_High:
					case PSP_SMEM_HighAligned:
						allocatedAddress = freeMemoryChunk.allocHigh(allocatedSize, alignment);
						break;
					case PSP_SMEM_Addr:
						allocatedAddress = freeMemoryChunk.alloc(addr & Memory.addressMask, allocatedSize);
						break;
					default:
						log.warn(string.Format("malloc: unknown type {0}", getTypeName(type)));
					break;
				}
			}

			SysMemInfo sysMemInfo;
			if (allocatedAddress == 0)
			{
				log.warn(string.Format("malloc cannot allocate partition={0:D}, name='{1}', type={2}, size=0x{3:X}, addr=0x{4:X8}, maxFreeMem=0x{5:X}, totalFreeMem=0x{6:X}", partitionid, name, getTypeName(type), size, addr, maxFreeMemSize(partitionid), totalFreeMemSize(partitionid)));
				if (log.TraceEnabled)
				{
					log.trace("Free list: " + DebugFreeMem);
					log.trace("Allocated blocks:\n" + DebugAllocatedMem + "\n");
				}
				sysMemInfo = null;
			}
			else
			{
				sysMemInfo = new SysMemInfo(partitionid, name, type, size, allocatedSize, allocatedAddress);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("malloc partition={0:D}, name='{1}', type={2}, size=0x{3:X}, addr=0x{4:X8}: returns 0x{5:X8}", partitionid, name, getTypeName(type), size, addr, allocatedAddress));
					if (log.TraceEnabled)
					{
						log.trace("Free list after malloc: " + DebugFreeMem);
						log.trace("Allocated blocks after malloc:\n" + DebugAllocatedMem + "\n");
					}
				}
			}

			return sysMemInfo;
		}

		public virtual string DebugFreeMem
		{
			get
			{
				return freeMemoryChunks[USER_PARTITION_ID].ToString();
			}
		}

		public virtual string DebugAllocatedMem
		{
			get
			{
				StringBuilder result = new StringBuilder();
    
				// Sort allocated blocks by address
				IList<SysMemInfo> sortedBlockList = Collections.list(Collections.enumeration(blockList.Values));
				sortedBlockList.Sort();
    
				foreach (SysMemInfo sysMemInfo in sortedBlockList)
				{
					if (result.Length > 0)
					{
						result.Append("\n");
					}
					result.Append(sysMemInfo.ToString());
				}
    
				return result.ToString();
			}
		}

		private void free(int partitionId, int addr, int size)
		{
			MemoryChunk memoryChunk = new MemoryChunk(addr, size);
			freeMemoryChunks[partitionId].add(memoryChunk);
		}

		private int alloc(int partitionId, int addr, int size)
		{
			return freeMemoryChunks[partitionId].alloc(addr, size);
		}

		public virtual void free(SysMemInfo info)
		{
			if (info != null)
			{
				info.free();
				free(info.partitionid, info.addr, info.allocatedSize);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("free {0}", info.ToString()));
					if (log.TraceEnabled)
					{
						log.trace("Free list after free: " + DebugFreeMem);
						log.trace("Allocated blocks after free:\n" + DebugAllocatedMem + "\n");
					}
				}
			}
		}

		public virtual int maxFreeMemSize(int partitionid)
		{
			int maxFreeMemSize = 0;
			if (isValidPartitionId(partitionid))
			{
				for (MemoryChunk memoryChunk = freeMemoryChunks[partitionid].LowMemoryChunk; memoryChunk != null; memoryChunk = memoryChunk.next)
				{
					if (memoryChunk.size > maxFreeMemSize)
					{
						maxFreeMemSize = memoryChunk.size;
					}
				}
			}
			return maxFreeMemSize;
		}

		public virtual int totalFreeMemSize(int partitionid)
		{
			int totalFreeMemSize = 0;
			if (isValidPartitionId(partitionid))
			{
				for (MemoryChunk memoryChunk = freeMemoryChunks[partitionid].LowMemoryChunk; memoryChunk != null; memoryChunk = memoryChunk.next)
				{
					totalFreeMemSize += memoryChunk.size;
				}
			}

			return totalFreeMemSize;
		}

		public virtual SysMemInfo getSysMemInfo(int uid)
		{
			return blockList[uid];
		}

		public virtual SysMemInfo getSysMemInfoByAddress(int address)
		{
			foreach (SysMemInfo info in blockList.Values)
			{
				if (address >= info.addr && address < info.addr + info.size)
				{
					return info;
				}
			}

			return null;
		}

		public virtual SysMemInfo separateMemoryBlock(SysMemInfo info, int size)
		{
			int newAddr = info.addr + size;
			int newSize = info.size - size;
			int newAllocatedSize = info.allocatedSize - size;

			// Create a new memory block
			SysMemInfo newSysMemInfo = new SysMemInfo(info.partitionid, info.name, info.type, newSize, newAllocatedSize, newAddr);

			// Resize the previous memory block
			info.size -= newSize;
			info.allocatedSize -= newAllocatedSize;

			return newSysMemInfo;
		}

		public virtual bool resizeMemoryBlock(SysMemInfo info, int leftShift, int rightShift)
		{
			if (rightShift < 0)
			{
				int sizeToFree = -rightShift;
				free(info.partitionid, info.addr + info.allocatedSize - sizeToFree, sizeToFree);
				info.allocatedSize -= sizeToFree;
				info.size -= sizeToFree;
			}
			else if (rightShift > 0)
			{
				int sizeToExtend = rightShift;
				int extendAddr = alloc(info.partitionid, info.addr + info.allocatedSize, sizeToExtend);
				if (extendAddr == 0)
				{
					return false;
				}
				info.allocatedSize += sizeToExtend;
				info.size += sizeToExtend;
			}

			if (leftShift < 0)
			{
				int sizeToFree = -leftShift;
				free(info.partitionid, info.addr, sizeToFree);
				info.addr += sizeToFree;
				info.size -= sizeToFree;
				info.allocatedSize -= sizeToFree;
			}
			else if (leftShift > 0)
			{
				int sizeToExtend = leftShift;
				int extendAddr = alloc(info.partitionid, info.addr - sizeToExtend, sizeToExtend);
				if (extendAddr == 0)
				{
					return false;
				}
				info.addr -= sizeToExtend;
				info.allocatedSize += sizeToExtend;
				info.size += sizeToExtend;
			}

			return true;
		}

		/// <param name="firmwareVersion"> : in this format: ABB, where A = major and B = minor, for example 271 </param>
		public virtual int FirmwareVersion
		{
			set
			{
				this.firmwareVersion = value;
			}
			get
			{
				return firmwareVersion;
			}
		}


		// note: we're only looking at user memory, so 0x08800000 - 0x0A000000
		// this is mainly to make it fit on one console line
		public virtual void dumpSysMemInfo()
		{
			const int MEMORY_SIZE = 0x1800000;
			const int SLOT_COUNT = 64; // 0x60000
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int SLOT_SIZE = MEMORY_SIZE / SLOT_COUNT;
			int SLOT_SIZE = MEMORY_SIZE / SLOT_COUNT; // 0x60000
			bool[] allocated = new bool[SLOT_COUNT];
			bool[] fragmented = new bool[SLOT_COUNT];
			int allocatedSize = 0;
			int fragmentedSize = 0;

			for (IEnumerator<SysMemInfo> it = blockList.Values.GetEnumerator(); it.MoveNext();)
			{
				SysMemInfo info = it.Current;
				for (int i = info.addr; i < info.addr + info.size; i += SLOT_SIZE)
				{
					if (i >= 0x08800000 && i < 0x0A000000)
					{
						allocated[(i - 0x08800000) / SLOT_SIZE] = true;
					}
				}
				allocatedSize += info.size;
			}

			for (MemoryChunk memoryChunk = freeMemoryChunks[USER_PARTITION_ID].LowMemoryChunk; memoryChunk != null; memoryChunk = memoryChunk.next)
			{
				for (int i = memoryChunk.addr; i < memoryChunk.addr + memoryChunk.size; i += SLOT_SIZE)
				{
					if (i >= 0x08800000 && i < 0x0A000000)
					{
						fragmented[(i - 0x08800000) / SLOT_SIZE] = true;
					}
				}
				fragmentedSize += memoryChunk.size;
			}

			StringBuilder allocatedDiagram = new StringBuilder();
			allocatedDiagram.Append("[");
			for (int i = 0; i < SLOT_COUNT; i++)
			{
				allocatedDiagram.Append(allocated[i] ? "X" : " ");
			}
			allocatedDiagram.Append("]");

			StringBuilder fragmentedDiagram = new StringBuilder();
			fragmentedDiagram.Append("[");
			for (int i = 0; i < SLOT_COUNT; i++)
			{
				fragmentedDiagram.Append(fragmented[i] ? "X" : " ");
			}
			fragmentedDiagram.Append("]");

			DumpDebugState.log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
			DumpDebugState.log(string.Format("Allocated memory:  {0:X8} {1:D} bytes", allocatedSize, allocatedSize));
			DumpDebugState.log(allocatedDiagram.ToString());
			DumpDebugState.log(string.Format("Fragmented memory: {0:X8} {1:D} bytes", fragmentedSize, fragmentedSize));
			DumpDebugState.log(fragmentedDiagram.ToString());

			DumpDebugState.log("Free list: " + DebugFreeMem);
			DumpDebugState.log("Allocated blocks:\n" + DebugAllocatedMem + "\n");
		}

		public virtual string hleKernelSprintf(CpuState cpu, string format, object[] formatParameters)
		{
			string formattedMsg = format;
			try
			{
				// Translate the C-like format string to a Java format string:
				// - %u or %i -> %d
				// - %4u -> %4d
				// - %lld or %ld -> %d
				// - %llx or %lx -> %x
				// - %p -> %08X
				string javaMsg = format;
				javaMsg = javaMsg.replaceAll("\\%(\\d*)l?l?[uid]", "%$1d");
				javaMsg = javaMsg.replaceAll("\\%(\\d*)l?l?([xX])", "%$1$2");
				javaMsg = javaMsg.replaceAll("\\%p", "%08X");

				// Support for "%s" (at any place and can occur multiple times)
				int index = -1;
				for (int parameterIndex = 0; parameterIndex < formatParameters.Length; parameterIndex++)
				{
					index = javaMsg.IndexOf('%', index + 1);
					if (index < 0)
					{
						break;
					}
					string parameterFormat = javaMsg.Substring(index);
					if (parameterFormat.StartsWith("%s", StringComparison.Ordinal))
					{
						// Convert an integer address to a String by reading
						// the String at the given address
						int address = ((int?) formatParameters[parameterIndex]).Value;
						if (address == 0)
						{
							formatParameters[parameterIndex] = "(null)";
						}
						else
						{
							formatParameters[parameterIndex] = Utilities.readStringZ(address);
						}
					}
				}

				// String.format: If there are more arguments than format specifiers, the extra arguments are ignored.
				formattedMsg = string.format(javaMsg, formatParameters);
			}
			catch (Exception)
			{
				// Ignore formatting exception
			}

			return formattedMsg;
		}

		public virtual string hleKernelSprintf(CpuState cpu, string format, int firstRegister)
		{
			// For now, use only the 7 register parameters: $a1-$a3, $t0-$t3
			// Further parameters are retrieved from the stack (assume max. 10 stack parameters).
			int registerParameters = _t3 - firstRegister + 1;
			object[] formatParameters = new object[registerParameters + 10];
			for (int i = 0; i < registerParameters; i++)
			{
				formatParameters[i] = cpu.getRegister(firstRegister + i);
			}
			Memory mem = Memory.Instance;
			for (int i = registerParameters; i < formatParameters.Length; i++)
			{
				formatParameters[i] = mem.read32(cpu._sp + ((i - registerParameters) << 2));
			}

			return hleKernelSprintf(cpu, format, formatParameters);
		}

		public virtual int hleKernelPrintf(CpuState cpu, PspString formatString, Logger logger)
		{
			// Format and print the message to the logger
			if (logger.InfoEnabled)
			{
				string formattedMsg = hleKernelSprintf(cpu, formatString.String, _a1);
				logger.info(formattedMsg);
			}

			return 0;
		}

		public virtual int hleKernelGetCompiledSdkVersion()
		{
			return compiledSdkVersion;
		}

		protected internal virtual void hleSetCompiledSdkVersion(int sdkVersion)
		{
			compiledSdkVersion = sdkVersion;
		}

		public virtual int hleKernelGetCompilerVersion()
		{
			return compilerVersion;
		}

		[HLEFunction(nid : 0xA291F107, version : 150)]
		public virtual int sceKernelMaxFreeMemSize()
		{
			int maxFreeMemSize = this.maxFreeMemSize(USER_PARTITION_ID);

			// Some games expect size to be rounded down in 16 bytes block
			maxFreeMemSize &= ~15;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelMaxFreeMemSize returning {0:D}(hex=0x{0:X})", maxFreeMemSize));
			}

			return maxFreeMemSize;
		}

		[HLEFunction(nid : 0xF919F628, version : 150)]
		public virtual int sceKernelTotalFreeMemSize()
		{
			int totalFreeMemSize = this.totalFreeMemSize(USER_PARTITION_ID);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelTotalFreeMemSize returning {0:D}(hex=0x{0:X})", totalFreeMemSize));
			}

			return totalFreeMemSize;
		}

		[HLEFunction(nid : 0x237DBD4F, version : 150)]
		public virtual int sceKernelAllocPartitionMemory(int partitionid, string name, int type, int size, int addr)
		{
			addr &= Memory.addressMask;

			if (type < PSP_SMEM_Low || type > PSP_SMEM_HighAligned)
			{
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_MEMBLOCK_ALLOC_TYPE;
			}

			SysMemInfo info = malloc(partitionid, name, type, size, addr);
			if (info == null)
			{
				return SceKernelErrors.ERROR_KERNEL_FAILED_ALLOC_MEMBLOCK;
			}

			return info.uid;
		}

		[HLEFunction(nid : 0xB6D61D02, version : 150)]
		public virtual int sceKernelFreePartitionMemory(int uid)
		{
			SceUidManager.checkUidPurpose(uid, "SysMem", true);

			SysMemInfo info = blockList.Remove(uid);
			if (info == null)
			{
				log.warn(string.Format("sceKernelFreePartitionMemory unknown uid=0x{0:X}", uid));
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_CHUNK_ID;
			}

			free(info);

			return 0;
		}

		[HLEFunction(nid : 0x9D9A5BA1, version : 150)]
		public virtual int sceKernelGetBlockHeadAddr(int uid)
		{
			SceUidManager.checkUidPurpose(uid, "SysMem", true);

			SysMemInfo info = blockList[uid];
			if (info == null)
			{
				log.warn(string.Format("sceKernelGetBlockHeadAddr unknown uid=0x{0:X}", uid));
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_CHUNK_ID;
			}

			return info.addr;
		}

		[HLEFunction(nid : 0xF12A62F7, version : 660)]
		public virtual int sceKernelGetBlockHeadAddr_660(int uid)
		{
			return sceKernelGetBlockHeadAddr(uid);
		}

		[HLEFunction(nid : 0x13A5ABEF, version : 150)]
		public virtual int sceKernelPrintf(CpuState cpu, PspString formatString)
		{
			return hleKernelPrintf(cpu, formatString, stdout);
		}

		[HLEFunction(nid : 0x3FC9AE6A, version : 150)]
		public virtual int sceKernelDevkitVersion()
		{
			int major = firmwareVersion / 100;
			int minor = (firmwareVersion / 10) % 10;
			int revision = firmwareVersion % 10;
			int devkitVersion = (major << 24) | (minor << 16) | (revision << 8) | 0x10;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelDevkitVersion returning 0x{0:X8}", devkitVersion));
			}

			return devkitVersion;
		}

		[HLEFunction(nid : 0xD8DE5C1E, version : 150)]
		public virtual int SysMemUserForUser_D8DE5C1E()
		{
			// Seems to always return 0...
			return 0;
		}

		[HLEFunction(nid : 0xFC114573, version : 200)]
		public virtual int sceKernelGetCompiledSdkVersion()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceKernelGetCompiledSdkVersion returning 0x{0:X8}", compiledSdkVersion));
			}
			return compiledSdkVersion;
		}

		[HLEFunction(nid : 0x7591C7DB, version : 200)]
		public virtual int sceKernelSetCompiledSdkVersion(int sdkVersion)
		{
			hleSetCompiledSdkVersion(sdkVersion);

			return 0;
		}

		[HLEFunction(nid : 0xF77D77CB, version : 200)]
		public virtual int sceKernelSetCompilerVersion(int compilerVersion)
		{
			this.compilerVersion = compilerVersion;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA6848DF8, version = 200) public int SysMemUserForUser_A6848DF8()
		[HLEFunction(nid : 0xA6848DF8, version : 200)]
		public virtual int SysMemUserForUser_A6848DF8()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x2A3E5280, version = 280) public int sceKernelQueryMemoryInfo(int address, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 partitionId, @CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 memoryBlockId)
		[HLEFunction(nid : 0x2A3E5280, version : 280)]
		public virtual int sceKernelQueryMemoryInfo(int address, TPointer32 partitionId, TPointer32 memoryBlockId)
		{
			int result = SceKernelErrors.ERROR_KERNEL_ILLEGAL_ADDR;

			foreach (int? key in blockList.Keys)
			{
				SysMemInfo info = blockList[key];
				if (info != null && info.addr <= address && address < info.addr + info.size)
				{
					partitionId.setValue(info.partitionid);
					memoryBlockId.setValue(info.uid);
					result = 0;
					break;
				}
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x39F49610, version = 280) public int sceKernelGetPTRIG()
		[HLEFunction(nid : 0x39F49610, version : 280)]
		public virtual int sceKernelGetPTRIG()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6231A71D, version = 280) public int sceKernelSetPTRIG()
		[HLEFunction(nid : 0x6231A71D, version : 280)]
		public virtual int sceKernelSetPTRIG()
		{
			return 0;
		}

		// sceKernelFreeMemoryBlock (internal name)
		[HLEFunction(nid : 0x50F61D8A, version : 352)]
		public virtual int SysMemUserForUser_50F61D8A(int uid)
		{
			SysMemInfo info = blockList.Remove(uid);
			if (info == null)
			{
				log.warn("SysMemUserForUser_50F61D8A(uid=0x" + uid.ToString("x") + ") unknown uid");
				return SceKernelErrors.ERROR_KERNEL_UNKNOWN_UID;
			}

			free(info);

			return 0;
		}

		[HLEFunction(nid : 0xACBD88CA, version : 352)]
		public virtual int sceKernelTotalMemSize()
		{
			return MemoryMap.SIZE_RAM;
		}

		// sceKernelGetMemoryBlockAddr (internal name)
		[HLEFunction(nid : 0xDB83A952, version : 352)]
		public virtual int SysMemUserForUser_DB83A952(int uid, TPointer32 addr)
		{
			SysMemInfo info = blockList[uid];
			if (info == null)
			{
				log.warn(string.Format("SysMemUserForUser_DB83A952 uid=0x{0:X}, addr={1}: unknown uid", uid, addr));
				return SceKernelErrors.ERROR_KERNEL_UNKNOWN_UID;
			}

			addr.setValue(info.addr);

			return 0;
		}

		// sceKernelAllocMemoryBlock (internal name)
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xFE707FDF, version = 352) public int SysMemUserForUser_FE707FDF(@StringInfo(maxLength=32) pspsharp.HLE.PspString name, int type, int size, @CanBeNull pspsharp.HLE.TPointer paramsAddr)
		[HLEFunction(nid : 0xFE707FDF, version : 352)]
		public virtual int SysMemUserForUser_FE707FDF(PspString name, int type, int size, TPointer paramsAddr)
		{
			if (paramsAddr.NotNull)
			{
				int length = paramsAddr.getValue32();
				if (length != 4)
				{
					log.warn(string.Format("SysMemUserForUser_FE707FDF: unknown parameters with length={0:D}", length));
				}
			}

			if (type < PSP_SMEM_Low || type > PSP_SMEM_High)
			{
				return SceKernelErrors.ERROR_KERNEL_ILLEGAL_MEMBLOCK_ALLOC_TYPE;
			}

			// Always allocate memory in user area (partitionid == 2).
			SysMemInfo info = malloc(SysMemUserForUser.USER_PARTITION_ID, name.String, type, size, 0);
			if (info == null)
			{
				return SceKernelErrors.ERROR_KERNEL_FAILED_ALLOC_MEMBLOCK;
			}

			return info.uid;
		}

		[HLEFunction(nid : 0x342061E5, version : 370)]
		public virtual int sceKernelSetCompiledSdkVersion370(int sdkVersion)
		{
			hleSetCompiledSdkVersion(sdkVersion);

			return 0;
		}

		[HLEFunction(nid : 0x315AD3A0, version : 380)]
		public virtual int sceKernelSetCompiledSdkVersion380_390(int sdkVersion)
		{
			hleSetCompiledSdkVersion(sdkVersion);

			return 0;
		}

		[HLEFunction(nid : 0xEBD5C3E6, version : 395)]
		public virtual int sceKernelSetCompiledSdkVersion395(int sdkVersion)
		{
			hleSetCompiledSdkVersion(sdkVersion);

			return 0;
		}

		[HLEFunction(nid : 0x91DE343C, version : 500)]
		public virtual int sceKernelSetCompiledSdkVersion500_505(int sdkVersion)
		{
			hleSetCompiledSdkVersion(sdkVersion);

			return 0;
		}

		[HLEFunction(nid : 0x7893F79A, version : 507)]
		public virtual int sceKernelSetCompiledSdkVersion507(int sdkVersion)
		{
			hleSetCompiledSdkVersion(sdkVersion);

			return 0;
		}

		[HLEFunction(nid : 0x35669D4C, version : 600)]
		public virtual int sceKernelSetCompiledSdkVersion600_602(int sdkVersion)
		{
			hleSetCompiledSdkVersion(sdkVersion);

			return 0;
		}

		[HLEFunction(nid : 0x1B4217BC, version : 603)]
		public virtual int sceKernelSetCompiledSdkVersion603_605(int sdkVersion)
		{
			hleSetCompiledSdkVersion(sdkVersion);

			return 0;
		}

		[HLEFunction(nid : 0x358CA1BB, version : 606)]
		public virtual int sceKernelSetCompiledSdkVersion606(int sdkVersion)
		{
			hleSetCompiledSdkVersion(sdkVersion);

			return 0;
		}

		[HLEFunction(nid : 0xC886B169, version : 150)]
		public virtual int sceKernelDevkitVersion_660()
		{
			return sceKernelDevkitVersion();
		}
	}
}