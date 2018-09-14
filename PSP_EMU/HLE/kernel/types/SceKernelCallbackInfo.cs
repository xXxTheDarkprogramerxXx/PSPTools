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

	public class SceKernelCallbackInfo : pspBaseCallback
	{
		private readonly string name;
		private readonly int threadId;
		private readonly int callbackArgument;
		private int notifyCount;
		private int notifyArg;

		private class SceKernelCallbackStatus : pspAbstractMemoryMappedStructureVariableLength
		{
			private readonly SceKernelCallbackInfo outerInstance;

			public SceKernelCallbackStatus(SceKernelCallbackInfo outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override void write()
			{
				base.write();
				writeStringNZ(32, outerInstance.name);
				write32(outerInstance.threadId);
				write32(outerInstance.CallbackFunction);
				write32(outerInstance.callbackArgument);
				write32(outerInstance.notifyCount);
				write32(outerInstance.notifyArg);
			}
		}

		public SceKernelCallbackInfo(string name, int threadId, int callback_addr, int callback_arg_addr) : base(callback_addr, 3)
		{
			this.name = name;
			this.threadId = threadId;
			this.callbackArgument = callback_arg_addr;
			notifyCount = 0;
			notifyArg = 0;
		}

		public virtual void write(ITPointerBase statusAddr)
		{
			SceKernelCallbackStatus status = new SceKernelCallbackStatus(this);
			status.write(statusAddr);
		}

		/// <summary>
		/// Call this to switch in the callback, in a given thread context.
		/// </summary>
		public override void call(SceKernelThreadInfo thread, IAction afterAction)
		{
			setArgument(0, notifyCount);
			setArgument(1, notifyArg);
			setArgument(2, callbackArgument);

			// clear the counter and the arg
			notifyCount = 0;
			notifyArg = 0;

			base.call(thread, afterAction);
		}

		public virtual int ThreadId
		{
			get
			{
				return threadId;
			}
		}

		public virtual int NotifyCount
		{
			get
			{
				return notifyCount;
			}
		}

		public virtual int NotifyArg
		{
			get
			{
				return notifyArg;
			}
			set
			{
				notifyCount++; // keep increasing this until we actually enter the callback
				this.notifyArg = value;
			}
		}

		public virtual void cancel()
		{
			notifyArg = 0;
			notifyCount = 0;
		}

		public virtual int CallbackArgument
		{
			get
			{
				return callbackArgument;
			}
		}


		public override string ToString()
		{
			return string.Format("uid:0x{0:X}, name:'{1}', thread:'{2}', PC:0x{3:X8}, $a0:0x{4:X8}, $a1:0x{5:X8}, $a2:0x{6:X8}", Uid, name, Modules.ThreadManForUserModule.getThreadName(threadId), CallbackFunction, notifyCount, notifyArg, callbackArgument);
		}
	}

}