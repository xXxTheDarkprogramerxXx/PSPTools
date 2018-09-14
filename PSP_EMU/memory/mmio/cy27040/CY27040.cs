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
namespace pspsharp.memory.mmio.cy27040
{

	using Logger = org.apache.log4j.Logger;

	using IState = pspsharp.state.IState;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class CY27040 : IState
	{
		public static Logger log = Logger.getLogger("CY27040");
		private const int STATE_VERSION = 0;
		private static CY27040 instance;
		// In clock register, used to manage the audio frequency
		public const int PSP_CLOCK_AUDIO_FREQ = 0x01;
		// In clock register, used to enable/disable the lepton DSP
		public const int PSP_CLOCK_LEPTON = 0x08;
		// In clock register, used to enable/disable audio
		public const int PSP_CLOCK_AUDIO = 0x10;
		// Possible revisions: 3, 4, 7, 8, 9, 10 or 15
		private int revision;
		private int clock;
		private int spreadSpectrum;

		public static CY27040 Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CY27040();
				}
				return instance;
			}
		}

		private CY27040()
		{
			revision = 0x03;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			revision = stream.readInt();
			clock = stream.readInt();
			spreadSpectrum = stream.readInt();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(revision);
			stream.writeInt(clock);
			stream.writeInt(spreadSpectrum);
		}

		public virtual void executeTransmitReceiveCommand(int[] transmitData, int[] receiveData)
		{
			int command = transmitData[0] & 0xFF;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("executeTransmitReceiveCommand command=0x{0:X2} on {1}", command, this));
			}

			switch (command)
			{
				case 0x00: // Retrieve all the registers
					receiveData[0] = 3; // Returning 3 register values
					receiveData[1] = revision;
					receiveData[2] = clock;
					receiveData[3] = spreadSpectrum;
					break;
				case 0x80:
					receiveData[0] = revision;
					break;
				case 0x81:
					receiveData[0] = clock;
					break;
				case 0x82:
					receiveData[0] = spreadSpectrum;
					break;
				default:
					log.error(string.Format("executeTransmitReceiveCommand unknown command 0x{0:X}", command));
					break;
			}
		}

		public virtual void executeTransmitCommand(int[] transmitData)
		{
			int command = transmitData[0] & 0xFF;
			switch (command)
			{
				case 0x80:
					revision = transmitData[1] & 0xFF;
					break;
				case 0x81:
					clock = transmitData[1] & 0xFF;
					break;
				case 0x82:
					spreadSpectrum = transmitData[1] & 0xFF;
					break;
				default:
					log.error(string.Format("executeTransmitCommand unknown command 0x{0:X}", command));
					break;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("executeTransmitCommand command=0x{0:X2} on {1}", command, this));
			}
		}

		private int AudioFreq
		{
			get
			{
				return (clock & PSP_CLOCK_AUDIO_FREQ) == 0 ? 44100 : 48000;
			}
		}

		private bool AudioEnabled
		{
			get
			{
				return (clock & PSP_CLOCK_AUDIO) != 0;
			}
		}

		private bool LeptonEnabled
		{
			get
			{
				return (clock & PSP_CLOCK_LEPTON) != 0;
			}
		}

		private string ToString(bool enabled)
		{
			return enabled ? "enabled" : "disabled";
		}

		private string toStringClock()
		{
			return string.Format("{0:D}, audio {1}, lepton {2}", AudioFreq, ToString(AudioEnabled), ToString(LeptonEnabled));
		}

		public override string ToString()
		{
			return string.Format("CY27040 revision=0x{0:X2}, clock=0x{1:X2}({2}), spreadSpectrum=0x{3:X2}", revision, clock, toStringClock(), spreadSpectrum);
		}
	}

}