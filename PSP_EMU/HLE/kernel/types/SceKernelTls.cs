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
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_High;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_HighAligned;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_Low;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_LowAligned;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.PSP_ATTR_ADDR_HIGH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.alignUp;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;

	/// <summary>
	/// Thread-local storage introduced in PSP 6.20.
	/// 
	/// </summary>
	public class SceKernelTls
	{
		public string name;
		public int attr;
		public int blockSize;
		public int alignedBlockSize;
		public int numberBlocks;
		public int uid;
		private const string uidPurpose = "SceKernelTls";
		private SysMemInfo sysMemInfo;
		private int[] threadIds;

		public SceKernelTls(string name, int partitionId, int attr, int blockSize, int alignedBlockSize, int numberBlocks, int alignment)
		{
			blockSize = alignUp(blockSize, 3);

			this.name = name;
			this.attr = attr;
			this.blockSize = blockSize;
			this.alignedBlockSize = alignedBlockSize;
			this.numberBlocks = numberBlocks;

			int type = alignment == 0 ? PSP_SMEM_Low : PSP_SMEM_LowAligned;
			if ((attr & PSP_ATTR_ADDR_HIGH) != 0)
			{
				type = alignment == 0 ? PSP_SMEM_High : PSP_SMEM_HighAligned;
			}

			int size = alignedBlockSize * numberBlocks;

			sysMemInfo = Modules.SysMemUserForUserModule.malloc(partitionId, name, type, size, alignment);
			uid = SceUidManager.getNewUid(uidPurpose);

			threadIds = new int[numberBlocks];
		}

		public virtual void free()
		{
			Memory.Instance.memset(sysMemInfo.addr, (sbyte) 0, sysMemInfo.allocatedSize);
			Modules.SysMemUserForUserModule.free(sysMemInfo);
			sysMemInfo = null;
			SceUidManager.releaseUid(uid, uidPurpose);
			uid = -1;
		}

		public virtual int BaseAddress
		{
			get
			{
				if (sysMemInfo == null)
				{
					return 0;
				}
    
				return sysMemInfo.addr;
			}
		}

		public virtual void freeTlsAddress()
		{
			if (sysMemInfo == null)
			{
				return;
			}

			int currentThreadId = Modules.ThreadManForUserModule.CurrentThreadID;
			for (int i = 0; i < threadIds.Length; i++)
			{
				if (threadIds[i] == currentThreadId)
				{
					threadIds[i] = 0;
					break;
				}
			}
		}

		public virtual int TlsAddress
		{
			get
			{
				if (sysMemInfo == null)
				{
					return 0;
				}
    
				int currentThreadId = Modules.ThreadManForUserModule.CurrentThreadID;
				int block = -1;
				// If a block has already been allocated for this thread, use it
				for (int i = 0; i < threadIds.Length; i++)
				{
					if (threadIds[i] == currentThreadId)
					{
						block = i;
						break;
					}
				}
    
				bool needsClear = false;
				if (block < 0)
				{
					// Return the first free block
					for (int i = 0; i < threadIds.Length; i++)
					{
						if (threadIds[i] == 0)
						{
							block = i;
							threadIds[block] = currentThreadId;
							needsClear = true;
							break;
						}
					}
    
					if (block < 0)
					{
						return 0;
					}
				}
    
				int address = sysMemInfo.addr + block * alignedBlockSize;
    
				if (needsClear)
				{
					Memory.Instance.memset(address, (sbyte) 0, blockSize);
				}
    
				return address;
			}
		}
	}

}