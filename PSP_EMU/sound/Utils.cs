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
namespace pspsharp.sound
{
	using Level = org.apache.log4j.Level;

	using Modules = pspsharp.HLE.Modules;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class Utils
	{
		/// <summary>
		/// Clamp a mono audio sample to a valid range [-0x8000..0x7FFF].
		/// </summary>
		/// <param name="value"> a signed 32-bit value </param>
		/// <returns>      0x7FFF if the input value is larger than 0x7FFF
		///              -0x8000 if the input value is smaller than -0x8000
		///              otherwise the input value itself. </returns>
		public static int clampMono(int value)
		{
			return System.Math.Max(System.Math.Min(value, 0x7FFF), -0x8000);
		}

		/// <summary>
		/// Mix (i.e. add) 2 mono audio values into a new mono audio value.
		/// A mono audio value is a signed 16-bit value in the range [-0x8000..0x7FFF].
		/// </summary>
		/// <param name="value1"> a value in the range [-0x8000..0x7FFF] </param>
		/// <param name="value2"> a value in the range [-0x8000..0x7FFF] </param>
		/// <returns>       the sum of value1 and value2, clamped to the range [-0x8000..0x7FFF] </returns>
		public static int mixMono(int value1, int value2)
		{
			return clampMono(value1 + value2);
		}

		/// <summary>
		/// Retrieve the right mono value from a stereo sample value.
		/// A stereo sample has the format
		///   0xRRRRLLLL
		/// where 0xRRRR is the signed 16-bit mono value for the right channel
		/// and 0xLLLL is the signed 16-bit mono value for the left channel.
		/// Each mono value is in the range [-0x8000..0x7FFF].
		/// </summary>
		/// <param name="value"> the stereo sample </param>
		/// <returns>      the mono value for the right channel (0xRRRR), in the range [-0x8000..0x7FFF] </returns>
		public static int getRightStereo(int value)
		{
			return value >> 16;
		}

		/// <summary>
		/// Retrieve the right mono value from a stereo sample value.
		/// A stereo sample has the format
		///   0xRRRRLLLL
		/// where 0xRRRR is the signed 16-bit mono value for the right channel
		/// and 0xLLLL is the signed 16-bit mono value for the left channel.
		/// Each mono value is in the range [-0x8000..0x7FFF].
		/// </summary>
		/// <param name="value"> the stereo sample </param>
		/// <returns>      the mono value for the left channel (0xLLLL), in the range [-0x8000..0x7FFF] </returns>
		public static int getLeftStereo(int value)
		{
			return (short) value;
		}

		/// <summary>
		/// Build a stereo sample value based on 2 mono values.
		/// Each mono value is assumed to be in the range [-0x8000..0x7FFF].
		/// An unexpected is returned if this is not the case.
		/// 
		/// A stereo sample has the format
		///   0xRRRRLLLL
		/// where 0xRRRR is the signed 16-bit mono value for the right channel
		/// and 0xLLLL is the signed 16-bit mono value for the left channel.
		/// Each mono value is in the range [-0x8000..0x7FFF].
		/// </summary>
		/// <param name="leftValue">  the mono value for the left channel (0xLLLL), in the range [-0x8000..0x7FFF] </param>
		/// <param name="rightValue"> the mono value for the right channel (0xRRRR), in the range [-0x8000..0x7FFF] </param>
		/// <returns>           the stereo sample value (0xRRRRLLLL) </returns>
		public static int getStereoWithoutClamp(int leftValue, int rightValue)
		{
			return (rightValue << 16) | (leftValue & 0xFFFF);
		}

		/// <summary>
		/// Build a stereo sample value based on 2 mono values.
		/// Each mono value will be clamped to the range [-0x8000..0x7FFF] before processing.
		/// 
		/// A stereo sample has the format
		///   0xRRRRLLLL
		/// where 0xRRRR is the signed 16-bit mono value for the right channel
		/// and 0xLLLL is the signed 16-bit mono value for the left channel.
		/// Each mono value is in the range [-0x8000..0x7FFF].
		/// </summary>
		/// <param name="leftValue">  the mono value for the left channel (0xLLLL) </param>
		/// <param name="rightValue"> the mono value for the right channel (0xRRRR) </param>
		/// <returns>           the stereo sample value (0xRRRRLLLL) </returns>
		public static int getStereo(int leftValue, int rightValue)
		{
			return getStereoWithoutClamp(clampMono(leftValue), clampMono(rightValue));
		}

		/// <summary>
		/// Multiply a stereo sample value by a volume value.
		/// The volume value has to be in the range [0..1],
		/// otherwise an unexpected result is returned.
		/// Each left and right channel can have different volume values.
		/// 
		/// A stereo sample has the format
		///   0xRRRRLLLL
		/// where 0xRRRR is the signed 16-bit mono value for the right channel
		/// and 0xLLLL is the signed 16-bit mono value for the left channel.
		/// Each mono value is in the range [-0x8000..0x7FFF].
		/// </summary>
		/// <param name="stereoValue"> the base stereo value </param>
		/// <param name="leftVolume">  the volume value for the left channel value,
		///                    in the range [0..1] </param>
		/// <param name="rightVolume"> the volume value for the right channel value,
		///                    in the range [0..1] </param>
		/// <returns>            the base stereo value multiplied by the volume values </returns>
		public static int getStereo(int stereoValue, float leftVolume, float rightVolume)
		{
			return getStereoWithoutClamp((int)(getLeftStereo(stereoValue) * leftVolume), (int)(getRightStereo(stereoValue) * rightVolume));
		}

		/// <summary>
		/// Mix (i.e. add) 2 stereo audio samples into a new stereo audio sample.
		/// 
		/// A stereo sample has the format
		///   0xRRRRLLLL
		/// where 0xRRRR is the signed 16-bit mono value for the right channel
		/// and 0xLLLL is the signed 16-bit mono value for the left channel.
		/// Each mono value is in the range [-0x8000..0x7FFF].
		/// </summary>
		/// <param name="value1"> the first stereo sample </param>
		/// <param name="value2"> the second stereo sample </param>
		/// <returns>       the sum of the first and second stereo samples,
		///               the mono values being clamped to a valid range. </returns>
		public static int mixStereo(int value1, int value2)
		{
			return getStereoWithoutClamp(mixMono(getLeftStereo(value1), getLeftStereo(value2)), mixMono(getRightStereo(value1), getRightStereo(value2)));
		}

		/// <summary>
		/// Mix stereo samples in memory: add one stereo sample stream to another
		/// stereo sample stream.
		/// </summary>
		/// <param name="inAddr">    the start address of the input stereo sample stream </param>
		/// <param name="inOutAddr"> the start address of the stereo sample being updated </param>
		/// <param name="samples">   the number of stereo samples </param>
		public static void mixStereoInMemory(int inAddr, int inOutAddr, int samples)
		{
			int length = samples << 2;
			IMemoryReader inReader = MemoryReader.getMemoryReader(inAddr, length, 4);
			IMemoryReader inOutReader = MemoryReader.getMemoryReader(inOutAddr, length, 4);
			IMemoryWriter inOutWriter = MemoryWriter.getMemoryWriter(inOutAddr, length, 4);

			for (int i = 0; i < samples; i++)
			{
				int inStereoValue = inReader.readNext();
				if (inStereoValue == 0)
				{
					// InOut unchanged for this sample
					inOutReader.skip(1);
					inOutWriter.skip(1);
				}
				else
				{
					int inOutStereoValue = inOutReader.readNext();
					inOutStereoValue = mixStereo(inStereoValue, inOutStereoValue);
					inOutWriter.writeNext(inOutStereoValue);
				}
			}
			inOutWriter.flush();
		}

		/// <summary>
		/// Mix stereo samples in memory: add one stereo sample stream (multiplied by
		/// a given volume value) to another stereo sample stream.
		/// </summary>
		/// <param name="inAddr">        the start address of the input stereo sample stream </param>
		/// <param name="inOutAddr">     the start address of the stereo sample being updated </param>
		/// <param name="samples">       the number of stereo samples </param>
		/// <param name="inLeftVolume">  the volume value for the input left channel stream,
		///                      in the range [0..1] </param>
		/// <param name="inRightVolume"> the volume value for the input right channel stream,
		///                      in the range [0..1] </param>
		public static void mixStereoInMemory(int inAddr, int inOutAddr, int samples, float inLeftVolume, float inRightVolume)
		{
			if (System.Math.Abs(inLeftVolume) < 0.0001f)
			{
				inLeftVolume = 0.0f;
			}
			if (System.Math.Abs(inRightVolume) < 0.0001f)
			{
				inRightVolume = 0.0f;
			}

			if (inLeftVolume == 0.0f && inRightVolume == 0.0f)
			{
				// Nothing to do
				return;
			}

			if (inLeftVolume == 1.0f && inRightVolume == 1.0f)
			{
				// Simple case, without inVolume
				mixStereoInMemory(inAddr, inOutAddr, samples);
				return;
			}

			if (inLeftVolume < 0.0f || inLeftVolume > 1.0f)
			{
				if (Modules.log.isEnabledFor(Level.WARN))
				{
					Modules.log.warn(string.Format("Utils.mixStereoInMemory left volume outside range {0:F}", inLeftVolume));
				}
			}
			if (inRightVolume < 0.0f || inRightVolume > 1.0f)
			{
				if (Modules.log.isEnabledFor(Level.WARN))
				{
					Modules.log.warn(string.Format("Utils.mixStereoInMemory right volume outside range {0:F}", inRightVolume));
				}
			}

			int length = samples << 2;
			IMemoryReader inReader = MemoryReader.getMemoryReader(inAddr, length, 4);
			IMemoryReader inOutReader = MemoryReader.getMemoryReader(inOutAddr, length, 4);
			IMemoryWriter inOutWriter = MemoryWriter.getMemoryWriter(inOutAddr, length, 4);

			for (int i = 0; i < samples; i++)
			{
				int inStereoValue = inReader.readNext();
				if (inStereoValue == 0)
				{
					// InOut unchanged for this sample
					inOutReader.skip(1);
					inOutWriter.skip(1);
				}
				else
				{
					inStereoValue = getStereo(inStereoValue, inLeftVolume, inRightVolume);
					int inOutStereoValue = inOutReader.readNext();
					inOutStereoValue = mixStereo(inStereoValue, inOutStereoValue);
					inOutWriter.writeNext(inOutStereoValue);
				}
			}
			inOutWriter.flush();
		}
	}

}