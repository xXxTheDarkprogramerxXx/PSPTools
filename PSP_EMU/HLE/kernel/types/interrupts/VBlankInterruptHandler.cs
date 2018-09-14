using System.Collections.Generic;

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

	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using Scheduler = pspsharp.scheduler.Scheduler;

	public class VBlankInterruptHandler : AbstractInterruptHandler
	{
		private IList<IAction> vblankActions = new LinkedList<IAction>();
		private IList<IAction> vblankActionsOnce = new LinkedList<IAction>();
		private long nextVblankSchedule = IntrManager.VBLANK_SCHEDULE_MICROS;

		protected internal override void executeInterrupt()
		{
			Scheduler scheduler = Emulator.Scheduler;

			// Re-schedule next VBLANK interrupt in 1/60 second
			scheduler.addAction(nextVblankSchedule, this);

			// The next VBlank schedule is the next 1/60 interval after now
			nextVblankSchedule += IntrManager.VBLANK_SCHEDULE_MICROS;
			long now = Scheduler.Now;
			while (nextVblankSchedule < now)
			{
				nextVblankSchedule += IntrManager.VBLANK_SCHEDULE_MICROS;
			}

			// Execute all the registered VBlank actions (each time)
			foreach (IAction action in vblankActions)
			{
				if (action != null)
				{
					action.execute();
				}
			}

			// Execute all the registered VBlank actions (once)
			foreach (IAction action in vblankActionsOnce)
			{
				if (action != null)
				{
					action.execute();
				}
			}
			vblankActionsOnce.Clear();

			// Trigger VBLANK interrupt
			IntrManager.Instance.triggerInterrupt(IntrManager.PSP_VBLANK_INTR, null, null);
		}

		public virtual void addVBlankAction(IAction action)
		{
			vblankActions.Add(action);
		}

		public virtual bool removeVBlankAction(IAction action)
		{
			return vblankActions.Remove(action);
		}

		public virtual void addVBlankActionOnce(IAction action)
		{
			vblankActionsOnce.Add(action);
		}

		public virtual bool removeVBlankActionOnce(IAction action)
		{
			return vblankActionsOnce.Remove(action);
		}
	}

}