using System;
using System.Collections.Generic;
using System.Text;

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
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._s0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._s1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.local.LocalVirtualFileSystem.getMsFileName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceUtilitySavedataParam.ERROR_SAVEDATA_CANCELLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceUtilityScreenshotParams.PSP_UTILITY_SCREENSHOT_FORMAT_JPEG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceUtilityScreenshotParams.PSP_UTILITY_SCREENSHOT_FORMAT_PNG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceUtilityScreenshotParams.PSP_UTILITY_SCREENSHOT_NAMERULE_AUTONUM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceFont.PSP_FONT_PIXELFORMAT_4;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSuspendForUser.KERNEL_VOLATILE_MEM_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceSuspendForUser.KERNEL_VOLATILE_MEM_START;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.ALPHA_ONE_MINUS_SOURCE_ALPHA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.ALPHA_SOURCE_ALPHA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.ALPHA_SOURCE_BLEND_OPERATION_ADD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.CLEAR_COLOR_BUFFER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.CMODE_FORMAT_32BIT_ABGR8888;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.PRIM_SPRITES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TFLT_LINEAR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TFUNC_FRAGMENT_DOUBLE_TEXTURE_EFECT_REPLACE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_4BIT_INDEXED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TWRAP_WRAP_MODE_CLAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.VTYPE_POSITION_FORMAT_16_BIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.VTYPE_TEXTURE_FORMAT_16_BIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.VTYPE_TRANSFORM_PIPELINE_RAW_COORD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.IRenderingEngine_Fields.GU_TEXTURE_2D;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.VideoEngine.alignBufferWidth;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.ImageReader.colorARGBtoABGR;
	using SettingsGUI = pspsharp.GUI.SettingsGUI;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;



	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using IVirtualFileSystem = pspsharp.HLE.VFS.IVirtualFileSystem;
	using Managers = pspsharp.HLE.kernel.Managers;
	using SystemTimeManager = pspsharp.HLE.kernel.managers.SystemTimeManager;
	using SceFontInfo = pspsharp.HLE.kernel.types.SceFontInfo;
	using SceIoStat = pspsharp.HLE.kernel.types.SceIoStat;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelLMOption = pspsharp.HLE.kernel.types.SceKernelLMOption;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using SceUtilityGamedataInstallParams = pspsharp.HLE.kernel.types.SceUtilityGamedataInstallParams;
	using SceUtilityGameSharingParams = pspsharp.HLE.kernel.types.SceUtilityGameSharingParams;
	using SceUtilityHtmlViewerParams = pspsharp.HLE.kernel.types.SceUtilityHtmlViewerParams;
	using SceUtilityInstallParams = pspsharp.HLE.kernel.types.SceUtilityInstallParams;
	using SceUtilityMsgDialogParams = pspsharp.HLE.kernel.types.SceUtilityMsgDialogParams;
	using SceUtilityNetconfParams = pspsharp.HLE.kernel.types.SceUtilityNetconfParams;
	using SceUtilityNpSigninParams = pspsharp.HLE.kernel.types.SceUtilityNpSigninParams;
	using SceUtilityOskParams = pspsharp.HLE.kernel.types.SceUtilityOskParams;
	using SceUtilitySavedataParam = pspsharp.HLE.kernel.types.SceUtilitySavedataParam;
	using SceUtilityScreenshotParams = pspsharp.HLE.kernel.types.SceUtilityScreenshotParams;
	using pspUtilityBaseDialog = pspsharp.HLE.kernel.types.pspUtilityBaseDialog;
	using pspUtilityDialogCommon = pspsharp.HLE.kernel.types.pspUtilityDialogCommon;
	using SceUtilityOskData = pspsharp.HLE.kernel.types.SceUtilityOskParams.SceUtilityOskData;
	using pspCharInfo = pspsharp.HLE.kernel.types.pspCharInfo;
	using LoadModuleContext = pspsharp.HLE.modules.ModuleMgrForUser.LoadModuleContext;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using CryptoEngine = pspsharp.crypto.CryptoEngine;
	using SeekableDataInput = pspsharp.filesystems.SeekableDataInput;
	using PNG = pspsharp.format.PNG;
	using PSF = pspsharp.format.PSF;
	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;
	using CaptureImage = pspsharp.graphics.capture.CaptureImage;
	using MemoryStick = pspsharp.hardware.MemoryStick;
	using Screen = pspsharp.hardware.Screen;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Settings = pspsharp.settings.Settings;
	using MemoryInputStream = pspsharp.util.MemoryInputStream;
	using Utilities = pspsharp.util.Utilities;
	using sceGu = pspsharp.util.sceGu;

	using Logger = org.apache.log4j.Logger;

	public class sceUtility : HLEModule
	{
		public static Logger log = Modules.getLogger("sceUtility");

		public override void start()
		{
			gameSharingState = new GameSharingUtilityDialogState("sceUtilityGameSharing");
			netplayDialogState = new NotImplementedUtilityDialogState("sceNetplayDialog");
			netconfState = new NetconfUtilityDialogState("sceUtilityNetconf");
			savedataState = new SavedataUtilityDialogState("sceUtilitySavedata");
			msgDialogState = new MsgDialogUtilityDialogState("sceUtilityMsgDialog");
			oskState = new OskUtilityDialogState("sceUtilityOsk");
			npSigninState = new NpSigninUtilityDialogState("sceUtilityNpSignin");
			PS3ScanState = new NotImplementedUtilityDialogState("sceUtilityPS3Scan");
			rssReaderState = new NotImplementedUtilityDialogState("sceUtilityRssReader");
			rssSubscriberState = new NotImplementedUtilityDialogState("sceUtilityRssSubscriber");
			screenshotState = new ScreenshotUtilityDialogState("sceUtilityScreenshot");
			htmlViewerState = new HtmlViewerUtilityDialogState("sceUtilityHtmlViewer");
			savedataErrState = new NotImplementedUtilityDialogState("sceUtilitySavedataErr");
			gamedataInstallState = new GamedataInstallUtilityDialogState("sceUtilityGamedataInstall");
			storeCheckoutState = new NotImplementedUtilityDialogState("sceUtilityStoreCheckout");
			psnState = new NotImplementedUtilityDialogState("sceUtilityPsn");
			installState = new InstallUtilityDialogState("sceUtilityInstall");
			startedDialogState = null;

			utilityPrivateModules = new Dictionary<string, string>();
			utilityPrivateModules["htmlviewer_ui"] = "flash0:/vsh/module/htmlviewer_ui.prx";
			utilityPrivateModules["hvauth_r"] = "flash0:/vsh/module/hvauth_r.prx";
			utilityPrivateModules["hvauth_t"] = "flash0:/vsh/module/hvauth_t.prx";
			utilityPrivateModules["netfront"] = "flash0:/vsh/module/netfront.prx";
			utilityPrivateModules["mgvideo"] = "flash0:/kd/mgvideo.prx";
			utilityPrivateModules["mm_flash"] = "flash0:/vsh/module/mm_flash.prx";
			utilityPrivateModules["libslim"] = "flash0:/vsh/module/libslim.prx";
			utilityPrivateModules["libwww"] = "flash0:/vsh/module/libwww.prx";
			utilityPrivateModules["libfont_hv"] = "flash0:/vsh/module/libfont_hv.prx";

			base.start();
		}

		public override void stop()
		{
			loadedNetModules.Clear();
			waitingNetModules.Clear();
			loadedAvModules.Clear();
			waitingAvModules.Clear();
			loadedUsbModules.Clear();
			waitingUsbModules.Clear();
			loadedModules.Clear();
			waitingModules.Clear();
			base.stop();
		}

		public const string SYSTEMPARAM_SETTINGS_OPTION_NICKNAME = "emu.sysparam.nickname";
		public const string SYSTEMPARAM_SETTINGS_OPTION_ADHOC_CHANNEL = "emu.sysparam.adhocchannel";
		public const string SYSTEMPARAM_SETTINGS_OPTION_WLAN_POWER_SAVE = "emu.sysparam.wlanpowersave";
		public const string SYSTEMPARAM_SETTINGS_OPTION_DATE_FORMAT = "emu.sysparam.dateformat";
		public const string SYSTEMPARAM_SETTINGS_OPTION_TIME_FORMAT = "emu.sysparam.timeformat";
		public const string SYSTEMPARAM_SETTINGS_OPTION_TIME_ZONE = "emu.sysparam.timezone";
		public const string SYSTEMPARAM_SETTINGS_OPTION_DAYLIGHT_SAVING_TIME = "emu.sysparam.daylightsavings";
		public const string SYSTEMPARAM_SETTINGS_OPTION_LANGUAGE = "emu.impose.language";
		public const string SYSTEMPARAM_SETTINGS_OPTION_BUTTON_PREFERENCE = "emu.impose.button";
		public const string SYSTEMPARAM_SETTINGS_OPTION_LOCK_PARENTAL_LEVEL = "emu.sysparam.locl.parentallevel";
		public const int PSP_SYSTEMPARAM_ID_STRING_NICKNAME = 1; // PSP Registry "/CONFIG/SYSTEM/owner_name"
		public const int PSP_SYSTEMPARAM_ID_INT_ADHOC_CHANNEL = 2; // PSP Registry "/CONFIG/NETWORK/ADHOC/channel"
		public const int PSP_SYSTEMPARAM_ID_INT_WLAN_POWERSAVE = 3; // PSP Registry "/CONFIG/SYSTEM/POWER_SAVING/wlan_mode"
		public const int PSP_SYSTEMPARAM_ID_INT_DATE_FORMAT = 4; // PSP Registry "/CONFIG/DATE/date_format"
		public const int PSP_SYSTEMPARAM_ID_INT_TIME_FORMAT = 5; // PSP Registry "/CONFIG/DATE/time_format"
		public const int PSP_SYSTEMPARAM_ID_INT_TIMEZONE = 6; // PSP Registry "/CONFIG/DATE/time_zone_offset"
		public const int PSP_SYSTEMPARAM_ID_INT_DAYLIGHTSAVINGS = 7; // PSP Registry "/CONFIG/DATE/summer_time"
		public const int PSP_SYSTEMPARAM_ID_INT_LANGUAGE = 8; // PSP Registry "/CONFIG/SYSTEM/XMB/language"
		public const int PSP_SYSTEMPARAM_ID_INT_BUTTON_PREFERENCE = 9; // PSP Registry "/CONFIG/SYSTEM/XMB/button_assign"
		public const int PSP_SYSTEMPARAM_ID_INT_LOCK_PARENTAL_LEVEL = 10; // PSP Registry "/CONFIG/SYSTEM/LOCK/parental_level"
		public const int PSP_SYSTEMPARAM_LANGUAGE_JAPANESE = 0;
		public const int PSP_SYSTEMPARAM_LANGUAGE_ENGLISH = 1;
		public const int PSP_SYSTEMPARAM_LANGUAGE_FRENCH = 2;
		public const int PSP_SYSTEMPARAM_LANGUAGE_SPANISH = 3;
		public const int PSP_SYSTEMPARAM_LANGUAGE_GERMAN = 4;
		public const int PSP_SYSTEMPARAM_LANGUAGE_ITALIAN = 5;
		public const int PSP_SYSTEMPARAM_LANGUAGE_DUTCH = 6;
		public const int PSP_SYSTEMPARAM_LANGUAGE_PORTUGUESE = 7;
		public const int PSP_SYSTEMPARAM_LANGUAGE_RUSSIAN = 8;
		public const int PSP_SYSTEMPARAM_LANGUAGE_KOREAN = 9;
		public const int PSP_SYSTEMPARAM_LANGUAGE_CHINESE_TRADITIONAL = 10;
		public const int PSP_SYSTEMPARAM_LANGUAGE_CHINESE_SIMPLIFIED = 11;
		public const int PSP_SYSTEMPARAM_DATE_FORMAT_YYYYMMDD = 0;
		public const int PSP_SYSTEMPARAM_DATE_FORMAT_MMDDYYYY = 1;
		public const int PSP_SYSTEMPARAM_DATE_FORMAT_DDMMYYYY = 2;
		public const int PSP_SYSTEMPARAM_TIME_FORMAT_24HR = 0;
		public const int PSP_SYSTEMPARAM_TIME_FORMAT_12HR = 1;
		public const int PSP_SYSTEMPARAM_BUTTON_CIRCLE = 0;
		public const int PSP_SYSTEMPARAM_BUTTON_CROSS = 1;
		public const int PSP_UTILITY_DIALOG_STATUS_NONE = 0;
		public const int PSP_UTILITY_DIALOG_STATUS_INIT = 1;
		public const int PSP_UTILITY_DIALOG_STATUS_VISIBLE = 2;
		public const int PSP_UTILITY_DIALOG_STATUS_QUIT = 3;
		public const int PSP_UTILITY_DIALOG_STATUS_FINISHED = 4;
		public const int PSP_UTILITY_DIALOG_STATUS_SCREENSHOT_UNKNOWN = 5;
		public const int PSP_UTILITY_DIALOG_RESULT_OK = 0;
		public const int PSP_UTILITY_DIALOG_RESULT_CANCELED = 1;
		public const int PSP_UTILITY_DIALOG_RESULT_ABORTED = 2;
		public const int PSP_NETPARAM_NAME = 0; // string
		public const int PSP_NETPARAM_SSID = 1; // string
		public const int PSP_NETPARAM_SECURE = 2; // int
		public const int PSP_NETPARAM_WEPKEY = 3; // string
		public const int PSP_NETPARAM_IS_STATIC_IP = 4; // int
		public const int PSP_NETPARAM_IP = 5; // string
		public const int PSP_NETPARAM_NETMASK = 6; // string
		public const int PSP_NETPARAM_ROUTE = 7; // string
		public const int PSP_NETPARAM_MANUAL_DNS = 8; // int
		public const int PSP_NETPARAM_PRIMARYDNS = 9; // string
		public const int PSP_NETPARAM_SECONDARYDNS = 10; // string
		public const int PSP_NETPARAM_PROXY_USER = 11; // string
		public const int PSP_NETPARAM_PROXY_PASS = 12; // string
		public const int PSP_NETPARAM_USE_PROXY = 13; // int
		public const int PSP_NETPARAM_PROXY_SERVER = 14; // string
		public const int PSP_NETPARAM_PROXY_PORT = 15; // int
		public const int PSP_NETPARAM_VERSION = 16; // int
		public const int PSP_NETPARAM_UNKNOWN = 17; // int
		public const int PSP_NETPARAM_8021X_AUTH_TYPE = 18; // int
		public const int PSP_NETPARAM_8021X_USER = 19; // string
		public const int PSP_NETPARAM_8021X_PASS = 20; // string
		public const int PSP_NETPARAM_WPA_TYPE = 21; // int
		public const int PSP_NETPARAM_WPA_KEY = 22; // string
		public const int PSP_NETPARAM_BROWSER = 23; // int
		public const int PSP_NETPARAM_WIFI_CONFIG = 24; // int
		public const int PSP_NETPARAM_MAX_NUMBER_DUMMY_ENTRIES = 10;
		protected internal const int maxLineLengthForDialog = 40;
		protected internal const int icon0Width = 144;
		protected internal const int icon0Height = 80;
		protected internal static readonly int icon0PixelFormat = TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
		protected internal const int smallIcon0Width = 80;
		protected internal const int smallIcon0Height = 44;
		// Round-up width to next valid buffer width
		protected internal static readonly int icon0BufferWidth = alignBufferWidth(icon0Width + pspsharp.graphics.RE.IRenderingEngine_Fields.alignementOfTextureBufferWidth[icon0PixelFormat] - 1, icon0PixelFormat);
		protected internal GameSharingUtilityDialogState gameSharingState;
		protected internal UtilityDialogState netplayDialogState;
		protected internal NetconfUtilityDialogState netconfState;
		protected internal SavedataUtilityDialogState savedataState;
		protected internal MsgDialogUtilityDialogState msgDialogState;
		protected internal OskUtilityDialogState oskState;
		protected internal UtilityDialogState npSigninState;
		protected internal UtilityDialogState PS3ScanState;
		protected internal UtilityDialogState rssReaderState;
		protected internal UtilityDialogState rssSubscriberState;
		protected internal ScreenshotUtilityDialogState screenshotState;
		protected internal HtmlViewerUtilityDialogState htmlViewerState;
		protected internal UtilityDialogState savedataErrState;
		protected internal GamedataInstallUtilityDialogState gamedataInstallState;
		protected internal UtilityDialogState storeCheckoutState;
		protected internal UtilityDialogState psnState;
		protected internal UtilityDialogState startedDialogState;
		private const string dummyNetParamName = "NetConf #%d";
		private static readonly int utilityThreadActionRegister = _s0; // $s0 is preserved across calls
		private static readonly int utilityThreadDelayRegister = _s1; // $s1 is preserved across calls
		private const int UTILITY_THREAD_ACTION_INIT_START = 0;
		private const int UTILITY_THREAD_ACTION_INIT_COMPLETE = 1;
		private const int UTILITY_THREAD_ACTION_SHUTDOWN_START = 2;
		private const int UTILITY_THREAD_ACTION_SHUTDOWN_COMPLETE = 3;
		protected internal Dictionary<int, SceModule> loadedAvModules = new Dictionary<int, SceModule>();
		protected internal Dictionary<int, string> waitingAvModules = new Dictionary<int, string>();
		protected internal Dictionary<int, SceModule> loadedUsbModules = new Dictionary<int, SceModule>();
		protected internal Dictionary<int, string> waitingUsbModules = new Dictionary<int, string>();
		protected internal Dictionary<int, IList<SceModule>> loadedModules = new Dictionary<int, IList<SceModule>>();
		protected internal Dictionary<int, string> waitingModules = new Dictionary<int, string>();
		public static readonly string[] utilityAvModuleNames = new string[] {"PSP_AV_MODULE_AVCODEC", "PSP_AV_MODULE_SASCORE", "PSP_AV_MODULE_ATRAC3PLUS", "PSP_AV_MODULE_MPEGBASE", "PSP_AV_MODULE_MP3", "PSP_AV_MODULE_VAUDIO", "PSP_AV_MODULE_AAC", "PSP_AV_MODULE_G729"};

		public static readonly string[] utilityUsbModuleNames = new string[] {"PSP_USB_MODULE_UNKNOWN_0", "PSP_USB_MODULE_PSPCM", "PSP_USB_MODULE_ACC", "PSP_USB_MODULE_MIC", "PSP_USB_MODULE_CAM", "PSP_USB_MODULE_GPS"};
		private static Dictionary<string, string> utilityPrivateModules;

		public const int PSP_AV_MODULE_AVCODEC = 0;
		public const int PSP_AV_MODULE_SASCORE = 1;
		public const int PSP_AV_MODULE_ATRAC3PLUS = 2;
		public const int PSP_AV_MODULE_MPEGBASE = 3;
		public const int PSP_AV_MODULE_MP3 = 4;
		public const int PSP_AV_MODULE_VAUDIO = 5;
		public const int PSP_AV_MODULE_AAC = 6;
		public const int PSP_AV_MODULE_G729 = 7;

		public const int PSP_USB_MODULE_PSPCM = 1;
		public const int PSP_USB_MODULE_ACC = 2;
		public const int PSP_USB_MODULE_MIC = 3;
		public const int PSP_USB_MODULE_CAM = 4;
		public const int PSP_USB_MODULE_GPS = 5;

		protected internal class InstallUtilityDialogState : UtilityDialogState
		{
			protected internal SceUtilityInstallParams installParams;

			public InstallUtilityDialogState(string name) : base(name)
			{
			}

			protected internal override bool executeUpdateVisible()
			{
				bool keepVisible = false;

				log.warn(string.Format("Partial sceUtilityInstallUpdate {0}", installParams.ToString()));

				// We only get the game name from the install params. Is the rest fixed?
				string fileName = string.Format("ms0:/PSP/GAME/{0}/EBOOT.PBP", installParams.gameName);
				try
				{
					SeekableDataInput moduleInput = Modules.IoFileMgrForUserModule.getFile(fileName, IoFileMgrForUser.PSP_O_RDONLY);
					if (moduleInput != null)
					{
						sbyte[] moduleBytes = new sbyte[(int) moduleInput.length()];
						moduleInput.readFully(moduleBytes);
						ByteBuffer moduleBuffer = ByteBuffer.wrap(moduleBytes);

						// TODO How is this module being loaded?
						// Does it unload the current module? i.e. re-init the PSP
						SceModule module = Emulator.Instance.load(name, moduleBuffer, true);
						Emulator.Clock.resume();

						if ((module.fileFormat & Loader.FORMAT_ELF) == Loader.FORMAT_ELF)
						{
							installParams.@base.result = 0;
							keepVisible = false;
						}
						else
						{
							log.warn("sceUtilityInstall - failed, target is not an ELF");
							installParams.@base.result = -1;
						}
						moduleInput.Dispose();
					}
				}
				catch (GeneralJpcspException e)
				{
					log.error("General Error : " + e.Message);
					Emulator.PauseEmu();
				}
				catch (IOException e)
				{
					log.error(string.Format("sceUtilityInstall - Error while loading module {0}: {1}", fileName, e.Message));
					installParams.@base.result = -1;
				}

				return keepVisible;
			}

			protected internal override pspUtilityBaseDialog createParams()
			{
				installParams = new SceUtilityInstallParams();
				return installParams;
			}

			protected internal override bool hasDialog()
			{
				return false;
			}
		}

		public static readonly string[] utilityNetModuleNames = new string[] {"PSP_NET_MODULE_UNKNOWN(1)", "PSP_NET_MODULE_COMMON", "PSP_NET_MODULE_ADHOC", "PSP_NET_MODULE_INET", "PSP_NET_MODULE_PARSEURI", "PSP_NET_MODULE_PARSEHTTP", "PSP_NET_MODULE_HTTP", "PSP_NET_MODULE_SSL"};

		public const int PSP_NET_MODULE_COMMON = 1;
		public const int PSP_NET_MODULE_ADHOC = 2;
		public const int PSP_NET_MODULE_INET = 3;
		public const int PSP_NET_MODULE_PARSEURI = 4;
		public const int PSP_NET_MODULE_PARSEHTTP = 5;
		public const int PSP_NET_MODULE_HTTP = 6;
		public const int PSP_NET_MODULE_SSL = 7;

		protected internal Dictionary<int, SceModule> loadedNetModules = new Dictionary<int, SceModule>();
		protected internal Dictionary<int, string> waitingNetModules = new Dictionary<int, string>();
		protected internal InstallUtilityDialogState installState;

		private string getNetModuleName(int module)
		{
			if (module < 0 || module >= utilityNetModuleNames.Length)
			{
				return "PSP_NET_MODULE_UNKNOWN_" + module;
			}
			return utilityNetModuleNames[module];
		}

		protected internal virtual int hleUtilityLoadNetModule(int module, string moduleName)
		{
			HLEModuleManager moduleManager = HLEModuleManager.Instance;
			if (loadedNetModules.ContainsKey(module) || waitingNetModules.ContainsKey(module))
			{ // Module already loaded.
				return SceKernelErrors.ERROR_NET_MODULE_ALREADY_LOADED;
			}
			else if (!moduleManager.hasFlash0Module(moduleName))
			{ // Can't load flash0 module.
				waitingNetModules[module] = moduleName; // Always save a load attempt.
				return SceKernelErrors.ERROR_NET_MODULE_BAD_ID;
			}
			else
			{
				// Load and save it in loadedNetModules.
				int sceModuleId = moduleManager.LoadFlash0Module(moduleName);
				SceModule sceModule = Managers.modules.getModuleByUID(sceModuleId);
				loadedNetModules[module] = sceModule;
				return 0;
			}
		}

		protected internal virtual int hleUtilityUnloadNetModule(int module)
		{
			if (loadedNetModules.ContainsKey(module))
			{
				// Unload the module.
				HLEModuleManager moduleManager = HLEModuleManager.Instance;
				SceModule sceModule = loadedNetModules.Remove(module);
				moduleManager.UnloadFlash0Module(sceModule);
				return 0;
			}
			else if (waitingNetModules.ContainsKey(module))
			{
				// Simulate a successful unload.
				waitingNetModules.Remove(module);
				return 0;
			}
			else
			{
				return SceKernelErrors.ERROR_NET_MODULE_NOT_LOADED;
			}
		}

		private static Locale getUtilityLocale(int language)
		{
			Locale utilityLocale = Locale.Default;

			switch (language)
			{
				case PSP_SYSTEMPARAM_LANGUAGE_JAPANESE:
					utilityLocale = Locale.JAPANESE;
					break;
				case PSP_SYSTEMPARAM_LANGUAGE_ENGLISH:
					utilityLocale = Locale.ENGLISH;
					break;
				case PSP_SYSTEMPARAM_LANGUAGE_FRENCH:
					utilityLocale = Locale.FRENCH;
					break;
				case PSP_SYSTEMPARAM_LANGUAGE_SPANISH:
					utilityLocale = new Locale("es");
					break;
				case PSP_SYSTEMPARAM_LANGUAGE_GERMAN:
					utilityLocale = Locale.GERMAN;
					break;
				case PSP_SYSTEMPARAM_LANGUAGE_ITALIAN:
					utilityLocale = Locale.ITALIAN;
					break;
				case PSP_SYSTEMPARAM_LANGUAGE_DUTCH:
					utilityLocale = new Locale("nl");
					break;
				case PSP_SYSTEMPARAM_LANGUAGE_PORTUGUESE:
					utilityLocale = new Locale("pt");
					break;
				case PSP_SYSTEMPARAM_LANGUAGE_RUSSIAN:
					utilityLocale = new Locale("ru");
					break;
				case PSP_SYSTEMPARAM_LANGUAGE_KOREAN:
					utilityLocale = Locale.KOREAN;
					break;
				case PSP_SYSTEMPARAM_LANGUAGE_CHINESE_TRADITIONAL:
					utilityLocale = Locale.TRADITIONAL_CHINESE;
					break;
				case PSP_SYSTEMPARAM_LANGUAGE_CHINESE_SIMPLIFIED:
					utilityLocale = Locale.CHINESE;
					break;
			}

			return utilityLocale;
		}

		private static string DateTimeFormatString
		{
			get
			{
				StringBuilder dateTimeFormat = new StringBuilder();
    
				switch (SystemParamDateFormat)
				{
					case PSP_SYSTEMPARAM_DATE_FORMAT_DDMMYYYY:
						dateTimeFormat.Append("%te/%<tm/%<tY");
						break;
					case PSP_SYSTEMPARAM_DATE_FORMAT_MMDDYYYY:
						dateTimeFormat.Append("%tm/%<te/%<tY");
						break;
					case PSP_SYSTEMPARAM_DATE_FORMAT_YYYYMMDD:
						dateTimeFormat.Append("%tY/%<tm/%<te");
						break;
					default:
						dateTimeFormat.Append("%tF");
						break;
				}
    
				dateTimeFormat.Append(" ");
    
				switch (SystemParamTimeFormat)
				{
					case PSP_SYSTEMPARAM_TIME_FORMAT_12HR:
						dateTimeFormat.Append("%<tl:%<tM %<Tp");
						break;
					case PSP_SYSTEMPARAM_TIME_FORMAT_24HR:
						dateTimeFormat.Append("%<tk:%<tM");
						break;
					default:
						dateTimeFormat.Append("%<tR");
						break;
				}
    
				return dateTimeFormat.ToString();
			}
		}

		private static string formatDateTime(DateTime dateTime)
		{
			string formattedDateTime = string.format(DateTimeFormatString, dateTime);

			// Java doesn't have a format for the month as single digit :-(
			// E.g. February is always formatted as "02".
			// The PSP is however displaying the month as a single digit,
			// so we manually remove any leading 0 for the month.
			if (formattedDateTime.StartsWith("0", StringComparison.Ordinal))
			{
				formattedDateTime = formattedDateTime.Substring(1);
			}
			formattedDateTime = formattedDateTime.Replace("/0", "/");

			return formattedDateTime;
		}

		public virtual void hleUtilityThread(Processor processor)
		{
			SceKernelThreadInfo currentThread = Modules.ThreadManForUserModule.CurrentThread;
			int action = processor.cpu.getRegister(utilityThreadActionRegister);
			int delay = processor.cpu.getRegister(utilityThreadDelayRegister);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("hleUtilityThread action={0:D}, delay={1:D}", action, delay));
			}

			switch (action)
			{
				case UTILITY_THREAD_ACTION_INIT_START:
					// Starting the init action.
					// Lock the volatile mem, it is used until sceUtilityXXXShutdown
					int lockResult = Modules.sceSuspendForUserModule.hleKernelVolatileMemLock(0, false);
					if (lockResult < 0)
					{
						log.error(string.Format("hleUtilityThread init thread cannot lock the volatile mem 0x{0:X8}", lockResult));
					}

					currentThread.cpuContext.setRegister(utilityThreadActionRegister, UTILITY_THREAD_ACTION_INIT_COMPLETE);
					if (currentThread.Running)
					{
						// Wait a short time before completing the init.
						if (delay > 0)
						{
							Modules.ThreadManForUserModule.hleKernelDelayThread(delay, false);
						}
					}
					break;
				case UTILITY_THREAD_ACTION_INIT_COMPLETE:
					// Completing the init action.
					// Move to status VISIBLE
					startedDialogState.status = PSP_UTILITY_DIALOG_STATUS_VISIBLE;
					startedDialogState.startVisibleTimeMillis = Emulator.Clock.currentTimeMillis();
					if (!startedDialogState.hasDialog())
					{
						startedDialogState.dialogState = UtilityDialogState.DialogState.quit;
					}

					processor.cpu._v0 = 0;
					Modules.ThreadManForUserModule.hleKernelExitDeleteThread();
					break;
				case UTILITY_THREAD_ACTION_SHUTDOWN_START:
					// Starting the shutdown action.
					// Unlock the volatile mem
					int unlockResult = Modules.sceSuspendForUserModule.hleKernelVolatileMemUnlock(0);
					if (unlockResult < 0)
					{
						log.error(string.Format("hleUtilityThread shutdown thread cannot unlock the volatile mem 0x{0:X8}", unlockResult));
					}
					else
					{
						Memory mem = Memory.Instance;
						// The volatile memory is cleared after its use
						mem.memset(KERNEL_VOLATILE_MEM_START, (sbyte) 0, KERNEL_VOLATILE_MEM_SIZE);
					}

					currentThread.cpuContext.setRegister(utilityThreadActionRegister, UTILITY_THREAD_ACTION_SHUTDOWN_COMPLETE);
					if (currentThread.Running)
					{
						// Wait a short time before completing the shutdown.
						if (delay > 0)
						{
							Modules.ThreadManForUserModule.hleKernelDelayThread(delay, false);
						}
					}
					break;
				case UTILITY_THREAD_ACTION_SHUTDOWN_COMPLETE:
					// Completing the shutdown action.
					startedDialogState.status = PSP_UTILITY_DIALOG_STATUS_NONE;
					processor.cpu._v0 = 0;
					Modules.ThreadManForUserModule.hleKernelExitDeleteThread();
					break;
			}
		}

		protected internal abstract class UtilityDialogState
		{

			protected internal string name;
			protected internal pspUtilityBaseDialog @params;
			protected internal TPointer paramsAddr;
			protected internal int status;
			protected internal UtilityDialog dialog;
			protected internal int drawSpeed;
			protected internal int minimumVisibleDurationMillis;
			protected internal long startVisibleTimeMillis;
			protected internal int buttonPressed;
			protected internal GuUtilityDialog guDialog;
			protected internal bool isOnlyGeGraphics;
			protected internal bool isYesSelected;

			protected internal enum DialogState
			{

				init,
				display,
				confirmation,
				inProgress,
				completed,
				quit
			}
			protected internal DialogState dialogState;

			public UtilityDialogState(string name)
			{
				this.name = name;
				status = PSP_UTILITY_DIALOG_STATUS_NONE;
				dialogState = DialogState.init;
				ButtonPressed = SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_INVALID;
			}

			protected internal virtual void openDialog(UtilityDialog dialog)
			{
				if (dialogState == DialogState.init)
				{
					dialogState = DialogState.display;
				}

				status = PSP_UTILITY_DIALOG_STATUS_VISIBLE;
				this.dialog = dialog;
				dialog.Visible = true;
			}

			protected internal virtual void openDialog(GuUtilityDialog guDialog)
			{
				if (dialogState == DialogState.init)
				{
					dialogState = DialogState.display;
				}

				status = PSP_UTILITY_DIALOG_STATUS_VISIBLE;
				this.guDialog = guDialog;

				// The option "Only GE Graphics" cannot be used during the
				// rendering of the GU dialog. The GE list has to be rendered
				// additionally to the application display.
				isOnlyGeGraphics = Modules.sceDisplayModule.OnlyGEGraphics;
				if (isOnlyGeGraphics)
				{
					Modules.sceDisplayModule.OnlyGEGraphics = false;
				}
			}

			protected internal virtual bool DialogOpen
			{
				get
				{
					return dialog != null || guDialog != null;
				}
			}

			protected internal virtual void updateDialog()
			{
				int delayMicros = 1000000 / 60;
	//            if (drawSpeed > 0) {
	//                delayMicros *= drawSpeed;
	//            }
				Modules.ThreadManForUserModule.hleKernelDelayThread(delayMicros, false);
			}

			protected internal virtual bool DialogActive
			{
				get
				{
					if (DialogOpen)
					{
						if (dialog != null)
						{
							return dialog.Visible;
						}
    
						if (guDialog != null)
						{
							return guDialog.Visible;
						}
					}
    
					return false;
				}
			}

			protected internal virtual void closeDialog()
			{
				if (dialog != null)
				{
					dialog = null;
				}
				if (guDialog != null)
				{
					// Reset the previous state of the option "Only GE Graphics"
					if (isOnlyGeGraphics)
					{
						Modules.sceDisplayModule.OnlyGEGraphics = isOnlyGeGraphics;
					}

					guDialog = null;
				}
			}

			internal virtual int Result
			{
				set
				{
					if (@params != null && @params.@base != null)
					{
						@params.@base.result = value;
						@params.@base.writeResult(paramsAddr);
					}
				}
			}

			protected internal virtual void quitDialog()
			{
				closeDialog();
				status = PSP_UTILITY_DIALOG_STATUS_QUIT;
				dialogState = DialogState.quit;
			}

			protected internal virtual void quitDialog(int result)
			{
				quitDialog();
				Result = result;
			}

			public virtual int ButtonPressed
			{
				get
				{
					return buttonPressed;
				}
				set
				{
					this.buttonPressed = value;
				}
			}


			public virtual int executeInitStart(TPointer paramsAddr)
			{
				if (status != PSP_UTILITY_DIALOG_STATUS_NONE)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("{0}InitStart already started status={1:D}", name, status));
					}
					return SceKernelErrors.ERROR_UTILITY_INVALID_STATUS;
				}

				this.paramsAddr = paramsAddr;
				this.@params = createParams();

				@params.read(paramsAddr);

				if (log.InfoEnabled)
				{
					log.info(string.Format("{0}InitStart {1}-0x{2:X8}: {3}", name, paramsAddr, paramsAddr.Address + @params.@sizeof(), @params.ToString()));
				}

				int validityResult = checkValidity();

				if (validityResult == 0)
				{
					// Start with INIT
					status = PSP_UTILITY_DIALOG_STATUS_INIT;
					dialogState = DialogState.init;
					Modules.sceUtilityModule.startedDialogState = this;

					// Execute the init thread, it will update the status
					SceKernelThreadInfo initThread = Modules.ThreadManForUserModule.hleKernelCreateThread("SceUtilityInit", ThreadManForUser.UTILITY_LOOP_ADDRESS, @params.@base.accessThread, 0x800, 0, 0, SysMemUserForUser.USER_PARTITION_ID);
					Modules.ThreadManForUserModule.hleKernelStartThread(initThread, 0, 0, initThread.gpReg_addr);
					initThread.cpuContext.setRegister(utilityThreadActionRegister, UTILITY_THREAD_ACTION_INIT_START);
					initThread.cpuContext.setRegister(utilityThreadDelayRegister, InitDelay);
				}

				return validityResult;
			}

			protected internal virtual bool ReadyForVisible
			{
				get
				{
					// Wait for all the buttons to be released
					if (State.controller.Buttons != 0)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Not ready for visible, button pressed 0x{0:X}", State.controller.Buttons));
						}
						return false;
					}
    
					return true;
				}
			}

			protected internal virtual bool hasDialog()
			{
				return true;
			}

			public virtual int executeGetStatus()
			{
				// Return ERROR_UTILITY_WRONG_TYPE if no sceUtilityXXXInitStart has ever been started or
				// if a different type of dialog was started.
				if (Modules.sceUtilityModule.startedDialogState == null || Modules.sceUtilityModule.startedDialogState != this)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("{0}GetStatus returning ERROR_UTILITY_WRONG_TYPE", name));
					}
					return SceKernelErrors.ERROR_UTILITY_WRONG_TYPE;
				}

				if (log.DebugEnabled)
				{
					log.debug(string.Format("{0}GetStatus status {1:D}", name, status));
				}

				int previousStatus = status;

				// Remark: moving from INIT status to VISIBLE is performed in the init thread.
				// Remark: moving from FINISHED status to NONE is performed in the shutdown thread.

				// After moving to status NONE, subsequent calls of sceUtilityXXXGetStatus
				// keep returning status NONE (if of the same type) and not ERROR_UTILITY_WRONG_TYPE.
				// Keep the current value in Modules.sceUtilityModule.startedDialogState for this purpose.
				return previousStatus;
			}

			public virtual int executeShutdownStart()
			{
				if (Modules.sceUtilityModule.startedDialogState == null || Modules.sceUtilityModule.startedDialogState != this)
				{
					if (log.DebugEnabled)
					{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("%ShutdownStart returning ERROR_UTILITY_WRONG_TYPE", name));
						log.debug(string.Format("%ShutdownStart returning ERROR_UTILITY_WRONG_TYPE", name));
					}
					return SceKernelErrors.ERROR_UTILITY_WRONG_TYPE;
				}

				if (status != PSP_UTILITY_DIALOG_STATUS_QUIT)
				{
					if (log.DebugEnabled)
					{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.debug(String.format("%ShutdownStart returning ERROR_UTILITY_INVALID_STATUS", name));
						log.debug(string.Format("%ShutdownStart returning ERROR_UTILITY_INVALID_STATUS", name));
					}
					return SceKernelErrors.ERROR_UTILITY_INVALID_STATUS;
				}

				status = PSP_UTILITY_DIALOG_STATUS_FINISHED;

				// Execute the shutdown thread, it will set the status to 0.
				SceKernelThreadInfo shutdownThread = Modules.ThreadManForUserModule.hleKernelCreateThread("SceUtilityShutdown", ThreadManForUser.UTILITY_LOOP_ADDRESS, @params.@base.accessThread, 0x800, 0, 0, SysMemUserForUser.USER_PARTITION_ID);
				Modules.ThreadManForUserModule.hleKernelStartThread(shutdownThread, 0, 0, shutdownThread.gpReg_addr);
				shutdownThread.cpuContext.setRegister(utilityThreadActionRegister, UTILITY_THREAD_ACTION_SHUTDOWN_START);
				shutdownThread.cpuContext.setRegister(utilityThreadDelayRegister, ShutdownDelay);

				return 0;
			}

			/// <param name="drawSpeed"> FPS used for internal animation sync (1 = 60 FPS; 2 = 30 FPS; 3 = 15 FPS)
			/// @return </param>
			public int executeUpdate(int drawSpeed)
			{
				this.drawSpeed = drawSpeed;

				if (Modules.sceUtilityModule.startedDialogState == null || Modules.sceUtilityModule.startedDialogState != this)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("{0}Update returning ERROR_UTILITY_WRONG_TYPE", name));
					}
					return SceKernelErrors.ERROR_UTILITY_WRONG_TYPE;
				}

				// PSP is returning ERROR_UTILITY_INVALID_STATUS when not in STATUS_VISIBLE
				int result = SceKernelErrors.ERROR_UTILITY_INVALID_STATUS;

				if (status == PSP_UTILITY_DIALOG_STATUS_INIT && ReadyForVisible)
				{
					// Move from INIT to VISIBLE
					status = PSP_UTILITY_DIALOG_STATUS_VISIBLE;
					startVisibleTimeMillis = Emulator.Clock.currentTimeMillis();
				}
				else if (status == PSP_UTILITY_DIALOG_STATUS_VISIBLE || status == PSP_UTILITY_DIALOG_STATUS_SCREENSHOT_UNKNOWN)
				{
					// PSP is returning 0 only in STATUS_VISIBLE
					result = 0;

					// Some games reach sceUtilitySavedataInitStart with empty params which only
					// get filled with a subsequent call to sceUtilitySavedataUpdate (eg.: To Love-Ru).
					// This is why we have to re-read the params here.
					@params.read(paramsAddr);

					if (guDialog != null)
					{
						guDialog.update(drawSpeed);
					}

					bool keepVisible = executeUpdateVisible();

					if (status == PSP_UTILITY_DIALOG_STATUS_VISIBLE && DialogOpen)
					{
						if (dialog != null)
						{
							dialog.checkController();
						}
						if (guDialog != null)
						{
							guDialog.checkController();
						}
					}

					if (status == PSP_UTILITY_DIALOG_STATUS_VISIBLE && !DialogOpen && !keepVisible && dialogState == DialogState.quit)
					{
						// Check if we stayed long enough in the VISIBLE state
						long now = Emulator.Clock.currentTimeMillis();
						if (now - startVisibleTimeMillis >= MinimumVisibleDurationMillis)
						{
							// There was no dialog or it has completed
							status = PSP_UTILITY_DIALOG_STATUS_QUIT;
						}
					}
				}

				if (log.DebugEnabled)
				{
					log.debug(string.Format("{0}Update returning 0x{1:X8}", name, result));
				}
				return result;
			}

			public virtual int executeAbort()
			{
				if (Modules.sceUtilityModule.startedDialogState == null || Modules.sceUtilityModule.startedDialogState != this)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("{0}Abort returning ERROR_UTILITY_WRONG_TYPE", name));
					}
					return SceKernelErrors.ERROR_UTILITY_WRONG_TYPE;
				}

				if (status != PSP_UTILITY_DIALOG_STATUS_VISIBLE)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("{0}Abort returning ERROR_UTILITY_INVALID_STATUS", name));
					}
					return SceKernelErrors.ERROR_UTILITY_INVALID_STATUS;
				}

				if (log.DebugEnabled)
				{
					log.debug(string.Format("{0}Abort", name));
				}

				quitDialog(PSP_UTILITY_DIALOG_RESULT_ABORTED);

				return 0;
			}

			public virtual void cancel()
			{
				quitDialog(PSP_UTILITY_DIALOG_RESULT_CANCELED);
			}

			protected internal abstract bool executeUpdateVisible();

			protected internal abstract pspUtilityBaseDialog createParams();

			protected internal virtual int checkValidity()
			{
				return 0;
			}

			public virtual int MinimumVisibleDurationMillis
			{
				get
				{
					return minimumVisibleDurationMillis;
				}
				set
				{
					this.minimumVisibleDurationMillis = value;
				}
			}


			protected internal virtual string getDialogTitle(string key, string defaultTitle, Locale utilityLocale)
			{
				string title;
				try
				{
					ResourceBundle bundle = ResourceBundle.getBundle("pspsharp/languages/pspsharp", utilityLocale);
					if (string.ReferenceEquals(key, null))
					{
						title = bundle.getString(name);
					}
					else
					{
						title = bundle.getString(name + "." + key);
					}
				}
				catch (MissingResourceException)
				{
					title = defaultTitle;
				}
				return title;
			}

			public virtual bool YesSelected
			{
				get
				{
					return isYesSelected;
				}
				set
				{
					this.isYesSelected = value;
				}
			}

			public virtual bool NoSelected
			{
				get
				{
					return !isYesSelected;
				}
			}


			protected internal virtual int ShutdownDelay
			{
				get
				{
					if (hasDialog())
					{
						// The shutdown is taking some time to complete
						// when a dialog has been shown to the user.
						return 50000;
					}
    
					return 0;
				}
			}

			protected internal virtual int InitDelay
			{
				get
				{
					return 0;
				}
			}
		}

		protected internal class NotImplementedUtilityDialogState : UtilityDialogState
		{

			public NotImplementedUtilityDialogState(string name) : base(name)
			{
			}

			public override int executeInitStart(TPointer paramsAddr)
			{
				log.warn(string.Format("Unimplemented: {0}InitStart params: {1}", name, Utilities.getMemoryDump(paramsAddr.Address, paramsAddr.getValue32())));

				return SceKernelErrors.ERROR_UTILITY_IS_UNKNOWN;
			}

			public override int executeShutdownStart()
			{
				log.warn(string.Format("Unimplemented: {0}ShutdownStart", name));

				return SceKernelErrors.ERROR_UTILITY_IS_UNKNOWN;
			}

			public override int executeGetStatus()
			{
				log.warn(string.Format("Unimplemented: {0}GetStatus", name));

				return SceKernelErrors.ERROR_UTILITY_IS_UNKNOWN;
			}

			protected internal override bool executeUpdateVisible()
			{
				log.warn(string.Format("Unimplemented: {0}Update", name));

				return false;
			}

			protected internal override pspUtilityBaseDialog createParams()
			{
				return null;
			}

			protected internal override bool hasDialog()
			{
				return false;
			}
		}

		protected internal class SavedataUtilityDialogState : UtilityDialogState
		{

			protected internal SceUtilitySavedataParam savedataParams;
			protected internal volatile string saveListSelection;
			protected internal volatile System.IO.Stream saveListSelectionIcon0;
			protected internal bool saveListEmpty;

			public SavedataUtilityDialogState(string name) : base(name)
			{

				// Stay at least 500ms in the VISIBLE state.
				// E.g. do not complete too quickly the AUTOLOAD/AUTOSAVE modes.
				MinimumVisibleDurationMillis = 500;
			}

			protected internal override pspUtilityBaseDialog createParams()
			{
				savedataParams = new SceUtilitySavedataParam();
				return savedataParams;
			}

			protected internal override int checkValidity()
			{
				int paramSize = savedataParams.@base.totalSizeof();
				// Only these parameter sizes are allowed:
				if (paramSize != 1480 && paramSize != 1500 && paramSize != 1536)
				{
					log.warn(string.Format("sceUtilitySavedataInitStart invalid parameter size {0:D}", paramSize));
					return SceKernelErrors.ERROR_UTILITY_INVALID_PARAM_SIZE;
				}

				return base.checkValidity();
			}
			// All SAVEDATA modes after MODE_SINGLEDELETE can be called multiple times and keep track of that.
			internal int savedataMultiStatus;

			protected internal virtual int checkMultipleCallStatus()
			{
				// Check the current multiple call status.
				if (savedataParams.multiStatus == SceUtilitySavedataParam.MULTI_STATUS_SINGLE || savedataParams.multiStatus == SceUtilitySavedataParam.MULTI_STATUS_INIT)
				{
					// If the multiple call status is SINGLE or INIT, just save it.
					savedataMultiStatus = savedataParams.multiStatus;
					return 0;
				}
				if (savedataParams.multiStatus == SceUtilitySavedataParam.MULTI_STATUS_RELAY || savedataParams.multiStatus == SceUtilitySavedataParam.MULTI_STATUS_FINISH)
				{
					// If the multiple call status is RELAY or FINISH, check if INIT or another RELAY has been called.
					if (savedataMultiStatus <= savedataParams.multiStatus)
					{
						savedataMultiStatus = savedataParams.multiStatus;
						return 0;
					}
				}

				return SceKernelErrors.ERROR_SAVEDATA_RW_BAD_STATUS;
			}

			protected internal override bool executeUpdateVisible()
			{
				Memory mem = Processor.memory;

				switch (savedataParams.mode)
				{
					case SceUtilitySavedataParam.MODE_AUTOLOAD:
					{
						if (string.ReferenceEquals(savedataParams.saveName, null) || savedataParams.saveName.Equals(SceUtilitySavedataParam.anyFileName) || savedataParams.saveName.Length == 0)
						{
							if (savedataParams.saveNameList != null && savedataParams.saveNameList.Length > 0)
							{
								savedataParams.saveName = savedataParams.saveNameList[0];
							}
						}

						try
						{
							savedataParams.load(mem);
							savedataParams.@base.result = 0;
							savedataParams.write(mem);
						}
						catch (IOException)
						{
							if (!savedataParams.GameDirectoryPresent)
							{
								savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_NO_DATA;
							}
							else if (savedataParams.@base.totalSizeof() < 1536)
							{
								savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_NO_DATA;
							}
							else
							{
								// The PSP is returning a different return code based on the size of the savedataParams input structure.
								savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_NO_UMD;
							}
						}
						catch (Exception e)
						{
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_ACCESS_ERROR;
							log.error(e);
						}
						break;
					}

					case SceUtilitySavedataParam.MODE_LOAD:
					{
						switch (dialogState)
						{
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.init:
							{
								if (string.ReferenceEquals(savedataParams.saveName, null) || savedataParams.saveName.Length == 0)
								{
									if (savedataParams.saveNameList != null && savedataParams.saveNameList.Length > 0)
									{
										savedataParams.saveName = savedataParams.saveNameList[0];
									}
								}

								YesSelected = true;
								GuSavedataDialogLoad gu = new GuSavedataDialogLoad(savedataParams, this);
								openDialog(gu);
								dialogState = DialogState.confirmation;
								break;
							}
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.confirmation:
							{
								if (!DialogActive)
								{
									if (ButtonPressed != SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_OK || NoSelected)
									{
										// The dialog has been cancelled or the user did not want to load.
										cancel();
									}
									else
									{
										closeDialog();
										dialogState = DialogState.inProgress;
									}
								}
								else
								{
									updateDialog();
								}
								break;
							}
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.inProgress:
							{
								try
								{
									savedataParams.load(mem);
									savedataParams.@base.result = 0;
									savedataParams.write(mem);
								}
								catch (IOException)
								{
									if (!savedataParams.GameDirectoryPresent)
									{
										savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_NO_DATA;
									}
									else if (savedataParams.@base.totalSizeof() < 1536)
									{
										savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_NO_DATA;
									}
									else
									{
										// The PSP is returning a different return code based on the size of the savedataParams input structure.
										savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_NO_UMD;
									}
								}
								catch (Exception e)
								{
									savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_ACCESS_ERROR;
									log.error(e);
								}

								if (ReadyForVisible)
								{
									GuSavedataDialogCompleted gu = new GuSavedataDialogCompleted(savedataParams, this, saveListSelectionIcon0);
									openDialog(gu);
									dialogState = DialogState.completed;
								}
								break;
							}
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.completed:
							{
								if (!DialogActive)
								{
									quitDialog();
								}
								else
								{
									updateDialog();
								}
								break;
							}
						case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.display:
						case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.quit:
							// Nothing to do
							break;
						}
						break;
					}

					case SceUtilitySavedataParam.MODE_LISTLOAD:
					{
						switch (dialogState)
						{
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.init:
							{
								// Search for valid saves.
								List<string> validNames = new List<string>();

								for (int i = 0; i < savedataParams.saveNameList.Length; i++)
								{
									savedataParams.saveName = savedataParams.saveNameList[i];

									if (savedataParams.Present)
									{
										validNames.Add(savedataParams.saveName);
									}
								}

								GuSavedataDialog gu = new GuSavedataDialog(savedataParams, this, validNames.ToArray());
								openDialog(gu);
								break;
							}
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.display:
							{
								if (!DialogActive)
								{
									if (ButtonPressed != SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_OK)
									{
										if (saveListEmpty)
										{
											// No data available
											savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_NO_DATA;
										}
										else
										{
											// Dialog cancelled
											savedataParams.@base.result = ERROR_SAVEDATA_CANCELLED;
										}
										quitDialog(savedataParams.@base.result);
									}
									else if (string.ReferenceEquals(saveListSelection, null))
									{
										log.warn("Savedata MODE_LISTLOAD no save selected");
										quitDialog(SceKernelErrors.ERROR_SAVEDATA_LOAD_BAD_PARAMS);
									}
									else
									{
										closeDialog();
										dialogState = DialogState.inProgress;
									}
								}
								else
								{
									updateDialog();
								}
								break;
							}
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.inProgress:
							{
								try
								{
									savedataParams.saveName = saveListSelection;
									if (log.DebugEnabled)
									{
										log.debug(string.Format("Loading savedata {0}", savedataParams.saveName));
									}
									savedataParams.load(mem);
									savedataParams.@base.result = 0;
									savedataParams.write(mem);
								}
								catch (IOException)
								{
									if (!savedataParams.GameDirectoryPresent)
									{
										savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_NO_DATA;
									}
									else if (savedataParams.@base.totalSizeof() < 1536)
									{
										savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_NO_DATA;
									}
									else
									{
										// The PSP is returning a different return code based on the size of the savedataParams input structure.
										savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_NO_UMD;
									}
								}
								catch (Exception e)
								{
									savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_LOAD_ACCESS_ERROR;
									log.error(e);
								}

								if (ReadyForVisible)
								{
									GuSavedataDialogCompleted gu = new GuSavedataDialogCompleted(savedataParams, this, saveListSelectionIcon0);
									openDialog(gu);
									dialogState = DialogState.completed;
								}
								break;
							}
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.completed:
							{
								if (!DialogActive)
								{
									quitDialog();
								}
								else
								{
									updateDialog();
								}
								break;
							}
						case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.confirmation:
						case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.quit:
							// Nothing to do
							break;
						}
						break;
					}

					case SceUtilitySavedataParam.MODE_AUTOSAVE:
					{
						if (string.ReferenceEquals(savedataParams.saveName, null) || savedataParams.saveName.Equals(SceUtilitySavedataParam.anyFileName) || savedataParams.saveName.Length == 0)
						{
							if (savedataParams.saveNameList != null && savedataParams.saveNameList.Length > 0)
							{
								savedataParams.saveName = savedataParams.saveNameList[0];
							}
						}

						try
						{
							savedataParams.save(mem, true);
							savedataParams.@base.result = 0;
						}
						catch (IOException)
						{
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_SAVE_ACCESS_ERROR;
						}
						catch (Exception e)
						{
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_SAVE_ACCESS_ERROR;
							log.error(e);
						}
						break;
					}

					case SceUtilitySavedataParam.MODE_SAVE:
					{
						switch (dialogState)
						{
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.init:
							{
								if (string.ReferenceEquals(savedataParams.saveName, null) || savedataParams.saveName.Length == 0)
								{
									if (savedataParams.saveNameList != null && savedataParams.saveNameList.Length > 0)
									{
										savedataParams.saveName = savedataParams.saveNameList[0];
									}
								}

								// Yes is selected by default if the save does not exist.
								// No is selected by default if the save does exist (overwrite).
								YesSelected = !savedataParams.Present;
								GuSavedataDialogSave gu = new GuSavedataDialogSave(savedataParams, this);
								openDialog(gu);
								dialogState = DialogState.confirmation;
								break;
							}
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.confirmation:
							{
								if (!DialogActive)
								{
									if (ButtonPressed != SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_OK || NoSelected)
									{
										// The dialog has been cancelled or the user did not want to save.
										cancel();
									}
									else
									{
										closeDialog();
										dialogState = DialogState.inProgress;
									}
								}
								else
								{
									updateDialog();
								}
								break;
							}
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.inProgress:
							{
								try
								{
									savedataParams.save(mem, true);
									savedataParams.@base.result = 0;
								}
								catch (IOException)
								{
									savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_SAVE_ACCESS_ERROR;
								}
								catch (Exception e)
								{
									savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_SAVE_ACCESS_ERROR;
									log.error(e);
								}

								if (ReadyForVisible)
								{
									GuSavedataDialogCompleted gu = new GuSavedataDialogCompleted(savedataParams, this, saveListSelectionIcon0);
									openDialog(gu);
									dialogState = DialogState.completed;
								}
								break;
							}
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.completed:
							{
								if (!DialogActive)
								{
									quitDialog();
								}
								else
								{
									updateDialog();
								}
								break;
							}
						case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.display:
						case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.quit:
							// Nothing to do
							break;
						}
						break;
					}

					case SceUtilitySavedataParam.MODE_LISTSAVE:
					{
						switch (dialogState)
						{
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.init:
							{
								GuSavedataDialog gu = new GuSavedataDialog(savedataParams, this, savedataParams.saveNameList);
								openDialog(gu);
								break;
							}
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.display:
							{
								if (!DialogActive)
								{
									closeDialog();
									if (ButtonPressed != SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_OK)
									{
										// Dialog cancelled
										quitDialog(SceKernelErrors.ERROR_SAVEDATA_SAVE_BAD_PARAMS);
									}
									else if (string.ReferenceEquals(saveListSelection, null))
									{
										log.warn("Savedata MODE_LISTSAVE no save selected");
										quitDialog(ERROR_SAVEDATA_CANCELLED);
									}
									else
									{
										savedataParams.saveName = saveListSelection;
										savedataParams.write(mem);
										if (savedataParams.isPresent(savedataParams.gameName, saveListSelection))
										{
											if (ReadyForVisible)
											{
												GuSavedataDialogSave gu = new GuSavedataDialogSave(savedataParams, this);
												openDialog(gu);
												dialogState = DialogState.confirmation;
											}
										}
										else
										{
											dialogState = DialogState.inProgress;
										}
									}
								}
								else
								{
									updateDialog();
								}
								break;
							}
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.confirmation:
							{
								if (!DialogActive)
								{
									if (ButtonPressed != SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_OK || NoSelected)
									{
										// The dialog has been cancelled or the user did not want to save.
										cancel();
									}
									else
									{
										closeDialog();
										dialogState = DialogState.inProgress;
									}
								}
								else
								{
									updateDialog();
								}
								break;
							}
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.inProgress:
							{
								try
								{
									if (log.DebugEnabled)
									{
										log.debug(string.Format("Saving savedata {0}", savedataParams.saveName));
									}
									savedataParams.save(mem, true);
									savedataParams.@base.result = 0;
								}
								catch (IOException)
								{
									savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_SAVE_ACCESS_ERROR;
								}
								catch (Exception e)
								{
									savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_SAVE_ACCESS_ERROR;
									log.error(e);
								}

								if (ReadyForVisible)
								{
									GuSavedataDialogCompleted gu = new GuSavedataDialogCompleted(savedataParams, this, saveListSelectionIcon0);
									openDialog(gu);
									dialogState = DialogState.completed;
								}
								break;
							}
							case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.completed:
							{
								if (!DialogActive)
								{
									quitDialog();
								}
								else
								{
									updateDialog();
								}
								break;
							}
						case pspsharp.HLE.modules.sceUtility.UtilityDialogState.DialogState.quit:
							// Nothing to do
							break;
						}
						break;
					}

					case SceUtilitySavedataParam.MODE_DELETE:
					{
						if (!DialogOpen)
						{
							// Search for valid saves.
							string pattern = savedataParams.gameName + ".*";

							string[] entries = Modules.IoFileMgrForUserModule.listFiles(SceUtilitySavedataParam.savedataPath, pattern);
							List<string> validNames = new List<string>();
							for (int i = 0; entries != null && i < entries.Length; i++)
							{
								string saveName = entries[i].Substring(savedataParams.gameName.Length);
								if (savedataParams.isPresent(savedataParams.gameName, saveName))
								{
									validNames.Add(saveName);
								}
							}

							GuSavedataDialog gu = new GuSavedataDialog(savedataParams, this, validNames.ToArray());
							openDialog(gu);
						}
						else if (!DialogActive)
						{
							if (ButtonPressed != SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_OK)
							{
								// Dialog cancelled
								savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_DELETE_BAD_PARAMS;
							}
							else if (string.ReferenceEquals(saveListSelection, null))
							{
								log.warn("Savedata MODE_DELETE no save selected");
								savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_DELETE_BAD_PARAMS;
							}
							else
							{
								string dirName = savedataParams.getBasePath(saveListSelection);
								if (savedataParams.deleteDir(dirName))
								{
									log.debug("Savedata MODE_DELETE deleting " + dirName);
									savedataParams.@base.result = 0;
								}
								else
								{
									savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_DELETE_ACCESS_ERROR;
								}
							}
							quitDialog(savedataParams.@base.result);
						}
						else
						{
							updateDialog();
						}
						break;
					}

					case SceUtilitySavedataParam.MODE_SIZES:
					{
						// "METAL SLUG XX" outputs the following on stdout after calling mode 8:
						//
						// ------ SIZES ------
						// ---------- savedata result ----------
						// result = 0x801103c7
						//
						// bind : un used(0x0).
						//
						// -- dir name --
						// title id : ULUS10495
						// user  id : METALSLUGXX
						//
						// ms free size
						//   cluster size(byte) : 32768 byte
						//   free cluster num   : 32768
						//   free size(KB)      : 1048576 KB
						//   free size(string)  : "1 GB"
						//
						// ms data size(titleId=ULUS10495, userId=METALSLUGXX)
						//   cluster num        : 0
						//   size (KB)          : 0 KB
						//   size (string)      : "0 KB"
						//   size (32KB)        : 0 KB
						//   size (32KB string) : "0 KB"
						//
						// utility data size
						//   cluster num        : 13
						//   size (KB)          : 416 KB
						//   size (string)      : "416 KB"
						//   size (32KB)        : 416 KB
						//   size (32KB string) : "416 KB"
						// error: SCE_UTILITY_SAVEDATA_TYPE_SIZES return 801103c7
						//
						int retval = 0;

						if (log.DebugEnabled)
						{
							log.debug(string.Format("MODE_SIZES: msFreeAddr=0x{0:X8}-0x{1:X8}, msDataAddr=0x{2:X8}-0x{3:X8}, utilityDataAddr=0x{4:X8}-0x{5:X8}", savedataParams.msFreeAddr, savedataParams.msFreeAddr + 20, savedataParams.msDataAddr, savedataParams.msDataAddr + 64, savedataParams.utilityDataAddr, savedataParams.utilityDataAddr + 28));
						}

						// Gets the amount of free space on the Memory Stick.
						int msFreeAddr = savedataParams.msFreeAddr;
						if (msFreeAddr != 0)
						{
							string memoryStickFreeSpaceString = MemoryStick.getSizeKbString(MemoryStick.FreeSizeKb);

							mem.write32(msFreeAddr + 0, MemoryStick.SectorSize);
							mem.write32(msFreeAddr + 4, MemoryStick.FreeSizeKb / MemoryStick.SectorSizeKb);
							mem.write32(msFreeAddr + 8, MemoryStick.FreeSizeKb);
							Utilities.writeStringNZ(mem, msFreeAddr + 12, 8, memoryStickFreeSpaceString);

							log.debug("Memory Stick Free Space = " + memoryStickFreeSpaceString);
						}

						// Gets the size of the data already saved on the Memory Stick.
						int msDataAddr = savedataParams.msDataAddr;
						if (msDataAddr != 0)
						{
							string gameName = Utilities.readStringNZ(mem, msDataAddr, 13);
							string saveName = Utilities.readStringNZ(mem, msDataAddr + 16, 20);

							saveName = savedataParams.getAnySaveName(gameName, saveName);
							if (savedataParams.isDirectoryPresent(gameName, saveName))
							{
								int savedataSizeKb = savedataParams.getSizeKb(gameName, saveName);
								int savedataSize32Kb = MemoryStick.getSize32Kb(savedataSizeKb);

								mem.write32(msDataAddr + 36, savedataSizeKb / MemoryStick.SectorSizeKb); // Number of sectors.
								mem.write32(msDataAddr + 40, savedataSizeKb); // Size in Kb.
								Utilities.writeStringNZ(mem, msDataAddr + 44, 8, MemoryStick.getSizeKbString(savedataSizeKb));
								mem.write32(msDataAddr + 52, savedataSize32Kb);
								Utilities.writeStringNZ(mem, msDataAddr + 56, 8, MemoryStick.getSizeKbString(savedataSize32Kb));

								log.debug("Memory Stick Used Space = " + MemoryStick.getSizeKbString(savedataSizeKb));
							}
							else
							{
								log.debug(string.Format("Savedata MODE_SIZES directory not found, gameName='{0}', saveName='{1}'", gameName, saveName));
								retval = SceKernelErrors.ERROR_SAVEDATA_SIZES_NO_DATA;
							}
						}

						// Gets the size of the data to be saved on the Memory Stick.
						int utilityDataAddr = savedataParams.utilityDataAddr;
						if (utilityDataAddr != 0)
						{
							int memoryStickRequiredSpaceKb = savedataParams.RequiredSizeKb;
							string memoryStickRequiredSpaceString = MemoryStick.getSizeKbString(memoryStickRequiredSpaceKb);
							int memoryStickRequiredSpace32Kb = MemoryStick.getSize32Kb(memoryStickRequiredSpaceKb);
							string memoryStickRequiredSpace32KbString = MemoryStick.getSizeKbString(memoryStickRequiredSpace32Kb);

							mem.write32(utilityDataAddr + 0, memoryStickRequiredSpaceKb / MemoryStick.SectorSizeKb);
							mem.write32(utilityDataAddr + 4, memoryStickRequiredSpaceKb);
							Utilities.writeStringNZ(mem, utilityDataAddr + 8, 8, memoryStickRequiredSpaceString);
							mem.write32(utilityDataAddr + 16, memoryStickRequiredSpace32Kb);
							Utilities.writeStringNZ(mem, utilityDataAddr + 20, 8, memoryStickRequiredSpace32KbString);

							log.debug("Memory Stick Required Space = " + memoryStickRequiredSpaceString);
						}
						savedataParams.@base.result = retval;
						break;
					}

					case SceUtilitySavedataParam.MODE_AUTODELETE:
					{
						if (savedataParams.deleteDir(savedataParams.BasePath))
						{
							savedataParams.@base.result = 0;
						}
						else
						{
							log.warn("Savedata MODE_AUTODELETE directory not found!");
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_DELETE_NO_DATA;
						}
						// Tests show certain applications expect the PSP to change the
						// dialog status automatically after delete.
						status = PSP_UTILITY_DIALOG_STATUS_QUIT;
						break;
					}

					case SceUtilitySavedataParam.MODE_SINGLEDELETE:
					{
						if (savedataParams.deleteFile(savedataParams.fileName))
						{
							savedataParams.@base.result = 0;
						}
						else
						{
							log.warn("Savedata MODE_SINGLEDELETE file not found!");
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_DELETE_NO_MEMSTICK;
						}
						// Tests show certain applications expect the PSP to change the
						// dialog status automatically after delete.
						status = PSP_UTILITY_DIALOG_STATUS_QUIT;
						break;
					}

					case SceUtilitySavedataParam.MODE_LIST:
					{
						int buffer4Addr = savedataParams.idListAddr;
						if (Memory.isAddressGood(buffer4Addr))
						{
							int maxEntries = mem.read32(buffer4Addr + 0);
							int entriesAddr = mem.read32(buffer4Addr + 8);
							string saveName = savedataParams.saveName;
							// PSP file name pattern:
							//   '?' matches one character
							//   '*' matches any character sequence
							// To convert to regular expressions:
							//   replace '?' with '.'
							//   replace '*' with '.*'
							string pattern = saveName.Replace('?', '.');
							pattern = pattern.Replace("*", ".*");
							pattern = savedataParams.gameName + pattern;

							string[] entries = Modules.IoFileMgrForUserModule.listFiles(SceUtilitySavedataParam.savedataPath, pattern);
							int numEntries = entries == null ? 0 : entries.Length;
							numEntries = System.Math.Min(numEntries, maxEntries);
							for (int i = 0; i < numEntries; i++)
							{
								string filePath = SceUtilitySavedataParam.savedataPath + "/" + entries[i];
								SceIoStat stat = Modules.IoFileMgrForUserModule.statFile(filePath);
								int entryAddr = entriesAddr + i * 72;
								if (stat != null)
								{
									mem.write32(entryAddr + 0, stat.mode);
									stat.ctime.write(mem, entryAddr + 4);
									stat.atime.write(mem, entryAddr + 20);
									stat.mtime.write(mem, entryAddr + 36);
								}
								string entryName = entries[i].Substring(savedataParams.gameName.Length);
								// File names are upper cased in some conditions
								entryName = getMsFileName(entryName);
								Utilities.writeStringNZ(mem, entryAddr + 52, 20, entryName);

								if (log.DebugEnabled)
								{
									log.debug(string.Format("MODE_LIST returning filePath={0}, stat={1}, entryName={2} at 0x{3:X8}", filePath, stat, entryName, entryAddr));
								}
							}
							mem.write32(buffer4Addr + 4, numEntries);

							if (log.DebugEnabled)
							{
								log.debug(string.Format("MODE_LIST returning {0:D} entries", numEntries));
							}
						}
						savedataParams.@base.result = checkMultipleCallStatus();
						break;
					}

					case SceUtilitySavedataParam.MODE_FILES:
					{
						int fileListAddr = savedataParams.fileListAddr;
						if (Memory.isAddressGood(fileListAddr))
						{
							int saveFileSecureMaxNumEntries = mem.read32(fileListAddr);
							int saveFileMaxNumEntries = mem.read32(fileListAddr + 4);
							int systemMaxNumEntries = mem.read32(fileListAddr + 8);

							if (log.DebugEnabled)
							{
								log.debug(string.Format("MaxFiles in FileList: secure={0:D}, normal={1:D}, system={2:D}", saveFileSecureMaxNumEntries, saveFileMaxNumEntries, systemMaxNumEntries));
							}

							int saveFileSecureEntriesAddr = mem.read32(fileListAddr + 24);
							int saveFileEntriesAddr = mem.read32(fileListAddr + 28);
							int systemEntriesAddr = mem.read32(fileListAddr + 32);

							string path = savedataParams.BasePath;
							string[] entries = Modules.IoFileMgrForUserModule.listFiles(path, null);

							int maxNumEntries = (entries == null) ? 0 : entries.Length;
							int saveFileSecureNumEntries = 0;
							int saveFileNumEntries = 0;
							int systemFileNumEntries = 0;

							// List all files in the savedata (normal and/or encrypted).
							for (int i = 0; i < maxNumEntries; i++)
							{
								// File names are upper cased in some conditions
								string entry = getMsFileName(entries[i]);
								string filePath = path + "/" + entry;
								SceIoStat stat = Modules.IoFileMgrForUserModule.statFile(filePath);

								// System files.
								if (SceUtilitySavedataParam.isSystemFile(entry))
								{
									if (systemEntriesAddr != 0 && systemFileNumEntries < systemMaxNumEntries)
									{
										int entryAddr = systemEntriesAddr + systemFileNumEntries * 80;
										if (stat != null)
										{
											mem.write32(entryAddr + 0, stat.mode);
											mem.write64(entryAddr + 8, stat.size);
											stat.ctime.write(mem, entryAddr + 16);
											stat.atime.write(mem, entryAddr + 32);
											stat.mtime.write(mem, entryAddr + 48);
										}
										Utilities.writeStringNZ(mem, entryAddr + 64, 16, entry);
										systemFileNumEntries++;
									}
								}
								else if (savedataParams.isSecureFile(entry))
								{
									// Write to secure.
									if (saveFileSecureEntriesAddr != 0 && saveFileSecureNumEntries < saveFileSecureMaxNumEntries)
									{
										int entryAddr = saveFileSecureEntriesAddr + saveFileSecureNumEntries * 80;
										if (stat != null)
										{
											mem.write32(entryAddr + 0, stat.mode);
											// Write the file size
											long fileSize = stat.size;
											if (CryptoEngine.SavedataCryptoStatus)
											{
												// Write the size of the decrypted file (fileSize -= IV).
												fileSize -= 0x10;
											}
											mem.write64(entryAddr + 8, fileSize);
											stat.ctime.write(mem, entryAddr + 16);
											stat.atime.write(mem, entryAddr + 32);
											stat.mtime.write(mem, entryAddr + 48);
										}
										Utilities.writeStringNZ(mem, entryAddr + 64, 16, entry);
										saveFileSecureNumEntries++;
									}
								}
								else
								{
									// Write to normal.
									if (saveFileEntriesAddr != 0 && saveFileNumEntries < saveFileMaxNumEntries)
									{
										int entryAddr = saveFileEntriesAddr + saveFileNumEntries * 80;
										if (stat != null)
										{
											mem.write32(entryAddr + 0, stat.mode);
											mem.write64(entryAddr + 8, stat.size);
											stat.ctime.write(mem, entryAddr + 16);
											stat.atime.write(mem, entryAddr + 32);
											stat.mtime.write(mem, entryAddr + 48);
										}
										Utilities.writeStringNZ(mem, entryAddr + 64, 16, entry);
										saveFileNumEntries++;
									}
								}
							}

							if (entries == null)
							{
								savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_NO_DATA;
							}
							else
							{
								savedataParams.@base.result = checkMultipleCallStatus();
							}

							if (savedataParams.@base.result == 0)
							{
								// These values are only written when no error is returned
								mem.write32(fileListAddr + 12, saveFileSecureNumEntries);
								mem.write32(fileListAddr + 16, saveFileNumEntries);
								mem.write32(fileListAddr + 20, systemFileNumEntries);
							}

							if (log.DebugEnabled)
							{
								log.debug(string.Format("FileList: {0}", Utilities.getMemoryDump(fileListAddr, 36)));
								if (saveFileSecureEntriesAddr != 0 && saveFileSecureNumEntries > 0)
								{
									log.debug(string.Format("SecureEntries: {0}", Utilities.getMemoryDump(saveFileSecureEntriesAddr, saveFileSecureNumEntries * 80)));
								}
								if (saveFileEntriesAddr != 0 && saveFileNumEntries > 0)
								{
									log.debug(string.Format("NormalEntries: {0}", Utilities.getMemoryDump(saveFileEntriesAddr, saveFileNumEntries * 80)));
								}
								if (systemEntriesAddr != 0 && systemFileNumEntries > 0)
								{
									log.debug(string.Format("SystemEntries: {0}", Utilities.getMemoryDump(systemEntriesAddr, systemFileNumEntries * 80)));
								}
							}
						}
						break;
					}

					case SceUtilitySavedataParam.MODE_MAKEDATA:
					case SceUtilitySavedataParam.MODE_MAKEDATASECURE:
					{
						// Split saving version.
						// Write system data files (encrypted or not).
						try
						{
							savedataParams.save(mem, savedataParams.mode == SceUtilitySavedataParam.MODE_MAKEDATASECURE);
							savedataParams.@base.result = checkMultipleCallStatus();
						}
						catch (IOException)
						{
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_ACCESS_ERROR;
						}
						catch (Exception e)
						{
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_ACCESS_ERROR;
							log.error(e);
						}
						break;
					}

					case SceUtilitySavedataParam.MODE_READ:
					case SceUtilitySavedataParam.MODE_READSECURE:
					{
						// Sub-types of mode LOAD.
						// Loads data and can be called multiple times for updating.
						if (string.ReferenceEquals(savedataParams.saveName, null) || savedataParams.saveName.Length == 0)
						{
							if (savedataParams.saveNameList != null && savedataParams.saveNameList.Length > 0)
							{
								savedataParams.saveName = savedataParams.saveNameList[0];
							}
						}

						try
						{
							savedataParams.load(mem);
							if (log.TraceEnabled)
							{
								log.trace(string.Format("MODE_READ/MODE_READSECURE reading {0}", Utilities.getMemoryDump(savedataParams.dataBuf, savedataParams.dataSize, 4, 16)));
							}
							savedataParams.@base.result = checkMultipleCallStatus();
							savedataParams.write(mem);
						}
						catch (FileNotFoundException)
						{
							if (savedataParams.GameDirectoryPresent)
							{
								// Directory exists but file does not exist
								savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_FILE_NOT_FOUND;
							}
							else
							{
								// Directory does not exist
								savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_NO_DATA;
							}
						}
						catch (IOException)
						{
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_NO_DATA;
						}
						catch (Exception e)
						{
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_ACCESS_ERROR;
							log.error(e);
						}
						break;
					}

					case SceUtilitySavedataParam.MODE_WRITE:
					case SceUtilitySavedataParam.MODE_WRITESECURE:
					{
						// Sub-types of mode SAVE.
						// Writes data and can be called multiple times for updating.
						try
						{
							savedataParams.save(mem, savedataParams.mode == SceUtilitySavedataParam.MODE_WRITESECURE);
							savedataParams.@base.result = checkMultipleCallStatus();
						}
						catch (IOException)
						{
							if (!savedataParams.GameDirectoryPresent)
							{
								savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_NO_DATA;
							}
							else
							{
								savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_ACCESS_ERROR;
							}
						}
						catch (Exception e)
						{
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_ACCESS_ERROR;
							log.error(e);
						}
						break;
					}

					case SceUtilitySavedataParam.MODE_DELETEDATA:
						// Sub-type of mode DELETE.
						// Deletes the contents of only one specified file.
						if (savedataParams.deleteFile(savedataParams.fileName))
						{
							savedataParams.@base.result = checkMultipleCallStatus();
						}
						else
						{
							log.warn("Savedata MODE_DELETEDATA no data found!");
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_NO_DATA;
						}
						break;

					case SceUtilitySavedataParam.MODE_GETSIZE:
						int buffer6Addr = savedataParams.sizeAddr;
						bool isPresent = savedataParams.Present;

						if (Memory.isAddressGood(buffer6Addr))
						{
							int saveFileSecureNumEntries = mem.read32(buffer6Addr + 0);
							int saveFileNumEntries = mem.read32(buffer6Addr + 4);
							int saveFileSecureEntriesAddr = mem.read32(buffer6Addr + 8);
							int saveFileEntriesAddr = mem.read32(buffer6Addr + 12);

							int totalSizeKb = 0;

							for (int i = 0; i < saveFileSecureNumEntries; i++)
							{
								int entryAddr = saveFileSecureEntriesAddr + i * 24;
								long size = mem.read64(entryAddr);
								string fileName = Utilities.readStringNZ(entryAddr + 8, 16);
								int sizeKb = Utilities.getSizeKb(size);
								if (log.DebugEnabled)
								{
									log.debug(string.Format("   Secure File '{0}', size {1:D} ({2:D} KB)", fileName, size, sizeKb));
								}

								totalSizeKb += sizeKb;
							}
							for (int i = 0; i < saveFileNumEntries; i++)
							{
								int entryAddr = saveFileEntriesAddr + i * 24;
								long size = mem.read64(entryAddr);
								string fileName = Utilities.readStringNZ(entryAddr + 8, 16);
								int sizeKb = Utilities.getSizeKb(size);
								if (log.DebugEnabled)
								{
									log.debug(string.Format("   File '{0}', size {1:D} ({2:D} KB)", fileName, size, sizeKb));
								}

								totalSizeKb += sizeKb;
							}

							// Free MS size.
							int freeSizeKb = MemoryStick.FreeSizeKb;
							string memoryStickFreeSpaceString = MemoryStick.getSizeKbString(freeSizeKb);
							mem.write32(buffer6Addr + 16, MemoryStick.SectorSize);
							mem.write32(buffer6Addr + 20, freeSizeKb / MemoryStick.SectorSizeKb);
							mem.write32(buffer6Addr + 24, freeSizeKb);
							Utilities.writeStringNZ(mem, buffer6Addr + 28, 8, memoryStickFreeSpaceString);

							// If there's not enough size, we have to write how much size we need.
							// With enough size, our needed size is always 0.
							if (totalSizeKb > freeSizeKb)
							{
								int neededSizeKb = totalSizeKb - freeSizeKb;

								// Additional size needed to write savedata.
								mem.write32(buffer6Addr + 36, neededSizeKb);
								Utilities.writeStringNZ(mem, buffer6Addr + 40, 8, MemoryStick.getSizeKbString(neededSizeKb));

								if (isPresent)
								{
									// Additional size needed to overwrite savedata.
									mem.write32(buffer6Addr + 48, neededSizeKb);
									Utilities.writeStringNZ(mem, buffer6Addr + 52, 8, MemoryStick.getSizeKbString(neededSizeKb));
								}
							}
							else
							{
								mem.write32(buffer6Addr + 36, 0);
								if (isPresent)
								{
									mem.write32(buffer6Addr + 48, 0);
								}
							}
						}

						// MODE_GETSIZE also checks if a MemoryStick is inserted and if there're no previous data.
						if (MemoryStick.StateMs != MemoryStick.PSP_MEMORYSTICK_STATE_DRIVER_READY)
						{
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_NO_MEMSTICK;
						}
						else if (!isPresent)
						{
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_NO_DATA;
						}
						else
						{
							savedataParams.@base.result = checkMultipleCallStatus();
						}
						break;

					case SceUtilitySavedataParam.MODE_ERASESECURE:
						if (!string.ReferenceEquals(savedataParams.fileName, null))
						{
							string save = savedataParams.getFileName(savedataParams.saveName, savedataParams.fileName);
							if (Modules.IoFileMgrForUserModule.deleteFile(save))
							{
								savedataParams.@base.result = checkMultipleCallStatus();
							}
							else if (savedataParams.GameDirectoryPresent)
							{
								savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_NO_DATA;
							}
							else
							{
								savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_FILE_NOT_FOUND;
							}
						}
						else
						{
							log.warn("Savedata MODE_ERASESECURE no fileName specified!");
							savedataParams.@base.result = SceKernelErrors.ERROR_SAVEDATA_RW_NO_DATA;
						}
						break;

					default:
						log.warn(string.Format("Savedata - Unsupported mode {0:D}", savedataParams.mode));
						quitDialog(-1);
						break;
				}

				savedataParams.@base.writeResult(mem);
				if (log.DebugEnabled)
				{
					log.debug(string.Format("hleUtilitySavedataDisplay result: 0x{0:X8}", savedataParams.@base.result));
				}

				return false;
			}

			protected internal override bool hasDialog()
			{
				switch (savedataParams.mode)
				{
					// Only these modes have a dialog with the user
					case SceUtilitySavedataParam.MODE_LOAD:
					case SceUtilitySavedataParam.MODE_SAVE:
					case SceUtilitySavedataParam.MODE_LISTLOAD:
					case SceUtilitySavedataParam.MODE_LISTSAVE:
					case SceUtilitySavedataParam.MODE_LISTDELETE:
					case SceUtilitySavedataParam.MODE_DELETE:
					case SceUtilitySavedataParam.MODE_SINGLEDELETE:
						return true;
				}

				// The other modes are silent
				return false;
			}
		}

		protected internal class MsgDialogUtilityDialogState : UtilityDialogState
		{

			protected internal SceUtilityMsgDialogParams msgDialogParams;

			public MsgDialogUtilityDialogState(string name) : base(name)
			{
			}

			protected internal override bool executeUpdateVisible()
			{
				Memory mem = Processor.memory;

				if (!DialogOpen)
				{
					GuMsgDialog gu = new GuMsgDialog(msgDialogParams, this);
					openDialog(gu);
				}
				else if (!DialogActive)
				{
					// buttonPressed is only set for mode TEXT, not for mode ERROR
					if (msgDialogParams.mode == SceUtilityMsgDialogParams.PSP_UTILITY_MSGDIALOG_MODE_TEXT)
					{
						msgDialogParams.buttonPressed = ButtonPressed;
					}
					else if (msgDialogParams.mode == SceUtilityMsgDialogParams.PSP_UTILITY_MSGDIALOG_MODE_ERROR)
					{
						msgDialogParams.buttonPressed = SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_ESC;
					}
					else
					{
						msgDialogParams.buttonPressed = SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_INVALID;
					}

					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceUtilityMsgDialog returning buttonPressed={0:D}", msgDialogParams.buttonPressed));
					}
					quitDialog(0);
					msgDialogParams.write(mem);
				}
				else
				{
					updateDialog();
				}

				return false;
			}

			protected internal override pspUtilityBaseDialog createParams()
			{
				msgDialogParams = new SceUtilityMsgDialogParams();
				return msgDialogParams;
			}
		}

		protected internal class OskUtilityDialogState : UtilityDialogState
		{

			protected internal SceUtilityOskParams oskParams;
			protected internal OskDialog oskDialog;

			public OskUtilityDialogState(string name) : base(name)
			{
			}

			protected internal override bool executeUpdateVisible()
			{
				Memory mem = Processor.memory;

				if (!DialogOpen)
				{
					oskDialog = new OskDialog(oskParams, this);
					openDialog(oskDialog);
				}
				else if (!DialogActive)
				{
					if (oskDialog.buttonPressed == SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_OK)
					{
						oskParams.oskData.result = SceUtilityOskParams.SceUtilityOskData.PSP_UTILITY_OSK_DATA_CHANGED;
						oskParams.oskData.outText = oskDialog.textField.Text;
						log.info("hleUtilityOskDisplay returning '" + oskParams.oskData.outText + "'");
					}
					else
					{
						oskParams.oskData.result = SceUtilityOskParams.SceUtilityOskData.PSP_UTILITY_OSK_DATA_CANCELED;
						oskParams.oskData.outText = oskDialog.textField.Text;
						log.info("hleUtilityOskDisplay cancelled");
					}
					quitDialog(0);
					oskParams.write(mem);
				}
				else
				{
					oskDialog.checkController();
					updateDialog();
				}

				return false;
			}

			protected internal override pspUtilityBaseDialog createParams()
			{
				oskParams = new SceUtilityOskParams();
				return oskParams;
			}
		}

		protected internal class GameSharingUtilityDialogState : UtilityDialogState
		{

			protected internal SceUtilityGameSharingParams gameSharingParams;

			public GameSharingUtilityDialogState(string name) : base(name)
			{
			}

			protected internal override bool executeUpdateVisible()
			{
				// TODO to be implemented
				return false;
			}

			protected internal override pspUtilityBaseDialog createParams()
			{
				gameSharingParams = new SceUtilityGameSharingParams();
				return gameSharingParams;
			}

			protected internal override bool hasDialog()
			{
				return false;
			}
		}

		protected internal class NetconfUtilityDialogState : UtilityDialogState
		{

			protected internal SceUtilityNetconfParams netconfParams;

			public NetconfUtilityDialogState(string name) : base(name)
			{
			}

			protected internal override bool executeUpdateVisible()
			{
				bool keepVisible = false;

				if (netconfParams.netAction == SceUtilityNetconfParams.PSP_UTILITY_NETCONF_CONNECT_APNET || netconfParams.netAction == SceUtilityNetconfParams.PSP_UTILITY_NETCONF_CONNECT_APNET_LASTUSED)
				{
					int state = Modules.sceNetApctlModule.hleNetApctlGetState();

					// The Netconf dialog stays visible until the network reaches
					// the state PSP_NET_APCTL_STATE_GOT_IP.
					if (state == sceNetApctl.PSP_NET_APCTL_STATE_GOT_IP)
					{
						quitDialog();
						keepVisible = false;
					}
					else
					{
						keepVisible = true;
						if (state == sceNetApctl.PSP_NET_APCTL_STATE_DISCONNECTED)
						{
							// When connecting with infrastructure, simulate a connection
							// using the first network configuration entry.
							Modules.sceNetApctlModule.hleNetApctlConnect(1);
						}
					}
				}
				else if (netconfParams.netAction == SceUtilityNetconfParams.PSP_UTILITY_NETCONF_CONNECT_ADHOC || netconfParams.netAction == SceUtilityNetconfParams.PSP_UTILITY_NETCONF_CREATE_ADHOC || netconfParams.netAction == SceUtilityNetconfParams.PSP_UTILITY_NETCONF_JOIN_ADHOC)
				{
					int state = Modules.sceNetAdhocctlModule.hleNetAdhocctlGetState();

					// The Netconf dialog stays visible until the network reaches
					// the state PSP_ADHOCCTL_STATE_CONNECTED.
					if (state == sceNetAdhocctl.PSP_ADHOCCTL_STATE_CONNECTED)
					{
						quitDialog();
						keepVisible = false;
					}
					else
					{
						updateDialog();
						keepVisible = true;
						if (state == sceNetAdhocctl.PSP_ADHOCCTL_STATE_DISCONNECTED && netconfParams.netconfData != null)
						{
							// Connect to the given group name
							Modules.sceNetAdhocctlModule.hleNetAdhocctlConnect(netconfParams.netconfData.groupName);
						}
					}
				}

				return keepVisible;
			}

			protected internal override pspUtilityBaseDialog createParams()
			{
				netconfParams = new SceUtilityNetconfParams();
				return netconfParams;
			}
		}

		protected internal class ScreenshotUtilityDialogState : UtilityDialogState
		{
			protected internal SceUtilityScreenshotParams screenshotParams;

			public ScreenshotUtilityDialogState(string name) : base(name)
			{
			}

			protected internal override bool executeUpdateVisible()
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("SceUtilityScreenshotParams {0}", Utilities.getMemoryDump(paramsAddr.Address, @params.@sizeof())));
				}

				if (status == PSP_UTILITY_DIALOG_STATUS_VISIBLE && (screenshotParams.ContModeAuto || screenshotParams.ContModeFinish))
				{
					status = PSP_UTILITY_DIALOG_STATUS_SCREENSHOT_UNKNOWN;
				}

				if (status == PSP_UTILITY_DIALOG_STATUS_VISIBLE && !screenshotParams.ContModeAuto && !screenshotParams.ContModeFinish && Memory.isAddressGood(screenshotParams.imgFrameBufAddr))
				{
					Buffer buffer = Memory.Instance.getBuffer(screenshotParams.imgFrameBufAddr, screenshotParams.imgFrameBufWidth * screenshotParams.displayHeigth * pspsharp.graphics.RE.IRenderingEngine_Fields.sizeOfTextureType[screenshotParams.imgPixelFormat]);
					string directoyName = string.format(Settings.Instance.getDirectoryMapping("ms0") + "PSP/SCREENSHOT/%s/", screenshotParams.screenshotID);
					System.IO.Directory.CreateDirectory(directoyName);
					string fileName = null;
					string fileSuffix = screenshotParams.imgFormat == PSP_UTILITY_SCREENSHOT_FORMAT_JPEG ? "jpeg" : "png";
					if (screenshotParams.nameRule == PSP_UTILITY_SCREENSHOT_NAMERULE_AUTONUM)
					{
						for (int fileIndex = 1; fileIndex <= 9999; fileIndex++)
						{
							fileName = string.Format("{0}{1}_{2:D4}.{3}", directoyName, screenshotParams.fileName, fileIndex, fileSuffix);
							if (!System.IO.Directory.Exists(fileName) || System.IO.File.Exists(fileName))
							{
								break;
							}
						}
					}
					else
					{
						fileName = string.Format("{0}{1}.{2}", directoyName, screenshotParams.fileName, fileSuffix);
					}
					CaptureImage captureImage = new CaptureImage(screenshotParams.imgFrameBufAddr, 0, buffer, screenshotParams.displayWidth, screenshotParams.displayHeigth, screenshotParams.imgFrameBufWidth, screenshotParams.imgPixelFormat, false, 0, false, true, null);
					if (screenshotParams.imgFormat == PSP_UTILITY_SCREENSHOT_FORMAT_PNG)
					{
						captureImage.FileFormat = "png";
					}
					else if (screenshotParams.imgFormat == PSP_UTILITY_SCREENSHOT_FORMAT_JPEG)
					{
						captureImage.FileFormat = "jpg";
					}
					captureImage.FileName = fileName;
					try
					{
						captureImage.write();
					}
					catch (IOException e)
					{
						log.error("sceUtilityScreenshot", e);
					}
				}

				// TODO to be implemented
				return false;
			}

			protected internal virtual int executeContStart(TPointer paramsAddr)
			{
				// Continuous mode which takes several screenshots
				// on regular intervals set by an internal counter.

				// To execute the cont mode, the screenshot utility must
				// be initialized with sceUtilityScreenshotInitStart and the startupType
				// parameter has to be PSP_UTILITY_SCREENSHOT_TYPE_CONT_AUTO, otherwise, an
				// error is returned.
				if (status != PSP_UTILITY_DIALOG_STATUS_SCREENSHOT_UNKNOWN)
				{
					return SceKernelErrors.ERROR_UTILITY_INVALID_STATUS;
				}

				this.paramsAddr = paramsAddr;
				this.@params = createParams();
				@params.read(paramsAddr);
				if (log.InfoEnabled)
				{
					log.info(string.Format("{0}ContStart {1}", name, @params.ToString()));
				}

				if (!screenshotParams.ContModeAuto)
				{
					return SceKernelErrors.ERROR_SCREENSHOT_CONT_MODE_NOT_INIT;
				}

				// PSP is moving to status QUIT
				status = PSP_UTILITY_DIALOG_STATUS_QUIT;

				return 0;
			}

			protected internal override pspUtilityBaseDialog createParams()
			{
				screenshotParams = new SceUtilityScreenshotParams();
				return screenshotParams;
			}

			protected internal override bool hasDialog()
			{
				return false;
			}
		}

		protected internal class GamedataInstallUtilityDialogState : UtilityDialogState
		{

			protected internal SceUtilityGamedataInstallParams gamedataInstallParams;

			public GamedataInstallUtilityDialogState(string name) : base(name)
			{
			}

			protected internal override pspUtilityBaseDialog createParams()
			{
				gamedataInstallParams = new SceUtilityGamedataInstallParams();
				return gamedataInstallParams;
			}

			protected internal override bool executeUpdateVisible()
			{
				IoFileMgrForUser fileMgr = Modules.IoFileMgrForUserModule;
				StringBuilder sourceLocalFileName = new StringBuilder();
				IVirtualFileSystem ivfs = fileMgr.getVirtualFileSystem("disc0:/PSP_GAME/INSDIR", sourceLocalFileName);
				if (ivfs != null)
				{
					string[] fileNames = ivfs.ioDopen(sourceLocalFileName.ToString());
					if (fileNames != null)
					{
						ivfs.ioDclose(sourceLocalFileName.ToString());

						StringBuilder destinationLocalFileName = new StringBuilder();
						IVirtualFileSystem ovfs = fileMgr.getVirtualFileSystem(string.Format("{0}{1}{2}", SceUtilitySavedataParam.savedataPath, gamedataInstallParams.gameName, gamedataInstallParams.dataName), destinationLocalFileName);
						if (ovfs != null)
						{
							int numberFiles = 0;
							for (int i = 0; i < fileNames.Length; i++)
							{
								string fileName = fileNames[i];
								// Skip iso special files
								if (!fileName.Equals(".") && !fileName.Equals("\x0001"))
								{
									string sourceFileName = string.Format("{0}/{1}", sourceLocalFileName.ToString(), fileName);
									IVirtualFile ivf = ivfs.ioOpen(sourceFileName, IoFileMgrForUser.PSP_O_RDONLY, 0);
									if (ivf != null)
									{
										string destinationFileName = string.Format("{0}/{1}", destinationLocalFileName.ToString(), fileName);
										IVirtualFile ovf = ovfs.ioOpen(destinationFileName, IoFileMgrForUser.PSP_O_WRONLY | IoFileMgrForUser.PSP_O_CREAT, 0x1FF);
										if (ovf != null)
										{
											if (log.DebugEnabled)
											{
												log.debug(string.Format("GamedataInstall: copying file disc0:/{0} to ms0:/{1}", sourceFileName, destinationFileName));
											}
											sbyte[] buffer = new sbyte[512 * 1024];
											long restLength = ivf.length();
											while (restLength > 0)
											{
												int length = buffer.Length;
												if (length > restLength)
												{
													length = (int) restLength;
												}
												length = ivf.ioRead(buffer, 0, length);
												ovf.ioWrite(buffer, 0, length);

												restLength -= length;
											}
											ovf.ioClose();
											numberFiles++;
										}
										ivf.ioClose();
									}
								}
							}
							// TODO Not sure about the values to return here
							gamedataInstallParams.unkResult1 = numberFiles;
							gamedataInstallParams.unkResult2 = numberFiles;
							gamedataInstallParams.write(paramsAddr);
						}
					}
				}

				return false;
			}

			protected internal override bool hasDialog()
			{
				return false;
			}

			protected internal override int ShutdownDelay
			{
				get
				{
					return 50000;
				}
			}
		}

		protected internal class NpSigninUtilityDialogState : UtilityDialogState
		{

			protected internal SceUtilityNpSigninParams npSigninParams;

			public NpSigninUtilityDialogState(string name) : base(name)
			{
			}

			protected internal override pspUtilityBaseDialog createParams()
			{
				npSigninParams = new SceUtilityNpSigninParams();
				return npSigninParams;
			}

			protected internal override bool executeUpdateVisible()
			{
				Memory mem = Processor.memory;

				npSigninParams.signinStatus = SceUtilityNpSigninParams.NP_SIGNING_STATUS_OK;
				npSigninParams.write(mem);

				int sceNp_E24DA399 = NIDMapper.Instance.getAddressByName("sceNp_E24DA399");
				if (sceNp_E24DA399 != 0)
				{
					int address = mem.read16(sceNp_E24DA399 + 0) << 16;
					address += (short) mem.read16(sceNp_E24DA399 + 8);
					if (Memory.isAddressGood(address))
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceNp_E24DA399 Address 0x{0:X8}", address));
						}
						mem.write32(address, 1);
					}
				}

				int sceNp_C48F2847 = NIDMapper.Instance.getAddressByName("sceNp_C48F2847");
				if (sceNp_C48F2847 != 0)
				{
					int address = mem.read16(sceNp_C48F2847 + 0x74) << 16;
					address += (short) mem.read16(sceNp_C48F2847 + 0x78);
					if (Memory.isAddressGood(address))
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceNp_C48F2847 Address 0x{0:X8}", address));
						}
						Utilities.writeStringZ(mem, address, Modules.sceNpModule.OnlineId);
					}
				}

				int sceNpService_7EF4312E = NIDMapper.Instance.getAddressByName("sceNpService_7EF4312E");
				if (sceNpService_7EF4312E != 0)
				{
					int subAddress = (mem.read32(sceNpService_7EF4312E + 0x78) & 0x3FFFFFF) << 2;
					int address = mem.read16(subAddress + 0x14) << 16;
					address += (short) mem.read16(subAddress + 0x38);
					if (Memory.isAddressGood(address))
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceNpService_7EF4312E Address 0x{0:X8}", address));
						}
						SysMemInfo memInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "sceNpService_7EF4312E", SysMemUserForUser.PSP_SMEM_Low, 100, 0);
						mem.write32(address, memInfo.addr);
						mem.memset(memInfo.addr, (sbyte) 0, 100);
						Utilities.writeStringZ(mem, memInfo.addr + 12, Modules.sceNpModule.OnlineId);
						Modules.SysMemForKernelModule.SysMemUserForUser_945E45DA(new TPointer(mem, memInfo.addr + 64));
					}
				}

				int sceNp_02CA8CAA = NIDMapper.Instance.getAddressByName("sceNp_02CA8CAA");
				if (sceNp_02CA8CAA != 0)
				{
					int address = mem.read16(sceNp_02CA8CAA + 0x8C) << 16;
					address += (short) mem.read16(sceNp_02CA8CAA + 0x90);
					if (Memory.isAddressGood(address))
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("sceNp_02CA8CAA Address 0x{0:X8}", address));
						}
						mem.write64(address, Modules.sceRtcModule.hleGetCurrentTick());
					}
				}

				return false;
			}

			protected internal override bool hasDialog()
			{
				return false;
			}
		}

		protected internal class HtmlViewerUtilityDialogState : UtilityDialogState
		{

			protected internal SceUtilityHtmlViewerParams htmlViewerParams;

			public HtmlViewerUtilityDialogState(string name) : base(name)
			{
			}

			protected internal override bool executeUpdateVisible()
			{
				// TODO to be implemented
				return false;
			}

			protected internal override pspUtilityBaseDialog createParams()
			{
				htmlViewerParams = new SceUtilityHtmlViewerParams();
				return htmlViewerParams;
			}

			protected internal override bool hasDialog()
			{
				return false;
			}
		}

		protected internal abstract class UtilityDialog : JComponent
		{

			internal const long serialVersionUID = -993546461292372048L;
			protected internal JDialog dialog;
			protected internal int buttonPressed;
			protected internal JPanel messagePane;
			protected internal JPanel buttonPane;
			protected internal ActionListener closeActionListener;
			protected internal const string actionCommandOK = "OK";
			protected internal const string actionCommandYES = "YES";
			protected internal const string actionCommandNO = "NO";
			protected internal const string actionCommandESC = "ESC";
			protected internal UtilityDialogState utilityDialogState;
			protected internal string confirmButtonActionCommand = actionCommandOK;
			protected internal string cancelButtonActionCommand = actionCommandESC;
			protected internal long pressedTimestamp;
			protected internal const int repeatDelay = 200000;
			protected internal bool downPressedButton;
			protected internal bool downPressedAnalog;
			protected internal bool upPressedButton;
			protected internal bool upPressedAnalog;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: protected void createDialog(final UtilityDialogState utilityDialogState, String message)
			protected internal virtual void createDialog(UtilityDialogState utilityDialogState, string message)
			{
				this.utilityDialogState = utilityDialogState;

				string title = string.Format("Message from {0}", State.title);
				dialog = new JDialog((Frame) null, title, false);
				dialog.DefaultCloseOperation = JFrame.DISPOSE_ON_CLOSE;

				messagePane = new JPanel();
				messagePane.Border = new EmptyBorder(5, 10, 5, 10);
				messagePane.Layout = new BoxLayout(messagePane, BoxLayout.Y_AXIS);

				if (!string.ReferenceEquals(message, null))
				{
					message = formatMessageForDialog(message);
					// Split the message according to the new lines
					while (message.Length > 0)
					{
						int newLinePosition = message.IndexOf("\n", StringComparison.Ordinal);
						JLabel label = new JLabel();
						label.HorizontalAlignment = JLabel.CENTER;
						label.AlignmentX = CENTER_ALIGNMENT;
						if (newLinePosition < 0)
						{
							label.Text = message;
							message = "";
						}
						else
						{
							string messagePart = message.Substring(0, newLinePosition);
							label.Text = messagePart;
							message = message.Substring(newLinePosition + 1);
						}
						messagePane.add(label);
					}
				}

				if (JDialog.DefaultLookAndFeelDecorated)
				{
					if (UIManager.LookAndFeel.SupportsWindowDecorations)
					{
						dialog.Undecorated = true;
						RootPane.WindowDecorationStyle = JRootPane.INFORMATION_DIALOG;
					}
				}

				buttonPane = new JPanel();
				buttonPane.Border = new EmptyBorder(5, 10, 5, 10);
				buttonPane.Layout = new BoxLayout(buttonPane, BoxLayout.X_AXIS);

				closeActionListener = new ActionListenerAnonymousInnerClass(this);
			}

			private class ActionListenerAnonymousInnerClass : ActionListener
			{
				private readonly UtilityDialog outerInstance;

				public ActionListenerAnonymousInnerClass(UtilityDialog outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void actionPerformed(ActionEvent @event)
				{
					outerInstance.processActionCommand(@event.ActionCommand);
					outerInstance.dispose();
				}
			}

			protected internal virtual void dispose()
			{
				dialog.dispose();
				Emulator.MainGUI.endWindowDialog();
			}

			protected internal virtual void processActionCommand(string actionCommand)
			{
				if (actionCommandYES.Equals(actionCommand))
				{
					buttonPressed = SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_YES;
				}
				else if (actionCommandNO.Equals(actionCommand))
				{
					buttonPressed = SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_NO;
				}
				else if (actionCommandOK.Equals(actionCommand))
				{
					buttonPressed = SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_OK;
				}
				else if (actionCommandESC.Equals(actionCommand))
				{
					buttonPressed = SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_ESC;
				}
				else
				{
					buttonPressed = SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_INVALID;
				}
			}

			protected internal virtual void endDialog()
			{
				Container contentPane = dialog.ContentPane;
				contentPane.Layout = new BoxLayout(contentPane, BoxLayout.Y_AXIS);
				contentPane.add(messagePane);
				contentPane.add(buttonPane);

				dialog.pack();

				Emulator.MainGUI.startWindowDialog(dialog);
			}

			protected internal virtual JButton DefaultButton
			{
				set
				{
					dialog.RootPane.DefaultButton = value;
				}
			}

			public override bool Visible
			{
				set
				{
					dialog.Visible = value;
				}
				get
				{
					return dialog.Visible;
				}
			}


			public override Point Location
			{
				get
				{
					return dialog.Location;
				}
			}

			public override Dimension Size
			{
				get
				{
					return dialog.Size;
				}
			}

			protected internal virtual bool isButtonPressed(int button)
			{
				Controller controller = State.controller;
				if ((controller.Buttons & button) == button)
				{
					return true;
				}

				return false;
			}

			protected internal virtual bool ConfirmButtonPressed
			{
				get
				{
					return isButtonPressed(SystemParamButtonPreference == PSP_SYSTEMPARAM_BUTTON_CIRCLE ? sceCtrl.PSP_CTRL_CIRCLE : sceCtrl.PSP_CTRL_CROSS);
				}
			}

			protected internal virtual bool CancelButtonPressed
			{
				get
				{
					return isButtonPressed(SystemParamButtonPreference == PSP_SYSTEMPARAM_BUTTON_CIRCLE ? sceCtrl.PSP_CTRL_CROSS : sceCtrl.PSP_CTRL_CIRCLE);
				}
			}

			internal virtual int ControllerLy
			{
				get
				{
					return State.controller.Ly & 0xFF;
				}
			}

			internal virtual int ControllerAnalogCenter
			{
				get
				{
					return Controller.analogCenter & 0xFF;
				}
			}

			internal virtual void checkRepeat()
			{
				if (pressedTimestamp != 0 && SystemTimeManager.SystemTime - pressedTimestamp > repeatDelay)
				{
					upPressedAnalog = false;
					upPressedButton = false;
					downPressedAnalog = false;
					downPressedButton = false;
					pressedTimestamp = 0;
				}
			}

			protected internal virtual bool UpPressed
			{
				get
				{
					checkRepeat();
					if (upPressedButton || upPressedAnalog)
					{
						if (!isButtonPressed(sceCtrl.PSP_CTRL_UP))
						{
							upPressedButton = false;
						}
    
						if (ControllerLy >= ControllerAnalogCenter)
						{
							upPressedAnalog = false;
						}
    
						return false;
					}
    
					if (isButtonPressed(sceCtrl.PSP_CTRL_UP))
					{
						upPressedButton = true;
						pressedTimestamp = SystemTimeManager.SystemTime;
						return true;
					}
    
					if (ControllerLy < ControllerAnalogCenter)
					{
						upPressedAnalog = true;
						pressedTimestamp = SystemTimeManager.SystemTime;
						return true;
					}
    
					return false;
				}
			}

			protected internal virtual bool DownPressed
			{
				get
				{
					checkRepeat();
					if (downPressedButton || downPressedAnalog)
					{
						if (!isButtonPressed(sceCtrl.PSP_CTRL_DOWN))
						{
							downPressedButton = false;
						}
    
						if (ControllerLy <= ControllerAnalogCenter)
						{
							downPressedAnalog = false;
						}
    
						return false;
					}
    
					if (isButtonPressed(sceCtrl.PSP_CTRL_DOWN))
					{
						downPressedButton = true;
						pressedTimestamp = SystemTimeManager.SystemTime;
						return true;
					}
    
					if (ControllerLy > ControllerAnalogCenter)
					{
						downPressedAnalog = true;
						pressedTimestamp = SystemTimeManager.SystemTime;
						return true;
					}
    
					return false;
				}
			}

			public virtual void checkController()
			{
				if (ConfirmButtonPressed)
				{
					processActionCommand(confirmButtonActionCommand);
					dispose();
				}
				else if (CancelButtonPressed)
				{
					processActionCommand(cancelButtonActionCommand);
					dispose();
				}
			}
		}

		protected internal abstract class GuUtilityDialog
		{

			protected internal long pressedTimestamp;
			protected internal const int repeatDelay = 100000;
			protected internal bool downPressedButton;
			protected internal bool downPressedAnalog;
			protected internal bool upPressedButton;
			protected internal bool upPressedAnalog;
			protected internal bool leftPressedButton;
			protected internal bool leftPressedAnalog;
			protected internal bool rightPressedButton;
			protected internal bool rightPressedAnalog;
			protected internal sceGu gu;
			protected internal UtilityDialogState utilityDialogState;
			internal int x;
			internal int y;
			internal int textX;
			internal int textY;
			internal int textWidth;
			internal int textHeight;
			internal int textLineHeight;
			internal int textAddr;
			internal SceFontInfo defaultFontInfo;
			protected internal const float defaultFontScale = 0.75f;
			protected internal const int baseAscender = 15;
			protected internal const int defaultTextWidth = 512;
			protected internal const int defaultTextHeight = 32;
			protected internal const int textColor = 0xFFFFFF;
			protected internal const int shadowColor = 0x000000;
			protected internal bool softShadows;
			protected internal long startDialogMillis;
			protected internal int drawSpeed;
			internal bool buttonsSwapped;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal bool hasNoButtons_Renamed;
			protected internal readonly Locale utilityLocale;
			internal readonly string strEnter;
			internal readonly string strBack;
			internal readonly string strYes;
			internal readonly string strNo;

			protected internal GuUtilityDialog(pspUtilityDialogCommon utilityDialogCommon)
			{
				utilityLocale = getUtilityLocale(utilityDialogCommon.language);
				buttonsSwapped = (utilityDialogCommon.buttonSwap == pspUtilityDialogCommon.BUTTON_ACCEPT_CIRCLE);
				hasNoButtons_Renamed = false;
				strEnter = ResourceBundle.getBundle("pspsharp/languages/pspsharp", utilityLocale).getString("sceUtilitySavedata.strEnter.text");
				strBack = ResourceBundle.getBundle("pspsharp/languages/pspsharp", utilityLocale).getString("sceUtilitySavedata.strBack.text");
				strYes = ResourceBundle.getBundle("pspsharp/languages/pspsharp", utilityLocale).getString("sceUtilitySavedata.strYes.text");
				strNo = ResourceBundle.getBundle("pspsharp/languages/pspsharp", utilityLocale).getString("sceUtilitySavedata.strNo.text");
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: protected void createDialog(final UtilityDialogState utilityDialogState)
			protected internal virtual void createDialog(UtilityDialogState utilityDialogState)
			{
				this.utilityDialogState = utilityDialogState;
				if (log.DebugEnabled)
				{
					int partitionId = SysMemUserForUser.USER_PARTITION_ID;
					log.debug(string.Format("Free memory total=0x{0:X}, max=0x{1:X}", Modules.SysMemUserForUserModule.totalFreeMemSize(partitionId), Modules.SysMemUserForUserModule.maxFreeMemSize(partitionId)));
				}

				startDialogMillis = Emulator.Clock.milliTime();

				// Allocate 1 MB
				gu = new sceGu(1 * 1024 * 1024);
			}

			protected internal virtual void dispose()
			{
				if (gu != null)
				{
					gu.free();
					gu = null;
				}
			}

			public virtual bool Visible
			{
				get
				{
					return gu != null;
				}
			}

			protected internal virtual void update(int drawSpeed)
			{
				this.drawSpeed = drawSpeed;

				// Do not overwrite a sceGu list still in drawing state
				if (gu != null && !gu.ListDrawing)
				{
					gu.sceGuStart();

					// Disable all common flags
					gu.sceGuDisable(pspsharp.graphics.RE.IRenderingEngine_Fields.GU_DEPTH_TEST);
					gu.sceGuDisable(pspsharp.graphics.RE.IRenderingEngine_Fields.GU_ALPHA_TEST);
					gu.sceGuDisable(pspsharp.graphics.RE.IRenderingEngine_Fields.GU_FOG);
					gu.sceGuDisable(pspsharp.graphics.RE.IRenderingEngine_Fields.GU_LIGHTING);
					gu.sceGuDisable(pspsharp.graphics.RE.IRenderingEngine_Fields.GU_COLOR_LOGIC_OP);
					gu.sceGuDisable(pspsharp.graphics.RE.IRenderingEngine_Fields.GU_STENCIL_TEST);
					gu.sceGuDisable(pspsharp.graphics.RE.IRenderingEngine_Fields.GU_CULL_FACE);
					gu.sceGuDisable(pspsharp.graphics.RE.IRenderingEngine_Fields.GU_SCISSOR_TEST);

					// Enable standard alpha blending
					gu.sceGuBlendFunc(ALPHA_SOURCE_BLEND_OPERATION_ADD, ALPHA_SOURCE_ALPHA, ALPHA_ONE_MINUS_SOURCE_ALPHA, 0, 0);
					gu.sceGuEnable(pspsharp.graphics.RE.IRenderingEngine_Fields.GU_BLEND);

					updateDialog();

					gu.sceGuFinish();
				}

				checkController();
			}

			protected internal virtual string truncateText(string text, int width, float scale)
			{
				string truncatedText = text;
				string truncation = "";
				while (true)
				{
					int textLength = getTextLength(DefaultFontInfo, truncatedText + truncation, scale);
					if (textLength <= width)
					{
						break;
					}
					truncatedText = truncatedText.Substring(0, truncatedText.Length - 1);
					truncation = "...";
				}

				return truncatedText + truncation;
			}

			internal virtual string wrapText(string text, int width, float scale)
			{
				SceFontInfo fontInfo = DefaultFontInfo;
				int glyphType = SceFontInfo.FONT_PGF_GLYPH_TYPE_CHAR;

				StringBuilder wrappedText = new StringBuilder();
				int x = 0;
				int lastSpaceStart = -1;
				int lastSpaceEnd = -1;
				bool isSpace = false;
				int scaledWidth = (int)(width / scale);
				for (int i = 0; i < text.Length; i++)
				{
					char c = text[i];
					pspCharInfo charInfo = fontInfo.getCharInfo(c, glyphType);

					if (c == ' ')
					{
						if (!isSpace)
						{
							lastSpaceStart = i;
							isSpace = true;
						}
						lastSpaceEnd = i + 1;
					}
					else if (isSpace)
					{
						lastSpaceEnd = i;
						isSpace = false;
					}

					int nextX = x + charInfo.sfp26AdvanceH >> 6;
					if (nextX > scaledWidth)
					{
						x = 0;
						if (lastSpaceStart >= 0)
						{
							wrappedText.Remove(lastSpaceStart, lastSpaceEnd - lastSpaceStart).Insert(lastSpaceStart, "\n");
						}
						else
						{
							wrappedText.Append('\n');
						}

						if (!isSpace)
						{
							wrappedText.Append(c);
						}
					}
					else
					{
						x = nextX;
						wrappedText.Append(c);
					}
				}

				return wrappedText.ToString();
			}

			internal virtual void drawText(SceFontInfo fontInfo, int baseAscender, char c, int glyphType)
			{
				pspCharInfo charInfo = fontInfo.getCharInfo(c, glyphType);
				if (log.TraceEnabled)
				{
					log.trace(string.Format("drawText '{0}'({1:D}), glyphType=0x{2:X}, baseAscender={3:D}, position ({4:D},{5:D}), {6}", c, (int) c, glyphType, baseAscender, x, y, charInfo));
				}

				if (charInfo == null)
				{
					return;
				}

				if (c == '\n')
				{
					x = textX;
					y += textLineHeight;
					return;
				}

				fontInfo.printFont(textAddr, textWidth / 2, textWidth, textHeight, x - textX + charInfo.bitmapLeft, y - textY + baseAscender - charInfo.bitmapTop, 0, 0, 0, 0, textWidth, textHeight, PSP_FONT_PIXELFORMAT_4, c, ' ', glyphType, true);

				if (glyphType != SceFontInfo.FONT_PGF_GLYPH_TYPE_CHAR)
				{
					// Take the advanceH from the character, not from the shadow
					charInfo = fontInfo.getCharInfo(c, SceFontInfo.FONT_PGF_GLYPH_TYPE_CHAR);
				}
				x += charInfo.sfp26AdvanceH >> 6;
			}

			protected internal virtual int getTextLines(string s)
			{
				int lines = 1;
				int index = 0;
				while (index < s.Length)
				{
					int newLine = s.IndexOf("\n", index, StringComparison.Ordinal);
					if (newLine < 0)
					{
						break;
					}
					lines++;
					index = newLine + 1;
				}

				return lines;
			}

			protected internal virtual int centerText(SceFontInfo fontInfo, string s, int x0, int x1, float scale)
			{
				int textLength = getTextLength(fontInfo, s, scale);
				int width = x1 - x0 + 1;
				if (textLength >= width)
				{
					return x0;
				}
				return x0 + (width - textLength) / 2;
			}

			protected internal virtual int getTextLength(SceFontInfo fontInfo, string s, float scale)
			{
				int textLength = getTextLength(fontInfo, s);
				textLength = (int)(scale * textLength);
				return textLength;
			}

			protected internal virtual int getTextLength(SceFontInfo fontInfo, string s)
			{
				int length = 0;

				for (int i = 0; i < s.Length; i++)
				{
					length += getTextLength(fontInfo, s[i]);
				}

				return length;
			}

			protected internal virtual int getTextLength(SceFontInfo fontInfo, char c)
			{
				pspCharInfo charInfo = fontInfo.getCharInfo(c, SceFontInfo.FONT_PGF_GLYPH_TYPE_CHAR);
				if (charInfo == null)
				{
					return 0;
				}
				return charInfo.sfp26AdvanceH >> 6;
			}

			protected internal virtual void drawTextWithShadow(int textX, int textY, float scale, string s)
			{
				drawTextWithShadow(textX, textY, textColor, scale, s);
			}

			protected internal virtual void drawTextWithShadow(int textX, int textY, int textColor, float scale, string s)
			{
				s = wrapText(s, Screen.width - textX, scale);

				int txtHeight = defaultTextHeight;
				if (s.Contains("\n"))
				{
					txtHeight = Screen.height - textY;
				}
				drawTextWithShadow(textX, textY, defaultTextWidth, txtHeight, 20, DefaultFontInfo, baseAscender, textColor, shadowColor, scale, s);
			}

			protected internal virtual void drawTextWithShadow(int textX, int textY, int textWidth, int textHeight, int textLineHeight, SceFontInfo fontInfo, int baseAscender, int textColor, int shadowColor, float scale, string s)
			{
				drawText(textX, textY, textWidth, textHeight, textLineHeight, fontInfo, baseAscender, scale, shadowColor, s, SceFontInfo.FONT_PGF_GLYPH_TYPE_SHADOW);
				drawText(textX, textY, textWidth, textHeight, textLineHeight, fontInfo, baseAscender, scale, textColor, s, SceFontInfo.FONT_PGF_GLYPH_TYPE_CHAR);
			}

			protected internal virtual bool SoftShadows
			{
				set
				{
					this.softShadows = value;
				}
			}

			protected internal virtual void drawText(int textX, int textY, int textWidth, int textHeight, int textLineHeight, SceFontInfo fontInfo, int baseAscender, float scale, int color, string s, int glyphType)
			{
				this.textX = textX;
				this.textY = textY;
				this.textWidth = textWidth;
				this.textHeight = textHeight;
				this.textLineHeight = textLineHeight;
				x = textX;
				y = textY;

				int textSize = textWidth * textHeight / 2;
				textAddr = gu.sceGuGetMemory(textSize);
				if (textAddr == 0)
				{
					return;
				}

				// Clear the texture for the text
				Memory.Instance.memset(textAddr, (sbyte) 0, textSize);

				for (int i = 0; i < s.Length; i++)
				{
					drawText(fontInfo, baseAscender, s[i], glyphType);
				}

				const int numberOfVertex = 2;
				int textVertexAddr = gu.sceGuGetMemory(10 * numberOfVertex);
				IMemoryWriter vertexWriter = MemoryWriter.getMemoryWriter(textVertexAddr, 2);
				// Texture (0,0)
				vertexWriter.writeNext(0);
				vertexWriter.writeNext(0);
				// Position
				vertexWriter.writeNext(textX);
				vertexWriter.writeNext(textY);
				vertexWriter.writeNext(0);
				// Texture (textWidth,textHeigt)
				vertexWriter.writeNext(textWidth);
				vertexWriter.writeNext(textHeight);
				// Position
				vertexWriter.writeNext(textX + (int)(textWidth * scale));
				vertexWriter.writeNext(textY + (int)(textHeight * scale));
				vertexWriter.writeNext(0);
				vertexWriter.flush();

				int clutAddr = gu.sceGuGetMemory(16 * 4);
				IMemoryWriter clutWriter = MemoryWriter.getMemoryWriter(clutAddr, 4);
				color &= 0x00FFFFFF;
				for (int i = 0; i < 16; i++)
				{
					int alpha = (i << 4) | i;

					// Reduce alpha by factor 2 if soft shadows are required (MsgDialog)
					if (softShadows && glyphType == SceFontInfo.FONT_PGF_GLYPH_TYPE_SHADOW)
					{
						alpha >>= 1;
					}

					clutWriter.writeNext((alpha << 24) | color);
				}
				gu.sceGuClutMode(CMODE_FORMAT_32BIT_ABGR8888, 0, 0xFF, 0);
				gu.sceGuClutLoad(2, clutAddr);

				gu.sceGuTexMode(TPSM_PIXEL_STORAGE_MODE_4BIT_INDEXED, 0, false);
				gu.sceGuTexFunc(TFUNC_FRAGMENT_DOUBLE_TEXTURE_EFECT_REPLACE, true, false);
				gu.sceGuTexEnvColor(0x000000);
				gu.sceGuTexWrap(TWRAP_WRAP_MODE_CLAMP, TWRAP_WRAP_MODE_CLAMP);
				gu.sceGuTexFilter(TFLT_LINEAR, TFLT_LINEAR);
				gu.sceGuEnable(GU_TEXTURE_2D);
				gu.sceGuTexImage(0, 512, 256, textWidth, textAddr);
				gu.sceGuDrawArray(PRIM_SPRITES, (VTYPE_TRANSFORM_PIPELINE_RAW_COORD << 23) | (VTYPE_TEXTURE_FORMAT_16_BIT) | (VTYPE_POSITION_FORMAT_16_BIT << 7), numberOfVertex, 0, textVertexAddr);
			}

			protected internal virtual void drawButton(int x, int y, string text, bool selected)
			{
				if (selected)
				{
					int alpha = getAnimationIndex(0xFF);
					gu.sceGuDrawRectangle(x, y, x + text.Length * 17, y + 16, (alpha << 24) | 0xC5C8CF);
				}
				drawTextWithShadow(x + 5, y + 2, 0.8f, text);
			}

			protected internal abstract void updateDialog();

			public virtual void checkController()
			{
				// In case the dialog has no buttons, assume the user is confirming.
				if (canConfirm() && (ConfirmButtonPressed || hasNoButtons()))
				{
					utilityDialogState.ButtonPressed = ButtonPressedOK;
					dispose();
				}
				else if (canCancel() && CancelButtonPressed)
				{
					utilityDialogState.ButtonPressed = SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_ESC;
					dispose();
				}

				if (hasYesNo())
				{
					if (LeftPressed)
					{
						utilityDialogState.YesSelected = true;
					}
					else if (RightPressed)
					{
						utilityDialogState.YesSelected = false;
					}
				}
			}

			protected internal virtual bool isButtonPressed(int button)
			{
				Controller controller = State.controller;
				if ((controller.Buttons & button) == button)
				{
					return true;
				}

				return false;
			}

			protected internal virtual bool ConfirmButtonPressed
			{
				get
				{
					return isButtonPressed(areButtonsSwapped() ? sceCtrl.PSP_CTRL_CIRCLE : sceCtrl.PSP_CTRL_CROSS);
				}
			}

			protected internal virtual bool CancelButtonPressed
			{
				get
				{
					return isButtonPressed(areButtonsSwapped() ? sceCtrl.PSP_CTRL_CROSS : sceCtrl.PSP_CTRL_CIRCLE);
				}
			}

			protected internal virtual void useNoButtons()
			{
				hasNoButtons_Renamed = true;
			}

			protected internal virtual bool hasNoButtons()
			{
				return hasNoButtons_Renamed;
			}

			internal virtual int ControllerLy
			{
				get
				{
					return State.controller.Ly & 0xFF;
				}
			}

			internal virtual int ControllerLx
			{
				get
				{
					return State.controller.Lx & 0xFF;
				}
			}

			internal virtual int ControllerAnalogCenter
			{
				get
				{
					return Controller.analogCenter & 0xFF;
				}
			}

			protected internal virtual bool canConfirm()
			{
				return true;
			}

			protected internal virtual bool canCancel()
			{
				return true;
			}

			protected internal virtual bool hasYesNo()
			{
				return false;
			}

			protected internal virtual int ButtonPressedOK
			{
				get
				{
					return SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_OK;
				}
			}

			internal virtual void checkRepeat()
			{
				if (pressedTimestamp != 0 && SystemTimeManager.SystemTime - pressedTimestamp > repeatDelay)
				{
					upPressedAnalog = false;
					upPressedButton = false;
					downPressedAnalog = false;
					downPressedButton = false;
					leftPressedAnalog = false;
					leftPressedButton = false;
					rightPressedAnalog = false;
					rightPressedButton = false;
					pressedTimestamp = 0;
				}
			}

			protected internal virtual bool UpPressed
			{
				get
				{
					checkRepeat();
					if (upPressedButton || upPressedAnalog)
					{
						if (!isButtonPressed(sceCtrl.PSP_CTRL_UP))
						{
							upPressedButton = false;
						}
    
						if (ControllerLy >= ControllerAnalogCenter)
						{
							upPressedAnalog = false;
						}
    
						return false;
					}
    
					if (isButtonPressed(sceCtrl.PSP_CTRL_UP))
					{
						upPressedButton = true;
						pressedTimestamp = SystemTimeManager.SystemTime;
						return true;
					}
    
					if (ControllerLy < ControllerAnalogCenter)
					{
						upPressedAnalog = true;
						pressedTimestamp = SystemTimeManager.SystemTime;
						return true;
					}
    
					return false;
				}
			}

			protected internal virtual bool DownPressed
			{
				get
				{
					checkRepeat();
					if (downPressedButton || downPressedAnalog)
					{
						if (!isButtonPressed(sceCtrl.PSP_CTRL_DOWN))
						{
							downPressedButton = false;
						}
    
						if (ControllerLy <= ControllerAnalogCenter)
						{
							downPressedAnalog = false;
						}
    
						return false;
					}
    
					if (isButtonPressed(sceCtrl.PSP_CTRL_DOWN))
					{
						downPressedButton = true;
						pressedTimestamp = SystemTimeManager.SystemTime;
						return true;
					}
    
					if (ControllerLy > ControllerAnalogCenter)
					{
						downPressedAnalog = true;
						pressedTimestamp = SystemTimeManager.SystemTime;
						return true;
					}
    
					return false;
				}
			}

			protected internal virtual bool LeftPressed
			{
				get
				{
					checkRepeat();
					if (leftPressedButton || leftPressedAnalog)
					{
						if (!isButtonPressed(sceCtrl.PSP_CTRL_LEFT))
						{
							leftPressedButton = false;
						}
    
						if (ControllerLx >= ControllerAnalogCenter)
						{
							leftPressedAnalog = false;
						}
    
						return false;
					}
    
					if (isButtonPressed(sceCtrl.PSP_CTRL_LEFT))
					{
						leftPressedButton = true;
						pressedTimestamp = SystemTimeManager.SystemTime;
						return true;
					}
    
					if (ControllerLx < ControllerAnalogCenter)
					{
						leftPressedAnalog = true;
						pressedTimestamp = SystemTimeManager.SystemTime;
						return true;
					}
    
					return false;
				}
			}

			protected internal virtual bool RightPressed
			{
				get
				{
					checkRepeat();
					if (rightPressedButton || rightPressedAnalog)
					{
						if (!isButtonPressed(sceCtrl.PSP_CTRL_RIGHT))
						{
							rightPressedButton = false;
						}
    
						if (ControllerLx <= ControllerAnalogCenter)
						{
							rightPressedAnalog = false;
						}
    
						return false;
					}
    
					if (isButtonPressed(sceCtrl.PSP_CTRL_RIGHT))
					{
						rightPressedButton = true;
						pressedTimestamp = SystemTimeManager.SystemTime;
						return true;
					}
    
					if (ControllerLx > ControllerAnalogCenter)
					{
						rightPressedAnalog = true;
						pressedTimestamp = SystemTimeManager.SystemTime;
						return true;
					}
    
					return false;
				}
			}

			protected internal virtual SceFontInfo DefaultFontInfo
			{
				get
				{
					if (defaultFontInfo == null)
					{
						// Use "jpn0" font
						defaultFontInfo = Modules.sceFontModule.getFont(0).fontInfo;
					}
					return defaultFontInfo;
				}
			}

			internal virtual string Cross
			{
				get
				{
					return "X";
				}
			}

			internal virtual string Circle
			{
				get
				{
					return "O";
				}
			}

			protected internal virtual bool areButtonsSwapped()
			{
				return buttonsSwapped;
			}

			// Cross is always displayed to the left (even when the buttons are swapped)
			internal virtual int CrossX
			{
				get
				{
					return 183;
				}
			}

			// Circle is always displayed to the right (even when the buttons are swapped)
			internal virtual int CircleX
			{
				get
				{
					return 260;
				}
			}

			internal virtual int EnterX
			{
				get
				{
					return areButtonsSwapped() ? CircleX : CrossX;
				}
			}

			internal virtual int BackX
			{
				get
				{
					return areButtonsSwapped() ? CrossX : CircleX;
				}
			}

			internal virtual string ConfirmString
			{
				get
				{
					return areButtonsSwapped() ? Circle : Cross;
				}
			}

			internal virtual string CancelString
			{
				get
				{
					return areButtonsSwapped() ? Cross : Circle;
				}
			}

			protected internal virtual void drawEnter()
			{
				drawTextWithShadow(EnterX, 254, defaultFontScale, string.Format("{0} {1}", ConfirmString, strEnter));
			}

			protected internal virtual void drawBack()
			{
				drawTextWithShadow(BackX, 254, defaultFontScale, string.Format("{0} {1}", CancelString, strBack));
			}

			protected internal virtual void drawEnterWithString(string str)
			{
				drawTextWithShadow(EnterX, 254, defaultFontScale, string.Format("{0} {1}", ConfirmString, str));
			}

			protected internal virtual void drawBackWithString(string str)
			{
				drawTextWithShadow(BackX, 254, defaultFontScale, string.Format("{0} {1}", CancelString, str));
			}

			protected internal virtual void drawHeader(string title)
			{
				// Draw rectangle on the top of the screen
				gu.sceGuDrawRectangle(0, 0, Screen.width, 22, unchecked((int)0x80605C54));

				// Draw dialog title in top rectangle
				drawText(30, 4, 128, 32, 20, DefaultFontInfo, 15, 0.82f, 0xFFFFFF, title, SceFontInfo.FONT_PGF_GLYPH_TYPE_CHAR);
				// Draw filled circle just before dialog title
				drawText(9, 5, 32, 32, 20, DefaultFontInfo, 15, 0.65f, 0xFFFFFF, new string(Character.toChars(0x25CF)), SceFontInfo.FONT_PGF_GLYPH_TYPE_CHAR);
			}

			protected internal virtual void drawYesNo(int xYes, int xNo, int y)
			{
				drawButton(xYes, y, strYes, utilityDialogState.YesSelected);
				drawButton(xNo, y, strNo, utilityDialogState.NoSelected);
			}

			protected internal virtual void drawIcon(int textureAddr, int iconX, int iconY, int iconWidth, int iconHeight)
			{
				if (textureAddr == 0)
				{
					return;
				}

				int numberOfVertex = 2;
				int iconVertexAddr = gu.sceGuGetMemory(10 * numberOfVertex);
				if (iconVertexAddr == 0)
				{
					return;
				}
				IMemoryWriter vertexWriter = MemoryWriter.getMemoryWriter(iconVertexAddr, 2);
				// Texture
				vertexWriter.writeNext(0);
				vertexWriter.writeNext(0);
				// Position
				vertexWriter.writeNext(iconX);
				vertexWriter.writeNext(iconY);
				vertexWriter.writeNext(0);
				// Texture
				vertexWriter.writeNext(icon0Width);
				vertexWriter.writeNext(icon0Height);
				// Position
				vertexWriter.writeNext(iconX + iconWidth);
				vertexWriter.writeNext(iconY + iconHeight);
				vertexWriter.writeNext(0);
				vertexWriter.flush();

				gu.sceGuTexEnvColor(0x000000);
				gu.sceGuTexMode(icon0PixelFormat, 0, false);
				gu.sceGuTexImage(0, 256, 128, icon0BufferWidth, textureAddr);
				gu.sceGuTexFunc(TFUNC_FRAGMENT_DOUBLE_TEXTURE_EFECT_REPLACE, true, false);
				gu.sceGuTexFilter(TFLT_LINEAR, TFLT_LINEAR);
				gu.sceGuTexWrap(TWRAP_WRAP_MODE_CLAMP, TWRAP_WRAP_MODE_CLAMP);
				gu.sceGuEnable(GU_TEXTURE_2D);
				gu.sceGuDrawArray(PRIM_SPRITES, (VTYPE_TRANSFORM_PIPELINE_RAW_COORD << 23) | (VTYPE_TEXTURE_FORMAT_16_BIT) | (VTYPE_POSITION_FORMAT_16_BIT << 7), numberOfVertex, 0, iconVertexAddr);
			}

			protected internal virtual int readIcon(System.IO.Stream @is)
			{
				BufferedImage image = null;

				// Get icon image
				if (@is != null)
				{
					try
					{
						image = ImageIO.read(@is);
					}
					catch (IOException e)
					{
						log.debug("getIcon0", e);
					}
					catch (Exception)
					{
						// Corrupted data, just ignore.
					}
				}

				// Default icon
				if (image == null)
				{
					try
					{
						image = ImageIO.read(this.GetType().getResource("/pspsharp/images/icon0.png"));
					}
					catch (IOException e)
					{
						log.error("Cannot read default icon0.png", e);
					}
				}

				if (image == null)
				{
					return 0;
				}

				int bytesPerPixel = pspsharp.graphics.RE.IRenderingEngine_Fields.sizeOfTextureType[icon0PixelFormat];
				int textureAddr = gu.sceGuGetMemory(icon0BufferWidth * icon0Height * bytesPerPixel);
				if (textureAddr == 0)
				{
					return 0;
				}

				IMemoryWriter textureWriter = MemoryWriter.getMemoryWriter(textureAddr, bytesPerPixel);
				int width = System.Math.Min(image.Width, icon0Width);
				int height = System.Math.Min(image.Height, icon0Height);
				for (int hy = 0; hy < height; hy++)
				{
					for (int wx = 0; wx < width; wx++)
					{
						int colorARGB = image.getRGB(wx, hy);
						int colorABGR = colorARGBtoABGR(colorARGB);
						textureWriter.writeNext(colorABGR);
					}
					for (int wx = width; wx < icon0BufferWidth; wx++)
					{
						textureWriter.writeNext(0);
					}
				}
				textureWriter.flush();

				return textureAddr;
			}

			protected internal virtual int readIcon(int address)
			{
				System.IO.Stream iconStream = null;

				if (address != 0)
				{
					iconStream = new MemoryInputStream(address);
				}

				return readIcon(iconStream);
			}

			protected internal virtual int getAnimationIndex(int maxIndex)
			{
				if (drawSpeed <= 0)
				{
					return maxIndex;
				}

				long now = Emulator.Clock.currentTimeMillis();
				int durationMillis = (int)(now - startDialogMillis);

				int animationIndex = durationMillis % 500 * (maxIndex + 1) / 500;
				if (((durationMillis / 500) % 2) != 0)
				{
					// Revert the animation index every 0.5 second
					animationIndex = maxIndex - animationIndex;
				}

				return animationIndex;
			}
		}

		protected internal class GuSavedataDialogSave : GuUtilityDialog
		{

			protected internal readonly SavedataUtilityDialogState savedataDialogState;
			protected internal readonly SceUtilitySavedataParam savedataParams;
			protected internal bool isYesSelected;
			internal readonly string strAskSaveData;
			internal readonly string strAskOverwriteData;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: protected GuSavedataDialogSave(final pspsharp.HLE.kernel.types.SceUtilitySavedataParam savedataParams, final SavedataUtilityDialogState savedataDialogState)
			protected internal GuSavedataDialogSave(SceUtilitySavedataParam savedataParams, SavedataUtilityDialogState savedataDialogState) : base(savedataParams.@base)
			{
				this.savedataDialogState = savedataDialogState;
				this.savedataParams = savedataParams;

				strAskSaveData = ResourceBundle.getBundle("pspsharp/languages/pspsharp", utilityLocale).getString("sceUtilitySavedata.strAskSaveData.text");
				strAskOverwriteData = ResourceBundle.getBundle("pspsharp/languages/pspsharp", utilityLocale).getString("sceUtilitySavedata.strAskOverwriteData.text");

				createDialog(savedataDialogState);
			}

			protected internal override void updateDialog()
			{
				string dialogTitle = savedataDialogState.getDialogTitle(savedataParams.ModeName, "Save", utilityLocale);
				DateTime savedTime = savedataParams.SavedTime;

				drawIcon(readIcon(savedataParams.icon0FileData.buf), 26, 96, icon0Width, icon0Height);

				string text = getText(dialogTitle);
				bool multiLines = getTextLines(text) > 1;
				if (hasYesNo())
				{
					int textX = multiLines ? 236 : centerText(DefaultFontInfo, text, 201, 464, defaultFontScale);
					gu.sceGuDrawHorizontalLine(201, 464, multiLines ? 87 : 97, unchecked((int)0xFF000000) | textColor);
					drawTextWithShadow(textX, multiLines ? 105 : 113, defaultFontScale, text);
					drawYesNo(278, 349, multiLines ? 154 : 145);
					gu.sceGuDrawHorizontalLine(201, 464, multiLines ? 184 : 174, unchecked((int)0xFF000000) | textColor);
				}
				else
				{
					int textX = multiLines ? 270 : centerText(DefaultFontInfo, text, 201, 464, defaultFontScale);
					gu.sceGuDrawHorizontalLine(201, 464, 114, unchecked((int)0xFF000000) | textColor);
					drawTextWithShadow(textX, 131, defaultFontScale, text);
					gu.sceGuDrawHorizontalLine(201, 464, 157, unchecked((int)0xFF000000) | textColor);
				}

				drawTextWithShadow(6, 202, defaultFontScale, savedataParams.sfoParam.savedataTitle);
				if (savedTime != null)
				{
					drawTextWithShadow(6, 219, 0.7f, formatDateTime(savedTime));
				}
				drawTextWithShadow(6, 237, defaultFontScale, MemoryStick.getSizeKbString(savedataParams.RequiredSizeKb));

				if (hasEnter())
				{
					drawEnter();
				}
				drawBack();

				drawHeader(dialogTitle);
			}

			protected internal virtual string getText(string dialogTitle)
			{
				return (savedataParams.Present) ? strAskOverwriteData : strAskSaveData;
			}

			protected internal virtual bool hasEnter()
			{
				return true;
			}

			protected internal override bool hasYesNo()
			{
				return true;
			}
		}

		protected internal class GuSavedataDialogLoad : GuUtilityDialog
		{

			protected internal readonly SavedataUtilityDialogState savedataDialogState;
			protected internal readonly SceUtilitySavedataParam savedataParams;
			protected internal bool isYesSelected;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			protected internal bool hasYesNo_Renamed;
			internal readonly string strNoData;
			internal readonly string strAskLoadData;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: protected GuSavedataDialogLoad(final pspsharp.HLE.kernel.types.SceUtilitySavedataParam savedataParams, final SavedataUtilityDialogState savedataDialogState)
			protected internal GuSavedataDialogLoad(SceUtilitySavedataParam savedataParams, SavedataUtilityDialogState savedataDialogState) : base(savedataParams.@base)
			{
				this.savedataDialogState = savedataDialogState;
				this.savedataParams = savedataParams;

				hasYesNo_Renamed = savedataParams.Present;

				strNoData = ResourceBundle.getBundle("pspsharp/languages/pspsharp", utilityLocale).getString("sceUtilitySavedata.strNoData.text");
				strAskLoadData = ResourceBundle.getBundle("pspsharp/languages/pspsharp", utilityLocale).getString("sceUtilitySavedata.strAskLoadData.text");

				createDialog(savedataDialogState);
			}

			protected internal override void updateDialog()
			{
				string dialogTitle = savedataDialogState.getDialogTitle(savedataParams.ModeName, "Load", utilityLocale);
				DateTime savedTime = savedataParams.SavedTime;

				drawIcon(readIcon(savedataParams.icon0FileData.buf), 26, 96, icon0Width, icon0Height);

				if (!hasYesNo())
				{
					gu.sceGuDrawHorizontalLine(201, 464, 114, unchecked((int)0xFF000000) | textColor);
					drawTextWithShadow(270, 131, defaultFontScale, strNoData);
					gu.sceGuDrawHorizontalLine(201, 464, 157, unchecked((int)0xFF000000) | textColor);
				}
				else
				{
					gu.sceGuDrawHorizontalLine(201, 464, 87, unchecked((int)0xFF000000) | textColor);
					drawTextWithShadow(236, 105, defaultFontScale, strAskLoadData);
					drawYesNo(278, 349, 154);
					gu.sceGuDrawHorizontalLine(201, 464, 184, unchecked((int)0xFF000000) | textColor);

					drawTextWithShadow(6, 202, defaultFontScale, savedataParams.sfoParam.savedataTitle);
					if (savedTime != null)
					{
						drawTextWithShadow(6, 219, 0.7f, formatDateTime(savedTime));
					}
					drawTextWithShadow(6, 237, defaultFontScale, MemoryStick.getSizeKbString(savedataParams.RequiredSizeKb));

					drawEnter();
				}
				drawBack();

				drawHeader(dialogTitle);
			}

			protected internal override bool hasYesNo()
			{
				return hasYesNo_Renamed;
			}

			protected internal override bool canConfirm()
			{
				return hasYesNo();
			}
		}

		protected internal class GuSavedataDialogCompleted : GuUtilityDialog
		{

			protected internal readonly SavedataUtilityDialogState savedataDialogState;
			protected internal readonly SceUtilitySavedataParam savedataParams;
			internal readonly System.IO.Stream icon0;
			protected internal bool isYesSelected;
			internal string strCompleted;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: protected GuSavedataDialogCompleted(final pspsharp.HLE.kernel.types.SceUtilitySavedataParam savedataParams, final SavedataUtilityDialogState savedataDialogState, final java.io.InputStream icon0)
			protected internal GuSavedataDialogCompleted(SceUtilitySavedataParam savedataParams, SavedataUtilityDialogState savedataDialogState, System.IO.Stream icon0) : base(savedataParams.@base)
			{
				this.savedataDialogState = savedataDialogState;
				this.savedataParams = savedataParams;
				this.icon0 = icon0;
				if (icon0 != null)
				{
					icon0.mark(int.MaxValue);
				}
				try
				{
					strCompleted = ResourceBundle.getBundle("pspsharp/languages/pspsharp", utilityLocale).getString(string.Format("sceUtilitySavedata.{0}.strCompleted.text", savedataParams.ModeName));
				}
				catch (MissingResourceException)
				{
					// Ignore exception and provide a default
					strCompleted = "Completed";
				}

				createDialog(savedataDialogState);
			}

			protected internal override void updateDialog()
			{
				string dialogTitle = savedataDialogState.getDialogTitle(savedataParams.ModeName, "Save", utilityLocale);
				DateTime savedTime = savedataParams.SavedTime;

				int textureAddr;
				// Take the icon0 from the selection in case the icon0 is not saved into the savedataParams
				if (savedataParams.icon0FileData.buf == 0 && icon0 != null)
				{
					try
					{
						icon0.reset();
					}
					catch (IOException)
					{
						// Ignore exception
					}

					textureAddr = readIcon(icon0);
				}
				else
				{
					textureAddr = readIcon(savedataParams.icon0FileData.buf);
				}
				drawIcon(textureAddr, 26, 96, icon0Width, icon0Height);

				gu.sceGuDrawHorizontalLine(201, 464, 114, unchecked((int)0xFF000000) | textColor);
				drawTextWithShadow(270, 131, defaultFontScale, strCompleted);
				gu.sceGuDrawHorizontalLine(201, 464, 157, unchecked((int)0xFF000000) | textColor);

				drawTextWithShadow(6, 202, defaultFontScale, savedataParams.sfoParam.savedataTitle);
				if (savedTime != null)
				{
					drawTextWithShadow(6, 219, 0.7f, formatDateTime(savedTime));
				}
				drawTextWithShadow(6, 237, defaultFontScale, MemoryStick.getSizeKbString(savedataParams.RequiredSizeKb));

				drawBack();

				drawHeader(dialogTitle);
			}

			protected internal override bool canConfirm()
			{
				return false;
			}
		}

		protected internal class GuSavedataDialog : GuUtilityDialog
		{

			internal readonly SavedataUtilityDialogState savedataDialogState;
			internal readonly SceUtilitySavedataParam savedataParams;
			internal readonly string[] saveNames;
			internal readonly int numberRows;
			internal int selectedRow;
			internal string strNewData;
			internal readonly string strNoData;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public GuSavedataDialog(final pspsharp.HLE.kernel.types.SceUtilitySavedataParam savedataParams, final SavedataUtilityDialogState savedataDialogState, final String[] saveNames)
			public GuSavedataDialog(SceUtilitySavedataParam savedataParams, SavedataUtilityDialogState savedataDialogState, string[] saveNames) : base(savedataParams.@base)
			{
				this.savedataDialogState = savedataDialogState;
				this.savedataParams = savedataParams;
				this.saveNames = saveNames;

				strNoData = ResourceBundle.getBundle("pspsharp/languages/pspsharp", utilityLocale).getString("sceUtilitySavedata.strNoData.text");

				if (savedataParams.newData != null && !string.ReferenceEquals(savedataParams.newData.title, null))
				{
					// the PspUtilitySavedataListSaveNewData structure contains the title
					// to be used for new data.
					strNewData = savedataParams.newData.title;
				}
				else
				{
					strNewData = ResourceBundle.getBundle("pspsharp/languages/pspsharp", utilityLocale).getString("sceUtilitySavedata.strNewData.text");
				}

				createDialog(savedataDialogState);

				numberRows = saveNames == null ? 0 : saveNames.Length;
				savedataDialogState.saveListEmpty = (numberRows <= 0);

				// Define the selected row according to the focus field
				selectedRow = 0;
				switch (savedataParams.focus)
				{
					case SceUtilitySavedataParam.FOCUS_FIRSTLIST:
					{
						selectedRow = 0;
						break;
					}
					case SceUtilitySavedataParam.FOCUS_LASTLIST:
					{
						selectedRow = numberRows - 1;
						break;
					}
					case SceUtilitySavedataParam.FOCUS_LATEST:
					{
						long latestTimestamp = long.MinValue;
						for (int i = 0; i < numberRows; i++)
						{
							long timestamp = getTimestamp(saveNames[i]);
							if (timestamp > latestTimestamp)
							{
								latestTimestamp = timestamp;
								selectedRow = i;
							}
						}
						break;
					}
					case SceUtilitySavedataParam.FOCUS_OLDEST:
					{
						long oldestTimestamp = long.MaxValue;
						for (int i = 0; i < numberRows; i++)
						{
							long timestamp = getTimestamp(saveNames[i]);
							if (timestamp < oldestTimestamp)
							{
								oldestTimestamp = timestamp;
								selectedRow = i;
							}
						}
						break;
					}
					case SceUtilitySavedataParam.FOCUS_FIRSTEMPTY:
					{
						for (int i = 0; i < numberRows; i++)
						{
							if (isEmpty(saveNames[i]))
							{
								selectedRow = i;
								break;
							}
						}
						break;
					}
					case SceUtilitySavedataParam.FOCUS_LASTEMPTY:
					{
						for (int i = numberRows - 1; i >= 0; i--)
						{
							if (isEmpty(saveNames[i]))
							{
								selectedRow = i;
								break;
							}
						}
						break;
					}
				}
			}

			internal virtual bool isEmpty(string saveName)
			{
				return !savedataParams.isPresent(savedataParams.gameName, saveName);
			}

			internal virtual long getTimestamp(string saveName)
			{
				return savedataParams.getTimestamp(savedataParams.gameName, saveName);
			}

			internal virtual System.IO.Stream getIcon0InputStream(int index)
			{
				System.IO.Stream iconStream = null;

				if (index < 0 || index >= saveNames.Length)
				{
					return iconStream;
				}

				// Get icon0 file
				string iconFileName = savedataParams.getFileName(saveNames[index], SceUtilitySavedataParam.icon0FileName);
				SeekableDataInput iconDataInput = Modules.IoFileMgrForUserModule.getFile(iconFileName, IoFileMgrForUser.PSP_O_RDONLY);
				if (iconDataInput != null)
				{
					try
					{
						int length = (int) iconDataInput.length();
						sbyte[] iconBuffer = new sbyte[length];
						iconDataInput.readFully(iconBuffer);
						iconDataInput.Dispose();
						iconStream = new System.IO.MemoryStream(iconBuffer);
					}
					catch (IOException e)
					{
						log.debug("getIcon0", e);
					}
				}
				else if (savedataParams.newData != null && savedataParams.newData.icon0 != null)
				{
					// the PspUtilitySavedataListSaveNewData structure contains the default
					// icon to be used for new data.
					int addr = savedataParams.newData.icon0.buf;
					int size = savedataParams.newData.icon0.size;
					if (addr != 0 && size > 0)
					{
						// An incorrect size for the icon0 is accepted, look for the end of the PNG
						size = PNG.getEndOfPNG(Memory.Instance, addr, size);

						sbyte[] iconBuffer = new sbyte[size];
						IMemoryReader memoryReader = MemoryReader.getMemoryReader(addr, size, 1);
						for (int i = 0; i < size; i++)
						{
							iconBuffer[i] = (sbyte) memoryReader.readNext();
						}
						iconStream = new System.IO.MemoryStream(iconBuffer);
					}
				}

				return iconStream;
			}

			internal virtual int getIcon0(int index)
			{
				System.IO.Stream iconStream = getIcon0InputStream(index);

				return readIcon(iconStream);
			}

			internal virtual PSF getPsf(int index)
			{
				PSF psf = null;
				if (index < 0 || index >= saveNames.Length)
				{
					return psf;
				}

				string sfoFileName = savedataParams.getFileName(saveNames[index], SceUtilitySavedataParam.paramSfoFileName);
				SeekableDataInput sfoDataInput = Modules.IoFileMgrForUserModule.getFile(sfoFileName, IoFileMgrForUser.PSP_O_RDONLY);
				if (sfoDataInput != null)
				{
					try
					{
						int length = (int) sfoDataInput.length();
						sbyte[] sfoBuffer = new sbyte[length];
						sfoDataInput.readFully(sfoBuffer);
						sfoDataInput.Dispose();

						psf = new PSF();
						psf.read(ByteBuffer.wrap(sfoBuffer));
					}
					catch (IOException)
					{
					}
				}

				return psf;
			}

			internal virtual void drawIconByRow(int row, int iconX, int iconY, int iconWidth, int iconHeight)
			{
				drawIcon(getIcon0(row), iconX, iconY, iconWidth, iconHeight);
			}

			protected internal override void updateDialog()
			{
				if (numberRows > 0)
				{
					drawIconByRow(selectedRow, 26, 96, icon0Width, icon0Height);

					// Get values (title, detail...) from SFO file
					PSF psf = getPsf(selectedRow);
					if (psf != null)
					{
						string title = psf.getString("TITLE");
						string detail = psf.getString("SAVEDATA_DETAIL");
						string savedataTitle = psf.getString("SAVEDATA_TITLE");
						DateTime savedTime = savedataParams.getSavedTime(saveNames[selectedRow]);

						int textX = 180;
						int textY = 119;

						drawTextWithShadow(textX, textY, 0xD1C6BA, 0.85f, truncateText(title, Screen.width - textX, 0.85f));

						textY += 22;
						if (savedTime != null)
						{
							string text = formatDateTime(savedTime);
							int sizeKb = savedataParams.getSizeKb(savedataParams.gameName, saveNames[selectedRow]);
							text += " " + MemoryStick.getSizeKbString(sizeKb);
							drawTextWithShadow(textX, textY, 0.7f, text);
						}

						// Draw horizontal line below title
						gu.sceGuDrawHorizontalLine(textX, Screen.width, textY - 6, unchecked((int)0xFF000000) | textColor);

						textX -= 5;
						textY += 23;
						drawTextWithShadow(textX, textY, 0.7f, savedataTitle);

						textY += 24;
						drawTextWithShadow(textX, textY, 0.7f, detail);
					}
					else
					{
						drawTextWithShadow(180, 130, defaultFontScale, strNewData);
					}

					drawEnter();
					drawBack();

					if (selectedRow > 0)
					{
						drawIconByRow(selectedRow - 1, 58, 38, smallIcon0Width, smallIcon0Height);
						if (selectedRow > 1)
						{
							drawIconByRow(selectedRow - 2, 58, -5, smallIcon0Width, smallIcon0Height);
						}
					}
					if (selectedRow < numberRows - 1)
					{
						drawIconByRow(selectedRow + 1, 58, 190, smallIcon0Width, smallIcon0Height);
						if (selectedRow < numberRows - 2)
						{
							drawIconByRow(selectedRow + 2, 58, 233, smallIcon0Width, smallIcon0Height);
						}
					}
				}
				else
				{
					gu.sceGuDrawHorizontalLine(201, 464, 114, unchecked((int)0xFF000000) | textColor);
					drawTextWithShadow(centerText(DefaultFontInfo, strNoData, 201, 464, defaultFontScale), 131, defaultFontScale, strNoData);
					gu.sceGuDrawHorizontalLine(201, 464, 157, unchecked((int)0xFF000000) | textColor);
					drawBack();
				}

				string dialogTitle = savedataDialogState.getDialogTitle(savedataParams.ModeName, "Savedata List", utilityLocale);
				drawHeader(dialogTitle);
			}

			public override void checkController()
			{
				if (DownPressed)
				{
					// One row down
					selectedRow++;
				}
				else if (UpPressed)
				{
					// One row up
					selectedRow--;
				}
				else if (isButtonPressed(sceCtrl.PSP_CTRL_LTRIGGER))
				{
					// First row
					selectedRow = 0;
				}
				else if (isButtonPressed(sceCtrl.PSP_CTRL_RTRIGGER))
				{
					// Last row
					selectedRow = numberRows - 1;
				}

				selectedRow = max(selectedRow, 0);
				selectedRow = min(selectedRow, numberRows - 1);

				if (selectedRow >= 0)
				{
					savedataDialogState.saveListSelection = saveNames[selectedRow];
					savedataDialogState.saveListSelectionIcon0 = getIcon0InputStream(selectedRow);
				}
				else
				{
					savedataDialogState.saveListSelection = null;
					savedataDialogState.saveListSelectionIcon0 = null;
				}

				base.checkController();
			}

			protected internal override bool canConfirm()
			{
				// Can only confirm if at least one row is displayed
				return numberRows > 0;
			}
		}

		protected internal class GuMsgDialog : GuUtilityDialog
		{

			protected internal SceUtilityMsgDialogParams msgDialogParams;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public GuMsgDialog(final pspsharp.HLE.kernel.types.SceUtilityMsgDialogParams msgDialogParams, MsgDialogUtilityDialogState msgDialogState)
			public GuMsgDialog(SceUtilityMsgDialogParams msgDialogParams, MsgDialogUtilityDialogState msgDialogState) : base(msgDialogParams.@base)
			{
				this.msgDialogParams = msgDialogParams;
				msgDialogState.YesSelected = msgDialogParams.OptionYesNoDefaultYes;

				createDialog(msgDialogState);
			}

			protected internal override void updateDialog()
			{
				// Shadows are softer in MsgDialog
				SoftShadows = true;

				// Clear screen in light gray color. Do not clear depth and stencil values.
				gu.sceGuClear(CLEAR_COLOR_BUFFER, 0x968681);

				int buttonY = 192;
				string message = Message;
				if (!string.ReferenceEquals(message, null))
				{
					int currentLineLength = 0;
					IList<string> lines = new LinkedList<string>();
					StringBuilder currentLine = new StringBuilder();
					int splitLineIndex = -1;
					int maxLineLength = 430;
					int longestLine = 0;
					SceFontInfo fontInfo = DefaultFontInfo;
					for (int i = 0; i < message.Length; i++)
					{
						char c = message[i];
						if (c == '\n')
						{
							longestLine = System.Math.Max(longestLine, currentLineLength);
							lines.Add(currentLine.ToString());
							currentLine.Length = 0;
							currentLineLength = 0;
							splitLineIndex = -1;
						}
						else
						{
							int charLength = getTextLength(fontInfo, c);
							if (currentLineLength + charLength > maxLineLength)
							{
								if (splitLineIndex < 0)
								{
									splitLineIndex = currentLine.Length;
								}
								string line = currentLine.Substring(0, splitLineIndex);
								longestLine = System.Math.Max(longestLine, getTextLength(fontInfo, line));
								lines.Add(line);
								currentLine.Remove(0, splitLineIndex);
								currentLineLength = getTextLength(fontInfo, currentLine.ToString());
								splitLineIndex = -1;
							}

							currentLine.Append(c);
							currentLineLength += charLength;

							if (c == ' ')
							{
								splitLineIndex = currentLine.Length;
							}
						}
					}
					if (currentLine.Length > 0)
					{
						longestLine = System.Math.Max(longestLine, currentLineLength);
						lines.Add(currentLine.ToString());
					}
					const int lineHeight = 19;
					int lineCount = lines.Count;
					int textHeight = lineHeight * lineCount;
					int totalHeight = textHeight + 24;
					if (msgDialogParams.OptionYesNo || msgDialogParams.OptionOk)
					{
						// Add height for button(s)
						totalHeight += 33;
					}
					int topLineY = (Screen.height - totalHeight) / 2;
					buttonY = topLineY + totalHeight - 29;

					int lineColor = unchecked((int)0xFFDFDAD9);
					// Draw top line
					gu.sceGuDrawHorizontalLine(60, 420, topLineY, lineColor);
					// Draw bottom line
					gu.sceGuDrawHorizontalLine(60, 420, topLineY + totalHeight, lineColor);

					const float scale = 0.79f;
					int y = topLineY + 17;
					// Center the text
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int x = 63 + (360 - Math.round(longestLine * scale)) / 2;
					int x = 63 + (360 - System.Math.Round(longestLine * scale)) / 2;
					foreach (string line in lines)
					{
						drawTextWithShadow(x, y, scale, line);
						y += lineHeight;
					}
				}

				if (msgDialogParams.OptionYesNo)
				{
					drawYesNo(185, 255, buttonY);
				}
				else if (msgDialogParams.OptionOk)
				{
					drawButton(223, buttonY, "OK", true);
				}

				if ((msgDialogParams.options & SceUtilityMsgDialogParams.PSP_UTILITY_MSGDIALOG_OPTION_NORMAL) != 0 && !msgDialogParams.OptionOk && !msgDialogParams.OptionYesNo)
				{
					// In this case, no buttons are displayed to the user.
					// In the PSP the user waits a few seconds and the dialog closes itself.
					useNoButtons();
				}
				else
				{
					if ((msgDialogParams.options & SceUtilityMsgDialogParams.PSP_UTILITY_MSGDIALOG_OPTION_DISABLE_CANCEL) == SceUtilityMsgDialogParams.PSP_UTILITY_MSGDIALOG_OPTION_ENABLE_CANCEL)
					{
						// Enter is not displayed when all options are 0
						if (msgDialogParams.options != 0)
						{
							if (!string.ReferenceEquals(msgDialogParams.enterButtonString, null))
							{
								if (!msgDialogParams.enterButtonString.Equals(""))
								{
									drawEnterWithString(msgDialogParams.enterButtonString);
								}
								else
								{
									drawEnter();
								}
							}
							else
							{
								drawEnter();
							}
						}
						if (!string.ReferenceEquals(msgDialogParams.backButtonString, null))
						{
							if (!msgDialogParams.backButtonString.Equals(""))
							{
								drawBackWithString(msgDialogParams.backButtonString);
							}
							else
							{
								drawBack();
							}
						}
						else
						{
							drawBack();
						}
					}
					else if ((msgDialogParams.options & SceUtilityMsgDialogParams.PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_MASK) != SceUtilityMsgDialogParams.PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_NONE)
					{
						if (!string.ReferenceEquals(msgDialogParams.enterButtonString, null))
						{
							if (!msgDialogParams.enterButtonString.Equals(""))
							{
								drawEnterWithString(msgDialogParams.enterButtonString);
							}
							else
							{
								drawEnter();
							}
						}
						else
						{
							drawEnter();
						}
					}
				}
			}

			protected internal virtual string Message
			{
				get
				{
					if (msgDialogParams.mode == SceUtilityMsgDialogParams.PSP_UTILITY_MSGDIALOG_MODE_ERROR)
					{
						return string.Format("Error 0x{0:X8}", msgDialogParams.errorValue);
					}
					return msgDialogParams.message;
				}
			}

			protected internal override bool canCancel()
			{
				return (msgDialogParams.options & SceUtilityMsgDialogParams.PSP_UTILITY_MSGDIALOG_OPTION_DISABLE_CANCEL) == SceUtilityMsgDialogParams.PSP_UTILITY_MSGDIALOG_OPTION_ENABLE_CANCEL;
			}

			protected internal override bool canConfirm()
			{
				return (msgDialogParams.options & SceUtilityMsgDialogParams.PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_MASK) != SceUtilityMsgDialogParams.PSP_UTILITY_MSGDIALOG_OPTION_BUTTON_TYPE_NONE;
			}

			protected internal override int ButtonPressedOK
			{
				get
				{
					if (msgDialogParams.OptionYesNo)
					{
						return utilityDialogState.YesSelected ? SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_YES : SceUtilityMsgDialogParams.PSP_UTILITY_BUTTON_PRESSED_NO;
					}
    
					return base.ButtonPressedOK;
				}
			}

			protected internal override bool hasYesNo()
			{
				return msgDialogParams.OptionYesNo;
			}
		}

		protected internal class OskDialog : UtilityDialog
		{

			internal const long serialVersionUID = 1155047781007677923L;
			protected internal JTextField textField;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public OskDialog(final pspsharp.HLE.kernel.types.SceUtilityOskParams oskParams, OskUtilityDialogState oskState)
			public OskDialog(SceUtilityOskParams oskParams, OskUtilityDialogState oskState)
			{
				createDialog(oskState, oskParams.oskData.desc);

				textField = new JTextField(oskParams.oskData.inText);
				messagePane.add(textField);

				JButton okButton = new JButton("Ok");
				okButton.addActionListener(closeActionListener);
				okButton.ActionCommand = actionCommandOK;
				buttonPane.add(okButton);
				DefaultButton = okButton;

				endDialog();
			}
		}

		public static string SystemParamNickname
		{
			get
			{
				return Settings.Instance.readString(SYSTEMPARAM_SETTINGS_OPTION_NICKNAME);
			}
		}

		public static int SystemParamAdhocChannel
		{
			get
			{
				int indexedValue = Settings.Instance.readInt(SYSTEMPARAM_SETTINGS_OPTION_ADHOC_CHANNEL, 0);
				string[] guiValues = SettingsGUI.SysparamAdhocChannels;
    
				int channel = 0;
				// Invalid value?
				if (guiValues == null || indexedValue < 0 || indexedValue >= guiValues.Length)
				{
					return channel;
				}
    
				// Auto value?
				if (indexedValue == 0)
				{
					return channel;
				}
    
				try
				{
					channel = int.Parse(guiValues[indexedValue]);
				}
				catch (System.FormatException)
				{
					log.error(string.Format("Invalid channel settings value {0:D} from {1}", indexedValue, guiValues));
				}
    
				return channel;
			}
		}

		public static int SystemParamWlanPowersave
		{
			get
			{
				return Settings.Instance.readInt(SYSTEMPARAM_SETTINGS_OPTION_WLAN_POWER_SAVE, 0);
			}
		}

		public static int SystemParamDateFormat
		{
			get
			{
				return Settings.Instance.readInt(SYSTEMPARAM_SETTINGS_OPTION_DATE_FORMAT, PSP_SYSTEMPARAM_DATE_FORMAT_YYYYMMDD);
			}
		}

		public static int SystemParamTimeFormat
		{
			get
			{
				return Settings.Instance.readInt(SYSTEMPARAM_SETTINGS_OPTION_TIME_FORMAT, PSP_SYSTEMPARAM_TIME_FORMAT_24HR);
			}
		}

		public static int SystemParamTimeZone
		{
			get
			{
				return Settings.Instance.readInt(SYSTEMPARAM_SETTINGS_OPTION_TIME_ZONE, 0);
			}
		}

		public static int SystemParamDaylightSavingTime
		{
			get
			{
				return Settings.Instance.readInt(SYSTEMPARAM_SETTINGS_OPTION_DAYLIGHT_SAVING_TIME, 0);
			}
		}

		public static int SystemParamLanguage
		{
			get
			{
				return Settings.Instance.readInt(SYSTEMPARAM_SETTINGS_OPTION_LANGUAGE, PSP_SYSTEMPARAM_LANGUAGE_ENGLISH);
			}
		}

		public static int SystemParamButtonPreference
		{
			get
			{
				return Settings.Instance.readInt(SYSTEMPARAM_SETTINGS_OPTION_BUTTON_PREFERENCE, PSP_SYSTEMPARAM_BUTTON_CROSS);
			}
		}

		public static int SystemParamLockParentalLevel
		{
			get
			{
				return Settings.Instance.readInt(SYSTEMPARAM_SETTINGS_OPTION_LOCK_PARENTAL_LEVEL, 0);
			}
		}

		protected internal static string getNetParamName(int id)
		{
			if (id == 0)
			{
				return "";
			}
			return string.format(dummyNetParamName, id);
		}

		protected internal static string formatMessageForDialog(string message)
		{
			StringBuilder formattedMessage = new StringBuilder();

			for (int i = 0; i < message.Length;)
			{
				string rest = message.Substring(i);
				int newLineIndex = rest.IndexOf("\n", StringComparison.Ordinal);
				if (newLineIndex >= 0 && newLineIndex < maxLineLengthForDialog)
				{
					formattedMessage.Append(rest.Substring(0, newLineIndex + 1));
					i += newLineIndex + 1;
				}
				else if (rest.Length > maxLineLengthForDialog)
				{
					int lastSpace = rest.LastIndexOf(' ', maxLineLengthForDialog);
					rest = rest.Substring(0, (lastSpace >= 0 ? lastSpace : maxLineLengthForDialog));
					formattedMessage.Append(rest);
					i += rest.Length + 1;
					formattedMessage.Append("\n");
				}
				else
				{
					formattedMessage.Append(rest);
					i += rest.Length;
				}
			}

			return formattedMessage.ToString();
		}

		public sealed class UtilityModule
		{
			public static readonly UtilityModule PSP_MODULE_NET_COMMON = new UtilityModule("PSP_MODULE_NET_COMMON", InnerEnum.PSP_MODULE_NET_COMMON, 0x0100);
			public static readonly UtilityModule PSP_MODULE_NET_ADHOC = new UtilityModule("PSP_MODULE_NET_ADHOC", InnerEnum.PSP_MODULE_NET_ADHOC, 0x0101);
			public static readonly UtilityModule PSP_MODULE_NET_INET = new UtilityModule("PSP_MODULE_NET_INET", InnerEnum.PSP_MODULE_NET_INET, 0x0102);
			public static readonly UtilityModule PSP_MODULE_NET_PARSEURI = new UtilityModule("PSP_MODULE_NET_PARSEURI", InnerEnum.PSP_MODULE_NET_PARSEURI, 0x0103);
			public static readonly UtilityModule PSP_MODULE_NET_PARSEHTTP = new UtilityModule("PSP_MODULE_NET_PARSEHTTP", InnerEnum.PSP_MODULE_NET_PARSEHTTP, 0x0104);
			public static readonly UtilityModule PSP_MODULE_NET_HTTP = new UtilityModule("PSP_MODULE_NET_HTTP", InnerEnum.PSP_MODULE_NET_HTTP, 0x0105);
			public static readonly UtilityModule PSP_MODULE_NET_SSL = new UtilityModule("PSP_MODULE_NET_SSL", InnerEnum.PSP_MODULE_NET_SSL, 0x0106, new string[] {"flash0:/kd/libssl.prx", "flash0:/kd/cert_loader.prx"});
			public static readonly UtilityModule PSP_MODULE_NET_UPNP = new UtilityModule("PSP_MODULE_NET_UPNP", InnerEnum.PSP_MODULE_NET_UPNP, 0x0107);
			public static readonly UtilityModule PSP_MODULE_NET_HTTPSTORAGE = new UtilityModule("PSP_MODULE_NET_HTTPSTORAGE", InnerEnum.PSP_MODULE_NET_HTTPSTORAGE, 0x0108);
			public static readonly UtilityModule PSP_MODULE_USB_PSPCM = new UtilityModule("PSP_MODULE_USB_PSPCM", InnerEnum.PSP_MODULE_USB_PSPCM, 0x0200);
			public static readonly UtilityModule PSP_MODULE_USB_MIC = new UtilityModule("PSP_MODULE_USB_MIC", InnerEnum.PSP_MODULE_USB_MIC, 0x0201);
			public static readonly UtilityModule PSP_MODULE_USB_CAM = new UtilityModule("PSP_MODULE_USB_CAM", InnerEnum.PSP_MODULE_USB_CAM, 0x0202);
			public static readonly UtilityModule PSP_MODULE_USB_GPS = new UtilityModule("PSP_MODULE_USB_GPS", InnerEnum.PSP_MODULE_USB_GPS, 0x0203);
			public static readonly UtilityModule PSP_MODULE_AV_AVCODEC = new UtilityModule("PSP_MODULE_AV_AVCODEC", InnerEnum.PSP_MODULE_AV_AVCODEC, 0x0300, "flash0:/kd/avcodec.prx");
			public static readonly UtilityModule PSP_MODULE_AV_SASCORE = new UtilityModule("PSP_MODULE_AV_SASCORE", InnerEnum.PSP_MODULE_AV_SASCORE, 0x0301, "flash0:/kd/sc_sascore.prx");
			public static readonly UtilityModule PSP_MODULE_AV_ATRAC3PLUS = new UtilityModule("PSP_MODULE_AV_ATRAC3PLUS", InnerEnum.PSP_MODULE_AV_ATRAC3PLUS, 0x0302, "flash0:/kd/libatrac3plus.prx");
			public static readonly UtilityModule PSP_MODULE_AV_MPEGBASE = new UtilityModule("PSP_MODULE_AV_MPEGBASE", InnerEnum.PSP_MODULE_AV_MPEGBASE, 0x0303, "flash0:/kd/mpeg.prx");
			public static readonly UtilityModule PSP_MODULE_AV_MP3 = new UtilityModule("PSP_MODULE_AV_MP3", InnerEnum.PSP_MODULE_AV_MP3, 0x0304);
			public static readonly UtilityModule PSP_MODULE_AV_VAUDIO = new UtilityModule("PSP_MODULE_AV_VAUDIO", InnerEnum.PSP_MODULE_AV_VAUDIO, 0x0305);
			public static readonly UtilityModule PSP_MODULE_AV_AAC = new UtilityModule("PSP_MODULE_AV_AAC", InnerEnum.PSP_MODULE_AV_AAC, 0x0306);
			public static readonly UtilityModule PSP_MODULE_AV_G729 = new UtilityModule("PSP_MODULE_AV_G729", InnerEnum.PSP_MODULE_AV_G729, 0x0307);
			public static readonly UtilityModule PSP_MODULE_AV_MP4 = new UtilityModule("PSP_MODULE_AV_MP4", InnerEnum.PSP_MODULE_AV_MP4, 0x0308, new string[] {"flash0:/kd/libmp4.prx", "flash0:/kd/mp4msv.prx"});
			public static readonly UtilityModule PSP_MODULE_NP_COMMON = new UtilityModule("PSP_MODULE_NP_COMMON", InnerEnum.PSP_MODULE_NP_COMMON, 0x0400, new string[] {"flash0:/kd/np.prx", "flash0:/kd/np_core.prx", "flash0:/kd/np_auth.prx"});
			public static readonly UtilityModule PSP_MODULE_NP_SERVICE = new UtilityModule("PSP_MODULE_NP_SERVICE", InnerEnum.PSP_MODULE_NP_SERVICE, 0x0401, "flash0:/kd/np_service.prx");
			public static readonly UtilityModule PSP_MODULE_NP_MATCHING2 = new UtilityModule("PSP_MODULE_NP_MATCHING2", InnerEnum.PSP_MODULE_NP_MATCHING2, 0x0402, "flash0:/kd/np_matching2.prx");
			public static readonly UtilityModule PSP_MODULE_NP_COMMERCE2 = new UtilityModule("PSP_MODULE_NP_COMMERCE2", InnerEnum.PSP_MODULE_NP_COMMERCE2, 0x0403, "flash0:/kd/np_commerce2.prx");
			public static readonly UtilityModule PSP_MODULE_NP_DRM = new UtilityModule("PSP_MODULE_NP_DRM", InnerEnum.PSP_MODULE_NP_DRM, 0x0500);
			public static readonly UtilityModule PSP_MODULE_IRDA = new UtilityModule("PSP_MODULE_IRDA", InnerEnum.PSP_MODULE_IRDA, 0x0600);

			private static readonly IList<UtilityModule> valueList = new List<UtilityModule>();

			static UtilityModule()
			{
				valueList.Add(PSP_MODULE_NET_COMMON);
				valueList.Add(PSP_MODULE_NET_ADHOC);
				valueList.Add(PSP_MODULE_NET_INET);
				valueList.Add(PSP_MODULE_NET_PARSEURI);
				valueList.Add(PSP_MODULE_NET_PARSEHTTP);
				valueList.Add(PSP_MODULE_NET_HTTP);
				valueList.Add(PSP_MODULE_NET_SSL);
				valueList.Add(PSP_MODULE_NET_UPNP);
				valueList.Add(PSP_MODULE_NET_HTTPSTORAGE);
				valueList.Add(PSP_MODULE_USB_PSPCM);
				valueList.Add(PSP_MODULE_USB_MIC);
				valueList.Add(PSP_MODULE_USB_CAM);
				valueList.Add(PSP_MODULE_USB_GPS);
				valueList.Add(PSP_MODULE_AV_AVCODEC);
				valueList.Add(PSP_MODULE_AV_SASCORE);
				valueList.Add(PSP_MODULE_AV_ATRAC3PLUS);
				valueList.Add(PSP_MODULE_AV_MPEGBASE);
				valueList.Add(PSP_MODULE_AV_MP3);
				valueList.Add(PSP_MODULE_AV_VAUDIO);
				valueList.Add(PSP_MODULE_AV_AAC);
				valueList.Add(PSP_MODULE_AV_G729);
				valueList.Add(PSP_MODULE_AV_MP4);
				valueList.Add(PSP_MODULE_NP_COMMON);
				valueList.Add(PSP_MODULE_NP_SERVICE);
				valueList.Add(PSP_MODULE_NP_MATCHING2);
				valueList.Add(PSP_MODULE_NP_COMMERCE2);
				valueList.Add(PSP_MODULE_NP_DRM);
				valueList.Add(PSP_MODULE_IRDA);
			}

			public enum InnerEnum
			{
				PSP_MODULE_NET_COMMON,
				PSP_MODULE_NET_ADHOC,
				PSP_MODULE_NET_INET,
				PSP_MODULE_NET_PARSEURI,
				PSP_MODULE_NET_PARSEHTTP,
				PSP_MODULE_NET_HTTP,
				PSP_MODULE_NET_SSL,
				PSP_MODULE_NET_UPNP,
				PSP_MODULE_NET_HTTPSTORAGE,
				PSP_MODULE_USB_PSPCM,
				PSP_MODULE_USB_MIC,
				PSP_MODULE_USB_CAM,
				PSP_MODULE_USB_GPS,
				PSP_MODULE_AV_AVCODEC,
				PSP_MODULE_AV_SASCORE,
				PSP_MODULE_AV_ATRAC3PLUS,
				PSP_MODULE_AV_MPEGBASE,
				PSP_MODULE_AV_MP3,
				PSP_MODULE_AV_VAUDIO,
				PSP_MODULE_AV_AAC,
				PSP_MODULE_AV_G729,
				PSP_MODULE_AV_MP4,
				PSP_MODULE_NP_COMMON,
				PSP_MODULE_NP_SERVICE,
				PSP_MODULE_NP_MATCHING2,
				PSP_MODULE_NP_COMMERCE2,
				PSP_MODULE_NP_DRM,
				PSP_MODULE_IRDA
			}

			public readonly InnerEnum innerEnumValue;
			private readonly string nameValue;
			private readonly int ordinalValue;
			private static int nextOrdinal = 0;

			internal int id;
			internal string[] prxNames;

			internal UtilityModule(string name, InnerEnum innerEnum, int id)
			{
				this.id = id;

				nameValue = name;
				ordinalValue = nextOrdinal++;
				innerEnumValue = innerEnum;
			}

			internal UtilityModule(string name, InnerEnum innerEnum, int id, string prxName)
			{
				this.id = id;
				prxNames = new string[] {prxName};

				nameValue = name;
				ordinalValue = nextOrdinal++;
				innerEnumValue = innerEnum;
			}

			internal UtilityModule(string name, InnerEnum innerEnum, int id, string[] prxNames)
			{
				this.id = id;
				this.prxNames = prxNames;

				nameValue = name;
				ordinalValue = nextOrdinal++;
				innerEnumValue = innerEnum;
			}

			public int ID
			{
				get
				{
					return id;
				}
			}

			public string[] PrxNames
			{
				get
				{
					if (prxNames == null)
					{
						return new string[] {outerInstance.ToString()};
					}
					return prxNames;
				}
			}

			public static IList<UtilityModule> values()
			{
				return valueList;
			}

			public int ordinal()
			{
				return ordinalValue;
			}

			public override string ToString()
			{
				return nameValue;
			}

			public static UtilityModule valueOf(string name)
			{
				foreach (UtilityModule enumInstance in UtilityModule.valueList)
				{
					if (enumInstance.nameValue == name)
					{
						return enumInstance;
					}
				}
				throw new System.ArgumentException(name);
			}
		}

		protected internal virtual string[] getModuleNames(int module)
		{
			foreach (UtilityModule m in UtilityModule.values())
			{
				if (m.ID == module)
				{
					return m.PrxNames;
				}
			}
			return new string[] {string.Format("PSP_MODULE_UNKNOWN_{0:X}", module)};
		}

		protected internal virtual int hleUtilityLoadModule(int module, string moduleName)
		{
			// Extract the PRX name from the module name
			string prxName = moduleName;
			if (moduleName.EndsWith(".prx", StringComparison.Ordinal))
			{
				prxName = StringHelper.SubstringSpecial(moduleName, moduleName.LastIndexOf("/", StringComparison.Ordinal) + 1, moduleName.Length - 4);
			}

			HLEModuleManager moduleManager = HLEModuleManager.Instance;
			if (!moduleManager.hasFlash0Module(prxName))
			{ // Can't load flash0 module.
				waitingModules[module] = moduleName; // Always save a load attempt.
				log.error("Can't load flash0 module");
				return SceKernelErrors.ERROR_MODULE_BAD_ID;
			}

			// Load and save it in loadedModules.
			int sceModuleId;
			if (moduleName.Equals(prxName))
			{
				sceModuleId = moduleManager.LoadFlash0Module(moduleName);
			}
			else
			{
				sceModuleId = Modules.ModuleMgrForUserModule.hleKernelLoadAndStartModule(moduleName, 0x10);
			}
			SceModule sceModule = Managers.modules.getModuleByUID(sceModuleId);
			if (!loadedModules.ContainsKey(module))
			{
				loadedModules[module] = new LinkedList<SceModule>();
			}
			loadedModules[module].Add(sceModule);

			return 0;
		}

		protected internal virtual int hleUtilityUnloadModule(int module)
		{
			if (loadedModules.ContainsKey(module))
			{
				// Unload the module.
				HLEModuleManager moduleManager = HLEModuleManager.Instance;
				foreach (SceModule sceModule in loadedModules.Remove(module))
				{
					moduleManager.UnloadFlash0Module(sceModule);
				}
				return 0;
			}
			else if (waitingModules.ContainsKey(module))
			{
				// Simulate a successful unload.
				waitingModules.Remove(module);
				return 0;
			}
			else
			{
				log.error("Not yet loaded");
				return SceKernelErrors.ERROR_MODULE_NOT_LOADED;
			}
		}

		private string getAvModuleName(int module)
		{
			if (module < 0 || module >= utilityAvModuleNames.Length)
			{
				return "PSP_AV_MODULE_UNKNOWN_" + module;
			}
			return utilityAvModuleNames[module];
		}

		private string getUsbModuleName(int module)
		{
			if (module < 0 || module >= utilityUsbModuleNames.Length)
			{
				return "PSP_USB_MODULE_UNKNOWN_" + module;
			}
			return utilityUsbModuleNames[module];
		}

		protected internal virtual int hleUtilityLoadAvModule(int module, string moduleName)
		{
			HLEModuleManager moduleManager = HLEModuleManager.Instance;
			if (loadedAvModules.ContainsKey(module) || waitingAvModules.ContainsKey(module))
			{ // Module already loaded.
				return SceKernelErrors.ERROR_AV_MODULE_ALREADY_LOADED;
			}
			else if (!moduleManager.hasFlash0Module(moduleName))
			{ // Can't load flash0 module.
				waitingAvModules[module] = moduleName; // Always save a load attempt.
				return SceKernelErrors.ERROR_AV_MODULE_BAD_ID;
			}
			else
			{
				// Load and save it in loadedAvModules.
				int sceModuleId = moduleManager.LoadFlash0Module(moduleName);
				SceModule sceModule = Managers.modules.getModuleByUID(sceModuleId);
				loadedAvModules[module] = sceModule;
				return 0;
			}
		}

		protected internal virtual int hleUtilityLoadUsbModule(int module, string moduleName)
		{
			HLEModuleManager moduleManager = HLEModuleManager.Instance;
			if (loadedUsbModules.ContainsKey(module) || waitingUsbModules.ContainsKey(module))
			{ // Module already loaded.
				return SceKernelErrors.ERROR_AV_MODULE_ALREADY_LOADED;
			}
			else if (!moduleManager.hasFlash0Module(moduleName))
			{ // Can't load flash0 module.
				waitingUsbModules[module] = moduleName; // Always save a load attempt.
				return SceKernelErrors.ERROR_AV_MODULE_BAD_ID;
			}
			else
			{
				// Load and save it in loadedAvModules.
				int sceModuleId = moduleManager.LoadFlash0Module(moduleName);
				SceModule sceModule = Managers.modules.getModuleByUID(sceModuleId);
				loadedUsbModules[module] = sceModule;
				return 0;
			}
		}

		protected internal virtual int hleUtilityUnloadAvModule(int module)
		{
			if (loadedAvModules.ContainsKey(module))
			{
				// Unload the module.
				HLEModuleManager moduleManager = HLEModuleManager.Instance;
				SceModule sceModule = loadedAvModules.Remove(module);
				moduleManager.UnloadFlash0Module(sceModule);
				return 0;
			}
			else if (waitingAvModules.ContainsKey(module))
			{
				// Simulate a successful unload.
				waitingAvModules.Remove(module);
				return 0;
			}
			else
			{
				return SceKernelErrors.ERROR_AV_MODULE_NOT_LOADED;
			}
		}

		[HLEFunction(nid : 0xC492F751, version : 150)]
		public virtual int sceUtilityGameSharingInitStart(TPointer paramsAddr)
		{
			return gameSharingState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0xEFC6F80F, version : 150)]
		public virtual int sceUtilityGameSharingShutdownStart()
		{
			return gameSharingState.executeShutdownStart();
		}

		[HLEFunction(nid : 0x7853182D, version : 150)]
		public virtual int sceUtilityGameSharingUpdate(int drawSpeed)
		{
			return gameSharingState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0x946963F3, version : 150)]
		public virtual int sceUtilityGameSharingGetStatus()
		{
			return gameSharingState.executeGetStatus();
		}

		[HLEFunction(nid : 0x3AD50AE7, version : 150)]
		public virtual int sceNetplayDialogInitStart(TPointer paramsAddr)
		{
			return netplayDialogState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0xBC6B6296, version : 150)]
		public virtual int sceNetplayDialogShutdownStart()
		{
			return netplayDialogState.executeShutdownStart();
		}

		[HLEFunction(nid : 0x417BED54, version : 150)]
		public virtual int sceNetplayDialogUpdate(int drawSpeed)
		{
			return netplayDialogState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0xB6CEE597, version : 150)]
		public virtual int sceNetplayDialogGetStatus()
		{
			return netplayDialogState.executeGetStatus();
		}

		[HLEFunction(nid : 0x4DB1E739, version : 150)]
		public virtual int sceUtilityNetconfInitStart(TPointer paramsAddr)
		{
			return netconfState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0xF88155F6, version : 150)]
		public virtual int sceUtilityNetconfShutdownStart()
		{
			return netconfState.executeShutdownStart();
		}

		[HLEFunction(nid : 0x91E70E35, version : 150)]
		public virtual int sceUtilityNetconfUpdate(int drawSpeed)
		{
			return netconfState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0x6332AA39, version : 150)]
		public virtual int sceUtilityNetconfGetStatus()
		{
			return netconfState.executeGetStatus();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x50C4CD57, version = 150) public int sceUtilitySavedataInitStart(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.variableLength, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer paramsAddr)
		[HLEFunction(nid : 0x50C4CD57, version : 150)]
		public virtual int sceUtilitySavedataInitStart(TPointer paramsAddr)
		{
			return savedataState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0x9790B33C, version : 150)]
		public virtual int sceUtilitySavedataShutdownStart()
		{
			return savedataState.executeShutdownStart();
		}

		[HLEFunction(nid : 0xD4B95FFB, version : 150)]
		public virtual int sceUtilitySavedataUpdate(int drawSpeed)
		{
			return savedataState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0x8874DBE0, version : 150)]
		public virtual int sceUtilitySavedataGetStatus()
		{
			return savedataState.executeGetStatus();
		}

		[HLEFunction(nid : 0x2995D020, version : 150)]
		public virtual int sceUtilitySavedataErrInitStart(TPointer paramsAddr)
		{
			return savedataErrState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0xB62A4061, version : 150)]
		public virtual int sceUtilitySavedataErrShutdownStart()
		{
			return savedataErrState.executeShutdownStart();
		}

		[HLEFunction(nid : 0xED0FAD38, version : 150)]
		public virtual int sceUtilitySavedataErrUpdate(int drawSpeed)
		{
			return savedataErrState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0x88BC7406, version : 150)]
		public virtual int sceUtilitySavedataErrGetStatus()
		{
			return savedataErrState.executeGetStatus();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x2AD8E239, version = 150) public int sceUtilityMsgDialogInitStart(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.variableLength, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer paramsAddr)
		[HLEFunction(nid : 0x2AD8E239, version : 150)]
		public virtual int sceUtilityMsgDialogInitStart(TPointer paramsAddr)
		{
			return msgDialogState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0x67AF3428, version : 150)]
		public virtual int sceUtilityMsgDialogShutdownStart()
		{
			return msgDialogState.executeShutdownStart();
		}

		[HLEFunction(nid : 0x95FC253B, version : 150)]
		public virtual int sceUtilityMsgDialogUpdate(int drawSpeed)
		{
			return msgDialogState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0x9A1C91D7, version : 150)]
		public virtual int sceUtilityMsgDialogGetStatus()
		{
			return msgDialogState.executeGetStatus();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF6269B82, version = 150) public int sceUtilityOskInitStart(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.variableLength, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer paramsAddr)
		[HLEFunction(nid : 0xF6269B82, version : 150)]
		public virtual int sceUtilityOskInitStart(TPointer paramsAddr)
		{
			return oskState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0x3DFAEBA9, version : 150)]
		public virtual int sceUtilityOskShutdownStart()
		{
			return oskState.executeShutdownStart();
		}

		[HLEFunction(nid : 0x4B85C861, version : 150)]
		public virtual int sceUtilityOskUpdate(int drawSpeed)
		{
			return oskState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0xF3F76017, version : 150)]
		public virtual int sceUtilityOskGetStatus()
		{
			return oskState.executeGetStatus();
		}

		[HLEFunction(nid : 0x16D02AF0, version : 150)]
		public virtual int sceUtilityNpSigninInitStart(TPointer paramsAddr)
		{
			return npSigninState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0xE19C97D6, version : 150)]
		public virtual int sceUtilityNpSigninShutdownStart()
		{
			return npSigninState.executeShutdownStart();
		}

		[HLEFunction(nid : 0xF3FBC572, version : 150)]
		public virtual int sceUtilityNpSigninUpdate(int drawSpeed)
		{
			return npSigninState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0x86ABDB1B, version : 150)]
		public virtual int sceUtilityNpSigninGetStatus()
		{
			return npSigninState.executeGetStatus();
		}

		[HLEFunction(nid : 0x42071A83, version : 150)]
		public virtual int sceUtilityPS3ScanInitStart(TPointer paramsAddr)
		{
			return PS3ScanState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0xD17A0573, version : 150)]
		public virtual int sceUtilityPS3ScanShutdownStart()
		{
			return PS3ScanState.executeShutdownStart();
		}

		[HLEFunction(nid : 0xD852CDCE, version : 150)]
		public virtual int sceUtilityPS3ScanUpdate(int drawSpeed)
		{
			return PS3ScanState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0x89317C8F, version : 150)]
		public virtual int sceUtilityPS3ScanGetStatus()
		{
			return PS3ScanState.executeGetStatus();
		}

		[HLEFunction(nid : 0x81c44706, version : 150)]
		public virtual int sceUtilityRssReaderInitStart(TPointer paramsAddr)
		{
			return rssReaderState.executeInitStart(paramsAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB0FB7FF5, version = 150) public int sceUtilityRssReaderContStart()
		[HLEFunction(nid : 0xB0FB7FF5, version : 150)]
		public virtual int sceUtilityRssReaderContStart()
		{
			return SceKernelErrors.ERROR_UTILITY_IS_UNKNOWN;
		}

		[HLEFunction(nid : 0xE7B778D8, version : 150)]
		public virtual int sceUtilityRssReaderShutdownStart()
		{
			return rssReaderState.executeShutdownStart();
		}

		[HLEFunction(nid : 0x6F56F9CF, version : 150)]
		public virtual int sceUtilityRssReaderUpdate(int drawSpeed)
		{
			return rssReaderState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0x8326AB05, version : 150)]
		public virtual int sceUtilityRssReaderGetStatus()
		{
			return rssReaderState.executeGetStatus();
		}

		[HLEFunction(nid : 0x4B0A8FE5, version : 150)]
		public virtual int sceUtilityRssSubscriberInitStart(TPointer paramsAddr)
		{
			return rssSubscriberState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0x06A48659, version : 150)]
		public virtual int sceUtilityRssSubscriberShutdownStart()
		{
			return rssSubscriberState.executeShutdownStart();
		}

		[HLEFunction(nid : 0xA084E056, version : 150)]
		public virtual int sceUtilityRssSubscriberUpdate(int drawSpeed)
		{
			return rssSubscriberState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0x2B96173B, version : 150)]
		public virtual int sceUtilityRssSubscriberGetStatus()
		{
			return rssSubscriberState.executeGetStatus();
		}

		[HLEFunction(nid : 0x0251B134, version : 150)]
		public virtual int sceUtilityScreenshotInitStart(TPointer paramsAddr)
		{
			return screenshotState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0x86A03A27, version : 150)]
		public virtual int sceUtilityScreenshotContStart(TPointer paramsAddr)
		{
			return screenshotState.executeContStart(paramsAddr);
		}

		[HLEFunction(nid : 0xF9E0008C, version : 150)]
		public virtual int sceUtilityScreenshotShutdownStart()
		{
			return screenshotState.executeShutdownStart();
		}

		[HLEFunction(nid : 0xAB083EA9, version : 150)]
		public virtual int sceUtilityScreenshotUpdate(int drawSpeed)
		{
			return screenshotState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0xD81957B7, version : 150)]
		public virtual int sceUtilityScreenshotGetStatus()
		{
			return screenshotState.executeGetStatus();
		}

		[HLEFunction(nid : 0xCDC3AA41, version : 150)]
		public virtual int sceUtilityHtmlViewerInitStart(TPointer paramsAddr)
		{
			return htmlViewerState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0xF5CE1134, version : 150)]
		public virtual int sceUtilityHtmlViewerShutdownStart()
		{
			return htmlViewerState.executeShutdownStart();
		}

		[HLEFunction(nid : 0x05AFB9E4, version : 150)]
		public virtual int sceUtilityHtmlViewerUpdate(int drawSpeed)
		{
			return htmlViewerState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0xBDA7D894, version : 150)]
		public virtual int sceUtilityHtmlViewerGetStatus()
		{
			return htmlViewerState.executeGetStatus();
		}

		[HLEFunction(nid : 0x24AC31EB, version : 150)]
		public virtual int sceUtilityGamedataInstallInitStart(TPointer paramsAddr)
		{
			return gamedataInstallState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0x32E32DCB, version : 150)]
		public virtual int sceUtilityGamedataInstallShutdownStart()
		{
			return gamedataInstallState.executeShutdownStart();
		}

		[HLEFunction(nid : 0x4AECD179, version : 150)]
		public virtual int sceUtilityGamedataInstallUpdate(int drawSpeed)
		{
			return gamedataInstallState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0xB57E95D9, version : 150)]
		public virtual int sceUtilityGamedataInstallGetStatus()
		{
			return gamedataInstallState.executeGetStatus();
		}

		[HLEFunction(nid : 0x45C18506, version : 150)]
		public virtual int sceUtilitySetSystemParamInt(int id, int value)
		{
			switch (id)
			{
				case PSP_SYSTEMPARAM_ID_INT_ADHOC_CHANNEL:
					if (value != 0 && value != 1 && value != 6 && value != 11)
					{
						return SceKernelErrors.ERROR_UTILITY_INVALID_ADHOC_CHANNEL;
					}
					Settings.Instance.writeInt(SYSTEMPARAM_SETTINGS_OPTION_ADHOC_CHANNEL, value);
					break;
				case PSP_SYSTEMPARAM_ID_INT_WLAN_POWERSAVE:
					Settings.Instance.writeInt(SYSTEMPARAM_SETTINGS_OPTION_WLAN_POWER_SAVE, value);
					break;
				default:
					// PSP can only set above int parameters
					return SceKernelErrors.ERROR_UTILITY_INVALID_SYSTEM_PARAM_ID;
			}

			return 0;
		}

		[HLELogging(level : "info"), HLEFunction(nid : 0x41E30674, version : 150)]
		public virtual int sceUtilitySetSystemParamString(int id, int @string)
		{
			// Always return ERROR_UTILITY_INVALID_SYSTEM_PARAM_ID
			return SceKernelErrors.ERROR_UTILITY_INVALID_SYSTEM_PARAM_ID;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA5DA2406, version = 150) public int sceUtilityGetSystemParamInt(int id, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 valueAddr)
		[HLEFunction(nid : 0xA5DA2406, version : 150)]
		public virtual int sceUtilityGetSystemParamInt(int id, TPointer32 valueAddr)
		{
			switch (id)
			{
				case PSP_SYSTEMPARAM_ID_INT_ADHOC_CHANNEL:
					valueAddr.setValue(SystemParamAdhocChannel);
					break;
				case PSP_SYSTEMPARAM_ID_INT_WLAN_POWERSAVE:
					valueAddr.setValue(SystemParamWlanPowersave);
					break;
				case PSP_SYSTEMPARAM_ID_INT_DATE_FORMAT:
					valueAddr.setValue(SystemParamDateFormat);
					break;
				case PSP_SYSTEMPARAM_ID_INT_TIME_FORMAT:
					valueAddr.setValue(SystemParamTimeFormat);
					break;
				case PSP_SYSTEMPARAM_ID_INT_TIMEZONE:
					valueAddr.setValue(SystemParamTimeZone);
					break;
				case PSP_SYSTEMPARAM_ID_INT_DAYLIGHTSAVINGS:
					valueAddr.setValue(SystemParamDaylightSavingTime);
					break;
				case PSP_SYSTEMPARAM_ID_INT_LANGUAGE:
					valueAddr.setValue(SystemParamLanguage);
					break;
				case PSP_SYSTEMPARAM_ID_INT_BUTTON_PREFERENCE:
					valueAddr.setValue(SystemParamButtonPreference);
					break;
				case PSP_SYSTEMPARAM_ID_INT_LOCK_PARENTAL_LEVEL:
					// This system param ID was introduced somewhere between v5.00 (not available) and v6.20 (available)
					if (Emulator.Instance.FirmwareVersion <= 500)
					{
						log.warn(string.Format("sceUtilityGetSystemParamInt id={0:D}, value_addr={1} PSP_SYSTEMPARAM_ID_INT_LOCK_PARENTAL_LEVEL not available in PSP v{2:D}", id, valueAddr, Emulator.Instance.FirmwareVersion));
						return SceKernelErrors.ERROR_UTILITY_INVALID_SYSTEM_PARAM_ID;
					}
					valueAddr.setValue(SystemParamLockParentalLevel);
					break;
				default:
					log.warn(string.Format("sceUtilityGetSystemParamInt id={0:D}, valueAddr={1} invalid id", id, valueAddr));
					return SceKernelErrors.ERROR_UTILITY_INVALID_SYSTEM_PARAM_ID;
			}

			return 0;
		}

		[HLEFunction(nid : 0x34B78343, version : 150)]
		public virtual int sceUtilityGetSystemParamString(int id, TPointer strAddr, int len)
		{
			switch (id)
			{
				case PSP_SYSTEMPARAM_ID_STRING_NICKNAME:
					strAddr.setStringNZ(len, SystemParamNickname);
					break;
				default:
					log.warn(string.Format("sceUtilityGetSystemParamString id={0:D}, strAddr={1}, len={2:D} invalid id", id, strAddr, len));
					return SceKernelErrors.ERROR_UTILITY_INVALID_SYSTEM_PARAM_ID;
			}

			return 0;
		}

		/// <summary>
		/// Check existence of a Net Configuration
		/// </summary>
		/// <param name="id"> - id of net Configuration (1 to n) </param>
		/// <returns> 0 on success, </returns>
		[HLEFunction(nid : 0x5EEE6548, version : 150)]
		public virtual int sceUtilityCheckNetParam(int id)
		{
			bool available = (id >= 0 && id <= 24);

			// We do not return too many entries as some homebrew only support a limited number of entries.
			if (id > PSP_NETPARAM_MAX_NUMBER_DUMMY_ENTRIES)
			{
				available = false;
			}

			return available ? 0 : SceKernelErrors.ERROR_NETPARAM_BAD_NETCONF;
		}

		/// <summary>
		/// Get Net Configuration Parameter
		/// </summary>
		/// <param name="conf"> - Net Configuration number (1 to n) (0 returns valid but
		/// seems to be a copy of the last config requested) </param>
		/// <param name="param"> - which parameter to get </param>
		/// <param name="data"> - parameter data </param>
		/// <returns> 0 on success, </returns>
		[HLEFunction(nid : 0x434D4B3A, version : 150)]
		public virtual int sceUtilityGetNetParam(int id, int param, TPointer data)
		{
			if (id < 0 || id > 24)
			{
				log.warn(string.Format("sceUtilityGetNetParam invalid id={0:D}", id));
				return SceKernelErrors.ERROR_NETPARAM_BAD_NETCONF;
			}

			switch (param)
			{
				case PSP_NETPARAM_NAME:
					data.StringZ = getNetParamName(id);
					break;
				case PSP_NETPARAM_SSID:
					data.StringZ = sceNetApctl.SSID;
					break;
				case PSP_NETPARAM_SECURE:
					// 0 is no security.
					// 1 is WEP (64bit).
					// 2 is WEP (128bit).
					// 3 is WPA.
					data.setValue32(1);
					break;
				case PSP_NETPARAM_WEPKEY:
					data.StringZ = "XXXXXXXXXXXXXXXXX";
					break;
				case PSP_NETPARAM_IS_STATIC_IP:
					// 0 is DHCP.
					// 1 is static.
					// 2 is PPPOE.
					data.setValue32(1);
					break;
				case PSP_NETPARAM_IP:
					data.StringZ = sceNetApctl.LocalHostIP;
					break;
				case PSP_NETPARAM_NETMASK:
					data.StringZ = sceNetApctl.SubnetMask;
					break;
				case PSP_NETPARAM_ROUTE:
					data.StringZ = sceNetApctl.Gateway;
					break;
				case PSP_NETPARAM_MANUAL_DNS:
					// 0 is auto.
					// 1 is manual.
					data.setValue32(0);
					break;
				case PSP_NETPARAM_PRIMARYDNS:
					data.StringZ = sceNetApctl.PrimaryDNS;
					break;
				case PSP_NETPARAM_SECONDARYDNS:
					data.StringZ = sceNetApctl.SecondaryDNS;
					break;
				case PSP_NETPARAM_PROXY_USER:
					data.StringZ = "pspsharp"; // Faking.
					break;
				case PSP_NETPARAM_PROXY_PASS:
					data.StringZ = "pspsharp"; // Faking.
					break;
				case PSP_NETPARAM_USE_PROXY:
					// 0 is to not use proxy.
					// 1 is to use proxy.
					data.setValue32(0);
					break;
				case PSP_NETPARAM_PROXY_SERVER:
					data.StringZ = "dummy_server"; // Faking.
					break;
				case PSP_NETPARAM_PROXY_PORT:
					data.setValue32(0); // Faking.
					break;
				case PSP_NETPARAM_VERSION:
					// 0 is not used.
					// 1 is old version.
					// 2 is new version.
					data.setValue32(2);
					break;
				case PSP_NETPARAM_UNKNOWN:
					data.setValue32(0);
					break;
				case PSP_NETPARAM_8021X_AUTH_TYPE:
					// 0 is none.
					// 1 is EAP (MD5).
					data.setValue32(0);
					break;
				case PSP_NETPARAM_8021X_USER:
					data.StringZ = "pspsharp"; // Faking.
					break;
				case PSP_NETPARAM_8021X_PASS:
					data.StringZ = "pspsharp"; // Faking.
					break;
				case PSP_NETPARAM_WPA_TYPE:
					// 0 is key in hexadecimal format.
					// 1 is key in ASCII format.
					data.setValue32(0);
					break;
				case PSP_NETPARAM_WPA_KEY:
					data.StringZ = "XXXXXXXXXXXXXXXXX";
					break;
				case PSP_NETPARAM_BROWSER:
					// 0 is to not start the native browser.
					// 1 is to start the native browser.
					data.setValue32(0);
					break;
				case PSP_NETPARAM_WIFI_CONFIG:
					// 0 is no config.
					// 1 is unknown.
					// 2 is Playstation Spot.
					// 3 is unknown.
					data.setValue32(0);
					break;
				default:
					log.warn(string.Format("sceUtilityGetNetParam invalid param {0:D}", param));
					return SceKernelErrors.ERROR_NETPARAM_BAD_PARAM;
			}

			return 0;
		}

		/// <summary>
		/// Get Current Net Configuration ID
		/// </summary>
		/// <param name="idAddr"> - Address to store the current net ID </param>
		/// <returns> 0 on success, </returns>
		[HLEFunction(nid : 0x4FED24D8, version : 150)]
		public virtual int sceUtilityGetNetParamLatestID(TPointer32 idAddr)
		{
			// This function is saving the last net param ID and not
			// the number of net configurations.
			idAddr.setValue(Modules.sceRegModule.NetworkLatestId);

			return 0;
		}

		[HLEFunction(nid : 0x1579A159, version : 200, checkInsideInterrupt : true)]
		public virtual int sceUtilityLoadNetModule(int module)
		{
			string moduleName = getNetModuleName(module);
			int result = hleUtilityLoadNetModule(module, moduleName);
			if (result == SceKernelErrors.ERROR_NET_MODULE_BAD_ID)
			{
				log.info(string.Format("IGNORING: sceUtilityLoadNetModule(module=0x{0:X4}) {1}", module, moduleName));
				Modules.ThreadManForUserModule.hleKernelDelayThread(ModuleMgrForUser.loadHLEModuleDelay, false);
				return 0;
			}

			log.info(string.Format("sceUtilityLoadNetModule(module=0x{0:X4}) {1} loaded", module, moduleName));

			if (result >= 0)
			{
				Modules.ThreadManForUserModule.hleKernelDelayThread(ModuleMgrForUser.loadHLEModuleDelay, false);
			}

			return result;
		}

		[HLEFunction(nid : 0x64D50C56, version : 200, checkInsideInterrupt : true)]
		public virtual int sceUtilityUnloadNetModule(int module)
		{
			string moduleName = getNetModuleName(module);
			log.info(string.Format("sceUtilityUnloadNetModule(module=0x{0:X4}) {1} unloaded", module, moduleName));

			return hleUtilityUnloadNetModule(module);
		}

		[HLEFunction(nid : 0x1281DA8E, version : 200)]
		public virtual int sceUtilityInstallInitStart(TPointer paramsAddr)
		{
			return installState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0x5EF1C24A, version : 200)]
		public virtual int sceUtilityInstallShutdownStart()
		{
			return installState.executeShutdownStart();
		}

		[HLEFunction(nid : 0xA03D29BA, version : 200)]
		public virtual int sceUtilityInstallUpdate(int drawSpeed)
		{
			return installState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0xC4700FA3, version : 200)]
		public virtual int sceUtilityInstallGetStatus()
		{
			return installState.executeGetStatus();
		}

		[HLEFunction(nid : 0x4928BD96, version : 260)]
		public virtual int sceUtilityMsgDialogAbort()
		{
			return msgDialogState.executeAbort();
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0xC629AF26, version : 270, checkInsideInterrupt : true)]
		public virtual int sceUtilityLoadAvModule(int module)
		{
			string moduleName = getAvModuleName(module);
			int result = hleUtilityLoadAvModule(module, moduleName);
			if (result == SceKernelErrors.ERROR_AV_MODULE_BAD_ID)
			{
				log.info(string.Format("IGNORING: sceUtilityLoadAvModule(module=0x{0:X4}) {1}", module, moduleName));
				Modules.ThreadManForUserModule.hleKernelDelayThread(ModuleMgrForUser.loadHLEModuleDelay, false);
				return 0;
			}

			if (result >= 0)
			{
				Modules.ThreadManForUserModule.hleKernelDelayThread(ModuleMgrForUser.loadHLEModuleDelay, false);
			}

			return result;
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0xF7D8D092, version : 270, checkInsideInterrupt : true)]
		public virtual int sceUtilityUnloadAvModule(int module)
		{
			return hleUtilityUnloadAvModule(module);
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x0D5BC6D2, version : 270, checkInsideInterrupt : true)]
		public virtual int sceUtilityLoadUsbModule(int module)
		{
			string moduleName = getUsbModuleName(module);
			int result = hleUtilityLoadUsbModule(module, moduleName);
			if (result == SceKernelErrors.ERROR_AV_MODULE_BAD_ID)
			{
				log.info(string.Format("IGNORING: sceUtilityLoadUsbModule(module=0x{0:X4}) {1}", module, moduleName));
				Modules.ThreadManForUserModule.hleKernelDelayThread(ModuleMgrForUser.loadHLEModuleDelay, false);
				return 0;
			}

			if (result >= 0)
			{
				Modules.ThreadManForUserModule.hleKernelDelayThread(ModuleMgrForUser.loadHLEModuleDelay, false);
			}

			return result;
		}

		[HLEFunction(nid : 0x2A2B3DE0, version : 303, checkInsideInterrupt : true)]
		public virtual int sceUtilityLoadModule(int module)
		{
			if (loadedModules.ContainsKey(module) || waitingModules.ContainsKey(module))
			{ // Module already loaded.
				return SceKernelErrors.ERROR_MODULE_ALREADY_LOADED;
			}
			else if ((module == UtilityModule.PSP_MODULE_NET_HTTPSTORAGE.id) && (!loadedModules.ContainsKey(UtilityModule.PSP_MODULE_NET_HTTP.id)))
			{
				log.error("Library not find");
				return SceKernelErrors.ERROR_KERNEL_LIBRARY_NOT_FOUND;
			}

			int currentThreadID = Modules.ThreadManForUserModule.CurrentThreadID;

			string[] moduleNames = getModuleNames(module);
			int result = 0;
			foreach (string moduleName in moduleNames)
			{
				log.info(string.Format("Loading: sceUtilityLoadModule(module=0x{0:X4}) {1}", module, moduleName));
				int loadResult = hleUtilityLoadModule(module, moduleName);

				if (loadResult == SceKernelErrors.ERROR_MODULE_BAD_ID)
				{
					log.info(string.Format("IGNORING: sceUtilityLoadModule(module=0x{0:X4}) {1}", module, moduleName));
				}
				else
				{
					if (loadResult < 0)
					{
						result = loadResult;
					}
					log.info(string.Format("sceUtilityLoadModule(module=0x{0:X4}) {1} loaded", module, moduleName));
				}
			}

			if (result >= 0)
			{
				int newCurrentThreadID = Modules.ThreadManForUserModule.CurrentThreadID;
				// Do not delay the current thread if a context switching has already happened,
				// the thread is already delayed.
				if (currentThreadID == newCurrentThreadID)
				{
					Modules.ThreadManForUserModule.hleKernelDelayThread(currentThreadID, ModuleMgrForUser.loadHLEModuleDelay, false);
				}
			}

			return result;
		}

		[HLEFunction(nid : 0xE49BFE92, version : 303, checkInsideInterrupt : true)]
		public virtual int sceUtilityUnloadModule(int module)
		{
			string[] moduleNames = getModuleNames(module);
			int result = 0;
			foreach (string moduleName in moduleNames)
			{
				log.info(string.Format("sceUtilityUnloadModule(module=0x{0:X4}) {1} unloaded", module, moduleName));
				int unloadResult = hleUtilityUnloadModule(module);
				if (unloadResult < 0)
				{
					result = unloadResult;
				}
			}

			return result;
		}

		[HLEFunction(nid : 0xDA97F1AA, version : 500)]
		public virtual int sceUtilityStoreCheckoutInitStart(TPointer paramsAddr)
		{
			return storeCheckoutState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0x54A5C62F, version : 500)]
		public virtual int sceUtilityStoreCheckoutShutdownStart()
		{
			return storeCheckoutState.executeShutdownStart();
		}

		[HLEFunction(nid : 0xB8592D5F, version : 500)]
		public virtual int sceUtilityStoreCheckoutUpdate(int drawSpeed)
		{
			return storeCheckoutState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0x3AAD51DC, version : 500)]
		public virtual int sceUtilityStoreCheckoutGetStatus()
		{
			return storeCheckoutState.executeGetStatus();
		}

		[HLEFunction(nid : 0xA7BB7C67, version : 500)]
		public virtual int sceUtilityPsnInitStart(TPointer paramsAddr)
		{
			return psnState.executeInitStart(paramsAddr);
		}

		[HLEFunction(nid : 0xC130D441, version : 500)]
		public virtual int sceUtilityPsnShutdownStart()
		{
			return psnState.executeShutdownStart();
		}

		[HLEFunction(nid : 0x0940A1B9, version : 500)]
		public virtual int sceUtilityPsnUpdate(int drawSpeed)
		{
			return psnState.executeUpdate(drawSpeed);
		}

		[HLEFunction(nid : 0x094198B8, version : 500)]
		public virtual int sceUtilityPsnGetStatus()
		{
			return psnState.executeGetStatus();
		}

		[HLEFunction(nid : 0x180F7B62, version : 600)]
		public virtual int sceUtilityGamedataInstallAbort()
		{
			return gamedataInstallState.executeAbort();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xECE1D3E5, version = 150) public int sceUtility_ECE1D3E5_setAuthName(@StringInfo(maxLength = 64) pspsharp.HLE.PspString authName)
		[HLEFunction(nid : 0xECE1D3E5, version : 150)]
		public virtual int sceUtility_ECE1D3E5_setAuthName(PspString authName)
		{
			Modules.sceRegModule.AuthName = authName.String;

			return 0;
		}

		[HLEFunction(nid : 0x28D35634, version : 150)]
		public virtual int sceUtility_28D35634_getAuthName(TPointer authNameAddr)
		{
			string authName = Modules.sceRegModule.AuthName;

			authNameAddr.setStringNZ(64, authName);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceUtility_28D35634_getAuthName returning '{0}'", authName));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x70267ADF, version = 150) public int sceUtility_70267ADF_setAuthKey(@StringInfo(maxLength = 64) pspsharp.HLE.PspString authKey)
		[HLEFunction(nid : 0x70267ADF, version : 150)]
		public virtual int sceUtility_70267ADF_setAuthKey(PspString authKey)
		{
			Modules.sceRegModule.AuthKey = authKey.String;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEF3582B2, version = 150) public int sceUtility_EF3582B2_getAuthKey(pspsharp.HLE.TPointer authKeyAddr)
		[HLEFunction(nid : 0xEF3582B2, version : 150)]
		public virtual int sceUtility_EF3582B2_getAuthKey(TPointer authKeyAddr)
		{
			string authKey = Modules.sceRegModule.AuthKey;

			authKeyAddr.setStringNZ(64, authKey);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceUtility_EF3582B2_getAuthKey returning '{0}'", authKey));
			}

			return 0;
		}

		[HLEFunction(nid : 0x67C2105B, version : 150)]
		public virtual int sceUtilityGetNetParamInternal(int id, int param, TPointer data)
		{
			return sceUtilityGetNetParam(id, param, data);
		}

		[HLEFunction(nid : 0x6D77B975, version : 150)]
		public virtual int sceUtilitySetNetParamLatestID(int id)
		{
			Modules.sceRegModule.NetworkLatestId = id;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5DCBD3C0, version = 150) public int sceUtility_private_5DCBD3C0()
		[HLEFunction(nid : 0x5DCBD3C0, version : 150)]
		public virtual int sceUtility_private_5DCBD3C0()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x048BFC46, version = 150) public int sceUtility_private_048BFC46(pspsharp.HLE.PspString libraryName, int unknown1, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0x048BFC46, version : 150)]
		public virtual int sceUtility_private_048BFC46(PspString libraryName, int unknown1, TPointer optionAddr)
		{
			string path = utilityPrivateModules[libraryName.String];
			if (string.ReferenceEquals(path, null))
			{
				return -1;
			}

			SceKernelLMOption lmOption = null;
			if (optionAddr.NotNull)
			{
				lmOption = new SceKernelLMOption();
				lmOption.read(optionAddr);
				if (log.InfoEnabled)
				{
					log.info(string.Format("sceUtility_private_048BFC46 options: {0}", lmOption));
				}
			}

			LoadModuleContext loadModuleContext = new LoadModuleContext();
			loadModuleContext.fileName = path;
			loadModuleContext.lmOption = lmOption;
			loadModuleContext.allocMem = true;

			return Modules.ModuleMgrForUserModule.hleKernelLoadModule(loadModuleContext);
		}

		[HLEFunction(nid : 0x78A2FE0C, version : 150)]
		public virtual int sceUtility_private_78A2FE0C(int uid)
		{
			return Modules.ModuleMgrForUserModule.hleKernelUnloadModule(uid);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x031D0944, version = 150) public int sceUtility_private_031D0944(int unknown)
		[HLEFunction(nid : 0x031D0944, version : 150)]
		public virtual int sceUtility_private_031D0944(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB9D9C78F, version = 150) public int sceUtility_private_B9D9C78F(int unknown)
		[HLEFunction(nid : 0xB9D9C78F, version : 150)]
		public virtual int sceUtility_private_B9D9C78F(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x61D0686E, version = 150) public int sceUtility_netparam_internal_61D0686E(int unknown)
		[HLEFunction(nid : 0x61D0686E, version : 150)]
		public virtual int sceUtility_netparam_internal_61D0686E(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4CB183A4, version = 150) public int sceUtility_netparam_internal_4CB183A4(int unknown1, int unknown2)
		[HLEFunction(nid : 0x4CB183A4, version : 150)]
		public virtual int sceUtility_netparam_internal_4CB183A4(int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x239F260D, version = 150) public int sceUtility_netparam_internal_239F260D(int type, pspsharp.HLE.TPointer value)
		[HLEFunction(nid : 0x239F260D, version : 150)]
		public virtual int sceUtility_netparam_internal_239F260D(int type, TPointer value)
		{
			switch (type)
			{
				case 0:
					string configurationName = value.StringZ;
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceUtility_netparam_internal_239F260D configurationName='{0}'", configurationName));
					}
					break;
				case 1:
					string ssid = value.StringZ;
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceUtility_netparam_internal_239F260D SSID='{0}'", ssid));
					}
					break;
				case 2:
					int security = value.getValue32(); // One of PSP_NET_APCTL_INFO_SECURITY_TYPE_*
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceUtility_netparam_internal_239F260D security={0:D}", security));
					}
					break;
				case 3:
				case 4:
				case 8:
				case 13:
				case 17:
				case 21:
				case 23:
				case 24:
				case 25:
				case 26:
				case 27:
				case 29:
				case 30:
				case 31:
					// ?
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceUtility_netparam_internal_239F260D unknown value: {0}", Utilities.getMemoryDump(value.Address, 16)));
					}
					break;
				case 22:
					string wepKey = value.StringZ;
					if (log.DebugEnabled)
					{
						log.debug(string.Format("sceUtility_netparam_internal_239F260D wepKey='{0}'", wepKey));
					}
					break;
			}

			return 0;
		}
	}

}