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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_ACCEPT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_CANCEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_COMPLETE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_DATA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_DISCONNECT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_HELLO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_INTERNAL_PING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_JOIN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.hardware.Wlan.MAC_ADDRESS_LENGTH;
	using sceNetAdhocMatching = pspsharp.HLE.modules.sceNetAdhocMatching;
	using MatchingObject = pspsharp.network.adhoc.MatchingObject;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class MatchingPacketFactory
	{
		public const int ADHOC_MATCHING_PACKET_PING = 0;
		public const int ADHOC_MATCHING_PACKET_HELLO = 1;
		public const int ADHOC_MATCHING_PACKET_JOIN = 2;
		public const int ADHOC_MATCHING_PACKET_ACCEPT = 3;
		public const int ADHOC_MATCHING_PACKET_CANCEL = 4;
		public const int ADHOC_MATCHING_PACKET_BULK = 5;
		public const int ADHOC_MATCHING_PACKET_BULK_ABORT = 6;
		public const int ADHOC_MATCHING_PACKET_BIRTH = 7;
		public const int ADHOC_MATCHING_PACKET_DEATH = 8;
		public const int ADHOC_MATCHING_PACKET_BYE = 9;

		private abstract class MatchingPacketOpcode : ProOnlineAdhocMatchingEventMessage
		{
			public MatchingPacketOpcode(MatchingObject matchingObject, int @event, sbyte[] message, int length) : base(matchingObject, @event, message, length)
			{
			}

			public MatchingPacketOpcode(MatchingObject matchingObject, int @event, int packetOpcode) : base(matchingObject, @event, packetOpcode)
			{
			}

			public override sbyte[] Message
			{
				get
				{
					sbyte[] message = new sbyte[MessageLength];
					offset = 0;
					addToBytes(message, (sbyte) PacketOpcode);
    
					return message;
				}
			}

			public override void setMessage(sbyte[] message, int length)
			{
				if (length >= MessageLength)
				{
					offset = 0;
					PacketOpcode = copyByteFromBytes(message);
				}
			}

			public override int MessageLength
			{
				get
				{
					return 1;
				}
			}
		}

		private class MatchingPacketPing : MatchingPacketOpcode
		{
			public MatchingPacketPing(MatchingObject matchingObject) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_INTERNAL_PING, ADHOC_MATCHING_PACKET_PING)
			{
			}

			public MatchingPacketPing(MatchingObject matchingObject, sbyte[] message, int length) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_INTERNAL_PING, message, length)
			{
			}
		}

		private class MatchingPacketHello : ProOnlineAdhocMatchingEventMessage
		{
			public MatchingPacketHello(MatchingObject matchingObject, int address, int length, sbyte[] toMacAddress) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_HELLO, ADHOC_MATCHING_PACKET_HELLO, address, length, toMacAddress)
			{
			}

			public MatchingPacketHello(MatchingObject matchingObject) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_HELLO, ADHOC_MATCHING_PACKET_HELLO)
			{
			}

			public MatchingPacketHello(MatchingObject matchingObject, sbyte[] message, int length) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_HELLO, message, length)
			{
			}
		}

		private class MatchingPacketJoin : ProOnlineAdhocMatchingEventMessage
		{
			public MatchingPacketJoin(MatchingObject matchingObject, int address, int length, sbyte[] toMacAddress) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_JOIN, ADHOC_MATCHING_PACKET_JOIN, address, length, toMacAddress)
			{
			}

			public MatchingPacketJoin(MatchingObject matchingObject) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_JOIN, ADHOC_MATCHING_PACKET_JOIN)
			{
			}

			public MatchingPacketJoin(MatchingObject matchingObject, sbyte[] message, int length) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_JOIN, message, length)
			{
			}
		}

		/// <summary>
		/// @author gid15
		/// 
		/// A MatchingPacketAccept is consisting of:
		/// - 1 byte for the event
		/// - 4 bytes for the message data length
		/// - 4 bytes for the sibling count
		/// - n bytes for the message data
		/// - m bytes for the sibling MAC addresses (6 bytes per sibling)
		/// </summary>
		private class MatchingPacketAccept : ProOnlineAdhocMatchingEventMessage
		{
			protected internal new const int HEADER_SIZE = 1 + 4 + 4;

			public MatchingPacketAccept(MatchingObject matchingObject, int address, int length, sbyte[] toMacAddress) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_ACCEPT, ADHOC_MATCHING_PACKET_ACCEPT, address, length, toMacAddress)
			{
			}

			public MatchingPacketAccept(MatchingObject matchingObject) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_ACCEPT, ADHOC_MATCHING_PACKET_ACCEPT)
			{
			}

			public MatchingPacketAccept(MatchingObject matchingObject, sbyte[] message, int length) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_ACCEPT, message, length)
			{
			}

			public override sbyte[] Message
			{
				get
				{
					sbyte[] message = new sbyte[MessageLength];
					offset = 0;
					addToBytes(message, (sbyte) PacketOpcode);
					addInt32ToBytes(message, DataLength);
					int siblingCount = SiblingCount;
					addInt32ToBytes(message, siblingCount);
					addToBytes(message, data);
					for (int i = 0; i < siblingCount; i++)
					{
						sbyte[] macAddress = MatchingObject.Members[i].macAddress;
						addToBytes(message, macAddress);
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Sending Sibling#{0:D}: MAC {1}", i, convertMacAddressToString(macAddress)));
						}
					}
    
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
					int siblingCount = copyInt32FromBytes(message);
					int restLength = length - HEADER_SIZE - siblingCount * MAC_ADDRESS_LENGTH;
					data = new sbyte[System.Math.Min(dataLength, restLength)];
					copyFromBytes(message, data);
					sbyte[] mac = new sbyte[MAC_ADDRESS_LENGTH];
					for (int i = 0; i < siblingCount; i++)
					{
						copyFromBytes(message, mac);
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Received Sibling#{0:D}: MAC {1}", i, convertMacAddressToString(mac)));
						}
					}
				}
			}

			protected internal virtual int SiblingCount
			{
				get
				{
					// Send siblings only for MODE_HOST
					if (MatchingObject.Mode != sceNetAdhocMatching.PSP_ADHOC_MATCHING_MODE_HOST)
					{
						return 0;
					}
					return MatchingObject.Members.Count;
				}
			}

			public override int MessageLength
			{
				get
				{
					return HEADER_SIZE + DataLength + SiblingCount * MAC_ADDRESS_LENGTH;
				}
			}

			public override void processOnReceive(int macAddr, int optData, int optLen)
			{
				// Send the PSP_ADHOC_MATCHING_EVENT_ACCEPT event immediately followed by
				// PSP_ADHOC_MATCHING_EVENT_COMPLETE
				base.processOnReceive(macAddr, optData, optLen);

				MatchingObject.notifyCallbackEvent(PSP_ADHOC_MATCHING_EVENT_COMPLETE, macAddr, optLen, optData);
			}

			public override void processOnSend(int macAddr, int optData, int optLen)
			{
				base.processOnSend(macAddr, optData, optLen);

				// Send the PSP_ADHOC_MATCHING_EVENT_COMPLETE event from the matching input thread
				MatchingObject.addCallbackEvent(PSP_ADHOC_MATCHING_EVENT_COMPLETE, macAddr, 0, 0);
			}
		}

		private class MatchingPacketCancel : ProOnlineAdhocMatchingEventMessage
		{
			public MatchingPacketCancel(MatchingObject matchingObject, int address, int length, sbyte[] toMacAddress) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_CANCEL, ADHOC_MATCHING_PACKET_CANCEL, address, length, toMacAddress)
			{
			}

			public MatchingPacketCancel(MatchingObject matchingObject) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_CANCEL, ADHOC_MATCHING_PACKET_CANCEL)
			{
			}

			public MatchingPacketCancel(MatchingObject matchingObject, sbyte[] message, int length) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_CANCEL, message, length)
			{
			}
		}

		private class MatchingPacketBulk : ProOnlineAdhocMatchingEventMessage
		{
			public MatchingPacketBulk(MatchingObject matchingObject, int address, int length, sbyte[] toMacAddress) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_DATA, ADHOC_MATCHING_PACKET_BULK, address, length, toMacAddress)
			{
			}

			public MatchingPacketBulk(MatchingObject matchingObject) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_DATA, ADHOC_MATCHING_PACKET_BULK)
			{
			}

			public MatchingPacketBulk(MatchingObject matchingObject, sbyte[] message, int length) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_DATA, message, length)
			{
			}
		}

		private class MatchingPacketBye : MatchingPacketOpcode
		{
			public MatchingPacketBye(MatchingObject matchingObject) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_DISCONNECT, ADHOC_MATCHING_PACKET_BYE)
			{
			}

			public MatchingPacketBye(MatchingObject matchingObject, sbyte[] message, int length) : base(matchingObject, PSP_ADHOC_MATCHING_EVENT_DISCONNECT, message, length)
			{
			}
		}

		public static ProOnlineAdhocMatchingEventMessage createPacket(ProOnlineNetworkAdapter proOnline, MatchingObject matchingObject, sbyte[] message, int length)
		{
			if (length > 0 && message != null && message.Length > 0)
			{
				switch (message[0])
				{
					case ADHOC_MATCHING_PACKET_PING:
						return new MatchingPacketPing(matchingObject, message, length);
					case ADHOC_MATCHING_PACKET_HELLO:
						return new MatchingPacketHello(matchingObject, message, length);
					case ADHOC_MATCHING_PACKET_JOIN:
						return new MatchingPacketJoin(matchingObject, message, length);
					case ADHOC_MATCHING_PACKET_ACCEPT:
						return new MatchingPacketAccept(matchingObject, message, length);
					case ADHOC_MATCHING_PACKET_CANCEL:
						return new MatchingPacketCancel(matchingObject, message, length);
					case ADHOC_MATCHING_PACKET_BULK:
						return new MatchingPacketBulk(matchingObject, message, length);
					case ADHOC_MATCHING_PACKET_BYE:
						return new MatchingPacketBye(matchingObject, message, length);
				}
			}

			return null;
		}

		public static ProOnlineAdhocMatchingEventMessage createPacket(ProOnlineNetworkAdapter proOnline, MatchingObject matchingObject, int @event)
		{
			switch (@event)
			{
				case PSP_ADHOC_MATCHING_EVENT_INTERNAL_PING:
					return new MatchingPacketPing(matchingObject);
				case PSP_ADHOC_MATCHING_EVENT_HELLO:
					return new MatchingPacketHello(matchingObject);
				case PSP_ADHOC_MATCHING_EVENT_JOIN:
					return new MatchingPacketJoin(matchingObject);
				case PSP_ADHOC_MATCHING_EVENT_ACCEPT:
					return new MatchingPacketAccept(matchingObject);
				case PSP_ADHOC_MATCHING_EVENT_CANCEL:
					return new MatchingPacketCancel(matchingObject);
				case PSP_ADHOC_MATCHING_EVENT_DATA:
					return new MatchingPacketBulk(matchingObject);
				case PSP_ADHOC_MATCHING_EVENT_DISCONNECT:
					return new MatchingPacketBye(matchingObject);
			}

			return null;
		}

		public static ProOnlineAdhocMatchingEventMessage createPacket(ProOnlineNetworkAdapter proOnline, MatchingObject matchingObject, int @event, int data, int dataLength, sbyte[] macAddress)
		{
			switch (@event)
			{
				case PSP_ADHOC_MATCHING_EVENT_INTERNAL_PING:
					return new MatchingPacketPing(matchingObject);
				case PSP_ADHOC_MATCHING_EVENT_HELLO:
					return new MatchingPacketHello(matchingObject, data, dataLength, macAddress);
				case PSP_ADHOC_MATCHING_EVENT_JOIN:
					return new MatchingPacketJoin(matchingObject, data, dataLength, macAddress);
				case PSP_ADHOC_MATCHING_EVENT_ACCEPT:
					return new MatchingPacketAccept(matchingObject, data, dataLength, macAddress);
				case PSP_ADHOC_MATCHING_EVENT_CANCEL:
					return new MatchingPacketCancel(matchingObject, data, dataLength, macAddress);
				case PSP_ADHOC_MATCHING_EVENT_DATA:
					return new MatchingPacketBulk(matchingObject, data, dataLength, macAddress);
				case PSP_ADHOC_MATCHING_EVENT_DISCONNECT:
					return new MatchingPacketBye(matchingObject);
			}

			return null;
		}
	}

}