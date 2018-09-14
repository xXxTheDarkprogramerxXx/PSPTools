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
//	import static pspsharp.HLE.modules.sceNet.convertMacAddressToString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.hardware.Wlan.MAC_ADDRESS_LENGTH;
	using AdhocMessage = pspsharp.network.adhoc.AdhocMessage;

	/// <summary>
	/// @author gid15
	/// 
	/// A JpcspAdhocPdpMessage is consisting of:
	/// - 6 bytes for the MAC address of the message sender
	/// - 6 bytes for the MAC address of the message recipient
	/// - 1 byte message type
	/// - n bytes for the message data
	/// </summary>
	public class JpcspAdhocPtpMessage : AdhocMessage
	{
		protected internal static readonly int HEADER_SIZE = MAC_ADDRESS_LENGTH + MAC_ADDRESS_LENGTH + 1;
		public const int PTP_MESSAGE_TYPE_CONNECT = 1;
		public const int PTP_MESSAGE_TYPE_CONNECT_CONFIRM = 2;
		public const int PTP_MESSAGE_TYPE_DATA = 3;
		protected internal sbyte type;

		public JpcspAdhocPtpMessage(int type) : base()
		{
			this.type = (sbyte) type;
		}

		public JpcspAdhocPtpMessage(int address, int length, int type) : base(address, length)
		{
			this.type = (sbyte) type;
		}

		public JpcspAdhocPtpMessage(sbyte[] message, int length) : base(message, length)
		{
		}

		public override sbyte[] Message
		{
			get
			{
				sbyte[] message = new sbyte[MessageLength];
				offset = 0;
				addToBytes(message, fromMacAddress);
				addToBytes(message, toMacAddress);
				addToBytes(message, type);
				addToBytes(message, data);
    
				return message;
			}
		}

		public override void setMessage(sbyte[] message, int length)
		{
			if (length >= HEADER_SIZE)
			{
				offset = 0;
				copyFromBytes(message, fromMacAddress);
				copyFromBytes(message, toMacAddress);
				type = copyByteFromBytes(message);
				data = new sbyte[length - HEADER_SIZE];
				copyFromBytes(message, data);
			}
		}

		public override int MessageLength
		{
			get
			{
				return base.MessageLength + HEADER_SIZE;
			}
		}

		public virtual int Type
		{
			get
			{
				return type;
			}
		}

		public override string ToString()
		{
			return string.Format("JpcspAdhocPtpMessage[fromMacAddress={0}, toMacAddress={1}, dataLength={2:D}, type={3:D}]", convertMacAddressToString(fromMacAddress), convertMacAddressToString(toMacAddress), DataLength, Type);
		}
	}

}