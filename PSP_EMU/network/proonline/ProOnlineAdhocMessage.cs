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
	using AdhocMessage = pspsharp.network.adhoc.AdhocMessage;

	/// <summary>
	/// @author gid15
	/// 
	/// A generic ProOnlineAdhocMessage is consisting of:
	/// - n bytes for the message data
	/// </summary>
	public class ProOnlineAdhocMessage : AdhocMessage
	{
		private ProOnlineNetworkAdapter proOnline;

		public ProOnlineAdhocMessage(ProOnlineNetworkAdapter networkAdapter, sbyte[] message, int length) : base(message, length)
		{
			this.proOnline = networkAdapter;
		}

		public ProOnlineAdhocMessage(ProOnlineNetworkAdapter networkAdapter, int address, int length) : base(address, length)
		{
			this.proOnline = networkAdapter;
		}

		public ProOnlineAdhocMessage(ProOnlineNetworkAdapter networkAdapter, int address, int length, sbyte[] toMacAddress) : base(address, length, toMacAddress)
		{
			this.proOnline = networkAdapter;
		}

		public override sbyte[] Message
		{
			get
			{
				sbyte[] message = new sbyte[MessageLength];
				offset = 0;
				addToBytes(message, data);
    
				return message;
			}
		}

		public override void setMessage(sbyte[] message, int length)
		{
			if (length >= 0)
			{
				offset = 0;
				data = new sbyte[length];
				copyFromBytes(message, data);
			}
		}

		public override string ToString()
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			return string.Format("{0}[fromMacAddress={1}, toMacAddress={2}(ip={3}), dataLength={4:D}]", this.GetType().FullName, convertMacAddressToString(fromMacAddress), convertMacAddressToString(toMacAddress), proOnline.getInetAddress(toMacAddress), DataLength);
		}
	}

}