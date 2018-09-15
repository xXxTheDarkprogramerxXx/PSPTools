using System;
using System.Collections.Generic;
using System.Threading;

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
namespace pspsharp.HLE.modules
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_INVALID_FORMAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_INVALID_MODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_INVALID_POINTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.kernel.types.SceKernelErrors.ERROR_INVALID_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.MemoryMap.START_VRAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TFLT_NEAREST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.GeCommands.TWRAP_WRAP_MODE_CLAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.graphics.VideoEngine.SIZEOF_FLOAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.makePow2;


	using IntrManager = pspsharp.HLE.kernel.managers.IntrManager;
	using PspGeList = pspsharp.HLE.kernel.types.PspGeList;
	using SceKernelErrors = pspsharp.HLE.kernel.types.SceKernelErrors;
	using IAction = pspsharp.HLE.kernel.types.IAction;
	using IWaitStateChecker = pspsharp.HLE.kernel.types.IWaitStateChecker;
	using SceKernelThreadInfo = pspsharp.HLE.kernel.types.SceKernelThreadInfo;
	using ThreadWaitInfo = pspsharp.HLE.kernel.types.ThreadWaitInfo;
	using GEProfiler = pspsharp.graphics.GEProfiler;
	using GeCommands = pspsharp.graphics.GeCommands;
	using VertexCache = pspsharp.graphics.VertexCache;
	using VideoEngine = pspsharp.graphics.VideoEngine;
	using IRenderingEngine = pspsharp.graphics.RE.IRenderingEngine;
	using RenderingEngineFactory = pspsharp.graphics.RE.RenderingEngineFactory;
	using RenderingEngineLwjgl = pspsharp.graphics.RE.RenderingEngineLwjgl;
	using IREBufferManager = pspsharp.graphics.RE.buffer.IREBufferManager;
	using ExternalGE = pspsharp.graphics.RE.externalge.ExternalGE;
	using CaptureManager = pspsharp.graphics.capture.CaptureManager;
	using GETexture = pspsharp.graphics.textures.GETexture;
	using GETextureManager = pspsharp.graphics.textures.GETextureManager;
	using TextureCache = pspsharp.graphics.textures.TextureCache;
	using Screen = pspsharp.hardware.Screen;
	using IMemoryReaderWriter = pspsharp.memory.IMemoryReaderWriter;
	using MemoryReaderWriter = pspsharp.memory.MemoryReaderWriter;
	using UnblockThreadAction = pspsharp.scheduler.UnblockThreadAction;
	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using AbstractStringSettingsListener = pspsharp.settings.AbstractStringSettingsListener;
	using ISettingsListener = pspsharp.settings.ISettingsListener;
	using Settings = pspsharp.settings.Settings;
	using DurationStatistics = pspsharp.util.DurationStatistics;
	using Utilities = pspsharp.util.Utilities;

	//using Logger = org.apache.log4j.Logger;
	using LWJGLException = org.lwjgl.LWJGLException;
	using AWTGLCanvas = org.lwjgl.opengl.AWTGLCanvas;
	using ContextAttribs = org.lwjgl.opengl.ContextAttribs;
	using Display = org.lwjgl.opengl.Display;
	using DisplayMode = org.lwjgl.opengl.DisplayMode;
	using PixelFormat = org.lwjgl.opengl.PixelFormat;

	public class sceDisplay : HLEModule
	{
		//public static Logger log = Modules.getLogger("sceDisplay");

		internal class AWTGLCanvas_sceDisplay : AWTGLCanvas
		{
			private readonly sceDisplay outerInstance;

			internal const long serialVersionUID = -3808789665048696700L;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public AWTGLCanvas_sceDisplay() throws org.lwjgl.LWJGLException
			public AWTGLCanvas_sceDisplay(sceDisplay outerInstance) : base(null, (new PixelFormat()).withBitsPerPixel(8).withAlphaBits(8).withStencilBits(8).withSamples(outerInstance.antiAliasSamplesNum), null, (new ContextAttribs()).withDebug(useDebugGL))
			{
				this.outerInstance = outerInstance;
			}

			protected internal override void paintGL()
			{
				VideoEngine videoEngine = VideoEngine.Instance;

				if (log.TraceEnabled)
				{
					log.trace(string.Format("paintGL resize={0:F}, size({1:D}x{2:D}), canvas({3:D}x{4:D}), location({5:D},{6:D})", viewportResizeFilterScaleFactor, outerInstance.canvas.Size.width, outerInstance.canvas.Size.height, outerInstance.canvasWidth, outerInstance.canvasHeight, outerInstance.canvas.Location.x, outerInstance.canvas.Location.y));
				}

				if (outerInstance.resizePending && Emulator.MainGUI.Visible)
				{
					// Resize the MainGUI to use the preferred size of this sceDisplay
					Emulator.MainGUI.pack();
					outerInstance.resizePending = false;
				}

				if (outerInstance.statistics != null)
				{
					outerInstance.statistics.start();
				}

				if (outerInstance.resetDisplaySettings)
				{
					// Some display settings have been updated,
					// a new rendering engine has to be created.
					if (outerInstance.isStarted)
					{
						videoEngine.stop();
					}
					TextureCache.Instance.reset(outerInstance.re);
					VertexCache.Instance.reset(outerInstance.re);
					outerInstance.startModules = true;
					outerInstance.re = null;
					outerInstance.reDisplay = null;
					outerInstance.resetDisplaySettings = false;

					outerInstance.saveGEToTexture = Settings.Instance.readBool("emu.enablegetexture");
					if (outerInstance.saveGEToTexture)
					{
						log.info("Saving GE to Textures");
					}
				}

				if (outerInstance.re == null)
				{
					if (outerInstance.startModules)
					{
						ExternalGE.init();
						outerInstance.re = RenderingEngineFactory.createRenderingEngine();
						if (outerInstance.UsingSoftwareRenderer)
						{
							outerInstance.reDisplay = RenderingEngineFactory.createRenderingEngineForDisplay();
							outerInstance.reDisplay.GeContext = videoEngine.Context;
						}
						else
						{
							outerInstance.reDisplay = outerInstance.re;
						}
					}
					else
					{
						outerInstance.re = RenderingEngineFactory.createInitialRenderingEngine();
						outerInstance.reDisplay = outerInstance.re;
					}
				}

				if (outerInstance.startModules)
				{
					outerInstance.saveGEToTexture = Settings.Instance.readBool("emu.enablegetexture");
					if (outerInstance.saveGEToTexture)
					{
						GETextureManager.Instance.reset(outerInstance.reDisplay);
					}
					videoEngine.start();
					outerInstance.drawBuffer = outerInstance.reDisplay.BufferManager.genBuffer(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_ARRAY_BUFFER, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FLOAT, 16, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DYNAMIC_DRAW);
					outerInstance.drawBufferArray = new float[16];
					outerInstance.startModules = false;
					if (outerInstance.saveGEToTexture && !outerInstance.re.FramebufferObjectAvailable)
					{
						outerInstance.saveGEToTexture = false;
						Console.WriteLine("Saving GE to Textures has been automatically disabled: FBO is not supported by this OpenGL version");
					}
					outerInstance.isStarted = true;
				}

				if (!outerInstance.isStarted)
				{
					outerInstance.reDisplay.clear(0.0f, 0.0f, 0.0f, 0.0f);
					return;
				}

				if (outerInstance.createTex)
				{
					// Create two textures: one at original PSP size and
					// one resized to the display size
					outerInstance.texFb = outerInstance.createTexture(outerInstance.texFb, false);
					outerInstance.resizedTexFb = outerInstance.createTexture(outerInstance.resizedTexFb, true);

					outerInstance.checkTemp();
					outerInstance.createTex = false;
				}

				if (outerInstance.resetGeTextures)
				{
					if (outerInstance.saveGEToTexture)
					{
						GETextureManager.Instance.reset(outerInstance.reDisplay);
					}
					outerInstance.resetGeTextures = false;
				}

				// If we are not rendering this frame, skip the next sceDisplaySetFrameBuf call,
				// assuming the application is doing double buffering.
				outerInstance.skipNextFrameBufferSwitch = videoEngine.SkipThisFrame;

				bool doSwapBuffers = true;

				// Copy the current frame buffer object, in case it is modified by currentFb while rendering.
				FrameBufferSettings currentFb = outerInstance.fb;

				outerInstance.InsideRendering = true;

				if (ExternalGE.Active)
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("sceDisplay.paintGL - ExternalGE - rendering the FB 0x{0:X8}", currentFb.TopAddr));
					}

					outerInstance.reDisplay.startDisplay();
					if (ExternalGE.ScreenScale <= 1)
					{
						outerInstance.drawFrameBufferFromMemory(currentFb);
					}
					else
					{
						ByteBuffer scaledScreen = ExternalGE.getScaledScreen(outerInstance.fb.TopAddr, outerInstance.fb.BufferWidth, outerInstance.fb.Height, outerInstance.fb.PixelFormat);
						if (scaledScreen == null)
						{
							outerInstance.drawFrameBufferFromMemory(currentFb);
						}
						else
						{
							int screenScale = ExternalGE.ScreenScale;
							outerInstance.fb.Pixels.clear();
							outerInstance.reDisplay.bindTexture(outerInstance.resizedTexFb);
							outerInstance.reDisplay.setTextureFormat(outerInstance.fb.PixelFormat, false);
							outerInstance.reDisplay.setPixelStore(outerInstance.fb.BufferWidth * screenScale, getPixelFormatBytes(outerInstance.fb.PixelFormat));
							outerInstance.reDisplay.setTexSubImage(0, 0, 0, outerInstance.fb.BufferWidth * screenScale, outerInstance.fb.Height * screenScale, outerInstance.fb.PixelFormat, outerInstance.fb.PixelFormat, scaledScreen.remaining(), scaledScreen);

							outerInstance.drawFrameBuffer(outerInstance.fb, false, true, outerInstance.fb.BufferWidth, outerInstance.fb.PixelFormat, outerInstance.displayScreen.getWidth(outerInstance.fb), outerInstance.displayScreen.getHeight(outerInstance.fb));
						}
					}
					outerInstance.reDisplay.endDisplay();

					//if (log.DebugEnabled)
					{
						Console.WriteLine("sceDisplay.paintGL - ExternalGE - end display");
					}
				}
				else if (outerInstance.UsingSoftwareRenderer)
				{
					// Software rendering: the processing of the GE list is done by the
					// SoftwareRenderingDisplayThread.
					// We just need to display the frame buffer.
					if (outerInstance.softwareRenderingDisplayThread == null)
					{
						outerInstance.re.startDisplay();
						videoEngine.update();
						outerInstance.re.endDisplay();
					}

					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("sceDisplay.paintGL - software - rendering the FB 0x{0:X8}", currentFb.TopAddr));
					}

					outerInstance.reDisplay.startDisplay();
					outerInstance.drawFrameBufferFromMemory(currentFb);
					outerInstance.reDisplay.endDisplay();

					//if (log.DebugEnabled)
					{
						Console.WriteLine("sceDisplay.paintGL - software - end display");
					}
				}
				else if (outerInstance.OnlyGEGraphics)
				{
					// Hardware rendering where only the currently rendered GE list is displayed,
					// not the frame buffer from memory.
					//if (log.DebugEnabled)
					{
						Console.WriteLine("sceDisplay.paintGL - start display - only GE");
					}
					outerInstance.re.startDisplay();

					// Display this screen (i.e. swap buffers) only if something has been rendered
					doSwapBuffers = videoEngine.update();

					outerInstance.re.endDisplay();
					//if (log.DebugEnabled)
					{
						Console.WriteLine("sceDisplay.paintGL - end display - only GE");
					}
				}
				else
				{
					// Hardware rendering:
					// 1) GE list is rendered to the screen
					// 2) the result of the rendering is stored into the GE frame buffer
					// 3) the active FB frame buffer is reloaded from memory to the screen for final display
					//if (log.DebugEnabled)
					{
						Console.WriteLine("sceDisplay.paintGL - start display");
					}
					outerInstance.re.startDisplay();

					// The GE will be reloaded to the screen by the VideoEngine
					if (videoEngine.update())
					{
						// Save the GE only if it actually drew something
						//if (log.DebugEnabled)
						{
							Console.WriteLine(string.Format("sceDisplay.paintGL - saving the GE to memory 0x{0:X8}", outerInstance.ge.TopAddr));
						}

						if (outerInstance.saveGEToTexture && !videoEngine.isVideoTexture(outerInstance.ge.TopAddr))
						{
							GETexture geTexture = GETextureManager.Instance.getGETexture(outerInstance.reDisplay, outerInstance.ge.TopAddr, outerInstance.ge.BufferWidth, outerInstance.ge.Width, outerInstance.ge.Height, outerInstance.ge.PixelFormat, true);
							geTexture.copyScreenToTexture(outerInstance.re);
						}
						else
						{
							// Set texFb as the current texture
							outerInstance.reDisplay.bindTexture(outerInstance.resizedTexFb);
							outerInstance.reDisplay.setTextureFormat(outerInstance.ge.PixelFormat, false);

							// Copy screen to the current texture
							outerInstance.reDisplay.copyTexSubImage(0, 0, 0, 0, 0, getResizedWidth(outerInstance.ge.Width), getResizedHeight(outerInstance.ge.Height));

							// Re-render GE/current texture upside down
							outerInstance.drawFrameBuffer(currentFb, true, true, currentFb.BufferWidth, currentFb.PixelFormat, currentFb.Width, currentFb.Height);

							// Save GE/current texture to vram
							outerInstance.copyScreenToPixels(outerInstance.ge.Pixels, outerInstance.ge.BufferWidth, outerInstance.ge.PixelFormat, outerInstance.ge.Width, outerInstance.ge.Height);
						}
					}

					// Render the FB
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("sceDisplay.paintGL - rendering the FB 0x{0:X8}", currentFb.TopAddr));
					}
					if (outerInstance.saveGEToTexture && !videoEngine.isVideoTexture(outerInstance.fb.TopAddr))
					{
						GETexture geTexture = GETextureManager.Instance.getGETexture(outerInstance.reDisplay, currentFb.TopAddr, currentFb.BufferWidth, currentFb.Width, currentFb.Height, currentFb.PixelFormat, true);
						geTexture.copyTextureToScreen(outerInstance.reDisplay);
					}
					else
					{
						outerInstance.drawFrameBufferFromMemory(currentFb);
					}

					outerInstance.re.endDisplay();

					//if (log.DebugEnabled)
					{
						Console.WriteLine("sceDisplay.paintGL - end display");
					}
				}

				outerInstance.InsideRendering = false;

				// Perform OpenGL double buffering
				if (doSwapBuffers)
				{
					outerInstance.paintFrameCount++;
					try
					{
						outerInstance.canvas.swapBuffers();
					}
					catch (LWJGLException)
					{
					}
				}

				// Update the current FPS every second
				outerInstance.reportFPSStats();

				if (outerInstance.statistics != null)
				{
					outerInstance.statistics.end();
				}

				foreach (IAction action in outerInstance.displayActions)
				{
					action.execute();
				}

				foreach (IAction action in outerInstance.displayActionsOnce)
				{
					action.execute();
				}
				outerInstance.displayActionsOnce.Clear();
			}

			protected internal override void initGL()
			{
				SwapInterval = 0;
				base.initGL();

				// Collect debugging information...
				outerInstance.initGLcalled = true;
				outerInstance.openGLversion = RenderingEngineLwjgl.Version;
			}

			public override void setBounds(int x, int y, int width, int height)
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("setBounds width={0:D}, height={1:D}", width, height));
				}
				outerInstance.canvasWidth = width;
				outerInstance.canvasHeight = height;
				base.setBounds(x, y, width, height);
			}

			public override void componentResized(ComponentEvent e)
			{
				outerInstance.setViewportResizeScaleFactor(Width, Height);
			}
		}

		private class FrameBufferSettings
		{

			internal int topAddr;
			internal int bottomAddr;
			internal int bufferWidth;
			internal int width;
			internal int height;
			internal int pixelFormat;
			internal Buffer pixels;
			internal int size;

			public FrameBufferSettings(int topAddr, int bufferWidth, int width, int height, int pixelFormat)
			{
				this.topAddr = topAddr & Memory.addressMask;
				this.bufferWidth = bufferWidth;
				this.width = width;
				this.height = height;
				this.pixelFormat = pixelFormat;
				update();
			}

			public FrameBufferSettings(FrameBufferSettings copy)
			{
				topAddr = copy.topAddr;
				bottomAddr = copy.bottomAddr;
				bufferWidth = copy.bufferWidth;
				width = copy.width;
				height = copy.height;
				pixelFormat = copy.pixelFormat;
				pixels = copy.pixels;
				size = copy.size;
			}

			internal virtual void update()
			{
				size = bufferWidth * height * getPixelFormatBytes(pixelFormat);
				bottomAddr = topAddr + size;
				pixels = Memory.Instance.getBuffer(topAddr, size);
			}

			public virtual int TopAddr
			{
				get
				{
					return topAddr;
				}
			}

			public virtual int BottomAddr
			{
				get
				{
					return bottomAddr;
				}
			}

			public virtual int BufferWidth
			{
				get
				{
					return bufferWidth;
				}
			}

			public virtual int PixelFormat
			{
				get
				{
					return pixelFormat;
				}
			}

			public virtual Buffer Pixels
			{
				get
				{
					return pixels;
				}
			}

			public virtual Buffer getPixels(int topAddr)
			{
				if (this.topAddr == topAddr)
				{
					return pixels;
				}
				return Memory.Instance.getBuffer(topAddr, size);
			}

			public virtual int Width
			{
				get
				{
					return width;
				}
			}

			public virtual int Height
			{
				get
				{
					return height;
				}
			}

			public virtual int Size
			{
				get
				{
					return size;
				}
			}

			public virtual bool isRawAddressInside(int address)
			{
				// vram address is lower than main memory so check the end of the buffer first, it's more likely to fail
				return address >= topAddr && address < bottomAddr;
			}

			public virtual bool isAddressInside(int address)
			{
				return isRawAddressInside(address & Memory.addressMask);
			}

			public virtual void setDimension(int width, int height)
			{
				this.width = width;
				this.height = height;
				update();
			}

			public override string ToString()
			{
				return string.Format("0x{0:X8}-0x{1:X8}, {2:D}x{3:D}, bufferWidth={4:D}, pixelFormat={5:D}", topAddr, bottomAddr, width, height, bufferWidth, pixelFormat);
			}
		}
		protected internal AWTGLCanvas_sceDisplay canvas;

		public virtual AWTGLCanvas Canvas
		{
			get
			{
				return canvas;
			}
		}
		private bool onlyGEGraphics = false;
		private bool saveGEToTexture = false;
		private bool useSoftwareRenderer = false;
		private bool saveStencilToMemory = false;
		private const bool useDebugGL = false;
		private const int internalTextureFormat = GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
		private const string resizeScaleFactorSettings = "emu.graphics.resizeScaleFactor";
		// sceDisplayModes enum
		public const int PSP_DISPLAY_MODE_LCD = 0;
		public const int PSP_DISPLAY_MODE_VESA1A = 0x1A;
		public const int PSP_DISPLAY_MODE_PSEUDO_VGA = 0x60;
		// sceDisplaySetBufSync enum
		public const int PSP_DISPLAY_SETBUF_IMMEDIATE = 0;
		public const int PSP_DISPLAY_SETBUF_NEXTFRAME = 1;
		private const float hCountPerVblank = 285.72f;
		private const float FRAME_PER_SEC = 59.940060f;
		// current Rendering Engine
		private IRenderingEngine re;
		private IRenderingEngine reDisplay;
		private bool startModules;
		private bool isStarted;
		private int drawBuffer;
		private float[] drawBufferArray;
		private bool resetDisplaySettings;
		private bool resetGeTextures;

		// current display mode
		private int mode;
		// Resizing options
		private static float viewportResizeFilterScaleFactor = 1f;
		private static int viewportResizeFilterScaleFactorInt = 1;
		private bool resizePending;
		// current framebuffer and GE settings
		private FrameBufferSettings fb;
		private FrameBufferSettings ge;
		private int sync;
		private bool setGeBufCalledAtLeastOnce;
		public bool gotBadGeBufParams;
		public bool gotBadFbBufParams;
		protected internal bool isFbShowing;
		private DisplayScreen displayScreen;

		// additional variables
		private bool detailsDirty;
		private bool displayDirty;
		private bool geDirty;
		private long lastUpdate;
		private bool initGLcalled;
		private string openGLversion;
		private bool calledFromCommandLine;
		private volatile bool doneCopyGeToMemory;
		// Canvas fields
		private Buffer temp;
		private ByteBuffer tempByteBuffer;
		private int[] tempIntArray;
		private int tempSize;
		private int canvasWidth;
		private int canvasHeight;
		private bool createTex;
		private int texFb;
		private int resizedTexFb;
		private float texS;
		private float texT;
		private Robot captureRobot;
		// fps counter variables
		private long prevStatsTime;
		private long frameCount;
		private long paintFrameCount;
		private long prevFrameCount;
		private long prevPaintFrameCount;
		private long reportCount;
		private int vcount;
		private long lastVblankMicroTime;
		private DisplayVblankAction displayVblankAction;
		public DurationStatistics statistics;
		public DurationStatistics statisticsCopyGeToMemory;
		public DurationStatistics statisticsCopyMemoryToGe;
		// Async Display
		private AsyncDisplayThread asyncDisplayThread;
		private SoftwareRenderingDisplayThread softwareRenderingDisplayThread;
		private volatile bool insideRendering;
		// VBLANK Multi.
		private IList<WaitVblankInfo> waitingOnVblank;
		// Anti-alias samples.
		private int antiAliasSamplesNum;
		// Frame skipping
		private int desiredFps = 0;
		private int maxFramesSkippedInSequence = 3;
		private int framesSkippedInSequence;
		private LinkedList<long> frameTimestamps = new LinkedList<long>();
		private bool skipNextFrameBufferSwitch;
		// Stencil copy
		private static readonly int[] stencilPixelMasks = new int[]{0, 0x7FFF, 0x0FFF, 0x00FFFFFF};
		private static readonly int[] stencilValueMasks = new int[]{0, 0x80, 0xF0, 0xFF};
		private static readonly int[] stencilValueShifts = new int[]{0, 8, 8, 24};
		// Mpeg audio hack
		private int framePerSecFactor;
		// Display actions
		private IList<IAction> displayActions = new LinkedList<IAction>();
		private IList<IAction> displayActionsOnce = new LinkedList<IAction>();

		private class OnlyGeSettingsListener : AbstractBoolSettingsListener
		{
			private readonly sceDisplay outerInstance;

			public OnlyGeSettingsListener(sceDisplay outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			protected internal override void settingsValueChanged(bool value)
			{
				outerInstance.OnlyGEGraphics = value;
			}
		}

		private class SoftwareRendererSettingsListener : AbstractBoolSettingsListener
		{
			private readonly sceDisplay outerInstance;

			public SoftwareRendererSettingsListener(sceDisplay outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			protected internal override void settingsValueChanged(bool value)
			{
				outerInstance.UseSoftwareRenderer = value;
			}
		}

		private class SaveStencilToMemorySettingsListener : AbstractBoolSettingsListener
		{
			private readonly sceDisplay outerInstance;

			public SaveStencilToMemorySettingsListener(sceDisplay outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			protected internal override void settingsValueChanged(bool value)
			{
				outerInstance.SaveStencilToMemory = value;
			}
		}

		private class WaitVblankInfo
		{

			public int threadId;
			public int unblockVcount;

			public WaitVblankInfo(int threadId, int unblockVcount)
			{
				this.threadId = threadId;
				this.unblockVcount = unblockVcount;
			}
		}

		private abstract class AbstractDisplayThread : Thread
		{

			internal Semaphore displaySemaphore = new Semaphore(1);
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			protected internal bool run_Renamed = true;

			public AbstractDisplayThread()
			{
				// Force the creation of a VideoEngine instance in this thread
				VideoEngine.Instance;
			}

			public override void run()
			{
				while (run_Renamed)
				{
					waitForDisplay();
					if (run_Renamed)
					{
						doDisplay();
					}
				}
			}

			protected internal abstract void doDisplay();

			public virtual void display()
			{
				displaySemaphore.release();
			}

			internal virtual void waitForDisplay()
			{
				while (true)
				{
					try
					{
						int availablePermits = displaySemaphore.drainPermits();
						if (availablePermits > 0)
						{
							break;
						}

						if (displaySemaphore.tryAcquire(100, TimeUnit.MILLISECONDS))
						{
							break;
						}
					}
					catch (InterruptedException)
					{
					}
				}
			}

			public virtual void exit()
			{
				run_Renamed = false;
				display();
			}
		}

		private class AsyncDisplayThread : AbstractDisplayThread
		{

			protected internal override void doDisplay()
			{
				if (!Modules.sceDisplayModule.OnlyGEGraphics || VideoEngine.Instance.hasDrawLists())
				{
					Modules.sceDisplayModule.canvas.repaint();
				}
			}
		}

		private class SoftwareRenderingDisplayThread : AbstractDisplayThread
		{

			protected internal override void doDisplay()
			{
				if (VideoEngine.Instance.hasDrawLists())
				{
					IRenderingEngine re = Modules.sceDisplayModule.RenderingEngine;

					if (re == null && !Screen.hasScreen())
					{
						re = RenderingEngineFactory.createRenderingEngine();
						Modules.sceDisplayModule.RenderingEngine = re;
						VideoEngine.Instance.start();
					}

					if (re != null)
					{
						re.startDisplay();
						VideoEngine.Instance.update();
						re.endDisplay();
					}
				}
			}
		}

		private class DisplayVblankAction : IAction
		{
			private readonly sceDisplay outerInstance;

			public DisplayVblankAction(sceDisplay outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void execute()
			{
				outerInstance.hleVblankStart();
			}
		}

		private class VblankWaitStateChecker : IWaitStateChecker
		{
			private readonly sceDisplay outerInstance;


			internal int vcount;

			public VblankWaitStateChecker(sceDisplay outerInstance, int vcount)
			{
				this.outerInstance = outerInstance;
				this.vcount = vcount;
			}

			public virtual bool continueWaitState(SceKernelThreadInfo thread, ThreadWaitInfo wait)
			{
				// Continue the wait state until the vcount changes
				bool continueWait = outerInstance.vcount < vcount;

				if (!continueWait)
				{
					ExternalGE.onDisplayStopWaitVblank();
				}

				return continueWait;
			}
		}

		private class VblankUnblockThreadAction : UnblockThreadAction
		{
			private readonly sceDisplay outerInstance;

			public VblankUnblockThreadAction(sceDisplay outerInstance, int threadId) : base(threadId)
			{
				this.outerInstance = outerInstance;
			}

			public override void execute()
			{
				ExternalGE.onDisplayStopWaitVblank();
				base.execute();
			}
		}

		private class AntiAliasSettingsListerner : AbstractStringSettingsListener
		{
			private readonly sceDisplay outerInstance;

			public AntiAliasSettingsListerner(sceDisplay outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			internal Pattern pattern = Pattern.compile("x(\\d+)", Pattern.CASE_INSENSITIVE);

			protected internal override void settingsValueChanged(string value)
			{
				int samples = 0;
				if (!string.ReferenceEquals(value, null))
				{
					Matcher matcher = pattern.matcher(value);
					if (matcher.matches())
					{
						samples = int.Parse(matcher.group(1));
					}
				}
				outerInstance.AntiAliasSamplesNum = samples;
			}
		}

		private class DisplaySettingsListener : ISettingsListener
		{
			private readonly sceDisplay outerInstance;

			public DisplaySettingsListener(sceDisplay outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void settingsValueChanged(string option, string value)
			{
				if (outerInstance.isStarted)
				{
					outerInstance.resetDisplaySettings = true;
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public sceDisplay() throws org.lwjgl.LWJGLException
		public sceDisplay()
		{
			setSettingsListener("emu.graphics.antialias", new AntiAliasSettingsListerner(this));

			DisplaySettingsListener displaySettingsListener = new DisplaySettingsListener(this);
			setSettingsListener("emu.useVertexCache", displaySettingsListener);
			setSettingsListener("emu.useshaders", displaySettingsListener);
			setSettingsListener("emu.useGeometryShader", displaySettingsListener);
			setSettingsListener("emu.disableubo", displaySettingsListener);
			setSettingsListener("emu.enablevao", displaySettingsListener);
			setSettingsListener("emu.enablegetexture", displaySettingsListener);
			setSettingsListener("emu.enablenativeclut", displaySettingsListener);
			setSettingsListener("emu.enabledynamicshaders", displaySettingsListener);
			setSettingsListener("emu.enableshaderstenciltest", displaySettingsListener);
			setSettingsListener("emu.enableshadercolormask", displaySettingsListener);

			displayScreen = new DisplayScreen(this);

			canvas = new AWTGLCanvas_sceDisplay(this);
			setScreenResolution(displayScreen.Width, displayScreen.Height);

			// Remember the last window size only if not running in full screen
			if (Emulator.MainGUI != null && !Emulator.MainGUI.FullScreen)
			{
				ViewportResizeScaleFactor = Settings.Instance.readFloat(resizeScaleFactorSettings, 1f);
			}

			texFb = -1;
			resizedTexFb = -1;
			startModules = false;
			isStarted = false;
			resizePending = false;
			tempSize = 0;

			fb = new FrameBufferSettings(START_VRAM, 512, Screen.width, Screen.height, TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888);
			ge = new FrameBufferSettings(fb);
		}

		public virtual int DesiredFPS
		{
			set
			{
				this.desiredFps = value;
			}
			get
			{
				return desiredFps;
			}
		}


		public void setScreenResolution(int width, int height)
		{
			canvasWidth = width;
			canvasHeight = height;
			canvas.setSize(width, height);
		}

		public virtual float ViewportResizeScaleFactor
		{
			get
			{
				return viewportResizeFilterScaleFactor;
			}
			set
			{
				if (value < 1)
				{
					// Invalid value
					return;
				}
    
				if (value != sceDisplay.viewportResizeFilterScaleFactor)
				{
					forceSetViewportResizeScaleFactor(value);
				}
			}
		}

		public virtual void setViewportResizeScaleFactor(int width, int height)
		{
			// Compute the scale factor in the horizontal and vertical directions
			float scaleWidth = ((float) width) / displayScreen.Width;
			float scaleHeight = ((float) height) / displayScreen.Height;

			// We are currently using only one scale factor to keep the PSP aspect ratio
			float scaleAspectRatio;
			if (Emulator.MainGUI != null && Emulator.MainGUI.FullScreen)
			{
				// In full screen mode, also keep the aspect ratio.
				// The best aspect ratio is when the horizontal or vertical dimension
				// is matching the screen size and the other dimension is less or equal
				// to the screen size.
				Dimension fullScreenDimension = MainGUI.FullScreenDimension;
				if (fullScreenDimension.width == width && fullScreenDimension.height > height)
				{
					// Screen stretched to the full width
					scaleAspectRatio = scaleWidth;
				}
				else if (fullScreenDimension.height == height && fullScreenDimension.width > width)
				{
					// Screen stretched to the full height
					scaleAspectRatio = scaleHeight;
				}
				else
				{
					scaleAspectRatio = System.Math.Min(scaleWidth, scaleHeight);
				}
			}
			else
			{
				scaleAspectRatio = (scaleWidth + scaleHeight) / 2;
			}
			ViewportResizeScaleFactor = scaleAspectRatio;

			resizePending = true;
		}

		private void forceSetViewportResizeScaleFactor(float viewportResizeFilterScaleFactor)
		{
			// Save the current window size only if not in full screen
			if (!Emulator.MainGUI.FullScreen)
			{
				Settings.Instance.writeFloat(resizeScaleFactorSettings, viewportResizeFilterScaleFactor);
			}

			// The GE has been resized, reset the GETextureManager at next paintGL
			resetGeTextures = true;

			sceDisplay.viewportResizeFilterScaleFactor = viewportResizeFilterScaleFactor;
			sceDisplay.viewportResizeFilterScaleFactorInt = System.Math.Round((float) System.Math.Ceiling(viewportResizeFilterScaleFactor));

			Dimension size = new Dimension(getResizedWidth(displayScreen.Width), getResizedHeight(displayScreen.Height));

			// Resize the component while keeping the PSP aspect ratio
			canvas.Size = size;

			// The preferred size is used when resizing the MainGUI
			canvas.PreferredSize = size;

			if (Emulator.MainGUI.FullScreen)
			{
				Emulator.MainGUI.setFullScreenDisplaySize();
			}

			// Recreate the texture if the scale factor has changed
			createTex = true;

			if (ExternalGE.Active)
			{
				ExternalGE.ScreenScale = viewportResizeFilterScaleFactorInt;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("setViewportResizeScaleFactor resize={0:F}, size({1:D}x{2:D}), canvas({3:D}x{4:D}), location({5:D},{6:D})", viewportResizeFilterScaleFactor, size.width, size.height, canvasWidth, canvasHeight, canvas.Location.x, canvas.Location.y));
			}
		}


		public virtual void updateDisplaySize()
		{
			float scaleFactor = viewportResizeFilterScaleFactor;
			setDisplayMinimumSize();
			Emulator.MainGUI.setDisplaySize(getResizedWidth(displayScreen.Width), getResizedHeight(displayScreen.Height));
			forceSetViewportResizeScaleFactor(scaleFactor);
		}

		public virtual void setDisplayMinimumSize()
		{
			Emulator.MainGUI.setDisplayMinimumSize(displayScreen.Width, displayScreen.Height);
		}

		/// <summary>
		/// Resize the given value according to the viewport resizing factor,
		/// assuming it is a value along the X-Axis (e.g. "x" or "width" value).
		/// </summary>
		/// <param name="width"> value on the X-Axis to be resized </param>
		/// <returns> the resized value </returns>
		public static int getResizedWidth(int width)
		{
			return System.Math.Round(width * viewportResizeFilterScaleFactor);
		}

		public virtual bool DisplaySwappedXY
		{
			get
			{
				return displayScreen.SwappedXY;
			}
		}

		/// <summary>
		/// Resize the given value according to the viewport resizing factor,
		/// assuming it is a value along the X-Axis being a power of 2 (i.e. 2^n).
		/// </summary>
		/// <param name="wantedWidth"> value on the X-Axis to be resized, must be a power of
		/// 2. </param>
		/// <returns> the resized value, as a power of 2. </returns>
		public static int getResizedWidthPow2(int widthPow2)
		{
			return widthPow2 * viewportResizeFilterScaleFactorInt;
		}

		/// <summary>
		/// Resize the given value according to the viewport resizing factor,
		/// assuming it is a value along the Y-Axis (e.g. "y" or "height" value).
		/// </summary>
		/// <param name="height"> value on the Y-Axis to be resized </param>
		/// <returns> the resized value </returns>
		public static int getResizedHeight(int height)
		{
			return System.Math.Round(height * viewportResizeFilterScaleFactor);
		}

		/// <summary>
		/// Resize the given value according to the viewport resizing factor,
		/// assuming it is a value along the Y-Axis being a power of 2 (i.e. 2^n).
		/// </summary>
		/// <param name="wantedWidth"> value on the Y-Axis to be resized, must be a power of
		/// 2. </param>
		/// <returns> the resized value, as a power of 2. </returns>
		public static int getResizedHeightPow2(int heightPow2)
		{
			return heightPow2 * viewportResizeFilterScaleFactorInt;
		}

		private int AntiAliasSamplesNum
		{
			set
			{
				antiAliasSamplesNum = value;
			}
		}

		public virtual int FramePerSecFactor
		{
			set
			{
				if (log.InfoEnabled)
				{
					log.info(string.Format("setFramePerSecFactor {0:D}", value));
				}
				this.framePerSecFactor = value;
			}
		}

		public override void start()
		{
			statistics = new DurationStatistics("sceDisplay Statistics");
			statisticsCopyGeToMemory = new DurationStatistics("Copy GE to Memory");
			statisticsCopyMemoryToGe = new DurationStatistics("Copy Memory to GE");

			// Log debug information...
			//if (log.DebugEnabled)
			{
				try
				{
					DisplayMode[] availableDisplayModes = Display.AvailableDisplayModes;
					for (int i = 0; availableDisplayModes != null && i < availableDisplayModes.Length; i++)
					{
						Console.WriteLine(string.Format("Available Display Mode #{0:D} = {1}", i, availableDisplayModes[i]));
					}
					Console.WriteLine(string.Format("Desktop Display Mode = {0}", Display.DesktopDisplayMode));
					Console.WriteLine(string.Format("Current Display Mode = {0}", Display.DisplayMode));
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("initGL called = %b, OpenGL Version = %s", initGLcalled, openGLversion));
					Console.WriteLine(string.Format("initGL called = %b, OpenGL Version = %s", initGLcalled, openGLversion));
				}
				catch (LWJGLException e)
				{
					Console.WriteLine(e);
				}
			}

			if (!initGLcalled && !calledFromCommandLine)
			{
				// Some problem occurred during the OpenGL/LWJGL initialization...
				throw new Exception("pspsharp cannot display.\nThe cause could be that you are using an old graphic card driver (try to update it)\nor your display format is not compatible with pspsharp (try to change your display format, pspsharp requires 32 bit color depth)\nor the anti-aliasing settings is not supported by your display (leave the pspsharp anti-aliasing to its default setting)");
			}

			// Reset the FB and GE settings only when not called from a syscall.
			// E.g. sceKernelLoadExec() is not clearing/resetting the display.
			if (!HLEModuleManager.Instance.StartFromSyscall)
			{
				mode = 0;
				const int bufferWidth = 512;
				fb = new FrameBufferSettings(START_VRAM, bufferWidth, Screen.width, Screen.height, TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888);
				ge = new FrameBufferSettings(fb);
				sync = PSP_DISPLAY_SETBUF_IMMEDIATE;

				texS = (float) fb.Width / (float) bufferWidth;
				texT = (float) fb.Height / (float) makePow2(fb.Height);
				displayScreen.update();

				createTex = true;
			}

			detailsDirty = true;
			displayDirty = true;
			geDirty = false;

			isFbShowing = false;
			setGeBufCalledAtLeastOnce = false;
			gotBadGeBufParams = false;
			gotBadFbBufParams = false;

			prevStatsTime = 0;
			frameCount = 0;
			paintFrameCount = 0;
			prevFrameCount = 0;
			prevPaintFrameCount = 0;
			reportCount = 0;
			insideRendering = false;
			framePerSecFactor = 1;

			vcount = 0;

			if (asyncDisplayThread == null)
			{
				asyncDisplayThread = new AsyncDisplayThread();
				asyncDisplayThread.Daemon = true;
				asyncDisplayThread.Name = "Async Display Thread";
				asyncDisplayThread.Start();
			}

			if (displayVblankAction == null)
			{
				displayVblankAction = new DisplayVblankAction(this);
				IntrManager.Instance.addVBlankAction(displayVblankAction);
			}

			waitingOnVblank = new LinkedList<WaitVblankInfo>();

			// The VideoEngine needs to be started when a valid GL is available.
			// Start the VideoEngine at the next display(GLAutoDrawable).
			startModules = true;
			re = null;
			reDisplay = null;
			resetDisplaySettings = false;

			saveGEToTexture = Settings.Instance.readBool("emu.enablegetexture");
			if (saveGEToTexture)
			{
				log.info("Saving GE to Textures");
			}

			try
			{
				captureRobot = new Robot();
				captureRobot.AutoDelay = 0;
			}
			catch (Exception)
			{
				// Ignore.
			}

			setSettingsListener("emu.onlyGEGraphics", new OnlyGeSettingsListener(this));
			setSettingsListener("emu.useSoftwareRenderer", new SoftwareRendererSettingsListener(this));
			setSettingsListener("emu.saveStencilToMemory", new SaveStencilToMemorySettingsListener(this));

			base.start();
		}

		public override void stop()
		{
			VideoEngine.Instance.stop();
			if (asyncDisplayThread != null)
			{
				asyncDisplayThread.exit();
				asyncDisplayThread = null;
			}
			re = null;
			reDisplay = null;
			startModules = false;
			isStarted = false;

			base.stop();
		}

		public virtual void exit()
		{
			if (statistics != null && DurationStatistics.collectStatistics)
			{
				log.info("----------------------------- sceDisplay exit -----------------------------");
				log.info(statistics.ToString());
				log.info(statisticsCopyGeToMemory.ToString());
				log.info(statisticsCopyMemoryToGe.ToString());
			}
		}

		public virtual void step(bool immediately)
		{
			long now = DateTimeHelper.CurrentUnixTimeMillis();
			if (immediately || now - lastUpdate > 1000 / 60 || geDirty)
			{
				if (!OnlyGEGraphics || VideoEngine.Instance.hasDrawLists())
				{
					if (geDirty || detailsDirty || displayDirty)
					{
						detailsDirty = false;
						displayDirty = false;
						geDirty = false;

						asyncDisplayThread.display();
					}
				}
				lastUpdate = now;
			}
		}

		public virtual void step()
		{
			step(false);
		}

		public void write8(int rawAddress)
		{
			if (fb.isRawAddressInside(rawAddress))
			{
				displayDirty = true;
			}
		}

		public void write16(int rawAddress)
		{
			if (fb.isRawAddressInside(rawAddress))
			{
				displayDirty = true;
			}
		}

		public void write32(int rawAddress)
		{
			if (fb.isRawAddressInside(rawAddress))
			{
				displayDirty = true;
			}
		}

		public void write(int rawAddress)
		{
			if (fb.isAddressInside(rawAddress))
			{
				displayDirty = true;
			}
		}

		public virtual IRenderingEngine RenderingEngine
		{
			get
			{
				return re;
			}
			set
			{
				this.re = value;
			}
		}


		public virtual bool GeDirty
		{
			set
			{
				geDirty = value;
    
				if (value && softwareRenderingDisplayThread != null)
				{
					// Start immediately the software rendering.
					// No need to wait for the OpenGL display call.
					softwareRenderingDisplayThread.display();
				}
			}
		}

		public virtual void hleDisplaySetGeMode(int width, int height)
		{
			if (width <= 0 || height <= 0)
			{
				Console.WriteLine(string.Format("hleDisplaySetGeMode width={0:D}, height={1:D} bad params", width, height));
			}
			else
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("hleDisplaySetGeMode width={0:D}, height={1:D}", width, height));
				}
				ge.setDimension(width, height);
			}
		}

		public virtual void hleDisplaySetGeBuf(int topaddr, int bufferwidth, int pixelformat, bool copyGEToMemory, bool forceLoadGEToScreen)
		{
			hleDisplaySetGeBuf(topaddr, bufferwidth, pixelformat, copyGEToMemory, forceLoadGEToScreen, ge.Width, ge.Height);
		}

		public virtual void hleDisplaySetGeBuf(int topaddr, int bufferwidth, int pixelformat, bool copyGEToMemory, bool forceLoadGEToScreen, int width, int height)
		{
			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("hleDisplaySetGeBuf topaddr=0x%08X, bufferwidth=%d, pixelformat=%d, copyGE=%b, with=%d, height=%d", topaddr, bufferwidth, pixelformat, copyGEToMemory, width, height));
				Console.WriteLine(string.Format("hleDisplaySetGeBuf topaddr=0x%08X, bufferwidth=%d, pixelformat=%d, copyGE=%b, with=%d, height=%d", topaddr, bufferwidth, pixelformat, copyGEToMemory, width, height));
			}

			// Do not copy the GE to memory or reload it if we are using the software
			// renderer or skipping this frame.
			if (UsingSoftwareRenderer || VideoEngine.Instance.SkipThisFrame)
			{
				copyGEToMemory = false;
				forceLoadGEToScreen = false;
			}

			// The lower 2 bits of the bufferwidth are ignored.
			// E.g., the following bufferwidth values are valid: 120, 240, 480, 256, 512...
			bufferwidth = bufferwidth & ~0x3;

			// The lower 3 bits of FBP are ignored and the upper 8 bits are forced to VRAM.
			topaddr = (topaddr & 0x00FFFFF0) | MemoryMap.START_VRAM;

			if (topaddr == ge.TopAddr && bufferwidth == ge.BufferWidth && pixelformat == ge.PixelFormat && width == ge.Width && height == ge.Height)
			{

				// Nothing changed
				if (forceLoadGEToScreen)
				{
					loadGEToScreen();
				}

				return;
			}

			if (topaddr < MemoryMap.START_VRAM || topaddr >= MemoryMap.END_VRAM || bufferwidth <= 0 || pixelformat < 0 || pixelformat > 3 || (sync != PSP_DISPLAY_SETBUF_IMMEDIATE && sync != PSP_DISPLAY_SETBUF_NEXTFRAME))
			{
				// First time is usually initializing GE, so we can ignore it
				if (setGeBufCalledAtLeastOnce)
				{
					Console.WriteLine(string.Format("hleDisplaySetGeBuf topaddr=0x{0:X8}, bufferwidth={1:D}, pixelformat={2:D} bad params", topaddr, bufferwidth, pixelformat));
					gotBadGeBufParams = true;
				}
				else
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("hleDisplaySetGeBuf topaddr=0x{0:X8}, bufferwidth={1:D}, pixelformat={2:D} bad params", topaddr, bufferwidth, pixelformat));
					}
					setGeBufCalledAtLeastOnce = true;
				}

				return;
			}
			if (gotBadGeBufParams)
			{
				// print when we get good params after bad params
				gotBadGeBufParams = false;
				if (log.InfoEnabled)
				{
					log.info(string.Format("hleDisplaySetGeBuf topaddr=0x{0:X8}, bufferwidth={1:D}, pixelformat={2:D} OK", topaddr, bufferwidth, pixelformat));
				}
			}

			if (re.VertexArrayAvailable)
			{
				re.bindVertexArray(0);
			}

			// Always reload the GE memory to the screen,
			// if not rendering in software and not skipping this frame.
			bool loadGEToScreen = !UsingSoftwareRenderer && !VideoEngine.Instance.SkipThisFrame;

			if (copyGEToMemory && (ge.TopAddr != topaddr || ge.PixelFormat != pixelformat))
			{
				copyGeToMemory(false, false);
				loadGEToScreen = true;
			}

			ge = new FrameBufferSettings(topaddr, bufferwidth, width, height, pixelformat);

			// Tested on PSP:
			// The height of the buffer always matches the display height.
			// This data can be obtained from hleDisplaySetGeMode, since the width
			// represents the display width in pixels, and the height represents
			// the display height in lines.

			checkTemp();

			if (loadGEToScreen)
			{
				this.loadGEToScreen();

				if (State.captureGeNextFrame)
				{
					captureGeImage();
				}
			}

			setGeBufCalledAtLeastOnce = true;
		}

		public static int getPixelFormatBytes(int pixelformat)
		{
			return pspsharp.graphics.RE.IRenderingEngine_Fields.sizeOfTextureType[pixelformat];
		}

		public virtual bool isGeAddress(int address)
		{
			if (ExternalGE.Active)
			{
				return ExternalGE.isGeAddress(address);
			}
			return ge.isAddressInside(address);
		}

		public virtual bool isFbAddress(int address)
		{
			return fb.isAddressInside(address);
		}

		public virtual bool OnlyGEGraphics
		{
			get
			{
				// "Only GE Graphics" makes only sense when the ExternalGE is not active
				return onlyGEGraphics && !ExternalGE.Active;
			}
			set
			{
				this.onlyGEGraphics = value;
	//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
	//ORIGINAL LINE: log.info(String.format("Only GE Graphics: %b", value));
				log.info(string.Format("Only GE Graphics: %b", value));
			}
		}


		public virtual bool SaveStencilToMemory
		{
			get
			{
				return saveStencilToMemory;
			}
			set
			{
				this.saveStencilToMemory = value;
	//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
	//ORIGINAL LINE: log.info(String.format("Save Stencil To Memory: %b", value));
				log.info(string.Format("Save Stencil To Memory: %b", value));
			}
		}


		public virtual bool UseSoftwareRenderer
		{
			set
			{
				this.useSoftwareRenderer = value;
    
				// Start/stop the software rendering display thread
				if (value)
				{
					if (!Screen.hasScreen() && softwareRenderingDisplayThread == null)
					{
						softwareRenderingDisplayThread = new SoftwareRenderingDisplayThread();
						softwareRenderingDisplayThread.Daemon = true;
						softwareRenderingDisplayThread.Name = "GUI";
						softwareRenderingDisplayThread.Start();
						Console.WriteLine("Starting Software Rendering Display Thread");
					}
				}
				else
				{
					if (softwareRenderingDisplayThread != null)
					{
						Console.WriteLine("Stopping Software Rendering Display Thread");
						softwareRenderingDisplayThread.exit();
						softwareRenderingDisplayThread = null;
					}
				}
    
				if (isStarted)
				{
					resetDisplaySettings = true;
				}
			}
		}

		public virtual bool UsingSoftwareRenderer
		{
			get
			{
				return useSoftwareRenderer;
			}
		}

		public virtual void rotate(int angleId)
		{
			switch (angleId)
			{
				case 0:
					displayScreen = new DisplayScreenRotation90(this);
					break;
				case 1:
					displayScreen = new DisplayScreenRotation270(this);
					break;
				case 2:
					displayScreen = new DisplayScreenRotation180(this);
					break;
				case 3:
					displayScreen = new DisplayScreenMirrorX(this, new DisplayScreen(this));
					break;
				case 4:
					displayScreen = new DisplayScreen(this);
					break;
			}
			updateDisplaySize();
		}

		public virtual void saveScreen()
		{
			string fileFormat = "png";
			string fileName;
			for (int id = 1; true; id++)
			{
				fileName = string.Format("{0}-Shot-{1:D}.{2}", State.discId, id, fileFormat);
				if (!System.IO.Directory.Exists(fileName) || System.IO.File.Exists(fileName))
				{
					break;
				}
			}

			Rectangle rect = Emulator.MainGUI.CaptureRectangle;
			try
			{
				BufferedImage img = captureRobot.createScreenCapture(rect);
				ImageIO.write(img, fileFormat, new File(fileName));
				img.flush();
			}
			catch (IOException e)
			{
				Console.WriteLine("Error saving screenshot", e);
			}
		}

		// For capture/replay
		public virtual int TopAddrFb
		{
			get
			{
				return fb.TopAddr;
			}
		}

		public virtual int BufferWidthFb
		{
			get
			{
				return fb.BufferWidth;
			}
		}

		public virtual int PixelFormatFb
		{
			get
			{
				return fb.PixelFormat;
			}
		}

		public virtual int Sync
		{
			get
			{
				return sync;
			}
		}

		public virtual int WidthFb
		{
			get
			{
				return fb.Width;
			}
		}

		public virtual int HeightFb
		{
			get
			{
				return fb.Height;
			}
		}

		public virtual int TopAddrGe
		{
			get
			{
				return ge.TopAddr;
			}
		}

		public virtual int BufferWidthGe
		{
			get
			{
				return ge.BufferWidth;
			}
		}

		public virtual int WidthGe
		{
			get
			{
				return ge.Width;
			}
		}

		public virtual int HeightGe
		{
			get
			{
				return ge.Height;
			}
		}

		public virtual BufferInfo BufferInfoGe
		{
			get
			{
				return new BufferInfo(ge.TopAddr, ge.BottomAddr, ge.Width, ge.Height, ge.BufferWidth, ge.PixelFormat);
			}
		}

		public virtual BufferInfo BufferInfoFb
		{
			get
			{
				return new BufferInfo(fb.TopAddr, fb.BottomAddr, fb.Width, fb.Height, fb.BufferWidth, fb.PixelFormat);
			}
		}

		public virtual bool SaveGEToTexture
		{
			get
			{
				return saveGEToTexture;
			}
		}

		public virtual int CanvasWidth
		{
			get
			{
				return canvasWidth;
			}
		}

		public virtual int CanvasHeight
		{
			get
			{
				return canvasHeight;
			}
		}

		public virtual void captureGeImage()
		{
			if (UsingSoftwareRenderer)
			{
				Buffer buffer = Memory.Instance.getBuffer(ge.TopAddr, ge.BufferWidth * ge.Height * getPixelFormatBytes(ge.PixelFormat));
				CaptureManager.captureImage(ge.TopAddr, 0, buffer, ge.Width, ge.Height, ge.BufferWidth, ge.PixelFormat, false, 0, false, false);
				return;
			}

			// Create a GE texture (the texture texFb might not have the right size)
			int texGe = re.genTexture();

			int texturePixelFormat = getTexturePixelFormat(ge.PixelFormat);
			re.bindTexture(texGe);
			re.setTextureFormat(ge.PixelFormat, false);
			re.setTexImage(0, internalTextureFormat, getResizedWidthPow2(ge.BufferWidth), getResizedHeightPow2(Utilities.makePow2(ge.Height)), texturePixelFormat, texturePixelFormat, 0, null);

			re.TextureMipmapMinFilter = TFLT_NEAREST;
			re.TextureMipmapMagFilter = TFLT_NEAREST;
			re.TextureMipmapMinLevel = 0;
			re.TextureMipmapMaxLevel = 0;
			re.setTextureWrapMode(TWRAP_WRAP_MODE_CLAMP, TWRAP_WRAP_MODE_CLAMP);
			re.setPixelStore(getResizedWidthPow2(ge.BufferWidth), getPixelFormatBytes(ge.PixelFormat));

			// Copy screen to the GE texture
			re.copyTexSubImage(0, 0, 0, 0, 0, getResizedWidth(System.Math.Min(ge.Width, ge.BufferWidth)), getResizedHeight(ge.Height));

			// Copy the GE texture into temp buffer
			temp.clear();
			re.getTexImage(0, texturePixelFormat, texturePixelFormat, temp);

			// Capture the GE image
			CaptureManager.captureImage(ge.TopAddr, 0, temp, getResizedWidth(ge.Width), getResizedHeight(ge.Height), getResizedWidthPow2(ge.BufferWidth), texturePixelFormat, false, 0, true, false);

			// Delete the GE texture
			re.deleteTexture(texGe);
		}

		private void convertABGRtoARGB(int[] abgr, int imageSize, bool needAlpha)
		{
			if (needAlpha)
			{
				for (int i = 0; i < imageSize; i++)
				{
					abgr[i] = Utilities.convertABGRtoARGB(abgr[i]);
				}
			}
			else
			{
				for (int i = 0; i < imageSize; i++)
				{
					abgr[i] = Utilities.convertABGRtoARGB(abgr[i]) & 0x00FFFFFF;
				}
			}
		}

		public virtual BufferedImage getCurrentDisplayAsBufferedImage(bool needAlpha)
		{
			BufferedImage image = null;
			int[] abgr = tempIntArray;
			int width = fb.Width;
			int height = fb.Height;
			int bufferWidth = fb.BufferWidth;
			int pixelFormat = fb.PixelFormat;
			if (UsingSoftwareRenderer)
			{
				int imageSize = bufferWidth * height;

				Buffer buffer = Memory.Instance.getBuffer(fb.TopAddr, imageSize * getPixelFormatBytes(pixelFormat));
				if (buffer is IntBuffer)
				{
					image = new BufferedImage(width, height, BufferedImage.TYPE_INT_RGB);
					((IntBuffer) buffer).get(abgr, 0, imageSize);
					convertABGRtoARGB(abgr, imageSize, needAlpha);
				}
				else
				{
					// TODO Implement getCurrentDisplayAsBufferedImage for the software renderer
					Console.WriteLine("sceDisplay.getCurrentDisplayAsBufferedImage not yet implemented for the software renderer");
				}
			}
			else
			{
				int lineWidth = getResizedWidth(System.Math.Min(width, bufferWidth));
				width = getResizedWidth(width);
				height = getResizedHeight(height);
				bufferWidth = getResizedWidthPow2(bufferWidth);
				int imageSize = bufferWidth * height;
				image = new BufferedImage(width, height, BufferedImage.TYPE_INT_RGB);

				if (ExternalGE.Active)
				{
					ByteBuffer scaledScreen = ExternalGE.getScaledScreen(fb.TopAddr, fb.BufferWidth, fb.Height, pixelFormat);
					if (scaledScreen == null)
					{
						Buffer buffer = Memory.Instance.getBuffer(fb.TopAddr, imageSize * getPixelFormatBytes(pixelFormat));
						if (buffer is IntBuffer)
						{
							((IntBuffer) buffer).get(abgr, 0, imageSize);
							convertABGRtoARGB(abgr, imageSize, needAlpha);
						}
					}
					else
					{
						scaledScreen.asIntBuffer().get(abgr, 0, imageSize);
						for (int i = 0; i < imageSize; i++)
						{
							abgr[i] = (int)((uint)abgr[i] >> 8);
						}
					}
				}
				else
				{
					// Create a GE texture (the texture texFb might not have the right size)
					int texGe = re.genTexture();

					re.bindTexture(texGe);
					re.setTextureFormat(fb.PixelFormat, false);
					re.setTexImage(0, internalTextureFormat, bufferWidth, getResizedHeightPow2(Utilities.makePow2(fb.Height)), pixelFormat, pixelFormat, 0, null);

					re.TextureMipmapMinFilter = TFLT_NEAREST;
					re.TextureMipmapMagFilter = TFLT_NEAREST;
					re.TextureMipmapMinLevel = 0;
					re.TextureMipmapMaxLevel = 0;
					re.setTextureWrapMode(TWRAP_WRAP_MODE_CLAMP, TWRAP_WRAP_MODE_CLAMP);
					re.setPixelStore(bufferWidth, getPixelFormatBytes(pixelFormat));

					// Copy screen to the GE texture
					re.copyTexSubImage(0, 0, 0, 0, 0, lineWidth, height);

					// Copy the GE texture into temp buffer
					temp.clear();
					re.getTexImage(0, pixelFormat, pixelFormat, temp);

					// Delete the GE texture
					re.deleteTexture(texGe);

					IntBuffer intBuffer = tempByteBuffer.asIntBuffer();
					// The image is flipped vertically
					for (int y = 0; y < height; y++)
					{
						intBuffer.get(abgr, (height - 1 - y) * bufferWidth, bufferWidth);
					}

					convertABGRtoARGB(abgr, imageSize, needAlpha);
				}
			}

			image.setRGB(0, 0, System.Math.Min(bufferWidth, width), height, abgr, 0, bufferWidth);

			return image;
		}

		public virtual void captureCurrentTexture(int address, int width, int height, int bufferWidth, int pixelFormat)
		{
			// Copy the texture into temp buffer
			re.setPixelStore(bufferWidth, getPixelFormatBytes(pixelFormat));
			temp.clear();
			re.getTexImage(0, pixelFormat, pixelFormat, temp);

			// Capture the image
			CaptureManager.captureImage(address, 0, temp, width, height, bufferWidth, pixelFormat, false, 0, true, false);
		}

		private void reportFPSStats()
		{
			long timeNow = DateTimeHelper.CurrentUnixTimeMillis();
			long realElapsedTime = timeNow - prevStatsTime;

			if (realElapsedTime > 1000L)
			{
				reportCount++;

				if (frameCount == prevFrameCount)
				{
					// If the application is not using a double-buffering technique
					// for the framebuffer display (i.e. if the application is not changing
					// the value of the framebuffer address), then use the number
					// of GE list executed to compute the FPS value.
					frameCount = paintFrameCount;
					prevFrameCount = prevPaintFrameCount;
				}

				int lastFPS = (int)(frameCount - prevFrameCount);
				double averageFPS = frameCount / (double) reportCount;
				prevFrameCount = frameCount;
				prevPaintFrameCount = paintFrameCount;
				prevStatsTime = timeNow;

				Emulator.FpsTitle = string.Format("FPS: {0:D}, averageFPS: {1:F1}", lastFPS, averageFPS);
			}
		}

		private void loadGEToScreen()
		{
			if (VideoEngine.log_Renamed.DebugEnabled)
			{
				VideoEngine.log_Renamed.debug(string.Format("Reloading GE Memory (0x{0:X8}-0x{1:X8}) to screen ({2:D}x{3:D})", ge.TopAddr, ge.BottomAddr, ge.Width, ge.Height));
			}

			if (statisticsCopyMemoryToGe != null)
			{
				statisticsCopyMemoryToGe.start();
			}

			if (saveGEToTexture && !VideoEngine.Instance.isVideoTexture(ge.TopAddr))
			{
				GETexture geTexture = GETextureManager.Instance.getGETexture(re, ge.TopAddr, ge.BufferWidth, ge.Width, ge.Height, ge.PixelFormat, true);
				geTexture.copyTextureToScreen(re);
			}
			else
			{
				if (re.VertexArrayAvailable)
				{
					re.bindVertexArray(0);
				}

				// Set texFb as the current texture
				re.bindTexture(texFb);

				// Define the texture from the GE Memory
				re.setPixelStore(ge.BufferWidth, getPixelFormatBytes(ge.PixelFormat));
				int textureSize = ge.BufferWidth * ge.Height * getPixelFormatBytes(ge.PixelFormat);
				ge.Pixels.clear();
				re.setTexSubImage(0, 0, 0, ge.BufferWidth, ge.Height, ge.PixelFormat, ge.PixelFormat, textureSize, ge.Pixels);

				// Draw the GE
				drawFrameBuffer(fb, false, true, ge.BufferWidth, ge.PixelFormat, ge.Width, ge.Height);
			}

			if (statisticsCopyMemoryToGe != null)
			{
				statisticsCopyMemoryToGe.end();
			}
		}

		private void copyStencilToMemory()
		{
			if (ge.PixelFormat >= stencilPixelMasks.Length)
			{
				Console.WriteLine(string.Format("copyGeToMemory: unimplemented pixelformat {0:D} for Stencil buffer copy", ge.PixelFormat));
				return;
			}
			if (stencilValueMasks[ge.PixelFormat] == 0)
			{
				// No stencil value for BGR5650, nothing to copy for the stencil
				return;
			}

			// Be careful to not overwrite parts of the GE memory used by the application for another purpose.
			VideoEngine videoEngine = VideoEngine.Instance;
			int stencilWidth = System.Math.Min(ge.Width, ge.BufferWidth);
			int stencilHeight = System.Math.Min(ge.Height, videoEngine.MaxSpriteHeight);
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Copy stencil to GE: pixelFormat={0:D}, {1:D}x{2:D}, maxSprite={3:D}x{4:D}", ge.PixelFormat, stencilWidth, stencilHeight, videoEngine.MaxSpriteWidth, videoEngine.MaxSpriteHeight));
			}

			int stencilBufferSize = stencilWidth * stencilHeight;
			tempByteBuffer.clear();
			re.setPixelStore(stencilWidth, 1);
			re.readStencil(0, 0, stencilWidth, stencilHeight, stencilBufferSize, tempByteBuffer);

			int bytesPerPixel = pspsharp.graphics.RE.IRenderingEngine_Fields.sizeOfTextureType[ge.PixelFormat];
			IMemoryReaderWriter memoryReaderWriter = MemoryReaderWriter.getMemoryReaderWriter(ge.TopAddr, stencilHeight * ge.BufferWidth * bytesPerPixel, bytesPerPixel);
			tempByteBuffer.rewind();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int stencilPixelMask = stencilPixelMasks[ge.getPixelFormat()];
			int stencilPixelMask = stencilPixelMasks[ge.PixelFormat];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int stencilValueMask = stencilValueMasks[ge.getPixelFormat()];
			int stencilValueMask = stencilValueMasks[ge.PixelFormat];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int stencilValueShift = stencilValueShifts[ge.getPixelFormat()];
			int stencilValueShift = stencilValueShifts[ge.PixelFormat];
			for (int y = 0; y < stencilHeight; y++)
			{
				// The stencil buffer is stored upside-down by OpenGL
				tempByteBuffer.position((stencilHeight - y - 1) * stencilWidth);

				for (int x = 0; x < stencilWidth; x++)
				{
					int pixel = memoryReaderWriter.readCurrent();
					int stencilValue = tempByteBuffer.get() & stencilValueMask;
					pixel = (pixel & stencilPixelMask) | (stencilValue << stencilValueShift);
					memoryReaderWriter.writeNext(pixel);
				}

				if (stencilWidth < ge.BufferWidth)
				{
					memoryReaderWriter.skip(ge.BufferWidth - stencilWidth);
				}
			}
			memoryReaderWriter.flush();

			if (GEProfiler.ProfilerEnabled)
			{
				GEProfiler.copyStencilToMemory();
			}
		}

		/// <summary>
		/// Copy the GE at from given address to memory.
		/// This is only required when saving the GE to textures.
		/// </summary>
		/// <param name="geTopAddress"> the GE address that need to be saved to memory </param>
		public virtual void copyGeToMemory(int geTopAddress)
		{
			if (UsingSoftwareRenderer || ExternalGE.Active)
			{
				// GE is already in memory when using the internal/external software renderer
				return;
			}
			if (!saveGEToTexture)
			{
				// Copying the GE to memory is only necessary when saving the GE to textures
				return;
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("copyGeToMemory starting with geTopAddress=0x{0:X8}", geTopAddress));
			}

			doneCopyGeToMemory = false;
			addDisplayActionOce(new CopyGeToMemoryAction(this, geTopAddress));

			geDirty = true;
			step(true);

			// Poll completion of copyGeToMemory action
			while (!doneCopyGeToMemory)
			{
				Utilities.sleep(1, 0);
			}
			doneCopyGeToMemory = false;

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("copyGeToMemory done with geTopAddress=0x{0:X8}", geTopAddress));
			}
		}

		public virtual void copyGeToMemory(bool preserveScreen, bool forceCopyToMemory)
		{
			copyGeToMemory(ge.TopAddr, preserveScreen, forceCopyToMemory);
		}

		public virtual void copyGeToMemory(int geTopAddress, bool preserveScreen, bool forceCopyToMemory)
		{
			if (UsingSoftwareRenderer)
			{
				// GE is already in memory when using the software renderer
				return;
			}

			if (VideoEngine.log_Renamed.DebugEnabled)
			{
				VideoEngine.log_Renamed.debug(string.Format("Copy GE Screen to Memory 0x{0:X8}-0x{1:X8}", geTopAddress, geTopAddress + ge.Size));
			}

			if (statisticsCopyGeToMemory != null)
			{
				statisticsCopyGeToMemory.start();
			}

			if (saveGEToTexture && !VideoEngine.Instance.isVideoTexture(geTopAddress))
			{
				GETexture geTexture = GETextureManager.Instance.getGETexture(re, geTopAddress, ge.BufferWidth, ge.Width, ge.Height, ge.PixelFormat, true);
				geTexture.copyScreenToTexture(re);
			}
			else
			{
				forceCopyToMemory = true;
			}

			if (forceCopyToMemory)
			{
				// Set texFb as the current texture
				re.bindTexture(resizedTexFb);
				re.setTextureFormat(ge.PixelFormat, false);

				// Copy screen to the current texture
				re.copyTexSubImage(0, 0, 0, 0, 0, getResizedWidth(System.Math.Min(ge.BufferWidth, ge.Width)), getResizedHeight(ge.Height));

				// Re-render GE/current texture upside down
				drawFrameBuffer(fb, true, true, ge.BufferWidth, ge.PixelFormat, ge.Width, ge.Height);

				copyScreenToPixels(ge.getPixels(geTopAddress), ge.BufferWidth, ge.PixelFormat, ge.Width, ge.Height);

				if (saveStencilToMemory)
				{
					copyStencilToMemory();
				}

				if (preserveScreen)
				{
					// Redraw the screen
					re.bindTexture(resizedTexFb);
					drawFrameBuffer(fb, false, false, ge.BufferWidth, ge.PixelFormat, ge.Width, ge.Height);
				}
			}

			if (statisticsCopyGeToMemory != null)
			{
				statisticsCopyGeToMemory.end();
			}

			if (GEProfiler.ProfilerEnabled)
			{
				GEProfiler.copyGeToMemory();
			}
		}

		/// <param name="keepOriginalSize"> : true = draw as psp size false = draw as window
		/// size
		///  </param>
		private void drawFrameBuffer(FrameBufferSettings fb, bool keepOriginalSize, bool invert, int bufferwidth, int pixelformat, int width, int height)
		{
			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("drawFrameBuffer fb=%s, keepOriginalSize=%b, invert=%b, bufferWidth=%d, pixelFormat=%d, width=%d, height=%d, %s", fb, keepOriginalSize, invert, bufferwidth, pixelformat, width, height, displayScreen));
				Console.WriteLine(string.Format("drawFrameBuffer fb=%s, keepOriginalSize=%b, invert=%b, bufferWidth=%d, pixelFormat=%d, width=%d, height=%d, %s", fb, keepOriginalSize, invert, bufferwidth, pixelformat, width, height, displayScreen));
			}

			reDisplay.startDirectRendering(true, false, true, true, !invert, width, height);
			if (keepOriginalSize)
			{
				reDisplay.setViewport(0, 0, width, height);
			}
			else
			{
				reDisplay.setViewport(0, 0, getResizedWidth(width), getResizedHeight(height));
			}

			reDisplay.setTextureFormat(pixelformat, false);

			float scale = 1f;
			if (keepOriginalSize)
			{
				// When keeping the original size, we still have to adjust the size of the texture mapping.
				// E.g. when the screen has been resized to 576x326 (resizeScaleFactor=1.2),
				// the texture has been created with a size 1024x1024 and the following texture
				// coordinates have to used:
				//     (576/1024, 326/1024),
				// while texS==480/512 and texT==272/512
				scale = (float) getResizedHeight(height) / (float) getResizedHeightPow2(makePow2(height));
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("drawFrameBuffer scale = {0:F} / {1:F} = {2:F}", scale, texT, scale / texT));
				}
				scale /= texT;
			}

			int i = 0;
			drawBufferArray[i++] = displayScreen.TextureLowerRightS * scale;
			drawBufferArray[i++] = displayScreen.TextureLowerRightT * scale;
			drawBufferArray[i++] = (float) width;
			drawBufferArray[i++] = (float) height;

			drawBufferArray[i++] = displayScreen.TextureLowerLeftS * scale;
			drawBufferArray[i++] = displayScreen.TextureLowerLeftT * scale;
			drawBufferArray[i++] = 0f;
			drawBufferArray[i++] = (float) height;

			drawBufferArray[i++] = displayScreen.TextureUpperLeftS * scale;
			drawBufferArray[i++] = displayScreen.TextureUpperLeftT * scale;
			drawBufferArray[i++] = 0f;
			drawBufferArray[i++] = 0f;

			drawBufferArray[i++] = displayScreen.TextureUpperRightS * scale;
			drawBufferArray[i++] = displayScreen.TextureUpperRightT * scale;
			drawBufferArray[i++] = (float) width;
			drawBufferArray[i++] = 0f;

			int bufferSizeInFloats = i;
			IREBufferManager bufferManager = reDisplay.BufferManager;
			ByteBuffer byteBuffer = bufferManager.getBuffer(drawBuffer);
			byteBuffer.clear();
			byteBuffer.asFloatBuffer().put(drawBufferArray, 0, bufferSizeInFloats);

			if (reDisplay.VertexArrayAvailable)
			{
				reDisplay.bindVertexArray(0);
			}
			reDisplay.setVertexInfo(null, false, false, true, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_QUADS);
			reDisplay.enableClientState(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_TEXTURE);
			reDisplay.disableClientState(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_COLOR);
			reDisplay.disableClientState(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_NORMAL);
			reDisplay.enableClientState(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_VERTEX);
			bufferManager.setTexCoordPointer(drawBuffer, 2, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FLOAT, 4 * SIZEOF_FLOAT, 0);
			bufferManager.setVertexPointer(drawBuffer, 2, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_FLOAT, 4 * SIZEOF_FLOAT, 2 * SIZEOF_FLOAT);
			bufferManager.setBufferData(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_ARRAY_BUFFER, drawBuffer, bufferSizeInFloats * SIZEOF_FLOAT, byteBuffer, pspsharp.graphics.RE.IRenderingEngine_Fields.RE_DYNAMIC_DRAW);
			reDisplay.drawArrays(pspsharp.graphics.RE.IRenderingEngine_Fields.RE_QUADS, 0, 4);

			reDisplay.endDirectRendering();
		}

		private void drawFrameBufferFromMemory(FrameBufferSettings fb)
		{
			fb.Pixels.clear();
			reDisplay.bindTexture(texFb);
			reDisplay.setTextureFormat(fb.PixelFormat, false);
			reDisplay.setPixelStore(fb.BufferWidth, getPixelFormatBytes(fb.PixelFormat));
			int textureSize = fb.BufferWidth * fb.Height * getPixelFormatBytes(fb.PixelFormat);
			reDisplay.setTexSubImage(0, 0, 0, fb.BufferWidth, fb.Height, fb.PixelFormat, fb.PixelFormat, textureSize, fb.Pixels);

			drawFrameBuffer(fb, false, true, fb.BufferWidth, fb.PixelFormat, displayScreen.getWidth(fb), displayScreen.getHeight(fb));
		}

		private void copyBufferByLines(IntBuffer dstBuffer, IntBuffer srcBuffer, int dstBufferWidth, int srcBufferWidth, int pixelFormat, int width, int height)
		{
			int pixelsPerElement = 4 / getPixelFormatBytes(pixelFormat);
			for (int y = 0; y < height; y++)
			{
				int srcStartOffset = y * srcBufferWidth / pixelsPerElement;
				int dstStartOffset = y * dstBufferWidth / pixelsPerElement;
				srcBuffer.limit(srcStartOffset + (width + 1) / pixelsPerElement);
				srcBuffer.position(srcStartOffset);
				dstBuffer.position(dstStartOffset);
				if (srcBuffer.remaining() < dstBuffer.remaining())
				{
					dstBuffer.put(srcBuffer);
				}
			}
		}

		private void copyScreenToPixels(Buffer pixels, int bufferWidth, int pixelFormat, int width, int height)
		{
			// Set texFb as the current texture
			reDisplay.bindTexture(texFb);
			reDisplay.setTextureFormat(fb.PixelFormat, false);

			reDisplay.setPixelStore(bufferWidth, getPixelFormatBytes(pixelFormat));

			// Copy screen to the current texture
			reDisplay.copyTexSubImage(0, 0, 0, 0, 0, System.Math.Min(bufferWidth, width), height);

			// Copy the current texture into memory
			Buffer buffer = (pixels.capacity() >= temp.capacity() ? pixels : temp);
			buffer.clear();
			reDisplay.getTexImage(0, pixelFormat, pixelFormat, buffer);

			// Copy temp into pixels, temp is probably square and pixels is less,
			// a smaller rectangle, otherwise we could copy straight into pixels.
			if (buffer == temp)
			{
				temp.clear();
				pixels.clear();
				temp.limit(pixels.limit());

				if (temp is ByteBuffer)
				{
					ByteBuffer srcBuffer = (ByteBuffer) temp;
					ByteBuffer dstBuffer = (ByteBuffer) pixels;
					dstBuffer.put(srcBuffer);
				}
				else if (temp is IntBuffer)
				{
					IntBuffer srcBuffer = (IntBuffer) temp;
					IntBuffer dstBuffer = (IntBuffer) pixels;

					VideoEngine videoEngine = VideoEngine.Instance;
					if (videoEngine.UsingTRXKICK && videoEngine.MaxSpriteHeight < int.MaxValue)
					{
						// Hack: God of War is using GE command lists stored into the non-visible
						// part of the GE buffer. The lists are copied from the main memory into
						// the VRAM using TRXKICK. Be careful to not overwrite these non-visible
						// parts.
						//
						// Copy only the visible part of the GE to the memory, e.g.
						// when width==480 and bufferwidth==1024, copy only 480 pixels
						// per line and skip 1024-480 pixels.
						int srcBufferWidth = bufferWidth;
						int dstBufferWidth = bufferWidth;
						int pixelsPerElement = 4 / getPixelFormatBytes(pixelFormat);
						int maxHeight = videoEngine.MaxSpriteHeight;
						int maxWidth = videoEngine.MaxSpriteWidth;
						int textureAlignment = (pixelsPerElement == 1 ? 3 : 7);
						maxHeight = (maxHeight + textureAlignment) & ~textureAlignment;
						maxWidth = (maxWidth + textureAlignment) & ~textureAlignment;
						if (VideoEngine.log_Renamed.DebugEnabled)
						{
							VideoEngine.log_Renamed.debug("maxSpriteHeight=" + maxHeight + ", maxSpriteWidth=" + maxWidth);
						}
						if (maxHeight > height)
						{
							maxHeight = height;
						}
						if (maxWidth > width)
						{
							maxWidth = width;
						}
						copyBufferByLines(dstBuffer, srcBuffer, dstBufferWidth, srcBufferWidth, pixelFormat, maxWidth, maxHeight);
					}
					else
					{
						dstBuffer.put(srcBuffer);
					}
				}
				else
				{
					throw new Exception("unhandled buffer type");
				}
			}
			// We only use "temp" buffer in this function, its limit() will get restored on the next call to clear()
		}

		public virtual int hleDisplayWaitVblankStart(int cycles, bool doCallbacks)
		{
			if (cycles <= 0)
			{
				return SceKernelErrors.ERROR_INVALID_VALUE;
			}

			ThreadManForUser threadMan = Modules.ThreadManForUserModule;
			SceKernelThreadInfo thread = threadMan.CurrentThread;
			int threadId = threadMan.CurrentThreadID;

			int lastWaitVblank = thread.displayLastWaitVcount;
			int unblockVcount = lastWaitVblank + cycles;
			if (unblockVcount <= vcount)
			{
				// This thread has just to wait for the next VBLANK.
				// Add a Vblank action to unblock the thread
				UnblockThreadAction vblankAction = new VblankUnblockThreadAction(this, threadId);
				IntrManager.Instance.addVBlankActionOnce(vblankAction);
				thread.displayLastWaitVcount = vcount + 1;
			}
			else
			{
				// This thread has to wait for multiple VBLANK's
				WaitVblankInfo waitVblankInfo = new WaitVblankInfo(threadId, unblockVcount);
				waitingOnVblank.Add(waitVblankInfo);
			}

			// Block the current thread.
			threadMan.hleBlockCurrentThread(SceKernelThreadInfo.JPCSP_WAIT_DISPLAY_VBLANK, unblockVcount, doCallbacks, null, new VblankWaitStateChecker(this, unblockVcount));

			ExternalGE.onDisplayStartWaitVblank();

			return 0;
		}

		private void hleVblankStart()
		{
			lastVblankMicroTime = Emulator.Clock.microTime();
			// Vcount increases at each VBLANK.
			vcount++;

			ExternalGE.onDisplayVblank();

			// Check the threads waiting for VBLANK (multi).
			if (waitingOnVblank.Count > 0)
			{
//JAVA TO C# CONVERTER WARNING: Unlike Java's ListIterator, enumerators in .NET do not allow altering the collection:
				for (IEnumerator<WaitVblankInfo> lit = waitingOnVblank.GetEnumerator(); lit.MoveNext();)
				{
					WaitVblankInfo waitVblankInfo = lit.Current;
					if (waitVblankInfo.unblockVcount <= vcount)
					{
						ThreadManForUser threadMan = Modules.ThreadManForUserModule;
						SceKernelThreadInfo thread = threadMan.getThreadById(waitVblankInfo.threadId);
						if (thread != null)
						{
							ExternalGE.onDisplayStopWaitVblank();
							thread.displayLastWaitVcount = vcount;
							threadMan.hleUnblockThread(waitVblankInfo.threadId);
						}
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						lit.remove();
					}
				}
			}
		}

		private bool Vblank
		{
			get
			{
				// Test result: isVblank == true during 4.39% of the time
				// -> Vblank takes 731.5 micros at each vblank interrupt
				long nowMicroTime = Emulator.Clock.microTime();
				long microTimeSinceLastVblank = nowMicroTime - lastVblankMicroTime;
    
				return (microTimeSinceLastVblank <= 731);
			}
		}

		private int CurrentHcount
		{
			get
			{
				// Test result: currentHcount is 0 at the start of a Vblank and increases
				// up to 285 just before the next Vblank.
				long nowMicroTime = Emulator.Clock.microTime();
				long microTimeSinceLastVblank = nowMicroTime - lastVblankMicroTime;
    
				float vblankStep = microTimeSinceLastVblank / 16666.6666f;
				if (vblankStep > 1)
				{
					vblankStep = 1;
				}
    
				return (int)(vblankStep * hCountPerVblank);
			}
		}

		public virtual int Vcount
		{
			get
			{
				return vcount;
			}
		}

		public static int getTexturePixelFormat(int pixelFormat)
		{
	//    	return pixelFormat;
			// Always use a 32-bit texture to store the GE.
			// 16-bit textures are causing color artifacts.
			return GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
		}

		private int createTexture(int textureId, bool isResized)
		{
			if (textureId != -1)
			{
				reDisplay.deleteTexture(textureId);
			}
			textureId = reDisplay.genTexture();

			reDisplay.bindTexture(textureId);
			reDisplay.setTextureFormat(fb.PixelFormat, false);

			//
			// The format of the frame (or GE) buffer is
			//   A the alpha & stencil value
			//   R the Red color component
			//   G the Green color component
			//   B the Blue color component
			//
			// GU_PSM_8888 : 0xAABBGGRR
			// GU_PSM_4444 : 0xABGR
			// GU_PSM_5551 : ABBBBBGGGGGRRRRR
			// GU_PSM_5650 : BBBBBGGGGGGRRRRR
			//
			reDisplay.setTexImage(0, internalTextureFormat, isResized ? getResizedWidthPow2(fb.BufferWidth) : fb.BufferWidth, isResized ? getResizedHeightPow2(Utilities.makePow2(fb.Height)) : Utilities.makePow2(fb.Height), getTexturePixelFormat(PixelFormatFb), getTexturePixelFormat(PixelFormatFb), 0, null);
			reDisplay.TextureMipmapMinFilter = GeCommands.TFLT_NEAREST;
			reDisplay.TextureMipmapMagFilter = GeCommands.TFLT_NEAREST;
			reDisplay.TextureMipmapMinLevel = 0;
			reDisplay.TextureMipmapMaxLevel = 0;
			reDisplay.setTextureWrapMode(TWRAP_WRAP_MODE_CLAMP, TWRAP_WRAP_MODE_CLAMP);

			return textureId;
		}

		private void checkTemp()
		{
			int bytesPerPixel = getPixelFormatBytes(System.Math.Max(fb.PixelFormat, ge.PixelFormat));
			// Buffer large enough to store the complete FB or GE texture
			int sizeInBytes = getResizedWidthPow2(System.Math.Max(fb.BufferWidth, ge.BufferWidth)) * getResizedHeightPow2(Utilities.makePow2(System.Math.Max(fb.Height, ge.Height))) * bytesPerPixel;

			if (sizeInBytes > tempSize)
			{
				tempByteBuffer = ByteBuffer.allocateDirect(sizeInBytes).order(ByteOrder.LITTLE_ENDIAN);

				if (Memory.Instance.MainMemoryByteBuffer is IntBuffer)
				{
					temp = tempByteBuffer.asIntBuffer();
				}
				else
				{
					temp = tempByteBuffer;
				}

				tempIntArray = new int[sizeInBytes / bytesPerPixel];

				tempSize = sizeInBytes;
			}
		}

		public virtual void setCalledFromCommandLine()
		{
			calledFromCommandLine = true;
		}

		public virtual void takeScreenshot()
		{
			addDisplayActionOce(new ScreenshotAction(this));
		}

		public virtual bool InsideRendering
		{
			get
			{
				if (ExternalGE.Active)
				{
					if (ExternalGE.InsideRendering)
					{
						return true;
					}
				}
				else
				{
					if (insideRendering)
					{
						PspGeList currentList = VideoEngine.Instance.CurrentList;
						if (currentList != null && currentList.StalledAtStart)
						{
							// We are not really rendering when stalling at the start of the list
							return false;
						}
					}
				}
    
				return insideRendering;
			}
			set
			{
				this.insideRendering = value;
			}
		}


		/// <summary>
		/// If the display is currently rendering to the given address, wait for the
		/// rendering completion. Otherwise, return immediately.
		/// </summary>
		/// <param name="address"> the address to be checked. </param>
		public virtual void waitForRenderingCompletion(int address)
		{
			int countWaitingOnStall = 0;
			while (InsideRendering && isGeAddress(address))
			{
				// Do not wait too long when the VideoEngine is also waiting on a stalled list...
				if (VideoEngine.Instance.WaitingOnStall)
				{
					countWaitingOnStall++;
					if (countWaitingOnStall > 10)
					{
						break;
					}
				}
				else
				{
					countWaitingOnStall = 0;
				}

				// Sleep 10 microseconds for polling...
				Utilities.sleep(10);
			}
		}

		public virtual void addDisplayAction(IAction action)
		{
			displayActions.Add(action);
		}

		public virtual bool removeDisplayAction(IAction action)
		{
			return displayActions.Remove(action);
		}

		public virtual void addDisplayActionOce(IAction action)
		{
			displayActionsOnce.Add(action);
		}

		public virtual bool removeDisplayActionOce(IAction action)
		{
			return displayActionsOnce.Remove(action);
		}

		[HLEFunction(nid : 0x0E20F177, version : 150, checkInsideInterrupt : true)]
		public virtual int sceDisplaySetMode(int displayMode, int displayWidth, int displayHeight)
		{
			if (displayWidth <= 0 || displayHeight <= 0 || (displayWidth & 0x7) != 0 || displayHeight > Screen.height)
			{
				return SceKernelErrors.ERROR_INVALID_SIZE;
			}

			if (displayMode != PSP_DISPLAY_MODE_LCD)
			{
				return SceKernelErrors.ERROR_INVALID_MODE;
			}

			mode = displayMode;
			fb.setDimension(displayWidth, displayHeight);

			detailsDirty = true;

			return 0;
		}

		[HLEFunction(nid : 0xDEA197D4, version : 150)]
		public virtual int sceDisplayGetMode(TPointer32 modeAddr, TPointer32 widthAddr, TPointer32 heightAddr)
		{
			modeAddr.setValue(mode);
			widthAddr.setValue(fb.Width);
			heightAddr.setValue(fb.Height);

			return 0;
		}

		[HLEFunction(nid : 0xDBA6C4C4, version : 150)]
		public virtual float sceDisplayGetFramePerSec()
		{
			// Some applications are using a video playback loop requiring a very exact
			// audio synchronization: the video playback keeps 4 buffers each for video
			// images and for decoded audio buffers. When the buffer timestamps between
			// audio and video differ by a too long delay value, the video playback breaks.
			// The application actually tries to skip video frames to sync up but usually
			// fails to it.
			//
			// The synchronization problem in pspsharp is caused by the audio queues in sceAudio:
			// the blocking methods (sceAudioOutputXXXBlocking) are not blocking as much as the
			// PSP methods are doing. Especially, the first few calls (until the pspsharp queue is
			// filled up) are not blocking at all. This is causing the audio to play faster than
			// the video at the beginning.
			//
			// The allowed delay is computed as follows:
			//    2 * int(sceMpeg.mpegTimestampPerSecond / sceDisplayGetFramePerSec() * 2) = 6006
			//
			// Allow artificially a longer delay by returning here a lower value:
			float framePerSec = FRAME_PER_SEC / framePerSecFactor;

			// The hack is only used once
			framePerSecFactor = 1;

			return framePerSec;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x7ED59BC4, version = 150, checkInsideInterrupt = true) public int sceDisplaySetHoldMode(int holdMode)
		[HLEFunction(nid : 0x7ED59BC4, version : 150, checkInsideInterrupt : true)]
		public virtual int sceDisplaySetHoldMode(int holdMode)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x3552AB11, version = 660, checkInsideInterrupt = true) public int sceDisplaySetHoldMode_660(int holdMode)
		[HLEFunction(nid : 0x3552AB11, version : 660, checkInsideInterrupt : true)]
		public virtual int sceDisplaySetHoldMode_660(int holdMode)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA544C486, version = 150, checkInsideInterrupt = true) public int sceDisplaySetResumeMode(int resumeMode)
		[HLEFunction(nid : 0xA544C486, version : 150, checkInsideInterrupt : true)]
		public virtual int sceDisplaySetResumeMode(int resumeMode)
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x289D82FE, version = 150) public int sceDisplaySetFrameBuf(@CanBeNull pspsharp.HLE.TPointer topaddr, int bufferwidth, int pixelformat, int syncType)
		[HLEFunction(nid : 0x289D82FE, version : 150)]
		public virtual int sceDisplaySetFrameBuf(TPointer topaddr, int bufferwidth, int pixelformat, int syncType)
		{
			return hleDisplaySetFrameBuf(topaddr.Address, bufferwidth, pixelformat, syncType);
		}

		private int hleDisplaySetFrameBufError(int topaddr, int bufferwidth, int pixelformat, int syncType, int error, string errorString)
		{
			Console.WriteLine(string.Format("sceDisplaySetFrameBuf topaddr=0x{0:X8}, bufferwidth={1:D}, pixelformat={2:D}, syncType={3:D} {4}: returning 0x{5:X8}", topaddr, bufferwidth, pixelformat, syncType, errorString, error));
			gotBadFbBufParams = true;
			return error;
		}

		public virtual int hleDisplaySetFrameBuf(int topaddr, int bufferwidth, int pixelformat, int syncType)
		{
			// The PSP is performing the following parameter checks in this sequence
			if (syncType != PSP_DISPLAY_SETBUF_IMMEDIATE && syncType != PSP_DISPLAY_SETBUF_NEXTFRAME)
			{
				return hleDisplaySetFrameBufError(topaddr, bufferwidth, pixelformat, syncType, ERROR_INVALID_MODE, "bad syncType");
			}

			if ((topaddr & 0xF) != 0)
			{
				return hleDisplaySetFrameBufError(topaddr, bufferwidth, pixelformat, syncType, ERROR_INVALID_POINTER, "bad topaddr");
			}

			if (topaddr != 0 && !Memory.isRAM(topaddr) && !Memory.isVRAM(topaddr))
			{
				return hleDisplaySetFrameBufError(topaddr, bufferwidth, pixelformat, syncType, ERROR_INVALID_POINTER, "bad topaddr");
			}

			if ((bufferwidth & 0x3F) != 0)
			{
				return hleDisplaySetFrameBufError(topaddr, bufferwidth, pixelformat, syncType, ERROR_INVALID_SIZE, "bad bufferwidth");
			}

			// bufferwidth can only be 0 when topaddr is NULL
			if (bufferwidth == 0 && topaddr != 0)
			{
				return hleDisplaySetFrameBufError(topaddr, bufferwidth, pixelformat, syncType, ERROR_INVALID_SIZE, "bad bufferwidth");
			}

			if (pixelformat < 0 || pixelformat > TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888)
			{
				return hleDisplaySetFrameBufError(topaddr, bufferwidth, pixelformat, syncType, ERROR_INVALID_FORMAT, "bad pixelformat");
			}

			if (topaddr == 0)
			{
				// If topaddr is NULL, the PSP's screen will be displayed as fully black
				// as the output is blocked. Under these circumstances, bufferwidth can be 0.
				log.info(string.Format("sceDisplaySetFrameBuf topaddr=0x{0:X8}, bufferwidth={1:D}, pixelformat={2:D}, syncType={3:D} (blocking display output)", topaddr, bufferwidth, pixelformat, syncType));
				isFbShowing = false;
				gotBadFbBufParams = true;
				return 0;
			}

			if (gotBadFbBufParams)
			{
				gotBadFbBufParams = false;
				log.info(string.Format("sceDisplaySetFrameBuf topaddr=0x{0:X8}, bufferwidth={1:D}, pixelformat={2:D}, syncType={3:D} ok", topaddr, bufferwidth, pixelformat, syncType));
			}

			if (topaddr == fb.TopAddr && bufferwidth == fb.BufferWidth && pixelformat == fb.PixelFormat && syncType == sync)
			{
				// No FB parameter changed, nothing to do...
				return 0;
			}

			if (topaddr != fb.TopAddr)
			{
				// New frame counting for FPS
				frameCount++;
			}

			// Keep track of how many frames have been skipped in sequence
			// (i.e. since the last rendering).
			if (skipNextFrameBufferSwitch)
			{
				framesSkippedInSequence++;
			}
			else
			{
				framesSkippedInSequence = 0;
			}

			bool skipThisFrame = false;
			if (desiredFps > 0)
			{
				// Remember the time stamps of the frames displayed during the last second.
				long currentFrameTimestamp = Emulator.Clock.currentTimeMillis();
				frameTimestamps.AddLast(currentFrameTimestamp);
				// Remove the time stamps older than 1 second
				while (currentFrameTimestamp - frameTimestamps.First.Value.longValue() > 1000L)
				{
					frameTimestamps.RemoveFirst();
				}

				// The current FPS is the number of frames displayed during the last second.
				int currentFps = frameTimestamps.Count;

				// Skip the rendering of the next frame if we are below the desired FPS
				// and if we have not already skipped too many frames since the last rendering.
				if (currentFps < desiredFps && framesSkippedInSequence < maxFramesSkippedInSequence)
				{
					skipThisFrame = true;
				}
			}
			VideoEngine.Instance.SkipThisFrame = skipThisFrame;

			if (skipNextFrameBufferSwitch)
			{
				// The rendering of the previous frame has been skipped.
				// Reuse the frame buffer of the current frame to avoid flickering.
				if (topaddr != fb.TopAddr)
				{
					Memory.Instance.memcpy(topaddr, fb.TopAddr, fb.BottomAddr - fb.TopAddr);
				}
				skipNextFrameBufferSwitch = false;
			}

			if (pixelformat != fb.PixelFormat || bufferwidth != fb.BufferWidth || makePow2(fb.Height) != makePow2(fb.Height))
			{
				createTex = true;
			}

			fb = new FrameBufferSettings(topaddr, bufferwidth, fb.Width, fb.Height, pixelformat);
			sync = syncType;

			texS = (float) fb.Width / (float) bufferwidth;
			texT = (float) fb.Height / (float) makePow2(fb.Height);
			displayScreen.update();

			detailsDirty = true;
			isFbShowing = true;

			if (State.captureGeNextFrame && CaptureManager.hasListExecuted())
			{
				State.captureGeNextFrame = false;
				CaptureManager.captureFrameBufDetails();
				CaptureManager.endCapture();
			}

			VideoEngine.Instance.hleSetFrameBuf(fb.TopAddr, fb.BufferWidth, fb.PixelFormat);

			return 0;
		}

		/// <param name="topaddrAddr"> - </param>
		/// <param name="bufferwidthAddr"> - </param>
		/// <param name="pixelformatAddr"> - </param>
		/// <param name="syncType"> - 0 or 1. All other value than 1 is interpreted as 0.
		/// @return </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0xEEDA2E54, version = 150) public int sceDisplayGetFrameBuf(pspsharp.HLE.TPointer32 topaddrAddr, @CanBeNull pspsharp.HLE.TPointer32 bufferwidthAddr, @CanBeNull pspsharp.HLE.TPointer32 pixelformatAddr, int syncType)
		[HLEFunction(nid : 0xEEDA2E54, version : 150)]
		public virtual int sceDisplayGetFrameBuf(TPointer32 topaddrAddr, TPointer32 bufferwidthAddr, TPointer32 pixelformatAddr, int syncType)
		{
			topaddrAddr.setValue(fb.TopAddr);
			bufferwidthAddr.setValue(fb.BufferWidth);
			pixelformatAddr.setValue(fb.PixelFormat);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceDisplayGetFrameBuf returning topaddr=0x{0:X8}, bufferwidth=0x{1:X}, pixelformat=0x{2:X}", fb.TopAddr, fb.BufferWidth, fb.PixelFormat));
			}
			return 0;
		}

		[HLEFunction(nid : 0xB4F378FA, version : 150)]
		public virtual bool sceDisplayIsForeground()
		{
			return isFbShowing;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEFunction(nid = 0x31C4BAA8, version = 150) public int sceDisplayGetBrightness(@CanBeNull pspsharp.HLE.TPointer32 levelAddr, @CanBeNull pspsharp.HLE.TPointer32 unknownAddr)
		[HLEFunction(nid : 0x31C4BAA8, version : 150)]
		public virtual int sceDisplayGetBrightness(TPointer32 levelAddr, TPointer32 unknownAddr)
		{
			levelAddr.setValue(Screen.BrightnessLevel);
			unknownAddr.setValue(0); // Always 0

			return 0;
		}

		[HLEFunction(nid : 0x9E3C6DC6, version : 150)]
		public virtual int sceDisplaySetBrightness(int level, int syncType)
		{
			if (level < 0 || level > 100)
			{
				return SceKernelErrors.ERROR_INVALID_ARGUMENT;
			}
			if (syncType != 0 && syncType != 1)
			{
				return SceKernelErrors.ERROR_INVALID_MODE;
			}

			Screen.BrightnessLevel = level;

			return 0;
		}

		[HLEFunction(nid : 0x9C6EAAD7, version : 150)]
		public virtual int sceDisplayGetVcount()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceDisplayGetVcount returning {0:D}", vcount));
			}
			// 60 units per second
			return vcount;
		}

		[HLEFunction(nid : 0x4D4E10EC, version : 150)]
		public virtual bool sceDisplayIsVblank()
		{
			bool isVblank = this.Vblank;

			//if (log.DebugEnabled)
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: Console.WriteLine(String.format("sceDisplayIsVblank returns %b", isVblank));
				Console.WriteLine(string.Format("sceDisplayIsVblank returns %b", isVblank));
			}

			return isVblank;
		}

		[HLEFunction(nid : 0x36CDFADE, version : 150, checkInsideInterrupt : true)]
		public virtual int sceDisplayWaitVblank()
		{
			if (!Vblank)
			{
				sceDisplayWaitVblankStart();
			}
			return 0;
		}

		[HLEFunction(nid : 0x8EB9EC49, version : 150, checkInsideInterrupt : true)]
		public virtual int sceDisplayWaitVblankCB()
		{
			if (!Vblank)
			{
				sceDisplayWaitVblankStartCB();
			}
			return 0;
		}

		[HLEFunction(nid : 0x984C27E7, version : 150, checkInsideInterrupt : true)]
		public virtual int sceDisplayWaitVblankStart()
		{
			return hleDisplayWaitVblankStart(1, false);
		}

		[HLEFunction(nid : 0x46F186C3, version : 150, checkInsideInterrupt : true)]
		public virtual int sceDisplayWaitVblankStartCB()
		{
			return hleDisplayWaitVblankStart(1, true);
		}

		[HLEFunction(nid : 0x773DD3A3, version : 150)]
		public virtual int sceDisplayGetCurrentHcount()
		{
			int currentHcount = CurrentHcount;

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceDisplayGetCurrentHcount returning {0:D}", currentHcount));
			}

			return currentHcount;
		}

		[HLEFunction(nid : 0x210EAB3A, version : 150)]
		public virtual int sceDisplayGetAccumulatedHcount()
		{
			// The accumulatedHcount is the currentHcount plus the sum of the Hcounts
			// from all the previous vblanks (vcount * number of Hcounts per Vblank).
			int currentHcount = CurrentHcount;
			int accumulatedHcount = currentHcount + (int)(vcount * hCountPerVblank);

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("sceDisplayGetAccumulatedHcount returning {0:D} (currentHcount={1:D})", accumulatedHcount, currentHcount));
			}

			return accumulatedHcount;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xA83EF139, version = 150) public int sceDisplayAdjustAccumulatedHcount()
		[HLEFunction(nid : 0xA83EF139, version : 150)]
		public virtual int sceDisplayAdjustAccumulatedHcount()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xBF79F646, version = 200) public int sceDisplayGetResumeMode()
		[HLEFunction(nid : 0xBF79F646, version : 200)]
		public virtual int sceDisplayGetResumeMode()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x69B53541, version = 200) public int sceDisplayGetVblankRest()
		[HLEFunction(nid : 0x69B53541, version : 200)]
		public virtual int sceDisplayGetVblankRest()
		{
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x21038913, version = 200) public int sceDisplayIsVsync()
		[HLEFunction(nid : 0x21038913, version : 200)]
		public virtual int sceDisplayIsVsync()
		{
			return 0;
		}

		/// <summary>
		/// Wait for Vblank start after multiple VSYNCs.
		/// </summary>
		/// <param name="cycleNum">  Number of VSYNCs to wait before blocking the thread on VBLANK. </param>
		/// <returns> 0 </returns>
		[HLEFunction(nid : 0x40F1469C, version : 500, checkInsideInterrupt : true)]
		public virtual int sceDisplayWaitVblankStartMulti(int cycleNum)
		{
			return hleDisplayWaitVblankStart(cycleNum, false);
		}

		/// <summary>
		/// Wait for Vblank start after multiple VSYNCs, with Callback execution.
		/// </summary>
		/// <param name="cycleNum">  Number of VSYNCs to wait before blocking the thread on VBLANK. </param>
		/// <returns> 0 </returns>
		[HLEFunction(nid : 0x77ED8B3A, version : 500, checkInsideInterrupt : true)]
		public virtual int sceDisplayWaitVblankStartMultiCB(int cycleNum)
		{
			return hleDisplayWaitVblankStart(cycleNum, true);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x996881D2, version = 660) public int sceDisplay_driver_996881D2()
		[HLEFunction(nid : 0x996881D2, version : 660)]
		public virtual int sceDisplay_driver_996881D2()
		{
			// Has no parameters
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0x9B18DDDD, version = 660) public int sceDisplay_driver_9B18DDDD(int unknown)
		[HLEFunction(nid : 0x9B18DDDD, version : 660)]
		public virtual int sceDisplay_driver_9B18DDDD(int unknown)
		{
			// This seems to be related to the registry entry "/CONFIG/DISPLAY/color_space_mode"
			return 0;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @HLEUnimplemented @HLEFunction(nid = 0xF455917F, version = 660) public int sceDisplay_driver_F455917F(int unknown)
		[HLEFunction(nid : 0xF455917F, version : 660)]
		public virtual int sceDisplay_driver_F455917F(int unknown)
		{
			// This seems to be related to the registry entry "/CONFIG/SYSTEM/POWER_SAVING/active_backlight_mode"
			return 0;
		}

		public class BufferInfo
		{

			public int topAddr;
			public int bottomAddr;
			public int width;
			public int height;
			public int bufferWidth;
			public int pixelFormat;

			public BufferInfo(int topAddr, int bottomAddr, int width, int height, int bufferWidth, int pixelFormat)
			{
				this.topAddr = topAddr;
				this.bottomAddr = bottomAddr;
				this.width = width;
				this.height = height;
				this.bufferWidth = bufferWidth;
				this.pixelFormat = pixelFormat;
			}
		}

		protected internal class DisplayScreen
		{
			private readonly sceDisplay outerInstance;

			internal float[] values;

			public DisplayScreen(sceDisplay outerInstance)
			{
				this.outerInstance = outerInstance;
				update();
			}

			public virtual void update()
			{
				int[] indices = Indices;
				if (indices == null)
				{
					return;
				}
				float[] baseValues = new float[] {0f, 0f, outerInstance.texS, 0f, 0f, outerInstance.texT, outerInstance.texS, outerInstance.texT};
				values = new float[baseValues.Length];
				for (int i = 0; i < values.Length; i++)
				{
					values[i] = baseValues[indices[i]];
				}
			}

			protected internal virtual int[] Indices
			{
				get
				{
					return new int[] {0, 1, 2, 3, 4, 5, 6, 7};
				}
			}

			protected internal virtual bool SwappedXY
			{
				get
				{
					return false;
				}
			}

			protected internal virtual int getWidth(int width, int height)
			{
				return SwappedXY ? height : width;
			}

			protected internal virtual int getHeight(int width, int height)
			{
				return SwappedXY ? width : height;
			}

			public virtual int Width
			{
				get
				{
					return getWidth(Screen.width, Screen.height);
				}
			}

			public virtual int Height
			{
				get
				{
					return getHeight(Screen.width, Screen.height);
				}
			}

			public virtual int getWidth(FrameBufferSettings fb)
			{
				return getWidth(fb.Width, fb.Height);
			}

			public virtual int getHeight(FrameBufferSettings fb)
			{
				return getHeight(fb.Width, fb.Height);
			}

			public virtual float TextureUpperLeftS
			{
				get
				{
					return values[0];
				}
			}

			public virtual float TextureUpperLeftT
			{
				get
				{
					return values[1];
				}
			}

			public virtual float TextureUpperRightS
			{
				get
				{
					return values[2];
				}
			}

			public virtual float TextureUpperRightT
			{
				get
				{
					return values[3];
				}
			}

			public virtual float TextureLowerLeftS
			{
				get
				{
					return values[4];
				}
			}

			public virtual float TextureLowerLeftT
			{
				get
				{
					return values[5];
				}
			}

			public virtual float TextureLowerRightS
			{
				get
				{
					return values[6];
				}
			}

			public virtual float TextureLowerRightT
			{
				get
				{
					return values[7];
				}
			}

			public override string ToString()
			{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("DisplayScreen [%f, %f, %f, %f, %f, %f, %f, %f, %b]", values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], isSwappedXY());
				return string.Format("DisplayScreen [%f, %f, %f, %f, %f, %f, %f, %f, %b]", values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], SwappedXY);
			}
		}

		protected internal class DisplayScreenRotation90 : DisplayScreen
		{
			private readonly sceDisplay outerInstance;

			public DisplayScreenRotation90(sceDisplay outerInstance) : base(outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override int[] Indices
			{
				get
				{
					return new int[] {4, 5, 0, 1, 6, 7, 2, 3};
				}
			}

			protected internal override bool SwappedXY
			{
				get
				{
					return true;
				}
			}
		}

		protected internal class DisplayScreenRotation180 : DisplayScreen
		{
			private readonly sceDisplay outerInstance;

			public DisplayScreenRotation180(sceDisplay outerInstance) : base(outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override int[] Indices
			{
				get
				{
					return new int[] {6, 7, 4, 5, 2, 3, 0, 1};
				}
			}
		}

		protected internal class DisplayScreenRotation270 : DisplayScreen
		{
			private readonly sceDisplay outerInstance;

			public DisplayScreenRotation270(sceDisplay outerInstance) : base(outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override int[] Indices
			{
				get
				{
					return new int[] {2, 3, 6, 7, 0, 1, 4, 5};
				}
			}

			protected internal override bool SwappedXY
			{
				get
				{
					return true;
				}
			}
		}

		protected internal class DisplayScreenMirrorX : DisplayScreen
		{
			private readonly sceDisplay outerInstance;

			internal DisplayScreen displayScreen;

			public DisplayScreenMirrorX(sceDisplay outerInstance, DisplayScreen displayScreen) : base(outerInstance)
			{
				this.outerInstance = outerInstance;
				this.displayScreen = displayScreen;
				update();
			}

			protected internal override int[] Indices
			{
				get
				{
					if (displayScreen == null)
					{
						return null;
					}
					int[] i = displayScreen.Indices;
					return new int[] {i[2], i[3], i[0], i[1], i[6], i[7], i[4], i[5]};
				}
			}

			protected internal override bool SwappedXY
			{
				get
				{
					return displayScreen.SwappedXY;
				}
			}
		}

		protected internal class DisplayScreenMirrorY : DisplayScreen
		{
			private readonly sceDisplay outerInstance;

			internal DisplayScreen displayScreen;

			public DisplayScreenMirrorY(sceDisplay outerInstance, DisplayScreen displayScreen) : base(outerInstance)
			{
				this.outerInstance = outerInstance;
				this.displayScreen = displayScreen;
			}

			protected internal override int[] Indices
			{
				get
				{
					int[] i = displayScreen.Indices;
					return new int[] {i[4], i[5], i[6], i[7], i[0], i[1], i[2], i[3]};
				}
			}

			protected internal override bool SwappedXY
			{
				get
				{
					return displayScreen.SwappedXY;
				}
			}
		}

		private class ScreenshotAction : IAction
		{
			private readonly sceDisplay outerInstance;

			public ScreenshotAction(sceDisplay outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void execute()
			{
				outerInstance.saveScreen();
			}
		}

		private class CopyGeToMemoryAction : IAction
		{
			private readonly sceDisplay outerInstance;

			internal int geTopAddress;

			public CopyGeToMemoryAction(sceDisplay outerInstance, int geTopAddress)
			{
				this.outerInstance = outerInstance;
				this.geTopAddress = geTopAddress;
			}

			public virtual void execute()
			{
				outerInstance.copyGeToMemory(geTopAddress, true, true);
				outerInstance.doneCopyGeToMemory = true;
			}
		}
	}

}