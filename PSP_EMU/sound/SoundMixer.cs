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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.sound.SoundChannel.MAX_VOLUME;

	//using Logger = org.apache.log4j.Logger;

	using Modules = pspsharp.HLE.Modules;
	using Audio = pspsharp.hardware.Audio;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;

	public class SoundMixer
	{
		private static Logger log = SoftwareSynthesizer.log;
		private SoundVoice[] voices;
		private SoftwareSynthesizer[] synthesizers;

		public SoundMixer(SoundVoice[] voices)
		{
			this.voices = voices;

			synthesizers = new SoftwareSynthesizer[voices.Length];
			for (int i = 0; i < voices.Length; i++)
			{
				synthesizers[i] = new SoftwareSynthesizer(voices[i]);
			}
		}

		private static short clampSample(int sample)
		{
			if (sample < short.MinValue)
			{
				return short.MinValue;
			}
			else if (sample > short.MaxValue)
			{
				return short.MaxValue;
			}

			return (short) sample;
		}

		private void mixStereo(int[] stereoSamples, ISampleSource sampleSource, int startIndex, int Length, int leftVol, int rightVol)
		{
			int endIndex = startIndex + Length;
			if (startIndex == 0)
			{
				sampleSource.resetToStart();
			}
			for (int i = startIndex, j = 0; i < endIndex; i++, j += 2)
			{
				int sample = sampleSource.NextSample;
				stereoSamples[j] += SoundChannel.adjustSample(getSampleLeft(sample), leftVol);
				stereoSamples[j + 1] += SoundChannel.adjustSample(getSampleRight(sample), rightVol);
			}
		}

		private void mixMono(int[] monoSamples, ISampleSource sampleSource, int startIndex, int Length, int monoVol)
		{
			int endIndex = startIndex + Length;
			if (startIndex == 0)
			{
				sampleSource.resetToStart();
			}
			for (int i = startIndex, j = 0; i < endIndex; i++, j++)
			{
				int sample = sampleSource.NextSample;
				monoSamples[j] += SoundChannel.adjustSample(getSampleLeft(sample), monoVol);
			}
		}

		private void copyStereoSamplesToMem(int[] mixedSamples, int addr, int samples, int leftVol, int rightVol, bool writeSamples)
		{
			// Adjust the volume according to the global volume settings
			leftVol = Audio.getVolume(leftVol);
			rightVol = Audio.getVolume(rightVol);

			if (!writeSamples)
			{
				// If the samples have not been changed and the volume settings
				// would also not adjust the samples, no need to copy them back to memory.
				if (leftVol == MAX_VOLUME && rightVol == MAX_VOLUME)
				{
					return;
				}
			}

			int lengthInBytes = mixedSamples.Length << 1;
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(addr, lengthInBytes, 4);
			for (int i = 0, j = 0; i < samples; i++, j += 2)
			{
				short sampleLeft = clampSample(mixedSamples[j]);
				short sampleRight = clampSample(mixedSamples[j + 1]);
				sampleLeft = SoundChannel.adjustSample(sampleLeft, leftVol);
				sampleRight = SoundChannel.adjustSample(sampleRight, rightVol);
				int sampleStereo = getSampleStereo(sampleLeft, sampleRight);
				memoryWriter.writeNext(sampleStereo);
			}
			memoryWriter.flush();
		}

		private void copyMonoSamplesToMem(int[] mixedSamples, int addr, int samples, int monoVol, bool writeSamples)
		{
			// Adjust the volume according to the global volume settings
			monoVol = Audio.getVolume(monoVol);

			if (!writeSamples)
			{
				// If the samples have not been changed and the volume settings
				// would also not adjust the samples, no need to copy them back to memory.
				if (monoVol == MAX_VOLUME)
				{
					return;
				}
			}

			int lengthInBytes = mixedSamples.Length << 1;
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(addr, lengthInBytes, 2);
			for (int i = 0, j = 0; i < samples; i++, j++)
			{
				short sampleMono = clampSample(mixedSamples[j]);
				sampleMono = SoundChannel.adjustSample(sampleMono, monoVol);
				memoryWriter.writeNext(sampleMono & 0xFFFF);
			}
			memoryWriter.flush();
		}

		private void mix(int[] mixedSamples, int addr, int samples, int leftVol, int rightVol, bool writeSamples)
		{
			bool isStereo = Modules.sceSasCoreModule.OutputMode == PSP_SAS_OUTPUTMODE_STEREO;

			for (int i = 0; i < voices.Length; i++)
			{
				SoundVoice voice = voices[i];

				if (voice.Playing && !voice.Paused)
				{
					ISampleSource sampleSource = synthesizers[i].SampleSource;
					int playSample = voice.PlaySample;
					if (sampleSource.Ended)
					{
						// End of voice sample reached
						if (log.TraceEnabled)
						{
							log.trace(string.Format("Reaching end of sample source for voice {0}", voice));
						}
						voice.Playing = false;
					}
					else
					{
						if (isStereo)
						{
							mixStereo(mixedSamples, sampleSource, playSample, samples, voice.LeftVolume, voice.RightVolume);
						}
						else
						{
							mixMono(mixedSamples, sampleSource, playSample, samples, voice.LeftVolume);
						}
						writeSamples = true;

						voice.PlaySample = 1;
					}
				}
			}

			if (isStereo)
			{
				copyStereoSamplesToMem(mixedSamples, addr, samples, leftVol, rightVol, writeSamples);
			}
			else
			{
				copyMonoSamplesToMem(mixedSamples, addr, samples, leftVol, writeSamples);
			}
		}

		/// <summary>
		/// Synthesizing audio function. </summary>
		/// <param name="addr"> Output address for the PCM data (must be 64-byte aligned). </param>
		/// <param name="samples"> Number of samples returned. </param>
		public virtual void synthesize(int addr, int samples)
		{
			int[] mixedSamples = new int[samples * 2];
			Arrays.Fill(mixedSamples, 0);

			mix(mixedSamples, addr, samples, MAX_VOLUME, MAX_VOLUME, true);
		}

		/// <summary>
		/// Synthesizing audio function with mix. </summary>
		/// <param name="addr"> Input and output address for the PCM data (must be 64-byte aligned). </param>
		/// <param name="samples"> Number of samples returned. </param>
		/// <param name="leftVol"> the volume of the left channel for modulating the input PCM data.
		///                This volume is not affecting the currently played samples. </param>
		/// <param name="rightVol"> the volume of the right channel for modulating the input PCM data.
		///                 This volume is not affecting the currently played samples. </param>
		public virtual void synthesizeWithMix(int addr, int samples, int leftVol, int rightVol)
		{
			int[] mixedSamples = new int[samples * 2];

			// Read the input buffer into mixedSamples.
			// Check first for simple cases...
			if (leftVol == 0 && rightVol == 0)
			{
				// Do not mix with the input buffer
				Arrays.Fill(mixedSamples, 0);
			}
			else if (leftVol == MAX_VOLUME && rightVol == MAX_VOLUME)
			{
				// Mix with the input buffer with no volume change
				int lengthInBytes = mixedSamples.Length * 2;
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(addr, lengthInBytes, 2);
				for (int i = 0; i < mixedSamples.Length; i++)
				{
					mixedSamples[i] = (short) memoryReader.readNext();
				}
			}
			else
			{
				// Mix with the input buffer with a volume adjustment
				int lengthInBytes = mixedSamples.Length * 2;
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(addr, lengthInBytes, 2);
				for (int i = 0; i < samples; i++)
				{
					short sampleLeft = (short) memoryReader.readNext();
					short sampleRight = (short) memoryReader.readNext();
					sampleLeft = SoundChannel.adjustSample(sampleLeft, leftVol);
					sampleRight = SoundChannel.adjustSample(sampleRight, rightVol);
					mixedSamples[i * 2] = sampleLeft;
					mixedSamples[i * 2 + 1] = sampleRight;
				}
			}

			mix(mixedSamples, addr, samples, MAX_VOLUME, MAX_VOLUME, false);
		}

		public static short getSampleLeft(int sample)
		{
			return unchecked((short)(sample & 0x0000FFFF));
		}

		public static short getSampleRight(int sample)
		{
			return (short)(sample >> 16);
		}

		public static int getSampleStereo(short left, short right)
		{
			return (left & 0x0000FFFF) | ((right & 0x0000FFFF) << 16);
		}
	}

}