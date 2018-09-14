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
	/// ThreadWaitingList where the list is ordered by the thread priority.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class ThreadWaitingListPriority : ThreadWaitingList
	{
		public ThreadWaitingListPriority(int waitType, int waitId) : base(waitType, waitId)
		{
		}

		public override void addWaitingThread(SceKernelThreadInfo thread)
		{
			bool added = false;

			if (waitingThreads.Count > 0)
			{
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
				for (IEnumerator<int> lit = waitingThreads.GetEnumerator(); lit.MoveNext();)
				{
					int uid = lit.Current.intValue();
					SceKernelThreadInfo waitingThread = Modules.ThreadManForUserModule.getThreadById(uid);
					if (waitingThread != null)
					{
						if (thread.currentPriority < waitingThread.currentPriority)
						{
							lit.previous();
							lit.add(thread.uid);
							added = true;
							break;
						}
					}
				}
			}

			if (!added)
			{
				waitingThreads.Add(thread.uid);
			}
		}
	}

}