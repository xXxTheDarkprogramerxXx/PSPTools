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
namespace pspsharp.media.codec.atrac3
{
	using AtracGainInfo = pspsharp.media.codec.atrac3plus.AtracGainInfo;

	public class GainBlock
	{
		public AtracGainInfo[] gBlock = new AtracGainInfo[4];

		public GainBlock()
		{
			for (int i = 0; i < gBlock.Length; i++)
			{
				gBlock[i] = new AtracGainInfo();
			}
		}
	}

}