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

	//using Logger = org.apache.log4j.Logger;

	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Utilities = pspsharp.util.Utilities;

	public class sceMd5 : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceMd5");
		protected internal MessageDigest md5;

		public override void start()
		{
			try
			{
				md5 = MessageDigest.getInstance("MD5");
			}
			catch (NoSuchAlgorithmException e)
			{
				Console.WriteLine("Cannot find MD5", e);
			}

			base.start();
		}

		public override void stop()
		{
			md5 = null;
			base.stop();
		}

		protected internal static sbyte[] getMemoryBytes(int address, int size)
		{
			sbyte[] bytes = new sbyte[size];
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(address, size, 1);
			for (int i = 0; i < size; i++)
			{
				bytes[i] = (sbyte) memoryReader.readNext();
			}

			return bytes;
		}

		protected internal static void writeMd5Digest(int address, sbyte[] digest)
		{
			// The PSP returns 16 bytes
			const int digestLength = 16;
			int size = digest == null ? 0 : System.Math.Min(digest.Length, digestLength);
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(address, digestLength, 1);
			for (int i = 0; i < size; i++)
			{
				memoryWriter.writeNext(digest[i] & 0xFF);
			}
			for (int i = size; i < digestLength; i++)
			{
				memoryWriter.writeNext(0);
			}
			memoryWriter.flush();

			if (log.TraceEnabled)
			{
				log.trace(string.Format("return MD5 digest: {0}", Utilities.getMemoryDump(address, digestLength)));
			}
		}

		[HLELogging(level:"info"), HLEFunction(nid : 0x19884A15, version : 150)]
		public virtual int sceMd5BlockInit(TPointer contextAddr)
		{
			md5.reset();

			// size of context seems to be 32 + 64 bytes
			contextAddr.setValue32(0, 0x67452301);
			contextAddr.setValue32(4, 0xEFCDAB89);
			contextAddr.setValue32(8, 0x98BADCFE);
			contextAddr.setValue32(12, 0x10325476);
			contextAddr.setValue16(20, (short) 0);
			contextAddr.setValue16(22, (short) 0);
			contextAddr.setValue32(24, 0);
			contextAddr.setValue32(28, 0);
			// followed by 64 bytes, not being initialized here (probably the data block being processed).

			return 0;
		}

		[HLEFunction(nid : 0xA30206C2, version : 150)]
		public virtual int sceMd5BlockUpdate(TPointer contextAddr, TPointer sourceAddr, int size)
		{
			sbyte[] source = getMemoryBytes(sourceAddr.Address, size);
			md5.update(source);

			return 0;
		}

		[HLEFunction(nid : 0x4876AFFF, version : 150)]
		public virtual int sceMd5BlockResult(TPointer contextAddr, TPointer resultAddr)
		{
			sbyte[] result = md5.digest();
			writeMd5Digest(resultAddr.Address, result);

			return 0;
		}

		[HLEFunction(nid : 0x98E31A9E, version : 150)]
		public virtual int sceMd5Digest(TPointer sourceAddr, int size, TPointer resultAddr)
		{
			sbyte[] source = getMemoryBytes(sourceAddr.Address, size);
			md5.reset();
			sbyte[] result = md5.digest(source);
			writeMd5Digest(resultAddr.Address, result);

			return 0;
		}
	}

}