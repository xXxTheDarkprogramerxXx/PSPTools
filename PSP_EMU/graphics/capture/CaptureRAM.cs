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

	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;

	/// <summary>
	/// captures a piece of RAM </summary>
	public class CaptureRAM
	{

		private int packetSize;
		private int address;
		private int length;
		private Buffer buffer;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public CaptureRAM(int address, int length) throws java.io.IOException
		public CaptureRAM(int address, int length)
		{
			packetSize = 8 + length;
			this.address = address;
			this.length = length;

			Memory mem = Memory.Instance;
			if (Memory.isAddressGood(address))
			{
				buffer = mem.getBuffer(address, length);
			}

			if (buffer == null)
			{
				throw new IOException(string.Format("CaptureRAM: Unable to read buffer {0:x8} - {1:x8}", address, address + length));
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(java.io.OutputStream out) throws java.io.IOException
		public virtual void write(System.IO.Stream @out)
		{
			DataOutputStream data = new DataOutputStream(@out);

			data.writeInt(packetSize);
			data.writeInt(address);
			data.writeInt(length);

			if (buffer is ByteBuffer)
			{
				WritableByteChannel channel = Channels.newChannel(@out);
				channel.write((ByteBuffer)buffer);
			}
			else
			{
				IMemoryReader reader = MemoryReader.getMemoryReader(address, length, 1);
				for (int i = 0; i < length; i++)
				{
					data.writeByte(reader.readNext());
				}
			}

			VideoEngine.log_Renamed.info(string.Format("Saved memory {0:x8} - {1:x8} (len {2:x8})", address, address + length, length));
			//VideoEngine.log.info("CaptureRAM write " + ((3 * 4) + length));

			//data.flush();
			//out.flush();
		}


		private CaptureRAM()
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static CaptureRAM read(java.io.InputStream in) throws java.io.IOException
		public static CaptureRAM read(System.IO.Stream @in)
		{
			CaptureRAM ramFragment = new CaptureRAM();

			DataInputStream data = new DataInputStream(@in);
			int sizeRemaining = data.readInt();
			if (sizeRemaining >= 8)
			{
				ramFragment.address = data.readInt();
				sizeRemaining -= 4;
				ramFragment.length = data.readInt();
				sizeRemaining -= 4;

				if (sizeRemaining > data.available())
				{
					VideoEngine.log_Renamed.warn("CaptureRAM read want=" + sizeRemaining + " available=" + data.available());
				}

				if (sizeRemaining >= ramFragment.length)
				{
					ByteBuffer bb = ByteBuffer.allocate(ramFragment.length);
					sbyte[] b = bb.array();
					if (b == null)
					{
						throw new IOException("Buffer is not backed by an array");
					}
					data.readFully(b, 0, ramFragment.length);
					ramFragment.buffer = bb;
					sizeRemaining -= ramFragment.length;

					data.skipBytes(sizeRemaining);

					VideoEngine.log_Renamed.info(string.Format("Loaded memory {0:x8} - {1:x8} (len {2:x8})", ramFragment.address, ramFragment.address + ramFragment.length, ramFragment.length));
				}
				else
				{
					throw new IOException("Not enough bytes remaining in stream");
				}
			}
			else
			{
				throw new IOException("Not enough bytes remaining in stream");
			}

			return ramFragment;
		}

		public virtual void commit()
		{
			Memory.Instance.copyToMemory(address, (ByteBuffer)buffer, length);
		}

		public virtual int Address
		{
			get
			{
				return address;
			}
		}

		public virtual int Length
		{
			get
			{
				return length;
			}
		}

		public virtual Buffer Buffer
		{
			get
			{
				return buffer;
			}
		}
	}

}