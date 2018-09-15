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
namespace pspsharp.format.rco.sound
{
	using BaseObject = pspsharp.format.rco.@object.BaseObject;
	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;

	public class Sound : BaseObject
	{
		private int format;
		private int channels;
		private int[] channelSize;
		private int[] channelOffset;

		public Sound(int format, int channels, int[] channelSize, int[] channelOffset)
		{
			this.format = format;
			this.channels = channels;
			this.channelSize = channelSize;
			this.channelOffset = channelOffset;
		}

		public virtual int Format
		{
			get
			{
				return format;
			}
		}

		public virtual int Channels
		{
			get
			{
				return channels;
			}
		}

		public virtual int getChannelSize(int channel)
		{
			return channelSize[channel];
		}

		public virtual int getChannelOffset(int channel)
		{
			return channelOffset[channel];
		}

		public virtual void play(VSMXBaseObject @object)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Sound.play"));
			}
		}
	}

}