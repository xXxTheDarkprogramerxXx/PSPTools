using System.Text;

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
namespace pspsharp.network.protocols
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.hardware.Wlan.MAC_ADDRESS_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.accesspoint.AccessPoint.IP_ADDRESS_LENGTH;

	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using BytesPacket = pspsharp.util.BytesPacket;

	public class NetPacket : BytesPacket
	{
		public NetPacket(int length) : base(length)
		{
			setBigEndian();
		}

		public NetPacket(sbyte[] buffer) : base(buffer)
		{
			setBigEndian();
		}

		public NetPacket(sbyte[] buffer, int length) : base(buffer, length)
		{
			setBigEndian();
		}

		public NetPacket(sbyte[] buffer, int offset, int length) : base(buffer, offset, length)
		{
			setBigEndian();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public pspsharp.HLE.kernel.types.pspNetMacAddress readMacAddress() throws java.io.EOFException
		public virtual pspNetMacAddress readMacAddress()
		{
			return readMacAddress(MAC_ADDRESS_LENGTH);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public pspsharp.HLE.kernel.types.pspNetMacAddress readMacAddress(int length) throws java.io.EOFException
		public virtual pspNetMacAddress readMacAddress(int length)
		{
			pspNetMacAddress macAddress = new pspNetMacAddress();
			readBytes(macAddress.macAddress, 0, System.Math.Min(length, MAC_ADDRESS_LENGTH));
			skip8(length - MAC_ADDRESS_LENGTH);
			return macAddress;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte[] readIpAddress() throws java.io.EOFException
		public virtual sbyte[] readIpAddress()
		{
			return readIpAddress(IP_ADDRESS_LENGTH);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public byte[] readIpAddress(int length) throws java.io.EOFException
		public virtual sbyte[] readIpAddress(int length)
		{
			return readBytes(new sbyte[length]);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String readDnsNameNotation() throws java.io.EOFException
		public virtual string readDnsNameNotation()
		{
			StringBuilder name = new StringBuilder();

			while (true)
			{
				int numberBytes = read8();
				if (numberBytes == 0)
				{
					break;
				}

				if (name.Length > 0)
				{
					name.Append('.');
				}

				for (int i = 0; i < numberBytes; i++)
				{
					name.Append(readAsciiChar());
				}
			}

			return name.ToString();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeDnsNameNotation(String name) throws java.io.EOFException
		public virtual void writeDnsNameNotation(string name)
		{
			if (!string.ReferenceEquals(name, null) && name.Length > 0)
			{
				string[] parts = name.Split("\\.", true);
				if (parts != null && parts.Length > 0)
				{
					foreach (string part in parts)
					{
						int length = part.Length;
						if (length > 0)
						{
							write8(length);
							for (int i = 0; i < length; i++)
							{
								writeAsciiChar(part[i]);
							}
						}
					}
				}
			}

			write8(0);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeMacAddress(pspsharp.HLE.kernel.types.pspNetMacAddress macAddress) throws java.io.EOFException
		public virtual void writeMacAddress(pspNetMacAddress macAddress)
		{
			writeMacAddress(macAddress, MAC_ADDRESS_LENGTH);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeMacAddress(pspsharp.HLE.kernel.types.pspNetMacAddress macAddress, int length) throws java.io.EOFException
		public virtual void writeMacAddress(pspNetMacAddress macAddress, int length)
		{
			writeBytes(macAddress.macAddress, 0, System.Math.Min(length, MAC_ADDRESS_LENGTH));
			skip8(length - MAC_ADDRESS_LENGTH);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeIpAddress(byte[] ip) throws java.io.EOFException
		public virtual void writeIpAddress(sbyte[] ip)
		{
			writeIpAddress(ip, IP_ADDRESS_LENGTH);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeIpAddress(byte[] ip, int length) throws java.io.EOFException
		public virtual void writeIpAddress(sbyte[] ip, int length)
		{
			writeBytes(ip, 0, length);
		}

		public static string getIpAddressString(sbyte[] ip)
		{
			return string.Format("{0:D}.{1:D}.{2:D}.{3:D}", ip[0] & 0xFF, ip[1] & 0xFF, ip[2] & 0xFF, ip[3] & 0xFF);
		}
	}

}