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
    public partial class Eboot_ELF : Form
    {
        public Eboot_ELF()
        {
            InitializeComponent();
        }

        private void Eboot_ELF_Load(object sender, EventArgs e)
        {
            try
            {
                PSP_Tools.Crypto.EncryptedPrx encrypted = new PSP_Tools.Crypto.EncryptedPrx();
                var decryptedboot = encrypted.Decrypt(System.IO.File.ReadAllBytes(@"C:\Users\3de Echelon\AppData\Local\Temp\PSP-FPKG\temp_eboot.bin"), true);

                System.IO.File.WriteAllBytes(@"C:\Users\3de Echelon\AppData\Local\Temp\PSP-FPKG\temp_eboot.bin.xdpx", decryptedboot);
            }
            catch(Exception ex)
            {

            }


        }
    }
}
