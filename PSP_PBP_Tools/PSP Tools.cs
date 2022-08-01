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
    public partial class PSP_Tools_Form : Form
    {
        public PSP_Tools_Form()
        {
            InitializeComponent();
        }

        private void btnLoadISO_Click(object sender, EventArgs e)
        {
            OpenFileDialog thedialog = new OpenFileDialog();
            thedialog.Title = "Select ISO";
            thedialog.Filter = "PSP ISO File (*.ISO)|*.ISO";
            //"Plain text files (*.csv;*.txt)|*.csv;*.txt";
            thedialog.Multiselect = true;
            thedialog.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
            if (thedialog.ShowDialog() == DialogResult.OK)
            {
                //load the iso
                txtISOLock.Text = thedialog.FileName;

                PSP_Tools.UMD.ISO iso = new PSP_Tools.UMD.ISO();
                string temp = iso.ReadISO(thedialog.FileName);
            }
        }
    }
}
