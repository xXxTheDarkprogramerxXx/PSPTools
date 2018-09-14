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
namespace pspsharp.graphics.RE.software
{
	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using IMemoryReaderWriter = pspsharp.memory.IMemoryReaderWriter;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class RendererWriter
	{
		public static IRendererWriter getRendererWriter(int fbAddress, int fbBufferWidth, int fbPixelFormat, int depthAddress, int depthBufferWidth, int depthPixelFormat, bool needDepthRead, bool needDepthWrite)
		{
			if (RuntimeContext.hasMemoryInt())
			{
				return getFastMemoryRendererWriter(fbAddress, fbBufferWidth, fbPixelFormat, depthAddress, depthBufferWidth, depthPixelFormat, needDepthRead, needDepthWrite);
			}

			return getRendererWriterGeneric(fbAddress, fbBufferWidth, fbPixelFormat, depthAddress, depthBufferWidth, depthPixelFormat, needDepthRead, needDepthWrite);
		}

		private static IRendererWriter getFastMemoryRendererWriter(int fbAddress, int fbBufferWidth, int fbPixelFormat, int depthAddress, int depthBufferWidth, int depthPixelFormat, bool needDepthRead, bool needDepthWrite)
		{
			int[] memInt = RuntimeContext.MemoryInt;

			if (depthPixelFormat == BaseRenderer.depthBufferPixelFormat)
			{
				switch (fbPixelFormat)
				{
					case GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888:
						if (needDepthRead)
						{
							if (needDepthWrite)
							{
								return new RendererWriterInt32(memInt, fbAddress, depthAddress);
							}
							return new RendererWriterNoDepthWriteInt32(memInt, fbAddress, depthAddress);
						}
						if (needDepthWrite)
						{
							return new RendererWriterNoDepthReadInt32(memInt, fbAddress, depthAddress);
						}
						return new RendererWriterNoDepthReadWriteInt32(memInt, fbAddress);
				}
			}

			return getRendererWriterGeneric(fbAddress, fbBufferWidth, fbPixelFormat, depthAddress, depthBufferWidth, depthPixelFormat, needDepthRead, needDepthWrite);
		}

		private static IRendererWriter getRendererWriterGeneric(int fbAddress, int fbBufferWidth, int fbPixelFormat, int depthAddress, int depthBufferWidth, int depthPixelFormat, bool needDepthRead, bool needDepthWrite)
		{
			if (!needDepthRead && !needDepthWrite)
			{
				return new RendererWriterGenericNoDepth(fbAddress, fbBufferWidth, fbPixelFormat);
			}
			return new RendererWriterGeneric(fbAddress, fbBufferWidth, fbPixelFormat, depthAddress, depthBufferWidth, depthPixelFormat);
		}

		private sealed class RendererWriterGeneric : IRendererWriter
		{
			internal readonly IMemoryReaderWriter fbWriter;
			internal readonly IMemoryReaderWriter depthWriter;

			public RendererWriterGeneric(int fbAddress, int fbBufferWidth, int fbPixelFormat, int depthAddress, int depthBufferWidth, int depthPixelFormat)
			{
				fbWriter = ImageWriter.getImageWriter(fbAddress, fbBufferWidth, fbBufferWidth, fbPixelFormat);
				depthWriter = ImageWriter.getImageWriter(depthAddress, depthBufferWidth, depthBufferWidth, depthPixelFormat);
			}

			public void readCurrent(ColorDepth colorDepth)
			{
				colorDepth.color = fbWriter.readCurrent();
				colorDepth.depth = depthWriter.readCurrent();
			}

			public void writeNext(ColorDepth colorDepth)
			{
				fbWriter.writeNext(colorDepth.color);
				depthWriter.writeNext(colorDepth.depth);
			}

			public void writeNextColor(int color)
			{
				fbWriter.writeNext(color);
				depthWriter.skip(1);
			}

			public void skip(int fbCount, int depthCount)
			{
				fbWriter.skip(fbCount);
				depthWriter.skip(depthCount);
			}

			public void flush()
			{
				fbWriter.flush();
				depthWriter.flush();
			}
		}

		private sealed class RendererWriterGenericNoDepth : IRendererWriter
		{
			internal readonly IMemoryReaderWriter fbWriter;

			public RendererWriterGenericNoDepth(int fbAddress, int fbBufferWidth, int fbPixelFormat)
			{
				fbWriter = ImageWriter.getImageWriter(fbAddress, fbBufferWidth, fbBufferWidth, fbPixelFormat);
			}

			public void readCurrent(ColorDepth colorDepth)
			{
				colorDepth.color = fbWriter.readCurrent();
			}

			public void writeNext(ColorDepth colorDepth)
			{
				fbWriter.writeNext(colorDepth.color);
			}

			public void writeNextColor(int color)
			{
				fbWriter.writeNext(color);
			}

			public void skip(int fbCount, int depthCount)
			{
				fbWriter.skip(fbCount);
			}

			public void flush()
			{
				fbWriter.flush();
			}
		}

		private sealed class RendererWriterInt32 : IRendererWriter
		{
			internal int fbIndex;
			internal int depthIndex;
			internal int depthOffset;
			internal readonly int[] memInt;

			public RendererWriterInt32(int[] memInt, int fbAddress, int depthAddress)
			{
				this.memInt = memInt;
				fbIndex = (fbAddress & Memory.addressMask) >> 2;
				depthIndex = (depthAddress & Memory.addressMask) >> 2;
				depthOffset = (depthAddress >> 1) & 1;
			}

			public void readCurrent(ColorDepth colorDepth)
			{
				colorDepth.color = memInt[fbIndex];
				if (depthOffset == 0)
				{
					colorDepth.depth = memInt[depthIndex] & 0x0000FFFF;
				}
				else
				{
					colorDepth.depth = (int)((uint)memInt[depthIndex] >> 16);
				}
			}

			internal void next()
			{
				fbIndex++;
				if (depthOffset == 0)
				{
					depthOffset = 1;
				}
				else
				{
					depthIndex++;
					depthOffset = 0;
				}
			}

			public void writeNext(ColorDepth colorDepth)
			{
				memInt[fbIndex] = colorDepth.color;
				if (depthOffset == 0)
				{
					memInt[depthIndex] = (memInt[depthIndex] & unchecked((int)0xFFFF0000)) | (colorDepth.depth & 0x0000FFFF);
				}
				else
				{
					memInt[depthIndex] = (memInt[depthIndex] & 0x0000FFFF) | (colorDepth.depth << 16);
				}
				next();
			}

			public void writeNextColor(int color)
			{
				memInt[fbIndex] = color;
				next();
			}

			public void skip(int fbCount, int depthCount)
			{
				fbIndex += fbCount;
				depthOffset += depthCount;
				depthIndex += depthOffset >> 1;
				depthOffset &= 1;
			}

			public void flush()
			{
			}
		}

		private sealed class RendererWriterNoDepthReadInt32 : IRendererWriter
		{
			internal int fbIndex;
			internal int depthIndex;
			internal int depthOffset;
			internal readonly int[] memInt;

			public RendererWriterNoDepthReadInt32(int[] memInt, int fbAddress, int depthAddress)
			{
				this.memInt = memInt;
				fbIndex = (fbAddress & Memory.addressMask) >> 2;
				depthIndex = (depthAddress & Memory.addressMask) >> 2;
				depthOffset = (depthAddress >> 1) & 1;
			}

			public void readCurrent(ColorDepth colorDepth)
			{
				colorDepth.color = memInt[fbIndex];
			}

			internal void next()
			{
				fbIndex++;
				if (depthOffset == 0)
				{
					depthOffset = 1;
				}
				else
				{
					depthIndex++;
					depthOffset = 0;
				}
			}

			public void writeNext(ColorDepth colorDepth)
			{
				memInt[fbIndex] = colorDepth.color;
				if (depthOffset == 0)
				{
					memInt[depthIndex] = (memInt[depthIndex] & unchecked((int)0xFFFF0000)) | (colorDepth.depth & 0x0000FFFF);
				}
				else
				{
					memInt[depthIndex] = (memInt[depthIndex] & 0x0000FFFF) | (colorDepth.depth << 16);
				}
				next();
			}

			public void writeNextColor(int color)
			{
				memInt[fbIndex] = color;
				next();
			}

			public void skip(int fbCount, int depthCount)
			{
				fbIndex += fbCount;
				depthOffset += depthCount;
				depthIndex += depthOffset >> 1;
				depthOffset &= 1;
			}

			public void flush()
			{
			}
		}

		private sealed class RendererWriterNoDepthWriteInt32 : IRendererWriter
		{
			internal int fbIndex;
			internal int depthIndex;
			internal int depthOffset;
			internal readonly int[] memInt;

			public RendererWriterNoDepthWriteInt32(int[] memInt, int fbAddress, int depthAddress)
			{
				this.memInt = memInt;
				fbIndex = (fbAddress & Memory.addressMask) >> 2;
				depthIndex = (depthAddress & Memory.addressMask) >> 2;
				depthOffset = (depthAddress >> 1) & 1;
			}

			public void readCurrent(ColorDepth colorDepth)
			{
				colorDepth.color = memInt[fbIndex];
				if (depthOffset == 0)
				{
					colorDepth.depth = memInt[depthIndex] & 0x0000FFFF;
				}
				else
				{
					colorDepth.depth = (int)((uint)memInt[depthIndex] >> 16);
				}
			}

			internal void next()
			{
				fbIndex++;
				if (depthOffset == 0)
				{
					depthOffset = 1;
				}
				else
				{
					depthIndex++;
					depthOffset = 0;
				}
			}

			public void writeNext(ColorDepth colorDepth)
			{
				memInt[fbIndex] = colorDepth.color;
				next();
			}

			public void writeNextColor(int color)
			{
				memInt[fbIndex] = color;
				next();
			}

			public void skip(int fbCount, int depthCount)
			{
				fbIndex += fbCount;
				depthOffset += depthCount;
				depthIndex += depthOffset >> 1;
				depthOffset &= 1;
			}

			public void flush()
			{
			}
		}

		private sealed class RendererWriterNoDepthReadWriteInt32 : IRendererWriter
		{
			internal int fbIndex;
			internal readonly int[] memInt;

			public RendererWriterNoDepthReadWriteInt32(int[] memInt, int fbAddress)
			{
				this.memInt = memInt;
				fbIndex = (fbAddress & Memory.addressMask) >> 2;
			}

			public void readCurrent(ColorDepth colorDepth)
			{
				colorDepth.color = memInt[fbIndex];
			}

			public void writeNext(ColorDepth colorDepth)
			{
				memInt[fbIndex] = colorDepth.color;
				fbIndex++;
			}

			public void writeNextColor(int color)
			{
				memInt[fbIndex] = color;
				fbIndex++;
			}

			public void skip(int fbCount, int depthCount)
			{
				fbIndex += fbCount;
			}

			public void flush()
			{
			}
		}
	}

}