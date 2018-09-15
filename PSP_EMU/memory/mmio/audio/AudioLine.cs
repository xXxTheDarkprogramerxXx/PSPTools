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
namespace pspsharp.memory.mmio.audio
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceAudio.PSP_AUDIO_VOLUME_MAX;


	//using Logger = org.apache.log4j.Logger;
	using AL10 = org.lwjgl.openal.AL10;

	using SoundBufferManager = pspsharp.sound.SoundBufferManager;
	using IState = pspsharp.state.IState;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class AudioLine : IState
	{
		//public static Logger log = MMIOHandlerAudio.log;
		private const int STATE_VERSION = 0;
		private SoundBufferManager soundBufferManager;
		private int alSource;
		private int frequency = 44100;

		public AudioLine()
		{
			soundBufferManager = SoundBufferManager.Instance;
			alSource = AL10.alGenSources();
			AL10.alSourcei(alSource, AL10.AL_LOOPING, AL10.AL_FALSE);
		}

		private void alSourcePlay()
		{
			int state = AL10.alGetSourcei(alSource, AL10.AL_SOURCE_STATE);
			if (state != AL10.AL_PLAYING)
			{
				AL10.alSourcePlay(alSource);
			}
		}

		private void checkFreeBuffers()
		{
			soundBufferManager.checkFreeBuffers(alSource);
		}

		public virtual int Frequency
		{
			set
			{
				this.frequency = value;
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("AudioLine frequency=0x{0:X}", value));
				}
			}
		}

		public virtual int Volume
		{
			set
			{
				float gain = value / (float) PSP_AUDIO_VOLUME_MAX;
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("AudioLine volume=0x{0:X}, gain={1:F}", value, gain));
				}
				AL10.alSourcef(alSource, AL10.AL_GAIN, gain);
			}
		}

		public virtual void writeAudioData(int[] data, int offset, int Length)
		{
			int audioBytesLength = Length * 4;
			ByteBuffer directBuffer = soundBufferManager.getDirectBuffer(audioBytesLength);
			directBuffer.order(ByteOrder.LITTLE_ENDIAN);
			directBuffer.clear();
			directBuffer.limit(audioBytesLength);
			directBuffer.asIntBuffer().put(data, offset, Length);
			directBuffer.rewind();

			int alBuffer = soundBufferManager.Buffer;
			AL10.alBufferData(alBuffer, AL10.AL_FORMAT_STEREO16, directBuffer, frequency);
			AL10.alSourceQueueBuffers(alSource, alBuffer);
			soundBufferManager.releaseDirectBuffer(directBuffer);

			alSourcePlay();
			checkFreeBuffers();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			frequency = stream.readInt();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(frequency);
		}
	}

}