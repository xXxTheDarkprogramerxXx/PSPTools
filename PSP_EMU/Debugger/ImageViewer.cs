using System.Windows.Forms;

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
namespace pspsharp.Debugger
{


	using Modules = pspsharp.HLE.Modules;
	using sceDisplay = pspsharp.HLE.modules.sceDisplay;
	using GeCommands = pspsharp.graphics.GeCommands;
	using IMemoryReader = pspsharp.memory.IMemoryReader;
	using ImageReader = pspsharp.memory.ImageReader;

	public class ImageViewer : javax.swing.JFrame
	{

		private const long serialVersionUID = 8837780642045065242L;
		private int startAddress = MemoryMap.START_VRAM;
		private int bufferWidth = 512;
		private int imageWidth = 480;
		private int imageHeight = 272;
		private bool imageSwizzle = false;
		private bool useAlpha = false;
		private int backgroundColor = 0;
		private int pixelFormat = GeCommands.TPSM_PIXEL_STORAGE_MODE_32BIT_ABGR8888;
		private int clutAddress = 0;
		private int clutNumberBlocks = 32;
		private int clutFormat = GeCommands.CMODE_FORMAT_32BIT_ABGR8888;
		private int clutStart = 0;
		private int clutShift = 0;
		private int clutMask = 0xFF;
		private static readonly Color[] backgroundColors = new Color[]{Color.WHITE, Color.BLACK, Color.RED, Color.GREEN, Color.BLUE, Color.GRAY};

		public ImageViewer()
		{
			// memoryImage construction overriden for MemoryImage
			initComponents();
			copyValuesToFields();

			WindowPropSaver.loadWindowProperties(this);
		}

		public virtual void SafeRefreshImage()
		{
			SwingUtilities.invokeLater(() =>
			{
			RefreshImage();
			});
		}

		public virtual void RefreshImage()
		{
			goToAddress();
		}

		private void valuesUpdated()
		{
			memoryImage.Size = memoryImage.PreferredSize;
			repaint();
		}

		private void goToAddress()
		{
			try
			{
				startAddress = Integer.decode(addressField.Text);
				imageWidth = Integer.decode(widthField.Text);
				imageHeight = Integer.decode(heightField.Text);
				bufferWidth = Integer.decode(bufferWidthField.Text);
				clutAddress = Integer.decode(clutAddressField.Text);
				clutNumberBlocks = Integer.decode(clutNumberBlocksField.Text);
			}
			catch (System.FormatException nfe)
			{
				java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp");
				MessageBox.Show(this, bundle.getString("ImageViewer.strInvalidNumber.text") + " " + nfe.LocalizedMessage);
				return;
			}

			pixelFormat = pixelFormatField.SelectedIndex;
			imageSwizzle = swizzleField.Selected;
			useAlpha = useAlphaField.Selected;
			backgroundColor = backgroundColorField.SelectedIndex;
			clutFormat = clutFormatField.SelectedIndex;

			// clean UI strings before updating
			copyValuesToFields();
			valuesUpdated();
		}

		private void copyValuesToFields()
		{
			addressField.Text = string.Format("0x{0:X8}", startAddress);
			widthField.Text = string.Format("{0:D}", imageWidth);
			heightField.Text = string.Format("{0:D}", imageHeight);
			bufferWidthField.Text = string.Format("{0:D}", bufferWidth);
			pixelFormatField.SelectedIndex = pixelFormat;
			swizzleField.Selected = imageSwizzle;
			useAlphaField.Selected = useAlpha;
			backgroundColorField.SelectedIndex = backgroundColor;
			clutAddressField.Text = string.Format("0x{0:X8}", clutAddress);
			clutNumberBlocksField.Text = string.Format("{0:D}", clutNumberBlocks);
			clutFormatField.SelectedIndex = clutFormat;
		}

		private void goToBufferInfo(sceDisplay.BufferInfo bufferInfo)
		{
			startAddress = bufferInfo.topAddr;
			imageWidth = bufferInfo.width;
			imageHeight = bufferInfo.height;
			bufferWidth = bufferInfo.bufferWidth;
			pixelFormat = bufferInfo.pixelFormat;
			imageSwizzle = false;
			useAlpha = false;

			copyValuesToFields();
			valuesUpdated();
		}

		public override void dispose()
		{
			Emulator.MainGUI.endWindowDialog();
			base.dispose();
		}

		private class MemoryImage : JPanel
		{
			private readonly ImageViewer outerInstance;


