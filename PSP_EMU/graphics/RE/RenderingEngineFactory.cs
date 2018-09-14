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
namespace pspsharp.graphics.RE
{
	using Modules = pspsharp.HLE.Modules;
	using RESoftware = pspsharp.graphics.RE.software.RESoftware;
	using DurationStatistics = pspsharp.util.DurationStatistics;

	/// <summary>
	/// @author gid15
	/// 
	/// </summary>
	public class RenderingEngineFactory
	{
		private const bool enableDebugProxy = false;
		private const bool enableCheckErrorsProxy = false;
		private const bool enableStatisticsProxy = false;

		private static IRenderingEngine createRenderingEngine(bool enableSoftwareRendering)
		{
			// Build the rendering pipeline, from the last entry to the first one.
			IRenderingEngine re;

			if (enableSoftwareRendering)
			{
				// RenderingEngine using a complete software implementation, i.e. not using the GPU
				re = new RESoftware();
			}
			else
			{
				// RenderingEngine performing the OpenGL calls by using the LWJGL library
				re = RenderingEngineLwjgl.newInstance();
			}

			if (enableCheckErrorsProxy)
			{
				re = new CheckErrorsProxy(re);
			}

			if (enableStatisticsProxy && DurationStatistics.collectStatistics)
			{
				// Proxy collecting statistics for all the calls (number of calls and execution time)
				re = new StatisticsProxy(re);
			}

			if (enableDebugProxy)
			{
				// Proxy logging the calls at the DEBUG level
				re = new DebugProxy(re);
			}

			if (!enableSoftwareRendering)
			{
				if (REShader.useShaders(re))
				{
					// RenderingEngine using shaders
					re = new REShader(re);
				}
				else
				{
					// RenderingEngine using the OpenGL fixed-function pipeline (i.e. without shaders)
					re = new REFixedFunction(re);
				}
			}

			// Proxy removing redundant calls.
			// E.g. calls setting multiple times the same value,
			// or calls with an invalid parameter (e.g. for unused shader uniforms).
			// In the rendering pipeline, the State Proxy has to be called after
			// the Anisotropic/Viewport filters. These are modifying some parameters
			// and the State Proxy has to use the final parameter values.
			re = new StateProxy(re);

			// Proxy implementing a texture anisotropic filter
			re = new AnisotropicFilter(re);

			// Proxy implementing a viewport resizing filter
			re = new ViewportFilter(re);

			// Return the first entry in the pipeline
			return re;
		}

		/// <summary>
		/// Create a rendering engine to be used for processing the GE lists.
		/// </summary>
		/// <returns> the rendering engine to be used </returns>
		public static IRenderingEngine createRenderingEngine()
		{
			return createRenderingEngine(Modules.sceDisplayModule.UsingSoftwareRenderer);
		}

		/// <summary>
		/// Create a rendering engine to be used for display.
		/// This rendering engine forces the use of OpenGL and is not using the software rendering.
		/// </summary>
		/// <returns> the rendering engine to be used for display </returns>
		public static IRenderingEngine createRenderingEngineForDisplay()
		{
			return createRenderingEngine(false);
		}

		/// <summary>
		/// Create a rendering engine to be used when the HLE modules have not yet
		/// been started.
		/// </summary>
		/// <returns> the initial rendering engine </returns>
		public static IRenderingEngine createInitialRenderingEngine()
		{
			IRenderingEngine re = RenderingEngineLwjgl.newInstance();

			if (enableDebugProxy)
			{
				re = new DebugProxy(re);
			}

			return re;
		}
	}

}