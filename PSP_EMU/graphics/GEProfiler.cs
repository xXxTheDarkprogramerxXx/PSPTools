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
namespace pspsharp.graphics
{

	using AbstractBoolSettingsListener = pspsharp.settings.AbstractBoolSettingsListener;
	using Settings = pspsharp.settings.Settings;

	//using Logger = org.apache.log4j.Logger;

	/// <summary>
	/// Profiler for the Graphics Engine
	/// 
	/// @author gid15
	/// 
	/// </summary>
	public class GEProfiler
	{
		//public static Logger log = Logger.getLogger("profiler");
		private static bool profilerEnabled = true;
		private static ProfilerEnabledSettingsListerner profilerEnabledSettingsListerner;
		private static readonly long? zero = new long?(0);
		private static Dictionary<int, long> cmdCounts = new Dictionary<int, long>();
		private static Dictionary<int, long> primVtypeCounts = new Dictionary<int, long>();
		private static Dictionary<int, string> vtypeNames = new Dictionary<int, string>();
		private static long geListCount;
		private static long textureLoadCount;
		private static long copyGeToMemoryCount;
		private static long copyStencilToMemoryCount;
		private static long geListDurationMicros;

		private class ProfilerEnabledSettingsListerner : AbstractBoolSettingsListener
		{
			protected internal override void settingsValueChanged(bool value)
			{
				ProfilerEnabled = value;
			}
		}

		public static void initialise()
		{
			if (profilerEnabledSettingsListerner == null)
			{
				profilerEnabledSettingsListerner = new ProfilerEnabledSettingsListerner();
				Settings.Instance.registerSettingsListener("Profiler", "emu.profiler", profilerEnabledSettingsListerner);
			}

			reset();
		}

		private static bool ProfilerEnabled
		{
			set
			{
				profilerEnabled = value;
			}
			get
			{
				return profilerEnabled;
			}
		}


		public static void reset()
		{
			if (!profilerEnabled)
			{
				return;
			}

			cmdCounts.Clear();
			primVtypeCounts.Clear();
			vtypeNames.Clear();
			geListCount = 0;
			textureLoadCount = 0;
			copyGeToMemoryCount = 0;
			geListDurationMicros = 0;
		}

		public static void exit()
		{
			if (!profilerEnabled || geListCount == 0)
			{
				return;
			}

			log.info("------------------ GEProfiler ----------------------");
			log.info(string.Format("GE list count: {0:D}", geListCount));
			log.info(string.Format("Texture load count: {0:D}, average {1:F1} per GE list", textureLoadCount, textureLoadCount / (double) geListCount));
			log.info(string.Format("Copy GE to memory: {0:D}, average {1:F1} per GE list", copyGeToMemoryCount, copyGeToMemoryCount / (double) geListCount));
			log.info(string.Format("Copy Stencil to memory: {0:D}, average {1:F1} per GE list", copyStencilToMemoryCount, copyStencilToMemoryCount / (double) geListCount));
			log.info(string.Format("GE list duration: {0:D}ms, average {1:F1}ms per GE list, max FPS is {2:F1}", geListDurationMicros / 1000, geListDurationMicros / (double) geListCount / 1000, 1000000 / (geListDurationMicros / (double) geListCount)));
			GeCommands geCommands = GeCommands.Instance;
			foreach (int? cmd in cmdCounts.Keys)
			{
				long? cmdCount = cmdCounts[cmd];
				log.info(string.Format("{0}: called {1:D} times, average {2:F1} per GE list", geCommands.getCommandString(cmd.Value), cmdCount.Value, cmdCount.Value / (double) geListCount));
			}

			// Sort the primVtypeCounts based on their counts (highest count first).
			IList<int> primVtypeSorted = new List<int>(primVtypeCounts.Keys);
			primVtypeSorted.Sort(new ComparatorAnonymousInnerClass());

			foreach (int? vtype in primVtypeSorted)
			{
				long? vtypeCount = primVtypeCounts[vtype];
				log.info(string.Format("{0}: used {1:D} times in PRIM, average {2:F1} per GE list", vtypeNames[vtype], vtypeCount.Value, vtypeCount.Value / (double) geListCount));
			}
		}

		private class ComparatorAnonymousInnerClass : Comparator<int>
		{
			public ComparatorAnonymousInnerClass()
			{
			}

			public int compare(int? vtype1, int? vtype2)
			{
				return -primVtypeCounts[vtype1].compareTo(primVtypeCounts[vtype2]);
			}
		}

		public static void startGeList()
		{
			geListCount++;
		}

		public static void startGeCmd(int cmd)
		{
			long? cmdCount = cmdCounts[cmd];
			if (cmdCount == null)
			{
				cmdCount = zero;
			}

			cmdCounts[cmd] = cmdCount + 1;

			if (cmd == GeCommands.PRIM)
			{
				VertexInfo vinfo = VideoEngine.Instance.Context.vinfo;
				int vtype = vinfo.vtype;
				long? vtypeCount = primVtypeCounts[vtype];
				if (vtypeCount == null)
				{
					vtypeCount = zero;
					vtypeNames[vtype] = vinfo.ToString();
				}
				primVtypeCounts[vtype] = vtypeCount + 1;
			}
		}

		public static void loadTexture()
		{
			textureLoadCount++;
		}

		public static void copyGeToMemory()
		{
			copyGeToMemoryCount++;
		}

		public static void copyStencilToMemory()
		{
			copyStencilToMemoryCount++;
		}

		public static void geListDuration(long micros)
		{
			geListDurationMicros += micros;
		}
	}

}