			internal const long serialVersionUID = 1372183323503668615L;

			public MemoryImage(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void paintComponent(Graphics g)
			{
				if (Memory.isAddressGood(outerInstance.startAddress))
				{
					Insets insets = Insets;
					int minWidth = System.Math.Min(outerInstance.imageWidth, outerInstance.bufferWidth);

					g.Color = backgroundColors[outerInstance.backgroundColor];
					g.fillRect(insets.left, insets.top, minWidth, outerInstance.imageHeight);

					IMemoryReader imageReader = ImageReader.getImageReader(outerInstance.startAddress, outerInstance.imageWidth, outerInstance.imageHeight, outerInstance.bufferWidth, outerInstance.pixelFormat, outerInstance.imageSwizzle, outerInstance.clutAddress, outerInstance.clutFormat, outerInstance.clutNumberBlocks, outerInstance.clutStart, outerInstance.clutShift, outerInstance.clutMask, null, null);

					for (int y = 0; y < outerInstance.imageHeight; y++)
					{
						for (int x = 0; x < minWidth; x++)
						{
							int colorABGR = imageReader.readNext();
							int colorARGB = ImageReader.colorABGRtoARGB(colorABGR);
							g.Color = new Color(colorARGB, outerInstance.useAlpha);

							drawPixel(g, x + insets.left, y + insets.top);
						}
					}
				}
			}

			internal virtual void drawPixel(Graphics g, int x, int y)
			{
				g.drawLine(x, y, x, y);
			}

			public override Dimension PreferredSize
			{
				get
				{
					Insets insets = Insets;
					return new Dimension(outerInstance.imageWidth + insets.left + insets.right, outerInstance.imageHeight + insets.top + insets.bottom);
				}
			}

			public override Dimension MaximumSize
			{
				get
				{
					return PreferredSize;
				}
			}
		}

