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
namespace pspsharp.HLE
{

	//using Logger = org.apache.log4j.Logger;

	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using IVirtualFileSystem = pspsharp.HLE.VFS.IVirtualFileSystem;
	using Managers = pspsharp.HLE.kernel.Managers;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceIoStat = pspsharp.HLE.kernel.types.SceIoStat;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	/// <summary>
	/// Manager for the HLE modules.
	/// It defines which modules are loaded by default and
	/// which modules are loaded explicitly from flash0 or from a PRX.
	/// 
	/// @author fiveofhearts
	/// @author gid15
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLELogging public class HLEModuleManager
	public class HLEModuleManager
	{
		private static Logger log = Modules.log;
		private static HLEModuleManager instance;
		private const int STATE_VERSION = 0;

		public const int HLESyscallNid = -1;
		public const int InternalSyscallNid = -1;

		private bool modulesStarted = false;
		private bool startFromSyscall;
		private NIDMapper nidMapper;

		private Dictionary<string, IList<HLEModule>> flash0prxMap;
		private ISet<HLEModule> installedModules = new HashSet<HLEModule>();
		private IDictionary<int, HLEModuleFunction> syscallToFunction;
		private IDictionary<int, HLEModuleFunction> nidToFunction;
		private IDictionary<HLEModule, ModuleInfo> moduleInfos;

		private HLELogging defaultHLEFunctionLogging;

		/// <summary>
		/// List of PSP modules that can be loaded when they are available.
		/// They will then replace the HLE equivalent.
		/// </summary>
		private static readonly string[] moduleFileNamesToBeLoaded = new string[] {"flash0:/kd/utility.prx", "flash0:/kd/vshbridge.prx", "flash0:/vsh/module/paf.prx", "flash0:/vsh/module/common_gui.prx", "flash0:/vsh/module/common_util.prx"};

		private static readonly string[] moduleFilesNameVshOnly = new string[] {"flash0:/kd/vshbridge.prx", "flash0:/vsh/module/paf.prx", "flash0:/vsh/module/common_gui.prx", "flash0:/vsh/module/common_util.prx"};

