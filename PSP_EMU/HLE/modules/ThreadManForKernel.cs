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
//	import static pspsharp.HLE.HLEModuleManager.HLESyscallNid;

	using Logger = org.apache.log4j.Logger;

	using Managers = pspsharp.HLE.kernel.Managers;

	public class ThreadManForKernel : HLEModule
	{
		public static Logger log = Modules.getLogger("ThreadManForKernel");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x04E72261, version = 150) public int sceKernelAllocateKTLS(int size, pspsharp.HLE.TPointer callback, int callbackArg)
		[HLEFunction(nid : 0x04E72261, version : 150)]
		public virtual int sceKernelAllocateKTLS(int size, TPointer callback, int callbackArg)
		{
			int id = Modules.ThreadManForUserModule.sceKernelCreateTlspl("KTLS", SysMemUserForUser.KERNEL_PARTITION_ID, SysMemUserForUser.PSP_SMEM_Low, size, 32, TPointer.NULL);

			return id;
		}

		[HLEFunction(nid : 0x4FE44D5E, version : 150)]
		public virtual int sceKernelCheckThreadKernelStack()
		{
			return 4096;
		}

		/// <summary>
		/// Checks if the current thread is a usermode thread.
		/// </summary>
		/// <returns> 0 if kernel, 1 if user, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x85A2A5BF, version = 150) public int sceKernelIsUserModeThread()
		[HLEFunction(nid : 0x85A2A5BF, version : 150)]
		public virtual int sceKernelIsUserModeThread()
		{
			return 1;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA249EAAE, version = 150) public int sceKernelGetKTLS(int id)
		[HLEFunction(nid : 0xA249EAAE, version : 150)]
		public virtual int sceKernelGetKTLS(int id)
		{
			return Modules.Kernel_LibraryModule.sceKernel_FA835CDE(id);
		}

		/// <summary>
		/// This HLE syscall is used when reading the hardware register 0xBC600000.
		/// It is equivalent to reading the system time.
		/// </summary>
		/// <returns> the same value as returned by sceKernelGetSystemTimeLow(). </returns>
		[HLEFunction(nid : HLESyscallNid, version : 150)]
		public virtual int hleKernelGetSystemTimeLow()
		{
			return Managers.systime.sceKernelGetSystemTimeLow();
		}
	}

}