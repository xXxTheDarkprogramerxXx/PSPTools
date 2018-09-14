using System;
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
	using JpcspDialogManager = pspsharp.util.JpcspDialogManager;
	using Utilities = pspsharp.util.Utilities;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.util.Utilities.parseHexLong;

	public class CheatsGUI : javax.swing.JFrame, KeyListener
	{
		private const long serialVersionUID = 2885526629263842635L;
		public const string identifierForConfig = "cheatsGUI";
		private const int cheatsThreadSleepMillis = 5;
		private CheatsThread cheatsThread = null;

		public CheatsGUI()
		{
			initComponents();

			WindowPropSaver.loadWindowProperties(this);
		}

		public override void keyTyped(KeyEvent e)
		{
			// do nothing
		}

		public override void keyPressed(KeyEvent e)
		{
			// do nothing
		}

		public override void keyReleased(KeyEvent e)
		{
			// do nothing
		}

		private class CheatsThread : Thread
		{

			internal string[] codes;
			internal int currentCode;
			internal readonly CheatsGUI cheats;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			internal volatile bool exit_Renamed;

			public CheatsThread(CheatsGUI cheats)
			{
				this.cheats = cheats;
			}

			public virtual void exit()
			{
				exit_Renamed = true;
			}

			internal virtual string NextCode
			{
				get
				{
					string code;
    
					while (true)
					{
						if (currentCode >= codes.Length)
						{
							code = null;
							break;
						}
    
						code = codes[currentCode++].Trim();
    
						if (code.StartsWith("_L", StringComparison.Ordinal))
						{
							code = code.Substring(2).Trim();
							break;
						}
						else if (code.StartsWith("0", StringComparison.Ordinal))
						{
							break;
						}
					}
    
					return code;
				}
			}

			internal virtual void skipCodes(int count)
			{
				for (int i = 0; i < count; i++)
				{
					if (string.ReferenceEquals(NextCode, null))
					{
						break;
					}
				}
			}

			internal virtual void skipAllCodes()
			{
				currentCode = codes.Length;
			}

			internal static int getAddress(int value)
			{
				// The User space base address has to be added to given value
				return (value + MemoryMap.START_USERSPACE) & Memory.addressMask;
			}

			public override void run()
			{
				Memory mem = Memory.Instance;

				// only read here, as the text area is disabled on thread enabling
				codes = cheats.CodesList;

				while (!exit_Renamed)
				{
					// Sleep a little bit to not use the CPU at 100%
					Utilities.sleep(cheatsThreadSleepMillis, 0);

					currentCode = 0;
					while (true)
					{
						string code = NextCode;
						if (string.ReferenceEquals(code, null))
						{
							break;
						}

						string[] parts = code.Split(" ", true);
						if (parts == null || parts.Length < 2)
						{
							continue;
						}

						int value;
						int comm = (int) parseHexLong(parts[0].Trim(), false);
						int arg = (int) parseHexLong(parts[1].Trim(), false);
						int addr = getAddress(comm & 0x0FFFFFFF);

						switch ((int)((uint)comm >> 28))
						{
							case 0: // 8-bit write.
								if (Memory.isAddressGood(addr))
								{
									mem.write8(addr, (sbyte) arg);
								}
								break;
							case 0x1: // 16-bit write.
								if (Memory.isAddressGood(addr))
								{
									mem.write16(addr, (short) arg);
								}
								break;
							case 0x2: // 32-bit write.
								if (Memory.isAddressGood(addr))
								{
									mem.write32(addr, arg);
								}
								break;
							case 0x3: // Increment/Decrement
								addr = getAddress(arg);
								value = 0;
								int increment = 0;
								// Read value from memory
								switch ((comm >> 20) & 0xF)
								{
									case 1:
									case 2: // 8-bit
										value = mem.read8(addr);
										increment = comm & 0xFF;
										break;
									case 3:
									case 4: // 16-bit
										value = mem.read16(addr);
										increment = comm & 0xFFFF;
										break;
									case 5:
									case 6: // 32-bit
										value = mem.read32(addr);
										code = NextCode;
										parts = code.Split(" ", true);
										if (parts != null && parts.Length >= 1)
										{
											increment = (int) parseHexLong(parts[0].Trim(), false);
										}
										break;
								}
								// Increment/Decrement value
								switch ((comm >> 20) & 0xF)
								{
									case 1:
									case 3:
									case 5: // Increment
										value += increment;
										break;
									case 2:
									case 4:
									case 6: // Decrement
										value -= increment;
										break;
								}
								// Write value back to memory
								switch ((comm >> 20) & 0xF)
								{
									case 1:
									case 2: // 8-bit
										mem.write8(addr, (sbyte) value);
										break;
									case 3:
									case 4: // 16-bit
										mem.write16(addr, (short) value);
										break;
									case 5:
									case 6: // 32-bit
										mem.write32(addr, value);
										break;
								}
								break;
							case 0x4: // 32-bit patch code.
								code = NextCode;
								parts = code.Split(" ", true);
								if (parts != null && parts.Length >= 1)
								{
									int data = (int) parseHexLong(parts[0].Trim(), false);
									int dataAdd = (int) parseHexLong(parts[1].Trim(), false);

									int maxAddr = (arg >> 16) & 0xFFFF;
									int stepAddr = (arg & 0xFFFF) * 4;
									for (int a = 0; a < maxAddr; a++)
									{
										if (Memory.isAddressGood(addr))
										{
											mem.write32(addr, data);
										}
										addr += stepAddr;
										data += dataAdd;
									}
								}
								break;
							case 0x5: // Memcpy command.
								code = NextCode;
								parts = code.Split(" ", true);
								if (parts != null && parts.Length >= 1)
								{
									int destAddr = (int) parseHexLong(parts[0].Trim(), false);
									if (Memory.isAddressGood(addr) && Memory.isAddressGood(destAddr))
									{
										mem.memcpy(destAddr, addr, arg);
									}
								}
								break;
							case 0x6: // Pointer commands
								code = NextCode;
								parts = code.Split(" ", true);
								if (parts != null && parts.Length >= 2)
								{
									int arg2 = (int) parseHexLong(parts[0].Trim(), false);
									int offset = (int) parseHexLong(parts[1].Trim(), false);
									int baseOffset = ((int)((uint)arg2 >> 20)) * 4;
									int @base = mem.read32(addr + baseOffset);
									int count = arg2 & 0xFFFF;
									int type = (arg2 >> 16) & 0xF;
									for (int i = 1; i < count; i++)
									{
										if (i + 1 < count)
										{
											code = NextCode;
											parts = code.Split(" ", true);
											if (parts != null && parts.Length >= 2)
											{
												int arg3 = (int) parseHexLong(parts[0].Trim(), false);
												int arg4 = (int) parseHexLong(parts[1].Trim(), false);
												int comm3 = (int)((uint)arg3 >> 28);
												switch (comm3)
												{
													case 0x1: // type copy byte
														int srcAddr = mem.read32(addr) + offset;
														int dstAddr = mem.read32(addr + baseOffset) + (arg3 & 0x0FFFFFFF);
														mem.memcpy(dstAddr, srcAddr, arg);
														type = -1; // Done
														break;
													case 0x2:
													case 0x3: // type pointer walk
														int walkOffset = arg3 & 0x0FFFFFFF;
														if (comm3 == 0x3)
														{
															walkOffset = -walkOffset;
														}
														@base = mem.read32(@base + walkOffset);
														int comm4 = (int)((uint)arg4 >> 28);
														switch (comm4)
														{
															case 0x2:
															case 0x3: // type pointer walk
																walkOffset = arg4 & 0x0FFFFFFF;
																if (comm4 == 0x3)
																{
																	walkOffset = -walkOffset;
																}
																@base = mem.read32(@base + walkOffset);
																break;
														}
														break;
													case 0x9: // type multi address write
														@base += arg3 & 0x0FFFFFFF;
														arg += arg4; // CHECKME Not sure about this?
														break;
												}
											}
										}
									}

									switch (type)
									{
										case 0: // 8-bit write
											mem.write8(@base + offset, (sbyte) arg);
											break;
										case 1: // 16-bit write
											mem.write16(@base + offset, (short) arg);
											break;
										case 2: // 32-bit write
											mem.write32(@base + offset, arg);
											break;
										case 3: // 8-bit inverse write
											mem.write8(@base - offset, (sbyte) arg);
											break;
										case 4: // 16-bit inverse write
											mem.write16(@base - offset, (short) arg);
											break;
										case 5: // 32-bit inverse write
											mem.write32(@base - offset, arg);
											break;
										case -1: // Operation already performed, nothing to do
											break;
									}
								}
								break;
							case 0x7: // Boolean commands.
								switch (arg >> 16)
								{
									case 0x0000: // 8-bit OR.
										if (Memory.isAddressGood(addr))
										{
											sbyte val1 = unchecked((sbyte)(arg & 0xFF));
											sbyte val2 = (sbyte) mem.read8(addr);
											mem.write8(addr, (sbyte)(val1 | val2));
										}
										break;
									case 0x0002: // 8-bit AND.
										if (Memory.isAddressGood(addr))
										{
											sbyte val1 = unchecked((sbyte)(arg & 0xFF));
											sbyte val2 = (sbyte) mem.read8(addr);
											mem.write8(addr, (sbyte)(val1 & val2));
										}
										break;
									case 0x0004: // 8-bit XOR.
										if (Memory.isAddressGood(addr))
										{
											sbyte val1 = unchecked((sbyte)(arg & 0xFF));
											sbyte val2 = (sbyte) mem.read8(addr);
											mem.write8(addr, (sbyte)(val1 ^ val2));
										}
										break;
									case 0x0001: // 16-bit OR.
										if (Memory.isAddressGood(addr))
										{
											short val1 = unchecked((short)(arg & 0xFFFF));
											short val2 = (short) mem.read16(addr);
											mem.write16(addr, (short)(val1 | val2));
										}
										break;
									case 0x0003: // 16-bit AND.
										if (Memory.isAddressGood(addr))
										{
											short val1 = unchecked((short)(arg & 0xFFFF));
											short val2 = (short) mem.read16(addr);
											mem.write16(addr, (short)(val1 & val2));
										}
										break;
									case 0x0005: // 16-bit XOR.
										if (Memory.isAddressGood(addr))
										{
											short val1 = unchecked((short)(arg & 0xFFFF));
											short val2 = (short) mem.read16(addr);
											mem.write16(addr, (short)(val1 ^ val2));
										}
										break;
								}
								break;
							case 0x8: // 8-bit and 16-bit patch code.
								code = NextCode;
								parts = code.Split(" ", true);
								if (parts != null && parts.Length >= 1)
								{
									int data = (int) parseHexLong(parts[0].Trim(), false);
									int dataAdd = (int) parseHexLong(parts[1].Trim(), false);

									bool is8Bit = (data >> 16) == 0x0000;
									int maxAddr = (arg >> 16) & 0xFFFF;
									int stepAddr = (arg & 0xFFFF) * (is8Bit ? 1 : 2);
									for (int a = 0; a < maxAddr; a++)
									{
										if (Memory.isAddressGood(addr))
										{
											if (is8Bit)
											{
												mem.write8(addr, unchecked((sbyte)(data & 0xFF)));
											}
											else
											{
												mem.write16(addr, unchecked((short)(data & 0xFFFF)));
											}
										}
										addr += stepAddr;
										data += dataAdd;
									}
								}
								break;
							case 0xB: // Time command
								// CHECKME Not sure what to do for this code?
								break;
							case 0xC: // Code stopper
								if (Memory.isAddressGood(addr))
								{
									value = mem.read32(addr);
									if (value != arg)
									{
										skipAllCodes();
									}
								}
								break;
							case 0xD: // Test commands & Jocker codes
								switch ((int)((uint)arg >> 28))
								{
									case 0:
									case 2: // Test commands, single skip
										bool is8Bit = (arg >> 28) == 0x2;
										if (Memory.isAddressGood(addr))
										{
											int memoryValue = is8Bit ? mem.read8(addr) : mem.read16(addr);
											int testValue = arg & (is8Bit ? 0xFF : 0xFFFF);
											bool executeNextLine = false;
											switch ((arg >> 20) & 0xF)
											{
												case 0x0: // Equal
													executeNextLine = memoryValue == testValue;
													break;
												case 0x1: // Not Equal
													executeNextLine = memoryValue != testValue;
													break;
												case 0x2: // Less Than
													executeNextLine = memoryValue < testValue;
													break;
												case 0x3: // Greater Than
													executeNextLine = memoryValue > testValue;
													break;
											}
											if (!executeNextLine)
											{
												skipCodes(1);
											}
										}
										break;
									case 4:
									case 5:
									case 6:
									case 7: // Address Test commands
										int addr1 = addr;
										int addr2 = getAddress(arg & 0x0FFFFFFF);
										if (Memory.isAddressGood(addr1) && Memory.isAddressGood(addr2))
										{
											code = NextCode;
											parts = code.Split(" ", true);
											if (parts != null && parts.Length >= 1)
											{
												int skip = (int) parseHexLong(parts[0].Trim(), false);
												int type = (int) parseHexLong(parts[1].Trim(), false);
												int value1 = 0;
												int value2 = 0;
												switch (type & 0xF)
												{
													case 0: // 8 bit
														value1 = mem.read8(addr1);
														value2 = mem.read8(addr2);
														break;
													case 1: // 16 bit
														value1 = mem.read16(addr1);
														value2 = mem.read16(addr2);
														break;
													case 2: // 32 bit
														value1 = mem.read32(addr1);
														value2 = mem.read32(addr2);
														break;
												}
												bool executeNextLines = false;
												switch ((int)((uint)arg >> 28))
												{
													case 4: // Equal
														executeNextLines = value1 == value2;
														break;
													case 5: // Not Equal
														executeNextLines = value1 != value2;
														break;
													case 6: // Less Than
														executeNextLines = value1 < value2;
														break;
													case 7: // Greater Than
														executeNextLines = value1 > value2;
														break;
												}
												if (!executeNextLines)
												{
													skipCodes(skip);
												}
											}
										}
										break;
									case 1: // Joker code
									case 3: // Inverse Joker code
										int testButtons = arg & 0x0FFFFFFF;
										int buttons = State.controller.Buttons;
										bool executeNextLines;
										if (((int)((uint)arg >> 28)) == 1)
										{
											executeNextLines = testButtons == buttons;
										}
										else
										{
											executeNextLines = testButtons != buttons;
										}
										if (!executeNextLines)
										{
											int skip = (comm & 0xFF) + 1;
											skipCodes(skip);
										}
										break;
								}
								break;
							case 0xE: // Test commands, multiple skip
								bool is8Bit = (comm >> 24) == 0x1;
								addr = getAddress(arg & 0x0FFFFFFF);
								if (Memory.isAddressGood(addr))
								{
									int memoryValue = is8Bit ? mem.read8(addr) : mem.read16(addr);
									int testValue = comm & (is8Bit ? 0xFF : 0xFFFF);
									bool executeNextLines = false;
									switch ((int)((uint)arg >> 28))
									{
										case 0x0: // Equal
											executeNextLines = memoryValue == testValue;
											break;
										case 0x1: // Not Equal
											executeNextLines = memoryValue != testValue;
											break;
										case 0x2: // Less Than
											executeNextLines = memoryValue < testValue;
											break;
										case 0x3: // Greater Than
											executeNextLines = memoryValue > testValue;
											break;
									}
									if (!executeNextLines)
									{
										int skip = (comm >> 16) & (is8Bit ? 0xFF : 0xFFF);
										skipCodes(skip);
									}
								}
								break;
						}
					}
				}

				// Exiting...
				cheats.onCheatsThreadEnded();
			}
		}

		public virtual string[] CodesList
		{
			get
			{
				return taCheats.Text.Split("\n");
			}
		}

		private void addCheatLine(string line)
		{
			string cheatCodes = taCheats.Text;
			if (string.ReferenceEquals(cheatCodes, null) || cheatCodes.Length <= 0)
			{
				cheatCodes = line;
			}
			else
			{
				cheatCodes += "\n" + line;
			}
			taCheats.Text = cheatCodes;
		}

		public virtual void onCheatsThreadEnded()
		{
			cheatsThread = null;
		}

		public override void dispose()
		{
			if (cheatsThread != null)
			{
				cheatsThread.exit();
			}

			Emulator.MainGUI.endWindowDialog();
			base.dispose();
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

			jScrollPane1 = new javax.swing.JScrollPane();
			taCheats = new javax.swing.JTextArea();
			btnImportCheatDB = new javax.swing.JButton();
			btnClear = new javax.swing.JButton();
			btnOnOff = new javax.swing.JToggleButton();

			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			Title = bundle.getString("CheatsGUI.title"); // NOI18N
			MinimumSize = new java.awt.Dimension(360, 360);
			Name = "frmCheatsGUI"; // NOI18N

			taCheats.Columns = 30;
			taCheats.Font = new java.awt.Font("Monospaced", 0, 12); // NOI18N
			taCheats.Rows = 20;
			taCheats.TabSize = 2;
			jScrollPane1.ViewportView = taCheats;

			btnImportCheatDB.Text = bundle.getString("CheatsGUI.btnImportCheatDB.text"); // NOI18N
			btnImportCheatDB.addActionListener(new ActionListenerAnonymousInnerClass(this));

			btnClear.Text = bundle.getString("ClearButton.text"); // NOI18N
			btnClear.addActionListener(new ActionListenerAnonymousInnerClass2(this));

			btnOnOff.Text = bundle.getString("CheatsGUI.btnOnOff.text"); // NOI18N
			btnOnOff.addActionListener(new ActionListenerAnonymousInnerClass3(this));

			javax.swing.GroupLayout layout = new javax.swing.GroupLayout(ContentPane);
			ContentPane.Layout = layout;
			layout.HorizontalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addContainerGap().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(jScrollPane1).addGroup(layout.createSequentialGroup().addComponent(btnOnOff, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addComponent(btnImportCheatDB, javax.swing.GroupLayout.DEFAULT_SIZE, 211, short.MaxValue).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addComponent(btnClear, javax.swing.GroupLayout.DEFAULT_SIZE, 100, short.MaxValue))).addContainerGap());
			layout.VerticalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addContainerGap().addComponent(jScrollPane1).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(btnImportCheatDB).addComponent(btnClear).addComponent(btnOnOff)).addContainerGap());

			pack();
		} // </editor-fold>//GEN-END:initComponents

		private class ActionListenerAnonymousInnerClass : java.awt.@event.ActionListener
		{
			private readonly CheatsGUI outerInstance;

			public ActionListenerAnonymousInnerClass(CheatsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.btnImportCheatDBActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass2 : java.awt.@event.ActionListener
		{
			private readonly CheatsGUI outerInstance;

			public ActionListenerAnonymousInnerClass2(CheatsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.btnClearActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass3 : java.awt.@event.ActionListener
		{
			private readonly CheatsGUI outerInstance;

			public ActionListenerAnonymousInnerClass3(CheatsGUI outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.btnOnOffActionPerformed(evt);
			}
		}

		private void btnClearActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_btnClearActionPerformed
			taCheats.Text = "";
		} //GEN-LAST:event_btnClearActionPerformed

		private void btnImportCheatDBActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_btnImportCheatDBActionPerformed
			File cheatDBFile = new File("cheat.db");
			if (cheatDBFile.canRead())
			{
				try
				{
					System.IO.StreamReader reader = new System.IO.StreamReader(cheatDBFile);
					bool insideApplicationid = false;
					while (reader.ready())
					{
						string line = reader.ReadLine();
						if (string.ReferenceEquals(line, null))
						{
							// end of file
							break;
						}
						line = line.Trim();
						if (line.StartsWith("_S ", StringComparison.Ordinal))
						{
							string applicationId = line.Substring(2).Trim().Replace("-", "");
							insideApplicationid = applicationId.Equals(State.discId, StringComparison.OrdinalIgnoreCase);
						}
						if (insideApplicationid)
						{
							// Add the line to the cheat codes
							addCheatLine(line);
						}
					}
					reader.Close();
				}
				catch (IOException e)
				{
					Emulator.log.error("Import from cheat.db", e);
				}
			}
			else
			{
				JpcspDialogManager.showInformation(this, java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp").getString("CheatsGUI.strReadFromDB.text"));
			}
		} //GEN-LAST:event_btnImportCheatDBActionPerformed

		private void btnOnOffActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_btnOnOffActionPerformed
			if (btnOnOff.Selected)
			{
				if (taCheats.Text.Empty)
				{
					JpcspDialogManager.showInformation(this, java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp").getString("CheatsGUI.strNoCheatsEntered.text"));
					btnOnOff.Selected = false;
					return;
				}

				if (cheatsThread == null)
				{
					taCheats.Editable = false;
					taCheats.Background = UIManager.getColor("Panel.background");
					btnClear.Enabled = false;
					btnImportCheatDB.Enabled = false;

					cheatsThread = new CheatsThread(this);
					cheatsThread.Priority = Thread.MIN_PRIORITY;
					cheatsThread.Name = "HLECheatThread";
					cheatsThread.Daemon = true;
					cheatsThread.Start();
				}
			}
			else
			{
				if (cheatsThread != null)
				{
					taCheats.Editable = true;
					taCheats.Background = UIManager.getColor("TextArea.background");
					btnClear.Enabled = true;
					btnImportCheatDB.Enabled = true;

					cheatsThread.exit();
				}
			}
		} //GEN-LAST:event_btnOnOffActionPerformed
		// Variables declaration - do not modify//GEN-BEGIN:variables
		private javax.swing.JButton btnClear;
		private javax.swing.JButton btnImportCheatDB;
		private javax.swing.JToggleButton btnOnOff;
		private javax.swing.JScrollPane jScrollPane1;
		private javax.swing.JTextArea taCheats;
		// End of variables declaration//GEN-END:variables
	}

}