		/// <summary>
		/// List of modules that can be loaded
		/// - by default in all firmwares (or only from a given FirmwareVersion)
		/// - by sceKernelLoadModule/sceUtilityLoadModule from the flash0 or from the UMD (.prx)
		/// </summary>
		private sealed class ModuleInfo
		{
			public static readonly ModuleInfo SysMemUserForUser = new ModuleInfo("SysMemUserForUser", InnerEnum.SysMemUserForUser, Modules.SysMemUserForUserModule);
			public static readonly ModuleInfo IoFileMgrForUser = new ModuleInfo("IoFileMgrForUser", InnerEnum.IoFileMgrForUser, Modules.IoFileMgrForUserModule);
			public static readonly ModuleInfo IoFileMgrForKernel = new ModuleInfo("IoFileMgrForKernel", InnerEnum.IoFileMgrForKernel, Modules.IoFileMgrForKernelModule);
			public const ModuleInfo pspsharp;
			public static readonly ModuleInfo ThreadManForKernel = new ModuleInfo("ThreadManForKernel", InnerEnum.ThreadManForKernel, Modules.ThreadManForKernelModule);
			public static readonly ModuleInfo SysMemForKernel = new ModuleInfo("SysMemForKernel", InnerEnum.SysMemForKernel, Modules.SysMemForKernelModule); // To be loaded after ThreadManForUser
			public static readonly ModuleInfo InterruptManager = new ModuleInfo("InterruptManager", InnerEnum.InterruptManager, Modules.InterruptManagerModule);
			public static readonly ModuleInfo LoadExecForUser = new ModuleInfo("LoadExecForUser", InnerEnum.LoadExecForUser, Modules.LoadExecForUserModule);
			public static readonly ModuleInfo LoadExecForKernel = new ModuleInfo("LoadExecForKernel", InnerEnum.LoadExecForKernel, Modules.LoadExecForKernelModule);
			public static readonly ModuleInfo StdioForUser = new ModuleInfo("StdioForUser", InnerEnum.StdioForUser, Modules.StdioForUserModule);
			public static readonly ModuleInfo StdioForKernel = new ModuleInfo("StdioForKernel", InnerEnum.StdioForKernel, Modules.StdioForKernelModule);
			public static readonly ModuleInfo sceUmdUser = new ModuleInfo("sceUmdUser", InnerEnum.sceUmdUser, Modules.sceUmdUserModule);
			public static readonly ModuleInfo scePower = new ModuleInfo("scePower", InnerEnum.scePower, Modules.scePowerModule);
			public static readonly ModuleInfo sceUtility = new ModuleInfo("sceUtility", InnerEnum.sceUtility, Modules.sceUtilityModule);
			public static readonly ModuleInfo UtilsForUser = new ModuleInfo("UtilsForUser", InnerEnum.UtilsForUser, Modules.UtilsForUserModule);
			public static readonly ModuleInfo sceDisplay = new ModuleInfo("sceDisplay", InnerEnum.sceDisplay, Modules.sceDisplayModule);
			public static readonly ModuleInfo sceGe_user = new ModuleInfo("sceGe_user", InnerEnum.sceGe_user, Modules.sceGe_userModule);
			public static readonly ModuleInfo sceRtc = new ModuleInfo("sceRtc", InnerEnum.sceRtc, Modules.sceRtcModule);
			public static readonly ModuleInfo KernelLibrary = new ModuleInfo("KernelLibrary", InnerEnum.KernelLibrary, Modules.Kernel_LibraryModule);
			public static readonly ModuleInfo ModuleMgrForUser = new ModuleInfo("ModuleMgrForUser", InnerEnum.ModuleMgrForUser, Modules.ModuleMgrForUserModule);
			public static readonly ModuleInfo LoadCoreForKernel = new ModuleInfo("LoadCoreForKernel", InnerEnum.LoadCoreForKernel, Modules.LoadCoreForKernelModule);
			public static readonly ModuleInfo sceCtrl = new ModuleInfo("sceCtrl", InnerEnum.sceCtrl, Modules.sceCtrlModule);
			public static readonly ModuleInfo sceAudio = new ModuleInfo("sceAudio", InnerEnum.sceAudio, Modules.sceAudioModule);
			public static readonly ModuleInfo sceImpose = new ModuleInfo("sceImpose", InnerEnum.sceImpose, Modules.sceImposeModule);
			public static readonly ModuleInfo sceSuspendForUser = new ModuleInfo("sceSuspendForUser", InnerEnum.sceSuspendForUser, Modules.sceSuspendForUserModule);
			public static readonly ModuleInfo sceSuspendForKernel = new ModuleInfo("sceSuspendForKernel", InnerEnum.sceSuspendForKernel, Modules.sceSuspendForKernelModule);
			public static readonly ModuleInfo sceDmac = new ModuleInfo("sceDmac", InnerEnum.sceDmac, Modules.sceDmacModule);
			public static readonly ModuleInfo sceHprm = new ModuleInfo("sceHprm", InnerEnum.sceHprm, Modules.sceHprmModule); // check if loaded by default
			public static readonly ModuleInfo sceAtrac3plus = new ModuleInfo("sceAtrac3plus", InnerEnum.sceAtrac3plus, Modules.sceAtrac3plusModule, new string[] {"libatrac3plus", "PSP_AV_MODULE_ATRAC3PLUS", "PSP_MODULE_AV_ATRAC3PLUS", "sceATRAC3plus_Library"}, "flash0:/kd/libatrac3plus.prx");
			public static readonly ModuleInfo sceSasCore = new ModuleInfo("sceSasCore", InnerEnum.sceSasCore, Modules.sceSasCoreModule, new string[] {"sc_sascore", "PSP_AV_MODULE_SASCORE", "PSP_MODULE_AV_SASCORE", "sceSAScore"}, "flash0:/kd/sc_sascore.prx");
			public static readonly ModuleInfo sceMpeg = new ModuleInfo("sceMpeg", InnerEnum.sceMpeg, Modules.sceMpegModule, new string[] {"mpeg", "PSP_AV_MODULE_MPEGBASE", "PSP_MODULE_AV_MPEGBASE", "sceMpeg_library"}, "flash0:/kd/mpeg.prx");
			public static readonly ModuleInfo sceMpegVsh = new ModuleInfo("sceMpegVsh", InnerEnum.sceMpegVsh, Modules.sceMpegModule, new string[] {"mpeg_vsh", "mpeg_vsh370"}, "flash0:/kd/mpeg_vsh.prx");
			public static readonly ModuleInfo sceMpegbase = new ModuleInfo("sceMpegbase", InnerEnum.sceMpegbase, Modules.sceMpegbaseModule, new string[] {"PSP_AV_MODULE_AVCODEC", "PSP_MODULE_AV_AVCODEC", "avcodec", "sceMpegbase_Driver"}, "flash0:/kd/avcodec.prx");
			public static readonly ModuleInfo sceFont = new ModuleInfo("sceFont", InnerEnum.sceFont, Modules.sceFontModule, new string[] {"libfont", "sceFont_Library"});
			public static readonly ModuleInfo scePsmfPlayer = new ModuleInfo("scePsmfPlayer", InnerEnum.scePsmfPlayer, Modules.scePsmfPlayerModule, new string[] {"libpsmfplayer", "psmf_jk", "scePsmfP_library"});
			public static readonly ModuleInfo scePsmf = new ModuleInfo("scePsmf", InnerEnum.scePsmf, Modules.scePsmfModule, new string[] {"psmf", "scePsmf_library"});
			public static readonly ModuleInfo sceMp3 = new ModuleInfo("sceMp3", InnerEnum.sceMp3, Modules.sceMp3Module, new string[] {"PSP_AV_MODULE_MP3", "PSP_MODULE_AV_MP3", "LIBMP3"});
			public static readonly ModuleInfo sceDeflt = new ModuleInfo("sceDeflt", InnerEnum.sceDeflt, Modules.sceDefltModule, new string[] {"libdeflt"});
			public static readonly ModuleInfo sceWlan = new ModuleInfo("sceWlan", InnerEnum.sceWlan, Modules.sceWlanModule);
			public static readonly ModuleInfo sceNet = new ModuleInfo("sceNet", InnerEnum.sceNet, Modules.sceNetModule, new string[] {"pspnet", "PSP_NET_MODULE_COMMON", "PSP_MODULE_NET_COMMON"}, "flash0:/kd/pspnet.prx");
			public static readonly ModuleInfo sceNetAdhoc = new ModuleInfo("sceNetAdhoc", InnerEnum.sceNetAdhoc, Modules.sceNetAdhocModule, new string[] {"pspnet_adhoc", "PSP_NET_MODULE_ADHOC", "PSP_MODULE_NET_ADHOC"}, "flash0:/kd/pspnet_adhoc.prx");
			public static readonly ModuleInfo sceNetAdhocctl = new ModuleInfo("sceNetAdhocctl", InnerEnum.sceNetAdhocctl, Modules.sceNetAdhocctlModule, new string[] {"pspnet_adhocctl", "PSP_NET_MODULE_ADHOC", "PSP_MODULE_NET_ADHOC"}, "flash0:/kd/pspnet_adhocctl.prx");
			public static readonly ModuleInfo sceNetAdhocDiscover = new ModuleInfo("sceNetAdhocDiscover", InnerEnum.sceNetAdhocDiscover, Modules.sceNetAdhocDiscoverModule, new string[] {"pspnet_adhoc_discover", "PSP_NET_MODULE_ADHOC", "PSP_MODULE_NET_ADHOC"}, "flash0:/kd/pspnet_adhoc_discover.prx");
			public static readonly ModuleInfo sceNetAdhocMatching = new ModuleInfo("sceNetAdhocMatching", InnerEnum.sceNetAdhocMatching, Modules.sceNetAdhocMatchingModule, new string[] {"pspnet_adhoc_matching", "PSP_NET_MODULE_ADHOC", "PSP_MODULE_NET_ADHOC"}, "flash0:/kd/pspnet_adhoc_matching.prx");
			public static readonly ModuleInfo sceNetAdhocTransInt = new ModuleInfo("sceNetAdhocTransInt", InnerEnum.sceNetAdhocTransInt, Modules.sceNetAdhocTransIntModule, new string[] {"pspnet_adhoc_transfer_int"}, "flash0:/kd/pspnet_adhoc_transfer_int.prx");
			public static readonly ModuleInfo sceNetAdhocAuth = new ModuleInfo("sceNetAdhocAuth", InnerEnum.sceNetAdhocAuth, Modules.sceNetAdhocAuthModule, new string[] {"pspnet_adhoc_auth", "sceNetAdhocAuth_Service"}, "flash0:/kd/pspnet_adhoc_auth.prx");
			public static readonly ModuleInfo sceNetAdhocDownload = new ModuleInfo("sceNetAdhocDownload", InnerEnum.sceNetAdhocDownload, Modules.sceNetAdhocDownloadModule, new string[] {"pspnet_adhoc_download"}, "flash0:/kd/pspnet_adhoc_download.prx");
			public static readonly ModuleInfo sceNetIfhandle = new ModuleInfo("sceNetIfhandle", InnerEnum.sceNetIfhandle, Modules.sceNetIfhandleModule, new string[] {"ifhandle", "PSP_NET_MODULE_COMMON", "PSP_MODULE_NET_COMMON", "sceNetIfhandle_Service"}, "flash0:/kd/ifhandle.prx");
			public static readonly ModuleInfo sceNetApctl = new ModuleInfo("sceNetApctl", InnerEnum.sceNetApctl, Modules.sceNetApctlModule, new string[] {"pspnet_apctl", "PSP_NET_MODULE_COMMON", "PSP_MODULE_NET_COMMON"}, "flash0:/kd/pspnet_apctl.prx");
			public static readonly ModuleInfo sceNetInet = new ModuleInfo("sceNetInet", InnerEnum.sceNetInet, Modules.sceNetInetModule, new string[] {"pspnet_inet", "PSP_NET_MODULE_INET", "PSP_MODULE_NET_INET"}, "flash0:/kd/pspnet_inet.prx");
			public static readonly ModuleInfo sceNetResolver = new ModuleInfo("sceNetResolver", InnerEnum.sceNetResolver, Modules.sceNetResolverModule, new string[] {"pspnet_resolver", "PSP_NET_MODULE_COMMON", "PSP_MODULE_NET_COMMON"}, "flash0:/kd/pspnet_resolver.prx");
			public static readonly ModuleInfo sceNetUpnp = new ModuleInfo("sceNetUpnp", InnerEnum.sceNetUpnp, Modules.sceNetUpnpModule, new string[] {"pspnet_upnp", "PSP_MODULE_NET_UPNP"}, "flash0:/kd/pspnet_upnp.prx");
			public static readonly ModuleInfo sceOpenPSID = new ModuleInfo("sceOpenPSID", InnerEnum.sceOpenPSID, Modules.sceOpenPSIDModule);
			public static readonly ModuleInfo sceNp = new ModuleInfo("sceNp", InnerEnum.sceNp, Modules.sceNpModule, new string[] {"np", "PSP_MODULE_NP_COMMON"}, "flash0:/kd/np.prx");
			public static readonly ModuleInfo sceNpCore = new ModuleInfo("sceNpCore", InnerEnum.sceNpCore, Modules.sceNpCoreModule, new string[] {"np_core"}, "flash0:/kd/np_core.prx");
			public static readonly ModuleInfo sceNpAuth = new ModuleInfo("sceNpAuth", InnerEnum.sceNpAuth, Modules.sceNpAuthModule, new string[] {"np_auth", "PSP_MODULE_NP_COMMON"}, "flash0:/kd/np_auth.prx");
			public static readonly ModuleInfo sceNpService = new ModuleInfo("sceNpService", InnerEnum.sceNpService, Modules.sceNpServiceModule, new string[] {"np_service", "PSP_MODULE_NP_SERVICE"}, "flash0:/kd/np_service.prx");
			public static readonly ModuleInfo sceNpCommerce2 = new ModuleInfo("sceNpCommerce2", InnerEnum.sceNpCommerce2, Modules.sceNpCommerce2Module, new string[] {"np_commerce2", "PSP_MODULE_NP_COMMERCE2"}, "flash0:/kd/np_commerce2.prx");
			public static readonly ModuleInfo sceNpCommerce2Store = new ModuleInfo("sceNpCommerce2Store", InnerEnum.sceNpCommerce2Store, Modules.sceNpCommerce2StoreModule, new string[] {"np_commerce2_store"}, "flash0:/kd/np_commerce2_store.prx");
			public static readonly ModuleInfo sceNpCommerce2RegCam = new ModuleInfo("sceNpCommerce2RegCam", InnerEnum.sceNpCommerce2RegCam, Modules.sceNpCommerce2RegCamModule, new string[] {"np_commerce2_regcam"}, "flash0:/kd/np_commerce2_regcam.prx");
			public static readonly ModuleInfo sceNpMatching2 = new ModuleInfo("sceNpMatching2", InnerEnum.sceNpMatching2, Modules.sceNpMatching2Module, new string[] {"np_matching2", "PSP_MODULE_NP_MATCHING2"}, "flash0:/kd/np_matching2.prx");
			public static readonly ModuleInfo sceNpInstall = new ModuleInfo("sceNpInstall", InnerEnum.sceNpInstall, Modules.sceNpInstallModule, new string[] {"np_inst"}, "flash0:/kd/np_inst.prx");
			public static readonly ModuleInfo sceNpCamp = new ModuleInfo("sceNpCamp", InnerEnum.sceNpCamp, Modules.sceNpCampModule, new string[] {"np_campaign"}, "flash0:/kd/np_campaign.prx");
			public static readonly ModuleInfo scePspNpDrm_user = new ModuleInfo("scePspNpDrm_user", InnerEnum.scePspNpDrm_user, Modules.scePspNpDrm_userModule, new string[] {"PSP_MODULE_NP_DRM", "npdrm"});
			public static readonly ModuleInfo sceVaudio = new ModuleInfo("sceVaudio", InnerEnum.sceVaudio, Modules.sceVaudioModule, new string[] {"PSP_AV_MODULE_VAUDIO", "PSP_MODULE_AV_VAUDIO"});
			public static readonly ModuleInfo sceMp4 = new ModuleInfo("sceMp4", InnerEnum.sceMp4, Modules.sceMp4Module, new string[] {"PSP_MODULE_AV_MP4", "libmp4"}, "flash0:/kd/libmp4.prx");
			public static readonly ModuleInfo mp4msv = new ModuleInfo("mp4msv", InnerEnum.mp4msv, Modules.mp4msvModule, new string[] {"mp4msv"}, "flash0:/kd/mp4msv.prx");
			public static readonly ModuleInfo sceHttp = new ModuleInfo("sceHttp", InnerEnum.sceHttp, Modules.sceHttpModule, new string[] {"libhttp", "libhttp_rfc", "PSP_NET_MODULE_HTTP", "PSP_MODULE_NET_HTTP"}, "flash0:/kd/libhttp.prx");
			public static readonly ModuleInfo sceHttps = new ModuleInfo("sceHttps", InnerEnum.sceHttps, Modules.sceHttpsModule, new string[] {"libhttp", "libhttp_rfc", "PSP_NET_MODULE_HTTP", "PSP_MODULE_NET_HTTP"}, "flash0:/kd/libhttp.prx");
			public static readonly ModuleInfo sceHttpStorage = new ModuleInfo("sceHttpStorage", InnerEnum.sceHttpStorage, Modules.sceHttpStorageModule, new string[] {"http_storage"}, "flash0:/kd/http_storage.prx");
			public static readonly ModuleInfo sceSsl = new ModuleInfo("sceSsl", InnerEnum.sceSsl, Modules.sceSslModule, new string[] {"libssl", "PSP_NET_MODULE_SSL", "PSP_MODULE_NET_SSL"}, "flash0:/kd/libssl.prx");
			public static readonly ModuleInfo sceP3da = new ModuleInfo("sceP3da", InnerEnum.sceP3da, Modules.sceP3daModule);
			public static readonly ModuleInfo sceGameUpdate = new ModuleInfo("sceGameUpdate", InnerEnum.sceGameUpdate, Modules.sceGameUpdateModule, new string[] {"libgameupdate"});
			public static readonly ModuleInfo sceUsbCam = new ModuleInfo("sceUsbCam", InnerEnum.sceUsbCam, Modules.sceUsbCamModule, new string[] {"PSP_USB_MODULE_CAM", "PSP_MODULE_USB_CAM", "usbcam"});
			public static readonly ModuleInfo sceJpeg = new ModuleInfo("sceJpeg", InnerEnum.sceJpeg, Modules.sceJpegModule, new string[] {"PSP_AV_MODULE_AVCODEC", "PSP_MODULE_AV_AVCODEC"}, "flash0:/kd/avcodec.prx");
			public static readonly ModuleInfo sceUsb = new ModuleInfo("sceUsb", InnerEnum.sceUsb, Modules.sceUsbModule);
			public static readonly ModuleInfo sceHeap = new ModuleInfo("sceHeap", InnerEnum.sceHeap, Modules.sceHeapModule, new string[] {"libheap"});
			public static readonly ModuleInfo KDebugForKernel = new ModuleInfo("KDebugForKernel", InnerEnum.KDebugForKernel, Modules.KDebugForKernelModule);
			public static readonly ModuleInfo sceCcc = new ModuleInfo("sceCcc", InnerEnum.sceCcc, Modules.sceCccModule, new string[] {"libccc"});
			public static readonly ModuleInfo scePauth = new ModuleInfo("scePauth", InnerEnum.scePauth, Modules.scePauthModule);
			public static readonly ModuleInfo sceSfmt19937 = new ModuleInfo("sceSfmt19937", InnerEnum.sceSfmt19937, Modules.sceSfmt19937Module);
			public static readonly ModuleInfo sceMd5 = new ModuleInfo("sceMd5", InnerEnum.sceMd5, Modules.sceMd5Module, new string[] {"libmd5"});
			public static readonly ModuleInfo sceParseUri = new ModuleInfo("sceParseUri", InnerEnum.sceParseUri, Modules.sceParseUriModule, new string[] {"libparse_uri", "libhttp_rfc", "PSP_NET_MODULE_HTTP", "PSP_MODULE_NET_HTTP", "PSP_MODULE_NET_PARSEURI"}, "flash0:/kd/libparse_uri.prx");
			public static readonly ModuleInfo sceUsbAcc = new ModuleInfo("sceUsbAcc", InnerEnum.sceUsbAcc, Modules.sceUsbAccModule, new string[] {"PSP_USB_MODULE_ACC", "USBAccBaseDriver"});
			public static readonly ModuleInfo sceMt19937 = new ModuleInfo("sceMt19937", InnerEnum.sceMt19937, Modules.sceMt19937Module, new string[] {"libmt19937"});
			public static readonly ModuleInfo sceAac = new ModuleInfo("sceAac", InnerEnum.sceAac, Modules.sceAacModule, new string[] {"libaac", "PSP_AV_MODULE_AAC", "PSP_MODULE_AV_AAC"});
			public static readonly ModuleInfo sceFpu = new ModuleInfo("sceFpu", InnerEnum.sceFpu, Modules.sceFpuModule, new string[] {"libfpu"});
			public static readonly ModuleInfo sceUsbMic = new ModuleInfo("sceUsbMic", InnerEnum.sceUsbMic, Modules.sceUsbMicModule, new string[] {"usbmic", "PSP_USB_MODULE_MIC", "PSP_MODULE_USB_MIC", "USBCamMicDriver"});
			public static readonly ModuleInfo sceAudioRouting = new ModuleInfo("sceAudioRouting", InnerEnum.sceAudioRouting, Modules.sceAudioRoutingModule);
			public static readonly ModuleInfo sceUsbGps = new ModuleInfo("sceUsbGps", InnerEnum.sceUsbGps, Modules.sceUsbGpsModule, new string[] {"PSP_USB_MODULE_GPS", "PSP_MODULE_USB_GPS", "usbgps"});
			public static readonly ModuleInfo sceAudiocodec = new ModuleInfo("sceAudiocodec", InnerEnum.sceAudiocodec, Modules.sceAudiocodecModule, new string[] {"PSP_AV_MODULE_AVCODEC", "PSP_MODULE_AV_AVCODEC", "avcodec", "sceAudiocodec_Driver"}, "flash0:/kd/avcodec.prx");
			public static readonly ModuleInfo sceVideocodec = new ModuleInfo("sceVideocodec", InnerEnum.sceVideocodec, Modules.sceVideocodecModule, new string[] {"PSP_AV_MODULE_AVCODEC", "PSP_MODULE_AV_AVCODEC", "avcodec", "sceVideocodec_Driver"}, "flash0:/kd/avcodec.prx");
			public static readonly ModuleInfo sceAdler = new ModuleInfo("sceAdler", InnerEnum.sceAdler, Modules.sceAdlerModule, new string[] {"libadler"});
			public static readonly ModuleInfo sceSha1 = new ModuleInfo("sceSha1", InnerEnum.sceSha1, Modules.sceSha1Module, new string[] {"libsha1"});
			public static readonly ModuleInfo sceSha256 = new ModuleInfo("sceSha256", InnerEnum.sceSha256, Modules.sceSha256Module, new string[] {"libsha256"});
			public static readonly ModuleInfo sceMeCore = new ModuleInfo("sceMeCore", InnerEnum.sceMeCore, Modules.sceMeCoreModule);
			public static readonly ModuleInfo sceMeBoot = new ModuleInfo("sceMeBoot", InnerEnum.sceMeBoot, Modules.sceMeBootModule);
			public static readonly ModuleInfo KUBridge = new ModuleInfo("KUBridge", InnerEnum.KUBridge, Modules.KUBridgeModule);
			public static readonly ModuleInfo SysclibForKernel = new ModuleInfo("SysclibForKernel", InnerEnum.SysclibForKernel, Modules.SysclibForKernelModule);
			public static readonly ModuleInfo semaphore = new ModuleInfo("semaphore", InnerEnum.semaphore, Modules.semaphoreModule);
			public static readonly ModuleInfo ModuleMgrForKernel = new ModuleInfo("ModuleMgrForKernel", InnerEnum.ModuleMgrForKernel, Modules.ModuleMgrForKernelModule);
			public static readonly ModuleInfo sceReg = new ModuleInfo("sceReg", InnerEnum.sceReg, Modules.sceRegModule);
			public static readonly ModuleInfo sceDve = new ModuleInfo("sceDve", InnerEnum.sceDve, Modules.sceDveModule);
			public static readonly ModuleInfo sceSysEventForKernel = new ModuleInfo("sceSysEventForKernel", InnerEnum.sceSysEventForKernel, Modules.sceSysEventForKernelModule);
			public static readonly ModuleInfo sceChkreg = new ModuleInfo("sceChkreg", InnerEnum.sceChkreg, Modules.sceChkregModule);
			public static readonly ModuleInfo sceMsAudio_Service = new ModuleInfo("sceMsAudio_Service", InnerEnum.sceMsAudio_Service, Modules.sceMsAudio_ServiceModule);
			public static readonly ModuleInfo sceMePower = new ModuleInfo("sceMePower", InnerEnum.sceMePower, Modules.sceMePowerModule);
			public static readonly ModuleInfo sceResmgr = new ModuleInfo("sceResmgr", InnerEnum.sceResmgr, Modules.sceResmgrModule);
			public static readonly ModuleInfo UtilsForKernel = new ModuleInfo("UtilsForKernel", InnerEnum.UtilsForKernel, Modules.UtilsForKernelModule);
			public static readonly ModuleInfo sceLibUpdateDL = new ModuleInfo("sceLibUpdateDL", InnerEnum.sceLibUpdateDL, Modules.sceLibUpdateDLModule, new string[] {"libupdown"});
			public static readonly ModuleInfo sceParseHttp = new ModuleInfo("sceParseHttp", InnerEnum.sceParseHttp, Modules.sceParseHttpModule, new string[] {"libparse_http", "PSP_MODULE_NET_PARSEHTTP"}, "flash0:/kd/libparse_http.prx");
			public static readonly ModuleInfo sceMgr_driver = new ModuleInfo("sceMgr_driver", InnerEnum.sceMgr_driver, Modules.sceMgr_driverModule);
			public static readonly ModuleInfo sceChnnlsv = new ModuleInfo("sceChnnlsv", InnerEnum.sceChnnlsv, Modules.sceChnnlsvModule, new string[] {"chnnlsv"});
			public static readonly ModuleInfo sceUsbstor = new ModuleInfo("sceUsbstor", InnerEnum.sceUsbstor, Modules.sceUsbstorModule);
			public static readonly ModuleInfo sceIdStorage = new ModuleInfo("sceIdStorage", InnerEnum.sceIdStorage, Modules.sceIdStorageModule);
			public static readonly ModuleInfo sceCertLoader = new ModuleInfo("sceCertLoader", InnerEnum.sceCertLoader, Modules.sceCertLoaderModule, new string[] {"cert_loader", "PSP_MODULE_NET_SSL"}, "flash0:/kd/cert_loader.prx");
			public static readonly ModuleInfo sceDNAS = new ModuleInfo("sceDNAS", InnerEnum.sceDNAS, Modules.sceDNASModule, new string[] {"libdnas"});
			public static readonly ModuleInfo sceDNASCore = new ModuleInfo("sceDNASCore", InnerEnum.sceDNASCore, Modules.sceDNASCoreModule, new string[] {"libdnas_core"});
			public static readonly ModuleInfo sceMcctrl = new ModuleInfo("sceMcctrl", InnerEnum.sceMcctrl, Modules.sceMcctrlModule, new string[] {"mcctrl"});
			public static readonly ModuleInfo sceNetStun = new ModuleInfo("sceNetStun", InnerEnum.sceNetStun, Modules.sceNetStunModule);
			public static readonly ModuleInfo sceMeMemory = new ModuleInfo("sceMeMemory", InnerEnum.sceMeMemory, Modules.sceMeMemoryModule);
			public static readonly ModuleInfo sceMeVideo = new ModuleInfo("sceMeVideo", InnerEnum.sceMeVideo, Modules.sceMeVideoModule);
			public static readonly ModuleInfo sceMeAudio = new ModuleInfo("sceMeAudio", InnerEnum.sceMeAudio, Modules.sceMeAudioModule);
			public static readonly ModuleInfo InitForKernel = new ModuleInfo("InitForKernel", InnerEnum.InitForKernel, Modules.InitForKernelModule);
			public static readonly ModuleInfo sceMemab = new ModuleInfo("sceMemab", InnerEnum.sceMemab, Modules.sceMemabModule, new string[] {"memab", "sceMemab"});
			public static readonly ModuleInfo DmacManForKernel = new ModuleInfo("DmacManForKernel", InnerEnum.DmacManForKernel, Modules.DmacManForKernelModule);
			public static readonly ModuleInfo sceSyscon = new ModuleInfo("sceSyscon", InnerEnum.sceSyscon, Modules.sceSysconModule);
			public static readonly ModuleInfo sceLed = new ModuleInfo("sceLed", InnerEnum.sceLed, Modules.sceLedModule);
			public static readonly ModuleInfo sceSysreg = new ModuleInfo("sceSysreg", InnerEnum.sceSysreg, Modules.sceSysregModule);
			public static readonly ModuleInfo scePsheet = new ModuleInfo("scePsheet", InnerEnum.scePsheet, Modules.scePsheetModule);
			public static readonly ModuleInfo sceUmdMan = new ModuleInfo("sceUmdMan", InnerEnum.sceUmdMan, Modules.sceUmdManModule);
			public static readonly ModuleInfo sceCodepage = new ModuleInfo("sceCodepage", InnerEnum.sceCodepage, Modules.sceCodepageModule);
			public static readonly ModuleInfo sceMSstor = new ModuleInfo("sceMSstor", InnerEnum.sceMSstor, Modules.sceMSstorModule);
			public static readonly ModuleInfo sceAta = new ModuleInfo("sceAta", InnerEnum.sceAta, Modules.sceAtaModule);
			public static readonly ModuleInfo sceGpio = new ModuleInfo("sceGpio", InnerEnum.sceGpio, Modules.sceGpioModule);
			public static readonly ModuleInfo sceNand = new ModuleInfo("sceNand", InnerEnum.sceNand, Modules.sceNandModule);
			public static readonly ModuleInfo sceBSMan = new ModuleInfo("sceBSMan", InnerEnum.sceBSMan, Modules.sceBSManModule);
			public static readonly ModuleInfo memlmd = new ModuleInfo("memlmd", InnerEnum.memlmd, Modules.memlmdModule);
			public static readonly ModuleInfo reboot = new ModuleInfo("reboot", InnerEnum.reboot, Modules.rebootModule);
			public static readonly ModuleInfo sceI2c = new ModuleInfo("sceI2c", InnerEnum.sceI2c, Modules.sceI2cModule);
			public static readonly ModuleInfo scePwm = new ModuleInfo("scePwm", InnerEnum.scePwm, Modules.scePwmModule);
			public static readonly ModuleInfo sceLcdc = new ModuleInfo("sceLcdc", InnerEnum.sceLcdc, Modules.sceLcdcModule);
			public static readonly ModuleInfo sceDmacplus = new ModuleInfo("sceDmacplus", InnerEnum.sceDmacplus, Modules.sceDmacplusModule);
			public static readonly ModuleInfo sceDdr = new ModuleInfo("sceDdr", InnerEnum.sceDdr, Modules.sceDdrModule);
			public static readonly ModuleInfo sceMScm = new ModuleInfo("sceMScm", InnerEnum.sceMScm, Modules.sceMScmModule);
			public static readonly ModuleInfo sceG729 = new ModuleInfo("sceG729", InnerEnum.sceG729, Modules.sceG729Module, new string[] {"PSP_MODULE_AV_G729", "g729"}, "flash0:/kd/g729.prx");
			public static readonly ModuleInfo scePopsMan = new ModuleInfo("scePopsMan", InnerEnum.scePopsMan, Modules.scePopsManModule);
			public static readonly ModuleInfo scePaf = new ModuleInfo("scePaf", InnerEnum.scePaf, Modules.scePafModule);

