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
//	import static pspsharp.HLE.TPointer.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_INVALID_MODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_POWER_VMEM_IN_USE;
	using Managers = pspsharp.HLE.kernel.Managers;
	using SceKernelSemaInfo = pspsharp.HLE.kernel.types.SceKernelSemaInfo;
	using Screen = pspsharp.hardware.Screen;

	//using Logger = org.apache.log4j.Logger;

	public class sceSuspendForUser : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceSuspendForUser");
		public const int KERNEL_POWER_TICK_SUSPEND_AND_DISPLAY = 0;
		public const int KERNEL_POWER_TICK_SUSPEND = 1;
		public const int KERNEL_POWER_TICK_DISPLAY = 6;
		protected internal SceKernelSemaInfo volatileMemSema;
		protected internal const int volatileMemSignal = 1;
		// Volatile mem is always at 0x08400000
		public const int KERNEL_VOLATILE_MEM_START = 0x08400000;
		// Volatile mem size is 4Megs
		public const int KERNEL_VOLATILE_MEM_SIZE = 0x400000;

		public override void start()
		{
			base.start();

			volatileMemSema = Managers.semas.hleKernelCreateSema("ScePowerVmem", 0, volatileMemSignal, volatileMemSignal, NULL);
		}

		public virtual int hleKernelVolatileMemLock(int type, bool trylock)
		{
			if (trylock)
			{
				if (Managers.semas.hleKernelPollSema(volatileMemSema, volatileMemSignal) != 0)
				{
					// Volatile mem is already locked
					return ERROR_POWER_VMEM_IN_USE;
				}
				return 0;
			}

			// If the volatile mem is already locked, the current thread has to wait
			// until it is unlocked.
			return Managers.semas.hleKernelWaitSema(volatileMemSema, volatileMemSignal, TPointer32.NULL, false);
		}

		public virtual int hleKernelVolatileMemUnlock(int type)
		{
			return Managers.semas.hleKernelSignalSema(volatileMemSema, volatileMemSignal);
		}

		protected internal virtual int hleKernelVolatileMemLock(int type, TPointer32 paddr, TPointer32 psize, bool trylock)
		{
			if (type != 0)
			{
				Console.WriteLine(string.Format("hleKernelVolatileMemLock bad param type={0:D}", type));
				return ERROR_INVALID_MODE;
			}

			paddr.setValue(KERNEL_VOLATILE_MEM_START);
			psize.setValue(KERNEL_VOLATILE_MEM_SIZE);

			return hleKernelVolatileMemLock(type, trylock);
		}

		public virtual int hleKernelPowerTick(int flag)
		{
			// The PSP is checking each of the lower 8 bits of the flag value to tick different
			// components.
			// Here we check only a few known bits...
			if ((flag & KERNEL_POWER_TICK_SUSPEND) == KERNEL_POWER_TICK_SUSPEND)
			{
				if (log.TraceEnabled)
				{
					log.trace("IGNORING:sceKernelPowerTick(KERNEL_POWER_TICK_SUSPEND)");
				}
			}

			if ((flag & KERNEL_POWER_TICK_DISPLAY) == KERNEL_POWER_TICK_DISPLAY)
			{
				Screen.hleKernelPowerTick();
				if (log.TraceEnabled)
				{
					log.trace("IGNORING:sceKernelPowerTick(KERNEL_POWER_TICK_DISPLAY)");
				}
			}

			if (flag == KERNEL_POWER_TICK_SUSPEND_AND_DISPLAY)
			{
				Screen.hleKernelPowerTick();
				if (log.TraceEnabled)
				{
					log.trace("IGNORING:sceKernelPowerTick(KERNEL_POWER_TICK_SUSPEND_AND_DISPLAY)");
				}
			}

			return 0;
		}

		[HLELogging(level:"trace"), HLEFunction(nid : 0xEADB1BD7, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelPowerLock(int type)
		{
			return 0;
		}

		[HLELogging(level:"trace"), HLEFunction(nid : 0x3AEE7261, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelPowerUnlock(int type)
		{
			return 0;
		}

		[HLELogging(level:"trace"), HLEFunction(nid : 0x090CCB3F, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelPowerTick(int flag)
		{
			return hleKernelPowerTick(flag);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x3E0271D3, version = 150, checkInsideInterrupt = true) public int sceKernelVolatileMemLock(int type, @CanBeNull pspsharp.HLE.TPointer32 paddr, @CanBeNull pspsharp.HLE.TPointer32 psize)
		[HLEFunction(nid : 0x3E0271D3, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelVolatileMemLock(int type, TPointer32 paddr, TPointer32 psize)
		{
			return hleKernelVolatileMemLock(type, paddr, psize, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA14F40B2, version = 150) public int sceKernelVolatileMemTryLock(int type, @CanBeNull pspsharp.HLE.TPointer32 paddr, @CanBeNull pspsharp.HLE.TPointer32 psize)
		[HLEFunction(nid : 0xA14F40B2, version : 150)]
		public virtual int sceKernelVolatileMemTryLock(int type, TPointer32 paddr, TPointer32 psize)
		{
			return hleKernelVolatileMemLock(type, paddr, psize, true);
		}

		[HLEFunction(nid : 0xA569E425, version : 150)]
		public virtual int sceKernelVolatileMemUnlock(int type)
		{
			if (type != 0)
			{
				Console.WriteLine(string.Format("sceKernelVolatileMemUnlock bad param type={0:D}", type));
				return ERROR_INVALID_MODE;
			}

			return hleKernelVolatileMemUnlock(type);
		}
	}
}