using System;
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
namespace pspsharp
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._a2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._v0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._v1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._zr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.ADDIU;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.MOVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.NOP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Memory.addressMask;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.Elf32ProgramHeader.PF_X;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.Elf32SectionHeader.SHF_ALLOCATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.Elf32SectionHeader.SHF_EXECUTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.Elf32SectionHeader.SHF_NONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.Elf32SectionHeader.SHF_WRITE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.patch;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.patchRemoveStringChar;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUnaligned32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeInt32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeUnaligned32;


	//using Logger = org.apache.log4j.Logger;

	using Common = pspsharp.Allegrex.Common;
	using Opcodes = pspsharp.Allegrex.Opcodes;
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using ElfHeaderInfo = pspsharp.Debugger.ElfHeaderInfo;
	using Modules = pspsharp.HLE.Modules;
	using Managers = pspsharp.HLE.kernel.Managers;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using SysMemUserForUser = pspsharp.HLE.modules.SysMemUserForUser;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using DeferredStub = pspsharp.format.DeferredStub;
	using DeferredVStub32 = pspsharp.format.DeferredVStub32;
	using DeferredVStubHI16 = pspsharp.format.DeferredVStubHI16;
	using DeferredVstubLO16 = pspsharp.format.DeferredVstubLO16;
	using Elf32 = pspsharp.format.Elf32;
	using Elf32EntHeader = pspsharp.format.Elf32EntHeader;
	using Elf32ProgramHeader = pspsharp.format.Elf32ProgramHeader;
	using Elf32Relocate = pspsharp.format.Elf32Relocate;
	using Elf32SectionHeader = pspsharp.format.Elf32SectionHeader;
	using Elf32StubHeader = pspsharp.format.Elf32StubHeader;
	using PBP = pspsharp.format.PBP;
	using PSF = pspsharp.format.PSF;
	using PSP = pspsharp.format.PSP;
	using PSPModuleInfo = pspsharp.format.PSPModuleInfo;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemorySection = pspsharp.memory.MemorySection;
	using MemorySections = pspsharp.memory.MemorySections;
	using Settings = pspsharp.settings.Settings;
	using Utilities = pspsharp.util.Utilities;

	public class Loader
	{
		private static Loader instance;
		private static Logger log = Logger.getLogger("loader");

		public const int SCE_MAGIC = 0x4543537E;
		public const int PSP_MAGIC = 0x50535000;
		public const int EDAT_MAGIC = 0x54414445;
		public const int FIRMWAREVERSION_HOMEBREW = 999; // Simulate version 9.99 instead of 1.50

		// Format bits
		public const int FORMAT_UNKNOWN = 0x00;
		public const int FORMAT_ELF = 0x01;
		public const int FORMAT_PRX = 0x02;
		public const int FORMAT_PBP = 0x04;
		public const int FORMAT_SCE = 0x08;
		public const int FORMAT_PSP = 0x10;

		public static Loader Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Loader();
				}
				return instance;
			}
		}

		private Loader()
		{
		}

		/// <param name="pspfilename">   Example:
		///                      ms0:/PSP/GAME/xxx/EBOOT.PBP
		///                      disc0:/PSP_GAME/SYSDIR/BOOT.BIN
		///                      disc0:/PSP_GAME/SYSDIR/EBOOT.BIN
		///                      xxx:/yyy/zzz.prx </param>
		/// <param name="f">             the module file contents </param>
		/// <param name="baseAddress">   should be at least 64-byte aligned,
		///                      or how ever much is the default alignment in pspsysmem. </param>
		/// <param name="analyzeOnly">   true, if the module is not really loaded, but only
		///                            the SceModule object is returned;
		///                      false, if the module is really loaded in memory. </param>
		/// <returns>              Always a SceModule object, you should check the
		///                      fileFormat member against the FORMAT_* bits.
		///                      Example: (fileFormat & FORMAT_ELF) == FORMAT_ELF
		///  </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public pspsharp.HLE.kernel.types.SceModule LoadModule(String pspfilename, ByteBuffer f, int baseAddress, int mpidText, int mpidData, boolean analyzeOnly, boolean allocMem, boolean fromSyscall) throws java.io.IOException
		public virtual SceModule LoadModule(string pspfilename, ByteBuffer f, int baseAddress, int mpidText, int mpidData, bool analyzeOnly, bool allocMem, bool fromSyscall)
		{
			SceModule module = new SceModule(false);

			int currentOffset = f.position();
			module.fileFormat = FORMAT_UNKNOWN;
			module.pspfilename = pspfilename;
			module.mpidtext = mpidText;
			module.mpiddata = mpidData;

			// The PSP startup code requires a ":" into the argument passed to the root thread.
			// On Linux, there is no ":" in the file name when loading a .pbp file;
			// on Windows, there is luckily one ":" in "C:/...".
			// Simulate a ":" by prefixing by "ms0:", as this is not really used by an application.
			if (!string.ReferenceEquals(module.pspfilename, null) && !module.pspfilename.Contains(":"))
			{
				module.pspfilename = "ms0:" + module.pspfilename;
			}

			if (f.capacity() - f.position() == 0)
			{
				Console.WriteLine("LoadModule: no data.");
				return module;
			}

			// chain loaders
			do
			{
				f.position(currentOffset);
				if (LoadPBP(f, module, baseAddress, analyzeOnly, allocMem, fromSyscall))
				{
					currentOffset = f.position();

					// probably kxploit stub
					if (currentOffset == f.limit())
					{
						break;
					}
				}
				else if (!fromSyscall)
				{
					loadPSF(module, analyzeOnly, allocMem, fromSyscall);
				}

				if (module.psf != null)
				{
					log.info(string.Format("PBP meta data:{0}{1}", Environment.NewLine, module.psf));

					if (!fromSyscall)
					{
						// Set firmware version from PSF embedded in PBP
						if (module.psf.LikelyHomebrew)
						{
							Emulator.Instance.FirmwareVersion = FIRMWAREVERSION_HOMEBREW;
						}
						else
						{
							Emulator.Instance.FirmwareVersion = module.psf.getString("PSP_SYSTEM_VER");
						}
						Modules.SysMemUserForUserModule.Memory64MB = module.psf.getNumeric("MEMSIZE") == 1;
					}
				}

				f.position(currentOffset);
				if (LoadSPRX(f, module, baseAddress, analyzeOnly, allocMem, fromSyscall))
				{
					break;
				}

				f.position(currentOffset);
				if (LoadSCE(f, module, baseAddress, analyzeOnly, allocMem, fromSyscall))
				{
					break;
				}

				f.position(currentOffset);
				if (LoadPSP(f, module, baseAddress, analyzeOnly, allocMem, fromSyscall))
				{
					break;
				}

				f.position(currentOffset);
				if (LoadELF(f, module, baseAddress, analyzeOnly, allocMem, fromSyscall))
				{
					if (!fromSyscall)
					{
						Emulator.Instance.FirmwareVersion = FIRMWAREVERSION_HOMEBREW;
					}
					break;
				}

				f.position(currentOffset);
				LoadUNK(f, module, baseAddress, analyzeOnly, allocMem, fromSyscall);
			} while (false);

			if (!analyzeOnly)
			{
				patchModule(module);
			}

			return module;
		}

		private void loadPSF(SceModule module, bool analyzeOnly, bool allocMem, bool fromSyscall)
		{
			if (module.psf != null)
			{
				return;
			}

			string filetoload = module.pspfilename;
			if (filetoload.StartsWith("ms0:", StringComparison.Ordinal))
			{
				filetoload = filetoload.Replace("ms0:", "ms0");
			}

			// PBP doesn't have a PSF included. Check for one in kxploit directories
			File metapbp = null;
			File pbpfile = new File(filetoload);
			if (pbpfile.ParentFile == null || pbpfile.ParentFile.ParentFile == null)
			{
				// probably dynamically loading a prx
				return;
			}

			// %__SCE__kxploit
			File metadir = new File(pbpfile.ParentFile.ParentFile.Path + System.IO.Path.DirectorySeparatorChar + "%" + pbpfile.ParentFile.Name);
			if (metadir.exists())
			{
				File[] eboot = metadir.listFiles(new FileFilterAnonymousInnerClass(this));
				if (eboot.Length > 0)
				{
					metapbp = eboot[0];
				}
			}

			// kxploit%
			metadir = new File(pbpfile.ParentFile.ParentFile.Path + System.IO.Path.DirectorySeparatorChar + pbpfile.ParentFile.Name + "%");
			if (metadir.exists())
			{
				File[] eboot = metadir.listFiles(new FileFilterAnonymousInnerClass2(this));
				if (eboot.Length > 0)
				{
					metapbp = eboot[0];
				}
			}

			if (metapbp != null)
			{
				// Load PSF embedded in PBP
				FileChannel roChannel;
				try
				{
					RandomAccessFile raf = new RandomAccessFile(metapbp, "r");
					roChannel = raf.Channel;
					ByteBuffer readbuffer = roChannel.map(FileChannel.MapMode.READ_ONLY, 0, (int)roChannel.size());
					PBP meta = new PBP(readbuffer);
					module.psf = meta.readPSF(readbuffer);
					raf.close();
				}
				catch (FileNotFoundException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
			else
			{
				// Load unpacked PSF in the same directory
				File[] psffile = pbpfile.ParentFile.listFiles(new FileFilterAnonymousInnerClass3(this));
				if (psffile != null && psffile.Length > 0)
				{
					try
					{
						RandomAccessFile raf = new RandomAccessFile(psffile[0], "r");
						FileChannel roChannel = raf.Channel;
						ByteBuffer readbuffer = roChannel.map(FileChannel.MapMode.READ_ONLY, 0, (int)roChannel.size());
						module.psf = new PSF();
						module.psf.read(readbuffer);
						raf.close();
					}
					catch (IOException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
				}
			}
		}

		private class FileFilterAnonymousInnerClass : FileFilter
		{
			private readonly Loader outerInstance;

			public FileFilterAnonymousInnerClass(Loader outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override bool accept(File arg0)
			{
				return arg0.Name.equalsIgnoreCase("eboot.pbp");
			}
		}

		private class FileFilterAnonymousInnerClass2 : FileFilter
		{
			private readonly Loader outerInstance;

			public FileFilterAnonymousInnerClass2(Loader outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override bool accept(File arg0)
			{
				return arg0.Name.equalsIgnoreCase("eboot.pbp");
			}
		}

		private class FileFilterAnonymousInnerClass3 : FileFilter
		{
			private readonly Loader outerInstance;

			public FileFilterAnonymousInnerClass3(Loader outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override bool accept(File arg0)
			{
				return arg0.Name.equalsIgnoreCase("param.sfo");
			}
		}

		/// <returns> true on success </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean LoadPBP(ByteBuffer f, pspsharp.HLE.kernel.types.SceModule module, int baseAddress, boolean analyzeOnly, boolean allocMem, boolean fromSyscall) throws java.io.IOException
		private bool LoadPBP(ByteBuffer f, SceModule module, int baseAddress, bool analyzeOnly, bool allocMem, bool fromSyscall)
		{
			PBP pbp = new PBP(f);
			if (pbp.Valid)
			{
				module.fileFormat |= FORMAT_PBP;

				// Dump PSF info
				if (pbp.OffsetParam > 0)
				{
					module.psf = pbp.readPSF(f);
				}

				// Dump unpacked PBP
				if (Settings.Instance.readBool("emu.pbpunpack"))
				{
					PBP.unpackPBP(f);
				}

				// Save PBP info for debugger
				ElfHeaderInfo.PbpInfo = pbp.ToString();

				// Setup position for chaining loaders
				f.position(pbp.OffsetPspData);
				return true;
			}
			// Not a valid PBP
			return false;
		}

		/// <returns> true on success </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean LoadSPRX(ByteBuffer f, pspsharp.HLE.kernel.types.SceModule module, int baseAddress, boolean analyzeOnly, boolean allocMem, boolean fromSyscall) throws java.io.IOException
		private bool LoadSPRX(ByteBuffer f, SceModule module, int baseAddress, bool analyzeOnly, bool allocMem, bool fromSyscall)
		{
			int magicPSP = Utilities.readWord(f);
			int magicEDAT = Utilities.readWord(f);
			if ((magicPSP == PSP_MAGIC) && (magicEDAT == EDAT_MAGIC))
			{
				Console.WriteLine("Encrypted file detected! (.PSPEDAT)");
				// Skip the EDAT header and load as a regular ~PSP prx.
				f.position(0x90);
				LoadPSP(f.slice(), module, baseAddress, analyzeOnly, allocMem, fromSyscall);
				return true;
			}
			// Not a valid SPRX
			return false;
		}

		/// <returns> true on success </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean LoadSCE(ByteBuffer f, pspsharp.HLE.kernel.types.SceModule module, int baseAddress, boolean analyzeOnly, boolean allocMem, boolean fromSyscall) throws java.io.IOException
		private bool LoadSCE(ByteBuffer f, SceModule module, int baseAddress, bool analyzeOnly, bool allocMem, bool fromSyscall)
		{
			int magic = Utilities.readWord(f);
			if (magic == SCE_MAGIC)
			{
				module.fileFormat |= FORMAT_SCE;
				Console.WriteLine("Encrypted file not supported! (~SCE)");
				return true;
			}
			// Not a valid PSP
			return false;
		}

		/// <returns> true on success </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean LoadPSP(ByteBuffer f, pspsharp.HLE.kernel.types.SceModule module, int baseAddress, boolean analyzeOnly, boolean allocMem, boolean fromSyscall) throws java.io.IOException
		private bool LoadPSP(ByteBuffer f, SceModule module, int baseAddress, bool analyzeOnly, bool allocMem, bool fromSyscall)
		{
			PSP psp = new PSP(f);
			if (!psp.Valid)
			{
				// Not a valid PSP
				return false;
			}
			module.fileFormat |= FORMAT_PSP;

			long start = DateTimeHelper.CurrentUnixTimeMillis();
			ByteBuffer decryptedPrx = psp.decrypt(f);
			long end = DateTimeHelper.CurrentUnixTimeMillis();

			if (decryptedPrx == null)
			{
				return false;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Called crypto engine for PRX (duration={0:D} ms)", end - start));
			}

			return LoadELF(decryptedPrx, module, baseAddress, analyzeOnly, allocMem, fromSyscall);
		}

		/// <returns> true on success </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean LoadELF(ByteBuffer f, pspsharp.HLE.kernel.types.SceModule module, int baseAddress, boolean analyzeOnly, boolean allocMem, boolean fromSyscall) throws java.io.IOException
		private bool LoadELF(ByteBuffer f, SceModule module, int baseAddress, bool analyzeOnly, bool allocMem, bool fromSyscall)
		{
			int elfOffset = f.position();
			Elf32 elf = new Elf32(f);
			if (elf.Header.Valid)
			{
				module.fileFormat |= FORMAT_ELF;

				if (!elf.Header.MIPSExecutable)
				{
					Console.WriteLine("Loader NOT a MIPS executable");
					return false;
				}

				if (elf.KernelMode)
				{
					module.mpidtext = SysMemUserForUser.KERNEL_PARTITION_ID;
					module.mpiddata = SysMemUserForUser.KERNEL_PARTITION_ID;
					if (!analyzeOnly && baseAddress == MemoryMap.START_USERSPACE + 0x4000)
					{
						baseAddress = MemoryMap.START_RAM + Utilities.alignUp(ThreadManForUser.INTERNAL_THREAD_ADDRESS_SIZE, SysMemUserForUser.defaultSizeAlignment - 1);
					}
				}

				if (elf.Header.PRXDetected)
				{
					Console.WriteLine("Loader: Relocation required (PRX)");
					module.fileFormat |= FORMAT_PRX;
				}
				else if (elf.Header.requiresRelocation())
				{
					// Seen in .elf's generated by pspsdk with BUILD_PRX=1 before conversion to .prx
					log.info("Loader: Relocation required (ELF)");
				}
				else
				{
					// After the user chooses a game to run and we load it, then
					// we can't load another PBP at the same time. We can only load
					// relocatable modules (PRX's) after the user loaded app.
					if (baseAddress > 0x08900000)
					{
						Console.WriteLine("Loader: Probably trying to load PBP ELF while another PBP ELF is already loaded");
					}

					baseAddress = 0;
				}

				module.baseAddress = baseAddress;
				if (elf.Header.E_entry == -1)
				{
					module.entry_addr = -1;
				}
				else
				{
					module.entry_addr = baseAddress + elf.Header.E_entry;
				}

				// Note: baseAddress is 0 unless we are loading a PRX
				// Set loadAddressLow to the highest possible address, it will be updated
				// by LoadELFProgram().
				module.loadAddressLow = (baseAddress != 0) ? baseAddress : MemoryMap.END_USERSPACE;
				module.loadAddressHigh = baseAddress;

				// Load into mem
				LoadELFProgram(f, module, baseAddress, elf, elfOffset, analyzeOnly);
				LoadELFSections(f, module, baseAddress, elf, elfOffset, analyzeOnly);

				if (module.loadAddressLow > module.loadAddressHigh)
				{
					Console.WriteLine(string.Format("Incorrect ELF module address: loadAddressLow=0x{0:X8}, loadAddressHigh=0x{1:X8}", module.loadAddressLow, module.loadAddressHigh));
					module.loadAddressHigh = module.loadAddressLow;
				}

				if (!analyzeOnly)
				{
					// Relocate PRX
					if (elf.Header.requiresRelocation())
					{
						relocateFromHeaders(f, module, baseAddress, elf, elfOffset);
					}

					// The following can only be done after relocation
					// Load .rodata.sceModuleInfo
					LoadELFModuleInfo(f, module, baseAddress, elf, elfOffset, analyzeOnly);
					if (allocMem)
					{
						// After LoadELFModuleInfo so the we can name the memory allocation after the module name
						LoadELFReserveMemory(module);
					}
					// Save imports
					LoadELFImports(module);
					// Save exports
					LoadELFExports(module);
					// Try to fixup imports for ALL modules
					Managers.modules.addModule(module);
					ProcessUnresolvedImports(module, fromSyscall);

					// Debug
					LoadELFDebuggerInfo(f, module, baseAddress, elf, elfOffset, fromSyscall);

					// If no text_addr is available up to now, use the lowest program header address
					if (module.text_addr == 0)
					{
						foreach (Elf32ProgramHeader phdr in elf.ProgramHeaderList)
						{
							if (module.text_addr == 0 || phdr.P_vaddr < module.text_addr)
							{
								module.text_addr = phdr.P_vaddr;
								// Align the text_addr if an alignment has been specified
								if (phdr.P_align > 0)
								{
									module.text_addr = Utilities.alignDown(module.text_addr, phdr.P_align - 1);
								}
							}
						}
					}

					// Flush module struct to psp mem
					module.write(Memory.Instance, module.address);
				}
				else
				{
					LoadELFModuleInfo(f, module, baseAddress, elf, elfOffset, analyzeOnly);
				}
				return true;
			}
			// Not a valid ELF
			Console.WriteLine("Loader: Not a ELF");
			return false;
		}

		/// <summary>
		/// Dummy loader for unrecognized file formats, put at the end of a loader chain. </summary>
		/// <returns> true on success  </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean LoadUNK(ByteBuffer f, pspsharp.HLE.kernel.types.SceModule module, int baseAddress, boolean analyzeOnly, boolean allocMem, boolean fromSyscall) throws java.io.IOException
		private bool LoadUNK(ByteBuffer f, SceModule module, int baseAddress, bool analyzeOnly, bool allocMem, bool fromSyscall)
		{

			sbyte m0 = f.get();
			sbyte m1 = f.get();
			sbyte m2 = f.get();
			sbyte m3 = f.get();

			// Catch common user errors
			if (m0 == 0x43 && m1 == 0x49 && m2 == 0x53 && m3 == 0x4F)
			{ // CSO
				log.info("This is not an executable file!");
				log.info("Try using the Load UMD menu item");
			}
			else if ((m0 == 0 && m1 == 0x50 && m2 == 0x53 && m3 == 0x46))
			{ // PSF
				log.info("This is not an executable file!");
			}
			else
			{
				bool handled = false;

				// check for ISO
				if (f.limit() >= 16 * 2048 + 6)
				{
					f.position(16 * 2048);
					sbyte[] id = new sbyte[6];
					f.get(id);
					if ((((char)id[1]) == 'C') && (((char)id[2]) == 'D') && (((char)id[3]) == '0') && (((char)id[4]) == '0') && (((char)id[5]) == '1'))
					{
						log.info("This is not an executable file!");
						log.info("Try using the Load UMD menu item");
						handled = true;
					}
				}

				if (!handled)
				{
					log.info("Unrecognized file format");
					log.info(string.Format("File magic {0:X2} {1:X2} {2:X2} {3:X2}", m0, m1, m2, m3));
					//if (log.DebugEnabled)
					{
						sbyte[] buffer = new sbyte[256];
						buffer[0] = m0;
						buffer[1] = m1;
						buffer[2] = m2;
						buffer[3] = m3;
						f.get(buffer, 4, buffer.Length - 4);
						Console.WriteLine(string.Format("File header: {0}", Utilities.getMemoryDump(buffer, 0, buffer.Length)));
					}
				}
			}

			return false;
		}

		// ELF Loader

		/// <summary>
		/// Load some programs into memory </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void LoadELFProgram(ByteBuffer f, pspsharp.HLE.kernel.types.SceModule module, int baseAddress, pspsharp.format.Elf32 elf, int elfOffset, boolean analyzeOnly) throws java.io.IOException
		private void LoadELFProgram(ByteBuffer f, SceModule module, int baseAddress, Elf32 elf, int elfOffset, bool analyzeOnly)
		{

			IList<Elf32ProgramHeader> programHeaderList = elf.ProgramHeaderList;
			Memory mem = Memory.Instance;

			module.text_size = 0;
			module.data_size = 0;
			module.bss_size = 0;

			int i = 0;
			foreach (Elf32ProgramHeader phdr in programHeaderList)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("ELF Program Header: {0}", phdr.ToString()));
				}
				if (phdr.P_type == 0x00000001L)
				{
					int fileOffset = (int)phdr.P_offset;
					int memOffset = baseAddress + (int)phdr.P_vaddr;
					if (!Memory.isAddressGood(memOffset))
					{
						memOffset = (int)phdr.P_vaddr;
						if (!Memory.isAddressGood(memOffset))
						{
							Console.WriteLine(string.Format("Program header has invalid memory offset 0x{0:X8}!", memOffset));
						}
					}
					int fileLen = (int)phdr.P_filesz;
					int memLen = (int)phdr.P_memsz;

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("PH#{0:D}: loading program {1:X8} - file {2:X8} - mem {3:X8}", i, memOffset, memOffset + fileLen, memOffset + memLen));
						Console.WriteLine(string.Format("PH#{0:D}:\n{1}", i, phdr));
					}

					f.position(elfOffset + fileOffset);
					if (f.position() + fileLen > f.limit())
					{
						int newLen = f.limit() - f.position();
						Console.WriteLine(string.Format("PH#{0:D}: program overflow clamping len {1:X8} to {2:X8}", i, fileLen, newLen));
						fileLen = newLen;
					}
					if (!analyzeOnly)
					{
						if (memLen > fileLen)
						{
							// Clear the memory part not loaded from the file
							mem.memset(memOffset + fileLen, (sbyte) 0, memLen - fileLen);
						}

						if (((memOffset | fileLen | f.position()) & 3) == 0)
						{
							ByteOrder order = f.order();
							f.order(ByteOrder.LITTLE_ENDIAN);
							IntBuffer intBuffer = f.asIntBuffer();
							// Optimize the most common case
							if (RuntimeContext.hasMemoryInt())
							{
								intBuffer.get(RuntimeContext.MemoryInt, (memOffset & addressMask) >> 2, fileLen >> 2);
							}
							else
							{
								int[] buffer = new int[fileLen >> 2];
								intBuffer.get(buffer);
								writeInt32(memOffset, fileLen, buffer, 0);
							}
							f.order(order);
							f.position(f.position() + fileLen);
						}
						else
						{
							mem.copyToMemory(memOffset, f, fileLen);
						}
					}

					// Update memory area consumed by the module
					if (memOffset < module.loadAddressLow)
					{
						module.loadAddressLow = memOffset;
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("PH#{0:D}: new loadAddressLow {1:X8}", i, module.loadAddressLow));
						}
					}
					if (memOffset + memLen > module.loadAddressHigh)
					{
						module.loadAddressHigh = memOffset + memLen;
						if (log.TraceEnabled)
						{
							log.trace(string.Format("PH#{0:D}: new loadAddressHigh {1:X8}", i, module.loadAddressHigh));
						}
					}

					module.segmentaddr[module.nsegment] = memOffset;
					module.segmentsize[module.nsegment] = memLen;
					module.nsegment++;

					/*
					 * If the segment is executable, it contains the .text section.
					 * Otherwise, it contains the .data section.
					 */
					if ((phdr.P_flags & PF_X) != 0)
					{
						module.text_size += fileLen;
					}
					else
					{
						module.data_size += fileLen;
					}

					/* Add the "extra" segment bytes to the .bss section. */
					if (fileLen < memLen)
					{
						module.bss_size += memLen - fileLen;
					}
				}
				i++;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("PH alloc consumption {0:X8} (mem {1:X8})", (module.loadAddressHigh - module.loadAddressLow), module.bss_size));
			}
		}

		/// <summary>
		/// Load some sections into memory </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void LoadELFSections(ByteBuffer f, pspsharp.HLE.kernel.types.SceModule module, int baseAddress, pspsharp.format.Elf32 elf, int elfOffset, boolean analyzeOnly) throws java.io.IOException
		private void LoadELFSections(ByteBuffer f, SceModule module, int baseAddress, Elf32 elf, int elfOffset, bool analyzeOnly)
		{
			IList<Elf32SectionHeader> sectionHeaderList = elf.SectionHeaderList;
			Memory mem = Memory.Instance;

			module.text_addr = baseAddress;

			foreach (Elf32SectionHeader shdr in sectionHeaderList)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("ELF Section Header: {0}", shdr.ToString()));
				}

				int memOffset = shdr.getSh_addr(baseAddress);
				int len = shdr.Sh_size;
				int flags = shdr.Sh_flags;

				if (flags != SHF_NONE && Memory.isAddressGood(memOffset))
				{
					bool read = (flags & SHF_ALLOCATE) != 0;
					bool write = (flags & SHF_WRITE) != 0;
					bool execute = (flags & SHF_EXECUTE) != 0;
					MemorySection memorySection = new MemorySection(memOffset, len, read, write, execute);
					MemorySections.Instance.addMemorySection(memorySection);
				}

				if ((flags & SHF_ALLOCATE) != 0)
				{
					switch (shdr.Sh_type)
					{
						case Elf32SectionHeader.SHT_PROGBITS:
						{ // 1
							// Load this section into memory
							// now loaded using program header type 1
							if (len == 0)
							{
								//if (log.DebugEnabled)
								{
									Console.WriteLine(string.Format("{0}: ignoring zero-Length type 1 section {1:X8}", shdr.Sh_namez, memOffset));
								}
							}
							else if (!Memory.isAddressGood(memOffset))
							{
								Console.WriteLine(string.Format("Section header (type 1) has invalid memory offset 0x{0:X8}!", memOffset));
							}
							else
							{
								// Update memory area consumed by the module
								if (memOffset < module.loadAddressLow)
								{
									Console.WriteLine(string.Format("{0}: section allocates more than program {1:X8} - {2:X8}", shdr.Sh_namez, memOffset, (memOffset + len)));
									module.loadAddressLow = memOffset;
								}
								if (memOffset + len > module.loadAddressHigh)
								{
									Console.WriteLine(string.Format("{0}: section allocates more than program {1:X8} - {2:X8}", shdr.Sh_namez, memOffset, (memOffset + len)));
									module.loadAddressHigh = memOffset + len;
								}

								if ((flags & SHF_WRITE) != 0)
								{
									if (log.TraceEnabled)
									{
										log.trace(string.Format("Section Header as data, len=0x{0:X8}, data_size=0x{1:X8}", len, module.data_size));
									}
								}
								else
								{
									if (log.TraceEnabled)
									{
										log.trace(string.Format("Section Header as text, len=0x{0:X8}, text_size=0x{1:X8}", len, module.text_size));
									}
								}
							}
							break;
						}

						case Elf32SectionHeader.SHT_NOBITS:
						{ // 8
							// Zero out this portion of memory
							if (len == 0)
							{
								//if (log.DebugEnabled)
								{
									Console.WriteLine(string.Format("{0}: ignoring zero-Length type 8 section {1:X8}", shdr.Sh_namez, memOffset));
								}
							}
							else if (!Memory.isAddressGood(memOffset))
							{
								Console.WriteLine(string.Format("Section header (type 8) has invalid memory offset 0x{0:X8}!", memOffset));
							}
							else
							{
								//if (log.DebugEnabled)
								{
									Console.WriteLine(string.Format("{0}: clearing section {1:X8} - {2:X8} (len {3:X8})", shdr.Sh_namez, memOffset, (memOffset + len), len));
								}

								if (!analyzeOnly)
								{
									mem.memset(memOffset, (sbyte) 0x0, len);
								}

								// Update memory area consumed by the module
								if (memOffset < module.loadAddressLow)
								{
									module.loadAddressLow = memOffset;
									//if (log.DebugEnabled)
									{
										Console.WriteLine(string.Format("{0}: new loadAddressLow {1:X8} (+{2:X8})", shdr.Sh_namez, module.loadAddressLow, len));
									}
								}
								if (memOffset + len > module.loadAddressHigh)
								{
									module.loadAddressHigh = memOffset + len;
									//if (log.DebugEnabled)
									{
										Console.WriteLine(string.Format("{0}: new loadAddressHigh {1:X8} (+{2:X8})", shdr.Sh_namez, module.loadAddressHigh, len));
									}
								}
							}
							break;
						}
					}
				}
			}

			if (log.TraceEnabled)
			{
				log.trace(string.Format("Storing module info: text addr 0x{0:X8}, text_size 0x{1:X8}, data_size 0x{2:X8}, bss_size 0x{3:X8}", module.text_addr, module.text_size, module.data_size, module.bss_size));
			}
		}

		private void LoadELFReserveMemory(SceModule module)
		{
			// Mark the area of memory the module loaded into as used
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Reserving 0x{0:X} bytes at 0x{1:X8} for module '{2}'", module.loadAddressHigh - module.loadAddressLow, module.loadAddressLow, module.pspfilename));
			}

			int address = module.loadAddressLow & ~(SysMemUserForUser.defaultSizeAlignment - 1); // Round down to match sysmem allocations
			int size = module.loadAddressHigh - address;

			int partition = module.mpidtext > 0 ? module.mpidtext : SysMemUserForUser.USER_PARTITION_ID;
			SysMemUserForUser.SysMemInfo info = Modules.SysMemUserForUserModule.malloc(partition, module.modname, SysMemUserForUser.PSP_SMEM_Addr, size, address);
			if (info == null || info.addr != (address & Memory.addressMask))
			{
				Console.WriteLine(string.Format("Failed to properly reserve memory consumed by module {0} at address 0x{1:X8}, size 0x{2:X}: allocated {3}", module.modname, address, size, info));
			}
			module.addAllocatedMemory(info);
		}

		/// <summary>
		/// Loads from memory </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void LoadELFModuleInfo(ByteBuffer f, pspsharp.HLE.kernel.types.SceModule module, int baseAddress, pspsharp.format.Elf32 elf, int elfOffset, boolean analyzeOnly) throws java.io.IOException
		private void LoadELFModuleInfo(ByteBuffer f, SceModule module, int baseAddress, Elf32 elf, int elfOffset, bool analyzeOnly)
		{

			Elf32ProgramHeader phdr = elf.getProgramHeader(0);
			Elf32SectionHeader shdr = elf.getSectionHeader(".rodata.sceModuleInfo");

			int moduleInfoAddr = 0;
			int moduleInfoFileOffset = -1;
			if (!elf.Header.PRXDetected && shdr == null)
			{
				if (analyzeOnly)
				{
					moduleInfoFileOffset = phdr.P_paddr & Memory.addressMask;
				}
				else
				{
					Console.WriteLine("ELF is not PRX, but has no section headers!");
					moduleInfoAddr = phdr.P_vaddr + (phdr.P_paddr & Memory.addressMask) - phdr.P_offset;
					Console.WriteLine("Manually locating ModuleInfo at address: 0x" + moduleInfoAddr.ToString("x"));
				}
			}
			else if (elf.Header.PRXDetected)
			{
				if (analyzeOnly)
				{
					moduleInfoFileOffset = phdr.P_paddr & Memory.addressMask;
				}
				else
				{
					moduleInfoAddr = baseAddress + (phdr.P_paddr & Memory.addressMask) - phdr.P_offset;
				}
			}
			else if (shdr != null)
			{
				moduleInfoAddr = shdr.getSh_addr(baseAddress);
			}

			if (moduleInfoAddr != 0)
			{
				PSPModuleInfo moduleInfo = new PSPModuleInfo();
				moduleInfo.read(Memory.Instance, moduleInfoAddr);
				module.copy(moduleInfo);
			}
			else if (moduleInfoFileOffset >= 0)
			{
				PSPModuleInfo moduleInfo = new PSPModuleInfo();
				f.position(moduleInfoFileOffset);
				moduleInfo.read(f);
				module.copy(moduleInfo);
			}
			else
			{
				Console.WriteLine("ModuleInfo not found!");
				return;
			}

			if (!analyzeOnly)
			{
				if (log.InfoEnabled)
				{
					log.info(string.Format("Found ModuleInfo at 0x{0:X8}, name:'{1}', version: {2:X2}{3:X2}, attr: 0x{4:X8}, gp: 0x{5:X8}", moduleInfoAddr, module.modname, module.version[1], module.version[0], module.attribute, module.gp_value));
				}

				if ((module.attribute & SceModule.PSP_MODULE_KERNEL) != 0)
				{
					Console.WriteLine("Kernel mode module detected");
				}
				if ((module.attribute & SceModule.PSP_MODULE_VSH) != 0)
				{
					Console.WriteLine("VSH mode module detected");
				}
			}
		}

		/// <param name="f">        The position of this buffer must be at the start of a
		///                 list of Elf32Rel structs. </param>
		/// <param name="RelCount"> The number of Elf32Rel structs to read and process. </param>
		/// <param name="pspRelocationFormat"> true if the relocation are in the PSP specific format,
		///                            false if the relocation is in standard ELF format. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void relocateFromBuffer(ByteBuffer f, pspsharp.HLE.kernel.types.SceModule module, int baseAddress, pspsharp.format.Elf32 elf, int RelCount, boolean pspRelocationFormat) throws java.io.IOException
		private void relocateFromBuffer(ByteBuffer f, SceModule module, int baseAddress, Elf32 elf, int RelCount, bool pspRelocationFormat)
		{

			Elf32Relocate rel = new Elf32Relocate();
			int AHL = 0; // (AHI << 16) | (ALO & 0xFFFF)
			IList<int> deferredHi16 = new LinkedList<int>(); // We'll use this to relocate R_MIPS_HI16 when we get a R_MIPS_LO16

			Memory mem = Memory.Instance;
			for (int i = 0; i < RelCount; i++)
			{
				rel.read(f);

				int phOffset;
				int phBaseOffset;

				int R_OFFSET = rel.R_offset;
				int R_TYPE = rel.R_info & 0xFF;
				if (pspRelocationFormat)
				{
					int OFS_BASE = (rel.R_info >> 8) & 0xFF;
					int ADDR_BASE = (rel.R_info >> 16) & 0xFF;
					if (log.TraceEnabled)
					{
						log.trace(string.Format("Relocation #{0:D} type={1:D}, Offset PH#{2:D}, Base Offset PH#{3:D}, Offset 0x{4:X8}", i, R_TYPE, OFS_BASE, ADDR_BASE, R_OFFSET));
					}

					phOffset = elf.getProgramHeader(OFS_BASE).P_vaddr;
					phBaseOffset = elf.getProgramHeader(ADDR_BASE).P_vaddr;
				}
				else
				{
					phOffset = 0;
					phBaseOffset = 0;
					if (log.TraceEnabled)
					{
						log.trace(string.Format("Relocation #{0:D} type={1:D}, Symbol 0x{2:X6}, Offset 0x{3:X8}", i, R_TYPE, rel.R_info >> 8, R_OFFSET));
					}
				}

				// Address of data to relocate
				int data_addr = baseAddress + R_OFFSET + phOffset;
				// Value of data to relocate
				int data = readUnaligned32(mem, data_addr);
				long result = 0; // Used to hold the result of relocation, OR this back into data

				int word32 = data & unchecked((int)0xFFFFFFFF); // <=> data;
				int targ26 = data & 0x03FFFFFF;
				int hi16 = data & 0x0000FFFF;
				int lo16 = data & 0x0000FFFF;

				int A = 0; // addend
				int S = baseAddress + phBaseOffset;

				switch (R_TYPE)
				{
					case 0: //R_MIPS_NONE
						// Tested on PSP: R_MIPS_NONE is ignored
						if (log.TraceEnabled)
						{
							log.trace(string.Format("R_MIPS_NONE addr=0x{0:X8}", data_addr));
						}
						break;

					case 1: // R_MIPS_16
						data = (data & unchecked((int)0xFFFF0000)) | ((data + S) & 0x0000FFFF);
						if (log.TraceEnabled)
						{
							log.trace(string.Format("R_MIPS_16 addr=0x{0:X8} before=0x{1:X8} after=0x{2:X8}", data_addr, word32, data));
						}
						break;

					case 2: //R_MIPS_32
						data += S;
						if (log.TraceEnabled)
						{
							log.trace(string.Format("R_MIPS_32 addr=0x{0:X8} before=0x{1:X8} after=0x{2:X8}", data_addr, word32, data));
						}
						break;

					case 4: //R_MIPS_26
						A = targ26;
						result = ((A << 2) + S) >> 2;
						data &= ~0x03FFFFFF;
						data |= (int)(result & 0x03FFFFFF); // truncate
						if (log.TraceEnabled)
						{
							log.trace(string.Format("R_MIPS_26 addr=0x{0:X8} before=0x{1:X8} after=0x{2:X8}", data_addr, word32, data));
						}
						break;

					case 5: //R_MIPS_HI16
						A = hi16;
						AHL = A << 16;
						deferredHi16.Add(data_addr);
						if (log.TraceEnabled)
						{
							log.trace(string.Format("R_MIPS_HI16 addr=0x{0:X8}", data_addr));
						}
						break;

					case 6: //R_MIPS_LO16
						A = lo16;
						AHL &= ~0x0000FFFF; // delete lower bits, since many R_MIPS_LO16 can follow one R_MIPS_HI16
						AHL |= A & 0x0000FFFF;
						result = AHL + S;
						data &= ~0x0000FFFF;
						data |= (int)(result & 0x0000FFFF); // truncate
						// Process deferred R_MIPS_HI16
						for (IEnumerator<int> it = deferredHi16.GetEnumerator(); it.MoveNext();)
						{
							int data_addr2 = it.Current;
							int data2 = readUnaligned32(mem, data_addr2);
							result = ((data2 & 0x0000FFFF) << 16) + A + S;
							// The low order 16 bits are always treated as a signed
							// value. Therefore, a negative value in the low order bits
							// requires an adjustment in the high order bits. We need
							// to make this adjustment in two ways: once for the bits we
							// took from the data, and once for the bits we are putting
							// back in to the data.
							if ((A & 0x8000) != 0)
							{
								result -= 0x10000;
							}
							if ((result & 0x8000) != 0)
							{
								 result += 0x10000;
							}
							data2 &= ~0x0000FFFF;
							data2 |= (int)((result >> 16) & 0x0000FFFF);
							if (log.TraceEnabled)
							{
								log.trace(string.Format("R_MIPS_HILO16 addr=0x{0:X8} before=0x{1:X8} after=0x{2:X8}", data_addr2, readUnaligned32(mem, data_addr2), data2));
							}
							writeUnaligned32(mem, data_addr2, data2);
							it.remove();
						}
						if (log.TraceEnabled)
						{
							log.trace(string.Format("R_MIPS_LO16 addr=0x{0:X8} before=0x{1:X8} after=0x{2:X8}", data_addr, word32, data));
						}
						break;

					case 7: // R_MIPS_GPREL16
						// This relocation type is ignored by the PSP
						if (log.TraceEnabled)
						{
							log.trace(string.Format("R_MIPS_GPREL16 addr=0x{0:X8} before=0x{1:X8} after=0x{2:X8}", data_addr, word32, data));
						}
						break;

					default:
						Console.WriteLine(string.Format("Unhandled relocation type {0:D} at 0x{1:X8}", R_TYPE, data_addr));
						break;
				}

				writeUnaligned32(mem, data_addr, data);
			}
		}

		private static string getRTypeName(int R_TYPE)
		{
			string[] names = new string[] {"R_MIPS_NONE", "R_MIPS_16", "R_MIPS_32", "R_MIPS_26", "R_MIPS_HI16", "R_MIPS_LO16", "R_MIPS_J26", "R_MIPS_JAL26"};
			if (R_TYPE < 0 || R_TYPE >= names.Length)
			{
				return string.Format("{0:D}", R_TYPE);
			}
			return names[R_TYPE];
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void relocateFromBufferA1(ByteBuffer f, pspsharp.HLE.kernel.types.SceModule module, pspsharp.format.Elf32 elf, int baseAddress, int programHeaderNumber, int size) throws java.io.IOException
		private void relocateFromBufferA1(ByteBuffer f, SceModule module, Elf32 elf, int baseAddress, int programHeaderNumber, int size)
		{
			Memory mem = Memory.Instance;

			// Relocation variables.
			int R_OFFSET = 0;
			int R_BASE = 0;
			int OFS_BASE = 0;

			// Data variables.
			int data_addr;
			int data;
			int lo16 = 0;
			int hi16;
			int phBaseOffset;
			int r = 0;

			// Buffer position variable.
			int pos = f.position();
			int end = pos + size;

			// Locate and read the flag, type and segment bits.
			f.position(pos + 2);
			int fbits = f.get();
			int flagShift = 0;
			int flagMask = (1 << fbits) - 1;

			int sbits = programHeaderNumber < 3 ? 1 : 2;
			int segmentShift = fbits;
			int segmentMask = (1 << sbits) - 1;

			int tbits = f.get();
			int typeShift = fbits + sbits;
			int typeMask = (1 << tbits) - 1;

			// Locate the flag table.
			int[] flags = new int[f.get() & 0xFF];
			flags[0] = flags.Length;
			for (int j = 1; j < flags.Length; j++)
			{
				flags[j] = f.get() & 0xFF;
				if (log.TraceEnabled)
				{
					log.trace(string.Format("R_FLAG({0:D} bits) 0x{1:X} -> 0x{2:X}", fbits, j, flags[j]));
				}
			}

			// Locate the type table.
			int[] types = new int[f.get() & 0xFF];
			types[0] = types.Length;
			for (int j = 1; j < types.Length; j++)
			{
				types[j] = f.get() & 0xFF;
				if (log.TraceEnabled)
				{
					log.trace(string.Format("R_TYPE({0:D} bits) 0x{1:X} -> 0x{2:X}", tbits, j, types[j]));
				}
			}

			// loadcore.prx and sysmem.prx are being loaded and relocated by
			// the PSP reboot code. It is using a different type mapping.
			// See https://github.com/uofw/uofw/blob/master/src/reboot/elf.c#L327
			if ("flash0:/kd/loadcore.prx".Equals(module.pspfilename) || "flash0:/kd/sysmem.prx".Equals(module.pspfilename))
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[] rebootTypeRemapping = new int[] { 0, 3, 6, 7, 1, 2, 4, 5 };
				int[] rebootTypeRemapping = new int[] {0, 3, 6, 7, 1, 2, 4, 5};
				for (int i = 1; i < types.Length; i++)
				{
					types[i] = rebootTypeRemapping[types[i]];
				}
			}

			// Save the current position.
			pos = f.position();

			int R_TYPE_OLD = types.Length;

			while (pos < end)
			{
				// Read the CMD byte.
				int R_CMD = (f.get() & 0xFF) | ((f.get() & 0xFF) << 8);
				pos += 2;

				// Process the relocation flag.
				int flagIndex = (R_CMD >> flagShift) & flagMask;
				int R_FLAG = flags[flagIndex];

				// Set the segment offset.
				int S = (R_CMD >> segmentShift) & segmentMask;

				// Process the relocation type.
				int typeIndex = (R_CMD >> typeShift) & typeMask;
				int R_TYPE = types[typeIndex];

				// Operate on segment offset based on the relocation flag.
				if ((R_FLAG & 0x01) == 0)
				{
					if (log.TraceEnabled)
					{
						log.trace(string.Format("Relocation 0x{0:X4}, R_FLAG=0x{1:X2}({2:D}), S={3:D}, rest=0x{4:X}", R_CMD, R_FLAG, flagIndex, S, R_CMD >> (fbits + sbits)));
					}

					OFS_BASE = S;
					if ((R_FLAG & 0x06) == 0)
					{
						R_BASE = (R_CMD >> (fbits + sbits));
					}
					else if ((R_FLAG & 0x06) == 4)
					{
						R_BASE = (f.get() & 0xFF);
						R_BASE |= ((f.get() & 0xFF) << 8);
						R_BASE |= ((f.get() & 0xFF) << 16);
						R_BASE |= ((f.get() & 0xFF) << 24);
						pos += 4;
					}
					else
					{
						Console.WriteLine("PH Relocation type 0x700000A1: Invalid size flag!");
						R_BASE = 0;
					}
				}
				else
				{ // Operate on segment address based on the relocation flag.
					if (log.TraceEnabled)
					{
						log.trace(string.Format("Relocation 0x{0:X4}, R_FLAG=0x{1:X2}({2:D}), S={3:D}, {4}({5:D}), rest=0x{6:X}", R_CMD, R_FLAG, flagIndex, S, getRTypeName(R_TYPE), typeIndex, R_CMD >> (fbits + tbits + sbits)));
					}

					int ADDR_BASE = S;
					phBaseOffset = baseAddress + elf.getProgramHeader(ADDR_BASE).P_vaddr;

					if ((R_FLAG & 0x06) == 0x00)
					{
						R_OFFSET = (int)(short) R_CMD; // sign-extend 16 to 32 bits
						R_OFFSET >>= (fbits + tbits + sbits);
						R_BASE += R_OFFSET;
					}
					else if ((R_FLAG & 0x06) == 0x02)
					{
						R_OFFSET = (R_CMD << 16) >> (fbits + tbits + sbits);
						R_OFFSET &= unchecked((int)0xFFFF0000);
						R_OFFSET |= (f.get() & 0xFF);
						R_OFFSET |= ((f.get() & 0xFF) << 8);
						pos += 2;
						R_BASE += R_OFFSET;
					}
					else if ((R_FLAG & 0x06) == 0x04)
					{
						R_BASE = (f.get() & 0xFF);
						R_BASE |= ((f.get() & 0xFF) << 8);
						R_BASE |= ((f.get() & 0xFF) << 16);
						R_BASE |= ((f.get() & 0xFF) << 24);
						pos += 4;
					}
					else
					{
						Console.WriteLine("PH Relocation type 0x700000A1: Invalid relocation size flag!");
					}

					// Process lo16.
					if ((R_FLAG & 0x38) == 0x00)
					{
						lo16 = 0;
					}
					else if ((R_FLAG & 0x38) == 0x08)
					{
						if ((R_TYPE_OLD ^ 0x04) != 0x00)
						{
							lo16 = 0;
						}
					}
					else if ((R_FLAG & 0x38) == 0x10)
					{
						lo16 = (f.get() & 0xFF);
						lo16 |= ((f.get() & 0xFF) << 8);
						lo16 = (int)(short) lo16; // sign-extend 16 to 32 bits
						pos += 2;
					}
					else if ((R_FLAG & 0x38) == 0x18)
					{
						Console.WriteLine("PH Relocation type 0x700000A1: Invalid lo16 setup!");
					}
					else
					{
						Console.WriteLine("PH Relocation type 0x700000A1: Invalid lo16 setup!");
					}

					// Read the data.
					data_addr = R_BASE + baseAddress + elf.getProgramHeader(OFS_BASE).P_vaddr;
					data = readUnaligned32(mem, data_addr);

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Relocation #{0:D} type={1:D}, Offset PH#{2:D}, Base Offset PH#{3:D}, Offset 0x{4:X8}", r, R_TYPE, OFS_BASE, ADDR_BASE, R_OFFSET));
					}

					int previousData = data;
					// Apply the changes as requested by the relocation type.
					switch (R_TYPE)
					{
						case 0: // R_MIPS_NONE
							break;
						case 2: // R_MIPS_32
							data += phBaseOffset;
							break;
						case 3: // R_MIPS_26
							data = (data & unchecked((int)0xFC000000)) | (((data & 0x03FFFFFF) + ((int)((uint)phBaseOffset >> 2))) & 0x03FFFFFF);
							break;
						case 6: // R_MIPS_J26
							data = (Opcodes.J << 26) | (((data & 0x03FFFFFF) + ((int)((uint)phBaseOffset >> 2))) & 0x03FFFFFF);
							break;
						case 7: // R_MIPS_JAL26
							data = (Opcodes.JAL << 26) | (((data & 0x03FFFFFF) + ((int)((uint)phBaseOffset >> 2))) & 0x03FFFFFF);
							break;
						case 4: // R_MIPS_HI16
							hi16 = ((data << 16) + lo16) + phBaseOffset;
							if ((hi16 & 0x8000) == 0x8000)
							{
								hi16 += 0x00010000;
							}
							data = (data & unchecked((int)0xffff0000)) | ((int)((uint)hi16 >> 16));
							break;
						case 1: // R_MIPS_16
						case 5: // R_MIPS_LO16
							data = (data & unchecked((int)0xffff0000)) | ((((int)(short) data) + phBaseOffset) & 0xffff);
							break;
						default:
							break;
					}

					if (previousData != data)
					{
						// Write the data.
						writeUnaligned32(mem, data_addr, data);
						if (log.TraceEnabled)
						{
							log.trace(string.Format("Relocation at 0x{0:X8}: 0x{1:X8} -> 0x{2:X8}", data_addr, previousData, data));
						}
					}
					r++;

					R_TYPE_OLD = R_TYPE;
				}
			}
		}

		private bool mustRelocate(Elf32 elf, Elf32SectionHeader shdr)
		{
			if (shdr.Sh_type == Elf32SectionHeader.SHT_PRXREL)
			{
				// PSP PRX relocation section
				return true;
			}

			if (shdr.Sh_type == Elf32SectionHeader.SHT_REL)
			{
				// Standard ELF relocation section
				Elf32SectionHeader relatedShdr = elf.getSectionHeader(shdr.Sh_info);
				// No relocation required for a debug section (sh_flags==SHF_NONE)
				if (relatedShdr != null && relatedShdr.Sh_flags != Elf32SectionHeader.SHF_NONE)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Uses info from the elf program headers and elf section headers to
		/// relocate a PRX. 
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void relocateFromHeaders(ByteBuffer f, pspsharp.HLE.kernel.types.SceModule module, int baseAddress, pspsharp.format.Elf32 elf, int elfOffset) throws java.io.IOException
		private void relocateFromHeaders(ByteBuffer f, SceModule module, int baseAddress, Elf32 elf, int elfOffset)
		{

			// Relocate from program headers
			int i = 0;
			foreach (Elf32ProgramHeader phdr in elf.ProgramHeaderList)
			{
				if (phdr.P_type == 0x700000A0L)
				{
					int RelCount = phdr.P_filesz / Elf32Relocate.@sizeof();
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("PH#{0:D}: relocating {1:D} entries", i, RelCount));
					}

					f.position(elfOffset + phdr.P_offset);
					relocateFromBuffer(f, module, baseAddress, elf, RelCount, true);
					return;
				}
				else if (phdr.P_type == 0x700000A1L)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Type 0x700000A1 PH#{0:D}: relocating A1 entries, size=0x{1:X}", i, phdr.P_filesz));
					}
					f.position(elfOffset + phdr.P_offset);
					relocateFromBufferA1(f, module, elf, baseAddress, i, phdr.P_filesz);
					return;
				}
				i++;
			}

			// Relocate from section headers
			foreach (Elf32SectionHeader shdr in elf.SectionHeaderList)
			{
				if (mustRelocate(elf, shdr))
				{
					int RelCount = shdr.Sh_size / Elf32Relocate.@sizeof();
					//if (log.DebugEnabled)
					{
						Console.WriteLine(shdr.Sh_namez + ": relocating " + RelCount + " entries");
					}

					f.position(elfOffset + shdr.Sh_offset);
					relocateFromBuffer(f, module, baseAddress, elf, RelCount, shdr.Sh_type != Elf32SectionHeader.SHT_REL);
				}
			}
		}

		private void ProcessUnresolvedImports(SceModule sourceModule, bool fromSyscall)
		{
			Memory mem = Memory.Instance;
			NIDMapper nidMapper = NIDMapper.Instance;
			int numberoffailedNIDS = 0;
			int numberofmappedNIDS = 0;

			foreach (SceModule module in Managers.modules.values())
			{
				module.importFixupAttempts++;
				for (IEnumerator<DeferredStub> it = module.unresolvedImports.GetEnumerator(); it.MoveNext();)
				{
					DeferredStub deferredStub = it.Current;
					string moduleName = deferredStub.ModuleName;
					int nid = deferredStub.Nid;
					int importAddress = deferredStub.ImportAddress;

					// Attempt to fixup stub to point to an already loaded module export
					int exportAddress = nidMapper.getAddressByNid(nid, moduleName);
					if (exportAddress != 0)
					{
						deferredStub.resolve(mem, exportAddress);
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						it.remove();
						sourceModule.resolvedImports.Add(deferredStub);
						numberofmappedNIDS++;

						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Mapped import at 0x{0:X8} to export at 0x{1:X8} [0x{2:X8}] (attempt {3:D})", importAddress, exportAddress, nid, module.importFixupAttempts));
						}
					}
					else if (nid == 0)
					{
						// Ignore patched nids
						Console.WriteLine(string.Format("Ignoring import at 0x{0:X8} [0x{1:X8}] (attempt {2:D})", importAddress, nid, module.importFixupAttempts));

						it.remove();
						// This is an import to be ignored, implement it with the following
						// code sequence:
						//    jr $ra          (already written to memory)
						//    li $v0, 0
						// Rem.: "BUST A MOVE GHOST" is testing the return value $v0,
						//       so it has to be set explicitly to 0.
						mem.write32(importAddress + 4, AllegrexOpcodes.ADDU | (2 << 11) | (0 << 16) | (0 << 21)); // addu $v0, $zr, $zr <=> li $v0, 0
					}
					else
					{
						// Attempt to fixup stub to known syscalls
						int code = nidMapper.getSyscallByNid(nid, moduleName);
						if (code >= 0)
						{
							// Fixup stub, replacing nop with syscall
							int returnInstruction = (AllegrexOpcodes.SPECIAL << 26) | AllegrexOpcodes.JR | ((Common._ra) << 21);
							int syscallInstruction = (AllegrexOpcodes.SPECIAL << 26) | AllegrexOpcodes.SYSCALL | ((code & 0x000fffff) << 6);

							// Some homebrews do not have a "jr $ra" set before the syscall
							if (mem.read32(importAddress) == 0)
							{
								mem.write32(importAddress, returnInstruction);
							}
							mem.write32(importAddress + 4, syscallInstruction);
							it.remove();
							numberofmappedNIDS++;

							if (fromSyscall && log.DebugEnabled)
							{
								Console.WriteLine(string.Format("Mapped import at 0x{0:X8} to syscall 0x{1:X5} [0x{2:X8}] (attempt {3:D})", importAddress, code, nid, module.importFixupAttempts));
							}
						}
						else
						{
							Console.WriteLine(string.Format("Failed to map import at 0x{0:X8} [0x{1:X8}] Module '{2}'(attempt {3:D})", importAddress, nid, moduleName, module.importFixupAttempts));
							numberoffailedNIDS++;
						}
					}
				}
			}

			log.info(numberofmappedNIDS + " NIDS mapped");
			if (numberoffailedNIDS > 0)
			{
				log.info(numberoffailedNIDS + " remaining unmapped NIDS");
			}
		}

		/* Loads from memory */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void LoadELFImports(pspsharp.HLE.kernel.types.SceModule module) throws java.io.IOException
		private void LoadELFImports(SceModule module)
		{
			Memory mem = Memory.Instance;
			int stubHeadersAddress = module.stub_top;
			int stubHeadersEndAddress = module.stub_top + module.stub_size;

			// n modules to import, 1 stub header per module to import.
			string moduleName;
			for (int i = 0; stubHeadersAddress < stubHeadersEndAddress; i++)
			{
				Elf32StubHeader stubHeader = new Elf32StubHeader(mem, stubHeadersAddress);

				// Skip 0 sized entries.
				if (stubHeader.Size <= 0)
				{
					Console.WriteLine("Skipping dummy entry with size " + stubHeader.Size);
					stubHeadersAddress += Elf32StubHeader.@sizeof() / 2;
				}
				else
				{
					if (Memory.isAddressGood((int)stubHeader.OffsetModuleName))
					{
						moduleName = Utilities.readStringNZ((int) stubHeader.OffsetModuleName, 64);
					}
					else
					{
						// Generate a module name.
						moduleName = module.modname;
					}
					stubHeader.ModuleNamez = moduleName;

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Processing Import #{0:D}: {1}", i, stubHeader.ToString()));
					}

					if (stubHeader.hasVStub())
					{
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("'{0}': Header with VStub has size {1:D}: {2}", stubHeader.ModuleNamez, stubHeader.Size, Utilities.getMemoryDump(stubHeadersAddress, stubHeader.Size * 4, 4, 16)));
						}
						int vStub = (int) stubHeader.VStub;
						if (vStub != 0)
						{
							int vStubSize = stubHeader.VStubSize;
							//if (log.DebugEnabled)
							{
								Console.WriteLine(string.Format("VStub has size {0:D}: {1}", vStubSize, Utilities.getMemoryDump(vStub, vStubSize * 8, 4, 16)));
							}
							IMemoryReader vstubReader = MemoryReader.getMemoryReader(vStub, vStubSize * 8, 4);
							for (int j = 0; j < vStubSize; j++)
							{
								int relocAddr = vstubReader.readNext();
								int nid = vstubReader.readNext();
								// relocAddr points to a list of relocation terminated by a 0
								IMemoryReader relocReader = MemoryReader.getMemoryReader(relocAddr, 4);
								while (true)
								{
									int reloc = relocReader.readNext();
									if (reloc == 0)
									{
										// End of relocation list
										break;
									}
									int opcode = (int)((uint)reloc >> 26);
									int address = (reloc & 0x03FFFFFF) << 2;
									DeferredStub deferredStub = null;
									switch (opcode)
									{
										case AllegrexOpcodes.BNE:
											deferredStub = new DeferredVStubHI16(module, stubHeader.ModuleNamez, address, nid);
											break;
										case AllegrexOpcodes.BLEZ:
											deferredStub = new DeferredVstubLO16(module, stubHeader.ModuleNamez, address, nid);
											break;
										case AllegrexOpcodes.J:
											deferredStub = new DeferredVStub32(module, stubHeader.ModuleNamez, address, nid);
											break;
										default:
											Console.WriteLine(string.Format("Unknown Vstub relocation nid 0x{0:X8}, reloc=0x{1:X8}", nid, reloc));
											break;
									}

									if (deferredStub != null)
									{
										//if (log.DebugEnabled)
										{
											Console.WriteLine(string.Format("Vstub reloc {0}", deferredStub));
										}
										module.unresolvedImports.Add(deferredStub);
									}
								}
							}
						}
					}
					stubHeadersAddress += stubHeader.Size * 4;

					if (!Memory.isAddressGood((int) stubHeader.OffsetNid) || !Memory.isAddressGood((int) stubHeader.OffsetText))
					{
						Console.WriteLine(string.Format("Incorrect s_nid or s_text address in StubHeader #{0:D}: {1}", i, stubHeader.ToString()));
					}
					else
					{
						// n stubs per module to import
						IMemoryReader nidReader = MemoryReader.getMemoryReader((int) stubHeader.OffsetNid, stubHeader.Imports * 4, 4);
						for (int j = 0; j < stubHeader.Imports; j++)
						{
							int nid = nidReader.readNext();
							int importAddress = (int)(stubHeader.OffsetText + j * 8);
							DeferredStub deferredStub = new DeferredStub(module, stubHeader.ModuleNamez, importAddress, nid);

							// Add a 0xfffff syscall so we can detect if an unresolved import is called
							deferredStub.unresolve(mem);
						}
					}
				}
			}

			if (module.unresolvedImports.Count > 0)
			{
				if (log.InfoEnabled)
				{
					log.info(string.Format("Found {0:D} unresolved imports", module.unresolvedImports.Count));
				}
			}
		}

		/* Loads from memory */
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void LoadELFExports(pspsharp.HLE.kernel.types.SceModule module) throws java.io.IOException
		private void LoadELFExports(SceModule module)
		{
			NIDMapper nidMapper = NIDMapper.Instance;
			Memory mem = Memory.Instance;
			int entHeadersAddress = module.ent_top;
			int entHeadersEndAddress = module.ent_top + module.ent_size;
			int entCount = 0;

			// n modules to export, 1 ent header per module to export.
			string moduleName;
			for (int i = 0; entHeadersAddress < entHeadersEndAddress; i++)
			{
				Elf32EntHeader entHeader = new Elf32EntHeader(mem, entHeadersAddress);

				if ((entHeader.Size <= 0))
				{
					// Skip 0 sized entries.
					Console.WriteLine("Skipping dummy entry with size " + entHeader.Size);
					entHeadersAddress += Elf32EntHeader.@sizeof() / 2;
				}
				else
				{
					if (Memory.isAddressGood((int)entHeader.OffsetModuleName))
					{
						moduleName = Utilities.readStringNZ((int) entHeader.OffsetModuleName, 64);
					}
					else
					{
						// Generate a module name.
						moduleName = module.modname;
					}
					entHeader.ModuleNamez = moduleName;

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Processing header #{0:D} at 0x{1:X8}: {2}", i, entHeadersAddress, entHeader.ToString()));
					}

					if (entHeader.Size > 4)
					{
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("'{0}': Header has size {1:D}: {2}", entHeader.ModuleNamez, entHeader.Size, Utilities.getMemoryDump(entHeadersAddress, entHeader.Size * 4, 4, 16)));
						}
						entHeadersAddress += entHeader.Size * 4;
					}
					else
					{
						entHeadersAddress += Elf32EntHeader.@sizeof();
					}

					// The export section is organized as as sequence of:
					// - 32-bit NID * functionCount
					// - 32-bit NID * variableCount
					// - 32-bit export address * functionCount
					// - 32-bit variable address * variableCount
					//   (each variable address references another structure, depending on its NID)
					//
					int functionCount = entHeader.FunctionCount;
					int variableCount = entHeader.VariableCount;
					int nidAddr = (int) entHeader.OffsetResident;
					IMemoryReader nidReader = MemoryReader.getMemoryReader(nidAddr, 4);
					int exportAddr = nidAddr + (functionCount + variableCount) * 4;
					IMemoryReader exportReader = MemoryReader.getMemoryReader(exportAddr, 4);
					if ((entHeader.Attr & 0x8000) == 0)
					{
						for (int j = 0; j < functionCount; j++)
						{
							int nid = nidReader.readNext();
							int exportAddress = exportReader.readNext();
							// Only accept exports with valid export addresses and
							// from custom modules (attr != 0x4000) unless
							// the module is a homebrew (loaded from MemoryStick) or
							// this is the EBOOT module.
							if (Memory.isAddressGood(exportAddress) && ((entHeader.Attr & 0x4000) != 0x4000) || module.pspfilename.StartsWith("ms0:", StringComparison.Ordinal) || module.pspfilename.StartsWith("disc0:/PSP_GAME/SYSDIR/EBOOT.", StringComparison.Ordinal) || module.pspfilename.StartsWith("flash0:", StringComparison.Ordinal))
							{
								nidMapper.addModuleNid(module, moduleName, nid, exportAddress, false);
								entCount++;
								//if (log.DebugEnabled)
								{
									Console.WriteLine(string.Format("Export found at 0x{0:X8} [0x{1:X8}]", exportAddress, nid));
								}
							}
						}
					}
					else
					{
						for (int j = 0; j < functionCount; j++)
						{
							int nid = nidReader.readNext();
							int exportAddress = exportReader.readNext();

							switch (nid)
							{
								case 0xD632ACDB: // module_start
									module.module_start_func = exportAddress;
									//if (log.DebugEnabled)
									{
										Console.WriteLine(string.Format("module_start found: nid=0x{0:X8}, function=0x{1:X8}", nid, exportAddress));
									}
									break;
								case 0xCEE8593C: // module_stop
									module.module_stop_func = exportAddress;
									//if (log.DebugEnabled)
									{
										Console.WriteLine(string.Format("module_stop found: nid=0x{0:X8}, function=0x{1:X8}", nid, exportAddress));
									}
									break;
								case 0x2F064FA6: // module_reboot_before
									module.module_reboot_before_func = exportAddress;
									//if (log.DebugEnabled)
									{
										Console.WriteLine(string.Format("module_reboot_before found: nid=0x{0:X8}, function=0x{1:X8}", nid, exportAddress));
									}
									break;
								case 0xADF12745: // module_reboot_phase
									module.module_reboot_phase_func = exportAddress;
									//if (log.DebugEnabled)
									{
										Console.WriteLine(string.Format("module_reboot_phase found: nid=0x{0:X8}, function=0x{1:X8}", nid, exportAddress));
									}
									break;
								case 0xD3744BE0: // module_bootstart
									module.module_bootstart_func = exportAddress;
									//if (log.DebugEnabled)
									{
										Console.WriteLine(string.Format("module_bootstart found: nid=0x{0:X8}, function=0x{1:X8}", nid, exportAddress));
									}
									break;
								default:
									// Only accept exports from custom modules (attr != 0x4000) and with valid export addresses.
									if (Memory.isAddressGood(exportAddress) && ((entHeader.Attr & 0x4000) != 0x4000))
									{
										nidMapper.addModuleNid(module, moduleName, nid, exportAddress, false);
										entCount++;
										//if (log.DebugEnabled)
										{
											Console.WriteLine(string.Format("Export found at 0x{0:X8} [0x{1:X8}]", exportAddress, nid));
										}
									}
									break;
							}
						}
					}

					int variableTableAddr = exportAddr + functionCount * 4;
					IMemoryReader variableReader = MemoryReader.getMemoryReader(variableTableAddr, 4);
					for (int j = 0; j < variableCount; j++)
					{
						int nid = nidReader.readNext();
						int variableAddr = variableReader.readNext();

						switch (nid)
						{
							case 0xF01D73A7: // module_info
								// Seems to be ignored by the PSP
								//if (log.DebugEnabled)
								{
									Console.WriteLine(string.Format("module_info found: nid=0x{0:X8}, addr=0x{1:X8}", nid, variableAddr));
								}
								break;
							case 0x0F7C276C: // module_start_thread_parameter
								module.module_start_thread_priority = mem.read32(variableAddr + 4);
								module.module_start_thread_stacksize = mem.read32(variableAddr + 8);
								module.module_start_thread_attr = mem.read32(variableAddr + 12);
								//if (log.DebugEnabled)
								{
									Console.WriteLine(string.Format("module_start_thread_parameter found: nid=0x{0:X8}, priority={1:D}, stacksize={2:D}, attr=0x{3:X8}", nid, module.module_start_thread_priority, module.module_start_thread_stacksize, module.module_start_thread_attr));
								}
								break;
							case 0xCF0CC697: // module_stop_thread_parameter
								module.module_stop_thread_priority = mem.read32(variableAddr + 4);
								module.module_stop_thread_stacksize = mem.read32(variableAddr + 8);
								module.module_stop_thread_attr = mem.read32(variableAddr + 12);
								//if (log.DebugEnabled)
								{
									Console.WriteLine(string.Format("module_stop_thread_parameter found: nid=0x{0:X8}, priority={1:D}, stacksize={2:D}, attr=0x{3:X8}", nid, module.module_stop_thread_priority, module.module_stop_thread_stacksize, module.module_stop_thread_attr));
								}
								break;
							case 0xF4F4299D: // module_reboot_before_thread_parameter
								module.module_reboot_before_thread_priority = mem.read32(variableAddr + 4);
								module.module_reboot_before_thread_stacksize = mem.read32(variableAddr + 8);
								module.module_reboot_before_thread_attr = mem.read32(variableAddr + 12);
								//if (log.DebugEnabled)
								{
									Console.WriteLine(string.Format("module_reboot_before_thread_parameter found: nid=0x{0:X8}, priority={1:D}, stacksize={2:D}, attr=0x{3:X8}", nid, module.module_reboot_before_thread_priority, module.module_reboot_before_thread_stacksize, module.module_reboot_before_thread_attr));
								}
								break;
							case 0x11B97506: // module_sdk_version
								// Currently ignored
								int sdk_version = mem.read32(variableAddr);
								//if (log.DebugEnabled)
								{
									Console.WriteLine(string.Format("module_sdk_version found: nid=0x{0:X8}, sdk_version=0x{1:X8}", nid, sdk_version));
								}
								break;
							default:
								// Only accept exports from custom modules (attr != 0x4000) and with valid export addresses.
								if (Memory.isAddressGood(variableAddr) && ((entHeader.Attr & 0x4000) != 0x4000))
								{
									nidMapper.addModuleNid(module, moduleName, nid, variableAddr, true);
									entCount++;
									//if (log.DebugEnabled)
									{
										Console.WriteLine(string.Format("Export found at 0x{0:X8} [0x{1:X8}]", variableAddr, nid));
									}
								}
								else
								{
									Console.WriteLine(string.Format("Unknown variable entry found: nid=0x{0:X8}, addr=0x{1:X8}", nid, variableAddr));
								}
								break;
						}
					}
				}
			}

			if (entCount > 0)
			{
				if (log.InfoEnabled)
				{
					log.info(string.Format("Found {0:D} exports", entCount));
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void LoadELFDebuggerInfo(ByteBuffer f, pspsharp.HLE.kernel.types.SceModule module, int baseAddress, pspsharp.format.Elf32 elf, int elfOffset, boolean fromSyscall) throws java.io.IOException
		private void LoadELFDebuggerInfo(ByteBuffer f, SceModule module, int baseAddress, Elf32 elf, int elfOffset, bool fromSyscall)
		{
			// Save executable section address/size for the debugger/instruction counter
			Elf32SectionHeader shdr;

			shdr = elf.getSectionHeader(".init");
			if (shdr != null)
			{
				module.initsection[0] = shdr.getSh_addr(baseAddress);
				module.initsection[1] = shdr.Sh_size;
			}

			shdr = elf.getSectionHeader(".fini");
			if (shdr != null)
			{
				module.finisection[0] = shdr.getSh_addr(baseAddress);
				module.finisection[1] = shdr.Sh_size;
			}

			shdr = elf.getSectionHeader(".sceStub.text");
			if (shdr != null)
			{
				module.stubtextsection[0] = shdr.getSh_addr(baseAddress);
				module.stubtextsection[1] = shdr.Sh_size;
			}

			if (!fromSyscall)
			{
				ElfHeaderInfo.ElfInfo = elf.ElfInfo;
				ElfHeaderInfo.ProgInfo = elf.ProgInfo;
				ElfHeaderInfo.SectInfo = elf.SectInfo;
			}
		}

		/// <summary>
		/// Apply patches to some VSH and Kernel modules
		/// </summary>
		/// <param name="module"> </param>
		private void patchModule(SceModule module)
		{
			Memory mem = Emulator.Memory;

			// Same patches as ProCFW
			if ("vsh_module".Equals(module.modname))
			{
				patch(mem, module, 0x000122B0, 0x506000E0, NOP());
				patch(mem, module, 0x00012058, 0x1440003B, NOP());
				patch(mem, module, 0x00012060, 0x14400039, NOP());
			}

			// Patches to replace "https" with "http" so that the URL calls
			// can be proxied through the internal HTTP server.
			if ("sceNpCommerce2".Equals(module.modname))
			{
				patch(mem, module, 0x0000A598, 0x00000073, 0x00000000); // replace "https" with "http"
				patch(mem, module, 0x00003A60, 0x240701BB, 0x24070050); // replace port 443 with 80
			}
			if ("sceNpCore".Equals(module.modname))
			{
				patchRemoveStringChar(mem, module, 0x00000D50, 's'); // replace "https" with "http" in "https://auth.%s.ac.playstation.net/nav/auth"
			}
			if ("sceNpService".Equals(module.modname))
			{
				patch(mem, module, 0x0001075C, 0x00000073, 0x00000000); // replace "https" with "http" for "https://getprof.%s.np.community.playstation.net/basic_view/sec/get_self_profile"
			}
			if ("sceVshNpInstaller_Module".Equals(module.modname))
			{
				patchRemoveStringChar(mem, module, 0x00016F90, 's'); // replace "https" with "http" in "https://commerce.%s.ac.playstation.net/cap.m"
				patchRemoveStringChar(mem, module, 0x00016FC0, 's'); // replace "https" with "http" in "https://commerce.%s.ac.playstation.net/cdp.m"
				patchRemoveStringChar(mem, module, 0x00016FF0, 's'); // replace "https" with "http" in "https://commerce.%s.ac.playstation.net/kdp.m"
				patchRemoveStringChar(mem, module, 0x00017020, 's'); // replace "https" with "http" in "https://account.%s.ac.playstation.net/ecomm/ingame/startDownloadDRM"
				patchRemoveStringChar(mem, module, 0x00017064, 's'); // replace "https" with "http" in "https://account.%s.ac.playstation.net/ecomm/ingame/finishDownloadDRM"
			}
			if ("marlindownloader".Equals(module.modname))
			{
				patchRemoveStringChar(mem, module, 0x000046C8, 's'); // replace "https" with "http" in "https://mds.%s.ac.playstation.net/"
			}
			if ("sceVshStoreBrowser_Module".Equals(module.modname))
			{
				patchRemoveStringChar(mem, module, 0x0005A244, 's'); // replace "https" with "http" in "https://nsx-e.sec.%s.dl.playstation.net/nsx/sec/..."
				patchRemoveStringChar(mem, module, 0x0005A2D8, 's'); // replace "https" with "http" in "https://nsx.sec.%s.dl.playstation.net/nsx/sec/..."
			}
			if ("sceGameUpdate_Library".Equals(module.modname))
			{
				patchRemoveStringChar(mem, module, 0x000030C4, 's'); // replace "https" with "http" in "https://a0.ww.%s.dl.playstation.net/tpl/..."
			}

			if ("sceMemlmd".Equals(module.modname))
			{
				patch(mem, module, 0x000017EC, 0x0E000000, MOVE(_v0, _zr), 0xFE000000); // replace "jal sceUtilsBufferCopy(cmd=15)" with "move $v0, $zr"
				SysMemUserForUser.SysMemInfo dummyArea = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "patch-sceMemlmd", SysMemUserForUser.PSP_SMEM_Low, 256, 0);
				patch(mem, module, 0x000024C0, 0xBFC00220, dummyArea.addr); // replace hardware register address with dummy address
				patch(mem, module, 0x000024D4, 0xBFC00280, dummyArea.addr); // replace hardware register address with dummy address
				patch(mem, module, 0x000024D8, 0xBFC00A00, dummyArea.addr); // replace hardware register address with dummy address
				patch(mem, module, 0x000024DC, 0xBFC00340, dummyArea.addr); // replace hardware register address with dummy address
				// Replace entry of sceUtilsBufferCopyWithRange with "return 0".
				patch(mem, module, 0x00001CFC, 0x27BDFFE0, ThreadManForUser.JR());
				patch(mem, module, 0x00001D00, 0x3C020000, MOVE(_v0, _zr), 0xFFFF0000);
				// Patch memlmd_9D36A439 with "return 1"
				patch(mem, module, 0x00001414, 0x0044102B, ADDIU(_v0, _zr, 1)); // replace "sltu $v0, $v0, $a0" with "li $v0, 1"
				// Patch memlmd_F26A33C3
				patch(mem, module, 0x000012D8, 0x90430000, ADDIU(_v1, _zr, 1)); // replace "lbu $v1, 0($v0)" with "li $v1, 1"
				patch(mem, module, 0x00001324, 0x90460000, ADDIU(_a2, _zr, 0)); // replace "lbu $a2, 0($v0)" with "li $a2, 0"
			}

			if ("sceModuleManager".Equals(module.modname))
			{
				patch(mem, module, 0x000030CC, 0x24030020, 0x24030010); // replace "li $v1, 32" with "li $v1, 16" (this will be stored at SceLoadCoreExecFileInfo.apiType)
			}

			if ("sceLoaderCore".Equals(module.modname))
			{
				patch(mem, module, 0x0000469C, 0x15C0FFA0, NOP()); // Allow loading of privileged modules being not encrypted (https://github.com/uofw/uofw/blob/master/src/loadcore/loadelf.c#L339)
				patch(mem, module, 0x00004548, 0x7C0F6244, NOP()); // Allow loading of privileged modules being not encrypted (take SceLoadCoreExecFileInfo.modInfoAttribute from the ELF module info, https://github.com/uofw/uofw/blob/master/src/loadcore/loadelf.c#L351)
				patch(mem, module, 0x00004550, 0x14E0002C, 0x1000002C); // Allow loading of privileged modules being not encrypted (https://github.com/uofw/uofw/blob/master/src/loadcore/loadelf.c#L352)
				patch(mem, module, 0x00003D58, 0x10C0FFBE, NOP()); // Allow linking user stub to kernel lib
			}
		}
	}
}