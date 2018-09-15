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

	using GameModeArea = pspsharp.HLE.modules.sceNetAdhoc.GameModeArea;
	using AdhocMatchingEventMessage = pspsharp.network.adhoc.AdhocMatchingEventMessage;
	using AdhocMessage = pspsharp.network.adhoc.AdhocMessage;
	using MatchingObject = pspsharp.network.adhoc.MatchingObject;
	using PdpObject = pspsharp.network.adhoc.PdpObject;
	using PtpObject = pspsharp.network.adhoc.PtpObject;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public interface INetworkAdapter
	{
		/// <summary>
		/// Called when starting the PSP sceNet module.
		/// </summary>
		void start();

		/// <summary>
		/// Called when stopping the PSP sceNet module.
		/// </summary>
		void stop();

		/// <summary>
		/// Create a new Pdp object </summary>
		/// <returns> a new Pdp object </returns>
		PdpObject createPdpObject();

		/// <summary>
		/// Create a new Ptp object </summary>
		/// <returns> a new Ptp object </returns>
		PtpObject createPtpObject();

		/// <summary>
		/// Create an Adhoc Pdp message from PSP memory data </summary>
		/// <param name="address">        the address for the data part of the Pdp message </param>
		/// <param name="Length">         the Length of the data part of the Pdp message </param>
		/// <param name="destMacAddress"> the destination MAC address </param>
		/// <returns>               an AdhocMessage </returns>
		AdhocMessage createAdhocPdpMessage(int address, int Length, sbyte[] destMacAddress);

		/// <summary>
		/// Create an Adhoc Pdp message from a network packet </summary>
		/// <param name="message">  the network packet received </param>
		/// <param name="Length">   the Length of the message </param>
		/// <returns>         an AdhocMessage </returns>
		AdhocMessage createAdhocPdpMessage(sbyte[] message, int Length);

		/// <summary>
		/// Create an Adhoc Ptp message from PSP memory data </summary>
		/// <param name="address"> the address for the data part of the Pdp message </param>
		/// <param name="Length">  the Length of the data part of the Pdp message </param>
		/// <returns>        an AdhocMessage </returns>
		AdhocMessage createAdhocPtpMessage(int address, int Length);

		/// <summary>
		/// Create an Adhoc Ptp message from a network packet </summary>
		/// <param name="message">  the network packet received </param>
		/// <param name="Length">   the Length of the message </param>
		/// <returns>         an AdhocMessage </returns>
		AdhocMessage createAdhocPtpMessage(sbyte[] message, int Length);

		/// <summary>
		/// Create an Adhoc GameMode message from a PSP GameModeArea </summary>
		/// <param name="gameModeArea"> the GameMode area </param>
		/// <returns>             an AdhocMessage </returns>
		AdhocMessage createAdhocGameModeMessage(GameModeArea gameModeArea);

		/// <summary>
		/// Create an Adhoc GameMode message from a network packet </summary>
		/// <param name="message">  the network packet received </param>
		/// <param name="Length">   the Length of the message </param>
		/// <returns>         an AdhocMessage </returns>
		AdhocMessage createAdhocGameModeMessage(sbyte[] message, int Length);

		/// <summary>
		/// Get the SocketAddress for the given MAC address and port. </summary>
		/// <param name="macAddress">  the MAC address </param>
		/// <param name="port">        the real port number (i.e. the shifted port if port shifting is active) </param>
		/// <returns>            the corresponding SocketAddress </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.net.SocketAddress getSocketAddress(byte[] macAddress, int realPort) throws java.net.UnknownHostException;
		SocketAddress getSocketAddress(sbyte[] macAddress, int realPort);

		/// <summary>
		/// Get the SocketAddress(es) for the given MAC address and port.
		/// Multiple Socket addresses can be returned. </summary>
		/// <param name="macAddress">  the MAC address </param>
		/// <param name="port">        the real port number (i.e. the shifted port if port shifting is active) </param>
		/// <returns>            the corresponding SocketAddress(es) </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.net.SocketAddress[] getMultiSocketAddress(byte[] macAddress, int realPort) throws java.net.UnknownHostException;
		SocketAddress[] getMultiSocketAddress(sbyte[] macAddress, int realPort);

		/// <summary>
		/// Create a new Matching object </summary>
		/// <returns> a new Matching object </returns>
		MatchingObject createMatchingObject();

		/// <summary>
		/// Create an Adhoc Matching message for an event. </summary>
		/// <param name="event"> the event </param>
		/// <returns>      the Adhoc Matching message for the event </returns>
		AdhocMatchingEventMessage createAdhocMatchingEventMessage(MatchingObject matchingObject, int @event);

		/// <summary>
		/// Create an Adhoc Matching message for an event with additional data. </summary>
		/// <param name="event">      the event </param>
		/// <param name="data">       the address of the additional data </param>
		/// <param name="dataLength"> the Length of the additional data </param>
		/// <param name="macAddress"> the destination MAC address </param>
		/// <returns>           the new Adhoc Matching message </returns>
		AdhocMatchingEventMessage createAdhocMatchingEventMessage(MatchingObject matchingObject, int @event, int data, int dataLength, sbyte[] macAddress);

		/// <summary>
		/// Create an Adhoc Matching message from a network packet </summary>
		/// <param name="message">  the network packet received </param>
		/// <param name="Length">   the Length of the message </param>
		/// <returns>         an Adhoc Matching </returns>
		AdhocMatchingEventMessage createAdhocMatchingEventMessage(MatchingObject matchingObject, sbyte[] message, int Length);

		/// <summary>
		/// When connecting or joining to a group, check when the CONNECTED state can be reached. </summary>
		/// <returns> true if the CONNECTED state can be reached
		///         false if still processing the connection </returns>
		bool ConnectComplete {get;}

		/// <summary>
		/// Send a chat message to the network group </summary>
		/// <param name="message"> the chat message to send </param>
		void sendChatMessage(string message);

		/// <summary>
		/// Called at regular intervals to keep the peers up-to-date.
		/// </summary>
		void updatePeers();

		/// <summary>
		/// Called when executing sceNetAdhocctlInit.
		/// </summary>
		void sceNetAdhocctlInit();

		/// <summary>
		/// Called when executing sceNetAdhocctlTerm.
		/// </summary>
		void sceNetAdhocctlTerm();

		/// <summary>
		/// Called when executing sceNetAdhocctlConnect.
		/// </summary>
		void sceNetAdhocctlConnect();

		/// <summary>
		/// Called when executing sceNetAdhocctlCreate.
		/// </summary>
		void sceNetAdhocctlCreate();

		/// <summary>
		/// Called when executing sceNetAdhocctlJoin.
		/// </summary>
		void sceNetAdhocctlJoin();

		/// <summary>
		/// Called when executing sceNetAdhocctlDisconnect.
		/// </summary>
		void sceNetAdhocctlDisconnect();

		/// <summary>
		/// Called when executing sceNetAdhocctlScan.
		/// </summary>
		void sceNetAdhocctlScan();
	}

}