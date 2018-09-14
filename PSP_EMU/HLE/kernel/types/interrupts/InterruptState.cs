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
namespace pspsharp.HLE.kernel.types.interrupts
{
	using CpuState = pspsharp.Allegrex.CpuState;

	public class InterruptState
	{
		internal bool insideInterrupt;
		private CpuState savedCpu;
		private IAction afterInterruptAction;
		private IAction afterHandlerAction;

		public virtual void save(bool insideInterrupt, CpuState cpu, IAction afterInterruptAction, IAction afterHandlerAction)
		{
			this.insideInterrupt = insideInterrupt;
			savedCpu = new CpuState(cpu);
			this.afterInterruptAction = afterInterruptAction;
			this.afterHandlerAction = afterHandlerAction;
		}

		public virtual bool restore(CpuState cpu)
		{
			cpu.copy(savedCpu);

			return insideInterrupt;
		}

		public virtual IAction AfterInterruptAction
		{
			get
			{
				return afterInterruptAction;
			}
		}

		public virtual IAction AfterHandlerAction
		{
			get
			{
				return afterHandlerAction;
			}
		}
	}

}