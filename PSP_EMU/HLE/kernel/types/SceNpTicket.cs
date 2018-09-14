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
namespace pspsharp.HLE.kernel.types
{

	using Utilities = pspsharp.util.Utilities;

	public class SceNpTicket : pspAbstractMemoryMappedStructure
	{
		public const int NUMBER_PARAMETERS = 12;
		public int version;
		public int size;
		public int unknown;
		public int sizeParams;
		public IList<TicketParam> parameters = new LinkedList<SceNpTicket.TicketParam>();
		public sbyte[] unknownBytes;

		public class TicketParam
		{
			public const int PARAM_TYPE_NULL = 0;
			public const int PARAM_TYPE_INT = 1;
			public const int PARAM_TYPE_LONG = 2;
			public const int PARAM_TYPE_STRING = 4;
			public const int PARAM_TYPE_DATE = 7;
			public const int PARAM_TYPE_STRING_ASCII = 8;
			public int type;
			internal sbyte[] value;

			public TicketParam(int type, sbyte[] value)
			{
				this.type = type;
				this.value = value;
			}

			public virtual int Type
			{
				get
				{
					return type;
				}
			}

			internal virtual int getIntValue(int offset)
			{
				return Utilities.endianSwap32(Utilities.readUnaligned32(value, offset));
			}

			public virtual int IntValue
			{
				get
				{
					return getIntValue(0);
				}
			}

			public virtual long LongValue
			{
				get
				{
					return (((long) getIntValue(0)) << 32) | (getIntValue(4) & 0xFFFFFFFFL);
				}
			}

			public virtual string StringValue
			{
				get
				{
					int length = value.Length;
					for (int i = 0; i < value.Length; i++)
					{
						if (value[i] == (sbyte) 0)
						{
							length = i;
							break;
						}
					}
					return StringHelper.NewString(value, 0, length);
				}
			}

			public virtual sbyte[] BytesValue
			{
				get
				{
					return value;
				}
			}

			public virtual DateTime DateValue
			{
				get
				{
					return new DateTime(LongValue);
				}
			}

			public virtual void writeForPSP(TPointer buffer)
			{
				switch (type)
				{
					case PARAM_TYPE_INT:
						// This value is written in PSP endianness
						buffer.setValue32(IntValue);
						break;
					case PARAM_TYPE_DATE:
					case PARAM_TYPE_LONG:
						// This value is written in PSP endianness
						buffer.Value64 = LongValue;
						break;
					case PARAM_TYPE_STRING:
					case PARAM_TYPE_STRING_ASCII:
						int length = value.Length;
						if (length >= 256)
						{
							length = 255; // PSP returns maximum 255 bytes
						}
						Utilities.writeBytes(buffer.Address, length, value, 0);
						// Add trailing 0
						buffer.setValue8(length, (sbyte) 0);
						break;
					default:
						// Copy nothing
						break;
				}
			}

			public override string ToString()
			{
				switch (type)
				{
					case PARAM_TYPE_INT:
						return string.Format("0x{0:X}", IntValue);
					case PARAM_TYPE_STRING_ASCII:
					case PARAM_TYPE_STRING:
						return StringValue;
					case PARAM_TYPE_DATE:
						return DateValue.ToString();
					case PARAM_TYPE_NULL:
						return "null";
					case PARAM_TYPE_LONG:
						return string.Format("0x{0,16:X}", LongValue);
				}
				return string.Format("type={0:D}, value={1}", type, Utilities.getMemoryDump(value, 0, value.Length));
			}
		}

		protected internal override void read()
		{
			version = read32();
			size = endianSwap32(read32());
			unknown = endianSwap16((short) read16());
			sizeParams = endianSwap16((short) read16());

			parameters.Clear();
			for (int i = 0; i < NUMBER_PARAMETERS; i++)
			{
				int type = endianSwap16((short) read16());
				int length = endianSwap16((short) read16());

				sbyte[] value = new sbyte[length];
				read8Array(value);

				TicketParam ticketParam = new TicketParam(type, value);
				parameters.Add(ticketParam);
			}

			unknownBytes = new sbyte[size - Offset + 8];
			read8Array(unknownBytes);
		}

		protected internal override void write()
		{
			write32(version);
			write32(endianSwap32(size));
			write16((short) endianSwap16((short) unknown));
			write16((short) endianSwap16((short) sizeParams));

			foreach (TicketParam ticketParam in parameters)
			{
				write16((short) endianSwap16((short) ticketParam.Type));
				sbyte[] value = ticketParam.BytesValue;
				write16((short) endianSwap16((short) value.Length));
				write8Array(value);
			}

			write8Array(unknownBytes);
		}

		public virtual sbyte[] toByteArray()
		{
			sbyte[] bytes = new sbyte[@sizeof()];
			ByteBuffer b = ByteBuffer.wrap(bytes).order(ByteOrder.LITTLE_ENDIAN);

			b.putInt(version);
			b.putInt(endianSwap32(size));
			b.putShort((short) endianSwap16((short) unknown));
			b.putShort((short) endianSwap16((short) sizeParams));

			foreach (TicketParam ticketParam in parameters)
			{
				b.putShort((short) endianSwap16((short) ticketParam.Type));
				sbyte[] value = ticketParam.BytesValue;
				b.putShort((short) endianSwap16((short) value.Length));
				b.put(value);
			}

			b.put(unknownBytes);

			return bytes;
		}

		public virtual void read(sbyte[] bytes, int offset, int length)
		{
			ByteBuffer b = ByteBuffer.wrap(bytes, offset, length).order(ByteOrder.LITTLE_ENDIAN);

			version = b.Int;
			size = endianSwap32(b.Int);
			unknown = endianSwap16(b.Short);
			sizeParams = endianSwap16(b.Short);
			int readSize = 12;

			parameters.Clear();
			for (int i = 0; i < NUMBER_PARAMETERS; i++)
			{
				int type = endianSwap16(b.Short);
				int valueLength = endianSwap16(b.Short);
				sbyte[] value = new sbyte[valueLength];
				b.get(value);

				readSize += 4 + valueLength;

				TicketParam ticketParam = new TicketParam(type, value);
				parameters.Add(ticketParam);
			}

			unknownBytes = new sbyte[size - readSize + 8];
			b.get(unknownBytes);
		}

		public override int @sizeof()
		{
			return size + 8;
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			s.Append(string.Format("version=0x{0:X8}", version));
			s.Append(string.Format(", size=0x{0:X}", size));
			s.Append(string.Format(", unknown=0x{0:X}", unknown));
			for (int i = 0; i < parameters.Count; i++)
			{
				TicketParam param = parameters[i];
				s.Append(string.Format(", param#{0:D}={1}", i, param));
			}
			s.Append(string.Format(", unknownBytes: {0}", Utilities.getMemoryDump(unknownBytes, 0, unknownBytes.Length)));

			return s.ToString();
		}
	}

}