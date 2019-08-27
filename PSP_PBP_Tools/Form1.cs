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
    public partial class Form1 : Form
    {

        PSP_Tools.Pbp pbp = new PSP_Tools.Pbp();
        System.Windows.Forms.FolderBrowserDialog saveFileDialog1 = new System.Windows.Forms.FolderBrowserDialog();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog thedialog = new OpenFileDialog();
            thedialog.Title = "Select PBP";
            thedialog.Filter = "PSP PBP File (*.PBP)|*.PBP";
            //"Plain text files (*.csv;*.txt)|*.csv;*.txt";
            thedialog.Multiselect = true;
            thedialog.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
            if (thedialog.ShowDialog() == DialogResult.OK)
            {

                try
                {
                    pbp = new PSP_Tools.Pbp();
                    pbp.LoadPbp(thedialog.FileName);

                    byte[] paramsfo = pbp.ReadFileFromPBP(PSP_Tools.Pbp.DataType.ParamSfo);
                    if (paramsfo.Length != 0)
                    {
                        PSP_Tools.PARAM_SFO sfo = new PSP_Tools.PARAM_SFO(paramsfo);

                        label1.Text = "Title : " + sfo.Title;

                        label2.Text = "Disc ID :" + sfo.DISC_ID;

                        label3.Text = "PSP System Version :" + sfo.PSP_SYSTEM_VER;
                        
                    }
                    string DiscID = pbp.GetPBPDiscID();

                    byte[] Icon0Png = pbp.ReadFileFromPBP(PSP_Tools.Pbp.DataType.Icon0Png);
                    if (Icon0Png.Length != 0)
                    {
                        pictureBox1.Image = ByteToImage(Icon0Png);
                    }

                    byte[] Pic1Png = pbp.ReadFileFromPBP(PSP_Tools.Pbp.DataType.Pic1Png);
                    if (Pic1Png.Length != 0)
                    {
                        pictureBox2.Image = ByteToImage(Pic1Png);
                    }
                    byte[] sound = pbp.ReadFileFromPBP(PSP_Tools.Pbp.DataType.Snd0At3);
                    //here you can play the sound somehow xD


                    //var temp 
                    //pbp.WritePDPFiles(System.AppDomain.CurrentDomain.BaseDirectory);//this is so cool
                    /*
                    Data.psar = boot.bin ;)
                    data.psp = eboot.bin*/



                    //lets extract the data while we are at it 
                    /*"psp.data",
                    "psar.data"*/



                    textBox1.Text = thedialog.FileName;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public static Bitmap ByteToImage(byte[] blob)
        {
            MemoryStream mStream = new MemoryStream();
            byte[] pData = blob;
            mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
            Bitmap bm = new Bitmap(mStream, false);
            mStream.Dispose();
            return bm;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //write all files to a location
            //saveFileDialog1.Filter = "PS4 PKG|*.pkg";
            //saveFileDialog1.Title = "Save an PS4 PKG File";
            //saveFileDialog1.ov
            if (System.Windows.Forms.DialogResult.OK != saveFileDialog1.ShowDialog())
            {
                return;
            }
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            pbp.WritePBPFiles(saveFileDialog1.SelectedPath);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            // Confirm user wants to close
            if (backgroundWorker1.IsBusy == true)
            {
                switch (MessageBox.Show(this, " Busy extracting PBP\n Are you sure you want to close?", "Closing", MessageBoxButtons.YesNo))
                {
                    case DialogResult.No:
                        e.Cancel = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            MessageBox.Show("Extraction Completed");
        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (System.Windows.Forms.DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                
                pbp.WritePBPFiles(saveFileDialog1.SelectedPath, pspdata: "EBOOT.BIN", psrdata: "DATA.BIN",make_eboot_boot:false);

                MessageBox.Show("Extraction Completed");
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
