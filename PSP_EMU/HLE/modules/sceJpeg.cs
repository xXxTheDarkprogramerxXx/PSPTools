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
 */
namespace pspsharp.HLE.modules
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.IRenderingEngine_Fields.sizeOfTextureType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.ImageReader.colorARGBtoABGR;



	//using Logger = org.apache.log4j.Logger;

	using VideoEngine = pspsharp.graphics.VideoEngine;
	using PixelColor = pspsharp.graphics.RE.software.PixelColor;
	using Screen = pspsharp.hardware.Screen;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Settings = pspsharp.settings.Settings;
	using Utilities = pspsharp.util.Utilities;

	public class sceJpeg : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceJpeg");

		protected internal const int PSP_JPEG_MJPEG_DHT_MODE = 0;
		protected internal const int PSP_JPEG_MJPEG_NO_DHT_MODE = 1;

		protected internal int jpegWidth = Screen.width;
		protected internal int jpegHeight = Screen.height;
		protected internal Dictionary<int, BufferedImage> bufferedImages;
		protected internal const string uidPurpose = "sceJpeg-BufferedImage";
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal const bool dumpJpegFile_Renamed = false;

		public override void start()
		{
			bufferedImages = new Dictionary<int, BufferedImage>();
			base.start();
		}

		public override void stop()
		{
			bufferedImages.Clear();
			base.stop();
		}

		private static int clamp(float value)
		{
			return clamp8bit((int) value);
		}

		public static int clamp8bit(int value)
		{
			return System.Math.Min(0xFF, System.Math.Max(0, value));
		}

		private static int colorYCbCrToABGR(int y, int cb, int cr)
		{
			// based on http://en.wikipedia.org/wiki/Yuv#Y.27UV444_to_RGB888_conversion
			cb -= 128;
			cr -= 128;
			int r = clamp8bit(y + cr + (cr >> 2) + (cr >> 3) + (cr >> 5));
			int g = clamp8bit(y - ((cb >> 2) + (cb >> 4) + (cb >> 5)) - ((cr >> 1) + (cr >> 3) + (cr >> 4) + (cr >> 5)));
			int b = clamp8bit(y + cb + (cb >> 1) + (cb >> 2) + (cb >> 6));
			return PixelColor.getColorBGR(b, g, r) | unchecked((int)0xFF000000);
		}

		private static int colorARGBToYCbCr(int argb)
		{
			int r = (argb >> 16) & 0xFF;
			int g = (argb >> 8) & 0xFF;
			int b = argb & 0xFF;
			int y = clamp(0.299f * r + 0.587f * g + 0.114f * b);
			int cb = clamp(-0.169f * r - 0.331f * g + 0.499f * b + 128f);
			int cr = clamp(0.499f * r - 0.418f * g - 0.0813f * b + 128f);
			return PixelColor.getColorBGR(y, cb, cr);
		}

		private static int getY(int ycbcr)
		{
			return PixelColor.getBlue(ycbcr);
		}

		private static int getCb(int ycbcr)
		{
			return PixelColor.getGreen(ycbcr);
		}

		private static int getCr(int ycbcr)
		{
			return PixelColor.getRed(ycbcr);
		}

		protected internal static BufferedImage readJpegImage(TPointer jpegBuffer, int jpegBufferSize)
		{
			BufferedImage bufferedImage = null;
			sbyte[] buffer = readJpegImageBytes(jpegBuffer, jpegBufferSize);

			if (dumpJpegFile_Renamed)
			{
				dumpJpegFile(jpegBuffer, jpegBufferSize);
			}

			System.IO.Stream imageInputStream = new System.IO.MemoryStream(buffer);
			try
			{
				bufferedImage = ImageIO.read(imageInputStream);
				imageInputStream.Close();
			}
			catch (IOException e)
			{
				Console.WriteLine("Error reading Jpeg image", e);
			}

			return bufferedImage;
		}

		protected internal static int getWidthHeight(int width, int height)
		{
			return (width << 16) | height;
		}

		protected internal static int getWidth(int widthHeight)
		{
			return (widthHeight >> 16) & 0xFFF;
		}

		protected internal static int getHeight(int widthHeight)
		{
			return widthHeight & 0xFFF;
		}

		protected internal static sbyte[] readJpegImageBytes(TPointer jpegBuffer, int jpegBufferSize)
		{
			sbyte[] buffer = new sbyte[jpegBufferSize];
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(jpegBuffer.Address, jpegBufferSize, 1);
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = (sbyte) memoryReader.readNext();
			}

			return buffer;
		}

		protected internal static void dumpJpegFile(TPointer jpegBuffer, int jpegBufferSize)
		{
			sbyte[] buffer = readJpegImageBytes(jpegBuffer, jpegBufferSize);
			try
			{
				System.IO.Stream os = new System.IO.FileStream(string.Format("{0}{1}Image{2:X8}.jpeg", Settings.Instance.readString("emu.tmppath"), System.IO.Path.DirectorySeparatorChar, jpegBuffer.Address), System.IO.FileMode.Create, System.IO.FileAccess.Write);
				os.Write(buffer, 0, buffer.Length);
				os.Close();
			}
			catch (IOException e)
			{
				Console.WriteLine("Error dumping Jpeg file", e);
			}
		}

		protected internal virtual void decodeImage(TPointer imageBuffer, BufferedImage bufferedImage, int width, int height, int bufferWidth, int pixelFormat, int startLine)
		{
			width = System.Math.Min(width, bufferedImage.Width);
			height = System.Math.Min(height, bufferedImage.Height);

			int bytesPerPixel = sizeOfTextureType[pixelFormat];
			int lineWidth = System.Math.Min(width, bufferWidth);
			int skipEndOfLine = System.Math.Max(0, bufferWidth - lineWidth);
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(imageBuffer.Address, height * bufferWidth * bytesPerPixel, bytesPerPixel);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int argb = bufferedImage.getRGB(x, y + startLine);
					int abgr = colorARGBtoABGR(argb);
					memoryWriter.writeNext(abgr);
				}
				memoryWriter.skip(skipEndOfLine);
			}
			memoryWriter.flush();

			VideoEngine.Instance.addVideoTexture(imageBuffer.Address, imageBuffer.Address + bufferWidth * height * sceDisplay.getPixelFormatBytes(pixelFormat));
		}

		protected internal virtual void generateFakeImage(TPointer imageBuffer, int width, int height, int bufferWidth, int pixelFormat)
		{
			sceMpeg.generateFakeImage(imageBuffer.Address, bufferWidth, width, height, pixelFormat);
			VideoEngine.Instance.addVideoTexture(imageBuffer.Address, imageBuffer.Address + bufferWidth * height * sceDisplay.getPixelFormatBytes(pixelFormat));
		}

		public virtual int hleGetYCbCrBufferSize(BufferedImage bufferedImage)
		{
			// Return necessary buffer size for conversion: 12 bits per pixel
			return ((bufferedImage.Width * bufferedImage.Height) >> 1) * 3;
		}

		/// <summary>
		/// Convert an image to YUV420p format.
		/// 
		/// See http://en.wikipedia.org/wiki/Yuv#Y.27UV420p_.28and_Y.27V12_or_YV12.29_to_RGB888_conversion
		/// for the description of the YUV420p format:
		/// 
		/// "Y'UV420p is a planar format, meaning that the Y', U, and V values are grouped together instead of interspersed.
		///  The reason for this is that by grouping the U and V values together, the image becomes much more compressible.
		///  When given an array of an image in the Y'UV420p format, all the Y' values come first,
		///  followed by all the U values, followed finally by all the V values.
		/// 
		///  As with most Y'UV formats, there are as many Y' values as there are pixels.
		///  Where X equals the height multiplied by the width,
		///  the first X indices in the array are Y' values that correspond to each individual pixel.
		///  However, there are only one fourth as many U and V values.
		///  The U and V values correspond to each 2 by 2 block of the image,
		///  meaning each U and V entry applies to four pixels. After the Y' values,
		///  the next X/4 indices are the U values for each 2 by 2 block,
		///  and the next X/4 indices after that are the V values that also apply to each 2 by 2 block.
		/// 
		///		size.total = size.width * size.height;
		///		y = yuv[position.y * size.width + position.x];
		///		u = yuv[(position.y / 2) * (size.width / 2) + (position.x / 2) + size.total];
		///		v = yuv[(position.y / 2) * (size.width / 2) + (position.x / 2) + size.total + (size.total / 4)];
		///		rgb = Y'UV444toRGB888(y, u, v);
		/// "
		/// </summary>
		/// <param name="bufferedImage">		the source image. </param>
		/// <param name="yCbCrBuffer">		the destination image in YUV420p format. </param>
		/// <param name="yCbCrBufferSize">	the size of the destination buffer. </param>
		/// <param name="dhtMode">			unknown.
		/// @return					the width & height of the image. </param>
		public virtual int hleJpegDecodeYCbCr(BufferedImage bufferedImage, TPointer yCbCrBuffer, int yCbCrBufferSize, int dhtMode)
		{
			int width = bufferedImage.Width;
			int height = bufferedImage.Height;

			int sizeY = width * height;
			int sizeCb = sizeY >> 2;
			int addressY = yCbCrBuffer.Address;
			int addressCb = addressY + sizeY;
			int addressCr = addressCb + sizeCb;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("hleJpegDecodeYCbCr 0x{0:X8}, 0x{1:X8}, 0x{2:X8}", addressY, addressCb, addressCr));
			}

			// Store all the Cb and Cr values into an array as they will not be accessed sequentially.
			int[] bufferCb = new int[sizeCb];
			int[] bufferCr = new int[sizeCb];
			IMemoryWriter imageWriterY = MemoryWriter.getMemoryWriter(addressY, sizeY, 1);
			for (int y = 0; y < height; y++)
			{
				int indexCb = (y >> 1) * (width >> 1);
				for (int x = 0; x < width; x += 2, indexCb++)
				{
					int argb0 = bufferedImage.getRGB(x, y);
					int yCbCr0 = colorARGBToYCbCr(argb0);
					int argb1 = bufferedImage.getRGB(x + 1, y);
					int yCbCr1 = colorARGBToYCbCr(argb1);
					imageWriterY.writeNext(getY(yCbCr0));
					imageWriterY.writeNext(getY(yCbCr1));

					bufferCb[indexCb] += getCb(yCbCr0);
					bufferCb[indexCb] += getCb(yCbCr1);
					bufferCr[indexCb] += getCr(yCbCr0);
					bufferCr[indexCb] += getCr(yCbCr1);
				}
			}
			imageWriterY.flush();

			IMemoryWriter imageWriterCb = MemoryWriter.getMemoryWriter(addressCb, sizeCb, 1);
			IMemoryWriter imageWriterCr = MemoryWriter.getMemoryWriter(addressCr, sizeCb, 1);
			for (int i = 0; i < sizeCb; i++)
			{
				// 4 pixel values have been written for each Cb and Cr value, average them.
				imageWriterCb.writeNext(bufferCb[i] >> 2);
				imageWriterCr.writeNext(bufferCr[i] >> 2);
			}
			imageWriterCb.flush();
			imageWriterCr.flush();

			return getWidthHeight(width, height);
		}

		protected internal virtual int hleJpegDecodeMJpegYCbCr(TPointer jpegBuffer, int jpegBufferSize, TPointer yCbCrBuffer, int yCbCrBufferSize, int dhtMode)
		{
			BufferedImage bufferedImage = readJpegImage(jpegBuffer, jpegBufferSize);
			if (bufferedImage == null)
			{
				yCbCrBuffer.clear(yCbCrBufferSize);
				return getWidthHeight(0, 0);
			}

			return hleJpegDecodeYCbCr(bufferedImage, yCbCrBuffer, yCbCrBufferSize, dhtMode);
		}

		/// <summary>
		/// Convert an image in YUV420p format to ABGR8888.
		/// 
		/// See http://en.wikipedia.org/wiki/Yuv#Y.27UV420p_.28and_Y.27V12_or_YV12.29_to_RGB888_conversion
		/// for the description of the YUV420p format:
		/// 
		/// "Y'UV420p is a planar format, meaning that the Y', U, and V values are grouped together instead of interspersed.
		///  The reason for this is that by grouping the U and V values together, the image becomes much more compressible.
		///  When given an array of an image in the Y'UV420p format, all the Y' values come first,
		///  followed by all the U values, followed finally by all the V values.
		/// 
		///  As with most Y'UV formats, there are as many Y' values as there are pixels.
		///  Where X equals the height multiplied by the width,
		///  the first X indices in the array are Y' values that correspond to each individual pixel.
		///  However, there are only one fourth as many U and V values.
		///  The U and V values correspond to each 2 by 2 block of the image,
		///  meaning each U and V entry applies to four pixels. After the Y' values,
		///  the next X/4 indices are the U values for each 2 by 2 block,
		///  and the next X/4 indices after that are the V values that also apply to each 2 by 2 block.
		/// 
		///		size.total = size.width * size.height;
		///		y = yuv[position.y * size.width + position.x];
		///		u = yuv[(position.y / 2) * (size.width / 2) + (position.x / 2) + size.total];
		///		v = yuv[(position.y / 2) * (size.width / 2) + (position.x / 2) + size.total + (size.total / 4)];
		///		rgb = Y'UV444toRGB888(y, u, v);
		/// "
		/// </summary>
		/// <param name="imageBuffer">	output image in ABGR8888 format. </param>
		/// <param name="yCbCrBuffer">   input image in YUV420p format. </param>
		/// <param name="widthHeight">   width & height of the image </param>
		/// <param name="bufferWidth">   buffer width of the image </param>
		/// <returns>               0 </returns>
		protected internal virtual int hleJpegCsc(TPointer imageBuffer, TPointer yCbCrBuffer, int widthHeight, int bufferWidth)
		{
			int height = getHeight(widthHeight);
			int width = getWidth(widthHeight);

			int pixelFormat = TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
			int bytesPerPixel = sizeOfTextureType[pixelFormat];
			int lineWidth = System.Math.Min(width, bufferWidth);
			int skipEndOfLine = System.Math.Max(0, bufferWidth - lineWidth);
			int imageSizeInBytes = height * bufferWidth * bytesPerPixel;
			IMemoryWriter imageWriter = MemoryWriter.getMemoryWriter(imageBuffer.Address, imageSizeInBytes, bytesPerPixel);

			int sizeY = width * height;
			int sizeCb = sizeY >> 2;
			int addressY = yCbCrBuffer.Address;
			int addressCb = addressY + sizeY;
			int addressCr = addressCb + sizeCb;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("hleJpegCsc 0x{0:X8}, 0x{1:X8}, 0x{2:X8}", addressY, addressCb, addressCr));
			}

			// Read all the Cb and Cr values into an array as they will not be accessed sequentially.
			int[] bufferCb = new int[sizeCb];
			int[] bufferCr = new int[sizeCb];
			IMemoryReader imageReaderCb = MemoryReader.getMemoryReader(addressCb, sizeCb, 1);
			IMemoryReader imageReaderCr = MemoryReader.getMemoryReader(addressCr, sizeCb, 1);
			for (int i = 0; i < sizeCb; i++)
			{
				bufferCb[i] = imageReaderCb.readNext();
				bufferCr[i] = imageReaderCr.readNext();
			}

			IMemoryReader imageReaderY = MemoryReader.getMemoryReader(addressY, sizeY, 1);
			for (int y = 0; y < height; y++)
			{
				int indexCb = (y >> 1) * (width >> 1);
				for (int x = 0; x < width; x += 2, indexCb++)
				{
					int y0 = imageReaderY.readNext();
					int y1 = imageReaderY.readNext();
					int cb = bufferCb[indexCb];
					int cr = bufferCr[indexCb];

					// Convert yCbCr to ABGR
					int abgr0 = colorYCbCrToABGR(y0, cb, cr);
					int abgr1 = colorYCbCrToABGR(y1, cb, cr);

					// Write ABGR
					imageWriter.writeNext(abgr0);
					imageWriter.writeNext(abgr1);
				}
				imageWriter.skip(skipEndOfLine);
			}
			imageWriter.flush();

			VideoEngine.Instance.addVideoTexture(imageBuffer.Address, imageBuffer.Address + imageSizeInBytes);

			return 0;
		}

		[HLEFunction(nid : 0x04B5AE02, version : 271)]
		public virtual int sceJpegMJpegCsc(TPointer imageBuffer, TPointer yCbCrBuffer, int widthHeight, int bufferWidth)
		{
			return hleJpegCsc(imageBuffer, yCbCrBuffer, widthHeight, bufferWidth);
		}

		/// <summary>
		/// Deletes the current decoder context.
		/// </summary>
		/// <returns> 0 on success, < 0 on error </returns>
		[HLEFunction(nid : 0x48B602B7, version : 271)]
		public virtual int sceJpegDeleteMJpeg()
		{
			return 0;
		}

		/// <summary>
		/// Finishes the MJpeg library
		/// </summary>
		/// <returns> 0 on success, < 0 on error </returns>
		[HLEFunction(nid : 0x7D2F3D7F, version : 271)]
		public virtual int sceJpegFinishMJpeg()
		{
			return 0;
		}

		[HLEFunction(nid : 0x91EED83C, version : 271)]
		public virtual int sceJpegDecodeMJpegYCbCr(TPointer jpegBuffer, int jpegBufferSize, TPointer yCbCrBuffer, int yCbCrBufferSize, int dhtMode)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceJpegDecodeMJpegYCbCr jpegBuffer: {0}", Utilities.getMemoryDump(jpegBuffer.Address, jpegBufferSize)));
			}

			return hleJpegDecodeMJpegYCbCr(jpegBuffer, jpegBufferSize, yCbCrBuffer, yCbCrBufferSize, dhtMode);
		}

		/// <summary>
		/// Creates the decoder context.
		/// </summary>
		/// <param name="width"> - The width of the frame </param>
		/// <param name="height"> - The height of the frame
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
		[HLELogging(level : "info"), HLEFunction(nid : 0x9D47469C, version : 271)]
		public virtual int sceJpegCreateMJpeg(int width, int height)
		{
			jpegWidth = width;
			jpegHeight = height;

			return 0;
		}

		/// <summary>
		/// Inits the MJpeg library
		/// </summary>
		/// <returns> 0 on success, < 0 on error </returns>
		[HLEFunction(nid : 0xAC9E70E6, version : 271)]
		public virtual int sceJpegInitMJpeg()
		{
			return 0;
		}

		/// <summary>
		/// Decodes a mjpeg frame.
		/// </summary>
		/// <param name="jpegbuf"> - the buffer with the mjpeg frame </param>
		/// <param name="size"> - size of the buffer pointed by jpegbuf </param>
		/// <param name="rgba"> - buffer where the decoded data in RGBA format will be
		/// stored. It should have a size of (width * height * 4). </param>
		/// <param name="dht"> - flag telling if this mjpeg has a DHT (Define Huffman Table)
		/// header or not.
		/// </param>
		/// <returns> (width << 16) + height on success, < 0 on error </returns>
		[HLEFunction(nid : 0x04B93CEF, version : 271)]
		public virtual int sceJpegDecodeMJpeg(TPointer jpegBuffer, int jpegBufferSize, TPointer imageBuffer, int dhtMode)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceJpegDecodeMJpeg jpegBuffer: {0}", Utilities.getMemoryDump(jpegBuffer.Address, jpegBufferSize)));
			}

			int pixelFormat = TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
			BufferedImage bufferedImage = readJpegImage(jpegBuffer, jpegBufferSize);
			if (bufferedImage == null)
			{
				generateFakeImage(imageBuffer, jpegWidth, jpegHeight, jpegWidth, pixelFormat);
			}
			else
			{
				decodeImage(imageBuffer, bufferedImage, jpegWidth, jpegHeight, jpegWidth, pixelFormat, 0);
			}

			// Return size of image
			return getWidthHeight(jpegWidth, jpegHeight);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8F2BB012, version = 271) public int sceJpegGetOutputInfo(pspsharp.HLE.TPointer jpegBuffer, int jpegBufferSize, @CanBeNull pspsharp.HLE.TPointer32 colorInfoBuffer, int dhtMode)
		[HLEFunction(nid : 0x8F2BB012, version : 271)]
		public virtual int sceJpegGetOutputInfo(TPointer jpegBuffer, int jpegBufferSize, TPointer32 colorInfoBuffer, int dhtMode)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceJpegGetOutputInfo jpegBuffer: {0}", Utilities.getMemoryDump(jpegBuffer.Address, jpegBufferSize)));
			}

			if (dumpJpegFile_Renamed)
			{
				dumpJpegFile(jpegBuffer, jpegBufferSize);
			}

			// Buffer to store info about the color space in use.
			// - Bits 24 to 32 (Always empty): 0x00
			// - Bits 16 to 24 (Color mode): 0x00 (Unknown), 0x01 (Greyscale) or 0x02 (YCbCr) 
			// - Bits 8 to 16 (Vertical chroma subsampling value): 0x00, 0x01 or 0x02
			// - Bits 0 to 8 (Horizontal chroma subsampling value): 0x00, 0x01 or 0x02
			if (colorInfoBuffer.NotNull)
			{
				colorInfoBuffer.setValue(0x00020202);
			}

			BufferedImage bufferedImage = readJpegImage(jpegBuffer, jpegBufferSize);
			if (bufferedImage == null)
			{
				return 0xC000;
			}

			return hleGetYCbCrBufferSize(bufferedImage);
		}

		/// <summary>
		/// Used in relation with sceMpegAvcConvertToYuv420. Maybe
		/// converting a Yuv420 image to ABGR888?
		/// </summary>
		/// <param name="imageBuffer"> </param>
		/// <param name="yCbCrBuffer"> </param>
		/// <param name="widthHeight"> </param>
		/// <param name="bufferWidth"> </param>
		/// <param name="colorInfo">
		/// @return </param>
		[HLEFunction(nid : 0x67F0ED84, version : 271)]
		public virtual int sceJpegCsc(TPointer imageBuffer, TPointer yCbCrBuffer, int widthHeight, int bufferWidth, int colorInfo)
		{
			return hleJpegCsc(imageBuffer, yCbCrBuffer, widthHeight, bufferWidth);
		}

		[HLEFunction(nid : 0x64B6F978, version : 271)]
		public virtual int sceJpegDecodeMJpegSuccessively(TPointer jpegBuffer, int jpegBufferSize, TPointer imageBuffer, int dhtMode)
		{
			// Works in the same way as sceJpegDecodeMJpeg, but sends smaller blocks to the Media Engine in a real PSP (avoids speed decrease).
			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceJpegDecodeMJpegSuccessively jpegBuffer: {0}", Utilities.getMemoryDump(jpegBuffer.Address, jpegBufferSize)));
			}

			int pixelFormat = TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
			BufferedImage bufferedImage = readJpegImage(jpegBuffer, jpegBufferSize);
			int width = jpegWidth;
			int height = jpegHeight;
			if (bufferedImage == null)
			{
				generateFakeImage(imageBuffer, jpegWidth, jpegHeight, jpegWidth, pixelFormat);
			}
			else
			{
				decodeImage(imageBuffer, bufferedImage, jpegWidth, jpegHeight, jpegWidth, pixelFormat, 0);
				width = bufferedImage.Width;
				height = bufferedImage.Height;
			}

			return getWidthHeight(width, height);
		}

		[HLEFunction(nid : 0x227662D7, version : 271)]
		public virtual int sceJpegDecodeMJpegYCbCrSuccessively(TPointer jpegBuffer, int jpegBufferSize, TPointer yCbCrBuffer, int yCbCrBufferSize, int dhtMode)
		{
			// Works in the same way as sceJpegDecodeMJpegYCbCr, but sends smaller blocks to the Media Engine in a real PSP (avoids speed decrease).
			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceJpegDecodeMJpegYCbCrSuccessively jpegBuffer: {0}", Utilities.getMemoryDump(jpegBuffer.Address, jpegBufferSize)));
			}

			return hleJpegDecodeMJpegYCbCr(jpegBuffer, jpegBufferSize, yCbCrBuffer, yCbCrBufferSize, dhtMode);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA06A75C4, version = 271) public int sceJpeg_A06A75C4(int unknown1, int unknown2, int unknown3, int unknown4, int unknown5)
		[HLEFunction(nid : 0xA06A75C4, version : 271)]
		public virtual int sceJpeg_A06A75C4(int unknown1, int unknown2, int unknown3, int unknown4, int unknown5)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9B36444C, version = 271) public int sceJpeg_9B36444C()
		[HLEFunction(nid : 0x9B36444C, version : 271)]
		public virtual int sceJpeg_9B36444C()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0425B986, version = 271) public int sceJpegDecompressAllImage()
		[HLEFunction(nid : 0x0425B986, version : 271)]
		public virtual int sceJpegDecompressAllImage()
		{
			return 0;
		}
	}
}