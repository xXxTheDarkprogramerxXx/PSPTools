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
//	import static Integer.rotateRight;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.HLEModuleManager.InternalSyscallNid;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.Modules.sceChkregModule;


	//using Logger = org.apache.log4j.Logger;

	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using IVirtualFileSystem = pspsharp.HLE.VFS.IVirtualFileSystem;
	using CompressPrxVirtualFileSystem = pspsharp.HLE.VFS.compress.CompressPrxVirtualFileSystem;
	using Fat12VirtualFile = pspsharp.HLE.VFS.fat.Fat12VirtualFile;
	using LocalVirtualFileSystem = pspsharp.HLE.VFS.local.LocalVirtualFileSystem;
	using PatchFileVirtualFileSystem = pspsharp.HLE.VFS.patch.PatchFileVirtualFileSystem;
	using SceNandSpare = pspsharp.HLE.kernel.types.SceNandSpare;
	using Wlan = pspsharp.hardware.Wlan;
	using Settings = pspsharp.settings.Settings;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;
	using Utilities = pspsharp.util.Utilities;

	public class sceNand : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceNand");
		private const int STATE_VERSION = 0;
		private const bool emulateNand = true;
		public const int pageSize = 0x200; // 512B per page
		public const int pagesPerBlock = 0x20; // 16KB per block
		private const int totalBlocks = 0x800; // 32MB in total
		private const int idStoragePpnStart = 0x600;
		private const int idStoragePpnEnd = 0x7FF;
		private const int iplId = 0x6DC64A38;
		private const int idStorageId = unchecked((int)0xFFFF0101);
		private sbyte[] dumpBlocks;
		private sbyte[] dumpSpares;
		private int[] dumpResults;
		private int[] ppnToLbn = new int[0x10000];
		private bool writeProtected;
		private int scramble;
		private Fat12VirtualFile vFile3;
		private Fat12VirtualFile vFile603;
		private Fat12VirtualFile vFile703;
		private static readonly int[] idStorageKeys = new int[] {0xFFFF, 0xFFFF, 0x0004, 0x0008, 0x0006, 0x0010, 0x0011, 0x0041, 0x0043, 0x0044, 0x0045, 0x0046, 0x0047, 0x0054, 0x0100, 0x0101, 0x0102, 0x0103, 0x0104, 0x0105, 0x0106, 0x0120, 0x0121, 0x0122, 0x0123, 0x0124, 0x0125, 0x0126};
		public static readonly int[] regionCodes = new int[] {unchecked((int)0xFFFFFFFF), unchecked((int)0x80000001), 0x00000002, unchecked((int)0x80000000), 0x0000000F, unchecked((int)0x80000000), 0x00000012, unchecked((int)0x80000000), 0x0000001F, unchecked((int)0x80000000), 0x00000022, unchecked((int)0x80000000), 0x0000002F, unchecked((int)0x80000000), 0x00000032, unchecked((int)0x80000000), 0x0000003F, unchecked((int)0x80000000), 0x00000042, unchecked((int)0x80000000), 0x0000004F, unchecked((int)0x80000000), 0x1000000F, unchecked((int)0x80000000), 0x1000001F, unchecked((int)0x80000000), 0x1000002F, unchecked((int)0x80000000), 0x1000003F, unchecked((int)0x80000000), 0x1000004F, unchecked((int)0x80000000), 0x2000000F, unchecked((int)0x80000000), 0x00000001, 0x00000000};

		public override void start()
		{
			writeProtected = true;
			scramble = 0;

			dumpBlocks = readBytes("nand.block");
			dumpSpares = readBytes("nand.spare");
			dumpResults = readInts("nand.result");

			int lbn = 0;
			for (int ppn = 0; ppn < ppnToLbn.Length; ppn++)
			{
				if (ppn < 0x800)
				{
					ppnToLbn[ppn] = 0x0000;
				}
				else
				{
					// The PSP code requires that we leave 16 blocks free every 0x1F0 blocks.
					// These are maybe used for bad blocks
					int freeBlockNumber = ((ppn - 0x800) / pagesPerBlock) % 0x1F0;
					const int freeBlockAreaStart = 0x7; // Trial and error show that valid values are from 0x7 to 0x62 (why???)
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int freeBlockAreaEnd = freeBlockAreaStart + 16;
					int freeBlockAreaEnd = freeBlockAreaStart + 16;

					if (freeBlockNumber >= freeBlockAreaStart && freeBlockNumber < freeBlockAreaEnd)
					{
						ppnToLbn[ppn] = 0xFFFF;
					}
					else
					{
						ppnToLbn[ppn] = lbn;
						if ((ppn % pagesPerBlock) == pagesPerBlock - 1)
						{
							lbn++;
						}
					}
				}
			}

			//if (log.DebugEnabled)
			{
				for (int ppn = 0; ppn < ppnToLbn.Length; ppn++)
				{
					if (ppnToLbn[ppn] == 0xFFFF)
					{
						int startFreePpn = ppn;
						int endFreePpn = ppn;
						for (; ppn < ppnToLbn.Length; ppn++)
						{
							if (ppnToLbn[ppn] != 0xFFFF)
							{
								ppn--;
								endFreePpn = ppn;
								break;
							}
						}

						Console.WriteLine(string.Format("Free blocks ppn=0x{0:X}-0x{1:X}", startFreePpn, endFreePpn));
					}
				}
			}

			if (log.TraceEnabled)
			{
				for (int ppn = 0; ppn < ppnToLbn.Length; ppn++)
				{
					log.trace(string.Format("ppn=0x{0:X4} -> lbn=0x{1:X4}", ppn, ppnToLbn[ppn]));
				}
			}

			if (!emulateNand)
			{
				sbyte[] fuseId = readBytes("nand.fuseid");
				if (fuseId != null && fuseId.Length == 8)
				{
					Modules.sceSysregModule.FuseId = Utilities.readUnaligned64(fuseId, 0);
				}
			}

			base.start();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public override void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			writeProtected = stream.readBoolean();
			scramble = stream.readInt();
			for (int i = 0; i < ppnToLbn.Length; i++)
			{
				ppnToLbn[i] = stream.readUnsignedShort();
			}

			bool vFile3present = stream.readBoolean();
			if (vFile3present)
			{
				openFile3();
				vFile3.read(stream);
			}

			bool vFile603present = stream.readBoolean();
			if (vFile603present)
			{
				openFile603();
				vFile603.read(stream);
			}

			bool vFile703present = stream.readBoolean();
			if (vFile703present)
			{
				openFile703();
				vFile703.read(stream);
			}

			base.read(stream);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public override void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			stream.writeBoolean(writeProtected);
			stream.writeInt(scramble);
			for (int i = 0; i < ppnToLbn.Length; i++)
			{
				stream.writeShort(ppnToLbn[i]);
			}

			if (vFile3 != null)
			{
				stream.writeBoolean(true);
				vFile3.write(stream);
			}
			else
			{
				stream.writeBoolean(false);
			}

			if (vFile603 != null)
			{
				stream.writeBoolean(true);
				vFile603.write(stream);
			}
			else
			{
				stream.writeBoolean(false);
			}

			if (vFile703 != null)
			{
				stream.writeBoolean(true);
				vFile703.write(stream);
			}
			else
			{
				stream.writeBoolean(false);
			}

			base.write(stream);
		}

		private static sbyte[] readBytes(string fileName)
		{
			sbyte[] bytes = null;
			try
			{
				File file = new File(fileName);
				System.IO.Stream @is = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				bytes = new sbyte[(int) file.Length()];
				@is.Read(bytes, 0, bytes.Length);
				@is.Close();
			}
			catch (FileNotFoundException)
			{
			}
			catch (IOException)
			{
			}

			return bytes;
		}

		private static int[] readInts(string fileName)
		{
			sbyte[] bytes = readBytes(fileName);
			if (bytes == null)
			{
				return null;
			}

			int[] ints = new int[bytes.Length / 4];
			ByteBuffer.wrap(bytes).order(ByteOrder.LITTLE_ENDIAN).asIntBuffer().get(ints);

			return ints;
		}

		public static void scramblePage(int scramble, int ppn, int[] source, int[] destination)
		{
			int scrmb = rotateRight(scramble, 21);
			int key = rotateRight(ppn, 17) ^ (scrmb * 7);
			int scrambleOffset = (((ppn ^ scrmb) & 0x1F) << 4) >> 2;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int pageSize4 = pageSize >> 2;
			int pageSize4 = pageSize >> 2;
			for (int i = 0; i < pageSize4;)
			{
				int value0 = source[i++];
				int value1 = source[i++];
				int value2 = source[i++];
				int value3 = source[i++];
				if (scrambleOffset >= pageSize4)
				{
					scrambleOffset -= pageSize4;
				}
				destination[scrambleOffset++] = value0 + key;
				key += value0;
				destination[scrambleOffset++] = value1 + key;
				key ^= value1;
				destination[scrambleOffset++] = value2 + key;
				key -= value2;
				destination[scrambleOffset++] = value3 + key;
				key += value3;
				key += scrmb;
				key = Integer.reverse(key);
			}
		}

		public static void descramblePage(int scramble, int ppn, int[] source, int[] destination)
		{
			int scrmb = rotateRight(scramble, 21);
			int key = rotateRight(ppn, 17) ^ (scrmb * 7);
			int scrambleOffset = (((ppn ^ scrmb) & 0x1F) << 4) >> 2;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int pageSize4 = pageSize >> 2;
			int pageSize4 = pageSize >> 2;
			for (int i = 0; i < pageSize4;)
			{
				int value0 = source[scrambleOffset++];
				int value1 = source[scrambleOffset++];
				int value2 = source[scrambleOffset++];
				int value3 = source[scrambleOffset++];
				if (scrambleOffset >= pageSize4)
				{
					scrambleOffset -= pageSize4;
				}
				value0 -= key;
				key += value0;
				destination[i++] = value0;
				value1 -= key;
				key ^= value1;
				destination[i++] = value1;
				value2 -= key;
				key -= value2;
				destination[i++] = value2;
				value3 -= key;
				key += value3;
				destination[i++] = value3;
				key += scrmb;
				key = Integer.reverse(key);
			}
		}

		protected internal virtual void descramblePage(int ppn, TPointer user, sbyte[] blocks, int offset)
		{
			int scrmb = rotateRight(scramble, 21);
			int key = rotateRight(ppn, 17) ^ (scrmb * 7);
			int scrambleOffset = ((ppn ^ scrmb) & 0x1F) << 4;

			for (int i = 0; i < pageSize; i += 16)
			{
				int value0 = Utilities.readUnaligned32(blocks, offset + scrambleOffset);
				int value1 = Utilities.readUnaligned32(blocks, offset + scrambleOffset + 4);
				int value2 = Utilities.readUnaligned32(blocks, offset + scrambleOffset + 8);
				int value3 = Utilities.readUnaligned32(blocks, offset + scrambleOffset + 12);
				scrambleOffset += 16;
				if (scrambleOffset >= pageSize)
				{
					scrambleOffset -= pageSize;
				}
				value0 -= key;
				key += value0;
				user.setValue32(i, value0);
				value1 -= key;
				key ^= value1;
				user.setValue32(i + 4, value1);
				value2 -= key;
				key -= value2;
				user.setValue32(i + 8, value2);
				value3 -= key;
				key += value3;
				user.setValue32(i + 12, value3);
				key += scrmb;
				key = Integer.reverse(key);
			}
		}

		protected internal virtual void descramble(int ppn, TPointer user, int len, sbyte[] blocks, int offset)
		{
			for (int i = 0; i < len; i++)
			{
				descramblePage(ppn, user, blocks, offset);
				ppn++;
				offset += pageSize;
				user.add(pageSize);
			}
		}

		private void readMasterBootRecord0(TPointer buffer)
		{
			// First partition entry
			int partitionEntry = 446;

			// Status of physical drive
			buffer.setValue8(partitionEntry + 0, (sbyte) 0x00);
			// CHS address of first absolute sector in partition
			buffer.setValue8(partitionEntry + 1, (sbyte) 0x00);
			buffer.setValue8(partitionEntry + 2, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 3, (sbyte) 0x01);
			// Partition type
			buffer.setValue8(partitionEntry + 4, (sbyte) 0x05);
			// CHS address of last absolute sector in partition
			buffer.setValue8(partitionEntry + 5, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 6, unchecked((sbyte) 0xE0));
			buffer.setValue8(partitionEntry + 7, unchecked((sbyte) 0xBE));
			// LBA of first absolute sector in the partition
			buffer.setUnalignedValue32(partitionEntry + 8, 0x40);
			// Number of sectors in partition
			buffer.setUnalignedValue32(partitionEntry + 12, 0xEF80);

			// Boot signature
			buffer.setValue8(510, (sbyte) 0x55);
			buffer.setValue8(511, unchecked((sbyte) 0xAA));
		}

		private void readMasterBootRecord2(TPointer buffer)
		{
			// First partition entry
			int partitionEntry = 446;

			// Status of physical drive
			buffer.setValue8(partitionEntry + 0, (sbyte) 0x00);
			// CHS address of first absolute sector in partition
			buffer.setValue8(partitionEntry + 1, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 2, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 3, (sbyte) 0x01);
			// Partition type
			buffer.setValue8(partitionEntry + 4, (sbyte) 0x01);
			// CHS address of last absolute sector in partition
			buffer.setValue8(partitionEntry + 5, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 6, unchecked((sbyte) 0xE0));
			buffer.setValue8(partitionEntry + 7, (sbyte) 0x00);
			// LBA of first absolute sector in the partition
			buffer.setUnalignedValue32(partitionEntry + 8, 0x20);
			// Number of sectors in partition
			buffer.setUnalignedValue32(partitionEntry + 12, 0xBFE0);

			// Second partition entry
			partitionEntry += 16;

			// Status of physical drive
			buffer.setValue8(partitionEntry + 0, (sbyte) 0x00);
			// CHS address of first absolute sector in partition
			buffer.setValue8(partitionEntry + 1, (sbyte) 0x00);
			buffer.setValue8(partitionEntry + 2, unchecked((sbyte) 0xC1));
			buffer.setValue8(partitionEntry + 3, (sbyte) 0x01);
			// Partition type
			buffer.setValue8(partitionEntry + 4, (sbyte) 0x05);
			// CHS address of last absolute sector in partition
			buffer.setValue8(partitionEntry + 5, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 6, unchecked((sbyte) 0xE0));
			buffer.setValue8(partitionEntry + 7, unchecked((sbyte) 0x80));
			// LBA of first absolute sector in the partition
			buffer.setUnalignedValue32(partitionEntry + 8, 0xC000);
			// Number of sectors in partition
			buffer.setUnalignedValue32(partitionEntry + 12, 0x2000);

			// Boot signature
			buffer.setValue8(510, (sbyte) 0x55);
			buffer.setValue8(511, unchecked((sbyte) 0xAA));
		}

		private void readMasterBootRecord602(TPointer buffer)
		{
			// First partition entry
			int partitionEntry = 446;

			// Status of physical drive
			buffer.setValue8(partitionEntry + 0, (sbyte) 0x00);
			// CHS address of first absolute sector in partition
			buffer.setValue8(partitionEntry + 1, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 2, unchecked((sbyte) 0xC1));
			buffer.setValue8(partitionEntry + 3, (sbyte) 0x01);
			// Partition type
			buffer.setValue8(partitionEntry + 4, (sbyte) 0x01);
			// CHS address of last absolute sector in partition
			buffer.setValue8(partitionEntry + 5, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 6, unchecked((sbyte) 0xE0));
			buffer.setValue8(partitionEntry + 7, unchecked((sbyte) 0x80));
			// LBA of first absolute sector in the partition
			buffer.setUnalignedValue32(partitionEntry + 8, 0x20);
			// Number of sectors in partition
			buffer.setUnalignedValue32(partitionEntry + 12, 0x1FE0);

			// Second partition entry
			partitionEntry += 16;

			// Status of physical drive
			buffer.setValue8(partitionEntry + 0, (sbyte) 0x00);
			// CHS address of first absolute sector in partition
			buffer.setValue8(partitionEntry + 1, (sbyte) 0x00);
			buffer.setValue8(partitionEntry + 2, unchecked((sbyte) 0xC1));
			buffer.setValue8(partitionEntry + 3, unchecked((sbyte) 0x81));
			// Partition type
			buffer.setValue8(partitionEntry + 4, (sbyte) 0x05);
			// CHS address of last absolute sector in partition
			buffer.setValue8(partitionEntry + 5, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 6, unchecked((sbyte) 0xE0));
			buffer.setValue8(partitionEntry + 7, unchecked((sbyte) 0xA0));
			// LBA of first absolute sector in the partition
			buffer.setUnalignedValue32(partitionEntry + 8, 0xE000);
			// Number of sectors in partition
			buffer.setUnalignedValue32(partitionEntry + 12, 0x800);

			// Boot signature
			buffer.setValue8(510, (sbyte) 0x55);
			buffer.setValue8(511, unchecked((sbyte) 0xAA));
		}

		private void readMasterBootRecord702(TPointer buffer)
		{
			// First partition entry
			int partitionEntry = 446;

			// Status of physical drive
			buffer.setValue8(partitionEntry + 0, (sbyte) 0x00);
			// CHS address of first absolute sector in partition
			buffer.setValue8(partitionEntry + 1, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 2, unchecked((sbyte) 0xC1));
			buffer.setValue8(partitionEntry + 3, unchecked((sbyte) 0x81));
			// Partition type
			buffer.setValue8(partitionEntry + 4, (sbyte) 0x01);
			// CHS address of last absolute sector in partition
			buffer.setValue8(partitionEntry + 5, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 6, unchecked((sbyte) 0xE0));
			buffer.setValue8(partitionEntry + 7, unchecked((sbyte) 0xA0));
			// LBA of first absolute sector in the partition
			buffer.setUnalignedValue32(partitionEntry + 8, 0x20);
			// Number of sectors in partition
			buffer.setUnalignedValue32(partitionEntry + 12, 0x7E0);

			// Second partition entry
			partitionEntry += 16;

			// Status of physical drive
			buffer.setValue8(partitionEntry + 0, (sbyte) 0x00);
			// CHS address of first absolute sector in partition
			buffer.setValue8(partitionEntry + 1, (sbyte) 0x00);
			buffer.setValue8(partitionEntry + 2, unchecked((sbyte) 0xC1));
			buffer.setValue8(partitionEntry + 3, unchecked((sbyte) 0xA1));
			// Partition type
			buffer.setValue8(partitionEntry + 4, (sbyte) 0x05);
			// CHS address of last absolute sector in partition
			buffer.setValue8(partitionEntry + 5, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 6, unchecked((sbyte) 0xE0));
			buffer.setValue8(partitionEntry + 7, unchecked((sbyte) 0xBE));
			// LBA of first absolute sector in the partition
			buffer.setUnalignedValue32(partitionEntry + 8, 0xE800);
			// Number of sectors in partition
			buffer.setUnalignedValue32(partitionEntry + 12, 0x780);

			// Boot signature
			buffer.setValue8(510, (sbyte) 0x55);
			buffer.setValue8(511, unchecked((sbyte) 0xAA));
		}

		private void readMasterBootRecord742(TPointer buffer)
		{
			// First partition entry
			int partitionEntry = 446;

			// Status of physical drive
			buffer.setValue8(partitionEntry + 0, (sbyte) 0x00);
			// CHS address of first absolute sector in partition
			buffer.setValue8(partitionEntry + 1, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 2, unchecked((sbyte) 0xC1));
			buffer.setValue8(partitionEntry + 3, unchecked((sbyte) 0xA1));
			// Partition type
			buffer.setValue8(partitionEntry + 4, (sbyte) 0x01);
			// CHS address of last absolute sector in partition
			buffer.setValue8(partitionEntry + 5, (sbyte) 0x01);
			buffer.setValue8(partitionEntry + 6, unchecked((sbyte) 0xE0));
			buffer.setValue8(partitionEntry + 7, unchecked((sbyte) 0xBE));
			// LBA of first absolute sector in the partition
			buffer.setUnalignedValue32(partitionEntry + 8, 0x20);
			// Number of sectors in partition
			buffer.setUnalignedValue32(partitionEntry + 12, 0x760);

			// Boot signature
			buffer.setValue8(510, (sbyte) 0x55);
			buffer.setValue8(511, unchecked((sbyte) 0xAA));
		}

		private void readFile(TPointer buffer, IVirtualFile vFile, int ppn, int lbnStart)
		{
			int lbn = ppnToLbn[ppn];
			int sectorNumber = (lbn - lbnStart) * pagesPerBlock + (ppn % pagesPerBlock);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("readFile ppn=0x{0:X}, lbnStart=0x{1:X}, lbn=0x{2:X}, sectorNumber=0x{3:X}", ppn, lbnStart, lbn, sectorNumber));
			}
			readFile(buffer, vFile, sectorNumber);
		}

		private void readFile(TPointer buffer, IVirtualFile vFile, int sectorNumber)
		{
			vFile.ioLseek(sectorNumber * pageSize);
			vFile.ioRead(buffer, pageSize);
		}

		private bool isIdStoragePageForKey(int page, int key)
		{
			if (page < 0 || page >= idStorageKeys.Length)
			{
				return false;
			}
			return idStorageKeys[page] == key;
		}

		private void writeStringType(TPointer buffer, int offset, string s)
		{
			buffer.setValue8(offset++, (sbyte)((s.Length + 1) * 2)); // number of bytes including terminating 0.
			buffer.setValue8(offset++, (sbyte) 3); // Showing a string type?
			for (int i = 0; i < s.Length; i++)
			{
				buffer.setValue8(offset++, (sbyte) s[i]);
				buffer.setValue8(offset++, (sbyte) 0);
			}
			// Terminating 0
			buffer.setValue8(offset++, (sbyte) 0);
			buffer.setValue8(offset++, (sbyte) 0);
		}

		private void readIdStoragePage(TPointer buffer, int page)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("readIdStoragePage page=0x{0:X}", page));
			}

			switch (page)
			{
				case 0:
					buffer.memset(unchecked((sbyte) 0xFF), pageSize);
					for (int i = 0; i < idStorageKeys.Length; i++)
					{
						buffer.setValue16(i << 1, (short) idStorageKeys[i]);
					}
					break;
				case 1:
					buffer.memset(unchecked((sbyte) 0xFF), pageSize);
					break;
				default:
					// Used by sceChkregCheckRegion()
					if (isIdStoragePageForKey(page, 0x0102))
					{
						buffer.setValue32(0x08C, 0x60); // Number of region code entries?
						for (int i = 0; i < regionCodes.Length; i++)
						{
							buffer.setValue32(0x0B0 + i * 4, regionCodes[i]);
						}
					// Used by sceChkreg_driver_6894A027()
					}
					else if (isIdStoragePageForKey(page, 0x100))
					{
						// A certificate is stored at offset 0x38
						int certificateOffset = 0x38;
						int certificateLength = 0xB8;
						buffer.clear(certificateOffset, certificateLength);
						int unknownValue = sceChkregModule.ValueReturnedBy6894A027;
						buffer.setValue8(certificateOffset + 8, (sbyte)((0x23 << 2) | ((unknownValue >> 6) & 0x03)));
						buffer.setValue8(certificateOffset + 9, unchecked((sbyte)((unknownValue << 2) & 0xFC)));
					// Used by usb.prx
					}
					else if (isIdStoragePageForKey(page, 0x41))
					{
						int offset = 0;
						buffer.setValue32(offset, 0x54C);
						offset += 4;
						writeStringType(buffer, 4, "Sony");
						offset += 64;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[] types = { 0x1C8, 0x1C9, 0x1CA, 0x1CB, 0x1CC };
						int[] types = new int[] {0x1C8, 0x1C9, 0x1CA, 0x1CB, 0x1CC};
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] typeNames = { "PSP Type A", "PSP Type B", "PSP Type C", "PSP Type D", "PSP Type E" };
						string[] typeNames = new string[] {"PSP Type A", "PSP Type B", "PSP Type C", "PSP Type D", "PSP Type E"};

						buffer.setValue32(offset, types.Length);
						offset += 4;

						for (int i = 0; i < types.Length; i++)
						{
							buffer.setValue32(offset, types[i]);
							offset += 4;
							writeStringType(buffer, offset, typeNames[i]);
							offset += 64;
						}
					// Used to display the MAC address in the VSH
					}
					else if (isIdStoragePageForKey(page, 0x44))
					{
						buffer.setArray(0, Wlan.MacAddress, Wlan.MAC_ADDRESS_LENGTH);
					}
					break;
			}
		}

		private int computeEcc(SceNandSpare spare)
		{
			int t0, t1, t2, t3, t4, t5, t6, t7, t9;
			int s0, s1, s2, s3, s4, s5;
			int a0, a3;
			int v0, v1;

			s3 = spare.blockFmt;
			v0 = spare.blockStat;
			t4 = spare.lbn >> 8;
			t2 = spare.lbn & 0xFF;
			s0 = spare.id & 0xFF;
			t7 = s3 ^ v0;
			s1 = (spare.id >> 8) & 0xFF;
			t9 = t4 ^ t7;
			s5 = (spare.id >> 16) & 0xFF;
			t6 = t2 ^ t9;
			v1 = s0 ^ t6;
			t9 = (spare.id >> 24) & 0xFF;
			a3 = s0 ^ s1;
			t5 = s1 ^ v1;
			s2 = t4 ^ t2;
			t0 = s5 ^ t5;
			t3 = s5 ^ a3;
			t5 = s5 ^ s2;
			s4 = t9 ^ t0;
			a3 = t9 ^ t3;
			a0 = s4 & 0xFF;
			s2 = v0 ^ t2;
			s4 = t6 & 0xFF;
			t3 = s0 ^ t7;
			t6 = a3 & 0xFF;
			t7 = t9 ^ t5;
			v0 = 0x6996;
			t0 = t7 & 0xFF;
			a3 = s4 >> 4;
			t7 = s3 ^ t4;
			t5 = s4 & 0x0F;
			s3 = s1 ^ s2;
			s4 = t6 & 0x0F;
			s2 = s1 ^ t3;
			t4 = t6 >> 4;
			s1 = a0 & 0xCC;
			t6 = v0 >> t4;
			v1 = v0 >> s4;
			t3 = s2 & 0xFF;
			t4 = a0 >> 4;
			t1 = v0 >> t5;
			s2 = s1 >> 4;
			t5 = v0 >> a3;
			s1 = s0 ^ t7;
			a3 = a0 & 0x0F;
			s0 = t9 ^ s3;
			t7 = a0 & 0x0C;
			s3 = t0 >> 4;
			s4 = t0 & 0x0F;
			t2 = s0 & 0xFF;
			s2 = v0 >> s2;
			s0 = v0 >> t7;
			t0 = v0 >> s4;
			t7 = v0 >> a3;
			s4 = v0 >> s3;
			a3 = v0 >> t4;
			t1 = t1 ^ t5;
			t4 = s5 ^ s1;
			t5 = v1 ^ t6;
			s5 = t3 >> 4;
			s3 = a0 & 0xAA;
			t6 = a0 & 0x03;
			s1 = (a0 >> 4) & 0x03;
			t3 = t3 & 0x0F;
			s1 = v0 >> s1;
			s5 = v0 >> s5;
			s0 = s0 ^ s2;
			t0 = t0 ^ s4;
			t4 = t4 & 0xFF;
			t6 = v0 >> t6;
			s2 = t7 & 0x01;
			s4 = t2 >> 4;
			t3 = v0 >> t3;
			v1 = t5 & 0x01;
			s3 = s3 >> 4;
			t5 = a0 & 0x0A;
			a3 = a3 & 0x01;
			t1 = t1 & 0x01;
			t2 = t2 & 0x0F;
			t6 = t6 ^ s1;
			t3 = t3 ^ s5;
			s3 = v0 >> s3;
			s5 = v0 >> t5;
			t7 = s2 << 2;
			s4 = v0 >> s4;
			a3 = a3 << 8;
			s2 = t4 >> 4;
			t2 = v0 >> t2;
			t1 = t1 << 5;
			s1 = a0 & 0x55;
			s0 = s0 & 0x01;
			t0 = t0 & 0x01;
			v1 = v1 << 11;
			t4 = t4 & 0x0F;
			t5 = s5 ^ s3;
			t2 = t2 ^ s4;
			s5 = a3 | t7;
			s4 = t6 & 0x01;
			s2 = v0 >> s2;
			t7 = t3 & 0x01;
			v1 = v1 | t1;
			s1 = s1 >> 4;
			t1 = v0 >> t4;
			s0 = s0 << 7;
			t0 = t0 << 10;
			a0 = a0 & 0x05;
			s3 = s5 | s0;
			s5 = t1 ^ s2;
			s0 = s4 << 1;
			s2 = v0 >> a0;
			t1 = v1 | t0;
			v0 = v0 >> s1;
			t0 = t5 & 0x01;
			s1 = t7 << 4;
			s4 = t2 & 0x01;
			t7 = s2 ^ v0;
			t6 = s5 & 0x01;
			s2 = s3 | s0;
			s0 = t1 | s1;
			s3 = t0 << 6;
			s1 = s4 << 9;
			v0 = s0 | s1;
			t3 = s2 | s3;
			t2 = t7 & 0x01;
			t1 = t6 << 3;
			a0 = v0 | t1;
			t0 = t3 | t2;
			v0 = t0 | a0;

			return v0;
		}

		private void openFile3()
		{
			if (vFile3 != null)
			{
				return;
			}

			IVirtualFileSystem vfs = new LocalVirtualFileSystem(Settings.Instance.getDirectoryMapping("flash0"), false);

			// Apply patches for some files as required
			vfs = new PatchFileVirtualFileSystem(vfs);

			// All the PRX files need to be compressed so that they can fit
			// into the space available on flash0.
			vfs = new CompressPrxVirtualFileSystem(vfs);

			vFile3 = new Fat12VirtualFile("flash0:", vfs, 0xBFE0);
			vFile3.scan();
		}

		private void openFile603()
		{
			if (vFile603 != null)
			{
				return;
			}

			IVirtualFileSystem vfs = new LocalVirtualFileSystem(Settings.Instance.getDirectoryMapping("flash1"), false);
			vFile603 = new Fat12VirtualFile("flash1:", vfs, 0x1FE0);
			vFile603.scan();
		}

		private void openFile703()
		{
			if (vFile703 != null)
			{
				return;
			}

			IVirtualFileSystem vfs = new LocalVirtualFileSystem(Settings.Instance.getDirectoryMapping("flash2"), false);
			vFile703 = new Fat12VirtualFile("flash2:", vfs, 0x7E0);
			vFile703.scan();
		}

		public virtual int hleNandReadPages(int ppn, TPointer user, TPointer spare, int len, bool raw, bool spareUserEcc, bool isLLE)
		{
			if (user.NotNull)
			{
				if (dumpBlocks != null && !emulateNand)
				{
					if (scramble != 0)
					{
						descramble(ppn, user, len, dumpBlocks, ppn * pageSize);
					}
					else
					{
						Utilities.writeBytes(user.Address, len * pageSize, dumpBlocks, ppn * pageSize);
					}
				}
				else
				{
					for (int i = 0; i < len; i++)
					{
						user.clear(pageSize);
						if ((ppn + i) == 0x80)
						{
							for (int n = 0; n < 8; n++)
							{
								user.setValue16(n * 2, (short)(16 + n));
							}
						}
						else if ((ppn + i) >= idStoragePpnStart && (ppn + i) <= idStoragePpnEnd)
						{
							readIdStoragePage(user, ppn + i - idStoragePpnStart);
						}
						else if (ppnToLbn[ppn + i] == 0)
						{
							// Master Boot Record
							readMasterBootRecord0(user);
						}
						else if (ppnToLbn[ppn + i] == 2)
						{
							// Master Boot Record
							readMasterBootRecord2(user);
						}
						else if (ppnToLbn[ppn + i] >= 0x3 && ppnToLbn[ppn + i] < 0x602)
						{
							openFile3();
							readFile(user, vFile3, ppn + i, 0x3);
						}
						else if (ppnToLbn[ppn + i] == 0x602)
						{
							// Master Boot Record
							readMasterBootRecord602(user);
						}
						else if (ppnToLbn[ppn + i] >= 0x603 && ppnToLbn[ppn + i] < 0x702)
						{
							openFile603();
							readFile(user, vFile603, ppn + i, 0x603);
						}
						else if (ppnToLbn[ppn + i] == 0x702)
						{
							// Master Boot Record
							readMasterBootRecord702(user);
						}
						else if (ppnToLbn[ppn + i] >= 0x703 && ppnToLbn[ppn + i] < 0x742)
						{
							openFile703();
							readFile(user, vFile703, ppn + i, 0x703);
						}
						else if (ppnToLbn[ppn + i] == 0x742)
						{
							// Master Boot Record
							readMasterBootRecord742(user);
						}
						user.add(pageSize);
					}
				}
			}

			if (spare.NotNull)
			{
				if (dumpSpares != null && !emulateNand)
				{
					if (spareUserEcc)
					{
						// Write the userEcc
						Utilities.writeBytes(spare.Address, len * 16, dumpSpares, ppn * 16);
					}
					else
					{
						// Do not return the userEcc
						for (int i = 0; i < len; i++)
						{
							Utilities.writeBytes(spare.Address + i * 12, 12, dumpSpares, (ppn + i) * 16 + 4);
						}
					}
				}
				else
				{
					SceNandSpare sceNandSpare = new SceNandSpare();
					for (int i = 0; i < len; i++)
					{
						sceNandSpare.blockFmt = (ppn + i) < 0x800 ? 0xFF : 0x00;
						sceNandSpare.blockStat = 0xFF;
						sceNandSpare.lbn = ppnToLbn[ppn + i];
						if (ppn == 0x80)
						{
							sceNandSpare.id = iplId; // For IPL area
						}
						else if (ppn >= idStoragePpnStart && ppn <= idStoragePpnEnd)
						{
							sceNandSpare.id = idStorageId; // For ID Storage area
							sceNandSpare.lbn = 0x7301;
						}
						sceNandSpare.reserved2[0] = 0xFF;
						sceNandSpare.reserved2[1] = 0xFF;

						sceNandSpare.spareEcc = computeEcc(sceNandSpare);

						if (!isLLE)
						{
							// All values are set to 0xFF when the lbn is 0xFFFF
							if (sceNandSpare.lbn == 0xFFFF)
							{
								sceNandSpare.userEcc[0] = 0xFF;
								sceNandSpare.userEcc[1] = 0xFF;
								sceNandSpare.userEcc[2] = 0xFF;
								sceNandSpare.reserved1 = 0xFF;
								sceNandSpare.blockFmt = 0xFF;
								sceNandSpare.blockStat = 0xFF;
								sceNandSpare.id = unchecked((int)0xFFFFFFFF);
								sceNandSpare.spareEcc = 0xFFFF;
								sceNandSpare.reserved2[0] = 0xFF;
								sceNandSpare.reserved2[1] = 0xFF;
							}
						}

						if (spareUserEcc)
						{
							sceNandSpare.write(spare, i * sceNandSpare.@sizeof());
						}
						else
						{
							sceNandSpare.writeNoUserEcc(spare, i * sceNandSpare.sizeofNoEcc());
						}
					}
				}
			}

			int result = 0;
			if (dumpResults != null && !emulateNand)
			{
				result = dumpResults[ppn / pagesPerBlock];
			}

			return result;
		}

		private void writeFile(TPointer buffer, IVirtualFile vFile, int ppn, int lbnStart)
		{
			int lbn = ppnToLbn[ppn];
			int sectorNumber = (lbn - lbnStart) * pagesPerBlock + (ppn % pagesPerBlock);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("writeFile ppn=0x{0:X}, lbnStart=0x{1:X}, lbn=0x{2:X}, sectorNumber=0x{3:X}", ppn, lbnStart, lbn, sectorNumber));
			}
			writeFile(buffer, vFile, sectorNumber);
		}

		private void writeFile(TPointer buffer, IVirtualFile vFile, int sectorNumber)
		{
			vFile.ioLseek(sectorNumber * pageSize);
			vFile.ioWrite(buffer, pageSize);
		}

		public virtual int hleNandWriteSparePages(int ppn, TPointer spare, int len, bool raw, bool spareUserEcc, bool isLLE)
		{
			int result = 0;

			if (spare.NotNull)
			{
				SceNandSpare sceNandSpare = new SceNandSpare();
				for (int i = 0; i < len; i++)
				{
					if (spareUserEcc)
					{
						sceNandSpare.read(spare, i * sceNandSpare.@sizeof());
					}
					else
					{
						sceNandSpare.readNoUserEcc(spare, i * sceNandSpare.sizeofNoEcc());
					}

					if (sceNandSpare.lbn != 0xFFFF && ppnToLbn[ppn + i] != sceNandSpare.lbn)
					{
						int offset = (ppn + i) % pagesPerBlock;
						for (int j = offset; j < ppnToLbn.Length; j += pagesPerBlock)
						{
							if (ppnToLbn[j] == sceNandSpare.lbn)
							{
								//if (log.DebugEnabled)
								{
									Console.WriteLine(string.Format("hleNandWriteSparePages moving lbn=0x{0:X4} from ppn=0x{1:X} to ppn=0x{2:X}", sceNandSpare.lbn, j, ppn + i));
								}
								ppnToLbn[j] = 0xFFFF;
								break;
							}
						}

						if (ppnToLbn[ppn + i] == 0xFFFF)
						{
							ppnToLbn[ppn + i] = sceNandSpare.lbn;
						}
						else
						{
							Console.WriteLine(string.Format("hleNandWriteSparePages moving lbn=0x{0:X4} to ppn=0x{1:X} not being free", sceNandSpare.lbn, ppn + i));
						}
					}
				}
			}

			return result;
		}

		public virtual int hleNandWriteUserPages(int ppn, TPointer user, int len, bool raw, bool isLLE)
		{
			int result = 0;

			if (user.NotNull)
			{
				for (int i = 0; i < len; i++)
				{
					if (ppnToLbn[ppn + i] >= 0x3 && ppnToLbn[ppn + i] < 0x602)
					{
						openFile3();
						writeFile(user, vFile3, ppn + i, 0x3);
					}
					else if (ppnToLbn[ppn + i] >= 0x603 && ppnToLbn[ppn + i] < 0x702)
					{
						openFile603();
						writeFile(user, vFile603, ppn + i, 0x603);
					}
					else if (ppnToLbn[ppn + i] >= 0x703 && ppnToLbn[ppn + i] < 0x742)
					{
						openFile703();
						writeFile(user, vFile703, ppn + i, 0x703);
					}
					else
					{
						Console.WriteLine(string.Format("hleNandWriteUserPages unimplemented write on ppn=0x{0:X}, lbn=0x{1:X}", ppn + i, ppnToLbn[ppn + i]));
					}
					user.add(pageSize);
				}
			}

			return result;
		}

		public virtual int hleNandWritePages(int ppn, TPointer user, TPointer spare, int len, bool raw, bool spareUserEcc, bool isLLE)
		{
			int result = hleNandWriteSparePages(ppn, spare, len, raw, spareUserEcc, isLLE);
			if (result != 0)
			{
				return result;
			}

			return hleNandWriteUserPages(ppn, user, len, raw, isLLE);
		}

		public virtual int getLbnFromPpn(int ppn)
		{
			return ppnToLbn[ppn];
		}

		[HLEFunction(nid : 0xB07C41D4, version : 150)]
		public virtual int sceNandGetPagesPerBlock()
		{
			// Has no parameters
			return pagesPerBlock;
		}

		[HLEFunction(nid : 0xCE9843E6, version : 150)]
		public virtual int sceNandGetPageSize()
		{
			// Has no parameters
			return pageSize;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xAE4438C7, version = 150) public int sceNandLock(int mode)
		[HLEFunction(nid : 0xAE4438C7, version : 150)]
		public virtual int sceNandLock(int mode)
		{
			sceNandSetWriteProtect(mode == 0);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x41FFA822, version = 150) public int sceNandUnlock()
		[HLEFunction(nid : 0x41FFA822, version : 150)]
		public virtual int sceNandUnlock()
		{
			// Has no parameters
			sceNandSetWriteProtect(true);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x01F09203, version = 150) public int sceNandIsBadBlock(int ppn)
		[HLEFunction(nid : 0x01F09203, version : 150)]
		public virtual int sceNandIsBadBlock(int ppn)
		{
			if ((ppn % pagesPerBlock) != 0)
			{
				return -1;
			}

			int result = 0;
			if (dumpSpares != null)
			{
				int blockStat = dumpSpares[ppn * 16 + 5] & 0xFF;
				if (blockStat == 0xFF)
				{
					result = 0;
				}
				else
				{
					result = 1;
				}
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0BEE8F36, version = 150) public int sceNandSetScramble(int scramble)
		[HLEFunction(nid : 0x0BEE8F36, version : 150)]
		public virtual int sceNandSetScramble(int scramble)
		{
			this.scramble = scramble;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x84EE5D76, version = 150) public boolean sceNandSetWriteProtect(boolean protect)
		[HLEFunction(nid : 0x84EE5D76, version : 150)]
		public virtual bool sceNandSetWriteProtect(bool protect)
		{
			bool result = writeProtected;

			writeProtected = protect;

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8932166A, version = 150) public int sceNandWritePagesRawExtra(int ppn, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=pageSize, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer user, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=16, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer spare, int len)
		[HLEFunction(nid : 0x8932166A, version : 150)]
		public virtual int sceNandWritePagesRawExtra(int ppn, TPointer user, TPointer spare, int len)
		{
			return hleNandWritePages(ppn, user, spare, len, true, false, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5182C394, version = 150) public int sceNandReadExtraOnly(int ppn, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=16, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer spare, int len)
		[HLEFunction(nid : 0x5182C394, version : 150)]
		public virtual int sceNandReadExtraOnly(int ppn, TPointer spare, int len)
		{
			hleNandReadPages(ppn, TPointer.NULL, spare, len, true, true, false);
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x89BDCA08, version = 150) public int sceNandReadPages(int ppn, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=pageSize, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer user, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer spare, int len)
		[HLEFunction(nid : 0x89BDCA08, version : 150)]
		public virtual int sceNandReadPages(int ppn, TPointer user, TPointer spare, int len)
		{
			return hleNandReadPages(ppn, user, spare, len, false, false, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE05AE88D, version = 150) public int sceNandReadPagesRawExtra(int ppn, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=pageSize, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer user, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer spare, int len)
		[HLEFunction(nid : 0xE05AE88D, version : 150)]
		public virtual int sceNandReadPagesRawExtra(int ppn, TPointer user, TPointer spare, int len)
		{
			return hleNandReadPages(ppn, user, spare, len, true, false, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC32EA051, version = 150) public int sceNandReadBlockWithRetry(int ppn, pspsharp.HLE.TPointer user, pspsharp.HLE.TPointer spare)
		[HLEFunction(nid : 0xC32EA051, version : 150)]
		public virtual int sceNandReadBlockWithRetry(int ppn, TPointer user, TPointer spare)
		{
			return hleNandReadPages(ppn, user, spare, pagesPerBlock, false, false, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xB2B021E5, version = 150) public int sceNandWriteBlockWithVerify(int ppn, pspsharp.HLE.TPointer user, pspsharp.HLE.TPointer spare)
		[HLEFunction(nid : 0xB2B021E5, version : 150)]
		public virtual int sceNandWriteBlockWithVerify(int ppn, TPointer user, TPointer spare)
		{
			return hleNandWritePages(ppn, user, spare, pagesPerBlock, false, false, false);
		}

		[HLEFunction(nid : 0xC1376222, version : 150)]
		public virtual int sceNandGetTotalBlocks()
		{
			// Has no parameters
			return totalBlocks;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xE41A11DE, version = 150) public int sceNandReadStatus()
		[HLEFunction(nid : 0xE41A11DE, version : 150)]
		public virtual int sceNandReadStatus()
		{
			// Has no parameters
			int result = 0;
			if (!writeProtected)
			{
				result |= 0x80; // not write protected
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEB0A0022, version = 150) public int sceNandEraseBlock(int ppn)
		[HLEFunction(nid : 0xEB0A0022, version : 150)]
		public virtual int sceNandEraseBlock(int ppn)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7AF7B77A, version = 150) public int sceNandReset(@CanBeNull @BufferInfo(usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer8 statusAddr)
		[HLEFunction(nid : 0x7AF7B77A, version : 150)]
		public virtual int sceNandReset(TPointer8 statusAddr)
		{
			statusAddr.Value = 0;

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = InternalSyscallNid, version = 150) public boolean sceNandIsReady()
		[HLEFunction(nid : InternalSyscallNid, version : 150)]
		public virtual bool sceNandIsReady()
		{
			return true;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = InternalSyscallNid, version = 150) public int sceNandInit2()
		[HLEFunction(nid : InternalSyscallNid, version : 150)]
		public virtual int sceNandInit2()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = InternalSyscallNid, version = 150) public int sceNandTransferDataToNandBuf()
		[HLEFunction(nid : InternalSyscallNid, version : 150)]
		public virtual int sceNandTransferDataToNandBuf()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = InternalSyscallNid, version = 150) public int sceNandIntrHandler()
		[HLEFunction(nid : InternalSyscallNid, version : 150)]
		public virtual int sceNandIntrHandler()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = InternalSyscallNid, version = 150) public int sceNandTransferDataFromNandBuf()
		[HLEFunction(nid : InternalSyscallNid, version : 150)]
		public virtual int sceNandTransferDataFromNandBuf()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFCDF7610, version = 150) public int sceNandReadId(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer8 id, int len)
		[HLEFunction(nid : 0xFCDF7610, version : 150)]
		public virtual int sceNandReadId(TPointer8 id, int len)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x766756EF, version = 150) public int sceNandReadAccess(int ppn, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=pageSize, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer user, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer spare, int len, int mode)
		[HLEFunction(nid : 0x766756EF, version : 150)]
		public virtual int sceNandReadAccess(int ppn, TPointer user, TPointer spare, int len, int mode)
		{
			return hleNandReadPages(ppn, user, spare, len, false, false, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0ADC8686, version = 150) public int sceNandWriteAccess(int ppn, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=pageSize, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer user, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer spare, int len, int mode)
		[HLEFunction(nid : 0x0ADC8686, version : 150)]
		public virtual int sceNandWriteAccess(int ppn, TPointer user, TPointer spare, int len, int mode)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8AF0AB9F, version = 150) public int sceNandWritePages(int ppn, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=pageSize, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer user, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer spare, int len)
		[HLEFunction(nid : 0x8AF0AB9F, version : 150)]
		public virtual int sceNandWritePages(int ppn, TPointer user, TPointer spare, int len)
		{
			return hleNandWritePages(ppn, user, spare, len, false, false, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC478C1DE, version = 150) public int sceNandReadPagesRawAll(int ppn, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=pageSize, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer user, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer spare, int len)
		[HLEFunction(nid : 0xC478C1DE, version : 150)]
		public virtual int sceNandReadPagesRawAll(int ppn, TPointer user, TPointer spare, int len)
		{
			return hleNandReadPages(ppn, user, spare, len, true, false, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5AC02755, version = 150) public int sceNandVerifyBlockWithRetry(int ppn, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=pageSize, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer user, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer spare)
		[HLEFunction(nid : 0x5AC02755, version : 150)]
		public virtual int sceNandVerifyBlockWithRetry(int ppn, TPointer user, TPointer spare)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x8933B2E0, version = 150) public int sceNandEraseBlockWithRetry(int ppn)
		[HLEFunction(nid : 0x8933B2E0, version : 150)]
		public virtual int sceNandEraseBlockWithRetry(int ppn)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xC29DA136, version = 150) public int sceNandDoMarkAsBadBlock(int ppn)
		[HLEFunction(nid : 0xC29DA136, version : 150)]
		public virtual int sceNandDoMarkAsBadBlock(int ppn)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2FF6081B, version = 150) public int sceNandDetectChipMakersBBM(int ppn)
		[HLEFunction(nid : 0x2FF6081B, version : 150)]
		public virtual int sceNandDetectChipMakersBBM(int ppn)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBADD5D46, version = 150) public int sceNandWritePagesRawAll(int ppn, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=pageSize, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer user, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=12, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer spare, int len)
		[HLEFunction(nid : 0xBADD5D46, version : 150)]
		public virtual int sceNandWritePagesRawAll(int ppn, TPointer user, TPointer spare, int len)
		{
			return hleNandWritePages(ppn, user, spare, len, true, false, false);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xD897C343, version = 150) public int sceNandDetectChip()
		[HLEFunction(nid : 0xD897C343, version : 150)]
		public virtual int sceNandDetectChip()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3F76BC21, version = 150) public int sceNandDumpWearBBMSize()
		[HLEFunction(nid : 0x3F76BC21, version : 150)]
		public virtual int sceNandDumpWearBBMSize()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEBA0E6C6, version = 150) public int sceNandCountChipMakersBBM()
		[HLEFunction(nid : 0xEBA0E6C6, version : 150)]
		public virtual int sceNandCountChipMakersBBM()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2674CFFE, version = 150) public int sceNandEraseAllBlock()
		[HLEFunction(nid : 0x2674CFFE, version : 150)]
		public virtual int sceNandEraseAllBlock()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9B2AC433, version = 150) public int sceNandTestBlock()
		[HLEFunction(nid : 0x9B2AC433, version : 150)]
		public virtual int sceNandTestBlock()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x716CD2B2, version = 150) public int sceNandWriteBlock()
		[HLEFunction(nid : 0x716CD2B2, version : 150)]
		public virtual int sceNandWriteBlock()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEF55F193, version = 150) public int sceNandCalcEcc(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=8, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer buffer)
		[HLEFunction(nid : 0xEF55F193, version : 150)]
		public virtual int sceNandCalcEcc(TPointer buffer)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x18B78661, version = 150) public int sceNandVerifyEcc()
		[HLEFunction(nid : 0x18B78661, version : 150)]
		public virtual int sceNandVerifyEcc()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x88CC9F72, version = 150) public int sceNandCorrectEcc(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=8, usage=pspsharp.HLE.BufferInfo.Usage.inout) pspsharp.HLE.TPointer buffer, int ecc)
		[HLEFunction(nid : 0x88CC9F72, version : 150)]
		public virtual int sceNandCorrectEcc(TPointer buffer, int ecc)
		{
			return 0;
		}
	}

}