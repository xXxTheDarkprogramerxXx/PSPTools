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
	using LengthInfo = pspsharp.HLE.BufferInfo.LengthInfo;
	using Usage = pspsharp.HLE.BufferInfo.Usage;


	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using SystemTimeManager = pspsharp.HLE.kernel.managers.SystemTimeManager;
	using Utilities = pspsharp.util.Utilities;

	//using Logger = org.apache.log4j.Logger;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLELogging public class UtilsForUser extends pspsharp.HLE.HLEModule
	public class UtilsForUser : HLEModule
	{
		//public static Logger log = Modules.getLogger("UtilsForUser");

		private Dictionary<int, SceKernelUtilsMt19937Context> Mt19937List;
		private SceKernelUtilsMd5Context md5Ctx;
		private SceKernelUtilsSha1Context sha1Ctx;

		private class SceKernelUtilsMt19937Context
		{
			internal System.Random r;

			public SceKernelUtilsMt19937Context(TPointer ctxAddr, int seed)
			{
				r = new System.Random(seed);

				// Overwrite the context memory (628 bytes)
				ctxAddr.memset(unchecked((sbyte) 0xCD), 628);
			}

			public virtual int getInt(TPointer ctxAddr)
			{
				return r.Next();
			}
		}

		private class SceKernelUtilsContext
		{
			internal readonly string algorithm;
			internal readonly int hashLength;
			// Context vars.
			internal int part1;
			internal int part2;
			internal int part3;
			internal int part4;
			internal int part5;
			internal short tmpBytesRemaining;
			internal short tmpBytesCalculated;
			internal long fullDataSize;
			internal sbyte[] buf;

			// Internal vars.
			internal sbyte[] input;

			protected internal SceKernelUtilsContext(string algorithm, int hashLength)
			{
				this.algorithm = algorithm;
				this.hashLength = hashLength;
				part1 = 0;
				part2 = 0;
				part3 = 0;
				part4 = 0;
				part5 = 0;
				tmpBytesRemaining = 0;
				tmpBytesCalculated = 0;
				fullDataSize = 0;
				buf = new sbyte[64];
			}

			public virtual int init(TPointer ctxAddr)
			{
				ctxAddr.setValue32(0, part1);
				ctxAddr.setValue32(4, part2);
				ctxAddr.setValue32(8, part3);
				ctxAddr.setValue32(12, part4);
				ctxAddr.setValue32(16, part5);
				ctxAddr.setValue16(20, tmpBytesRemaining);
				ctxAddr.setValue16(22, tmpBytesCalculated);
				ctxAddr.setValue64(24, fullDataSize);
				ctxAddr.setArray(32, buf, 64);

				return 0;
			}

			public virtual int update(TPointer ctxAddr, TPointer dataAddr, int dataSize)
			{
				input = dataAddr.getArray8(dataSize);

				return 0;
			}

			public virtual int result(TPointer ctxAddr, TPointer resultAddr)
			{
				sbyte[] hash = null;
				if (input != null)
				{
					try
					{
						MessageDigest md = MessageDigest.getInstance(algorithm);
						hash = md.digest(input);
					}
					catch (Exception e)
					{
						// Ignore...
						Console.WriteLine(string.Format("SceKernelUtilsContext({0}).result", algorithm), e);
					}
				}

				if (hash != null)
				{
					resultAddr.setArray(hash, hashLength);
				}

				return 0;
			}

			protected internal static int digest(TPointer inAddr, int inSize, TPointer outAddr, string algorithm, int hashLength)
			{
				sbyte[] input = inAddr.getArray8(inSize);
				sbyte[] hash = null;
				try
				{
					MessageDigest md = MessageDigest.getInstance(algorithm);
					hash = md.digest(input);
				}
				catch (Exception e)
				{
					// Ignore...
					Console.WriteLine(string.Format("SceKernelUtilsContext({0}).digest", algorithm), e);
				}
				if (hash != null)
				{
					outAddr.setArray(hash, hashLength);
				}

				return 0;
			}
		}

		private class SceKernelUtilsMd5Context : SceKernelUtilsContext
		{
			internal const string algorithm = "MD5";

			public SceKernelUtilsMd5Context() : base(algorithm, 16)
			{
			}

			public static int digest(TPointer inAddr, int inSize, TPointer outAddr)
			{
				return digest(inAddr, inSize, outAddr, algorithm, 16);
			}
		}

		private class SceKernelUtilsSha1Context : SceKernelUtilsContext
		{
			internal const string algorithm = "SHA-1";

			public SceKernelUtilsSha1Context() : base(algorithm, 20)
			{
			}

			public static int digest(TPointer inAddr, int inSize, TPointer outAddr)
			{
				return digest(inAddr, inSize, outAddr, algorithm, 20);
			}
		}

		public override void start()
		{
			Mt19937List = new Dictionary<int, SceKernelUtilsMt19937Context>();

			base.start();
		}

		protected internal const int PSP_KERNEL_ICACHE_PROBE_MISS = 0;
		protected internal const int PSP_KERNEL_ICACHE_PROBE_HIT = 1;
		protected internal const int PSP_KERNEL_DCACHE_PROBE_MISS = 0;
		protected internal const int PSP_KERNEL_DCACHE_PROBE_HIT = 1;
		protected internal const int PSP_KERNEL_DCACHE_PROBE_HIT_DIRTY = 2;

		[HLELogging(level:"trace"), HLEFunction(nid : 0xBFA98062, version : 150)]
		public virtual int sceKernelDcacheInvalidateRange(TPointer addr, int size)
		{
			return 0;
		}

		[HLEFunction(nid : 0xC2DF770E, version : 150)]
		public virtual int sceKernelIcacheInvalidateRange(TPointer addr, int size)
		{
			if (log.InfoEnabled)
			{
				log.info(string.Format("sceKernelIcacheInvalidateRange addr={0}, size={1:D}", addr, size));
			}

			RuntimeContext.invalidateRange(addr.Address, size);

			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC8186A58, version = 150) public int sceKernelUtilsMd5Digest(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer inAddr, int inSize, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=16, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer outAddr)
		[HLEFunction(nid : 0xC8186A58, version : 150)]
		public virtual int sceKernelUtilsMd5Digest(TPointer inAddr, int inSize, TPointer outAddr)
		{
			int result = SceKernelUtilsMd5Context.digest(inAddr, inSize, outAddr);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceKernelUtilsMd5Digest input:{0}, output:{1}", Utilities.getMemoryDump(inAddr.Address, inSize), Utilities.getMemoryDump(outAddr.Address, 16)));
			}
			return result;
		}

		[HLEFunction(nid : 0x9E5C5086, version : 150)]
		public virtual int sceKernelUtilsMd5BlockInit(TPointer md5CtxAddr)
		{
			md5Ctx = new SceKernelUtilsMd5Context();
			return md5Ctx.init(md5CtxAddr);
		}

		[HLEFunction(nid : 0x61E1E525, version : 150)]
		public virtual int sceKernelUtilsMd5BlockUpdate(TPointer md5CtxAddr, TPointer inAddr, int inSize)
		{
			return md5Ctx.update(md5CtxAddr, inAddr, inSize);
		}

		[HLEFunction(nid : 0xB8D24E78, version : 150)]
		public virtual int sceKernelUtilsMd5BlockResult(TPointer md5CtxAddr, TPointer outAddr)
		{
			return md5Ctx.result(md5CtxAddr, outAddr);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x840259F1, version = 150) public int sceKernelUtilsSha1Digest(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.nextParameter, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer inAddr, int inSize, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=20, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer outAddr)
		[HLEFunction(nid : 0x840259F1, version : 150)]
		public virtual int sceKernelUtilsSha1Digest(TPointer inAddr, int inSize, TPointer outAddr)
		{
			return SceKernelUtilsSha1Context.digest(inAddr, inSize, outAddr);
		}

		[HLEFunction(nid : 0xF8FCD5BA, version : 150)]
		public virtual int sceKernelUtilsSha1BlockInit(TPointer sha1CtxAddr)
		{
			sha1Ctx = new SceKernelUtilsSha1Context();
			return sha1Ctx.init(sha1CtxAddr);
		}

		[HLEFunction(nid : 0x346F6DA8, version : 150)]
		public virtual int sceKernelUtilsSha1BlockUpdate(TPointer sha1CtxAddr, TPointer inAddr, int inSize)
		{
			return sha1Ctx.update(sha1CtxAddr, inAddr, inSize);
		}

		[HLEFunction(nid : 0x585F1C09, version : 150)]
		public virtual int sceKernelUtilsSha1BlockResult(TPointer sha1CtxAddr, TPointer outAddr)
		{
			return sha1Ctx.result(sha1CtxAddr, outAddr);
		}

		[HLEFunction(nid : 0xE860E75E, version : 150)]
		public virtual int sceKernelUtilsMt19937Init(TPointer ctxAddr, int seed)
		{
			// We'll use the address of the ctx as a key
			Mt19937List.Remove(ctxAddr.Address); // Remove records of any already existing context at a0
			Mt19937List[ctxAddr.Address] = new SceKernelUtilsMt19937Context(ctxAddr, seed);

			return 0;
		}

		[HLEFunction(nid : 0x06FB8A63, version : 150)]
		public virtual int sceKernelUtilsMt19937UInt(TPointer ctxAddr)
		{
			SceKernelUtilsMt19937Context ctx = Mt19937List[ctxAddr.Address];
			if (ctx == null)
			{
				Console.WriteLine(string.Format("sceKernelUtilsMt19937UInt uninitialised context {0}", ctxAddr));
				return 0;
			}

			return ctx.getInt(ctxAddr);
		}

		[HLEFunction(nid : 0x37FB5C42, version : 150)]
		public virtual int sceKernelGetGPI()
		{
			int gpi;

			if (State.debugger != null)
			{
				gpi = State.debugger.GetGPI();
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceKernelGetGPI returning 0x{0:X2}", gpi));
				}
			}
			else
			{
				gpi = 0;
				//if (log.DebugEnabled)
				{
					Console.WriteLine("sceKernelGetGPI debugger not enabled");
				}
			}

			return gpi;
		}

		[HLEFunction(nid : 0x6AD345D7, version : 150)]
		public virtual int sceKernelSetGPO(int value)
		{
			if (State.debugger != null)
			{
				State.debugger.SetGPO(value);
			}
			else
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine("sceKernelSetGPO debugger not enabled");
				}
			}

			return 0;
		}

		[HLEFunction(nid : 0x91E4F6A7, version : 150)]
		public virtual int sceKernelLibcClock()
		{
			return (int) SystemTimeManager.SystemTime;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x27CC57F0, version = 150) public int sceKernelLibcTime(@CanBeNull pspsharp.HLE.TPointer32 time_t_addr)
		[HLEFunction(nid : 0x27CC57F0, version : 150)]
		public virtual int sceKernelLibcTime(TPointer32 time_t_addr)
		{
			int seconds = (int)(new DateTime().Ticks / 1000);
			time_t_addr.setValue(seconds);

			return seconds;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x71EC4271, version = 150) public int sceKernelLibcGettimeofday(@CanBeNull pspsharp.HLE.TPointer32 tp, @CanBeNull pspsharp.HLE.TPointer32 tzp)
		[HLEFunction(nid : 0x71EC4271, version : 150)]
		public virtual int sceKernelLibcGettimeofday(TPointer32 tp, TPointer32 tzp)
		{
			Clock.TimeNanos currentTimeNano = Emulator.Clock.currentTimeNanos();
			int tv_sec = currentTimeNano.seconds;
			int tv_usec = currentTimeNano.millis * 1000 + currentTimeNano.micros;
			tp.setValue(0, tv_sec);
			tp.setValue(4, tv_usec);

			// PSP always returning 0 for these 2 values:
			int tz_minuteswest = 0;
			int tz_dsttime = 0;
			tzp.setValue(0, tz_minuteswest);
			tzp.setValue(4, tz_dsttime);

			return 0;
		}

		[HLELogging(level:"trace"), HLEFunction(nid : 0x79D1C3FA, version : 150)]
		public virtual void sceKernelDcacheWritebackAll()
		{
		}

		[HLELogging(level:"trace"), HLEFunction(nid : 0xB435DEC5, version : 150)]
		public virtual void sceKernelDcacheWritebackInvalidateAll()
		{
		}

		[HLELogging(level:"trace"), HLEFunction(nid : 0x3EE30821, version : 150)]
		public virtual int sceKernelDcacheWritebackRange(TPointer addr, int size)
		{
			return 0;
		}

		[HLELogging(level:"trace"), HLEFunction(nid : 0x34B9FA9E, version : 150)]
		public virtual void sceKernelDcacheWritebackInvalidateRange(TPointer addr, int size)
		{
		}

		[HLELogging(level:"trace"), HLEFunction(nid : 0x80001C4C, version : 150)]
		public virtual int sceKernelDcacheProbe(TPointer addr)
		{
			return PSP_KERNEL_DCACHE_PROBE_HIT; // Dummy
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x16641D70, version = 150) public int sceKernelDcacheReadTag()
		[HLEFunction(nid : 0x16641D70, version : 150)]
		public virtual int sceKernelDcacheReadTag()
		{
			return 0;
		}

		[HLEFunction(nid : 0x920F104A, version : 150)]
		public virtual void sceKernelIcacheInvalidateAll()
		{
			// Some games attempt to change compiled code at runtime
			// by calling this function.
			// Use the RuntimeContext to regenerate a compiling context
			// and restart from there.
			// This method only works for compiled code being called by
			//    JR   $rs
			// or
			//    JALR $rs, $rd
			// but not for compiled code being called by
			//    JAL xxxx
			RuntimeContext.invalidateAll();
		}

		[HLELogging(level:"trace"), HLEFunction(nid : 0x4FD31C9D, version : 150)]
		public virtual int sceKernelIcacheProbe(TPointer addr)
		{
			return PSP_KERNEL_ICACHE_PROBE_HIT; // Dummy
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xFB05FAD0, version = 150) public void sceKernelIcacheReadTag()
		[HLEFunction(nid : 0xFB05FAD0, version : 150)]
		public virtual void sceKernelIcacheReadTag()
		{
		}
	}
}