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
namespace pspsharp.graphics.RE.buffer
{


	//using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public abstract class BaseBufferManager : IREBufferManager
	{
		public abstract void setBufferSubData(int target, int buffer, int offset, int size, Buffer data, int usage);
		public abstract void setBufferData(int target, int buffer, int size, Buffer data, int usage);
		public abstract void setVertexAttribPointer(int buffer, int id, int size, int type, bool normalized, int stride, int offset);
		public abstract void setWeightPointer(int buffer, int size, int type, int stride, int offset);
		public abstract void setNormalPointer(int buffer, int type, int stride, int offset);
		public abstract void setVertexPointer(int buffer, int size, int type, int stride, int offset);
		public abstract void setColorPointer(int buffer, int size, int type, int stride, int offset);
		public abstract void setTexCoordPointer(int buffer, int size, int type, int stride, int offset);
		public abstract void bindBuffer(int target, int buffer);
		public abstract int genBuffer(int target, int type, int size, int usage);
		public abstract bool useVBO();
		protected internal static readonly Logger log = VideoEngine.log_Renamed;
		protected internal IRenderingEngine re;
		protected internal Dictionary<int, BufferInfo> buffers;
		protected internal static readonly int[] sizeOfType = new int[] {1, 1, 2, 2, 4, 4, 4, 8};

		protected internal class BufferInfo
		{
			public int buffer;
			public ByteBuffer byteBuffer;
			public Buffer typedBuffer;
			public int type;
			public int size;
			public int usage;
			public int elementSize;
			public int totalSize;

			public BufferInfo(int buffer, ByteBuffer byteBuffer, int type, int size)
			{
				this.buffer = buffer;
				this.byteBuffer = byteBuffer;
				this.type = type;
				this.size = size;
				elementSize = sizeOfType[type];
				totalSize = size * elementSize;

				typedBuffer = byteBuffer;
				switch (type)
				{
					case pspsharp.graphics.RE.IRenderingEngine_Fields.RE_BYTE:
					case pspsharp.graphics.RE.IRenderingEngine_Fields.RE_UNSIGNED_BYTE:
						break;
					case pspsharp.graphics.RE.IRenderingEngine_Fields.RE_SHORT:
					case pspsharp.graphics.RE.IRenderingEngine_Fields.RE_UNSIGNED_SHORT:
						typedBuffer = byteBuffer.asShortBuffer();
						break;
					case pspsharp.graphics.RE.IRenderingEngine_Fields.RE_INT:
					case pspsharp.graphics.RE.IRenderingEngine_Fields.RE_UNSIGNED_INT:
						typedBuffer = byteBuffer.asIntBuffer();
						break;
					case pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FLOAT:
						typedBuffer = byteBuffer.asFloatBuffer();
						break;
					case pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DOUBLE:
						typedBuffer = byteBuffer.asDoubleBuffer();
						break;
				}
			}

			public virtual Buffer getBufferPosition(int offset)
			{
				return typedBuffer.position(offset / elementSize);
			}

			public virtual int BufferSize
			{
				get
				{
					return totalSize;
				}
			}
		}

		public BaseBufferManager()
		{
			init();
		}

		protected internal virtual void init()
		{
			buffers = new Dictionary<int, BufferInfo>();
		}

		public virtual IRenderingEngine RenderingEngine
		{
			set
			{
				this.re = value;
			}
		}

		protected internal virtual ByteBuffer createByteBuffer(int size)
		{
			return ByteBuffer.allocateDirect(size).order(ByteOrder.nativeOrder());
		}

		public virtual void deleteBuffer(int buffer)
		{
			buffers.Remove(buffer);
		}

		public virtual ByteBuffer getBuffer(int buffer)
		{
			return buffers[buffer].byteBuffer;
		}
	}

}