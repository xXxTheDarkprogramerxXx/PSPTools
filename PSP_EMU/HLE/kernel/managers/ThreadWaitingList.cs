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
namespace pspsharp.HLE.kernel.managers
{

	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;

	/// <summary>
	/// Base implementation of a list of waiting threads.
	/// Two implementations are provided to implement a FIFO list
	/// and a list ordered by the thread priority.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public abstract class ThreadWaitingList
	{
		protected internal IList<int> waitingThreads = new LinkedList<int>();
		protected internal int waitType;
		protected internal int waitId;

		public static ThreadWaitingList createThreadWaitingList(int waitType, int waitId, int attr, int attrPrioriyFlag)
		{
			if ((attr & attrPrioriyFlag) == attrPrioriyFlag)
			{
				return new ThreadWaitingListPriority(waitType, waitId);
			}
			return new ThreadWaitingListFIFO(waitType, waitId);
		}

		protected internal ThreadWaitingList(int waitType, int waitId)
		{
			this.waitType = waitType;
			this.waitId = waitId;
		}

		public virtual int NumWaitingThreads
		{
			get
			{
				return waitingThreads.Count;
			}
		}

		public abstract void addWaitingThread(SceKernelThreadInfo thread);

		public virtual void removeWaitingThread(SceKernelThreadInfo thread)
		{
			waitingThreads.RemoveAt(new int?(thread.uid));
		}

		public virtual SceKernelThreadInfo getNextWaitingThread(SceKernelThreadInfo baseThread)
		{
			if (baseThread == null)
			{
				return FirstWaitingThread;
			}

			int index = waitingThreads.IndexOf(baseThread.uid);
			if (index < 0 || (index + 1) >= NumWaitingThreads)
			{
				return null;
			}

			int uid = waitingThreads[index + 1];
			SceKernelThreadInfo thread = Modules.ThreadManForUserModule.getThreadById(uid);

			// Is the thread still existing
			if (thread == null)
			{
				// Thread is no longer existing, delete it from the waiting list and retry
				waitingThreads.RemoveAt(index + 1);
				return getNextWaitingThread(baseThread);
			}

			// Is the thread still waiting on this ID?
			if (!thread.isWaitingForType(waitType) || thread.waitId != waitId)
			{
				// The thread is no longer waiting on this object, remove it from the waiting list and retry
				waitingThreads.RemoveAt(index + 1);
				return getNextWaitingThread(baseThread);
			}

			return thread;
		}

		public virtual SceKernelThreadInfo FirstWaitingThread
		{
			get
			{
				// Is the waiting list empty?
				if (NumWaitingThreads <= 0)
				{
					return null;
				}
    
				int uid = waitingThreads[0];
				SceKernelThreadInfo thread = Modules.ThreadManForUserModule.getThreadById(uid);
    
				// Is the thread still existing
				if (thread == null)
				{
					// Thread is no longer existing, delete it from the waiting list and retry
					waitingThreads.RemoveAt(0);
					return FirstWaitingThread;
				}
    
				// Is the thread still waiting on this ID?
				if (!thread.isWaitingForType(waitType) || thread.waitId != waitId)
				{
					// The thread is no longer waiting on this object, remove it from the waiting list and retry
					waitingThreads.RemoveAt(0);
					return FirstWaitingThread;
				}
    
				return thread;
			}
		}

		public virtual void removeAllWaitingThreads()
		{
			waitingThreads.Clear();
		}
	}

}