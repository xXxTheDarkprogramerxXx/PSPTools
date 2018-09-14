using System;
using System.Collections.Generic;
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
namespace pspsharp.Debugger.MemoryBreakpoints
{

	using DebuggerMemory = pspsharp.memory.DebuggerMemory;
	using AccessType = pspsharp.Debugger.MemoryBreakpoints.MemoryBreakpoint.AccessType;
	using Constants = pspsharp.util.Constants;

	public class MemoryBreakpointsDialog : javax.swing.JDialog
	{

		private IList<MemoryBreakpoint> memoryBreakpoints;
		private MemoryBreakpointsModel memoryBreakpointsModel;
		private readonly int COL_STARTADDRESS = 0;
		private readonly int COL_ENDADDRESS = 1;
		private readonly int COL_ACCESSTYPE = 2;
		private readonly int COL_ACTIVE = 3;
		private readonly int COL_LAST = 4;
		private static readonly Font tableFont = new Font("Courier new", Font.PLAIN, 12);

		public MemoryBreakpointsDialog(java.awt.Frame parent) : base(parent)
		{

			memoryBreakpoints = ((DebuggerMemory) Memory.Instance).MemoryBreakpoints;
			memoryBreakpointsModel = new MemoryBreakpointsModel(this);

			initComponents();

			TableColumn accessType = tblBreakpoints.ColumnModel.getColumn(COL_ACCESSTYPE);
			JComboBox combo = new JComboBox();
			combo.addItem("READ");
			combo.addItem("WRITE");
			combo.addItem("READWRITE");
			accessType.CellEditor = new DefaultCellEditor(combo);

			tblBreakpoints.ColumnModel.getColumn(COL_STARTADDRESS).CellEditor = new AddressCellEditor(this);
			tblBreakpoints.ColumnModel.getColumn(COL_ENDADDRESS).CellEditor = new AddressCellEditor(this);

			tblBreakpoints.SelectionModel.addListSelectionListener(new ListSelectionListenerAnonymousInnerClass(this));

			tblBreakpoints.Model.addTableModelListener(new TableModelListenerAnonymousInnerClass(this));

			// copy trace settings to UI
			updateTraceSettings();
		}

		private class ListSelectionListenerAnonymousInnerClass : ListSelectionListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ListSelectionListenerAnonymousInnerClass(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void valueChanged(ListSelectionEvent e)
			{
				outerInstance.btnRemove.Enabled = !((ListSelectionModel) e.Source).SelectionEmpty;
			}
		}

		private class TableModelListenerAnonymousInnerClass : TableModelListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public TableModelListenerAnonymousInnerClass(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void tableChanged(TableModelEvent tme)
			{
				MemoryBreakpointsModel mbpm = (MemoryBreakpointsModel) tme.Source;
				outerInstance.btnExport.Enabled = mbpm.RowCount > 0;

				// validate entered addresses
				if (tme.Column == outerInstance.COL_STARTADDRESS || tme.Column == outerInstance.COL_ENDADDRESS)
				{
					for (int i = tme.FirstRow; i <= tme.LastRow; i++)
					{
						int start = Integer.decode(mbpm.getValueAt(i, outerInstance.COL_STARTADDRESS).ToString());
						int end = Integer.decode(mbpm.getValueAt(i, outerInstance.COL_ENDADDRESS).ToString());

						if (tme.Column == outerInstance.COL_STARTADDRESS && start > end)
						{
							mbpm.setValueAt(new int?(start), i, outerInstance.COL_ENDADDRESS);
						}
						if (tme.Column == outerInstance.COL_ENDADDRESS && end < start)
						{
							mbpm.setValueAt(new int?(end), i, outerInstance.COL_STARTADDRESS);
						}
					}
				}
			}
		}

