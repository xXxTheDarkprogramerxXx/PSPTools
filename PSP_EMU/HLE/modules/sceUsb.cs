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
namespace pspsharp.HLE.modules
{


	using Logger = org.apache.log4j.Logger;

	using Managers = pspsharp.HLE.kernel.Managers;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using Usb = pspsharp.hardware.Usb;

	public class sceUsb : HLEModule
	{
		public static Logger log = Modules.getLogger("sceUsb");

		public const string PSP_USBBUS_DRIVERNAME = "USBBusDriver";

		public const int PSP_USB_CONNECTION_NOT_ESTABLISHED = 0x001;
		public const int PSP_USB_CONNECTION_ESTABLISHED = 0x002;
		public const int PSP_USB_CABLE_DISCONNECTED = 0x010;
		public const int PSP_USB_CABLE_CONNECTED = 0x020;
		public const int PSP_USB_DEACTIVATED = 0x100;
		public const int PSP_USB_ACTIVATED = 0x200;
		protected internal const int WAIT_MODE_ANDOR_MASK = 0x1;
		protected internal const int WAIT_MODE_AND = 0x0;
		protected internal const int WAIT_MODE_OR = 0x1;

		protected internal bool usbActivated = false;
		protected internal bool usbStarted = false;
		protected internal Dictionary<string, SceModule> loadedModules;
		protected internal int callbackId = -1;

		public override void start()
		{
			usbActivated = false;
			usbStarted = false;
			loadedModules = new Dictionary<string, SceModule>();

			base.start();
		}

		protected internal virtual int UsbState
		{
			get
			{
				int state = Usb.CableConnected ? PSP_USB_CABLE_CONNECTED : PSP_USB_CABLE_DISCONNECTED;
    
				// USB has been activated?
				state |= usbActivated ? PSP_USB_ACTIVATED : PSP_USB_DEACTIVATED;
    
				// USB has been started?
				state |= usbStarted ? PSP_USB_CONNECTION_ESTABLISHED : PSP_USB_CONNECTION_NOT_ESTABLISHED;
    
				return state;
			}
		}

		protected internal virtual bool matchState(int waitState, int waitMode)
		{
			int state = UsbState;
			if ((waitMode & WAIT_MODE_ANDOR_MASK) == WAIT_MODE_AND)
			{
				// WAIT_MODE_AND
				return (state & waitState) == waitState;
			}
			// WAIT_MODE_OR
			return (state & waitState) != 0;
		}

		protected internal virtual void notifyCallback()
		{
			if (callbackId >= 0)
			{
				Modules.ThreadManForUserModule.hleKernelNotifyCallback(SceKernelThreadInfo.THREAD_CALLBACK_USB, UsbState);
			}
		}

		/// <summary>
		/// Start a USB driver.
		/// </summary>
		/// <param name="driverName"> - name of the USB driver to start </param>
		/// <param name="size"> - Size of arguments to pass to USB driver start </param>
		/// <param name="args"> - Arguments to pass to USB driver start
		/// </param>
		/// <returns> 0 on success </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAE5DE6AF, version = 150) public int sceUsbStart(String driverName, int size, @CanBeNull pspsharp.HLE.TPointer args)
		[HLEFunction(nid : 0xAE5DE6AF, version : 150)]
		public virtual int sceUsbStart(string driverName, int size, TPointer args)
		{
			usbStarted = true;

			HLEModuleManager moduleManager = HLEModuleManager.Instance;
			if (moduleManager.hasFlash0Module(driverName))
			{
				log.info(string.Format("Loading HLE module '{0}'", driverName));
				int sceModuleId = moduleManager.LoadFlash0Module(driverName);
				SceModule module = Managers.modules.getModuleByUID(sceModuleId);
				loadedModules[driverName] = module;
			}

			notifyCallback();

			return 0;
		}

		/// <summary>
		/// Stop a USB driver.
		/// </summary>
		/// <param name="driverName"> - name of the USB driver to stop </param>
		/// <param name="size"> - Size of arguments to pass to USB driver start </param>
		/// <param name="args"> - Arguments to pass to USB driver start
		/// </param>
		/// <returns> 0 on success </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC2464FA0, version = 150) public int sceUsbStop(pspsharp.HLE.PspString driverName, int size, @CanBeNull pspsharp.HLE.TPointer args)
		[HLEFunction(nid : 0xC2464FA0, version : 150)]
		public virtual int sceUsbStop(PspString driverName, int size, TPointer args)
		{
			usbStarted = false;

			SceModule module = loadedModules.Remove(driverName.String);
			if (module != null)
			{
				HLEModuleManager moduleManager = HLEModuleManager.Instance;
				moduleManager.UnloadFlash0Module(module);
			}

			notifyCallback();

			return 0;
		}

		/// <summary>
		/// Get USB state
		/// </summary>
		/// <returns> OR'd PSP_USB_* constants </returns>
		[HLEFunction(nid : 0xC21645A4, version : 150)]
		public virtual int sceUsbGetState()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceUsbGetState returning 0x{0:X}", UsbState));
			}

