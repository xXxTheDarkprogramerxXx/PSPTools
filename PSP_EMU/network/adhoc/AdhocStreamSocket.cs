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
	public class AdhocStreamSocket : AdhocSocket
	{
		private Socket socket;

		public AdhocStreamSocket()
		{
		}

		protected internal AdhocStreamSocket(Socket socket)
		{
			this.socket = socket;

			try
			{
				socket.SoTimeout = 1;
			}
			catch (SocketException)
			{
				// Ignore error
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int bind(int port) throws java.io.IOException
		public override int bind(int port)
		{
			socket = new Socket();
			socket.bind(new InetSocketAddress(port));
			socket.SoTimeout = 1;

			return socket.LocalPort;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void close() throws java.io.IOException
		public override void close()
		{
			if (socket != null)
			{
				socket.close();
				socket = null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void setTimeout(int millis) throws java.net.SocketException
		public override int Timeout
		{
			set
			{
				socket.SoTimeout = value;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void send(java.net.SocketAddress socketAddress, AdhocMessage adhocMessage) throws java.io.IOException
		public override void send(SocketAddress socketAddress, AdhocMessage adhocMessage)
		{
			socket.OutputStream.write(adhocMessage.Message);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public int receive(byte[] buffer, int size) throws java.io.IOException
		public override int receive(sbyte[] buffer, int size)
		{
			return socket.InputStream.read(buffer, 0, size);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void connect(java.net.SocketAddress socketAddress, int port) throws java.io.IOException
		public override void connect(SocketAddress socketAddress, int port)
		{
			socket.connect(socketAddress);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public AdhocSocket accept() throws java.io.IOException
		public override AdhocSocket accept()
		{
			// Accept not supported on non-server sockets
			return null;
		}
	}

}