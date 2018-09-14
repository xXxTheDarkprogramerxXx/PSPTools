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
namespace pspsharp
{
	/// <summary>
	/// A PSP Clock that can run slower or faster than the normal clock.
	/// The speed can be changed dynamically to let an application run slower/faster.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class VariableSpeedClock : Clock
	{
		private int numerator;
		private int denominator;
		private long baseSystemNanoTime;
		private long baseSystemMilliTime;
		private long baseStartSystemNanoTime;
		private long baseStartSystemMilliTime;

		public VariableSpeedClock(Clock clock, int numerator, int denominator) : base(clock)
		{
			setSpeed(numerator, denominator);
		}

		public virtual void setSpeed(int numerator, int denominator)
		{
			if (this.denominator == 0)
			{
				baseStartSystemNanoTime = base.SystemNanoTime;
				baseStartSystemMilliTime = base.SystemMilliTime;
			}
			else
			{
				baseStartSystemNanoTime = SystemNanoTime;
				baseStartSystemMilliTime = SystemMilliTime;
			}

			this.numerator = numerator;
			this.denominator = denominator;

			baseSystemNanoTime = base.SystemNanoTime;
			baseSystemMilliTime = base.SystemMilliTime;
		}

		private long adaptToSpeed(long value)
		{
			return (value * numerator) / denominator;
		}

		protected internal override long SystemNanoTime
		{
			get
			{
				long systemNanoTime = base.SystemNanoTime;
				long deltaNanoTime = systemNanoTime - baseSystemNanoTime;
    
				return baseStartSystemNanoTime + adaptToSpeed(deltaNanoTime);
			}
		}

		protected internal override long SystemMilliTime
		{
			get
			{
				long systemMilliTime = base.SystemMilliTime;
				long deltaMilliTime = systemMilliTime - baseSystemMilliTime;
    
				return baseStartSystemMilliTime + adaptToSpeed(deltaMilliTime);
			}
		}
	}

}