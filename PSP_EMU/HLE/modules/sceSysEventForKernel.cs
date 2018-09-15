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
	//using Logger = org.apache.log4j.Logger;

	using pspSysEventHandler = pspsharp.HLE.kernel.types.pspSysEventHandler;

	public class sceSysEventForKernel : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceSysEventForKernel");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAEB300AE, version = 150) public int sceKernelIsRegisterSysEventHandler()
		[HLEFunction(nid : 0xAEB300AE, version : 150)]
		public virtual int sceKernelIsRegisterSysEventHandler()
		{
			return 0;
		}

		/// <summary>
		/// Register a SysEvent handler.
		/// </summary>
		/// <param name="handler">			the handler to register
		/// @return					0 on success, < 0 on error </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCD9E4BB5, version = 150) public int sceKernelRegisterSysEventHandler(pspsharp.HLE.TPointer handler)
		[HLEFunction(nid : 0xCD9E4BB5, version : 150)]
		public virtual int sceKernelRegisterSysEventHandler(TPointer handler)
		{
			pspSysEventHandler sysEventHandler = new pspSysEventHandler();
			sysEventHandler.read(handler);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelRegisterSysEventHandler handler: {0}", sysEventHandler));
			}

			if ("SceFatfsSysEvent".Equals(sysEventHandler.name))
			{
				Modules.sceMSstorModule.installDrivers();
			}

			return 0;
		}

		/// <summary>
		/// Unregister a SysEvent handler.
		/// </summary>
		/// <param name="handler">			the handler to unregister
		/// @return					0 on success, < 0 on error </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD7D3FDCD, version = 150) public int sceKernelUnregisterSysEventHandler(pspsharp.HLE.TPointer handler)
		[HLEFunction(nid : 0xD7D3FDCD, version : 150)]
		public virtual int sceKernelUnregisterSysEventHandler(TPointer handler)
		{
			return 0;
		}

		/// <summary>
		/// Dispatch a SysEvent event.
		/// </summary>
		/// <param name="eventTypeMask">		the event type mask </param>
		/// <param name="eventId">			the event id </param>
		/// <param name="eventName">			the event name </param>
		/// <param name="param">				the pointer to the custom parameters </param>
		/// <param name="resultAddr">		the pointer to the result </param>
		/// <param name="breakNonzero">		set to 1 to interrupt the calling chain after the first non-zero return </param>
		/// <param name="breakHandler">		the pointer to the event handler having interrupted
		/// @return					0 on success, < 0 on error </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x36331294, version = 150) public int sceKernelSysEventDispatch(int eventTypeMask, int eventId, String eventName, int param, pspsharp.HLE.TPointer32 resultAddr, int breakNonzero, int breakHandler)
		[HLEFunction(nid : 0x36331294, version : 150)]
		public virtual int sceKernelSysEventDispatch(int eventTypeMask, int eventId, string eventName, int param, TPointer32 resultAddr, int breakNonzero, int breakHandler)
		{
			return 0;
		}

		/// <summary>
		/// Get the first SysEvent handler (the rest can be found with the linked list).
		/// 
		/// @return					0 on error, handler on success
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x68D55505, version = 150) public int sceKernelReferSysEventHandler()
		[HLEFunction(nid : 0x68D55505, version : 150)]
		public virtual int sceKernelReferSysEventHandler()
		{
			return 0;
		}
	}

}