using System;
using System.Collections.Generic;

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

assumes:
- list contains fbw/fbp command
- list clears the screen
- sceDisplaySetFrameBuf is called after the list has executed

todo:
- need to save GE state
  - texture on/off
  - blend on/off + params.
  - save matrices, looks like something wrong with projection
  - more ...
- don't save the same piece of ram twice (multiple texture uploads of the same texture/clut)
- capture multiple lists per frame, ideally we want to capture everything between two calls to sceDisplaySetFrameBuf
*/

namespace pspsharp.graphics.capture
{

	using PspGeList = pspsharp.HLE.kernel.types.PspGeList;

	using Level = org.apache.log4j.Level;

	public class CaptureManager
	{

		private static System.IO.Stream @out;
		public static bool captureInProgress;
		private static bool listExecuted;
		private static CaptureFrameBufDetails replayFrameBufDetails;
		private static Level logLevel;
		private static HashSet<int> capturedImages;

		public static void startReplay(string filename)
		{
			if (captureInProgress)
			{
				VideoEngine.log_Renamed.error("Ignoring startReplay, capture is in progress");
				return;
			}

			VideoEngine.log_Renamed.info("Starting replay: " + filename);

			try
			{
				System.IO.Stream @in = new BufferedInputStream(new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read));

				while (@in.available() > 0)
				{
					CaptureHeader header = CaptureHeader.read(@in);
					int packetType = header.PacketType;

					switch (packetType)
					{
						case CaptureHeader.PACKET_TYPE_LIST:
							CaptureList list = CaptureList.read(@in);
							list.commit();
							break;

						case CaptureHeader.PACKET_TYPE_RAM:
							CaptureRAM ramFragment = CaptureRAM.read(@in);
							ramFragment.commit();
							break;

						// deprecated
						case CaptureHeader.PACKET_TYPE_DISPLAY_DETAILS:
							CaptureDisplayDetails displayDetails = CaptureDisplayDetails.read(@in);
							displayDetails.commit();
							break;

						case CaptureHeader.PACKET_TYPE_FRAMEBUF_DETAILS:
							// don't replay this one immediately, wait until after the list has finished executing
							replayFrameBufDetails = CaptureFrameBufDetails.read(@in);
							break;

						default:
							throw new Exception("Unknown packet type " + packetType);
					}
				}

				@in.Close();
			}
			catch (Exception e)
			{
				VideoEngine.log_Renamed.error("Failed to start replay: " + e.Message);
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		public static void endReplay()
		{
			// replay final sceDisplaySetFrameBuf
			replayFrameBufDetails.commit();
			replayFrameBufDetails = null;

			VideoEngine.log_Renamed.info("Replay completed");
			Emulator.PauseEmu();
		}

		public static void startCapture(string filename, PspGeList list)
		{
		//public static void startCapture(int displayBufferAddress, int displayBufferWidth, int displayBufferPsm,
		//    int drawBufferAddress, int drawBufferWidth, int drawBufferPsm,
		//    int depthBufferAddress, int depthBufferWidth) {
			if (captureInProgress)
			{
				VideoEngine.log_Renamed.error("Ignoring startCapture, capture is already in progress");
				return;
			}

			// Set the VideoEngine log level to TRACE when capturing,
			// the information in the log file is also interesting
			logLevel = VideoEngine.log_Renamed.Level;
			VideoEngine.Instance.LogLevel = Level.TRACE;
			capturedImages = new HashSet<int>();

			try
			{
				VideoEngine.log_Renamed.info("Starting capture... (list=" + list.id + ")");
				@out = new BufferedOutputStream(new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write));

				CaptureHeader header;

				/*
				// write render target details
				header = new CaptureHeader(CaptureHeader.PACKET_TYPE_DISPLAY_DETAILS);
				header.write(out);
				CaptureDisplayDetails displayDetails = new CaptureDisplayDetails();
				displayDetails.write(out);
				*/

				// write command buffer
				header = new CaptureHeader(CaptureHeader.PACKET_TYPE_LIST);
				header.write(@out);
				CaptureList commandList = new CaptureList(list);
				commandList.write(@out);

				captureInProgress = true;
				listExecuted = false;
			}
			catch (Exception e)
			{
				VideoEngine.log_Renamed.error("Failed to start capture: " + e.Message);
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Emulator.PauseEmu();
			}
		}

		public static void endCapture()
		{
			if (!captureInProgress)
			{
				VideoEngine.log_Renamed.warn("Ignoring endCapture, capture hasn't been started");
				Emulator.PauseEmu();
				return;
			}

			try
			{
				@out.Flush();
				@out.Close();
				@out = null;
			}
			catch (Exception e)
			{
				VideoEngine.log_Renamed.error("Failed to end capture: " + e.Message);
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Emulator.PauseEmu();
			}

			captureInProgress = false;

			VideoEngine.log_Renamed.info("Capture completed");
			VideoEngine.log_Renamed.Level = logLevel;
			Emulator.PauseEmu();
		}

		public static void captureRAM(int address, int length)
		{
			if (!captureInProgress)
			{
				VideoEngine.log_Renamed.warn("Ignoring captureRAM, capture hasn't been started");
				return;
			}

			if (!Memory.isAddressGood(address))
			{
				return;
			}

			try
			{
				// write ram fragment
				CaptureHeader header = new CaptureHeader(CaptureHeader.PACKET_TYPE_RAM);
				header.write(@out);

				CaptureRAM captureRAM = new CaptureRAM(address, length);
				captureRAM.write(@out);
			}
			catch (Exception e)
			{
				VideoEngine.log_Renamed.error("Failed to capture RAM: " + e.Message);
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		public static void captureImage(int imageaddr, int level, Buffer buffer, int width, int height, int bufferWidth, int imageType, bool compressedImage, int compressedImageSize, bool invert, bool overwriteFile)
		{
			try
			{
				// write image to the file system, not to the capture file itself
				CaptureImage captureImage = new CaptureImage(imageaddr, level, buffer, width, height, bufferWidth, imageType, compressedImage, compressedImageSize, invert, overwriteFile, null);
				captureImage.write();
				if (capturedImages != null)
				{
					capturedImages.Add(imageaddr);
				}
			}
			catch (Exception e)
			{
				VideoEngine.log_Renamed.error("Failed to capture Image: " + e.Message);
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Emulator.PauseEmu();
			}
		}

		public static bool isImageCaptured(int imageaddr)
		{
			if (capturedImages == null)
			{
				return false;
			}

			return capturedImages.Contains(imageaddr);
		}

		public static void captureFrameBufDetails()
		{
			if (!captureInProgress)
			{
				VideoEngine.log_Renamed.warn("Ignoring captureRAM, capture hasn't been started");
				return;
			}

			try
			{
				CaptureHeader header = new CaptureHeader(CaptureHeader.PACKET_TYPE_FRAMEBUF_DETAILS);
				header.write(@out);

				CaptureFrameBufDetails details = new CaptureFrameBufDetails();
				details.write(@out);
			}
			catch (Exception e)
			{
				VideoEngine.log_Renamed.error("Failed to capture frame buf details: " + e.Message);
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Emulator.PauseEmu();
			}
		}

		public static void markListExecuted()
		{
			listExecuted = true;
		}

		public static bool hasListExecuted()
		{
			return listExecuted;
		}
	}

}