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
namespace pspsharp.graphics.RE.externalge
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Memory.isAddressGood;


	using Logger = org.apache.log4j.Logger;

	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using DurationStatistics = pspsharp.util.DurationStatistics;
	using Hash = pspsharp.util.Hash;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class NativeCallbacks
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private static readonly Logger log_Renamed = Logger.getLogger("NativeCallbacks");
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private static DurationStatistics read32_Renamed = new DurationStatistics("read32");
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private static DurationStatistics readByteBuffer_Renamed = new DurationStatistics("readByteBuffer");
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private static DurationStatistics writeByteBuffer_Renamed = new DurationStatistics("writeByteBuffer");
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private static DurationStatistics writeByteBufferArea_Renamed = new DurationStatistics("writeByteBufferArea");
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private static DurationStatistics getHashCode_Renamed = new DurationStatistics("getHashCode");

		// Array indexed by the log category
		private static readonly Logger[] logs = new Logger[] {log_Renamed, Logger.getLogger("externalge")};

		public static void exit()
		{
			if (DurationStatistics.collectStatistics)
			{
				log_Renamed.info(read32_Renamed.ToString());
				log_Renamed.info(readByteBuffer_Renamed.ToString());
				log_Renamed.info(writeByteBuffer_Renamed.ToString());
				log_Renamed.info(getHashCode_Renamed.ToString());
			}
		}

		private static Memory Memory
		{
			get
			{
				return Memory.Instance;
			}
		}

		public static int read32(int address)
		{
			if (DurationStatistics.collectStatistics)
			{
				read32_Renamed.start();
				int value = Memory.read32(address);
				read32_Renamed.end();
				return value;
			}
			return Memory.read32(address);
		}

		public static int read16(int address)
		{
			return Memory.read16(address);
		}

		public static int read8(int address)
		{
			return Memory.read8(address);
		}

		public static int readByteBuffer(int address, ByteBuffer destination, int length)
		{
			readByteBuffer_Renamed.start();
			Buffer source = Memory.getBuffer(address, length);
			int offset = 0;
			if (source != null)
			{
				if (source is IntBuffer)
				{
					offset = address & 3;
				}
				Utilities.putBuffer(destination, source, ByteOrder.LITTLE_ENDIAN, length + offset);
			}
			readByteBuffer_Renamed.end();
			return offset;
		}

		public static void write32(int address, int value)
		{
			Memory.write32(address, value);
		}

		public static void write16(int address, short value)
		{
			Memory.write16(address, value);
		}

		public static void write8(int address, sbyte value)
		{
			Memory.write8(address, value);
		}

		public static void copy(int destination, int source, int length)
		{
			Memory.memcpy(destination, source, length);
		}

		public static void writeByteBuffer(int address, ByteBuffer source, int length)
		{
			writeByteBuffer_Renamed.start();
			if (RuntimeContext.hasMemoryInt() && (address & 3) == 0 && (length & 3) == 0 && isAddressGood(address))
			{
				IntBuffer destination = IntBuffer.wrap(RuntimeContext.MemoryInt, (address & Memory.addressMask) >> 2, length >> 2);
				source.order(ByteOrder.nativeOrder());
				destination.put(source.asIntBuffer());
			}
			else
			{
				Memory.copyToMemory(address, source, length);
			}
			writeByteBuffer_Renamed.end();
		}

		public static void writeByteBufferArea(int address, ByteBuffer source, int bufferWidth, int width, int height)
		{
			writeByteBufferArea_Renamed.start();
			if (RuntimeContext.hasMemoryInt() && (address & 3) == 0 && (width & 3) == 0 && (bufferWidth & 3) == 0 && isAddressGood(address))
			{
				int length = bufferWidth * height;
				int destinationOffset = (address & Memory.addressMask) >> 2;
				IntBuffer destination = IntBuffer.wrap(RuntimeContext.MemoryInt, destinationOffset, length >> 2);
				source.order(ByteOrder.nativeOrder());
				IntBuffer sourceInt = source.asIntBuffer();
				int width4 = width >> 2;
				int bufferWidth4 = bufferWidth >> 2;
				for (int y = 0; y < height; y++)
				{
					int offset = y * bufferWidth4;
					sourceInt.limit(offset + width4);
					sourceInt.position(offset);
					destination.position(destinationOffset + offset);
					destination.put(sourceInt);
				}
			}
			else
			{
				Memory mem = Memory;
				for (int y = 0; y < height; y++)
				{
					int offset = y * bufferWidth;
					source.position(offset);
					mem.copyToMemory(address + offset, source, width);
				}
			}
			writeByteBufferArea_Renamed.end();
		}

		public static int getHashCode(int hashCode, int addr, int lengthInBytes, int strideInBytes)
		{
			getHashCode_Renamed.start();
			int value = Hash.getHashCode(hashCode, addr, lengthInBytes, strideInBytes);
			getHashCode_Renamed.end();
			return value;
		}

		public static void log(int category, int level, string message)
		{
			Logger log;
			if (category >= 0 && category < logs.Length)
			{
				log = logs[category];
			}
			else
			{
				log = NativeCallbacks.log_Renamed;
			}

			// Values matching pspsharp::log::Level defined in pspsharp.log.h
			switch (level)
			{
				case 0: // E_OFF
					break;
				case 1: // E_FATAL
					log.fatal(message);
					break;
				case 2: // E_ERROR
					log.error(message);
					break;
				case 3: // E_WARN
					log.warn(message);
					break;
				case 4: // E_INFO
					log.info(message);
					break;
				case 5: // E_DEBUG
					log.debug(message);
					break;
				case 6: // E_TRACE
					log.trace(message);
					break;
				case 7: // E_FORCE
					log.info(message);
					break;
				default:
					log.error(string.Format("Unknown log level {0:D}: {1}", level, message));
					break;
			}
		}
	}

}