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
namespace pspsharp.GUI
{


	using sceUtility = pspsharp.HLE.modules.sceUtility;
	using Settings = pspsharp.settings.Settings;

	using FolderChooser = com.jidesoft.swing.FolderChooser;
	using JpcspDialogManager = pspsharp.util.JpcspDialogManager;

	/// 
	/// <summary>
	/// @author shadow
	/// </summary>
	public class SettingsGUI : javax.swing.JFrame
	{

		private const long serialVersionUID = -732715495873159718L;
		private Settings settings;

		/// <summary>
		/// Creates new form SettingsGUI
		/// </summary>
		public SettingsGUI()
		{
			settings = Settings.Instance;

			initComponents();

			setAllComponentsFromSettings();

			lbUMDPaths.SelectionModel.addListSelectionListener(new ListSelectionListenerAnonymousInnerClass(this));

			WindowPropSaver.loadWindowProperties(this);
		}

		private class ListSelectionListenerAnonymousInnerClass : ListSelectionListener
		{
			private readonly SettingsGUI outerInstance;

			public ListSelectionListenerAnonymousInnerClass(SettingsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void valueChanged(ListSelectionEvent e)
			{
				// make sure that at least one UMD path is always in the list
				outerInstance.btnUMDPathRemove.Enabled = !((ListSelectionModel) e.Source).SelectionEmpty && outerInstance.lbUMDPaths.Model.Size > 1;
			}
		}

		private void setAllComponentsFromSettings()
		{
			setBoolFromSettings(pbpunpackcheck, "emu.pbpunpack");
			setBoolFromSettings(saveWindowPosCheck, "gui.saveWindowPos");
			setBoolFromSettings(fullscreenCheck, "gui.fullscreen");
			setBoolFromSettings(useCompiler, "emu.compiler");
			setBoolFromSettings(profilerCheck, "emu.profiler");
			setBoolFromSettings(shadersCheck, "emu.useshaders");
			setBoolFromSettings(geometryShaderCheck, "emu.useGeometryShader");
			setBoolFromSettings(loadAndRunCheck, "emu.loadAndRun");
			setIntFromSettings(languageBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_LANGUAGE);
			setIntFromSettings(buttonBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_BUTTON_PREFERENCE);
			setIntFromSettings(daylightBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_DAYLIGHT_SAVING_TIME);
			setIntFromSettings(timezoneSpinner, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_TIME_ZONE);
			setIntFromSettings(timeFormatBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_TIME_FORMAT);
			setIntFromSettings(dateFormatBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_DATE_FORMAT);
			setIntFromSettings(wlanPowerBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_WLAN_POWER_SAVE);
			setIntFromSettings(adhocChannelBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_ADHOC_CHANNEL);
			setStringFromSettings(nicknameTextField, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_NICKNAME);
			setBoolFromSettings(disableVBOCheck, "emu.disablevbo");
			setBoolFromSettings(disableUBOCheck, "emu.disableubo");
			setBoolFromSettings(enableVAOCheck, "emu.enablevao");
			setBoolFromSettings(enableGETextureCheck, "emu.enablegetexture");
			setBoolFromSettings(enableNativeCLUTCheck, "emu.enablenativeclut");
			setBoolFromSettings(enableDynamicShadersCheck, "emu.enabledynamicshaders");
			setBoolFromSettings(enableShaderStencilTestCheck, "emu.enableshaderstenciltest");
			setBoolFromSettings(enableShaderColorMaskCheck, "emu.enableshadercolormask");
			setBoolFromSettings(disableOptimizedVertexInfoReading, "emu.disableoptimizedvertexinforeading");
			setBoolFromSettings(saveStencilToMemory, "emu.saveStencilToMemory");
			setBoolFromSettings(useSoftwareRenderer, "emu.useSoftwareRenderer");
			setBoolFromSettings(useExternalSoftwareRenderer, "emu.useExternalSoftwareRenderer");
			//set opengl render selected if none of the other renders are selected
			if (!useSoftwareRenderer.Selected && !useExternalSoftwareRenderer.Selected)
			{
				useOpenglRenderer.Selected = true;
			}

			setBoolFromSettings(onlyGEGraphicsCheck, "emu.onlyGEGraphics");
			setBoolFromSettings(useDebugFont, "emu.useDebugFont");
			setBoolFromSettings(useDebugMemory, "emu.useDebuggerMemory");
			setBoolFromSettings(useVertexCache, "emu.useVertexCache");
			setBoolFromSettings(invalidMemoryCheck, "emu.ignoreInvalidMemoryAccess");
			setBoolFromSettings(ignoreUnmappedImports, "emu.ignoreUnmappedImports");
			setIntAsStringFromSettings(methodMaxInstructionsBox, "emu.compiler.methodMaxInstructions", 3000);
			setBoolFromSettings(extractEboot, "emu.extractEboot");
			setBoolFromSettings(cryptoSavedata, "emu.cryptoSavedata");
			setBoolFromSettings(extractPGD, "emu.extractPGD");
			setBoolFromSettings(extractSavedataKey, "emu.extractSavedataKey");
			setBoolFromSettings(disableDLC, "emu.disableDLC");
			setStringFromSettings(antiAliasingBox, "emu.graphics.antialias");
			setStringFromSettings(resolutionBox, "emu.graphics.resolution");
			setStringFromSettings(tmppath, "emu.tmppath");
			setIntFromSettings(modelBox, "emu.model");
			setBoolFromSettings(umdBrowser, classicUmdDialog, "emu.umdbrowser");
			setStringFromSettings(metaServerTextField, "network.ProOnline.metaServer");
			setStringFromSettings(broadcastAddressTextField, "network.broadcastAddress");
			setBoolFromSettings(lanMultiPlayerRadioButton, "emu.lanMultiPlayer");
			setBoolFromSettings(netServerPortShiftRadioButton, "emu.netServerPortShift");
			setBoolFromSettings(netClientPortShiftRadioButton, "emu.netClientPortShift");
			setBoolFromSettings(enableProOnlineRadioButton, "emu.enableProOnline");

			// special handling for UMD paths
			DefaultListModel dlm = (DefaultListModel) lbUMDPaths.Model;
			dlm.clear();
			dlm.addElement(settings.readString("emu.umdpath"));
			for (int i = 1; true; i++)
			{
				string umdPath = settings.readString(string.Format("emu.umdpath.{0:D}", i), null);
				if (string.ReferenceEquals(umdPath, null))
				{
					break;
				}

				// elements should only be added once
				if (!dlm.contains(umdPath))
				{
					dlm.addElement(umdPath);
				}
			}
		}

		private bool isEnabledSettings(string settingsOption)
		{
			return !settings.isOptionFromPatch(settingsOption);
		}

		private void setBoolFromSettings(JRadioButton trueButton, JRadioButton falseButton, string settingsOption)
		{
			bool value = settings.readBool(settingsOption);
			trueButton.Selected = value;
			falseButton.Selected = !value;
			trueButton.Enabled = isEnabledSettings(settingsOption);
			falseButton.Enabled = isEnabledSettings(settingsOption);
		}
	   private void setBoolFromSettings(JRadioButton radioButton, string settingsOption)
	   {
			radioButton.Selected = settings.readBool(settingsOption);
			radioButton.Enabled = isEnabledSettings(settingsOption);
	   }


		private void setBoolFromSettings(JCheckBox checkBox, string settingsOption)
		{
			checkBox.Selected = settings.readBool(settingsOption);
			checkBox.Enabled = isEnabledSettings(settingsOption);
		}

		private void setIntFromSettings(JComboBox comboBox, string settingsOption)
		{
			comboBox.SelectedIndex = settings.readInt(settingsOption);
			comboBox.Enabled = isEnabledSettings(settingsOption);
		}

		private void setIntAsStringFromSettings(JComboBox comboBox, string settingsOption, int defaultValue)
		{
			comboBox.SelectedItem = Convert.ToString(settings.readInt(settingsOption, defaultValue));
			comboBox.Enabled = isEnabledSettings(settingsOption);
		}

