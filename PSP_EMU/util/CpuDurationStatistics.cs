using System.Text;

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
namespace pspsharp.util
{

	public class CpuDurationStatistics : DurationStatistics
	{
		public long cumulatedCpuTimeNanos;
		private long startCpuTimeNanos;
		private ThreadMXBean threadMXBean;

		public CpuDurationStatistics() : base()
		{
			threadMXBean = ManagementFactory.ThreadMXBean;
		}

		public CpuDurationStatistics(string name) : base(name)
		{
			threadMXBean = ManagementFactory.ThreadMXBean;
		}

		public virtual long CpuDurationMillis
		{
			get
			{
				return cumulatedCpuTimeNanos / 1000000L;
			}
		}

		public override void start()
		{
			if (!collectStatistics)
			{
				return;
			}

			if (threadMXBean.ThreadCpuTimeEnabled)
			{
				startCpuTimeNanos = threadMXBean.CurrentThreadCpuTime;
			}
			base.start();
		}

		public override void end()
		{
			if (!collectStatistics)
			{
				return;
			}

			if (threadMXBean.ThreadCpuTimeEnabled)
			{
				long duration = threadMXBean.CurrentThreadCpuTime - startCpuTimeNanos;
				cumulatedCpuTimeNanos += duration;
			}
			base.end();
		}

		public override void reset()
		{
			cumulatedCpuTimeNanos = 0;
			startCpuTimeNanos = 0;
			base.reset();
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			result.Append(base.ToString());
			if (collectStatistics && numberCalls > 0 && threadMXBean.ThreadCpuTimeEnabled)
			{
				result.Append(string.Format(" CPU {0:F3}s", cumulatedCpuTimeNanos / 1000000000.0));
			}

			return result.ToString();
		}

		public virtual int CompareTo(DurationStatistics o)
		{
			if (o is CpuDurationStatistics)
			{
				CpuDurationStatistics cpuDurationStatistics = (CpuDurationStatistics) o;
				if (cumulatedCpuTimeNanos < cpuDurationStatistics.cumulatedCpuTimeNanos)
				{
					return 1;
				}
				else if (cumulatedCpuTimeNanos > cpuDurationStatistics.cumulatedCpuTimeNanos)
				{
					return -1;
				}
			}

			return base.CompareTo(o);
		}
	}

}