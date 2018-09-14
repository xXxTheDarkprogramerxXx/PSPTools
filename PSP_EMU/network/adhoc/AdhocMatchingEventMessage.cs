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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_INTERNAL_PING;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public abstract class AdhocMatchingEventMessage : AdhocMessage
	{
		// One of sceNetAdhocMatching.PSP_ADHOC_MATCHING_EVENT_xxx
		private int @event;
		private MatchingObject matchingObject;

		public AdhocMatchingEventMessage(MatchingObject matchingObject, int @event) : base()
		{
			this.@event = @event;
			this.matchingObject = matchingObject;
		}

		public AdhocMatchingEventMessage(MatchingObject matchingObject, int @event, int address, int length, sbyte[] toMacAddress) : base(address, length, toMacAddress)
		{
			this.@event = @event;
			this.matchingObject = matchingObject;
		}

		public AdhocMatchingEventMessage(MatchingObject matchingObject, sbyte[] message, int length) : base(message, length)
		{
			this.matchingObject = matchingObject;
		}

		public virtual int Event
		{
			get
			{
				return @event;
			}
			set
			{
				this.@event = value;
			}
		}


		protected internal virtual MatchingObject MatchingObject
		{
			get
			{
				return matchingObject;
			}
		}

		public virtual void processOnReceive(int macAddr, int optData, int optLen)
		{
			if (@event != PSP_ADHOC_MATCHING_EVENT_INTERNAL_PING)
			{
				matchingObject.notifyCallbackEvent(Event, macAddr, optLen, optData);
			}
		}

		public virtual void processOnSend(int macAddr, int optData, int optLen)
		{
			// Nothing to do
		}
	}

}