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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.SyscallHandler.syscallUnmappedImport;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.J;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.SYSCALL;

	using SceModule = pspsharp.HLE.kernel.types.SceModule;

	public class DeferredStub
	{
		protected internal SceModule sourceModule;
		private string moduleName;
		private int importAddress;
		private int nid;
		private bool savedImport;
		private int savedImport1;
		private int savedImport2;

		public DeferredStub(SceModule sourceModule, string moduleName, int importAddress, int nid)
		{
			this.sourceModule = sourceModule;
			this.moduleName = moduleName;
			this.importAddress = importAddress & Memory.addressMask;
			this.nid = nid;
		}

		public virtual string ModuleName
		{
			get
			{
				return moduleName;
			}
		}

		public virtual int ImportAddress
		{
			get
			{
				return importAddress;
			}
		}

		public virtual int Nid
		{
			get
			{
				return nid;
			}
		}

		public virtual void resolve(Memory mem, int address)
		{
			if (!savedImport)
			{
				savedImport1 = mem.read32(importAddress);
				savedImport2 = mem.read32(importAddress + 4);
				savedImport = true;
			}

			// j <address>
			mem.write32(importAddress, J(address));
			mem.write32(importAddress + 4, 0); // write a nop over our "unmapped import detection special syscall"
		}

		public virtual void unresolve(Memory mem)
		{
			if (savedImport)
			{
				mem.write32(importAddress, savedImport1);
				mem.write32(importAddress + 4, savedImport2);
			}
			else
			{
				// syscall <syscallUnmappedImport>
				mem.write32(importAddress + 4, SYSCALL(syscallUnmappedImport));
			}

			if (sourceModule != null)
			{
				// Add this stub back to the list of unresolved imports from the source module
				sourceModule.unresolvedImports.Add(this);
			}
		}

		public override string ToString()
		{
			return string.Format("0x{0:X8} [0x{1:X8}] Module '{2}'", ImportAddress, Nid, ModuleName);
		}
	}

}