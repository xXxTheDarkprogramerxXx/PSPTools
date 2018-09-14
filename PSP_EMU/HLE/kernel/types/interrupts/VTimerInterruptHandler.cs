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

	public class VTimerInterruptHandler : AbstractAllegrexInterruptHandler
	{
		private SceKernelVTimerInfo sceKernelVTimerInfo;

		public VTimerInterruptHandler(SceKernelVTimerInfo sceKernelVTimerInfo) : base(sceKernelVTimerInfo.handlerAddress)
		{
			this.sceKernelVTimerInfo = sceKernelVTimerInfo;
			setArgument(0, sceKernelVTimerInfo.uid);
			setArgument(3, sceKernelVTimerInfo.handlerArgument);
		}

		public override void copyArgumentsToCpu(CpuState cpu)
		{
			Memory mem = Memory.Instance;
			int internalMemory = sceKernelVTimerInfo.InternalMemory;
			if (internalMemory != 0)
			{
				mem.write64(internalMemory, sceKernelVTimerInfo.schedule);
				mem.write64(internalMemory + 8, sceKernelVTimerInfo.CurrentTime);
				setArgument(1, internalMemory);
				setArgument(2, internalMemory + 8);
			}
			else
			{
				setArgument(1, 0);
				setArgument(2, 0);
			}

			base.copyArgumentsToCpu(cpu);
		}
	}

}