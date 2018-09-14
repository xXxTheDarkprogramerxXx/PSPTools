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
	public class SceKernelSystemStatus : pspAbstractMemoryMappedStructureVariableLength
	{
		/// <summary>
		/// The status ? </summary>
		public int status;
		/// <summary>
		/// The number of cpu clocks in the idle thread </summary>
		public long idleClocks;
		/// <summary>
		/// Number of times we resumed from idle </summary>
		public int comesOutOfIdleCount;
		/// <summary>
		/// Number of thread context switches </summary>
		public int threadSwitchCount;
		/// <summary>
		/// Number of vfpu switches ? </summary>
		public int vfpuSwitchCount;

		protected internal override void read()
		{
			base.read();
			status = read32();
			idleClocks = read64();
			comesOutOfIdleCount = read32();
			threadSwitchCount = read32();
			vfpuSwitchCount = read32();
		}

		protected internal override void write()
		{
			base.write();
			write32(status);
			write64(idleClocks);
			write32(comesOutOfIdleCount);
			write32(threadSwitchCount);
			write32(vfpuSwitchCount);
		}
	}

}