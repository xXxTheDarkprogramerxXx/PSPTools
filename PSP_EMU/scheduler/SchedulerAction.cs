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
	using IAction = pspsharp.HLE.kernel.types.IAction;

	public class SchedulerAction
	{
		private long schedule;
		private IAction action;

		public SchedulerAction(long schedule, IAction action)
		{
			this.schedule = schedule;
			this.action = action;
		}

		public virtual long Schedule
		{
			get
			{
				return schedule;
			}
			set
			{
				this.schedule = value;
			}
		}


		public virtual IAction Action
		{
			get
			{
				return action;
			}
			set
			{
				this.action = value;
			}
		}


		public override string ToString()
		{
			return string.Format("schedule=0x{0:X}, action={1}", schedule, action);
		}
	}

}