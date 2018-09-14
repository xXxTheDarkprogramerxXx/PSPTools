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
namespace pspsharp.Debugger.DisassemblerModule
{


	public class RegisterTable : JTable
	{

		private const long serialVersionUID = 1L;
		private static readonly Font tableFont = new Font("Courier new", Font.PLAIN, 12);
		private Dictionary<string, Color> highlights;

		private class Register
		{
			private readonly RegisterTable outerInstance;


			public Register(RegisterTable outerInstance, string name)
			{
				this.outerInstance = outerInstance;
				this.name = name;
				value = 0;
				changed = false;
				outerInstance.highlights = new Dictionary<string, Color>();
			}
			public string name;
			public int value;
			public bool changed;

			public override string ToString()
			{
				return string.Format("0x{0:X8}", value);
			}
		}

		public RegisterTable(string[] regnames) : base()
		{
			Font = tableFont;
			setDefaultRenderer(typeof(string), new RegisterNameRenderer(this));
			setDefaultRenderer(typeof(Register), new RegisterValueRenderer(this));
			setDefaultEditor(typeof(Register), new RegisterValueEditor(this));

			SelectionMode = ListSelectionModel.SINGLE_SELECTION;

			Registers = regnames;
		}

		public RegisterTable() : base()
		{
			Font = tableFont;
			setDefaultRenderer(typeof(string), new RegisterNameRenderer(this));
			setDefaultRenderer(typeof(Register), new RegisterValueRenderer(this));
			setDefaultEditor(typeof(Register), new RegisterValueEditor(this));

			SelectionMode = ListSelectionModel.SINGLE_SELECTION;
		}

		public string[] Registers
		{
			set
			{
				Model = new RegisterTableModel(this, value);
			}
		}

		public virtual void highlightRegister(string regname, Color color)
		{
			// strip leading '$'
			if (regname.StartsWith("$", StringComparison.Ordinal))
			{
				regname = regname.Substring(1);
			}
			highlights[regname.ToLower()] = color;
			repaint();
		}

		public virtual void clearRegisterHighlights()
		{
			highlights.Clear();
			repaint();
		}

		public override TableModel Model
		{
			set
			{
				// needed to allow setting model property in NetBeans to null
				if (value != null)
				{
					base.Model = value;
				}
			}
		}

		public virtual void resetChanges()
		{
			((RegisterTableModel) Model).resetChanges();
		}

		public virtual int getAddressAt(int rowIndex)
		{
			return ((Register)((RegisterTableModel) Model).getValueAt(rowIndex, 1)).value;
		}

		private class RegisterNameRenderer : JLabel, TableCellRenderer
		{
			private readonly RegisterTable outerInstance;


			internal const long serialVersionUID = 1L;

			public RegisterNameRenderer(RegisterTable outerInstance) : base()
			{
				this.outerInstance = outerInstance;
				Font = tableFont;
				Background = selectionBackground;
			}

			public override Component getTableCellRendererComponent(JTable table, object value, bool isSelected, bool hasFocus, int row, int column)
			{
				string reg = (string) table.Model.getValueAt(row, column);
				Text = reg;

				reg = reg.ToLower();
				if (outerInstance.highlights.ContainsKey(reg))
				{
					// shade the highlight color on selection
					if (isSelected)
					{
						Background = outerInstance.highlights[reg].darker();
					}
					else
					{
						Background = outerInstance.highlights[reg];
					}

					// always display the colour
					Opaque = true;
				}
				else
				{
					// handle regular selection
					Background = selectionBackground;
					Opaque = isSelected;
				}
				return this;
			}
		}

		internal class RegisterValueEditor : DefaultCellEditor
		{
			private readonly RegisterTable outerInstance;


			internal const long serialVersionUID = 1L;

			public RegisterValueEditor(RegisterTable outerInstance) : base(new JTextField())
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
				tf.Text = string.Format("0x{0:X}", ((Register) table.Model.getValueAt(row, column)).value);

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

		private class RegisterValueRenderer : JLabel, TableCellRenderer
		{
			private readonly RegisterTable outerInstance;


			internal const long serialVersionUID = 1L;

			public RegisterValueRenderer(RegisterTable outerInstance) : base()
			{
				this.outerInstance = outerInstance;
				Font = tableFont;
				Background = selectionBackground;
			}

			public override Component getTableCellRendererComponent(JTable table, object color, bool isSelected, bool hasFocus, int row, int column)
			{

				Register reg = (Register) table.Model.getValueAt(row, column);
				Text = string.Format("0x{0:X8}", reg.value);

				if (reg.changed)
				{
					Foreground = Color.RED;
					Font = Font.deriveFont(Font.BOLD);
				}
				else
				{
					Foreground = Color.BLACK;
					Font = Font.deriveFont(Font.PLAIN);
				}

				// handle selection highlight
				Opaque = isSelected;

				return this;
			}
		}

		private class RegisterTableModel : AbstractTableModel
		{
			private readonly RegisterTable outerInstance;


			internal const long serialVersionUID = 1L;
			internal IList<Register> reginfo;

			public RegisterTableModel(RegisterTable outerInstance, string[] regnames) : base()
			{
				this.outerInstance = outerInstance;

				reginfo = new LinkedList<Register>();
				for (int i = 0; i < regnames.Length; i++)
				{
					reginfo.Add(new Register(outerInstance, regnames[i]));
				}
			}

			public virtual void resetChanges()
			{
				IEnumerator<Register> it = reginfo.GetEnumerator();
				while (it.MoveNext())
				{
					(it.Current).changed = false;
				}
				fireTableDataChanged();
			}

			public override Type getColumnClass(int columnIndex)
			{
				switch (columnIndex)
				{
					case 0:
						return typeof(string);
					case 1:
						return typeof(Register);
					default:
						throw new System.IndexOutOfRangeException("column index out of range");
				}
			}

			public override string getColumnName(int column)
			{
				switch (column)
				{
					case 0:
						return "REG";
					case 1:
						return "HEX";
					default:
						throw new System.IndexOutOfRangeException("column index out of range");
				}
			}

			public override bool isCellEditable(int rowIndex, int columnIndex)
			{
				// only the values of the registers are editable
				return (columnIndex == 1);
			}

			public override int ColumnCount
			{
				get
				{
					// REG, HEX
					return 2;
				}
			}

			public override int RowCount
			{
				get
				{
					return reginfo.Count;
				}
			}

			public override object getValueAt(int rowIndex, int columnIndex)
			{
				switch (columnIndex)
				{
					case 0:
						return reginfo[rowIndex].name;
					case 1:
						return reginfo[rowIndex];
					default:
						throw new System.IndexOutOfRangeException("column index out of range");
				}
			}

			public override void setValueAt(object aValue, int rowIndex, int columnIndex)
			{
				int value;
				if (aValue is int?)
				{
					value = (int?) aValue.Value;
				}
				else if (aValue is string)
				{
					try
					{
						value = Integer.decode((string) aValue);
					}
					catch (System.FormatException)
					{
						// ignore - will revert to old value instead
						return;
					}
				}
				else
				{
					throw new System.ArgumentException("setValueAt() will only handle Integer and String objects");
				}

				reginfo[rowIndex].changed = value != reginfo[rowIndex].value;
				if (reginfo[rowIndex].changed)
				{
					reginfo[rowIndex].value = value;
					fireTableCellUpdated(rowIndex, columnIndex);
				}
			}
		}
	}

}