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
	using sceSasCore = pspsharp.HLE.modules.sceSasCore;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class SampleSourceWithPitch : ISampleSource
	{
		private ISampleSource sampleSource;
		private SoundVoice voice;
		private int pitchRest;
		private int currentSample;

		public SampleSourceWithPitch(ISampleSource sampleSource, SoundVoice voice)
		{
			this.sampleSource = sampleSource;
			this.voice = voice;
		}

		private int Pitch
		{
			get
			{
				return voice.Pitch;
			}
		}

		public virtual int NextSample
		{
			get
			{
				while (pitchRest <= 0)
				{
					currentSample = sampleSource.NextSample;
					pitchRest += sceSasCore.PSP_SAS_PITCH_BASE;
				}
				pitchRest -= Pitch;
    
				return currentSample;
			}
		}

		public virtual void resetToStart()
		{
			sampleSource.resetToStart();
			pitchRest = 0;
		}

		public virtual bool Ended
		{
			get
			{
				return sampleSource.Ended;
			}
		}

		public override string ToString()
		{
			return string.Format("SampleSourceWithPitch[pitchRest=0x{0:X}, pitch=0x{1:X}, {2}]", pitchRest, Pitch, sampleSource.ToString());
		}
	}

}