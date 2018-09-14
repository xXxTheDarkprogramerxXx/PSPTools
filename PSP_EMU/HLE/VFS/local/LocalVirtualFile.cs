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
namespace pspsharp.HLE.VFS.local
{

	using IoFileMgrForUser = pspsharp.HLE.modules.IoFileMgrForUser;
	using IoOperation = pspsharp.HLE.modules.IoFileMgrForUser.IoOperation;
	using IoOperationTiming = pspsharp.HLE.modules.IoFileMgrForUser.IoOperationTiming;
	using SeekableRandomFile = pspsharp.filesystems.SeekableRandomFile;
	using Utilities = pspsharp.util.Utilities;

	public class LocalVirtualFile : AbstractVirtualFile
	{
		protected internal new SeekableRandomFile file;
		protected internal bool truncateAtNextWrite;

		public LocalVirtualFile(SeekableRandomFile file) : base(file)
		{
			this.file = file;
		}

		public override int ioWrite(TPointer inputPointer, int inputLength)
		{
			try
			{
				Utilities.write(file, inputPointer.Address, inputLength);
			}
			catch (IOException e)
			{
				log.error("ioWrite", e);
				return IO_ERROR;
			}

			return inputLength;
		}

		public override int ioWrite(sbyte[] inputBuffer, int inputOffset, int inputLength)
		{
			try
			{
				if (TruncateAtNextWrite)
				{
					// The file was open with PSP_O_TRUNC: truncate the file at the first write
					long position = Position;
					if (position < file.length())
					{
						file.Length = Position;
					}
					TruncateAtNextWrite = false;
				}

				file.write(inputBuffer, inputOffset, inputLength);
			}
			catch (IOException e)
			{
				log.error("ioWrite", e);
				return IO_ERROR;
			}

			return inputLength;
		}

		public override int ioIoctl(int command, TPointer inputPointer, int inputLength, TPointer outputPointer, int outputLength)
		{
			int result;
			switch (command)
			{
				case 0x00005001:
					if (inputLength != 0 || outputLength != 0)
					{
						result = IO_ERROR;
					}
					else
					{
						result = 0;
					}
					break;
				// Check if LoadExec is allowed on the file
				case 0x00208013:
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Checking if LoadExec is allowed on '{0}'", this));
					}
					// Result == 0: LoadExec allowed
					// Result != 0: LoadExec prohibited
					result = 0;
					break;
				// Check if LoadModule is allowed on the file
				case 0x00208003:
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Checking if LoadModule is allowed on '{0}'", this));
					}
					// Result == 0: LoadModule allowed
					// Result != 0: LoadModule prohibited
					result = 0;
					break;
				// Check if PRX type is allowed on the file
				case 0x00208081:
				case 0x00208082:
					if (log.DebugEnabled)
					{
						log.debug(string.Format("Checking if PRX type is allowed on '{0}'", this));
					}
					// Result == 0: PRX type allowed
					// Result != 0: PRX type prohibited
					result = 0;
					break;
				default:
					result = base.ioIoctl(command, inputPointer, inputLength, outputPointer, outputLength);
					break;
			}

			return result;
		}

		public virtual bool TruncateAtNextWrite
		{
			get
			{
				return truncateAtNextWrite;
			}
			set
			{
				this.truncateAtNextWrite = value;
			}
		}


		public override IVirtualFile duplicate()
		{
			try
			{
				return new LocalVirtualFile(new SeekableRandomFile(file.FileName, file.Mode));
			}
			catch (FileNotFoundException)
			{
			}

			return base.duplicate();
		}

		public override IDictionary<IoFileMgrForUser.IoOperation, IoFileMgrForUser.IoOperationTiming> Timings
		{
			get
			{
				return IoFileMgrForUser.noDelayTimings;
			}
		}

		public override string ToString()
		{
			return string.Format("LocalVirtualFile {0}", file);
		}
	}

}