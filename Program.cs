using System;
using System.Windows.Forms;

namespace SafariGUIFinal
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Enable visual styles for modern look
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Run the main form of the Safari simulation
            Application.Run(new MainForm());
        }
    }
}