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
namespace pspsharp.memory.mmio
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.managers.IntrManager.PSP_AUDIO_INTR;

	//using Logger = org.apache.log4j.Logger;

	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using sceAudio = pspsharp.HLE.modules.sceAudio;
	using AudioLine = pspsharp.memory.mmio.audio.AudioLine;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class MMIOHandlerAudio : MMIOHandlerBase
	{
		public static new Logger log = sceAudio.log;
		private const int STATE_VERSION = 0;
		public const int AUDIO_HW_FREQUENCY_8000 = 0x01;
		public const int AUDIO_HW_FREQUENCY_11025 = 0x02;
		public const int AUDIO_HW_FREQUENCY_12000 = 0x04;
		public const int AUDIO_HW_FREQUENCY_16000 = 0x08;
		public const int AUDIO_HW_FREQUENCY_22050 = 0x10;
		public const int AUDIO_HW_FREQUENCY_24000 = 0x20;
		public const int AUDIO_HW_FREQUENCY_32000 = 0x40;
		public const int AUDIO_HW_FREQUENCY_44100 = 0x80;
		public const int AUDIO_HW_FREQUENCY_48000 = 0x100;
		private int busy;
		private int interrupt;
		private int inProgress;
		private int flags10;
		private int flags20;
		private int flags24;
		private int flags2C;
		private int volume;
		private int frequency0;
		private int frequency1;
		private int frequencyFlags;
		private int hardwareFrequency;
		private readonly int[] audioData0 = new int[24 + (256 / 4)];
		private readonly int[] audioData1 = new int[24 + (256 / 4)];
		private int audioDataIndex0;
		private int audioDataIndex1;
		private readonly AudioLine[] audioLines = new AudioLine[2];

		public MMIOHandlerAudio(int baseAddress) : base(baseAddress)
		{

			for (int i = 0; i < audioLines.Length; i++)
			{
				audioLines[i] = new AudioLine();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			busy = stream.readInt();
			interrupt = stream.readInt();
			inProgress = stream.readInt();
			flags10 = stream.readInt();
			flags20 = stream.readInt();
			flags24 = stream.readInt();
			flags2C = stream.readInt();
			volume = stream.readInt();
			frequency0 = stream.readInt();
			frequency1 = stream.readInt();
			frequencyFlags = stream.readInt();
			hardwareFrequency = stream.readInt();
			stream.readInts(audioData0);
			stream.readInts(audioData1);
			audioDataIndex0 = stream.readInt();
			audioDataIndex1 = stream.readInt();
			for (int i = 0; i < audioLines.Length; i++)
			{
				audioLines[i].read(stream);
			}
			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(busy);
			stream.writeInt(interrupt);
			stream.writeInt(inProgress);
			stream.writeInt(flags10);
			stream.writeInt(flags20);
			stream.writeInt(flags24);
			stream.writeInt(flags2C);
			stream.writeInt(volume);
			stream.writeInt(frequency0);
			stream.writeInt(frequency1);
			stream.writeInt(frequencyFlags);
			stream.writeInt(hardwareFrequency);
			stream.writeInts(audioData0);
			stream.writeInts(audioData1);
			stream.writeInt(audioDataIndex0);
			stream.writeInt(audioDataIndex1);
			for (int i = 0; i < audioLines.Length; i++)
			{
				audioLines[i].write(stream);
			}
			base.write(stream);
		}

		private int Flags24
		{
			set
			{
				this.flags24 = value;
				interrupt &= value;
    
				checkInterrupt();
			}
		}

		private void checkInterrupt()
		{
			if (interrupt != 0)
			{
				RuntimeContextLLE.triggerInterrupt(Processor, PSP_AUDIO_INTR);
			}
			else
			{
				RuntimeContextLLE.clearInterrupt(Processor, PSP_AUDIO_INTR);
			}
		}

		private int sendAudioData(int value, int line, int index, int[] data, int interrupt)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("sendAudioData value=0x{0:X8}, line={1:D}, index=0x{2:X}, interrupt={3:D}", value, line, index, interrupt));
			}
			data[index++] = value;

			if (index >= data.Length)
			{
				audioLines[line].writeAudioData(data, 0, index);

				if (log.TraceEnabled)
				{
					log.trace(string.Format("sendAudioData line#{0:D}:", line));
					StringBuilder sb = new StringBuilder();
					for (int i = 0; i < data.Length; i++)
					{
						if (sb.Length > 0)
						{
							sb.Append(" ");
						}
						else
						{
							sb.Append("    ");
						}

						sb.Append(string.Format("0x{0:X4} 0x{1:X4}", data[i] & 0xFFFF, (int)((uint)data[i] >> 16)));

						if (((i + 1) % 4) == 0)
						{
							log.trace(sb.ToString());
							sb.Length = 0;
						}
					}
				}
				index = 0;
			}

			return index;
		}

		private void startAudio(int flags)
		{
			if ((flags & 0x1) == 0)
			{
				audioDataIndex0 = 0;
				inProgress |= 1;
			}
			if ((flags & 0x2) == 0)
			{
				audioDataIndex1 = 0;
				inProgress |= 2;
			}
		}

		private void stopAudio(int flags)
		{
			if ((inProgress & 0x1) != 0 && (flags & 0x1) == 0)
			{
				inProgress &= ~0x1;
			}
			if ((inProgress & 0x2) != 0 && (flags & 0x2) == 0)
			{
				inProgress &= ~0x2;
			}
			if ((inProgress & 0x4) != 0 && (flags & 0x4) == 0)
			{
				inProgress &= ~0x4;
			}
		}

		private static int getFrequencyValue(int hwFrequency)
		{
			switch (hwFrequency)
			{
				case AUDIO_HW_FREQUENCY_8000 :
					return 8000;
				case AUDIO_HW_FREQUENCY_11025:
					return 11025;
				case AUDIO_HW_FREQUENCY_12000:
					return 12000;
				case AUDIO_HW_FREQUENCY_16000:
					return 16000;
				case AUDIO_HW_FREQUENCY_22050:
					return 22050;
				case AUDIO_HW_FREQUENCY_24000:
					return 24000;
				case AUDIO_HW_FREQUENCY_32000:
					return 32000;
				case AUDIO_HW_FREQUENCY_44100:
					return 44100;
				case AUDIO_HW_FREQUENCY_48000:
					return 48000;
			}
			return hwFrequency;
		}

		private int Frequency0
		{
			set
			{
				this.frequency0 = value;
				updateAudioLineFrequency();
			}
		}

		private int Frequency1
		{
			set
			{
				this.frequency1 = value;
				updateAudioLineFrequency();
			}
		}

		private int HardwareFrequency
		{
			set
			{
				this.hardwareFrequency = value;
				updateAudioLineFrequency();
			}
		}

		private void updateAudioLineFrequency()
		{
			// TODO In the VSH, only frequency0 is being set. When to use frequency1 and hardwareFrequency?
			int frequency = getFrequencyValue(frequency0);
			for (int i = 0; i < audioLines.Length; i++)
			{
				audioLines[i].Frequency = frequency;
			}
		}

		private int Volume
		{
			set
			{
				this.volume = value;
				updateAudioLineVolume();
			}
		}

		private void updateAudioLineVolume()
		{
			for (int i = 0; i < audioLines.Length; i++)
			{
				audioLines[i].Volume = volume;
			}
		}

		public override int read32(int address)
		{
			int value;
			switch (address - baseAddress)
			{
				case 0x00:
					value = busy;
					break;
				case 0x0C:
					value = inProgress;
					break;
				case 0x1C:
					value = interrupt;
					break;
				case 0x28:
					value = 0x37;
					break; // flags when some actions are completed?
					goto case 0x50;
				case 0x50:
					value = 0;
					break; // This doesn't seem to return the volume value
					goto default;
				default:
					value = base.read32(address);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - read32(0x{1:X8}) returning 0x{2:X8}", Pc, address, value));
			}

			return value;
		}

		public override void write32(int address, int value)
		{
			switch (address - baseAddress)
			{
				case 0x00:
					busy = value;
					break;
				case 0x04:
					stopAudio(value);
					break;
				case 0x08:
					startAudio(value);
					break;
				case 0x10:
					flags10 = value;
					break;
				case 0x14:
					if (value != 0x1208)
					{
						base.write32(address, value);
					}
					break;
				case 0x18:
					if (value != 0x0)
					{
						base.write32(address, value);
					}
					break;
				case 0x20:
					flags20 = value;
					break;
				case 0x24:
					Flags24 = value;
					break;
				case 0x2C:
					flags2C = value;
					break;
				case 0x38:
					Frequency0 = value;
					break;
				case 0x3C:
					Frequency1 = value;
					break;
				case 0x40:
					frequencyFlags = value;
					break;
				case 0x44:
					HardwareFrequency = value;
					break;
				case 0x50:
					Volume = value;
					break;
				case 0x60:
					audioDataIndex0 = sendAudioData(value, 0, audioDataIndex0, audioData0, 1);
					break;
				case 0x70:
					audioDataIndex1 = sendAudioData(value, 1, audioDataIndex1, audioData1, 2);
					break;
				default:
					base.write32(address, value);
					break;
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("0x{0:X8} - write32(0x{1:X8}, 0x{2:X8}) on {3}", Pc, address, value, this));
			}
		}

		public override string ToString()
		{
			return string.Format("busy=0x{0:X}, interrupt=0x{1:X}, inProgress=0x{2:X}, flags10=0x{3:X}, flags20=0x{4:X}, flags24=0x{5:X}, flags2C=0x{6:X}, volume=0x{7:X}, frequency0=0x{8:X}({9:D}), frequency1=0x{10:X}({11:D}), frequencyFlags=0x{12:X}, hardwareFrequency=0x{13:X}({14:D})", busy, interrupt, inProgress, flags10, flags20, flags24, flags2C, volume, frequency0, getFrequencyValue(frequency0), frequency1, getFrequencyValue(frequency1), frequencyFlags, hardwareFrequency, getFrequencyValue(hardwareFrequency));
		}
	}

}