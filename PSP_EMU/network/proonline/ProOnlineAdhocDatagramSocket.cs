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

	using AdhocDatagramSocket = pspsharp.network.adhoc.AdhocDatagramSocket;
	using AdhocMessage = pspsharp.network.adhoc.AdhocMessage;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class ProOnlineAdhocDatagramSocket : AdhocDatagramSocket
	{
		private ProOnlineNetworkAdapter proOnline;

		public ProOnlineAdhocDatagramSocket(ProOnlineNetworkAdapter proOnline)
		{
			this.proOnline = proOnline;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void send(java.net.SocketAddress socketAddress, pspsharp.network.adhoc.AdhocMessage adhocMessage) throws java.io.IOException
		public override void send(SocketAddress socketAddress, AdhocMessage adhocMessage)
		{
			if (proOnline.isBroadcast(socketAddress))
			{
				int port = proOnline.getBroadcastPort(socketAddress);
				// Broadcast to all MAC's/IP's
				int numberMacIps = proOnline.NumberMacIps;
				for (int i = 0; i < numberMacIps; i++)
				{
					MacIp macIp = proOnline.getMacIp(i);
					if (macIp != null)
					{
						SocketAddress remoteSocketAddress = proOnline.getSocketAddress(macIp.mac, port);
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Sending broadcasted message to {0}: {1}", macIp, adhocMessage));
						}
						base.send(remoteSocketAddress, adhocMessage);
					}
				}
			}
			else
			{
				base.send(socketAddress, adhocMessage);
			}
		}
	}

}