		private void updateTraceSettings()
		{
			DebuggerMemory dbgmem = ((DebuggerMemory) Memory.Instance);

			cbTraceRead.Selected = dbgmem.traceMemoryRead;
			cbTraceRead8.Selected = dbgmem.traceMemoryRead8;
			cbTraceRead16.Selected = dbgmem.traceMemoryRead16;
			cbTraceRead32.Selected = dbgmem.traceMemoryRead32;

			cbTraceWrite.Selected = dbgmem.traceMemoryWrite;
			cbTraceWrite8.Selected = dbgmem.traceMemoryWrite8;
			cbTraceWrite16.Selected = dbgmem.traceMemoryWrite16;
			cbTraceWrite32.Selected = dbgmem.traceMemoryWrite32;

			chkPauseOnHit.Selected = dbgmem.pauseEmulatorOnMemoryBreakpoint;
		}

		private class AddressCellEditor : DefaultCellEditor
		{
			private readonly MemoryBreakpointsDialog outerInstance;


			internal const long serialVersionUID = 1L;

			public AddressCellEditor(MemoryBreakpointsDialog outerInstance) : base(new JTextField())
			{
				this.outerInstance = outerInstance;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.swing.JTextField tf = ((javax.swing.JTextField) getComponent());
				JTextField tf = ((JTextField) Component);
				tf.Font = tableFont;
			}

			public override object CellEditorValue
			{
				get
				{
					return ((JTextField) Component).Text;
				}
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public java.awt.Component getTableCellEditorComponent(final javax.swing.JTable table, final Object value, final boolean isSelected, final int row, final int column)
			public override Component getTableCellEditorComponent(JTable table, object value, bool isSelected, int row, int column)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.swing.JTextField tf = ((javax.swing.JTextField) getComponent());
				JTextField tf = ((JTextField) Component);
				tf.Text = string.Format("0x{0:X}", Integer.decode((string) table.Model.getValueAt(row, column)));

				// needed for double-click to work, otherwise the second click
				// is interpreted to position the caret
				SwingUtilities.invokeLater(() =>
				{
				// automatically select text after '0x'
				tf.select(2, tf.Text.length());
				});
				return tf;
			}
		}

