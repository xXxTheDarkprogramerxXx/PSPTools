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
//	import static pspsharp.hardware.Wlan.MAC_ADDRESS_LENGTH;
	using AdhocMatchingEventMessage = pspsharp.network.adhoc.AdhocMatchingEventMessage;
	using MatchingObject = pspsharp.network.adhoc.MatchingObject;

	/// <summary>
	/// @author gid15
	/// 
	/// A JpcspAdhocMatchingEventMessage is consisting of:
	/// - 6 bytes for the MAC address of the message sender
	/// - 6 bytes for the MAC address of the message recipient
	/// - 1 byte for the event
	/// - n bytes for the message data
	/// </summary>
	public class JpcspAdhocMatchingEventMessage : AdhocMatchingEventMessage
	{
		protected internal static readonly int HEADER_SIZE = MAC_ADDRESS_LENGTH + MAC_ADDRESS_LENGTH + 1;

		public JpcspAdhocMatchingEventMessage(MatchingObject matchingObject, int @event) : base(matchingObject, @event)
		{
		}

		public JpcspAdhocMatchingEventMessage(MatchingObject matchingObject, int @event, int address, int Length, sbyte[] toMacAddress) : base(matchingObject, @event, address, Length, toMacAddress)
		{
		}

		public JpcspAdhocMatchingEventMessage(MatchingObject matchingObject, sbyte[] message, int Length) : base(matchingObject, message, Length)
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
				addToBytes(message, (sbyte) Event);
				addToBytes(message, data);
    
				return message;
			}
		}

		public override void setMessage(sbyte[] message, int Length)
		{
			if (Length >= HEADER_SIZE)
			{
				offset = 0;
				copyFromBytes(message, fromMacAddress);
				copyFromBytes(message, toMacAddress);
				Event = copyByteFromBytes(message);
				data = new sbyte[Length - HEADER_SIZE];
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
	}

}