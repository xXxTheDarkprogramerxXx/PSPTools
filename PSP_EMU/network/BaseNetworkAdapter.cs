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
namespace pspsharp.network
{

	using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public abstract class BaseNetworkAdapter : INetworkAdapter
	{
		public abstract void sceNetAdhocctlScan();
		public abstract void sceNetAdhocctlDisconnect();
		public abstract void sceNetAdhocctlJoin();
		public abstract void sceNetAdhocctlCreate();
		public abstract void sceNetAdhocctlConnect();
		public abstract void sceNetAdhocctlTerm();
		public abstract void sceNetAdhocctlInit();
		public abstract void updatePeers();
		public abstract void sendChatMessage(string message);
		public abstract bool ConnectComplete {get;}
		public abstract pspsharp.network.adhoc.AdhocMatchingEventMessage createAdhocMatchingEventMessage(adhoc.MatchingObject matchingObject, sbyte[] message, int length);
		public abstract pspsharp.network.adhoc.AdhocMatchingEventMessage createAdhocMatchingEventMessage(adhoc.MatchingObject matchingObject, int @event, int data, int dataLength, sbyte[] macAddress);
		public abstract pspsharp.network.adhoc.AdhocMatchingEventMessage createAdhocMatchingEventMessage(adhoc.MatchingObject matchingObject, int @event);
		public abstract pspsharp.network.adhoc.MatchingObject createMatchingObject();
		public abstract SocketAddress getSocketAddress(sbyte[] macAddress, int realPort);
		public abstract pspsharp.network.adhoc.AdhocMessage createAdhocGameModeMessage(sbyte[] message, int length);
		public abstract pspsharp.network.adhoc.AdhocMessage createAdhocGameModeMessage(pspsharp.HLE.modules.sceNetAdhoc.GameModeArea gameModeArea);
		public abstract pspsharp.network.adhoc.AdhocMessage createAdhocPtpMessage(sbyte[] message, int length);
		public abstract pspsharp.network.adhoc.AdhocMessage createAdhocPtpMessage(int address, int length);
		public abstract pspsharp.network.adhoc.AdhocMessage createAdhocPdpMessage(sbyte[] message, int length);
		public abstract pspsharp.network.adhoc.AdhocMessage createAdhocPdpMessage(int address, int length, sbyte[] destMacAddress);
		public abstract pspsharp.network.adhoc.PtpObject createPtpObject();
		public abstract pspsharp.network.adhoc.PdpObject createPdpObject();
		protected internal static Logger log = Logger.getLogger("network");

		public virtual void start()
		{
		}

		public virtual void stop()
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public java.net.SocketAddress[] getMultiSocketAddress(byte[] macAddress, int realPort) throws java.net.UnknownHostException
		public virtual SocketAddress[] getMultiSocketAddress(sbyte[] macAddress, int realPort)
		{
			SocketAddress[] socketAddresses = new SocketAddress[1];
			socketAddresses[0] = getSocketAddress(macAddress, realPort);

			return socketAddresses;
		}
	}

}