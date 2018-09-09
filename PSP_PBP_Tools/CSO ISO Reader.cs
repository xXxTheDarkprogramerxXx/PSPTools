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

                PSP_Tools.UMD.CISO.GlobalMembers.Decompress_CISO(thedialog.FileName.Replace(".cso", ".iso"), thedialog.FileName);
                

            }
        }
    }
}
