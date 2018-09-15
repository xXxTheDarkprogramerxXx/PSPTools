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

	using sceNetAdhoc = pspsharp.HLE.modules.sceNetAdhoc;

	//using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public abstract class AdhocSocket
	{
		protected internal static Logger log = sceNetAdhoc.log;
		private int receivedPort;
		private InetAddress receivedAddress;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract int bind(int port) throws java.io.IOException;
		public abstract int bind(int port);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void connect(java.net.SocketAddress socketAddress, int port) throws java.io.IOException;
		public abstract void connect(SocketAddress socketAddress, int port);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void close() throws java.io.IOException;
		public abstract void close();
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void setTimeout(int millis) throws java.net.SocketException;
		public abstract int Timeout {set;}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void send(java.net.SocketAddress socketAddress, AdhocMessage adhocMessage) throws java.io.IOException;
		public abstract void send(SocketAddress socketAddress, AdhocMessage adhocMessage);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract int receive(byte[] buffer, int size) throws java.io.IOException;
		public abstract int receive(sbyte[] buffer, int size);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract AdhocSocket accept() throws java.io.IOException;
		public abstract AdhocSocket accept();

		public virtual int ReceivedPort
		{
			get
			{
				return receivedPort;
			}
			set
			{
				this.receivedPort = value;
			}
		}


		public virtual InetAddress ReceivedAddress
		{
			get
			{
				return receivedAddress;
			}
			set
			{
				this.receivedAddress = value;
			}
		}

	}

}