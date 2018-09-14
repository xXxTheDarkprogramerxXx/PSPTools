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
namespace pspsharp.HLE.VFS.patch
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._v0;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._zr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.JR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.MOVE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.NOP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.ThreadManForUser.SYSCALL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.read8;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUnaligned16;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.readUnaligned32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeUnaligned16;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.writeUnaligned32;


	using Elf32Header = pspsharp.format.Elf32Header;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// Virtual file system patching/modifying files (e.g. PRX's).
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class PatchFileVirtualFileSystem : AbstractProxyVirtualFileSystem
	{
		private static readonly PatchInfo[] allPatches = new PatchInfo[]
		{
			new PrxPatchInfo("kd/loadcore.prx", 0x0000469C, 0x15C0FFA0, NOP()),
			new PrxPatchInfo("kd/loadcore.prx", 0x00004548, 0x7C0F6244, NOP()),
			new PrxPatchInfo("kd/loadcore.prx", 0x00004550, 0x14E0002C, 0x1000002C),
			new PrxPatchInfo("kd/loadcore.prx", 0x00003D58, 0x10C0FFBE, NOP()),
			new PrxPatchInfo("kd/loadcore.prx", 0x00005D1C, 0x5040FE91, NOP()),
			new PrxPatchInfo("kd/loadcore.prx", 0x00005D20, 0x3C118002, NOP()),
			new PrxPatchInfo("kd/loadcore.prx", 0x00005790, 0x5462FFF4, NOP()),
			new PrxPatchInfo("kd/loadcore.prx", 0x00005794, 0x3C118002, NOP()),
			new PrxPatchInfo("kd/loadcore.prx", 0x00004378, 0x5120FFDB, NOP()),
			new PrxPatchInfo("kd/semawm.prx", 0x00005620, 0x27BDFFD0, JR()),
			new PrxPatchInfo("kd/semawm.prx", 0x00005624, unchecked((int)0xAFBF0024), MOVE(_v0, _zr)),
			new PatchInfo("XXX dummy XXX", 0, 0, 0)
		};

		private class PatchInfo
		{
			protected internal string fileName;
			protected internal int offset;
			protected internal int oldValue;
			protected internal int newValue;

			public PatchInfo(string fileName, int offset, int oldValue, int newValue)
			{
				this.fileName = fileName;
				this.offset = offset;
				this.oldValue = oldValue;
				this.newValue = newValue;
			}

			public virtual bool matches(string fileName)
			{
				return this.fileName.Equals(fileName, StringComparison.OrdinalIgnoreCase);
			}

			protected internal virtual void apply(sbyte[] buffer, int offset)
			{
				if (offset >= 0 && offset < buffer.Length)
				{
					int checkValue = readUnaligned32(buffer, offset);
					if (checkValue != oldValue)
					{
						log.error(string.Format("Patching of file '{0}' failed at offset 0x{1:X8}, 0x{2:X8} found instead of 0x{3:X8}", fileName, offset, checkValue, oldValue));
					}
					else
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Patching file '{0}' at offset 0x{1:X8}: 0x{2:X8} -> 0x{3:X8}", fileName, offset, oldValue, newValue));
						}
						writeUnaligned32(buffer, offset, newValue);
					}
				}
			}

			public virtual void apply(sbyte[] buffer)
			{
				apply(buffer, offset);
			}

			protected internal virtual void patch16(sbyte[] buffer, int offset, int oldValue, int newValue)
			{
				if (offset >= 0 && offset < buffer.Length)
				{
					int checkValue = readUnaligned16(buffer, offset);
					if (checkValue != oldValue)
					{
						log.error(string.Format("Patching of file '{0}' failed at offset 0x{1:X8}, 0x{2:X4} found instead of 0x{3:X4}", fileName, offset, checkValue, oldValue));
					}
					else
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("Patching file '{0}' at offset 0x{1:X8}: 0x{2:X4} -> 0x{3:X4}", fileName, offset, oldValue, newValue));
						}
						writeUnaligned16(buffer, offset, newValue);
					}
				}
			}

			public override string ToString()
			{
				return string.Format("Patch '{0}' at offset 0x{1:X8}: 0x{2:X8} -> 0x{3:X8}", fileName, offset, oldValue, newValue);
			}
		}

		private class PrxPatchInfo : PatchInfo
		{
			internal int removeLocationOffset = 4;

			public PrxPatchInfo(string fileName, int offset, int oldValue, int newValue) : base(fileName, offset, oldValue, newValue)
			{
			}

			public PrxPatchInfo(string fileName, int offset, int oldValue, int newValue, int removeLocationOffset) : base(fileName, offset, oldValue, newValue)
			{
				this.removeLocationOffset = removeLocationOffset;
			}

			internal virtual int getFileOffset(sbyte[] buffer)
			{
				int elfMagic = readUnaligned32(buffer, 0);
				if (elfMagic != Elf32Header.ELF_MAGIC)
				{
					return offset;
				}

				int phOffset = readUnaligned32(buffer, 28);
				int phEntSize = readUnaligned16(buffer, 42);
				int phNum = readUnaligned16(buffer, 44);

				int segmentOffset = offset;
				// Scan all the ELF program headers
				for (int i = 0; i < phNum; i++)
				{
					int offset = phOffset + i * phEntSize;
					int phEntFileSize = readUnaligned32(buffer, offset + 16);
					if (segmentOffset < phEntFileSize)
					{
						int phFileOffset = readUnaligned32(buffer, offset + 4);
						return phFileOffset + segmentOffset;
					}

					int phEntMemSize = readUnaligned32(buffer, offset + 20);
					segmentOffset -= phEntMemSize;
					if (segmentOffset < 0)
					{
						log.error(string.Format("Patching of file '{0}' failed: incorrect offset 0x{1:X8} outside of program header segment #{2:D}", fileName, offset, i));
						return -1;
					}
				}

				log.error(string.Format("Patching of file '{0}' failed: incorrect offset 0x{1:X8} outside of all program header segments", fileName, offset));
				return -1;
			}

			internal virtual int getRelocationSegmentNumber(sbyte[] buffer)
			{
				int phOffset = readUnaligned32(buffer, 28);
				int phEntSize = readUnaligned16(buffer, 42);
				int phNum = readUnaligned16(buffer, 44);

				// Scan all the ELF program headers
				for (int i = 0; i < phNum; i++)
				{
					int offset = phOffset + i * phEntSize;
					int phType = readUnaligned32(buffer, offset + 0);
					if (phType == 0x700000A1)
					{
						return i;
					}
				}

				return -1;
			}

			internal virtual void removeRelocation(sbyte[] buffer)
			{
				int relocationSegmentNumber = getRelocationSegmentNumber(buffer);
				if (relocationSegmentNumber < 0)
				{
					return;
				}

				int phOffset = readUnaligned32(buffer, 28);
				int phEntSize = readUnaligned16(buffer, 42);

				int o = readUnaligned32(buffer, phOffset + relocationSegmentNumber * phEntSize + 4);
				o += 2;

				int fbits = read8(buffer, o++);
				int flagShift = 0;
				int flagMask = (1 << fbits) - 1;

				int sbits = relocationSegmentNumber < 3 ? 1 : 2;
				int segmentShift = fbits;
				int segmentMask = (1 << sbits) - 1;

				int tbits = read8(buffer, o++);
				int typeShift = fbits + sbits;
				int typeMask = (1 << tbits) - 1;

				int nflags = read8(buffer, o++);
				int[] flags = new int[nflags];
				flags[0] = nflags;
				for (int i = 1; i < nflags; i++)
				{
					flags[i] = read8(buffer, o++);
				}

				int ntypes = read8(buffer, o++);
				int[] types = new int[ntypes];
				types[0] = ntypes;
				for (int i = 1; i < ntypes; i++)
				{
					types[i] = read8(buffer, o++);
				}

				int offsetShift = fbits + tbits + sbits;
				int OFS_BASE = 0;
				int R_BASE = 0;
				while (o < buffer.Length)
				{
					int cmdOffset = o;
					int R_CMD = readUnaligned16(buffer, o);
					o += 2;

					// Process the relocation flag.
					int flagIndex = (R_CMD >> flagShift) & flagMask;
					int R_FLAG = flags[flagIndex];

					// Set the segment offset.
					int S = (R_CMD >> segmentShift) & segmentMask;

					// Process the relocation type.
					int typeIndex = (R_CMD >> typeShift) & typeMask;
					//int R_TYPE = types[typeIndex];

					if ((R_FLAG & 1) == 0)
					{
						OFS_BASE = S;
						switch (R_FLAG & 6)
						{
							case 0:
								R_BASE = R_CMD >> (fbits + sbits);
								break;
							case 4:
								R_BASE = readUnaligned32(buffer, o);
								o += 4;
								break;
							default:
								return;
						}
					}
					else
					{
						int R_OFFSET;
						switch (R_FLAG & 6)
						{
							case 0:
								R_OFFSET = (int)(short) R_CMD; // sign-extend 16 to 32 bits
								R_OFFSET >>= offsetShift;
								R_BASE += R_OFFSET;
								break;
							case 2:
								R_OFFSET = (R_CMD << 16) >> offsetShift;
								R_OFFSET &= unchecked((int)0xFFFF0000);
								R_OFFSET |= read8(buffer, o++);
								R_OFFSET |= read8(buffer, o++) << 8;
								R_BASE += R_OFFSET;
								break;
							case 4:
								R_BASE = readUnaligned32(buffer, o);
								o += 4;
								break;
							default:
								return;
						}

						switch (R_FLAG & 0x38)
						{
							case 0x0:
							case 0x8:
								break;
							case 0x10:
								o += 2;
								break;
							default:
								return;
						}

						if (log.TraceEnabled)
						{
							log.trace(string.Format("Relocation R_BASE=0x{0:X8}", R_BASE));
						}

						if (R_BASE == offset)
						{
							int newOffset = ((int)(short) R_CMD) >> offsetShift;
							if ((R_FLAG & 7) == 1)
							{
								newOffset += removeLocationOffset;
							}
							else
							{
								log.error(string.Format("Unsupported relocation patch at 0x{0:X8}, R_FLAG=0x{1:X}", R_BASE, R_FLAG));
								return;
							}
							int newCmd = (flagIndex << flagShift) | (OFS_BASE << segmentShift) | (typeIndex << typeShift) | (newOffset << offsetShift);
							newCmd &= 0xFFFF;
							patch16(buffer, cmdOffset, R_CMD, newCmd);

							int nextCmd = readUnaligned16(buffer, o);
							int nextFlagIndex = (nextCmd >> flagShift) & flagMask;
							int nextFlag = flags[nextFlagIndex];
							int nextSegment = (nextCmd >> segmentShift) & segmentMask;
							int nextTypeIndex = (nextCmd >> typeShift) & typeMask;

							int newNextOffset = ((int)(short) nextCmd) >> offsetShift;
							if ((nextFlag & 7) == 1)
							{
								newNextOffset -= removeLocationOffset;
							}
							else
							{
								log.error(string.Format("Unsupported relocation patch at 0x{0:X8}, R_CMD=0x{1:X4}, nextCmd=0x{2:X4}", R_BASE, R_CMD, nextCmd));
								return;
							}
							int newNextCmd = (nextFlagIndex << flagShift) | (nextSegment << segmentShift) | (nextTypeIndex << typeShift) | (newNextOffset << offsetShift);
							patch16(buffer, o, nextCmd, newNextCmd);

							return;
						}
					}
				}
			}

			public override void apply(sbyte[] buffer)
			{
				// Can only patch PRX in ELF format
				int headerMagic = Utilities.readUnaligned32(buffer, 0);
				if (headerMagic != Elf32Header.ELF_MAGIC)
				{
					return;
				}

				int fileOffset = getFileOffset(buffer);

				if (fileOffset >= 0)
				{
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Patching file '{0}' at PRX offset 0x{1:X8}: 0x{2:X8} -> 0x{3:X8}", fileName, offset, oldValue, newValue));
					}
					base.apply(buffer, fileOffset);

					removeRelocation(buffer);
				}
			}
		}

		protected internal class PrxSyscallPatchInfo : PrxPatchInfo
		{
			internal PrxPatchInfo patchInfo2;
			internal string functionName;

			public PrxSyscallPatchInfo(string fileName, HLEModule hleModule, string functionName, int offset, int oldValue1, int oldValue2) : base(fileName, offset, oldValue1, JR(), 8)
			{
				this.functionName = functionName;
				patchInfo2 = new PrxPatchInfo(fileName, offset + 4, oldValue2, SYSCALL(hleModule, functionName));
			}

			public override void apply(sbyte[] buffer)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("Patching file '{0}' at PRX offset 0x{1:X8}: {2}", fileName, offset, functionName));
				}
				base.apply(buffer);
				patchInfo2.apply(buffer);
			}
		}

		public PatchFileVirtualFileSystem(IVirtualFileSystem vfs) : base(vfs)
		{
		}

		private IList<PatchInfo> getPatches(string fileName)
		{
			IList<PatchInfo> filePatches = new LinkedList<PatchInfo>();
			foreach (PatchInfo patch in allPatches)
			{
				if (patch.matches(fileName))
				{
					filePatches.Add(patch);
				}
			}

			if (filePatches.Count == 0)
			{
				return null;
			}
			return filePatches;
		}

		private IVirtualFile ioOpenPatchedFile(string fileName, int flags, int mode, IList<PatchInfo> patches)
		{
			IVirtualFile vFile = base.ioOpen(fileName, flags, mode);
			if (vFile == null)
			{
				return null;
			}

			sbyte[] buffer = Utilities.readCompleteFile(vFile);
			vFile.ioClose();
			if (buffer == null)
			{
				return null;
			}

			foreach (PatchInfo patch in patches)
			{
				patch.apply(buffer);
			}

			return new ByteArrayVirtualFile(buffer);
		}

		public override IVirtualFile ioOpen(string fileName, int flags, int mode)
		{
			IList<PatchInfo> patches = getPatches(fileName);
			if (patches != null)
			{
				return ioOpenPatchedFile(fileName, flags, mode, patches);
			}

			return base.ioOpen(fileName, flags, mode);
		}
	}

}