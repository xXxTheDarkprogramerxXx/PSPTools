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

		private void checkBufferForWrite(int length)
		{
			if (bufferLength + length > buffer.Length)
			{
				// The buffer has to be extended
				sbyte[] extendedBuffer = new sbyte[bufferLength + length];
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

		private void copyToBuffer(int offset, int length, Buffer src)
		{
			ByteBuffer byteBuffer = ByteBuffer.wrap(buffer, offset, length);
			Utilities.putBuffer(byteBuffer, src, ByteOrder.LITTLE_ENDIAN, length);
		}

		public virtual void write(Buffer src, int length)
		{
			if (buffer == null)
			{
				return; // FIFOByteBuffer has been deleted
			}

			checkBufferForWrite(length);

			// Copy the src content to the buffer at offset bufferWriteOffset
			if (bufferWriteOffset + length <= buffer.Length)
			{
				// No buffer wrap, only 1 copy operation necessary
				copyToBuffer(bufferWriteOffset, length, src);
			}
			else
			{
				// The buffer wraps at the end, 2 copy operations necessary
				int lengthEndBuffer = buffer.Length - bufferWriteOffset;
				if ((lengthEndBuffer & 3) == 0 || !(src is IntBuffer))
				{
					copyToBuffer(bufferWriteOffset, lengthEndBuffer, src);
					copyToBuffer(0, length - lengthEndBuffer, src);
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

					copyToBuffer(bytesCopyLength2, length - lengthEndBuffer - bytesCopyLength2, src);
				}
			}
			bufferWriteOffset = incrementOffset(bufferWriteOffset, length);
			bufferLength += length;
		}

		public virtual void write(int address, int length)
		{
			if (length > 0 && Memory.isAddressGood(address))
			{
				Buffer memoryBuffer = Memory.Instance.getBuffer(address, length);
				write(memoryBuffer, length);
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

		public virtual void write(sbyte[] src, int offset, int length)
		{
			write(ByteBuffer.wrap(src, offset, length), length);
		}

		public virtual int readByteBuffer(ByteBuffer dst)
		{
			if (buffer == null)
			{
				return 0;
			}

			int length = dst.remaining();
			if (length > bufferLength)
			{
				length = bufferLength;
			}

			if (bufferReadOffset + length > buffer.Length)
			{
				int lengthEndBuffer = buffer.Length - bufferReadOffset;
				dst.put(buffer, bufferReadOffset, lengthEndBuffer);
				dst.put(buffer, 0, length - lengthEndBuffer);
			}
			else
			{
				dst.put(buffer, bufferReadOffset, length);
			}
			bufferReadOffset = incrementOffset(bufferReadOffset, length);
			bufferLength -= length;

			return length;
		}

		public virtual bool forward(int length)
		{
			if (buffer == null || length < 0)
			{
				return false;
			}

			if (length == 0)
			{
				return true;
			}

			if (length > bufferLength)
			{
				return false;
			}

			bufferLength -= length;
			bufferReadOffset = incrementOffset(bufferReadOffset, length);

			return true;
		}

		public virtual bool rewind(int length)
		{
			if (buffer == null || length < 0)
			{
				return false;
			}

			if (length == 0)
			{
				return true;
			}

			int maxRewindLength = buffer.Length - bufferLength;
			if (length > maxRewindLength)
			{
				return false;
			}

			bufferLength += length;
			bufferReadOffset = incrementOffset(bufferReadOffset, -length);

			return true;
		}

		public virtual int length()
		{
			return bufferLength;
		}

		public virtual void delete()
		{
			buffer = null;
		}

		/// <summary>
		/// Prepare the internal buffer to receive all least length bytes.
		/// This is just a hint to avoid resizing the internal buffer too often.
		/// </summary>
		/// <param name="length">  recommended size in bytes for the internal buffer. </param>
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