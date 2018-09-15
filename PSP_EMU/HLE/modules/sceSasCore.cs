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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_SAS_INVALID_ADDRESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_SAS_INVALID_ADSR_CURVE_MODE;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using AtracID = pspsharp.HLE.modules.sceAtrac3plus.AtracID;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using SoundVoice = pspsharp.sound.SoundVoice;
	using SoundMixer = pspsharp.sound.SoundMixer;
	using VoiceADSREnvelope = pspsharp.sound.SoundVoice.VoiceADSREnvelope;

	//using Logger = org.apache.log4j.Logger;

	public class sceSasCore : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceSasCore");

		public override void start()
		{
			sasCoreUid = -1;
			voices = new SoundVoice[32];
			for (int i = 0; i < voices.Length; i++)
			{
				voices[i] = new SoundVoice(i);
			}
			mixer = new SoundMixer(voices);
			grainSamples = PSP_SAS_GRAIN_SAMPLES;
			outputMode = PSP_SAS_OUTPUTMODE_STEREO;

			base.start();
		}

		public const int PSP_SAS_VOICES_MAX = 32;
		public const int PSP_SAS_GRAIN_SAMPLES = 256;
		public const int PSP_SAS_VOL_MAX = 0x1000;
		public const int PSP_SAS_LOOP_MODE_OFF = 0;
		public const int PSP_SAS_LOOP_MODE_ON = 1;
		public const int PSP_SAS_PITCH_MIN = 0x1;
		public const int PSP_SAS_PITCH_BASE = 0x1000;
		public const int PSP_SAS_PITCH_MAX = 0x4000;
		public const int PSP_SAS_NOISE_FREQ_MAX = 0x3F;
		public const int PSP_SAS_ENVELOPE_HEIGHT_MAX = 0x40000000;
		public const int PSP_SAS_ENVELOPE_FREQ_MAX = 0x7FFFFFFF;
		public const int PSP_SAS_ADSR_CURVE_MODE_LINEAR_INCREASE = 0;
		public const int PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE = 1;
		public const int PSP_SAS_ADSR_CURVE_MODE_LINEAR_BENT = 2;
		public const int PSP_SAS_ADSR_CURVE_MODE_EXPONENT_DECREASE = 3;
		public const int PSP_SAS_ADSR_CURVE_MODE_EXPONENT_INCREASE = 4;
		public const int PSP_SAS_ADSR_CURVE_MODE_DIRECT = 5;
		public const int PSP_SAS_ADSR_ATTACK = 1;
		public const int PSP_SAS_ADSR_DECAY = 2;
		public const int PSP_SAS_ADSR_SUSTAIN = 4;
		public const int PSP_SAS_ADSR_RELEASE = 8;
		public const int PSP_SAS_OUTPUTMODE_STEREO = 0;
		public const int PSP_SAS_OUTPUTMODE_MONO = 1;
		public const int PSP_SAS_EFFECT_TYPE_OFF = -1;
		public const int PSP_SAS_EFFECT_TYPE_ROOM = 0;
		public const int PSP_SAS_EFFECT_TYPE_UNK1 = 1;
		public const int PSP_SAS_EFFECT_TYPE_UNK2 = 2;
		public const int PSP_SAS_EFFECT_TYPE_UNK3 = 3;
		public const int PSP_SAS_EFFECT_TYPE_HALL = 4;
		public const int PSP_SAS_EFFECT_TYPE_SPACE = 5;
		public const int PSP_SAS_EFFECT_TYPE_ECHO = 6;
		public const int PSP_SAS_EFFECT_TYPE_DELAY = 7;
		public const int PSP_SAS_EFFECT_TYPE_PIPE = 8;
		private static readonly string[] sasADSRCurveTypeNames = new string[] {"LINEAR_INCREASE", "LINEAR_DECREASE", "LINEAR_BENT", "EXPONENT_REV", "EXPONENT", "DIRECT"};
		private const int SASCORE_ATRAC3_CONTEXT_OFFSET = 20;
		private const int SASCORE_VOICE_SIZE = 56;

		protected internal int sasCoreUid;
		protected internal SoundVoice[] voices;
		protected internal SoundMixer mixer;
		protected internal int grainSamples;
		protected internal int outputMode;
		protected internal const int waveformBufMaxSize = 1024; // 256 sound samples.
		protected internal int waveformEffectType;
		protected internal int waveformEffectLeftVol;
		protected internal int waveformEffectRightVol;
		protected internal int waveformEffectDelay;
		protected internal int waveformEffectFeedback;
		protected internal bool waveformEffectIsDryOn;
		protected internal bool waveformEffectIsWetOn;
		protected internal const int sasCoreDelay = 5000; // Average microseconds, based on PSP tests.
		protected internal const string sasCodeUidPurpose = "sceSasCore-SasCore";

		public static string getSasADSRCurveTypeName(int curveType)
		{
			if (curveType < 0 || curveType >= sasADSRCurveTypeNames.Length)
			{
				return string.Format("UNKNOWN_{0:D}", curveType);
			}

			return sasADSRCurveTypeNames[curveType];
		}

		protected internal virtual void checkSasAddressGood(int sasCore)
		{
			if (!Memory.isAddressGood(sasCore))
			{
				Console.WriteLine(string.Format("{0} bad sasCore Address 0x{1:X8}", getCallingFunctionName(3), sasCore));
				throw (new SceKernelErrorException(ERROR_SAS_INVALID_ADDRESS));
			}

			if (!Memory.isAddressAlignedTo(sasCore, 64))
			{
				Console.WriteLine(string.Format("{0} bad sasCore Address 0x{1:X8} (not aligned to 64)", getCallingFunctionName(3), sasCore));
				throw (new SceKernelErrorException(ERROR_SAS_INVALID_ADDRESS));
			}
		}

		protected internal virtual void checkSasHandleGood(int sasCore)
		{
			checkSasAddressGood(sasCore);

			if (Processor.memory.read32(sasCore) != sasCoreUid)
			{
				Console.WriteLine(string.Format("{0} bad sasCoreUid 0x{1:X} (should be 0x{2:X})", getCallingFunctionName(3), Processor.memory.read32(sasCore), sasCoreUid));
				throw (new SceKernelErrorException(SceKernelErrors.ERROR_SAS_NOT_INIT));
			}
		}

		protected internal virtual void checkVoiceNumberGood(int voice)
		{
			if (voice < 0 || voice >= voices.Length)
			{
				Console.WriteLine(string.Format("{0} bad voice number {1:D}", getCallingFunctionName(3), voice));
				throw (new SceKernelErrorException(SceKernelErrors.ERROR_SAS_INVALID_VOICE_INDEX));
			}
		}

		protected internal virtual void checkSasAndVoiceHandlesGood(int sasCore, int voice)
		{
			checkSasHandleGood(sasCore);
			checkVoiceNumberGood(voice);
		}

		protected internal virtual void checkADSRmode(int curveIndex, int flag, int curveType)
		{
			int[] validCurveTypes = new int[] {(1 << PSP_SAS_ADSR_CURVE_MODE_LINEAR_INCREASE) | (1 << PSP_SAS_ADSR_CURVE_MODE_LINEAR_BENT) | (1 << PSP_SAS_ADSR_CURVE_MODE_EXPONENT_INCREASE), (1 << PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE) | (1 << PSP_SAS_ADSR_CURVE_MODE_EXPONENT_DECREASE) | (1 << PSP_SAS_ADSR_CURVE_MODE_DIRECT), (1 << PSP_SAS_ADSR_CURVE_MODE_LINEAR_INCREASE) | (1 << PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE) | (1 << PSP_SAS_ADSR_CURVE_MODE_LINEAR_BENT) | (1 << PSP_SAS_ADSR_CURVE_MODE_EXPONENT_DECREASE) | (1 << PSP_SAS_ADSR_CURVE_MODE_EXPONENT_INCREASE) | (1 << PSP_SAS_ADSR_CURVE_MODE_DIRECT), (1 << PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE) | (1 << PSP_SAS_ADSR_CURVE_MODE_EXPONENT_DECREASE) | (1 << PSP_SAS_ADSR_CURVE_MODE_DIRECT)};

			if ((flag & (1 << curveIndex)) != 0)
			{
				if ((validCurveTypes[curveIndex] & (1 << curveType)) == 0)
				{
					throw new SceKernelErrorException(SceKernelErrors.ERROR_SAS_INVALID_ADSR_CURVE_MODE);
				}
			}
		}

		protected internal virtual void checkVoiceNotPaused(int voice, bool requiredOnState)
		{
			if (voices[voice].Paused || voices[voice].On != requiredOnState)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("checkVoiceNotPaused returning 0x{0:X8}(ERROR_SAS_VOICE_PAUSED)", SceKernelErrors.ERROR_SAS_VOICE_PAUSED));
				}
				throw new SceKernelErrorException(SceKernelErrors.ERROR_SAS_VOICE_PAUSED);
			}
		}

		public virtual int checkVolume(int volume)
		{
			if (volume < -PSP_SAS_VOL_MAX || volume > PSP_SAS_VOL_MAX)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_SAS_INVALID_VOLUME_VAL);
			}

			return volume;
		}

		private void delayThread(long startMicros, int delayMicros, int minimumDelayMicros)
		{
			long now = Emulator.Clock.microTime();
			int threadDelayMicros = delayMicros - (int)(now - startMicros);
			threadDelayMicros = System.Math.Max(threadDelayMicros, minimumDelayMicros);
			if (threadDelayMicros > 0)
			{
				Modules.ThreadManForUserModule.hleKernelDelayThread(threadDelayMicros, false);
			}
			else
			{
				Modules.ThreadManForUserModule.hleRescheduleCurrentThread();
			}
		}

		private void delayThreadSasCore(long startMicros)
		{
			// Based on PSP timings: the delay for __sceSasCore is always about
			// 600 microseconds, independently of the number of samples generated
			// and of the number of voices currently playing.
			int delayMicros = 600;
			delayThread(startMicros, delayMicros, delayMicros);
		}

		public virtual int OutputMode
		{
			get
			{
				return outputMode;
			}
		}

		protected internal virtual void setSasCoreAtrac3Context(int sasCore, int voice, int atrac3Context)
		{
			Memory mem = Memory.Instance;
			mem.write32(sasCore + SASCORE_VOICE_SIZE * voice + SASCORE_ATRAC3_CONTEXT_OFFSET, atrac3Context);
		}

		protected internal virtual int getSasCoreAtrac3Context(int sasCore, int voice)
		{
			Memory mem = Memory.Instance;
			return mem.read32(sasCore + SASCORE_VOICE_SIZE * voice + SASCORE_ATRAC3_CONTEXT_OFFSET);
		}

		/// <summary>
		/// Set the ADSR rates for a specific voice.
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <param name="voice">       voice number, [0..31] </param>
		/// <param name="flag">        Bitfield to indicate which of the following 4 parameters
		///                    has to be updated. Logical OR of the following values:
		///                        0x1: update attack rate
		///                        0x2: update decay rate
		///                        0x4: update sustain rate
		///                        0x8: update release rate </param>
		/// <param name="attack">      Envelope's attack rate, [0..0x7FFFFFFF]. </param>
		/// <param name="decay">       Envelope's decay rate, [0..0x7FFFFFFF]. </param>
		/// <param name="sustain">     Envelope's sustain rate, [0..0x7FFFFFFF]. </param>
		/// <param name="release">     Envelope's release rate, [0..0x7FFFFFFF]. </param>
		/// <returns>            0 if OK
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		///                    ERROR_SAS_INVALID_VOICE if an invalid voice number is provided </returns>
		[HLEFunction(nid : 0x019B25EB, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetADSR(int sasCore, int voice, int flag, int attack, int decay, int sustain, int release)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);

			SoundVoice.VoiceADSREnvelope envelope = voices[voice].Envelope;
			if ((flag & 0x1) != 0)
			{
				envelope.AttackRate = attack;
			}
			if ((flag & 0x2) != 0)
			{
				envelope.DecayRate = decay;
			}
			if ((flag & 0x4) != 0)
			{
				envelope.SustainRate = sustain;
			}
			if ((flag & 0x8) != 0)
			{
				envelope.ReleaseRate = release;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("__sceSasSetADSR voice=0x{0:X}: {1}", voice, envelope.ToString()));
			}

			return 0;
		}

		/// <summary>
		/// Set the wave form effect delay and feedback parameters (unknown parameters).
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <param name="delay">       (unknown) wave form effect delay </param>
		/// <param name="feedback">    (unknown) wave form effect feedback </param>
		/// <returns>            0 if OK
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		/// @return </returns>
		[HLEFunction(nid : 0x267A6DD2, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasRevParam(int sasCore, int delay, int feedback)
		{
			checkSasHandleGood(sasCore);

			waveformEffectDelay = delay;
			waveformEffectFeedback = feedback;

			return 0;
		}

		/// <summary>
		/// Get the pause flag for all the voices.
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <returns>            bitfield with bit 0 for voice 0, bit 1 for voice 1...
		///                       bit=0, corresponding voice is not paused
		///                       bit=1, corresponding voice is paused
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided </returns>
		[HLEFunction(nid : 0x2C8E6AB3, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasGetPauseFlag(int sasCore)
		{
			checkSasHandleGood(sasCore);

			int pauseFlag = 0;
			for (int i = 0; i < voices.Length; i++)
			{
				if (voices[i].Paused)
				{
					pauseFlag |= (1 << i);
				}
			}

			return pauseFlag;
		}

		/// <summary>
		/// Set the wave form effect type (unknown parameter).
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <param name="type">        unknown parameter </param>
		/// <returns>            wave form effect type
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided </returns>
		[HLEFunction(nid : 0x33D4AB37, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasRevType(int sasCore, int type)
		{
			checkSasHandleGood(sasCore);

			waveformEffectType = type;

			return 0;
		}

		/// <summary>
		/// Initialize a new sasCore handle.
		/// </summary>
		/// <param name="sasCore">     sasCore handle, must be a valid address
		///                    (Uid will be written at this address). </param>
		/// <param name="grain">       number of samples processed by one call to __sceSasCore </param>
		/// <param name="maxVoices">   number of voices (maximum 32) </param>
		/// <param name="outputMode">  (unknown) 0 stereo
		///                              1 multichannel </param>
		/// <param name="sampleRate">  the default sample rate (number of samples per second)
		///                    for all the voices </param>
		/// <returns>            0 </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x42778A9F, version = 150) public int __sceSasInit(@CanBeNull pspsharp.HLE.TPointer sasCore, int grain, int maxVoices, int outputMode, int sampleRate)
		[HLEFunction(nid : 0x42778A9F, version : 150)]
		public virtual int __sceSasInit(TPointer sasCore, int grain, int maxVoices, int outputMode, int sampleRate)
		{
			checkSasAddressGood(sasCore.Address);

			if (grain < 0x40 || grain > 0x800 || (grain & 0x1F) != 0)
			{
				return SceKernelErrors.ERROR_SAS_INVALID_GRAIN;
			}
			if (sampleRate != 44100)
			{
				return SceKernelErrors.ERROR_SAS_INVALID_SAMPLE_RATE;
			}
			if (maxVoices <= 0 || maxVoices > PSP_SAS_VOICES_MAX)
			{
				return SceKernelErrors.ERROR_SAS_INVALID_MAX_VOICES;
			}
			if (outputMode != PSP_SAS_OUTPUTMODE_STEREO && outputMode != PSP_SAS_OUTPUTMODE_MONO)
			{
				return SceKernelErrors.ERROR_SAS_INVALID_OUTPUT_MODE;
			}

			if (sasCoreUid != -1)
			{
				// Only one Sas core can be active at a time.
				// If a previous Uid was allocated, release it.
				SceUidManager.releaseUid(sasCoreUid, sasCodeUidPurpose);
			}

			// Size of SasCore structure is 0xE20 bytes
			sasCore.clear(0xE20);

			sasCoreUid = SceUidManager.getNewUid(sasCodeUidPurpose);
			sasCore.setValue32(0, sasCoreUid);

			grainSamples = grain;
			this.outputMode = outputMode;
			for (int i = 0; i < voices.Length; i++)
			{
				voices[i].SampleRate = sampleRate; // Set default sample rate
			}

			return 0;
		}

		/// <summary>
		/// Set the volume for one voice.
		/// </summary>
		/// <param name="sasCore">           sasCore handle </param>
		/// <param name="voice">             voice number </param>
		/// <param name="leftVolume">        Left channel volume, [0..0x1000]. </param>
		/// <param name="rightVolume">       Right channel volume, [0..0x1000]. </param>
		/// <param name="effectLeftVolume">  (unknown) Left effect channel volume, [0..0x1000]. </param>
		/// <param name="effectRightVolume"> (unknown) Right effect channel volume, [0..0x1000]. </param>
		/// <returns> 0                if OK
		///                          ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		///                          ERROR_SAS_INVALID_VOICE if an invalid voice number is provided </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x440CA7D8, version = 150, checkInsideInterrupt = true) public int __sceSasSetVolume(int sasCore, int voice, @CheckArgument("checkVolume") int leftVolume, @CheckArgument("checkVolume") int rightVolume, @CheckArgument("checkVolume") int effectLeftVolumne, @CheckArgument("checkVolume") int effectRightVolume)
		[HLEFunction(nid : 0x440CA7D8, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetVolume(int sasCore, int voice, int leftVolume, int rightVolume, int effectLeftVolumne, int effectRightVolume)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);

			voices[voice].LeftVolume = leftVolume << 3; // 0 - 0x8000
			voices[voice].RightVolume = rightVolume << 3; // 0 - 0x8000

			return 0;
		}

		/// <summary>
		/// Process the voices and generate the next samples.
		/// Mix the resulting samples in an exiting buffer.
		/// </summary>
		/// <param name="sasCore">           sasCore handle </param>
		/// <param name="sasInOut">          address for the input and output buffer.
		///                          Samples are stored as 2 16-bit values
		///                          (left then right channel samples) </param>
		/// <param name="leftVolume">        Left channel volume, [0..0x1000]. </param>
		/// <param name="rightVolume">       Right channel volume, [0..0x1000]. </param>
		/// <returns> 0                if OK
		///                          ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided </returns>
		[HLEFunction(nid : 0x50A14DFC, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasCoreWithMix(int sasCore, int sasInOut, int leftVolume, int rightVolume)
		{
			checkSasHandleGood(sasCore);

			long startTime = Emulator.Clock.microTime();
			mixer.synthesizeWithMix(sasInOut, grainSamples, leftVolume << 3, rightVolume << 3);
			delayThreadSasCore(startTime);

			return 0;
		}

		/// <summary>
		/// Set the sustain level for one voice.
		/// </summary>
		/// <param name="sasCore">           sasCore handle </param>
		/// <param name="voice">             voice number [0..31] </param>
		/// <param name="level">             sustain level [0..0x40000000] </param>
		/// <returns> 0                if OK
		///                          ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		///                          ERROR_SAS_INVALID_VOICE if an invalid voice number is provided </returns>
		[HLEFunction(nid : 0x5F9529F6, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetSL(int sasCore, int voice, int level)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);

			voices[voice].Envelope.SustainLevel = level;

			return 0;
		}

		/// <summary>
		/// Get the end flag for all the voices.
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <returns>            bitfield with bit 0 for voice 0, bit 1 for voice 1...
		///                       bit=0, corresponding voice is not ended
		///                       bit=1, corresponding voice is ended
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided </returns>
		[HLEFunction(nid : 0x68A46B95, version : 150)]
		public virtual int __sceSasGetEndFlag(int sasCore)
		{
			checkSasHandleGood(sasCore);

			int endFlag = 0;
			for (int i = 0; i < voices.Length; i++)
			{
				if (voices[i].Ended)
				{
					endFlag |= (1 << i);
				}
			}

			return endFlag;
		}

		/// <summary>
		/// Get the current envelope height for one voice.
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <param name="voice">       voice number [0..31] </param>
		/// <returns>            envelope height [0..0x40000000]
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		///                    ERROR_SAS_INVALID_VOICE if an invalid voice number is provided </returns>
		[HLEFunction(nid : 0x74AE582A, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasGetEnvelopeHeight(int sasCore, int voice)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);

			return voices[voice].Envelope.height;
		}

		/// <summary>
		/// Set one voice on.
		/// </summary>
		/// <param name="sasCore">           sasCore handle </param>
		/// <param name="voice">             voice number [0..31] </param>
		/// <returns> 0                if OK
		///                          ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		///                          ERROR_SAS_INVALID_VOICE if an invalid voice number is provided
		///                          ERROR_SAS_VOICE_PAUSED if the voice was paused or already on </returns>
		[HLEFunction(nid : 0x76F01ACA, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetKeyOn(int sasCore, int voice)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);
			checkVoiceNotPaused(voice, false);

			voices[voice].on();

			return 0;
		}

		/// <summary>
		/// Set or reset the pause parameter for the voices.
		/// </summary>
		/// <param name="sasCore">           sasCore handle </param>
		/// <param name="voice_bit">         a bitfield with bit 0 for voice 0, bit 1 for voice 1...
		///                          Only the bits with 1 are processed. </param>
		/// <param name="setPause">          when 0: reset the pause flag for all the voices
		///                                  having a bit 1 in the voice_bit field
		///                          when non-0: set the pause flag for all the voices
		///                                  having a bit 1 in the voice_bit field </param>
		/// <returns> 0                if OK
		///                          ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided </returns>
		[HLEFunction(nid : 0x787D04D5, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetPause(int sasCore, int voice_bit, bool setPause)
		{
			checkSasHandleGood(sasCore);

			// Update only the pause flag of the voices
			// where the corresponding bit is set:
			// set or reset the pause flag according to the "setPause" parameter.
			for (int i = 0; voice_bit != 0; i++, voice_bit = (int)((uint)voice_bit >> 1))
			{
				if ((voice_bit & 1) != 0)
				{
					voices[i].Paused = setPause;
				}
			}

			return 0;
		}

		/// <summary>
		/// Set the VAG waveform data for one voice.
		/// </summary>
		/// <param name="sasCore">           sasCore handle </param>
		/// <param name="voice">             voice number [0..31] </param>
		/// <param name="vagAddr">           address of the VAG waveform data </param>
		/// <param name="size">              size in bytes of the VAG waveform data </param>
		/// <param name="loopmode">          0 ignore the VAG looping information
		///                          1 process the VAG looping information </param>
		/// <returns> 0                if OK
		///                          ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		///                          ERROR_SAS_INVALID_VOICE if an invalid voice number is provided
		///                          ERROR_SAS_INVALID_PARAMETER if an invalid size is provided </returns>
		[HLEFunction(nid : 0x99944089, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetVoice(int sasCore, int voice, int vagAddr, int size, int loopmode)
		{
			if (size <= 0 || (size & 0xF) != 0)
			{
				Console.WriteLine(string.Format("__sceSasSetVoice invalid size 0x{0:X8}", size));
				return SceKernelErrors.ERROR_SAS_INVALID_ADPCM_SIZE;
			}

			checkSasAndVoiceHandlesGood(sasCore, voice);

			voices[voice].setVAG(vagAddr, size);
			voices[voice].LoopMode = loopmode;

			return 0;
		}

		/// <summary>
		/// Set the ADSR curve types for a specific voice.
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <param name="voice">       voice number, [0..31] </param>
		/// <param name="flag">        Bitfield to indicate which of the following 4 parameters
		///                    has to be updated. Logical OR of the following values:
		///                        0x1: update attack curve type
		///                        0x2: update decay curve type
		///                        0x4: update sustain curve type
		///                        0x8: update release curve type </param>
		/// <param name="attack">      Envelope's attack curve type, [0..5]. </param>
		/// <param name="decay">       Envelope's decay curve type, [0..5]. </param>
		/// <param name="sustain">     Envelope's sustain curve type, [0..5]. </param>
		/// <param name="release">     Envelope's release curve type, [0..5]. </param>
		/// <returns>            0 if OK
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		///                    ERROR_SAS_INVALID_VOICE if an invalid voice number is provided
		///                    ERROR_SAS_INVALID_ADSR_CURVE_MODE if an invalid curve mode or curve mode combination is provided </returns>
		[HLEFunction(nid : 0x9EC3676A, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetADSRmode(int sasCore, int voice, int flag, int attackType, int decayType, int sustainType, int releaseType)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);

			checkADSRmode(0, flag, attackType);
			checkADSRmode(1, flag, decayType);
			checkADSRmode(2, flag, sustainType);
			checkADSRmode(3, flag, releaseType);

			SoundVoice.VoiceADSREnvelope envelope = voices[voice].Envelope;
			if ((flag & 0x1) != 0)
			{
				envelope.AttackCurveType = attackType;
			}
			if ((flag & 0x2) != 0)
			{
				envelope.DecayCurveType = decayType;
			}
			if ((flag & 0x4) != 0)
			{
				envelope.SustainCurveType = sustainType;
			}
			if ((flag & 0x8) != 0)
			{
				envelope.ReleaseCurveType = releaseType;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("__sceSasSetADSRmode voice=0x{0:X}: {1}", voice, envelope.ToString()));
			}

			return 0;
		}

		/// <summary>
		/// Set one voice off.
		/// </summary>
		/// <param name="sasCore">           sasCore handle </param>
		/// <param name="voice">             voice number [0..31] </param>
		/// <returns> 0                if OK
		///                          ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		///                          ERROR_SAS_INVALID_VOICE if an invalid voice number is provided
		///                          ERROR_SAS_VOICE_PAUSED if the voice was paused or already off </returns>
		[HLEFunction(nid : 0xA0CF2FA4, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetKeyOff(int sasCore, int voice)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);
			checkVoiceNotPaused(voice, true);

			voices[voice].off();

			return 0;
		}

		/// <summary>
		/// (Unknown) Set a triangular waveform for one voice.
		/// </summary>
		/// <param name="sasCore">           sasCore handle </param>
		/// <param name="voice">             voice number [0..31] </param>
		/// <param name="unknown">           unknown parameter </param>
		/// <returns> 0                if OK
		///                          ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		///                          ERROR_SAS_INVALID_VOICE if an invalid voice number is provided </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA232CBE6, version = 150, checkInsideInterrupt = true) public int __sceSasSetTriangularWave(int sasCore, int voice, int unknown)
		[HLEFunction(nid : 0xA232CBE6, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetTriangularWave(int sasCore, int voice, int unknown)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);

			return 0;
		}

		/// <summary>
		/// Process the voices and generate the next samples.
		/// </summary>
		/// <param name="sasCore">           sasCore handle </param>
		/// <param name="sasOut">            address for the output buffer.
		///                          Samples are stored as 2 16-bit values
		///                          (left then right channel samples) </param>
		/// <returns> 0                if OK
		///                          ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided </returns>
		[HLEFunction(nid : 0xA3589D81, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasCore(int sasCore, int sasOut)
		{
			checkSasHandleGood(sasCore);

			long startTime = Emulator.Clock.microTime();
			mixer.synthesize(sasOut, grainSamples);
			delayThreadSasCore(startTime);

			return 0;
		}

		/// <summary>
		/// Set the pitch of one voice.
		/// </summary>
		/// <param name="sasCore">           sasCore handle </param>
		/// <param name="voice">             voice number [0..31] </param>
		/// <param name="pitch">             the pitch value, [1..0x4000] </param>
		/// <returns> 0                if OK
		///                          ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		///                          ERROR_SAS_INVALID_VOICE if an invalid voice number is provided </returns>
		[HLEFunction(nid : 0xAD84D37F, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetPitch(int sasCore, int voice, int pitch)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);

			voices[voice].Pitch = pitch;

			return 0;
		}

		/// <summary>
		/// (Unknown) Set a noise waveform for one voice.
		/// </summary>
		/// <param name="sasCore">           sasCore handle </param>
		/// <param name="voice">             voice number [0..31] </param>
		/// <param name="freq">              unknown parameter </param>
		/// <returns> 0                if OK
		///                          ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		///                          ERROR_SAS_INVALID_VOICE if an invalid voice number is provided </returns>
		[HLEFunction(nid : 0xB7660A23, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetNoise(int sasCore, int voice, int freq)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);

			voices[voice].Noise = freq;

			return 0;
		}

		/// <summary>
		/// Get the number of samples generated by one __sceSasCore call.
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <returns>            0 if OK
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		/// @return </returns>
		[HLEFunction(nid : 0xBD11B7C2, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasGetGrain(int sasCore)
		{
			checkSasHandleGood(sasCore);

			return grainSamples;
		}

		private int getSimpleSustainLevel(int bitfield1)
		{
			return ((bitfield1 & 0x000F) + 1) << 26;
		}

		private int getSimpleDecayRate(int bitfield1)
		{
			int bitShift = (bitfield1 >> 4) & 0x000F;
			if (bitShift == 0)
			{
				return PSP_SAS_ENVELOPE_FREQ_MAX;
			}
			return (int)((uint)0x80000000 >> bitShift);
		}

		private int getSimpleRate(int n)
		{
			n &= 0x7F;
			if (n == 0x7F)
			{
				return 0;
			}
			int rate = (int)((uint)((7 - (n & 0x3)) << 26) >> (n >> 2));
			if (rate == 0)
			{
				return 1;
			}
			return rate;
		}

		private int getSimpleExponentRate(int n)
		{
			n &= 0x7F;
			if (n == 0x7F)
			{
				return 0;
			}
			int rate = (int)((uint)((7 - (n & 0x3)) << 24) >> (n >> 2));
			if (rate == 0)
			{
				return 1;
			}
			return rate;
		}

		private int getSimpleAttackRate(int bitfield1)
		{
			return getSimpleRate(bitfield1 >> 8);
		}

		private int getSimpleAttackCurveType(int bitfield1)
		{
			return (bitfield1 & 0x8000) == 0 ? PSP_SAS_ADSR_CURVE_MODE_LINEAR_INCREASE : PSP_SAS_ADSR_CURVE_MODE_LINEAR_BENT;
		}

		private int getSimpleReleaseRate(int bitfield2)
		{
			int n = bitfield2 & 0x001F;
			if (n == 31)
			{
				return 0;
			}
			if (getSimpleReleaseCurveType(bitfield2) == PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE)
			{
				if (n == 30)
				{
					return 0x40000000;
				}
				else if (n == 29)
				{
					return 1;
				}
				return 0x10000000 >> n;
			}
			if (n == 0)
			{
				return PSP_SAS_ENVELOPE_FREQ_MAX;
			}
			return (int)((uint)0x80000000 >> n);
		}

		private int getSimpleReleaseCurveType(int bitfield2)
		{
			return (bitfield2 & 0x0020) == 0 ? PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE : PSP_SAS_ADSR_CURVE_MODE_EXPONENT_DECREASE;
		}

		private int getSimpleSustainRate(int bitfield2)
		{
			if (getSimpleSustainCurveType(bitfield2) == PSP_SAS_ADSR_CURVE_MODE_EXPONENT_DECREASE)
			{
				return getSimpleExponentRate(bitfield2 >> 6);
			}
			return getSimpleRate(bitfield2 >> 6);
		}

		private int getSimpleSustainCurveType(int bitfield2)
		{
			switch (bitfield2 >> 13)
			{
				case 0:
					return PSP_SAS_ADSR_CURVE_MODE_LINEAR_INCREASE;
				case 2:
					return PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE;
				case 4:
					return PSP_SAS_ADSR_CURVE_MODE_LINEAR_BENT;
				case 6:
					return PSP_SAS_ADSR_CURVE_MODE_EXPONENT_DECREASE;
			}

			throw new SceKernelErrorException(ERROR_SAS_INVALID_ADSR_CURVE_MODE);
		}

		/// <summary>
		/// Set the ADSR parameters for a specific voice with simplified parameters.
		/// The Decay curve type is always exponential decrease.
		/// 
		/// Simple Rate coding: bitfield [0..0x7F]
		///   0x7F: rate = 0
		///   Bits [0..1]: 0x0: base rate=0x1C000000
		///                0x1: base rate=0x18000000
		///                0x2: base rate=0x14000000
		///                0x3: base rate=0x10000000
		///   Bits [2..6]: number of bits to logically shift the base rate to the right
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <param name="voice">       voice number, [0..31] </param>
		/// <param name="ADSREnv1">    ADSR bitfield 1
		///                    Bits [0..3]: Sustain Level, coded as the bits [29..26]-1
		///                                 of the sustain level
		///                    Bits [4..7]: Decay Rate, coded as the number of bits to
		///                                 logically shift 0x80000000 to the right
		///                    Bits [8..14]: Attack Rate, coded as a Simple Rate
		///                    Bit  [15]: Attack curve type
		///                               (0=linear increase, 1=linear bent) </param>
		/// <param name="ADSREnv2">    ADSR bitfield 2
		///                    Bits [0..4]: Release Rate
		///                                 0x1F: release rate = 0
		///                                 [0..0x1E]: n
		///                                    if release curve type is linear decrease
		///                                      release rate = 0x40000000 >>> (n+2)
		///                                    else
		///                                      release rate = 0x80000000 >>> n
		///                    Bit  [5]: Release curve type
		///                              (0=linear decrease, 1=exponential decrease)
		///                    Bits [6..12]: Sustain Rate, coded as a Simple Rate
		///                    Bits [13..15]: Sustain curve type
		///                                   (0=linear increase,
		///                                    2=linear decrease,
		///                                    4=linear bent,
		///                                    6=exponential decrease,
		///                                    other values are invalid) </param>
		/// <returns>            0 if OK
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		///                    ERROR_SAS_INVALID_VOICE if an invalid voice number is provided
		///                    ERROR_SAS_INVALID_ADSR_CURVE_MODE if an invalid sustain curve type is provided </returns>
		[HLEFunction(nid : 0xCBCD4F79, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetSimpleADSR(int sasCore, int voice, int ADSREnv1, int ADSREnv2)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);

			// Only the low-order 16 bits are valid for both parameters.
			int env1Bitfield = (ADSREnv1 & 0xFFFF);
			int env2Bitfield = (ADSREnv2 & 0xFFFF);

			// The bitfields represent every value except for the decay curve shape,
			// which seems to be unchanged in simple mode.
			SoundVoice.VoiceADSREnvelope envelope = voices[voice].Envelope;
			envelope.SustainLevel = getSimpleSustainLevel(env1Bitfield);
			envelope.DecayRate = getSimpleDecayRate(env1Bitfield);
			envelope.DecayCurveType = PSP_SAS_ADSR_CURVE_MODE_EXPONENT_DECREASE;
			envelope.AttackRate = getSimpleAttackRate(env1Bitfield);
			envelope.AttackCurveType = getSimpleAttackCurveType(env1Bitfield);

			envelope.ReleaseRate = getSimpleReleaseRate(env2Bitfield);
			envelope.ReleaseCurveType = getSimpleReleaseCurveType(env2Bitfield);
			envelope.SustainRate = getSimpleSustainRate(env2Bitfield);
			envelope.SustainCurveType = getSimpleSustainCurveType(env2Bitfield);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("__sceSasSetSimpleADSR voice=0x{0:X}: {1}", voice, envelope.ToString()));
			}

			return 0;
		}

		/// <summary>
		/// Set the number of samples generated by one __sceSasCore call.
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <param name="grain">       number of samples </param>
		/// <returns>            0 if OK
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided </returns>
		[HLEFunction(nid : 0xD1E0A01E, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetGrain(int sasCore, int grain)
		{
			checkSasHandleGood(sasCore);

			grainSamples = grain;

			return 0;
		}

		/// <summary>
		/// Set the wave form effect volume (unknown parameters).
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <param name="leftVolume">  unknown parameter </param>
		/// <param name="rightVolume"> unknown parameter </param>
		/// <returns>            wave form effect type
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided </returns>
		[HLEFunction(nid : 0xD5A229C9, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasRevEVOL(int sasCore, int leftVolume, int rightVolume)
		{
			checkSasHandleGood(sasCore);

			waveformEffectLeftVol = leftVolume;
			waveformEffectRightVol = rightVolume;

			return 0;
		}

		/// <summary>
		/// (Unknown) Set a steep waveform for one voice.
		/// </summary>
		/// <param name="sasCore">           sasCore handle </param>
		/// <param name="voice">             voice number [0..31] </param>
		/// <param name="unknown">           unknown parameter </param>
		/// <returns> 0                if OK
		///                          ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		///                          ERROR_SAS_INVALID_VOICE if an invalid voice number is provided </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD5EBBBCD, version = 150, checkInsideInterrupt = true) public int __sceSasSetSteepWave(int sasCore, int voice, int unknown)
		[HLEFunction(nid : 0xD5EBBBCD, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetSteepWave(int sasCore, int voice, int unknown)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);

			return 0;
		}

		/// <summary>
		/// (Unknown) Get the output mode.
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <returns>            (unknown) 0 stereo
		///                              1 multichannel
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided </returns>
		[HLEFunction(nid : 0xE175EF66, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasGetOutputmode(int sasCore)
		{
			checkSasHandleGood(sasCore);

			return OutputMode;
		}

		/// <summary>
		/// (Unknown) Set the output mode.
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <param name="outputMode">  (unknown) 0 stereo
		///                              1 multichannel </param>
		/// <returns> 0          if OK
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided </returns>
		[HLEFunction(nid : 0xE855BF76, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasSetOutputmode(int sasCore, int outputMode)
		{
			checkSasHandleGood(sasCore);
			this.outputMode = outputMode;

			return 0;
		}

		/// <summary>
		/// Set the wave form effect dry and wet status (unknown parameters).
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <param name="dry">         unknown parameter </param>
		/// <param name="wet">         unknown parameter </param>
		/// <returns>            wave form effect type
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided </returns>
		[HLEFunction(nid : 0xF983B186, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasRevVON(int sasCore, int dry, int wet)
		{
			checkSasHandleGood(sasCore);

			waveformEffectIsDryOn = (dry > 0);
			waveformEffectIsWetOn = (wet > 0);

			return 0;
		}

		/// <summary>
		/// Get the current envelope height for all the voices.
		/// </summary>
		/// <param name="sasCore">     sasCore handle </param>
		/// <param name="heightsAddr"> (int *) address where to return the envelope heights,
		///                    stored as 32 bit values [0..0x40000000].
		///                        heightsAddr[0] = envelope height of voice 0
		///                        heightsAddr[1] = envelope height of voice 1
		///                        ... </param>
		/// <returns>            0 if OK
		///                    ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x07F58C24, version = 150, checkInsideInterrupt = true) public int __sceSasGetAllEnvelopeHeights(int sasCore, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=PSP_SAS_VOICES_MAX*4, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 heightsAddr)
		[HLEFunction(nid : 0x07F58C24, version : 150, checkInsideInterrupt : true)]
		public virtual int __sceSasGetAllEnvelopeHeights(int sasCore, TPointer32 heightsAddr)
		{
			checkSasHandleGood(sasCore);

			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(heightsAddr.Address, voices.Length * 4, 4);
			for (int i = 0; i < voices.Length; i++)
			{
				int voiceHeight = voices[i].Envelope.height;
				memoryWriter.writeNext(voiceHeight);
				if (log.TraceEnabled && voiceHeight != 0)
				{
					log.trace(string.Format("__sceSasGetAllEnvelopeHeights height voice #{0:D}=0x{1:X8}", i, voiceHeight));
				}
			}
			memoryWriter.flush();

			return 0;
		}

		/// <summary>
		/// Identical to __sceSasSetVoice, but for raw PCM data (VAG/ADPCM is not allowed). </summary>
		[HLEFunction(nid : 0xE1CD9561, version : 500, checkInsideInterrupt : true)]
		public virtual int __sceSasSetVoicePCM(int sasCore, int voice, TPointer pcmAddr, int size, int loopmode)
		{
			if (size <= 0 || size > 0x10000)
			{
				Console.WriteLine(string.Format("__sceSasSetVoicePCM invalid size 0x{0:X8}", size));

				return SceKernelErrors.ERROR_SAS_INVALID_SIZE;
			}

			checkSasAndVoiceHandlesGood(sasCore, voice);

			voices[voice].setPCM(pcmAddr.Address, size);
			voices[voice].LoopMode = loopmode;

			return 0;
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x4AA9EAD6, version : 600, checkInsideInterrupt : true)]
		public virtual int __sceSasSetVoiceATRAC3(int sasCore, int voice, int atrac3Context)
		{
			// atrac3Context is the value returned by _sceAtracGetContextAddress

			checkSasAndVoiceHandlesGood(sasCore, voice);

			AtracID atracId = Modules.sceAtrac3plusModule.getAtracIdFromContext(atrac3Context);
			if (atracId != null)
			{
				voices[voice].AtracId = atracId;

				// Store the atrac3Context address into the sasCore structure.
				setSasCoreAtrac3Context(sasCore, voice, atrac3Context);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7497EA85, version = 600, checkInsideInterrupt = true) public int __sceSasConcatenateATRAC3(int sasCore, int voice, @CanBeNull pspsharp.HLE.TPointer atrac3DataAddr, int atrac3DataLength)
		[HLEFunction(nid : 0x7497EA85, version : 600, checkInsideInterrupt : true)]
		public virtual int __sceSasConcatenateATRAC3(int sasCore, int voice, TPointer atrac3DataAddr, int atrac3DataLength)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);

			int atrac3Context = getSasCoreAtrac3Context(sasCore, voice);
			AtracID atracID = Modules.sceAtrac3plusModule.getAtracIdFromContext(atrac3Context);
			if (atracID.SecondBufferAddr != -1)
			{
				return SceKernelErrors.ERROR_SAS_CANNOT_CONCATENATE_ATRA3;
			}
			atracID.setSecondBuffer(atrac3DataAddr.Address, atrac3DataLength);

			return 0;
		}

		[HLEFunction(nid : 0xF6107F00, version : 600, checkInsideInterrupt : true)]
		public virtual int __sceSasUnsetATRAC3(int sasCore, int voice)
		{
			checkSasAndVoiceHandlesGood(sasCore, voice);

			voices[voice].AtracId = null;

			// Reset the atrac3Context address
			setSasCoreAtrac3Context(sasCore, voice, 0);

			return 0;
		}
	}
}