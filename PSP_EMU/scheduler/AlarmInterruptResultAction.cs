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
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelAlarmInfo = pspsharp.HLE.kernel.types.SceKernelAlarmInfo;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;

	public class AlarmInterruptResultAction : IAction
	{
		private SceKernelAlarmInfo sceKernelAlarmInfo;

		public AlarmInterruptResultAction(SceKernelAlarmInfo sceKernelAlarmInfo)
		{
			this.sceKernelAlarmInfo = sceKernelAlarmInfo;
		}

		public virtual void execute()
		{
			ThreadManForUser timerManager = Modules.ThreadManForUserModule;

			int alarmInterruptResult = Emulator.Processor.cpu._v0;
			if (Modules.log.DebugEnabled)
			{
				Modules.log.debug("Alarm returned value " + alarmInterruptResult);
			}

			if (alarmInterruptResult == 0)
			{
				// Alarm is canceled
				timerManager.cancelAlarm(sceKernelAlarmInfo);
			}
			else
			{
				timerManager.rescheduleAlarm(sceKernelAlarmInfo, alarmInterruptResult);
			}
		}
	}

}