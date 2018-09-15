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
namespace pspsharp
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.compiler.RuntimeContext.getPc;


	using Modules = pspsharp.HLE.Modules;
	using VideoEngine = pspsharp.graphics.VideoEngine;
	using Screen = pspsharp.hardware.Screen;
	using DebuggerMemory = pspsharp.memory.DebuggerMemory;
	using DirectBufferMemory = pspsharp.memory.DirectBufferMemory;
	using FastMemory = pspsharp.memory.FastMemory;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using NativeMemory = pspsharp.memory.NativeMemory;
	using SafeDirectBufferMemory = pspsharp.memory.SafeDirectBufferMemory;
	using SafeFastMemory = pspsharp.memory.SafeFastMemory;
	using SafeNativeMemory = pspsharp.memory.SafeNativeMemory;
	using SafeSparseNativeMemory = pspsharp.memory.SafeSparseNativeMemory;
	using SparseNativeMemory = pspsharp.memory.SparseNativeMemory;
	using StandardMemory = pspsharp.memory.StandardMemory;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using Settings = pspsharp.settings.Settings;
	using IState = pspsharp.state.IState;
	using StateInputStream = pspsharp.state.StateInputStream;
	using StateOutputStream = pspsharp.state.StateOutputStream;

	//using Logger = org.apache.log4j.Logger;

	public abstract class Memory : IState
	{
		//public static Logger log = Logger.getLogger("memory");
		private static Memory instance = null;
		public static bool useNativeMemory = false;
		public static bool useDirectBufferMemory = false;
		public static bool useSafeMemory = true;
		public const int addressMask = 0x1FFFFFFF;
		private bool ignoreInvalidMemoryAccess = false;
		protected internal const int MEMORY_PAGE_SHIFT = 12;
		protected internal static readonly bool[] validMemoryPage = new bool[1 << ((sizeof(int) * 8) - MEMORY_PAGE_SHIFT)];
		// Assume that a video check during a memcpy is only necessary
		// when copying at least one screen row (at 2 bytes per pixel).
		private static readonly int MINIMUM_LENGTH_FOR_VIDEO_CHECK = Screen.width * 2;
		private const int STATE_VERSION = 0;

		public static Memory Instance
		{
			get
			{
				if (instance == null)
				{
					//
					// The following memory implementations are available:
					// - StandardMemory        :  low memory requirements, performs address checking, slow
					// - SafeFastMemory        : high memory requirements, performs address checking, fast
					// - FastMemory            : high memory requirements, no address checking, very fast
					// - SafeDirectBufferMemory: high memory requirements, performs address checking, moderate
					// - DirectBufferMemory    : high memory requirements, no address checking, fast
					//
					// Best choices are currently
					// 1) SafeFastMemory (address check is useful when debugging programs)
					// 2) StandardMemory when available memory is not sufficient for 1st choice
					//
    
					bool useDebuggerMemory = false;
					if (Settings.Instance.readBool("emu.useDebuggerMemory") || System.IO.Directory.Exists(DebuggerMemory.mBrkFilePath) || System.IO.File.Exists(DebuggerMemory.mBrkFilePath))
					{
						useDebuggerMemory = true;
						// Always use the safe memory when using the debugger memory
						useSafeMemory = true;
					}
    
					// Disable address checking when the option
					// "ignoring invalid memory access" is selected.
					if (Settings.Instance.readBool("emu.ignoreInvalidMemoryAccess") && !useDebuggerMemory)
					{
						useSafeMemory = false;
					}
    
					if (useNativeMemory)
					{
						try
						{
//JAVA TO C# CONVERTER TODO TASK: The library is specified in the 'DllImport' attribute for .NET:
//							System.loadLibrary("memory");
						}
						catch (UnsatisfiedLinkError e)
						{
							Console.WriteLine("Cannot load memory library", e);
							useNativeMemory = false;
						}
					}
    
					if (useNativeMemory)
					{
						if (useSafeMemory)
						{
							instance = new SafeNativeMemory();
						}
						else
						{
							instance = new NativeMemory();
						}
					}
					else if (useDirectBufferMemory)
					{
						if (useSafeMemory)
						{
							instance = new SafeDirectBufferMemory();
						}
						else
						{
							instance = new DirectBufferMemory();
						}
					}
					else
					{
						if (useSafeMemory)
						{
							instance = new SafeFastMemory();
						}
						else
						{
							instance = new FastMemory();
						}
					}
    
					if (instance != null)
					{
						if (!instance.allocate())
						{
							instance = null;
    
							// Second chance for a native memory...
							if (useNativeMemory)
							{
								if (useSafeMemory)
								{
									instance = new SafeSparseNativeMemory();
								}
								else
								{
									instance = new SparseNativeMemory();
								}
    
								if (!instance.allocate())
								{
									Console.WriteLine(string.Format("Cannot allocate native memory"));
									instance = null;
								}
							}
						}
					}
    
					if (instance == null)
					{
						instance = new StandardMemory();
						if (!instance.allocate())
						{
							instance = null;
						}
					}
    
					if (instance == null)
					{
						throw new System.OutOfMemoryException("Cannot allocate memory");
					}
    
					if (useDebuggerMemory)
					{
						DebuggerMemory.install();
					}
    
					//if (log.DebugEnabled)
					{
	//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						Console.WriteLine(string.Format("Using {0}", instance.GetType().FullName));
					}
				}
    
				return instance;
			}
			set
			{
				instance = value;
			}
		}

		private class IgnoreInvalidMemoryAccessSettingsListerner : AbstractBoolSettingsListener
		{
			private readonly Memory outerInstance;

			public IgnoreInvalidMemoryAccessSettingsListerner(Memory outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			protected internal override void settingsValueChanged(bool value)
			{
				outerInstance.IgnoreInvalidMemoryAccess = value;
			}
		}

		protected internal Memory()
		{
			Settings.Instance.registerSettingsListener("Memory", "emu.ignoreInvalidMemoryAccess", new IgnoreInvalidMemoryAccessSettingsListerner(this));
		}


		public virtual void invalidMemoryAddress(int address, string prefix, int status)
		{
			string message = string.Format("{0} - Invalid memory address: 0x{1:X8} PC=0x{2:X8}", prefix, address, Pc);

			if (ignoreInvalidMemoryAccess)
			{
				Console.WriteLine("IGNORED: " + message);
			}
			else
			{
				Console.WriteLine(message);
				Emulator.PauseEmuWithStatus(status);
			}
		}

		public virtual void invalidMemoryAddress(int address, int Length, string prefix, int status)
		{
			string message = string.Format("{0} - Invalid memory address: 0x{1:X8}-0x{2:X8}(Length=0x{3:X}) PC=0x{4:X8}", prefix, address, address + Length, Length, Pc);

			if (ignoreInvalidMemoryAccess)
			{
				Console.WriteLine("IGNORED: " + message);
			}
			else
			{
				Console.WriteLine(message);
				Emulator.PauseEmuWithStatus(status);
			}
		}

		public virtual bool read32AllowedInvalidAddress(int address)
		{
			//
			// Ugly hack for programs using pspsdk :-(
			//
			// The function pspSdkInstallNoPlainModuleCheckPatch()
			// is trying to patch 2 psp modules and is expecting to have
			// the module stub implemented as a Jump instruction,
			// something like:
			//          [08XXXXXX]: j YYYYYYYY        // YYYYYYYY = XXXXXX << 2
			//          [00000000]: nop
			//
			// pspsharp is however based on the following code sequence, e.g.:
			//          [03E00008]: jr $ra
			//          [00081B4C]: syscall 0x0206D
			//
			// The function pspSdkInstallNoPlainModuleCheckPatch()
			// is retrieving the address of the Jump instruction and reading
			// from it in kernel mode.
			// On pspsharp, it is thus trying to read at the following address
			//          0x8f800020 = (0x03E00008 << 2) || 0x80000000
			// up to    0x8f8001ac
			//
			// The hack here is to allow these memory reads and returns 0.
			//
			// Here is the C code from pspsdk:
			//
			//          int pspSdkInstallNoPlainModuleCheckPatch(void)
			//          {
			//              u32 *addr;
			//              int i;
			//
			//              addr = (u32*) (0x80000000 | ((sceKernelProbeExecutableObject & 0x03FFFFFF) << 2));
			//              //printf("sceKernelProbeExecutableObject %p\n", addr);
			//              for(i = 0; i < 100; i++)
			//              {
			//                  if((addr[i] & 0xFFE0FFFF) == LOAD_EXEC_PLAIN_CHECK)
			//                  {
			//                      //printf("Found instruction %p\n", &addr[i]);
			//                      addr[i] = (LOAD_EXEC_PLAIN_PATCH | (addr[i] & ~0xFFE0FFFF));
			//                  }
			//              }
			//
			//              addr = (u32*) (0x80000000 | ((sceKernelCheckPspConfig & 0x03FFFFFF) << 2));
			//              //printf("sceCheckPspConfig %p\n", addr);
			//              for(i = 0; i < 100; i++)
			//              {
			//                  if((addr[i] & 0xFFE0FFFF) == LOAD_EXEC_PLAIN_CHECK)
			//                  {
			//                      //printf("Found instruction %p\n", &addr[i]);
			//                      addr[i] = (LOAD_EXEC_PLAIN_PATCH | (addr[i] & ~0xFFE0FFFF));
			//                  }
			//              }
			//
			//              sceKernelDcacheWritebackAll();
			//
			//              return 0;
			//          }
			//
			if ((address >= unchecked((int)0x8f800020) && address <= unchecked((int)0x8f8001ac)) || (address >= 0x0f800020 && address <= 0x0f8001ac))
			{ // Accept also masked address
				Console.WriteLine("read32 - ignoring pspSdkInstallNoPlainModuleCheckPatch");
				return true;
			}

			return false;
		}

		public abstract void Initialise();

		public abstract int read8(int address);

		public abstract int read16(int address);

		public abstract int read32(int address);

		public abstract void write8(int address, sbyte data);

		public abstract void write16(int address, short data);

		public abstract void write32(int address, int data);

		public abstract void memset(int address, sbyte data, int Length);

		public abstract Buffer MainMemoryByteBuffer {get;}

		public abstract Buffer getBuffer(int address, int Length);

		public abstract void copyToMemory(int address, ByteBuffer source, int Length);

		protected internal abstract void memcpy(int destination, int source, int Length, bool checkOverlap);

		public static bool isAddressGood(int address)
		{
			return validMemoryPage[(int)((uint)address >> MEMORY_PAGE_SHIFT)];
		}

		public static bool isAddressAlignedTo(int address, int alignment)
		{
			return (address % alignment) == 0;
		}

		public static bool isRawAddressGood(int rawAddress)
		{
			return validMemoryPage[rawAddress >> MEMORY_PAGE_SHIFT];
		}

		public virtual bool allocate()
		{
			for (int i = 0; i < validMemoryPage.Length; i++)
			{
				int address = normalizeAddress(i << MEMORY_PAGE_SHIFT);

				bool isValid = false;
				if (address >= MemoryMap.START_RAM && address <= MemoryMap.END_RAM)
				{
					isValid = true;
				}
				else if (address >= MemoryMap.START_VRAM && address <= MemoryMap.END_VRAM)
				{
					isValid = true;
				}
				else if (address >= MemoryMap.START_SCRATCHPAD && address <= MemoryMap.END_SCRATCHPAD)
				{
					isValid = true;
				}

				validMemoryPage[i] = isValid;
			}

			return true;
		}

		public virtual long read64(int address)
		{
			long low = read32(address);
			long high = read32(address + 4);
			return (low & 0xFFFFFFFFL) | (high << 32);
		}

		public virtual void write64(int address, long data)
		{
			write32(address, (int) data);
			write32(address + 4, (int)(data >> 32));
		}

		// memcpy does not check overlapping source and destination areas
		public virtual void memcpy(int destination, int source, int Length)
		{
			memcpy(destination, source, Length, false);
		}

		/// <summary>
		/// Same as memcpy but checking if the source/destination are not used as video textures.
		/// </summary>
		/// <param name="destination">   destination address </param>
		/// <param name="source">        source address </param>
		/// <param name="Length">        Length in bytes to be copied </param>
		public virtual void memcpyWithVideoCheck(int destination, int source, int Length)
		{
			// As an optimization, do not perform the video check if we are copying only a small memory area.
			if (Length >= MINIMUM_LENGTH_FOR_VIDEO_CHECK)
			{
				// If copying to the VRAM or the frame buffer, do not cache the texture
				if (isVRAM(destination) || Modules.sceDisplayModule.isFbAddress(destination))
				{
					// If the display is rendering to the destination address, wait for its completion
					// before performing the memcpy.
					Modules.sceDisplayModule.waitForRenderingCompletion(destination);

					VideoEngine.Instance.addVideoTexture(destination, source, Length);
				}
				// If copying from the VRAM, force the saving of the GE to memory
				if (isVRAM(source) && Modules.sceDisplayModule.SaveGEToTexture)
				{
					VideoEngine.Instance.addVideoTexture(source, source + Length);
				}
				if (isVRAM(source))
				{
					Modules.sceDisplayModule.waitForRenderingCompletion(source);
				}
			}
			else if (isVRAM(destination))
			{
				Modules.sceDisplayModule.waitForRenderingCompletion(destination);
			}

			memcpy(destination, source, Length);
		}

		/// <summary>
		/// Same as memset but checking if the destination is not used as video texture.
		/// </summary>
		/// <param name="address">   destination address </param>
		/// <param name="data">      byte to be set in memory </param>
		/// <param name="Length">    Length in bytes to be set </param>
		public virtual void memsetWithVideoCheck(int address, sbyte data, int Length)
		{
			// As an optimization, do not perform the video check if we are setting only a small memory area.
			if (Length >= MINIMUM_LENGTH_FOR_VIDEO_CHECK)
			{
				// If changing the VRAM or the frame buffer, do not cache the texture
				if (isVRAM(address) || Modules.sceDisplayModule.isFbAddress(address))
				{
					// If the display is rendering to the destination address, wait for its completion
					// before performing the memcpy.
					Modules.sceDisplayModule.waitForRenderingCompletion(address);

					VideoEngine.Instance.addVideoTexture(address, address + Length);
				}
			}
			else if (isVRAM(address))
			{
				Modules.sceDisplayModule.waitForRenderingCompletion(address);
			}

			memset(address, data, Length);
		}

		// memmove reproduces the bytes correctly at destination even if the two areas overlap
		public virtual void memmove(int destination, int source, int Length)
		{
			memcpy(destination, source, Length, true);
		}

		public virtual int normalize(int address)
		{
			return address & addressMask;
		}

		public static int normalizeAddress(int address)
		{
			address &= addressMask;

			// Test on a PSP: 0x4200000 is equivalent to 0x4000000
			if ((address & unchecked((int)0xFF000000)) == MemoryMap.START_VRAM)
			{
				address &= unchecked((int)0xFF1FFFFF);
			}

			return address;
		}

		protected internal virtual bool areOverlapping(int destination, int source, int Length)
		{
			if (source + Length <= destination || destination + Length <= source)
			{
				return false;
			}

			return true;
		}

		public virtual bool IgnoreInvalidMemoryAccess
		{
			get
			{
				return ignoreInvalidMemoryAccess;
			}
			set
			{
				this.ignoreInvalidMemoryAccess = value;
			}
		}


		public static bool isRAM(int address)
		{
			address &= addressMask;
			return address >= MemoryMap.START_RAM && address <= MemoryMap.END_RAM;
		}

		public static bool isVRAM(int address)
		{
			address &= addressMask;
			// Test first against END_VRAM as it is most likely to fail first (because RAM is above VRAM)
			return address <= MemoryMap.END_VRAM && address >= MemoryMap.START_VRAM;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void read(pspsharp.state.StateInputStream stream, int address, int Length) throws java.io.IOException
		protected internal virtual void read(StateInputStream stream, int address, int Length)
		{
			IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(this, address, Length, 4);
			for (int i = 0; i < Length; i += 4)
			{
				memoryWriter.writeNext(stream.readInt());
			}
			memoryWriter.flush();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void write(pspsharp.state.StateOutputStream stream, int address, int Length) throws java.io.IOException
		protected internal virtual void write(StateOutputStream stream, int address, int Length)
		{
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(this, address, Length, 4);
			for (int i = 0; i < Length; i += 4)
			{
				stream.writeInt(memoryReader.readNext());
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void read(pspsharp.state.StateInputStream stream) throws java.io.IOException
		public virtual void read(StateInputStream stream)
		{
			stream.readVersion(STATE_VERSION);
			read(stream, MemoryMap.START_SCRATCHPAD, MemoryMap.SIZE_SCRATCHPAD);
			read(stream, MemoryMap.START_VRAM, MemoryMap.SIZE_VRAM);
			read(stream, MemoryMap.START_RAM, MemoryMap.SIZE_RAM);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public void write(pspsharp.state.StateOutputStream stream) throws java.io.IOException
		public virtual void write(StateOutputStream stream)
		{
			stream.writeVersion(STATE_VERSION);
			write(stream, MemoryMap.START_SCRATCHPAD, MemoryMap.SIZE_SCRATCHPAD);
			write(stream, MemoryMap.START_VRAM, MemoryMap.SIZE_VRAM);
			write(stream, MemoryMap.START_RAM, MemoryMap.SIZE_RAM);
		}
	}
}