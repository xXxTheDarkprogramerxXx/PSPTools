using System;

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
//	import static pspsharp.HLE.modules.sceNetAdhocctl.ADHOC_ID_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocctl.GROUP_NAME_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocctl.NICK_NAME_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.hardware.Wlan.MAC_ADDRESS_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.network.proonline.ProOnlineNetworkAdapter.convertIpToString;

	using Logger = org.apache.log4j.Logger;

	using Modules = pspsharp.HLE.Modules;
	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using sceNetAdhocctl = pspsharp.HLE.modules.sceNetAdhocctl;
	using sceUtility = pspsharp.HLE.modules.sceUtility;
	using Wlan = pspsharp.hardware.Wlan;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class PacketFactory
	{
		protected internal static Logger log = ProOnlineNetworkAdapter.log;
		protected internal const int OPCODE_PING = 0;
		protected internal const int OPCODE_LOGIN = 1;
		protected internal const int OPCODE_CONNECT = 2;
		protected internal const int OPCODE_DISCONNECT = 3;
		protected internal const int OPCODE_SCAN = 4;
		protected internal const int OPCODE_SCAN_COMPLETE = 5;
		protected internal const int OPCODE_CONNECT_BSSID = 6;
		protected internal const int OPCODE_CHAT = 7;
		private const int CHAT_MESSAGE_LENGTH = 64;

		protected internal abstract class SceNetAdhocctlPacketBase
		{
			protected internal readonly ProOnlineNetworkAdapter proOnline;
			protected internal int opcode;
			protected internal int offset;

			protected internal SceNetAdhocctlPacketBase(ProOnlineNetworkAdapter proOnline)
			{
				this.proOnline = proOnline;
			}

			public virtual sbyte[] Bytes
			{
				get
				{
					sbyte[] bytes = new sbyte[Length];
					getBytes(bytes);
    
					return bytes;
				}
			}

			protected internal virtual void getBytes(sbyte[] bytes)
			{
				offset = 0;
				bytes[offset] = (sbyte) opcode;
				offset++;
			}

			protected internal virtual void copyToBytes(sbyte[] bytes, string s, int length)
			{
				for (int i = 0; i < length; i++, offset++)
				{
					bytes[offset] = (sbyte)(i < s.Length ? s[i] : 0);
				}
			}

			protected internal virtual string copyStringFromBytes(sbyte[] bytes, int length)
			{
				int stringLength = length;
				for (int i = 0; i < length; i++)
				{
					if (bytes[offset + i] == (sbyte) 0)
					{
						stringLength = i;
						break;
					}
				}

				string s = StringHelper.NewString(bytes, offset, stringLength);
				offset += length;

				return s;
			}

			protected internal virtual int copyInt8FromBytes(sbyte[] bytes)
			{
				return bytes[offset++] & 0xFF;
			}

			protected internal virtual int copyInt32FromBytes(sbyte[] bytes)
			{
				return (copyInt8FromBytes(bytes)) | (copyInt8FromBytes(bytes) << 8) | (copyInt8FromBytes(bytes) << 16) | (copyInt8FromBytes(bytes) << 24);
			}

			protected internal virtual pspNetMacAddress copyMacFromBytes(sbyte[] bytes)
			{
				pspNetMacAddress mac = new pspNetMacAddress();
				mac.setMacAddress(bytes, offset);
				offset += MAC_ADDRESS_LENGTH;

				return mac;
			}

			protected internal virtual void copyToBytes(sbyte[] bytes, pspNetMacAddress mac)
			{
				Array.Copy(mac.macAddress, 0, bytes, offset, MAC_ADDRESS_LENGTH);
				offset += MAC_ADDRESS_LENGTH;
			}

			protected internal virtual void copyInt8ToBytes(sbyte[] bytes, int value)
			{
				bytes[offset++] = unchecked((sbyte)(value & 0xFF));
			}

			protected internal virtual void copyInt32ToBytes(sbyte[] bytes, int value)
			{
				copyInt8ToBytes(bytes, value);
				copyInt8ToBytes(bytes, value >> 8);
				copyInt8ToBytes(bytes, value >> 16);
				copyInt8ToBytes(bytes, value >> 24);
			}

			protected internal virtual void init(sbyte[] bytes, int length)
			{
				offset = 0;
				if (length >= Length)
				{
					opcode = bytes[offset];
					offset++;
				}
			}

			public virtual int Length
			{
				get
				{
					return 1;
				}
			}

			public override string ToString()
			{
				return string.Format("{0}", this.GetType().Name);
			}
		}

		protected internal abstract class SceNetAdhocctlPacketBaseC2S : SceNetAdhocctlPacketBase
		{
			protected internal ProOnlineServer proOnlineServer;

			protected internal SceNetAdhocctlPacketBaseC2S(ProOnlineNetworkAdapter proOnline) : base(proOnline)
			{
			}

			protected internal SceNetAdhocctlPacketBaseC2S(ProOnlineNetworkAdapter proOnline, ProOnlineServer proOnlineServer) : base(proOnline)
			{
				this.proOnlineServer = proOnlineServer;
			}

			public abstract void process();
		}

		protected internal abstract class SceNetAdhocctlPacketBaseS2C : SceNetAdhocctlPacketBase
		{
			protected internal SceNetAdhocctlPacketBaseS2C(ProOnlineNetworkAdapter proOnline) : base(proOnline)
			{
			}

			public abstract void process();
		}

		protected internal class SceNetAdhocctlPingPacketC2S : SceNetAdhocctlPacketBaseC2S
		{
			public SceNetAdhocctlPingPacketC2S(ProOnlineNetworkAdapter proOnline) : base(proOnline)
			{
				opcode = OPCODE_PING;
			}

			public SceNetAdhocctlPingPacketC2S(ProOnlineNetworkAdapter proOnline, ProOnlineServer proOnlineServer, sbyte[] bytes, int length) : base(proOnline, proOnlineServer)
			{
				init(bytes, length);
			}

			public override void process()
			{
				// Nothing to do
			}
		}

		protected internal class SceNetAdhocctlDisconnectPacketC2S : SceNetAdhocctlPacketBaseC2S
		{
			public SceNetAdhocctlDisconnectPacketC2S(ProOnlineNetworkAdapter proOnline) : base(proOnline)
			{
				opcode = OPCODE_DISCONNECT;
			}

			public SceNetAdhocctlDisconnectPacketC2S(ProOnlineNetworkAdapter proOnline, ProOnlineServer proOnlineServer, sbyte[] bytes, int length) : base(proOnline, proOnlineServer)
			{
				init(bytes, length);
			}

			public override void process()
			{
				proOnlineServer.processDisconnect();
			}
		}

		protected internal class SceNetAdhocctlScanPacketC2S : SceNetAdhocctlPacketBaseC2S
		{
			public SceNetAdhocctlScanPacketC2S(ProOnlineNetworkAdapter proOnline) : base(proOnline)
			{
				opcode = OPCODE_SCAN;
			}

			public SceNetAdhocctlScanPacketC2S(ProOnlineNetworkAdapter proOnline, ProOnlineServer proOnlineServer, sbyte[] bytes, int length) : base(proOnline, proOnlineServer)
			{
				init(bytes, length);
			}

			public override void process()
			{
				proOnlineServer.processScan();
			}
		}

		protected internal class SceNetAdhocctlLoginPacketC2S : SceNetAdhocctlPacketBaseC2S
		{
			internal pspNetMacAddress mac = new pspNetMacAddress();
			internal string nickName;
			internal string game;

			public SceNetAdhocctlLoginPacketC2S(ProOnlineNetworkAdapter proOnline) : base(proOnline)
			{
				opcode = OPCODE_LOGIN;
				mac.MacAddress = Wlan.MacAddress;
				nickName = sceUtility.SystemParamNickname;
				game = Modules.sceNetAdhocctlModule.hleNetAdhocctlGetAdhocID();
			}

			public SceNetAdhocctlLoginPacketC2S(ProOnlineNetworkAdapter proOnline, ProOnlineServer proOnlineServer, sbyte[] bytes, int length) : base(proOnline, proOnlineServer)
			{
				init(bytes, length);
			}

			protected internal override void init(sbyte[] bytes, int length)
			{
				base.init(bytes, length);
				if (length >= Length)
				{
					mac = copyMacFromBytes(bytes);
					nickName = copyStringFromBytes(bytes, NICK_NAME_LENGTH);
					game = copyStringFromBytes(bytes, ADHOC_ID_LENGTH);
				}
			}

			protected internal override void getBytes(sbyte[] bytes)
			{
				base.getBytes(bytes);
				copyToBytes(bytes, mac);
				copyToBytes(bytes, nickName, NICK_NAME_LENGTH);
				copyToBytes(bytes, game, ADHOC_ID_LENGTH);
			}

			public override int Length
			{
				get
				{
					return base.Length + MAC_ADDRESS_LENGTH + NICK_NAME_LENGTH + ADHOC_ID_LENGTH;
				}
			}

			public override void process()
			{
				proOnlineServer.processLogin(mac, nickName, game);
			}
		}

		protected internal class SceNetAdhocctlConnectPacketC2S : SceNetAdhocctlPacketBaseC2S
		{
			internal string group;

			public SceNetAdhocctlConnectPacketC2S(ProOnlineNetworkAdapter proOnline) : base(proOnline)
			{
				opcode = OPCODE_CONNECT;
				group = Modules.sceNetAdhocctlModule.hleNetAdhocctlGetGroupName();
			}

			public SceNetAdhocctlConnectPacketC2S(ProOnlineNetworkAdapter proOnline, ProOnlineServer proOnlineServer, sbyte[] bytes, int length) : base(proOnline, proOnlineServer)
			{
				init(bytes, length);
			}

			protected internal override void init(sbyte[] bytes, int length)
			{
				base.init(bytes, length);
				if (length >= Length)
				{
					group = copyStringFromBytes(bytes, GROUP_NAME_LENGTH);
				}
			}

			protected internal override void getBytes(sbyte[] bytes)
			{
				base.getBytes(bytes);
				copyToBytes(bytes, group, GROUP_NAME_LENGTH);
			}

			public override int Length
			{
				get
				{
					return base.Length + GROUP_NAME_LENGTH;
				}
			}

			public override void process()
			{
				proOnlineServer.processConnect(group);
			}
		}

		protected internal class SceNetAdhocctlChatPacketC2S : SceNetAdhocctlPacketBaseC2S
		{
			internal string message;

			public SceNetAdhocctlChatPacketC2S(ProOnlineNetworkAdapter proOnline, string message) : base(proOnline)
			{
				opcode = OPCODE_CHAT;
				this.message = message;
			}

			public SceNetAdhocctlChatPacketC2S(ProOnlineNetworkAdapter proOnline, ProOnlineServer proOnlineServer, sbyte[] bytes, int length) : base(proOnline, proOnlineServer)
			{
				init(bytes, length);
			}

			protected internal override void init(sbyte[] bytes, int length)
			{
				base.init(bytes, length);
				if (length >= Length)
				{
					message = copyStringFromBytes(bytes, CHAT_MESSAGE_LENGTH);
				}
			}

			protected internal override void getBytes(sbyte[] bytes)
			{
				base.getBytes(bytes);
				copyToBytes(bytes, message, CHAT_MESSAGE_LENGTH);
			}

			public override int Length
			{
				get
				{
					return base.Length + CHAT_MESSAGE_LENGTH;
				}
			}

			public override void process()
			{
				proOnlineServer.processChat(message);
			}
		}

		private class SceNetAdhocctlPingPacketS2C : SceNetAdhocctlPacketBaseS2C
		{
			public SceNetAdhocctlPingPacketS2C(ProOnlineNetworkAdapter proOnline, sbyte[] bytes, int length) : base(proOnline)
			{
				init(bytes, length);
			}

			public override void process()
			{
				// Nothing to do
			}

			public override string ToString()
			{
				return string.Format("PingPacketS2C");
			}
		}

		protected internal class SceNetAdhocctlConnectPacketS2C : SceNetAdhocctlPacketBaseS2C
		{
			internal string nickName;
			internal pspNetMacAddress mac;
			internal int ip;

			public SceNetAdhocctlConnectPacketS2C(ProOnlineNetworkAdapter proOnline, sbyte[] bytes, int length) : base(proOnline)
			{
				init(bytes, length);
			}

			public SceNetAdhocctlConnectPacketS2C(string nickName, pspNetMacAddress mac, int ip) : base(null)
			{
				opcode = OPCODE_CONNECT;
				this.nickName = nickName;
				this.mac = mac;
				this.ip = ip;
			}

			protected internal override void init(sbyte[] bytes, int length)
			{
				base.init(bytes, length);
				if (length >= Length)
				{
					nickName = copyStringFromBytes(bytes, NICK_NAME_LENGTH);
					mac = copyMacFromBytes(bytes);
					ip = copyInt32FromBytes(bytes);
				}
			}

			protected internal override void getBytes(sbyte[] bytes)
			{
				base.getBytes(bytes);
				copyToBytes(bytes, nickName, NICK_NAME_LENGTH);
				copyToBytes(bytes, mac);
				copyInt32ToBytes(bytes, ip);
			}

			public override int Length
			{
				get
				{
					return base.Length + NICK_NAME_LENGTH + MAC_ADDRESS_LENGTH + 4;
				}
			}

			public override string ToString()
			{
				return string.Format("ConnectPacketS2C[nickName='{0}', mac={1}, ip={2}]", nickName, mac, convertIpToString(ip));
			}

			public override void process()
			{
				proOnline.addFriend(nickName, mac, ip);
			}
		}

		protected internal class SceNetAdhocctlConnectBSSIDPacketS2C : SceNetAdhocctlPacketBaseS2C
		{
			internal pspNetMacAddress mac;

			public SceNetAdhocctlConnectBSSIDPacketS2C(ProOnlineNetworkAdapter proOnline, sbyte[] bytes, int length) : base(proOnline)
			{
				init(bytes, length);
			}

			public SceNetAdhocctlConnectBSSIDPacketS2C(pspNetMacAddress mac) : base(null)
			{
				opcode = OPCODE_CONNECT_BSSID;
				this.mac = mac;
			}

			protected internal override void init(sbyte[] bytes, int length)
			{
				base.init(bytes, length);
				if (length >= Length)
				{
					mac = copyMacFromBytes(bytes);
				}
			}

			public override void process()
			{
				log.info(string.Format("Received MAC address {0}", mac));
				proOnline.ConnectComplete = true;
			}

			protected internal override void getBytes(sbyte[] bytes)
			{
				base.getBytes(bytes);
				copyToBytes(bytes, mac);
			}

			public override int Length
			{
				get
				{
					return base.Length + MAC_ADDRESS_LENGTH;
				}
			}

			public override string ToString()
			{
				return string.Format("ConnectBSSIDPacketS2C[mac={0}]", mac);
			}
		}

		protected internal class SceNetAdhocctlScanPacketS2C : SceNetAdhocctlPacketBaseS2C
		{
			internal string group;
			internal pspNetMacAddress mac;

			public SceNetAdhocctlScanPacketS2C(ProOnlineNetworkAdapter proOnline, sbyte[] bytes, int length) : base(proOnline)
			{
				init(bytes, length);
			}

			public SceNetAdhocctlScanPacketS2C(string group, pspNetMacAddress mac) : base(null)
			{
				opcode = OPCODE_SCAN;
				this.group = group;
				this.mac = mac;
			}

			protected internal override void init(sbyte[] bytes, int length)
			{
				base.init(bytes, length);
				if (length >= Length)
				{
					group = copyStringFromBytes(bytes, GROUP_NAME_LENGTH);
					mac = copyMacFromBytes(bytes);
				}
			}

			protected internal override void getBytes(sbyte[] bytes)
			{
				base.getBytes(bytes);
				copyToBytes(bytes, group, GROUP_NAME_LENGTH);
				copyToBytes(bytes, mac);
			}

			public override int Length
			{
				get
				{
					return base.Length + GROUP_NAME_LENGTH + MAC_ADDRESS_LENGTH;
				}
			}

			public override void process()
			{
				Modules.sceNetAdhocctlModule.hleNetAdhocctlAddNetwork(group, mac, sceNetAdhocctl.PSP_ADHOCCTL_MODE_NORMAL);
			}

			public override string ToString()
			{
				return string.Format("ScanPacketS2C[group='{0}', mac={1}]", group, mac);
			}
		}

		private class SceNetAdhocctlScanCompletePacketS2C : SceNetAdhocctlPacketBaseS2C
		{
			public SceNetAdhocctlScanCompletePacketS2C(ProOnlineNetworkAdapter proOnline, sbyte[] bytes, int length) : base(proOnline)
			{
				init(bytes, length);
			}

			public override void process()
			{
				Modules.sceNetAdhocctlModule.hleNetAdhocctlScanComplete();
			}

			public override string ToString()
			{
				return string.Format("ScanCompletePacketS2C");
			}
		}

		protected internal class SceNetAdhocctlDisconnectPacketS2C : SceNetAdhocctlPacketBaseS2C
		{
			internal int ip;

			public SceNetAdhocctlDisconnectPacketS2C(ProOnlineNetworkAdapter proOnline, sbyte[] bytes, int length) : base(proOnline)
			{
				init(bytes, length);
			}

			public SceNetAdhocctlDisconnectPacketS2C(int ip) : base(null)
			{
				opcode = OPCODE_DISCONNECT;
				this.ip = ip;
			}

			protected internal override void init(sbyte[] bytes, int length)
			{
				base.init(bytes, length);
				if (length >= Length)
				{
					ip = copyInt32FromBytes(bytes);
				}
			}

			protected internal override void getBytes(sbyte[] bytes)
			{
				base.getBytes(bytes);
				copyInt32ToBytes(bytes, ip);
			}

			public override void process()
			{
				proOnline.deleteFriend(ip);
			}

			public override int Length
			{
				get
				{
					return base.Length + 4;
				}
			}

			public override string ToString()
			{
				return string.Format("DisconnectPacketS2C ip={0}", convertIpToString(ip));
			}
		}

		protected internal class SceNetAdhocctlChatPacketS2C : SceNetAdhocctlPacketBaseS2C
		{
			internal string message;
			internal string nickName;

			public SceNetAdhocctlChatPacketS2C(ProOnlineNetworkAdapter proOnline, sbyte[] bytes, int length) : base(proOnline)
			{
				init(bytes, length);
			}

			public SceNetAdhocctlChatPacketS2C(string message, string nickName) : base(null)
			{
				opcode = OPCODE_CHAT;
				this.message = message;
				this.nickName = nickName;
			}

			protected internal override void init(sbyte[] bytes, int length)
			{
				base.init(bytes, length);
				if (length >= Length)
				{
					message = copyStringFromBytes(bytes, CHAT_MESSAGE_LENGTH);
					nickName = copyStringFromBytes(bytes, NICK_NAME_LENGTH);
				}
			}

			public override void process()
			{
				proOnline.displayChatMessage(nickName, message);
			}

			protected internal override void getBytes(sbyte[] bytes)
			{
				base.getBytes(bytes);
				copyToBytes(bytes, message, CHAT_MESSAGE_LENGTH);
				copyToBytes(bytes, nickName, NICK_NAME_LENGTH);
			}

			public override int Length
			{
				get
				{
					return base.Length + CHAT_MESSAGE_LENGTH + NICK_NAME_LENGTH;
				}
			}

			public override string ToString()
			{
				return string.Format("ChatPacketS2C message='{0}' from '{1}'", message, nickName);
			}
		}

		public virtual SceNetAdhocctlPacketBaseS2C createPacketS2C(ProOnlineNetworkAdapter proOnline, sbyte[] buffer, int length)
		{
			if (length > 0)
			{
				switch (buffer[0])
				{
					case OPCODE_PING:
						return new SceNetAdhocctlPingPacketS2C(proOnline, buffer, length);
					case OPCODE_CONNECT_BSSID:
						return new SceNetAdhocctlConnectBSSIDPacketS2C(proOnline, buffer, length);
					case OPCODE_CONNECT:
						return new SceNetAdhocctlConnectPacketS2C(proOnline, buffer, length);
					case OPCODE_SCAN:
						return new SceNetAdhocctlScanPacketS2C(proOnline, buffer, length);
					case OPCODE_SCAN_COMPLETE:
						return new SceNetAdhocctlScanCompletePacketS2C(proOnline, buffer, length);
					case OPCODE_DISCONNECT:
						return new SceNetAdhocctlDisconnectPacketS2C(proOnline, buffer, length);
					case OPCODE_CHAT:
						return new SceNetAdhocctlChatPacketS2C(proOnline, buffer, length);
					default:
						ProOnlineNetworkAdapter.log.error(string.Format("Received unknown S2C opcode {0:D}", buffer[0]));
						break;
				}
			}

			return null;
		}

		public virtual SceNetAdhocctlPacketBaseC2S createPacketC2S(ProOnlineNetworkAdapter proOnline, ProOnlineServer proOnlineServer, sbyte[] buffer, int length)
		{
			if (length > 0)
			{
				switch (buffer[0])
				{
					case OPCODE_LOGIN:
						return new SceNetAdhocctlLoginPacketC2S(proOnline, proOnlineServer, buffer, length);
					case OPCODE_PING:
						return new SceNetAdhocctlPingPacketC2S(proOnline, proOnlineServer, buffer, length);
					case OPCODE_CONNECT:
						return new SceNetAdhocctlConnectPacketC2S(proOnline, proOnlineServer, buffer, length);
					case OPCODE_DISCONNECT:
						return new SceNetAdhocctlDisconnectPacketC2S(proOnline, proOnlineServer, buffer, length);
					case OPCODE_SCAN:
						return new SceNetAdhocctlScanPacketC2S(proOnline, proOnlineServer, buffer, length);
					case OPCODE_CHAT:
						return new SceNetAdhocctlChatPacketC2S(proOnline, proOnlineServer, buffer, length);
					default:
						ProOnlineNetworkAdapter.log.error(string.Format("Received unknown C2S opcode {0:D}", buffer[0]));
						break;
				}
			}

			return null;
		}
	}

}