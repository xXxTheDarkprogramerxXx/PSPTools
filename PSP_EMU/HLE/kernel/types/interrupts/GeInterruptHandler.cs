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

	public class GeInterruptHandler : AbstractInterruptHandler
	{
		private GeCallbackInterruptHandler geCallbackInterruptHandler;
		private AfterGeCallbackAction afterGeCallbackAction;

		public GeInterruptHandler(GeCallbackInterruptHandler geCallbackInterruptHandler, int listId, int behavior, int id)
		{
			this.geCallbackInterruptHandler = geCallbackInterruptHandler;

			// Argument $a0 of GE callback is the signal/finish ID
			geCallbackInterruptHandler.Id = id;

			if (listId >= 0)
			{
				afterGeCallbackAction = new AfterGeCallbackAction(listId, behavior);
			}
			else
			{
				afterGeCallbackAction = null;
			}
		}

		protected internal override void executeInterrupt()
		{
			// Trigger GE interrupt: execute GeCallback
			IntrManager.Instance.triggerInterrupt(IntrManager.PSP_GE_INTR, afterGeCallbackAction, null, geCallbackInterruptHandler);
		}
	}

}