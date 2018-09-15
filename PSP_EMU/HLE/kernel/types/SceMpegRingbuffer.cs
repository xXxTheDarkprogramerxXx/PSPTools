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
namespace pspsharp.HLE.kernel.types
{

	public class SceMpegRingbuffer : pspAbstractMemoryMappedStructure
	{
		public const int ringbufferPacketSize = 2048;
		// PSP info
		private int packets;
		private int packetsRead;
		private int packetsWritten;
		private int packetsInRingbuffer;
		private int packetSize; // 2048
		private int data; // address, ring buffer
		private int callbackAddr; // see sceMpegRingbufferPut
		private int callbackArgs;
		private int dataUpperBound;
		private int semaID; // unused?
		private int mpeg; // pointer to mpeg struct, fixed up in sceMpegCreate
		// Internal info
		private pspFileBuffer videoBuffer;
		private pspFileBuffer audioBuffer;
		private pspFileBuffer userDataBuffer;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool hasAudio_Renamed = false;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool hasVideo_Renamed = true;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private bool hasUserData_Renamed = false;
		private int writePacketOffset;
		private int internalPacketsInRingbuffer;

		public SceMpegRingbuffer(int packets, int data, int size, int callbackAddr, int callbackArgs)
		{
			this.packets = packets;
			packetSize = ringbufferPacketSize;
			this.data = data;
			this.callbackAddr = callbackAddr;
			this.callbackArgs = callbackArgs;
			dataUpperBound = data + packets * ringbufferPacketSize;
			semaID = -1;
			mpeg = 0;
			initBuffer();

			if (dataUpperBound > data + size)
			{
				dataUpperBound = data + size;
				Modules.Console.WriteLine("SceMpegRingbuffer clamping dataUpperBound to " + dataUpperBound);
			}

			reset();
		}

		private SceMpegRingbuffer()
		{
		}

		private void initBuffer()
		{
			videoBuffer = new pspFileBuffer(data, dataUpperBound - data);
			audioBuffer = new pspFileBuffer(data, dataUpperBound - data);
			userDataBuffer = new pspFileBuffer(data, dataUpperBound - data);
			// No check on file size for MPEG.
			videoBuffer.FileMaxSize = int.MaxValue;
			audioBuffer.FileMaxSize = int.MaxValue;
			userDataBuffer.FileMaxSize = int.MaxValue;

			writePacketOffset = 0;
		}

		public static SceMpegRingbuffer fromMem(TPointer address)
		{
			SceMpegRingbuffer ringbuffer = new SceMpegRingbuffer();
			ringbuffer.read(address);
			ringbuffer.initBuffer();

			return ringbuffer;
		}

		public virtual void reset()
		{
			packetsRead = 0;
			packetsWritten = 0;
			packetsInRingbuffer = 0;
			internalPacketsInRingbuffer = 0;
			writePacketOffset = 0;
			videoBuffer.reset(0, 0);
			audioBuffer.reset(0, 0);
			userDataBuffer.reset(0, 0);
		}

		protected internal override void read()
		{
			packets = read32();
			packetsRead = read32();
			packetsWritten = read32();
			packetsInRingbuffer = read32();
			packetSize = read32();
			data = read32();
			callbackAddr = read32();
			callbackArgs = read32();
			dataUpperBound = read32();
			semaID = read32();
			mpeg = read32();
		}

		protected internal override void write()
		{
			write32(packets);
			write32(packetsRead);
			write32(packetsWritten);
			write32(packetsInRingbuffer);
			write32(packetSize);
			write32(data);
			write32(callbackAddr);
			write32(callbackArgs);
			write32(dataUpperBound);
			write32(semaID);
			write32(mpeg);
		}

		public virtual int FreePackets
		{
			get
			{
				return packets - PacketsInRingbuffer;
			}
		}

		public virtual void addPackets(int packetsAdded)
		{
			videoBuffer.notifyWrite(packetsAdded * packetSize);
			audioBuffer.notifyWrite(packetsAdded * packetSize);
			userDataBuffer.notifyWrite(packetsAdded * packetSize);
			packetsRead += packetsAdded;
			packetsWritten += packetsAdded;
			packetsInRingbuffer += packetsAdded;
			internalPacketsInRingbuffer += packetsAdded;

			writePacketOffset += packetsAdded;
			if (writePacketOffset >= packets)
			{
				// Wrap around
				writePacketOffset -= packets;
			}
		}

