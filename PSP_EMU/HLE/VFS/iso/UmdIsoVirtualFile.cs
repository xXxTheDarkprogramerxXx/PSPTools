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
namespace pspsharp.HLE.VFS.iso
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_INVALID_ARGUMENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_KERNEL_FILE_READ_ERROR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_SEEK_CUR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_SEEK_END;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.IoFileMgrForUser.PSP_SEEK_SET;

	using UmdIsoFile = pspsharp.filesystems.umdiso.UmdIsoFile;
	using UmdIsoReader = pspsharp.filesystems.umdiso.UmdIsoReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Utilities = pspsharp.util.Utilities;

	public class UmdIsoVirtualFile : AbstractVirtualFile
	{
		protected internal readonly new UmdIsoFile file;
		protected internal readonly bool sectorBlockMode;
		protected internal readonly UmdIsoReader iso;

		public UmdIsoVirtualFile(UmdIsoFile file) : base(file)
		{
			this.file = file;
			this.sectorBlockMode = false;
			this.iso = file.UmdIsoReader;
		}

		public UmdIsoVirtualFile(UmdIsoFile file, bool sectorBlockMode, UmdIsoReader iso) : base(file)
		{
			this.file = file;
			this.sectorBlockMode = sectorBlockMode;
			this.iso = iso;
		}

		public override bool SectorBlockMode
		{
			get
			{
				return sectorBlockMode;
			}
		}

		public override int ioIoctl(int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			int result;

			switch (command)
			{
				// UMD file seek set.
				case 0x01010005:
				{
					if (inputPointer.AddressGood && inputLength >= 4)
					{
						int offset = inputPointer.getValue32();
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("ioIoctl umd file seek set {0:D}", offset));
						}
						Position = offset;
						result = 0;
					}
					else
					{
						result = ERROR_INVALID_ARGUMENT;
					}
					break;
				}
				// Get UMD Primary Volume Descriptor
				case 0x01020001:
				{
					if (outputPointer.AddressGood && outputLength == UmdIsoFile.sectorLength)
					{
						try
						{
							sbyte[] primaryVolumeSector = iso.readSector(UmdIsoReader.startSector);
							IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(outputPointer.Address, outputLength, 1);
							for (int i = 0; i < outputLength; i++)
							{
								memoryWriter.writeNext(primaryVolumeSector[i] & 0xFF);
							}
							memoryWriter.flush();
							result = 0;
						}
						catch (IOException e)
						{
							Console.WriteLine("ioIoctl", e);
							result = ERROR_KERNEL_FILE_READ_ERROR;
						}
					}
					else
					{
						result = ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				// Get UMD Path Table
				case 0x01020002:
				{
					if (outputPointer.AddressGood && outputLength <= UmdIsoFile.sectorLength)
					{
						try
						{
							sbyte[] primaryVolumeSector = iso.readSector(UmdIsoReader.startSector);
							ByteBuffer primaryVolume = ByteBuffer.wrap(primaryVolumeSector);
							primaryVolume.position(140);
							int pathTableLocation = Utilities.readWord(primaryVolume);
							sbyte[] pathTableSector = iso.readSector(pathTableLocation);
							IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(outputPointer.Address, outputLength, 1);
							for (int i = 0; i < outputLength; i++)
							{
								memoryWriter.writeNext(pathTableSector[i] & 0xFF);
							}
							memoryWriter.flush();
							result = 0;
						}
						catch (IOException e)
						{
							Console.WriteLine("ioIoctl", e);
							result = ERROR_KERNEL_FILE_READ_ERROR;
						}
					}
					else
					{
						result = ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				// Get Sector size
				case 0x01020003:
				{
					if (outputPointer.AddressGood && outputLength == 4)
					{
						outputPointer.setValue32(UmdIsoFile.sectorLength);
						result = 0;
					}
					else
					{
						result = ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				// Get UMD file pointer.
				case 0x01020004:
				{
					if (outputPointer.AddressGood && outputLength >= 4)
					{
						try
						{
							int fPointer = (int) file.FilePointer;
							outputPointer.setValue32(fPointer);
							//if (log.DebugEnabled)
							{
								Console.WriteLine(string.Format("ioIoctl umd file get file pointer {0:D}", fPointer));
							}
							result = 0;
						}
						catch (IOException e)
						{
							Console.WriteLine("ioIoctl", e);
							result = ERROR_KERNEL_FILE_READ_ERROR;
						}
					}
					else
					{
						result = ERROR_INVALID_ARGUMENT;
					}
					break;
				}
				// Get UMD file start sector.
				case 0x01020006:
				{
					if (outputPointer.AddressGood && outputLength >= 4)
					{
						int startSector = 0;
						startSector = file.StartSector;
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("ioIoctl umd file get start sector {0:D}", startSector));
						}
						outputPointer.setValue32(startSector);
						result = 0;
					}
					else
					{
						result = ERROR_INVALID_ARGUMENT;
					}
					break;
				}
				// Get UMD file Length in bytes.
				case 0x01020007:
				{
					if (outputPointer.AddressGood && outputLength >= 8)
					{
						long Length = this.Length();
						outputPointer.Value64 = Length;
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("ioIoctl get file size {0:D}", Length));
						}
						result = 0;
					}
					else
					{
						result = ERROR_INVALID_ARGUMENT;
					}
					break;
				}
				// Read UMD file.
				case 0x01030008:
				{
					if (inputPointer.AddressGood && inputLength >= 4)
					{
						int Length = inputPointer.getValue32();
						if (Length > 0)
						{
							if (outputPointer.AddressGood && outputLength >= Length)
							{
								try
								{
									Utilities.readFully(file, outputPointer.Address, Length);
									Position = Position + Length;
									result = Length;
								}
								catch (IOException e)
								{
									Console.WriteLine("ioIoctl", e);
									result = ERROR_KERNEL_FILE_READ_ERROR;
								}
							}
							else
							{
								result = ERROR_INVALID_ARGUMENT;
							}
						}
						else
						{
							result = ERROR_INVALID_ARGUMENT;
						}
					}
					else
					{
						result = ERROR_INVALID_ARGUMENT;
					}
					break;
				}
				// UMD disc read sectors operation.
				case 0x01F30003:
				{
					if (inputPointer.AddressGood && inputLength >= 4)
					{
						int numberOfSectors = inputPointer.getValue32();
						if (numberOfSectors > 0)
						{
							if (outputPointer.AddressGood && outputLength >= numberOfSectors)
							{
								try
								{
									int Length = numberOfSectors * UmdIsoFile.sectorLength;
									Utilities.readFully(file, outputPointer.Address, Length);
									Position = Position + Length;
									result = Length / UmdIsoFile.sectorLength;
								}
								catch (IOException e)
								{
									Console.WriteLine("ioIoctl", e);
									result = ERROR_KERNEL_FILE_READ_ERROR;
								}
							}
							else
							{
								result = ERROR_ERRNO_INVALID_ARGUMENT;
							}
						}
						else
						{
							result = ERROR_ERRNO_INVALID_ARGUMENT;
						}
					}
					else
					{
						result = ERROR_ERRNO_INVALID_ARGUMENT;
					}
					break;
				}
				// UMD file seek whence.
				case 0x01F100A6:
				{
					if (inputPointer.AddressGood && inputLength >= 16)
					{
						long offset = inputPointer.getValue64(0);
						int whence = inputPointer.getValue32(12);
						if (SectorBlockMode)
						{
							offset *= UmdIsoFile.sectorLength;
						}
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("ioIoctl UMD file seek offset {0:D}, whence {1:D}", offset, whence));
						}
						switch (whence)
						{
							case PSP_SEEK_SET:
								Position = offset;
								result = 0;
								break;
							case PSP_SEEK_CUR:
								Position = Position + offset;
								result = 0;
								break;
							case PSP_SEEK_END:
								Position = Length() + offset;
								result = 0;
								break;
							default:
								Console.WriteLine(string.Format("ioIoctl - unhandled whence {0:D}", whence));
								result = ERROR_INVALID_ARGUMENT;
								break;
						}
					}
					else
					{
						result = ERROR_INVALID_ARGUMENT;
					}
					break;
				}
				default:
					result = base.ioIoctl(command, inputPointer, inputLength, outputPointer, outputLength);
				break;
			}

			return result;
		}

		public override IVirtualFile duplicate()
		{
			IVirtualFile duplicate = null;

			try
			{
				UmdIsoFile umdIsoFile = file.duplicate();
				if (umdIsoFile != null)
				{
					duplicate = new UmdIsoVirtualFile(umdIsoFile, SectorBlockMode, iso);
				}
			}
			catch (IOException e)
			{
				Console.WriteLine("UmdIsoVirtualFile.duplicate", e);
			}

			return duplicate;
		}

		public virtual long Length
		{
			set
			{
				file.Length = value;
			}
		}

		public override string ToString()
		{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("UmdIsoVirtualFile[%s, sectorBlockMode=%b]", file, sectorBlockMode);
			return string.Format("UmdIsoVirtualFile[%s, sectorBlockMode=%b]", file, sectorBlockMode);
		}
	}

}