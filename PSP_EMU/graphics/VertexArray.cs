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
namespace pspsharp.graphics
{
	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class VertexArray
	{
		private int id = -1;
		private int vtype;
		private VertexBuffer vertexBuffer;
		private int stride;
		private bool pendingReload = false;

		public VertexArray(int vtype, VertexBuffer vertexBuffer, int stride)
		{
			this.vtype = vtype & VertexInfo.vtypeMask;
			this.vertexBuffer = vertexBuffer;
			this.stride = stride;
		}

		public virtual bool bind(IRenderingEngine re)
		{
			bool needSetDataPointers = pendingReload;
			pendingReload = false;
			if (id == -1)
			{
				id = re.genVertexArray();
				needSetDataPointers = true;
			}
			re.bindVertexArray(id);

			return needSetDataPointers;
		}

		public virtual void delete(IRenderingEngine re)
		{
			re.deleteVertexArray(id);
			id = -1;
		}

		public virtual bool isMatching(int vtype, VertexBuffer vertexBuffer, int address, int stride)
		{
			if (this.vertexBuffer != vertexBuffer || this.stride != stride)
			{
				return false;
			}

			if (this.vtype != (vtype & VertexInfo.vtypeMask))
			{
				return false;
			}

			if ((vertexBuffer.getBufferOffset(address) % stride) != 0)
			{
				return false;
			}

			return true;
		}

		public virtual int getVertexOffset(int address)
		{
			return vertexBuffer.getBufferOffset(address) / stride;
		}

		public virtual VertexBuffer VertexBuffer
		{
			get
			{
				return vertexBuffer;
			}
		}

		public virtual void forceReload()
		{
			pendingReload = true;
		}

		public override string ToString()
		{
			VertexInfo vinfo = new VertexInfo();
			vinfo.processType(vtype);
			return string.Format("VertexArray[{0}, stride {1:D}, id {2:D}, {3}]", vinfo, stride, id, vertexBuffer);
		}
	}

}