		public virtual void consumeAllPackets()
		{
			videoBuffer.notifyReadAll();
			audioBuffer.notifyReadAll();
			userDataBuffer.notifyReadAll();
			packetsInRingbuffer = 0;
			internalPacketsInRingbuffer = 0;
		}

		public virtual int PacketsInRingbuffer
		{
			get
			{
				return packetsInRingbuffer;
			}
		}

		public virtual bool Empty
		{
			get
			{
				return PacketsInRingbuffer == 0;
			}
		}

		public virtual int ReadPackets
		{
			get
			{
				return packetsRead;
			}
			set
			{
				this.packetsRead = value;
			}
		}

		public virtual bool hasReadPackets()
		{
			return packetsRead != 0;
		}


		public virtual int TotalPackets
		{
			get
			{
				return packets;
			}
		}

		public virtual int PacketSize
		{
			get
			{
				return packetSize;
			}
		}

		public virtual int PutDataAddr
		{
			get
			{
				return data + writePacketOffset * packetSize;
			}
		}

		public virtual int PutSequentialPackets
		{
			get
			{
				return System.Math.Min(FreePackets, packets - writePacketOffset);
			}
		}

		public virtual int getTmpAddress(int Length)
		{
			return dataUpperBound - Length;
		}

		public virtual int Mpeg
		{
			set
			{
				this.mpeg = value;
			}
		}

		public virtual int UpperDataAddr
		{
			get
			{
				return dataUpperBound;
			}
		}

		public virtual int CallbackAddr
		{
			get
			{
				return callbackAddr;
			}
		}

		public virtual int CallbackArgs
		{
			get
			{
				return callbackArgs;
			}
		}

		public virtual pspFileBuffer AudioBuffer
		{
			get
			{
				return audioBuffer;
			}
		}

		public virtual pspFileBuffer VideoBuffer
		{
			get
			{
				return videoBuffer;
			}
		}

		public virtual pspFileBuffer UserDataBuffer
		{
			get
			{
				return userDataBuffer;
			}
		}

		public virtual void notifyRead(int pendingImages)
		{
			if (packetSize == 0)
			{
				return;
			}

			int remainingLength = 0;
			if (hasAudio())
			{
				remainingLength = System.Math.Max(remainingLength, audioBuffer.CurrentSize);
			}
			if (hasVideo())
			{
				int videoBufferLength = videoBuffer.CurrentSize;
				// Do not empty completely the ringbuffer when we still have pending images
				if (pendingImages > 1)
				{
					videoBufferLength = System.Math.Max(videoBufferLength, 1);
				}
				remainingLength = System.Math.Max(remainingLength, videoBufferLength);
			}
			if (hasUserData())
			{
				remainingLength = System.Math.Max(remainingLength, userDataBuffer.CurrentSize);
			}
			int remainingPackets = (remainingLength + packetSize - 1) / packetSize;

			if (internalPacketsInRingbuffer > remainingPackets)
			{
				internalPacketsInRingbuffer = remainingPackets;
			}
		}

		public virtual void notifyConsumed()
		{
			packetsInRingbuffer = internalPacketsInRingbuffer;
		}

		public virtual bool hasAudio()
		{
			return hasAudio_Renamed;
		}

		public virtual bool HasAudio
		{
			set
			{
				this.hasAudio_Renamed = value;
			}
		}

		public virtual bool hasVideo()
		{
			return hasVideo_Renamed;
		}

		public virtual bool HasVideo
		{
			set
			{
				this.hasVideo_Renamed = value;
			}
		}

		public virtual bool hasUserData()
		{
			return hasUserData_Renamed;
		}

		public virtual bool HasUserData
		{
			set
			{
				this.hasUserData_Renamed = value;
			}
		}

		public override int @sizeof()
		{
			return 44;
		}

		public override string ToString()
		{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("SceMpegRingbuffer(packets=0x%X, packetsRead=0x%X, packetsWritten=0x%X, packetsFree=0x%X, packetSize=0x%X, hasVideo=%b, videoBuffer=%s, hasAudio=%b, audioBuffer=%s)", packets, packetsRead, packetsWritten, getFreePackets(), packetSize, hasVideo, videoBuffer, hasAudio, audioBuffer);
			return string.Format("SceMpegRingbuffer(packets=0x%X, packetsRead=0x%X, packetsWritten=0x%X, packetsFree=0x%X, packetSize=0x%X, hasVideo=%b, videoBuffer=%s, hasAudio=%b, audioBuffer=%s)", packets, packetsRead, packetsWritten, FreePackets, packetSize, hasVideo_Renamed, videoBuffer, hasAudio_Renamed, audioBuffer);
		}
	}

}