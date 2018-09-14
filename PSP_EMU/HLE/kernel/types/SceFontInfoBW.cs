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
//	import static pspsharp.format.BWFont.charBitmapBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.BWFont.charBitmapHeight;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.BWFont.charBitmapWidth;
	using BWFont = pspsharp.format.BWFont;
	using Debug = pspsharp.util.Debug;

	/// <summary>
	/// BW font file format.
	/// Based on
	///    https://github.com/GeeckoDev/intraFont-G/blob/master/intraFont.c
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class SceFontInfoBW : SceFontInfo
	{
		private short[][] charBitmapData;
		private static readonly int[] pixelColors = new int[] {0x00000000, unchecked((int)0xFFFFFFFF)};

		public SceFontInfoBW(BWFont fontFile)
		{
			sbyte[] fontData = fontFile.FontData;
			charmap_compr = fontFile.CharmapCompressed;
			int numberCharBitmaps = fontData.Length / charBitmapBytes;
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: charBitmapData = new short[numberCharBitmaps][charBitmapHeight];
			charBitmapData = RectangularArrays.ReturnRectangularShortArray(numberCharBitmaps, charBitmapHeight);
			shadowScaleX = 24;
			shadowScaleY = 24;

			int fontDataIndex = 0;
			for (int i = 0; i < numberCharBitmaps; i++)
			{
				for (int j = 0; j < charBitmapHeight; j++, fontDataIndex += 2)
				{
					int bitmapRow = (int)((uint)Integer.reverse(((fontData[fontDataIndex] & 0xFF) << 8) | (fontData[fontDataIndex + 1] & 0xFF)) >> 16);
					charBitmapData[i][j] = (short) bitmapRow;
				}
			}
		}

		public override void printFont(int @base, int bpl, int bufWidth, int bufHeight, int x, int y, int x64, int y64, int clipX, int clipY, int clipWidth, int clipHeight, int pixelformat, int charCode, int altCharCode, int glyphType, bool addColor)
		{
			if (glyphType != FONT_PGF_GLYPH_TYPE_CHAR)
			{
				return;
			}

			int charIndex = getCharIndex(charCode, charmap_compr);
			if (charIndex < 0 || charIndex >= charBitmapData.Length)
			{
				return;
			}
			short[] bitmapData = charBitmapData[charIndex];

			for (int yy = 0; yy < charBitmapHeight; yy++)
			{
				int bitmapRow = bitmapData[yy] & 0xFFFF;
				for (int xx = 0; xx < charBitmapWidth; xx++, bitmapRow >>= 1)
				{
					int pixelX = x + xx;
					int pixelY = y + yy;
					if (pixelX >= clipX && pixelX < clipX + clipWidth && pixelY >= clipY && pixelY < clipY + clipHeight)
					{
						int pixelColor = pixelColors[bitmapRow & 1];
						if (addColor)
						{
							Debug.addFontPixel(@base, bpl, bufWidth, bufHeight, pixelX, pixelY, pixelColor, pixelformat);
						}
						else
						{
							Debug.setFontPixel(@base, bpl, bufWidth, bufHeight, pixelX, pixelY, pixelColor, pixelformat);
						}
					}
				}
			}
		}

		public override pspCharInfo getCharInfo(int charCode, int glyphType)
		{
			pspCharInfo charInfo = new pspCharInfo();
			if (glyphType != FONT_PGF_GLYPH_TYPE_CHAR)
			{
				return charInfo;
			}

			charInfo.bitmapWidth = charBitmapWidth;
			charInfo.bitmapHeight = charBitmapHeight + 1;
			charInfo.sfp26Width = charBitmapWidth << 6;
			charInfo.sfp26Height = (charBitmapHeight + 1) << 6;
			charInfo.sfp26Ascender = charBitmapHeight << 6;
			charInfo.sfp26BearingHY = charBitmapHeight << 6;
			charInfo.sfp26BearingVX = -480; // -7.5
			charInfo.sfp26AdvanceH = (charBitmapWidth + 1) << 6;
			charInfo.sfp26AdvanceV = (charBitmapHeight + 2) << 6;

			return charInfo;
		}
	}

}