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
//	import static pspsharp.util.Utilities.readStringNZ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUByte;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUHalf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readWord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.skipUnknown;

	using SceFontInfo = pspsharp.HLE.kernel.types.SceFontInfo;

	public class PGF
	{
		public class FontStyle
		{
			public const int FONT_FAMILY_SANS_SERIF = 1;
			public const int FONT_FAMILY_SERIF = 2;

			public const int FONT_STYLE_REGULAR = 1;
			public const int FONT_STYLE_ITALIC = 2;
			public const int FONT_STYLE_BOLD = 5;
			public const int FONT_STYLE_BOLD_ITALIC = 6;
			public const int FONT_STYLE_DB = 103; // Demi-Bold / semi-bold

			public const int FONT_LANGUAGE_JAPANESE = 1;
			public const int FONT_LANGUAGE_LATIN = 2;
			public const int FONT_LANGUAGE_KOREAN = 3;

			public float fontH;
			public float fontV;
			public float fontHRes;
			public float fontVRes;
			public float fontWeight;
			public short fontFamily;
			public short fontStyle;
			public short fontStyleSub;
			public short fontLanguage;
			public short fontRegion;
			public short fontCountry;
			public string fontName;
			public int fontAttributes;
			public int fontExpire;
		}

		public class Info
		{
			// Glyph metrics
			public int maxGlyphWidthI;
			public int maxGlyphHeightI;
			public int maxGlyphAscenderI;
			public int maxGlyphDescenderI;
			public int maxGlyphLeftXI;
			public int maxGlyphBaseYI;
			public int minGlyphCenterXI;
			public int maxGlyphTopYI;
			public int maxGlyphAdvanceXI;
			public int maxGlyphAdvanceYI;
			// Glyph metrics (replicated as float).
			public float maxGlyphWidthF;
			public float maxGlyphHeightF;
			public float maxGlyphAscenderF;
			public float maxGlyphDescenderF;
			public float maxGlyphLeftXF;
			public float maxGlyphBaseYF;
			public float minGlyphCenterXF;
			public float maxGlyphTopYF;
			public float maxGlyphAdvanceXF;
			public float maxGlyphAdvanceYF;
			// Bitmap dimensions.
			public short maxGlyphWidth;
			public short maxGlyphHeight;
			public int charMapLength; // Number of elements in the font's charmap.
			public int shadowMapLength; // Number of elements in the font's shadow charmap.
			public FontStyle fontStyle;
			public int Bpp = 4;
		}

		protected internal int headerOffset;
		protected internal int headerSize;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		protected internal string PGFMagic_Renamed;
		protected internal int revision;
		protected internal int version;

		protected internal int charMapLength;
		protected internal int charPointerLength;
		protected internal int charMapBpe;
		protected internal int charPointerBpe;

		protected internal int bpp;
		protected internal int hSize;
		protected internal int vSize;
		protected internal int hResolution;
		protected internal int vResolution;

		protected internal string fontName;
		protected internal string fontType;

		protected internal int firstGlyph;
		protected internal int lastGlyph;

		protected internal int maxAscender;
		protected internal int maxDescender;
		protected internal int maxLeftXAdjust;
		protected internal int maxBaseYAdjust;
		protected internal int minCenterXAdjust;
		protected internal int maxTopYAdjust;

		protected internal int[] maxAdvance = new int[2];
		protected internal int[] maxSize = new int[2];
		protected internal int maxGlyphWidth;
		protected internal int maxGlyphHeight;

		protected internal int dimTableLength;
		protected internal int xAdjustTableLength;
		protected internal int yAdjustTableLength;
		protected internal int advanceTableLength;

		protected internal int shadowMapLength;
		protected internal int shadowMapBpe;
		protected internal int[] shadowScale = new int[2];

		protected internal int compCharMapBpe1;
		protected internal int compCharMapLength1;
		protected internal int compCharMapBpe2;
		protected internal int compCharMapLength2;

		protected internal int[][] dimensionTable;
		protected internal int[][] xAdjustTable;
		protected internal int[][] yAdjustTable;
		protected internal int[][] charmapCompressionTable1;
		protected internal int[][] charmapCompressionTable2;
		protected internal int[][] advanceTable;
		protected internal int[] shadowCharMap;
		protected internal int[] charMap;
		protected internal int[] charPointerTable;

		protected internal int[] fontData;
		protected internal int fontDataOffset;
		protected internal int fontDataLength;

		protected internal string fileNamez = "";

		protected internal PGF()
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PGF(ByteBuffer f) throws java.io.IOException
		public PGF(ByteBuffer f)
		{
			read(f);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void read(ByteBuffer f) throws java.io.IOException
		private void read(ByteBuffer f)
		{
			if (f.capacity() == 0)
			{
				return;
			}

			// PGF Header.
			headerOffset = readUHalf(f);
			headerSize = readUHalf(f);

			// Offset 4
			PGFMagic_Renamed = readStringNZ(f, 4); // PGF0.
			revision = readWord(f);
			version = readWord(f);

			// Offset 16
			charMapLength = readWord(f);
			charPointerLength = readWord(f);
			charMapBpe = readWord(f);
			charPointerBpe = readWord(f);
			skipUnknown(f, 2);

			// Offset 34
			bpp = readUByte(f);
			skipUnknown(f, 1);

			// Offset 36
			hSize = readWord(f);
			vSize = readWord(f);
			hResolution = readWord(f);
			vResolution = readWord(f);
			skipUnknown(f, 1);

			// Offset 53
			fontName = readStringNZ(f, 64);
			fontType = readStringNZ(f, 64);
			skipUnknown(f, 1);

			// Offset 182
			firstGlyph = readUHalf(f);
			lastGlyph = readUHalf(f);
			skipUnknown(f, 26);

			// Offset 212
			maxAscender = readWord(f);
			maxDescender = readWord(f);
			maxLeftXAdjust = readWord(f);
			maxBaseYAdjust = readWord(f);
			minCenterXAdjust = readWord(f);
			maxTopYAdjust = readWord(f);

			// Offset 236
			maxAdvance[0] = readWord(f);
			maxAdvance[1] = readWord(f);
			maxSize[0] = readWord(f);
			maxSize[1] = readWord(f);
			maxGlyphWidth = readUHalf(f);
			maxGlyphHeight = readUHalf(f);
			skipUnknown(f, 2);

			// Offset 258
			dimTableLength = readUByte(f);
			xAdjustTableLength = readUByte(f);
			yAdjustTableLength = readUByte(f);
			advanceTableLength = readUByte(f);
			skipUnknown(f, 102); // NULL.

			// Offset 364
			shadowMapLength = readWord(f);
			shadowMapBpe = readWord(f);
			skipUnknown(f, 4); // 24.0625.
			shadowScale[0] = readWord(f);
			shadowScale[1] = readWord(f);
			skipUnknown(f, 8); // 15.0.

			// Offset 392
			if (revision == 3)
			{
				compCharMapBpe1 = readWord(f);
				compCharMapLength1 = readUHalf(f);
				skipUnknown(f, 2);
				compCharMapBpe2 = readWord(f);
				compCharMapLength2 = readUHalf(f);
				skipUnknown(f, 6);
			}

			// PGF Tables.
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: dimensionTable = new int[2][dimTableLength];
			dimensionTable = RectangularArrays.ReturnRectangularIntArray(2, dimTableLength);
			for (int i = 0; i < dimTableLength; i++)
			{
				dimensionTable[0][i] = readWord(f);
				dimensionTable[1][i] = readWord(f);
			}

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: xAdjustTable = new int[2][xAdjustTableLength];
			xAdjustTable = RectangularArrays.ReturnRectangularIntArray(2, xAdjustTableLength);
			for (int i = 0; i < xAdjustTableLength; i++)
			{
				xAdjustTable[0][i] = readWord(f);
				xAdjustTable[1][i] = readWord(f);
			}

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: yAdjustTable = new int[2][yAdjustTableLength];
			yAdjustTable = RectangularArrays.ReturnRectangularIntArray(2, yAdjustTableLength);
			for (int i = 0; i < yAdjustTableLength; i++)
			{
				yAdjustTable[0][i] = readWord(f);
				yAdjustTable[1][i] = readWord(f);
			}

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: advanceTable = new int[2][advanceTableLength];
			advanceTable = RectangularArrays.ReturnRectangularIntArray(2, advanceTableLength);
			for (int i = 0; i < advanceTableLength; i++)
			{
				advanceTable[0][i] = readWord(f);
				advanceTable[1][i] = readWord(f);
			}

			int shadowCharMapSize = ((shadowMapLength * shadowMapBpe + 31) & ~31) / 8;
			shadowCharMap = new int[shadowCharMapSize];
			for (int i = 0; i < shadowCharMapSize; i++)
			{
				shadowCharMap[i] = readUByte(f);
			}

			if (revision == 3)
			{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: charmapCompressionTable1 = new int[2][compCharMapLength1];
				charmapCompressionTable1 = RectangularArrays.ReturnRectangularIntArray(2, compCharMapLength1);
				for (int i = 0; i < compCharMapLength1; i++)
				{
					charmapCompressionTable1[0][i] = readUHalf(f);
					charmapCompressionTable1[1][i] = readUHalf(f);
				}

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: charmapCompressionTable2 = new int[2][compCharMapLength2];
				charmapCompressionTable2 = RectangularArrays.ReturnRectangularIntArray(2, compCharMapLength2);
				for (int i = 0; i < compCharMapLength2; i++)
				{
					charmapCompressionTable2[0][i] = readUHalf(f);
					charmapCompressionTable2[1][i] = readUHalf(f);
				}
			}

			int charMapSize = ((charMapLength * charMapBpe + 31) & ~31) / 8;
			charMap = new int[charMapSize];
			for (int i = 0; i < charMapSize; i++)
			{
				charMap[i] = readUByte(f);
			}

			int charPointerSize = (((charPointerLength * charPointerBpe + 31) & ~31) / 8);
			charPointerTable = new int[charPointerSize];
			for (int i = 0; i < charPointerSize; i++)
			{
				charPointerTable[i] = readUByte(f);
			}

			// PGF Fontdata.
			fontDataOffset = f.position();
			fontDataLength = f.capacity() - fontDataOffset;
			fontData = new int[fontDataLength];
			for (int i = 0; i < fontDataLength; i++)
			{
				fontData[i] = readUByte(f);
			}
		}

		public virtual string FileNamez
		{
			set
			{
				fileNamez = value;
			}
			get
			{
				return fileNamez;
			}
		}
		public virtual string PGFMagic
		{
			get
			{
				return PGFMagic_Renamed;
			}
		}
		public virtual int HeaderSize
		{
			get
			{
				return headerSize;
			}
		}
		public virtual int Revision
		{
			get
			{
				return revision;
			}
		}
		public virtual int Version
		{
			get
			{
				return version;
			}
		}
		public virtual string FontName
		{
			get
			{
				return fontName;
			}
		}
		public virtual string FontType
		{
			get
			{
				return fontType;
			}
		}
		public virtual int FirstGlyphInCharMap
		{
			get
			{
				return firstGlyph;
			}
		}
		public virtual int LastGlyphInCharMap
		{
			get
			{
				return lastGlyph;
			}
		}
		public virtual int MaxGlyphWidth
		{
			get
			{
				return maxGlyphWidth;
			}
		}
		public virtual int MaxGlyphHeight
		{
			get
			{
				return maxGlyphHeight;
			}
		}
		public virtual int[] MaxSize
		{
			get
			{
				return maxSize;
			}
		}
		public virtual int MaxLeftXAdjust
		{
			get
			{
				return maxLeftXAdjust;
			}
		}
		public virtual int MinCenterXAdjust
		{
			get
			{
				return minCenterXAdjust;
			}
		}
		public virtual int MaxBaseYAdjust
		{
			get
			{
				return maxBaseYAdjust;
			}
		}
		public virtual int MaxTopYAdjust
		{
			get
			{
				return maxTopYAdjust;
			}
		}
		public virtual int CharMapLength
		{
			get
			{
				return charMapLength;
			}
		}
		public virtual int CharPointerLength
		{
			get
			{
				return charPointerLength;
			}
		}
		public virtual int ShadowMapLength
		{
			get
			{
				return shadowMapLength;
			}
		}
		public virtual int CompCharMapLength
		{
			get
			{
				return compCharMapLength1 + compCharMapLength2;
			}
		}
		public virtual int CharMapBpe
		{
			get
			{
				return charMapBpe;
			}
		}
		public virtual int CharPointerBpe
		{
			get
			{
				return charPointerBpe;
			}
		}
		public virtual int ShadowMapBpe
		{
			get
			{
				return shadowMapBpe;
			}
		}
		public virtual int[] MaxAdvance
		{
			get
			{
				return maxAdvance;
			}
		}
		public virtual int[][] AdvanceTable
		{
			get
			{
				return advanceTable;
			}
		}
		public virtual int[] CharMap
		{
			get
			{
				return charMap;
			}
		}
		public virtual int[] CharPointerTable
		{
			get
			{
				return charPointerTable;
			}
		}
		public virtual int[][] CharMapCompressionTable1
		{
			get
			{
				return charmapCompressionTable1;
			}
		}
		public virtual int[][] CharMapCompressionTable2
		{
			get
			{
				return charmapCompressionTable2;
			}
		}
		public virtual int[] ShadowCharMap
		{
			get
			{
				return shadowCharMap;
			}
		}
		public virtual int[] ShadowScale
		{
			get
			{
				return shadowScale;
			}
		}
		public virtual int[] Fontdata
		{
			get
			{
				return fontData;
			}
		}

		public virtual int HSize
		{
			get
			{
				return hSize;
			}
		}

		public virtual int VSize
		{
			get
			{
				return vSize;
			}
		}

		public virtual int HResolution
		{
			get
			{
				return hResolution;
			}
		}

		public virtual int VResolution
		{
			get
			{
				return vResolution;
			}
		}

		public virtual int MaxAscender
		{
			get
			{
				return maxAscender;
			}
		}

		public virtual int MaxDescender
		{
			get
			{
				return maxDescender;
			}
		}

		public virtual int Bpp
		{
			get
			{
				return bpp;
			}
		}

		public virtual int AdvanceTableLength
		{
			get
			{
				return advanceTableLength;
			}
		}

		public virtual int[][] DimensionTable
		{
			get
			{
				return dimensionTable;
			}
		}

		public virtual int DimensionTableLength
		{
			get
			{
				return dimTableLength;
			}
		}

		public virtual int[][] XAdjustTable
		{
			get
			{
				return xAdjustTable;
			}
		}

		public virtual int XAdjustTableLength
		{
			get
			{
				return xAdjustTableLength;
			}
		}

		public virtual int[][] YAdjustTable
		{
			get
			{
				return yAdjustTable;
			}
		}

		public virtual int YAdjustTableLength
		{
			get
			{
				return yAdjustTableLength;
			}
		}

		public virtual SceFontInfo createFontInfo()
		{
			return new SceFontInfo(this);
		}
	}
}