			private static readonly IList<ModuleInfo> valueList = new List<ModuleInfo>();

			static ModuleInfo()
			{
				valueList.Add(SysMemUserForUser);
				valueList.Add(IoFileMgrForUser);
				valueList.Add(IoFileMgrForKernel);
				valueList.Add(pspsharp.HLE.modules.ThreadManForUser);
				valueList.Add(ThreadManForKernel);
				valueList.Add(SysMemForKernel);
				valueList.Add(InterruptManager);
				valueList.Add(LoadExecForUser);
				valueList.Add(LoadExecForKernel);
				valueList.Add(StdioForUser);
				valueList.Add(StdioForKernel);
				valueList.Add(sceUmdUser);
				valueList.Add(scePower);
				valueList.Add(sceUtility);
				valueList.Add(UtilsForUser);
				valueList.Add(sceDisplay);
				valueList.Add(sceGe_user);
				valueList.Add(sceRtc);
				valueList.Add(KernelLibrary);
				valueList.Add(ModuleMgrForUser);
				valueList.Add(LoadCoreForKernel);
				valueList.Add(sceCtrl);
				valueList.Add(sceAudio);
				valueList.Add(sceImpose);
				valueList.Add(sceSuspendForUser);
				valueList.Add(sceSuspendForKernel);
				valueList.Add(sceDmac);
				valueList.Add(sceHprm);
				valueList.Add(sceAtrac3plus);
				valueList.Add(sceSasCore);
				valueList.Add(sceMpeg);
				valueList.Add(sceMpegVsh);
				valueList.Add(sceMpegbase);
				valueList.Add(sceFont);
				valueList.Add(scePsmfPlayer);
				valueList.Add(scePsmf);
				valueList.Add(sceMp3);
				valueList.Add(sceDeflt);
				valueList.Add(sceWlan);
				valueList.Add(sceNet);
				valueList.Add(sceNetAdhoc);
				valueList.Add(sceNetAdhocctl);
				valueList.Add(sceNetAdhocDiscover);
				valueList.Add(sceNetAdhocMatching);
				valueList.Add(sceNetAdhocTransInt);
				valueList.Add(sceNetAdhocAuth);
				valueList.Add(sceNetAdhocDownload);
				valueList.Add(sceNetIfhandle);
				valueList.Add(sceNetApctl);
				valueList.Add(sceNetInet);
				valueList.Add(sceNetResolver);
				valueList.Add(sceNetUpnp);
				valueList.Add(sceOpenPSID);
				valueList.Add(sceNp);
				valueList.Add(sceNpCore);
				valueList.Add(sceNpAuth);
				valueList.Add(sceNpService);
				valueList.Add(sceNpCommerce2);
				valueList.Add(sceNpCommerce2Store);
				valueList.Add(sceNpCommerce2RegCam);
				valueList.Add(sceNpMatching2);
				valueList.Add(sceNpInstall);
				valueList.Add(sceNpCamp);
				valueList.Add(scePspNpDrm_user);
				valueList.Add(sceVaudio);
				valueList.Add(sceMp4);
				valueList.Add(mp4msv);
				valueList.Add(sceHttp);
				valueList.Add(sceHttps);
				valueList.Add(sceHttpStorage);
				valueList.Add(sceSsl);
				valueList.Add(sceP3da);
				valueList.Add(sceGameUpdate);
				valueList.Add(sceUsbCam);
				valueList.Add(sceJpeg);
				valueList.Add(sceUsb);
				valueList.Add(sceHeap);
				valueList.Add(KDebugForKernel);
				valueList.Add(sceCcc);
				valueList.Add(scePauth);
				valueList.Add(sceSfmt19937);
				valueList.Add(sceMd5);
				valueList.Add(sceParseUri);
				valueList.Add(sceUsbAcc);
				valueList.Add(sceMt19937);
				valueList.Add(sceAac);
				valueList.Add(sceFpu);
				valueList.Add(sceUsbMic);
				valueList.Add(sceAudioRouting);
				valueList.Add(sceUsbGps);
				valueList.Add(sceAudiocodec);
				valueList.Add(sceVideocodec);
				valueList.Add(sceAdler);
				valueList.Add(sceSha1);
				valueList.Add(sceSha256);
				valueList.Add(sceMeCore);
				valueList.Add(sceMeBoot);
				valueList.Add(KUBridge);
				valueList.Add(SysclibForKernel);
				valueList.Add(semaphore);
				valueList.Add(ModuleMgrForKernel);
				valueList.Add(sceReg);
				valueList.Add(sceDve);
				valueList.Add(sceSysEventForKernel);
				valueList.Add(sceChkreg);
				valueList.Add(sceMsAudio_Service);
				valueList.Add(sceMePower);
				valueList.Add(sceResmgr);
				valueList.Add(UtilsForKernel);
				valueList.Add(sceLibUpdateDL);
				valueList.Add(sceParseHttp);
				valueList.Add(sceMgr_driver);
				valueList.Add(sceChnnlsv);
				valueList.Add(sceUsbstor);
				valueList.Add(sceIdStorage);
				valueList.Add(sceCertLoader);
				valueList.Add(sceDNAS);
				valueList.Add(sceDNASCore);
				valueList.Add(sceMcctrl);
				valueList.Add(sceNetStun);
				valueList.Add(sceMeMemory);
				valueList.Add(sceMeVideo);
				valueList.Add(sceMeAudio);
				valueList.Add(InitForKernel);
				valueList.Add(sceMemab);
				valueList.Add(DmacManForKernel);
				valueList.Add(sceSyscon);
				valueList.Add(sceLed);
				valueList.Add(sceSysreg);
				valueList.Add(scePsheet);
				valueList.Add(sceUmdMan);
				valueList.Add(sceCodepage);
				valueList.Add(sceMSstor);
				valueList.Add(sceAta);
				valueList.Add(sceGpio);
				valueList.Add(sceNand);
				valueList.Add(sceBSMan);
				valueList.Add(memlmd);
				valueList.Add(reboot);
				valueList.Add(sceI2c);
				valueList.Add(scePwm);
				valueList.Add(sceLcdc);
				valueList.Add(sceDmacplus);
				valueList.Add(sceDdr);
				valueList.Add(sceMScm);
				valueList.Add(sceG729);
				valueList.Add(scePopsMan);
				valueList.Add(scePaf);
			}

