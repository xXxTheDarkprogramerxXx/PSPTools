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
namespace pspsharp.format.rco.anim
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.scheduler.Scheduler.getNow;

	using Logger = org.apache.log4j.Logger;

	using IAction = pspsharp.HLE.kernel.types.IAction;
	using VSMX = pspsharp.format.rco.vsmx.VSMX;

	public abstract class AbstractAnimAction : IAction
	{
		protected internal static readonly Logger log = VSMX.log;
		private int duration;
		private long start;

		protected internal AbstractAnimAction(int duration)
		{
			this.duration = duration * 1000;

			start = Now;
		}

		private long getNextSchedule(long now)
		{
			return now + System.Math.Max(duration / 10000, 1000);
		}

		public virtual void execute()
		{
			long now = Now;
			int currentDuration = (int)(now - start);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("BaseAnimAction duration={0:D}/{1:D}", currentDuration, duration));
			}
			currentDuration = System.Math.Min(currentDuration, duration);
			float step = currentDuration == duration ? 1f : currentDuration / (float) duration;

			anim(step);

			if (currentDuration < duration)
			{
				Emulator.Scheduler.addAction(getNextSchedule(now), this);
			}
		}

		protected internal static float interpolate(float start, float end, float step)
		{
			return start + (end - start) * step;
		}

		protected internal abstract void anim(float step);
	}

}