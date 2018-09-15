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
namespace pspsharp.Allegrex.compiler.nativeCode.graphics
{
	//using Logger = org.apache.log4j.Logger;

	using Modules = pspsharp.HLE.Modules;
	using PspGeList = pspsharp.HLE.kernel.types.PspGeList;
	using AsyncVertexCache = pspsharp.graphics.AsyncVertexCache;
	using GeCommands = pspsharp.graphics.GeCommands;
	using VideoEngine = pspsharp.graphics.VideoEngine;
	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class sceGu : AbstractNativeCodeSequence
	{
		protected internal static new Logger log = VideoEngine.log_Renamed;
		private const bool writeTFLUSH = false;
		private const bool writeTSYNC = false;
		private const bool writeDUMMY = false;

		private static void sceGeListUpdateStallAddr(int addr)
		{
			// Simplification: we can update the stall address only if the VideoEngine
			// is processing one and only one GE list.
			VideoEngine videoEngine = VideoEngine.Instance;
			if (videoEngine.numberDrawLists() == 0)
			{
				PspGeList geList = videoEngine.CurrentList;
				if (geList != null && geList.StallAddr != 0)
				{
					addr &= Memory.addressMask;
					geList.StallAddr = addr;
				}
			}
		}

		public static void sceGuDrawArray(int contextAddr1, int contextAddr2, int listCurrentOffset, int updateStall)
		{
			Memory mem = Memory;
			int prim = GprA0;
			int vtype = GprA1;
			int count = GprA2;
			int indices = GprA3;
			int vertices = GprT0;
			int context = mem.read32(getRelocatedAddress(contextAddr1, contextAddr2));
			int listCurrent = mem.read32(context + listCurrentOffset);
			int cmd;

			if (vtype != 0)
			{
				cmd = (GeCommands.VTYPE << 24) | (vtype & 0x00FFFFFF);
				mem.write32(listCurrent, cmd);
				listCurrent += 4;
			}

			if (indices != 0)
			{
				cmd = (GeCommands.BASE << 24) | ((indices >> 8) & 0x000F0000);
				mem.write32(listCurrent, cmd);
				listCurrent += 4;

				cmd = (GeCommands.IADDR << 24) | (indices & 0x00FFFFFF);
				mem.write32(listCurrent, cmd);
				listCurrent += 4;
			}

			if (vertices != 0)
			{
				cmd = (GeCommands.BASE << 24) | ((vertices >> 8) & 0x000F0000);
				mem.write32(listCurrent, cmd);
				listCurrent += 4;

				cmd = (GeCommands.VADDR << 24) | (vertices & 0x00FFFFFF);
				mem.write32(listCurrent, cmd);
				listCurrent += 4;
			}

			cmd = (GeCommands.PRIM << 24) | ((prim & 0x7) << 16) | count;
			mem.write32(listCurrent, cmd);
			listCurrent += 4;

			mem.write32(context + listCurrentOffset, listCurrent);

			if (updateStall != 0)
			{
				sceGeListUpdateStallAddr(listCurrent);
			}

			if (VideoEngine.Instance.useAsyncVertexCache())
			{
				AsyncVertexCache.Instance.addAsyncCheck(prim, vtype, count, indices, vertices);
			}
		}

		public static void sceGuDrawArrayN(int contextAddr1, int contextAddr2, int listCurrentOffset, int updateStall)
		{
			Memory mem = Memory;
			int prim = GprA0;
			int vtype = GprA1;
			int count = GprA2;
			int n = GprA3;
			int indices = GprT0;
			int vertices = GprT1;
			int context = mem.read32(getRelocatedAddress(contextAddr1, contextAddr2));
			int listCurrent = mem.read32(context + listCurrentOffset);
			int cmd;

			IMemoryWriter listWriter = MemoryWriter.getMemoryWriter(listCurrent, (5 + n) << 2, 4);

			if (vtype != 0)
			{
				cmd = (GeCommands.VTYPE << 24) | (vtype & 0x00FFFFFF);
				listWriter.writeNext(cmd);
				listCurrent += 4;
			}

			if (indices != 0)
			{
				cmd = (GeCommands.BASE << 24) | ((indices >> 8) & 0x000F0000);
				listWriter.writeNext(cmd);
				listCurrent += 4;

				cmd = (GeCommands.IADDR << 24) | (indices & 0x00FFFFFF);
				listWriter.writeNext(cmd);
				listCurrent += 4;
			}

			if (vertices != 0)
			{
				cmd = (GeCommands.BASE << 24) | ((vertices >> 8) & 0x000F0000);
				listWriter.writeNext(cmd);
				listCurrent += 4;

				cmd = (GeCommands.VADDR << 24) | (vertices & 0x00FFFFFF);
				listWriter.writeNext(cmd);
				listCurrent += 4;
			}

			if (n > 0)
			{
				cmd = (GeCommands.PRIM << 24) | ((prim & 0x7) << 16) | count;
				for (int i = 0; i < n; i++)
				{
					listWriter.writeNext(cmd);
				}
				listCurrent += (n << 2);
			}

			mem.write32(context + listCurrentOffset, listCurrent);

			if (updateStall != 0)
			{
				sceGeListUpdateStallAddr(listCurrent);
			}
		}

		public static void sceGuDrawSpline(int contextAddr1, int contextAddr2, int listCurrentOffset, int updateStall)
		{
			Memory mem = Memory;
			int vtype = GprA0;
			int ucount = GprA1;
			int vcount = GprA2;
			int uedge = GprA3;
			int vedge = GprT0;
			int indices = GprT1;
			int vertices = GprT2;
			int context = mem.read32(getRelocatedAddress(contextAddr1, contextAddr2));
			int listCurrent = mem.read32(context + listCurrentOffset);
			int cmd;

			if (vtype != 0)
			{
				cmd = (GeCommands.VTYPE << 24) | (vtype & 0x00FFFFFF);
				mem.write32(listCurrent, cmd);
				listCurrent += 4;
			}

			if (indices != 0)
			{
				cmd = (GeCommands.BASE << 24) | ((indices >> 8) & 0x000F0000);
				mem.write32(listCurrent, cmd);
				listCurrent += 4;

				cmd = (GeCommands.IADDR << 24) | (indices & 0x00FFFFFF);
				mem.write32(listCurrent, cmd);
				listCurrent += 4;
			}

			if (vertices != 0)
			{
				cmd = (GeCommands.BASE << 24) | ((vertices >> 8) & 0x000F0000);
				mem.write32(listCurrent, cmd);
				listCurrent += 4;

				cmd = (GeCommands.VADDR << 24) | (vertices & 0x00FFFFFF);
				mem.write32(listCurrent, cmd);
				listCurrent += 4;
			}

			cmd = (GeCommands.SPLINE << 24) | (vedge << 18) | (uedge << 16) | (vcount << 8) | ucount;
			mem.write32(listCurrent, cmd);
			listCurrent += 4;

			mem.write32(context + listCurrentOffset, listCurrent);

			if (updateStall != 0)
			{
				sceGeListUpdateStallAddr(listCurrent);
			}
		}

		public static void sceGuCopyImage(int contextAddr1, int contextAddr2, int listCurrentOffset)
		{
			Memory mem = Memory;
			int psm = GprA0;
			int sx = GprA1;
			int sy = GprA2;
			int width = GprA3;
			int height = GprT0;
			int srcw = GprT1;
			int src = GprT2;
			int dx = GprT3;
			int dy = StackParam0;
			int destw = StackParam1;
			int dest = StackParam2;
			int context = mem.read32(getRelocatedAddress(contextAddr1, contextAddr2));
			int listCurrent = mem.read32(context + listCurrentOffset);
			int cmd;

			IMemoryWriter listWriter = MemoryWriter.getMemoryWriter(listCurrent, 32, 4);

			cmd = (GeCommands.TRXSBP << 24) | (src & 0x00FFFFFF);
			listWriter.writeNext(cmd);

			cmd = (GeCommands.TRXSBW << 24) | ((src >> 8) & 0x000F0000) | srcw;
			listWriter.writeNext(cmd);

			cmd = (GeCommands.TRXPOS << 24) | (sy << 10) | sx;
			listWriter.writeNext(cmd);

			cmd = (GeCommands.TRXDBP << 24) | (dest & 0x00FFFFFF);
			listWriter.writeNext(cmd);

			cmd = (GeCommands.TRXDBW << 24) | ((dest >> 8) & 0x000F0000) | destw;
			listWriter.writeNext(cmd);

			cmd = (GeCommands.TRXDPOS << 24) | (dy << 10) | dx;
			listWriter.writeNext(cmd);

			cmd = (GeCommands.TRXSIZE << 24) | ((height - 1) << 10) | (width - 1);
			listWriter.writeNext(cmd);

			cmd = (GeCommands.TRXKICK << 24) | (pspsharp.graphics.RE.IRenderingEngine_Fields.sizeOfTextureType[psm] == 4 ? GeCommands.TRXKICK_32BIT_TEXEL_SIZE : GeCommands.TRXKICK_16BIT_TEXEL_SIZE);
			listWriter.writeNext(cmd);

			listWriter.flush();
			mem.write32(context + listCurrentOffset, listCurrent + 32);
		}

		public static void sceGuTexImage(int contextAddr1, int contextAddr2, int listCurrentOffset)
		{
			Memory mem = Memory;
			int mipmap = GprA0;
			int width = GprA1;
			int height = GprA2;
			int tbw = GprA3;
			int tbp = GprT0;
			int context = mem.read32(getRelocatedAddress(contextAddr1, contextAddr2));
			int listCurrent = mem.read32(context + listCurrentOffset);
			int cmd;

			cmd = ((GeCommands.TBP0 + mipmap) << 24) | (tbp & 0x00FFFFFF);
			mem.write32(listCurrent, cmd);
			listCurrent += 4;

			cmd = ((GeCommands.TBW0 + mipmap) << 24) | ((tbp >> 8) & 0x000F0000) | tbw;
			mem.write32(listCurrent, cmd);
			listCurrent += 4;

			// widthExp = 31 - CLZ(width)
			int widthExp = 31 - Integer.numberOfLeadingZeros(width);
			int heightExp = 31 - Integer.numberOfLeadingZeros(height);
			cmd = ((GeCommands.TSIZE0 + mipmap) << 24) | (heightExp << 8) | widthExp;
			mem.write32(listCurrent, cmd);
			listCurrent += 4;

			if (writeTFLUSH)
			{
				cmd = (GeCommands.TFLUSH << 24);
				mem.write32(listCurrent, cmd);
				listCurrent += 4;
			}

			mem.write32(context + listCurrentOffset, listCurrent);
		}

		public static void sceGuTexSync(int contextAddr1, int contextAddr2, int listCurrentOffset)
		{
			if (writeTSYNC)
			{
				Memory mem = Memory;
				int context = mem.read32(getRelocatedAddress(contextAddr1, contextAddr2));
				int listCurrent = mem.read32(context + listCurrentOffset);
				int cmd;

				cmd = (GeCommands.TSYNC << 24);
				mem.write32(listCurrent, cmd);
				listCurrent += 4;

				mem.write32(context + listCurrentOffset, listCurrent);
			}
		}

		public static void sceGuTexMapMode(int contextAddr1, int contextAddr2, int listCurrentOffset, int texProjMapOffset, int texMapModeOffset)
		{
			Memory mem = Memory;
			int texMapMode = GprA0 & 0x3;
			int texShadeU = GprA1;
			int texShadeV = GprA2;
			int context = mem.read32(getRelocatedAddress(contextAddr1, contextAddr2));
			int listCurrent = mem.read32(context + listCurrentOffset);
			int cmd;

			int texProjMap = mem.read32(context + texProjMapOffset);
			mem.write32(context + texMapModeOffset, texMapMode);

			cmd = (GeCommands.TMAP << 24) | (texProjMap << 8) | texMapMode;
			mem.write32(listCurrent, cmd);
			listCurrent += 4;

			cmd = (GeCommands.TEXTURE_ENV_MAP_MATRIX << 24) | (texShadeV << 8) | texShadeU;
			mem.write32(listCurrent, cmd);
			listCurrent += 4;

			mem.write32(context + listCurrentOffset, listCurrent);
		}

		public static void sceGuTexProjMapMode(int contextAddr1, int contextAddr2, int listCurrentOffset, int texProjMapOffset, int texMapModeOffset)
		{
			Memory mem = Memory;
			int texProjMap = GprA0 & 0x3;
			int context = mem.read32(getRelocatedAddress(contextAddr1, contextAddr2));
			int listCurrent = mem.read32(context + listCurrentOffset);
			int cmd;

			int texMapMode = mem.read32(context + texMapModeOffset);
			mem.write32(context + texProjMapOffset, texProjMap);

			cmd = (GeCommands.TMAP << 24) | (texProjMap << 8) | texMapMode;
			mem.write32(listCurrent, cmd);
			listCurrent += 4;

			mem.write32(context + listCurrentOffset, listCurrent);
		}

		public static void sceGuTexLevelMode(int contextAddr1, int contextAddr2, int listCurrentOffset)
		{
			Memory mem = Memory;
			int mode = GprA0;
			float bias = FprF12;
			int context = mem.read32(getRelocatedAddress(contextAddr1, contextAddr2));
			int listCurrent = mem.read32(context + listCurrentOffset);
			int cmd;

			int offset = (int)(bias * 16.0f);
			if (offset > 127)
			{
				offset = 127;
			}
			else if (offset < -128)
			{
				offset = -128;
			}

			cmd = (GeCommands.TBIAS << 24) | (offset << 16) | mode;
			mem.write32(listCurrent, cmd);
			listCurrent += 4;

			mem.write32(context + listCurrentOffset, listCurrent);
		}

		public static void sceGuMaterial(int contextAddr1, int contextAddr2, int listCurrentOffset)
		{
			int mode = GprA0;
			int color = GprA1;
			int context = Memory.read32(getRelocatedAddress(contextAddr1, contextAddr2));
			sceGuMaterial(context, listCurrentOffset, mode, color);
		}

		public static void sceGuMaterial(int listCurrentOffset)
		{
			int context = GprA0;
			int mode = GprA1;
			int color = GprA2;
			sceGuMaterial(context, listCurrentOffset, mode, color);
		}

		private static void sceGuMaterial(int context, int listCurrentOffset, int mode, int color)
		{
			Memory mem = Memory;
			int listCurrent = mem.read32(context + listCurrentOffset);
			int cmd;
			int rgb = color & 0x00FFFFFF;

			if ((mode & 0x01) != 0)
			{
				cmd = (GeCommands.AMC << 24) | rgb;
				mem.write32(listCurrent, cmd);
				listCurrent += 4;

				cmd = (GeCommands.AMA << 24) | ((int)((uint)color >> 24));
				mem.write32(listCurrent, cmd);
				listCurrent += 4;
			}

			if ((mode & 0x02) != 0)
			{
				cmd = (GeCommands.DMC << 24) | rgb;
				mem.write32(listCurrent, cmd);
				listCurrent += 4;
			}

			if ((mode & 0x04) != 0)
			{
				cmd = (GeCommands.SMC << 24) | rgb;
				mem.write32(listCurrent, cmd);
				listCurrent += 4;
			}

			mem.write32(context + listCurrentOffset, listCurrent);
		}

		public static void sceGuSetMatrix(int contextAddr1, int contextAddr2, int listCurrentOffset)
		{
			int type = GprA0;
			int matrix = GprA1;
			int context = Memory.read32(getRelocatedAddress(contextAddr1, contextAddr2));
			sceGuSetMatrix(context, listCurrentOffset, type, matrix);
		}

		public static void sceGuSetMatrix(int listCurrentOffset)
		{
			int context = GprA0;
			int type = GprA1;
			int matrix = GprA2;
			sceGuSetMatrix(context, listCurrentOffset, type, matrix);
		}

		private static int sceGuSetMatrix4x4(IMemoryWriter listWriter, IMemoryReader matrixReader, int startCmd, int matrixCmd, int index)
		{
			listWriter.writeNext((startCmd << 24) + index);
			int cmd = matrixCmd << 24;
			for (int i = 0; i < 16; i++)
			{
				listWriter.writeNext(cmd | ((int)((uint)matrixReader.readNext() >> 8)));
			}
			return 68;
		}

		private static int sceGuSetMatrix4x3(IMemoryWriter listWriter, IMemoryReader matrixReader, int startCmd, int matrixCmd, int index)
		{
			listWriter.writeNext((startCmd << 24) + index);
			int cmd = matrixCmd << 24;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					listWriter.writeNext(cmd | ((int)((uint)matrixReader.readNext() >> 8)));
				}
				matrixReader.skip(1);
			}
			return 52;
		}

		private static void sceGuSetMatrix(int context, int listCurrentOffset, int type, int matrix)
		{
			Memory mem = Memory;
			int listCurrent = mem.read32(context + listCurrentOffset);

			IMemoryWriter listWriter = MemoryWriter.getMemoryWriter(listCurrent, 68, 4);
			IMemoryReader matrixReader = MemoryReader.getMemoryReader(matrix, 64, 4);
			switch (type)
			{
				case 0:
					listCurrent += sceGuSetMatrix4x4(listWriter, matrixReader, GeCommands.PMS, GeCommands.PROJ, 0);
					break;
				case 1:
					listCurrent += sceGuSetMatrix4x3(listWriter, matrixReader, GeCommands.VMS, GeCommands.VIEW, 0);
					break;
				case 2:
					listCurrent += sceGuSetMatrix4x3(listWriter, matrixReader, GeCommands.MMS, GeCommands.MODEL, 0);
					break;
				case 3:
					listCurrent += sceGuSetMatrix4x3(listWriter, matrixReader, GeCommands.TMS, GeCommands.TMATRIX, 0);
					break;
			}
			listWriter.flush();

			mem.write32(context + listCurrentOffset, listCurrent);
		}

		public static void sceGuBoneMatrix(int contextAddr1, int contextAddr2, int listCurrentOffset)
		{
			int index = GprA0;
			int matrix = GprA1;
			int context = Memory.read32(getRelocatedAddress(contextAddr1, contextAddr2));
			sceGuBoneMatrix(context, listCurrentOffset, index, matrix);
		}

		public static void sceGuBoneMatrix(int listCurrentOffset)
		{
			int context = GprA0;
			int index = GprA1;
			int matrix = GprA2;
			sceGuBoneMatrix(context, listCurrentOffset, index, matrix);
		}

		private static void sceGuBoneMatrix(int context, int listCurrentOffset, int index, int matrix)
		{
			Memory mem = Memory;
			int listCurrent = mem.read32(context + listCurrentOffset);

			IMemoryWriter listWriter = MemoryWriter.getMemoryWriter(listCurrent, 56, 4);
			if (writeDUMMY)
			{
				listWriter.writeNext(GeCommands.DUMMY << 24);
				listCurrent += 4;
			}
			IMemoryReader matrixReader = MemoryReader.getMemoryReader(matrix, 64, 4);
			listCurrent += sceGuSetMatrix4x3(listWriter, matrixReader, GeCommands.BOFS, GeCommands.BONE, index * 12);
			listWriter.flush();

			mem.write32(context + listCurrentOffset, listCurrent);
		}

		public static void sceGuDrawSprite(int contextAddr1, int contextAddr2, int listCurrentOffset, int wOffset, int hOffset, int dxOffset, int dyOffset)
		{
			int x = GprA0;
			int y = GprA1;
			int z = GprA2;
			int u = GprA3;
			int v = GprT0;
			int flip = GprT1;
			int rotation = GprT2;
			int context = Memory.read32(getRelocatedAddress(contextAddr1, contextAddr2));
			sceGuDrawSprite(context, listCurrentOffset, x, y, z, u, v, flip, rotation, wOffset, hOffset, dxOffset, dyOffset);
		}

		public static void sceGuDrawSprite(int listCurrentOffset, int wOffset, int hOffset, int dxOffset, int dyOffset)
		{
			int context = GprA0;
			int x = GprA1;
			int y = GprA2;
			int z = GprA3;
			int u = GprT0;
			int v = GprT1;
			int flip = GprT2;
			int rotation = GprT3;
			sceGuDrawSprite(context, listCurrentOffset, x, y, z, u, v, flip, rotation, wOffset, hOffset, dxOffset, dyOffset);
		}

		private static void sceGuDrawSprite(int context, int listCurrentOffset, int x, int y, int z, int u, int v, int flip, int rotation, int wOffset, int hOffset, int dxOffset, int dyOffset)
		{
			Memory mem = Memory;
			int listCurrent = mem.read32(context + listCurrentOffset);
			int w = mem.read32(context + wOffset);
			int h = mem.read32(context + hOffset);
			int dx = mem.read32(context + dxOffset);
			int dy = mem.read32(context + dyOffset);
			int cmd;

			IMemoryWriter listWriter = MemoryWriter.getMemoryWriter(listCurrent, 44, 4);

			int vertexAddress = listCurrent + 8;
			IMemoryWriter vertexWriter = MemoryWriter.getMemoryWriter(vertexAddress, 20, 2);
			int v0u = u;
			int v0v = v;
			int v0x = x;
			int v0y = y;
			int v1u = u + w;
			int v1v = v + h;
			int v1x = x + dx;
			int v1y = y + dy;

			if ((flip & 1) != 0)
			{
				int tmp = v0u;
				v0u = v1u;
				v1u = tmp;
			}
			if ((flip & 2) != 0)
			{
				int tmp = v0v;
				v0v = v1v;
				v1v = tmp;
			}
			switch (rotation)
			{
				case 1:
				{
					int tmp = v0y;
					v0y = v1y;
					v1y = tmp;
					break;
				}
				case 2:
				{
					int tmp = v0x;
					v0x = v1x;
					v1x = tmp;
					tmp = v0y;
					v0y = v1y;
					v1y = tmp;
					break;
				}
				case 3:
				{
					int tmp = v0x;
					v0x = v1x;
					v1x = tmp;
					break;
				}
			}

			vertexWriter.writeNext(v0u);
			vertexWriter.writeNext(v0v);
			vertexWriter.writeNext(v0x);
			vertexWriter.writeNext(v0y);
			vertexWriter.writeNext(z);
			vertexWriter.writeNext(v1u);
			vertexWriter.writeNext(v1v);
			vertexWriter.writeNext(v1x);
			vertexWriter.writeNext(v1y);
			vertexWriter.writeNext(z);
			vertexWriter.flush();

			int jumpAddr = vertexAddress + 20;
			cmd = (GeCommands.BASE << 24) | ((jumpAddr >> 8) & 0x00FF0000);
			listWriter.writeNext(cmd);

			cmd = (GeCommands.JUMP << 24) | (jumpAddr & 0x00FFFFFF);
			listWriter.writeNext(cmd);

			// Skip the 2 vertex entries
			listWriter.skip(5);

			cmd = (GeCommands.VTYPE << 24) | ((GeCommands.VTYPE_TRANSFORM_PIPELINE_RAW_COORD << 23) | (GeCommands.VTYPE_POSITION_FORMAT_16_BIT << 7) | GeCommands.VTYPE_TEXTURE_FORMAT_16_BIT);
			listWriter.writeNext(cmd);

			cmd = (GeCommands.BASE << 24) | ((vertexAddress >> 8) & 0x00FF0000);
			listWriter.writeNext(cmd);

			cmd = (GeCommands.VADDR << 24) | (vertexAddress & 0x00FFFFFF);
			listWriter.writeNext(cmd);

			cmd = (GeCommands.PRIM << 24) | (GeCommands.PRIM_SPRITES << 16) | 2;
			listWriter.writeNext(cmd);

			listWriter.flush();

			mem.write32(context + listCurrentOffset, jumpAddr + 16);
		}

		private static int getListSize(int listAddr)
		{
			IMemoryReader memoryReader = MemoryReader.getMemoryReader(listAddr, 4);
			for (int i = 1; true; i++)
			{
				int instruction = memoryReader.readNext();
				int cmd = VideoEngine.command(instruction);
				if (cmd == GeCommands.RET)
				{
					return i;
				}
			}
		}

		public static void sceGuCallList()
		{
			int callAddr = GprA0;
			if (Modules.ThreadManForUserModule.isCurrentThreadStackAddress(callAddr))
			{
				// Some games are calling GE lists stored on the thread stack... dirty programming!
				// Such a list can be overwritten as the thread stack gets used in further calls.
				// These changes are however not seen immediately by the GE engine due to the memory caching.
				// The developer of the games probably never found this bug due to the PSP hardware memory caching.
				// This is however an issue with pspsharp as no memory caching is implemented.
				// So, we simulate a memory cache here by reading the called list into an array and force the
				// VideoEngine to reuse these cached values when processing the GE list.
				int listSize = getListSize(callAddr);
				int memorySize = listSize << 2;

				if (log.InfoEnabled)
				{
					log.info(string.Format("sceGuCallList Stack address 0x{0:X8}-0x{1:X8}", callAddr, callAddr + memorySize));
				}

				int[] instructions = Utilities.readInt32(callAddr, memorySize);
				VideoEngine.Instance.addCachedInstructions(callAddr, instructions);
			}
		}

		public static void saveGeToMemoryHook()
		{
			int geTopAddress = GprA0;
			Modules.sceDisplayModule.copyGeToMemory(geTopAddress);
		}
	}

}