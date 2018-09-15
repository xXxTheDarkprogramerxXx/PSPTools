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

	//using Logger = org.apache.log4j.Logger;

	public class SystemTimeManager
	{

		protected internal static Logger log = Modules.getLogger("ThreadManForUser");

		public virtual void reset()
		{
		}

		/// <summary>
		/// Convert a number of sysclocks into microseconds.
		/// </summary>
		/// <param name="sysclocks">	- number of sysclocks </param>
		/// <returns> microseconds </returns>
		public static long hleSysClock2USec(long sysclocks)
		{
			// 1 sysclock == 1 microsecond
			return sysclocks;
		}

		/// <summary>
		/// Convert a number of sysclocks into microseconds,
		/// truncating to 32 bits.
		/// </summary>
		/// <param name="sysclocks">	- number of sysclocks </param>
		/// <returns> microseconds (truncated to 32 bits)
		///         Integer.MAX_VALUE or MIN_VALUE in case of truncation overflow. </returns>
		public static int hleSysClock2USec32(long sysclocks)
		{
			long micros64 = hleSysClock2USec(sysclocks);

			int micros32 = (int) micros64;
			if (micros64 > int.MaxValue)
			{
				micros32 = int.MaxValue;
			}
			else if (micros64 < int.MinValue)
			{
				micros32 = int.MinValue;
			}

			return micros32;
		}

		public virtual int sceKernelUSec2SysClock(int usec, TPointer64 sysClockAddr)
		{
			sysClockAddr.Value = usec & 0xFFFFFFFFL;
			return 0;
		}

		public virtual long sceKernelUSec2SysClockWide(int usec)
		{
			return usec & 0xFFFFFFFFL;
		}

		public virtual int sceKernelSysClock2USec(TPointer64 sysClockAddr, TPointer32 secAddr, TPointer32 microSecAddr)
		{
			long sysClock = sysClockAddr.Value;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelSysClock2USec sysClockAddr={0}({1:D}), secAddr={2}, microSecAddr={3}", sysClockAddr, sysClock, secAddr, microSecAddr));
			}

			if (secAddr.Null)
			{
				// PSP is copying sysclock value directly to microSecAddr when secAddr is NULL
				microSecAddr.setValue((int) sysClock);
			}
			else
			{
				secAddr.setValue((int)(sysClock / 1000000));
				microSecAddr.setValue((int)(sysClock % 1000000));
			}

			return 0;
		}

		public virtual int sceKernelSysClock2USecWide(long sysClock, TPointer32 secAddr, TPointer32 microSecAddr)
		{
			if (secAddr.Null)
			{
				// PSP is copying sysclock value directly to microSecAddr when secAddr is NULL
				microSecAddr.setValue((int) sysClock);
			}
			else
			{
				secAddr.setValue((int)(sysClock / 1000000));
				microSecAddr.setValue((int)(sysClock % 1000000));
			}

			return 0;
		}

		public static long SystemTime
		{
			get
			{
				// System time is number of microseconds since program start
				return Emulator.Clock.microTime();
			}
		}

		public virtual int sceKernelGetSystemTime(TPointer64 time_addr)
		{
			long systemTime = SystemTime;
			time_addr.Value = systemTime;

			return 0;
		}

		public virtual long sceKernelGetSystemTimeWide()
		{
			long systemTime = SystemTime;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelGetSystemTimeWide returning {0:D}", systemTime));
			}
			return systemTime;
		}

		public virtual int sceKernelGetSystemTimeLow()
		{
			int systemTimeLow = unchecked((int)(SystemTime & 0xFFFFFFFFL));
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelGetSystemTimeLow returning {0:D}", systemTimeLow));
			}
			return systemTimeLow;
		}

		public static readonly SystemTimeManager singleton = new SystemTimeManager();

		private SystemTimeManager()
		{
		}
	}
}