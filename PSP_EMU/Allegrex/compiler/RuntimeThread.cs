using System.Threading;

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
namespace pspsharp.Allegrex.compiler
{

	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class RuntimeThread : Thread
	{
		private Semaphore semaphore = new Semaphore(1);
		private SceKernelThreadInfo threadInfo;
		private bool isInSyscall;
		private int stackSize;
		private const int maxStackSize = 1000;

		public RuntimeThread(SceKernelThreadInfo threadInfo)
		{
			this.threadInfo = threadInfo;
			threadInfo.javaThreadId = Id;
			isInSyscall = false;
			if (RuntimeContext.log.DebugEnabled)
			{
				Name = string.Format("{0}_0x{1:X}", threadInfo.name, threadInfo.uid);
			}
			else
			{
				Name = threadInfo.name;
			}
			suspendRuntimeExecution();
		}

		public override void run()
		{
			RuntimeContext.runThread(this);
			InSyscall = true;

			ThreadMXBean threadMXBean = ManagementFactory.ThreadMXBean;
			if (threadMXBean.ThreadCpuTimeEnabled)
			{
				threadInfo.javaThreadCpuTimeNanos = threadMXBean.CurrentThreadCpuTime;
			}
		}

		public virtual void suspendRuntimeExecution()
		{
			bool acquired = false;

			while (!acquired)
			{
				try
				{
					semaphore.acquire();
					acquired = true;
				}
				catch (InterruptedException)
				{
				}
			}
		}

		public virtual void continueRuntimeExecution()
		{
			semaphore.release();
		}

		public virtual SceKernelThreadInfo ThreadInfo
		{
			get
			{
				return threadInfo;
			}
		}

		public virtual bool InSyscall
		{
			get
			{
				return isInSyscall;
			}
			set
			{
				this.isInSyscall = value;
			}
		}


		public virtual void increaseStackSize()
		{
			stackSize++;
		}

		public virtual void decreaseStackSize()
		{
			stackSize--;
		}

		public virtual bool StackMaxSize
		{
			get
			{
				return stackSize > maxStackSize;
			}
		}

		public virtual int StackSize
		{
			get
			{
				return stackSize;
			}
		}

		public virtual void onThreadStart()
		{
			if (threadInfo != null)
			{
				threadInfo.onThreadStart();
			}
		}
	}

}