			public enum InnerEnum
			{
				SysMemUserForUser,
				IoFileMgrForUser,
				IoFileMgrForKernel,
				pspsharp.HLE.modules.ThreadManForUser,
				ThreadManForKernel,
				SysMemForKernel,
				InterruptManager,
				LoadExecForUser,
				LoadExecForKernel,
				StdioForUser,
				StdioForKernel,
				sceUmdUser,
				scePower,
				sceUtility,
				UtilsForUser,
				sceDisplay,
				sceGe_user,
				sceRtc,
				KernelLibrary,
				ModuleMgrForUser,
				LoadCoreForKernel,
				sceCtrl,
				sceAudio,
				sceImpose,
				sceSuspendForUser,
				sceSuspendForKernel,
				sceDmac,
				sceHprm,
				sceAtrac3plus,
				sceSasCore,
				sceMpeg,
				sceMpegVsh,
				sceMpegbase,
				sceFont,
				scePsmfPlayer,
				scePsmf,
				sceMp3,
				sceDeflt,
				sceWlan,
				sceNet,
				sceNetAdhoc,
				sceNetAdhocctl,
				sceNetAdhocDiscover,
				sceNetAdhocMatching,
				sceNetAdhocTransInt,
				sceNetAdhocAuth,
				sceNetAdhocDownload,
				sceNetIfhandle,
				sceNetApctl,
				sceNetInet,
				sceNetResolver,
				sceNetUpnp,
				sceOpenPSID,
				sceNp,
				sceNpCore,
				sceNpAuth,
				sceNpService,
				sceNpCommerce2,
				sceNpCommerce2Store,
				sceNpCommerce2RegCam,
				sceNpMatching2,
				sceNpInstall,
				sceNpCamp,
				scePspNpDrm_user,
				sceVaudio,
				sceMp4,
				mp4msv,
				sceHttp,
				sceHttps,
				sceHttpStorage,
				sceSsl,
				sceP3da,
				sceGameUpdate,
				sceUsbCam,
				sceJpeg,
				sceUsb,
				sceHeap,
				KDebugForKernel,
				sceCcc,
				scePauth,
				sceSfmt19937,
				sceMd5,
				sceParseUri,
				sceUsbAcc,
				sceMt19937,
				sceAac,
				sceFpu,
				sceUsbMic,
				sceAudioRouting,
				sceUsbGps,
				sceAudiocodec,
				sceVideocodec,
				sceAdler,
				sceSha1,
				sceSha256,
				sceMeCore,
				sceMeBoot,
				KUBridge,
				SysclibForKernel,
				semaphore,
				ModuleMgrForKernel,
				sceReg,
				sceDve,
				sceSysEventForKernel,
				sceChkreg,
				sceMsAudio_Service,
				sceMePower,
				sceResmgr,
				UtilsForKernel,
				sceLibUpdateDL,
				sceParseHttp,
				sceMgr_driver,
				sceChnnlsv,
				sceUsbstor,
				sceIdStorage,
				sceCertLoader,
				sceDNAS,
				sceDNASCore,
				sceMcctrl,
				sceNetStun,
				sceMeMemory,
				sceMeVideo,
				sceMeAudio,
				InitForKernel,
				sceMemab,
				DmacManForKernel,
				sceSyscon,
				sceLed,
				sceSysreg,
				scePsheet,
				sceUmdMan,
				sceCodepage,
				sceMSstor,
				sceAta,
				sceGpio,
				sceNand,
				sceBSMan,
				memlmd,
				reboot,
				sceI2c,
				scePwm,
				sceLcdc,
				sceDmacplus,
				sceDdr,
				sceMScm,
				sceG729,
				scePopsMan,
				scePaf
			}

