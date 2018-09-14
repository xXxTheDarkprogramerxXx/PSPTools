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

	using Modules = pspsharp.HLE.Modules;
	using sceDisplay = pspsharp.HLE.modules.sceDisplay;

	/// <summary>
	/// captures sceDisplaySetFrameBuf </summary>
	public class CaptureFrameBufDetails
	{

		private const int packetSize = 4 * 4;
		private int topaddrFb;
		private int bufferwidthFb;
		private int pixelformatFb;
		private int sync;

		public CaptureFrameBufDetails()
		{
			sceDisplay display = Modules.sceDisplayModule;

			topaddrFb = display.TopAddrFb;
			bufferwidthFb = display.BufferWidthFb;
			pixelformatFb = display.PixelFormatFb;
			sync = display.Sync;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(java.io.OutputStream out) throws java.io.IOException
		public virtual void write(System.IO.Stream @out)
		{
			DataOutputStream data = new DataOutputStream(@out);

			data.writeInt(packetSize);

			data.writeInt(topaddrFb);
			data.writeInt(bufferwidthFb);
			data.writeInt(pixelformatFb);
			data.writeInt(sync);

			//VideoEngine.log.info("CaptureDisplayDetails write " + (4 + packetSize));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static CaptureFrameBufDetails read(java.io.InputStream in) throws java.io.IOException
		public static CaptureFrameBufDetails read(System.IO.Stream @in)
		{
			CaptureFrameBufDetails details = new CaptureFrameBufDetails();

			DataInputStream data = new DataInputStream(@in);
			int sizeRemaining = data.readInt();
			if (sizeRemaining >= packetSize)
			{
				details.topaddrFb = data.readInt();
				sizeRemaining -= 4;
				details.bufferwidthFb = data.readInt();
				sizeRemaining -= 4;
				details.pixelformatFb = data.readInt();
				sizeRemaining -= 4;
				details.sync = data.readInt();
				sizeRemaining -= 4;

				data.skipBytes(sizeRemaining);
			}
			else
			{
				throw new IOException("Not enough bytes remaining in stream");
			}

			return details;
		}

		public virtual void commit()
		{
			sceDisplay display = Modules.sceDisplayModule;

			// This is almost side effect free, but replay is going to trash the emulator state anyway
			display.hleDisplaySetFrameBuf(topaddrFb, bufferwidthFb, pixelformatFb, sync);
		}
	}

}