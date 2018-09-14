using System;
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
namespace pspsharp.Debugger
{

	using Modules = pspsharp.HLE.Modules;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;

	public class DumpDebugState
	{
		public static void dumpDebugState()
		{
			log("------------------------------------------------------------");
			if (GameLoaded)
			{
				dumpCurrentFrame();
				dumpThreads();
				Modules.SysMemUserForUserModule.dumpSysMemInfo();
			}
			else
			{
				log("No game loaded");
			}
			log("------------------------------------------------------------");
		}

		public static void dumpThreads()
		{
			ThreadManForUser threadMan = Modules.ThreadManForUserModule;

			for (IEnumerator<SceKernelThreadInfo> it = threadMan.GetEnumerator(); it.MoveNext();)
			{
				SceKernelThreadInfo thread = it.Current;

				log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
				log(string.Format("Thread Name: '{0}' ID: 0x{1:X4} Module ID: 0x{2:X4}", thread.name, thread.uid, thread.moduleid));
				log(string.Format("Thread Status: 0x{0:X8} {1}", thread.status, thread.StatusName));
				log(string.Format("Thread Attr: 0x{0:X8} Current Priority: 0x{1:X2} Initial Priority: 0x{2:X2}", thread.attr, thread.currentPriority, thread.initPriority));
				log(string.Format("Thread Entry: 0x{0:X8} Stack: 0x{1:X8} - 0x{2:X8} Stack Size: 0x{3:X8}", thread.entry_addr, thread.StackAddr, thread.StackAddr + thread.stackSize, thread.stackSize));
				log(string.Format("Thread Run Clocks: {0:D} Exit Code: 0x{1:X8}", thread.runClocks, thread.exitStatus));
				log(string.Format("Thread Wait Type: {0} Us: {1:D} Forever: {2}", thread.WaitName, thread.wait.micros, thread.wait.forever));
			}
		}

		public static void dumpCurrentFrame()
		{
			StepFrame frame = new StepFrame();
			frame.make(Emulator.Processor.cpu);
			log(frame.Message);
		}

		private static bool GameLoaded
		{
			get
			{
				return Modules.ThreadManForUserModule.CurrentThreadID != -1;
			}
		}

		public static void log(string msg)
		{
			Console.Error.WriteLine(msg);
			Modules.log.error(msg);
		}
	}
}