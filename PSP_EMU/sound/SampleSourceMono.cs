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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.sound.SoundMixer.getSampleLeft;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.sound.SoundMixer.getSampleStereo;

	/// <summary>
	/// @author gid15
	/// 
	/// Converts a mono sample source to the requested stereo.
	/// </summary>
	public class SampleSourceMono : ISampleSource
	{
		private ISampleSource sampleSource;

		public SampleSourceMono(ISampleSource sampleSource)
		{
			this.sampleSource = sampleSource;
		}

		public virtual int NextSample
		{
			get
			{
				short mono = getSampleLeft(sampleSource.NextSample);
    
				return getSampleStereo(mono, mono);
			}
		}

		public virtual void resetToStart()
		{
			sampleSource.resetToStart();
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