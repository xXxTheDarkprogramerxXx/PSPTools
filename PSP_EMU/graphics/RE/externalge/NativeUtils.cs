using System.Collections.Generic;
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
namespace pspsharp.graphics.RE.externalge
{

	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using DurationStatistics = pspsharp.util.DurationStatistics;
	using NativeCpuInfo = pspsharp.util.NativeCpuInfo;
	using Utilities = pspsharp.util.Utilities;

	using Level = org.apache.log4j.Level;
	using Logger = org.apache.log4j.Logger;

	using Unsafe = sun.misc.Unsafe;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class NativeUtils
	{
		public static Logger log = ExternalGE.log;
		private static bool isInitialized = false;
		private static bool isAvailable = false;
		private static Unsafe @unsafe = null;
		private static bool unsafeInitialized = false;
		private static long intArrayBaseOffset = 0L;
		private static long memoryIntAddress = 0L;
		private static object[] arrayObject = new object[] {null, 0x123456789ABCDEFL, 0x1111111122222222L};
		private static long arrayObjectBaseOffset = 0L;
		private static int arrayObjectIndexScale = 0;
		private static int addressSize = 0;
		private static DurationStatistics coreInterpret = new DurationStatistics("coreInterpret");
		public const int EVENT_GE_START_LIST = 0;
		public const int EVENT_GE_FINISH_LIST = 1;
		public const int EVENT_GE_ENQUEUE_LIST = 2;
		public const int EVENT_GE_UPDATE_STALL_ADDR = 3;
		public const int EVENT_GE_WAIT_FOR_LIST = 4;
		public const int EVENT_DISPLAY_WAIT_VBLANK = 5;
		public const int EVENT_DISPLAY_VBLANK = 6;
		public const int INTR_STAT_SIGNAL = 0x1;
		public const int INTR_STAT_END = 0x2;
		public const int INTR_STAT_FINISH = 0x4;
		public const int CTRL_ACTIVE = 0x1;

		public static void init()
		{
			if (!isInitialized)
			{
				IList<string> libraries = new LinkedList<string>();
				if (NativeCpuInfo.Available)
				{
					NativeCpuInfo.init();
					if (NativeCpuInfo.hasAVX2())
					{
						libraries.Add("software-ge-renderer-AVX2");
					}
					if (NativeCpuInfo.hasAVX())
					{
						libraries.Add("software-ge-renderer-AVX");
					}
					if (NativeCpuInfo.hasSSE41())
					{
						libraries.Add("software-ge-renderer-SSE41");
					}
					if (NativeCpuInfo.hasSSE3())
					{
						libraries.Add("software-ge-renderer-SSE3");
					}
					if (NativeCpuInfo.hasSSE2())
					{
						libraries.Add("software-ge-renderer-SSE2");
					}
				}
				libraries.Add("software-ge-renderer");

				bool libraryExisting = false;
				// Search for an available library in preference order
				foreach (string library in libraries)
				{
					if (Utilities.isSystemLibraryExisting(library))
					{
						libraryExisting = true;
						try
						{
//JAVA TO C# CONVERTER TODO TASK: The library is specified in the 'DllImport' attribute for .NET:
//							System.loadLibrary(library);
							RuntimeContext.updateMemory();
							initNative();
							log.info(string.Format("Loaded {0} library", library));
							isAvailable = true;
						}
						catch (UnsatisfiedLinkError e)
						{
							log.error(string.Format("Could not load external software library {0}: {1}", library, e));
							isAvailable = false;
						}
						break;
					}
				}
				if (!libraryExisting)
				{
					log.error(string.Format("Missing external software library"));
				}

				isInitialized = true;
			}
		}

		public static void exit()
		{
			if (DurationStatistics.collectStatistics)
			{
				log.info(coreInterpret.ToString());
			}
		}

		public static bool Available
		{
			get
			{
				return isAvailable;
			}
		}

		public static void checkMemoryIntAddress()
		{
			long address = MemoryUnsafeAddr;
			if (address == 0L)
			{
				return;
			}

			int[] memoryInt = RuntimeContext.MemoryInt;
			memoryInt[0] = 0x12345678;
			int x = @unsafe.getInt(memoryIntAddress);
			if (x != memoryInt[0])
			{
				log.error(string.Format("Non matching value 0x{0:X8} - 0x{1:X8}", x, memoryInt[0]));
			}
			else
			{
				log.info(string.Format("Matching value 0x{0:X8} - 0x{1:X8}", x, memoryInt[0]));
			}
			memoryInt[0] = 0;
		}

		public static long MemoryUnsafeAddr
		{
			get
			{
				if (!ExternalGE.useUnsafe || !RuntimeContext.hasMemoryInt())
				{
					return 0L;
				}
    
				if (!unsafeInitialized)
				{
					try
					{
						Field f = typeof(Unsafe).getDeclaredField("theUnsafe");
						if (f != null)
						{
							f.Accessible = true;
							@unsafe = (Unsafe) f.get(null);
							intArrayBaseOffset = @unsafe.arrayBaseOffset(RuntimeContext.MemoryInt.GetType());
							arrayObjectBaseOffset = @unsafe.arrayBaseOffset(arrayObject.GetType());
							arrayObjectIndexScale = @unsafe.arrayIndexScale(arrayObject.GetType());
							addressSize = @unsafe.addressSize();
    
							if (log.InfoEnabled)
							{
								log.info(string.Format("Unsafe address information: addressSize={0:D}, arrayBaseOffset={1:D}, indexScale={2:D}", addressSize, arrayObjectBaseOffset, arrayObjectIndexScale));
							}
    
							if (addressSize != 4 && addressSize != 8)
							{
								log.error(string.Format("Unknown addressSize={0:D}", addressSize));
							}
							if (arrayObjectIndexScale != 4 && arrayObjectIndexScale != 8)
							{
								log.error(string.Format("Unknown addressSize={0:D}, indexScale={1:D}", addressSize, arrayObjectIndexScale));
							}
							if (arrayObjectIndexScale > addressSize)
							{
								log.error(string.Format("Unknown addressSize={0:D}, indexScale={1:D}", addressSize, arrayObjectIndexScale));
							}
						}
					}
					catch (NoSuchFieldException e)
					{
						log.error("getMemoryUnsafeAddr", e);
					}
					catch (SecurityException e)
					{
						log.error("getMemoryUnsafeAddr", e);
					}
					catch (System.ArgumentException e)
					{
						log.error("getMemoryUnsafeAddr", e);
					}
					catch (IllegalAccessException e)
					{
						log.error("getMemoryUnsafeAddr", e);
					}
					unsafeInitialized = true;
				}
    
				if (@unsafe == null)
				{
					return 0L;
				}
    
				arrayObject[0] = RuntimeContext.MemoryInt;
				long address = 0L;
				if (addressSize == 4)
				{
					address = @unsafe.getInt(arrayObject, arrayObjectBaseOffset);
					address &= 0xFFFFFFFFL;
				}
				else if (addressSize == 8)
				{
					if (arrayObjectIndexScale == 8)
					{
						// The JVM is running with the following option disabled:
						//   -XX:-UseCompressedOops
						// Object addresses are stored as 64-bit values.
						address = @unsafe.getLong(arrayObject, arrayObjectBaseOffset);
					}
					else if (arrayObjectIndexScale == 4)
					{
						// The JVM is running with the following option enabled
						//   -XX:+UseCompressedOops
						// Object addresses are stored as compressed 32-bit values (shifted by 3).
						address = @unsafe.getInt(arrayObject, arrayObjectBaseOffset) & 0xFFFFFFFFL;
						address <<= 3;
					}
				}
    
				if (address == 0L)
				{
					return address;
				}
    
				if (false)
				{
					// Perform a self-test
					int[] memoryInt = RuntimeContext.MemoryInt;
					int testValue = 0x12345678;
					int originalValue = memoryInt[0];
					memoryInt[0] = testValue;
					int resultValue = @unsafe.getInt(address + intArrayBaseOffset);
					memoryInt[0] = originalValue;
					if (resultValue != testValue)
					{
						log.error(string.Format("Unsafe self-test failed: 0x{0:X8} != 0x{1:X8}", testValue, resultValue));
					}
				}
    
				return address + intArrayBaseOffset;
			}
		}

		public static void updateMemoryUnsafeAddr()
		{
			long address = MemoryUnsafeAddr;
			if (memoryIntAddress != address)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("memoryInt at 0x{0:X}", address));
				}
				if (log.InfoEnabled && memoryIntAddress != 0L)
				{
					log.info(string.Format("memoryInt MOVED from 0x{0:X} to 0x{1:X}", memoryIntAddress, address));
				}

