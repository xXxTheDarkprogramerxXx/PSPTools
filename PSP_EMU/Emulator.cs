using System.Threading;

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
namespace pspsharp
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.SysMemUserForUser.USER_PARTITION_ID;

	using Compiler = pspsharp.Allegrex.compiler.Compiler;
	using Profiler = pspsharp.Allegrex.compiler.Profiler;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using InstructionCounter = pspsharp.Debugger.InstructionCounter;
	using StepLogger = pspsharp.Debugger.StepLogger;
	using IMainGUI = pspsharp.GUI.IMainGUI;
	using HLEModuleManager = pspsharp.HLE.HLEModuleManager;
	using HLEUidObjectMapping = pspsharp.HLE.HLEUidObjectMapping;
	using Modules = pspsharp.HLE.Modules;
	using Managers = pspsharp.HLE.kernel.Managers;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using SysMemUserForUser = pspsharp.HLE.modules.SysMemUserForUser;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using GEProfiler = pspsharp.graphics.GEProfiler;
	using VertexCache = pspsharp.graphics.VertexCache;
	using VideoEngine = pspsharp.graphics.VideoEngine;
	using ExternalGE = pspsharp.graphics.RE.externalge.ExternalGE;
	using BasePrimitiveRenderer = pspsharp.graphics.RE.software.BasePrimitiveRenderer;
	using BaseRenderer = pspsharp.graphics.RE.software.BaseRenderer;
	using RendererExecutor = pspsharp.graphics.RE.software.RendererExecutor;
	using TextureCache = pspsharp.graphics.textures.TextureCache;
	using Battery = pspsharp.hardware.Battery;
	using Wlan = pspsharp.hardware.Wlan;
	using MemorySections = pspsharp.memory.MemorySections;
	using ProOnlineNetworkAdapter = pspsharp.network.proonline.ProOnlineNetworkAdapter;
	using Scheduler = pspsharp.scheduler.Scheduler;
	using SoundChannel = pspsharp.sound.SoundChannel;
	using DurationStatistics = pspsharp.util.DurationStatistics;
	using JpcspDialogManager = pspsharp.util.JpcspDialogManager;

	using Logger = org.apache.log4j.Logger;

	/*
	 * TODO list:
	 * 1. Cleanup initialization in initNewPsp():
	 *  - UMD: calls setFirmwareVersion before initNewPsp (PSF is read separate from BOOT.BIN).
	 *  - PBP: calls initNewPsp before setFirmwareVersion (PSF is embedded in PBP).
	 *  - ELF/PRX: only calls initNewPsp (doesn't have a PSF).
	 */
	public class Emulator : ThreadStart
	{

		private static Emulator instance;
		private static Processor processor;
		private static Clock clock;
		private static Scheduler scheduler;
		private bool moduleLoaded;
		private Thread mainThread;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		public static bool run_Renamed = false;
		public static bool pause = false;
		private static IMainGUI gui;
		private InstructionCounter instructionCounter;
		public static Logger log = Logger.getLogger("emu");
		private SceModule module;
		private int firmwareVersion = 999;
		private string[] bootModuleBlackList = new string[] {"Prometheus Loader"};

		public Emulator(IMainGUI gui)
		{
			Emulator.gui = gui;
			processor = new Processor();
			clock = new Clock();
			scheduler = Scheduler.Instance;

			moduleLoaded = false;
			mainThread = new Thread(this, "Emu");

			instance = this;
		}

		public virtual Thread MainThread
		{
			get
			{
				return mainThread;
			}
		}

		public static void exit()
		{
			if (DurationStatistics.collectStatistics)
			{
				log.info(TextureCache.Instance.statistics);
			}
			RendererExecutor.exit();
			VertexCache.Instance.exit();
			Compiler.exit();
			RuntimeContext.exit();
			Profiler.exit();
			GEProfiler.exit();
			BaseRenderer.exit();
			BasePrimitiveRenderer.exit();
			ExternalGE.exit();
			if (DurationStatistics.collectStatistics && Modules.ThreadManForUserModule.statistics != null && Modules.sceDisplayModule.statistics != null)
			{
				long totalMillis = Clock.milliTime();
				long displayMillis = Modules.sceDisplayModule.statistics.cumulatedTimeMillis;
				long idleCpuMillis = RuntimeContext.idleDuration.CpuDurationMillis;
				long compilationCpuMillis = Compiler.compileDuration.CpuDurationMillis;
				long cpuMillis = Modules.ThreadManForUserModule.statistics.allCpuMillis - compilationCpuMillis - idleCpuMillis;
				long cpuCycles = Modules.ThreadManForUserModule.statistics.allCycles;
				double totalSecs = totalMillis / 1000.0;
				double displaySecs = displayMillis / 1000.0;
				double cpuSecs = cpuMillis / 1000.0;
				if (totalSecs != 0)
				{
					log.info("Total execution time: " + string.Format("{0:F3}", totalSecs) + "s");
					log.info("     PSP CPU time: " + string.Format("{0:F3}", cpuSecs) + "s (" + string.Format("{0:F1}", cpuSecs / totalSecs * 100) + "%)");
					log.info("     Display time: " + string.Format("{0:F3}", displaySecs) + "s (" + string.Format("{0:F1}", displaySecs / totalSecs * 100) + "%)");
				}
				if (VideoEngine.Statistics != null)
				{
					long videoCalls = VideoEngine.Statistics.numberCalls;
					if (videoCalls != 0)
					{
						log.info("Elapsed time per frame: " + string.Format("{0:F3}", totalSecs / videoCalls) + "s:");
						log.info("    Display time: " + string.Format("{0:F3}", displaySecs / videoCalls));
						log.info("    PSP CPU time: " + string.Format("{0:F3}", cpuSecs / videoCalls) + " (" + (cpuCycles / videoCalls) + " instr)");
					}
					if (totalSecs != 0)
					{
						log.info("Display Speed: " + string.Format("{0:F2}", videoCalls / totalSecs) + " FPS");
					}
				}
				if (cpuSecs != 0)
				{
					log.info("PSP CPU Speed: " + string.Format("{0:F2}", cpuCycles / cpuSecs / 1000000.0) + "MHz (" + (long)(cpuCycles / cpuSecs) + " instructions per second)");
				}
			}
			SoundChannel.exit();
		}

		private bool isBootModuleBad(string name)
		{
			foreach (string moduleName in bootModuleBlackList)
			{
				if (name.Equals(moduleName))
				{
					return true;
				}
			}
			return false;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public pspsharp.HLE.kernel.types.SceModule load(String pspfilename, ByteBuffer f) throws java.io.IOException, GeneralJpcspException
		public virtual SceModule load(string pspfilename, ByteBuffer f)
		{
			return load(pspfilename, f, false);
		}

		private int LoadAddress
		{
			get
			{
				SysMemUserForUser.SysMemInfo testInfo = Modules.SysMemUserForUserModule.malloc(USER_PARTITION_ID, "test-LoadAddress", SysMemUserForUser.PSP_SMEM_Low, 0x100, 0);
				if (testInfo == null)
				{
					return MemoryMap.START_USERSPACE + 0x4000;
				}
    
				int lowestAddress = testInfo.addr;
				Modules.SysMemUserForUserModule.free(testInfo);
    
				return lowestAddress;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public pspsharp.HLE.kernel.types.SceModule load(String pspfilename, ByteBuffer f, boolean fromSyscall) throws java.io.IOException, GeneralJpcspException
		public virtual SceModule load(string pspfilename, ByteBuffer f, bool fromSyscall)
		{
			initNewPsp(fromSyscall);

			HLEModuleManager.Instance.loadAvailableFlash0Modules(fromSyscall);

			int loadAddress = LoadAddress;
			module = Loader.Instance.LoadModule(pspfilename, f, loadAddress, USER_PARTITION_ID, USER_PARTITION_ID, false, true, fromSyscall);

			if ((module.fileFormat & Loader.FORMAT_ELF) != Loader.FORMAT_ELF)
			{
				throw new GeneralJpcspException("File format not supported!");
			}
			if (isBootModuleBad(module.modname))
			{
				JpcspDialogManager.showError(null, java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp").getString("Emulator.strPrometheusLoader.text"));
			}

			moduleLoaded = true;
			initCpu(fromSyscall);

			// Delete breakpoints and reset to PC
			if (State.debugger != null)
			{
				State.debugger.resetDebugger();
			}

			// Update instruction counter dialog with the new app
			if (instructionCounter != null)
			{
				instructionCounter.Module = module;
			}

			return module;
		}

		private void initCpu(bool fromSyscall)
		{
			RuntimeContext.update();

			int entryAddr = module.entry_addr;
			if (Memory.isAddressGood(module.module_start_func))
			{
				if (module.module_start_func != entryAddr)
				{
					log.warn(string.Format("Using the module start function as module entry: 0x{0:X8} instead of 0x{1:X8}", module.module_start_func, entryAddr));
					entryAddr = module.module_start_func;
				}
			}

			HLEModuleManager.Instance.startModules(fromSyscall);
			Modules.ThreadManForUserModule.Initialise(module, entryAddr, module.attribute, module.pspfilename, module.modid, module.gp_value, fromSyscall);

			if (State.memoryViewer != null)
			{
				State.memoryViewer.RefreshMemory();
			}
		}

		public virtual void initNewPsp(bool fromSyscall)
		{
			moduleLoaded = false;

			HLEModuleManager.Instance.stopModules();
			NIDMapper.Instance.unloadAll();
			RuntimeContext.reset();

			if (!fromSyscall)
			{
				// Do not reset the profiler if we have been called from sceKernelLoadExec
				Profiler.reset();
				GEProfiler.reset();
				// Do not reset the clock if we have been called from sceKernelLoadExec
				Clock.reset();
			}

			Processor.reset();
			Scheduler.reset();

			Memory mem = Memory.Instance;
			if (!fromSyscall)
			{
				// Clear all memory, including VRAM.
				mem.Initialise();
			}
			else
			{
				// Clear all memory excepted VRAM.
				// E.g. screen is not cleared when executing syscall sceKernelLoadExec().
				mem.memset(MemoryMap.START_SCRATCHPAD, (sbyte) 0, MemoryMap.SIZE_SCRATCHPAD);
				mem.memset(MemoryMap.START_RAM, (sbyte) 0, MemoryMap.SIZE_RAM);
			}

			Battery.initialize();
			Wlan.initialize();
			SceModule.ResetAllocator();
			SceUidManager.reset();
			HLEUidObjectMapping.reset();
			ProOnlineNetworkAdapter.init();

			if (State.fileLogger != null)
			{
				State.fileLogger.resetLogging();
			}
			MemorySections.Instance.reset();

			HLEModuleManager.Instance.init();
			Managers.reset();
			Modules.SysMemUserForUserModule.start();
			Modules.SysMemUserForUserModule.FirmwareVersion = firmwareVersion;
			Modules.ThreadManForUserModule.start();
		}

		public override void run()
		{
			RuntimeContext.start();
			RuntimeContextLLE.start();
			GEProfiler.initialise();

			clock.resume();

			while (true)
			{
				if (pause)
				{
					clock.pause();
					try
					{
						lock (this)
						{
							while (pause)
							{
								Monitor.Wait(this);
							}
						}
					}
					catch (InterruptedException)
					{
						// Ignore exception
					}
					clock.resume();
				}

				if (RuntimeContext.CompilerEnabled)
				{
					RuntimeContext.run();
				}
				else
				{
					processor.step();
					Modules.sceGe_userModule.step();
					Modules.ThreadManForUserModule.step();
					scheduler.step();
					Modules.sceDisplayModule.step();

					if (State.debugger != null)
					{
						State.debugger.step();
					}
				}
			}

		}

		public virtual void RunEmu()
		{
			lock (this)
			{
				if (!moduleLoaded)
				{
					Emulator.log.debug("Nothing loaded, can't run...");
					gui.RefreshButtons();
					return;
				}
        
				if (pause)
				{
					pause = false;
					Monitor.PulseAll(this);
				}
				else if (!run_Renamed)
				{
					run_Renamed = true;
					mainThread.Start();
				}
        
				Modules.sceDisplayModule.GeDirty = true;
        
				gui.RefreshButtons();
				if (State.debugger != null)
				{
					State.debugger.RefreshButtons();
				}
			}
		}

		private static void PauseEmu(bool hasStatus, int status)
		{
			if (run_Renamed && !pause)
			{
				pause = true;

				if (hasStatus)
				{
					StepLogger.Status = status;
				}

				gui.RefreshButtons();

				if (State.debugger != null)
				{
					State.debugger.RefreshButtons();
					State.debugger.SafeRefreshDebugger(true);
				}

				if (State.memoryViewer != null)
				{
					State.memoryViewer.SafeRefreshMemory();
				}

				if (State.imageViewer != null)
				{
					State.imageViewer.SafeRefreshImage();
				}

				StepLogger.flush();
			}
		}

		public static void PauseEmu()
		{
			lock (typeof(Emulator))
			{
				PauseEmu(false, 0);
			}
		}
		public const int EMU_STATUS_OK = 0x00;
		public const int EMU_STATUS_UNKNOWN = unchecked((int)0xFFFFFFFF);
		public const int EMU_STATUS_WDT_IDLE = 0x01;
		public const int EMU_STATUS_WDT_HOG = 0x02;
		public const int EMU_STATUS_WDT_ANY = EMU_STATUS_WDT_IDLE | EMU_STATUS_WDT_HOG;
		public const int EMU_STATUS_MEM_READ = 0x04;
		public const int EMU_STATUS_MEM_WRITE = 0x08;
		public const int EMU_STATUS_MEM_ANY = EMU_STATUS_MEM_READ | EMU_STATUS_MEM_WRITE;
		public const int EMU_STATUS_BREAKPOINT = 0x10;
		public const int EMU_STATUS_UNIMPLEMENTED = 0x20;
		public const int EMU_STATUS_PAUSE = 0x40;
		public const int EMU_STATUS_JUMPSELF = 0x80;
		public const int EMU_STATUS_BREAK = 0x100;
		public const int EMU_STATUS_HALT = 0x200;

		public static void PauseEmuWithStatus(int status)
		{
			lock (typeof(Emulator))
			{
				PauseEmu(true, status);
			}
		}

		public static string FpsTitle
		{
			set
			{
				gui.MainTitle = value;
			}
		}

		public static Processor Processor
		{
			get
			{
				return processor;
			}
		}

		public static Memory Memory
		{
			get
			{
				return Memory.Instance;
			}
		}

		public static Clock Clock
		{
			get
			{
				return clock;
			}
			set
			{
				Emulator.clock = value;
			}
		}


		public static Scheduler Scheduler
		{
			get
			{
				return scheduler;
			}
		}

		public static IMainGUI MainGUI
		{
			get
			{
				return gui;
			}
		}

		public static Emulator Instance
		{
			get
			{
				return instance;
			}
		}

		public virtual InstructionCounter InstructionCounter
		{
			set
			{
				this.instructionCounter = value;
				value.Module = module;
			}
		}

		public virtual int getFirmwareVersion()
		{
			return firmwareVersion;
		}

		/// <param name="firmwareVersion"> : in this format: ABB, where A = major and B =
		/// minor, for example 271 </param>
		public virtual void setFirmwareVersion(int firmwareVersion)
		{
			this.firmwareVersion = firmwareVersion;

			Modules.SysMemUserForUserModule.FirmwareVersion = this.firmwareVersion;
			RuntimeContext.FirmwareVersion = firmwareVersion;
		}

		/// <param name="firmwareVersion"> : in this format: "A.BB", where A = major and B =
		/// minor, for example "2.71" </param>
		public virtual void setFirmwareVersion(string firmwareVersion)
		{
			setFirmwareVersion(HLEModuleManager.psfFirmwareVersionToInt(firmwareVersion));
		}

		public static void setVariableSpeedClock(int numerator, int denominator)
		{
			if (Clock is VariableSpeedClock)
			{
				// Update the speed of the current variable speed clock
				((VariableSpeedClock) Clock).setSpeed(numerator, denominator);
			}
			else if (numerator != 1 || denominator != 1)
			{
				// Change the clock to a variable speed clock with the given speed
				VariableSpeedClock variableSpeedClock = new VariableSpeedClock(clock, numerator, denominator);
				Clock = variableSpeedClock;
			}
		}

		public virtual bool ModuleLoaded
		{
			set
			{
				this.moduleLoaded = value;
			}
		}
	}

}