		private void setIntFromSettings(JSpinner spinner, string settingsOption)
		{
			spinner.Value = settings.readInt(settingsOption);
			spinner.Enabled = isEnabledSettings(settingsOption);
		}

		private void setStringFromSettings(JComboBox comboBox, string settingsOption)
		{
			comboBox.SelectedItem = settings.readString(settingsOption);
			comboBox.Enabled = isEnabledSettings(settingsOption);
		}

		private void setStringFromSettings(JTextField textField, string settingsOption)
		{
			textField.Text = settings.readString(settingsOption);
			textField.Enabled = isEnabledSettings(settingsOption);
		}

		private void setAllComponentsToSettings()
		{
			setBoolToSettings(pbpunpackcheck, "emu.pbpunpack");
			setBoolToSettings(saveWindowPosCheck, "gui.saveWindowPos");
			setBoolToSettings(fullscreenCheck, "gui.fullscreen");
			setBoolToSettings(useCompiler, "emu.compiler");
			setBoolToSettings(profilerCheck, "emu.profiler");
			setBoolToSettings(shadersCheck, "emu.useshaders");
			setBoolToSettings(geometryShaderCheck, "emu.useGeometryShader");
			setBoolToSettings(loadAndRunCheck, "emu.loadAndRun");
			setIntToSettings(languageBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_LANGUAGE);
			setIntToSettings(buttonBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_BUTTON_PREFERENCE);
			setIntToSettings(daylightBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_DAYLIGHT_SAVING_TIME);
			setIntToSettings(timezoneSpinner, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_TIME_ZONE);
			setIntToSettings(timeFormatBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_TIME_FORMAT);
			setIntToSettings(dateFormatBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_DATE_FORMAT);
			setIntToSettings(wlanPowerBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_WLAN_POWER_SAVE);
			setIntToSettings(adhocChannelBox, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_ADHOC_CHANNEL);
			setStringToSettings(nicknameTextField, sceUtility.SYSTEMPARAM_SETTINGS_OPTION_NICKNAME);
			setBoolToSettings(disableVBOCheck, "emu.disablevbo");
			setBoolToSettings(disableUBOCheck, "emu.disableubo");
			setBoolToSettings(enableVAOCheck, "emu.enablevao");
			setBoolToSettings(enableGETextureCheck, "emu.enablegetexture");
			setBoolToSettings(enableNativeCLUTCheck, "emu.enablenativeclut");
			setBoolToSettings(enableDynamicShadersCheck, "emu.enabledynamicshaders");
			setBoolToSettings(enableShaderStencilTestCheck, "emu.enableshaderstenciltest");
			setBoolToSettings(enableShaderColorMaskCheck, "emu.enableshadercolormask");
			setBoolToSettings(disableOptimizedVertexInfoReading, "emu.disableoptimizedvertexinforeading");
			setBoolToSettings(saveStencilToMemory, "emu.saveStencilToMemory");
			setBoolToSettings(useSoftwareRenderer, "emu.useSoftwareRenderer");
			setBoolToSettings(useExternalSoftwareRenderer, "emu.useExternalSoftwareRenderer");
			//set opengl render selected if none of the other renders are selected
			if (!useSoftwareRenderer.Selected && !useExternalSoftwareRenderer.Selected)
			{
				useOpenglRenderer.Selected = true;
			}

			setBoolToSettings(onlyGEGraphicsCheck, "emu.onlyGEGraphics");
			setBoolToSettings(useDebugFont, "emu.useDebugFont");
			setBoolToSettings(useDebugMemory, "emu.useDebuggerMemory");
			setBoolToSettings(useVertexCache, "emu.useVertexCache");
			setBoolToSettings(invalidMemoryCheck, "emu.ignoreInvalidMemoryAccess");
			setBoolToSettings(ignoreUnmappedImports, "emu.ignoreUnmappedImports");
			setIntAsStringToSettings(methodMaxInstructionsBox, "emu.compiler.methodMaxInstructions", 3000);
			setBoolToSettings(extractEboot, "emu.extractEboot");
			setBoolToSettings(cryptoSavedata, "emu.cryptoSavedata");
			setBoolToSettings(extractPGD, "emu.extractPGD");
			setBoolToSettings(extractSavedataKey, "emu.extractSavedataKey");
			setBoolToSettings(disableDLC, "emu.disableDLC");
			setStringToSettings(antiAliasingBox, "emu.graphics.antialias");
			setStringToSettings(resolutionBox, "emu.graphics.resolution");
			setStringToSettings(tmppath, "emu.tmppath");
			setIntToSettings(modelBox, "emu.model");
			setBoolToSettings(umdBrowser, "emu.umdbrowser");
			setStringToSettings(metaServerTextField, "network.ProOnline.metaServer");
			setStringToSettings(broadcastAddressTextField,"network.broadcastAddress");
			setBoolToSettings(lanMultiPlayerRadioButton, "emu.lanMultiPlayer");
			setBoolToSettings(netServerPortShiftRadioButton, "emu.netServerPortShift");
			setBoolToSettings(netClientPortShiftRadioButton, "emu.netClientPortShift");
			setBoolToSettings(enableProOnlineRadioButton, "emu.enableProOnline");

			// special handling for UMD paths
			DefaultListModel dlm = (DefaultListModel) lbUMDPaths.Model;
			settings.writeString("emu.umdpath", (string) dlm.getElementAt(0));
			for (int i = 1; i < dlm.Size; i++)
			{
				settings.writeString(string.Format("emu.umdpath.{0:D}", i), (string) dlm.getElementAt(i));
			}

			// clean excess elements
			for (int i = dlm.Size; true; i++)
			{
				if (settings.hasProperty(string.Format("emu.umdpath.{0:D}", i)))
				{
					settings.clearProperty(string.Format("emu.umdpath.{0:D}", i));
				}
				else
				{
					break;
				}
			}
		}

		private void setBoolToSettings(JRadioButton radioButton, string settingsOption)
		{
			if (isEnabledSettings(settingsOption))
			{
				settings.writeBool(settingsOption, radioButton.Selected);
			}
		}

		private void setBoolToSettings(JCheckBox checkBox, string settingsOption)
		{
			if (isEnabledSettings(settingsOption))
			{
				settings.writeBool(settingsOption, checkBox.Selected);
			}
		}

		private void setIntToSettings(JComboBox comboBox, string settingsOption)
		{
			if (isEnabledSettings(settingsOption))
			{
				settings.writeInt(settingsOption, comboBox.SelectedIndex);
			}
		}

		private void setIntAsStringToSettings(JComboBox comboBox, string settingsOption, int defaultValue)
		{
			if (isEnabledSettings(settingsOption))
			{
				settings.writeInt(settingsOption, int.Parse(comboBox.SelectedItem.ToString()));
			}
		}

		private void setIntToSettings(JSpinner spinner, string settingsOption)
		{
			if (isEnabledSettings(settingsOption))
			{
				settings.writeInt(settingsOption, int.Parse(spinner.Value.ToString()));
			}
		}

		private void setStringToSettings(JComboBox comboBox, string settingsOption)
		{
			if (isEnabledSettings(settingsOption))
			{
				settings.writeString(settingsOption, comboBox.SelectedItem.ToString());
			}
		}

		private void setStringToSettings(JTextField textField, string settingsOption)
		{
			if (isEnabledSettings(settingsOption))
			{
				settings.writeString(settingsOption, textField.Text);
			}
		}

		private ComboBoxModel makeComboBoxModel(string[] items)
		{
			MutableComboBoxModel comboBox = new DefaultComboBoxModel();
			foreach (string item in items)
			{
				comboBox.addElement(item);
			}
			return comboBox;
		}

		public static string[] ImposeLanguages
		{
			get
			{
				return new string[]{"Japanese", "English", "French", "Spanish", "German", "Italian", "Dutch", "Portuguese", "Russian", "Korean", "Traditional Chinese", "Simplfied Chinese"};
			}
		}

