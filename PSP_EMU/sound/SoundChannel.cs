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
namespace pspsharp.sound
{

	using sceAudio = pspsharp.HLE.modules.sceAudio;

	using Logger = org.apache.log4j.Logger;
	using LWJGLException = org.lwjgl.LWJGLException;
	using AL = org.lwjgl.openal.AL;
	using AL10 = org.lwjgl.openal.AL10;
	using AL11 = org.lwjgl.openal.AL11;

	public class SoundChannel
	{
		private static Logger log = sceAudio.log;
		private static volatile bool isExit = false;
		public const int FORMAT_MONO = 0x10;
		public const int FORMAT_STEREO = 0x00;
		//
		// The PSP is using a buffer equal to the sampleSize.
		// However, the audio data is not always streamed as fast on pspsharp as on
		// a real PSP which can lead to buffer underflows,
		// causing discontinuities in the audio that are perceived as "clicks".
		//
		// So, we allocate several buffers of sampleSize, just enough to store
		// a given amount of audio time.
		// This has the disadvantage to introduce a small delay when playing
		// a new sound: a PSP application is typically sending continuously
		// sound data, even when nothing can be heard ("0" values are sent).
		// And we have first to play these buffered blanks before hearing
		// the real sound itself.
		// E.g. BUFFER_SIZE_IN_MILLIS = 100 gives a 0.1 second delay.
		private const int BUFFER_SIZE_IN_MILLIS = 100;
		public const int MAX_VOLUME = 0x8000;
		private const int DEFAULT_VOLUME = MAX_VOLUME;
		private const int DEFAULT_SAMPLE_RATE = 44100;
		private SoundBufferManager soundBufferManager;
		private int index;
		private bool reserved;
		private int leftVolume;
		private int rightVolume;
		private int alSource;
		private int sampleRate;
		private int sampleLength;
		private int format;
		private int numberBlockingBuffers;
		private int minimumNumberBuffers;
		private bool busy;

		public static void init()
		{
			if (!AL.Created)
			{
				try
				{
					AL.create();
					isExit = false;
				}
				catch (LWJGLException e)
				{
					log.error(e);
				}
			}
		}

		public static void exit()
		{
			if (AL.Created)
			{
				isExit = true;
				AL.destroy();
			}
		}

		public SoundChannel(int index)
		{
			soundBufferManager = SoundBufferManager.Instance;
			this.index = index;
			reserved = false;
			leftVolume = DEFAULT_VOLUME;
			rightVolume = DEFAULT_VOLUME;
			alSource = AL10.alGenSources();
			sampleRate = DEFAULT_SAMPLE_RATE;
			updateNumberBlockingBuffers();

			AL10.alSourcei(alSource, AL10.AL_LOOPING, AL10.AL_FALSE);
		}

		private void updateNumberBlockingBuffers()
		{
			if (SampleLength > 0)
			{
				// Compute the number of buffers required to store the required
				// amount of audio time
				float bufferSizeInSamples = SampleRate * BUFFER_SIZE_IN_MILLIS / 1000.0f;
				numberBlockingBuffers = System.Math.Round(bufferSizeInSamples / SampleLength);
			}

			// At least 1 blocking buffer
			numberBlockingBuffers = System.Math.Max(numberBlockingBuffers, 1);

			// For very small sample length, wait for a minimum number of buffers
			// before starting playing the audio otherwise, small cracks can be produced.
			if (SampleLength <= 0x40)
			{
				minimumNumberBuffers = 10;
			}
			else
			{
				minimumNumberBuffers = 0;
			}
		}

		public virtual int Index
		{
			get
			{
				return index;
			}
		}

		public virtual bool Reserved
		{
			get
			{
				return reserved;
			}
			set
			{
				this.reserved = value;
			}
		}


		public virtual int LeftVolume
		{
			get
			{
				return leftVolume;
			}
			set
			{
				this.leftVolume = value;
			}
		}


		public virtual int RightVolume
		{
			get
			{
				return rightVolume;
			}
			set
			{
				this.rightVolume = value;
			}
		}


		public virtual int Volume
		{
			set
			{
				LeftVolume = value;
				RightVolume = value;
			}
		}

		public virtual int SampleLength
		{
			get
			{
				return sampleLength;
			}
			set
			{
				if (this.sampleLength != value)
				{
					this.sampleLength = value;
					updateNumberBlockingBuffers();
				}
			}
		}


		public virtual int Format
		{
			get
			{
				return format;
			}
			set
			{
				this.format = value;
			}
		}


		public virtual bool FormatStereo
		{
			get
			{
				return (format & FORMAT_MONO) == FORMAT_STEREO;
			}
		}

		public virtual bool FormatMono
		{
			get
			{
				return (format & FORMAT_MONO) == FORMAT_MONO;
			}
		}

		public virtual int SampleRate
		{
			get
			{
				return sampleRate;
			}
			set
			{
				if (this.sampleRate != value)
				{
					this.sampleRate = value;
					updateNumberBlockingBuffers();
				}
			}
		}


		private void alSourcePlay()
		{
			int state = AL10.alGetSourcei(alSource, AL10.AL_SOURCE_STATE);
			if (state != AL10.AL_PLAYING)
			{
				if (minimumNumberBuffers <= 0 || WaitingBuffers >= minimumNumberBuffers)
				{
					AL10.alSourcePlay(alSource);
				}
			}
		}

