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
namespace pspsharp.HLE.modules
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using Audio = pspsharp.hardware.Audio;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using AudioBlockingOutputAction = pspsharp.sound.AudioBlockingOutputAction;
	using SoundChannel = pspsharp.sound.SoundChannel;
	using Utilities = pspsharp.util.Utilities;

	//using Logger = org.apache.log4j.Logger;
	using BufferUtils = org.lwjgl.BufferUtils;
	using AL10 = org.lwjgl.openal.AL10;
	using ALC10 = org.lwjgl.openal.ALC10;
	using ALC11 = org.lwjgl.openal.ALC11;
	using ALCdevice = org.lwjgl.openal.ALCdevice;

	public class sceAudio : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceAudio");
		public sbyte[] audioData;

		public override void start()
		{
			SoundChannel.init();

			// The audio driver is capable of handling PCM and VAG (ADPCM) playback,
			// but it uses the same channels for this processing.
			// E.g.: Use channels 0 to 4 to playback 4 VAG files or use channels 0 to 2
			// to playback raw PCM data.
			// Note: Currently, working with pspPCMChannels only is enough.
			pspPCMChannels = new SoundChannel[PSP_AUDIO_CHANNEL_MAX];
			for (int channel = 0; channel < pspPCMChannels.Length; channel++)
			{
				pspPCMChannels[channel] = new SoundChannel(channel);
			}
			pspSRC1Channel = new SoundChannel(8); // Use a special channel 8 to handle SRC functions (first channel).
			pspSRC2Channel = new SoundChannel(9); // Use a special channel 9 to handle SRC functions (second channel).

			base.start();
		}

		public override void stop()
		{
			if (inputDevice != null)
			{
				ALC11.alcCaptureCloseDevice(inputDevice);
				inputDevice = null;
			}
			inputDeviceInitialized = false;
			captureBuffer = null;

			base.stop();
		}

		public const int PSP_AUDIO_VOLUME_MAX = 0x8000;
		protected internal const int PSP_AUDIO_CHANNEL_MAX = 8;
		protected internal const int PSP_AUDIO_SAMPLE_MIN = 64;
		protected internal const int PSP_AUDIO_SAMPLE_MAX = 65472;
		protected internal const int PSP_AUDIO_FORMAT_STEREO = 0;
		protected internal const int PSP_AUDIO_FORMAT_MONO = 0x10;

		protected internal SoundChannel[] pspPCMChannels;
		// Two different threads can output audio on the SRC channel
		// without interfering with each others.
		protected internal SoundChannel pspSRC1Channel;
		protected internal SoundChannel pspSRC2Channel;

		protected internal ALCdevice inputDevice;
		protected internal ByteBuffer captureBuffer;
		protected internal IntBuffer samplesBuffer;
		protected internal bool inputDeviceInitialized;

		protected internal class AudioBlockingInputAction : IAction
		{
			internal int threadId;
			internal int addr;
			internal int samples;
			internal int frequency;

			public AudioBlockingInputAction(int threadId, int addr, int samples, int frequency)
			{
				this.threadId = threadId;
				this.addr = addr;
				this.samples = samples;
				this.frequency = frequency;
			}

			public virtual void execute()
			{
				Modules.sceAudioModule.hleAudioBlockingInput(threadId, addr, samples, frequency);
			}
		}

		protected internal static int doAudioOutput(SoundChannel channel, int pvoid_buf)
		{
			int ret = -1;

			if (channel.Reserved)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("doAudioOutput({0}, 0x{1:X8})", channel.ToString(), pvoid_buf));
				}
				int bytesPerSample = channel.FormatStereo ? 4 : 2;
				int nbytes = bytesPerSample * channel.SampleLength;
				sbyte[] data = new sbyte[nbytes];

				IMemoryReader memoryReader = MemoryReader.getMemoryReader(pvoid_buf, nbytes, 2);
				if (channel.FormatMono)
				{
					int volume = Audio.getVolume(channel.LeftVolume);
					for (int i = 0; i < nbytes; i += 2)
					{
						short sample = (short) memoryReader.readNext();

						sample = SoundChannel.adjustSample(sample, volume);

						SoundChannel.storeSample(sample, data, i);
					}
				}
				else
				{
					int leftVolume = Audio.getVolume(channel.LeftVolume);
					int rightVolume = Audio.getVolume(channel.RightVolume);
					for (int i = 0; i < nbytes; i += 4)
					{
						short lsample = (short) memoryReader.readNext();
						short rsample = (short) memoryReader.readNext();

						lsample = SoundChannel.adjustSample(lsample, leftVolume);
						rsample = SoundChannel.adjustSample(rsample, rightVolume);

						SoundChannel.storeSample(lsample, data, i);
						SoundChannel.storeSample(rsample, data, i + 2);
					}
				}
				Modules.sceAudioModule.audioData = data;
				channel.play(data);
				ret = channel.SampleLength;
			}
			else
			{
				Console.WriteLine("doAudioOutput: channel " + channel.Index + " not reserved");
			}
			return ret;
		}

		protected internal static void blockThreadOutput(SoundChannel channel, int addr, int leftVolume, int rightVolume)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			blockThreadOutput(threadMan.CurrentThreadID, channel, addr, leftVolume, rightVolume);
			threadMan.hleBlockCurrentThread(SceKernelThreadInfo.JPCSP_WAIT_AUDIO);
			channel.Busy = true;
		}

		protected internal static void blockThreadOutput(int threadId, SoundChannel channel, int addr, int leftVolume, int rightVolume)
		{
			IAction action = new AudioBlockingOutputAction(threadId, channel, addr, leftVolume, rightVolume);
			int delayMicros = channel.getUnblockOutputDelayMicros(addr == 0);
			long schedule = Emulator.Clock.microTime() + delayMicros;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("blockThreadOutput micros={0:D}, schedule={1:D}", delayMicros, schedule));
			}
			Emulator.Scheduler.addAction(schedule, action);
		}

		public virtual void hleAudioBlockingOutput(int threadId, SoundChannel channel, int addr, int leftVolume, int rightVolume)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("hleAudioBlockingOutput {0}", channel.ToString()));
			}

			if (addr == 0)
			{
				// If another thread is also sending audio data on this channel,
				// do not wait for the channel to be drained, unblock the thread now.
				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				SceKernelThreadInfo thread = threadMan.getThreadById(threadId);
				if (thread != null)
				{
					thread.cpuContext._v0 = channel.SampleLength;
					threadMan.hleUnblockThread(threadId);
				}
				channel.Busy = false;
			}
			else if (!channel.OutputBlocking)
			{
				ThreadManForUser threadMan = Modules.ThreadManForUserModule;
				SceKernelThreadInfo thread = threadMan.getThreadById(threadId);
				if (thread != null)
				{
					changeChannelVolume(channel, leftVolume, rightVolume);
					int ret = doAudioOutput(channel, addr);
					thread.cpuContext._v0 = ret;
					threadMan.hleUnblockThread(threadId);
				}
				channel.Busy = false;
			}
			else
			{
				blockThreadOutput(threadId, channel, addr, leftVolume, rightVolume);
			}
		}

		protected internal static int changeChannelVolume(SoundChannel channel, int leftvol, int rightvol)
		{
			int ret = -1;

			if (channel.Reserved)
			{
				// Negative volume means no change
				if (leftvol >= 0)
				{
					channel.LeftVolume = leftvol;
				}
				if (rightvol >= 0)
				{
					channel.RightVolume = rightvol;
				}
				ret = 0;
			}

			return ret;
		}

		protected internal virtual int hleAudioGetChannelRestLength(SoundChannel channel)
		{
			int len = channel.RestLength;

			// To avoid small "clicks" in the sound, simulate a rest Length of 0
			// when approaching the end of the buffered samples.
			// 2048 is an empirical value.
			if (len > 0 && len <= 2048)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleAudioGetChannelRestLength truncating rest Length {0:D} to 0", len));
				}
				len = 0;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("hleAudioGetChannelRestLength({0:D}) = {1:D}", channel.Index, len));
			}

			return len;
		}

		protected internal virtual SoundChannel FreeSRCChannel
		{
			get
			{
				if (!pspSRC1Channel.Busy)
				{
					return pspSRC1Channel;
				}
				if (!pspSRC2Channel.Busy)
				{
					return pspSRC2Channel;
				}
				return null;
			}
		}

		protected internal virtual int hleAudioSRCChReserve(int sampleCount, int freq, int format)
		{
			if (pspSRC1Channel.Reserved)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleAudioSRCChReserve returning ERROR_AUDIO_CHANNEL_ALREADY_RESERVED"));
				}
				return SceKernelErrors.ERROR_AUDIO_CHANNEL_ALREADY_RESERVED;
			}

			// Reserve both SRC channels
			pspSRC1Channel.SampleRate = freq;
			pspSRC1Channel.Reserved = true;
			pspSRC1Channel.SampleLength = sampleCount;
			pspSRC1Channel.Format = format;

			pspSRC2Channel.SampleRate = freq;
			pspSRC2Channel.Reserved = true;
			pspSRC2Channel.SampleLength = sampleCount;
			pspSRC2Channel.Format = format;

			return 0;
		}

		public virtual int checkChannel(int channel)
		{
			if (channel < 0 || channel >= PSP_AUDIO_CHANNEL_MAX)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Invalid channel number {0:D}", channel));
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_AUDIO_INVALID_CHANNEL);
			}

			return channel;
		}

		public virtual int checkReservedChannel(int channel)
		{
			channel = checkChannel(channel);
			if (!pspPCMChannels[channel].Reserved)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Channel not reserved {0:D}", channel));
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_AUDIO_CHANNEL_NOT_INIT);
			}

			return channel;
		}

		public virtual int checkSampleCount(int sampleCount)
		{
			if (sampleCount <= 0 || sampleCount > 0xFFC0 || (sampleCount & 0x3F) != 0)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Invalid sampleCount 0x{0:X}", sampleCount));
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_AUDIO_OUTPUT_SAMPLE_DATA_SIZE_NOT_ALIGNED);
			}

			return sampleCount;
		}

		public virtual int checkSmallSampleCount(int sampleCount)
		{
			if (sampleCount < 17 || sampleCount >= 4095 + 17)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Invalid small sampleCount 0x{0:X}", sampleCount));
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_AUDIO_OUTPUT_SAMPLE_DATA_SIZE_NOT_ALIGNED);
			}

			return sampleCount;
		}

		public virtual int checkReserveSampleCount(int sampleCount)
		{
			if (sampleCount < 17 || sampleCount >= 4095 + 17)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Invalid reserve sampleCount 0x{0:X}", sampleCount));
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_SIZE);
			}

			return sampleCount;
		}

		public virtual int checkVolume(int volume)
		{
			// Negative volume is allowed
			if (volume > 0xFFFF)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Invalid volume 0x{0:X}", volume));
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_AUDIO_INVALID_VOLUME);
			}

			return volume;
		}

		public virtual int checkVolume2(int volume)
		{
			if (volume < 0 || volume > 0xFFFFF)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Invalid volume 0x{0:X}", volume));
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_AUDIO_INVALID_VOLUME);
			}

			return volume;
		}

		public virtual int checkFormat(int format)
		{
			if (format != PSP_AUDIO_FORMAT_STEREO && format != PSP_AUDIO_FORMAT_MONO)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Invalid format 0x{0:X}", format));
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_AUDIO_INVALID_FORMAT);
			}

			return format;
		}

		public virtual int checkFrequency(int frequency)
		{
			// No change in frequency (e.g. default frequency 44100) is accepted
			if (pspSRC1Channel.SampleRate == frequency)
			{
				return frequency;
			}

			switch (frequency)
			{
				case 0:
				case 8000:
				case 11025:
				case 12000:
				case 16000:
				case 22050:
				case 24000:
				case 32000:
				case 48000:
					// OK
					break;
				default:
					throw new SceKernelErrorException(SceKernelErrors.ERROR_AUDIO_INVALID_FREQUENCY);
			}

			return frequency;
		}

		public virtual int checkChannelCount(int channelCount)
		{
			if (channelCount != 2)
			{
				if (channelCount == 4)
				{
					throw new SceKernelErrorException(SceKernelErrors.ERROR_NOT_IMPLEMENTED);
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_SIZE);
			}

			return channelCount;
		}

		protected internal virtual void hleAudioBlockingInput(int threadId, int addr, int samples, int frequency)
		{
			int availableSamples = hleAudioGetInputLength();
			if (log.TraceEnabled)
			{
				log.trace(string.Format("hleAudioBlockingInput available samples: {0:D} from {1:D}", availableSamples, samples));
			}

			int bufferBytes = samples << 1;
			if (inputDevice == null)
			{
				// No input device available, fake device input
				Memory.Instance.memset(addr, (sbyte) 0, bufferBytes);
				Modules.ThreadManForUserModule.hleUnblockThread(threadId);
			}
			else if (availableSamples >= samples)
			{
				if (captureBuffer == null || captureBuffer.capacity() < bufferBytes)
				{
					captureBuffer = BufferUtils.createByteBuffer(bufferBytes);
				}
				else
				{
					captureBuffer.rewind();
				}

				ALC11.alcCaptureSamples(inputDevice, captureBuffer, samples);

				captureBuffer.rewind();
				IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(addr, samples, 2);
				for (int i = 0; i < samples; i++)
				{
					short sample = captureBuffer.Short;
					memoryWriter.writeNext(sample & 0xFFFF);
				}

				if (log.TraceEnabled)
				{
					log.trace(string.Format("hleAudioBlockingInput returning {0:D} samples: {1}", samples, Utilities.getMemoryDump(addr, bufferBytes, 2, 16)));
				}
				Modules.ThreadManForUserModule.hleUnblockThread(threadId);
			}
			else
			{
				blockThreadInput(threadId, addr, samples, frequency, availableSamples);
			}
		}

		public virtual int hleAudioGetInputLength()
		{
			if (inputDevice == null)
			{
				return 0;
			}

			if (samplesBuffer == null)
			{
				samplesBuffer = BufferUtils.createIntBuffer(1);
			}

			ALC10.alcGetInteger(inputDevice, ALC11.ALC_CAPTURE_SAMPLES, samplesBuffer);

			return samplesBuffer.get(0);
		}

		protected internal virtual int getUnblockInputDelayMicros(int availableSamples, int samples, int frequency)
		{
			if (availableSamples >= samples)
			{
				return 0;
			}

			int missingSamples = samples - availableSamples;
			int delayMicros = (int)(missingSamples * 1000000L / frequency);

			return delayMicros;
		}

		protected internal virtual void blockThreadInput(int addr, int samples, int frequency)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			int threadId = threadMan.CurrentThreadID;
			threadMan.hleBlockCurrentThread(SceKernelThreadInfo.JPCSP_WAIT_AUDIO);
			blockThreadInput(threadId, addr, samples, frequency, hleAudioGetInputLength());
		}

		protected internal virtual void blockThreadInput(int threadId, int addr, int samples, int frequency, int availableSamples)
		{
			int delayMicros = getUnblockInputDelayMicros(availableSamples, samples, frequency);
			if (log.TraceEnabled)
			{
				log.trace(string.Format("blockThreadInput waiting {0:D} micros", delayMicros));
			}
			Emulator.Scheduler.addAction(Emulator.Clock.microTime() + delayMicros, new AudioBlockingInputAction(threadId, addr, samples, frequency));
		}

		public virtual int hleAudioInputBlocking(int maxSamples, int frequency, TPointer buffer)
		{
			if (!inputDeviceInitialized)
			{
				IntBuffer majorVersion = BufferUtils.createIntBuffer(1);
				IntBuffer minorVersion = BufferUtils.createIntBuffer(1);
				ALC10.alcGetInteger(null, ALC10.ALC_MAJOR_VERSION, majorVersion);
				ALC10.alcGetInteger(null, ALC10.ALC_MINOR_VERSION, minorVersion);
				log.info(string.Format("OpenAL Version {0:D}.{1:D}, extensions {2}", majorVersion.get(0), minorVersion.get(0), ALC10.alcGetString(null, ALC10.ALC_EXTENSIONS)));

				inputDevice = ALC11.alcCaptureOpenDevice(null, frequency, AL10.AL_FORMAT_MONO16, 10 * 1024);
				if (inputDevice != null)
				{
					ALC11.alcCaptureStart(inputDevice);
				}
				else
				{
					Console.WriteLine(string.Format("No audio input device available, faking."));
				}

				inputDeviceInitialized = true;
			}

			blockThreadInput(buffer.Address, maxSamples, frequency);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x80F1F7E0, version = 150, checkInsideInterrupt = true) public int sceAudioInit()
		[HLEFunction(nid : 0x80F1F7E0, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioInit()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x210567F7, version = 150, checkInsideInterrupt = true) public int sceAudioEnd()
		[HLEFunction(nid : 0x210567F7, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioEnd()
		{
			return 0;
		}

		[HLEFunction(nid : 0xA2BEAA6C, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioSetFrequency(int frequency)
		{
			if (frequency != 44100 && frequency != 48000)
			{
				return SceKernelErrors.ERROR_AUDIO_INVALID_FREQUENCY;
			}

			for (int i = 0; i < pspPCMChannels.Length; i++)
			{
				pspPCMChannels[i].SampleRate = frequency;
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB61595C0, version = 150, checkInsideInterrupt = true) public int sceAudioLoopbackTest()
		[HLEFunction(nid : 0xB61595C0, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioLoopbackTest()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x927AC32B, version = 150, checkInsideInterrupt = true) public int sceAudioSetVolumeOffset()
		[HLEFunction(nid : 0x927AC32B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioSetVolumeOffset()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8C1009B2, version = 150, checkInsideInterrupt = true) public int sceAudioOutput(@CheckArgument("checkReservedChannel") int channel, @CheckArgument("checkVolume") int vol, @CanBeNull pspsharp.HLE.TPointer pvoid_buf)
		[HLEFunction(nid : 0x8C1009B2, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioOutput(int channel, int vol, TPointer pvoid_buf)
		{
			if (pspPCMChannels[channel].OutputBlocking)
			{
				return SceKernelErrors.ERROR_AUDIO_CHANNEL_BUSY;
			}

			changeChannelVolume(pspPCMChannels[channel], vol, vol);
			int result = doAudioOutput(pspPCMChannels[channel], pvoid_buf.Address);
			Modules.ThreadManForUserModule.hleRescheduleCurrentThread();

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x136CAF51, version = 150, checkInsideInterrupt = true) public int sceAudioOutputBlocking(@CheckArgument("checkReservedChannel") int channel, @CheckArgument("checkVolume") int vol, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=0x700, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer pvoid_buf)
		[HLEFunction(nid : 0x136CAF51, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioOutputBlocking(int channel, int vol, TPointer pvoid_buf)
		{
			int result = 0;
			if (pvoid_buf.Null)
			{
				if (!pspPCMChannels[channel].Drained)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine("sceAudioOutputBlocking[pvoid_buf==0] blocking " + pspPCMChannels[channel].ToString());
					}
					blockThreadOutput(pspPCMChannels[channel], pvoid_buf.Address, vol, vol);
				}
				result = pspPCMChannels[channel].SampleLength;
			}
			else if (!pspPCMChannels[channel].OutputBlocking)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine("sceAudioOutputBlocking[not blocking] " + pspPCMChannels[channel].ToString());
				}
				changeChannelVolume(pspPCMChannels[channel], vol, vol);
				result = doAudioOutput(pspPCMChannels[channel], pvoid_buf.Address);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceAudioOutputBlocking[not blocking] returning {0:D} ({1})", result, pspPCMChannels[channel]));
				}
				Modules.ThreadManForUserModule.hleRescheduleCurrentThread();
			}
			else
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine("sceAudioOutputBlocking[blocking] " + pspPCMChannels[channel].ToString());
				}
				blockThreadOutput(pspPCMChannels[channel], pvoid_buf.Address, vol, vol);
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE2D56B2D, version = 150, checkInsideInterrupt = true) public int sceAudioOutputPanned(@CheckArgument("checkReservedChannel") int channel, @CheckArgument("checkVolume") int leftvol, @CheckArgument("checkVolume") int rightvol, @CanBeNull pspsharp.HLE.TPointer pvoid_buf)
		[HLEFunction(nid : 0xE2D56B2D, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioOutputPanned(int channel, int leftvol, int rightvol, TPointer pvoid_buf)
		{
			if (pspPCMChannels[channel].OutputBlocking)
			{
				return SceKernelErrors.ERROR_AUDIO_CHANNEL_BUSY;
			}

			changeChannelVolume(pspPCMChannels[channel], leftvol, rightvol);
			int result = doAudioOutput(pspPCMChannels[channel], pvoid_buf.Address);
			Modules.ThreadManForUserModule.hleRescheduleCurrentThread();

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x13F592BC, version = 150, checkInsideInterrupt = true) public int sceAudioOutputPannedBlocking(@CheckArgument("checkReservedChannel") int channel, @CheckArgument("checkVolume") int leftvol, @CheckArgument("checkVolume") int rightvol, @CanBeNull pspsharp.HLE.TPointer pvoid_buf)
		[HLEFunction(nid : 0x13F592BC, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioOutputPannedBlocking(int channel, int leftvol, int rightvol, TPointer pvoid_buf)
		{
			int result = 0;

			if (leftvol == int.MinValue || rightvol == int.MinValue)
			{
				// In case of blocking panned output, 0x80000000 is not allowed.
				// In case of non-blocking panned output, 0x80000000 is allowed.
				return SceKernelErrors.ERROR_AUDIO_INVALID_VOLUME;
			}

			if (pvoid_buf.Null)
			{
				// Tested on PSP:
				// An output adress of 0 is actually a special code for the PSP.
				// It means that we must stall processing until all the previous
				// unplayed samples' data is output.
				if (!pspPCMChannels[channel].Drained)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine("sceAudioOutputPannedBlocking[pvoid_buf==0] blocking " + pspPCMChannels[channel].ToString());
					}
					blockThreadOutput(pspPCMChannels[channel], pvoid_buf.Address, leftvol, rightvol);
				}
				result = pspPCMChannels[channel].SampleLength;
			}
			else if (!pspPCMChannels[channel].OutputBlocking)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceAudioOutputPannedBlocking[not blocking] leftVol={0:D}, rightVol={1:D}, channel={2}", leftvol, rightvol, pspPCMChannels[channel].ToString()));
				}
				changeChannelVolume(pspPCMChannels[channel], leftvol, rightvol);
				result = doAudioOutput(pspPCMChannels[channel], pvoid_buf.Address);
				Modules.ThreadManForUserModule.hleRescheduleCurrentThread();
			}
			else
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceAudioOutputPannedBlocking[blocking] leftVol={0:D}, rightVol={1:D}, channel={2}", leftvol, rightvol, pspPCMChannels[channel].ToString()));
				}
				blockThreadOutput(pspPCMChannels[channel], pvoid_buf.Address, leftvol, rightvol);
			}

			return result;
		}

		[HLEFunction(nid : 0x5EC81C55, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioChReserve(int channel, int sampleCount, int format)
		{
			if (channel >= 0)
			{
				channel = checkChannel(channel);
				if (pspPCMChannels[channel].Reserved)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("sceAudioChReserve failed - channel {0:D} already in use", channel));
					}
					return SceKernelErrors.ERROR_AUDIO_INVALID_CHANNEL;
				}
			}
			else
			{
				// The PSP is searching for a free channel, starting with the highest channel number.
				for (int i = pspPCMChannels.Length - 1; i >= 0; i--)
				{
					if (!pspPCMChannels[i].Reserved)
					{
						channel = i;
						break;
					}
				}

				if (channel < 0)
				{
					Console.WriteLine("sceAudioChReserve failed - no free channels available");
					return SceKernelErrors.ERROR_AUDIO_NO_CHANNELS_AVAILABLE;
				}
			}

			// The validity of the sampleCount and format parameters is only checked after the channel check
			sampleCount = checkSampleCount(sampleCount);
			format = checkFormat(format);

			pspPCMChannels[channel].Reserved = true;
			pspPCMChannels[channel].SampleLength = sampleCount;
			pspPCMChannels[channel].Format = format;

			return channel;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x41EFADE7, version = 150, checkInsideInterrupt = true) public int sceAudioOneshotOutput()
		[HLEFunction(nid : 0x41EFADE7, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioOneshotOutput()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x6FC46853, version = 150, checkInsideInterrupt = true) public int sceAudioChRelease(@CheckArgument("checkChannel") int channel)
		[HLEFunction(nid : 0x6FC46853, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioChRelease(int channel)
		{
			if (!pspPCMChannels[channel].Reserved)
			{
				return SceKernelErrors.ERROR_AUDIO_CHANNEL_NOT_RESERVED;
			}

			pspPCMChannels[channel].release();
			pspPCMChannels[channel].Reserved = false;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB011922F, version = 150, checkInsideInterrupt = true) public int sceAudioGetChannelRestLength(@CheckArgument("checkChannel") int channel)
		[HLEFunction(nid : 0xB011922F, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioGetChannelRestLength(int channel)
		{
			return hleAudioGetChannelRestLength(pspPCMChannels[channel]);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xCB2E439E, version = 150, checkInsideInterrupt = true) public int sceAudioSetChannelDataLen(@CheckArgument("checkReservedChannel") int channel, @CheckArgument("checkSampleCount") int sampleCount)
		[HLEFunction(nid : 0xCB2E439E, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioSetChannelDataLen(int channel, int sampleCount)
		{
			pspPCMChannels[channel].SampleLength = sampleCount;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x95FD0C2D, version = 150, checkInsideInterrupt = true) public int sceAudioChangeChannelConfig(@CheckArgument("checkReservedChannel") int channel, @CheckArgument("checkFormat") int format)
		[HLEFunction(nid : 0x95FD0C2D, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioChangeChannelConfig(int channel, int format)
		{
			pspPCMChannels[channel].Format = format;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB7E1D8E7, version = 150, checkInsideInterrupt = true) public int sceAudioChangeChannelVolume(@CheckArgument("checkReservedChannel") int channel, @CheckArgument("checkVolume") int leftvol, @CheckArgument("checkVolume") int rightvol)
		[HLEFunction(nid : 0xB7E1D8E7, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioChangeChannelVolume(int channel, int leftvol, int rightvol)
		{
			return changeChannelVolume(pspPCMChannels[channel], leftvol, rightvol);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x01562BA3, version = 150, checkInsideInterrupt = true) public int sceAudioOutput2Reserve(@CheckArgument("checkReserveSampleCount") int sampleCount)
		[HLEFunction(nid : 0x01562BA3, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioOutput2Reserve(int sampleCount)
		{
			return hleAudioSRCChReserve(sampleCount, 44100, SoundChannel.FORMAT_STEREO);
		}

		[HLEFunction(nid : 0x43196845, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioOutput2Release()
		{
			return sceAudioSRCChRelease();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x2D53F36E, version = 150, checkInsideInterrupt = true) public int sceAudioOutput2OutputBlocking(@CheckArgument("checkVolume2") int vol, @CanBeNull pspsharp.HLE.TPointer buf)
		[HLEFunction(nid : 0x2D53F36E, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioOutput2OutputBlocking(int vol, TPointer buf)
		{
			return sceAudioSRCOutputBlocking(vol, buf);
		}

		[HLEFunction(nid : 0x647CEF33, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioOutput2GetRestSample()
		{
			if (!pspSRC1Channel.Reserved)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceAudioOutput2GetRestSample returning ERROR_AUDIO_CHANNEL_NOT_RESERVED"));
				}
				return SceKernelErrors.ERROR_AUDIO_CHANNEL_NOT_RESERVED;
			}

			int rest1 = pspSRC1Channel.Busy ? pspSRC1Channel.SampleLength : 0;
			int rest2 = pspSRC2Channel.Busy ? pspSRC2Channel.SampleLength : 0;
			int rest = rest1 + rest2;

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceAudioOutput2GetRestSample returning 0x{0:X} (rest1=0x{1:X}, rest2=0x{2:X})", rest, rest1, rest2));
			}

			return rest;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x63F2889C, version = 150, checkInsideInterrupt = true) public int sceAudioOutput2ChangeLength(@CheckArgument("checkSmallSampleCount") int sampleCount)
		[HLEFunction(nid : 0x63F2889C, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioOutput2ChangeLength(int sampleCount)
		{
			if (!pspSRC1Channel.Reserved)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceAudioOutput2ChangeLength returning ERROR_AUDIO_CHANNEL_NOT_RESERVED"));
				}
				return SceKernelErrors.ERROR_AUDIO_CHANNEL_NOT_RESERVED;
			}

			pspSRC1Channel.SampleLength = sampleCount;
			pspSRC2Channel.SampleLength = sampleCount;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x38553111, version = 150, checkInsideInterrupt = true) public int sceAudioSRCChReserve(@CheckArgument("checkReserveSampleCount") int sampleCount, @CheckArgument("checkFrequency") int freq, @CheckArgument("checkChannelCount") int format)
		[HLEFunction(nid : 0x38553111, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioSRCChReserve(int sampleCount, int freq, int format)
		{
			return hleAudioSRCChReserve(sampleCount, freq, format);
		}

		[HLEFunction(nid : 0x5C37C0AE, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioSRCChRelease()
		{
			if (!pspSRC1Channel.Reserved)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceAudioSRCChRelease returning ERROR_AUDIO_CHANNEL_NOT_RESERVED"));
				}
				return SceKernelErrors.ERROR_AUDIO_CHANNEL_NOT_RESERVED;
			}

			pspSRC1Channel.release();
			pspSRC1Channel.Reserved = false;

			pspSRC2Channel.release();
			pspSRC2Channel.Reserved = false;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE0727056, version = 150, checkInsideInterrupt = true) public int sceAudioSRCOutputBlocking(@CheckArgument("checkVolume2") int vol, @CanBeNull pspsharp.HLE.TPointer buf)
		[HLEFunction(nid : 0xE0727056, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioSRCOutputBlocking(int vol, TPointer buf)
		{
			// Tested on PSP: any sound volume above MAX_VOLUME has the same effect as MAX_VOLUME.
			int channelVolume = min(SoundChannel.MAX_VOLUME, vol);

			SoundChannel pspSRCChannel = FreeSRCChannel;
			if (pspSRCChannel == null)
			{
				return SceKernelErrors.ERROR_AUDIO_CHANNEL_BUSY;
			}

			pspSRCChannel.Volume = channelVolume;

			if (buf.Null)
			{
				// Tested on PSP:
				// SRC audio also delays when buf == 0, in order to drain all
				// audio samples from the audio driver.
				if (!pspSRCChannel.Drained)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine("sceAudioSRCOutputBlocking[buf==0] blocking " + pspSRCChannel);
					}
					// Do not update volume, it has already been updated above
					blockThreadOutput(pspSRCChannel, buf.Address, -1, -1);
				}
				else
				{
					Modules.ThreadManForUserModule.hleYieldCurrentThread();
				}
			}
			else if (!pspSRC1Channel.Reserved)
			{
				// Channel is automatically reserved. The audio data (buf) is not used in this case.
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceAudioSRCOutputBlocking automatically reserving channel {0}", pspSRCChannel));
				}
				pspSRC1Channel.Reserved = true;
				pspSRC2Channel.Reserved = true;
			}
			else
			{
				if (!pspSRCChannel.OutputBlocking)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("sceAudioSRCOutputBlocking[not blocking] {0} to {1}", buf, pspSRCChannel.ToString()));
					}
					Modules.ThreadManForUserModule.hleRescheduleCurrentThread();
					return doAudioOutput(pspSRCChannel, buf.Address);
				}

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceAudioSRCOutputBlocking[blocking] {0} to {1}", buf, pspSRCChannel.ToString()));
				}
				// Do not update volume, it has already been updated above
				blockThreadOutput(pspSRCChannel, buf.Address, -1, -1);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x086E5895, version = 150, checkInsideInterrupt = true) public int sceAudioInputBlocking(int maxSamples, int frequency, pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0x086E5895, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioInputBlocking(int maxSamples, int frequency, TPointer buffer)
		{
			if (frequency != 44100 && frequency != 22050 && frequency != 11025)
			{
				return SceKernelErrors.ERROR_AUDIO_INVALID_FREQUENCY;
			}

			return hleAudioInputBlocking(maxSamples, frequency, buffer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6D4BEC68, version = 150, checkInsideInterrupt = true) public int sceAudioInput()
		[HLEFunction(nid : 0x6D4BEC68, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioInput()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA708C6A6, version = 150, checkInsideInterrupt = true) public int sceAudioGetInputLength()
		[HLEFunction(nid : 0xA708C6A6, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioGetInputLength()
		{
			return hleAudioGetInputLength();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x87B2E651, version = 150, checkInsideInterrupt = true) public int sceAudioWaitInputEnd()
		[HLEFunction(nid : 0x87B2E651, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioWaitInputEnd()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7DE61688, version = 150, checkInsideInterrupt = true) public int sceAudioInputInit()
		[HLEFunction(nid : 0x7DE61688, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioInputInit()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE926D3FB, version = 150, checkInsideInterrupt = true) public int sceAudioInputInitEx()
		[HLEFunction(nid : 0xE926D3FB, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioInputInitEx()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA633048E, version = 150, checkInsideInterrupt = true) public int sceAudioPollInputEnd()
		[HLEFunction(nid : 0xA633048E, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioPollInputEnd()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xE9D97901, version = 150, checkInsideInterrupt = true) public int sceAudioGetChannelRestLen(@CheckArgument("checkChannel") int channel)
		[HLEFunction(nid : 0xE9D97901, version : 150, checkInsideInterrupt : true)]
		public virtual int sceAudioGetChannelRestLen(int channel)
		{
			return hleAudioGetChannelRestLength(pspPCMChannels[channel]);
		}

		[HLEFunction(nid : 0x9DB844C6, version : 500, checkInsideInterrupt : true)]
		public virtual int sceAudioSetFrequency500(int frequency)
		{
			return sceAudioSetFrequency(frequency);
		}
	}
}