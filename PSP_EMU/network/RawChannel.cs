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

	public class RawChannel : SelectableChannel
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private SelectableRawSocket socket_Renamed;
		private bool blocking;
		internal Selector selector;
		internal RawSelectionKey selectionKey;

		public RawChannel()
		{
			socket_Renamed = new SelectableRawSocket();
		}

		public virtual SelectableRawSocket socket()
		{
			return socket_Renamed;
		}

		public override object blockingLock()
		{
			return socket_Renamed;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public java.nio.channels.SelectableChannel configureBlocking(boolean block) throws java.io.IOException
		public override SelectableChannel configureBlocking(bool block)
		{
			blocking = block;
			return this;
		}

		public override bool Blocking
		{
			get
			{
				return blocking;
			}
		}

		public override bool Registered
		{
			get
			{
				return selector != null;
			}
		}

		public override SelectionKey keyFor(Selector sel)
		{
			return selectionKey;
		}

		public override SelectorProvider provider()
		{
			return SelectorProvider.provider();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public java.nio.channels.SelectionKey register(java.nio.channels.Selector sel, int ops, Object att) throws java.nio.channels.ClosedChannelException
		public override SelectionKey register(Selector sel, int ops, object att)
		{
			if (sel is RawSelector)
			{
				RawSelector rawSelector = (RawSelector) sel;

				selector = sel;
				selectionKey = new RawSelectionKey(this, rawSelector);
				selectionKey.attach(att);
				selectionKey.interestOps(ops);

				rawSelector.register(selectionKey);
			}

			return selectionKey;
		}

		public override int validOps()
		{
			return SelectionKey.OP_READ | SelectionKey.OP_WRITE;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected void implCloseChannel() throws java.io.IOException
		protected internal override void implCloseChannel()
		{
			socket_Renamed.close();
		}

		public virtual void onSelectorClosed()
		{
			selector = null;
			selectionKey = null;
		}
	}

}