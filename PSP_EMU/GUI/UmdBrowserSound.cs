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
namespace pspsharp.GUI
{

	using Modules = pspsharp.HLE.Modules;
	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using SysMemUserForUser = pspsharp.HLE.modules.SysMemUserForUser;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using sceAtrac3plus = pspsharp.HLE.modules.sceAtrac3plus;
	using AtracFileInfo = pspsharp.HLE.modules.sceAtrac3plus.AtracFileInfo;
	using CodecFactory = pspsharp.media.codec.CodecFactory;
	using ICodec = pspsharp.media.codec.ICodec;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Utilities = pspsharp.util.Utilities;

	public class UmdBrowserSound
	{
		private bool done;
		private bool threadExit;
		private Memory mem;
		private SysMemUserForUser.SysMemInfo memInfo;
		private int samplesAddr;
		private int inputAddr;
		private int inputLength;
		private int inputPosition;
		private int inputOffset;
		private int inputBytesPerFrame;
		private int channels;
		private ICodec codec;
		private SourceDataLine mLine;

		private class SoundPlayThread : Thread
		{
			private readonly UmdBrowserSound outerInstance;

			public SoundPlayThread(UmdBrowserSound outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void run()
			{
				while (!outerInstance.done)
				{
					outerInstance.stepSound();
				}
				outerInstance.threadExit = true;
			}
		}

		public UmdBrowserSound(Memory mem, sbyte[] data)
		{
			initMemory(mem);

			if (read(data))
			{
				startThread();
			}
			else
			{
				threadExit = true;
			}
		}

		public UmdBrowserSound(Memory mem, IVirtualFile vFile, int codecType, sceAtrac3plus.AtracFileInfo atracFileInfo)
		{
			initMemory(mem);

			sbyte[] audioData = Utilities.readCompleteFile(vFile);
			int atracBytesPerFrame = (((audioData[2] & 0x03) << 8) | ((audioData[3] & 0xFF) << 3)) + 8;
			int headerLength = 8;
			inputLength = 0;
			for (int i = 0; i < audioData.Length; i += headerLength + atracBytesPerFrame)
			{
				write(mem, inputAddr + inputLength, audioData, i + headerLength, atracBytesPerFrame);
				inputLength += atracBytesPerFrame;
			}
			atracFileInfo.atracBytesPerFrame = atracBytesPerFrame;

			if (read(codecType, atracFileInfo))
			{
				startThread();
			}
			else
			{
				threadExit = true;
			}
		}

		private void initMemory(Memory mem)
		{
			this.mem = mem;

			memInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "UmdBrowserSound", SysMemUserForUser.PSP_SMEM_Low, 0x20000, 0);
			if (memInfo != null)
			{
				samplesAddr = memInfo.addr;
				inputAddr = memInfo.addr + 0x10000;
			}
			else
			{
				// PSP not yet initialized, use any memory space
				samplesAddr = MemoryMap.START_USERSPACE;
				inputAddr = MemoryMap.START_USERSPACE + 0x10000;
			}
		}

		public virtual void stopSound()
		{
			done = true;
			while (!threadExit)
			{
				Utilities.sleep(1, 0);
			}

			if (mLine != null)
			{
				mLine.close();
			}

			if (memInfo != null)
			{
				Modules.SysMemUserForUserModule.free(memInfo);
				memInfo = null;
				samplesAddr = 0;
				inputAddr = 0;
			}
		}

		private static void write(Memory mem, int addr, sbyte[] data, int offset, int length)
		{
			length = System.Math.Min(length, data.Length - offset);
			for (int i = 0; i < length; i++)
			{
				mem.write8(addr + i, data[offset + i]);
			}
		}

		private void startThread()
		{
			Thread soundPlayThread = new SoundPlayThread(this);
			soundPlayThread.Daemon = true;
			soundPlayThread.Name = "Umd Browser Sound Play Thread";
			soundPlayThread.Start();
		}

		private bool read(sbyte[] data)
		{
			if (data == null || data.Length == 0)
			{
				return false;
			}

			inputLength = data.Length;
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(inputAddr, inputLength, 1);
			for (int i = 0; i < data.Length; i++)
			{
				memoryWriter.writeNext(data[i] & 0xFF);
			}
			memoryWriter.flush();

			sceAtrac3plus.AtracFileInfo atracFileInfo = new sceAtrac3plus.AtracFileInfo();
			int codecType = sceAtrac3plus.analyzeRiffFile(mem, inputAddr, inputLength, atracFileInfo);
			if (codecType < 0)
			{
				return false;
			}

			bool result = read(codecType, atracFileInfo);

			return result;
		}

		private bool read(int codecType, sceAtrac3plus.AtracFileInfo atracFileInfo)
		{
			codec = CodecFactory.getCodec(codecType);
			if (codec == null)
			{
				return false;
			}

			int result = codec.init(atracFileInfo.atracBytesPerFrame, atracFileInfo.atracChannels, atracFileInfo.atracChannels, atracFileInfo.atracCodingMode);
			if (result < 0)
			{
				return false;
			}

			AudioFormat audioFormat = new AudioFormat(44100, 16, atracFileInfo.atracChannels, true, false);
			DataLine.Info info = new DataLine.Info(typeof(SourceDataLine), audioFormat);
			try
			{
				mLine = (SourceDataLine) AudioSystem.getLine(info);
				mLine.open(audioFormat);
			}
			catch (LineUnavailableException)
			{
				return false;
			}
			mLine.start();

			inputOffset = atracFileInfo.inputFileDataOffset;
			inputPosition = inputOffset;
			inputBytesPerFrame = atracFileInfo.atracBytesPerFrame;
			channels = atracFileInfo.atracChannels;

			return true;
		}

		private bool stepSound()
		{
			if (inputPosition + inputBytesPerFrame >= inputLength)
			{
				// Loop sound
				inputPosition = inputOffset;
			}

			int result = codec.decode(inputAddr + inputPosition, inputBytesPerFrame, samplesAddr);
			if (result < 0)
			{
				return false;
			}

			inputPosition += inputBytesPerFrame;

			sbyte[] bytes = new sbyte[codec.NumberOfSamples * 2 * channels];
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = (sbyte) mem.read8(samplesAddr + i);
			}
			mLine.write(bytes, 0, bytes.Length);

			return true;
		}
	}

}