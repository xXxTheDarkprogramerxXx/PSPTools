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

	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using AdhocMessage = pspsharp.network.adhoc.AdhocMessage;
	using AdhocSocket = pspsharp.network.adhoc.AdhocSocket;
	using PdpObject = pspsharp.network.adhoc.PdpObject;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class ProOnlinePdpObject : PdpObject
	{
		protected internal readonly ProOnlineNetworkAdapter proOnline;
		private const string socketProtocol = "UDP";

		public ProOnlinePdpObject(INetworkAdapter networkAdapter) : base(networkAdapter)
		{
			proOnline = (ProOnlineNetworkAdapter) networkAdapter;
		}

		protected internal override AdhocSocket createSocket()
		{
			return new ProOnlineAdhocDatagramSocket(proOnline);
		}

		public override int create(pspNetMacAddress macAddress, int port, int bufSize)
		{
			// Open the UDP port in the router
			proOnline.sceNetPortOpen(socketProtocol, port);

			return base.create(macAddress, port, bufSize);
		}

		public override void delete()
		{
			// Close the UDP port in the router
			proOnline.sceNetPortClose(socketProtocol, Port);

			base.delete();
		}

		protected internal override bool isForMe(AdhocMessage adhocMessage, int port, InetAddress address)
		{
			return proOnline.isForMe(adhocMessage, port, address);
		}
	}

}