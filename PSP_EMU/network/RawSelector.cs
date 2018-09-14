using System.Collections.Generic;

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

	public class RawSelector : Selector
	{
		internal ISet<RawSelectionKey> rawSelectionKeys;
		internal ISet<SelectionKey> rawSelectedKeys;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static RawSelector open() throws java.io.IOException
		public static RawSelector open()
		{
			return new RawSelector();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected RawSelector() throws java.io.IOException
		protected internal RawSelector()
		{
			rawSelectionKeys = new HashSet<RawSelectionKey>();
			rawSelectedKeys = new HashSet<SelectionKey>();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void close()
		{
			foreach (SelectionKey rawSelectionKey in rawSelectionKeys)
			{
				((RawSelectionKey) rawSelectionKey).RawChannel.onSelectorClosed();
			}
			rawSelectionKeys.Clear();
			rawSelectedKeys.Clear();
		}

		public override bool Open
		{
			get
			{
				return true;
			}
		}

		public override ISet<SelectionKey> keys()
		{
			ISet<SelectionKey> setKeys = new HashSet<SelectionKey>();
			setKeys.addAll(rawSelectionKeys);
			return setKeys;
		}

		public override SelectorProvider provider()
		{
			return null;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int select() throws java.io.IOException
		public override int select()
		{
			return selectNow();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int select(long timeout) throws java.io.IOException
		public override int select(long timeout)
		{
			return selectNow();
		}

		protected internal virtual void selectNow(RawSelectionKey rawSelectionKey)
		{
			SelectableRawSocket rawSocket = rawSelectionKey.RawChannel.socket();
			rawSelectionKey.clearReadyOps();

			if ((rawSelectionKey.interestOps() & SelectionKey.OP_READ) != 0)
			{
				if (rawSocket.SelectedForRead)
				{
					rawSelectionKey.addReadyOp(SelectionKey.OP_READ);
				}
			}

			if ((rawSelectionKey.interestOps() & SelectionKey.OP_WRITE) != 0)
			{
				if (rawSocket.SelectedForWrite)
				{
					rawSelectionKey.addReadyOp(SelectionKey.OP_WRITE);
				}
			}

			if (rawSelectionKey.readyOps() != 0)
			{
				rawSelectedKeys.Add(rawSelectionKey);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int selectNow() throws java.io.IOException
		public override int selectNow()
		{
			rawSelectedKeys.Clear();

			foreach (RawSelectionKey rawSelectionKey in rawSelectionKeys)
			{
				selectNow(rawSelectionKey);
			}

			return rawSelectedKeys.Count;
		}

		public override ISet<SelectionKey> selectedKeys()
		{
			return rawSelectedKeys;
		}

		public override Selector wakeup()
		{
			return this;
		}

		public virtual void register(RawSelectionKey rawSelectionKey)
		{
			rawSelectionKeys.Add(rawSelectionKey);
		}
	}

}