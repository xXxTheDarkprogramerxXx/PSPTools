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
namespace pspsharp.Debugger.DisassemblerModule
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common._ra;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static pspsharp.Allegrex.Common.gprNames;



	using CpuState = pspsharp.Allegrex.CpuState;
	using Decoder = pspsharp.Allegrex.Decoder;
	using GprState = pspsharp.Allegrex.GprState;
	using Instructions = pspsharp.Allegrex.Instructions;
	using Instruction = pspsharp.Allegrex.Common.Instruction;
	using Compiler = pspsharp.Allegrex.compiler.Compiler;
	using JpcspDialogManager = pspsharp.util.JpcspDialogManager;
	using Utilities = pspsharp.util.Utilities;

	using StyledListCellRenderer = com.jidesoft.list.StyledListCellRenderer;
	using StyleRange = com.jidesoft.swing.StyleRange;
	using StyledLabel = com.jidesoft.swing.StyledLabel;
	using MemoryBreakpointsDialog = pspsharp.Debugger.MemoryBreakpoints.MemoryBreakpointsDialog;
	using DebuggerMemory = pspsharp.memory.DebuggerMemory;
	using Constants = pspsharp.util.Constants;
	using Logger = org.apache.log4j.Logger;

	/// 
	/// <summary>
	/// @author shadow
	/// </summary>
	public class DisassemblerFrame : javax.swing.JFrame, ClipboardOwner
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			selectedRegNames = new string[selectedRegColors.Length];
		}


		private const long serialVersionUID = -8481807175706172292L;
		private int DebuggerPC;
		private int SelectedPC;
		private Emulator emu;
		private DefaultListModel listmodel = new DefaultListModel();
		private List<int> breakpoints = new List<int>();
		private int temporaryBreakpoint1;
		private int temporaryBreakpoint2;
		private bool stepOut;
		protected internal int gpi, gpo;
		private int selectedRegCount;
		private readonly Color[] selectedRegColors = new Color[]
		{
			new Color(128, 255, 255),
			new Color(255, 255, 128),
			new Color(128, 255, 128)
		};
		private string[] selectedRegNames;
		private readonly Color selectedAddressColor = new Color(255, 128, 255);
		private string selectedAddress;
		private MemoryBreakpointsDialog mbpDialog;
		private SearchTask searchTask;

		/// <summary>
		/// Creates new form DisassemblerFrame
		/// </summary>
		public DisassemblerFrame(Emulator emu)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			this.emu = emu;
			listmodel = new DefaultListModel();

			initComponents();
			RefreshButtons();

			// calculate the fixed cell height and width based on a dummy string
			disasmList.PrototypeCellValue = "PROTOTYPE";

			gprTable.addPropertyChangeListener(new PropertyChangeListenerAnonymousInnerClass(this));

			ViewTooltips.register(disasmList);
			disasmList.CellRenderer = new StyledListCellRendererAnonymousInnerClass(this);
			disasmList.addListSelectionListener(new ListSelectionListenerAnonymousInnerClass(this));

			RefreshDebugger(true);

			WindowPropSaver.loadWindowProperties(this);
		}

		private class PropertyChangeListenerAnonymousInnerClass : PropertyChangeListener
		{
			private readonly DisassemblerFrame outerInstance;

			public PropertyChangeListenerAnonymousInnerClass(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void propertyChange(PropertyChangeEvent evt)
			{
				if ("tableCellEditor".Equals(evt.PropertyName))
				{
					if (!outerInstance.gprTable.Editing)
					{
						// editor finished editing the cell
						int row = outerInstance.gprTable.EditingRow;
						int value = outerInstance.gprTable.getAddressAt(row);
						bool changedPC = false;

						CpuState cpu = Emulator.Processor.cpu;
						switch (row)
						{
							case 0:
								if (value % 4 == 0)
								{
									// PC value is valid - perform change
									cpu.pc = value;
									changedPC = true;
								}
								else
								{
									// reset entry to current PC - no change
									outerInstance.gprTable.setValueAt(cpu.pc, row, 1);
								}
								break;
							case 1:
								cpu.Hi = value;
								break;
							case 2:
								cpu.Lo = value;
								break;
							default:
								cpu.setRegister(row - 3, value);
								break;
						}

						if (changedPC)
						{
							SwingUtilities.invokeLater(() =>
							{
							outerInstance.RefreshDebuggerDisassembly(true);
							});
						}
					}
				}
			}
		}

		private class StyledListCellRendererAnonymousInnerClass : StyledListCellRenderer
		{
			private readonly DisassemblerFrame outerInstance;

			public StyledListCellRendererAnonymousInnerClass(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
				serialVersionUID = 3921020228217850610L;
			}

			private static readonly long serialVersionUID;

			protected internal override void customizeStyledLabel(JList list, object value, int index, bool isSelected, bool cellHasFocus)
			{
				base.customizeStyledLabel(list, value, index, isSelected, cellHasFocus);
				string text = Text;
				Border = BorderFactory.createEmptyBorder(0, 2, 0, 2);
				Icon = null;
				// highlight the selected line
				if (index == outerInstance.disasmListGetSelectedIndex())
				{
					Background = Color.LIGHT_GRAY;
				}
				outerInstance.customizeStyledLabel(this, text);
			}
		}

		private class ListSelectionListenerAnonymousInnerClass : ListSelectionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ListSelectionListenerAnonymousInnerClass(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void valueChanged(ListSelectionEvent e)
			{
				if (!e.ValueIsAdjusting)
				{
					// this is the only place we can use disasmList.getSelectedValue(),
					// all other places should go through disasmListGetSelectedValue()
					string text = (string) outerInstance.disasmList.SelectedValue;
					if (!string.ReferenceEquals(text, null))
					{
						// this is the only place we can use disasmList.getSelectedIndex(),
						// all other places should go through disasmListGetSelectedIndex()
						outerInstance.SelectedPC = outerInstance.DebuggerPC + outerInstance.disasmList.SelectedIndex * 4;
						outerInstance.updateSelectedRegisters(text);
						outerInstance.disasmList.clearSelection();
						outerInstance.disasmList.repaint();
					}
				}
			}
		}

		private void customizeStyledLabel(StyledLabel label, string text)
		{
			// breakpoint
			if (text.StartsWith("<*>", StringComparison.Ordinal))
			{
				label.addStyleRange(new StyleRange(0, 3, Font.BOLD, Color.RED));
			}

			// PC line highlighting
			// TODO highlight entire line except for breakpoint highlighted registers
			// it seems the longest style overrides any shorter styles (such as the register highlighting)
			if (text.Contains(string.Format("{0:X8}:", Emulator.Processor.cpu.pc)))
			{
				// highlight: entire line, except gutter
				//label.addStyleRange(new StyleRange(3, -1, Font.BOLD, Color.BLACK));

				// highlight: address, raw opcode, opcode. no operands.
				int length = 32;
				if (length > text.Length - 3)
				{
					length = text.Length - 3;
				}

				label.addStyleRange(new StyleRange(3, length, Font.BOLD, Color.BLACK));
				// testing label.addStyleRange(new StyleRange(3, length, Font.PLAIN, Color.RED, Color.GREEN, 0));

				// highlight gutter if there is no breakpoint
				if (!text.StartsWith("<*>", StringComparison.Ordinal))
				{
					label.addStyleRange(new StyleRange(0, 3, Font.BOLD, Color.BLACK, Color.YELLOW, 0));
				}
			}

			// selected line highlighting
			// moved to cell renderer, we can highlight the entire line independantly of StyleRange

			// syscall highlighting
			if (text.Contains(" ["))
			{
				int find = text.IndexOf(" [", StringComparison.Ordinal);
				label.addStyleRange(new StyleRange(find, -1, Font.PLAIN, Color.BLUE));
			}

			// alias highlighting
			if (text.Contains("<=>"))
			{
				int find = text.IndexOf("<=>", StringComparison.Ordinal);
				label.addStyleRange(new StyleRange(find, -1, Font.PLAIN, Color.GRAY));
			}

			// address highlighting
			if (!string.ReferenceEquals(selectedAddress, null) && text.Contains("0x" + selectedAddress) && !text.Contains("syscall"))
			{
				int find = text.IndexOf("0x" + selectedAddress, StringComparison.Ordinal);
				label.addStyleRange(new StyleRange(find, 10, Font.PLAIN, Color.BLACK, selectedAddressColor, 0));
			}
			else if (!string.ReferenceEquals(selectedAddress, null) && text.Contains(selectedAddress) && !text.Contains("syscall"))
			{
				int find = text.IndexOf(selectedAddress, StringComparison.Ordinal);
				label.addStyleRange(new StyleRange(find, 8, Font.PLAIN, Color.BLACK, selectedAddressColor, 0));
			}

			// register highlighting
			int lastfind = 0;

			// find register in disassembly
			while ((lastfind = text.IndexOf("$", lastfind, StringComparison.Ordinal)) != -1)
			{

				string regName = text.Substring(lastfind);
				for (int i = 0; i < gprNames.length; i++)
				{
					// we still need to check every possible register because a tracked register may not be the first operand
					if (!regName.StartsWith(gprNames[i], StringComparison.Ordinal))
					{
						continue;
					}
					// check for tracked register
					for (int j = 0; j < selectedRegCount; j++)
					{
						if (regName.StartsWith(selectedRegNames[j], StringComparison.Ordinal))
						{
							label.addStyleRange(new StyleRange(lastfind, 3, Font.PLAIN, Color.BLACK, selectedRegColors[j], 0));
						}
					}
					break;
				}
				// move on to the remainder of the disassembled line on the next iteration
				lastfind += 3;
			}
		}

		/// <summary>
		/// Delete breakpoints and reset to PC
		/// </summary>
		public virtual void resetDebugger()
		{
			DeleteAllBreakpoints();
			RefreshDebugger(true);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void SafeRefreshDebugger(final boolean moveToPC)
		public virtual void SafeRefreshDebugger(bool moveToPC)
		{
			SwingUtilities.invokeLater(() =>
			{
			RefreshDebugger(moveToPC);
			});
		}

		private void RefreshDebuggerDisassembly(bool moveToPC)
		{
			CpuState cpu = Emulator.Processor.cpu;
			int pc;

			if (moveToPC)
			{
				SelectedPC = DebuggerPC = cpu.pc;
			}

			ViewTooltips.unregister(disasmList);
			lock (listmodel)
			{
				listmodel.clear();

				// compute the number of visible rows, based on the widget's size
				int numVisibleRows = disasmList.Height / disasmList.FixedCellHeight;
				Memory mem = MemoryViewer.Memory;
				for (pc = DebuggerPC; pc < (DebuggerPC + numVisibleRows * 0x00000004); pc += 0x00000004)
				{
					if (MemoryViewer.isAddressGood(pc))
					{
						int opcode = mem.read32(pc);

						Instruction insn = Decoder.instruction(opcode);

						string line;
						if (breakpoints.IndexOf(pc) != -1)
						{
							line = string.Format("<*>{0:X8}:[{1:X8}]: {2}", pc, opcode, insn.disasm(pc, opcode));
						}
						else if (pc == cpu.pc)
						{
							line = string.Format("-->{0:X8}:[{1:X8}]: {2}", pc, opcode, insn.disasm(pc, opcode));
						}
						else
						{
							line = string.Format("   {0:X8}:[{1:X8}]: {2}", pc, opcode, insn.disasm(pc, opcode));
						}
						listmodel.addElement(line);

						// update register highlighting
						if (pc == SelectedPC)
						{
							updateSelectedRegisters(line);
						}

					}
					else
					{
						listmodel.addElement(string.Format("   0x{0:X8}: invalid address", pc));
					}
				}
			}
			ViewTooltips.register(disasmList);
		}

		private void RefreshDebuggerRegisters()
		{
			CpuState cpu = Emulator.Processor.cpu;

			// refresh registers
			// gpr
			gprTable.resetChanges();
			gprTable.setValueAt(cpu.pc, 0, 1);
			gprTable.setValueAt(cpu.Hi, 1, 1);
			gprTable.setValueAt(cpu.Lo, 2, 1);
			for (int i = 0; i < GprState.NUMBER_REGISTERS; i++)
			{
				gprTable.setValueAt(cpu.getRegister(i), 3 + i, 1);
			}

			// fpr
			for (int i = 0; i < cpu.fpr.Length; i++)
			{
				cop1Table.setValueAt(cpu.fpr[i], i, 1);
			}

			// vfpu
			VfpuFrame.Instance.updateRegisters(cpu);
		}

		public void RefreshDebugger(bool moveToPC)
		{
			RefreshDebuggerDisassembly(moveToPC);
			RefreshDebuggerRegisters();

			// enable memory breakpoint manager if debugger memory is available
			ManageMemBreaks.Enabled = Memory.Instance is DebuggerMemory;
			miManageMemoryBreakpoints.Enabled = Memory.Instance is DebuggerMemory;
		}

		private void updateSelectedRegisters(string text)
		{

			// selected address (highlight constant branch/jump addresses)
			selectedAddress = null;
			int find = text.IndexOf(" 0x", StringComparison.Ordinal);
			if (find != -1 && (find + 11) <= text.Length && text[find + 7] != ' ')
			{
				selectedAddress = text.Substring(find + 3, 8);
			}

			// clear tracked registers and reset table highlighting
			selectedRegCount = 0;
			gprTable.clearRegisterHighlights();

			int lastFind = 0;
			while ((lastFind = text.IndexOf("$", lastFind, StringComparison.Ordinal)) != -1 && selectedRegCount < selectedRegColors.Length)
			{

				// find register in disassembly
				string regName = text.Substring(lastFind);
				for (int i = 0; i < gprNames.length; i++)
				{
					if (!regName.StartsWith(gprNames[i], StringComparison.Ordinal))
					{
						continue;
					}
					// check if we are already tracking this register
					bool found = false;
					for (int j = 0; j < selectedRegCount && !found; j++)
					{
						found = regName.StartsWith(selectedRegNames[j], StringComparison.Ordinal);
					}

					// start tracking this register and update the highlighting
					// of the table
					if (!found)
					{
						selectedRegNames[selectedRegCount] = gprNames[i];
						gprTable.highlightRegister(selectedRegNames[selectedRegCount], selectedRegColors[selectedRegCount]);
						selectedRegCount++;
					}
					break;
				}
				// move on to the remainder of the disassembled line
				lastFind += 3;
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

			DisMenu = new javax.swing.JPopupMenu();
			CopyAddress = new javax.swing.JMenuItem();
			CopyAll = new javax.swing.JMenuItem();
			BranchOrJump = new javax.swing.JMenuItem();
			SetPCToCursor = new javax.swing.JMenuItem();
			RegMenu = new javax.swing.JPopupMenu();
			CopyValue = new javax.swing.JMenuItem();
			tbDisasm = new javax.swing.JToolBar();
			RunDebugger = new javax.swing.JToggleButton();
			PauseDebugger = new javax.swing.JToggleButton();
			jSeparator1 = new javax.swing.JToolBar.Separator();
			btnStepInto = new JButton();
			btnStepOver = new JButton();
			btnStepOut = new JButton();
			jSeparator2 = new javax.swing.JToolBar.Separator();
			ResetToPCbutton = new JButton();
			JumpToAddress = new JButton();
			jSeparator4 = new javax.swing.JToolBar.Separator();
			DumpCodeToText = new JButton();
			tbBreakpoints = new javax.swing.JToolBar();
			AddBreakpoint = new JButton();
			DeleteBreakpoint = new JButton();
			DeleteAllBreakpoints_Renamed = new JButton();
			jSeparator3 = new javax.swing.JToolBar.Separator();
			ManageMemBreaks = new JButton();
			jSeparator7 = new javax.swing.JToolBar.Separator();
			ImportBreaks = new JButton();
			ExportBreaks = new JButton();
			disasmList = new JList(listmodel);
			disasmTabs = new javax.swing.JTabbedPane();
			gprTable = new pspsharp.Debugger.DisassemblerModule.RegisterTable();
			cop0Table = new JTable();
			cop1Table = new JTable();
			miscPanel = new javax.swing.JPanel();
			gpiButton1 = new javax.swing.JToggleButton();
			gpiButton2 = new javax.swing.JToggleButton();
			gpiButton3 = new javax.swing.JToggleButton();
			gpiButton4 = new javax.swing.JToggleButton();
			gpiButton5 = new javax.swing.JToggleButton();
			gpiButton6 = new javax.swing.JToggleButton();
			gpiButton7 = new javax.swing.JToggleButton();
			gpiButton8 = new javax.swing.JToggleButton();
			gpoLabel1 = new javax.swing.JLabel();
			gpoLabel2 = new javax.swing.JLabel();
			gpoLabel3 = new javax.swing.JLabel();
			gpoLabel4 = new javax.swing.JLabel();
			gpoLabel5 = new javax.swing.JLabel();
			gpoLabel6 = new javax.swing.JLabel();
			gpoLabel7 = new javax.swing.JLabel();
			gpoLabel8 = new javax.swing.JLabel();
			gpioLabel = new javax.swing.JLabel();
			lblCaptureReplay = new javax.swing.JLabel();
			btnCapture = new javax.swing.JToggleButton();
			btnReplay = new javax.swing.JToggleButton();
			lblDumpState = new javax.swing.JLabel();
			btnDumpDebugState = new JButton();
			txtSearch = new javax.swing.JTextField();
			lblSearch = new javax.swing.JLabel();
			prgBarSearch = new javax.swing.JProgressBar();
			btnCancelSearch = new JButton();
			statusPanel = new javax.swing.JPanel();
			statusLabel = new javax.swing.JLabel();
			mbMain = new javax.swing.JMenuBar();
			mFile = new javax.swing.JMenu();
			miClose = new javax.swing.JMenuItem();
			mDebug = new javax.swing.JMenu();
			miRun = new javax.swing.JMenuItem();
			miPause = new javax.swing.JMenuItem();
			jSeparator9 = new javax.swing.JPopupMenu.Separator();
			miStepInto = new javax.swing.JMenuItem();
			miStepOver = new javax.swing.JMenuItem();
			miStepOut = new javax.swing.JMenuItem();
			jSeparator10 = new javax.swing.JPopupMenu.Separator();
			miResetToPC = new javax.swing.JMenuItem();
			miJumpTo = new javax.swing.JMenuItem();
			mBreakpoints = new javax.swing.JMenu();
			miNewBreakpoint = new javax.swing.JMenuItem();
			miDeleteBreakpoint = new javax.swing.JMenuItem();
			miDeleteAllBreakpoints = new javax.swing.JMenuItem();
			miImportBreakpoints = new javax.swing.JMenuItem();
			miExportBreakpoints = new javax.swing.JMenuItem();
			jSeparator11 = new javax.swing.JPopupMenu.Separator();
			miManageMemoryBreakpoints = new javax.swing.JMenuItem();
			mDisassembler = new javax.swing.JMenu();
			miDumpCode = new javax.swing.JMenuItem();

			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			CopyAddress.Text = bundle.getString("DisassemblerFrame.CopyAddress.text"); // NOI18N
			CopyAddress.addActionListener(new ActionListenerAnonymousInnerClass(this));
			DisMenu.add(CopyAddress);

			CopyAll.Text = bundle.getString("DisassemblerFrame.CopyAll.text"); // NOI18N
			CopyAll.addActionListener(new ActionListenerAnonymousInnerClass2(this));
			DisMenu.add(CopyAll);

			BranchOrJump.Text = bundle.getString("DisassemblerFrame.CopyBranchOrJump.text"); // NOI18N
			BranchOrJump.Enabled = false; //disable as default
			BranchOrJump.addActionListener(new ActionListenerAnonymousInnerClass3(this));
			DisMenu.add(BranchOrJump);

			SetPCToCursor.Text = bundle.getString("DisassemblerFrame.SetPCToCursor.text"); // NOI18N
			SetPCToCursor.addActionListener(new ActionListenerAnonymousInnerClass4(this));
			DisMenu.add(SetPCToCursor);

			CopyValue.Text = bundle.getString("DisassemblerFrame.CopyValue.text"); // NOI18N
			CopyValue.addActionListener(new ActionListenerAnonymousInnerClass5(this));
			RegMenu.add(CopyValue);

			Title = bundle.getString("DisassemblerFrame.title"); // NOI18N
			MinimumSize = new java.awt.Dimension(800, 700);
			Name = "frmDebugger"; // NOI18N

			tbDisasm.Floatable = false;
			tbDisasm.Rollover = true;
			tbDisasm.Opaque = false;

			RunDebugger.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/PlayIcon.png")); // NOI18N
			RunDebugger.ToolTipText = bundle.getString("DisassemblerFrame.miRun.text"); // NOI18N
			RunDebugger.Focusable = false;
			RunDebugger.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			RunDebugger.IconTextGap = 2;
			RunDebugger.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			RunDebugger.addActionListener(new ActionListenerAnonymousInnerClass6(this));
			tbDisasm.add(RunDebugger);

			PauseDebugger.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/PauseIcon.png")); // NOI18N
			PauseDebugger.ToolTipText = bundle.getString("DisassemblerFrame.miPause.text"); // NOI18N
			PauseDebugger.Focusable = false;
			PauseDebugger.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			PauseDebugger.IconTextGap = 2;
			PauseDebugger.InheritsPopupMenu = true;
			PauseDebugger.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			PauseDebugger.addActionListener(new ActionListenerAnonymousInnerClass7(this));
			tbDisasm.add(PauseDebugger);
			tbDisasm.add(jSeparator1);

			btnStepInto.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/StepIntoIcon.png")); // NOI18N
			btnStepInto.ToolTipText = bundle.getString("DisassemblerFrame.miStepInto.text"); // NOI18N
			btnStepInto.Focusable = false;
			btnStepInto.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			btnStepInto.IconTextGap = 2;
			btnStepInto.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			btnStepInto.addActionListener(new ActionListenerAnonymousInnerClass8(this));
			tbDisasm.add(btnStepInto);

			btnStepOver.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/StepOverIcon.png")); // NOI18N
			btnStepOver.ToolTipText = bundle.getString("DisassemblerFrame.miStepOver.text"); // NOI18N
			btnStepOver.Focusable = false;
			btnStepOver.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			btnStepOver.IconTextGap = 2;
			btnStepOver.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			btnStepOver.addActionListener(new ActionListenerAnonymousInnerClass9(this));
			tbDisasm.add(btnStepOver);

			btnStepOut.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/StepOutIcon.png")); // NOI18N
			btnStepOut.ToolTipText = bundle.getString("DisassemblerFrame.miStepOut.text"); // NOI18N
			btnStepOut.Focusable = false;
			btnStepOut.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			btnStepOut.IconTextGap = 2;
			btnStepOut.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			btnStepOut.addActionListener(new ActionListenerAnonymousInnerClass10(this));
			tbDisasm.add(btnStepOut);
			tbDisasm.add(jSeparator2);

			ResetToPCbutton.setIcon(new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/ResetToPc.png"))); // NOI18N
			ResetToPCbutton.setToolTipText(bundle.getString("DisassemblerFrame.miResetToPC.text")); // NOI18N
			ResetToPCbutton.setFocusable(false);
			ResetToPCbutton.setHorizontalTextPosition(javax.swing.SwingConstants.RIGHT);
			ResetToPCbutton.setIconTextGap(2);
			ResetToPCbutton.setVerticalTextPosition(javax.swing.SwingConstants.BOTTOM);
			ResetToPCbutton.addActionListener(new ActionListenerAnonymousInnerClass11(this));
			tbDisasm.add(ResetToPCbutton);

			JumpToAddress.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/JumpTo.png")); // NOI18N
			JumpToAddress.ToolTipText = bundle.getString("DisassemblerFrame.miJumpTo.text"); // NOI18N
			JumpToAddress.Focusable = false;
			JumpToAddress.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			JumpToAddress.IconTextGap = 2;
			JumpToAddress.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			JumpToAddress.addActionListener(new ActionListenerAnonymousInnerClass12(this));
			tbDisasm.add(JumpToAddress);
			tbDisasm.add(jSeparator4);

			DumpCodeToText.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/Dump.png")); // NOI18N
			DumpCodeToText.ToolTipText = bundle.getString("DisassemblerFrame.miDumpCode.text"); // NOI18N
			DumpCodeToText.Focusable = false;
			DumpCodeToText.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			DumpCodeToText.IconTextGap = 2;
			DumpCodeToText.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			DumpCodeToText.addActionListener(new ActionListenerAnonymousInnerClass13(this));
			tbDisasm.add(DumpCodeToText);

			tbBreakpoints.Floatable = false;
			tbBreakpoints.Rollover = true;
			tbBreakpoints.Opaque = false;

			AddBreakpoint.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/NewBreakpointIcon.png")); // NOI18N
			AddBreakpoint.ToolTipText = bundle.getString("DisassemblerFrame.miNewBreakpoint.text"); // NOI18N
			AddBreakpoint.Focusable = false;
			AddBreakpoint.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			AddBreakpoint.IconTextGap = 2;
			AddBreakpoint.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			AddBreakpoint.addActionListener(new ActionListenerAnonymousInnerClass14(this));
			tbBreakpoints.add(AddBreakpoint);

			DeleteBreakpoint.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/DeleteBreakpointIcon.png")); // NOI18N
			DeleteBreakpoint.ToolTipText = bundle.getString("DisassemblerFrame.miDeleteBreakpoint.text"); // NOI18N
			DeleteBreakpoint.Focusable = false;
			DeleteBreakpoint.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			DeleteBreakpoint.IconTextGap = 2;
			DeleteBreakpoint.InheritsPopupMenu = true;
			DeleteBreakpoint.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			DeleteBreakpoint.addActionListener(new ActionListenerAnonymousInnerClass15(this));
			tbBreakpoints.add(DeleteBreakpoint);

			DeleteAllBreakpoints_Renamed.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/DeleteAllBreakpointsIcon.png")); // NOI18N
			DeleteAllBreakpoints_Renamed.ToolTipText = bundle.getString("DisassemblerFrame.miDeleteAllBreakpoints.text"); // NOI18N
			DeleteAllBreakpoints_Renamed.Focusable = false;
			DeleteAllBreakpoints_Renamed.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			DeleteAllBreakpoints_Renamed.IconTextGap = 2;
			DeleteAllBreakpoints_Renamed.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			DeleteAllBreakpoints_Renamed.addActionListener(new ActionListenerAnonymousInnerClass16(this));
			tbBreakpoints.add(DeleteAllBreakpoints_Renamed);
			tbBreakpoints.add(jSeparator3);

			ManageMemBreaks.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/MemoryBreakpointsIcon.png")); // NOI18N
			ManageMemBreaks.ToolTipText = bundle.getString("DisassemblerFrame.miManageMemoryBreakpoints.text"); // NOI18N
			ManageMemBreaks.Focusable = false;
			ManageMemBreaks.HorizontalTextPosition = javax.swing.SwingConstants.CENTER;
			ManageMemBreaks.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			ManageMemBreaks.addActionListener(new ActionListenerAnonymousInnerClass17(this));
			tbBreakpoints.add(ManageMemBreaks);
			tbBreakpoints.add(jSeparator7);

			ImportBreaks.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/LoadStateIcon.png")); // NOI18N
			ImportBreaks.ToolTipText = bundle.getString("DisassemblerFrame.miImportBreakpoints.text"); // NOI18N
			ImportBreaks.Focusable = false;
			ImportBreaks.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			ImportBreaks.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			ImportBreaks.addActionListener(new ActionListenerAnonymousInnerClass18(this));
			tbBreakpoints.add(ImportBreaks);

			ExportBreaks.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/SaveStateIcon.png")); // NOI18N
			ExportBreaks.ToolTipText = bundle.getString("DisassemblerFrame.miExportBreakpoints.text"); // NOI18N
			ExportBreaks.Focusable = false;
			ExportBreaks.HorizontalTextPosition = javax.swing.SwingConstants.RIGHT;
			ExportBreaks.VerticalTextPosition = javax.swing.SwingConstants.BOTTOM;
			ExportBreaks.addActionListener(new ActionListenerAnonymousInnerClass19(this));
			tbBreakpoints.add(ExportBreaks);

			disasmList.Font = new Font("Courier New", 0, 12); // NOI18N
			disasmList.SelectionMode = javax.swing.ListSelectionModel.SINGLE_SELECTION;
			disasmList.MinimumSize = new java.awt.Dimension(500, 50);
			disasmList.addMouseWheelListener(new MouseWheelListenerAnonymousInnerClass(this));
			disasmList.addMouseListener(new MouseAdapterAnonymousInnerClass(this));
			disasmList.addComponentListener(new ComponentAdapterAnonymousInnerClass(this));
			disasmList.addKeyListener(new KeyAdapterAnonymousInnerClass(this));

			disasmTabs.MinimumSize = new java.awt.Dimension(280, 587);
			disasmTabs.PreferredSize = new java.awt.Dimension(280, 587);

			gprTable.Model = null;
			gprTable.Registers = new string[] {"PC", "HI", "LO", "zr", "at", "v0", "v1", "a0", "a1", "a2", "a3", "t0", "t1", "t2", "t3", "t4", "t5", "t6", "t7", "s0", "s1", "s2", "s3", "s4", "s5", "s6", "s7", "t8", "t9", "k0", "k1", "gp", "sp", "fp", "ra"};
			disasmTabs.addTab(bundle.getString("DisassemblerFrame.gprTable.TabConstraints.tabTitle"), gprTable); // NOI18N

			cop0Table.Model = new DefaultTableModelAnonymousInnerClass(this, new object [][] { }, new string [] { "REG", "HEX" });
			disasmTabs.addTab(bundle.getString("DisassemblerFrame.cop0Table.TabConstraints.tabTitle"), cop0Table); // NOI18N

			cop1Table.Font = new Font("Courier New", 0, 12); // NOI18N
			cop1Table.Model = new DefaultTableModelAnonymousInnerClass2(this, new object [][]
			{
				new object[] {"FPR0", null},
				new object[] {"FPR1", null},
				new object[] {"FPR2", null},
				new object[] {"FPR3", null},
				new object[] {"FPR4", null},
				new object[] {"FPR5", null},
				new object[] {"FPR6", null},
				new object[] {"FPR7", null},
				new object[] {"FPR8", null},
				new object[] {"FPR9", null},
				new object[] {"FPR10", null},
				new object[] {"FPR11", null},
				new object[] {"FPR12", null},
				new object[] {"FPR13", null},
				new object[] {"FPR14", null},
				new object[] {"FPR15", null},
				new object[] {"FPR16", null},
				new object[] {"FPR17", null},
				new object[] {"FPR18", null},
				new object[] {"FPR19", null},
				new object[] {"FPR20", null},
				new object[] {"FPR21", null},
				new object[] {"FPR22", null},
				new object[] {"FPR23", null},
				new object[] {"FPR24", null},
				new object[] {"FPR25", null},
				new object[] {"FPR26", null},
				new object[] {"FPR27", null},
				new object[] {"FPR28", null},
				new object[] {"FPR29", null},
				new object[] {"FPR30", null},
				new object[] {"FPR31", null}
			}, new string [] {"REG", "FLOAT"});
			cop1Table.addMouseListener(new MouseAdapterAnonymousInnerClass2(this));
			disasmTabs.addTab(bundle.getString("DisassemblerFrame.cop1Table.TabConstraints.tabTitle"), cop1Table); // NOI18N

			gpiButton1.Text = "1"; // NOI18N
			gpiButton1.Border = null;
			gpiButton1.PreferredSize = new java.awt.Dimension(16, 16);
			gpiButton1.addActionListener(new ActionListenerAnonymousInnerClass20(this));

			gpiButton2.Text = "2"; // NOI18N
			gpiButton2.Border = null;
			gpiButton2.PreferredSize = new java.awt.Dimension(16, 16);
			gpiButton2.addActionListener(new ActionListenerAnonymousInnerClass21(this));

			gpiButton3.Text = "3"; // NOI18N
			gpiButton3.Border = null;
			gpiButton3.PreferredSize = new java.awt.Dimension(16, 16);
			gpiButton3.addActionListener(new ActionListenerAnonymousInnerClass22(this));

			gpiButton4.Text = "4"; // NOI18N
			gpiButton4.Border = null;
			gpiButton4.PreferredSize = new java.awt.Dimension(16, 16);
			gpiButton4.addActionListener(new ActionListenerAnonymousInnerClass23(this));

			gpiButton5.Text = "5"; // NOI18N
			gpiButton5.Border = null;
			gpiButton5.PreferredSize = new java.awt.Dimension(16, 16);
			gpiButton5.addActionListener(new ActionListenerAnonymousInnerClass24(this));

			gpiButton6.Text = "6"; // NOI18N
			gpiButton6.Border = null;
			gpiButton6.PreferredSize = new java.awt.Dimension(16, 16);
			gpiButton6.addActionListener(new ActionListenerAnonymousInnerClass25(this));

			gpiButton7.Text = "7"; // NOI18N
			gpiButton7.Border = null;
			gpiButton7.PreferredSize = new java.awt.Dimension(16, 16);
			gpiButton7.addActionListener(new ActionListenerAnonymousInnerClass26(this));

			gpiButton8.Text = "8"; // NOI18N
			gpiButton8.Border = null;
			gpiButton8.PreferredSize = new java.awt.Dimension(16, 16);
			gpiButton8.addActionListener(new ActionListenerAnonymousInnerClass27(this));

			gpoLabel1.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/tick.gif")); // NOI18N
			gpoLabel1.Enabled = false;

			gpoLabel2.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/tick.gif")); // NOI18N
			gpoLabel2.Enabled = false;

			gpoLabel3.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/tick.gif")); // NOI18N
			gpoLabel3.Enabled = false;

			gpoLabel4.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/tick.gif")); // NOI18N
			gpoLabel4.Enabled = false;

			gpoLabel5.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/tick.gif")); // NOI18N
			gpoLabel5.Enabled = false;

			gpoLabel6.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/tick.gif")); // NOI18N
			gpoLabel6.Enabled = false;

			gpoLabel7.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/tick.gif")); // NOI18N
			gpoLabel7.Enabled = false;

			gpoLabel8.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/tick.gif")); // NOI18N
			gpoLabel8.Enabled = false;

			gpioLabel.Text = bundle.getString("DisassemblerFrame.gpioLabel.text"); // NOI18N

			lblCaptureReplay.Text = bundle.getString("DisassemblerFrame.lblCaptureReplay.text"); // NOI18N

			btnCapture.Text = bundle.getString("DisassemblerFrame.btnCapture.text"); // NOI18N
			btnCapture.addActionListener(new ActionListenerAnonymousInnerClass28(this));

			btnReplay.Text = bundle.getString("DisassemblerFrame.btnReplay.text"); // NOI18N
			btnReplay.addActionListener(new ActionListenerAnonymousInnerClass29(this));

			lblDumpState.Text = bundle.getString("DisassemblerFrame.lblDebugState.text"); // NOI18N

			btnDumpDebugState.Text = bundle.getString("DisassemblerFrame.btnDumpDebugState.text"); // NOI18N
			btnDumpDebugState.addActionListener(new ActionListenerAnonymousInnerClass30(this));

			txtSearch.addActionListener(new ActionListenerAnonymousInnerClass31(this));
			txtSearch.addFocusListener(new FocusAdapterAnonymousInnerClass(this));

			lblSearch.Text = bundle.getString("DisassemblerFrame.lblSearch.text"); // NOI18N

			btnCancelSearch.Text = bundle.getString("DisassemblerFrame.btnCancelSearch.text"); // NOI18N
			btnCancelSearch.Enabled = false;
			btnCancelSearch.addActionListener(new ActionListenerAnonymousInnerClass32(this));

			javax.swing.GroupLayout miscPanelLayout = new javax.swing.GroupLayout(miscPanel);
			miscPanel.Layout = miscPanelLayout;
			miscPanelLayout.HorizontalGroup = miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(miscPanelLayout.createSequentialGroup().addContainerGap().addGroup(miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING, false).addGroup(miscPanelLayout.createSequentialGroup().addGroup(miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(gpoLabel1).addComponent(gpiButton1, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(gpiButton2, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(gpoLabel2)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(gpiButton3, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(gpoLabel3)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(gpiButton4, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(gpoLabel4)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(gpiButton5, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(gpoLabel5)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(gpiButton6, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(gpoLabel6)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(gpiButton7, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(gpoLabel7)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(gpoLabel8).addComponent(gpiButton8, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE))).addComponent(txtSearch).addComponent(lblSearch).addComponent(gpioLabel).addComponent(lblCaptureReplay).addComponent(lblDumpState).addComponent(btnDumpDebugState, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(btnCapture, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(btnReplay, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(prgBarSearch, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addComponent(btnCancelSearch, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue)).addContainerGap());
			miscPanelLayout.VerticalGroup = miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(miscPanelLayout.createSequentialGroup().addContainerGap().addComponent(gpioLabel).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(miscPanelLayout.createSequentialGroup().addGroup(miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(gpoLabel1).addComponent(gpoLabel2).addComponent(gpoLabel3).addComponent(gpoLabel4).addComponent(gpoLabel5).addComponent(gpoLabel6).addComponent(gpoLabel7)).addGap(11, 11, 11).addGroup(miscPanelLayout.createParallelGroup(javax.swing.GroupLayout.Alignment.CENTER).addComponent(gpiButton1, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(gpiButton2, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(gpiButton3, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(gpiButton4, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(gpiButton5, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(gpiButton6, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(gpiButton7, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(gpiButton8, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addComponent(lblCaptureReplay)).addComponent(gpoLabel8)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(btnCapture).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(btnReplay).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addComponent(lblDumpState).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(btnDumpDebugState).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addComponent(lblSearch, javax.swing.GroupLayout.PREFERRED_SIZE, 11, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(txtSearch, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(0, 0, 0).addComponent(prgBarSearch, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(btnCancelSearch).addContainerGap(243, short.MaxValue));

			disasmTabs.addTab(bundle.getString("DisassemblerFrame.miscPanel.TabConstraints.tabTitle"), miscPanel); // NOI18N

			statusPanel.Border = BorderFactory.createBevelBorder(javax.swing.border.BevelBorder.LOWERED);
			statusPanel.Layout = new javax.swing.BoxLayout(statusPanel, javax.swing.BoxLayout.LINE_AXIS);

			statusLabel.Font = new Font("Dialog", 0, 12); // NOI18N
			statusLabel.Text = "ready"; // NOI18N
			statusPanel.add(statusLabel);

			mFile.Text = bundle.getString("DisassemblerFrame.mFile.text"); // NOI18N

			miClose.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/CloseIcon.png")); // NOI18N
			miClose.Text = bundle.getString("CloseButton.text"); // NOI18N
			miClose.addActionListener(new ActionListenerAnonymousInnerClass33(this));
			mFile.add(miClose);

			mbMain.add(mFile);

			mDebug.Text = bundle.getString("DisassemblerFrame.mDebug.text"); // NOI18N
			mDebug.addActionListener(new ActionListenerAnonymousInnerClass34(this));

			miRun.Accelerator = javax.swing.KeyStroke.getKeyStroke(java.awt.@event.KeyEvent.VK_F8, 0);
			miRun.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/PlayIcon.png")); // NOI18N
			miRun.Text = bundle.getString("DisassemblerFrame.miRun.text"); // NOI18N
			miRun.addActionListener(new ActionListenerAnonymousInnerClass35(this));
			mDebug.add(miRun);

			miPause.Accelerator = javax.swing.KeyStroke.getKeyStroke(java.awt.@event.KeyEvent.VK_F9, 0);
			miPause.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/PauseIcon.png")); // NOI18N
			miPause.Text = bundle.getString("DisassemblerFrame.miPause.text"); // NOI18N
			miPause.addActionListener(new ActionListenerAnonymousInnerClass36(this));
			mDebug.add(miPause);
			mDebug.add(jSeparator9);

			miStepInto.Accelerator = javax.swing.KeyStroke.getKeyStroke(java.awt.@event.KeyEvent.VK_F5, 0);
			miStepInto.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/StepIntoIcon.png")); // NOI18N
			miStepInto.Text = bundle.getString("DisassemblerFrame.miStepInto.text"); // NOI18N
			miStepInto.addActionListener(new ActionListenerAnonymousInnerClass37(this));
			mDebug.add(miStepInto);

			miStepOver.Accelerator = javax.swing.KeyStroke.getKeyStroke(java.awt.@event.KeyEvent.VK_F6, 0);
			miStepOver.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/StepOverIcon.png")); // NOI18N
			miStepOver.Text = bundle.getString("DisassemblerFrame.miStepOver.text"); // NOI18N
			miStepOver.addActionListener(new ActionListenerAnonymousInnerClass38(this));
			mDebug.add(miStepOver);

			miStepOut.Accelerator = javax.swing.KeyStroke.getKeyStroke(java.awt.@event.KeyEvent.VK_F7, 0);
			miStepOut.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/StepOutIcon.png")); // NOI18N
			miStepOut.Text = bundle.getString("DisassemblerFrame.miStepOut.text"); // NOI18N
			mDebug.add(miStepOut);
			mDebug.add(jSeparator10);

			miResetToPC.setAccelerator(javax.swing.KeyStroke.getKeyStroke(java.awt.@event.KeyEvent.VK_P, java.awt.@event.InputEvent.CTRL_MASK));
			miResetToPC.setIcon(new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/ResetToPc.png"))); // NOI18N
			miResetToPC.setText(bundle.getString("DisassemblerFrame.miResetToPC.text")); // NOI18N
			miResetToPC.addActionListener(new ActionListenerAnonymousInnerClass39(this));
			mDebug.add(miResetToPC);

			miJumpTo.Accelerator = javax.swing.KeyStroke.getKeyStroke(java.awt.@event.KeyEvent.VK_J, java.awt.@event.InputEvent.CTRL_MASK);
			miJumpTo.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/JumpTo.png")); // NOI18N
			miJumpTo.Text = bundle.getString("DisassemblerFrame.miJumpTo.text"); // NOI18N
			miJumpTo.addActionListener(new ActionListenerAnonymousInnerClass40(this));
			mDebug.add(miJumpTo);

			mbMain.add(mDebug);

			mBreakpoints.Text = bundle.getString("DisassemblerFrame.mBreakpoints.text"); // NOI18N

			miNewBreakpoint.Accelerator = javax.swing.KeyStroke.getKeyStroke(java.awt.@event.KeyEvent.VK_B, java.awt.@event.InputEvent.CTRL_MASK);
			miNewBreakpoint.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/NewBreakpointIcon.png")); // NOI18N
			miNewBreakpoint.Text = bundle.getString("DisassemblerFrame.miNewBreakpoint.text"); // NOI18N
			miNewBreakpoint.addActionListener(new ActionListenerAnonymousInnerClass41(this));
			mBreakpoints.add(miNewBreakpoint);

			miDeleteBreakpoint.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/DeleteBreakpointIcon.png")); // NOI18N
			miDeleteBreakpoint.Text = bundle.getString("DisassemblerFrame.miDeleteBreakpoint.text"); // NOI18N
			miDeleteBreakpoint.addActionListener(new ActionListenerAnonymousInnerClass42(this));
			mBreakpoints.add(miDeleteBreakpoint);

			miDeleteAllBreakpoints.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/DeleteAllBreakpointsIcon.png")); // NOI18N
			miDeleteAllBreakpoints.Text = bundle.getString("DisassemblerFrame.miDeleteAllBreakpoints.text"); // NOI18N
			miDeleteAllBreakpoints.addActionListener(new ActionListenerAnonymousInnerClass43(this));
			mBreakpoints.add(miDeleteAllBreakpoints);

			miImportBreakpoints.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/LoadStateIcon.png")); // NOI18N
			miImportBreakpoints.Text = bundle.getString("DisassemblerFrame.miImportBreakpoints.text"); // NOI18N
			miImportBreakpoints.addActionListener(new ActionListenerAnonymousInnerClass44(this));
			mBreakpoints.add(miImportBreakpoints);

			miExportBreakpoints.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/SaveStateIcon.png")); // NOI18N
			miExportBreakpoints.Text = bundle.getString("DisassemblerFrame.miExportBreakpoints.text"); // NOI18N
			miExportBreakpoints.addActionListener(new ActionListenerAnonymousInnerClass45(this));
			mBreakpoints.add(miExportBreakpoints);
			mBreakpoints.add(jSeparator11);

			miManageMemoryBreakpoints.Accelerator = javax.swing.KeyStroke.getKeyStroke(java.awt.@event.KeyEvent.VK_M, java.awt.@event.InputEvent.CTRL_MASK);
			miManageMemoryBreakpoints.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/SettingsIcon.png")); // NOI18N
			miManageMemoryBreakpoints.Text = bundle.getString("DisassemblerFrame.miManageMemoryBreakpoints.text"); // NOI18N
			miManageMemoryBreakpoints.addActionListener(new ActionListenerAnonymousInnerClass46(this));
			mBreakpoints.add(miManageMemoryBreakpoints);

			mbMain.add(mBreakpoints);

			mDisassembler.Text = bundle.getString("DisassemblerFrame.mDisassembler.text"); // NOI18N

			miDumpCode.Accelerator = javax.swing.KeyStroke.getKeyStroke(java.awt.@event.KeyEvent.VK_D, java.awt.@event.InputEvent.CTRL_MASK);
			miDumpCode.Icon = new javax.swing.ImageIcon(this.GetType().getResource("/pspsharp/icons/Dump.png")); // NOI18N
			miDumpCode.Text = bundle.getString("DisassemblerFrame.miDumpCode.text"); // NOI18N
			miDumpCode.addActionListener(new ActionListenerAnonymousInnerClass47(this));
			mDisassembler.add(miDumpCode);

			mbMain.add(mDisassembler);

			JMenuBar = mbMain;

			javax.swing.GroupLayout layout = new javax.swing.GroupLayout(ContentPane);
			ContentPane.Layout = layout;
			layout.HorizontalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addComponent(tbDisasm, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(tbBreakpoints, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(0, 0, short.MaxValue)).addGroup(layout.createSequentialGroup().addContainerGap().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(statusPanel, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, short.MaxValue).addGroup(layout.createSequentialGroup().addComponent(disasmList, javax.swing.GroupLayout.DEFAULT_SIZE, 500, short.MaxValue).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(disasmTabs, javax.swing.GroupLayout.PREFERRED_SIZE, 265, javax.swing.GroupLayout.PREFERRED_SIZE))))).addContainerGap());
			layout.VerticalGroup = layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(layout.createSequentialGroup().addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(tbDisasm, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(tbBreakpoints, javax.swing.GroupLayout.PREFERRED_SIZE, 32, javax.swing.GroupLayout.PREFERRED_SIZE)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addComponent(disasmTabs, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(disasmList, javax.swing.GroupLayout.DEFAULT_SIZE, 647, short.MaxValue)).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(statusPanel, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE, javax.swing.GroupLayout.PREFERRED_SIZE).addContainerGap());

			pack();
		} // </editor-fold>//GEN-END:initComponents

		private class ActionListenerAnonymousInnerClass : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.CopyAddressActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass2 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass2(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.CopyAllActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass3 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass3(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.BranchOrJumpActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass4 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass4(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.SetPCToCursorActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass5 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass5(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.CopyValueActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass6 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass6(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.RunDebuggerActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass7 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass7(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.PauseDebuggerActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass8 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass8(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.StepIntoActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass9 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass9(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.StepOverActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass10 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass10(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.StepOutActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass11 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass11(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ResetToPCActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass12 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass12(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.JumpToAddressActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass13 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass13(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.DumpCodeToTextActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass14 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass14(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.AddBreakpointActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass15 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass15(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.DeleteBreakpointActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass16 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass16(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.DeleteAllBreakpointsActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass17 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass17(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ManageMemBreaksActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass18 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass18(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ImportBreaksActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass19 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass19(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ExportBreaksActionPerformed(evt);
			}
		}

		private class MouseWheelListenerAnonymousInnerClass : java.awt.@event.MouseWheelListener
		{
			private readonly DisassemblerFrame outerInstance;

			public MouseWheelListenerAnonymousInnerClass(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseWheelMoved(java.awt.@event.MouseWheelEvent evt)
			{
				outerInstance.disasmListMouseWheelMoved(evt);
			}
		}

		private class MouseAdapterAnonymousInnerClass : java.awt.@event.MouseAdapter
		{
			private readonly DisassemblerFrame outerInstance;

			public MouseAdapterAnonymousInnerClass(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.disasmListMouseClicked(evt);
			}
		}

		private class ComponentAdapterAnonymousInnerClass : java.awt.@event.ComponentAdapter
		{
			private readonly DisassemblerFrame outerInstance;

			public ComponentAdapterAnonymousInnerClass(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void componentResized(java.awt.@event.ComponentEvent evt)
			{
				outerInstance.disasmListComponentResized(evt);
			}
		}

		private class KeyAdapterAnonymousInnerClass : java.awt.@event.KeyAdapter
		{
			private readonly DisassemblerFrame outerInstance;

			public KeyAdapterAnonymousInnerClass(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void keyPressed(java.awt.@event.KeyEvent evt)
			{
				outerInstance.disasmListKeyPressed(evt);
			}
		}

		private class DefaultTableModelAnonymousInnerClass : javax.swing.table.DefaultTableModel
		{
			private readonly DisassemblerFrame outerInstance;

			public DefaultTableModelAnonymousInnerClass(DisassemblerFrame outerInstance, object[][] new, string[] new) : base(new object [][] { }, new string [] { "REG", "HEX" })
			{
				this.outerInstance = outerInstance;
				types = new Type [] {typeof(string), typeof(object)};
				canEdit = new bool [] {false, false};
			}

			internal Type[] types;
			internal bool[] canEdit;

			public Type getColumnClass(int columnIndex)
			{
				return types [columnIndex];
			}

			public bool isCellEditable(int rowIndex, int columnIndex)
			{
				return canEdit [columnIndex];
			}
		}

		private class DefaultTableModelAnonymousInnerClass2 : javax.swing.table.DefaultTableModel
		{
			private readonly DisassemblerFrame outerInstance;

			public DefaultTableModelAnonymousInnerClass2(DisassemblerFrame outerInstance, object[][] new, string[] new) : base(new object [][] {new object[] {"FPR0", null}, new object[] {"FPR1", null}, new object[] {"FPR2", null}, new object[] {"FPR3", null}, new object[] {"FPR4", null}, new object[] {"FPR5", null}, new object[] {"FPR6", null}, new object[] {"FPR7", null}, new object[] {"FPR8", null}, new object[] {"FPR9", null}, new object[] {"FPR10", null}, new object[] {"FPR11", null}, new object[] {"FPR12", null}, new object[] {"FPR13", null}, new object[] {"FPR14", null}, new object[] {"FPR15", null}, new object[] {"FPR16", null}, new object[] {"FPR17", null}, new object[] {"FPR18", null}, new object[] {"FPR19", null}, new object[] {"FPR20", null}, new object[] {"FPR21", null}, new object[] {"FPR22", null}, new object[] {"FPR23", null}, new object[] {"FPR24", null}, new object[] {"FPR25", null}, new object[] {"FPR26", null}, new object[] {"FPR27", null}, new object[] {"FPR28", null}, new object[] {"FPR29", null}, new object[] {"FPR30", null}, new object[] {"FPR31", null}}, new string [] {"REG", "FLOAT"})
			{
				this.outerInstance = outerInstance;
				types = new Type [] {typeof(string), typeof(Float)};
				canEdit = new bool [] {false, false};
			}

			internal Type[] types;
			internal bool[] canEdit;

			public Type getColumnClass(int columnIndex)
			{
				return types [columnIndex];
			}

			public bool isCellEditable(int rowIndex, int columnIndex)
			{
				return canEdit [columnIndex];
			}
		}

		private class MouseAdapterAnonymousInnerClass2 : java.awt.@event.MouseAdapter
		{
			private readonly DisassemblerFrame outerInstance;

			public MouseAdapterAnonymousInnerClass2(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void mouseClicked(java.awt.@event.MouseEvent evt)
			{
				outerInstance.cop1TableMouseClicked(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass20 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass20(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.gpiButton1ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass21 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass21(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.gpiButton2ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass22 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass22(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.gpiButton3ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass23 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass23(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.gpiButton4ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass24 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass24(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.gpiButton5ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass25 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass25(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.gpiButton6ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass26 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass26(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.gpiButton7ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass27 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass27(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.gpiButton8ActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass28 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass28(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.btnCaptureActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass29 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass29(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.btnReplayActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass30 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass30(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.btnDumpDebugStateActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass31 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass31(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.txtSearchActionPerformed(evt);
			}
		}

		private class FocusAdapterAnonymousInnerClass : java.awt.@event.FocusAdapter
		{
			private readonly DisassemblerFrame outerInstance;

			public FocusAdapterAnonymousInnerClass(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void focusGained(java.awt.@event.FocusEvent evt)
			{
				outerInstance.txtSearchFocusGained(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass32 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass32(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.btnCancelSearchActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass33 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass33(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.CloseActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass34 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass34(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.RunDebuggerActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass35 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass35(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.RunDebuggerActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass36 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass36(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.PauseDebuggerActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass37 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass37(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.StepIntoActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass38 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass38(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.StepOverActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass39 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass39(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ResetToPCActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass40 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass40(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.JumpToAddressActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass41 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass41(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.AddBreakpointActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass42 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass42(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.DeleteBreakpointActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass43 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass43(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.DeleteAllBreakpointsActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass44 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass44(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ImportBreaksActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass45 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass45(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ExportBreaksActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass46 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass46(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.ManageMemBreaksActionPerformed(evt);
			}
		}

		private class ActionListenerAnonymousInnerClass47 : java.awt.@event.ActionListener
		{
			private readonly DisassemblerFrame outerInstance;

			public ActionListenerAnonymousInnerClass47(DisassemblerFrame outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void actionPerformed(ActionEvent evt)
			{
				outerInstance.DumpCodeToTextActionPerformed(evt);
			}
		}

	private void disasmListKeyPressed(java.awt.@event.KeyEvent evt)
	{ //GEN-FIRST:event_disasmListKeyPressed
			int keyCode = evt.KeyCode;
			int numVisibleRows = disasmList.Height / disasmList.FixedCellHeight;

			switch (keyCode)
			{
				case java.awt.@event.KeyEvent.VK_DOWN:
					DebuggerPC += 4;
					RefreshDebuggerDisassembly(false);
					updateSelectedIndex();
					evt.consume();
					break;

				case java.awt.@event.KeyEvent.VK_UP:
					DebuggerPC -= 4;
					RefreshDebuggerDisassembly(false);
					updateSelectedIndex();
					evt.consume();
					break;

				case java.awt.@event.KeyEvent.VK_PAGE_UP:
					DebuggerPC -= numVisibleRows * 0x00000004;
					RefreshDebuggerDisassembly(false);
					updateSelectedIndex();
					evt.consume();
					break;

				case java.awt.@event.KeyEvent.VK_PAGE_DOWN:
					DebuggerPC += numVisibleRows * 0x00000004;
					RefreshDebuggerDisassembly(false);
					updateSelectedIndex();
					evt.consume();
					break;
			}
	} //GEN-LAST:event_disasmListKeyPressed

	private void disasmListMouseWheelMoved(java.awt.@event.MouseWheelEvent evt)
	{ //GEN-FIRST:event_disasmListMouseWheelMoved
			if (evt.WheelRotation < 0)
			{
				DebuggerPC -= 4;
				RefreshDebuggerDisassembly(false);
				updateSelectedIndex();
				evt.consume();
			}
			else
			{
				DebuggerPC += 4;
				RefreshDebuggerDisassembly(false);
				updateSelectedIndex();
				evt.consume();
			}
	} //GEN-LAST:event_disasmListMouseWheelMoved

		private void updateSelectedIndex()
		{
			int numVisibleRows = disasmList.Height / disasmList.FixedCellHeight;
			if (SelectedPC >= DebuggerPC && SelectedPC < DebuggerPC + numVisibleRows * 0x00000004)
			{
				disasmList.SelectedIndex = (SelectedPC - DebuggerPC) / 4;
			}
		}

		/// <summary>
		/// replacement for disasmList.getSelectedIndex() because there is no longer
		/// a selected index, we don't want the blue highlight from the operating
		/// system/look and feel, we want our own.
		/// </summary>
		private int disasmListGetSelectedIndex()
		{
			return (SelectedPC - DebuggerPC) / 4;
		}

		/// <summary>
		/// replacement for disasmList.getSelectedValue() because there is no longer
		/// a selected index, we don't want the blue highlight from the operating
		/// system/look and feel, we want our own.
		/// </summary>
		private object disasmListGetSelectedValue()
		{
			if (disasmListGetSelectedIndex() < 0)
			{
				return null;
			}
			return disasmList.Model.getElementAt(disasmListGetSelectedIndex());
		}

	private void ResetToPCActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ResetToPCActionPerformed
			RefreshDebuggerDisassembly(true);
	} //GEN-LAST:event_ResetToPCActionPerformed

	private void JumpToAddressActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_JumpToAddressActionPerformed
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp"); // NOI18N
			string input = (string) JOptionPane.showInputDialog(this, bundle.getString("DisassemblerFrame.strEnterToJump.text"), "pspsharp", JOptionPane.QUESTION_MESSAGE, null, null, string.Format("{0:x8}", Emulator.Processor.cpu.pc)); // NOI18N
			if (string.ReferenceEquals(input, null))
			{
				return;
			}
			try
			{
				int value = Utilities.parseAddress(input);
				DebuggerPC = value;
				SelectedPC = value;
			}
			catch (Exception)
			{
				MessageBox.Show(this, bundle.getString("MemoryViewer.strInvalidAddress.text"));
				return;
			}

			RefreshDebuggerDisassembly(false);
	} //GEN-LAST:event_JumpToAddressActionPerformed

	private void DumpCodeToTextActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_DumpCodeToTextActionPerformed
			DumpCodeDialog dlgDC = new DumpCodeDialog(this, DebuggerPC);
			dlgDC.Visible = true;

			if (dlgDC.ReturnValue != DumpCodeDialog.DUMPCODE_APPROVE)
			{
				return;
			}

			Logger.RootLogger.debug("Start address: " + dlgDC.StartAddress);
			Logger.RootLogger.debug("End address: " + dlgDC.EndAddress);
			Logger.RootLogger.debug("File name: " + dlgDC.Filename);

			System.IO.StreamWriter bufferedWriter = null;
			try
			{
				bufferedWriter = new System.IO.StreamWriter(dlgDC.Filename);
				bufferedWriter.Write("------- pspsharp DISASM -------");
				bufferedWriter.newLine();
				for (int i = dlgDC.StartAddress; i <= dlgDC.EndAddress; i += 4)
				{
					if (Memory.isAddressGood(i))
					{
						int opcode = Memory.Instance.read32(i);
						Instruction insn = Decoder.instruction(opcode);
						string disasm;
						try
						{
							disasm = insn.disasm(i, opcode);
						}
						catch (Exception)
						{
							disasm = "???";
						}
						bufferedWriter.BaseStream.WriteByte(string.Format("{0:X8}:[{1:X8}]: {2}", i, opcode, disasm));
					}
					else
					{
						// should we even both printing these?
						bufferedWriter.BaseStream.WriteByte(string.Format("{0:X8}: invalid address", i));
					}

					bufferedWriter.newLine();
				}
			}
			catch (FileNotFoundException ex)
			{
				Console.WriteLine(ex.ToString());
				Console.Write(ex.StackTrace);
			}
			catch (IOException ex)
			{
				Console.WriteLine(ex.ToString());
				Console.Write(ex.StackTrace);
			}
			finally
			{
				Utilities.close(bufferedWriter);
			}
	} //GEN-LAST:event_DumpCodeToTextActionPerformed

	// following methods are for the JPopmenu in Jlist
	private void CopyAddressActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_CopyAddressActionPerformed
			string value = (string) disasmListGetSelectedValue();
			string address = value.Substring(3, 8);
			StringSelection stringSelection = new StringSelection(address);
			Clipboard clipboard = Toolkit.DefaultToolkit.SystemClipboard;
			clipboard.setContents(stringSelection, this);
	} //GEN-LAST:event_CopyAddressActionPerformed

	private void CopyAllActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_CopyAllActionPerformed
			string value = (string) disasmListGetSelectedValue();
			StringSelection stringSelection = new StringSelection(value);
			Clipboard clipboard = Toolkit.DefaultToolkit.SystemClipboard;
			clipboard.setContents(stringSelection, this);
	} //GEN-LAST:event_CopyAllActionPerformed

	private void BranchOrJumpActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_BranchOrJumpActionPerformed
			string value = (string) disasmListGetSelectedValue();
			int address = value.IndexOf("0x", StringComparison.Ordinal);
			if (address == -1)
			{
				JpcspDialogManager.showError(this, "Can't find the jump or branch address");
				return;
			}
			string add = value.Substring(address + 2, value.Length - (address + 2));

			// Remove syscall code, if present
			int addressend = add.IndexOf(" ", StringComparison.Ordinal);
			if (addressend != -1)
			{
				add = add.Substring(0, addressend);
			}

			StringSelection stringSelection = new StringSelection(add);
			Clipboard clipboard = Toolkit.DefaultToolkit.SystemClipboard;
			clipboard.setContents(stringSelection, this);
	} //GEN-LAST:event_BranchOrJumpActionPerformed

		public override void lostOwnership(Clipboard aClipboard, Transferable aContents)
		{
			//do nothing
		}

	private void disasmListMouseClicked(java.awt.@event.MouseEvent evt)
	{ //GEN-FIRST:event_disasmListMouseClicked

			BranchOrJump.Enabled = false;
			SetPCToCursor.Enabled = false;

			if (SwingUtilities.isRightMouseButton(evt) && disasmList.locationToIndex(evt.Point) == disasmListGetSelectedIndex())
			{
				//check if we can enable branch or jump address copy
				string line = (string) disasmListGetSelectedValue();
				int finddot = line.IndexOf("]:", StringComparison.Ordinal);
				string opcode = line.Substring(finddot + 3, line.Length - (finddot + 3));
				if (opcode.StartsWith("b", StringComparison.Ordinal) || opcode.StartsWith("j", StringComparison.Ordinal)) //it is definately a branch or jump opcode
				{
					BranchOrJump.Enabled = true;
				}

				//check if we should enable set pc to cursor
				int addr = DebuggerPC + disasmListGetSelectedIndex() * 4;
				if (Memory.isAddressGood(addr))
				{
					SetPCToCursor.Enabled = true;
				}

				DisMenu.show(disasmList, evt.X, evt.Y);
			}
	} //GEN-LAST:event_disasmListMouseClicked

	private void AddBreakpointActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_AddBreakpointActionPerformed
			string value = (string) disasmListGetSelectedValue();
			if (!string.ReferenceEquals(value, null))
			{
				try
				{
					string address = value.Substring(3, 8);
					int addr = Utilities.parseAddress(address);
					if (!breakpoints.Contains(addr))
					{
						breakpoints.Add(addr);
					}
					RefreshDebuggerDisassembly(false);
				}
				catch (System.FormatException)
				{
					// Ignore it, probably already a breakpoint there
				}
			}
			else
			{
				JpcspDialogManager.showInformation(this, "Breakpoint Help : " + "Select the line to add a breakpoint to.");
			}
	} //GEN-LAST:event_AddBreakpointActionPerformed

	private void DeleteAllBreakpointsActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_DeleteAllBreakpointsActionPerformed
			DeleteAllBreakpoints();
	} //GEN-LAST:event_DeleteAllBreakpointsActionPerformed

		public virtual void DeleteAllBreakpoints()
		{
			if (breakpoints.Count > 0)
			{
				breakpoints.Clear();
				RefreshDebuggerDisassembly(false);
			}
		}

	private void DeleteBreakpointActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_DeleteBreakpointActionPerformed
			string value = (string) disasmListGetSelectedValue();
			if (!string.ReferenceEquals(value, null))
			{
				bool breakpointexists = value.StartsWith("<*>", StringComparison.Ordinal);
				if (breakpointexists)
				{
					string address = value.Substring(3, 8);
					int addr = Utilities.parseAddress(address);
					int b = breakpoints.IndexOf(addr);
					breakpoints.RemoveAt(b);
					RefreshDebuggerDisassembly(false);
				}
			}
			else
			{
				JpcspDialogManager.showInformation(this, "Breakpoint Help : " + "Select the line to remove a breakpoint from.");
			}
	} //GEN-LAST:event_DeleteBreakpointActionPerformed

		private void removeTemporaryBreakpoints()
		{
			if (temporaryBreakpoint1 != 0)
			{
				breakpoints.RemoveAt(new int?(temporaryBreakpoint1));
				temporaryBreakpoint1 = 0;
			}
			if (temporaryBreakpoint2 != 0)
			{
				breakpoints.RemoveAt(new int?(temporaryBreakpoint2));
				temporaryBreakpoint2 = 0;
			}
		}

		private void addTemporaryBreakpoints()
		{
			if (temporaryBreakpoint1 != 0)
			{
				breakpoints.Add(new int?(temporaryBreakpoint1));
			}
			if (temporaryBreakpoint2 != 0)
			{
				breakpoints.Add(new int?(temporaryBreakpoint2));
			}
		}

		private bool TemporaryBreakpoints
		{
			set
			{
				removeTemporaryBreakpoints();
    
				int pc = Emulator.Processor.cpu.pc;
				int opcode = Emulator.Memory.read32(pc);
				Instruction insn = Decoder.instruction(opcode);
				if (insn != null)
				{
					int branchingTo = 0;
					bool isBranching = false;
					int npc = pc + 4;
					if (value && insn.hasFlags(Instruction.FLAG_STARTS_NEW_BLOCK))
					{
						// Stepping over new blocks
					}
					else if (insn.hasFlags(Instruction.FLAG_IS_JUMPING))
					{
						branchingTo = Compiler.jumpTarget(npc, opcode);
						isBranching = true;
					}
					else if (insn.hasFlags(Instruction.FLAG_IS_BRANCHING))
					{
						branchingTo = Compiler.branchTarget(npc, opcode);
						isBranching = true;
					}
					else if (insn == Instructions.JR)
					{
						int rs = (opcode >> 21) & 31;
						branchingTo = Emulator.Processor.cpu.getRegister(rs);
						isBranching = true;
						// End of stepOut when reaching "jr $ra"
						if (stepOut && rs == _ra)
						{
							stepOut = false;
						}
					}
					else if (insn == Instructions.JALR && !value)
					{
						int rs = (opcode >> 21) & 31;
						branchingTo = Emulator.Processor.cpu.getRegister(rs);
						isBranching = true;
					}
    
					if (!isBranching)
					{
						temporaryBreakpoint1 = npc;
					}
					else if (branchingTo != 0)
					{
						temporaryBreakpoint1 = branchingTo;
						if (insn.hasFlags(Instruction.FLAG_IS_CONDITIONAL))
						{
							temporaryBreakpoint2 = npc;
							if (insn.hasFlags(Instruction.FLAG_HAS_DELAY_SLOT))
							{
								// Also skip the delay slot instruction
								temporaryBreakpoint2 += 4;
							}
						}
					}
				}
    
				addTemporaryBreakpoints();
				emu.RunEmu();
			}
		}

	private void StepIntoActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_StepIntoActionPerformed
			TemporaryBreakpoints = false;
	} //GEN-LAST:event_StepIntoActionPerformed

	private void StepOverActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_StepOverActionPerformed
			stepOut = false;
			TemporaryBreakpoints = true;
	} //GEN-LAST:event_StepOverActionPerformed

	private void StepOutActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_StepOutActionPerformed
			stepOut = true;
			TemporaryBreakpoints = true;
	} //GEN-LAST:event_StepOutActionPerformed

	private void RunDebuggerActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_RunDebuggerActionPerformed
			stepOut = false;
			removeTemporaryBreakpoints();
			emu.RunEmu();
	} //GEN-LAST:event_RunDebuggerActionPerformed

	// Called from Emulator
		public virtual void step()
		{
			// Fast check (most common case): nothing to do if there are no breakpoints at all.
			if (breakpoints.Count == 0)
			{
				return;
			}

			// Check if we have reached a breakpoint
			if (breakpoints.Contains(Emulator.Processor.cpu.pc))
			{
				if (stepOut)
				{
					// When stepping out, step over all instructions
					// until we reach "jr $ra".
					TemporaryBreakpoints = true;
				}
				else
				{
					removeTemporaryBreakpoints();

					Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_BREAKPOINT);
				}
			}
		}

	private void PauseDebuggerActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_PauseDebuggerActionPerformed
			Emulator.PauseEmuWithStatus(Emulator.EMU_STATUS_PAUSE);
	} //GEN-LAST:event_PauseDebuggerActionPerformed

		public void RefreshButtons()
		{
			// Called from Emulator
			RunDebugger.Selected = Emulator.run_Renamed && !Emulator.pause;
			PauseDebugger.Selected = Emulator.run_Renamed && Emulator.pause;

			btnCapture.Selected = State.captureGeNextFrame;
			btnReplay.Selected = State.replayGeNextFrame;

			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp");
			if (Emulator.run_Renamed && !Emulator.pause)
			{
				statusLabel.Text = bundle.getString("DisassemblerFrame.strEmuRunning.text");
			}
			else if (Emulator.run_Renamed && Emulator.pause)
			{
				statusLabel.Text = bundle.getString("DisassemblerFrame.strEmuHalted.text") + " uniquetempvar.";
			}
			else
			{
				statusLabel.Text = bundle.getString("DisassemblerFrame.strEmuNotRunning.text");
			}
		}

		private bool isCellChecked(JTable table)
		{
			for (int i = 0; i < table.RowCount; i++)
			{
				if (table.isCellSelected(i, 1))
				{
					return true;
				}

			}
			return false;
		}

	private void CopyValueActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_CopyValueActionPerformed
			if (cop1Table.Showing)
			{
				float value = (float?) cop1Table.getValueAt(cop1Table.SelectedRow, 1).Value;
				StringSelection stringSelection = new StringSelection(Convert.ToString(value));
				Clipboard clipboard = Toolkit.DefaultToolkit.SystemClipboard;
				clipboard.setContents(stringSelection, this);
			}
	} //GEN-LAST:event_CopyValueActionPerformed

		public virtual int GetGPI()
		{
			return gpi;
		}

		public virtual void SetGPO(int gpo)
		{
			this.gpo = gpo;
			// TODO if we want to use a visibility check here, then we need to refresh
			// gpo onFocus too otherwise it will be stale.
			//if (jPanel1.isVisible()) {
			// Refresh GPO
			for (int i = 0; i < 8; i++)
			{
				SetGPO(i, (gpo & (1 << i)) != 0);
			}
			//}
		}

		private void ToggleGPI(int index)
		{
			gpi ^= 1 << index;

			// Refresh GPI buttons
			for (int i = 0; i < 8; i++)
			{
				SetGPI(i, (gpi & (1 << i)) != 0);
			}
		}

		private void SetGPO(int index, bool on)
		{
			switch (index)
			{
				case 0:
					gpoLabel1.Enabled = on;
					break;
				case 1:
					gpoLabel2.Enabled = on;
					break;
				case 2:
					gpoLabel3.Enabled = on;
					break;
				case 3:
					gpoLabel4.Enabled = on;
					break;
				case 4:
					gpoLabel5.Enabled = on;
					break;
				case 5:
					gpoLabel6.Enabled = on;
					break;
				case 6:
					gpoLabel7.Enabled = on;
					break;
				case 7:
					gpoLabel8.Enabled = on;
					break;
			}
		}

		private void SetGPI(int index, bool on)
		{
			switch (index)
			{
				case 0:
					gpiButton1.Selected = on;
					break;
				case 1:
					gpiButton2.Selected = on;
					break;
				case 2:
					gpiButton3.Selected = on;
					break;
				case 3:
					gpiButton4.Selected = on;
					break;
				case 4:
					gpiButton5.Selected = on;
					break;
				case 5:
					gpiButton6.Selected = on;
					break;
				case 6:
					gpiButton7.Selected = on;
					break;
				case 7:
					gpiButton8.Selected = on;
					break;
			}
		}

	private void SetPCToCursorActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_SetPCToCursorActionPerformed
			int index = disasmListGetSelectedIndex();
			if (index != -1)
			{
				Emulator.Processor.cpu.pc = DebuggerPC + index * 4;
				RefreshDebuggerDisassembly(true);
			}
			else
			{
				Console.WriteLine("dpc: " + DebuggerPC.ToString("x"));
				Console.WriteLine("idx: " + index.ToString("x"));
				Console.WriteLine("npc: " + (DebuggerPC + index * 4).ToString("x"));
			}
	} //GEN-LAST:event_SetPCToCursorActionPerformed

	private void ExportBreaksActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ExportBreaksActionPerformed
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp");
			JFileChooser fc = new JFileChooser();
			fc.SelectedFile = new File(State.discId + ".brk");
			fc.DialogTitle = bundle.getString("DisassemblerFrame.miExportBreakpoints.text");
			fc.CurrentDirectory = new File(".");
			fc.addChoosableFileFilter(Constants.fltBreakpointFiles);
			fc.FileFilter = Constants.fltBreakpointFiles;

			int returnVal = fc.showSaveDialog(this);
			if (returnVal != JFileChooser.APPROVE_OPTION)
			{
				return;
			}

			File f = fc.SelectedFile;
			System.IO.StreamWriter @out = null;
			try
			{
				if (f.exists())
				{
					int res = MessageBox.Show(this, bundle.getString("ConsoleWindow.strFileExists.text"), bundle.getString("DisassemblerFrame.miExportBreakpoints.text"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

					if (res != DialogResult.Yes)
					{
						return;
					}
				}

				@out = new System.IO.StreamWriter(f);

				for (int i = 0; i < breakpoints.Count; i++)
				{
					@out.BaseStream.WriteByte(breakpoints[i].ToString("x") + System.getProperty("line.separator"));
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.Write(ex.StackTrace);
			}
			finally
			{
				Utilities.close(@out);
			}
	} //GEN-LAST:event_ExportBreaksActionPerformed

	private void ImportBreaksActionPerformed(ActionEvent evt)
	{ //GEN-FIRST:event_ImportBreaksActionPerformed
			java.util.ResourceBundle bundle = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp");
			JFileChooser fc = new JFileChooser();
			fc.DialogTitle = bundle.getString("DisassemblerFrame.miImportBreakpoints.text");
			fc.SelectedFile = new File(State.discId + ".brk");
			fc.CurrentDirectory = new File(".");
			fc.addChoosableFileFilter(Constants.fltBreakpointFiles);
			fc.FileFilter = Constants.fltBreakpointFiles;

			int returnVal = fc.showOpenDialog(this);
			if (returnVal != JFileChooser.APPROVE_OPTION)
			{
				return;
			}

			File f = fc.SelectedFile;
			System.IO.StreamReader @in = null;
			try
			{
				// TODO check content instead of ending
				if (!f.Name.contains(".brk"))
				{
					MessageBox.Show(this, bundle.getString("DisassemblerFrame.strInvalidBRKFile.text"), bundle.getString("DisassemblerFrame.miImportBreakpoints.text"), MessageBoxIcon.Error);

					return;
				}

				@in = new System.IO.StreamReader(f);
				string nextBrk = @in.ReadLine();

				while (!string.ReferenceEquals(nextBrk, null))
				{
					breakpoints.Add(Convert.ToInt32(nextBrk, 16));
					nextBrk = @in.ReadLine();
				}

				RefreshDebuggerDisassembly(false);

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.Write(ex.StackTrace);
			}
			finally
			{
				Utilities.close(@in);
			}
	} //GEN-LAST:event_ImportBreaksActionPerformed

		private void ManageMemBreaksActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_ManageMemBreaksActionPerformed
			if (mbpDialog == null)
			{
				mbpDialog = new MemoryBreakpointsDialog(this);
			}
			mbpDialog.Visible = true;
		} //GEN-LAST:event_ManageMemBreaksActionPerformed

		private void CloseActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_CloseActionPerformed
			Visible = false;
		} //GEN-LAST:event_CloseActionPerformed

		private class SearchTask : javax.swing.SwingWorker
		{
			private readonly DisassemblerFrame outerInstance;


			internal string search = "";
			internal int position;

			public SearchTask(DisassemblerFrame outerInstance, string search, int startAt)
			{
				this.outerInstance = outerInstance;
				this.search = search;
				this.position = startAt;
			}

			protected internal override void done()
			{
				try
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Nullable<int> address = (System.Nullable<int>) get();
					int? address = (int?) get();
					if (address != null)
					{
						SwingUtilities.invokeLater(() =>
						{
						// jump to the finding and select it
						outerInstance.DebuggerPC = address.Value;
						outerInstance.SelectedPC = address.Value;
						outerInstance.RefreshDebuggerDisassembly(false);
						});
					}
				}
				catch (CancellationException)
				{
					// do nothing
				}
				catch (InterruptedException)
				{
					// do nothing
				}
				catch (ExecutionException)
				{
					// do nothing
				}

				outerInstance.prgBarSearch.Indeterminate = false;
				outerInstance.txtSearch.Enabled = true;
				outerInstance.btnCancelSearch.Enabled = false;

				// if the search entry is visible change the focus back to allow
				// continous search
				if (outerInstance.txtSearch.Visible)
				{
					outerInstance.txtSearch.requestFocus();
				}
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override protected Object doInBackground() throws Exception
			protected internal override object doInBackground()
			{
				if (search.Length == 0)
				{
					return null;
				}

				while (Memory.isAddressGood(position))
				{
					int opcode = Memory.Instance.read32(position);
					Instruction insn = Decoder.instruction(opcode);

					// just use the text portion here
					if (insn.disasm(position, opcode).Contains(search))
					{
						return new int?(position);
					}
					position += 4;

					// check if the user requested a cancellation
					if (Cancelled)
					{
						break;
					}
				}
				return null;
			}
		}

		private void txtSearchActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_txtSearchActionPerformed
			if (txtSearch.Text.Empty)
			{
				return;
			}

			// we do not know when the string will be found
			prgBarSearch.Indeterminate = true;
			txtSearch.Enabled = false;
			btnCancelSearch.Enabled = true;

			// add 4 to the selected address to avoid stopping on the current entry
			searchTask = new SearchTask(this, txtSearch.Text, SelectedPC + 4);
			searchTask.execute();
		} //GEN-LAST:event_txtSearchActionPerformed

		private void btnDumpDebugStateActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_btnDumpDebugStateActionPerformed
			DumpDebugState.dumpDebugState();
		} //GEN-LAST:event_btnDumpDebugStateActionPerformed

		private void gpiButton8ActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_gpiButton8ActionPerformed
			ToggleGPI(7);
		} //GEN-LAST:event_gpiButton8ActionPerformed

		private void gpiButton7ActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_gpiButton7ActionPerformed
			ToggleGPI(6);
		} //GEN-LAST:event_gpiButton7ActionPerformed

		private void gpiButton6ActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_gpiButton6ActionPerformed
			ToggleGPI(5);
		} //GEN-LAST:event_gpiButton6ActionPerformed

		private void gpiButton5ActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_gpiButton5ActionPerformed
			ToggleGPI(4);
		} //GEN-LAST:event_gpiButton5ActionPerformed

		private void gpiButton4ActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_gpiButton4ActionPerformed
			ToggleGPI(3);
		} //GEN-LAST:event_gpiButton4ActionPerformed

		private void gpiButton3ActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_gpiButton3ActionPerformed
			ToggleGPI(2);
		} //GEN-LAST:event_gpiButton3ActionPerformed

		private void gpiButton2ActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_gpiButton2ActionPerformed
			ToggleGPI(1);
		} //GEN-LAST:event_gpiButton2ActionPerformed

		private void gpiButton1ActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_gpiButton1ActionPerformed
			ToggleGPI(0);
		} //GEN-LAST:event_gpiButton1ActionPerformed

		private void cop1TableMouseClicked(java.awt.@event.MouseEvent evt)
		{ //GEN-FIRST:event_cop1TableMouseClicked
			if (SwingUtilities.isRightMouseButton(evt) && cop1Table.isColumnSelected(1) && isCellChecked(cop1Table))
			{
				RegMenu.show(cop1Table, evt.X, evt.Y);
			}
		} //GEN-LAST:event_cop1TableMouseClicked

		private void disasmListComponentResized(java.awt.@event.ComponentEvent evt)
		{ //GEN-FIRST:event_disasmListComponentResized
			RefreshDebuggerDisassembly(false);
		} //GEN-LAST:event_disasmListComponentResized

		private void btnCaptureActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_btnCaptureActionPerformed
			State.captureGeNextFrame = btnCapture.Selected;
		} //GEN-LAST:event_btnCaptureActionPerformed

		private void btnReplayActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_btnReplayActionPerformed
			State.replayGeNextFrame = btnReplay.Selected;
		} //GEN-LAST:event_btnReplayActionPerformed

		private void btnCancelSearchActionPerformed(ActionEvent evt)
		{ //GEN-FIRST:event_btnCancelSearchActionPerformed
			// request cancellation of the search thread
			searchTask.cancel(false);
		} //GEN-LAST:event_btnCancelSearchActionPerformed

		private void txtSearchFocusGained(java.awt.@event.FocusEvent evt)
		{ //GEN-FIRST:event_txtSearchFocusGained
			txtSearch.selectAll();
		} //GEN-LAST:event_txtSearchFocusGained

		public override void dispose()
		{
			if (mbpDialog != null)
			{
				mbpDialog.dispose();
			}
			Emulator.MainGUI.endWindowDialog();
			base.dispose();
		}
		// Variables declaration - do not modify//GEN-BEGIN:variables
		private JButton AddBreakpoint;
		private javax.swing.JMenuItem BranchOrJump;
		private javax.swing.JMenuItem CopyAddress;
		private javax.swing.JMenuItem CopyAll;
		private javax.swing.JMenuItem CopyValue;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		private JButton DeleteAllBreakpoints_Renamed;
		private JButton DeleteBreakpoint;
		private javax.swing.JPopupMenu DisMenu;
		private JButton DumpCodeToText;
		private JButton ExportBreaks;
		private JButton ImportBreaks;
		private JButton JumpToAddress;
		private JButton ManageMemBreaks;
		private javax.swing.JToggleButton PauseDebugger;
		private javax.swing.JPopupMenu RegMenu;
		private JButton ResetToPCbutton;
		private javax.swing.JToggleButton RunDebugger;
		private javax.swing.JMenuItem SetPCToCursor;
		private JButton btnCancelSearch;
		private javax.swing.JToggleButton btnCapture;
		private JButton btnDumpDebugState;
		private javax.swing.JToggleButton btnReplay;
		private JButton btnStepInto;
		private JButton btnStepOut;
		private JButton btnStepOver;
		private JTable cop0Table;
		private JTable cop1Table;
		private JList disasmList;
		private javax.swing.JTabbedPane disasmTabs;
		private javax.swing.JToggleButton gpiButton1;
		private javax.swing.JToggleButton gpiButton2;
		private javax.swing.JToggleButton gpiButton3;
		private javax.swing.JToggleButton gpiButton4;
		private javax.swing.JToggleButton gpiButton5;
		private javax.swing.JToggleButton gpiButton6;
		private javax.swing.JToggleButton gpiButton7;
		private javax.swing.JToggleButton gpiButton8;
		private javax.swing.JLabel gpioLabel;
		private javax.swing.JLabel gpoLabel1;
		private javax.swing.JLabel gpoLabel2;
		private javax.swing.JLabel gpoLabel3;
		private javax.swing.JLabel gpoLabel4;
		private javax.swing.JLabel gpoLabel5;
		private javax.swing.JLabel gpoLabel6;
		private javax.swing.JLabel gpoLabel7;
		private javax.swing.JLabel gpoLabel8;
		private pspsharp.Debugger.DisassemblerModule.RegisterTable gprTable;
		private javax.swing.JToolBar.Separator jSeparator1;
		private javax.swing.JPopupMenu.Separator jSeparator10;
		private javax.swing.JPopupMenu.Separator jSeparator11;
		private javax.swing.JToolBar.Separator jSeparator2;
		private javax.swing.JToolBar.Separator jSeparator3;
		private javax.swing.JToolBar.Separator jSeparator4;
		private javax.swing.JToolBar.Separator jSeparator7;
		private javax.swing.JPopupMenu.Separator jSeparator9;
		private javax.swing.JLabel lblCaptureReplay;
		private javax.swing.JLabel lblDumpState;
		private javax.swing.JLabel lblSearch;
		private javax.swing.JMenu mBreakpoints;
		private javax.swing.JMenu mDebug;
		private javax.swing.JMenu mDisassembler;
		private javax.swing.JMenu mFile;
		private javax.swing.JMenuBar mbMain;
		private javax.swing.JMenuItem miClose;
		private javax.swing.JMenuItem miDeleteAllBreakpoints;
		private javax.swing.JMenuItem miDeleteBreakpoint;
		private javax.swing.JMenuItem miDumpCode;
		private javax.swing.JMenuItem miExportBreakpoints;
		private javax.swing.JMenuItem miImportBreakpoints;
		private javax.swing.JMenuItem miJumpTo;
		private javax.swing.JMenuItem miManageMemoryBreakpoints;
		private javax.swing.JMenuItem miNewBreakpoint;
		private javax.swing.JMenuItem miPause;
		private javax.swing.JMenuItem miResetToPC;
		private javax.swing.JMenuItem miRun;
		private javax.swing.JMenuItem miStepInto;
		private javax.swing.JMenuItem miStepOut;
		private javax.swing.JMenuItem miStepOver;
		private javax.swing.JPanel miscPanel;
		private javax.swing.JProgressBar prgBarSearch;
		private javax.swing.JLabel statusLabel;
		private javax.swing.JPanel statusPanel;
		private javax.swing.JToolBar tbBreakpoints;
		private javax.swing.JToolBar tbDisasm;
		private javax.swing.JTextField txtSearch;
		// End of variables declaration//GEN-END:variables

		private class ClickAction : AbstractAction
		{

			internal const long serialVersionUID = -6595335927462915819L;
			internal JButton button;

			public ClickAction(JButton button)
			{
				this.button = button;
			}

			public override void actionPerformed(ActionEvent e)
			{
				button.doClick();
			}
		}
	}

}