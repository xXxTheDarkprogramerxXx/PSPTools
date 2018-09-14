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
namespace pspsharp.graphics.RE.software
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.getColor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.RE.software.PixelColor.getColorBGR;


	using Logger = org.apache.log4j.Logger;

	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using Modules = pspsharp.HLE.Modules;
	using CaptureManager = pspsharp.graphics.capture.CaptureManager;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using ImageReader = pspsharp.memory.ImageReader;
	using DurationStatistics = pspsharp.util.DurationStatistics;
	using LongLongKey = pspsharp.util.LongLongKey;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// This class is the base for all renderers.
	/// It can be re-used for multiple primitives (e.g. multiple triangles)
	/// belonging to the same IRenderingEngine.drawArrays call.
	/// This class contains all the information based
	/// on the GeContext but has no vertex-specific information.
	/// </summary>
	public abstract class BaseRenderer : IRenderer
	{
		public abstract IRenderer duplicate();
		public abstract void render();
		public abstract bool prepare(GeContext context);
		protected internal static readonly Logger log = VideoEngine.log_Renamed;
		protected internal readonly bool isLogTraceEnabled;
		protected internal readonly bool isLogDebugEnabled;
		protected internal readonly bool isLogInfoEnabled;

		protected internal const bool captureEachPrimitive = false;
		protected internal const bool captureZbuffer = false;
		public const int depthBufferPixelFormat = GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_INDEXED;
		protected internal const int MAX_NUMBER_FILTERS = 15;
		public int imageWriterSkipEOL;
		public int depthWriterSkipEOL;
		protected internal RendererTemplate compiledRenderer;
		protected internal LongLongKey compiledRendererKey;
		protected internal LongLongKey baseRendererKey;
		protected internal bool transform2D;
		protected internal int viewportWidth;
		protected internal int viewportHeight;
		protected internal int viewportX;
		protected internal int viewportY;
		protected internal int screenOffsetX;
		protected internal int screenOffsetY;
		protected internal float zscale;
		protected internal float zpos;
		public int nearZ;
		public int farZ;
		public int scissorX1, scissorY1;
		public int scissorX2, scissorY2;
		protected internal bool setVertexPrimaryColor;
		protected internal int fbp, fbw, psm;
		protected internal int zbp, zbw;
		protected internal bool clearMode;
		protected internal bool cullFaceEnabled;
		protected internal bool frontFaceCw;
		protected internal bool clipPlanesEnabled;
		protected internal bool useVertexTexture;
		public IRandomTextureAccess textureAccess;
		protected internal int mipmapLevel = 0;
		public Lighting lighting;
		private static Dictionary<LongLongKey, int> filtersStatistics = new Dictionary<LongLongKey, int>();
		private static Dictionary<LongLongKey, string> filterNames = new Dictionary<LongLongKey, string>();
		protected internal bool renderingInitialized;
		public CachedTextureResampled cachedTexture;
		protected internal bool isTriangle;
		public int colorTestRef;
		public int colorTestMsk;
		public int alphaRef;
		public int alphaMask;
		public int stencilRef;
		public int stencilMask;
		public int sfix;
		public int dfix;
		public int colorMask;
		public bool primaryColorSetGlobally;
		public float texTranslateX;
		public float texTranslateY;
		public float texScaleX = 1f;
		public float texScaleY = 1f;
		public int textureWidth;
		public int textureHeight;
		public int texEnvColorB;
		public int texEnvColorG;
		public int texEnvColorR;
		public float[] envMapLightPosU;
		public float[] envMapLightPosV;
		public bool envMapDiffuseLightU;
		public bool envMapDiffuseLightV;
		public float envMapShininess;
		public int texMinFilter;
		public int texMagFilter;
		public int primaryColor;
		public int[] ditherMatrix;

		protected internal virtual void copy(BaseRenderer from)
		{
			imageWriterSkipEOL = from.imageWriterSkipEOL;
			depthWriterSkipEOL = from.depthWriterSkipEOL;
			compiledRendererKey = from.compiledRendererKey;
			compiledRenderer = from.compiledRenderer;
			lighting = from.lighting;
			textureAccess = from.textureAccess;
			transform2D = from.transform2D;
			nearZ = from.nearZ;
			farZ = from.farZ;
			scissorX1 = from.scissorX1;
			scissorY1 = from.scissorY1;
			scissorX2 = from.scissorX2;
			scissorY2 = from.scissorY2;
			cachedTexture = from.cachedTexture;
			isTriangle = from.isTriangle;
			colorTestRef = from.colorTestRef;
			colorTestMsk = from.colorTestMsk;
			alphaRef = from.alphaRef;
			alphaMask = from.alphaMask;
			stencilRef = from.stencilRef;
			stencilMask = from.stencilMask;
			sfix = from.sfix;
			dfix = from.dfix;
			colorMask = from.colorMask;
			primaryColorSetGlobally = from.primaryColorSetGlobally;
			texTranslateX = from.texTranslateX;
			texTranslateY = from.texTranslateY;
			texScaleX = from.texScaleX;
			texScaleY = from.texScaleY;
			textureWidth = from.textureWidth;
			textureHeight = from.textureHeight;
			texEnvColorB = from.texEnvColorB;
			texEnvColorG = from.texEnvColorG;
			texEnvColorR = from.texEnvColorR;
			envMapLightPosU = from.envMapLightPosU;
			envMapLightPosV = from.envMapLightPosV;
			envMapDiffuseLightU = from.envMapDiffuseLightU;
			envMapDiffuseLightV = from.envMapDiffuseLightV;
			envMapShininess = from.envMapShininess;
			texMinFilter = from.texMinFilter;
			texMagFilter = from.texMagFilter;
			primaryColor = from.primaryColor;
			ditherMatrix = from.ditherMatrix;
		}

		protected internal BaseRenderer()
		{
			isLogTraceEnabled = log.TraceEnabled;
			isLogDebugEnabled = log.DebugEnabled;
			isLogInfoEnabled = log.InfoEnabled;
		}

		protected internal virtual int getTextureAddress(int address, int x, int y, int textureWidth, int pixelFormat)
		{
			int bytesPerPixel = pspsharp.graphics.RE.IRenderingEngine_Fields.sizeOfTextureType[pixelFormat];
			int numberOfPixels = y * textureWidth + x;
			address &= Memory.addressMask;
			// bytesPerPixel == 0 means 2 pixels per byte (4bit indexed)
			return address + (bytesPerPixel == 0 ? numberOfPixels >> 1 : numberOfPixels * bytesPerPixel);
		}

		private static int getFrameBufferAddress(int addr)
		{
			addr &= Memory.addressMask;
			if (addr < MemoryMap.START_VRAM)
			{
				addr += MemoryMap.START_VRAM;
			}

			return addr;
		}

		protected internal virtual void init(GeContext context, CachedTextureResampled texture, bool useVertexTexture, bool isTriangle)
		{
			this.cachedTexture = texture;
			this.useVertexTexture = useVertexTexture;
			this.isTriangle = isTriangle;
			nearZ = context.nearZ;
			farZ = context.farZ;
			scissorX1 = context.scissor_x1;
			scissorY1 = context.scissor_y1;
			scissorX2 = context.scissor_x2;
			scissorY2 = context.scissor_y2;
			clearMode = context.clearMode;
			cullFaceEnabled = context.cullFaceFlag.Enabled;
			frontFaceCw = context.frontFaceCw;
			clipPlanesEnabled = context.clipPlanesFlag.Enabled;
			fbw = context.fbw;
			zbw = context.zbw;
			if (context.ditherFlag.Enabled)
			{
				ditherMatrix = context.dither_matrix.Clone();
			}

			transform2D = context.vinfo.transform2D;
			if (!transform2D)
			{
				viewportWidth = context.viewport_width;
				viewportHeight = context.viewport_height;
				viewportX = context.viewport_cx;
				viewportY = context.viewport_cy;
				screenOffsetX = context.offset_x;
				screenOffsetY = context.offset_y;
				zscale = context.zscale;
				zpos = context.zpos;
				if (context.tex_map_mode == GeCommands.TMAP_TEXTURE_MAP_MODE_TEXTURE_COORDIATES_UV)
				{
					texTranslateX = context.tex_translate_x;
					texTranslateY = context.tex_translate_y;
					texScaleX = context.tex_scale_x;
					texScaleY = context.tex_scale_y;
				}
				else if (context.tex_map_mode == GeCommands.TMAP_TEXTURE_MAP_MODE_ENVIRONMENT_MAP)
				{
					envMapLightPosU = new float[4];
					envMapLightPosV = new float[4];
					Utilities.copy(envMapLightPosU, context.light_pos[context.tex_shade_u]);
					Utilities.copy(envMapLightPosV, context.light_pos[context.tex_shade_v]);
					envMapDiffuseLightU = context.light_type[context.tex_shade_u] == GeCommands.LIGHT_AMBIENT_DIFFUSE;
					envMapDiffuseLightV = context.light_type[context.tex_shade_v] == GeCommands.LIGHT_AMBIENT_DIFFUSE;
					envMapShininess = context.materialShininess;
				}
			}

			if (isLogTraceEnabled)
			{
				log.trace(string.Format("Context: {0}", context.ToString()));
			}
		}

		protected internal virtual bool isUsingTexture(GeContext context)
		{
			return context.textureFlag.Enabled && (!transform2D || useVertexTexture) && !clearMode;
		}

		protected internal virtual void initRendering(GeContext context)
		{
			if (renderingInitialized)
			{
				return;
			}

			fbp = getFrameBufferAddress(context.fbp);
			psm = context.psm;
			zbp = getFrameBufferAddress(context.zbp);
			colorTestRef = getColorBGR(context.colorTestRef);
			colorTestMsk = getColorBGR(context.colorTestMsk);
			alphaRef = context.alphaRef & context.alphaMask;
			alphaMask = context.alphaMask;
			stencilRef = context.stencilRef & context.stencilMask;
			stencilMask = context.stencilMask;
			sfix = context.sfix;
			dfix = context.dfix;
			colorMask = getColor(context.colorMask);
			textureWidth = context.texture_width[mipmapLevel];
			textureHeight = context.texture_height[mipmapLevel];
			texEnvColorB = getColor(context.tex_env_color[2]);
			texEnvColorG = getColor(context.tex_env_color[1]);
			texEnvColorR = getColor(context.tex_env_color[0]);
			texMinFilter = context.tex_min_filter;
			texMagFilter = context.tex_mag_filter;
			primaryColor = getColor(context.vertexColor);

			baseRendererKey = getBaseRendererKey(context);

			if (!transform2D && context.lightingFlag.Enabled)
			{
				lighting = new Lighting(context.view_uploaded_matrix, context.mat_emissive, context.ambient_light, context.lightFlags, context.light_pos, context.light_kind, context.light_type, context.lightAmbientColor, context.lightDiffuseColor, context.lightSpecularColor, context.lightConstantAttenuation, context.lightLinearAttenuation, context.lightQuadraticAttenuation, context.spotLightCutoff, context.spotLightCosCutoff, context.light_dir, context.spotLightExponent, context.materialShininess, context.lightMode, context.vinfo.normal != 0);
			}

			// Is the lighting model using the material color from the vertex color?
			if (!transform2D && context.lightingFlag.Enabled && context.mat_flags != 0 && context.useVertexColor && context.vinfo.color != 0 && isTriangle)
			{
				setVertexPrimaryColor = true;
			}

			primaryColorSetGlobally = false;
			if (transform2D || !context.lightingFlag.Enabled)
			{
				// No lighting, take the primary color from the vertex.
				// This will be done by the BasePrimitiveRenderer when the vertices are known.
				if (context.useVertexColor && context.vinfo.color != 0)
				{
					setVertexPrimaryColor = true;
					if (!isTriangle)
					{
						// Use the color of the 2nd sprite vertex
						primaryColorSetGlobally = true;
					}
				}
				else
				{
					// Use context.vertexColor as the primary color
					primaryColorSetGlobally = true;
				}
			}

			textureAccess = null;
			if (isUsingTexture(context))
			{
				int textureBufferWidth = VideoEngine.alignBufferWidth(context.texture_buffer_width[mipmapLevel], context.texture_storage);
				int textureHeight = context.texture_height[mipmapLevel];
				int textureAddress = context.texture_base_pointer[mipmapLevel];
				if (cachedTexture == null)
				{
					int[] clut32 = VideoEngine.Instance.readClut32(mipmapLevel);
					short[] clut16 = VideoEngine.Instance.readClut16(mipmapLevel);
					// Always request the whole buffer width
					IMemoryReader imageReader = ImageReader.getImageReader(textureAddress, textureBufferWidth, textureHeight, textureBufferWidth, context.texture_storage, context.texture_swizzle, context.tex_clut_addr, context.tex_clut_mode, context.tex_clut_num_blocks, context.tex_clut_start, context.tex_clut_shift, context.tex_clut_mask, clut32, clut16);
					textureAccess = new RandomTextureAccessReader(imageReader, textureBufferWidth, textureHeight);
				}
				else
				{
					textureAccess = cachedTexture.OriginalTexture;
				}

				// Avoid an access outside the texture area
				textureAccess = TextureClip.getTextureClip(context, mipmapLevel, textureAccess, textureBufferWidth, textureHeight);
			}

			renderingInitialized = true;
		}

		private LongLongKey getBaseRendererKey(GeContext context)
		{
			LongLongKey key = new LongLongKey();

			key.addKeyComponent(RuntimeContext.hasMemoryInt());
			key.addKeyComponent(transform2D);
			key.addKeyComponent(clearMode);
			if (clearMode)
			{
				key.addKeyComponent(context.clearModeColor);
				key.addKeyComponent(context.clearModeStencil);
				key.addKeyComponent(context.clearModeDepth);
			}
			else
			{
				key.addKeyComponent(false);
				key.addKeyComponent(false);
				key.addKeyComponent(false);
			}
			key.addKeyComponent(nearZ == 0x0000);
			key.addKeyComponent(farZ == 0xFFFF);

			key.addKeyComponent(context.colorTestFlag.Enabled ? context.colorTestFunc : GeCommands.CTST_COLOR_FUNCTION_ALWAYS_PASS_PIXEL, 2);

			if (context.alphaTestFlag.Enabled)
			{
				key.addKeyComponent(context.alphaFunc, 3);
				key.addKeyComponent(context.alphaRef == 0x00);
				key.addKeyComponent(context.alphaRef == 0xFF);
			}
			else
			{
				key.addKeyComponent(GeCommands.ATST_ALWAYS_PASS_PIXEL, 3);
				key.addKeyComponent(false);
				key.addKeyComponent(false);
			}

			if (context.stencilTestFlag.Enabled)
			{
				key.addKeyComponent(context.stencilFunc, 3);
				key.addKeyComponent(context.stencilOpFail, 3);
				key.addKeyComponent(context.stencilOpZFail, 3);
				key.addKeyComponent(context.stencilOpZPass, 3);
			}
			else
			{
				key.addKeyComponent(GeCommands.STST_FUNCTION_ALWAYS_PASS_STENCIL_TEST, 3);
				// Use invalid stencil operations
				key.addKeyComponent(7, 3);
				key.addKeyComponent(7, 3);
				key.addKeyComponent(7, 3);
			}

			key.addKeyComponent(context.depthTestFlag.Enabled ? context.depthFunc : GeCommands.ZTST_FUNCTION_ALWAYS_PASS_PIXEL, 3);

			if (context.blendFlag.Enabled)
			{
				key.addKeyComponent(context.blendEquation, 3);
				key.addKeyComponent(context.blend_src, 4);
				key.addKeyComponent(context.blend_dst, 4);
				key.addKeyComponent(context.sfix == 0x000000);
				key.addKeyComponent(context.sfix == 0xFFFFFF);
				key.addKeyComponent(context.dfix == 0x000000);
				key.addKeyComponent(context.dfix == 0xFFFFFF);
			}
			else
			{
				// Use an invalid blend equation value
				key.addKeyComponent(7, 3);
				key.addKeyComponent(15, 4);
				key.addKeyComponent(15, 4);
				key.addKeyComponent(false);
				key.addKeyComponent(false);
				key.addKeyComponent(false);
				key.addKeyComponent(false);
			}

			key.addKeyComponent(context.colorLogicOpFlag.Enabled ? context.logicOp : GeCommands.LOP_COPY, 4);

			key.addKeyComponent(PixelColor.getColor(context.colorMask) == 0x00000000);
			key.addKeyComponent(context.depthMask);
			key.addKeyComponent(context.textureFlag.Enabled);
			key.addKeyComponent(useVertexTexture);
			key.addKeyComponent(context.lightingFlag.Enabled);
			key.addKeyComponent(setVertexPrimaryColor);
			key.addKeyComponent(primaryColorSetGlobally);
			key.addKeyComponent(isTriangle);
			key.addKeyComponent(context.mat_flags, 3);
			key.addKeyComponent(context.useVertexColor);
			key.addKeyComponent(context.textureColorDoubled);
			key.addKeyComponent(context.lightMode, 1);
			key.addKeyComponent(context.tex_map_mode, 2);
			if (context.tex_map_mode == GeCommands.TMAP_TEXTURE_MAP_MODE_TEXTURE_MATRIX)
			{
				key.addKeyComponent(context.tex_proj_map_mode, 2);
			}
			else
			{
				key.addKeyComponent(0, 2);
			}
			key.addKeyComponent(context.tex_translate_x == 0f);
			key.addKeyComponent(context.tex_translate_y == 0f);
			key.addKeyComponent(context.tex_scale_x == 1f);
			key.addKeyComponent(context.tex_scale_y == 1f);
			key.addKeyComponent(context.tex_wrap_s, 1);
			key.addKeyComponent(context.tex_wrap_t, 1);
			key.addKeyComponent(context.textureFunc, 3);
			key.addKeyComponent(context.textureAlphaUsed);
			key.addKeyComponent(context.psm, 2);
			key.addKeyComponent(context.tex_min_filter, 3);
			key.addKeyComponent(context.tex_mag_filter, 1);
			key.addKeyComponent(isLogTraceEnabled);
			key.addKeyComponent(DurationStatistics.collectStatistics);
			key.addKeyComponent(context.ditherFlag.Enabled);

			return key;
		}

		protected internal virtual void preRender()
		{
		}

		protected internal virtual void postRender()
		{
			if (captureEachPrimitive && State.captureGeNextFrame)
			{
				// Capture the GE screen after each primitive
				Modules.sceDisplayModule.captureGeImage();
			}
			if (captureZbuffer && State.captureGeNextFrame)
			{
				captureZbufferImage();
			}
		}

		protected internal virtual void captureZbufferImage()
		{
			GeContext context = VideoEngine.Instance.Context;
			int width = context.zbw;
			int height = Modules.sceDisplayModule.HeightFb;
			int address = getTextureAddress(zbp, 0, 0, zbw, depthBufferPixelFormat);
			Buffer buffer = Memory.Instance.getBuffer(address, width * height * pspsharp.graphics.RE.IRenderingEngine_Fields.sizeOfTextureType[depthBufferPixelFormat]);
			CaptureManager.captureImage(address, 0, buffer, width, height, width, depthBufferPixelFormat, false, 0, false, false);
		}

		protected internal virtual void statisticsFilters(int numberPixels)
		{
			if (!DurationStatistics.collectStatistics || !isLogInfoEnabled)
			{
				return;
			}

			int? count = filtersStatistics[compiledRendererKey];
			if (count == null)
			{
				count = 0;
			}
			filtersStatistics[compiledRendererKey] = count + numberPixels;

			if (!filterNames.ContainsKey(compiledRendererKey))
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				filterNames[compiledRendererKey] = compiledRenderer.GetType().FullName;
			}
		}

		public static void exit()
		{
			if (log.InfoEnabled && DurationStatistics.collectStatistics)
			{
				LongLongKey[] filterKeys = filtersStatistics.Keys.toArray(new LongLongKey[filtersStatistics.Count]);
				Arrays.sort(filterKeys, new FilterComparator());
				foreach (LongLongKey filterKey in filterKeys)
				{
					int? count = filtersStatistics[filterKey];
					log.info(string.Format("Filter: count={0:D}, id={1}, {2}", count, filterKey, filterNames[filterKey]));
				}
			}

			FilterCompiler.exit();
		}

		private class FilterComparator : Comparator<LongLongKey>
		{
			public virtual int Compare(LongLongKey o1, LongLongKey o2)
			{
				return filtersStatistics[o1].compareTo(filtersStatistics[o2]);
			}
		}
	}

}