				memoryIntAddress = address;
				MemoryUnsafeAddr = memoryIntAddress;
			}
		}

		public static void setLogLevel()
		{
			LogLevel = log;
		}

		public static Logger LogLevel
		{
			set
			{
				int level = 7; // E_DEFAULT
				switch (value.EffectiveLevel.toInt())
				{
					case Level.ALL_INT:
						level = 7; // E_FORCE
						break;
					case Level.TRACE_INT:
						level = 6; // E_TRACE
						break;
					case Level.DEBUG_INT:
						level = 5; // E_DEBUG
						break;
					case Level.INFO_INT:
						level = 4; // E_INFO
						break;
					case Level.WARN_INT:
						level = 3; // E_WARN
						break;
					case Level.ERROR_INT:
						level = 2; // E_ERROR
						break;
					case Level.FATAL_INT:
						level = 1; // E_FATAL
						break;
					case Level.OFF_INT:
						level = 0; // E_OFF
						break;
				}
    
				LogLevel = level;
			}
		}

		public static bool coreInterpretWithStatistics()
		{
			coreInterpret.start();
			bool result = coreInterpret();
			coreInterpret.end();

			return result;
		}

		public static bool CoreCtrlActive
		{
			get
			{
				return (CoreCtrl & CTRL_ACTIVE) != 0;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int initNative();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern boolean coreInterpret();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreCtrl();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreCtrl(int ctrl);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreCtrlActive();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreStat();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreStat(int stat);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreMadr();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreMadr(int madr);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreSadr();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreSadr(int sadr);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreIntrStat();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreIntrStat(int intrStat);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreInterrupt();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreInterrupt(int interrupt);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreCmdArray(int cmd);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreCmdArray(int cmd, int value);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void interpretCoreCmd(int cmd, int value, int madr);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern float getCoreMtxArray(int mtx);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreMtxArray(int mtx, float value);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setLogLevel(int level);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setMemoryUnsafeAddr(long addr);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void startEvent(int @event);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void stopEvent(int @event);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void notifyEvent(int @event);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setRendererAsyncRendering(bool asyncRendering);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getRendererIndexCount();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void rendererRender(int lineMask);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void rendererTerminate();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setDumpFrames(bool dumpFrames);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setDumpTextures(bool dumpTextures);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void saveCoreContext(int addr);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void restoreCoreContext(int addr);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setScreenScale(int screenScale);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern ByteBuffer getScaledScreen(int address, int bufferWidth, int height, int pixelFormat);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void addVideoTexture(int destinationAddress, int sourceAddress, int length);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setMaxTextureSizeLog2(int maxTextureSizeLog2);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setDoubleTexture2DCoords(bool doubleTexture2DCoords);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void doTests();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreOadr();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreOadr(int oadr);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreOadr1();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreOadr1(int oadr1);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreOadr2();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreOadr2(int oadr2);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreRadr1();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreRadr1(int radr1);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreRadr2();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreRadr2(int radr2);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreVadr();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreVadr(int vadr);
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern int getCoreIadr();
//JAVA TO C# CONVERTER TODO TASK: Replace 'unknown' with the appropriate dll name:
		[DllImport("unknown")]
		public static extern void setCoreIadr(int iadr);
	}

}