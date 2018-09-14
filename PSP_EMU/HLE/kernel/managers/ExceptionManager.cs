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
	public class ExceptionManager
	{
		public const int EXCEP_INT = 0; // Interrupt
		public const int EXCEP_ADEL = 4; // Load of instruction fetch exception
		public const int EXCEP_ADES = 5; // Address store exception
		public const int EXCEP_IBE = 6; // Instruction fetch bus error
		public const int EXCEP_DBE = 7; // Load/store bus error
		public const int EXCEP_SYS = 8; // Syscall
		public const int EXCEP_BP = 9; // Breakpoint
		public const int EXCEP_RI = 10; // Reserved instruction
		public const int EXCEP_CPU = 11; // Coprocessor unusable
		public const int EXCEP_OV = 12; // Arithmetic overflow
		public const int EXCEP_FPE = 15; // Floating-point exception
		public const int EXCEP_WATCH = 23; // Watch (reference to WatchHi/WatchLo)
		public const int EXCEP_VCED = 31; // "Virtual Coherency Exception Data" (used for NMI handling apparently)

		public static readonly int IP0 = (1 << 0);
		public static readonly int IP1 = (1 << 1);
		public static readonly int IP2 = (1 << 2);
		public static readonly int IP3 = (1 << 3);
		public static readonly int IP4 = (1 << 4);
		public static readonly int IP5 = (1 << 5);
		public static readonly int IP6 = (1 << 6);
		public static readonly int IP7 = (1 << 7);
	}

}