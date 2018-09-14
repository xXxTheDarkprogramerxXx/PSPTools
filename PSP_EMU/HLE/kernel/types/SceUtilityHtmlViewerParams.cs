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

	public class SceUtilityHtmlViewerParams : pspUtilityBaseDialog
	{
		public int dataAddr;
		public int dataSize;
		public int bookmarkNum;
		public int bookmarkAddr;
		public SceUtilityHtmlViewerBookmark bookmark;
		public int urlAddr;
		public string url;
		public int tabNum;
		public int userInterfaceLevel;
			public const int PSP_UTILITY_HTMLVIEWER_USER_INTERFACE_FULL = 0;
			public const int PSP_UTILITY_HTMLVIEWER_USER_INTERFACE_PARTIAL = 1;
			public const int PSP_UTILITY_HTMLVIEWER_USER_INTERFACE_NONE = 2;
		public int initFlag;
			public const int PSP_UTILITY_HTMLVIEWER_USE_START_SCREEN = 0x1;
			public const int PSP_UTILITY_HTMLVIEWER_DISABLE_RESTRICTIONS = 0x2;
		public SceUtilityHtmlViewerFile fileDownload;
		public SceUtilityHtmlViewerFile fileUpload;
		public int fileConfigAddr;
		public SceUtilityHtmlViewerConfig fileConfig;
		public int disconnectAutoFlag;
			public const int PSP_UTILITY_HTMLVIEWER_DISCONNECT_AUTO_ON = 0;
			public const int PSP_UTILITY_HTMLVIEWER_DISCONNECT_AUTO_OFF = 1;
			public const int PSP_UTILITY_HTMLVIEWER_DISCONNECT_AUTO_ASK = 2;

		public class SceUtilityHtmlViewerBookmark : pspAbstractMemoryMappedStructure
		{
			public int urlAddr;
			public string url;
			public int titleAddr;
			public string title;
			public int unk1;
			public int unk2;

			protected internal override void read()
			{
				urlAddr = read32();
				url = readStringZ(urlAddr);
				titleAddr = read32();
				title = readStringZ(titleAddr);
				unk1 = read32();
				unk2 = read32();
			}

			protected internal override void write()
			{
				write32(urlAddr);
				writeStringZ(url, urlAddr);
				write32(titleAddr);
				writeStringZ(title, titleAddr);
				write32(unk1);
				write32(unk2);
			}

			public override int @sizeof()
			{
				return 4 * 4;
			}

			public override string ToString()
			{
				return string.Format("SceUtilityHtmlViewerBookmark[url='{0}', title='{1}']", url, title);
			}
		}

		public class SceUtilityHtmlViewerFile : pspAbstractMemoryMappedStructure
		{
			public int pathAddr;
			public string path;
			public int fileNameAddr;
			public string fileName;

			protected internal override void read()
			{
				pathAddr = read32();
				path = readStringZ(pathAddr);
				fileNameAddr = read32();
				fileName = readStringZ(fileNameAddr);
			}

			protected internal override void write()
			{
				write32(pathAddr);
				writeStringZ(path, pathAddr);
				write32(fileNameAddr);
				writeStringZ(fileName, fileNameAddr);
			}

			public override int @sizeof()
			{
				return 2 * 4;
			}

			public override string ToString()
			{
				return string.Format("SceUtilityHtmlViewerFile[path='{0}', fileName='{1}']", path, fileName);
			}
		}

		public class SceUtilityHtmlViewerConfig : pspAbstractMemoryMappedStructure
		{
			public int cookiePolicyFlag;
				public const int PSP_UTILITY_HTMLVIEWER_COOKIE_POLICY_REJECT = 0;
				public const int PSP_UTILITY_HTMLVIEWER_COOKIE_POLICY_ACCEPT = 1;
				public const int PSP_UTILITY_HTMLVIEWER_COOKIE_POLICY_ASK = 2;
				public const int PSP_UTILITY_HTMLVIEWER_COOKIE_POLICY_DEFAULT = 2;
			public int cacheSize;
			public int homeUrlAddr;
			public string homeUrl;

			protected internal override void read()
			{
				cookiePolicyFlag = read32();
				cacheSize = read32();
				homeUrlAddr = read32();
				homeUrl = readStringZ(homeUrlAddr);
			}

			protected internal override void write()
			{
				write32(cookiePolicyFlag);
				write32(cacheSize);
				write32(homeUrlAddr);
				writeStringZ(homeUrl, homeUrlAddr);
			}

			public override int @sizeof()
			{
				return 3 * 4;
			}

			public override string ToString()
			{
				return string.Format("SceUtilityHtmlViewerConfig[cookiePolicy=0x{0:X}, cacheSize=0x{1:X}, homeUrl='{2}']", cookiePolicyFlag, cacheSize, homeUrl);
			}
		}

		protected internal override void read()
		{
			@base = new pspUtilityDialogCommon();
			read(@base);
			MaxSize = @base.totalSizeof();

			dataAddr = read32();
			dataSize = read32();
			bookmarkNum = read32();
			bookmarkAddr = read32();
			if (bookmarkAddr != 0)
			{
				bookmark = new SceUtilityHtmlViewerBookmark();
				bookmark.read(mem, bookmarkAddr);
			}
			else
			{
				bookmark = null;
			}
			urlAddr = read32();
			url = readStringZ(urlAddr);
			tabNum = read32();
			userInterfaceLevel = read32();
			initFlag = read32();
			fileDownload = new SceUtilityHtmlViewerFile();
			read(fileDownload);
			fileUpload = new SceUtilityHtmlViewerFile();
			read(fileUpload);
			fileConfig = new SceUtilityHtmlViewerConfig();
			read(fileConfig);
			disconnectAutoFlag = read32();
		}

		protected internal override void write()
		{
			write(@base);
			MaxSize = @base.totalSizeof();

			write32(dataAddr);
			write32(dataSize);
			write32(bookmarkNum);
			write32(bookmarkAddr);
			if (bookmark != null && bookmarkAddr != 0)
			{
				bookmark.write(mem, bookmarkAddr);
			}
			write32(urlAddr);
			writeStringUTF16Z(urlAddr, url);
			write32(tabNum);
			write32(userInterfaceLevel);
			write32(initFlag);
			write(fileDownload);
			write(fileUpload);
			write(fileConfig);
			write32(disconnectAutoFlag);
		}

		public override int @sizeof()
		{
			return @base.totalSizeof();
		}

		public override string ToString()
		{
			return string.Format("SceUtilityHtmlViewerParams[dataAddr=0x{0:X8}, dataSize=0x{1:X}, bookmarkNum={2:D}, url='{3}', fileDownload={4}, fileUpload={5}, fileConfig={6}]", dataAddr, dataSize, bookmarkNum, url, fileDownload, fileUpload, fileConfig);
		}
	}
}