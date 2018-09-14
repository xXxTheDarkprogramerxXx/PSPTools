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

	public class AfterSubIntrAction : IAction
	{
		private IntrManager intrManager;
		private IEnumerator<AbstractAllegrexInterruptHandler> allegrexInterruptHandlersIterator;
		private InterruptState interruptState;

		public AfterSubIntrAction(IntrManager intrManager, InterruptState interruptState, IEnumerator<AbstractAllegrexInterruptHandler> allegrexInterruptHandlersIterator)
		{
			this.intrManager = intrManager;
			this.interruptState = interruptState;
			this.allegrexInterruptHandlersIterator = allegrexInterruptHandlersIterator;
		}

		public virtual void execute()
		{
			IAction afterHandlerAction = interruptState.AfterHandlerAction;
			if (afterHandlerAction != null)
			{
				afterHandlerAction.execute();
			}

			intrManager.continueCallAllegrexInterruptHandler(interruptState, allegrexInterruptHandlersIterator, this);
		}
	}

}