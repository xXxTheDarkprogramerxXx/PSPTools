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
 */
namespace pspsharp.graphics.capture
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR4444;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR5551;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_INDEXED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_4BIT_INDEXED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_DXT1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_DXT3;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_DXT5;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DEPTH_STENCIL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.BaseRenderer.depthBufferPixelFormat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.memory.ImageReader.colorABGRtoARGB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.alignUp;


	using Logger = org.apache.log4j.Logger;

	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class CaptureImage
	{
		private static Logger log = VideoEngine.log_Renamed;
		private const string bmpFileFormat = "bmp";
		private const string pngFileFormat = "png";
		private int imageaddr;
		private int level;
		private Buffer buffer;
		private IMemoryReader imageReader;
		private int width;
		private int height;
		private int bufferWidth;
		private int bufferStorage;
		private bool compressedImage;
		private int compressedImageSize;
		private bool invert;
		private bool overwriteFile;
		private string fileNamePrefix;
		private string directory = "tmp/";
		private string fileFormat = bmpFileFormat;
		private string fileName;
		private string fileNameSuffix = "";
		private static Dictionary<int, int> lastFileIndex = new Dictionary<int, int>();

		private abstract class AbstractCaptureImage
		{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeHeader(String fileName, String fileFormat, int width, int height, int readWidth) throws java.io.IOException;
			public abstract void writeHeader(string fileName, string fileFormat, int width, int height, int readWidth);
			public abstract void startLine(int y);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writePixel(int pixel) throws java.io.IOException;
			public abstract void writePixel(int pixel);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract void writeEnd() throws java.io.IOException;
			public abstract void writeEnd();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writePixel(byte[] pixel) throws java.io.IOException
			public virtual void writePixel(sbyte[] pixel)
			{
				writePixel(getARGB(pixel));
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void endLine() throws java.io.IOException
			public virtual void endLine()
			{
			}

			public virtual bool Inverted
			{
				get
				{
					return false;
				}
			}
		}

		// See http://en.wikipedia.org/wiki/BMP_file_format
		// for detailed information about the BMP file format
		private class CaptureImageBMP : AbstractCaptureImage
		{
			internal int imageRawBytes;
			internal sbyte[] completeImageBytes;
			internal int pixelIndex;
			internal int width;
			internal int height;
			internal string fileName;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeHeader(String fileName, String fileFormat, int width, int height, int readWidth) throws java.io.IOException
			public override void writeHeader(string fileName, string fileFormat, int width, int height, int readWidth)
			{
				this.fileName = fileName;
				this.width = width;
				this.height = height;
				int rowPad = (4 - ((width * 4) & 3)) & 3;
				imageRawBytes = (width * 4) + rowPad;

				completeImageBytes = new sbyte[height * imageRawBytes];
			}


			public override void startLine(int y)
			{
				pixelIndex = y * imageRawBytes;
			}

			public override void writePixel(int pixel)
			{
				completeImageBytes[pixelIndex + 0] = (sbyte)(pixel >> 16); // B
				completeImageBytes[pixelIndex + 1] = (sbyte)(pixel >> 8); // G
				completeImageBytes[pixelIndex + 2] = (sbyte)(pixel); // R
				completeImageBytes[pixelIndex + 3] = (sbyte)(pixel >> 24); // A

				pixelIndex += 4;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeEnd() throws java.io.IOException
			public override void writeEnd()
			{
				sbyte[] fileHeader = new sbyte[14];
				sbyte[] dibHeader = new sbyte[56];
				int fileSize = fileHeader.Length + dibHeader.Length + completeImageBytes.Length;

				fileHeader[0] = (sbyte)'B'; // Magic number
				fileHeader[1] = (sbyte)'M'; // Magic number
				storeLittleEndianInt(fileHeader, 2, fileSize); // Size of the BMP file
				storeLittleEndianInt(fileHeader, 10, fileHeader.Length + dibHeader.Length); // Offset where the Pixel Array (bitmap data) can be found

				storeLittleEndianInt(dibHeader, 0, dibHeader.Length); // Number of bytes in the DIB header (from this point)
				storeLittleEndianInt(dibHeader, 4, width); // Width of the bitmap in pixels
				storeLittleEndianInt(dibHeader, 8, height); // Height of the bitmap in pixels
				storeLittleEndianShort(dibHeader, 12, 1); // Number of color planes being used
				storeLittleEndianShort(dibHeader, 14, 32); // Number of bits per pixel
				storeLittleEndianInt(dibHeader, 16, 0); // BI_BITFIELDS, no Pixel Array compression used
				storeLittleEndianInt(dibHeader, 20, completeImageBytes.Length); // Size of the raw data in the Pixel Array (including padding)
				storeLittleEndianInt(dibHeader, 24, 2835); // Horizontal physical resolution of the image (pixels/meter)
				storeLittleEndianInt(dibHeader, 28, 2835); // Vertical physical resolution of the image (pixels/meter)
				storeLittleEndianInt(dibHeader, 32, 0); // Number of colors in the palette
				storeLittleEndianInt(dibHeader, 36, 0); // 0 means all colors are important
				storeLittleEndianInt(dibHeader, 40, 0x00FF0000); // Red channel bit mask in big-endian (valid because BI_BITFIELDS is specified)
				storeLittleEndianInt(dibHeader, 44, 0x0000FF00); // Green channel bit mask in big-endian (valid because BI_BITFIELDS is specified)
				storeLittleEndianInt(dibHeader, 48, 0x000000FF); // Blue channel bit mask in big-endian (valid because BI_BITFIELDS is specified)
				storeLittleEndianInt(dibHeader, 52, unchecked((int)0xFF000000)); // Alpha channel bit mask in big-endian

				System.IO.Stream outStream = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				outStream.Write(fileHeader, 0, fileHeader.Length);
				outStream.Write(dibHeader, 0, dibHeader.Length);
				outStream.Write(completeImageBytes, 0, completeImageBytes.Length);
				outStream.Close();
			}

			public override bool Inverted
			{
				get
				{
					// The image in the BMP file has always to be upside-down as compared to the PSP image
					return true;
				}
			}
		}

		private class CaptureImageImageIO : AbstractCaptureImage
		{
			internal BufferedImage im;
			internal int[] lineARGB;
			internal int x;
			internal int y;
			internal int width;
			internal string fileName;
			internal string fileFormat;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeHeader(String fileName, String fileFormat, int width, int height, int readWidth) throws java.io.IOException
			public override void writeHeader(string fileName, string fileFormat, int width, int height, int readWidth)
			{
				this.fileName = fileName;
				this.fileFormat = fileFormat;
				this.width = width;

				// Remark: use TYPE_3BYTE_BGR instead of TYPE_4BYTE_ABGR, it looks like ImageIO
				// is not correctly handling images with alpha values. Incorrect png and jpg images
				// are created when using an alpha component.
				im = new BufferedImage(width, height, BufferedImage.TYPE_3BYTE_BGR);
				lineARGB = new int[width];
			}

			public override void startLine(int y)
			{
				this.y = y;
				x = 0;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writePixel(int pixel) throws java.io.IOException
			public override void writePixel(int pixel)
			{
				lineARGB[x] = colorABGRtoARGB(pixel);
				x++;
			}

			public override void endLine()
			{
				im.setRGB(0, y, width, 1, lineARGB, 0, width);
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeEnd() throws java.io.IOException
			public override void writeEnd()
			{
				if (!ImageIO.write(im, fileFormat, new File(fileName)))
				{
					log.error(string.Format("Cannot save image in format {0} using ImageIO: {1}", fileFormat, fileName));
				}
			}
		}

		private class CaptureImagePNG : AbstractCaptureImage
		{
			internal int width;
			internal int height;
			internal string fileName;
			internal sbyte[] buffer;
			internal int pixelIndex;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeHeader(String fileName, String fileFormat, int width, int height, int readWidth) throws java.io.IOException
			public override void writeHeader(string fileName, string fileFormat, int width, int height, int readWidth)
			{
				this.fileName = fileName;
				this.width = width;
				this.height = height;

				// 4 bytes per pixel plus one byte per line for the filter type
				buffer = new sbyte[width * height * 4 + height];
			}

			public override void startLine(int y)
			{
				pixelIndex = width * y * 4 + y;

				// Write the filter type byte at the beginning of each line
				buffer[pixelIndex] = 0; // filter type: None
				pixelIndex++;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writePixel(int pixel) throws java.io.IOException
			public override void writePixel(int pixel)
			{
				buffer[pixelIndex + 0] = (sbyte)(pixel); // R
				buffer[pixelIndex + 1] = (sbyte)(pixel >> 8); // G
				buffer[pixelIndex + 2] = (sbyte)(pixel >> 16); // B
				buffer[pixelIndex + 3] = (sbyte)(pixel >> 24); // A

				pixelIndex += 4;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void writeEnd() throws java.io.IOException
			public override void writeEnd()
			{
				// See https://en.wikipedia.org/wiki/Portable_Network_Graphics
				// for detailed information about the PNG file format

				sbyte[] fileHeader = new sbyte[8];
				fileHeader[0] = unchecked((sbyte) 0x89);
				fileHeader[1] = (sbyte)'P';
				fileHeader[2] = (sbyte)'N';
				fileHeader[3] = (sbyte)'G';
				fileHeader[4] = (sbyte)'\r';
				fileHeader[5] = (sbyte)'\n';
				fileHeader[6] = (sbyte) 0x1A;
				fileHeader[7] = (sbyte)'\n';

				sbyte[] ihdr = new sbyte[13 + 8 + 4];
				storeBigEndianInt(ihdr, 0, 13);
				storeChunkType(ihdr, 4, 'I', 'H', 'D', 'R');
				storeBigEndianInt(ihdr, 8, width);
				storeBigEndianInt(ihdr, 12, height);
				ihdr[16] = 8; // bit depth
				ihdr[17] = 6; // color type: red, green, blue, alpha
				ihdr[18] = 0; // compression method: deflate/inflate
				ihdr[19] = 0; // filter method: none
				ihdr[20] = 0; // interlace method: no interlace
				storeCRC(ihdr, 21);

				Deflater deflater = new Deflater();
				deflater.Input = buffer;
				deflater.finish();
				sbyte[] data = new sbyte[buffer.Length];
				int dataLength = deflater.deflate(data);
				sbyte[] idat = new sbyte[8 + dataLength + 4];
				storeBigEndianInt(idat, 0, dataLength);
				storeChunkType(idat, 4, 'I', 'D', 'A', 'T');
				Array.Copy(data, 0, idat, 8, dataLength);
				storeCRC(idat, 8 + dataLength);

				sbyte[] iend = new sbyte[12];
				storeBigEndianInt(iend, 0, 0);
				storeChunkType(iend, 4, 'I', 'E', 'N', 'D');
				storeCRC(iend, 8);

				System.IO.Stream outStream = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				outStream.Write(fileHeader, 0, fileHeader.Length);
				outStream.Write(ihdr, 0, ihdr.Length);
				outStream.Write(idat, 0, idat.Length);
				outStream.Write(iend, 0, iend.Length);
				outStream.Close();
			}
		}

		public CaptureImage(int imageaddr, int level, Buffer buffer, int width, int height, int bufferWidth, int bufferStorage, bool compressedImage, int compressedImageSize, bool invert, bool overwriteFile, string fileNamePrefix)
		{
			this.imageaddr = imageaddr;
			this.level = level;
			this.buffer = buffer;
			this.width = width;
			this.height = height;
			this.bufferWidth = bufferWidth;
			this.bufferStorage = bufferStorage;
			this.compressedImage = compressedImage;
			this.compressedImageSize = compressedImageSize;
			this.invert = invert;
			this.overwriteFile = overwriteFile;
			this.fileNamePrefix = string.ReferenceEquals(fileNamePrefix, null) ? "Image" : fileNamePrefix;
		}

		public CaptureImage(int imageaddr, int level, IMemoryReader imageReader, int width, int height, int bufferWidth, bool invert, bool overwriteFile, string fileNamePrefix)
		{
			this.imageaddr = imageaddr;
			this.level = level;
			this.imageReader = imageReader;
			this.width = width;
			this.height = height;
			this.bufferWidth = bufferWidth;
			this.bufferStorage = TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
			this.compressedImage = false;
			this.compressedImageSize = 0;
			this.invert = invert;
			this.overwriteFile = overwriteFile;
			this.fileNamePrefix = string.ReferenceEquals(fileNamePrefix, null) ? "Image" : fileNamePrefix;
		}

		public virtual string FileFormat
		{
			set
			{
				this.fileFormat = value;
			}
		}

		public virtual string Directory
		{
			set
			{
				this.directory = value;
			}
		}

		public virtual string FileName
		{
			get
			{
				if (string.ReferenceEquals(fileName, null))
				{
					string levelName = "";
					if (level > 0)
					{
						levelName = "_" + level;
					}
    
					int scanIndex = 0;
					int? lastIndex = lastFileIndex[imageaddr];
					if (lastIndex != null)
					{
						scanIndex = lastIndex.Value + 1;
					}
					for (int i = scanIndex; ; i++)
					{
						string id = (i == 0 ? "" : "-" + i);
						fileName = string.Format("{0}{1}{2:X8}{3}{4}{5}.{6}", directory, fileNamePrefix, imageaddr, fileNameSuffix, levelName, id, fileFormat);
						if (overwriteFile)
						{
							break;
						}
    
						File file = new File(fileName);
						if (!file.exists())
						{
							lastFileIndex[imageaddr] = i;
							break;
						}
					}
				}
    
				return fileName;
			}
			set
			{
				this.fileName = value;
			}
		}


		public virtual bool fileExists()
		{
			return System.IO.Directory.Exists(FileName) || System.IO.File.Exists(FileName);
		}

		public virtual string FileNameSuffix
		{
			set
			{
				this.fileNameSuffix = value;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write() throws java.io.IOException
		public virtual void write()
		{
			if (bufferStorage >= TPSM_PIXEL_STORAGE_MODE_4BIT_INDEXED && bufferStorage <= TPSM_PIXEL_STORAGE_MODE_32BIT_INDEXED && bufferStorage != depthBufferPixelFormat)
			{
				// Writing of indexed images not supported
				return;
			}

			if (compressedImage)
			{
				decompressImage();
			}

			bool imageInvert = invert;

			int readWidth = System.Math.Min(width, bufferWidth);
			sbyte[] pixelBytes = new sbyte[4];
			sbyte[] blackPixelBytes = new sbyte[pixelBytes.Length];

			// ImageIO doesn't support the bmp file format and
			// doesn't properly write PNG files with pixel alpha values
			AbstractCaptureImage captureImage;
			if (bmpFileFormat.Equals(fileFormat))
			{
				captureImage = new CaptureImageBMP();
			}
			else if (pngFileFormat.Equals(fileFormat))
			{
				captureImage = new CaptureImagePNG();
			}
			else
			{
				captureImage = new CaptureImageImageIO();
			}

			captureImage.writeHeader(FileName, fileFormat, width, height, readWidth);
			if (captureImage.Inverted)
			{
				imageInvert = !imageInvert;
			}

			bool imageType32Bit = bufferStorage == TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888 || bufferStorage == RE_DEPTH_STENCIL;
			if (imageReader != null)
			{
				for (int y = 0; y < height; y++)
				{
					captureImage.startLine(imageInvert ? (height - y - 1) : y);
					for (int x = 0; x < readWidth; x++)
					{
						int pixel = imageReader.readNext();
						captureImage.writePixel(pixel);
					}
					captureImage.endLine();
				}
				captureImage.writeEnd();
			}
			else if (buffer is IntBuffer && imageType32Bit)
			{
				IntBuffer intBuffer = (IntBuffer) buffer;
				for (int y = 0; y < height; y++)
				{
					intBuffer.position((imageInvert ? (height - y - 1) : y) * bufferWidth);
					captureImage.startLine(imageInvert ? (height - y - 1) : y);
					for (int x = 0; x < readWidth; x++)
					{
						try
						{
							int pixel = intBuffer.get();
							captureImage.writePixel(pixel);
						}
						catch (BufferUnderflowException)
						{
							captureImage.writePixel(blackPixelBytes);
						}
					}
					captureImage.endLine();
				}
			}
			else if (buffer is IntBuffer && !imageType32Bit)
			{
				IntBuffer intBuffer = (IntBuffer) buffer;
				for (int y = 0; y < height; y++)
				{
					intBuffer.position((imageInvert ? (height - y - 1) : y) * bufferWidth / 2);
					captureImage.startLine(imageInvert ? (height - y - 1) : y);
					for (int x = 0; x < readWidth; x += 2)
					{
						try
						{
							int twoPixels = intBuffer.get();
							getPixelBytes((short) twoPixels, bufferStorage, pixelBytes);
							captureImage.writePixel(pixelBytes);
							if (x + 1 < readWidth)
							{
								getPixelBytes((short)((int)((uint)twoPixels >> 16)), bufferStorage, pixelBytes);
								captureImage.writePixel(pixelBytes);
							}
						}
						catch (BufferUnderflowException)
						{
							captureImage.writePixel(blackPixelBytes);
							captureImage.writePixel(blackPixelBytes);
						}
					}
					captureImage.endLine();
				}
			}
			else if (buffer is ShortBuffer && !imageType32Bit)
			{
				ShortBuffer shortBuffer = (ShortBuffer) buffer;
				for (int y = 0; y < height; y++)
				{
					shortBuffer.position((imageInvert ? (height - y - 1) : y) * bufferWidth);
					captureImage.startLine(imageInvert ? (height - y - 1) : y);
					for (int x = 0; x < readWidth; x++)
					{
						short pixel = shortBuffer.get();
						getPixelBytes(pixel, bufferStorage, pixelBytes);
						captureImage.writePixel(pixelBytes);
					}
					captureImage.endLine();
				}
			}
			else if (imageType32Bit)
			{
				for (int y = 0; y < height; y++)
				{
					IMemoryReader memoryReader = MemoryReader.getMemoryReader(imageaddr + (imageInvert ? (height - y - 1) : y) * bufferWidth * 4, bufferWidth * 4, 4);
					captureImage.startLine(imageInvert ? (height - y - 1) : y);
					for (int x = 0; x < readWidth; x++)
					{
						int pixel = memoryReader.readNext();
						captureImage.writePixel(pixel);
					}
					captureImage.endLine();
				}
			}
			else
			{
				for (int y = 0; y < height; y++)
				{
					IMemoryReader memoryReader = MemoryReader.getMemoryReader(imageaddr + (imageInvert ? (height - y - 1) : y) * bufferWidth * 2, bufferWidth * 2, 2);
					captureImage.startLine(imageInvert ? (height - y - 1) : y);
					for (int x = 0; x < readWidth; x++)
					{
						short pixel = (short) memoryReader.readNext();
						getPixelBytes(pixel, bufferStorage, pixelBytes);
						captureImage.writePixel(pixelBytes);
					}
					captureImage.endLine();
				}
			}

			if (buffer != null)
			{
				buffer.rewind();
			}
			captureImage.writeEnd();

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Saved image to {0}", FileName));
			}
		}

		private static void storeLittleEndianInt(sbyte[] buffer, int offset, int value)
		{
			buffer[offset] = (sbyte)(value);
			buffer[offset + 1] = (sbyte)(value >> 8);
			buffer[offset + 2] = (sbyte)(value >> 16);
			buffer[offset + 3] = (sbyte)(value >> 24);
		}

		private static void storeBigEndianInt(sbyte[] buffer, int offset, int value)
		{
			buffer[offset] = (sbyte)(value >> 24);
			buffer[offset + 1] = (sbyte)(value >> 16);
			buffer[offset + 2] = (sbyte)(value >> 8);
			buffer[offset + 3] = (sbyte)(value);
		}

		private static void storeChunkType(sbyte[] buffer, int offset, char c1, char c2, char c3, char c4)
		{
			buffer[offset] = (sbyte) c1;
			buffer[offset + 1] = (sbyte) c2;
			buffer[offset + 2] = (sbyte) c3;
			buffer[offset + 3] = (sbyte) c4;
		}

		private static void storeCRC(sbyte[] buffer, int offset)
		{
			CRC32 crc32 = new CRC32();
			crc32.update(buffer, 4, offset - 4);
			storeBigEndianInt(buffer, offset, (int) crc32.Value);
		}

		private static void storeLittleEndianShort(sbyte[] buffer, int offset, int value)
		{
			buffer[offset] = (sbyte)(value);
			buffer[offset + 1] = (sbyte)(value >> 8);
		}

		private void getPixelBytes(short pixel, int imageType, sbyte[] pixelBytes)
		{
			switch (imageType)
			{
				case TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650:
					pixelBytes[0] = unchecked((sbyte)((pixel >> 8) & 0xF8)); // B
					pixelBytes[1] = unchecked((sbyte)((pixel >> 3) & 0xFC)); // G
					pixelBytes[2] = unchecked((sbyte)((pixel << 3) & 0xF8)); // R
					pixelBytes[3] = 0; // A
					break;
				case TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR5551:
					pixelBytes[0] = unchecked((sbyte)((pixel >> 7) & 0xF8)); // B
					pixelBytes[1] = unchecked((sbyte)((pixel >> 2) & 0xF8)); // G
					pixelBytes[2] = unchecked((sbyte)((pixel << 3) & 0xF8)); // R
					pixelBytes[3] = (sbyte)((pixel >> 15) != 0 ? 0xFF : 0x00); // A
					break;
				case TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR4444:
					pixelBytes[0] = unchecked((sbyte)((pixel >> 4) & 0xF0)); // B
					pixelBytes[1] = unchecked((sbyte)((pixel) & 0xF0)); // G
					pixelBytes[2] = unchecked((sbyte)((pixel << 4) & 0xF0)); // R
					pixelBytes[3] = unchecked((sbyte)((pixel >> 8) & 0xF0)); // A
					break;
				case depthBufferPixelFormat:
					// Gray color value based on depth value
					pixelBytes[0] = (sbyte)(pixel >> 8);
					pixelBytes[1] = pixelBytes[0];
					pixelBytes[2] = pixelBytes[0];
					pixelBytes[3] = pixelBytes[0];
					break;
				default:
					// Black pixel
					pixelBytes[0] = 0;
					pixelBytes[1] = 0;
					pixelBytes[2] = 0;
					pixelBytes[3] = 0;
					break;
			}
		}

		private static int getARGB(sbyte[] pixelBytes)
		{
			return ((pixelBytes[3] & 0xFF) << 24) | ((pixelBytes[2] & 0xFF) << 16) | ((pixelBytes[1] & 0xFF) << 8) | ((pixelBytes[0] & 0xFF));
		}

		private void storePixel(IntBuffer buffer, int x, int y, int color)
		{
			buffer.put(y * width + x, color);
		}

		private int round4(int n)
		{
			return alignUp(n, 3);
		}

		private int getInt32(Buffer buffer)
		{
			if (buffer is IntBuffer)
			{
				return ((IntBuffer) buffer).get();
			}
			else if (buffer is ShortBuffer)
			{
				ShortBuffer shortBuffer = (ShortBuffer) buffer;
				int n0 = shortBuffer.get() & 0xFFFF;
				int n1 = shortBuffer.get() & 0xFFFF;
				return (n1 << 16) | n0;
			}
			else if (buffer is ByteBuffer)
			{
				return ((ByteBuffer) buffer).Int;
			}

			return 0;
		}

		private void decompressImageDXT(int dxtLevel)
		{
			IntBuffer decompressedBuffer = IntBuffer.allocate(round4(width) * round4(height));

			//
			// For more information of the S3 Texture compression (DXT), see
			// http://en.wikipedia.org/wiki/S3_Texture_Compression
			//
			int strideX = 0;
			int strideY = 0;
			int[] colors = new int[4];
			int strideSize = (dxtLevel == 1 ? 8 : 16);
			int[] alphas = new int[16];
			int[] alphasLookup = new int[8];
			for (int i = 0; i < compressedImageSize; i += strideSize)
			{
				if (dxtLevel > 1)
				{
					if (dxtLevel <= 3)
					{
						// 64 bits of alpha channel data: four bits for each pixel
						int alphaBits = 0;
						for (int j = 0; j < 16; j++, alphaBits = (int)((uint)alphaBits >> 4))
						{
							if ((j % 8) == 0)
							{
								alphaBits = getInt32(buffer);
							}
							int alpha = alphaBits & 0x0F;
							alphas[j] = alpha << 4;
						}
					}
					else
					{
						// 64 bits of alpha channel data: two 8 bit alpha values and a 4x4 3 bit lookup table
						int bits0 = getInt32(buffer);
						int bits1 = getInt32(buffer);
						int alpha0 = bits0 & 0xFF;
						int alpha1 = (bits0 >> 8) & 0xFF;
						alphasLookup[0] = alpha0;
						alphasLookup[1] = alpha1;
						if (alpha0 > alpha1)
						{
							alphasLookup[2] = (6 * alpha0 + 1 * alpha1) / 7;
							alphasLookup[3] = (5 * alpha0 + 2 * alpha1) / 7;
							alphasLookup[4] = (4 * alpha0 + 3 * alpha1) / 7;
							alphasLookup[5] = (3 * alpha0 + 4 * alpha1) / 7;
							alphasLookup[6] = (2 * alpha0 + 5 * alpha1) / 7;
							alphasLookup[7] = (1 * alpha0 + 6 * alpha1) / 7;
						}
						else
						{
							alphasLookup[2] = (4 * alpha0 + 1 * alpha1) / 5;
							alphasLookup[3] = (3 * alpha0 + 2 * alpha1) / 5;
							alphasLookup[4] = (2 * alpha0 + 3 * alpha1) / 5;
							alphasLookup[5] = (1 * alpha0 + 4 * alpha1) / 5;
							alphasLookup[6] = 0x00;
							alphasLookup[7] = 0xFF;
						}
						int bits = bits0 >> 16;
						for (int j = 0; j < 16; j++)
						{
							int lookup;
							if (j == 5)
							{
								lookup = (bits & 1) << 2 | (bits1 & 3);
								bits = (int)((uint)bits1 >> 2);
							}
							else
							{
								lookup = bits & 7;
								bits = (int)((uint)bits >> 3);
							}
							alphas[j] = alphasLookup[lookup];
						}
					}
				}
				int color = getInt32(buffer);
				int color0 = (color >> 0) & 0xFFFF;
				int color1 = (color >> 16) & 0xFFFF;

				int r0 = (color0 >> 8) & 0xF8;
				int g0 = (color0 >> 3) & 0xFC;
				int b0 = (color0 << 3) & 0xF8;

				int r1 = (color1 >> 8) & 0xF8;
				int g1 = (color1 >> 3) & 0xFC;
				int b1 = (color1 << 3) & 0xF8;

				int r2, g2, b2;
				if (color0 > color1 || dxtLevel > 1)
				{
					r2 = (r0 * 2 + r1) / 3;
					g2 = (g0 * 2 + g1) / 3;
					b2 = (b0 * 2 + b1) / 3;
				}
				else
				{
					r2 = (r0 + r1) / 2;
					g2 = (g0 + g1) / 2;
					b2 = (b0 + b1) / 2;
				}

				int r3, g3, b3;
				if (color0 > color1 || dxtLevel > 1)
				{
					r3 = (r0 + r1 * 2) / 3;
					g3 = (g0 + g1 * 2) / 3;
					b3 = (b0 + b1 * 2) / 3;
				}
				else
				{
					r3 = 0x00;
					g3 = 0x00;
					b3 = 0x00;
				}

				colors[0] = ((b0 & 0xFF) << 16) | ((g0 & 0xFF) << 8) | (r0 & 0xFF);
				colors[1] = ((b1 & 0xFF) << 16) | ((g1 & 0xFF) << 8) | (r1 & 0xFF);
				colors[2] = ((b2 & 0xFF) << 16) | ((g2 & 0xFF) << 8) | (r2 & 0xFF);
				colors[3] = ((b3 & 0xFF) << 16) | ((g3 & 0xFF) << 8) | (r3 & 0xFF);

				int bits = getInt32(buffer);
				for (int y = 0, alphaIndex = 0; y < 4; y++)
				{
					for (int x = 0; x < 4; x++, bits = (int)((uint)bits >> 2), alphaIndex++)
					{
						int bgr = colors[bits & 3];
						int alpha = alphas[alphaIndex] << 24;
						storePixel(decompressedBuffer, strideX + x, strideY + y, bgr | alpha);
					}
				}

				strideX += 4;
				if (strideX >= width)
				{
					strideX = 0;
					strideY += 4;
				}
			}

			buffer.rewind();
			compressedImage = false;
			buffer = decompressedBuffer;
			bufferWidth = width;
			bufferStorage = TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
		}

		private void decompressImage()
		{
			switch (bufferStorage)
			{
				case TPSM_PIXEL_STORAGE_MODE_DXT1:
					decompressImageDXT(1);
					break;
				case TPSM_PIXEL_STORAGE_MODE_DXT3:
					decompressImageDXT(3);
					break;
				case TPSM_PIXEL_STORAGE_MODE_DXT5:
					decompressImageDXT(5);
					break;
				default:
					log.warn(string.Format("Unsupported compressed buffer storage {0:D}", bufferStorage));
					break;
			}
		}
	}

}