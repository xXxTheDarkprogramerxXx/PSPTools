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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNet.convertMacAddressToString;

	using Logger = org.apache.log4j.Logger;

	using AdhocMatchingEventMessage = pspsharp.network.adhoc.AdhocMatchingEventMessage;
	using MatchingObject = pspsharp.network.adhoc.MatchingObject;

	/// <summary>
	/// @author gid15
	/// 
	/// A generic ProOnlineAdhocMatchingEventMessage is consisting of:
	/// - 1 byte for the event
	/// - 4 bytes for the message data length
	/// - n bytes for the message data
	/// </summary>
	public class ProOnlineAdhocMatchingEventMessage : AdhocMatchingEventMessage
	{
		protected internal static Logger log = ProOnlineNetworkAdapter.log;
		protected internal const int HEADER_SIZE = 1 + 4;
		private int packetOpcode;

		public ProOnlineAdhocMatchingEventMessage(MatchingObject matchingObject, int @event, int packetOpcode) : base(matchingObject, @event)
		{
			this.packetOpcode = packetOpcode;
		}

		public ProOnlineAdhocMatchingEventMessage(MatchingObject matchingObject, int @event, int packetOpcode, int address, int length, sbyte[] toMacAddress) : base(matchingObject, @event, address, length, toMacAddress)
		{
			this.packetOpcode = packetOpcode;
		}

		public ProOnlineAdhocMatchingEventMessage(MatchingObject matchingObject, int @event, sbyte[] message, int length) : base(matchingObject, message, length)
		{
			Event = @event;
		}

		public override sbyte[] Message
		{
			get
			{
				sbyte[] message = new sbyte[MessageLength];
				offset = 0;
				addToBytes(message, (sbyte) PacketOpcode);
				addInt32ToBytes(message, DataLength);
				addToBytes(message, data);
    
				return message;
			}
		}

		public override void setMessage(sbyte[] message, int length)
		{
			if (length >= HEADER_SIZE)
			{
				offset = 0;
				PacketOpcode = copyByteFromBytes(message);
				int dataLength = copyInt32FromBytes(message);
				data = new sbyte[System.Math.Min(dataLength, length - HEADER_SIZE)];
				copyFromBytes(message, data);
			}
		}

		protected internal virtual int PacketOpcode
		{
			get
			{
				return packetOpcode;
			}
			set
			{
				this.packetOpcode = value;
			}
		}


		public override int MessageLength
		{
			get
			{
				return base.MessageLength + HEADER_SIZE;
			}
		}

		public override string ToString()
		{
			return string.Format("{0}[fromMacAddress={1}, toMacAddress={2}, dataLength={3:D}, event={4:D}, packetOpcode={5:D}]", this.GetType().Name, convertMacAddressToString(fromMacAddress), convertMacAddressToString(toMacAddress), DataLength, Event, PacketOpcode);
		}
	}

}