using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Music_Visual
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            for (int i = 0; i < e.Args.Length; i++)
            {
                if (e.Args[i] == "-parentHWND")
                {
                    App.HWND = long.Parse(e.Args[i + 1]);
                }
                if (e.Args[i] == "-wpe_h")
                {
                    App.BGH = Convert.ToInt32(e.Args[i + 1]);
                }if (e.Args[i] == "-wpe_w")
                {
                    App.BGW = Convert.ToInt32(e.Args[i + 1]);
                }
            }
        }

        public static long HWND;
        public static int BGW;
        public static int BGH;
    }
}
