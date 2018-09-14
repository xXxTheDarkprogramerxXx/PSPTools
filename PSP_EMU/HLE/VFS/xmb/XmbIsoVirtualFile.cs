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
namespace pspsharp.HLE.VFS.xmb
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUnaligned16;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUnaligned32;


	using UmdIsoVirtualFileSystem = pspsharp.HLE.VFS.iso.UmdIsoVirtualFileSystem;
	using SceIoStat = pspsharp.HLE.kernel.types.SceIoStat;
	using IoFileMgrForUser = pspsharp.HLE.modules.IoFileMgrForUser;
	using IoOperation = pspsharp.HLE.modules.IoFileMgrForUser.IoOperation;
	using IoOperationTiming = pspsharp.HLE.modules.IoFileMgrForUser.IoOperationTiming;
	using UmdIsoReader = pspsharp.filesystems.umdiso.UmdIsoReader;
	using PBP = pspsharp.format.PBP;
	using Settings = pspsharp.settings.Settings;
	using Utilities = pspsharp.util.Utilities;

	public class XmbIsoVirtualFile : AbstractVirtualFile
	{
		protected internal class PbpSection
		{
			internal int index;
			internal int size;
			internal int offset;
			internal bool availableInContents;
			internal string umdFilename;
			internal File cacheFile;
		}

		private string umdFilename;
		private string umdName;
		protected internal long filePointer;
		protected internal long totalLength;
		protected internal static readonly string[] umdFilenames = new string [] {"PSP_GAME/PARAM.SFO", "PSP_GAME/ICON0.PNG", "PSP_GAME/ICON1.PMF", "PSP_GAME/PIC0.PNG", "PSP_GAME/PIC1.PNG", "PSP_GAME/SND0.AT3"};
		protected internal sbyte[] contents;
		protected internal PbpSection[] sections;

		public XmbIsoVirtualFile(string umdFilename) : base(null)
		{

			this.umdFilename = umdFilename;
			umdName = System.IO.Path.GetFileName(umdFilename);

			File cacheDirectory = new File(CacheDirectory);
			bool createCacheFiles = !cacheDirectory.Directory;
			if (createCacheFiles)
			{
				cacheDirectory.mkdirs();
			}

			try
			{
				UmdIsoReader iso = new UmdIsoReader(umdFilename);
				IVirtualFileSystem vfs = new UmdIsoVirtualFileSystem(iso);
				sections = new PbpSection[umdFilenames.Length + 1];
				sections[0] = new PbpSection();
				sections[0].index = 0;
				sections[0].offset = 0;
				sections[0].size = 0x28;
				sections[0].availableInContents = true;
				int offset = 0x28;
				SceIoStat stat = new SceIoStat();
				for (int i = 0; i < umdFilenames.Length; i++)
				{
					PbpSection section = new PbpSection();
					section.index = i + 1;
					section.offset = offset;
					section.umdFilename = umdFilenames[i];
					if (vfs.ioGetstat(section.umdFilename, stat) >= 0)
					{
						section.size = (int) stat.size;
						if (log.TraceEnabled)
						{
							log.trace(string.Format("{0}: mapping {1} at offset 0x{2:X}, size 0x{3:X}", umdFilename, umdFilenames[i], section.offset, section.size));
						}
					}

					string cacheFileName = getCacheFileName(section);
					File cacheFile = new File(cacheFileName);

					// Create only cache files for PARAM.SFO and ICON0.PNG
					if (createCacheFiles && i < 2)
					{
						IVirtualFile vFile = vfs.ioOpen(section.umdFilename, IoFileMgrForUser.PSP_O_RDONLY, 0);
						if (vFile != null)
						{
							section.size = (int) vFile.length();
							sbyte[] buffer = new sbyte[section.size];
							int length = vFile.ioRead(buffer, 0, buffer.Length);
							vFile.ioClose();

							System.IO.Stream os = new System.IO.FileStream(cacheFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
							os.Write(buffer, 0, length);
							os.Close();
						}
					}

					if (cacheFile.canRead())
					{
						section.cacheFile = cacheFile;
					}

					sections[section.index] = section;
					offset += section.size;
				}
				totalLength = offset;

				contents = new sbyte[offset];
				ByteBuffer buffer = ByteBuffer.wrap(contents).order(ByteOrder.LITTLE_ENDIAN);
				buffer.putInt(PBP.PBP_MAGIC);
				buffer.putInt(0x10000); // version
				for (int i = 1; i < sections.Length; i++)
				{
					buffer.putInt(sections[i].offset);
				}
				int endSectionOffset = sections[sections.Length - 1].offset + sections[sections.Length - 1].size;
				for (int i = sections.Length; i <= 8; i++)
				{
					buffer.putInt(endSectionOffset);
				}

				if (log.TraceEnabled)
				{
					log.trace(string.Format("{0}: PBP header :{1}", umdFilename, Utilities.getMemoryDump(contents, sections[0].offset, sections[0].size)));
				}
				vfs.ioExit();
			}
			catch (FileNotFoundException e)
			{
				log.debug("XmbIsoVirtualFile", e);
			}
			catch (IOException e)
			{
				log.debug("XmbIsoVirtualFile", e);
			}
		}

		protected internal virtual string CacheDirectory
		{
			get
			{
				return string.Format("{0}{1}UmdBrowserCache{1}{2}", Settings.Instance.readString("emu.tmppath"), System.IO.Path.DirectorySeparatorChar, umdName);
			}
		}

		protected internal virtual string getCacheFileName(PbpSection section)
		{
			return string.Format("{0}{1}{2}", CacheDirectory, System.IO.Path.DirectorySeparatorChar, section.umdFilename.Substring(9));
		}

		protected internal virtual void readSection(PbpSection section)
		{
			if (section.size > 0)
			{
				try
				{
					if (section.cacheFile != null)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("XmbIsoVirtualFile.readSection from Cache {0}", section.cacheFile));
						}

						System.IO.Stream @is = new System.IO.FileStream(section.cacheFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
						@is.Read(contents, section.offset, section.size);
						@is.Close();
					}
					else
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("XmbIsoVirtualFile.readSection from UMD {0}", section.umdFilename));
						}

						UmdIsoReader iso = new UmdIsoReader(umdFilename);
						IVirtualFileSystem vfs = new UmdIsoVirtualFileSystem(iso);
						IVirtualFile vFile = vfs.ioOpen(section.umdFilename, IoFileMgrForUser.PSP_O_RDONLY, 0);
						if (vFile != null)
						{
							vFile.ioRead(contents, section.offset, section.size);
							vFile.ioClose();
						}
						vfs.ioExit();
					}
				}
				catch (IOException e)
				{
					log.debug("readSection", e);
				}

				// PARAM.SFO?
				if (section.index == 1)
				{
					// Patch the CATEGORY in the PARAM.SFO:
					// the VSH is checking that the CATEGORY value is starting
					// with 'M' (meaning MemoryStick) and not 'U' (UMD).
					// Change the first letter 'U' into 'M'.
					int offset = section.offset;
					int keyTableOffset = readUnaligned32(contents, offset + 8) + offset;
					int valueTableOffset = readUnaligned32(contents, offset + 12) + offset;
					int numberKeys = readUnaligned32(contents, offset + 16);
					for (int i = 0; i < numberKeys; i++)
					{
						int keyOffset = readUnaligned16(contents, offset + 20 + i * 16);
						string key = Utilities.readStringZ(contents, keyTableOffset + keyOffset);
						if ("CATEGORY".Equals(key))
						{
							int valueOffset = readUnaligned32(contents, offset + 20 + i * 16 + 12);
							char valueFirstChar = (char) contents[valueTableOffset + valueOffset];

							// Change the first letter 'U' into 'M'.
							if (valueFirstChar == 'U')
							{
								contents[valueTableOffset + valueOffset] = (sbyte)'M';
							}
							break;
						}
					}
				}
			}

			section.availableInContents = true;
		}

		protected internal virtual int ioRead(PbpSection section, TPointer outputPointer, int offset, int length)
		{
			if (filePointer < section.offset || filePointer >= section.offset + section.size)
			{
				return 0;
			}

			length = System.Math.Min(length, section.size - (int)(filePointer - section.offset));
			if (length > 0)
			{
				if (!section.availableInContents)
				{
					readSection(section);
				}

				outputPointer.setArray(offset, contents, (int) filePointer, length);
			}

			return length;
		}

		public override int ioRead(TPointer outputPointer, int outputLength)
		{
			int remaining = (int) System.Math.Min(outputLength, contents.Length - filePointer);
			int offset = 0;
			for (int i = 0; remaining > 0 && i < sections.Length; i++)
			{
				int length = ioRead(sections[i], outputPointer, offset, remaining);
				filePointer += length;
				offset += length;
				remaining -= length;
			}

			return offset;
		}

		public override int ioRead(sbyte[] outputBuffer, int outputOffset, int outputLength)
		{
			return base.ioRead(outputBuffer, outputOffset, outputLength);
		}

		public override long length()
		{
			return totalLength;
		}

		public override long ioLseek(long offset)
		{
			filePointer = offset;

			return filePointer;
		}

		public override int ioClose()
		{
			filePointer = 0L;

			return 0;
		}

		public override IDictionary<IoFileMgrForUser.IoOperation, IoFileMgrForUser.IoOperationTiming> Timings
		{
			get
			{
				// Do not delay IO operations on faked EBOOT.PBP files
				return IoFileMgrForUser.noDelayTimings;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public pspsharp.HLE.VFS.IVirtualFile ioReadForLoadExec() throws java.io.FileNotFoundException, java.io.IOException
		public virtual IVirtualFile ioReadForLoadExec()
		{
			UmdIsoReader iso = IsoReader;
			IVirtualFileSystem vfs = new UmdIsoVirtualFileSystem(iso);
			return vfs.ioOpen("PSP_GAME/SYSDIR/EBOOT.BIN", IoFileMgrForUser.PSP_O_RDONLY, 0);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public pspsharp.filesystems.umdiso.UmdIsoReader getIsoReader() throws java.io.FileNotFoundException, java.io.IOException
		public virtual UmdIsoReader IsoReader
		{
			get
			{
				return new UmdIsoReader(umdFilename);
			}
		}
	}

}