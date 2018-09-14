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
namespace pspsharp.HLE.modules
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_CANCELLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_STATUS_RELEASED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_WAIT_TIMEOUT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_UMD_NOT_READY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.JPCSP_WAIT_UMD;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;


	using IAction = pspsharp.HLE.kernel.types.IAction;
	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using pspUmdInfo = pspsharp.HLE.kernel.types.pspUmdInfo;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using UmdIsoReader = pspsharp.filesystems.umdiso.UmdIsoReader;
	using Scheduler = pspsharp.scheduler.Scheduler;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class sceUmdUser : HLEModule
	{
		public static Logger log = Modules.getLogger("sceUmdUser");
		private bool umdAllowReplace;

		public override void start()
		{
			// Remember if the UMD was activated even after a call to sceKernelLoadExec()
			setUmdActivated();

			umdDeactivateCalled = false;
			waitingThreads = new LinkedList<SceKernelThreadInfo>();
			umdErrorStat = 0;
			umdWaitStateChecker = new UmdWaitStateChecker(this);

			UmdAllowReplace = false;

			base.start();
		}

		protected internal class UmdWaitStateChecker : IWaitStateChecker
		{
			private readonly sceUmdUser outerInstance;

			public UmdWaitStateChecker(sceUmdUser outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				if (outerInstance.checkDriveStat(wait.wantedUmdStat))
				{
					outerInstance.waitingThreads.Remove(thread);
					// Return success
					thread.cpuContext._v0 = 0;

					// Do not continue the wait state
					return false;
				}

				return true;
			}
		}

		private class DelayedUmdSwitch : IAction
		{
			internal UmdIsoReader iso;

			public DelayedUmdSwitch(UmdIsoReader iso)
			{
				this.iso = iso;
			}

			public virtual void execute()
			{
				Modules.sceUmdUserModule.hleDelayedUmdSwitch(iso);
			}
		}

		private class DelayedUmdRemoved : IAction
		{
			public virtual void execute()
			{
				Modules.sceUmdUserModule.hleDelayedUmdSwitch(null);
			}
		}

		protected internal const int PSP_UMD_INIT = 0x00;
		protected internal const int PSP_UMD_NOT_PRESENT = 0x01;
		protected internal const int PSP_UMD_PRESENT = 0x02;
		protected internal const int PSP_UMD_CHANGED = 0x04;
		protected internal const int PSP_UMD_NOT_READY = 0x08;
		protected internal const int PSP_UMD_READY = 0x10;
		protected internal const int PSP_UMD_READABLE = 0x20;
		protected internal UmdIsoReader iso;
		protected internal bool umdActivated;
		protected internal bool umdDeactivateCalled;
		protected internal IList<SceKernelThreadInfo> waitingThreads;
		protected internal int umdErrorStat;
		protected internal UmdWaitStateChecker umdWaitStateChecker;

		public virtual UmdIsoReader IsoReader
		{
			set
			{
				this.iso = value;
				setUmdActivated();
			}
			get
			{
				return iso;
			}
		}


		public virtual int UmdErrorStat
		{
			set
			{
				umdErrorStat = value;
			}
			get
			{
				return umdErrorStat;
			}
		}


		private void setUmdActivated()
		{
			if (iso == null)
			{
				umdActivated = false;
			}
			else
			{
				umdActivated = true;
			}
			Modules.IoFileMgrForUserModule.registerUmdIso();
		}

		public virtual bool UmdActivated
		{
			get
			{
				return umdActivated;
			}
		}

		public virtual int UmdStat
		{
			get
			{
				int stat;
				if (iso != null)
				{
					stat = PSP_UMD_PRESENT | PSP_UMD_READY;
					if (umdActivated)
					{
						stat |= PSP_UMD_READABLE;
					}
				}
				else
				{
					stat = PSP_UMD_NOT_PRESENT;
					if (umdDeactivateCalled)
					{
						stat |= PSP_UMD_NOT_READY;
					}
				}
				return stat;
			}
		}

		public virtual int checkWantedStat(int wantedStat)
		{
			if ((wantedStat & (PSP_UMD_READY | PSP_UMD_READABLE | PSP_UMD_NOT_READY | PSP_UMD_PRESENT | PSP_UMD_NOT_PRESENT)) == 0)
			{
				throw new SceKernelErrorException(ERROR_ERRNO_INVALID_ARGUMENT);
			}
			return wantedStat;
		}

		protected internal virtual bool checkDriveStat(int wantedStat)
		{
			int currentStat = UmdStat;
			return (currentStat & wantedStat) != 0;
		}

		protected internal virtual void removeWaitingThread(SceKernelThreadInfo thread)
		{
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
			for (IEnumerator<SceKernelThreadInfo> lit = waitingThreads.GetEnumerator(); lit.MoveNext();)
			{
				SceKernelThreadInfo waitingThread = lit.Current;
				if (waitingThread.uid == thread.uid)
				{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					lit.remove();
					break;
				}
			}
		}

		public virtual void onThreadWaitTimeout(SceKernelThreadInfo thread)
		{
			log.info("UMD stat timedout");
			removeWaitingThread(thread);
			// Return WAIT_TIMEOUT
			thread.cpuContext._v0 = ERROR_KERNEL_WAIT_TIMEOUT;
		}

		public virtual void onThreadWaitReleased(SceKernelThreadInfo thread)
		{
			log.info("UMD stat released");
			removeWaitingThread(thread);
			// Return ERROR_WAIT_STATUS_RELEASED
			thread.cpuContext._v0 = ERROR_KERNEL_WAIT_STATUS_RELEASED;
		}

		public virtual void onThreadDeleted(SceKernelThreadInfo thread)
		{
			if (thread.waitType == JPCSP_WAIT_UMD)
			{
				removeWaitingThread(thread);
			}
		}

		protected internal virtual void checkWaitingThreads()
		{
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
			for (IEnumerator<SceKernelThreadInfo> lit = waitingThreads.GetEnumerator(); lit.MoveNext();)
			{
				SceKernelThreadInfo waitingThread = lit.Current;
				if (waitingThread.status == SceKernelThreadInfo.PSP_THREAD_WAITING)
				{
					int wantedUmdStat = waitingThread.wait.wantedUmdStat;
					if (waitingThread.waitType == JPCSP_WAIT_UMD && checkDriveStat(wantedUmdStat))
					{
						if (log.DebugEnabled)
						{
							log.debug("sceUmdUser - checkWaitingThreads waking " + waitingThread.uid.ToString("x") + " thread:'" + waitingThread.name + "'");
						}
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						lit.remove();
						// Return success
						waitingThread.cpuContext._v0 = 0;
						// Wakeup thread
						Modules.ThreadManForUserModule.hleChangeThreadState(waitingThread, SceKernelThreadInfo.PSP_THREAD_READY);
					}
				}
			}
		}

		protected internal virtual int hleUmdWaitDriveStat(int wantedStat, bool doCallbacks, bool doTimeout, int timeout)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;

			if (!checkDriveStat(wantedStat))
			{
				SceKernelThreadInfo currentThread = threadMan.CurrentThread;
				// Wait on a specific umdStat.
				currentThread.wait.wantedUmdStat = wantedStat;
				waitingThreads.Add(currentThread);
				threadMan.hleKernelThreadEnterWaitState(currentThread, JPCSP_WAIT_UMD, -1, umdWaitStateChecker, timeout, !doTimeout, doCallbacks);
			}
			threadMan.hleRescheduleCurrentThread(doCallbacks);

			return 0;
		}

		protected internal virtual int NotificationArg
		{
			get
			{
				return getNotificationArg(iso != null);
			}
		}

		protected internal virtual int getNotificationArg(bool umdPresent)
		{
			int notifyArg;

			if (umdPresent)
			{
				notifyArg = PSP_UMD_PRESENT | PSP_UMD_READABLE;
				// The PSP is returning 0x32 instead of 0x22 when
				//     sceKernelSetCompiledSdkVersion()
				// has been called (i.e. when sceKernelGetCompiledSdkVersion() != 0).
				if (Modules.SysMemUserForUserModule.hleKernelGetCompiledSdkVersion() != 0)
				{
					notifyArg |= PSP_UMD_READY;
				}
			}
			else
			{
				notifyArg = PSP_UMD_NOT_PRESENT | PSP_UMD_NOT_READY;
			}

			return notifyArg;
		}

		public virtual bool UmdAllowReplace
		{
			get
			{
				return umdAllowReplace;
			}
			set
			{
				this.umdAllowReplace = value;
    
				// Update the visibility of the "Switch UMD" menu item
				Emulator.MainGUI.onUmdChange();
			}
		}


		public virtual void hleUmdSwitch(UmdIsoReader newIso)
		{
			Scheduler scheduler = Scheduler.Instance;

			long delayedUmdSwitchSchedule = Scheduler.Now;
			if (iso != null)
			{
				// First notify that the UMD has been removed
				scheduler.addAction(new DelayedUmdRemoved());

				// After 100ms delay, notify that a new UMD has been inserted
				delayedUmdSwitchSchedule += 100 * 1000;
			}

			scheduler.addAction(delayedUmdSwitchSchedule, new DelayedUmdSwitch(newIso));
		}

		protected internal virtual void hleDelayedUmdRemoved()
		{
			int notifyArg = getNotificationArg(false);
			Modules.ThreadManForUserModule.hleKernelNotifyCallback(SceKernelThreadInfo.THREAD_CALLBACK_UMD, notifyArg);
		}

		protected internal virtual void hleDelayedUmdSwitch(UmdIsoReader iso)
		{
			Modules.IoFileMgrForUserModule.IsoReader = iso;
			IsoReader = iso;

			int notifyArg = NotificationArg | PSP_UMD_CHANGED;
			Modules.ThreadManForUserModule.hleKernelNotifyCallback(SceKernelThreadInfo.THREAD_CALLBACK_UMD, notifyArg);
		}

		[HLEFunction(nid : 0x46EBB729, version : 150)]
		public virtual bool sceUmdCheckMedium()
		{
			return iso != null;
		}

		[HLEFunction(nid : 0xC6183D47, version : 150, checkInsideInterrupt : true)]
		public virtual int sceUmdActivate(int mode, PspString drive)
		{
			umdActivated = true;
			Modules.IoFileMgrForUserModule.registerUmdIso();

			// Notify the callback.
			// The callback will be executed at the next sceXXXXCB() syscall.
			int notifyArg = NotificationArg;
			Modules.ThreadManForUserModule.hleKernelNotifyCallback(SceKernelThreadInfo.THREAD_CALLBACK_UMD, notifyArg);

			checkWaitingThreads();

			// int arg[] = { 1 };
			// sceIoAssign(drive, "umd0:", "isofs0:", 1, &arg, 4);
			int sceIoAssign = NIDMapper.Instance.getAddressByName("sceIoAssign");
			if (sceIoAssign != 0)
			{
				SysMemInfo memInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "sceUmdActivate", SysMemUserForUser.PSP_SMEM_Low, 32, 0);
				int argAddr = memInfo.addr;
				int umdAddr = memInfo.addr + 4;
				int isofsAddr = memInfo.addr + 10;

				Memory mem = Memory.Instance;
				Utilities.writeStringZ(mem, umdAddr, "umd0:");
				Utilities.writeStringZ(mem, isofsAddr, "isofs0:");
				mem.write32(argAddr, 1);

				SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;
				Modules.ThreadManForUserModule.executeCallback(thread, sceIoAssign, null, false, drive.Address, umdAddr, isofsAddr, 1, argAddr, 4);
			}

			return 0;
		}

		[HLEFunction(nid : 0xE83742BA, version : 150, checkInsideInterrupt : true)]
		public virtual int sceUmdDeactivate(int mode, PspString drive)
		{
			// Trigger the callback only if the UMD was already activated.
			// The callback will be executed at the next sceXXXXCB() syscall.
			bool triggerCallback = umdActivated;
			umdActivated = false;
			Modules.IoFileMgrForUserModule.registerUmdIso();
			umdDeactivateCalled = true;
			if (triggerCallback)
			{
				int notifyArg;
				if (iso != null)
				{
					notifyArg = PSP_UMD_PRESENT | PSP_UMD_READY;
				}
				else
				{
					notifyArg = PSP_UMD_NOT_PRESENT | PSP_UMD_NOT_READY;
				}
				Modules.ThreadManForUserModule.hleKernelNotifyCallback(SceKernelThreadInfo.THREAD_CALLBACK_UMD, notifyArg);
			}
			checkWaitingThreads();

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8EF08FCE, version = 150, checkInsideInterrupt = true) public int sceUmdWaitDriveStat(@CheckArgument("checkWantedStat") int wantedStat)
		[HLEFunction(nid : 0x8EF08FCE, version : 150, checkInsideInterrupt : true)]
		public virtual int sceUmdWaitDriveStat(int wantedStat)
		{
			return hleUmdWaitDriveStat(wantedStat, false, false, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x56202973, version = 150, checkInsideInterrupt = true) public int sceUmdWaitDriveStatWithTimer(@CheckArgument("checkWantedStat") int wantedStat, int timeout)
		[HLEFunction(nid : 0x56202973, version : 150, checkInsideInterrupt : true)]
		public virtual int sceUmdWaitDriveStatWithTimer(int wantedStat, int timeout)
		{
			return hleUmdWaitDriveStat(wantedStat, false, true, timeout);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x4A9E5E29, version = 150, checkInsideInterrupt = true) public int sceUmdWaitDriveStatCB(@CheckArgument("checkWantedStat") int wantedStat, int timeout)
		[HLEFunction(nid : 0x4A9E5E29, version : 150, checkInsideInterrupt : true)]
		public virtual int sceUmdWaitDriveStatCB(int wantedStat, int timeout)
		{
			return hleUmdWaitDriveStat(wantedStat, true, true, timeout);
		}

		[HLEFunction(nid : 0x6AF9B50A, version : 150)]
		public virtual int sceUmdCancelWaitDriveStat()
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;

//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
			for (IEnumerator<SceKernelThreadInfo> lit = waitingThreads.GetEnumerator(); lit.MoveNext();)
			{
				SceKernelThreadInfo waitingThread = lit.Current;
				if (!waitingThread.Waiting || waitingThread.waitType != JPCSP_WAIT_UMD)
				{
					log.warn(string.Format("sceUmdCancelWaitDriveStat thread {0} not waiting on umd", waitingThread));
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceUmdCancelWaitDriveStat waking thread {0}", waitingThread));
					}
					lit.remove();
					// Return WAIT_CANCELLED.
					waitingThread.cpuContext._v0 = ERROR_KERNEL_WAIT_CANCELLED;
					// Wakeup thread
					threadMan.hleChangeThreadState(waitingThread, SceKernelThreadInfo.PSP_THREAD_READY);
				}
			}

			return 0;
		}

		[HLEFunction(nid : 0x6B4A146C, version : 150)]
		public virtual int sceUmdGetDriveStat()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceUmdGetDriveStat returning 0x{0:X}", UmdStat));
			}

			return UmdStat;
		}

		[HLEFunction(nid : 0x20628E6F, version : 150)]
		public virtual int sceUmdGetErrorStat()
		{
			return UmdErrorStat;
		}

		[HLEFunction(nid : 0x340B7686, version : 150, checkInsideInterrupt : true)]
		public virtual int sceUmdGetDiscInfo(TPointer pspUmdInfoAddr)
		{
			pspUmdInfo umdInfo = new pspUmdInfo();
			umdInfo.read(pspUmdInfoAddr);
			if (umdInfo.@sizeof() != 8)
			{
				return SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
			}
			umdInfo.type = pspUmdInfo.PSP_UMD_TYPE_GAME;
			umdInfo.write(pspUmdInfoAddr);

			return 0;
		}

		[HLEFunction(nid : 0xAEE7404D, version : 150, checkInsideInterrupt : true)]
		public virtual int sceUmdRegisterUMDCallBack(int uid)
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			if (!threadMan.hleKernelRegisterCallback(SceKernelThreadInfo.THREAD_CALLBACK_UMD, uid))
			{
				return -1;
			}

			return 0;
		}

		[HLEFunction(nid : 0xBD2BDE07, version : 150)]
		public virtual int sceUmdUnRegisterUMDCallBack(int uid)
		{
			if (!Modules.ThreadManForUserModule.hleKernelUnRegisterCallback(SceKernelThreadInfo.THREAD_CALLBACK_UMD, uid))
			{
				return -1;
			}

			return 0;
		}

		[HLEFunction(nid : 0x87533940, version : 200)]
		public virtual int sceUmdReplaceProhibit()
		{
			if ((UmdStat & PSP_UMD_READY) != PSP_UMD_READY || (UmdStat & PSP_UMD_READABLE) != PSP_UMD_READABLE)
			{
				return ERROR_UMD_NOT_READY;
			}

			UmdAllowReplace = false;

			return 0;
		}

		[HLEFunction(nid : 0xCBE9F02A, version : 200)]
		public virtual int sceUmdReplacePermit()
		{
			UmdAllowReplace = true;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x14C6C45C, version = 660) public int sceUmdUnuseUMDInMsUsbWlan()
		[HLEFunction(nid : 0x14C6C45C, version : 660)]
		public virtual int sceUmdUnuseUMDInMsUsbWlan()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB103FA38, version = 660) public int sceUmdUseUMDInMsUsbWlan()
		[HLEFunction(nid : 0xB103FA38, version : 660)]
		public virtual int sceUmdUseUMDInMsUsbWlan()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x816E656B, version = 660) public void sceUmdSetSuspendResumeMode(int mode)
		[HLEFunction(nid : 0x816E656B, version : 660)]
		public virtual void sceUmdSetSuspendResumeMode(int mode)
		{
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x899B5C41, version = 660) public int sceUmdGetSuspendResumeMode()
		[HLEFunction(nid : 0x899B5C41, version : 660)]
		public virtual int sceUmdGetSuspendResumeMode()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD45D1FE6, version = 150) public int sceUmdGetDriveStatus()
		[HLEFunction(nid : 0xD45D1FE6, version : 150)]
		public virtual int sceUmdGetDriveStatus()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB7BF4C31, version = 660) public int sceUmdGetDriveStatus_660()
		[HLEFunction(nid : 0xB7BF4C31, version : 660)]
		public virtual int sceUmdGetDriveStatus_660()
		{
			return sceUmdGetDriveStatus();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x230666E3, version = 150) public int sceUmdSetDriveStatus(int state)
		[HLEFunction(nid : 0x230666E3, version : 150)]
		public virtual int sceUmdSetDriveStatus(int state)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x982272FE, version = 660) public int sceUmdSetDriveStatus_660(int state)
		[HLEFunction(nid : 0x982272FE, version : 660)]
		public virtual int sceUmdSetDriveStatus_660(int state)
		{
			return sceUmdSetDriveStatus(state);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAE53DC2D, version = 150) public int sceUmdClearDriveStatus(int state)
		[HLEFunction(nid : 0xAE53DC2D, version : 150)]
		public virtual int sceUmdClearDriveStatus(int state)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6EDF57F1, version = 660) public int sceUmdClearDriveStatus_660(int state)
		[HLEFunction(nid : 0x6EDF57F1, version : 660)]
		public virtual int sceUmdClearDriveStatus_660(int state)
		{
			return sceUmdClearDriveStatus(state);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x040A7090, version = 660) public int sceUmd_040A7090(int errorCode)
		[HLEFunction(nid : 0x040A7090, version : 660)]
		public virtual int sceUmd_040A7090(int errorCode)
		{
			// Error code mapping?
			return errorCode;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7850F057, version = 150) public int sceUmdRegisterGetUMDInfoCallBack(pspsharp.HLE.TPointer callback, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.variableLength, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer discInfoAddr)
		[HLEFunction(nid : 0x7850F057, version : 150)]
		public virtual int sceUmdRegisterGetUMDInfoCallBack(TPointer callback, TPointer discInfoAddr)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x48EF868C, version = 660) public int sceUmdRegisterGetUMDInfoCallBack_660(pspsharp.HLE.TPointer callback, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.variableLength, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer discInfoAddr)
		[HLEFunction(nid : 0x48EF868C, version : 660)]
		public virtual int sceUmdRegisterGetUMDInfoCallBack_660(TPointer callback, TPointer discInfoAddr)
		{
			return sceUmdRegisterGetUMDInfoCallBack(callback, discInfoAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x63517CBA, version = 150) public int sceUmd_63517CBA(pspsharp.HLE.TPointer callback, int callbackArg)
		[HLEFunction(nid : 0x63517CBA, version : 150)]
		public virtual int sceUmd_63517CBA(TPointer callback, int callbackArg)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x086DDC0D, version = 150) public int sceUmdRegisterActivateCallBack(pspsharp.HLE.TPointer callback, int callbackArg)
		[HLEFunction(nid : 0x086DDC0D, version : 150)]
		public virtual int sceUmdRegisterActivateCallBack(TPointer callback, int callbackArg)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9B0F59CE, version = 660) public int sceUmdRegisterActivateCallBack_660(pspsharp.HLE.TPointer callback, int callbackArg)
		[HLEFunction(nid : 0x9B0F59CE, version : 660)]
		public virtual int sceUmdRegisterActivateCallBack_660(TPointer callback, int callbackArg)
		{
			return sceUmdRegisterActivateCallBack(callback, callbackArg);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2D81508D, version = 150) public int sceUmdRegisterDeactivateCallBack(pspsharp.HLE.TPointer callback, int callbackArg)
		[HLEFunction(nid : 0x2D81508D, version : 150)]
		public virtual int sceUmdRegisterDeactivateCallBack(TPointer callback, int callbackArg)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD1C80E51, version = 660) public int sceUmdRegisterDeactivateCallBack_660(pspsharp.HLE.TPointer callback, int callbackArg)
		[HLEFunction(nid : 0xD1C80E51, version : 660)]
		public virtual int sceUmdRegisterDeactivateCallBack_660(TPointer callback, int callbackArg)
		{
			return sceUmdRegisterDeactivateCallBack(callback, callbackArg);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4832ABF3, version = 150) public int sceUmdRegisterReplaceCallBack(pspsharp.HLE.TPointer callback)
		[HLEFunction(nid : 0x4832ABF3, version : 150)]
		public virtual int sceUmdRegisterReplaceCallBack(TPointer callback)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3748C4DB, version = 660) public int sceUmdRegisterReplaceCallBack_660(pspsharp.HLE.TPointer callback)
		[HLEFunction(nid : 0x3748C4DB, version : 660)]
		public virtual int sceUmdRegisterReplaceCallBack_660(TPointer callback)
		{
			return sceUmdRegisterReplaceCallBack(callback);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x76D356F9, version = 660) public int sceUmd_76D356F9(pspsharp.HLE.TPointer callback)
		[HLEFunction(nid : 0x76D356F9, version : 660)]
		public virtual int sceUmd_76D356F9(TPointer callback)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEB56097E, version = 150) public int sceUmdGetDetectUMDCallBackId()
		[HLEFunction(nid : 0xEB56097E, version : 150)]
		public virtual int sceUmdGetDetectUMDCallBackId()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA55109DD, version = 660) public int sceUmdGetDetectUMDCallBackId_660()
		[HLEFunction(nid : 0xA55109DD, version : 660)]
		public virtual int sceUmdGetDetectUMDCallBackId_660()
		{
			return 0;
		}
	}
}