		private void alSourceQueueBuffer(sbyte[] buffer)
		{
			int alBuffer = soundBufferManager.Buffer;
			ByteBuffer directBuffer = soundBufferManager.getDirectBuffer(buffer.Length);
			directBuffer.clear();
			directBuffer.limit(buffer.Length);
			directBuffer.put(buffer);
			directBuffer.rewind();
			int alFormat = FormatStereo ? AL10.AL_FORMAT_STEREO16 : AL10.AL_FORMAT_MONO16;
			AL10.alBufferData(alBuffer, alFormat, directBuffer, SampleRate);
			AL10.alSourceQueueBuffers(alSource, alBuffer);
			soundBufferManager.releaseDirectBuffer(directBuffer);
			alSourcePlay();
			checkFreeBuffers();

			if (log.DebugEnabled)
			{
				log.debug(string.Format("alSourceQueueBuffer buffer={0:D}, {1}", alBuffer, ToString()));
			}
		}

		public virtual void checkFreeBuffers()
		{
			soundBufferManager.checkFreeBuffers(alSource);
		}

		public virtual void release()
		{
			AL10.alSourceStop(alSource);
			checkFreeBuffers();
		}

		public virtual void play(sbyte[] buffer)
		{
			alSourceQueueBuffer(buffer);
		}

		private int WaitingBuffers
		{
			get
			{
				checkFreeBuffers();
    
				return AL10.alGetSourcei(alSource, AL10.AL_BUFFERS_QUEUED);
			}
		}

		private int SourceSampleOffset
		{
			get
			{
				int sampleOffset = AL10.alGetSourcei(alSource, AL11.AL_SAMPLE_OFFSET);
				if (FormatStereo)
				{
					sampleOffset /= 2;
				}
    
				return sampleOffset;
			}
		}

		public virtual bool OutputBlocking
		{
			get
			{
				if (isExit)
				{
					return true;
				}
    
				return WaitingBuffers >= numberBlockingBuffers;
			}
		}

		public virtual bool Drained
		{
			get
			{
				if (Ended)
				{
					return true;
				}
    
				if (WaitingBuffers > 1)
				{
					return false;
				}
    
				return true;
			}
		}

		public virtual int getUnblockOutputDelayMicros(bool waitForCompleteDrain)
		{
			// Return the delay required for the processing of the playing buffer
			if (isExit || Ended)
			{
				return 0;
			}

			int samples;
			if (waitForCompleteDrain)
			{
				samples = DrainLength;
			}
			else
			{
				samples = SampleLength - SourceSampleOffset;
			}
			float delaySecs = samples / (float) SampleRate;
			int delayMicros = (int)(delaySecs * 1000000);

			return delayMicros;
		}

		public virtual int DrainLength
		{
			get
			{
				int waitingBuffers = WaitingBuffers;
				if (waitingBuffers > 0)
				{
					// getWaitingBuffers also returns the currently playing buffer,
					// do not count it
					waitingBuffers--;
				}
				int restLength = waitingBuffers * SampleLength;
    
				return restLength;
			}
		}

		public virtual int RestLength
		{
			get
			{
				int restLength = DrainLength;
				if (!Ended)
				{
					restLength += SampleLength - SourceSampleOffset;
				}
    
				return restLength;
			}
		}

		public virtual bool Ended
		{
			get
			{
				checkFreeBuffers();
    
				int state = AL10.alGetSourcei(alSource, AL10.AL_SOURCE_STATE);
				if (state == AL10.AL_PLAYING)
				{
					return false;
				}
    
				return true;
			}
		}

		public static short adjustSample(short sample, int volume)
		{
			return (short)((((int) sample) * volume) >> 15);
		}

		public static void storeSample(short sample, sbyte[] data, int index)
		{
			data[index] = (sbyte) sample;
			data[index + 1] = (sbyte)(sample >> 8);
		}

		public virtual bool Busy
		{
			get
			{
				return busy;
			}
			set
			{
				this.busy = value;
			}
		}


		public override string ToString()
		{
			StringBuilder s = new StringBuilder();

			s.Append(string.Format("SoundChannel[{0:D}](", index));
			if (!isExit)
			{
				s.Append(string.Format("sourceSampleOffset={0:D}", SourceSampleOffset));
				s.Append(string.Format(", restLength={0:D}", RestLength));
				s.Append(string.Format(", buffers queued={0:D}", WaitingBuffers));
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: s.append(String.format(", isOutputBlock=%b", isOutputBlocking()));
				s.Append(string.Format(", isOutputBlock=%b", OutputBlocking));
				s.Append(string.Format(", {0}", FormatStereo ? "Stereo" : "Mono"));
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: s.append(String.format(", reserved=%b", reserved));
				s.Append(string.Format(", reserved=%b", reserved));
				s.Append(string.Format(", sampleLength={0:D}", SampleLength));
				s.Append(string.Format(", sampleRate={0:D}", SampleRate));
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: s.append(String.format(", busy=%b", busy));
				s.Append(string.Format(", busy=%b", busy));
			}
			s.Append(")");

			return s.ToString();
		}
	}
}