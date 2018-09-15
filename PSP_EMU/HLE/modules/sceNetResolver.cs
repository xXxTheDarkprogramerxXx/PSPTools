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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_NET_RESOLVER_BAD_ID;
	using Usage = pspsharp.HLE.BufferInfo.Usage;


	//using Logger = org.apache.log4j.Logger;

	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;

	public class sceNetResolver : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceNetResolver");
		private const string uidPurpose = "sceNetResolver-NetResolver";

		protected internal class ResolverID
		{
			internal int id;
			internal bool isRunning;

			public ResolverID(int id, bool running)
			{
				this.id = id;
				this.isRunning = running;
			}

			public virtual int ID
			{
				get
				{
					return id;
				}
			}

			public virtual bool IDStatus
			{
				get
				{
					return isRunning;
				}
			}

			public virtual void stop()
			{
				isRunning = false;
			}
		}

		protected internal Dictionary<int, ResolverID> RIDs = new Dictionary<int, ResolverID>();

		public virtual int checkRid(int rid)
		{
			if (!RIDs.ContainsKey(rid))
			{
				throw new SceKernelErrorException(ERROR_NET_RESOLVER_BAD_ID);
			}

			return rid;
		}

		/// <summary>
		/// Initialize the resolver library
		/// </summary>
		/// <returns> 0 on success, < 0 on error. </returns>
		[HLEFunction(nid : 0xF3370E61, version : 150)]
		public virtual int sceNetResolverInit()
		{
			return 0;
		}

		/// <summary>
		/// Terminate the resolver library
		/// </summary>
		/// <returns> 0 on success, < 0 on error </returns>
		[HLEFunction(nid : 0x6138194A, version : 150)]
		public virtual int sceNetResolverTerm()
		{
			return 0;
		}

		/// <summary>
		/// Create a resolver object
		/// </summary>
		/// <param name="rid"> - Pointer to receive the resolver id </param>
		/// <param name="buf"> - Temporary buffer </param>
		/// <param name="buflen"> - Length of the temporary buffer
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
		[HLEFunction(nid : 0x244172AF, version : 150)]
		public virtual int sceNetResolverCreate(TPointer32 pRid, TPointer buffer, int bufferLength)
		{
			int newID = SceUidManager.getNewUid(uidPurpose);
			ResolverID newRID = new ResolverID(newID, true);
			RIDs[newID] = newRID;
			pRid.setValue(newRID.ID);

			return 0;
		}

		/// <summary>
		/// Delete a resolver
		/// </summary>
		/// <param name="rid"> - The resolver to delete
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x94523E09, version = 150) public int sceNetResolverDelete(@CheckArgument("checkRid") int rid)
		[HLEFunction(nid : 0x94523E09, version : 150)]
		public virtual int sceNetResolverDelete(int rid)
		{
			RIDs.Remove(rid);
			SceUidManager.releaseUid(rid, uidPurpose);

			return 0;
		}

		/// <summary>
		/// Begin a name to address lookup
		/// </summary>
		/// <param name="rid"> - Resolver id </param>
		/// <param name="hostname"> - Name to resolve </param>
		/// <param name="addr"> - Pointer to in_addr structure to receive the address </param>
		/// <param name="timeout"> - Number of seconds before timeout </param>
		/// <param name="retry"> - Number of retires
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x224C5F44, version = 150) public int sceNetResolverStartNtoA(@CheckArgument("checkRid") int rid, pspsharp.HLE.PspString hostname, @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer32 addr, int timeout, int retry)
		[HLEFunction(nid : 0x224C5F44, version : 150)]
		public virtual int sceNetResolverStartNtoA(int rid, PspString hostname, TPointer32 addr, int timeout, int retry)
		{
			try
			{
				InetAddress inetAddress = InetAddress.getByName(hostname.String);
				int resolvedAddress = sceNetInet.bytesToInternetAddress(inetAddress.Address);
				addr.setValue(resolvedAddress);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceNetResolverStartNtoA returning address 0x{0:X8}('{1}')", resolvedAddress, sceNetInet.internetAddressToString(resolvedAddress)));
				}
				else if (log.InfoEnabled)
				{
					log.info(string.Format("sceNetResolverStartNtoA resolved '{0}' into '{1}'", hostname.String, sceNetInet.internetAddressToString(resolvedAddress)));
				}
			}
			catch (UnknownHostException e)
			{
				Console.WriteLine(e);
				return SceKernelErrors.ERROR_NET_RESOLVER_INVALID_HOST;
			}

			return 0;
		}

		/// <summary>
		/// Begin a address to name lookup
		/// </summary>
		/// <param name="rid"> -Resolver id </param>
		/// <param name="addr"> - Pointer to the address to resolve </param>
		/// <param name="hostname"> - Buffer to receive the name </param>
		/// <param name="hostname_len"> - Length of the buffer </param>
		/// <param name="timeout"> - Number of seconds before timeout </param>
		/// <param name="retry"> - Number of retries
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x629E2FB7, version = 150) public int sceNetResolverStartAtoN(@CheckArgument("checkRid") int rid, int addr, pspsharp.HLE.TPointer hostnameAddr, int hostnameLength, int timeout, int retry)
		[HLEFunction(nid : 0x629E2FB7, version : 150)]
		public virtual int sceNetResolverStartAtoN(int rid, int addr, TPointer hostnameAddr, int hostnameLength, int timeout, int retry)
		{
			try
			{
				sbyte[] bytes = sceNetInet.internetAddressToBytes(addr);
				InetAddress inetAddress = InetAddress.getByAddress(bytes);
				string hostName = inetAddress.HostName;
				hostnameAddr.setStringNZ(hostnameLength, hostName);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceNetResolverStartAtoN returning host name '{0}'", hostName));
				}
			}
			catch (UnknownHostException e)
			{
				Console.WriteLine(e);
				return SceKernelErrors.ERROR_NET_RESOLVER_INVALID_HOST;
			}

			return 0;
		}

		/// <summary>
		/// Stop a resolver operation
		/// </summary>
		/// <param name="rid"> - Resolver id
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x808F6063, version = 150) public int sceNetResolverStop(@CheckArgument("checkRid") int rid)
		[HLEFunction(nid : 0x808F6063, version : 150)]
		public virtual int sceNetResolverStop(int rid)
		{
			ResolverID currentRID = RIDs[rid];
			if (!currentRID.IDStatus)
			{
				return SceKernelErrors.ERROR_NET_RESOLVER_ALREADY_STOPPED;
			}

			currentRID.stop();

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x14C17EF9, version = 150) public int sceNetResolverStartNtoAAsync()
		[HLEFunction(nid : 0x14C17EF9, version : 150)]
		public virtual int sceNetResolverStartNtoAAsync()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAAC09184, version = 150) public int sceNetResolverStartAtoNAsync()
		[HLEFunction(nid : 0xAAC09184, version : 150)]
		public virtual int sceNetResolverStartAtoNAsync()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4EE99358, version = 150) public int sceNetResolverPollAsync()
		[HLEFunction(nid : 0x4EE99358, version : 150)]
		public virtual int sceNetResolverPollAsync()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x12748EB9, version = 150) public int sceNetResolverWaitAsync(pspsharp.Processor processor)
		[HLEFunction(nid : 0x12748EB9, version : 150)]
		public virtual int sceNetResolverWaitAsync(Processor processor)
		{
			return 0;
		}
	}
}