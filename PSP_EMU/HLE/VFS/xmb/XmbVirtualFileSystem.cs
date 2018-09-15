using System;
using System.Collections.Generic;
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
namespace pspsharp.HLE.VFS.xmb
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.MainGUI.getUmdPaths;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.merge;


	using UmdBrowser = pspsharp.GUI.UmdBrowser;
	using LocalVirtualFileSystem = pspsharp.HLE.VFS.local.LocalVirtualFileSystem;
	using SceIoDirent = pspsharp.HLE.kernel.types.SceIoDirent;
	using SceIoStat = pspsharp.HLE.kernel.types.SceIoStat;
	using IoFileMgrForUser = pspsharp.HLE.modules.IoFileMgrForUser;
	using IoOperation = pspsharp.HLE.modules.IoFileMgrForUser.IoOperation;
	using IoOperationTiming = pspsharp.HLE.modules.IoFileMgrForUser.IoOperationTiming;
	using Settings = pspsharp.settings.Settings;

	public class XmbVirtualFileSystem : AbstractVirtualFileSystem
	{
		public const string PSP_GAME = "PSP/GAME";
		private const string EBOOT_PBP = "EBOOT.PBP";
		private const string DOCUMENT_DAT = "DOCUMENT.DAT";
		private static readonly string ISO_DIR = Settings.Instance.getDirectoryMapping("ms0") + "ISO";
		private IVirtualFileSystem vfs;
		private File[] umdPaths;
		private IDictionary<string, IVirtualFileSystem> umdVfs;
		private IList<VirtualPBP> umdFiles;

		private class VirtualPBP
		{
			internal string umdFile;
			internal IVirtualFile vFile;
		}

		public XmbVirtualFileSystem(IVirtualFileSystem vfs)
		{
			this.vfs = vfs;

			umdPaths = getUmdPaths(true);
			File isoDir = new File(ISO_DIR);
			if (isoDir.Directory)
			{
				umdPaths = add(umdPaths, isoDir);
			}

			umdVfs = new Dictionary<string, IVirtualFileSystem>();
			for (int i = 0; i < umdPaths.Length; i++)
			{
				IVirtualFileSystem localVfs = new LocalVirtualFileSystem(umdPaths[i].AbsolutePath + "/", false);
				umdVfs[umdPaths[i].AbsolutePath] = localVfs;
			}

			umdFiles = new LinkedList<XmbVirtualFileSystem.VirtualPBP>();
		}

		private bool isVirtualFile(string name)
		{
			return EBOOT_PBP.Equals(name) || DOCUMENT_DAT.Equals(name);
		}

		private string[] addUmdFileNames(string dirName, File[] files)
		{
			if (files == null)
			{
				return null;
			}

			string[] fileNames = new string[files.Length];
			for (int i = 0; i < files.Length; i++)
			{
				VirtualPBP virtualPBP = new VirtualPBP();
				virtualPBP.umdFile = files[i].AbsolutePath;

				int umdIndex = umdFiles.Count;
				umdFiles.Add(virtualPBP);
				fileNames[i] = string.Format("@UMD{0:D}", umdIndex);

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("{0}={1}", fileNames[i], files[i].AbsolutePath));
				}
			}

			return fileNames;
		}

		private VirtualPBP getVirtualPBP(string fileName, StringBuilder restFileName)
		{
			if (!string.ReferenceEquals(fileName, null))
			{
				int umdMarkerIndex = fileName.IndexOf("@UMD", StringComparison.Ordinal);
				if (umdMarkerIndex >= 0)
				{
					string umdIndexString = fileName.Substring(umdMarkerIndex + 4);
					int sepIndex = umdIndexString.IndexOf("/", StringComparison.Ordinal);
					if (sepIndex >= 0)
					{
						if (restFileName != null)
						{
							restFileName.Append(umdIndexString.Substring(sepIndex + 1));
						}
						umdIndexString = umdIndexString.Substring(0, sepIndex);
					}

					int umdIndex = int.Parse(umdIndexString);
					if (umdIndex >= 0 && umdIndex < umdFiles.Count)
					{
						return umdFiles[umdIndex];
					}
				}
			}

			return null;
		}

		private string getUmdFileName(string fileName, StringBuilder restFileName)
		{
			VirtualPBP virtualPBP = getVirtualPBP(fileName, restFileName);
			if (virtualPBP == null)
			{
				return null;
			}

			return virtualPBP.umdFile;
		}

		private IVirtualFileSystem getUmdVfs(string umdFileName, StringBuilder localFileName)
		{
			foreach (string path in umdVfs.Keys)
			{
				if (umdFileName.StartsWith(path, StringComparison.Ordinal))
				{
					if (localFileName != null)
					{
						localFileName.Append(umdFileName.Substring(path.Length + 1));
					}
					return umdVfs[path];
				}
			}

			return null;
		}

		private int umdIoGetstat(string umdFileName, string restFileName, SceIoStat stat)
		{
			StringBuilder localFileName = new StringBuilder();
			IVirtualFileSystem vfs = getUmdVfs(umdFileName, localFileName);
			if (vfs != null)
			{
				int result = vfs.ioGetstat(localFileName.ToString(), stat);
				// If the UMD file is actually a directory
				// (e.g. containing the EBOOT.PBP),
				// then stat the real file (EBOOT.PBP or DOCUMENT.DAT).
				if (!string.ReferenceEquals(restFileName, null) && restFileName.Length > 0 && result == 0)
				{
					if ((stat.attr & 0x10) != 0)
					{
						result = vfs.ioGetstat(localFileName.ToString() + "/" + restFileName, stat);
					}
				}
				return result;
			}

			return IO_ERROR;
		}

		public override string[] ioDopen(string dirName)
		{
			string[] entries = null;

			StringBuilder restFileName = new StringBuilder();
			string umdFileName = getUmdFileName(dirName, restFileName);
			if (!string.ReferenceEquals(umdFileName, null) && restFileName.Length == 0)
			{
				entries = new string[] {EBOOT_PBP};
			}
			else if (PSP_GAME.Equals(dirName))
			{
				for (int i = 0; i < umdPaths.Length; i++)
				{
					File umdPath = umdPaths[i];
					if (umdPath.Directory)
					{
						File[] umdFiles = umdPath.listFiles(new UmdBrowser.UmdFileFilter());
						entries = merge(entries, addUmdFileNames(dirName, umdFiles));
					}
				}

				entries = merge(entries, vfs.ioDopen(dirName));
			}
			else
			{
				entries = vfs.ioDopen(dirName);
			}

			return entries;
		}

		public override int ioDread(string dirName, SceIoDirent dir)
		{
			StringBuilder restFileName = new StringBuilder();
			string umdFileName = getUmdFileName(dirName, restFileName);
			if (!string.ReferenceEquals(umdFileName, null) && restFileName.Length == 0 && EBOOT_PBP.Equals(dir.filename))
			{
				int result = umdIoGetstat(umdFileName, restFileName.ToString(), dir.stat);
				if (result < 0)
				{
					return result;
				}

				return 1;
			}

			restFileName = new StringBuilder();
			umdFileName = getUmdFileName(dir.filename, restFileName);
			if (!string.ReferenceEquals(umdFileName, null) && restFileName.Length == 0)
			{
				int result = umdIoGetstat(umdFileName, restFileName.ToString(), dir.stat);
				if (result < 0)
				{
					return result;
				}

				// Change attribute from "file" to "directory"
				dir.stat.attr = (dir.stat.attr & ~0x20) | 0x10;
				dir.stat.mode = (dir.stat.mode & ~0x2000) | 0x1000;

				return 1;
			}

			return vfs.ioDread(dirName, dir);
		}

		public override int ioGetstat(string fileName, SceIoStat stat)
		{
			StringBuilder restFileName = new StringBuilder();
			string umdFileName = getUmdFileName(fileName, restFileName);
			if (!string.ReferenceEquals(umdFileName, null) && isVirtualFile(restFileName.ToString()))
			{
				return umdIoGetstat(umdFileName, restFileName.ToString(), stat);
			}

			return vfs.ioGetstat(fileName, stat);
		}

		public override IVirtualFile ioOpen(string fileName, int flags, int mode)
		{
			StringBuilder restFileName = new StringBuilder();
			VirtualPBP virtualPBP = getVirtualPBP(fileName, restFileName);
			if (virtualPBP != null && isVirtualFile(restFileName.ToString()))
			{
				string umdFileName = virtualPBP.umdFile;
				File umdFile = new File(umdFileName);

				// Is it a directory containing an EBOOT.PBP file?
				if (umdFile.Directory)
				{
					StringBuilder localFileName = new StringBuilder();
					IVirtualFileSystem vfs = getUmdVfs(umdFileName, localFileName);
					if (vfs != null)
					{
						// Open the EBOOT.PBP file inside the directory
						return vfs.ioOpen(localFileName.ToString() + "/" + restFileName.ToString(), flags, mode);
					}
				}

				// Map the ISO/CSO file into a virtual PBP file
				if (virtualPBP.vFile == null)
				{
					virtualPBP.vFile = new XmbIsoVirtualFile(umdFileName);
				}
				if (virtualPBP.vFile.Length() > 0)
				{
					return virtualPBP.vFile;
				}

				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("XmbVirtualFileSystem.ioOpen could not open UMD file '{0}'", umdFileName));
				}
			}

			return vfs.ioOpen(fileName, flags, mode);
		}

		public override IDictionary<IoFileMgrForUser.IoOperation, IoFileMgrForUser.IoOperationTiming> Timings
		{
			get
			{
				// Do not delay IO operations on faked EBOOT.PBP files
				return IoFileMgrForUser.noDelayTimings;
			}
		}

		public override int ioRename(string oldFileName, string newFileName)
		{
			return vfs.ioRename(oldFileName, newFileName);
		}

		public override int ioChstat(string fileName, SceIoStat stat, int bits)
		{
			return vfs.ioChstat(fileName, stat, bits);
		}

		public override int ioRemove(string name)
		{
			return vfs.ioRemove(name);
		}

		public override int ioMkdir(string name, int mode)
		{
			return vfs.ioMkdir(name, mode);
		}

		public override int ioRmdir(string name)
		{
			return vfs.ioRmdir(name);
		}

		public override int ioChdir(string directoryName)
		{
			return vfs.ioChdir(directoryName);
		}

		public override int ioMount()
		{
			return vfs.ioMount();
		}

		public override int ioUmount()
		{
			return vfs.ioUmount();
		}

		public override int ioDevctl(string deviceName, int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			return vfs.ioDevctl(deviceName, command, inputPointer, inputLength, outputPointer, outputLength);
		}
	}

}