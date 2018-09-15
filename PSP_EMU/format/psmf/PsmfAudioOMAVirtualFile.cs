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
namespace pspsharp.format.psmf
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.VFS.AbstractVirtualFileSystem.IO_ERROR;

	//using Logger = org.apache.log4j.Logger;

	using TPointer = pspsharp.HLE.TPointer;
	using AbstractProxyVirtualFile = pspsharp.HLE.VFS.AbstractProxyVirtualFile;
	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;

	/// <summary>
	/// Provides a IVirtualFile interface to convert the audio from an Mpeg to OMA.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class PsmfAudioOMAVirtualFile : AbstractProxyVirtualFile
	{
		private static new Logger log = Emulator.log;
		private int remainingFrameLength;
		private readonly sbyte[] header = new sbyte[8];

		public PsmfAudioOMAVirtualFile(IVirtualFile vFile) : base(vFile)
		{
		}

		private bool readHeader()
		{
			int Length = vFile.ioRead(header, 0, header.Length);
			if (Length < header.Length)
			{
				return false;
			}

			if (header[0] != (sbyte) 0x0F || header[1] != unchecked((sbyte) 0xD0))
			{
				Console.WriteLine(string.Format("Invalid header 0x{0:X2} 0x{1:X2}", header[0] & 0xFF, header[1] & 0xFF));
				return false;
			}

			remainingFrameLength = (((header[2] & 0x03) << 8) | ((header[3] & 0xFF) << 3)) + 8;

			return true;
		}

		public override int ioRead(TPointer outputPointer, int outputLength)
		{
			return base.ioRead(outputPointer, outputLength);
		}

		public override int ioRead(sbyte[] outputBuffer, int outputOffset, int outputLength)
		{
			int readLength = 0;
			bool error = false;

			while (outputLength > 0)
			{
				if (remainingFrameLength == 0)
				{
					if (!readHeader())
					{
						error = true;
						break;
					}
				}

				int Length = vFile.ioRead(outputBuffer, outputOffset, System.Math.Min(outputLength, remainingFrameLength));
				if (Length < 0)
				{
					error = true;
					break;
				}

				readLength += Length;
				outputOffset += Length;
				outputLength -= Length;
				remainingFrameLength -= Length;
			}

			if (error && readLength == 0)
			{
				return IO_ERROR;
			}

			return readLength;
		}
	}

}