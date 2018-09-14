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
	using AlarmInterruptHandler = pspsharp.HLE.kernel.types.interrupts.AlarmInterruptHandler;
	using AlarmInterruptAction = pspsharp.scheduler.AlarmInterruptAction;
	using AlarmInterruptResultAction = pspsharp.scheduler.AlarmInterruptResultAction;

	public class SceKernelAlarmInfo : pspAbstractMemoryMappedStructureVariableLength
	{
		private const string uidPurpose = "ThreadMan-Alarm";
		public long schedule;
		public int handlerAddress;
		public int handlerArgument;

		public readonly int uid;
		public readonly AlarmInterruptHandler alarmInterruptHandler;
		public readonly AlarmInterruptAction alarmInterruptAction;
		public readonly AlarmInterruptResultAction alarmInterruptResultAction;

		public SceKernelAlarmInfo(long schedule, int handlerAddress, int handlerArgument)
		{
			this.schedule = schedule;
			this.handlerAddress = handlerAddress;
			this.handlerArgument = handlerArgument;

			uid = SceUidManager.getNewUid(uidPurpose);
			alarmInterruptHandler = new AlarmInterruptHandler(handlerAddress, handlerArgument);
			alarmInterruptAction = new AlarmInterruptAction(this);
			alarmInterruptResultAction = new AlarmInterruptResultAction(this);
		}

		protected internal override void read()
		{
			base.read();
			schedule = read64();
			handlerAddress = read32();
			handlerArgument = read32();
		}

		protected internal override void write()
		{
			base.write();
			write64(schedule);
			write32(handlerAddress);
			write32(handlerArgument);
		}

		public virtual void delete()
		{
			SceUidManager.releaseUid(uid, uidPurpose);
		}
	}

}