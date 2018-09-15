using System;

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
//	import static pspsharp.Allegrex.Common._a2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._a3;

	//using Logger = org.apache.log4j.Logger;

	using CpuState = pspsharp.Allegrex.CpuState;
	using AbstractNativeCodeSequence = pspsharp.Allegrex.compiler.nativeCode.AbstractNativeCodeSequence;
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using Utilities = pspsharp.util.Utilities;

	public class SysclibForKernel : HLEModule
	{
		//public static Logger log = Modules.getLogger("SysclibForKernel");
		private const string validNumberCharactersUpperCase = "0123456789ABCDEF";
		private const string validNumberCharactersLowerCase = "0123456789abcdef";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x10F3BB61, version = 150) public int memset(@CanBeNull pspsharp.HLE.TPointer destAddr, int data, int size)
		[HLEFunction(nid : 0x10F3BB61, version : 150)]
		public virtual int memset(TPointer destAddr, int data, int size)
		{
			if (destAddr.NotNull)
			{
				destAddr.memset((sbyte) data, size);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xEC6F1CF2, version = 150) public int strcpy(@CanBeNull pspsharp.HLE.TPointer destAddr, @CanBeNull pspsharp.HLE.TPointer srcAddr)
		[HLEFunction(nid : 0xEC6F1CF2, version : 150)]
		public virtual int strcpy(TPointer destAddr, TPointer srcAddr)
		{
			if (destAddr.NotNull && srcAddr.NotNull)
			{
				AbstractNativeCodeSequence.strcpy(destAddr.Address, srcAddr.Address);
			}

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC0AB8932, version = 150) public int strcmp(@CanBeNull pspsharp.HLE.TPointer src1Addr, @CanBeNull pspsharp.HLE.TPointer src2Addr)
		[HLEFunction(nid : 0xC0AB8932, version : 150)]
		public virtual int strcmp(TPointer src1Addr, TPointer src2Addr)
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("strcmp '{0}', '{1}'", Utilities.readStringZ(src1Addr.Address), Utilities.readStringZ(src2Addr.Address)));
			}
			return AbstractNativeCodeSequence.strcmp(src1Addr.Address, src2Addr.Address);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x52DF196C, version = 150) public int strlen(@CanBeNull pspsharp.HLE.TPointer srcAddr)
		[HLEFunction(nid : 0x52DF196C, version : 150)]
		public virtual int strlen(TPointer srcAddr)
		{
			return AbstractNativeCodeSequence.getStrlen(srcAddr.Address);
		}

		[HLEFunction(nid : 0x81D0D1F7, version : 150)]
		public virtual int memcmp(TPointer src1Addr, TPointer src2Addr, int size)
		{
			int result = 0;

			if (size > 0)
			{
				IMemoryReader memoryReader1 = MemoryReader.getMemoryReader(src1Addr.Address, size, 1);
				IMemoryReader memoryReader2 = MemoryReader.getMemoryReader(src2Addr.Address, size, 1);
				for (int i = 0; i < size; i++)
				{
					int c1 = memoryReader1.readNext();
					int c2 = memoryReader2.readNext();
					if (c1 != c2)
					{
						result = c1 < c2 ? -1 : 1;
						break;
					}
				}
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xAB7592FF, version = 150) public int memcpy(@CanBeNull pspsharp.HLE.TPointer destAddr, pspsharp.HLE.TPointer srcAddr, int size)
		[HLEFunction(nid : 0xAB7592FF, version : 150)]
		public virtual int memcpy(TPointer destAddr, TPointer srcAddr, int size)
		{
			if (destAddr.NotNull && destAddr.Address != srcAddr.Address)
			{
				destAddr.Memory.memcpyWithVideoCheck(destAddr.Address, srcAddr.Address, size);
			}

			return destAddr.Address;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x7AB35214, version = 150) public int strncmp(@CanBeNull pspsharp.HLE.TPointer src1Addr, pspsharp.HLE.TPointer src2Addr, int size)
		[HLEFunction(nid : 0x7AB35214, version : 150)]
		public virtual int strncmp(TPointer src1Addr, TPointer src2Addr, int size)
		{
			int result = 0;
			if (size > 0)
			{
				IMemoryReader memoryReader1 = MemoryReader.getMemoryReader(src1Addr.Address, size, 1);
				IMemoryReader memoryReader2 = MemoryReader.getMemoryReader(src2Addr.Address, size, 1);

				if (memoryReader1 != null && memoryReader2 != null)
				{
					for (int i = 0; i < size; i++)
					{
						int c1 = memoryReader1.readNext();
						int c2 = memoryReader2.readNext();
						if (c1 != c2)
						{
							result = c1 - c2;
							break;
						}
						else if (c1 == 0)
						{
							// c1 == 0 and c2 == 0
							break;
						}
					}
				}
			}
			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xA48D2592, version = 150) public int memmove(@CanBeNull pspsharp.HLE.TPointer destAddr, pspsharp.HLE.TPointer srcAddr, int size)
		[HLEFunction(nid : 0xA48D2592, version : 150)]
		public virtual int memmove(TPointer destAddr, TPointer srcAddr, int size)
		{
			if (destAddr.NotNull && destAddr.Address != srcAddr.Address)
			{
				destAddr.Memory.memmove(destAddr.Address, srcAddr.Address, size);
			}
			return 0;
		}

		[HLEFunction(nid : 0x7661E728, version : 150)]
		public virtual int sprintf(CpuState cpu, TPointer buffer, string format)
		{
			string formattedString = Modules.SysMemUserForUserModule.hleKernelSprintf(cpu, format, _a2);
			Utilities.writeStringZ(buffer.Memory, buffer.Address, formattedString);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sprintf returning '{0}'", formattedString));
			}

			return formattedString.Length;
		}

		/// <summary>
		/// Returns a pointer to the first occurrence of s2 in s1, or a null pointer if s2 is not part of s1.
		/// The matching process does not include the terminating null-characters, but it stops there. </summary>
		/// <param name="s1"> string to be scanned </param>
		/// <param name="s2"> string containing the sequence of characters to match </param>
		/// <returns> a pointer to the first occurrence in s1 or the entire sequence of characters specified in s2,
		///         or a null pointer if the sequence is not present in s1. </returns>
		[HLEFunction(nid : 0x0D188658, version : 150)]
		public virtual int strstr(PspString s1, PspString s2)
		{
			int index = s1.String.IndexOf(s2.String, StringComparison.Ordinal);
			if (index < 0)
			{
				return 0;
			}
			return s1.Address + index;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x476FD94A, version = 150) public int strcat(@CanBeNull pspsharp.HLE.TPointer destAddr, @CanBeNull pspsharp.HLE.TPointer srcAddr)
		[HLEFunction(nid : 0x476FD94A, version : 150)]
		public virtual int strcat(TPointer destAddr, TPointer srcAddr)
		{
			if (destAddr.Null || srcAddr.Null)
			{
				return 0;
			}

			int dstLength = AbstractNativeCodeSequence.getStrlen(destAddr.Address);
			int srcLength = AbstractNativeCodeSequence.getStrlen(srcAddr.Address);
			destAddr.memcpy(dstLength, srcAddr.Address, srcLength + 1);

			return destAddr.Address;
		}

		[HLEFunction(nid : 0xC2145E80, version : 150)]
		public virtual int snprintf(CpuState cpu, TPointer buffer, int n, string format)
		{
			string formattedString = Modules.SysMemUserForUserModule.hleKernelSprintf(cpu, format, _a3);
			if (formattedString.Length >= n)
			{
				formattedString = formattedString.Substring(0, n - 1);
			}

			Utilities.writeStringZ(buffer.Memory, buffer.Address, formattedString);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("snprintf returning '{0}'", formattedString));
			}

			return formattedString.Length;
		}

		private bool isNumberValidCharacter(int c, int @base)
		{
			if (@base > validNumberCharactersUpperCase.Length)
			{
				@base = validNumberCharactersUpperCase.Length;
			}

			if (validNumberCharactersUpperCase.Substring(0, @base).IndexOf(c) >= 0)
			{
				return true;
			}

			if (validNumberCharactersLowerCase.Substring(0, @base).IndexOf(c) >= 0)
			{
				return true;
			}

			return false;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x47DD934D, version = 150) public int strtol(@CanBeNull pspsharp.HLE.PspString string, @CanBeNull pspsharp.HLE.TPointer32 endString, int super)
		[HLEFunction(nid : 0x47DD934D, version : 150)]
		public virtual int strtol(PspString @string, TPointer32 endString, int @base)
		{
			// base == 0 seems to be handled as base == 10.
			if (@base == 0)
			{
				@base = 10;
			}

			IMemoryReader memoryReader = MemoryReader.getMemoryReader(@string.Address, 1);
			string s = @string.String;

			// Skip any leading "0x" in case of base 16
			if (@base == 16 && (s.StartsWith("0x", StringComparison.Ordinal) || s.StartsWith("0X", StringComparison.Ordinal)))
			{
				memoryReader.skip(2);
				s = s.Substring(2);
			}

			for (int i = 0; true; i++)
			{
				int c = memoryReader.readNext();
				if (c == 0 || !isNumberValidCharacter(c, @base))
				{
					endString.setValue(@string.Address + i);
					s = s.Substring(0, i);
					break;
				}
			}

			int result;
			if (s.Length == 0)
			{
				result = 0;
			}
			else
			{
				result = Integer.parseInt(s, @base);
			}

			//if (log.DebugEnabled)
			{
				if (@base == 10)
				{
					Console.WriteLine(string.Format("strtol on '{0}' returning {1:D}", s, result));
				}
				else
				{
					Console.WriteLine(string.Format("strtol on '{0}' returning 0x{1:X}", s, result));
				}
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x6A7900E1, version = 150) public int strtoul(@CanBeNull pspsharp.HLE.PspString string, @CanBeNull pspsharp.HLE.TPointer32 endString, int super)
		[HLEFunction(nid : 0x6A7900E1, version : 150)]
		public virtual int strtoul(PspString @string, TPointer32 endString, int @base)
		{
			// Assume same as strtol
			return strtol(@string, endString, @base);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xB49A7697, version = 150) public int strncpy(@CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextNextParameter, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer destAddr, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer srcAddr, int size)
		[HLEFunction(nid : 0xB49A7697, version : 150)]
		public virtual int strncpy(TPointer destAddr, TPointer srcAddr, int size)
		{
			int srcLength = AbstractNativeCodeSequence.getStrlen(srcAddr.Address);
			if (srcLength < size)
			{
				destAddr.memcpy(srcAddr.Address, srcLength + 1);
				destAddr.clear(srcLength + 1, size - srcLength - 1);
			}
			else
			{
				destAddr.memcpy(srcAddr.Address, size);
			}

			return destAddr.Address;
		}

		[HLEFunction(nid : 0x7DEE14DE, version : 150)]
		public virtual long __udivdi3(long a, long b)
		{
			return a / b;
		}

		[HLEFunction(nid : 0x5E8E5F42, version : 150)]
		public virtual long __umoddi3(long a, long b)
		{
			return a % b;
		}

		[HLEFunction(nid : 0xB1DC2AE8, version : 150)]
		public virtual int strchr(PspString @string, int c)
		{
			int index = @string.String.IndexOf(c);
			if (index < 0)
			{
				return 0;
			}

			return @string.Address + index;
		}

		[HLEFunction(nid : 0x32C767F2, version : 150)]
		public virtual int look_ctype_table(int c)
		{
			return Modules.sceNetModule.sceNetLook_ctype_table(c);
		}

		[HLEFunction(nid : 0x3EC5BBF6, version : 150)]
		public virtual int tolower(int c)
		{
			return Modules.sceNetModule.sceNetTolower(c);
		}

		[HLEFunction(nid : 0xCE2F7487, version : 150)]
		public virtual int toupper(int c)
		{
			return Modules.sceNetModule.sceNetToupper(c);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x87C78FB6, version = 150) public int prnt()
		[HLEFunction(nid : 0x87C78FB6, version : 150)]
		public virtual int prnt()
		{
			return 0;
		}

		[HLEFunction(nid : 0x4C0E0274, version : 150)]
		public virtual int strrchr(PspString @string, int c)
		{
			int index = @string.String.LastIndexOf(c);
			if (index < 0)
			{
				return 0;
			}

			return @string.Address + index;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x86FEFCE9, version = 150) public void bzero(@CanBeNull pspsharp.HLE.TPointer destAddr, int size)
		[HLEFunction(nid : 0x86FEFCE9, version : 150)]
		public virtual void bzero(TPointer destAddr, int size)
		{
			memset(destAddr, 0, size);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x90C5573D, version = 150) public int strnlen(@CanBeNull pspsharp.HLE.TPointer srcAddr, int size)
		[HLEFunction(nid : 0x90C5573D, version : 150)]
		public virtual int strnlen(TPointer srcAddr, int size)
		{
			if (srcAddr.Null || size == 0)
			{
				return 0;
			}

			IMemoryReader memoryReader = MemoryReader.getMemoryReader(srcAddr.Address, size, 1);
			for (int i = 0; i < size; i++)
			{
				int c = memoryReader.readNext();
				if (c == 0)
				{
					return i;
				}
			}

			return size;
		}
	}

}