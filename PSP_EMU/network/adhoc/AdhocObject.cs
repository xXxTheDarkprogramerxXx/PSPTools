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
namespace pspsharp.network.adhoc
{

	using Logger = org.apache.log4j.Logger;

	using Modules = pspsharp.HLE.Modules;
	using SceUidManager = pspsharp.HLE.kernel.managers.SceUidManager;
	using SysMemUserForUser = pspsharp.HLE.modules.SysMemUserForUser;
	using sceNetAdhoc = pspsharp.HLE.modules.sceNetAdhoc;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public abstract class AdhocObject
	{
		protected internal static Logger log = sceNetAdhoc.log;
		private const string uidPurpose = "sceNetAdhoc";

		/// <summary>
		/// uid </summary>
		private readonly int id;
		private int port;
		protected internal AdhocSocket socket;
		/// <summary>
		/// Buffer size </summary>
		private int bufSize;
		protected internal SysMemUserForUser.SysMemInfo buffer;
		/// <summary>
		/// Network Adapter </summary>
		protected internal readonly INetworkAdapter networkAdapter;

		public AdhocObject(INetworkAdapter networkAdapter)
		{
			this.networkAdapter = networkAdapter;
			id = SceUidManager.getNewUid(uidPurpose);
		}

		public AdhocObject(AdhocObject adhocObject)
		{
			networkAdapter = adhocObject.networkAdapter;
			id = SceUidManager.getNewUid(uidPurpose);
			port = adhocObject.port;
			BufSize = adhocObject.bufSize;
		}

		public virtual int Id
		{
			get
			{
				return id;
			}
		}

		public virtual int Port
		{
			get
			{
				return port;
			}
			set
			{
				this.port = value;
			}
		}


		public virtual int BufSize
		{
			get
			{
				return bufSize;
			}
			set
			{
				this.bufSize = value;
				if (buffer != null)
				{
					Modules.SysMemUserForUserModule.free(buffer);
					buffer = null;
				}
				buffer = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.USER_PARTITION_ID, Modules.sceNetAdhocModule.Name, SysMemUserForUser.PSP_SMEM_Low, value, 0);
			}
		}


		public virtual void delete()
		{
			closeSocket();
			if (buffer != null)
			{
				Modules.SysMemUserForUserModule.free(buffer);
				buffer = null;
			}
			SceUidManager.releaseUid(id, uidPurpose);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void openSocket() throws java.net.UnknownHostException, java.io.IOException
		public virtual void openSocket()
		{
			if (socket == null)
			{
				socket = createSocket();
				if (Port == 0)
				{
					int localPort = socket.bind(Port);
					Port = localPort;
				}
				else
				{
					int realPort = Modules.sceNetAdhocModule.getRealPortFromServerPort(Port);
					socket.bind(realPort);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected abstract AdhocSocket createSocket() throws java.net.UnknownHostException, java.io.IOException;
		protected internal abstract AdhocSocket createSocket();

		protected internal virtual void closeSocket()
		{
			if (socket != null)
			{
				try
				{
					socket.close();
				}
				catch (IOException e)
				{
					log.error("Error while closing Adhoc socket", e);
				}
				socket = null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void setTimeout(int timeout, int nonblock) throws java.net.SocketException
		protected internal virtual void setTimeout(int timeout, int nonblock)
		{
			if (nonblock != 0)
			{
				socket.Timeout = 1;
			}
			else
			{
				// SoTimeout accepts milliseconds, PSP timeout is given in microseconds
				socket.Timeout = System.Math.Max(timeout / 1000, 1);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void send(AdhocMessage adhocMessage) throws java.io.IOException
		protected internal virtual void send(AdhocMessage adhocMessage)
		{
			send(adhocMessage, Port);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void send(AdhocMessage adhocMessage, int destPort) throws java.io.IOException
		protected internal virtual void send(AdhocMessage adhocMessage, int destPort)
		{
			if (adhocMessage == null)
			{
				// Nothing to send
				return;
			}

			openSocket();

			int realPort = Modules.sceNetAdhocModule.getRealPortFromClientPort(adhocMessage.ToMacAddress, destPort);
			SocketAddress[] socketAddress = Modules.sceNetAdhocModule.getMultiSocketAddress(adhocMessage.ToMacAddress, realPort);
			for (int i = 0; i < socketAddress.Length; i++)
			{
				socket.send(socketAddress[i], adhocMessage);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("Successfully sent {0:D} bytes to {1}, port {2:D}({3:D}): {4}", adhocMessage.DataLength, socketAddress[i], destPort, realPort, adhocMessage));
				}
			}
		}

		public virtual AdhocSocket Socket
		{
			set
			{
				this.socket = value;
			}
		}
	}

}