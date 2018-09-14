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
namespace pspsharp.network
{

	public class RawSelectionKey : SelectionKey
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		internal RawChannel channel_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		internal RawSelector selector_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		internal int interestOps_Renamed;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		internal int readyOps_Renamed;

		public RawSelectionKey(RawChannel channel, RawSelector selector)
		{
			this.channel_Renamed = channel;
			this.selector_Renamed = selector;
		}

		public override void cancel()
		{
		}

		public override SelectableChannel channel()
		{
			return channel_Renamed;
		}

		public virtual RawChannel RawChannel
		{
			get
			{
				return channel_Renamed;
			}
		}

		public override int interestOps()
		{
			return interestOps_Renamed;
		}

		public override SelectionKey interestOps(int ops)
		{
			interestOps_Renamed = ops;
			return this;
		}

		public override bool Valid
		{
			get
			{
				return true;
			}
		}

		public override int readyOps()
		{
			return readyOps_Renamed;
		}

		public override Selector selector()
		{
			return selector_Renamed;
		}

		public virtual void addReadyOp(int op)
		{
			readyOps_Renamed |= op;
		}

		public virtual void clearReadyOps()
		{
			readyOps_Renamed = 0;
		}
	}

}