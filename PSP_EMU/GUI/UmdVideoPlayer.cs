using System;
using System.Collections.Generic;
using System.Text;
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
namespace pspsharp.GUI
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceAudiocodec.PSP_CODEC_AT3PLUS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_CIRCLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_CROSS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_DOWN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_LEFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_LTRIGGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_RIGHT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_RTRIGGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_TRIANGLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceCtrl.PSP_CTRL_UP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceMpeg.UNKNOWN_TIMESTAMP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceMpeg.mpegTimestampPerSecond;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceUtility.PSP_SYSTEMPARAM_BUTTON_CROSS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceUtility.getSystemParamButtonPreference;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.psmf.PsmfAudioDemuxVirtualFile.PACK_START_CODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.psmf.PsmfAudioDemuxVirtualFile.PADDING_STREAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.psmf.PsmfAudioDemuxVirtualFile.PRIVATE_STREAM_1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.psmf.PsmfAudioDemuxVirtualFile.PRIVATE_STREAM_2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.format.psmf.PsmfAudioDemuxVirtualFile.SYSTEM_HEADER_START_CODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.endianSwap16;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.endianSwap32;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.sleep;



	//using Logger = org.apache.log4j.Logger;

	using UmdIsoFile = pspsharp.filesystems.umdiso.UmdIsoFile;
	using UmdIsoReader = pspsharp.filesystems.umdiso.UmdIsoReader;
	using RCO = pspsharp.format.RCO;
	using PesHeader = pspsharp.format.psmf.PesHeader;
	using Display = pspsharp.format.rco.Display;
	using RCOState = pspsharp.format.rco.RCOState;
	using MoviePlayer = pspsharp.format.rco.vsmx.objects.MoviePlayer;
	using Screen = pspsharp.hardware.Screen;
	using CodecFactory = pspsharp.media.codec.CodecFactory;
	using ICodec = pspsharp.media.codec.ICodec;
	using IVideoCodec = pspsharp.media.codec.IVideoCodec;
	using H264Utils = pspsharp.media.codec.h264.H264Utils;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using IMemoryWriter = pspsharp.memory.IMemoryWriter;
	using MemoryReader = pspsharp.memory.MemoryReader;
	using MemoryWriter = pspsharp.memory.MemoryWriter;
	using Utilities = pspsharp.util.Utilities;
	using Modules = pspsharp.HLE.Modules;
	using sceMpeg = pspsharp.HLE.modules.sceMpeg;
	using sceUtility = pspsharp.HLE.modules.sceUtility;

	using GetBitContext = com.twilight.h264.decoder.GetBitContext;
	using H264Context = com.twilight.h264.decoder.H264Context;

	public class UmdVideoPlayer : KeyListener
	{
		private static Logger log = Logger.getLogger("videoplayer");
		private const bool dumpFrames = false;

		// ISO file
		private string fileName;
		private UmdIsoReader iso;
		private UmdIsoFile isoFile;

		// Stream storage
		private IList<MpsStreamInfo> mpsStreams;
		private int currentStreamIndex;

		// Display
		private JLabel display;
		private Display rcoDisplay;
		private int screenWidth;
		private int screenHeigth;
		private Image image;
		private int resizeScaleFactor;
		private MainGUI gui;

		// Video
		private IVideoCodec videoCodec;
		private bool videoCodecInit;
		private int[] videoData = new int[0x10000];
		private int videoDataOffset;
		private int videoChannel = 0;
		public static int frame;
		private int videoWidth;
		private int videoHeight;
		private int videoAspectRatioNum;
		private int videoAspectRatioDen;
		private int[] luma;
		private int[] cr;
		private int[] cb;
		private int[] abgr;
		private bool foundFrameStart;
		private int parseState;
		private int[] parseHistory = new int[6];
		private int parseHistoryCount;
		private int parseLastMb;
		private int nalLengthSize = 0;
		private bool isAvc = false;
		private int lastParsePosition;
		// Audio
		private ICodec audioCodec;
		private bool audioCodecInitialized;
		private int[] audioData = new int[0x10000];
		private int audioDataOffset;
		private int audioFrameLength;
		private int audioChannels;
		private readonly int[] frameHeader = new int[8];
		private int frameHeaderLength;
		private int audioChannel = 0;
		private int samplesAddr = MemoryMap.START_USERSPACE;
		private int audioBufferAddr = MemoryMap.START_USERSPACE + 0x10000;
		private sbyte[] audioBytes;
		// Time synchronization
		private PesHeader pesHeaderAudio;
		private PesHeader pesHeaderVideo;
		private long currentVideoTimestamp;
		private int currentChapterNumber;
		private long startTime;
		private int fastForwardSpeed;
		private int fastRewindSpeed;
		private static readonly int[] fastForwardSpeeds = new int[] {1, 50, 100, 200, 400};
		private static readonly int[] fastRewindSpeeds = new int[] {1, 50, 100, 200, 400};

		// State (for sync thread).
		private volatile bool videoPaused;
		private volatile bool done;
		private volatile bool endOfVideo;
		private volatile bool threadExit;

		// Internal data
		private MpsDisplayThread displayThread;
		private SourceDataLine mLine;

		// RCO MoviePlayer
		private MoviePlayer moviePlayer;
		private RCOState rcoState;
		private DisplayControllerThread displayControllerThread;

		// MPS stream class.
		protected internal class MpsStreamInfo
		{
			private readonly UmdVideoPlayer outerInstance;

			internal string streamName;
			internal int streamWidth;
			internal int streamHeigth;
			internal int streamFirstTimestamp;
			internal int streamLastTimestamp;
			internal MpsStreamMarkerInfo[] streamMarkers;
			internal int playListNumber;

			public MpsStreamInfo(UmdVideoPlayer outerInstance, string name, int width, int heigth, int firstTimestamp, int lastTimestamp, MpsStreamMarkerInfo[] markers, int playListNumber)
			{
				this.outerInstance = outerInstance;
				streamName = name;
				streamWidth = width;
				streamHeigth = heigth;
				streamFirstTimestamp = firstTimestamp;
				streamLastTimestamp = lastTimestamp;
				streamMarkers = markers;
				this.playListNumber = playListNumber;
			}

			public virtual string Name
			{
				get
				{
					return streamName;
				}
			}

			public virtual int Width
			{
				get
				{
					return streamWidth;
				}
			}

			public virtual int Heigth
			{
				get
				{
					return streamHeigth;
				}
			}

			public virtual int FirstTimestamp
			{
				get
				{
					return streamFirstTimestamp;
				}
			}

			public virtual int LastTimestamp
			{
				get
				{
					return streamLastTimestamp;
				}
			}

			public virtual MpsStreamMarkerInfo[] Markers
			{
				get
				{
					return streamMarkers;
				}
			}

			public virtual int PlayListNumber
			{
				get
				{
					return playListNumber;
				}
			}

			public virtual int getChapterNumber(long timestamp)
			{
				int marker = -1;
				if (streamMarkers != null)
				{
					for (int i = 0; i < streamMarkers.Length; i++)
					{
						if (streamMarkers[i].Timestamp <= timestamp)
						{
							marker = i;
						}
						else
						{
							break;
						}
					}
				}

				return marker;
			}

			public override string ToString()
			{
				StringBuilder s = new StringBuilder();
				s.Append(string.Format("name='{0}', {1:D}x{2:D}, {3}({4:D} to {5:D}), markers=[", Name, Width, Heigth, getTimestampString(LastTimestamp - FirstTimestamp), FirstTimestamp, LastTimestamp));
				for (int i = 0; i < streamMarkers.Length; i++)
				{
					if (i > 0)
					{
						s.Append(", ");
					}
					s.Append(streamMarkers[i]);
				}
				s.Append("]");

				return s.ToString();
			}
		}

		// MPS stream's marker class.
		protected internal class MpsStreamMarkerInfo
		{
			private readonly UmdVideoPlayer outerInstance;

			internal string streamMarkerName;
			internal long streamMarkerTimestamp;

			public MpsStreamMarkerInfo(UmdVideoPlayer outerInstance, string name, long timestamp)
			{
				this.outerInstance = outerInstance;
				streamMarkerName = name;
				streamMarkerTimestamp = timestamp;
			}

			public virtual string Name
			{
				get
				{
					return streamMarkerName;
				}
			}

			public virtual long Timestamp
			{
				get
				{
					return streamMarkerTimestamp;
				}
			}

			public override string ToString()
			{
				return string.Format("'{0}' {1}(timeStamp={2:D})", Name, getTimestampString(Timestamp), Timestamp);
			}
		}

		private static string getTimestampString(long timestamp)
		{
			int seconds = (int)(timestamp / mpegTimestampPerSecond);
			int hundredth = (int)(timestamp - ((long) seconds) * mpegTimestampPerSecond);
			hundredth = 100 * hundredth / mpegTimestampPerSecond;
			int minutes = seconds / 60;
			seconds -= minutes * 60;
			int hours = minutes / 60;
			minutes -= hours * 60;

			return string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D2}", hours, minutes, seconds, hundredth);
		}

		public UmdVideoPlayer(MainGUI gui, UmdIsoReader iso)
		{
			this.iso = iso;

			display = new JLabel();
			rcoDisplay = new Display();
			JPanel panel = new JPanel();
			panel.Layout = new OverlayLayout(panel);
			panel.add(rcoDisplay);
			panel.add(display);
			gui.remove(Modules.sceDisplayModule.Canvas);
			gui.ContentPane.add(panel, BorderLayout.CENTER);
			gui.addKeyListener(this);
			setVideoPlayerResizeScaleFactor(gui, 1);

			init();
		}

		public virtual void exit()
		{
			stopDisplayThread();
		}

		public override void keyPressed(KeyEvent @event)
		{
			State.controller.keyPressed(@event);

			if (moviePlayer != null)
			{
				if ((State.controller.Buttons & PSP_CTRL_UP) != 0)
				{
					moviePlayer.onUp();
				}
				if ((State.controller.Buttons & PSP_CTRL_DOWN) != 0)
				{
					moviePlayer.onDown();
				}
				if ((State.controller.Buttons & PSP_CTRL_LEFT) != 0)
				{
					moviePlayer.onLeft();
				}
				if ((State.controller.Buttons & PSP_CTRL_RIGHT) != 0)
				{
					moviePlayer.onRight();
				}
				int pushButton = SystemParamButtonPreference == PSP_SYSTEMPARAM_BUTTON_CROSS ? PSP_CTRL_CROSS : PSP_CTRL_CIRCLE;
				if ((State.controller.Buttons & pushButton) != 0)
				{
					moviePlayer.onPush();
				}

				// TODO Non-standard key mappings...
				if ((State.controller.Buttons & PSP_CTRL_RTRIGGER) != 0)
				{
					fastForward();
				}
				if ((State.controller.Buttons & PSP_CTRL_LTRIGGER) != 0)
				{
					rewind();
				}
				if ((State.controller.Buttons & PSP_CTRL_TRIANGLE) != 0)
				{
					resumeVideo();
				}
			}
			else
			{
				if (@event.KeyCode == KeyEvent.VK_RIGHT)
				{
					stopDisplayThread();
					goToNextMpsStream();
				}
				else if (@event.KeyCode == KeyEvent.VK_LEFT && currentStreamIndex > 0)
				{
					stopDisplayThread();
					goToPreviousMpsStream();
				}
				else if (@event.KeyCode == KeyEvent.VK_W && !videoPaused)
				{
					pauseVideo();
				}
				else if (@event.KeyCode == KeyEvent.VK_S)
				{
					resumeVideo();
				}
				else if (@event.KeyCode == KeyEvent.VK_A)
				{
					rewind();
				}
				else if (@event.KeyCode == KeyEvent.VK_D)
				{
					fastForward();
				}
				else if (@event.KeyCode == KeyEvent.VK_UP)
				{
					if (moviePlayer != null)
					{
						moviePlayer.onUp();
					}
				}
				else if (@event.KeyCode == KeyEvent.VK_DOWN)
				{
					if (moviePlayer != null)
					{
						moviePlayer.onDown();
					}
				}
			}
		}

		public override void keyReleased(KeyEvent @event)
		{
			State.controller.keyReleased(@event);
		}

		public override void keyTyped(KeyEvent keyCode)
		{
		}

		private void init()
		{
			Emulator.Scheduler.reset();
			Emulator.Clock.resume();

			displayControllerThread = new DisplayControllerThread(this);
			displayControllerThread.Name = "Display Controller Thread";
			displayControllerThread.Daemon = true;
			displayControllerThread.Start();

			done = false;
			threadExit = false;
			isoFile = null;
			mpsStreams = new LinkedList<UmdVideoPlayer.MpsStreamInfo>();
			pauseVideo();
			currentStreamIndex = 0;
			parsePlaylistFile();
			parseRCO();

			if (videoPaused)
			{
				goToMpsStream(currentStreamIndex);
			}
		}

		public virtual void setVideoPlayerResizeScaleFactor(MainGUI gui, int resizeScaleFactor)
		{
			this.resizeScaleFactor = resizeScaleFactor;
			this.gui = gui;

			resizeVideoPlayer();
		}

		private void resizeVideoPlayer()
		{
			if (videoWidth <= 0 || videoHeight <= 0)
			{
				screenWidth = Screen.width * resizeScaleFactor;
				screenHeigth = Screen.height * resizeScaleFactor;
			}
			else
			{
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("video size {0:D}x{1:D} resizeScaleFactor={2:D}", videoWidth, videoHeight, resizeScaleFactor));
				}
				screenWidth = videoWidth * videoAspectRatioNum / videoAspectRatioDen * resizeScaleFactor;
				screenHeigth = videoHeight * resizeScaleFactor;
			}

			gui.setDisplayMinimumSize(screenWidth, screenHeigth);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int readByteHexTo10(pspsharp.filesystems.umdiso.UmdIsoFile file) throws java.io.IOException
		private int readByteHexTo10(UmdIsoFile file)
		{
			int hex = file.readByte() & 0xFF;
			return (hex >> 4) * 10 + (hex & 0x0F);
		}

		private void parsePlaylistFile()
		{
			try
			{
				UmdIsoFile file = iso.getFile("UMD_VIDEO/PLAYLIST.UMD");
				int umdvMagic = file.readInt();
				int umdvVersion = file.readInt();
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Magic 0x{0:X8},  version 0x{1:X8}", umdvMagic, umdvVersion));
				}
				int globalDataOffset = endianSwap32(file.readInt());
				file.seek(globalDataOffset);
				int playListSize = endianSwap32(file.readInt());
				int playListTracksNum = endianSwap16(file.readShort());
				file.skipBytes(2); // NULL.
				if (umdvMagic != 0x56444D55)
				{ // UMDV
					Console.WriteLine("Accessing invalid PLAYLIST.UMD file!");
				}
				else
				{
					log.info(string.Format("Accessing valid PLAYLIST.UMD file: playListSize={0:D}, playListTracksNum={1:D}", playListSize, playListTracksNum));
				}
				for (int i = 0; i < playListTracksNum; i++)
				{
					file.skipBytes(2); // 0x035C.
					file.skipBytes(2); // 0x0310.
					file.skipBytes(2); // 0x0332.
					file.skipBytes(30); // NULL.
					file.skipBytes(2); // 0x02E8.
					int unknown = endianSwap16(file.readShort());
					int releaseDateYear = readByteHexTo10(file) * 100 + readByteHexTo10(file);
					int releaseDateDay = file.readByte();
					int releaseDateMonth = file.readByte();
					file.skipBytes(4); // NULL.
					file.skipBytes(4); // Unknown (found 0x00000900).
					int nameLength = file.readByte() & 0xFF;
					sbyte[] nameBuffer = new sbyte[nameLength];
					file.read(nameBuffer);
					string name = StringHelper.NewString(nameBuffer);
					file.skipBytes(732 - nameLength); // Unknown NULL area with size 0x2DC.
					int streamHeight = (int)(file.readByte() * 0x10); // Stream's original height.
					file.skipBytes(2); // NULL.
					file.skipBytes(4); // 0x00010000.
					file.skipBytes(1); // NULL.
					int streamWidth = (int)(file.readByte() * 0x10); // Stream's original width.
					file.skipBytes(1); // NULL.
					int streamNameCharsNum = (int) file.readByte(); // Stream's name non null characters count.
					sbyte[] stringBuf = new sbyte[streamNameCharsNum];
					file.read(stringBuf);
					string streamName = StringHelper.NewString(stringBuf);
					file.skipBytes(8 - streamNameCharsNum); // NULL chars.
					file.skipBytes(2); // NULL.
					int streamFirstTimestamp = endianSwap32(file.readInt());
					file.skipBytes(2); // NULL.
					int streamLastTimestamp = endianSwap32(file.readInt());
					file.skipBytes(2); // NULL.
					int streamMarkerDataLength = endianSwap16(file.readShort()); // Stream's markers' data Length.
					MpsStreamMarkerInfo[] streamMarkers;
					if (streamMarkerDataLength > 0)
					{
						int streamMarkersNum = endianSwap16(file.readShort()); // Stream's number of markers.
						streamMarkers = new MpsStreamMarkerInfo[streamMarkersNum];
						for (int j = 0; j < streamMarkersNum; j++)
						{
							file.skipBytes(1); // 0x05.
							int streamMarkerCharsNum = (int) file.readByte(); // Marker name Length.
							file.skipBytes(4); // NULL.
							long streamMarkerTimestamp = endianSwap32(file.readInt()) & 0xFFFFFFFFL;
							file.skipBytes(2); // NULL.
							file.skipBytes(4); // NULL.
							sbyte[] markerBuf = new sbyte[streamMarkerCharsNum];
							file.read(markerBuf);
							string markerName = StringHelper.NewString(markerBuf);
							file.skipBytes(24 - streamMarkerCharsNum);
							streamMarkers[j] = new MpsStreamMarkerInfo(this, markerName, streamMarkerTimestamp);
						}
						file.skip(2); // NULL
					}
					else
					{
						streamMarkers = new MpsStreamMarkerInfo[0];
					}
					// Map this stream.
					MpsStreamInfo info = new MpsStreamInfo(this, streamName, streamWidth, streamHeight, streamFirstTimestamp, streamLastTimestamp, streamMarkers, i + 1);
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Release date {0:D}-{1:D}-{2:D}, name '{3}', unknown=0x{4:X4}", releaseDateYear, releaseDateMonth, releaseDateDay, name, unknown));
						Console.WriteLine(string.Format("StreamInfo #{0:D}: {1}", i, info));
					}
					mpsStreams.Add(info);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("parsePlaylistFile", e);
			}
		}

		private void parseRCO()
		{
			try
			{
				string[] resources = iso.listDirectory("UMD_VIDEO/RESOURCE");
				if (resources == null || resources.Length <= 0)
				{
					return;
				}

				int preferredLanguage = sceUtility.SystemParamLanguage;
				string languagePrefix = "EN";
				switch (preferredLanguage)
				{
					case sceUtility.PSP_SYSTEMPARAM_LANGUAGE_JAPANESE:
						languagePrefix = "JA";
						break;
					case sceUtility.PSP_SYSTEMPARAM_LANGUAGE_ENGLISH:
						languagePrefix = "EN";
						break;
					case sceUtility.PSP_SYSTEMPARAM_LANGUAGE_FRENCH:
						languagePrefix = "FR";
						break;
					case sceUtility.PSP_SYSTEMPARAM_LANGUAGE_SPANISH:
						languagePrefix = "ES";
						break;
					case sceUtility.PSP_SYSTEMPARAM_LANGUAGE_GERMAN:
						languagePrefix = "DE";
						break;
					case sceUtility.PSP_SYSTEMPARAM_LANGUAGE_ITALIAN:
						languagePrefix = "IT";
						break;
					case sceUtility.PSP_SYSTEMPARAM_LANGUAGE_DUTCH:
						languagePrefix = "NL";
						break;
					case sceUtility.PSP_SYSTEMPARAM_LANGUAGE_PORTUGUESE:
						languagePrefix = "PO";
						break;
					case sceUtility.PSP_SYSTEMPARAM_LANGUAGE_RUSSIAN:
						languagePrefix = "RU";
						break;
					case sceUtility.PSP_SYSTEMPARAM_LANGUAGE_KOREAN:
						languagePrefix = "KO";
						break;
					case sceUtility.PSP_SYSTEMPARAM_LANGUAGE_CHINESE_TRADITIONAL:
						languagePrefix = "CN";
						break;
					case sceUtility.PSP_SYSTEMPARAM_LANGUAGE_CHINESE_SIMPLIFIED:
						languagePrefix = "CN";
						break;
				}

				// The resource names are tried in this order:
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String resourceNames[] = new String[] { "100000", "000000", "110000", "010000" };
				string[] resourceNames = new string[] {"100000", "000000", "110000", "010000"};
				string resourceFileName = null;
				foreach (string resourceName in resourceNames)
				{
					string fileName = languagePrefix + resourceName + ".RCO";
					if (iso.hasFile("UMD_VIDEO/RESOURCE/" + fileName))
					{
						resourceFileName = fileName;
						break;
					}
				}

				if (!string.ReferenceEquals(resourceFileName, null))
				{
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("Reading RCO file '{0}'", resourceFileName));
					}
					UmdIsoFile file = iso.getFile("UMD_VIDEO/RESOURCE/" + resourceFileName);
					sbyte[] buffer = new sbyte[(int) file.Length()];
					file.read(buffer);
					RCO rco = new RCO(buffer);
					//if (log.DebugEnabled)
					{
						Console.WriteLine(string.Format("RCO: {0}", rco));
					}

					rcoState = rco.execute(this, resourceFileName.Replace(".RCO", ""));
				}
			}
			catch (FileNotFoundException)
			{
			}
			catch (IOException e)
			{
				Console.WriteLine("parse RCO", e);
			}

		}

		public virtual void changeResource(string resourceName)
		{
			try
			{
				UmdIsoFile file = iso.getFile(string.Format("UMD_VIDEO/RESOURCE/{0}.RCO", resourceName));
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("Reading RCO file '{0}.RCO'", resourceName));
				}
				sbyte[] buffer = new sbyte[(int) file.Length()];
				file.read(buffer);
				RCO rco = new RCO(buffer);
				//if (log.DebugEnabled)
				{
					Console.WriteLine(string.Format("RCO: {0}", rco));
				}

				RCODisplay.changeResource();
				rcoState = rco.execute(rcoState, this, resourceName);
			}
			catch (FileNotFoundException)
			{
			}
			catch (IOException e)
			{
				Console.WriteLine("changeResource", e);
			}
		}

		private int getStreamIndexFromPlayListNumber(int playListNumber)
		{
			for (int i = 0; i < mpsStreams.Count; i++)
			{
				MpsStreamInfo info = mpsStreams[i];
				if (info.PlayListNumber == playListNumber)
				{
					return i;
				}
			}

			return playListNumber;
		}

		public virtual MoviePlayer MoviePlayer
		{
			set
			{
				this.moviePlayer = value;
			}
		}

		public virtual void play(int playListNumber, int chapterNumber, int videoNumber, int audioNumber, int audioFlag, int subtitleNumber, int subtitleFlag)
		{
			done = false;
			int streamIndex = getStreamIndexFromPlayListNumber(playListNumber);
			goToMpsStream(streamIndex);
		}

		private bool goToMpsStream(int streamIndex)
		{
			if (streamIndex < 0 || streamIndex >= mpsStreams.Count)
			{
				return false;
			}

			currentStreamIndex = streamIndex;
			MpsStreamInfo info = mpsStreams[currentStreamIndex];
			fileName = "UMD_VIDEO/STREAM/" + info.Name + ".MPS";
			log.info("Loading stream: " + fileName);
			try
			{
				isoFile = iso.getFile(fileName);
				string cpiFileName = "UMD_VIDEO/CLIPINF/" + info.Name + ".CLP";
				UmdIsoFile cpiFile = iso.getFile(cpiFileName);
				if (cpiFile != null)
				{
					log.info("Found CLIPINF data for this stream: " + cpiFileName);
				}
			}
			catch (FileNotFoundException)
			{
			}
			catch (IOException e)
			{
				Emulator.Console.WriteLine(e);
			}

			if (isoFile != null)
			{
				startVideo();
				initVideo();
			}

			return true;
		}

		private bool goToNextMpsStream()
		{
			return goToMpsStream(currentStreamIndex + 1);
		}

		private bool goToPreviousMpsStream()
		{
			return goToMpsStream(currentStreamIndex - 1);
		}

		public virtual void initVideo()
		{
			done = false;
			videoPaused = false;
			if (displayThread == null)
			{
				displayThread = new MpsDisplayThread(this);
				displayThread.Daemon = true;
				displayThread.Name = "UMD Video Player Thread";
				displayThread.Start();
			}
		}

		public virtual void pauseVideo()
		{
			videoPaused = true;
		}

		public virtual void resumeVideo()
		{
			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Resume video"));
			}
			videoPaused = false;
			fastForwardSpeed = 0;
			fastRewindSpeed = 0;
		}

		public virtual void fastForward()
		{
			if (fastRewindSpeed > 0)
			{
				fastRewindSpeed--;
			}
			else
			{
				fastForwardSpeed = System.Math.Min(fastForwardSpeeds.Length - 1, fastForwardSpeed + 1);
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Fast forward {0:D}, fast rewind {1:D}", fastForwardSpeed, fastRewindSpeed));
			}
		}

		public virtual void rewind()
		{
			if (fastForwardSpeed > 0)
			{
				fastForwardSpeed--;
			}
			else
			{
				fastRewindSpeed = System.Math.Min(fastRewindSpeeds.Length - 1, fastRewindSpeed + 1);
			}

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("Fast forward {0:D}, fast rewind {1:D}", fastForwardSpeed, fastRewindSpeed));
			}
		}

		private int read8()
		{
			try
			{
				return isoFile.read();
			}
			catch (IOException)
			{
				// Ignore exception
			}

			return -1;
		}

		private int read16()
		{
			return (read8() << 8) | read8();
		}

		private int read32()
		{
			return (read8() << 24) | (read8() << 16) | (read8() << 8) | read8();
		}

		private void skip(int n)
		{
			if (n > 0)
			{
				try
				{
					isoFile.skip(n);
				}
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

		private long readPts(int c)
		{
			return (((long)(c & 0x0E)) << 29) | ((read16() >> 1) << 15) | (read16() >> 1);
		}

		private long readPts()
		{
			return readPts(read8());
		}

		private int readPesHeader(int startCode, PesHeader pesHeader)
		{
			int pesLength = 0;
			int c = read8();
			pesLength++;
			while (c == 0xFF)
			{
				c = read8();
				pesLength++;
			}

			if ((c & 0xC0) == 0x40)
			{
				skip(1);
				c = read8();
				pesLength += 2;
			}

			pesHeader.DtsPts = UNKNOWN_TIMESTAMP;
			if ((c & 0xE0) == 0x20)
			{
				pesHeader.DtsPts = readPts(c);
				pesLength += 4;
				if ((c & 0x10) != 0)
				{
					pesHeader.Pts = readPts();
					pesLength += 5;
				}
			}
			else if ((c & 0xC0) == 0x80)
			{
				int flags = read8();
				int headerLength = read8();
				pesLength += 2;
				pesLength += headerLength;
				if ((flags & 0x80) != 0)
				{
					pesHeader.DtsPts = readPts();
					headerLength -= 5;
					if ((flags & 0x40) != 0)
					{
						pesHeader.Dts = readPts();
						headerLength -= 5;
					}
				}
				if ((flags & 0x3F) != 0 && headerLength == 0)
				{
					flags &= 0xC0;
				}
				if ((flags & 0x01) != 0)
				{
					int pesExt = read8();
					headerLength--;
					int skip = (pesExt >> 4) & 0x0B;
					skip += skip & 0x09;
					if ((pesExt & 0x40) != 0 || skip > headerLength)
					{
						pesExt = skip = 0;
					}
					this.skip(skip);
					headerLength -= skip;
					if ((pesExt & 0x01) != 0)
					{
						int ext2Length = read8();
						headerLength--;
						 if ((ext2Length & 0x7F) != 0)
						 {
							 int idExt = read8();
							 headerLength--;
							 if ((idExt & 0x80) == 0)
							 {
								 startCode = ((startCode & 0xFF) << 8) | idExt;
							 }
						 }
					}
				}
				skip(headerLength);
			}

			if (startCode == 0x1BD)
			{ // PRIVATE_STREAM_1
				int channel = read8();
				pesHeader.Channel = channel;
				pesLength++;
				if (channel >= 0x80 && channel <= 0xCF)
				{
					skip(3);
					pesLength += 3;
					if (channel >= 0xB0 && channel <= 0xBF)
					{
						skip(1);
						pesLength++;
					}
				}
				else
				{
					skip(3);
					pesLength += 3;
				}
			}

			return pesLength;
		}

		private sbyte[] resize(sbyte[] array, int size)
		{
			if (array == null)
			{
				return new sbyte[size];
			}

			if (size <= array.Length)
			{
				return array;
			}

			sbyte[] newArray = new sbyte[size];
			Array.Copy(array, 0, newArray, 0, array.Length);

			return newArray;
		}

		private int[] resize(int[] array, int size)
		{
			if (array == null)
			{
				return new int[size];
			}

			if (size <= array.Length)
			{
				return array;
			}

			int[] newArray = new int[size];
			Array.Copy(array, 0, newArray, 0, array.Length);

			return newArray;
		}

		private void addVideoData(int Length, long position)
		{
			videoData = resize(videoData, videoDataOffset + Length);

			for (int i = 0; i < Length; i++)
			{
				videoData[videoDataOffset++] = read8();
			}
		}

		private void addAudioData(int Length)
		{
			audioData = resize(audioData, audioDataOffset + Length);

			while (Length > 0)
			{
				int currentFrameLength = audioFrameLength == 0 ? 0 : audioDataOffset % audioFrameLength;
				if (currentFrameLength == 0)
				{
					// 8 bytes header:
					// - byte 0: 0x0F
					// - byte 1: 0xD0
					// - byte 2: 0x28
					// - byte 3: (frameLength - 8) / 8
					// - bytes 4-7: 0x00
					while (frameHeaderLength < frameHeader.Length && Length > 0)
					{
						frameHeader[frameHeaderLength++] = read8();
						Length--;
					}
					if (frameHeaderLength < frameHeader.Length)
					{
						// Frame header not yet complete
						break;
					}
					if (Length == 0)
					{
						// Frame header is complete but no data is following the header.
						// Retry when some data is available
						break;
					}

					int frameHeader23 = (frameHeader[2] << 8) | frameHeader[3];
					audioFrameLength = ((frameHeader23 & 0x3FF) << 3) + 8;
					if (frameHeader[0] != 0x0F || frameHeader[1] != 0xD0)
					{
						if (log.InfoEnabled)
						{
							Console.WriteLine(string.Format("Audio frame Length 0x{0:X} with incorrect header (header: {1:X2} {2:X2} {3:X2} {4:X2} {5:X2} {6:X2} {7:X2} {8:X2})", audioFrameLength, frameHeader[0], frameHeader[1], frameHeader[2], frameHeader[3], frameHeader[4], frameHeader[5], frameHeader[6], frameHeader[7]));
						}
					}
					else if (log.TraceEnabled)
					{
						log.trace(string.Format("Audio frame Length 0x{0:X} (header: {1:X2} {2:X2} {3:X2} {4:X2} {5:X2} {6:X2} {7:X2} {8:X2})", audioFrameLength, frameHeader[0], frameHeader[1], frameHeader[2], frameHeader[3], frameHeader[4], frameHeader[5], frameHeader[6], frameHeader[7]));
					}

					frameHeaderLength = 0;
				}
				int lengthToNextFrame = audioFrameLength - currentFrameLength;
				int readLength = Utilities.min(Length, lengthToNextFrame);
				for (int i = 0; i < readLength; i++)
				{
					audioData[audioDataOffset++] = read8();
				}
				Length -= readLength;
			}
		}

		private long CurrentFilePosition
		{
			get
			{
				try
				{
					return isoFile.FilePointer;
				}
				catch (IOException)
				{
				}
    
				return -1L;
			}
		}

		private bool readPsmfPacket(int videoChannel, int audioChannel)
		{
			while (!done)
			{
				int startCode = read32();
				if (startCode == -1)
				{
					// End of file
					break;
				}

				int codeLength, pesLength;
				switch (startCode)
				{
					case PACK_START_CODE:
						skip(10);
						break;
					case SYSTEM_HEADER_START_CODE:
						skip(14);
						break;
					case PADDING_STREAM:
					case PRIVATE_STREAM_2:
						codeLength = read16();
						skip(codeLength);
						break;
					case PRIVATE_STREAM_1: // Audio stream
						codeLength = read16();
						pesLength = readPesHeader(startCode, pesHeaderAudio);
						codeLength -= pesLength;
						if (pesHeaderAudio.Channel == audioChannel || audioChannel < 0)
						{
							addAudioData(codeLength);
							return true;
						}
						skip(codeLength);
						break;
					case 0x1E0:
				case 0x1E1:
			case 0x1E2:
		case 0x1E3: // Video streams
					case 0x1E4:
				case 0x1E5:
			case 0x1E6:
		case 0x1E7:
					case 0x1E8:
				case 0x1E9:
			case 0x1EA:
		case 0x1EB:
					case 0x1EC:
				case 0x1ED:
			case 0x1EE:
		case 0x1EF:
						codeLength = read16();
						if (videoChannel < 0 || startCode - 0x1E0 == videoChannel)
						{
							pesLength = readPesHeader(startCode, pesHeaderVideo);
							codeLength -= pesLength;
							addVideoData(codeLength, CurrentFilePosition);
							return true;
						}
						skip(codeLength);
						break;
				}
			}

			return false;
		}

		private void consumeVideoData(int Length)
		{
			if (Length >= videoDataOffset)
			{
				videoDataOffset = 0;
				lastParsePosition = 0;
			}
			else
			{
				Array.Copy(videoData, Length, videoData, 0, videoDataOffset - Length);
				videoDataOffset -= Length;
				lastParsePosition -= Length;
			}
		}

		private void consumeAudioData(int Length)
		{
			if (Length >= audioDataOffset)
			{
				audioDataOffset = 0;
			}
			else
			{
				Array.Copy(audioData, Length, audioData, 0, audioDataOffset - Length);
				audioDataOffset -= Length;
			}
		}

		private int startCodeFindCandidate(int offset, int size)
		{
			for (int i = 0; i < size; i++)
			{
				if (videoData[offset + i] == 0x00)
				{
					return i;
				}
			}

			return size;
		}

		private int findVideoFrameEnd()
		{
			if (parseState > 13)
			{
				parseState = 7;
			}

			if (lastParsePosition < 0)
			{
				lastParsePosition = 0;
			}

			int nextAvc = isAvc ? 0 : videoDataOffset;
			int found = -1;
			for (int i = lastParsePosition; i < videoDataOffset; i++)
			{
				if (i >= nextAvc)
				{
					int nalSize = 0;
					i = nextAvc;
					for (int j = 0; j < nalLengthSize; j++)
					{
						nalSize = (nalSize << 8) | videoData[i++];
					}
					if (nalSize <= 0 || nalSize > videoDataOffset - 1)
					{
						return videoDataOffset;
					}
					nextAvc = i + nalLengthSize;
					parseState = 5;
				}

				if (parseState == 7)
				{
					i += startCodeFindCandidate(i, nextAvc - i);
					if (i < nextAvc)
					{
						parseState = 2;
					}
				}
				else if (parseState <= 2)
				{
					if (videoData[i] == 1)
					{
						parseState ^= 5; // 2->7, 1->4, 0->5
					}
					else if (videoData[i] != 0)
					{
						parseState = 7;
					}
					else
					{
						parseState >>= 1; // 2->1, 1->0, 0->0
					}
				}
				else if (parseState <= 5)
				{
					int naluType = videoData[i] & 0x1F;
					if (naluType == H264Context.NAL_SEI || naluType == H264Context.NAL_SPS || naluType == H264Context.NAL_PPS || naluType == H264Context.NAL_AUD)
					{
						if (foundFrameStart)
						{
							found = i + 1;
							break;
						}
					}
					else if (naluType == H264Context.NAL_SLICE || naluType == H264Context.NAL_DPA || naluType == H264Context.NAL_IDR_SLICE)
					{
						parseState += 8;
						continue;
					}
					parseState = 7;
				}
				else
				{
					parseHistory[parseHistoryCount++] = videoData[i];
					if (parseHistoryCount > 5)
					{
						int lastMb = parseLastMb;
						GetBitContext gb = new GetBitContext();
						gb.init_get_bits(parseHistory, 0, 8 * parseHistoryCount);
						parseHistoryCount = 0;
						int mb = gb.get_ue_golomb("UmdVideoPlayer.findVideoFrameEnd");
						parseLastMb = mb;
						if (foundFrameStart)
						{
							if (mb <= lastMb)
							{
								found = i;
								break;
							}
						}
						else
						{
							foundFrameStart = true;
						}
						parseState = 7;
					}
				}
			}

			if (found >= 0)
			{
				foundFrameStart = false;
				found -= (parseState & 5);
				if (parseState > 7)
				{
					found -= 5;
				}
				parseState = 7;
				lastParsePosition = found;
			}
			else
			{
				lastParsePosition = videoDataOffset;
			}

			return found;
		}

		public virtual bool startVideo()
		{
			endOfVideo = false;
			videoPaused = false;

			videoCodec = CodecFactory.VideoCodec;
			videoCodecInit = false;
			videoDataOffset = 0;
			videoWidth = 0;
			videoHeight = 0;

			audioCodec = CodecFactory.getCodec(PSP_CODEC_AT3PLUS);
			audioCodecInitialized = false;
			audioChannels = 2;
			audioDataOffset = 0;
			audioFrameLength = 0;
			frameHeaderLength = 0;
			foundFrameStart = false;

			pesHeaderAudio = new PesHeader(audioChannel);
			pesHeaderVideo = new PesHeader(videoChannel);

			startTime = DateTimeHelper.CurrentUnixTimeMillis();
			frame = 0;
			currentChapterNumber = -1;

			return true;
		}

		private void stopDisplayThread()
		{
			done = true;
			while (displayThread != null && !threadExit)
			{
				sleep(1, 0);
			}
		}

		private void writeFile(int[] values, int size, string name)
		{
			try
			{
				System.IO.Stream os = new System.IO.FileStream(name, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				sbyte[] bytes = new sbyte[size];
				for (int i = 0; i < size; i++)
				{
					bytes[i] = (sbyte) values[i];
				}
				os.Write(bytes, 0, bytes.Length);
				os.Close();
			}
			catch (FileNotFoundException)
			{
			}
			catch (IOException)
			{
			}
		}

		public virtual void stepVideo()
		{
			image = null;

			int frameSize = -1;
			do
			{
				if (!readPsmfPacket(videoChannel, audioChannel))
				{
					if (videoDataOffset <= 0)
					{
						// Enf of file reached
						break;
					}
					frameSize = findVideoFrameEnd();
					if (frameSize < 0)
					{
						// Process pending last frame
						frameSize = videoDataOffset;
					}
				}
				else
				{
					frameSize = findVideoFrameEnd();
				}
			} while (frameSize <= 0 && !done);

			if (frameSize <= 0)
			{
				endOfVideo = true;
				return;
			}

			if (!videoCodecInit)
			{
				int[] extraData = null;
				int extraDataLength = H264Utils.findExtradata(videoData, 0, frameSize);
				if (extraDataLength > 0)
				{
					extraData = new int[extraDataLength];
					Array.Copy(videoData, 0, extraData, 0, extraDataLength);
				}

				if (videoCodec.init(extraData) == 0)
				{
					videoCodecInit = true;
				}
				else
				{
					endOfVideo = true;
					return;
				}
			}

			int consumedLength = videoCodec.decode(videoData, 0, frameSize);
			if (consumedLength < 0)
			{
				endOfVideo = true;
				return;
			}

			if (videoCodec.hasImage())
			{
				int[] aspectRatio = new int[2];
				videoCodec.getAspectRatio(aspectRatio);
				videoAspectRatioNum = aspectRatio[0];
				videoAspectRatioDen = aspectRatio[1];

				frame++;
			}

			consumeVideoData(consumedLength);

			bool skipFrame = false;
			if ((frame % fastForwardSpeeds[fastForwardSpeed]) != 0)
			{
				skipFrame = true;
				startTime -= sceMpeg.videoTimestampStep;
			}

			if (videoCodec.hasImage() && !skipFrame)
			{
				int width = videoCodec.ImageWidth;
				int height = videoCodec.ImageHeight;
				bool resized = false;
				if (videoWidth <= 0)
				{
					videoWidth = width;
					resized = true;
				}
				if (videoHeight <= 0)
				{
					videoHeight = height;
					resized = true;
				}
				if (log.TraceEnabled)
				{
					log.trace(string.Format("Decoded video frame {0:D}x{1:D} (video {2:D}x{3:D}), pes={4}, SAR {5:D}:{6:D}", width, height, videoWidth, videoHeight, pesHeaderVideo, videoAspectRatioNum, videoAspectRatioDen));
				}
				if (resized)
				{
					resizeVideoPlayer();
				}

				int size = width * height;
				int size2 = size >> 2;
				luma = resize(luma, size);
				cr = resize(cr, size2);
				cb = resize(cb, size2);

				if (videoCodec.getImage(luma, cb, cr) == 0)
				{
					if (dumpFrames)
					{
						writeFile(luma, size, string.Format("Frame{0:D}.y", frame));
						writeFile(cb, size2, string.Format("Frame{0:D}.cb", frame));
						writeFile(cr, size2, string.Format("Frame{0:D}.cr", frame));
					}

					abgr = resize(abgr, size);
					// TODO How to find out if we have a YUVJ image?
					// H264Utils.YUVJ2YUV(luma, luma, size);
					H264Utils.YUV2ARGB(width, height, luma, cb, cr, abgr);
					image = display.createImage(new MemoryImageSource(videoWidth, videoHeight, abgr, 0, width));

					long now = DateTimeHelper.CurrentUnixTimeMillis();
					long currentDuration = now - startTime;
					long videoDuration = frame * 100000L / sceMpeg.videoTimestampStep;
					if (currentDuration < videoDuration)
					{
						Utilities.sleep((int)(videoDuration - currentDuration), 0);
					}
				}
			}

			if (videoCodec.hasImage())
			{
				if (pesHeaderVideo.Pts != UNKNOWN_TIMESTAMP)
				{
					currentVideoTimestamp = pesHeaderVideo.Pts;
				}
				else
				{
					currentVideoTimestamp += sceMpeg.videoTimestampStep;
				}
				if (log.TraceEnabled)
				{
					MpsStreamInfo streamInfo = mpsStreams[currentStreamIndex];
					log.trace(string.Format("Playing stream {0:D}: {1} / {2}", currentStreamIndex, getTimestampString(currentVideoTimestamp - streamInfo.streamFirstTimestamp), getTimestampString(streamInfo.streamLastTimestamp - streamInfo.streamFirstTimestamp)));
				}
			}

			if (pesHeaderVideo.Pts != UNKNOWN_TIMESTAMP)
			{
				int chapterNumber = mpsStreams[currentStreamIndex].getChapterNumber(pesHeaderVideo.Pts);
				if (chapterNumber != currentChapterNumber)
				{
					if (moviePlayer != null)
					{
						// For the MoviePlayer, chapters are numbered starting from 1
						moviePlayer.onChapter(chapterNumber + 1);
					}
					currentChapterNumber = chapterNumber;
				}
			}

			if (audioFrameLength > 0 && audioDataOffset >= audioFrameLength)
			{
				if (!audioCodecInitialized)
				{
					audioCodec.init(audioFrameLength, audioChannels, audioChannels, 0);

					AudioFormat audioFormat = new AudioFormat(44100, 16, audioChannels, true, false);
					DataLine.Info info = new DataLine.Info(typeof(SourceDataLine), audioFormat);
					try
					{
						mLine = (SourceDataLine) AudioSystem.getLine(info);
						mLine.open(audioFormat);
					}
					catch (LineUnavailableException)
					{
						// Ignore error
					}
					mLine.start();

					audioCodecInitialized = true;
				}

				int result = -1;
				if (fastForwardSpeed == 0)
				{
					IMemoryWriter memoryWriter = MemoryWriter.getMemoryWriter(audioBufferAddr, audioFrameLength, 1);
					for (int i = 0; i < audioFrameLength; i++)
					{
						memoryWriter.writeNext(audioData[i]);
					}
					memoryWriter.flush();
					result = audioCodec.decode(audioBufferAddr, audioFrameLength, samplesAddr);
				}
				consumeAudioData(audioFrameLength);

				if (result > 0)
				{
					int audioBytesLength = audioCodec.NumberOfSamples * 2 * audioChannels;
					audioBytes = resize(audioBytes, audioBytesLength);
					IMemoryReader memoryReader = MemoryReader.getMemoryReader(samplesAddr, audioBytesLength, 1);
					for (int i = 0; i < audioBytesLength; i++)
					{
						audioBytes[i] = (sbyte) memoryReader.readNext();
					}
					mLine.write(audioBytes, 0, audioBytesLength);
				}
			}
		}

		public virtual void takeScreenshot()
		{
			int tag = 0;
			string screenshotName = State.title + "-" + "Shot" + "-" + tag + ".png";
			File screenshot = new File(screenshotName);
			File directory = new File(System.getProperty("user.dir"));
			foreach (File file in directory.listFiles())
			{
				if (file.Name.contains(State.title + "-" + "Shot"))
				{
					screenshotName = State.title + "-" + "Shot" + "-" + ++tag + ".png";
					screenshot = new File(screenshotName);
				}
			}
			try
			{
				BufferedImage img = (BufferedImage)Image;
				ImageIO.write(img, "png", screenshot);
				img.flush();
			}
			catch (Exception)
			{
				return;
			}
		}

		private Image Image
		{
			get
			{
				return image;
			}
		}

		public virtual Display RCODisplay
		{
			get
			{
				return rcoDisplay;
			}
		}

		private class DisplayControllerThread : Thread
		{
			private readonly UmdVideoPlayer outerInstance;

			public DisplayControllerThread(UmdVideoPlayer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal volatile bool done = false;

			public override void run()
			{
				while (!done)
				{
					Emulator.Scheduler.step();
					State.controller.hleControllerPoll();
					Utilities.sleep(10, 0);
				}
			}
		}

		private class MpsDisplayThread : Thread
		{
			private readonly UmdVideoPlayer outerInstance;

			public MpsDisplayThread(UmdVideoPlayer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void run()
			{
				if (log.TraceEnabled)
				{
					log.trace(string.Format("Starting Mps Display thread"));
				}

				outerInstance.threadExit = false;

				while (!outerInstance.done)
				{
					while (!outerInstance.endOfVideo && !outerInstance.done)
					{
						if (!outerInstance.videoPaused)
						{
							outerInstance.stepVideo();
							if (outerInstance.display != null && outerInstance.image != null)
							{
								Image scaledImage = outerInstance.Image;
								if (outerInstance.videoWidth != outerInstance.screenWidth || outerInstance.videoHeight != outerInstance.screenHeigth)
								{
									if (log.TraceEnabled)
									{
										log.trace(string.Format("Scaling video image from {0:D}x{1:D} to {2:D}x{3:D}", outerInstance.videoWidth, outerInstance.videoHeight, outerInstance.screenWidth, outerInstance.screenHeigth));
									}
									scaledImage = scaledImage.getScaledInstance(outerInstance.screenWidth, outerInstance.screenHeigth, Image.SCALE_SMOOTH);
								}
								outerInstance.display.Icon = new ImageIcon(scaledImage);
							}
						}
						else
						{
							Utilities.sleep(10, 0);
						}
					}
					if (!outerInstance.done)
					{
						if (outerInstance.moviePlayer != null)
						{
							outerInstance.done = true;
							outerInstance.moviePlayer.onPlayListEnd(outerInstance.mpsStreams[outerInstance.currentStreamIndex].PlayListNumber);
						}
						else
						{
							if (log.TraceEnabled)
							{
								log.trace(string.Format("Switching to next stream"));
							}
							if (!outerInstance.goToNextMpsStream())
							{
								outerInstance.done = true;
							}
						}
					}
				}

				outerInstance.threadExit = true;
				outerInstance.displayThread = null;

				if (log.TraceEnabled)
				{
					log.trace(string.Format("Exiting Mps Display thread"));
				}
			}
		}
	}
}