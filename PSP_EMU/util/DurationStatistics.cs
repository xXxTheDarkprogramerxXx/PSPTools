using System;
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
	public class DurationStatistics : IComparable<DurationStatistics>
	{
		public const bool collectStatistics = false;
		public string name;
		public long cumulatedTimeMillis;
		public long numberCalls;
		private long startTimeMillis;
		private long maxTimeMillis;

		public DurationStatistics()
		{
			reset();
		}

		public DurationStatistics(string name)
		{
			this.name = name;
			reset();
		}

		public virtual void start()
		{
			if (!collectStatistics)
			{
				return;
			}

			startTimeMillis = DateTimeHelper.CurrentUnixTimeMillis();
		}

		public virtual void end()
		{
			if (!collectStatistics)
			{
				return;
			}

			long duration = DurationMillis;
			cumulatedTimeMillis += duration;
			numberCalls++;
			maxTimeMillis = System.Math.Max(maxTimeMillis, duration);
		}

		public virtual long DurationMillis
		{
			get
			{
				return DateTimeHelper.CurrentUnixTimeMillis() - startTimeMillis;
			}
		}

		public virtual void add(DurationStatistics durationStatistics)
		{
			cumulatedTimeMillis += durationStatistics.cumulatedTimeMillis;
			numberCalls += durationStatistics.numberCalls;
			maxTimeMillis = System.Math.Max(maxTimeMillis, durationStatistics.maxTimeMillis);
		}

		public virtual void reset()
		{
			cumulatedTimeMillis = 0;
			numberCalls = 0;
			startTimeMillis = 0;
			maxTimeMillis = 0;
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			if (!string.ReferenceEquals(name, null))
			{
				result.Append(name);
				result.Append(": ");
			}

			if (!collectStatistics)
			{
				result.Append("Statistics disabled");
			}
			else
			{
				result.Append(numberCalls);
				result.Append(" calls");
				if (numberCalls > 0)
				{
					result.Append(" in ");
					result.Append(string.Format("{0:F3}s", cumulatedTimeMillis / 1000.0));
					result.Append(" (avg=");
					double average = cumulatedTimeMillis / (1000.0 * numberCalls);
					if (average < 0.000001)
					{
						result.Append(string.Format("{0:F3}us", average * 1000000));
					}
					else if (average < 0.001)
					{
						result.Append(string.Format("{0:F3}ms", average * 1000));
					}
					else
					{
						result.Append(string.Format("{0:F3}s", average));
					}
					result.Append(string.Format(", max={0:D}ms", maxTimeMillis));
					result.Append(")");
				}
			}

			return result.ToString();
		}

		public virtual int CompareTo(DurationStatistics o)
		{
			if (cumulatedTimeMillis < o.cumulatedTimeMillis)
			{
				return 1;
			}
			else if (cumulatedTimeMillis > o.cumulatedTimeMillis)
			{
				return -1;
			}
			else if (numberCalls < o.numberCalls)
			{
				return 1;
			}
			else if (numberCalls > o.numberCalls)
			{
				return -1;
			}

			return 0;
		}
	}

}