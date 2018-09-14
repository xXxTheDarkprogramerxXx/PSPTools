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

	public class SceLoadCoreBootInfo : pspAbstractMemoryMappedStructure
	{
		// This structure seems to be the same as SceKernelRebootParam.
		/// <summary>
		/// Pointer to a memory block which will be cleared in case the system initialization via 
		/// Loadcore fails.
		/// </summary>
		public int memBase;
		/// <summary>
		/// The size of the memory block to clear. </summary>
		public int memSize;
		/// <summary>
		/// Number of modules already loaded during boot process. </summary>
		public int loadedModules;
		/// <summary>
		/// Number of modules to boot. </summary>
		public int numModules;
		/// <summary>
		/// The modules to boot. </summary>
		public TPointer startAddr;
		public TPointer endAddr;
		public int unknown24;
		public readonly sbyte[] reserved = new sbyte[3];
		/// <summary>
		/// The number of protected (?)modules. </summary>
		public int numProtects;
		/// <summary>
		/// Pointer to the protected (?)modules. </summary>
		public TPointer protects;
		/// <summary>
		/// The ID of a protected info. </summary>
		public int modProtId;
		/// <summary>
		/// The ID of a module's arguments? </summary>
		public int modArgProtId;
		/// <summary>
		/// The PSP model as returned by sceKernelGetModel() </summary>
		public int model;
		public int buildVersion;
		public int unknown52;
		/// <summary>
		/// The path/name of a boot configuration file. </summary>
		public TPointer configFile;
		public int unknown60;
		public int dipswLo;
		public int dipswHi;
		public int unknown72;
		public int unknown76;
		public int cpTime;

		protected internal override void read()
		{
			memBase = read32(); // Offset 0
			memSize = read32(); // Offset 4
			loadedModules = read32(); // Offset 8
			numModules = read32(); // Offset 12
			startAddr = readPointer(); // Offset 16
			endAddr = readPointer(); // Offset 20
			unknown24 = read8(); // Offset 24
			read8Array(reserved); // Offset 25
			numProtects = read32(); // Offset 28
			protects = readPointer(); // Offset 32
			modProtId = read32(); // Offset 36
			modArgProtId = read32(); // Offset 40
			model = read32(); // Offset 44
			buildVersion = read32(); // Offset 48
			unknown52 = read32(); // Offset 52
			configFile = readPointer(); // Offset 56
			unknown60 = read32(); // Offset 60
			dipswLo = read32(); // Offset 64
			dipswHi = read32(); // Offset 68
			unknown72 = read32(); // Offset 72
			unknown76 = read32(); // Offset 76
			cpTime = read32(); // Offset 80
		}

		protected internal override void write()
		{
			write32(memBase);
			write32(memSize);
			write32(loadedModules);
			write32(numModules);
			writePointer(startAddr);
			writePointer(endAddr);
			write8((sbyte) unknown24);
			write8Array(reserved);
			write32(numProtects);
			writePointer(protects);
			write32(modProtId);
			write32(modArgProtId);
			write32(model);
			write32(buildVersion);
			write32(unknown52);
			writePointer(configFile);
			write32(unknown60);
			write32(dipswLo);
			write32(dipswHi);
			write32(unknown72);
			write32(unknown76);
			write32(cpTime);
		}

		public override int @sizeof()
		{
			// Size of SceKernelRebootParam
			return 0x1000;
		}
	}

}