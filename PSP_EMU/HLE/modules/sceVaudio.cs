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
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SoundChannel = pspsharp.sound.SoundChannel;

	using Logger = org.apache.log4j.Logger;

	public class sceVaudio : HLEModule
	{
		public static Logger log = Modules.getLogger("sceVaudio");

		public override void start()
		{
			SoundChannel.init();

			// The PSP is using the same channel as the SRC channel(s)
			pspVaudio1Channel = Modules.sceAudioModule.pspSRC1Channel;
			pspVaudio2Channel = Modules.sceAudioModule.pspSRC2Channel;
			pspVaudioChannelReserved = false;

			base.start();
		}

		protected internal const int PSP_VAUDIO_VOLUME_BASE = 0x8000;
		protected internal const int PSP_VAUDIO_SAMPLE_MIN = 256;
		protected internal const int PSP_VAUDIO_SAMPLE_MAX = 2048;
		protected internal const int PSP_VAUDIO_FORMAT_MONO = 0x0;
		protected internal const int PSP_VAUDIO_FORMAT_STEREO = 0x2;

		protected internal const int PSP_VAUDIO_EFFECT_TYPE_NONE = 0;
		protected internal const int PSP_VAUDIO_EFFECT_TYPE_1 = 1;
		protected internal const int PSP_VAUDIO_EFFECT_TYPE_2 = 2;
		protected internal const int PSP_VAUDIO_EFFECT_TYPE_3 = 3;
		protected internal const int PSP_VAUDIO_EFFECT_TYPE_4 = 4;

		protected internal const int PSP_VAUDIO_ALC_MODE_NONE = 0;
		protected internal const int PSP_VAUDIO_ALC_MODE_1 = 1;

		protected internal SoundChannel pspVaudio1Channel;
		protected internal SoundChannel pspVaudio2Channel;
		protected internal bool pspVaudioChannelReserved;

		public virtual int checkSampleCount(int sampleCount)
		{
			if (sampleCount < 256 || sampleCount > 2048)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Invalid sampleCount 0x{0:X}", sampleCount));
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_SIZE);
			}

			return sampleCount;
		}

		public virtual int checkFrequency(int frequency)
		{
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
				case 44100:
				case 48000:
					// OK
					break;
				default:
					// PSP is yielding in this error code
					Modules.ThreadManForUserModule.hleYieldCurrentThread();
					throw new SceKernelErrorException(SceKernelErrors.ERROR_AUDIO_INVALID_FREQUENCY);
			}

			return frequency;
		}

		public virtual int checkChannelCount(int channelCount)
		{
			if (channelCount != 2 && channelCount != 4)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_INVALID_FORMAT);
			}

			return channelCount;
		}

		protected internal virtual int doAudioOutput(SoundChannel channel, int pvoid_buf)
		{
			return sceAudio.doAudioOutput(channel, pvoid_buf);
		}

		protected internal virtual void blockThreadOutput(SoundChannel channel, int addr, int leftVolume, int rightVolume)
		{
			sceAudio.blockThreadOutput(channel, addr, leftVolume, rightVolume);
		}

		protected internal virtual int changeChannelVolume(SoundChannel channel, int leftvol, int rightvol)
		{
			return sceAudio.changeChannelVolume(channel, leftvol, rightvol);
		}

		protected internal virtual int hleVaudioChReserve(int sampleCount, int freq, int format, bool buffering)
		{
			// Returning a different error code if the channel has been reserved by sceVaudioChReserve or by sceAudioSRCChReserve
			if (pspVaudioChannelReserved)
			{
				return SceKernelErrors.ERROR_BUSY;
			}
			if (pspVaudio1Channel.Reserved)
			{
				// PSP is yielding in this error case
				Modules.ThreadManForUserModule.hleYieldCurrentThread();
				return SceKernelErrors.ERROR_AUDIO_CHANNEL_ALREADY_RESERVED;
			}

			pspVaudioChannelReserved = true;
			pspVaudio1Channel.Reserved = true;
			pspVaudio1Channel.SampleLength = sampleCount;
			pspVaudio1Channel.SampleRate = freq;
			pspVaudio1Channel.Format = format == PSP_VAUDIO_FORMAT_MONO ? sceAudio.PSP_AUDIO_FORMAT_MONO : sceAudio.PSP_AUDIO_FORMAT_STEREO;

			pspVaudio2Channel.Reserved = true;
			pspVaudio2Channel.SampleLength = sampleCount;
			pspVaudio2Channel.SampleRate = freq;
			pspVaudio2Channel.Format = format == PSP_VAUDIO_FORMAT_MONO ? sceAudio.PSP_AUDIO_FORMAT_MONO : sceAudio.PSP_AUDIO_FORMAT_STEREO;

			Modules.ThreadManForUserModule.hleYieldCurrentThread();

			return 0;
		}

		[HLEFunction(nid : 0x67585DFD, version : 150, checkInsideInterrupt : true)]
		public virtual int sceVaudioChRelease()
		{
			if (!pspVaudio1Channel.Reserved)
			{
				return SceKernelErrors.ERROR_AUDIO_CHANNEL_NOT_RESERVED;
			}

			pspVaudioChannelReserved = false;
			pspVaudio1Channel.release();
			pspVaudio1Channel.Reserved = false;
			pspVaudio2Channel.release();
			pspVaudio2Channel.Reserved = false;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x03B6807D, version = 150, checkInsideInterrupt = true) public int sceVaudioChReserve(@CheckArgument("checkSampleCount") int sampleCount, @CheckArgument("checkFrequency") int freq, @CheckArgument("checkChannelCount") int format)
		[HLEFunction(nid : 0x03B6807D, version : 150, checkInsideInterrupt : true)]
		public virtual int sceVaudioChReserve(int sampleCount, int freq, int format)
		{
			return hleVaudioChReserve(sampleCount, freq, format, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8986295E, version = 150, checkInsideInterrupt = true) public int sceVaudioOutputBlocking(int vol, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=32, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer buf)
		[HLEFunction(nid : 0x8986295E, version : 150, checkInsideInterrupt : true)]
		public virtual int sceVaudioOutputBlocking(int vol, TPointer buf)
		{
			int result = 0;

			SoundChannel pspVaudioChannel = Modules.sceAudioModule.FreeSRCChannel;
			if (!pspVaudioChannel.OutputBlocking)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceVaudioOutputBlocking[not blocking] {0}", pspVaudioChannel));
				}
				if ((vol & PSP_VAUDIO_VOLUME_BASE) != PSP_VAUDIO_VOLUME_BASE)
				{
					changeChannelVolume(pspVaudioChannel, vol, vol);
				}
				result = doAudioOutput(pspVaudioChannel, buf.Address);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceVaudioOutputBlocking[not blocking] returning {0:D} ({1})", result, pspVaudioChannel));
				}
				Modules.ThreadManForUserModule.hleRescheduleCurrentThread();
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceVaudioOutputBlocking[blocking] {0}", pspVaudioChannel));
				}
				blockThreadOutput(pspVaudioChannel, buf.Address, vol, vol);
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x346FBE94, version = 150, checkInsideInterrupt = true) public int sceVaudioSetEffectType(int type, int vol)
		[HLEFunction(nid : 0x346FBE94, version : 150, checkInsideInterrupt : true)]
		public virtual int sceVaudioSetEffectType(int type, int vol)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCBD4AC51, version = 150, checkInsideInterrupt = true) public int sceVaudioSetAlcMode(int alcMode)
		[HLEFunction(nid : 0xCBD4AC51, version : 150, checkInsideInterrupt : true)]
		public virtual int sceVaudioSetAlcMode(int alcMode)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE8E78DC8, version = 150) public int sceVaudio_E8E78DC8(@CheckArgument("checkSampleCount") int sampleCount, @CheckArgument("checkFrequency") int freq, @CheckArgument("checkChannelCount") int format)
		[HLEFunction(nid : 0xE8E78DC8, version : 150)]
		public virtual int sceVaudio_E8E78DC8(int sampleCount, int freq, int format)
		{
			// What is the difference with sceVaudioChReserveBuffering?
			return hleVaudioChReserve(sampleCount, freq, format, true);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x504E4745, version = 150) public int sceVaudio_504E4745(int unknown)
		[HLEFunction(nid : 0x504E4745, version : 150)]
		public virtual int sceVaudio_504E4745(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x27ACC20B, version = 150) public int sceVaudioChReserveBuffering(@CheckArgument("checkSampleCount") int sampleCount, @CheckArgument("checkFrequency") int freq, @CheckArgument("checkChannelCount") int format)
		[HLEFunction(nid : 0x27ACC20B, version : 150)]
		public virtual int sceVaudioChReserveBuffering(int sampleCount, int freq, int format)
		{
			return hleVaudioChReserve(sampleCount, freq, format, true);
		}
	}
}