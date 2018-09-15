using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;

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
//	import static pspsharp.Allegrex.compiler.RuntimeContext.setLog4jMDC;


	using Profiler = pspsharp.Allegrex.compiler.Profiler;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using RuntimeContextLLE = pspsharp.Allegrex.compiler.RuntimeContextLLE;
	using AutoTestsRunner = pspsharp.autotests.AutoTestsRunner;
	using AES128 = pspsharp.crypto.AES128;
	using PreDecrypt = pspsharp.crypto.PreDecrypt;
	using ElfHeaderInfo = pspsharp.Debugger.ElfHeaderInfo;
	using ImageViewer = pspsharp.Debugger.ImageViewer;
	using InstructionCounter = pspsharp.Debugger.InstructionCounter;
	using MemoryViewer = pspsharp.Debugger.MemoryViewer;
	using StepLogger = pspsharp.Debugger.StepLogger;
	using DisassemblerFrame = pspsharp.Debugger.DisassemblerModule.DisassemblerFrame;
	using VfpuFrame = pspsharp.Debugger.DisassemblerModule.VfpuFrame;
	using CheatsGUI = pspsharp.GUI.CheatsGUI;
	using IMainGUI = pspsharp.GUI.IMainGUI;
	using RecentElement = pspsharp.GUI.RecentElement;
	using SettingsGUI = pspsharp.GUI.SettingsGUI;
	using ControlsGUI = pspsharp.GUI.ControlsGUI;
	using LogGUI = pspsharp.GUI.LogGUI;
	using UmdBrowser = pspsharp.GUI.UmdBrowser;
	using UmdVideoPlayer = pspsharp.GUI.UmdVideoPlayer;
	using HLEModuleManager = pspsharp.HLE.HLEModuleManager;
	using Modules = pspsharp.HLE.Modules;
	using LocalVirtualFile = pspsharp.HLE.VFS.local.LocalVirtualFile;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using IoFileMgrForUser = pspsharp.HLE.modules.IoFileMgrForUser;
	using reboot = pspsharp.HLE.modules.reboot;
	using sceDisplay = pspsharp.HLE.modules.sceDisplay;
	using sceUtility = pspsharp.HLE.modules.sceUtility;
	using SeekableDataInput = pspsharp.filesystems.SeekableDataInput;
	using SeekableRandomFile = pspsharp.filesystems.SeekableRandomFile;
	using UmdIsoFile = pspsharp.filesystems.umdiso.UmdIsoFile;
	using UmdIsoReader = pspsharp.filesystems.umdiso.UmdIsoReader;
	using PBP = pspsharp.format.PBP;
	using PSF = pspsharp.format.PSF;
	using GEProfiler = pspsharp.graphics.GEProfiler;
	using VideoEngine = pspsharp.graphics.VideoEngine;
	using Audio = pspsharp.hardware.Audio;
	using MemoryStick = pspsharp.hardware.MemoryStick;
	using Screen = pspsharp.hardware.Screen;
	using LogWindow = pspsharp.log.LogWindow;
	using LoggingOutputStream = pspsharp.log.LoggingOutputStream;
	using MMIOHandlerUmd = pspsharp.memory.mmio.MMIOHandlerUmd;
	using ProOnlineNetworkAdapter = pspsharp.network.proonline.ProOnlineNetworkAdapter;
	using HTTPServer = pspsharp.remote.HTTPServer;
	using Settings = pspsharp.settings.Settings;
	using pspsharp.util;

	using Level = org.apache.log4j.Level;
	//using Logger = org.apache.log4j.Logger;
	using DOMConfigurator = org.apache.log4j.xml.DOMConfigurator;

	using LookAndFeelFactory = com.jidesoft.plaf.LookAndFeelFactory;

	using FileLoggerFrame = pspsharp.Debugger.FileLogger.FileLoggerFrame;
	using Model = pspsharp.hardware.Model;

	/// 
	/// <summary>
	/// @author shadow
	/// </summary>
	public class MainGUI : javax.swing.JFrame, KeyListener, ComponentListener, MouseListener, IMainGUI
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			recentUMD = new LinkedList<RecentElement>();
			recentFile = new LinkedList<RecentElement>();
		}

		static MainGUI()
		{
			LWJGLFixer.fixOnce();
		}

		private const long serialVersionUID = -3647025845406693230L;
		private static Logger log = Emulator.log;
		public const int MAX_RECENT = 10;
		internal Emulator emulator;
		internal UmdBrowser umdbrowser;
		internal UmdVideoPlayer umdvideoplayer;
		internal InstructionCounter instructioncounter;
		internal File loadedFile;
		internal bool umdLoaded;
		internal bool useFullscreen;
		internal JPopupMenu fullScreenMenu;
		private IList<RecentElement> recentUMD;
		private IList<RecentElement> recentFile;
		private static readonly string[] userDir = new string[] {"ms0/PSP/SAVEDATA", "ms0/PSP/GAME", "tmp"};
		private const string logConfigurationSettingLeft = "    %1$-40s %3$c%2$s%4$c";
		private const string logConfigurationSettingRight = "    %3$c%2$s%4$c %1$s";
		private const string logConfigurationSettingLeftPatch = "    %1$-40s %3$c%2$s%4$c (%5$s)";
		private const string logConfigurationSettingRightPatch = "    %3$c%2$s%4$c %1$s (%5$s)";
		public const int displayModeBitDepth = 32;
		public const int preferredDisplayModeRefreshRate = 60; // Preferred refresh rate if 60Hz
		private DisplayMode displayMode;
		private SetLocationThread setLocationThread;
		private JComponent fillerLeft;
		private JComponent fillerRight;
		private JComponent fillerTop;
		private JComponent fillerBottom;
		private static bool jideInitialized;
		// map to hold action listeners for menu entries in fullscreen mode
		private Dictionary<KeyStroke, ActionListener[]> actionListenerMap;
		private bool doUmdBuffering = false;
		private bool runFromVsh = false;

		public virtual DisplayMode DisplayMode
		{
			get
			{
				return displayMode;
			}
		}

		/// <summary>
		/// Creates new form MainGUI
		/// </summary>
		public MainGUI()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			System.Out = new PrintStream(new LoggingOutputStream(Logger.getLogger("emu"), Level.INFO));

			actionListenerMap = new Dictionary<KeyStroke, ActionListener[]>();

			// create log window in a local variable - see explanation further down
			LogWindow logwin = new LogWindow();

			// create needed user directories
			foreach (string dirName in userDir)
			{
				File dir = new File(dirName);
				if (!dir.exists())
				{
					dir.mkdirs();
				}
			}

			emulator = new Emulator(this);
			Screen.start();

			// must be done after initialising the Emulator class as State initialises
			// its elements indirectly via getting the pointer to MainGUI by means
			// of the Emulator class...
			State.logWindow = logwin;

			// next two lines are for overlay menus over joglcanvas
			JPopupMenu.DefaultLightWeightPopupEnabled = false;
			ToolTipManager.sharedInstance().LightWeightPopupEnabled = false;

			useFullscreen = Settings.Instance.readBool("gui.fullscreen");
			if (useFullscreen && !Displayable)
			{
				Undecorated = true;
				setLocation(0, 0);
				Size = FullScreenDimension;
				PreferredSize = FullScreenDimension;
			}

			string resolution = Settings.Instance.readString("emu.graphics.resolution");
			if (!string.ReferenceEquals(resolution, null) && resolution.Contains("x"))
			{
				int width = int.Parse(resolution.Split("x", true)[0]);
				int heigth = int.Parse(resolution.Split("x", true)[1]);
				changeScreenResolution(width, heigth);
			}

			initJide();
			createComponents();

			onUmdChange();

			Title = MetaInformation.FULL_NAME;

			// add glcanvas to frame and pack frame to get the canvas size
			ContentPane.add(Modules.sceDisplayModule.Canvas, java.awt.BorderLayout.CENTER);
			Modules.sceDisplayModule.Canvas.addKeyListener(this);
			Modules.sceDisplayModule.Canvas.addMouseListener(this);
			addComponentListener(this);
			pack();

			// Check if any plugins are available.
			xbrzCheck.Enabled = false;
			string path = System.getProperty("java.library.path");
			if (!string.ReferenceEquals(path, null) && path.Length > 0)
			{
				File plugins = new File(path);
				string[] pluginList = plugins.list();
				if (pluginList != null)
				{
					foreach (string list in pluginList)
					{
						if (list.Contains("XBRZ4JPCSP"))
						{
							xbrzCheck.Enabled = true;
						}
					}
				}
			}

			SwingUtilities.invokeLater(() =>
			{
			// let the layout manager settle before setting the minimum size
			Modules.sceDisplayModule.setDisplayMinimumSize();

			// as the console log window position depends on the main
			// window's size run this here
			if (Settings.Instance.readBool("gui.snapLogwindow"))
			{
				updateConsoleWinPosition();
			}
			});

			WindowPropSaver.loadWindowProperties(this);

			try
			{
				Image iconImage = (new ImageIcon(ClassLoader.SystemClassLoader.getResource("pspsharp/icon.png"))).Image;
				this.IconImages = Array.asList(iconImage.getScaledInstance(16, 16, Image.SCALE_SMOOTH), iconImage.getScaledInstance(32, 32, Image.SCALE_SMOOTH), iconImage);
			}
			catch (Exception t)
			{
				Console.WriteLine(t.ToString());
				Console.Write(t.StackTrace);
			}
		}

		private Dimension getDimensionFromDisplay(int width, int height)
		{
			Insets insets = Insets;
			Dimension dimension = new Dimension(width + insets.left + insets.right, height + insets.top + insets.bottom);
			return dimension;
		}

		public virtual void setDisplayMinimumSize(int width, int height)
		{
			Dimension dim = getDimensionFromDisplay(width, height);
			dim.height += mainToolBar.Height;
			dim.height += MenuBar.Height;
			MinimumSize = dim;
		}

		public virtual void setDisplaySize(int width, int height)
		{
			Size = getDimensionFromDisplay(width, height);
		}

		private void initJide()
		{
			if (!jideInitialized)
			{
				LookAndFeelFactory.installJideExtension(LookAndFeelFactory.VSNET_STYLE_WITHOUT_MENU);
				jideInitialized = true;
			}
		}

		/// <summary>
		/// This method is called from within the constructor to initialize the form.
		/// WARNING: Do NOT modify this code. The content of this method is always
		/// regenerated by the Form Editor.
		/// </summary>
		// <editor-fold defaultstate="collapsed" desc="Generated Code">//GEN-BEGIN:initComponents
		private void initComponents()
		{

			filtersGroup = new javax.swing.ButtonGroup();
			resGroup = new javax.swing.ButtonGroup();
			frameSkipGroup = new javax.swing.ButtonGroup();
			clockSpeedGroup = new javax.swing.ButtonGroup();
			mainToolBar = new javax.swing.JToolBar();
			RunButton = new javax.swing.JToggleButton();
			PauseButton = new javax.swing.JToggleButton();
			ResetButton = new javax.swing.JButton();
			MenuBar = new javax.swing.JMenuBar();
			FileMenu = new javax.swing.JMenu();
			openUmd = new javax.swing.JMenuItem();
			OpenFile = new javax.swing.JMenuItem();
			RecentMenu = new javax.swing.JMenu();
			switchUmd = new javax.swing.JMenuItem();
			ejectMs = new javax.swing.JMenuItem();
			jSeparator2 = new javax.swing.JSeparator();
			SaveSnap = new javax.swing.JMenuItem();
			LoadSnap = new javax.swing.JMenuItem();
			ExportMenu = new javax.swing.JMenu();
			ExportVisibleElements = new javax.swing.JMenuItem();
			ExportAllElements = new javax.swing.JMenuItem();
			jSeparator1 = new javax.swing.JSeparator();
			ExitEmu = new javax.swing.JMenuItem();
			OptionsMenu = new javax.swing.JMenu();
			VideoOpt = new javax.swing.JMenu();
			ResizeMenu = new javax.swing.JMenu();
			oneTimeResize = new javax.swing.JCheckBoxMenuItem();
			twoTimesResize = new javax.swing.JCheckBoxMenuItem();
			threeTimesResize = new javax.swing.JCheckBoxMenuItem();
			FiltersMenu = new javax.swing.JMenu();
			noneCheck = new javax.swing.JCheckBoxMenuItem();
			anisotropicCheck = new javax.swing.JCheckBoxMenuItem();
			FrameSkipMenu = new javax.swing.JMenu();
			FrameSkipNone = new javax.swing.JCheckBoxMenuItem();
			FPS5 = new javax.swing.JCheckBoxMenuItem();
			FPS10 = new javax.swing.JCheckBoxMenuItem();
			FPS15 = new javax.swing.JCheckBoxMenuItem();
			FPS20 = new javax.swing.JCheckBoxMenuItem();
			FPS30 = new javax.swing.JCheckBoxMenuItem();
			FPS60 = new javax.swing.JCheckBoxMenuItem();
			ShotItem = new javax.swing.JMenuItem();
			RotateItem = new javax.swing.JMenuItem();
			AudioOpt = new javax.swing.JMenu();
			MuteOpt = new javax.swing.JCheckBoxMenuItem();
			ClockSpeedOpt = new javax.swing.JMenu();
			ClockSpeed50 = new javax.swing.JCheckBoxMenuItem();
			ClockSpeed75 = new javax.swing.JCheckBoxMenuItem();
			ClockSpeedNormal = new javax.swing.JCheckBoxMenuItem();
			ClockSpeed150 = new javax.swing.JCheckBoxMenuItem();
			ClockSpeed200 = new javax.swing.JCheckBoxMenuItem();
			ClockSpeed300 = new javax.swing.JCheckBoxMenuItem();
			ControlsConf = new javax.swing.JMenuItem();
			ConfigMenu = new javax.swing.JMenuItem();
			DebugMenu = new javax.swing.JMenu();
			ToolsSubMenu = new javax.swing.JMenu();
			LoggerMenu = new javax.swing.JMenu();
			ToggleLogger = new javax.swing.JCheckBoxMenuItem();
			CustomLogger = new javax.swing.JMenuItem();
			EnterDebugger = new javax.swing.JMenuItem();
			EnterMemoryViewer = new javax.swing.JMenuItem();
			EnterImageViewer = new javax.swing.JMenuItem();
			VfpuRegisters = new javax.swing.JMenuItem();
			ElfHeaderViewer = new javax.swing.JMenuItem();
			FileLog = new javax.swing.JMenuItem();
			InstructionCounter = new javax.swing.JMenuItem();
			DumpIso = new javax.swing.JMenuItem();
			ResetProfiler = new javax.swing.JMenuItem();
			ClearTextureCache = new javax.swing.JMenuItem();
			ClearVertexCache = new javax.swing.JMenuItem();
			ExportISOFile = new javax.swing.JMenuItem();
			CheatsMenu = new javax.swing.JMenu();
			cwcheat = new javax.swing.JMenuItem();
			LanguageMenu = new javax.swing.JMenu();
			SystemLocale = new javax.swing.JMenuItem();
			EnglishGB = new javax.swing.JMenuItem();
			EnglishUS = new javax.swing.JMenuItem();
			French = new javax.swing.JMenuItem();
			German = new javax.swing.JMenuItem();
			Lithuanian = new javax.swing.JMenuItem();
			Spanish = new javax.swing.JMenuItem();
			Catalan = new javax.swing.JMenuItem();
			Portuguese = new javax.swing.JMenuItem();
			PortugueseBR = new javax.swing.JMenuItem();
			Japanese = new javax.swing.JMenuItem();
			Russian = new javax.swing.JMenuItem();
			Polish = new javax.swing.JMenuItem();
			ChinesePRC = new javax.swing.JMenuItem();
			ChineseTW = new javax.swing.JMenuItem();
			Italian = new javax.swing.JMenuItem();
			Greek = new javax.swing.JMenuItem();
			PluginsMenu = new javax.swing.JMenu();
			xbrzCheck = new javax.swing.JCheckBoxMenuItem();
			HelpMenu = new javax.swing.JMenu();
			About = new javax.swing.JMenuItem();

			DefaultCloseOperation = javax.swing.WindowConstants.EXIT_ON_CLOSE;
			Cursor = new java.awt.Cursor(java.awt.Cursor.DEFAULT_CURSOR);
			Foreground = java.awt.Color.white;
			addWindowListener(new WindowAdapterAnonymousInnerClass(this));
			addComponentListener(new ComponentAdapterAnonymousInnerClass(this));

			mainToolBar.Floatable = false;
			mainToolBar.Rollover = true;

			RunButton.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/PlayIcon.png")); // NOI18N
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			RunButton.Text = bundle.getString("MainGUI.RunButton.text"); // NOI18N
			RunButton.Focusable = false;
			RunButton.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			RunButton.IconTextGap = 2;
			RunButton.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			RunButton.addActionListener(new ActionListenerAnonymousInnerClass(this));
			mainToolBar.add(RunButton);

			PauseButton.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/PauseIcon.png")); // NOI18N
			PauseButton.Text = bundle.getString("MainGUI.PauseButton.text"); // NOI18N
			PauseButton.Focusable = false;
			PauseButton.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			PauseButton.IconTextGap = 2;
			PauseButton.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			PauseButton.addActionListener(new ActionListenerAnonymousInnerClass2(this));
			mainToolBar.add(PauseButton);

			ResetButton.setIcon(new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/StopIcon.png"))); // NOI18N
			ResetButton.setText(bundle.getString("MainGUI.ResetButton.text")); // NOI18N
			ResetButton.setFocusable(false);
			ResetButton.setHorizontalTextPosition(javax.swing.SwingConstants.RIGHT);
			ResetButton.setIconTextGap(2);
			ResetButton.setVerticalTextPosition(javax.swing.SwingConstants.BOTTOM);
			ResetButton.addActionListener(new ActionListenerAnonymousInnerClass3(this));
			mainToolBar.add(ResetButton);

			ContentPane.add(mainToolBar, java.awt.BorderLayout.NORTH);

			FileMenu.Text = bundle.getString("MainGUI.FileMenu.text"); // NOI18N

			openUmd.Accelerator = javax.swing.KeyStroke.getKeyStroke(KeyEvent.VK_O, java.awt.@event.InputEvent.CTRL_MASK);
			openUmd.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/LoadUmdIcon.png")); // NOI18N
			openUmd.Text = bundle.getString("MainGUI.openUmd.text"); // NOI18N
			openUmd.addActionListener(new ActionListenerAnonymousInnerClass4(this));
			FileMenu.add(openUmd);

			OpenFile.Accelerator = javax.swing.KeyStroke.getKeyStroke(KeyEvent.VK_O, java.awt.@event.InputEvent.ALT_MASK);
			OpenFile.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/LoadFileIcon.png")); // NOI18N
			OpenFile.Text = bundle.getString("MainGUI.OpenFile.text"); // NOI18N
			OpenFile.addActionListener(new ActionListenerAnonymousInnerClass5(this));
			FileMenu.add(OpenFile);

			RecentMenu.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/RecentIcon.png")); // NOI18N
			RecentMenu.Text = bundle.getString("MainGUI.RecentMenu.text"); // NOI18N
			FileMenu.add(RecentMenu);

			switchUmd.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/LoadUmdIcon.png")); // NOI18N
			switchUmd.Text = bundle.getString("MainGUI.switchUmd.text"); // NOI18N
			switchUmd.addActionListener(new ActionListenerAnonymousInnerClass6(this));
			FileMenu.add(switchUmd);

			ejectMs.Text = bundle.getString("MainGUI.ejectMs.text"); // NOI18N
			ejectMs.addActionListener(new ActionListenerAnonymousInnerClass7(this));
			FileMenu.add(ejectMs);
			FileMenu.add(jSeparator2);

			SaveSnap.Accelerator = javax.swing.KeyStroke.getKeyStroke(KeyEvent.VK_S, java.awt.@event.InputEvent.SHIFT_MASK);
			SaveSnap.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/SaveStateIcon.png")); // NOI18N
			SaveSnap.Text = bundle.getString("MainGUI.SaveSnap.text"); // NOI18N
			SaveSnap.addActionListener(new ActionListenerAnonymousInnerClass8(this));
			FileMenu.add(SaveSnap);

			LoadSnap.Accelerator = javax.swing.KeyStroke.getKeyStroke(KeyEvent.VK_L, java.awt.@event.InputEvent.SHIFT_MASK);
			LoadSnap.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/LoadStateIcon.png")); // NOI18N
			LoadSnap.Text = bundle.getString("MainGUI.LoadSnap.text"); // NOI18N
			LoadSnap.addActionListener(new ActionListenerAnonymousInnerClass9(this));
			FileMenu.add(LoadSnap);

			ExportMenu.Text = bundle.getString("MainGUI.ExportMenu.text"); // NOI18N

			ExportVisibleElements.Text = bundle.getString("MainGUI.ExportVisibleElements.text"); // NOI18N
			ExportVisibleElements.addActionListener(new ActionListenerAnonymousInnerClass10(this));
			ExportMenu.add(ExportVisibleElements);

			ExportAllElements.Text = bundle.getString("MainGUI.ExportAllElements.text"); // NOI18N
			ExportAllElements.addActionListener(new ActionListenerAnonymousInnerClass11(this));
			ExportMenu.add(ExportAllElements);

			FileMenu.add(ExportMenu);
			FileMenu.add(jSeparator1);

			ExitEmu.Accelerator = javax.swing.KeyStroke.getKeyStroke(KeyEvent.VK_E, java.awt.@event.InputEvent.CTRL_MASK);
			ExitEmu.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/CloseIcon.png")); // NOI18N
			ExitEmu.Text = bundle.getString("MainGUI.ExitEmu.text"); // NOI18N
			ExitEmu.addActionListener(new ActionListenerAnonymousInnerClass12(this));
			FileMenu.add(ExitEmu);

			MenuBar.add(FileMenu);

			OptionsMenu.Text = bundle.getString("MainGUI.OptionsMenu.text"); // NOI18N

			VideoOpt.Text = bundle.getString("MainGUI.VideoOpt.text"); // NOI18N

			ResizeMenu.Text = bundle.getString("MainGUI.ResizeMenu.text"); // NOI18N

			resGroup.add(oneTimeResize);
			oneTimeResize.Selected = true;
			oneTimeResize.Text = "1x"; // NOI18N
			oneTimeResize.addActionListener(new ActionListenerAnonymousInnerClass13(this));
			ResizeMenu.add(oneTimeResize);

			resGroup.add(twoTimesResize);
			twoTimesResize.Text = "2x"; // NOI18N
			twoTimesResize.addActionListener(new ActionListenerAnonymousInnerClass14(this));
			ResizeMenu.add(twoTimesResize);

			resGroup.add(threeTimesResize);
			threeTimesResize.Text = "3x"; // NOI18N
			threeTimesResize.addActionListener(new ActionListenerAnonymousInnerClass15(this));
			ResizeMenu.add(threeTimesResize);

			VideoOpt.add(ResizeMenu);

			FiltersMenu.Text = bundle.getString("MainGUI.FiltersMenu.text"); // NOI18N

			filtersGroup.add(noneCheck);
			noneCheck.Selected = true;
			noneCheck.Text = bundle.getString("MainGUI.noneCheck.text"); // NOI18N
			noneCheck.addActionListener(new ActionListenerAnonymousInnerClass16(this));
			FiltersMenu.add(noneCheck);

			filtersGroup.add(anisotropicCheck);
			anisotropicCheck.Selected = Settings.Instance.readBool("emu.graphics.filters.anisotropic");
			anisotropicCheck.Text = bundle.getString("MainGUI.anisotropicCheck.text"); // NOI18N
			anisotropicCheck.addActionListener(new ActionListenerAnonymousInnerClass17(this));
			FiltersMenu.add(anisotropicCheck);

			VideoOpt.add(FiltersMenu);

			FrameSkipMenu.Text = bundle.getString("MainGUI.FrameSkipMenu.text"); // NOI18N

			frameSkipGroup.add(FrameSkipNone);
			FrameSkipNone.Selected = true;
			FrameSkipNone.Text = bundle.getString("MainGUI.FrameSkipNone.text"); // NOI18N
			FrameSkipNone.addActionListener(new ActionListenerAnonymousInnerClass18(this));
			FrameSkipMenu.add(FrameSkipNone);

			frameSkipGroup.add(FPS5);
			FPS5.Text = "5 FPS"; // NOI18N
			FPS5.addActionListener(new ActionListenerAnonymousInnerClass19(this));
			FrameSkipMenu.add(FPS5);

			frameSkipGroup.add(FPS10);
			FPS10.Text = "10 FPS"; // NOI18N
			FPS10.addActionListener(new ActionListenerAnonymousInnerClass20(this));
			FrameSkipMenu.add(FPS10);

			frameSkipGroup.add(FPS15);
			FPS15.Text = "15 FPS"; // NOI18N
			FPS15.addActionListener(new ActionListenerAnonymousInnerClass21(this));
			FrameSkipMenu.add(FPS15);

			frameSkipGroup.add(FPS20);
			FPS20.Text = "20 FPS"; // NOI18N
			FPS20.addActionListener(new ActionListenerAnonymousInnerClass22(this));
			FrameSkipMenu.add(FPS20);

			frameSkipGroup.add(FPS30);
			FPS30.Text = "30 FPS"; // NOI18N
			FPS30.addActionListener(new ActionListenerAnonymousInnerClass23(this));
			FrameSkipMenu.add(FPS30);

			frameSkipGroup.add(FPS60);
			FPS60.Text = "60 FPS"; // NOI18N
			FPS60.addActionListener(new ActionListenerAnonymousInnerClass24(this));
			FrameSkipMenu.add(FPS60);

			VideoOpt.add(FrameSkipMenu);

			ShotItem.Accelerator = javax.swing.KeyStroke.getKeyStroke(KeyEvent.VK_F5, 0);
			ShotItem.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/ScreenshotIcon.png")); // NOI18N
			ShotItem.Text = bundle.getString("MainGUI.ShotItem.text"); // NOI18N
			ShotItem.addActionListener(new ActionListenerAnonymousInnerClass25(this));
			VideoOpt.add(ShotItem);

			RotateItem.Accelerator = javax.swing.KeyStroke.getKeyStroke(KeyEvent.VK_F6, 0);
			RotateItem.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/RotateIcon.png")); // NOI18N
			RotateItem.Text = bundle.getString("MainGUI.RotateItem.text"); // NOI18N
			RotateItem.addActionListener(new ActionListenerAnonymousInnerClass26(this));
			VideoOpt.add(RotateItem);

			OptionsMenu.add(VideoOpt);

			AudioOpt.Text = bundle.getString("MainGUI.AudioOpt.text"); // NOI18N

			MuteOpt.Accelerator = javax.swing.KeyStroke.getKeyStroke(KeyEvent.VK_M, java.awt.@event.InputEvent.SHIFT_MASK);
			MuteOpt.Text = bundle.getString("MainGUI.MuteOpt.text"); // NOI18N
			MuteOpt.addActionListener(new ActionListenerAnonymousInnerClass27(this));
			AudioOpt.add(MuteOpt);

			OptionsMenu.add(AudioOpt);

			ClockSpeedOpt.Text = bundle.getString("MainGUI.ClockSpeedOpt.text"); // NOI18N

			clockSpeedGroup.add(ClockSpeed50);
			ClockSpeed50.Text = bundle.getString("MainGUI.ClockSpeed50.text"); // NOI18N
			ClockSpeed50.addActionListener(new ActionListenerAnonymousInnerClass28(this));
			ClockSpeedOpt.add(ClockSpeed50);

			clockSpeedGroup.add(ClockSpeed75);
			ClockSpeed75.Text = bundle.getString("MainGUI.ClockSpeed75.text"); // NOI18N
			ClockSpeed75.addActionListener(new ActionListenerAnonymousInnerClass29(this));
			ClockSpeedOpt.add(ClockSpeed75);

			clockSpeedGroup.add(ClockSpeedNormal);
			ClockSpeedNormal.Selected = true;
			ClockSpeedNormal.Text = bundle.getString("MainGUI.ClockSpeedNormal.text"); // NOI18N
			ClockSpeedNormal.addActionListener(new ActionListenerAnonymousInnerClass30(this));
			ClockSpeedOpt.add(ClockSpeedNormal);

			clockSpeedGroup.add(ClockSpeed150);
			ClockSpeed150.Text = bundle.getString("MainGUI.ClockSpeed150.text"); // NOI18N
			ClockSpeed150.addActionListener(new ActionListenerAnonymousInnerClass31(this));
			ClockSpeedOpt.add(ClockSpeed150);

			clockSpeedGroup.add(ClockSpeed200);
			ClockSpeed200.Text = bundle.getString("MainGUI.ClockSpeed200.text"); // NOI18N
			ClockSpeed200.addActionListener(new ActionListenerAnonymousInnerClass32(this));
			ClockSpeedOpt.add(ClockSpeed200);

			clockSpeedGroup.add(ClockSpeed300);
			ClockSpeed300.Text = bundle.getString("MainGUI.ClockSpeed300.text"); // NOI18N
			ClockSpeed300.addActionListener(new ActionListenerAnonymousInnerClass33(this));
			ClockSpeedOpt.add(ClockSpeed300);

			OptionsMenu.add(ClockSpeedOpt);

			ControlsConf.Accelerator = javax.swing.KeyStroke.getKeyStroke(KeyEvent.VK_F11, 0);
			ControlsConf.Text = bundle.getString("MainGUI.ControlsConf.text"); // NOI18N
			ControlsConf.addActionListener(new ActionListenerAnonymousInnerClass34(this));
			OptionsMenu.add(ControlsConf);

			ConfigMenu.Accelerator = javax.swing.KeyStroke.getKeyStroke(KeyEvent.VK_F12, 0);
			ConfigMenu.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/SettingsIcon.png")); // NOI18N
			ConfigMenu.Text = bundle.getString("MainGUI.ConfigMenu.text"); // NOI18N
			ConfigMenu.addActionListener(new ActionListenerAnonymousInnerClass35(this));
			OptionsMenu.add(ConfigMenu);

			MenuBar.add(OptionsMenu);

			DebugMenu.Text = bundle.getString("MainGUI.DebugMenu.text"); // NOI18N

			ToolsSubMenu.Text = bundle.getString("MainGUI.ToolsSubMenu.text"); // NOI18N

			LoggerMenu.Text = bundle.getString("ConsoleWindow.title"); // NOI18N

			ToggleLogger.Text = bundle.getString("MainGUI.ToggleLogger.text"); // NOI18N
			ToggleLogger.addActionListener(new ActionListenerAnonymousInnerClass36(this));
			LoggerMenu.add(ToggleLogger);

			CustomLogger.Text = bundle.getString("MainGUI.CustomLogger.text"); // NOI18N
			CustomLogger.addActionListener(new ActionListenerAnonymousInnerClass37(this));
			LoggerMenu.add(CustomLogger);

			ToolsSubMenu.add(LoggerMenu);

			EnterDebugger.Text = bundle.getString("DisassemblerFrame.title"); // NOI18N
			EnterDebugger.addActionListener(new ActionListenerAnonymousInnerClass38(this));
			ToolsSubMenu.add(EnterDebugger);

			EnterMemoryViewer.Text = bundle.getString("MemoryViewer.title"); // NOI18N
			EnterMemoryViewer.addActionListener(new ActionListenerAnonymousInnerClass39(this));
			ToolsSubMenu.add(EnterMemoryViewer);

			EnterImageViewer.Text = bundle.getString("ImageViewer.title"); // NOI18N
			EnterImageViewer.addActionListener(new ActionListenerAnonymousInnerClass40(this));
			ToolsSubMenu.add(EnterImageViewer);

			VfpuRegisters.Text = bundle.getString("VfpuFrame.title"); // NOI18N
			VfpuRegisters.addActionListener(new ActionListenerAnonymousInnerClass41(this));
			ToolsSubMenu.add(VfpuRegisters);

			ElfHeaderViewer.Text = bundle.getString("ElfHeaderInfo.title"); // NOI18N
			ElfHeaderViewer.addActionListener(new ActionListenerAnonymousInnerClass42(this));
			ToolsSubMenu.add(ElfHeaderViewer);

			FileLog.Text = bundle.getString("FileLoggerFrame.title"); // NOI18N
			FileLog.addActionListener(new ActionListenerAnonymousInnerClass43(this));
			ToolsSubMenu.add(FileLog);

			InstructionCounter.Text = bundle.getString("InstructionCounter.title"); // NOI18N
			InstructionCounter.addActionListener(new ActionListenerAnonymousInnerClass44(this));
			ToolsSubMenu.add(InstructionCounter);

			DebugMenu.add(ToolsSubMenu);

			DumpIso.Text = bundle.getString("MainGUI.DumpIso.text"); // NOI18N
			DumpIso.Enabled = false;
			DumpIso.addActionListener(new ActionListenerAnonymousInnerClass45(this));
			DebugMenu.add(DumpIso);

			ResetProfiler.setText(bundle.getString("MainGUI.ResetProfiler.text")); // NOI18N
			ResetProfiler.addActionListener(new ActionListenerAnonymousInnerClass46(this));
			DebugMenu.add(ResetProfiler);

			ClearTextureCache.Text = bundle.getString("MainGUI.ClearTextureCache.text"); // NOI18N
			ClearTextureCache.addActionListener(new ActionListenerAnonymousInnerClass47(this));
			DebugMenu.add(ClearTextureCache);

			ClearVertexCache.Text = bundle.getString("MainGUI.ClearVertexCache.text"); // NOI18N
			ClearVertexCache.addActionListener(new ActionListenerAnonymousInnerClass48(this));
			DebugMenu.add(ClearVertexCache);

			ExportISOFile.Text = bundle.getString("MainGUI.ExportISOFile.text"); // NOI18N
			ExportISOFile.Enabled = false;
			ExportISOFile.addActionListener(new ActionListenerAnonymousInnerClass49(this));
			DebugMenu.add(ExportISOFile);

			MenuBar.add(DebugMenu);

			CheatsMenu.Text = bundle.getString("MainGUI.CheatsMenu.text"); // NOI18N

			cwcheat.Text = "CWCheat"; // NOI18N
			cwcheat.addActionListener(new ActionListenerAnonymousInnerClass50(this));
			CheatsMenu.add(cwcheat);

			MenuBar.add(CheatsMenu);

			LanguageMenu.Text = "Language"; // NOI18N

			SystemLocale.Text = bundle.getString("MainGUI.SystemLocale.text"); // NOI18N
			SystemLocale.addActionListener(new ActionListenerAnonymousInnerClass51(this));
			LanguageMenu.add(SystemLocale);

			EnglishGB.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/en_UK.png")); // NOI18N
			java.util.ResourceBundle bundle1 = java.util.ResourceBundle.getBundle("pspsharp/languages/common"); // NOI18N
			EnglishGB.Text = bundle1.getString("englishUK"); // NOI18N
			EnglishGB.addActionListener(new ActionListenerAnonymousInnerClass52(this));
			LanguageMenu.add(EnglishGB);

			EnglishUS.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/en_US.png")); // NOI18N
			EnglishUS.Text = bundle1.getString("englishUS"); // NOI18N
			EnglishUS.addActionListener(new ActionListenerAnonymousInnerClass53(this));
			LanguageMenu.add(EnglishUS);

			French.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/fr_FR.png")); // NOI18N
			French.Text = bundle1.getString("french"); // NOI18N
			French.addActionListener(new ActionListenerAnonymousInnerClass54(this));
			LanguageMenu.add(French);

			German.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/de_DE.png")); // NOI18N
			German.Text = bundle1.getString("german"); // NOI18N
			German.addActionListener(new ActionListenerAnonymousInnerClass55(this));
			LanguageMenu.add(German);

			Lithuanian.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/lt_LT.png")); // NOI18N
			Lithuanian.Text = bundle1.getString("lithuanian"); // NOI18N
			Lithuanian.addActionListener(new ActionListenerAnonymousInnerClass56(this));
			LanguageMenu.add(Lithuanian);

			Spanish.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/es_ES.png")); // NOI18N
			Spanish.Text = bundle1.getString("spanish"); // NOI18N
			Spanish.addActionListener(new ActionListenerAnonymousInnerClass57(this));
			LanguageMenu.add(Spanish);

			Catalan.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/es_CA.png")); // NOI18N
			Catalan.Text = bundle1.getString("catalan"); // NOI18N
			Catalan.addActionListener(new ActionListenerAnonymousInnerClass58(this));
			LanguageMenu.add(Catalan);

			Portuguese.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/pt_PT.png")); // NOI18N
			Portuguese.Text = bundle1.getString("portuguese"); // NOI18N
			Portuguese.addActionListener(new ActionListenerAnonymousInnerClass59(this));
			LanguageMenu.add(Portuguese);

			PortugueseBR.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/pt_BR.png")); // NOI18N
			PortugueseBR.Text = bundle1.getString("portuguesebr"); // NOI18N
			PortugueseBR.addActionListener(new ActionListenerAnonymousInnerClass60(this));
			LanguageMenu.add(PortugueseBR);

			Japanese.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/jp_JP.png")); // NOI18N
			Japanese.Text = bundle1.getString("japanese"); // NOI18N
			Japanese.addActionListener(new ActionListenerAnonymousInnerClass61(this));
			LanguageMenu.add(Japanese);

			Russian.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/ru_RU.png")); // NOI18N
			Russian.Text = bundle1.getString("russian"); // NOI18N
			Russian.addActionListener(new ActionListenerAnonymousInnerClass62(this));
			LanguageMenu.add(Russian);

			Polish.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/pl_PL.png")); // NOI18N
			Polish.Text = bundle1.getString("polish"); // NOI18N
			Polish.addActionListener(new ActionListenerAnonymousInnerClass63(this));
			LanguageMenu.add(Polish);

			ChinesePRC.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/cn_CN.png")); // NOI18N
			ChinesePRC.Text = bundle1.getString("simplifiedChinese"); // NOI18N
			ChinesePRC.addActionListener(new ActionListenerAnonymousInnerClass64(this));
			LanguageMenu.add(ChinesePRC);

			ChineseTW.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/tw_TW.png")); // NOI18N
			ChineseTW.Text = bundle1.getString("traditionalChinese"); // NOI18N
			ChineseTW.addActionListener(new ActionListenerAnonymousInnerClass65(this));
			LanguageMenu.add(ChineseTW);

			Italian.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/it_IT.png")); // NOI18N
			Italian.Text = bundle1.getString("italian"); // NOI18N
			Italian.addActionListener(new ActionListenerAnonymousInnerClass66(this));
			LanguageMenu.add(Italian);

			Greek.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/flags/gr_EL.png")); // NOI18N
			Greek.Text = bundle1.getString("greek"); // NOI18N
			Greek.addActionListener(new ActionListenerAnonymousInnerClass67(this));
			LanguageMenu.add(Greek);

			MenuBar.add(LanguageMenu);

			PluginsMenu.Text = bundle.getString("MainGUI.PluginsMenu.text"); // NOI18N

			xbrzCheck.Selected = Settings.Instance.readBool("emu.plugins.xbrz");
			xbrzCheck.Text = bundle.getString("MainGUI.xbrzCheck.text"); // NOI18N
			xbrzCheck.addActionListener(new ActionListenerAnonymousInnerClass68(this));
			PluginsMenu.add(xbrzCheck);

			MenuBar.add(PluginsMenu);

			HelpMenu.Text = bundle.getString("MainGUI.HelpMenu.text"); // NOI18N

			About.Accelerator = javax.swing.KeyStroke.getKeyStroke(KeyEvent.VK_F1, 0);
			About.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/AboutIcon.png")); // NOI18N
			About.Text = bundle.getString("MainGUI.About.text"); // NOI18N
			About.addActionListener(new ActionListenerAnonymousInnerClass69(this));
			HelpMenu.add(About);

			MenuBar.add(HelpMenu);

			JMenuBar = MenuBar;

			pack();
		} // </editor-fold>//GEN-END:initComponents

		private class WindowAdapterAnonymousInnerClass : java.awt.@event.WindowAdapter
		{
			private readonly MainGUI outerInstance;

			public WindowAdapterAnonymousInnerClass(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void windowClosing(java.awt.@event.WindowEvent evt)
			{
				outerInstance.formWindowClosing(evt);
			}
		}

		private class ComponentAdapterAnonymousInnerClass : java.awt.@event.ComponentAdapter
		{
			private readonly MainGUI outerInstance;

			public ComponentAdapterAnonymousInnerClass(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void componentResized(ComponentEvent evt)
			{
				outerInstance.formComponentResized(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.RunButtonActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass2 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass2(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.PauseButtonActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass3 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass3(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ResetButtonActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass4 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass4(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.openUmdActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass5 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass5(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.OpenFileActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass6 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass6(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.switchUmdActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass7 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass7(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ejectMsActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass8 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass8(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.SaveSnapActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass9 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass9(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.LoadSnapActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass10 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass10(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ExportVisibleElementsActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass11 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass11(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ExportAllElementsActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass12 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass12(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ExitEmuActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass13 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass13(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.oneTimeResizeActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass14 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass14(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.twoTimesResizeActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass15 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass15(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.threeTimesResizeActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass16 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass16(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.noneCheckActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass17 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass17(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.anisotropicCheckActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass18 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass18(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.frameSkipNoneActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass19 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass19(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.frameSkipFPS5ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass20 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass20(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.frameSkipFPS10ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass21 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass21(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.frameSkipFPS15ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass22 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass22(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.frameSkipFPS20ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass23 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass23(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.frameSkipFPS30ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass24 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass24(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.frameSkipFPS60ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass25 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass25(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ShotItemActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass26 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass26(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.RotateItemActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass27 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass27(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.MuteOptActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass28 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass28(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ClockSpeed50ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass29 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass29(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ClockSpeed75ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass30 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass30(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ClockSpeedNormalActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass31 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass31(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ClockSpeed150ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass32 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass32(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ClockSpeed200ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass33 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass33(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ClockSpeed300ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass34 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass34(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ControlsConfActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass35 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass35(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ConfigMenuActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass36 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass36(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ToggleLoggerActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass37 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass37(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.CustomLoggerActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass38 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass38(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.EnterDebuggerActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass39 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass39(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.EnterMemoryViewerActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass40 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass40(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.EnterImageViewerActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass41 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass41(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.VfpuRegistersActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass42 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass42(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ElfHeaderViewerActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass43 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass43(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.FileLogActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass44 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass44(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.InstructionCounterActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass45 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass45(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.DumpIsoActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass46 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass46(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ResetProfilerActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass47 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass47(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ClearTextureCacheActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass48 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass48(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ClearVertexCacheActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass49 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass49(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ExportISOFileActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass50 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass50(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.cwcheatActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass51 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass51(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.SystemLocaleActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass52 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass52(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.EnglishGBActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass53 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass53(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.EnglishUSActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass54 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass54(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.FrenchActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass55 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass55(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.GermanActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass56 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass56(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.LithuanianActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass57 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass57(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.SpanishActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass58 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass58(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.CatalanActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass59 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass59(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.PortugueseActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass60 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass60(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.PortugueseBRActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass61 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass61(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.JapaneseActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass62 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass62(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.RussianActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass63 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass63(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.PolishActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass64 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass64(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ChinesePRCActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass65 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass65(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ChineseTWActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass66 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass66(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ItalianActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass67 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass67(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.GreekActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass68 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass68(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.xbrzCheckActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass69 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass69(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.AboutActionPerformed(evt);
			}
		}

		private void createComponents()
		{
			initComponents();

			if (useFullscreen)
			{
				// Hide the menu bar and the toolbar in full screen mode
				MenuBar.Visible = false;
				mainToolBar.Visible = false;
				ContentPane.remove(mainToolBar);

				fillerLeft = new JLabel();
				fillerRight = new JLabel();
				fillerTop = new JLabel();
				fillerBottom = new JLabel();

				fillerLeft.Background = Color.BLACK;
				fillerRight.Background = Color.BLACK;
				fillerTop.Background = Color.BLACK;
				fillerBottom.Background = Color.BLACK;

				fillerLeft.Opaque = true;
				fillerRight.Opaque = true;
				fillerTop.Opaque = true;
				fillerBottom.Opaque = true;

				ContentPane.add(fillerLeft, BorderLayout.LINE_START);
				ContentPane.add(fillerRight, BorderLayout.LINE_END);
				ContentPane.add(fillerTop, BorderLayout.NORTH);
				ContentPane.add(fillerBottom, BorderLayout.SOUTH);

				makeFullScreenMenu();
			}
			else
			{
				float viewportResizeScaleFactor = Modules.sceDisplayModule.ViewportResizeScaleFactor;
				if (viewportResizeScaleFactor <= 1.5f)
				{
					oneTimeResize.Selected = true;
				}
				else if (viewportResizeScaleFactor <= 2.5f)
				{
					twoTimesResize.Selected = true;
				}
				else
				{
					threeTimesResize.Selected = true;
				}
			}

			populateRecentMenu();
		}

		private void changeLanguage(string language)
		{
			Settings.Instance.writeString("emu.language", language);
			JpcspDialogManager.showInformation(this, "Language change will take effect after application restart.");
		}

		/// <summary>
		/// Create a popup menu for use in full screen mode. In full screen mode, the
		/// menu bar and the toolbar are not displayed. To keep a consistent user
		/// interface, the popup menu is composed of the entries from the toolbar and
		/// from the menu bar.
		/// 
		/// Accelerators do not work natively as the popup menu must have focus for
		/// them to work. Therefore the accelerators are copied and handled in the
		/// KeyListener related code of MainGUI for fullscreen mode.
		/// </summary>
		private void makeFullScreenMenu()
		{
			fullScreenMenu = new JPopupMenu();

			JMenuItem popupMenuItemRun = new JMenuItem(java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp").getString("MainGUI.RunButton.text"));
			popupMenuItemRun.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/PlayIcon.png"));
			popupMenuItemRun.addActionListener(new ActionListenerAnonymousInnerClass70(this));

			JMenuItem popupMenuItemPause = new JMenuItem(java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp").getString("MainGUI.PauseButton.text"));
			popupMenuItemPause.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/PauseIcon.png"));
			popupMenuItemPause.addActionListener(new ActionListenerAnonymousInnerClass71(this));

			JMenuItem popupMenuItemReset = new JMenuItem(java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp").getString("MainGUI.ResetButton.text"));
			popupMenuItemReset.setIcon(new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/StopIcon.png")));
			popupMenuItemReset.addActionListener(new ActionListenerAnonymousInnerClass72(this));

			fullScreenMenu.add(popupMenuItemRun);
			fullScreenMenu.add(popupMenuItemPause);
			fullScreenMenu.add(popupMenuItemReset);
			fullScreenMenu.addSeparator();

			// add all the menu entries from the MenuBar to the full screen menu
			while (MenuBar.MenuCount > 0)
			{
				fullScreenMenu.add(MenuBar.getMenu(0));
			}

			// move the 'Exit' menu item from the 'File' menu
			// to the end of the full screen menu for convenience
			fullScreenMenu.addSeparator();
			fullScreenMenu.add(ExitEmu);

			// the 'Resize' menu is not relevant in full screen mode
			VideoOpt.remove(ResizeMenu);

			// copy accelerators to actionListenerMap to have them handled in
			// MainGUI using the keyPressed event
			globalAccelFromMenu(fullScreenMenu);
		}

		private class ActionListenerAnonymousInnerClass70 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass70(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void actionPerformed(ActionEvent e)
			{
				outerInstance.RunButtonActionPerformed(e);
			}
		}

		private class ActionListenerAnonymousInnerClass71 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass71(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void actionPerformed(ActionEvent e)
			{
				outerInstance.PauseButtonActionPerformed(e);
			}
		}

		private class ActionListenerAnonymousInnerClass72 : ActionListener
		{
			private readonly MainGUI outerInstance;

			public ActionListenerAnonymousInnerClass72(MainGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void actionPerformed(ActionEvent e)
			{
				outerInstance.ResetButtonActionPerformed(e);
			}
		}

		/// <summary>
		/// Add accelerators to the global action map of MainGUI.
		/// 
		/// This function will traverse a MenuElement tree to find all JMenuItems and
		/// to add accelerators if available for the given menu item.
		/// 
		/// The element tree is traversed in a recursive manner.
		/// </summary>
		/// <param name="me"> The root menu element to start. </param>
		private void globalAccelFromMenu(MenuElement me)
		{
			foreach (MenuElement element in me.SubElements)
			{
				// check for JMenu before JMenuItem, as JMenuItem is derived from JMenu
				if ((element is JPopupMenu) || (element is JMenu))
				{
					// recursively do the same for underlying menus
					globalAccelFromMenu(element);
				}
				else if (element is JMenuItem)
				{
					JMenuItem item = (JMenuItem) element;
					// only check if the accelerator exists (i.e. is not null)
					// if no ActionListeners exist, an empty array is returned
					if (item.Accelerator != null)
					{
						actionListenerMap[item.Accelerator] = item.ActionListeners;
					}
				}
			}
		}

		public static Dimension FullScreenDimension
		{
			get
			{
				DisplayMode displayMode;
				if (Emulator.MainGUI.DisplayMode != null)
				{
					displayMode = Emulator.MainGUI.DisplayMode;
				}
				else
				{
					displayMode = GraphicsEnvironment.LocalGraphicsEnvironment.DefaultScreenDevice.DisplayMode;
				}
				return new Dimension(displayMode.Width, displayMode.Height);
		//    	return GraphicsEnvironment.getLocalGraphicsEnvironment().getMaximumWindowBounds().getSize();
			}
		}

		public virtual void startWindowDialog(Window window)
		{
			GraphicsDevice localDevice = GraphicsEnvironment.LocalGraphicsEnvironment.DefaultScreenDevice;
			if (localDevice.FullScreenWindow != null)
			{
				localDevice.FullScreenWindow = null;
			}
			window.Visible = true;
		}

		public virtual void startBackgroundWindowDialog(Window window)
		{
			startWindowDialog(window);
			requestFocus();
			Modules.sceDisplayModule.Canvas.requestFocusInWindow();
		}

		public virtual void endWindowDialog()
		{
			if (displayMode != null)
			{
				GraphicsDevice localDevice = GraphicsEnvironment.LocalGraphicsEnvironment.DefaultScreenDevice;
				if (localDevice.FullScreenWindow == null)
				{
					localDevice.FullScreenWindow = this;
					setDisplayMode();
				}
				if (useFullscreen)
				{
					setFullScreenDisplaySize();
				}
			}
		}

		private void changeScreenResolution(int width, int height)
		{
			// Find the matching display mode with the preferred refresh rate
			// (or the highest refresh rate if the preferred refresh rate is not found).
			GraphicsDevice localDevice = GraphicsEnvironment.LocalGraphicsEnvironment.DefaultScreenDevice;
			DisplayMode[] displayModes = localDevice.DisplayModes;
			DisplayMode bestDisplayMode = null;
			for (int i = 0; displayModes != null && i < displayModes.Length; i++)
			{
				DisplayMode dispMode = displayModes[i];
				if (dispMode.Width == width && dispMode.Height == height && dispMode.BitDepth == displayModeBitDepth)
				{
					if (bestDisplayMode == null || (bestDisplayMode.RefreshRate < dispMode.RefreshRate && bestDisplayMode.RefreshRate != preferredDisplayModeRefreshRate))
					{
						bestDisplayMode = dispMode;
					}
				}
			}

			if (bestDisplayMode != null)
			{
				changeScreenResolution(bestDisplayMode);
			}
		}

		private void setDisplayMode()
		{
			if (displayMode != null)
			{
				GraphicsDevice localDevice = GraphicsEnvironment.LocalGraphicsEnvironment.DefaultScreenDevice;
				localDevice.DisplayMode = displayMode;

				if (setLocationThread == null)
				{
					// Set up a thread calling setLocation() at regular intervals.
					// It seems that the window location is sometimes lost when
					// changing the DisplayMode.
					setLocationThread = new SetLocationThread();
					setLocationThread.setName("Set MainGUI Location Thread");
					setLocationThread.setDaemon(true);
					setLocationThread.Start();
				}
			}
		}

		public virtual void setLocation()
		{
			if (displayMode != null && useFullscreen)
			{
				// FIXME When running in non-native resolution, the window is not displaying
				// if it is completely visible. It is only displaying if part of it is
				// hidden (e.g. outside screen borders).
				// This seems to be a Java bug.
				// Hack here is to move the window 1 pixel outside the screen so that
				// it gets displayed.
				if (fillerTop == null || fillerTop.Height == 0)
				{
					if (Location.y != -1)
					{
						setLocation(0, -1);
					}
				}
				else if (fillerLeft.Width == 0)
				{
					if (Location.x != -1)
					{
						setLocation(-1, 0);
					}
				}
			}
		}

		public virtual void setFullScreenDisplaySize()
		{
			Dimension size = new Dimension(sceDisplay.getResizedWidth(Screen.width), sceDisplay.getResizedHeight(Screen.height));
			FullScreenDisplaySize = size;
		}

		private Dimension FullScreenDisplaySize
		{
			set
			{
				Dimension fullScreenSize = FullScreenDimension;
    
				setLocation();
				if (value.width < fullScreenSize.width)
				{
					fillerLeft.setSize((fullScreenSize.width - value.width) / 2, fullScreenSize.height);
					fillerRight.setSize(fullScreenSize.width - value.width - fillerLeft.Width, fullScreenSize.height);
				}
				else
				{
					fillerLeft.setSize(0, 0);
					fillerRight.setSize(1, fullScreenSize.height);
					setSize(fullScreenSize.width + 1, fullScreenSize.height);
					PreferredSize = Size;
				}
    
				if (value.height < fullScreenSize.height)
				{
					fillerTop.setSize(fullScreenSize.width, (fullScreenSize.height - value.height) / 2);
					fillerBottom.setSize(fullScreenSize.width, fullScreenSize.height - value.height - fillerTop.Height);
				}
				else
				{
					fillerTop.setSize(0, 0);
					fillerBottom.setSize(fullScreenSize.width, 1);
					setSize(fullScreenSize.width, fullScreenSize.height + 1);
					PreferredSize = Size;
				}
    
				fillerLeft.PreferredSize = fillerLeft.Size;
				fillerRight.PreferredSize = fillerRight.Size;
				fillerTop.PreferredSize = fillerTop.Size;
				fillerBottom.PreferredSize = fillerBottom.Size;
			}
		}

		private void changeScreenResolution(DisplayMode displayMode)
		{
			GraphicsDevice localDevice = GraphicsEnvironment.LocalGraphicsEnvironment.DefaultScreenDevice;
			if (localDevice.FullScreenSupported)
			{
				this.displayMode = displayMode;
				localDevice.FullScreenWindow = this;
				setDisplayMode();
				if (useFullscreen)
				{
					Size = FullScreenDimension;
					PreferredSize = FullScreenDimension;
					setLocation();
				}

				if (log.InfoEnabled)
				{
					log.info(string.Format("Changing resolution to {0:D}x{1:D}, {2:D} bits, {3:D} Hz", displayMode.Width, displayMode.Height, displayMode.BitDepth, displayMode.RefreshRate));
				}
			}
		}

		public virtual LogWindow ConsoleWindow
		{
			get
			{
				return State.logWindow;
			}
		}

		private void populateRecentMenu()
		{
			RecentMenu.removeAll();
			recentUMD.Clear();
			recentFile.Clear();

			Settings.Instance.readRecent("umd", recentUMD);
			Settings.Instance.readRecent("file", recentFile);

			foreach (RecentElement umd in recentUMD)
			{
				JMenuItem item = new JMenuItem(umd.ToString());
				item.addActionListener(new RecentElementActionListener(this, this, RecentElementActionListener.TYPE_UMD, umd.path));
				RecentMenu.add(item);
			}

			if (recentUMD.Count > 0 && recentFile.Count > 0)
			{
				RecentMenu.addSeparator();
			}

			foreach (RecentElement file in recentFile)
			{
				JMenuItem item = new JMenuItem(file.ToString());
				item.addActionListener(new RecentElementActionListener(this, this, RecentElementActionListener.TYPE_FILE, file.path));
				RecentMenu.add(item);
			}
		}

	private void EnterDebuggerActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_EnterDebuggerActionPerformed
			if (State.debugger == null)
			{
				State.debugger = new DisassemblerFrame(emulator);
			}
			else
			{
				State.debugger.RefreshDebugger(false);
			}
			startWindowDialog(State.debugger);
	} //GEN-LAST:event_EnterDebuggerActionPerformed

	private void RunButtonActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_RunButtonActionPerformed
			run();
	} //GEN-LAST:event_RunButtonActionPerformed

		private JFileChooser makeJFileChooser()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final JFileChooser fc = new JFileChooser();
			JFileChooser fc = new JFileChooser();
			fc.DialogTitle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp").getString("MainGUI.strOpenELFPBP.text");
			fc.CurrentDirectory = new File(".");
			return fc;
		}
	private void OpenFileActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_OpenFileActionPerformed
			PauseEmu();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final JFileChooser fc = makeJFileChooser();
			JFileChooser fc = makeJFileChooser();
			string lastOpenedFolder = Settings.Instance.readString("gui.lastOpenedFileFolder");
			if (!string.ReferenceEquals(lastOpenedFolder, null))
			{
				fc.CurrentDirectory = new File(lastOpenedFolder);
			}
			int returnVal = fc.showOpenDialog(this);

			if (userChooseSomething(returnVal))
			{
				Settings.Instance.writeString("gui.lastOpenedFileFolder", fc.SelectedFile.Parent);
				File file = fc.SelectedFile;
				switch (FileUtil.getExtension(file))
				{
					case "iso":
					case "cso":
						loadUMD(file);
						break;
					default:
						loadFile(file);
						break;
				}
			}
	} //GEN-LAST:event_OpenFileActionPerformed

		private string pspifyFilename(string pcfilename)
		{
			// Files relative to ms0 directory
			if (pcfilename.StartsWith("ms0", StringComparison.Ordinal))
			{
				return "ms0:" + pcfilename.Substring(3).replaceAll("\\\\", "/").ToUpper();
			}
			// Files relative to flash0 directory
			if (pcfilename.StartsWith("flash0", StringComparison.Ordinal))
			{
				return "flash0:" + pcfilename.Substring(6).replaceAll("\\\\", "/");
			}

			// Files with absolute path but also in ms0 directory
			try
			{
				string ms0path = (new File("ms0")).CanonicalPath;
				if (pcfilename.StartsWith(ms0path, StringComparison.Ordinal))
				{
					// Strip off absolute prefix
					return "ms0:" + pcfilename.Substring(ms0path.Length).replaceAll("\\\\", "/");
				}
			}
			catch (Exception e)
			{
				// Required by File.getCanonicalPath
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			// Files anywhere on user's hard drive, may not work
			// use host0:/ ?
			return pcfilename.replaceAll("\\\\", "/");
		}

		public virtual void loadFile(File file)
		{
			loadFile(file, false);
		}

		public virtual void loadFile(File file, bool isInternal)
		{
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			Model.Model = Settings.Instance.readInt("emu.model");

			//This is where a real application would open the file.
			try
			{
				if (State.logWindow != null)
				{
					State.logWindow.clearScreenMessages();
				}
				log.info(MetaInformation.FULL_NAME);

				umdLoaded = false;
				loadedFile = file;

				// Create a read-only memory-mapped file
				RandomAccessFile raf = new RandomAccessFile(file, "r");
				ByteBuffer readbuffer;
				FileChannel roChannel = null;
				long size = raf.Length();
				// Do not try to map very large files, this would raise on OutOfMemory exception.
				if (size > 1 * 1024 * 1024)
				{
					readbuffer = Utilities.readAsByteBuffer(raf);
				}
				else
				{
					roChannel = raf.Channel;
					readbuffer = roChannel.map(FileChannel.MapMode.READ_ONLY, 0, (int) roChannel.size());
				}
				SceModule module = emulator.load(pspifyFilename(file.Path), readbuffer);
				if (roChannel != null)
				{
					roChannel.close();
				}
				raf.close();

				bool isHomebrew;
				if (isInternal)
				{
					isHomebrew = false;
				}
				else
				{
					PSF psf = module.psf;
					string title;
					string discId = State.DISCID_UNKNOWN_FILE;
					if (psf != null)
					{
						title = psf.getPrintableString("TITLE");
						discId = psf.getString("DISC_ID");
						if (string.ReferenceEquals(discId, null))
						{
							discId = State.DISCID_UNKNOWN_FILE;
						}
						isHomebrew = psf.LikelyHomebrew;
					}
					else
					{
						title = file.ParentFile.Name;
						isHomebrew = true; // missing psf, assume homebrew
					}
					Title = MetaInformation.FULL_NAME + " - " + title;
					addRecentFile(file, title);

					RuntimeContext.IsHomebrew = isHomebrew;
					State.discId = discId;
					State.title = title;
				}

				// Strip off absolute file path if the file is inside our ms0 directory
				string filepath = file.Parent;
				string ms0path = (new File("ms0")).CanonicalPath;
				if (filepath.StartsWith(ms0path, StringComparison.Ordinal))
				{
					filepath = filepath.Substring(ms0path.Length - 3); // path must start with "ms0"
				}

				Modules.IoFileMgrForUserModule.setfilepath(filepath);
				Modules.IoFileMgrForUserModule.IsoReader = null;
				Modules.sceUmdUserModule.IsoReader = null;

				if (!isHomebrew && !isInternal)
				{
					Settings.Instance.loadPatchSettings();
				}
				if (!RunningFromVsh && !RunningReboot)
				{
					logStart();
				}

				if (instructioncounter != null)
				{
					instructioncounter.RefreshWindow();
				}
				StepLogger.clear();
				StepLogger.Name = file.Path;
			}
			catch (GeneralJpcspException e)
			{
				JpcspDialogManager.showError(this, bundle.getString("MainGUI.strGeneralError.text") + ": " + e.LocalizedMessage);
			}
			catch (IOException e)
			{
				if (file.Name.contains("iso") || file.Name.contains("cso"))
				{
					JpcspDialogManager.showError(this, bundle.getString("MainGUI.strCriticalError.text") + ": " + bundle.getString("MainGUI.strWrongLoader.text"));
				}
				else
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					JpcspDialogManager.showError(this, bundle.getString("ioError") + ": " + e.LocalizedMessage);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.Write(ex.StackTrace);
				if (ex.Message != null)
				{
					JpcspDialogManager.showError(this, bundle.getString("MainGUI.strCriticalError.text") + ": " + ex.LocalizedMessage);
				}
				else
				{
					JpcspDialogManager.showError(this, bundle.getString("MainGUI.strCriticalError.text") + ": " + bundle.getString("MainGUI.strCheckConsole.text"));
				}
			}
			RefreshUI();
		}

		private void addRecentFile(File file, string title)
		{
			string s = file.Path;
			for (int i = 0; i < recentFile.Count; ++i)
			{
				if (recentFile[i].path.Equals(s))
				{
					recentFile.RemoveAt(i--);
				}
			}
			recentFile.Insert(0, new RecentElement(s, title));
			while (recentFile.Count > MAX_RECENT)
			{
				recentFile.RemoveAt(MAX_RECENT);
			}
			Settings.Instance.writeRecent("file", recentFile);
			populateRecentMenu();
		}

		private void removeRecentFile(string file)
		{
			// use Iterator to safely remove elements while traversing
			IEnumerator<RecentElement> it = recentFile.GetEnumerator();
			while (it.MoveNext())
			{
				RecentElement re = it.Current;
				if (re.path.Equals(file))
				{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					it.remove();
				}
			}
			Settings.Instance.writeRecent("file", recentFile);
			populateRecentMenu();
		}

		private void addRecentUMD(File file, string title)
		{
			if (file == null)
			{
				return;
			}

			try
			{
				string s = file.CanonicalPath;
				for (int i = 0; i < recentUMD.Count; ++i)
				{
					if (recentUMD[i].path.Equals(s))
					{
						recentUMD.RemoveAt(i--);
					}
				}
				recentUMD.Insert(0, new RecentElement(s, title));
				while (recentUMD.Count > MAX_RECENT)
				{
					recentUMD.RemoveAt(MAX_RECENT);
				}
				Settings.Instance.writeRecent("umd", recentUMD);
				populateRecentMenu();
			}
			catch (IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		private void removeRecentUMD(string file)
		{
			// use Iterator to safely remove elements while traversing
			IEnumerator<RecentElement> it = recentUMD.GetEnumerator();
			while (it.MoveNext())
			{
				RecentElement re = it.Current;
				if (re.path.Equals(file))
				{
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					it.remove();
				}
			}
			Settings.Instance.writeRecent("umd", recentUMD);
			populateRecentMenu();
		}

	private void PauseButtonActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_PauseButtonActionPerformed
			pause();
	} //GEN-LAST:event_PauseButtonActionPerformed

	private void ElfHeaderViewerActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ElfHeaderViewerActionPerformed
			if (State.elfHeader == null)
			{
				State.elfHeader = new ElfHeaderInfo();
			}
			else
			{
				State.elfHeader.RefreshWindow();
			}
			startWindowDialog(State.elfHeader);
	} //GEN-LAST:event_ElfHeaderViewerActionPerformed

	private void EnterMemoryViewerActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_EnterMemoryViewerActionPerformed
			if (State.memoryViewer == null)
			{
				State.memoryViewer = new MemoryViewer();
			}
			else
			{
				State.memoryViewer.RefreshMemory();
			}
			startWindowDialog(State.memoryViewer);
	} //GEN-LAST:event_EnterMemoryViewerActionPerformed

	private void EnterImageViewerActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_EnterImageViewerActionPerformed
			if (State.imageViewer == null)
			{
				State.imageViewer = new ImageViewer();
			}
			else
			{
				State.imageViewer.RefreshImage();
			}
			startWindowDialog(State.imageViewer);
	} //GEN-LAST:event_EnterImageViewerActionPerformed

	private void AboutActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_AboutActionPerformed
			StringBuilder message = new StringBuilder();
			message.Append("<html>").Append("<h2>").Append(MetaInformation.FULL_NAME).Append("</h2>").Append("<hr/>").Append("Official site      : <a href='").Append(MetaInformation.OFFICIAL_SITE).Append("'>").Append(MetaInformation.OFFICIAL_SITE).Append("</a><br/>").Append("Official forum     : <a href='").Append(MetaInformation.OFFICIAL_FORUM).Append("'>").Append(MetaInformation.OFFICIAL_FORUM).Append("</a><br/>").Append("Official repository: <a href='").Append(MetaInformation.OFFICIAL_REPOSITORY).Append("'>").Append(MetaInformation.OFFICIAL_REPOSITORY).Append("</a><br/>").Append("<hr/>").Append("<i>Team:</i> <font color='gray'>").Append(MetaInformation.TEAM).Append("</font>").Append("</html>");
			MessageBox.Show(this, message.ToString(), MetaInformation.FULL_NAME, MessageBoxIcon.Information);
	} //GEN-LAST:event_AboutActionPerformed

	private void ConfigMenuActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ConfigMenuActionPerformed
			if (State.settingsGUI == null)
			{
				State.settingsGUI = new SettingsGUI();
			}
			else
			{
				State.settingsGUI.RefreshWindow();
			}
			startWindowDialog(State.settingsGUI);
	} //GEN-LAST:event_ConfigMenuActionPerformed

	private void ExitEmuActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ExitEmuActionPerformed
			exitEmu();
	} //GEN-LAST:event_ExitEmuActionPerformed

	private void formWindowClosing(java.awt.@event.WindowEvent evt)
	{ //GEN-FIRST:event_formWindowClosing
			// this is only needed for the main screen, as it can be closed without
			// being deactivated first
			WindowPropSaver.saveWindowProperties(this);
			exitEmu();
	} //GEN-LAST:event_formWindowClosing

	private void openUmdActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_openUmdActionPerformed
			if (!RunningFromVsh)
			{
				PauseEmu();
			}

			if (Settings.Instance.readBool("emu.umdbrowser"))
			{
				umdbrowser = new UmdBrowser(this, getUmdPaths(false));
				umdbrowser.Visible = true;
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final JFileChooser fc = makeJFileChooser();
				JFileChooser fc = makeJFileChooser();
				string lastOpenedFolder = Settings.Instance.readString("gui.lastOpenedUmdFolder");
				if (!string.ReferenceEquals(lastOpenedFolder, null))
				{
					fc.CurrentDirectory = new File(lastOpenedFolder);

				}
				fc.DialogTitle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp").getString("MainGUI.strOpenUMD.text");
				int returnVal = fc.showOpenDialog(this);

				if (userChooseSomething(returnVal))
				{
					Settings.Instance.writeString("gui.lastOpenedUmdFolder", fc.SelectedFile.Parent);
					File file = fc.SelectedFile;
					loadAndRunUMD(file);
				}
			}
	} //GEN-LAST:event_openUmdActionPerformed

	private void switchUmdActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_switchUmdActionPerformed
			if (Settings.Instance.readBool("emu.umdbrowser"))
			{
				umdbrowser = new UmdBrowser(this, getUmdPaths(false));
				umdbrowser.SwitchingUmd = true;
				umdbrowser.Visible = true;
			}
			else
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final JFileChooser fc = makeJFileChooser();
				JFileChooser fc = makeJFileChooser();
				string lastOpenedFolder = Settings.Instance.readString("gui.lastOpenedUmdFolder");
				if (!string.ReferenceEquals(lastOpenedFolder, null))
				{
					fc.CurrentDirectory = new File(lastOpenedFolder);
				}
				fc.DialogTitle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp").getString("MainGUI.switchUMD.text");
				int returnVal = fc.showOpenDialog(this);

				if (userChooseSomething(returnVal))
				{
					Settings.Instance.writeString("gui.lastOpenedUmdFolder", fc.SelectedFile.Parent);
					File file = fc.SelectedFile;
					switchUMD(file);
				}
			}
	} //GEN-LAST:event_switchUmdActionPerformed

	private void ejectMsActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ejectMsActionPerformed
		if (MemoryStick.Inserted)
		{
			Modules.IoFileMgrForUserModule.hleEjectMemoryStick();
		}
		else
		{
			Modules.IoFileMgrForUserModule.hleInsertMemoryStick();
		}
	} //GEN-LAST:event_ejectMsActionPerformed

		public virtual void onMemoryStickChange()
		{
			ResourceBundle bundle = ResourceBundle.getBundle("pspsharp/languages/pspsharp");
			if (MemoryStick.Inserted)
			{
				ejectMs.Text = bundle.getString("MainGUI.ejectMs.text");
			}
			else
			{
				ejectMs.Text = bundle.getString("MainGUI.insertMs.text");
			}
		}

		public virtual void RefreshUI()
		{
			ExportISOFile.Enabled = umdLoaded;
			DumpIso.Enabled = umdLoaded;
		}

		/// <summary>
		/// Don't call this directly, see loadUMD(File file)
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean loadUMD(pspsharp.filesystems.umdiso.UmdIsoReader iso, String bootPath) throws java.io.IOException
		private bool loadUMD(UmdIsoReader iso, string bootPath)
		{
			bool success = false;
			try
			{
				UmdIsoFile bootBin = iso.getFile(bootPath);
				if (bootBin.Length() != 0)
				{
					sbyte[] bootfile = new sbyte[(int) bootBin.Length()];
					bootBin.read(bootfile);
					ByteBuffer buf = ByteBuffer.wrap(bootfile);
					emulator.load("disc0:/" + bootPath, buf);
					success = true;
				}
			}
			catch (FileNotFoundException e)
			{
				Console.WriteLine(e.Message);
			}
			catch (GeneralJpcspException)
			{
			}
			return success;
		}

		/// <summary>
		/// Don't call this directly, see loadUMD(File file)
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean loadUnpackedUMD(String filename) throws java.io.IOException, GeneralJpcspException
		private bool loadUnpackedUMD(string filename)
		{
			if (doUmdBuffering)
			{
				return false;
			}

			// Load unpacked BOOT.BIN as if it came from the umd
			File file = new File(filename);
			if (file.exists())
			{
				RandomAccessFile raf = new RandomAccessFile(file, "r");
				FileChannel roChannel = raf.Channel;
				ByteBuffer readbuffer = roChannel.map(FileChannel.MapMode.READ_ONLY, 0, (int) roChannel.size());
				emulator.load("disc0:/PSP_GAME/SYSDIR/EBOOT.BIN", readbuffer);
				raf.close();
				log.info("Using unpacked UMD EBOOT.BIN image");
				return true;
			}
			return false;
		}

		public virtual void loadAndRunUMD(File file)
		{
			loadUMD(file);

			if (!RunningFromVsh && !RunningReboot)
			{
				loadAndRun();
			}
		}

		public virtual void loadUMD(File file)
		{
			string filePath = file == null ? null : file.Path;
			UmdIsoReader.DoIsoBuffering = doUmdBuffering;
			Model.Model = Settings.Instance.readInt("emu.model");

			UmdIsoReader iso = null;
			bool closeIso = false;
			try
			{
				iso = new UmdIsoReader(filePath);
				logStartIso(iso);
				if (RunningReboot)
				{
					MMIOHandlerUmd.Instance.switchUmd(filePath);
				}
				else if (RunningFromVsh)
				{
					Modules.sceUmdUserModule.hleUmdSwitch(iso);
				}
				else
				{
					closeIso = true;
					if (iso.hasFile("PSP_GAME/param.sfo"))
					{
						loadUMDGame(file);
					}
					else if (iso.hasFile("UMD_VIDEO/param.sfo"))
					{
						loadUMDVideo(file);
					}
					else if (iso.hasFile("UMD_AUDIO/param.sfo"))
					{
						loadUMDAudio(file);
					}
					else
					{
						// Does the EBOOT.PBP contain an ELF file?
						sbyte[] pspData = iso.readPspData();
						if (pspData != null)
						{
							loadFile(file);
						}
					}
				}
			}
			catch (IOException e)
			{
				Console.WriteLine("loadUMD", e);
				closeIso = true;
			}
			finally
			{
				if (closeIso)
				{
					try
					{
						if (iso != null)
						{
							iso.close();
						}
					}
					catch (IOException e)
					{
						Console.WriteLine("loadUMD", e);
					}
				}
			}
			RefreshUI();
		}

		public virtual void switchUMD(File file)
		{
			try
			{
				UmdIsoReader iso = new UmdIsoReader(file.Path);
				if (!iso.hasFile("PSP_GAME/param.sfo"))
				{
					Console.WriteLine(string.Format("The UMD '{0}' is not a PSP_GAME UMD", file));
					return;
				}

				log.info(string.Format("Switching to the UMD {0}", file));

				Modules.sceUmdUserModule.hleUmdSwitch(iso);
			}
			catch (IOException e)
			{
				Console.WriteLine("switchUMD", e);
			}
		}

		public virtual void loadUMDGame(File file)
		{
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			string filePath = file == null ? null : file.Path;
			try
			{
				if (State.logWindow != null)
				{
					State.logWindow.clearScreenMessages();
				}
				logStart();

				Modules.SysMemUserForUserModule.reset();
				log.info(MetaInformation.FULL_NAME);

				umdLoaded = true;
				loadedFile = file;

				UmdIsoReader iso = new UmdIsoReader(filePath);

				// Dump unpacked PBP
				if (iso.PBP && Settings.Instance.readBool("emu.pbpunpack"))
				{
					PBP.unpackPBP(new LocalVirtualFile(new SeekableRandomFile(filePath, "r")));
				}

				UmdIsoFile psfFile = iso.getFile("PSP_GAME/param.sfo");

				PSF psf = new PSF();
				sbyte[] data = new sbyte[(int) psfFile.Length()];
				psfFile.read(data);
				psf.read(ByteBuffer.wrap(data));

				string title = psf.getPrintableString("TITLE");
				string discId = psf.getString("DISC_ID");
				string titleFormat = "%s - %s";
				if (string.ReferenceEquals(discId, null))
				{
					discId = State.DISCID_UNKNOWN_UMD;
				}
				else
				{
					titleFormat += " [%s]";
				}
				Title = string.format(titleFormat, MetaInformation.FULL_NAME, title, discId);

				addRecentUMD(file, title);

				if (psf.LikelyHomebrew)
				{
					emulator.setFirmwareVersion(Loader.FIRMWAREVERSION_HOMEBREW);
				}
				else
				{
					emulator.setFirmwareVersion(psf.getString("PSP_SYSTEM_VER"));
				}
				RuntimeContext.IsHomebrew = psf.LikelyHomebrew;

				State.discId = discId;

				State.umdId = null;
				try
				{
					UmdIsoFile umdDataBin = iso.getFile("UMD_DATA.BIN");
					if (umdDataBin != null)
					{
						sbyte[] buffer = new sbyte[(int) umdDataBin.Length()];
						umdDataBin.readFully(buffer);
						umdDataBin.Dispose();
						string umdDataBinContent = (StringHelper.NewString(buffer)).Replace((char) 0, ' ');

						string[] parts = umdDataBinContent.Split("\\|", true);
						if (parts != null && parts.Length >= 2)
						{
							State.umdId = parts[1];
						}
					}
				}
				catch (FileNotFoundException)
				{
					// Ignore exception
				}

				Settings.Instance.loadPatchSettings();

				// Set the memory model 32MB/64MB before loading the EBOOT.BIN
				int memorySize = Settings.Instance.readInt("memorySize", 0);
				if (memorySize > 0)
				{
					log.info(string.Format("Using memory size 0x{0:X} from settings for {1}", memorySize, State.discId));
					Modules.SysMemUserForUserModule.MemorySize = memorySize;
				}
				else
				{
					bool hasMemory64MB = psf.getNumeric("MEMSIZE") == 1;
					if (Settings.Instance.readBool("memory64MB"))
					{
						log.info(string.Format("Using 64MB memory from settings for {0}", State.discId));
						hasMemory64MB = true;
					}
					Modules.SysMemUserForUserModule.Memory64MB = hasMemory64MB;
				}

				if ((!discId.Equals(State.DISCID_UNKNOWN_UMD) && loadUnpackedUMD(discId + ".BIN")) || (!discId.Equals(State.DISCID_UNKNOWN_UMD) && loadUnpackedUMD(Settings.Instance.DiscTmpDirectory + "EBOOT.BIN")) || loadUMD(iso, "PSP_GAME/SYSDIR/EBOOT.OLD") || loadUMD(iso, "PSP_GAME/SYSDIR/EBOOT.BIN") || loadUMD(iso, "PSP_GAME/SYSDIR/BOOT.BIN"))
				{

					State.title = title;

					Modules.IoFileMgrForUserModule.setfilepath("disc0/");

					Modules.IoFileMgrForUserModule.IsoReader = iso;
					Modules.sceUmdUserModule.IsoReader = iso;

					if (instructioncounter != null)
					{
						instructioncounter.RefreshWindow();
					}
					StepLogger.clear();
					if (!string.ReferenceEquals(filePath, null))
					{
						StepLogger.Name = filePath;
					}
				}
				else
				{
					State.discId = State.DISCID_UNKNOWN_NOTHING_LOADED;
					throw new GeneralJpcspException(bundle.getString("MainGUI.strEncryptedBoot.text"));
				}
			}
			catch (GeneralJpcspException e)
			{
				JpcspDialogManager.showError(this, e.LocalizedMessage);
			}
			catch (IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				JpcspDialogManager.showError(this, bundle.getString("MainGUI.strIOError.text") + " : " + e.LocalizedMessage);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.Write(ex.StackTrace);
				if (ex.Message != null)
				{
					JpcspDialogManager.showError(this, bundle.getString("MainGUI.strCriticalError.text") + " : " + ex.LocalizedMessage);
				}
				else
				{
					JpcspDialogManager.showError(this, bundle.getString("MainGUI.strCriticalError.text") + " : " + bundle.getString("MainGUI.strCheckConsole.text"));
				}
			}
		}

		public virtual void loadUMDVideo(File file)
		{
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			string filePath = file == null ? null : file.Path;
			try
			{
				if (State.logWindow != null)
				{
					State.logWindow.clearScreenMessages();
				}
				logStart();
				Modules.SysMemUserForUserModule.reset();
				log.info(MetaInformation.FULL_NAME);

				umdLoaded = true;
				loadedFile = file;

				UmdIsoReader iso = new UmdIsoReader(filePath);
				UmdIsoFile psfFile = iso.getFile("UMD_VIDEO/param.sfo");
				UmdIsoFile umdDataFile = iso.getFile("UMD_DATA.BIN");

				PSF psf = new PSF();
				sbyte[] data = new sbyte[(int) psfFile.Length()];
				psfFile.read(data);
				psf.read(ByteBuffer.wrap(data));

				string title = psf.getPrintableString("TITLE");
				string discId = psf.getString("DISC_ID");
				if (string.ReferenceEquals(discId, null))
				{
					sbyte[] umdDataId = new sbyte[10];
					string umdDataIdString;
					umdDataFile.readFully(umdDataId, 0, 9);
					umdDataIdString = StringHelper.NewString(umdDataId);
					if (umdDataIdString.Equals(""))
					{
						discId = State.DISCID_UNKNOWN_UMD;
					}
					else
					{
						discId = umdDataIdString;
					}
				}

				Title = MetaInformation.FULL_NAME + " - " + title;
				addRecentUMD(file, title);

				emulator.setFirmwareVersion(psf.getString("PSP_SYSTEM_VER"));
				RuntimeContext.IsHomebrew = false;
				Modules.SysMemUserForUserModule.Memory64MB = psf.getNumeric("MEMSIZE") == 1;

				State.discId = discId;
				State.title = title;

				umdvideoplayer = new UmdVideoPlayer(this, iso);

				Modules.IoFileMgrForUserModule.setfilepath("disc0/");
				Modules.IoFileMgrForUserModule.IsoReader = iso;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.Write(ex.StackTrace);
				if (ex.Message != null)
				{
					JpcspDialogManager.showError(this, bundle.getString("MainGUI.strCriticalError.text") + ": " + ex.LocalizedMessage);
				}
				else
				{
					JpcspDialogManager.showError(this, bundle.getString("MainGUI.strCriticalError.text") + ": " + bundle.getString("MainGUI.strCheckConsole.text"));
				}
			}
		}

		public virtual void loadUMDAudio(File file)
		{
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			try
			{
				if (State.logWindow != null)
				{
					State.logWindow.clearScreenMessages();
				}
				logStart();
				Modules.SysMemUserForUserModule.reset();
				log.info(MetaInformation.FULL_NAME);

				umdLoaded = true;
				loadedFile = file;

				UmdIsoReader iso = new UmdIsoReader(file.Path);
				UmdIsoFile psfFile = iso.getFile("UMD_AUDIO/param.sfo");

				PSF psf = new PSF();
				sbyte[] data = new sbyte[(int) psfFile.Length()];
				psfFile.read(data);
				psf.read(ByteBuffer.wrap(data));

				string title = psf.getPrintableString("TITLE");
				string discId = psf.getString("DISC_ID");
				if (string.ReferenceEquals(discId, null))
				{
					discId = State.DISCID_UNKNOWN_UMD;
				}

				Title = MetaInformation.FULL_NAME + " - " + title;
				addRecentUMD(file, title);

				emulator.setFirmwareVersion(psf.getString("PSP_SYSTEM_VER"));
				RuntimeContext.IsHomebrew = false;
				Modules.SysMemUserForUserModule.Memory64MB = psf.getNumeric("MEMSIZE") == 1;

				State.discId = discId;
				State.title = title;
			}
			catch (System.ArgumentException)
			{
				// Ignore...
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.Write(ex.StackTrace);
				if (ex.Message != null)
				{
					JpcspDialogManager.showError(this, bundle.getString("MainGUI.strCriticalError.text") + ": " + ex.LocalizedMessage);
				}
				else
				{
					JpcspDialogManager.showError(this, bundle.getString("MainGUI.strCriticalError.text") + ": " + bundle.getString("MainGUI.strCheckConsole.text"));
				}
			}
		}

		private void logConfigurationSetting(string resourceKey, string settingKey, string value, bool textLeft, bool square)
		{
			bool isSettingFromPatch = string.ReferenceEquals(settingKey, null) ? false : Settings.Instance.isOptionFromPatch(settingKey);
			string format;
			if (isSettingFromPatch)
			{
				format = textLeft ? logConfigurationSettingLeftPatch : logConfigurationSettingRightPatch;
			}
			else
			{
				format = textLeft ? logConfigurationSettingLeft : logConfigurationSettingRight;
			}

			string text = resourceKey;
			try
			{
				java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp", new Locale("en"));
				text = bundle.getString(resourceKey);
			}
			catch (MissingResourceException)
			{
				// do nothing
			}
			log.info(string.format(format, text, value, square ? '[' : '(', square ? ']' : ')', "from patch file"));
		}

		private void logConfigurationSettingBool(string resourceKey, bool value, bool textLeft, bool square)
		{
			logConfigurationSetting(resourceKey, null, value ? "X" : " ", textLeft, square);
		}

		private void logConfigurationSettingBool(string resourceKey, string settingKey, bool textLeft, bool square)
		{
			bool value = Settings.Instance.readBool(settingKey);
			logConfigurationSetting(resourceKey, settingKey, value ? "X" : " ", textLeft, square);
		}

		private void logConfigurationSettingInt(string resourceKey, string settingKey, bool textLeft, bool square)
		{
			int value = Settings.Instance.readInt(settingKey);
			logConfigurationSetting(resourceKey, settingKey, Convert.ToString(value), textLeft, square);
		}

		private void logConfigurationSettingString(string resourceKey, string settingKey, bool textLeft, bool square)
		{
			string value = Settings.Instance.readString(settingKey);
			logConfigurationSetting(resourceKey, settingKey, value, textLeft, square);
		}

		private void logConfigurationSettingList(string resourceKey, string settingKey, string[] values, bool textLeft, bool square)
		{
			int valueIndex = Settings.Instance.readInt(settingKey);
			string value = Convert.ToString(valueIndex);
			if (values != null && valueIndex >= 0 && valueIndex < values.Length)
			{
				value = values[valueIndex];
			}
			logConfigurationSetting(resourceKey, settingKey, value, textLeft, square);
		}

		private void logConfigurationPanel(string resourceKey)
		{
			// jog here only in the English locale
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp", new Locale("en"));
			log.info(string.Format("{0} / {1}", bundle.getString("SettingsGUI.title"), bundle.getString(resourceKey)));
		}

		private void logDirectory(File dir, string prefix)
		{
			if (dir == null || !dir.exists())
			{
				return;
			}

			if (dir.Directory)
			{
				log.info(string.Format("{0}{1}:", prefix, dir.Name));
				File[] files = dir.listFiles();
				if (files != null)
				{
					foreach (File file in files)
					{
						logDirectory(file, prefix + "    ");
					}
				}
			}
			else
			{
				log.info(string.Format("{0}{1}, size=0x{2:X}", prefix, dir.Name, dir.Length()));
			}
		}

		private void logDirectory(string dirName)
		{
			File dir = new File(dirName);
			if (dir.exists())
			{
				log.info(string.Format("Contents of '{0}' directory:", dirName));
				logDirectory(dir, "  ");
			}
			else
			{
				log.info(string.Format("Non existing directory '{0}'", dirName));
			}
		}

		private void logStart()
		{
			log.info(string.Format("Java version: {0} ({1})", System.getProperty("java.version"), System.getProperty("java.runtime.version")));
			log.info(string.Format("Java library path: {0}", System.getProperty("java.library.path")));

			logConfigurationSettings();

			logDirectory(Settings.Instance.getDirectoryMapping("flash0"));
		}

		private void logStartIso(UmdIsoReader iso)
		{
			if (!log.InfoEnabled)
			{
				return;
			}

			string[] paramSfoFiles = new string[] {"PSP_GAME/param.sfo", "UMD_VIDEO/param.sfo", "UMD_AUDIO/param.sfo"};
			foreach (string paramSfoFile in paramSfoFiles)
			{
				if (iso.hasFile(paramSfoFile))
				{
					try
					{
						UmdIsoFile psfFile = iso.getFile(paramSfoFile);
						PSF psf = new PSF();
						sbyte[] data = new sbyte[(int) psfFile.Length()];
						psfFile.read(data);
						psf.read(ByteBuffer.wrap(data));

						log.info(string.Format("Content of {0}:{1}{2}", paramSfoFile, Environment.NewLine, psf));
					}
					catch (IOException)
					{
						// Ignore exception
					}
				}
			}

			try
			{
				UmdIsoFile umdDataBin = iso.getFile("UMD_DATA.BIN");
				if (umdDataBin != null)
				{
					sbyte[] buffer = new sbyte[(int) umdDataBin.Length()];
					umdDataBin.readFully(buffer);
					umdDataBin.Dispose();
					string umdDataBinContent = (StringHelper.NewString(buffer)).Replace((char) 0, ' ');
					log.info(string.Format("Content of UMD_DATA.BIN: '{0}'", umdDataBinContent));
				}
			}
			catch (FileNotFoundException)
			{
				// Ignore exception
			}
			catch (IOException)
			{
				// Ignore exception
			}
		}

		private void logConfigurationSettings()
		{
			if (!log.InfoEnabled)
			{
				return;
			}

			log.info("Using the following settings:");

			// Log the configuration settings
			logConfigurationPanel("SettingsGUI.RegionPanel.title");
			logConfigurationSettingList("SettingsGUI.languageLabel.text", sceUtility.SYSTEMPARAM_SETTINGS_OPTION_LANGUAGE, SettingsGUI.ImposeLanguages, true, true);
			logConfigurationSettingList("SettingsGUI.buttonLabel.text", sceUtility.SYSTEMPARAM_SETTINGS_OPTION_BUTTON_PREFERENCE, SettingsGUI.ImposeButtons, true, true);
			logConfigurationSettingList("SettingsGUI.daylightLabel.text", sceUtility.SYSTEMPARAM_SETTINGS_OPTION_DAYLIGHT_SAVING_TIME, SettingsGUI.SysparamDaylightSavings, true, true);
			logConfigurationSettingList("SettingsGUI.timeFormatLabel.text", sceUtility.SYSTEMPARAM_SETTINGS_OPTION_TIME_FORMAT, SettingsGUI.SysparamTimeFormats, true, true);
			logConfigurationSettingList("SettingsGUI.dateFormatLabel.text", sceUtility.SYSTEMPARAM_SETTINGS_OPTION_DATE_FORMAT, SettingsGUI.SysparamDateFormats, true, true);
			logConfigurationSettingList("SettingsGUI.wlanPowerLabel.text", sceUtility.SYSTEMPARAM_SETTINGS_OPTION_WLAN_POWER_SAVE, SettingsGUI.SysparamWlanPowerSaves, true, true);
			logConfigurationSettingList("SettingsGUI.adhocChannel.text", sceUtility.SYSTEMPARAM_SETTINGS_OPTION_ADHOC_CHANNEL, SettingsGUI.SysparamAdhocChannels, true, true);
			logConfigurationSettingInt("SettingsGUI.timezoneLabel.text", sceUtility.SYSTEMPARAM_SETTINGS_OPTION_TIME_ZONE, true, true);
			logConfigurationSettingString("SettingsGUI.nicknameLabel.text", sceUtility.SYSTEMPARAM_SETTINGS_OPTION_NICKNAME, true, true);

			logConfigurationPanel("SettingsGUI.VideoPanel.title");
			logConfigurationSettingBool("SettingsGUI.useOpenglRenderer.text", !Settings.Instance.readBool("emu.useSoftwareRenderer") && !Settings.Instance.readBool("emu.useExternalSoftwareRenderer"), false, false);
			logConfigurationSettingBool("SettingsGUI.useSoftwareRenderer.text", "emu.useSoftwareRenderer", false, false);
			logConfigurationSettingBool("SettingsGUI.useExternalSoftwareRenderer.text", "emu.useExternalSoftwareRenderer", false, false);
			logConfigurationSettingBool("SettingsGUI.disableVBOCheck.text", "emu.disablevbo", false, true);
			logConfigurationSettingBool("SettingsGUI.onlyGEGraphicsCheck.text", "emu.onlyGEGraphics", false, true);
			logConfigurationSettingBool("SettingsGUI.useVertexCache.text", "emu.useVertexCache", false, true);
			logConfigurationSettingBool("SettingsGUI.shaderCheck.text", "emu.useshaders", false, true);
			logConfigurationSettingBool("SettingsGUI.geometryShaderCheck.text", "emu.useGeometryShader", false, true);
			logConfigurationSettingBool("SettingsGUI.disableUBOCheck.text", "emu.disableubo", false, true);
			logConfigurationSettingBool("SettingsGUI.enableVAOCheck.text", "emu.enablevao", false, true);
			logConfigurationSettingBool("SettingsGUI.enableGETextureCheck.text", "emu.enablegetexture", false, true);
			logConfigurationSettingBool("SettingsGUI.enableNativeCLUTCheck.text", "emu.enablenativeclut", false, true);
			logConfigurationSettingBool("SettingsGUI.enableDynamicShadersCheck.text", "emu.enabledynamicshaders", false, true);
			logConfigurationSettingBool("SettingsGUI.enableShaderStencilTestCheck.text", "emu.enableshaderstenciltest", false, true);
			logConfigurationSettingBool("SettingsGUI.enableShaderColorMaskCheck.text", "emu.enableshadercolormask", false, true);
			logConfigurationSettingBool("SettingsGUI.disableOptimizedVertexInfoReading.text", "emu.disableoptimizedvertexinforeading", false, true);
			logConfigurationSettingBool("SettingsGUI.saveStencilToMemory.text", "emu.saveStencilToMemory", false, true);

			logConfigurationPanel("SettingsGUI.MemoryPanel.title");
			logConfigurationSettingBool("SettingsGUI.invalidMemoryCheck.text", "emu.ignoreInvalidMemoryAccess", false, true);
			logConfigurationSettingBool("SettingsGUI.ignoreUnmappedImports.text", "emu.ignoreUnmappedImports", false, true);
			logConfigurationSettingBool("SettingsGUI.useDebugMemory.text", "emu.useDebuggerMemory", false, true);

			logConfigurationPanel("SettingsGUI.CompilerPanel.title");
			logConfigurationSettingBool("SettingsGUI.useCompiler.text", "emu.compiler", false, true);
			logConfigurationSettingBool("SettingsGUI.profileCheck.text", "emu.profiler", false, true);
			logConfigurationSettingInt("SettingsGUI.methodMaxInstructionsLabel.text", "emu.compiler.methodMaxInstructions", false, true);

			logConfigurationPanel("SettingsGUI.DisplayPanel.title");
			logConfigurationSettingString("SettingsGUI.antiAliasLabel.text", "emu.graphics.antialias", true, true);
			logConfigurationSettingString("SettingsGUI.resolutionLabel.text", "emu.graphics.resolution", true, true);
			logConfigurationSettingBool("SettingsGUI.fullscreenCheck.text", "gui.fullscreen", false, true);

			logConfigurationPanel("SettingsGUI.MiscPanel.title");
			logConfigurationSettingBool("SettingsGUI.useDebugFont.text", "emu.useDebugFont", false, true);

			logConfigurationPanel("SettingsGUI.CryptoPanel.title");
			logConfigurationSettingBool("SettingsGUI.cryptoSavedata.text", "emu.cryptoSavedata", false, true);
			logConfigurationSettingBool("SettingsGUI.extractSavedataKey.text", "emu.extractSavedataKey", false, true);
			logConfigurationSettingBool("SettingsGUI.extractPGD.text", "emu.extractPGD", false, true);
			logConfigurationSettingBool("SettingsGUI.extractEboot.text", "emu.extractEboot", false, true);
			logConfigurationSettingBool("SettingsGUI.disableDLC.text", "emu.disableDLC", false, true);

			logConfigurationPanel("SettingsGUI.networkPanel.TabConstraints.tabTitle");
			logConfigurationSettingBool("SettingsGUI.lanMultiPlayerRadioButton.text", "emu.lanMultiPlayer", false, false);
			logConfigurationSettingBool("SettingsGUI.netServerPortShiftRadioButton.text", "emu.netServerPortShift", false, false);
			logConfigurationSettingBool("SettingsGUI.netClientPortShiftRadioButton.text", "emu.netClientPortShift", false, false);
			logConfigurationSettingBool("SettingsGUI.enableProOnlineRadioButton.text", "emu.enableProOnline", false, false);
			logConfigurationSettingString("SettingsGUI.metaServerLabel.text", "network.ProOnline.metaServer", true, true);
			logConfigurationSettingString("SettingsGUI.broadcastAddressLabel.text", "network.broadcastAddress", true, true);
		}

		public virtual void loadAndRun()
		{
			if (Settings.Instance.readBool("emu.loadAndRun"))
			{
				RunEmu();
			}
		}

		private void ResetButtonActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_ResetButtonActionPerformed
			reset();
		} //GEN-LAST:event_ResetButtonActionPerformed

		private void resetEmu()
		{
			if (loadedFile != null)
			{
				PauseEmu();
				RuntimeContext.reset();
				HLEModuleManager.Instance.stopModules();
				if (umdLoaded)
				{
					loadUMD(loadedFile);
				}
				else
				{
					loadFile(loadedFile);
				}
			}
		}
	private void InstructionCounterActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_InstructionCounterActionPerformed
			PauseEmu();
			if (State.instructionCounter == null)
			{
				State.instructionCounter = new InstructionCounter();
				emulator.InstructionCounter = State.instructionCounter;
			}
			else
			{
				State.instructionCounter.RefreshWindow();
			}
			startWindowDialog(State.instructionCounter);
	} //GEN-LAST:event_InstructionCounterActionPerformed

	private void FileLogActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_FileLogActionPerformed
			if (State.fileLogger == null)
			{
				State.fileLogger = new FileLoggerFrame();
			}
			startWindowDialog(State.fileLogger);
	} //GEN-LAST:event_FileLogActionPerformed

	private void VfpuRegistersActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_VfpuRegistersActionPerformed
			startWindowDialog(VfpuFrame.Instance);
	} //GEN-LAST:event_VfpuRegistersActionPerformed

	private void DumpIsoActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_DumpIsoActionPerformed
			if (umdLoaded)
			{
				UmdIsoReader iso = Modules.IoFileMgrForUserModule.IsoReader;
				if (iso != null)
				{
					try
					{
						iso.dumpIndexFile("iso-index.txt");
					}
					catch (IOException)
					{
						// Ignore Exception
					}
				}
			}
	} //GEN-LAST:event_DumpIsoActionPerformed

	private void ResetProfilerActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ResetProfilerActionPerformed
			Profiler.reset();
			GEProfiler.reset();
	} //GEN-LAST:event_ResetProfilerActionPerformed

	private void ClearTextureCacheActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ClearTextureCacheActionPerformed
			VideoEngine.Instance.clearTextureCache();
	} //GEN-LAST:event_ClearTextureCacheActionPerformed

	private void ClearVertexCacheActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ClearVertexCacheActionPerformed
			VideoEngine.Instance.clearVertexCache();
	} //GEN-LAST:event_ClearVertexCacheActionPerformed

	private void ExportISOFileActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ExportISOFileActionPerformed
			ResourceBundle bundle = ResourceBundle.getBundle("pspsharp/languages/pspsharp");
			string fileName = JOptionPane.showInputDialog(null, bundle.getString("MainGUI.ExportISOFileQuestion.text"), "disc0:/");
			if (string.ReferenceEquals(fileName, null))
			{
				// Input cancelled
				return;
			}

			SeekableDataInput input = Modules.IoFileMgrForUserModule.getFile(fileName, IoFileMgrForUser.PSP_O_RDONLY);
			if (input == null)
			{
				// File does not exit
				MessageBox.Show(null, bundle.getString("MainGUI.FileDoesNotExist.text"), null, MessageBoxIcon.Error);
				return;
			}

			string exportFileName = fileName;
			if (exportFileName.Contains("/"))
			{
				exportFileName = exportFileName.Substring(exportFileName.LastIndexOf('/') + 1);
			}
			if (exportFileName.Contains(":"))
			{
				exportFileName = exportFileName.Substring(exportFileName.LastIndexOf(':') + 1);
			}

			try
			{
				System.IO.Stream output = new System.IO.FileStream(exportFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				sbyte[] buffer = new sbyte[10 * 1024];
				long readLength = 0;
				long totalLength = input.Length();
				while (readLength < totalLength)
				{
					int Length = (int) System.Math.Min(totalLength - readLength, buffer.Length);
					input.readFully(buffer, 0, Length);
					output.Write(buffer, 0, Length);
					readLength += Length;
				}
				output.Close();
				input.Dispose();

				log.info(string.Format("Exported file '{0}' to '{1}'", fileName, exportFileName));
				string messageFormat = bundle.getString("MainGUI.FileExported.text");
				string message = MessageFormat.format(messageFormat, fileName, exportFileName);
				MessageBox.Show(null, message, null, MessageBoxIcon.Information);
			}
			catch (IOException e)
			{
				Console.WriteLine(e);
			}

	} //GEN-LAST:event_ExportISOFileActionPerformed

	private void ShotItemActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ShotItemActionPerformed
			if (umdvideoplayer != null)
			{
				umdvideoplayer.takeScreenshot();
			}
			Modules.sceDisplayModule.takeScreenshot();
	} //GEN-LAST:event_ShotItemActionPerformed

	private void RotateItemActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_RotateItemActionPerformed
			sceDisplay screen = Modules.sceDisplayModule;
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N

			IList<object> options = new List<object>();
			options.Add(bundle.getString("MainGUI.strRotate90CW.text"));
			options.Add(bundle.getString("MainGUI.strRotate90CCW.text"));
			options.Add(bundle.getString("MainGUI.strRotate180.text"));
			options.Add(bundle.getString("MainGUI.strRotateMirror.text"));
			options.Add(bundle.getString("MainGUI.strRotateNormal.text"));

			int jop = JOptionPane.showOptionDialog(null, bundle.getString("MainGUI.strChooseRotation.text"), bundle.getString("MainGUI.strRotate.text"), JOptionPane.UNDEFINED_CONDITION, JOptionPane.QUESTION_MESSAGE, null, options.ToArray(), options[4]);
			if (jop != -1)
			{
				screen.rotate(jop);
			}
	} //GEN-LAST:event_RotateItemActionPerformed

	private void ExportVisibleElementsActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ExportVisibleElementsActionPerformed
			State.exportGeNextFrame = true;
			State.exportGeOnlyVisibleElements = true;
	} //GEN-LAST:event_ExportVisibleElementsActionPerformed

	private void ExportAllElementsActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ExportAllElementsActionPerformed
			State.exportGeNextFrame = true;
			State.exportGeOnlyVisibleElements = false;
	} //GEN-LAST:event_ExportAllElementsActionPerformed

		private string StateFileName
		{
			get
			{
				if (RuntimeContextLLE.LLEActive)
				{
					return string.Format("State.bin");
				}
				return string.Format("State_{0}.bin", State.discId);
			}
		}

	private void SaveSnapActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_SaveSnapActionPerformed
		try
		{
			(new pspsharp.state.State()).write(StateFileName);
		}
		catch (IOException e)
		{
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
		}
	} //GEN-LAST:event_SaveSnapActionPerformed

	private void LoadSnapActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_LoadSnapActionPerformed
		try
		{
			(new pspsharp.state.State()).read(StateFileName);
		}
		catch (IOException e)
		{
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
		}
	} //GEN-LAST:event_LoadSnapActionPerformed

	private void EnglishUSActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_EnglishUSActionPerformed
			changeLanguage("en_US");
	} //GEN-LAST:event_EnglishUSActionPerformed

	private void FrenchActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_FrenchActionPerformed
			changeLanguage("fr_FR");
	} //GEN-LAST:event_FrenchActionPerformed

	private void GermanActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_GermanActionPerformed
			changeLanguage("de_DE");
	} //GEN-LAST:event_GermanActionPerformed

	private void LithuanianActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_LithuanianActionPerformed
			changeLanguage("lt_LT");
	} //GEN-LAST:event_LithuanianActionPerformed

	private void SpanishActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_SpanishActionPerformed
			changeLanguage("es_ES");
	} //GEN-LAST:event_SpanishActionPerformed

	private void CatalanActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_CatalanActionPerformed
			changeLanguage("ca_ES");
	} //GEN-LAST:event_CatalanActionPerformed

	private void PortugueseActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_PortugueseActionPerformed
			changeLanguage("pt_PT");
	} //GEN-LAST:event_PortugueseActionPerformed

	private void JapaneseActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_JapaneseActionPerformed
			changeLanguage("ja_JP");
	} //GEN-LAST:event_JapaneseActionPerformed

	private void PortugueseBRActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_PortugueseBRActionPerformed
			changeLanguage("pt_BR");
	} //GEN-LAST:event_PortugueseBRActionPerformed

	private void cwcheatActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_cwcheatActionPerformed
			if (State.cheatsGUI == null)
			{
				State.cheatsGUI = new CheatsGUI();
			}
			startWindowDialog(State.cheatsGUI);
	} //GEN-LAST:event_cwcheatActionPerformed

	private void RussianActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_RussianActionPerformed
			changeLanguage("ru_RU");
	} //GEN-LAST:event_RussianActionPerformed

	private void PolishActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_PolishActionPerformed
			changeLanguage("pl_PL");
	} //GEN-LAST:event_PolishActionPerformed

	private void ItalianActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ItalianActionPerformed
			changeLanguage("it_IT");
	} //GEN-LAST:event_ItalianActionPerformed

	private void GreekActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_GreekActionPerformed
			changeLanguage("el_GR");
	} //GEN-LAST:event_GreekActionPerformed

	private void ControlsConfActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ControlsConfActionPerformed
			startWindowDialog(new ControlsGUI());
	} //GEN-LAST:event_ControlsConfActionPerformed

	private void MuteOptActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_MuteOptActionPerformed
			Audio.Muted = !Audio.Muted;
			MuteOpt.Selected = Audio.Muted;
	} //GEN-LAST:event_MuteOptActionPerformed

	private void ClockSpeedNormalActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ClockSpeedNormalActionPerformed
			// Set clock speed to 1/1
			Emulator.setVariableSpeedClock(1, 1);
	} //GEN-LAST:event_ClockSpeedNormalActionPerformed

	private void ClockSpeed50ActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ClockSpeed50ActionPerformed
			// Set clock speed to 1/2
			Emulator.setVariableSpeedClock(1, 2);
	} //GEN-LAST:event_ClockSpeed50ActionPerformed

	private void ClockSpeed75ActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ClockSpeed75ActionPerformed
			// Set clock speed to 3/4
			Emulator.setVariableSpeedClock(3, 4);
	} //GEN-LAST:event_ClockSpeed75ActionPerformed

	private void ClockSpeed150ActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ClockSpeed150ActionPerformed
			// Set clock speed to 3/2
			Emulator.setVariableSpeedClock(3, 2);
	} //GEN-LAST:event_ClockSpeed150ActionPerformed

	private void ClockSpeed200ActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ClockSpeed200ActionPerformed
			// Set clock speed to 2/1
			Emulator.setVariableSpeedClock(2, 1);
	} //GEN-LAST:event_ClockSpeed200ActionPerformed

	private void ClockSpeed300ActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ClockSpeed300ActionPerformed
			// Set clock speed to 3/1
			Emulator.setVariableSpeedClock(3, 1);
	} //GEN-LAST:event_ClockSpeed300ActionPerformed

	private void ToggleLoggerActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ToggleLoggerActionPerformed
			if (!State.logWindow.Visible)
			{
				updateConsoleWinPosition();
			}
			State.logWindow.Visible = !State.logWindow.Visible;
			ToggleLogger.Selected = State.logWindow.Visible;
	} //GEN-LAST:event_ToggleLoggerActionPerformed

	private void CustomLoggerActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_CustomLoggerActionPerformed
			if (State.logGUI == null)
			{
				State.logGUI = new LogGUI(this);
			}
			startWindowDialog(State.logGUI);
	} //GEN-LAST:event_CustomLoggerActionPerformed

		private void ChinesePRCActionPerformed(ActionEvent evt)
		{
			changeLanguage("zh_CN");
		}

	private void ChineseTWActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ChinesePRCActionPerformed
			changeLanguage("zh_TW");
	} //GEN-LAST:event_ChinesePRCActionPerformed

	private void noneCheckActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_noneCheckActionPerformed
			VideoEngine.Instance.UseTextureAnisotropicFilter = false;
			Settings.Instance.writeBool("emu.graphics.filters.anisotropic", false);
	} //GEN-LAST:event_noneCheckActionPerformed

	private void anisotropicCheckActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_anisotropicCheckActionPerformed
			VideoEngine.Instance.UseTextureAnisotropicFilter = true;
			Settings.Instance.writeBool("emu.graphics.filters.anisotropic", anisotropicCheck.Selected);
	} //GEN-LAST:event_anisotropicCheckActionPerformed

	private void frameSkipNoneActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_frameSkipNoneActionPerformed
			Modules.sceDisplayModule.DesiredFPS = 0;
			Settings.Instance.writeInt("emu.graphics.frameskip.desiredFPS", Modules.sceDisplayModule.DesiredFPS);
	} //GEN-LAST:event_frameSkipNoneActionPerformed

	private void frameSkipFPS5ActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_frameSkipFPS5ActionPerformed
			Modules.sceDisplayModule.DesiredFPS = 5;
			Settings.Instance.writeInt("emu.graphics.frameskip.desiredFPS", Modules.sceDisplayModule.DesiredFPS);
	} //GEN-LAST:event_frameSkipFPS5ActionPerformed

	private void frameSkipFPS10ActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_frameSkipFPS10ActionPerformed
			Modules.sceDisplayModule.DesiredFPS = 10;
			Settings.Instance.writeInt("emu.graphics.frameskip.desiredFPS", Modules.sceDisplayModule.DesiredFPS);
	} //GEN-LAST:event_frameSkipFPS10ActionPerformed

	private void frameSkipFPS15ActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_frameSkipFPS15ActionPerformed
			Modules.sceDisplayModule.DesiredFPS = 15;
			Settings.Instance.writeInt("emu.graphics.frameskip.desiredFPS", Modules.sceDisplayModule.DesiredFPS);
	} //GEN-LAST:event_frameSkipFPS15ActionPerformed

	private void frameSkipFPS20ActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_frameSkipFPS20ActionPerformed
			Modules.sceDisplayModule.DesiredFPS = 20;
			Settings.Instance.writeInt("emu.graphics.frameskip.desiredFPS", Modules.sceDisplayModule.DesiredFPS);
	} //GEN-LAST:event_frameSkipFPS20ActionPerformed

	private void frameSkipFPS30ActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_frameSkipFPS30ActionPerformed
			Modules.sceDisplayModule.DesiredFPS = 30;
			Settings.Instance.writeInt("emu.graphics.frameskip.desiredFPS", Modules.sceDisplayModule.DesiredFPS);
	} //GEN-LAST:event_frameSkipFPS30ActionPerformed

	private void frameSkipFPS60ActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_frameSkipFPS60ActionPerformed
			Modules.sceDisplayModule.DesiredFPS = 60;
			Settings.Instance.writeInt("emu.graphics.frameskip.desiredFPS", Modules.sceDisplayModule.DesiredFPS);
	} //GEN-LAST:event_frameSkipFPS60ActionPerformed

		private int ViewportResizeScaleFactor
		{
			set
			{
				Modules.sceDisplayModule.ViewportResizeScaleFactor = value;
				pack();
				if (umdvideoplayer != null)
				{
					umdvideoplayer.pauseVideo();
					umdvideoplayer.setVideoPlayerResizeScaleFactor(this, value);
					umdvideoplayer.resumeVideo();
				}
			}
		}

	private void oneTimeResizeActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_oneTimeResizeActionPerformed
			ViewportResizeScaleFactor = 1;
	} //GEN-LAST:event_oneTimeResizeActionPerformed

	private void twoTimesResizeActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_twoTimesResizeActionPerformed
			ViewportResizeScaleFactor = 2;
	} //GEN-LAST:event_twoTimesResizeActionPerformed

	private void threeTimesResizeActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_threeTimesResizeActionPerformed
			ViewportResizeScaleFactor = 3;
	} //GEN-LAST:event_threeTimesResizeActionPerformed

		private void SystemLocaleActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_SystemLocaleActionPerformed
			changeLanguage("systemLocale");
		} //GEN-LAST:event_SystemLocaleActionPerformed

		private void formComponentResized(ComponentEvent evt)
		{ //GEN-FIRST:event_formComponentResized
			updateConsoleWinPosition();
		} //GEN-LAST:event_formComponentResized

		private void EnglishGBActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_EnglishGBActionPerformed
			changeLanguage("en_GB");
		} //GEN-LAST:event_EnglishGBActionPerformed

		private void xbrzCheckActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_xbrzCheckActionPerformed
			Settings.Instance.writeBool("emu.plugins.xbrz", xbrzCheck.Selected);
		} //GEN-LAST:event_xbrzCheckActionPerformed

		private void exitEmu()
		{
			if (umdvideoplayer != null)
			{
				umdvideoplayer.exit();
			}
			ProOnlineNetworkAdapter.exit();
			Modules.ThreadManForUserModule.exit();
			Modules.sceDisplayModule.exit();
			Modules.IoFileMgrForUserModule.exit();
			VideoEngine.exit();
			Screen.exit();
			Emulator.exit();

			Environment.Exit(0);
		}

		public virtual void updateConsoleWinPosition()
		{
			if (Settings.Instance.readBool("gui.snapLogwindow"))
			{
				Point mainwindowPos = Location;
				State.logWindow.setLocation(mainwindowPos.x, mainwindowPos.y + Height);
			}
		}

		private void RunEmu()
		{
			emulator.RunEmu();
			Modules.sceDisplayModule.Canvas.requestFocusInWindow();
		}

		private void TogglePauseEmu()
		{
			// This is a toggle, so can pause and unpause
			if (Emulator.run_Renamed)
			{
				if (!Emulator.pause)
				{
					Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_PAUSE);
				}
				else
				{
					RunEmu();
				}
			}
		}

		private void PauseEmu()
		{
			// This will only enter pause mode
			if (Emulator.run_Renamed && !Emulator.pause)
			{
				Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_PAUSE);
			}
		}

		public virtual void RefreshButtons()
		{
			RunButton.Selected = Emulator.run_Renamed && !Emulator.pause;
			PauseButton.Selected = Emulator.run_Renamed && Emulator.pause;
		}

		public void onUmdChange()
		{
			// Only enable the menu entry "Switch UMD" when sceUmdReplacePermit has been called by the application.
			switchUmd.Enabled = Modules.sceUmdUserModule.UmdAllowReplace;
		}

		/// <summary>
		/// set the FPS portion of the title
		/// </summary>
		public virtual string MainTitle
		{
			set
			{
				string oldtitle = Title;
				int sub = oldtitle.IndexOf("FPS:", StringComparison.Ordinal);
				if (sub != -1)
				{
					string newtitle = oldtitle.Substring(0, sub - 1);
					Title = newtitle + " " + value;
				}
				else
				{
					Title = oldtitle + " " + value;
				}
			}
		}

		public static File[] getUmdPaths(bool ignorePSPGame)
		{
			IList<File> umdPaths = new LinkedList<File>();
			umdPaths.Add(new File(Settings.Instance.readString("emu.umdpath") + "/"));
			for (int i = 1; true; i++)
			{
				string umdPath = Settings.Instance.readString(string.Format("emu.umdpath.{0:D}", i), null);
				if (string.ReferenceEquals(umdPath, null))
				{
					break;
				}

				if (!ignorePSPGame || !(umdPath.Equals("ms0\\PSP\\GAME") || umdPath.Equals(Settings.Instance.getDirectoryMapping("ms0") + "PSP/GAME")))
				{
					umdPaths.Add(new File(umdPath + "/"));
				}
			}

			return umdPaths.ToArray();
		}

		private void printUsage()
		{
			string javaLibraryPath = System.getProperty("java.library.path");
			if (string.ReferenceEquals(javaLibraryPath, null) || javaLibraryPath.Length == 0)
			{
				javaLibraryPath = "lib/windows-amd64";
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.PrintStream out = System.err;
			PrintStream @out = System.err;
			@out.println(string.Format("Usage: java -Xmx1024m -Xss2m -XX:ReservedCodeCacheSize=64m -Djava.library.path={0} -jar bin/pspsharp.jar [OPTIONS]", javaLibraryPath));
			@out.println();
			@out.println("  -d, --debugger             Open debugger at start.");
			@out.println("  -f, --loadfile FILE        Load a file.");
			@out.println("                             Example: ms0/PSP/GAME/pspsolitaire/EBOOT.PBP");
			@out.println("  -u, --loadumd FILE         Load a UMD. Example: umdimages/cube.iso");
			@out.println("  -r, --run                  Run loaded file or umd. Use with -f or -u option.");
			@out.println("  -t, --tests                Run the automated tests.");
			@out.println("  --netClientPortShift N     Increase Network client ports by N (when running 2 pspsharp on the same computer)");
			@out.println("  --netServerPortShift N     Increase Network server ports by N (when running 2 pspsharp on the same computer)");
			@out.println("  --flash0 DIRECTORY         Use the given directory name for the PSP flash0:  device, instead of \"flash0/\"  by default.");
			@out.println("  --flash1 DIRECTORY         Use the given directory name for the PSP flash1:  device, instead of \"flash1/\"  by default.");
			@out.println("  --flash2 DIRECTORY         Use the given directory name for the PSP flash2:  device, instead of \"flash2/\"  by default.");
			@out.println("  --ms0 DIRECTORY            Use the given directory name for the PSP ms0:     device, instead of \"ms0/\"     by default.");
			@out.println("  --exdata0 DIRECTORY        Use the given directory name for the PSP exdata0: device, instead of \"exdata0/\" by default.");
			@out.println("  --logsettings FILE         Use the given file for the log4j configuration, instead of \"LogSettings.xml\" by default.");
			@out.println("  --vsh                      Run the PSP VSH.");
			@out.println("  --reboot                   Run a low-level emulation of the complete PSP reboot process. Still experimental.");
		}

		private void processArgs(string[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].Equals("-t") || args[i].Equals("--tests"))
				{
					throw (new Exception("Shouldn't get there"));
				}
				else if (args[i].Equals("-d") || args[i].Equals("--debugger"))
				{
					EnterDebuggerActionPerformed(null);
				}
				else if (args[i].Equals("-f") || args[i].Equals("--loadfile"))
				{
					i++;
					if (i < args.Length)
					{
						File file = new File(args[i]);
						if (file.exists())
						{
							Modules.sceDisplayModule.setCalledFromCommandLine();
							loadFile(file);
						}
					}
					else
					{
						printUsage();
						break;
					}
				}
				else if (args[i].Equals("-u") || args[i].Equals("--loadumd"))
				{
					i++;
					if (i < args.Length)
					{
						File file = new File(args[i]);
						if (file.exists())
						{
							Modules.sceDisplayModule.setCalledFromCommandLine();
							loadUMD(file);
						}
					}
					else
					{
						printUsage();
						break;
					}
				}
				else if (args[i].Equals("--bufferumd"))
				{
					doUmdBuffering = true;
				}
				else if (args[i].Equals("--loadbufferedumd"))
				{
					doUmdBuffering = true;
					Modules.sceDisplayModule.setCalledFromCommandLine();
					loadUMD(null);
				}
				else if (args[i].Equals("-r") || args[i].Equals("--run"))
				{
					RunEmu();
				}
				else if (args[i].Equals("--netClientPortShift"))
				{
					i++;
					if (i < args.Length)
					{
						int netClientPortShift = int.Parse(args[i]);
						Modules.sceNetAdhocModule.NetClientPortShift = netClientPortShift;
					}
					else
					{
						printUsage();
						break;
					}
				}
				else if (args[i].Equals("--netServerPortShift"))
				{
					i++;
					if (i < args.Length)
					{
						int netServerPortShift = int.Parse(args[i]);
						Modules.sceNetAdhocModule.NetServerPortShift = netServerPortShift;
					}
					else
					{
						printUsage();
						break;
					}
				}
				else if (args[i].Equals("--ProOnline"))
				{
					ProOnlineNetworkAdapter.Enabled = true;
				}
				else if (args[i].Equals("--vsh"))
				{
					runFromVsh = true;
					logStart();
					Title = MetaInformation.FULL_NAME + " - VSH";
					Emulator.Instance.FirmwareVersion = 660;
					Modules.sceDisplayModule.setCalledFromCommandLine();
					HTTPServer.processProxyRequestLocally = true;

					if (!Modules.rebootModule.loadAndRun())
					{
						loadFile(new File(Settings.Instance.getDirectoryMapping("flash0") + "vsh/module/vshmain.prx"), true);
					}

					Modules.IoFileMgrForUserModule.setfilepath(Settings.Instance.getDirectoryMapping("ms0") + "PSP/GAME");

					// Start VSH with the lowest priority so that the initialization of the other
					// modules can be completed.
					// The VSH root thread is running in KERNEL mode.
					SceKernelThreadInfo rootThread = Modules.ThreadManForUserModule.getRootThread(null);
					if (rootThread != null)
					{
						rootThread.currentPriority = 0x7E;
						rootThread.attr |= SceKernelThreadInfo.PSP_THREAD_ATTR_KERNEL;
						rootThread.attr &= ~SceKernelThreadInfo.PSP_THREAD_ATTR_USER;
					}

					HLEModuleManager.Instance.LoadFlash0Module("PSP_MODULE_AV_VAUDIO");
					HLEModuleManager.Instance.LoadFlash0Module("PSP_MODULE_AV_ATRAC3PLUS");
					HLEModuleManager.Instance.LoadFlash0Module("PSP_MODULE_AV_AVCODEC");
				}
				else if (args[i].Equals("--reboot"))
				{
					reboot.enableReboot = true;
					logStart();
					Title = MetaInformation.FULL_NAME + " - reboot";
					Modules.sceDisplayModule.setCalledFromCommandLine();
					HTTPServer.processProxyRequestLocally = true;

					if (!Modules.rebootModule.loadAndRun())
					{
						Console.WriteLine(string.Format("Cannot reboot - missing files"));
						reboot.enableReboot = false;
					}
				}
				else if (args[i].Equals("--debugCodeBlockCalls"))
				{
					RuntimeContext.debugCodeBlockCalls = true;
				}
				else if (args[i].matches("--flash[0-2]") || args[i].matches("--ms[0]") || args[i].matches("--exdata[0]"))
				{
					string directoryName = args[i].Substring(2);
					i++;
					if (i < args.Length)
					{
						string mappedDirectoryName = args[i];
						// The mapped directory name must end with "/"
						if (!mappedDirectoryName.EndsWith("/", StringComparison.Ordinal))
						{
							mappedDirectoryName += "/";
						}
						Settings.Instance.setDirectoryMapping(directoryName, mappedDirectoryName);
						log.info(string.Format("Mapping '{0}' to directory '{1}'", directoryName, mappedDirectoryName));
					}
					else
					{
						printUsage();
						break;
					}
				}
				else if (args[i].Equals("--logsettings"))
				{
					// This argument has already been processed in initLog()
					i++;
				}
				else
				{
					printUsage();
					break;
				}
			}
		}

		private static void initLog(string[] args)
		{
			string logSettingsFileName = "LogSettings.xml";

			// Verify if another LogSettings.xml file name has been provided on the command line
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].Equals("--logsettings"))
				{
					i++;
					logSettingsFileName = args[i];
				}
			}

			DOMConfigurator.configure(logSettingsFileName);
			setLog4jMDC();
		}

		/// <param name="args"> the command line arguments </param>
		public static void Main(string[] args)
		{
			initLog(args);

			// Re-enable all disabled algorithms as the PSP is allowing them
			Security.setProperty("jdk.certpath.disabledAlgorithms", "");
			Security.setProperty("jdk.tls.disabledAlgorithms", "");

			PreDecrypt.init();
			AES128.init();

			HTTPServer.Instance;

			// prepare i18n
			string locale = Settings.Instance.readString("emu.language");
			if (!locale.Equals("systemLocale"))
			{
				// extract language and country for Locale()
				string language = locale.Substring(0, 2);
				string country = locale.Substring(3, 2);

				Locale.Default = new Locale(language, country);
				ResourceBundle.clearCache();
			}

			if (args.Length > 0)
			{
				if (args[0].Equals("--tests"))
				{
					(new AutoTestsRunner()).run();
					return;
				}
			}

			try
			{
				UIManager.LookAndFeel = UIManager.SystemLookAndFeelClassName;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			// add the window property saver class to the event listeners for
			// automatic persistent saving of the window positions if needed
			Toolkit.DefaultToolkit.addAWTEventListener(new WindowPropSaver(), AWTEvent.WINDOW_EVENT_MASK);

			// final copy of args for use in inner class
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] fargs = args;
			string[] fargs = args;

			EventQueue.invokeLater(() =>
			{
			Thread.CurrentThread.Name = "GUI";
			MainGUI maingui = new MainGUI();
			maingui.Visible = true;

			if (Settings.Instance.readBool("gui.openLogwindow"))
			{
				State.logWindow.Visible = true;
				maingui.ToggleLogger.Selected = true;
			}

			maingui.processArgs(fargs);
			});
		}

		public virtual bool FullScreen
		{
			get
			{
				return useFullscreen;
			}
		}
		// Variables declaration - do not modify//GEN-BEGIN:variables
		private javax.swing.JMenuItem About;
		private javax.swing.JMenu AudioOpt;
		private javax.swing.JMenuItem Catalan;
		private javax.swing.JMenu CheatsMenu;
		private javax.swing.JMenuItem ChinesePRC;
		private javax.swing.JMenuItem ChineseTW;
		private javax.swing.JMenuItem ClearTextureCache;
		private javax.swing.JMenuItem ClearVertexCache;
		private javax.swing.JCheckBoxMenuItem ClockSpeed150;
		private javax.swing.JCheckBoxMenuItem ClockSpeed200;
		private javax.swing.JCheckBoxMenuItem ClockSpeed300;
		private javax.swing.JCheckBoxMenuItem ClockSpeed50;
		private javax.swing.JCheckBoxMenuItem ClockSpeed75;
		private javax.swing.JCheckBoxMenuItem ClockSpeedNormal;
		private javax.swing.JMenu ClockSpeedOpt;
		private javax.swing.JMenuItem ConfigMenu;
		private javax.swing.JMenuItem ControlsConf;
		private javax.swing.JMenuItem CustomLogger;
		private javax.swing.JMenu DebugMenu;
		private javax.swing.JMenuItem DumpIso;
		private javax.swing.JMenuItem ElfHeaderViewer;
		private javax.swing.JMenuItem EnglishGB;
		private javax.swing.JMenuItem EnglishUS;
		private javax.swing.JMenuItem EnterDebugger;
		private javax.swing.JMenuItem EnterImageViewer;
		private javax.swing.JMenuItem EnterMemoryViewer;
		private javax.swing.JMenuItem ExitEmu;
		private javax.swing.JMenuItem ExportAllElements;
		private javax.swing.JMenuItem ExportISOFile;
		private javax.swing.JMenu ExportMenu;
		private javax.swing.JMenuItem ExportVisibleElements;
		private javax.swing.JCheckBoxMenuItem FPS10;
		private javax.swing.JCheckBoxMenuItem FPS15;
		private javax.swing.JCheckBoxMenuItem FPS20;
		private javax.swing.JCheckBoxMenuItem FPS30;
		private javax.swing.JCheckBoxMenuItem FPS5;
		private javax.swing.JCheckBoxMenuItem FPS60;
		private javax.swing.JMenuItem FileLog;
		private javax.swing.JMenu FileMenu;
		private javax.swing.JMenu FiltersMenu;
		private javax.swing.JMenu FrameSkipMenu;
		private javax.swing.JCheckBoxMenuItem FrameSkipNone;
		private javax.swing.JMenuItem French;
		private javax.swing.JMenuItem German;
		private javax.swing.JMenuItem Greek;
		private javax.swing.JMenu HelpMenu;
		private javax.swing.JMenuItem pspsharp;
		private javax.swing.JMenuItem Italian;
		private javax.swing.JMenuItem Japanese;
		private javax.swing.JMenu LanguageMenu;
		private javax.swing.JMenuItem Lithuanian;
		private javax.swing.JMenuItem LoadSnap;
		private javax.swing.JMenu LoggerMenu;
		private javax.swing.JMenuBar MenuBar;
		private javax.swing.JCheckBoxMenuItem MuteOpt;
		private javax.swing.JMenuItem OpenFile;
		private javax.swing.JMenu OptionsMenu;
		private javax.swing.JToggleButton PauseButton;
		private javax.swing.JMenu PluginsMenu;
		private javax.swing.JMenuItem Polish;
		private javax.swing.JMenuItem Portuguese;
		private javax.swing.JMenuItem PortugueseBR;
		private javax.swing.JMenu RecentMenu;
		private javax.swing.JButton ResetButton;
		private javax.swing.JMenuItem ResetProfiler;
		private javax.swing.JMenu ResizeMenu;
		private javax.swing.JMenuItem RotateItem;
		private javax.swing.JToggleButton RunButton;
		private javax.swing.JMenuItem Russian;
		private javax.swing.JMenuItem SaveSnap;
		private javax.swing.JMenuItem ShotItem;
		private javax.swing.JMenuItem Spanish;
		private javax.swing.JMenuItem SystemLocale;
		private javax.swing.JCheckBoxMenuItem ToggleLogger;
		private javax.swing.JMenu ToolsSubMenu;
		private javax.swing.JMenuItem VfpuRegisters;
		private javax.swing.JMenu VideoOpt;
		private javax.swing.JCheckBoxMenuItem anisotropicCheck;
		private javax.swing.ButtonGroup clockSpeedGroup;
		private javax.swing.JMenuItem cwcheat;
		private javax.swing.ButtonGroup filtersGroup;
		private javax.swing.ButtonGroup frameSkipGroup;
		private javax.swing.JSeparator jSeparator1;
		private javax.swing.JSeparator jSeparator2;
		private javax.swing.JToolBar mainToolBar;
		private javax.swing.JCheckBoxMenuItem noneCheck;
		private javax.swing.JCheckBoxMenuItem oneTimeResize;
		private javax.swing.JMenuItem openUmd;
		private javax.swing.ButtonGroup resGroup;
		private javax.swing.JMenuItem switchUmd;
		private javax.swing.JMenuItem ejectMs;
		private javax.swing.JCheckBoxMenuItem threeTimesResize;
		private javax.swing.JCheckBoxMenuItem twoTimesResize;
		private javax.swing.JCheckBoxMenuItem xbrzCheck;
		// End of variables declaration//GEN-END:variables

		private bool userChooseSomething(int returnVal)
		{
			return returnVal == JFileChooser.APPROVE_OPTION;
		}

		public override void mousePressed(MouseEvent @event)
		{
			if (useFullscreen && @event.PopupTrigger)
			{
				fullScreenMenu.show(@event.Component, @event.X, @event.Y);
			}
		}

		public override void mouseReleased(MouseEvent @event)
		{
			if (useFullscreen && @event.PopupTrigger)
			{
				fullScreenMenu.show(@event.Component, @event.X, @event.Y);
			}
		}

		public override void mouseClicked(MouseEvent @event)
		{
		}

		public override void mouseEntered(MouseEvent @event)
		{
		}

		public override void mouseExited(MouseEvent @event)
		{
		}

		public override void keyTyped(KeyEvent @event)
		{
		}

		public override void keyPressed(KeyEvent @event)
		{
			State.controller.keyPressed(@event);

			// check if the stroke is a known accelerator and call the associated ActionListener(s)
			KeyStroke stroke = KeyStroke.getKeyStroke(@event.KeyCode, @event.Modifiers);
			if (actionListenerMap.ContainsKey(stroke))
			{
				foreach (ActionListener al in actionListenerMap[stroke])
				{
					al.actionPerformed(new ActionEvent(@event.Source, @event.ID, ""));
				}
			}
		}

		public override void keyReleased(KeyEvent @event)
		{
			State.controller.keyReleased(@event);
		}

		public override void componentHidden(ComponentEvent e)
		{
		}

		public override void componentMoved(ComponentEvent e)
		{
			if (State.logWindow.Visible)
			{
				updateConsoleWinPosition();
			}
		}

		public override void componentResized(ComponentEvent e)
		{
		}

		public override void componentShown(ComponentEvent e)
		{
		}

		private class RecentElementActionListener : ActionListener
		{
			private readonly MainGUI outerInstance;


			public const int TYPE_UMD = 0;
			public const int TYPE_FILE = 1;
			internal int type;
			internal string path;
			internal Component parent;

			public RecentElementActionListener(MainGUI outerInstance, Component parent, int type, string path)
			{
				this.outerInstance = outerInstance;
				this.parent = parent;
				this.path = path;
				this.type = type;
			}

			public override void actionPerformed(ActionEvent e)
			{
				File file = new File(path);
				if (file.exists())
				{
					if (type == TYPE_UMD)
					{
						outerInstance.loadUMD(file);
					}
					else
					{
						outerInstance.loadFile(file);
					}
					outerInstance.loadAndRun();
				}
				else
				{
					ResourceBundle bundle = ResourceBundle.getBundle("pspsharp/languages/pspsharp");
					string messageFormat = bundle.getString("MainGUI.RecentFileNotFound.text");
					string message = MessageFormat.format(messageFormat, path);
					JpcspDialogManager.showError(parent, message);
					if (type == TYPE_UMD)
					{
						outerInstance.removeRecentUMD(path);
					}
					else
					{
						outerInstance.removeRecentFile(path);
					}
				}
			}
		}

		private class SetLocationThread : Thread
		{

			public override void run()
			{
				while (true)
				{
					try
					{
						// Wait for 1 second
						sleep(1000);
					}
					catch (InterruptedException)
					{
						// Ignore Exception
					}

					Emulator.MainGUI.setLocation();
				}
			}
		}

		public virtual Rectangle CaptureRectangle
		{
			get
			{
				Insets insets = Insets;
				Rectangle canvasBounds = Modules.sceDisplayModule.Canvas.Bounds;
				Rectangle contentBounds = ContentPane.Bounds;
    
				return new Rectangle(X + insets.left + contentBounds.x + canvasBounds.x, Y + insets.top + contentBounds.y + canvasBounds.y, canvasBounds.width, canvasBounds.height);
			}
		}

		public virtual void run()
		{
			if (umdvideoplayer != null)
			{
				umdvideoplayer.initVideo();
			}
			RunEmu();
		}

		public virtual void pause()
		{
			if (umdvideoplayer != null)
			{
				umdvideoplayer.pauseVideo();
			}
			TogglePauseEmu();
		}

		public virtual void reset()
		{
			resetEmu();
		}

		public virtual bool RunningFromVsh
		{
			get
			{
				return runFromVsh;
			}
		}

		public virtual bool RunningReboot
		{
			get
			{
				return reboot.enableReboot;
			}
		}
	}

}