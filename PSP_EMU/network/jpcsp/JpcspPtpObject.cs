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
namespace pspsharp.network.pspsharp
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.adhoc.AdhocMessage.MAX_HEADER_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.pspsharp.JpcspAdhocPtpMessage.PTP_MESSAGE_TYPE_CONNECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.pspsharp.JpcspAdhocPtpMessage.PTP_MESSAGE_TYPE_CONNECT_CONFIRM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.pspsharp.JpcspAdhocPtpMessage.PTP_MESSAGE_TYPE_DATA;


	using Modules = pspsharp.HLE.Modules;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using AdhocDatagramSocket = pspsharp.network.adhoc.AdhocDatagramSocket;
	using AdhocMessage = pspsharp.network.adhoc.AdhocMessage;
	using AdhocSocket = pspsharp.network.adhoc.AdhocSocket;
	using PtpObject = pspsharp.network.adhoc.PtpObject;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class JpcspPtpObject : PtpObject
	{
		private JpcspAdhocPtpMessage connectRequest;
		private int connectRequestPort;
		private JpcspAdhocPtpMessage connectConfirm;
		private bool connected;
		private ISet<int> acceptedIds = new HashSet<int>();

		public JpcspPtpObject(PtpObject ptpObject) : base(ptpObject)
		{
		}

		public JpcspPtpObject(INetworkAdapter networkAdapter) : base(networkAdapter)
		{
		}

		public override bool canAccept()
		{
			return connectRequest != null;
		}

		public override bool canConnect()
		{
			return connectConfirm != null;
		}

		public override int connect(int timeout, int nonblock)
		{
			int result = 0;

			try
			{
				JpcspAdhocPtpMessage adhocPtpMessage = new JpcspAdhocPtpMessage(PTP_MESSAGE_TYPE_CONNECT);
				adhocPtpMessage.DataInt32 = Id;
				send(adhocPtpMessage);

				result = base.connect(timeout, nonblock);
			}
			catch (SocketException e)
			{
				log.error("connect", e);
			}
			catch (IOException e)
			{
				log.error("connect", e);
			}

			return result;
		}

		protected internal override bool pollAccept(int peerMacAddr, int peerPortAddr, SceKernelThreadInfo thread)
		{
			bool acceptCompleted = false;
			Memory mem = Memory.Instance;

			try
			{
				// Process a previously received connect message, if available
				JpcspAdhocPtpMessage adhocPtpMessage = connectRequest;
				int adhocPtpMessagePort = connectRequestPort;
				if (adhocPtpMessage == null)
				{
					sbyte[] bytes = new sbyte[BufSize + MAX_HEADER_SIZE];
					int length = socket.receive(bytes, bytes.Length);
					if (length > 0)
					{
						adhocPtpMessage = new JpcspAdhocPtpMessage(bytes, length);
						adhocPtpMessagePort = socket.ReceivedPort;

						if (log.DebugEnabled)
						{
							log.debug(string.Format("pollAccept: received message {0}", adhocPtpMessage));
						}
					}
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("pollAccept: processing pending message {0}", adhocPtpMessage));
					}
				}

				if (adhocPtpMessage != null && adhocPtpMessage.ForMe)
				{
					switch (adhocPtpMessage.Type)
					{
						case PTP_MESSAGE_TYPE_CONNECT:
							int acceptedId = adhocPtpMessage.DataInt32;
							if (acceptedIds.Contains(acceptedId))
							{
								if (log.DebugEnabled)
								{
									log.debug(string.Format("Connect message received for an id={0:D} already accepted. Dropping message.", acceptedId));
								}
							}
							else
							{
								pspNetMacAddress peerMacAddress = new pspNetMacAddress();
								peerMacAddress.MacAddress = adhocPtpMessage.FromMacAddress;
								int peerPort = Modules.sceNetAdhocModule.getClientPortFromRealPort(adhocPtpMessage.FromMacAddress, adhocPtpMessagePort);

								if (peerMacAddr != 0)
								{
									peerMacAddress.write(mem, peerMacAddr);
								}
								if (peerPortAddr != 0)
								{
									mem.write16(peerPortAddr, (short) peerPort);
								}

								// As a result of the "accept" call, create a new PTP Object
								PtpObject ptpObject = new JpcspPtpObject(this);
								ptpObject.DestMacAddress = peerMacAddress;
								ptpObject.DestPort = peerPort;
								ptpObject.Port = 0;
								Modules.sceNetAdhocModule.hleAddPtpObject(ptpObject);

								// Return the ID of the new PTP Object
								setReturnValue(thread, ptpObject.Id);

								// Remember that we have already accepted this Id.
								acceptedIds.Add(acceptedId);

								// Get a new free port
								ptpObject.Port = 0;
								ptpObject.openSocket();

								// Send a connect confirmation message including the new port
								JpcspAdhocPtpMessage confirmMessage = new JpcspAdhocPtpMessage(PTP_MESSAGE_TYPE_CONNECT_CONFIRM);
								confirmMessage.DataInt32 = ptpObject.Port;
								ptpObject.send(confirmMessage);

								if (log.DebugEnabled)
								{
									log.debug(string.Format("accept completed, creating new Ptp object {0}", ptpObject));
								}

								acceptCompleted = true;
								connectRequest = null;
							}
							break;
					}
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("pollAccept: received a message not for me: {0}", adhocPtpMessage));
					}
				}
			}
			catch (SocketException e)
			{
				log.error("pollAccept", e);
			}
			catch (SocketTimeoutException)
			{
				// Ignore exception
			}
			catch (IOException e)
			{
				log.error("pollAccept", e);
			}

			return acceptCompleted;
		}

		protected internal override bool pollConnect(SceKernelThreadInfo thread)
		{
			bool connectCompleted = false;

			try
			{
				// Process a previously received confirm message, if available
				JpcspAdhocPtpMessage adhocPtpMessage = connectConfirm;
				if (adhocPtpMessage == null)
				{
					sbyte[] bytes = new sbyte[BufSize + MAX_HEADER_SIZE];
					int length = socket.receive(bytes, bytes.Length);
					if (length > 0)
					{
						adhocPtpMessage = new JpcspAdhocPtpMessage(bytes, length);
						if (log.DebugEnabled)
						{
							log.debug(string.Format("pollConnect: received message {0}", adhocPtpMessage));
						}
					}
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("pollConnect: processing pending message {0}", adhocPtpMessage));
					}
				}

				if (adhocPtpMessage != null && adhocPtpMessage.ForMe)
				{
					switch (adhocPtpMessage.Type)
					{
						case PTP_MESSAGE_TYPE_CONNECT_CONFIRM:
							// Connect successfully completed, retrieve the new destination port
							int port = Modules.sceNetAdhocModule.getClientPortFromRealPort(adhocPtpMessage.FromMacAddress, adhocPtpMessage.DataInt32);
							if (log.DebugEnabled)
							{
								log.debug(string.Format("Received connect confirmation, changing destination port from {0:D} to {1:D}", DestPort, port));
							}
							DestPort = port;
							setReturnValue(thread, 0);
							connectConfirm = null;
							connectCompleted = true;
							break;
					}
				}
				else
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("pollConnect: received a message not for me: {0}", adhocPtpMessage));
					}
				}
			}
			catch (SocketException e)
			{
				log.error("pollConnect", e);
			}
			catch (SocketTimeoutException)
			{
				// Ignore exception
			}
			catch (IOException e)
			{
				log.error("pollConnect", e);
			}

			if (connectCompleted)
			{
				connected = true;
			}

			return connectCompleted;
		}

		protected internal override bool isForMe(AdhocMessage adhocMessage, int port, InetAddress address)
		{
			if (adhocMessage is JpcspAdhocPtpMessage)
			{
				JpcspAdhocPtpMessage adhocPtpMessage = (JpcspAdhocPtpMessage) adhocMessage;
				int type = adhocPtpMessage.Type;
				if (type == PTP_MESSAGE_TYPE_CONNECT_CONFIRM)
				{
					if (connected)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Received connect confirmation but already connected, discarding"));
						}
					}
					else
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Received connect confirmation, processing later"));
						}
						connectConfirm = adhocPtpMessage;
					}
					return false;
				}
				else if (type == PTP_MESSAGE_TYPE_CONNECT)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Received connect request, processing later"));
					}
					connectRequest = adhocPtpMessage;
					connectRequestPort = port;
					return false;
				}
				else if (type != PTP_MESSAGE_TYPE_DATA)
				{
					return false;
				}
			}

			return base.isForMe(adhocMessage, port, address);
		}

		protected internal override void closeSocket()
		{
			base.closeSocket();
			connected = false;
		}

		protected internal override AdhocSocket createSocket()
		{
			return new AdhocDatagramSocket();
		}
	}

}