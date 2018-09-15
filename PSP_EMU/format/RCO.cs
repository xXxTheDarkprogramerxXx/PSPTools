using System;
using System.Collections.Generic;
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
namespace pspsharp.format
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.endianSwap32;


	using UmdVideoPlayer = pspsharp.GUI.UmdVideoPlayer;
	using AnimFactory = pspsharp.format.rco.AnimFactory;
	using LZR = pspsharp.format.rco.LZR;
	using ObjectFactory = pspsharp.format.rco.ObjectFactory;
	using RCOContext = pspsharp.format.rco.RCOContext;
	using RCOState = pspsharp.format.rco.RCOState;
	using SoundFactory = pspsharp.format.rco.SoundFactory;
	using BaseObject = pspsharp.format.rco.@object.BaseObject;
	using ImageObject = pspsharp.format.rco.@object.ImageObject;
	using VSMX = pspsharp.format.rco.vsmx.VSMX;
	using VSMXBaseObject = pspsharp.format.rco.vsmx.interpreter.VSMXBaseObject;
	using VSMXInterpreter = pspsharp.format.rco.vsmx.interpreter.VSMXInterpreter;
	using Controller = pspsharp.format.rco.vsmx.objects.Controller;
	using GlobalVariables = pspsharp.format.rco.vsmx.objects.GlobalVariables;
	using MoviePlayer = pspsharp.format.rco.vsmx.objects.MoviePlayer;
	using Resource = pspsharp.format.rco.vsmx.objects.Resource;
	using Utilities = pspsharp.util.Utilities;

	//using Logger = org.apache.log4j.Logger;

	public class RCO
	{
		public static readonly Logger log = Logger.getLogger("rco");
		private const bool dumpImages = false;
		private const int RCO_HEADER_SIZE = 164;
		private const int RCO_MAGIC = 0x00505246;
		private const int RCO_NULL_PTR = unchecked((int)0xFFFFFFFF);
		public const int RCO_TABLE_MAIN = 1;
		public const int RCO_TABLE_VSMX = 2;
		public const int RCO_TABLE_TEXT = 3;
		public const int RCO_TABLE_IMG = 4;
		public const int RCO_TABLE_MODEL = 5;
		public const int RCO_TABLE_SOUND = 6;
		public const int RCO_TABLE_FONT = 7;
		public const int RCO_TABLE_OBJ = 8;
		public const int RCO_TABLE_ANIM = 9;
		public const int RCO_DATA_COMPRESSION_NONE = 0;
		public const int RCO_DATA_COMPRESSION_ZLIB = 1;
		public const int RCO_DATA_COMPRESSION_RLZ = 2;
		private static readonly Charset textDataCharset = Charset.forName("UTF-16LE");
		private sbyte[] buffer;
		private int offset;
		private bool valid;
		private int pVSMXTable;
		private int pTextData;
		private int lTextData;
		private int pLabelData;
		private int lLabelData;
		private int pImgData;
		private int lImgData;
		private RCOEntry mainTable;
		private int[] compressedTextDataOffset;
		private IDictionary<int, RCOEntry> entries;
		private IDictionary<int, string> events;
		private IDictionary<int, BufferedImage> images;
		private IDictionary<int, BaseObject> objects;

		public class RCOEntry
		{
			private readonly RCO outerInstance;

			public RCOEntry(RCO outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal const int RCO_ENTRY_SIZE = 40;
			public int type; // main table uses 0x01; may be used as a current entry depth value
			public int id;
			public int labelOffset;
			public string label;
			public int eHeadSize;
			public int entrySize;
			public int numSubEntries;
			public int nextEntryOffset;
			public int prevEntryOffset;
			public int parentTblOffset;
			public RCOEntry[] subEntries;
			public RCOEntry parent;
			public sbyte[] data;
			public BaseObject obj;
			public string[] texts;
			public VSMXBaseObject vsmxBaseObject;

			public virtual void read()
			{
				int entryOffset = outerInstance.tell();
				type = outerInstance.read8();
				id = outerInstance.read8();
				outerInstance.skip16();
				labelOffset = outerInstance.read32();
				eHeadSize = outerInstance.read32();
				entrySize = outerInstance.read32();
				numSubEntries = outerInstance.read32();
				nextEntryOffset = outerInstance.read32();
				prevEntryOffset = outerInstance.read32();
				parentTblOffset = outerInstance.read32();
				outerInstance.skip32();
				outerInstance.skip32();

				outerInstance.entries[entryOffset] = this;

				if (parentTblOffset != 0)
				{
					parent = outerInstance.entries[entryOffset - parentTblOffset];
				}

				if (labelOffset != RCO_NULL_PTR)
				{
					label = outerInstance.readLabel(labelOffset);
				}

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("RCO entry at offset 0x{0:X}: {1}", entryOffset, ToString()));
				}

				switch (id)
				{
					case RCO_TABLE_MAIN:
						if (type != 1)
						{
							Console.WriteLine(string.Format("Unknown RCO entry type 0x{0:X} at offset 0x{1:X}", type, entryOffset));
						}
						break;
					case RCO_TABLE_VSMX:
						if (type == 1)
						{
							int offsetVSMX = outerInstance.read32();
							int lengthVSMX = outerInstance.read32();
							outerInstance.skip(offsetVSMX);
							data = outerInstance.readBytes(lengthVSMX);
							// 4-bytes alignment
							outerInstance.skip(Utilities.alignUp(lengthVSMX, 3) - lengthVSMX);
						}
						else
						{
							Console.WriteLine(string.Format("Unknown RCO entry type 0x{0:X} at offset 0x{1:X}", type, entryOffset));
						}
						break;
					case RCO_TABLE_IMG:
					case RCO_TABLE_MODEL:
						if (type == 1)
						{
							int format = outerInstance.read16();
							int compression = outerInstance.read16();
							int sizePacked = outerInstance.read32();
							int offset = outerInstance.read32();
							int sizeUnpacked; // this value doesn't exist if entry isn't compressed
							if (compression != RCO_DATA_COMPRESSION_NONE)
							{
								sizeUnpacked = outerInstance.read32();
							}
							else
							{
								sizeUnpacked = sizePacked;
							}

							if (id == RCO_TABLE_IMG)
							{
								BufferedImage image = outerInstance.readImage(offset, sizePacked);
								if (image != null)
								{
									obj = new ImageObject(image);
									outerInstance.images[entryOffset] = image;
								}
							}

							//if (log.DebugEnabled)
							{
								Console.WriteLine(string.Format("RCO entry {0}: format={1:D}, compression={2:D}, sizePacked=0x{3:X}, offset=0x{4:X}, sizeUnpacked=0x{5:X}", id == RCO_TABLE_IMG ? "IMG" : "MODEL", format, compression, sizePacked, offset, sizeUnpacked));
							}
						}
						else if (type != 0)
						{
							Console.WriteLine(string.Format("Unknown RCO entry type 0x{0:X} at offset 0x{1:X}", type, entryOffset));
						}
						break;
					case RCO_TABLE_SOUND:
						if (type == 1)
						{
							int format = outerInstance.read16(); // 0x01 = VAG
							int channels = outerInstance.read16(); // 1 or 2 channels
							int sizeTotal = outerInstance.read32();
							int offset = outerInstance.read32();
							int[] channelSize = new int[channels];
							int[] channelOffset = new int[channels];
							// now pairs of size/offset for each channel
							//if (log.DebugEnabled)
							{
								Console.WriteLine(string.Format("RCO entry SOUND: format={0:D}, channels={1:D}, sizeTotal=0x{2:X}, offset=0x{3:X}", format, channels, sizeTotal, offset));
							}
							for (int channel = 0; channel < channels; channel++)
							{
								channelSize[channel] = outerInstance.read32();
								channelOffset[channel] = outerInstance.read32();
								//if (log.DebugEnabled)
								{
									Console.WriteLine(string.Format("Channel {0:D}: size=0x{1:X}, offset=0x{2:X}", channel, channelSize[channel], channelOffset[channel]));
								}
							}

							obj = SoundFactory.newSound(format, channels, channelSize, channelOffset);

							// there _must_ be two channels defined (no clear indication of size otherwise)
							if (channels < 2)
							{
								for (int i = channels; i < 2; i++)
								{
									int dummyChannelSize = outerInstance.read32();
									int dummyChannelOffset = outerInstance.read32();
									if (log.TraceEnabled)
									{
										log.trace(string.Format("Dummy channel {0:D}: size=0x{1:X}, offset=0x{2:X}", i, dummyChannelSize, dummyChannelOffset));
									}
								}
							}
						}
						else if (type != 0)
						{
							Console.WriteLine(string.Format("Unknown RCO entry type 0x{0:X} at offset 0x{1:X}", type, entryOffset));
						}
						break;
					case RCO_TABLE_OBJ:
						if (type > 0)
						{
							obj = ObjectFactory.newObject(type);

							if (obj != null && entrySize == 0)
							{
								entrySize = obj.size() + RCO_ENTRY_SIZE;
							}

							if (entrySize > RCO_ENTRY_SIZE)
							{
								int dataLength = entrySize - RCO_ENTRY_SIZE;
								data = outerInstance.readBytes(dataLength);
								if (log.TraceEnabled)
								{
									log.trace(string.Format("OBJ data at 0x{0:X}: {1}", entryOffset + RCO_ENTRY_SIZE, Utilities.getMemoryDump(data, 0, dataLength)));
								}

								if (obj != null)
								{
									RCOContext context = new RCOContext(data, 0, outerInstance.events, outerInstance.images, outerInstance.objects);
									obj.read(context);
									if (context.offset != dataLength)
									{
										Console.WriteLine(string.Format("Incorrect Length data for ANIM"));
									}

									outerInstance.objects[entryOffset] = obj;

									//if (log.DebugEnabled)
									{
										Console.WriteLine(string.Format("OBJ: {0}", obj));
									}
								}
							}
						}
						break;
					case RCO_TABLE_ANIM:
						if (type > 0)
						{
							obj = AnimFactory.newAnim(type);

							if (obj != null && entrySize == 0)
							{
								entrySize = obj.size() + RCO_ENTRY_SIZE;
							}

							if (entrySize > RCO_ENTRY_SIZE)
							{
								int dataLength = entrySize - RCO_ENTRY_SIZE;
								data = outerInstance.readBytes(dataLength);
								if (log.TraceEnabled)
								{
									log.trace(string.Format("ANIM data at 0x{0:X}: {1}", entryOffset + RCO_ENTRY_SIZE, Utilities.getMemoryDump(data, 0, dataLength)));
								}

								if (obj != null)
								{
									RCOContext context = new RCOContext(data, 0, outerInstance.events, outerInstance.images, outerInstance.objects);
									obj.read(context);
									if (context.offset != dataLength)
									{
										Console.WriteLine(string.Format("Incorrect Length data for ANIM"));
									}

									outerInstance.objects[entryOffset] = obj;

									//if (log.DebugEnabled)
									{
										Console.WriteLine(string.Format("ANIM: {0}", obj));
									}
								}
							}
						}
						break;
					case RCO_TABLE_FONT:
						if (type == 1)
						{
							int format = outerInstance.read16();
							int compression = outerInstance.read16();
							int unknown1 = outerInstance.read32();
							int unknown2 = outerInstance.read32();
							//if (log.DebugEnabled)
							{
								Console.WriteLine(string.Format("RCO entry FONT: format={0:D}, compression={1:D}, unknown1=0x{2:X}, unknown2=0x{3:X}", format, compression, unknown1, unknown2));
							}
						}
						else if (type != 0)
						{
							Console.WriteLine(string.Format("Unknown RCO FONT entry type 0x{0:X} at offset 0x{1:X}", type, entryOffset));
						}
						break;
					case RCO_TABLE_TEXT:
						if (type == 1)
						{
							int lang = outerInstance.read16();
							int format = outerInstance.read16();
							int numIndexes = outerInstance.read32();
							//if (log.DebugEnabled)
							{
								Console.WriteLine(string.Format("RCO entry TEXT: lang={0:D}, format={1:D}, numIndexes=0x{2:X}", lang, format, numIndexes));
							}
							texts = new string[numIndexes];
							for (int i = 0; i < numIndexes; i++)
							{
								int labelOffset = outerInstance.read32();
								int Length = outerInstance.read32();
								int offset = outerInstance.read32();
								texts[i] = outerInstance.readText(lang, offset, Length);
								//if (log.DebugEnabled)
								{
									Console.WriteLine(string.Format("RCO entry TEXT Index#{0:D}: labelOffset={1:D}, Length={2:D}, offset=0x{3:X}; '{4}'", i, labelOffset, Length, offset, texts[i]));
								}
							}
						}
						else if (type != 0)
						{
							Console.WriteLine(string.Format("Unknown RCO TEXT entry type 0x{0:X} at offset 0x{1:X}", type, entryOffset));
						}
						break;
					default:
						Console.WriteLine(string.Format("Unknown RCO entry id 0x{0:X} at offset 0x{1:X}", id, entryOffset));
						break;
				}

				if (numSubEntries > 0)
				{
					subEntries = new RCOEntry[numSubEntries];
					for (int i = 0; i < numSubEntries; i++)
					{
						subEntries[i] = new RCOEntry(outerInstance);
						subEntries[i].read();
					}
				}

			}

			internal virtual string getIdName(int id)
			{
				string[] idNames = new string[] {null, "MAIN", "VSMX", "TEXT", "IMG", "MODEL", "SOUND", "FONT", "OBJ", "ANIM"};
				if (id < 0 || id >= idNames.Length || string.ReferenceEquals(idNames[id], null))
				{
					return string.Format("0x{0:X}", id);
				}

				return idNames[id];
			}

			public override string ToString()
			{
				return string.Format("RCOEntry[type=0x{0:X}, id={1}, labelOffset=0x{2:X}('{3}'), eHeadSize=0x{4:X}, entrySize=0x{5:X}, numSubEntries={6:D}, nextEntryOffset=0x{7:X}, prevEntryOffset=0x{8:X}, parentTblOffset=0x{9:X}", type, getIdName(id), labelOffset, !string.ReferenceEquals(label, null) ? label : "", eHeadSize, entrySize, numSubEntries, nextEntryOffset, prevEntryOffset, parentTblOffset);
			}
		}

		private int read8()
		{
			return buffer[offset++] & 0xFF;
		}

		private int read16()
		{
			return read8() | (read8() << 8);
		}

		private int read32()
		{
			return read16() | (read16() << 16);
		}

		private void skip(int n)
		{
			offset += n;
		}

		private void skip32()
		{
			skip(4);
		}

		private void skip16()
		{
			skip(2);
		}

		private void seek(int offset)
		{
			this.offset = offset;
		}

		private int tell()
		{
			return offset;
		}

		public RCO(sbyte[] buffer)
		{
			this.buffer = buffer;

			valid = read();
		}

		public virtual bool Valid
		{
			get
			{
				return valid;
			}
		}

		private RCOEntry readRCOEntry()
		{
			RCOEntry entry = new RCOEntry(this);
			entry.read();
			return entry;
		}

		private RCOEntry readRCOEntry(int offset)
		{
			seek(offset);
			return readRCOEntry();
		}

		private bool isNull(int ptr)
		{
			return ptr == RCO_NULL_PTR;
		}

		private sbyte[] readBytes(int Length)
		{
			if (Length < 0)
			{
				return null;
			}

			sbyte[] bytes = new sbyte[Length];
			for (int i = 0; i < Length; i++)
			{
				bytes[i] = (sbyte) read8();
			}

			return bytes;
		}

		private sbyte[] readVSMX(int offset, StringBuilder name)
		{
			if (isNull(offset))
			{
				return null;
			}

			RCOEntry entry = readRCOEntry(offset);

			name.Append(entry.label);

			return entry.data;
		}

		private string readLabel(int labelOffset)
		{
			StringBuilder s = new StringBuilder();

			int currentPosition = tell();
			seek(pLabelData + labelOffset);
			for (int maxLength = lLabelData - labelOffset; maxLength > 0; maxLength--)
			{
				int b = read8();
				if (b == 0)
				{
					break;
				}
				s.Append((char) b);
			}
			seek(currentPosition);

			return s.ToString();
		}

		private string readText(int lang, int offset, int Length)
		{
			if (offset == RCO_NULL_PTR)
			{
				return null;
			}

			int currentPosition = tell();
			if (compressedTextDataOffset != null)
			{
				seek(compressedTextDataOffset[lang] + offset);
			}
			else
			{
				seek(pTextData + offset);
			}
			sbyte[] buffer = readBytes(Length);
			seek(currentPosition);

			// Trailing null bytes?
			if (Length >= 2 && buffer[Length - 1] == (sbyte) 0 && buffer[Length - 2] == (sbyte) 0)
			{
				// Remove trailing null bytes
				Length -= 2;
			}

			return StringHelper.NewString(buffer, 0, Length, textDataCharset);
		}

		private BufferedImage readImage(int offset, int Length)
		{
			int currentPosition = tell();
			seek(pImgData + offset);
			sbyte[] buffer = readBytes(Length);
			seek(currentPosition);

			System.IO.Stream imageInputStream = new System.IO.MemoryStream(buffer);
			BufferedImage bufferedImage = null;
			try
			{
				bufferedImage = ImageIO.read(imageInputStream);
				imageInputStream.Close();

				// Add an alpha color channel if not available
				if (!bufferedImage.ColorModel.hasAlpha())
				{
					BufferedImage bufferedImageWithAlpha = new BufferedImage(bufferedImage.Width, bufferedImage.Height, BufferedImage.TYPE_INT_ARGB);
					Graphics2D g = bufferedImageWithAlpha.createGraphics();
					g.drawImage(bufferedImage, 0, 0, null);
					g.dispose();
					bufferedImage = bufferedImageWithAlpha;
				}

				if (dumpImages)
				{
					ImageIO.write(bufferedImage, "png", new File(string.Format("tmp/Image0x{0:X}.png", offset)));
				}
			}
			catch (IOException e)
			{
				Console.WriteLine(string.Format("Error reading image from RCO at 0x{0:X}, Length=0x{1:X}", offset, Length), e);
			}

			return bufferedImage;
		}

		private string readString()
		{
			StringBuilder s = new StringBuilder();
			while (true)
			{
				int b = read8();
				if (b == 0)
				{
					break;
				}
				s.Append((char) b);
			}

			return s.ToString();
		}

		private static sbyte[] append(sbyte[] a, sbyte[] b)
		{
			if (a == null || a.Length == 0)
			{
				return b;
			}
			if (b == null || b.Length == 0)
			{
				return a;
			}

			sbyte[] ab = new sbyte[a.Length + b.Length];
			Array.Copy(a, 0, ab, 0, a.Length);
			Array.Copy(b, 0, ab, a.Length, b.Length);

			return ab;
		}

		private static sbyte[] append(sbyte[] a, int Length, sbyte[] b)
		{
			if (a == null || a.Length == 0 || Length <= 0)
			{
				return b;
			}
			if (b == null || b.Length == 0)
			{
				return a;
			}
			Length = System.Math.Min(a.Length, Length);

			sbyte[] ab = new sbyte[Length + b.Length];
			Array.Copy(a, 0, ab, 0, Length);
			Array.Copy(b, 0, ab, Length, b.Length);

			return ab;
		}

		private static int[] extend(int[] a, int Length)
		{
			if (a == null)
			{
				return new int[Length];
			}
			if (a.Length >= Length)
			{
				return a;
			}
			int[] b = new int[Length];
			Array.Copy(a, 0, b, 0, a.Length);

			return b;
		}

		/// <summary>
		/// Read a RCO file.
		/// See description of an RCO file structure in
		/// https://github.com/kakaroto/RCOMage/blob/master/src/rcofile.h
		/// </summary>
		/// <returns> true  RCO file is valid
		///         false RCO file is invalid </returns>
		private bool read()
		{
			int magic = endianSwap32(read32());
			if (magic != RCO_MAGIC)
			{
				Console.WriteLine(string.Format("Invalid RCO magic 0x{0:X8}", magic));
				return false;
			}
			int version = read32();
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("RCO version 0x{0:X}", version));
			}

			skip32(); // null
			int compression = read32();
			int umdFlag = compression & 0x0F;
			int headerCompression = (compression & 0xF0) >> 4;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("umdFlag=0x{0:X}, headerCompression=0x{1:X}", umdFlag, headerCompression));
			}

			int pMainTable = read32();
			pVSMXTable = read32();
			int pTextTable = read32();
			int pSoundTable = read32();
			int pModelTable = read32();
			int pImgTable = read32();
			skip32(); // pUnknown
			int pFontTable = read32();
			int pObjTable = read32();
			int pAnimTable = read32();
			pTextData = read32();
			lTextData = read32();
			pLabelData = read32();
			lLabelData = read32();
			int pEventData = read32();
			int lEventData = read32();
			int pTextPtrs = read32();
			int lTextPtrs = read32();
			int pImgPtrs = read32();
			int lImgPtrs = read32();
			int pModelPtrs = read32();
			int lModelPtrs = read32();
			int pSoundPtrs = read32();
			int lSoundPtrs = read32();
			int pObjPtrs = read32();
			int lObjPtrs = read32();
			int pAnimPtrs = read32();
			int lAnimPtrs = read32();
			pImgData = read32();
			lImgData = read32();
			int pSoundData = read32();
			int lSoundData = read32();
			int pModelData = read32();
			int lModelData = read32();

			skip32(); // Always 0xFFFFFFFF
			skip32(); // Always 0xFFFFFFFF
			skip32(); // Always 0xFFFFFFFF

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("pMainTable=0x{0:X}, pVSMXTable=0x{1:X}, pTextTable=0x{2:X}, pSoundTable=0x{3:X}, pModelTable=0x{4:X}, pImgTable=0x{5:X}, pFontTable=0x{6:X}, pObjTable=0x{7:X}, pAnimTable=0x{8:X}", pMainTable, pVSMXTable, pTextTable, pSoundTable, pModelTable, pImgTable, pFontTable, pObjTable, pAnimTable));
				Console.WriteLine(string.Format("TextData=0x{0:X}[0x{1:X}], LabelData=0x{2:X}[0x{3:X}], EventData=0x{4:X}[0x{5:X}]", pTextData, lTextData, pLabelData, lLabelData, pEventData, lEventData));
				Console.WriteLine(string.Format("TextPtrs=0x{0:X}[0x{1:X}], ImgPtrs=0x{2:X}[0x{3:X}], ModelPtrs=0x{4:X}[0x{5:X}], SoundPtrs=0x{6:X}[0x{7:X}], ObjPtrs=0x{8:X}[0x{9:X}], AnimPtrs=0x{10:X}[0x{11:X}]", pTextPtrs, lTextPtrs, pImgPtrs, lImgPtrs, pModelPtrs, lModelPtrs, pSoundPtrs, lSoundPtrs, pObjPtrs, lObjPtrs, pAnimPtrs, lAnimPtrs));
				Console.WriteLine(string.Format("ImgData=0x{0:X}[0x{1:X}], SoundData=0x{2:X}[0x{3:X}], ModelData=0x{4:X}[0x{5:X}]", pImgData, lImgData, pSoundData, lSoundData, pModelData, lModelData));
			}

			if (headerCompression != 0)
			{
				int lenPacked = read32();
				int lenUnpacked = read32();
				int lenLongestText = read32();
				sbyte[] packedBuffer = readBytes(lenPacked);
				sbyte[] unpackedBuffer = new sbyte[lenUnpacked];
				int result;

				if (headerCompression == RCO_DATA_COMPRESSION_RLZ)
				{
					result = LZR.decompress(unpackedBuffer, lenUnpacked, packedBuffer);
				}
				else
				{
					Console.WriteLine(string.Format("Unimplemented compression {0:D}", headerCompression));
					result = -1;
				}

				if (log.TraceEnabled)
				{
					log.trace(string.Format("Unpack header longestText=0x{0:X}, result=0x{1:X}: {2}", lenLongestText, result, Utilities.getMemoryDump(unpackedBuffer, 0, lenUnpacked)));
				}

				if (pTextData != RCO_NULL_PTR && lTextData > 0)
				{
					seek(pTextData);
					int nextOffset;
					do
					{
						int textLang = read16();
						skip16();
						nextOffset = read32();
						int textLenPacked = read32();
						int textLenUnpacked = read32();

						sbyte[] textPackedBuffer = readBytes(textLenPacked);
						sbyte[] textUnpackedBuffer = new sbyte[textLenUnpacked];

						if (headerCompression == RCO_DATA_COMPRESSION_RLZ)
						{
							result = LZR.decompress(textUnpackedBuffer, textLenUnpacked, textPackedBuffer);
						}
						else
						{
							Console.WriteLine(string.Format("Unimplemented compression {0:D}", headerCompression));
							result = -1;
						}

						if (log.TraceEnabled)
						{
							log.trace(string.Format("Unpack text lang={0:D}, result=0x{1:X}: {2}", textLang, result, Utilities.getMemoryDump(textUnpackedBuffer, 0, textLenUnpacked)));
						}

						if (result >= 0)
						{
							compressedTextDataOffset = extend(compressedTextDataOffset, textLang + 1);
							compressedTextDataOffset[textLang] = unpackedBuffer.Length + RCO_HEADER_SIZE;
							unpackedBuffer = append(unpackedBuffer, textUnpackedBuffer);
						}

						if (nextOffset == 0)
						{
							break;
						}
						skip(nextOffset - 16 - textLenPacked);
					} while (nextOffset != 0);
				}

				if (result >= 0)
				{
					buffer = append(buffer, RCO_HEADER_SIZE, unpackedBuffer);
				}
			}

			events = new Dictionary<int, string>();
			if (pEventData != RCO_NULL_PTR && lEventData > 0)
			{
				seek(pEventData);
				while (tell() < pEventData + lEventData)
				{
					int index = tell() - pEventData;
					string s = readString();
					if (!string.ReferenceEquals(s, null) && s.Length > 0)
					{
						events[index] = s;
					}
				}
			}

			entries = new Dictionary<int, RCO.RCOEntry>();
			images = new Dictionary<int, BufferedImage>();
			objects = new Dictionary<int, BaseObject>();

			mainTable = readRCOEntry(pMainTable);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("mainTable: {0}", mainTable));
			}

			if (pObjPtrs != RCO_NULL_PTR)
			{
				seek(pObjPtrs);
				for (int i = 0; i < lObjPtrs; i += 4)
				{
					int objPtr = read32();
					if (objPtr != 0 && !objects.ContainsKey(objPtr))
					{
						Console.WriteLine(string.Format("Object 0x{0:X} not read", objPtr));
					}
				}
			}

			if (pImgPtrs != RCO_NULL_PTR)
			{
				seek(pImgPtrs);
				for (int i = 0; i < lImgPtrs; i += 4)
				{
					int imgPtr = read32();
					if (imgPtr != 0 && !images.ContainsKey(imgPtr))
					{
						Console.WriteLine(string.Format("Image 0x{0:X} not read", imgPtr));
					}
				}
			}

			RCOContext context = new RCOContext(null, 0, events, images, objects);
			foreach (BaseObject @object in objects.Values)
			{
				@object.init(context);
			}
			return true;
		}

		public virtual RCOState execute(UmdVideoPlayer umdVideoPlayer, string resourceName)
		{
			RCOState state = null;
			if (pVSMXTable != RCO_NULL_PTR)
			{
				state = new RCOState();
				state.interpreter = new VSMXInterpreter();
				state.controller = Controller.create(state.interpreter, umdVideoPlayer, resourceName);
				state = execute(state, umdVideoPlayer, resourceName);

				state.controller.Object.callCallback(state.interpreter, "onAutoPlay", null);
			}

			return state;
		}

		public virtual RCOState execute(RCOState state, UmdVideoPlayer umdVideoPlayer, string resourceName)
		{
			if (pVSMXTable != RCO_NULL_PTR)
			{
				StringBuilder vsmxName = new StringBuilder();
				VSMX vsmx = new VSMX(readVSMX(pVSMXTable, vsmxName), vsmxName.ToString());
				state.interpreter.VSMX = vsmx;
				state.globalVariables = GlobalVariables.create(state.interpreter);
				state.globalVariables.setPropertyValue(Controller.objectName, state.controller);
				state.globalVariables.setPropertyValue(MoviePlayer.objectName, MoviePlayer.create(state.interpreter, umdVideoPlayer, state.controller));
				state.globalVariables.setPropertyValue(Resource.objectName, Resource.create(state.interpreter, umdVideoPlayer.RCODisplay, state.controller, mainTable));
				state.globalVariables.setPropertyValue(pspsharp.format.rco.vsmx.objects.Math.objectName, pspsharp.format.rco.vsmx.objects.Math.create(state.interpreter));
				state.interpreter.run(state.globalVariables);
			}

			return state;
		}

		public override string ToString()
		{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("RCO valid=%b", valid);
			return string.Format("RCO valid=%b", valid);
		}
	}

}