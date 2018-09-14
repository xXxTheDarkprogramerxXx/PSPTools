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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.IRenderingEngine_Fields.RE_ARRAY_BUFFER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.round4;

	using Settings = pspsharp.settings.Settings;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class BufferManagerVBO : BaseBufferManager
	{
		private int[] bufferDataSize;

		public static bool useVBO(IRenderingEngine re)
		{
			return !Settings.Instance.readBool("emu.disablevbo") && re.isExtensionAvailable("GL_ARB_vertex_buffer_object");
		}

		protected internal override void init()
		{
			// Start with 100 possible buffer entries.
			// The array will be dynamically extended if more entries are required.
			bufferDataSize = new int[100];

			base.init();
			log.info("Using VBO");
		}

		public override bool useVBO()
		{
			return true;
		}

		public override int genBuffer(int target, int type, int size, int usage)
		{
			int totalSize = size * sizeOfType[type];
			ByteBuffer byteBuffer = createByteBuffer(totalSize);

			int buffer = re.genBuffer();
			if (buffer >= bufferDataSize.Length)
			{
				bufferDataSize = Utilities.extendArray(bufferDataSize, buffer - bufferDataSize.Length + 1);
			}
			setBufferData(target, buffer, totalSize, byteBuffer, usage);

			buffers[buffer] = new BufferInfo(buffer, byteBuffer, type, size);

			return buffer;
		}

		public override void bindBuffer(int target, int buffer)
		{
			re.bindBuffer(target, buffer);
		}

		public override void deleteBuffer(int buffer)
		{
			re.deleteBuffer(buffer);
			base.deleteBuffer(buffer);
		}

		public override void setColorPointer(int buffer, int size, int type, int stride, int offset)
		{
			bindBuffer(RE_ARRAY_BUFFER, buffer);
			re.setColorPointer(size, type, stride, offset);
		}

		public override void setNormalPointer(int buffer, int type, int stride, int offset)
		{
			bindBuffer(RE_ARRAY_BUFFER, buffer);
			re.setNormalPointer(type, stride, offset);
		}

		public override void setTexCoordPointer(int buffer, int size, int type, int stride, int offset)
		{
			bindBuffer(RE_ARRAY_BUFFER, buffer);
			re.setTexCoordPointer(size, type, stride, offset);
		}

		public override void setVertexAttribPointer(int buffer, int id, int size, int type, bool normalized, int stride, int offset)
		{
			bindBuffer(RE_ARRAY_BUFFER, buffer);
			re.setVertexAttribPointer(id, size, type, normalized, stride, offset);
		}

		public override void setVertexPointer(int buffer, int size, int type, int stride, int offset)
		{
			bindBuffer(RE_ARRAY_BUFFER, buffer);
			re.setVertexPointer(size, type, stride, offset);
		}

		public override void setWeightPointer(int buffer, int size, int type, int stride, int offset)
		{
			bindBuffer(RE_ARRAY_BUFFER, buffer);
			re.setWeightPointer(size, type, stride, offset);
		}

		public override void setBufferData(int target, int buffer, int size, Buffer data, int usage)
		{
			bindBuffer(target, buffer);
			re.setBufferData(target, size, data, usage);
			bufferDataSize[buffer] = size;
		}

		public override void setBufferSubData(int target, int buffer, int offset, int size, Buffer data, int usage)
		{
			bindBuffer(target, buffer);

			// Some drivers seem to require an aligned buffer data size to handle correctly unaligned data.
			int requiredBufferDataSize = round4(offset) + round4(size);
			if (requiredBufferDataSize > bufferDataSize[buffer])
			{
				setBufferData(target, buffer, requiredBufferDataSize, null, usage);
			}

			re.setBufferSubData(target, offset, size, data);
		}
	}

}