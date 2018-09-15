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
	using Modules = pspsharp.HLE.Modules;
	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelAlarmInfo = pspsharp.HLE.kernel.types.SceKernelAlarmInfo;

	public class AlarmInterruptAction : IAction
	{
		private SceKernelAlarmInfo sceKernelAlarmInfo;

		public AlarmInterruptAction(SceKernelAlarmInfo sceKernelAlarmInfo)
		{
			this.sceKernelAlarmInfo = sceKernelAlarmInfo;
		}

		public virtual void execute()
		{
			long now = Scheduler.Now;

			// Trigger interrupt
			if (Modules.log.DebugEnabled)
			{
				Modules.Console.WriteLine(string.Format("Calling Timer uid={0:x}, now={1:D}", sceKernelAlarmInfo.uid, now));
			}

			// Set the real time when the alarm was called
			sceKernelAlarmInfo.schedule = now;

			IntrManager.Instance.triggerInterrupt(IntrManager.PSP_SYSTIMER0_INTR, null, sceKernelAlarmInfo.alarmInterruptResultAction, sceKernelAlarmInfo.alarmInterruptHandler);
		}
	}

}