			return UsbState;
		}

		[HLEFunction(nid : 0x4E537366, version : 150)]
		public virtual int sceUsbGetDrvList(int unknown1, int unknown2, int unknown3)
		{
			log.warn(string.Format("Unimplemented sceUsbGetDrvList unknown1=0x{0:X8}, unknown2=0x{1:X8}, unknown3=0x{2:X8}", unknown1, unknown2, unknown3));

			return 0;
		}

		/// <summary>
		/// Get state of a specific USB driver
		/// </summary>
		/// <param name="driverName"> - name of USB driver to get status from
		/// </param>
		/// <returns> 1 if the driver has been started, 2 if it is stopped </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x112CC951, version = 150) public int sceUsbGetDrvState(pspsharp.HLE.PspString driverName)
		[HLEFunction(nid : 0x112CC951, version : 150)]
		public virtual int sceUsbGetDrvState(PspString driverName)
		{
			return 0;
		}

		/// <summary>
		/// Activate a USB driver.
		/// </summary>
		/// <param name="pid"> - Product ID for the default USB Driver
		/// </param>
		/// <returns> 0 on success </returns>
		[HLEFunction(nid : 0x586DB82C, version : 150)]
		public virtual int sceUsbActivate(int pid)
		{
			return sceUsbActivateWithCharging(pid, false);
		}

		/// <summary>
		/// Deactivate USB driver.
		/// </summary>
		/// <param name="pid"> - Product ID for the default USB driver
		/// </param>
		/// <returns> 0 on success </returns>
		[HLEFunction(nid : 0xC572A9C8, version : 150)]
		public virtual int sceUsbDeactivate(int pid)
		{
			usbActivated = false;
			notifyCallback();

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x5BE0E002, version = 150) public int sceUsbWaitState(int state, int waitMode, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x5BE0E002, version : 150)]
		public virtual int sceUsbWaitState(int state, int waitMode, TPointer32 timeoutAddr)
		{
			if (!matchState(state, waitMode))
			{
				log.warn(string.Format("Unimplemented sceUsbWaitState state=0x{0:X}, waitMode=0x{1:X}, timeoutAddr={2} - non-matching state not implemented", state, waitMode, timeoutAddr));
				Modules.ThreadManForUserModule.hleBlockCurrentThread(SceKernelThreadInfo.JPCSP_WAIT_USB);
				return 0;
			}

			int usbState = UsbState;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceUsbWaitState returning 0x{0:X}", usbState));
			}
			return usbState;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x616F2B61, version = 150) public int sceUsbWaitStateCB(int state, int waitMode, @CanBeNull pspsharp.HLE.TPointer32 timeoutAddr)
		[HLEFunction(nid : 0x616F2B61, version : 150)]
		public virtual int sceUsbWaitStateCB(int state, int waitMode, TPointer32 timeoutAddr)
		{
			if (!matchState(state, waitMode))
			{
				log.warn(string.Format("Unimplemented sceUsbWaitStateCB state=0x{0:X}, waitMode=0x{1:X}, timeoutAddr={2} - non-matching state not implemented", state, waitMode, timeoutAddr));
				Modules.ThreadManForUserModule.hleBlockCurrentThread(SceKernelThreadInfo.JPCSP_WAIT_USB);
				return 0;
			}

			int usbState = UsbState;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceUsbWaitStateCB returning 0x{0:X}", usbState));
			}
			return usbState;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1C360735, version = 150) public int sceUsbWaitCancel()
		[HLEFunction(nid : 0x1C360735, version : 150)]
		public virtual int sceUsbWaitCancel()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8BFC3DE8, version = 150) public int sceUsb_8BFC3DE8(int callbackId, int unknown1, int unknown2)
		[HLEFunction(nid : 0x8BFC3DE8, version : 150)]
		public virtual int sceUsb_8BFC3DE8(int callbackId, int unknown1, int unknown2)
		{
			// Registering a callback?
			if (Modules.ThreadManForUserModule.hleKernelRegisterCallback(SceKernelThreadInfo.THREAD_CALLBACK_USB, callbackId))
			{
				this.callbackId = callbackId;
				notifyCallback();
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x89DE0DC5, version = 150) public int sceUsb_89DE0DC5(int callbackId)
		[HLEFunction(nid : 0x89DE0DC5, version : 150)]
		public virtual int sceUsb_89DE0DC5(int callbackId)
		{
			// Unregistering a callback?
			if (this.callbackId == callbackId)
			{
				this.callbackId = -1;
			}

			return 0;
		}

		/// <summary>
		/// Activate a USB driver.
		/// </summary>
		/// <param name="pid">      - Product ID for the default USB Driver </param>
		/// <param name="charging"> - charging the PSP while the USB is connected?
		/// </param>
		/// <returns> 0 on success </returns>
		[HLEFunction(nid : 0xE20B23A6, version : 150)]
		public virtual int sceUsbActivateWithCharging(int pid, bool charging)
		{
			usbActivated = true;
			notifyCallback();

			return 0;
		}
	}
}