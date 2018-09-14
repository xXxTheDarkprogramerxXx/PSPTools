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

	using CpuState = pspsharp.Allegrex.CpuState;

	public class VfpuFrame : JFrame
	{

		private const long serialVersionUID = -3354614570041807689L;
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: internal JTextField[][][] registers = new JTextField[8][4][4];
		internal JTextField[][][] registers = RectangularArrays.ReturnRectangularJTextFieldArray(8, 4, 4);
		internal JPanel[] panels = new JPanel[8];
		private static VfpuFrame instance;

		public static VfpuFrame Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new VfpuFrame();
				}
				return instance;
			}
		}

		private VfpuFrame()
		{
			DefaultCloseOperation = DISPOSE_ON_CLOSE;
			Title = java.util.ResourceBundle.getBundle("pspsharp/languages/pspsharp").getString("VfpuFrame.title"); // NOI18N

			for (int i = 0; i < 8; ++i)
			{
				for (int j = 0; j < 4; ++j)
				{
					for (int k = 0; k < 4; ++k)
					{
						registers[i][j][k] = new JTextField();
					}
				}
			}

			GroupLayout layout = new GroupLayout(ContentPane);
			ContentPane.Layout = layout;

			for (int i = 0; i < panels.Length; ++i)
			{
				JPanel panel = new JPanel();
				panels[i] = panel;

				GroupLayout l = new GroupLayout(panel);
				panel.Layout = l;
				panel.Border = BorderFactory.createCompoundBorder(BorderFactory.createTitledBorder("Block " + i), null);

				l.HorizontalGroup = l.createSequentialGroup().addGroup(l.createParallelGroup().addComponent(registers[i][0][0]).addComponent(registers[i][1][0]).addComponent(registers[i][2][0]).addComponent(registers[i][3][0])).addGroup(l.createParallelGroup().addComponent(registers[i][0][1]).addComponent(registers[i][1][1]).addComponent(registers[i][2][1]).addComponent(registers[i][3][1])).addGroup(l.createParallelGroup().addComponent(registers[i][0][2]).addComponent(registers[i][1][2]).addComponent(registers[i][2][2]).addComponent(registers[i][3][2])).addGroup(l.createParallelGroup().addComponent(registers[i][0][3]).addComponent(registers[i][1][3]).addComponent(registers[i][2][3]).addComponent(registers[i][3][3]));
				l.VerticalGroup = l.createSequentialGroup().addGroup(l.createParallelGroup().addComponent(registers[i][0][0]).addComponent(registers[i][0][1]).addComponent(registers[i][0][2]).addComponent(registers[i][0][3])).addGroup(l.createParallelGroup().addComponent(registers[i][1][0]).addComponent(registers[i][1][1]).addComponent(registers[i][1][2]).addComponent(registers[i][1][3])).addGroup(l.createParallelGroup().addComponent(registers[i][2][0]).addComponent(registers[i][2][1]).addComponent(registers[i][2][2]).addComponent(registers[i][2][3])).addGroup(l.createParallelGroup().addComponent(registers[i][3][0]).addComponent(registers[i][3][1]).addComponent(registers[i][3][2]).addComponent(registers[i][3][3]));
			}

			layout.HorizontalGroup = layout.createParallelGroup().addGroup(layout.createSequentialGroup().addComponent(panels[0]).addComponent(panels[1])).addGroup(layout.createSequentialGroup().addComponent(panels[2]).addComponent(panels[3])).addGroup(layout.createSequentialGroup().addComponent(panels[4]).addComponent(panels[5])).addGroup(layout.createSequentialGroup().addComponent(panels[6]).addComponent(panels[7]));
			layout.VerticalGroup = layout.createParallelGroup().addGroup(layout.createSequentialGroup().addComponent(panels[0]).addComponent(panels[2]).addComponent(panels[4]).addComponent(panels[6])).addGroup(layout.createSequentialGroup().addComponent(panels[1]).addComponent(panels[3]).addComponent(panels[5]).addComponent(panels[7]));

			MinimumSize = new Dimension(450, 450);
			WindowPropSaver.loadWindowProperties(this);
		}

		public virtual void updateRegisters(CpuState cpu)
		{
			for (int i = 0; i < 8; ++i)
			{
				for (int j = 0; j < 4; ++j)
				{
					for (int k = 0; k < 4; ++k)
					{
						registers[i][k][j].Text = (new float?(cpu.getVprFloat(i, j, k))).ToString();
						registers[i][k][j].CaretPosition = 0;
					}
				}
			}
		}

		public override void dispose()
		{
			Emulator.MainGUI.endWindowDialog();
			base.dispose();
		}
	}

}