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
namespace pspsharp.memory.mmio.dmac
{

	using IAction = pspsharp.HLE.kernel.types.IAction;
	using IState = pspsharp.state.IState;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	public class DmacProcessor : IState
	{
		private const int STATE_VERSION = 0;
		// Dmac STATUS flags:
		public const int DMAC_STATUS_IN_PROGRESS = 0x00000001;
		public const int DMAC_STATUS_REQUIRES_DDR = 0x00000100;
		// Dmac ATTRIBUTES flags:
		public const int DMAC_ATTRIBUTES_LENGTH = 0x00000FFF;
		//                                                               0x00007000
		public const int DMAC_ATTRIBUTES_SRC_STEP_SHIFT = 12;
		//                                                               0x00038000
		public const int DMAC_ATTRIBUTES_DST_STEP_SHIFT = 15;
		//                                                               0x001C0000
		public const int DMAC_ATTRIBUTES_SRC_LENGTH_SHIFT_SHIFT = 18;
		//                                                               0x00E00000
		public const int DMAC_ATTRIBUTES_DST_LENGTH_SHIFT_SHIFT = 21;
		public const int DMAC_ATTRIBUTES_UNKNOWN = 0x02000000;
		public const int DMAC_ATTRIBUTES_SRC_INCREMENT = 0x04000000;
		public const int DMAC_ATTRIBUTES_DST_INCREMENT = 0x08000000;
		public const int DMAC_ATTRIBUTES_TRIGGER_INTERRUPT = unchecked((int)0x80000000);
		private Memory memSrc;
		private Memory memDst;
		private IAction interruptAction;
		private IAction completedAction;
		private DmacThread dmacThread;
		private int dst;
		private int src;
		private int next;
		private int attributes;
		private int status;

		private class CompletedAction : IAction
		{
			private readonly DmacProcessor outerInstance;

			public CompletedAction(DmacProcessor outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				// Clear the status "in progress"
				outerInstance.status &= ~DMAC_STATUS_IN_PROGRESS;
			}
		}

		public DmacProcessor(Memory memSrc, Memory memDst, int baseAddress, IAction interruptAction)
		{
			this.memSrc = memSrc;
			this.memDst = memDst;
			this.interruptAction = interruptAction;
			this.completedAction = new CompletedAction(this);

			dmacThread = new DmacThread(this);
			dmacThread.Name = string.Format("Dmac Thread for 0x{0:X8}", baseAddress);
			dmacThread.Daemon = true;
			dmacThread.Start();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			dst = stream.readInt();
			src = stream.readInt();
			next = stream.readInt();
			attributes = stream.readInt();
			status = stream.readInt();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeInt(dst);
			stream.writeInt(src);
			stream.writeInt(next);
			stream.writeInt(attributes);
			stream.writeInt(status);
		}

		public virtual int Dst
		{
			get
			{
				return dst;
			}
			set
			{
				this.dst = value;
			}
		}


		public virtual int Src
		{
			get
			{
				return src;
			}
			set
			{
				this.src = value;
			}
		}


		public virtual int Next
		{
			get
			{
				return next;
			}
			set
			{
				this.next = value;
			}
		}


		public virtual int Attributes
		{
			get
			{
				return attributes;
			}
			set
			{
				this.attributes = value;
			}
		}


		public virtual int Status
		{
			get
			{
				return status;
			}
			set
			{
				int previousStatus = this.status;
				this.status = value;
    
				// Status "in progress" changed?
				if ((previousStatus & DMAC_STATUS_IN_PROGRESS) != (value & DMAC_STATUS_IN_PROGRESS))
				{
					if ((value & DMAC_STATUS_IN_PROGRESS) != 0)
					{
						// Starting...
						dmacThread.execute(memDst, memSrc, dst, src, next, attributes, value, interruptAction, completedAction);
					}
					else
					{
						// Stopping...
						dmacThread.abortJob();
					}
				}
			}
		}


		public virtual void write32(int offset, int value)
		{
			switch (offset)
			{
				case 0x00:
					Src = value;
					break;
				case 0x04:
					Dst = value;
					break;
				case 0x08:
					Next = value;
					break;
				case 0x0C:
					Attributes = value;
					break;
				case 0x10:
					Status = value;
					break;
			}
		}

		public virtual int read32(int offset)
		{
			switch (offset)
			{
				case 0x00:
					return Src;
				case 0x04:
					return Dst;
				case 0x08:
					return Next;
				case 0x0C:
					return Attributes;
				case 0x10:
					return Status;
			}

			return 0;
		}
	}

}