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
namespace pspsharp.HLE.kernel.types
{
	public class SceUtilityScreenshotParams : pspUtilityBaseDialog
	{
		public int startupType;
			public const int PSP_UTILITY_SCREENSHOT_TYPE_GUI = 0;
			public const int PSP_UTILITY_SCREENSHOT_TYPE_AUTO = 1;
			public const int PSP_UTILITY_SCREENSHOT_TYPE_LIST_SAVE = 2;
			public const int PSP_UTILITY_SCREENSHOT_TYPE_LIST_VIEW = 3;
			public const int PSP_UTILITY_SCREENSHOT_TYPE_CONT_FINISH = 4;
			public const int PSP_UTILITY_SCREENSHOT_TYPE_CONT_AUTO = 5;
		public int status;
			public const int PSP_UTILITY_SCREENSHOT_STATUS_BUSY = 0;
			public const int PSP_UTILITY_SCREENSHOT_STATUS_DONE = 1;
		public int imgFormat;
			public const int PSP_UTILITY_SCREENSHOT_FORMAT_PNG = 1;
			public const int PSP_UTILITY_SCREENSHOT_FORMAT_JPEG = 2;
		public int imgQuality;
		public int imgFrameBufAddr;
		public int imgFrameBufWidth;
		public int imgPixelFormat;
		public int screenshotOffsetX;
		public int screenshotOffsetY;
		public int displayWidth;
		public int displayHeigth;
		public string screenshotID;
		public string fileName;
		public int nameRule;
			public const int PSP_UTILITY_SCREENSHOT_NAMERULE_NONE = 0;
			public const int PSP_UTILITY_SCREENSHOT_NAMERULE_AUTONUM = 1;
		public string title;
		public int parentalLevel;
		public int pscmFileFlag;
			public const int PSP_UTILITY_SCREENSHOT_PSCM_CREATE = 0;
			public const int PSP_UTILITY_SCREENSHOT_PSCM_OVERWRITE = 1;
		public string iconPath;
		public int iconPathSize;
		public int iconFileSize;
		public string backgroundPath;
		public int backgroundPathSize;
		public int backgroundFileSize;
		public int commentFlag;
			public const int PSP_UTILITY_SCREENSHOT_COMMENT_CREATE = 0;
			public const int PSP_UTILITY_SCREENSHOT_COMMENT_DONT_CREATE = 1;
		public SceUtilityScreenshotCommentShape commentShape;
		public SceUtilityScreenshotCommentText commentText;

		public class SceUtilityScreenshotCommentShape : pspAbstractMemoryMappedStructure
		{
			public int width;
			public int heigth;
			public int backgroundColor;
			public int alignX;
			public int alignY;
			public int contentAlignY;
			public int marginTop;
			public int marginBottom;
			public int marginLeft;
			public int marginRight;
			public int paddingTop;
			public int paddingBottom;
			public int paddingLeft;
			public int paddingRight;

			protected internal override void read()
			{
				width = read32();
				heigth = read32();
				backgroundColor = read32();
				alignX = read32();
				alignY = read32();
				contentAlignY = read32();
				marginTop = read32();
				marginBottom = read32();
				marginLeft = read32();
				marginRight = read32();
				paddingTop = read32();
				paddingBottom = read32();
				paddingLeft = read32();
				paddingRight = read32();
			}

			protected internal override void write()
			{
				write32(width);
				write32(heigth);
				write32(backgroundColor);
				write32(alignX);
				write32(alignY);
				write32(contentAlignY);
				write32(marginTop);
				write32(marginBottom);
				write32(marginLeft);
				write32(marginRight);
				write32(paddingTop);
				write32(paddingBottom);
				write32(paddingLeft);
				write32(paddingRight);
			}

			public override int @sizeof()
			{
				return 14 * 4;
			}

			public override string ToString()
			{
				return string.Format("width={0:D}, height={1:D}, backgroundColor=0x{2:X8}, align [X={3:D}, Y={4:D}], margin [T={5:D}, B={6:D}, L={7:D}, R={8:D}], padding [T={9:D}, B={10:D}, L={11:D}, R={12:D}]", width, heigth, backgroundColor, alignX, alignY, marginTop, marginBottom, marginLeft, marginRight, paddingTop, paddingBottom, paddingLeft, paddingRight);
			}
		}

		public class SceUtilityScreenshotCommentText : pspAbstractMemoryMappedStructure
		{
			public int textColor;
			public int shadowType;
				public const int PSP_UTILITY_SCREENSHOT_COMMENT_SHADOW_DEFAULT = 0;
				public const int PSP_UTILITY_SCREENSHOT_COMMENT_SHADOW_ON = 1;
				public const int PSP_UTILITY_SCREENSHOT_COMMENT_SHADOW_OFF = 2;
			public int shadowColor;
			public int fontSize;
				public const int PSP_UTILITY_SCREENSHOT_COMMENT_FONT_SIZE_DEFAULT = 0;
				public const int PSP_UTILITY_SCREENSHOT_COMMENT_FONT_SIZE_SMALL = 1;
				public const int PSP_UTILITY_SCREENSHOT_COMMENT_FONT_SIZE_MEDIUM = 2;
				public const int PSP_UTILITY_SCREENSHOT_COMMENT_FONT_SIZE_LARGE = 3;
			public int lineSpace;
			public int alignX;
			public string textComment;
			public int textCommentLength;

			protected internal override void read()
			{
				textColor = read32();
				shadowType = read32();
				shadowColor = read32();
				fontSize = read32();
				lineSpace = read32();
				alignX = read32();
				textComment = readStringUTF16NZ(256);
				textCommentLength = read32();
			}

