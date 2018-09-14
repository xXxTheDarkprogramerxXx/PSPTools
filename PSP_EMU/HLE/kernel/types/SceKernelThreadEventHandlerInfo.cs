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
namespace pspsharp.HLE.kernel.types
{
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;

	public class SceKernelThreadEventHandlerInfo : pspAbstractMemoryMappedStructureVariableLength
	{
		public readonly string name;
		public readonly int thid;
		public int mask;
		public int handler;
		public readonly int common;
		private bool active;

		// Internal info.
		public readonly int uid;

		private const string uidPurpose = "ThreadMan-ThreadEventHandler";

		// Thread Event IDs.
		public const int THREAD_EVENT_ID_ALL = unchecked((int)0xFFFFFFFF);
		public const int THREAD_EVENT_ID_KERN = unchecked((int)0xFFFFFFF8);
		public const int THREAD_EVENT_ID_USER = unchecked((int)0xFFFFFFF0);
		public const int THREAD_EVENT_ID_CURRENT = 0x0;
		// Thread Events.
		public const int THREAD_EVENT_CREATE = 0x1;
		public const int THREAD_EVENT_START = 0x2;
		public const int THREAD_EVENT_EXIT = 0x4;
		public const int THREAD_EVENT_DELETE = 0x8;
		public const int THREAD_EVENT_ALL = 0xF;

		public SceKernelThreadEventHandlerInfo(string name, int thid, int mask, int handler, int common)
		{
			this.name = name;
			this.thid = thid;
			this.mask = mask;
			this.handler = handler;
			this.common = common;
			Active = true;

			uid = SceUidManager.getNewUid(uidPurpose);
		}

		public virtual void release()
		{
			SceUidManager.releaseUid(uid, uidPurpose);
			mask = 0;
			handler = 0;
			Active = false;
		}

		public virtual bool hasEventMask(int @event)
		{
			return (mask & @event) == @event;
		}

		public virtual void triggerThreadEventHandler(SceKernelThreadInfo contextThread, int @event)
		{
			// Execute the handler and preserve the complete CpuState (i.e. restore all the CPU register after the execution of the handler)
			Modules.ThreadManForUserModule.executeCallback(contextThread, handler, new AfterEventHandler(this), false, true, @event, thid, common);
		}

		public virtual bool appliesFor(SceKernelThreadInfo currentThread, SceKernelThreadInfo thread, int @event)
		{
			if ((mask & @event) == 0)
			{
				return false;
			}
			if (!Active)
			{
				return false;
			}
			if (!currentThread.UserMode && thread.UserMode)
			{
				return false;
			}
			if (thid == THREAD_EVENT_ID_ALL || thid == THREAD_EVENT_ID_KERN || thid == thread.uid)
			{
				return true;
			}
			if (thid == THREAD_EVENT_ID_USER && thread.UserMode && currentThread.UserMode)
			{
				return true;
			}

			return false;
		}

		public virtual bool Active
		{
			get
			{
				return active;
			}
			set
			{
				this.active = value;
			}
		}


		protected internal override void write()
		{
			base.write();
			writeStringNZ(32, name);
			write32(thid);
			write32(mask);
			write32(handler);
			write32(common);
		}

		private class AfterEventHandler : IAction
		{
			private readonly SceKernelThreadEventHandlerInfo outerInstance;

			public AfterEventHandler(SceKernelThreadEventHandlerInfo outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				int result = Emulator.Processor.cpu._v0;

				// The event handler is deleted when it returns a value != 0
				if (result != 0)
				{
					outerInstance.Active = false;
				}

				if (Modules.log.InfoEnabled)
				{
					Modules.log.info(string.Format("Thread Event Handler exit detected (thid=0x{0:X}, result=0x{1:X8})", outerInstance.thid, result));
				}
			}
		}
	}
}