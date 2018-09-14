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
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class SampleSourcePCM : ISampleSource
	{
		private SoundVoice voice;
		private int addr;
		private int size;
		private IMemoryReader memoryReader;
		private int sampleIndex;
		private int samples;
		private bool looping;

		public SampleSourcePCM(SoundVoice voice, int addr, int samples, int loopMode)
		{
			this.voice = voice;
			this.addr = addr;
			this.samples = samples;
			size = samples << 1;
			sampleIndex = samples;

			looping = loopMode >= 0;
		}

		public virtual int NextSample
		{
			get
			{
				if (sampleIndex >= samples)
				{
					if (!voice.On)
					{
						// Voice is off, stop playing
						looping = false;
						return 0;
					}
					resetToStart();
				}
				sampleIndex++;
    
				return memoryReader.readNext();
			}
		}

		public virtual void resetToStart()
		{
			memoryReader = MemoryReader.getMemoryReader(addr, size, 2);
			sampleIndex = 0;
		}

		public virtual bool Ended
		{
			get
			{
				if (looping)
				{
					// Never ending
					return false;
				}
				return sampleIndex >= samples;
			}
		}
	}

}