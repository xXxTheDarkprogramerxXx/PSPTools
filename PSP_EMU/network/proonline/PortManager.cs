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
namespace pspsharp.network.proonline
{

	using sceNetApctl = pspsharp.HLE.modules.sceNetApctl;
	using UPnP = pspsharp.network.upnp.UPnP;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class PortManager
	{
		private IList<string> hosts = new LinkedList<string>();
		private IList<PortInfo> portInfos = new LinkedList<PortInfo>();
		private UPnP upnp;
		private string localIPAddress;
		private const int portLeaseDuration = 0;
		private const string portDescription = "pspsharp ProOnline Network";
		private const bool SUPPORTS_MAPPING_FOR_MULTIPLE_REMOTE_HOSTS = false;
		private const string ALL_REMOTE_HOSTS = "";

		private class PortInfo
		{
			internal int port;
			internal string protocol;

			public PortInfo(int port, string protocol)
			{
				this.port = port;
				this.protocol = protocol;
			}

			public override bool Equals(object obj)
			{
				if (obj is PortInfo)
				{
					PortInfo portInfo = (PortInfo) obj;
					return port == portInfo.port && protocol.Equals(portInfo.protocol);
				}
				return base.Equals(obj);
			}
		}

		public PortManager(UPnP upnp)
		{
			this.upnp = upnp;
			localIPAddress = sceNetApctl.LocalHostIP;
		}

		protected internal virtual string LocalIPAddress
		{
			get
			{
				return localIPAddress;
			}
		}

		public virtual void addHost(string host)
		{
			lock (this)
			{
				if (hosts.Contains(host))
				{
					return;
				}
        
				if (SUPPORTS_MAPPING_FOR_MULTIPLE_REMOTE_HOSTS)
				{
					// Open all the ports to this new host
					foreach (PortInfo portInfo in portInfos)
					{
						upnp.IGD.addPortMapping(upnp, host, portInfo.port, portInfo.protocol, portInfo.port, LocalIPAddress, portDescription, portLeaseDuration);
					}
				}
				else
				{
					if (hosts.Count == 0)
					{
						foreach (PortInfo portInfo in portInfos)
						{
							upnp.IGD.addPortMapping(upnp, ALL_REMOTE_HOSTS, portInfo.port, portInfo.protocol, portInfo.port, LocalIPAddress, portDescription, portLeaseDuration);
						}
					}
				}
        
				hosts.Add(host);
			}
		}

		public virtual void removeHost(string host)
		{
			lock (this)
			{
				if (!hosts.Contains(host))
				{
					return;
				}
        
				hosts.Remove(host);
        
				if (SUPPORTS_MAPPING_FOR_MULTIPLE_REMOTE_HOSTS)
				{
					// Remove all the port mappings from this host
					foreach (PortInfo portInfo in portInfos)
					{
						upnp.IGD.deletePortMapping(upnp, host, portInfo.port, portInfo.protocol);
					}
				}
				else
				{
					if (hosts.Count == 0)
					{
						foreach (PortInfo portInfo in portInfos)
						{
							upnp.IGD.deletePortMapping(upnp, ALL_REMOTE_HOSTS, portInfo.port, portInfo.protocol);
						}
					}
				}
			}
		}

		public virtual void addPort(int port, string protocol)
		{
			lock (this)
			{
				PortInfo portInfo = new PortInfo(port, protocol);
				if (portInfos.Contains(portInfo))
				{
					return;
				}
        
				if (SUPPORTS_MAPPING_FOR_MULTIPLE_REMOTE_HOSTS)
				{
					// All the new port mapping for all the hosts
					foreach (string host in hosts)
					{
						upnp.IGD.addPortMapping(upnp, host, port, protocol, port, LocalIPAddress, portDescription, portLeaseDuration);
					}
				}
				else
				{
					upnp.IGD.addPortMapping(upnp, ALL_REMOTE_HOSTS, port, protocol, port, LocalIPAddress, portDescription, portLeaseDuration);
				}
        
				portInfos.Add(portInfo);
			}
		}

		public virtual void removePort(int port, string protocol)
		{
			lock (this)
			{
				PortInfo portInfo = new PortInfo(port, protocol);
				if (!portInfos.Contains(portInfo))
				{
					return;
				}
        
				if (SUPPORTS_MAPPING_FOR_MULTIPLE_REMOTE_HOSTS)
				{
					// Remove the port mapping for all the hosts
					foreach (string host in hosts)
					{
						upnp.IGD.deletePortMapping(upnp, host, port, protocol);
					}
				}
				else
				{
					upnp.IGD.deletePortMapping(upnp, ALL_REMOTE_HOSTS, port, protocol);
				}
        
				portInfos.Remove(portInfo);
			}
		}

		public virtual void clear()
		{
			lock (this)
			{
				// Remove all the hosts
				while (hosts.Count > 0)
				{
					string host = hosts[0];
					removeHost(host);
				}
        
				// ...and remove all the ports
				while (portInfos.Count > 0)
				{
					PortInfo portInfo = portInfos[0];
					removePort(portInfo.port, portInfo.protocol);
				}
			}
		}
	}

}