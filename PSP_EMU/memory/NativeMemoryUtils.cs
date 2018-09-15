using System.Runtime.InteropServices;

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
namespace pspsharp.memory
{

	/// <summary>
	/// Collection of native functions to handle memory allocated natively.
	/// This is similar to direct buffers (NIO), but more PSP-like.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class NativeMemoryUtils
	{
		/// <summary>
		/// Initialization method.
		/// Has to be called at least once before any other native method can be used.
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void init();

		/// <summary>
		/// Allocate native memory.
		/// </summary>
		/// <param name="size"> size in bytes of the memory to be allocated. </param>
		/// <returns>     the base address of the allocated native memory. </returns>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern long alloc(int size);

		/// <summary>
		/// Free native memory.
		/// </summary>
		/// <param name="memory"> the base address of the native memory. </param>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void free(long memory);

		/// <summary>
		/// Read one byte (8 bits) from a native memory.
		/// </summary>
		/// <param name="memory">   the base address of the native memory (as returned by alloc). </param>
		/// <param name="address">  the offset inside the native memory. </param>
		/// <returns>         the unsigned 8-bits value at the given address. </returns>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int read8(long memory, int address);

		/// <summary>
		/// Write one byte (8 bits) into a native memory.
		/// </summary>
		/// <param name="memory">   the base address of the native memory (as returned by alloc). </param>
		/// <param name="address">  the offset inside the native memory. </param>
		/// <param name="value">    the unsigned 8-bits value to be written. </param>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void write8(long memory, int address, int value);

		/// <summary>
		/// Read one short (16 bits) from a native memory.
		/// </summary>
		/// <param name="memory">   the base address of the native memory (as returned by alloc). </param>
		/// <param name="address">  the offset inside the native memory. </param>
		/// <returns>         the unsigned 16-bits value at the given address. </returns>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int read16(long memory, int address);

		/// <summary>
		/// Write one short (16 bits) into a native memory.
		/// </summary>
		/// <param name="memory">   the base address of the native memory (as returned by alloc). </param>
		/// <param name="address">  the offset inside the native memory. </param>
		/// <param name="value">    the unsigned 16-bits value to be written. </param>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void write16(long memory, int address, int value);

		/// <summary>
		/// Read one int (32 bits) from a native memory.
		/// </summary>
		/// <param name="memory">   the base address of the native memory (as returned by alloc). </param>
		/// <param name="address">  the offset inside the native memory. </param>
		/// <returns>         the signed 32-bits value at the given address. </returns>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int read32(long memory, int address);

		/// <summary>
		/// Write one int (32 bits) into a native memory.
		/// </summary>
		/// <param name="memory">   the base address of the native memory (as returned by alloc). </param>
		/// <param name="address">  the offset inside the native memory. </param>
		/// <param name="value">    the signed 32-bits value to be written. </param>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void write32(long memory, int address, int value);

		/// <summary>
		/// Fill an area into a native memory with one byte (8 bits) value.
		/// </summary>
		/// <param name="memory">   the base address of the native memory (as returned by alloc). </param>
		/// <param name="address">  the offset inside the native memory. </param>
		/// <param name="value">    the unsigned 8-bits value to be written. </param>
		/// <param name="Length">   the number of bytes to be written. </param>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void memset(long memory, int address, int value, int Length);

		/// <summary>
		/// Copy an area from a native memory to another area of the native memory.
		/// </summary>
		/// <param name="memoryDestination"> the destination base address of the native memory (as returned by alloc). </param>
		/// <param name="destination">       the offset inside the native memory for the destination area. </param>
		/// <param name="memorySource">      the source base address of the native memory (as returned by alloc). </param>
		/// <param name="source">            the offset inside the native memory for the source area. </param>
		/// <param name="Length">            the number of bytes to be copied. </param>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void memcpy(long memoryDestination, int destination, long memorySource, int source, int Length);

		/// <summary>
		/// Return the Length of a null-terminated string stored in a native memory
		/// (using the standard "strlen" function).
		/// The string has to be terminated by a byte having the value 0.
		/// </summary>
		/// <param name="memory">   the base address of the native memory (as returned by alloc). </param>
		/// <param name="address">  the offset inside the native memory. </param>
		/// <returns>         the Length of the string. </returns>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int strlen(long memory, int address);

		/// <summary>
		/// Create a Direct Buffer representing an area into a native memory.
		/// </summary>
		/// <param name="memory">   the base address of the native memory (as returned by alloc). </param>
		/// <param name="address">  the offset inside the native memory. </param>
		/// <param name="Length">   the number of bytes of the area. </param>
		/// <returns>         a new Direct Buffer representing the area of the native memory.
		///                 The Direct Buffer has always the default ByteOrder BIG_ENDIAN. </returns>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern ByteBuffer getBuffer(long memory, int address, int Length);

		/// <summary>
		/// Check if the current emulator host is little or big endian.
		/// Remark: the PSP is little endian.
		/// </summary>
		/// <returns>  true if the current host is little endian.
		///          false if the current host is big endian. </returns>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern boolean isLittleEndian();

		/// <summary>
		/// Copy bytes from a Direct Buffer to a native memory.
		/// </summary>
		/// <param name="memory">   the base address of the native memory (as returned by alloc). </param>
		/// <param name="address">  the offset inside the native memory. </param>
		/// <param name="buffer">   the Direct Buffer to be used as a source.
		///                 The buffer position, capacity and limit are ignored. </param>
		/// <param name="bufferOffset"> the offset in bytes from the Direct Buffer. </param>
		/// <param name="Length">   the number of bytes to be copied. </param>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void copyBufferToMemory(long memory, int address, java.nio.Buffer buffer, int bufferOffset, int Length);

		/// <summary>
		/// Copy bytes from a native memory to a Direct Buffer.
		/// </summary>
		/// <param name="buffer">   the Direct Buffer to be used as a destination.
		///                 The buffer position, capacity and limit are ignored. </param>
		/// <param name="bufferOffset"> the offset in bytes from the Direct Buffer. </param>
		/// <param name="memory">   the base address of the native memory (as returned by alloc). </param>
		/// <param name="address">  the offset inside the native memory. </param>
		/// <param name="Length">   the number of bytes to be copied. </param>
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void copyMemoryToBuffer(java.nio.Buffer buffer, int bufferOffset, long memory, int address, int Length);
	}

}