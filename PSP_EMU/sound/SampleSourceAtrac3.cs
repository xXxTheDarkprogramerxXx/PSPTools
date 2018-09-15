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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ATRAC_ALL_DATA_DECODED;

	//using Logger = org.apache.log4j.Logger;

	using TPointer32 = pspsharp.HLE.TPointer32;
	using sceSasCore = pspsharp.HLE.modules.sceSasCore;
	using AtracID = pspsharp.HLE.modules.sceAtrac3plus.AtracID;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class SampleSourceAtrac3 : ISampleSource
	{
		private Logger log = sceSasCore.log;
		private readonly AtracID id;
		private readonly int maxSamples;
		private readonly int buffer;
		private int sampleIndex;
		private int currentSampleIndex;
		private int bufferedSamples;
		private readonly Memory mem;

		public SampleSourceAtrac3(AtracID id)
		{
			this.id = id;
			maxSamples = id.MaxSamples;
			id.createInternalBuffer(maxSamples * 4);
			buffer = id.InternalBuffer.addr;
			sampleIndex = 0;
			bufferedSamples = 0;
			currentSampleIndex = -1;
			mem = Memory.Instance;
		}

		private void decode()
		{
			int result = id.decodeData(buffer, TPointer32.NULL);
			if (result == ERROR_ATRAC_ALL_DATA_DECODED)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("SampleSourceAtrac3 decodeData returned 0x{0:X8}", result));
				}
				bufferedSamples = 0;
			}
			else if (result < 0)
			{
				Console.WriteLine(string.Format("SampleSourceAtrac3 decodeData returned 0x{0:X8}", result));
				bufferedSamples = 0;
			}
			else
			{
				bufferedSamples = id.Codec.NumberOfSamples;
			}

			if (!id.InputBuffer.FileEnd)
			{
				int requestedSize = min(id.InputFileSize - id.InputBuffer.FilePosition, id.InputBuffer.MaxSize);
				id.setContextDecodeResult(0, requestedSize);
			}
			else
			{
				id.setContextDecodeResult(0, 0);
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("SampleSourceAtrac3 decode: bufferedSamples={0:D}, currentSample={1:D}, endSample={2:D}", bufferedSamples, currentSampleIndex, id.AtracEndSample));
			}

			sampleIndex = 0;
		}

		public virtual int NextSample
		{
			get
			{
				if (sampleIndex >= bufferedSamples)
				{
					if (Ended)
					{
						return 0;
					}
					decode();
					if (bufferedSamples <= 0)
					{
						return 0;
					}
				}
    
				int sample = mem.read32(buffer + (sampleIndex << 2));
				currentSampleIndex++;
				sampleIndex++;
    
				return sample;
			}
		}

		public virtual void resetToStart()
		{
			currentSampleIndex = 0;
			id.PlayPosition = 0;
		}

		public virtual bool Ended
		{
			get
			{
				return currentSampleIndex >= id.AtracEndSample;
			}
		}
	}

}