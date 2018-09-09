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
    public partial class GIM : Form
    {
        public GIM()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Open Gim
            OpenFileDialog thedialog = new OpenFileDialog();
            thedialog.Title = "Select GIM";
            thedialog.Filter = "PSP GIM File (*.gim)|*.gim";
            //"Plain text files (*.csv;*.txt)|*.csv;*.txt";
            thedialog.Multiselect = false;//psp emu only supports 1.8Gig so we might as well only allow one iso/pbp/cso file
            thedialog.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();

            if (thedialog.ShowDialog() == DialogResult.OK)
            {

                PSP_Tools.GIM gim = new PSP_Tools.GIM(thedialog.FileName);
                var converted = gim.ConvertToBitmaps();

                pictureBox1.Image = converted[0];
            }
        }
    }
}
