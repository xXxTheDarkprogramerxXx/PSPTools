using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSP_PBP_Tools
{
    public partial class CSO_ISO_Reader : Form
    {
        public CSO_ISO_Reader()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog thedialog = new OpenFileDialog();
            thedialog.Title = "Select UMD Dump";
            thedialog.Filter = "PSP UMD File (*.ISO,*CSO)|*.ISO;*.CSO";
            //"Plain text files (*.csv;*.txt)|*.csv;*.txt";
            thedialog.Multiselect = false;//psp emu only supports 1.8Gig so we might as well only allow one iso/pbp/cso file
            thedialog.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();

            if (thedialog.ShowDialog() == DialogResult.OK)
            {

                ////This is not currently working so we will use this untill its fixed or i have time to fix it
                //.UMD.CISO.GlobalMembers.Decompress_CISO(thedialog.FileName.Replace(".cso", "Test.iso"), thedialog.FileName);


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
                    ////temporary decompressor
                    PSP_Tools.UMD.CISO.DecompressCSO(thedialog.FileName,saveFileDialog1.FileName);

                    MessageBox.Show("Decryption Completed");
                }
            }
        }
    }
}
