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
	public class pspFontStyle : pspAbstractMemoryMappedStructure
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
		public const int FONT_LANGUAGE_CHINESE = 4;

		public float fontH; // Horizontal size.
		public float fontV; // Vertical size.
		public float fontHRes; // Horizontal resolution.
		public float fontVRes; // Vertical resolution.
		public float fontWeight; // Font weight.
		public short fontFamily; // Font family (SYSTEM = 0, probably more).
		public short fontStyle; // Style (SYSTEM = 0, STANDARD = 1, probably more).
		public short fontStyleSub; // Subset of style (only used in Asian fonts, unknown values).
		public short fontLanguage; // Language code (UNK = 0, JAPANESE = 1, ENGLISH = 2, probably more).
		public short fontRegion; // Region code (UNK = 0, JAPAN = 1, probably more).
		public short fontCountry; // Country code (UNK = 0, JAPAN = 1, US = 2, probably more).
		public string fontName; // Font name (maximum size is 64).
		public string fontFileName; // File name (maximum size is 64).
		public int fontAttributes; // Additional attributes.
		public int fontExpire; // Expiration date.

		protected internal override void read()
		{
			fontH = readFloat();
			fontV = readFloat();
			fontHRes = readFloat();
			fontVRes = readFloat();
			fontWeight = readFloat();
			fontFamily = (short) read16();
			fontStyle = (short) read16();
			fontStyleSub = (short) read16();
			fontLanguage = (short) read16();
			fontRegion = (short) read16();
			fontCountry = (short) read16();
			fontName = readStringNZ(64);
			fontFileName = readStringNZ(64);
			fontAttributes = read32();
			fontExpire = read32();
		}

		protected internal override void write()
		{
			writeFloat(fontH); // Offset 0
			writeFloat(fontV); // Offset 4
			writeFloat(fontHRes); // Offset 8
			writeFloat(fontVRes); // Offset 12
			writeFloat(fontWeight); // Offset 16
			write16(fontFamily); // Offset 20
			write16(fontStyle); // Offset 22
			write16(fontStyleSub); // Offset 24
			write16(fontLanguage); // Offset 26
			write16(fontRegion); // Offset 28
			write16(fontCountry); // Offset 30
			writeStringNZ(64, fontName); // Offset 32
			writeStringNZ(64, fontFileName); // Offset 96
			write32(fontAttributes); // Offset 160
			write32(fontExpire); // Offset 164
		}

		public override int @sizeof()
		{
			return 168;
		}

		public virtual bool isMatching(pspFontStyle fontStyle, bool optimum)
		{
			// A value 0 in each field of the fontStyle means "any value"
			if (!optimum)
			{
				if (fontStyle.fontH != 0f)
				{
					if (System.Math.Round(fontStyle.fontH) != System.Math.Round(fontH))
					{
						return false;
					}
				}
				if (fontStyle.fontV != 0f)
				{
					if (System.Math.Round(fontStyle.fontV) != System.Math.Round(fontV))
					{
						return false;
					}
				}
				if (fontStyle.fontHRes != 0f)
				{
					if (System.Math.Round(fontStyle.fontHRes) != System.Math.Round(fontHRes))
					{
						return false;
					}
				}
				if (fontStyle.fontVRes != 0f)
				{
					if (System.Math.Round(fontStyle.fontVRes) != System.Math.Round(fontVRes))
					{
						return false;
					}
				}
			}
			if (fontStyle.fontWeight != 0f && fontStyle.fontWeight != fontWeight)
			{
				return false;
			}
			if (fontStyle.fontFamily != 0 && fontStyle.fontFamily != fontFamily)
			{
				return false;
			}
			if (fontStyle.fontStyle != 0 && fontStyle.fontStyle != this.fontStyle)
			{
				return false;
			}
			if (fontStyle.fontStyleSub != 0 && fontStyle.fontStyleSub != fontStyleSub)
			{
				return false;
			}
			if (fontStyle.fontLanguage != 0 && fontStyle.fontLanguage != fontLanguage)
			{
				return false;
			}
			if (fontStyle.fontRegion != 0 && fontStyle.fontRegion != fontRegion)
			{
				return false;
			}
			if (fontStyle.fontCountry != 0 && fontStyle.fontCountry != fontCountry)
			{
				return false;
			}
			if (fontStyle.fontName.Length > 0 && !fontStyle.fontName.Equals(fontName))
			{
				return false;
			}
			if (fontStyle.fontFileName.Length > 0 && !fontStyle.fontFileName.Equals(fontFileName))
			{
				return false;
			}
			if (fontStyle.fontAttributes != 0 && fontStyle.fontAttributes != fontAttributes)
			{
				return false;
			}

			return true;
		}

		public virtual bool Empty
		{
			get
			{
				if (fontH != 0f || fontV != 0f || fontHRes != 0f || fontVRes != 0f)
				{
					return false;
				}
				if (fontWeight != 0f || fontFamily != 0 || fontStyle != 0 || fontStyleSub != 0)
				{
					return false;
				}
				if (fontLanguage != 0 || fontRegion != 0 || fontCountry != 0)
				{
					return false;
				}
				if (fontName.Length > 0 || fontFileName.Length > 0 || fontAttributes != 0)
				{
					return false;
				}
    
				return true;
			}
		}

		public override string ToString()
		{
			return string.Format("fontH {0:F}, fontV {1:F}, fontHRes {2:F}, fontVRes {3:F}, fontWeight {4:F}, fontFamily {5:D}, fontStyle {6:D}, fontStyleSub {7:D}, fontLanguage {8:D}, fontRegion {9:D}, fontCountry {10:D}, fontName '{11}', fontFileName '{12}', fontAttributes {13:D}, fontExpire {14:D}", fontH, fontV, fontHRes, fontVRes, fontWeight, fontFamily, fontStyle, fontStyleSub, fontLanguage, fontRegion, fontCountry, fontName, fontFileName, fontAttributes, fontExpire);
		}
	}

}