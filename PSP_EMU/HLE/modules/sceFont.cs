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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_FONT_INVALID_PARAMETER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.pspFontStyle.FONT_FAMILY_SANS_SERIF;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.pspFontStyle.FONT_FAMILY_SERIF;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.pspFontStyle.FONT_LANGUAGE_CHINESE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.pspFontStyle.FONT_LANGUAGE_JAPANESE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.pspFontStyle.FONT_LANGUAGE_KOREAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.pspFontStyle.FONT_LANGUAGE_LATIN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.pspFontStyle.FONT_STYLE_BOLD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.pspFontStyle.FONT_STYLE_BOLD_ITALIC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.pspFontStyle.FONT_STYLE_DB;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.pspFontStyle.FONT_STYLE_ITALIC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.pspFontStyle.FONT_STYLE_REGULAR;


	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using SceIoStat = pspsharp.HLE.kernel.types.SceIoStat;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceFontInfo = pspsharp.HLE.kernel.types.SceFontInfo;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using pspCharInfo = pspsharp.HLE.kernel.types.pspCharInfo;
	using pspFontStyle = pspsharp.HLE.kernel.types.pspFontStyle;
	using SeekableDataInput = pspsharp.filesystems.SeekableDataInput;
	using BWFont = pspsharp.format.BWFont;
	using PGF = pspsharp.format.PGF;
	using GeCommands = pspsharp.graphics.GeCommands;
	using CaptureImage = pspsharp.graphics.capture.CaptureImage;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using Settings = pspsharp.settings.Settings;
	using Debug = pspsharp.util.Debug;
	using Utilities = pspsharp.util.Utilities;

	//using Logger = org.apache.log4j.Logger;

	//
	// The stackUsage values are based on tests performed using JpcspTrace
	//
	public class sceFont : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceFont");
		private const bool dumpUserFont = false;

		private class UseDebugFontSettingsListerner : AbstractBoolSettingsListener
		{
			private readonly sceFont outerInstance;

			public UseDebugFontSettingsListerner(sceFont outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override void settingsValueChanged(bool value)
			{
				outerInstance.UseDebugFont = value;
			}
		}

		public override int MemoryUsage
		{
			get
			{
				return 0x7D00;
			}
		}

		public override void start()
		{
			setSettingsListener("emu.useDebugFont", new UseDebugFontSettingsListerner(this));

			internalFonts = new LinkedList<Font>();
			fontLibsMap = new Dictionary<int, FontLib>();
			fontsMap = new Dictionary<int, Font>();
			loadFontRegistry();
			loadDefaultSystemFont();

			base.start();
		}

		[HLEUidClass(errorValueOnNotFound : SceKernelErrors.ERROR_FONT_INVALID_LIBID)]
		public class Font
		{
			private readonly sceFont outerInstance;

			public PGF pgf;
			public SceFontInfo fontInfo;
			public FontLib fontLib;
			internal readonly int handle;
			internal int fontFileSize;
			public int maxGlyphBaseYI;
			public int maxBitmapWidth;
			public int maxBitmapHeight;

			public Font(sceFont outerInstance, PGF pgf, SceFontInfo fontInfo, int fontFileSize)
			{
				this.outerInstance = outerInstance;
				this.pgf = pgf;
				this.fontInfo = fontInfo;
				fontLib = null;
				this.handle = 0;
				this.fontFileSize = fontFileSize;
				if (pgf != null)
				{
					maxGlyphBaseYI = pgf.MaxBaseYAdjust;
					maxBitmapWidth = pgf.MaxGlyphWidth;
					maxBitmapHeight = pgf.MaxGlyphHeight;
				}
			}

			public Font(sceFont outerInstance, Font font, FontLib fontLib, int handle, int fontFileSize)
			{
				this.outerInstance = outerInstance;
				this.pgf = font.pgf;
				this.fontInfo = font.fontInfo;
				this.fontLib = fontLib;
				this.handle = handle;
				this.fontFileSize = fontFileSize;
				maxGlyphBaseYI = font.maxGlyphBaseYI;
				maxBitmapWidth = font.maxBitmapWidth;
				maxBitmapHeight = font.maxBitmapHeight;
			}

			public virtual pspFontStyle FontStyle
			{
				get
				{
					pspFontStyle fontStyle = fontInfo.FontStyle;
					if (fontStyle == null)
					{
						fontStyle = new pspFontStyle();
						fontStyle.fontH = pgf.HSize / 64.0f;
						fontStyle.fontV = pgf.VSize / 64.0f;
						fontStyle.fontHRes = pgf.HResolution / 64.0f;
						fontStyle.fontVRes = pgf.VResolution / 64.0f;
						fontStyle.fontStyle = sceFont.getFontStyle(pgf.FontType);
						fontStyle.fontName = pgf.FontName;
						fontStyle.fontFileName = pgf.FileNamez;
					}
    
					return fontStyle;
				}
			}

			public virtual int Handle
			{
				get
				{
					return handle;
				}
			}

			public virtual bool Closed
			{
				get
				{
					return fontLib == null;
				}
			}

			public virtual void close()
			{
				fontLib = null;
				// Keep PGF and SceFontInfo information.
				// A call to sceFontGetFontInfo is allowed on a closed font.
			}

			public override string ToString()
			{
				if (Closed)
				{
					return string.Format("Font[handle=0x{0:X} closed]", Handle);
				}
				return string.Format("Font[handle=0x{0:X}, '{1}' - '{2}']", Handle, pgf.FileNamez, pgf.FontName);
			}
		}

		public class FontRegistryEntry
		{
			public int h_size;
			public int v_size;
			public int h_resolution;
			public int v_resolution;
			public int extra_attributes;
			public int weight;
			public int family_code;
			public int style;
			public int sub_style;
			public int language_code;
			public int region_code;
			public int country_code;
			public string file_name;
			public string font_name;
			public int expire_date;
			public int shadow_option;
			public int fontFileSize;
			public int maxGlyphBaseYI;
			public int maxBitmapWidth;
			public int maxBitmapHeight;

			public FontRegistryEntry(int h_size, int v_size, int h_resolution, int v_resolution, int extra_attributes, int weight, int family_code, int style, int sub_style, int language_code, int region_code, int country_code, string file_name, string font_name, int expire_date, int shadow_option, int fontFileSize, int maxGlyphBaseYI, int maxBitmapWidth, int maxBitmapHeight)
			{
				this.h_size = h_size;
				this.v_size = v_size;
				this.h_resolution = h_resolution;
				this.v_resolution = v_resolution;
				this.extra_attributes = extra_attributes;
				this.weight = weight;
				this.family_code = family_code;
				this.style = style;
				this.sub_style = sub_style;
				this.language_code = language_code;
				this.region_code = region_code;
				this.country_code = country_code;
				this.file_name = file_name;
				this.font_name = font_name;
				this.expire_date = expire_date;
				this.shadow_option = shadow_option;
				this.fontFileSize = fontFileSize;
				this.maxGlyphBaseYI = maxGlyphBaseYI;
				this.maxBitmapWidth = maxBitmapWidth;
				this.maxBitmapHeight = maxBitmapHeight;
			}

			public FontRegistryEntry()
			{
			}
		}
		public static readonly int PGF_MAGIC = 'P' << 24 | 'G' << 16 | 'F' << 8 | '0';
		public const string customFontFile = "debug.jpft";
		public const int PSP_FONT_PIXELFORMAT_4 = 0; // 2 pixels packed in 1 byte (natural order)
		public const int PSP_FONT_PIXELFORMAT_4_REV = 1; // 2 pixels packed in 1 byte (reversed order)
		public const int PSP_FONT_PIXELFORMAT_8 = 2; // 1 pixel in 1 byte
		public const int PSP_FONT_PIXELFORMAT_24 = 3; // 1 pixel in 3 bytes (RGB)
		public const int PSP_FONT_PIXELFORMAT_32 = 4; // 1 pixel in 4 bytes (RGBA)
		public const int PSP_FONT_MODE_FILE = 0;
		public const int PSP_FONT_MODE_MEMORY = 1;
		private bool useDebugFont = false;
		private const bool dumpFonts = false;
		private IList<Font> internalFonts;
		private Dictionary<int, FontLib> fontLibsMap;
		private Dictionary<int, Font> fontsMap;
		protected internal string uidPurpose = "sceFont";
		private string fontDirPath = "flash0:/font";
		private IList<FontRegistryEntry> fontRegistry;
		protected internal const float pointDPI = 72.0f;

		protected internal virtual bool UseDebugFont
		{
			get
			{
				return useDebugFont;
			}
			set
			{
				useDebugFont = value;
			}
		}


		public virtual IList<FontRegistryEntry> FontRegistry
		{
			get
			{
				return fontRegistry;
			}
		}

		public virtual string FontDirPath
		{
			get
			{
				return fontDirPath;
			}
			set
			{
				this.fontDirPath = value;
			}
		}


		protected internal virtual void loadFontRegistry()
		{
			fontRegistry = new LinkedList<FontRegistryEntry>();
			fontRegistry.Add(new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SANS_SERIF, FONT_STYLE_DB, 0, FONT_LANGUAGE_JAPANESE, 0, 1, "jpn0.pgf", "FTT-NewRodin Pro DB", 0, 0, 1581700, 0x4B4, 19, 20));
			fontRegistry.Add(new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SANS_SERIF, FONT_STYLE_REGULAR, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn0.pgf", "FTT-NewRodin Pro Latin", 0, 0, 69108, 0x4B2, 23, 20));
			fontRegistry.Add(new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SERIF, FONT_STYLE_REGULAR, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn1.pgf", "FTT-Matisse Pro Latin", 0, 0, 65124, 0x482, 23, 20));
			fontRegistry.Add(new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SANS_SERIF, FONT_STYLE_ITALIC, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn2.pgf", "FTT-NewRodin Pro Latin", 0, 0, 72948, 0x4B2, 25, 20));
			fontRegistry.Add(new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SERIF, FONT_STYLE_ITALIC, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn3.pgf", "FTT-Matisse Pro Latin", 0, 0, 67700, 0x482, 25, 20));
			fontRegistry.Add(new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SANS_SERIF, FONT_STYLE_BOLD, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn4.pgf", "FTT-NewRodin Pro Latin", 0, 0, 72828, 0x4F7, 24, 21));
			fontRegistry.Add(new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SERIF, FONT_STYLE_BOLD, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn5.pgf", "FTT-Matisse Pro Latin", 0, 0, 68220, 0x49C, 24, 20));
			fontRegistry.Add(new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SANS_SERIF, FONT_STYLE_BOLD_ITALIC, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn6.pgf", "FTT-NewRodin Pro Latin", 0, 0, 77032, 0x4F7, 27, 21));
			fontRegistry.Add(new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SERIF, FONT_STYLE_BOLD_ITALIC, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn7.pgf", "FTT-Matisse Pro Latin", 0, 0, 71144, 0x49C, 27, 20));
			fontRegistry.Add(new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SANS_SERIF, FONT_STYLE_REGULAR, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn8.pgf", "FTT-NewRodin Pro Latin", 0, 0, 41000, 0x321, 16, 14));
			fontRegistry.Add(new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SERIF, FONT_STYLE_REGULAR, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn9.pgf", "FTT-Matisse Pro Latin", 0, 0, 40164, 0x302, 16, 14));
			fontRegistry.Add(new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SANS_SERIF, FONT_STYLE_ITALIC, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn10.pgf", "FTT-NewRodin Pro Latin", 0, 0, 42692, 0x321, 17, 14));
			fontRegistry.Add(new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SERIF, FONT_STYLE_ITALIC, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn11.pgf", "FTT-Matisse Pro Latin", 0, 0, 41488, 0x302, 17, 14));
			fontRegistry.Add(new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SANS_SERIF, FONT_STYLE_BOLD, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn12.pgf", "FTT-NewRodin Pro Latin", 0, 0, 43136, 0x34F, 17, 15));
			fontRegistry.Add(new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SERIF, FONT_STYLE_BOLD, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn13.pgf", "FTT-Matisse Pro Latin", 0, 0, 41772, 0x312, 17, 14));
			fontRegistry.Add(new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SANS_SERIF, FONT_STYLE_BOLD_ITALIC, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn14.pgf", "FTT-NewRodin Pro Latin", 0, 0, 45184, 0x34F, 18, 15));
			fontRegistry.Add(new FontRegistryEntry(0x1c0, 0x1c0, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SERIF, FONT_STYLE_BOLD_ITALIC, 0, FONT_LANGUAGE_LATIN, 0, 1, "ltn15.pgf", "FTT-Matisse Pro Latin", 0, 0, 43044, 0x312, 18, 14));
			fontRegistry.Add(new FontRegistryEntry(0x288, 0x288, 0x2000, 0x2000, 0, 0, FONT_FAMILY_SANS_SERIF, FONT_STYLE_REGULAR, 0, FONT_LANGUAGE_KOREAN, 0, 3, "kr0.pgf", "AsiaNHH(512Johab)", 0, 0, 394192, 0x3CB, 21, 20));

			// Add the Chinese fixed font file if it is present, i.e. if copied from a real PSP flash0:/font/gb3s1518.bwfon
			if (Modules.IoFileMgrForUserModule.statFile(fontDirPath + "/gb3s1518.bwfon") != null)
			{
				fontRegistry.Add(new FontRegistryEntry(BWFont.charBitmapWidth << 6, BWFont.charBitmapHeight << 6, 0, 0, 0, 0, 0, FONT_STYLE_REGULAR, 0, FONT_LANGUAGE_CHINESE, 0, 0, "gb3s1518.bwfon", "gb3s1518", 0, 0, 1023372, 0, 0, 0));
			}
		}

		protected internal virtual void loadDefaultSystemFont()
		{
			try
			{
				SeekableDataInput fontFile = Modules.IoFileMgrForUserModule.getFile(fontDirPath + "/" + customFontFile, IoFileMgrForUser.PSP_O_RDONLY);
				if (fontFile != null)
				{
					fontFile.skipBytes(32); // Skip custom header.
					sbyte[] c = new sbyte[(int) fontFile.Length() - 32];
					fontFile.readFully(c);
					fontFile.Dispose();
					Debug.Font.DebugFont = c; // Set the internal debug font.
					Debug.Font.DebugCharSize = 8;
					Debug.Font.DebugCharHeight = 8;
					Debug.Font.DebugCharWidth = 8;
				}
			}
			catch (IOException e)
			{
				// The file was removed from flash0.
				Console.WriteLine(e);
			}
		}

		/// <summary>
		/// Dump a font as a .BMP image in the tmp directory for debugging purpose.
		/// </summary>
		/// <param name="font"> the font to be dumped </param>
		protected internal virtual void dumpFont(Font font)
		{
			int addr = MemoryMap.START_VRAM;
			int fontPixelFormat = PSP_FONT_PIXELFORMAT_32;
			int bufferStorage = GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
			int bufferWidth = 800;
			int fontBufWidth = bufferWidth;
			int fontBpl = bufferWidth * sceDisplay.getPixelFormatBytes(bufferStorage);
			int fontBufHeight = MemoryMap.SIZE_VRAM / fontBpl;
			SceFontInfo fontInfo = font.fontInfo;
			PGF pgf = font.pgf;

			int memoryLength = fontBpl * fontBufHeight * sceDisplay.getPixelFormatBytes(bufferStorage);
			Memory mem = Memory.Instance;
			mem.memset(addr, (sbyte) 0, memoryLength);

			Buffer memoryBuffer = Memory.Instance.getBuffer(addr, memoryLength);
			string fileNamePrefix = string.Format("Font-{0}-", pgf.FileNamez);

			int maxGlyphWidth = pgf.MaxSize[0] >> 6;
			int maxGlyphHeight = pgf.MaxSize[1] >> 6;
			int level = 0;
			int x = 0;
			int y = 0;
			int firstCharCode = pgf.FirstGlyphInCharMap;
			int lastCharCode = pgf.LastGlyphInCharMap;
			for (int charCode = firstCharCode; charCode <= lastCharCode; charCode++)
			{
				if (x == 0)
				{
					string linePrefix = string.Format("0x{0:X4}: ", charCode);
					Debug.printFramebuffer(addr, fontBufWidth, x, y, unchecked((int)0xFFFFFFFF), 0x00000000, bufferStorage, linePrefix);
					x += linePrefix.Length * Debug.Font.charWidth;
				}

				fontInfo.printFont(addr, fontBpl, fontBufWidth, fontBufHeight, x, y, 0, 0, 0, 0, fontBufWidth, fontBufHeight, fontPixelFormat, charCode, ' ', SceFontInfo.FONT_PGF_GLYPH_TYPE_CHAR, true);

				x += maxGlyphWidth;
				if (x + maxGlyphWidth > fontBufWidth)
				{
					x = 0;
					y += maxGlyphHeight;
					if (y + maxGlyphHeight > fontBufHeight)
					{
						CaptureImage image = new CaptureImage(addr, level, memoryBuffer, fontBufWidth, fontBufHeight, bufferWidth, bufferStorage, false, 0, false, true, fileNamePrefix);
						log.info(string.Format("Dumping font {0} from charCode 0x{1:X4} to file {2}", pgf.FontName, firstCharCode, image.FileName));
						try
						{
							image.write();
						}
						catch (IOException e)
						{
							Console.WriteLine(e);
						}
						mem.memset(addr, (sbyte) 0, memoryLength);
						level++;
						firstCharCode = charCode + 1;
						x = 0;
						y = 0;
					}
				}
			}

			CaptureImage image = new CaptureImage(addr, level, memoryBuffer, fontBufWidth, fontBufHeight, bufferWidth, bufferStorage, false, 0, false, true, fileNamePrefix);
			log.info(string.Format("Dumping font {0} from charCode 0x{1:X4} to file {2}", pgf.FontName, firstCharCode, image.FileName));
			try
			{
				image.write();
			}
			catch (IOException e)
			{
				Console.WriteLine(e);
			}
		}

		protected internal virtual Font openFontFile(ByteBuffer pgfBuffer, string fileName)
		{
			Font font = null;

			try
			{
				PGF pgfFile;
				if (!string.ReferenceEquals(fileName, null) && fileName.EndsWith(".bwfon", StringComparison.Ordinal))
				{
					pgfFile = new BWFont(pgfBuffer, fileName);
				}
				else
				{
					pgfFile = new PGF(pgfBuffer);
				}

				if (!string.ReferenceEquals(fileName, null))
				{
					pgfFile.FileNamez = fileName;
				}

				SceFontInfo fontInfo = pgfFile.createFontInfo();

				font = new Font(this, pgfFile, fontInfo, pgfBuffer.capacity());

				if (dumpFonts)
				{
					dumpFont(font);
				}
			}
			catch (Exception e)
			{
				// Can't parse file.
				Console.WriteLine("openFontFile", e);
			}

			return font;
		}

		protected internal virtual Font openFontFile(string fileName)
		{
			Font font = null;

			try
			{
				SeekableDataInput fontFile = Modules.IoFileMgrForUserModule.getFile(fileName, IoFileMgrForUser.PSP_O_RDONLY);
				if (fontFile != null)
				{
					sbyte[] pgfBytes = new sbyte[(int) fontFile.Length()];
					fontFile.readFully(pgfBytes);
					fontFile.Dispose();
					ByteBuffer pgfBuffer = ByteBuffer.wrap(pgfBytes);

					font = openFontFile(pgfBuffer, System.IO.Path.GetFileName(fileName));
				}
			}
			catch (IOException e)
			{
				// Can't open file.
				Console.WriteLine(e);
			}

			return font;
		}

		protected internal virtual Font openFontFile(int addr, int Length)
		{
			if (dumpUserFont)
			{
				try
				{
					System.IO.Stream os = new System.IO.FileStream(string.Format("{0}userFont-0x{1:X8}.pgf", Settings.Instance.TmpDirectory, addr), System.IO.FileMode.Create, System.IO.FileAccess.Write);
					IMemoryReader memoryReader = MemoryReader.getMemoryReader(addr, Length, 1);
					for (int i = 0; i < Length; i++)
					{
						os.WriteByte(memoryReader.readNext());
					}
					os.Close();
				}
				catch (FileNotFoundException)
				{
				}
				catch (IOException)
				{
				}
			}
			ByteBuffer pgfBuffer = ByteBuffer.allocate(Length);
			Buffer memBuffer = Memory.Instance.getBuffer(addr, Length);
			Utilities.putBuffer(pgfBuffer, memBuffer, ByteOrder.LITTLE_ENDIAN, Length);
			pgfBuffer.rewind();

			Font font = openFontFile(pgfBuffer, null);

			return font;
		}

		protected internal virtual void setFontAttributesFromRegistry(Font font, FontRegistryEntry fontRegistryEntry)
		{
			pspFontStyle fontStyle = new pspFontStyle();
			fontStyle.fontH = fontRegistryEntry.h_size / 64.0f;
			fontStyle.fontV = fontRegistryEntry.v_size / 64.0f;
			fontStyle.fontHRes = fontRegistryEntry.h_resolution / 64.0f;
			fontStyle.fontVRes = fontRegistryEntry.v_resolution / 64.0f;
			fontStyle.fontWeight = fontRegistryEntry.weight;
			fontStyle.fontFamily = (short) fontRegistryEntry.family_code;
			fontStyle.fontStyle = (short) fontRegistryEntry.style;
			fontStyle.fontStyleSub = (short) fontRegistryEntry.sub_style;
			fontStyle.fontLanguage = (short) fontRegistryEntry.language_code;
			fontStyle.fontRegion = (short) fontRegistryEntry.region_code;
			fontStyle.fontCountry = (short) fontRegistryEntry.country_code;
			fontStyle.fontName = fontRegistryEntry.font_name;
			fontStyle.fontFileName = fontRegistryEntry.file_name;
			fontStyle.fontAttributes = fontRegistryEntry.extra_attributes;
			fontStyle.fontExpire = fontRegistryEntry.expire_date;

			font.fontInfo.FontStyle = fontStyle;

			// The font file size is used during a sceOpenFont() call to allocate memory.
			// The pspsharp font files differ significantly in size from the real PSP files.
			// So it is important to use the font file sizes from a real PSP, so that
			// the correct memory is being allocated.
			font.fontFileSize = fontRegistryEntry.fontFileSize;

			// The following values are critical for some applications and need to match
			// the values from a real PSP font file.
			font.maxGlyphBaseYI = fontRegistryEntry.maxGlyphBaseYI;
			font.maxBitmapWidth = fontRegistryEntry.maxBitmapWidth;
			font.maxBitmapHeight = fontRegistryEntry.maxBitmapHeight;
		}

		protected internal virtual Font FontAttributesFromRegistry
		{
			set
			{
				foreach (FontRegistryEntry fontRegistryEntry in fontRegistry)
				{
					if (fontRegistryEntry.file_name.Equals(value.pgf.FileNamez))
					{
						if (fontRegistryEntry.font_name.Equals(value.pgf.FontName))
						{
							setFontAttributesFromRegistry(value, fontRegistryEntry);
							break;
						}
					}
				}
			}
		}

		protected internal virtual void loadAllFonts()
		{
			internalFonts.Clear();

			// Load the fonts in the same order as on a PSP.
			// Some applications are always using the first font returned by
			// sceFontGetFontList.
			foreach (FontRegistryEntry fontRegistryEntry in fontRegistry)
			{
				string fontFileName = fontDirPath + "/" + fontRegistryEntry.file_name;
				SceIoStat stat = Modules.IoFileMgrForUserModule.statFile(fontFileName);
				if (stat != null)
				{
					Font font = openFontFile(fontFileName);
					if (font != null)
					{
						setFontAttributesFromRegistry(font, fontRegistryEntry);
						internalFonts.Add(font);
						log.info(string.Format("Loading font file '{0}'. Font='{1}' Type='{2}'", fontRegistryEntry.file_name, font.pgf.FontName, font.pgf.FontType));
					}
				}
			}
		}

		protected internal static short getFontStyle(string styleString)
		{
			if ("Regular".Equals(styleString))
			{
				return FONT_STYLE_REGULAR;
			}
			if ("Italic".Equals(styleString))
			{
				return FONT_STYLE_ITALIC;
			}
			if ("Bold".Equals(styleString))
			{
				return FONT_STYLE_BOLD;
			}
			if ("Bold Italic".Equals(styleString))
			{
				return FONT_STYLE_BOLD_ITALIC;
			}

			return 0;
		}

		[HLEUidClass(errorValueOnNotFound : SceKernelErrors.ERROR_FONT_INVALID_LIBID)]
		protected internal class FontLib
		{
			internal bool InstanceFieldsInitialized = false;

			internal virtual void InitializeInstanceFields()
			{
				allocatedAddresses = new int[allocatedSizes.Length];
			}

			private readonly sceFont outerInstance;

			internal const int FONT_IS_CLOSED = 0;
			internal const int FONT_IS_OPEN = 1;
			protected internal int userDataAddr;
			protected internal int numFonts;
			protected internal int cacheDataAddr;
			protected internal int allocFuncAddr;
			protected internal int freeFuncAddr;
			protected internal int openFuncAddr;
			protected internal int closeFuncAddr;
			protected internal int readFuncAddr;
			protected internal int seekFuncAddr;
			protected internal int errorFuncAddr;
			protected internal int ioFinishFuncAddr;
			protected internal int fileFontHandle;
			protected internal int altCharCode;
			protected internal float fontHRes = 128.0f;
			protected internal float fontVRes = 128.0f;
			protected internal int handle;
			protected internal int[] fonts;
			protected internal int[] allocatedSizes = new int[] {0x4C, 0x130, 0x8C0, 0xC78};
			protected internal int[] allocatedAddresses;
			protected internal int allocatedAddressIndex;
			protected internal int[] openAllocatedAddresses;
			protected internal int charInfoBitmapAddress;

			public FontLib(sceFont outerInstance, TPointer32 @params)
			{
				this.outerInstance = outerInstance;

				if (!InstanceFieldsInitialized)
				{
					InitializeInstanceFields();
					InstanceFieldsInitialized = true;
				}
				read(@params);

				// On a PSP, FontLib handle and Font handles are addresses pointing to memory
				// allocated by the "Alloc" callback.
				// Here, we just fake theses addresses by allocating an area small enough to
				// provide different addresses for the required FontLib and Font handles.
				// E.g.
				//    addr     = FontLib handle
				//    addr + 4 = Font handle for 1st font
				//    addr + 8 = Font handle for 2nd font
				//    ...
				// Furthermore, the value stored at a Font handle address indicates if the
				// font is closed (e.g. free to be opened) or open.
				//    mem.read32(fontHandle) == FONT_IS_OPEN: font is already open
				//    mem.read32(fontHandle) == FONT_IS_CLOSED: font is not open
				//
				openAllocatedAddresses = new int[numFonts];

				allocateAddresses();
			}

			internal virtual void allocateAddresses()
			{
				int minimumSize = numFonts * 4 + 4;
				if (allocatedSizes[0] < minimumSize)
				{
					allocatedSizes[0] = minimumSize;
				}

				allocatedAddressIndex = 0;
				triggerAllocCallback(allocatedSizes[allocatedAddressIndex], new AfterCreateAllocCallback(this));
			}

			public virtual int NumFonts
			{
				get
				{
					return numFonts;
				}
			}

			internal virtual Font openFont(Font font, int mode, bool needAllocForFontFile)
			{
				if (font == null)
				{
					throw (new SceKernelErrorException(SceKernelErrors.ERROR_FONT_INVALID_PARAMETER));
				}

				Memory mem = Memory.Instance;
				int freeFontIndex = -1;
				// Search for a free font slot
				for (int i = 0; i < numFonts; i++)
				{
					if (mem.read32(fonts[i]) == FONT_IS_CLOSED)
					{
						freeFontIndex = i;
						break;
					}
				}
				if (freeFontIndex < 0)
				{
					throw (new SceKernelErrorException(SceKernelErrors.ERROR_FONT_TOO_MANY_OPEN_FONTS));
				}

				font = new Font(outerInstance, font, this, fonts[freeFontIndex], font.fontFileSize);
				mem.write32(fonts[freeFontIndex], FONT_IS_OPEN);
				outerInstance.fontsMap[font.Handle] = font;

				int allocSize = 12;
				if (needAllocForFontFile)
				{
					if (mode == 0)
					{
						// mode == 0: only parts of the font file are read.
						// For jpn0.pgf, the alloc callback is called multiple times:
						//     0x7F8, 0x7F8, 0x7F8, 0x350, 0x3FC, 0x0, 0x0, 0x1BFC8, 0x5AB8
						// in total: 0x239B4
						allocSize = 0x239B4;
					}
					else if (mode == 1)
					{
						// mode == 1: the whole font file is read in memory
						allocSize += font.fontFileSize;
					}
				}

				triggerAllocCallback(allocSize, new AfterOpenAllocCallback(this, freeFontIndex));

				return font;
			}

			internal virtual void closeFont(Font font)
			{
				HLEUidObjectMapping.removeObject(font);

				for (int i = 0; i < numFonts; i++)
				{
					if (fonts[i] == font.Handle)
					{
						Memory mem = Memory.Instance;
						mem.write32(fonts[i], FONT_IS_CLOSED);

						if (openAllocatedAddresses[i] != 0)
						{
							triggerFreeCallback(openAllocatedAddresses[i], null);
							openAllocatedAddresses[i] = 0;
						}
						break;
					}
				}

				flushFont(font);

				font.close();
			}

			public virtual void flushFont(Font font)
			{
				if (charInfoBitmapAddress != 0)
				{
					triggerFreeCallback(charInfoBitmapAddress, null);
					charInfoBitmapAddress = 0;
				}
			}

			public virtual void done()
			{
				Memory mem = Memory.Instance;
				for (int i = 0; i < numFonts; i++)
				{
					if (mem.read32(fonts[i]) == FONT_IS_OPEN)
					{
						closeFont(outerInstance.fontsMap[fonts[i]]);
					}
					outerInstance.fontsMap.Remove(fonts[i]);
				}
				triggerFreeCallback(allocatedAddresses[--allocatedAddressIndex], new AfterFreeCallback(this));
				fonts = null;
			}

			public virtual int triggetGetCharInfo(pspCharInfo charInfo)
			{
				int result = 0;

				// The callbacks are not triggered by characters not present in the font
				if (charInfo.sfp26AdvanceH != 0 || charInfo.sfp26AdvanceV != 0)
				{
					SceKernelThreadInfo thread = Modules.ThreadManForUserModule.CurrentThread;

					if (charInfoBitmapAddress != 0)
					{
						triggerFreeCallback(charInfoBitmapAddress, new AfterCharInfoFreeCallback(this, thread, charInfo));
					}
					else
					{
						triggerAllocCallback(charInfo.bitmapWidth * charInfo.bitmapHeight, new AfterCharInfoAllocCallback(this, thread));
					}

					if (charInfoBitmapAddress == 0)
					{
						result = SceKernelErrors.ERROR_FONT_OUT_OF_MEMORY;
					}
				}

				return result;
			}

			public virtual int Handle
			{
				get
				{
					return handle;
				}
			}

			protected internal virtual void triggerAllocCallback(int size, IAction afterAllocCallback)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("triggerAllocCallback size=0x{0:X}", size));
				}
				Modules.ThreadManForUserModule.executeCallback(null, allocFuncAddr, afterAllocCallback, true, userDataAddr, size);
			}

			protected internal virtual void triggerFreeCallback(int addr, IAction afterFreeCallback)
			{
				if (Memory.isAddressGood(addr))
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Calling free callback on 0x{0:X8}", addr));
					}
					Modules.ThreadManForUserModule.executeCallback(null, freeFuncAddr, afterFreeCallback, true, userDataAddr, addr);
				}
			}

			protected internal virtual void triggerOpenCallback(int fileNameAddr, int errorCodeAddr)
			{
				Modules.ThreadManForUserModule.executeCallback(null, openFuncAddr, new AfterOpenCallback(this), true, userDataAddr, fileNameAddr, errorCodeAddr);
			}

			protected internal virtual void triggerCloseCallback()
			{
				if (fileFontHandle != 0)
				{
					Modules.ThreadManForUserModule.executeCallback(null, closeFuncAddr, null, true, userDataAddr, fileFontHandle);
				}
			}

			internal virtual void read(TPointer32 @params)
			{
				userDataAddr = @params.getValue(0);
				numFonts = @params.getValue(4);
				cacheDataAddr = @params.getValue(8);
				allocFuncAddr = @params.getValue(12);
				freeFuncAddr = @params.getValue(16);
				openFuncAddr = @params.getValue(20);
				closeFuncAddr = @params.getValue(24);
				readFuncAddr = @params.getValue(28);
				seekFuncAddr = @params.getValue(32);
				errorFuncAddr = @params.getValue(36);
				ioFinishFuncAddr = @params.getValue(40);

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("userDataAddr 0x{0:X8}, numFonts={1:D}, cacheDataAddr=0x{2:X8}, allocFuncAddr=0x{3:X8}, freeFuncAddr=0x{4:X8}, openFuncAddr=0x{5:X8}, closeFuncAddr=0x{6:X8}, readFuncAddr=0x{7:X8}, seekFuncAddr=0x{8:X8}, errorFuncAddr=0x{9:X8}, ioFinishFuncAddr=0x{10:X8}", userDataAddr, numFonts, cacheDataAddr, allocFuncAddr, freeFuncAddr, openFuncAddr, closeFuncAddr, readFuncAddr, seekFuncAddr, errorFuncAddr, ioFinishFuncAddr));
				}
			}

			public virtual int AltCharCode
			{
				get
				{
					return altCharCode;
				}
				set
				{
					this.altCharCode = value;
				}
			}


			public virtual int getFontHandle(int index)
			{
				return fonts[index];
			}

			private class AfterCreateAllocCallback : IAction
			{
				private readonly sceFont.FontLib outerInstance;

				public AfterCreateAllocCallback(sceFont.FontLib outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public virtual void execute()
				{
					int allocatedAddr = Emulator.Processor.cpu._v0;

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("FontLib's allocation callback#{0:D} returned 0x{1:X8} for size 0x{2:X}", outerInstance.allocatedAddressIndex, allocatedAddr, outerInstance.allocatedSizes[outerInstance.allocatedAddressIndex]));
					}

					if (outerInstance.allocatedAddressIndex == 0)
					{
						int addr = allocatedAddr;
						outerInstance.handle = addr;
						addr += 4;
						outerInstance.fonts = new int[outerInstance.numFonts];
						Memory mem = Memory.Instance;
						for (int i = 0; i < outerInstance.numFonts; i++)
						{
							mem.write32(addr, FONT_IS_CLOSED);
							outerInstance.fonts[i] = addr;
							addr += 4;
						}
					}

					outerInstance.allocatedAddresses[outerInstance.allocatedAddressIndex++] = allocatedAddr;
					if (outerInstance.allocatedAddressIndex < outerInstance.allocatedSizes.Length)
					{
						outerInstance.triggerAllocCallback(outerInstance.allocatedSizes[outerInstance.allocatedAddressIndex], this);
					}
				}
			}

			private class AfterFreeCallback : IAction
			{
				private readonly sceFont.FontLib outerInstance;

				public AfterFreeCallback(sceFont.FontLib outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public virtual void execute()
				{
					if (outerInstance.allocatedAddressIndex > 0)
					{
						outerInstance.triggerFreeCallback(outerInstance.allocatedAddresses[--outerInstance.allocatedAddressIndex], this);
					}
				}
			}

			private class AfterOpenAllocCallback : IAction
			{
				private readonly sceFont.FontLib outerInstance;

				internal int fontIndex;

				public AfterOpenAllocCallback(sceFont.FontLib outerInstance, int fontIndex)
				{
					this.outerInstance = outerInstance;
					this.fontIndex = fontIndex;
				}

				public virtual void execute()
				{
					int allocatedAddr = Emulator.Processor.cpu._v0;

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("FontLib's allocation callback on open#{0:D} returned 0x{1:X8}", fontIndex, allocatedAddr));
					}

					outerInstance.openAllocatedAddresses[fontIndex] = allocatedAddr;
				}
			}

			private class AfterOpenCallback : IAction
			{
				private readonly sceFont.FontLib outerInstance;

				public AfterOpenCallback(sceFont.FontLib outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public virtual void execute()
				{
					outerInstance.fileFontHandle = Emulator.Processor.cpu._v0;

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("FontLib's file open callback returned 0x{0:X}", outerInstance.fileFontHandle));
					}
				}
			}

			private class AfterCharInfoFreeCallback : IAction
			{
				private readonly sceFont.FontLib outerInstance;

				internal SceKernelThreadInfo thread;
				internal pspCharInfo charInfo;

				public AfterCharInfoFreeCallback(sceFont.FontLib outerInstance, SceKernelThreadInfo thread, pspCharInfo charInfo)
				{
					this.outerInstance = outerInstance;
					this.thread = thread;
					this.charInfo = charInfo;
				}

				public virtual void execute()
				{
					outerInstance.charInfoBitmapAddress = 0;
					outerInstance.triggerAllocCallback(charInfo.bitmapWidth * charInfo.bitmapHeight, new AfterCharInfoAllocCallback(outerInstance, thread));
				}
			}

			private class AfterCharInfoAllocCallback : IAction
			{
				private readonly sceFont.FontLib outerInstance;

				internal SceKernelThreadInfo thread;

				public AfterCharInfoAllocCallback(sceFont.FontLib outerInstance, SceKernelThreadInfo thread)
				{
					this.outerInstance = outerInstance;
					this.thread = thread;
				}

				public virtual void execute()
				{
					outerInstance.charInfoBitmapAddress = Emulator.Processor.cpu._v0;

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("FontLib's allocation callback on getCharInfo returned 0x{0:X8}", outerInstance.charInfoBitmapAddress));
					}

					if (outerInstance.charInfoBitmapAddress == 0)
					{
						thread.cpuContext._v0 = SceKernelErrors.ERROR_FONT_OUT_OF_MEMORY;
					}
				}
			}

			public override string ToString()
			{
				return string.Format("FontLib - Handle: '0x{0:X8}', Fonts: '{1:D}'", Handle, NumFonts);
			}
		}

		private bool isFontMatchingStyle(Font font, pspFontStyle fontStyle, bool optimum)
		{
			if (font != null && font.fontInfo != null && font.fontInfo.FontStyle != null)
			{
				return font.fontInfo.FontStyle.isMatching(fontStyle, optimum);
			}
			// Faking: always matching
			return true;
		}

		/// <summary>
		/// Check if a given font is better matching the fontStyle than the currently best font.
		/// The check is based on the fontH and fontV.
		/// </summary>
		/// <param name="fontStyle">    the criteria for the optimum font </param>
		/// <param name="optimumFont">  the currently optimum font </param>
		/// <param name="matchingFont"> a candidate matching the fontStyle </param>
		/// <returns>             the matchingFont if it is better matching the fontStyle than the optimumFont,
		///                     the optimumFont otherwise </returns>
		private Font getOptimiumFont(pspFontStyle fontStyle, Font optimumFont, Font matchingFont)
		{
			if (optimumFont == null)
			{
				return matchingFont;
			}
			pspFontStyle optimiumStyle = optimumFont.fontInfo.FontStyle;
			pspFontStyle matchingStyle = matchingFont.fontInfo.FontStyle;

			// Check the fontH if it is specified or both fontH and fontV are unspecified
			bool testH = fontStyle.fontH != 0f || fontStyle.fontV == 0f;
			if (testH && System.Math.Abs(fontStyle.fontH - optimiumStyle.fontH) > System.Math.Abs(fontStyle.fontH - matchingStyle.fontH))
			{
				return matchingFont;
			}

			// Check the fontV if it is specified or both fontH and fontV are unspecified
			bool testV = fontStyle.fontV != 0f || fontStyle.fontH == 0f;
			if (testV && System.Math.Abs(fontStyle.fontV - optimiumStyle.fontV) > System.Math.Abs(fontStyle.fontV - matchingStyle.fontV))
			{
				return matchingFont;
			}

			return optimumFont;
		}

		private Font getOptimumFont(pspFontStyle fontStyle)
		{
			Font optimumFont = null;
			for (int i = 0; i < internalFonts.Count; i++)
			{
				Font font = internalFonts[i];
				if (isFontMatchingStyle(font, fontStyle, true))
				{
					optimumFont = getOptimiumFont(fontStyle, optimumFont, font);
				}
			}

			return optimumFont;
		}

		private int getFontIndex(Font font)
		{
			if (font != null)
			{
				for (int i = 0; i < internalFonts.Count; i++)
				{
					if (internalFonts[i] == font)
					{
						return i;
					}
				}
			}

			return -1;
		}

		protected internal virtual FontLib getFontLib(int fontLibHandle)
		{
			FontLib fontLib = fontLibsMap[fontLibHandle];
			if (fontLib == null)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_FONT_INVALID_PARAMETER);
			}

			return fontLib;
		}

		protected internal virtual Font getFont(int fontHandle, bool allowClosedFont)
		{
			Font font = fontsMap[fontHandle];
			if (font == null || font.fontInfo == null)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_FONT_INVALID_PARAMETER);
			}
			if (!allowClosedFont && font.Closed)
			{
				throw new SceKernelErrorException(SceKernelErrors.ERROR_FONT_INVALID_PARAMETER);
			}

			return font;
		}

		public virtual Font getFont(int index)
		{
			if (internalFonts.Count == 0)
			{
				loadAllFonts();
			}
			return internalFonts[index];
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x67F17ED7, version = 150, checkInsideInterrupt = true, stackUsage = 0x590) public int sceFontNewLib(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=44, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer32 paramsPtr, @CanBeNull pspsharp.HLE.TErrorPointer32 errorCodePtr)
		[HLEFunction(nid : 0x67F17ED7, version : 150, checkInsideInterrupt : true, stackUsage : 0x590)]
		public virtual int sceFontNewLib(TPointer32 paramsPtr, TErrorPointer32 errorCodePtr)
		{
			loadAllFonts();
			errorCodePtr.Value = 0;
			FontLib fontLib = new FontLib(this, paramsPtr);
			fontLibsMap[fontLib.Handle] = fontLib;

			return fontLib.Handle;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x57FCB733, version = 150, checkInsideInterrupt = true) public int sceFontOpenUserFile(int fontLibHandle, pspsharp.HLE.PspString fileName, int mode, @CanBeNull pspsharp.HLE.TErrorPointer32 errorCodePtr)
		[HLEFunction(nid : 0x57FCB733, version : 150, checkInsideInterrupt : true)]
		public virtual int sceFontOpenUserFile(int fontLibHandle, PspString fileName, int mode, TErrorPointer32 errorCodePtr)
		{
			FontLib fontLib = getFontLib(fontLibHandle);
			errorCodePtr.Value = 0;

			// "open" callback is not called in this case. Tested on PSP.

			Font font = openFontFile(fileName.String);
			if (font == null)
			{
				errorCodePtr.Value = SceKernelErrors.ERROR_FONT_FILE_NOT_FOUND;
				return 0;
			}

			return fontLib.openFont(font, mode, true).Handle;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xBB8E7FE6, version = 150, checkInsideInterrupt = true, stackUsage = 0x440) public int sceFontOpenUserMemory(int fontLibHandle, pspsharp.HLE.TPointer memoryFontPtr, int memoryFontLength, @CanBeNull pspsharp.HLE.TErrorPointer32 errorCodePtr)
		[HLEFunction(nid : 0xBB8E7FE6, version : 150, checkInsideInterrupt : true, stackUsage : 0x440)]
		public virtual int sceFontOpenUserMemory(int fontLibHandle, TPointer memoryFontPtr, int memoryFontLength, TErrorPointer32 errorCodePtr)
		{
			FontLib fontLib = getFontLib(fontLibHandle);
			errorCodePtr.Value = 0;
			return fontLib.openFont(openFontFile(memoryFontPtr.Address, memoryFontLength), 0, false).Handle;
		}

		[HLEFunction(nid : 0x0DA7535E, version : 150, checkInsideInterrupt : true, stackUsage : 0x0)]
		public virtual int sceFontGetFontInfo(int fontHandle, TPointer fontInfoPtr)
		{
			// A call to sceFontGetFontInfo is allowed on a closed font.
			Font font = getFont(fontHandle, true);

			PGF currentPGF = font.pgf;
			int maxGlyphWidthI = currentPGF.MaxSize[0];
			int maxGlyphHeightI = currentPGF.MaxSize[1];
			int maxGlyphAscenderI = currentPGF.MaxAscender;
			int maxGlyphDescenderI = currentPGF.MaxDescender;
			int maxGlyphLeftXI = currentPGF.MaxLeftXAdjust;
			int maxGlyphBaseYI = font.maxGlyphBaseYI;
			int minGlyphCenterXI = currentPGF.MinCenterXAdjust;
			int maxGlyphTopYI = currentPGF.MaxTopYAdjust;
			int maxGlyphAdvanceXI = currentPGF.MaxAdvance[0];
			int maxGlyphAdvanceYI = currentPGF.MaxAdvance[1];
			int maxBitmapWidth = font.maxBitmapWidth;
			int maxBitmapHeight = font.maxBitmapHeight;
			pspFontStyle fontStyle = font.FontStyle;

			// Glyph metrics (in 26.6 signed fixed-point).
			fontInfoPtr.setValue32(0, maxGlyphWidthI);
			fontInfoPtr.setValue32(4, maxGlyphHeightI);
			fontInfoPtr.setValue32(8, maxGlyphAscenderI);
			fontInfoPtr.setValue32(12, maxGlyphDescenderI);
			fontInfoPtr.setValue32(16, maxGlyphLeftXI);
			fontInfoPtr.setValue32(20, maxGlyphBaseYI);
			fontInfoPtr.setValue32(24, minGlyphCenterXI);
			fontInfoPtr.setValue32(28, maxGlyphTopYI);
			fontInfoPtr.setValue32(32, maxGlyphAdvanceXI);
			fontInfoPtr.setValue32(36, maxGlyphAdvanceYI);

			// Glyph metrics (replicated as float).
			for (int i = 0; i < 40; i += 4)
			{
				int intValue = fontInfoPtr.getValue32(i);
				float floatValue = intValue / 64.0f;
				fontInfoPtr.setFloat(i + 40, floatValue);
			}

			// Bitmap dimensions.
			fontInfoPtr.setValue16(80, (short) maxBitmapWidth);
			fontInfoPtr.setValue16(82, (short) maxBitmapHeight);
			fontInfoPtr.setValue32(84, currentPGF.CharPointerLength); // Number of elements in the font's charmap.
			fontInfoPtr.setValue32(88, 0); // Number of elements in the font's shadow charmap.

			// Font style (used by font comparison functions).
			fontStyle.write(fontInfoPtr, 92);

			fontInfoPtr.setValue8(260, (sbyte) currentPGF.Bpp); // Font's BPP.
			fontInfoPtr.setValue8(261, (sbyte) 0); // Padding.
			fontInfoPtr.setValue8(262, (sbyte) 0); // Padding.
			fontInfoPtr.setValue8(263, (sbyte) 0); // Padding.

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceFontGetFontInfo returning maxGlyphWidthI={0:D}, maxGlyphHeightI={1:D}, maxGlyphAscenderI={2:D}, maxGlyphDescenderI={3:D}, maxGlyphLeftXI={4:D}, maxGlyphBaseYI={5:D}, minGlyphCenterXI={6:D}, maxGlyphTopYI={7:D}, maxGlyphAdvanceXI={8:D}, maxGlyphAdvanceYI={9:D}, maxBitmapWidth={10:D}, maxBitmapHeight={11:D}, fontStyle=[{12}]{13}", maxGlyphWidthI, maxGlyphHeightI, maxGlyphAscenderI, maxGlyphDescenderI, maxGlyphLeftXI, maxGlyphBaseYI, minGlyphCenterXI, maxGlyphTopYI, maxGlyphAdvanceXI, maxGlyphAdvanceYI, maxBitmapWidth, maxBitmapHeight, fontStyle, Utilities.getMemoryDump(fontInfoPtr.Address, 264)));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xDCC80C2F, version = 150, checkInsideInterrupt = true, stackUsage = 0x100) public int sceFontGetCharInfo(int fontHandle, int charCode, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=60, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer charInfoPtr)
		[HLEFunction(nid : 0xDCC80C2F, version : 150, checkInsideInterrupt : true, stackUsage : 0x100)]
		public virtual int sceFontGetCharInfo(int fontHandle, int charCode, TPointer charInfoPtr)
		{
			Font font = getFont(fontHandle, false);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceFontGetCharInfo charCode={0:X4} ({1})", charCode, (charCode <= 0xFF ? (char) charCode : '?')));
			}
			charCode &= 0xFFFF;
			pspCharInfo pspCharInfo = null;
			if (!UseDebugFont)
			{
				pspCharInfo = font.fontInfo.getCharInfo(charCode, SceFontInfo.FONT_PGF_GLYPH_TYPE_CHAR);
			}
			if (pspCharInfo == null)
			{
				pspCharInfo = new pspCharInfo();
				pspCharInfo.bitmapWidth = Debug.Font.charWidth * Debug.fontPixelSize;
				pspCharInfo.bitmapHeight = Debug.Font.charHeight * Debug.fontPixelSize;
				pspCharInfo.sfp26Width = pspCharInfo.bitmapWidth << 6;
				pspCharInfo.sfp26Height = pspCharInfo.bitmapHeight << 6;
				pspCharInfo.sfp26AdvanceH = pspCharInfo.bitmapWidth << 6;
				pspCharInfo.sfp26AdvanceV = pspCharInfo.bitmapHeight << 6;
			}
			int result = font.fontLib.triggetGetCharInfo(pspCharInfo);

			if (result == 0)
			{
				pspCharInfo.write(charInfoPtr);
			}

			return result;
		}

		[HLEFunction(nid : 0x980F4895, version : 150, checkInsideInterrupt : true)]
		public virtual int sceFontGetCharGlyphImage(int fontHandle, int charCode, TPointer glyphImagePtr)
		{
			charCode &= 0xffff;
			Font font = getFont(fontHandle, false);

			// Read GlyphImage data.
			int pixelFormat = glyphImagePtr.getValue32(0);
			int xPos64 = glyphImagePtr.getValue32(4);
			int yPos64 = glyphImagePtr.getValue32(8);
			int bufWidth = glyphImagePtr.getValue16(12);
			int bufHeight = glyphImagePtr.getValue16(14);
			int bytesPerLine = glyphImagePtr.getValue16(16);
			int buffer = glyphImagePtr.getValue32(20);
			// 26.6 fixed-point.
			int xPosI = xPos64 >> 6;
			int yPosI = yPos64 >> 6;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceFontGetCharGlyphImage charCode={0:X4} ({1}), xPos={2:D}, yPos={3:D}, buffer=0x{4:X8}, bufWidth={5:D}, bufHeight={6:D}, bytesPerLine={7:D}, pixelFormat={8:D}", charCode, (charCode <= 0xFF ? (char) charCode : '?'), xPosI, yPosI, buffer, bufWidth, bufHeight, bytesPerLine, pixelFormat));
			}

			// If there's an internal font loaded, use it to display the text.
			// Otherwise, switch to our Debug font.
			if (!UseDebugFont)
			{
				font.fontInfo.printFont(buffer, bytesPerLine, bufWidth, bufHeight, xPosI, yPosI, xPos64 % 64, yPos64 % 64, 0, 0, bufWidth, bufHeight, pixelFormat, charCode, font.fontLib.AltCharCode, SceFontInfo.FONT_PGF_GLYPH_TYPE_CHAR, false);
			}
			else
			{
				// Font adjustment.
				// TODO: Instead of using the loaded PGF, figure out
				// the proper values for the Debug font.
				yPosI -= font.pgf.MaxBaseYAdjust >> 6;
				yPosI += font.pgf.MaxTopYAdjust >> 6;

				Debug.printFontbuffer(buffer, bytesPerLine, bufWidth, bufHeight, xPosI, yPosI, pixelFormat, charCode, font.fontLib.AltCharCode);
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x099EF33C, version = 150, checkInsideInterrupt = false) public int sceFontFindOptimumFont(int fontLibHandle, pspsharp.HLE.kernel.types.pspFontStyle fontStyle, @CanBeNull pspsharp.HLE.TErrorPointer32 errorCodePtr)
		[HLEFunction(nid : 0x099EF33C, version : 150, checkInsideInterrupt : false)]
		public virtual int sceFontFindOptimumFont(int fontLibHandle, pspFontStyle fontStyle, TErrorPointer32 errorCodePtr)
		{
			if (fontStyle.Empty)
			{
				// Always return the first font entry if no criteria is specified for the fontStyle
				return 0;
			}

			Font optimumFont = getOptimumFont(fontStyle);

			// No font found for the given style, try to find a font without the given font style (bold, italic...)
			if (optimumFont == null && fontStyle.fontStyle != 0)
			{
				fontStyle.fontStyle = 0;
				fontStyle.fontStyleSub = 0;
				optimumFont = getOptimumFont(fontStyle);
			}

			// No font found for the given style, try to find a font without the given font size.
			if (optimumFont == null && (fontStyle.fontH != 0f || fontStyle.fontV != 0f))
			{
				fontStyle.fontH = 0f;
				fontStyle.fontV = 0f;
				optimumFont = getOptimumFont(fontStyle);
			}

			// No font found for the given style, try to find a font without the given country.
			if (optimumFont == null && (fontStyle.fontCountry != 0))
			{
				fontStyle.fontCountry = 0;
				optimumFont = getOptimumFont(fontStyle);
			}

			int index = getFontIndex(optimumFont);
			if (index < 0)
			{
				// optimum font not found, assume font at index 0?
				index = 0;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceFontFindOptimumFont found font at index {0:D}: {1}", index, optimumFont));
			}

			return index;
		}

		[HLEFunction(nid : 0x3AEA8CB6, version : 150, checkInsideInterrupt : true)]
		public virtual int sceFontClose(int fontHandle)
		{
			Font font = fontsMap[fontHandle];

			if (font != null && font.fontLib != null)
			{
				font.fontLib.closeFont(font);
			}
			else
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceFontClose font already closed font={0}", font));
				}
			}

			return 0;
		}

		[HLEFunction(nid : 0x574B6FBC, version : 150, checkInsideInterrupt : true)]
		public virtual int sceFontDoneLib(int fontLibHandle)
		{
			FontLib fontLib = fontLibsMap[fontLibHandle];

			if (fontLib != null)
			{
				// Free all reserved font lib space and close all open font files.
				fontLib.triggerCloseCallback();
				fontLib.done();
				fontLibsMap.Remove(fontLibHandle);
				HLEUidObjectMapping.removeObject(fontLib);
			}
			else
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceFontDoneLib font lib already done 0x{0:X8}", fontLibHandle));
				}
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA834319D, version = 150, checkInsideInterrupt = false) public int sceFontOpen(int fontLibHandle, int index, int mode, @CanBeNull pspsharp.HLE.TErrorPointer32 errorCodePtr)
		[HLEFunction(nid : 0xA834319D, version : 150, checkInsideInterrupt : false)]
		public virtual int sceFontOpen(int fontLibHandle, int index, int mode, TErrorPointer32 errorCodePtr)
		{
			FontLib fontLib = getFontLib(fontLibHandle);

			if (index < 0)
			{
				errorCodePtr.Value = SceKernelErrors.ERROR_FONT_INVALID_PARAMETER;
				return 0;
			}

			Font font = fontLib.openFont(internalFonts[index], mode, true);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Opening '{0}' - '{1}', font={2}", font.pgf.FontName, font.pgf.FontType, font));
			}
			errorCodePtr.Value = 0;

			return font.Handle;
		}

		[HLEFunction(nid : 0xCA1E6945, version : 150, checkInsideInterrupt : true, stackUsage : 0x120)]
		public virtual int sceFontGetCharGlyphImage_Clip(int fontHandle, int charCode, TPointer glyphImagePtr, int clipXPos, int clipYPos, int clipWidth, int clipHeight)
		{
			charCode &= 0xffff;
			Font font = getFont(fontHandle, false);
			// Identical to sceFontGetCharGlyphImage, but uses a clipping
			// rectangle over the char.

			// Read GlyphImage data.
			int pixelFormat = glyphImagePtr.getValue32(0);
			int xPos64 = glyphImagePtr.getValue32(4);
			int yPos64 = glyphImagePtr.getValue32(8);
			int bufWidth = glyphImagePtr.getValue16(12);
			int bufHeight = glyphImagePtr.getValue16(14);
			int bytesPerLine = glyphImagePtr.getValue16(16);
			int buffer = glyphImagePtr.getValue32(20);

			// 26.6 fixed-point.
			int xPosI = xPos64 >> 6;
			int yPosI = yPos64 >> 6;

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceFontGetCharGlyphImage_Clip charCode={0:X4} ({1}), xPos={2:D}({3:D}), yPos={4:D}({5:D}), buffer=0x{6:X8}, bufWidth={7:D}, bufHeight={8:D}, bytesPerLine={9:D}, pixelFormat={10:D}", charCode, (charCode <= 0xFF ? (char) charCode : '?'), xPosI, xPos64, yPosI, yPos64, buffer, bufWidth, bufHeight, bytesPerLine, pixelFormat));
			}

			// If there's an internal font loaded, use it to display the text.
			// Otherwise, switch to our Debug font.
			if (!UseDebugFont)
			{
				font.fontInfo.printFont(buffer, bytesPerLine, bufWidth, bufHeight, xPosI, yPosI, xPos64 % 64, yPos64 % 64, clipXPos, clipYPos, clipWidth, clipHeight, pixelFormat, charCode, font.fontLib.AltCharCode, SceFontInfo.FONT_PGF_GLYPH_TYPE_CHAR, false);
			}
			else
			{
				// Font adjustment.
				// TODO: Instead of using the loaded PGF, figure out
				// the proper values for the Debug font.
				yPosI -= font.pgf.MaxBaseYAdjust >> 6;
				yPosI += font.pgf.MaxTopYAdjust >> 6;
				if (yPosI < 0)
				{
					yPosI = 0;
				}
				Debug.printFontbuffer(buffer, bytesPerLine, bufWidth, bufHeight, xPosI, yPosI, pixelFormat, charCode, font.fontLib.AltCharCode);
			}
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x27F6E642, version = 150, checkInsideInterrupt = true) public int sceFontGetNumFontList(int fontLibHandle, @CanBeNull pspsharp.HLE.TErrorPointer32 errorCodePtr)
		[HLEFunction(nid : 0x27F6E642, version : 150, checkInsideInterrupt : true)]
		public virtual int sceFontGetNumFontList(int fontLibHandle, TErrorPointer32 errorCodePtr)
		{
			// Get all the available fonts
			int numFonts = internalFonts.Count;
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceFontGetNumFontList returning {0:D}", numFonts));
			}
			errorCodePtr.Value = 0;

			return numFonts;
		}

		[HLEFunction(nid : 0xBC75D85B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceFontGetFontList(int fontLibHandle, TPointer fontStylePtr, int numFonts)
		{
			int fontsNum = System.Math.Min(internalFonts.Count, numFonts);
			for (int i = 0; i < fontsNum; i++)
			{
				Font font = internalFonts[i];
				pspFontStyle fontStyle = font.FontStyle;
				fontStyle.write(fontStylePtr, i * fontStyle.@sizeof());
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceFontGetFontList returning font #{0:D} at 0x{1:X8}: {2}", i, fontStylePtr.Address + i * fontStyle.@sizeof(), fontStyle));
				}
			}

			return 0;
		}

		[HLEFunction(nid : 0xEE232411, version : 150, checkInsideInterrupt : true)]
		public virtual int sceFontSetAltCharacterCode(int fontLibHandle, int charCode)
		{
			FontLib fontLib = getFontLib(fontLibHandle);
			charCode &= 0xffff;
			fontLib.AltCharCode = charCode;

			return 0;
		}

		[HLEFunction(nid : 0x5C3E4A9E, version : 150, checkInsideInterrupt : true)]
		public virtual int sceFontGetCharImageRect(int fontHandle, int charCode, TPointer16 charRectPtr)
		{
			charCode &= 0xffff;
			Font font = getFont(fontHandle, false);
			pspCharInfo charInfo = font.fontInfo.getCharInfo(charCode, SceFontInfo.FONT_PGF_GLYPH_TYPE_CHAR);

			// This function retrieves the dimensions of a specific char.
			if (charInfo != null)
			{
				charRectPtr.setValue(0, charInfo.bitmapWidth);
				charRectPtr.setValue(2, charInfo.bitmapHeight);
			}
			else
			{
				charRectPtr.setValue(0, 0);
				charRectPtr.setValue(2, 0);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x472694CD, version = 150) public float sceFontPointToPixelH(int fontLibHandle, float fontPointsH, @CanBeNull pspsharp.HLE.TErrorPointer32 errorCodePtr)
		[HLEFunction(nid : 0x472694CD, version : 150)]
		public virtual float sceFontPointToPixelH(int fontLibHandle, float fontPointsH, TErrorPointer32 errorCodePtr)
		{
			FontLib fontLib = getFontLib(fontLibHandle);
			errorCodePtr.Value = 0;

			return fontPointsH * fontLib.fontHRes / pointDPI;
		}

		[HLEFunction(nid : 0x5333322D, version : 150, checkInsideInterrupt : true)]
		public virtual int sceFontGetFontInfoByIndexNumber(int fontLibHandle, TPointer fontStylePtr, int fontIndex)
		{
			// It says FontInfo but it means Style - this is like sceFontGetFontList().
			getFontLib(fontLibHandle);
			if (fontIndex < 0 || fontIndex >= internalFonts.Count)
			{
				return ERROR_FONT_INVALID_PARAMETER;
			}
			Font font = internalFonts[fontIndex];
			pspFontStyle fontStyle = font.FontStyle;
			fontStyle.write(fontStylePtr);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceFontGetFontInfoByIndexNumber returning font #{0:D} at {1}: {2}", fontIndex, fontStylePtr, fontStyle));
			}

			return 0;
		}

		[HLEFunction(nid : 0x48293280, version : 150, checkInsideInterrupt : true)]
		public virtual int sceFontSetResolution(int fontLibHandle, float hRes, float vRes)
		{
			FontLib fontLib = getFontLib(fontLibHandle);
			fontLib.fontHRes = hRes;
			fontLib.fontVRes = vRes;

			return 0;
		}

		[HLEFunction(nid : 0x02D7F94B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceFontFlush(int fontHandle)
		{
			Font font = getFont(fontHandle, false);
			font.fontLib.flushFont(font);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x681E61A7, version = 150, checkInsideInterrupt = true) public int sceFontFindFont(int fontLibHandle, pspsharp.HLE.kernel.types.pspFontStyle fontStyle, @CanBeNull pspsharp.HLE.TErrorPointer32 errorCodePtr)
		[HLEFunction(nid : 0x681E61A7, version : 150, checkInsideInterrupt : true)]
		public virtual int sceFontFindFont(int fontLibHandle, pspFontStyle fontStyle, TErrorPointer32 errorCodePtr)
		{
			errorCodePtr.Value = 0;
			int fontsNum = internalFonts.Count;
			for (int i = 0; i < fontsNum; i++)
			{
				if (isFontMatchingStyle(internalFonts[i], fontStyle, false))
				{
					return i;
				}
			}

			return -1;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x3C4B7E82, version = 150) public float sceFontPointToPixelV(int fontLibHandle, float fontPointsV, @CanBeNull pspsharp.HLE.TErrorPointer32 errorCodePtr)
		[HLEFunction(nid : 0x3C4B7E82, version : 150)]
		public virtual float sceFontPointToPixelV(int fontLibHandle, float fontPointsV, TErrorPointer32 errorCodePtr)
		{
			FontLib fontLib = getFontLib(fontLibHandle);
			errorCodePtr.Value = 0;

			return fontPointsV * fontLib.fontVRes / pointDPI;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x74B21701, version = 150) public float sceFontPixelToPointH(int fontLibHandle, float fontPixelsH, @CanBeNull pspsharp.HLE.TErrorPointer32 errorCodePtr)
		[HLEFunction(nid : 0x74B21701, version : 150)]
		public virtual float sceFontPixelToPointH(int fontLibHandle, float fontPixelsH, TErrorPointer32 errorCodePtr)
		{
			FontLib fontLib = getFontLib(fontLibHandle);
			errorCodePtr.Value = 0;

			return fontPixelsH * pointDPI / fontLib.fontHRes;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xF8F0752E, version = 150) public float sceFontPixelToPointV(int fontLibHandle, float fontPixelsV, @CanBeNull pspsharp.HLE.TErrorPointer32 errorCodePtr)
		[HLEFunction(nid : 0xF8F0752E, version : 150)]
		public virtual float sceFontPixelToPointV(int fontLibHandle, float fontPixelsV, TErrorPointer32 errorCodePtr)
		{
			FontLib fontLib = getFontLib(fontLibHandle);
			errorCodePtr.Value = 0;

			// Convert vertical pixels to floating points (Pixels Per Inch to Points Per Inch).
			// points = (pixels / dpiX) * 72.
			return fontPixelsV * pointDPI / fontLib.fontVRes;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2F67356A, version = 150) public int sceFontCalcMemorySize()
		[HLEFunction(nid : 0x2F67356A, version : 150)]
		public virtual int sceFontCalcMemorySize()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x48B06520, version = 150) public int sceFontGetShadowImageRect(int fontHandle, int charCode, pspsharp.HLE.TPointer charInfoPtr)
		[HLEFunction(nid : 0x48B06520, version : 150)]
		public virtual int sceFontGetShadowImageRect(int fontHandle, int charCode, TPointer charInfoPtr)
		{
			charCode &= 0xffff;
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x568BE516, version = 150) public int sceFontGetShadowGlyphImage(int fontHandle, int charCode, pspsharp.HLE.TPointer glyphImagePtr)
		[HLEFunction(nid : 0x568BE516, version : 150)]
		public virtual int sceFontGetShadowGlyphImage(int fontHandle, int charCode, TPointer glyphImagePtr)
		{
			charCode &= 0xffff;
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5DCF6858, version = 150) public int sceFontGetShadowGlyphImage_Clip(int fontHandle, int charCode, pspsharp.HLE.TPointer glyphImagePtr, int clipXPos, int clipYPos, int clipWidth, int clipHeight)
		[HLEFunction(nid : 0x5DCF6858, version : 150)]
		public virtual int sceFontGetShadowGlyphImage_Clip(int fontHandle, int charCode, TPointer glyphImagePtr, int clipXPos, int clipYPos, int clipWidth, int clipHeight)
		{
			charCode &= 0xffff;
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAA3DE7B5, version = 150) public int sceFontGetShadowInfo(int fontHandle, int charCode, pspsharp.HLE.TPointer charInfoPtr)
		[HLEFunction(nid : 0xAA3DE7B5, version : 150)]
		public virtual int sceFontGetShadowInfo(int fontHandle, int charCode, TPointer charInfoPtr)
		{
			charCode &= 0xffff;
			return 0;
		}
	}
}