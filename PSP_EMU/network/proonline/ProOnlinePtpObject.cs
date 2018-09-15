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

	using Modules = pspsharp.HLE.Modules;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using AdhocMessage = pspsharp.network.adhoc.AdhocMessage;
	using AdhocServerStreamSocket = pspsharp.network.adhoc.AdhocServerStreamSocket;
	using AdhocSocket = pspsharp.network.adhoc.AdhocSocket;
	using AdhocStreamSocket = pspsharp.network.adhoc.AdhocStreamSocket;
	using PtpObject = pspsharp.network.adhoc.PtpObject;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class ProOnlinePtpObject : PtpObject
	{
		protected internal readonly ProOnlineNetworkAdapter proOnline;
		private const string socketProtocol = "TCP";
		protected internal bool isServerSocket;

		public ProOnlinePtpObject(ProOnlinePtpObject ptpObject) : base(ptpObject)
		{
			proOnline = ptpObject.proOnline;
		}

		public ProOnlinePtpObject(INetworkAdapter networkAdapter) : base(networkAdapter)
		{
			proOnline = (ProOnlineNetworkAdapter) networkAdapter;
		}

		protected internal override bool pollAccept(int peerMacAddr, int peerPortAddr, SceKernelThreadInfo thread)
		{
			bool acceptCompleted = false;

			try
			{
				AdhocSocket acceptedSocket = socket.accept();
				if (acceptedSocket != null)
				{
					sbyte[] destMacAddress = proOnline.getMacAddress(acceptedSocket.ReceivedAddress);
					if (destMacAddress != null)
					{
						// Return the accepted peer address and port
						pspNetMacAddress peerMacAddress = new pspNetMacAddress(destMacAddress);
						int peerPort = acceptedSocket.ReceivedPort;
						Memory mem = Memory.Instance;
						if (peerMacAddr != 0)
						{
							peerMacAddress.write(mem, peerMacAddr);
						}
						if (peerPortAddr != 0)
						{
							mem.write16(peerPortAddr, (short) peerPort);
						}

						// As a result of the "accept" call, create a new PTP Object
						PtpObject ptpObject = new ProOnlinePtpObject(this);
						// Add information about the accepted peer address and port
						ptpObject.DestMacAddress = peerMacAddress;
						ptpObject.DestPort = peerPort;
						ptpObject.Socket = acceptedSocket;

						// Add the received socket as a new Ptp Object
						Modules.sceNetAdhocModule.hleAddPtpObject(ptpObject);

						// Return the ID of the new PTP Object
						setReturnValue(thread, ptpObject.Id);

						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("accept completed, creating new Ptp object {0}", ptpObject));
						}

						acceptCompleted = true;
					}
				}
			}
			catch (SocketTimeoutException)
			{
				// Ignore exception
			}
			catch (IOException e)
			{
				Console.WriteLine("pollAccept", e);
			}

			return acceptCompleted;
		}

		protected internal override bool pollConnect(SceKernelThreadInfo thread)
		{
			// A StreamSocket is always connected
			return true;
		}

		public override bool canAccept()
		{
			// TODO Auto-generated method stub
			return true;
		}

		public override bool canConnect()
		{
			// A StreamSocket is always connected
			return true;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected pspsharp.network.adhoc.AdhocSocket createSocket() throws java.net.UnknownHostException, java.io.IOException
		protected internal override AdhocSocket createSocket()
		{
			return isServerSocket ? new AdhocServerStreamSocket() : new AdhocStreamSocket();
		}

		public override int open()
		{
			// Open the TCP port in the router
			proOnline.sceNetPortOpen(socketProtocol, Port);

			// This is a normal socket, no server socket
			isServerSocket = false;

			return base.open();
		}

		public override int listen()
		{
			// Open the TCP port in the router
			proOnline.sceNetPortOpen(socketProtocol, Port);

			// This is a server socket
			isServerSocket = true;

			return base.listen();
		}

		public override void delete()
		{
			// Close the TCP port in the router
			proOnline.sceNetPortClose(socketProtocol, Port);

			base.delete();
		}

		protected internal override bool isForMe(AdhocMessage adhocMessage, int port, InetAddress address)
		{
			// Always for me on stream sockets
			return true;
		}
	}

}