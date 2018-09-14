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
//	import static pspsharp.util.Utilities.readStringZ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUByte;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUHalf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUWord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeByte;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeHalf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeStringZ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeWord;

	using Constants = pspsharp.util.Constants;

	public class PSF
	{
		private int psfOffset;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private int size_Renamed;

		private bool sizeDirty;
		private bool tablesDirty;

		private int ident;
		private int version; // yapspd: 0x1100. actual: 0x0101.
		private int keyTableOffset;
		private int valueTableOffset;
		private int indexEntryCount;

		private LinkedList<PSFKeyValuePair> pairList;

		public const int PSF_IDENT = 0x46535000;

		public const int PSF_DATA_TYPE_BINARY = 0;
		public const int PSF_DATA_TYPE_STRING = 2;
		public const int PSF_DATA_TYPE_INT32 = 4;

		public PSF(int psfOffset)
		{
			this.psfOffset = psfOffset;
			size_Renamed = 0;

			sizeDirty = true;
			tablesDirty = true;

			ident = PSF_IDENT;
			version = 0x0101;

			pairList = new LinkedList<PSFKeyValuePair>();

		}

		public PSF() : this(0)
		{
		}

		/// <summary>
		/// f.position() is undefined after calling this </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(ByteBuffer f) throws java.io.IOException
		public virtual void read(ByteBuffer f)
		{
			psfOffset = f.position();

			ident = readUWord(f);
			if (ident != PSF_IDENT)
			{
				Console.WriteLine("Not a valid PSF file (ident=" + string.Format("{0:X8}", ident) + ")");
				return;
			}

			// header
			version = readUWord(f); // 0x0101
			keyTableOffset = readUWord(f);
			valueTableOffset = readUWord(f);
			indexEntryCount = readUWord(f);

			// index table
			for (int i = 0; i < indexEntryCount; i++)
			{
				PSFKeyValuePair pair = new PSFKeyValuePair();
				pair.read(f);
				pairList.AddLast(pair);
			}

			// key/pairs
			foreach (PSFKeyValuePair pair in pairList)
			{
				f.position(psfOffset + keyTableOffset + pair.keyOffset);
				pair.key = readStringZ(f);

				f.position(psfOffset + valueTableOffset + pair.valueOffset);
				switch (pair.dataType)
				{
					case PSF_DATA_TYPE_BINARY:
						sbyte[] data = new sbyte[pair.dataSize];
						f.get(data);
						pair.data = data;

						//System.out.println(String.format("offset=%08X key='%s' binary packed [len=%d]",
						//    keyTableOffset + pair.keyOffset, pair.key, pair.dataSize));
						break;

					case PSF_DATA_TYPE_STRING:
						// String may not be in english!
						sbyte[] s = new sbyte[pair.dataSize];
						f.get(s);
						// Strip trailing null character
						pair.data = StringHelper.NewString(s, 0, s[s.Length - 1] == (sbyte)'\0' ? s.Length - 1 : s.Length, Constants.charset);

						//System.out.println(String.format("offset=%08X key='%s' string '%s' [len=%d]",
						//    keyTableOffset + pair.keyOffset, pair.key, pair.data, pair.dataSize));
						break;

					case PSF_DATA_TYPE_INT32:
						pair.data = readUWord(f);

						//System.out.println(String.format("offset=%08X key='%s' int32 %08X %d [len=%d]",
						//    keyTableOffset + pair.keyOffset, pair.key, pair.data, pair.data, pair.dataSize));
						break;

					default:
						Console.WriteLine(string.Format("offset={0:X8} key='{1}' unhandled data type {2:D} [len={3:D}]", keyTableOffset + pair.keyOffset, pair.key, pair.dataType, pair.dataSize));
						break;
				}
			}

			sizeDirty = true;
			tablesDirty = false;
			calculateSize();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(java.io.RandomAccessFile output) throws java.io.IOException
		public virtual void write(RandomAccessFile output)
		{
			sbyte[] buffer = new sbyte[size()];
			ByteBuffer byteBuffer = ByteBuffer.wrap(buffer);
			write(byteBuffer);

			// Write the file and truncate it to the correct length
			output.write(buffer);
			output.Length = buffer.Length;
		}

		// assumes we want to write at the start of the buffer, and that the current buffer position is 0
		// doesn't handle psfOffset
		public virtual void write(ByteBuffer f)
		{
			if (indexEntryCount != pairList.Count)
			{
				throw new Exception("incremental size and actual size do not match! " + indexEntryCount + "/" + pairList.Count);
			}

			if (tablesDirty)
			{
				calculateTables();
			}

			// header
			writeWord(f, ident);
			writeWord(f, version);
			writeWord(f, keyTableOffset);
			writeWord(f, valueTableOffset);
			writeWord(f, indexEntryCount);

			// index table
			foreach (PSFKeyValuePair pair in pairList)
			{
				pair.write(f);
			}

			// key/value pairs

			foreach (PSFKeyValuePair pair in pairList)
			{
				f.position(keyTableOffset + pair.keyOffset);
				//System.err.println("PSF write key   fp=" + f.position() + " datalen=" + (pair.key.length() + 1) + " top=" + (f.position() + pair.key.length() + 1));
				writeStringZ(f, pair.key);

				f.position(valueTableOffset + pair.valueOffset);
				//System.err.println("PSF write value fp=" + f.position() + " datalen=" + (pair.dataSizePadded) + " top=" + (f.position() + pair.dataSizePadded));
				switch (pair.dataType)
				{
					case PSF_DATA_TYPE_BINARY:
						f.put((sbyte[])pair.data);
						break;

					case PSF_DATA_TYPE_STRING:
						string s = (string)pair.data;
						f.put(s.GetBytes(Constants.charset));
						writeByte(f, (sbyte)0);
						break;

					case PSF_DATA_TYPE_INT32:
						writeWord(f, (int?)pair.data);
						break;

					default:
						Console.WriteLine("not writing unhandled data type " + pair.dataType);
						break;
				}
			}
		}

		public virtual object get(string key)
		{
			foreach (PSFKeyValuePair pair in pairList)
			{
				if (pair.key.Equals(key))
				{
					return pair.data;
				}
			}
			return null;
		}

		public virtual string getString(string key)
		{
			object obj = get(key);
			if (obj != null)
			{
				return (string)obj;
			}
			return null;
		}

		/// <summary>
		/// kxploit patcher tool adds "\nKXPloit Boot by PSP-DEV Team" </summary>
		public virtual string getPrintableString(string key)
		{
			string rawString = getString(key);
			if (string.ReferenceEquals(rawString, null))
			{
				return null;
			}

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < rawString.Length; i++)
			{
				char c = rawString[i];
				if (c == '\0' || c == '\n')
				{
					break;
				}
				sb.Append(rawString[i]);
			}

			return sb.ToString();
		}

		public virtual int getNumeric(string key)
		{
			object obj = get(key);
			if (obj != null)
			{
				return (int?)obj.Value;
			}
			return 0;
		}

		public virtual void put(string key, sbyte[] data)
		{
			PSFKeyValuePair pair = new PSFKeyValuePair(key, PSF_DATA_TYPE_BINARY, data.Length, data);
			pairList.AddLast(pair);

			sizeDirty = true;
			tablesDirty = true;
			indexEntryCount++;
		}

		public virtual void put(string key, string data, int rawlen)
		{
			sbyte[] b = (data.GetBytes(Constants.charset));

			//if (b.length != data.length())
			//    System.out.println("put string '" + data + "' size mismatch. UTF-8=" + b.length + " regular=" + (data.length() + 1));

			//PSFKeyValuePair pair = new PSFKeyValuePair(key, PSF_DATA_TYPE_STRING, data.length() + 1, rawlen, data);
			PSFKeyValuePair pair = new PSFKeyValuePair(key, PSF_DATA_TYPE_STRING, b.Length + 1, rawlen, data);
			pairList.AddLast(pair);

			sizeDirty = true;
			tablesDirty = true;
			indexEntryCount++;
		}

		public virtual void put(string key, string data)
		{
			sbyte[] b = (data.GetBytes(Constants.charset));
			//int rawlen = data.length() + 1;
			int rawlen = b.Length + 1;

			put(key, data, (rawlen + 3) & ~3);
		}

		public virtual void put(string key, int data)
		{
			PSFKeyValuePair pair = new PSFKeyValuePair(key, PSF_DATA_TYPE_INT32, 4, data);
			pairList.AddLast(pair);

			sizeDirty = true;
			tablesDirty = true;
			indexEntryCount++;
		}

		private void calculateTables()
		{
			tablesDirty = false;

			// position the key table after the index table and before the value table
			// 20 byte PSF header
			// 16 byte per index entry
			keyTableOffset = 5 * 4 + indexEntryCount * 0x10;


			// position the value table after the key table
			valueTableOffset = keyTableOffset;

			foreach (PSFKeyValuePair pair in pairList)
			{
				// keys are not aligned
				valueTableOffset += pair.key.Length + 1;
			}

			// 32-bit align for data start
			valueTableOffset = (valueTableOffset + 3) & ~3;


			// index table
			int keyRunningOffset = 0;
			int valueRunningOffset = 0;

			foreach (PSFKeyValuePair pair in pairList)
			{
				pair.keyOffset = keyRunningOffset;
				keyRunningOffset += pair.key.Length + 1;

				pair.valueOffset = valueRunningOffset;
				valueRunningOffset += pair.dataSizePadded;
			}
		}

		private void calculateSize()
		{
			sizeDirty = false;
			size_Renamed = 0;

			if (tablesDirty)
			{
				calculateTables();
			}

			foreach (PSFKeyValuePair pair in pairList)
			{
				int keyHighBound = keyTableOffset + pair.keyOffset + pair.key.Length + 1;
				int valueHighBound = valueTableOffset + pair.valueOffset + pair.dataSizePadded;
				if (keyHighBound > size_Renamed)
				{
					size_Renamed = keyHighBound;
				}
				if (valueHighBound > size_Renamed)
				{
					size_Renamed = valueHighBound;
				}
			}
		}

		public virtual int size()
		{
			if (sizeDirty)
			{
				calculateSize();
			}

			return size_Renamed;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			foreach (PSFKeyValuePair pair in pairList)
			{
				if (sb.Length > 0)
				{
					sb.Append(Environment.NewLine);
				}
				sb.Append(pair.ToString());
			}

			if (LikelyHomebrew)
			{
				if (sb.Length > 0)
				{
					sb.Append(Environment.NewLine);
				}
				sb.Append("This is likely a homebrew");
			}

			return sb.ToString();
		}

		/// <summary>
		/// used by isLikelyHomebrew() </summary>
		private bool safeEquals(object a, object b)
		{
			return (a == null && b == null) || (a != null && a.Equals(b));
		}

		public virtual bool LikelyHomebrew
		{
			get
			{
				bool homebrew = false;
    
				string disc_version = getString("DISC_VERSION");
				string disc_id = getString("DISC_ID");
				string category = getString("CATEGORY");
				int? bootable = (int?)get("BOOTABLE"); // don't use getNumeric, we also want to know if the entry exists or not
				int? region = (int?)get("REGION");
				string psp_system_ver = getString("PSP_SYSTEM_VER");
				int? parental_level = (int?)get("PARENTAL_LEVEL");
    
				int? ref_one = new int?(1);
				int? ref_region = new int?(32768);
    
				if (safeEquals(disc_version, "1.00") && safeEquals(disc_id, "UCJS10041") && safeEquals(category, "MG") && safeEquals(bootable, ref_one) && safeEquals(region, ref_region) && safeEquals(psp_system_ver, "1.00") && safeEquals(parental_level, ref_one))
				{
    
					if (indexEntryCount == 8)
					{
						homebrew = true;
					}
					else if (indexEntryCount == 9 && safeEquals(get("MEMSIZE"), ref_one))
					{
						// lua player hm 8
						homebrew = true;
					}
				}
				else if (indexEntryCount == 4 && safeEquals(category, "MG") && safeEquals(bootable, ref_one) && safeEquals(region, ref_region))
				{
					homebrew = true;
				}
    
				return homebrew;
			}
		}

		public class PSFKeyValuePair
		{
			// index table info
			public int keyOffset;
			public int unknown1;
			public int dataType;
			public int dataSize;
			public int dataSizePadded;
			public int valueOffset;

			// key table info
			public string key;

			// data table info
			public object data;

			public PSFKeyValuePair() : this(null, 0, 0, null)
			{
			}

			public PSFKeyValuePair(string key, int dataType, int dataSize, object data) : this(key, dataType, dataSize, (dataSize + 3) & ~3, data)
			{
			}

			public PSFKeyValuePair(string key, int dataType, int dataSize, int dataSizePadded, object data)
			{
				this.key = key;
				this.dataType = dataType;
				this.dataSize = dataSize;
				this.dataSizePadded = dataSizePadded;
				this.data = data;

				// yapspd: 4
				// probably alignment of the value data
				unknown1 = 4;
			}

			/// <summary>
			/// only reads the index entry, since this class has doesn't know about the psf/key/value offsets </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void read(ByteBuffer f) throws java.io.IOException
			public virtual void read(ByteBuffer f)
			{
				// index table entry
				keyOffset = readUHalf(f);
				unknown1 = readUByte(f);
				dataType = readUByte(f);
				dataSize = readUWord(f);
				dataSizePadded = readUWord(f);
				valueOffset = readUWord(f);
			}

			/// <summary>
			/// only writes the index entry, since this class has doesn't know about the psf/key/value offsets </summary>
			public virtual void write(ByteBuffer f)
			{
				// index table entry
				writeHalf(f, keyOffset);
				writeByte(f, unknown1);
				writeByte(f, dataType);
				writeWord(f, dataSize);
				writeWord(f, dataSizePadded);
				writeWord(f, valueOffset);
			}

			public override string ToString()
			{
				StringBuilder sb = new StringBuilder();

				/*
				sb.append("index entry:\n");
				sb.append(String.format("keyOffset 0x%08X %d\n", keyOffset, keyOffset));
				sb.append(String.format("unknown1 0x%08X %d\n", unknown1, unknown1));
				sb.append(String.format("dataType 0x%08X %d\n", dataType, dataType));
				sb.append(String.format("dataSize 0x%08X %d\n", dataSize, dataSize));
				sb.append(String.format("dataSizePadding 0x%08X %d\n", dataSizePadding, dataSizePadding));
				sb.append(String.format("valueOffset 0x%08X %d\n", valueOffset, valueOffset));
				*/

				//sb.append(String.format("[offset=%08X] '%s' = [offset=%08X,len=%d,rawlen=%d] '" + data + "'",
				//    keyOffset, key, valueOffset, dataSize, dataSizePadded));

				sb.Append(key + " = " + data);

				return sb.ToString();
			}
		}
	}

}