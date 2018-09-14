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

	/// <summary>
	/// This structure represents executable file information used to load the file.
	/// </summary>
	public class SceLoadCoreExecFileInfo : pspAbstractMemoryMappedStructure
	{
		/// <summary>
		/// The maximum number of segments a module can have. </summary>
		public const int SCE_KERNEL_MAX_MODULE_SEGMENT = 4;
		public int unknown0; //0
		/// <summary>
		/// The mode attribute of the executable file. One of ::SceExecFileModeAttr. </summary>
		public int modeAttribute; //4
		/// <summary>
		/// The API type. </summary>
		public int apiType; //8
		/// <summary>
		/// Unknown. </summary>
		public int unknown12; //12
		/// <summary>
		/// The size of the executable, including the ~PSP header. </summary>
		public int execSize; //16
		/// <summary>
		/// The maximum size needed for the decompression. </summary>
		public int maxAllocSize; //20
		/// <summary>
		/// The memory ID of the decompression buffer. </summary>
		public int decompressionMemId; //24
		/// <summary>
		/// Pointer to the compressed module data. </summary>
		public TPointer fileBase; //28
		/// <summary>
		/// Indicates the ELF type of the executable. One of ::SceExecFileElfType. </summary>
		public int elfType; //32
		/// <summary>
		/// The start address of the TEXT segment of the executable in memory. </summary>
		public TPointer topAddr; //36
		/// <summary>
		/// The entry address of the module. It is the offset from the start of the TEXT segment to the 
		/// program's entry point. 
		/// </summary>
		public int entryAddr; //40
		/// <summary>
		/// Unknown. </summary>
		public int unknown44;
		/// <summary>
		/// The size of the largest module segment. Should normally be "textSize", but technically can 
		/// be any other segment. 
		/// </summary>
		public int largestSegSize; //48
		/// <summary>
		/// The size of the TEXT segment. </summary>
		public int textSize; //52
		/// <summary>
		/// The size of the DATA segment. </summary>
		public int dataSize; //56
		/// <summary>
		/// The size of the BSS segment. </summary>
		public int bssSize; //60
		/// <summary>
		/// The memory partition of the executable. </summary>
		public int partitionId; //64
		/// <summary>
		/// Indicates whether the executable is a kernel module or not. Set to 1 for kernel module, 
		/// 0 for user module. 
		/// </summary>
		public int isKernelMod; //68
		/// <summary>
		/// Indicates whether the executable is decrypted or not. Set to 1 if it is successfully decrypted, 
		/// 0 for encrypted. 
		/// </summary>
		public int isDecrypted; //72
		/// <summary>
		/// The offset from the start address of the TEXT segment to the SceModuleInfo section. </summary>
		public int moduleInfoOffset; //76
		/// <summary>
		/// The pointer to the module's SceModuleInfo section. </summary>
		public TPointer moduleInfo; //80
		/// <summary>
		/// Indicates whether the module is compressed or not. Set to 1 if it is compressed, otherwise 0. </summary>
		public int isCompressed; //84
		/// <summary>
		/// The module's attributes. One or more of ::SceModuleAttribute and ::SceModulePrivilegeLevel. </summary>
		public int modInfoAttribute; //88
		/// <summary>
		/// The attributes of the executable file. One of ::SceExecFileAttr. </summary>
		public int execAttribute; //90
		/// <summary>
		/// The size of the decompressed module, including its headers. </summary>
		public int decSize; //92
		/// <summary>
		/// Indicates whether the module is decompressed or not. Set to 1 for decompressed, otherwise 0. </summary>
		public int isDecompressed; //96
		/// <summary>
		/// Indicates whether the module was signChecked or not. Set to 1 for signChecked, otherwise 0. 
		/// A signed module has a "mangled" executable header, in other words, the "~PSP" signature can't 
		/// be seen. 
		/// </summary>
		public int isSignChecked; //100
		/// <summary>
		/// Unknown. </summary>
		public int unknown104;
		/// <summary>
		/// The size of the GZIP compression overlap. </summary>
		public int overlapSize; //108
		/// <summary>
		/// Pointer to the first resident library entry table of the module. </summary>
		public TPointer exportsInfo; //112
		/// <summary>
		/// The size of all resident library entry tables of the module. </summary>
		public int exportsSize; //116
		/// <summary>
		/// Pointer to the first stub library entry table of the module. </summary>
		public TPointer importsInfo; //120
		/// <summary>
		/// The size of all stub library entry tables of the module. </summary>
		public int importsSize; //124
		/// <summary>
		/// Pointer to the string table section. </summary>
		public TPointer strtabOffset; //128
		/// <summary>
		/// The number of segments in the executable. </summary>
		public int numSegments; //132
		/// <summary>
		/// Reserved. </summary>
		public readonly sbyte[] padding = new sbyte[3]; //133
		/// <summary>
		/// An array containing the start address of each segment. </summary>
		public readonly TPointer[] segmentAddr = new TPointer[SCE_KERNEL_MAX_MODULE_SEGMENT]; //136
		/// <summary>
		/// An array containing the size of each segment. </summary>
		public readonly int[] segmentSize = new int[SCE_KERNEL_MAX_MODULE_SEGMENT]; //152
		/// <summary>
		/// The ID of the ELF memory block containing the TEXT, DATA and BSS segment. </summary>
		public int memBlockId; //168
		/// <summary>
		/// An array containing the alignment information of each segment. </summary>
		public readonly int[] segmentAlign = new int[SCE_KERNEL_MAX_MODULE_SEGMENT]; //172
		/// <summary>
		/// The largest value of the segmentAlign array. </summary>
		public int maxSegAlign; //188

		protected internal override void read()
		{
			unknown0 = read32();
			modeAttribute = read32();
			apiType = read32();
			unknown12 = read32();
			execSize = read32();
			maxAllocSize = read32();
			decompressionMemId = read32();
			fileBase = readPointer();
			elfType = read32();
			topAddr = readPointer();
			entryAddr = read32();
			unknown44 = read32();
			largestSegSize = read32();
			textSize = read32();
			dataSize = read32();
			bssSize = read32();
			partitionId = read32();
			isKernelMod = read32();
			isDecrypted = read32();
			moduleInfoOffset = read32();
			moduleInfo = readPointer();
			isCompressed = read32();
			modInfoAttribute = read16();
			execAttribute = read16();
			decSize = read32();
			isDecompressed = read32();
			isSignChecked = read32();
			unknown104 = read32();
			overlapSize = read32();
			exportsInfo = readPointer();
			exportsSize = read32();
			importsInfo = readPointer();
			importsSize = read32();
			strtabOffset = readPointer();
			numSegments = read8();
			read8Array(padding);
			readPointerArray(segmentAddr);
			read32Array(segmentSize);
			memBlockId = read32();
			read32Array(segmentAlign);
			maxSegAlign = read32();
		}

		protected internal override void write()
		{
			write32(unknown0);
			write32(modeAttribute);
			write32(apiType);
			write32(unknown12);
			write32(execSize);
			write32(maxAllocSize);
			write32(decompressionMemId);
			writePointer(fileBase);
			write32(elfType);
			writePointer(topAddr);
			write32(entryAddr);
			write32(unknown44);
			write32(largestSegSize);
			write32(textSize);
			write32(dataSize);
			write32(bssSize);
			write32(partitionId);
			write32(isKernelMod);
			write32(isDecrypted);
			write32(moduleInfoOffset);
			writePointer(moduleInfo);
			write32(isCompressed);
			write16((short) modInfoAttribute);
			write16((short) execAttribute);
			write32(decSize);
			write32(isDecompressed);
			write32(isSignChecked);
			write32(unknown104);
			write32(overlapSize);
			writePointer(exportsInfo);
			write32(exportsSize);
			writePointer(importsInfo);
			write32(importsSize);
			writePointer(strtabOffset);
			write8((sbyte) numSegments);
			write8Array(padding);
			writePointerArray(segmentAddr);
			write32Array(segmentSize);
			write32(memBlockId);
			write32Array(segmentAlign);
			write32(maxSegAlign);
		}

		public override int @sizeof()
		{
			return 192;
		}
	}

}