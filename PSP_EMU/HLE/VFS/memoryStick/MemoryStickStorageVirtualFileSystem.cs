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

	/// <summary>
	/// Virtual File System implementing the PSP device msstor0p1.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class MemoryStickStorageVirtualFileSystem : AbstractVirtualFileSystem
	{
		public override IVirtualFile ioOpen(string fileName, int flags, int mode)
		{
			return new MemoryStickStorageVirtualFile();
		}

		public override int ioDevctl(string deviceName, int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			int result;

			switch (command)
			{
				case 0x02125802:
					if (outputPointer.NotNull && outputLength >= 4)
					{
						if (log.DebugEnabled)
						{
							log.debug(string.Format("ioIoctl msstor cmd 0x{0:X8}", command));
						}
						// Output value 0x11 or 0x41: the Memory Stick is locked
						outputPointer.setValue32(0);
						result = 0;
					}
					else
					{
						result = ERROR_INVALID_ARGUMENT;
					}
					break;
				default:
					result = base.ioDevctl(deviceName, command, inputPointer, inputLength, outputPointer, outputLength);
					break;
			}

			return result;
		}
	}

}