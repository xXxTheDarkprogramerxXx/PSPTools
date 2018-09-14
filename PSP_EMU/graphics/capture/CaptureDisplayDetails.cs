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
	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;

	/// <summary>
	/// captures draw, depth and display buffers along with their settings (width, height, etc) </summary>
	public class CaptureDisplayDetails
	{

		private const bool captureRenderTargets = false;

		private const int packetSize = (5 + 4) * 4;

		private int fbp;
		private int fbw;
		private int zbp;
		private int zbw;
		private int psm;

		private int topaddrFb;
		private int bufferwidthFb;
		private int pixelformatFb;
		private int sync;

		private CaptureRAM drawBuffer;
		private CaptureRAM depthBuffer;
		private CaptureRAM displayBuffer;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public CaptureDisplayDetails() throws java.io.IOException
		public CaptureDisplayDetails()
		{
			VideoEngine ge = VideoEngine.Instance;
			sceDisplay display = Modules.sceDisplayModule;

			fbp = ge.FBP;
			fbw = ge.FBW;
			zbp = ge.ZBP;
			zbw = ge.ZBW;
			psm = ge.PSM;

			topaddrFb = display.TopAddrFb;
			bufferwidthFb = display.BufferWidthFb;
			pixelformatFb = display.PixelFormatFb;
			sync = display.Sync;

			// TODO clamp lengths to within valid RAM range
			int pixelFormatBytes = pspsharp.graphics.RE.IRenderingEngine_Fields.sizeOfTextureType[psm];
			drawBuffer = new CaptureRAM(fbp + MemoryMap.START_VRAM, fbw * 272 * pixelFormatBytes);

			depthBuffer = new CaptureRAM(zbp + MemoryMap.START_VRAM, zbw * 272 * 2);

			pixelFormatBytes = pspsharp.graphics.RE.IRenderingEngine_Fields.sizeOfTextureType[pixelformatFb];
			displayBuffer = new CaptureRAM(topaddrFb, bufferwidthFb * 272 * pixelFormatBytes);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(java.io.OutputStream out) throws java.io.IOException
		public virtual void write(System.IO.Stream @out)
		{
			DataOutputStream data = new DataOutputStream(@out);

			data.writeInt(packetSize);
			data.writeInt(fbp);
			data.writeInt(fbw);
			data.writeInt(zbp);
			data.writeInt(zbw);
			data.writeInt(psm);

			data.writeInt(topaddrFb);
			data.writeInt(bufferwidthFb);
			data.writeInt(pixelformatFb);
			data.writeInt(sync);

			//VideoEngine.log.info("CaptureDisplayDetails write " + (4 + packetSize));

			if (captureRenderTargets)
			{
				// write draw buffer
				CaptureHeader header = new CaptureHeader(CaptureHeader.PACKET_TYPE_RAM);
				header.write(@out);
				drawBuffer.write(@out);

				// write depth buffer
				header = new CaptureHeader(CaptureHeader.PACKET_TYPE_RAM);
				header.write(@out);
				depthBuffer.write(@out);

				// write display buffer
				header = new CaptureHeader(CaptureHeader.PACKET_TYPE_RAM);
				header.write(@out);
				displayBuffer.write(@out);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static CaptureDisplayDetails read(java.io.InputStream in) throws java.io.IOException
		public static CaptureDisplayDetails read(System.IO.Stream @in)
		{
			CaptureDisplayDetails details = new CaptureDisplayDetails();

			DataInputStream data = new DataInputStream(@in);
			int sizeRemaining = data.readInt();
			if (sizeRemaining >= packetSize)
			{
				details.fbp = data.readInt();
				sizeRemaining -= 4;
				details.fbw = data.readInt();
				sizeRemaining -= 4;
				details.zbp = data.readInt();
				sizeRemaining -= 4;
				details.zbw = data.readInt();
				sizeRemaining -= 4;
				details.psm = data.readInt();
				sizeRemaining -= 4;

				details.topaddrFb = data.readInt();
				sizeRemaining -= 4;
				details.bufferwidthFb = data.readInt();
				sizeRemaining -= 4;
				details.pixelformatFb = data.readInt();
				sizeRemaining -= 4;
				details.sync = data.readInt();
				sizeRemaining -= 4;

				data.skipBytes(sizeRemaining);

				if (captureRenderTargets)
				{
					// read draw, depth and display buffers
					CaptureHeader header = CaptureHeader.read(@in);
					int packetType = header.PacketType;
					if (packetType != CaptureHeader.PACKET_TYPE_RAM)
					{
						throw new IOException("Expected CaptureRAM(" + CaptureHeader.PACKET_TYPE_RAM + ") packet, found " + packetType);
					}
					details.drawBuffer = CaptureRAM.read(@in);

					header = CaptureHeader.read(@in);
					packetType = header.PacketType;
					if (packetType != CaptureHeader.PACKET_TYPE_RAM)
					{
						throw new IOException("Expected CaptureRAM(" + CaptureHeader.PACKET_TYPE_RAM + ") packet, found " + packetType);
					}
					details.depthBuffer = CaptureRAM.read(@in);

					header = CaptureHeader.read(@in);
					packetType = header.PacketType;
					if (packetType != CaptureHeader.PACKET_TYPE_RAM)
					{
						throw new IOException("Expected CaptureRAM(" + CaptureHeader.PACKET_TYPE_RAM + ") packet, found " + packetType);
					}
					details.displayBuffer = CaptureRAM.read(@in);
				}
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
			//VideoEngine ge = VideoEngine.getInstance();

			// This is almost side effect free, but replay is going to trash the emulator state anyway
			display.hleDisplaySetFrameBuf(topaddrFb, bufferwidthFb, pixelformatFb, sync);
			display.hleDisplaySetGeBuf(fbp, fbw, psm, false, false);

			if (captureRenderTargets)
			{
				drawBuffer.commit();
				depthBuffer.commit();
				displayBuffer.commit();
			}
		}
	}

}