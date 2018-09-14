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

	using IState = pspsharp.state.IState;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class Clock : IState
	{
		private const int STATE_VERSION = 0;
		private long baseNanos;
		private long pauseNanos;
		private long baseTimeMillis;
		private bool isPaused;

		public Clock()
		{
			reset();
		}

		public Clock(Clock clock)
		{
			if (clock != null)
			{
				this.baseNanos = clock.baseNanos;
				this.pauseNanos = clock.pauseNanos;
				this.baseTimeMillis = clock.baseTimeMillis;
				this.isPaused = clock.isPaused;
			}
			else
			{
				reset();
			}
		}

		public virtual void pause()
		{
			lock (this)
			{
				if (!isPaused)
				{
					pauseNanos = SystemNanoTime;
					isPaused = true;
				}
			}
		}

		public virtual void resume()
		{
			lock (this)
			{
				if (isPaused)
				{
					// Do not take into account the elapsed time between pause() & resume()
					baseNanos += SystemNanoTime - pauseNanos;
					isPaused = false;
				}
			}
		}

		public virtual void reset()
		{
			lock (this)
			{
				baseNanos = SystemNanoTime;
				baseTimeMillis = SystemMilliTime;
        
				// Start with a paused Clock
				pauseNanos = baseNanos;
				isPaused = true;
			}
		}

		public virtual long nanoTime()
		{
			long now;

			if (isPaused)
			{
				now = pauseNanos;
			}
			else
			{
				now = SystemNanoTime;
			}

			return now - baseNanos;
		}

		public virtual long milliTime()
		{
			return nanoTime() / 1000000;
		}

		public virtual long microTime()
		{
			return nanoTime() / 1000;
		}

		public virtual long currentTimeMillis()
		{
			return baseTimeMillis + milliTime();
		}

		public virtual TimeNanos currentTimeNanos()
		{
			long nanoTime = this.nanoTime();
			long currentTimeMillis = baseTimeMillis + (nanoTime / 1000000);

			// Be careful that subsequent calls always return ascending values
			TimeNanos timeNano = new TimeNanos();
			timeNano.nanos = (int)(nanoTime % 1000);
			timeNano.micros = (int)((nanoTime / 1000) % 1000);
			timeNano.millis = (int)(currentTimeMillis % 1000);
			timeNano.seconds = (int)(currentTimeMillis / 1000);

			return timeNano;
		}

		protected internal virtual long SystemNanoTime
		{
			get
			{
				return System.nanoTime();
			}
		}

		protected internal virtual long SystemMilliTime
		{
			get
			{
				return DateTimeHelper.CurrentUnixTimeMillis();
			}
		}

		public class TimeNanos
		{
			public int seconds;
			public int millis;
			public int micros;
			public int nanos;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			long systemNanoTimeAtWrite = stream.readLong();

			// Do not take into account the elapsed time between write() & read()
			long delta = SystemNanoTime - systemNanoTimeAtWrite;
			baseNanos += delta;
			pauseNanos += delta;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeLong(SystemNanoTime);
		}
	}

}