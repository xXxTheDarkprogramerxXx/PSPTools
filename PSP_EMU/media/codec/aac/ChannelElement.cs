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
namespace pspsharp.media.codec.aac
{
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// channel element - generic struct for SCE/CPE/CCE/LFE
	/// </summary>
	public class ChannelElement
	{
		// CPE specific
		public int commonWindow;
		public int msMode;
		public int[] msMask = new int[128];
		// shared
		public SingleChannelElement[] ch = new SingleChannelElement[2];
		// CCE specific
		public ChannelCoupling coup = new ChannelCoupling();
		public SpectralBandReplication sbr = new SpectralBandReplication();

		public ChannelElement()
		{
			for (int i = 0; i < ch.Length; i++)
			{
				ch[i] = new SingleChannelElement();
			}
		}

		public virtual void copy(ChannelElement that)
		{
			commonWindow = that.commonWindow;
			msMode = that.msMode;
			Utilities.copy(msMask, that.msMask);
			for (int i = 0; i < ch.Length; i++)
			{
				ch[i].copy(that.ch[i]);
			}
			coup.copy(that.coup);
			sbr.copy(that.sbr);
		}
	}

}