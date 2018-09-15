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
namespace pspsharp.HLE.VFS.memoryStick
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_INVALID_ARGUMENT;
	using MemoryStick = pspsharp.hardware.MemoryStick;

	public class MemoryStickStorageVirtualFile : AbstractVirtualFile
	{
		public MemoryStickStorageVirtualFile() : base(null)
		{
		}

		public override int ioIoctl(int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			int result;

			switch (command)
			{
				case 0x02125009:
					// Is the memory stick locked?
					if (outputPointer.NotNull && outputLength >= 4)
					{
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("ioIoctl msstor cmd 0x{0:X8}", command));
						}
						outputPointer.setValue32(MemoryStick.Locked);
						result = 0;
					}
					else
					{
						result = ERROR_INVALID_ARGUMENT;
					}
					break;
				case 0x02125008:
					// Is the memory stick inserted?
					if (outputPointer.NotNull && outputLength >= 4)
					{
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("ioIoctl msstor cmd 0x{0:X8}", command));
						}
						// Unknown output value
						outputPointer.setValue32(MemoryStick.Inserted);
						result = 0;
					}
					else
					{
						result = ERROR_INVALID_ARGUMENT;
					}
					break;
				case 0x02125803:
					if (outputPointer.NotNull && outputLength >= 96)
					{
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("ioIoctl msstor cmd 0x{0:X8}", command));
						}
						// Unknown output values
						outputPointer.clear(96);
						outputPointer.setStringNZ(12, 16, ""); // This value will be set in registry as /CONFIG/CAMERA/msid
						result = 0;
					}
					else
					{
						result = ERROR_INVALID_ARGUMENT;
					}
					break;
				default:
					result = base.ioIoctl(command, inputPointer, inputLength, outputPointer, outputLength);
					break;
			}

			return result;
		}

		public override int ioClose()
		{
			return 0;
		}

		public override int ioRead(TPointer outputPointer, int outputLength)
		{
			return IO_ERROR;
		}

		public override int ioRead(sbyte[] outputBuffer, int outputOffset, int outputLength)
		{
			return IO_ERROR;
		}

		public override long ioLseek(long offset)
		{
			return IO_ERROR;
		}

		public override long Length()
		{
			return 0L;
		}
	}

}