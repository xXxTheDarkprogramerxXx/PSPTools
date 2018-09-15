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
namespace pspsharp.network.adhoc
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNet.convertMacAddressToString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhoc.ANY_MAC_ADDRESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhoc.isAnyMacAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceNetAdhoc.isSameMacAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeBytes;
	using Wlan = pspsharp.hardware.Wlan;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;

	/// <summary>
	/// @author gid15
	/// 
	/// An AdhocMessage is consisting of:
	/// - 6 bytes for the MAC address of the message sender
	/// - 6 bytes for the MAC address of the message recipient
	/// - n bytes for the message data
	/// </summary>
	public abstract class AdhocMessage
	{
		protected internal sbyte[] fromMacAddress = new sbyte[Wlan.MAC_ADDRESS_LENGTH];
		protected internal sbyte[] toMacAddress = new sbyte[Wlan.MAC_ADDRESS_LENGTH];
		protected internal sbyte[] data = new sbyte[0];
		protected internal int offset;
		public const int MAX_HEADER_SIZE = 13;

		public AdhocMessage()
		{
			init(0, 0, ANY_MAC_ADDRESS);
		}

		public AdhocMessage(sbyte[] fromMacAddress, sbyte[] toMacAddress)
		{
			init(0, 0, toMacAddress);
		}

		public AdhocMessage(sbyte[] message, int Length)
		{
			setMessage(message, Length);
		}

		protected internal virtual void addToBytes(sbyte[] bytes, sbyte value)
		{
			bytes[offset++] = value;
		}

		protected internal virtual void addToBytes(sbyte[] bytes, sbyte[] src)
		{
			Array.Copy(src, 0, bytes, offset, src.Length);
			offset += src.Length;
		}

		protected internal virtual void addInt32ToBytes(sbyte[] bytes, int value)
		{
			addToBytes(bytes, (sbyte) value);
			addToBytes(bytes, (sbyte)(value >> 8));
			addToBytes(bytes, (sbyte)(value >> 16));
			addToBytes(bytes, (sbyte)(value >> 24));
		}

		protected internal virtual sbyte copyByteFromBytes(sbyte[] bytes)
		{
			return bytes[offset++];
		}

		protected internal virtual void copyFromBytes(sbyte[] bytes, sbyte[] dst)
		{
			Array.Copy(bytes, offset, dst, 0, dst.Length);
			offset += dst.Length;
		}

		protected internal virtual int copyInt32FromBytes(sbyte[] bytes)
		{
			return (copyByteFromBytes(bytes) & 0xFF) | ((copyByteFromBytes(bytes) & 0xFF) << 8) | ((copyByteFromBytes(bytes) & 0xFF) << 16) | ((copyByteFromBytes(bytes) & 0xFF) << 24);
		}

		public AdhocMessage(int address, int Length)
		{
			init(address, Length, ANY_MAC_ADDRESS);
		}

		public AdhocMessage(int address, int Length, sbyte[] toMacAddress)
		{
			init(address, Length, toMacAddress);
		}

		private void init(int address, int Length, sbyte[] toMacAddress)
		{
			init(address, Length, Wlan.MacAddress, toMacAddress);
		}

		private void init(int address, int Length, sbyte[] fromMacAddress, sbyte[] toMacAddress)
		{
			FromMacAddress = fromMacAddress;
			ToMacAddress = toMacAddress;
			data = new sbyte[Length];
			if (Length > 0 && address != 0)
			{
				IMemoryReader memoryReader = MemoryReader.getMemoryReader(address, Length, 1);
				for (int i = 0; i < Length; i++)
				{
					data[i] = (sbyte) memoryReader.readNext();
				}
			}
		}

		public virtual sbyte[] Data
		{
			set
			{
				this.data = new sbyte[value.Length];
				Array.Copy(value, 0, this.data, 0, value.Length);
			}
			get
			{
				return data;
			}
		}

		public virtual int DataInt32
		{
			set
			{
				data = new sbyte[4];
				for (int i = 0; i < 4; i++)
				{
					data[i] = (sbyte)(value >> (i * 8));
				}
			}
			get
			{
				int value = 0;
    
				for (int i = 0; i < 4 && i < data.Length; i++)
				{
					value |= (data[i] & 0xFF) << (i * 8);
				}
    
				return value;
			}
		}


		public abstract sbyte[] Message {get;}

		public virtual int MessageLength
		{
			get
			{
				return DataLength;
			}
		}

		public abstract void setMessage(sbyte[] message, int Length);

		public virtual void writeDataToMemory(int address)
		{
			writeBytes(address, DataLength, data, 0);
		}

		public virtual void writeDataToMemory(int address, int maxLength)
		{
			writeBytes(address, System.Math.Min(DataLength, maxLength), data, 0);
		}

		public virtual void writeDataToMemory(int address, int offset, int maxLength)
		{
			writeBytes(address, System.Math.Min(DataLength - offset, maxLength), data, offset);
		}


		public virtual int DataLength
		{
			get
			{
				return data.Length;
			}
		}

		public virtual sbyte[] FromMacAddress
		{
			get
			{
				return fromMacAddress;
			}
			set
			{
				Array.Copy(value, 0, this.fromMacAddress, 0, this.fromMacAddress.Length);
			}
		}

		public virtual sbyte[] ToMacAddress
		{
			get
			{
				return toMacAddress;
			}
			set
			{
				Array.Copy(value, 0, this.toMacAddress, 0, this.toMacAddress.Length);
			}
		}



		public virtual bool ForMe
		{
			get
			{
				return isAnyMacAddress(toMacAddress) || isSameMacAddress(toMacAddress, Wlan.MacAddress);
			}
		}

		public override string ToString()
		{
			return string.Format("{0}[fromMacAddress={1}, toMacAddress={2}, dataLength={3:D}]", this.GetType().Name, convertMacAddressToString(fromMacAddress), convertMacAddressToString(toMacAddress), DataLength);
		}
	}

}