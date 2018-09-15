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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_MESSAGEBOX_NO_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_NOT_FOUND_MESSAGE_BOX;
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
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_MBX;


	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using SceKernelMbxInfo = pspsharp.HLE.kernel.types.SceKernelMbxInfo;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;

	//using Logger = org.apache.log4j.Logger;

	public class MbxManager
	{
		protected internal static Logger log = Modules.getLogger("ThreadManForUser");

		private Dictionary<int, SceKernelMbxInfo> mbxMap;
		private MbxWaitStateChecker mbxWaitStateChecker;

		public const int PSP_MBX_ATTR_FIFO = 0;
		public const int PSP_MBX_ATTR_PRIORITY = 0x100;
		private const int PSP_MBX_ATTR_MSG_FIFO = 0; // Add new messages by FIFO.
		private const int PSP_MBX_ATTR_MSG_PRIORITY = 0x400; // Add new messages by MsgPacket priority.

		public virtual void reset()
		{
			mbxMap = new Dictionary<int, SceKernelMbxInfo>();
			mbxWaitStateChecker = new MbxWaitStateChecker(this);
		}

		private bool removeWaitingThread(SceKernelThreadInfo thread)
		{
			SceKernelMbxInfo info = mbxMap[thread.wait.Mbx_id];
			if (info == null)
			{
				return false;
			}

			info.threadWaitingList.removeWaitingThread(thread);

			return true;
		}

		public virtual void onThreadWaitTimeout(SceKernelThreadInfo thread)
		{
			if (removeWaitingThread(thread))
			{
				thread.cpuContext._v0 = ERROR_KERNEL_WAIT_TIMEOUT;
			}
			else
			{
				Console.WriteLine("Mbx deleted while we were waiting for it! (timeout expired)");
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
			if (thread.isWaitingForType(PSP_WAIT_MBX))
			{
				removeWaitingThread(thread);
			}
		}

		private void onMbxDeletedCancelled(int mbxid, int result)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			bool reschedule = false;

			for (IEnumerator<SceKernelThreadInfo> it = threadMan.GetEnumerator(); it.MoveNext();)
			{
				SceKernelThreadInfo thread = it.Current;
				if (thread.isWaitingFor(PSP_WAIT_MBX, mbxid))
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

		private void onMbxDeleted(int mbxid)
		{
			onMbxDeletedCancelled(mbxid, ERROR_KERNEL_WAIT_DELETE);
		}

		private void onMbxCancelled(int mbxid)
		{
			onMbxDeletedCancelled(mbxid, ERROR_KERNEL_WAIT_CANCELLED);
		}

		public virtual int checkMbxID(int uid)
		{
			if (!mbxMap.ContainsKey(uid))
			{
				Console.WriteLine(string.Format("checkMbxID unknown uid=0x{0:X}", uid));
				throw new SceKernelErrorException(ERROR_KERNEL_NOT_FOUND_MESSAGE_BOX);
			}

			return uid;
		}

		public virtual int sceKernelCreateMbx(string name, int attr, TPointer option)
		{
			if (option.NotNull)
			{
				int optionSize = option.getValue32();
				Console.WriteLine(string.Format("sceKernelCreateMbx option at {0}: size={1:D}", option, optionSize));
			}

			SceKernelMbxInfo info = new SceKernelMbxInfo(name, attr);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelCreateMbx returning {0}", info));
			}
			mbxMap[info.uid] = info;

			return info.uid;
		}

		public virtual int sceKernelDeleteMbx(int uid)
		{
			mbxMap.Remove(uid);
			onMbxDeleted(uid);

			return 0;
		}

		public virtual int sceKernelSendMbx(int uid, TPointer msgAddr)
		{
			SceKernelMbxInfo info = mbxMap[uid];

			bool msgConsumed = false;

			// If the Mbx is empty, check if some thread is already waiting.
			// If a thread is already waiting, do not update the msg "nextMsgPacketAddr" field.
			if (!info.hasMessage())
			{
				SceKernelThreadInfo thread = info.threadWaitingList.FirstWaitingThread;
				if (thread != null)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("sceKernelSendMbx waking thread {0}", thread));
					}
					thread.wait.Mbx_resultAddr.setValue(msgAddr.Address);
					info.threadWaitingList.removeWaitingThread(thread);
					thread.cpuContext._v0 = 0;

					ThreadManForUser threadMan = Modules.ThreadManForUserModule;
					threadMan.hleChangeThreadState(thread, PSP_THREAD_READY);
					threadMan.hleRescheduleCurrentThread();

					msgConsumed = true;
				}
			}

			// Add the message if it has not yet been consumed by a waiting thread
			if (!msgConsumed)
			{
				if ((info.attr & PSP_MBX_ATTR_MSG_PRIORITY) == PSP_MBX_ATTR_MSG_FIFO)
				{
					info.addMsg(msgAddr.Memory, msgAddr.Address);
				}
				else if ((info.attr & PSP_MBX_ATTR_MSG_PRIORITY) == PSP_MBX_ATTR_MSG_PRIORITY)
				{
					info.addMsgByPriority(msgAddr.Memory, msgAddr.Address);
				}
			}

			return 0;
		}

		private int hleKernelReceiveMbx(int uid, TPointer32 addrMsgAddr, TPointer32 timeoutAddr, bool doCallbacks, bool poll)
		{
			SceKernelMbxInfo info = mbxMap[uid];
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			if (!info.hasMessage())
			{
				if (!poll)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("hleKernelReceiveMbx - {0} (waiting)", info));
					}
					SceKernelThreadInfo currentThread = threadMan.CurrentThread;
					info.threadWaitingList.addWaitingThread(currentThread);
					currentThread.wait.Mbx_id = uid;
					currentThread.wait.Mbx_resultAddr = addrMsgAddr;
					threadMan.hleKernelThreadEnterWaitState(PSP_WAIT_MBX, uid, mbxWaitStateChecker, timeoutAddr.Address, doCallbacks);
				}
				else
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine("hleKernelReceiveMbx has no messages.");
					}
					return ERROR_KERNEL_MESSAGEBOX_NO_MESSAGE;
				}
			}
			else
			{
				// Success, do not reschedule the current thread.
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleKernelReceiveMbx - {0} fast check succeeded", info));
				}
				int msgAddr = info.removeMsg(Memory.Instance);
				addrMsgAddr.setValue(msgAddr);
			}

			return 0;
		}

		public virtual int sceKernelReceiveMbx(int uid, TPointer32 addrMsgAddr, TPointer32 timeoutAddr)
		{
			return hleKernelReceiveMbx(uid, addrMsgAddr, timeoutAddr, false, false);
		}

		public virtual int sceKernelReceiveMbxCB(int uid, TPointer32 addrMsgAddr, TPointer32 timeoutAddr)
		{
			return hleKernelReceiveMbx(uid, addrMsgAddr, timeoutAddr, true, false);
		}

		public virtual int sceKernelPollMbx(int uid, TPointer32 addrMsgAddr)
		{
			return hleKernelReceiveMbx(uid, addrMsgAddr, TPointer32.NULL, false, true);
		}

		public virtual int sceKernelCancelReceiveMbx(int uid, TPointer32 pnumAddr)
		{
			SceKernelMbxInfo info = mbxMap[uid];
			pnumAddr.setValue(info.NumWaitThreads);
			info.threadWaitingList.removeAllWaitingThreads();
			onMbxCancelled(uid);

			return 0;
		}

		public virtual int sceKernelReferMbxStatus(int uid, TPointer infoAddr)
		{
			SceKernelMbxInfo info = mbxMap[uid];
			info.write(infoAddr);

			return 0;
		}

		private class MbxWaitStateChecker : IWaitStateChecker
		{
			private readonly MbxManager outerInstance;

			public MbxWaitStateChecker(MbxManager outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				// Check if the thread has to continue its wait state or if the mbx
				// has received a new message during the callback execution.
				SceKernelMbxInfo info = outerInstance.mbxMap[wait.Mbx_id];
				if (info == null)
				{
					thread.cpuContext._v0 = ERROR_KERNEL_NOT_FOUND_MESSAGE_BOX;
					return false;
				}

				// Check the mbx for a new message.
				if (info.hasMessage())
				{
					Memory mem = Memory.Instance;
					int msgAddr = info.removeMsg(mem);
					wait.Mbx_resultAddr.setValue(msgAddr);
					info.threadWaitingList.removeWaitingThread(thread);
					thread.cpuContext._v0 = 0;
					return false;
				}

				return true;
			}
		}
		public static readonly MbxManager singleton = new MbxManager();

		private MbxManager()
		{
		}
	}
}