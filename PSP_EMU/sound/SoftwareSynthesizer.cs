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
//	import static pspsharp.HLE.modules.sceSasCore.PSP_SAS_OUTPUTMODE_STEREO;

	//using Logger = org.apache.log4j.Logger;

	using Modules = pspsharp.HLE.Modules;
	using sceSasCore = pspsharp.HLE.modules.sceSasCore;

	public class SoftwareSynthesizer
	{
		//public static Logger log = Logger.getLogger("sound");
		private SoundVoice voice;
		private ISampleSource sampleSource;
		private const int defaultDelay = 32;

		public SoftwareSynthesizer(SoundVoice voice)
		{
			this.voice = voice;
		}

		public virtual ISampleSource SampleSource
		{
			get
			{
				if (sampleSource == null || voice.Changed)
				{
					voice.Changed = false;
    
					if (voice.AtracId != null)
					{
						sampleSource = new SampleSourceAtrac3(voice.AtracId);
					}
					else if (voice.PcmAddress != 0)
					{
						sampleSource = new SampleSourcePCM(voice, voice.PcmAddress, voice.PcmSize, voice.LoopMode);
						if (Modules.sceSasCoreModule.OutputMode == PSP_SAS_OUTPUTMODE_STEREO)
						{
							// Convert mono VAG to stereo
							sampleSource = new SampleSourceMono(sampleSource);
						}
					}
					else if (voice.VAGAddress != 0)
					{
						sampleSource = new SampleSourceVAG(voice, voice.VAGAddress, voice.VAGSize, voice.LoopMode != sceSasCore.PSP_SAS_LOOP_MODE_OFF);
						if (Modules.sceSasCoreModule.OutputMode == PSP_SAS_OUTPUTMODE_STEREO)
						{
							// Convert mono VAG to stereo
							sampleSource = new SampleSourceMono(sampleSource);
						}
					}
					else
					{
						sampleSource = new SampleSourceEmpty();
					}
    
					// Modify the sample according to the pitch (even if we use the default pitch as it can change on the fly)
					sampleSource = new SampleSourceWithPitch(sampleSource, voice);
    
					sampleSource = new SampleSourceWithADSR(sampleSource, voice, voice.Envelope);
    
					// PSP implementation always adds 32 samples delay before actually starting
					if (defaultDelay > 0)
					{
						sampleSource = new SampleSourceWithDelay(sampleSource, defaultDelay);
					}
				}
    
				return sampleSource;
			}
		}
	}

}