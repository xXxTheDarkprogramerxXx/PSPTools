using System;
using System.Collections.Generic;
using System.Threading;

/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
namespace pspsharp.GUI
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.HLE.modules.sceAudiocodec.PSP_CODEC_AT3PLUS;



	//using Logger = org.apache.log4j.Logger;

	using IVirtualFile = pspsharp.HLE.VFS.IVirtualFile;
	using UmdIsoVirtualFile = pspsharp.HLE.VFS.iso.UmdIsoVirtualFile;
	using sceMpeg = pspsharp.HLE.modules.sceMpeg;
	using AtracFileInfo = pspsharp.HLE.modules.sceAtrac3plus.AtracFileInfo;
	using PSMFHeader = pspsharp.HLE.modules.sceMpeg.PSMFHeader;
	using UmdIsoFile = pspsharp.filesystems.umdiso.UmdIsoFile;
	using UmdIsoReader = pspsharp.filesystems.umdiso.UmdIsoReader;
	using PSF = pspsharp.format.PSF;
	using PsmfAudioDemuxVirtualFile = pspsharp.format.psmf.PsmfAudioDemuxVirtualFile;
	using Settings = pspsharp.settings.Settings;
	using Constants = pspsharp.util.Constants;

	/// <summary>
	/// @author Orphis, gid15
	/// </summary>
	public class UmdBrowser : javax.swing.JDialog
	{
		private const long serialVersionUID = 7788144302296106541L;
		private static Logger log = Emulator.log;

		private class EbootFileFilter : FileFilter
		{
			public override bool accept(File file)
			{
				return file.Name.equalsIgnoreCase("eboot.pbp");
			}
		}

		private static bool isEbootFile(File file)
		{
			if (!file.File)
			{
				return false;
			}

			// Basic sanity checks on EBOOT.PBP
			DataInputStream @is = null;
			try
			{
				@is = new DataInputStream(new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read));
				sbyte[] header = new sbyte[0x24];
				int Length = @is.read(header);
				if (Length != header.Length)
				{
					return false;
				}
				// PBP header?
				if (header[0] != 0 || header[1] != (sbyte)'P' || header[2] != (sbyte)'B' || header[3] != (sbyte)'P')
				{
					return false;
				}
			}
			catch (IOException)
			{
				return false;
			}
			finally
			{
				if (@is != null)
				{
					try
					{
						@is.close();
					}
					catch (IOException)
					{
						// Ignore exception
					}
				}
			}

			// Valid EBOOT.PBP
			return true;
		}

		private static bool isEbootDirectory(File directory)
		{
			if (!directory.Directory)
			{
				return false;
			}

			File[] eboot = directory.listFiles(new EbootFileFilter());
			if (eboot.Length != 1)
			{
				return false;
			}

			return isEbootFile(eboot[0]);
		}

		public class UmdFileFilter : FileFilter
		{
			public override bool accept(File file)
			{
				string lower = file.Name.ToLower();
				if (lower.EndsWith(".cso", StringComparison.Ordinal) || lower.EndsWith(".iso", StringComparison.Ordinal))
				{
					return true;
				}
				if (isEbootDirectory(file))
				{
					return true;
				}
				return false;
			}
		}

		private sealed class MemStickTableModel : AbstractTableModel
		{
			private readonly UmdBrowser outerInstance;

			internal const long serialVersionUID = -1675488447176776560L;
			internal UmdInfoLoader umdInfoLoader;

			public MemStickTableModel(UmdBrowser outerInstance, File[] paths)
			{
				this.outerInstance = outerInstance;
				// Default values in case we return an error
				outerInstance.umdInfoLoaded = new bool[0];

				// Collect all the programs for all the given paths
				IList<File> programList = new List<File>();
				foreach (File path in paths)
				{
					if (!path.Directory)
					{
						Console.WriteLine("'" + path + "' is not a directory.");
						return;
					}

					try
					{
						outerInstance.pathPrefix = path.CanonicalPath;
					}
					catch (IOException)
					{
						outerInstance.pathPrefix = path.Path;
					}

					File[] pathPrograms = path.listFiles(new UmdFileFilter());

					((List<File>)programList).AddRange(Array.asList(pathPrograms));
				}

				// Sort the programs based on their file name
				programList.Sort(new ComparatorAnonymousInnerClass(this));

				outerInstance.programs = programList.ToArray();

				// The UMD informations are loaded asynchronously
				// to provide a faster loading time for the UmdBrowser.
				// Prepare the containers for the information and
				// start the async loader thread as a daemon running at low priority.
				outerInstance.icons = new ImageIcon[outerInstance.programs.Length];
				outerInstance.psfs = new PSF[outerInstance.programs.Length];
				outerInstance.umdInfoLoaded = new bool[outerInstance.programs.Length];
				outerInstance.filteredItems = new int[outerInstance.programs.Length];
				outerInstance.numberFilteredItems = outerInstance.programs.Length;

				for (int i = 0; i < outerInstance.programs.Length; ++i)
				{
					outerInstance.umdInfoLoaded[i] = false;
					outerInstance.filteredItems[i] = i;
				}
				// load the first row: its size is used to compute the table size
				outerInstance.loadUmdInfo(0);

				umdInfoLoader = new UmdInfoLoader(outerInstance);
				umdInfoLoader.Name = "Umd Browser - Umd Info Loader";
				umdInfoLoader.Priority = Thread.MIN_PRIORITY;
				umdInfoLoader.Daemon = true;
				umdInfoLoader.Start();
			}

			private class ComparatorAnonymousInnerClass : Comparator<File>
			{
				private readonly MemStickTableModel outerInstance;

				public ComparatorAnonymousInnerClass(MemStickTableModel outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public int compare(File file1, File file2)
				{
					if (file1 == null)
					{
						return (file2 == null ? 0 : 1);
					}
					else if (file2 == null)
					{
						return -1;
					}

					string name1 = file1.Name.ToLower();
					string name2 = file2.Name.ToLower();
					if (name1.Equals(name2))
					{
						return compare(file1.ParentFile, file2.ParentFile);
					}
					return name1.CompareTo(name2);
				}
			}

			public override Type getColumnClass(int columnIndex)
			{
				switch (columnIndex)
				{
					case 0:
						return typeof(Icon);
					case 1:
						return typeof(string);
					default:
						throw new System.IndexOutOfRangeException("column index out of range");
				}
			}

			public override string getColumnName(int column)
			{
				java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
				switch (column)
				{
					case 0:
						return bundle.getString("MemStickBrowser.column.icon.text");
					case 1:
						return bundle.getString("MemStickBrowser.column.title.text");
					default:
						throw new System.IndexOutOfRangeException("column index out of range");
				}
			}

			public override int ColumnCount
			{
				get
				{
					return 2;
				}
			}

			public override int RowCount
			{
				get
				{
					if (outerInstance.programs == null)
					{
						return 0;
					}
					if (outerInstance.numberFilteredItems >= 0)
					{
						return outerInstance.numberFilteredItems;
					}
					return outerInstance.programs.Length;
				}
			}

			public override object getValueAt(int rowIndex, int columnIndex)
			{
				if (rowIndex >= outerInstance.numberFilteredItems)
				{
					return null;
				}
				rowIndex = outerInstance.filteredItems[rowIndex];

				if (rowIndex >= outerInstance.umdInfoLoaded.Length)
				{
					return null;
				}

				try
				{
					outerInstance.waitForUmdInfoLoaded(rowIndex);

					switch (columnIndex)
					{
						case 0:
							return outerInstance.icons[rowIndex];
						case 1:
							string title = outerInstance.getTitle(rowIndex);
							string discid = outerInstance.getDiscId(rowIndex);
							string firmware = outerInstance.getFirmware(rowIndex);
							string prgPath = outerInstance.getProgramPath(rowIndex);

							java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
							string text = string.Format("{0}\n{1}: {2}\n{3}: {4}\n{5}", title, bundle.getString("UmdBrowser.strDiscID.text"), discid, bundle.getString("UmdBrowser.strFirmware.text"), firmware, prgPath);
							return text;
					}
				}
				catch (IOException e)
				{
					Console.WriteLine(e);
				}
				return null;
			}
		}
		private File[] programs;
		private ImageIcon[] icons;
		private PSF[] psfs;
		private volatile bool[] umdInfoLoaded;
		private UmdBrowserPmf umdBrowserPmf;
		private UmdBrowserSound umdBrowserSound;
		private int lastRowIndex = -1;
		private bool isSwitchingUmd;
		private MainGUI gui;
		private File[] paths;
		private int[] filteredItems;
		private int numberFilteredItems = -1;
		private string pathPrefix;

		/// <summary>
		/// Creates new form UmdBrowser
		/// </summary>
		public UmdBrowser(MainGUI gui, File[] paths) : base(gui)
		{

			this.gui = gui;
			this.paths = paths;

			initPNG();

			initComponents();

			// set blinking border for ICON0
			icon0Label.Border = new PmfBorder(this);

			// restrict icon column width manually
			table.ColumnModel.getColumn(0).MinWidth = Constants.ICON0_WIDTH;
			table.ColumnModel.getColumn(0).MaxWidth = Constants.ICON0_WIDTH;

			// set custom renderers
			table.setDefaultRenderer(typeof(Icon), new DefaultTableCellRendererAnonymousInnerClass(this));
			table.setDefaultRenderer(typeof(string), new DefaultTableCellRendererAnonymousInnerClass2(this));

			// update icons on selection change
			table.SelectionModel.addListSelectionListener(new ListSelectionListenerAnonymousInnerClass(this));

			// update the filtering on filter change
			filterField.Document.addDocumentListener(new DocumentListenerAnonymousInnerClass(this));

			filterField.requestFocus();

			WindowPropSaver.loadWindowProperties(this);
		}

		private class DefaultTableCellRendererAnonymousInnerClass : DefaultTableCellRenderer
		{
			private readonly UmdBrowser outerInstance;

			public DefaultTableCellRendererAnonymousInnerClass(UmdBrowser outerInstance)
			{
				this.outerInstance = outerInstance;
				serialVersionUID = 1L;
			}

			private static readonly long serialVersionUID;

			public override Component getTableCellRendererComponent(JTable table, object value, bool isSelected, bool hasFocus, int row, int column)
			{
				Text = ""; // NOI18N
				Icon = (Icon) value;
				return this;
			}
		}

		private class DefaultTableCellRendererAnonymousInnerClass2 : DefaultTableCellRenderer
		{
			private readonly UmdBrowser outerInstance;

			public DefaultTableCellRendererAnonymousInnerClass2(UmdBrowser outerInstance)
			{
				this.outerInstance = outerInstance;
				serialVersionUID = 1L;
			}

			private static readonly long serialVersionUID;

			public override Component getTableCellRendererComponent(JTable table, object value, bool isSelected, bool hasFocus, int row, int column)
			{
				JTextArea textArea = new JTextArea((string) value);
				textArea.Font = Font;
				if (isSelected)
				{
					textArea.Foreground = table.SelectionForeground;
					textArea.Background = table.SelectionBackground;
				}
				else
				{
					textArea.Foreground = table.Foreground;
					textArea.Background = table.Background;
				}
				return textArea;
			}
		}

		private class ListSelectionListenerAnonymousInnerClass : ListSelectionListener
		{
			private readonly UmdBrowser outerInstance;

			public ListSelectionListenerAnonymousInnerClass(UmdBrowser outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void valueChanged(ListSelectionEvent @event)
			{
				outerInstance.onSelectionChanged(@event);
			}
		}

		private class DocumentListenerAnonymousInnerClass : DocumentListener
		{
			private readonly UmdBrowser outerInstance;

			public DocumentListenerAnonymousInnerClass(UmdBrowser outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void removeUpdate(DocumentEvent e)
			{
				outerInstance.onFilterChanged();
			}

			public override void insertUpdate(DocumentEvent e)
			{
				outerInstance.onFilterChanged();
			}

			public override void changedUpdate(DocumentEvent e)
			{
				outerInstance.onFilterChanged();
			}
		}

		private void waitForUmdInfoLoaded(int rowIndex)
		{
			// The UMD info is loaded asynchronously.
			// Wait for the information to be loaded.
			while (!umdInfoLoaded[rowIndex])
			{
				sleep(1);
			}
		}

		private void initPNG()
		{
			// Invoke
			//    sun.awt.image.PNGImageDecoder.setCheckCRC(false)
			// to avoid an exception "PNGImageDecoder$PNGException: crc corruption" when reading incorrect PNG files.
			// As this is a class in the "sun" package, be careful as this class could disappear in a later JDK version:
			// do not statically reference this class and invoke the method using reflection.
			try
			{
				this.GetType().ClassLoader.loadClass("sun.awt.image.PNGImageDecoder").getMethod("setCheckCRC", typeof(bool)).invoke(null, false);
			}
			catch (Exception e)
			{
				log.info(e);
			}
		}

		private string getUmdBrowseCacheDirectory(string name)
		{
			// Return "tmp/UmdBrowserCache/<name>/"
			return string.Format("{0}{1}UmdBrowserCache{1}{2}{1}", Settings.Instance.readString("emu.tmppath"), System.IO.Path.DirectorySeparatorChar, name);
		}

		private void writeUmdBrowseCacheFile(string cacheDirectory, string name, sbyte[] content)
		{
			try
			{
				System.IO.Stream os = new System.IO.FileStream(cacheDirectory + name, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				os.Write(content, 0, content.Length);
				os.Close();
			}
			catch (FileNotFoundException e)
			{
				Console.WriteLine(e);
			}
			catch (IOException e)
			{
				Console.WriteLine(e);
			}
		}

		private void loadUmdInfo(int rowIndex)
		{
			if (rowIndex >= umdInfoLoaded.Length || umdInfoLoaded[rowIndex])
			{
				return;
			}

			try
			{
				bool cacheEntry = true;
				string entryName = programs[rowIndex].Name;
				if (programs[rowIndex].Directory)
				{
					File[] eboot = programs[rowIndex].listFiles(new EbootFileFilter());
					if (eboot.Length > 0)
					{
						programs[rowIndex] = eboot[0];
					}
					else
					{
						cacheEntry = false;
					}
				}

				if (cacheEntry)
				{
					string cacheDirectory = getUmdBrowseCacheDirectory(entryName);
					File sfoFile = new File(cacheDirectory + "param.sfo");
					if (sfoFile.canRead())
					{
						// Read the param.sfo and ICON0.PNG from the UmdBrowserCache
						sbyte[] sfo = new sbyte[(int) sfoFile.Length()];
						System.IO.Stream @is = new System.IO.FileStream(sfoFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
						@is.Read(sfo, 0, sfo.Length);
						@is.Close();
						psfs[rowIndex] = new PSF();
						psfs[rowIndex].read(ByteBuffer.wrap(sfo));

						File icon0File = new File(cacheDirectory + "ICON0.PNG");
						if (icon0File.canRead())
						{
							icons[rowIndex] = new ImageIcon(icon0File.Path);
						}
						else
						{
							icons[rowIndex] = new ImageIcon(this.GetType().getResource("/pspsharp/images/icon0.png"));
						}
					}
					else
					{
						// Read the param.sfo and ICON0.PNG from the ISO and
						// store them in the UmdBrowserCache.

						// Create the UmdBrowse Cache directories
						System.IO.Directory.CreateDirectory(cacheDirectory);

						UmdIsoReader iso = new UmdIsoReader(programs[rowIndex].Path);

						sbyte[] sfo = iso.readParamSFO();
						if (sfo == null)
						{
							throw new FileNotFoundException();
						}
						writeUmdBrowseCacheFile(cacheDirectory, "param.sfo", sfo);
						ByteBuffer buf = ByteBuffer.wrap(sfo);
						psfs[rowIndex] = new PSF();
						psfs[rowIndex].read(buf);

						sbyte[] icon0 = iso.readIcon0();
						if (icon0 == null)
						{
							// default icon
							icons[rowIndex] = new ImageIcon(this.GetType().getResource("/pspsharp/images/icon0.png"));
						}
						else
						{
							writeUmdBrowseCacheFile(cacheDirectory, "ICON0.PNG", icon0);
							icons[rowIndex] = new ImageIcon(icon0);
						}
					}
				}
			}
			catch (FileNotFoundException)
			{
				// Check if we're dealing with a UMD_VIDEO.
				try
				{
					UmdIsoReader iso = new UmdIsoReader(programs[rowIndex].Path);

					UmdIsoFile paramSfo = iso.getFile("UMD_VIDEO/param.sfo");
					UmdIsoFile umdDataFile = iso.getFile("UMD_DATA.BIN");

					// Manually fetch the DISC ID from the UMD_DATA.BIN (video ISO files lack
					// this param in their param.sfo).
					sbyte[] umdDataId = new sbyte[10];
					umdDataFile.readFully(umdDataId, 0, 9);
					string umdDataIdString = StringHelper.NewString(umdDataId);

					sbyte[] sfo = new sbyte[(int) paramSfo.Length()];
					paramSfo.read(sfo);
					paramSfo.Dispose();
					ByteBuffer buf = ByteBuffer.wrap(sfo);
					psfs[rowIndex] = new PSF();
					psfs[rowIndex].read(buf);
					psfs[rowIndex].put("DISC_ID", umdDataIdString);

					UmdIsoFile icon0umd = iso.getFile("UMD_VIDEO/ICON0.PNG");
					sbyte[] icon0 = new sbyte[(int) icon0umd.Length()];
					icon0umd.read(icon0);
					icon0umd.Dispose();
					icons[rowIndex] = new ImageIcon(icon0);
				}
				catch (FileNotFoundException)
				{
					// default icon
					icons[rowIndex] = new ImageIcon(this.GetType().getResource("/pspsharp/images/icon0.png"));
				}
				catch (IOException ve)
				{
					Console.WriteLine(ve);
				}
			}
			catch (IOException e)
			{
				Console.WriteLine(e);
			}

			umdInfoLoaded[rowIndex] = true;

			updateFilteredItem(rowIndex);
		}

		private void onSelectionChanged(ListSelectionEvent @event)
		{
			loadButton.Enabled = !((ListSelectionModel) @event.Source).SelectionEmpty;

			ImageIcon pic0Icon = null;
			ImageIcon pic1Icon = null;
			ImageIcon icon0Icon = null;
			try
			{
				int rowIndex = SelectedRowIndex;
				UmdIsoReader iso = new UmdIsoReader(programs[rowIndex].Path);

				// Read PIC0.PNG
				try
				{
					sbyte[] pic0 = iso.readPic0();
					if (pic0 != null)
					{
						pic0Icon = new ImageIcon(pic0);
					}
				}
				catch (IOException e)
				{
					Console.WriteLine(e);
				}

				// Read PIC1.PNG
				try
				{
					sbyte[] pic1 = iso.readPic1();
					if (pic1 != null)
					{
						pic1Icon = new ImageIcon(pic1);
					}
					else
					{
						// Check if we're dealing with a UMD_VIDEO.
						try
						{
							UmdIsoFile pic1umd = iso.getFile("UMD_VIDEO/PIC1.PNG");
							pic1 = new sbyte[(int) pic1umd.Length()];
							pic1umd.read(pic1);
							pic1umd.Dispose();
							pic1Icon = new ImageIcon(pic1);
						}
						catch (FileNotFoundException)
						{
							// Generate an empty image
							pic1Icon = new ImageIcon();
							BufferedImage image = new BufferedImage(Constants.PSPSCREEN_WIDTH, Constants.PSPSCREEN_HEIGHT, BufferedImage.TYPE_INT_ARGB);
							pic1Icon.Image = image;
						}
					}
				}
				catch (IOException e)
				{
					Console.WriteLine(e);
				}

				icon0Icon = icons[rowIndex];

				if (lastRowIndex != rowIndex)
				{
					stopVideo();
					umdBrowserPmf = new UmdBrowserPmf(iso, "PSP_GAME/ICON1.PMF", icon0Label);
					if (iso.hasFile("PSP_GAME/SND0.AT3"))
					{
						umdBrowserSound = new UmdBrowserSound(Memory.Instance, iso.readSnd0());
					}
					else
					{
						IVirtualFile pmf = new UmdIsoVirtualFile(iso.getFile("PSP_GAME/ICON1.PMF"));
						sbyte[] mpegHeader = new sbyte[sceMpeg.MPEG_HEADER_BUFFER_MINIMUM_SIZE];
						if (pmf.ioRead(mpegHeader, 0, mpegHeader.Length) == mpegHeader.Length)
						{
							sceMpeg.PSMFHeader psmfHeader = new sceMpeg.PSMFHeader(0, mpegHeader);
							if (psmfHeader.getSpecificStreamNum(sceMpeg.PSMF_ATRAC_STREAM) > 0)
							{
								IVirtualFile audio = new PsmfAudioDemuxVirtualFile(pmf, psmfHeader.mpegOffset, 0);
								AtracFileInfo atracFileInfo = new AtracFileInfo();
								atracFileInfo.inputFileDataOffset = 0;
								atracFileInfo.atracChannels = 2;
								atracFileInfo.atracCodingMode = 0;
								umdBrowserSound = new UmdBrowserSound(Memory.Instance, audio, PSP_CODEC_AT3PLUS, atracFileInfo);
								audio.ioClose();
							}
						}
						pmf.ioClose();
					}
				}

				lastRowIndex = rowIndex;
			}
			catch (FileNotFoundException)
			{
				// Ignore exception
			}
			catch (IOException e)
			{
				Console.WriteLine(e);
			}
			pic0Label.Icon = pic0Icon;
			pic1Label.Icon = pic1Icon;
			icon0Label.Icon = icon0Icon;
		}

		private string getTitle(int rowIndex)
		{
			string title;
			if (psfs[rowIndex] == null || string.ReferenceEquals((title = psfs[rowIndex].getString("TITLE")), null))
			{
				// No PSF TITLE, get the parent directory name
				title = programs[rowIndex].ParentFile.Name;
			}

			return title;
		}

		private string getDiscId(int rowIndex)
		{
			string discId;
			if (psfs[rowIndex] == null || string.ReferenceEquals((discId = psfs[rowIndex].getString("DISC_ID")), null))
			{
				discId = "No ID";
			}

			return discId;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private String getProgramPath(int rowIndex) throws java.io.IOException
		private string getProgramPath(int rowIndex)
		{
			string programPath = programs[rowIndex].CanonicalPath;
			if (programPath.StartsWith(pathPrefix, StringComparison.Ordinal))
			{
				programPath = programPath.Substring(pathPrefix.Length + 1);
			}
			else
			{
				string cwdPath = (new File(".")).CanonicalPath;
				if (programPath.StartsWith(cwdPath, StringComparison.Ordinal))
				{
					programPath = programPath.Substring(cwdPath.Length + 1);
				}
			}

			return programPath;
		}

		private string getFirmware(int rowIndex)
		{
			string firmware;
			if (psfs[rowIndex] == null || string.ReferenceEquals((firmware = psfs[rowIndex].getString("PSP_SYSTEM_VER")), null))
			{
				firmware = "Not found";
			}

			return firmware;
		}

		private void scrollTo(char c)
		{
			c = char.ToLower(c);
			int scrollToRow = -1;
			for (int rowIndex = 0; rowIndex < programs.Length; rowIndex++)
			{
				string title = getTitle(rowIndex);
				if (!string.ReferenceEquals(title, null) && title.Length > 0)
				{
					char firstChar = char.ToLower(title[0]);
					if (firstChar == c)
					{
						scrollToRow = rowIndex;
						break;
					}
				}
			}

			if (scrollToRow >= 0)
			{
				table.scrollRectToVisible(table.getCellRect(scrollToRow, 0, true));
			}
		}

		private void stopVideo()
		{
			if (umdBrowserPmf != null)
			{
				umdBrowserPmf.stopVideo();
				umdBrowserPmf = null;
			}

			if (umdBrowserSound != null)
			{
				umdBrowserSound.stopSound();
				umdBrowserSound = null;
			}
		}

		private bool filter(string filter, string text)
		{
			text = text.ToLower();

			return text.Contains(filter);
		}

		private bool filterItems(string filter)
		{
			lock (filteredItems)
			{
				numberFilteredItems = 0;

				for (int rowIndex = 0; rowIndex < programs.Length; rowIndex++)
				{
					filterItem(filter, rowIndex);
				}
			}

			return true;
		}

		private string Filter
		{
			get
			{
				string filter = filterField.Text;
				filter = filter.Trim().ToLower();
    
				return filter;
			}
		}

		private void updateFilteredItem(int rowIndex)
		{
			bool alreadyPresent = false;
			for (int i = 0; i < numberFilteredItems; i++)
			{
				if (filteredItems[i] == rowIndex)
				{
					alreadyPresent = true;
					break;
				}
			}

			bool modified = false;
			if (!alreadyPresent)
			{
				string filter = Filter;
				lock (filteredItems)
				{
					modified = filterItem(filter, rowIndex);
				}
			}

			if (modified)
			{
				((AbstractTableModel) table.Model).fireTableDataChanged();
			}
		}

		private bool filterItem(string filter, int rowIndex)
		{
			bool show = false;

			if (umdInfoLoaded[rowIndex])
			{
				try
				{
					string title = getTitle(rowIndex);
					string discId = getDiscId(rowIndex);
					string programPath = getProgramPath(rowIndex);
					string firmware = getFirmware(rowIndex);

					if (this.filter(filter, title) || this.filter(filter, discId) || this.filter(filter, programPath) || this.filter(filter, firmware))
					{
						show = true;
					}
				}
				catch (IOException)
				{
					show = true;
				}
			}

			if (show)
			{
				filteredItems[numberFilteredItems] = rowIndex;
				numberFilteredItems++;
			}

			return show;
		}

		private void onFilterChanged()
		{
			string filter = Filter;

			//if (log.DebugEnabled)
			{
				Console.WriteLine(string.Format("onFilterChanged '{0}'", filter));
			}

			if (filterItems(filter))
			{
				((AbstractTableModel) table.Model).fireTableDataChanged();
			}
		}

		private int SelectedRowIndex
		{
			get
			{
				int rowIndex = table.SelectedRow;
				if (rowIndex < numberFilteredItems)
				{
					rowIndex = filteredItems[rowIndex];
				}
    
				return rowIndex;
			}
		}

		private void loadSelectedfile()
		{
			stopVideo();

			File selectedFile = programs[SelectedRowIndex];
			if (SwitchingUmd)
			{
				gui.switchUMD(selectedFile);
				Visible = false;
				dispose();
			}
			else
			{
				gui.loadAndRunUMD(selectedFile);
				dispose();
			}
		}

		public override void dispose()
		{
			// Stop the PMF video and sound before closing the UMD Browser
			stopVideo();
			base.dispose();
		}

		private static void sleep(long millis)
		{
			if (millis > 0)
			{
				try
				{
					Thread.Sleep(millis);
				}
				catch (InterruptedException)
				{
					// Ignore exception
				}
			}
		}

		/// <summary>
		/// Load asynchronously all the UMD information (icon, PSF).
		/// </summary>
		private class UmdInfoLoader : Thread
		{
			private readonly UmdBrowser outerInstance;

			public UmdInfoLoader(UmdBrowser outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void run()
			{
				for (int i = 0; i < outerInstance.umdInfoLoaded.Length; i++)
				{
					outerInstance.loadUmdInfo(i);
				}
			}
		}

		public virtual bool SwitchingUmd
		{
			get
			{
				return isSwitchingUmd;
			}
			set
			{
				this.isSwitchingUmd = value;
				loadButton.Text = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp").getString(this.SwitchingUmd ? "UmdBrowser.loadButtonSwitch.text" : "UmdBrowser.loadButton.text");
			}
		}


		private class PmfBorder : AbstractBorder
		{
			private readonly UmdBrowser outerInstance;

			public PmfBorder(UmdBrowser outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal const long serialVersionUID = -700510222853542503L;
			internal const int leftSpace = 20;
			internal const int topSpace = 8;
			internal const int borderWidth = 8;
			internal const int millisPerBeat = 1500;

			public override Insets getBorderInsets(Component c, Insets insets)
			{
				insets.set(topSpace, leftSpace, borderWidth, borderWidth);

				return insets;
			}

			public override Insets getBorderInsets(Component c)
			{
				return getBorderInsets(c, new Insets(0, 0, 0, 0));
			}

			public override void paintBorder(Component c, Graphics g, int x, int y, int width, int height)
			{
				if (outerInstance.icon0Label.Icon == null)
				{
					return;
				}

				long now = DateTimeHelper.CurrentUnixTimeMillis();
				float beat = (now % millisPerBeat) / (float) millisPerBeat;
				float noBeat = 0.5f;

				// Draw border lines
				for (int i = 0; i < borderWidth; i++)
				{
					int alpha = getAlpha(noBeat, i);
					setColor(g, beat, alpha);

					// Vertical line on the right side
					g.drawLine(x + width - borderWidth + i, y + topSpace, x + width - borderWidth + i, y + height - borderWidth);

					// Horizontal line at the bottom
					g.drawLine(x + leftSpace, y + height - borderWidth + i, x + width - borderWidth, y + height - borderWidth + i);

					alpha = getAlpha(beat, i);
					setColor(g, noBeat, alpha);

					// Vertical line on the left side
					g.drawLine(x + leftSpace - i, y + topSpace, x + leftSpace - i, y + height - borderWidth);

					// Horizontal line at the top
					g.drawLine(x + leftSpace, y + topSpace - i, x + width - borderWidth, y + topSpace - i);
				}

				// Top left corner
				drawCorner(g, beat, noBeat, x + leftSpace - borderWidth, y + topSpace - borderWidth, borderWidth, borderWidth);

				// Top right corner
				drawCorner(g, beat, noBeat, x + width - borderWidth, y + topSpace - borderWidth, 0, borderWidth);

				// Bottom left corner
				drawCorner(g, beat, noBeat, x + leftSpace - borderWidth, y + height - borderWidth, borderWidth, 0);

				// Bottom right corner
				drawCorner(g, noBeat, beat, x + width - borderWidth, y + height - borderWidth, 0, 0);
			}

			internal virtual void drawCorner(Graphics g, float alphaBeat, float colorBeat, int x, int y, int centerX, int centerY)
			{
				for (int ix = 1; ix < borderWidth; ix++)
				{
					for (int iy = 1; iy < borderWidth; iy++)
					{
						int alpha = getAlpha(alphaBeat, ix - centerX, iy - centerY);
						setColor(g, colorBeat, alpha);
						drawPoint(g, x + ix, y + iy);
					}
				}
			}

			internal virtual int getAlpha(float beat, int distanceX, int distanceY)
			{
				float distance = (float) System.Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

				return getAlpha(beat, distance);
			}

			internal virtual int getAlpha(float beat, float distance)
			{
				const float maxDistance = borderWidth;

				int maxAlpha = 0xF0;
				if (beat < 0.5f)
				{
					// beat 0.0 -> 0.5: increase alpha from 0 to max
					maxAlpha = (int)(maxAlpha * beat * 2);
				}
				else
				{
					// beat 0.5 -> 1.0: decrease alpha from max to 0
					maxAlpha = (int)(maxAlpha * (1 - beat) * 2);
				}

				distance = System.Math.Abs(distance);
				distance = System.Math.Min(distance, maxDistance);

				return maxAlpha - (int)((distance * maxAlpha) / maxDistance);
			}

			internal virtual void setColor(Graphics g, float beat, int alpha)
			{
				int color = 0xA0;

				if (beat < 0.5f)
				{
					// beat 0.0 -> 0.5: increase color from 0 to max
					color = (int)(color * beat * 2);
				}
				else
				{
					// beat 0.5 -> 1.0: decrease alpha from max to 0
					color = (int)(color * (1 - beat) * 2);
				}

				g.Color = new Color(color, color, color, alpha);
			}

			internal virtual void drawPoint(Graphics g, int x, int y)
			{
				g.drawLine(x, y, x, y);
			}
		}

		/// <summary>
		/// This method is called from within the constructor to initialize the form.
		/// WARNING: Do NOT modify this code. The content of this method is always
		/// regenerated by the Form Editor.
		/// </summary>
		// <editor-fold defaultstate="collapsed" desc="Generated Code">//GEN-BEGIN:initComponents
		private void initComponents()
		{
			java.awt.GridBagConstraints gridBagConstraints;

			filterLabel = new javax.swing.JLabel();
			filterField = new javax.swing.JTextField();
			loadButton = new javax.swing.JButton();
			cancelButton = new pspsharp.GUI.CancelButton();
			jScrollPane1 = new javax.swing.JScrollPane();
			table = new JTable();
			imagePanel = new javax.swing.JPanel();
			icon0Label = new javax.swing.JLabel();
			pic0Label = new javax.swing.JLabel();
			pic1Label = new javax.swing.JLabel();

			DefaultCloseOperation = javax.swing.WindowConstants.DISPOSE_ON_CLOSE;
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			Title = bundle.getString("UmdBrowser.title"); // NOI18N
			ModalityType = java.awt.Dialog.ModalityType.APPLICATION_MODAL;

			filterLabel.Text = bundle.getString("FilterLabel.text"); // NOI18N

			filterField.Text = ""; // NOI18N

			loadButton.Text = bundle.getString("LoadButton.text"); // NOI18N
			loadButton.Enabled = false;
			loadButton.addActionListener(new ActionListenerAnonymousInnerClass(this));

			cancelButton.Text = bundle.getString("CancelButton.text"); // NOI18N
			cancelButton.Parent = this;

			table.Model = new MemStickTableModel(this, paths);
			table.AutoResizeMode = JTable.AUTO_RESIZE_NEXT_COLUMN;
			table.RowHeight = Constants.ICON0_HEIGHT;
			table.SelectionMode = ListSelectionModel.SINGLE_SELECTION;
			table.addMouseListener(new MouseAdapterAnonymousInnerClass(this));
			table.addKeyListener(new KeyAdapterAnonymousInnerClass(this));
			jScrollPane1.ViewportView = table;

			imagePanel.Border = javax.swing.BorderFactory.createLineBorder(new Color(0, 0, 0));
			imagePanel.PreferredSize = new java.awt.Dimension(480, 272);
			imagePanel.Layout = new java.awt.GridBagLayout();

			icon0Label.Background = new Color(255, 255, 255);
			icon0Label.Border = javax.swing.BorderFactory.createEmptyBorder(0, 22, 0, 0);
			gridBagConstraints = new java.awt.GridBagConstraints();
			gridBagConstraints.gridx = 0;
			gridBagConstraints.gridy = 0;
			gridBagConstraints.anchor = java.awt.GridBagConstraints.WEST;
			imagePanel.add(icon0Label, gridBagConstraints);

			pic0Label.Background = new Color(204, 204, 204);
			pic0Label.MaximumSize = new java.awt.Dimension(310, 180);
			pic0Label.MinimumSize = new java.awt.Dimension(310, 180);
			pic0Label.PreferredSize = new java.awt.Dimension(310, 180);
			gridBagConstraints = new java.awt.GridBagConstraints();
			gridBagConstraints.gridx = 0;
			gridBagConstraints.gridy = 0;
			gridBagConstraints.anchor = java.awt.GridBagConstraints.EAST;
			imagePanel.add(pic0Label, gridBagConstraints);

			pic1Label.Background = new Color(153, 153, 153);
			pic1Label.MaximumSize = new java.awt.Dimension(480, 272);
			pic1Label.MinimumSize = new java.awt.Dimension(480, 272);
			pic1Label.PreferredSize = new java.awt.Dimension(480, 272);
			gridBagConstraints = new java.awt.GridBagConstraints();
			gridBagConstraints.gridx = 0;
			gridBagConstraints.gridy = 0;
			imagePanel.add(pic1Label, gridBagConstraints);

			javax.swing.GroupLayout layout = new javax.swing.GroupLayout(ContentPane);
			ContentPane.Layout = layout;
			layout.HorizontalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, layout.createSequentialGroup().addContainerGap().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.TRAILING).addGroup(layout.createSequentialGroup().addGap(0, 0, short.MaxValue).addComponent(filterLabel).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(filterField, javax.swing.GroupLayout.PREFERRED_SIZE, 150, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(loadButton).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(cancelButton, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addGroup(layout.createSequentialGroup().addComponent(imagePanel, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(jScrollPane1, javax.swing.GroupLayout.DEFAULT_SIZE, 375, short.MaxValue))).addContainerGap());
			layout.VerticalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, layout.createSequentialGroup().addContainerGap().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(imagePanel, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(jScrollPane1, javax.swing.GroupLayout.PREFERRED_SIZE, 0, short.MaxValue)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(filterLabel).addComponent(filterField).addComponent(loadButton).addComponent(cancelButton, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addContainerGap());

			pack();
		} // </editor-fold>//GEN-END:initComponents

		private class ActionListenerAnonymousInnerClass : java.awt.@event.ActionListener
		{
			private readonly UmdBrowser outerInstance;

			public ActionListenerAnonymousInnerClass(UmdBrowser outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.loadButtonActionPerformed(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass : java.awt.@event.MouseAdapter
		{
			private readonly UmdBrowser outerInstance;

			public MouseAdapterAnonymousInnerClass(UmdBrowser outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void mouseClicked(MouseEvent evt)
			{
				outerInstance.tableMouseClicked(evt);
			}
		}

		private class KeyAdapterAnonymousInnerClass : java.awt.@event.KeyAdapter
		{
			private readonly UmdBrowser outerInstance;

			public KeyAdapterAnonymousInnerClass(UmdBrowser outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void keyTyped(java.awt.@event.KeyEvent evt)
			{
				outerInstance.tableKeyTyped(evt);
			}
			public override void keyPressed(java.awt.@event.KeyEvent evt)
			{
				outerInstance.tableKeyPressed(evt);
			}
			public override void keyReleased(java.awt.@event.KeyEvent evt)
			{
				outerInstance.tableKeyReleased(evt);
			}
		}

		private void tableMouseClicked(MouseEvent evt)
		{ //GEN-FIRST:event_tableMouseClicked
			if (evt.ClickCount == 2 && evt.Button == MouseEvent.BUTTON1)
			{
				loadSelectedfile();
			}
		} //GEN-LAST:event_tableMouseClicked

		private void tableKeyPressed(java.awt.@event.KeyEvent evt)
		{ //GEN-FIRST:event_tableKeyPressed
			// do nothing
		} //GEN-LAST:event_tableKeyPressed

		private void tableKeyReleased(java.awt.@event.KeyEvent evt)
		{ //GEN-FIRST:event_tableKeyReleased
			// do nothing
		} //GEN-LAST:event_tableKeyReleased

		private void tableKeyTyped(java.awt.@event.KeyEvent evt)
		{ //GEN-FIRST:event_tableKeyTyped
			scrollTo(evt.KeyChar);
		} //GEN-LAST:event_tableKeyTyped

		private void loadButtonActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_loadButtonActionPerformed
			loadSelectedfile();
		} //GEN-LAST:event_loadButtonActionPerformed
		// Variables declaration - do not modify//GEN-BEGIN:variables
		private pspsharp.GUI.CancelButton cancelButton;
		private javax.swing.JLabel icon0Label;
		private javax.swing.JPanel imagePanel;
		private javax.swing.JScrollPane jScrollPane1;
		private javax.swing.JLabel filterLabel;
		private javax.swing.JTextField filterField;
		private javax.swing.JButton loadButton;
		private javax.swing.JLabel pic0Label;
		private javax.swing.JLabel pic1Label;
		private JTable table;
		// End of variables declaration//GEN-END:variables
	}

}