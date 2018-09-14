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
namespace pspsharp.scheduler
{
	using Modules = pspsharp.HLE.Modules;
	using IAction = pspsharp.HLE.kernel.types.IAction;

	public class DelayThreadAction : IAction
	{
		private int threadId;
		private int micros;
		private bool doCallbacks;
		private bool unblockThread;

		public DelayThreadAction(int threadId, int micros, bool doCallbacks, bool unblockThread)
		{
			this.threadId = threadId;
			this.micros = micros;
			this.doCallbacks = doCallbacks;
			this.unblockThread = unblockThread;
		}

		public virtual void execute()
		{
			if (unblockThread)
			{
				Modules.ThreadManForUserModule.hleUnblockThread(threadId);
			}
			Modules.ThreadManForUserModule.hleKernelDelayThread(threadId, micros, doCallbacks);
		}
	}

}