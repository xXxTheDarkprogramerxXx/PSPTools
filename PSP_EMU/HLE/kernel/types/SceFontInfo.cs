using System;

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
	//using Logger = org.apache.log4j.Logger;

	using PGF = pspsharp.format.PGF;
	using Debug = pspsharp.util.Debug;
	using sceFont = pspsharp.HLE.modules.sceFont;

	/*
	 * SceFontInfo struct based on BenHur's intraFont application.
	 * This struct is used to give an easy and organized access to the PGF data.
	 */

	public class SceFontInfo
	{
		private static readonly Logger log = sceFont.log;

		private const bool dumpGlyphs = false;

		// Statics based on intraFont's findings.
		public const int FONT_FILETYPE_PGF = 0x00;
		public const int FONT_FILETYPE_BWFON = 0x01;
		public const int FONT_PGF_BMP_H_ROWS = 0x01;
		public const int FONT_PGF_BMP_V_ROWS = 0x02;
		public const int FONT_PGF_BMP_OVERLAY = 0x03;
		public const int FONT_PGF_METRIC_DIMENSION_INDEX = 0x04;
		public const int FONT_PGF_METRIC_BEARING_X_INDEX = 0x08;
		public const int FONT_PGF_METRIC_BEARING_Y_INDEX = 0x10;
		public const int FONT_PGF_METRIC_ADVANCE_INDEX = 0x20;
		public const int FONT_PGF_GLYPH_TYPE_CHAR = 0;
		public const int FONT_PGF_GLYPH_TYPE_SHADOW = 1;

		// PGF file.
		protected internal string fileName; // The PGF file name.
		protected internal string fileType; // The file type (only PGF support for now).
		protected internal int[] fontdata; // Fontdata extracted from the PGF.
		protected internal long fontdataBits;

		// Characters properties and glyphs.
		protected internal int advancex;
		protected internal int advancey;
		protected internal int charmap_compr_len;
		protected internal int[] charmap_compr;
		protected internal int shadow_compr_len;
		protected internal int[] shadow_compr;
		protected internal int[] charmap;
		protected internal Glyph[] glyphs;
		protected internal int firstGlyph;

		// Shadow characters properties and glyphs.
		protected internal int shadowScaleX;
		protected internal int shadowScaleY;
		protected internal Glyph[] shadowGlyphs;

		// Font style from registry
		protected internal pspFontStyle fontStyle;

		// Glyph class.
		protected internal class Glyph
		{
			protected internal int x;
			protected internal int y;
			protected internal int w;
			protected internal int h;
			protected internal int left;
			protected internal int top;
			protected internal int flags;
			protected internal int shadowID;
			protected internal int advanceH;
			protected internal int advanceV;
			protected internal int dimensionWidth, dimensionHeight;
			protected internal int xAdjustH, xAdjustV;
			protected internal int yAdjustH, yAdjustV;
			protected internal long ptr;
			// Internal debugging data
			protected internal long glyphPtr;

			public virtual bool hasFlag(int flag)
			{
				return (flags & flag) == flag;
			}

			public override string ToString()
			{
				return string.Format("Glyph[x={0:D}, y={1:D}, w={2:D}, h={3:D}, left={4:D}, top={5:D}, flags=0x{6:X}, shadowID={7:D}, advance={8:D}, ptr={9:D}]", x, y, w, h, left, top, flags, shadowID, advanceH, ptr);
			}
		}

		private int[] getTable(int[] rawTable, int bpe, int Length)
		{
			int[] table = new int[Length];
			for (int i = 0, bitPtr = 0; i < Length; i++, bitPtr += bpe)
			{
				table[i] = getBits(bpe, rawTable, bitPtr);
			}

			return table;
		}

		protected internal SceFontInfo()
		{
		}

		public SceFontInfo(PGF fontFile)
		{
			// PGF.
			fileName = fontFile.FileNamez;
			fileType = fontFile.PGFMagic;

			// Characters/Shadow characters' variables.
			charmap = new int[fontFile.CharMapLength * 2];
			advancex = fontFile.MaxAdvance[0] / 16;
			advancey = fontFile.MaxAdvance[1] / 16;
			shadowScaleX = fontFile.ShadowScale[0];
			shadowScaleY = fontFile.ShadowScale[1];
			glyphs = new Glyph[fontFile.CharPointerLength];
			shadowGlyphs = new Glyph[fontFile.ShadowMapLength];
			firstGlyph = fontFile.FirstGlyphInCharMap;

			// Charmap compression tables
			int[][] charmapCompressionTable1 = fontFile.CharMapCompressionTable1;
			int[][] charmapCompressionTable2 = fontFile.CharMapCompressionTable2;
			if (charmapCompressionTable1 == null || charmapCompressionTable2 == null)
			{
				charmap_compr_len = 1;
				charmap_compr = new int[2];
				charmap_compr[0] = firstGlyph;
				charmap_compr[1] = fontFile.LastGlyphInCharMap - firstGlyph + 1;

				shadow_compr_len = 1;
				shadow_compr = new int[2];
				shadow_compr[0] = charmap_compr[0];
				shadow_compr[1] = charmap_compr[1];
			}
			else
			{
				charmap_compr_len = charmapCompressionTable1[0].Length;
				charmap_compr = new int[charmap_compr_len * 2];
				for (int j = 0, i = 0; j < charmapCompressionTable1[0].Length; j++)
				{
					charmap_compr[i++] = charmapCompressionTable1[0][j];
					charmap_compr[i++] = charmapCompressionTable1[1][j];
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("CharMap Compression Table #{0:D}: 0x{1:X}, Length={2:D}", j, charmapCompressionTable1[0][j], charmapCompressionTable1[1][j]));
					}
				}

				shadow_compr_len = charmapCompressionTable2[0].Length;
				shadow_compr = new int[shadow_compr_len * 2];
				for (int j = 0, i = 0; j < charmapCompressionTable2[0].Length; j++)
				{
					shadow_compr[i++] = charmapCompressionTable2[0][j];
					shadow_compr[i++] = charmapCompressionTable2[1][j];
				}
			}

			// Get char map.
			int[] rawCharMap = fontFile.CharMap;
			for (int i = 0; i < fontFile.CharMapLength; i++)
			{
				charmap[i] = getBits(fontFile.CharMapBpe, rawCharMap, i * fontFile.CharMapBpe);
				if (charmap[i] >= glyphs.Length)
				{
					charmap[i] = 65535;
				}
			}

			// Get raw fontdata.
			fontdata = fontFile.Fontdata;
			fontdataBits = fontdata.Length * 8L;

			int[] charPointers = getTable(fontFile.CharPointerTable, fontFile.CharPointerBpe, glyphs.Length);
			int[] shadowMap = getTable(fontFile.ShadowCharMap, fontFile.ShadowMapBpe, shadowGlyphs.Length);

			// Generate glyphs for all chars.
			for (int i = 0; i < glyphs.Length; i++)
			{
				glyphs[i] = getGlyph(fontdata, (charPointers[i] * 4 * 8), FONT_PGF_GLYPH_TYPE_CHAR, fontFile);
			}

			// Generate shadow glyphs for all chars.
			if (shadowGlyphs.Length > 0)
			{
				for (int i = 0; i < glyphs.Length; i++)
				{
					int shadowId = glyphs[i].shadowID;
					if (shadowId >= 0 && shadowId < shadowMap.Length && shadowId < shadowGlyphs.Length)
					{
						int charId = getCharID(shadowMap[shadowId]);
						if (charId >= 0 && charId < glyphs.Length)
						{
							if (shadowGlyphs[shadowId] == null)
							{
								shadowGlyphs[shadowId] = getGlyph(fontdata, (charPointers[charId] * 4 * 8), FONT_PGF_GLYPH_TYPE_SHADOW, fontFile);
							}
						}
					}
				}
			}

			// Dump all glyphs for debugging purpose
			if (dumpGlyphs && log.TraceEnabled)
			{
				for (int i = 0; i < glyphs.Length; i++)
				{
					int endPtr = (int)(i + 1 < glyphs.Length ? glyphs[i + 1].glyphPtr : glyphs[i].ptr);
					log.trace(string.Format("charCode=0x{0:X4}: 0x{1:X}-0x{2:X}-0x{3:X}", getCharCode(i, charmap_compr), glyphs[i].glyphPtr, glyphs[i].ptr, endPtr));
					for (int j = (int) glyphs[i].ptr; j < endPtr; j++)
					{
						log.trace(string.Format("  0x{0:X}: 0x{1:X2}", j, fontdata[j]));
					}
				}
				for (int i = 0; i < shadowGlyphs.Length; i++)
				{
					if (shadowGlyphs[i] != null)
					{
						log.trace(string.Format("shadowGlyphs#{0:D}: 0x{1:X}-0x{2:X}", i, shadowGlyphs[i].glyphPtr, shadowGlyphs[i].ptr));
					}
				}
			}
		}

		// Retrieve bits from a byte buffer based on bpe.
		private int getBits(int bpe, int[] buf, long pos)
		{
			int v = 0;
			for (int i = 0; i < bpe; i++)
			{
				v += (((buf[(int)(pos / 8)] >> ((pos) % 8)) & 1) << i);
				pos++;
			}
			return v;
		}

		private bool isIncorrectFont(PGF fontFile)
		{
			// Fonts created by ttf2pgf (e.g. default pspsharp fonts)
			// do not contain complete Glyph information.
			string fontName = fontFile.FontName;
			return fontName.StartsWith("Liberation", StringComparison.Ordinal) || fontName.StartsWith("Sazanami", StringComparison.Ordinal) || fontName.StartsWith("UnDotum", StringComparison.Ordinal);
		}

		// Create and retrieve a glyph from the font data.
		private Glyph getGlyph(int[] fontdata, long charPtr, int glyphType, PGF fontFile)
		{
			Glyph glyph = new Glyph();

			if (glyphType == FONT_PGF_GLYPH_TYPE_SHADOW)
			{
				if (charPtr + 96 > fontdataBits)
				{
					return null;
				}
				// First 14 bits are offset to shadow glyph
				charPtr += getBits(14, fontdata, charPtr) * 8;
			}
			if (charPtr + 96 > fontdataBits)
			{
				return null;
			}

			glyph.glyphPtr = charPtr / 8;

			charPtr += 14;

			glyph.w = getBits(7, fontdata, charPtr);
			charPtr += 7;

			glyph.h = getBits(7, fontdata, charPtr);
			charPtr += 7;

			glyph.left = getBits(7, fontdata, charPtr);
			charPtr += 7;
			if (glyph.left >= 64)
			{
				glyph.left -= 128;
			}

			glyph.top = getBits(7, fontdata, charPtr);
			charPtr += 7;
			if (glyph.top >= 64)
			{
				glyph.top -= 128;
			}

			glyph.flags = getBits(6, fontdata, charPtr);
			charPtr += 6;

			if (glyphType == FONT_PGF_GLYPH_TYPE_CHAR)
			{
				// Unknown values
				int unknown1 = getBits(2, fontdata, charPtr);
				charPtr += 2;
				int unknown2 = getBits(2, fontdata, charPtr);
				charPtr += 2;
				int unknown3 = getBits(3, fontdata, charPtr);
				charPtr += 3;
				if (log.TraceEnabled)
				{
					log.trace(string.Format("unknown1={0:D}, unknown2={1:D}, unknown3={2:D}", unknown1, unknown2, unknown3));
				}

				glyph.shadowID = getBits(9, fontdata, charPtr);
				charPtr += 9;

				if (glyph.hasFlag(FONT_PGF_METRIC_DIMENSION_INDEX))
				{
					int dimensionIndex = getBits(8, fontdata, charPtr);
					charPtr += 8;
					if (dimensionIndex < fontFile.DimensionTableLength)
					{
						int[][] dimensionTable = fontFile.DimensionTable;
						glyph.dimensionWidth = dimensionTable[0][dimensionIndex];
						glyph.dimensionHeight = dimensionTable[1][dimensionIndex];
					}

					if (dimensionIndex == 0 && isIncorrectFont(fontFile))
					{
						// Fonts created by ttf2pgf do not contain complete Glyph information.
						// Provide default values.
						glyph.dimensionWidth = glyph.w << 6;
						glyph.dimensionHeight = glyph.h << 6;
					}
				}
				else
				{
					glyph.dimensionWidth = getBits(32, fontdata, charPtr);
					charPtr += 32;
					glyph.dimensionHeight = getBits(32, fontdata, charPtr);
					charPtr += 32;
				}

				if (glyph.hasFlag(FONT_PGF_METRIC_BEARING_X_INDEX))
				{
					int xAdjustIndex = getBits(8, fontdata, charPtr);
					charPtr += 8;
					if (xAdjustIndex < fontFile.XAdjustTableLength)
					{
						int[][] xAdjustTable = fontFile.XAdjustTable;
						glyph.xAdjustH = xAdjustTable[0][xAdjustIndex];
						glyph.xAdjustV = xAdjustTable[1][xAdjustIndex];
					}

					if (xAdjustIndex == 0 && isIncorrectFont(fontFile))
					{
						// Fonts created by ttf2pgf do not contain complete Glyph information.
						// Provide default values.
						glyph.xAdjustH = glyph.left << 6;
						glyph.xAdjustV = glyph.left << 6;
					}
				}
				else
				{
					glyph.xAdjustH = getBits(32, fontdata, charPtr);
					charPtr += 32;
					glyph.xAdjustV = getBits(32, fontdata, charPtr);
					charPtr += 32;
				}

				if (glyph.hasFlag(FONT_PGF_METRIC_BEARING_Y_INDEX))
				{
					int yAdjustIndex = getBits(8, fontdata, charPtr);
					charPtr += 8;
					if (yAdjustIndex < fontFile.YAdjustTableLength)
					{
						int[][] yAdjustTable = fontFile.YAdjustTable;
						glyph.yAdjustH = yAdjustTable[0][yAdjustIndex];
						glyph.yAdjustV = yAdjustTable[1][yAdjustIndex];
					}

					if (yAdjustIndex == 0 && isIncorrectFont(fontFile))
					{
						// Fonts created by ttf2pgf do not contain complete Glyph information.
						// Provide default values.
						glyph.yAdjustH = glyph.top << 6;
						glyph.yAdjustV = glyph.top << 6;
					}
				}
				else
				{
					glyph.yAdjustH = getBits(32, fontdata, charPtr);
					charPtr += 32;
					glyph.yAdjustV = getBits(32, fontdata, charPtr);
					charPtr += 32;
				}

				if (glyph.hasFlag(FONT_PGF_METRIC_ADVANCE_INDEX))
				{
					int advanceIndex = getBits(8, fontdata, charPtr);
					charPtr += 8;
					if (advanceIndex < fontFile.AdvanceTableLength)
					{
						int[][] advanceTable = fontFile.AdvanceTable;
						glyph.advanceH = advanceTable[0][advanceIndex];
						glyph.advanceV = advanceTable[1][advanceIndex];
					}
				}
				else
				{
					glyph.advanceH = getBits(32, fontdata, charPtr);
					charPtr += 32;
					glyph.advanceV = getBits(32, fontdata, charPtr);
					charPtr += 32;
				}
			}
			else
			{
				glyph.shadowID = 65535;
				glyph.advanceH = 0;
			}

			glyph.ptr = charPtr / 8;

			return glyph;
		}

		private int getCharID(int charCode)
		{
			// TODO Implement compressed charmap (PGF revision 3)
			charCode -= firstGlyph;
			if (charCode >= 0 && charCode < charmap.Length)
			{
				charCode = charmap[charCode];
			}

			return charCode;
		}

		private Glyph getCharGlyph(int charCode, int glyphType)
		{
			if (charCode < firstGlyph)
			{
				return null;
			}

			int charIndex = getCharIndex(charCode, charmap_compr);
			if (charIndex < 0 || charIndex >= glyphs.Length)
			{
				return null;
			}

			Glyph glyph = glyphs[charIndex];

			if (log.TraceEnabled)
			{
				log.trace(string.Format("charCode=0x{0:X4} mapped to glyph#{1:D}", charCode, charIndex));
			}

			if (glyphType == FONT_PGF_GLYPH_TYPE_SHADOW)
			{
				int shadowID = glyph.shadowID;
				if (shadowID < 0 || shadowID >= shadowGlyphs.Length)
				{
					return null;
				}
				glyph = shadowGlyphs[shadowID];
			}

			return glyph;
		}

		private int sample(int[][] bitmap, int x, int y, int factor4096)
		{
			if (factor4096 == 0 || y < 0 || x < 0 || y >= bitmap.Length || x >= bitmap[y].Length)
			{
				return 0;
			}

			return bitmap[y][x] * factor4096;
		}

		private int sample(int[][] bitmap, int x, int y, int x64, int y64)
		{
			int color = 0;

			// PSP is not interpolating on the y-axis, i.e. y64 is ignored
			color += sample(bitmap, x, y, 64 - x64);
			color += sample(bitmap, x - 1, y, x64);

			// This seems to be the rule used by the PSP to round up or down
			if (color >= 0x1E0)
			{
				// Round up
				color += 0x20;
			}
			color = color >> 6;

			return color;
		}

		private void generateFontTexture(int @base, int bpl, int bufWidth, int bufHeight, int x, int y, int x64, int y64, int clipX, int clipY, int clipWidth, int clipHeight, int pixelformat, Glyph glyph, int glyphType, bool addColor)
		{
			if (((glyph.flags & FONT_PGF_BMP_OVERLAY) != FONT_PGF_BMP_H_ROWS) && ((glyph.flags & FONT_PGF_BMP_OVERLAY) != FONT_PGF_BMP_V_ROWS))
			{
				return;
			}

			long bitPtr = glyph.ptr * 8;
			const int nibbleBits = 4;
			bool bitmapHorizontalRows = (glyph.flags & FONT_PGF_BMP_OVERLAY) == FONT_PGF_BMP_H_ROWS;
			int numberPixels = glyph.w * glyph.h;

			int scaleX = 1;
			int scaleY = 1;
			if (glyphType == FONT_PGF_GLYPH_TYPE_SHADOW)
			{
				scaleX = 64 / shadowScaleX;
				scaleY = 64 / shadowScaleY;
			}

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] bitmap = new int[glyph.h][glyph.w];
			int[][] bitmap = RectangularArrays.ReturnRectangularIntArray(glyph.h, glyph.w);
			int pixelIndex = 0;
			while (pixelIndex < numberPixels && bitPtr + 8 < fontdataBits)
			{
				int nibble = getBits(nibbleBits, fontdata, bitPtr);
				bitPtr += nibbleBits;

				int count;
				int value = 0;
				if (nibble < 8)
				{
					value = getBits(nibbleBits, fontdata, bitPtr);
					bitPtr += nibbleBits;
					count = nibble + 1;
				}
				else
				{
					count = 16 - nibble;
				}

				for (int i = 0; i < count && pixelIndex < numberPixels; i++)
				{
					if (nibble >= 8)
					{
						value = getBits(nibbleBits, fontdata, bitPtr);
						bitPtr += nibbleBits;
					}

					int xx, yy;
					if (bitmapHorizontalRows)
					{
						xx = pixelIndex % glyph.w;
						yy = pixelIndex / glyph.w;
					}
					else
					{
						xx = pixelIndex / glyph.h;
						yy = pixelIndex % glyph.h;
					}
					bitmap[yy][xx] = value;

					pixelIndex++;
				}
			}

			for (int yy = 0; yy <= glyph.h; yy++)
			{
				for (int xx = 0; xx <= glyph.w; xx++)
				{
					int sample = this.sample(bitmap, xx, yy, x64, y64);
					// A pixel with value 0 is not changing the value in the buffer (tested on PSP)
					if (sample != 0)
					{
						int pixelX = x + xx * scaleX;
						int pixelY = y + yy * scaleY;
						if (pixelX >= clipX && pixelX < clipX + clipWidth && pixelY >= clipY && pixelY < clipY + clipHeight)
						{
							// 4-bit color value
							int pixelColor = sample;
							switch (pixelformat)
							{
								case sceFont.PSP_FONT_PIXELFORMAT_8:
									// 8-bit color value
									pixelColor |= pixelColor << 4;
									break;
								case sceFont.PSP_FONT_PIXELFORMAT_24:
									// 24-bit color value
									pixelColor |= pixelColor << 4;
									pixelColor |= pixelColor << 8;
									pixelColor |= pixelColor << 8;
									break;
								case sceFont.PSP_FONT_PIXELFORMAT_32:
									// 32-bit color value
									pixelColor |= pixelColor << 4;
									pixelColor |= pixelColor << 8;
									pixelColor |= pixelColor << 16;
									break;
							}

							for (int yyy = 0; yyy < scaleY; yyy++)
							{
								for (int xxx = 0; xxx < scaleX; xxx++)
								{
									if (addColor)
									{
										Debug.addFontPixel(@base, bpl, bufWidth, bufHeight, pixelX + xxx, pixelY + yyy, pixelColor, pixelformat);
									}
									else
									{
										Debug.setFontPixel(@base, bpl, bufWidth, bufHeight, pixelX + xxx, pixelY + yyy, pixelColor, pixelformat);
									}
								}
							}
						}
					}
				}
			}
		}

		// Generate a 4bpp texture for the given char id.
		private void generateFontTexture(int @base, int bpl, int bufWidth, int bufHeight, int x, int y, int x64, int y64, int clipX, int clipY, int clipWidth, int clipHeight, int pixelformat, int charCode, int altCharCode, int glyphType, bool addColor)
		{
			Glyph glyph = getCharGlyph(charCode, glyphType);
			if (glyph == null)
			{
				// No Glyph available for this charCode, try to use the alternate char.
				charCode = altCharCode;
				glyph = getCharGlyph(charCode, glyphType);
				if (glyph == null)
				{
					return;
				}
			}

			if (glyph.w <= 0 || glyph.h <= 0)
			{
				return;
			}

			// Overlay glyph?
			if ((glyph.flags & FONT_PGF_BMP_OVERLAY) == FONT_PGF_BMP_OVERLAY)
			{
				if (!addColor)
				{
					// First clear the bitmap area of the main glyph.
					// Sub-glyphs will just add colors.
					int pixelColor = 0; // Set to black
					for (int pixelY = 0; pixelY < glyph.h; pixelY++)
					{
						for (int pixelX = 0; pixelX < glyph.w; pixelX++)
						{
							Debug.setFontPixel(@base, bpl, bufWidth, bufHeight, x + pixelX, y + pixelY, pixelColor, pixelformat);
						}
					}
				}

				// 1 to 3 sub-glyphs can be mixed together to form the main glyph
				for (int i = 0; i < 3; i++)
				{
					// The character code of each sub-glyph is stored into the glyph font data
					int subCharCode = fontdata[(int) glyph.ptr + i * 2] | (fontdata[(int) glyph.ptr + i * 2 + 1] << 8);
					if (subCharCode != 0)
					{
						if (log.TraceEnabled)
						{
							log.trace(string.Format("generateFontTexture charCode=0x{0:X4}, subCharCode=0x{1:X4} (@ptr=0x{2:X})", charCode, subCharCode, glyph.ptr + i * 2));
						}

						Glyph subGlyph = getCharGlyph(subCharCode, glyphType);
						if (subGlyph != null)
						{
							// Draw the sub-glyph relative to the main glyph
							int left = subGlyph.left - glyph.left;
							int top = glyph.top - subGlyph.top;

							generateFontTexture(@base, bpl, bufWidth, bufHeight, x + left, y + top, x64, y64, clipX, clipY, clipWidth, clipHeight, pixelformat, subGlyph, glyphType, true);
						}
					}
				}
			}
			else
			{
				// Regular glyph rendering
				generateFontTexture(@base, bpl, bufWidth, bufHeight, x, y, x64, y64, clipX, clipY, clipWidth, clipHeight, pixelformat, glyph, glyphType, addColor);
			}
		}

		public virtual void printFont(int @base, int bpl, int bufWidth, int bufHeight, int x, int y, int x64, int y64, int clipX, int clipY, int clipWidth, int clipHeight, int pixelformat, int charCode, int altCharCode, int glyphType, bool addColor)
		{
			generateFontTexture(@base, bpl, bufWidth, bufHeight, x, y, x64, y64, clipX, clipY, clipWidth, clipHeight, pixelformat, charCode, altCharCode, glyphType, addColor);
		}

		public virtual pspCharInfo getCharInfo(int charCode, int glyphType)
		{
			pspCharInfo charInfo = new pspCharInfo();
			Glyph glyph = getCharGlyph(charCode, glyphType);
			if (glyph == null)
			{
				// For a character not present in the font, return pspCharInfo with all fields set to 0.
				// Confirmed on a PSP.
				return charInfo;
			}

			charInfo.bitmapWidth = glyph.w;
			charInfo.bitmapHeight = glyph.h;
			charInfo.bitmapLeft = glyph.left;
			charInfo.bitmapTop = glyph.top;
			charInfo.sfp26Width = glyph.dimensionWidth;
			charInfo.sfp26Height = glyph.dimensionHeight;
			// TODO Test if the Ascender and Descender values are now matching the PSP
			charInfo.sfp26Ascender = glyph.yAdjustH;
			charInfo.sfp26Descender = charInfo.sfp26Ascender - charInfo.sfp26Height;
			charInfo.sfp26BearingHX = glyph.xAdjustH;
			charInfo.sfp26BearingHY = glyph.yAdjustH;
			charInfo.sfp26BearingVX = glyph.xAdjustV;
			charInfo.sfp26BearingVY = glyph.yAdjustV;
			charInfo.sfp26AdvanceH = glyph.advanceH;
			charInfo.sfp26AdvanceV = glyph.advanceV;

			if (glyphType == FONT_PGF_GLYPH_TYPE_SHADOW)
			{
				int scaleX = 64 / shadowScaleX;
				int scaleY = 64 / shadowScaleY;
				charInfo.bitmapWidth *= scaleX;
				charInfo.bitmapHeight *= scaleY;
				charInfo.bitmapLeft *= scaleX;
				charInfo.bitmapTop *= scaleY;
				charInfo.sfp26Ascender *= scaleY;
				charInfo.sfp26Descender *= scaleY;
			}

			return charInfo;
		}

		public virtual pspFontStyle FontStyle
		{
			get
			{
				return fontStyle;
			}
			set
			{
				this.fontStyle = value;
			}
		}


		private int getCharCode(int charIndex, int[] charmapCompressed)
		{
			for (int i = 0; i < charmapCompressed.Length; i += 2)
			{
				if (charIndex > charmapCompressed[i + 1])
				{
					charIndex -= charmapCompressed[i + 1];
				}
				else
				{
					return charmapCompressed[i] + charIndex;
				}
			}

			return -1;
		}

		public virtual int getCharIndex(int charCode, int[] charmapCompressed)
		{
			int charIndex = 0;
			for (int i = 0; i < charmapCompressed.Length; i += 2)
			{
				if (charCode >= charmapCompressed[i] && charCode < charmapCompressed[i] + charmapCompressed[i + 1])
				{
					charIndex += charCode - charmapCompressed[i];

					if (charmap != null && charIndex >= 0 && charIndex < charmap.Length)
					{
						charIndex = charmap[charIndex];
					}

					return charIndex;
				}
				charIndex += charmapCompressed[i + 1];
			}

			return -1;
		}
	}
}