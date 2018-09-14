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
namespace pspsharp.format
{
	using SceModule = pspsharp.HLE.kernel.types.SceModule;

	public class DeferredVStub32 : DeferredStub
	{
		private bool hasSavedValue;
		private int savedValue;

		public DeferredVStub32(SceModule sourceModule, string moduleName, int importAddress, int nid) : base(sourceModule, moduleName, importAddress, nid)
		{
		}

		public override void resolve(Memory mem, int address)
		{
			if (!hasSavedValue)
			{
				savedValue = mem.read32(ImportAddress);
				hasSavedValue = true;
			}

			// Perform a R_MIPS_32 relocation

			// Retrieve the current 32bit value
			int value = mem.read32(ImportAddress);

			// Relocate the value
			value += address;

			// Write back the relocated 32bit value
			mem.write32(ImportAddress, value);
		}

		public override void unresolve(Memory mem)
		{
			if (hasSavedValue)
			{
				mem.write32(ImportAddress, savedValue);
			}

			if (sourceModule != null)
			{
				// Add this stub back to the list of unresolved imports from the source module
				sourceModule.unresolvedImports.Add(this);
			}
		}

		public override string ToString()
		{
			return string.Format("Reloc32 {0}", base.ToString());
		}
	}

}