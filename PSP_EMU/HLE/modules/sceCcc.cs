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

	using Logger = org.apache.log4j.Logger;

	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;

	public class sceCcc : HLEModule
	{
		public static Logger log = Modules.getLogger("sceCcc");

		protected internal static readonly Charset charsetUTF8 = getCharset("UTF-8");
		protected internal static readonly Charset charsetUTF16 = getCharset("UTF-16LE");
		protected internal static readonly Charset charsetSJIS = getCharset("Shift_JIS");
		protected internal int errorCharUTF8;
		protected internal int errorCharUTF16;
		protected internal int errorCharSJIS;

		public override void stop()
		{
			errorCharUTF8 = 0;
			errorCharUTF16 = 0;
			errorCharSJIS = 0;

			base.stop();
		}

		protected internal static Charset getCharset(string charsetName)
		{
			if (!Charset.isSupported(charsetName))
			{
				log.warn(string.Format("Charset not supported by this JVM: {0}", charsetName));
				if (log.InfoEnabled)
				{
					SortedDictionary<string, Charset> availableCharsets = Charset.availableCharsets();
					log.info("Supported Charsets:");
					foreach (string availableCharsetName in availableCharsets.Keys)
					{
						log.info(availableCharsetName);
					}
				}

				return Charset.defaultCharset();
			}

			return Charset.forName(charsetName);
		}

		protected internal static sbyte[] addByteToArray(sbyte[] array, sbyte b)
		{
			sbyte[] newArray = new sbyte[array.Length + 1];
			Array.Copy(array, 0, newArray, 0, array.Length);
			newArray[array.Length] = b;

			return newArray;
		}

		protected internal static sbyte[] getBytesUTF16(int addr)
		{
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(addr, 2);
			sbyte[] bytes = new sbyte[0];
			while (true)
			{
				int utf16 = memoryReader.readNext();
				if (utf16 == 0)
				{
					break;
				}
				bytes = addByteToArray(bytes, (sbyte) utf16);
				bytes = addByteToArray(bytes, (sbyte)(utf16 >> 8));
			}

			return bytes;
		}

		protected internal static string getStringUTF16(int addr)
		{
			return StringHelper.NewString(getBytesUTF16(addr), charsetUTF16);
		}

		protected internal static sbyte[] getBytesUTF8(int addr)
		{
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(addr, 1);
			sbyte[] bytes = new sbyte[0];
			while (true)
			{
				int utf8 = memoryReader.readNext();
				if (utf8 == 0)
				{
					break;
				}
				bytes = addByteToArray(bytes, (sbyte) utf8);
			}

			return bytes;
		}

		protected internal static string getStringUTF8(int addr)
		{
			return StringHelper.NewString(getBytesUTF8(addr), charsetUTF8);
		}

		protected internal static string getStringSJIS(int addr)
		{
			return StringHelper.NewString(getBytesUTF8(addr), charsetSJIS);
		}

		protected internal virtual int writeStringBytes(sbyte[] bytes, int addr, int maxSize, int trailingNulls)
		{
			int bytesWritten = 0;
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(addr, 1);
			if (bytes != null)
			{
				int length = System.Math.Min(bytes.Length, maxSize - 1);
				for (int i = 0; i < length; i++)
				{
					memoryWriter.writeNext(bytes[i] & 0xFF);
				}
				bytesWritten += length;
			}

			// write trailing '\0'
			for (int i = 0; i < trailingNulls; i++)
			{
				memoryWriter.writeNext(0);
			}

			memoryWriter.flush();

			return bytesWritten;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x00D1378F, version = 150) public int sceCccUTF8toUTF16()
		[HLEFunction(nid : 0x00D1378F, version : 150)]
		public virtual int sceCccUTF8toUTF16()
		{
			return 0;
		}

		[HLEFunction(nid : 0x068C4320, version : 150)]
		public virtual int sceCccEncodeSJIS(TPointer32 dstAddr, int ucs4char)
		{
			char[] chars = Character.toChars(ucs4char);
			if (chars == null)
			{
				return 0;
			}

			string s = new string(chars);
			sbyte[] bytes = s.GetBytes(charsetSJIS);
			int addr = dstAddr.getValue();
			int length = writeStringBytes(bytes, addr, 100, 0);
			dstAddr.setValue(addr + length);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCccEncodeSJIS encoding '{0}' into {1:D} bytes", s, length));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0A00ECF9, version = 150) public int sceCccSwprintfSJIS()
		[HLEFunction(nid : 0x0A00ECF9, version : 150)]
		public virtual int sceCccSwprintfSJIS()
		{
			return 0;
		}

		[HLEFunction(nid : 0x17E1D813, version : 150)]
		public virtual int sceCccSetErrorCharUTF8(int errorChar)
		{
			int previousErrorChar = errorCharUTF8;
			errorCharUTF8 = errorChar;

			return previousErrorChar;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3AEC5274, version = 150) public int sceCccSwprintfUTF8()
		[HLEFunction(nid : 0x3AEC5274, version : 150)]
		public virtual int sceCccSwprintfUTF8()
		{
			return 0;
		}

		[HLEFunction(nid : 0x41B724A5, version : 150)]
		public virtual int sceCccUTF16toUTF8(TPointer dstAddr, int dstSize, TPointer srcAddr)
		{
			string dstString = getStringUTF16(srcAddr.Address);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCccUTF16toUTF8 string='{0}'", dstString));
			}
			sbyte[] dstBytes = dstString.GetBytes(charsetUTF8);
			return writeStringBytes(dstBytes, dstAddr.Address, dstSize, 1);
		}

		[HLEFunction(nid : 0x4BDEB2A8, version : 150)]
		public virtual int sceCccStrlenUTF16(TPointer strUTF16)
		{
			string str = getStringUTF16(strUTF16.Address);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCccStrlenUTF16 str='{0}'", str));
			}

			return str.Length;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x67BF0D19, version = 150) public int sceCccIsValidSJIS()
		[HLEFunction(nid : 0x67BF0D19, version : 150)]
		public virtual int sceCccIsValidSJIS()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6CBB36A0, version = 150) public int sceCccVswprintfUTF8()
		[HLEFunction(nid : 0x6CBB36A0, version : 150)]
		public virtual int sceCccVswprintfUTF8()
		{
			return 0;
		}

		[HLEFunction(nid : 0x6F82EE03, version : 150)]
		public virtual int sceCccUTF8toSJIS(TPointer dstAddr, int dstSize, TPointer srcAddr)
		{
			string dstString = getStringUTF8(srcAddr.Address);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCccUTF8toSJIS string='{0}'", dstString));
			}
			sbyte[] dstBytes = dstString.GetBytes(charsetSJIS);
			return writeStringBytes(dstBytes, dstAddr.Address, dstSize, 1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x70ECAA10, version = 150) public int sceCccUCStoJIS()
		[HLEFunction(nid : 0x70ECAA10, version : 150)]
		public virtual int sceCccUCStoJIS()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x76E33E9C, version = 150) public int sceCccIsValidUCS2()
		[HLEFunction(nid : 0x76E33E9C, version : 150)]
		public virtual int sceCccIsValidUCS2()
		{
			return 0;
		}

		[HLEFunction(nid : 0x8406F469, version : 150)]
		public virtual int sceCccEncodeUTF16(TPointer32 dstAddr, int ucs4char)
		{
			char[] chars = Character.toChars(ucs4char);
			if (chars == null)
			{
				return 0;
			}

			string s = new string(chars);
			sbyte[] bytes = s.GetBytes(charsetUTF16);
			int addr = dstAddr.getValue();
			int length = writeStringBytes(bytes, addr, 100, 0);
			dstAddr.setValue(addr + length);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCccEncodeUTF16 encoding '{0}' into {1:D} bytes", s, length));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x90521AC5, version = 150) public int sceCccIsValidUTF8()
		[HLEFunction(nid : 0x90521AC5, version : 150)]
		public virtual int sceCccIsValidUTF8()
		{
			return 0;
		}

		[HLEFunction(nid : 0x92C05851, version : 150)]
		public virtual int sceCccEncodeUTF8(TPointer32 dstAddr, int ucs4char)
		{
			char[] chars = Character.toChars(ucs4char);
			if (chars == null)
			{
				return 0;
			}

			string s = new string(chars);
			sbyte[] bytes = s.GetBytes(charsetUTF8);
			int addr = dstAddr.getValue();
			int length = writeStringBytes(bytes, addr, 100, 0);
			dstAddr.setValue(addr + length);

			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCccEncodeUTF8 encoding '{0}' into {1:D} bytes", s, length));
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x953E6C10, version = 150) public int sceCccDecodeSJIS()
		[HLEFunction(nid : 0x953E6C10, version : 150)]
		public virtual int sceCccDecodeSJIS()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA2D5D209, version = 150) public int sceCccIsValidJIS()
		[HLEFunction(nid : 0xA2D5D209, version : 150)]
		public virtual int sceCccIsValidJIS()
		{
			return 0;
		}

		[HLEFunction(nid : 0xA62E6E80, version : 150)]
		public virtual int sceCccSJIStoUTF8(TPointer dstUTF8, int dstSize, TPointer srcSJIS)
		{
			string str = getStringSJIS(srcSJIS.Address);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCccSJIStoUTF8 str='{0}'", str));
			}
			sbyte[] bytesUTF8 = str.GetBytes(charsetUTF8);
			return writeStringBytes(bytesUTF8, dstUTF8.Address, dstSize, 1);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB4D1CBBF, version = 150) public int sceCccSetTable(pspsharp.HLE.TPointer jis2ucs, pspsharp.HLE.TPointer ucs2jis)
		[HLEFunction(nid : 0xB4D1CBBF, version : 150)]
		public virtual int sceCccSetTable(TPointer jis2ucs, TPointer ucs2jis)
		{
			// Both tables jis2ucs and ucs2jis have a size of 0x20000 bytes
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB7D3C112, version = 150) public int sceCccStrlenUTF8(pspsharp.HLE.TPointer strUTF8)
		[HLEFunction(nid : 0xB7D3C112, version : 150)]
		public virtual int sceCccStrlenUTF8(TPointer strUTF8)
		{
			string str = getStringUTF16(strUTF8.Address);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCccStrlenUTF8 str='{0}'", str));
			}

			return str.Length;
		}

		[HLEFunction(nid : 0xB8476CF4, version : 150)]
		public virtual int sceCccSetErrorCharUTF16(int errorChar)
		{
			int previousErrorChar = errorCharUTF16;
			errorCharUTF16 = errorChar;

			return previousErrorChar;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBD11EEF3, version = 150) public int sceCccIsValidUnicode()
		[HLEFunction(nid : 0xBD11EEF3, version : 150)]
		public virtual int sceCccIsValidUnicode()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBDC4D699, version = 150) public int sceCccVswprintfSJIS()
		[HLEFunction(nid : 0xBDC4D699, version : 150)]
		public virtual int sceCccVswprintfSJIS()
		{
			return 0;
		}

		[HLEFunction(nid : 0xBEB47224, version : 150)]
		public virtual int sceCccSJIStoUTF16(TPointer dstUTF16, int dstSize, TPointer srcSJIS)
		{
			string str = getStringSJIS(srcSJIS.Address);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCccSJIStoUTF16 str='{0}'", str));
			}
			sbyte[] bytesUTF16 = str.GetBytes(charsetUTF16);
			return writeStringBytes(bytesUTF16, dstUTF16.Address, dstSize, 2);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC56949AD, version = 150) public int sceCccSetErrorCharSJIS(int errorChar)
		[HLEFunction(nid : 0xC56949AD, version : 150)]
		public virtual int sceCccSetErrorCharSJIS(int errorChar)
		{
			int previousErrorChar = errorCharSJIS;
			errorCharSJIS = errorChar;

			return previousErrorChar;
		}

		[HLEFunction(nid : 0xC6A8BEE2, version : 150)]
		public virtual int sceCccDecodeUTF8(TPointer32 srcAddrUTF8)
		{
			string srcString = getStringUTF8(srcAddrUTF8.getValue());
			int codePoint = char.ConvertToUtf32(srcString, 0);
			int codePointSize = Character.charCount(codePoint);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCccDecodeUTF8 string='{0}'(0x{1:X8}), codePoint=0x{2:X}(size={3:D})", srcString, srcAddrUTF8.getValue(), codePoint, codePointSize));
			}

			srcAddrUTF8.setValue(srcAddrUTF8.getValue() + codePointSize);

			return codePoint;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xCC0A8BDA, version = 150) public int sceCccIsValidUTF16()
		[HLEFunction(nid : 0xCC0A8BDA, version : 150)]
		public virtual int sceCccIsValidUTF16()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD2B18485, version = 150) public int sceCccIsValidUCS4()
		[HLEFunction(nid : 0xD2B18485, version : 150)]
		public virtual int sceCccIsValidUCS4()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD9392CCB, version = 150) public int sceCccStrlenSJIS(pspsharp.HLE.TPointer strSJIS)
		[HLEFunction(nid : 0xD9392CCB, version : 150)]
		public virtual int sceCccStrlenSJIS(TPointer strSJIS)
		{
			string str = getStringUTF16(strSJIS.Address);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCccStrlenSJIS str='{0}'", str));
			}

			return str.Length;
		}

		[HLEFunction(nid : 0xE0CF8091, version : 150)]
		public virtual int sceCccDecodeUTF16(TPointer32 srcAddrUTF16)
		{
			string srcString = getStringUTF16(srcAddrUTF16.getValue());
			int codePoint = char.ConvertToUtf32(srcString, 0);
			int codePointSize = Character.charCount(codePoint);
			if (log.DebugEnabled)
			{
				log.debug(string.Format("sceCccDecodeUTF16 string='{0}'(0x{1:X8}), codePoint=0x{2:X}(size={3:D})", srcString, srcAddrUTF16.getValue(), codePoint, codePointSize));
			}

			srcAddrUTF16.setValue(srcAddrUTF16.getValue() + (codePointSize << 1));

			return codePoint;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF1B73D12, version = 150) public int sceCccUTF16toSJIS()
		[HLEFunction(nid : 0xF1B73D12, version : 150)]
		public virtual int sceCccUTF16toSJIS()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFB7846E2, version = 150) public int sceCccJIStoUCS()
		[HLEFunction(nid : 0xFB7846E2, version : 150)]
		public virtual int sceCccJIStoUCS()
		{
			return 0;
		}
	}
}