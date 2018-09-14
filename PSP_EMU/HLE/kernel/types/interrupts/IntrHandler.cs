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
namespace pspsharp.HLE.kernel.types.interrupts
{

	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;

	public class IntrHandler : AbstractInterruptHandler
	{
		private List<SubIntrHandler> subInterrupts = new List<SubIntrHandler>();
		private int minIndex = int.MaxValue;
		private int maxIndex = int.MinValue;
		private bool enabled;

		public IntrHandler()
		{
			enabled = true;
		}

		public IntrHandler(bool enabled)
		{
			this.enabled = enabled;
		}

		public virtual void addSubIntrHandler(int id, SubIntrHandler subIntrHandler)
		{
			if (id >= subInterrupts.Count)
			{
				subInterrupts.Capacity = id + 1;
			}

			if (id < minIndex)
			{
				minIndex = id;
			}
			if (id > maxIndex)
			{
				maxIndex = id;
			}

			subInterrupts[id] = subIntrHandler;
		}

		public virtual bool removeSubIntrHandler(int id)
		{
			if (id < 0 || id >= subInterrupts.Count)
			{
				return false;
			}

			bool removed = (subInterrupts[id] != null);
			subInterrupts[id] = null;

			// Find the first non-null sub-interrupt
			minIndex = int.MaxValue;
			for (int i = 0; i < subInterrupts.Count; i++)
			{
				if (subInterrupts[i] != null)
				{
					minIndex = i;
					break;
				}
			}

			// Find the last non-null sub-interrupt
			maxIndex = int.MinValue;
			for (int i = subInterrupts.Count - 1; i >= minIndex; i--)
			{
				if (subInterrupts[i] != null)
				{
					maxIndex = i;
					break;
				}
			}

			return removed;
		}

		public virtual SubIntrHandler getSubIntrHandler(int id)
		{
			if (id < 0 || id >= subInterrupts.Count)
			{
				return null;
			}

			return subInterrupts[id];
		}

		public virtual bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				this.enabled = value;
			}
		}


		public override void execute()
		{
			if (Enabled)
			{
				base.execute();
			}
		}

		protected internal override void executeInterrupt()
		{
			if (Enabled)
			{
				for (int id = minIndex; id <= maxIndex; id++)
				{
					SubIntrHandler subIntrHandler = getSubIntrHandler(id);
					if (subIntrHandler != null && subIntrHandler.Enabled)
					{
						IntrManager.Instance.pushAllegrexInterruptHandler(subIntrHandler);
					}
				}
			}
		}
	}

}