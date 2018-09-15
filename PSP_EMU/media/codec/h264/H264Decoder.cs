using System;

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
namespace pspsharp.media.codec.h264
{
	//using Logger = org.apache.log4j.Logger;

	using AVFrame = com.twilight.h264.decoder.AVFrame;
	using AVPacket = com.twilight.h264.decoder.AVPacket;
	using AVRational = com.twilight.h264.decoder.AVRational;
	using MpegEncContext = com.twilight.h264.decoder.MpegEncContext;

	public class H264Decoder : IVideoCodec
	{
		private static Logger log = Logger.getLogger("h264");
		private MpegEncContext context;
		private AVFrame picture;
		private AVPacket packet;
		private readonly int[] gotPicture = new int[1];
		private AVRational aspectRatio;

		public virtual int init(int[] extraData)
		{
			context = MpegEncContext.avcodec_alloc_context();

			picture = AVFrame.avcodec_alloc_frame();

			packet = new AVPacket();
			packet.av_init_packet();

			if (extraData != null)
			{
				context.extradata_size = extraData.Length;
				// Add 4 additional values to avoid exceptions while parsing
				int[] extraDataPlus4 = new int[context.extradata_size + 4];
				Array.Copy(extraData, 0, extraDataPlus4, 0, context.extradata_size);
				context.extradata = extraDataPlus4;
			}

			int result = context.avcodec_open(new com.twilight.h264.decoder.H264Decoder());
			if (result < 0)
			{
				return result;
			}

			gotPicture[0] = 0;

			return 0;
		}

		public virtual int decode(int[] input, int inputOffset, int inputLength)
		{
			packet.data_base = input;
			packet.data_offset = inputOffset;
			packet.size = inputLength;

			int consumedLength;
			try
			{
				consumedLength = context.avcodec_decode_video2(picture, gotPicture, packet);
			}
			catch (System.IndexOutOfRangeException e)
			{
				Console.WriteLine("H264Decoder.decode", e);
				return -1;
			}

			if (consumedLength < 0)
			{
				Console.WriteLine(string.Format("H264 decode error 0x{0:X8}", consumedLength));
				gotPicture[0] = 0;
				return consumedLength;
			}

			if (hasImage())
			{
				context.priv_data.displayPicture.copyTo(picture);
				aspectRatio = context.sample_aspect_ratio;
			}

			return consumedLength;
		}

		public virtual bool hasImage()
		{
			return gotPicture[0] != 0;
		}

		public virtual int ImageWidth
		{
			get
			{
				return context.width;
			}
		}

		public virtual int ImageHeight
		{
			get
			{
				return context.height;
			}
		}

		public virtual bool KeyFrame
		{
			get
			{
				return picture.key_frame != 0;
			}
		}

		public virtual void getAspectRatio(int[] numDen)
		{
			numDen[0] = aspectRatio.num;
			numDen[1] = aspectRatio.den;
		}

		public virtual int getImage(int[] luma, int[] cb, int[] cr)
		{
			int width = ImageWidth;
			int height = ImageHeight;
			int width2 = width >> 1;
			int height2 = height >> 1;

			// Copy luma component: width * height values
			for (int y = 0; y < height; y++)
			{
				Array.Copy(picture.data_base[0], y * picture.linesize[0] + picture.data_offset[0], luma, y * width, width);
			}

			// Copy Cb and Cr components: width2 * height2 values each
			for (int y = 0; y < height2; y++)
			{
				Array.Copy(picture.data_base[1], y * picture.linesize[1] + picture.data_offset[1], cb, y * width2, width2);
				Array.Copy(picture.data_base[2], y * picture.linesize[2] + picture.data_offset[2], cr, y * width2, width2);
			}

			return 0;
		}
	}

}