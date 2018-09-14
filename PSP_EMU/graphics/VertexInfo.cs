using System;
using System.Text;

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
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using MemoryReader = pspsharp.memory.MemoryReader;

	// Based on soywiz/pspemulator
	public class VertexInfo
	{
		// vtype
		public const int vtypeMask = 0x009DDFFF;
		public bool transform2D;
		public int skinningWeightCount;
		public int morphingVertexCount;
		public int texture;
		public int color;
		public int normal;
		public int position;
		public int weight;
		public int index;
		public int vtype;

		// vaddr, iaddr
		public int ptr_vertex;
		public int ptr_index;

		// other data
		public int vertexSize;
		public int oneVertexSize;
		public int textureOffset;
		public int colorOffset;
		public int normalOffset;
		public int positionOffset;
		public int alignmentSize;
		public static int[] size_mapping = new int[]{0, 1, 2, 4};
		private static int[] size_padding = new int[]{0, 0, 1, 3};
		private static int[] color_size_mapping = new int[]{0, 1, 1, 1, 2, 2, 2, 4};
		private static int[] color_size_padding = new int[]{0, 0, 0, 0, 1, 1, 1, 3};
		private float[] morph_weight = new float[8];
		private static string[] texture_info = new string[]{null, "GU_TEXTURE_8BIT", "GU_TEXTURE_16BIT", "GU_TEXTURE_32BITF"};
		private static string[] color_info = new string[]{null, "GU_COLOR_UNK2", "GU_COLOR_UNK3", "GU_COLOR_UNK4", "GU_COLOR_5650", "GU_COLOR_5551", "GU_COLOR_4444", "GU_COLOR_8888"};
		private static string[] normal_info = new string[]{null, "GU_NORMAL_8BIT", "GU_NORMAL_16BIT", "GU_NORMAL_32BITF"};
		private static string[] vertex_info = new string[]{null, "GU_VERTEX_8BIT", "GU_VERTEX_16BIT", "GU_VERTEX_32BITF"};
		private static string[] weight_info = new string[]{null, "GU_WEIGHT_8BIT", "GU_WEIGHT_16BIT", "GU_WEIGHT_32BITF"};
		private static string[] index_info = new string[]{null, "GU_INDEX_8BIT", "GU_INDEX_16BIT", "GU_INDEX_UNK3"};
		private static string[] transform_info = new string[]{"GU_TRANSFORM_3D", "GU_TRANSFORM_2D"};
		private VertexInfoReaderTemplate vertexInfoReader;
		public bool readTexture;

		// cache data
		private int bufferId = -1; // id created by glGenBuffers
		private int vertexArrayId = -1;
		private int[] cachedVertices;
		private int[] cachedIndices;
		private int cachedNumberOfVertex;
		private float[] cachedMorphWeights;
		private float[][] cachedBoneMatrix;
		private ByteBuffer cachedBuffer;
		private VertexCache vertexCache;

		public VertexInfo()
		{
		}

		public VertexInfo(VertexInfo vertexInfo)
		{
			vtype = vertexInfo.vtype;
			transform2D = vertexInfo.transform2D;
			skinningWeightCount = vertexInfo.skinningWeightCount;
			morphingVertexCount = vertexInfo.morphingVertexCount;
			texture = vertexInfo.texture;
			color = vertexInfo.color;
			normal = vertexInfo.normal;
			position = vertexInfo.position;
			weight = vertexInfo.weight;
			index = vertexInfo.index;
			ptr_vertex = vertexInfo.ptr_vertex;
			ptr_index = vertexInfo.ptr_index;
			vertexSize = vertexInfo.vertexSize;
			oneVertexSize = vertexInfo.oneVertexSize;
			textureOffset = vertexInfo.textureOffset;
			colorOffset = vertexInfo.colorOffset;
			normalOffset = vertexInfo.normalOffset;
			positionOffset = vertexInfo.positionOffset;
			alignmentSize = vertexInfo.alignmentSize;
			morph_weight = vertexInfo.morph_weight;
			cachedIndices = vertexInfo.cachedIndices;
			cachedVertices = vertexInfo.cachedVertices;
			vertexCache = vertexInfo.vertexCache;
		}

		public virtual void processType(int param)
		{
			vtype = param & vtypeMask;

			updateVertexInfoReader(texture != 0);

			vertexInfoReader.processType(this);
		}

		public static void processType(VertexInfo vinfo, int vtype)
		{
			vinfo.vtype = vtype & vtypeMask;
			vinfo.texture = (vtype >> 0) & 0x3;
			vinfo.color = (vtype >> 2) & 0x7;
			vinfo.normal = (vtype >> 5) & 0x3;
			vinfo.position = (vtype >> 7) & 0x3;
			vinfo.weight = (vtype >> 9) & 0x3;
			vinfo.index = (vtype >> 11) & 0x3;
			vinfo.skinningWeightCount = ((vtype >> 14) & 0x7) + 1;
			vinfo.morphingVertexCount = ((vtype >> 18) & 0x7) + 1;
			vinfo.transform2D = ((vtype >> 23) & 0x1) != 0;

			int vertexSize = 0;
			vertexSize += size_mapping[vinfo.weight] * vinfo.skinningWeightCount;
			vertexSize = (vertexSize + size_padding[vinfo.texture]) & ~size_padding[vinfo.texture];

			vinfo.textureOffset = vertexSize;
			vertexSize += size_mapping[vinfo.texture] * 2;
			vertexSize = (vertexSize + color_size_padding[vinfo.color]) & ~color_size_padding[vinfo.color];

			vinfo.colorOffset = vertexSize;
			vertexSize += color_size_mapping[vinfo.color];
			vertexSize = (vertexSize + size_padding[vinfo.normal]) & ~size_padding[vinfo.normal];

			vinfo.normalOffset = vertexSize;
			vertexSize += size_mapping[vinfo.normal] * 3;
			vertexSize = (vertexSize + size_padding[vinfo.position]) & ~size_padding[vinfo.position];

			vinfo.positionOffset = vertexSize;
			vertexSize += size_mapping[vinfo.position] * 3;

			vinfo.alignmentSize = System.Math.Max(size_mapping[vinfo.weight], System.Math.Max(color_size_mapping[vinfo.color], System.Math.Max(size_mapping[vinfo.normal], System.Math.Max(size_mapping[vinfo.texture], size_mapping[vinfo.position]))));

			vertexSize = (vertexSize + vinfo.alignmentSize - 1) & ~(vinfo.alignmentSize - 1);
			vinfo.oneVertexSize = vertexSize;
			vinfo.vertexSize = vertexSize * vinfo.morphingVertexCount;
		}

		private void updateVertexInfoReader(bool readTexture)
		{
			this.readTexture = readTexture;
			vertexInfoReader = VertexInfoCompiler.Instance.getCompiledVertexInfoReader(vtype, readTexture);
		}

		public virtual int getAddress(Memory mem, int i)
		{
			if (ptr_index != 0 && index != 0)
			{
				int addr = ptr_index + i * index;
				switch (index)
				{
					case 1:
						i = mem.read8(addr);
						break; // GU_INDEX_8BIT
						goto case 2;
					case 2:
						i = mem.read16(addr);
						break; // GU_INDEX_16BIT
						goto case 3;
					case 3:
						i = mem.read32(addr);
						break; // GU_INDEX_UNK3 (assume 32bit)
					break;
				}
			}

			return ptr_vertex + i * vertexSize;
		}

		public virtual float[] MorphWeights
		{
			set
			{
				morph_weight = value;
    
				if (morphingVertexCount == 1)
				{
					morph_weight[0] = 1.0f;
				}
			}
		}

		public virtual VertexState readVertex(Memory mem, int addr, bool readTexture, bool doubleTexture2DCoords)
		{
			VertexState v = new VertexState();
			readVertex(mem, addr, v, readTexture, doubleTexture2DCoords);

			return v;
		}

		public virtual void readVertex(Memory mem, int addr, VertexState v, bool readTexture, bool doubleTexture2DCoords)
		{
			if (texture == 0)
			{
				readTexture = false;
			}
			if (readTexture != this.readTexture)
			{
				updateVertexInfoReader(readTexture);
			}
			vertexInfoReader.readVertex(mem, addr, v, morph_weight);

			// HD Remaster can require to double the 2D texture coordinates
			if (doubleTexture2DCoords && transform2D && readTexture)
			{
				v.t[0] *= 2f;
				v.t[1] *= 2f;
			}
		}

		public virtual void setDirty()
		{
			cachedIndices = null;
			cachedVertices = null;
		}

		private bool Equals(int[] a, int[] b)
		{
			if (a == null)
			{
				if (b != null)
				{
					return false;
				}
			}
			else
			{
				if (b == null)
				{
					return false;
				}

				if (a.Length != b.Length)
				{
					return false;
				}

				for (int i = 0; i < a.Length; i++)
				{
					if (a[i] != b[i])
					{
						return false;
					}
				}
			}

			return true;
		}

		public virtual bool Equals(VertexInfo vertexInfo, int numberOfVertex)
		{
			// Do not compare the vertices and indices of the new vertex if it has already
			// been checked during this display cycle
			if (!vertexCache.vertexAlreadyChecked(vertexInfo))
			{
				vertexInfo.readForCache(numberOfVertex);
				if (!Equals(cachedVertices, vertexInfo.cachedVertices))
				{
					return false;
				}
				if (!Equals(cachedIndices, vertexInfo.cachedIndices))
				{
					return false;
				}
				vertexCache.VertexAlreadyChecked = vertexInfo;
			}
			else
			{
				if (index != 0 && cachedIndices == null)
				{
					return false;
				}
				if (ptr_vertex != 0 && cachedVertices == null)
				{
					return false;
				}
			}

			return true;
		}

		public virtual bool Equals(VertexInfo vertexInfo, int numberOfVertex, float[][] boneMatrix, int numberOfWeightsForBuffer)
		{
			if (vtype != vertexInfo.vtype || cachedNumberOfVertex != numberOfVertex || ptr_index != vertexInfo.ptr_index)
			{
				return false;
			}

			if (morphingVertexCount > 1)
			{
				for (int i = 0; i < morphingVertexCount; i++)
				{
					if (cachedMorphWeights[i] != vertexInfo.morph_weight[i])
					{
						return false;
					}
				}
			}

			// Check if the bone matrix has changed, only if not using Skinning Shaders
			if (weight != 0 && numberOfWeightsForBuffer == 0 && boneMatrix != null)
			{
				for (int i = 0; i < skinningWeightCount; i++)
				{
					for (int j = 0; j < 12; j++)
					{
						if (cachedBoneMatrix[i][j] != boneMatrix[i][j])
						{
							return false;
						}
					}
				}
			}

			if (VideoEngine.Instance.useOptimisticVertexCache)
			{
				if (index != 0 && cachedIndices == null)
				{
					return false;
				}
				if (ptr_vertex != 0 && cachedVertices == null)
				{
					return false;
				}
				return true;
			}

			return Equals(vertexInfo, numberOfVertex);
		}

		public virtual bool bindVertex(IRenderingEngine re)
		{
			return bindVertex(re, false);
		}

		private bool bindVertex(IRenderingEngine re, bool bindBuffer)
		{
			bool needSetDataPointers;
			if (vertexArrayId >= 0)
			{
				re.bindVertexArray(vertexArrayId);
				needSetDataPointers = false;
				if (bindBuffer)
				{
					re.bindBuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_ARRAY_BUFFER, bufferId);
				}
			}
			else
			{
				re.bindBuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_ARRAY_BUFFER, bufferId);
				needSetDataPointers = true;
			}

			return needSetDataPointers;
		}

		public virtual bool loadVertex(IRenderingEngine re, float[] buffer, int size)
		{
			bool needSetDataPointers = false;

			if (vertexArrayId == -1 && re.VertexArrayAvailable)
			{
				vertexArrayId = re.genVertexArray();
				needSetDataPointers = true;
			}
			if (bufferId == -1)
			{
				bufferId = re.genBuffer();
				needSetDataPointers = true;
			}

			if (bindVertex(re, true))
			{
				needSetDataPointers = true;
			}

			int bufferSize = size * VideoEngine.SIZEOF_FLOAT;
			if (cachedBuffer == null || cachedBuffer.capacity() < bufferSize)
			{
				cachedBuffer = ByteBuffer.allocateDirect(bufferSize).order(ByteOrder.LITTLE_ENDIAN);
			}
			else
			{
				cachedBuffer.clear();
			}

			cachedBuffer.asFloatBuffer().put(buffer, 0, size);
			cachedBuffer.rewind();

			re.setBufferData(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_ARRAY_BUFFER, size * VideoEngine.SIZEOF_FLOAT, cachedBuffer, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_STATIC_DRAW);

			return needSetDataPointers;
		}

		public virtual void deleteVertex(IRenderingEngine re)
		{
			if (bufferId != -1)
			{
				re.deleteBuffer(bufferId);
				bufferId = -1;
			}
			if (vertexArrayId != -1)
			{
				re.deleteVertexArray(vertexArrayId);
				vertexArrayId = -1;
			}
			cachedMorphWeights = null;
			cachedBoneMatrix = null;
			cachedBuffer = null;
			cachedIndices = null;
			cachedVertices = null;
		}

		private void readForCache(int numberOfVertex)
		{
			if (cachedIndices != null || cachedVertices != null)
			{
				return;
			}

			int vertexArraySize;
			if (ptr_index != 0 && index != 0)
			{
				IMemoryReader memoryReader = null;
				switch (index)
				{
					case 1:
					{ // GU_INDEX_8BIT
						memoryReader = MemoryReader.getMemoryReader(ptr_index, 1 * numberOfVertex, 1);
						break;
					}
					case 2:
					{ // GU_INDEX_16BIT
						memoryReader = MemoryReader.getMemoryReader(ptr_index, 2 * numberOfVertex, 2);
						break;
					}
					case 3:
					{ // GU_INDEX_UNK3 (assume 32bit)
						memoryReader = MemoryReader.getMemoryReader(ptr_index, 4 * numberOfVertex, 4);
						break;
					}
				}

				// Remember the largest index
				int maxIndex = -1;
				if (memoryReader != null)
				{
					cachedIndices = new int[numberOfVertex];
					for (int i = 0; i < numberOfVertex; i++)
					{
						int index = memoryReader.readNext();
						cachedIndices[i] = index;
						if (index > maxIndex)
						{
							maxIndex = index;
						}
					}
				}

				// The vertex array extends only up to the largest index
				vertexArraySize = vertexSize * (maxIndex + 1);
			}
			else
			{
				vertexArraySize = vertexSize * numberOfVertex;
			}

			if (ptr_vertex != 0)
			{
				vertexArraySize = (vertexArraySize + 3) & ~3;
				cachedVertices = new int[vertexArraySize >> 2];
				IMemoryReader verticesReader = MemoryReader.getMemoryReader(ptr_vertex, vertexArraySize, 4);
				for (int i = 0; i < cachedVertices.Length; i++)
				{
					cachedVertices[i] = verticesReader.readNext();
				}
			}
		}

		public virtual void prepareForCache(VertexCache vertexCache, int numberOfVertex, float[][] boneMatrix, int numberOfWeightsForBuffer)
		{
			this.vertexCache = vertexCache;
			cachedNumberOfVertex = numberOfVertex;

			cachedMorphWeights = new float[morphingVertexCount];
			Array.Copy(morph_weight, 0, cachedMorphWeights, 0, morphingVertexCount);

			if (weight != 0 && numberOfWeightsForBuffer == 0 && boneMatrix != null)
			{
				cachedBoneMatrix = new float[skinningWeightCount][];
				for (int i = 0; i < skinningWeightCount; i++)
				{
					cachedBoneMatrix[i] = new float[12];
					Array.Copy(boneMatrix[i], 0, cachedBoneMatrix[i], 0, 12);
				}
			}
			else
			{
				cachedBoneMatrix = null;
			}

			readForCache(numberOfVertex);
		}

		public virtual void reuseCachedBuffer(VertexInfo vertexInfo)
		{
			if (vertexInfo != null && vertexInfo.cachedBuffer != null)
			{
				// Reuse the cachedBuffer if we don't have one or if we have a smaller one
				if (cachedBuffer == null || cachedBuffer.capacity() < vertexInfo.cachedBuffer.capacity())
				{
					// Reuse the cachedBuffer
					cachedBuffer = vertexInfo.cachedBuffer;
					vertexInfo.cachedBuffer = null;
				}
			}
		}

		public static string ToString(int texture, int color, int normal, int position, int weight, int skinningWeightCount, int morphingVertexCount, int index, bool transform2D, int vertexSize)
		{
			StringBuilder sb = new StringBuilder();

			if (!string.ReferenceEquals(texture_info[texture], null))
			{
				sb.Append(texture_info[texture] + "|");
			}
			if (!string.ReferenceEquals(color_info[color], null))
			{
				sb.Append(color_info[color] + "|");
			}
			if (!string.ReferenceEquals(normal_info[normal], null))
			{
				sb.Append(normal_info[normal] + "|");
			}
			if (!string.ReferenceEquals(vertex_info[position], null))
			{
				sb.Append(vertex_info[position] + "|");
			}
			if (!string.ReferenceEquals(weight_info[weight], null))
			{
				sb.Append(weight_info[weight] + "|");
				sb.Append("GU_WEIGHTS(" + skinningWeightCount + ")|");
			}
			if (morphingVertexCount > 1)
			{
				sb.Append("GU_VERTICES(" + morphingVertexCount + ")|");
			}
			if (!string.ReferenceEquals(index_info[index], null))
			{
				sb.Append(index_info[index] + "|");
			}
			sb.Append(transform_info[transform2D ? 1 : 0]);
			sb.Append(" size=" + vertexSize);
			return sb.ToString();
		}

		public override string ToString()
		{
			return ToString(texture, color, normal, position, weight, skinningWeightCount, morphingVertexCount, index, transform2D, vertexSize);
		}
	}

}