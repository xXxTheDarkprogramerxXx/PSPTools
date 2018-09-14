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
namespace pspsharp.hardware
{
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using Settings = pspsharp.settings.Settings;

	public class Audio
	{
		public const int PSP_AUDIO_VOLUME_MIN = 0;
		public const int PSP_AUDIO_VOLUME_MAX = 0x8000;
		public const int PSP_AUDIO_VOLUME_STEP = 0x100;
		private static int volume = PSP_AUDIO_VOLUME_MAX;
		private static bool muted;
		private static AudioMutedSettingsListerner audioMutedSettingsListerner;

		private class AudioMutedSettingsListerner : AbstractBoolSettingsListener
		{
			protected internal override void settingsValueChanged(bool value)
			{
				Muted = value;
			}
		}

		public static int Volume
		{
			get
			{
				return volume;
			}
			set
			{
				if (value > PSP_AUDIO_VOLUME_MAX)
				{
					value = PSP_AUDIO_VOLUME_MAX;
				}
				else if (value < PSP_AUDIO_VOLUME_MIN)
				{
					value = PSP_AUDIO_VOLUME_MIN;
				}
    
				Audio.volume = value;
			}
		}


		private static void init()
		{
			if (audioMutedSettingsListerner == null)
			{
				audioMutedSettingsListerner = new AudioMutedSettingsListerner();
				Settings.Instance.registerSettingsListener("HardwareAudio", "emu.mutesound", audioMutedSettingsListerner);
			}
		}

		public static bool Muted
		{
			get
			{
				init();
				return muted;
			}
			set
			{
				init();
				Audio.muted = value;
			}
		}


		public static void setVolumeUp()
		{
			Volume = volume + PSP_AUDIO_VOLUME_STEP;
		}

		public static void setVolumeDown()
		{
			Volume = volume - PSP_AUDIO_VOLUME_STEP;
		}

		public static int getVolume(int volume)
		{
			if (Muted)
			{
				volume = 0;
			}
			else
			{
				volume = volume * Volume / PSP_AUDIO_VOLUME_MAX;
				if (volume < PSP_AUDIO_VOLUME_MIN)
				{
					volume = PSP_AUDIO_VOLUME_MIN;
				}
				else if (volume > PSP_AUDIO_VOLUME_MAX)
				{
					volume = PSP_AUDIO_VOLUME_MAX;
				}
			}

			return volume;
		}
	}

}