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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceMp4.TRACK_TYPE_AUDIO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceMp4.TRACK_TYPE_VIDEO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.alignUp;

	public class SceMp4TrackSampleBuf : pspAbstractMemoryMappedStructure
	{
		public int mp4;
		public int baseBufferAddr;
		public int samplesPut;
		public int sampleSize;
		public int unknown;
		public int currentSample; // Incremented at each sceMp4TrackSamplePutBuf by the number of samples put
		public int timeScale;
		public int duration;
		public int totalNumberSamples;
		public int trackType;
		public int readBufferAddr;
		public int readBufferSize;
		public long currentFileOffset;
		public int sizeAvailableInReadBuffer;
		public int bytesBufferAddr;
		public int bytesBufferLength;
		public SceMp4TrackSampleBufInfo bufBytes;
		public SceMp4TrackSampleBufInfo bufSamples;

		public class SceMp4TrackSampleBufInfo : pspAbstractMemoryMappedStructure
		{
			public int totalSize;
			public int readOffset;
			public int writeOffset;
			public int sizeAvailableForRead;
			public int unknown16;
			public int bufferAddr;
			public int callback24;
			public int unknown28;
			public int unknown36;

			protected internal override void read()
			{
				totalSize = read32();
				readOffset = read32();
				writeOffset = read32();
				sizeAvailableForRead = read32();
				unknown16 = read32();
				bufferAddr = read32();
				callback24 = read32();
				unknown28 = read32();
				readUnknown(4);
				unknown36 = read32();
			}

			protected internal override void write()
			{
				write32(totalSize);
				write32(readOffset);
				write32(writeOffset);
				write32(sizeAvailableForRead);
				write32(unknown16);
				write32(bufferAddr);
				write32(callback24);
				write32(unknown28);
				writeUnknown(4);
				write32(unknown36);
			}

			public virtual int WritableSpace
			{
				get
				{
					return totalSize - sizeAvailableForRead;
				}
			}

			public virtual void notifyRead(int Length)
			{
				Length = System.Math.Min(Length, sizeAvailableForRead);
				if (Length > 0)
				{
					readOffset += Length;
					if (readOffset >= totalSize)
					{
						readOffset -= totalSize;
					}
					sizeAvailableForRead -= Length;
				}
			}

			public virtual void flush()
			{
				readOffset = 0;
				writeOffset = 0;
				sizeAvailableForRead = 0;
			}

			public override int @sizeof()
			{
				return 40;
			}

			public override string ToString()
			{
				return string.Format("SceMp4TrackSampleBufInfo[totalSize=0x{0:X}, readOffset=0x{1:X}, writeOffset=0x{2:X}, sizeAvailableForRead=0x{3:X}, bufferAddr=0x{4:X8}]", totalSize, readOffset, writeOffset, sizeAvailableForRead, bufferAddr);
			}
		}

		protected internal override void read()
		{
			readUnknown(36);
			currentSample = read32(); // Offset 36
			timeScale = read32(); // Offset 40
			duration = read32(); // Offset 44
			totalNumberSamples = read32(); // Offset 48
			read32(); // Offset 52
			read32(); // Offset 56
			trackType = read32(); // Offset 60
			readUnknown(4);
			baseBufferAddr = read32(); // Offset 68
			samplesPut = read32(); // Offset 72
			sampleSize = read32(); // Offset 76
			unknown = read32(); // Offset 80
			bytesBufferAddr = read32(); // Offset 84
			bytesBufferLength = read32(); // Offset 88
			read32(); // Offset 92
			read32(); // Offset 96
			bufBytes = new SceMp4TrackSampleBufInfo(); // Offset 100
			read(bufBytes);
			bufSamples = new SceMp4TrackSampleBufInfo(); // Offset 140
			read(bufSamples);
			read32(); // Offset 180
			currentFileOffset = read64(); // Offset 184
			read32(); // Offset 192
			read32(); // Offset 196
			read32(); // Offset 200
			read32(); // Offset 204
			read32(); // Offset 208
			read32(); // Offset 212
			read32(); // Offset 216
			read32(); // Offset 220
			readBufferAddr = read32(); // Offset 224
			readBufferSize = read32(); // Offset 228
			sizeAvailableInReadBuffer = read32(); // Offset 232
			read32(); // Offset 236
		}

		protected internal override void write()
		{
			writeUnknown(36);
			write32(currentSample); // Offset 36
			write32(timeScale); // Offset 40
			write32(duration); // Offset 44
			write32(totalNumberSamples); // Offset 48
			write32(BaseAddress + 240); // Offset 52
			write32(BaseAddress + 72); // Offset 56
			write32(trackType); // Offset 60
			writeUnknown(4);
			write32(baseBufferAddr); // Offset 68
			write32(samplesPut); // Offset 72
			write32(sampleSize); // Offset 76
			write32(unknown); // Offset 80
			write32(bytesBufferAddr); // Offset 84
			write32(bytesBufferLength); // Offset 88
			write32(alignUp(baseBufferAddr, 63)); // Offset 92
			write32(samplesPut << 6); // Offset 96
			write(bufBytes); // Offset 100
			write(bufSamples); // Offset 140
			write32(BaseAddress + 184); // Offset 180
			write64(currentFileOffset); // Offset 184
			write32(0); // Offset 192 (callback address in libmp4 module?)
			write32(0); // Offset 196
			write32(0); // Offset 200
			write32(0); // Offset 204
			write32(mp4); // Offset 208
			write32(0); // Offset 212
			write32(0); // Offset 216
			write32(0); // Offset 220
			write32(readBufferAddr); // Offset 224
			write32(readBufferSize); // Offset 228
			write32(sizeAvailableInReadBuffer); // Offset 232
			write32(1); // Offset 236
		}

		public virtual bool isOfType(int trackType)
		{
			int mask = TRACK_TYPE_VIDEO | TRACK_TYPE_AUDIO;

			return (this.trackType & mask) == (trackType & mask);
		}

		public virtual bool isInReadBuffer(int offset)
		{
			return offset >= currentFileOffset && offset < currentFileOffset + sizeAvailableInReadBuffer;
		}

		private void addBytesToTrackSequential(int addr, int Length)
		{
			if (Length > 0)
			{
				mem.memcpy(bufBytes.bufferAddr + bufBytes.writeOffset, addr, Length);
				bufBytes.writeOffset += Length;
				bufBytes.sizeAvailableForRead += Length;
			}
		}

		public virtual void addBytesToTrack(int addr, int Length)
		{
			int length1 = System.Math.Min(Length, bufBytes.totalSize - bufBytes.writeOffset);
			addBytesToTrackSequential(addr, length1);

			int length2 = Length - length1;
			if (length2 > 0)
			{
				bufBytes.writeOffset = 0;
				addBytesToTrackSequential(addr + length1, length2);
			}
		}

		public virtual void addSamplesToTrack(int samples)
		{
			bufSamples.sizeAvailableForRead += samples;
			currentSample += samples;
		}

		public virtual void readBytes(int addr, int Length)
		{
			Length = System.Math.Min(Length, bufBytes.sizeAvailableForRead);
			if (Length > 0)
			{
				int length1 = System.Math.Min(Length, bufBytes.totalSize - bufBytes.readOffset);
				mem.memcpy(addr, bufBytes.bufferAddr + bufBytes.readOffset, length1);

				int length2 = Length - length1;
				if (length2 > 0)
				{
					mem.memcpy(addr + length1, bufBytes.bufferAddr, length2);
				}

				bufBytes.notifyRead(Length);
			}
		}

		public override int @sizeof()
		{
			return 240;
		}

		public override string ToString()
		{
			return string.Format("SceMp4TrackSampleBuf currentSample=0x{0:X}, timeScale=0x{1:X}, duration=0x{2:X}, totalNumberSamples=0x{3:X}, trackType=0x{4:X}, baseBufferAddr=0x{5:X8}, numSamples=0x{6:X}, sampleSize=0x{7:X}, unknown=0x{8:X}, readBufferAddr=0x{9:X8}, readBufferSize=0x{10:X}, currentFileOffset=0x{11:X}, sizeAvailableInReadBuffer=0x{12:X}, bufBytes={13}, bufSamples={14}", currentSample, timeScale, duration, totalNumberSamples, trackType, baseBufferAddr, samplesPut, sampleSize, unknown, readBufferAddr, readBufferSize, currentFileOffset, sizeAvailableInReadBuffer, bufBytes, bufSamples);
		}
	}

}