		public static string[] ImposeButtons
		{
			get
			{
				return new string[]{"\"O\" for \"Enter\"", "\"X\" for \"Enter\""};
			}
		}

		public static string[] SysparamDaylightSavings
		{
			get
			{
				return new string[]{"Off", "On"};
			}
		}

		public static string[] SysparamTimeFormats
		{
			get
			{
				return new string[]{"24H", "12H"};
			}
		}

		public static string[] SysparamDateFormats
		{
			get
			{
				return new string[]{"YYYY/M/D", "M/D/YYYY", "D/M/YYYY"};
			}
		}

		public static string[] SysparamWlanPowerSaves
		{
			get
			{
				return new string[]{"Off", "On"};
			}
		}

		public static string[] SysparamAdhocChannels
		{
			get
			{
				return new string[]{"Auto", "1", "6", "11"};
			}
		}

		private ComboBoxModel makeResolutions()
		{
			MutableComboBoxModel comboBox = new DefaultComboBoxModel();
			comboBox.addElement("Native");

			ISet<string> resolutions = new HashSet<string>();
			GraphicsDevice localDevice = GraphicsEnvironment.LocalGraphicsEnvironment.DefaultScreenDevice;
			DisplayMode[] displayModes = localDevice.DisplayModes;
			for (int i = 0; displayModes != null && i < displayModes.Length; i++)
			{
				DisplayMode displayMode = displayModes[i];
				if (displayMode.BitDepth == MainGUI.displayModeBitDepth)
				{
					string resolution = string.Format("{0:D}x{1:D}", displayMode.Width, displayMode.Height);
					if (!resolutions.Contains(resolution))
					{
						comboBox.addElement(resolution);
						resolutions.Add(resolution);
					}
				}
			}

			return comboBox;
		}

