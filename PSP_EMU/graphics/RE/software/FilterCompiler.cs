using System;
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
//	import static pspsharp.graphics.GeCommands.CMAT_FLAG_AMBIENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.CMAT_FLAG_DIFFUSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.CMAT_FLAG_SPECULAR;


	using Logger = org.apache.log4j.Logger;

	using RuntimeContext = pspsharp.Allegrex.compiler.RuntimeContext;
	using ClassSpecializer = pspsharp.util.ClassSpecializer;
	using DurationStatistics = pspsharp.util.DurationStatistics;
	using LongLongKey = pspsharp.util.LongLongKey;

	/// <summary>
	/// @author gid15
	/// 
	/// Implementation of a filter compilation.
	/// The class RendererTemplate is specialized using fixed GE values/flags.
	/// </summary>
	public class FilterCompiler
	{
		private static Logger log = VideoEngine.log_Renamed;
		private static FilterCompiler instance;
		private Dictionary<LongLongKey, RendererTemplate> compiledRenderers = new Dictionary<LongLongKey, RendererTemplate>();
		private static int classNameId = 0;

		public static FilterCompiler Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new FilterCompiler();
				}
				return instance;
			}
		}

		private FilterCompiler()
		{
		}

		public virtual RendererTemplate getCompiledRenderer(BasePrimitiveRenderer renderer, LongLongKey id, GeContext context)
		{
			RendererTemplate compiledRenderer = compiledRenderers[id];
			if (compiledRenderer == null)
			{
				compiledRenderer = compileRenderer(renderer, id, context);
				if (compiledRenderer != null)
				{
					compiledRenderers[id] = compiledRenderer;
				}
			}

			return compiledRenderer;
		}

		private static string NewCompiledRendererClassName
		{
			get
			{
				return string.Format("Renderer{0:D}", classNameId++);
			}
		}

		private RendererTemplate compileRenderer(BasePrimitiveRenderer renderer, LongLongKey id, GeContext context)
		{
			if (log.InfoEnabled)
			{
				log.info(string.Format("Compiling Renderer {0}", id));
			}

			Dictionary<string, object> variables = new Dictionary<string, object>();
			// All these variables have to be defined as static members in the class RendererTemplate.
			variables["hasMemInt"] = Convert.ToBoolean(RuntimeContext.hasMemoryInt());
			variables["transform2D"] = Convert.ToBoolean(renderer.transform2D);
			variables["clearMode"] = Convert.ToBoolean(renderer.clearMode);
			variables["clearModeColor"] = Convert.ToBoolean(context.clearModeColor);
			variables["clearModeStencil"] = Convert.ToBoolean(context.clearModeStencil);
			variables["clearModeDepth"] = Convert.ToBoolean(context.clearModeDepth);
			variables["needSourceDepthRead"] = Convert.ToBoolean(renderer.needSourceDepthRead);
			variables["needDestinationDepthRead"] = Convert.ToBoolean(renderer.needDestinationDepthRead);
			variables["needDepthWrite"] = Convert.ToBoolean(renderer.needDepthWrite);
			variables["needTextureUV"] = Convert.ToBoolean(renderer.needTextureUV);
			variables["simpleTextureUV"] = Convert.ToBoolean(renderer.simpleTextureUV);
			variables["swapTextureUV"] = Convert.ToBoolean(renderer.swapTextureUV);
			variables["needScissoringX"] = Convert.ToBoolean(renderer.needScissoringX);
			variables["needScissoringY"] = Convert.ToBoolean(renderer.needScissoringY);
			variables["nearZ"] = new int?(renderer.nearZ);
			variables["farZ"] = new int?(renderer.farZ);
			variables["colorTestFlagEnabled"] = Convert.ToBoolean(context.colorTestFlag.Enabled);
			variables["colorTestFunc"] = new int?(context.colorTestFunc);
			variables["alphaTestFlagEnabled"] = Convert.ToBoolean(context.alphaTestFlag.Enabled);
			variables["alphaFunc"] = new int?(context.alphaFunc);
			variables["alphaRef"] = new int?(context.alphaRef);
			variables["alphaMask"] = new int?(context.alphaMask);
			variables["stencilTestFlagEnabled"] = Convert.ToBoolean(context.stencilTestFlag.Enabled);
			variables["stencilFunc"] = new int?(context.stencilFunc);
			variables["stencilOpFail"] = new int?(context.stencilOpFail);
			variables["stencilOpZFail"] = new int?(context.stencilOpZFail);
			variables["stencilOpZPass"] = new int?(context.stencilOpZPass);
			variables["stencilRef"] = new int?(context.stencilRef);
			variables["depthTestFlagEnabled"] = Convert.ToBoolean(context.depthTestFlag.Enabled);
			variables["depthFunc"] = new int?(context.depthFunc);
			variables["blendFlagEnabled"] = Convert.ToBoolean(context.blendFlag.Enabled);
			variables["blendEquation"] = new int?(context.blendEquation);
			variables["blendSrc"] = new int?(context.blend_src);
			variables["blendDst"] = new int?(context.blend_dst);
			variables["sfix"] = new int?(context.sfix);
			variables["dfix"] = new int?(context.dfix);
			variables["colorLogicOpFlagEnabled"] = Convert.ToBoolean(context.colorLogicOpFlag.Enabled);
			variables["logicOp"] = new int?(context.logicOp);
			variables["colorMask"] = new int?(PixelColor.getColor(context.colorMask));
			variables["depthMask"] = Convert.ToBoolean(context.depthMask);
			variables["textureFlagEnabled"] = Convert.ToBoolean(context.textureFlag.Enabled);
			variables["useVertexTexture"] = Convert.ToBoolean(renderer.useVertexTexture);
			variables["lightingFlagEnabled"] = Convert.ToBoolean(context.lightingFlag.Enabled);
			variables["sameVertexColor"] = Convert.ToBoolean(renderer.sameVertexColor);
			variables["setVertexPrimaryColor"] = Convert.ToBoolean(renderer.setVertexPrimaryColor);
			variables["primaryColorSetGlobally"] = Convert.ToBoolean(renderer.primaryColorSetGlobally);
			variables["isTriangle"] = Convert.ToBoolean(renderer.isTriangle);
			variables["matFlagAmbient"] = Convert.ToBoolean((context.mat_flags & CMAT_FLAG_AMBIENT) != 0);
			variables["matFlagDiffuse"] = Convert.ToBoolean((context.mat_flags & CMAT_FLAG_DIFFUSE) != 0);
			variables["matFlagSpecular"] = Convert.ToBoolean((context.mat_flags & CMAT_FLAG_SPECULAR) != 0);
			variables["useVertexColor"] = Convert.ToBoolean(context.useVertexColor);
			variables["textureColorDoubled"] = Convert.ToBoolean(context.textureColorDoubled);
			variables["lightMode"] = new int?(context.lightMode);
			variables["texMapMode"] = new int?(context.tex_map_mode);
			variables["texProjMapMode"] = new int?(context.tex_proj_map_mode);
			variables["texTranslateX"] = new float?(context.tex_translate_x);
			variables["texTranslateY"] = new float?(context.tex_translate_y);
			variables["texScaleX"] = new float?(context.tex_scale_x);
			variables["texScaleY"] = new float?(context.tex_scale_y);
			variables["texWrapS"] = new int?(context.tex_wrap_s);
			variables["texWrapT"] = new int?(context.tex_wrap_t);
			variables["textureFunc"] = new int?(context.textureFunc);
			variables["textureAlphaUsed"] = Convert.ToBoolean(context.textureAlphaUsed);
			variables["psm"] = new int?(context.psm);
			variables["texMagFilter"] = new int?(context.tex_mag_filter);
			variables["needTextureWrapU"] = Convert.ToBoolean(renderer.needTextureWrapU);
			variables["needTextureWrapV"] = Convert.ToBoolean(renderer.needTextureWrapV);
			variables["needSourceDepthClamp"] = Convert.ToBoolean(renderer.needSourceDepthClamp);
			variables["isLogTraceEnabled"] = Convert.ToBoolean(renderer.isLogTraceEnabled);
			variables["collectStatistics"] = Convert.ToBoolean(DurationStatistics.collectStatistics);
			variables["ditherFlagEnabled"] = Convert.ToBoolean(context.ditherFlag.Enabled);

			string specializedClassName = NewCompiledRendererClassName;
			ClassSpecializer cs = new ClassSpecializer();
			Type specializedClass = cs.specialize(specializedClassName, typeof(RendererTemplate), variables);
			RendererTemplate compiledRenderer = null;
			if (specializedClass != null)
			{
				try
				{
					compiledRenderer = (RendererTemplate) System.Activator.CreateInstance(specializedClass);
				}
				catch (InstantiationException e)
				{
					log.error("Error while instanciating compiled renderer", e);
				}
				catch (IllegalAccessException e)
				{
					log.error("Error while instanciating compiled renderer", e);
				}
			}

			return compiledRenderer;
		}

		public static void exit()
		{
			if (instance == null)
			{
				return;
			}

			if (log.InfoEnabled && DurationStatistics.collectStatistics)
			{
				DurationStatistics[] statistics = new DurationStatistics[instance.compiledRenderers.Count];
				int n = 0;
				foreach (RendererTemplate renderer in instance.compiledRenderers.Values)
				{
					statistics[n++] = renderer.Statistics;
				}
				Arrays.sort(statistics, 0, n);
				for (int i = 0; i < n; i++)
				{
					log.info(statistics[i]);
				}
			}
		}
	}

}