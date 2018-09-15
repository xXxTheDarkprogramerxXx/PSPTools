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

	//using Logger = org.apache.log4j.Logger;

	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryReaderWriter = pspsharp.memory.IMemoryReaderWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryReaderWriter = pspsharp.memory.MemoryReaderWriter;

	// Positional 3D Audio Library
	public class sceP3da : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceP3da");

		public const int PSP_P3DA_SAMPLES_NUM_STEP = 32;
		public const int PSP_P3DA_SAMPLES_NUM_MIN = 64;
		public const int PSP_P3DA_SAMPLES_NUM_DEFAULT = 256;
		public const int PSP_P3DA_SAMPLES_NUM_MAX = 2048;

		public const int PSP_P3DA_CHANNELS_NUM_MAX = 4;

		protected internal int p3daChannelsNum;
		protected internal int p3daSamplesNum;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x374500A5, version = 280) public int sceP3daBridgeInit(int channelsNum, int samplesNum)
		[HLEFunction(nid : 0x374500A5, version : 280)]
		public virtual int sceP3daBridgeInit(int channelsNum, int samplesNum)
		{
			p3daChannelsNum = channelsNum;
			p3daSamplesNum = samplesNum;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x43F756A2, version = 280) public int sceP3daBridgeExit()
		[HLEFunction(nid : 0x43F756A2, version : 280)]
		public virtual int sceP3daBridgeExit()
		{
			return 0;
		}

		[HLEFunction(nid : 0x013016F3, version : 280)]
		public virtual int sceP3daBridgeCore(TPointer32 p3daCoreAddr, int channelsNum, int samplesNum, TPointer32 inputAddr, TPointer outputAddr)
		{
			int[] p3daCore = new int[2];
			for (int i = 0; i < p3daCore.Length; i++)
			{
				p3daCore[i] = p3daCoreAddr.getValue(i << 2);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceP3daBridgeCore p3daCore[{0:D}]=0x{1:X8}", i, p3daCore[i]));
				}
			}

			outputAddr.clear(samplesNum * 4);
			for (int i = 0; i < channelsNum; i++)
			{
				int inputChannelAddr = inputAddr.getValue(i << 2);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceP3daBridgeCore channel={0:D}, inputChannelAddr=0x{1:X8}", i, inputChannelAddr));
				}
				if (inputChannelAddr != 0)
				{
					IMemoryReaderWriter outputReaderWriter = MemoryReaderWriter.getMemoryReaderWriter(outputAddr.Address, samplesNum << 2, 2);
					IMemoryReader inputChannelReader = MemoryReader.getMemoryReader(inputChannelAddr, samplesNum << 1, 2);
					for (int sample = 0; sample < samplesNum; sample++)
					{
						int inputSample = inputChannelReader.readNext();
						int outputSampleLeft = outputReaderWriter.readCurrent();
						outputReaderWriter.writeNext(inputSample + outputSampleLeft);
						int outputSampleRight = outputReaderWriter.readCurrent();
						outputReaderWriter.writeNext(inputSample + outputSampleRight);
					}
					outputReaderWriter.flush();
				}
			}

			// Overwrite these values, just like in sceSasCore.
			p3daChannelsNum = channelsNum;
			p3daSamplesNum = samplesNum;

			Modules.ThreadManForUserModule.hleKernelDelayThread(600, false);

			return 0;
		}
	}
}