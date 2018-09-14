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
	using MemoryViewer = pspsharp.Debugger.MemoryViewer;
	using DisassemblerFrame = pspsharp.Debugger.DisassemblerModule.DisassemblerFrame;
	using ElfHeaderInfo = pspsharp.Debugger.ElfHeaderInfo;
	using FileLoggerFrame = pspsharp.Debugger.FileLogger.FileLoggerFrame;
	using ImageViewer = pspsharp.Debugger.ImageViewer;
	using InstructionCounter = pspsharp.Debugger.InstructionCounter;
	using CheatsGUI = pspsharp.GUI.CheatsGUI;
	using LogGUI = pspsharp.GUI.LogGUI;
	using SettingsGUI = pspsharp.GUI.SettingsGUI;
	using LogWindow = pspsharp.log.LogWindow;

	/// 
	/// <summary>
	/// @author hli
	/// </summary>
	public class State : pspsharp.HLE.Modules
	{

		public static readonly Memory memory;
		public static readonly Controller controller;
		// additional frames
		public static DisassemblerFrame debugger;
		public static MemoryViewer memoryViewer;
		public static ImageViewer imageViewer;
		public static FileLoggerFrame fileLogger;
		public static CheatsGUI cheatsGUI;
		public static LogGUI logGUI;
		public static ElfHeaderInfo elfHeader;
		public static SettingsGUI settingsGUI;
		public static LogWindow logWindow;
		public static InstructionCounter instructionCounter;
		// disc related
		public static string discId;
		public static string title;
		// The UMD ID extracted from the UMD_DATA.BIN file
		public static string umdId;
		// make sure these are valid filenames because it gets used by the screenshot system
		public const string DISCID_UNKNOWN_NOTHING_LOADED = "[unknown, nothing loaded]";
		public const string DISCID_UNKNOWN_FILE = "[unknown, file]";
		public const string DISCID_UNKNOWN_UMD = "[unknown, umd]";
		public static bool captureGeNextFrame;
		public static bool replayGeNextFrame;
		public static bool exportGeNextFrame;
		public static bool exportGeOnlyVisibleElements;

		static State()
		{
			memory = Memory.Instance;
			controller = Controller.Instance;
			discId = DISCID_UNKNOWN_NOTHING_LOADED;
			captureGeNextFrame = false;
			replayGeNextFrame = false;
			exportGeNextFrame = false;
		}
	}

}