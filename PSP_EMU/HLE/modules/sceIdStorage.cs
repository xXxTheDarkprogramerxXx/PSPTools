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

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;

	public class sceIdStorage : HLEModule
	{
		public static Logger log = Modules.getLogger("sceIdStorage");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAB129D20, version = 150) public int sceIdStorageInit()
		[HLEFunction(nid : 0xAB129D20, version : 150)]
		public virtual int sceIdStorageInit()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2CE0BE69, version = 150) public int sceIdStorageEnd()
		[HLEFunction(nid : 0x2CE0BE69, version : 150)]
		public virtual int sceIdStorageEnd()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF77565B6, version = 150) public int sceIdStorageSuspend()
		[HLEFunction(nid : 0xF77565B6, version : 150)]
		public virtual int sceIdStorageSuspend()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFE51173D, version = 150) public int sceIdStorageResume()
		[HLEFunction(nid : 0xFE51173D, version : 150)]
		public virtual int sceIdStorageResume()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEB830733, version = 150) public int sceIdStorageGetLeafSize()
		[HLEFunction(nid : 0xEB830733, version : 150)]
		public virtual int sceIdStorageGetLeafSize()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFEFA40C2, version = 150) public int sceIdStorageIsFormatted()
		[HLEFunction(nid : 0xFEFA40C2, version : 150)]
		public virtual int sceIdStorageIsFormatted()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2D633688, version = 150) public int sceIdStorageIsReadOnly()
		[HLEFunction(nid : 0x2D633688, version : 150)]
		public virtual int sceIdStorageIsReadOnly()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB9069BAD, version = 150) public int sceIdStorageIsDirty()
		[HLEFunction(nid : 0xB9069BAD, version : 150)]
		public virtual int sceIdStorageIsDirty()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x958089DB, version = 150) public int sceIdStorageFormat()
		[HLEFunction(nid : 0x958089DB, version : 150)]
		public virtual int sceIdStorageFormat()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF4BCB3EE, version = 150) public int sceIdStorageUnformat()
		[HLEFunction(nid : 0xF4BCB3EE, version : 150)]
		public virtual int sceIdStorageUnformat()
		{
			return 0;
		}

		/// <summary>
		/// Retrieves the whole 512 byte container for the key.
		/// </summary>
		/// <param name="key">    idstorage key </param>
		/// <param name="buffer"> buffer with at last 512 bytes of storage </param>
		/// <returns>       0. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEB00C509, version = 150) public int sceIdStorageReadLeaf(int key, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, length=512, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0xEB00C509, version : 150)]
		public virtual int sceIdStorageReadLeaf(int key, TPointer buffer)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x1FA4D135, version = 150) public int sceIdStorageWriteLeaf()
		[HLEFunction(nid : 0x1FA4D135, version : 150)]
		public virtual int sceIdStorageWriteLeaf()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x08A471A6, version = 150) public int sceIdStorageCreateLeaf()
		[HLEFunction(nid : 0x08A471A6, version : 150)]
		public virtual int sceIdStorageCreateLeaf()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2C97AB36, version = 150) public int sceIdStorageDeleteLeaf()
		[HLEFunction(nid : 0x2C97AB36, version : 150)]
		public virtual int sceIdStorageDeleteLeaf()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x99ACCB71, version = 150) public int sceIdStorage_driver_99ACCB71()
		[HLEFunction(nid : 0x99ACCB71, version : 150)]
		public virtual int sceIdStorage_driver_99ACCB71()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x37833CB8, version = 150) public int sceIdStorage_driver_37833CB8()
		[HLEFunction(nid : 0x37833CB8, version : 150)]
		public virtual int sceIdStorage_driver_37833CB8()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x31E08AFB, version = 150) public int sceIdStorageEnumId()
		[HLEFunction(nid : 0x31E08AFB, version : 150)]
		public virtual int sceIdStorageEnumId()
		{
			return 0;
		}

		/// <summary>
		/// Retrieves the value associated with a key.
		/// </summary>
		/// <param name="key">     	idstorage key </param>
		/// <param name="offset">    offset within the 512 byte leaf </param>
		/// <param name="buffer">    buffer with enough storage </param>
		/// <param name="length">    amount of data to retrieve (offset + length must be <= 512 bytes)
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x6FE062D1, version = 150) public int sceIdStorageLookup(int key, int offset, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer buffer, int length)
		[HLEFunction(nid : 0x6FE062D1, version : 150)]
		public virtual int sceIdStorageLookup(int key, int offset, TPointer buffer, int length)
		{
			buffer.clear(length);
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x683AAC10, version = 150) public int sceIdStorageUpdate()
		[HLEFunction(nid : 0x683AAC10, version : 150)]
		public virtual int sceIdStorageUpdate()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3AD32523, version = 150) public int sceIdStorageFlush()
		[HLEFunction(nid : 0x3AD32523, version : 150)]
		public virtual int sceIdStorageFlush()
		{
			return 0;
		}
	}

}