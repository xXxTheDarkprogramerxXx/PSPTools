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

	using AdhocMessage = pspsharp.network.adhoc.AdhocMessage;
	using AdhocSocket = pspsharp.network.adhoc.AdhocSocket;
	using MatchingObject = pspsharp.network.adhoc.MatchingObject;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class ProOnlineMatchingObject : MatchingObject
	{
		internal ProOnlineNetworkAdapter proOnline;

		public ProOnlineMatchingObject(INetworkAdapter networkAdapter) : base(networkAdapter)
		{
			proOnline = (ProOnlineNetworkAdapter) networkAdapter;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected pspsharp.network.adhoc.AdhocSocket createSocket() throws java.net.UnknownHostException, java.io.IOException
		protected internal override AdhocSocket createSocket()
		{
			return new ProOnlineAdhocDatagramSocket(proOnline);
		}

		public override void create()
		{
			// Open the UDP port in the router
			proOnline.sceNetPortOpen("UDP", Port);

			base.create();
		}

		protected internal override bool isForMe(AdhocMessage adhocMessage, int port, InetAddress address)
		{
			return proOnline.isForMe(adhocMessage, port, address);
		}
	}

}