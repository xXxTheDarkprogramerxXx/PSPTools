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
namespace pspsharp.HLE.kernel.types
{

	using sceNet = pspsharp.HLE.modules.sceNet;
	using sceNetAdhoc = pspsharp.HLE.modules.sceNetAdhoc;
	using Wlan = pspsharp.hardware.Wlan;

	public class pspNetMacAddress : pspAbstractMemoryMappedStructure
	{
		public readonly sbyte[] macAddress = new sbyte[Wlan.MAC_ADDRESS_LENGTH];

		public pspNetMacAddress()
		{
		}

		public pspNetMacAddress(sbyte[] macAddress)
		{
			MacAddress = macAddress;
		}

		protected internal override void read()
		{
			for (int i = 0; i < macAddress.Length; i++)
			{
				macAddress[i] = (sbyte) read8();
			}
		}

		protected internal override void write()
		{
			for (int i = 0; i < macAddress.Length; i++)
			{
				write8(macAddress[i]);
			}
		}

		public virtual sbyte[] MacAddress
		{
			set
			{
				setMacAddress(value, 0);
			}
		}

		public virtual void setMacAddress(sbyte[] macAddress, int offset)
		{
			Array.Copy(macAddress, offset, this.macAddress, 0, System.Math.Min(macAddress.Length - offset, this.macAddress.Length));
		}

		public override int @sizeof()
		{
			return macAddress.Length;
		}

		/// <summary>
		/// Is the MAC address the special ANY MAC address (FF:FF:FF:FF:FF:FF)?
		/// </summary>
		/// <returns>    true if this is the special ANY MAC address
		///            false otherwise </returns>
		public virtual bool AnyMacAddress
		{
			get
			{
				for (int i = 0; i < macAddress.Length; i++)
				{
					if (macAddress[i] != unchecked((sbyte) 0xFF))
					{
						return false;
					}
				}
    
				return true;
			}
		}

		/// <summary>
		/// Is the MAC address the empty MAC address (00:00:00:00:00:00)?
		/// </summary>
		/// <returns>    true if this is the empty MAC address
		///            false otherwise </returns>
		public virtual bool EmptyMacAddress
		{
			get
			{
				for (int i = 0; i < macAddress.Length; i++)
				{
					if (macAddress[i] != (sbyte) 0x00)
					{
						return false;
					}
				}
    
				return true;
			}
		}

		public override bool Equals(object @object)
		{
			if (@object is pspNetMacAddress)
			{
				pspNetMacAddress macAddress = (pspNetMacAddress) @object;
				return sceNetAdhoc.isSameMacAddress(macAddress.macAddress, this.macAddress);
			}
			return base.Equals(@object);
		}

		public virtual bool Equals(sbyte[] macAddress)
		{
			return sceNetAdhoc.isSameMacAddress(macAddress, this.macAddress);
		}

		public static sbyte[] RandomMacAddress
		{
			get
			{
				sbyte[] macAddress = new sbyte[Wlan.MAC_ADDRESS_LENGTH];
    
				System.Random random = new System.Random();
				for (int i = 0; i < macAddress.Length; i++)
				{
					macAddress[i] = (sbyte) random.Next(256);
				}
				// Both least significant bits of the first byte have a special meaning
				// (see http://en.wikipedia.org/wiki/Mac_address):
				// bit 0: 0=Unicast / 1=Multicast
				// bit 1: 0=Globally unique / 1=Locally administered
				macAddress[0] &= unchecked((sbyte)0xFC);
    
				return macAddress;
			}
		}

		public override string ToString()
		{
			// When the base address is not set, return the MAC address only:
			// "nn:nn:nn:nn:nn:nn"
			if (BaseAddress == 0)
			{
				return sceNet.convertMacAddressToString(macAddress);
			}
			// When the MAC address is not set, return the base address only:
			// "0xNNNNNNNN"
			if (EmptyMacAddress)
			{
				return base.ToString();
			}

			// When both the base address and the MAC address are set,
			// return "0xNNNNNNNN(nn:nn:nn:nn:nn:nn)"
			return string.Format("{0}({1})", base.ToString(), sceNet.convertMacAddressToString(macAddress));
		}
	}

}