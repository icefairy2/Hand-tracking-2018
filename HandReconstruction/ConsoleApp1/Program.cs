using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace sample3dscan.cs
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Don't allow multiple instances to run simultaneously.
            using (Mutex mutex = new Mutex(false, "Global\\" + "8FD0E7DE-9937-48D8-A723-2834EFD3BAF3"))
            {
                if(!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("An instance of " +
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().Name 
                        + " is already running");
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                PXCMSession session = PXCMSession.CreateInstance();
                if (session != null)
                {
                    Application.Run(new MainForm(session));
                    session.Dispose();
                }
            }
        }
    }
}