			protected internal override void write()
			{
				write32(textColor);
				write32(shadowType);
				write32(shadowColor);
				write32(fontSize);
				write32(lineSpace);
				write32(alignX);
				textCommentLength = writeStringUTF16Z(256, textComment);
				write32(textCommentLength);
			}

			public override int @sizeof()
			{
				return 7 * 4 + 256;
			}

			public override string ToString()
			{
				return string.Format("textColor=0x{0:X8}, shadowType={1:D}, shadowColor=0x{2:X8}, fontSize={3:D}, lineSpace={4:D}, alignX={5:D}, textComment='{6}', textCommentLength={7:D}", textColor, shadowType, shadowColor, fontSize, lineSpace, alignX, textComment, textCommentLength);
			}
		}

		protected internal override void read()
		{
			@base = new pspUtilityDialogCommon();
			read(@base);
			MaxSize = @base.totalSizeof();

			startupType = read32();
			status = read32();
			imgFormat = read32();
			imgQuality = read32();
			imgFrameBufAddr = read32();
			imgFrameBufWidth = read32();
			imgPixelFormat = read32();
			screenshotOffsetX = read32();
			screenshotOffsetY = read32();
			displayWidth = read32();
			displayHeigth = read32();
			screenshotID = readStringNZ(12);
			fileName = readStringNZ(192);
			nameRule = read32();
			readUnknown(4);
			title = readStringNZ(128);
			parentalLevel = read32();
			pscmFileFlag = read32();
			iconPath = readStringNZ(64);
			iconPathSize = read32();
			iconFileSize = read32();
			backgroundPath = readStringNZ(64);
			backgroundPathSize = read32();
			backgroundFileSize = read32();
			commentFlag = read32();
			commentShape = new SceUtilityScreenshotCommentShape();
			read(commentShape);
			commentText = new SceUtilityScreenshotCommentText();
			read(commentText);
			readUnknown(4);
		}

		protected internal override void write()
		{
			write(@base);
			MaxSize = @base.totalSizeof();

			write32(startupType);
			write32(status);
			write32(imgFormat);
			write32(imgQuality);
			write32(imgFrameBufAddr);
			write32(imgFrameBufWidth);
			write32(imgPixelFormat);
			write32(screenshotOffsetX);
			write32(screenshotOffsetY);
			write32(displayWidth);
			write32(displayHeigth);
			writeStringNZ(12, screenshotID);
			writeStringNZ(192, fileName);
			write32(nameRule);
			writeUnknown(4);
			writeStringNZ(128, title);
			write32(parentalLevel);
			write32(pscmFileFlag);
			writeStringNZ(64, iconPath);
			write32(iconPathSize);
			write32(iconFileSize);
			writeStringNZ(64, backgroundPath);
			write32(backgroundPathSize);
			write32(backgroundFileSize);
			write32(commentFlag);
			write(commentShape);
			write(commentText);
			writeUnknown(4);
		}

		public virtual bool ContModeAuto
		{
			get
			{
				return (startupType & 0x7) == PSP_UTILITY_SCREENSHOT_TYPE_CONT_AUTO;
			}
		}

		public virtual bool ContModeFinish
		{
			get
			{
				return (startupType & 0x7) == PSP_UTILITY_SCREENSHOT_TYPE_CONT_FINISH;
			}
		}

		public override int @sizeof()
		{
			return @base.totalSizeof();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(string.Format("startupType={0:D}", startupType));
			sb.Append(string.Format(", status={0:D}", status));
			sb.Append(string.Format(", imgFormat={0:D}", imgFormat));
			sb.Append(string.Format(", imgQuality={0:D}", imgQuality));
			sb.Append(string.Format(", imgFrameBufAddr=0x{0:X8}", imgFrameBufAddr));
			sb.Append(string.Format(", imgFrameBufWidth={0:D}", imgFrameBufWidth));
			sb.Append(string.Format(", imgPixelFormat={0:D}", imgPixelFormat));
			sb.Append(string.Format(", screenshotOffsetX={0:D}", screenshotOffsetX));
			sb.Append(string.Format(", screenshotOffsetY={0:D}", screenshotOffsetY));
			sb.Append(string.Format(", displayWidth={0:D}", displayWidth));
			sb.Append(string.Format(", displayHeigth={0:D}", displayHeigth));
			sb.Append(string.Format(", screenshotID='{0}'", screenshotID));
			sb.Append(string.Format(", fileName='{0}'", fileName));
			sb.Append(string.Format(", nameRule={0:D}", nameRule));
			sb.Append(string.Format(", title='{0}'", title));
			sb.Append(string.Format(", parentalLevel={0:D}", parentalLevel));
			sb.Append(string.Format(", pscmFileFlag={0:D}", pscmFileFlag));
			sb.Append(string.Format(", iconPath='{0}'", iconPath));
			sb.Append(string.Format(", iconPathSize={0:D}", iconPathSize));
			sb.Append(string.Format(", iconFileSize={0:D}", iconFileSize));
			sb.Append(string.Format(", backgroundPath='{0}'", backgroundPath));
			sb.Append(string.Format(", backgroundPathSize={0:D}", backgroundPathSize));
			sb.Append(string.Format(", backgroundFileSize={0:D}", backgroundFileSize));
			sb.Append(string.Format(", commentFlag={0:D}", commentFlag));
			sb.Append(string.Format(", commentShape [{0}]", commentShape));
			sb.Append(string.Format(", commentText [{0}]", commentText));

			return sb.ToString();
		}
	}
}