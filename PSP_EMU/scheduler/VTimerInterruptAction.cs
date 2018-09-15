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
namespace pspsharp.scheduler
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.log;
	using Modules = pspsharp.HLE.Modules;
	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelVTimerInfo = pspsharp.HLE.kernel.types.SceKernelVTimerInfo;
	using VTimerInterruptHandler = pspsharp.HLE.kernel.types.interrupts.VTimerInterruptHandler;

	public class VTimerInterruptAction : IAction
	{
		private SceKernelVTimerInfo sceKernelVTimerInfo;

		public VTimerInterruptAction(SceKernelVTimerInfo sceKernelVTimerInfo)
		{
			this.sceKernelVTimerInfo = sceKernelVTimerInfo;
		}

		public virtual void execute()
		{
			if (sceKernelVTimerInfo.handlerAddress == 0)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("VTimerInterruptAction with null handler, cancelling the VTimer {0}", sceKernelVTimerInfo));
				}

				Modules.ThreadManForUserModule.cancelVTimer(sceKernelVTimerInfo);
			}
			else
			{
				long now = Scheduler.Now;

				// Trigger interrupt
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Calling VTimer uid=0x{0:X}, now={1:D}", sceKernelVTimerInfo.uid, now));
				}

				sceKernelVTimerInfo.vtimerInterruptHandler = new VTimerInterruptHandler(sceKernelVTimerInfo);

				IntrManager.Instance.triggerInterrupt(IntrManager.PSP_SYSTIMER0_INTR, null, sceKernelVTimerInfo.vtimerInterruptResultAction, sceKernelVTimerInfo.vtimerInterruptHandler);
			}
		}
	}

}