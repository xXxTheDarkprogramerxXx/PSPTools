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

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class AdhocServerStreamSocket : AdhocSocket
	{
		private ServerSocket serverSocket;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int bind(int port) throws java.io.IOException
		public override int bind(int port)
		{
			serverSocket = new ServerSocket(port);
			serverSocket.SoTimeout = 1;

			return serverSocket.LocalPort;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void connect(java.net.SocketAddress socketAddress, int port) throws java.io.IOException
		public override void connect(SocketAddress socketAddress, int port)
		{
			Console.WriteLine(string.Format("Connect not supported on ServerSocket: address={0}, port={1:D}", socketAddress, port));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void close()
		{
			if (serverSocket != null)
			{
				serverSocket.close();
				serverSocket = null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void setTimeout(int millis) throws java.net.SocketException
		public override int Timeout
		{
			set
			{
				serverSocket.SoTimeout = value;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void send(java.net.SocketAddress socketAddress, AdhocMessage adhocMessage) throws java.io.IOException
		public override void send(SocketAddress socketAddress, AdhocMessage adhocMessage)
		{
			Console.WriteLine(string.Format("Send not supported on ServerSocket: address={0}, message={1}", socketAddress, adhocMessage));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int receive(byte[] buffer, int size) throws java.io.IOException
		public override int receive(sbyte[] buffer, int size)
		{
			Console.WriteLine(string.Format("Receive not supported on ServerSocket"));
			return -1;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public AdhocSocket accept() throws java.io.IOException
		public override AdhocSocket accept()
		{
			Socket socket = serverSocket.accept();

			if (socket == null)
			{
				return null;
			}

			AdhocSocket adhocSocket = new AdhocStreamSocket(socket);
			// Provide information about the accepted socket
			adhocSocket.ReceivedAddress = socket.InetAddress;
			adhocSocket.ReceivedPort = socket.Port;

			return adhocSocket;
		}
	}

}