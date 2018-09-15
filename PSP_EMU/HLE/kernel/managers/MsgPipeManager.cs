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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_ILLEGAL_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_MESSAGE_PIPE_EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_MESSAGE_PIPE_FULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NOT_FOUND_MESSAGE_PIPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_CANCELLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_DELETE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_STATUS_RELEASED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_TIMEOUT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_THREAD_READY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_MSGPIPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_High;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.PSP_SMEM_Low;


	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelMppInfo = pspsharp.HLE.kernel.types.SceKernelMppInfo;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;

	//using Logger = org.apache.log4j.Logger;

	public class MsgPipeManager
	{
		protected internal static Logger log = Modules.getLogger("ThreadManForUser");

		private Dictionary<int, SceKernelMppInfo> msgMap;
		private MsgPipeSendWaitStateChecker msgPipeSendWaitStateChecker;
		private MsgPipeReceiveWaitStateChecker msgPipeReceiveWaitStateChecker;

		public const int PSP_MPP_ATTR_SEND_FIFO = 0;
		public const int PSP_MPP_ATTR_SEND_PRIORITY = 0x100;
		public const int PSP_MPP_ATTR_RECEIVE_FIFO = 0;
		public const int PSP_MPP_ATTR_RECEIVE_PRIORITY = 0x1000;
		private const int PSP_MPP_ATTR_ADDR_HIGH = 0x4000;

		public const int PSP_MPP_WAIT_MODE_COMPLETE = 0; // receive always a complete buffer
		public const int PSP_MPP_WAIT_MODE_PARTIAL = 1; // can receive a partial buffer

		public virtual void reset()
		{
			msgMap = new Dictionary<int, SceKernelMppInfo>();
			msgPipeSendWaitStateChecker = new MsgPipeSendWaitStateChecker(this);
			msgPipeReceiveWaitStateChecker = new MsgPipeReceiveWaitStateChecker(this);
		}

		private bool removeWaitingThread(SceKernelThreadInfo thread)
		{
			SceKernelMppInfo info = msgMap[thread.wait.MsgPipe_id];
			if (info == null)
			{
				return false;
			}

			if (thread.wait.MsgPipe_isSend)
			{
				info.sendThreadWaitingList.removeWaitingThread(thread);
			}
			else
			{
				info.receiveThreadWaitingList.removeWaitingThread(thread);
			}

			return true;
		}

		public virtual void onThreadWaitTimeout(SceKernelThreadInfo thread)
		{
			// Untrack
			if (removeWaitingThread(thread))
			{
				// Return WAIT_TIMEOUT
				thread.cpuContext._v0 = ERROR_KERNEL_WAIT_TIMEOUT;
			}
			else
			{
				Console.WriteLine("MsgPipe deleted while we were waiting for it! (timeout expired)");
				// Return WAIT_DELETE
				thread.cpuContext._v0 = ERROR_KERNEL_WAIT_DELETE;
			}
		}

		public virtual void onThreadWaitReleased(SceKernelThreadInfo thread)
		{
			// Untrack
			if (removeWaitingThread(thread))
			{
				// Return ERROR_WAIT_STATUS_RELEASED
				thread.cpuContext._v0 = ERROR_KERNEL_WAIT_STATUS_RELEASED;
			}
			else
			{
				Console.WriteLine("EventFlag deleted while we were waiting for it!");
				// Return WAIT_DELETE
				thread.cpuContext._v0 = ERROR_KERNEL_WAIT_DELETE;
			}
		}

		public virtual void onThreadDeleted(SceKernelThreadInfo thread)
		{
			removeWaitingThread(thread);
		}

		private void onMsgPipeDeletedCancelled(int msgpid, int result)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			for (IEnumerator<SceKernelThreadInfo> it = threadMan.GetEnumerator(); it.MoveNext();)
			{
				SceKernelThreadInfo thread = it.Current;
				if (thread.isWaitingFor(PSP_WAIT_MSGPIPE, msgpid))
				{
					thread.cpuContext._v0 = result;
					threadMan.hleChangeThreadState(thread, PSP_THREAD_READY);
					reschedule = true;
				}
			}
			// Reschedule only if threads waked up.
			if (reschedule)
			{
				threadMan.hleRescheduleCurrentThread();
			}
		}

		private void onMsgPipeDeleted(int msgpid)
		{
			onMsgPipeDeletedCancelled(msgpid, ERROR_KERNEL_WAIT_DELETE);
		}

		private void onMsgPipeCancelled(int msgpid)
		{
			onMsgPipeDeletedCancelled(msgpid, ERROR_KERNEL_WAIT_CANCELLED);
		}

		private void onMsgPipeSendModified(SceKernelMppInfo info)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			SceKernelThreadInfo checkedThread = null;
			while (true)
			{
				SceKernelThreadInfo thread = info.sendThreadWaitingList.getNextWaitingThread(checkedThread);
				if (thread == null)
				{
					break;
				}
				if (thread.wait.MsgPipe_isSend && trySendMsgPipe(info, thread.wait.MsgPipe_address, thread.wait.MsgPipe_size, thread.wait.MsgPipe_waitMode, thread.wait.MsgPipe_resultSize_addr))
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("onMsgPipeSendModified waking thread {0}", thread));
					}
					info.sendThreadWaitingList.removeWaitingThread(thread);
					thread.cpuContext._v0 = 0;
					threadMan.hleChangeThreadState(thread, PSP_THREAD_READY);
					reschedule = true;
				}
				else
				{
					checkedThread = thread;
				}
			}

			// Reschedule only if threads waked up.
			if (reschedule)
			{
				threadMan.hleRescheduleCurrentThread();
			}
		}

		private void onMsgPipeReceiveModified(SceKernelMppInfo info)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			SceKernelThreadInfo checkedThread = null;
			while (true)
			{
				SceKernelThreadInfo thread = info.receiveThreadWaitingList.getNextWaitingThread(checkedThread);
				if (thread == null)
				{
					break;
				}
				if (!thread.wait.MsgPipe_isSend && tryReceiveMsgPipe(info, thread.wait.MsgPipe_address, thread.wait.MsgPipe_size, thread.wait.MsgPipe_waitMode, thread.wait.MsgPipe_resultSize_addr))
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("onMsgPipeReceiveModified waking thread {0}", thread));
					}
					info.receiveThreadWaitingList.removeWaitingThread(thread);
					thread.cpuContext._v0 = 0;
					threadMan.hleChangeThreadState(thread, PSP_THREAD_READY);
					reschedule = true;
				}
				else
				{
					checkedThread = thread;
				}
			}

			// Reschedule only if threads waked up.
			if (reschedule)
			{
				threadMan.hleRescheduleCurrentThread();
			}
		}

		private bool trySendMsgPipe(SceKernelMppInfo info, TPointer addr, int size, int waitMode, TPointer32 resultSizeAddr)
		{
			if (size > 0)
			{
				// When the bufSize is 0, the data is transfered directly
				// from the sender to the receiver without being buffered.
				if (info.bufSize == 0)
				{
					info.setUserData(addr.Address, size);
					onMsgPipeReceiveModified(info);
					if (info.UserSize > 0)
					{
						// wait if nothing has been sent or
						// if we have to wait to send everything
						if (size == info.UserSize || waitMode == PSP_MPP_WAIT_MODE_COMPLETE)
						{
							return false;
						}
					}
				}
				else
				{
					int availableSize = info.availableWriteSize();
					if (availableSize == 0)
					{
						return false;
					}
					// Trying to send more than available?
					if (size > availableSize)
					{
						// Do we need to send the complete size?
						if (waitMode == PSP_MPP_WAIT_MODE_COMPLETE)
						{
							return false;
						}
						// We can just send the available size.
						size = availableSize;
					}
					info.append(addr.Memory, addr.Address, size);
				}
			}
			resultSizeAddr.setValue(size);

			return true;
		}

		private bool tryReceiveMsgPipe(SceKernelMppInfo info, TPointer addr, int size, int waitMode, TPointer32 resultSizeAddr)
		{
			if (size > 0)
			{
				int availableSize = info.availableReadSize();
				if (availableSize == 0)
				{
					return false;
				}
				// Trying to receive more than available?
				if (size > availableSize)
				{
					// Do we need to receive the complete size?
					if (waitMode == PSP_MPP_WAIT_MODE_COMPLETE)
					{
						return false;
					}
					// We can just receive the available size.
					size = availableSize;
				}
				info.consume(addr.Memory, addr.Address, size);

				if (info.bufSize == 0 && info.availableReadSize() == 0 && info.NumSendWaitThreads > 0)
				{
					SceKernelThreadInfo thread = info.sendThreadWaitingList.FirstWaitingThread;
					if (thread.wait.MsgPipe_isSend)
					{
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("tryReceiveMsgPipe waking thread {0}", thread));
						}
						ThreadManForUser threadMan = Modules.ThreadManForUserModule;
						info.sendThreadWaitingList.removeWaitingThread(thread);
						thread.cpuContext._v0 = 0;
						threadMan.hleChangeThreadState(thread, PSP_THREAD_READY);
						threadMan.hleRescheduleCurrentThread();
					}
				}
			}
			resultSizeAddr.setValue(size);

			return true;
		}

		public virtual int checkMsgPipeID(int uid)
		{
			if (!msgMap.ContainsKey(uid))
			{
				Console.WriteLine(string.Format("checkMsgPipeID unknown uid=0x{0:X}", uid));
				throw new SceKernelErrorException(ERROR_KERNEL_NOT_FOUND_MESSAGE_PIPE);
			}

			return uid;
		}

		public virtual SceKernelMppInfo getMsgPipeInfo(int uid)
		{
			return msgMap[uid];
		}

		public virtual int sceKernelCreateMsgPipe(string name, int partitionid, int attr, int size, TPointer option)
		{
			if (option.NotNull)
			{
				int optionSize = option.getValue32();
				Console.WriteLine(string.Format("sceKernelCreateMsgPipe option at {0}, size={1:D}", option, optionSize));
			}

			int memType = PSP_SMEM_Low;
			if ((attr & PSP_MPP_ATTR_ADDR_HIGH) == PSP_MPP_ATTR_ADDR_HIGH)
			{
				memType = PSP_SMEM_High;
			}

			SceKernelMppInfo info = new SceKernelMppInfo(name, partitionid, attr, size, memType);
			if (!info.MemoryAllocated)
			{
				Console.WriteLine(string.Format("sceKernelCreateMsgPipe name='{0}', partitionId={1:D}, attr=0x{2:X}, size=0x{3:X}, option={4} not enough memory", name, partitionid, attr, size, option));
				info.delete();
				return SceKernelErrors.ERROR_KERNEL_NO_MEMORY;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelCreateMsgPipe returning {0}", info));
			}
			msgMap[info.uid] = info;

			return info.uid;
		}

		public virtual int sceKernelDeleteMsgPipe(int uid)
		{
			SceKernelMppInfo info = msgMap.Remove(uid);
			info.delete();
			onMsgPipeDeleted(uid);

			return 0;
		}

		public virtual int hleKernelSendMsgPipe(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr, TPointer32 timeoutAddr, bool doCallbacks, bool poll)
		{
			SceKernelMppInfo info = msgMap[uid];
			if (info.bufSize != 0 && size > info.bufSize)
			{
				Console.WriteLine(string.Format("hleKernelSendMsgPipe illegal size 0x{0:X} max 0x{1:X}", size, info.bufSize));
				return ERROR_KERNEL_ILLEGAL_SIZE;
			}
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			if (!trySendMsgPipe(info, msgAddr, size, waitMode, resultSizeAddr))
			{
				if (!poll)
				{
					// Failed, but it's ok, just wait a little
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("hleKernelSendMsgPipe {0} waiting for 0x{1:X} bytes to become available", info, size));
					}
					SceKernelThreadInfo currentThread = threadMan.CurrentThread;
					info.sendThreadWaitingList.addWaitingThread(currentThread);
					// Wait on a specific MsgPipe.
					currentThread.wait.MsgPipe_isSend = true;
					currentThread.wait.MsgPipe_id = uid;
					currentThread.wait.MsgPipe_address = msgAddr;
					currentThread.wait.MsgPipe_size = size;
					currentThread.wait.MsgPipe_resultSize_addr = resultSizeAddr;
					threadMan.hleKernelThreadEnterWaitState(PSP_WAIT_MSGPIPE, uid, msgPipeSendWaitStateChecker, timeoutAddr.Address, doCallbacks);
				}
				else
				{
					Console.WriteLine(string.Format("hleKernelSendMsgPipe illegal size 0x{0:X}, max 0x{1:X} (pipe needs consuming)", size, info.freeSize));
					return ERROR_KERNEL_MESSAGE_PIPE_FULL;
				}
			}
			else
			{
				// Success, do not reschedule the current thread.
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleKernelSendMsgPipe {0} fast check succeeded", info));
				}
				onMsgPipeReceiveModified(info);
			}

			return 0;
		}

		public virtual int sceKernelSendMsgPipe(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr, TPointer32 timeoutAddr)
		{
			return hleKernelSendMsgPipe(uid, msgAddr, size, waitMode, resultSizeAddr, timeoutAddr, false, false);
		}

		public virtual int sceKernelSendMsgPipeCB(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr, TPointer32 timeoutAddr)
		{
			return hleKernelSendMsgPipe(uid, msgAddr, size, waitMode, resultSizeAddr, timeoutAddr, true, false);
		}

		public virtual int sceKernelTrySendMsgPipe(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr)
		{
			return hleKernelSendMsgPipe(uid, msgAddr, size, waitMode, resultSizeAddr, TPointer32.NULL, false, true);
		}

		private int hleKernelReceiveMsgPipe(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr, TPointer32 timeoutAddr, bool doCallbacks, bool poll)
		{
			SceKernelMppInfo info = msgMap[uid];
			if (info.bufSize != 0 && size > info.bufSize)
			{
				Console.WriteLine(string.Format("hleKernelReceiveMsgPipe illegal size 0x{0:X}, max 0x{1:X}", size, info.bufSize));
				return ERROR_KERNEL_ILLEGAL_SIZE;
			}

			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			if (!tryReceiveMsgPipe(info, msgAddr, size, waitMode, resultSizeAddr))
			{
				if (!poll)
				{
					// Failed, but it's ok, just wait a little
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("hleKernelReceiveMsgPipe {0} waiting for 0x{1:X} bytes to become available", info, size));
					}
					SceKernelThreadInfo currentThread = threadMan.CurrentThread;
					info.receiveThreadWaitingList.addWaitingThread(currentThread);
					// Wait on a specific MsgPipe.
					currentThread.wait.MsgPipe_isSend = false;
					currentThread.wait.MsgPipe_id = uid;
					currentThread.wait.MsgPipe_address = msgAddr;
					currentThread.wait.MsgPipe_size = size;
					currentThread.wait.MsgPipe_resultSize_addr = resultSizeAddr;
					threadMan.hleKernelThreadEnterWaitState(PSP_WAIT_MSGPIPE, uid, msgPipeReceiveWaitStateChecker, timeoutAddr.Address, doCallbacks);
				}
				else
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("hleKernelReceiveMsgPipe trying to read more than is available size 0x{0:X}, available 0x{1:X}", size, info.bufSize - info.freeSize));
					}
					return ERROR_KERNEL_MESSAGE_PIPE_EMPTY;
				}
			}
			else
			{
				// Success, do not reschedule the current thread.
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleKernelReceiveMsgPipe {0} fast check succeeded", info));
				}
				onMsgPipeSendModified(info);
			}

			return 0;
		}

		public virtual int sceKernelReceiveMsgPipe(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr, TPointer32 timeoutAddr)
		{
			return hleKernelReceiveMsgPipe(uid, msgAddr, size, waitMode, resultSizeAddr, timeoutAddr, false, false);
		}

		public virtual int sceKernelReceiveMsgPipeCB(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr, TPointer32 timeoutAddr)
		{
			return hleKernelReceiveMsgPipe(uid, msgAddr, size, waitMode, resultSizeAddr, timeoutAddr, true, false);
		}

		public virtual int sceKernelTryReceiveMsgPipe(int uid, TPointer msgAddr, int size, int waitMode, TPointer32 resultSizeAddr)
		{
			return hleKernelReceiveMsgPipe(uid, msgAddr, size, waitMode, resultSizeAddr, TPointer32.NULL, false, true);
		}

		public virtual int sceKernelCancelMsgPipe(int uid, TPointer32 sendAddr, TPointer32 recvAddr)
		{
			SceKernelMppInfo info = msgMap[uid];

			sendAddr.setValue(info.NumSendWaitThreads);
			recvAddr.setValue(info.NumReceiveWaitThreads);
			info.sendThreadWaitingList.removeAllWaitingThreads();
			info.receiveThreadWaitingList.removeAllWaitingThreads();
			onMsgPipeCancelled(uid);

			return 0;
		}

		public virtual int sceKernelReferMsgPipeStatus(int uid, TPointer infoAddr)
		{
			SceKernelMppInfo info = msgMap[uid];
			info.write(infoAddr);

			return 0;
		}

		private class MsgPipeSendWaitStateChecker : IWaitStateChecker
		{
			private readonly MsgPipeManager outerInstance;

			public MsgPipeSendWaitStateChecker(MsgPipeManager outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				// Check if the thread has to continue its wait state or if the msgpipe
				// has received a new message during the callback execution.
				SceKernelMppInfo info = outerInstance.msgMap[wait.MsgPipe_id];
				if (info == null)
				{
					thread.cpuContext._v0 = ERROR_KERNEL_NOT_FOUND_MESSAGE_PIPE;
					return false;
				}

				if (outerInstance.trySendMsgPipe(info, thread.wait.MsgPipe_address, thread.wait.MsgPipe_size, thread.wait.MsgPipe_waitMode, thread.wait.MsgPipe_resultSize_addr))
				{
					info.sendThreadWaitingList.removeWaitingThread(thread);
					thread.cpuContext._v0 = 0;
					return false;
				}

				return true;
			}
		}

		private class MsgPipeReceiveWaitStateChecker : IWaitStateChecker
		{
			private readonly MsgPipeManager outerInstance;

			public MsgPipeReceiveWaitStateChecker(MsgPipeManager outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				// Check if the thread has to continue its wait state or if the msgpipe
				// has been sent a new message during the callback execution.
				SceKernelMppInfo info = outerInstance.msgMap[wait.MsgPipe_id];
				if (info == null)
				{
					thread.cpuContext._v0 = ERROR_KERNEL_NOT_FOUND_MESSAGE_PIPE;
					return false;
				}

				if (outerInstance.tryReceiveMsgPipe(info, wait.MsgPipe_address, wait.MsgPipe_size, wait.MsgPipe_waitMode, wait.MsgPipe_resultSize_addr))
				{
					info.receiveThreadWaitingList.removeWaitingThread(thread);
					thread.cpuContext._v0 = 0;
					return false;
				}

				return true;
			}
		}
		public static readonly MsgPipeManager singleton = new MsgPipeManager();

		private MsgPipeManager()
		{
		}
	}
}