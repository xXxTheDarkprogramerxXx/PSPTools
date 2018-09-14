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
namespace pspsharp.format.psmf
{
	/// <summary>
	/// PES Header in a PSMF/MPEG file.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class PesHeader
	{
		private long pts;
		private long dts;
		private int channel;

		public PesHeader(int channel)
		{
			Pts = 0;
			Dts = 0;
			Channel = channel;
		}

		public PesHeader(PesHeader pesHeader)
		{
			pts = pesHeader.pts;
			dts = pesHeader.dts;
			channel = pesHeader.channel;
		}

		public virtual long Pts
		{
			get
			{
				return pts;
			}
			set
			{
				this.pts = value;
			}
		}


		public virtual long Dts
		{
			get
			{
				return dts;
			}
			set
			{
				this.dts = value;
			}
		}


		public virtual long DtsPts
		{
			set
			{
				this.dts = value;
				this.pts = value;
			}
		}

		public virtual int Channel
		{
			get
			{
				return channel;
			}
			set
			{
				this.channel = value;
			}
		}


		public override string ToString()
		{
			return string.Format("PesHeader(channel={0:D}, pts={1:D}, dts={2:D})", Channel, Pts, Dts);
		}
	}

}