			public readonly InnerEnum innerEnumValue;
			private readonly string nameValue;
			private readonly int ordinalValue;
			private static int nextOrdinal = 0;

			internal HLEModule module;
			internal bool loadedByDefault;
			internal string[] names;
			internal string prxFileName;

			// Module loaded by default in all Firmware versions
			internal ModuleInfo(string name, InnerEnum innerEnum, HLEModule module)
			{
				this.module = module;
				loadedByDefault = true;
				names = null;

				nameValue = name;
				ordinalValue = nextOrdinal++;
				innerEnumValue = innerEnum;
			}

			// Module only loaded as a PRX, under different names
			internal ModuleInfo(string name, InnerEnum innerEnum, HLEModule module, string[] prxNames)
			{
				this.module = module;
				loadedByDefault = false;
				this.names = prxNames;

				nameValue = name;
				ordinalValue = nextOrdinal++;
				innerEnumValue = innerEnum;
			}

			// Module only loaded as a PRX, under different names
			internal ModuleInfo(string name, InnerEnum innerEnum, HLEModule module, string[] prxNames, string prxFileName)
			{
				this.module = module;
				loadedByDefault = false;
				this.names = prxNames;
				this.prxFileName = prxFileName;

				nameValue = name;
				ordinalValue = nextOrdinal++;
				innerEnumValue = innerEnum;
			}

