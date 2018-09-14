using System;
using System.Threading;

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
//	import static pspsharp.HLE.modules.sceJpeg.clamp8bit;



	using Logger = org.apache.log4j.Logger;

	using IMediaReader = com.xuggle.mediatool.IMediaReader;
	using MediaListenerAdapter = com.xuggle.mediatool.MediaListenerAdapter;
	using ToolFactory = com.xuggle.mediatool.ToolFactory;
	using IVideoPictureEvent = com.xuggle.mediatool.@event.IVideoPictureEvent;
	using ICodec = com.xuggle.xuggler.ICodec;
	using IContainer = com.xuggle.xuggler.IContainer;
	using IContainerFormat = com.xuggle.xuggler.IContainerFormat;
	using Type = com.xuggle.xuggler.IPixelFormat.Type;
	using IStream = com.xuggle.xuggler.IStream;
	using IStreamCoder = com.xuggle.xuggler.IStreamCoder;
	using IVideoPicture = com.xuggle.xuggler.IVideoPicture;
	using IVideoResampler = com.xuggle.xuggler.IVideoResampler;
	using ConverterFactory = com.xuggle.xuggler.video.ConverterFactory;
	using IConverter = com.xuggle.xuggler.video.IConverter;

	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using pspUsbCamSetupMicExParam = pspsharp.HLE.kernel.types.pspUsbCamSetupMicExParam;
	using pspUsbCamSetupMicParam = pspsharp.HLE.kernel.types.pspUsbCamSetupMicParam;
	using pspUsbCamSetupStillExParam = pspsharp.HLE.kernel.types.pspUsbCamSetupStillExParam;
	using pspUsbCamSetupStillParam = pspsharp.HLE.kernel.types.pspUsbCamSetupStillParam;
	using pspUsbCamSetupVideoExParam = pspsharp.HLE.kernel.types.pspUsbCamSetupVideoExParam;
	using pspUsbCamSetupVideoParam = pspsharp.HLE.kernel.types.pspUsbCamSetupVideoParam;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryWriter = pspsharp.memory.MemoryWriter;

	public class sceUsbCam : HLEModule
	{
		public static Logger log = Modules.getLogger("sceUsbCam");
		private const bool dumpJpeg = false;

		public const int PSP_USBCAM_PID = 0x282;
		public const string PSP_USBCAM_DRIVERNAME = "USBCamDriver";
		public const string PSP_USBCAMMIC_DRIVERNAME = "USBCamMicDriver";

		/// <summary>
		/// Resolutions for sceUsbCamSetupStill & sceUsbCamSetupVideo
		///    ** DO NOT use on sceUsbCamSetupStillEx & sceUsbCamSetupVideoEx 
		/// </summary>
		public const int PSP_USBCAM_RESOLUTION_160_120 = 0;
		public const int PSP_USBCAM_RESOLUTION_176_144 = 1;
		public const int PSP_USBCAM_RESOLUTION_320_240 = 2;
		public const int PSP_USBCAM_RESOLUTION_352_288 = 3;
		public const int PSP_USBCAM_RESOLUTION_640_480 = 4;
		public const int PSP_USBCAM_RESOLUTION_1024_768 = 5;
		public const int PSP_USBCAM_RESOLUTION_1280_960 = 6;
		public const int PSP_USBCAM_RESOLUTION_480_272 = 7;
		public const int PSP_USBCAM_RESOLUTION_360_272 = 8;
		protected internal static readonly int[] resolutionWidth = new int[] {160, 176, 320, 352, 640, 1024, 1280, 480, 360};
		protected internal static readonly int[] resolutionHeight = new int[] {120, 144, 240, 288, 480, 768, 960, 272, 272};

		/// <summary>
		/// Resolutions for sceUsbCamSetupStillEx & sceUsbCamSetupVideoEx
		///    ** DO NOT use on sceUsbCamSetupStill & sceUsbCamSetupVideo 
		/// </summary>
		public const int PSP_USBCAM_RESOLUTION_EX_160_120 = 0;
		public const int PSP_USBCAM_RESOLUTION_EX_176_144 = 1;
		public const int PSP_USBCAM_RESOLUTION_EX_320_240 = 2;
		public const int PSP_USBCAM_RESOLUTION_EX_352_288 = 3;
		public const int PSP_USBCAM_RESOLUTION_EX_360_272 = 4;
		public const int PSP_USBCAM_RESOLUTION_EX_480_272 = 5;
		public const int PSP_USBCAM_RESOLUTION_EX_640_480 = 6;
		public const int PSP_USBCAM_RESOLUTION_EX_1024_768 = 7;
		public const int PSP_USBCAM_RESOLUTION_EX_1280_960 = 8;

		/// <summary>
		/// Flags for reverse effects. </summary>
		public const int PSP_USBCAM_FLIP = 1;
		public const int PSP_USBCAM_MIRROR = 0x100;

		/// <summary>
		/// Delay to take pictures </summary>
		public const int PSP_USBCAM_NODELAY = 0;
		public const int PSP_USBCAM_DELAY_10SEC = 1;
		public const int PSP_USBCAM_DELAY_20SEC = 2;
		public const int PSP_USBCAM_DELAY_30SEC = 3;

		/// <summary>
		/// Usbcam framerates </summary>
		public const int PSP_USBCAM_FRAMERATE_3_75_FPS = 0; // 3.75 fps
		public const int PSP_USBCAM_FRAMERATE_5_FPS = 1;
		public const int PSP_USBCAM_FRAMERATE_7_5_FPS = 2; // 7.5 fps
		public const int PSP_USBCAM_FRAMERATE_10_FPS = 3;
		public const int PSP_USBCAM_FRAMERATE_15_FPS = 4;
		public const int PSP_USBCAM_FRAMERATE_20_FPS = 5;
		public const int PSP_USBCAM_FRAMERATE_30_FPS = 6;
		public const int PSP_USBCAM_FRAMERATE_60_FPS = 7;

		/// <summary>
		/// White balance values </summary>
		public const int PSP_USBCAM_WB_AUTO = 0;
		public const int PSP_USBCAM_WB_DAYLIGHT = 1;
		public const int PSP_USBCAM_WB_FLUORESCENT = 2;
		public const int PSP_USBCAM_WB_INCADESCENT = 3;

		/// <summary>
		/// Effect modes </summary>
		public const int PSP_USBCAM_EFFECTMODE_NORMAL = 0;
		public const int PSP_USBCAM_EFFECTMODE_NEGATIVE = 1;
		public const int PSP_USBCAM_EFFECTMODE_BLACKWHITE = 2;
		public const int PSP_USBCAM_EFFECTMODE_SEPIA = 3;
		public const int PSP_USBCAM_EFFECTMODE_BLUE = 4;
		public const int PSP_USBCAM_EFFECTMODE_RED = 5;
		public const int PSP_USBCAM_EFFECTMODE_GREEN = 6;

		/// <summary>
		/// Exposure levels </summary>
		public const int PSP_USBCAM_EVLEVEL_2_0_POSITIVE = 0; // +2.0
		public const int PSP_USBCAM_EVLEVEL_1_7_POSITIVE = 1; // +1.7
		public const int PSP_USBCAM_EVLEVEL_1_5_POSITIVE = 2; // +1.5
		public const int PSP_USBCAM_EVLEVEL_1_3_POSITIVE = 3; // +1.3
		public const int PSP_USBCAM_EVLEVEL_1_0_POSITIVE = 4; // +1.0
		public const int PSP_USBCAM_EVLEVEL_0_7_POSITIVE = 5; // +0.7
		public const int PSP_USBCAM_EVLEVEL_0_5_POSITIVE = 6; // +0.5
		public const int PSP_USBCAM_EVLEVEL_0_3_POSITIVE = 7; // +0.3
		public const int PSP_USBCAM_EVLEVEL_0_0 = 8; // 0.0
		public const int PSP_USBCAM_EVLEVEL_0_3_NEGATIVE = 9; // -0.3
		public const int PSP_USBCAM_EVLEVEL_0_5_NEGATIVE = 10; // -0.5
		public const int PSP_USBCAM_EVLEVEL_0_7_NEGATIVE = 11; // -0.7
		public const int PSP_USBCAM_EVLEVEL_1_0_NEGATIVE = 12; // -1.0
		public const int PSP_USBCAM_EVLEVEL_1_3_NEGATIVE = 13; // -1.3
		public const int PSP_USBCAM_EVLEVEL_1_5_NEGATIVE = 14; // -1.5
		public const int PSP_USBCAM_EVLEVEL_1_7_NEGATIVE = 15; // -1.7
		public const int PSP_USBCAM_EVLEVEL_2_0_NEGATIVE = 16; // -2.0

		protected internal int workArea;
		protected internal int workAreaSize;
		protected internal TPointer jpegBuffer;
		protected internal int jpegBufferSize;
		protected internal BufferedImage currentVideoImage;
		protected internal sbyte[] currentVideoImageBytes;
		protected internal int currentVideoFrameCount;
		protected internal int lastVideoFrameCount;
		protected internal VideoListener videoListener;

		// Camera settings
		protected internal int resolution; // One of PSP_USBCAM_RESOLUTION_* (not PSP_USBCAM_RESOLUTION_EX_*)
		protected internal int frameRate;
		protected internal int whiteBalance;
		protected internal int frameSize;
		protected internal int saturation;
		protected internal int brightness;
		protected internal int contrast;
		protected internal int sharpness;
		protected internal int imageEffectMode;
		protected internal int evLevel;
		protected internal bool flip;
		protected internal bool mirror;
		protected internal int zoom;
		protected internal bool autoImageReverseSW;
		protected internal bool lensDirectionAtYou;
		protected internal int micFrequency;
		protected internal int micGain;

		protected internal TPointer readMicBuffer;
		protected internal int readMicBufferSize;

		protected internal class VideoListener : MediaListenerAdapter
		{
			private readonly sceUsbCam outerInstance;

			internal IMediaReader reader;
			internal int videoStream;
			internal IStreamCoder videoCoder;
			internal IConverter videoConverter;
			internal IVideoResampler videoResampler;
			internal IVideoPicture videoPicture;
			internal VideoReaderThread videoReaderThread;

			public VideoListener(sceUsbCam outerInstance, IMediaReader reader, int width, int height)
			{
				this.outerInstance = outerInstance;
				this.reader = reader;
				IContainer container = reader.Container;
				int numStreams = container.NumStreams;
				for (int i = 0; i < numStreams; i++)
				{
					IStream stream = container.getStream(i);
					IStreamCoder coder = stream.StreamCoder;

					if (coder.CodecType == ICodec.Type.CODEC_TYPE_VIDEO)
					{
						videoStream = i;
						videoCoder = coder;
					}
				}

				if (videoCoder != null)
				{
					videoConverter = ConverterFactory.createConverter(ConverterFactory.XUGGLER_BGR_24, videoCoder.PixelType, videoCoder.Width, videoCoder.Height);
					videoPicture = IVideoPicture.make(videoCoder.PixelType, width, height);
					videoResampler = IVideoResampler.make(width, height, videoCoder.PixelType, videoCoder.Width, videoCoder.Height, videoCoder.PixelType);
				}

				videoReaderThread = new VideoReaderThread(outerInstance, reader);
				videoReaderThread.Name = "Video Reader Thread";
				videoReaderThread.Daemon = true;
				videoReaderThread.Start();
			}

			public virtual void stop()
			{
				videoReaderThread.end();
			}

			public override void onVideoPicture(IVideoPictureEvent @event)
			{
				BufferedImage image = @event.Image;
				if (image == null && videoConverter != null)
				{
					IVideoPicture eventPicture = @event.Picture;
					videoResampler.resample(videoPicture, eventPicture);
					try
					{
						image = videoConverter.toImage(videoPicture);
					}
					catch (Exception e)
					{
						if (videoPicture.PixelType == Type.YUYV422)
						{
							image = outerInstance.convertYUYV422toRGB(videoPicture.Width, videoPicture.Height, videoPicture.ByteBuffer);
						}
						else
						{
							log.error(string.Format("VideoListener.onVideoPicture: {0}", videoPicture), e);
						}
					}
				}

				if (log.DebugEnabled)
				{
					log.debug(string.Format("onVideoPicture event={0}, image={1}", @event, image));
				}
				if (image != null)
				{
					outerInstance.CurrentVideoImage = image;
				}
			}
		}

		protected internal class VideoReaderThread : Thread
		{
			private readonly sceUsbCam outerInstance;

			internal IMediaReader reader;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal volatile bool end_Renamed;

			public VideoReaderThread(sceUsbCam outerInstance, IMediaReader reader)
			{
				this.outerInstance = outerInstance;
				this.reader = reader;
			}

			public virtual void end()
			{
				end_Renamed = true;
			}

			public override void run()
			{
				while (!end_Renamed)
				{
					reader.readPacket();
				}
				reader.close();
			}
		}

		// Faked video reading
		protected internal long lastVideoFrameMillis;
		protected internal static readonly int[] framerateFrameDurationMillis = new int[] {267, 200, 133, 100, 67, 50, 33, 17};

		protected internal static int convertYUVtoRGB(int y, int u, int v)
		{
			// based on http://en.wikipedia.org/wiki/Yuv#Y.27UV444_to_RGB888_conversion
			int c = y - 16;
			int d = u - 128;
			int e = v - 128;
			int r = clamp8bit((298 * c + 409 * e + 128) >> 8);
			int g = clamp8bit((298 * c - 100 * d - 208 * e + 128) >> 8);
			int b = clamp8bit((298 * c + 516 * d + 128) >> 8);

			return (r << 16) | (g << 8) | b;
		}

		protected internal virtual BufferedImage convertYUYV422toRGB(int width, int height, ByteBuffer buffer)
		{
			sbyte[] input = new sbyte[width * height * 2];
			buffer.get(input);

			int[] output = new int[width * height];
			int i = 0;
			int j = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x += 2)
				{
					int y0 = input[i++] & 0xFF;
					int u = input[i++] & 0xFF;
					int y1 = input[i++] & 0xFF;
					int v = input[i++] & 0xFF;
					output[j++] = convertYUVtoRGB(y0, u, v);
					output[j++] = convertYUVtoRGB(y1, u, v);
				}
			}

			BufferedImage image = new BufferedImage(width, height, BufferedImage.TYPE_INT_RGB);
			image.setRGB(0, 0, width, height, output, 0, width);

			if ((currentVideoFrameCount % 30) == 0)
			{
				int imageIndex = currentVideoFrameCount / 30;
				try
				{
					System.IO.Stream os = new System.IO.FileStream(string.Format("tmp/UsbCam-{0:D}.yuyv422", imageIndex), System.IO.FileMode.Create, System.IO.FileAccess.Write);
					os.Write(input, 0, input.Length);
					os.Close();
				}
				catch (IOException e)
				{
					log.error("dumping yuyv422 image", e);
				}
			}

			return image;
		}

		/// <summary>
		/// Convert a value PSP_USBCAM_RESOLUTION_EX_*
		/// to the corresponding PSP_USBCAM_RESOLUTION_*
		/// </summary>
		/// <param name="resolutionEx"> One of PSP_USBCAM_RESOLUTION_EX_* </param>
		/// <returns>             The corresponding value PSP_USBCAM_RESOLUTION_* </returns>
		protected internal virtual int convertResolutionExToResolution(int resolutionEx)
		{
			switch (resolutionEx)
			{
				case PSP_USBCAM_RESOLUTION_EX_160_120:
					return PSP_USBCAM_RESOLUTION_160_120;
				case PSP_USBCAM_RESOLUTION_EX_176_144:
					return PSP_USBCAM_RESOLUTION_176_144;
				case PSP_USBCAM_RESOLUTION_EX_320_240:
					return PSP_USBCAM_RESOLUTION_320_240;
				case PSP_USBCAM_RESOLUTION_EX_352_288:
					return PSP_USBCAM_RESOLUTION_352_288;
				case PSP_USBCAM_RESOLUTION_EX_360_272:
					return PSP_USBCAM_RESOLUTION_360_272;
				case PSP_USBCAM_RESOLUTION_EX_480_272:
					return PSP_USBCAM_RESOLUTION_480_272;
				case PSP_USBCAM_RESOLUTION_EX_640_480:
					return PSP_USBCAM_RESOLUTION_640_480;
				case PSP_USBCAM_RESOLUTION_EX_1024_768:
					return PSP_USBCAM_RESOLUTION_1024_768;
				case PSP_USBCAM_RESOLUTION_EX_1280_960:
					return PSP_USBCAM_RESOLUTION_1280_960;
			}

			return resolutionEx;
		}

		protected internal virtual int FramerateFrameDurationMillis
		{
			get
			{
				if (frameRate < 0 || frameRate > PSP_USBCAM_FRAMERATE_60_FPS)
				{
					return framerateFrameDurationMillis[PSP_USBCAM_FRAMERATE_60_FPS];
				}
				return framerateFrameDurationMillis[frameRate];
			}
		}

		private void stopVideo()
		{
			if (videoListener != null)
			{
				videoListener.stop();
				videoListener = null;
			}
		}

		public override void stop()
		{
			stopVideo();

			base.stop();
		}

		protected internal static int getResolutionWidth(int resolution)
		{
			if (resolution < 0 || resolution >= resolutionWidth.Length)
			{
				return 0;
			}
			return resolutionWidth[resolution];
		}

		protected internal static int getResolutionHeight(int resolution)
		{
			if (resolution < 0 || resolution >= resolutionHeight.Length)
			{
				return 0;
			}
			return resolutionHeight[resolution];
		}

		protected internal virtual bool setupVideo()
		{
			if (videoListener != null)
			{
				return true;
			}

			IContainer container = IContainer.make();
			IContainerFormat format = IContainerFormat.make();
			int ret = format.setInputFormat("vfwcap");
			if (ret < 0)
			{
				container.close();
				format.delete();
				log.error(string.Format("USB Cam: cannot open WebCam ('vfwcap' device)"));
				return false;
			}
			ret = container.open("0", IContainer.Type.READ, format);
			if (ret < 0)
			{
				container.close();
				format.delete();
				log.error(string.Format("USB Cam: cannot open WebCam ('0')"));
				return false;
			}
			IMediaReader reader = ToolFactory.makeReader(container);
			videoListener = new VideoListener(this, reader, getResolutionWidth(resolution), getResolutionHeight(resolution));
			reader.addListener(videoListener);

			return true;
		}

		protected internal virtual BufferedImage CurrentVideoImage
		{
			set
			{
				sbyte[] newVideoImageBytes = null;
				if (value != null)
				{
					System.IO.MemoryStream outputStream = new System.IO.MemoryStream(jpegBufferSize);
					try
					{
						if (ImageIO.write(value, "jpeg", outputStream))
						{
							outputStream.Close();
							newVideoImageBytes = outputStream.toByteArray();
    
							if (dumpJpeg)
							{
								System.IO.FileStream dumpFile = new System.IO.FileStream("dumpUsbCam.jpeg", System.IO.FileMode.Create, System.IO.FileAccess.Write);
								dumpFile.Write(newVideoImageBytes, 0, newVideoImageBytes.Length);
								dumpFile.Close();
							}
						}
					}
					catch (IOException e)
					{
						log.error("setCurrentVideoImage", e);
					}
    
					currentVideoFrameCount++;
				}
    
				currentVideoImage = value;
				currentVideoImageBytes = newVideoImageBytes;
			}
			get
			{
				return currentVideoImage;
			}
		}


		private int CurrentVideoImageSize
		{
			get
			{
				if (currentVideoImageBytes == null)
				{
					return jpegBufferSize;
				}
    
				return currentVideoImageBytes.Length;
			}
		}

		public virtual int writeCurrentVideoImage(TPointer jpegBuffer, int jpegBufferSize)
		{
			lastVideoFrameCount = currentVideoFrameCount;

			if (currentVideoImageBytes == null)
			{
				if (jpegBuffer == null)
				{
					return 0;
				}

				// Image has to be stored in Jpeg format in buffer
				jpegBuffer.clear(jpegBufferSize);

				return jpegBufferSize;
			}

			int length = System.Math.Min(currentVideoImageBytes.Length, jpegBufferSize);
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(jpegBuffer.Address, length, 1);
			for (int i = 0; i < length; i++)
			{
				memoryWriter.writeNext(currentVideoImageBytes[i] & 0xFF);
			}
			memoryWriter.flush();

			return length;
		}

		private void waitForNextVideoFrame()
		{
			long now = Emulator.Clock.currentTimeMillis();
			int millisSinceLastFrame = (int)(now - lastVideoFrameMillis);
			int frameDurationMillis = FramerateFrameDurationMillis;
			if (millisSinceLastFrame >= 0 && millisSinceLastFrame < frameDurationMillis)
			{
				int delayMillis = frameDurationMillis - millisSinceLastFrame;
				Modules.ThreadManForUserModule.hleKernelDelayThread(delayMillis * 1000, false);
				lastVideoFrameMillis = now + delayMillis;
			}
			else
			{
				lastVideoFrameMillis = now;
			}
		}

		/// <summary>
		/// Set ups the parameters for video capture.
		/// </summary>
		/// <param name="param"> - Pointer to a pspUsbCamSetupVideoParam structure. </param>
		/// <param name="workarea"> - Pointer to a buffer used as work area by the driver. </param>
		/// <param name="wasize"> - Size of the work area.
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x17F7B2FB, version = 271) public int sceUsbCamSetupVideo(pspsharp.HLE.kernel.types.pspUsbCamSetupVideoParam usbCamSetupVideoParam, pspsharp.HLE.TPointer workArea, int workAreaSize)
		[HLEFunction(nid : 0x17F7B2FB, version : 271)]
		public virtual int sceUsbCamSetupVideo(pspUsbCamSetupVideoParam usbCamSetupVideoParam, TPointer workArea, int workAreaSize)
		{
			this.workArea = workArea.Address;
			this.workAreaSize = workAreaSize;
			resolution = usbCamSetupVideoParam.resolution;
			frameRate = usbCamSetupVideoParam.framerate;
			whiteBalance = usbCamSetupVideoParam.wb;
			saturation = usbCamSetupVideoParam.saturation;
			brightness = usbCamSetupVideoParam.brightness;
			contrast = usbCamSetupVideoParam.contrast;
			sharpness = usbCamSetupVideoParam.sharpness;
			imageEffectMode = usbCamSetupVideoParam.effectmode;
			frameSize = usbCamSetupVideoParam.framesize;
			evLevel = usbCamSetupVideoParam.evlevel;

			if (!setupVideo())
			{
				log.warn(string.Format("Cannot find webcam"));
				return SceKernelErrors.ERROR_USBCAM_NOT_READY;
			}

			return 0;
		}

		/// <summary>
		/// Sets if the image should be automatically reversed, depending of the position
		/// of the camera.
		/// </summary>
		/// <param name="on"> - 1 to set the automatical reversal of the image, 0 to set it off
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF93C4669, version = 271) public int sceUsbCamAutoImageReverseSW(boolean on)
		[HLEFunction(nid : 0xF93C4669, version : 271)]
		public virtual int sceUsbCamAutoImageReverseSW(bool on)
		{
			autoImageReverseSW = on;

			return 0;
		}

		/// <summary>
		/// Starts video input from the camera.
		/// </summary>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x574A8C3F, version = 271) public int sceUsbCamStartVideo()
		[HLEFunction(nid : 0x574A8C3F, version : 271)]
		public virtual int sceUsbCamStartVideo()
		{
			if (!setupVideo())
			{
				log.warn(string.Format("Cannot find webcam"));
			}

			return 0;
		}

		/// <summary>
		/// Stops video input from the camera.
		/// </summary>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6CF32CB9, version = 271) public int sceUsbCamStopVideo()
		[HLEFunction(nid : 0x6CF32CB9, version : 271)]
		public virtual int sceUsbCamStopVideo()
		{
			// No parameters
			stopVideo();

			return 0;
		}

		[HLEFunction(nid : 0x03ED7A82, version : 271)]
		public virtual int sceUsbCamSetupMic(pspUsbCamSetupMicParam camSetupMicParam, TPointer workArea, int workAreaSize)
		{
			micFrequency = camSetupMicParam.frequency;
			micGain = camSetupMicParam.gain;

			return 0;
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x82A64030, version : 271)]
		public virtual int sceUsbCamStartMic()
		{
			return 0;
		}

		/// <summary>
		/// Reads a video frame. The function doesn't return until the frame
		/// has been acquired.
		/// </summary>
		/// <param name="buf"> - The buffer that receives the frame jpeg data </param>
		/// <param name="size"> - The size of the buffer.
		/// </param>
		/// <returns> size of acquired frame on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7DAC0C71, version = 271) public int sceUsbCamReadVideoFrameBlocking(pspsharp.HLE.TPointer jpegBuffer, int jpegBufferSize)
		[HLEFunction(nid : 0x7DAC0C71, version : 271)]
		public virtual int sceUsbCamReadVideoFrameBlocking(TPointer jpegBuffer, int jpegBufferSize)
		{
			this.jpegBuffer = jpegBuffer;
			this.jpegBufferSize = jpegBufferSize;

			waitForNextVideoFrame();

			return writeCurrentVideoImage(jpegBuffer, jpegBufferSize);
		}

		/// <summary>
		/// Reads a video frame. The function returns immediately, and
		/// the completion has to be handled by calling sceUsbCamWaitReadVideoFrameEnd
		/// or sceUsbCamPollReadVideoFrameEnd.
		/// </summary>
		/// <param name="buf"> - The buffer that receives the frame jpeg data </param>
		/// <param name="size"> - The size of the buffer.
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x99D86281, version = 271) public int sceUsbCamReadVideoFrame(pspsharp.HLE.TPointer jpegBuffer, int jpegBufferSize)
		[HLEFunction(nid : 0x99D86281, version : 271)]
		public virtual int sceUsbCamReadVideoFrame(TPointer jpegBuffer, int jpegBufferSize)
		{
			this.jpegBuffer = jpegBuffer;
			this.jpegBufferSize = jpegBufferSize;

			return writeCurrentVideoImage(jpegBuffer, jpegBufferSize);
		}

		/// <summary>
		/// Polls the status of video frame read completion.
		/// </summary>
		/// <returns> the size of the acquired frame if it has been read,
		/// 0 if the frame has not yet been read, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x41E73E95, version = 271) public int sceUsbCamPollReadVideoFrameEnd()
		[HLEFunction(nid : 0x41E73E95, version : 271)]
		public virtual int sceUsbCamPollReadVideoFrameEnd()
		{
			if (jpegBuffer == null || jpegBuffer.Null)
			{
				return SceKernelErrors.ERROR_USBCAM_NO_READ_ON_VIDEO_FRAME;
			}

			if (currentVideoFrameCount <= lastVideoFrameCount)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceUsbCamPollReadVideoFrameEnd not frame end ({0:D} - {1:D})", currentVideoFrameCount, lastVideoFrameCount));
				}
				return SceKernelErrors.ERROR_USBCAM_NO_VIDEO_FRAME_AVAILABLE;
			}

			return writeCurrentVideoImage(jpegBuffer, jpegBufferSize);
		}

		/// <summary>
		/// Waits until the current frame has been read.
		/// </summary>
		/// <returns> the size of the acquired frame on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF90B2293, version = 271) public int sceUsbCamWaitReadVideoFrameEnd()
		[HLEFunction(nid : 0xF90B2293, version : 271)]
		public virtual int sceUsbCamWaitReadVideoFrameEnd()
		{
			waitForNextVideoFrame();

			return CurrentVideoImageSize;
		}

		/// <summary>
		/// Gets the direction of the camera lens
		/// </summary>
		/// <returns> 1 if the camera is "looking to you", 0 if the camera
		/// is "looking to the other side". </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4C34F553, version = 271) public boolean sceUsbCamGetLensDirection()
		[HLEFunction(nid : 0x4C34F553, version : 271)]
		public virtual bool sceUsbCamGetLensDirection()
		{
			return lensDirectionAtYou;
		}

		/// <summary>
		/// Setups the parameters to take a still image.
		/// </summary>
		/// <param name="param"> - pointer to a pspUsbCamSetupStillParam
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3F0CF289, version = 271) public int sceUsbCamSetupStill(pspsharp.HLE.kernel.types.pspUsbCamSetupStillParam usbCamSetupStillParam)
		[HLEFunction(nid : 0x3F0CF289, version : 271)]
		public virtual int sceUsbCamSetupStill(pspUsbCamSetupStillParam usbCamSetupStillParam)
		{
			return 0;
		}

		/// <summary>
		/// Setups the parameters to take a still image (with more options)
		/// </summary>
		/// <param name="param"> - pointer to a pspUsbCamSetupStillParamEx
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0A41A298, version = 271) public int sceUsbCamSetupStillEx(pspsharp.HLE.kernel.types.pspUsbCamSetupStillExParam usbCamSetupStillExParam)
		[HLEFunction(nid : 0x0A41A298, version : 271)]
		public virtual int sceUsbCamSetupStillEx(pspUsbCamSetupStillExParam usbCamSetupStillExParam)
		{
			return 0;
		}

		/// <summary>
		/// Gets a still image. The function doesn't return until the image
		/// has been acquired.
		/// </summary>
		/// <param name="buf"> - The buffer that receives the image jpeg data </param>
		/// <param name="size"> - The size of the buffer.
		/// </param>
		/// <returns> size of acquired image on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x61BE5CAC, version = 271) public int sceUsbCamStillInputBlocking(pspsharp.HLE.TPointer buffer, int size)
		[HLEFunction(nid : 0x61BE5CAC, version : 271)]
		public virtual int sceUsbCamStillInputBlocking(TPointer buffer, int size)
		{
			return 0;
		}

		/// <summary>
		/// Gets a still image. The function returns immediately, and
		/// the completion has to be handled by calling ::sceUsbCamStillWaitInputEnd
		/// or ::sceUsbCamStillPollInputEnd.
		/// </summary>
		/// <param name="buf"> - The buffer that receives the image jpeg data </param>
		/// <param name="size"> - The size of the buffer.
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFB0A6C5D, version = 271) public int sceUsbCamStillInput(pspsharp.HLE.TPointer buffer, int size)
		[HLEFunction(nid : 0xFB0A6C5D, version : 271)]
		public virtual int sceUsbCamStillInput(TPointer buffer, int size)
		{
			return 0;
		}

		/// <summary>
		/// Waits until still input has been finished.
		/// </summary>
		/// <returns> the size of the acquired image on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7563AFA1, version = 271) public int sceUsbCamStillWaitInputEnd()
		[HLEFunction(nid : 0x7563AFA1, version : 271)]
		public virtual int sceUsbCamStillWaitInputEnd()
		{
			return 0;
		}

		/// <summary>
		/// Polls the status of still input completion.
		/// </summary>
		/// <returns> the size of the acquired image if still input has ended,
		/// 0 if the input has not ended, < 0 on error. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1A46CFE7, version = 271) public int sceUsbCamStillPollInputEnd()
		[HLEFunction(nid : 0x1A46CFE7, version : 271)]
		public virtual int sceUsbCamStillPollInputEnd()
		{
			return 0;
		}

		/// <summary>
		/// Cancels the still input.
		/// </summary>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA720937C, version = 271) public int sceUsbCamStillCancelInput()
		[HLEFunction(nid : 0xA720937C, version : 271)]
		public virtual int sceUsbCamStillCancelInput()
		{
			return 0;
		}

		/// <summary>
		/// Gets the size of the acquired still image.
		/// </summary>
		/// <returns> the size of the acquired image on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE5959C36, version = 271) public int sceUsbCamStillGetInputLength()
		[HLEFunction(nid : 0xE5959C36, version : 271)]
		public virtual int sceUsbCamStillGetInputLength()
		{
			return 0;
		}

		/// <summary>
		/// Set ups the parameters for video capture (with more options)
		/// </summary>
		/// <param name="param"> - Pointer to a pspUsbCamSetupVideoExParam structure. </param>
		/// <param name="workarea"> - Pointer to a buffer used as work area by the driver. </param>
		/// <param name="wasize"> - Size of the work area.
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCFE9E999, version = 271) public int sceUsbCamSetupVideoEx(pspsharp.HLE.kernel.types.pspUsbCamSetupVideoExParam usbCamSetupVideoExParam, pspsharp.HLE.TPointer workArea, int workAreaSize)
		[HLEFunction(nid : 0xCFE9E999, version : 271)]
		public virtual int sceUsbCamSetupVideoEx(pspUsbCamSetupVideoExParam usbCamSetupVideoExParam, TPointer workArea, int workAreaSize)
		{
			this.workArea = workArea.Address;
			this.workAreaSize = workAreaSize;
			resolution = convertResolutionExToResolution(usbCamSetupVideoExParam.resolution);
			frameRate = usbCamSetupVideoExParam.framerate;
			whiteBalance = usbCamSetupVideoExParam.wb;
			saturation = usbCamSetupVideoExParam.saturation;
			brightness = usbCamSetupVideoExParam.brightness;
			contrast = usbCamSetupVideoExParam.contrast;
			sharpness = usbCamSetupVideoExParam.sharpness;
			imageEffectMode = usbCamSetupVideoExParam.effectmode;
			frameSize = usbCamSetupVideoExParam.framesize;
			evLevel = usbCamSetupVideoExParam.evlevel;

			if (!setupVideo())
			{
				log.warn(string.Format("Cannot find webcam"));
				return SceKernelErrors.ERROR_USBCAM_NOT_READY;
			}

			return 0;
		}

		/// <summary>
		/// Gets the size of the acquired frame.
		/// </summary>
		/// <returns> the size of the acquired frame on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xDF9D0C92, version = 271) public int sceUsbCamGetReadVideoFrameSize()
		[HLEFunction(nid : 0xDF9D0C92, version : 271)]
		public virtual int sceUsbCamGetReadVideoFrameSize()
		{
			return jpegBufferSize;
		}

		/// <summary>
		/// Sets the saturation
		/// </summary>
		/// <param name="saturation"> - The saturation (0-255)
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6E205974, version = 271) public int sceUsbCamSetSaturation(int saturation)
		[HLEFunction(nid : 0x6E205974, version : 271)]
		public virtual int sceUsbCamSetSaturation(int saturation)
		{
			this.saturation = saturation;

			return 0;
		}

		/// <summary>
		/// Sets the brightness
		/// </summary>
		/// <param name="brightness"> - The brightness (0-255)
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4F3D84D5, version = 271) public int sceUsbCamSetBrightness(int brightness)
		[HLEFunction(nid : 0x4F3D84D5, version : 271)]
		public virtual int sceUsbCamSetBrightness(int brightness)
		{
			this.brightness = brightness;

			return 0;
		}

		/// <summary>
		/// Sets the contrast
		/// </summary>
		/// <param name="contrast"> - The contrast (0-255)
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x09C26C7E, version = 271) public int sceUsbCamSetContrast(int contrast)
		[HLEFunction(nid : 0x09C26C7E, version : 271)]
		public virtual int sceUsbCamSetContrast(int contrast)
		{
			this.contrast = contrast;

			return 0;
		}

		/// <summary>
		/// Sets the sharpness
		/// </summary>
		/// <param name="sharpness"> - The sharpness (0-255)
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x622F83CC, version = 271) public int sceUsbCamSetSharpness(int sharpness)
		[HLEFunction(nid : 0x622F83CC, version : 271)]
		public virtual int sceUsbCamSetSharpness(int sharpness)
		{
			this.sharpness = sharpness;

			return 0;
		}

		/// <summary>
		/// Sets the image effect mode
		/// </summary>
		/// <param name="effectmode"> - The effect mode, one of ::PspUsbCamEffectMode
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD4876173, version = 271) public int sceUsbCamSetImageEffectMode(int imageEffectMode)
		[HLEFunction(nid : 0xD4876173, version : 271)]
		public virtual int sceUsbCamSetImageEffectMode(int imageEffectMode)
		{
			this.imageEffectMode = imageEffectMode;

			return 0;
		}

		/// <summary>
		/// Sets the exposure level
		/// </summary>
		/// <param name="ev"> - The exposure level, one of ::PspUsbCamEVLevel
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1D686870, version = 271) public int sceUsbCamSetEvLevel(int evLevel)
		[HLEFunction(nid : 0x1D686870, version : 271)]
		public virtual int sceUsbCamSetEvLevel(int evLevel)
		{
			this.evLevel = evLevel;

			return 0;
		}

		/// <summary>
		/// Sets the reverse mode
		/// </summary>
		/// <param name="reverseflags"> - The reverse flags, zero or more of ::PspUsbCamReverseFlags
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x951BEDF5, version = 271) public int sceUsbCamSetReverseMode(int reverseMode)
		[HLEFunction(nid : 0x951BEDF5, version : 271)]
		public virtual int sceUsbCamSetReverseMode(int reverseMode)
		{
			this.flip = (reverseMode & PSP_USBCAM_FLIP) != 0;
			this.mirror = (reverseMode & PSP_USBCAM_MIRROR) != 0;

			return 0;
		}

		/// <summary>
		/// Sets the zoom.
		/// </summary>
		/// <param name="zoom"> - The zoom level starting by 10. (10 = 1X, 11 = 1.1X, etc)
		/// 
		/// @returns 0 on success, < 0 on error </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC484901F, version = 271) public int sceUsbCamSetZoom(int zoom)
		[HLEFunction(nid : 0xC484901F, version : 271)]
		public virtual int sceUsbCamSetZoom(int zoom)
		{
			this.zoom = zoom;

			return 0;
		}

		/// <summary>
		/// Gets the current saturation
		/// </summary>
		/// <param name="saturation"> - pointer to a variable that receives the current saturation
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x383E9FA8, version = 271) public int sceUsbCamGetSaturation(pspsharp.HLE.TPointer32 saturationAddr)
		[HLEFunction(nid : 0x383E9FA8, version : 271)]
		public virtual int sceUsbCamGetSaturation(TPointer32 saturationAddr)
		{
			saturationAddr.setValue(saturation);

			return 0;
		}

		/// <summary>
		/// Gets the current brightness
		/// </summary>
		/// <param name="brightness"> - pointer to a variable that receives the current brightness
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x70F522C5, version = 271) public int sceUsbCamGetBrightness(pspsharp.HLE.TPointer32 brightnessAddr)
		[HLEFunction(nid : 0x70F522C5, version : 271)]
		public virtual int sceUsbCamGetBrightness(TPointer32 brightnessAddr)
		{
			brightnessAddr.setValue(brightness);

			return 0;
		}

		/// <summary>
		/// Gets the current contrast
		/// </summary>
		/// <param name="contrast"> - pointer to a variable that receives the current contrast
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA063A957, version = 271) public int sceUsbCamGetContrast(pspsharp.HLE.TPointer32 contrastAddr)
		[HLEFunction(nid : 0xA063A957, version : 271)]
		public virtual int sceUsbCamGetContrast(TPointer32 contrastAddr)
		{
			contrastAddr.setValue(contrast);

			return 0;
		}

		/// <summary>
		/// Gets the current sharpness
		/// </summary>
		/// <param name="brightness"> - pointer to a variable that receives the current sharpness
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFDB68C23, version = 271) public int sceUsbCamGetSharpness(pspsharp.HLE.TPointer32 sharpnessAddr)
		[HLEFunction(nid : 0xFDB68C23, version : 271)]
		public virtual int sceUsbCamGetSharpness(TPointer32 sharpnessAddr)
		{
			sharpnessAddr.setValue(sharpness);

			return 0;
		}

		/// <summary>
		/// Gets the current image efect mode
		/// </summary>
		/// <param name="effectmode"> - pointer to a variable that receives the current effect mode
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x994471E0, version = 271) public int sceUsbCamGetImageEffectMode(pspsharp.HLE.TPointer32 imageEffectModeAddr)
		[HLEFunction(nid : 0x994471E0, version : 271)]
		public virtual int sceUsbCamGetImageEffectMode(TPointer32 imageEffectModeAddr)
		{
			imageEffectModeAddr.setValue(imageEffectMode);

			return 0;
		}

		/// <summary>
		/// Gets the current exposure level.
		/// </summary>
		/// <param name="ev"> - pointer to a variable that receives the current exposure level
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2BCD50C0, version = 271) public int sceUsbCamGetEvLevel(pspsharp.HLE.TPointer32 evLevelAddr)
		[HLEFunction(nid : 0x2BCD50C0, version : 271)]
		public virtual int sceUsbCamGetEvLevel(TPointer32 evLevelAddr)
		{
			evLevelAddr.setValue(evLevel);

			return 0;
		}

		/// <summary>
		/// Gets the current reverse mode.
		/// </summary>
		/// <param name="reverseflags"> - pointer to a variable that receives the current reverse mode flags
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD5279339, version = 271) public int sceUsbCamGetReverseMode(pspsharp.HLE.TPointer32 reverseModeAddr)
		[HLEFunction(nid : 0xD5279339, version : 271)]
		public virtual int sceUsbCamGetReverseMode(TPointer32 reverseModeAddr)
		{
			int reverseMode = 0;
			if (mirror)
			{
				reverseMode |= PSP_USBCAM_MIRROR;
			}
			if (flip)
			{
				reverseMode |= PSP_USBCAM_FLIP;
			}

			reverseModeAddr.setValue(reverseMode);

			return 0;
		}

		/// <summary>
		/// Gets the current zoom.
		/// </summary>
		/// <param name="zoom"> - pointer to a variable that receives the current zoom
		/// </param>
		/// <returns> 0 on success, < 0 on error </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9E8AAF8D, version = 271) public int sceUsbCamGetZoom(pspsharp.HLE.TPointer32 zoomAddr)
		[HLEFunction(nid : 0x9E8AAF8D, version : 271)]
		public virtual int sceUsbCamGetZoom(TPointer32 zoomAddr)
		{
			zoomAddr.setValue(zoom);

			return 0;
		}

		/// <summary>
		/// Gets the state of the autoreversal of the image.
		/// </summary>
		/// <returns> 1 if it is set to automatic, 0 otherwise </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x11A1F128, version = 271) public boolean sceUsbCamGetAutoImageReverseState()
		[HLEFunction(nid : 0x11A1F128, version : 271)]
		public virtual bool sceUsbCamGetAutoImageReverseState()
		{
			return autoImageReverseSW;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x08AEE98A, version = 271) public int sceUsbCamSetMicGain()
		[HLEFunction(nid : 0x08AEE98A, version : 271)]
		public virtual int sceUsbCamSetMicGain()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2E930264, version = 271) public int sceUsbCamSetupMicEx(pspsharp.HLE.kernel.types.pspUsbCamSetupMicExParam camSetupMicExParam, pspsharp.HLE.TPointer workArea, int workAreaSize)
		[HLEFunction(nid : 0x2E930264, version : 271)]
		public virtual int sceUsbCamSetupMicEx(pspUsbCamSetupMicExParam camSetupMicExParam, TPointer workArea, int workAreaSize)
		{
			micFrequency = camSetupMicExParam.frequency;
			micGain = camSetupMicExParam.gain;

			return 0;
		}

		[HLEFunction(nid : 0x36636925, version : 271)]
		public virtual int sceUsbCamReadMicBlocking(TPointer buffer, int bufferSize)
		{
			return Modules.sceAudioModule.hleAudioInputBlocking(bufferSize >> 1, micFrequency, buffer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3DC0088E, version = 271) public int sceUsbCamReadMic(pspsharp.HLE.TPointer buffer, int bufferSize)
		[HLEFunction(nid : 0x3DC0088E, version : 271)]
		public virtual int sceUsbCamReadMic(TPointer buffer, int bufferSize)
		{
			readMicBuffer = buffer;
			readMicBufferSize = bufferSize;

			buffer.clear(bufferSize);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x41EE8797, version = 271) public int sceUsbCamUnregisterLensRotationCallback()
		[HLEFunction(nid : 0x41EE8797, version : 271)]
		public virtual int sceUsbCamUnregisterLensRotationCallback()
		{
			return 0;
		}

		[HLEFunction(nid : 0x5145868A, version : 271)]
		public virtual int sceUsbCamStopMic()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5778B452, version = 271) public int sceUsbCamGetMicDataLength()
		[HLEFunction(nid : 0x5778B452, version : 271)]
		public virtual int sceUsbCamGetMicDataLength()
		{
			return readMicBufferSize;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6784E6A8, version = 271) public int sceUsbCamSetAntiFlicker()
		[HLEFunction(nid : 0x6784E6A8, version : 271)]
		public virtual int sceUsbCamSetAntiFlicker()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAA7D94BA, version = 271) public int sceUsbCamGetAntiFlicker()
		[HLEFunction(nid : 0xAA7D94BA, version : 271)]
		public virtual int sceUsbCamGetAntiFlicker()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB048A67D, version = 271) public int sceUsbCamWaitReadMicEnd()
		[HLEFunction(nid : 0xB048A67D, version : 271)]
		public virtual int sceUsbCamWaitReadMicEnd()
		{
			return Modules.sceAudioModule.hleAudioInputBlocking(readMicBufferSize >> 1, micFrequency, readMicBuffer);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD293A100, version = 271) public int sceUsbCamRegisterLensRotationCallback()
		[HLEFunction(nid : 0xD293A100, version : 271)]
		public virtual int sceUsbCamRegisterLensRotationCallback()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF8847F60, version = 271) public int sceUsbCamPollReadMicEnd()
		[HLEFunction(nid : 0xF8847F60, version : 271)]
		public virtual int sceUsbCamPollReadMicEnd()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1E958148, version = 271) public int sceUsbCamIoctl()
		[HLEFunction(nid : 0x1E958148, version : 271)]
		public virtual int sceUsbCamIoctl()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x00631D06, version = 271) public int sceUsbCam_00631D06()
		[HLEFunction(nid : 0x00631D06, version : 271)]
		public virtual int sceUsbCam_00631D06()
		{
			return 0;
		}
	}

}