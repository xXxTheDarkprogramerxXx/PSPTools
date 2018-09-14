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
//	import static pspsharp.network.protocols.NetPacket.getIpAddressString;

	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;

	public class ARP
	{
		// See https://en.wikipedia.org/wiki/Address_Resolution_Protocol
		public const int ARP_OPERATION_REQUEST = 1;
		public const int ARP_OPERATION_REPLY = 2;
		private static readonly string[] ARP_OPERATION_NAMES = new string[] {null, "REQUEST", "REPLY"};
		public int hardwareType;
		public int protocolType;
		public int hardwareAddressLength;
		public int protocolAddressLength;
		public int operation;
		public pspNetMacAddress senderHardwareAddress;
		public sbyte[] senderProtocolAddress;
		public pspNetMacAddress targetHardwareAddress;
		public sbyte[] targetProtocolAddress;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(NetPacket packet) throws java.io.EOFException
		public virtual void read(NetPacket packet)
		{
			hardwareType = packet.read16();
			protocolType = packet.read16();
			hardwareAddressLength = packet.read8();
			protocolAddressLength = packet.read8();
			operation = packet.read16();
			senderHardwareAddress = packet.readMacAddress(hardwareAddressLength);
			senderProtocolAddress = packet.readIpAddress(protocolAddressLength);
			targetHardwareAddress = packet.readMacAddress(hardwareAddressLength);
			targetProtocolAddress = packet.readIpAddress(protocolAddressLength);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write(NetPacket packet) throws java.io.EOFException
		public virtual NetPacket write(NetPacket packet)
		{
			packet.write16(hardwareType);
			packet.write16(protocolType);
			packet.write8(hardwareAddressLength);
			packet.write8(protocolAddressLength);
			packet.write16(operation);
			packet.writeMacAddress(senderHardwareAddress, hardwareAddressLength);
			packet.writeIpAddress(senderProtocolAddress, protocolAddressLength);
			packet.writeMacAddress(targetHardwareAddress, hardwareAddressLength);
			packet.writeIpAddress(targetProtocolAddress, protocolAddressLength);

			return packet;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NetPacket write() throws java.io.EOFException
		public virtual NetPacket write()
		{
			return write(new NetPacket(sizeOf()));
		}

		public virtual int sizeOf()
		{
			return 8 + 2 * (hardwareAddressLength + protocolAddressLength);
		}

		public override string ToString()
		{
			return string.Format("operation={0}, sender={1}/{2}, target={3}/{4}", ARP_OPERATION_NAMES[operation], senderHardwareAddress, getIpAddressString(senderProtocolAddress), targetHardwareAddress, getIpAddressString(targetProtocolAddress));
		}
	}

}