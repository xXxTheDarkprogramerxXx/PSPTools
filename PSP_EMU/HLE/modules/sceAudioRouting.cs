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
	using Logger = org.apache.log4j.Logger;


	public class sceAudioRouting : HLEModule
	{
		public static Logger log = Modules.getLogger("sceAudioRouting");
		protected internal const int AUDIO_ROUTING_SPEAKER_OFF = 0;
		protected internal const int AUDIO_ROUTING_SPEAKER_ON = 1;
		protected internal int audioRoutingMode = AUDIO_ROUTING_SPEAKER_ON;
		protected internal int audioRoutineVolumeMode = AUDIO_ROUTING_SPEAKER_ON;

		/// <summary>
		/// Set routing mode.
		/// </summary>
		/// <param name="mode"> The routing mode to set (0 or 1)
		/// </param>
		/// <returns> the previous routing mode, or < 0 on error </returns>
		[HLELogging(level:"info"), HLEFunction(nid : 0x36FD8AA9, version : 150)]
		public virtual int sceAudioRoutingSetMode(int mode)
		{
			int previousMode = audioRoutingMode;

			audioRoutingMode = mode;

			return previousMode;
		}

		/// <summary>
		/// Get routing mode.
		/// </summary>
		/// <returns> the current routing mode. </returns>
		[HLEFunction(nid : 0x39240E7D, version : 150)]
		public virtual int sceAudioRoutingGetMode()
		{
			return audioRoutingMode;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x28235C56, version = 150) public int sceAudioRoutingGetVolumeMode()
		[HLEFunction(nid : 0x28235C56, version : 150)]
		public virtual int sceAudioRoutingGetVolumeMode()
		{
			return audioRoutineVolumeMode;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBB548475, version = 150) public int sceAudioRoutingSetVolumeMode(int mode)
		[HLEFunction(nid : 0xBB548475, version : 150)]
		public virtual int sceAudioRoutingSetVolumeMode(int mode)
		{
			int previousMode = audioRoutineVolumeMode;

			audioRoutineVolumeMode = mode;

			return previousMode;
		}
	}

}