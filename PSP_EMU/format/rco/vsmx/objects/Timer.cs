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
namespace pspsharp.format.rco.vsmx.objects
{

	using Logger = org.apache.log4j.Logger;

	using IAction = pspsharp.HLE.kernel.types.IAction;
	using VSMXArray = pspsharp.format.rco.vsmx.interpreter.VSMXArray;
	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXFunction = pspsharp.format.rco.vsmx.interpreter.VSMXFunction;
	using VSMXInterpreter = pspsharp.format.rco.vsmx.interpreter.VSMXInterpreter;
	using VSMXNativeObject = pspsharp.format.rco.vsmx.interpreter.VSMXNativeObject;
	using VSMXNumber = pspsharp.format.rco.vsmx.interpreter.VSMXNumber;
	using Scheduler = pspsharp.scheduler.Scheduler;

	public class Timer : BaseNativeObject
	{
		private new static readonly Logger log = VSMX.log;
		private int currentTimerId = 0;
		private VSMXInterpreter interpreter;
		private IDictionary<int, TimerAction> timers;

		private class TimerAction : IAction
		{
			private readonly Timer outerInstance;

			internal int id;
			internal VSMXBaseObject @object;
			internal VSMXBaseObject function;
			internal VSMXBaseObject[] parameters;

			public TimerAction(Timer outerInstance, int id, VSMXBaseObject @object, VSMXBaseObject function, VSMXBaseObject[] parameters)
			{
				this.outerInstance = outerInstance;
				this.id = id;
				this.@object = @object;
				this.function = function;
				this.parameters = parameters;
			}

			public virtual void execute()
			{
				outerInstance.onTimer(id, @object, function, parameters);
			}
		}

		public static VSMXNativeObject create(VSMXInterpreter interpreter)
		{
			Timer timer = new Timer(interpreter);
			VSMXNativeObject @object = new VSMXNativeObject(interpreter, timer);
			timer.Object = @object;

			return @object;
		}

		private Timer(VSMXInterpreter interpreter)
		{
			this.interpreter = interpreter;
			timers = new Dictionary<int, Timer.TimerAction>();
		}

		private void onTimer(int id, VSMXBaseObject @object, VSMXBaseObject function, VSMXBaseObject[] parameters)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Timer.onTimer id={0:D}, object={1}, function={2}, parameters={3}", id, @object, function, parameters));
			}

			if (function is VSMXFunction)
			{
				interpreter.interpretFunction((VSMXFunction) function, @object, parameters);
			}
		}

		private VSMXBaseObject setInterval(VSMXBaseObject @object, VSMXBaseObject function, VSMXBaseObject interval, params VSMXBaseObject[] parameters)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Timer.setInterval function={0}, interval={1:D}, numberOfParameters={2:D}", function, interval.IntValue, parameters.Length));
				for (int i = 0; i < parameters.Length; i++)
				{
					log.debug(string.Format("Timer.setInterval param{0:D}={1}", i, parameters[i]));
				}
			}

			int id = currentTimerId++;
			long schedule = Scheduler.Now + interval.IntValue * 1000;

			TimerAction timerAction = new TimerAction(this, id, @object, function, parameters);
			timers[id] = timerAction;
			Scheduler.Instance.addAction(schedule, timerAction);

			// setInterval seems to return an array object. Not sure how to fill it.
			VSMXArray result = new VSMXArray(interpreter, 1);
			result.setPropertyValue(0, new VSMXNumber(interpreter, id));

			return result;
		}

		public virtual VSMXBaseObject setInterval(VSMXBaseObject @object, VSMXBaseObject function, VSMXBaseObject interval)
		{
			return setInterval(@object, function, interval, new VSMXBaseObject[0]);
		}

		public virtual VSMXBaseObject setInterval(VSMXBaseObject @object, VSMXBaseObject function, VSMXBaseObject interval, VSMXBaseObject param1)
		{
			return setInterval(@object, function, interval, new VSMXBaseObject[] {param1});
		}

		public virtual VSMXBaseObject setInterval(VSMXBaseObject @object, VSMXBaseObject function, VSMXBaseObject interval, VSMXBaseObject param1, VSMXBaseObject param2)
		{
			return setInterval(@object, function, interval, new VSMXBaseObject[] {param1, param2});
		}

		public virtual VSMXBaseObject setInterval(VSMXBaseObject @object, VSMXBaseObject function, VSMXBaseObject interval, VSMXBaseObject param1, VSMXBaseObject param2, VSMXBaseObject param3)
		{
			return setInterval(@object, function, interval, new VSMXBaseObject[] {param1, param2, param3});
		}

		public virtual VSMXBaseObject setInterval(VSMXBaseObject @object, VSMXBaseObject function, VSMXBaseObject interval, VSMXBaseObject param1, VSMXBaseObject param2, VSMXBaseObject param3, VSMXBaseObject param4)
		{
			return setInterval(@object, function, interval, new VSMXBaseObject[] {param1, param2, param3, param4});
		}

		public virtual VSMXBaseObject setInterval(VSMXBaseObject @object, VSMXBaseObject function, VSMXBaseObject interval, VSMXBaseObject param1, VSMXBaseObject param2, VSMXBaseObject param3, VSMXBaseObject param4, VSMXBaseObject param5)
		{
			return setInterval(@object, function, interval, new VSMXBaseObject[] {param1, param2, param3, param4, param5});
		}

		public virtual VSMXBaseObject setInterval(VSMXBaseObject @object, VSMXBaseObject function, VSMXBaseObject interval, VSMXBaseObject param1, VSMXBaseObject param2, VSMXBaseObject param3, VSMXBaseObject param4, VSMXBaseObject param5, VSMXBaseObject param6)
		{
			return setInterval(@object, function, interval, new VSMXBaseObject[] {param1, param2, param3, param4, param5, param6});
		}

		public virtual void clearInterval(VSMXBaseObject @object, VSMXBaseObject id)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("Timer.clearInterval {0:D}", id.getPropertyValue(0).IntValue));
			}
		}
	}

}