			public HLEModule Module
			{
				get
				{
					return module;
				}
			}

			public string[] Names
			{
				get
				{
					return names;
				}
			}

			public bool LoadedByDefault
			{
				get
				{
					return loadedByDefault;
				}
			}

			public string PrxFileName
			{
				get
				{
					return prxFileName;
				}
			}

			public static IList<ModuleInfo> values()
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

			public static ModuleInfo valueOf(string name)
			{
				foreach (ModuleInfo enumInstance in ModuleInfo.valueList)
				{
					if (enumInstance.nameValue == name)
					{
						return enumInstance;
					}
				}
				throw new System.ArgumentException(name);
			}
		}

		public static HLEModuleManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new HLEModuleManager();
				}
				return instance;
			}
		}

		private HLEModuleManager()
		{
			defaultHLEFunctionLogging = typeof(HLEModuleManager).getAnnotation(typeof(HLELogging));
			nidMapper = NIDMapper.Instance;
			syscallToFunction = new Dictionary<>();
			nidToFunction = new Dictionary<>();
		}

		/// <summary>
		/// (String)"2.71" to (int)271 </summary>
		public static int psfFirmwareVersionToInt(string firmwareVersion)
		{
			int version = Emulator.Instance.FirmwareVersion;

			if (!string.ReferenceEquals(firmwareVersion, null))
			{
				// Some games have firmwareVersion = "5.00?", keep only the digits
				while (!char.IsDigit(firmwareVersion[firmwareVersion.Length - 1]))
				{
					firmwareVersion = firmwareVersion.Substring(0, firmwareVersion.Length - 1);
				}

				version = (int)(float.Parse(firmwareVersion) * 100);

				// We started implementing stuff under 150 even if it existed in 100
				if (version < 150)
				{
					version = 150;
				}
			}

			return version;
		}

		public virtual void init()
		{
			installedModules.Clear();
			installDefaultModules();
			initialiseFlash0PRXMap();
		}

		/// <summary>
		/// Install the modules that are loaded by default on the current firmware version.
		/// </summary>
		private void installDefaultModules()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Loading default HLE modules"));
			}

			foreach (ModuleInfo defaultModule in ModuleInfo.values())
			{
				if (defaultModule.LoadedByDefault)
				{
					installModuleWithAnnotations(defaultModule.Module);
				}
				else
				{
					// This module is not loaded by default on this firmware version.
					// Install and Uninstall the module to register the module syscalls
					// so that the loader can still resolve the imports for this module.
					installModuleWithAnnotations(defaultModule.Module);
					uninstallModuleWithAnnotations(defaultModule.Module);
				}
			}
		}

		private void addToFlash0PRXMap(string prxName, HLEModule module)
		{
			prxName = prxName.ToLower();
			if (!flash0prxMap.ContainsKey(prxName))
			{
				flash0prxMap[prxName] = new LinkedList<HLEModule>();
			}
			IList<HLEModule> modules = flash0prxMap[prxName];
			modules.Add(module);
		}

		// Add modules in flash (or on UMD) that aren't loaded by default on this firmwareVersion
		private void initialiseFlash0PRXMap()
		{
			flash0prxMap = new Dictionary<string, IList<HLEModule>>();
			moduleInfos = new Dictionary<HLEModule, ModuleInfo>();

			foreach (ModuleInfo moduleInfo in ModuleInfo.values())
			{
				HLEModule hleModule = moduleInfo.Module;

				moduleInfos[hleModule] = moduleInfo;

				if (!moduleInfo.LoadedByDefault)
				{
					string[] names = moduleInfo.Names;
					for (int i = 0; names != null && i < names.Length; i++)
					{
						addToFlash0PRXMap(names[i], hleModule);
					}
				}
			}
		}

		public virtual bool hasFlash0Module(string prxname)
		{
			if (string.ReferenceEquals(prxname, null))
			{
				return false;
			}

			return flash0prxMap.ContainsKey(prxname.ToLower());
		}

		public virtual string getModulePrxFileName(string name)
		{
			if (!string.ReferenceEquals(name, null))
			{
				IList<HLEModule> modules = flash0prxMap[name.ToLower()];
				if (modules != null)
				{
					foreach (HLEModule module in modules)
					{
						ModuleInfo moduleInfo = moduleInfos[module];
						if (moduleInfo != null)
						{
							return moduleInfo.PrxFileName;
						}
					}
				}
			}

			return null;
		}

		/// <returns> the UID assigned to the module or negative on error
		/// TODO need to figure out how the uids work when 1 prx contains several modules.  </returns>
		public virtual int LoadFlash0Module(string name)
		{
			if (!string.ReferenceEquals(name, null))
			{
				IList<HLEModule> modules = flash0prxMap[name.ToLower()];
				if (modules != null)
				{
					foreach (HLEModule module in modules)
					{
						installModuleWithAnnotations(module);
					}
				}
			}

			SceModule fakeModule = new SceModule(true);
			fakeModule.modname = name;
			fakeModule.write(Memory.Instance, fakeModule.address);
			Managers.modules.addModule(fakeModule);

			return fakeModule.modid;
		}

		public virtual void UnloadFlash0Module(SceModule sceModule)
		{
			if (sceModule == null)
			{
				return;
			}

			if (!string.ReferenceEquals(sceModule.modname, null))
			{
				IList<HLEModule> prx = flash0prxMap[sceModule.modname.ToLower()];
				if (prx != null)
				{
					foreach (HLEModule module in prx)
					{
						uninstallModuleWithAnnotations(module);
					}
				}
			}

			// TODO terminate delete all threads that belong to this module

			sceModule.free();

			Managers.modules.removeModule(sceModule.modid);

			if (!sceModule.isFlashModule)
			{
				// Invalidate the compiled code from the unloaded module
				RuntimeContext.invalidateAll();
			}
		}

		public virtual void addFunction(int nid, HLEModuleFunction func)
		{
			int syscallCode;
			if (nid == HLESyscallNid)
			{
				syscallCode = nidMapper.NewSyscallNumber;
			}
			else
			{
				if (!nidMapper.addHLENid(nid, func.FunctionName, func.ModuleName, func.FirmwareVersion))
				{
					Console.WriteLine(string.Format("Tried to register a second handler for NID 0x{0:X8} called {1}", nid, func.FunctionName));
				}

				nidToFunction[nid] = func;

				syscallCode = nidMapper.getSyscallByNid(nid, func.ModuleName);
			}

			if (syscallCode >= 0)
			{
				func.SyscallCode = syscallCode;
				syscallToFunction[syscallCode] = func;
			}
		}

		public virtual HLEModuleFunction getFunctionFromSyscallCode(int syscallCode)
		{
			return syscallToFunction[syscallCode];
		}

		public virtual HLEModuleFunction getFunctionFromAddress(int address)
		{
			int nid = nidMapper.getNidByAddress(address);
			if (nid == 0)
			{
				// Verify if this not the address of a stub call:
				//   J   realAddress
				//   NOP
				if (Memory.isAddressGood(address))
				{
					Memory mem = Memory.Instance;
					if (((int)((uint)mem.read32(address) >> 26)) == AllegrexOpcodes.J)
					{
						if (mem.read32(address + 4) == ThreadManForUser.NOP())
						{
							int jumpAddress = (mem.read32(address) & 0x03FFFFFF) << 2;

							nid = nidMapper.getNidByAddress(jumpAddress);
						}
					}
				}
			}

			if (nid == 0)
			{
				return null;
			}

			HLEModuleFunction func = nidToFunction[nid];

			return func;
		}

		public virtual HLEModuleFunction getFunctionFromNID(int nid)
		{
			return nidToFunction[nid];
		}

		public virtual int getNIDFromFunctionName(string functionName)
		{
			foreach (HLEModuleFunction function in nidToFunction.Values)
			{
				if (functionName.Equals(function.FunctionName))
				{
					return function.Nid;
				}
			}

			return 0;
		}

		public virtual void removeFunction(HLEModuleFunction func)
		{
			nidMapper.unloadNid(func.Nid);
		}

		public virtual void startModules(bool startFromSyscall)
		{
			if (modulesStarted)
			{
				return;
			}

			this.startFromSyscall = startFromSyscall;

			foreach (ModuleInfo defaultModule in ModuleInfo.values())
			{
				if (defaultModule.module.Started)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Module {0} already started", defaultModule.module.Name));
					}
				}
				else
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Starting module {0}", defaultModule.module.Name));
					}

					defaultModule.module.start();

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Started module {0}", defaultModule.module.Name));
					}
				}
			}

			this.startFromSyscall = false;
			modulesStarted = true;
		}

		public virtual void stopModules()
		{
			if (!modulesStarted)
			{
				return;
			}

			foreach (ModuleInfo defaultModule in ModuleInfo.values())
			{
				defaultModule.module.stop();
			}

			modulesStarted = false;
		}

		public virtual bool StartFromSyscall
		{
			get
			{
				return startFromSyscall;
			}
		}

		private void installFunctionWithAnnotations(HLEFunction hleFunction, Method method, HLEModule hleModule)
		{
			HLEUnimplemented hleUnimplemented = method.getAnnotation(typeof(HLEUnimplemented));
			HLELogging hleLogging = method.getAnnotation(typeof(HLELogging));

			// Take the module default logging if no HLELogging has been
			// defined at the function level and if the function is not
			// unimplemented (which will produce it's own logging).
			if (hleLogging == null)
			{
				if (hleUnimplemented != null)
				{
					// Take the logging level of the HLEUnimplemented class
					// as default value for unimplemented functions
					hleLogging = typeof(HLEUnimplemented).getAnnotation(typeof(HLELogging));
				}
				else
				{
					HLELogging hleModuleLogging = method.DeclaringClass.getAnnotation(typeof(HLELogging));
					if (hleModuleLogging != null)
					{
						// Take the module default logging
						hleLogging = hleModuleLogging;
					}
					else
					{
						hleLogging = defaultHLEFunctionLogging;
					}
				}
			}

			string moduleName = hleFunction.moduleName();
			string functionName = hleFunction.functionName();

			if (moduleName.Length == 0)
			{
				moduleName = hleModule.Name;
			}

			if (functionName.Length == 0)
			{
				functionName = method.Name;
			}

			HLEModuleFunction hleModuleFunction = new HLEModuleFunction(moduleName, functionName, hleFunction.nid(), hleModule, method, hleFunction.checkInsideInterrupt(), hleFunction.checkDispatchThreadEnabled(), hleFunction.stackUsage(), hleFunction.version());

			if (hleUnimplemented != null)
			{
				hleModuleFunction.Unimplemented = true;
			}

			if (hleLogging != null)
			{
				hleModuleFunction.LoggingLevel = hleLogging.level();
			}

			hleModule.installedHLEModuleFunctions[functionName] = hleModuleFunction;

			addFunction(hleFunction.nid(), hleModuleFunction);
		}

		/// <summary>
		/// Iterates over an object fields searching for HLEFunction annotations and install them.
		/// </summary>
		/// <param name="hleModule"> </param>
		public virtual void installModuleWithAnnotations(HLEModule hleModule)
		{
			if (installedModules.Contains(hleModule))
			{
				return;
			}

			try
			{
				foreach (Method method in hleModule.GetType().GetMethods())
				{
					HLEFunction hleFunction = method.getAnnotation(typeof(HLEFunction));
					if (hleFunction != null)
					{
						installFunctionWithAnnotations(hleFunction, method, hleModule);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("installModuleWithAnnotations", e);
			}

			installedModules.Add(hleModule);
			hleModule.load();
		}

		/// <summary>
		/// Same as installModuleWithAnnotations but uninstalling.
		/// </summary>
		/// <param name="hleModule"> </param>
		public virtual void uninstallModuleWithAnnotations(HLEModule hleModule)
		{
			try
			{
				foreach (HLEModuleFunction hleModuleFunction in hleModule.installedHLEModuleFunctions.Values)
				{
					this.removeFunction(hleModuleFunction);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("uninstallModuleWithAnnotations", e);
			}

			installedModules.remove(hleModule);
			hleModule.unload();
		}

		private bool isModuleFileNameVshOnly(string moduleFileName)
		{
			for (int i = 0; i < moduleFilesNameVshOnly.Length; i++)
			{
				if (moduleFilesNameVshOnly[i].Equals(moduleFileName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		public virtual void loadAvailableFlash0Modules(bool fromSyscall)
		{
			bool runningFromVsh = Emulator.MainGUI.RunningFromVsh && !fromSyscall;

			IList<string> availableModuleFileNames = new LinkedList<string>();
			foreach (string moduleFileName in moduleFileNamesToBeLoaded)
			{
				if (runningFromVsh || !isModuleFileNameVshOnly(moduleFileName))
				{
					StringBuilder localFileName = new StringBuilder();
					IVirtualFileSystem vfs = Modules.IoFileMgrForUserModule.getVirtualFileSystem(moduleFileName, localFileName);
					if (vfs != null && vfs.ioGetstat(localFileName.ToString(), new SceIoStat()) == 0)
					{
						// The module is available, load it
						availableModuleFileNames.Add(moduleFileName);
					}
				}
			}

			if (availableModuleFileNames.Count == 0)
			{
				// No module available, do nothing
				return;
			}

			// This HLE module need to be started in order
			// to be able to load and start the available modules.
			Modules.ModuleMgrForUserModule.start();

			int startPriority = 0x10;
			foreach (string moduleFileName in availableModuleFileNames)
			{
				if (log.InfoEnabled)
				{
					log.info(string.Format("Loading and starting the module '{0}', it will replace the equivalent HLE functions", moduleFileName));
				}

				IAction onModuleStartAction = null;

				// loadcore.prx requires start parameters
				if ("flash0:/kd/loadcore.prx".Equals(moduleFileName))
				{
					onModuleStartAction = Modules.LoadCoreForKernelModule.ModuleStartAction;
				}

				Modules.ModuleMgrForUserModule.hleKernelLoadAndStartModule(moduleFileName, startPriority++, onModuleStartAction);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			foreach (ModuleInfo moduleInfo in ModuleInfo.values())
			{
				HLEModule hleModule = moduleInfo.Module;
				hleModule.read(stream);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			foreach (ModuleInfo moduleInfo in ModuleInfo.values())
			{
				HLEModule hleModule = moduleInfo.Module;
				hleModule.write(stream);
			}
		}
	}
}