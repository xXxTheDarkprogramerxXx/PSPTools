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
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelVTimerInfo = pspsharp.HLE.kernel.types.SceKernelVTimerInfo;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;

	public class VTimerInterruptResultAction : IAction
	{
		private SceKernelVTimerInfo sceKernelVTimerInfo;

		public VTimerInterruptResultAction(SceKernelVTimerInfo sceKernelVTimerInfo)
		{
			this.sceKernelVTimerInfo = sceKernelVTimerInfo;
		}

		public virtual void execute()
		{
			ThreadManForUser timerManager = Modules.ThreadManForUserModule;

			int vtimerInterruptResult = Emulator.Processor.cpu._v0;
			if (log.DebugEnabled)
			{
				log.debug("VTimer returned value " + vtimerInterruptResult);
			}

			if (vtimerInterruptResult == 0)
			{
				// VTimer is canceled
				timerManager.cancelVTimer(sceKernelVTimerInfo);
			}
			else
			{
				timerManager.rescheduleVTimer(sceKernelVTimerInfo, vtimerInterruptResult);
			}
		}

	}

}