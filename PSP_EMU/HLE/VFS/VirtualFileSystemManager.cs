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
namespace pspsharp.HLE.VFS
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.xmb.XmbVirtualFileSystem.PSP_GAME;

	using LocalVirtualFileSystem = pspsharp.HLE.VFS.local.LocalVirtualFileSystem;
	using XmbVirtualFileSystem = pspsharp.HLE.VFS.xmb.XmbVirtualFileSystem;
	using Settings = pspsharp.settings.Settings;

	public class VirtualFileSystemManager
	{
		protected internal Dictionary<string, IVirtualFileSystem> virtualFileSystems = new Dictionary<string, IVirtualFileSystem>();
		private IVirtualFileSystem xmbVfs;

		public virtual void register(string name, IVirtualFileSystem vfs)
		{
			name = name.ToLower();
			virtualFileSystems[name] = vfs;
		}

		public virtual void unregister(string name)
		{
			name = name.ToLower();
			virtualFileSystems.Remove(name);
		}

		public virtual IVirtualFileSystem getVirtualFileSystem(string absoluteFileName, StringBuilder localFileName)
		{
			int colon = absoluteFileName.IndexOf(':');
			if (colon < 0)
			{
				return null;
			}

			string name = absoluteFileName.Substring(0, colon);
			name = name.ToLower();

			if (localFileName != null)
			{
				localFileName.Length = 0;
				localFileName.Append(absoluteFileName.Substring(colon + 1));

				normalizeLocalFileName(localFileName);

				if ("ms0".Equals(name) && localFileName.ToString().StartsWith(PSP_GAME, StringComparison.Ordinal))
				{
					if (xmbVfs == null)
					{
						xmbVfs = new XmbVirtualFileSystem(new LocalVirtualFileSystem(Settings.Instance.getDirectoryMapping("ms0"), true));
					}
					return xmbVfs;
				}
			}

			return virtualFileSystems[name];
		}

		/// <summary>
		/// Normalize the given local file name:
		/// - resolve ".." and "." special notation
		/// - remove leading and trailing "/"
		/// </summary>
		/// <param name="localFileName">   the local file name to be normalized </param>
		private void normalizeLocalFileName(StringBuilder localFileName)
		{
			// Remove "/../" in the local file name
			// E.g.:
			//      /PSP_GAME/USRDIR/A/../B
			// is transformed into
			//      /PSP_GAME/USRDIR/B
			while (true)
			{
				int dotDotIndex = localFileName.ToString().IndexOf("/../");
				if (dotDotIndex < 0)
				{
					break;
				}
				int parentIndex = localFileName.lastIndexOf("/", dotDotIndex - 1);
				if (parentIndex < 0)
				{
					break;
				}
				localFileName.Remove(parentIndex, dotDotIndex + 3 - parentIndex);
			}

			// Remove "/.." at the end of the local file name
			// E.g.:
			//      PSP_GAME/USRDIR/A/..
			// is transformed into
			//      PSP_GAME/USRDIR
			if (localFileName.Length >= 3 && localFileName.lastIndexOf("/..") == localFileName.Length - 3)
			{
				if (localFileName.Length <= 3)
				{
					localFileName.Length = 0;
				}
				else
				{
					int parentIndex = localFileName.lastIndexOf("/", localFileName.Length - 4);
					if (parentIndex < 0)
					{
						localFileName.Length = 0;
					}
					else
					{
						localFileName.Length = parentIndex;
					}
				}
			}

			// Remove "/./" in the local file name
			// E.g.:
			//     PSP_GAME/USRDIR/A/./B
			// is transformed into
			//     PSP_GAME/USRDIR/A/B
			while (true)
			{
				int dotIndex = localFileName.ToString().IndexOf("/./");
				if (dotIndex < 0)
				{
					break;
				}
				localFileName.Remove(dotIndex, dotIndex + 2 - dotIndex);
			}

			// Remove "/." at the end of the local file name
			// E.g.:
			//     PSP_GAME/USRDIR/A/.
			// is transformed into
			//     PSP_GAME/USRDIR/A
			if (localFileName.Length >= 2 && localFileName.lastIndexOf("/.") == localFileName.Length - 2)
			{
				localFileName.Length = localFileName.Length - 2;
			}

			// Delete any leading "/"
			if (localFileName.Length > 0 && localFileName[0] == '/')
			{
				localFileName.Remove(0, 1);
			}

			// Delete any trailing "/"
			if (localFileName.Length > 0 && localFileName[localFileName.Length - 1] == '/')
			{
				localFileName.Length = localFileName.Length - 1;
			}
		}

		public static string getFileNameLastPart(string fileName)
		{
			if (!string.ReferenceEquals(fileName, null))
			{
				int lastSepIndex = fileName.LastIndexOf('/');
				if (lastSepIndex >= 0)
				{
					fileName = fileName.Substring(lastSepIndex + 1);
				}
			}

			return fileName;
		}
	}

}