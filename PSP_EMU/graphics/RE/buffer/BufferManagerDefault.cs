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
namespace pspsharp.graphics.RE.buffer
{

	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class BufferManagerDefault : BaseBufferManager
	{
		protected internal int currentBufferId;

		protected internal override void init()
		{
			base.init();
			currentBufferId = 12345678;
		}

		public override bool useVBO()
		{
			return false;
		}

		public override int genBuffer(int target, int type, int size, int usage)
		{
			int totalSize = size * sizeOfType[type];
			ByteBuffer byteBuffer = createByteBuffer(totalSize);

			int buffer = currentBufferId++;

			buffers[buffer] = new BufferInfo(buffer, byteBuffer, type, size);

			return buffer;
		}

		public override void bindBuffer(int target, int buffer)
		{
			// Not supported
		}

		public override void setColorPointer(int buffer, int size, int type, int stride, int offset)
		{
			BufferInfo bufferInfo = buffers[buffer];
			re.setColorPointer(size, type, stride, bufferInfo.BufferSize - offset, bufferInfo.getBufferPosition(offset));
		}

		public override void setNormalPointer(int buffer, int type, int stride, int offset)
		{
			BufferInfo bufferInfo = buffers[buffer];
			re.setNormalPointer(type, stride, bufferInfo.BufferSize - offset, bufferInfo.getBufferPosition(offset));
		}

		public override void setTexCoordPointer(int buffer, int size, int type, int stride, int offset)
		{
			BufferInfo bufferInfo = buffers[buffer];
			re.setTexCoordPointer(size, type, stride, bufferInfo.BufferSize - offset, bufferInfo.getBufferPosition(offset));
		}

		public override void setVertexAttribPointer(int buffer, int id, int size, int type, bool normalized, int stride, int offset)
		{
			BufferInfo bufferInfo = buffers[buffer];
			re.setVertexAttribPointer(id, size, type, normalized, stride, bufferInfo.BufferSize - offset, bufferInfo.getBufferPosition(offset));
		}

		public override void setVertexPointer(int buffer, int size, int type, int stride, int offset)
		{
			BufferInfo bufferInfo = buffers[buffer];
			re.setVertexPointer(size, type, stride, bufferInfo.BufferSize - offset, bufferInfo.getBufferPosition(offset));
		}

		public override void setWeightPointer(int buffer, int size, int type, int stride, int offset)
		{
			BufferInfo bufferInfo = buffers[buffer];
			re.setWeightPointer(size, type, stride, bufferInfo.BufferSize - offset, bufferInfo.getBufferPosition(offset));
		}

		public override void setBufferData(int target, int buffer, int size, Buffer data, int usage)
		{
			BufferInfo bufferInfo = buffers[buffer];
			if (bufferInfo.byteBuffer != data)
			{
				bufferInfo.byteBuffer.clear();
				Utilities.putBuffer(bufferInfo.byteBuffer, data, ByteOrder.nativeOrder());
			}
			else
			{
				bufferInfo.byteBuffer.position(0);
			}
		}

		public override void setBufferSubData(int target, int buffer, int offset, int size, Buffer data, int usage)
		{
			BufferInfo bufferInfo = buffers[buffer];
			if (bufferInfo.byteBuffer != data)
			{
				bufferInfo.byteBuffer.clear();
				Utilities.putBuffer(bufferInfo.byteBuffer, data, ByteOrder.nativeOrder());
			}
			else
			{
				bufferInfo.byteBuffer.position(0);
			}
		}
	}

}