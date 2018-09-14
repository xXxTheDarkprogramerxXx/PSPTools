using System.Collections.Generic;
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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.MemoryMap.START_VRAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TBIAS_MODE_AUTO;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TFLT_NEAREST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TMAP_TEXTURE_MAP_MODE_TEXTURE_COORDIATES_UV;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TMAP_TEXTURE_PROJECTION_MODE_POSITION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TWRAP_WRAP_MODE_REPEAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.VideoEngine.NUM_LIGHTS;


	using Logger = org.apache.log4j.Logger;

	using pspAbstractMemoryMappedStructure = pspsharp.HLE.kernel.types.pspAbstractMemoryMappedStructure;
	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class GeContext : pspAbstractMemoryMappedStructure
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			alphaTestFlag = new EnableDisableFlag(this, "GU_ALPHA_TEST", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_ALPHA_TEST);
			depthTestFlag = new EnableDisableFlag(this, "GU_DEPTH_TEST", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_DEPTH_TEST);
			scissorTestFlag = new EnableDisableFlag(this, "GU_SCISSOR_TEST", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_SCISSOR_TEST);
			stencilTestFlag = new EnableDisableFlag(this, "GU_STENCIL_TEST", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_STENCIL_TEST);
			blendFlag = new EnableDisableFlag(this, "GU_BLEND", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_BLEND);
			cullFaceFlag = new EnableDisableFlag(this, "GU_CULL_FACE", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_CULL_FACE);
			ditherFlag = new EnableDisableFlag(this, "GU_DITHER", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_DITHER);
			fogFlag = new EnableDisableFlag(this, "GU_FOG", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_FOG);
			clipPlanesFlag = new EnableDisableFlag(this, "GU_CLIP_PLANES", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_CLIP_PLANES);
			textureFlag = new EnableDisableFlag(this, "GU_TEXTURE_2D", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_TEXTURE_2D);
			lightingFlag = new EnableDisableFlag(this, "GU_LIGHTING", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_LIGHTING);
			lightFlags = new EnableDisableFlag[]
			{
				new EnableDisableFlag(this, "GU_LIGHT0", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_LIGHT0),
				new EnableDisableFlag(this, "GU_LIGHT1", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_LIGHT1),
				new EnableDisableFlag(this, "GU_LIGHT2", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_LIGHT2),
				new EnableDisableFlag(this, "GU_LIGHT3", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_LIGHT3)
			};
			lineSmoothFlag = new EnableDisableFlag(this, "GU_LINE_SMOOTH", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_LINE_SMOOTH);
			patchCullFaceFlag = new EnableDisableFlag(this, "GU_PATCH_CULL_FACE", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_PATCH_CULL_FACE);
			colorTestFlag = new EnableDisableFlag(this, "GU_COLOR_TEST", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_COLOR_TEST);
			colorLogicOpFlag = new EnableDisableFlag(this, "GU_COLOR_LOGIC_OP", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_COLOR_LOGIC_OP);
			faceNormalReverseFlag = new EnableDisableFlag(this, "GU_FACE_NORMAL_REVERSE", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_FACE_NORMAL_REVERSE);
			patchFaceFlag = new EnableDisableFlag(this, "GU_PATCH_FACE", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_PATCH_FACE);
			fragment2xFlag = new EnableDisableFlag(this, "GU_FRAGMENT_2X", pspsharp.graphics.RE.IRenderingEngine_Fields.GU_FRAGMENT_2X);
			reColorMaterial = new EnableDisableFlag(this, "RE_COLOR_MATERIAL", pspsharp.graphics.RE.IRenderingEngine_Fields.RE_COLOR_MATERIAL);
			reTextureGenS = new EnableDisableFlag(this, "RE_TEXTURE_GEN_S", pspsharp.graphics.RE.IRenderingEngine_Fields.RE_TEXTURE_GEN_S);
			reTextureGenT = new EnableDisableFlag(this, "RE_TEXTURE_GEN_T", pspsharp.graphics.RE.IRenderingEngine_Fields.RE_TEXTURE_GEN_T);
		}

		private static readonly Logger log = VideoEngine.log_Renamed;
		// pspsdk defines the context as an array of 512 unsigned int's
		public const int SIZE_OF = 512 * 4;

		protected internal IRenderingEngine re;
		protected internal bool dirty;

		public int @base;
		// The value of baseOffset has to be added (not ORed) to the base value.
		// baseOffset is updated by the ORIGIN_ADDR and OFFSET_ADDR commands,
		// and both commands share the same value field.
		public int baseOffset;
		public int fbp = START_VRAM, fbw; // frame buffer pointer and width
		public int zbp = START_VRAM, zbw; // depth buffer pointer and width
		public int psm; // pixel format
		public int region_x1, region_y1, region_x2, region_y2;
		public int region_width, region_height; // derived
		public int scissor_x1, scissor_y1, scissor_x2, scissor_y2;
		public int scissor_width, scissor_height; // derived
		public int offset_x, offset_y;
		public int viewport_width, viewport_height; // derived from xyscale
		public int viewport_cx, viewport_cy;
		public float[] proj_uploaded_matrix = new float[4 * 4];
		public float[] texture_uploaded_matrix = new float[4 * 4];
		public float[] model_uploaded_matrix = new float[4 * 4];
		public float[] view_uploaded_matrix = new float[4 * 4];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] bone_uploaded_matrix = new float[8][4 * 3];
		public float[][] bone_uploaded_matrix = RectangularArrays.ReturnRectangularFloatArray(8, 4 * 3);
		public float[] boneMatrixLinear = new float[8 * 4 * 4]; // Linearized version of bone_uploaded_matrix
		public bool depthMask;
		public int[] colorMask = new int[] {0x00, 0x00, 0x00, 0x00};
		public int alphaFunc;
		public int alphaRef;
		public int alphaMask;
		public int stencilFunc;
		public int stencilRef;
		public int stencilMask;
		public int stencilOpFail;
		public int stencilOpZFail;
		public int stencilOpZPass;
		public int textureFunc;
		public bool textureColorDoubled;
		public bool textureAlphaUsed;
		public bool frontFaceCw;
		public int depthFunc;
		public float[] morph_weight = new float[8];
		public float[] tex_envmap_matrix = new float[4 * 4];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] light_pos = new float[NUM_LIGHTS][4];
		public float[][] light_pos = RectangularArrays.ReturnRectangularFloatArray(NUM_LIGHTS, 4);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] light_dir = new float[NUM_LIGHTS][4];
		public float[][] light_dir = RectangularArrays.ReturnRectangularFloatArray(NUM_LIGHTS, 4);
		public int[] light_type = new int[NUM_LIGHTS];
		public int[] light_kind = new int[NUM_LIGHTS];
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] lightAmbientColor = new float[NUM_LIGHTS][4];
		public float[][] lightAmbientColor = RectangularArrays.ReturnRectangularFloatArray(NUM_LIGHTS, 4);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] lightDiffuseColor = new float[NUM_LIGHTS][4];
		public float[][] lightDiffuseColor = RectangularArrays.ReturnRectangularFloatArray(NUM_LIGHTS, 4);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: public float[][] lightSpecularColor = new float[NUM_LIGHTS][4];
		public float[][] lightSpecularColor = RectangularArrays.ReturnRectangularFloatArray(NUM_LIGHTS, 4);
		public float[] spotLightExponent = new float[NUM_LIGHTS];
		public float[] spotLightCutoff = new float[NUM_LIGHTS];
		public float[] spotLightCosCutoff = new float[NUM_LIGHTS];
		public float[] lightConstantAttenuation = new float[NUM_LIGHTS];
		public float[] lightLinearAttenuation = new float[NUM_LIGHTS];
		public float[] lightQuadraticAttenuation = new float[NUM_LIGHTS];
		public int lightMode;
		public float[] fog_color = new float[4];
		public float fog_far = 0.0f, fog_dist = 0.0f;
		public int nearZ, farZ;
		public float zscale, zpos;
		public int mat_flags = 0;
		public float[] mat_ambient = new float[4];
		public float[] mat_diffuse = new float[4];
		public float[] mat_specular = new float[4];
		public float[] mat_emissive = new float[4];
		public float[] ambient_light = new float[4];
		public float materialShininess;
		public int texture_storage, texture_num_mip_maps;
		public bool texture_swizzle;
		public int[] texture_base_pointer = new int[8];
		public int[] texture_width = new int[8];
		public int[] texture_height = new int[8];
		public int[] texture_buffer_width = new int[8];
		public int tex_min_filter = TFLT_NEAREST;
		public int tex_mag_filter = TFLT_NEAREST;
		public int tex_mipmap_mode;
		public float tex_mipmap_bias;
		public int tex_mipmap_bias_int;
		public bool mipmapShareClut;
		public float tex_translate_x = 0.0f, tex_translate_y = 0.0f;
		public float tex_scale_x = 1.0f, tex_scale_y = 1.0f;
		public float[] tex_env_color = new float[4];
		public int tex_clut_addr;
		public int tex_clut_num_blocks;
		public int tex_clut_mode, tex_clut_shift, tex_clut_mask, tex_clut_start;
		public int tex_wrap_s = TWRAP_WRAP_MODE_REPEAT, tex_wrap_t = TWRAP_WRAP_MODE_REPEAT;
		public int tex_shade_u = 0;
		public int tex_shade_v = 0;
		public int patch_div_s;
		public int patch_div_t;
		public int patch_prim;
		public float tslope_level;
		public int transform_mode;
		public int textureTx_sourceAddress;
		public int textureTx_sourceLineWidth;
		public int textureTx_destinationAddress;
		public int textureTx_destinationLineWidth;
		public int textureTx_width;
		public int textureTx_height;
		public int textureTx_sx;
		public int textureTx_sy;
		public int textureTx_dx;
		public int textureTx_dy;
		public int textureTx_pixelSize;
		public float[] dfix_color = new float[4];
		public float[] sfix_color = new float[4];
		public int sfix;
		public int dfix;
		public int blend_src;
		public int blend_dst;
		public int blendEquation;
		public int[] dither_matrix = new int[16];
		public int tex_map_mode = TMAP_TEXTURE_MAP_MODE_TEXTURE_COORDIATES_UV;
		public int tex_proj_map_mode = TMAP_TEXTURE_PROJECTION_MODE_POSITION;
		public int colorTestFunc;
		public int[] colorTestRef = new int[] {0, 0, 0};
		public int[] colorTestMsk = new int[] {0, 0, 0};
		public int shadeModel;
		public int logicOp;
		public readonly IList<EnableDisableFlag> flags = new LinkedList<EnableDisableFlag>();
		public EnableDisableFlag alphaTestFlag;
		public EnableDisableFlag depthTestFlag;
		public EnableDisableFlag scissorTestFlag;
		public EnableDisableFlag stencilTestFlag;
		public EnableDisableFlag blendFlag;
		public EnableDisableFlag cullFaceFlag;
		public EnableDisableFlag ditherFlag;
		public EnableDisableFlag fogFlag;
		public EnableDisableFlag clipPlanesFlag;
		public EnableDisableFlag textureFlag;
		public EnableDisableFlag lightingFlag;
		public EnableDisableFlag[] lightFlags;
		public EnableDisableFlag lineSmoothFlag;
		public EnableDisableFlag patchCullFaceFlag;
		public EnableDisableFlag colorTestFlag;
		public EnableDisableFlag colorLogicOpFlag;
		public EnableDisableFlag faceNormalReverseFlag;
		public EnableDisableFlag patchFaceFlag;
		public EnableDisableFlag fragment2xFlag;
		public EnableDisableFlag reColorMaterial;
		public EnableDisableFlag reTextureGenS;
		public EnableDisableFlag reTextureGenT;
		public float[] vertexColor = new float[4];
		public bool useVertexColor;
		public bool clearMode;
		public bool clearModeColor;
		public bool clearModeStencil;
		public bool clearModeDepth;
		public VertexInfo vinfo = new VertexInfo();
		public int currentTextureId;

		public GeContext()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			tex_envmap_matrix[0] = tex_envmap_matrix[5] = tex_envmap_matrix[10] = tex_envmap_matrix[15] = 1.0f;
			light_pos[0][3] = light_pos[1][3] = light_pos[2][3] = light_pos[3][3] = 1.0f;
			light_dir[0][3] = light_dir[1][3] = light_dir[2][3] = light_dir[3][3] = 1.0f;
			morph_weight[0] = 1.0f;
			tex_mipmap_mode = TBIAS_MODE_AUTO;
			tex_mipmap_bias = 0.0f;
			tex_mipmap_bias_int = 0;
			mipmapShareClut = true;
			@base = 0;
			baseOffset = 0;

			light_type[0] = light_type[1] = light_type[2] = light_type[3] = -1;
			light_kind[0] = light_kind[1] = light_kind[2] = light_kind[3] = -1;
			lightSpecularColor[0][0] = lightSpecularColor[1][0] = lightSpecularColor[2][0] = lightSpecularColor[3][0] = -1;

			dirty = false;
		}

		public virtual IRenderingEngine RenderingEngine
		{
			set
			{
				this.re = value;
			}
		}

		/// <summary>
		/// Update the RenderingEngine based on the context values.
		/// This method can only be called by a thread being allowed to perform
		/// RenderingEngine calls (i.e. the GUI thread).
		/// </summary>
		public virtual void update()
		{
			if (!dirty)
			{
				return;
			}

			foreach (EnableDisableFlag flag in flags)
			{
				flag.update();
			}
			if (fogFlag.Enabled)
			{
				re.setFogHint();
			}
			if (lineSmoothFlag.Enabled)
			{
				re.setLineSmoothHint();
			}
			re.setPatchDiv(patch_div_s, patch_div_t);
			re.PatchPrim = patch_prim;
			re.ShadeModel = shadeModel;
			re.MaterialEmissiveColor = mat_emissive;
			re.MaterialShininess = materialShininess;
			re.LightModelAmbientColor = ambient_light;
			re.LightMode = lightMode;
			for (int light = 0; light < NUM_LIGHTS; light++)
			{
				re.setLightType(light, light_type[light], light_kind[light]);
				re.setLightConstantAttenuation(light, lightConstantAttenuation[light]);
				re.setLightLinearAttenuation(light, lightLinearAttenuation[light]);
				re.setLightQuadraticAttenuation(light, lightQuadraticAttenuation[light]);
				re.setLightAmbientColor(light, lightAmbientColor[light]);
				re.setLightDiffuseColor(light, lightDiffuseColor[light]);
				re.setLightSpecularColor(light, lightSpecularColor[light]);
			}
			re.FrontFace = frontFaceCw;
			re.TextureEnvColor = tex_env_color;
			re.FogColor = fog_color;
			re.ColorTestFunc = colorTestFunc;
			re.ColorTestReference = colorTestRef;
			re.ColorTestMask = colorTestMsk;
			re.setAlphaFunc(alphaFunc, alphaRef, alphaMask);
			re.setStencilFunc(stencilFunc, stencilRef, stencilMask);
			re.setStencilOp(stencilOpFail, stencilOpZFail, stencilOpZPass);
			re.BlendEquation = blendEquation;
			re.LogicOp = logicOp;
			re.DepthMask = depthMask;
			re.setColorMask(colorMask[0], colorMask[1], colorMask[2], colorMask[3]);
			re.setTextureFunc(textureFunc, textureAlphaUsed, textureColorDoubled);

			dirty = false;
		}

		public virtual void setDirty()
		{
			dirty = true;
		}

		protected internal override void read()
		{
			@base = read32();
			baseOffset = read32();

			fbp = read32();
			fbw = read32();
			zbp = read32();
			zbw = read32();
			psm = read32();

			int flagBits = read32();
			foreach (EnableDisableFlag flag in flags)
			{
				flag.restore(flagBits);
			}

			region_x1 = read32();
			region_y1 = read32();
			region_x2 = read32();
			region_y2 = read32();

			region_width = read32();
			region_height = read32();
			scissor_x1 = read32();
			scissor_y1 = read32();
			scissor_x2 = read32();
			scissor_y2 = read32();
			scissor_width = read32();
			scissor_height = read32();
			offset_x = read32();
			offset_y = read32();
			viewport_width = read32();
			viewport_height = read32();
			viewport_cx = read32();
			viewport_cy = read32();

			readFloatArray(proj_uploaded_matrix);
			readFloatArray(texture_uploaded_matrix);
			readFloatArray(model_uploaded_matrix);
			readFloatArray(view_uploaded_matrix);
			readFloatArray(bone_uploaded_matrix);

			// Rebuild boneMatrixLinear from bone_uploaded_matrix
			for (int matrix = 0, j = 0; matrix < bone_uploaded_matrix.Length; matrix++)
			{
				for (int i = 0; i < 12; i++, j++)
				{
					boneMatrixLinear[(j / 3) * 4 + (j % 3)] = bone_uploaded_matrix[matrix][i];
				}
			}

			depthMask = readBoolean();
			read32Array(colorMask);
			alphaFunc = read32();
			alphaRef = read32();
			alphaMask = read32();
			stencilFunc = read32();
			stencilRef = read32();
			stencilMask = read32();
			stencilOpFail = read32();
			stencilOpZFail = read32();
			stencilOpZPass = read32();
			textureFunc = read32();
			textureColorDoubled = readBoolean();
			textureAlphaUsed = readBoolean();
			frontFaceCw = readBoolean();
			depthFunc = read32();

			readFloatArray(morph_weight);
			readFloatArray(tex_envmap_matrix);
			readFloatArray(light_pos);
			readFloatArray(light_dir);

			read32Array(light_type);
			read32Array(light_kind);
			readFloatArray(lightAmbientColor);
			readFloatArray(lightDiffuseColor);
			readFloatArray(lightSpecularColor);
			readFloatArray(spotLightExponent);
			readFloatArray(spotLightCutoff);
			readFloatArray(lightConstantAttenuation);
			readFloatArray(lightLinearAttenuation);
			readFloatArray(lightQuadraticAttenuation);
			lightMode = read32();

			readFloatArray(fog_color);
			fog_far = readFloat();
			fog_dist = readFloat();

			nearZ = read16();
			farZ = read16();
			zscale = readFloat();
			zpos = readFloat();

			mat_flags = read32();
			readFloatArray(mat_ambient);
			readFloatArray(mat_diffuse);
			readFloatArray(mat_specular);
			readFloatArray(mat_emissive);

			readFloatArray(ambient_light);
			materialShininess = readFloat();

			texture_storage = read32();
			texture_num_mip_maps = read32();
			texture_swizzle = readBoolean();

			read32Array(texture_base_pointer);
			read32Array(texture_width);
			read32Array(texture_height);
			read32Array(texture_buffer_width);
			tex_min_filter = read32();
			tex_mag_filter = read32();
			tex_mipmap_mode = read32();
			tex_mipmap_bias = readFloat();
			tex_mipmap_bias_int = read32();
			mipmapShareClut = readBoolean();

			tex_translate_x = readFloat();
			tex_translate_y = readFloat();
			tex_scale_x = readFloat();
			tex_scale_y = readFloat();
			readFloatArray(tex_env_color);

			tex_clut_addr = read32();
			tex_clut_num_blocks = read32();
			tex_clut_mode = read32();
			tex_clut_shift = read32();
			tex_clut_mask = read32();
			tex_clut_start = read32();
			tex_wrap_s = read32();
			tex_wrap_t = read32();
			tex_shade_u = read32();
			tex_shade_v = read32();
			patch_div_s = read32();
			patch_div_t = read32();
			patch_prim = read32();
			tslope_level = readFloat();

			transform_mode = read32();

			textureTx_sourceAddress = read32();
			textureTx_sourceLineWidth = read32();
			textureTx_destinationAddress = read32();
			textureTx_destinationLineWidth = read32();
			textureTx_width = read32();
			textureTx_height = read32();
			textureTx_sx = read32();
			textureTx_sy = read32();
			textureTx_dx = read32();
			textureTx_dy = read32();
			textureTx_pixelSize = read32();

			readFloatArray(dfix_color);
			readFloatArray(sfix_color);
			blend_src = read32();
			blend_dst = read32();
			blendEquation = read32();

			read32Array(dither_matrix);

			tex_map_mode = read32();
			tex_proj_map_mode = read32();

			colorTestFunc = read32();
			read32Array(colorTestRef);
			read32Array(colorTestMsk);

			shadeModel = read32();
			logicOp = read32();

			VideoEngine.Instance.resetCurrentListCMDValues();

			if (Offset > @sizeof())
			{
				log.error(string.Format("GE context overflow: {0:D} (max allowed={1:D})", Offset, @sizeof()));
			}
			if (log.DebugEnabled)
			{
				log.debug(string.Format("GE context read size: {0:D} (max allowed={1:D})", Offset, @sizeof()));
			}
		}

		protected internal override void write()
		{
			write32(@base);
			write32(baseOffset);

			write32(fbp);
			write32(fbw);
			write32(zbp);
			write32(zbw);
			write32(psm);

			int flagBits = 0;
			foreach (EnableDisableFlag flag in flags)
			{
				flagBits = flag.save(flagBits);
			}
			write32(flagBits);

			write32(region_x1);
			write32(region_y1);
			write32(region_x2);
			write32(region_y2);

			write32(region_width);
			write32(region_height);
			write32(scissor_x1);
			write32(scissor_y1);
			write32(scissor_x2);
			write32(scissor_y2);
			write32(scissor_width);
			write32(scissor_height);
			write32(offset_x);
			write32(offset_y);
			write32(viewport_width);
			write32(viewport_height);
			write32(viewport_cx);
			write32(viewport_cy);

			writeFloatArray(proj_uploaded_matrix);
			writeFloatArray(texture_uploaded_matrix);
			writeFloatArray(model_uploaded_matrix);
			writeFloatArray(view_uploaded_matrix);
			writeFloatArray(bone_uploaded_matrix);

			writeBoolean(depthMask);
			write32Array(colorMask);
			write32(alphaFunc);
			write32(alphaRef);
			write32(alphaMask);
			write32(stencilFunc);
			write32(stencilRef);
			write32(stencilMask);
			write32(stencilOpFail);
			write32(stencilOpZFail);
			write32(stencilOpZPass);
			write32(textureFunc);
			writeBoolean(textureColorDoubled);
			writeBoolean(textureAlphaUsed);
			writeBoolean(frontFaceCw);
			write32(depthFunc);

			writeFloatArray(morph_weight);
			writeFloatArray(tex_envmap_matrix);
			writeFloatArray(light_pos);
			writeFloatArray(light_dir);

			write32Array(light_type);
			write32Array(light_kind);
			writeFloatArray(lightAmbientColor);
			writeFloatArray(lightDiffuseColor);
			writeFloatArray(lightSpecularColor);
			writeFloatArray(spotLightExponent);
			writeFloatArray(spotLightCutoff);
			writeFloatArray(lightConstantAttenuation);
			writeFloatArray(lightLinearAttenuation);
			writeFloatArray(lightQuadraticAttenuation);
			write32(lightMode);

			writeFloatArray(fog_color);
			writeFloat(fog_far);
			writeFloat(fog_dist);

			write16((short) nearZ);
			write16((short) farZ);
			writeFloat(zscale);
			writeFloat(zpos);

			write32(mat_flags);
			writeFloatArray(mat_ambient);
			writeFloatArray(mat_diffuse);
			writeFloatArray(mat_specular);
			writeFloatArray(mat_emissive);

			writeFloatArray(ambient_light);
			writeFloat(materialShininess);

			write32(texture_storage);
			write32(texture_num_mip_maps);
			writeBoolean(texture_swizzle);

			write32Array(texture_base_pointer);
			write32Array(texture_width);
			write32Array(texture_height);
			write32Array(texture_buffer_width);
			write32(tex_min_filter);
			write32(tex_mag_filter);
			write32(tex_mipmap_mode);
			writeFloat(tex_mipmap_bias);
			write32(tex_mipmap_bias_int);
			writeBoolean(mipmapShareClut);

			writeFloat(tex_translate_x);
			writeFloat(tex_translate_y);
			writeFloat(tex_scale_x);
			writeFloat(tex_scale_y);
			writeFloatArray(tex_env_color);

			write32(tex_clut_addr);
			write32(tex_clut_num_blocks);
			write32(tex_clut_mode);
			write32(tex_clut_shift);
			write32(tex_clut_mask);
			write32(tex_clut_start);
			write32(tex_wrap_s);
			write32(tex_wrap_t);
			write32(tex_shade_u);
			write32(tex_shade_v);
			write32(patch_div_s);
			write32(patch_div_t);
			write32(patch_prim);
			writeFloat(tslope_level);

			write32(transform_mode);

			write32(textureTx_sourceAddress);
			write32(textureTx_sourceLineWidth);
			write32(textureTx_destinationAddress);
			write32(textureTx_destinationLineWidth);
			write32(textureTx_width);
			write32(textureTx_height);
			write32(textureTx_sx);
			write32(textureTx_sy);
			write32(textureTx_dx);
			write32(textureTx_dy);
			write32(textureTx_pixelSize);

			writeFloatArray(dfix_color);
			writeFloatArray(sfix_color);
			write32(blend_src);
			write32(blend_dst);
			write32(blendEquation);

			write32Array(dither_matrix);

			write32(tex_map_mode);
			write32(tex_proj_map_mode);

			write32(colorTestFunc);
			write32Array(colorTestRef);
			write32Array(colorTestMsk);

			write32(shadeModel);
			write32(logicOp);

			if (Offset > @sizeof())
			{
				log.error(string.Format("GE context overflow: {0:D} (max allowed={1:D})", Offset, @sizeof()));
			}
			if (log.DebugEnabled)
			{
				log.debug(string.Format("GE context write size: {0:D} (max allowed={1:D})", Offset, @sizeof()));
			}
		}

		public override int @sizeof()
		{
			return SIZE_OF;
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();

			foreach (EnableDisableFlag flag in flags)
			{
				if (flag.Enabled)
				{
					if (result.Length > 0)
					{
						result.Append(", ");
					}
					result.Append(flag.ToString());
				}
			}

			return result.ToString();
		}

		private static int contextBitCount = 0;
		public class EnableDisableFlag
		{
			private readonly GeContext outerInstance;

			internal bool enabled;
			internal readonly int reFlag;
			internal readonly string name;
			internal int contextBit;

			public EnableDisableFlag(GeContext outerInstance, string name, int reFlag)
			{
				this.outerInstance = outerInstance;
				this.name = name;
				this.reFlag = reFlag;
				init();
			}

			internal virtual void init()
			{
				enabled = false;
				contextBit = contextBitCount++;
				outerInstance.flags.Add(this);
			}

			public virtual bool isEnabled()
			{
				return enabled;
			}

			public virtual int EnabledInt
			{
				get
				{
					return enabled ? 1 : 0;
				}
			}

			public virtual void setEnabled(int enabledInt)
			{
				setEnabled(enabledInt != 0);
			}

			/// <summary>
			/// Enable/Disable the flag. Update the flag in RenderingEngine.
			/// </summary>
			/// <param name="enabled">        new flag value </param>
			public virtual void setEnabled(bool enabled)
			{
				this.enabled = enabled;
				update();

				if (log.DebugEnabled && !string.ReferenceEquals(name, null))
				{
					log.debug(string.Format("sceGu{0}({1})", enabled ? "Enable" : "Disable", name));
				}
			}

			public virtual void update()
			{
				// Update the flag in RenderingEngine
				if (enabled)
				{
					outerInstance.re.enableFlag(reFlag);
				}
				else
				{
					outerInstance.re.disableFlag(reFlag);
				}
			}

			public virtual void updateEnabled()
			{
				if (enabled)
				{
					outerInstance.re.enableFlag(reFlag);
				}
			}

			public virtual int save(int bits)
			{
				if (enabled)
				{
					bits |= (1 << contextBit);
				}
				return bits;
			}

			public virtual void restore(int bits)
			{
				enabled = (bits & (1 << contextBit)) != 0;
			}

			public virtual int ReFlag
			{
				get
				{
					return reFlag;
				}
			}

			public override string ToString()
			{
				return name;
			}
		}
	}

}