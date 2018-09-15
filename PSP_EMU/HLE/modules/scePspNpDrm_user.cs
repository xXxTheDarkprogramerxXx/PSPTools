using System;
using System.Text;

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
	using CryptoEngine = pspsharp.crypto.CryptoEngine;
	using SeekableDataInput = pspsharp.filesystems.SeekableDataInput;
	using SeekableRandomFile = pspsharp.filesystems.SeekableRandomFile;
	using Usage = pspsharp.HLE.BufferInfo.Usage;
	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using SeekableDataInputVirtualFile = pspsharp.HLE.VFS.SeekableDataInputVirtualFile;
	using EDATVirtualFile = pspsharp.HLE.VFS.crypto.EDATVirtualFile;
	using PGDVirtualFile = pspsharp.HLE.VFS.crypto.PGDVirtualFile;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelLMOption = pspsharp.HLE.kernel.types.SceKernelLMOption;
	using SceModule = pspsharp.HLE.kernel.types.SceModule;
	using IoInfo = pspsharp.HLE.modules.IoFileMgrForUser.IoInfo;
	using LoadModuleContext = pspsharp.HLE.modules.ModuleMgrForUser.LoadModuleContext;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;

	//using Logger = org.apache.log4j.Logger;

	public class scePspNpDrm_user : HLEModule
	{

		//public static Logger log = Modules.getLogger("scePspNpDrm_user");

		public override void start()
		{
			setSettingsListener("emu.disableDLC", new DisableDLCSettingsListerner(this));
			base.start();
		}

		public const int PSP_NPDRM_KEY_LENGHT = 0x10;
		private sbyte[] npDrmKey = new sbyte[PSP_NPDRM_KEY_LENGHT];
		private bool isNpDrmKeySet = false;
		private bool disableDLCDecryption;

		public virtual bool DisableDLCStatus
		{
			set
			{
				disableDLCDecryption = value;
			}
			get
			{
				return disableDLCDecryption;
			}
		}


		protected internal virtual bool NpDrmKeyStatus
		{
			set
			{
				isNpDrmKeySet = value;
			}
			get
			{
				return isNpDrmKeySet;
			}
		}


		private class DisableDLCSettingsListerner : AbstractBoolSettingsListener
		{
			private readonly scePspNpDrm_user outerInstance;

			public DisableDLCSettingsListerner(scePspNpDrm_user outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override void settingsValueChanged(bool value)
			{
				outerInstance.DisableDLCStatus = value;
			}
		}

		[HLEFunction(nid : 0xA1336091, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpDrmSetLicenseeKey(TPointer npDrmKeyAddr)
		{
			StringBuilder key = new StringBuilder();
			for (int i = 0; i < PSP_NPDRM_KEY_LENGHT; i++)
			{
				npDrmKey[i] = (sbyte) npDrmKeyAddr.getValue8(i);
				key.Append(string.Format("{0:X2}", npDrmKey[i] & 0xFF));
			}
			NpDrmKeyStatus = true;
			if (log.InfoEnabled)
			{
				log.info(string.Format("NPDRM Encryption key detected: 0x{0}", key.ToString()));
			}

			return 0;
		}

		[HLEFunction(nid : 0x9B745542, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpDrmClearLicenseeKey()
		{
			Arrays.Fill(npDrmKey, (sbyte) 0);
			NpDrmKeyStatus = false;

			return 0;
		}

		[HLEFunction(nid : 0x275987D1, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpDrmRenameCheck(PspString fileName)
		{
			CryptoEngine crypto = new CryptoEngine();
			int result = 0;

			if (!NpDrmKeyStatus)
			{
				result = SceKernelErrors.ERROR_NPDRM_NO_K_LICENSEE_SET;
			}
			else
			{
				try
				{
					string pcfilename = Modules.IoFileMgrForUserModule.getDeviceFilePath(fileName.String);
					SeekableRandomFile file = new SeekableRandomFile(pcfilename, "r");

					string[] name = pcfilename.Split("/", true);
					string fName = name[name.Length - 1];
					for (int i = 0; i < name.Length; i++)
					{
						if (name[i].ToUpper().Contains("EDAT"))
						{
							fName = name[i];
						}
					}

					// The file must contain a valid PSPEDAT header.
					if (file.Length() < 0x80)
					{
						// Test if we're using already decrypted DLC.
						// Discard the error in this situatuion.
						if (!DisableDLCStatus)
						{
							Console.WriteLine("sceNpDrmRenameCheck: invalid file size");
							result = SceKernelErrors.ERROR_NPDRM_INVALID_FILE;
						}
						file.Dispose();
					}
					else
					{
						// Setup the buffers.
						sbyte[] inBuf = new sbyte[0x80];
						sbyte[] srcData = new sbyte[0x30];
						sbyte[] srcHash = new sbyte[0x10];

						// Read the header.
						file.readFully(inBuf);
						file.Dispose();

						// The data seed is stored at offset 0x10 of the PSPEDAT header.
						Array.Copy(inBuf, 0x10, srcData, 0, 0x30);

						// The hash to compare is stored at offset 0x40 of the PSPEDAT header.
						Array.Copy(inBuf, 0x40, srcHash, 0, 0x10);

						// If the CryptoEngine fails to find a match, then the file has been renamed.
						if (!crypto.PGDEngine.CheckEDATRenameKey(fName.GetBytes(), srcHash, srcData))
						{
							if (!DisableDLCStatus)
							{
								result = SceKernelErrors.ERROR_NPDRM_NO_FILENAME_MATCH;
								Console.WriteLine("sceNpDrmRenameCheck: the file has been renamed");
							}
						}
					}
				}
				catch (FileNotFoundException e)
				{
					result = SceKernelErrors.ERROR_NPDRM_INVALID_FILE;
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("sceNpDrmRenameCheck: file '{0}' not found: {1}", fileName.String, e.ToString()));
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("sceNpDrmRenameCheck", e);
				}
			}

			return result;
		}

		[HLELogging(level : "info"), HLEFunction(nid : 0x08D98894, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpDrmEdataSetupKey(int edataFd)
		{
			// Return an error if the key has not been set.
			// Note: An empty key is valid, as long as it was set with sceNpDrmSetLicenseeKey.
			if (!NpDrmKeyStatus)
			{
				return SceKernelErrors.ERROR_NPDRM_NO_K_LICENSEE_SET;
			}

			IoInfo info = Modules.IoFileMgrForUserModule.getFileIoInfo(edataFd);
			if (info == null)
			{
				return -1;
			}

			int result = 0;

			// Check if the DLC decryption is enabled
			if (!DisableDLCStatus)
			{
				IVirtualFile vFile = info.vFile;
				if (vFile == null && info.readOnlyFile != null)
				{
					vFile = new SeekableDataInputVirtualFile(info.readOnlyFile);
				}
				PGDVirtualFile pgdFile = new EDATVirtualFile(vFile);
				if (pgdFile.Valid)
				{
					info.vFile = pgdFile;
				}
			}

			return result;
		}

		[HLEFunction(nid : 0x219EF5CC, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpDrmEdataGetDataSize(int edataFd)
		{
			IoInfo info = Modules.IoFileMgrForUserModule.getFileIoInfo(edataFd);
			int size = 0;
			if (info != null)
			{
				if (info.vFile != null)
				{
					size = (int) info.vFile.Length();
				}
				else if (info.readOnlyFile != null)
				{
					try
					{
						size = (int) info.readOnlyFile.Length();
					}
					catch (IOException e)
					{
						Console.WriteLine("sceNpDrmEdataGetDataSize", e);
					}
				}
			}

			return size;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2BAA4294, version = 150, checkInsideInterrupt = true) public int sceNpDrmOpen(pspsharp.HLE.PspString name, int flags, int permissions)
		[HLEFunction(nid : 0x2BAA4294, version : 150, checkInsideInterrupt : true)]
		public virtual int sceNpDrmOpen(PspString name, int flags, int permissions)
		{
			if (!NpDrmKeyStatus)
			{
				return SceKernelErrors.ERROR_NPDRM_NO_K_LICENSEE_SET;
			}
			// Open the file with flags ORed with PSP_O_FGAMEDATA and send it to the IoFileMgr.
			int fd = Modules.IoFileMgrForUserModule.hleIoOpen(name, flags | 0x40000000, permissions, true);
			return sceNpDrmEdataSetupKey(fd);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xC618D0B1, version = 150, checkInsideInterrupt = true) public int sceKernelLoadModuleNpDrm(pspsharp.HLE.PspString path, int flags, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0xC618D0B1, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelLoadModuleNpDrm(PspString path, int flags, TPointer optionAddr)
		{
			SceKernelLMOption lmOption = null;
			if (optionAddr.NotNull)
			{
				lmOption = new SceKernelLMOption();
				lmOption.read(optionAddr);
				if (log.InfoEnabled)
				{
					log.info(string.Format("sceKernelLoadModuleNpDrm options: {0}", lmOption));
				}
			}

			// SPRX modules can't be decrypted yet.
			if (!DisableDLCStatus)
			{
				Console.WriteLine(string.Format("sceKernelLoadModuleNpDrm detected encrypted DLC module: {0}", path.String));
				return SceKernelErrors.ERROR_NPDRM_INVALID_PERM;
			}

			LoadModuleContext loadModuleContext = new LoadModuleContext();
			loadModuleContext.fileName = path.String;
			loadModuleContext.flags = flags;
			loadModuleContext.lmOption = lmOption;
			loadModuleContext.needModuleInfo = true;
			loadModuleContext.allocMem = true;

			return Modules.ModuleMgrForUserModule.hleKernelLoadModule(loadModuleContext);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xAA5FC85B, version = 150, checkInsideInterrupt = true) public int sceKernelLoadExecNpDrm(pspsharp.HLE.PspString fileName, @CanBeNull pspsharp.HLE.TPointer optionAddr)
		[HLEFunction(nid : 0xAA5FC85B, version : 150, checkInsideInterrupt : true)]
		public virtual int sceKernelLoadExecNpDrm(PspString fileName, TPointer optionAddr)
		{
			// Flush system memory to mimic a real PSP reset.
			Modules.SysMemUserForUserModule.reset();

			if (optionAddr.NotNull)
			{
				int optSize = optionAddr.getValue32(0); // Size of the option struct.
				int argSize = optionAddr.getValue32(4); // Number of args (strings).
				int argAddr = optionAddr.getValue32(8); // Pointer to a list of strings.
				int keyAddr = optionAddr.getValue32(12); // Pointer to an encryption key (may not be used).

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("sceKernelLoadExecNpDrm (params: optSize={0:D}, argSize={1:D}, argAddr=0x{2:X8}, keyAddr=0x{3:X8})", optSize, argSize, argAddr, keyAddr));
				}
			}

			// SPRX modules can't be decrypted yet.
			if (!DisableDLCStatus)
			{
				Console.WriteLine(string.Format("sceKernelLoadModuleNpDrm detected encrypted DLC module: {0}", fileName.String));
				return SceKernelErrors.ERROR_NPDRM_INVALID_PERM;
			}

			int result;
			try
			{
				SeekableDataInput moduleInput = Modules.IoFileMgrForUserModule.getFile(fileName.String, IoFileMgrForUser.PSP_O_RDONLY);
				if (moduleInput != null)
				{
					sbyte[] moduleBytes = new sbyte[(int) moduleInput.Length()];
					moduleInput.readFully(moduleBytes);
					moduleInput.Dispose();
					ByteBuffer moduleBuffer = ByteBuffer.wrap(moduleBytes);

					SceModule module = Emulator.Instance.load(fileName.String, moduleBuffer, true);
					Emulator.Clock.resume();

					if ((module.fileFormat & Loader.FORMAT_ELF) == Loader.FORMAT_ELF)
					{
						result = 0;
					}
					else
					{
						Console.WriteLine("sceKernelLoadExecNpDrm - failed, target is not an ELF");
						result = SceKernelErrors.ERROR_KERNEL_ILLEGAL_LOADEXEC_FILENAME;
					}
				}
				else
				{
					result = SceKernelErrors.ERROR_KERNEL_PROHIBIT_LOADEXEC_DEVICE;
				}
			}
			catch (GeneralJpcspException e)
			{
				Console.WriteLine("sceKernelLoadExecNpDrm", e);
				result = SceKernelErrors.ERROR_KERNEL_PROHIBIT_LOADEXEC_DEVICE;
			}
			catch (IOException e)
			{
				Console.WriteLine(string.Format("sceKernelLoadExecNpDrm - Error while loading module '{0}'", fileName), e);
				result = SceKernelErrors.ERROR_KERNEL_PROHIBIT_LOADEXEC_DEVICE;
			}

			return result;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xEBB198ED, version = 150) public int sceNpDrmDecActivation(pspsharp.HLE.TPointer unknown1, pspsharp.HLE.TPointer unknown2)
		[HLEFunction(nid : 0xEBB198ED, version : 150)]
		public virtual int sceNpDrmDecActivation(TPointer unknown1, TPointer unknown2)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x17E3F4BB, version = 150) public int sceNpDrmVerifyAct(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=4152, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer actDatAddr)
		[HLEFunction(nid : 0x17E3F4BB, version : 150)]
		public virtual int sceNpDrmVerifyAct(TPointer actDatAddr)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x37B9B10D, version = 150) public int sceNpDrmVerifyRif(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=152, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer rifAddr)
		[HLEFunction(nid : 0x37B9B10D, version : 150)]
		public virtual int sceNpDrmVerifyRif(TPointer rifAddr)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9A34AC9F, version = 150) public int sceNpDrm_9A34AC9F(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=152, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer rifAddr)
		[HLEFunction(nid : 0x9A34AC9F, version : 150)]
		public virtual int sceNpDrm_9A34AC9F(TPointer rifAddr)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x0F9547E6, version = 150) public int sceNpDrmGetVersionKey(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=32, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer keyAddr, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=4152, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer actDatAddr, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=152, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer rifAddr, int type)
		[HLEFunction(nid : 0x0F9547E6, version : 150)]
		public virtual int sceNpDrmGetVersionKey(TPointer keyAddr, TPointer actDatAddr, TPointer rifAddr, int type)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x5667B7B9, version = 150) public int sceNpDrmGetContentKey(@BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=32, usage=pspsharp.HLE.BufferInfo.Usage.out) pspsharp.HLE.TPointer keyAddr, @CanBeNull @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=4152, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer actDatAddr, @BufferInfo(lengthInfo=pspsharp.HLE.BufferInfo.LengthInfo.fixedLength, Length=152, usage=pspsharp.HLE.BufferInfo.Usage.in) pspsharp.HLE.TPointer rifAddr)
		[HLEFunction(nid : 0x5667B7B9, version : 150)]
		public virtual int sceNpDrmGetContentKey(TPointer keyAddr, TPointer actDatAddr, TPointer rifAddr)
		{
			return sceNpDrmGetVersionKey(keyAddr, actDatAddr, rifAddr, 0);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x2D88879A, version = 150) public int sceNpDrm_2D88879A()
		[HLEFunction(nid : 0x2D88879A, version : 150)]
		public virtual int sceNpDrm_2D88879A()
		{
			return 0;
		}
	}

}