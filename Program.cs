using System;
using System.Text;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using SlimDX.Direct3D9;
using SlimDX;

namespace CCleanerBiosUpdater
{

    public static class StartProgram
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
