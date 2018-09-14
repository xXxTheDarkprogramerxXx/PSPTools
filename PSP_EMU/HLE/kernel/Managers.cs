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
namespace pspsharp.HLE.kernel
{
	using EventFlagManager = pspsharp.HLE.kernel.managers.EventFlagManager;
	using FplManager = pspsharp.HLE.kernel.managers.FplManager;
	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using MbxManager = pspsharp.HLE.kernel.managers.MbxManager;
	using ModuleManager = pspsharp.HLE.kernel.managers.ModuleManager;
	using MsgPipeManager = pspsharp.HLE.kernel.managers.MsgPipeManager;
	using MutexManager = pspsharp.HLE.kernel.managers.MutexManager;
	using LwMutexManager = pspsharp.HLE.kernel.managers.LwMutexManager;
	using SemaManager = pspsharp.HLE.kernel.managers.SemaManager;
	using SystemTimeManager = pspsharp.HLE.kernel.managers.SystemTimeManager;
	using VplManager = pspsharp.HLE.kernel.managers.VplManager;

	/// 
	/// <summary>
	/// @author hli
	/// </summary>
	public class Managers
	{
		public static SemaManager semas;
		public static EventFlagManager eventFlags;
		public static FplManager fpl;
		public static VplManager vpl;
		public static MutexManager mutex;
		public static LwMutexManager lwmutex;
		public static MsgPipeManager msgPipes;
		public static ModuleManager modules;
		public static SystemTimeManager systime;
		public static MbxManager mbx;
		public static IntrManager intr;

		/// <summary>
		/// call this when resetting the emulator </summary>
		public static void reset()
		{
			semas.reset();
			eventFlags.reset();
			fpl.reset();
			vpl.reset();
			mutex.reset();
			lwmutex.reset();
			msgPipes.reset();
			modules.reset();
			systime.reset();
			mbx.reset();
			intr.reset();
		}

		static Managers()
		{
			semas = SemaManager.singleton;
			eventFlags = EventFlagManager.singleton;
			fpl = FplManager.singleton;
			vpl = VplManager.singleton;
			mutex = MutexManager.singleton;
			lwmutex = LwMutexManager.singleton;
			msgPipes = MsgPipeManager.singleton;
			modules = ModuleManager.singleton;
			systime = SystemTimeManager.singleton;
			mbx = MbxManager.singleton;
			intr = IntrManager.Instance;
		}
	}
}