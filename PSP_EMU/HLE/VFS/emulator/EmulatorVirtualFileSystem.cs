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
namespace pspsharp.HLE.VFS.emulator
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceDisplay.getPixelFormatBytes;


	using BufferInfo = pspsharp.HLE.modules.sceDisplay.BufferInfo;
	using AutoTestsOutput = pspsharp.autotests.AutoTestsOutput;
	using CaptureImage = pspsharp.graphics.capture.CaptureImage;
	using Screen = pspsharp.hardware.Screen;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;

	public class EmulatorVirtualFileSystem : AbstractVirtualFileSystem
	{
		public const int EMULATOR_DEVCTL_GET_HAS_DISPLAY = 0x01;
		public const int EMULATOR_DEVCTL_SEND_OUTPUT = 0x02;
		public const int EMULATOR_DEVCTL_IS_EMULATOR = 0x03;
		public const int EMULATOR_DEVCTL_SEND_CTRLDATA = 0x10;
		public const int EMULATOR_DEVCTL_EMIT_SCREENSHOT = 0x20;
		private static string screenshotFileName = "testResult.bmp";
		private static string screenshotFormat = "bmp";

		public override int ioDevctl(string deviceName, int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			switch (command)
			{
				case EMULATOR_DEVCTL_GET_HAS_DISPLAY:
					if (!outputPointer.AddressGood || outputLength < 4)
					{
						return base.ioDevctl(deviceName, command, inputPointer, inputLength, outputPointer, outputLength);
					}
					outputPointer.setValue32(Screen.hasScreen());
					break;
				case EMULATOR_DEVCTL_SEND_OUTPUT:
					sbyte[] input = new sbyte[inputLength];
					IMemoryReader memoryReader = MemoryReader.getMemoryReader(inputPointer.Address, inputLength, 1);
					for (int i = 0; i < inputLength; i++)
					{
						input[i] = (sbyte) memoryReader.readNext();
					}
					string outputString = StringHelper.NewString(input);
					//if (log.DebugEnabled)
					{
						Console.WriteLine(outputString);
					}
					AutoTestsOutput.appendString(outputString);
					break;
				case EMULATOR_DEVCTL_IS_EMULATOR:
					break;
				case EMULATOR_DEVCTL_EMIT_SCREENSHOT:
					BufferInfo fb = Modules.sceDisplayModule.BufferInfoFb;
					Buffer buffer = Memory.Instance.getBuffer(fb.topAddr, fb.bufferWidth * fb.height * getPixelFormatBytes(fb.pixelFormat));
					CaptureImage captureImage = new CaptureImage(fb.topAddr, 0, buffer, fb.width, fb.height, fb.bufferWidth,fb.pixelFormat, false, 0, false, true, null);
					captureImage.FileName = ScreenshotFileName;
					captureImage.FileFormat = ScreenshotFormat;
					try
					{
						captureImage.write();
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("Screenshot 0x{0:X8}-0x{1:X8} saved under '{2}'", fb.topAddr, fb.bottomAddr, captureImage.FileName));
						}
					}
					catch (IOException e)
					{
						Console.WriteLine("Emit Screenshot", e);
					}
					break;
				default:
					// Unknown command
					return base.ioDevctl(deviceName, command, inputPointer, inputLength, outputPointer, outputLength);
			}

			return 0;
		}

		public static string ScreenshotFileName
		{
			get
			{
				return screenshotFileName;
			}
			set
			{
				EmulatorVirtualFileSystem.screenshotFileName = value;
			}
		}


		public static string ScreenshotFormat
		{
			get
			{
				return screenshotFormat;
			}
			set
			{
				EmulatorVirtualFileSystem.screenshotFormat = value;
			}
		}

	}

}