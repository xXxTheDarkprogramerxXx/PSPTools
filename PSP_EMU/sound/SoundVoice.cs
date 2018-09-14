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
//	import static pspsharp.HLE.modules.sceSasCore.PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSasCore.PSP_SAS_ADSR_CURVE_MODE_LINEAR_INCREASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSasCore.PSP_SAS_PITCH_BASE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSasCore.getSasADSRCurveTypeName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.hardware.Audio.PSP_AUDIO_VOLUME_MAX;
	using AtracID = pspsharp.HLE.modules.sceAtrac3plus.AtracID;

	public class SoundVoice
	{
		private bool changed;
		private int leftVolume;
		private int rightVolume;
		private int sampleRate;
		private int vagAddress;
		private int vagSize;
		private AtracID atracId;
		private int pcmAddress;
		private int pcmSize;
		private int loopMode;
		private int pitch;
		private int noise;
		private bool playing;
		private bool paused;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool on_Renamed;
		private VoiceADSREnvelope envelope;
		private int playSample;
		private int index;

		public class VoiceADSREnvelope
		{
			private readonly SoundVoice outerInstance;

			public int AttackRate;
			public int DecayRate;
			public int SustainRate;
			public int ReleaseRate;
			public int AttackCurveType;
			public int DecayCurveType;
			public int SustainCurveType;
			public int ReleaseCurveType;
			public int SustainLevel;
			public int height;

			public VoiceADSREnvelope(SoundVoice outerInstance)
			{
				this.outerInstance = outerInstance;
				// Default values (like on PSP)
				AttackRate = 0;
				DecayRate = 0;
				SustainRate = 0;
				ReleaseRate = 0;
				AttackCurveType = PSP_SAS_ADSR_CURVE_MODE_LINEAR_INCREASE;
				DecayCurveType = PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE;
				SustainCurveType = PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE;
				ReleaseCurveType = PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE;
				SustainLevel = 0;
				height = 0;
			}

			public override string ToString()
			{
				return string.Format("VoiceADSREnvelope[AR 0x{0:X8}({1}), DR 0x{2:X8}({3}), SR 0x{4:X8}({5}, SL 0x{6:X8}), RR 0x{7:X8}({8})]", AttackRate, getSasADSRCurveTypeName(AttackCurveType), DecayRate, getSasADSRCurveTypeName(DecayCurveType), SustainRate, getSasADSRCurveTypeName(SustainCurveType), SustainLevel, ReleaseRate, getSasADSRCurveTypeName(ReleaseCurveType));
			}
		}

		public SoundVoice(int index)
		{
			this.index = index;
			changed = true;
			leftVolume = PSP_AUDIO_VOLUME_MAX;
			rightVolume = PSP_AUDIO_VOLUME_MAX;
			vagAddress = 0;
			vagSize = 0;
			pcmAddress = 0;
			pcmSize = 0;
			loopMode = 0;
			pitch = PSP_SAS_PITCH_BASE;
			noise = 0;
			playing = false;
			paused = false;
			on_Renamed = false;
			envelope = new VoiceADSREnvelope(this);
			playSample = 0;
			atracId = null;
		}

		public virtual void onVoiceChanged()
		{
			changed = true;
			if (On && !Playing)
			{
				// A parameter was changed while the voice was ON but no longer playing.
				// Restart playing.
				Playing = true;
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


		public virtual void on()
		{
			on_Renamed = true;
			Playing = true;
		}

		public virtual void off()
		{
			on_Renamed = false;
		}

		public virtual VoiceADSREnvelope Envelope
		{
			get
			{
				return envelope;
			}
		}

		public virtual bool On
		{
			get
			{
				return on_Renamed;
			}
		}

		public virtual bool Playing
		{
			get
			{
				return playing;
			}
			set
			{
				playSample = 0;
				if (!value)
				{
					envelope.height = 0;
					off();
				}
				this.playing = value;
			}
		}


		public virtual bool Paused
		{
			get
			{
				return paused;
			}
			set
			{
				this.paused = value;
			}
		}


		public virtual void setVAG(int address, int size)
		{
			vagAddress = address;
			vagSize = size;
			atracId = null;
			pcmAddress = 0;
			onVoiceChanged();
		}

		public virtual int VAGAddress
		{
			get
			{
				return vagAddress;
			}
		}

		public virtual int VAGSize
		{
			get
			{
				return vagSize;
			}
		}

		public virtual int LoopMode
		{
			get
			{
				return loopMode;
			}
			set
			{
				if (this.loopMode != value)
				{
					this.loopMode = value;
					onVoiceChanged();
				}
			}
		}


		public virtual int Pitch
		{
			get
			{
				return pitch;
			}
			set
			{
				this.pitch = value;
			}
		}


		public virtual int Noise
		{
			get
			{
				return noise;
			}
			set
			{
				if (this.noise != value)
				{
					this.noise = value;
					onVoiceChanged();
				}
			}
		}


		public virtual int PlaySample
		{
			get
			{
				return playSample;
			}
			set
			{
				this.playSample = value;
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
				this.sampleRate = value;
			}
		}


		public virtual bool Ended
		{
			get
			{
				return !Playing;
			}
		}

		public virtual bool Changed
		{
			get
			{
				return changed;
			}
			set
			{
				this.changed = value;
			}
		}


		public virtual int Index
		{
			get
			{
				return index;
			}
		}

		public override string ToString()
		{
			return string.Format("SoundVoice #{0:D}", index);
		}

		public virtual AtracID AtracId
		{
			get
			{
				return atracId;
			}
			set
			{
				this.atracId = value;
				pcmAddress = 0;
				vagAddress = 0;
				onVoiceChanged();
			}
		}


		public virtual void setPCM(int address, int size)
		{
			pcmAddress = address;
			pcmSize = size;
			atracId = null;
			vagAddress = 0;
			onVoiceChanged();
		}

		public virtual int PcmAddress
		{
			get
			{
				return pcmAddress;
			}
		}

		public virtual int PcmSize
		{
			get
			{
				return pcmSize;
			}
		}
	}
}