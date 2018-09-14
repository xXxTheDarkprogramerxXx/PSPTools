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
namespace pspsharp.scheduler
{

	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using IAction = pspsharp.HLE.kernel.types.IAction;

	public class Scheduler
	{
		private static Scheduler instance = null;
		private IList<SchedulerAction> actions;
		private SchedulerAction nextAction;

		public static Scheduler Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Scheduler();
				}
    
				return instance;
			}
		}

		public virtual void reset()
		{
			lock (this)
			{
				actions = new LinkedList<SchedulerAction>();
				nextAction = null;
			}
		}

		public virtual void step()
		{
			lock (this)
			{
				if (nextAction == null)
				{
					return;
				}
			}

			long now = Now;
			while (true)
			{
				IAction action = getAction(now);
				if (action == null)
				{
					break;
				}
				action.execute();
			}
		}

		public virtual long getNextActionDelay(long noActionDelay)
		{
			lock (this)
			{
				if (nextAction == null)
				{
					return noActionDelay;
				}
        
				long now = Now;
				return nextAction.Schedule - now;
			}
		}

		private void addSchedulerAction(SchedulerAction schedulerAction)
		{
			actions.Add(schedulerAction);
			if (updateNextAction(schedulerAction))
			{
				RuntimeContext.onNextScheduleModified();
			}
		}

		/// <summary>
		/// Add a new action to be executed as soon as possible to the Scheduler.
		/// This method has to be thread-safe.
		/// </summary>
		/// <param name="action">	action to be executed on the defined schedule. </param>
		public virtual void addAction(IAction action)
		{
			lock (this)
			{
				SchedulerAction schedulerAction = new SchedulerAction(0, action);
				addSchedulerAction(schedulerAction);
			}
		}

		/// <summary>
		/// Add a new action to the Scheduler.
		/// This method has to be thread-safe.
		/// </summary>
		/// <param name="schedule">	microTime when the action has to be executed. 0 for now. </param>
		/// <param name="action">	action to be executed on the defined schedule. </param>
		public virtual void addAction(long schedule, IAction action)
		{
			lock (this)
			{
				SchedulerAction schedulerAction = new SchedulerAction(schedule, action);
				addSchedulerAction(schedulerAction);
			}
		}

		public virtual void removeAction(long schedule, IAction action)
		{
			lock (this)
			{
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
				for (IEnumerator<SchedulerAction> lit = actions.GetEnumerator(); lit.MoveNext();)
				{
					SchedulerAction schedulerAction = lit.Current;
					if (schedulerAction.Schedule == schedule && schedulerAction.Action == action)
					{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						lit.remove();
						updateNextAction();
						break;
					}
				}
			}
		}

		private bool updateNextAction(SchedulerAction schedulerAction)
		{
			if (nextAction == null || schedulerAction.Schedule < nextAction.Schedule)
			{
				nextAction = schedulerAction;
				return true;
			}

			return false;
		}

		private void updateNextAction()
		{
			nextAction = null;

			for (IEnumerator<SchedulerAction> it = actions.GetEnumerator(); it.MoveNext();)
			{
				SchedulerAction schedulerAction = it.Current;
				updateNextAction(schedulerAction);
			}

			RuntimeContext.onNextScheduleModified();
		}

		public virtual IAction getAction(long now)
		{
			lock (this)
			{
				if (nextAction == null || now < nextAction.Schedule)
				{
					return null;
				}
        
				IAction action = nextAction.Action;
        
				actions.Remove(nextAction);
				updateNextAction();
        
				return action;
			}
		}

		public static long Now
		{
			get
			{
				return Emulator.Clock.microTime();
			}
		}
	}

}