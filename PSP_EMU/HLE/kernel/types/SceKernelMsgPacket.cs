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

	public class SceKernelMsgPacket : pspAbstractMemoryMappedStructure, Comparator<SceKernelMsgPacket>
	{
		private const int OFFSET_NEXT_MSG_PACKET_ADDR = 0;
		private const int OFFSET_MSG_PRIORITY = 4;
		public int nextMsgPacketAddr;
		internal int msgPriority; // SceUChar
		internal int unknow0; // SceUChar
		internal int unknow1; // SceUChar
		internal int unknow2; // SceUChar

		protected internal override void read()
		{
			nextMsgPacketAddr = read32();
			msgPriority = read8();
			unknow0 = read8();
			unknow1 = read8();
			unknow2 = read8();
		}

		protected internal override void write()
		{
			write32(nextMsgPacketAddr);
			write8((sbyte) msgPriority);
			write8((sbyte) unknow0);
			write8((sbyte) unknow1);
			write8((sbyte) unknow2);
		}

		public static void writeNext(Memory mem, int msgAddr, int nextMsgAddr)
		{
			mem.write32(msgAddr + OFFSET_NEXT_MSG_PACKET_ADDR, nextMsgAddr);
		}

		public static int readNext(Memory mem, int msgAddr)
		{
			return mem.read32(msgAddr + OFFSET_NEXT_MSG_PACKET_ADDR);
		}

		public override int @sizeof()
		{
			return 8;
		}

		public static int compare(Memory mem, int msgAddr1, int msgAddr2)
		{
			return mem.read8(msgAddr1 + OFFSET_MSG_PRIORITY) - mem.read8(msgAddr2 + OFFSET_MSG_PRIORITY);
		}

		public virtual int Compare(SceKernelMsgPacket m1, SceKernelMsgPacket m2)
		{
			return m1.msgPriority - m2.msgPriority;
		}
	}

}