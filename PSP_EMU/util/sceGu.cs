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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.PRIM_LINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TFUNC_FRAGMENT_DOUBLE_ENABLE_COLOR_DOUBLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TFUNC_FRAGMENT_DOUBLE_ENABLE_COLOR_UNTOUCHED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TFUNC_FRAGMENT_DOUBLE_TEXTURE_COLOR_ALPHA_IS_IGNORED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TFUNC_FRAGMENT_DOUBLE_TEXTURE_COLOR_ALPHA_IS_READ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.VTYPE_COLOR_FORMAT_32BIT_ABGR_8888;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.VTYPE_POSITION_FORMAT_16_BIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.VTYPE_TRANSFORM_PIPELINE_RAW_COORD;

	using Logger = org.apache.log4j.Logger;

	using Modules = pspsharp.HLE.Modules;
	using TPointer = pspsharp.HLE.TPointer;
	using SysMemUserForUser = pspsharp.HLE.modules.SysMemUserForUser;
	using sceGe_user = pspsharp.HLE.modules.sceGe_user;
	using SysMemInfo = pspsharp.HLE.modules.SysMemUserForUser.SysMemInfo;
	using GeCommands = pspsharp.graphics.GeCommands;
	using GeContext = pspsharp.graphics.GeContext;
	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;
	using Screen = pspsharp.hardware.Screen;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryWriter = pspsharp.memory.MemoryWriter;

	/// <summary>
	/// Utility class to draw to the PSP GE from inside HLE functions.
	/// Used for example by sceUtilitySavedata to render its user interface.
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class sceGu
	{
		private static Logger log = Logger.getLogger("sceGu");
		private SysMemUserForUser.SysMemInfo sysMemInfo;
		private int bottomAddr;
		private int topAddr;
		private int listAddr;
		private IMemoryWriter listWriter;
		private int listId = -1;

		public sceGu(int totalMemorySize)
		{
			sysMemInfo = Modules.SysMemUserForUserModule.malloc(SysMemUserForUser.KERNEL_PARTITION_ID, "sceGu", SysMemUserForUser.PSP_SMEM_Low, totalMemorySize, 0);
			if (sysMemInfo == null)
			{
				log.error(string.Format("Cannot allocate sceGu memory, size=0x{0:X}", totalMemorySize));
			}
		}

		public virtual int sceGuGetMemory(int size)
		{
			size = Utilities.alignUp(size, 3);
			if (topAddr - size < listWriter.CurrentAddress)
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceGuGetMemory size=0x{0:X} - not enough memory, available=0x{1:X}", size, topAddr - listWriter.CurrentAddress));
				}
				// Not enough memory available
				return 0;
			}

			topAddr -= size;

			return topAddr;
		}

		public virtual void free()
		{
			if (sysMemInfo != null)
			{
				Modules.SysMemUserForUserModule.free(sysMemInfo);
				sysMemInfo = null;
			}
			bottomAddr = 0;
			topAddr = 0;
			listWriter = null;
		}

		public virtual void sendCommandi(int cmd, int argument)
		{
			listWriter.writeNext((cmd << 24) | (argument & 0x00FFFFFF));
		}

		public virtual void sendCommandf(int cmd, float argument)
		{
			sendCommandi(cmd, Float.floatToRawIntBits(argument) >> 8);
		}

		protected internal virtual void sendCommandBase(int cmd, int address)
		{
			sendCommandi(GeCommands.BASE, (long)((ulong)(address & 0xFF000000) >> 8));
			sendCommandi(cmd, address & 0x00FFFFFF);
		}

		public virtual void sceGuStart()
		{
			if (sysMemInfo != null)
			{
				bottomAddr = sysMemInfo.addr;
				topAddr = sysMemInfo.addr + sysMemInfo.size;
			}
			else
			{
				// Reserve memory for 2 complete screens (double buffering)
				int reservedSize = 512 * Screen.height * 4 * 2;

				// Use the rest of the VRAM
				bottomAddr = MemoryMap.START_VRAM + reservedSize;
				topAddr = bottomAddr + (MemoryMap.SIZE_VRAM - reservedSize);
			}

			listAddr = bottomAddr;
			listWriter = MemoryWriter.getMemoryWriter(listAddr, 4);
			listId = -1;

			// Init some values
			sceGuOffsetAddr(0);
			sendCommandi(GeCommands.BASE, 0);
		}

		public virtual void sceGuFinish()
		{
			sendCommandi(GeCommands.FINISH, 0);
			sendCommandi(GeCommands.END, 0);

			if (topAddr < listWriter.CurrentAddress)
			{
				log.error(string.Format("sceGu memory overwrite mem={0}, listAddr=0x{1:X8}, topAddr=0x{2:X8}", sysMemInfo, listWriter.CurrentAddress, topAddr));
			}
			else
			{
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceGu memory usage free=0x{0:X}, mem={1}, listAddr=0x{2:X8}, topAddr=0x{3:X8}", topAddr - listWriter.CurrentAddress, sysMemInfo, listWriter.CurrentAddress, topAddr));
				}
			}

			listWriter.flush();

			int saveContextAddr = sceGuGetMemory(GeContext.SIZE_OF);

			Memory mem = Memory.Instance;
			listId = Modules.sceGe_userModule.hleGeListEnQueue(new TPointer(mem, listAddr), TPointer.NULL, -1, TPointer.NULL, saveContextAddr, false);
		}

		public virtual bool ListDrawing
		{
			get
			{
				if (listId < 0)
				{
					return false;
				}
    
				int listState = Modules.sceGe_userModule.hleGeListSync(listId);
    
				if (log.DebugEnabled)
				{
					log.debug(string.Format("sceGu list 0x{0:X}: state {1:D}", listId, listState));
				}
    
				return listState == sceGe_user.PSP_GE_LIST_DRAWING || listState == sceGe_user.PSP_GE_LIST_QUEUED;
			}
		}

		private void sceGuSetFlag(int flag, int value)
		{
			switch (flag)
			{
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_ALPHA_TEST:
					sendCommandi(GeCommands.ATE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_DEPTH_TEST:
					sendCommandi(GeCommands.ZTE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_SCISSOR_TEST:
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_STENCIL_TEST:
					sendCommandi(GeCommands.STE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_BLEND:
					sendCommandi(GeCommands.ABE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_CULL_FACE:
					sendCommandi(GeCommands.BCE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_DITHER:
					sendCommandi(GeCommands.DTE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_FOG:
					sendCommandi(GeCommands.FGE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_CLIP_PLANES:
					sendCommandi(GeCommands.CPE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_TEXTURE_2D:
					sendCommandi(GeCommands.TME, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_LIGHTING:
					sendCommandi(GeCommands.LTE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_LIGHT0:
					sendCommandi(GeCommands.LTE0, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_LIGHT1:
					sendCommandi(GeCommands.LTE1, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_LIGHT2:
					sendCommandi(GeCommands.LTE2, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_LIGHT3:
					sendCommandi(GeCommands.LTE3, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_LINE_SMOOTH:
					sendCommandi(GeCommands.AAE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_PATCH_CULL_FACE:
					sendCommandi(GeCommands.PCE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_COLOR_TEST:
					sendCommandi(GeCommands.CTE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_COLOR_LOGIC_OP:
					sendCommandi(GeCommands.LOE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_FACE_NORMAL_REVERSE:
					sendCommandi(GeCommands.RNORM, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_PATCH_FACE:
					sendCommandi(GeCommands.PFACE, value);
					break;
				case pspsharp.graphics.RE.IRenderingEngine_Fields.GU_FRAGMENT_2X:
					break;
			}
		}

		public virtual void sceGuEnable(int flag)
		{
			sceGuSetFlag(flag, 1);
		}

		public virtual void sceGuDisable(int flag)
		{
			sceGuSetFlag(flag, 0);
		}

		public virtual void sceGuDrawArray(int prim, int vtype, int count, int indices, int vertices)
		{
			if (vtype != 0)
			{
				sendCommandi(GeCommands.VTYPE, vtype);
			}

			if (indices != 0)
			{
				sendCommandBase(GeCommands.IADDR, indices);
			}

			if (vertices != 0)
			{
				sendCommandBase(GeCommands.VADDR, vertices);
			}

			sendCommandi(GeCommands.PRIM, (prim << 16) | (count & 0xFFFF));
		}

		private int getExp(int val)
		{
			int i;
			for (i = 9; i > 0 && ((val >> i) & 1) == 0; i--)
			{
			}

			return i;
		}

		public virtual void sceGuTexImage(int mipmap, int width, int height, int tbw, int tbp)
		{
			sendCommandi(GeCommands.TBP0 + mipmap, tbp & 0x00FFFFFF);
			sendCommandi(GeCommands.TBW0 + mipmap, ((long)((ulong)(tbp & 0xFF000000) >> 8)) | (tbw & 0xFFFF));
			sendCommandi(GeCommands.TSIZE0 + mipmap, (getExp(height) << 8) | getExp(width));
			sendCommandi(GeCommands.TFLUSH, 0);
		}

		public virtual void sceGuTexMode(int tpsm, int maxMipmaps, bool swizzle)
		{
			sendCommandi(GeCommands.TMODE, (maxMipmaps << 16) | (swizzle ? 1 : 0));
			sendCommandi(GeCommands.TPSM, tpsm);
		}

		public virtual void sceGuClutMode(int cpsm, int shift, int mask, int start)
		{
			sendCommandi(GeCommands.CMODE, cpsm | (shift << 12) | (mask << 8) | (start << 16));
		}

		public virtual void sceGuClutLoad(int numBlocks, int cbp)
		{
			sendCommandi(GeCommands.CBP, cbp & 0x00FFFFFF);
			sendCommandi(GeCommands.CBPH, (long)((ulong)(cbp & 0xFF000000) >> 8));
			sendCommandi(GeCommands.CLOAD, numBlocks);
		}

		public virtual void sceGuTexFunc(int textureFunc, bool textureAlphaUsed, bool textureColorDoubled)
		{
			sendCommandi(GeCommands.TFUNC, textureFunc | ((textureAlphaUsed ? TFUNC_FRAGMENT_DOUBLE_TEXTURE_COLOR_ALPHA_IS_READ : TFUNC_FRAGMENT_DOUBLE_TEXTURE_COLOR_ALPHA_IS_IGNORED) << 8) | ((textureColorDoubled ? TFUNC_FRAGMENT_DOUBLE_ENABLE_COLOR_DOUBLED : TFUNC_FRAGMENT_DOUBLE_ENABLE_COLOR_UNTOUCHED) << 16));
		}

		public virtual void sceGuBlendFunc(int op, int src, int dest, int srcFix, int destFix)
		{
			sendCommandi(GeCommands.ALPHA, src | (dest << 4) | (op << 8));
			if (src >= GeCommands.ALPHA_FIX)
			{
				sendCommandi(GeCommands.SFIX, srcFix);
			}
			if (dest >= GeCommands.ALPHA_FIX)
			{
				sendCommandi(GeCommands.DFIX, destFix);
			}
		}

		public virtual void sceGuOffsetAddr(int offsetAddr)
		{
			sendCommandi(GeCommands.OFFSET_ADDR, (int)((uint)offsetAddr >> 8));
		}

		public virtual void sceGuTexEnvColor(int color)
		{
			sendCommandi(GeCommands.TEC, color & 0x00FFFFFF);
		}

		public virtual void sceGuTexWrap(int u, int v)
		{
			sendCommandi(GeCommands.TWRAP, (v << 8) | u);
		}

		public virtual void sceGuTexFilter(int min, int mag)
		{
			sendCommandi(GeCommands.TFLT, (mag << 8) | min);
		}

		public virtual void sceGuDrawHorizontalLine(int x0, int x1, int y, int color)
		{
			sceGuDrawLine(x0, y, x1, y, color);
		}

		public virtual void sceGuDrawLine(int x0, int y0, int x1, int y1, int color)
		{
			int numberOfVertex = 2;
			int lineVertexAddr = sceGuGetMemory(12 * numberOfVertex);
			IMemoryWriter lineVertexWriter = MemoryWriter.getMemoryWriter(lineVertexAddr, 2);
			// Color
			lineVertexWriter.writeNext(color & 0xFFFF);
			lineVertexWriter.writeNext((int)((uint)color >> 16));
			// Position
			lineVertexWriter.writeNext(x0);
			lineVertexWriter.writeNext(y0);
			lineVertexWriter.writeNext(0);
			// Align on 32-bit
			lineVertexWriter.writeNext(0);
			// Color
			lineVertexWriter.writeNext(color & 0xFFFF);
			lineVertexWriter.writeNext((int)((uint)color >> 16));
			// Position
			lineVertexWriter.writeNext(x1);
			lineVertexWriter.writeNext(y1);
			lineVertexWriter.writeNext(0);
			// Align on 32-bit
			lineVertexWriter.writeNext(0);
			lineVertexWriter.flush();

			sceGuDisable(pspsharp.graphics.RE.IRenderingEngine_Fields.GU_TEXTURE_2D);
			sceGuDrawArray(PRIM_LINE, (VTYPE_TRANSFORM_PIPELINE_RAW_COORD << 23) | (VTYPE_COLOR_FORMAT_32BIT_ABGR_8888 << 2) | (VTYPE_POSITION_FORMAT_16_BIT << 7), numberOfVertex, 0, lineVertexAddr);
		}

		public virtual void sceGuDrawRectangle(int x0, int y0, int x1, int y1, int color)
		{
			int numberOfVertex = 2;
			int lineVertexAddr = sceGuGetMemory(12 * numberOfVertex);
			IMemoryWriter lineVertexWriter = MemoryWriter.getMemoryWriter(lineVertexAddr, 2);
			// Color
			lineVertexWriter.writeNext(color & 0xFFFF);
			lineVertexWriter.writeNext((int)((uint)color >> 16));
			// Position
			lineVertexWriter.writeNext(x0);
			lineVertexWriter.writeNext(y0);
			lineVertexWriter.writeNext(0);
			// Align on 32-bit
			lineVertexWriter.writeNext(0);
			// Color
			lineVertexWriter.writeNext(color & 0xFFFF);
			lineVertexWriter.writeNext((int)((uint)color >> 16));
			// Position
			lineVertexWriter.writeNext(x1);
			lineVertexWriter.writeNext(y1);
			lineVertexWriter.writeNext(0);
			// Align on 32-bit
			lineVertexWriter.writeNext(0);
			lineVertexWriter.flush();

			sceGuDisable(pspsharp.graphics.RE.IRenderingEngine_Fields.GU_TEXTURE_2D);
			sceGuDrawArray(GeCommands.PRIM_SPRITES, (VTYPE_TRANSFORM_PIPELINE_RAW_COORD << 23) | (VTYPE_COLOR_FORMAT_32BIT_ABGR_8888 << 2) | (VTYPE_POSITION_FORMAT_16_BIT << 7), numberOfVertex, 0, lineVertexAddr);
		}

		public virtual void sceGuClear(int mode, int color)
		{
			sendCommandi(GeCommands.CLEAR, ((mode & 0x7) << 8) | 0x01);
			sceGuDrawRectangle(0, 0, Screen.width, Screen.height, color);
			sendCommandi(GeCommands.CLEAR, 0x00);
		}
	}

}