		/// <summary>
		/// This method is called from within the constructor to initialize the form.
		/// WARNING: Do NOT modify this code. The content of this method is always
		/// regenerated by the Form Editor.
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private void initComponents()
		private void initComponents()
		{

			addressField = new javax.swing.JTextField();
			lblWidth = new javax.swing.JLabel();
			widthField = new javax.swing.JTextField();
			lblHeight = new javax.swing.JLabel();
			heightField = new javax.swing.JTextField();
			lblBufferWidth = new javax.swing.JLabel();
			bufferWidthField = new javax.swing.JTextField();
			lblAddress = new javax.swing.JLabel();
			lblPixelFormat = new javax.swing.JLabel();
			pixelFormatField = new javax.swing.JComboBox();
			swizzleField = new javax.swing.JCheckBox();
			lblCLUT = new javax.swing.JLabel();
			clutAddressField = new javax.swing.JTextField();
			lblCLUTNumberBlocks = new javax.swing.JLabel();
			clutNumberBlocksField = new javax.swing.JTextField();
			lblCLUTFormat = new javax.swing.JLabel();
			clutFormatField = new javax.swing.JComboBox();
			lblBackgroundColor = new javax.swing.JLabel();
			backgroundColorField = new javax.swing.JComboBox();
			btnGoToAddress = new javax.swing.JButton();
			btnGoToGE = new javax.swing.JButton();
			btnGoToFB = new javax.swing.JButton();
			useAlphaField = new javax.swing.JCheckBox();
			memoryImage = new MemoryImage(this);

			DefaultCloseOperation = javax.swing.WindowConstants.DISPOSE_ON_CLOSE;
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			Title = bundle.getString("ImageViewer.title"); // NOI18N
			MinimumSize = new Dimension(532, 500);

			addressField.Font = new java.awt.Font("Courier New", 0, 12); // NOI18N
			addressField.HorizontalAlignment = javax.swing.JTextField.CENTER;
			addressField.Text = "0x00000000"; // NOI18N
			addressField.ToolTipText = bundle.getString("ImageViewer.addressField.toolTipText"); // NOI18N
			addressField.addKeyListener(new KeyAdapterAnonymousInnerClass(this));

			lblWidth.Text = bundle.getString("ImageViewer.lblWidth.text"); // NOI18N

			widthField.HorizontalAlignment = javax.swing.JTextField.CENTER;
			widthField.Text = "480"; // NOI18N
			widthField.ToolTipText = bundle.getString("ImageViewer.widthField.toolTipText"); // NOI18N
			widthField.addKeyListener(new KeyAdapterAnonymousInnerClass2(this));

			lblHeight.Text = bundle.getString("ImageViewer.lblHeight.text"); // NOI18N

			heightField.HorizontalAlignment = javax.swing.JTextField.CENTER;
			heightField.Text = "272"; // NOI18N
			heightField.ToolTipText = bundle.getString("ImageViewer.heightField.toolTipText"); // NOI18N
			heightField.addKeyListener(new KeyAdapterAnonymousInnerClass3(this));

			lblBufferWidth.Text = bundle.getString("ImageViewer.lblBufferWidth.text"); // NOI18N

			bufferWidthField.HorizontalAlignment = javax.swing.JTextField.CENTER;
			bufferWidthField.Text = "512"; // NOI18N
			bufferWidthField.ToolTipText = bundle.getString("ImageViewer.bufferWidthField.toolTipText"); // NOI18N
			bufferWidthField.addKeyListener(new KeyAdapterAnonymousInnerClass4(this));

			lblAddress.Text = bundle.getString("ImageViewer.lblAddress.text"); // NOI18N

			lblPixelFormat.Text = bundle.getString("ImageViewer.lblPixelFormat.text"); // NOI18N

			pixelFormatField.Model = new javax.swing.DefaultComboBoxModel(new string[] {"565", "5551", "4444", "8888", "Indexed 4", "Indexed 8", "Indexed 16", "Indexed 32", "DXT1", "DXT3", "DXT5"});
			pixelFormatField.addActionListener(new ActionListenerAnonymousInnerClass(this));

			swizzleField.Text = bundle.getString("ImageViewer.swizzleField.text"); // NOI18N
			swizzleField.addActionListener(new ActionListenerAnonymousInnerClass2(this));

			lblCLUT.Text = bundle.getString("ImageViewer.lblCLUT.text"); // NOI18N

			clutAddressField.Font = new java.awt.Font("Courier New", 0, 12); // NOI18N
			clutAddressField.HorizontalAlignment = javax.swing.JTextField.CENTER;
			clutAddressField.Text = "0x00000000"; // NOI18N
			clutAddressField.ToolTipText = bundle.getString("ImageViewer.clutAddressField.toolTipText"); // NOI18N
			clutAddressField.addKeyListener(new KeyAdapterAnonymousInnerClass5(this));

			lblCLUTNumberBlocks.Text = bundle.getString("ImageViewer.lblCLUTNumberBlocks.text"); // NOI18N

			clutNumberBlocksField.HorizontalAlignment = javax.swing.JTextField.CENTER;
			clutNumberBlocksField.Text = "32"; // NOI18N
			clutNumberBlocksField.ToolTipText = bundle.getString("ImageViewer.clutNumberBlocksField.toolTipText"); // NOI18N
			clutNumberBlocksField.addKeyListener(new KeyAdapterAnonymousInnerClass6(this));

			lblCLUTFormat.Text = bundle.getString("ImageViewer.lblCLUTFormat.text"); // NOI18N

			clutFormatField.Model = new javax.swing.DefaultComboBoxModel(new string[] {"565", "5551", "4444", "8888"});
			clutFormatField.addActionListener(new ActionListenerAnonymousInnerClass3(this));

			lblBackgroundColor.Text = bundle.getString("ImageViewer.lblBackgroundColor.text"); // NOI18N

			backgroundColorField.Model = new javax.swing.DefaultComboBoxModel(new string[] {"White", "Black", "Red", "Green", "Blue", "Gray"});
			backgroundColorField.SelectedItem = "Black";
			backgroundColorField.addActionListener(new ActionListenerAnonymousInnerClass4(this));

			btnGoToAddress.Text = bundle.getString("ImageViewer.btnGoToAddress.text"); // NOI18N
			btnGoToAddress.addActionListener(new ActionListenerAnonymousInnerClass5(this));

			btnGoToGE.Text = bundle.getString("ImageViewer.btnGoToGE.text"); // NOI18N
			btnGoToGE.addActionListener(new ActionListenerAnonymousInnerClass6(this));

			btnGoToFB.Text = bundle.getString("ImageViewer.btnGoToFB.text"); // NOI18N
			btnGoToFB.addActionListener(new ActionListenerAnonymousInnerClass7(this));

			useAlphaField.Text = bundle.getString("ImageViewer.useAlphaField.text"); // NOI18N
			useAlphaField.addActionListener(new ActionListenerAnonymousInnerClass8(this));

			memoryImage.Background = new Color(0, 0, 0);
			memoryImage.Border = javax.swing.BorderFactory.createLineBorder(new Color(255, 0, 0), 10);

			javax.swing.GroupLayout memoryImageLayout = new javax.swing.GroupLayout(memoryImage);
			memoryImage.Layout = memoryImageLayout;
			memoryImageLayout.HorizontalGroup = memoryImageLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGap(0, 480, short.MaxValue);
			memoryImageLayout.VerticalGroup = memoryImageLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGap(0, 272, short.MaxValue);

			javax.swing.GroupLayout layout = new javax.swing.GroupLayout(ContentPane);
			ContentPane.Layout = layout;
			layout.HorizontalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addContainerGap().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING, false).addGroup(layout.createSequentialGroup().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(lblPixelFormat).addComponent(lblAddress).addComponent(lblCLUT)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.TRAILING, false).addComponent(addressField, javax.swing.GroupLayout.Alignment.LEADING).addComponent(clutAddressField).addComponent(pixelFormatField, javax.swing.GroupLayout.Alignment.LEADING, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE))).addGroup(layout.createSequentialGroup().addComponent(lblBackgroundColor).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(backgroundColorField, 0, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue))).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addGap(26, 26, 26).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addComponent(swizzleField).addGap(18, 18, 18)).addGroup(javax.swing.GroupLayout.Alignment.TRAILING, layout.createSequentialGroup().addComponent(widthField, javax.swing.GroupLayout.PREFERRED_SIZE, 50, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(35, 35, 35))).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(useAlphaField).addGroup(layout.createSequentialGroup().addGap(10, 10, 10).addComponent(heightField, javax.swing.GroupLayout.PREFERRED_SIZE, 50, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(lblBufferWidth).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(bufferWidthField, javax.swing.GroupLayout.PREFERRED_SIZE, 50, javax.swing.GroupLayout.PREFERRED_SIZE)))).addGroup(layout.createSequentialGroup().addComponent(lblCLUTNumberBlocks).addGap(15, 15, 15).addComponent(clutNumberBlocksField, javax.swing.GroupLayout.PREFERRED_SIZE, 40, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(lblCLUTFormat).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(clutFormatField, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)))).addGroup(layout.createSequentialGroup().addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(lblWidth).addGap(74, 74, 74).addComponent(lblHeight)))).addComponent(memoryImage, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addGap(0, 8, short.MaxValue)).addGroup(layout.createSequentialGroup().addComponent(btnGoToAddress, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(btnGoToGE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(btnGoToFB, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue))).addContainerGap());
			layout.VerticalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addContainerGap().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(addressField, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(lblWidth).addComponent(widthField, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(lblHeight).addComponent(heightField, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(lblBufferWidth).addComponent(bufferWidthField, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(lblAddress)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(lblPixelFormat).addComponent(pixelFormatField, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(swizzleField).addComponent(useAlphaField)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(clutAddressField, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(lblCLUTNumberBlocks).addComponent(clutNumberBlocksField, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(lblCLUTFormat).addComponent(clutFormatField, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(lblCLUT)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(lblBackgroundColor).addComponent(backgroundColorField, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(btnGoToAddress).addComponent(btnGoToGE).addComponent(btnGoToFB)).addGap(18, 18, 18).addComponent(memoryImage, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addContainerGap(javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue));

			pack();
		} // </editor-fold>//GEN-END:initComponents

		private class KeyAdapterAnonymousInnerClass : java.awt.@event.KeyAdapter
		{
			private readonly ImageViewer outerInstance;

			public KeyAdapterAnonymousInnerClass(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void keyPressed(KeyEvent evt)
			{
				outerInstance.keyPressed(evt);
			}
		}

		private class KeyAdapterAnonymousInnerClass2 : java.awt.@event.KeyAdapter
		{
			private readonly ImageViewer outerInstance;

			public KeyAdapterAnonymousInnerClass2(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void keyPressed(KeyEvent evt)
			{
				outerInstance.keyPressed(evt);
			}
		}

		private class KeyAdapterAnonymousInnerClass3 : java.awt.@event.KeyAdapter
		{
			private readonly ImageViewer outerInstance;

			public KeyAdapterAnonymousInnerClass3(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void keyPressed(KeyEvent evt)
			{
				outerInstance.keyPressed(evt);
			}
		}

		private class KeyAdapterAnonymousInnerClass4 : java.awt.@event.KeyAdapter
		{
			private readonly ImageViewer outerInstance;

			public KeyAdapterAnonymousInnerClass4(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void keyPressed(KeyEvent evt)
			{
				outerInstance.keyPressed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass : java.awt.@event.ActionListener
		{
			private readonly ImageViewer outerInstance;

			public ActionListenerAnonymousInnerClass(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.changeImageActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass2 : java.awt.@event.ActionListener
		{
			private readonly ImageViewer outerInstance;

			public ActionListenerAnonymousInnerClass2(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.changeImageActionPerformed(evt);
			}
		}

		private class KeyAdapterAnonymousInnerClass5 : java.awt.@event.KeyAdapter
		{
			private readonly ImageViewer outerInstance;

			public KeyAdapterAnonymousInnerClass5(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void keyPressed(KeyEvent evt)
			{
				outerInstance.keyPressed(evt);
			}
		}

		private class KeyAdapterAnonymousInnerClass6 : java.awt.@event.KeyAdapter
		{
			private readonly ImageViewer outerInstance;

			public KeyAdapterAnonymousInnerClass6(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void keyPressed(KeyEvent evt)
			{
				outerInstance.keyPressed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass3 : java.awt.@event.ActionListener
		{
			private readonly ImageViewer outerInstance;

			public ActionListenerAnonymousInnerClass3(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.changeImageActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass4 : java.awt.@event.ActionListener
		{
			private readonly ImageViewer outerInstance;

			public ActionListenerAnonymousInnerClass4(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.changeImageActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass5 : java.awt.@event.ActionListener
		{
			private readonly ImageViewer outerInstance;

			public ActionListenerAnonymousInnerClass5(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.btnGoToAddressActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass6 : java.awt.@event.ActionListener
		{
			private readonly ImageViewer outerInstance;

			public ActionListenerAnonymousInnerClass6(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.btnGoToGEActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass7 : java.awt.@event.ActionListener
		{
			private readonly ImageViewer outerInstance;

			public ActionListenerAnonymousInnerClass7(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.btnGoToFBActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass8 : java.awt.@event.ActionListener
		{
			private readonly ImageViewer outerInstance;

			public ActionListenerAnonymousInnerClass8(ImageViewer outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.changeImageActionPerformed(evt);
			}
		}

		private void btnGoToAddressActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_btnGoToAddressActionPerformed
			goToAddress();
		} //GEN-LAST:event_btnGoToAddressActionPerformed

		private void btnGoToGEActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_btnGoToGEActionPerformed
			goToBufferInfo(Modules.sceDisplayModule.BufferInfoGe);
		} //GEN-LAST:event_btnGoToGEActionPerformed

		private void btnGoToFBActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_btnGoToFBActionPerformed
			goToBufferInfo(Modules.sceDisplayModule.BufferInfoFb);
		} //GEN-LAST:event_btnGoToFBActionPerformed

		private void keyPressed(KeyEvent evt)
		{ //GEN-FIRST:event_keyPressed
			if (evt.KeyCode == KeyEvent.VK_ENTER)
			{
				RefreshImage();
			}
		} //GEN-LAST:event_keyPressed

		private void changeImageActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_changeImageActionPerformed
			RefreshImage();
		} //GEN-LAST:event_changeImageActionPerformed
		// Variables declaration - do not modify//GEN-BEGIN:variables
		private javax.swing.JTextField addressField;
		private javax.swing.JComboBox backgroundColorField;
		private javax.swing.JButton btnGoToAddress;
		private javax.swing.JButton btnGoToFB;
		private javax.swing.JButton btnGoToGE;
		private javax.swing.JTextField bufferWidthField;
		private javax.swing.JTextField clutAddressField;
		private javax.swing.JComboBox clutFormatField;
		private javax.swing.JTextField clutNumberBlocksField;
		private javax.swing.JTextField heightField;
		private javax.swing.JLabel lblAddress;
		private javax.swing.JLabel lblBackgroundColor;
		private javax.swing.JLabel lblBufferWidth;
		private javax.swing.JLabel lblCLUT;
		private javax.swing.JLabel lblCLUTFormat;
		private javax.swing.JLabel lblCLUTNumberBlocks;
		private javax.swing.JLabel lblHeight;
		private javax.swing.JLabel lblPixelFormat;
		private javax.swing.JLabel lblWidth;
		private JPanel memoryImage;
		private javax.swing.JComboBox pixelFormatField;
		private javax.swing.JCheckBox swizzleField;
		private javax.swing.JCheckBox useAlphaField;
		private javax.swing.JTextField widthField;
		// End of variables declaration//GEN-END:variables
	}

}