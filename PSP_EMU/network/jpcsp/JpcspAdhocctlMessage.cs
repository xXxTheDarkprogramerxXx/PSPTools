using System;
using System.Collections.Generic;
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
namespace pspsharp.network.pspsharp
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.Modules.sceNetAdhocctlModule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocctl.GROUP_NAME_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocctl.IBSS_NAME_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocctl.MAX_GAME_MODE_MACS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhocctl.NICK_NAME_LENGTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.hardware.Wlan.MAC_ADDRESS_LENGTH;

	using pspNetMacAddress = pspsharp.HLE.kernel.types.pspNetMacAddress;
	using sceNet = pspsharp.HLE.modules.sceNet;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class JpcspAdhocctlMessage
	{
		protected internal string nickName;
		protected internal sbyte[] macAddress = new sbyte[MAC_ADDRESS_LENGTH];
		protected internal string groupName;
		protected internal string ibss;
		protected internal int mode;
		protected internal int channel;
		protected internal bool gameModeComplete;
		protected internal sbyte[][] gameModeMacs;

		public JpcspAdhocctlMessage(string nickName, sbyte[] macAddress, string groupName)
		{
			this.nickName = nickName;
			Array.Copy(macAddress, 0, this.macAddress, 0, this.macAddress.Length);
			this.groupName = groupName;
			ibss = sceNetAdhocctlModule.hleNetAdhocctlGetIBSS();
			mode = sceNetAdhocctlModule.hleNetAdhocctlGetMode();
			channel = sceNetAdhocctlModule.hleNetAdhocctlGetChannel();
			gameModeComplete = false;
			gameModeMacs = null;
		}

		public JpcspAdhocctlMessage(sbyte[] message, int length)
		{
			int offset = 0;
			nickName = copyFromMessage(message, offset, NICK_NAME_LENGTH);
			offset += NICK_NAME_LENGTH;
			copyFromMessage(message, offset, macAddress);
			offset += macAddress.Length;
			groupName = copyFromMessage(message, offset, GROUP_NAME_LENGTH);
			offset += GROUP_NAME_LENGTH;
			ibss = copyFromMessage(message, offset, IBSS_NAME_LENGTH);
			offset += IBSS_NAME_LENGTH;
			mode = copyInt32FromMessage(message, offset);
			offset += 4;
			channel = copyInt32FromMessage(message, offset);
			offset += 4;
			gameModeComplete = copyBoolFromMessage(message, offset);
			offset++;
			int numberGameModeMacs = copyInt32FromMessage(message, offset);
			offset += 4;
			if (numberGameModeMacs > 0)
			{
				gameModeMacs = copyMacsFromMessage(message, offset, numberGameModeMacs);
				offset += MAC_ADDRESS_LENGTH * numberGameModeMacs;
			}
		}

		public virtual void setGameModeComplete(bool gameModeComplete, IList<pspNetMacAddress> requiredGameModeMacs)
		{
			this.gameModeComplete = gameModeComplete;
			int numberGameModeMacs = requiredGameModeMacs.Count;
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: gameModeMacs = new sbyte[numberGameModeMacs][MAC_ADDRESS_LENGTH];
			gameModeMacs = RectangularArrays.ReturnRectangularSbyteArray(numberGameModeMacs, MAC_ADDRESS_LENGTH);
			int i = 0;
			foreach (pspNetMacAddress macAddress in requiredGameModeMacs)
			{
				gameModeMacs[i] = macAddress.macAddress;
				i++;
			}
		}

		private string copyFromMessage(sbyte[] message, int offset, int length)
		{
			StringBuilder s = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				sbyte b = message[offset + i];
				if (b == 0)
				{
					break;
				}
				s.Append((char) b);
			}

			return s.ToString();
		}

		private int copyInt32FromMessage(sbyte[] message, int offset)
		{
			int n = 0;
			for (int i = 0; i < 4; i++)
			{
				n |= (message[offset + i] & 0xFF) << (i * 8);
			}

			return n;
		}

		private bool copyBoolFromMessage(sbyte[] message, int offset)
		{
			return message[offset] != 0;
		}

		private void copyFromMessage(sbyte[] message, int offset, sbyte[] bytes)
		{
			Array.Copy(message, offset, bytes, 0, bytes.Length);
		}

		private sbyte[][] copyMacsFromMessage(sbyte[] message, int offset, int numberMacs)
		{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: sbyte[][] macs = new sbyte[numberMacs][MAC_ADDRESS_LENGTH];
			sbyte[][] macs = RectangularArrays.ReturnRectangularSbyteArray(numberMacs, MAC_ADDRESS_LENGTH);
			for (int i = 0; i < numberMacs; i++)
			{
				copyFromMessage(message, offset, macs[i]);
				offset += macs[i].Length;
			}

			return macs;
		}

		private void copyToMessage(sbyte[] message, int offset, string s)
		{
			if (!string.ReferenceEquals(s, null))
			{
				int length = s.Length;
				for (int i = 0; i < length; i++)
				{
					message[offset + i] = (sbyte) s[i];
				}
			}
		}

		private void copyToMessage(sbyte[] message, int offset, sbyte[] bytes)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				message[offset + i] = bytes[i];
			}
		}

		private void copyInt32ToMessage(sbyte[] message, int offset, int value)
		{
			for (int i = 0; i < 4; i++)
			{
				message[offset + i] = (sbyte)(value >> (i * 8));
			}
		}

		private void copyBoolToMessage(sbyte[] message, int offset, bool value)
		{
			message[offset] = (sbyte)(value ? 1 : 0);
		}

		private void copyMacsToMessage(sbyte[] message, int offset, sbyte[][] macs)
		{
			for (int i = 0; i < macs.Length; i++)
			{
				copyToMessage(message, offset, macs[i]);
				offset += macs[i].Length;
			}
		}

		public virtual sbyte[] Message
		{
			get
			{
				sbyte[] message = new sbyte[MessageLength];
    
				int offset = 0;
				copyToMessage(message, offset, nickName);
				offset += NICK_NAME_LENGTH;
				copyToMessage(message, offset, macAddress);
				offset += macAddress.Length;
				copyToMessage(message, offset, groupName);
				offset += GROUP_NAME_LENGTH;
				copyToMessage(message, offset, ibss);
				offset += IBSS_NAME_LENGTH;
				copyInt32ToMessage(message, offset, mode);
				offset += 4;
				copyInt32ToMessage(message, offset, channel);
				offset += 4;
				copyBoolToMessage(message, offset, gameModeComplete);
				offset++;
				if (gameModeMacs == null)
				{
					copyInt32ToMessage(message, offset, 0);
					offset += 4;
				}
				else
				{
					copyInt32ToMessage(message, offset, gameModeMacs.Length);
					offset += 4;
					copyMacsToMessage(message, offset, gameModeMacs);
					offset += gameModeMacs.Length * MAC_ADDRESS_LENGTH;
				}
    
				return message;
			}
		}

		public static int MessageLength
		{
			get
			{
				return NICK_NAME_LENGTH + MAC_ADDRESS_LENGTH + GROUP_NAME_LENGTH + IBSS_NAME_LENGTH + 4 + 4 + 1 + 4 + MAX_GAME_MODE_MACS * MAC_ADDRESS_LENGTH;
			}
		}

		public override string ToString()
		{
			StringBuilder macs = new StringBuilder();
			if (gameModeMacs != null)
			{
				macs.Append(", gameModeMacs=[");
				for (int i = 0; i < gameModeMacs.Length; i++)
				{
					if (i > 0)
					{
						macs.Append(", ");
					}
					macs.Append(sceNet.convertMacAddressToString(gameModeMacs[i]));
				}
				macs.Append("]");
			}

//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("JpcspAdhocctlMessage[nickName='%s', macAddress=%s, groupName='%s', IBSS='%s', mode=%d, channel=%d, gameModeComplete=%b%s]", nickName, pspsharp.HLE.modules.sceNet.convertMacAddressToString(macAddress), groupName, ibss, mode, channel, gameModeComplete, macs.toString());
			return string.Format("JpcspAdhocctlMessage[nickName='%s', macAddress=%s, groupName='%s', IBSS='%s', mode=%d, channel=%d, gameModeComplete=%b%s]", nickName, sceNet.convertMacAddressToString(macAddress), groupName, ibss, mode, channel, gameModeComplete, macs.ToString());
		}
	}

}