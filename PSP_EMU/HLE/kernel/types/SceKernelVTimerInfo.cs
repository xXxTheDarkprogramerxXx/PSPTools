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
	using SystemTimeManager = pspsharp.HLE.kernel.managers.SystemTimeManager;
	using VTimerInterruptHandler = pspsharp.HLE.kernel.types.interrupts.VTimerInterruptHandler;
	using SysMemUserForUser = pspsharp.HLE.modules.SysMemUserForUser;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using VTimerInterruptAction = pspsharp.scheduler.VTimerInterruptAction;
	using VTimerInterruptResultAction = pspsharp.scheduler.VTimerInterruptResultAction;

	public class SceKernelVTimerInfo : pspAbstractMemoryMappedStructureVariableLength
	{
		public string name;
		public int active;
		public long @base;
		public long current;
		public long schedule;
		public int handlerAddress;
		public int handlerArgument;

		public readonly int uid;
		public VTimerInterruptHandler vtimerInterruptHandler;
		public readonly VTimerInterruptAction vtimerInterruptAction;
		public readonly VTimerInterruptResultAction vtimerInterruptResultAction;
		private int internalMemory;
		private SysMemUserForUser.SysMemInfo sysMemInfo;

		public const int ACTIVE_RUNNING = 1;
		public const int ACTIVE_STOPPED = 0;

		public SceKernelVTimerInfo(string name)
		{
			this.name = name;
			active = ACTIVE_STOPPED;

			uid = SceUidManager.getNewUid("ThreadMan-VTimer");
			vtimerInterruptHandler = new VTimerInterruptHandler(this);
			vtimerInterruptAction = new VTimerInterruptAction(this);
			vtimerInterruptResultAction = new VTimerInterruptResultAction(this);
			internalMemory = 0;
		}

		public virtual int InternalMemory
		{
			get
			{
				if (internalMemory == 0)
				{
					// Allocate enough memory to store "current" and "schedule"
					sysMemInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.USER_PARTITION_ID, "SceKernelVTimerInfo", SysMemUserForUser.PSP_SMEM_Low, 16, 0);
					if (sysMemInfo != null)
					{
						internalMemory = sysMemInfo.addr;
					}
				}
    
				return internalMemory;
			}
		}

		public virtual void delete()
		{
			if (internalMemory != 0)
			{
				Modules.SysMemUserForUserModule.free(sysMemInfo);
				internalMemory = 0;
			}
		}

		protected internal override void write()
		{
			base.write();
			writeStringNZ(32, name);
			write32(active);
			write64(@base);
			write64(CurrentTime);
			write64(schedule);
			write32(handlerAddress);
			write32(handlerArgument);
		}

		public virtual long RunningTime
		{
			get
			{
				if (active != ACTIVE_RUNNING)
				{
					return 0;
				}
    
				return SystemTimeManager.SystemTime - @base;
			}
		}

		public virtual long CurrentTime
		{
			get
			{
				return current + RunningTime;
			}
		}

		public override string ToString()
		{
			return string.Format("VTimer uid=0x{0:X}, name='{1}', handler=0x{2:X8}(arg=0x{3:X})", uid, name, handlerAddress, handlerArgument);
		}
	}

}