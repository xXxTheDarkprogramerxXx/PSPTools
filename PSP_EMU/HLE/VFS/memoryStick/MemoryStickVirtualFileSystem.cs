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
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadManForUser = pspsharp.HLE.modules.ThreadManForUser;
	using MemoryStick = pspsharp.hardware.MemoryStick;

	/// <summary>
	/// Virtual File System implementing the PSP device mscmhc0.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class MemoryStickVirtualFileSystem : AbstractVirtualFileSystem
	{

		public override int ioDevctl(string deviceName, int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			int result;

			switch (command)
			{
				// Check the MemoryStick's driver status (mscmhc0).
				case 0x02025801:
				{
					Console.WriteLine("ioDevctl check ms driver status");
					if (outputPointer.AddressGood)
					{
						// 0 = Driver busy.
						// 1 = Driver ready.
						// 4 = ???
						outputPointer.setValue32(4);
						result = 0;
					}
					else
					{
						result = IO_ERROR;
					}
					break;
				}
				// Register MemoryStick's insert/eject callback (mscmhc0).
				case 0x02015804:
				{
					Console.WriteLine("ioDevctl register memorystick insert/eject callback (mscmhc0)");
					ThreadManForUser threadMan = Modules.ThreadManForUserModule;
					if (inputPointer.AddressGood && inputLength == 4)
					{
						int cbid = inputPointer.getValue32();
						const int callbackType = SceKernelThreadInfo.THREAD_CALLBACK_MEMORYSTICK;
						if (threadMan.hleKernelRegisterCallback(callbackType, cbid))
						{
							// Trigger the registered callback immediately.
							threadMan.hleKernelNotifyCallback(callbackType, cbid, MemoryStick.StateMs);
							result = 0; // Success.
						}
						else
						{
							result = SceKernelErrors.ERROR_MEMSTICK_DEVCTL_TOO_MANY_CALLBACKS;
						}
					}
					else
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					break;
				}
				// Unregister MemoryStick's insert/eject callback (mscmhc0).
				case 0x02015805:
				{
					Console.WriteLine("ioDevctl unregister memorystick insert/eject callback (mscmhc0)");
					ThreadManForUser threadMan = Modules.ThreadManForUserModule;
					if (inputPointer.AddressGood && inputLength == 4)
					{
						int cbid = inputPointer.getValue32();
						if (threadMan.hleKernelUnRegisterCallback(SceKernelThreadInfo.THREAD_CALLBACK_MEMORYSTICK, cbid))
						{
							result = 0; // Success.
						}
						else
						{
							result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS; // No such callback.
						}
					}
					else
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					break;
				}
				// Check if the device is inserted (mscmhc0).
				case 0x02025806:
				{
					Console.WriteLine("ioDevctl check ms inserted (mscmhc0)");
					if (outputPointer.AddressGood && outputLength >= 4)
					{
						// 0 = Not inserted.
						// 1 = Inserted.
						outputPointer.setValue32(1);
						result = 0;
					}
					else
					{
						result = ERROR_MEMSTICK_DEVCTL_BAD_PARAMS;
					}
					break;
				}
				default:
					result = base.ioDevctl(deviceName, command, inputPointer, inputLength, outputPointer, outputLength);
				break;
			}

			return result;
		}
	}

}