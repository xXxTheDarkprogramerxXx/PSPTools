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
//	import static pspsharp.HLE.kernel.types.SceNetAdhocDiscoverParam.NET_ADHOC_DISCOVER_RESULT_PEER_FOUND;
	using SceNetAdhocDiscoverParam = pspsharp.HLE.kernel.types.SceNetAdhocDiscoverParam;

	using Logger = org.apache.log4j.Logger;

	public class sceNetAdhocDiscover : HLEModule
	{
		public static Logger log = Modules.getLogger("sceNetAdhocDiscover");
		protected internal const int NET_ADHOC_DISCOVER_STATUS_NONE = 0;
		protected internal const int NET_ADHOC_DISCOVER_STATUS_IN_PROGRESS = 1;
		protected internal const int NET_ADHOC_DISCOVER_STATUS_COMPLETED = 2;
		protected internal int status;
		protected internal SceNetAdhocDiscoverParam netAdhocDiscoverParam;
		protected internal long discoverStartMillis;
		protected internal const int DISCOVER_DURATION_MILLIS = 2000;

		public override void start()
		{
			status = NET_ADHOC_DISCOVER_STATUS_NONE;

			base.start();
		}

		[HLEFunction(nid : 0x941B3877, version : 150)]
		public virtual int sceNetAdhocDiscoverInitStart(SceNetAdhocDiscoverParam netAdhocDiscoverParam)
		{
			this.netAdhocDiscoverParam = netAdhocDiscoverParam;
			status = NET_ADHOC_DISCOVER_STATUS_IN_PROGRESS;
			discoverStartMillis = Emulator.Clock.currentTimeMillis();

			return 0;
		}

		[HLEFunction(nid : 0x52DE1B97, version : 150)]
		public virtual int sceNetAdhocDiscoverUpdate()
		{
			if (status == NET_ADHOC_DISCOVER_STATUS_IN_PROGRESS)
			{
				long now = Emulator.Clock.currentTimeMillis();
				if (now >= discoverStartMillis + DISCOVER_DURATION_MILLIS)
				{
					// Fake a successful completion after some time
					status = NET_ADHOC_DISCOVER_STATUS_COMPLETED;
					netAdhocDiscoverParam.result = NET_ADHOC_DISCOVER_RESULT_PEER_FOUND;
				}
			}
			netAdhocDiscoverParam.write(Memory.Instance);

			return 0;
		}

		[HLEFunction(nid : 0x944DDBC6, version : 150)]
		public virtual int sceNetAdhocDiscoverGetStatus()
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceNetAdhocDiscoverGetStatus returning {0:D}", status));
			}

			return status;
		}

		[HLEFunction(nid : 0xA2246614, version : 150)]
		public virtual int sceNetAdhocDiscoverTerm()
		{
			status = NET_ADHOC_DISCOVER_STATUS_NONE;

			return 0;
		}

		[HLEFunction(nid : 0xF7D13214, version : 150)]
		public virtual int sceNetAdhocDiscoverStop()
		{
			status = NET_ADHOC_DISCOVER_STATUS_COMPLETED;
			netAdhocDiscoverParam.write(Memory.Instance);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA423A21B, version = 150) public int sceNetAdhocDiscoverRequestSuspend()
		[HLEFunction(nid : 0xA423A21B, version : 150)]
		public virtual int sceNetAdhocDiscoverRequestSuspend()
		{
			return 0;
		}
	}
}