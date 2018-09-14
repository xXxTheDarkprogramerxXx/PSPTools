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
namespace pspsharp.HLE.modules
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelThreadInfo.PSP_WAIT_MUTEX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_SEEK_SET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceAudiocodec.PSP_CODEC_AAC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceMpeg.mpegTimestampPerSecond;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.alignUp;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.endianSwap32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.getReturnValue64;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUnaligned32;


	using IAction = pspsharp.HLE.kernel.types.IAction;
	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using SceMp4SampleInfo = pspsharp.HLE.kernel.types.SceMp4SampleInfo;
	using SceMp4TrackSampleBuf = pspsharp.HLE.kernel.types.SceMp4TrackSampleBuf;
	using SceMp4TrackSampleBufInfo = pspsharp.HLE.kernel.types.SceMp4TrackSampleBuf.SceMp4TrackSampleBufInfo;
	using SceMpegAu = pspsharp.HLE.kernel.types.SceMpegAu;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using CodecFactory = pspsharp.media.codec.CodecFactory;
	using ICodec = pspsharp.media.codec.ICodec;
	using H264Utils = pspsharp.media.codec.h264.H264Utils;
	using Utilities = pspsharp.util.Utilities;

	using Logger = org.apache.log4j.Logger;

	public class sceMp4 : HLEModule
	{
		public static Logger log = Modules.getLogger("sceMp4");
		protected internal int callbackParam;
		protected internal int callbackGetCurrentPosition;
		protected internal int callbackSeek;
		protected internal int callbackRead;
		protected internal int readBufferAddr;
		protected internal int readBufferSize;
		// Values for video track
		protected internal int[] videoSamplesOffset;
		protected internal int[] videoSamplesSize;
		protected internal int[] videoSamplesDuration;
		protected internal int[] videoSamplesPresentationOffset;
		protected internal int[] videoSyncSamples;
		protected internal int videoDuration;
		protected internal int videoTimeScale;
		protected internal long videoCurrentTimestamp;
		// Values for audio track
		protected internal int[] audioSamplesOffset;
		protected internal int[] audioSamplesSize;
		protected internal int[] audioSamplesDuration;
		protected internal int[] audioSamplesPresentationOffset;
		protected internal int[] audioSyncSamples;
		protected internal int audioDuration;
		protected internal int audioTimeScale;
		protected internal long audioCurrentTimestamp;

		protected internal int[] numberOfSamplesPerChunk;
		protected internal int[] samplesSize;
		protected internal long parseOffset;
		protected internal int timeScale;
		protected internal int duration;
		protected internal int numberOfTracks;
		protected internal int trackType;
		protected internal int[] currentAtomContent;
		protected internal int currentAtom;
		protected internal int currentAtomSize;
		protected internal int currentAtomOffset;
		protected internal int trackTimeScale;
		protected internal int trackDuration;
		protected internal int[] videoCodecExtraData;
		protected internal SceMp4TrackSampleBuf currentTrack;
		protected internal TPointer currentTracAddr;
		protected internal bool bufferPutInProgress;
		protected internal int bufferPutSamples;
		protected internal int bufferPutCurrentSampleRemainingBytes;
		protected internal int bufferPutSamplesPut;
		protected internal SceKernelThreadInfo bufferPutThread;
		protected internal ICodec audioCodec;
		protected internal int audioChannels;
		protected internal IList<int> threadsWaitingOnBufferPut;

		public const int TRACK_TYPE_VIDEO = 0x10;
		public const int TRACK_TYPE_AUDIO = 0x20;

		protected internal const int SEARCH_BACKWARDS = 0;
		protected internal const int SEARCH_FORWARDS = 1;

		protected internal const int ATOM_FTYP = 0x66747970; // "ftyp"
		protected internal const int ATOM_MOOV = 0x6D6F6F76; // "moov"
		protected internal const int ATOM_TRAK = 0x7472616B; // "trak"
		protected internal const int ATOM_MDIA = 0x6D646961; // "mdia"
		protected internal const int ATOM_MINF = 0x6D696E66; // "minf"
		protected internal const int ATOM_STBL = 0x7374626C; // "stbl"
		protected internal const int ATOM_STSD = 0x73747364; // "stsd"
		protected internal const int ATOM_MVHD = 0x6D766864; // "mvhd"
		protected internal const int ATOM_STSC = 0x73747363; // "stsc"
		protected internal const int ATOM_STSZ = 0x7374737A; // "stsz"
		protected internal const int ATOM_STTS = 0x73747473; // "stts"
		protected internal const int ATOM_CTTS = 0x63747473; // "ctts"
		protected internal const int ATOM_STCO = 0x7374636F; // "stco"
		protected internal const int ATOM_STSS = 0x73747373; // "stss"
		protected internal const int ATOM_MDHD = 0x6D646864; // "mdhd"
		protected internal const int ATOM_AVCC = 0x61766343; // "avcC"
		protected internal const int FILE_TYPE_MSNV = 0x4D534E56; // "MSNV"
		protected internal const int FILE_TYPE_ISOM = 0x69736F6D; // "isom"
		protected internal const int FILE_TYPE_MP42 = 0x6D703432; // "mp42"
		protected internal const int DATA_FORMAT_AVC1 = 0x61766331; // "avc1"
		protected internal const int DATA_FORMAT_MP4A = 0x6D703461; // "mp4a"

		private static bool isContainerAtom(int atom)
		{
			switch (atom)
			{
				case ATOM_MOOV:
				case ATOM_TRAK:
				case ATOM_MDIA:
				case ATOM_MINF:
				case ATOM_STBL:
					return true;
			}

			return false;
		}

		private static bool isAtomContentRequired(int atom)
		{
			switch (atom)
			{
				case ATOM_MVHD:
				case ATOM_STSD:
				case ATOM_STSC:
				case ATOM_STSZ:
				case ATOM_STTS:
				case ATOM_CTTS:
				case ATOM_STCO:
				case ATOM_STSS:
				case ATOM_MDHD:
				case ATOM_AVCC:
					return true;
			}

			return false;
		}

		private static string atomToString(int atom)
		{
			return string.Format("{0}{1}{2}{3}", (char)((int)((uint)atom >> 24)), (char)((atom >> 16) & 0xFF), (char)((atom >> 8) & 0xFF), (char)(atom & 0xFF));
		}

		private static int read32(Memory mem, int addr)
		{
			return endianSwap32(readUnaligned32(mem, addr));
		}

		private static int read32(int[] content, int o)
		{
			return (content[o] << 24) | (content[o + 1] << 16) | (content[o + 2] << 8) | content[o + 3];
		}

		private static int read16(int[] content, int o)
		{
			return (content[o] << 8) | content[o + 1];
		}

		private static int[] extend(int[] array, int length)
		{
			if (length > 0)
			{
				if (array == null)
				{
					array = new int[length];
				}
				else if (array.Length < length)
				{
					int[] newArray = new int[length];
					Array.Copy(array, 0, newArray, 0, array.Length);
					array = newArray;
				}
			}

			return array;
		}

		private void processAtom(Memory mem, int addr, int atom, int size)
		{
			int[] content = new int[size];
			for (int i = 0; i < size; i++, addr++)
			{
				content[i] = mem.read8(addr);
			}

			processAtom(atom, content, size);
		}

		private void setTrackDurationAndTimeScale()
		{
			if (trackType == 0)
			{
				return;
			}

			if (currentTrack != null && currentTracAddr != null && currentTrack.isOfType(trackType))
			{
				currentTrack.timeScale = trackTimeScale;
				currentTrack.duration = trackDuration;
				currentTrack.write(currentTracAddr);
			}

			switch (trackType)
			{
				case TRACK_TYPE_VIDEO:
					videoTimeScale = trackTimeScale;
					videoDuration = trackDuration;
					break;
				case TRACK_TYPE_AUDIO:
					audioTimeScale = trackTimeScale;
					audioDuration = trackDuration;
					break;
				default:
					log.error(string.Format("processAtom 'mdhd' unknown track type {0:D}", trackType));
					break;
			}
		}

		private void processAtom(int atom, int[] content, int size)
		{
			switch (atom)
			{
				case ATOM_MVHD:
					if (size >= 20)
					{
						timeScale = read32(content, 12);
						duration = read32(content, 16);
					}
					break;
				case ATOM_MDHD:
					if (size >= 20)
					{
						trackTimeScale = read32(content, 12);
						trackDuration = read32(content, 16);

						setTrackDurationAndTimeScale();
					}
					break;
				case ATOM_STSD:
					if (size >= 16)
					{
						int dataFormat = read32(content, 12);
						switch (dataFormat)
						{
							case DATA_FORMAT_AVC1:
								if (log.DebugEnabled)
								{
									log.debug(string.Format("trackType video {0}", atomToString(dataFormat)));
								}
								trackType = TRACK_TYPE_VIDEO;

								if (size >= 44)
								{
									int videoFrameWidth = read16(content, 40);
									int videoFrameHeight = read16(content, 42);
									if (log.DebugEnabled)
									{
										log.debug(string.Format("Video frame size {0:D}x{1:D}", videoFrameWidth, videoFrameHeight));
									}

									Modules.sceMpegModule.VideoFrameHeight = videoFrameHeight;
								}
								if (size >= 102)
								{
									int atomAvcC = read32(content, 98);
									int atomAvcCsize = read32(content, 94);
									if (atomAvcC == ATOM_AVCC && atomAvcCsize <= size - 94)
									{
										videoCodecExtraData = new int[atomAvcCsize - 8];
										Array.Copy(content, 102, videoCodecExtraData, 0, videoCodecExtraData.Length);
										Modules.sceMpegModule.VideoCodecExtraData = videoCodecExtraData;
									}
								}
								break;
							case DATA_FORMAT_MP4A:
								if (log.DebugEnabled)
								{
									log.debug(string.Format("trackType audio {0}", atomToString(dataFormat)));
								}
								trackType = TRACK_TYPE_AUDIO;

								if (size >= 34)
								{
									audioChannels = read16(content, 32);
								}
								break;
							default:
								log.warn(string.Format("Unknown track type 0x{0:X8}({1})", dataFormat, atomToString(dataFormat)));
								break;
						}

						setTrackDurationAndTimeScale();
					}
					break;
				case ATOM_STSC:
				{
					numberOfSamplesPerChunk = null;
					if (size >= 8)
					{
						int numberOfEntries = read32(content, 4);
						if (size >= numberOfEntries * 12 + 8)
						{
							int offset = 8;
							int previousChunk = 1;
							int previousSamplesPerChunk = 0;
							for (int i = 0; i < numberOfEntries; i++, offset += 12)
							{
								int firstChunk = read32(content, offset);
								int samplesPerChunk = read32(content, offset + 4);
								numberOfSamplesPerChunk = extend(numberOfSamplesPerChunk, firstChunk);
								for (int j = previousChunk; j < firstChunk; j++)
								{
									numberOfSamplesPerChunk[j - 1] = previousSamplesPerChunk;
								}
								previousChunk = firstChunk;
								previousSamplesPerChunk = samplesPerChunk;
							}
							numberOfSamplesPerChunk = extend(numberOfSamplesPerChunk, previousChunk);
							numberOfSamplesPerChunk[previousChunk - 1] = previousSamplesPerChunk;
						}
					}
					break;
				}
				case ATOM_STSZ:
				{
					samplesSize = null;
					if (size >= 8)
					{
						int sampleSize = read32(content, 4);
						if (sampleSize > 0)
						{
							samplesSize = new int[1];
							samplesSize[0] = sampleSize;
						}
						else if (size >= 12)
						{
							int numberOfEntries = read32(content, 8);
							samplesSize = new int[numberOfEntries];
							int offset = 12;
							for (int i = 0; i < numberOfEntries; i++, offset += 4)
							{
								samplesSize[i] = read32(content, offset);
							}
						}
					}

					switch (trackType)
					{
						case TRACK_TYPE_VIDEO:
							videoSamplesSize = samplesSize;
							break;
						case TRACK_TYPE_AUDIO:
							audioSamplesSize = samplesSize;
							break;
						default:
							log.error(string.Format("processAtom 'stsz' unknown track type {0:D}", trackType));
							break;
					}
					break;
				}
				case ATOM_STTS:
				{
					int[] samplesDuration = null;
					if (size >= 8)
					{
						int numberOfEntries = read32(content, 4);
						int offset = 8;
						int sample = 0;
						for (int i = 0; i < numberOfEntries; i++, offset += 8)
						{
							int sampleCount = read32(content, offset);
							int sampleDuration = read32(content, offset + 4);
							samplesDuration = extend(samplesDuration, sample + sampleCount);
							Arrays.fill(samplesDuration, sample, sample + sampleCount, sampleDuration);
							sample += sampleCount;
						}
					}

					switch (trackType)
					{
						case TRACK_TYPE_VIDEO:
							videoSamplesDuration = samplesDuration;
							break;
						case TRACK_TYPE_AUDIO:
							audioSamplesDuration = samplesDuration;
							break;
						default:
							log.error(string.Format("processAtom 'stts' unknown track type {0:D}", trackType));
							break;
					}
					break;
				}
				case ATOM_CTTS:
				{
					int[] samplesPresentationOffset = null;
					if (size >= 8)
					{
						int numberOfEntries = read32(content, 4);
						int offset = 8;
						int sample = 0;
						for (int i = 0; i < numberOfEntries; i++, offset += 8)
						{
							int sampleCount = read32(content, offset);
							int samplePresentationOffset = read32(content, offset + 4);
							samplesPresentationOffset = extend(samplesPresentationOffset, sample + sampleCount);
							Arrays.fill(samplesPresentationOffset, sample, sample + sampleCount, samplePresentationOffset);
							sample += sampleCount;
						}
					}

					switch (trackType)
					{
						case TRACK_TYPE_VIDEO:
							videoSamplesPresentationOffset = samplesPresentationOffset;
							break;
						case TRACK_TYPE_AUDIO:
							audioSamplesPresentationOffset = samplesPresentationOffset;
							break;
						default:
							log.error(string.Format("processAtom 'ctts' unknown track type {0:D}", trackType));
							break;
					}
					break;
				}
				case ATOM_STCO:
				{
					int[] chunksOffset = null;
					if (size >= 8)
					{
						int numberOfEntries = read32(content, 4);
						chunksOffset = extend(chunksOffset, numberOfEntries);
						int offset = 8;
						for (int i = 0; i < numberOfEntries; i++, offset += 4)
						{
							chunksOffset[i] = read32(content, offset);
						}
					}

					int[] samplesOffset = null;
					if (numberOfSamplesPerChunk != null && samplesSize != null && chunksOffset != null)
					{
						// numberOfSamplesPerChunk could be shorter if the last chunks all have the same length.
						// Extend numberOfSamplesPerChunk by repeating the size of the last chunk.
						int compactedChunksLength = numberOfSamplesPerChunk.Length;
						numberOfSamplesPerChunk = extend(numberOfSamplesPerChunk, chunksOffset.Length);
						Arrays.fill(numberOfSamplesPerChunk, compactedChunksLength, chunksOffset.Length, numberOfSamplesPerChunk[compactedChunksLength - 1]);

						// Compute the total number of samples
						int numberOfSamples = 0;
						for (int i = 0; i < numberOfSamplesPerChunk.Length; i++)
						{
							numberOfSamples += numberOfSamplesPerChunk[i];
						}

						// samplesSize could be shorter than the number of samples.
						// Extend samplesSize by repeating the size of the last sample.
						int compactedSamplesLength = samplesSize.Length;
						samplesSize = extend(samplesSize, numberOfSamples);
						Arrays.fill(samplesSize, compactedSamplesLength, numberOfSamples, samplesSize[compactedSamplesLength - 1]);

						samplesOffset = new int[numberOfSamples];
						int sample = 0;
						for (int i = 0; i < chunksOffset.Length; i++)
						{
							int offset = chunksOffset[i];
							for (int j = 0; j < numberOfSamplesPerChunk[i]; j++, sample++)
							{
								samplesOffset[sample] = offset;
								offset += samplesSize[sample];
							}
						}

						if (log.TraceEnabled)
						{
							for (int i = 0; i < samplesOffset.Length; i++)
							{
								log.trace(string.Format("Sample#{0:D} offset=0x{1:X}, size=0x{2:X}", i, samplesOffset[i], samplesSize[i]));
							}
						}

						if (currentTrack != null && currentTracAddr != null && currentTrack.isOfType(trackType))
						{
							currentTrack.totalNumberSamples = numberOfSamples;
							currentTrack.write(currentTracAddr);
						}
					}

					switch (trackType)
					{
						case TRACK_TYPE_VIDEO:
							videoSamplesOffset = samplesOffset;
							break;
						case TRACK_TYPE_AUDIO:
							audioSamplesOffset = samplesOffset;
							break;
						default:
							log.error(string.Format("processAtom 'stco' unknown track type {0:D}", trackType));
							break;
					}
					break;
				}
				case ATOM_STSS:
				{
					int[] syncSamples = null;
					if (size >= 8)
					{
						int numberOfEntries = read32(content, 4);
						syncSamples = new int[numberOfEntries];
						int offset = 8;
						for (int i = 0; i < numberOfEntries; i++, offset += 4)
						{
							syncSamples[i] = read32(content, offset) - 1; // Sync samples are numbered starting at 1
							if (log.TraceEnabled)
							{
								log.trace(string.Format("Sync sample#{0:D}=0x{1:X}", i, syncSamples[i]));
							}
						}
					}

					switch (trackType)
					{
						case TRACK_TYPE_VIDEO:
							videoSyncSamples = syncSamples;
							break;
						case TRACK_TYPE_AUDIO:
							audioSyncSamples = syncSamples;
							break;
						default:
							log.error(string.Format("processAtom 'stss' unknown track type {0:D}", trackType));
							break;
					}
					break;
				}
			}
		}

		private void processAtom(int atom)
		{
			switch (atom)
			{
				case ATOM_TRAK:
					// We start a new track.
					trackType = 0;
					numberOfSamplesPerChunk = null;
					samplesSize = null;
					numberOfTracks++;
					break;
			}
		}

		private void addCurrentAtomContent(Memory mem, int addr, int size)
		{
			for (int i = 0; i < size; i++)
			{
				currentAtomContent[currentAtomOffset++] = mem.read8(addr++);
			}
		}

		private void parseAtoms(Memory mem, int addr, int size)
		{
			int offset = 0;

			if (currentAtom != 0)
			{
				int length = System.Math.Min(size, currentAtomSize - currentAtomOffset);
				addCurrentAtomContent(mem, addr, length);
				offset += length;

				if (currentAtomOffset >= currentAtomSize)
				{
					processAtom(currentAtom, currentAtomContent, currentAtomSize);
					currentAtom = 0;
					currentAtomContent = null;
				}
			}

			while (offset + 8 <= size)
			{
				int atomSize = read32(mem, addr + offset);
				int atom = read32(mem, addr + offset + 4);

				if (log.DebugEnabled)
				{
					log.debug(string.Format("parseAtoms atom=0x{0:X8}({1}), size=0x{2:X}, offset=0x{3:X}", atom, atomToString(atom), atomSize, parseOffset + offset));
				}

				if (atomSize <= 0)
				{
					break;
				}

				if (isAtomContentRequired(atom))
				{
					if (offset + atomSize <= size)
					{
						processAtom(mem, addr + offset + 8, atom, atomSize - 8);
					}
					else
					{
						currentAtom = atom;
						currentAtomSize = atomSize - 8;
						currentAtomOffset = 0;
						currentAtomContent = new int[currentAtomSize];
						addCurrentAtomContent(mem, addr + offset + 8, size - offset - 8);
						atomSize = size - offset;
					}
				}
				else
				{
					// Process an atom without content
					processAtom(atom);
				}

				if (isContainerAtom(atom))
				{
					offset += 8;
				}
				else
				{
					offset += atomSize;
				}
			}

			parseOffset += offset;
		}

		private class AfterReadHeadersRead : IAction
		{
			private readonly sceMp4 outerInstance;

			public AfterReadHeadersRead(sceMp4 outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.afterReadHeadersRead(Emulator.Processor.cpu._v0);
			}
		}

		private class AfterReadHeadersSeek : IAction
		{
			private readonly sceMp4 outerInstance;

			public AfterReadHeadersSeek(sceMp4 outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.afterReadHeadersSeek(getReturnValue64(Emulator.Processor.cpu));
			}
		}

		private void afterReadHeadersRead(int readSize)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("afterReadHeadersRead: {0}", Utilities.getMemoryDump(readBufferAddr, readSize)));
			}

			Memory mem = Memory.Instance;
			if (parseOffset == 0L && readSize >= 12)
			{
				int header1 = read32(mem, readBufferAddr);
				int header2 = read32(mem, readBufferAddr + 4);
				int header3 = read32(mem, readBufferAddr + 8);
				if (header1 < 12 || header2 != ATOM_FTYP || (header3 != FILE_TYPE_MSNV && header3 != FILE_TYPE_ISOM && header3 != FILE_TYPE_MP42))
				{
					log.warn(string.Format("Invalid MP4 file header 0x{0:X8} 0x{1:X8} 0x{2:X8}: {3}", header1, header2, header3, Utilities.getMemoryDump(readBufferAddr, System.Math.Min(16, readSize))));
					readSize = 0;
				}
			}

			parseAtoms(mem, readBufferAddr, readSize);

			// Continue reading?
			if (readSize > 0)
			{
				// Seek to the next atom
				callSeekCallback(null, new AfterReadHeadersSeek(this), parseOffset, PSP_SEEK_SET);
			}
			else
			{
				if (log.TraceEnabled && currentTrack != null)
				{
					log.trace(string.Format("afterReadHeadersRead updated track {0}", currentTrack));
				}
				currentTrack = null;
				currentTracAddr = null;

				Modules.sceMpegModule.VideoFrameSizes = videoSamplesSize;
			}
		}

		private void afterReadHeadersSeek(long seek)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("afterReadHeadersSeek seek=0x{0:X}", seek));
			}

			callReadCallback(null, new AfterReadHeadersRead(this), readBufferAddr, readBufferSize);
		}

		protected internal virtual void readHeaders(SceMp4TrackSampleBuf track, TPointer trackAddr)
		{
			if (videoSamplesOffset != null && videoSamplesSize != null)
			{
				return;
			}

			parseOffset = 0L;
			duration = 0;
			currentAtom = 0;
			numberOfTracks = 0;
			currentTrack = track;
			currentTracAddr = trackAddr;

			// Start reading all the atoms.
			// First seek to the beginning of the file.
			callSeekCallback(null, new AfterReadHeadersSeek(this), parseOffset, PSP_SEEK_SET);
		}

		protected internal virtual int getSampleOffset(int sample)
		{
			return getSampleOffset(currentTrack.trackType, sample);
		}

		protected internal virtual int getSampleOffset(int trackType, int sample)
		{
			if ((trackType & TRACK_TYPE_AUDIO) != 0)
			{
				if (audioSamplesOffset == null || sample < 0 || sample >= audioSamplesOffset.Length)
				{
					return -1;
				}
				return audioSamplesOffset[sample];
			}
			if ((trackType & TRACK_TYPE_VIDEO) != 0)
			{
				if (videoSamplesOffset == null || sample < 0 || sample >= videoSamplesOffset.Length)
				{
					return -1;
				}
				return videoSamplesOffset[sample];
			}

			log.error(string.Format("getSampleOffset unknown trackType=0x{0:X}", trackType));

			return -1;
		}

		protected internal virtual int getSampleSize(int sample)
		{
			return getSampleSize(currentTrack.trackType, sample);
		}

		protected internal virtual int getSampleSize(int trackType, int sample)
		{
			if ((trackType & TRACK_TYPE_AUDIO) != 0)
			{
				if (audioSamplesSize == null || sample < 0 || sample >= audioSamplesSize.Length)
				{
					return -1;
				}
				return audioSamplesSize[sample];
			}
			if ((trackType & TRACK_TYPE_VIDEO) != 0)
			{
				if (videoSamplesSize == null || sample < 0 || sample >= videoSamplesSize.Length)
				{
					return -1;
				}
				return videoSamplesSize[sample];
			}

			log.error(string.Format("getSampleSize unknown trackType=0x{0:X}", trackType));

			return -1;
		}

		protected internal virtual int getSampleDuration(int sample)
		{
			return getSampleDuration(currentTrack.trackType, sample);
		}

		protected internal virtual int getSampleDuration(int trackType, int sample)
		{
			if ((trackType & TRACK_TYPE_AUDIO) != 0)
			{
				if (audioSamplesDuration == null || sample < 0 || sample >= audioSamplesDuration.Length)
				{
					return -1;
				}
				return audioSamplesDuration[sample];
			}
			if ((trackType & TRACK_TYPE_VIDEO) != 0)
			{
				if (videoSamplesDuration == null || sample < 0 || sample >= videoSamplesDuration.Length)
				{
					return -1;
				}
				return videoSamplesDuration[sample];
			}

			log.error(string.Format("getSampleDuration unknown trackType=0x{0:X}", trackType));

			return -1;
		}

		protected internal virtual int getSamplePresentationOffset(int sample)
		{
			return getSamplePresentationOffset(currentTrack.trackType, sample);
		}

		protected internal virtual int getSamplePresentationOffset(int trackType, int sample)
		{
			if ((trackType & TRACK_TYPE_AUDIO) != 0)
			{
				if (audioSamplesPresentationOffset == null || sample < 0 || sample >= audioSamplesPresentationOffset.Length)
				{
					return 0;
				}
				return audioSamplesPresentationOffset[sample];
			}
			if ((trackType & TRACK_TYPE_VIDEO) != 0)
			{
				if (videoSamplesPresentationOffset == null || sample < 0 || sample >= videoSamplesPresentationOffset.Length)
				{
					return 0;
				}
				return videoSamplesPresentationOffset[sample];
			}

			log.error(string.Format("getSamplePresentationOffset unknown trackType=0x{0:X}", trackType));

			return 0;
		}

		private class AfterBufferPutSeek : IAction
		{
			private readonly sceMp4 outerInstance;

			public AfterBufferPutSeek(sceMp4 outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.afterBufferPutSeek(getReturnValue64(Emulator.Processor.cpu));
			}
		}

		private class AfterBufferPutRead : IAction
		{
			private readonly sceMp4 outerInstance;

			public AfterBufferPutRead(sceMp4 outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.afterBufferPutRead(Emulator.Processor.cpu._v0);
			}
		}

		private void afterBufferPutSeek(long seek)
		{
			currentTrack.currentFileOffset = seek;
			callReadCallback(bufferPutThread, new AfterBufferPutRead(this), currentTrack.readBufferAddr, currentTrack.readBufferSize);
		}

		private void afterBufferPutRead(int size)
		{
			currentTrack.sizeAvailableInReadBuffer = size;
			bufferPut();
		}

		private void bufferPut(long seek)
		{
			// PSP is always reading in multiples of readBufferSize
			seek = Utilities.alignDown(seek, currentTrack.readBufferSize - 1);

			callSeekCallback(bufferPutThread, new AfterBufferPutSeek(this), seek, PSP_SEEK_SET);
		}

		private void addBytesToTrack(Memory mem, int addr, int length)
		{
			if (log.TraceEnabled)
			{
				log.trace(string.Format("addBytesToTrack addr=0x{0:X8}, length=0x{1:X}: {2}", addr, length, Utilities.getMemoryDump(addr, length)));
			}

			currentTrack.addBytesToTrack(addr, length);

			if (currentTrack.isOfType(TRACK_TYPE_VIDEO))
			{
				Modules.sceMpegModule.addToVideoBuffer(mem, addr, length);
			}
		}

		private void bufferPut()
		{
			Memory mem = Memory.Instance;
			while (bufferPutSamples > 0)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("bufferPut samples=0x{0:X}, remainingBytes=0x{1:X}, currentTrack={2}", bufferPutSamples, bufferPutCurrentSampleRemainingBytes, currentTrack));
				}

				if (bufferPutCurrentSampleRemainingBytes > 0)
				{
					int length = System.Math.Min(currentTrack.readBufferSize, bufferPutCurrentSampleRemainingBytes);
					addBytesToTrack(mem, currentTrack.readBufferAddr, length);
					bufferPutCurrentSampleRemainingBytes -= length;

					if (bufferPutCurrentSampleRemainingBytes > 0)
					{
						bufferPut(currentTrack.currentFileOffset + currentTrack.readBufferSize);
						break;
					}

					currentTrack.addSamplesToTrack(1);
					bufferPutSamples--;
					bufferPutSamplesPut++;
				}
				else
				{
					// Read one sample
					int sample = currentTrack.currentSample;
					int sampleOffset = getSampleOffset(sample);
					int sampleSize = getSampleSize(sample);

					if (log.TraceEnabled)
					{
						log.trace(string.Format("bufferPut sample=0x{0:X}, offset=0x{1:X}, size=0x{2:X}, currentFilePosition=0x{3:X}, readSize=0x{4:X}", sample, sampleOffset, sampleSize, currentTrack.currentFileOffset, currentTrack.sizeAvailableInReadBuffer));
					}

					if (sampleOffset < 0)
					{
						if (log.TraceEnabled)
						{
							log.trace(string.Format("bufferPut reached last frame at sample 0x{0:X}, stopping", sample));
						}
						bufferPutSamples = 0;
						break;
					}

					if (sampleSize > currentTrack.bufBytes.WritableSpace)
					{
						if (log.TraceEnabled)
						{
							log.trace(string.Format("bufferPut bufBytes full (remaining 0x{0:X} bytes, sample size=0x{1:X}), stopping", currentTrack.bufBytes.WritableSpace, sampleSize));
						}
						bufferPutSamples = 0;
						break;
					}

					if (currentTrack.isInReadBuffer(sampleOffset))
					{
						int sampleReadBufferOffset = (int)(sampleOffset - currentTrack.currentFileOffset);
						int sampleAddr = currentTrack.readBufferAddr + sampleReadBufferOffset;
						if (currentTrack.isInReadBuffer(sampleOffset + sampleSize))
						{
							// Sample completely available in the read buffer
							addBytesToTrack(mem, sampleAddr, sampleSize);
							currentTrack.addSamplesToTrack(1);

							bufferPutSamples--;
							bufferPutSamplesPut++;
						}
						else
						{
							// Sample partially available in the read buffer
							int availableSampleLength = currentTrack.sizeAvailableInReadBuffer - sampleReadBufferOffset;
							addBytesToTrack(mem, sampleAddr, availableSampleLength);
							bufferPutCurrentSampleRemainingBytes = sampleSize - availableSampleLength;

							bufferPut(currentTrack.currentFileOffset + currentTrack.readBufferSize);
							break;
						}
					}
					else
					{
						bufferPut(sampleOffset);
						break;
					}
				}
			}

			if (bufferPutSamples <= 0 && bufferPutInProgress)
			{
				// sceMp4TrackSampleBufPut is now completed, write the current track back to memory...
				currentTrack.write(currentTracAddr);

				// ... and return the number of samples put
				if (log.TraceEnabled)
				{
					log.trace(string.Format("bufferPut returning 0x{0:X} for thread {1}", bufferPutSamplesPut, bufferPutThread));
				}
				bufferPutThread.cpuContext._v0 = bufferPutSamplesPut;

				Modules.sceMpegModule.hleMpegNotifyVideoDecoderThread();

				bufferPutInProgress = false;

				if (threadsWaitingOnBufferPut.Count > 0)
				{
					int threadUid = threadsWaitingOnBufferPut.RemoveAt(0).intValue();
					if (log.TraceEnabled)
					{
						log.trace(string.Format("bufferPut unblocking thread {0}", Modules.ThreadManForUserModule.getThreadById(threadUid)));
					}
					Modules.ThreadManForUserModule.hleUnblockThread(threadUid);
				}
			}
		}

		private class StartBufferPut : IAction
		{
			private readonly sceMp4 outerInstance;

			internal SceMp4TrackSampleBuf track;
			internal TPointer trackAddr;
			internal int samples;
			internal SceKernelThreadInfo thread;

			public StartBufferPut(sceMp4 outerInstance, SceMp4TrackSampleBuf track, TPointer trackAddr, int samples, SceKernelThreadInfo thread)
			{
				this.outerInstance = outerInstance;
				this.track = track;
				this.trackAddr = trackAddr;
				this.samples = samples;
				this.thread = thread;
			}

			public virtual void execute()
			{
				outerInstance.bufferPut(thread, track, trackAddr, samples);
			}
		}

		private class BufferPutUnblock : IAction
		{
			private readonly sceMp4 outerInstance;

			internal SceMp4TrackSampleBuf track;
			internal TPointer trackAddr;
			internal int samples;
			internal SceKernelThreadInfo thread;

			public BufferPutUnblock(sceMp4 outerInstance, SceMp4TrackSampleBuf track, TPointer trackAddr, int samples, SceKernelThreadInfo thread)
			{
				this.outerInstance = outerInstance;
				this.track = track;
				this.trackAddr = trackAddr;
				this.samples = samples;
				this.thread = thread;
			}

			public virtual void execute()
			{
				// Start bufferPut in the thread context when it will be scheduled
				Modules.ThreadManForUserModule.pushActionForThread(thread, new StartBufferPut(outerInstance, track, trackAddr, samples, thread));
			}
		}

		private class BufferPutWaitStateChecker : IWaitStateChecker
		{
			private readonly sceMp4 outerInstance;

			public BufferPutWaitStateChecker(sceMp4 outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				if (log.TraceEnabled)
				{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: log.trace(String.format("BufferPutWaitStateChecker.continueWaitState for thread %s returning %b", thread, threadsWaitingOnBufferPut.contains(thread.uid)));
					log.trace(string.Format("BufferPutWaitStateChecker.continueWaitState for thread %s returning %b", thread, outerInstance.threadsWaitingOnBufferPut.Contains(thread.uid)));
				}

				if (outerInstance.threadsWaitingOnBufferPut.Contains(thread.uid))
				{
					return true;
				}

				return false;
			}
		}

		protected internal virtual void bufferPut(SceKernelThreadInfo thread, SceMp4TrackSampleBuf track, TPointer trackAddr, int samples)
		{
			if (bufferPutInProgress)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("bufferPut blocking thread {0}", thread));
				}
				BufferPutUnblock bufferPutUnblock = new BufferPutUnblock(this, track, trackAddr, samples, thread);
				threadsWaitingOnBufferPut.Add(thread.uid);
				Modules.ThreadManForUserModule.hleBlockThread(thread, PSP_WAIT_MUTEX, 0, false, bufferPutUnblock, new BufferPutWaitStateChecker(this));
			}
			else
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("bufferPut starting samples=0x{0:X}, thread={1}", samples, thread));
				}
				bufferPutInProgress = true;
				currentTrack = track;
				currentTracAddr = trackAddr;
				bufferPutSamples = samples;
				bufferPutCurrentSampleRemainingBytes = 0;
				bufferPutSamplesPut = 0;
				bufferPutThread = thread;

				bufferPut();
			}
		}

		protected internal virtual void callReadCallback(SceKernelThreadInfo thread, IAction afterAction, int readAddr, int readBytes)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("callReadCallback readAddr=0x{0:X8}, readBytes=0x{1:X}", readAddr, readBytes));
			}
			Modules.ThreadManForUserModule.executeCallback(thread, callbackRead, afterAction, false, callbackParam, readAddr, readBytes);
		}

		protected internal virtual void callGetCurrentPositionCallback(SceKernelThreadInfo thread, IAction afterAction)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("callGetCurrentPositionCallback"));
			}
			Modules.ThreadManForUserModule.executeCallback(thread, callbackGetCurrentPosition, afterAction, false, callbackParam);
		}

		protected internal virtual void callSeekCallback(SceKernelThreadInfo thread, IAction afterAction, long offset, int whence)
		{
			if (log.DebugEnabled)
			{
				log.debug(string.Format("callSeekCallback offset=0x{0:X}, whence={1}", offset, IoFileMgrForUser.getWhenceName(whence)));
			}
			Modules.ThreadManForUserModule.executeCallback(thread, callbackSeek, afterAction, false, callbackParam, 0, unchecked((int)(offset & 0xFFFFFFFF)), (int)((long)((ulong)offset >> 32)), whence);
		}

		protected internal virtual void hleMp4Init()
		{
			readBufferAddr = 0;
			readBufferSize = 0;
			videoSamplesOffset = null;
			videoSamplesSize = null;
			videoSamplesDuration = null;
			videoSamplesPresentationOffset = null;
			videoCurrentTimestamp = 0L;
			audioSamplesOffset = null;
			audioSamplesSize = null;
			audioSamplesDuration = null;
			audioSamplesPresentationOffset = null;
			audioCurrentTimestamp = 0L;
			trackType = 0;
			threadsWaitingOnBufferPut = new LinkedList<int>();
			bufferPutInProgress = false;

			// TODO MP4 videos seem to decode with no alpha... or does it depend on the movie data?
			H264Utils.Alpha = 0x00;
		}

		protected internal virtual void readCallbacks(TPointer32 callbacks)
		{
			callbackParam = callbacks.getValue(0);
			callbackGetCurrentPosition = callbacks.getValue(4);
			callbackSeek = callbacks.getValue(8);
			callbackRead = callbacks.getValue(12);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMp4 callbacks: param=0x{0:X8}, getCurrentPosition=0x{1:X8}, seek=0x{2:X8}, read=0x{3:X8}", callbackParam, callbackGetCurrentPosition, callbackSeek, callbackRead));
			}
		}

		protected internal virtual long sampleToFrameDuration(long sampleDuration, SceMp4TrackSampleBuf track)
		{
			return sampleToFrameDuration(sampleDuration, track.timeScale);
		}

		protected internal virtual long sampleToFrameDuration(long sampleDuration, int timeScale)
		{
			if (timeScale == 0)
			{
				return sampleDuration;
			}
			return sampleDuration * mpegTimestampPerSecond / timeScale;
		}

		protected internal virtual long getTotalFrameDuration(SceMp4TrackSampleBuf track)
		{
			long totalSampleDuration = 0L;
			for (int sample = 0; sample < track.totalNumberSamples; sample++)
			{
				int sampleDuration = getSampleDuration(track.trackType, sample);
				totalSampleDuration += sampleDuration;
			}

			long totalFrameDuration = sampleToFrameDuration(totalSampleDuration, track);

			return totalFrameDuration;
		}

		[HLEFunction(nid : 0x68651CBC, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp4Init(bool unk1, bool unk2)
		{
			hleMp4Init();

			return 0;
		}

		[HLEFunction(nid : 0x9042B257, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp4Finish()
		{
			videoSamplesOffset = null;
			videoSamplesSize = null;
			audioSamplesOffset = null;
			audioSamplesSize = null;
			currentTrack = null;
			currentTracAddr = null;

			return 0;
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0xB1221EE7, version : 150, checkInsideInterrupt : true)]
		public virtual int sceMp4Create(int mp4, TPointer32 callbacks, TPointer readBufferAddr, int readBufferSize)
		{
			this.readBufferAddr = readBufferAddr.Address;
			this.readBufferSize = readBufferSize;

			Modules.sceMpegModule.hleCreateRingbuffer();

			readCallbacks(callbacks);

			readHeaders(null, null);

			return 0;
		}

		[HLEFunction(nid : 0x538C2057, version : 150)]
		public virtual int sceMp4Delete()
		{
			// Reset default alpha
			H264Utils.Alpha = 0xFF;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x113E9E7B, version = 150) public int sceMp4GetNumberOfMetaData(int mp4)
		[HLEFunction(nid : 0x113E9E7B, version : 150)]
		public virtual int sceMp4GetNumberOfMetaData(int mp4)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7443AF1D, version = 150) public int sceMp4GetMovieInfo(int mp4, @CanBeNull pspsharp.HLE.TPointer32 movieInfo)
		[HLEFunction(nid : 0x7443AF1D, version : 150)]
		public virtual int sceMp4GetMovieInfo(int mp4, TPointer32 movieInfo)
		{
			movieInfo.setValue(0, numberOfTracks);
			movieInfo.setValue(4, 0); // Always 0
			movieInfo.setValue(8, (int) sampleToFrameDuration(duration, timeScale));

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMp4GetMovieInfo returning numberOfTracks={0:D}, duration=0x{1:X}", movieInfo.getValue(0), movieInfo.getValue(8)));
			}

			return 0;
		}

		[HLEFunction(nid : 0x5EB65F26, version : 150)]
		public virtual int sceMp4GetNumberOfSpecificTrack(int mp4, int trackType)
		{
			if ((trackType & TRACK_TYPE_VIDEO) != 0)
			{
				return videoSamplesOffset != null ? 1 : 0;
			}
			if ((trackType & TRACK_TYPE_AUDIO) != 0)
			{
				return audioSamplesOffset != null ? 1 : 0;
			}

			log.warn(string.Format("sceMp4GetNumberOfSpecificTrack unknown trackType={0:X}", trackType));

			return 0;
		}

		[HLEFunction(nid : 0x7ADFD01C, version : 150)]
		public virtual int sceMp4RegistTrack(int mp4, int trackType, int unknown, TPointer32 callbacks, TPointer trackAddr)
		{
			SceMp4TrackSampleBuf track = new SceMp4TrackSampleBuf();
			track.read(trackAddr);

			track.currentSample = 0;
			track.trackType = trackType;

			if ((trackType & TRACK_TYPE_VIDEO) != 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMp4RegistTrack TRACK_TYPE_VIDEO"));
				}
				track.timeScale = videoTimeScale;
				track.duration = videoDuration;
				track.totalNumberSamples = videoSamplesSize != null ? videoSamplesSize.Length : 0;
			}

			if ((trackType & TRACK_TYPE_AUDIO) != 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMp4RegistTrack TRACK_TYPE_AUDIO"));
				}
				track.timeScale = audioTimeScale;
				track.duration = audioDuration;
				track.totalNumberSamples = audioSamplesSize != null ? audioSamplesSize.Length : 0;
			}

			readCallbacks(callbacks);

			track.write(trackAddr);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4RegistTrack track {0}", track));
			}

			readHeaders(track, trackAddr);

			return 0;
		}

		[HLEFunction(nid : 0xBCA9389C, version : 150)]
		public virtual int sceMp4TrackSampleBufQueryMemSize(int trackType, int numSamples, int sampleSize, int unknown, int readBufferSize)
		{
			int value = System.Math.Max(numSamples * sampleSize, unknown << 1) + (numSamples << 6) + readBufferSize + 256;
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMp4TrackSampleBufQueryMemSize returning 0x{0:X}", value));
			}

			return value;
		}

		[HLEFunction(nid : 0x9C8F4FC1, version : 150)]
		public virtual int sceMp4TrackSampleBufConstruct(int mp4, TPointer trackAddr, TPointer buffer, int sampleBufQueyMemSize, int numSamples, int sampleSize, int unknown, int readBufferSize)
		{
			// sampleBufQueyMemSize is the value returned by sceMp4TrackSampleBufQueryMemSize

			SceMp4TrackSampleBuf track = new SceMp4TrackSampleBuf();
			track.read(trackAddr);

			track.mp4 = mp4;
			track.baseBufferAddr = buffer.Address;
			track.samplesPut = numSamples;
			track.sampleSize = sampleSize;
			track.unknown = unknown;
			track.bytesBufferAddr = alignUp(buffer.Address, 63) + (numSamples << 6);
			track.bytesBufferLength = System.Math.Max(numSamples * sampleSize, unknown << 1);
			track.readBufferSize = readBufferSize;
			track.readBufferAddr = track.bytesBufferAddr + track.bytesBufferLength + 48;
			track.currentFileOffset = -1;
			track.sizeAvailableInReadBuffer = 0;

			track.bufBytes = new SceMp4TrackSampleBuf.SceMp4TrackSampleBufInfo();
			track.bufBytes.totalSize = track.bytesBufferLength;
			track.bufBytes.readOffset = 0;
			track.bufBytes.writeOffset = 0;
			track.bufBytes.sizeAvailableForRead = 0;
			track.bufBytes.unknown16 = 1;
			track.bufBytes.bufferAddr = track.bytesBufferAddr;
			track.bufBytes.callback24 = 0;
			track.bufBytes.unknown28 = trackAddr.Address + 184;
			track.bufBytes.unknown36 = mp4;

			track.bufSamples = new SceMp4TrackSampleBuf.SceMp4TrackSampleBufInfo();
			track.bufSamples.totalSize = numSamples;
			track.bufSamples.readOffset = 0;
			track.bufSamples.writeOffset = 0;
			track.bufSamples.sizeAvailableForRead = 0;
			track.bufSamples.unknown16 = 64;
			track.bufSamples.bufferAddr = alignUp(buffer.Address, 63);
			track.bufSamples.callback24 = 0;
			track.bufSamples.unknown28 = 0;
			track.bufSamples.unknown36 = mp4;

			track.write(trackAddr);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4TrackSampleBufConstruct track {0}", track));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x0F0187D2, version = 150) public int sceMp4GetAvcTrackInfoData(int mp4, pspsharp.HLE.TPointer trackAddr, @CanBeNull pspsharp.HLE.TPointer32 infoAddr)
		[HLEFunction(nid : 0x0F0187D2, version : 150)]
		public virtual int sceMp4GetAvcTrackInfoData(int mp4, TPointer trackAddr, TPointer32 infoAddr)
		{
			SceMp4TrackSampleBuf track = new SceMp4TrackSampleBuf();
			track.read(trackAddr);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4GetAvcTrackInfoData track {0}", track));
			}

			long totalFrameDuration = getTotalFrameDuration(track);

			// Returning 3 32-bit values in infoAddr
			infoAddr.setValue(0, 0); // Always 0
			infoAddr.setValue(4, (int) totalFrameDuration);
			infoAddr.setValue(8, track.totalNumberSamples);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4GetAvcTrackInfoData returning info:{0}", Utilities.getMemoryDump(infoAddr.Address, 12)));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x9CE6F5CF, version = 150) public int sceMp4GetAacTrackInfoData(int mp4, pspsharp.HLE.TPointer trackAddr, @CanBeNull pspsharp.HLE.TPointer32 infoAddr)
		[HLEFunction(nid : 0x9CE6F5CF, version : 150)]
		public virtual int sceMp4GetAacTrackInfoData(int mp4, TPointer trackAddr, TPointer32 infoAddr)
		{
			SceMp4TrackSampleBuf track = new SceMp4TrackSampleBuf();
			track.read(trackAddr);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4GetAacTrackInfoData track {0}", track));
			}

			long totalFrameDuration = getTotalFrameDuration(track);

			// Returning 5 32-bit values in infoAddr
			infoAddr.setValue(0, 0); // Always 0
			infoAddr.setValue(4, (int) totalFrameDuration);
			infoAddr.setValue(8, track.totalNumberSamples);
			infoAddr.setValue(12, track.timeScale);
			infoAddr.setValue(16, audioChannels);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4GetAacTrackInfoData returning info:{0}", Utilities.getMemoryDump(infoAddr.Address, 20)));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4ED4AB1E, version = 150) public int sceMp4AacDecodeInitResource(int unknown)
		[HLEFunction(nid : 0x4ED4AB1E, version : 150)]
		public virtual int sceMp4AacDecodeInitResource(int unknown)
		{
			return 0;
		}

		[HLEFunction(nid : 0x10EE0D2C, version : 150)]
		public virtual int sceMp4AacDecodeInit(TPointer32 aac)
		{
			aac.setValue(0); // Always 0?

			audioCodec = CodecFactory.getCodec(PSP_CODEC_AAC);

			int channels = 2; // Always stereo?
			audioCodec.init(0, channels, channels, 0);

			return 0;
		}

		[HLEFunction(nid : 0x496E8A65, version : 150)]
		public virtual int sceMp4TrackSampleBufFlush(int mp4, TPointer trackAddr)
		{
			SceMp4TrackSampleBuf track = new SceMp4TrackSampleBuf();
			track.read(trackAddr);

			track.bufBytes.flush();
			track.bufSamples.flush();

			track.write(trackAddr);

			if (track.isOfType(TRACK_TYPE_VIDEO))
			{
				Modules.sceMpegModule.flushVideoFrameData();
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB4B400D1, version = 150) public int sceMp4GetSampleNumWithTimeStamp(int mp4, pspsharp.HLE.TPointer trackAddr, pspsharp.HLE.TPointer32 timestampAddr)
		[HLEFunction(nid : 0xB4B400D1, version : 150)]
		public virtual int sceMp4GetSampleNumWithTimeStamp(int mp4, TPointer trackAddr, TPointer32 timestampAddr)
		{
			SceMp4TrackSampleBuf track = new SceMp4TrackSampleBuf();
			track.read(trackAddr);

			// Only value at offset 4 is used
			int timestamp = timestampAddr.getValue(4);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4GetSampleNumWithTimeStamp timestamp=0x{0:X}, track {1}", timestamp, track));
			}

			int sample = track.currentSample;

			return sample;
		}

		[HLEFunction(nid : 0xF7C51EC1, version : 150)]
		public virtual int sceMp4GetSampleInfo(int mp4, TPointer trackAddr, int sample, TPointer infoAddr)
		{
			SceMp4TrackSampleBuf track = new SceMp4TrackSampleBuf();
			track.read(trackAddr);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4GetSampleInfo track {0}", track));
			}

			if (sample == -1)
			{
				sample = track.currentSample;
			}

			SceMp4SampleInfo info = new SceMp4SampleInfo();
			int sampleDuration = getSampleDuration(track.trackType, sample);
			long frameDuration = sampleToFrameDuration(sampleDuration, track);

			info.sample = sample;
			info.sampleOffset = getSampleOffset(track.trackType, sample);
			info.sampleSize = getSampleSize(track.trackType, sample);
			info.unknown1 = 0;
			info.frameDuration = (int) frameDuration;
			info.unknown2 = 0;
			info.timestamp1 = sample * info.frameDuration;
			info.timestamp2 = sample * info.frameDuration;

			info.write(infoAddr);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4GetSampleInfo returning info={0}", info));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x74A1CA3E, version = 150) public int sceMp4SearchSyncSampleNum(int mp4, pspsharp.HLE.TPointer trackAddr, int searchDirection, int sample)
		[HLEFunction(nid : 0x74A1CA3E, version : 150)]
		public virtual int sceMp4SearchSyncSampleNum(int mp4, TPointer trackAddr, int searchDirection, int sample)
		{
			if (searchDirection != SEARCH_BACKWARDS && searchDirection != SEARCH_FORWARDS)
			{
				return SceKernelErrors.ERROR_MP4_INVALID_VALUE;
			}

			SceMp4TrackSampleBuf track = new SceMp4TrackSampleBuf();
			track.read(trackAddr);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4SearchSyncSampleNum track {0}", track));
			}

			int[] syncSamples;
			if (track.isOfType(TRACK_TYPE_AUDIO))
			{
				syncSamples = audioSyncSamples;
			}
			else if (track.isOfType(TRACK_TYPE_VIDEO))
			{
				syncSamples = videoSyncSamples;
			}
			else
			{
				log.error(string.Format("sceMp4SearchSyncSampleNum unknown track type 0x{0:X}", track.trackType));
				return -1;
			}

			int syncSample = 0;
			if (syncSamples != null)
			{
				for (int i = 0; i < syncSamples.Length; i++)
				{
					if (sample > syncSamples[i])
					{
						syncSample = syncSamples[i];
					}
					else if (sample == syncSamples[i] && searchDirection == SEARCH_FORWARDS)
					{
						syncSample = syncSamples[i];
					}
					else
					{
						if (searchDirection == SEARCH_FORWARDS)
						{
							syncSample = syncSamples[i];
						}
						break;
					}
				}
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMp4SearchSyncSampleNum returning 0x{0:X}", syncSample));
			}

			return syncSample;
		}

		[HLEFunction(nid : 0xD8250B75, version : 150)]
		public virtual int sceMp4PutSampleNum(int mp4, TPointer trackAddr, int sample)
		{
			SceMp4TrackSampleBuf track = new SceMp4TrackSampleBuf();
			track.read(trackAddr);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4PutSampleNum track {0}", track));
			}

			if (sample < 0 || sample >= track.totalNumberSamples)
			{
				return SceKernelErrors.ERROR_MP4_INVALID_SAMPLE_NUMBER;
			}

			track.currentSample = sample;
			track.write(trackAddr);

			if (track.isOfType(TRACK_TYPE_VIDEO))
			{
				Modules.sceMpegModule.VideoFrame = sample;
			}

			return 0;
		}

		/// <summary>
		/// Similar to sceMpegRingbufferAvailableSize.
		/// </summary>
		/// <param name="mp4"> </param>
		/// <param name="trackAddr"> </param>
		/// <param name="writableSamplesAddr"> </param>
		/// <param name="writableBytesAddr">
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x8754ECB8, version = 150) public int sceMp4TrackSampleBufAvailableSize(int mp4, pspsharp.HLE.TPointer trackAddr, @CanBeNull pspsharp.HLE.TPointer32 writableSamplesAddr, @CanBeNull pspsharp.HLE.TPointer32 writableBytesAddr)
		[HLEFunction(nid : 0x8754ECB8, version : 150)]
		public virtual int sceMp4TrackSampleBufAvailableSize(int mp4, TPointer trackAddr, TPointer32 writableSamplesAddr, TPointer32 writableBytesAddr)
		{
			SceMp4TrackSampleBuf track = new SceMp4TrackSampleBuf();
			track.read(trackAddr);
			writableSamplesAddr.setValue(track.bufSamples.WritableSpace);
			writableBytesAddr.setValue(track.bufBytes.WritableSpace);

			int result = 0;
			if (writableSamplesAddr.getValue() < 0 || writableBytesAddr.getValue() < 0)
			{
				result = SceKernelErrors.ERROR_MP4_NO_AVAILABLE_SIZE;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMp4TrackSampleBufAvailableSize returning writableSamples=0x{0:X}, writableBytes=0x{1:X}, result=0x{2:X8}", writableSamplesAddr.getValue(), writableBytesAddr.getValue(), result));
			}

			return result;
		}

		/// <summary>
		/// Similar to sceMpegRingbufferPut.
		/// </summary>
		/// <param name="mp4"> </param>
		/// <param name="track"> </param>
		/// <param name="samples">
		/// @return </param>
		[HLEFunction(nid : 0x31BCD7E0, version : 150)]
		public virtual int sceMp4TrackSampleBufPut(int mp4, TPointer trackAddr, int samples)
		{
			SceMp4TrackSampleBuf track = new SceMp4TrackSampleBuf();
			track.read(trackAddr);

			readHeaders(track, trackAddr);

			if (samples > 0)
			{
				// Start bufferPut in the thread context when it will be scheduled
				SceKernelThreadInfo currentThread = Modules.ThreadManForUserModule.CurrentThread;
				Modules.ThreadManForUserModule.pushActionForThread(currentThread, new StartBufferPut(this, track, trackAddr, samples, currentThread));
			}

			return 0;
		}

		/// <summary>
		/// Similar to sceMpegGetAtracAu.
		/// </summary>
		/// <param name="mp4"> </param>
		/// <param name="trackAddr"> </param>
		/// <param name="auAddr"> </param>
		/// <param name="infoAddr">
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x5601A6F0, version = 150) public int sceMp4GetAacAu(int mp4, pspsharp.HLE.TPointer trackAddr, pspsharp.HLE.TPointer auAddr, @CanBeNull pspsharp.HLE.TPointer infoAddr)
		[HLEFunction(nid : 0x5601A6F0, version : 150)]
		public virtual int sceMp4GetAacAu(int mp4, TPointer trackAddr, TPointer auAddr, TPointer infoAddr)
		{
			SceMp4TrackSampleBuf track = new SceMp4TrackSampleBuf();
			track.read(trackAddr);

			SceMpegAu au = new SceMpegAu();
			au.read(auAddr);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4GetAacAu track {0}, au {1}", track, au));
			}

			if (track.bufSamples.sizeAvailableForRead <= 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMp4GetAacAu returning ERROR_MP4_NO_MORE_DATA"));
				}
				return SceKernelErrors.ERROR_MP4_NO_MORE_DATA;
			}

			int sample = track.currentSample - track.bufSamples.sizeAvailableForRead;
			int sampleSize = getSampleSize(track.trackType, sample);
			int sampleDuration = getSampleDuration(track.trackType, sample);
			int samplePresentationOffset = getSamplePresentationOffset(track.trackType, sample);
			long frameDuration = sampleToFrameDuration(sampleDuration, track);
			long framePresentationOffset = sampleToFrameDuration(samplePresentationOffset, track);

			// Consume one frame
			track.bufSamples.notifyRead(1);
			track.readBytes(au.esBuffer, sampleSize);
			au.esSize = sampleSize;
			au.dts = audioCurrentTimestamp;
			audioCurrentTimestamp += frameDuration;
			au.pts = au.dts + framePresentationOffset;

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4GetAacAu consuming one frame of size=0x{0:X}, duration=0x{1:X}, track {2}", sampleSize, frameDuration, track));
			}

			au.write(auAddr);
			track.write(trackAddr);

			if (infoAddr.NotNull)
			{
				SceMp4SampleInfo info = new SceMp4SampleInfo();

				info.sample = sample;
				info.sampleOffset = getSampleOffset(track.trackType, sample);
				info.sampleSize = getSampleSize(track.trackType, sample);
				info.unknown1 = 0;
				info.frameDuration = (int) frameDuration;
				info.unknown2 = 0;
				info.timestamp1 = (int) au.dts;
				info.timestamp2 = (int) au.pts;

				info.write(infoAddr);

				if (log.TraceEnabled)
				{
					log.trace(string.Format("sceMp4GetAacAu returning info={0}", info));
				}
			}

			return 0;
		}

		/// <summary>
		/// Similar to sceMpegAtracDecode.
		/// </summary>
		/// <param name="aac"> </param>
		/// <param name="auAddr"> </param>
		/// <param name="outputBufferAddr"> </param>
		/// <param name="init">		1 at first call, 0 afterwards </param>
		/// <param name="frequency">	44100
		/// @return </param>
		[HLEFunction(nid : 0x7663CB5C, version : 150)]
		public virtual int sceMp4AacDecode(TPointer32 aac, TPointer auAddr, TPointer bufferAddr, int init, int frequency)
		{
			SceMpegAu au = new SceMpegAu();
			au.read(auAddr);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4AacDecode au={0}, esBuffer:{1}", au, Utilities.getMemoryDump(au.esBuffer, au.esSize)));
			}

			int result = audioCodec.decode(au.esBuffer, au.esSize, bufferAddr.Address);

			if (result < 0)
			{
				log.error(string.Format("sceMp4AacDecode audio codec returned 0x{0:X8}", result));
				result = SceKernelErrors.ERROR_MP4_AAC_DECODE_ERROR;
			}
			else
			{
				result = 0;
			}

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceMp4AacDecode returning 0x{0:X}", result));
			}

			return result;
		}

		/// <summary>
		/// Similar to sceMpegGetAvcAu.
		/// Video decoding is done by sceMpegAvcDecode.
		/// </summary>
		/// <param name="mp4"> </param>
		/// <param name="trackAddr"> </param>
		/// <param name="auAddr"> </param>
		/// <param name="infoAddr">
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x503A3CBA, version = 150) public int sceMp4GetAvcAu(int mp4, pspsharp.HLE.TPointer trackAddr, pspsharp.HLE.TPointer auAddr, @CanBeNull pspsharp.HLE.TPointer infoAddr)
		[HLEFunction(nid : 0x503A3CBA, version : 150)]
		public virtual int sceMp4GetAvcAu(int mp4, TPointer trackAddr, TPointer auAddr, TPointer infoAddr)
		{
			SceMp4TrackSampleBuf track = new SceMp4TrackSampleBuf();
			track.read(trackAddr);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4GetAvcAu track {0}", track));
			}

			if (track.bufSamples.sizeAvailableForRead <= 0)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceMp4GetAvcAu returning ERROR_MP4_NO_MORE_DATA"));
				}
				return SceKernelErrors.ERROR_MP4_NO_MORE_DATA;
			}

			SceMpegAu au = new SceMpegAu();
			au.read(auAddr);
			Modules.sceMpegModule.MpegAvcAu = au;

			int sample = track.currentSample - track.bufSamples.sizeAvailableForRead;
			int sampleSize = getSampleSize(track.trackType, sample);
			int sampleDuration = getSampleDuration(track.trackType, sample);
			int samplePresentationOffset = getSamplePresentationOffset(track.trackType, sample);
			long frameDuration = sampleToFrameDuration(sampleDuration, track);
			long framePresentationOffset = sampleToFrameDuration(samplePresentationOffset, track);

			// Consume one frame
			track.bufSamples.notifyRead(1);
			track.bufBytes.notifyRead(sampleSize);

			au.dts = videoCurrentTimestamp;
			videoCurrentTimestamp += frameDuration;
			au.pts = au.dts + framePresentationOffset;

			if (log.TraceEnabled)
			{
				log.trace(string.Format("sceMp4GetAvcAu consuming one frame of size=0x{0:X}, duration=0x{1:X}, track {2}", sampleSize, frameDuration, track));
			}

			au.write(auAddr);
			track.write(trackAddr);

			if (infoAddr.NotNull)
			{
				SceMp4SampleInfo info = new SceMp4SampleInfo();

				info.sample = sample;
				info.sampleOffset = getSampleOffset(track.trackType, sample);
				info.sampleSize = getSampleSize(track.trackType, sample);
				info.unknown1 = 0;
				info.frameDuration = (int) frameDuration;
				info.unknown2 = 0;
				info.timestamp1 = (int) au.dts;
				info.timestamp2 = (int) au.pts;

				info.write(infoAddr);

				if (log.TraceEnabled)
				{
					log.trace(string.Format("sceMp4GetAvcAu returning info={0}", info));
				}
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x01C76489, version = 150) public int sceMp4TrackSampleBufDestruct(int unknown1, int unknown2)
		[HLEFunction(nid : 0x01C76489, version : 150)]
		public virtual int sceMp4TrackSampleBufDestruct(int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6710FE77, version = 150) public int sceMp4UnregistTrack(int unknown1, int unknown2)
		[HLEFunction(nid : 0x6710FE77, version : 150)]
		public virtual int sceMp4UnregistTrack(int unknown1, int unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5D72B333, version = 150) public int sceMp4AacDecodeExit(int unknown)
		[HLEFunction(nid : 0x5D72B333, version : 150)]
		public virtual int sceMp4AacDecodeExit(int unknown)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7D332394, version = 150) public int sceMp4AacDecodeTermResource()
		[HLEFunction(nid : 0x7D332394, version : 150)]
		public virtual int sceMp4AacDecodeTermResource()
		{
			return 0;
		}

		[HLEFunction(nid : 0x131BDE57, version : 150)]
		public virtual int sceMp4InitAu(int mp4, TPointer bufferAddr, TPointer auAddr)
		{
			SceMpegAu au = new SceMpegAu();
			au.esBuffer = bufferAddr.Address;
			au.esSize = 0;
			au.write(auAddr);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x17EAA97D, version = 150) public int sceMp4GetAvcAuWithoutSampleBuf(int mp4)
		[HLEFunction(nid : 0x17EAA97D, version : 150)]
		public virtual int sceMp4GetAvcAuWithoutSampleBuf(int mp4)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x28CCB940, version = 150) public int sceMp4GetTrackEditList(int mp4)
		[HLEFunction(nid : 0x28CCB940, version : 150)]
		public virtual int sceMp4GetTrackEditList(int mp4)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3069C2B5, version = 150) public int sceMp4GetAvcParamSet(int mp4)
		[HLEFunction(nid : 0x3069C2B5, version : 150)]
		public virtual int sceMp4GetAvcParamSet(int mp4)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD2AC9A7E, version = 150) public int sceMp4GetMetaData(int mp4)
		[HLEFunction(nid : 0xD2AC9A7E, version : 150)]
		public virtual int sceMp4GetMetaData(int mp4)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x4FB5B756, version = 150) public int sceMp4GetMetaDataInfo(int mp4)
		[HLEFunction(nid : 0x4FB5B756, version : 150)]
		public virtual int sceMp4GetMetaDataInfo(int mp4)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x427BEF7F, version = 150) public int sceMp4GetTrackNumOfEditList(int mp4)
		[HLEFunction(nid : 0x427BEF7F, version : 150)]
		public virtual int sceMp4GetTrackNumOfEditList(int mp4)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x532029B8, version = 150) public int sceMp4GetAacAuWithoutSampleBuf(int mp4)
		[HLEFunction(nid : 0x532029B8, version : 150)]
		public virtual int sceMp4GetAacAuWithoutSampleBuf(int mp4)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA6C724DC, version = 150) public int sceMp4GetSampleNum(int mp4)
		[HLEFunction(nid : 0xA6C724DC, version : 150)]
		public virtual int sceMp4GetSampleNum(int mp4)
		{
			return 0;
		}
	}
}