		private class MemoryBreakpointsModel : AbstractTableModel
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public MemoryBreakpointsModel(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void setValueAt(object aValue, int rowIndex, int columnIndex)
			{
				MemoryBreakpoint mbp = outerInstance.memoryBreakpoints[rowIndex];
				switch (columnIndex)
				{
					case outerInstance.COL_STARTADDRESS:
					case outerInstance.COL_ENDADDRESS:
						int address = 0;
						if (aValue is string)
						{
							try
							{
								address = Integer.decode((string) aValue);
							}
							catch (System.FormatException)
							{
								// do nothing - cell will revert to previous value
								return;
							}
						}
						else if (aValue is int?)
						{
							address = ((int?) aValue).Value;
						}
						else
						{
							throw new System.ArgumentException("only String or Integer values allowed");
						}

						if (columnIndex == outerInstance.COL_STARTADDRESS)
						{
							mbp.StartAddress = address;
						}
						else if (columnIndex == outerInstance.COL_ENDADDRESS)
						{
							mbp.EndAddress = address;
						}
						break;
					case outerInstance.COL_ACCESSTYPE:
						string value = ((string) aValue).ToUpper();
						if (value.Equals("READ"))
						{
							mbp.Access = AccessType.READ;
						}
						else if (value.Equals("WRITE"))
						{
							mbp.Access = AccessType.WRITE;
						}
						else if (value.Equals("READWRITE"))
						{
							mbp.Access = AccessType.READWRITE;
						}
						break;
					case outerInstance.COL_ACTIVE:
						// TODO check if ranges overlap and prevent update
						mbp.Enabled = (bool?) aValue.Value;
						break;
					default:
						throw new System.ArgumentException("column out of range: " + columnIndex);
				}
				fireTableCellUpdated(rowIndex, columnIndex);
			}

			public override bool isCellEditable(int rowIndex, int columnIndex)
			{
				// all cells are editable
				return true;
			}

			public override Type getColumnClass(int columnIndex)
			{
				switch (columnIndex)
				{
					case outerInstance.COL_STARTADDRESS:
					case outerInstance.COL_ENDADDRESS:
					case outerInstance.COL_ACCESSTYPE:
						return typeof(string);
					case outerInstance.COL_ACTIVE:
						return typeof(Boolean);
					default:
						throw new System.ArgumentException("column out of range: " + columnIndex);
				}
			}

			public override string getColumnName(int column)
			{
				java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
				switch (column)
				{
					case outerInstance.COL_STARTADDRESS:
						return bundle.getString("MemoryBreakpointsDialog.strStartAddress.text");
					case outerInstance.COL_ENDADDRESS:
						return bundle.getString("MemoryBreakpointsDialog.strEndAddress.text");
					case outerInstance.COL_ACCESSTYPE:
						return bundle.getString("MemoryBreakpointsDialog.strAccess.text");
					case outerInstance.COL_ACTIVE:
						return bundle.getString("MemoryBreakpointsDialog.strActive.text");
					default:
						throw new System.ArgumentException("column out of range: " + column);
				}
			}

			public override int RowCount
			{
				get
				{
					return outerInstance.memoryBreakpoints.Count;
				}
			}

			public override int ColumnCount
			{
				get
				{
					return outerInstance.COL_LAST;
				}
			}

			public override object getValueAt(int rowIndex, int columnIndex)
			{
				MemoryBreakpoint mbp = outerInstance.memoryBreakpoints[rowIndex];
				switch (columnIndex)
				{
					case outerInstance.COL_STARTADDRESS:
						return string.Format("0x{0:X8}", mbp.StartAddress);
					case outerInstance.COL_ENDADDRESS:
						return string.Format("0x{0:X8}", mbp.EndAddress);
					case outerInstance.COL_ACCESSTYPE:
						switch (mbp.Access)
						{
							case READ:
								return "READ";
							case WRITE:
								return "WRITE";
							case READWRITE:
								return "READWRITE";
							default:
								throw new System.ArgumentException("unknown access type");
						}
					case outerInstance.COL_ACTIVE:
						return (mbp.Enabled) ? true : false;
					default:
						throw new System.ArgumentException("column out of range: " + columnIndex);
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

			btnAdd = new javax.swing.JButton();
			btnRemove = new javax.swing.JButton();
			jPanel2 = new javax.swing.JPanel();
			jSeparator1 = new javax.swing.JSeparator();
			jPanel1 = new javax.swing.JPanel();
			cbTraceRead = new javax.swing.JCheckBox();
			cbTraceWrite = new javax.swing.JCheckBox();
			cbTraceRead8 = new javax.swing.JCheckBox();
			cbTraceWrite8 = new javax.swing.JCheckBox();
			cbTraceRead16 = new javax.swing.JCheckBox();
			cbTraceWrite16 = new javax.swing.JCheckBox();
			cbTraceRead32 = new javax.swing.JCheckBox();
			cbTraceWrite32 = new javax.swing.JCheckBox();
			chkPauseOnHit = new javax.swing.JCheckBox();
			btnClose = new javax.swing.JButton();
			btnExport = new javax.swing.JButton();
			btnImport = new javax.swing.JButton();
			jScrollPane2 = new javax.swing.JScrollPane();
			tblBreakpoints = new JTable();

			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			Title = bundle.getString("MemoryBreakpointsDialog.title"); // NOI18N
			LocationByPlatform = true;
			Name = "dialog"; // NOI18N

			btnAdd.Text = bundle.getString("MemoryBreakpointsDialog.btnAdd.text"); // NOI18N
			btnAdd.MaximumSize = new java.awt.Dimension(140, 25);
			btnAdd.MinimumSize = new java.awt.Dimension(140, 25);
			btnAdd.PreferredSize = new java.awt.Dimension(140, 25);
			btnAdd.addActionListener(new ActionListenerAnonymousInnerClass(this));

			btnRemove.Text = bundle.getString("MemoryBreakpointsDialog.btnRemove.text"); // NOI18N
			btnRemove.Enabled = false;
			btnRemove.MaximumSize = new java.awt.Dimension(140, 25);
			btnRemove.MinimumSize = new java.awt.Dimension(140, 25);
			btnRemove.PreferredSize = new java.awt.Dimension(140, 25);
			btnRemove.addActionListener(new ActionListenerAnonymousInnerClass2(this));

			jPanel1.Layout = new java.awt.GridLayout(5, 2);

			cbTraceRead.Text = bundle.getString("MemoryBreakpointsDialog.cbTraceRead.text"); // NOI18N
			cbTraceRead.addItemListener(new ItemListenerAnonymousInnerClass(this));
			jPanel1.add(cbTraceRead);

			cbTraceWrite.Text = bundle.getString("MemoryBreakpointsDialog.cbTraceWrite.text"); // NOI18N
			cbTraceWrite.addItemListener(new ItemListenerAnonymousInnerClass2(this));
			jPanel1.add(cbTraceWrite);

			cbTraceRead8.Text = bundle.getString("MemoryBreakpointsDialog.cbTraceRead8.text"); // NOI18N
			cbTraceRead8.addItemListener(new ItemListenerAnonymousInnerClass3(this));
			jPanel1.add(cbTraceRead8);

			cbTraceWrite8.Text = bundle.getString("MemoryBreakpointsDialog.cbTraceWrite8.text"); // NOI18N
			cbTraceWrite8.addItemListener(new ItemListenerAnonymousInnerClass4(this));
			jPanel1.add(cbTraceWrite8);

			cbTraceRead16.Text = bundle.getString("MemoryBreakpointsDialog.cbTraceRead16.text"); // NOI18N
			cbTraceRead16.addItemListener(new ItemListenerAnonymousInnerClass5(this));
			jPanel1.add(cbTraceRead16);

			cbTraceWrite16.Text = bundle.getString("MemoryBreakpointsDialog.cbTraceWrite16.text"); // NOI18N
			cbTraceWrite16.addItemListener(new ItemListenerAnonymousInnerClass6(this));
			jPanel1.add(cbTraceWrite16);

			cbTraceRead32.Text = bundle.getString("MemoryBreakpointsDialog.cbTraceRead32.text"); // NOI18N
			cbTraceRead32.addItemListener(new ItemListenerAnonymousInnerClass7(this));
			jPanel1.add(cbTraceRead32);

			cbTraceWrite32.Text = bundle.getString("MemoryBreakpointsDialog.cbTraceWrite32.text"); // NOI18N
			cbTraceWrite32.addItemListener(new ItemListenerAnonymousInnerClass8(this));
			jPanel1.add(cbTraceWrite32);

			chkPauseOnHit.Selected = ((DebuggerMemory)Memory.Instance).pauseEmulatorOnMemoryBreakpoint;
			chkPauseOnHit.Text = bundle.getString("MemoryBreakpointsDialog.chkPauseOnHit.text"); // NOI18N
			chkPauseOnHit.addItemListener(new ItemListenerAnonymousInnerClass9(this));
			jPanel1.add(chkPauseOnHit);

			btnClose.Text = bundle.getString("CloseButton.text"); // NOI18N
			btnClose.addActionListener(new ActionListenerAnonymousInnerClass3(this));

			btnExport.Text = bundle.getString("MemoryBreakpointsDialog.btnExport.text"); // NOI18N
			btnExport.Enabled = false;
			btnExport.addActionListener(new ActionListenerAnonymousInnerClass4(this));

			btnImport.Text = bundle.getString("MemoryBreakpointsDialog.btnImport.text"); // NOI18N
			btnImport.addActionListener(new ActionListenerAnonymousInnerClass5(this));

			javax.swing.GroupLayout jPanel2Layout = new javax.swing.GroupLayout(jPanel2);
			jPanel2.Layout = jPanel2Layout;
			jPanel2Layout.HorizontalGroup = jPanel2Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(jSeparator1).addGroup(jPanel2Layout.createSequentialGroup().addGap(0, 0, short.MaxValue).addComponent(btnImport).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(btnExport).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(btnClose)).addComponent(jPanel1, javax.swing.GroupLayout.DEFAULT_SIZE, 576, short.MaxValue);
			jPanel2Layout.VerticalGroup = jPanel2Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(jPanel2Layout.createSequentialGroup().addContainerGap().addComponent(jSeparator1, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(jPanel1, javax.swing.GroupLayout.PREFERRED_SIZE, 127, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGroup(jPanel2Layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(btnExport).addComponent(btnClose).addComponent(btnImport)).addGap(31, 31, 31));

			tblBreakpoints.Font = new Font("Courier New", 0, 12); // NOI18N
			tblBreakpoints.Model = memoryBreakpointsModel;
			tblBreakpoints.SelectionMode = ListSelectionModel.SINGLE_SELECTION;
			jScrollPane2.ViewportView = tblBreakpoints;

			javax.swing.GroupLayout layout = new javax.swing.GroupLayout(ContentPane);
			ContentPane.Layout = layout;
			layout.HorizontalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addContainerGap().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.TRAILING).addComponent(jScrollPane2, javax.swing.GroupLayout.PREFERRED_SIZE, 0, short.MaxValue).addGroup(javax.swing.GroupLayout.Alignment.LEADING, layout.createSequentialGroup().addGap(0, 0, short.MaxValue).addComponent(btnAdd, javax.swing.GroupLayout.PREFERRED_SIZE, 140, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(btnRemove, javax.swing.GroupLayout.PREFERRED_SIZE, 140, javax.swing.GroupLayout.PREFERRED_SIZE)).addComponent(jPanel2, javax.swing.GroupLayout.Alignment.LEADING, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue)).addContainerGap());
			layout.VerticalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addContainerGap().addComponent(jScrollPane2, javax.swing.GroupLayout.DEFAULT_SIZE, 104, short.MaxValue).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(btnAdd, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(btnRemove, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addGap(18, 18, 18).addComponent(jPanel2, javax.swing.GroupLayout.PREFERRED_SIZE, 178, javax.swing.GroupLayout.PREFERRED_SIZE).addContainerGap());

			pack();
		} // </editor-fold>//GEN-END:initComponents

		private class ActionListenerAnonymousInnerClass : java.awt.@event.ActionListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ActionListenerAnonymousInnerClass(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.btnAddActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass2 : java.awt.@event.ActionListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ActionListenerAnonymousInnerClass2(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.btnRemoveActionPerformed(evt);
			}
		}

		private class ItemListenerAnonymousInnerClass : java.awt.@event.ItemListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ItemListenerAnonymousInnerClass(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void itemStateChanged(java.awt.@event.ItemEvent evt)
			{
				outerInstance.cbTraceReadItemStateChanged(evt);
			}
		}

		private class ItemListenerAnonymousInnerClass2 : java.awt.@event.ItemListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ItemListenerAnonymousInnerClass2(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void itemStateChanged(java.awt.@event.ItemEvent evt)
			{
				outerInstance.cbTraceWriteItemStateChanged(evt);
			}
		}

		private class ItemListenerAnonymousInnerClass3 : java.awt.@event.ItemListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ItemListenerAnonymousInnerClass3(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void itemStateChanged(java.awt.@event.ItemEvent evt)
			{
				outerInstance.cbTraceRead8ItemStateChanged(evt);
			}
		}

		private class ItemListenerAnonymousInnerClass4 : java.awt.@event.ItemListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ItemListenerAnonymousInnerClass4(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void itemStateChanged(java.awt.@event.ItemEvent evt)
			{
				outerInstance.cbTraceWrite8ItemStateChanged(evt);
			}
		}

		private class ItemListenerAnonymousInnerClass5 : java.awt.@event.ItemListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ItemListenerAnonymousInnerClass5(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void itemStateChanged(java.awt.@event.ItemEvent evt)
			{
				outerInstance.cbTraceRead16ItemStateChanged(evt);
			}
		}

		private class ItemListenerAnonymousInnerClass6 : java.awt.@event.ItemListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ItemListenerAnonymousInnerClass6(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void itemStateChanged(java.awt.@event.ItemEvent evt)
			{
				outerInstance.cbTraceWrite16ItemStateChanged(evt);
			}
		}

		private class ItemListenerAnonymousInnerClass7 : java.awt.@event.ItemListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ItemListenerAnonymousInnerClass7(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void itemStateChanged(java.awt.@event.ItemEvent evt)
			{
				outerInstance.cbTraceRead32ItemStateChanged(evt);
			}
		}

		private class ItemListenerAnonymousInnerClass8 : java.awt.@event.ItemListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ItemListenerAnonymousInnerClass8(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void itemStateChanged(java.awt.@event.ItemEvent evt)
			{
				outerInstance.cbTraceWrite32ItemStateChanged(evt);
			}
		}

		private class ItemListenerAnonymousInnerClass9 : java.awt.@event.ItemListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ItemListenerAnonymousInnerClass9(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void itemStateChanged(java.awt.@event.ItemEvent evt)
			{
				outerInstance.chkPauseOnHitItemStateChanged(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass3 : java.awt.@event.ActionListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ActionListenerAnonymousInnerClass3(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.btnCloseActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass4 : java.awt.@event.ActionListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ActionListenerAnonymousInnerClass4(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.btnExportActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass5 : java.awt.@event.ActionListener
		{
			private readonly MemoryBreakpointsDialog outerInstance;

			public ActionListenerAnonymousInnerClass5(MemoryBreakpointsDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(java.awt.@event.ActionEvent evt)
			{
				outerInstance.btnImportActionPerformed(evt);
			}
		}

		private void btnCloseActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_btnCloseActionPerformed
			Visible = false;
		} //GEN-LAST:event_btnCloseActionPerformed

		private void chkPauseOnHitItemStateChanged(java.awt.@event.ItemEvent evt)
		{ //GEN-FIRST:event_chkPauseOnHitItemStateChanged
			((DebuggerMemory) Memory.Instance).pauseEmulatorOnMemoryBreakpoint = chkPauseOnHit.Selected;
		} //GEN-LAST:event_chkPauseOnHitItemStateChanged

		private void btnAddActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_btnAddActionPerformed
			MemoryBreakpoint mbp = new MemoryBreakpoint();
			memoryBreakpoints.Add(mbp);
			memoryBreakpointsModel.fireTableRowsInserted(memoryBreakpoints.Count - 1, memoryBreakpoints.Count - 1);
		} //GEN-LAST:event_btnAddActionPerformed

		private void btnRemoveActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_btnRemoveActionPerformed
			int row = tblBreakpoints.SelectedRow;
			MemoryBreakpoint mbp = memoryBreakpoints.RemoveAt(row);

			// make sure breakpoint is uninstalled after being removed
			mbp.Enabled = false;
			memoryBreakpointsModel.fireTableRowsDeleted(row, row);

			// after removal no item is selected - so disable the button once again
			btnRemove.Enabled = false;
		} //GEN-LAST:event_btnRemoveActionPerformed

		private void cbTraceReadItemStateChanged(java.awt.@event.ItemEvent evt)
		{ //GEN-FIRST:event_cbTraceReadItemStateChanged
			((DebuggerMemory) Memory.Instance).traceMemoryRead = cbTraceRead.Selected;
		} //GEN-LAST:event_cbTraceReadItemStateChanged

		private void cbTraceRead8ItemStateChanged(java.awt.@event.ItemEvent evt)
		{ //GEN-FIRST:event_cbTraceRead8ItemStateChanged
			((DebuggerMemory) Memory.Instance).traceMemoryRead8 = cbTraceRead8.Selected;
		} //GEN-LAST:event_cbTraceRead8ItemStateChanged

		private void cbTraceRead16ItemStateChanged(java.awt.@event.ItemEvent evt)
		{ //GEN-FIRST:event_cbTraceRead16ItemStateChanged
			((DebuggerMemory) Memory.Instance).traceMemoryRead16 = cbTraceRead16.Selected;
		} //GEN-LAST:event_cbTraceRead16ItemStateChanged

		private void cbTraceRead32ItemStateChanged(java.awt.@event.ItemEvent evt)
		{ //GEN-FIRST:event_cbTraceRead32ItemStateChanged
			((DebuggerMemory) Memory.Instance).traceMemoryRead32 = cbTraceRead32.Selected;
		} //GEN-LAST:event_cbTraceRead32ItemStateChanged

		private void cbTraceWriteItemStateChanged(java.awt.@event.ItemEvent evt)
		{ //GEN-FIRST:event_cbTraceWriteItemStateChanged
			((DebuggerMemory) Memory.Instance).traceMemoryWrite = cbTraceWrite.Selected;
		} //GEN-LAST:event_cbTraceWriteItemStateChanged

		private void cbTraceWrite8ItemStateChanged(java.awt.@event.ItemEvent evt)
		{ //GEN-FIRST:event_cbTraceWrite8ItemStateChanged
			((DebuggerMemory) Memory.Instance).traceMemoryWrite8 = cbTraceWrite8.Selected;
		} //GEN-LAST:event_cbTraceWrite8ItemStateChanged

		private void cbTraceWrite16ItemStateChanged(java.awt.@event.ItemEvent evt)
		{ //GEN-FIRST:event_cbTraceWrite16ItemStateChanged
			((DebuggerMemory) Memory.Instance).traceMemoryWrite16 = cbTraceWrite16.Selected;
		} //GEN-LAST:event_cbTraceWrite16ItemStateChanged

		private void cbTraceWrite32ItemStateChanged(java.awt.@event.ItemEvent evt)
		{ //GEN-FIRST:event_cbTraceWrite32ItemStateChanged
			((DebuggerMemory) Memory.Instance).traceMemoryWrite32 = cbTraceWrite32.Selected;
		} //GEN-LAST:event_cbTraceWrite32ItemStateChanged

		private void btnExportActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_btnExportActionPerformed
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.swing.JFileChooser fc = new javax.swing.JFileChooser();
			JFileChooser fc = new JFileChooser();
			fc.DialogTitle = bundle.getString("MemoryBreakpointsDialog.dlgExport.title");
			fc.SelectedFile = new File(State.discId + ".mbrk");
			fc.CurrentDirectory = new File(".");
			fc.addChoosableFileFilter(Constants.fltMemoryBreakpointFiles);
			fc.FileFilter = Constants.fltMemoryBreakpointFiles;

			if (fc.showSaveDialog(this) == JFileChooser.APPROVE_OPTION)
			{
				File f = fc.SelectedFile;
				if (f.exists())
				{
					int rc = MessageBox.Show(this, bundle.getString("ConsoleWindow.strFileExists.text"), bundle.getString("ConsoleWindow.strFileExistsTitle.text"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

					if (rc != DialogResult.Yes)
					{
						return;
					}
				}
				((DebuggerMemory) Memory.Instance).exportBreakpoints(fc.SelectedFile);
			}
		} //GEN-LAST:event_btnExportActionPerformed

		private void btnImportActionPerformed(java.awt.@event.ActionEvent evt)
		{ //GEN-FIRST:event_btnImportActionPerformed
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.swing.JFileChooser fc = new javax.swing.JFileChooser();
			JFileChooser fc = new JFileChooser();
			fc.DialogTitle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp").getString("MemoryBreakpointsDialog.dlgImport.title");
			fc.SelectedFile = new File(State.discId + ".mbrk");
			fc.CurrentDirectory = new File(".");
			fc.addChoosableFileFilter(Constants.fltMemoryBreakpointFiles);
			fc.FileFilter = Constants.fltMemoryBreakpointFiles;

			if (fc.showOpenDialog(this) == JFileChooser.APPROVE_OPTION)
			{
				((DebuggerMemory) Memory.Instance).importBreakpoints(fc.SelectedFile);
			}
			memoryBreakpointsModel.fireTableDataChanged();
			updateTraceSettings();
		} //GEN-LAST:event_btnImportActionPerformed
		// Variables declaration - do not modify//GEN-BEGIN:variables
		private javax.swing.JButton btnAdd;
		private javax.swing.JButton btnClose;
		private javax.swing.JButton btnExport;
		private javax.swing.JButton btnImport;
		private javax.swing.JButton btnRemove;
		private javax.swing.JCheckBox cbTraceRead;
		private javax.swing.JCheckBox cbTraceRead16;
		private javax.swing.JCheckBox cbTraceRead32;
		private javax.swing.JCheckBox cbTraceRead8;
		private javax.swing.JCheckBox cbTraceWrite;
		private javax.swing.JCheckBox cbTraceWrite16;
		private javax.swing.JCheckBox cbTraceWrite32;
		private javax.swing.JCheckBox cbTraceWrite8;
		private javax.swing.JCheckBox chkPauseOnHit;
		private javax.swing.JPanel jPanel1;
		private javax.swing.JPanel jPanel2;
		private javax.swing.JScrollPane jScrollPane2;
		private javax.swing.JSeparator jSeparator1;
		private JTable tblBreakpoints;
		// End of variables declaration//GEN-END:variables
	}

}