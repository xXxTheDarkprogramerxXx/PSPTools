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

	/// <summary>
	/// generic packet header for saved capture stream
	/// immutable 
	/// </summary>
	public class CaptureHeader
	{

		// for use by other classes
		public const int PACKET_TYPE_RESERVED = 0;
		public const int PACKET_TYPE_LIST = 1;
		public const int PACKET_TYPE_RAM = 2;
		public const int PACKET_TYPE_DISPLAY_DETAILS = 3;
		public const int PACKET_TYPE_FRAMEBUF_DETAILS = 4;


		private const int size = 4;
		private int packetType;

		public CaptureHeader(int packetType)
		{
			this.packetType = packetType;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(java.io.OutputStream out) throws java.io.IOException
		public virtual void write(System.IO.Stream @out)
		{
			DataOutputStream data = new DataOutputStream(@out);
			data.writeInt(size);
			data.writeInt(packetType);
		}

		private CaptureHeader()
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static CaptureHeader read(java.io.InputStream in) throws java.io.IOException
		public static CaptureHeader read(System.IO.Stream @in)
		{
			CaptureHeader header = new CaptureHeader();

			DataInputStream data = new DataInputStream(@in);
			int sizeRemaining = data.readInt();
			if (sizeRemaining >= size)
			{
				header.packetType = data.readInt();
				sizeRemaining -= 4;
				data.skipBytes(sizeRemaining);
				//VideoEngine.log.info("CaptureHeader type " + header.packetType);
			}
			else
			{
				throw new IOException("Not enough bytes remaining in stream");
			}

			return header;
		}

		public virtual int PacketType
		{
			get
			{
				return packetType;
			}
		}
	}

}