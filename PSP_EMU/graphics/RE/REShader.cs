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
namespace pspsharp.graphics.RE
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceDisplay.getTexturePixelFormat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_4BIT_INDEXED;


	using Modules = pspsharp.HLE.Modules;
	using sceDisplay = pspsharp.HLE.modules.sceDisplay;
	using FBTexture = pspsharp.graphics.textures.FBTexture;
	using GETexture = pspsharp.graphics.textures.GETexture;
	using Texture = pspsharp.graphics.textures.Texture;
	using TextureCache = pspsharp.graphics.textures.TextureCache;
	using Settings = pspsharp.settings.Settings;
	using CpuDurationStatistics = pspsharp.util.CpuDurationStatistics;
	using DurationStatistics = pspsharp.util.DurationStatistics;
	using Utilities = pspsharp.util.Utilities;

	/// <summary>
	/// @author gid15
	/// 
	/// This RenderingEngine class implements the required logic
	/// to use OpenGL vertex and fragment shaders, i.e. without using
	/// the OpenGL fixed-function pipeline.
	/// If geometry shaders are available, implement the GU_SPRITES primitive by
	/// inserting a geometry shader into the shader program.
	/// 
	/// This class is implemented as a Proxy, forwarding the non-relevant calls
	/// to the proxy.
	/// </summary>
	public class REShader : BaseRenderingEngineFunction
	{
		public const int ACTIVE_TEXTURE_NORMAL = 0;
		protected internal const int ACTIVE_TEXTURE_CLUT = 1;
		protected internal const int ACTIVE_TEXTURE_FRAMEBUFFER = 2;
		protected internal const int ACTIVE_TEXTURE_INTEGER = 3;
		protected internal static readonly float[] positionScale = new float[] {1, 0x7F, 0x7FFF, 1};
		protected internal static readonly float[] normalScale = new float[] {1, 0x7F, 0x7FFF, 1};
		protected internal static readonly float[] textureScale = new float[] {1, 0x80, 0x8000, 1};
		protected internal static readonly float[] weightScale = new float[] {1, 0x80, 0x8000, 1};
		protected internal const string attributeNameTexture = "pspTexture";
		protected internal const string attributeNameColor = "pspColor";
		protected internal const string attributeNameNormal = "pspNormal";
		protected internal const string attributeNamePosition = "pspPosition";
		protected internal const string attributeNameWeights1 = "pspWeights1";
		protected internal const string attributeNameWeights2 = "pspWeights2";
		protected internal ShaderContext shaderContext;
		protected internal ShaderProgram defaultShaderProgram;
		protected internal ShaderProgram defaultSpriteShaderProgram;
		protected internal int numberOfWeightsForShader;
		protected internal const int spriteGeometryShaderInputType = IRenderingEngine_Fields.GU_LINES;
		protected internal const int spriteGeometryShaderOutputType = IRenderingEngine_Fields.GU_TRIANGLE_STRIP;
		protected internal bool useGeometryShader;
		protected internal bool useUniformBufferObject = true;
		protected internal bool useNativeClut;
		protected internal bool useShaderDepthTest = false;
		protected internal bool useShaderStencilTest = false;
		protected internal bool useShaderColorMask = false;
		// Always use the alpha test implementation in the shader
		// to support the alpha test mask (not being supported by OpenGL)
		protected internal bool useShaderAlphaTest = true;
		protected internal bool useShaderBlendTest = false;
		protected internal bool useRenderToTexture = false;
		protected internal bool useTextureBarrier = false;
		protected internal int clutTextureId = -1;
		protected internal ByteBuffer clutBuffer;
		protected internal DurationStatistics textureCacheLookupStatistics = new CpuDurationStatistics("Lookup in TextureCache for CLUTs");
		protected internal string shaderStaticDefines;
		protected internal string shaderDummyDynamicDefines;
		protected internal int shaderVersion = 120;
		protected internal ShaderProgramManager shaderProgramManager;
		protected internal bool useDynamicShaders;
		protected internal ShaderProgram currentShaderProgram;
		protected internal StringBuilder infoLogs;
		protected internal GETexture fbTexture;
		protected internal bool stencilTestFlag;
		protected internal int viewportWidth;
		protected internal int viewportHeight;
		protected internal FBTexture renderTexture;
		protected internal FBTexture copyOfRenderTexture;
		protected internal int pixelFormat;

		public REShader(IRenderingEngine proxy) : base(proxy)
		{
			initShader();
		}

		protected internal virtual void initShader()
		{
			log.info("Using shaders with Skinning");

			useDynamicShaders = Settings.Instance.readBool("emu.enabledynamicshaders");
			if (useDynamicShaders)
			{
				log.info("Using dynamic shaders");
				shaderProgramManager = new ShaderProgramManager();
			}

			useGeometryShader = Settings.Instance.readBool("emu.useGeometryShader");

			if (!re.isExtensionAvailable("GL_ARB_geometry_shader4"))
			{
				useGeometryShader = false;
			}
			if (useGeometryShader)
			{
				log.info("Using Geometry Shader for SPRITES");
			}

			if (!ShaderContextUBO.useUBO(re))
			{
				useUniformBufferObject = false;
			}
			if (useUniformBufferObject)
			{
				log.info("Using Uniform Buffer Object (UBO)");
			}

			useNativeClut = Settings.Instance.readBool("emu.enablenativeclut");
			if (useNativeClut)
			{
				if (!base.canNativeClut(0, -1, false))
				{
					log.warn("Disabling Native Color Lookup Tables (CLUT)");
					useNativeClut = false;
				}
				else
				{
					log.info("Using Native Color Lookup Tables (CLUT)");
				}
			}

			useShaderStencilTest = Settings.Instance.readBool("emu.enableshaderstenciltest");
			useShaderColorMask = Settings.Instance.readBool("emu.enableshadercolormask");

			if (useUniformBufferObject)
			{
				shaderContext = new ShaderContextUBO(re);
			}
			else
			{
				shaderContext = new ShaderContext();
			}

			if (useShaderStencilTest)
			{
				// When implementing the stencil test in the fragment shader,
				// we need to implement the alpha test and blend test
				// in the shader as well.
				// The alpha test has to be performed before the stencil test
				// in order to test the correct alpha values because these are updated
				// by the stencil test.
				// The alpha test of the fixed OpenGL functionality is always
				// executed after the shader execution and would then use
				// incorrect alpha test values.
				// The blend test has also to use the correct alpha value,
				// i.e. the alpha value before the stencil test.
				useShaderAlphaTest = true;
				useShaderBlendTest = true;
				useShaderDepthTest = true;
			}

			if (useShaderStencilTest || useShaderBlendTest || useShaderColorMask)
			{
				// If we are using shaders requiring the current frame buffer content
				// as a texture, activate the rendering to a texture if available.
				if (re.FramebufferObjectAvailable)
				{
					useRenderToTexture = true;
					useTextureBarrier = re.TextureBarrierAvailable;
					log.info(string.Format("Rendering to a texture with {0}", useTextureBarrier ? "texture barrier" : "texture blit (slow)"));
				}
				else
				{
					log.info("Not rendering to a texture, FBO's are not supported by your graphics card. This will have a negative performance impact.");
				}
			}

			initShadersDefines();
			loadShaders();
			if (defaultShaderProgram == null)
			{
				return;
			}

			shaderContext.ColorDoubling = 1;

			shaderContext.Tex = ACTIVE_TEXTURE_NORMAL;
			shaderContext.FbTex = ACTIVE_TEXTURE_FRAMEBUFFER;
			if (useNativeClut)
			{
				shaderContext.Clut = ACTIVE_TEXTURE_CLUT;
				shaderContext.Utex = ACTIVE_TEXTURE_INTEGER;
				clutBuffer = ByteBuffer.allocateDirect(4096 * 4).order(ByteOrder.LITTLE_ENDIAN);
			}
		}

		protected internal virtual bool ValidShader
		{
			get
			{
				if (defaultShaderProgram == null)
				{
					return false;
				}
    
				return true;
			}
		}

		protected internal static void addDefine(StringBuilder defines, string name, string value)
		{
			defines.Append(string.Format("#define {0} {1}{2}", name, escapeString(value), System.getProperty("line.separator")));
		}

		protected internal static void addDefine(StringBuilder defines, string name, int value)
		{
			addDefine(defines, name, Convert.ToString(value));
		}

		protected internal static void addDefine(StringBuilder defines, string name, bool value)
		{
			addDefine(defines, name, value ? 1 : 0);
		}

		protected internal virtual void replace(StringBuilder s, string oldText, string newText)
		{
			int offset = s.ToString().IndexOf(oldText);
			if (offset >= 0)
			{
				s.Remove(offset, offset + oldText.Length - offset).Insert(offset, newText);
			}
		}

		protected internal static string escapeString(string s)
		{
			return s.Replace('\n', ' ');
		}

		protected internal virtual int AvailableShadingLanguageVersion
		{
			get
			{
				int availableVersion = 0;
    
				string shadingLanguageVersion = re.ShadingLanguageVersion;
				if (string.ReferenceEquals(shadingLanguageVersion, null))
				{
					return availableVersion;
				}
    
				Matcher matcher = Pattern.compile("(\\d+)\\.(\\d+).*").matcher(shadingLanguageVersion);
				if (!matcher.matches())
				{
					log.error(string.Format("Cannot parse Shading Language Version '{0}'", shadingLanguageVersion));
					return availableVersion;
				}
    
				try
				{
					int majorNumber = int.Parse(matcher.group(1));
					int minorNumber = int.Parse(matcher.group(2));
    
					availableVersion = majorNumber * 100 + minorNumber;
				}
				catch (System.FormatException)
				{
					log.error(string.Format("Cannot parse Shading Language Version '{0}'", shadingLanguageVersion));
				}
    
				return availableVersion;
			}
		}

		protected internal virtual void initShadersDefines()
		{
			StringBuilder staticDefines = new StringBuilder();

			if (AvailableShadingLanguageVersion >= 140)
			{
				// Use at least version 1.40 when available.
				// Version 1.20/1.30 is causing problems with AMD drivers.
				shaderVersion = System.Math.Max(140, shaderVersion);
			}

			addDefine(staticDefines, "USE_GEOMETRY_SHADER", useGeometryShader);
			addDefine(staticDefines, "USE_UBO", useUniformBufferObject);
			if (useUniformBufferObject)
			{
				// UBO requires at least shader version 1.40
				shaderVersion = System.Math.Max(140, shaderVersion);
				addDefine(staticDefines, "UBO_STRUCTURE", ((ShaderContextUBO) shaderContext).ShaderUniformText);
			}

			addDefine(staticDefines, "USE_NATIVE_CLUT", useNativeClut);
			if (useNativeClut)
			{
				// Native clut requires at least shader version 1.30
				shaderVersion = System.Math.Max(130, shaderVersion);
			}

			if (useShaderStencilTest || useShaderBlendTest || useShaderColorMask)
			{
				// Function texelFetch requires at least shader version 1.30
				shaderVersion = System.Math.Max(130, shaderVersion);
			}

			bool useBitOperators = re.isExtensionAvailable("GL_EXT_gpu_shader4");
			addDefine(staticDefines, "USE_BIT_OPERATORS", useBitOperators);
			if (!useBitOperators)
			{
				log.info("Extension GL_EXT_gpu_shader4 not available: not using bit operators in shader");
			}

			shaderStaticDefines = staticDefines.ToString();
			shaderDummyDynamicDefines = ShaderProgram.DummyDynamicDefines;

			if (log.DebugEnabled)
			{
				log.debug(string.Format("Using shader version {0:D}, available shading language version {1:D}", shaderVersion, AvailableShadingLanguageVersion));
			}
		}

		protected internal virtual void preprocessShader(StringBuilder src, ShaderProgram shaderProgram)
		{
			StringBuilder defines = new StringBuilder(shaderStaticDefines);

			bool useDynamicDefines;
			if (shaderProgram != null)
			{
				defines.Append(shaderProgram.DynamicDefines);
				useDynamicDefines = true;
			}
			else
			{
				// Set dummy values to the dynamic defines
				// so that the preprocessor doesn't complain about undefined values
				defines.Append(shaderDummyDynamicDefines);
				useDynamicDefines = false;
			}
			addDefine(defines, "USE_DYNAMIC_DEFINES", useDynamicDefines);

			replace(src, "// INSERT VERSION", string.Format("#version {0:D}", shaderVersion));
			replace(src, "// INSERT DEFINES", defines.ToString());
		}

		protected internal virtual bool loadShader(int shader, string resourceName, bool silentError, ShaderProgram shaderProgram)
		{
			StringBuilder src = new StringBuilder();

			try
			{
				System.IO.Stream resourceStream = this.GetType().getResourceAsStream(resourceName);
				if (resourceStream == null)
				{
					return false;
				}
				src.Append(Utilities.ToString(resourceStream, true));
			}
			catch (IOException e)
			{
				log.error(e);
				return false;
			}

			preprocessShader(src, shaderProgram);

			if (log.TraceEnabled)
			{
				log.trace(string.Format("Compiling shader {0:D} from {1}:\n{2}", shader, resourceName, src.ToString()));
			}

			bool compiled = re.compilerShader(shader, src.ToString());
			if (compiled || !silentError)
			{
				addShaderInfoLog(shader);
			}

			return compiled;
		}

		/// <summary>
		/// Link and validate a shader program.
		/// </summary>
		/// <param name="program">       the program to be linked </param>
		/// <returns> true         if the link was successful
		///         false        if the link was not successful </returns>
		protected internal virtual bool linkShaderProgram(int program)
		{
			bool linked = re.linkProgram(program);
			addProgramInfoLog(program);

			// Trying to avoid warning message from AMD drivers:
			// "Validation warning! - Sampler value tex has not been set."
			// and error message:
			// "Validation failed! - Different sampler types for same sample texture unit in fragment shader"
			re.useProgram(program);
			re.setUniform(re.getUniformLocation(program, Uniforms.tex.UniformString), ACTIVE_TEXTURE_NORMAL);
			re.setUniform(re.getUniformLocation(program, Uniforms.fbTex.UniformString), ACTIVE_TEXTURE_FRAMEBUFFER);
			if (useNativeClut)
			{
				re.setUniform(re.getUniformLocation(program, Uniforms.clut.UniformString), ACTIVE_TEXTURE_CLUT);
				re.setUniform(re.getUniformLocation(program, Uniforms.utex.UniformString), ACTIVE_TEXTURE_INTEGER);
			}

			bool validated = re.validateProgram(program);
			addProgramInfoLog(program);

			return linked && validated;
		}

		protected internal virtual ShaderProgram createShader(bool hasGeometryShader, ShaderProgram shaderProgram)
		{
			infoLogs = new StringBuilder();

			int programId = tryCreateShader(hasGeometryShader, shaderProgram);
			if (programId == -1)
			{
				printInfoLog(true);
				shaderProgram = null;
			}
			else
			{
				if (shaderProgram == null)
				{
					shaderProgram = new ShaderProgram();
				}
				shaderProgram.setProgramId(re, programId);
				printInfoLog(false);
			}

			return shaderProgram;
		}

		private int tryCreateShader(bool hasGeometryShader, ShaderProgram shaderProgram)
		{
			int vertexShader = re.createShader(IRenderingEngine_Fields.RE_VERTEX_SHADER);
			int fragmentShader = re.createShader(IRenderingEngine_Fields.RE_FRAGMENT_SHADER);

			if (!loadShader(vertexShader, "/pspsharp/graphics/shader.vert", false, shaderProgram))
			{
				return -1;
			}
			if (!loadShader(fragmentShader, "/pspsharp/graphics/shader.frag", false, shaderProgram))
			{
				return -1;
			}

			int program = re.createProgram();
			re.attachShader(program, vertexShader);
			re.attachShader(program, fragmentShader);

			if (hasGeometryShader)
			{
				int geometryShader = re.createShader(IRenderingEngine_Fields.RE_GEOMETRY_SHADER);
				bool compiled;
				compiled = loadShader(geometryShader, "/pspsharp/graphics/shader-150.geom", false, shaderProgram);
				if (compiled)
				{
					log.info("Using Geometry Shader shader-150.geom");
				}
				else
				{
					compiled = loadShader(geometryShader, "/pspsharp/graphics/shader-120.geom", false, shaderProgram);
					if (compiled)
					{
						log.info("Using Geometry Shader shader-120.geom");
					}
				}

				if (!compiled)
				{
					return -1;
				}
				re.attachShader(program, geometryShader);
				re.setProgramParameter(program, IRenderingEngine_Fields.RE_GEOMETRY_INPUT_TYPE, spriteGeometryShaderInputType);
				re.setProgramParameter(program, IRenderingEngine_Fields.RE_GEOMETRY_OUTPUT_TYPE, spriteGeometryShaderOutputType);
				re.setProgramParameter(program, IRenderingEngine_Fields.RE_GEOMETRY_VERTICES_OUT, 4);
			}

			// Use the same attribute index values for all shader programs.
			//
			// Issue: AMD driver is incorrectly handling attributes referenced in a shader
			// (even when not really used, e.g. in an "if" statement)
			// but disabled using disableVertexAttribArray. The solution for this
			// issue is to use dynamic shaders.
			//
			// Read on AMD forum: the vertex attribute 0 has to be defined.
			// Using the position here, as it is always defined.
			//
			int index = 0;
			re.bindAttribLocation(program, index++, attributeNamePosition);
			re.bindAttribLocation(program, index++, attributeNameTexture);
			re.bindAttribLocation(program, index++, attributeNameColor);
			re.bindAttribLocation(program, index++, attributeNameNormal);
			re.bindAttribLocation(program, index++, attributeNameWeights1);
			re.bindAttribLocation(program, index++, attributeNameWeights2);

			bool linked = linkShaderProgram(program);
			if (!linked)
			{
				return -1;
			}

			re.useProgram(program);

			if (log.DebugEnabled)
			{
				int shaderAttribWeights1 = re.getAttribLocation(program, attributeNameWeights1);
				int shaderAttribWeights2 = re.getAttribLocation(program, attributeNameWeights2);
				int shaderAttribPosition = re.getAttribLocation(program, attributeNamePosition);
				int shaderAttribNormal = re.getAttribLocation(program, attributeNameNormal);
				int shaderAttribColor = re.getAttribLocation(program, attributeNameColor);
				int shaderAttribTexture = re.getAttribLocation(program, attributeNameTexture);
				log.debug(string.Format("Program {0:D} attribute locations: weights1={1:D}, weights2={2:D}, position={3:D}, normal={4:D}, color={5:D}, texture={6:D}", program, shaderAttribWeights1, shaderAttribWeights2, shaderAttribPosition, shaderAttribNormal, shaderAttribColor, shaderAttribTexture));
			}

			foreach (Uniforms uniform in Uniforms.values())
			{
				uniform.allocateId(re, program);
			}

			shaderContext.initShaderProgram(re, program);

			return program;
		}

		protected internal virtual void loadShaders()
		{
			defaultShaderProgram = createShader(false, null);
			if (defaultShaderProgram != null)
			{
				if (useGeometryShader)
				{
					defaultSpriteShaderProgram = createShader(true, null);
				}

				defaultShaderProgram.use(re);
			}

			if (defaultSpriteShaderProgram == null)
			{
				useGeometryShader = false;
			}
		}

		public static bool useShaders(IRenderingEngine re)
		{
			if (!Settings.Instance.readBool("emu.useshaders"))
			{
				return false;
			}

			if (!re.ShaderAvailable)
			{
				log.info("Shaders are not available on your computer. They have been automatically disabled.");
				return false;
			}

			REShader reTestShader = new REShader(re);
			if (!reTestShader.ValidShader)
			{
				log.warn("Shaders do not run correctly on your computer. They have been automatically disabled.");
				return false;
			}

			return true;
		}

		protected internal virtual void printInfoLog(bool isError)
		{
			if (infoLogs != null && infoLogs.Length > 0)
			{
				if (isError)
				{
					log.error("Shader error log: " + infoLogs);
				}
				else
				{
					// Remove all the useless AMD messages
					string infoLog = infoLogs.ToString();
					infoLog = infoLog.Replace("Vertex shader was successfully compiled to run on hardware.\n", "");
					infoLog = infoLog.Replace("Fragment shader was successfully compiled to run on hardware.\n", "");
					infoLog = infoLog.Replace("Geometry shader was successfully compiled to run on hardware.\n", "");
					infoLog = infoLog.Replace("Fragment shader(s) linked, vertex shader(s) linked. \n", "");
					infoLog = infoLog.Replace("Vertex shader(s) linked, fragment shader(s) linked. \n", "");
					infoLog = infoLog.Replace("Vertex shader(s) linked, fragment shader(s) linked.\n", "");
					infoLog = infoLog.Replace("Validation successful.\n", "");

					if (infoLog.Length > 0)
					{
						log.warn("Shader log: " + infoLog);
					}
				}
			}
		}

		protected internal virtual void addInfoLog(string infoLog)
		{
			if (!string.ReferenceEquals(infoLog, null) && infoLog.Length > 0)
			{
				infoLogs.Append(infoLog);
			}
		}

		protected internal virtual void addShaderInfoLog(int shader)
		{
			string infoLog = re.getShaderInfoLog(shader);
			addInfoLog(infoLog);
		}

		protected internal virtual void addProgramInfoLog(int program)
		{
			string infoLog = re.getProgramInfoLog(program);
			addInfoLog(infoLog);
		}

		/// <summary>
		/// Set the given flag in the shader context, if relevant. </summary>
		/// <param name="flag">   the flag to be set </param>
		/// <param name="value">  the value of the flag (0 is disabled, 1 is enabled) </param>
		/// <returns>       true if the flag as to be enabled in OpenGL as well,
		///               false if the flag has to stay disabled in OpenGL. </returns>
		protected internal virtual bool setShaderFlag(int flag, int value)
		{
			bool setFlag = true;

			switch (flag)
			{
				case IRenderingEngine_Fields.GU_LIGHT0:
				case IRenderingEngine_Fields.GU_LIGHT1:
				case IRenderingEngine_Fields.GU_LIGHT2:
				case IRenderingEngine_Fields.GU_LIGHT3:
					shaderContext.setLightEnabled(flag - IRenderingEngine_Fields.GU_LIGHT0, value);
					setFlag = false;
					break;
				case IRenderingEngine_Fields.GU_COLOR_TEST:
					shaderContext.CtestEnable = value;
					setFlag = false;
					break;
				case IRenderingEngine_Fields.GU_LIGHTING:
					shaderContext.LightingEnable = value;
					setFlag = false;
					break;
				case IRenderingEngine_Fields.GU_TEXTURE_2D:
					shaderContext.TexEnable = value;
					break;
				case IRenderingEngine_Fields.GU_DEPTH_TEST:
					if (useShaderDepthTest)
					{
						shaderContext.DepthTestEnable = value;
					}
					break;
				case IRenderingEngine_Fields.GU_STENCIL_TEST:
					if (useShaderStencilTest)
					{
						shaderContext.StencilTestEnable = value;
						stencilTestFlag = (value != 0);
						AlphaMask = stencilTestFlag;
						setFlag = false;
					}
					break;
				case IRenderingEngine_Fields.GU_ALPHA_TEST:
					if (useShaderAlphaTest)
					{
						shaderContext.AlphaTestEnable = value;
						setFlag = false;
					}
					break;
				case IRenderingEngine_Fields.GU_BLEND:
					if (useShaderBlendTest)
					{
						shaderContext.BlendTestEnable = value;
						setFlag = false;
					}
					break;
				case IRenderingEngine_Fields.GU_FOG:
					shaderContext.FogEnable = value;
					setFlag = false;
					break;
				case IRenderingEngine_Fields.GU_CLIP_PLANES:
					shaderContext.ClipPlaneEnable = value;
					setFlag = false;
					break;
			}

			return setFlag;
		}

		public override void exit()
		{
			if (DurationStatistics.collectStatistics)
			{
				if (useNativeClut)
				{
					log.info(textureCacheLookupStatistics);
				}
			}
			base.exit();
		}

		public override void enableFlag(int flag)
		{
			if (canUpdateFlag(flag))
			{
				if (setShaderFlag(flag, 1))
				{
					base.enableFlag(flag);
				}
			}
		}

		public override void disableFlag(int flag)
		{
			if (canUpdateFlag(flag))
			{
				if (setShaderFlag(flag, 0))
				{
					base.disableFlag(flag);
				}
			}
		}

		public override void setDepthRange(float zpos, float zscale, int near, int far)
		{
			shaderContext.ZPos = zpos;
			shaderContext.ZScale = zscale;
			base.setDepthRange(zpos, zscale, near, far);
		}

		public override int LightMode
		{
			set
			{
				shaderContext.LightMode = value;
				base.LightMode = value;
			}
		}

		public override void setLightType(int light, int type, int kind)
		{
			shaderContext.setLightType(light, type);
			shaderContext.setLightKind(light, kind);
			base.setLightType(light, type, kind);
		}

		public override void setTextureEnvironmentMapping(int u, int v)
		{
			shaderContext.setTexShade(0, u);
			shaderContext.setTexShade(1, v);
			base.setTextureEnvironmentMapping(u, v);
		}

		public override int ColorTestFunc
		{
			set
			{
				shaderContext.CtestFunc = value;
				base.ColorTestFunc = value;
			}
		}

		public override int[] ColorTestMask
		{
			set
			{
				shaderContext.setCtestMsk(0, value[0]);
				shaderContext.setCtestMsk(1, value[1]);
				shaderContext.setCtestMsk(2, value[2]);
				base.ColorTestMask = value;
			}
		}

		public override int[] ColorTestReference
		{
			set
			{
				shaderContext.setCtestRef(0, value[0]);
				shaderContext.setCtestRef(1, value[1]);
				shaderContext.setCtestRef(2, value[2]);
				base.ColorTestReference = value;
			}
		}

		public override void setTextureFunc(int func, bool alphaUsed, bool colorDoubled)
		{
			shaderContext.setTexEnvMode(0, func);
			shaderContext.setTexEnvMode(1, alphaUsed ? 1 : 0);
			shaderContext.ColorDoubling = colorDoubled ? 2.0f : 1.0f;
			base.setTextureFunc(func, alphaUsed, colorDoubled);
		}

		public override int setBones(int count, float[] values)
		{
			shaderContext.NumberBones = count;
			shaderContext.setBoneMatrix(count, values);
			numberOfWeightsForShader = count;
			base.setBones(count, values);

			return numberOfWeightsForShader; // Number of weights to be copied into the Buffer
		}

		public override void setTextureMapMode(int mode, int proj)
		{
			shaderContext.TexMapMode = mode;
			shaderContext.TexMapProj = proj;
			base.setTextureMapMode(mode, proj);
		}

		public override void setColorMaterial(bool ambient, bool diffuse, bool specular)
		{
			shaderContext.setMatFlags(0, ambient ? 1 : 0);
			shaderContext.setMatFlags(1, diffuse ? 1 : 0);
			shaderContext.setMatFlags(2, specular ? 1 : 0);
			base.setColorMaterial(ambient, diffuse, specular);
		}

		public override void startDisplay()
		{
			defaultShaderProgram.use(re);

			if (useRenderToTexture)
			{
				sceDisplay display = Modules.sceDisplayModule;
				int width = display.WidthFb;
				int height = display.HeightFb;
				int bufferWidth = display.BufferWidthFb;
				int pixelFormat = display.PixelFormatFb;

				// if the format of the Frame Buffer has changed, re-create a new texture
				if (renderTexture != null && !renderTexture.isCompatible(width, height, bufferWidth, pixelFormat))
				{
					renderTexture.delete(re);
					renderTexture = null;
					if (copyOfRenderTexture != null)
					{
						copyOfRenderTexture.delete(re);
						copyOfRenderTexture = null;
					}
				}

				// Activate the rendering to a texture
				if (renderTexture == null)
				{
					renderTexture = new FBTexture(display.TopAddrFb, bufferWidth, width, height, getTexturePixelFormat(pixelFormat));
					renderTexture.bind(re, false);
					re.bindActiveTexture(ACTIVE_TEXTURE_FRAMEBUFFER, renderTexture.TextureId);
				}
				else
				{
					renderTexture.bind(re, false);
				}
			}

			base.startDisplay();

			// We don't use Client States
			base.disableClientState(IRenderingEngine_Fields.RE_TEXTURE);
			base.disableClientState(IRenderingEngine_Fields.RE_COLOR);
			base.disableClientState(IRenderingEngine_Fields.RE_NORMAL);
			base.disableClientState(IRenderingEngine_Fields.RE_VERTEX);

			// The value of the flags are lost when starting a new display
			setShaderFlag(IRenderingEngine_Fields.GU_CLIP_PLANES, 1);
		}

		public override void endDisplay()
		{
			if (useRenderToTexture)
			{
				// Copy the rendered texture back to the main frame buffer
				renderTexture.copyTextureToScreen(re);
			}

			re.useProgram(0);
			base.endDisplay();
		}

		public override void enableClientState(int type)
		{
			switch (type)
			{
				case IRenderingEngine_Fields.RE_VERTEX:
					re.enableVertexAttribArray(currentShaderProgram.ShaderAttribPosition);

					if (numberOfWeightsForShader > 0)
					{
						re.enableVertexAttribArray(currentShaderProgram.ShaderAttribWeights1);
						if (numberOfWeightsForShader > 4)
						{
							re.enableVertexAttribArray(currentShaderProgram.ShaderAttribWeights2);
						}
						else
						{
							re.disableVertexAttribArray(currentShaderProgram.ShaderAttribWeights2);
						}
					}
					else
					{
						re.disableVertexAttribArray(currentShaderProgram.ShaderAttribWeights1);
						re.disableVertexAttribArray(currentShaderProgram.ShaderAttribWeights2);
					}
					break;
				case IRenderingEngine_Fields.RE_NORMAL:
					re.enableVertexAttribArray(currentShaderProgram.ShaderAttribNormal);
					break;
				case IRenderingEngine_Fields.RE_COLOR:
					re.enableVertexAttribArray(currentShaderProgram.ShaderAttribColor);
					break;
				case IRenderingEngine_Fields.RE_TEXTURE:
					re.enableVertexAttribArray(currentShaderProgram.ShaderAttribTexture);
					break;
			}
		}

		public override void disableClientState(int type)
		{
			switch (type)
			{
				case IRenderingEngine_Fields.RE_VERTEX:
					re.disableVertexAttribArray(currentShaderProgram.ShaderAttribPosition);
					re.disableVertexAttribArray(currentShaderProgram.ShaderAttribWeights1);
					re.disableVertexAttribArray(currentShaderProgram.ShaderAttribWeights2);
					break;
				case IRenderingEngine_Fields.RE_NORMAL:
					re.disableVertexAttribArray(currentShaderProgram.ShaderAttribNormal);
					break;
				case IRenderingEngine_Fields.RE_COLOR:
					re.disableVertexAttribArray(currentShaderProgram.ShaderAttribColor);
					break;
				case IRenderingEngine_Fields.RE_TEXTURE:
					re.disableVertexAttribArray(currentShaderProgram.ShaderAttribTexture);
					break;
			}
		}

		public override void setWeightPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			if (size > 0)
			{
				re.setVertexAttribPointer(currentShaderProgram.ShaderAttribWeights1, System.Math.Min(size, 4), type, false, stride, bufferSize, buffer);
				if (size > 4)
				{
					re.setVertexAttribPointer(currentShaderProgram.ShaderAttribWeights2, size - 4, type, false, stride, bufferSize, buffer);
				}
			}
		}

		public override void setWeightPointer(int size, int type, int stride, long offset)
		{
			if (size > 0)
			{
				re.setVertexAttribPointer(currentShaderProgram.ShaderAttribWeights1, System.Math.Min(size, 4), type, false, stride, offset);
				if (size > 4)
				{
					re.setVertexAttribPointer(currentShaderProgram.ShaderAttribWeights2, size - 4, type, false, stride, offset + IRenderingEngine_Fields.sizeOfType[type] * 4);
				}
			}
		}

		public override bool canAllNativeVertexInfo()
		{
			// Shader supports all PSP native vertex formats
			return true;
		}

		public override bool canNativeSpritesPrimitive()
		{
			// Geometry shader supports native GU_SPRITES primitive
			return useGeometryShader;
		}

		public override void setVertexInfo(VertexInfo vinfo, bool allNativeVertexInfo, bool useVertexColor, bool useTexture, int type)
		{
			if (allNativeVertexInfo)
			{
				// Weight
				shaderContext.WeightScale = weightScale[vinfo.weight];

				// Texture
				shaderContext.VinfoTexture = useTexture ? (vinfo.texture != 0 ? vinfo.texture : 3) : 0;
				shaderContext.TextureScale = vinfo.transform2D ? 1.0f : textureScale[vinfo.texture];

				// Color
				shaderContext.VinfoColor = useVertexColor ? vinfo.color : 8;

				// Normal
				shaderContext.VinfoNormal = vinfo.normal;
				shaderContext.NormalScale = vinfo.transform2D ? 1.0f : normalScale[vinfo.normal];

				// Position
				shaderContext.VinfoPosition = vinfo.position;
				shaderContext.PositionScale = vinfo.transform2D ? 1.0f : positionScale[vinfo.position];
			}
			else
			{
				// Weight
				shaderContext.WeightScale = 1;

				// Texture
				shaderContext.VinfoTexture = useTexture ? 3 : 0;
				shaderContext.TextureScale = 1;

				// Color
				shaderContext.VinfoColor = useVertexColor ? 0 : 8;

				// Normal
				shaderContext.VinfoNormal = vinfo == null || vinfo.normal == 0 ? 0 : 3;
				shaderContext.NormalScale = 1;

				// Position
				shaderContext.VinfoPosition = vinfo != null && vinfo.position == 0 ? 0 : 3;
				shaderContext.PositionScale = 1;
			}
			shaderContext.VinfoTransform2D = vinfo == null || vinfo.transform2D ? 1 : 0;
			CurrentShaderProgram = type;

			base.setVertexInfo(vinfo, allNativeVertexInfo, useVertexColor, useTexture, type);
		}

		private int CurrentShaderProgram
		{
			set
			{
				ShaderProgram shaderProgram;
				bool hasGeometryShader = (value == IRenderingEngine_Fields.GU_SPRITES);
				if (useDynamicShaders)
				{
					shaderProgram = shaderProgramManager.getShaderProgram(shaderContext, hasGeometryShader);
					if (shaderProgram.ProgramId == -1)
					{
						shaderProgram = createShader(hasGeometryShader, shaderProgram);
						if (log.DebugEnabled)
						{
							log.debug("Created shader " + shaderProgram);
						}
						if (shaderProgram == null)
						{
							log.error("Cannot create shader");
							return;
						}
					}
				}
				else if (hasGeometryShader)
				{
					shaderProgram = defaultSpriteShaderProgram;
				}
				else
				{
					shaderProgram = defaultShaderProgram;
				}
				shaderProgram.use(re);
				if (log.TraceEnabled)
				{
					log.trace("Using shader " + shaderProgram);
				}
				currentShaderProgram = shaderProgram;
			}
		}

		/// <summary>
		/// Check if the fragment shader requires the frame buffer texture (fbTex sampler)
		/// to be updated with the current screen content.
		/// 
		/// A copy of the current screen content to the frame buffer texture
		/// can be avoided if this method returns "false". The execution of such a copy
		/// is quite expensive and should be avoided as much as possible.
		/// 
		/// We need to copy the current screen to the FrameBuffer texture only when one
		/// of the following applies:
		/// - the STENCIL_TEST flag is enabled
		/// - the BLEND_TEST flag is enabled
		/// - the Color mask is enabled
		/// </summary>
		/// <returns>  true  if the shader will use the fbTex sampler
		///          false if the shader will not use the fbTex sampler </returns>
		private bool FbTextureNeeded
		{
			get
			{
				if (useShaderDepthTest && shaderContext.DepthTestEnable != 0)
				{
					return true;
				}
    
				if (useShaderStencilTest && shaderContext.StencilTestEnable != 0)
				{
					return true;
				}
    
				if (useShaderBlendTest && shaderContext.BlendTestEnable != 0)
				{
					return true;
				}
    
				if (useShaderColorMask && shaderContext.ColorMaskEnable != 0)
				{
					return true;
				}
    
				return false;
			}
		}

		/// <summary>
		/// If necessary, load the frame buffer texture with the current screen
		/// content so that it can be used by the fragment shader (fbTex sampler).
		/// </summary>
		private void loadFbTexture()
		{
			if (!FbTextureNeeded)
			{
				return;
			}

			int width = viewportWidth;
			int height = viewportHeight;
			int bufferWidth = context.fbw;
			int pixelFormat = context.psm;

			if (useRenderToTexture)
			{
				// Use the render texture if it is compatible with the current GE settings.
				if (renderTexture.ResizedWidth >= width && renderTexture.ResizedHeight >= height && renderTexture.BufferWidth >= bufferWidth)
				{
					if (useTextureBarrier)
					{
						// The shader can use as input the texture used as output for the frame buffer.
						// For this feature, the texture barrier extension is required.
						renderTexture.bind(re, false);

						// Tell the shader which texture has to be used for the fbTex sampler.
						re.bindActiveTexture(ACTIVE_TEXTURE_FRAMEBUFFER, renderTexture.TextureId);

						re.textureBarrier();
					}
					else
					{
						// The shader cannot use as input the texture used as output for the frame buffer,
						// we need to copy the output texture to another texture and use the copy
						// as input for the shader.
						if (copyOfRenderTexture == null)
						{
							copyOfRenderTexture = new FBTexture(renderTexture);
						}
						copyOfRenderTexture.blitFrom(re, renderTexture);

						// Tell the shader which texture has to be used for the fbTex sampler.
						re.bindActiveTexture(ACTIVE_TEXTURE_FRAMEBUFFER, copyOfRenderTexture.TextureId);
					}
					return;
				}
				// If the render texture is not compatible with the current GE settings,
				// we are not lucky and have to copy the current screen to a compatible
				// texture.
			}

			// Delete the texture and recreate a new one if its dimension has changed
			if (fbTexture != null && !fbTexture.isCompatible(width, height, bufferWidth, pixelFormat))
			{
				fbTexture.delete(re);
				fbTexture = null;
			}

			// Create a new texture
			if (fbTexture == null)
			{
				fbTexture = new GETexture(Modules.sceDisplayModule.TopAddrGe, bufferWidth, width, height, pixelFormat, true);
			}

			// Copy the current screen (FrameBuffer) content to the texture
			re.ActiveTexture = ACTIVE_TEXTURE_FRAMEBUFFER;
			fbTexture.copyScreenToTexture(re);
			re.ActiveTexture = ACTIVE_TEXTURE_NORMAL;
		}

		public override void setVertexPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			re.setVertexAttribPointer(currentShaderProgram.ShaderAttribPosition, size, type, false, stride, bufferSize, buffer);
		}

		public override void setVertexPointer(int size, int type, int stride, long offset)
		{
			re.setVertexAttribPointer(currentShaderProgram.ShaderAttribPosition, size, type, false, stride, offset);
		}

		public override void setColorPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			re.setVertexAttribPointer(currentShaderProgram.ShaderAttribColor, size, type, false, stride, bufferSize, buffer);
		}

		public override void setColorPointer(int size, int type, int stride, long offset)
		{
			re.setVertexAttribPointer(currentShaderProgram.ShaderAttribColor, size, type, false, stride, offset);
		}

		public override void setNormalPointer(int type, int stride, int bufferSize, Buffer buffer)
		{
			re.setVertexAttribPointer(currentShaderProgram.ShaderAttribNormal, 3, type, false, stride, bufferSize, buffer);
		}

		public override void setNormalPointer(int type, int stride, long offset)
		{
			re.setVertexAttribPointer(currentShaderProgram.ShaderAttribNormal, 3, type, false, stride, offset);
		}

		public override void setTexCoordPointer(int size, int type, int stride, int bufferSize, Buffer buffer)
		{
			re.setVertexAttribPointer(currentShaderProgram.ShaderAttribTexture, size, type, false, stride, bufferSize, buffer);
		}

		public override void setTexCoordPointer(int size, int type, int stride, long offset)
		{
			re.setVertexAttribPointer(currentShaderProgram.ShaderAttribTexture, size, type, false, stride, offset);
		}

		private int prepareDraw(int primitive, bool burstMode)
		{
			if (primitive == IRenderingEngine_Fields.GU_SPRITES)
			{
				primitive = spriteGeometryShaderInputType;
			}
			if (!burstMode)
			{
				// The uniform values are specific to a shader program:
				// update the uniform values after switching the active shader program.
				shaderContext.setUniforms(re, currentShaderProgram.ProgramId);
			}
			loadFbTexture();
			loadIntegerTexture();

			return primitive;
		}

		public override void drawArrays(int primitive, int first, int count)
		{
			primitive = prepareDraw(primitive, false);
			base.drawArrays(primitive, first, count);
		}

		public override void drawArraysBurstMode(int primitive, int first, int count)
		{
			// drawArraysBurstMode is equivalent to drawArrays
			// but without the need to set the uniforms (they are unchanged
			// since the last call to drawArrays).
			primitive = prepareDraw(primitive, true);
			base.drawArraysBurstMode(primitive, first, count);
		}

		public override void drawElements(int primitive, int count, int indexType, Buffer indices, int indicesOffset)
		{
			primitive = prepareDraw(primitive, false);
			base.drawElements(primitive, count, indexType, indices, indicesOffset);
		}

		public override void drawElements(int primitive, int count, int indexType, long indicesOffset)
		{
			primitive = prepareDraw(primitive, false);
			base.drawElements(primitive, count, indexType, indicesOffset);
		}

		public override void drawElementsBurstMode(int primitive, int count, int indexType, long indicesOffset)
		{
			// drawElementsBurstMode is equivalent to drawElements
			// but without the need to set the uniforms (they are unchanged
			// since the last call to drawElements).
			primitive = prepareDraw(primitive, true);
			base.drawElementsBurstMode(primitive, count, indexType, indicesOffset);
		}

		public override bool canNativeClut(int textureAddress, int pixelFormat, bool textureSwizzle)
		{
			// The clut processing is implemented into the fragment shader
			// and the clut values are passed as a sampler2D.
			// Do not process clut's for swizzled texture, there is no performance gain.
			return useNativeClut && !textureSwizzle && base.canNativeClut(textureAddress, pixelFormat, textureSwizzle);
		}

		private int getClutIndexHint(int pixelFormat, int numEntries)
		{
			if (context.tex_clut_start == 0 && context.tex_clut_mask == numEntries - 1)
			{
				int currentBit = 0;
				if (context.tex_clut_shift == currentBit)
				{
					return IRenderingEngine_Fields.RE_CLUT_INDEX_RED_ONLY;
				}

				switch (pixelFormat)
				{
					case IRenderingEngine_Fields.RE_PIXEL_STORAGE_16BIT_INDEXED_BGR5650:
						currentBit += 5;
						break;
					case IRenderingEngine_Fields.RE_PIXEL_STORAGE_16BIT_INDEXED_ABGR5551:
						currentBit += 5;
						break;
					case IRenderingEngine_Fields.RE_PIXEL_STORAGE_16BIT_INDEXED_ABGR4444:
						currentBit += 4;
						break;
					case IRenderingEngine_Fields.RE_PIXEL_STORAGE_32BIT_INDEXED_ABGR8888:
						currentBit += 8;
						break;
					default:
						switch (context.tex_clut_mode)
						{
							case GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650:
								currentBit += 5;
								break;
							case GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR5551:
								currentBit += 5;
								break;
							case GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR4444:
								currentBit += 4;
								break;
							case GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888:
								currentBit += 8;
								break;
						}
				}
				if (context.tex_clut_shift == currentBit)
				{
					return IRenderingEngine_Fields.RE_CLUT_INDEX_GREEN_ONLY;
				}

				switch (pixelFormat)
				{
					case IRenderingEngine_Fields.RE_PIXEL_STORAGE_16BIT_INDEXED_BGR5650:
						currentBit += 6;
						break;
					case IRenderingEngine_Fields.RE_PIXEL_STORAGE_16BIT_INDEXED_ABGR5551:
						currentBit += 5;
						break;
					case IRenderingEngine_Fields.RE_PIXEL_STORAGE_16BIT_INDEXED_ABGR4444:
						currentBit += 4;
						break;
					case IRenderingEngine_Fields.RE_PIXEL_STORAGE_32BIT_INDEXED_ABGR8888:
						currentBit += 8;
						break;
					default:
						switch (context.tex_clut_mode)
						{
							case GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650:
								currentBit += 6;
								break;
							case GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR5551:
								currentBit += 5;
								break;
							case GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR4444:
								currentBit += 4;
								break;
							case GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888:
								currentBit += 8;
								break;
						}
				}
				if (context.tex_clut_shift == currentBit)
				{
					return IRenderingEngine_Fields.RE_CLUT_INDEX_BLUE_ONLY;
				}

				switch (pixelFormat)
				{
					case IRenderingEngine_Fields.RE_PIXEL_STORAGE_16BIT_INDEXED_BGR5650:
						currentBit += 5;
						break;
					case IRenderingEngine_Fields.RE_PIXEL_STORAGE_16BIT_INDEXED_ABGR5551:
						currentBit += 5;
						break;
					case IRenderingEngine_Fields.RE_PIXEL_STORAGE_16BIT_INDEXED_ABGR4444:
						currentBit += 4;
						break;
					case IRenderingEngine_Fields.RE_PIXEL_STORAGE_32BIT_INDEXED_ABGR8888:
						currentBit += 8;
						break;
					default:
						switch (context.tex_clut_mode)
						{
							case GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_BGR5650:
								currentBit += 5;
								break;
							case GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR5551:
								currentBit += 5;
								break;
							case GeCommands.TPSM_PIXEL_STORAGE_MODE_16BIT_ABGR4444:
								currentBit += 4;
								break;
							case GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888:
								currentBit += 8;
								break;
						}
				}
				if (context.tex_clut_shift == currentBit)
				{
					return IRenderingEngine_Fields.RE_CLUT_INDEX_ALPHA_ONLY;
				}
			}

			return IRenderingEngine_Fields.RE_CLUT_INDEX_NO_HINT;
		}

		private void loadIntegerTexture()
		{
			if (!context.textureFlag.Enabled || context.clearMode)
			{
				// Not using a texture
				return;
			}
			if (!useNativeClut || !IRenderingEngine_Fields.isTextureTypeIndexed[pixelFormat])
			{
				// Not using a native clut
				return;
			}
			if (pixelFormat == TPSM_PIXEL_STORAGE_MODE_4BIT_INDEXED)
			{
				// 4bit index has been decoded in the VideoEngine
				return;
			}

			// Associate the current texture to the integer texture sampler.
			// AMD/ATI driver requires different texture units for samplers of different types.
			// Otherwise, the shader compilation fails with the following error:
			// "Different sampler types for same sample texture unit in fragment shader"
			re.bindActiveTexture(ACTIVE_TEXTURE_INTEGER, context.currentTextureId);
		}

		private void loadClut(int pixelFormat)
		{
			int numEntries = VideoEngine.Instance.ClutNumEntries;
			int clutPixelFormat = context.tex_clut_mode;
			int bytesPerEntry = IRenderingEngine_Fields.sizeOfTextureType[clutPixelFormat];

			shaderContext.ClutShift = context.tex_clut_shift;
			shaderContext.ClutMask = context.tex_clut_mask;
			shaderContext.ClutOffset = context.tex_clut_start << 4;
			shaderContext.MipmapShareClut = context.mipmapShareClut;
			shaderContext.ClutIndexHint = getClutIndexHint(pixelFormat, numEntries);

			int[] clut32 = (bytesPerEntry == 4 ? VideoEngine.Instance.readClut32(0) : null);
			short[] clut16 = (bytesPerEntry == 2 ? VideoEngine.Instance.readClut16(0) : null);

			Texture texture;
			int textureId;
			if (VideoEngine.useTextureCache)
			{
				TextureCache textureCache = TextureCache.Instance;

				textureCacheLookupStatistics.start();
				texture = textureCache.getTexture(context.tex_clut_addr, numEntries, numEntries, 1, clutPixelFormat, 0, 0, 0, 0, 0, 0, 0, false, clut16, clut32);
				textureCacheLookupStatistics.end();

				if (texture == null)
				{
					texture = new Texture(textureCache, context.tex_clut_addr, numEntries, numEntries, 1, clutPixelFormat, 0, 0, 0, 0, 0, 0, 0, false, clut16, clut32);
					textureCache.addTexture(re, texture);
				}

				textureId = texture.getTextureId(re);
			}
			else
			{
				texture = null;
				if (clutTextureId == -1)
				{
					clutTextureId = re.genTexture();
				}

				textureId = clutTextureId;
			}

			if (texture == null || !texture.Loaded)
			{
				re.ActiveTexture = ACTIVE_TEXTURE_CLUT;
				re.bindTexture(textureId);

				clutBuffer.clear();
				if (clut32 != null)
				{
					clutBuffer.asIntBuffer().put(clut32, 0, numEntries);
				}
				else
				{
					clutBuffer.asShortBuffer().put(clut16, 0, numEntries);
				}

				re.setPixelStore(numEntries, bytesPerEntry);
				re.TextureMipmapMagFilter = GeCommands.TFLT_NEAREST;
				re.TextureMipmapMinFilter = GeCommands.TFLT_NEAREST;
				re.TextureMipmapMinLevel = 0;
				re.TextureMipmapMaxLevel = 0;
				re.setTextureWrapMode(GeCommands.TWRAP_WRAP_MODE_CLAMP, GeCommands.TWRAP_WRAP_MODE_CLAMP);

				// Load the CLUT as a Nx1 texture.
				// (gid15) I did not manage to make this code work with 1D textures,
				// probably because they are very seldom used and buggy.
				// To use a 2D texture Nx1 is the safest way...
				int clutSize = bytesPerEntry * numEntries;
				re.setTexImage(0, clutPixelFormat, numEntries, 1, clutPixelFormat, clutPixelFormat, clutSize, clutBuffer);

				if (texture != null)
				{
					texture.setIsLoaded();
				}

				re.ActiveTexture = ACTIVE_TEXTURE_NORMAL;
			}
			else
			{
				// The call
				//     bindActiveTexture(ACTIVE_TEXTURE_CLUT, textureId)
				// is equivalent to
				//     setActiveTexture(ACTIVE_TEXTURE_CLUT)
				//     bindTexture(textureId)
				//     setActiveTexture(ACTIVE_TEXTURE_NORMAL)
				// but executes faster: StateProxy can eliminate the 3 OpenGL calls
				// if they are redundant.
				re.bindActiveTexture(ACTIVE_TEXTURE_CLUT, textureId);
			}
		}

		public override void setTextureFormat(int pixelFormat, bool swizzle)
		{
			this.pixelFormat = pixelFormat;
			if (IRenderingEngine_Fields.isTextureTypeIndexed[pixelFormat])
			{
				if (pixelFormat == GeCommands.TPSM_PIXEL_STORAGE_MODE_4BIT_INDEXED)
				{
					// 4bit index has been decoded in the VideoEngine
					pixelFormat = context.tex_clut_mode;
				}
				else if (!canNativeClut(context.texture_base_pointer[0], pixelFormat, swizzle))
				{
					// Textures are decoded in the VideoEngine when not using native CLUTs
					pixelFormat = context.tex_clut_mode;
				}
				else
				{
					loadClut(pixelFormat);
				}
			}
			shaderContext.TexPixelFormat = pixelFormat;
			base.setTextureFormat(pixelFormat, swizzle);
		}

		public override float[] VertexColor
		{
			set
			{
				shaderContext.VertexColor = value;
				base.VertexColor = value;
			}
		}

		public override int DepthFunc
		{
			set
			{
				if (useShaderDepthTest)
				{
					shaderContext.DepthFunc = value;
				}
				base.DepthFunc = value;
			}
		}

		public override bool DepthMask
		{
			set
			{
				if (useShaderDepthTest)
				{
					shaderContext.DepthMask = value ? 0xFF : 0x00;
				}
				base.DepthMask = value;
			}
		}

		public override void setStencilFunc(int func, int @ref, int mask)
		{
			if (useShaderStencilTest)
			{
				shaderContext.StencilFunc = func;
				// Pre-mask the reference value with the mask value
				shaderContext.StencilRef = @ref & mask;
				shaderContext.StencilMask = mask;
			}
			base.setStencilFunc(func, @ref, mask);
		}

		public override void setStencilOp(int fail, int zfail, int zpass)
		{
			if (useShaderStencilTest)
			{
				shaderContext.StencilOpFail = fail;
				shaderContext.StencilOpZFail = zfail;
				shaderContext.StencilOpZPass = zpass;
			}
			base.setStencilOp(fail, zfail, zpass);
		}

		public override void setColorMask(int redMask, int greenMask, int blueMask, int alphaMask)
		{
			if (useShaderColorMask)
			{
				shaderContext.setColorMask(redMask, greenMask, blueMask, alphaMask);
				// The pre-computed not-values for the color masks
				shaderContext.setNotColorMask((~redMask) & 0xFF, (~greenMask) & 0xFF, (~blueMask) & 0xFF, (~alphaMask) & 0xFF);
				// The color mask is enabled when at least one color mask is non-zero
				shaderContext.ColorMaskEnable = redMask != 0x00 || greenMask != 0x00 || blueMask != 0x00 || alphaMask != 0x00 ? 1 : 0;

				// Do not call the "super" method in BaseRenderingEngineFunction
				proxy.setColorMask(redMask, greenMask, blueMask, alphaMask);
				// Set the on/off color masks
				re.setColorMask(redMask != 0xFF, greenMask != 0xFF, blueMask != 0xFF, stencilTestFlag);
			}
			else
			{
				base.setColorMask(redMask, greenMask, blueMask, alphaMask);
			}
		}

		public override void setAlphaFunc(int func, int @ref, int mask)
		{
			if (useShaderAlphaTest)
			{
				shaderContext.AlphaTestFunc = func;
				shaderContext.AlphaTestRef = @ref & mask;
				shaderContext.AlphaTestMask = mask;
			}
			base.setAlphaFunc(func, @ref, mask);
		}

		public override int BlendEquation
		{
			set
			{
				if (useShaderBlendTest)
				{
					shaderContext.BlendEquation = value;
				}
				base.BlendEquation = value;
			}
		}

		public override void setBlendFunc(int src, int dst)
		{
			if (useShaderBlendTest)
			{
				shaderContext.BlendSrc = src;
				shaderContext.BlendDst = dst;
				// Do not call the "super" method in BaseRenderingEngineFunction
				proxy.setBlendFunc(src, dst);
			}
			else
			{
				base.setBlendFunc(src, dst);
			}
		}

		public override void setBlendSFix(int sfix, float[] color)
		{
			if (useShaderBlendTest)
			{
				shaderContext.BlendSFix = color;
				// Do not call the "super" method in BaseRenderingEngineFunction
				proxy.setBlendSFix(sfix, color);
			}
			else
			{
				base.setBlendSFix(sfix, color);
			}
		}

		public override void setBlendDFix(int dfix, float[] color)
		{
			if (useShaderBlendTest)
			{
				shaderContext.BlendDFix = color;
				// Do not call the "super" method in BaseRenderingEngineFunction
				proxy.setBlendDFix(dfix, color);
			}
			else
			{
				base.setBlendDFix(dfix, color);
			}
		}

		public override void setViewport(int x, int y, int width, int height)
		{
			// Remember the viewport size for later use in loadFbTexture().
			viewportWidth = width;
			viewportHeight = height;
			base.setViewport(x, y, width, height);
		}

		public override bool setCopyRedToAlpha(bool copyRedToAlpha)
		{
			shaderContext.CopyRedToAlpha = copyRedToAlpha ? 1 : 0;
			return base.setCopyRedToAlpha(copyRedToAlpha);
		}

		public override void setTextureWrapMode(int s, int t)
		{
			shaderContext.WrapModeS = s;
			shaderContext.WrapModeT = t;
			base.setTextureWrapMode(s, t);
		}

		public override float[] FogColor
		{
			set
			{
				shaderContext.FogColor = value;
				base.FogColor = value;
			}
		}

		public override void setFogDist(float end, float scale)
		{
			shaderContext.FogEnd = end;
			shaderContext.FogScale = scale;
			base.setFogDist(end, scale);
		}

		public override bool canDiscardVertices()
		{
			// Functionality to discard vertices has been implemented in the shaders
			return true;
		}

		public override void setViewportPos(float x, float y, float z)
		{
			shaderContext.setViewportPos(x, y, z);
			base.setViewportPos(x, y, z);
		}

		public override void setViewportScale(float sx, float sy, float sz)
		{
			shaderContext.setViewportScale(sx, sy, sz);
			base.setViewportScale(sx, sy, sz);
		}
	}
}