		/// <summary>
		/// This method is called from within the constructor to initialize the form.
		/// WARNING: Do NOT modify this code. The content of this method is always
		/// regenerated by the Form Editor.
		/// </summary>
		// <editor-fold defaultstate="collapsed" desc="Generated Code">//GEN-BEGIN:initComponents
		private void initComponents()
		{

			buttonGroup1 = new javax.swing.ButtonGroup();
			buttonGroup2 = new javax.swing.ButtonGroup();
			buttonGroup3 = new javax.swing.ButtonGroup();
			jButtonOK = new javax.swing.JButton();
			jButtonApply = new javax.swing.JButton();
			jTabbedPane1 = new javax.swing.JTabbedPane();
			GeneralPanel = new javax.swing.JPanel();
			pbpunpackcheck = new JCheckBox();
			saveWindowPosCheck = new JCheckBox();
			loadAndRunCheck = new JCheckBox();
			umdBrowser = new JRadioButton();
			classicUmdDialog = new JRadioButton();
			umdPathLabel = new javax.swing.JLabel();
			tmpPathLabel = new javax.swing.JLabel();
			tmppath = new JTextField();
			tmpPathBrowseButton = new javax.swing.JButton();
			jScrollPane1 = new javax.swing.JScrollPane();
			lbUMDPaths = new javax.swing.JList();
			btnUMDPathRemove = new javax.swing.JButton();
			btnUMDPathAdd = new javax.swing.JButton();
			modelLabel = new javax.swing.JLabel();
			modelBox = new JComboBox();
			RegionPanel = new javax.swing.JPanel();
			jPanel1 = new javax.swing.JPanel();
			imposeLabel = new javax.swing.JLabel();
			jPanel2 = new javax.swing.JPanel();
			languageLabel = new javax.swing.JLabel();
			languageBox = new JComboBox();
			buttonLabel = new javax.swing.JLabel();
			buttonBox = new JComboBox();
			jPanel4 = new javax.swing.JPanel();
			jPanel5 = new javax.swing.JPanel();
			sysParmLabel = new javax.swing.JLabel();
			jPanel3 = new javax.swing.JPanel();
			daylightLabel = new javax.swing.JLabel();
			daylightBox = new JComboBox();
			timeFormatLabel = new javax.swing.JLabel();
			timeFormatBox = new JComboBox();
			dateFormatLabel = new javax.swing.JLabel();
			dateFormatBox = new JComboBox();
			wlanPowerLabel = new javax.swing.JLabel();
			wlanPowerBox = new JComboBox();
			adhocChannelLabel = new javax.swing.JLabel();
			adhocChannelBox = new JComboBox();
			timezoneLabel = new javax.swing.JLabel();
			timezoneSpinner = new JSpinner();
			nicknamelLabel = new javax.swing.JLabel();
			nicknameTextField = new JTextField();
			VideoPanel = new javax.swing.JPanel();
			disableVBOCheck = new JCheckBox();
			onlyGEGraphicsCheck = new JCheckBox();
			useVertexCache = new JCheckBox();
			shadersCheck = new JCheckBox();
			geometryShaderCheck = new JCheckBox();
			disableUBOCheck = new JCheckBox();
			enableVAOCheck = new JCheckBox();
			enableGETextureCheck = new JCheckBox();
			enableNativeCLUTCheck = new JCheckBox();
			enableDynamicShadersCheck = new JCheckBox();
			enableShaderStencilTestCheck = new JCheckBox();
			enableShaderColorMaskCheck = new JCheckBox();
			disableOptimizedVertexInfoReading = new JCheckBox();
			saveStencilToMemory = new JCheckBox();
			renderPanel = new javax.swing.JPanel();
			useOpenglRenderer = new JRadioButton();
			useSoftwareRenderer = new JRadioButton();
			useExternalSoftwareRenderer = new JRadioButton();
			MemoryPanel = new javax.swing.JPanel();
			invalidMemoryCheck = new JCheckBox();
			ignoreUnmappedImports = new JCheckBox();
			useDebugMemory = new JCheckBox();
			CompilerPanel = new javax.swing.JPanel();
			useCompiler = new JCheckBox();
			methodMaxInstructionsBox = new JComboBox();
			profilerCheck = new JCheckBox();
			methodMaxInstructionsLabel = new javax.swing.JLabel();
			DisplayPanel = new javax.swing.JPanel();
			antiAliasLabel = new javax.swing.JLabel();
			antiAliasingBox = new JComboBox();
			resolutionLabel = new javax.swing.JLabel();
			resolutionBox = new JComboBox();
			fullscreenCheck = new JCheckBox();
			MiscPanel = new javax.swing.JPanel();
			useDebugFont = new JCheckBox();
			CryptoPanel = new javax.swing.JPanel();
			extractEboot = new JCheckBox();
			cryptoSavedata = new JCheckBox();
			extractPGD = new JCheckBox();
			extractSavedataKey = new JCheckBox();
			disableDLC = new JCheckBox();
			networkPanel = new javax.swing.JPanel();
			lanMultiPlayerRadioButton = new JRadioButton();
			netServerPortShiftRadioButton = new JRadioButton();
			netClientPortShiftRadioButton = new JRadioButton();
			enableProOnlineRadioButton = new JRadioButton();
			lanMultiPlayerLabel = new javax.swing.JLabel();
			netServerPortShiftLabel = new javax.swing.JLabel();
			netClientPortShiftLabel = new javax.swing.JLabel();
			enableProOnlineLabel = new javax.swing.JLabel();
			metaServerLabel = new javax.swing.JLabel();
			metaServerTextField = new JTextField();
			metaServerRemindLabel = new javax.swing.JLabel();
			broadcastAddressLabel = new javax.swing.JLabel();
			broadcastAddressTextField = new JTextField();
			broadcastAddressRemindLabel = new javax.swing.JLabel();
			cancelButton = new pspsharp.GUI.CancelButton();

			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			Title = bundle.getString("SettingsGUI.title"); // NOI18N

			jButtonOK.Text = bundle.getString("OkButton.text"); // NOI18N
			jButtonOK.addActionListener(new ActionListenerAnonymousInnerClass(this));

			jButtonApply.Text = bundle.getString("SettingsGUI.jButtonApply.text"); // NOI18N
			jButtonApply.addActionListener(new ActionListenerAnonymousInnerClass2(this));

			pbpunpackcheck.Text = bundle.getString("SettingsGUI.pbpunpackcheck.text"); // NOI18N

			saveWindowPosCheck.Text = bundle.getString("SettingsGUI.saveWindowPosCheck.text"); // NOI18N

			loadAndRunCheck.Text = bundle.getString("SettingsGUI.loadAndRunCheck.text"); // NOI18N

			buttonGroup1.add(umdBrowser);
			umdBrowser.Text = bundle.getString("SettingsGUI.umdBrowser.text"); // NOI18N

			buttonGroup1.add(classicUmdDialog);
			classicUmdDialog.Text = bundle.getString("SettingsGUI.classicUmdDialog.text"); // NOI18N

			umdPathLabel.Text = bundle.getString("SettingsGUI.umdPathLabel.text"); // NOI18N

			tmpPathLabel.Text = bundle.getString("SettingsGUI.tmpPathLabel.text"); // NOI18N

			tmppath.Editable = false;
			tmppath.Text = "tmp"; // NOI18N

			tmpPathBrowseButton.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/AddFolderIcon.png")); // NOI18N
			tmpPathBrowseButton.PreferredSize = new java.awt.Dimension(26, 26);
			tmpPathBrowseButton.addActionListener(new ActionListenerAnonymousInnerClass3(this));

			lbUMDPaths.Font = new java.awt.Font("Dialog", 0, 12); // NOI18N
			lbUMDPaths.Model = new DefaultListModel();
			lbUMDPaths.SelectionMode = ListSelectionModel.SINGLE_SELECTION;
			jScrollPane1.ViewportView = lbUMDPaths;

			btnUMDPathRemove.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/DeleteIcon.png")); // NOI18N
			btnUMDPathRemove.Enabled = false;
			btnUMDPathRemove.PreferredSize = new java.awt.Dimension(26, 26);
			btnUMDPathRemove.addActionListener(new ActionListenerAnonymousInnerClass4(this));

			btnUMDPathAdd.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/AddFolderIcon.png")); // NOI18N
			btnUMDPathAdd.PreferredSize = new java.awt.Dimension(26, 26);
			btnUMDPathAdd.addActionListener(new ActionListenerAnonymousInnerClass5(this));

			modelLabel.Text = bundle.getString("SettingsGUI.modelLabel.text"); // NOI18N

			modelBox.Model = new DefaultComboBoxModel(new string[] {"PSP-1000", "PSP-2000", "PSP-3000", "PSP-3000 (V2)", "PSP-N1000 (GO)"});

			javax.swing.GroupLayout GeneralPanelLayout = new javax.swing.GroupLayout(GeneralPanel);
			GeneralPanel.Layout = GeneralPanelLayout;
			GeneralPanelLayout.HorizontalGroup = GeneralPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(GeneralPanelLayout.createSequentialGroup().addContainerGap().addGroup(GeneralPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(GeneralPanelLayout.createSequentialGroup().addComponent(pbpunpackcheck, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(431, 431, 431)).addGroup(GeneralPanelLayout.createSequentialGroup().addComponent(saveWindowPosCheck, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(493, 493, 493)).addGroup(GeneralPanelLayout.createSequentialGroup().addComponent(loadAndRunCheck, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(489, 489, 489)).addGroup(GeneralPanelLayout.createSequentialGroup().addComponent(umdBrowser, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(582, 582, 582)).addGroup(GeneralPanelLayout.createSequentialGroup().addComponent(classicUmdDialog, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(482, 482, 482)).addGroup(GeneralPanelLayout.createSequentialGroup().addGroup(GeneralPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(umdPathLabel).addComponent(tmpPathLabel).addComponent(modelLabel)).addGap(21, 21, 21).addGroup(GeneralPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(GeneralPanelLayout.createSequentialGroup().addComponent(modelBox, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(0, 0, short.MaxValue)).addGroup(GeneralPanelLayout.createSequentialGroup().addGroup(GeneralPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING, false).addComponent(jScrollPane1, javax.swing.GroupLayout.DEFAULT_SIZE, 385, short.MaxValue).addComponent(tmppath)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(GeneralPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(tmpPathBrowseButton, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(btnUMDPathRemove, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(btnUMDPathAdd, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue))))));
			GeneralPanelLayout.VerticalGroup = GeneralPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(GeneralPanelLayout.createSequentialGroup().addContainerGap().addComponent(pbpunpackcheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(saveWindowPosCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(loadAndRunCheck).addGap(18, 18, 18).addComponent(umdBrowser).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(classicUmdDialog).addGap(18, 18, 18).addGroup(GeneralPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING, false).addGroup(GeneralPanelLayout.createSequentialGroup().addComponent(umdPathLabel).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(btnUMDPathAdd, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(btnUMDPathRemove, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addComponent(jScrollPane1, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addGap(18, 18, 18).addGroup(GeneralPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(GeneralPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(tmppath, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(tmpPathLabel)).addComponent(tmpPathBrowseButton, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addGap(22, 22, 22).addGroup(GeneralPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(modelLabel).addComponent(modelBox, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addContainerGap(116, short.MaxValue));

			jTabbedPane1.addTab(bundle.getString("SettingsGUI.GeneralPanel.title"), GeneralPanel); // NOI18N

			jPanel1.Layout = new java.awt.GridLayout(20, 2, 10, 0);

			imposeLabel.Text = bundle.getString("SettingsGUI.imposeLabel.text"); // NOI18N
			jPanel1.add(imposeLabel);

			javax.swing.GroupLayout jPanel2Layout = new javax.swing.GroupLayout(jPanel2);
			jPanel2.Layout = jPanel2Layout;
			jPanel2Layout.HorizontalGroup = jPanel2Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGap(0, 758, short.MaxValue);
			jPanel2Layout.VerticalGroup = jPanel2Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGap(0, 23, short.MaxValue);

			jPanel1.add(jPanel2);

			languageLabel.Text = bundle.getString("SettingsGUI.languageLabel.text"); // NOI18N
			jPanel1.add(languageLabel);

			languageBox.Model = makeComboBoxModel(ImposeLanguages);
			jPanel1.add(languageBox);

			buttonLabel.Text = bundle.getString("SettingsGUI.buttonLabel.text"); // NOI18N
			jPanel1.add(buttonLabel);

			buttonBox.Model = makeComboBoxModel(ImposeButtons);
			jPanel1.add(buttonBox);

			javax.swing.GroupLayout jPanel4Layout = new javax.swing.GroupLayout(jPanel4);
			jPanel4.Layout = jPanel4Layout;
			jPanel4Layout.HorizontalGroup = jPanel4Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGap(0, 758, short.MaxValue);
			jPanel4Layout.VerticalGroup = jPanel4Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGap(0, 23, short.MaxValue);

			jPanel1.add(jPanel4);

			javax.swing.GroupLayout jPanel5Layout = new javax.swing.GroupLayout(jPanel5);
			jPanel5.Layout = jPanel5Layout;
			jPanel5Layout.HorizontalGroup = jPanel5Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGap(0, 758, short.MaxValue);
			jPanel5Layout.VerticalGroup = jPanel5Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGap(0, 23, short.MaxValue);

			jPanel1.add(jPanel5);

			sysParmLabel.Text = bundle.getString("SettingsGUI.sysParmLabel.text"); // NOI18N
			jPanel1.add(sysParmLabel);

			javax.swing.GroupLayout jPanel3Layout = new javax.swing.GroupLayout(jPanel3);
			jPanel3.Layout = jPanel3Layout;
			jPanel3Layout.HorizontalGroup = jPanel3Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGap(0, 758, short.MaxValue);
			jPanel3Layout.VerticalGroup = jPanel3Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGap(0, 23, short.MaxValue);

			jPanel1.add(jPanel3);

			daylightLabel.Text = bundle.getString("SettingsGUI.daylightLabel.text"); // NOI18N
			jPanel1.add(daylightLabel);

			daylightBox.Model = makeComboBoxModel(SysparamDaylightSavings);
			jPanel1.add(daylightBox);

			timeFormatLabel.Text = bundle.getString("SettingsGUI.timeFormatLabel.text"); // NOI18N
			jPanel1.add(timeFormatLabel);

			timeFormatBox.Model = makeComboBoxModel(SysparamTimeFormats);
			jPanel1.add(timeFormatBox);

			dateFormatLabel.Text = bundle.getString("SettingsGUI.dateFormatLabel.text"); // NOI18N
			jPanel1.add(dateFormatLabel);

			dateFormatBox.Model = makeComboBoxModel(SysparamDateFormats);
			jPanel1.add(dateFormatBox);

			wlanPowerLabel.Text = bundle.getString("SettingsGUI.wlanPowerLabel.text"); // NOI18N
			jPanel1.add(wlanPowerLabel);

			wlanPowerBox.Model = makeComboBoxModel(SysparamWlanPowerSaves);
			jPanel1.add(wlanPowerBox);

			adhocChannelLabel.Text = bundle.getString("SettingsGUI.adhocChannel.text"); // NOI18N
			jPanel1.add(adhocChannelLabel);

			adhocChannelBox.Model = new DefaultComboBoxModel(new string[] {"Auto", "1", "6", "11"});
			jPanel1.add(adhocChannelBox);

			timezoneLabel.Text = bundle.getString("SettingsGUI.timezoneLabel.text"); // NOI18N
			jPanel1.add(timezoneLabel);

			timezoneSpinner.Model = new javax.swing.SpinnerNumberModel(0, -720, 720, 1);
			jPanel1.add(timezoneSpinner);

			nicknamelLabel.Text = bundle.getString("SettingsGUI.nicknameLabel.text"); // NOI18N
			jPanel1.add(nicknamelLabel);

			nicknameTextField.HorizontalAlignment = JTextField.RIGHT;
			nicknameTextField.Text = "pspsharp"; // NOI18N
			jPanel1.add(nicknameTextField);

			javax.swing.GroupLayout RegionPanelLayout = new javax.swing.GroupLayout(RegionPanel);
			RegionPanel.Layout = RegionPanelLayout;
			RegionPanelLayout.HorizontalGroup = RegionPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(RegionPanelLayout.createSequentialGroup().addContainerGap().addComponent(jPanel1, javax.swing.GroupLayout.DEFAULT_SIZE, 1527, short.MaxValue).addContainerGap());
			RegionPanelLayout.VerticalGroup = RegionPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(RegionPanelLayout.createSequentialGroup().addContainerGap().addComponent(jPanel1, javax.swing.GroupLayout.PREFERRED_SIZE, 468, javax.swing.GroupLayout.PREFERRED_SIZE).addContainerGap());

			jTabbedPane1.addTab(bundle.getString("SettingsGUI.RegionPanel.title"), RegionPanel); // NOI18N

			disableVBOCheck.Text = bundle.getString("SettingsGUI.disableVBOCheck.text"); // NOI18N

			onlyGEGraphicsCheck.Text = bundle.getString("SettingsGUI.onlyGEGraphicsCheck.text"); // NOI18N

			useVertexCache.Text = bundle.getString("SettingsGUI.useVertexCache.text"); // NOI18N

			shadersCheck.Text = bundle.getString("SettingsGUI.shaderCheck.text"); // NOI18N

			geometryShaderCheck.Text = bundle.getString("SettingsGUI.geometryShaderCheck.text"); // NOI18N

			disableUBOCheck.Text = bundle.getString("SettingsGUI.disableUBOCheck.text"); // NOI18N

			enableVAOCheck.Text = bundle.getString("SettingsGUI.enableVAOCheck.text"); // NOI18N

			enableGETextureCheck.Text = bundle.getString("SettingsGUI.enableGETextureCheck.text"); // NOI18N

			enableNativeCLUTCheck.Text = bundle.getString("SettingsGUI.enableNativeCLUTCheck.text"); // NOI18N

			enableDynamicShadersCheck.Text = bundle.getString("SettingsGUI.enableDynamicShadersCheck.text"); // NOI18N

			enableShaderStencilTestCheck.Text = bundle.getString("SettingsGUI.enableShaderStencilTestCheck.text"); // NOI18N

			enableShaderColorMaskCheck.Text = bundle.getString("SettingsGUI.enableShaderColorMaskCheck.text"); // NOI18N

			disableOptimizedVertexInfoReading.Text = bundle.getString("SettingsGUI.disableOptimizedVertexInfoReading.text"); // NOI18N

			saveStencilToMemory.Text = bundle.getString("SettingsGUI.saveStencilToMemory.text"); // NOI18N

			renderPanel.Border = javax.swing.BorderFactory.createTitledBorder(null, bundle.getString("SettingsGUI.renderPanel.border.title"), javax.swing.border.TitledBorder.DEFAULT_JUSTIFICATION, javax.swing.border.TitledBorder.DEFAULT_POSITION, null, new java.awt.Color(51, 51, 255)); // NOI18N

			buttonGroup2.add(useOpenglRenderer);
			useOpenglRenderer.Text = bundle.getString("SettingsGUI.useOpenglRenderer.text"); // NOI18N

			buttonGroup2.add(useSoftwareRenderer);
			useSoftwareRenderer.Text = bundle.getString("SettingsGUI.useSoftwareRenderer.text"); // NOI18N

			buttonGroup2.add(useExternalSoftwareRenderer);
			useExternalSoftwareRenderer.Text = bundle.getString("SettingsGUI.useExternalSoftwareRenderer.text"); // NOI18N

			javax.swing.GroupLayout renderPanelLayout = new javax.swing.GroupLayout(renderPanel);
			renderPanel.Layout = renderPanelLayout;
			renderPanelLayout.HorizontalGroup = renderPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(renderPanelLayout.createSequentialGroup().addGroup(renderPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(useExternalSoftwareRenderer).addComponent(useOpenglRenderer).addComponent(useSoftwareRenderer)).addGap(0, 437, short.MaxValue));
			renderPanelLayout.VerticalGroup = renderPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, renderPanelLayout.createSequentialGroup().addContainerGap().addComponent(useOpenglRenderer).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(useSoftwareRenderer).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(useExternalSoftwareRenderer).addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue));

			javax.swing.GroupLayout VideoPanelLayout = new javax.swing.GroupLayout(VideoPanel);
			VideoPanel.Layout = VideoPanelLayout;
			VideoPanelLayout.HorizontalGroup = VideoPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(VideoPanelLayout.createSequentialGroup().addContainerGap().addGroup(VideoPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(VideoPanelLayout.createSequentialGroup().addComponent(renderPanel, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addContainerGap()).addGroup(VideoPanelLayout.createSequentialGroup().addGroup(VideoPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(VideoPanelLayout.createSequentialGroup().addComponent(disableVBOCheck, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(315, 315, 315)).addGroup(VideoPanelLayout.createSequentialGroup().addComponent(onlyGEGraphicsCheck, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(331, 331, 331)).addGroup(VideoPanelLayout.createSequentialGroup().addComponent(useVertexCache, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(575, 575, 575)).addGroup(VideoPanelLayout.createSequentialGroup().addComponent(shadersCheck, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(606, 606, 606)).addComponent(geometryShaderCheck, javax.swing.GroupLayout.PREFERRED_SIZE, 720, javax.swing.GroupLayout.PREFERRED_SIZE).addGroup(VideoPanelLayout.createSequentialGroup().addComponent(disableUBOCheck, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(64, 64, 64)).addGroup(VideoPanelLayout.createSequentialGroup().addComponent(enableVAOCheck, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(235, 235, 235)).addGroup(VideoPanelLayout.createSequentialGroup().addComponent(enableGETextureCheck, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(304, 304, 304)).addGroup(VideoPanelLayout.createSequentialGroup().addComponent(enableNativeCLUTCheck, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(18, 18, 18)).addComponent(enableDynamicShadersCheck, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGroup(VideoPanelLayout.createSequentialGroup().addComponent(enableShaderStencilTestCheck, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(103, 103, 103)).addGroup(VideoPanelLayout.createSequentialGroup().addComponent(enableShaderColorMaskCheck, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(108, 108, 108)).addGroup(VideoPanelLayout.createSequentialGroup().addComponent(disableOptimizedVertexInfoReading, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(234, 234, 234)).addGroup(VideoPanelLayout.createSequentialGroup().addComponent(saveStencilToMemory, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGap(334, 334, 334))).addGap(17, 17, 17))));
			VideoPanelLayout.VerticalGroup = VideoPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, VideoPanelLayout.createSequentialGroup().addContainerGap().addComponent(renderPanel, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addComponent(disableVBOCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(onlyGEGraphicsCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(useVertexCache).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(shadersCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(geometryShaderCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(disableUBOCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(enableVAOCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(enableGETextureCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(enableNativeCLUTCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(enableDynamicShadersCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(enableShaderStencilTestCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(enableShaderColorMaskCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(disableOptimizedVertexInfoReading).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(saveStencilToMemory).addContainerGap(51, short.MaxValue));

			jTabbedPane1.addTab(bundle.getString("SettingsGUI.VideoPanel.title"), VideoPanel); // NOI18N

			invalidMemoryCheck.Text = bundle.getString("SettingsGUI.invalidMemoryCheck.text"); // NOI18N

			ignoreUnmappedImports.Text = bundle.getString("SettingsGUI.ignoreUnmappedImports.text"); // NOI18N

			useDebugMemory.Text = bundle.getString("SettingsGUI.useDebugMemory.text"); // NOI18N

			javax.swing.GroupLayout MemoryPanelLayout = new javax.swing.GroupLayout(MemoryPanel);
			MemoryPanel.Layout = MemoryPanelLayout;
			MemoryPanelLayout.HorizontalGroup = MemoryPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(MemoryPanelLayout.createSequentialGroup().addContainerGap().addGroup(MemoryPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(invalidMemoryCheck, javax.swing.GroupLayout.DEFAULT_SIZE, 1535, short.MaxValue).addComponent(ignoreUnmappedImports, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(useDebugMemory, javax.swing.GroupLayout.Alignment.TRAILING, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue)).addContainerGap());
			MemoryPanelLayout.VerticalGroup = MemoryPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(MemoryPanelLayout.createSequentialGroup().addContainerGap().addComponent(invalidMemoryCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(ignoreUnmappedImports).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(useDebugMemory).addContainerGap(414, short.MaxValue));

			jTabbedPane1.addTab(bundle.getString("SettingsGUI.MemoryPanel.title"), MemoryPanel); // NOI18N

			useCompiler.Text = bundle.getString("SettingsGUI.useCompiler.text"); // NOI18N

			methodMaxInstructionsBox.Model = new DefaultComboBoxModel(new string[] {"50", "100", "500", "1000", "3000"});

			profilerCheck.Text = bundle.getString("SettingsGUI.profileCheck.text"); // NOI18N

			methodMaxInstructionsLabel.Text = bundle.getString("SettingsGUI.methodMaxInstructionsLabel.text"); // NOI18N

			javax.swing.GroupLayout CompilerPanelLayout = new javax.swing.GroupLayout(CompilerPanel);
			CompilerPanel.Layout = CompilerPanelLayout;
			CompilerPanelLayout.HorizontalGroup = CompilerPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(CompilerPanelLayout.createSequentialGroup().addContainerGap().addGroup(CompilerPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.TRAILING, false).addComponent(useCompiler, javax.swing.GroupLayout.Alignment.LEADING, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(profilerCheck, javax.swing.GroupLayout.Alignment.LEADING, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGroup(javax.swing.GroupLayout.Alignment.LEADING, CompilerPanelLayout.createSequentialGroup().addComponent(methodMaxInstructionsBox, javax.swing.GroupLayout.PREFERRED_SIZE, 143, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(methodMaxInstructionsLabel, javax.swing.GroupLayout.PREFERRED_SIZE, 238, javax.swing.GroupLayout.PREFERRED_SIZE))).addContainerGap(358, short.MaxValue));
			CompilerPanelLayout.VerticalGroup = CompilerPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(CompilerPanelLayout.createSequentialGroup().addContainerGap().addComponent(useCompiler).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(profilerCheck).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(CompilerPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(methodMaxInstructionsBox, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(methodMaxInstructionsLabel)).addContainerGap(415, short.MaxValue));

			jTabbedPane1.addTab(bundle.getString("SettingsGUI.CompilerPanel.title"), CompilerPanel); // NOI18N

			antiAliasLabel.Text = bundle.getString("SettingsGUI.antiAliasLabel.text"); // NOI18N

			antiAliasingBox.Model = new DefaultComboBoxModel(new string[] {"OFF", "x4", "x8", "x16"});

			resolutionLabel.Text = bundle.getString("SettingsGUI.resolutionLabel.text"); // NOI18N

			resolutionBox.Model = makeResolutions();

			fullscreenCheck.Text = bundle.getString("SettingsGUI.fullscreenCheck.text"); // NOI18N

			javax.swing.GroupLayout DisplayPanelLayout = new javax.swing.GroupLayout(DisplayPanel);
			DisplayPanel.Layout = DisplayPanelLayout;
			DisplayPanelLayout.HorizontalGroup = DisplayPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(DisplayPanelLayout.createSequentialGroup().addContainerGap().addGroup(DisplayPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(fullscreenCheck).addGroup(DisplayPanelLayout.createSequentialGroup().addGroup(DisplayPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(DisplayPanelLayout.createSequentialGroup().addComponent(resolutionLabel).addGap(0, 0, short.MaxValue)).addComponent(antiAliasLabel, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addGroup(DisplayPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING, false).addComponent(antiAliasingBox, 0, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(resolutionBox, 0, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue)))).addGap(1318, 1318, 1318));
			DisplayPanelLayout.VerticalGroup = DisplayPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(DisplayPanelLayout.createSequentialGroup().addContainerGap().addGroup(DisplayPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(antiAliasLabel).addComponent(antiAliasingBox, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(DisplayPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(resolutionBox, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(resolutionLabel)).addGap(18, 18, 18).addComponent(fullscreenCheck).addContainerGap(392, short.MaxValue));

			jTabbedPane1.addTab(bundle.getString("SettingsGUI.DisplayPanel.title"), DisplayPanel); // NOI18N

			useDebugFont.Text = bundle.getString("SettingsGUI.useDebugFont.text"); // NOI18N

			javax.swing.GroupLayout MiscPanelLayout = new javax.swing.GroupLayout(MiscPanel);
			MiscPanel.Layout = MiscPanelLayout;
			MiscPanelLayout.HorizontalGroup = MiscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(MiscPanelLayout.createSequentialGroup().addContainerGap().addGroup(MiscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(useDebugFont, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue)).addContainerGap());
			MiscPanelLayout.VerticalGroup = MiscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(MiscPanelLayout.createSequentialGroup().addContainerGap().addComponent(useDebugFont).addContainerGap(368, short.MaxValue));

			jTabbedPane1.addTab(bundle.getString("SettingsGUI.MiscPanel.title"), MiscPanel); // NOI18N

			extractEboot.Text = bundle.getString("SettingsGUI.extractEboot.text"); // NOI18N

			cryptoSavedata.Text = bundle.getString("SettingsGUI.cryptoSavedata.text"); // NOI18N

			extractPGD.Text = bundle.getString("SettingsGUI.extractPGD.text"); // NOI18N

			extractSavedataKey.Text = bundle.getString("SettingsGUI.extractSavedataKey.text"); // NOI18N

			disableDLC.Text = bundle.getString("SettingsGUI.disableDLC.text"); // NOI18N

			javax.swing.GroupLayout CryptoPanelLayout = new javax.swing.GroupLayout(CryptoPanel);
			CryptoPanel.Layout = CryptoPanelLayout;
			CryptoPanelLayout.HorizontalGroup = CryptoPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(CryptoPanelLayout.createSequentialGroup().addContainerGap().addGroup(CryptoPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.TRAILING, false).addComponent(extractEboot, javax.swing.GroupLayout.Alignment.LEADING, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(cryptoSavedata, javax.swing.GroupLayout.Alignment.LEADING, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(extractPGD, javax.swing.GroupLayout.Alignment.LEADING).addComponent(extractSavedataKey, javax.swing.GroupLayout.Alignment.LEADING, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(disableDLC, javax.swing.GroupLayout.Alignment.LEADING, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue)).addContainerGap(394, short.MaxValue));
			CryptoPanelLayout.VerticalGroup = CryptoPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(CryptoPanelLayout.createSequentialGroup().addContainerGap().addComponent(cryptoSavedata).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(extractSavedataKey).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(extractPGD).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(extractEboot).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(disableDLC).addContainerGap(368, short.MaxValue));

			jTabbedPane1.addTab(bundle.getString("SettingsGUI.CryptoPanel.title"), CryptoPanel); // NOI18N

			buttonGroup3.add(lanMultiPlayerRadioButton);
			lanMultiPlayerRadioButton.Text = bundle.getString("SettingsGUI.lanMultiPlayerRadioButton.text"); // NOI18N

			buttonGroup3.add(netServerPortShiftRadioButton);
			netServerPortShiftRadioButton.Text = bundle.getString("SettingsGUI.netServerPortShiftRadioButton.text"); // NOI18N

			buttonGroup3.add(netClientPortShiftRadioButton);
			netClientPortShiftRadioButton.Text = bundle.getString("SettingsGUI.netClientPortShiftRadioButton.text"); // NOI18N

			buttonGroup3.add(enableProOnlineRadioButton);
			enableProOnlineRadioButton.Text = bundle.getString("SettingsGUI.enableProOnlineRadioButton.text"); // NOI18N

			lanMultiPlayerLabel.Text = bundle.getString("SettingsGUI.lanMultiPlayerLabel.text"); // NOI18N

			netServerPortShiftLabel.Text = bundle.getString("SettingsGUI.netServerPortShiftLabel.text"); // NOI18N

			netClientPortShiftLabel.Text = bundle.getString("SettingsGUI.netClientPortShiftLabel.text"); // NOI18N

			enableProOnlineLabel.Text = bundle.getString("SettingsGUI.enableProOnlineLabel.text"); // NOI18N

			metaServerLabel.Text = bundle.getString("SettingsGUI.metaServerLabel.text"); // NOI18N

			metaServerTextField.Text = bundle.getString("SettingsGUI.metaServerTextField.text"); // NOI18N

			metaServerRemindLabel.Text = bundle.getString("SettingsGUI.metaServerRemindLabel.text"); // NOI18N

			broadcastAddressLabel.Text = bundle.getString("SettingsGUI.broadcastAddressLabel.text"); // NOI18N

			broadcastAddressTextField.Text = bundle.getString("SettingsGUI.broadcastAddressTextField.text"); // NOI18N

			broadcastAddressRemindLabel.Text = bundle.getString("SettingsGUI.broadcastAddressRemindLabel.text"); // NOI18N

			javax.swing.GroupLayout networkPanelLayout = new javax.swing.GroupLayout(networkPanel);
			networkPanel.Layout = networkPanelLayout;
			networkPanelLayout.HorizontalGroup = networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(networkPanelLayout.createSequentialGroup().addGap(23, 23, 23).addGroup(networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(networkPanelLayout.createSequentialGroup().addGroup(networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(metaServerLabel).addComponent(broadcastAddressLabel)).addGap(39, 39, 39).addGroup(networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(networkPanelLayout.createSequentialGroup().addComponent(metaServerTextField, javax.swing.GroupLayout.PREFERRED_SIZE, 154, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(31, 31, 31).addComponent(metaServerRemindLabel)).addGroup(networkPanelLayout.createSequentialGroup().addGap(10, 10, 10).addComponent(broadcastAddressRemindLabel)).addComponent(broadcastAddressTextField, javax.swing.GroupLayout.PREFERRED_SIZE, 264, javax.swing.GroupLayout.PREFERRED_SIZE))).addGroup(networkPanelLayout.createSequentialGroup().addGroup(networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(netClientPortShiftRadioButton).addComponent(enableProOnlineRadioButton)).addGap(18, 18, 18).addGroup(networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(enableProOnlineLabel).addComponent(netClientPortShiftLabel))).addGroup(networkPanelLayout.createSequentialGroup().addGroup(networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(netServerPortShiftRadioButton).addComponent(lanMultiPlayerRadioButton)).addGap(18, 18, 18).addGroup(networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(lanMultiPlayerLabel).addComponent(netServerPortShiftLabel)))).addContainerGap(150, short.MaxValue));
			networkPanelLayout.VerticalGroup = networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(networkPanelLayout.createSequentialGroup().addGap(53, 53, 53).addGroup(networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(lanMultiPlayerRadioButton).addComponent(lanMultiPlayerLabel)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(netServerPortShiftRadioButton).addComponent(netServerPortShiftLabel)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(netClientPortShiftRadioButton).addComponent(netClientPortShiftLabel)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(enableProOnlineRadioButton).addComponent(enableProOnlineLabel)).addGap(30, 30, 30).addGroup(networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(metaServerLabel).addComponent(metaServerTextField, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(metaServerRemindLabel)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addGroup(networkPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(broadcastAddressLabel).addComponent(broadcastAddressTextField, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addComponent(broadcastAddressRemindLabel).addContainerGap(242, short.MaxValue));

			jTabbedPane1.addTab(bundle.getString("SettingsGUI.networkPanel.TabConstraints.tabTitle"), networkPanel); // NOI18N

			cancelButton.Text = bundle.getString("CancelButton.text"); // NOI18N
			cancelButton.Parent = this;

			javax.swing.GroupLayout layout = new javax.swing.GroupLayout(ContentPane);
			ContentPane.Layout = layout;
			layout.HorizontalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addContainerGap(395, short.MaxValue).addComponent(jButtonOK, javax.swing.GroupLayout.DEFAULT_SIZE, 105, short.MaxValue).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(cancelButton, javax.swing.GroupLayout.DEFAULT_SIZE, 132, short.MaxValue).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(jButtonApply, javax.swing.GroupLayout.DEFAULT_SIZE, 124, short.MaxValue)).addComponent(jTabbedPane1, javax.swing.GroupLayout.PREFERRED_SIZE, 0, short.MaxValue)).addContainerGap());
			layout.VerticalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addComponent(jTabbedPane1).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(jButtonApply).addComponent(jButtonOK).addComponent(cancelButton, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue)).addContainerGap());

			pack();
		} // </editor-fold>//GEN-END:initComponents

		private class ActionListenerAnonymousInnerClass : java.awt.@event.ActionListener
		{
			private readonly SettingsGUI outerInstance;

			public ActionListenerAnonymousInnerClass(SettingsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.jButtonOKActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass2 : java.awt.@event.ActionListener
		{
			private readonly SettingsGUI outerInstance;

			public ActionListenerAnonymousInnerClass2(SettingsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.jButtonApplyActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass3 : java.awt.@event.ActionListener
		{
			private readonly SettingsGUI outerInstance;

			public ActionListenerAnonymousInnerClass3(SettingsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.tmpPathBrowseButtonActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass4 : java.awt.@event.ActionListener
		{
			private readonly SettingsGUI outerInstance;

			public ActionListenerAnonymousInnerClass4(SettingsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.btnUMDPathRemoveActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass5 : java.awt.@event.ActionListener
		{
			private readonly SettingsGUI outerInstance;

			public ActionListenerAnonymousInnerClass5(SettingsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.btnUMDPathAddActionPerformed(evt);
			}
		}

		public virtual void RefreshWindow()
		{
			setAllComponentsFromSettings();
		}

		private void jButtonOKActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_jButtonOKActionPerformed
			setAllComponentsToSettings();
			dispose();
		} //GEN-LAST:event_jButtonOKActionPerformed

		private void jButtonApplyActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_jButtonApplyActionPerformed
			setAllComponentsToSettings();
		} //GEN-LAST:event_jButtonApplyActionPerformed

		private void tmpPathBrowseButtonActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_tmpPathBrowseButtonActionPerformed
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			FolderChooser folderChooser = new FolderChooser(bundle.getString("SettingsGUI.strSelectTMPPath.text"));
			int result = folderChooser.showSaveDialog(tmpPathBrowseButton.TopLevelAncestor);
			if (result == FolderChooser.APPROVE_OPTION)
			{
				tmppath.Text = folderChooser.SelectedFile.Path;
			}
		} //GEN-LAST:event_tmpPathBrowseButtonActionPerformed

		private void btnUMDPathAddActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_btnUMDPathAddActionPerformed
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			FolderChooser folderChooser = new FolderChooser(bundle.getString("SettingsGUI.strSelectUMDPath.text"));
			int result = folderChooser.showSaveDialog(lbUMDPaths.TopLevelAncestor);
			if (result == FolderChooser.APPROVE_OPTION)
			{
				DefaultListModel dlm = (DefaultListModel) lbUMDPaths.Model;
				File pathtoadd = folderChooser.SelectedFile;

				// avoid double entries
				for (int i = 0; i < lbUMDPaths.Model.Size; i++)
				{
					File check = new File((string) lbUMDPaths.Model.getElementAt(i));
					if (check.Equals(pathtoadd))
					{
						JpcspDialogManager.showInformation(this, java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp").getString("SettingsGUI.strPathInList.text")); //NOI18N
						return;
					}
				}
				dlm.addElement(pathtoadd.Path);
			}
		} //GEN-LAST:event_btnUMDPathAddActionPerformed

		private void btnUMDPathRemoveActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_btnUMDPathRemoveActionPerformed
			DefaultListModel dlm = (DefaultListModel) lbUMDPaths.Model;
			dlm.remove(lbUMDPaths.SelectedIndex);
		} //GEN-LAST:event_btnUMDPathRemoveActionPerformed

		public override void dispose()
		{
			Emulator.MainGUI.endWindowDialog();
			base.dispose();
		}
		// Variables declaration - do not modify//GEN-BEGIN:variables
		private javax.swing.JPanel CompilerPanel;
		private javax.swing.JPanel CryptoPanel;
		private javax.swing.JPanel DisplayPanel;
		private javax.swing.JPanel GeneralPanel;
		private javax.swing.JPanel MemoryPanel;
		private javax.swing.JPanel MiscPanel;
		private javax.swing.JPanel RegionPanel;
		private javax.swing.JPanel VideoPanel;
		private JComboBox adhocChannelBox;
		private javax.swing.JLabel adhocChannelLabel;
		private javax.swing.JLabel antiAliasLabel;
		private JComboBox antiAliasingBox;
		private javax.swing.JLabel broadcastAddressLabel;
		private javax.swing.JLabel broadcastAddressRemindLabel;
		private JTextField broadcastAddressTextField;
		private javax.swing.JButton btnUMDPathAdd;
		private javax.swing.JButton btnUMDPathRemove;
		private JComboBox buttonBox;
		private javax.swing.ButtonGroup buttonGroup1;
		private javax.swing.ButtonGroup buttonGroup2;
		private javax.swing.ButtonGroup buttonGroup3;
		private javax.swing.JLabel buttonLabel;
		private pspsharp.GUI.CancelButton cancelButton;
		private JRadioButton classicUmdDialog;
		private JCheckBox cryptoSavedata;
		private JComboBox dateFormatBox;
		private javax.swing.JLabel dateFormatLabel;
		private JComboBox daylightBox;
		private javax.swing.JLabel daylightLabel;
		private JCheckBox disableDLC;
		private JCheckBox disableOptimizedVertexInfoReading;
		private JCheckBox disableUBOCheck;
		private JCheckBox disableVBOCheck;
		private JCheckBox enableDynamicShadersCheck;
		private JCheckBox enableGETextureCheck;
		private JCheckBox enableNativeCLUTCheck;
		private javax.swing.JLabel enableProOnlineLabel;
		private JRadioButton enableProOnlineRadioButton;
		private JCheckBox enableShaderColorMaskCheck;
		private JCheckBox enableShaderStencilTestCheck;
		private JCheckBox enableVAOCheck;
		private JCheckBox extractEboot;
		private JCheckBox extractPGD;
		private JCheckBox extractSavedataKey;
		private JCheckBox fullscreenCheck;
		private JCheckBox geometryShaderCheck;
		private JCheckBox ignoreUnmappedImports;
		private javax.swing.JLabel imposeLabel;
		private JCheckBox invalidMemoryCheck;
		private javax.swing.JButton jButtonApply;
		private javax.swing.JButton jButtonOK;
		private javax.swing.JPanel jPanel1;
		private javax.swing.JPanel jPanel2;
		private javax.swing.JPanel jPanel3;
		private javax.swing.JPanel jPanel4;
		private javax.swing.JPanel jPanel5;
		private javax.swing.JScrollPane jScrollPane1;
		private javax.swing.JTabbedPane jTabbedPane1;
		private javax.swing.JLabel lanMultiPlayerLabel;
		private JRadioButton lanMultiPlayerRadioButton;
		private JComboBox languageBox;
		private javax.swing.JLabel languageLabel;
		private javax.swing.JList lbUMDPaths;
		private JCheckBox loadAndRunCheck;
		private javax.swing.JLabel metaServerLabel;
		private javax.swing.JLabel metaServerRemindLabel;
		private JTextField metaServerTextField;
		private JComboBox methodMaxInstructionsBox;
		private javax.swing.JLabel methodMaxInstructionsLabel;
		private JComboBox modelBox;
		private javax.swing.JLabel modelLabel;
		private javax.swing.JLabel netClientPortShiftLabel;
		private JRadioButton netClientPortShiftRadioButton;
		private javax.swing.JLabel netServerPortShiftLabel;
		private JRadioButton netServerPortShiftRadioButton;
		private javax.swing.JPanel networkPanel;
		private JTextField nicknameTextField;
		private javax.swing.JLabel nicknamelLabel;
		private JCheckBox onlyGEGraphicsCheck;
		private JCheckBox pbpunpackcheck;
		private JCheckBox profilerCheck;
		private javax.swing.JPanel renderPanel;
		private JComboBox resolutionBox;
		private javax.swing.JLabel resolutionLabel;
		private JCheckBox saveStencilToMemory;
		private JCheckBox saveWindowPosCheck;
		private JCheckBox shadersCheck;
		private javax.swing.JLabel sysParmLabel;
		private JComboBox timeFormatBox;
		private javax.swing.JLabel timeFormatLabel;
		private javax.swing.JLabel timezoneLabel;
		private JSpinner timezoneSpinner;
		private javax.swing.JButton tmpPathBrowseButton;
		private javax.swing.JLabel tmpPathLabel;
		private JTextField tmppath;
		private JRadioButton umdBrowser;
		private javax.swing.JLabel umdPathLabel;
		private JCheckBox useCompiler;
		private JCheckBox useDebugFont;
		private JCheckBox useDebugMemory;
		private JRadioButton useExternalSoftwareRenderer;
		private JRadioButton useOpenglRenderer;
		private JRadioButton useSoftwareRenderer;
		private JCheckBox useVertexCache;
		private JComboBox wlanPowerBox;
		private javax.swing.JLabel wlanPowerLabel;
		// End of variables declaration//GEN-END:variables
	}

}