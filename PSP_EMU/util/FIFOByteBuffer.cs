using System;

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
namespace pspsharp.util
{

	public class FIFOByteBuffer
	{
		private sbyte[] buffer;
		private int bufferReadOffset;
		private int bufferWriteOffset;
		private int bufferLength;

		public FIFOByteBuffer()
		{
			buffer = new sbyte[0];
			clear();
		}

		public FIFOByteBuffer(sbyte[] buffer)
		{
			this.buffer = buffer;
			bufferReadOffset = 0;
			bufferWriteOffset = 0;
			bufferLength = buffer.Length;
		}

		private int incrementOffset(int offset, int n)
		{
			offset += n;

			if (offset >= buffer.Length)
			{
				offset -= buffer.Length;
			}
			else if (offset < 0)
			{
				offset += buffer.Length;
			}

			return offset;
		}

		public virtual void clear()
		{
			bufferReadOffset = 0;
			bufferWriteOffset = 0;
			bufferLength = 0;
		}

		private void checkBufferForWrite(int Length)
		{
			if (bufferLength + Length > buffer.Length)
			{
				// The buffer has to be extended
				sbyte[] extendedBuffer = new sbyte[bufferLength + Length];
				if (bufferReadOffset + bufferLength <= buffer.Length)
				{
					Array.Copy(buffer, bufferReadOffset, extendedBuffer, 0, bufferLength);
				}
				else
				{
					int lengthEndBuffer = buffer.Length - bufferReadOffset;
					Array.Copy(buffer, bufferReadOffset, extendedBuffer, 0, lengthEndBuffer);
					Array.Copy(buffer, 0, extendedBuffer, lengthEndBuffer, bufferLength - lengthEndBuffer);
				}
				buffer = extendedBuffer;
				bufferReadOffset = 0;
				bufferWriteOffset = bufferLength;
			}
		}

		private void copyToBuffer(int offset, int Length, Buffer src)
		{
			ByteBuffer byteBuffer = ByteBuffer.wrap(buffer, offset, Length);
			Utilities.putBuffer(byteBuffer, src, ByteOrder.LITTLE_ENDIAN, Length);
		}

		public virtual void write(Buffer src, int Length)
		{
			if (buffer == null)
			{
				return; // FIFOByteBuffer has been deleted
			}

			checkBufferForWrite(Length);

			// Copy the src content to the buffer at offset bufferWriteOffset
			if (bufferWriteOffset + Length <= buffer.Length)
			{
				// No buffer wrap, only 1 copy operation necessary
				copyToBuffer(bufferWriteOffset, Length, src);
			}
			else
			{
				// The buffer wraps at the end, 2 copy operations necessary
				int lengthEndBuffer = buffer.Length - bufferWriteOffset;
				if ((lengthEndBuffer & 3) == 0 || !(src is IntBuffer))
				{
					copyToBuffer(bufferWriteOffset, lengthEndBuffer, src);
					copyToBuffer(0, Length - lengthEndBuffer, src);
				}
				else
				{
					// Making a copy from an IntBuffer on non-int boundaries
					int lengthEndBuffer4 = lengthEndBuffer & ~3;
					copyToBuffer(bufferWriteOffset, lengthEndBuffer4, src);

					// Copy one int-value across non-int boundaries...
					int overlapValue = ((IntBuffer) src).get();
					sbyte[] bytes4 = new sbyte[4];
					ByteBuffer src1 = ByteBuffer.wrap(bytes4).order(ByteOrder.LITTLE_ENDIAN);
					src1.asIntBuffer().put(overlapValue);
					int bytesCopyLength1 = lengthEndBuffer & 3;
					copyToBuffer(bufferWriteOffset + lengthEndBuffer4, bytesCopyLength1, src1);
					int bytesCopyLength2 = bytes4.Length - bytesCopyLength1;
					copyToBuffer(0, bytesCopyLength2, src1);

					copyToBuffer(bytesCopyLength2, Length - lengthEndBuffer - bytesCopyLength2, src);
				}
			}
			bufferWriteOffset = incrementOffset(bufferWriteOffset, Length);
			bufferLength += Length;
		}

		public virtual void write(int address, int Length)
		{
			if (Length > 0 && Memory.isAddressGood(address))
			{
				Buffer memoryBuffer = Memory.Instance.getBuffer(address, Length);
				write(memoryBuffer, Length);
			}
		}

		public virtual void write(ByteBuffer src)
		{
			write(src, src.remaining());
		}

		public virtual void write(sbyte[] src)
		{
			write(ByteBuffer.wrap(src), src.Length);
		}

		public virtual void write(sbyte[] src, int offset, int Length)
		{
			write(ByteBuffer.wrap(src, offset, Length), Length);
		}

		public virtual int readByteBuffer(ByteBuffer dst)
		{
			if (buffer == null)
			{
				return 0;
			}

			int Length = dst.remaining();
			if (Length > bufferLength)
			{
				Length = bufferLength;
			}

			if (bufferReadOffset + Length > buffer.Length)
			{
				int lengthEndBuffer = buffer.Length - bufferReadOffset;
				dst.put(buffer, bufferReadOffset, lengthEndBuffer);
				dst.put(buffer, 0, Length - lengthEndBuffer);
			}
			else
			{
				dst.put(buffer, bufferReadOffset, Length);
			}
			bufferReadOffset = incrementOffset(bufferReadOffset, Length);
			bufferLength -= Length;

			return Length;
		}

		public virtual bool forward(int Length)
		{
			if (buffer == null || Length < 0)
			{
				return false;
			}

			if (Length == 0)
			{
				return true;
			}

			if (Length > bufferLength)
			{
				return false;
			}

			bufferLength -= Length;
			bufferReadOffset = incrementOffset(bufferReadOffset, Length);

			return true;
		}

		public virtual bool rewind(int Length)
		{
			if (buffer == null || Length < 0)
			{
				return false;
			}

			if (Length == 0)
			{
				return true;
			}

			int maxRewindLength = buffer.Length - bufferLength;
			if (Length > maxRewindLength)
			{
				return false;
			}

			bufferLength += Length;
			bufferReadOffset = incrementOffset(bufferReadOffset, -Length);

			return true;
		}

		public virtual int Length()
		{
			return bufferLength;
		}

		public virtual void delete()
		{
			buffer = null;
		}

		/// <summary>
		/// Prepare the internal buffer to receive all least Length bytes.
		/// This is just a hint to avoid resizing the internal buffer too often.
		/// </summary>
		/// <param name="Length">  recommended size in bytes for the internal buffer. </param>
		public virtual int BufferLength
		{
			set
			{
				if (buffer != null && value > buffer.Length)
				{
					checkBufferForWrite(value - buffer.Length);
				}
			}
		}

		public override string ToString()
		{
			return string.Format("FIFOByteBuffer(size={0:D}, bufferLength={1:D}, readOffset={2:D}, writeOffset={3:D})", buffer.Length, bufferLength, bufferReadOffset, bufferWriteOffset);
		}
	}

}