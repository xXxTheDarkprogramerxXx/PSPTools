using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSP_PBP_Tools
{
    public partial class UMDCreator : Form
    {
        public UMDCreator()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string folderPath = "";
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                folderPath = folderBrowserDialog1.SelectedPath;

                //


                textBox1.Text = folderPath;
                //PSP_Tools.UMD.ISO.PSPTitle = "Medievil";



            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string LocDatapsar = @"C:\Users\3deEchelon\Desktop\PSP\PBP Creation Test\DATA.PSAR";
            string locdatapsp = @"C:\Users\3deEchelon\Desktop\PSP\PBP Creation Test\DATA.PSP";
            string locicon0 = @"C:\Users\3deEchelon\Desktop\PSP\PBP Creation Test\ICON0.PNG";
            string locicon1pmf = @"C:\Users\3deEchelon\Desktop\PSP\PBP Creation Test\ICON1.PMF";
            string locparam = @"C:\Users\3deEchelon\Desktop\PSP\PBP Creation Test\PARAM.SFO";
            string locpic0 =@"C:\Users\3deEchelon\Desktop\PSP\PBP Creation Test\PIC0.PNG";
            string locpic1 = @"C:\Users\3deEchelon\Desktop\PSP\PBP Creation Test\PIC1.PNG";
            string locsnd0 = @"C:\Users\3deEchelon\Desktop\PSP\PBP Creation Test\SND0.AT3";

            //PSP_Tools.UMD.Sign.PSN.Create_DATA_PSARC(new FileInfo(locparam).Length, new FileInfo(locicon0).Length, new FileInfo(locicon1pmf).Length, new FileInfo(locpic0).Length, new FileInfo(locpic1).Length, new FileInfo(locsnd0).Length, new FileInfo(locdatapsp).Length);


            PSP_Tools.UMD.Sign.PSN psn = new PSP_Tools.UMD.Sign.PSN(locparam, locicon0, locicon1pmf, locpic0, locpic1, locsnd0, locdatapsp, LocDatapsar);
            psn.Create_PSP_Signed(@"C:\Users\3deEchelon\Desktop\PSP\psy-mhf.iso", @"C:\Users\3deEchelon\Desktop\PSP\PBP Creation Test\test.pbp", "HW1633-ULES00318_00-HOMEBREWSSSSSSSS");
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.InitialDirectory = Application.StartupPath;

            saveFileDialog1.Title = "Save ISO File";

            saveFileDialog1.CheckFileExists = false;

            saveFileDialog1.CheckPathExists = true;

            saveFileDialog1.DefaultExt = "iso";

            saveFileDialog1.Filter = "ISO Files (*.ISO)|*.ISO|All files (*.*)|*.*";

            saveFileDialog1.FilterIndex = 2;

            saveFileDialog1.RestoreDirectory = true;



            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                PSP_Tools.UMD.ISO isotol = new PSP_Tools.UMD.ISO();
                isotol.PSPTitle = string.IsNullOrEmpty(txtTitle.Text) ? "MedievilPSX" : txtTitle.Text;//set a spesific title fod the iso else it will be fetched from the PARAM.SFO
                isotol.CreateISO(textBox1.Text, saveFileDialog1.FileName);
                while (isotol.Status == PSP_Tools.UMD.ISO.ISOStatus.Busy)
                {
                    //sleep the thread
                    System.Threading.Thread.Sleep(100);
                }
                if (isotol.Status == PSP_Tools.UMD.ISO.ISOStatus.Completed)
                {
                    MessageBox.Show("Iso Completed");
                }
            }
        }

        private void UMDCreator_Load(object sender, EventArgs e)
        {
            try
            {
                PSP_Tools.UMD.ISO umd = new PSP_Tools.UMD.ISO();
                var read = umd.ReadISO(@"C:\Users\3de Echelon\AppData\Local\Temp\PSP-FPKG\psphd\data\USER_L0.IMG");

                PSP_Tools.UMD.ISO isotol = new PSP_Tools.UMD.ISO();
                isotol.PSPTitle = read;//set a spesific title fod the iso else it will be fetched from the PARAM.SFO
                isotol.CreateISO(@"C:\Users\3de Echelon\AppData\Roaming\Ps4Tools\USER_L0", @"D:\Users\3deEchelon\Documents\Visual Studio 2015\Projects\PSP_PBP_Tools\PSPTools\PSP_PBP_Tools\bin\Debug\TEST.ISO");
            }
            catch(Exception ex)
            {

            }
        }
    }
}
