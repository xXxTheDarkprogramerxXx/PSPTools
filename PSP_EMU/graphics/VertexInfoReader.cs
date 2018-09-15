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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.IRenderingEngine_Fields.RE_BYTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FLOAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.IRenderingEngine_Fields.RE_INT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.IRenderingEngine_Fields.RE_SHORT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.IRenderingEngine_Fields.RE_UNSIGNED_BYTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.IRenderingEngine_Fields.RE_UNSIGNED_SHORT;


	//using Logger = org.apache.log4j.Logger;

	using BufferedMemoryReader = pspsharp.memory.BufferedMemoryReader;
	using ImageReader = pspsharp.memory.ImageReader;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class VertexInfoReader
	{
		private static Logger log = VideoEngine.log_Renamed;
		private VideoEngine videoEngine;
		private BufferedMemoryReader memoryReader;
		private VertexInfo vertexInfo;
		private IVertexDataBuffer vertexDataBuffer;
		private int weightOffset;
		private int weightType;
		private int weightNumberValues;
		private int textureOffset;
		private int textureType;
		private int textureNumberValues;
		private int colorOffset;
		private int colorType;
		private int colorNumberValues;
		private int normalOffset;
		private int normalType;
		private int normalNumberValues;
		private int positionOffset;
		private int positionType;
		private int positionNumberValues;
		private int stride;
		private bool weightNative;
		private bool textureNative;
		private bool colorNative;
		private bool normalNative;
		private bool positionNative;
		private float[] boneWeights = new float[8];
		private float[] normal = new float[3];
		private float[] position = new float[3];
		private bool canAllNativeVertexInfo;

		private const int typeNone = -1;

		// Readers skipping the padding at the end of a vertex element,
		// indexed by the alignment (1 = byte-aligned, 2 = short-aligned, 4 = int-aligned)
		private final IVertexInfoReader[] paddingReaders = new IVertexInfoReader[] { new NotImplementedReader("Padding 0")
								, new NopReader(), new AlignShortReader(), new NotImplementedReader("Padding 3"), new AlignIntReader() // Alignment on 32 bit boundary
	};

		// Readers skipping 2 elements, indexed by the element type (1 = byte, 2 = short, 3 = float)
		private final IVertexInfoReader[] skip2Readers = new IVertexInfoReader[] { new NopReader()
								, new Skip2BytesReader(), new Skip2ShortsReader(), new Skip2FloatsReader()
};

		// Readers skipping 3 elements, indexed by the element type (1 = byte, 2 = short, 3 = float)
		private final IVertexInfoReader[] skip3Readers = new IVertexInfoReader[] { new NopReader()
								, new Skip3BytesReader(), new Skip3ShortsReader(), new Skip3FloatsReader()
								 };

		// Readers skipping a color element, indexed by the color type
		private final IVertexInfoReader[] skipColorReaders = new IVertexInfoReader[] { new NopReader()
								, new NotImplementedReader("Color 1"), new NotImplementedReader("Color 2"), new NotImplementedReader("Color 3"), new Skip1ShortReader(), new Skip1ShortReader(), new Skip1ShortReader(), new Skip1IntReader() // GU_COLOR_8888
								 };

		// Readers reading a texture element, indexed by the texture type
		private final IVertexInfoReader[] textureReaders = new IVertexInfoReader[] { new NopReader()
								, new Texture1Reader(), new Texture2Reader(), new Texture3Reader()
								 };

		// Readers reading a color element, indexed by the color type
		private final IVertexInfoReader[] colorReaders = new IVertexInfoReader[] { new NopReader()
								, new NotImplementedReader("Color 1"), new NotImplementedReader("Color 2"), new NotImplementedReader("Color 3"), new Color4Reader(), new Color5Reader(), new Color6Reader(), new Color7Reader() // GU_COLOR_8888
								 };

		// Readers reading a normal element, indexed by the normal type
		private final IVertexInfoReader[] normalReaders = new IVertexInfoReader[] { new NopReader()
								, new Normal1Reader(), new Normal2Reader(), new Normal3Reader()
								 };

		// Readers reading a position (vertex) element, indexed by the position type
		private final IVertexInfoReader[] positionReaders = new IVertexInfoReader[] { new NopReader()
								, new Position1Reader(), new Position2Reader(), new Position3Reader()
								 };

		// Readers reading a weight element, indexed by the weight type
		private final IVertexInfoReader[] weightReaders = new IVertexInfoReader[] { new NopReader()
								, new Weight1Reader(), new Weight2Reader(), new Weight3Reader()
								 };

		// Readers skipping a weight element, indexed by the weight type
		private final IVertexInfoReader[] skipWeightReaders = new IVertexInfoReader[] { new NopReader()
								, new SkipWeight1Reader(), new SkipWeight2Reader(), new SkipWeight3Reader()
								 };

		public VertexInfoReader()
		{
		}

		private void setAddress(int address)
		{
			memoryReader = new BufferedMemoryReader(address);
		}

		/// <summary>
		/// Sets the "native", "type" and "offset" attributes for all the vertex elements
		/// (texture, color, normal and position).
		/// Computes the stride.
		/// </summary>
		private void update()
		{
			stride = 0;

			// Weight
			IVertexInfoReader weightReader = getWeightReader(false);
			weightNative = weightReader.Native;
			weightType = weightReader.type();
			weightOffset = 0;
			weightNumberValues = weightReader.numberValues();
			int weightSize = weightReader.size();
			stride += weightSize;

			// Texture
			IVertexInfoReader textureReader = getTextureReader(false);
			textureNative = textureReader.Native;
			textureType = textureReader.type();
			textureOffset = (textureNative ? vertexInfo.textureOffset : stride);
			textureNumberValues = textureReader.numberValues();
			int textureSize = textureReader.size();
			stride += textureSize;

			// Color
			IVertexInfoReader colorReader = getColorReader(false);
			colorNative = colorReader.Native;
			colorType = colorReader.type();
			colorOffset = (colorNative ? vertexInfo.colorOffset : stride);
			colorNumberValues = colorReader.numberValues();
			int colorSize = colorReader.size();
			stride += colorSize;

			// Normal
			IVertexInfoReader normalReader = getNormalReader(false);
			normalNative = normalReader.Native;
			normalType = normalReader.type();
			normalOffset = (normalNative ? vertexInfo.normalOffset : stride);
			normalNumberValues = normalReader.numberValues();
			int normalSize = normalReader.size();
			stride += normalSize;

			// Position
			IVertexInfoReader positionReader = getPositionReader(false);
			positionNative = positionReader.Native;
			positionType = positionReader.type();
			positionOffset = (positionNative ? vertexInfo.positionOffset : stride);
			positionNumberValues = positionReader.numberValues();
			int positionSize = positionReader.size();
			stride += positionSize;
		}

		public void addNativeOffset(int offset)
		{
			if (offset != 0)
			{
				if (weightNative)
				{
					weightOffset += offset;
				}
				if (textureNative)
				{
					textureOffset += offset;
				}
				if (colorNative)
				{
					colorOffset += offset;
				}
				if (normalNative)
				{
					normalOffset += offset;
				}
				if (positionNative)
				{
					positionOffset += offset;
				}
			}
		}

		public int WeightOffset
		{
			return weightOffset;
		}

		public int WeightType
		{
			return weightType;
		}

		public int WeightNumberValues
		{
			return weightNumberValues;
		}

		public int TextureOffset
		{
			return textureOffset;
		}

		public int TextureType
		{
			return textureType;
		}

		public int TextureNumberValues
		{
			return textureNumberValues;
		}

		public int ColorOffset
		{
			return colorOffset;
		}

		public int ColorType
		{
			return colorType;
		}

		public int ColorNumberValues
		{
			return colorNumberValues;
		}

		public int NormalOffset
		{
			return normalOffset;
		}

		public int NormalType
		{
			return normalType;
		}

		public int NormalNumberValues
		{
			return normalNumberValues;
		}

		public int PositionOffset
		{
			return positionOffset;
		}

		public int PositionType
		{
			return positionType;
		}

		public int PositionNumberValues
		{
			return positionNumberValues;
		}

		public int Stride
		{
			return stride;
		}

		public bool WeightNative
		{
			return weightNative;
		}

		public bool TextureNative
		{
			return textureNative;
		}

		public bool ColorNative
		{
			return colorNative;
		}

		public bool NormalNative
		{
			return normalNative;
		}

		public bool PositionNative
		{
			return positionNative;
		}

		/// <returns> true if the vertex has at least one native element, false otherwise. </returns>
		public bool hasNative()
		{
			if (textureNative && vertexInfo.texture != 0)
			{
				return true;
			}

			if (colorNative && vertexInfo.color != 0)
			{
				return true;
			}

			if (normalNative && vertexInfo.normal != 0)
			{
				return true;
			}

			if (positionNative && vertexInfo.position != 0)
			{
				return true;
			}

			return false;
		}

		/// <returns> true if the vertex has only native element, false otherwise. </returns>
		public bool AllNative
		{
			return textureNative && colorNative && normalNative && positionNative;
		}

		/// <summary>
		/// Reads a sequence of VertexInfo structures.
		/// </summary>
		/// <param name="vertexInfo">		The VertexInfo prepared by the command list </param>
		/// <param name="address">			The start address of the structures </param>
		/// <param name="numberOfVertex">	The number of VertexInfo to read
		/// @return					A Buffer containing all the non-native VertexInfo
		///							elements. The native elements are not included.
		///							Returns "null" if all the elements are native. </param>
		public Buffer read(VertexInfo vertexInfo, int address, int firstVertex, int numberOfVertex, bool canAllNativeVertexInfo)
		{
			videoEngine = VideoEngine.Instance;
			this.vertexInfo = vertexInfo;
			this.canAllNativeVertexInfo = canAllNativeVertexInfo;

			update();

			// Don't need to read the vertex data if all elements are native
			if (AllNative)
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Not reading Vertex, all native at 0x{0:X8}", address));
				}
				return null;
			}

			// Display debug information on non-native elements
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Reading {0:D} Vertex at 0x{1:X8}", numberOfVertex, address + firstVertex * vertexInfo.vertexSize));
				if (!textureNative)
				{
					Console.WriteLine("Texture non-native " + vertexInfo.ToString());
				}
				if (!colorNative)
				{
					Console.WriteLine("Color non-native " + vertexInfo.ToString());
				}
				if (!normalNative)
				{
					Console.WriteLine("Normal non-native " + vertexInfo.ToString());
				}
				if (!positionNative)
				{
					Console.WriteLine("Position non-native " + vertexInfo.ToString());
				}
			}

			Address = address + firstVertex * vertexInfo.vertexSize;
			createVertexDataBuffer(numberOfVertex);

			// Prepare all the element readers
			IVertexInfoReader weightReader = getWeightReader(weightNative);
			IVertexInfoReader textureReader = getTextureReader(textureNative);
			IVertexInfoReader colorReader = getColorReader(colorNative);
			IVertexInfoReader normalReader = getNormalReader(normalNative);
			IVertexInfoReader positionReader = getPositionReader(positionNative);
			IVertexInfoReader padReader = getPaddingReader(vertexInfo.alignmentSize);

			// Read all the VertexInfo in sequence
			for (int i = 0; i < numberOfVertex; i++)
			{
				weightReader.read();
				textureReader.read();
				colorReader.read();
				normalReader.read();
				positionReader.read();
				padReader.read();
			}

			return vertexDataBuffer.Buffer;
		}

		private IVertexInfoReader getWeightReader(bool isNative)
		{
			return (isNative ? skipWeightReaders[vertexInfo.weight] : weightReaders[vertexInfo.weight]);
		}

		private IVertexInfoReader getTextureReader(bool isNative)
		{
			return (isNative ? skip2Readers[vertexInfo.texture] : textureReaders[vertexInfo.texture]);
		}

		private IVertexInfoReader getColorReader(bool isNative)
		{
			return (isNative ? skipColorReaders[vertexInfo.color] : colorReaders[vertexInfo.color]);
		}

		private IVertexInfoReader getNormalReader(bool isNative)
		{
			return (isNative ? skip3Readers[vertexInfo.normal] : normalReaders[vertexInfo.normal]);
		}

		private IVertexInfoReader getPositionReader(bool isNative)
		{
			return (isNative ? skip3Readers[vertexInfo.position] : positionReaders[vertexInfo.position]);
		}

		private IVertexInfoReader getPaddingReader(int size)
		{
			return paddingReaders[size];
		}

		/// <summary>
		/// Create the VertexDataBuffer for storing all the non-native elements.
		/// An "int"-based or a "float"-based buffer can be created, trying to find
		/// the best performance by avoiding conversions.
		/// The current decision is
		/// - GU_TRANSFORM_2D: use "int"-based buffer
		/// - GU_TRANSFORM_3D: use "float"-based buffer
		/// </summary>
		/// <param name="numberOfVertex">	The number of VertexInfo to read </param>
		private void createVertexDataBuffer(int numberOfVertex)
		{
			bool intBufferType = false;

			// Decide which buffer type is better (for performance)
			if (vertexInfo.transform2D)
			{
				intBufferType = true;
			}

			if (intBufferType)
			{
				vertexDataBuffer = new IntVertexDataBuffer(stride * numberOfVertex);
			}
			else
			{
				vertexDataBuffer = new FloatVertexDataBuffer(stride * numberOfVertex);
			}
		}

		/// <summary>
		/// Interface for all VertexDataBuffer classes
		/// 
		/// </summary>
		private interface IVertexDataBuffer
		{
			public Buffer Buffer;
			public void put(int data);
			public void put(float data);
		}

		/// <summary>
		/// VertexDataBuffer based on "int" values
		/// 
		/// </summary>
		private static class IntVertexDataBuffer implements IVertexDataBuffer
		{
			private int[] buffer;
			private int index;

			public IntVertexDataBuffer(int sizeInBytes)
			{
				buffer = new int[(sizeInBytes + 3) / 4];
				index = 0;
			}

			public Buffer Buffer
			{
				return IntBuffer.wrap(buffer);
			}

			public void put(int data)
			{
				buffer[index] = data;
				index++;
			}

			public void put(float data)
			{
				buffer[index] = Float.floatToRawIntBits(data);
				index++;
			}
		}

		/// <summary>
		/// VertexDataBuffer based on "float" values
		/// 
		/// </summary>
		private static class FloatVertexDataBuffer implements IVertexDataBuffer
		{
			private float[] buffer;
			private int index;

			public FloatVertexDataBuffer(int sizeInBytes)
			{
				buffer = new float[(sizeInBytes + 3) / 4];
				index = 0;
			}

			public Buffer Buffer
			{
				return FloatBuffer.wrap(buffer);
			}

			public void put(int data)
			{
				buffer[index] = Float.intBitsToFloat(data);
				index++;
			}

			public void put(float data)
			{
				buffer[index] = data;
				index++;
			}
		}

		/// <summary>
		/// Interface for all readers
		/// 
		/// </summary>
		private interface IVertexInfoReader
		{
			/// <summary>
			/// Reads the vertex data from the memory and stores them into the vertex data buffer
			/// </summary>
			public void read();

			/// <summary>
			/// Returns the number of bytes stored into the vertex data buffer by one read() call
			/// </summary>
			public int size();

			/// <summary>
			/// Returns the Rendering Engine type of the values put into the vertex data buffer
			/// </summary>
			public int type();

			/// <summary>
			/// Returns the number of values for the element type
			/// </summary>
			public int numberValues();

			/// <summary>
			/// Returns if the vertex data can be used directly by the RE, without conversion
			/// </summary>
			public bool Native;
		}

		/// <summary>
		/// Abstract Reader for all native readers
		/// 
		/// </summary>
		private abstract class AbstractNativeReader implements IVertexInfoReader
		{
			public void read()
			{
				// Raise error
				VideoEngine.Instance.error("This vertex information is always native! " + vertexInfo.ToString());
			}

			public bool Native
			{
				return true;
			}

			public int size()
			{
				return 0;
			}
		}

		/// <summary>
		/// Abstract Reader for all skip readers
		/// 
		/// </summary>
		private abstract class AbstractSkipReader implements IVertexInfoReader
		{
			public bool Native
			{
				return true;
			}

			public int size()
			{
				return 0;
			}

			public int type()
			{
				return typeNone;
			}

			public int numberValues()
			{
				return 0;
			}
		}

		/// <summary>
		/// Reader displaying "Not Implemented" error
		/// 
		/// </summary>
		private class NotImplementedReader extends AbstractSkipReader
		{
			private string comment;

			public NotImplementedReader(string comment)
			{
				this.comment = comment;
			}

			public void read()
			{
				// Raise error
				VideoEngine.Instance.error(string.Format("Unsupported Vertex Information {0} for {1}", comment, vertexInfo.ToString()));
			}
		}

		/// <summary>
		/// Reader doing nothing
		/// 
		/// </summary>
		private class NopReader extends AbstractSkipReader
		{
			public void read()
			{
				// Nothing to do
			}
		}

		/// <summary>
		/// Reader aligning on a 16 bit boundary
		/// 
		/// </summary>
		private class AlignShortReader extends AbstractSkipReader
		{
			public void read()
			{
				// Align on a short boundary
				memoryReader.align16();
			}
		}

		/// <summary>
		/// Reader aligning on a 32 bit boundary
		/// 
		/// </summary>
		private class AlignIntReader extends AbstractSkipReader
		{
			public void read()
			{
				// Align on an int boundary
				memoryReader.align32();
			}
		}

		/// <summary>
		/// Reader skipping 2 bytes (2 x 8 bit)
		/// 
		/// </summary>
		private class Skip2BytesReader extends AbstractSkipReader
		{
			public void read()
			{
				// Skip 2 Bytes
				memoryReader.skipNext8();
				memoryReader.skipNext8();
			}
		}

		/// <summary>
		/// Reader skipping 3 bytes (3 x 8 bit)
		/// 
		/// </summary>
		private class Skip3BytesReader extends AbstractSkipReader
		{
			public void read()
			{
				// Skip 3 Bytes
				memoryReader.skipNext8();
				memoryReader.skipNext8();
				memoryReader.skipNext8();
			}
		}

		/// <summary>
		/// Reader skipping 1 short (1 x 16 bit)
		/// 
		/// </summary>
		private class Skip1ShortReader extends AbstractSkipReader
		{
			public void read()
			{
				// Skip 1 Shorts
				memoryReader.skipNext16();
			}
		}

		/// <summary>
		/// Reader skipping 2 shorts (2 x 16 bit)
		/// 
		/// </summary>
		private class Skip2ShortsReader extends AbstractSkipReader
		{
			public void read()
			{
				// Skip 2 Shorts
				memoryReader.skipNext16();
				memoryReader.skipNext16();
			}
		}

		/// <summary>
		/// Reader skipping 3 shorts (3 x 16 bit)
		/// 
		/// </summary>
		private class Skip3ShortsReader extends AbstractSkipReader
		{
			public void read()
			{
				// Skip 3 Shorts
				memoryReader.skipNext16();
				memoryReader.skipNext16();
				memoryReader.skipNext16();
			}
		}

		/// <summary>
		/// Reader skipping 1 integer (1 x 32 bit)
		/// 
		/// </summary>
		private class Skip1IntReader extends AbstractSkipReader
		{
			public void read()
			{
				// Skip 1 Int
				memoryReader.skipNext32();
			}
		}

		/// <summary>
		/// Reader skipping 2 floats (2 x 32 bit)
		/// 
		/// </summary>
		private class Skip2FloatsReader extends AbstractSkipReader
		{
			public void read()
			{
				// Skip 2 Floats
				memoryReader.skipNext32(2);
			}
		}

		/// <summary>
		/// Reader skipping 3 floats (3 x 32 bit)
		/// 
		/// </summary>
		private class Skip3FloatsReader extends AbstractSkipReader
		{
			public void read()
			{
				// Skip 3 Floats
				memoryReader.skipNext32(3);
			}
		}

		/// <summary>
		/// Reader for Texture type 1 (GU_TEXTURE_8BIT)
		/// 
		/// </summary>
		private class Texture1Reader implements IVertexInfoReader
		{
			public void read()
			{
				// Unsigned 8 bit
				int texture1 = memoryReader.readNext8();
				int texture2 = memoryReader.readNext8();

				if (vertexInfo.transform2D)
				{
					// Transform 2 unsigned 8 bit into 2 signed 16 bit
					vertexDataBuffer.put(texture1 | (texture2 << 16));
				}
				else
				{
					// To be mapped to [0..2] for 3D
					vertexDataBuffer.put(texture1 / 128f);
					vertexDataBuffer.put(texture2 / 128f);
				}
			}

			public bool Native
			{
				// Unsigned byte is not available as a native texture coordinate value
				return canAllNativeVertexInfo;
			}

			public int size()
			{
				return (Native ? 0 : (vertexInfo.transform2D ? 4 : 8));
			}

			public int type()
			{
				return (Native ? RE_UNSIGNED_BYTE : (vertexInfo.transform2D ? RE_SHORT : RE_FLOAT));
			}

			public int numberValues()
			{
				return 2;
			}
		}

		/// <summary>
		/// Reader for Texture type 2 (GU_TEXTURE_16BIT)
		/// 
		/// </summary>
		private class Texture2Reader implements IVertexInfoReader
		{
			public void read()
			{
				// Unsigned 16 bit
				int texture1 = memoryReader.readNext16();
				int texture2 = memoryReader.readNext16();

				// To be mapped to [0..2] for 3D
				vertexDataBuffer.put(texture1 / 32768f);
				vertexDataBuffer.put(texture2 / 32768f);
			}

			public bool Native
			{
				return canAllNativeVertexInfo || vertexInfo.transform2D;
			}

			public int size()
			{
				return (Native ? 0 : 8);
			}

			public int type()
			{
				// Use signed Int16 because unsigned Int16 is not allowed for glTexCoordPointer
				return (canAllNativeVertexInfo ? RE_UNSIGNED_SHORT : (Native ? RE_SHORT : RE_FLOAT));
			}

			public int numberValues()
			{
				return 2;
			}
		}

		/// <summary>
		/// Reader for Texture type 3 (GU_TEXTURE_32BITF)
		/// 
		/// </summary>
		private class Texture3Reader extends AbstractNativeReader
		{
			public int type()
			{
				return RE_FLOAT;
			}

			public int numberValues()
			{
				return 2;
			}
		}

		/// <summary>
		/// Reader for Color type 4 (GU_COLOR_5650)
		/// 
		/// </summary>
		private class Color4Reader implements IVertexInfoReader
		{
			public void read()
			{
				vertexDataBuffer.put(ImageReader.color565to8888(memoryReader.readNext16()));
			}

			public bool Native
			{
				return canAllNativeVertexInfo;
			}

			public int size()
			{
				return (Native ? 0 : 4);
			}

			public int type()
			{
				return (Native ? RE_UNSIGNED_SHORT : RE_UNSIGNED_BYTE);
			}

			public int numberValues()
			{
				return (Native ? 1 : 4);
			}
		}

		/// <summary>
		/// Reader for Color type 5 (GU_COLOR_5551)
		/// 
		/// </summary>
		private class Color5Reader implements IVertexInfoReader
		{
			public void read()
			{
				vertexDataBuffer.put(ImageReader.color5551to8888(memoryReader.readNext16()));
			}

			public bool Native
			{
				return canAllNativeVertexInfo;
			}

			public int size()
			{
				return (Native ? 0 : 4);
			}

			public int type()
			{
				return (Native ? RE_UNSIGNED_SHORT : RE_UNSIGNED_BYTE);
			}

			public int numberValues()
			{
				return (Native ? 1 : 4);
			}
		}

		/// <summary>
		/// Reader for Color type 6 (GU_COLOR_4444)
		/// 
		/// </summary>
		private class Color6Reader implements IVertexInfoReader
		{
			public void read()
			{
				vertexDataBuffer.put(ImageReader.color4444to8888(memoryReader.readNext16()));
			}

			public bool Native
			{
				return canAllNativeVertexInfo;
			}

			public int size()
			{
				return (Native ? 0 : 4);
			}

			public int type()
			{
				return (Native ? RE_UNSIGNED_SHORT : RE_UNSIGNED_BYTE);
			}

			public int numberValues()
			{
				return (Native ? 1 : 4);
			}
		}

		/// <summary>
		/// Reader for Color type 7 (GU_COLOR_8888)
		/// 
		/// </summary>
		private class Color7Reader extends AbstractNativeReader
		{
			public int type()
			{
				return RE_UNSIGNED_BYTE;
			}

			public int numberValues()
			{
				return 4;
			}
		}

		/// <summary>
		/// Reader for Normal type 1 (GU_NORMAL_8BIT)
		/// 
		/// </summary>
		private class Normal1Reader implements IVertexInfoReader
		{
			public void read()
			{
				// TODO Check if this value is signed like position or unsigned like texture
				// Signed 8 bit

				// To be mapped to [-1..1] for 3D
				normal[0] = ((sbyte) memoryReader.readNext8()) / 127f;
				normal[1] = ((sbyte) memoryReader.readNext8()) / 127f;
				normal[2] = ((sbyte) memoryReader.readNext8()) / 127f;
				if (vertexInfo.weight != 0)
				{
					videoEngine.doNormalSkinning(vertexInfo, boneWeights, normal);
				}
				vertexDataBuffer.put(normal[0]);
				vertexDataBuffer.put(normal[1]);
				vertexDataBuffer.put(normal[2]);
			}

			public bool Native
			{
				return canAllNativeVertexInfo || vertexInfo.transform2D;
			}

			public int size()
			{
				return (Native ? 0 : 12);
			}

			public int type()
			{
				return (Native ? RE_BYTE : RE_FLOAT);
			}

			public int numberValues()
			{
				return 3;
			}
		}

		/// <summary>
		/// Reader for Normal type 2 (GU_NORMAL_16BIT)
		/// 
		/// </summary>
		private class Normal2Reader implements IVertexInfoReader
		{
			public void read()
			{
				// TODO Check if this value is signed like position or unsigned like texture
				// Signed 16 bit

				// To be mapped to [-1..1] for 3D
				normal[0] = ((short) memoryReader.readNext16()) / 32767f;
				normal[1] = ((short) memoryReader.readNext16()) / 32767f;
				normal[2] = ((short) memoryReader.readNext16()) / 32767f;
				if (vertexInfo.weight != 0)
				{
					videoEngine.doNormalSkinning(vertexInfo, boneWeights, normal);
				}
				vertexDataBuffer.put(normal[0]);
				vertexDataBuffer.put(normal[1]);
				vertexDataBuffer.put(normal[2]);
			}

			public bool Native
			{
				return canAllNativeVertexInfo || vertexInfo.transform2D;
			}

			public int size()
			{
				return (Native ? 0 : 12);
			}

			public int type()
			{
				return (Native ? RE_SHORT : RE_FLOAT);
			}

			public int numberValues()
			{
				return 3;
			}
		}

		/// <summary>
		/// Reader for Normal type 3 (GU_NORMAL_32BITF)
		/// 
		/// </summary>
		private class Normal3Reader implements IVertexInfoReader
		{
			public void read()
			{
				normal[0] = memoryReader.readNextFloat();
				normal[1] = memoryReader.readNextFloat();
				normal[2] = memoryReader.readNextFloat();
				videoEngine.doNormalSkinning(vertexInfo, boneWeights, normal);
				vertexDataBuffer.put(normal[0]);
				vertexDataBuffer.put(normal[1]);
				vertexDataBuffer.put(normal[2]);
			}

			public bool Native
			{
				return canAllNativeVertexInfo || vertexInfo.weight == 0;
			}

			public int size()
			{
				return (Native ? 0 : 12);
			}

			public int type()
			{
				return RE_FLOAT;
			}

			public int numberValues()
			{
				return 3;
			}
		}

		/// <summary>
		/// Reader for Position (Vertex) type 1 (GU_VERTEX_8BIT)
		/// 
		/// </summary>
		private class Position1Reader implements IVertexInfoReader
		{
			public void read()
			{
				if (vertexInfo.transform2D)
				{
					// X and Y are signed 8 bit, Z is unsigned 8 bit
					vertexDataBuffer.put((sbyte) memoryReader.readNext8());
					vertexDataBuffer.put((sbyte) memoryReader.readNext8());
					vertexDataBuffer.put(memoryReader.readNext8());
				}
				else
				{
					// Signed 8 bit, to be mapped to [-1..1] for 3D
					position[0] = ((sbyte) memoryReader.readNext8()) / 127f;
					position[1] = ((sbyte) memoryReader.readNext8()) / 127f;
					position[2] = ((sbyte) memoryReader.readNext8()) / 127f;
					if (vertexInfo.weight != 0)
					{
						videoEngine.doPositionSkinning(vertexInfo, boneWeights, position);
					}
					vertexDataBuffer.put(position[0]);
					vertexDataBuffer.put(position[1]);
					vertexDataBuffer.put(position[2]);
				}
			}

			public bool Native
			{
				// Cannot be native because X and Y are signed and Z is unsigned
				return canAllNativeVertexInfo;
			}

			public int size()
			{
				return (Native ? 0 : 12);
			}

			public int type()
			{
				return (Native ? RE_BYTE : (vertexInfo.transform2D ? RE_INT : RE_FLOAT));
			}

			public int numberValues()
			{
				return 3;
			}
		}

		/// <summary>
		/// Reader for Position (Vertex) type 2 (GU_VERTEX_16BIT)
		/// 
		/// </summary>
		private class Position2Reader implements IVertexInfoReader
		{
			public void read()
			{
				if (vertexInfo.transform2D)
				{
					// X and Y are signed 16 bit, Z is unsigned 16 bit
					vertexDataBuffer.put((short) memoryReader.readNext16());
					vertexDataBuffer.put((short) memoryReader.readNext16());
					vertexDataBuffer.put(memoryReader.readNext16());
				}
				else
				{
					// Signed 16 bit, to be mapped to [-1..1] for 3D
					position[0] = ((short) memoryReader.readNext16()) / 32767f;
					position[1] = ((short) memoryReader.readNext16()) / 32767f;
					position[2] = ((short) memoryReader.readNext16()) / 32767f;
					if (vertexInfo.weight != 0)
					{
						videoEngine.doPositionSkinning(vertexInfo, boneWeights, position);
					}
					vertexDataBuffer.put(position[0]);
					vertexDataBuffer.put(position[1]);
					vertexDataBuffer.put(position[2]);
				}
			}

			public bool Native
			{
				// Cannot be native in 2D because X and Y are signed and Z is unsigned
				return canAllNativeVertexInfo;
			}

			public int size()
			{
				return (Native ? 0 : 12);
			}

			public int type()
			{
				return (Native ? RE_SHORT : (vertexInfo.transform2D ? RE_INT : RE_FLOAT));
			}

			public int numberValues()
			{
				return 3;
			}
		}

		/// <summary>
		/// Reader for Position (Vertex) type 3 (GU_VERTEX_32BITF)
		/// 
		/// </summary>
		private class Position3Reader implements IVertexInfoReader
		{
			public void read()
			{
				position[0] = memoryReader.readNextFloat();
				position[1] = memoryReader.readNextFloat();
				position[2] = memoryReader.readNextFloat();
				if (vertexInfo.weight != 0)
				{
					videoEngine.doPositionSkinning(vertexInfo, boneWeights, position);
				}
				if (vertexInfo.transform2D)
				{
					// Z is an integer value clamped between 0 and 65535
					if (position[2] < 0f)
					{
						position[2] = 0f;
					}
					else if (position[2] > 65535f)
					{
						position[2] = 65535f;
					}
					else
					{
						// 2D positions are always integer values: truncate float value
						position[2] = (int) position[2];
					}
				}
				vertexDataBuffer.put(position[0]);
				vertexDataBuffer.put(position[1]);
				vertexDataBuffer.put(position[2]);
			}

			public bool Native
			{
				// 2D needs some corrections of the Z coord.
				return canAllNativeVertexInfo || !vertexInfo.transform2D;
			}

			public int size()
			{
				return (Native ? 0 : 12);
			}

			public int type()
			{
				return RE_FLOAT;
			}

			public int numberValues()
			{
				return 3;
			}
		}

		/// <summary>
		/// Reader for Weight type 1 (GU_WEIGHT_8BIT)
		/// 
		/// </summary>
		public class Weight1Reader implements IVertexInfoReader
		{
			public void read()
			{
				for (int i = 0; i < vertexInfo.skinningWeightCount; i++)
				{
					// Unsigned 8 bit, mapped to [0..2]
					boneWeights[i] = memoryReader.readNext8() / 128f;
				}
			}

			public bool Native
			{
				return true;
			}

			public int size()
			{
				return 0;
			}

			public int type()
			{
				return RE_UNSIGNED_BYTE;
			}

			public int numberValues()
			{
				return vertexInfo.skinningWeightCount;
			}
		}

		/// <summary>
		/// Reader for Weight type 2 (GU_WEIGHT_16BIT)
		/// 
		/// </summary>
		public class Weight2Reader implements IVertexInfoReader
		{
			public void read()
			{
				for (int i = 0; i < vertexInfo.skinningWeightCount; i++)
				{
					// Unsigned 16 bit, mapped to [0..2]
					boneWeights[i] = memoryReader.readNext16() / 32768f;
				}
			}

			public bool Native
			{
				return true;
			}

			public int size()
			{
				return 0;
			}

			public int type()
			{
				return RE_UNSIGNED_SHORT;
			}

			public int numberValues()
			{
				return vertexInfo.skinningWeightCount;
			}
		}

		/// <summary>
		/// Reader for Weight type 3 (GU_WEIGHT_32BITF)
		/// 
		/// </summary>
		public class Weight3Reader implements IVertexInfoReader
		{
			public void read()
			{
				for (int i = 0; i < vertexInfo.skinningWeightCount; i++)
				{
					// Float value
					boneWeights[i] = memoryReader.readNextFloat();
				}
			}

			public bool Native
			{
				return true;
			}

			public int size()
			{
				return 0;
			}

			public int type()
			{
				return RE_FLOAT;
			}

			public int numberValues()
			{
				return vertexInfo.skinningWeightCount;
			}
		}

		/// <summary>
		/// Reader skipping the weight bytes (N x 8 bit)
		/// 
		/// </summary>
		private class SkipWeight1Reader extends AbstractSkipReader
		{
			public void read()
			{
				// Skip Weight Bytes
				for (int i = 0; i < vertexInfo.skinningWeightCount; i++)
				{
					memoryReader.skipNext8();
				}
			}
		}

		/// <summary>
		/// Reader skipping the weight shorts (N x 16 bit)
		/// 
		/// </summary>
		private class SkipWeight2Reader extends AbstractSkipReader
		{
			public void read()
			{
				// Skip Weight Shorts
				for (int i = 0; i < vertexInfo.skinningWeightCount; i++)
				{
					memoryReader.skipNext16();
				}
			}
		}

		/// <summary>
		/// Reader skipping the weight floats (N x 32 bit)
		/// 
		/// </summary>
		private class SkipWeight3Reader extends AbstractSkipReader
		{
			public void read()
			{
				// Skip Weight Floats
				memoryReader.skipNext32(vertexInfo.skinningWeightCount);
			}
		}
	}

}