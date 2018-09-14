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
	/// This structure is used to boot system modules during the initialization of Loadcore. It represents
	/// a module object with all the necessary information needed to boot it.
	/// </summary>
	public class SceLoadCoreBootModuleInfo : pspAbstractMemoryMappedStructure
	{
		/// <summary>
		/// The full path (including filename) of the module. </summary>
		public TPointer modPath; //0
		/// <summary>
		/// The buffer with the entire file content. </summary>
		public TPointer modBuf; //4
		/// <summary>
		/// The size of the module. </summary>
		public int modSize; //8
		/// <summary>
		/// Unknown. </summary>
		public TPointer unk12; //12
		/// <summary>
		/// Attributes. </summary>
		public int attr; //16
		/// <summary>
		/// Contains the API type of the module prior to the allocation of memory for the module. 
		/// Once memory is allocated, ::bootData contains the ID of that memory partition.
		/// </summary>
		public int bootData; //20
		/// <summary>
		/// The size of the arguments passed to the module's entry function? </summary>
		public int argSize; //24
		/// <summary>
		/// The partition ID of the arguments passed to the module's entry function? </summary>
		public int argPartId; //28

		protected internal override void read()
		{
			modPath = readPointer();
			modBuf = readPointer();
			modSize = read32();
			unk12 = readPointer();
			attr = read32();
			bootData = read32();
			argSize = read32();
			argPartId = read32();
		}

		protected internal override void write()
		{
			writePointer(modPath);
			writePointer(modBuf);
			write32(modSize);
			writePointer(unk12);
			write32(attr);
			write32(bootData);
			write32(argSize);
			write32(argPartId);
		}

		public override int @sizeof()
		{
			return 32;
		}
	}

}