using System;//
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PSP_PBP_Tools
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ////to go to PBP Tools
            //Application.Run(new Form1());

            ////to go to GIM tools
            //Application.Run(new GIM());

            ////to go to UMD tools
            //Application.Run(new UMDSign());

            ////to go to UMD CSO ISO Tool
            Application.Run(new CSO_ISO_Reader());

            //to go to ISO UMD Tool
            //Application.Run(new UMDCreator());
        }
    }
}
