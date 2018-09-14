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
namespace pspsharp.sound
{
	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class SampleSourceWithDelay : ISampleSource
	{
		private ISampleSource sampleSource;
		private int delay;
		private int sampleIndex;

		public SampleSourceWithDelay(ISampleSource sampleSource, int delay)
		{
			this.sampleSource = sampleSource;
			this.delay = delay;
		}

		public virtual int NextSample
		{
			get
			{
				int sample;
    
				if (sampleIndex < delay)
				{
					sample = 0;
					sampleIndex++;
				}
				else
				{
					sample = sampleSource.NextSample;
				}
    
				return sample;
			}
		}

		public virtual void resetToStart()
		{
			sampleSource.resetToStart();
			sampleIndex = 0;
		}

		public virtual bool Ended
		{
			get
			{
				return sampleSource.Ended;
			}
		}
	}

}