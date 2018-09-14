using System.Text;

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
	using Logger = org.apache.log4j.Logger;

	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class SampleSourceVAG : ISampleSource
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			sampleIndex = samples.Length;
		}

		private static Logger log = SoftwareSynthesizer.log;
		private SoundVoice voice;
		private int address;
		private int numberSamples;
		private IMemoryReader memoryReader;
		private readonly int[] unpackedSamples = new int[28];
		private readonly short[] samples = new short[28];
		private int sampleIndex;
		private int numberVGABlocks;
		private int currentVAGBlock;
		private int currentSampleIndex;
		private int hist1;
		private int hist2;
		private bool loopMode;
		private int loopStartVAGBlock;
		private bool loopAtNextVAGBlock;
		private static readonly double[][] VAG_f = new double[][]
		{
			new double[] {0.0, 0.0},
			new double[] {60.0 / 64.0, 0.0},
			new double[] {115.0 / 64.0, -52.0 / 64.0},
			new double[] {98.0 / 64.0, -55.0 / 64.0},
			new double[] {122.0 / 64.0, -60.0 / 64.0}
		};

		public SampleSourceVAG(SoundVoice voice, int address, int size, bool loopMode)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			this.voice = voice;
			this.address = address;
			this.loopMode = loopMode;

			if (address == 0)
			{
				numberSamples = 0;
				numberVGABlocks = 0;
			}
			else
			{
				readHeader();

				numberVGABlocks = size / 16;
				numberSamples = numberVGABlocks * 28;
				currentSampleIndex = -1;
				SampleIndex = 0;

				if (log.TraceEnabled)
				{
					log.trace(string.Format("VAG numberVGABlocks={0:D}, numberSamples={1:D}", numberVGABlocks, numberSamples));
				}
			}
		}

		private void readHeader()
		{
			Memory mem = Memory.Instance;

			int header = mem.read32(address);
			if ((header & 0x00FFFFFF) == 0x00474156)
			{ // VAGx.
				int version = Integer.reverseBytes(mem.read32(address + 4));
				int dataSize = Integer.reverseBytes(mem.read32(address + 12));
				int sampleRate = Integer.reverseBytes(mem.read32(address + 16));
//JAVA TO C# CONVERTER TODO TASK: There is no .NET StringBuilder equivalent to the Java 'reverse' method:
				string dataName = (new StringBuilder(Utilities.readStringNZ(address + 32, 16))).reverse().ToString();
				if (log.DebugEnabled)
				{
					log.debug(string.Format("SampleSourceVAG found VAG/ADPCM data: version={0:D}, size={1:D}, sampleRate={2:D}, dataName='{3}'", version, dataSize, sampleRate, dataName));
				}
				address += 0x30;
			}
		}

		private bool unpackNextVAGBlock()
		{
			if (currentVAGBlock >= numberVGABlocks)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("VAG reached end of blocks currentVAGBlock={0:D}, numberVAGBlocks={1:D}", currentVAGBlock, numberVGABlocks));
				}
				return false;
			}

			sampleIndex = 0;

			int n = memoryReader.readNext();
			int predict_nr = n >> 4;
			if (predict_nr >= VAG_f.Length)
			{
				predict_nr = 0;
			}
			int shift_factor = n & 0x0F;
			int flag = memoryReader.readNext();
			if (flag == 0x03)
			{
				// If loop mode is enabled, this flag indicates
				// the final block of the loop.
				// Do not loop if the voice has been keyed Off.
				if (loopMode && voice.On)
				{
					if (log.TraceEnabled)
					{
						log.trace(string.Format("SampleSourceVAG loop at next VAG Block[{0:D}], voice=0x{1:X}", currentVAGBlock, voice.Index));
					}
					loopAtNextVAGBlock = true;
				}
			}
			else if (flag == 0x06)
			{
				// If loop mode is enabled, this flag indicates
				// the first block of the loop.
				// TODO: Implement loop processing by decoding
				// the same samples within the loop flags
				// when loop mode is on.
				if (log.TraceEnabled)
				{
					log.trace(string.Format("SampleSourceVAG loop start VAG Block[{0:D}], voice=0x{1:X}", currentVAGBlock, voice.Index));
				}
				loopStartVAGBlock = currentVAGBlock;
			}
			else if (flag == 0x07)
			{
				numberVGABlocks = currentVAGBlock;
				numberSamples = numberVGABlocks * 28;
				sampleIndex = samples.Length;
				return false; // End of stream flag.
			}

			for (int j = 0; j < 28; j += 2)
			{
				int d = memoryReader.readNext();
				int s = (short)((d & 0x0F) << 12);
				unpackedSamples[j] = s >> shift_factor;
				s = (short)((d & 0xF0) << 8);
				unpackedSamples[j + 1] = s >> shift_factor;
			}

			for (int j = 0; j < 28; j++)
			{
				int sample = (int)(unpackedSamples[j] + hist1 * VAG_f[predict_nr][0] + hist2 * VAG_f[predict_nr][1]);
				hist2 = hist1;
				hist1 = sample;
				if (sample < -32768)
				{
					samples[j] = -32768;
				}
				else if (sample > 0x7FFF)
				{
					samples[j] = 0x7FFF;
				}
				else
				{
					samples[j] = (short) sample;
				}
			}

			currentVAGBlock++;

			return true;
		}

		public virtual int NextSample
		{
			get
			{
				if (sampleIndex >= samples.Length)
				{
					if (!unpackNextVAGBlock())
					{
						return 0;
					}
				}
    
				short sample = samples[sampleIndex];
				if (log.TraceEnabled)
				{
					log.trace(string.Format("SampleSourceVAG.getNextSample[{0:D}/{1:D}]=0x{2:X4}, voice=0x{3:X}", sampleIndex, currentVAGBlock, sample & 0xFFFF, voice.Index));
				}
    
				sampleIndex++;
				currentSampleIndex++;
    
				if (loopAtNextVAGBlock && sampleIndex >= samples.Length)
				{
					loopAtNextVAGBlock = false;
					SampleIndex = loopStartVAGBlock * 28;
				}
    
				return sample & 0x0000FFFF;
			}
		}

		private int SampleIndex
		{
			set
			{
				currentSampleIndex = value;
				currentVAGBlock = value / 28;
    
				if (currentVAGBlock >= numberVGABlocks)
				{
					sampleIndex = samples.Length;
				}
				else
				{
					int restSamples = numberSamples - value;
					memoryReader = MemoryReader.getMemoryReader(address + (currentVAGBlock << 4), restSamples << 2, 1);
					if (unpackNextVAGBlock())
					{
						sampleIndex = value % 28;
					}
				}
    
				if (log.TraceEnabled)
				{
					log.trace(string.Format("SampleSourceVAG.setSampleIndex {0:D} = {1:D}/{2:D}, voice=0x{3:X}", value, sampleIndex, currentVAGBlock, voice.Index));
				}
			}
		}

		public virtual void resetToStart()
		{
			currentSampleIndex = -1;
			SampleIndex = 0;
		}

		public virtual bool Ended
		{
			get
			{
				if (currentVAGBlock > numberVGABlocks)
				{
					return true;
				}
				if (currentVAGBlock == numberVGABlocks && sampleIndex >= samples.Length)
				{
					return true;
				}
    
				return false;
			}
		}

		public override string ToString()
		{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("SampleSourceVAG[index=%d,VAG=%d[%d],loopStart=%d,loop at next=%b]", currentSampleIndex, currentVAGBlock, sampleIndex, loopStartVAGBlock, loopAtNextVAGBlock);
			return string.Format("SampleSourceVAG[index=%d,VAG=%d[%d],loopStart=%d,loop at next=%b]", currentSampleIndex, currentVAGBlock, sampleIndex, loopStartVAGBlock, loopAtNextVAGBlock);
		}
	}

}