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
namespace pspsharp.graphics.capture
{

	using PspGeList = pspsharp.HLE.kernel.types.PspGeList;

	/// <summary>
	/// captures a display list
	/// - PspGeList details
	/// - backing RAM containing the GE instructions 
	/// </summary>
	public class CaptureList
	{

		private const int packetSize = 16;
		private PspGeList list;
		private CaptureRAM listBuffer;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public CaptureList(pspsharp.HLE.kernel.types.PspGeList list) throws Exception
		public CaptureList(PspGeList list)
		{
			this.list = new PspGeList(list.id);
			this.list.init(list.list_addr, list.StallAddr, list.cbid, list.optParams);

			if (list.StallAddr - list.list_addr == 0)
			{
				VideoEngine.log_Renamed.error("Capture: Command list is empty");
			}

			int listSize = 0;
			if (list.StallAddr == 0)
			{
				// Scan list for END command
				Memory mem = Memory.Instance;
				for (int listPc = list.list_addr; Memory.isAddressGood(listPc); listPc += 4)
				{
					int instruction = mem.read32(listPc);
					int command = VideoEngine.command(instruction);
					if (command == GeCommands.END)
					{
						listSize = listPc - list.list_addr + 4;
						break;
					}
					else if (command == GeCommands.JUMP)
					{
						VideoEngine.log_Renamed.error("Found a JUMP instruction while scanning the list. Aborting the scan.");
						listSize = listPc - list.list_addr + 4;
						break;
					}
					else if (command == GeCommands.RET)
					{
						VideoEngine.log_Renamed.error("Found a RET instruction while scanning the list. Aborting the scan.");
						listSize = listPc - list.list_addr + 4;
						break;
					}
					else if (command == GeCommands.CALL)
					{
						VideoEngine.log_Renamed.warn("Found a CALL instruction while scanning the list. Ignoring the called list.");
					}
				}
			}
			else
			{
				listSize = list.StallAddr - list.list_addr;
			}

			listBuffer = new CaptureRAM(list.list_addr & Memory.addressMask, listSize);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(java.io.OutputStream out) throws java.io.IOException
		public virtual void write(System.IO.Stream @out)
		{
			DataOutputStream data = new DataOutputStream(@out);

			data.writeInt(packetSize);
			data.writeInt(list.list_addr);
			data.writeInt(list.StallAddr);
			data.writeInt(list.cbid);

			//VideoEngine.log.info("CaptureList write " + (5 * 4));

			CaptureHeader header = new CaptureHeader(CaptureHeader.PACKET_TYPE_RAM);
			header.write(@out);
			listBuffer.write(@out);
		}


		private CaptureList()
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static CaptureList read(java.io.InputStream in) throws java.io.IOException
		public static CaptureList read(System.IO.Stream @in)
		{
			CaptureList list = new CaptureList();

			DataInputStream data = new DataInputStream(@in);
			int sizeRemaining = data.readInt();
			if (sizeRemaining >= 16)
			{
				int list_addr = data.readInt();
				sizeRemaining -= 4;
				int stall_addr = data.readInt();
				sizeRemaining -= 4;
				int cbid = data.readInt();
				sizeRemaining -= 4;
				data.skipBytes(sizeRemaining);

				list.list = new PspGeList(0);
				list.list.init(list_addr, stall_addr, cbid, null);

				CaptureHeader header = CaptureHeader.read(@in);
				int packetType = header.PacketType;
				if (packetType != CaptureHeader.PACKET_TYPE_RAM)
				{
					throw new IOException("Expected CaptureRAM(" + CaptureHeader.PACKET_TYPE_RAM + ") packet, found " + packetType);
				}
				list.listBuffer = CaptureRAM.read(@in);
			}
			else
			{
				throw new IOException("Not enough bytes remaining in stream");
			}

			return list;
		}

		//public PspGeList getPspGeList() {
		//    return list;
		//}

		public virtual void commit()
		{
			VideoEngine.Instance.pushDrawList(list);
			listBuffer.commit();
		}
	}

}